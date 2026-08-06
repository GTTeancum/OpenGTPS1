using RecompOne.Runtime.Context;
using RecompOne.Runtime.Memory;
using RecompOne.Runtime.Sdk;

namespace Recompiled.Arcade;

public static partial class GranTurismo2ArcadePC
{
    const uint LiveryPreviewScratch = 0x807EFEF0u;

    public static void ReloadLiveryPreview(
        CpuContext c,
        IMemory m,
        uint preview,
        uint frontendContext)
    {
        uint targetBody = m.ReadU32(preview + 0x8u);
        uint targetPalette = m.ReadU8(preview + 0xDu);
        ulong resolved = GT2Compat.ResolveLiveryBodyAndPalette(
            targetBody, targetPalette);
        uint body = (uint)resolved;
        uint palette = (uint)(resolved >> 32);

        c.A0 = preview;
        c.A1 = frontendContext;
        c.A2 = body;
        GranTurismo2ArcadePC.func_800162A4(c, m);

        c.A0 = body;
        c.A1 = LiveryPreviewScratch;
        c.A2 = LiveryPreviewScratch + 4u;
        GranTurismo2ArcadePC.func_80060AFC(c, m);
        if (c.V0 == 0u || palette >= c.V0)
            throw new InvalidDataException(
                "GT2 alternate-livery preview palette is invalid: " +
                $"target=0x{targetBody:X8}/{targetPalette}, " +
                $"body=0x{body:X8}/{palette}, count={c.V0}");

        m.WriteU8(preview + 0xDu, (byte)palette);
        c.A0 = body;
        c.A1 = preview + 0x190u;
        GranTurismo2ArcadePC.func_80076864(c, m);
        c.A0 = preview + 0x190u;
        c.A1 = preview + 0x214u;
        GranTurismo2ArcadePC.func_800770BC(c, m);
        m.WriteU16(preview + 0x444u, 2);
        m.WriteU32(preview + 0x440u, 1);

        // Frontend state, save/race hand-off, and the swatch cursor continue
        // to carry the one customer-facing identity and extended color slot.
        // Only the already-loaded native preview assets use the hidden body.
        m.WriteU32(preview + 0x8u, targetBody);
        m.WriteU8(preview + 0xDu, (byte)targetPalette);
    }
}
