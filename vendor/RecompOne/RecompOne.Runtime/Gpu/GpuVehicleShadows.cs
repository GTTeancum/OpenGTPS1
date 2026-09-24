using System.Buffers.Binary;
using RecompOne.Runtime.Memory;

namespace RecompOne.Runtime;

// Read-only source capture. The original guest still selects the LOD and draws
// every packet. Hidden faces are sent only to the native lighting caster stream.
public sealed partial class Gpu
{
    public void CaptureVehicleShadowSource(uint selectedModel, IMemory memory)
    {
        WorldObjectContext owner = WorldCaptureContext.Current;
        if (selectedModel == 0 || owner.Kind != WorldObjectKind.Vehicle ||
            owner.SceneGeneration == 0 || owner.ScenePass != WorldScenePass.Main ||
            !owner.DepthScaleValid || _liveWorldCapture == null || !_liveWorldCapture.Enabled)
            return;
        byte[]? bytes = VehicleShadowSource.Decode(selectedModel, owner,
            Host.InputManager.CurrentPoll, memory);
        if (bytes != null)
            _liveWorldCapture.RegisterResidentTrackMesh(bytes);
    }
}

internal static class VehicleShadowSource
{
    // OGTCAR01, separate from the resident course format; no ABI layout cast.
    internal const ulong Magic = 0x313052414354474FUL;
    static readonly uint[] Strides = [16,16,24,28,28,28,36,40];
    static bool Range(uint address, uint size) =>
        (address & 0xFFE00000u) is 0x80000000u or 0xA0000000u &&
        (ulong)(address & 0x1FFFFFu) + size <= 0x200000UL;

    internal static byte[]? Decode(uint model, WorldObjectContext owner,
        int poll, IMemory memory)
    {
        if (!Range(model, 0x50) || poll < 0) return null;
        uint vertices = memory.ReadU32(model + 0x14);
        uint count = memory.ReadU16(model);
        if (count < 3 || count > 256 || !Range(vertices, count * 8)) return null;
        int triangles = 0;
        for (uint stream = 0; stream < 8; stream++)
        {
            uint n = memory.ReadU16(model + 4 + stream * 2);
            uint pointer = memory.ReadU32(model + 0x1C + stream * 4);
            uint stride = Strides[stream];
            if (n > 4096 || (n != 0 && !Range(pointer, n * stride))) return null;
            triangles += checked((int)n * ((stream & 1) == 0 ? 1 : 2));
        }
        if (triangles == 0 || triangles > 8192) return null;
        byte[] output = new byte[48 + triangles * 32];
        Span<byte> dest = output;
        BinaryPrimitives.WriteUInt64LittleEndian(dest, Magic);
        BinaryPrimitives.WriteUInt32LittleEndian(dest[8..], 1);
        BinaryPrimitives.WriteUInt32LittleEndian(dest[12..], 48);
        BinaryPrimitives.WriteUInt32LittleEndian(dest[16..], owner.ModelPointer);
        BinaryPrimitives.WriteUInt32LittleEndian(dest[20..], model);
        BinaryPrimitives.WriteUInt32LittleEndian(dest[24..], owner.StableId);
        BinaryPrimitives.WriteInt32LittleEndian(dest[28..], poll);
        BinaryPrimitives.WriteUInt32LittleEndian(dest[32..], owner.SceneGeneration);
        BinaryPrimitives.WriteInt32LittleEndian(dest[36..], triangles);
        uint paletteBase = memory.ReadU32(0x1F80039C);
        uint paletteMask = memory.ReadU32(0x1F8003A0);
        int offset = 48;
        Span<byte> refs = stackalloc byte[4];
        Span<ushort> uvs = stackalloc ushort[4];
        for (uint stream = 0; stream < 8; stream++)
        {
            uint n = memory.ReadU16(model + 4 + stream * 2);
            uint pointer = memory.ReadU32(model + 0x1C + stream * 4);
            bool textured = stream >= 4, quad = (stream & 1) != 0;
            uint stride = Strides[stream];
            for (uint item = 0; item < n; item++)
            {
                uint record = pointer + item * stride;
                uint indices = memory.ReadU32(record);
                for (int corner = 0; corner < (quad ? 4 : 3); corner++)
                {
                    refs[corner] = (byte)(indices >> (corner * 8));
                    if (refs[corner] >= count) return null;
                }
                byte opcode = memory.ReadU8(record + 15);
                if ((opcode & 0xE0) != 0x20 || ((opcode & 4) != 0) != textured ||
                    ((opcode & 8) != 0) != quad || ((opcode & 16) != 0) != ((stream & 2) != 0))
                    return null;
                uint flags = (textured ? 1u : 0u) | ((opcode & 2) != 0 ? 2u : 0u);
                ushort page = 0, clut = 0;
                uvs.Clear();
                if (textured)
                {
                    uint uvOffset = stream < 6 ? 16u : (quad ? 28u : 24u);
                    uint uv0 = memory.ReadU32(record + uvOffset);
                    uint sourceFlags = memory.ReadU32(record + 4);
                    uint adjusted = unchecked(uv0 + (((uint)((int)sourceFlags >> 14)) & paletteMask) + paletteBase);
                    clut = (ushort)(adjusted >> 16);
                    uint uv1 = memory.ReadU32(record + uvOffset + 4);
                    uint uv23 = memory.ReadU32(record + uvOffset + 8);
                    page = (ushort)(uv1 >> 16);
                    uvs[0] = (ushort)uv0; uvs[1] = (ushort)uv1;
                    uvs[2] = (ushort)uv23; uvs[3] = (ushort)(uv23 >> 16);
                }
                for (int half = 0; half < (quad ? 2 : 1); half++)
                {
                    BinaryPrimitives.WriteUInt32LittleEndian(dest[offset..], flags);
                    BinaryPrimitives.WriteUInt16LittleEndian(dest[(offset + 4)..], page);
                    BinaryPrimitives.WriteUInt16LittleEndian(dest[(offset + 6)..], clut);
                    // GT2 emits source A,B,D,C into the PS1 strip packet.
                    // Its GPU then draws A/B/D and B/D/C, NOT A/B/C + B/C/D.
                    for (int corner = 0; corner < 3; corner++)
                    {
                        int packetCorner = corner + half;
                        int index = quad && packetCorner >= 2 ? 5 - packetCorner : packetCorner;
                        uint v = vertices + refs[index] * 8u;
                        int d = offset + 8 + corner * 8;
                        for (uint axis = 0; axis < 3; axis++)
                            BinaryPrimitives.WriteUInt16LittleEndian(dest[(d + (int)axis * 2)..], memory.ReadU16(v + axis * 2));
                        BinaryPrimitives.WriteUInt16LittleEndian(dest[(d + 6)..], uvs[index]);
                    }
                    offset += 32;
                }
            }
        }
        return output;
    }
}
