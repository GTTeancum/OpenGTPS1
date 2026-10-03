using RecompOne.Runtime.Context;
using RecompOne.Runtime.Dispatch;
using RecompOne.Runtime.Memory;

namespace Recompiled.Arcade;

public static partial class GranTurismo2ArcadePC
{
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010000_gt2_arcade_overlay_5(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A1 + 0u;
        c.A0 = c.S0 + 0x8u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        MemoryAccess.WriteU32(m, (c.V0 - 0x75ACu), c.S0);
        MemoryAccess.WriteU32(m, (c.S0 + 0x4u), 0u);
        c.RA = 0x8001002Cu;
        GranTurismo2ArcadePC.func_80010610(c, m);
        c.A0 = c.S0 + 0x24u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80010038u;
        GranTurismo2ArcadePC.func_80010950(c, m);
        c.V1 = 0x80010000u;
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 + 0x70u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0xCu));
        c.V1 = c.V1 + 0x5ACu;
        MemoryAccess.WriteU32(m, (c.V0 + 0xCu), c.V1);
        MemoryAccess.WriteU32(m, c.S0, c.A0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010068(CpuContext c, IMemory m)
    {
        c.V1 = MemoryAccess.ReadU32(m, c.A0);
        c.V0 = 0x801F0000u;
        MemoryAccess.WriteU32(m, (c.V0 + 0x7Cu), c.V1);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010078_gt2_arcade_overlay_5(CpuContext c, IMemory m)
    {
        c.A0 = c.A0 + 0x8u;
        MemoryAccess.WriteU16(m, (c.A0 + 0xAu), (ushort)c.A1);
        MemoryAccess.WriteU32(m, (c.A0 + 0xCu), c.A2);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010088_gt2_arcade_overlay_5(CpuContext c, IMemory m)
    {
        MemoryAccess.WriteU32(m, (c.A0 + 0x44u), c.A1);
        MemoryAccess.WriteU32(m, (c.A0 + 0x48u), c.A2);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010094(CpuContext c, IMemory m)
    {
        c.A0 = c.A0 + 0x24u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x10u), c.A1);
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.A2);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800100A4(CpuContext c, IMemory m)
    {
        RecompOne.Runtime.Sdk.GT2Compat.ReconcileArcadeOpeningMovieExtent(m);
        c.SP = c.SP - 0x18u;
        c.V1 = 0x80090000u;
        c.V1 = c.V1 + 0x2088u;
        c.V0 = c.A1 << 2;
        c.V0 = c.V0 + c.V1;
        c.A1 = c.A1 + 0x1u;
        c.A1 = c.A1 << 2;
        c.A1 = c.A1 + c.V1;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.V0 = 0x801D0000u;
        c.A2 = MemoryAccess.ReadU32(m, c.A1);
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 - 0x71D4u));
        c.A2 = c.A2 - c.A3;
        c.A1 = c.A3 + c.A1;
        c.A2 = c.A2 - 0x19u;
        c.RA = 0x800100E8u;
        GranTurismo2ArcadePC.func_800100F8(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800100F8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.S0 = c.A2 + 0u;
        c.RA = 0x80010114u;
        GranTurismo2ArcadePC.func_80081B6C(c, m);
        c.S0 = c.S1 + c.S0;
        c.S0 = c.S0 - 0x1u;
        c.V1 = 0x801F0000u;
        c.V1 = c.V1 - 0x100u;
        c.V0 = 0x000000C0u;
        MemoryAccess.WriteU8(m, (c.V1 + 0x21u), (byte)c.V0);
        c.V0 = 0u | 0x8000u;
        MemoryAccess.WriteU16(m, (c.V1 + 0x18u), (ushort)c.V0);
        c.V0 = 0x00000001u;
        c.A0 = c.V1 + 0x20u;
        MemoryAccess.WriteU8(m, (c.V1 + 0x20u), (byte)0u);
        MemoryAccess.WriteU32(m, (c.V1 + 0x24u), c.S1);
        MemoryAccess.WriteU32(m, (c.V1 + 0x28u), c.S0);
        MemoryAccess.WriteU8(m, (c.A0 + 0x2u), (byte)c.V0);
        c.V0 = 0x00000007u;
        MemoryAccess.WriteU8(m, (c.A0 + 0x3u), (byte)c.V0);
        c.V0 = 0x80010000u;
        c.V0 = c.V0 + 0x420u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x2Cu), c.V0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010174(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        c.A0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        MemoryAccess.WriteU32(m, (c.S0 + 0x4Cu), 0u);
        c.RA = 0x80010190u;
        GranTurismo2ArcadePC.func_80086568(c, m);
        c.A0 = 0x80010000u;
        c.A0 = c.A0 + 0x49Cu;
        c.RA = 0x8001019Cu;
        GranTurismo2ArcadePC.func_80086804(c, m);
        c.A0 = c.S0 + 0x8u;
        c.RA = 0x800101A4u;
        GranTurismo2ArcadePC.func_80010634(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800101B4(CpuContext c, IMemory m)
    {
        RecompOne.Runtime.Sdk.GT2Compat.ServiceCdDevice(c, m);
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        c.A0 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.RA = 0x800101D0u;
        GranTurismo2ArcadePC.func_80081C08(c, m);
        c.S1 = 0x00000001u;
        c.V1 = MemoryAccess.ReadU16(m, (c.S0 + 0x10u));
        if (c.V1 != 0u) {
            c.A0 = c.V0 + 0u;
            goto L8001021C;
        }
        c.A0 = c.V0 + 0u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x8u));
        c.V0 = c.V0 & 0x0004u;
        if (c.V0 == 0u) {
            c.V0 = 0x801F0000u;
            goto L80010200;
        }
        c.V0 = 0x801F0000u;
        c.S1 = 0x00000002u;
        goto L80010230;
        L80010200: ;
        c.V0 = c.V0 - 0x100u;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x166u));
        if (c.V0 != 0u) {
            goto L80010230;
        }
        c.S1 = 0x00000003u;
        goto L80010230;
        L8001021C: ;
        c.V1 = MemoryAccess.ReadU8(m, (c.S0 + 0x2Cu));
        c.V0 = 0x00000002u;
        if (c.V1 != c.V0) {
            goto L80010230;
        }
        c.S1 = 0u + 0u;
        L80010230: ;
        c.RA = 0x80010238u;
        GranTurismo2ArcadePC.func_80081C08(c, m);
        c.V0 = c.S1 + 0u;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010250(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.A0 = 0u + 0u;
        c.RA = 0x80010268u;
        GranTurismo2ArcadePC.func_80086804(c, m);
        c.A0 = c.S0 + 0x24u;
        c.RA = 0x80010270u;
        GranTurismo2ArcadePC.func_80010A90(c, m);
        c.A0 = c.S0 + 0x8u;
        c.RA = 0x80010278u;
        GranTurismo2ArcadePC.func_800106A4(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010288(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = 0x00000006u;
        c.RA = 0x80010298u;
        GranTurismo2ArcadePC.func_8007C3FC(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800102A8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S2 = MemoryAccess.ReadU32(m, (c.S1 + 0x4Cu));
        if (c.S2 == 0u) {
            c.S0 = c.S1 + 0x24u;
            goto L800102FC;
        }
        c.S0 = c.S1 + 0x24u;
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x40u));
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x42u));
        c.A0 = c.S0 + 0u;
        c.RA = 0x800102E0u;
        GranTurismo2ArcadePC.func_80010994(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x3Cu));
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x3Eu));
        c.A3 = c.S2 + 0u;
        c.RA = 0x800102F4u;
        GranTurismo2ArcadePC.func_800109E4(c, m);
        c.V0 = 0x00000001u;
        goto L80010300;
        L800102FC: ;
        c.V0 = 0u + 0u;
        L80010300: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010318(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = c.S0 + 0x8u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        c.A0 = c.S3 + 0u;
        c.RA = 0x80010348u;
        GranTurismo2ArcadePC.func_80010894(c, m);
        MemoryAccess.WriteU16(m, (c.S0 + 0x3Cu), (ushort)c.S1);
        MemoryAccess.WriteU16(m, (c.S0 + 0x3Eu), (ushort)c.S2);
        c.V1 = MemoryAccess.ReadU16(m, (c.V0 + 0x4u));
        c.S1 = MemoryAccess.ReadU32(m, (c.S0 + 0x44u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x40u), (ushort)c.V1);
        c.A1 = MemoryAccess.ReadU16(m, (c.V0 + 0x6u));
        c.V1 = MemoryAccess.ReadU32(m, (c.S0 + 0x48u));
        c.A0 = c.V0 + 0u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x48u), c.S1);
        MemoryAccess.WriteU16(m, (c.S0 + 0x42u), (ushort)c.A1);
        c.A1 = c.S1 + 0u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x44u), c.V1);
        c.RA = 0x8001037Cu;
        GranTurismo2ArcadePC.func_80010AC0(c, m);
        c.A0 = c.S3 + 0u;
        c.RA = 0x80010384u;
        GranTurismo2ArcadePC.func_8001090C(c, m);
        c.V0 = c.S1 + 0u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x4Cu), c.V0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800103A8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        c.S2 = c.A2 + 0u;
        c.RA = 0x800103CCu;
        GranTurismo2ArcadePC.func_800102A8(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0u;
        c.A2 = c.S2 + 0u;
        c.RA = 0x800103DCu;
        GranTurismo2ArcadePC.func_80010318(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800103F4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.S0 = c.A0 + 0u;
        c.RA = 0x80010408u;
        GranTurismo2ArcadePC.func_80010318(c, m);
        c.A0 = c.S0 + 0u;
        c.RA = 0x80010410u;
        GranTurismo2ArcadePC.func_800102A8(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010420_gt2_arcade_overlay_5(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V0 = c.A0 + 0u;
        c.A2 = c.A1 + 0u;
        c.V1 = 0x800B0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V1 - 0x75ACu));
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A1 = c.V0 + 0u;
        c.RA = 0x80010440u;
        GranTurismo2ArcadePC.func_80010450(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010450(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.A1 = c.A0 + 0x8u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V0 = MemoryAccess.ReadU8(m, (c.A0 + 0x2Cu));
        c.V1 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        if (c.V0 != 0u) {
            c.V0 = c.V1 | 0x0008u;
            goto L8001047C;
        }
        c.V0 = c.V1 | 0x0008u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x8u), c.V0);
        c.V0 = c.V1 >> 1;
        c.V0 = c.V0 & 0x0002u;
        goto L8001048C;
        L8001047C: ;
        c.A0 = c.A1 + 0u;
        c.RA = 0x80010484u;
        GranTurismo2ArcadePC.func_800106EC(c, m);
        c.V0 = 0u < c.V0 ? 1u : 0u;
        c.V0 = c.V0 << 1;
        L8001048C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001049C(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75ACu));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.RA = 0x800104B4u;
        GranTurismo2ArcadePC.func_800104C4(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800104C4(CpuContext c, IMemory m)
    {
        RecompOne.Runtime.Runtime.BeginIrqDeferral();
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.S1 + 0x24u;
        c.A0 = c.S0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = MemoryAccess.ReadU16(m, (c.S1 + 0x32u));
        c.V0 = c.S2 & 0x0001u;
        c.V0 = c.V0 << 2;
        c.V0 = c.S1 + c.V0;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0x34u));
        c.S3 = MemoryAccess.ReadU8(m, (c.S1 + 0x2Du));
        c.A2 = 0u + 0u;
        c.RA = 0x8001050Cu;
        GranTurismo2ArcadePC.func_8007B980(c, m);
        c.V0 = MemoryAccess.ReadU16(m, (c.S1 + 0x24u));
        c.V1 = MemoryAccess.ReadU16(m, (c.S0 + 0x4u));
        c.S2 = c.S2 - 0x1u;
        c.V0 = c.V0 + c.V1;
        c.V1 = MemoryAccess.ReadU32(m, (c.S1 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S1 + 0x32u), (ushort)c.S2);
        MemoryAccess.WriteU16(m, (c.S1 + 0x24u), (ushort)c.V0);
        c.V0 = c.V1 & 0x0008u;
        if (c.V0 == 0u) {
            c.A0 = c.S1 + 0x8u;
            goto L80010544;
        }
        c.A0 = c.S1 + 0x8u;
        c.V0 = 0xFFFFFFF7u;
        c.V0 = c.V1 & c.V0;
        MemoryAccess.WriteU32(m, (c.S1 + 0x8u), c.V0);
        c.RA = 0x80010544u;
        GranTurismo2ArcadePC.func_800106EC(c, m);
        L80010544: ;
        if (c.S2 == 0u) {
            c.V0 = 0x00000001u;
            goto L8001055C;
        }
        c.V0 = 0x00000001u;
        c.V0 = MemoryAccess.ReadU8(m, (c.S1 + 0x2Cu));
        if (c.V0 == 0u) {
            c.V0 = 0x00000001u;
            goto L80010564;
        }
        c.V0 = 0x00000001u;
        L8001055C: ;
        MemoryAccess.WriteU8(m, (c.S1 + 0x2Cu), (byte)c.V0);
        goto L80010590;
        L80010564: ;
        if (c.S3 != 0u) {
            c.V0 = c.S2 & 0x0001u;
            goto L80010578;
        }
        c.V0 = c.S2 & 0x0001u;
        c.RA = 0x80010574u;
        GranTurismo2ArcadePC.func_8007AE40(c, m);
        c.V0 = c.S2 & 0x0001u;
        L80010578: ;
        c.V0 = c.V0 << 2;
        c.V0 = c.S1 + c.V0;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x34u));
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x2Eu));
        c.RA = 0x80010590u;
        GranTurismo2ArcadePC.func_8008673C(c, m);
        L80010590: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        RecompOne.Runtime.Runtime.EndIrqDeferral();
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800105AC(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75ACu));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.RA = 0x800105C4u;
        GranTurismo2ArcadePC.func_800105D4(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800105D4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V1 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V0 = MemoryAccess.ReadU8(m, (c.A0 + 0x2Cu));
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0x4u));
        if (c.V0 != c.V1) {
            c.V0 = 0x00000002u;
            goto L80010600;
        }
        c.V0 = 0x00000002u;
        if (c.A1 == 0u) {
            MemoryAccess.WriteU8(m, (c.A0 + 0x2Cu), (byte)c.V0);
            goto L80010600;
        }
        MemoryAccess.WriteU8(m, (c.A0 + 0x2Cu), (byte)c.V0);
        c.RA = 0x80010600u;
        Dispatcher.Call(c, m, c.A1);
        L80010600: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010610(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.A1 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A2 = 0x0000001Cu;
        c.RA = 0x80010624u;
        GranTurismo2ArcadePC.func_8008CD40(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010634(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.A0 = 0u + 0u;
        c.RA = 0x8001064Cu;
        GranTurismo2ArcadePC.func_80081C08(c, m);
        c.V1 = MemoryAccess.ReadU32(m, c.S0);
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0xCu));
        MemoryAccess.WriteU16(m, (c.S0 + 0x4u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.S0 + 0x6u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.S0 + 0x8u), (ushort)0u);
        c.V1 = c.V1 & 0x0001u;
        MemoryAccess.WriteU32(m, c.S0, c.V1);
        c.V1 = MemoryAccess.ReadU32(m, c.A0);
        c.A0 = 0x00000005u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x14u), 0u);
        MemoryAccess.WriteU32(m, (c.S0 + 0x18u), 0u);
        MemoryAccess.WriteU32(m, (c.S0 + 0x10u), c.V1);
        c.S0 = c.V0 + 0u;
        c.RA = 0x80010684u;
        GranTurismo2ArcadePC.func_8007C3FC(c, m);
        c.A0 = c.S0 + 0u;
        c.S0 = c.V0 + 0u;
        c.RA = 0x80010690u;
        GranTurismo2ArcadePC.func_80081C08(c, m);
        c.V0 = c.S0 + 0u;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800106A4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.A0 = 0u + 0u;
        c.RA = 0x800106BCu;
        GranTurismo2ArcadePC.func_80081C08(c, m);
        c.V1 = MemoryAccess.ReadU32(m, c.S0);
        c.V1 = c.V1 | 0x0004u;
        MemoryAccess.WriteU32(m, c.S0, c.V1);
        c.S0 = c.V0 + 0u;
        c.RA = 0x800106D4u;
        GranTurismo2ArcadePC.func_8007C480(c, m);
        c.A0 = c.S0 + 0u;
        c.RA = 0x800106DCu;
        GranTurismo2ArcadePC.func_80081C08(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800106EC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x50u;
        MemoryAccess.WriteU32(m, (c.SP + 0x34u), c.S1);
        c.S1 = c.A0 + 0u;
        c.A0 = c.SP + 0x10u;
        c.A1 = 0x00000008u;
        MemoryAccess.WriteU32(m, (c.SP + 0x4Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x48u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x3Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x38u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.S0);
        c.RA = 0x80010720u;
        GranTurismo2ArcadePC.func_8007CEE4(c, m);
        c.V1 = 0x00000160u;
        c.V0 = MemoryAccess.ReadU16(m, (c.SP + 0x10u));
        c.A0 = MemoryAccess.ReadU16(m, (c.SP + 0x12u));
        if (c.V0 != c.V1) {
            c.V0 = 0u + 0u;
            goto L8001086C;
        }
        c.V0 = 0u + 0u;
        c.V0 = 0x00005349u;
        if (c.A0 != c.V0) {
            c.V0 = 0u + 0u;
            goto L8001086C;
        }
        c.V0 = 0u + 0u;
        c.S0 = MemoryAccess.ReadU32(m, c.S1);
        c.S4 = MemoryAccess.ReadU32(m, (c.S1 + 0x18u));
        c.S6 = MemoryAccess.ReadU32(m, (c.S1 + 0x14u));
        c.A0 = MemoryAccess.ReadU32(m, (c.S1 + 0x10u));
        c.S2 = MemoryAccess.ReadU16(m, (c.S1 + 0x6u));
        c.S5 = MemoryAccess.ReadU16(m, (c.S1 + 0x8u));
        c.T0 = MemoryAccess.ReadU16(m, (c.S1 + 0xAu));
        c.V0 = c.S0 & 0x0004u;
        if (c.V0 == 0u) {
            c.S3 = c.A0 + 0u;
            goto L80010770;
        }
        c.S3 = c.A0 + 0u;
        c.V0 = 0x00000001u;
        goto L8001086C;
        L80010770: ;
        c.V1 = 0x3FFF0000u;
        c.V1 = c.V1 | 0xFFFFu;
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.A1 = MemoryAccess.ReadU16(m, (c.SP + 0x14u));
        c.A3 = MemoryAccess.ReadU16(m, (c.SP + 0x16u));
        if (c.A1 != 0u) {
            c.T1 = c.V0 & c.V1;
            goto L800107C4;
        }
        c.T1 = c.V0 & c.V1;
        c.V0 = 0xFFFFFFFDu;
        c.S0 = c.S0 & c.V0;
        c.V1 = c.S5 << 16;
        c.V0 = c.T0 << 16;
        if (c.V1 == c.V0) {
            c.V0 = c.S2 << 16;
            goto L800107E0;
        }
        c.V0 = c.S2 << 16;
        c.S6 = c.A2 + 0u;
        c.V1 = MemoryAccess.ReadU32(m, (c.S1 + 0xCu));
        c.V0 = (uint)((int)c.V0 >> 14);
        c.V0 = c.V0 + c.V1;
        c.A0 = MemoryAccess.ReadU32(m, c.V0);
        c.S4 = 0u + 0u;
        goto L800107F4;
        L800107C4: ;
        c.V0 = c.S0 & 0x0002u;
        if (c.V0 != 0u) {
            c.V0 = c.A2 < c.T1 ? 1u : 0u;
            goto L800107E4;
        }
        c.V0 = c.A2 < c.T1 ? 1u : 0u;
        if (c.S6 != c.A2) {
            goto L800107E4;
        }
        if (c.S4 == c.A1) {
            goto L800107F4;
        }
        L800107E0: ;
        c.V0 = c.A2 < c.T1 ? 1u : 0u;
        L800107E4: ;
        if (c.V0 != 0u) {
            c.S0 = c.S0 | 0x0002u;
            goto L8001084C;
        }
        c.S0 = c.S0 | 0x0002u;
        c.S0 = c.S0 | 0x0004u;
        goto L8001084C;
        L800107F4: ;
        c.S4 = c.S4 + 0x1u;
        c.V0 = c.A3 - 0x1u;
        if (c.A1 != c.V0) {
            c.S3 = c.A0 + 0x7E0u;
            goto L80010844;
        }
        c.S3 = c.A0 + 0x7E0u;
        c.V0 = c.S2 + 0x1u;
        c.S2 = c.V0 + 0u;
        c.V0 = c.V0 << 16;
        c.V1 = c.T0 << 16;
        if (c.V0 != c.V1) {
            c.S5 = c.S5 + 0x1u;
            goto L80010820;
        }
        c.S5 = c.S5 + 0x1u;
        c.S2 = 0u + 0u;
        L80010820: ;
        c.V0 = c.S2 << 16;
        c.V1 = MemoryAccess.ReadU32(m, (c.S1 + 0xCu));
        c.V0 = (uint)((int)c.V0 >> 14);
        c.V0 = c.V0 + c.V1;
        c.S3 = MemoryAccess.ReadU32(m, c.V0);
        c.V0 = c.A2 < c.T1 ? 1u : 0u;
        if (c.V0 != 0u) {
            goto L80010844;
        }
        c.S0 = c.S0 | 0x0004u;
        L80010844: ;
        c.A1 = 0x000001F8u;
        c.RA = 0x8001084Cu;
        GranTurismo2ArcadePC.func_8007CEE4(c, m);
        L8001084C: ;
        c.V0 = c.S0 & 0x0004u;
        c.V0 = 0u < c.V0 ? 1u : 0u;
        MemoryAccess.WriteU32(m, c.S1, c.S0);
        MemoryAccess.WriteU32(m, (c.S1 + 0x18u), c.S4);
        MemoryAccess.WriteU32(m, (c.S1 + 0x14u), c.S6);
        MemoryAccess.WriteU32(m, (c.S1 + 0x10u), c.S3);
        MemoryAccess.WriteU16(m, (c.S1 + 0x6u), (ushort)c.S2);
        MemoryAccess.WriteU16(m, (c.S1 + 0x8u), (ushort)c.S5);
        L8001086C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x4Cu));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x48u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x44u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x40u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x3Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x38u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x34u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x30u));
        c.SP = c.SP + 0x50u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010894(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        c.A0 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.RA = 0x800108B0u;
        GranTurismo2ArcadePC.func_80081C08(c, m);
        c.A0 = c.V0 + 0u;
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x4u));
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x8u));
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0xAu));
        if (c.V0 == 0u) {
            c.S1 = 0u + 0u;
            goto L800108EC;
        }
        c.S1 = 0u + 0u;
        c.V0 = c.A1 << 2;
        c.V1 = MemoryAccess.ReadU32(m, (c.S0 + 0xCu));
        c.A1 = c.A1 + 0x1u;
        c.V0 = c.V0 + c.V1;
        c.S1 = MemoryAccess.ReadU32(m, c.V0);
        if (c.A1 != c.A2) {
            goto L800108E8;
        }
        c.A1 = 0u + 0u;
        L800108E8: ;
        MemoryAccess.WriteU16(m, (c.S0 + 0x4u), (ushort)c.A1);
        L800108EC: ;
        c.RA = 0x800108F4u;
        GranTurismo2ArcadePC.func_80081C08(c, m);
        c.V0 = c.S1 + 0u;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001090C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.A0 = 0u + 0u;
        c.RA = 0x80010924u;
        GranTurismo2ArcadePC.func_80081C08(c, m);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x8u));
        if (c.V1 == 0u) {
            c.A0 = c.V0 + 0u;
            goto L80010938;
        }
        c.A0 = c.V0 + 0u;
        c.V1 = c.V1 - 0x1u;
        L80010938: ;
        MemoryAccess.WriteU16(m, (c.S0 + 0x8u), (ushort)c.V1);
        c.RA = 0x80010940u;
        GranTurismo2ArcadePC.func_80081C08(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010950(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A1 + 0u;
        c.A1 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.A2 = 0x00000018u;
        c.RA = 0x80010974u;
        GranTurismo2ArcadePC.func_8008CD40(c, m);
        c.V0 = 0x00000002u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x8u), (byte)c.V0);
        MemoryAccess.WriteU8(m, (c.S0 + 0x9u), (byte)c.S1);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010994(CpuContext c, IMemory m)
    {
        c.A3 = c.A0 + 0u;
        c.A1 = c.A1 + 0xFu;
        c.A1 = (uint)((int)c.A1 >> 4);
        c.A0 = c.A2 + 0xFu;
        c.V0 = 0xFFFFFFF0u;
        c.V1 = MemoryAccess.ReadU8(m, (c.A3 + 0x9u));
        c.A0 = c.A0 & c.V0;
        if (c.V1 != 0u) {
            MemoryAccess.WriteU16(m, (c.A3 + 0xCu), (ushort)c.A1);
            goto L800109C4;
        }
        MemoryAccess.WriteU16(m, (c.A3 + 0xCu), (ushort)c.A1);
        c.V0 = c.A0 << 3;
        c.A1 = 0x00000010u;
        goto L800109D4;
        L800109C4: ;
        c.V0 = c.A0 << 1;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.A1 = 0x00000018u;
        L800109D4: ;
        MemoryAccess.WriteU16(m, (c.A3 + 0x4u), (ushort)c.A1);
        MemoryAccess.WriteU16(m, (c.A3 + 0x6u), (ushort)c.A2);
        MemoryAccess.WriteU16(m, (c.A3 + 0xAu), (ushort)c.V0);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800109E4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = MemoryAccess.ReadU8(m, (c.S0 + 0x9u));
        c.S2 = MemoryAccess.ReadU16(m, (c.S0 + 0xCu));
        c.S3 = c.A3 + 0u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x8u), (byte)0u);
        if (c.S1 == 0u) {
            MemoryAccess.WriteU16(m, (c.S0 + 0xEu), (ushort)c.S2);
            goto L80010A2C;
        }
        MemoryAccess.WriteU16(m, (c.S0 + 0xEu), (ushort)c.S2);
        c.V0 = c.A1 << 1;
        c.V0 = c.V0 + c.A1;
        c.V0 = (uint)((int)c.V0 >> 1);
        MemoryAccess.WriteU16(m, c.S0, (ushort)c.V0);
        goto L80010A30;
        L80010A2C: ;
        MemoryAccess.WriteU16(m, c.S0, (ushort)c.A1);
        L80010A30: ;
        MemoryAccess.WriteU16(m, (c.S0 + 0x2u), (ushort)c.A2);
        c.A0 = 0x00000001u;
        c.RA = 0x80010A3Cu;
        GranTurismo2ArcadePC.func_80086568(c, m);
        if (c.S1 != 0u) {
            c.A0 = c.S3 + 0u;
            goto L80010A50;
        }
        c.A0 = c.S3 + 0u;
        c.RA = 0x80010A4Cu;
        GranTurismo2ArcadePC.func_8007AE40(c, m);
        c.A0 = c.S3 + 0u;
        L80010A50: ;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80010A58u;
        GranTurismo2ArcadePC.func_800866C0(c, m);
        c.V0 = c.S2 & 0x0001u;
        c.V0 = c.V0 << 2;
        c.V0 = c.S0 + c.V0;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x10u));
        c.A1 = MemoryAccess.ReadU16(m, (c.S0 + 0xAu));
        c.RA = 0x80010A74u;
        GranTurismo2ArcadePC.func_8008673C(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010A90(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.A0 = 0x00000001u;
        c.RA = 0x80010AA8u;
        GranTurismo2ArcadePC.func_80086568(c, m);
        c.V0 = 0x00000003u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x8u), (byte)c.V0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010AC0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.V0 = MemoryAccess.ReadU16(m, c.A0);
        c.S2 = c.A1 + 0u;
        MemoryAccess.WriteU16(m, c.S2, (ushort)c.V0);
        c.V0 = MemoryAccess.ReadU16(m, (c.A0 + 0x2u));
        MemoryAccess.WriteU16(m, (c.S2 + 0x2u), (ushort)c.V0);
        c.V1 = MemoryAccess.ReadU16(m, (c.A0 + 0x4u));
        c.V0 = MemoryAccess.ReadU16(m, (c.A0 + 0x6u));
        c.V1 = c.V1 + 0xFu;
        c.V1 = (uint)((int)c.V1 >> 4);
        c.V0 = c.V0 + 0xFu;
        c.V0 = (uint)((int)c.V0 >> 4);
        { var _r = (long)(int)c.V1 * (int)c.V0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.FP = c.SP + 0u;
        c.S0 = c.A0 + 0xAu;
        c.V0 = MemoryAccess.ReadU16(m, (c.A0 + 0x8u));
        c.A1 = c.S0 + 0u;
        c.S0 = c.S0 + c.V0;
        c.V1 = c.LO;
        c.A2 = c.V1 << 1;
        c.A2 = c.A2 + c.V1;
        c.S1 = c.A2 << 1;
        c.A2 = c.A2 << 2;
        c.V0 = c.A2 + 0x7u;
        c.V0 = c.V0 >> 3;
        c.V0 = c.V0 << 3;
        c.SP = c.SP - c.V0;
        c.A0 = c.SP + 0x10u;
        c.RA = 0x80010B4Cu;
        GranTurismo2ArcadePC.func_80083C1C(c, m);
        c.V1 = MemoryAccess.ReadU16(m, c.S0);
        c.A0 = MemoryAccess.ReadU16(m, (c.S0 + 0x2u));
        c.S0 = c.S0 + 0x4u;
        c.A1 = 0x00000010u;
        c.T1 = c.V0 + 0u;
        c.T3 = c.T1 + c.S1;
        c.A3 = c.S2 + 0x4u;
        c.T9 = 0xFFFFFFFFu;
        c.T8 = 0u | 0xFE00u;
        c.T2 = c.A1 + 0u;
        c.T7 = 0x00000006u;
        c.T6 = 0x00000005u;
        c.T5 = 0x00000017u;
        c.V0 = 0x80010000u;
        c.T4 = c.V0 + 0x14CCu;
        c.V1 = c.V1 << (int)(c.A1 & 31u);
        L80010B8C: ;
        c.A0 = c.V1 | c.A0;
        L80010B90: ;
        c.S1 = c.S1 - 0x1u;
        if (c.S1 == c.T9) {
            goto L80010CE0;
        }
        c.V1 = MemoryAccess.ReadU8(m, c.T3);
        c.T3 = c.T3 + 0x1u;
        c.V0 = MemoryAccess.ReadU8(m, c.T1);
        c.T1 = c.T1 + 0x1u;
        c.V0 = c.V0 << 8;
        c.V1 = c.V1 | c.V0;
        MemoryAccess.WriteU16(m, c.A3, (ushort)c.V1);
        c.A3 = c.A3 + 0x2u;
        L80010BBC: ;
        RecompOne.Runtime.Gte.Write(30, c.A0);
        if ((int)c.A0 >= 0) {
            c.V1 = c.A0 >> 29;
            goto L80010C18;
        }
        c.V1 = c.A0 >> 29;
        c.V0 = (int)c.V1 < 6 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A2 = 0x00000003u;
            goto L80010C00;
        }
        c.A2 = 0x00000003u;
        MemoryAccess.WriteU16(m, c.A3, (ushort)c.T8);
        c.A3 = c.A3 + 0x2u;
        c.A1 = c.A1 - 0x2u;
        if ((int)c.A1 > 0) {
            c.A0 = c.A0 << 2;
            goto L80010B90;
        }
        c.A0 = c.A0 << 2;
        c.A1 = c.A1 + 0x10u;
        c.V1 = MemoryAccess.ReadU16(m, c.S0);
        c.S0 = c.S0 + 0x2u;
        c.V0 = c.T2 - c.A1;
        c.V1 = c.V1 << (int)(c.V0 & 31u);
        goto L80010B8C;
        L80010C00: ;
        if (c.V1 != c.T7) {
            c.V0 = 0x000003FFu;
            goto L80010C0C;
        }
        c.V0 = 0x000003FFu;
        c.V0 = 0x00000001u;
        L80010C0C: ;
        MemoryAccess.WriteU16(m, c.A3, (ushort)c.V0);
        c.A3 = c.A3 + 0x2u;
        goto L80010CB8;
        L80010C18: ;
        c.A2 = RecompOne.Runtime.Gte.Read(31);
        c.V0 = c.A0 >> 23;
        if (c.V0 != 0u) {
            c.T0 = c.A2 << 8;
            goto L80010C54;
        }
        c.T0 = c.A2 << 8;
        c.A1 = c.A1 - 0x9u;
        if ((int)c.A1 > 0) {
            c.A0 = c.A0 << 9;
            goto L80010C4C;
        }
        c.A0 = c.A0 << 9;
        c.A1 = c.A1 + 0x10u;
        c.V1 = MemoryAccess.ReadU16(m, c.S0);
        c.S0 = c.S0 + 0x2u;
        c.V0 = c.T2 - c.A1;
        c.V1 = c.V1 << (int)(c.V0 & 31u);
        c.A0 = c.A0 | c.V1;
        L80010C4C: ;
        c.A2 = c.A2 - 0x9u;
        goto L80010C94;
        L80010C54: ;
        if (c.A2 != c.T6) {
            c.V0 = c.T5 - c.A2;
            goto L80010C98;
        }
        c.V0 = c.T5 - c.A2;
        c.A1 = c.A1 - 0x6u;
        if ((int)c.A1 > 0) {
            c.A0 = c.A0 << 6;
            goto L80010C80;
        }
        c.A0 = c.A0 << 6;
        c.A1 = c.A1 + 0x10u;
        c.V1 = MemoryAccess.ReadU16(m, c.S0);
        c.S0 = c.S0 + 0x2u;
        c.V0 = c.T2 - c.A1;
        c.V1 = c.V1 << (int)(c.V0 & 31u);
        c.A0 = c.A0 | c.V1;
        L80010C80: ;
        c.V0 = c.A0 >> 16;
        MemoryAccess.WriteU16(m, c.A3, (ushort)c.V0);
        c.A3 = c.A3 + 0x2u;
        c.A2 = 0x00000010u;
        goto L80010CB8;
        L80010C94: ;
        c.V0 = c.T5 - c.A2;
        L80010C98: ;
        c.V0 = c.A0 >> (int)(c.V0 & 31u);
        c.V0 = c.V0 & 0x00FCu;
        c.T0 = c.T0 + c.V0;
        c.V0 = c.T0 + c.T4;
        c.V0 = MemoryAccess.ReadU32(m, c.V0);
        c.A3 = c.A3 + 0x2u;
        c.A2 = c.V0 >> 26;
        MemoryAccess.WriteU16(m, (c.A3 - 0x2u), (ushort)c.V0);
        L80010CB8: ;
        c.A1 = c.A1 - c.A2;
        if ((int)c.A1 > 0) {
            c.A0 = c.A0 << (int)(c.A2 & 31u);
            goto L80010BBC;
        }
        c.A0 = c.A0 << (int)(c.A2 & 31u);
        c.A1 = c.A1 + 0x10u;
        c.V1 = MemoryAccess.ReadU16(m, c.S0);
        c.S0 = c.S0 + 0x2u;
        c.V0 = c.T2 - c.A1;
        c.V1 = c.V1 << (int)(c.V0 & 31u);
        c.A0 = c.A0 | c.V1;
        goto L80010BBC;
        L80010CE0: ;
        c.V0 = MemoryAccess.ReadU16(m, c.S2);
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + 0x4u;
        c.A1 = c.S2 + c.V0;
        c.V0 = c.A3 < c.A1 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L80010D18;
        }
        c.V1 = 0u | 0xFE00u;
        L80010D04: ;
        MemoryAccess.WriteU16(m, c.A3, (ushort)c.V1);
        c.A3 = c.A3 + 0x2u;
        c.V0 = c.A3 < c.A1 ? 1u : 0u;
        if (c.V0 != 0u) {
            goto L80010D04;
        }
        L80010D18: ;
        c.SP = c.FP + 0u;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.FP = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010D38(CpuContext c, IMemory m)
    {
        c.A1 = 0x0000001Fu;
        c.V0 = 0x80180000u;
        c.V0 = c.V0 - 0x39A8u;
        c.A0 = c.V0 + 0x7Cu;
        c.V1 = 0x00080000u;
        c.V1 = c.V1 | 0x9520u;
        c.V0 = 0x800B0000u;
        c.V0 = c.V0 - 0x75A8u;
        c.V0 = c.V0 + c.V1;
        L80010D5C: ;
        MemoryAccess.WriteU32(m, c.A0, c.V0);
        c.A0 = c.A0 - 0x4u;
        c.A1 = c.A1 - 0x1u;
        if ((int)c.A1 >= 0) {
            c.V0 = c.V0 - 0x46E0u;
            goto L80010D5C;
        }
        c.V0 = c.V0 - 0x46E0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010D78(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = 0xFFFFFFFFu;
        c.RA = 0x80010D88u;
        GranTurismo2ArcadePC.func_8007D14C(c, m);
        c.A0 = 0x00000001u;
        c.RA = 0x80010D90u;
        GranTurismo2ArcadePC.func_8007F740(c, m);
        c.A0 = 0x00000003u;
        c.RA = 0x80010D98u;
        GranTurismo2ArcadePC.func_8007D14C(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010DA8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x3C28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x3C24u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x3C20u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x3C1Cu), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x3C18u), c.S0);
        c.RA = 0x80010DC0u;
        GranTurismo2ArcadePC.func_80010D78(c, m);
        c.A0 = c.SP + 0x10u;
        c.A1 = 0u + 0u;
        c.A2 = 0x00003C00u;
        c.RA = 0x80010DD0u;
        GranTurismo2ArcadePC.func_8008CD40(c, m);
        c.S0 = 0u + 0u;
        c.S2 = 0x000003C0u;
        c.S1 = 0x00000008u;
        c.V0 = (int)c.S0 < 512 ? 1u : 0u;
        L80010DE0: ;
        if (c.V0 == 0u) {
            c.A0 = c.SP + 0x3C10u;
            goto L80010E14;
        }
        c.A0 = c.SP + 0x3C10u;
        c.A1 = c.SP + 0x10u;
        c.A2 = 0u + 0u;
        MemoryAccess.WriteU16(m, (c.SP + 0x3C10u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.SP + 0x3C12u), (ushort)c.S0);
        MemoryAccess.WriteU16(m, (c.SP + 0x3C14u), (ushort)c.S2);
        MemoryAccess.WriteU16(m, (c.SP + 0x3C16u), (ushort)c.S1);
        c.RA = 0x80010E04u;
        GranTurismo2ArcadePC.func_8007B980(c, m);
        c.S0 = c.S0 + 0x8u;
        c.RA = 0x80010E0Cu;
        GranTurismo2ArcadePC.func_8007AE40(c, m);
        c.V0 = (int)c.S0 < 512 ? 1u : 0u;
        goto L80010DE0;
        L80010E14: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x3C24u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x3C20u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x3C1Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x3C18u));
        c.SP = c.SP + 0x3C28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010E2C(CpuContext c, IMemory m)
    {
        c.V0 = 0x80090000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x56A8u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V0 = MemoryAccess.ReadU32(m, c.A0);
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4Cu));
        c.RA = 0x80010E54u;
        Dispatcher.Call(c, m, c.V0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010E64(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        c.S2 = c.A2 + 0u;
        c.RA = 0x80010E88u;
        GranTurismo2ArcadePC.func_80080768(c, m);
        c.A0 = c.S1 + 0x6Cu;
        c.V0 = 0x80010000u;
        c.V0 = c.V0 + 0x156Cu;
        c.A2 = 0x800A0000u;
        c.A1 = 0u + 0u;
        c.A2 = c.A2 + 0x6C54u;
        MemoryAccess.WriteU32(m, c.S1, c.V0);
        c.RA = 0x80010EA8u;
        GranTurismo2ArcadePC.func_8007F9C8(c, m);
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.S1 + 0xE0u), 0u);
        MemoryAccess.WriteU32(m, (c.S1 + 0xE4u), c.V0);
        if (c.S2 == 0u) {
            MemoryAccess.WriteU32(m, (c.S1 + 0x138u), c.S0);
            goto L80010EC8;
        }
        MemoryAccess.WriteU32(m, (c.S1 + 0x138u), c.S0);
        c.V0 = 0x00000002u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x13Cu), c.V0);
        goto L80010ECC;
        L80010EC8: ;
        MemoryAccess.WriteU32(m, (c.S1 + 0x13Cu), 0u);
        L80010ECC: ;
        c.V0 = 0x80090000u;
        if (c.S2 == 0u) {
            MemoryAccess.WriteU8(m, (c.V0 + 0x56A4u), (byte)0u);
            goto L80010EE0;
        }
        MemoryAccess.WriteU8(m, (c.V0 + 0x56A4u), (byte)0u);
        c.V0 = 0x00000003u;
        goto L80010EE4;
        L80010EE0: ;
        c.V0 = 0x00000002u;
        L80010EE4: ;
        MemoryAccess.WriteU8(m, (c.S1 + 0x1Eu), (byte)c.V0);
        if (c.S2 == 0u) {
            MemoryAccess.WriteU32(m, (c.S1 + 0x148u), 0u);
            goto L80010EFC;
        }
        MemoryAccess.WriteU32(m, (c.S1 + 0x148u), 0u);
        c.V0 = 0x80010000u;
        c.V0 = c.V0 + 0x20D4u;
        goto L80010F04;
        L80010EFC: ;
        c.V0 = 0x80010000u;
        c.V0 = c.V0 + 0x20CCu;
        L80010F04: ;
        MemoryAccess.WriteU32(m, (c.S1 + 0x14Cu), c.V0);
        c.V0 = c.S1 + 0u;
        MemoryAccess.WriteU32(m, (c.V0 + 0x150u), 0u);
        MemoryAccess.WriteU32(m, (c.V0 + 0x154u), 0u);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010F2C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A1 + 0u;
        c.V0 = 0x80010000u;
        c.V0 = c.V0 + 0x156Cu;
        c.A0 = c.S0 + 0x6Cu;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        MemoryAccess.WriteU32(m, c.S0, c.V0);
        c.RA = 0x80010F58u;
        GranTurismo2ArcadePC.func_8007FA48(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80010F64u;
        GranTurismo2ArcadePC.func_800807D4(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010F78(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.RA = 0x80010F90u;
        GranTurismo2ArcadePC.func_800808C0(c, m);
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x140u), 0u);
        MemoryAccess.WriteU8(m, (c.S1 + 0x1Fu), (byte)c.V0);
        MemoryAccess.WriteU8(m, (c.S1 + 0x1Du), (byte)0u);
        MemoryAccess.WriteU32(m, (c.S1 + 0x144u), 0u);
        c.RA = 0x80010FA8u;
        GranTurismo2ArcadePC.func_80010D38(c, m);
        c.S0 = c.S1 + 0xE8u;
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0xE4u));
        c.A0 = c.S0 + 0u;
        c.RA = 0x80010FB8u;
        Dispatcher.Call(c, m, 0x80010000u);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x00000020u;
        c.A2 = 0x80180000u;
        c.A2 = c.A2 - 0x39A8u;
        c.V0 = 0x80010000u;
        c.V0 = c.V0 + 0xE2Cu;
        MemoryAccess.WriteU32(m, (c.S0 + 0x4u), c.V0);
        c.RA = 0x80010FD8u;
        Dispatcher.Call(c, m, 0x80010078u);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x80130000u;
        c.A1 = c.A1 + 0x6658u;
        c.A2 = 0x80150000u;
        c.A2 = c.A2 + 0x6658u;
        c.RA = 0x80010FF0u;
        Dispatcher.Call(c, m, 0x80010088u);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x80170000u;
        c.A1 = c.A1 + 0x6658u;
        c.A2 = c.A1 + 0x3000u;
        c.RA = 0x80011004u;
        GranTurismo2ArcadePC.func_80010094(c, m);
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x138u));
        c.A0 = c.S0 + 0u;
        c.RA = 0x80011010u;
        GranTurismo2ArcadePC.func_800100A4(c, m);
        c.A0 = c.S0 + 0u;
        c.RA = 0x80011018u;
        GranTurismo2ArcadePC.func_80010174(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001102C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        c.A0 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.RA = 0x80011050u;
        GranTurismo2ArcadePC.func_8007D14C(c, m);
        c.S2 = c.V0 + 0u;
        c.S3 = 0x00000001u;
        L80011058: ;
        c.A0 = 0u + 0u;
        c.RA = 0x80011060u;
        GranTurismo2ArcadePC.func_8007D14C(c, m);
        c.A0 = c.S0 + 0x78u;
        c.A1 = c.S0 + 0xD0u;
        c.S1 = c.V0 + 0u;
        c.RA = 0x80011070u;
        GranTurismo2ArcadePC.func_800838A8(c, m);
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0xE0u));
        if (c.V0 != 0u) {
            c.V1 = 0x00010000u;
            goto L800110AC;
        }
        c.V1 = 0x00010000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0xD4u));
        c.V0 = c.V0 & c.V1;
        if (c.V0 == 0u) {
            goto L800110AC;
        }
        L80011094: ;
        c.A0 = c.S0 + 0xE8u;
        c.RA = 0x8001109Cu;
        GranTurismo2ArcadePC.func_800101B4(c, m);
        if (c.V0 == c.S3) {
            goto L80011094;
        }
        L800110A4: ;
        c.V0 = 0u + 0u;
        goto L800111B0;
        L800110AC: ;
        c.A0 = c.S0 + 0xE8u;
        c.RA = 0x800110B4u;
        GranTurismo2ArcadePC.func_800101B4(c, m);
        c.A0 = c.V0 + 0u;
        if (c.A0 != c.S3) {
            c.V0 = c.S1 - c.S2;
            goto L800110D4;
        }
        c.V0 = c.S1 - c.S2;
        c.V0 = (int)c.V0 < 900 ? 1u : 0u;
        if (c.V0 != 0u) {
            goto L800110D4;
        }
        c.S2 = c.S1 + 0u;
        c.A0 = 0x00000003u;
        L800110D4: ;
        if (c.A0 != 0u) {
            c.V0 = 0x00000002u;
            goto L80011124;
        }
        c.V0 = 0x00000002u;
        c.V1 = MemoryAccess.ReadU32(m, (c.S0 + 0x13Cu));
        if (c.V1 == 0u) {
            goto L8001110C;
        }
        c.V0 = MemoryAccess.ReadU16(m, (c.S0 + 0xF8u));
        c.V0 = c.V0 << 16;
        c.V0 = (uint)((int)c.V0 >> 16);
        c.V0 = (int)c.V0 < (int)c.V1 ? 1u : 0u;
        if (c.V0 != 0u) {
            goto L80011058;
        }
        MemoryAccess.WriteU32(m, (c.S0 + 0x13Cu), 0u);
        L8001110C: ;
        c.V0 = MemoryAccess.ReadU8(m, (c.S0 + 0x1Eu));
        c.V1 = MemoryAccess.ReadU32(m, (c.S0 + 0x140u));
        c.V0 = c.V0 - 0x1u;
        c.V1 = (int)c.V1 < (int)c.V0 ? 1u : 0u;
        if (c.V1 != 0u) {
            c.V0 = 0x00000002u;
            goto L80011160;
        }
        c.V0 = 0x00000002u;
        L80011124: ;
        if (c.A0 == c.V0) {
            c.V0 = 0x00000003u;
            goto L800110A4;
        }
        c.V0 = 0x00000003u;
        if (c.A0 != c.V0) {
            goto L80011058;
        }
        c.V0 = MemoryAccess.ReadU32(m, c.S0);
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x48u));
        c.A0 = c.S0 + 0u;
        c.RA = 0x8001114Cu;
        Dispatcher.Call(c, m, c.V0);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 == 0u) {
            c.V0 = 0u + 0u;
            goto L80011058;
        }
        c.V0 = 0u + 0u;
        goto L800111B0;
        L80011160: ;
        c.A0 = c.S0 + 0xE8u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x144u));
        c.V1 = MemoryAccess.ReadU32(m, (c.S0 + 0x14Cu));
        c.A1 = MemoryAccess.ReadU32(m, (c.S0 + 0x150u));
        c.A2 = MemoryAccess.ReadU32(m, (c.S0 + 0x154u));
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.V1;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, c.V0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x2u));
        c.A1 = c.V1 + c.A1;
        c.A2 = c.V0 + c.A2;
        c.RA = 0x80011190u;
        GranTurismo2ArcadePC.func_800103A8(c, m);
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x144u));
        c.V1 = MemoryAccess.ReadU8(m, (c.S0 + 0x1Eu));
        c.V0 = c.V0 + 0x1u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x144u), c.V0);
        c.V0 = (int)c.V0 < (int)c.V1 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = 0x00000001u;
            goto L800111B0;
        }
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x144u), 0u);
        L800111B0: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800111CC(CpuContext c, IMemory m)
    {
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800111D4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x148u));
        c.V1 = MemoryAccess.ReadU32(m, (c.S0 + 0x18u));
        c.V0 = c.V0 + 0x1u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x148u), c.V0);
        c.V0 = (int)c.V0 < (int)c.V1 ? 1u : 0u;
        if (c.V0 != 0u) {
            goto L80011280;
        }
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x140u));
        if ((int)c.V0 <= 0) {
            MemoryAccess.WriteU32(m, (c.S0 + 0x148u), 0u);
            goto L80011280;
        }
        MemoryAccess.WriteU32(m, (c.S0 + 0x148u), 0u);
        c.V1 = MemoryAccess.ReadU8(m, (c.S0 + 0x1Du));
        c.V0 = c.V0 - 0x1u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x140u), c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x14Cu));
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.V1);
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.V1 + 0x2u));
        c.A0 = c.V0 << 1;
        c.A0 = c.A0 + c.V0;
        c.A0 = (uint)((int)c.A0 >> 1);
        c.RA = 0x80011240u;
        GranTurismo2ArcadePC.func_80081E3C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.S0 + 0x1Du));
        c.V1 = MemoryAccess.ReadU8(m, (c.S0 + 0x1Eu));
        c.V0 = c.V0 + 0x1u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x1Du), (byte)c.V0);
        c.V0 = c.V0 & 0x00FFu;
        c.V0 = c.V0 < c.V1 ? 1u : 0u;
        if (c.V0 != 0u) {
            goto L80011264;
        }
        MemoryAccess.WriteU8(m, (c.S0 + 0x1Du), (byte)0u);
        L80011264: ;
        c.V0 = MemoryAccess.ReadU8(m, (c.S0 + 0x1Fu));
        if (c.V0 == 0u) {
            goto L80011280;
        }
        MemoryAccess.WriteU8(m, (c.S0 + 0x1Fu), (byte)0u);
        c.A0 = 0u + 0u;
        c.RA = 0x80011280u;
        GranTurismo2ArcadePC.func_8007F740(c, m);
        L80011280: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80011290(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.S1 + 0xE8u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.A0 = c.S0 + 0u;
        c.RA = 0x800112B0u;
        GranTurismo2ArcadePC.func_80010250(c, m);
        c.A0 = c.S0 + 0u;
        c.RA = 0x800112B8u;
        GranTurismo2ArcadePC.func_80010068(c, m);
        c.A0 = c.S1 + 0u;
        c.RA = 0x800112C0u;
        GranTurismo2ArcadePC.func_80080B54(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800112D4(CpuContext c, IMemory m)
    {
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800112DC(CpuContext c, IMemory m)
    {
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800112E4(CpuContext c, IMemory m)
    {
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800112EC(CpuContext c, IMemory m)
    {
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800112F4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = c.A0 + 0xE8u;
        c.RA = 0x80011304u;
        GranTurismo2ArcadePC.func_80010288(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = 0x00000001u;
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80011314(CpuContext c, IMemory m)
    {
        c.V0 = MemoryAccess.ReadU32(m, (c.A0 + 0x140u));
        c.V0 = c.V0 + 0x1u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x140u), c.V0);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80011328(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x170u;
        c.A0 = c.SP + 0x10u;
        c.A1 = 0x00000018u;
        c.A2 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x16Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x168u), c.S0);
        c.RA = 0x80011344u;
        GranTurismo2ArcadePC.func_80010E64(c, m);
        c.RA = 0x8001134Cu;
        GranTurismo2ArcadePC.func_80010DA8(c, m);
        c.A0 = 0xFFFFFFFFu;
        c.RA = 0x80011354u;
        GranTurismo2ArcadePC.func_8007D14C(c, m);
        c.A0 = 0x00000031u;
        c.RA = 0x8001135Cu;
        GranTurismo2ArcadePC.func_8007D000(c, m);
        c.A0 = 0x00000002u;
        c.RA = 0x80011364u;
        GranTurismo2ArcadePC.func_8007D14C(c, m);
        c.S0 = c.SP + 0x10u;
        c.A0 = c.S0 + 0u;
        c.V0 = 0x00000018u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x154u), c.V0);
        c.V0 = 0x00000002u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x150u), 0u);
        MemoryAccess.WriteU32(m, (c.S0 + 0x18u), c.V0);
        c.RA = 0x80011384u;
        GranTurismo2ArcadePC.func_800832F8(c, m);
        c.RA = 0x8001138Cu;
        GranTurismo2ArcadePC.func_80010D78(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x00000002u;
        c.RA = 0x80011398u;
        GranTurismo2ArcadePC.func_80010F2C(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x16Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x168u));
        c.SP = c.SP + 0x170u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800113A8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x170u;
        c.A0 = c.SP + 0x10u;
        c.A1 = 0x0000001Au;
        c.A2 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x16Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x168u), c.S0);
        c.RA = 0x800113C4u;
        GranTurismo2ArcadePC.func_80010E64(c, m);
        c.RA = 0x800113CCu;
        GranTurismo2ArcadePC.func_80010DA8(c, m);
        c.A0 = 0xFFFFFFFFu;
        c.RA = 0x800113D4u;
        GranTurismo2ArcadePC.func_8007D14C(c, m);
        c.A0 = 0x00000013u;
        c.RA = 0x800113DCu;
        GranTurismo2ArcadePC.func_8007D000(c, m);
        c.A0 = 0x00000002u;
        c.RA = 0x800113E4u;
        GranTurismo2ArcadePC.func_8007D14C(c, m);
        c.S0 = c.SP + 0x10u;
        c.A0 = c.S0 + 0u;
        c.V0 = 0x00000008u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x154u), c.V0);
        c.V0 = 0x00000004u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x18u), c.V0);
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x150u), 0u);
        MemoryAccess.WriteU32(m, (c.S0 + 0xE0u), c.V0);
        c.RA = 0x8001140Cu;
        GranTurismo2ArcadePC.func_800832F8(c, m);
        c.RA = 0x80011414u;
        GranTurismo2ArcadePC.func_80010D78(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x00000002u;
        c.RA = 0x80011420u;
        GranTurismo2ArcadePC.func_80010F2C(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x16Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x168u));
        c.SP = c.SP + 0x170u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80011430(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x170u;
        c.A0 = c.SP + 0x10u;
        c.A1 = 0x00000019u;
        c.A2 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x16Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x168u), c.S0);
        c.RA = 0x8001144Cu;
        GranTurismo2ArcadePC.func_80010E64(c, m);
        c.RA = 0x80011454u;
        GranTurismo2ArcadePC.func_80010DA8(c, m);
        c.A0 = 0xFFFFFFFFu;
        c.RA = 0x8001145Cu;
        GranTurismo2ArcadePC.func_8007D14C(c, m);
        c.A0 = 0x00000033u;
        c.RA = 0x80011464u;
        GranTurismo2ArcadePC.func_8007D000(c, m);
        c.A0 = 0x00000002u;
        c.RA = 0x8001146Cu;
        GranTurismo2ArcadePC.func_8007D14C(c, m);
        c.S0 = c.SP + 0x10u;
        c.A0 = c.S0 + 0u;
        c.V0 = 0x0000000Cu;
        MemoryAccess.WriteU32(m, (c.S0 + 0x154u), c.V0);
        c.V0 = 0x00000004u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x18u), c.V0);
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x150u), 0u);
        MemoryAccess.WriteU32(m, (c.S0 + 0xE0u), c.V0);
        c.RA = 0x80011494u;
        GranTurismo2ArcadePC.func_800832F8(c, m);
        c.RA = 0x8001149Cu;
        GranTurismo2ArcadePC.func_80010D78(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x00000002u;
        c.RA = 0x800114A8u;
        GranTurismo2ArcadePC.func_80010F2C(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x16Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x168u));
        c.SP = c.SP + 0x170u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800114B8(CpuContext c, IMemory m)
    {
        RecompOne.Runtime.Sdk.GT2Compat.ReconcileArcadeOpeningMovieExtent(m);
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.RA = 0x800114C8u;
        GranTurismo2ArcadePC.func_80011328(c, m);
        c.A0 = 0x00000001u;
        c.RA = 0x800114D0u;
        GranTurismo2ArcadePC.func_8005D9AC(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800114E0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        if (c.A0 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
            goto L800114FC;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.RA = 0x800114F4u;
        GranTurismo2ArcadePC.func_800113A8(c, m);
        goto L80011504;
        L800114FC: ;
        c.RA = 0x80011504u;
        GranTurismo2ArcadePC.func_80011430(c, m);
        L80011504: ;
        c.A0 = 0x00000001u;
        c.RA = 0x8001150Cu;
        GranTurismo2ArcadePC.func_8005D9AC(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001151C(CpuContext c, IMemory m)
    {
        c.V0 = 0x80180000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 - 0x3920u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.V0 - 0x3920u;
        if (c.V1 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
            goto L80011558;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.RA = 0x80011540u;
        GranTurismo2ArcadePC.func_80080DAC(c, m);
        c.A1 = 0x80010000u;
        c.A2 = 0x801F0000u;
        c.A0 = c.S0 + 0u;
        c.A1 = c.A1 + 0x15BCu;
        c.A2 = c.A2 + 0x6E0u;
        c.RA = 0x80011558u;
        GranTurismo2ArcadePC.func_80085A4C(c, m);
        L80011558: ;
        c.V0 = c.S0 + 0u;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
}

public sealed class Gt2_arcade_overlay_5DispatchTable : IOverlay
{
    public string Name => "gt2_arcade_overlay_5";
    public int LbaStart => 469;
    public uint Base => 0x80010000u;
    public uint Size => 0x20E0u;
    public uint ImageSize => 0x27BDFFE0u;
    public bool Relocatable => false;
    public IReadOnlyDictionary<uint, Action<CpuContext, IMemory>> Functions { get; } =
        new Dictionary<uint, Action<CpuContext, IMemory>>
        {
            [0x80010000u] = GranTurismo2ArcadePC.func_80010000_gt2_arcade_overlay_5,
            [0x80010068u] = GranTurismo2ArcadePC.func_80010068,
            [0x80010078u] = GranTurismo2ArcadePC.func_80010078_gt2_arcade_overlay_5,
            [0x80010088u] = GranTurismo2ArcadePC.func_80010088_gt2_arcade_overlay_5,
            [0x80010094u] = GranTurismo2ArcadePC.func_80010094,
            [0x800100A4u] = GranTurismo2ArcadePC.func_800100A4,
            [0x800100F8u] = GranTurismo2ArcadePC.func_800100F8,
            [0x80010174u] = GranTurismo2ArcadePC.func_80010174,
            [0x800101B4u] = GranTurismo2ArcadePC.func_800101B4,
            [0x80010250u] = GranTurismo2ArcadePC.func_80010250,
            [0x80010288u] = GranTurismo2ArcadePC.func_80010288,
            [0x800102A8u] = GranTurismo2ArcadePC.func_800102A8,
            [0x80010318u] = GranTurismo2ArcadePC.func_80010318,
            [0x800103A8u] = GranTurismo2ArcadePC.func_800103A8,
            [0x800103F4u] = GranTurismo2ArcadePC.func_800103F4,
            [0x80010420u] = GranTurismo2ArcadePC.func_80010420_gt2_arcade_overlay_5,
            [0x80010450u] = GranTurismo2ArcadePC.func_80010450,
            [0x8001049Cu] = GranTurismo2ArcadePC.func_8001049C,
            [0x800104C4u] = GranTurismo2ArcadePC.func_800104C4,
            [0x800105ACu] = GranTurismo2ArcadePC.func_800105AC,
            [0x800105D4u] = GranTurismo2ArcadePC.func_800105D4,
            [0x80010610u] = GranTurismo2ArcadePC.func_80010610,
            [0x80010634u] = GranTurismo2ArcadePC.func_80010634,
            [0x800106A4u] = GranTurismo2ArcadePC.func_800106A4,
            [0x800106ECu] = GranTurismo2ArcadePC.func_800106EC,
            [0x80010894u] = GranTurismo2ArcadePC.func_80010894,
            [0x8001090Cu] = GranTurismo2ArcadePC.func_8001090C,
            [0x80010950u] = GranTurismo2ArcadePC.func_80010950,
            [0x80010994u] = GranTurismo2ArcadePC.func_80010994,
            [0x800109E4u] = GranTurismo2ArcadePC.func_800109E4,
            [0x80010A90u] = GranTurismo2ArcadePC.func_80010A90,
            [0x80010AC0u] = GranTurismo2ArcadePC.func_80010AC0,
            [0x80010D38u] = GranTurismo2ArcadePC.func_80010D38,
            [0x80010D78u] = GranTurismo2ArcadePC.func_80010D78,
            [0x80010DA8u] = GranTurismo2ArcadePC.func_80010DA8,
            [0x80010E2Cu] = GranTurismo2ArcadePC.func_80010E2C,
            [0x80010E64u] = GranTurismo2ArcadePC.func_80010E64,
            [0x80010F2Cu] = GranTurismo2ArcadePC.func_80010F2C,
            [0x80010F78u] = GranTurismo2ArcadePC.func_80010F78,
            [0x8001102Cu] = GranTurismo2ArcadePC.func_8001102C,
            [0x800111CCu] = GranTurismo2ArcadePC.func_800111CC,
            [0x800111D4u] = GranTurismo2ArcadePC.func_800111D4,
            [0x80011290u] = GranTurismo2ArcadePC.func_80011290,
            [0x800112D4u] = GranTurismo2ArcadePC.func_800112D4,
            [0x800112DCu] = GranTurismo2ArcadePC.func_800112DC,
            [0x800112E4u] = GranTurismo2ArcadePC.func_800112E4,
            [0x800112ECu] = GranTurismo2ArcadePC.func_800112EC,
            [0x800112F4u] = GranTurismo2ArcadePC.func_800112F4,
            [0x80011314u] = GranTurismo2ArcadePC.func_80011314,
            [0x80011328u] = GranTurismo2ArcadePC.func_80011328,
            [0x800113A8u] = GranTurismo2ArcadePC.func_800113A8,
            [0x80011430u] = GranTurismo2ArcadePC.func_80011430,
            [0x800114B8u] = GranTurismo2ArcadePC.func_800114B8,
            [0x800114E0u] = GranTurismo2ArcadePC.func_800114E0,
            [0x8001151Cu] = GranTurismo2ArcadePC.func_8001151C,
        };
}
