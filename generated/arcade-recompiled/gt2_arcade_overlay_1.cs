using RecompOne.Runtime.Context;
using RecompOne.Runtime.Dispatch;
using RecompOne.Runtime.Memory;

namespace Recompiled.Arcade;

public static partial class GranTurismo2ArcadePC
{
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010000_gt2_arcade_overlay_1(CpuContext c, IMemory m)
    {
        c.V0 = (int)c.A1 < (int)c.A0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = (int)c.A0 < (int)c.A2 ? 1u : 0u;
            goto L8001001C;
        }
        c.V0 = (int)c.A0 < (int)c.A2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.A0 + 0u;
            goto L80010034;
        }
        c.V0 = c.A0 + 0u;
        c.V0 = c.A2 + 0u;
        return;
        L8001001C: ;
        c.V0 = c.A2 + 0u;
        c.V1 = (int)c.A1 < (int)c.V0 ? 1u : 0u;
        if (c.V1 != 0u) {
            goto L80010034;
        }
        c.V0 = c.A1 + 0u;
        return;
        L80010034: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001003C_gt2_arcade_overlay_1(CpuContext c, IMemory m)
    {
        c.V0 = (int)c.A0 < (int)c.A1 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = (int)c.A2 < (int)c.A0 ? 1u : 0u;
            goto L80010058;
        }
        c.V0 = (int)c.A2 < (int)c.A0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.A0 + 0u;
            goto L80010070;
        }
        c.V0 = c.A0 + 0u;
        c.V0 = c.A2 + 0u;
        return;
        L80010058: ;
        c.V0 = c.A2 + 0u;
        c.V1 = (int)c.V0 < (int)c.A1 ? 1u : 0u;
        if (c.V1 != 0u) {
            goto L80010070;
        }
        c.V0 = c.A1 + 0u;
        return;
        L80010070: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010078_gt2_arcade_overlay_1(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        c.V1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        c.S4 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = c.V1 & 0x001Fu;
        c.A0 = c.S3 + 0u;
        c.V0 = c.V1 & 0x03E0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.V0 >> 5;
        c.A1 = c.S1 + 0u;
        c.V1 = c.V1 & 0x7C00u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.V1 >> 10;
        c.A2 = c.S2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.RA = 0x800100C0u;
        Dispatcher.Call(c, m, 0x80010000u);
        c.A0 = c.S3 + 0u;
        c.A1 = c.S1 + 0u;
        c.A2 = c.S2 + 0u;
        c.S0 = c.V0 + 0u;
        c.RA = 0x800100D4u;
        Dispatcher.Call(c, m, 0x8001003Cu);
        if (c.S0 != 0u) {
            c.V1 = c.V0 + 0u;
            goto L800100E4;
        }
        c.V1 = c.V0 + 0u;
        c.A2 = 0u + 0u;
        goto L800100F4;
        L800100E4: ;
        c.V0 = c.S0 - c.V1;
        c.V0 = c.V0 << 5;
        if (c.S0 != 0u) { if ((int)c.V0 == int.MinValue && (int)c.S0 == -1) { c.LO = 0x80000000u; c.HI = 0u; } else { c.LO = (uint)((int)c.V0 / (int)c.S0); c.HI = (uint)((int)c.V0 % (int)c.S0); } }
        c.A2 = c.LO;
        L800100F4: ;
        if (c.A2 == 0u) {
            c.A0 = 0u + 0u;
            goto L80010194;
        }
        c.A0 = 0u + 0u;
        c.V0 = c.S0 - c.S3;
        c.V0 = c.V0 << 5;
        c.V1 = c.S0 - c.V1;
        if (c.V1 != 0u) { if ((int)c.V0 == int.MinValue && (int)c.V1 == -1) { c.LO = 0x80000000u; c.HI = 0u; } else { c.LO = (uint)((int)c.V0 / (int)c.V1); c.HI = (uint)((int)c.V0 % (int)c.V1); } }
        c.A3 = c.LO;
        c.V0 = c.S0 - c.S1;
        c.V0 = c.V0 << 5;
        if (c.V1 != 0u) { if ((int)c.V0 == int.MinValue && (int)c.V1 == -1) { c.LO = 0x80000000u; c.HI = 0u; } else { c.LO = (uint)((int)c.V0 / (int)c.V1); c.HI = (uint)((int)c.V0 % (int)c.V1); } }
        c.A1 = c.LO;
        c.V0 = c.S0 - c.S2;
        c.V0 = c.V0 << 5;
        if (c.V1 != 0u) { if ((int)c.V0 == int.MinValue && (int)c.V1 == -1) { c.LO = 0x80000000u; c.HI = 0u; } else { c.LO = (uint)((int)c.V0 / (int)c.V1); c.HI = (uint)((int)c.V0 % (int)c.V1); } }
        c.V1 = c.LO;
        if (c.S3 != c.S0) {
            goto L8001013C;
        }
        c.A0 = c.V1 - c.A1;
        L8001013C: ;
        if (c.S1 != c.S0) {
            c.V0 = c.V1 - 0x40u;
            goto L80010148;
        }
        c.V0 = c.V1 - 0x40u;
        c.A0 = c.A3 - c.V0;
        L80010148: ;
        if (c.S2 != c.S0) {
            c.V0 = c.A0 << 4;
            goto L8001015C;
        }
        c.V0 = c.A0 << 4;
        c.V0 = c.A3 - 0x80u;
        c.A0 = c.A1 - c.V0;
        c.V0 = c.A0 << 4;
        L8001015C: ;
        c.V0 = c.V0 - c.A0;
        c.A0 = c.V0 << 2;
        if ((int)c.A0 >= 0) {
            c.V0 = (int)c.A0 < 11520 ? 1u : 0u;
            goto L80010188;
        }
        c.V0 = (int)c.A0 < 11520 ? 1u : 0u;
        c.A0 = c.A0 + 0x2D00u;
        L80010170: ;
        if ((int)c.A0 < 0) {
            c.A0 = c.A0 + 0x2D00u;
            goto L80010170;
        }
        c.A0 = c.A0 + 0x2D00u;
        c.A0 = c.A0 - 0x2D00u;
        c.V0 = (int)c.A0 < 11520 ? 1u : 0u;
        goto L80010188;
        L80010184: ;
        c.V0 = (int)c.A0 < 11520 ? 1u : 0u;
        L80010188: ;
        if (c.V0 == 0u) {
            c.A0 = c.A0 - 0x2D00u;
            goto L80010184;
        }
        c.A0 = c.A0 - 0x2D00u;
        c.A0 = c.A0 + 0x2D00u;
        L80010194: ;
        c.V0 = (uint)((int)c.A0 >> 5);
        MemoryAccess.WriteU16(m, c.S4, (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.S4 + 0x2u), (ushort)c.A2);
        MemoryAccess.WriteU16(m, (c.S4 + 0x4u), (ushort)c.S0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800101C4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x50u;
        c.V0 = 0x80090000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x2570u));
        c.V1 = 0x00000002u;
        MemoryAccess.WriteU32(m, (c.SP + 0x4Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x48u), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x3Cu), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x38u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x34u), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x50u), c.A0);
        MemoryAccess.WriteU32(m, (c.SP + 0x58u), c.A2);
        if (c.V0 != c.V1) {
            MemoryAccess.WriteU32(m, (c.SP + 0x5Cu), c.A3);
            goto L8001021C;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x5Cu), c.A3);
        c.V0 = 0x80090000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x2568u));
        c.V0 = c.V0 + 0x10u;
        goto L80010220;
        L8001021C: ;
        c.V0 = c.A1 + 0x100u;
        L80010220: ;
        c.S7 = 0x00000040u;
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0x58u));
        c.S4 = c.V0 + 0u;
        c.RA = 0x80010230u;
        GranTurismo2ArcadePC.func_80078048(c, m);
        c.S6 = c.V0 + 0u;
        if ((int)c.S6 > 0) {
            c.V0 = 0u + 0u;
            goto L80010248;
        }
        c.V0 = 0u + 0u;
        goto L80010404;
        L80010244: ;
        c.S7 = c.S7 - 0x1u;
        L80010248: ;
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0x5Cu));
        c.RA = 0x80010254u;
        GranTurismo2ArcadePC.func_800839F0(c, m);
        if (c.S6 != 0u) { c.LO = c.V0 / c.S6; c.HI = c.V0 % c.S6; }
        c.S0 = c.HI;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x58u));
        c.V0 = c.S0 << 2;
        c.V0 = c.T0 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.A0 = c.S4 + 0u;
        c.T0 = c.V0 >> 26;
        c.S5 = c.V0 + 0u;
        c.S0 = c.S5 & 0xFFFFu;
        c.A1 = c.S0 - 0x1u;
        MemoryAccess.WriteU16(m, (c.SP + 0x20u), (ushort)c.T0);
        c.RA = 0x80010288u;
        GranTurismo2ArcadePC.func_80077F48(c, m);
        c.A0 = MemoryAccess.ReadU32(m, c.V0);
        c.RA = 0x80010294u;
        GranTurismo2ArcadePC.func_80060A80(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 != 0u) {
            c.S2 = 0u + 0u;
            goto L80010248;
        }
        c.S2 = 0u + 0u;
        c.S3 = c.S0 + 0u;
        c.FP = (int)c.S2 < (int)c.S7 ? 1u : 0u;
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x60u));
        L800102AC: ;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x64u));
        c.V0 = (int)c.S2 < (int)c.T0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A0 = c.S4 + 0u;
            goto L80010320;
        }
        c.A0 = c.S4 + 0u;
        c.S0 = MemoryAccess.ReadU16(m, (c.S1 + 0x8u));
        c.A1 = c.S3 - 0x1u;
        c.S0 = c.S0 ^ c.S3;
        c.S0 = c.S0 < 0x00000001u ? 1u : 0u;
        c.RA = 0x800102D4u;
        GranTurismo2ArcadePC.func_80077F48(c, m);
        c.V1 = MemoryAccess.ReadU32(m, c.V0);
        c.V0 = MemoryAccess.ReadU32(m, c.S1);
        c.V0 = c.V0 ^ c.V1;
        c.V0 = c.V0 < 0x00000001u ? 1u : 0u;
        c.S0 = c.S0 | c.V0;
        if (c.S0 == 0u) {
            goto L80010314;
        }
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0x5Cu));
        c.RA = 0x80010300u;
        GranTurismo2ArcadePC.func_800839F0(c, m);
        c.V0 = c.V0 & 0x001Fu;
        c.V0 = (int)c.V0 < 29 ? 1u : 0u;
        c.V0 = c.V0 & c.FP;
        if (c.V0 != 0u) {
            goto L80010244;
        }
        L80010314: ;
        c.S1 = c.S1 + 0xCu;
        c.S2 = c.S2 + 0x1u;
        goto L800102AC;
        L80010320: ;
        c.A1 = c.S5 & 0xFFFFu;
        c.A1 = c.A1 - 0x1u;
        c.RA = 0x8001032Cu;
        GranTurismo2ArcadePC.func_80077F48(c, m);
        c.V1 = MemoryAccess.ReadU16(m, (c.SP + 0x20u));
        c.S4 = MemoryAccess.ReadU32(m, c.V0);
        if (c.V1 == 0u) {
            c.V0 = 0x80090000u;
            goto L80010364;
        }
        c.V0 = 0x80090000u;
        c.V0 = c.V0 + 0x1318u;
        c.V0 = c.V1 + c.V0;
        c.A1 = MemoryAccess.ReadU8(m, c.V0);
        c.A0 = c.S4 + 0u;
        c.S0 = c.A1 + 0u;
        c.A1 = c.A1 << 24;
        c.A1 = (uint)((int)c.A1 >> 24);
        c.RA = 0x8001035Cu;
        GranTurismo2ArcadePC.func_80060C38(c, m);
        c.V1 = c.V0 + 0u;
        goto L800103E0;
        L80010364: ;
        c.A0 = c.S4 + 0u;
        c.A1 = c.SP + 0x18u;
        c.A2 = c.SP + 0x1Cu;
        c.RA = 0x80010374u;
        GranTurismo2ArcadePC.func_80060AFC(c, m);
        c.S3 = c.V0 + 0u;
        c.S2 = 0u + 0u;
        L8001037C: ;
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0x5Cu));
        c.RA = 0x80010388u;
        GranTurismo2ArcadePC.func_800839F0(c, m);
        if (c.S3 != 0u) { c.LO = c.V0 / c.S3; c.HI = c.V0 % c.S3; }
        c.S0 = c.HI;
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = c.S0 << 1;
        c.V0 = c.S1 + c.V0;
        c.A0 = MemoryAccess.ReadU16(m, c.V0);
        c.A1 = c.SP + 0x10u;
        c.RA = 0x800103A8u;
        Dispatcher.Call(c, m, 0x80010078u);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.SP + 0x12u));
        c.V0 = (int)c.V0 < 6 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.S2 = c.S2 + 0x1u;
            goto L800103C8;
        }
        c.S2 = c.S2 + 0x1u;
        c.V0 = (int)c.S2 < 2 ? 1u : 0u;
        if (c.V0 != 0u) {
            goto L8001037C;
        }
        L800103C8: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.V1 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.V0 = c.V0 + c.S0;
        c.V1 = c.S1 + c.V1;
        c.S0 = MemoryAccess.ReadU8(m, c.V0);
        c.V1 = MemoryAccess.ReadU16(m, c.V1);
        L800103E0: ;
        c.T0 = MemoryAccess.ReadU16(m, (c.SP + 0x20u));
        c.V0 = c.T0 + 0u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x50u));
        MemoryAccess.WriteU32(m, c.T0, c.S4);
        MemoryAccess.WriteU8(m, (c.T0 + 0x5u), (byte)c.S0);
        MemoryAccess.WriteU16(m, (c.T0 + 0x6u), (ushort)c.V1);
        MemoryAccess.WriteU16(m, (c.T0 + 0x8u), (ushort)c.S5);
        L80010404: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x4Cu));
        c.FP = MemoryAccess.ReadU32(m, (c.SP + 0x48u));
        c.S7 = MemoryAccess.ReadU32(m, (c.SP + 0x44u));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x40u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x3Cu));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x38u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x34u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x30u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x2Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.SP = c.SP + 0x50u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010434(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S1);
        c.S1 = c.A0 + 0u;
        c.A0 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S0);
        c.S0 = c.A3 + 0u;
        if (c.A2 == 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
            goto L80010484;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        c.V0 = 0x80090000u;
        c.V0 = c.V0 + 0x1318u;
        c.V1 = c.A2 & 0x003Fu;
        c.V1 = c.V1 + c.V0;
        c.A1 = MemoryAccess.ReadU8(m, c.V1);
        c.S0 = c.A1 + 0u;
        c.A1 = c.A1 << 24;
        c.A1 = (uint)((int)c.A1 >> 24);
        c.RA = 0x8001047Cu;
        GranTurismo2ArcadePC.func_80060C38(c, m);
        MemoryAccess.WriteU8(m, (c.S1 + 0x5u), (byte)c.S0);
        goto L800104C8;
        L80010484: ;
        c.A1 = c.SP + 0x10u;
        c.A2 = c.SP + 0x14u;
        c.RA = 0x80010490u;
        GranTurismo2ArcadePC.func_80060AFC(c, m);
        c.A0 = c.S0 + 0u;
        c.S0 = c.V0 + 0u;
        c.RA = 0x8001049Cu;
        GranTurismo2ArcadePC.func_800839F0(c, m);
        if (c.S0 != 0u) { c.LO = c.V0 / c.S0; c.HI = c.V0 % c.S0; }
        c.V1 = c.HI;
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.V0 = c.V0 + c.V1;
        c.S0 = MemoryAccess.ReadU8(m, c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V1 = c.V1 << 1;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU16(m, c.V1);
        MemoryAccess.WriteU8(m, (c.S1 + 0x5u), (byte)c.S0);
        L800104C8: ;
        MemoryAccess.WriteU16(m, (c.S1 + 0x6u), (ushort)c.V0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800104E0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0xA0u;
        c.V1 = c.A0 + 0u;
        c.V0 = 0x80090000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x2B64u));
        c.V0 = 0x801D0000u;
        c.V0 = c.V0 - 0x6CC0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x78u), c.S0);
        c.S0 = c.V0 + 0u;
        c.V0 = 0u | 0xBF7Cu;
        MemoryAccess.WriteU32(m, (c.SP + 0x8Cu), c.S5);
        c.S5 = c.S0 + c.V0;
        c.V0 = 0x80090000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x2B68u));
        MemoryAccess.WriteU32(m, (c.SP + 0x98u), c.FP);
        c.FP = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x9Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x94u), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x90u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x88u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x84u), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x80u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x7Cu), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0xA4u), c.A1);
        if (c.A1 == 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0xACu), c.A3);
            goto L80010548;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0xACu), c.A3);
        c.A0 = c.V0 + 0u;
        L80010548: ;
        if (c.V1 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x64u), c.A0);
            goto L80010898;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x64u), c.A0);
        c.A0 = 0u + 0u;
        MemoryAccess.WriteU16(m, (c.S5 + 0x58u), (ushort)0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x68u), c.FP);
        c.RA = 0x80010560u;
        GranTurismo2ArcadePC.func_8007D14C(c, m);
        c.A0 = c.S5 + 0u;
        c.A1 = 0u + 0u;
        c.A2 = 0x0000058Cu;
        MemoryAccess.WriteU32(m, (c.SP + 0x60u), c.V0);
        c.RA = 0x80010574u;
        GranTurismo2ArcadePC.func_8008CD40(c, m);
        c.V0 = 0x00000002u;
        c.T0 = MemoryAccess.ReadWordLeft(m, c.T0, (c.S0 + 0x4u));
        c.T0 = MemoryAccess.ReadWordRight(m, c.T0, (c.S0 + 0x1u));
        c.T1 = MemoryAccess.ReadWordLeft(m, c.T1, (c.S0 + 0x8u));
        c.T1 = MemoryAccess.ReadWordRight(m, c.T1, (c.S0 + 0x5u));
        MemoryAccess.WriteWordLeft(m, (c.S5 + 0x3u), c.T0);
        MemoryAccess.WriteWordRight(m, c.S5, c.T0);
        MemoryAccess.WriteWordLeft(m, (c.S5 + 0x7u), c.T1);
        MemoryAccess.WriteWordRight(m, (c.S5 + 0x4u), c.T1);
        MemoryAccess.WriteU8(m, (c.S5 + 0x8u), (byte)c.V0);
        MemoryAccess.WriteU8(m, (c.S5 + 0x9u), (byte)c.V0);
        c.V0 = 0x00000004u;
        MemoryAccess.WriteU8(m, (c.S5 + 0xAu), (byte)c.V0);
        c.V0 = 0x00000005u;
        MemoryAccess.WriteU8(m, (c.S5 + 0xBu), (byte)c.V0);
        MemoryAccess.WriteU8(m, (c.S5 + 0xCu), (byte)0u);
        c.V0 = MemoryAccess.ReadU8(m, (c.FP + 0x44u));
        MemoryAccess.WriteU8(m, (c.S5 + 0xEu), (byte)0u);
        c.V0 = c.V0 < 0x00000001u ? 1u : 0u;
        MemoryAccess.WriteU8(m, (c.S5 + 0xDu), (byte)c.V0);
        c.V0 = MemoryAccess.ReadU8(m, (c.FP + 0x45u));
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0x64u));
        MemoryAccess.WriteU8(m, (c.S5 + 0xFu), (byte)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, c.FP);
        c.S4 = 0u + 0u;
        c.RA = 0x800105DCu;
        GranTurismo2ArcadePC.func_8007807C(c, m);
        c.A0 = c.S5 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x800105E8u;
        GranTurismo2ArcadePC.func_8005E458(c, m);
        c.A1 = MemoryAccess.ReadU16(m, (c.FP + 0x2u));
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0x64u));
        c.RA = 0x800105F8u;
        GranTurismo2ArcadePC.func_8007807C(c, m);
        c.A0 = c.S5 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x80010604u;
        GranTurismo2ArcadePC.func_8005E500(c, m);
        c.A1 = MemoryAccess.ReadU16(m, (c.FP + 0x94u));
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0x64u));
        c.RA = 0x80010614u;
        GranTurismo2ArcadePC.func_8007807C(c, m);
        c.A0 = c.S5 + 0x44u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x80010620u;
        GranTurismo2ArcadePC.func_8008CDEC(c, m);
        c.V1 = MemoryAccess.ReadU32(m, (c.S5 + 0x588u));
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU16(m, (c.S5 + 0x57Cu), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.S5 + 0x582u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.S5 + 0x584u), (ushort)c.V0);
        c.V0 = 0xFFFFFFF9u;
        MemoryAccess.WriteU16(m, (c.S5 + 0x586u), (ushort)0u);
        MemoryAccess.WriteU8(m, (c.S5 + 0x580u), (byte)0u);
        c.V1 = c.V1 | 0x0001u;
        c.V1 = c.V1 & c.V0;
        MemoryAccess.WriteU32(m, (c.S5 + 0x588u), c.V1);
        c.T4 = MemoryAccess.ReadU32(m, (c.SP + 0xB0u));
        if (c.T4 == 0u) {
            goto L8001067C;
        }
        c.S4 = 0x00000001u;
        c.T0 = MemoryAccess.ReadU32(m, c.T4);
        c.T1 = MemoryAccess.ReadU32(m, (c.T4 + 0x4u));
        c.T2 = MemoryAccess.ReadU32(m, (c.T4 + 0x8u));
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.T0);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.T1);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.T2);
        MemoryAccess.WriteU16(m, (c.SP + 0x20u), (ushort)0u);
        L8001067C: ;
        c.T4 = MemoryAccess.ReadU32(m, (c.SP + 0xB8u));
        if (c.T4 == 0u) {
            c.V1 = c.S4 << 1;
            goto L800106BC;
        }
        c.V1 = c.S4 << 1;
        c.V1 = c.V1 + c.S4;
        c.S4 = c.S4 + 0x1u;
        c.V1 = c.V1 << 2;
        c.V0 = c.SP + 0x18u;
        c.V0 = c.V0 + c.V1;
        c.T0 = MemoryAccess.ReadU32(m, c.T4);
        c.T1 = MemoryAccess.ReadU32(m, (c.T4 + 0x4u));
        c.T2 = MemoryAccess.ReadU32(m, (c.T4 + 0x8u));
        MemoryAccess.WriteU32(m, c.V0, c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x4u), c.T1);
        MemoryAccess.WriteU32(m, (c.V0 + 0x8u), c.T2);
        MemoryAccess.WriteU16(m, (c.V0 + 0x8u), (ushort)0u);
        L800106BC: ;
        c.A0 = c.FP + 0u;
        c.RA = 0x800106C4u;
        GranTurismo2ArcadePC.func_80078048(c, m);
        c.S6 = 0x00000006u;
        c.S3 = 0u + 0u;
        c.T4 = c.SP + 0x18u;
        c.S1 = c.T4 + 0u;
        c.T0 = 0x0000005Cu;
        MemoryAccess.WriteU8(m, (c.S5 + 0x5Au), (byte)c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x6Cu), c.T4);
        MemoryAccess.WriteU32(m, (c.SP + 0x70u), c.T0);
        c.V0 = (int)c.S3 < (int)c.S6 ? 1u : 0u;
        L800106E8: ;
        if (c.V0 == 0u) {
            c.S7 = 0u + 0u;
            goto L800108A8;
        }
        c.S7 = 0u + 0u;
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x70u));
        c.S2 = 0x00000001u;
        if (c.S3 == 0u) {
            c.S0 = c.S5 + c.T1;
            goto L80010710;
        }
        c.S0 = c.S5 + c.T1;
        if (c.S3 == c.S2) {
            c.A0 = c.S0 + 0u;
            goto L80010728;
        }
        c.A0 = c.S0 + 0u;
        c.A1 = 0u + 0u;
        goto L80010744;
        L80010710: ;
        c.T2 = MemoryAccess.ReadU32(m, (c.SP + 0xB0u));
        if (c.T2 == 0u) {
            c.A0 = c.S0 + 0u;
            goto L80010740;
        }
        c.A0 = c.S0 + 0u;
        c.S2 = 0x00000003u;
        goto L80010740;
        L80010728: ;
        c.T3 = MemoryAccess.ReadU32(m, (c.SP + 0xB8u));
        if (c.T3 == 0u) {
            c.A1 = 0u + 0u;
            goto L80010744;
        }
        c.A1 = 0u + 0u;
        c.S2 = 0x00000004u;
        c.A0 = c.S0 + 0u;
        L80010740: ;
        c.A1 = 0u + 0u;
        L80010744: ;
        c.A2 = 0x000000D0u;
        c.RA = 0x8001074Cu;
        GranTurismo2ArcadePC.func_8008CD40(c, m);
        c.V0 = (int)c.S3 < (int)c.S4 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.A0 = c.S1 + 0u;
            goto L80010780;
        }
        c.A0 = c.S1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S4);
        c.S4 = c.S4 + 0x1u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0x64u));
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0x68u));
        c.T4 = MemoryAccess.ReadU32(m, (c.SP + 0x6Cu));
        c.A3 = c.SP + 0x60u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T4);
        c.RA = 0x80010778u;
        GranTurismo2ArcadePC.func_800101C4(c, m);
        c.S7 = c.V0 + 0u;
        MemoryAccess.WriteU8(m, (c.S1 + 0x4u), (byte)0u);
        L80010780: ;
        c.V0 = MemoryAccess.ReadU32(m, c.S1);
        MemoryAccess.WriteU32(m, c.S0, c.V0);
        c.V1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0x5u));
        c.T0 = 0x00000001u;
        c.V0 = c.S3 + 0x1u;
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0xB4u));
        c.V0 = c.S6 - c.V0;
        MemoryAccess.WriteU8(m, (c.S0 + 0x8Du), (byte)c.V0);
        c.V0 = (int)c.S2 < 2 ? 1u : 0u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x8Cu), (byte)c.T0);
        if (c.V0 != 0u) {
            MemoryAccess.WriteU32(m, (c.S0 + 0x4u), c.V1);
            goto L800107E4;
        }
        MemoryAccess.WriteU32(m, (c.S0 + 0x4u), c.V1);
        c.V0 = (int)c.S2 < 4 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = 0x00000004u;
            goto L800107D0;
        }
        c.V0 = 0x00000004u;
        if (c.S2 == c.V0) {
            goto L800107D4;
        }
        goto L800107E4;
        L800107D0: ;
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0xACu));
        L800107D4: ;
        c.A1 = c.S0 + 0x8u;
        c.RA = 0x800107DCu;
        GranTurismo2ArcadePC.func_80076ED0(c, m);
        goto L8001083C;
        L800107E4: ;
        c.A0 = MemoryAccess.ReadU16(m, (c.S1 + 0x8u));
        c.RA = 0x800107F0u;
        GranTurismo2ArcadePC.func_800767D0(c, m);
        c.A0 = c.V0 + 0u;
        c.A1 = c.S0 + 0x8u;
        c.RA = 0x800107FCu;
        GranTurismo2ArcadePC.func_80076E6C(c, m);
        c.A1 = MemoryAccess.ReadU16(m, (c.S0 + 0x24u));
        c.A0 = 0x00000005u;
        c.RA = 0x80010808u;
        GranTurismo2ArcadePC.func_80076E3C(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = MemoryAccess.ReadU8(m, (c.V1 + 0xEu));
        if (c.V0 == 0u) {
            c.A0 = c.S1 + 0u;
            goto L8001083C;
        }
        c.A0 = c.S1 + 0u;
        c.A2 = c.S7 + 0u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.A3 = c.SP + 0x60u;
        MemoryAccess.WriteU32(m, c.S0, c.A1);
        c.RA = 0x80010830u;
        GranTurismo2ArcadePC.func_80010434(c, m);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0x5u));
        MemoryAccess.WriteU32(m, (c.S0 + 0x4u), c.V0);
        L8001083C: ;
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0xA4u));
        if (c.T1 == 0u) {
            c.V1 = c.S0 + 0x8u;
            goto L8001085C;
        }
        c.V1 = c.S0 + 0x8u;
        c.V0 = MemoryAccess.ReadU8(m, (c.V1 + 0x7Au));
        c.V0 = c.V0 | 0x0040u;
        MemoryAccess.WriteU8(m, (c.V1 + 0x7Au), (byte)c.V0);
        L8001085C: ;
        MemoryAccess.WriteU8(m, (c.S0 + 0x8Eu), (byte)c.S2);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0x4u));
        c.T2 = MemoryAccess.ReadU32(m, (c.SP + 0x70u));
        c.S1 = c.S1 + 0xCu;
        c.T2 = c.T2 + 0xD0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x70u), c.T2);
        c.A0 = MemoryAccess.ReadU32(m, c.S0);
        c.S3 = c.S3 + 0x1u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x8Fu), (byte)c.V0);
        c.RA = 0x80010884u;
        GranTurismo2ArcadePC.func_800609F8(c, m);
        c.A0 = c.S0 + 0x90u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x80010890u;
        GranTurismo2ArcadePC.func_8008CDEC(c, m);
        c.V0 = (int)c.S3 < (int)c.S6 ? 1u : 0u;
        goto L800106E8;
        L80010898: ;
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0x64u));
        c.A1 = c.S5 + 0x10u;
        c.RA = 0x800108A4u;
        GranTurismo2ArcadePC.func_8007821C(c, m);
        c.FP = c.V0 + 0u;
        L800108A8: ;
        c.V0 = 0x801D0000u;
        c.A1 = c.V0 - 0x6D00u;
        c.V1 = c.FP + 0x44u;
        c.V0 = c.V1 | c.A1;
        c.V0 = c.V0 & 0x0003u;
        if (c.V0 == 0u) {
            c.V0 = c.FP + 0x84u;
            goto L80010918;
        }
        c.V0 = c.FP + 0x84u;
        L800108C4: ;
        c.T3 = MemoryAccess.ReadWordLeft(m, c.T3, (c.V1 + 0x3u));
        c.T3 = MemoryAccess.ReadWordRight(m, c.T3, c.V1);
        c.T4 = MemoryAccess.ReadWordLeft(m, c.T4, (c.V1 + 0x7u));
        c.T4 = MemoryAccess.ReadWordRight(m, c.T4, (c.V1 + 0x4u));
        c.T0 = MemoryAccess.ReadWordLeft(m, c.T0, (c.V1 + 0xBu));
        c.T0 = MemoryAccess.ReadWordRight(m, c.T0, (c.V1 + 0x8u));
        c.T1 = MemoryAccess.ReadWordLeft(m, c.T1, (c.V1 + 0xFu));
        c.T1 = MemoryAccess.ReadWordRight(m, c.T1, (c.V1 + 0xCu));
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0x3u), c.T3);
        MemoryAccess.WriteWordRight(m, c.A1, c.T3);
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0x7u), c.T4);
        MemoryAccess.WriteWordRight(m, (c.A1 + 0x4u), c.T4);
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0xBu), c.T0);
        MemoryAccess.WriteWordRight(m, (c.A1 + 0x8u), c.T0);
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0xFu), c.T1);
        MemoryAccess.WriteWordRight(m, (c.A1 + 0xCu), c.T1);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.V0) {
            c.A1 = c.A1 + 0x10u;
            goto L800108C4;
        }
        c.A1 = c.A1 + 0x10u;
        c.S2 = 0u + 0u;
        goto L80010948;
        L80010918: ;
        c.T2 = MemoryAccess.ReadU32(m, c.V1);
        c.T3 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T4 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, c.A1, c.T2);
        MemoryAccess.WriteU32(m, (c.A1 + 0x4u), c.T3);
        MemoryAccess.WriteU32(m, (c.A1 + 0x8u), c.T4);
        MemoryAccess.WriteU32(m, (c.A1 + 0xCu), c.T0);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.V0) {
            c.A1 = c.A1 + 0x10u;
            goto L80010918;
        }
        c.A1 = c.A1 + 0x10u;
        c.S2 = 0u + 0u;
        L80010948: ;
        c.V0 = 0x801D0000u;
        c.S3 = c.V0 - 0x6CC0u;
        c.S1 = 0x00010000u;
        c.S1 = c.S1 | 0x4FDAu;
        c.S0 = 0x0000005Cu;
        L8001095C: ;
        c.V0 = MemoryAccess.ReadU8(m, (c.S5 + 0x5Au));
        c.V0 = (int)c.S2 < (int)c.V0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A0 = c.S5 + c.S0;
            goto L8001098C;
        }
        c.A0 = c.S5 + c.S0;
        c.A0 = c.A0 + 0x8u;
        c.A1 = c.S1 + c.S3;
        c.RA = 0x8001097Cu;
        GranTurismo2ArcadePC.func_800770BC(c, m);
        c.S1 = c.S1 + 0x1C0u;
        c.S0 = c.S0 + 0xD0u;
        c.S2 = c.S2 + 0x1u;
        goto L8001095C;
        L8001098C: ;
        c.V0 = c.S5 + 0u;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x9Cu));
        c.FP = MemoryAccess.ReadU32(m, (c.SP + 0x98u));
        c.S7 = MemoryAccess.ReadU32(m, (c.SP + 0x94u));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x90u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x8Cu));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x88u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x84u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x80u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x7Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x78u));
        c.SP = c.SP + 0xA0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800109C0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        c.V0 = 0x80090000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x2B68u));
        c.V0 = 0x801D0000u;
        c.V0 = c.V0 - 0x6CC0u;
        c.V1 = 0u | 0xBF7Cu;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = c.V0 + c.V1;
        c.A1 = c.S3 + 0x10u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.RA = 0x800109FCu;
        GranTurismo2ArcadePC.func_8007821C(c, m);
        c.V1 = 0x801D0000u;
        c.V1 = c.V1 - 0x6D00u;
        c.A1 = c.V0 + 0u;
        c.A0 = c.A1 + 0x44u;
        c.V0 = c.A0 | c.V1;
        c.V0 = c.V0 & 0x0003u;
        if (c.V0 == 0u) {
            c.V0 = c.A1 + 0x84u;
            goto L80010A70;
        }
        c.V0 = c.A1 + 0x84u;
        L80010A1C: ;
        c.A2 = MemoryAccess.ReadWordLeft(m, c.A2, (c.A0 + 0x3u));
        c.A2 = MemoryAccess.ReadWordRight(m, c.A2, c.A0);
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.A0 + 0x7u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, (c.A0 + 0x4u));
        c.T0 = MemoryAccess.ReadWordLeft(m, c.T0, (c.A0 + 0xBu));
        c.T0 = MemoryAccess.ReadWordRight(m, c.T0, (c.A0 + 0x8u));
        c.T1 = MemoryAccess.ReadWordLeft(m, c.T1, (c.A0 + 0xFu));
        c.T1 = MemoryAccess.ReadWordRight(m, c.T1, (c.A0 + 0xCu));
        MemoryAccess.WriteWordLeft(m, (c.V1 + 0x3u), c.A2);
        MemoryAccess.WriteWordRight(m, c.V1, c.A2);
        MemoryAccess.WriteWordLeft(m, (c.V1 + 0x7u), c.A3);
        MemoryAccess.WriteWordRight(m, (c.V1 + 0x4u), c.A3);
        MemoryAccess.WriteWordLeft(m, (c.V1 + 0xBu), c.T0);
        MemoryAccess.WriteWordRight(m, (c.V1 + 0x8u), c.T0);
        MemoryAccess.WriteWordLeft(m, (c.V1 + 0xFu), c.T1);
        MemoryAccess.WriteWordRight(m, (c.V1 + 0xCu), c.T1);
        c.A0 = c.A0 + 0x10u;
        if (c.A0 != c.V0) {
            c.V1 = c.V1 + 0x10u;
            goto L80010A1C;
        }
        c.V1 = c.V1 + 0x10u;
        c.S2 = 0u + 0u;
        goto L80010AA0;
        L80010A70: ;
        c.A2 = MemoryAccess.ReadU32(m, c.A0);
        c.A3 = MemoryAccess.ReadU32(m, (c.A0 + 0x4u));
        c.T0 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.T1 = MemoryAccess.ReadU32(m, (c.A0 + 0xCu));
        MemoryAccess.WriteU32(m, c.V1, c.A2);
        MemoryAccess.WriteU32(m, (c.V1 + 0x4u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x8u), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0xCu), c.T1);
        c.A0 = c.A0 + 0x10u;
        if (c.A0 != c.V0) {
            c.V1 = c.V1 + 0x10u;
            goto L80010A70;
        }
        c.V1 = c.V1 + 0x10u;
        c.S2 = 0u + 0u;
        L80010AA0: ;
        c.V0 = 0x801D0000u;
        c.S4 = c.V0 - 0x6CC0u;
        c.S1 = 0x00010000u;
        c.S1 = c.S1 | 0x4FDAu;
        c.S0 = 0x0000005Cu;
        L80010AB4: ;
        c.V0 = MemoryAccess.ReadU8(m, (c.S3 + 0x5Au));
        c.V0 = (int)c.S2 < (int)c.V0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A0 = c.S3 + c.S0;
            goto L80010AE4;
        }
        c.A0 = c.S3 + c.S0;
        c.A0 = c.A0 + 0x8u;
        c.A1 = c.S1 + c.S4;
        c.RA = 0x80010AD4u;
        GranTurismo2ArcadePC.func_800770BC(c, m);
        c.S1 = c.S1 + 0x1C0u;
        c.S0 = c.S0 + 0xD0u;
        c.S2 = c.S2 + 0x1u;
        goto L80010AB4;
        L80010AE4: ;
        c.V0 = c.S3 + 0u;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010B08(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        c.V0 = 0x80090000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x2B64u));
        c.V0 = 0x801D0000u;
        c.V0 = c.V0 - 0x6CC0u;
        c.V1 = 0u | 0xBF7Cu;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = c.V0 + c.V1;
        c.A1 = c.S3 + 0x10u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.RA = 0x80010B44u;
        GranTurismo2ArcadePC.func_8007821C(c, m);
        c.V1 = 0x801D0000u;
        c.V1 = c.V1 - 0x6D00u;
        c.A1 = c.V0 + 0u;
        c.A0 = c.A1 + 0x44u;
        c.V0 = c.A0 | c.V1;
        c.V0 = c.V0 & 0x0003u;
        if (c.V0 == 0u) {
            c.V0 = c.A1 + 0x84u;
            goto L80010BB8;
        }
        c.V0 = c.A1 + 0x84u;
        L80010B64: ;
        c.A2 = MemoryAccess.ReadWordLeft(m, c.A2, (c.A0 + 0x3u));
        c.A2 = MemoryAccess.ReadWordRight(m, c.A2, c.A0);
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.A0 + 0x7u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, (c.A0 + 0x4u));
        c.T0 = MemoryAccess.ReadWordLeft(m, c.T0, (c.A0 + 0xBu));
        c.T0 = MemoryAccess.ReadWordRight(m, c.T0, (c.A0 + 0x8u));
        c.T1 = MemoryAccess.ReadWordLeft(m, c.T1, (c.A0 + 0xFu));
        c.T1 = MemoryAccess.ReadWordRight(m, c.T1, (c.A0 + 0xCu));
        MemoryAccess.WriteWordLeft(m, (c.V1 + 0x3u), c.A2);
        MemoryAccess.WriteWordRight(m, c.V1, c.A2);
        MemoryAccess.WriteWordLeft(m, (c.V1 + 0x7u), c.A3);
        MemoryAccess.WriteWordRight(m, (c.V1 + 0x4u), c.A3);
        MemoryAccess.WriteWordLeft(m, (c.V1 + 0xBu), c.T0);
        MemoryAccess.WriteWordRight(m, (c.V1 + 0x8u), c.T0);
        MemoryAccess.WriteWordLeft(m, (c.V1 + 0xFu), c.T1);
        MemoryAccess.WriteWordRight(m, (c.V1 + 0xCu), c.T1);
        c.A0 = c.A0 + 0x10u;
        if (c.A0 != c.V0) {
            c.V1 = c.V1 + 0x10u;
            goto L80010B64;
        }
        c.V1 = c.V1 + 0x10u;
        c.S2 = 0u + 0u;
        goto L80010BE8;
        L80010BB8: ;
        c.A2 = MemoryAccess.ReadU32(m, c.A0);
        c.A3 = MemoryAccess.ReadU32(m, (c.A0 + 0x4u));
        c.T0 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.T1 = MemoryAccess.ReadU32(m, (c.A0 + 0xCu));
        MemoryAccess.WriteU32(m, c.V1, c.A2);
        MemoryAccess.WriteU32(m, (c.V1 + 0x4u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x8u), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0xCu), c.T1);
        c.A0 = c.A0 + 0x10u;
        if (c.A0 != c.V0) {
            c.V1 = c.V1 + 0x10u;
            goto L80010BB8;
        }
        c.V1 = c.V1 + 0x10u;
        c.S2 = 0u + 0u;
        L80010BE8: ;
        c.V0 = 0x801D0000u;
        c.S4 = c.V0 - 0x6CC0u;
        c.S1 = 0x00010000u;
        c.S1 = c.S1 | 0x4FDAu;
        c.S0 = 0x0000005Cu;
        L80010BFC: ;
        c.V0 = MemoryAccess.ReadU8(m, (c.S3 + 0x5Au));
        c.V0 = (int)c.S2 < (int)c.V0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A0 = c.S3 + c.S0;
            goto L80010C2C;
        }
        c.A0 = c.S3 + c.S0;
        c.A0 = c.A0 + 0x8u;
        c.A1 = c.S1 + c.S4;
        c.RA = 0x80010C1Cu;
        GranTurismo2ArcadePC.func_800770BC(c, m);
        c.S1 = c.S1 + 0x1C0u;
        c.S0 = c.S0 + 0xD0u;
        c.S2 = c.S2 + 0x1u;
        goto L80010BFC;
        L80010C2C: ;
        c.V0 = c.S3 + 0u;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010C50(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x30u;
        c.V0 = 0x80090000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x2B64u));
        c.V0 = 0x801D0000u;
        c.V0 = c.V0 - 0x6CC0u;
        c.V1 = 0u | 0xBF7Cu;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        c.S4 = c.V0 + c.V1;
        c.A1 = c.S4 + 0x10u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.RA = 0x80010C94u;
        GranTurismo2ArcadePC.func_8007821C(c, m);
        c.V1 = 0x801D0000u;
        c.V1 = c.V1 - 0x6D00u;
        c.A1 = c.V0 + 0u;
        c.A0 = c.A1 + 0x44u;
        c.V0 = c.A0 | c.V1;
        c.V0 = c.V0 & 0x0003u;
        if (c.V0 == 0u) {
            c.V0 = c.A1 + 0x84u;
            goto L80010D08;
        }
        c.V0 = c.A1 + 0x84u;
        L80010CB4: ;
        c.A2 = MemoryAccess.ReadWordLeft(m, c.A2, (c.A0 + 0x3u));
        c.A2 = MemoryAccess.ReadWordRight(m, c.A2, c.A0);
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.A0 + 0x7u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, (c.A0 + 0x4u));
        c.T0 = MemoryAccess.ReadWordLeft(m, c.T0, (c.A0 + 0xBu));
        c.T0 = MemoryAccess.ReadWordRight(m, c.T0, (c.A0 + 0x8u));
        c.T1 = MemoryAccess.ReadWordLeft(m, c.T1, (c.A0 + 0xFu));
        c.T1 = MemoryAccess.ReadWordRight(m, c.T1, (c.A0 + 0xCu));
        MemoryAccess.WriteWordLeft(m, (c.V1 + 0x3u), c.A2);
        MemoryAccess.WriteWordRight(m, c.V1, c.A2);
        MemoryAccess.WriteWordLeft(m, (c.V1 + 0x7u), c.A3);
        MemoryAccess.WriteWordRight(m, (c.V1 + 0x4u), c.A3);
        MemoryAccess.WriteWordLeft(m, (c.V1 + 0xBu), c.T0);
        MemoryAccess.WriteWordRight(m, (c.V1 + 0x8u), c.T0);
        MemoryAccess.WriteWordLeft(m, (c.V1 + 0xFu), c.T1);
        MemoryAccess.WriteWordRight(m, (c.V1 + 0xCu), c.T1);
        c.A0 = c.A0 + 0x10u;
        if (c.A0 != c.V0) {
            c.V1 = c.V1 + 0x10u;
            goto L80010CB4;
        }
        c.V1 = c.V1 + 0x10u;
        c.S2 = 0u + 0u;
        goto L80010D38;
        L80010D08: ;
        c.A2 = MemoryAccess.ReadU32(m, c.A0);
        c.A3 = MemoryAccess.ReadU32(m, (c.A0 + 0x4u));
        c.T0 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.T1 = MemoryAccess.ReadU32(m, (c.A0 + 0xCu));
        MemoryAccess.WriteU32(m, c.V1, c.A2);
        MemoryAccess.WriteU32(m, (c.V1 + 0x4u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x8u), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0xCu), c.T1);
        c.A0 = c.A0 + 0x10u;
        if (c.A0 != c.V0) {
            c.V1 = c.V1 + 0x10u;
            goto L80010D08;
        }
        c.V1 = c.V1 + 0x10u;
        c.S2 = 0u + 0u;
        L80010D38: ;
        c.V0 = 0x801D0000u;
        c.S5 = c.V0 - 0x6CC0u;
        c.S1 = 0x00010000u;
        c.S1 = c.S1 | 0x4FDAu;
        c.S0 = 0x0000005Cu;
        L80010D4C: ;
        c.V0 = MemoryAccess.ReadU8(m, (c.S4 + 0x5Au));
        c.V0 = (int)c.S2 < (int)c.V0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S4 + c.S0;
            goto L80010D9C;
        }
        c.V0 = c.S4 + c.S0;
        c.V0 = c.V0 + 0x8u;
        c.V1 = MemoryAccess.ReadU8(m, (c.V0 + 0x7Au));
        c.V1 = c.V1 >> 6;
        c.V1 = c.V1 & 0x0001u;
        if (c.V1 == 0u) {
            c.A0 = c.V0 + 0u;
            goto L80010D84;
        }
        c.A0 = c.V0 + 0u;
        c.S3 = 0x00000001u;
        goto L80010D8C;
        L80010D84: ;
        c.A1 = c.S1 + c.S5;
        c.RA = 0x80010D8Cu;
        GranTurismo2ArcadePC.func_800770BC(c, m);
        L80010D8C: ;
        c.S1 = c.S1 + 0x1C0u;
        c.S0 = c.S0 + 0xD0u;
        c.S2 = c.S2 + 0x1u;
        goto L80010D4C;
        L80010D9C: ;
        c.V0 = c.S3 + 0u;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x30u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010DC4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        c.V0 = 0x801D0000u;
        c.V0 = c.V0 - 0x6CC0u;
        c.V1 = 0u | 0xBF7Cu;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = c.V0 + c.V1;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        c.S4 = c.V0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = 0x00010000u;
        c.S1 = c.S1 | 0x4FDAu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0x0000005Cu;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.RA);
        L80010E04: ;
        c.V0 = MemoryAccess.ReadU8(m, (c.S3 + 0x5Au));
        c.V0 = (int)c.S2 < (int)c.V0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S3 + c.S0;
            goto L80010E4C;
        }
        c.V0 = c.S3 + c.S0;
        c.A0 = c.V0 + 0x8u;
        c.V0 = MemoryAccess.ReadU8(m, (c.A0 + 0x7Au));
        c.V0 = c.V0 >> 6;
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u) {
            goto L80010E3C;
        }
        c.A1 = c.S1 + c.S4;
        c.RA = 0x80010E3Cu;
        GranTurismo2ArcadePC.func_800770BC(c, m);
        L80010E3C: ;
        c.S1 = c.S1 + 0x1C0u;
        c.S0 = c.S0 + 0xD0u;
        c.S2 = c.S2 + 0x1u;
        goto L80010E04;
        L80010E4C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010E6C(CpuContext c, IMemory m)
    {
        c.A1 = 0x80020000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.A1 + 0x3310u));
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        if (c.V0 == 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
            goto L80010EC4;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A1 + 0x3310u;
        L80010E90: ;
        c.A1 = MemoryAccess.ReadU32(m, c.S0);
        c.A0 = c.S1 + 0u;
        c.RA = 0x80010E9Cu;
        GranTurismo2ArcadePC.func_8008CE10(c, m);
        if (c.V0 != 0u) {
            goto L80010EB0;
        }
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x4u));
        goto L80010EC8;
        L80010EB0: ;
        c.S0 = c.S0 + 0x8u;
        c.V0 = MemoryAccess.ReadU32(m, c.S0);
        if (c.V0 != 0u) {
            goto L80010E90;
        }
        L80010EC4: ;
        c.V0 = 0x00000093u;
        L80010EC8: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010EDC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        c.V0 = 0x801D0000u;
        c.V0 = c.V0 - 0x6CC0u;
        c.V1 = 0u | 0xBF7Cu;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = c.V0 + c.V1;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.V1 = MemoryAccess.ReadU8(m, (c.S3 + 0x9u));
        if (c.V1 == 0u) {
            c.V0 = 0x00000001u;
            goto L80010F28;
        }
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            goto L80010F6C;
        }
        goto L800110A4;
        L80010F28: ;
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x12C0u;
        c.A1 = 0x00030000u;
        c.RA = 0x80010F38u;
        GranTurismo2ArcadePC.func_80076D14(c, m);
        c.S0 = 0x80090000u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x2B64u), c.V0);
        c.RA = 0x80010F44u;
        GranTurismo2ArcadePC.func_80010C50(c, m);
        if (c.V0 == 0u) {
            c.A0 = 0x800E0000u;
            goto L800110A4;
        }
        c.A0 = 0x800E0000u;
        c.A0 = c.A0 + 0x12C0u;
        c.A1 = 0x000C0000u;
        c.A1 = c.A1 | 0x8000u;
        c.RA = 0x80010F5Cu;
        GranTurismo2ArcadePC.func_80076C84(c, m);
        MemoryAccess.WriteU32(m, (c.S0 + 0x2B64u), c.V0);
        c.RA = 0x80010F64u;
        GranTurismo2ArcadePC.func_80010DC4(c, m);
        goto L800110A4;
        L80010F6C: ;
        c.V0 = MemoryAccess.ReadU8(m, (c.S3 + 0xAu));
        c.V1 = c.V0 - 0x1u;
        c.V0 = c.V1 < 0x0000000Bu ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80020000u;
            goto L80011088;
        }
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0xC88u;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x80010FA0u: goto L80010FA0;
            case 0x80011088u: goto L80011088;
            case 0x80010FE0u: goto L80010FE0;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L80010FA0: ;
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x12C0u;
        c.A1 = 0x00030000u;
        c.RA = 0x80010FB0u;
        GranTurismo2ArcadePC.func_80076C08(c, m);
        c.A0 = 0x800E0000u;
        c.A0 = c.A0 + 0x12C0u;
        c.A1 = 0x000C0000u;
        c.A1 = c.A1 | 0x8000u;
        c.V1 = 0x80090000u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x2B68u), c.V0);
        c.RA = 0x80010FCCu;
        GranTurismo2ArcadePC.func_80076C84(c, m);
        c.V1 = 0x80090000u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x2B64u), c.V0);
        c.RA = 0x80010FD8u;
        GranTurismo2ArcadePC.func_800109C0(c, m);
        goto L800110A4;
        L80010FE0: ;
        c.A0 = c.S3 + 0x10u;
        c.RA = 0x80010FE8u;
        GranTurismo2ArcadePC.func_80010E6C(c, m);
        c.A0 = c.V0 + 0u;
        c.S0 = 0x800B0000u;
        c.S0 = c.S0 + 0x12C0u;
        c.A1 = c.S0 + 0u;
        c.RA = 0x80010FFCu;
        GranTurismo2ArcadePC.func_8005D844(c, m);
        c.A0 = c.S0 + 0u;
        c.RA = 0x80011004u;
        GranTurismo2ArcadePC.func_80069CEC(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x00030000u;
        c.RA = 0x80011010u;
        GranTurismo2ArcadePC.func_80076C08(c, m);
        c.A0 = 0x800E0000u;
        c.A0 = c.A0 + 0x12C0u;
        c.A1 = 0x000C0000u;
        c.A1 = c.A1 | 0x8000u;
        c.V1 = 0x80090000u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x2B68u), c.V0);
        c.RA = 0x8001102Cu;
        GranTurismo2ArcadePC.func_80076C84(c, m);
        c.V1 = 0x80090000u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x2B64u), c.V0);
        c.RA = 0x80011038u;
        GranTurismo2ArcadePC.func_800109C0(c, m);
        c.S2 = 0u + 0u;
        c.V0 = 0x801D0000u;
        c.S4 = c.V0 - 0x6CC0u;
        c.S1 = 0x00010000u;
        c.S1 = c.S1 | 0x4FDAu;
        c.S0 = 0x0000005Cu;
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.S3 + 0x5Au), (byte)c.V0);
        L80011058: ;
        c.V0 = MemoryAccess.ReadU8(m, (c.S3 + 0x5Au));
        c.V0 = (int)c.S2 < (int)c.V0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A0 = c.S3 + c.S0;
            goto L800110A4;
        }
        c.A0 = c.S3 + c.S0;
        c.A0 = c.A0 + 0x8u;
        c.A1 = c.S1 + c.S4;
        c.RA = 0x80011078u;
        GranTurismo2ArcadePC.func_800770BC(c, m);
        c.S1 = c.S1 + 0x1C0u;
        c.S0 = c.S0 + 0xD0u;
        c.S2 = c.S2 + 0x1u;
        goto L80011058;
        L80011088: ;
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x12C0u;
        c.A1 = 0x00030000u;
        c.RA = 0x80011098u;
        GranTurismo2ArcadePC.func_80076D98(c, m);
        c.V1 = 0x80090000u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x2B64u), c.V0);
        c.RA = 0x800110A4u;
        GranTurismo2ArcadePC.func_80010B08(c, m);
        L800110A4: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800110C4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A1 + 0u;
        c.A0 = c.S0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.A1 = c.V0 + 0u;
        c.RA = 0x800110E4u;
        GranTurismo2ArcadePC.func_8005D810(c, m);
        c.A0 = c.S0 + 0u;
        c.RA = 0x800110ECu;
        GranTurismo2ArcadePC.func_8005D6D8(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800110FC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        c.V1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.V0 = c.V0 >> 3;
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u) {
            c.A0 = c.V1 + 0x8u;
            goto L8001112C;
        }
        c.A0 = c.V1 + 0x8u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.A0 = c.A0 + c.V0;
        L8001112C: ;
        c.V0 = c.A1 & 0x000Fu;
        c.V0 = c.V0 << 6;
        MemoryAccess.WriteU16(m, (c.SP + 0x10u), (ushort)c.V0);
        c.V0 = c.A1 & 0x0010u;
        c.V0 = c.V0 << 4;
        MemoryAccess.WriteU16(m, (c.SP + 0x12u), (ushort)c.V0);
        c.V0 = MemoryAccess.ReadU16(m, (c.A0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.SP + 0x14u), (ushort)c.V0);
        c.V0 = MemoryAccess.ReadU16(m, (c.A0 + 0xAu));
        c.A1 = c.SP + 0x10u;
        MemoryAccess.WriteU16(m, (c.SP + 0x16u), (ushort)c.V0);
        c.RA = 0x80011160u;
        GranTurismo2ArcadePC.func_8007BAE4(c, m);
        c.RA = 0x80011168u;
        GranTurismo2ArcadePC.func_8007AE40(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80011178(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0x800E0000u;
        c.S0 = c.S0 + 0x12C0u;
        c.A0 = c.S0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.A1 = 0x0000000Fu;
        c.RA = 0x80011198u;
        GranTurismo2ArcadePC.func_800110C4(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x0000001Du;
        c.RA = 0x800111A4u;
        GranTurismo2ArcadePC.func_800110FC(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x0000000Bu;
        c.RA = 0x800111B0u;
        GranTurismo2ArcadePC.func_800110C4(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x0000001Eu;
        c.RA = 0x800111BCu;
        GranTurismo2ArcadePC.func_800110FC(c, m);
        c.A0 = 0x80020000u;
        c.A0 = c.A0 + 0x3438u;
        c.A1 = 0u | 0xC800u;
        c.RA = 0x800111CCu;
        GranTurismo2ArcadePC.func_80060908(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800111DC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        c.V0 = 0x801D0000u;
        c.V1 = 0x80020000u;
        c.V1 = c.V1 + 0x33F0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S0);
        c.S0 = 0x800E0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S1);
        c.S1 = MemoryAccess.ReadU8(m, (c.V0 - 0x6CC0u));
        c.S0 = c.S0 + 0x12C0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S2);
        c.V0 = c.S1 << 1;
        c.V0 = c.V0 + c.V1;
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, c.V0);
        c.A0 = c.S0 + 0u;
        c.RA = 0x8001121Cu;
        GranTurismo2ArcadePC.func_800110C4(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x0000000Eu;
        c.RA = 0x80011228u;
        GranTurismo2ArcadePC.func_800110FC(c, m);
        c.A0 = 0x0000002Bu;
        c.A1 = c.S0 + 0u;
        c.RA = 0x80011234u;
        GranTurismo2ArcadePC.func_8005D844(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x0000000Cu;
        c.RA = 0x80011240u;
        GranTurismo2ArcadePC.func_800110FC(c, m);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x3400u;
        c.S1 = c.S1 << 2;
        c.S1 = c.S1 + c.V0;
        c.A0 = MemoryAccess.ReadU32(m, c.S1);
        c.A1 = c.S0 + 0u;
        c.RA = 0x8001125Cu;
        GranTurismo2ArcadePC.func_8005D844(c, m);
        c.S1 = c.S0 + 0x8u;
        c.A0 = c.S1 + 0u;
        c.A1 = c.SP + 0x10u;
        c.S2 = 0x00000180u;
        MemoryAccess.WriteU16(m, (c.SP + 0x10u), (ushort)c.S2);
        c.V1 = MemoryAccess.ReadU16(m, (c.S1 + 0x8u));
        c.A2 = MemoryAccess.ReadU16(m, (c.S1 + 0xAu));
        c.V0 = 0x000001FFu;
        MemoryAccess.WriteU16(m, (c.SP + 0x12u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.SP + 0x14u), (ushort)c.V1);
        MemoryAccess.WriteU16(m, (c.SP + 0x16u), (ushort)c.A2);
        c.RA = 0x8001128Cu;
        GranTurismo2ArcadePC.func_8007BAE4(c, m);
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.SP + 0x10u), (ushort)c.S2);
        MemoryAccess.WriteU16(m, (c.SP + 0x12u), (ushort)0u);
        c.A0 = c.A0 + c.S1;
        c.V0 = MemoryAccess.ReadU16(m, (c.A0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.SP + 0x14u), (ushort)c.V0);
        c.V0 = MemoryAccess.ReadU16(m, (c.A0 + 0xAu));
        c.A1 = c.SP + 0x10u;
        MemoryAccess.WriteU16(m, (c.SP + 0x16u), (ushort)c.V0);
        c.RA = 0x800112B8u;
        GranTurismo2ArcadePC.func_8007BAE4(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800112D0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x240u;
        MemoryAccess.WriteU32(m, (c.SP + 0x23Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x238u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x234u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x230u), c.S0);
        c.RA = 0x800112E8u;
        GranTurismo2ArcadePC.func_80011178(c, m);
        c.S0 = 0x801F0000u;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 - 0xFE0u));
        if (c.V0 != 0u) {
            goto L8001134C;
        }
        c.RA = 0x80011304u;
        GranTurismo2ArcadePC.func_800161B4(c, m);
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.S0 - 0xFE0u), (byte)c.V0);
        c.A0 = c.SP + 0x10u;
        c.RA = 0x80011314u;
        GranTurismo2ArcadePC.func_80011AC8(c, m);
        c.A0 = c.SP + 0x10u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x51D8u;
        c.RA = 0x80011324u;
        GranTurismo2ArcadePC.func_80011B24(c, m);
        L80011324: ;
        c.A0 = c.SP + 0x10u;
        c.RA = 0x8001132Cu;
        GranTurismo2ArcadePC.func_800832F8(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L80011324;
        }
        if ((int)c.V1 < 0) {
            c.A0 = c.SP + 0x10u;
            goto L80011324;
        }
        c.A0 = c.SP + 0x10u;
        c.A1 = 0x00000002u;
        c.RA = 0x8001134Cu;
        Dispatcher.Call(c, m, 0x80011AFCu);
        L8001134C: ;
        c.S0 = 0x801D0000u;
        c.RA = 0x80011354u;
        GranTurismo2ArcadePC.func_800161B4(c, m);
        c.S0 = c.S0 - 0x6CC0u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x80011360u;
        GranTurismo2ArcadePC.func_80011720(c, m);
        c.S0 = c.S0 + 0xB8u;
        c.RA = 0x80011368u;
        GranTurismo2ArcadePC.func_800111DC(c, m);
        c.V0 = 0x801F0000u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x44u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.S0 + 0x46u), (ushort)0u);
        c.V1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V0 - 0xFDEu));
        c.V0 = 0x00000002u;
        if (c.V1 == c.V0) {
            c.V0 = (int)c.V1 < 3 ? 1u : 0u;
            goto L800115E8;
        }
        c.V0 = (int)c.V1 < 3 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A0 = 0x00000001u;
            goto L80011394;
        }
        c.A0 = 0x00000001u;
        if (c.V1 == 0u) {
            goto L800113A4;
        }
        L80011394: ;
        c.A1 = 0x80010000u;
        c.A1 = c.A1 + 0x1698u;
        c.A2 = 0u + 0u;
        c.RA = 0x800113A4u;
        GranTurismo2ArcadePC.func_8005D9EC(c, m);
        L800113A4: ;
        c.A0 = c.SP + 0x10u;
        c.RA = 0x800113ACu;
        GranTurismo2ArcadePC.func_80011AC8(c, m);
        c.A0 = c.SP + 0x10u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x4EB4u;
        c.RA = 0x800113BCu;
        GranTurismo2ArcadePC.func_80011B24(c, m);
        L800113BC: ;
        c.A0 = c.SP + 0x10u;
        c.RA = 0x800113C4u;
        GranTurismo2ArcadePC.func_800832F8(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L800113BC;
        }
        if ((int)c.V1 < 0) {
            c.A0 = c.SP + 0x10u;
            goto L800113BC;
        }
        c.A0 = c.SP + 0x10u;
        c.A1 = 0x00000002u;
        c.RA = 0x800113E4u;
        Dispatcher.Call(c, m, 0x80011AFCu);
        c.A0 = 0x00000001u;
        c.RA = 0x800113ECu;
        GranTurismo2ArcadePC.func_8007F740(c, m);
        c.V0 = 0x801F0000u;
        c.V1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V0 - 0xFDDu));
        c.V0 = c.V1 < 0x00000007u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80020000u;
            goto L8001145C;
        }
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0xCB8u;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x80011420u: goto L80011420;
            case 0x80011444u: goto L80011444;
            case 0x800114DCu: goto L800114DC;
            case 0x8001145Cu: goto L8001145C;
            case 0x8001149Cu: goto L8001149C;
            case 0x80011524u: goto L80011524;
            case 0x8001156Cu: goto L8001156C;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L80011420: ;
        c.A0 = 0x00000002u;
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0xFE0u;
        c.V1 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.V0 + 0x1u), (byte)c.V1);
        MemoryAccess.WriteU8(m, (c.V0 + 0x2u), (byte)c.V1);
        c.RA = 0x8001143Cu;
        GranTurismo2ArcadePC.func_8005D9AC(c, m);
        goto L8001145C;
        L80011444: ;
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0xFE0u;
        c.V1 = 0x00000002u;
        MemoryAccess.WriteU8(m, (c.V0 + 0x1u), (byte)c.V1);
        MemoryAccess.WriteU8(m, (c.V0 + 0x2u), (byte)c.V1);
        goto L800115E8;
        L8001145C: ;
        c.A0 = c.SP + 0x10u;
        c.RA = 0x80011464u;
        GranTurismo2ArcadePC.func_80011AC8(c, m);
        c.A0 = c.SP + 0x10u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x55C8u;
        c.RA = 0x80011474u;
        GranTurismo2ArcadePC.func_80011B24(c, m);
        L80011474: ;
        c.A0 = c.SP + 0x10u;
        c.RA = 0x8001147Cu;
        GranTurismo2ArcadePC.func_800832F8(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L80011474;
        }
        if ((int)c.V1 < 0) {
            c.A0 = c.SP + 0x10u;
            goto L80011474;
        }
        c.A0 = c.SP + 0x10u;
        goto L8001155C;
        L8001149C: ;
        c.A0 = c.SP + 0x10u;
        c.RA = 0x800114A4u;
        GranTurismo2ArcadePC.func_80011AC8(c, m);
        c.A0 = c.SP + 0x10u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x5520u;
        c.RA = 0x800114B4u;
        GranTurismo2ArcadePC.func_80011B24(c, m);
        L800114B4: ;
        c.A0 = c.SP + 0x10u;
        c.RA = 0x800114BCu;
        GranTurismo2ArcadePC.func_800832F8(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L800114B4;
        }
        if ((int)c.V1 < 0) {
            c.A0 = c.SP + 0x10u;
            goto L800114B4;
        }
        c.A0 = c.SP + 0x10u;
        goto L8001155C;
        L800114DC: ;
        c.A0 = c.SP + 0x10u;
        c.RA = 0x800114E4u;
        GranTurismo2ArcadePC.func_80011AC8(c, m);
        c.A0 = c.SP + 0x10u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x4B58u;
        c.RA = 0x800114F4u;
        GranTurismo2ArcadePC.func_80011B24(c, m);
        L800114F4: ;
        c.A0 = c.SP + 0x10u;
        c.RA = 0x800114FCu;
        GranTurismo2ArcadePC.func_800832F8(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L800114F4;
        }
        if ((int)c.V1 < 0) {
            c.A0 = 0x801D0000u;
            goto L800114F4;
        }
        c.A0 = 0x801D0000u;
        c.A0 = c.A0 - 0x6CC0u;
        c.RA = 0x8001151Cu;
        GranTurismo2ArcadePC.func_80011720(c, m);
        c.A0 = c.SP + 0x10u;
        goto L8001155C;
        L80011524: ;
        c.A0 = c.SP + 0x10u;
        c.RA = 0x8001152Cu;
        GranTurismo2ArcadePC.func_80011AC8(c, m);
        c.A0 = c.SP + 0x10u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x468Cu;
        c.RA = 0x8001153Cu;
        GranTurismo2ArcadePC.func_80011B24(c, m);
        L8001153C: ;
        c.A0 = c.SP + 0x10u;
        c.RA = 0x80011544u;
        GranTurismo2ArcadePC.func_800832F8(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L8001153C;
        }
        if ((int)c.V1 < 0) {
            c.A0 = c.SP + 0x10u;
            goto L8001153C;
        }
        c.A0 = c.SP + 0x10u;
        L8001155C: ;
        c.A1 = 0x00000002u;
        c.RA = 0x80011564u;
        Dispatcher.Call(c, m, 0x80011AFCu);
        goto L800113A4;
        L8001156C: ;
        c.V0 = 0x801F0000u;
        c.S2 = c.V0 - 0xFE0u;
        c.V1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S2 + 0x10u));
        MemoryAccess.WriteU8(m, (c.S2 + 0x1u), (byte)0u);
        c.V1 = c.V1 + 0x1u;
        c.V0 = (int)c.V1 < 4 ? 1u : 0u;
        if (c.V0 != 0u) {
            MemoryAccess.WriteU8(m, (c.S2 + 0x2u), (byte)0u);
            goto L80011590;
        }
        MemoryAccess.WriteU8(m, (c.S2 + 0x2u), (byte)0u);
        c.V1 = 0u + 0u;
        L80011590: ;
        if (c.V1 != 0u) {
            MemoryAccess.WriteU8(m, (c.S2 + 0x10u), (byte)c.V1);
            goto L800115A0;
        }
        MemoryAccess.WriteU8(m, (c.S2 + 0x10u), (byte)c.V1);
        c.A0 = 0x00000005u;
        c.RA = 0x800115A0u;
        GranTurismo2ArcadePC.func_8005D9AC(c, m);
        L800115A0: ;
        c.S1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0xEu));
        c.S0 = 0x800E0000u;
        c.S0 = c.S0 + 0x12C0u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x800115B4u;
        GranTurismo2ArcadePC.func_80020A50(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x80100000u;
        c.A1 = c.A1 + 0x52C0u;
        c.S0 = c.V0 + 0u;
        c.A2 = c.S1 + 0u;
        c.RA = 0x800115CCu;
        GranTurismo2ArcadePC.func_80020A98(c, m);
        c.S1 = c.S1 + 0x1u;
        c.S0 = (int)c.S1 < (int)c.S0 ? 1u : 0u;
        if (c.S0 != 0u) {
            goto L800115E0;
        }
        c.S1 = 0u + 0u;
        L800115E0: ;
        MemoryAccess.WriteU16(m, (c.S2 + 0xEu), (ushort)c.S1);
        goto L80011654;
        L800115E8: ;
        c.A0 = c.SP + 0x10u;
        c.RA = 0x800115F0u;
        GranTurismo2ArcadePC.func_80011AC8(c, m);
        c.A0 = c.SP + 0x10u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x57C0u;
        c.RA = 0x80011600u;
        GranTurismo2ArcadePC.func_80011B24(c, m);
        L80011600: ;
        c.A0 = c.SP + 0x10u;
        c.RA = 0x80011608u;
        GranTurismo2ArcadePC.func_800832F8(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L80011600;
        }
        if ((int)c.V1 < 0) {
            c.A0 = c.SP + 0x10u;
            goto L80011600;
        }
        c.A0 = c.SP + 0x10u;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.A0 + 0x21Cu));
        if (c.V0 != 0u) {
            c.V0 = 0x801F0000u;
            goto L8001164C;
        }
        c.V0 = 0x801F0000u;
        c.A1 = 0x00000002u;
        c.V0 = c.V0 - 0xFE0u;
        MemoryAccess.WriteU8(m, (c.V0 + 0x1u), (byte)0u);
        MemoryAccess.WriteU8(m, (c.V0 + 0x2u), (byte)0u);
        c.RA = 0x80011644u;
        Dispatcher.Call(c, m, 0x80011AFCu);
        goto L800113A4;
        L8001164C: ;
        c.A1 = 0x00000002u;
        c.RA = 0x80011654u;
        Dispatcher.Call(c, m, 0x80011AFCu);
        L80011654: ;
        c.RA = 0x8001165Cu;
        GranTurismo2ArcadePC.func_80010EDC(c, m);
        c.A0 = 0x00000001u;
        c.RA = 0x80011664u;
        GranTurismo2ArcadePC.func_8007D14C(c, m);
        c.A0 = 0x00000001u;
        c.RA = 0x8001166Cu;
        GranTurismo2ArcadePC.func_8007F740(c, m);
        c.A0 = 0x00000002u;
        c.RA = 0x80011674u;
        GranTurismo2ArcadePC.func_8007D14C(c, m);
        c.A0 = 0u + 0u;
        c.A1 = 0x80010000u;
        c.A1 = c.A1 + 0x1F64u;
        c.A2 = 0x00000001u;
        c.RA = 0x80011688u;
        GranTurismo2ArcadePC.func_8005D9EC(c, m);
        c.A0 = 0x00000001u;
        c.RA = 0x80011690u;
        GranTurismo2ArcadePC.func_8007F740(c, m);
        goto L800113A4;
        c.SP = c.SP - 0x28u;
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x12C0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S0);
        c.S0 = 0x801F0000u;
        c.S0 = c.S0 - 0xFE0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.RA);
        c.V0 = MemoryAccess.ReadU16(m, (c.S0 + 0xEu));
        c.A1 = 0x00030000u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x2u), (byte)0u);
        c.V0 = c.V0 + 0x1u;
        MemoryAccess.WriteU16(m, (c.S0 + 0xEu), (ushort)c.V0);
        c.RA = 0x800116CCu;
        GranTurismo2ArcadePC.func_80076D14(c, m);
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0xEu));
        c.V1 = 0x80090000u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x2B64u), c.V0);
        c.RA = 0x800116DCu;
        Dispatcher.Call(c, m, 0x80060D54u);
        c.A0 = 0x00000001u;
        c.A1 = 0u + 0u;
        c.A2 = c.A1 + 0u;
        c.A3 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.RA = 0x800116FCu;
        GranTurismo2ArcadePC.func_800104E0(c, m);
        c.A0 = 0u + 0u;
        c.A1 = 0x80010000u;
        c.A1 = c.A1 + 0x1F64u;
        c.A2 = 0x00000001u;
        c.RA = 0x80011710u;
        GranTurismo2ArcadePC.func_8005D9EC(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80011720(CpuContext c, IMemory m)
    {
        c.V0 = 0x800A0000u;
        c.A3 = c.V0 + 0x6BE4u;
        c.V1 = MemoryAccess.ReadWordLeft(m, c.V1, (c.A0 + 0x4Bu));
        c.V1 = MemoryAccess.ReadWordRight(m, c.V1, (c.A0 + 0x48u));
        c.A1 = MemoryAccess.ReadWordLeft(m, c.A1, (c.A0 + 0x4Fu));
        c.A1 = MemoryAccess.ReadWordRight(m, c.A1, (c.A0 + 0x4Cu));
        c.A2 = MemoryAccess.ReadWordLeft(m, c.A2, (c.A0 + 0x53u));
        c.A2 = MemoryAccess.ReadWordRight(m, c.A2, (c.A0 + 0x50u));
        MemoryAccess.WriteWordLeft(m, (c.A3 + 0x3u), c.V1);
        MemoryAccess.WriteWordRight(m, c.A3, c.V1);
        MemoryAccess.WriteWordLeft(m, (c.A3 + 0x7u), c.A1);
        MemoryAccess.WriteWordRight(m, (c.A3 + 0x4u), c.A1);
        MemoryAccess.WriteWordLeft(m, (c.A3 + 0xBu), c.A2);
        MemoryAccess.WriteWordRight(m, (c.A3 + 0x8u), c.A2);
        c.V1 = MemoryAccess.ReadWordLeft(m, c.V1, (c.A0 + 0x57u));
        c.V1 = MemoryAccess.ReadWordRight(m, c.V1, (c.A0 + 0x54u));
        c.A1 = MemoryAccess.ReadWordLeft(m, c.A1, (c.A0 + 0x5Bu));
        c.A1 = MemoryAccess.ReadWordRight(m, c.A1, (c.A0 + 0x58u));
        MemoryAccess.WriteWordLeft(m, (c.A3 + 0xFu), c.V1);
        MemoryAccess.WriteWordRight(m, (c.A3 + 0xCu), c.V1);
        MemoryAccess.WriteWordLeft(m, (c.A3 + 0x13u), c.A1);
        MemoryAccess.WriteWordRight(m, (c.A3 + 0x10u), c.A1);
        c.V0 = c.V0 + 0x6BE4u;
        c.V1 = MemoryAccess.ReadWordLeft(m, c.V1, (c.A0 + 0x9Du));
        c.V1 = MemoryAccess.ReadWordRight(m, c.V1, (c.A0 + 0x9Au));
        c.A1 = MemoryAccess.ReadWordLeft(m, c.A1, (c.A0 + 0xA1u));
        c.A1 = MemoryAccess.ReadWordRight(m, c.A1, (c.A0 + 0x9Eu));
        c.A2 = MemoryAccess.ReadWordLeft(m, c.A2, (c.A0 + 0xA5u));
        c.A2 = MemoryAccess.ReadWordRight(m, c.A2, (c.A0 + 0xA2u));
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.A0 + 0xA9u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, (c.A0 + 0xA6u));
        MemoryAccess.WriteWordLeft(m, (c.V0 + 0x17u), c.V1);
        MemoryAccess.WriteWordRight(m, (c.V0 + 0x14u), c.V1);
        MemoryAccess.WriteWordLeft(m, (c.V0 + 0x1Bu), c.A1);
        MemoryAccess.WriteWordRight(m, (c.V0 + 0x18u), c.A1);
        MemoryAccess.WriteWordLeft(m, (c.V0 + 0x1Fu), c.A2);
        MemoryAccess.WriteWordRight(m, (c.V0 + 0x1Cu), c.A2);
        MemoryAccess.WriteWordLeft(m, (c.V0 + 0x23u), c.A3);
        MemoryAccess.WriteWordRight(m, (c.V0 + 0x20u), c.A3);
        c.V1 = MemoryAccess.ReadWordLeft(m, c.V1, (c.A0 + 0xADu));
        c.V1 = MemoryAccess.ReadWordRight(m, c.V1, (c.A0 + 0xAAu));
        MemoryAccess.WriteWordLeft(m, (c.V0 + 0x27u), c.V1);
        MemoryAccess.WriteWordRight(m, (c.V0 + 0x24u), c.V1);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800117D4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A1 + 0u;
        c.A0 = c.S0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.A1 = c.V0 + 0u;
        c.RA = 0x800117F4u;
        GranTurismo2ArcadePC.func_8005D810(c, m);
        c.A0 = c.S0 + 0u;
        c.RA = 0x800117FCu;
        GranTurismo2ArcadePC.func_8005D6D8(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001180C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        c.V1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.V0 = c.V0 >> 3;
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u) {
            c.A0 = c.V1 + 0x8u;
            goto L8001183C;
        }
        c.A0 = c.V1 + 0x8u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.A0 = c.A0 + c.V0;
        L8001183C: ;
        c.V0 = c.A1 & 0x000Fu;
        c.V0 = c.V0 << 6;
        MemoryAccess.WriteU16(m, (c.SP + 0x10u), (ushort)c.V0);
        c.V0 = c.A1 & 0x0010u;
        c.V0 = c.V0 << 4;
        MemoryAccess.WriteU16(m, (c.SP + 0x12u), (ushort)c.V0);
        c.V0 = MemoryAccess.ReadU16(m, (c.A0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.SP + 0x14u), (ushort)c.V0);
        c.V0 = MemoryAccess.ReadU16(m, (c.A0 + 0xAu));
        c.A1 = c.SP + 0x10u;
        MemoryAccess.WriteU16(m, (c.SP + 0x16u), (ushort)c.V0);
        c.RA = 0x80011870u;
        GranTurismo2ArcadePC.func_8007BAE4(c, m);
        c.RA = 0x80011878u;
        GranTurismo2ArcadePC.func_8007AE40(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80011888(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x40u;
        MemoryAccess.WriteU32(m, (c.SP + 0x38u), c.FP);
        c.FP = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.S6);
        c.S6 = c.A3 + 0u;
        c.V0 = 0x801C0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x3Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x34u), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.A0);
        MemoryAccess.WriteU32(m, (c.SP + 0x48u), c.A2);
        c.V1 = MemoryAccess.ReadU32(m, (c.A2 + 0x8u));
        c.S2 = c.V0 - 0x6D00u;
        if (c.FP == 0u) {
            c.S7 = c.V1 + 0x10u;
            goto L80011A98;
        }
        c.S7 = c.V1 + 0x10u;
        c.V0 = MemoryAccess.ReadU32(m, (c.FP + 0x10u));
        if (c.V0 == 0u) {
            c.A0 = c.S2 + 0u;
            goto L80011A70;
        }
        c.A0 = c.S2 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6D10u;
        c.RA = 0x800118F4u;
        GranTurismo2ArcadePC.func_8007D990(c, m);
        c.A0 = c.S2 + 0u;
        c.V0 = (uint)((int)c.S6 >> 2);
        c.S1 = 0x00000022u;
        c.S1 = c.S1 - c.V0;
        MemoryAccess.WriteU32(m, (c.S2 + 0x10u), c.S7);
        c.A1 = MemoryAccess.ReadU32(m, (c.FP + 0x10u));
        c.A2 = c.S1 + 0u;
        c.RA = 0x80011914u;
        GranTurismo2ArcadePC.func_8006AC4C(c, m);
        c.S5 = c.V0 + 0u;
        c.A0 = c.S7 + 0u;
        c.A1 = c.S6 << 4;
        c.A1 = c.A1 - c.S6;
        c.A1 = c.A1 << 3;
        c.A1 = c.A1 + c.S6;
        c.A1 = c.A1 << 1;
        c.A1 = (uint)((int)c.A1 >> 7);
        c.T0 = 0x00000160u;
        c.S0 = c.T0 - c.S5;
        c.S0 = (uint)((int)c.S0 >> 1);
        c.RA = 0x80011944u;
        GranTurismo2ArcadePC.func_8007CF34(c, m);
        c.V1 = c.S6 << 1;
        c.V1 = c.V1 + c.S6;
        c.S3 = c.V1 << 4;
        c.V1 = c.V1 + c.S3;
        c.V1 = c.V1 << 1;
        c.S4 = (uint)((int)c.V1 >> 7);
        c.A0 = c.S2 + 0u;
        c.A2 = c.S0 + 0u;
        c.V1 = c.S0 + 0x1u;
        MemoryAccess.WriteU16(m, c.V0, (ushort)c.V1);
        c.V1 = 0x0000005Cu;
        MemoryAccess.WriteU16(m, (c.V0 + 0x2u), (ushort)c.V1);
        c.V1 = 0x00000002u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x4u), (ushort)c.S5);
        MemoryAccess.WriteU16(m, (c.V0 + 0x6u), (ushort)c.V1);
        c.V0 = c.S4 << 8;
        c.V0 = c.S4 | c.V0;
        c.V1 = c.S4 << 16;
        c.V0 = c.V0 | c.V1;
        MemoryAccess.WriteU32(m, (c.S2 + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S1);
        c.A1 = MemoryAccess.ReadU32(m, (c.FP + 0x10u));
        c.A3 = 0x0000005Au;
        c.RA = 0x800119A4u;
        GranTurismo2ArcadePC.func_8006ABA0(c, m);
        c.A0 = c.S2 + 0u;
        c.A2 = c.S0 + 0x3u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S1);
        c.A1 = MemoryAccess.ReadU32(m, (c.FP + 0x10u));
        c.A3 = 0x0000005Du;
        c.RA = 0x800119C0u;
        GranTurismo2ArcadePC.func_8006ABA0(c, m);
        c.V1 = MemoryAccess.ReadU32(m, (c.FP + 0xCu));
        c.V0 = c.V1 & 0x00FFu;
        { var _r = (long)(int)c.V0 * (int)c.S6; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.A1 = c.LO;
        c.V0 = c.V1 >> 8;
        c.V0 = c.V0 & 0x00FFu;
        { var _r = (long)(int)c.V0 * (int)c.S6; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V0 = c.LO;
        c.V1 = c.V1 >> 16;
        c.V1 = c.V1 & 0x00FFu;
        { var _r = (long)(int)c.V1 * (int)c.S6; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.S5 = (uint)((int)c.S3 >> 7);
        c.A0 = c.S7 + 0u;
        c.S0 = (uint)((int)c.A1 >> 7);
        c.S4 = (uint)((int)c.V0 >> 7);
        c.V1 = c.LO;
        c.S1 = (uint)((int)c.V1 >> 7);
        c.RA = 0x80011A0Cu;
        GranTurismo2ArcadePC.func_8007DFC0(c, m);
        c.A0 = c.S7 + 0u;
        c.A1 = 0x00000220u;
        c.V1 = c.S4 << 8;
        c.S0 = c.S0 | c.V1;
        c.S1 = c.S1 << 16;
        c.S0 = c.S0 | c.S1;
        c.V1 = 0x3A000000u;
        c.S0 = c.S0 | c.V1;
        c.T0 = 0x00000160u;
        c.V1 = 0x00000030u;
        c.V1 = c.V1 - c.S5;
        MemoryAccess.WriteU16(m, (c.V0 + 0xEu), (ushort)c.V1);
        MemoryAccess.WriteU16(m, (c.V0 + 0x6u), (ushort)c.V1);
        c.V1 = c.S5 + 0x30u;
        MemoryAccess.WriteU32(m, (c.V0 + 0x8u), c.S0);
        MemoryAccess.WriteU32(m, c.V0, c.S0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x18u), 0u);
        MemoryAccess.WriteU32(m, (c.V0 + 0x10u), 0u);
        MemoryAccess.WriteU16(m, (c.V0 + 0x14u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.V0 + 0x4u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.V0 + 0x1Cu), (ushort)c.T0);
        MemoryAccess.WriteU16(m, (c.V0 + 0xCu), (ushort)c.T0);
        MemoryAccess.WriteU16(m, (c.V0 + 0x1Eu), (ushort)c.V1);
        MemoryAccess.WriteU16(m, (c.V0 + 0x16u), (ushort)c.V1);
        c.RA = 0x80011A70u;
        GranTurismo2ArcadePC.func_8007D954(c, m);
        L80011A70: ;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x40u));
        MemoryAccess.WriteU32(m, (c.T0 + 0x1C4u), c.FP);
        c.A1 = MemoryAccess.ReadU32(m, (c.FP + 0x8u));
        if (c.A1 == 0u) {
            goto L80011A98;
        }
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0x48u));
        c.RA = 0x80011A98u;
        Dispatcher.Call(c, m, c.A1);
        L80011A98: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x3Cu));
        c.FP = MemoryAccess.ReadU32(m, (c.SP + 0x38u));
        c.S7 = MemoryAccess.ReadU32(m, (c.SP + 0x34u));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x30u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x2Cu));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.SP = c.SP + 0x40u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80011AC8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.S0 = c.A0 + 0u;
        c.RA = 0x80011ADCu;
        GranTurismo2ArcadePC.func_8007FD9C(c, m);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0xCF0u;
        MemoryAccess.WriteU32(m, c.S0, c.V0);
        c.V0 = c.S0 + 0u;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80011AFC_gt2_arcade_overlay_1(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0xCF0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU32(m, c.A0, c.V0);
        c.RA = 0x80011B14u;
        GranTurismo2ArcadePC.func_8007FDD8(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80011B24(CpuContext c, IMemory m)
    {
        MemoryAccess.WriteU8(m, (c.A0 + 0x20Cu), (byte)0u);
        MemoryAccess.WriteU32(m, (c.A0 + 0x1CCu), c.A1);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80011B30(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x48u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x3Cu), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x38u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x34u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S0);
        c.RA = 0x80011B60u;
        GranTurismo2ArcadePC.func_800808C0(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = 0x00000064u;
        MemoryAccess.WriteU8(m, (c.S1 + 0x21Cu), (byte)0u);
        c.RA = 0x80011B70u;
        GranTurismo2ArcadePC.func_8007FE80(c, m);
        c.A0 = c.S1 + 0x58u;
        c.A1 = 0x801B0000u;
        c.A1 = c.A1 - 0x6D40u;
        c.A2 = 0x00010000u;
        c.RA = 0x80011B84u;
        GranTurismo2ArcadePC.func_800803A4(c, m);
        c.S0 = c.S1 + 0x38u;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x80040000u;
        c.A1 = c.A1 - 0x63C8u;
        c.A2 = 0x00010000u;
        c.A2 = c.A2 | 0x0A40u;
        c.RA = 0x80011BA0u;
        GranTurismo2ArcadePC.func_8007FF98(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x00000004u;
        c.S5 = 0x00000001u;
        c.V1 = 0x00000020u;
        c.V0 = 0x00000002u;
        c.S7 = 0x00000003u;
        MemoryAccess.WriteU16(m, (c.S1 + 0xACu), (ushort)c.V0);
        c.V0 = 0x00001060u;
        c.S6 = 0x00000005u;
        MemoryAccess.WriteU16(m, (c.S1 + 0x7Cu), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.S1 + 0x7Eu), (ushort)c.A1);
        MemoryAccess.WriteU16(m, (c.S1 + 0x9Cu), (ushort)c.S5);
        MemoryAccess.WriteU16(m, (c.S1 + 0x9Eu), (ushort)c.V1);
        MemoryAccess.WriteU16(m, (c.S1 + 0xAEu), (ushort)c.V1);
        MemoryAccess.WriteU16(m, (c.S1 + 0xBCu), (ushort)c.S7);
        MemoryAccess.WriteU16(m, (c.S1 + 0xBEu), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.S1 + 0xCCu), (ushort)c.A1);
        MemoryAccess.WriteU16(m, (c.S1 + 0xCEu), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.S1 + 0x8Cu), (ushort)c.S6);
        MemoryAccess.WriteU16(m, (c.S1 + 0x8Eu), (ushort)c.A1);
        c.RA = 0x80011BF4u;
        GranTurismo2ArcadePC.func_80080010(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0x7Cu;
        c.RA = 0x80011C00u;
        GranTurismo2ArcadePC.func_8007FFC4(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0x9Cu;
        c.RA = 0x80011C0Cu;
        GranTurismo2ArcadePC.func_8007FFC4(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0xACu;
        c.RA = 0x80011C18u;
        GranTurismo2ArcadePC.func_8007FFC4(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0xBCu;
        c.RA = 0x80011C24u;
        GranTurismo2ArcadePC.func_8007FFC4(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0xCCu;
        c.RA = 0x80011C30u;
        GranTurismo2ArcadePC.func_8007FFC4(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0x8Cu;
        c.RA = 0x80011C3Cu;
        GranTurismo2ArcadePC.func_8007FFC4(c, m);
        c.S0 = 0x800E0000u;
        c.S0 = c.S0 + 0x12C0u;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x0000000Cu;
        MemoryAccess.WriteU8(m, (c.S1 + 0x70u), (byte)0u);
        c.RA = 0x80011C54u;
        GranTurismo2ArcadePC.func_800117D4(c, m);
        c.S2 = 0x801C0000u;
        c.S2 = c.S2 - 0x6D00u;
        c.A0 = c.S2 + 0u;
        c.A1 = 0x0000001Eu;
        c.V0 = c.V0 + 0x3u;
        c.V1 = 0xFFFFFFFCu;
        c.V0 = c.V0 & c.V1;
        c.V1 = 0x800B0000u;
        c.V0 = c.V0 + c.S0;
        MemoryAccess.WriteU32(m, (c.V1 - 0x759Cu), c.V0);
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V0 - 0x75A0u), c.S1);
        c.RA = 0x80011C88u;
        GranTurismo2ArcadePC.func_8006AB78(c, m);
        c.S4 = c.S1 + 0xDCu;
        c.A0 = c.S4 + 0u;
        c.A1 = 0u + 0u;
        c.S3 = 0x800A0000u;
        c.S3 = c.S3 + 0x6C54u;
        c.A2 = c.S3 + 0u;
        c.T3 = 0x801C0000u;
        c.A3 = c.T3 - 0x6D10u;
        c.V0 = 0x0000000Cu;
        c.T4 = 0x801C0000u;
        c.T0 = c.T4 - 0x6CE0u;
        MemoryAccess.WriteU8(m, (c.S2 + 0x1Cu), (byte)c.S5);
        MemoryAccess.WriteU8(m, (c.A3 + 0x8u), (byte)c.V0);
        c.V0 = 0x00000007u;
        c.T5 = 0x801C0000u;
        c.T1 = c.T5 - 0x6D40u;
        c.T6 = 0x801C0000u;
        c.T2 = c.T6 - 0x6D30u;
        MemoryAccess.WriteU8(m, (c.A3 + 0x9u), (byte)0u);
        MemoryAccess.WriteU8(m, (c.T0 + 0x8u), (byte)c.V0);
        MemoryAccess.WriteU8(m, (c.T0 + 0x9u), (byte)0u);
        MemoryAccess.WriteU8(m, (c.T1 + 0x8u), (byte)c.S6);
        MemoryAccess.WriteU8(m, (c.T1 + 0x9u), (byte)0u);
        MemoryAccess.WriteU8(m, (c.T2 + 0x8u), (byte)c.S7);
        MemoryAccess.WriteU8(m, (c.T2 + 0x9u), (byte)0u);
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x4u));
        c.V1 = MemoryAccess.ReadU32(m, (c.S0 + 0x8u));
        c.V0 = c.V0 + c.S0;
        c.V1 = c.V1 + c.S0;
        MemoryAccess.WriteU32(m, (c.T3 - 0x6D10u), c.V0);
        MemoryAccess.WriteU32(m, (c.A3 + 0x4u), c.V1);
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0xCu));
        c.V1 = MemoryAccess.ReadU32(m, (c.S0 + 0x10u));
        c.V0 = c.V0 + c.S0;
        c.V1 = c.V1 + c.S0;
        MemoryAccess.WriteU32(m, (c.T4 - 0x6CE0u), c.V0);
        MemoryAccess.WriteU32(m, (c.T0 + 0x4u), c.V1);
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x14u));
        c.V1 = MemoryAccess.ReadU32(m, (c.S0 + 0x18u));
        c.V0 = c.V0 + c.S0;
        c.V1 = c.V1 + c.S0;
        MemoryAccess.WriteU32(m, (c.T5 - 0x6D40u), c.V0);
        MemoryAccess.WriteU32(m, (c.T1 + 0x4u), c.V1);
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x1Cu));
        c.V1 = MemoryAccess.ReadU32(m, (c.S0 + 0x20u));
        c.V0 = c.V0 + c.S0;
        c.V1 = c.V1 + c.S0;
        MemoryAccess.WriteU32(m, (c.T6 - 0x6D30u), c.V0);
        MemoryAccess.WriteU32(m, (c.T2 + 0x4u), c.V1);
        c.RA = 0x80011D50u;
        GranTurismo2ArcadePC.func_8007F9C8(c, m);
        c.S0 = c.S1 + 0x140u;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x00000001u;
        c.A2 = c.S3 + 0u;
        c.RA = 0x80011D64u;
        GranTurismo2ArcadePC.func_8007F9C8(c, m);
        c.A0 = c.S4 + 0u;
        c.RA = 0x80011D6Cu;
        GranTurismo2ArcadePC.func_80083914(c, m);
        c.A0 = c.S0 + 0u;
        c.RA = 0x80011D74u;
        GranTurismo2ArcadePC.func_80083914(c, m);
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 - 0x75A4u;
        c.RA = 0x80011D80u;
        GranTurismo2ArcadePC.func_8006EA64(c, m);
        c.A0 = 0x80030000u;
        c.A0 = c.A0 - 0x3C8u;
        c.A1 = 0x00000160u;
        c.A2 = 0u + 0u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V0 - 0x75A8u), c.A0);
        c.V0 = 0x000001E0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.V0 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V0);
        c.V0 = 0x000001E1u;
        c.A3 = 0x00000080u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.V0);
        c.RA = 0x80011DB8u;
        GranTurismo2ArcadePC.func_8006EB00(c, m);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0x20Cu));
        MemoryAccess.WriteU32(m, (c.S1 + 0x1C8u), 0u);
        MemoryAccess.WriteU16(m, (c.S1 + 0x20Eu), (ushort)0u);
        c.V0 = c.V0 << 2;
        c.V0 = c.S1 + c.V0;
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1CCu));
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0x20Cu));
        c.V0 = c.V0 << 2;
        MemoryAccess.WriteU32(m, (c.S1 + 0x1C4u), c.V1);
        c.S1 = c.S1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x1CCu));
        c.V0 = MemoryAccess.ReadU32(m, c.V0);
        if (c.V0 == 0u) {
            goto L80011E04;
        }
        c.A0 = 0u + 0u;
        c.RA = 0x80011E04u;
        Dispatcher.Call(c, m, c.V0);
        L80011E04: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x40u));
        c.S7 = MemoryAccess.ReadU32(m, (c.SP + 0x3Cu));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x38u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x34u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x30u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x2Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.SP = c.SP + 0x48u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80011E30(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.RA = 0x80011E48u;
        GranTurismo2ArcadePC.func_80080BAC(c, m);
        c.A0 = c.S0 + 0xE8u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x214u));
        c.A1 = c.S0 + 0x1A4u;
        c.V0 = c.V0 + 0x1u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x214u), c.V0);
        c.RA = 0x80011E60u;
        GranTurismo2ArcadePC.func_800838A8(c, m);
        c.A0 = c.S0 + 0x14Cu;
        c.A1 = c.S0 + 0x1B4u;
        c.RA = 0x80011E6Cu;
        GranTurismo2ArcadePC.func_800838A8(c, m);
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 - 0x75A4u;
        c.RA = 0x80011E78u;
        GranTurismo2ArcadePC.func_8006EA74(c, m);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x70u));
        if (c.V0 == 0u) {
            c.S1 = 0x00000001u;
            goto L80011EA4;
        }
        c.S1 = 0x00000001u;
        c.A1 = c.S1 + 0u;
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x78u));
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x74u));
        c.A2 = c.S1 + 0u;
        c.RA = 0x80011EA0u;
        Dispatcher.Call(c, m, c.V0);
        MemoryAccess.WriteU8(m, (c.S0 + 0x70u), (byte)c.V0);
        L80011EA4: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x20Eu));
        c.V1 = MemoryAccess.ReadU16(m, (c.S0 + 0x20Eu));
        if ((int)c.V0 <= 0) {
            c.V0 = c.V1 - 0x1u;
            goto L80011EEC;
        }
        c.V0 = c.V1 - 0x1u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x20Eu), (ushort)c.V0);
        c.V0 = c.V0 << 16;
        if ((int)c.V0 <= 0) {
            goto L80011EEC;
        }
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x1C8u));
        if (c.V0 == 0u) {
            MemoryAccess.WriteU32(m, (c.S0 + 0x1C4u), c.V0);
            goto L80011EEC;
        }
        MemoryAccess.WriteU32(m, (c.S0 + 0x1C4u), c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        if (c.V0 == 0u) {
            goto L80011EEC;
        }
        c.A0 = 0u + 0u;
        c.RA = 0x80011EECu;
        Dispatcher.Call(c, m, c.V0);
        L80011EEC: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x20Cu));
        c.V0 = c.V0 << 2;
        c.V0 = c.S0 + c.V0;
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1CCu));
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x20Cu));
        c.V0 = c.V0 << 2;
        c.V0 = c.S0 + c.V0;
        MemoryAccess.WriteU32(m, (c.S0 + 0x1C4u), c.V1);
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1CCu));
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        if (c.V0 == 0u) {
            c.V1 = 0u + 0u;
            goto L80011F38;
        }
        c.V1 = 0u + 0u;
        c.A0 = 0x00000001u;
        c.RA = 0x80011F34u;
        Dispatcher.Call(c, m, c.V0);
        c.V1 = c.V0 + 0u;
        L80011F38: ;
        c.V0 = c.V1 < 0x00000005u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80020000u;
            goto L80012028;
        }
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0xCD8u;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x80012028u: goto L80012028;
            case 0x80011F60u: goto L80011F60;
            case 0x80011FB8u: goto L80011FB8;
            case 0x80012024u: goto L80012024;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L80011F60: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x20Cu));
        c.V0 = c.V0 << 2;
        c.V0 = c.S0 + c.V0;
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1CCu));
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x20Cu));
        c.V0 = c.V0 << 2;
        c.V0 = c.S0 + c.V0;
        MemoryAccess.WriteU32(m, (c.S0 + 0x1C4u), c.V1);
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1CCu));
        c.V0 = MemoryAccess.ReadU32(m, c.V0);
        if (c.V0 == 0u) {
            goto L80011FA8;
        }
        c.A0 = 0u + 0u;
        c.RA = 0x80011FA8u;
        Dispatcher.Call(c, m, c.V0);
        L80011FA8: ;
        c.V0 = 0x00000010u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x20Eu), (ushort)c.V0);
        MemoryAccess.WriteU8(m, (c.S0 + 0x20Du), (byte)0u);
        goto L80012028;
        L80011FB8: ;
        c.A0 = c.S0 + 0u;
        c.RA = 0x80011FC0u;
        GranTurismo2ArcadePC.func_80012240(c, m);
        if ((int)c.V0 < 0) {
            c.V0 = c.S1 + 0u;
            goto L8001202C;
        }
        c.V0 = c.S1 + 0u;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x20Cu));
        c.V0 = c.V0 << 2;
        c.V0 = c.S0 + c.V0;
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1CCu));
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x20Cu));
        c.V0 = c.V0 << 2;
        c.V0 = c.S0 + c.V0;
        MemoryAccess.WriteU32(m, (c.S0 + 0x1C4u), c.V1);
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1CCu));
        c.V0 = MemoryAccess.ReadU32(m, c.V0);
        if (c.V0 == 0u) {
            goto L80012010;
        }
        c.A0 = 0x00000001u;
        c.RA = 0x80012010u;
        Dispatcher.Call(c, m, c.V0);
        L80012010: ;
        c.V0 = 0x00000010u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x20Eu), (ushort)c.V0);
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x20Du), (byte)c.V0);
        goto L80012028;
        L80012024: ;
        c.S1 = 0u + 0u;
        L80012028: ;
        c.V0 = c.S1 + 0u;
        L8001202C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012040(CpuContext c, IMemory m)
    {
        c.V0 = 0x00000160u;
        MemoryAccess.WriteU16(m, (c.A2 + 0x4u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.A1 + 0x4u), (ushort)c.V0);
        c.V0 = 0x000001E0u;
        MemoryAccess.WriteU16(m, (c.A2 + 0x6u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.A1 + 0x6u), (ushort)c.V0);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.A0 + 0x20Du));
        if (c.V0 != 0u) {
            c.V1 = 0x00000010u;
            goto L800120BC;
        }
        c.V1 = 0x00000010u;
        MemoryAccess.WriteU16(m, c.A2, (ushort)0u);
        MemoryAccess.WriteU16(m, c.A1, (ushort)0u);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0x20Eu));
        c.V0 = c.V1 << 1;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 3;
        if ((int)c.V0 >= 0) {
            goto L80012098;
        }
        c.V0 = c.V0 + 0xFu;
        L80012098: ;
        c.V0 = (uint)((int)c.V0 >> 4);
        MemoryAccess.WriteU16(m, (c.A1 + 0x2u), (ushort)c.V0);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0x20Eu));
        c.V1 = c.V1 - 0x10u;
        c.V0 = c.V1 << 2;
        c.V0 = c.V0 + c.V1;
        MemoryAccess.WriteU16(m, (c.A2 + 0x2u), (ushort)c.V0);
        return;
        L800120BC: ;
        MemoryAccess.WriteU16(m, c.A2, (ushort)0u);
        MemoryAccess.WriteU16(m, c.A1, (ushort)0u);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0x20Eu));
        c.V1 = c.V1 - c.V0;
        c.V0 = c.V1 << 2;
        c.V0 = c.V0 + c.V1;
        MemoryAccess.WriteU16(m, (c.A2 + 0x2u), (ushort)c.V0);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0x20Eu));
        c.V1 = 0u - c.V1;
        c.V0 = c.V1 << 1;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 3;
        if ((int)c.V0 >= 0) {
            goto L80012108;
        }
        c.V0 = c.V0 + 0xFu;
        L80012108: ;
        c.V0 = (uint)((int)c.V0 >> 4);
        MemoryAccess.WriteU16(m, (c.A1 + 0x2u), (ushort)c.V0);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012114(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x38u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S0);
        c.RA = 0x80012134u;
        GranTurismo2ArcadePC.func_80080C14(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.SP + 0x10u;
        c.S2 = c.SP + 0x18u;
        c.A2 = c.S2 + 0u;
        c.RA = 0x80012148u;
        GranTurismo2ArcadePC.func_80012040(c, m);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x20Eu));
        c.A3 = c.V0 << 7;
        if ((int)c.A3 >= 0) {
            goto L80012160;
        }
        c.A3 = c.A3 + 0xFu;
        L80012160: ;
        if ((int)c.V0 <= 0) {
            c.S3 = (uint)((int)c.A3 >> 4);
            goto L8001218C;
        }
        c.S3 = (uint)((int)c.A3 >> 4);
        c.A0 = c.S1 + 0u;
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x1C8u));
        c.S0 = c.S1 + 0xACu;
        c.A2 = c.S0 + 0u;
        c.A3 = c.S3 + 0u;
        c.RA = 0x80012180u;
        GranTurismo2ArcadePC.func_80011888(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S2 + 0u;
        c.RA = 0x8001218Cu;
        GranTurismo2ArcadePC.func_8008025C(c, m);
        L8001218C: ;
        c.A0 = c.S1 + 0u;
        c.S0 = c.S1 + 0x9Cu;
        c.A2 = c.S0 + 0u;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0x20Cu));
        c.A3 = 0x00000080u;
        c.V0 = c.V0 << 2;
        c.V0 = c.S1 + c.V0;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1CCu));
        c.A3 = c.A3 - c.S3;
        c.RA = 0x800121B4u;
        GranTurismo2ArcadePC.func_80011888(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.SP + 0x10u;
        c.RA = 0x800121C0u;
        GranTurismo2ArcadePC.func_8008025C(c, m);
        c.A0 = MemoryAccess.ReadU32(m, (c.S1 + 0x94u));
        c.A1 = 0u + 0u;
        c.RA = 0x800121CCu;
        GranTurismo2ArcadePC.func_8007CF34(c, m);
        c.V1 = 0x00000160u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x4u), (ushort)c.V1);
        c.V1 = 0x801F0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V1 + 0x88u));
        c.V1 = 0x000001E0u;
        MemoryAccess.WriteU16(m, c.V0, (ushort)0u);
        MemoryAccess.WriteU16(m, (c.V0 + 0x2u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.V0 + 0x6u), (ushort)c.V1);
        MemoryAccess.WriteU32(m, (c.S1 + 0x210u), c.A0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x30u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x2Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.SP = c.SP + 0x38u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001220C(CpuContext c, IMemory m)
    {
        c.V1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.A0 + 0x20Cu));
        c.V0 = MemoryAccess.ReadU8(m, (c.A0 + 0x20Cu));
        c.V1 = c.V1 << 2;
        c.V1 = c.A0 + c.V1;
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 + 0x1CCu));
        c.V0 = c.V0 + 0x1u;
        MemoryAccess.WriteU8(m, (c.A0 + 0x20Cu), (byte)c.V0);
        c.V0 = c.V0 << 24;
        c.V0 = (uint)((int)c.V0 >> 22);
        MemoryAccess.WriteU32(m, (c.A0 + 0x1C8u), c.V1);
        c.A0 = c.A0 + c.V0;
        MemoryAccess.WriteU32(m, (c.A0 + 0x1CCu), c.A1);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012240(CpuContext c, IMemory m)
    {
        c.A1 = c.A0 + 0u;
        c.V1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.A1 + 0x20Cu));
        if ((int)c.V1 <= 0) {
            c.V1 = c.V1 << 2;
            goto L80012278;
        }
        c.V1 = c.V1 << 2;
        c.V1 = c.A1 + c.V1;
        c.A0 = MemoryAccess.ReadU8(m, (c.A1 + 0x20Cu));
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 + 0x1CCu));
        c.A0 = c.A0 - 0x1u;
        c.V0 = c.A0 << 24;
        c.V0 = (uint)((int)c.V0 >> 24);
        MemoryAccess.WriteU32(m, (c.A1 + 0x1C8u), c.V1);
        MemoryAccess.WriteU8(m, (c.A1 + 0x20Cu), (byte)c.A0);
        return;
        L80012278: ;
        c.V0 = 0xFFFFFFFFu;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012280(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.S0 = c.A0 + 0u;
        c.RA = 0x80012294u;
        GranTurismo2ArcadePC.func_8007FE00(c, m);
        c.A0 = c.S0 + 0xDCu;
        c.RA = 0x8001229Cu;
        GranTurismo2ArcadePC.func_8007FA48(c, m);
        c.A0 = c.S0 + 0x140u;
        c.RA = 0x800122A4u;
        GranTurismo2ArcadePC.func_8007FA48(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800122B4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.S0 = c.A0 + 0u;
        c.RA = 0x800122C8u;
        GranTurismo2ArcadePC.func_80080BA4(c, m);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x70u));
        if (c.V0 == 0u) {
            c.A1 = 0x00000001u;
            goto L800122F0;
        }
        c.A1 = 0x00000001u;
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x78u));
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x74u));
        c.A2 = 0u + 0u;
        c.RA = 0x800122ECu;
        Dispatcher.Call(c, m, c.V0);
        MemoryAccess.WriteU8(m, (c.S0 + 0x70u), (byte)c.V0);
        L800122F0: ;
        c.V0 = 0x800B0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A8u));
        c.RA = 0x80012300u;
        GranTurismo2ArcadePC.func_8006EBBC(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012310(CpuContext c, IMemory m)
    {
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.A0 + 0x70u));
        if (c.V0 != 0u) {
            c.V0 = 0x00000001u;
            goto L80012334;
        }
        c.V0 = 0x00000001u;
        c.V1 = c.V0 + 0u;
        MemoryAccess.WriteU8(m, (c.A0 + 0x70u), (byte)c.V1);
        MemoryAccess.WriteU32(m, (c.A0 + 0x74u), c.A1);
        MemoryAccess.WriteU32(m, (c.A0 + 0x78u), c.A2);
        return;
        L80012334: ;
        c.V0 = 0u + 0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001233C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x70u));
        if (c.V0 == 0u) {
            c.A1 = 0u + 0u;
            goto L80012370;
        }
        c.A1 = 0u + 0u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x74u));
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x78u));
        c.A2 = 0x00000002u;
        c.RA = 0x8001236Cu;
        Dispatcher.Call(c, m, c.V0);
        MemoryAccess.WriteU8(m, (c.S0 + 0x70u), (byte)0u);
        L80012370: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012380(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A1 = 0x00000001u;
        c.RA = 0x80012390u;
        GranTurismo2ArcadePC.func_80080E34(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800123A0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.RA = 0x800123B0u;
        GranTurismo2ArcadePC.func_8007C480(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800123C0(CpuContext c, IMemory m)
    {
        c.V0 = 0x801C0000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 - 0x6D20u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.V0 - 0x6D20u;
        if (c.V1 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
            goto L800123FC;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.RA = 0x800123E4u;
        GranTurismo2ArcadePC.func_8007FF48(c, m);
        c.A1 = 0x80020000u;
        c.A2 = 0x801F0000u;
        c.A0 = c.S0 + 0u;
        c.A1 = c.A1 + 0xD34u;
        c.A2 = c.A2 + 0x6D0u;
        c.RA = 0x800123FCu;
        GranTurismo2ArcadePC.func_80085A4C(c, m);
        L800123FC: ;
        c.V0 = c.S0 + 0u;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012410_gt2_arcade_overlay_1(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1C4u));
        c.V0 = 0x00000014u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
        if (c.A0 != 0u) {
            MemoryAccess.WriteU32(m, (c.V1 + 0x18u), c.A0);
            goto L80012444;
        }
        MemoryAccess.WriteU32(m, (c.V1 + 0x18u), c.A0);
        c.V0 = 0x800B0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 - 0x759Cu));
        c.RA = 0x80012444u;
        GranTurismo2ArcadePC.func_80020A50(c, m);
        L80012444: ;
        c.A0 = 0u + 0u;
        c.RA = 0x8001244Cu;
        GranTurismo2ArcadePC.func_80012380(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001245C(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V1 = MemoryAccess.ReadU32(m, (c.A0 + 0x1C4u));
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x14u));
        if ((int)c.V0 <= 0) {
            c.A1 = 0u + 0u;
            goto L800124B0;
        }
        c.A1 = 0u + 0u;
        c.V0 = c.V0 - 0x1u;
        if (c.V0 != 0u) {
            MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
            goto L800124B0;
        }
        MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 + 0x18u));
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            c.A1 = 0x00000004u;
            goto L800124B0;
        }
        c.A1 = 0x00000004u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x5718u;
        c.RA = 0x800124ACu;
        GranTurismo2ArcadePC.func_8001220C(c, m);
        c.A1 = 0x00000001u;
        L800124B0: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = c.A1 + 0u;
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800124C0(CpuContext c, IMemory m)
    {
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800124C8(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1C4u));
        c.V0 = 0x00000014u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
        c.RA = 0x800124E8u;
        GranTurismo2ArcadePC.func_800123A0(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800124F8(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1C4u));
        c.V1 = MemoryAccess.ReadU32(m, (c.A0 + 0x14u));
        if ((int)c.V1 <= 0) {
            c.V0 = 0u + 0u;
            goto L8001252C;
        }
        c.V0 = 0u + 0u;
        c.V0 = c.V1 - 0x1u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.V0);
        c.V0 = c.V0 < 0x00000001u ? 1u : 0u;
        c.V0 = c.V0 << 2;
        L8001252C: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012534(CpuContext c, IMemory m)
    {
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001253C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A2 + 0u;
        c.V0 = c.S1 << 2;
        c.V0 = c.V0 + c.S1;
        c.V0 = c.V0 << 2;
        c.V1 = 0x800B0000u;
        c.V1 = c.V1 - 0x7598u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.V0 + c.V1;
        c.V0 = c.A0 < 0x00000009u ? 1u : 0u;
        if (c.V0 == 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
            goto L80012610;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0xD40u;
        c.V1 = c.A0 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x80012590u: goto L80012590;
            case 0x800125C0u: goto L800125C0;
            case 0x800125C8u: goto L800125C8;
            case 0x800125D0u: goto L800125D0;
            case 0x800125E0u: goto L800125E0;
            case 0x80012610u: goto L80012610;
            case 0x80012608u: goto L80012608;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L80012590: ;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x5924u;
        c.RA = 0x800125A0u;
        GranTurismo2ArcadePC.func_800162EC(c, m);
        c.V0 = c.S1 << 1;
        c.V0 = c.V0 + c.S1;
        c.V0 = c.V0 << 2;
        c.V1 = 0x80050000u;
        c.V1 = c.V1 - 0x5988u;
        c.V0 = c.V0 + c.V1;
        MemoryAccess.WriteU32(m, (c.S0 + 0xCu), c.V0);
        goto L80012610;
        L800125C0: ;
        MemoryAccess.WriteU16(m, (c.S0 + 0x10u), (ushort)0u);
        goto L80012610;
        L800125C8: ;
        c.V0 = 0xFFFFFFF3u;
        goto L8001260C;
        L800125D0: ;
        c.A0 = c.S0 + 0u;
        c.RA = 0x800125D8u;
        GranTurismo2ArcadePC.func_80016320(c, m);
        goto L80012610;
        L800125E0: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0xCu));
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0xEu));
        c.A0 = c.S0 + 0u;
        MemoryAccess.WriteU16(m, (c.A0 + 0x4u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.A0 + 0x6u), (ushort)c.V1);
        c.A1 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.A2 = 0u + 0u;
        c.RA = 0x80012600u;
        GranTurismo2ArcadePC.func_80016368(c, m);
        goto L80012610;
        L80012608: ;
        c.V0 = 0x0000000Cu;
        L8001260C: ;
        MemoryAccess.WriteU16(m, (c.S0 + 0x10u), (ushort)c.V0);
        L80012610: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = 0x00000001u;
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012628(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        c.V0 = 0x800B0000u;
        c.A1 = 0x80010000u;
        c.A1 = c.A1 + 0x253Cu;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        c.A2 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S2 = MemoryAccess.ReadU32(m, (c.V0 + 0x1C4u));
        c.V0 = 0x00000014u;
        MemoryAccess.WriteU32(m, (c.S2 + 0x14u), c.V0);
        c.V0 = 0x80050000u;
        c.S1 = c.V0 - 0x5920u;
        c.A0 = c.S1 + 0u;
        c.RA = 0x80012670u;
        GranTurismo2ArcadePC.func_8006CCDC(c, m);
        if (c.S0 == 0u) {
            MemoryAccess.WriteU16(m, (c.S1 + 0x6u), (ushort)0u);
            goto L80012684;
        }
        MemoryAccess.WriteU16(m, (c.S1 + 0x6u), (ushort)0u);
        c.V0 = MemoryAccess.ReadU32(m, (c.S2 + 0x18u));
        MemoryAccess.WriteU16(m, (c.S1 + 0x6u), (ushort)c.V0);
        L80012684: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001269C(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1C4u));
        if (c.A0 != 0u) {
            c.S3 = c.V0 + 0x1A4u;
            goto L800126D4;
        }
        c.S3 = c.V0 + 0x1A4u;
        c.S3 = c.S2 + 0u;
        L800126D4: ;
        c.S0 = MemoryAccess.ReadU32(m, (c.S1 + 0x14u));
        if ((int)c.S0 <= 0) {
            goto L800126F8;
        }
        c.S0 = c.S0 - 0x1u;
        if (c.S0 != 0u) {
            c.A0 = 0x80050000u;
            goto L800126F8;
        }
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x5920u;
        c.RA = 0x800126F8u;
        GranTurismo2ArcadePC.func_8006CD80(c, m);
        L800126F8: ;
        MemoryAccess.WriteU32(m, (c.S1 + 0x14u), c.S0);
        c.V0 = 0x80050000u;
        c.S4 = c.V0 - 0x5920u;
        c.A0 = c.S4 + 0u;
        c.A1 = c.S3 + 0u;
        c.RA = 0x80012710u;
        GranTurismo2ArcadePC.func_8006CED4(c, m);
        c.S0 = c.V0 + 0u;
        c.V0 = 0xFFFFFFFDu;
        if (c.S0 == c.V0) {
            c.V0 = (int)c.S0 < -2 ? 1u : 0u;
            goto L80012754;
        }
        c.V0 = (int)c.S0 < -2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0xFFFFFFFCu;
            goto L80012738;
        }
        c.V0 = 0xFFFFFFFCu;
        if (c.S0 == c.V0) {
            c.V0 = c.S2 + 0u;
            goto L800127BC;
        }
        c.V0 = c.S2 + 0u;
        goto L8001277C;
        L80012738: ;
        c.V0 = 0xFFFFFFFEu;
        if (c.S0 == c.V0) {
            c.V0 = 0xFFFFFFFFu;
            goto L800127B8;
        }
        c.V0 = 0xFFFFFFFFu;
        if (c.S0 == c.V0) {
            goto L80012764;
        }
        goto L8001277C;
        L80012754: ;
        c.A0 = 0x00000005u;
        c.RA = 0x8001275Cu;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.V0 = c.S2 + 0u;
        goto L800127BC;
        L80012764: ;
        c.A0 = 0x00000004u;
        c.RA = 0x8001276Cu;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.A0 = c.S4 + 0u;
        c.RA = 0x80012774u;
        GranTurismo2ArcadePC.func_8006CDE8(c, m);
        c.S2 = 0x00000002u;
        goto L800127B8;
        L8001277C: ;
        c.A0 = 0x00000003u;
        c.RA = 0x80012784u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x5920u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x18u), c.S0);
        c.RA = 0x80012794u;
        GranTurismo2ArcadePC.func_8006CDE8(c, m);
        c.V0 = 0x800B0000u;
        c.V1 = 0x80050000u;
        c.V1 = c.V1 - 0x5934u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        c.V0 = c.S0 << 2;
        c.V0 = c.V0 + c.V1;
        c.A1 = MemoryAccess.ReadU32(m, c.V0);
        c.S2 = 0x00000001u;
        c.RA = 0x800127B8u;
        GranTurismo2ArcadePC.func_8001220C(c, m);
        L800127B8: ;
        c.V0 = c.S2 + 0u;
        L800127BC: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800127DC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x5920u;
        c.A1 = c.A1 + 0x10u;
        c.RA = 0x800127F8u;
        GranTurismo2ArcadePC.func_8006D41C(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012808(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.A2 = 0x80050000u;
        c.A0 = c.A2 - 0x58ECu;
        c.V0 = 0x800B0000u;
        c.V0 = c.V0 - 0x75A4u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU32(m, (c.A0 + 0x8u), c.V0);
        c.V0 = 0x800B0000u;
        c.A1 = 0x00040000u;
        c.V1 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x759Cu));
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 - 0x75A8u));
        c.V0 = c.V0 + c.A1;
        MemoryAccess.WriteU32(m, (c.A2 - 0x58ECu), c.V0);
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.V1);
        c.RA = 0x80012848u;
        GranTurismo2ArcadePC.func_8007275C(c, m);
        c.V1 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V1 - 0x7548u), c.V0);
        c.RA = 0x80012854u;
        GranTurismo2ArcadePC.func_80072DFC(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012864(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 - 0x75A0u));
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        if (c.A0 != 0u) {
            c.V0 = c.V0 + 0x1A4u;
            goto L8001288C;
        }
        c.V0 = c.V0 + 0x1A4u;
        c.V0 = c.S0 + 0u;
        L8001288C: ;
        c.A0 = c.V0 + 0u;
        c.RA = 0x80012894u;
        GranTurismo2ArcadePC.func_80072800(c, m);
        c.V1 = c.V0 + 0u;
        c.A1 = 0x00000001u;
        if (c.V1 == c.A1) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L800128C0;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = c.S0 + 0u;
            goto L800128F8;
        }
        c.V0 = c.S0 + 0u;
        c.V0 = 0x00000002u;
        if (c.V1 == c.V0) {
            c.V0 = c.S0 + 0u;
            goto L800128D0;
        }
        c.V0 = c.S0 + 0u;
        goto L800128F8;
        L800128C0: ;
        c.A0 = 0x00000004u;
        c.RA = 0x800128C8u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.S0 = 0x00000002u;
        goto L800128F4;
        L800128D0: ;
        c.A0 = 0x00000003u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 - 0x75A0u));
        c.S0 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.V0 + 0x21Cu), (byte)c.A1);
        c.RA = 0x800128E4u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.A1 = 0x80050000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.S1 - 0x75A0u));
        c.A1 = c.A1 - 0x576Cu;
        c.RA = 0x800128F4u;
        GranTurismo2ArcadePC.func_8001220C(c, m);
        L800128F4: ;
        c.V0 = c.S0 + 0u;
        L800128F8: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001290C_gt2_arcade_overlay_1(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.A0 = c.A0 + 0x10u;
        c.RA = 0x80012920u;
        GranTurismo2ArcadePC.func_80072A88(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012930(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.A2 = 0x80050000u;
        c.A0 = c.A2 - 0x58ECu;
        c.V0 = 0x800B0000u;
        c.V0 = c.V0 - 0x75A4u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU32(m, (c.A0 + 0x8u), c.V0);
        c.V0 = 0x800B0000u;
        c.A1 = 0x00040000u;
        c.V1 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x759Cu));
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 - 0x75A8u));
        c.V0 = c.V0 + c.A1;
        MemoryAccess.WriteU32(m, (c.A2 - 0x58ECu), c.V0);
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.V1);
        c.RA = 0x80012970u;
        GranTurismo2ArcadePC.func_8007275C(c, m);
        c.V1 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V1 - 0x7544u), c.V0);
        c.RA = 0x8001297Cu;
        GranTurismo2ArcadePC.func_80072E30(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001298C(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        if (c.A0 != 0u) {
            c.V0 = c.V0 + 0x1A4u;
            goto L800129B0;
        }
        c.V0 = c.V0 + 0x1A4u;
        c.V0 = c.S0 + 0u;
        L800129B0: ;
        c.A0 = c.V0 + 0u;
        c.RA = 0x800129B8u;
        GranTurismo2ArcadePC.func_80072800(c, m);
        c.V1 = c.V0 + 0u;
        if (c.V1 == 0u) {
            c.V0 = 0x00000001u;
            goto L800129D8;
        }
        c.V0 = 0x00000001u;
        if (c.V1 != c.V0) {
            c.V0 = c.S0 + 0u;
            goto L800129DC;
        }
        c.V0 = c.S0 + 0u;
        c.A0 = 0x00000004u;
        c.RA = 0x800129D4u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.S0 = 0x00000002u;
        L800129D8: ;
        c.V0 = c.S0 + 0u;
        L800129DC: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800129EC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.A0 = c.A0 + 0x10u;
        c.RA = 0x80012A00u;
        GranTurismo2ArcadePC.func_80072A88(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012A10(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.A2 = 0x80050000u;
        c.A0 = c.A2 - 0x58D0u;
        c.V0 = 0x800B0000u;
        c.V0 = c.V0 - 0x75A4u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU32(m, (c.A0 + 0x8u), c.V0);
        c.V0 = 0x800B0000u;
        c.A1 = 0x00040000u;
        c.V1 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x759Cu));
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 - 0x75A8u));
        c.V0 = c.V0 + c.A1;
        MemoryAccess.WriteU32(m, (c.A2 - 0x58D0u), c.V0);
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.V1);
        c.RA = 0x80012A50u;
        GranTurismo2ArcadePC.func_80015C58(c, m);
        c.V1 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V1 - 0x7540u), c.V0);
        c.RA = 0x80012A5Cu;
        GranTurismo2ArcadePC.func_80015CF8(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012A6C(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        if (c.A0 != 0u) {
            c.V0 = c.V0 + 0x1A4u;
            goto L80012A90;
        }
        c.V0 = c.V0 + 0x1A4u;
        c.V0 = c.S0 + 0u;
        L80012A90: ;
        c.A0 = c.V0 + 0u;
        c.RA = 0x80012A98u;
        GranTurismo2ArcadePC.func_80015D20(c, m);
        c.V1 = c.V0 + 0u;
        if (c.V1 == 0u) {
            c.V0 = 0x00000001u;
            goto L80012AB8;
        }
        c.V0 = 0x00000001u;
        if (c.V1 != c.V0) {
            c.V0 = c.S0 + 0u;
            goto L80012ABC;
        }
        c.V0 = c.S0 + 0u;
        c.A0 = 0x00000004u;
        c.RA = 0x80012AB4u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.S0 = 0x00000002u;
        L80012AB8: ;
        c.V0 = c.S0 + 0u;
        L80012ABC: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012ACC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.A0 = c.A0 + 0x10u;
        c.RA = 0x80012AE0u;
        GranTurismo2ArcadePC.func_80015EAC(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012AF0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x68u;
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x48u), c.S2);
        c.S2 = c.A1 + 0u;
        c.A0 = c.SP + 0x20u;
        c.A1 = 0x0000001Eu;
        c.V1 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x60u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x5Cu), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x58u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x54u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x50u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x4Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.S1);
        c.V0 = MemoryAccess.ReadU32(m, (c.S2 + 0x4u));
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 - 0x759Cu));
        c.S1 = MemoryAccess.ReadU32(m, c.S2);
        c.S3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0xCu));
        c.S4 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0xEu));
        c.S6 = c.V0 + 0x4u;
        c.V0 = c.A2 << 1;
        c.V0 = c.V0 + c.A2;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 - c.A2;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + 0x988u;
        c.S5 = c.V1 + c.V0;
        c.RA = 0x80012B64u;
        GranTurismo2ArcadePC.func_8006AB78(c, m);
        c.S7 = 0x00000001u;
        c.A2 = c.SP + 0x20u;
        c.V0 = 0x00000004u;
        if (c.S0 != c.V0) {
            MemoryAccess.WriteU8(m, (c.A2 + 0x1Cu), (byte)c.S7);
            goto L80012C20;
        }
        MemoryAccess.WriteU8(m, (c.A2 + 0x1Cu), (byte)c.S7);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x26u));
        c.V0 = ~(0u | c.V1);
        c.V0 = c.V0 >> 31;
        c.A0 = (int)c.V1 < -1 ? 1u : 0u;
        c.V0 = c.V0 | c.A0;
        if (c.V0 == 0u) {
            c.V0 = c.S7 + 0u;
            goto L80012C24;
        }
        c.V0 = c.S7 + 0u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x24u));
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x14u));
        c.V0 = c.V0 << 7;
        if (c.V1 != 0u) { if ((int)c.V0 == int.MinValue && (int)c.V1 == -1) { c.LO = 0x80000000u; c.HI = 0u; } else { c.LO = (uint)((int)c.V0 / (int)c.V1); c.HI = (uint)((int)c.V0 % (int)c.V1); } }
        c.V1 = c.LO;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x10u));
        { var _r = (long)(int)c.V1 * (int)c.V0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.T1 = c.LO;
        c.A3 = (uint)((int)c.T1 >> 7);
        if (c.A3 == 0u) {
            c.V0 = 0x00000080u;
            goto L80012C20;
        }
        c.V0 = 0x00000080u;
        c.V1 = c.V0 - c.V1;
        if ((int)c.V1 >= 0) {
            c.V0 = c.V1 << 1;
            goto L80012BDC;
        }
        c.V0 = c.V1 << 1;
        c.V1 = 0u + 0u;
        c.V0 = c.V1 << 1;
        L80012BDC: ;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 - c.V1;
        c.V0 = c.V0 << 4;
        if (c.A0 == 0u) {
            c.V1 = (uint)((int)c.V0 >> 7);
            goto L80012BF8;
        }
        c.V1 = (uint)((int)c.V0 >> 7);
        c.V1 = 0u + 0u;
        L80012BF8: ;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x5880u;
        c.A1 = c.S6 + 0u;
        c.V0 = c.S3 + c.V1;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.A3);
        c.A3 = c.S5 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), 0u);
        c.RA = 0x80012C20u;
        GranTurismo2ArcadePC.func_8006A3F4(c, m);
        L80012C20: ;
        c.V0 = c.S7 + 0u;
        L80012C24: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x60u));
        c.S7 = MemoryAccess.ReadU32(m, (c.SP + 0x5Cu));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x58u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x54u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x50u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x4Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x48u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x44u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x40u));
        c.SP = c.SP + 0x68u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012C50(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        if (c.A0 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
            goto L80012C90;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = 0x800B0000u;
        c.V0 = 0x800B0000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 - 0x759Cu));
        c.V0 = 0x00000018u;
        MemoryAccess.WriteU16(m, (c.A0 - 0x7538u), (ushort)c.V0);
        c.A0 = 0x80050000u;
        c.A1 = 0x80010000u;
        c.A1 = c.A1 + 0x2AF0u;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V1 + 0x200u));
        c.A2 = 0u + 0u;
        MemoryAccess.WriteU16(m, (c.A0 - 0x58B4u), (ushort)c.V0);
        c.A0 = c.A0 - 0x58B4u;
        c.RA = 0x80012C90u;
        GranTurismo2ArcadePC.func_8006CCDC(c, m);
        L80012C90: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012CA0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A0 + 0u;
        c.V0 = 0x800B0000u;
        c.A0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        c.V1 = MemoryAccess.ReadU16(m, (c.A0 - 0x7538u));
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.V0 + 0x1A4u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 - 0x7538u));
        c.S1 = 0u + 0u;
        if ((int)c.V0 <= 0) {
            MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
            goto L80012D00;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        c.V0 = c.V1 - 0x1u;
        MemoryAccess.WriteU16(m, (c.A0 - 0x7538u), (ushort)c.V0);
        c.V0 = c.V0 << 16;
        if (c.V0 != 0u) {
            goto L80012D00;
        }
        c.A0 = 0x00000007u;
        c.RA = 0x80012CF4u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x58B4u;
        c.RA = 0x80012D00u;
        GranTurismo2ArcadePC.func_8006CD80(c, m);
        L80012D00: ;
        if (c.S2 != 0u) {
            c.V0 = 0x80050000u;
            goto L80012D0C;
        }
        c.V0 = 0x80050000u;
        c.S0 = 0u + 0u;
        L80012D0C: ;
        c.S2 = c.V0 - 0x58B4u;
        c.A0 = c.S2 + 0u;
        c.A1 = c.S0 + 0u;
        c.RA = 0x80012D1Cu;
        GranTurismo2ArcadePC.func_8006CED4(c, m);
        c.S0 = c.V0 + 0u;
        c.V0 = 0xFFFFFFFDu;
        if (c.S0 == c.V0) {
            c.V0 = (int)c.S0 < -2 ? 1u : 0u;
            goto L80012D70;
        }
        c.V0 = (int)c.S0 < -2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0xFFFFFFFCu;
            goto L80012D44;
        }
        c.V0 = 0xFFFFFFFCu;
        if (c.S0 == c.V0) {
            goto L80012D80;
        }
        goto L80012D90;
        L80012D44: ;
        c.V0 = 0xFFFFFFFEu;
        if (c.S0 == c.V0) {
            c.V0 = 0xFFFFFFFFu;
            goto L80012DC4;
        }
        c.V0 = 0xFFFFFFFFu;
        if (c.S0 != c.V0) {
            goto L80012D90;
        }
        c.A0 = 0x00000004u;
        c.RA = 0x80012D60u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.A0 = c.S2 + 0u;
        c.RA = 0x80012D68u;
        GranTurismo2ArcadePC.func_8006CDE8(c, m);
        c.S1 = 0x00000002u;
        goto L80012DC4;
        L80012D70: ;
        c.A0 = 0x00000006u;
        c.RA = 0x80012D78u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.V0 = c.S1 + 0u;
        goto L80012DC8;
        L80012D80: ;
        c.A0 = 0u + 0u;
        c.RA = 0x80012D88u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.V0 = c.S1 + 0u;
        goto L80012DC8;
        L80012D90: ;
        c.A0 = 0x00000003u;
        c.RA = 0x80012D98u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x58B4u;
        c.RA = 0x80012DA4u;
        GranTurismo2ArcadePC.func_8006CDE8(c, m);
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x5814u;
        c.S1 = 0x00000001u;
        c.V0 = 0x800B0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V0 - 0x753Cu), c.S0);
        c.RA = 0x80012DC4u;
        GranTurismo2ArcadePC.func_8001220C(c, m);
        L80012DC4: ;
        c.V0 = c.S1 + 0u;
        L80012DC8: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012DE0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x58B4u;
        c.A1 = c.A1 + 0x10u;
        c.RA = 0x80012DFCu;
        GranTurismo2ArcadePC.func_8006D41C(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012E0C(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        c.V1 = MemoryAccess.ReadU32(m, (c.A0 + 0x1C4u));
        c.V0 = 0x00000014u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.A0 + 0x21Cu), (byte)c.V0);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012E30(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1C4u));
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x14u));
        if ((int)c.V0 <= 0) {
            c.A0 = 0u + 0u;
            goto L80012E84;
        }
        c.A0 = 0u + 0u;
        c.V0 = c.V0 - 0x1u;
        if (c.V0 != 0u) {
            MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
            goto L80012E84;
        }
        MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
        c.V0 = 0x800B0000u;
        c.A1 = 0x00020000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 - 0x759Cu));
        c.V0 = 0x800B0000u;
        c.A2 = MemoryAccess.ReadU32(m, (c.V0 - 0x753Cu));
        c.A1 = c.A0 + c.A1;
        c.RA = 0x80012E80u;
        GranTurismo2ArcadePC.func_80020A98(c, m);
        c.A0 = 0x00000004u;
        L80012E84: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = c.A0 + 0u;
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012E94(CpuContext c, IMemory m)
    {
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012E9C(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1C4u));
        c.V0 = 0x00000014u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x18u), c.A0);
        c.A0 = 0x00000007u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
        c.RA = 0x80012EC4u;
        GranTurismo2ArcadePC.func_80012380(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012ED4(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V1 = MemoryAccess.ReadU32(m, (c.A0 + 0x1C4u));
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x14u));
        if ((int)c.V0 <= 0) {
            c.A1 = 0u + 0u;
            goto L80012F28;
        }
        c.A1 = 0u + 0u;
        c.V0 = c.V0 - 0x1u;
        if (c.V0 != 0u) {
            MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
            goto L80012F28;
        }
        MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 + 0x18u));
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            c.A1 = 0x00000004u;
            goto L80012F28;
        }
        c.A1 = 0x00000004u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x5574u;
        c.RA = 0x80012F24u;
        GranTurismo2ArcadePC.func_8001220C(c, m);
        c.A1 = 0x00000001u;
        L80012F28: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = c.A1 + 0u;
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012F38(CpuContext c, IMemory m)
    {
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012F40(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.A2 = 0x80050000u;
        c.A0 = c.A2 - 0x58ECu;
        c.V0 = 0x800B0000u;
        c.V0 = c.V0 - 0x75A4u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU32(m, (c.A0 + 0x8u), c.V0);
        c.V0 = 0x800B0000u;
        c.A1 = 0x00040000u;
        c.V1 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x759Cu));
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 - 0x75A8u));
        c.V0 = c.V0 + c.A1;
        MemoryAccess.WriteU32(m, (c.A2 - 0x58ECu), c.V0);
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.V1);
        c.RA = 0x80012F80u;
        GranTurismo2ArcadePC.func_8007275C(c, m);
        c.A0 = 0u + 0u;
        c.RA = 0x80012F88u;
        GranTurismo2ArcadePC.func_80072EAC(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012F98(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        if (c.A0 != 0u) {
            c.V0 = c.V0 + 0x1A4u;
            goto L80012FBC;
        }
        c.V0 = c.V0 + 0x1A4u;
        c.V0 = c.S0 + 0u;
        L80012FBC: ;
        c.A0 = c.V0 + 0u;
        c.RA = 0x80012FC4u;
        GranTurismo2ArcadePC.func_80072800(c, m);
        c.V1 = c.V0 + 0u;
        if (c.V1 == 0u) {
            c.V0 = 0x00000001u;
            goto L80012FE4;
        }
        c.V0 = 0x00000001u;
        if (c.V1 != c.V0) {
            c.V0 = c.S0 + 0u;
            goto L80012FE8;
        }
        c.V0 = c.S0 + 0u;
        c.A0 = 0x00000004u;
        c.RA = 0x80012FE0u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.S0 = 0x00000002u;
        L80012FE4: ;
        c.V0 = c.S0 + 0u;
        L80012FE8: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012FF8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.A0 = c.A0 + 0x10u;
        c.RA = 0x8001300Cu;
        GranTurismo2ArcadePC.func_80072A88(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001301C(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1C4u));
        c.V0 = 0x00000014u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x18u), c.A0);
        c.A0 = 0x00000007u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
        c.RA = 0x80013044u;
        GranTurismo2ArcadePC.func_80012380(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80013054(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V1 = MemoryAccess.ReadU32(m, (c.A0 + 0x1C4u));
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x14u));
        if ((int)c.V0 <= 0) {
            c.A1 = 0u + 0u;
            goto L800130A8;
        }
        c.A1 = 0u + 0u;
        c.V0 = c.V0 - 0x1u;
        if (c.V0 != 0u) {
            MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
            goto L800130A8;
        }
        MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 + 0x18u));
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            c.A1 = 0x00000004u;
            goto L800130A8;
        }
        c.A1 = 0x00000004u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x54CCu;
        c.RA = 0x800130A4u;
        GranTurismo2ArcadePC.func_8001220C(c, m);
        c.A1 = 0x00000001u;
        L800130A8: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = c.A1 + 0u;
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800130B8(CpuContext c, IMemory m)
    {
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800130C0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.A2 = 0x80050000u;
        c.A0 = c.A2 - 0x58ECu;
        c.V0 = 0x800B0000u;
        c.V0 = c.V0 - 0x75A4u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU32(m, (c.A0 + 0x8u), c.V0);
        c.V0 = 0x800B0000u;
        c.A1 = 0x00040000u;
        c.V1 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x759Cu));
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 - 0x75A8u));
        c.V0 = c.V0 + c.A1;
        MemoryAccess.WriteU32(m, (c.A2 - 0x58ECu), c.V0);
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.V1);
        c.RA = 0x80013100u;
        GranTurismo2ArcadePC.func_8007275C(c, m);
        c.A0 = 0u + 0u;
        c.RA = 0x80013108u;
        GranTurismo2ArcadePC.func_80072F20(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80013118(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        if (c.A0 != 0u) {
            c.V0 = c.V0 + 0x1A4u;
            goto L8001313C;
        }
        c.V0 = c.V0 + 0x1A4u;
        c.V0 = c.S0 + 0u;
        L8001313C: ;
        c.A0 = c.V0 + 0u;
        c.RA = 0x80013144u;
        GranTurismo2ArcadePC.func_80072800(c, m);
        c.V1 = c.V0 + 0u;
        if (c.V1 == 0u) {
            c.V0 = 0x00000001u;
            goto L80013164;
        }
        c.V0 = 0x00000001u;
        if (c.V1 != c.V0) {
            c.V0 = c.S0 + 0u;
            goto L80013168;
        }
        c.V0 = c.S0 + 0u;
        c.A0 = 0x00000004u;
        c.RA = 0x80013160u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.S0 = 0x00000002u;
        L80013164: ;
        c.V0 = c.S0 + 0u;
        L80013168: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80013178(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.A0 = c.A0 + 0x10u;
        c.RA = 0x8001318Cu;
        GranTurismo2ArcadePC.func_80072A88(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001319C(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V1 = 0x801D0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        c.V1 = MemoryAccess.ReadU8(m, (c.V1 - 0x6CC0u));
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S2 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        if (c.V1 != 0u) {
            goto L80013220;
        }
        c.A0 = c.S2 + 0u;
        c.RA = 0x800131D4u;
        GranTurismo2ArcadePC.func_8007F084(c, m);
        c.A0 = c.S2 + 0u;
        c.S1 = 0x801C0000u;
        c.S1 = c.S1 - 0x5F89u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x800131E8u;
        GranTurismo2ArcadePC.func_8006EBE8(c, m);
        c.A0 = c.S2 + 0u;
        c.S0 = 0x801F0000u;
        c.S0 = c.S0 - 0x87Au;
        c.A1 = c.S0 + 0u;
        c.RA = 0x800131FCu;
        GranTurismo2ArcadePC.func_8006EBE8(c, m);
        c.A0 = c.S2 + 0u;
        c.A1 = c.S0 - 0x11u;
        c.RA = 0x80013208u;
        GranTurismo2ArcadePC.func_8006EBE8(c, m);
        c.A0 = c.S2 + 0u;
        c.A1 = c.S1 + 0xFu;
        c.RA = 0x80013214u;
        GranTurismo2ArcadePC.func_8006EBE8(c, m);
        c.A0 = c.S2 + 0u;
        c.A1 = c.S1 + 0x24u;
        c.RA = 0x80013220u;
        GranTurismo2ArcadePC.func_8006EBE8(c, m);
        L80013220: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80013238(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V0 = 0x800B0000u;
        c.V1 = 0x801D0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        c.V1 = MemoryAccess.ReadU8(m, (c.V1 - 0x6CC0u));
        c.A1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        if (c.V1 != 0u) {
            goto L80013268;
        }
        c.RA = 0x80013268u;
        GranTurismo2ArcadePC.func_8006EBE8(c, m);
        L80013268: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80013278(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x30u;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S6);
        c.S6 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = c.A3 + 0u;
        c.V0 = 0x800B0000u;
        c.V1 = 0x801D0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S5);
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x40u));
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        c.V1 = MemoryAccess.ReadU8(m, (c.V1 - 0x6CC0u));
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S4 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        if (c.V1 != 0u) {
            c.A0 = 0u + 0u;
            goto L80013374;
        }
        c.A0 = 0u + 0u;
        c.V0 = MemoryAccess.ReadU8(m, c.A1);
        if (c.V0 == 0u) {
            c.V1 = c.A1 + 0u;
            goto L800132EC;
        }
        c.V1 = c.A1 + 0u;
        L800132D8: ;
        c.V1 = c.V1 + 0x2u;
        c.V0 = MemoryAccess.ReadU8(m, c.V1);
        if (c.V0 != 0u) {
            c.A0 = c.A0 + 0xCu;
            goto L800132D8;
        }
        c.A0 = c.A0 + 0xCu;
        L800132EC: ;
        c.V0 = (uint)((int)c.A0 >> 1);
        c.S1 = c.S1 - c.V0;
        c.S0 = c.A1 + 0u;
        c.S2 = c.S3 + 0x1Fu;
        L800132FC: ;
        c.V0 = MemoryAccess.ReadU8(m, c.S0);
        if (c.V0 == 0u) {
            c.A0 = c.S4 + 0u;
            goto L80013374;
        }
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.V0 = MemoryAccess.ReadU8(m, (c.S0 + 0x1u));
        c.A1 = c.A1 << 8;
        c.A1 = c.A1 | c.V0;
        c.RA = 0x80013320u;
        GranTurismo2ArcadePC.func_8007F09C(c, m);
        if (c.V0 == 0u) {
            c.A0 = c.V0 + 0u;
            goto L80013368;
        }
        c.A0 = c.V0 + 0u;
        c.A1 = c.S6 + 0u;
        c.A2 = c.S5 + 0u;
        c.RA = 0x80013334u;
        GranTurismo2ArcadePC.func_8007EF2C(c, m);
        c.V1 = MemoryAccess.ReadU16(m, (c.V0 + 0x12u));
        c.A0 = c.S1 + 0xFu;
        MemoryAccess.WriteU16(m, (c.V0 + 0x4u), (ushort)c.S1);
        MemoryAccess.WriteU16(m, (c.V0 + 0x6u), (ushort)c.S3);
        MemoryAccess.WriteU16(m, (c.V0 + 0xCu), (ushort)c.A0);
        MemoryAccess.WriteU16(m, (c.V0 + 0xEu), (ushort)c.S3);
        MemoryAccess.WriteU16(m, (c.V0 + 0x14u), (ushort)c.S1);
        MemoryAccess.WriteU16(m, (c.V0 + 0x16u), (ushort)c.S2);
        MemoryAccess.WriteU16(m, (c.V0 + 0x1Cu), (ushort)c.A0);
        MemoryAccess.WriteU16(m, (c.V0 + 0x1Eu), (ushort)c.S2);
        c.V1 = c.V1 & 0xFF9Fu;
        c.V1 = c.V1 | 0x0020u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x12u), (ushort)c.V1);
        L80013368: ;
        c.S1 = c.S1 + 0xCu;
        c.S0 = c.S0 + 0x2u;
        goto L800132FC;
        L80013374: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x2Cu));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x30u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001339C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V1 = 0x801D0000u;
        c.V1 = MemoryAccess.ReadU8(m, (c.V1 - 0x6CC0u));
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        if (c.V1 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
            goto L800133DC;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.RA = 0x800133C4u;
        GranTurismo2ArcadePC.func_8001319C(c, m);
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x24u));
        c.RA = 0x800133D0u;
        GranTurismo2ArcadePC.func_80013238(c, m);
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x28u));
        c.RA = 0x800133DCu;
        GranTurismo2ArcadePC.func_80013238(c, m);
        L800133DC: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800133EC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V1 = 0x801D0000u;
        c.V1 = MemoryAccess.ReadU8(m, (c.V1 - 0x6CC0u));
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        if (c.V1 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
            goto L80013420;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.RA = 0x80013414u;
        GranTurismo2ArcadePC.func_8001319C(c, m);
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x20u));
        c.RA = 0x80013420u;
        GranTurismo2ArcadePC.func_80013238(c, m);
        L80013420: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80013430(CpuContext c, IMemory m)
    {
        c.A1 = 0u + 0u;
        c.V1 = c.A1 + 0u;
        c.V0 = 0x800B0000u;
        c.A2 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        c.A0 = 0x00000988u;
        c.A3 = c.A2 + 0x1A1Cu;
        L80013448: ;
        c.V0 = (int)c.V1 < 32 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.A2 + c.V1;
            goto L8001347C;
        }
        c.V0 = c.A2 + c.V1;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V0 + 0x2FA8u));
        if (c.V0 == 0u) {
            c.V0 = c.A3 + c.A0;
            goto L80013470;
        }
        c.V0 = c.A3 + c.A0;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x52u));
        c.A1 = c.A1 + c.V0;
        L80013470: ;
        c.A0 = c.A0 + 0x5Cu;
        c.V1 = c.V1 + 0x1u;
        goto L80013448;
        L8001347C: ;
        c.V0 = c.A1 + 0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80013484(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        c.A0 = 0u + 0u;
        c.V1 = c.A0 + 0u;
        c.V0 = c.A1 + c.V1;
        L80013498: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V0 + 0x2FA8u));
        if (c.V0 == 0u) {
            goto L800134AC;
        }
        c.A0 = c.A0 + 0x1u;
        L800134AC: ;
        c.V1 = c.V1 + 0x1u;
        c.V0 = (int)c.V1 < 32 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = c.A1 + c.V1;
            goto L80013498;
        }
        c.V0 = c.A1 + c.V1;
        c.V0 = c.A0 + 0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800134C4(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V0 + 0x68Cu));
        c.RA = 0x800134E4u;
        GranTurismo2ArcadePC.func_80013484(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.V0 = c.S0 + c.V0;
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800134F8(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x68Eu));
        c.A0 = c.V0 + 0x690u;
        c.RA = 0x80013518u;
        GranTurismo2ArcadePC.func_80068EF8(c, m);
        c.S0 = c.S0 - c.V0;
        c.RA = 0x80013520u;
        GranTurismo2ArcadePC.func_80013430(c, m);
        c.V0 = c.S0 - c.V0;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80013534(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x68Eu));
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001354C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        c.S1 = c.A1 + 0u;
        c.RA = 0x80013574u;
        GranTurismo2ArcadePC.func_800134F8(c, m);
        c.V1 = c.V0 + 0u;
        MemoryAccess.WriteU32(m, c.S1, 0u);
        c.V0 = c.S2 + c.S0;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V0 + 0x2FA8u));
        c.A0 = 0x00000001u;
        if (c.V0 != c.A0) {
            c.V0 = c.S0 << 1;
            goto L80013598;
        }
        c.V0 = c.S0 << 1;
        c.V0 = 0x00000001u;
        goto L800135E8;
        L80013598: ;
        c.V0 = c.V0 + c.S0;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 - c.S0;
        c.V0 = c.V0 << 2;
        c.V0 = c.S2 + c.V0;
        MemoryAccess.WriteU32(m, c.S1, c.A0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x23F6u));
        c.V1 = c.V1 - c.V0;
        if ((int)c.V1 < 0) {
            c.V0 = 0u + 0u;
            goto L800135E8;
        }
        c.V0 = 0u + 0u;
        c.V0 = 0x00000002u;
        MemoryAccess.WriteU32(m, c.S1, c.V0);
        c.RA = 0x800135D0u;
        GranTurismo2ArcadePC.func_800134C4(c, m);
        c.V1 = 0x00000020u;
        if (c.V0 == c.V1) {
            c.V0 = 0x00000001u;
            goto L800135E4;
        }
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, c.S1, 0u);
        goto L800135E8;
        L800135E4: ;
        c.V0 = 0u + 0u;
        L800135E8: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80013600(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        c.V1 = c.V0 + c.A0;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V1 + 0x2FA8u));
        c.A0 = 0x00000001u;
        if (c.V0 != c.A0) {
            goto L80013628;
        }
        MemoryAccess.WriteU8(m, (c.V1 + 0x2FA8u), (byte)0u);
        return;
        L80013628: ;
        MemoryAccess.WriteU8(m, (c.V1 + 0x2FA8u), (byte)c.A0);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80013630(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        c.V1 = 0x00000020u;
        c.V0 = c.V0 + c.V1;
        L80013640: ;
        MemoryAccess.WriteU8(m, (c.V0 + 0x2FA8u), (byte)0u);
        c.V1 = c.V1 - 0x1u;
        if ((int)c.V1 >= 0) {
            c.V0 = c.V0 - 0x1u;
            goto L80013640;
        }
        c.V0 = c.V0 - 0x1u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80013658_gt2_arcade_overlay_1(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0x68Eu));
        c.S0 = c.A0 + 0x1A18u;
        MemoryAccess.WriteU16(m, (c.A0 + 0x1A18u), (ushort)c.V0);
        c.A0 = c.A0 + 0x690u;
        c.RA = 0x80013680u;
        GranTurismo2ArcadePC.func_80068EF8(c, m);
        MemoryAccess.WriteU16(m, (c.S0 + 0x2u), (ushort)c.V0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80013694(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x168u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x15Cu), c.S7);
        c.S7 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        MemoryAccess.WriteU32(m, (c.SP + 0x140u), c.S0);
        c.S0 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x164u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x160u), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0x158u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x154u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x150u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x14Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x148u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x144u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x168u), c.A0);
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S7 + 0x2Cu));
        c.A0 = c.SP + 0x20u;
        c.RA = 0x800136DCu;
        GranTurismo2ArcadePC.func_8006AB78(c, m);
        c.V1 = 0xFF9F0000u;
        c.V1 = c.V1 | 0xFFFFu;
        c.S6 = c.SP + 0x20u;
        c.A0 = c.S6 + 0u;
        c.A1 = 0x800B0000u;
        c.A1 = c.A1 - 0x7528u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S6 + 0xCu));
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x168u));
        c.V0 = c.V0 & c.V1;
        c.V1 = 0x00200000u;
        c.V0 = c.V0 | c.V1;
        MemoryAccess.WriteU32(m, (c.S6 + 0x10u), c.T0);
        MemoryAccess.WriteU32(m, (c.S6 + 0xCu), c.V0);
        c.RA = 0x80013714u;
        GranTurismo2ArcadePC.func_8007D990(c, m);
        c.V0 = 0x024A0000u;
        c.V0 = c.V0 | 0x4136u;
        c.FP = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.S6 + 0x1Cu), (byte)c.FP);
        if (c.S0 == 0u) {
            MemoryAccess.WriteU32(m, (c.S6 + 0x14u), c.V0);
            goto L80013818;
        }
        MemoryAccess.WriteU32(m, (c.S6 + 0x14u), c.V0);
        c.S2 = c.SP + 0x40u;
        c.A0 = c.S2 + 0u;
        c.S0 = c.S7 + 0x48Cu;
        c.S1 = 0x801F0000u;
        c.S1 = c.S1 - 0xA7Eu;
        c.A2 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x200u));
        c.A1 = c.S1 + 0u;
        c.RA = 0x8001374Cu;
        GranTurismo2ArcadePC.func_8008CE44(c, m);
        c.A0 = c.S6 + 0u;
        c.A1 = c.S2 + 0u;
        c.A2 = c.FP + 0u;
        c.A3 = 0u + 0u;
        c.RA = 0x80013760u;
        GranTurismo2ArcadePC.func_8006AF54(c, m);
        c.A0 = c.S6 + 0u;
        c.A1 = c.S2 + 0u;
        c.S5 = 0x00000140u;
        c.A2 = c.S5 - c.V0;
        c.A3 = 0x0000008Au;
        c.S4 = 0xFFFFFFFEu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.RA = 0x80013788u;
        GranTurismo2ArcadePC.func_8006AE50(c, m);
        c.S3 = c.S7 + 0x690u;
        c.S0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x202u));
        c.A0 = c.S3 + 0u;
        c.RA = 0x80013798u;
        GranTurismo2ArcadePC.func_80068EF8(c, m);
        c.S0 = c.S0 - c.V0;
        c.RA = 0x800137A0u;
        GranTurismo2ArcadePC.func_80013534(c, m);
        c.A0 = c.S2 + 0u;
        c.A1 = c.S1 + 0x19u;
        c.A2 = c.S0 + 0u;
        c.A3 = c.V0 + 0u;
        c.RA = 0x800137B4u;
        GranTurismo2ArcadePC.func_8008CE44(c, m);
        c.A0 = c.S6 + 0u;
        c.A1 = c.S2 + 0u;
        c.A2 = c.FP + 0u;
        c.A3 = 0u + 0u;
        c.RA = 0x800137C8u;
        GranTurismo2ArcadePC.func_8006AF54(c, m);
        c.A0 = c.S6 + 0u;
        c.A1 = c.S2 + 0u;
        c.A2 = c.S5 - c.V0;
        c.A3 = 0x0000009Eu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.RA = 0x800137E8u;
        GranTurismo2ArcadePC.func_8006AE50(c, m);
        c.A0 = c.S3 + 0u;
        c.RA = 0x800137F0u;
        GranTurismo2ArcadePC.func_80068EF8(c, m);
        c.A0 = c.S7 + 0x1A18u;
        c.A2 = 0x000000B0u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0x168u));
        c.A3 = 0x000000AAu;
        MemoryAccess.WriteU16(m, (c.A0 + 0x2u), (ushort)c.V0);
        c.V0 = 0x00000080u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        goto L800138F4;
        L80013818: ;
        c.S1 = 0x801F0000u;
        c.RA = 0x80013820u;
        GranTurismo2ArcadePC.func_800134C4(c, m);
        c.S2 = c.SP + 0x40u;
        c.A0 = c.S2 + 0u;
        c.S1 = c.S1 - 0xA7Eu;
        c.A1 = c.S1 + 0u;
        c.A2 = c.V0 + 0u;
        c.RA = 0x80013838u;
        GranTurismo2ArcadePC.func_8008CE44(c, m);
        c.A0 = c.S6 + 0u;
        c.A1 = c.S2 + 0u;
        c.A2 = 0x00000001u;
        c.A3 = 0u + 0u;
        c.RA = 0x8001384Cu;
        GranTurismo2ArcadePC.func_8006AF54(c, m);
        c.A0 = c.S6 + 0u;
        c.A1 = c.S2 + 0u;
        c.S4 = 0x00000140u;
        c.A2 = c.S4 - c.V0;
        c.A3 = 0x0000008Au;
        c.S3 = 0xFFFFFFFEu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.RA = 0x80013874u;
        GranTurismo2ArcadePC.func_8006AE50(c, m);
        c.RA = 0x8001387Cu;
        GranTurismo2ArcadePC.func_800134F8(c, m);
        c.S0 = c.V0 + 0u;
        c.RA = 0x80013884u;
        GranTurismo2ArcadePC.func_80013534(c, m);
        c.A0 = c.S2 + 0u;
        c.A1 = c.S1 + 0x19u;
        c.A2 = c.S0 + 0u;
        c.A3 = c.V0 + 0u;
        c.RA = 0x80013898u;
        GranTurismo2ArcadePC.func_8008CE44(c, m);
        c.A0 = c.S6 + 0u;
        c.A1 = c.S2 + 0u;
        c.A2 = 0x00000001u;
        c.A3 = 0u + 0u;
        c.RA = 0x800138ACu;
        GranTurismo2ArcadePC.func_8006AF54(c, m);
        c.A0 = c.S6 + 0u;
        c.A1 = c.S2 + 0u;
        c.A2 = c.S4 - c.V0;
        c.A3 = 0x0000009Eu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.RA = 0x800138CCu;
        GranTurismo2ArcadePC.func_8006AE50(c, m);
        c.RA = 0x800138D4u;
        GranTurismo2ArcadePC.func_80013430(c, m);
        c.A0 = c.S7 + 0x1A18u;
        c.A2 = 0x000000B0u;
        c.A3 = 0x000000AAu;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0x168u));
        c.V1 = 0x00000080u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V1);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.V0);
        L800138F4: ;
        c.RA = 0x800138FCu;
        GranTurismo2ArcadePC.func_8006A978(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x164u));
        c.FP = MemoryAccess.ReadU32(m, (c.SP + 0x160u));
        c.S7 = MemoryAccess.ReadU32(m, (c.SP + 0x15Cu));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x158u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x154u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x150u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x14Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x148u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x144u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x140u));
        c.SP = c.SP + 0x168u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001392C(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        c.SP = c.SP - 0x150u;
        MemoryAccess.WriteU32(m, (c.SP + 0x144u), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x148u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x140u), c.S0);
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x2Cu));
        c.A0 = c.SP + 0x20u;
        c.RA = 0x80013958u;
        GranTurismo2ArcadePC.func_8006AB78(c, m);
        c.V1 = 0xFF9F0000u;
        c.V1 = c.V1 | 0xFFFFu;
        c.S0 = c.SP + 0x20u;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0xCu));
        c.A1 = c.A1 - 0x7528u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x10u), c.S1);
        c.V0 = c.V0 & c.V1;
        c.V1 = 0x00200000u;
        c.V0 = c.V0 | c.V1;
        MemoryAccess.WriteU32(m, (c.S0 + 0xCu), c.V0);
        c.RA = 0x8001398Cu;
        GranTurismo2ArcadePC.func_8007D990(c, m);
        c.V0 = 0x024A0000u;
        c.V0 = c.V0 | 0x4136u;
        c.S2 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x1Cu), (byte)c.S2);
        MemoryAccess.WriteU32(m, (c.S0 + 0x14u), c.V0);
        c.RA = 0x800139A4u;
        GranTurismo2ArcadePC.func_80013484(c, m);
        c.S1 = c.SP + 0x40u;
        c.A0 = c.S1 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x68CDu;
        c.A2 = c.V0 + 0u;
        c.RA = 0x800139BCu;
        GranTurismo2ArcadePC.func_8008CE44(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0u;
        c.A2 = c.S2 + 0u;
        c.A3 = 0u + 0u;
        c.RA = 0x800139D0u;
        GranTurismo2ArcadePC.func_8006AF54(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0u;
        c.A2 = 0x00000140u;
        c.A2 = c.A2 - c.V0;
        c.A3 = 0x000000E0u;
        c.V0 = 0xFFFFFFFEu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.RA = 0x800139F8u;
        GranTurismo2ArcadePC.func_8006AE50(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x148u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x144u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x140u));
        c.SP = c.SP + 0x150u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80013A10(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x98u;
        MemoryAccess.WriteU32(m, (c.SP + 0x78u), c.S2);
        c.S2 = c.A0 + 0u;
        c.V1 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x74u), c.S1);
        c.S1 = 0x00000001u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x88u), c.S6);
        c.S6 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        c.A0 = c.SP + 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x94u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x90u), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0x8Cu), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x84u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x80u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x7Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x70u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0xA0u), c.A2);
        MemoryAccess.WriteU32(m, (c.SP + 0x5Cu), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x60u), 0u);
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.S0 = MemoryAccess.ReadU32(m, c.V1);
        c.T1 = (uint)(short)MemoryAccess.ReadU16(m, (c.V1 + 0xCu));
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S6 + 0x2Cu));
        MemoryAccess.WriteU32(m, (c.SP + 0x58u), c.T1);
        c.FP = (uint)(short)MemoryAccess.ReadU16(m, (c.V1 + 0xEu));
        c.S3 = (uint)(short)MemoryAccess.ReadU16(m, (c.V1 + 0x10u));
        c.S5 = c.V0 + 0x4u;
        c.RA = 0x80013A84u;
        GranTurismo2ArcadePC.func_8006AB78(c, m);
        c.S4 = c.SP + 0x20u;
        c.V0 = 0x00000004u;
        if (c.S2 == c.V0) {
            MemoryAccess.WriteU8(m, (c.S4 + 0x1Cu), (byte)c.S1);
            goto L80013AB8;
        }
        MemoryAccess.WriteU8(m, (c.S4 + 0x1Cu), (byte)c.S1);
        c.V0 = (int)c.S2 < 5 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = 0x00000006u;
            goto L80013D4C;
        }
        c.V0 = 0x00000006u;
        if (c.S2 == c.V0) {
            c.V0 = 0x00000008u;
            goto L80013D4C;
        }
        c.V0 = 0x00000008u;
        if (c.S2 == c.V0) {
            goto L80013D20;
        }
        goto L80013D4C;
        L80013AB8: ;
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0xA0u));
        c.V0 = c.T1 << 1;
        c.V0 = c.V0 + c.T1;
        c.T1 = c.V0 << 1;
        MemoryAccess.WriteU32(m, (c.SP + 0x64u), c.V0);
        c.V0 = c.T1 + c.S6;
        MemoryAccess.WriteU32(m, (c.SP + 0x68u), c.T1);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x39Au));
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x26u));
        c.V0 = c.V0 ^ 0x0001u;
        c.S1 = c.V0 < 0x00000001u ? 1u : 0u;
        c.V0 = ~(0u | c.V1);
        c.V0 = c.V0 >> 31;
        c.V1 = (int)c.V1 < -1 ? 1u : 0u;
        c.V0 = c.V0 | c.V1;
        if (c.V0 == 0u) {
            goto L80013D4C;
        }
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x24u));
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x14u));
        c.V0 = c.V0 << 7;
        if (c.V1 != 0u) { if ((int)c.V0 == int.MinValue && (int)c.V1 == -1) { c.LO = 0x80000000u; c.HI = 0u; } else { c.LO = (uint)((int)c.V0 / (int)c.V1); c.HI = (uint)((int)c.V0 % (int)c.V1); } }
        c.V0 = c.LO;
        { var _r = (long)(int)c.V0 * (int)c.S3; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V1 = c.LO;
        c.S7 = (uint)((int)c.V1 >> 7);
        if (c.S7 == 0u) {
            c.V1 = 0x005A0000u;
            goto L80013D4C;
        }
        c.V1 = 0x005A0000u;
        if (c.S1 != 0u) {
            c.V1 = c.V1 | 0x4A3Eu;
            goto L80013B40;
        }
        c.V1 = c.V1 | 0x4A3Eu;
        c.V1 = 0x00080000u;
        c.V1 = c.V1 | 0x082Au;
        L80013B40: ;
        c.T1 = 0x80050000u;
        c.T1 = c.T1 - 0x52CCu;
        MemoryAccess.WriteU32(m, (c.T1 + 0x10u), c.V1);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x24u));
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x14u));
        c.V0 = c.V0 << 7;
        if (c.V1 != 0u) { if ((int)c.V0 == int.MinValue && (int)c.V1 == -1) { c.LO = 0x80000000u; c.HI = 0u; } else { c.LO = (uint)((int)c.V0 / (int)c.V1); c.HI = (uint)((int)c.V0 % (int)c.V1); } }
        c.V0 = c.LO;
        c.V1 = 0x00000080u;
        c.S3 = c.V1 - c.V0;
        if ((int)c.S3 >= 0) {
            c.V0 = c.S3 << 1;
            goto L80013B78;
        }
        c.V0 = c.S3 << 1;
        c.S3 = 0u + 0u;
        c.V0 = c.S3 << 1;
        L80013B78: ;
        c.V0 = c.V0 + c.S3;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 - c.S3;
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0xA0u));
        c.V0 = c.V0 << 4;
        c.V1 = c.S6 + c.T1;
        c.V1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V1 + 0x2FA8u));
        if (c.V1 == 0u) {
            c.S3 = (uint)((int)c.V0 >> 7);
            goto L80013CA4;
        }
        c.S3 = (uint)((int)c.V0 >> 7);
        c.V0 = 0x005A0000u;
        c.V0 = c.V0 | 0x8C32u;
        c.A0 = c.S4 + 0u;
        c.A1 = 0x800B0000u;
        c.A1 = c.A1 - 0x7518u;
        MemoryAccess.WriteU32(m, (c.SP + 0x50u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x54u), c.V0);
        c.RA = 0x80013BC0u;
        GranTurismo2ArcadePC.func_8007D990(c, m);
        c.T0 = 0xFF9F0000u;
        c.T0 = c.T0 | 0xFFFFu;
        c.V0 = 0x020A0000u;
        c.V0 = c.V0 | 0x5A5Au;
        c.A0 = c.S4 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x687Au;
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x58u));
        c.V1 = MemoryAccess.ReadU32(m, (c.S4 + 0xCu));
        c.A3 = c.FP + 0x18u;
        MemoryAccess.WriteU32(m, (c.S4 + 0x14u), c.V0);
        c.V0 = 0x00200000u;
        MemoryAccess.WriteU32(m, (c.S4 + 0x10u), c.S5);
        c.S0 = c.T1 + c.S3;
        c.A2 = c.S0 - 0x98u;
        c.V1 = c.V1 & c.T0;
        c.V1 = c.V1 | c.V0;
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.S4 + 0xCu), c.V1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.RA = 0x80013C14u;
        GranTurismo2ArcadePC.func_8006ABA0(c, m);
        c.S2 = c.SP + 0x50u;
        c.A0 = c.S2 + 0u;
        c.S1 = c.SP + 0x54u;
        c.A1 = c.S1 + 0u;
        c.A2 = c.S7 + 0u;
        c.A3 = 0x00000080u;
        c.S0 = c.S0 - 0x9Cu;
        c.V0 = c.V0 + 0x8u;
        MemoryAccess.WriteU16(m, (c.SP + 0x44u), (ushort)c.V0);
        c.V0 = c.FP - 0x4u;
        MemoryAccess.WriteU16(m, (c.SP + 0x42u), (ushort)c.V0);
        c.V0 = 0x00000022u;
        MemoryAccess.WriteU16(m, (c.SP + 0x40u), (ushort)c.S0);
        MemoryAccess.WriteU16(m, (c.SP + 0x46u), (ushort)c.V0);
        c.RA = 0x80013C50u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S2 + 0u;
        c.A2 = 0x00000020u;
        c.A3 = 0x00000080u;
        MemoryAccess.WriteU32(m, (c.SP + 0x54u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x48u), c.V0);
        c.RA = 0x80013C6Cu;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S5 + 0u;
        c.S0 = c.SP + 0x40u;
        c.A1 = c.S0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x4Cu), c.V0);
        c.RA = 0x80013C80u;
        GranTurismo2ArcadePC.func_8006B5F4(c, m);
        c.A0 = c.S5 + 0u;
        c.A1 = 0x00000220u;
        c.RA = 0x80013C8Cu;
        GranTurismo2ArcadePC.func_8007D954(c, m);
        c.A0 = c.S5 + 0u;
        c.A1 = c.S0 + 0u;
        c.RA = 0x80013C98u;
        GranTurismo2ArcadePC.func_8006B5F4(c, m);
        c.A0 = c.S5 + 0u;
        c.A1 = 0x00000240u;
        c.RA = 0x80013CA4u;
        GranTurismo2ArcadePC.func_8007D954(c, m);
        L80013CA4: ;
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x68u));
        c.V0 = c.T1 + c.S6;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x398u));
        if (c.V1 != 0u) {
            goto L80013CE8;
        }
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x64u));
        c.V0 = c.T1 << 3;
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0xA0u));
        c.V0 = c.V0 - c.T1;
        c.V0 = c.V0 << 2;
        c.V0 = c.S6 + c.V0;
        c.V0 = c.V0 + 0x23A4u;
        MemoryAccess.WriteU32(m, (c.SP + 0x60u), c.V0);
        L80013CE8: ;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x52CCu;
        c.A1 = c.S5 + 0u;
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0x60u));
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x58u));
        c.A2 = c.S4 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.V1);
        c.V0 = c.T1 + c.S3;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.RA = 0x80013D18u;
        GranTurismo2ArcadePC.func_8006A3F4(c, m);
        goto L80013D4C;
        L80013D20: ;
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0xA0u));
        c.V0 = c.T1 << 1;
        c.V0 = c.V0 + c.T1;
        c.V0 = c.V0 << 1;
        c.V0 = c.V0 + c.S6;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x39Au));
        c.V0 = c.V0 ^ 0x0001u;
        c.V0 = c.V0 < 0x00000001u ? 1u : 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x5Cu), c.V0);
        L80013D4C: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x5Cu));
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x94u));
        c.FP = MemoryAccess.ReadU32(m, (c.SP + 0x90u));
        c.S7 = MemoryAccess.ReadU32(m, (c.SP + 0x8Cu));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x88u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x84u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x80u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x7Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x78u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x74u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x70u));
        c.SP = c.SP + 0x98u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80013D80(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.V1 = MemoryAccess.ReadU32(m, (c.A1 + 0xCu));
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.V1);
        if (c.A0 == 0u) {
            c.S0 = c.V0 < 0x00000001u ? 1u : 0u;
            goto L80013DC0;
        }
        c.S0 = c.V0 < 0x00000001u ? 1u : 0u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V1 + 0x2u));
        if (c.V0 == 0u) {
            c.S0 = c.S0 & 0x0001u;
            goto L80013DDC;
        }
        c.S0 = c.S0 & 0x0001u;
        c.S0 = 0u + 0u;
        goto L80013DDC;
        L80013DC0: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V1 + 0x2u));
        c.V1 = c.V0 < 0x00000001u ? 1u : 0u;
        c.V0 = c.V0 ^ 0x0003u;
        c.V0 = c.V0 < 0x00000001u ? 1u : 0u;
        c.V1 = c.V1 | c.V0;
        c.S0 = c.S0 & c.V1;
        L80013DDC: ;
        if (c.S0 == 0u) {
            c.A0 = c.A1 + 0x1A1Cu;
            goto L80013DF8;
        }
        c.A0 = c.A1 + 0x1A1Cu;
        c.A1 = 0u + 0u;
        c.RA = 0x80013DECu;
        GranTurismo2ArcadePC.func_800695D4(c, m);
        if (c.V0 != 0u) {
            c.S0 = c.S0 & 0x0001u;
            goto L80013DF8;
        }
        c.S0 = c.S0 & 0x0001u;
        c.S0 = 0u + 0u;
        L80013DF8: ;
        c.V0 = c.S0 + 0u;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80013E0C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x30u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S5);
        c.S5 = c.A2 + 0u;
        c.V1 = 0x800B0000u;
        c.V0 = 0x80020000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = MemoryAccess.ReadU32(m, (c.V1 - 0x7530u));
        c.V0 = c.V0 + 0xD68u;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        MemoryAccess.WriteU32(m, c.S1, c.V0);
        MemoryAccess.WriteU32(m, c.S3, c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.S2 + 0xCu));
        c.S4 = 0u + 0u;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, c.V0);
        c.V0 = c.V1 < 0x00000005u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.S0 = c.S4 + 0u;
            goto L80013EDC;
        }
        c.S0 = c.S4 + 0u;
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0xD70u;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x80013EB0u: goto L80013EB0;
            case 0x80013ECCu: goto L80013ECC;
            case 0x80013E8Cu: goto L80013E8C;
            case 0x80013E98u: goto L80013E98;
            case 0x80013EA4u: goto L80013EA4;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L80013E8C: ;
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6CD0u;
        goto L80013ED4;
        L80013E98: ;
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6A9Au;
        goto L80013ED4;
        L80013EA4: ;
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6C28u;
        goto L80013ED4;
        L80013EB0: ;
        c.A0 = c.S2 + 0x1A1Cu;
        c.A1 = 0u + 0u;
        c.RA = 0x80013EBCu;
        GranTurismo2ArcadePC.func_800695D4(c, m);
        if (c.V0 != 0u) {
            c.V0 = 0x801C0000u;
            goto L80013EDC;
        }
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6A9Au;
        goto L80013ED4;
        L80013ECC: ;
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6BD2u;
        L80013ED4: ;
        MemoryAccess.WriteU32(m, c.S1, c.V0);
        c.S0 = 0x00000001u;
        L80013EDC: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S2 + 0xCu));
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x2u));
        c.V0 = 0x00000002u;
        if (c.V1 == c.V0) {
            c.A0 = c.V1 + 0u;
            goto L80013F2C;
        }
        c.A0 = c.V1 + 0u;
        c.V0 = (int)c.V1 < 3 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000001u;
            goto L80013F10;
        }
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            c.V0 = 0x801C0000u;
            goto L80013F50;
        }
        c.V0 = 0x801C0000u;
        goto L80013F5C;
        L80013F10: ;
        c.V0 = 0x00000003u;
        if (c.A0 == c.V0) {
            c.V0 = 0x00000004u;
            goto L80013F38;
        }
        c.V0 = 0x00000004u;
        if (c.A0 == c.V0) {
            c.V0 = 0x801C0000u;
            goto L80013F48;
        }
        c.V0 = 0x801C0000u;
        goto L80013F5C;
        L80013F2C: ;
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6CABu;
        goto L80013F54;
        L80013F38: ;
        if (c.S5 == 0u) {
            c.V0 = 0x801C0000u;
            goto L80013F5C;
        }
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6C57u;
        goto L80013F54;
        L80013F48: ;
        c.V0 = c.V0 - 0x6BFDu;
        goto L80013F54;
        L80013F50: ;
        c.V0 = c.V0 - 0x6BAEu;
        L80013F54: ;
        MemoryAccess.WriteU32(m, c.S3, c.V0);
        c.S0 = 0x00000001u;
        L80013F5C: ;
        if (c.S0 != 0u) {
            c.V0 = c.S4 + 0u;
            goto L80013F6C;
        }
        c.V0 = c.S4 + 0u;
        c.S4 = 0x00000001u;
        c.V0 = c.S4 + 0u;
        L80013F6C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x30u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80013F90(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        c.V0 = 0x00000001u;
        if (c.A0 == c.V0) {
            c.A1 = 0xFFFFFFFFu;
            goto L80013FC0;
        }
        c.A1 = 0xFFFFFFFFu;
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L80013FE0;
        }
        if (c.A0 != 0u) {
            c.V0 = 0x00000018u;
            goto L80013FE0;
        }
        c.V0 = 0x00000018u;
        MemoryAccess.WriteU16(m, (c.V1 + 0x2Eu), (ushort)c.V0);
        goto L80013FE0;
        L80013FC0: ;
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 + 0x2Eu));
        c.V0 = c.V0 - 0x1u;
        MemoryAccess.WriteU16(m, (c.V1 + 0x2Eu), (ushort)c.V0);
        c.V0 = c.V0 << 16;
        if ((int)c.V0 >= 0) {
            goto L80013FE0;
        }
        c.A1 = 0x00000002u;
        L80013FE0: ;
        c.V0 = c.A1 + 0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80013FE8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x50u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x3Cu), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.S3);
        c.S3 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x38u), c.S0);
        c.S0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x48u), c.RA);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0xF4u));
        if (c.A0 == c.S0) {
            c.S2 = 0xFFFFFFFFu;
            goto L8001407C;
        }
        c.S2 = 0xFFFFFFFFu;
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L80014038;
        }
        c.V0 = 0x00000002u;
        if (c.A0 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L80014048;
        }
        c.V0 = c.S2 + 0u;
        goto L8001415C;
        L80014038: ;
        if (c.A0 == c.V0) {
            c.V0 = c.S2 + 0u;
            goto L800140BC;
        }
        c.V0 = c.S2 + 0u;
        goto L8001415C;
        L80014048: ;
        c.A0 = 0u + 0u;
        c.RA = 0x80014050u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.S0 = c.S1 + 0x60u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x8001405Cu;
        GranTurismo2ArcadePC.func_8006E298(c, m);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0xD68u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x7Du), (byte)0u);
        MemoryAccess.WriteU32(m, (c.S1 + 0x24u), c.V0);
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        c.RA = 0x80014074u;
        GranTurismo2ArcadePC.func_8001339C(c, m);
        c.V0 = c.S2 + 0u;
        goto L8001415C;
        L8001407C: ;
        if (c.V0 == 0u) {
            goto L800140AC;
        }
        if ((int)c.V0 > 0) {
            goto L8001409C;
        }
        if (c.V0 == c.S2) {
            c.V0 = c.S2 + 0u;
            goto L800140A4;
        }
        c.V0 = c.S2 + 0u;
        goto L8001415C;
        L8001409C: ;
        if (c.V0 != c.S0) {
            c.V0 = c.S2 + 0u;
            goto L8001415C;
        }
        c.V0 = c.S2 + 0u;
        L800140A4: ;
        c.S2 = 0x00000016u;
        goto L80014158;
        L800140AC: ;
        c.A0 = 0x00000001u;
        c.RA = 0x800140B4u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.S2 = 0x00000002u;
        goto L80014158;
        L800140BC: ;
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x2Cu));
        c.A0 = c.SP + 0x18u;
        c.RA = 0x800140C8u;
        GranTurismo2ArcadePC.func_8006AB78(c, m);
        c.A0 = c.SP + 0x18u;
        c.A1 = 0x800B0000u;
        c.A1 = c.A1 - 0x7518u;
        c.RA = 0x800140D8u;
        GranTurismo2ArcadePC.func_8007D990(c, m);
        c.A0 = 0xFF9F0000u;
        c.A0 = c.A0 | 0xFFFFu;
        c.V0 = 0x022A0000u;
        c.A1 = c.SP + 0x18u;
        c.V1 = MemoryAccess.ReadU32(m, (c.A1 + 0xCu));
        c.V0 = c.V0 | 0x485Cu;
        MemoryAccess.WriteU32(m, (c.A1 + 0x14u), c.V0);
        c.V0 = 0x801D0000u;
        MemoryAccess.WriteU8(m, (c.A1 + 0x1Cu), (byte)c.S0);
        MemoryAccess.WriteU32(m, (c.A1 + 0x10u), c.S3);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6CC0u));
        c.V1 = c.V1 & c.A0;
        c.A0 = 0x00200000u;
        c.V1 = c.V1 | c.A0;
        if (c.V0 != 0u) {
            MemoryAccess.WriteU32(m, (c.A1 + 0xCu), c.V1);
            goto L80014140;
        }
        MemoryAccess.WriteU32(m, (c.A1 + 0xCu), c.V1);
        c.V0 = 0x02600000u;
        c.V0 = c.V0 | 0x6060u;
        c.A0 = c.S3 + 0u;
        c.A2 = 0x000000B0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x20u));
        c.A3 = 0x00000112u;
        c.RA = 0x80014138u;
        GranTurismo2ArcadePC.func_80013278(c, m);
        c.V0 = c.S2 + 0u;
        goto L8001415C;
        L80014140: ;
        c.A0 = c.A1 + 0u;
        c.A2 = 0x000000B0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x20u));
        c.A3 = 0x00000122u;
        c.RA = 0x80014158u;
        GranTurismo2ArcadePC.func_8006ACC4(c, m);
        L80014158: ;
        c.V0 = c.S2 + 0u;
        L8001415C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x48u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x44u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x40u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x3Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x38u));
        c.SP = c.SP + 0x50u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80014178(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x30u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S1);
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S2);
        c.S3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x224u));
        if (c.A0 == c.V0) {
            c.S1 = 0xFFFFFFFFu;
            goto L80014218;
        }
        c.S1 = 0xFFFFFFFFu;
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S1 + 0u;
            goto L80014374;
        }
        c.V0 = c.S1 + 0u;
        if (c.A0 != 0u) {
            c.V1 = c.S0 + 0x30u;
            goto L80014374;
        }
        c.V1 = c.S0 + 0x30u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V1 + 0x18u));
        if ((int)c.V0 < 0) {
            goto L800141F0;
        }
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 + 0x4u));
        c.V0 = ~(0u | c.V0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x18u), (ushort)c.V0);
        c.V1 = c.S0 + 0x4Cu;
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 + 0x10u));
        c.V0 = ~(0u | c.V0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x12u), (ushort)c.V0);
        L800141F0: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0xCu));
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x2u));
        c.V0 = 0x00000003u;
        if (c.V1 != c.V0) {
            goto L80014210;
        }
        c.S1 = 0x00000003u;
        goto L80014370;
        L80014210: ;
        c.A0 = c.S0 + 0x190u;
        c.RA = 0x80014218u;
        GranTurismo2ArcadePC.func_8006E298(c, m);
        L80014218: ;
        c.A0 = c.SP + 0x10u;
        c.A1 = c.SP + 0x14u;
        c.A2 = 0x00000001u;
        c.RA = 0x80014228u;
        GranTurismo2ArcadePC.func_80013E0C(c, m);
        c.S2 = c.V0 + 0u;
        if (c.S2 != 0u) {
            c.V0 = 0x801C0000u;
            goto L80014248;
        }
        c.V0 = 0x801C0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        MemoryAccess.WriteU32(m, (c.S0 + 0x24u), c.V0);
        MemoryAccess.WriteU32(m, (c.S0 + 0x28u), c.V1);
        goto L80014258;
        L80014248: ;
        c.V0 = c.V0 - 0x6ADEu;
        MemoryAccess.WriteU32(m, (c.S0 + 0x24u), c.V0);
        c.V0 = c.V0 + 0x19u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x28u), c.V0);
        L80014258: ;
        c.RA = 0x80014260u;
        GranTurismo2ArcadePC.func_8001339C(c, m);
        c.V0 = 0x02800000u;
        c.V0 = c.V0 | 0x400Cu;
        c.V1 = c.S0 + 0x190u;
        if (c.S2 != 0u) {
            MemoryAccess.WriteU32(m, (c.V1 + 0x8Cu), c.V0);
            goto L8001428C;
        }
        MemoryAccess.WriteU32(m, (c.V1 + 0x8Cu), c.V0);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V1 + 0x7Du));
        if (c.V0 != 0u) {
            c.V0 = 0x02080000u;
            goto L8001428C;
        }
        c.V0 = 0x02080000u;
        c.V0 = c.V0 | 0x0830u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x8Cu), c.V0);
        L8001428C: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0xCu));
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x2u));
        if (c.V1 == 0u) {
            c.V0 = 0x00000003u;
            goto L800142C4;
        }
        c.V0 = 0x00000003u;
        if (c.V1 != c.V0) {
            goto L800142E8;
        }
        c.A0 = 0x00000007u;
        c.RA = 0x800142B4u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.A0 = c.S0 + 0x190u;
        c.RA = 0x800142BCu;
        GranTurismo2ArcadePC.func_8006E30C(c, m);
        c.S1 = 0x00000003u;
        goto L80014370;
        L800142C4: ;
        c.A0 = c.S0 + 0x48Cu;
        c.A1 = 0x00000001u;
        c.RA = 0x800142D0u;
        GranTurismo2ArcadePC.func_800695D4(c, m);
        if (c.V0 != 0u) {
            goto L800142E8;
        }
        c.A0 = c.S0 + 0x190u;
        c.RA = 0x800142E0u;
        GranTurismo2ArcadePC.func_8006E30C(c, m);
        c.S1 = 0x00000005u;
        goto L80014370;
        L800142E8: ;
        if (c.S3 == 0u) {
            goto L80014324;
        }
        if ((int)c.S3 > 0) {
            c.V0 = 0x00000001u;
            goto L8001430C;
        }
        c.V0 = 0x00000001u;
        c.V0 = 0xFFFFFFFFu;
        if (c.S3 == c.V0) {
            c.V0 = c.S1 + 0u;
            goto L80014314;
        }
        c.V0 = c.S1 + 0u;
        goto L80014374;
        L8001430C: ;
        if (c.S3 != c.V0) {
            c.V0 = c.S1 + 0u;
            goto L80014374;
        }
        c.V0 = c.S1 + 0u;
        L80014314: ;
        c.A0 = c.S0 + 0x190u;
        c.RA = 0x8001431Cu;
        GranTurismo2ArcadePC.func_8006E30C(c, m);
        c.S1 = 0x00000016u;
        goto L80014370;
        L80014324: ;
        c.A0 = 0x00000001u;
        c.RA = 0x8001432Cu;
        GranTurismo2ArcadePC.func_80013D80(c, m);
        if (c.V0 == 0u) {
            goto L80014368;
        }
        c.A0 = 0x00000001u;
        c.RA = 0x8001433Cu;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.A0 = c.S0 + 0x190u;
        c.RA = 0x80014344u;
        GranTurismo2ArcadePC.func_8006E30C(c, m);
        c.S1 = 0x00000009u;
        c.RA = 0x8001434Cu;
        GranTurismo2ArcadePC.func_80013630(c, m);
        c.A0 = c.S0 + 0x48Cu;
        c.A1 = 0x00000001u;
        c.RA = 0x80014358u;
        GranTurismo2ArcadePC.func_800695D4(c, m);
        if (c.V0 != 0u) {
            c.V0 = c.S1 + 0u;
            goto L80014374;
        }
        c.V0 = c.S1 + 0u;
        c.S1 = 0x00000005u;
        goto L80014370;
        L80014368: ;
        c.A0 = 0u + 0u;
        c.RA = 0x80014370u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        L80014370: ;
        c.V0 = c.S1 + 0u;
        L80014374: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.SP = c.SP + 0x30u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80014390(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        c.S4 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x2BCu));
        if (c.A0 == c.S4) {
            c.S3 = 0xFFFFFFFFu;
            goto L80014408;
        }
        c.S3 = 0xFFFFFFFFu;
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S3 + 0u;
            goto L80014470;
        }
        c.V0 = c.S3 + 0u;
        if (c.A0 != 0u) {
            c.S0 = c.S1 + 0x228u;
            goto L80014470;
        }
        c.S0 = c.S1 + 0x228u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x800143E0u;
        GranTurismo2ArcadePC.func_8006E298(c, m);
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6A40u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x7Du), (byte)c.S4);
        MemoryAccess.WriteU32(m, (c.S1 + 0x24u), c.V0);
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0xDF3u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        c.RA = 0x80014400u;
        GranTurismo2ArcadePC.func_8001339C(c, m);
        MemoryAccess.WriteU16(m, (c.S1 + 0x48u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.S1 + 0x5Eu), (ushort)0u);
        L80014408: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0xCu));
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x2u));
        c.V0 = 0x00000003u;
        if (c.V1 == c.V0) {
            goto L80014430;
        }
        c.A0 = c.S1 + 0x228u;
        c.RA = 0x80014428u;
        GranTurismo2ArcadePC.func_8006E30C(c, m);
        c.S3 = 0x00000002u;
        goto L8001446C;
        L80014430: ;
        if (c.S2 == 0u) {
            goto L80014460;
        }
        if ((int)c.S2 > 0) {
            goto L80014450;
        }
        if (c.S2 == c.S3) {
            c.V0 = c.S3 + 0u;
            goto L80014458;
        }
        c.V0 = c.S3 + 0u;
        goto L80014470;
        L80014450: ;
        if (c.S2 != c.S4) {
            c.V0 = c.S3 + 0u;
            goto L80014470;
        }
        c.V0 = c.S3 + 0u;
        L80014458: ;
        c.S3 = 0x00000016u;
        goto L8001446C;
        L80014460: ;
        c.A0 = 0x00000001u;
        c.RA = 0x80014468u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.S3 = 0x00000004u;
        L8001446C: ;
        c.V0 = c.S3 + 0u;
        L80014470: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80014490(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = 0xFFFFFFFFu;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        c.V0 = 0x00000001u;
        if (c.S0 == c.V0) {
            MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
            goto L80014500;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        c.V0 = (int)c.S0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L8001453C;
        }
        c.V0 = c.S2 + 0u;
        if (c.S0 != 0u) {
            goto L8001453C;
        }
        c.A0 = 0x00000001u;
        c.RA = 0x800144D8u;
        GranTurismo2ArcadePC.func_8007F4B8(c, m);
        if (c.V0 != 0u) {
            c.V0 = 0x801C0000u;
            goto L80014534;
        }
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6303u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x24u), c.V0);
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0xE85u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        c.RA = 0x800144F8u;
        GranTurismo2ArcadePC.func_8001339C(c, m);
        c.V0 = c.S2 + 0u;
        goto L8001453C;
        L80014500: ;
        c.RA = 0x80014508u;
        GranTurismo2ArcadePC.func_8007D6DC(c, m);
        if (c.V0 == 0u) {
            goto L80014520;
        }
        if (c.V0 != c.S0) {
            c.V0 = 0x801C0000u;
            goto L80014528;
        }
        c.V0 = 0x801C0000u;
        c.V0 = c.S2 + 0u;
        goto L8001453C;
        L80014520: ;
        c.S2 = 0x00000005u;
        goto L80014538;
        L80014528: ;
        c.V0 = c.V0 - 0x62D8u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x20u), c.V0);
        c.RA = 0x80014534u;
        GranTurismo2ArcadePC.func_800133EC(c, m);
        L80014534: ;
        c.S2 = 0x00000001u;
        L80014538: ;
        c.V0 = c.S2 + 0u;
        L8001453C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80014554(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x356u));
        if (c.A0 == c.V0) {
            c.S1 = 0xFFFFFFFFu;
            goto L800145F0;
        }
        c.S1 = 0xFFFFFFFFu;
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S1 + 0u;
            goto L8001465C;
        }
        c.V0 = c.S1 + 0u;
        if (c.A0 != 0u) {
            goto L8001465C;
        }
        c.V0 = 0x801F0000u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V0 + 0x5D2u));
        c.V0 = (int)c.A0 < 3 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80050000u;
            goto L800145B0;
        }
        c.V0 = 0x80050000u;
        c.S1 = 0x00000006u;
        goto L80014658;
        L800145B0: ;
        c.V0 = c.V0 - 0x5388u;
        c.V1 = 0x00000003u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x6u), (ushort)c.V1);
        MemoryAccess.WriteU16(m, (c.V0 + 0x8u), (ushort)c.A0);
        MemoryAccess.WriteU16(m, (c.V0 + 0x4u), (ushort)c.V1);
        MemoryAccess.WriteU16(m, (c.V0 + 0x1Cu), (ushort)0u);
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x62A9u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x24u), c.V0);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0xD68u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x28u), c.V0);
        c.RA = 0x800145E4u;
        GranTurismo2ArcadePC.func_8001339C(c, m);
        MemoryAccess.WriteU16(m, (c.S0 + 0x48u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.S0 + 0x5Eu), (ushort)0u);
        goto L80014658;
        L800145F0: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0xCu));
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x2u));
        if (c.V0 == 0u) {
            c.V0 = 0x80050000u;
            goto L80014614;
        }
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU16(m, (c.V0 - 0x536Cu), (ushort)c.S1);
        c.S1 = 0x00000002u;
        goto L80014658;
        L80014614: ;
        c.V0 = (int)c.A1 < -3 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V1 = 0x80050000u;
            goto L80014644;
        }
        c.V1 = 0x80050000u;
        c.V0 = (int)c.A1 < -1 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = c.S1 + 0u;
            goto L8001465C;
        }
        c.V0 = c.S1 + 0u;
        if (c.A1 != c.S1) {
            c.V0 = 0xFFFFFFFFu;
            goto L80014648;
        }
        c.V0 = 0xFFFFFFFFu;
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU16(m, (c.V0 - 0x536Cu), (ushort)c.S1);
        c.S1 = 0x00000016u;
        goto L80014658;
        L80014644: ;
        c.V0 = 0xFFFFFFFFu;
        L80014648: ;
        MemoryAccess.WriteU16(m, (c.V1 - 0x536Cu), (ushort)c.V0);
        c.A0 = c.S0 + 0x48Cu;
        c.RA = 0x80014654u;
        GranTurismo2ArcadePC.func_8006902C(c, m);
        c.S1 = 0x00000007u;
        L80014658: ;
        c.V0 = c.S1 + 0u;
        L8001465C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80014670(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0xF4u));
        if (c.A0 == c.V0) {
            c.S2 = 0xFFFFFFFFu;
            goto L800146EC;
        }
        c.S2 = 0xFFFFFFFFu;
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L80014754;
        }
        c.V0 = c.S2 + 0u;
        if (c.A0 != 0u) {
            goto L80014754;
        }
        c.A0 = 0u + 0u;
        c.RA = 0x800146B8u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.S0 = c.S1 + 0x60u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x800146C4u;
        GranTurismo2ArcadePC.func_8006E298(c, m);
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6A11u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x7Du), (byte)0u);
        MemoryAccess.WriteU32(m, (c.S1 + 0x24u), c.V0);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0xD68u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        c.RA = 0x800146E4u;
        GranTurismo2ArcadePC.func_8001339C(c, m);
        c.V0 = c.S2 + 0u;
        goto L80014754;
        L800146EC: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0xCu));
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x2u));
        if (c.V0 == 0u) {
            goto L80014714;
        }
        c.A0 = c.S1 + 0x60u;
        c.RA = 0x8001470Cu;
        GranTurismo2ArcadePC.func_8006E30C(c, m);
        c.S2 = 0x00000002u;
        goto L80014750;
        L80014714: ;
        if (c.V1 == 0u) {
            goto L80014744;
        }
        if ((int)c.V1 > 0) {
            goto L80014734;
        }
        if (c.V1 == c.S2) {
            c.V0 = c.S2 + 0u;
            goto L8001473C;
        }
        c.V0 = c.S2 + 0u;
        goto L80014754;
        L80014734: ;
        if (c.V1 != c.A0) {
            c.V0 = c.S2 + 0u;
            goto L80014754;
        }
        c.V0 = c.S2 + 0u;
        L8001473C: ;
        c.S2 = 0x00000016u;
        goto L80014750;
        L80014744: ;
        c.A0 = 0x00000001u;
        c.RA = 0x8001474Cu;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.S2 = 0x00000002u;
        L80014750: ;
        c.V0 = c.S2 + 0u;
        L80014754: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001476C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = 0xFFFFFFFFu;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        c.V0 = 0x00000001u;
        if (c.S0 == c.V0) {
            MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
            goto L800147E0;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        c.V0 = (int)c.S0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L8001481C;
        }
        c.V0 = c.S2 + 0u;
        if (c.S0 != 0u) {
            c.A0 = c.S1 + 0x48Cu;
            goto L8001481C;
        }
        c.A0 = c.S1 + 0x48Cu;
        c.A1 = 0x00000001u;
        c.RA = 0x800147B4u;
        GranTurismo2ArcadePC.func_800695FC(c, m);
        if (c.V0 == 0u) {
            c.V0 = 0x801C0000u;
            goto L800147C0;
        }
        c.V0 = 0x801C0000u;
        c.S2 = 0x00000001u;
        L800147C0: ;
        c.V0 = c.V0 - 0x6278u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x24u), c.V0);
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0xE85u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        c.RA = 0x800147D8u;
        GranTurismo2ArcadePC.func_8001339C(c, m);
        c.V0 = c.S2 + 0u;
        goto L8001481C;
        L800147E0: ;
        c.RA = 0x800147E8u;
        GranTurismo2ArcadePC.func_8007D6DC(c, m);
        if (c.V0 == 0u) {
            goto L80014800;
        }
        if (c.V0 != c.S0) {
            c.V0 = 0x801C0000u;
            goto L80014808;
        }
        c.V0 = 0x801C0000u;
        c.V0 = c.S2 + 0u;
        goto L8001481C;
        L80014800: ;
        c.S2 = 0x00000008u;
        goto L80014818;
        L80014808: ;
        c.V0 = c.V0 - 0x6243u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x20u), c.V0);
        c.RA = 0x80014814u;
        GranTurismo2ArcadePC.func_800133EC(c, m);
        c.S2 = 0x00000001u;
        L80014818: ;
        c.V0 = c.S2 + 0u;
        L8001481C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80014834(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = 0xFFFFFFFFu;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        c.V0 = 0x00000001u;
        if (c.S0 == c.V0) {
            MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
            goto L800148F4;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        c.V0 = (int)c.S0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L8001487C;
        }
        c.V0 = 0x00000002u;
        if (c.S0 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L8001488C;
        }
        c.V0 = c.S2 + 0u;
        goto L80014954;
        L8001487C: ;
        if (c.S0 == c.V0) {
            c.V0 = c.S2 + 0u;
            goto L80014948;
        }
        c.V0 = c.S2 + 0u;
        goto L80014954;
        L8001488C: ;
        c.S0 = c.S1 + 0x48Cu;
        c.A0 = c.S0 + 0u;
        c.RA = 0x80014898u;
        GranTurismo2ArcadePC.func_800692FC(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x00000001u;
        c.RA = 0x800148A4u;
        GranTurismo2ArcadePC.func_8006962C(c, m);
        if (c.V0 == 0u) {
            c.V0 = 0x801C0000u;
            goto L800148B4;
        }
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6A69u;
        goto L80014938;
        L800148B4: ;
        c.V1 = 0x000C0000u;
        c.V1 = c.V1 | 0x5090u;
        c.A0 = c.S1 + 0x35Cu;
        c.V0 = 0x0000150Cu;
        MemoryAccess.WriteU32(m, (c.A0 + 0x10u), c.V0);
        MemoryAccess.WriteU32(m, (c.A0 + 0xCu), c.V1);
        c.RA = 0x800148D0u;
        GranTurismo2ArcadePC.func_8006BF5C(c, m);
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6328u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x24u), c.V0);
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0xE85u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        c.RA = 0x800148ECu;
        GranTurismo2ArcadePC.func_8001339C(c, m);
        c.V0 = c.S2 + 0u;
        goto L80014954;
        L800148F4: ;
        c.RA = 0x800148FCu;
        GranTurismo2ArcadePC.func_8007D6DC(c, m);
        if (c.V0 == 0u) {
            goto L80014924;
        }
        if (c.V0 != c.S0) {
            c.V0 = 0x801C0000u;
            goto L80014930;
        }
        c.V0 = 0x801C0000u;
        c.V0 = 0x801F0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0xC8u));
        c.A0 = c.S1 + 0x35Cu;
        c.RA = 0x8001491Cu;
        GranTurismo2ArcadePC.func_8006BF84(c, m);
        c.V0 = c.S2 + 0u;
        goto L80014954;
        L80014924: ;
        MemoryAccess.WriteU16(m, (c.S1 + 0x390u), (ushort)c.S2);
        c.S2 = 0x00000002u;
        goto L80014950;
        L80014930: ;
        c.V0 = c.V0 - 0x6A69u;
        MemoryAccess.WriteU16(m, (c.S1 + 0x390u), (ushort)c.S2);
        L80014938: ;
        MemoryAccess.WriteU32(m, (c.S1 + 0x20u), c.V0);
        c.RA = 0x80014940u;
        GranTurismo2ArcadePC.func_800133EC(c, m);
        c.S2 = 0x00000001u;
        goto L80014950;
        L80014948: ;
        c.A0 = c.S1 + 0x35Cu;
        c.RA = 0x80014950u;
        GranTurismo2ArcadePC.func_8006C084(c, m);
        L80014950: ;
        c.V0 = c.S2 + 0u;
        L80014954: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001496C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = 0xFFFFFFFFu;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = 0x00000001u;
        if (c.S2 == c.S3) {
            MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
            goto L80014A44;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        c.V0 = (int)c.S2 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L800149B8;
        }
        c.V0 = 0x00000002u;
        if (c.S2 == 0u) {
            c.V0 = c.S1 + 0u;
            goto L800149C8;
        }
        c.V0 = c.S1 + 0u;
        goto L80014AE8;
        L800149B8: ;
        if (c.S2 == c.V0) {
            c.V0 = c.S1 + 0u;
            goto L80014ADC;
        }
        c.V0 = c.S1 + 0u;
        goto L80014AE8;
        L800149C8: ;
        c.A0 = c.S0 + 0x48Cu;
        c.A1 = 0x00000001u;
        c.RA = 0x800149D4u;
        GranTurismo2ArcadePC.func_800696BC(c, m);
        if (c.V0 == 0u) {
            c.V0 = 0x801C0000u;
            goto L800149E4;
        }
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6B5Fu;
        goto L80014ACC;
        L800149E4: ;
        c.V1 = 0x00900000u;
        c.V1 = c.V1 | 0x500Cu;
        c.A0 = c.S0 + 0x35Cu;
        c.V0 = 0x0000150Cu;
        MemoryAccess.WriteU16(m, (c.S0 + 0x48u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.S0 + 0x5Eu), (ushort)0u);
        MemoryAccess.WriteU32(m, (c.A0 + 0x10u), c.V0);
        MemoryAccess.WriteU32(m, (c.A0 + 0xCu), c.V1);
        c.RA = 0x80014A08u;
        GranTurismo2ArcadePC.func_8006BF5C(c, m);
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6BAEu;
        MemoryAccess.WriteU32(m, (c.S0 + 0x24u), c.V0);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0xD68u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x28u), c.V0);
        c.RA = 0x80014A24u;
        GranTurismo2ArcadePC.func_8001339C(c, m);
        c.A0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x394u), c.S3);
        c.RA = 0x80014A30u;
        GranTurismo2ArcadePC.func_80013D80(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 == 0u) {
            c.V0 = c.S1 + 0u;
            goto L80014AE8;
        }
        c.V0 = c.S1 + 0u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x394u), 0u);
        goto L80014AE8;
        L80014A44: ;
        c.A0 = 0x00000001u;
        c.RA = 0x80014A4Cu;
        GranTurismo2ArcadePC.func_80013D80(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 == 0u) {
            goto L80014A5C;
        }
        MemoryAccess.WriteU32(m, (c.S0 + 0x394u), 0u);
        L80014A5C: ;
        c.RA = 0x80014A64u;
        GranTurismo2ArcadePC.func_8007D6DC(c, m);
        if (c.V0 == 0u) {
            goto L80014A8C;
        }
        if (c.V0 != c.S2) {
            c.V0 = 0x801C0000u;
            goto L80014AC4;
        }
        c.V0 = 0x801C0000u;
        c.V0 = 0x801F0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0xC8u));
        c.A0 = c.S0 + 0x35Cu;
        c.RA = 0x80014A84u;
        GranTurismo2ArcadePC.func_8006BF84(c, m);
        c.V0 = c.S1 + 0u;
        goto L80014AE8;
        L80014A8C: ;
        MemoryAccess.WriteU16(m, (c.S0 + 0x390u), (ushort)c.S1);
        c.A0 = c.S0 + 0x48Cu;
        c.RA = 0x80014A98u;
        GranTurismo2ArcadePC.func_800690EC(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 == 0u) {
            c.V0 = 0x801C0000u;
            goto L80014AAC;
        }
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6B09u;
        goto L80014ACC;
        L80014AAC: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x394u));
        if (c.V0 != 0u) {
            c.S1 = 0x0000000Au;
            goto L80014AE4;
        }
        c.S1 = 0x0000000Au;
        c.S1 = 0x00000002u;
        goto L80014AE4;
        L80014AC4: ;
        c.V0 = c.V0 - 0x6B5Fu;
        MemoryAccess.WriteU16(m, (c.S0 + 0x390u), (ushort)c.S1);
        L80014ACC: ;
        MemoryAccess.WriteU32(m, (c.S0 + 0x20u), c.V0);
        c.RA = 0x80014AD4u;
        GranTurismo2ArcadePC.func_800133EC(c, m);
        c.S1 = 0x00000001u;
        goto L80014AE4;
        L80014ADC: ;
        c.A0 = c.S0 + 0x35Cu;
        c.RA = 0x80014AE4u;
        GranTurismo2ArcadePC.func_8006C084(c, m);
        L80014AE4: ;
        c.V0 = c.S1 + 0u;
        L80014AE8: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80014B04(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = 0xFFFFFFFFu;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        c.V0 = 0x00000001u;
        if (c.A0 == c.V0) {
            MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
            goto L80014BD4;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L80014B4C;
        }
        c.V0 = 0x00000002u;
        if (c.A0 == 0u) {
            c.V0 = c.S1 + 0u;
            goto L80014B5C;
        }
        c.V0 = c.S1 + 0u;
        goto L80014CB4;
        L80014B4C: ;
        if (c.A0 == c.V0) {
            c.V0 = c.S1 + 0u;
            goto L80014C98;
        }
        c.V0 = c.S1 + 0u;
        goto L80014CB4;
        L80014B5C: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x394u));
        if (c.V0 == 0u) {
            c.A0 = c.S0 + 0x1A1Cu;
            goto L80014C74;
        }
        c.A0 = c.S0 + 0x1A1Cu;
        c.A1 = 0u + 0u;
        c.RA = 0x80014B74u;
        GranTurismo2ArcadePC.func_800696BC(c, m);
        if (c.V0 != 0u) {
            c.V0 = 0x801C0000u;
            goto L80014C84;
        }
        c.V0 = 0x801C0000u;
        c.RA = 0x80014B84u;
        Dispatcher.Call(c, m, 0x80013658u);
        c.V1 = 0x00900000u;
        c.V1 = c.V1 | 0x500Cu;
        c.A0 = c.S0 + 0x35Cu;
        c.V0 = 0x0000150Cu;
        MemoryAccess.WriteU32(m, (c.A0 + 0x10u), c.V0);
        MemoryAccess.WriteU32(m, (c.A0 + 0xCu), c.V1);
        c.RA = 0x80014BA0u;
        GranTurismo2ArcadePC.func_8006BF5C(c, m);
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6BD2u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x24u), c.V0);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0xD68u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x28u), c.V0);
        c.RA = 0x80014BBCu;
        GranTurismo2ArcadePC.func_8001339C(c, m);
        c.A0 = 0x00000001u;
        c.RA = 0x80014BC4u;
        GranTurismo2ArcadePC.func_80013D80(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 == 0u) {
            goto L80014BD4;
        }
        MemoryAccess.WriteU32(m, (c.S0 + 0x394u), 0u);
        L80014BD4: ;
        c.A0 = 0x00000001u;
        c.RA = 0x80014BDCu;
        GranTurismo2ArcadePC.func_80013D80(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 == 0u) {
            goto L80014BEC;
        }
        MemoryAccess.WriteU32(m, (c.S0 + 0x394u), 0u);
        L80014BEC: ;
        c.RA = 0x80014BF4u;
        GranTurismo2ArcadePC.func_8007D6DC(c, m);
        c.V1 = c.V0 + 0u;
        if (c.V1 == 0u) {
            c.V0 = 0x00000001u;
            goto L80014C20;
        }
        c.V0 = 0x00000001u;
        if (c.V1 != c.V0) {
            c.V0 = 0xFFFFFFFFu;
            goto L80014C7C;
        }
        c.V0 = 0xFFFFFFFFu;
        c.V0 = 0x801F0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0xC8u));
        c.A0 = c.S0 + 0x35Cu;
        c.RA = 0x80014C18u;
        GranTurismo2ArcadePC.func_8006BF84(c, m);
        c.V0 = c.S1 + 0u;
        goto L80014CB4;
        L80014C20: ;
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU16(m, (c.S0 + 0x390u), (ushort)c.V0);
        c.S1 = c.S0 + 0x1A1Cu;
        c.A0 = c.S1 + 0u;
        c.RA = 0x80014C34u;
        GranTurismo2ArcadePC.func_800690EC(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 == 0u) {
            c.V0 = 0x801C0000u;
            goto L80014C48;
        }
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6B34u;
        goto L80014C88;
        L80014C48: ;
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x10u), (ushort)c.V0);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0x200u));
        if (c.V0 != 0u) {
            c.S1 = 0x0000000Bu;
            goto L80014C64;
        }
        c.S1 = 0x0000000Bu;
        c.S1 = 0x00000014u;
        L80014C64: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x394u));
        if (c.V0 != 0u) {
            MemoryAccess.WriteU16(m, (c.S0 + 0x48Au), (ushort)0u);
            goto L80014CB0;
        }
        MemoryAccess.WriteU16(m, (c.S0 + 0x48Au), (ushort)0u);
        L80014C74: ;
        c.S1 = 0x00000002u;
        goto L80014CB0;
        L80014C7C: ;
        MemoryAccess.WriteU16(m, (c.S0 + 0x390u), (ushort)c.V0);
        c.V0 = 0x801C0000u;
        L80014C84: ;
        c.V0 = c.V0 - 0x6B8Au;
        L80014C88: ;
        MemoryAccess.WriteU32(m, (c.S0 + 0x20u), c.V0);
        c.RA = 0x80014C90u;
        GranTurismo2ArcadePC.func_800133EC(c, m);
        c.S1 = 0x00000001u;
        goto L80014CB0;
        L80014C98: ;
        c.A0 = c.S2 + 0u;
        c.A1 = 0u + 0u;
        c.RA = 0x80014CA4u;
        GranTurismo2ArcadePC.func_80013694(c, m);
        c.A0 = c.S0 + 0x35Cu;
        c.A1 = c.S2 + 0u;
        c.RA = 0x80014CB0u;
        GranTurismo2ArcadePC.func_8006C084(c, m);
        L80014CB0: ;
        c.V0 = c.S1 + 0u;
        L80014CB4: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80014CCC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x68u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x48u), c.S2);
        c.S2 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        MemoryAccess.WriteU32(m, (c.SP + 0x50u), c.S4);
        c.S4 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x58u), c.S6);
        c.S6 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x54u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x5Cu), c.S7);
        c.S7 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x60u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x4Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.S0);
        c.S0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x358u));
        c.S3 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S2 + 0x1C1Cu));
        if (c.S4 == c.S7) {
            c.S5 = 0xFFFFFFFFu;
            goto L80014E04;
        }
        c.S5 = 0xFFFFFFFFu;
        c.V0 = (int)c.S4 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L80014D34;
        }
        c.V0 = 0x00000002u;
        if (c.S4 == 0u) {
            c.V0 = c.S5 + 0u;
            goto L80014D44;
        }
        c.V0 = c.S5 + 0u;
        goto L8001508C;
        L80014D34: ;
        if (c.S4 == c.V0) {
            c.V0 = c.S5 + 0u;
            goto L80014F50;
        }
        c.V0 = c.S5 + 0u;
        goto L8001508C;
        L80014D44: ;
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0xD68u;
        MemoryAccess.WriteU32(m, (c.S2 + 0x24u), c.V0);
        MemoryAccess.WriteU32(m, (c.S2 + 0x28u), c.V0);
        c.RA = 0x80014D58u;
        GranTurismo2ArcadePC.func_8001339C(c, m);
        c.S1 = 0u + 0u;
        c.S0 = c.S1 + 0u;
        c.V0 = 0x80050000u;
        c.V1 = c.S3 + 0x1u;
        MemoryAccess.WriteU16(m, (c.V0 - 0x5368u), (ushort)c.V1);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x48Au));
        c.V0 = c.V0 - 0x5368u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x6u), (ushort)c.V1);
        L80014D78: ;
        c.V0 = (int)c.S1 < (int)c.S3 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S0 + c.S2;
            goto L80014DB4;
        }
        c.V0 = c.S0 + c.S2;
        c.A0 = c.S1 + 0u;
        c.A1 = c.SP + 0x38u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x398u), (ushort)0u);
        c.RA = 0x80014D94u;
        GranTurismo2ArcadePC.func_8001354C(c, m);
        c.V1 = c.S0 + c.S2;
        c.A0 = c.V1 + 0u;
        c.S0 = c.S0 + 0x6u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0x38u));
        c.S1 = c.S1 + 0x1u;
        MemoryAccess.WriteU16(m, (c.V1 + 0x39Au), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.A0 + 0x39Cu), (ushort)c.A1);
        goto L80014D78;
        L80014DB4: ;
        c.S0 = c.S1 << 1;
        c.S0 = c.S0 + c.S1;
        c.S0 = c.S0 << 1;
        c.V0 = c.S0 + c.S2;
        c.V1 = 0x00000002u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x398u), (ushort)c.V1);
        c.RA = 0x80014DD0u;
        GranTurismo2ArcadePC.func_80013484(c, m);
        c.V1 = c.S0 + c.S2;
        c.S0 = c.V1 + 0u;
        c.A0 = 0x00000001u;
        c.V0 = (int)0u < (int)c.V0 ? 1u : 0u;
        MemoryAccess.WriteU16(m, (c.V1 + 0x39Au), (ushort)c.V0);
        c.V0 = 0x00000003u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x39Cu), (ushort)c.V0);
        c.RA = 0x80014DF0u;
        GranTurismo2ArcadePC.func_80013D80(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 == 0u) {
            c.V0 = c.S5 + 0u;
            goto L8001508C;
        }
        c.V0 = c.S5 + 0u;
        MemoryAccess.WriteU32(m, (c.S2 + 0x394u), 0u);
        goto L8001508C;
        L80014E04: ;
        c.A0 = 0x00000001u;
        c.RA = 0x80014E0Cu;
        GranTurismo2ArcadePC.func_80013D80(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 == 0u) {
            goto L80014E1C;
        }
        MemoryAccess.WriteU32(m, (c.S2 + 0x394u), 0u);
        L80014E1C: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S2 + 0x394u));
        if (c.V0 != 0u) {
            c.A0 = 0x80050000u;
            goto L80014E3C;
        }
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x5368u;
        c.RA = 0x80014E34u;
        GranTurismo2ArcadePC.func_8006CDE8(c, m);
        c.S5 = 0x00000002u;
        goto L80015088;
        L80014E3C: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x10u));
        c.V1 = MemoryAccess.ReadU16(m, (c.S2 + 0x10u));
        if ((int)c.V0 <= 0) {
            c.V0 = c.V1 - 0x1u;
            goto L80014E70;
        }
        c.V0 = c.V1 - 0x1u;
        MemoryAccess.WriteU16(m, (c.S2 + 0x10u), (ushort)c.V0);
        c.V0 = c.V0 << 16;
        if (c.V0 != 0u) {
            c.V0 = 0xFFFFFFFDu;
            goto L80014E74;
        }
        c.V0 = 0xFFFFFFFDu;
        c.A0 = 0x00000007u;
        c.RA = 0x80014E64u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x5368u;
        c.RA = 0x80014E70u;
        GranTurismo2ArcadePC.func_8006CD80(c, m);
        L80014E70: ;
        c.V0 = 0xFFFFFFFDu;
        L80014E74: ;
        if (c.S0 == c.V0) {
            c.V0 = (int)c.S0 < -2 ? 1u : 0u;
            goto L80014EC0;
        }
        c.V0 = (int)c.S0 < -2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0xFFFFFFFCu;
            goto L80014E94;
        }
        c.V0 = 0xFFFFFFFCu;
        if (c.S0 == c.V0) {
            goto L80014ED0;
        }
        goto L80014EE0;
        L80014E94: ;
        c.V0 = 0xFFFFFFFEu;
        if (c.S0 == c.V0) {
            c.V0 = 0xFFFFFFFFu;
            goto L80015088;
        }
        c.V0 = 0xFFFFFFFFu;
        if (c.S0 != c.V0) {
            c.A0 = 0x80050000u;
            goto L80014EE0;
        }
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x5368u;
        c.RA = 0x80014EB0u;
        GranTurismo2ArcadePC.func_8006CDE8(c, m);
        c.A0 = 0x00000002u;
        c.RA = 0x80014EB8u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.S5 = 0x00000002u;
        goto L80015088;
        L80014EC0: ;
        c.A0 = 0x00000005u;
        c.RA = 0x80014EC8u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.V0 = c.S5 + 0u;
        goto L8001508C;
        L80014ED0: ;
        c.A0 = 0u + 0u;
        c.RA = 0x80014ED8u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.V0 = c.S5 + 0u;
        goto L8001508C;
        L80014EE0: ;
        c.A0 = 0x00000001u;
        c.RA = 0x80014EE8u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.V0 = c.S0 << 1;
        c.V0 = c.V0 + c.S0;
        c.V0 = c.V0 << 1;
        c.V0 = c.V0 + c.S2;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x398u));
        if (c.V0 != 0u) {
            c.V1 = 0xFFFFFFFFu;
            goto L80014F0C;
        }
        c.V1 = 0xFFFFFFFFu;
        c.V1 = c.S0 + 0u;
        L80014F0C: ;
        if ((int)c.V1 < 0) {
            goto L80014F34;
        }
        c.A0 = 0x00000001u;
        c.RA = 0x80014F1Cu;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.A0 = c.S0 + 0u;
        c.RA = 0x80014F24u;
        GranTurismo2ArcadePC.func_80013600(c, m);
        c.S5 = 0x0000000Bu;
        MemoryAccess.WriteU16(m, (c.S2 + 0x10u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.S2 + 0x48Au), (ushort)c.S0);
        goto L80015088;
        L80014F34: ;
        c.A0 = 0x00000001u;
        c.RA = 0x80014F3Cu;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x5368u;
        c.RA = 0x80014F48u;
        GranTurismo2ArcadePC.func_8006CDE8(c, m);
        c.S5 = 0x0000000Cu;
        goto L80015088;
        L80014F50: ;
        c.A0 = c.S6 + 0u;
        c.A1 = 0u + 0u;
        c.RA = 0x80014F5Cu;
        GranTurismo2ArcadePC.func_80013694(c, m);
        c.A0 = c.S6 + 0u;
        c.RA = 0x80014F64u;
        GranTurismo2ArcadePC.func_8001392C(c, m);
        c.V0 = 0x80020000u;
        c.S1 = c.V0 + 0xD68u;
        c.V0 = 0x80050000u;
        c.S0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 - 0x5362u));
        c.S3 = 0x00640000u;
        c.V0 = c.S0 << 1;
        c.V0 = c.V0 + c.S0;
        c.V0 = c.V0 << 1;
        c.V0 = c.V0 + c.S2;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x39Cu));
        if (c.V1 == c.S7) {
            c.S3 = c.S3 | 0x6464u;
            goto L80014FF8;
        }
        c.S3 = c.S3 | 0x6464u;
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L80014FB4;
        }
        if (c.V1 == 0u) {
            c.V0 = 0x801D0000u;
            goto L80014FE0;
        }
        c.V0 = 0x801D0000u;
        goto L80015004;
        L80014FB4: ;
        if (c.V1 == c.S4) {
            c.V0 = 0x00000003u;
            goto L80014FEC;
        }
        c.V0 = 0x00000003u;
        if (c.V1 != c.V0) {
            c.V0 = 0x801D0000u;
            goto L80015004;
        }
        c.V0 = 0x801D0000u;
        c.V0 = 0x801C0000u;
        c.S1 = c.V0 - 0x5F65u;
        c.RA = 0x80014FD0u;
        GranTurismo2ArcadePC.func_80013484(c, m);
        if ((int)c.V0 <= 0) {
            c.V0 = 0x801D0000u;
            goto L80015004;
        }
        c.V0 = 0x801D0000u;
        c.S1 = c.S1 - 0x15u;
        goto L80015004;
        L80014FE0: ;
        c.V0 = 0x801C0000u;
        c.S1 = c.V0 - 0x5F89u;
        goto L80015000;
        L80014FEC: ;
        c.V0 = 0x801F0000u;
        c.S1 = c.V0 - 0x88Bu;
        goto L80015000;
        L80014FF8: ;
        c.V0 = 0x801F0000u;
        c.S1 = c.V0 - 0x87Au;
        L80015000: ;
        c.V0 = 0x801D0000u;
        L80015004: ;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6CC0u));
        if (c.V0 != 0u) {
            c.A0 = c.SP + 0x18u;
            goto L80015034;
        }
        c.A0 = c.SP + 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S3);
        c.A0 = c.S6 + 0u;
        c.A1 = c.S1 + 0u;
        c.A2 = 0x000000B0u;
        c.A3 = 0x000001A8u;
        c.RA = 0x8001502Cu;
        GranTurismo2ArcadePC.func_80013278(c, m);
        c.V0 = c.S5 + 0u;
        goto L8001508C;
        L80015034: ;
        c.A1 = 0x800B0000u;
        c.A1 = c.A1 - 0x7518u;
        c.RA = 0x80015040u;
        GranTurismo2ArcadePC.func_8007D990(c, m);
        c.T0 = 0xFF9F0000u;
        c.T0 = c.T0 | 0xFFFFu;
        c.A0 = c.SP + 0x18u;
        c.A1 = c.S1 + 0u;
        c.A2 = 0x000000B0u;
        c.A3 = 0x000001B8u;
        c.V1 = MemoryAccess.ReadU32(m, (c.A0 + 0xCu));
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.A0 + 0x1Cu), (byte)c.V0);
        c.V0 = 0x00200000u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x10u), c.S6);
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.S3);
        c.V1 = c.V1 & c.T0;
        c.V1 = c.V1 | c.V0;
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.A0 + 0xCu), c.V1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.RA = 0x80015088u;
        GranTurismo2ArcadePC.func_8006ACC4(c, m);
        L80015088: ;
        c.V0 = c.S5 + 0u;
        L8001508C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x60u));
        c.S7 = MemoryAccess.ReadU32(m, (c.SP + 0x5Cu));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x58u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x54u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x50u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x4Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x48u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x44u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x40u));
        c.SP = c.SP + 0x68u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800150B8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        c.A3 = 0u + 0u;
        c.T5 = c.A3 + 0u;
        c.T0 = c.A3 + 0u;
        c.A1 = c.A3 + 0u;
        c.T6 = 0x00020000u;
        c.T7 = 0xFFFFFF80u;
        c.V0 = 0x800B0000u;
        c.A2 = 0x00000988u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        c.V0 = c.T6 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.T4 = c.S0 + 0x1A1Cu;
        c.T3 = c.S0 + 0u;
        c.T2 = c.S0 + 0u;
        c.T1 = c.S0 + 0u;
        c.V0 = c.S0 + c.V0;
        MemoryAccess.WriteU16(m, (c.V0 + 0x2FD0u), (ushort)0u);
        L8001510C: ;
        c.V0 = (int)c.A1 < 32 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S0 + c.A1;
            goto L800151C4;
        }
        c.V0 = c.S0 + c.A1;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V0 + 0x2FA8u));
        if (c.V0 == 0u) {
            c.V0 = c.T1 + c.T6;
            goto L800151B8;
        }
        c.V0 = c.T1 + c.T6;
        MemoryAccess.WriteU16(m, (c.V0 + 0x2FD8u), (ushort)c.A1);
        c.V0 = c.T2 + c.T6;
        c.V0 = c.V0 + 0x3260u;
        c.V1 = c.T4 + c.A2;
        c.A0 = c.V1 + 0x50u;
        L8001513C: ;
        c.S1 = MemoryAccess.ReadU32(m, c.V1);
        c.S2 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T8 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T9 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, c.V0, c.S1);
        MemoryAccess.WriteU32(m, (c.V0 + 0x4u), c.S2);
        MemoryAccess.WriteU32(m, (c.V0 + 0x8u), c.T8);
        MemoryAccess.WriteU32(m, (c.V0 + 0xCu), c.T9);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.A0) {
            c.V0 = c.V0 + 0x10u;
            goto L8001513C;
        }
        c.V0 = c.V0 + 0x10u;
        c.A0 = c.T3 + c.T6;
        c.T3 = c.T3 + 0x4u;
        c.T2 = c.T2 + 0x5Cu;
        c.S1 = MemoryAccess.ReadU32(m, c.V1);
        c.S2 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T8 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        MemoryAccess.WriteU32(m, c.V0, c.S1);
        MemoryAccess.WriteU32(m, (c.V0 + 0x4u), c.S2);
        MemoryAccess.WriteU32(m, (c.V0 + 0x8u), c.T8);
        c.V1 = c.T4 + c.A2;
        MemoryAccess.WriteU32(m, (c.A0 + 0x3020u), c.A3);
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x54u));
        c.T1 = c.T1 + 0x2u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x3140u), c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x54u));
        c.T0 = c.T0 + 0x1u;
        c.V0 = c.V0 + 0x7Fu;
        c.V0 = c.V0 & c.T7;
        c.A3 = c.A3 + c.V0;
        MemoryAccess.WriteU32(m, (c.A0 + 0x30B0u), c.V0);
        L800151B8: ;
        c.A2 = c.A2 + 0x5Cu;
        c.A1 = c.A1 + 0x1u;
        goto L8001510C;
        L800151C4: ;
        c.V0 = 0x00020000u;
        c.V0 = c.S0 + c.V0;
        MemoryAccess.WriteU16(m, (c.V0 + 0x2FD2u), (ushort)c.T0);
        if (c.T0 != 0u) {
            MemoryAccess.WriteU32(m, (c.V0 + 0x2FD4u), c.A3);
            goto L800151E0;
        }
        MemoryAccess.WriteU32(m, (c.V0 + 0x2FD4u), c.A3);
        c.V0 = 0x00000002u;
        goto L8001524C;
        L800151E0: ;
        if ((int)c.T0 <= 0) {
            c.A1 = 0u + 0u;
            goto L80015214;
        }
        c.A1 = 0u + 0u;
        c.A2 = 0x00020000u;
        c.A0 = c.S0 + 0u;
        L800151F0: ;
        c.V1 = c.A0 + c.A2;
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x30B0u));
        c.A1 = c.A1 + 0x1u;
        c.T5 = c.T5 + c.V0;
        c.V0 = c.A3 - c.T5;
        MemoryAccess.WriteU32(m, (c.V1 + 0x31D0u), c.V0);
        c.V0 = (int)c.A1 < (int)c.T0 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.A0 = c.A0 + 0x4u;
            goto L800151F0;
        }
        c.A0 = c.A0 + 0x4u;
        L80015214: ;
        c.V0 = 0x00500000u;
        c.V0 = c.V0 | 0x782Cu;
        c.A0 = c.S0 + 0x35Cu;
        MemoryAccess.WriteU32(m, (c.A0 + 0x10u), c.A3);
        MemoryAccess.WriteU32(m, (c.A0 + 0xCu), c.V0);
        c.RA = 0x8001522Cu;
        GranTurismo2ArcadePC.func_8006BF5C(c, m);
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x634Fu;
        MemoryAccess.WriteU32(m, (c.S0 + 0x24u), c.V0);
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0xE85u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x28u), c.V0);
        c.RA = 0x80015248u;
        GranTurismo2ArcadePC.func_8001339C(c, m);
        c.V0 = 0x0000000Du;
        L8001524C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80015264(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x38u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S2);
        c.S2 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.S6);
        c.S6 = c.A1 + 0u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        c.A0 = 0x00020000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x34u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S1);
        c.V0 = c.S0 + c.A0;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x2FD0u));
        c.V1 = c.V0 << 1;
        c.V1 = c.S0 + c.V1;
        c.V0 = c.V0 << 2;
        c.V0 = c.S0 + c.V0;
        c.S3 = c.V0 + c.A0;
        c.V1 = c.V1 + c.A0;
        c.V0 = MemoryAccess.ReadU32(m, (c.S3 + 0x3020u));
        c.S4 = (uint)(short)MemoryAccess.ReadU16(m, (c.V1 + 0x2FD8u));
        c.V0 = c.V0 + 0x2FD0u;
        c.S5 = c.S0 + c.V0;
        c.V0 = 0x00000001u;
        if (c.S2 == c.V0) {
            c.S1 = 0xFFFFFFFFu;
            goto L80015368;
        }
        c.S1 = 0xFFFFFFFFu;
        c.V0 = (int)c.S2 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L800152F8;
        }
        c.V0 = 0x00000002u;
        if (c.S2 == 0u) {
            c.V0 = c.S1 + 0u;
            goto L80015308;
        }
        c.V0 = c.S1 + 0u;
        goto L8001540C;
        L800152F8: ;
        if (c.S2 == c.V0) {
            c.V0 = c.S1 + 0u;
            goto L800153F0;
        }
        c.V0 = c.S1 + 0u;
        goto L8001540C;
        L80015308: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x394u));
        if (c.V0 != 0u) {
            c.V0 = 0x00020000u;
            goto L80015324;
        }
        c.V0 = 0x00020000u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x390u), (ushort)c.S1);
        c.S1 = 0x00000002u;
        goto L80015408;
        L80015324: ;
        c.V0 = c.V0 | 0x3F50u;
        c.V0 = c.S0 + c.V0;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.A0 = c.S0 + 0x1A1Cu;
        c.A1 = 0u + 0u;
        c.A2 = c.S4 + 0u;
        c.A3 = c.S5 + 0u;
        c.RA = 0x80015344u;
        GranTurismo2ArcadePC.func_800696F8(c, m);
        if (c.V0 != 0u) {
            c.V0 = 0x801C0000u;
            goto L800153D8;
        }
        c.V0 = 0x801C0000u;
        c.A0 = 0x00000001u;
        c.RA = 0x80015354u;
        GranTurismo2ArcadePC.func_80013D80(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 == 0u) {
            c.V0 = c.S1 + 0u;
            goto L8001540C;
        }
        c.V0 = c.S1 + 0u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x394u), 0u);
        goto L8001540C;
        L80015368: ;
        c.A0 = 0x00000001u;
        c.RA = 0x80015370u;
        GranTurismo2ArcadePC.func_80013D80(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 == 0u) {
            goto L80015380;
        }
        MemoryAccess.WriteU32(m, (c.S0 + 0x394u), 0u);
        L80015380: ;
        c.RA = 0x80015388u;
        GranTurismo2ArcadePC.func_8007D6DC(c, m);
        if (c.V0 == 0u) {
            c.A0 = c.S0 + 0x1A1Cu;
            goto L800153B8;
        }
        c.A0 = c.S0 + 0x1A1Cu;
        if (c.V0 != c.S2) {
            c.V0 = 0x801C0000u;
            goto L800153D8;
        }
        c.V0 = 0x801C0000u;
        c.V0 = 0x801F0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0xC8u));
        c.A1 = MemoryAccess.ReadU32(m, (c.S3 + 0x31D0u));
        c.A0 = c.S0 + 0x35Cu;
        c.A1 = c.V0 + c.A1;
        c.RA = 0x800153B0u;
        GranTurismo2ArcadePC.func_8006BF84(c, m);
        c.V0 = c.S1 + 0u;
        goto L8001540C;
        L800153B8: ;
        c.A1 = c.S4 + 0u;
        c.A2 = c.S5 + 0u;
        c.RA = 0x800153C4u;
        GranTurismo2ArcadePC.func_800691EC(c, m);
        if (c.V0 != 0u) {
            c.S1 = 0x0000000Eu;
            goto L80015408;
        }
        c.S1 = 0x0000000Eu;
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6B34u;
        goto L800153E0;
        L800153D8: ;
        c.V0 = c.V0 - 0x6B8Au;
        MemoryAccess.WriteU16(m, (c.S0 + 0x390u), (ushort)c.S1);
        L800153E0: ;
        MemoryAccess.WriteU32(m, (c.S0 + 0x20u), c.V0);
        c.RA = 0x800153E8u;
        GranTurismo2ArcadePC.func_800133EC(c, m);
        c.S1 = 0x00000001u;
        goto L80015408;
        L800153F0: ;
        c.A0 = c.S6 + 0u;
        c.A1 = 0u + 0u;
        c.RA = 0x800153FCu;
        GranTurismo2ArcadePC.func_80013694(c, m);
        c.A0 = c.S0 + 0x35Cu;
        c.A1 = c.S6 + 0u;
        c.RA = 0x80015408u;
        GranTurismo2ArcadePC.func_8006C084(c, m);
        L80015408: ;
        c.V0 = c.S1 + 0u;
        L8001540C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x34u));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x30u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x2Cu));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.SP = c.SP + 0x38u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80015434(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x394u));
        if (c.V0 != 0u) {
            c.A0 = 0x0000000Fu;
            goto L80015460;
        }
        c.A0 = 0x0000000Fu;
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU16(m, (c.V1 + 0x390u), (ushort)c.V0);
        c.A0 = 0x00000002u;
        goto L80015494;
        L80015460: ;
        c.V0 = 0x00020000u;
        c.V0 = c.V1 + c.V0;
        c.V1 = MemoryAccess.ReadU16(m, (c.V0 + 0x2FD0u));
        c.V1 = c.V1 + 0x1u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x2FD0u), (ushort)c.V1);
        c.V1 = c.V1 << 16;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x2FD2u));
        c.V1 = (uint)((int)c.V1 >> 16);
        c.V1 = (int)c.V1 < (int)c.V0 ? 1u : 0u;
        if (c.V1 == 0u) {
            goto L80015494;
        }
        c.A0 = 0x0000000Du;
        L80015494: ;
        c.V0 = c.A0 + 0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001549C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V1 = 0x000C0000u;
        c.V1 = c.V1 | 0x5090u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        c.V0 = 0x00020000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.V0 = c.S0 + c.V0;
        MemoryAccess.WriteU16(m, (c.V0 + 0x46D2u), (ushort)0u);
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x2FD4u));
        c.A0 = c.S0 + 0x35Cu;
        MemoryAccess.WriteU32(m, (c.A0 + 0xCu), c.V1);
        c.V0 = c.V0 + 0x150Cu;
        MemoryAccess.WriteU32(m, (c.A0 + 0x10u), c.V0);
        c.RA = 0x800154DCu;
        GranTurismo2ArcadePC.func_8006BF5C(c, m);
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6328u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x24u), c.V0);
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0xE85u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x28u), c.V0);
        c.RA = 0x800154F8u;
        GranTurismo2ArcadePC.func_8001339C(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = 0x00000010u;
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001550C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x38u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S2);
        c.S2 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.S6);
        c.S6 = c.A1 + 0u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        c.V1 = 0x00020000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x34u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S1);
        c.V0 = c.S0 + c.V1;
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x46D2u));
        c.V0 = c.A3 << 2;
        c.V0 = c.S0 + c.V0;
        c.S1 = c.V0 + c.V1;
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x3020u));
        c.S4 = MemoryAccess.ReadU32(m, (c.S1 + 0x3140u));
        c.V0 = c.V0 + 0x2FD0u;
        c.S5 = c.S0 + c.V0;
        c.V0 = 0x00000001u;
        if (c.S2 == c.V0) {
            c.S3 = 0xFFFFFFFFu;
            goto L80015644;
        }
        c.S3 = 0xFFFFFFFFu;
        c.V0 = (int)c.S2 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L80015594;
        }
        c.V0 = 0x00000002u;
        if (c.S2 == 0u) {
            c.V0 = c.S3 + 0u;
            goto L800155A4;
        }
        c.V0 = c.S3 + 0u;
        goto L800156BC;
        L80015594: ;
        if (c.S2 == c.V0) {
            c.V0 = c.S3 + 0u;
            goto L800156A0;
        }
        c.V0 = c.S3 + 0u;
        goto L800156BC;
        L800155A4: ;
        c.V0 = 0x00020000u;
        c.V0 = c.V0 | 0x3260u;
        c.V1 = 0x00020000u;
        c.V1 = c.V1 | 0x3F50u;
        c.S1 = c.S0 + 0x48Cu;
        c.A0 = c.S1 + 0u;
        c.A1 = 0xFFFFFFFFu;
        c.A2 = c.A3 << 1;
        c.A2 = c.A2 + c.A3;
        c.A2 = c.A2 << 3;
        c.A2 = c.A2 - c.A3;
        c.A2 = c.A2 << 2;
        c.A2 = c.A2 + c.V0;
        c.A2 = c.S0 + c.A2;
        c.A3 = c.S5 + 0u;
        c.S2 = c.S0 + c.V1;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S2);
        c.RA = 0x800155F0u;
        GranTurismo2ArcadePC.func_80069328(c, m);
        if (c.V0 != 0u) {
            c.V0 = 0x801C0000u;
            goto L80015600;
        }
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6B09u;
        goto L80015690;
        L80015600: ;
        c.A0 = c.S1 + 0u;
        c.RA = 0x80015608u;
        GranTurismo2ArcadePC.func_800690EC(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 == 0u) {
            c.V0 = 0x801C0000u;
            goto L8001561C;
        }
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6B09u;
        goto L80015690;
        L8001561C: ;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S2);
        c.A0 = c.S1 + 0u;
        c.A1 = 0x00000001u;
        c.A2 = c.S5 + 0u;
        c.A3 = c.S4 + 0u;
        c.RA = 0x80015634u;
        GranTurismo2ArcadePC.func_80069668(c, m);
        if (c.V0 == 0u) {
            c.V0 = 0x801C0000u;
            goto L800156B8;
        }
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6A69u;
        goto L80015690;
        L80015644: ;
        c.RA = 0x8001564Cu;
        GranTurismo2ArcadePC.func_8007D6DC(c, m);
        if (c.V0 == 0u) {
            goto L80015680;
        }
        if (c.V0 != c.S2) {
            c.V0 = 0x801C0000u;
            goto L80015688;
        }
        c.V0 = 0x801C0000u;
        c.V0 = 0x801F0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0xC8u));
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x31D0u));
        c.A0 = c.S0 + 0x35Cu;
        c.A1 = c.A1 + c.V0;
        c.A1 = c.A1 + 0x150Cu;
        c.RA = 0x80015678u;
        GranTurismo2ArcadePC.func_8006BF84(c, m);
        c.V0 = c.S3 + 0u;
        goto L800156BC;
        L80015680: ;
        c.S3 = 0x00000011u;
        goto L800156B8;
        L80015688: ;
        c.V0 = c.V0 - 0x6A69u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x390u), (ushort)c.S3);
        L80015690: ;
        MemoryAccess.WriteU32(m, (c.S0 + 0x20u), c.V0);
        c.RA = 0x80015698u;
        GranTurismo2ArcadePC.func_800133EC(c, m);
        c.S3 = 0x00000001u;
        goto L800156B8;
        L800156A0: ;
        c.A0 = c.S6 + 0u;
        c.A1 = 0x00000001u;
        c.RA = 0x800156ACu;
        GranTurismo2ArcadePC.func_80013694(c, m);
        c.A0 = c.S0 + 0x35Cu;
        c.A1 = c.S6 + 0u;
        c.RA = 0x800156B8u;
        GranTurismo2ArcadePC.func_8006C084(c, m);
        L800156B8: ;
        c.V0 = c.S3 + 0u;
        L800156BC: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x34u));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x30u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x2Cu));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.SP = c.SP + 0x38u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800156E4(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x2u));
        if (c.V0 == 0u) {
            c.A0 = 0x00000012u;
            goto L80015724;
        }
        c.A0 = 0x00000012u;
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6A69u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x20u), c.V0);
        c.RA = 0x8001571Cu;
        GranTurismo2ArcadePC.func_800133EC(c, m);
        c.A0 = 0x00000001u;
        goto L80015758;
        L80015724: ;
        c.V0 = 0x00020000u;
        c.V0 = c.V1 + c.V0;
        c.V1 = MemoryAccess.ReadU16(m, (c.V0 + 0x46D2u));
        c.V1 = c.V1 + 0x1u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x46D2u), (ushort)c.V1);
        c.V1 = c.V1 << 16;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x2FD2u));
        c.V1 = (uint)((int)c.V1 >> 16);
        c.V1 = (int)c.V1 < (int)c.V0 ? 1u : 0u;
        if (c.V1 == 0u) {
            goto L80015758;
        }
        c.A0 = 0x00000010u;
        L80015758: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = c.A0 + 0u;
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80015768(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = 0xFFFFFFFFu;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        c.V0 = 0x00000001u;
        if (c.S1 == c.V0) {
            MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
            goto L80015810;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        c.V0 = (int)c.S1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L800157B8;
        }
        c.V0 = 0x00000002u;
        if (c.S1 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L800157C8;
        }
        c.V0 = c.S2 + 0u;
        goto L80015880;
        L800157B8: ;
        if (c.S1 == c.V0) {
            c.V0 = c.S2 + 0u;
            goto L80015864;
        }
        c.V0 = c.S2 + 0u;
        goto L80015880;
        L800157C8: ;
        c.A0 = c.S0 + 0x48Cu;
        c.A1 = 0x00000001u;
        c.RA = 0x800157D4u;
        GranTurismo2ArcadePC.func_8006962C(c, m);
        if (c.V0 == 0u) {
            c.V0 = 0x801C0000u;
            goto L800157EC;
        }
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6A69u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x20u), c.V0);
        c.RA = 0x800157E8u;
        GranTurismo2ArcadePC.func_800133EC(c, m);
        c.S2 = 0x00000001u;
        L800157EC: ;
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6328u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x24u), c.V0);
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0xE85u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x28u), c.V0);
        c.RA = 0x80015808u;
        GranTurismo2ArcadePC.func_8001339C(c, m);
        c.V0 = c.S2 + 0u;
        goto L80015880;
        L80015810: ;
        c.RA = 0x80015818u;
        GranTurismo2ArcadePC.func_8007D6DC(c, m);
        if (c.V0 == 0u) {
            goto L80015840;
        }
        if (c.V0 != c.S1) {
            c.V0 = 0x801C0000u;
            goto L8001584C;
        }
        c.V0 = 0x801C0000u;
        c.V0 = 0x801F0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0xC8u));
        c.A0 = c.S0 + 0x35Cu;
        c.RA = 0x80015838u;
        GranTurismo2ArcadePC.func_8006BF84(c, m);
        c.V0 = c.S2 + 0u;
        goto L80015880;
        L80015840: ;
        MemoryAccess.WriteU16(m, (c.S0 + 0x390u), (ushort)c.S2);
        c.S2 = 0x00000013u;
        goto L8001587C;
        L8001584C: ;
        c.V0 = c.V0 - 0x6A69u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x390u), (ushort)c.S2);
        MemoryAccess.WriteU32(m, (c.S0 + 0x20u), c.V0);
        c.RA = 0x8001585Cu;
        GranTurismo2ArcadePC.func_800133EC(c, m);
        c.S2 = 0x00000001u;
        goto L8001587C;
        L80015864: ;
        c.A0 = c.S3 + 0u;
        c.A1 = 0x00000001u;
        c.RA = 0x80015870u;
        GranTurismo2ArcadePC.func_80013694(c, m);
        c.A0 = c.S0 + 0x35Cu;
        c.A1 = c.S3 + 0u;
        c.RA = 0x8001587Cu;
        GranTurismo2ArcadePC.func_8006C084(c, m);
        L8001587C: ;
        c.V0 = c.S2 + 0u;
        L80015880: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001589C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x354u));
        if (c.A0 == c.V0) {
            c.S2 = 0xFFFFFFFFu;
            goto L80015938;
        }
        c.S2 = 0xFFFFFFFFu;
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L80015978;
        }
        c.V0 = c.S2 + 0u;
        if (c.A0 != 0u) {
            c.V1 = c.S1 + 0x30u;
            goto L80015978;
        }
        c.V1 = c.S1 + 0x30u;
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 + 0x4u));
        c.V0 = ~(0u | c.V0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x18u), (ushort)c.V0);
        c.V1 = c.S1 + 0x4Cu;
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 + 0x10u));
        c.A0 = 0x00000007u;
        c.V0 = ~(0u | c.V0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x12u), (ushort)c.V0);
        c.RA = 0x80015904u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.S0 = c.S1 + 0x2C0u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x80015910u;
        GranTurismo2ArcadePC.func_8006E298(c, m);
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x69E2u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x7Du), (byte)0u);
        MemoryAccess.WriteU32(m, (c.S1 + 0x24u), c.V0);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0xD68u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        c.RA = 0x80015930u;
        GranTurismo2ArcadePC.func_8001339C(c, m);
        c.V0 = c.S2 + 0u;
        goto L80015978;
        L80015938: ;
        if (c.V1 == 0u) {
            goto L80015968;
        }
        if ((int)c.V1 > 0) {
            goto L80015958;
        }
        if (c.V1 == c.S2) {
            c.V0 = c.S2 + 0u;
            goto L80015960;
        }
        c.V0 = c.S2 + 0u;
        goto L80015978;
        L80015958: ;
        if (c.V1 != c.A0) {
            c.V0 = c.S2 + 0u;
            goto L80015978;
        }
        c.V0 = c.S2 + 0u;
        L80015960: ;
        c.S2 = 0x00000016u;
        goto L80015974;
        L80015968: ;
        c.A0 = 0x00000001u;
        c.RA = 0x80015970u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.S2 = 0x00000002u;
        L80015974: ;
        c.V0 = c.S2 + 0u;
        L80015978: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80015990(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x18Cu));
        if (c.A0 == c.V0) {
            c.S2 = 0xFFFFFFFFu;
            goto L80015A08;
        }
        c.S2 = 0xFFFFFFFFu;
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L80015A48;
        }
        c.V0 = c.S2 + 0u;
        if (c.A0 != 0u) {
            goto L80015A48;
        }
        c.A0 = 0u + 0u;
        c.RA = 0x800159D8u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.S0 = c.S1 + 0xF8u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x800159E4u;
        GranTurismo2ArcadePC.func_8006E298(c, m);
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0xAA0u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x7Du), (byte)0u);
        MemoryAccess.WriteU32(m, (c.S1 + 0x24u), c.V0);
        c.V0 = c.V0 - 0x110u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        c.RA = 0x80015A00u;
        GranTurismo2ArcadePC.func_8001339C(c, m);
        c.V0 = c.S2 + 0u;
        goto L80015A48;
        L80015A08: ;
        if (c.V1 == 0u) {
            goto L80015A38;
        }
        if ((int)c.V1 > 0) {
            goto L80015A28;
        }
        if (c.V1 == c.S2) {
            c.V0 = c.S2 + 0u;
            goto L80015A30;
        }
        c.V0 = c.S2 + 0u;
        goto L80015A48;
        L80015A28: ;
        if (c.V1 != c.A0) {
            c.V0 = c.S2 + 0u;
            goto L80015A48;
        }
        c.V0 = c.S2 + 0u;
        L80015A30: ;
        c.S2 = 0x00000016u;
        goto L80015A44;
        L80015A38: ;
        c.A0 = 0x00000001u;
        c.RA = 0x80015A40u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.S2 = 0x00000002u;
        L80015A44: ;
        c.V0 = c.S2 + 0u;
        L80015A48: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80015A60(CpuContext c, IMemory m)
    {
        c.V0 = 0xFFFFFFFFu;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80015A68(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        c.SP = c.SP - 0x18u;
        if ((int)c.A0 >= 0) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
            goto L80015A88;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V0 = 0x80010000u;
        c.V0 = c.V0 + 0x5A60u;
        goto L80015AA0;
        L80015A88: ;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 - 0x52B4u;
        c.V1 = c.A0 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        L80015AA0: ;
        MemoryAccess.WriteU32(m, (c.A1 + 0x4u), c.V0);
        MemoryAccess.WriteU16(m, c.A1, (ushort)c.A0);
        c.A0 = 0u + 0u;
        c.V0 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.A1 = c.A0 + 0u;
        c.RA = 0x80015ABCu;
        Dispatcher.Call(c, m, c.V0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80015ACC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x5448u;
        c.V0 = 0x800B0000u;
        c.A2 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        c.V0 = 0x80050000u;
        c.V1 = 0xFFFFFFFFu;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.A0 = c.S0 + 0x60u;
        c.T2 = c.V0 - 0x5478u;
        c.A3 = MemoryAccess.ReadU32(m, c.T2);
        c.T0 = MemoryAccess.ReadU32(m, (c.T2 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.T2 + 0x8u));
        MemoryAccess.WriteU32(m, (c.S0 + 0x30u), c.A3);
        MemoryAccess.WriteU32(m, (c.S0 + 0x34u), c.T0);
        MemoryAccess.WriteU32(m, (c.S0 + 0x38u), c.T1);
        c.A3 = MemoryAccess.ReadU32(m, (c.T2 + 0xCu));
        c.T0 = MemoryAccess.ReadU32(m, (c.T2 + 0x10u));
        c.T1 = MemoryAccess.ReadU32(m, (c.T2 + 0x14u));
        MemoryAccess.WriteU32(m, (c.S0 + 0x3Cu), c.A3);
        MemoryAccess.WriteU32(m, (c.S0 + 0x40u), c.T0);
        MemoryAccess.WriteU32(m, (c.S0 + 0x44u), c.T1);
        c.A3 = MemoryAccess.ReadU32(m, (c.T2 + 0x18u));
        MemoryAccess.WriteU32(m, (c.S0 + 0x48u), c.A3);
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x48u), (ushort)c.V1);
        c.T2 = c.V0 - 0x545Cu;
        c.A3 = MemoryAccess.ReadU32(m, c.T2);
        c.T0 = MemoryAccess.ReadU32(m, (c.T2 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.T2 + 0x8u));
        MemoryAccess.WriteU32(m, (c.S0 + 0x4Cu), c.A3);
        MemoryAccess.WriteU32(m, (c.S0 + 0x50u), c.T0);
        MemoryAccess.WriteU32(m, (c.S0 + 0x54u), c.T1);
        c.A3 = MemoryAccess.ReadU32(m, (c.T2 + 0xCu));
        c.T0 = MemoryAccess.ReadU32(m, (c.T2 + 0x10u));
        MemoryAccess.WriteU32(m, (c.S0 + 0x58u), c.A3);
        MemoryAccess.WriteU32(m, (c.S0 + 0x5Cu), c.T0);
        MemoryAccess.WriteU16(m, (c.S0 + 0x5Eu), (ushort)c.V1);
        c.RA = 0x80015B74u;
        GranTurismo2ArcadePC.func_8006E0DC(c, m);
        c.A0 = c.S0 + 0xF8u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x5418u;
        c.A2 = 0u + 0u;
        c.RA = 0x80015B88u;
        GranTurismo2ArcadePC.func_8006E0DC(c, m);
        c.A0 = c.S0 + 0x190u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x53E8u;
        c.A2 = 0u + 0u;
        c.RA = 0x80015B9Cu;
        GranTurismo2ArcadePC.func_8006E0DC(c, m);
        c.A0 = c.S0 + 0x228u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x53B8u;
        c.A2 = 0u + 0u;
        c.RA = 0x80015BB0u;
        GranTurismo2ArcadePC.func_8006E0DC(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x5388u;
        c.RA = 0x80015BBCu;
        GranTurismo2ArcadePC.func_8006D8D8(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x5368u;
        c.A1 = 0x80010000u;
        c.A1 = c.A1 + 0x3A10u;
        c.A2 = 0u + 0u;
        c.RA = 0x80015BD4u;
        GranTurismo2ArcadePC.func_8006CCDC(c, m);
        c.A0 = c.S0 + 0x2C0u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x5334u;
        c.A2 = 0u + 0u;
        c.RA = 0x80015BE8u;
        GranTurismo2ArcadePC.func_8006E0DC(c, m);
        c.V1 = c.S0 + 0x35Cu;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 - 0x5304u;
        c.A0 = c.V0 + 0x30u;
        L80015BF8: ;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V0 + 0xCu));
        MemoryAccess.WriteU32(m, c.V1, c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x4u), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x8u), c.T1);
        MemoryAccess.WriteU32(m, (c.V1 + 0xCu), c.T2);
        c.V0 = c.V0 + 0x10u;
        if (c.V0 != c.A0) {
            c.V1 = c.V1 + 0x10u;
            goto L80015BF8;
        }
        c.V1 = c.V1 + 0x10u;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        MemoryAccess.WriteU32(m, c.V1, c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x4u), c.T0);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0xD68u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x24u), c.V0);
        MemoryAccess.WriteU32(m, (c.S0 + 0x28u), c.V0);
        c.RA = 0x80015C48u;
        GranTurismo2ArcadePC.func_8001339C(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80015C58(CpuContext c, IMemory m)
    {
        c.V0 = MemoryAccess.ReadU32(m, c.A0);
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0xCu));
        c.V1 = 0x800B0000u;
        c.T1 = c.V1 - 0x7508u;
        c.A2 = MemoryAccess.ReadU32(m, c.A1);
        c.A3 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.T0 = MemoryAccess.ReadU32(m, (c.A1 + 0x8u));
        MemoryAccess.WriteU32(m, c.T1, c.A2);
        MemoryAccess.WriteU32(m, (c.T1 + 0x4u), c.A3);
        MemoryAccess.WriteU32(m, (c.T1 + 0x8u), c.T0);
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0x10u));
        c.V1 = 0x800B0000u;
        c.T1 = c.V1 - 0x7528u;
        c.A2 = MemoryAccess.ReadU32(m, c.A1);
        c.A3 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.T0 = MemoryAccess.ReadU32(m, (c.A1 + 0x8u));
        MemoryAccess.WriteU32(m, c.T1, c.A2);
        MemoryAccess.WriteU32(m, (c.T1 + 0x4u), c.A3);
        MemoryAccess.WriteU32(m, (c.T1 + 0x8u), c.T0);
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0x14u));
        c.V1 = 0x800B0000u;
        c.T1 = c.V1 - 0x7518u;
        c.A2 = MemoryAccess.ReadU32(m, c.A1);
        c.A3 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.T0 = MemoryAccess.ReadU32(m, (c.A1 + 0x8u));
        MemoryAccess.WriteU32(m, c.T1, c.A2);
        MemoryAccess.WriteU32(m, (c.T1 + 0x4u), c.A3);
        MemoryAccess.WriteU32(m, (c.T1 + 0x8u), c.T0);
        c.V1 = MemoryAccess.ReadU32(m, (c.A0 + 0x4u));
        c.A1 = 0x00020000u;
        MemoryAccess.WriteU32(m, (c.V0 + 0x8u), c.V1);
        c.V1 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.A1 = c.A1 | 0x46D4u;
        MemoryAccess.WriteU32(m, (c.V0 + 0xCu), c.V1);
        c.A0 = MemoryAccess.ReadU16(m, (c.A0 + 0x18u));
        c.V1 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V1 - 0x7530u), c.V0);
        MemoryAccess.WriteU16(m, (c.V0 + 0x2Cu), (ushort)c.A0);
        c.V0 = c.V0 + c.A1;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80015CF8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.RA = 0x80015D08u;
        GranTurismo2ArcadePC.func_80015ACC(c, m);
        c.A0 = 0u + 0u;
        c.RA = 0x80015D10u;
        GranTurismo2ArcadePC.func_80015A68(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80015D20(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.S0 + 0x1Cu), 0u);
        MemoryAccess.WriteU32(m, (c.S0 + 0x14u), 0u);
        if (c.S1 == 0u) {
            MemoryAccess.WriteU32(m, (c.S0 + 0x18u), 0u);
            goto L80015D78;
        }
        MemoryAccess.WriteU32(m, (c.S0 + 0x18u), 0u);
        c.V0 = MemoryAccess.ReadU32(m, c.S1);
        MemoryAccess.WriteU32(m, (c.S0 + 0x18u), c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x4u));
        MemoryAccess.WriteU32(m, (c.S0 + 0x14u), c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0xCu));
        MemoryAccess.WriteU32(m, (c.S0 + 0x1Cu), c.V0);
        L80015D78: ;
        c.A0 = c.S0 + 0x30u;
        c.RA = 0x80015D80u;
        GranTurismo2ArcadePC.func_8006BD74(c, m);
        c.A0 = c.S0 + 0x4Cu;
        c.RA = 0x80015D88u;
        GranTurismo2ArcadePC.func_8006BBC8(c, m);
        c.A0 = c.S0 + 0x60u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80015D94u;
        GranTurismo2ArcadePC.func_8006E34C(c, m);
        c.A0 = c.S0 + 0xF8u;
        c.A1 = c.S1 + 0u;
        MemoryAccess.WriteU16(m, (c.S0 + 0xF4u), (ushort)c.V0);
        c.RA = 0x80015DA4u;
        GranTurismo2ArcadePC.func_8006E34C(c, m);
        c.A0 = c.S0 + 0x190u;
        c.A1 = c.S1 + 0u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x18Cu), (ushort)c.V0);
        c.RA = 0x80015DB4u;
        GranTurismo2ArcadePC.func_8006E34C(c, m);
        c.A0 = c.S0 + 0x228u;
        c.A1 = c.S1 + 0u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x224u), (ushort)c.V0);
        c.RA = 0x80015DC4u;
        GranTurismo2ArcadePC.func_8006E34C(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x5388u;
        c.A1 = c.S1 + 0u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x2BCu), (ushort)c.V0);
        c.RA = 0x80015DD8u;
        GranTurismo2ArcadePC.func_8006D8EC(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x5368u;
        c.A1 = c.S1 + 0u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x356u), (ushort)c.V0);
        c.RA = 0x80015DECu;
        GranTurismo2ArcadePC.func_8006CED4(c, m);
        c.A0 = c.S0 + 0x2C0u;
        c.A1 = c.S1 + 0u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x358u), (ushort)c.V0);
        c.RA = 0x80015DFCu;
        GranTurismo2ArcadePC.func_8006E34C(c, m);
        c.A0 = 0x00000001u;
        c.V1 = MemoryAccess.ReadU32(m, (c.S0 + 0x4u));
        c.A1 = 0u + 0u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x354u), (ushort)c.V0);
        c.RA = 0x80015E10u;
        Dispatcher.Call(c, m, c.V1);
        c.A0 = c.V0 + 0u;
        L80015E14: ;
        c.V0 = 0xFFFFFFFFu;
        if (c.A0 == c.V0) {
            c.V0 = 0x00000016u;
            goto L80015E90;
        }
        c.V0 = 0x00000016u;
        if (c.A0 != c.V0) {
            c.V1 = c.S0 + 0x30u;
            goto L80015E80;
        }
        c.V1 = c.S0 + 0x30u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V1 + 0x18u));
        if ((int)c.V0 < 0) {
            c.V0 = 0x80020000u;
            goto L80015E60;
        }
        c.V0 = 0x80020000u;
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 + 0x4u));
        c.V0 = ~(0u | c.V0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x18u), (ushort)c.V0);
        c.V1 = c.S0 + 0x4Cu;
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 + 0x10u));
        c.V0 = ~(0u | c.V0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x12u), (ushort)c.V0);
        c.V0 = 0x80020000u;
        L80015E60: ;
        c.V0 = c.V0 + 0xD68u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x24u), c.V0);
        MemoryAccess.WriteU32(m, (c.S0 + 0x28u), c.V0);
        c.RA = 0x80015E70u;
        GranTurismo2ArcadePC.func_8001339C(c, m);
        c.A0 = 0xFFFFFFFFu;
        c.RA = 0x80015E78u;
        GranTurismo2ArcadePC.func_80015A68(c, m);
        c.S2 = 0x00000001u;
        goto L80015E90;
        L80015E80: ;
        c.RA = 0x80015E88u;
        GranTurismo2ArcadePC.func_80015A68(c, m);
        c.A0 = c.V0 + 0u;
        goto L80015E14;
        L80015E90: ;
        c.V0 = c.S2 + 0u;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80015EAC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x78u;
        MemoryAccess.WriteU32(m, (c.SP + 0x68u), c.S4);
        c.S4 = c.A0 + 0u;
        c.V1 = 0x02420000u;
        c.V1 = c.V1 | 0x362Au;
        c.A0 = c.SP + 0x18u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x64u), c.S3);
        c.S3 = MemoryAccess.ReadU32(m, (c.V0 - 0x7530u));
        c.V0 = 0x02000000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x74u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x70u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x6Cu), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x60u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x5Cu), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x58u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x48u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x4Cu), c.V1);
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S3 + 0x2Cu));
        c.S6 = c.S4 + 0x10u;
        c.RA = 0x80015F00u;
        GranTurismo2ArcadePC.func_8006AB78(c, m);
        c.A0 = c.SP + 0x18u;
        c.A1 = 0x800B0000u;
        c.A1 = c.A1 - 0x7518u;
        c.RA = 0x80015F10u;
        GranTurismo2ArcadePC.func_8007D990(c, m);
        c.A1 = 0xFF9F0000u;
        c.A1 = c.A1 | 0xFFFFu;
        c.A0 = 0x025C0000u;
        c.A0 = c.A0 | 0x5248u;
        c.S0 = c.SP + 0x18u;
        c.V1 = MemoryAccess.ReadU32(m, (c.S0 + 0xCu));
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x1Cu), (byte)c.V0);
        c.V0 = 0x00200000u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x10u), c.S4);
        MemoryAccess.WriteU32(m, (c.S0 + 0x14u), c.A0);
        c.V1 = c.V1 & c.A1;
        c.V1 = c.V1 | c.V0;
        c.V0 = 0x801D0000u;
        MemoryAccess.WriteU32(m, (c.S0 + 0xCu), c.V1);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6CC0u));
        if (c.V0 != 0u) {
            c.S1 = 0x00000001u;
            goto L80015F9C;
        }
        c.S1 = 0x00000001u;
        c.S0 = 0x02600000u;
        c.S0 = c.S0 | 0x6060u;
        c.A0 = c.S4 + 0u;
        c.A2 = 0x000000B0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.A1 = MemoryAccess.ReadU32(m, (c.S3 + 0x24u));
        c.A3 = 0x00000102u;
        c.RA = 0x80015F7Cu;
        GranTurismo2ArcadePC.func_80013278(c, m);
        c.A0 = c.S4 + 0u;
        c.A2 = 0x000000B0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.A1 = MemoryAccess.ReadU32(m, (c.S3 + 0x28u));
        c.A3 = 0x00000126u;
        c.RA = 0x80015F94u;
        GranTurismo2ArcadePC.func_80013278(c, m);
        c.S0 = c.S3 + 0x30u;
        goto L80015FD0;
        L80015F9C: ;
        c.A0 = c.S0 + 0u;
        c.A2 = 0x000000B0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S1);
        c.A1 = MemoryAccess.ReadU32(m, (c.S3 + 0x24u));
        c.A3 = 0x00000112u;
        c.RA = 0x80015FB4u;
        GranTurismo2ArcadePC.func_8006ACC4(c, m);
        c.A0 = c.S0 + 0u;
        c.A2 = 0x000000B0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S1);
        c.A1 = MemoryAccess.ReadU32(m, (c.S3 + 0x28u));
        c.A3 = 0x00000136u;
        c.RA = 0x80015FCCu;
        GranTurismo2ArcadePC.func_8006ACC4(c, m);
        c.S0 = c.S3 + 0x30u;
        L80015FD0: ;
        c.A0 = c.S0 + 0u;
        c.RA = 0x80015FD8u;
        GranTurismo2ArcadePC.func_8006BDC4(c, m);
        c.S5 = c.V0 + 0u;
        if (c.S5 == 0u) {
            c.S2 = c.SP + 0x48u;
            goto L800160F8;
        }
        c.S2 = c.SP + 0x48u;
        c.A0 = c.S2 + 0u;
        c.A1 = c.SP + 0x4Cu;
        c.A2 = c.S5 + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x80015FF8u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.SP + 0x18u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6895u;
        c.A2 = 0x00000014u;
        c.A3 = 0x0000008Au;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), 0u);
        c.RA = 0x80016018u;
        GranTurismo2ArcadePC.func_8006ABA0(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S4 + 0u;
        c.A2 = 0x00000010u;
        c.A3 = 0x0000007Au;
        c.RA = 0x8001602Cu;
        GranTurismo2ArcadePC.func_8006BE04(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = 0x00000220u;
        c.RA = 0x80016038u;
        GranTurismo2ArcadePC.func_8007D954(c, m);
        c.S0 = c.S3 + 0x4Cu;
        c.A0 = c.S0 + 0u;
        c.A1 = c.S4 + 0u;
        c.S1 = 0x000000B0u;
        c.V0 = 0x000000C6u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x8u), (ushort)c.S1);
        MemoryAccess.WriteU16(m, (c.S0 + 0xAu), (ushort)c.V0);
        c.RA = 0x80016058u;
        GranTurismo2ArcadePC.func_8006BC18(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S4 + 0u;
        c.V0 = 0x0000006Eu;
        MemoryAccess.WriteU16(m, (c.A0 + 0x8u), (ushort)c.S1);
        MemoryAccess.WriteU16(m, (c.A0 + 0xAu), (ushort)c.V0);
        c.RA = 0x80016070u;
        GranTurismo2ArcadePC.func_8006BC18(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = 0x00000220u;
        c.RA = 0x8001607Cu;
        GranTurismo2ArcadePC.func_8007D954(c, m);
        c.V1 = 0x025A0000u;
        c.V1 = c.V1 | 0x5A5Au;
        c.V0 = 0x02140000u;
        c.V0 = c.V0 | 0x1414u;
        c.A0 = c.S2 + 0u;
        c.A1 = c.SP + 0x50u;
        c.A2 = c.S5 + 0u;
        c.A3 = 0x00000080u;
        MemoryAccess.WriteU32(m, (c.SP + 0x54u), c.V0);
        c.V0 = 0x00000160u;
        MemoryAccess.WriteU16(m, (c.SP + 0x3Cu), (ushort)c.V0);
        c.V0 = 0x0000006Eu;
        MemoryAccess.WriteU16(m, (c.SP + 0x3Au), (ushort)c.V0);
        c.V0 = 0x00000058u;
        MemoryAccess.WriteU32(m, (c.SP + 0x50u), c.V1);
        MemoryAccess.WriteU16(m, (c.SP + 0x38u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.SP + 0x3Eu), (ushort)c.V0);
        c.RA = 0x800160C4u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S2 + 0u;
        c.A1 = c.SP + 0x54u;
        c.A2 = c.S5 + 0u;
        c.A3 = 0x00000080u;
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.V0);
        c.RA = 0x800160DCu;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S6 + 0u;
        c.A1 = c.SP + 0x38u;
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.V0);
        c.RA = 0x800160ECu;
        GranTurismo2ArcadePC.func_8006B5F4(c, m);
        c.A0 = c.S6 + 0u;
        c.A1 = 0x00000200u;
        c.RA = 0x800160F8u;
        GranTurismo2ArcadePC.func_8007D954(c, m);
        L800160F8: ;
        c.A0 = c.S3 + 0x60u;
        c.A1 = c.S4 + 0u;
        c.A2 = c.SP + 0x18u;
        c.RA = 0x80016108u;
        GranTurismo2ArcadePC.func_8006E4C8(c, m);
        c.A0 = c.S3 + 0xF8u;
        c.A1 = c.S4 + 0u;
        c.A2 = c.SP + 0x18u;
        c.RA = 0x80016118u;
        GranTurismo2ArcadePC.func_8006E4C8(c, m);
        c.A0 = c.S3 + 0x190u;
        c.A1 = c.S4 + 0u;
        c.A2 = c.SP + 0x18u;
        c.RA = 0x80016128u;
        GranTurismo2ArcadePC.func_8006E4C8(c, m);
        c.A0 = c.S3 + 0x228u;
        c.A1 = c.S4 + 0u;
        c.A2 = c.SP + 0x18u;
        c.RA = 0x80016138u;
        GranTurismo2ArcadePC.func_8006E4C8(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x5388u;
        c.A1 = c.S4 + 0u;
        c.A2 = c.SP + 0x18u;
        c.RA = 0x8001614Cu;
        GranTurismo2ArcadePC.func_8006DA08(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = 0x00000020u;
        c.RA = 0x80016158u;
        GranTurismo2ArcadePC.func_8007D954(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x5368u;
        c.A1 = c.S6 + 0u;
        c.RA = 0x80016168u;
        GranTurismo2ArcadePC.func_8006D41C(c, m);
        c.A0 = c.S3 + 0x2C0u;
        c.A1 = c.S4 + 0u;
        c.A2 = c.SP + 0x18u;
        c.RA = 0x80016178u;
        GranTurismo2ArcadePC.func_8006E4C8(c, m);
        c.A0 = 0x00000002u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S3 + 0x4u));
        c.A1 = c.S4 + 0u;
        c.RA = 0x8001618Cu;
        Dispatcher.Call(c, m, c.V0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x74u));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x70u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x6Cu));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x68u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x64u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x60u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x5Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x58u));
        c.SP = c.SP + 0x78u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800161B4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x6D88u;
        c.A0 = 0x80020000u;
        c.A0 = c.A0 + 0xD88u;
        MemoryAccess.WriteU32(m, (c.SP + 0x6D80u), c.RA);
        c.A1 = c.SP + 0x10u;
        c.RA = 0x800161CCu;
        GranTurismo2ArcadePC.func_80082EBC(c, m);
        c.A0 = 0x801C0000u;
        c.V0 = 0x801D0000u;
        c.V1 = MemoryAccess.ReadU8(m, (c.V0 - 0x6CC0u));
        c.A0 = c.A0 - 0x6CD0u;
        c.V0 = c.V1 << 3;
        c.V0 = c.V0 - c.V1;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 - c.V1;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 - c.V1;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 - c.V1;
        c.V0 = c.V0 << 1;
        c.V1 = c.SP + 0x10u;
        c.V1 = c.V1 + c.V0;
        c.V0 = c.V1 | c.A0;
        c.V0 = c.V0 & 0x0003u;
        if (c.V0 == 0u) {
            c.V0 = c.V1 + 0xDA0u;
            goto L8001626C;
        }
        c.V0 = c.V1 + 0xDA0u;
        L80016218: ;
        c.A2 = MemoryAccess.ReadWordLeft(m, c.A2, (c.V1 + 0x3u));
        c.A2 = MemoryAccess.ReadWordRight(m, c.A2, c.V1);
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V1 + 0x7u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, (c.V1 + 0x4u));
        c.T0 = MemoryAccess.ReadWordLeft(m, c.T0, (c.V1 + 0xBu));
        c.T0 = MemoryAccess.ReadWordRight(m, c.T0, (c.V1 + 0x8u));
        c.T1 = MemoryAccess.ReadWordLeft(m, c.T1, (c.V1 + 0xFu));
        c.T1 = MemoryAccess.ReadWordRight(m, c.T1, (c.V1 + 0xCu));
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0x3u), c.A2);
        MemoryAccess.WriteWordRight(m, c.A0, c.A2);
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0x7u), c.A3);
        MemoryAccess.WriteWordRight(m, (c.A0 + 0x4u), c.A3);
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0xBu), c.T0);
        MemoryAccess.WriteWordRight(m, (c.A0 + 0x8u), c.T0);
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0xFu), c.T1);
        MemoryAccess.WriteWordRight(m, (c.A0 + 0xCu), c.T1);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.V0) {
            c.A0 = c.A0 + 0x10u;
            goto L80016218;
        }
        c.A0 = c.A0 + 0x10u;
        goto L80016298;
        L8001626C: ;
        c.A2 = MemoryAccess.ReadU32(m, c.V1);
        c.A3 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, c.A0, c.A2);
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.A3);
        MemoryAccess.WriteU32(m, (c.A0 + 0x8u), c.T0);
        MemoryAccess.WriteU32(m, (c.A0 + 0xCu), c.T1);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.V0) {
            c.A0 = c.A0 + 0x10u;
            goto L8001626C;
        }
        c.A0 = c.A0 + 0x10u;
        L80016298: ;
        c.A2 = MemoryAccess.ReadWordLeft(m, c.A2, (c.V1 + 0x3u));
        c.A2 = MemoryAccess.ReadWordRight(m, c.A2, c.V1);
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V1 + 0x7u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, (c.V1 + 0x4u));
        c.T0 = MemoryAccess.ReadWordLeft(m, c.T0, (c.V1 + 0xBu));
        c.T0 = MemoryAccess.ReadWordRight(m, c.T0, (c.V1 + 0x8u));
        c.T1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V1 + 0xCu));
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0x3u), c.A2);
        MemoryAccess.WriteWordRight(m, c.A0, c.A2);
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0x7u), c.A3);
        MemoryAccess.WriteWordRight(m, (c.A0 + 0x4u), c.A3);
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0xBu), c.T0);
        MemoryAccess.WriteWordRight(m, (c.A0 + 0x8u), c.T0);
        MemoryAccess.WriteU8(m, (c.A0 + 0xCu), (byte)c.T1);
        c.A2 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V1 + 0xDu));
        MemoryAccess.WriteU8(m, (c.A0 + 0xDu), (byte)c.A2);
        c.RA = 0x800162DCu;
        GranTurismo2ArcadePC.func_8001D674(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x6D80u));
        c.SP = c.SP + 0x6D88u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800162EC(CpuContext c, IMemory m)
    {
        c.V0 = MemoryAccess.ReadU8(m, c.A1);
        MemoryAccess.WriteU8(m, c.A0, (byte)c.V0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0x2u));
        MemoryAccess.WriteU16(m, (c.A0 + 0x2u), (ushort)c.V0);
        c.V1 = MemoryAccess.ReadU8(m, (c.A1 + 0x1u));
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU16(m, (c.A0 + 0x10u), (ushort)c.V0);
        c.V0 = 0x00000080u;
        MemoryAccess.WriteU16(m, (c.A0 + 0x12u), (ushort)c.V0);
        MemoryAccess.WriteU8(m, (c.A0 + 0x8u), (byte)c.V1);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80016320(CpuContext c, IMemory m)
    {
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0x10u));
        c.V1 = MemoryAccess.ReadU16(m, (c.A0 + 0x10u));
        if ((int)c.V0 < 0) {
            c.V0 = (int)c.V0 < -1 ? 1u : 0u;
            goto L80016354;
        }
        c.V0 = (int)c.V0 < -1 ? 1u : 0u;
        c.V0 = c.V1 + 0x1u;
        MemoryAccess.WriteU16(m, (c.A0 + 0x10u), (ushort)c.V0);
        c.V0 = c.V0 << 16;
        c.V0 = (uint)((int)c.V0 >> 16);
        c.V0 = (int)c.V0 < 12 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = 0x0000000Cu;
            goto L80016360;
        }
        c.V0 = 0x0000000Cu;
        MemoryAccess.WriteU16(m, (c.A0 + 0x10u), (ushort)c.V0);
        return;
        L80016354: ;
        if (c.V0 == 0u) {
            c.V0 = c.V1 + 0x1u;
            goto L80016360;
        }
        c.V0 = c.V1 + 0x1u;
        MemoryAccess.WriteU16(m, (c.A0 + 0x10u), (ushort)c.V0);
        L80016360: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80016368(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x58u;
        MemoryAccess.WriteU32(m, (c.SP + 0x3Cu), c.S3);
        c.S3 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x50u), c.FP);
        c.FP = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.S5);
        c.S5 = 0x00000001u;
        c.T0 = 0u + 0u;
        c.V1 = 0xFFFFFFFFu;
        MemoryAccess.WriteU32(m, (c.SP + 0x54u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x4Cu), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x48u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x38u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x34u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x60u), c.A2);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S3 + 0x10u));
        c.S1 = MemoryAccess.ReadU32(m, (c.S3 + 0xCu));
        if (c.V0 == c.V1) {
            c.A3 = c.T0 + 0u;
            goto L800168D8;
        }
        c.A3 = c.T0 + 0u;
        c.V0 = MemoryAccess.ReadU16(m, (c.S1 + 0x4u));
        c.S4 = (uint)(short)MemoryAccess.ReadU16(m, (c.S3 + 0x4u));
        c.V1 = MemoryAccess.ReadU8(m, c.S3);
        c.S6 = (uint)(short)MemoryAccess.ReadU16(m, (c.S3 + 0x6u));
        c.V0 = c.V0 << 16;
        c.T2 = (uint)((int)c.V0 >> 17);
        c.V1 = c.V1 & 0x0060u;
        c.V0 = 0x00000020u;
        if (c.V1 == c.V0) {
            c.V0 = 0x00000040u;
            goto L800163F4;
        }
        c.V0 = 0x00000040u;
        if (c.V1 == c.V0) {
            goto L800163FC;
        }
        goto L80016400;
        L800163F4: ;
        c.S4 = c.S4 + c.T2;
        goto L80016400;
        L800163FC: ;
        c.S4 = c.S4 - c.T2;
        L80016400: ;
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S3 + 0x10u));
        if ((int)c.A1 >= 0) {
            c.V0 = (int)c.A1 < 12 ? 1u : 0u;
            goto L80016554;
        }
        c.V0 = (int)c.A1 < 12 ? 1u : 0u;
        c.V0 = (int)c.A1 < -1 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A1 = c.A1 + 0xDu;
            goto L800168D8;
        }
        c.A1 = c.A1 + 0xDu;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x4u));
        { var _r = (long)(int)c.A2 * (int)c.A1; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.A2 = c.LO;
        c.A3 = 0x2AAA0000u;
        c.A3 = c.A3 | 0xAAABu;
        { var _r = (long)(int)c.A2 * (int)c.A3; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.T3 = c.HI;
        c.T1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S3 + 0x2u));
        { var _r = (long)(int)c.T1 * (int)c.A1; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V0 = MemoryAccess.ReadU32(m, (c.S3 + 0xCu));
        c.V0 = MemoryAccess.ReadU16(m, (c.V0 + 0x6u));
        c.T1 = c.LO;
        c.V0 = c.V0 << 16;
        c.S2 = (uint)((int)c.V0 >> 17);
        { var _r = (long)(int)c.S2 * (int)c.A1; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.T0 = MemoryAccess.ReadU8(m, (c.S3 + 0x8u));
        c.A0 = c.LO;
        c.V1 = 0u - c.T0;
        { var _r = (long)(int)c.V1 * (int)c.A1; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V1 = c.LO;
        { var _r = (long)(int)c.A0 * (int)c.A3; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.T5 = c.HI;
        { var _r = (long)(int)c.V1 * (int)c.A3; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.A1 = c.HI;
        c.A2 = (uint)((int)c.A2 >> 31);
        c.V0 = (uint)((int)c.T3 >> 1);
        { var _r = (long)(int)c.T1 * (int)c.A3; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.S5 = c.V0 - c.A2;
        c.A0 = (uint)((int)c.A0 >> 31);
        c.V0 = (uint)((int)c.T5 >> 1);
        c.V0 = c.V0 - c.A0;
        c.A2 = c.S2 - c.V0;
        c.V1 = (uint)((int)c.V1 >> 31);
        c.V0 = (uint)((int)c.A1 >> 1);
        c.V0 = c.V0 - c.V1;
        c.T0 = c.T0 + c.V0;
        c.S2 = c.T0 << 1;
        c.T1 = (uint)((int)c.T1 >> 31);
        c.A3 = c.HI;
        c.V0 = (uint)((int)c.A3 >> 1);
        c.S1 = c.V0 - c.T1;
        c.V0 = (int)c.S2 < 193 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.A0 = c.FP + 0u;
            goto L800164F0;
        }
        c.A0 = c.FP + 0u;
        c.S2 = 0x000000C0u;
        L800164F0: ;
        c.A1 = c.SP + 0x10u;
        c.V0 = c.T2 << 1;
        c.V0 = c.V0 + c.S1;
        c.V0 = c.V0 - c.S5;
        MemoryAccess.WriteU16(m, (c.SP + 0x14u), (ushort)c.V0);
        c.V0 = c.S6 - c.A2;
        MemoryAccess.WriteU16(m, (c.SP + 0x12u), (ushort)c.V0);
        c.V0 = c.A2 << 1;
        c.S0 = c.S4 - c.T2;
        MemoryAccess.WriteU16(m, (c.SP + 0x16u), (ushort)c.V0);
        c.V0 = c.S0 + c.S5;
        MemoryAccess.WriteU16(m, (c.SP + 0x10u), (ushort)c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S2);
        c.RA = 0x8001652Cu;
        GranTurismo2ArcadePC.func_8006B52C(c, m);
        c.A0 = c.FP + 0u;
        c.A1 = c.SP + 0x10u;
        c.S0 = c.S0 - c.S1;
        MemoryAccess.WriteU16(m, (c.SP + 0x10u), (ushort)c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), 0u);
        c.RA = 0x80016548u;
        GranTurismo2ArcadePC.func_8006B52C(c, m);
        c.A0 = c.FP + 0u;
        c.A1 = 0x00000020u;
        goto L800168D0;
        L80016554: ;
        c.S2 = MemoryAccess.ReadU8(m, (c.S3 + 0x8u));
        if (c.V0 == 0u) {
            { var _r = (long)(int)c.S2 * (int)c.A1; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
            goto L800165BC;
        }
        { var _r = (long)(int)c.S2 * (int)c.A1; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S3 + 0x2u));
        c.A0 = c.LO;
        c.V0 = 0x0000000Cu;
        c.V0 = c.V0 - c.A1;
        { var _r = (long)(int)c.V1 * (int)c.V0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V1 = c.LO;
        c.V0 = 0x2AAA0000u;
        c.V0 = c.V0 | 0xAAABu;
        { var _r = (long)(int)c.A0 * (int)c.V0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.A3 = c.HI;
        { var _r = (long)(int)c.V1 * (int)c.V0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.T0 = 0x00000001u;
        c.A1 = c.S2 + 0u;
        c.A0 = (uint)((int)c.A0 >> 31);
        c.V0 = (uint)((int)c.A3 >> (int)(c.T0 & 31u));
        c.S2 = c.V0 - c.A0;
        c.A3 = c.A1 - c.S2;
        c.V1 = (uint)((int)c.V1 >> 31);
        c.A2 = c.HI;
        c.V0 = (uint)((int)c.A2 >> (int)(c.T0 & 31u));
        c.S5 = c.V0 - c.V1;
        L800165BC: ;
        c.V1 = MemoryAccess.ReadU8(m, c.S3);
        c.V0 = c.V1 & 0x0004u;
        if (c.V0 != 0u) {
            goto L800165D4;
        }
        c.T0 = 0u + 0u;
        L800165D4: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S3 + 0x12u));
        { var _r = (long)(int)c.S2 * (int)c.V0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V0 = c.V1 & 0x0018u;
        c.S7 = c.V0 << 2;
        MemoryAccess.WriteU16(m, (c.SP + 0x20u), (ushort)c.S7);
        c.T4 = c.LO;
        c.S2 = (uint)((int)c.T4 >> 7);
        if (c.T0 == 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.T4);
            goto L800166CC;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.T4);
        c.A0 = c.FP + 0u;
        c.S0 = c.A3 << 8;
        c.S0 = c.A3 | c.S0;
        c.V0 = c.A3 << 16;
        c.S0 = c.S0 | c.V0;
        c.V0 = 0x02000000u;
        c.S0 = c.S0 | c.V0;
        c.A1 = c.S0 + 0u;
        c.RA = 0x80016620u;
        GranTurismo2ArcadePC.func_80081388(c, m);
        c.A2 = c.V0 + 0u;
        c.A0 = c.S4 - c.S5;
        c.V1 = MemoryAccess.ReadU16(m, (c.S1 + 0x4u));
        c.V0 = MemoryAccess.ReadU16(m, (c.S1 + 0x6u));
        c.V1 = c.V1 << 16;
        c.V1 = (uint)((int)c.V1 >> 17);
        c.A0 = c.A0 - c.V1;
        c.V0 = c.V0 << 16;
        c.V0 = (uint)((int)c.V0 >> 17);
        c.V0 = c.S6 - c.V0;
        c.V0 = c.V0 << 16;
        c.A0 = c.A0 + c.V0;
        MemoryAccess.WriteU32(m, c.A2, c.A0);
        c.V0 = MemoryAccess.ReadU32(m, c.S1);
        c.A1 = c.S0 + 0u;
        MemoryAccess.WriteU32(m, (c.A2 + 0x4u), c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x4u));
        c.A0 = c.FP + 0u;
        MemoryAccess.WriteU32(m, (c.A2 + 0x8u), c.V0);
        c.RA = 0x80016670u;
        GranTurismo2ArcadePC.func_80081388(c, m);
        c.A2 = c.V0 + 0u;
        c.A0 = c.S4 + c.S5;
        c.V1 = MemoryAccess.ReadU16(m, (c.S1 + 0x4u));
        c.V0 = MemoryAccess.ReadU16(m, (c.S1 + 0x6u));
        c.V1 = c.V1 << 16;
        c.V1 = (uint)((int)c.V1 >> 17);
        c.A0 = c.A0 - c.V1;
        c.V0 = c.V0 << 16;
        c.V0 = (uint)((int)c.V0 >> 17);
        c.V0 = c.S6 - c.V0;
        c.V0 = c.V0 << 16;
        c.A0 = c.A0 + c.V0;
        MemoryAccess.WriteU32(m, c.A2, c.A0);
        c.V0 = MemoryAccess.ReadU32(m, c.S1);
        MemoryAccess.WriteU32(m, (c.A2 + 0x4u), c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x4u));
        MemoryAccess.WriteU32(m, (c.A2 + 0x8u), c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x8u));
        c.A0 = c.FP + 0u;
        c.A1 = c.A1 | c.S7;
        c.RA = 0x800166CCu;
        GranTurismo2ArcadePC.func_8007D954(c, m);
        L800166CC: ;
        c.V0 = MemoryAccess.ReadU8(m, c.S3);
        c.V0 = c.V0 & 0x0002u;
        if (c.V0 == 0u) {
            c.A0 = c.FP + 0u;
            goto L80016864;
        }
        c.A0 = c.FP + 0u;
        c.A1 = c.S2 << 8;
        c.A1 = c.S2 | c.A1;
        c.V0 = c.S2 << 16;
        c.A1 = c.A1 | c.V0;
        c.V0 = 0x02000000u;
        c.A1 = c.A1 | c.V0;
        c.RA = 0x800166FCu;
        GranTurismo2ArcadePC.func_80081388(c, m);
        c.A2 = c.V0 + 0u;
        c.V1 = MemoryAccess.ReadU16(m, (c.S1 + 0x4u));
        c.V0 = MemoryAccess.ReadU16(m, (c.S1 + 0x6u));
        c.V1 = c.V1 << 16;
        c.V1 = (uint)((int)c.V1 >> 17);
        c.V1 = c.S4 - c.V1;
        c.V0 = c.V0 << 16;
        c.V0 = (uint)((int)c.V0 >> 17);
        c.V0 = c.S6 - c.V0;
        c.V0 = c.V0 << 16;
        c.V1 = c.V1 + c.V0;
        MemoryAccess.WriteU32(m, c.A2, c.V1);
        c.V0 = MemoryAccess.ReadU32(m, c.S1);
        MemoryAccess.WriteU32(m, (c.A2 + 0x4u), c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x4u));
        MemoryAccess.WriteU32(m, (c.A2 + 0x8u), c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x8u));
        c.A0 = c.FP + 0u;
        c.A1 = c.A1 | c.S7;
        c.RA = 0x80016754u;
        GranTurismo2ArcadePC.func_8007D954(c, m);
        c.V0 = MemoryAccess.ReadU8(m, c.S3);
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u) {
            goto L800168C0;
        }
        c.T4 = MemoryAccess.ReadU32(m, (c.SP + 0x60u));
        if (c.T4 == 0u) {
            c.A0 = c.FP + 0u;
            goto L800167DC;
        }
        c.A0 = c.FP + 0u;
        c.A1 = 0x02000000u;
        c.RA = 0x80016780u;
        GranTurismo2ArcadePC.func_80081388(c, m);
        c.A2 = c.V0 + 0u;
        c.V1 = MemoryAccess.ReadU16(m, (c.S1 + 0x4u));
        c.V0 = MemoryAccess.ReadU16(m, (c.S1 + 0x6u));
        c.V1 = c.V1 << 16;
        c.V1 = (uint)((int)c.V1 >> 17);
        c.V1 = c.S4 - c.V1;
        c.V0 = c.V0 << 16;
        c.V0 = (uint)((int)c.V0 >> 17);
        c.V0 = c.S6 - c.V0;
        c.V0 = c.V0 << 16;
        c.V1 = c.V1 + c.V0;
        MemoryAccess.WriteU32(m, c.A2, c.V1);
        c.V0 = MemoryAccess.ReadU32(m, c.S1);
        MemoryAccess.WriteU32(m, (c.A2 + 0x4u), c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x4u));
        MemoryAccess.WriteU32(m, (c.A2 + 0x8u), c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x8u));
        c.A0 = c.FP + 0u;
        c.RA = 0x800167D4u;
        GranTurismo2ArcadePC.func_8007D954(c, m);
        c.A0 = c.FP + 0u;
        goto L800167E8;
        L800167DC: ;
        c.T4 = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.S2 = (uint)((int)c.T4 >> 8);
        L800167E8: ;
        c.A1 = c.S2 << 8;
        c.A1 = c.S2 | c.A1;
        c.V0 = c.S2 << 16;
        c.A1 = c.A1 | c.V0;
        c.V0 = 0x02000000u;
        c.A1 = c.A1 | c.V0;
        c.RA = 0x80016804u;
        GranTurismo2ArcadePC.func_80081388(c, m);
        c.A2 = c.V0 + 0u;
        c.V1 = MemoryAccess.ReadU16(m, (c.S1 + 0x4u));
        c.V0 = MemoryAccess.ReadU16(m, (c.S1 + 0x6u));
        c.V1 = c.V1 << 16;
        c.V1 = (uint)((int)c.V1 >> 17);
        c.V1 = c.S4 - c.V1;
        c.V0 = c.V0 << 16;
        c.V0 = (uint)((int)c.V0 >> 17);
        c.V0 = c.S6 - c.V0;
        c.V0 = c.V0 << 16;
        c.V1 = c.V1 + c.V0;
        MemoryAccess.WriteU32(m, c.A2, c.V1);
        c.V0 = MemoryAccess.ReadU32(m, c.S1);
        MemoryAccess.WriteU32(m, (c.A2 + 0x4u), c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x4u));
        MemoryAccess.WriteU32(m, (c.A2 + 0x8u), c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x8u));
        c.A0 = c.FP + 0u;
        c.A1 = c.A1 | 0x0040u;
        c.RA = 0x8001685Cu;
        GranTurismo2ArcadePC.func_8007D954(c, m);
        goto L800168C0;
        L80016864: ;
        c.A1 = c.S2 << 8;
        c.A1 = c.S2 | c.A1;
        c.V0 = c.S2 << 16;
        c.A1 = c.A1 | c.V0;
        c.RA = 0x80016878u;
        GranTurismo2ArcadePC.func_80081388(c, m);
        c.A2 = c.V0 + 0u;
        c.V1 = MemoryAccess.ReadU16(m, (c.S1 + 0x4u));
        c.V0 = MemoryAccess.ReadU16(m, (c.S1 + 0x6u));
        c.V1 = c.V1 << 16;
        c.V1 = (uint)((int)c.V1 >> 17);
        c.V1 = c.S4 - c.V1;
        c.V0 = c.V0 << 16;
        c.V0 = (uint)((int)c.V0 >> 17);
        c.V0 = c.S6 - c.V0;
        c.V0 = c.V0 << 16;
        c.V1 = c.V1 + c.V0;
        MemoryAccess.WriteU32(m, c.A2, c.V1);
        c.V0 = MemoryAccess.ReadU32(m, c.S1);
        MemoryAccess.WriteU32(m, (c.A2 + 0x4u), c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x4u));
        MemoryAccess.WriteU32(m, (c.A2 + 0x8u), c.V0);
        L800168C0: ;
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x8u));
        c.T4 = MemoryAccess.ReadU16(m, (c.SP + 0x20u));
        c.A0 = c.FP + 0u;
        c.A1 = c.A1 | c.T4;
        L800168D0: ;
        c.RA = 0x800168D8u;
        GranTurismo2ArcadePC.func_8007D954(c, m);
        L800168D8: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x54u));
        c.FP = MemoryAccess.ReadU32(m, (c.SP + 0x50u));
        c.S7 = MemoryAccess.ReadU32(m, (c.SP + 0x4Cu));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x48u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x44u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x40u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x3Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x38u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x34u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x30u));
        c.SP = c.SP + 0x58u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80016908(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        c.V1 = 0x801D0000u;
        c.V1 = MemoryAccess.ReadU8(m, (c.V1 - 0x6CC0u));
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A8u));
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        if (c.V1 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
            goto L80016954;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.A0 = c.S1 + 0u;
        c.RA = 0x80016934u;
        GranTurismo2ArcadePC.func_8007F084(c, m);
        c.A0 = c.S1 + 0u;
        c.S0 = 0x801F0000u;
        c.S0 = c.S0 - 0xAA0u;
        c.A1 = c.S0 + 0u;
        c.RA = 0x80016948u;
        GranTurismo2ArcadePC.func_8006EBE8(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S0 + 0x11u;
        c.RA = 0x80016954u;
        GranTurismo2ArcadePC.func_8006EBE8(c, m);
        L80016954: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80016968(CpuContext c, IMemory m)
    {
        c.A1 = c.A0 + 0u;
        c.V0 = 0x800B0000u;
        c.V1 = 0x801D0000u;
        c.V1 = MemoryAccess.ReadU8(m, (c.V1 - 0x6CC0u));
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A8u));
        c.SP = c.SP - 0x18u;
        if (c.V1 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
            goto L80016990;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.RA = 0x80016990u;
        GranTurismo2ArcadePC.func_8006EBE8(c, m);
        L80016990: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800169A0(CpuContext c, IMemory m)
    {
        c.V0 = 0x801D0000u;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6CC0u));
        c.SP = c.SP - 0x18u;
        if (c.V0 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
            goto L800169DC;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.RA = 0x800169BCu;
        GranTurismo2ArcadePC.func_80016908(c, m);
        c.V0 = 0x800B0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0xF14u));
        c.RA = 0x800169CCu;
        GranTurismo2ArcadePC.func_80016968(c, m);
        c.V0 = 0x800B0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0xF18u));
        c.RA = 0x800169DCu;
        GranTurismo2ArcadePC.func_80016968(c, m);
        L800169DC: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800169EC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x38u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        c.S4 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = c.A3 + 0u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0x48u));
        c.V0 = 0x801D0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.S7);
        c.S7 = MemoryAccess.ReadU32(m, (c.SP + 0x4Cu));
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6CC0u));
        c.V1 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S5);
        c.S5 = MemoryAccess.ReadU32(m, (c.V1 - 0x75A8u));
        c.A0 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        if (c.V0 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
            goto L80016B04;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        if (c.A2 == 0u) {
            c.S0 = c.A1 + 0u;
            goto L80016A78;
        }
        c.S0 = c.A1 + 0u;
        c.V0 = MemoryAccess.ReadU8(m, c.A1);
        if (c.V0 == 0u) {
            c.V1 = c.A1 + 0u;
            goto L80016A6C;
        }
        c.V1 = c.A1 + 0u;
        L80016A58: ;
        c.V1 = c.V1 + 0x2u;
        c.V0 = MemoryAccess.ReadU8(m, c.V1);
        if (c.V0 != 0u) {
            c.A0 = c.A0 + 0xCu;
            goto L80016A58;
        }
        c.A0 = c.A0 + 0xCu;
        L80016A6C: ;
        c.V0 = (uint)((int)c.A0 >> 1);
        c.S1 = c.S1 - c.V0;
        c.S0 = c.A1 + 0u;
        L80016A78: ;
        c.S6 = 0x02000000u;
        c.S2 = c.S3 + 0x1Fu;
        L80016A80: ;
        c.V0 = MemoryAccess.ReadU8(m, c.S0);
        if (c.V0 == 0u) {
            c.A0 = c.S5 + 0u;
            goto L80016AF8;
        }
        c.A0 = c.S5 + 0u;
        c.A1 = c.V0 + 0u;
        c.V0 = MemoryAccess.ReadU8(m, (c.S0 + 0x1u));
        c.A1 = c.A1 << 8;
        c.A1 = c.A1 | c.V0;
        c.RA = 0x80016AA4u;
        GranTurismo2ArcadePC.func_8007F09C(c, m);
        if (c.V0 == 0u) {
            c.A0 = c.V0 + 0u;
            goto L80016AEC;
        }
        c.A0 = c.V0 + 0u;
        c.A1 = c.S4 + 0u;
        c.A2 = c.S7 | c.S6;
        c.RA = 0x80016AB8u;
        GranTurismo2ArcadePC.func_8007EF2C(c, m);
        c.V1 = MemoryAccess.ReadU16(m, (c.V0 + 0x12u));
        c.A0 = c.S1 + 0xFu;
        MemoryAccess.WriteU16(m, (c.V0 + 0x4u), (ushort)c.S1);
        MemoryAccess.WriteU16(m, (c.V0 + 0x6u), (ushort)c.S3);
        MemoryAccess.WriteU16(m, (c.V0 + 0xCu), (ushort)c.A0);
        MemoryAccess.WriteU16(m, (c.V0 + 0xEu), (ushort)c.S3);
        MemoryAccess.WriteU16(m, (c.V0 + 0x14u), (ushort)c.S1);
        MemoryAccess.WriteU16(m, (c.V0 + 0x16u), (ushort)c.S2);
        MemoryAccess.WriteU16(m, (c.V0 + 0x1Cu), (ushort)c.A0);
        MemoryAccess.WriteU16(m, (c.V0 + 0x1Eu), (ushort)c.S2);
        c.V1 = c.V1 & 0xFF9Fu;
        c.V1 = c.V1 | 0x0020u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x12u), (ushort)c.V1);
        L80016AEC: ;
        c.S1 = c.S1 + 0xCu;
        c.S0 = c.S0 + 0x2u;
        goto L80016A80;
        L80016AF8: ;
        c.A0 = c.S4 + 0u;
        c.A1 = 0x00000220u;
        c.RA = 0x80016B04u;
        GranTurismo2ArcadePC.func_8007D954(c, m);
        L80016B04: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x30u));
        c.S7 = MemoryAccess.ReadU32(m, (c.SP + 0x2Cu));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x38u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80016B30(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V0 + 0xF08u), 0u);
        c.V0 = 0x800B0000u;
        c.V1 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V0 + 0xF10u), 0u);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x2288u;
        MemoryAccess.WriteU32(m, (c.V1 + 0xF14u), c.V0);
        c.V1 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU32(m, (c.V1 + 0xF18u), c.V0);
        c.RA = 0x80016B64u;
        GranTurismo2ArcadePC.func_800169A0(c, m);
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 - 0x74F8u;
        c.RA = 0x80016B70u;
        GranTurismo2ArcadePC.func_80069F48(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80016B80(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.V1 = 0x800B0000u;
        c.V0 = 0x800B0000u;
        c.A0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0xF10u));
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0xF08u));
        c.V1 = c.V1 - 0x75A4u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.V0 = c.V0 << 1;
        c.V0 = c.V0 + c.V1;
        c.V1 = 0x800B0000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 - 0x75A0u));
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, c.V0);
        c.A2 = c.V1 + 0x1A4u;
        c.V1 = c.A1 < 0x0000000Bu ? 1u : 0u;
        if (c.V1 == 0u) {
            c.S1 = 0u + 0u;
            goto L80016EAC;
        }
        c.S1 = 0u + 0u;
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x2290u;
        c.V1 = c.A1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x80016BECu: goto L80016BEC;
            case 0x80016E64u: goto L80016E64;
            case 0x80016C60u: goto L80016C60;
            case 0x80016CE0u: goto L80016CE0;
            case 0x80016D4Cu: goto L80016D4C;
            case 0x80016E2Cu: goto L80016E2C;
            case 0x80016DE0u: goto L80016DE0;
            case 0x80016E74u: goto L80016E74;
            case 0x80016E90u: goto L80016E90;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L80016BEC: ;
        if (c.A0 == 0u) {
            c.V0 = 0x00000001u;
            goto L80016C04;
        }
        c.V0 = 0x00000001u;
        if (c.A0 == c.V0) {
            c.A1 = 0x800B0000u;
            goto L80016EAC;
        }
        c.A1 = 0x800B0000u;
        c.A0 = 0x800B0000u;
        goto L80016C48;
        L80016C04: ;
        c.S0 = 0x800B0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.S0 + 0xF10u));
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 - 0x74F8u;
        c.RA = 0x80016C18u;
        GranTurismo2ArcadePC.func_80069FE4(c, m);
        if (c.V0 == 0u) {
            c.V1 = 0x800B0000u;
            goto L80016C2C;
        }
        c.V1 = 0x800B0000u;
        c.V0 = 0x00000002u;
        MemoryAccess.WriteU32(m, (c.V1 + 0xF08u), c.V0);
        goto L80016EAC;
        L80016C2C: ;
        c.V0 = 0x800B0000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.S0 + 0xF10u));
        c.A0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.V0 + 0xF08u), c.A0);
        c.V1 = c.V1 + c.A0;
        MemoryAccess.WriteU32(m, (c.S0 + 0xF10u), c.V1);
        goto L80016EAC;
        L80016C48: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.A0 + 0xF10u));
        c.V1 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.A1 + 0xF08u), c.V1);
        c.V0 = c.V0 + c.V1;
        MemoryAccess.WriteU32(m, (c.A0 + 0xF10u), c.V0);
        goto L80016EAC;
        L80016C60: ;
        if (c.A0 != 0u) {
            c.V1 = 0x800B0000u;
            goto L80016D40;
        }
        c.V1 = 0x800B0000u;
        c.V0 = 0x800B0000u;
        c.S0 = c.V0 - 0x74F8u;
        c.A0 = c.S0 + 0u;
        c.V0 = 0x800B0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0xF10u));
        c.A2 = c.S0 + 0u;
        c.RA = 0x80016C84u;
        GranTurismo2ArcadePC.func_8006A0C4(c, m);
        if (c.V0 == 0u) {
            c.V1 = 0x800B0000u;
            goto L80016C98;
        }
        c.V1 = 0x800B0000u;
        c.V0 = 0x00000007u;
        MemoryAccess.WriteU32(m, (c.V1 + 0xF08u), c.V0);
        goto L80016CD4;
        L80016C98: ;
        c.V1 = 0x800B0000u;
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0xD34u;
        MemoryAccess.WriteU32(m, (c.V1 + 0xF14u), c.V0);
        c.V1 = 0x800B0000u;
        c.V0 = c.V0 - 0x151u;
        MemoryAccess.WriteU32(m, (c.V1 + 0xF18u), c.V0);
        c.RA = 0x80016CB8u;
        GranTurismo2ArcadePC.func_800169A0(c, m);
        c.A0 = c.S0 + 0u;
        c.RA = 0x80016CC0u;
        GranTurismo2ArcadePC.func_8006A03C(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x5260u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x10u), c.V0);
        c.RA = 0x80016CD0u;
        GranTurismo2ArcadePC.func_8006BF5C(c, m);
        c.V1 = 0x800B0000u;
        L80016CD4: ;
        c.V0 = 0x00000003u;
        MemoryAccess.WriteU32(m, (c.V1 + 0xF08u), c.V0);
        goto L80016EAC;
        L80016CE0: ;
        c.RA = 0x80016CE8u;
        GranTurismo2ArcadePC.func_8007D6DC(c, m);
        c.V1 = c.V0 + 0u;
        if (c.V1 == 0u) {
            c.V0 = 0x00000001u;
            goto L80016D18;
        }
        c.V0 = 0x00000001u;
        if (c.V1 != c.V0) {
            c.V1 = 0x80050000u;
            goto L80016D34;
        }
        c.V1 = 0x80050000u;
        c.V0 = 0x801F0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0xC8u));
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x5260u;
        c.RA = 0x80016D10u;
        GranTurismo2ArcadePC.func_8006BF84(c, m);
        c.V0 = c.S1 + 0u;
        goto L80016EB0;
        L80016D18: ;
        c.V1 = 0x80050000u;
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU16(m, (c.V1 - 0x522Cu), (ushort)c.V0);
        c.V1 = 0x800B0000u;
        c.V0 = 0x00000004u;
        MemoryAccess.WriteU32(m, (c.V1 + 0xF08u), c.V0);
        goto L80016EAC;
        L80016D34: ;
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU16(m, (c.V1 - 0x522Cu), (ushort)c.V0);
        c.V1 = 0x800B0000u;
        L80016D40: ;
        c.V0 = 0x00000007u;
        MemoryAccess.WriteU32(m, (c.V1 + 0xF08u), c.V0);
        goto L80016EAC;
        L80016D4C: ;
        c.V0 = 0x800B0000u;
        c.S0 = c.V0 - 0x74F8u;
        c.A0 = c.S0 + 0u;
        c.A1 = c.S0 + 0u;
        c.RA = 0x80016D60u;
        GranTurismo2ArcadePC.func_8006A224(c, m);
        if (c.V0 == 0u) {
            c.A0 = c.S0 + 0u;
            goto L80016DA8;
        }
        c.A0 = c.S0 + 0u;
        c.A1 = c.A0 + 0u;
        c.RA = 0x80016D70u;
        GranTurismo2ArcadePC.func_8006A188(c, m);
        c.V1 = 0x800B0000u;
        c.V0 = 0x000000B4u;
        MemoryAccess.WriteU32(m, (c.V1 + 0xF0Cu), c.V0);
        c.V1 = 0x800B0000u;
        c.V0 = 0x00000005u;
        MemoryAccess.WriteU32(m, (c.V1 + 0xF08u), c.V0);
        c.V1 = 0x800B0000u;
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x2288u;
        MemoryAccess.WriteU32(m, (c.V1 + 0xF14u), c.V0);
        c.V1 = 0x800B0000u;
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0xD21u;
        goto L80016E1C;
        L80016DA8: ;
        c.V1 = 0x800B0000u;
        c.V0 = 0x000000F0u;
        MemoryAccess.WriteU32(m, (c.V1 + 0xF0Cu), c.V0);
        c.V1 = 0x800B0000u;
        c.V0 = 0x00000006u;
        MemoryAccess.WriteU32(m, (c.V1 + 0xF08u), c.V0);
        c.V1 = 0x800B0000u;
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x2288u;
        MemoryAccess.WriteU32(m, (c.V1 + 0xF14u), c.V0);
        c.V1 = 0x800B0000u;
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0xC0Cu;
        goto L80016E1C;
        L80016DE0: ;
        c.A0 = 0u + 0u;
        c.RA = 0x80016DE8u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.V1 = 0x800B0000u;
        c.V0 = 0x000000F0u;
        MemoryAccess.WriteU32(m, (c.V1 + 0xF0Cu), c.V0);
        c.V1 = 0x800B0000u;
        c.V0 = 0x00000008u;
        MemoryAccess.WriteU32(m, (c.V1 + 0xF08u), c.V0);
        c.V1 = 0x800B0000u;
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x2288u;
        MemoryAccess.WriteU32(m, (c.V1 + 0xF14u), c.V0);
        c.V1 = 0x800B0000u;
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0xD0Au;
        L80016E1C: ;
        MemoryAccess.WriteU32(m, (c.V1 + 0xF18u), c.V0);
        c.RA = 0x80016E24u;
        GranTurismo2ArcadePC.func_800169A0(c, m);
        c.V0 = c.S1 + 0u;
        goto L80016EB0;
        L80016E2C: ;
        c.S0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0xF0Cu));
        c.V1 = MemoryAccess.ReadU32(m, (c.A2 + 0x4u));
        c.V0 = c.V0 - 0x1u;
        c.V1 = c.V1 & 0x0F00u;
        if (c.V1 == 0u) {
            MemoryAccess.WriteU32(m, (c.S0 + 0xF0Cu), c.V0);
            goto L80016E54;
        }
        MemoryAccess.WriteU32(m, (c.S0 + 0xF0Cu), c.V0);
        MemoryAccess.WriteU32(m, (c.S0 + 0xF0Cu), 0u);
        c.A0 = 0x00000001u;
        c.RA = 0x80016E54u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        L80016E54: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0xF0Cu));
        if (c.V0 != 0u) {
            c.V0 = c.S1 + 0u;
            goto L80016EB0;
        }
        c.V0 = c.S1 + 0u;
        L80016E64: ;
        c.V1 = 0x800B0000u;
        c.V0 = 0x00000009u;
        MemoryAccess.WriteU32(m, (c.V1 + 0xF08u), c.V0);
        goto L80016EAC;
        L80016E74: ;
        c.V1 = 0x800B0000u;
        c.V0 = 0x00000006u;
        MemoryAccess.WriteU32(m, (c.V1 + 0xF0Cu), c.V0);
        c.V1 = 0x800B0000u;
        c.V0 = 0x0000000Au;
        MemoryAccess.WriteU32(m, (c.V1 + 0xF08u), c.V0);
        goto L80016EAC;
        L80016E90: ;
        c.V1 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0xF0Cu));
        c.V0 = c.V0 - 0x1u;
        if (c.V0 != 0u) {
            MemoryAccess.WriteU32(m, (c.V1 + 0xF0Cu), c.V0);
            goto L80016EAC;
        }
        MemoryAccess.WriteU32(m, (c.V1 + 0xF0Cu), c.V0);
        c.S1 = 0x00000004u;
        L80016EAC: ;
        c.V0 = c.S1 + 0u;
        L80016EB0: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80016EC4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x50u;
        MemoryAccess.WriteU32(m, (c.SP + 0x3Cu), c.S1);
        c.S1 = 0x00600000u;
        c.S1 = c.S1 | 0x4610u;
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.S2);
        c.S2 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x48u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x38u), c.S0);
        c.S3 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.A0 = c.SP + 0x18u;
        c.A1 = 0x0000001Eu;
        c.RA = 0x80016EF8u;
        GranTurismo2ArcadePC.func_8006AB78(c, m);
        c.S0 = c.SP + 0x18u;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6CE0u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.S0 + 0x10u), c.S3);
        c.RA = 0x80016F14u;
        GranTurismo2ArcadePC.func_8007D990(c, m);
        c.V0 = 0x800B0000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0xF08u));
        c.V0 = 0x00000005u;
        if (c.V1 == c.V0) {
            c.V0 = (int)c.V1 < 6 ? 1u : 0u;
            goto L80017000;
        }
        c.V0 = (int)c.V1 < 6 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000003u;
            goto L80016F40;
        }
        c.V0 = 0x00000003u;
        if (c.V1 == c.V0) {
            c.V0 = 0x801D0000u;
            goto L80016F5C;
        }
        c.V0 = 0x801D0000u;
        goto L800170EC;
        L80016F40: ;
        c.V0 = 0x00000006u;
        if (c.V1 == c.V0) {
            c.V0 = 0x00000008u;
            goto L80017060;
        }
        c.V0 = 0x00000008u;
        if (c.V1 == c.V0) {
            c.S1 = 0x020A0000u;
            goto L8001703C;
        }
        c.S1 = 0x020A0000u;
        goto L800170EC;
        L80016F5C: ;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6CC0u));
        if (c.V0 != 0u) {
            c.V0 = 0x00000001u;
            goto L80016FCC;
        }
        c.V0 = 0x00000001u;
        c.S1 = 0x026E0000u;
        c.S1 = c.S1 | 0x5A50u;
        c.A0 = c.S3 + 0u;
        c.A2 = 0x000000B0u;
        c.A3 = 0x000000D0u;
        c.V0 = 0x800B0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0xF14u));
        c.S0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.RA = 0x80016F98u;
        GranTurismo2ArcadePC.func_800169EC(c, m);
        c.A0 = c.S3 + 0u;
        c.A2 = 0x000000B0u;
        c.V0 = 0x800B0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0xF18u));
        c.A3 = 0x000000F0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.RA = 0x80016FB8u;
        GranTurismo2ArcadePC.func_800169EC(c, m);
        c.A0 = c.S3 + 0u;
        c.A1 = 0x00000220u;
        c.RA = 0x80016FC4u;
        GranTurismo2ArcadePC.func_8007D954(c, m);
        c.A0 = 0x80050000u;
        goto L80016FEC;
        L80016FCC: ;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x69CAu;
        c.A2 = 0x000000B0u;
        c.A3 = 0x000000F0u;
        c.RA = 0x80016FE8u;
        GranTurismo2ArcadePC.func_8006ACC4(c, m);
        c.A0 = 0x80050000u;
        L80016FEC: ;
        c.A0 = c.A0 - 0x5260u;
        c.A1 = c.S3 + 0u;
        c.RA = 0x80016FF8u;
        GranTurismo2ArcadePC.func_8006C084(c, m);
        c.S2 = 0x00000001u;
        goto L800170EC;
        L80017000: ;
        c.V0 = 0x801D0000u;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6CC0u));
        if (c.V0 != 0u) {
            c.V0 = 0x00000001u;
            goto L80017020;
        }
        c.V0 = 0x00000001u;
        c.S1 = 0x026E0000u;
        c.S1 = c.S1 | 0x5A50u;
        goto L80017078;
        L80017020: ;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x69A8u;
        c.A2 = 0x000000B0u;
        c.A3 = 0x000000F0u;
        goto L800170E4;
        L8001703C: ;
        c.V0 = 0x801D0000u;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6CC0u));
        if (c.V0 == 0u) {
            c.S1 = c.S1 | 0x50F0u;
            goto L80017078;
        }
        c.S1 = c.S1 | 0x50F0u;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x698Du;
        goto L800170D0;
        L80017060: ;
        c.S1 = 0x020A0000u;
        c.V0 = 0x801D0000u;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6CC0u));
        if (c.V0 != 0u) {
            c.S1 = c.S1 | 0x50F0u;
            goto L800170C4;
        }
        c.S1 = c.S1 | 0x50F0u;
        L80017078: ;
        c.A0 = c.S3 + 0u;
        c.A2 = 0x000000B0u;
        c.A3 = 0x000000D0u;
        c.V0 = 0x800B0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0xF14u));
        c.S0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.RA = 0x8001709Cu;
        GranTurismo2ArcadePC.func_800169EC(c, m);
        c.A0 = c.S3 + 0u;
        c.A2 = 0x000000B0u;
        c.V0 = 0x800B0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0xF18u));
        c.A3 = 0x000000F0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.RA = 0x800170BCu;
        GranTurismo2ArcadePC.func_800169EC(c, m);
        c.S2 = 0x00000001u;
        goto L800170EC;
        L800170C4: ;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6971u;
        L800170D0: ;
        c.A2 = 0x000000B0u;
        c.A3 = 0x000000F0u;
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        L800170E4: ;
        c.S2 = 0x00000001u;
        c.RA = 0x800170ECu;
        GranTurismo2ArcadePC.func_8006ACC4(c, m);
        L800170EC: ;
        if (c.S2 == 0u) {
            c.S1 = 0x026E0000u;
            goto L80017158;
        }
        c.S1 = 0x026E0000u;
        c.V0 = 0x801F0000u;
        c.A1 = c.V0 - 0xAA0u;
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0xF10u));
        c.V1 = 0x00000001u;
        if (c.V0 != c.V1) {
            c.S1 = c.S1 | 0x6460u;
            goto L80017114;
        }
        c.S1 = c.S1 | 0x6460u;
        c.A1 = c.A1 + 0x11u;
        L80017114: ;
        c.V0 = 0x801D0000u;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6CC0u));
        if (c.V0 != 0u) {
            c.A0 = c.SP + 0x18u;
            goto L80017148;
        }
        c.A0 = c.SP + 0x18u;
        c.A0 = c.S3 + 0u;
        c.A2 = 0x000000B0u;
        c.A3 = 0x00000094u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V1);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.RA = 0x80017140u;
        GranTurismo2ArcadePC.func_800169EC(c, m);
        goto L80017158;
        L80017148: ;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V1);
        c.A2 = 0x000000B0u;
        c.A3 = 0x000000B4u;
        c.RA = 0x80017158u;
        GranTurismo2ArcadePC.func_8006ACC4(c, m);
        L80017158: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x48u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x44u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x40u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x3Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x38u));
        c.SP = c.SP + 0x50u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80017174(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x58u;
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.S3);
        c.S3 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x3Cu), c.S1);
        c.S1 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x4Cu), c.S5);
        c.S5 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x48u), c.S4);
        c.S4 = 0u + 0u;
        c.A0 = c.SP + 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x50u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x38u), c.S0);
        c.S2 = MemoryAccess.ReadU32(m, c.S1);
        c.S0 = MemoryAccess.ReadU32(m, (c.S1 + 0x4u));
        c.A1 = 0x0000001Eu;
        c.RA = 0x800171B8u;
        GranTurismo2ArcadePC.func_8006AB78(c, m);
        c.V0 = 0x02600000u;
        c.V0 = c.V0 | 0x4610u;
        c.A0 = c.SP + 0x18u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6CE0u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.A0 + 0x10u), c.S0);
        c.RA = 0x800171D8u;
        GranTurismo2ArcadePC.func_8007D990(c, m);
        c.V0 = c.S3 < 0x00000009u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80020000u;
            goto L8001725C;
        }
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x22F8u;
        c.V1 = c.S3 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x8001725Cu: goto L8001725C;
            case 0x80017200u: goto L80017200;
            case 0x80017258u: goto L80017258;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L80017200: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        if (c.S5 != c.V0) {
            c.V0 = 0x02000000u;
            goto L80017218;
        }
        c.V0 = 0x02000000u;
        c.V0 = c.V0 | 0x6478u;
        goto L80017220;
        L80017218: ;
        c.V0 = 0x02280000u;
        c.V0 = c.V0 | 0x2828u;
        L80017220: ;
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.V0);
        c.V0 = 0x00000001u;
        c.V1 = 0x80050000u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0xCu));
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0xEu));
        c.V1 = c.V1 - 0x5228u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.V0 = c.S5 << 2;
        c.V0 = c.V0 + c.V1;
        c.A1 = MemoryAccess.ReadU32(m, c.V0);
        c.A0 = c.SP + 0x18u;
        c.RA = 0x80017250u;
        GranTurismo2ArcadePC.func_8006ABA0(c, m);
        c.V0 = c.S4 + 0u;
        goto L80017260;
        L80017258: ;
        c.S4 = 0x00000001u;
        L8001725C: ;
        c.V0 = c.S4 + 0u;
        L80017260: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x50u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x4Cu));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x48u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x44u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x40u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x3Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x38u));
        c.SP = c.SP + 0x58u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80017284(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0x80050000u;
        c.S0 = c.S0 - 0x520Cu;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x80010000u;
        c.A1 = c.A1 + 0x7174u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.A2 = 0u + 0u;
        c.RA = 0x800172ACu;
        GranTurismo2ArcadePC.func_8006CCDC(c, m);
        c.A0 = c.S0 + 0u;
        c.RA = 0x800172B4u;
        GranTurismo2ArcadePC.func_8006CD80(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800172C4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = 0u + 0u;
        c.V0 = 0x800B0000u;
        c.A0 = 0x80050000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        c.A0 = c.A0 - 0x520Cu;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.A1 = c.A1 + 0x1A4u;
        c.RA = 0x800172F0u;
        GranTurismo2ArcadePC.func_8006CED4(c, m);
        c.S0 = c.V0 + 0u;
        c.V0 = 0xFFFFFFFDu;
        if (c.S0 == c.V0) {
            c.V0 = (int)c.S0 < -2 ? 1u : 0u;
            goto L80017328;
        }
        c.V0 = (int)c.S0 < -2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0xFFFFFFFCu;
            goto L80017318;
        }
        c.V0 = 0xFFFFFFFCu;
        if (c.S0 == c.V0) {
            c.V0 = c.S1 + 0u;
            goto L80017350;
        }
        c.V0 = c.S1 + 0u;
        goto L80017338;
        L80017318: ;
        if ((int)c.S0 >= 0) {
            c.V0 = c.S1 + 0u;
            goto L80017338;
        }
        c.V0 = c.S1 + 0u;
        goto L80017350;
        L80017328: ;
        c.A0 = 0x00000006u;
        c.RA = 0x80017330u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.V0 = c.S1 + 0u;
        goto L80017350;
        L80017338: ;
        c.A0 = 0x00000003u;
        c.RA = 0x80017340u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.V0 = 0x801D0000u;
        MemoryAccess.WriteU8(m, (c.V0 - 0x6CC0u), (byte)c.S0);
        c.S1 = 0x00000004u;
        c.V0 = c.S1 + 0u;
        L80017350: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80017364(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x520Cu;
        c.RA = 0x8001737Cu;
        GranTurismo2ArcadePC.func_8006D41C(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001738C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        c.V1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.V0 = c.V0 >> 3;
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u) {
            c.A0 = c.V1 + 0x8u;
            goto L800173BC;
        }
        c.A0 = c.V1 + 0x8u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.A0 = c.A0 + c.V0;
        L800173BC: ;
        c.V0 = c.A1 & 0x000Fu;
        c.V0 = c.V0 << 6;
        MemoryAccess.WriteU16(m, (c.SP + 0x10u), (ushort)c.V0);
        c.V0 = c.A1 & 0x0010u;
        c.V0 = c.V0 << 4;
        MemoryAccess.WriteU16(m, (c.SP + 0x12u), (ushort)c.V0);
        c.V0 = MemoryAccess.ReadU16(m, (c.A0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.SP + 0x14u), (ushort)c.V0);
        c.V0 = MemoryAccess.ReadU16(m, (c.A0 + 0xAu));
        c.A1 = c.SP + 0x10u;
        MemoryAccess.WriteU16(m, (c.SP + 0x16u), (ushort)c.V0);
        c.RA = 0x800173F0u;
        GranTurismo2ArcadePC.func_8007BAE4(c, m);
        c.RA = 0x800173F8u;
        GranTurismo2ArcadePC.func_8007AE40(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80017408(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        c.T0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.V1 = c.S1 << 2;
        c.V1 = c.V1 + c.S1;
        c.V1 = c.V1 << 2;
        c.V0 = 0x800B0000u;
        c.V0 = c.V0 + 0xF28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.V1 + c.V0;
        c.A0 = 0x801D0000u;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 - 0x4F28u;
        c.V1 = c.S1 << 1;
        c.V1 = c.V1 + c.V0;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0x10u));
        c.A2 = MemoryAccess.ReadU32(m, c.A1);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.V1);
        c.S2 = MemoryAccess.ReadU8(m, (c.A0 - 0x6CC0u));
        if ((int)c.V0 < 0) {
            c.S3 = 0u + 0u;
            goto L80017578;
        }
        c.S3 = 0u + 0u;
        c.V0 = c.T0 < 0x00000009u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80020000u;
            goto L80017578;
        }
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x2330u;
        c.V1 = c.T0 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x80017494u: goto L80017494;
            case 0x800174E4u: goto L800174E4;
            case 0x80017578u: goto L80017578;
            case 0x800174ECu: goto L800174EC;
            case 0x800174FCu: goto L800174FC;
            case 0x80017568u: goto L80017568;
            case 0x80017574u: goto L80017574;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L80017494: ;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x4F08u;
        c.RA = 0x800174A4u;
        GranTurismo2ArcadePC.func_800162EC(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4ED0u;
        c.A1 = c.S2 << 2;
        c.V1 = 0x80050000u;
        c.V1 = c.V1 - 0x4F18u;
        c.V0 = c.S1 << 1;
        c.V0 = c.V0 + c.V1;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, c.V0);
        c.A1 = c.A1 + c.A0;
        c.V0 = c.V1 << 1;
        c.V0 = c.V0 + c.V1;
        c.V1 = MemoryAccess.ReadU32(m, c.A1);
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.V1;
        MemoryAccess.WriteU32(m, (c.S0 + 0xCu), c.V0);
        goto L80017578;
        L800174E4: ;
        MemoryAccess.WriteU16(m, (c.S0 + 0x10u), (ushort)0u);
        goto L80017578;
        L800174EC: ;
        c.A0 = c.S0 + 0u;
        c.RA = 0x800174F4u;
        GranTurismo2ArcadePC.func_80016320(c, m);
        c.V0 = c.S3 + 0u;
        goto L8001757C;
        L800174FC: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A2 + 0x24u));
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.A2 + 0x14u));
        c.V0 = c.V0 << 7;
        if (c.V1 != 0u) { if ((int)c.V0 == int.MinValue && (int)c.V1 == -1) { c.LO = 0x80000000u; c.HI = 0u; } else { c.LO = (uint)((int)c.V0 / (int)c.V1); c.HI = (uint)((int)c.V0 % (int)c.V1); } }
        c.V0 = c.LO;
        { var _r = (long)(int)c.A3 * (int)c.V0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A2 + 0x6u));
        c.V0 = c.V0 ^ c.S1;
        c.A2 = c.V0 < 0x00000001u ? 1u : 0u;
        c.V1 = c.LO;
        if (c.A2 != 0u) {
            c.A3 = (uint)((int)c.V1 >> 7);
            goto L8001753C;
        }
        c.A3 = (uint)((int)c.V1 >> 7);
        c.A3 = (uint)((int)c.V1 >> 9);
        L8001753C: ;
        MemoryAccess.WriteU16(m, (c.S0 + 0x12u), (ushort)c.A3);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0xCu));
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0xEu));
        c.A0 = c.S0 + 0u;
        MemoryAccess.WriteU16(m, (c.A0 + 0x4u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.A0 + 0x6u), (ushort)c.V1);
        c.A1 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.RA = 0x80017560u;
        GranTurismo2ArcadePC.func_80016368(c, m);
        c.V0 = c.S3 + 0u;
        goto L8001757C;
        L80017568: ;
        c.V0 = 0x0000000Cu;
        MemoryAccess.WriteU16(m, (c.S0 + 0x10u), (ushort)c.V0);
        goto L80017578;
        L80017574: ;
        c.S3 = 0x00000001u;
        L80017578: ;
        c.V0 = c.S3 + 0u;
        L8001757C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80017598(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU16(m, (c.V0 - 0x4F2Cu), (ushort)0u);
        c.V0 = 0x80050000u;
        c.V1 = 0x00000010u;
        MemoryAccess.WriteU16(m, (c.V0 - 0x4F2Au), (ushort)c.V1);
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU16(m, (c.V0 + 0xF24u), (ushort)0u);
        c.V0 = 0x800B0000u;
        c.A0 = 0x80050000u;
        c.A1 = 0x80010000u;
        c.A0 = c.A0 - 0x4F04u;
        c.A1 = c.A1 + 0x7408u;
        c.A2 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU16(m, (c.V0 + 0xF26u), (ushort)c.V1);
        c.RA = 0x800175DCu;
        GranTurismo2ArcadePC.func_8006CCDC(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V0 + 0xF20u), 0u);
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800175F0(CpuContext c, IMemory m)
    {
        RecompOne.Runtime.Sdk.GT2Compat.BeginUnifiedArcadeFrontendFrame(m);
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        c.S4 = 0u + 0u;
        c.A2 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.A2 + 0xF24u));
        c.A1 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        if (c.V1 == c.A1) {
            c.S3 = c.V0 + 0x1A4u;
            goto L80017694;
        }
        c.S3 = c.V0 + 0x1A4u;
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L8001764C;
        }
        c.V0 = 0x00000002u;
        if (c.V1 == 0u) {
            c.V1 = 0x800B0000u;
            goto L8001765C;
        }
        c.V1 = 0x800B0000u;
        goto L80017708;
        L8001764C: ;
        if (c.V1 == c.V0) {
            c.V1 = 0x80050000u;
            goto L800176C4;
        }
        c.V1 = 0x80050000u;
        goto L80017708;
        L8001765C: ;
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 + 0xF26u));
        c.V0 = c.V0 - 0x1u;
        MemoryAccess.WriteU16(m, (c.V1 + 0xF26u), (ushort)c.V0);
        c.V0 = c.V0 << 16;
        if (c.V0 != 0u) {
            c.V1 = 0x80050000u;
            goto L80017698;
        }
        c.V1 = 0x80050000u;
        c.S0 = 0x80050000u;
        c.S0 = c.S0 - 0x4F04u;
        c.A0 = c.S0 + 0u;
        MemoryAccess.WriteU16(m, (c.A2 + 0xF24u), (ushort)c.A1);
        c.RA = 0x8001768Cu;
        GranTurismo2ArcadePC.func_8006CD80(c, m);
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU16(m, (c.S0 + 0x1Cu), (ushort)c.V0);
        L80017694: ;
        c.V1 = 0x80050000u;
        L80017698: ;
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 - 0x4F2Cu));
        c.V0 = c.V0 + 0x1u;
        MemoryAccess.WriteU16(m, (c.V1 - 0x4F2Cu), (ushort)c.V0);
        c.V0 = c.V0 << 16;
        c.V0 = (uint)((int)c.V0 >> 16);
        c.V0 = (int)c.V0 < 13 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = 0x0000000Cu;
            goto L80017708;
        }
        c.V0 = 0x0000000Cu;
        MemoryAccess.WriteU16(m, (c.V1 - 0x4F2Cu), (ushort)c.V0);
        goto L80017708;
        L800176C4: ;
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 - 0x4F2Cu));
        c.V0 = c.V0 - 0x1u;
        MemoryAccess.WriteU16(m, (c.V1 - 0x4F2Cu), (ushort)c.V0);
        c.V0 = c.V0 << 16;
        if ((int)c.V0 >= 0) {
            goto L800176E4;
        }
        MemoryAccess.WriteU16(m, (c.V1 - 0x4F2Cu), (ushort)0u);
        L800176E4: ;
        c.V1 = 0x80050000u;
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 - 0x4F2Au));
        c.V0 = c.V0 - 0x1u;
        MemoryAccess.WriteU16(m, (c.V1 - 0x4F2Au), (ushort)c.V0);
        c.V0 = c.V0 << 16;
        if ((int)c.V0 >= 0) {
            goto L80017708;
        }
        c.S4 = 0x00000004u;
        L80017708: ;
        c.V0 = MemoryAccess.ReadU32(m, c.S3);
        if (c.V0 == 0u) {
            c.V0 = 0x800B0000u;
            goto L80017720;
        }
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V0 + 0xF20u), 0u);
        goto L80017760;
        L80017720: ;
        c.V1 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0xF20u));
        c.V0 = c.V0 + 0x1u;
        MemoryAccess.WriteU32(m, (c.V1 + 0xF20u), c.V0);
        c.V0 = (int)c.V0 < 901 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.A0 = 0x80050000u;
            goto L80017760;
        }
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4F04u;
        c.RA = 0x80017748u;
        GranTurismo2ArcadePC.func_8006CDE8(c, m);
        c.V1 = 0x800B0000u;
        c.V0 = 0x00000002u;
        MemoryAccess.WriteU16(m, (c.V1 + 0xF24u), (ushort)c.V0);
        c.V1 = 0x801F0000u;
        c.V0 = 0x00000006u;
        MemoryAccess.WriteU8(m, (c.V1 - 0xFDDu), (byte)c.V0);
        L80017760: ;
        if (c.S1 != 0u) {
            c.V0 = 0x80050000u;
            goto L8001776C;
        }
        c.V0 = 0x80050000u;
        c.S3 = 0u + 0u;
        L8001776C: ;
        c.S1 = c.V0 - 0x4F04u;
        c.A0 = c.S1 + 0u;
        c.S2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x6u));
        c.A1 = c.S3 + 0u;
        c.RA = 0x80017780u;
        GranTurismo2ArcadePC.func_8006CED4(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = 0x00000001u;
        c.S0 = c.V0 + 0u;
        c.A2 = 0x00000006u;
        c.RA = 0x80017794u;
        GranTurismo2ArcadePC.func_8006D3C0(c, m);
        c.V0 = 0xFFFFFFFDu;
        if (c.S0 == c.V0) {
            c.V0 = (int)c.S0 < -2 ? 1u : 0u;
            goto L800177C8;
        }
        c.V0 = (int)c.S0 < -2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0xFFFFFFFCu;
            goto L800177B8;
        }
        c.V0 = 0xFFFFFFFCu;
        if (c.S0 == c.V0) {
            c.V0 = c.S4 + 0u;
            goto L8001784C;
        }
        c.V0 = c.S4 + 0u;
        goto L800177E8;
        L800177B8: ;
        if ((int)c.S0 >= 0) {
            c.V0 = c.S4 + 0u;
            goto L800177E8;
        }
        c.V0 = c.S4 + 0u;
        goto L8001784C;
        L800177C8: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x6u));
        if (c.S2 == c.V0) {
            c.V0 = c.S4 + 0u;
            goto L8001784C;
        }
        c.V0 = c.S4 + 0u;
        c.A0 = 0x00000006u;
        c.RA = 0x800177E0u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.V0 = c.S4 + 0u;
        goto L8001784C;
        L800177E8: ;
        c.A0 = 0x00000003u;
        c.RA = 0x800177F0u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4F04u;
        c.RA = 0x800177FCu;
        GranTurismo2ArcadePC.func_8006CDE8(c, m);
        c.V1 = 0x800B0000u;
        c.V0 = 0x00000002u;
        MemoryAccess.WriteU16(m, (c.V1 + 0xF24u), (ushort)c.V0);
        c.V0 = 0x801F0000u;
        c.A0 = c.V0 - 0xFE0u;
        MemoryAccess.WriteU8(m, (c.A0 + 0x11u), (byte)0u);
        c.V0 = MemoryAccess.ReadU32(m, c.S3);
        c.V1 = 0x00010000u;
        c.V0 = c.V0 & c.V1;
        if (c.V0 == 0u) {
            c.V0 = 0x00000001u;
            goto L8001782C;
        }
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.A0 + 0x11u), (byte)c.V0);
        L8001782C: ;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 - 0x4F28u;
        c.V1 = c.S0 << 1;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU8(m, c.V1);
        MemoryAccess.WriteU8(m, (c.A0 + 0x3u), (byte)c.V0);
        c.V0 = c.S4 + 0u;
        L8001784C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        RecompOne.Runtime.Sdk.GT2Compat.CompleteUnifiedArcadeFrontendFrame();
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001786C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S3 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.A0 = c.V0 - 0x4F04u;
        c.A1 = c.S3 + 0u;
        c.RA = 0x8001789Cu;
        GranTurismo2ArcadePC.func_8006D41C(c, m);
        c.A0 = 0x2AAA0000u;
        c.V0 = 0x80050000u;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 - 0x4F2Cu));
        c.A0 = c.A0 | 0xAAABu;
        c.V1 = c.V1 << 7;
        { var _r = (long)(int)c.V1 * (int)c.A0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.A0 = c.S3 + 0u;
        c.V1 = (uint)((int)c.V1 >> 31);
        c.A2 = c.HI;
        c.V0 = (uint)((int)c.A2 >> 1);
        c.V0 = c.V0 - c.V1;
        c.S0 = c.V0 << 8;
        c.S0 = c.V0 | c.S0;
        c.V0 = c.V0 << 16;
        c.S0 = c.S0 | c.V0;
        c.A1 = c.S0 + 0u;
        c.RA = 0x800178E0u;
        GranTurismo2ArcadePC.func_80080360(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = c.S3 + 0u;
        c.A1 = c.S0 + 0u;
        c.V0 = 0x00000086u;
        c.S1 = 0x00000100u;
        c.S2 = 0x00007FD8u;
        MemoryAccess.WriteU16(m, c.V1, (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x8u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.V1 + 0xAu), (ushort)0u);
        MemoryAccess.WriteU8(m, (c.V1 + 0xCu), (byte)0u);
        MemoryAccess.WriteU8(m, (c.V1 + 0xDu), (byte)0u);
        MemoryAccess.WriteU16(m, (c.V1 + 0x10u), (ushort)c.S1);
        MemoryAccess.WriteU16(m, (c.V1 + 0x12u), (ushort)c.S1);
        MemoryAccess.WriteU16(m, (c.V1 + 0xEu), (ushort)c.S2);
        c.RA = 0x8001791Cu;
        GranTurismo2ArcadePC.func_80080360(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = c.S3 + 0u;
        c.A1 = c.S0 + 0u;
        c.V0 = 0x00000096u;
        c.S4 = 0x000000E0u;
        MemoryAccess.WriteU16(m, c.V1, (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x8u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.V1 + 0xAu), (ushort)c.S1);
        MemoryAccess.WriteU8(m, (c.V1 + 0xCu), (byte)0u);
        MemoryAccess.WriteU8(m, (c.V1 + 0xDu), (byte)0u);
        MemoryAccess.WriteU16(m, (c.V1 + 0x10u), (ushort)c.S1);
        MemoryAccess.WriteU16(m, (c.V1 + 0x12u), (ushort)c.S4);
        MemoryAccess.WriteU16(m, (c.V1 + 0xEu), (ushort)c.S2);
        c.RA = 0x80017954u;
        GranTurismo2ArcadePC.func_80080360(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = c.S3 + 0u;
        c.A1 = c.S0 + 0u;
        c.V0 = 0x00000088u;
        c.S0 = 0x00000060u;
        MemoryAccess.WriteU16(m, c.V1, (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x8u), (ushort)c.S1);
        MemoryAccess.WriteU16(m, (c.V1 + 0xAu), (ushort)0u);
        MemoryAccess.WriteU8(m, (c.V1 + 0xCu), (byte)0u);
        MemoryAccess.WriteU8(m, (c.V1 + 0xDu), (byte)0u);
        MemoryAccess.WriteU16(m, (c.V1 + 0x10u), (ushort)c.S0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x12u), (ushort)c.S1);
        MemoryAccess.WriteU16(m, (c.V1 + 0xEu), (ushort)c.S2);
        c.RA = 0x8001798Cu;
        GranTurismo2ArcadePC.func_80080360(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = c.S3 + 0u;
        c.A1 = 0x00000280u;
        c.V0 = 0x00000098u;
        MemoryAccess.WriteU16(m, c.V1, (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x8u), (ushort)c.S1);
        MemoryAccess.WriteU16(m, (c.V1 + 0xAu), (ushort)c.S1);
        MemoryAccess.WriteU8(m, (c.V1 + 0xCu), (byte)0u);
        MemoryAccess.WriteU8(m, (c.V1 + 0xDu), (byte)0u);
        MemoryAccess.WriteU16(m, (c.V1 + 0x10u), (ushort)c.S0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x12u), (ushort)c.S4);
        MemoryAccess.WriteU16(m, (c.V1 + 0xEu), (ushort)c.S2);
        c.RA = 0x800179C0u;
        GranTurismo2ArcadePC.func_8007D954(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800179E0(CpuContext c, IMemory m)
    {
        c.V0 = 0x801D0000u;
        c.V1 = (uint)(sbyte)MemoryAccess.ReadU8(m, c.A0);
        c.A0 = c.V0 - 0x6CC0u;
        c.V0 = c.V1 < 0x0000000Fu ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A1 = 0u + 0u;
            goto L80017ACC;
        }
        c.A1 = 0u + 0u;
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x2358u;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x80017A18u: goto L80017A18;
            case 0x80017A24u: goto L80017A24;
            case 0x80017A30u: goto L80017A30;
            case 0x80017A3Cu: goto L80017A3C;
            case 0x80017A48u: goto L80017A48;
            case 0x80017A54u: goto L80017A54;
            case 0x80017A60u: goto L80017A60;
            case 0x80017A6Cu: goto L80017A6C;
            case 0x80017A78u: goto L80017A78;
            case 0x80017A84u: goto L80017A84;
            case 0x80017A90u: goto L80017A90;
            case 0x80017A9Cu: goto L80017A9C;
            case 0x80017AA8u: goto L80017AA8;
            case 0x80017AB4u: goto L80017AB4;
            case 0x80017AC0u: goto L80017AC0;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L80017A18: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.A0 + 0x2u));
        c.V0 = c.A1 + 0u;
        return;
        L80017A24: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.A0 + 0x3u));
        c.V0 = c.A1 + 0u;
        return;
        L80017A30: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.A0 + 0x4u));
        c.V0 = c.A1 + 0u;
        return;
        L80017A3C: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.A0 + 0x5u));
        c.V0 = c.A1 + 0u;
        return;
        L80017A48: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.A0 + 0x6u));
        c.V0 = c.A1 + 0u;
        return;
        L80017A54: ;
        c.A1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.A0 + 0x7u));
        c.V0 = c.A1 + 0u;
        return;
        L80017A60: ;
        c.A1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.A0 + 0x8u));
        c.V0 = c.A1 + 0u;
        return;
        L80017A6C: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.A0 + 0xAEu));
        c.V0 = c.A1 + 0u;
        return;
        L80017A78: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.A0 + 0xAFu));
        c.V0 = c.A1 + 0u;
        return;
        L80017A84: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.A0 + 0xB0u));
        c.V0 = c.A1 + 0u;
        return;
        L80017A90: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.A0 + 0xB1u));
        c.V0 = c.A1 + 0u;
        return;
        L80017A9C: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.A0 + 0xB2u));
        c.V0 = c.A1 + 0u;
        return;
        L80017AA8: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.A0 + 0xB3u));
        c.V0 = c.A1 + 0u;
        return;
        L80017AB4: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.A0 + 0xB4u));
        c.V0 = c.A1 + 0u;
        return;
        L80017AC0: ;
        c.V0 = MemoryAccess.ReadU8(m, (c.A0 + 0x36u));
        c.A1 = c.V0 < 0x00000001u ? 1u : 0u;
        L80017ACC: ;
        c.V0 = c.A1 + 0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80017AD4(CpuContext c, IMemory m)
    {
        c.V0 = 0x801D0000u;
        c.V1 = (uint)(sbyte)MemoryAccess.ReadU8(m, c.A0);
        c.A0 = c.V0 - 0x6CC0u;
        c.V0 = c.V1 < 0x0000000Fu ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80020000u;
            goto L80017B90;
        }
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x2398u;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x80017B08u: goto L80017B08;
            case 0x80017B14u: goto L80017B14;
            case 0x80017B1Cu: goto L80017B1C;
            case 0x80017B24u: goto L80017B24;
            case 0x80017B30u: goto L80017B30;
            case 0x80017B38u: goto L80017B38;
            case 0x80017B40u: goto L80017B40;
            case 0x80017B48u: goto L80017B48;
            case 0x80017B50u: goto L80017B50;
            case 0x80017B58u: goto L80017B58;
            case 0x80017B60u: goto L80017B60;
            case 0x80017B68u: goto L80017B68;
            case 0x80017B70u: goto L80017B70;
            case 0x80017B78u: goto L80017B78;
            case 0x80017B80u: goto L80017B80;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L80017B08: ;
        c.V0 = 0u < c.A1 ? 1u : 0u;
        MemoryAccess.WriteU8(m, (c.A0 + 0x2u), (byte)c.V0);
        return;
        L80017B14: ;
        MemoryAccess.WriteU8(m, (c.A0 + 0x3u), (byte)c.A1);
        return;
        L80017B1C: ;
        MemoryAccess.WriteU8(m, (c.A0 + 0x4u), (byte)c.A1);
        return;
        L80017B24: ;
        c.V0 = 0u < c.A1 ? 1u : 0u;
        MemoryAccess.WriteU8(m, (c.A0 + 0x5u), (byte)c.V0);
        return;
        L80017B30: ;
        MemoryAccess.WriteU8(m, (c.A0 + 0x6u), (byte)c.A1);
        return;
        L80017B38: ;
        MemoryAccess.WriteU8(m, (c.A0 + 0x7u), (byte)c.A1);
        return;
        L80017B40: ;
        MemoryAccess.WriteU8(m, (c.A0 + 0x8u), (byte)c.A1);
        return;
        L80017B48: ;
        MemoryAccess.WriteU8(m, (c.A0 + 0xAEu), (byte)c.A1);
        return;
        L80017B50: ;
        MemoryAccess.WriteU8(m, (c.A0 + 0xAFu), (byte)c.A1);
        return;
        L80017B58: ;
        MemoryAccess.WriteU8(m, (c.A0 + 0xB0u), (byte)c.A1);
        return;
        L80017B60: ;
        MemoryAccess.WriteU8(m, (c.A0 + 0xB1u), (byte)c.A1);
        return;
        L80017B68: ;
        MemoryAccess.WriteU8(m, (c.A0 + 0xB2u), (byte)c.A1);
        return;
        L80017B70: ;
        MemoryAccess.WriteU8(m, (c.A0 + 0xB3u), (byte)c.A1);
        return;
        L80017B78: ;
        MemoryAccess.WriteU8(m, (c.A0 + 0xB4u), (byte)c.A1);
        return;
        L80017B80: ;
        c.V0 = c.A1 ^ 0x0001u;
        c.V0 = 0u < c.V0 ? 1u : 0u;
        MemoryAccess.WriteU8(m, (c.A0 + 0x36u), (byte)c.V0);
        MemoryAccess.WriteU8(m, (c.A0 + 0x88u), (byte)c.V0);
        L80017B90: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80017B98(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.V1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S2 + 0x1u));
        c.V0 = 0x00000002u;
        if (c.V1 == c.V0) {
            c.S4 = c.A2 + 0u;
            goto L80017BFC;
        }
        c.S4 = c.A2 + 0u;
        c.V0 = (int)c.V1 < 3 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000001u;
            goto L80017BE8;
        }
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            goto L80017C1C;
        }
        goto L80017C58;
        L80017BE8: ;
        c.V0 = 0x00000003u;
        if (c.V1 == c.V0) {
            goto L80017C54;
        }
        goto L80017C58;
        L80017BFC: ;
        c.A0 = c.S2 + 0u;
        c.RA = 0x80017C04u;
        GranTurismo2ArcadePC.func_800179E0(c, m);
        c.S0 = c.V0 + 0u;
        c.S3 = c.S0 + 0u;
        c.V0 = c.S1 + c.S4;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x2u));
        c.S0 = c.S0 + c.V0;
        goto L80017C70;
        L80017C1C: ;
        c.A0 = c.S2 + 0u;
        c.RA = 0x80017C24u;
        GranTurismo2ArcadePC.func_800179E0(c, m);
        c.S1 = c.S1 << 4;
        c.A0 = c.S2 + 0u;
        c.S3 = c.V0 + 0u;
        c.RA = 0x80017C34u;
        GranTurismo2ArcadePC.func_800179E0(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = c.V1 + 0xFu;
        if ((int)c.V0 >= 0) {
            c.V0 = (uint)((int)c.V0 >> 4);
            goto L80017C4C;
        }
        c.V0 = (uint)((int)c.V0 >> 4);
        c.V0 = c.V1 + 0x1Eu;
        c.V0 = (uint)((int)c.V0 >> 4);
        L80017C4C: ;
        c.S0 = c.V0 << 4;
        goto L80017C68;
        L80017C54: ;
        c.S1 = c.S1 + c.S4;
        L80017C58: ;
        c.A0 = c.S2 + 0u;
        c.RA = 0x80017C60u;
        GranTurismo2ArcadePC.func_800179E0(c, m);
        c.S0 = c.V0 + 0u;
        c.S3 = c.S0 + 0u;
        L80017C68: ;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x2u));
        c.S0 = c.S0 + c.S1;
        L80017C70: ;
        c.V0 = (int)c.S0 < (int)c.V1 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L80017C80;
        }
        c.S0 = c.V1 + 0u;
        L80017C80: ;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x4u));
        c.V0 = (int)c.S0 < (int)c.V1 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.A0 = c.S2 + 0u;
            goto L80017C98;
        }
        c.A0 = c.S2 + 0u;
        c.S0 = c.V1 - 0x1u;
        L80017C98: ;
        c.A1 = c.S0 + 0u;
        c.RA = 0x80017CA0u;
        GranTurismo2ArcadePC.func_80017AD4(c, m);
        c.V0 = c.S3 ^ c.S0;
        c.V0 = 0u < c.V0 ? 1u : 0u;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80017CC8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0xF8u;
        MemoryAccess.WriteU32(m, (c.SP + 0xECu), c.S7);
        c.S7 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0xE8u), c.S6);
        c.S6 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0xF0u), c.FP);
        c.FP = MemoryAccess.ReadU32(m, (c.SP + 0x108u));
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU32(m, (c.SP + 0xD0u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10Cu));
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x4E5Cu));
        MemoryAccess.WriteU32(m, (c.SP + 0xF4u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0xE4u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0xE0u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0xDCu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0xD8u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0xD4u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0xFCu), c.A1);
        MemoryAccess.WriteU32(m, (c.SP + 0x104u), c.A3);
        MemoryAccess.WriteU32(m, (c.SP + 0xC0u), c.V0);
        c.RA = 0x80017D1Cu;
        GranTurismo2ArcadePC.func_800179E0(c, m);
        if (c.S0 == 0u) {
            c.S4 = c.V0 + 0u;
            goto L80017D34;
        }
        c.S4 = c.V0 + 0u;
        c.V0 = 0x80050000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x4E58u));
        MemoryAccess.WriteU32(m, (c.SP + 0xC0u), c.V0);
        L80017D34: ;
        c.A0 = c.SP + 0x20u;
        c.A1 = 0x0000001Eu;
        c.RA = 0x80017D40u;
        GranTurismo2ArcadePC.func_8006AB78(c, m);
        c.V1 = 0xFF9F0000u;
        c.V1 = c.V1 | 0xFFFFu;
        c.S1 = c.SP + 0x20u;
        c.A0 = c.S1 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6D40u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0xCu));
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xFCu));
        c.V0 = c.V0 & c.V1;
        c.V1 = 0x00200000u;
        c.V0 = c.V0 | c.V1;
        MemoryAccess.WriteU32(m, (c.S1 + 0x10u), c.T0);
        MemoryAccess.WriteU32(m, (c.S1 + 0xCu), c.V0);
        c.RA = 0x80017D78u;
        GranTurismo2ArcadePC.func_8007D990(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4E60u;
        c.S5 = c.SP + 0xC0u;
        c.A1 = c.S5 + 0u;
        c.A2 = c.FP + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x80017D94u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S1 + 0u;
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0x104u));
        c.S3 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x14u), c.V0);
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S7 + 0x6u));
        c.T0 = 0xFFFFFFFEu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.T0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.A1 = MemoryAccess.ReadU32(m, (c.S7 + 0xCu));
        c.A2 = c.S6 + c.A2;
        c.RA = 0x80017DC4u;
        GranTurismo2ArcadePC.func_8006AE50(c, m);
        c.S2 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S7 + 0x1u));
        if (c.S2 == c.S3) {
            c.V0 = (int)c.S2 < 2 ? 1u : 0u;
            goto L80017EA8;
        }
        c.V0 = (int)c.S2 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L80017DEC;
        }
        if (c.S2 == 0u) {
            c.S0 = 0u + 0u;
            goto L80017E08;
        }
        c.S0 = 0u + 0u;
        goto L800181B0;
        L80017DEC: ;
        c.V0 = 0x00000002u;
        if (c.S2 == c.V0) {
            c.V0 = 0x00000003u;
            goto L80017F3C;
        }
        c.V0 = 0x00000003u;
        if (c.S2 == c.V0) {
            c.S0 = c.SP + 0x80u;
            goto L80018134;
        }
        c.S0 = c.SP + 0x80u;
        goto L800181B0;
        L80017E08: ;
        c.S2 = c.S1 + 0u;
        c.S5 = 0x00000001u;
        c.S3 = 0xFFFFFFFEu;
        L80017E14: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S7 + 0x4u));
        c.V0 = (int)c.S0 < (int)c.V0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.T0 = 0x80050000u;
            goto L800181B0;
        }
        c.T0 = 0x80050000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.T0 - 0x4E54u));
        MemoryAccess.WriteU32(m, (c.SP + 0xC4u), c.V0);
        c.V1 = MemoryAccess.ReadU32(m, (c.S7 + 0x10u));
        c.V0 = c.S0 << 2;
        c.V0 = c.V0 + c.V1;
        c.S1 = MemoryAccess.ReadU32(m, c.V0);
        if (c.S0 != c.S4) {
            c.T0 = 0x80050000u;
            goto L80017E60;
        }
        c.T0 = 0x80050000u;
        c.T0 = 0x80050000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.T0 - 0x4E50u));
        MemoryAccess.WriteU32(m, (c.SP + 0xC4u), c.V0);
        c.T0 = 0x80050000u;
        L80017E60: ;
        c.A0 = c.T0 - 0x4E60u;
        c.A1 = c.SP + 0xC4u;
        c.A2 = c.FP + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x80017E74u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S2 + 0u;
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0x104u));
        c.A1 = c.S1 + 0u;
        MemoryAccess.WriteU32(m, (c.S2 + 0x14u), c.V0);
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S7 + 0x8u));
        c.S0 = c.S0 + 0x1u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.A2 = c.S6 + c.A2;
        c.RA = 0x80017EA0u;
        GranTurismo2ArcadePC.func_8006AE50(c, m);
        c.S6 = c.S6 + 0x3Cu;
        goto L80017E14;
        L80017EA8: ;
        c.S0 = 0u + 0u;
        c.S5 = 0x80050000u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x104u));
        c.S2 = 0x00000005u;
        c.S1 = 0x00000014u;
        c.S3 = c.T0 - 0x14u;
        L80017EC0: ;
        c.T0 = 0x80050000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.T0 - 0x4E44u));
        MemoryAccess.WriteU32(m, (c.SP + 0xC0u), c.V0);
        c.V0 = c.S0 << 4;
        c.V0 = (int)c.V0 < (int)c.S4 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.A0 = c.S5 - 0x4E60u;
            goto L80017EF0;
        }
        c.A0 = c.S5 - 0x4E60u;
        c.T0 = 0x80050000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.T0 - 0x4E48u));
        MemoryAccess.WriteU32(m, (c.SP + 0xC0u), c.V0);
        L80017EF0: ;
        c.A1 = c.SP + 0xC0u;
        c.A2 = c.FP + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x80017F00u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0xFCu));
        c.A1 = c.V0 + 0u;
        c.RA = 0x80017F0Cu;
        GranTurismo2ArcadePC.func_8007CF34(c, m);
        c.V1 = MemoryAccess.ReadU16(m, (c.S7 + 0x8u));
        c.S0 = c.S0 + 0x1u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x2u), (ushort)c.S3);
        MemoryAccess.WriteU16(m, (c.V0 + 0x4u), (ushort)c.S2);
        MemoryAccess.WriteU16(m, (c.V0 + 0x6u), (ushort)c.S1);
        c.V1 = c.V1 + c.S6;
        MemoryAccess.WriteU16(m, c.V0, (ushort)c.V1);
        c.V0 = (int)c.S0 < 16 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S6 = c.S6 + 0x6u;
            goto L80017EC0;
        }
        c.S6 = c.S6 + 0x6u;
        goto L800181B0;
        L80017F3C: ;
        c.V0 = 0x02780000u;
        c.V0 = c.V0 | 0x5030u;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4E60u;
        c.A1 = c.S5 + 0u;
        c.A2 = c.FP + 0u;
        c.A3 = 0x00000080u;
        MemoryAccess.WriteU32(m, (c.SP + 0xC0u), c.V0);
        c.RA = 0x80017F60u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0xFCu));
        c.A1 = c.V0 + 0u;
        c.RA = 0x80017F6Cu;
        GranTurismo2ArcadePC.func_8007CF34(c, m);
        c.A0 = c.V0 + 0u;
        c.V1 = MemoryAccess.ReadU16(m, (c.S7 + 0x8u));
        c.V0 = 0x0000008Cu;
        MemoryAccess.WriteU16(m, (c.A0 + 0x4u), (ushort)c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x104u));
        c.V0 = c.T0 - 0xAu;
        MemoryAccess.WriteU16(m, (c.A0 + 0x2u), (ushort)c.V0);
        c.V0 = 0x00000004u;
        c.V1 = c.V1 + c.S6;
        MemoryAccess.WriteU16(m, (c.A0 + 0x6u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, c.A0, (ushort)c.V1);
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S7 + 0x2u));
        c.V0 = c.S4 - c.A0;
        c.V1 = c.V0 << 3;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 - c.V0;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S7 + 0x4u));
        c.V1 = c.V1 << 2;
        c.V0 = c.V0 - c.A0;
        if (c.V0 != 0u) { if ((int)c.V1 == int.MinValue && (int)c.V0 == -1) { c.LO = 0x80000000u; c.HI = 0u; } else { c.LO = (uint)((int)c.V1 / (int)c.V0); c.HI = (uint)((int)c.V1 % (int)c.V0); } }
        c.V1 = c.LO;
        c.A1 = c.S5 + 0u;
        c.A2 = c.FP + 0u;
        c.A3 = 0x00000080u;
        MemoryAccess.WriteU32(m, (c.SP + 0xC8u), 0u);
        c.S0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S7 + 0x8u));
        c.V0 = 0x02900000u;
        c.V0 = c.V0 | 0x9090u;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4E60u;
        c.S0 = c.S6 + c.S0;
        MemoryAccess.WriteU32(m, (c.SP + 0xC0u), c.V0);
        c.S0 = c.S0 + c.V1;
        c.RA = 0x80018000u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0xFCu));
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001800Cu;
        GranTurismo2ArcadePC.func_8007CF34(c, m);
        c.A0 = c.V0 + 0u;
        c.S0 = c.S0 - 0x1u;
        MemoryAccess.WriteU16(m, c.A0, (ushort)c.S0);
        MemoryAccess.WriteU16(m, (c.A0 + 0x4u), (ushort)c.S2);
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x104u));
        c.FP = 0u + 0u;
        c.V0 = c.T0 - 0xDu;
        MemoryAccess.WriteU16(m, (c.A0 + 0x2u), (ushort)c.V0);
        c.V0 = 0x0000000Au;
        if ((int)c.S4 >= 0) {
            MemoryAccess.WriteU16(m, (c.A0 + 0x6u), (ushort)c.V0);
            goto L80018050;
        }
        MemoryAccess.WriteU16(m, (c.A0 + 0x6u), (ushort)c.V0);
        c.V1 = 0u - c.S4;
        c.V0 = c.V1 << 2;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 1;
        MemoryAccess.WriteU32(m, (c.SP + 0xC8u), c.V0);
        goto L8001805C;
        L80018050: ;
        c.V0 = c.S4 << 2;
        c.V0 = c.V0 + c.S4;
        c.FP = c.V0 << 1;
        L8001805C: ;
        c.A0 = c.SP + 0x20u;
        c.S0 = 0x801C0000u;
        c.S0 = c.S0 - 0x6639u;
        c.A1 = c.S0 + 0u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x104u));
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S7 + 0x8u));
        c.S2 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S2);
        c.S3 = c.T0 + 0x12u;
        c.A3 = c.S3 + 0u;
        c.A2 = c.S6 + c.A2;
        c.A2 = c.A2 + 0x10u;
        c.RA = 0x80018090u;
        GranTurismo2ArcadePC.func_8006AD38(c, m);
        c.S1 = c.SP + 0x40u;
        c.A0 = c.S1 + 0u;
        c.S5 = c.S0 + 0xAu;
        c.A1 = c.S5 + 0u;
        c.A2 = c.FP + 0u;
        c.RA = 0x800180A8u;
        GranTurismo2ArcadePC.func_8008CE44(c, m);
        c.A0 = c.SP + 0x20u;
        c.A1 = c.S1 + 0u;
        c.A3 = c.S3 + 0u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S7 + 0x8u));
        c.S4 = 0xFFFFFFFEu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.A2 = c.S6 + c.A2;
        c.A2 = c.A2 + 0x38u;
        c.RA = 0x800180D4u;
        GranTurismo2ArcadePC.func_8006B094(c, m);
        c.A0 = c.SP + 0x20u;
        c.A1 = c.S0 + 0x5u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S7 + 0x8u));
        c.A3 = c.S3 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S2);
        c.A2 = c.S6 + c.A2;
        c.A2 = c.A2 + 0x60u;
        c.RA = 0x800180F4u;
        GranTurismo2ArcadePC.func_8006AD38(c, m);
        c.A0 = c.S1 + 0u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0xC8u));
        c.A1 = c.S5 + 0u;
        c.RA = 0x80018104u;
        GranTurismo2ArcadePC.func_8008CE44(c, m);
        c.A0 = c.SP + 0x20u;
        c.A1 = c.S1 + 0u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S7 + 0x8u));
        c.A3 = c.S3 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.A2 = c.S6 + c.A2;
        c.A2 = c.A2 + 0x88u;
        c.RA = 0x8001812Cu;
        GranTurismo2ArcadePC.func_8006B094(c, m);
        goto L800181B0;
        L80018134: ;
        c.A0 = c.S0 + 0u;
        c.V0 = 0x801C0000u;
        c.S2 = c.V0 - 0x67DAu;
        c.A1 = c.S2 + 0u;
        c.RA = 0x80018148u;
        GranTurismo2ArcadePC.func_8008CDEC(c, m);
        c.V0 = (int)c.S4 < 2 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.A0 = c.S0 + 0u;
            goto L80018160;
        }
        c.A0 = c.S0 + 0u;
        c.A1 = c.S2 + 0x9u;
        c.A2 = c.S4 + 0u;
        c.RA = 0x80018160u;
        GranTurismo2ArcadePC.func_8008CE44(c, m);
        L80018160: ;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4E60u;
        c.A1 = c.S5 + 0u;
        c.A2 = c.FP + 0u;
        c.V0 = 0x80050000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x4E44u));
        c.A3 = 0x00000080u;
        MemoryAccess.WriteU32(m, (c.SP + 0xC0u), c.V0);
        c.RA = 0x80018184u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S0 + 0u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.V0);
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S7 + 0x8u));
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0x104u));
        c.T0 = 0xFFFFFFFEu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.T0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.A2 = c.S6 + c.A2;
        c.RA = 0x800181B0u;
        GranTurismo2ArcadePC.func_8006AE50(c, m);
        L800181B0: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0xF4u));
        c.FP = MemoryAccess.ReadU32(m, (c.SP + 0xF0u));
        c.S7 = MemoryAccess.ReadU32(m, (c.SP + 0xECu));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0xE8u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0xE4u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0xE0u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0xDCu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0xD8u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0xD4u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0xD0u));
        c.SP = c.SP + 0xF8u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800181E0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        c.T0 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S0);
        c.A2 = MemoryAccess.ReadU32(m, c.A1);
        c.T1 = c.A0 + 0u;
        c.A0 = MemoryAccess.ReadU32(m, (c.A2 + 0x30u));
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0x10u));
        c.V1 = MemoryAccess.ReadU32(m, c.A0);
        { var _r = (long)(int)c.V0 * (int)c.V1; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0xEu));
        c.T2 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.T3 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0xCu));
        c.A1 = MemoryAccess.ReadU32(m, (c.A1 + 0x8u));
        c.V0 = c.T0 << 2;
        c.V0 = c.V0 + c.T0;
        c.V1 = MemoryAccess.ReadU32(m, (c.A2 + 0x2Cu));
        c.V0 = c.V0 << 2;
        c.T6 = c.V1 + c.V0;
        c.V0 = 0x00000004u;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.A2 + 0x6u));
        c.T4 = c.LO;
        if (c.T1 == c.V0) {
            c.T5 = (uint)((int)c.T4 >> 7);
            goto L800182F4;
        }
        c.T5 = (uint)((int)c.T4 >> 7);
        c.V0 = (int)c.T1 < 5 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000003u;
            goto L8001833C;
        }
        c.V0 = 0x00000003u;
        if (c.T1 != c.V0) {
            goto L8001833C;
        }
        if (c.T0 != c.V1) {
            goto L8001833C;
        }
        if (c.A1 == 0u) {
            c.S0 = 0u + 0u;
            goto L8001833C;
        }
        c.S0 = 0u + 0u;
        c.V1 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.V0 = MemoryAccess.ReadU32(m, (c.A1 + 0xCu));
        c.V1 = c.V1 | c.V0;
        c.V0 = c.V1 & 0x0004u;
        if (c.V0 == 0u) {
            c.A2 = c.S0 + 0u;
            goto L80018288;
        }
        c.A2 = c.S0 + 0u;
        c.S0 = 0xFFFFFFFFu;
        L80018288: ;
        c.V0 = c.V1 & 0x0008u;
        if (c.V0 == 0u) {
            goto L80018298;
        }
        c.S0 = 0x00000001u;
        L80018298: ;
        c.V1 = MemoryAccess.ReadU32(m, c.A1);
        c.V0 = c.V1 & 0x0010u;
        if (c.V0 == 0u) {
            c.V0 = c.V1 & 0x1000u;
            goto L800182B0;
        }
        c.V0 = c.V1 & 0x1000u;
        c.A2 = 0xFFFFFFFFu;
        L800182B0: ;
        if (c.V0 == 0u) {
            c.V0 = 0u < c.S0 ? 1u : 0u;
            goto L800182BC;
        }
        c.V0 = 0u < c.S0 ? 1u : 0u;
        c.A2 = 0x00000001u;
        L800182BC: ;
        c.V1 = 0u < c.A2 ? 1u : 0u;
        c.V0 = c.V0 | c.V1;
        if (c.V0 == 0u) {
            c.A0 = c.T6 + 0u;
            goto L8001833C;
        }
        c.A0 = c.T6 + 0u;
        c.A1 = c.S0 + 0u;
        c.RA = 0x800182D4u;
        GranTurismo2ArcadePC.func_80017B98(c, m);
        if (c.V0 == 0u) {
            goto L8001833C;
        }
        if (c.S0 == 0u) {
            goto L8001833C;
        }
        c.A0 = 0x00000005u;
        c.RA = 0x800182ECu;
        GranTurismo2ArcadePC.func_80060750(c, m);
        goto L8001833C;
        L800182F4: ;
        c.V0 = c.T0 << 1;
        c.V0 = c.A0 + c.V0;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x4u));
        c.T0 = c.T0 ^ c.V1;
        c.A3 = c.A3 + c.V0;
        c.V0 = ~(0u | c.V1);
        c.V0 = c.V0 >> 31;
        c.V1 = 0u < c.T0 ? 1u : 0u;
        c.V0 = c.V0 & c.V1;
        if (c.V0 == 0u) {
            c.A0 = c.T6 + 0u;
            goto L80018324;
        }
        c.A0 = c.T6 + 0u;
        c.T5 = (uint)((int)c.T4 >> 8);
        L80018324: ;
        c.A1 = c.T2 + 0u;
        c.A2 = c.T3 + 0u;
        c.V0 = c.T0 < 0x00000001u ? 1u : 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T5);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V0);
        c.RA = 0x8001833Cu;
        GranTurismo2ArcadePC.func_80017CC8(c, m);
        L8001833C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.V0 = 0x00000001u;
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80018350_gt2_arcade_overlay_1(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x60u;
        MemoryAccess.WriteU32(m, (c.SP + 0x50u), c.S6);
        c.S6 = c.A0 + 0u;
        c.V0 = c.A1 + 0u;
        c.A0 = c.SP + 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x5Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x58u), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0x54u), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x4Cu), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x48u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x3Cu), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x38u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x68u), c.A2);
        c.S2 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.S7 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        c.S4 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0xCu));
        c.FP = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0xEu));
        c.S5 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x10u));
        c.S0 = MemoryAccess.ReadU32(m, c.V0);
        c.A1 = 0x0000001Eu;
        c.RA = 0x800183ACu;
        GranTurismo2ArcadePC.func_8006AB78(c, m);
        c.V1 = 0xFF9F0000u;
        c.V1 = c.V1 | 0xFFFFu;
        c.S1 = c.SP + 0x18u;
        c.A0 = c.S1 + 0u;
        c.A1 = 0x801C0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0xCu));
        c.A1 = c.A1 - 0x6D10u;
        c.V0 = c.V0 & c.V1;
        c.V1 = 0x00200000u;
        c.V0 = c.V0 | c.V1;
        MemoryAccess.WriteU32(m, (c.S1 + 0x10u), c.S7);
        MemoryAccess.WriteU32(m, (c.S1 + 0xCu), c.V0);
        c.RA = 0x800183E0u;
        GranTurismo2ArcadePC.func_8007D990(c, m);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x9u));
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x68u));
        if (c.V0 == c.T0) {
            c.S3 = 0x00000001u;
            goto L800183F8;
        }
        c.S3 = 0x00000001u;
        c.S2 = 0u + 0u;
        L800183F8: ;
        if (c.S6 == c.S3) {
            c.V0 = (int)c.S6 < 2 ? 1u : 0u;
            goto L800187F4;
        }
        c.V0 = (int)c.S6 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000003u;
            goto L80018418;
        }
        c.V0 = 0x00000003u;
        if (c.S6 == 0u) {
            goto L8001861C;
        }
        goto L80018B34;
        L80018418: ;
        if (c.S6 == c.V0) {
            c.V0 = 0x00000004u;
            goto L80018430;
        }
        c.V0 = 0x00000004u;
        if (c.S6 == c.V0) {
            goto L800184F4;
        }
        goto L80018B34;
        L80018430: ;
        MemoryAccess.WriteU8(m, (c.S0 + 0x10u), (byte)c.S3);
        MemoryAccess.WriteU8(m, (c.S0 + 0x11u), (byte)c.S3);
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x68u));
        c.V0 = c.T0 < 0x00000005u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80020000u;
            goto L80018B34;
        }
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x23D8u;
        c.V1 = c.T0 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x80018468u: goto L80018468;
            case 0x80018474u: goto L80018474;
            case 0x80018480u: goto L80018480;
            case 0x80018494u: goto L80018494;
            case 0x800184B4u: goto L800184B4;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L80018468: ;
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU16(m, (c.V0 - 0x4D4Au), (ushort)0u);
        goto L800184E4;
        L80018474: ;
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU16(m, (c.V0 - 0x4C12u), (ushort)0u);
        goto L800184E4;
        L80018480: ;
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0xFC8u;
        c.RA = 0x8001848Cu;
        GranTurismo2ArcadePC.func_8001A4A0(c, m);
        goto L800184E4;
        L80018494: ;
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0xFD0u;
        c.RA = 0x800184A0u;
        GranTurismo2ArcadePC.func_8001C2FC(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 == 0u) {
            goto L800184E4;
        }
        goto L800184CC;
        L800184B4: ;
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x1050u;
        c.RA = 0x800184C0u;
        GranTurismo2ArcadePC.func_8001C2FC(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 == 0u) {
            goto L800184E4;
        }
        L800184CC: ;
        c.A0 = 0u + 0u;
        c.RA = 0x800184D4u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x10u), (byte)0u);
        MemoryAccess.WriteU8(m, (c.S0 + 0x11u), (byte)c.V0);
        goto L80018B34;
        L800184E4: ;
        c.A0 = 0x00000001u;
        c.RA = 0x800184ECu;
        GranTurismo2ArcadePC.func_80060750(c, m);
        goto L80018B34;
        L800184F4: ;
        c.V1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0xEu));
        c.V0 = c.V1 < 0x00000005u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80020000u;
            goto L80018594;
        }
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x23F0u;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x80018524u: goto L80018524;
            case 0x8001854Cu: goto L8001854C;
            case 0x80018594u: goto L80018594;
            case 0x80018578u: goto L80018578;
            case 0x80018584u: goto L80018584;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L80018524: ;
        c.V1 = 0x80050000u;
        c.V1 = c.V1 - 0x4CF8u;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4CDCu;
        c.A1 = 0x80050000u;
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 + 0x4u));
        c.A1 = c.A1 - 0x4CC0u;
        c.V0 = ~(0u | c.V0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x18u), (ushort)c.V0);
        goto L8001855C;
        L8001854C: ;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4BC0u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x4BA4u;
        L8001855C: ;
        c.V0 = MemoryAccess.ReadU16(m, (c.A0 + 0x4u));
        c.V1 = MemoryAccess.ReadU16(m, (c.A1 + 0x4u));
        c.V0 = ~(0u | c.V0);
        c.V1 = ~(0u | c.V1);
        MemoryAccess.WriteU16(m, (c.A0 + 0x18u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.A1 + 0x18u), (ushort)c.V1);
        goto L80018594;
        L80018578: ;
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0xFD0u;
        goto L8001858C;
        L80018584: ;
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x1050u;
        L8001858C: ;
        c.RA = 0x80018594u;
        GranTurismo2ArcadePC.func_8001C2B4(c, m);
        L80018594: ;
        c.V1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x9u));
        c.V0 = c.V1 < 0x00000005u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80020000u;
            goto L80018B34;
        }
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x2408u;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x800185C4u: goto L800185C4;
            case 0x800185E0u: goto L800185E0;
            case 0x80018B34u: goto L80018B34;
            case 0x800185F4u: goto L800185F4;
            case 0x80018608u: goto L80018608;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L800185C4: ;
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU16(m, (c.V0 - 0x4CE0u), (ushort)0u);
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU16(m, (c.V0 - 0x4CC4u), (ushort)0u);
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU16(m, (c.V0 - 0x4CA8u), (ushort)0u);
        goto L80018B34;
        L800185E0: ;
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU16(m, (c.V0 - 0x4BA8u), (ushort)0u);
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU16(m, (c.V0 - 0x4B8Cu), (ushort)0u);
        goto L80018B34;
        L800185F4: ;
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0xFD0u;
        c.RA = 0x80018600u;
        GranTurismo2ArcadePC.func_8001C27C(c, m);
        goto L80018B34;
        L80018608: ;
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x1050u;
        c.RA = 0x80018614u;
        GranTurismo2ArcadePC.func_8001C27C(c, m);
        goto L80018B34;
        L8001861C: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x10u));
        if (c.V0 != 0u) {
            goto L80018630;
        }
        c.S2 = 0u + 0u;
        L80018630: ;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x68u));
        c.V0 = c.T0 < 0x00000005u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80020000u;
            goto L800187B0;
        }
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x2420u;
        c.V1 = c.T0 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x80018660u: goto L80018660;
            case 0x800186A8u: goto L800186A8;
            case 0x80018700u: goto L80018700;
            case 0x80018744u: goto L80018744;
            case 0x80018760u: goto L80018760;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L80018660: ;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4D50u;
        c.A1 = c.S2 + 0u;
        c.RA = 0x80018670u;
        GranTurismo2ArcadePC.func_8006CED4(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = 0xFFFFFFFEu;
        if (c.V1 == c.V0) {
            c.V0 = (int)c.V1 < -1 ? 1u : 0u;
            goto L800187B0;
        }
        c.V0 = (int)c.V1 < -1 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0xFFFFFFFDu;
            goto L80018690;
        }
        c.V0 = 0xFFFFFFFDu;
        if (c.V1 == c.V0) {
            goto L800186D8;
        }
        L80018690: ;
        c.A0 = 0x00000001u;
        c.RA = 0x80018698u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.V0 = 0x80050000u;
        c.V1 = 0xFFFFFFFFu;
        MemoryAccess.WriteU16(m, (c.V0 - 0x4D4Au), (ushort)c.V1);
        goto L800187A4;
        L800186A8: ;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4C18u;
        c.A1 = c.S2 + 0u;
        c.RA = 0x800186B8u;
        GranTurismo2ArcadePC.func_8006CED4(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = 0xFFFFFFFEu;
        if (c.V1 == c.V0) {
            c.V0 = (int)c.V1 < -1 ? 1u : 0u;
            goto L800187B0;
        }
        c.V0 = (int)c.V1 < -1 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0xFFFFFFFDu;
            goto L800186E8;
        }
        c.V0 = 0xFFFFFFFDu;
        if (c.V1 != c.V0) {
            goto L800186E8;
        }
        L800186D8: ;
        c.A0 = 0x00000006u;
        c.RA = 0x800186E0u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.A0 = 0x80050000u;
        goto L800187B4;
        L800186E8: ;
        c.A0 = 0x00000001u;
        c.RA = 0x800186F0u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.V0 = 0x80050000u;
        c.V1 = 0xFFFFFFFFu;
        MemoryAccess.WriteU16(m, (c.V0 - 0x4C12u), (ushort)c.V1);
        goto L800187A4;
        L80018700: ;
        c.V0 = 0x800B0000u;
        c.S1 = c.V0 + 0xFC8u;
        c.A0 = c.S1 + 0u;
        c.A1 = c.S2 + 0u;
        c.RA = 0x80018714u;
        GranTurismo2ArcadePC.func_8001A4CC(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = 0xFFFFFFFFu;
        if (c.V1 == c.V0) {
            c.A0 = 0x80050000u;
            goto L800187B4;
        }
        c.A0 = 0x80050000u;
        if (c.V1 != 0u) {
            goto L800187B4;
        }
        c.A0 = 0x00000001u;
        c.RA = 0x80018734u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.A0 = c.S1 + 0u;
        c.RA = 0x8001873Cu;
        GranTurismo2ArcadePC.func_8001A4B8(c, m);
        c.V0 = 0x00000001u;
        goto L800187A8;
        L80018744: ;
        c.V0 = 0x800B0000u;
        c.S1 = c.V0 + 0xFD0u;
        goto L80018768;
        L80018750: ;
        if (c.V1 == c.V0) {
            c.A0 = 0x80050000u;
            goto L8001879C;
        }
        c.A0 = 0x80050000u;
        goto L800187B4;
        L80018760: ;
        c.V0 = 0x800B0000u;
        c.S1 = c.V0 + 0x1050u;
        L80018768: ;
        c.A0 = c.S1 + 0u;
        c.A1 = c.S2 + 0u;
        c.RA = 0x80018774u;
        GranTurismo2ArcadePC.func_8001C424(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = 0xFFFFFFFFu;
        if (c.V1 == c.V0) {
            c.A0 = 0x80050000u;
            goto L800187B4;
        }
        c.A0 = 0x80050000u;
        if ((int)c.V1 < 0) {
            c.V0 = 0xFFFFFFFEu;
            goto L80018750;
        }
        c.V0 = 0xFFFFFFFEu;
        if (c.V1 != 0u) {
            goto L800187B4;
        }
        c.A0 = 0x00000001u;
        c.RA = 0x8001879Cu;
        GranTurismo2ArcadePC.func_80060750(c, m);
        L8001879C: ;
        c.A0 = c.S1 + 0u;
        c.RA = 0x800187A4u;
        GranTurismo2ArcadePC.func_8001C2F0(c, m);
        L800187A4: ;
        c.V0 = 0x00000001u;
        L800187A8: ;
        MemoryAccess.WriteU8(m, (c.S0 + 0x10u), (byte)0u);
        MemoryAccess.WriteU8(m, (c.S0 + 0x11u), (byte)c.V0);
        L800187B0: ;
        c.A0 = 0x80050000u;
        L800187B4: ;
        c.A0 = c.A0 - 0x4CF8u;
        c.RA = 0x800187BCu;
        GranTurismo2ArcadePC.func_8006BD74(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4CDCu;
        c.RA = 0x800187C8u;
        GranTurismo2ArcadePC.func_8006BD74(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4CC0u;
        c.RA = 0x800187D4u;
        GranTurismo2ArcadePC.func_8006BD74(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4BC0u;
        c.RA = 0x800187E0u;
        GranTurismo2ArcadePC.func_8006BD74(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4BA4u;
        c.RA = 0x800187ECu;
        GranTurismo2ArcadePC.func_8006BD74(c, m);
        goto L80018B34;
        L800187F4: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x10u));
        if (c.V0 == 0u) {
            c.A3 = 0x00000080u;
            goto L80018808;
        }
        c.A3 = 0x00000080u;
        c.A3 = 0x00000200u;
        L80018808: ;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4E60u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x4E4Cu;
        c.A2 = c.S5 + 0u;
        c.RA = 0x80018820u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S1 + 0u;
        c.A2 = c.S4 + 0u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.V0);
        c.V0 = 0x80050000u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x68u));
        c.V0 = c.V0 - 0x4B88u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), 0u);
        c.S0 = c.T0 << 2;
        c.V0 = c.S0 + c.V0;
        c.A1 = MemoryAccess.ReadU32(m, c.V0);
        c.A3 = c.FP + 0x14u;
        c.RA = 0x80018850u;
        GranTurismo2ArcadePC.func_8006ACC4(c, m);
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x68u));
        c.V0 = c.T0 < 0x00000005u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80020000u;
            goto L80018B34;
        }
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x2438u;
        c.V0 = c.S0 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V0);
        switch (c.V0)
        {
            case 0x8001887Cu: goto L8001887C;
            case 0x800189D0u: goto L800189D0;
            case 0x80018AE4u: goto L80018AE4;
            case 0x80018B08u: goto L80018B08;
            case 0x80018B18u: goto L80018B18;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L8001887C: ;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4D50u;
        c.A1 = c.S7 + 0u;
        c.V0 = c.FP + 0x2Eu;
        MemoryAccess.WriteU16(m, (c.A0 + 0x12u), (ushort)c.V0);
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU16(m, (c.A0 + 0x10u), (ushort)c.S4);
        MemoryAccess.WriteU32(m, (c.V0 - 0x4D1Cu), c.S5);
        c.RA = 0x800188A0u;
        GranTurismo2ArcadePC.func_8006D41C(c, m);
        c.A0 = c.SP + 0x18u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6CE0u;
        c.RA = 0x800188B0u;
        GranTurismo2ArcadePC.func_8007D990(c, m);
        c.S3 = 0x80050000u;
        c.S3 = c.S3 - 0x4E60u;
        c.A0 = c.S3 + 0u;
        c.S2 = 0x80050000u;
        c.S2 = c.S2 - 0x4E40u;
        c.A1 = c.S2 + 0u;
        c.A2 = c.S5 + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x800188D4u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.S1 = c.SP + 0x18u;
        c.A0 = c.S1 + 0u;
        c.S0 = 0x801C0000u;
        c.S0 = c.S0 - 0x6842u;
        c.A1 = c.S0 + 0u;
        c.S6 = c.S4 - 0x8Eu;
        c.A2 = c.S6 + 0u;
        c.A3 = c.FP + 0x3Au;
        MemoryAccess.WriteU32(m, (c.S1 + 0x14u), c.V0);
        c.T0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        c.RA = 0x80018904u;
        GranTurismo2ArcadePC.func_8006ABA0(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4CF8u;
        c.A1 = c.S7 + 0u;
        c.S4 = c.S4 - 0x94u;
        c.A2 = c.S4 + 0u;
        c.A3 = c.FP + 0x21u;
        c.RA = 0x80018920u;
        GranTurismo2ArcadePC.func_8006BE04(c, m);
        c.A0 = c.S7 + 0u;
        c.A1 = 0x00000220u;
        c.RA = 0x8001892Cu;
        GranTurismo2ArcadePC.func_8007D954(c, m);
        c.A0 = c.S3 + 0u;
        c.A1 = c.S2 + 0u;
        c.A2 = c.S5 + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x80018940u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S0 + 0x11u;
        c.A2 = c.S6 + 0u;
        c.A3 = c.FP + 0xD2u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x14u), c.V0);
        c.T0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        c.RA = 0x80018960u;
        GranTurismo2ArcadePC.func_8006ABA0(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4CDCu;
        c.A1 = c.S7 + 0u;
        c.A2 = c.S4 + 0u;
        c.A3 = c.FP + 0xB9u;
        c.RA = 0x80018978u;
        GranTurismo2ArcadePC.func_8006BE04(c, m);
        c.A0 = c.S7 + 0u;
        c.A1 = 0x00000220u;
        c.RA = 0x80018984u;
        GranTurismo2ArcadePC.func_8007D954(c, m);
        c.A0 = c.S3 + 0u;
        c.A1 = c.S2 + 0u;
        c.A2 = c.S5 + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x80018998u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S0 + 0x18u;
        c.A2 = c.S6 + 0u;
        c.A3 = c.FP + 0x122u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.V0);
        c.T0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        c.RA = 0x800189B8u;
        GranTurismo2ArcadePC.func_8006ABA0(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4CC0u;
        c.A1 = c.S7 + 0u;
        c.A2 = c.S4 + 0u;
        c.A3 = c.FP + 0x109u;
        goto L80018AC8;
        L800189D0: ;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4C18u;
        c.A1 = c.S7 + 0u;
        c.V0 = c.FP + 0x2Eu;
        MemoryAccess.WriteU16(m, (c.A0 + 0x12u), (ushort)c.V0);
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU16(m, (c.A0 + 0x10u), (ushort)c.S4);
        MemoryAccess.WriteU32(m, (c.V0 - 0x4BE4u), c.S5);
        c.RA = 0x800189F4u;
        GranTurismo2ArcadePC.func_8006D41C(c, m);
        c.A0 = c.SP + 0x18u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6CE0u;
        c.RA = 0x80018A04u;
        GranTurismo2ArcadePC.func_8007D990(c, m);
        c.S2 = 0x80050000u;
        c.S2 = c.S2 - 0x4E60u;
        c.A0 = c.S2 + 0u;
        c.S1 = 0x80050000u;
        c.S1 = c.S1 - 0x4E40u;
        c.A1 = c.S1 + 0u;
        c.A2 = c.S5 + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x80018A28u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.S3 = c.SP + 0x18u;
        c.A0 = c.S3 + 0u;
        c.S0 = 0x801C0000u;
        c.S0 = c.S0 - 0x67FDu;
        c.A1 = c.S0 + 0u;
        c.S6 = c.S4 - 0x8Eu;
        c.A2 = c.S6 + 0u;
        c.A3 = c.FP + 0x3Au;
        MemoryAccess.WriteU32(m, (c.S3 + 0x14u), c.V0);
        c.T0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        c.RA = 0x80018A58u;
        GranTurismo2ArcadePC.func_8006ABA0(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4BC0u;
        c.A1 = c.S7 + 0u;
        c.S4 = c.S4 - 0x94u;
        c.A2 = c.S4 + 0u;
        c.A3 = c.FP + 0x21u;
        c.RA = 0x80018A74u;
        GranTurismo2ArcadePC.func_8006BE04(c, m);
        c.A0 = c.S7 + 0u;
        c.A1 = 0x00000220u;
        c.RA = 0x80018A80u;
        GranTurismo2ArcadePC.func_8007D954(c, m);
        c.A0 = c.S2 + 0u;
        c.A1 = c.S1 + 0u;
        c.A2 = c.S5 + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x80018A94u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S3 + 0u;
        c.A1 = c.S0 + 0xFu;
        c.A2 = c.S6 + 0u;
        c.A3 = c.FP + 0x8Au;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.V0);
        c.T0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        c.RA = 0x80018AB4u;
        GranTurismo2ArcadePC.func_8006ABA0(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4BA4u;
        c.A1 = c.S7 + 0u;
        c.A2 = c.S4 + 0u;
        c.A3 = c.FP + 0x71u;
        L80018AC8: ;
        c.RA = 0x80018AD0u;
        GranTurismo2ArcadePC.func_8006BE04(c, m);
        c.A0 = c.S7 + 0u;
        c.A1 = 0x00000220u;
        c.RA = 0x80018ADCu;
        GranTurismo2ArcadePC.func_8007D954(c, m);
        goto L80018B34;
        L80018AE4: ;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S5);
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0xFC8u;
        c.A1 = c.S7 + 0u;
        c.A2 = c.S4 + 0u;
        c.A3 = c.FP + 0x40u;
        c.RA = 0x80018B00u;
        GranTurismo2ArcadePC.func_8001A6F0(c, m);
        goto L80018B34;
        L80018B08: ;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S5);
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0xFD0u;
        goto L80018B24;
        L80018B18: ;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S5);
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x1050u;
        L80018B24: ;
        c.A1 = c.S7 + 0u;
        c.A2 = c.S4 + 0u;
        c.A3 = c.FP + 0x2Eu;
        c.RA = 0x80018B34u;
        GranTurismo2ArcadePC.func_8001CA94(c, m);
        L80018B34: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x5Cu));
        c.FP = MemoryAccess.ReadU32(m, (c.SP + 0x58u));
        c.S7 = MemoryAccess.ReadU32(m, (c.SP + 0x54u));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x50u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x4Cu));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x48u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x44u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x40u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x3Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x38u));
        c.V0 = 0u + 0u;
        c.SP = c.SP + 0x60u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80018B68(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0x80050000u;
        c.S0 = c.S0 - 0x4B74u;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x80020000u;
        c.A1 = c.A1 - 0x7CB0u;
        c.A2 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.RA = 0x80018B98u;
        GranTurismo2ArcadePC.func_8001D11C(c, m);
        c.A0 = c.S0 + 0u;
        c.RA = 0x80018BA0u;
        GranTurismo2ArcadePC.func_8001D140(c, m);
        c.S1 = 0x80050000u;
        c.S1 = c.S1 - 0x4D50u;
        c.A0 = c.S1 + 0u;
        c.S0 = 0x80020000u;
        c.S0 = c.S0 - 0x7E20u;
        c.A1 = c.S0 + 0u;
        c.A2 = 0x80050000u;
        c.A2 = c.A2 - 0x4DF0u;
        c.RA = 0x80018BC4u;
        GranTurismo2ArcadePC.func_8006CCDC(c, m);
        c.A0 = c.S1 + 0u;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 - 0x4D1Cu;
        MemoryAccess.WriteU32(m, (c.S1 + 0x30u), c.V0);
        c.RA = 0x80018BD8u;
        GranTurismo2ArcadePC.func_8006CD80(c, m);
        c.S2 = 0x80050000u;
        c.S2 = c.S2 - 0x4C18u;
        c.A0 = c.S2 + 0u;
        c.A1 = c.S0 + 0u;
        c.A2 = 0x80050000u;
        c.A2 = c.A2 - 0x4CA4u;
        c.S0 = 0xFFFFFFFFu;
        c.T0 = 0x80050000u;
        c.T0 = c.T0 - 0x4CF8u;
        c.A3 = c.S0 + 0u;
        c.V1 = 0x80050000u;
        c.V1 = c.V1 - 0x4CDCu;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 - 0x4CC0u;
        MemoryAccess.WriteU16(m, (c.S1 + 0x6u), (ushort)c.S0);
        MemoryAccess.WriteU16(m, (c.T0 + 0x18u), (ushort)c.A3);
        MemoryAccess.WriteU16(m, (c.V1 + 0x18u), (ushort)c.A3);
        MemoryAccess.WriteU16(m, (c.V0 + 0x18u), (ushort)c.A3);
        MemoryAccess.WriteU16(m, (c.T0 + 0x18u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.V1 + 0x18u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.V0 + 0x18u), (ushort)0u);
        c.RA = 0x80018C30u;
        GranTurismo2ArcadePC.func_8006CCDC(c, m);
        c.A0 = c.S2 + 0u;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 - 0x4BE4u;
        MemoryAccess.WriteU32(m, (c.S2 + 0x30u), c.V0);
        c.RA = 0x80018C44u;
        GranTurismo2ArcadePC.func_8006CD80(c, m);
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0xFC8u;
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU16(m, (c.S2 + 0x6u), (ushort)c.S0);
        MemoryAccess.WriteU16(m, (c.V0 - 0x4BA8u), (ushort)c.S0);
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU16(m, (c.V0 - 0x4B8Cu), (ushort)c.S0);
        c.RA = 0x80018C64u;
        GranTurismo2ArcadePC.func_8001A44C(c, m);
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0xFD0u;
        c.A1 = 0u + 0u;
        c.RA = 0x80018C74u;
        GranTurismo2ArcadePC.func_8001C0F8(c, m);
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x1050u;
        c.A1 = 0x00000001u;
        c.RA = 0x80018C84u;
        GranTurismo2ArcadePC.func_8001C0F8(c, m);
        c.A0 = 0x00000006u;
        c.RA = 0x80018C8Cu;
        GranTurismo2ArcadePC.func_80012380(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80018CA4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = 0u + 0u;
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.V0 - 0x4B74u;
        c.A0 = c.S0 + 0u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.A3 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x9u));
        c.A2 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        c.A3 = c.A3 ^ 0x0004u;
        c.A1 = c.A2 + 0x1A4u;
        c.A2 = c.A2 + 0x1B4u;
        c.A3 = 0u < c.A3 ? 1u : 0u;
        c.RA = 0x80018CE4u;
        GranTurismo2ArcadePC.func_8001D160(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = 0xFFFFFFFEu;
        if (c.V1 == c.V0) {
            c.V0 = (int)c.V1 < -1 ? 1u : 0u;
            goto L80018D44;
        }
        c.V0 = (int)c.V1 < -1 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0xFFFFFFFDu;
            goto L80018D0C;
        }
        c.V0 = 0xFFFFFFFDu;
        if (c.V1 == c.V0) {
            c.V0 = c.S1 + 0u;
            goto L80018D20;
        }
        c.V0 = c.S1 + 0u;
        goto L80018D48;
        L80018D0C: ;
        c.V0 = 0xFFFFFFFFu;
        if (c.V1 == c.V0) {
            c.V0 = c.S1 + 0u;
            goto L80018D30;
        }
        c.V0 = c.S1 + 0u;
        goto L80018D48;
        L80018D20: ;
        c.A0 = 0x00000007u;
        c.RA = 0x80018D28u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.V0 = c.S1 + 0u;
        goto L80018D48;
        L80018D30: ;
        c.A0 = c.S0 + 0u;
        c.RA = 0x80018D38u;
        GranTurismo2ArcadePC.func_8001D154(c, m);
        c.A0 = 0x00000004u;
        c.RA = 0x80018D40u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.S1 = 0x00000002u;
        L80018D44: ;
        c.V0 = c.S1 + 0u;
        L80018D48: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80018D5C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4B74u;
        c.RA = 0x80018D74u;
        GranTurismo2ArcadePC.func_8001D42C(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80018D84(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1C4u));
        c.V0 = 0x00000010u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x18u), c.A0);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80018DA4(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V1 = MemoryAccess.ReadU32(m, (c.A0 + 0x1C4u));
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x14u));
        if ((int)c.V0 <= 0) {
            c.A1 = 0u + 0u;
            goto L80018DF8;
        }
        c.A1 = 0u + 0u;
        c.V0 = c.V0 - 0x1u;
        if (c.V0 != 0u) {
            MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
            goto L80018DF8;
        }
        MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 + 0x18u));
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            c.A1 = 0x00000004u;
            goto L80018DF8;
        }
        c.A1 = 0x00000004u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x4AB0u;
        c.RA = 0x80018DF4u;
        GranTurismo2ArcadePC.func_8001220C(c, m);
        c.A1 = 0x00000001u;
        L80018DF8: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = c.A1 + 0u;
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80018E08(CpuContext c, IMemory m)
    {
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80018E10(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1C4u));
        c.V0 = 0x00000010u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
        c.RA = 0x80018E30u;
        GranTurismo2ArcadePC.func_800123A0(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80018E40(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1C4u));
        c.V1 = MemoryAccess.ReadU32(m, (c.A0 + 0x14u));
        if ((int)c.V1 <= 0) {
            c.V0 = 0u + 0u;
            goto L80018E74;
        }
        c.V0 = 0u + 0u;
        c.V0 = c.V1 - 0x1u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.V0);
        c.V0 = c.V0 < 0x00000001u ? 1u : 0u;
        c.V0 = c.V0 << 2;
        L80018E74: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80018E7C(CpuContext c, IMemory m)
    {
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80018E84(CpuContext c, IMemory m)
    {
        c.V0 = 0x00000009u;
        if (c.A0 != c.V0) {
            c.V0 = 0x0000000Au;
            goto L80018E98;
        }
        c.V0 = 0x0000000Au;
        c.V0 = 0x00000081u;
        return;
        L80018E98: ;
        if (c.A0 == c.V0) {
            c.V1 = 0x00000004u;
            goto L80018EB0;
        }
        c.V1 = 0x00000004u;
        if (c.A0 == c.V1) {
            c.V0 = 0x00000083u;
            goto L80018EB4;
        }
        c.V0 = 0x00000083u;
        c.V0 = c.A0 + 0u;
        return;
        L80018EB0: ;
        c.V0 = 0x00000082u;
        L80018EB4: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80018EBC(CpuContext c, IMemory m)
    {
        c.V0 = 0x00000081u;
        if (c.A0 != c.V0) {
            c.V0 = 0x00000082u;
            goto L80018ED0;
        }
        c.V0 = 0x00000082u;
        c.V0 = 0x00000009u;
        return;
        L80018ED0: ;
        if (c.A0 == c.V0) {
            c.V1 = 0x00000083u;
            goto L80018EE8;
        }
        c.V1 = 0x00000083u;
        if (c.A0 == c.V1) {
            c.V0 = 0x00000004u;
            goto L80018EEC;
        }
        c.V0 = 0x00000004u;
        c.V0 = c.A0 + 0u;
        return;
        L80018EE8: ;
        c.V0 = 0x0000000Au;
        L80018EEC: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80018EF4(CpuContext c, IMemory m)
    {
        c.V0 = 0x000000C3u;
        if (c.A0 != c.V0) {
            c.V0 = 0x000000A3u;
            goto L80018F08;
        }
        c.V0 = 0x000000A3u;
        c.V0 = 0x00000003u;
        return;
        L80018F08: ;
        if (c.A0 != c.V0) {
            c.V0 = 0x000000C2u;
            goto L80018F18;
        }
        c.V0 = 0x000000C2u;
        c.V0 = 0x00000002u;
        return;
        L80018F18: ;
        if (c.A0 != c.V0) {
            c.V0 = 0x000000A2u;
            goto L80018F28;
        }
        c.V0 = 0x000000A2u;
        c.V0 = 0x00000001u;
        return;
        L80018F28: ;
        if (c.A0 != c.V0) {
            c.V0 = 0x000000C1u;
            goto L80018F38;
        }
        c.V0 = 0x000000C1u;
        c.V0 = 0u + 0u;
        return;
        L80018F38: ;
        if (c.A0 != c.V0) {
            c.V0 = 0x000000A1u;
            goto L80018F48;
        }
        c.V0 = 0x000000A1u;
        c.V0 = 0x00000007u;
        return;
        L80018F48: ;
        if (c.A0 != c.V0) {
            c.V0 = 0x000000C0u;
            goto L80018F58;
        }
        c.V0 = 0x000000C0u;
        c.V0 = 0x00000006u;
        return;
        L80018F58: ;
        if (c.A0 == c.V0) {
            c.V0 = c.A0 ^ 0x00A0u;
            goto L80018F6C;
        }
        c.V0 = c.A0 ^ 0x00A0u;
        c.V0 = c.V0 < 0x00000001u ? 1u : 0u;
        c.V0 = c.V0 << 2;
        return;
        L80018F6C: ;
        c.V0 = 0x00000005u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80018F74(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.RA = 0x80018F84u;
        GranTurismo2ArcadePC.func_80018EBC(c, m);
        c.A0 = c.V0 + 0u;
        c.V1 = c.A0 - 0x9u;
        c.V1 = c.V1 < 0x00000002u ? 1u : 0u;
        c.V0 = c.A0 ^ 0x0004u;
        c.V0 = c.V0 < 0x00000001u ? 1u : 0u;
        c.V1 = c.V1 | c.V0;
        if (c.V1 != 0u) {
            c.V1 = 0x80050000u;
            goto L80018FAC;
        }
        c.V1 = 0x80050000u;
        c.V0 = 0u + 0u;
        goto L80018FD8;
        L80018FAC: ;
        c.V1 = c.V1 - 0x4830u;
        c.V0 = c.A0 << 1;
        c.V0 = c.V0 + c.V1;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.V0);
        c.V1 = c.V0 << 1;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 2;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 - 0x4958u;
        c.V0 = c.V1 + c.V0;
        L80018FD8: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80018FE8(CpuContext c, IMemory m)
    {
        c.V0 = 0x00000003u;
        MemoryAccess.WriteU32(m, c.A0, c.V0);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80018FF4(CpuContext c, IMemory m)
    {
        c.T0 = 0x00000003u;
        c.A3 = 0u + 0u;
        c.V1 = 0x801D0000u;
        c.V1 = c.V1 - 0x6CC0u;
        c.V0 = c.A2 << 2;
        c.V0 = c.V0 + c.A2;
        c.V0 = c.V0 << (int)(c.T0 & 31u);
        c.V0 = c.V0 + c.A2;
        c.V0 = c.V0 << 1;
        c.V0 = c.V0 + 0xAu;
        c.A1 = MemoryAccess.ReadU8(m, (c.A1 + 0x2u));
        c.A2 = c.V0 + c.V1;
        c.V0 = 0x00000004u;
        if (c.A1 == c.V0) {
            c.V1 = c.A1 + 0u;
            goto L80019068;
        }
        c.V1 = c.A1 + 0u;
        c.V0 = (int)c.A1 < 5 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L8001904C;
        }
        c.V0 = 0x00000002u;
        if (c.A1 == c.V0) {
            goto L80019070;
        }
        MemoryAccess.WriteU32(m, c.A0, c.T0);
        goto L80019084;
        L8001904C: ;
        c.V0 = 0x00000005u;
        if (c.V1 == c.V0) {
            c.V0 = 0x00000007u;
            goto L80019078;
        }
        c.V0 = 0x00000007u;
        if (c.V1 == c.V0) {
            goto L80019078;
        }
        MemoryAccess.WriteU32(m, c.A0, c.T0);
        goto L80019084;
        L80019068: ;
        c.T0 = 0u + 0u;
        goto L80019080;
        L80019070: ;
        c.T0 = 0x00000002u;
        goto L8001907C;
        L80019078: ;
        c.T0 = 0x00000001u;
        L8001907C: ;
        c.A3 = c.T0 + 0u;
        L80019080: ;
        MemoryAccess.WriteU32(m, c.A0, c.T0);
        L80019084: ;
        c.V0 = 0x00000003u;
        if (c.T0 == c.V0) {
            c.V0 = c.A3 << 1;
            goto L800190FC;
        }
        c.V0 = c.A3 << 1;
        c.V0 = c.V0 + c.A3;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 - c.A3;
        c.V0 = c.A2 + c.V0;
        c.T1 = MemoryAccess.ReadWordLeft(m, c.T1, (c.V0 + 0x3u));
        c.T1 = MemoryAccess.ReadWordRight(m, c.T1, c.V0);
        c.T2 = MemoryAccess.ReadWordLeft(m, c.T2, (c.V0 + 0x7u));
        c.T2 = MemoryAccess.ReadWordRight(m, c.T2, (c.V0 + 0x4u));
        c.T3 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        c.T4 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V0 + 0x9u));
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0xBu), c.T1);
        MemoryAccess.WriteWordRight(m, (c.A0 + 0x8u), c.T1);
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0xFu), c.T2);
        MemoryAccess.WriteWordRight(m, (c.A0 + 0xCu), c.T2);
        MemoryAccess.WriteU8(m, (c.A0 + 0x10u), (byte)c.T3);
        MemoryAccess.WriteU8(m, (c.A0 + 0x11u), (byte)c.T4);
        c.T1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V0 + 0xAu));
        MemoryAccess.WriteU8(m, (c.A0 + 0x12u), (byte)c.T1);
        c.V0 = c.A3 << 2;
        c.V0 = c.V0 + 0x2Du;
        c.V0 = c.A2 + c.V0;
        c.T1 = MemoryAccess.ReadWordLeft(m, c.T1, (c.V0 + 0x3u));
        c.T1 = MemoryAccess.ReadWordRight(m, c.T1, c.V0);
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0x7u), c.T1);
        MemoryAccess.WriteWordRight(m, (c.A0 + 0x4u), c.T1);
        L800190FC: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019104(CpuContext c, IMemory m)
    {
        c.A3 = 0x00000003u;
        c.A2 = 0u + 0u;
        c.V1 = 0x801D0000u;
        c.V1 = c.V1 - 0x6CC0u;
        c.V0 = c.A1 << 2;
        c.V0 = c.V0 + c.A1;
        c.V0 = c.V0 << (int)(c.A3 & 31u);
        c.V0 = c.V0 + c.A1;
        c.V0 = c.V0 << 1;
        c.V0 = c.V0 + 0xAu;
        c.A1 = MemoryAccess.ReadU32(m, c.A0);
        c.T0 = c.V0 + c.V1;
        c.V0 = 0x00000001u;
        if (c.A1 == c.V0) {
            c.V1 = c.A1 + 0u;
            goto L8001917C;
        }
        c.V1 = c.A1 + 0u;
        c.V0 = (int)c.A1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L8001915C;
        }
        c.V0 = 0x00000002u;
        if (c.A1 == 0u) {
            c.V0 = 0x00000003u;
            goto L8001916C;
        }
        c.V0 = 0x00000003u;
        goto L80019188;
        L8001915C: ;
        if (c.V1 == c.V0) {
            c.V0 = 0x00000003u;
            goto L80019174;
        }
        c.V0 = 0x00000003u;
        goto L80019188;
        L8001916C: ;
        c.A3 = 0u + 0u;
        goto L80019184;
        L80019174: ;
        c.A3 = 0x00000002u;
        goto L80019180;
        L8001917C: ;
        c.A3 = 0x00000001u;
        L80019180: ;
        c.A2 = c.A3 + 0u;
        L80019184: ;
        c.V0 = 0x00000003u;
        L80019188: ;
        if (c.A3 == c.V0) {
            c.V0 = c.A2 << 1;
            goto L800191FC;
        }
        c.V0 = c.A2 << 1;
        c.V0 = c.V0 + c.A2;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 - c.A2;
        c.V0 = c.T0 + c.V0;
        c.T1 = MemoryAccess.ReadWordLeft(m, c.T1, (c.A0 + 0xBu));
        c.T1 = MemoryAccess.ReadWordRight(m, c.T1, (c.A0 + 0x8u));
        c.T2 = MemoryAccess.ReadWordLeft(m, c.T2, (c.A0 + 0xFu));
        c.T2 = MemoryAccess.ReadWordRight(m, c.T2, (c.A0 + 0xCu));
        c.T3 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.A0 + 0x10u));
        c.T4 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.A0 + 0x11u));
        MemoryAccess.WriteWordLeft(m, (c.V0 + 0x3u), c.T1);
        MemoryAccess.WriteWordRight(m, c.V0, c.T1);
        MemoryAccess.WriteWordLeft(m, (c.V0 + 0x7u), c.T2);
        MemoryAccess.WriteWordRight(m, (c.V0 + 0x4u), c.T2);
        MemoryAccess.WriteU8(m, (c.V0 + 0x8u), (byte)c.T3);
        MemoryAccess.WriteU8(m, (c.V0 + 0x9u), (byte)c.T4);
        c.T1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.A0 + 0x12u));
        MemoryAccess.WriteU8(m, (c.V0 + 0xAu), (byte)c.T1);
        c.V0 = c.A2 << 2;
        c.V0 = c.V0 + 0x2Du;
        c.V0 = c.T0 + c.V0;
        c.T1 = MemoryAccess.ReadWordLeft(m, c.T1, (c.A0 + 0x7u));
        c.T1 = MemoryAccess.ReadWordRight(m, c.T1, (c.A0 + 0x4u));
        MemoryAccess.WriteWordLeft(m, (c.V0 + 0x3u), c.T1);
        MemoryAccess.WriteWordRight(m, c.V0, c.T1);
        L800191FC: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019204(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        c.A0 = c.A1 + 0u;
        c.V1 = 0x00000004u;
        c.A1 = c.S0 + 0x8u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        L80019220: ;
        c.V0 = (int)c.V1 < 10 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.A1 + c.V1;
            goto L80019244;
        }
        c.V0 = c.A1 + c.V1;
        c.V0 = MemoryAccess.ReadU8(m, c.V0);
        if (c.V0 == c.A0) {
            c.V0 = c.V1 + 0u;
            goto L80019344;
        }
        c.V0 = c.V1 + 0u;
        c.V1 = c.V1 + 0x1u;
        goto L80019220;
        L80019244: ;
        c.V1 = MemoryAccess.ReadU32(m, c.S0);
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L800192E4;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L8001926C;
        }
        c.V0 = 0x00000002u;
        if (c.V1 == 0u) {
            c.V0 = 0xFFFFFFFFu;
            goto L8001927C;
        }
        c.V0 = 0xFFFFFFFFu;
        goto L80019344;
        L8001926C: ;
        if (c.V1 == c.V0) {
            c.V0 = 0xFFFFFFFFu;
            goto L80019294;
        }
        c.V0 = 0xFFFFFFFFu;
        goto L80019344;
        L8001927C: ;
        c.V0 = MemoryAccess.ReadU8(m, (c.S0 + 0xAu));
        if (c.V0 == c.A0) {
            c.V0 = 0x00000002u;
            goto L80019344;
        }
        c.V0 = 0x00000002u;
        goto L800192CC;
        L80019294: ;
        c.V0 = MemoryAccess.ReadU8(m, (c.S0 + 0xAu));
        if (c.V0 == c.A0) {
            c.V0 = 0x00000002u;
            goto L80019344;
        }
        c.V0 = 0x00000002u;
        c.V0 = MemoryAccess.ReadU8(m, (c.S0 + 0xBu));
        if (c.V0 == c.A0) {
            goto L800192DC;
        }
        c.RA = 0x800192BCu;
        GranTurismo2ArcadePC.func_80018E84(c, m);
        c.V1 = MemoryAccess.ReadU8(m, (c.S0 + 0xAu));
        c.A0 = c.V0 + 0u;
        if (c.V1 == c.A0) {
            c.V0 = 0x00000002u;
            goto L80019344;
        }
        c.V0 = 0x00000002u;
        L800192CC: ;
        c.V0 = MemoryAccess.ReadU8(m, (c.S0 + 0xBu));
        if (c.V0 != c.A0) {
            c.V0 = 0xFFFFFFFFu;
            goto L80019344;
        }
        c.V0 = 0xFFFFFFFFu;
        L800192DC: ;
        c.V0 = 0x00000003u;
        goto L80019344;
        L800192E4: ;
        c.V1 = MemoryAccess.ReadU8(m, (c.S0 + 0xAu));
        c.V0 = c.V1 & 0x00FFu;
        if (c.V0 == c.A0) {
            c.V0 = 0x00000002u;
            goto L80019344;
        }
        c.V0 = 0x00000002u;
        c.V0 = MemoryAccess.ReadU8(m, (c.S0 + 0xBu));
        if (c.V0 == c.A0) {
            c.V0 = c.V1 << 24;
            goto L800192DC;
        }
        c.V0 = c.V1 << 24;
        if ((int)c.V0 >= 0) {
            goto L80019320;
        }
        c.V0 = MemoryAccess.ReadU8(m, (c.S0 + 0x6u));
        if (c.V0 == c.A0) {
            c.V0 = 0x00000002u;
            goto L80019344;
        }
        c.V0 = 0x00000002u;
        L80019320: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0xBu));
        if ((int)c.V0 >= 0) {
            c.V0 = 0xFFFFFFFFu;
            goto L80019344;
        }
        c.V0 = 0xFFFFFFFFu;
        c.V1 = MemoryAccess.ReadU8(m, (c.S0 + 0x7u));
        if (c.V1 == c.A0) {
            c.V0 = 0x00000003u;
            goto L80019344;
        }
        c.V0 = 0x00000003u;
        c.V0 = 0xFFFFFFFFu;
        L80019344: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019354(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = c.A2 + 0u;
        c.A1 = c.S3 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.RA = 0x80019380u;
        GranTurismo2ArcadePC.func_80019204(c, m);
        c.A1 = c.V0 + 0u;
        c.V1 = MemoryAccess.ReadU32(m, c.S1);
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L80019454;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L800193AC;
        }
        if (c.V1 == 0u) {
            goto L800193C0;
        }
        goto L80019584;
        L800193AC: ;
        c.V0 = 0x00000002u;
        if (c.V1 == c.V0) {
            goto L800193DC;
        }
        goto L80019584;
        L800193C0: ;
        if ((int)c.A1 < 0) {
            c.V0 = c.S1 + 0x8u;
            goto L8001957C;
        }
        c.V0 = c.S1 + 0x8u;
        c.V1 = c.V0 + c.S2;
        c.V1 = MemoryAccess.ReadU8(m, c.V1);
        c.V0 = c.V0 + c.A1;
        MemoryAccess.WriteU8(m, c.V0, (byte)c.V1);
        goto L8001957C;
        L800193DC: ;
        if ((int)c.A1 < 0) {
            c.V0 = (int)c.A1 < 4 ? 1u : 0u;
            goto L80019428;
        }
        c.V0 = (int)c.A1 < 4 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = (int)c.A1 < 2 ? 1u : 0u;
            goto L8001940C;
        }
        c.V0 = (int)c.A1 < 2 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S0 = c.S1 + 0x8u;
            goto L8001940C;
        }
        c.S0 = c.S1 + 0x8u;
        c.V0 = c.S0 + c.S2;
        c.A0 = MemoryAccess.ReadU8(m, c.V0);
        c.S0 = c.S0 + c.A1;
        c.RA = 0x80019404u;
        GranTurismo2ArcadePC.func_80018E84(c, m);
        MemoryAccess.WriteU8(m, c.S0, (byte)c.V0);
        goto L80019428;
        L8001940C: ;
        if ((int)c.A1 < 0) {
            c.S0 = c.S1 + 0x8u;
            goto L80019428;
        }
        c.S0 = c.S1 + 0x8u;
        c.V0 = c.S0 + c.S2;
        c.A0 = MemoryAccess.ReadU8(m, c.V0);
        c.S0 = c.S0 + c.A1;
        c.RA = 0x80019424u;
        GranTurismo2ArcadePC.func_80018EBC(c, m);
        MemoryAccess.WriteU8(m, c.S0, (byte)c.V0);
        L80019428: ;
        c.V0 = (int)c.S2 < 4 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = (int)c.S2 < 2 ? 1u : 0u;
            goto L8001957C;
        }
        c.V0 = (int)c.S2 < 2 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = c.S1 + c.S2;
            goto L80019580;
        }
        c.V0 = c.S1 + c.S2;
        c.A0 = c.S3 + 0u;
        c.S0 = c.S1 + 0x8u;
        c.S0 = c.S0 + c.S2;
        c.RA = 0x8001944Cu;
        GranTurismo2ArcadePC.func_80018E84(c, m);
        MemoryAccess.WriteU8(m, c.S0, (byte)c.V0);
        goto L80019584;
        L80019454: ;
        if ((int)c.A1 < 0) {
            c.V0 = c.S1 + c.S2;
            goto L80019510;
        }
        c.V0 = c.S1 + c.S2;
        c.V1 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        c.V0 = 0x00000002u;
        if (c.S2 == c.V0) {
            c.V0 = 0x00000003u;
            goto L8001947C;
        }
        c.V0 = 0x00000003u;
        if (c.S2 == c.V0) {
            c.V0 = 0x00000002u;
            goto L80019498;
        }
        c.V0 = 0x00000002u;
        goto L800194AC;
        L8001947C: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0xAu));
        if ((int)c.V0 >= 0) {
            c.V0 = 0x00000002u;
            goto L800194AC;
        }
        c.V0 = 0x00000002u;
        c.V1 = MemoryAccess.ReadU8(m, (c.S1 + 0x6u));
        goto L800194AC;
        L80019498: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0xBu));
        if ((int)c.V0 >= 0) {
            c.V0 = 0x00000002u;
            goto L800194AC;
        }
        c.V0 = 0x00000002u;
        c.V1 = MemoryAccess.ReadU8(m, (c.S1 + 0x7u));
        L800194AC: ;
        if (c.A1 == c.V0) {
            c.V0 = 0x00000003u;
            goto L800194C4;
        }
        c.V0 = 0x00000003u;
        if (c.A1 == c.V0) {
            goto L800194E4;
        }
        goto L80019504;
        L800194C4: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0xAu));
        if ((int)c.V0 >= 0) {
            goto L800194DC;
        }
        MemoryAccess.WriteU8(m, (c.S1 + 0x6u), (byte)c.V1);
        goto L80019510;
        L800194DC: ;
        MemoryAccess.WriteU8(m, (c.S1 + 0xAu), (byte)c.V1);
        goto L80019510;
        L800194E4: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0xBu));
        if ((int)c.V0 >= 0) {
            goto L800194FC;
        }
        MemoryAccess.WriteU8(m, (c.S1 + 0x7u), (byte)c.V1);
        goto L80019510;
        L800194FC: ;
        MemoryAccess.WriteU8(m, (c.S1 + 0xBu), (byte)c.V1);
        goto L80019510;
        L80019504: ;
        if ((int)c.A1 < 0) {
            c.V0 = c.S1 + c.A1;
            goto L80019510;
        }
        c.V0 = c.S1 + c.A1;
        MemoryAccess.WriteU8(m, (c.V0 + 0x8u), (byte)c.V1);
        L80019510: ;
        c.V0 = 0x00000002u;
        if (c.S2 == c.V0) {
            c.V0 = 0x00000003u;
            goto L8001952C;
        }
        c.V0 = 0x00000003u;
        if (c.S2 == c.V0) {
            c.V0 = c.S1 + c.S2;
            goto L80019554;
        }
        c.V0 = c.S1 + c.S2;
        MemoryAccess.WriteU8(m, (c.V0 + 0x8u), (byte)c.S3);
        goto L80019584;
        L8001952C: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0xBu));
        if ((int)c.V0 >= 0) {
            c.V1 = c.S1 + 0x4u;
            goto L8001954C;
        }
        c.V1 = c.S1 + 0x4u;
        c.V0 = MemoryAccess.ReadU8(m, (c.V1 + 0x3u));
        MemoryAccess.WriteU8(m, (c.S1 + 0xBu), (byte)c.V0);
        MemoryAccess.WriteU8(m, (c.V1 + 0x1u), (byte)0u);
        L8001954C: ;
        MemoryAccess.WriteU8(m, (c.S1 + 0xAu), (byte)c.S3);
        goto L80019584;
        L80019554: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0xAu));
        if ((int)c.V0 >= 0) {
            c.V1 = c.S1 + 0x4u;
            goto L80019574;
        }
        c.V1 = c.S1 + 0x4u;
        c.V0 = MemoryAccess.ReadU8(m, (c.V1 + 0x2u));
        MemoryAccess.WriteU8(m, (c.S1 + 0xAu), (byte)c.V0);
        MemoryAccess.WriteU8(m, (c.V1 + 0x1u), (byte)0u);
        L80019574: ;
        MemoryAccess.WriteU8(m, (c.S1 + 0xBu), (byte)c.S3);
        goto L80019584;
        L8001957C: ;
        c.V0 = c.S1 + c.S2;
        L80019580: ;
        MemoryAccess.WriteU8(m, (c.V0 + 0x8u), (byte)c.S3);
        L80019584: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800195A0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.V1 = 0x00010000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.V0 = MemoryAccess.ReadU32(m, c.A1);
        c.A0 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.V0 = c.V0 & c.V1;
        if (c.V0 == 0u) {
            c.S2 = 0x00000001u;
            goto L800195EC;
        }
        c.S2 = 0x00000001u;
        c.V0 = 0xFFFE0000u;
        c.V0 = c.V0 | 0xFFFFu;
        c.V0 = c.A0 & c.V0;
        if (c.V0 == 0u) {
            c.V0 = 0u + 0u;
            goto L80019778;
        }
        c.V0 = 0u + 0u;
        goto L800196CC;
        L800195EC: ;
        c.V1 = MemoryAccess.ReadU32(m, c.S1);
        if (c.V1 == c.S2) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L800196BC;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L80019614;
        }
        c.V0 = 0x00000002u;
        if (c.V1 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L80019624;
        }
        c.V0 = c.S2 + 0u;
        goto L80019778;
        L80019614: ;
        if (c.V1 == c.V0) {
            c.V0 = c.S2 + 0u;
            goto L80019644;
        }
        c.V0 = c.S2 + 0u;
        goto L80019778;
        L80019624: ;
        c.V0 = 0xFFFFFFFCu;
        c.V0 = c.A0 & c.V0;
        if (c.V0 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L80019778;
        }
        c.V0 = c.S2 + 0u;
        c.A0 = 0u + 0u;
        c.RA = 0x8001963Cu;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.S2 = 0u + 0u;
        goto L80019774;
        L80019644: ;
        c.V0 = 0xFFFFFFF0u;
        c.V0 = c.A0 & c.V0;
        if (c.V0 != 0u) {
            c.V0 = c.A0 & 0x0004u;
            goto L800196CC;
        }
        c.V0 = c.A0 & 0x0004u;
        c.S0 = MemoryAccess.ReadU8(m, (c.S1 + 0x4u));
        if (c.V0 == 0u) {
            c.V0 = c.A0 & 0x0008u;
            goto L80019670;
        }
        c.V0 = c.A0 & 0x0008u;
        c.S0 = c.S0 - 0x1u;
        if ((int)c.S0 >= 0) {
            goto L80019670;
        }
        c.S0 = 0x00000001u;
        L80019670: ;
        if (c.V0 == 0u) {
            goto L8001968C;
        }
        c.S0 = c.S0 + 0x1u;
        c.V0 = (int)c.S0 < 2 ? 1u : 0u;
        if (c.V0 != 0u) {
            goto L8001968C;
        }
        c.S0 = 0u + 0u;
        L8001968C: ;
        c.V0 = MemoryAccess.ReadU8(m, (c.S1 + 0x4u));
        if (c.S0 == c.V0) {
            c.V0 = c.S2 + 0u;
            goto L80019778;
        }
        c.V0 = c.S2 + 0u;
        c.S2 = 0u + 0u;
        c.A0 = 0x00000005u;
        c.RA = 0x800196A8u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        if (c.S0 != 0u) {
            MemoryAccess.WriteU8(m, (c.S1 + 0x4u), (byte)c.S0);
            goto L80019764;
        }
        MemoryAccess.WriteU8(m, (c.S1 + 0x4u), (byte)c.S0);
        c.V0 = 0x00000080u;
        MemoryAccess.WriteU8(m, (c.S1 + 0x8u), (byte)c.V0);
        goto L80019770;
        L800196BC: ;
        c.V0 = 0xFFFFFFF0u;
        c.V0 = c.A0 & c.V0;
        if (c.V0 == 0u) {
            c.V0 = c.A0 & 0x0004u;
            goto L800196DC;
        }
        c.V0 = c.A0 & 0x0004u;
        L800196CC: ;
        c.A0 = 0u + 0u;
        c.RA = 0x800196D4u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.V0 = 0u + 0u;
        goto L80019778;
        L800196DC: ;
        c.S0 = MemoryAccess.ReadU8(m, (c.S1 + 0x4u));
        if (c.V0 == 0u) {
            c.V0 = c.A0 & 0x0008u;
            goto L800196F8;
        }
        c.V0 = c.A0 & 0x0008u;
        c.S0 = c.S0 - 0x1u;
        if ((int)c.S0 >= 0) {
            goto L800196F8;
        }
        c.S0 = 0x00000002u;
        L800196F8: ;
        if (c.V0 == 0u) {
            goto L80019714;
        }
        c.S0 = c.S0 + 0x1u;
        c.V0 = (int)c.S0 < 3 ? 1u : 0u;
        if (c.V0 != 0u) {
            goto L80019714;
        }
        c.S0 = 0u + 0u;
        L80019714: ;
        c.V0 = MemoryAccess.ReadU8(m, (c.S1 + 0x4u));
        if (c.S0 == c.V0) {
            c.V0 = c.S2 + 0u;
            goto L80019778;
        }
        c.V0 = c.S2 + 0u;
        c.S2 = 0u + 0u;
        c.A0 = 0x00000005u;
        c.RA = 0x80019730u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        if (c.S0 == 0u) {
            MemoryAccess.WriteU8(m, (c.S1 + 0x4u), (byte)c.S0);
            goto L8001974C;
        }
        MemoryAccess.WriteU8(m, (c.S1 + 0x4u), (byte)c.S0);
        c.V0 = 0x00000001u;
        if (c.S0 == c.V0) {
            c.V0 = 0x00000002u;
            goto L80019758;
        }
        c.V0 = 0x00000002u;
        MemoryAccess.WriteU8(m, (c.S1 + 0x8u), (byte)c.V0);
        goto L8001976C;
        L8001974C: ;
        c.V0 = 0x00000082u;
        MemoryAccess.WriteU8(m, (c.S1 + 0x8u), (byte)c.V0);
        goto L80019770;
        L80019758: ;
        c.V0 = 0x00000080u;
        MemoryAccess.WriteU8(m, (c.S1 + 0x8u), (byte)c.V0);
        goto L80019770;
        L80019764: ;
        c.V0 = 0x00000002u;
        MemoryAccess.WriteU8(m, (c.S1 + 0x8u), (byte)c.V0);
        L8001976C: ;
        c.V0 = 0x00000003u;
        L80019770: ;
        MemoryAccess.WriteU8(m, (c.S1 + 0x9u), (byte)c.V0);
        L80019774: ;
        c.V0 = c.S2 + 0u;
        L80019778: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019790(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.V0 = MemoryAccess.ReadU32(m, c.A1);
        c.V1 = 0x00010000u;
        c.V0 = c.V0 & c.V1;
        c.V1 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        if (c.V0 != 0u) {
            c.A0 = 0x00000001u;
            goto L8001989C;
        }
        c.A0 = 0x00000001u;
        c.V0 = MemoryAccess.ReadU32(m, c.S2);
        if (c.V0 != c.A0) {
            c.V0 = c.A0 + 0u;
            goto L800198A0;
        }
        c.V0 = c.A0 + 0u;
        c.V0 = c.V1 & 0x000Cu;
        if (c.V0 == 0u) {
            c.V0 = c.V1 & 0x0004u;
            goto L8001989C;
        }
        c.V0 = c.V1 & 0x0004u;
        c.S1 = MemoryAccess.ReadU8(m, (c.S2 + 0x5u));
        if (c.V0 == 0u) {
            c.V0 = c.V1 & 0x0008u;
            goto L800197F8;
        }
        c.V0 = c.V1 & 0x0008u;
        c.S1 = c.S1 - 0x1u;
        if ((int)c.S1 >= 0) {
            goto L800197F8;
        }
        c.S1 = 0x00000008u;
        L800197F8: ;
        if (c.V0 == 0u) {
            c.S0 = c.S2 + 0x4u;
            goto L80019814;
        }
        c.S0 = c.S2 + 0x4u;
        c.S1 = c.S1 + 0x1u;
        c.V0 = (int)c.S1 < 9 ? 1u : 0u;
        if (c.V0 != 0u) {
            goto L80019814;
        }
        c.S1 = 0u + 0u;
        L80019814: ;
        c.V0 = MemoryAccess.ReadU8(m, (c.S0 + 0x1u));
        if (c.S1 == c.V0) {
            c.V0 = c.A0 + 0u;
            goto L800198A0;
        }
        c.V0 = c.A0 + 0u;
        c.A0 = 0x00000005u;
        c.RA = 0x8001982Cu;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.S0 + 0x1u));
        if (c.V0 != 0u) {
            c.A0 = 0u + 0u;
            goto L80019854;
        }
        c.A0 = 0u + 0u;
        c.V0 = MemoryAccess.ReadU8(m, (c.S2 + 0xAu));
        MemoryAccess.WriteU8(m, (c.S0 + 0x2u), (byte)c.V0);
        c.V0 = MemoryAccess.ReadU8(m, (c.S2 + 0xBu));
        MemoryAccess.WriteU8(m, (c.S0 + 0x3u), (byte)c.V0);
        L80019854: ;
        if (c.S1 != 0u) {
            MemoryAccess.WriteU8(m, (c.S0 + 0x1u), (byte)c.S1);
            goto L80019874;
        }
        MemoryAccess.WriteU8(m, (c.S0 + 0x1u), (byte)c.S1);
        c.V0 = MemoryAccess.ReadU8(m, (c.S0 + 0x2u));
        MemoryAccess.WriteU8(m, (c.S2 + 0xAu), (byte)c.V0);
        c.V0 = MemoryAccess.ReadU8(m, (c.S0 + 0x3u));
        MemoryAccess.WriteU8(m, (c.S2 + 0xBu), (byte)c.V0);
        goto L8001989C;
        L80019874: ;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 - 0x4A0Cu;
        c.V1 = c.S1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU8(m, c.V1);
        MemoryAccess.WriteU8(m, (c.S2 + 0xAu), (byte)c.V0);
        c.V0 = MemoryAccess.ReadU8(m, (c.V1 + 0x2u));
        MemoryAccess.WriteU8(m, (c.S2 + 0xBu), (byte)c.V0);
        L8001989C: ;
        c.V0 = c.A0 + 0u;
        L800198A0: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800198B8(CpuContext c, IMemory m)
    {
        c.V1 = MemoryAccess.ReadU32(m, c.A0);
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            c.A1 = c.V1 + 0u;
            goto L8001997C;
        }
        c.A1 = c.V1 + 0u;
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L800198E4;
        }
        if (c.V1 == 0u) {
            c.V0 = 0x80090000u;
            goto L800198F8;
        }
        c.V0 = 0x80090000u;
        return;
        L800198E4: ;
        c.V0 = 0x00000002u;
        if (c.A1 == c.V0) {
            c.V0 = 0x80090000u;
            goto L80019938;
        }
        c.V0 = 0x80090000u;
        return;
        L800198F8: ;
        c.T1 = c.V0 + 0x1268u;
        c.A2 = MemoryAccess.ReadWordLeft(m, c.A2, (c.T1 + 0x3u));
        c.A2 = MemoryAccess.ReadWordRight(m, c.A2, c.T1);
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.T1 + 0x7u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, (c.T1 + 0x4u));
        c.T0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.T1 + 0x8u));
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0xBu), c.A2);
        MemoryAccess.WriteWordRight(m, (c.A0 + 0x8u), c.A2);
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0xFu), c.A3);
        MemoryAccess.WriteWordRight(m, (c.A0 + 0xCu), c.A3);
        MemoryAccess.WriteU8(m, (c.A0 + 0x10u), (byte)c.T0);
        c.A2 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.T1 + 0x9u));
        c.A3 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.T1 + 0xAu));
        MemoryAccess.WriteU8(m, (c.A0 + 0x11u), (byte)c.A2);
        MemoryAccess.WriteU8(m, (c.A0 + 0x12u), (byte)c.A3);
        return;
        L80019938: ;
        c.T1 = c.V0 + 0x1280u;
        c.A2 = MemoryAccess.ReadWordLeft(m, c.A2, (c.T1 + 0x3u));
        c.A2 = MemoryAccess.ReadWordRight(m, c.A2, c.T1);
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.T1 + 0x7u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, (c.T1 + 0x4u));
        c.T0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.T1 + 0x8u));
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0xBu), c.A2);
        MemoryAccess.WriteWordRight(m, (c.A0 + 0x8u), c.A2);
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0xFu), c.A3);
        MemoryAccess.WriteWordRight(m, (c.A0 + 0xCu), c.A3);
        MemoryAccess.WriteU8(m, (c.A0 + 0x10u), (byte)c.T0);
        c.A2 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.T1 + 0x9u));
        c.A3 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.T1 + 0xAu));
        MemoryAccess.WriteU8(m, (c.A0 + 0x11u), (byte)c.A2);
        MemoryAccess.WriteU8(m, (c.A0 + 0x12u), (byte)c.A3);
        MemoryAccess.WriteU8(m, (c.A0 + 0x4u), (byte)0u);
        return;
        L8001997C: ;
        c.V0 = 0x80090000u;
        c.T1 = c.V0 + 0x1274u;
        c.A2 = MemoryAccess.ReadWordLeft(m, c.A2, (c.T1 + 0x3u));
        c.A2 = MemoryAccess.ReadWordRight(m, c.A2, c.T1);
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.T1 + 0x7u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, (c.T1 + 0x4u));
        c.T0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.T1 + 0x8u));
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0xBu), c.A2);
        MemoryAccess.WriteWordRight(m, (c.A0 + 0x8u), c.A2);
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0xFu), c.A3);
        MemoryAccess.WriteWordRight(m, (c.A0 + 0xCu), c.A3);
        MemoryAccess.WriteU8(m, (c.A0 + 0x10u), (byte)c.T0);
        c.A2 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.T1 + 0x9u));
        c.A3 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.T1 + 0xAu));
        MemoryAccess.WriteU8(m, (c.A0 + 0x11u), (byte)c.A2);
        MemoryAccess.WriteU8(m, (c.A0 + 0x12u), (byte)c.A3);
        MemoryAccess.WriteU8(m, (c.A0 + 0x4u), (byte)0u);
        MemoryAccess.WriteU8(m, (c.A0 + 0x5u), (byte)0u);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800199C8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x30u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S5);
        c.S5 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        c.V0 = 0x80050000u;
        c.V0 = c.V0 - 0x4A50u;
        c.A2 = c.A2 << 1;
        c.A2 = c.A2 + c.V0;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S2 = (uint)(short)MemoryAccess.ReadU16(m, c.A2);
        c.S1 = MemoryAccess.ReadU32(m, (c.S3 + 0x4u));
        if (c.S2 == c.S0) {
            c.S4 = 0u + 0u;
            goto L80019A4C;
        }
        c.S4 = 0u + 0u;
        if ((int)c.S2 >= 0) {
            c.V0 = 0xFFFFFFFEu;
            goto L80019A2C;
        }
        c.V0 = 0xFFFFFFFEu;
        if (c.S2 == c.V0) {
            c.V0 = c.S1 & 0x0A00u;
            goto L80019A3C;
        }
        c.V0 = c.S1 & 0x0A00u;
        goto L80019A88;
        L80019A2C: ;
        if (c.S2 == 0u) {
            c.A0 = c.S5 + 0u;
            goto L80019A70;
        }
        c.A0 = c.S5 + 0u;
        goto L80019A88;
        L80019A3C: ;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L80019B88;
        }
        c.V0 = 0x00000002u;
        goto L80019B8C;
        L80019A4C: ;
        c.V0 = c.S1 & 0x0A00u;
        if (c.V0 == 0u) {
            c.V0 = c.S4 + 0u;
            goto L80019B8C;
        }
        c.V0 = c.S4 + 0u;
        c.A0 = c.S5 + 0u;
        c.RA = 0x80019A60u;
        GranTurismo2ArcadePC.func_800198B8(c, m);
        c.A0 = 0x00000001u;
        c.RA = 0x80019A68u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.V0 = 0x00000001u;
        goto L80019B8C;
        L80019A70: ;
        c.A1 = c.S3 + 0u;
        c.RA = 0x80019A78u;
        GranTurismo2ArcadePC.func_800195A0(c, m);
        if (c.V0 != 0u) {
            c.V0 = c.S4 + 0u;
            goto L80019B8C;
        }
        c.V0 = c.S4 + 0u;
        c.S4 = 0x00000001u;
        goto L80019B88;
        L80019A88: ;
        c.V1 = MemoryAccess.ReadU32(m, c.S5);
        c.V0 = 0x00000001u;
        if (c.V1 != c.V0) {
            c.V0 = (int)c.S2 < 4 ? 1u : 0u;
            goto L80019AB8;
        }
        c.V0 = (int)c.S2 < 4 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = (int)c.S2 < 2 ? 1u : 0u;
            goto L80019AB8;
        }
        c.V0 = (int)c.S2 < 2 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.A0 = c.S5 + 0u;
            goto L80019AB8;
        }
        c.A0 = c.S5 + 0u;
        c.A1 = c.S3 + 0u;
        c.RA = 0x80019AB0u;
        GranTurismo2ArcadePC.func_80019790(c, m);
        if (c.V0 == 0u) {
            c.V0 = 0x00000001u;
            goto L80019B8C;
        }
        c.V0 = 0x00000001u;
        L80019AB8: ;
        c.V0 = MemoryAccess.ReadU32(m, c.S3);
        c.V1 = 0x00010000u;
        c.V0 = c.V0 & c.V1;
        if (c.V0 == 0u) {
            c.V0 = c.S1 & 0x0001u;
            goto L80019B00;
        }
        c.V0 = c.S1 & 0x0001u;
        if (c.V0 == 0u) {
            c.V0 = c.S1 & 0x0002u;
            goto L80019AD8;
        }
        c.V0 = c.S1 & 0x0002u;
        c.S0 = 0u + 0u;
        L80019AD8: ;
        if (c.V0 == 0u) {
            c.V0 = c.S1 & 0x0004u;
            goto L80019AE4;
        }
        c.V0 = c.S1 & 0x0004u;
        c.S0 = 0x00000001u;
        L80019AE4: ;
        if (c.V0 == 0u) {
            c.V0 = c.S1 & 0x0008u;
            goto L80019AF0;
        }
        c.V0 = c.S1 & 0x0008u;
        c.S0 = 0x00000002u;
        L80019AF0: ;
        if (c.V0 == 0u) {
            goto L80019B64;
        }
        c.S0 = 0x00000003u;
        goto L80019B64;
        L80019B00: ;
        c.V0 = c.S1 & 0x0010u;
        if (c.V0 == 0u) {
            c.V0 = c.S1 & 0x0020u;
            goto L80019B10;
        }
        c.V0 = c.S1 & 0x0020u;
        c.S0 = 0x00000004u;
        L80019B10: ;
        if (c.V0 == 0u) {
            c.V0 = c.S1 & 0x1000u;
            goto L80019B1C;
        }
        c.V0 = c.S1 & 0x1000u;
        c.S0 = 0x00000005u;
        L80019B1C: ;
        if (c.V0 == 0u) {
            c.V0 = c.S1 & 0x2000u;
            goto L80019B28;
        }
        c.V0 = c.S1 & 0x2000u;
        c.S0 = 0x0000000Cu;
        L80019B28: ;
        if (c.V0 == 0u) {
            c.V0 = c.S1 & 0x0200u;
            goto L80019B34;
        }
        c.V0 = c.S1 & 0x0200u;
        c.S0 = 0x0000000Du;
        L80019B34: ;
        if (c.V0 == 0u) {
            c.V0 = c.S1 & 0x0100u;
            goto L80019B40;
        }
        c.V0 = c.S1 & 0x0100u;
        c.S0 = 0x00000009u;
        L80019B40: ;
        if (c.V0 == 0u) {
            c.V0 = c.S1 & 0x0400u;
            goto L80019B4C;
        }
        c.V0 = c.S1 & 0x0400u;
        c.S0 = 0x00000008u;
        L80019B4C: ;
        if (c.V0 == 0u) {
            c.V0 = c.S1 & 0x0800u;
            goto L80019B58;
        }
        c.V0 = c.S1 & 0x0800u;
        c.S0 = 0x0000000Au;
        L80019B58: ;
        if (c.V0 == 0u) {
            goto L80019B64;
        }
        c.S0 = 0x0000000Bu;
        L80019B64: ;
        if ((int)c.S0 < 0) {
            c.V0 = c.S4 + 0u;
            goto L80019B8C;
        }
        c.V0 = c.S4 + 0u;
        c.A0 = 0x00000001u;
        c.RA = 0x80019B74u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.A0 = c.S5 + 0u;
        c.A1 = c.S2 + 0u;
        c.A2 = c.S0 + 0u;
        c.RA = 0x80019B84u;
        GranTurismo2ArcadePC.func_80019354(c, m);
        c.S4 = 0x00000001u;
        L80019B88: ;
        c.V0 = c.S4 + 0u;
        L80019B8C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x30u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019BB0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x70u;
        MemoryAccess.WriteU32(m, (c.SP + 0x58u), c.S4);
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x80u));
        MemoryAccess.WriteU32(m, (c.SP + 0x4Cu), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x5Cu), c.S5);
        c.S5 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x68u), c.FP);
        c.FP = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x50u), c.S2);
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x84u));
        c.A0 = c.SP + 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x60u), c.S6);
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x88u));
        c.A1 = 0x0000001Eu;
        MemoryAccess.WriteU32(m, (c.SP + 0x6Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x64u), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x54u), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x48u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x7Cu), c.A3);
        c.RA = 0x80019C04u;
        GranTurismo2ArcadePC.func_8006AB78(c, m);
        c.V1 = 0xFF9F0000u;
        c.V1 = c.V1 | 0xFFFFu;
        c.S0 = c.SP + 0x20u;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6D40u;
        c.T0 = 0x80050000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0xCu));
        c.S3 = c.T0 - 0x4880u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x10u), c.S5);
        c.V0 = c.V0 & c.V1;
        c.V1 = 0x00200000u;
        c.V0 = c.V0 | c.V1;
        MemoryAccess.WriteU32(m, (c.S0 + 0xCu), c.V0);
        c.RA = 0x80019C40u;
        GranTurismo2ArcadePC.func_8007D990(c, m);
        c.V0 = 0x00140000u;
        c.V0 = c.V0 | 0x2864u;
        c.A1 = 0x00000030u;
        c.S7 = 0x00000001u;
        if (c.S6 != c.S2) {
            MemoryAccess.WriteU32(m, (c.S0 + 0x14u), c.V0);
            goto L80019C5C;
        }
        MemoryAccess.WriteU32(m, (c.S0 + 0x14u), c.V0);
        c.A1 = 0x00000080u;
        L80019C5C: ;
        if ((int)c.S6 >= 0) {
            { var _r = (long)(int)c.S4 * (int)c.A1; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
            goto L80019C6C;
        }
        { var _r = (long)(int)c.S4 * (int)c.A1; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.A1 = 0x00000080u;
        { var _r = (long)(int)c.S4 * (int)c.A1; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        L80019C6C: ;
        c.V1 = MemoryAccess.ReadU32(m, c.S1);
        c.V0 = 0x00000003u;
        c.T0 = c.LO;
        if (c.V1 == c.V0) {
            c.S4 = (uint)((int)c.T0 >> 7);
            goto L8001A260;
        }
        c.S4 = (uint)((int)c.T0 >> 7);
        c.V0 = 0x00000009u;
        if (c.S2 == c.V0) {
            c.V0 = (int)c.S2 < 10 ? 1u : 0u;
            goto L80019CF4;
        }
        c.V0 = (int)c.S2 < 10 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x0000000Au;
            goto L80019CA4;
        }
        c.V0 = 0x0000000Au;
        if (c.S2 == 0u) {
            c.A1 = 0u + 0u;
            goto L80019D50;
        }
        c.A1 = 0u + 0u;
        c.V1 = c.S2 - 0x1u;
        goto L80019FDC;
        L80019CA4: ;
        if (c.S2 != c.V0) {
            c.A1 = 0u + 0u;
            goto L80019FD8;
        }
        c.A1 = 0u + 0u;
        if ((int)c.S6 < 0) {
            c.A0 = c.S0 + 0u;
            goto L8001A260;
        }
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6D30u;
        c.RA = 0x80019CC0u;
        GranTurismo2ArcadePC.func_8007D990(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4A5Cu;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x4A54u;
        c.A2 = c.S4 + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x80019CDCu;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x61FAu;
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0x7Cu));
        c.A2 = c.FP + 0u;
        goto L80019D3C;
        L80019CF4: ;
        if ((int)c.S6 < 0) {
            c.A0 = c.S0 + 0u;
            goto L8001A260;
        }
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6D30u;
        c.RA = 0x80019D08u;
        GranTurismo2ArcadePC.func_8007D990(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4A5Cu;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x4A54u;
        c.A2 = c.S4 + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x80019D24u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x61F5u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x7Cu));
        c.A2 = c.FP + 0u;
        c.A3 = c.T0 + 0x6u;
        L80019D3C: ;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S7);
        c.RA = 0x80019D48u;
        GranTurismo2ArcadePC.func_8006ACC4(c, m);
        goto L8001A260;
        L80019D50: ;
        if (c.V1 == c.S7) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L80019ECC;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L80019D70;
        }
        if (c.V1 == 0u) {
            c.A0 = c.S5 + 0u;
            goto L80019D84;
        }
        c.A0 = c.S5 + 0u;
        goto L8001A260;
        L80019D70: ;
        c.V0 = 0x00000002u;
        if (c.V1 == c.V0) {
            goto L80019DDC;
        }
        goto L8001A260;
        L80019D84: ;
        c.A1 = c.S4 << 8;
        c.A1 = c.S4 | c.A1;
        c.V0 = c.S4 << 16;
        c.A1 = c.A1 | c.V0;
        c.RA = 0x80019D98u;
        GranTurismo2ArcadePC.func_80081388(c, m);
        c.A0 = c.V0 + 0u;
        c.V1 = MemoryAccess.ReadU16(m, (c.S3 + 0x4u));
        c.V0 = MemoryAccess.ReadU16(m, (c.S3 + 0x6u));
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x7Cu));
        c.V1 = c.V1 << 16;
        c.V1 = (uint)((int)c.V1 >> 17);
        c.V1 = c.FP - c.V1;
        c.V0 = c.V0 << 16;
        c.V0 = (uint)((int)c.V0 >> 17);
        c.V0 = c.T0 - c.V0;
        c.V0 = c.V0 << 16;
        c.V1 = c.V1 + c.V0;
        c.T0 = 0x80050000u;
        MemoryAccess.WriteU32(m, c.A0, c.V1);
        c.V0 = MemoryAccess.ReadU32(m, (c.T0 - 0x4880u));
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.V0);
        goto L8001A240;
        L80019DDC: ;
        c.V0 = MemoryAccess.ReadU8(m, (c.S1 + 0x4u));
        if (c.V0 != 0u) {
            c.A0 = c.S5 + 0u;
            goto L80019DF4;
        }
        c.A0 = c.S5 + 0u;
        c.V0 = 0x80050000u;
        c.S3 = c.V0 - 0x485Cu;
        L80019DF4: ;
        c.A1 = c.S4 << 8;
        c.A1 = c.S4 | c.A1;
        c.V0 = c.S4 << 16;
        c.A1 = c.A1 | c.V0;
        c.RA = 0x80019E08u;
        GranTurismo2ArcadePC.func_80081388(c, m);
        c.A0 = c.V0 + 0u;
        c.V1 = MemoryAccess.ReadU16(m, (c.S3 + 0x4u));
        c.V0 = MemoryAccess.ReadU16(m, (c.S3 + 0x6u));
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x7Cu));
        c.V1 = c.V1 << 16;
        c.V1 = (uint)((int)c.V1 >> 17);
        c.V1 = c.FP - c.V1;
        c.V0 = c.V0 << 16;
        c.V0 = (uint)((int)c.V0 >> 17);
        c.V0 = c.T0 - c.V0;
        c.V0 = c.V0 << 16;
        c.V1 = c.V1 + c.V0;
        MemoryAccess.WriteU32(m, c.A0, c.V1);
        c.V0 = MemoryAccess.ReadU32(m, c.S3);
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.S3 + 0x4u));
        MemoryAccess.WriteU32(m, (c.A0 + 0x8u), c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S3 + 0x8u));
        c.A0 = c.S5 + 0u;
        c.A1 = c.A1 | 0x0020u;
        c.RA = 0x80019E64u;
        GranTurismo2ArcadePC.func_8007D954(c, m);
        if (c.S6 != 0u) {
            c.S1 = c.SP + 0x40u;
            goto L8001A260;
        }
        c.S1 = c.SP + 0x40u;
        c.A0 = c.S1 + 0u;
        c.A1 = c.S5 + 0u;
        c.A2 = c.FP + 0x10u;
        c.V0 = 0x00000006u;
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0x7Cu));
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x8Cu));
        c.S0 = 0x0000000Au;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.T0);
        c.RA = 0x80019E98u;
        GranTurismo2ArcadePC.func_8006B958(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S5 + 0u;
        c.A2 = c.FP - 0x10u;
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0x7Cu));
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x8Cu));
        c.V0 = 0xFFFFFFFAu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.T0);
        c.RA = 0x80019EC0u;
        GranTurismo2ArcadePC.func_8006B958(c, m);
        c.A0 = c.S5 + 0u;
        c.A1 = 0x00000020u;
        goto L8001A258;
        L80019ECC: ;
        c.A0 = MemoryAccess.ReadU8(m, (c.S1 + 0x4u));
        if (c.A0 == 0u) {
            c.V0 = 0x80050000u;
            goto L80019EEC;
        }
        c.V0 = 0x80050000u;
        if (c.A0 == c.V1) {
            c.A0 = c.S5 + 0u;
            goto L80019EF4;
        }
        c.A0 = c.S5 + 0u;
        c.A1 = c.S4 << 8;
        goto L80019F04;
        L80019EEC: ;
        c.S3 = c.V0 - 0x4874u;
        goto L80019EFC;
        L80019EF4: ;
        c.V0 = 0x80050000u;
        c.S3 = c.V0 - 0x4868u;
        L80019EFC: ;
        c.A0 = c.S5 + 0u;
        c.A1 = c.S4 << 8;
        L80019F04: ;
        c.A1 = c.S4 | c.A1;
        c.V0 = c.S4 << 16;
        c.A1 = c.A1 | c.V0;
        c.RA = 0x80019F14u;
        GranTurismo2ArcadePC.func_80081388(c, m);
        c.A0 = c.V0 + 0u;
        c.V1 = MemoryAccess.ReadU16(m, (c.S3 + 0x4u));
        c.V0 = MemoryAccess.ReadU16(m, (c.S3 + 0x6u));
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x7Cu));
        c.V1 = c.V1 << 16;
        c.V1 = (uint)((int)c.V1 >> 17);
        c.V1 = c.FP - c.V1;
        c.V0 = c.V0 << 16;
        c.V0 = (uint)((int)c.V0 >> 17);
        c.V0 = c.T0 - c.V0;
        c.V0 = c.V0 << 16;
        c.V1 = c.V1 + c.V0;
        MemoryAccess.WriteU32(m, c.A0, c.V1);
        c.V0 = MemoryAccess.ReadU32(m, c.S3);
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.S3 + 0x4u));
        MemoryAccess.WriteU32(m, (c.A0 + 0x8u), c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S3 + 0x8u));
        c.A0 = c.S5 + 0u;
        c.A1 = c.A1 | 0x0020u;
        c.RA = 0x80019F70u;
        GranTurismo2ArcadePC.func_8007D954(c, m);
        if (c.S6 != 0u) {
            c.S1 = c.SP + 0x40u;
            goto L8001A260;
        }
        c.S1 = c.SP + 0x40u;
        c.A0 = c.S1 + 0u;
        c.A1 = c.S5 + 0u;
        c.A2 = c.FP + 0x10u;
        c.V0 = 0x00000006u;
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0x7Cu));
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x8Cu));
        c.S0 = 0x0000000Au;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.T0);
        c.RA = 0x80019FA4u;
        GranTurismo2ArcadePC.func_8006B958(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S5 + 0u;
        c.A2 = c.FP - 0x10u;
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0x7Cu));
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x8Cu));
        c.V0 = 0xFFFFFFFAu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.T0);
        c.RA = 0x80019FCCu;
        GranTurismo2ArcadePC.func_8006B958(c, m);
        c.A0 = c.S5 + 0u;
        c.A1 = 0x00000020u;
        goto L8001A258;
        L80019FD8: ;
        c.V1 = c.S2 - 0x1u;
        L80019FDC: ;
        c.V0 = c.V1 < 0x00000008u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80020000u;
            goto L8001A05C;
        }
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x2458u;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x8001A004u: goto L8001A004;
            case 0x8001A010u: goto L8001A010;
            case 0x8001A01Cu: goto L8001A01C;
            case 0x8001A028u: goto L8001A028;
            case 0x8001A034u: goto L8001A034;
            case 0x8001A040u: goto L8001A040;
            case 0x8001A04Cu: goto L8001A04C;
            case 0x8001A058u: goto L8001A058;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L8001A004: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.S1 + 0xAu));
        goto L8001A05C;
        L8001A010: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.S1 + 0xBu));
        goto L8001A05C;
        L8001A01C: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.S1 + 0xDu));
        goto L8001A05C;
        L8001A028: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.S1 + 0xCu));
        goto L8001A05C;
        L8001A034: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.S1 + 0xEu));
        goto L8001A05C;
        L8001A040: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.S1 + 0xFu));
        goto L8001A05C;
        L8001A04C: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.S1 + 0x11u));
        goto L8001A05C;
        L8001A058: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.S1 + 0x10u));
        L8001A05C: ;
        c.A0 = MemoryAccess.ReadU32(m, c.S1);
        c.V0 = 0x00000001u;
        if (c.A0 == c.V0) {
            c.V1 = c.A0 + 0u;
            goto L8001A0CC;
        }
        c.V1 = c.A0 + 0u;
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L8001A088;
        }
        if (c.A0 == 0u) {
            c.A0 = c.S5 + 0u;
            goto L8001A09C;
        }
        c.A0 = c.S5 + 0u;
        c.A1 = c.S4 << 8;
        goto L8001A1F0;
        L8001A088: ;
        c.V0 = 0x00000002u;
        if (c.V1 == c.V0) {
            c.A0 = c.S5 + 0u;
            goto L8001A1B0;
        }
        c.A0 = c.S5 + 0u;
        c.A1 = c.S4 << 8;
        goto L8001A1F0;
        L8001A09C: ;
        c.V1 = 0x80050000u;
        c.V1 = c.V1 - 0x4850u;
        c.V0 = c.A1 << 1;
        c.V0 = c.V0 + c.V1;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.V0);
        c.V1 = c.V0 << 1;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 2;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 - 0x49E8u;
        goto L8001A1E4;
        L8001A0CC: ;
        c.V0 = (int)c.A1 < 128 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V1 = 0x80050000u;
            goto L8001A0F8;
        }
        c.V1 = 0x80050000u;
        c.A0 = c.A1 + 0u;
        c.RA = 0x8001A0E0u;
        GranTurismo2ArcadePC.func_80018EF4(c, m);
        c.V1 = c.V0 << 1;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 2;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 - 0x48E0u;
        goto L8001A120;
        L8001A0F8: ;
        c.V1 = c.V1 - 0x4850u;
        c.V0 = c.A1 << 1;
        c.V0 = c.V0 + c.V1;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.V0);
        c.V1 = c.V0 << 1;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 2;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 - 0x49E8u;
        L8001A120: ;
        c.S3 = c.V1 + c.V0;
        c.V1 = c.S6 - 0x1u;
        c.V1 = c.V1 < 0x00000002u ? 1u : 0u;
        c.V0 = c.S2 ^ 0x0001u;
        c.V0 = c.V0 < 0x00000001u ? 1u : 0u;
        c.V1 = c.V1 & c.V0;
        if (c.V1 == 0u) {
            c.S2 = c.SP + 0x40u;
            goto L8001A1E8;
        }
        c.S2 = c.SP + 0x40u;
        c.A0 = c.S2 + 0u;
        c.A1 = c.S5 + 0u;
        c.A2 = c.FP + 0x10u;
        c.V0 = 0x00000006u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x7Cu));
        c.S0 = 0x0000000Au;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S0);
        c.S1 = c.T0 + 0xDu;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x8Cu));
        c.A3 = c.S1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.T0);
        c.RA = 0x8001A174u;
        GranTurismo2ArcadePC.func_8006B958(c, m);
        c.A0 = c.S2 + 0u;
        c.A1 = c.S5 + 0u;
        c.A2 = c.FP - 0x10u;
        c.A3 = c.S1 + 0u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x8Cu));
        c.V0 = 0xFFFFFFFAu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.T0);
        c.RA = 0x8001A19Cu;
        GranTurismo2ArcadePC.func_8006B958(c, m);
        c.A0 = c.S5 + 0u;
        c.A1 = 0x00000020u;
        c.RA = 0x8001A1A8u;
        GranTurismo2ArcadePC.func_8007D954(c, m);
        c.A0 = c.S5 + 0u;
        goto L8001A1EC;
        L8001A1B0: ;
        c.A0 = c.A1 + 0u;
        c.S0 = 0x80050000u;
        c.S0 = c.S0 - 0x4830u;
        c.RA = 0x8001A1C0u;
        GranTurismo2ArcadePC.func_80018EBC(c, m);
        c.V0 = c.V0 << 1;
        c.V0 = c.V0 + c.S0;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.V0);
        c.V1 = c.V0 << 1;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 2;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 - 0x4958u;
        L8001A1E4: ;
        c.S3 = c.V1 + c.V0;
        L8001A1E8: ;
        c.A0 = c.S5 + 0u;
        L8001A1EC: ;
        c.A1 = c.S4 << 8;
        L8001A1F0: ;
        c.A1 = c.S4 | c.A1;
        c.V0 = c.S4 << 16;
        c.A1 = c.A1 | c.V0;
        c.RA = 0x8001A200u;
        GranTurismo2ArcadePC.func_80081388(c, m);
        c.A0 = c.V0 + 0u;
        c.V1 = MemoryAccess.ReadU16(m, (c.S3 + 0x4u));
        c.V0 = MemoryAccess.ReadU16(m, (c.S3 + 0x6u));
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x7Cu));
        c.V1 = c.V1 << 16;
        c.V1 = (uint)((int)c.V1 >> 17);
        c.V1 = c.FP - c.V1;
        c.V0 = c.V0 << 16;
        c.V0 = (uint)((int)c.V0 >> 17);
        c.V0 = c.T0 - c.V0;
        c.V0 = c.V0 << 16;
        c.V1 = c.V1 + c.V0;
        MemoryAccess.WriteU32(m, c.A0, c.V1);
        c.V0 = MemoryAccess.ReadU32(m, c.S3);
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.V0);
        L8001A240: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S3 + 0x4u));
        MemoryAccess.WriteU32(m, (c.A0 + 0x8u), c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S3 + 0x8u));
        c.A0 = c.S5 + 0u;
        c.A1 = c.A1 | 0x0020u;
        L8001A258: ;
        c.RA = 0x8001A260u;
        GranTurismo2ArcadePC.func_8007D954(c, m);
        L8001A260: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x6Cu));
        c.FP = MemoryAccess.ReadU32(m, (c.SP + 0x68u));
        c.S7 = MemoryAccess.ReadU32(m, (c.SP + 0x64u));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x60u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x5Cu));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x58u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x54u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x50u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x4Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x48u));
        c.SP = c.SP + 0x70u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001A290(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x68u;
        MemoryAccess.WriteU32(m, (c.SP + 0x5Cu), c.S7);
        c.S7 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x48u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x70u), c.A2);
        c.S2 = c.A2 + 0u;
        c.A0 = c.SP + 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x50u), c.S4);
        c.S4 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x60u), c.FP);
        c.FP = MemoryAccess.ReadU32(m, (c.SP + 0x80u));
        c.A1 = 0x0000001Eu;
        MemoryAccess.WriteU32(m, (c.SP + 0x58u), c.S6);
        c.S6 = c.A3 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x64u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x54u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x4Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.S0);
        c.RA = 0x8001A2E0u;
        GranTurismo2ArcadePC.func_8006AB78(c, m);
        c.V1 = 0xFF9F0000u;
        c.V1 = c.V1 | 0xFFFFu;
        c.A0 = c.SP + 0x20u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6D40u;
        c.S1 = 0u + 0u;
        c.V0 = MemoryAccess.ReadU32(m, (c.A0 + 0xCu));
        c.S5 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x10u), c.S7);
        c.V0 = c.V0 & c.V1;
        c.V1 = 0x00200000u;
        c.V0 = c.V0 | c.V1;
        MemoryAccess.WriteU32(m, (c.A0 + 0xCu), c.V0);
        c.RA = 0x8001A318u;
        GranTurismo2ArcadePC.func_8007D990(c, m);
        c.V0 = 0x80050000u;
        c.S3 = c.V0 - 0x4A38u;
        L8001A320: ;
        c.V0 = (int)c.S1 < 11 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.T0 = 0x80050000u;
            goto L8001A3D0;
        }
        c.T0 = 0x80050000u;
        c.A0 = c.T0 - 0x4A5Cu;
        c.T0 = 0x80050000u;
        c.A1 = c.T0 - 0x4A58u;
        c.A2 = c.S6 + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x8001A344u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S5 + 0u;
        c.A2 = c.S4 + 0u;
        c.A3 = c.S2 + 0u;
        c.S0 = c.S2 - 0xAu;
        c.S2 = c.S2 + 0x1Au;
        MemoryAccess.WriteU32(m, (c.S5 + 0x14u), c.V0);
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.A1 = MemoryAccess.ReadU32(m, c.S3);
        c.S3 = c.S3 + 0x4u;
        c.RA = 0x8001A370u;
        GranTurismo2ArcadePC.func_8006ACC4(c, m);
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x10D0u;
        c.A1 = c.S7 + 0u;
        c.A2 = c.S4 - 0x50u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x78u));
        c.A3 = c.S0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.T0);
        c.RA = 0x8001A39Cu;
        GranTurismo2ArcadePC.func_80019BB0(c, m);
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x10E8u;
        c.A1 = c.S7 + 0u;
        c.A2 = c.S4 + 0x50u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x7Cu));
        c.A3 = c.S0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.T0);
        c.RA = 0x8001A3C8u;
        GranTurismo2ArcadePC.func_80019BB0(c, m);
        c.S1 = c.S1 + 0x1u;
        goto L8001A320;
        L8001A3D0: ;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x78u));
        if ((int)c.T0 < 0) {
            c.A0 = c.SP + 0x20u;
            goto L8001A41C;
        }
        c.A0 = c.SP + 0x20u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6D30u;
        c.RA = 0x8001A3ECu;
        GranTurismo2ArcadePC.func_8007D990(c, m);
        c.V0 = 0x000B0000u;
        c.V0 = c.V0 | 0x3060u;
        c.A0 = c.SP + 0x20u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x618Eu;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x70u));
        c.A2 = c.S4 + 0u;
        c.A3 = c.T0 - 0x18u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.V0);
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.RA = 0x8001A41Cu;
        GranTurismo2ArcadePC.func_8006ACC4(c, m);
        L8001A41C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x64u));
        c.FP = MemoryAccess.ReadU32(m, (c.SP + 0x60u));
        c.S7 = MemoryAccess.ReadU32(m, (c.SP + 0x5Cu));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x58u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x54u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x50u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x4Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x48u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x44u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x40u));
        c.SP = c.SP + 0x68u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001A44C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x10D0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        MemoryAccess.WriteU16(m, c.S1, (ushort)c.S0);
        MemoryAccess.WriteU16(m, (c.S1 + 0x2u), (ushort)c.S0);
        MemoryAccess.WriteU16(m, (c.S1 + 0x4u), (ushort)0u);
        c.RA = 0x8001A47Cu;
        GranTurismo2ArcadePC.func_80018FE8(c, m);
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x10E8u;
        c.RA = 0x8001A488u;
        GranTurismo2ArcadePC.func_80018FE8(c, m);
        MemoryAccess.WriteU16(m, (c.S1 + 0x6u), (ushort)c.S0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001A4A0(CpuContext c, IMemory m)
    {
        c.V0 = 0x0000000Au;
        MemoryAccess.WriteU16(m, (c.A0 + 0x2u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, c.A0, (ushort)c.V0);
        c.V0 = 0x00000004u;
        MemoryAccess.WriteU16(m, (c.A0 + 0x6u), (ushort)c.V0);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001A4B8(CpuContext c, IMemory m)
    {
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU16(m, (c.A0 + 0x2u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, c.A0, (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.A0 + 0x6u), (ushort)c.V0);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001A4CC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x30u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S5);
        c.S5 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        c.S4 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x6u));
        c.V1 = MemoryAccess.ReadU16(m, (c.S0 + 0x6u));
        if ((int)c.V0 <= 0) {
            c.S6 = c.S4 + 0u;
            goto L8001A520;
        }
        c.S6 = c.S4 + 0u;
        c.S2 = 0u + 0u;
        c.V0 = c.V1 - 0x1u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x6u), (ushort)c.V0);
        L8001A520: ;
        c.V0 = MemoryAccess.ReadU16(m, (c.S0 + 0x4u));
        c.V0 = c.V0 + 0x1u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x4u), (ushort)c.V0);
        c.V0 = c.V0 << 16;
        c.V0 = (uint)((int)c.V0 >> 16);
        c.V0 = (int)c.V0 < 45 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = 0x800B0000u;
            goto L8001A548;
        }
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x4u), (ushort)0u);
        L8001A548: ;
        c.S3 = c.V0 + 0x10D0u;
        c.A0 = c.S3 + 0u;
        c.A1 = c.S5 + 0xDCu;
        c.A2 = 0u + 0u;
        c.RA = 0x8001A55Cu;
        GranTurismo2ArcadePC.func_80018FF4(c, m);
        c.V0 = 0x800B0000u;
        c.S1 = c.V0 + 0x10E8u;
        c.A0 = c.S1 + 0u;
        c.A1 = c.S5 + 0x140u;
        c.A2 = 0x00000001u;
        c.RA = 0x8001A574u;
        GranTurismo2ArcadePC.func_80018FF4(c, m);
        if (c.S2 == 0u) {
            c.A0 = c.S3 + 0u;
            goto L8001A5A0;
        }
        c.A0 = c.S3 + 0u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, c.S0);
        c.A1 = c.S5 + 0x1A4u;
        c.RA = 0x8001A588u;
        GranTurismo2ArcadePC.func_800199C8(c, m);
        c.S4 = c.V0 + 0u;
        c.A0 = c.S1 + 0u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x2u));
        c.A1 = c.S5 + 0x1B4u;
        c.RA = 0x8001A59Cu;
        GranTurismo2ArcadePC.func_800199C8(c, m);
        c.S6 = c.V0 + 0u;
        L8001A5A0: ;
        c.A0 = c.S3 + 0u;
        c.A1 = 0u + 0u;
        c.RA = 0x8001A5ACu;
        GranTurismo2ArcadePC.func_80019104(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = 0x00000001u;
        c.RA = 0x8001A5B8u;
        GranTurismo2ArcadePC.func_80019104(c, m);
        c.V1 = c.S4 ^ 0x0002u;
        c.V1 = c.V1 < 0x00000001u ? 1u : 0u;
        c.V0 = c.S6 ^ 0x0002u;
        c.V0 = c.V0 < 0x00000001u ? 1u : 0u;
        c.V1 = c.V1 | c.V0;
        if (c.V1 != 0u) {
            c.V0 = 0u + 0u;
            goto L8001A6C8;
        }
        c.V0 = 0u + 0u;
        c.V0 = 0u < c.S2 ? 1u : 0u;
        c.V1 = c.S4 < 0x00000001u ? 1u : 0u;
        c.V0 = c.V0 & c.V1;
        if (c.V0 == 0u) {
            c.V0 = 0u < c.S2 ? 1u : 0u;
            goto L8001A650;
        }
        c.V0 = 0u < c.S2 ? 1u : 0u;
        c.V1 = MemoryAccess.ReadU32(m, (c.S2 + 0x4u));
        c.V0 = MemoryAccess.ReadU32(m, (c.S2 + 0xCu));
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, c.S0);
        c.V1 = c.V1 | c.V0;
        c.V0 = c.V1 & 0x0001u;
        if (c.V0 == 0u) {
            c.V0 = c.V1 & 0x0002u;
            goto L8001A614;
        }
        c.V0 = c.V1 & 0x0002u;
        c.A0 = c.A0 - 0x1u;
        if ((int)c.A0 >= 0) {
            goto L8001A614;
        }
        c.A0 = 0x0000000Au;
        L8001A614: ;
        if (c.V0 == 0u) {
            goto L8001A630;
        }
        c.A0 = c.A0 + 0x1u;
        c.V0 = (int)c.A0 < 11 ? 1u : 0u;
        if (c.V0 != 0u) {
            goto L8001A630;
        }
        c.A0 = 0u + 0u;
        L8001A630: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.S0);
        if (c.A0 == c.V0) {
            c.V0 = 0u < c.S2 ? 1u : 0u;
            goto L8001A650;
        }
        c.V0 = 0u < c.S2 ? 1u : 0u;
        MemoryAccess.WriteU16(m, c.S0, (ushort)c.A0);
        c.A0 = 0x00000006u;
        c.RA = 0x8001A64Cu;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.V0 = 0u < c.S2 ? 1u : 0u;
        L8001A650: ;
        c.V1 = c.S6 < 0x00000001u ? 1u : 0u;
        c.V0 = c.V0 & c.V1;
        if (c.V0 == 0u) {
            c.V0 = c.S5 + 0x1B4u;
            goto L8001A6C4;
        }
        c.V0 = c.S5 + 0x1B4u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0xCu));
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x2u));
        c.V1 = c.V1 | c.V0;
        c.V0 = c.V1 & 0x0001u;
        if (c.V0 == 0u) {
            c.V0 = c.V1 & 0x0002u;
            goto L8001A68C;
        }
        c.V0 = c.V1 & 0x0002u;
        c.A0 = c.A0 - 0x1u;
        if ((int)c.A0 >= 0) {
            goto L8001A68C;
        }
        c.A0 = 0x0000000Au;
        L8001A68C: ;
        if (c.V0 == 0u) {
            goto L8001A6A8;
        }
        c.A0 = c.A0 + 0x1u;
        c.V0 = (int)c.A0 < 11 ? 1u : 0u;
        if (c.V0 != 0u) {
            goto L8001A6A8;
        }
        c.A0 = 0u + 0u;
        L8001A6A8: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x2u));
        if (c.A0 == c.V0) {
            c.V0 = 0xFFFFFFFFu;
            goto L8001A6C8;
        }
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU16(m, (c.S0 + 0x2u), (ushort)c.A0);
        c.A0 = 0x00000006u;
        c.RA = 0x8001A6C4u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        L8001A6C4: ;
        c.V0 = 0xFFFFFFFFu;
        L8001A6C8: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x2Cu));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x30u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001A6F0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.A0);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0x2u));
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0x4u));
        c.A0 = c.A1 + 0u;
        c.A1 = c.A2 + 0u;
        c.A2 = c.A3 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.V0);
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0x38u));
        c.RA = 0x8001A730u;
        GranTurismo2ArcadePC.func_8001A290(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001A740(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V1 = 0x801D0000u;
        c.V1 = MemoryAccess.ReadU8(m, (c.V1 - 0x6CC0u));
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A8u));
        c.SP = c.SP - 0x18u;
        if (c.V1 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
            goto L8001A764;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.RA = 0x8001A764u;
        GranTurismo2ArcadePC.func_8007F084(c, m);
        L8001A764: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001A774(CpuContext c, IMemory m)
    {
        c.A1 = c.A0 + 0u;
        c.V0 = 0x800B0000u;
        c.V1 = 0x801D0000u;
        c.V1 = MemoryAccess.ReadU8(m, (c.V1 - 0x6CC0u));
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A8u));
        c.SP = c.SP - 0x18u;
        if (c.V1 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
            goto L8001A79C;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.RA = 0x8001A79Cu;
        GranTurismo2ArcadePC.func_8006EBE8(c, m);
        L8001A79C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001A7AC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x38u;
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.S7);
        c.S7 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = c.A3 + 0u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0x48u));
        c.V0 = 0x801D0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S6);
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x4Cu));
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6CC0u));
        c.V1 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        c.S4 = MemoryAccess.ReadU32(m, (c.V1 - 0x75A8u));
        c.A0 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        if (c.V0 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
            goto L8001A8B8;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        if (c.A2 == 0u) {
            c.S0 = c.A1 + 0u;
            goto L8001A838;
        }
        c.S0 = c.A1 + 0u;
        c.V0 = MemoryAccess.ReadU8(m, c.A1);
        if (c.V0 == 0u) {
            c.V1 = c.A1 + 0u;
            goto L8001A82C;
        }
        c.V1 = c.A1 + 0u;
        L8001A818: ;
        c.V1 = c.V1 + 0x2u;
        c.V0 = MemoryAccess.ReadU8(m, c.V1);
        if (c.V0 != 0u) {
            c.A0 = c.A0 + 0xCu;
            goto L8001A818;
        }
        c.A0 = c.A0 + 0xCu;
        L8001A82C: ;
        c.V0 = (uint)((int)c.A0 >> 1);
        c.S1 = c.S1 - c.V0;
        c.S0 = c.A1 + 0u;
        L8001A838: ;
        c.S5 = 0x02000000u;
        c.S2 = c.S3 + 0x1Fu;
        L8001A840: ;
        c.V0 = MemoryAccess.ReadU8(m, c.S0);
        if (c.V0 == 0u) {
            c.A0 = c.S4 + 0u;
            goto L8001A8B8;
        }
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.V0 = MemoryAccess.ReadU8(m, (c.S0 + 0x1u));
        c.A1 = c.A1 << 8;
        c.A1 = c.A1 | c.V0;
        c.RA = 0x8001A864u;
        GranTurismo2ArcadePC.func_8007F09C(c, m);
        if (c.V0 == 0u) {
            c.A0 = c.V0 + 0u;
            goto L8001A8AC;
        }
        c.A0 = c.V0 + 0u;
        c.A1 = c.S7 + 0u;
        c.A2 = c.S6 | c.S5;
        c.RA = 0x8001A878u;
        GranTurismo2ArcadePC.func_8007EF2C(c, m);
        c.V1 = MemoryAccess.ReadU16(m, (c.V0 + 0x12u));
        c.A0 = c.S1 + 0xFu;
        MemoryAccess.WriteU16(m, (c.V0 + 0x4u), (ushort)c.S1);
        MemoryAccess.WriteU16(m, (c.V0 + 0x6u), (ushort)c.S3);
        MemoryAccess.WriteU16(m, (c.V0 + 0xCu), (ushort)c.A0);
        MemoryAccess.WriteU16(m, (c.V0 + 0xEu), (ushort)c.S3);
        MemoryAccess.WriteU16(m, (c.V0 + 0x14u), (ushort)c.S1);
        MemoryAccess.WriteU16(m, (c.V0 + 0x16u), (ushort)c.S2);
        MemoryAccess.WriteU16(m, (c.V0 + 0x1Cu), (ushort)c.A0);
        MemoryAccess.WriteU16(m, (c.V0 + 0x1Eu), (ushort)c.S2);
        c.V1 = c.V1 & 0xFF9Fu;
        c.V1 = c.V1 | 0x0020u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x12u), (ushort)c.V1);
        L8001A8AC: ;
        c.S1 = c.S1 + 0xCu;
        c.S0 = c.S0 + 0x2u;
        goto L8001A840;
        L8001A8B8: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x30u));
        c.S7 = MemoryAccess.ReadU32(m, (c.SP + 0x2Cu));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x38u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001A8E4(CpuContext c, IMemory m)
    {
        c.V0 = 0x801D0000u;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6CC0u));
        c.SP = c.SP - 0x18u;
        if (c.V0 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
            goto L8001A910;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.RA = 0x8001A900u;
        GranTurismo2ArcadePC.func_8001A740(c, m);
        c.V0 = 0x800B0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1100u));
        c.RA = 0x8001A910u;
        GranTurismo2ArcadePC.func_8001A774(c, m);
        L8001A910: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001A920(CpuContext c, IMemory m)
    {
        c.V1 = MemoryAccess.ReadU32(m, c.A0);
        c.V0 = (int)c.V1 < 64 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = (int)c.V1 < 193 ? 1u : 0u;
            goto L8001A93C;
        }
        c.V0 = (int)c.V1 < 193 ? 1u : 0u;
        c.V1 = 0x00000040u;
        c.V0 = (int)c.V1 < 193 ? 1u : 0u;
        L8001A93C: ;
        if (c.V0 != 0u) {
            goto L8001A948;
        }
        c.V1 = 0x000000C0u;
        L8001A948: ;
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0x10u));
        c.V0 = (int)c.V1 < (int)c.A1 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L8001A960;
        }
        c.V1 = c.A1 + 0u;
        L8001A960: ;
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0xCu));
        c.V0 = (int)c.A1 < (int)c.V1 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L8001A978;
        }
        c.V1 = c.A1 + 0u;
        L8001A978: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.A0 + 0x4u));
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0x10u));
        c.A2 = c.V1 + 0u;
        MemoryAccess.WriteU32(m, c.A0, c.V1);
        c.V1 = c.V1 - c.V0;
        c.V0 = (int)c.V1 < (int)c.A1 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V1 = c.A2 - c.A1;
            goto L8001A99C;
        }
        c.V1 = c.A2 - c.A1;
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.V1);
        L8001A99C: ;
        c.A2 = MemoryAccess.ReadU32(m, c.A0);
        c.V0 = MemoryAccess.ReadU32(m, (c.A0 + 0x4u));
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0xCu));
        c.V1 = c.A2 + c.V0;
        c.V0 = (int)c.A1 < (int)c.V1 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V1 = c.A1 - c.A2;
            goto L8001A9BC;
        }
        c.V1 = c.A1 - c.A2;
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.V1);
        L8001A9BC: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.A0 + 0x4u));
        if ((int)c.V0 >= 0) {
            goto L8001A9D0;
        }
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), 0u);
        L8001A9D0: ;
        c.V1 = MemoryAccess.ReadU32(m, (c.A0 + 0x4u));
        c.V0 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.V0 = (int)c.V1 < (int)c.V0 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L8001A9EC;
        }
        MemoryAccess.WriteU32(m, (c.A0 + 0x8u), c.V1);
        L8001A9EC: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        if ((int)c.V0 >= 0) {
            goto L8001AA00;
        }
        MemoryAccess.WriteU32(m, (c.A0 + 0x8u), 0u);
        L8001AA00: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001AA08(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU32(m, (c.A0 + 0x8u), c.A1);
        c.RA = 0x8001AA18u;
        GranTurismo2ArcadePC.func_8001A920(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001AA28(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.A1);
        c.RA = 0x8001AA38u;
        GranTurismo2ArcadePC.func_8001A920(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001AA48(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU32(m, c.A0, c.A1);
        c.RA = 0x8001AA58u;
        GranTurismo2ArcadePC.func_8001A920(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001AA68(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V0 = (int)c.A1 < (int)c.A2 ? 1u : 0u;
        if (c.V0 == 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
            goto L8001AA84;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V0 = c.A1 + 0u;
        c.A1 = c.A2 + 0u;
        c.A2 = c.V0 + 0u;
        L8001AA84: ;
        MemoryAccess.WriteU32(m, (c.A0 + 0xCu), c.A1);
        MemoryAccess.WriteU32(m, (c.A0 + 0x10u), c.A2);
        c.RA = 0x8001AA90u;
        GranTurismo2ArcadePC.func_8001A920(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001AAA0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x58u;
        MemoryAccess.WriteU32(m, (c.SP + 0x4Cu), c.S5);
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x68u));
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x70u));
        MemoryAccess.WriteU32(m, (c.SP + 0x3Cu), c.S1);
        c.S1 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x50u), c.S6);
        c.S6 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.S3);
        c.S3 = c.A3 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x48u), c.S4);
        c.S4 = 0x00000080u;
        MemoryAccess.WriteU32(m, (c.SP + 0x54u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.S2);
        if ((int)c.V0 < 0) {
            MemoryAccess.WriteU32(m, (c.SP + 0x38u), c.S0);
            goto L8001AAE4;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x38u), c.S0);
        c.S4 = 0x00000180u;
        L8001AAE4: ;
        c.A0 = c.SP + 0x18u;
        c.A1 = 0x0000001Eu;
        c.RA = 0x8001AAF0u;
        GranTurismo2ArcadePC.func_8006AB78(c, m);
        c.V1 = 0xFF9F0000u;
        c.V1 = c.V1 | 0xFFFFu;
        c.S0 = c.SP + 0x18u;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0xCu));
        c.A1 = c.A1 - 0x6CE0u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x10u), c.S1);
        c.V0 = c.V0 & c.V1;
        c.V1 = 0x00200000u;
        c.V0 = c.V0 | c.V1;
        MemoryAccess.WriteU32(m, (c.S0 + 0xCu), c.V0);
        c.RA = 0x8001AB24u;
        GranTurismo2ArcadePC.func_8007D990(c, m);
        c.S1 = 0x80050000u;
        c.S1 = c.S1 - 0x4810u;
        c.A0 = c.S1 + 0u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x47E4u;
        c.A2 = c.S5 + 0u;
        c.A3 = c.S4 + 0u;
        c.RA = 0x8001AB44u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S0 + 0u;
        c.A2 = c.S6 - 0x76u;
        c.S3 = c.S3 + 0x2Cu;
        c.A3 = c.S3 + 0u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0x6Cu));
        c.S2 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S2);
        c.RA = 0x8001AB68u;
        GranTurismo2ArcadePC.func_8006ABA0(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6D40u;
        c.RA = 0x8001AB78u;
        GranTurismo2ArcadePC.func_8007D990(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x47ECu;
        c.A2 = c.S5 + 0u;
        c.A3 = c.S4 + 0u;
        c.RA = 0x8001AB90u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x612Fu;
        c.A2 = c.S6 + 0x6Eu;
        c.A3 = c.S3 + 0u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S2);
        c.RA = 0x8001ABB0u;
        GranTurismo2ArcadePC.func_8006AD38(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x54u));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x50u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x4Cu));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x48u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x44u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x40u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x3Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x38u));
        c.SP = c.SP + 0x58u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001ABD8_gt2_arcade_overlay_1(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0xB0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x8Cu), c.S1);
        c.S1 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x98u), c.S4);
        c.S4 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0xA0u), c.S6);
        c.S6 = c.A3 + 0u;
        c.V1 = MemoryAccess.ReadU32(m, (c.SP + 0xC4u));
        c.T0 = 0x00000080u;
        MemoryAccess.WriteU32(m, (c.SP + 0x9Cu), c.S5);
        c.S5 = c.T0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0xACu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0xA8u), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0xA4u), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x94u), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x90u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x88u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0xB0u), c.A0);
        MemoryAccess.WriteU32(m, (c.SP + 0x84u), c.T0);
        if ((int)c.V1 < 0) {
            MemoryAccess.WriteU32(m, (c.SP + 0x80u), c.T0);
            goto L8001AC8C;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x80u), c.T0);
        c.T0 = 0x00000180u;
        c.S5 = c.T0 + 0u;
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x84u), c.T0);
        if (c.V1 == c.V0) {
            MemoryAccess.WriteU32(m, (c.SP + 0x80u), c.T0);
            goto L8001AC78;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x80u), c.T0);
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L8001AC60;
        }
        c.V0 = 0x00000002u;
        if (c.V1 == 0u) {
            c.A0 = c.SP + 0x20u;
            goto L8001AC70;
        }
        c.A0 = c.SP + 0x20u;
        goto L8001AC90;
        L8001AC60: ;
        if (c.V1 == c.V0) {
            c.A0 = c.SP + 0x20u;
            goto L8001AC84;
        }
        c.A0 = c.SP + 0x20u;
        goto L8001AC90;
        L8001AC70: ;
        c.S5 = 0x00000080u;
        goto L8001AC8C;
        L8001AC78: ;
        c.T0 = 0x00000080u;
        MemoryAccess.WriteU32(m, (c.SP + 0x80u), c.T0);
        goto L8001AC8C;
        L8001AC84: ;
        c.T0 = 0x00000080u;
        MemoryAccess.WriteU32(m, (c.SP + 0x84u), c.T0);
        L8001AC8C: ;
        c.A0 = c.SP + 0x20u;
        L8001AC90: ;
        c.A1 = 0x0000001Eu;
        c.RA = 0x8001AC98u;
        GranTurismo2ArcadePC.func_8006AB78(c, m);
        c.V1 = 0xFF9F0000u;
        c.V1 = c.V1 | 0xFFFFu;
        c.S0 = c.SP + 0x20u;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0xCu));
        c.A1 = c.A1 - 0x6CE0u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x10u), c.S1);
        c.V0 = c.V0 & c.V1;
        c.V1 = 0x00200000u;
        c.V0 = c.V0 | c.V1;
        MemoryAccess.WriteU32(m, (c.S0 + 0xCu), c.V0);
        c.RA = 0x8001ACCCu;
        GranTurismo2ArcadePC.func_8007D990(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4810u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x47E4u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0xC0u));
        c.A3 = 0x00000080u;
        c.RA = 0x8001ACE8u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S0 + 0u;
        c.A2 = c.S4 - 0x76u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xC8u));
        c.A3 = c.S6 + 0x2Cu;
        MemoryAccess.WriteU32(m, (c.S0 + 0x14u), c.V0);
        c.T0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        c.RA = 0x8001AD08u;
        GranTurismo2ArcadePC.func_8006ABA0(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6D40u;
        c.RA = 0x8001AD18u;
        GranTurismo2ArcadePC.func_8007D990(c, m);
        c.FP = c.S4 + 0x48u;
        c.S6 = c.S6 + 0x1Eu;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4810u;
        c.S2 = 0x80050000u;
        c.S2 = c.S2 - 0x47ECu;
        c.A1 = c.S2 + 0u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0xC0u));
        c.A3 = c.S5 + 0u;
        c.RA = 0x8001AD40u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S0 + 0u;
        c.S1 = 0x801C0000u;
        c.S1 = c.S1 - 0x6141u;
        c.A1 = c.S1 + 0u;
        c.A2 = c.FP + 0u;
        c.A3 = c.S6 + 0u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x14u), c.V0);
        c.T0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        c.RA = 0x8001AD68u;
        GranTurismo2ArcadePC.func_8006AD38(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4810u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0xC0u));
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0x80u));
        c.A1 = c.S2 + 0u;
        c.RA = 0x8001AD80u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0x7u;
        c.A2 = c.FP + 0u;
        c.S7 = c.S6 + 0x14u;
        c.A3 = c.S7 + 0u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x14u), c.V0);
        c.T0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        c.RA = 0x8001ADA4u;
        GranTurismo2ArcadePC.func_8006AD38(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4810u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0xC0u));
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0x84u));
        c.A1 = c.S2 + 0u;
        c.RA = 0x8001ADBCu;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0xBu;
        c.A2 = c.FP + 0u;
        c.S3 = c.S6 + 0x28u;
        c.A3 = c.S3 + 0u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x14u), c.V0);
        c.T0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        c.RA = 0x8001ADE0u;
        GranTurismo2ArcadePC.func_8006AD38(c, m);
        c.FP = c.S4 + 0x6Eu;
        c.S4 = c.SP + 0x40u;
        c.A0 = c.S4 + 0u;
        c.S2 = 0x80020000u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xB0u));
        c.S2 = c.S2 + 0x2478u;
        c.A2 = MemoryAccess.ReadU32(m, c.T0);
        c.A1 = c.S2 + 0u;
        c.A2 = c.A2 - 0x80u;
        c.RA = 0x8001AE08u;
        GranTurismo2ArcadePC.func_8008CE44(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4810u;
        c.S1 = 0x80050000u;
        c.S1 = c.S1 - 0x47E8u;
        c.A1 = c.S1 + 0u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0xC0u));
        c.A3 = c.S5 + 0u;
        c.RA = 0x8001AE28u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S4 + 0u;
        c.A2 = c.FP + 0u;
        c.A3 = c.S6 + 0u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x14u), c.V0);
        c.T0 = 0x00000001u;
        c.S6 = 0xFFFFFFFEu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.RA = 0x8001AE54u;
        GranTurismo2ArcadePC.func_8006B094(c, m);
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xB0u));
        c.A0 = c.S4 + 0u;
        c.A2 = MemoryAccess.ReadU32(m, (c.T0 + 0x4u));
        c.A1 = c.S2 + 0u;
        c.RA = 0x8001AE68u;
        GranTurismo2ArcadePC.func_8008CE44(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4810u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0xC0u));
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0x80u));
        c.A1 = c.S1 + 0u;
        c.RA = 0x8001AE80u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S4 + 0u;
        c.A2 = c.FP + 0u;
        c.A3 = c.S7 + 0u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x14u), c.V0);
        c.T0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.RA = 0x8001AEA8u;
        GranTurismo2ArcadePC.func_8006B094(c, m);
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xB0u));
        c.A0 = c.S4 + 0u;
        c.A2 = MemoryAccess.ReadU32(m, (c.T0 + 0x8u));
        c.A1 = c.S2 + 0u;
        c.RA = 0x8001AEBCu;
        GranTurismo2ArcadePC.func_8008CE44(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4810u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0xC0u));
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0x84u));
        c.A1 = c.S1 + 0u;
        c.RA = 0x8001AED4u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S4 + 0u;
        c.A2 = c.FP + 0u;
        c.A3 = c.S3 + 0u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.V0);
        c.T0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.RA = 0x8001AEFCu;
        GranTurismo2ArcadePC.func_8006B094(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0xACu));
        c.FP = MemoryAccess.ReadU32(m, (c.SP + 0xA8u));
        c.S7 = MemoryAccess.ReadU32(m, (c.SP + 0xA4u));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0xA0u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x9Cu));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x98u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x94u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x90u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x8Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x88u));
        c.SP = c.SP + 0xB0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001AF2C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x78u;
        MemoryAccess.WriteU32(m, (c.SP + 0x64u), c.S5);
        c.S5 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x60u), c.S4);
        c.S4 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x70u), c.FP);
        c.FP = c.A3 + 0u;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4810u;
        MemoryAccess.WriteU32(m, (c.SP + 0x50u), c.S0);
        c.S0 = 0x80050000u;
        c.S0 = c.S0 - 0x4804u;
        MemoryAccess.WriteU32(m, (c.SP + 0x6Cu), c.S7);
        c.S7 = MemoryAccess.ReadU32(m, (c.SP + 0x88u));
        c.A1 = c.S0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x68u), c.S6);
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x8Cu));
        c.A3 = 0x00000080u;
        MemoryAccess.WriteU32(m, (c.SP + 0x74u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x5Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x58u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x54u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x80u), c.A2);
        c.V1 = MemoryAccess.ReadU32(m, c.S5);
        c.T0 = MemoryAccess.ReadU32(m, (c.S5 + 0x8u));
        c.T1 = MemoryAccess.ReadU32(m, (c.S5 + 0x4u));
        c.T2 = MemoryAccess.ReadU32(m, (c.S5 + 0xCu));
        c.T3 = MemoryAccess.ReadU32(m, (c.S5 + 0x10u));
        c.V0 = c.FP - 0x81u;
        MemoryAccess.WriteU16(m, (c.SP + 0x20u), (ushort)c.V0);
        c.V0 = 0x00000102u;
        MemoryAccess.WriteU16(m, (c.SP + 0x24u), (ushort)c.V0);
        c.V0 = 0x0000000Eu;
        c.T4 = MemoryAccess.ReadU32(m, (c.SP + 0x90u));
        c.S2 = c.FP - 0x80u;
        MemoryAccess.WriteU16(m, (c.SP + 0x26u), (ushort)c.V0);
        c.A2 = c.S6 + 0u;
        c.V0 = c.S7 - 0x7u;
        c.S3 = c.S7 - 0x6u;
        c.T4 = c.S2 + c.T4;
        c.T5 = c.S2 + c.V1;
        MemoryAccess.WriteU16(m, (c.SP + 0x22u), (ushort)c.V0);
        c.V0 = c.V1 + c.T0;
        c.V0 = c.S2 + c.V0;
        c.T0 = c.V1 - c.T0;
        c.T0 = c.S2 + c.T0;
        MemoryAccess.WriteU32(m, (c.SP + 0x34u), c.V0);
        c.V0 = c.V1 + c.T1;
        c.V0 = c.S2 + c.V0;
        c.V1 = c.V1 - c.T1;
        c.V1 = c.S2 + c.V1;
        c.T2 = c.S2 + c.T2;
        c.T3 = c.S2 + c.T3;
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.T4);
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.T5);
        MemoryAccess.WriteU32(m, (c.SP + 0x38u), c.T0);
        MemoryAccess.WriteU32(m, (c.SP + 0x3Cu), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.V1);
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.T2);
        MemoryAccess.WriteU32(m, (c.SP + 0x48u), c.T3);
        c.RA = 0x8001B020u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.S1 = c.SP + 0x20u;
        c.A2 = c.S1 + 0u;
        c.RA = 0x8001B034u;
        GranTurismo2ArcadePC.func_8007E690(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4810u;
        c.A1 = c.S0 + 0u;
        c.A2 = c.S6 + 0u;
        c.A3 = 0x00000080u;
        c.V0 = c.FP - 0x82u;
        MemoryAccess.WriteU16(m, (c.SP + 0x20u), (ushort)c.V0);
        c.V0 = 0x00000104u;
        MemoryAccess.WriteU16(m, (c.SP + 0x24u), (ushort)c.V0);
        c.V0 = c.S7 - 0x8u;
        MemoryAccess.WriteU16(m, (c.SP + 0x22u), (ushort)c.V0);
        c.V0 = 0x00000010u;
        MemoryAccess.WriteU16(m, (c.SP + 0x26u), (ushort)c.V0);
        c.RA = 0x8001B06Cu;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.A2 = c.S1 + 0u;
        c.RA = 0x8001B07Cu;
        GranTurismo2ArcadePC.func_8007E690(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = 0x00000020u;
        c.RA = 0x8001B088u;
        GranTurismo2ArcadePC.func_8007D954(c, m);
        c.T4 = MemoryAccess.ReadU32(m, (c.SP + 0x98u));
        if ((int)c.T4 < 0) {
            c.V0 = (int)c.T4 < 2 ? 1u : 0u;
            goto L8001B3E4;
        }
        c.V0 = (int)c.T4 < 2 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.A0 = c.S5 + 0u;
            goto L8001B158;
        }
        c.A0 = c.S5 + 0u;
        c.V0 = 0x00000002u;
        if (c.T4 != c.V0) {
            c.A0 = 0x80050000u;
            goto L8001B3E8;
        }
        c.A0 = 0x80050000u;
        c.T5 = MemoryAccess.ReadU32(m, (c.SP + 0x9Cu));
        if ((int)c.T5 < 0) {
            c.S0 = c.S2 + c.T5;
            goto L8001B100;
        }
        c.S0 = c.S2 + c.T5;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x480Cu;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x47F0u;
        c.A2 = c.S6 + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x8001B0D8u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001B0E4u;
        GranTurismo2ArcadePC.func_8007CF34(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU16(m, (c.V1 + 0x4u), (ushort)c.V0);
        c.V0 = 0x0000000Cu;
        MemoryAccess.WriteU16(m, c.V1, (ushort)c.S0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x2u), (ushort)c.S3);
        MemoryAccess.WriteU16(m, (c.V1 + 0x6u), (ushort)c.V0);
        L8001B100: ;
        c.T4 = MemoryAccess.ReadU32(m, (c.SP + 0xA0u));
        if ((int)c.T4 < 0) {
            c.S0 = c.S2 + c.T4;
            goto L8001B3E4;
        }
        c.S0 = c.S2 + c.T4;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x480Cu;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x47F0u;
        c.A2 = c.S6 + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x8001B12Cu;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001B138u;
        GranTurismo2ArcadePC.func_8007CF34(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU16(m, (c.V1 + 0x4u), (ushort)c.V0);
        c.V0 = 0x0000000Cu;
        MemoryAccess.WriteU16(m, c.V1, (ushort)c.S0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x2u), (ushort)c.S3);
        MemoryAccess.WriteU16(m, (c.V1 + 0x6u), (ushort)c.V0);
        goto L8001B3E4;
        L8001B158: ;
        c.A1 = c.S4 + 0u;
        c.A2 = c.FP + 0u;
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x94u));
        c.T5 = MemoryAccess.ReadU32(m, (c.SP + 0x80u));
        c.A3 = c.S7 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.T5);
        c.RA = 0x8001B17Cu;
        Dispatcher.Call(c, m, 0x8001ABD8u);
        c.T4 = MemoryAccess.ReadU32(m, (c.SP + 0x90u));
        if ((int)c.T4 < 0) {
            c.A0 = 0x80050000u;
            goto L8001B1F4;
        }
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x480Cu;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x47F0u;
        c.A2 = c.S6 + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x8001B1A4u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001B1B0u;
        GranTurismo2ArcadePC.func_8007CF34(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = c.SP + 0x28u;
        c.A1 = c.S4 + 0u;
        c.A3 = c.S7 - 0x16u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0x2Cu));
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU16(m, (c.V1 + 0x4u), (ushort)c.V0);
        c.V0 = 0x0000000Cu;
        MemoryAccess.WriteU16(m, (c.V1 + 0x6u), (ushort)c.V0);
        c.V0 = 0x00000003u;
        MemoryAccess.WriteU16(m, (c.V1 + 0x2u), (ushort)c.S3);
        MemoryAccess.WriteU16(m, c.V1, (ushort)c.A2);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.V0 = 0x0000000Eu;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.RA = 0x8001B1F4u;
        GranTurismo2ArcadePC.func_8006B898(c, m);
        L8001B1F4: ;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x480Cu;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x47F4u;
        c.A2 = c.S6 + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x8001B210u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001B21Cu;
        GranTurismo2ArcadePC.func_8007CF34(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4810u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x4800u;
        c.A2 = c.S6 + 0u;
        c.A3 = 0x00000080u;
        c.T5 = MemoryAccess.ReadU16(m, (c.SP + 0x30u));
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU16(m, (c.V1 + 0x4u), (ushort)c.V0);
        c.V0 = 0x00000006u;
        MemoryAccess.WriteU16(m, (c.V1 + 0x2u), (ushort)c.S3);
        MemoryAccess.WriteU16(m, (c.V1 + 0x6u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, c.V1, (ushort)c.T5);
        c.RA = 0x8001B258u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001B264u;
        GranTurismo2ArcadePC.func_8007CF34(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4810u;
        c.S0 = 0x80050000u;
        c.S0 = c.S0 - 0x47FCu;
        c.A1 = c.S0 + 0u;
        c.A2 = c.S6 + 0u;
        c.A3 = 0x00000080u;
        c.T4 = MemoryAccess.ReadU16(m, (c.SP + 0x38u));
        MemoryAccess.WriteU16(m, c.V1, (ushort)c.T4);
        c.T5 = MemoryAccess.ReadU32(m, (c.SP + 0x34u));
        c.T4 = MemoryAccess.ReadU32(m, (c.SP + 0x38u));
        c.S1 = 0x0000000Cu;
        MemoryAccess.WriteU16(m, (c.V1 + 0x2u), (ushort)c.S3);
        MemoryAccess.WriteU16(m, (c.V1 + 0x6u), (ushort)c.S1);
        c.V0 = c.T5 - c.T4;
        MemoryAccess.WriteU16(m, (c.V1 + 0x4u), (ushort)c.V0);
        c.RA = 0x8001B2B0u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001B2BCu;
        GranTurismo2ArcadePC.func_8007CF34(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4810u;
        c.A1 = c.S0 + 0u;
        c.A2 = c.S6 + 0u;
        c.T5 = MemoryAccess.ReadU16(m, (c.SP + 0x34u));
        MemoryAccess.WriteU16(m, c.V1, (ushort)c.T5);
        c.T4 = MemoryAccess.ReadU32(m, (c.SP + 0x3Cu));
        c.T5 = MemoryAccess.ReadU32(m, (c.SP + 0x34u));
        c.A3 = 0x00000080u;
        c.V0 = c.T4 - c.T5;
        MemoryAccess.WriteU16(m, (c.V1 + 0x4u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x2u), (ushort)c.S3);
        MemoryAccess.WriteU16(m, (c.V1 + 0x6u), (ushort)c.S1);
        c.RA = 0x8001B2FCu;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001B308u;
        GranTurismo2ArcadePC.func_8007CF34(c, m);
        c.V1 = c.V0 + 0u;
        c.T4 = MemoryAccess.ReadU16(m, (c.SP + 0x40u));
        MemoryAccess.WriteU16(m, c.V1, (ushort)c.T4);
        c.T5 = MemoryAccess.ReadU32(m, (c.SP + 0x38u));
        c.T4 = MemoryAccess.ReadU32(m, (c.SP + 0x40u));
        c.V0 = c.T5 - c.T4;
        MemoryAccess.WriteU16(m, (c.V1 + 0x4u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x2u), (ushort)c.S3);
        MemoryAccess.WriteU16(m, (c.V1 + 0x6u), (ushort)c.S1);
        c.T5 = MemoryAccess.ReadU32(m, (c.SP + 0x98u));
        c.V0 = 0x00000001u;
        if (c.T5 != c.V0) {
            c.A0 = 0x80050000u;
            goto L8001B3E8;
        }
        c.A0 = 0x80050000u;
        c.T4 = MemoryAccess.ReadU32(m, (c.SP + 0x44u));
        c.V0 = 0x00000100u;
        c.V1 = c.T4 - c.S2;
        c.S0 = c.V0 - c.V1;
        if ((int)c.S0 <= 0) {
            c.A2 = c.S6 + 0u;
            goto L8001B398;
        }
        c.A2 = c.S6 + 0u;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4810u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x47F8u;
        c.A3 = 0x00000080u;
        c.RA = 0x8001B374u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001B380u;
        GranTurismo2ArcadePC.func_8007CF34(c, m);
        c.V1 = c.V0 + 0u;
        c.T5 = MemoryAccess.ReadU16(m, (c.SP + 0x44u));
        MemoryAccess.WriteU16(m, (c.V1 + 0x4u), (ushort)c.S0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x2u), (ushort)c.S3);
        MemoryAccess.WriteU16(m, (c.V1 + 0x6u), (ushort)c.S1);
        MemoryAccess.WriteU16(m, c.V1, (ushort)c.T5);
        L8001B398: ;
        c.T4 = MemoryAccess.ReadU32(m, (c.SP + 0x48u));
        c.S0 = c.T4 - c.S2;
        if ((int)c.S0 <= 0) {
            c.A2 = c.S6 + 0u;
            goto L8001B3E4;
        }
        c.A2 = c.S6 + 0u;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4810u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x47F8u;
        c.A3 = 0x00000080u;
        c.RA = 0x8001B3C4u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001B3D0u;
        GranTurismo2ArcadePC.func_8007CF34(c, m);
        c.V1 = c.V0 + 0u;
        MemoryAccess.WriteU16(m, c.V1, (ushort)c.S2);
        MemoryAccess.WriteU16(m, (c.V1 + 0x4u), (ushort)c.S0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x2u), (ushort)c.S3);
        MemoryAccess.WriteU16(m, (c.V1 + 0x6u), (ushort)c.S1);
        L8001B3E4: ;
        c.A0 = 0x80050000u;
        L8001B3E8: ;
        c.A0 = c.A0 - 0x4810u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x4808u;
        c.A2 = c.S6 + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x8001B400u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001B40Cu;
        GranTurismo2ArcadePC.func_8007CF34(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = c.S4 + 0u;
        c.A1 = 0u + 0u;
        c.V0 = 0x00000100u;
        MemoryAccess.WriteU16(m, (c.V1 + 0x4u), (ushort)c.V0);
        c.V0 = 0x0000000Cu;
        MemoryAccess.WriteU16(m, c.V1, (ushort)c.S2);
        MemoryAccess.WriteU16(m, (c.V1 + 0x2u), (ushort)c.S3);
        MemoryAccess.WriteU16(m, (c.V1 + 0x6u), (ushort)c.V0);
        c.RA = 0x8001B434u;
        GranTurismo2ArcadePC.func_8007D954(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x74u));
        c.FP = MemoryAccess.ReadU32(m, (c.SP + 0x70u));
        c.S7 = MemoryAccess.ReadU32(m, (c.SP + 0x6Cu));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x68u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x64u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x60u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x5Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x58u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x54u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x50u));
        c.SP = c.SP + 0x78u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001B464(CpuContext c, IMemory m)
    {
        c.V0 = MemoryAccess.ReadU32(m, c.A0);
        c.V0 = (int)c.V0 < 256 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = 0x000000FFu;
            goto L8001B47C;
        }
        c.V0 = 0x000000FFu;
        MemoryAccess.WriteU32(m, c.A0, c.V0);
        L8001B47C: ;
        c.V0 = MemoryAccess.ReadU32(m, c.A0);
        c.V0 = (int)c.V0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L8001B494;
        }
        c.V0 = 0x00000002u;
        MemoryAccess.WriteU32(m, c.A0, c.V0);
        L8001B494: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.A0 + 0x4u));
        c.V1 = MemoryAccess.ReadU32(m, c.A0);
        c.V0 = (int)c.V0 < (int)c.V1 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = c.V1 - 0x1u;
            goto L8001B4B0;
        }
        c.V0 = c.V1 - 0x1u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.V0);
        L8001B4B0: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.A0 + 0x4u));
        if ((int)c.V0 > 0) {
            c.V0 = 0x00000001u;
            goto L8001B4C4;
        }
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.V0);
        L8001B4C4: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001B4CC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.A1);
        c.RA = 0x8001B4DCu;
        GranTurismo2ArcadePC.func_8001B464(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001B4EC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU32(m, c.A0, c.A1);
        c.RA = 0x8001B4FCu;
        GranTurismo2ArcadePC.func_8001B464(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001B50C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x58u;
        MemoryAccess.WriteU32(m, (c.SP + 0x4Cu), c.S5);
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x68u));
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x70u));
        MemoryAccess.WriteU32(m, (c.SP + 0x3Cu), c.S1);
        c.S1 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x50u), c.S6);
        c.S6 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.S3);
        c.S3 = c.A3 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x48u), c.S4);
        c.S4 = 0x00000080u;
        MemoryAccess.WriteU32(m, (c.SP + 0x54u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.S2);
        if ((int)c.V0 < 0) {
            MemoryAccess.WriteU32(m, (c.SP + 0x38u), c.S0);
            goto L8001B550;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x38u), c.S0);
        c.S4 = 0x00000180u;
        L8001B550: ;
        c.A0 = c.SP + 0x18u;
        c.A1 = 0x0000001Eu;
        c.RA = 0x8001B55Cu;
        GranTurismo2ArcadePC.func_8006AB78(c, m);
        c.V1 = 0xFF9F0000u;
        c.V1 = c.V1 | 0xFFFFu;
        c.S0 = c.SP + 0x18u;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0xCu));
        c.A1 = c.A1 - 0x6CE0u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x10u), c.S1);
        c.V0 = c.V0 & c.V1;
        c.V1 = 0x00200000u;
        c.V0 = c.V0 | c.V1;
        MemoryAccess.WriteU32(m, (c.S0 + 0xCu), c.V0);
        c.RA = 0x8001B590u;
        GranTurismo2ArcadePC.func_8007D990(c, m);
        c.S1 = 0x80050000u;
        c.S1 = c.S1 - 0x4810u;
        c.A0 = c.S1 + 0u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x47E4u;
        c.A2 = c.S5 + 0u;
        c.A3 = c.S4 + 0u;
        c.RA = 0x8001B5B0u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S0 + 0u;
        c.A2 = c.S6 - 0x76u;
        c.S3 = c.S3 + 0x2Cu;
        c.A3 = c.S3 + 0u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0x6Cu));
        c.S2 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S2);
        c.RA = 0x8001B5D4u;
        GranTurismo2ArcadePC.func_8006ABA0(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6D40u;
        c.RA = 0x8001B5E4u;
        GranTurismo2ArcadePC.func_8007D990(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x47ECu;
        c.A2 = c.S5 + 0u;
        c.A3 = c.S4 + 0u;
        c.RA = 0x8001B5FCu;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x612Fu;
        c.A2 = c.S6 + 0x6Eu;
        c.A3 = c.S3 + 0u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S2);
        c.RA = 0x8001B61Cu;
        GranTurismo2ArcadePC.func_8006AD38(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x54u));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x50u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x4Cu));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x48u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x44u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x40u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x3Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x38u));
        c.SP = c.SP + 0x58u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001B644(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0xB0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x88u), c.S0);
        c.S0 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x98u), c.S4);
        c.S4 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x9Cu), c.S5);
        c.S5 = c.A3 + 0u;
        c.V1 = MemoryAccess.ReadU32(m, (c.SP + 0xC4u));
        c.T0 = 0x00000080u;
        MemoryAccess.WriteU32(m, (c.SP + 0xA0u), c.S6);
        c.S6 = c.T0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0xACu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0xA8u), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0xA4u), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x94u), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x90u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x8Cu), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0xB0u), c.A0);
        if ((int)c.V1 < 0) {
            MemoryAccess.WriteU32(m, (c.SP + 0x80u), c.T0);
            goto L8001B6C8;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x80u), c.T0);
        c.T0 = 0x00000180u;
        MemoryAccess.WriteU32(m, (c.SP + 0x80u), c.T0);
        if (c.V1 == 0u) {
            c.S6 = c.T0 + 0u;
            goto L8001B6B8;
        }
        c.S6 = c.T0 + 0u;
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            c.A0 = c.SP + 0x20u;
            goto L8001B6C0;
        }
        c.A0 = c.SP + 0x20u;
        goto L8001B6CC;
        L8001B6B8: ;
        c.S6 = 0x00000080u;
        goto L8001B6C8;
        L8001B6C0: ;
        c.T0 = 0x00000080u;
        MemoryAccess.WriteU32(m, (c.SP + 0x80u), c.T0);
        L8001B6C8: ;
        c.A0 = c.SP + 0x20u;
        L8001B6CC: ;
        c.A1 = 0x0000001Eu;
        c.RA = 0x8001B6D4u;
        GranTurismo2ArcadePC.func_8006AB78(c, m);
        c.V1 = 0xFF9F0000u;
        c.V1 = c.V1 | 0xFFFFu;
        c.S2 = c.SP + 0x20u;
        c.A0 = c.S2 + 0u;
        c.A1 = 0x801C0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S2 + 0xCu));
        c.A1 = c.A1 - 0x6CE0u;
        MemoryAccess.WriteU32(m, (c.S2 + 0x10u), c.S0);
        c.V0 = c.V0 & c.V1;
        c.V1 = 0x00200000u;
        c.V0 = c.V0 | c.V1;
        MemoryAccess.WriteU32(m, (c.S2 + 0xCu), c.V0);
        c.RA = 0x8001B708u;
        GranTurismo2ArcadePC.func_8007D990(c, m);
        c.S3 = 0x80050000u;
        c.S3 = c.S3 - 0x4810u;
        c.A0 = c.S3 + 0u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x47E4u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0xC0u));
        c.A3 = 0x00000080u;
        c.RA = 0x8001B728u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S2 + 0u;
        c.A2 = c.S4 - 0x76u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xC8u));
        c.A3 = c.S5 + 0x2Cu;
        MemoryAccess.WriteU32(m, (c.S2 + 0x14u), c.V0);
        c.T0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        c.RA = 0x8001B748u;
        GranTurismo2ArcadePC.func_8006ABA0(c, m);
        c.A0 = c.S2 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6D40u;
        c.RA = 0x8001B758u;
        GranTurismo2ArcadePC.func_8007D990(c, m);
        c.FP = c.S4 + 0x48u;
        c.S5 = c.S5 + 0x1Eu;
        c.A0 = c.S3 + 0u;
        c.S1 = 0x80050000u;
        c.S1 = c.S1 - 0x47ECu;
        c.A1 = c.S1 + 0u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0xC0u));
        c.A3 = c.S6 + 0u;
        c.RA = 0x8001B77Cu;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S2 + 0u;
        c.S0 = 0x801C0000u;
        c.S0 = c.S0 - 0x613Au;
        c.A1 = c.S0 + 0u;
        c.A2 = c.FP + 0u;
        c.A3 = c.S5 + 0u;
        MemoryAccess.WriteU32(m, (c.S2 + 0x14u), c.V0);
        c.T0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        c.RA = 0x8001B7A4u;
        GranTurismo2ArcadePC.func_8006AD38(c, m);
        c.A0 = c.S3 + 0u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0xC0u));
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0x80u));
        c.A1 = c.S1 + 0u;
        c.RA = 0x8001B7B8u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S2 + 0u;
        c.A1 = c.S0 + 0x4u;
        c.A2 = c.FP + 0u;
        c.S7 = c.S5 + 0x14u;
        c.A3 = c.S7 + 0u;
        MemoryAccess.WriteU32(m, (c.S2 + 0x14u), c.V0);
        c.T0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        c.RA = 0x8001B7DCu;
        GranTurismo2ArcadePC.func_8006AD38(c, m);
        c.FP = c.S4 + 0x6Eu;
        c.S4 = c.SP + 0x40u;
        c.A0 = c.S4 + 0u;
        c.S1 = 0x80020000u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xB0u));
        c.S1 = c.S1 + 0x2478u;
        c.A2 = MemoryAccess.ReadU32(m, c.T0);
        c.A1 = c.S1 + 0u;
        c.RA = 0x8001B800u;
        GranTurismo2ArcadePC.func_8008CE44(c, m);
        c.A0 = c.S3 + 0u;
        c.S0 = 0x80050000u;
        c.S0 = c.S0 - 0x47E8u;
        c.A1 = c.S0 + 0u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0xC0u));
        c.A3 = c.S6 + 0u;
        c.RA = 0x8001B81Cu;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S2 + 0u;
        c.A1 = c.S4 + 0u;
        c.A2 = c.FP + 0u;
        c.A3 = c.S5 + 0u;
        MemoryAccess.WriteU32(m, (c.S2 + 0x14u), c.V0);
        c.T0 = 0x00000001u;
        c.S5 = 0xFFFFFFFEu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.RA = 0x8001B848u;
        GranTurismo2ArcadePC.func_8006B094(c, m);
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xB0u));
        c.A0 = c.S4 + 0u;
        c.A2 = MemoryAccess.ReadU32(m, (c.T0 + 0x4u));
        c.A1 = c.S1 + 0u;
        c.RA = 0x8001B85Cu;
        GranTurismo2ArcadePC.func_8008CE44(c, m);
        c.A0 = c.S3 + 0u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0xC0u));
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0x80u));
        c.A1 = c.S0 + 0u;
        c.RA = 0x8001B870u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S2 + 0u;
        c.A1 = c.S4 + 0u;
        c.A2 = c.FP + 0u;
        c.A3 = c.S7 + 0u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.V0);
        c.T0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.RA = 0x8001B898u;
        GranTurismo2ArcadePC.func_8006B094(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0xACu));
        c.FP = MemoryAccess.ReadU32(m, (c.SP + 0xA8u));
        c.S7 = MemoryAccess.ReadU32(m, (c.SP + 0xA4u));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0xA0u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x9Cu));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x98u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x94u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x90u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x8Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x88u));
        c.SP = c.SP + 0xB0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001B8C8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x60u;
        MemoryAccess.WriteU32(m, (c.SP + 0x58u), c.FP);
        c.FP = MemoryAccess.ReadU32(m, (c.SP + 0x70u));
        MemoryAccess.WriteU32(m, (c.SP + 0x48u), c.S4);
        c.S4 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x50u), c.S6);
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x74u));
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x7Cu));
        MemoryAccess.WriteU32(m, (c.SP + 0x5Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x54u), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x4Cu), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x3Cu), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x38u), c.S0);
        c.V1 = MemoryAccess.ReadU32(m, (c.A0 + 0x4u));
        c.T0 = MemoryAccess.ReadU32(m, c.A0);
        c.S1 = c.A3 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.A2);
        c.A2 = c.S1 + 0u;
        c.S5 = c.S1 - 0x80u;
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x78u));
        c.A3 = c.FP + 0u;
        c.S7 = c.FP - 0x6u;
        c.T1 = c.S5 + c.T1;
        c.S3 = c.S5 + c.V1;
        c.T0 = c.S5 + c.T0;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.T1);
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.T0);
        c.RA = 0x8001B948u;
        GranTurismo2ArcadePC.func_8001B644(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4810u;
        c.S0 = 0x80050000u;
        c.S0 = c.S0 - 0x4804u;
        c.A1 = c.S0 + 0u;
        c.A2 = c.S6 + 0u;
        c.A3 = 0x00000080u;
        c.V0 = c.S1 - 0x81u;
        MemoryAccess.WriteU16(m, (c.SP + 0x20u), (ushort)c.V0);
        c.V0 = 0x00000102u;
        MemoryAccess.WriteU16(m, (c.SP + 0x24u), (ushort)c.V0);
        c.V0 = c.FP - 0x7u;
        MemoryAccess.WriteU16(m, (c.SP + 0x22u), (ushort)c.V0);
        c.V0 = 0x0000000Eu;
        MemoryAccess.WriteU16(m, (c.SP + 0x26u), (ushort)c.V0);
        c.RA = 0x8001B988u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.S2 = c.SP + 0x20u;
        c.A2 = c.S2 + 0u;
        c.RA = 0x8001B99Cu;
        GranTurismo2ArcadePC.func_8007E690(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4810u;
        c.A1 = c.S0 + 0u;
        c.A2 = c.S6 + 0u;
        c.A3 = 0x00000080u;
        c.S1 = c.S1 - 0x82u;
        c.V0 = 0x00000104u;
        MemoryAccess.WriteU16(m, (c.SP + 0x24u), (ushort)c.V0);
        c.V0 = c.FP - 0x8u;
        MemoryAccess.WriteU16(m, (c.SP + 0x22u), (ushort)c.V0);
        c.V0 = 0x00000010u;
        MemoryAccess.WriteU16(m, (c.SP + 0x20u), (ushort)c.S1);
        MemoryAccess.WriteU16(m, (c.SP + 0x26u), (ushort)c.V0);
        c.RA = 0x8001B9D4u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.A2 = c.S2 + 0u;
        c.RA = 0x8001B9E4u;
        GranTurismo2ArcadePC.func_8007E690(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = 0x00000020u;
        c.RA = 0x8001B9F0u;
        GranTurismo2ArcadePC.func_8007D954(c, m);
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x78u));
        if ((int)c.T1 < 0) {
            c.A0 = 0x80050000u;
            goto L8001BA68;
        }
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x480Cu;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x47F0u;
        c.A2 = c.S6 + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x8001BA18u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001BA24u;
        GranTurismo2ArcadePC.func_8007CF34(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = c.SP + 0x28u;
        c.A1 = c.S4 + 0u;
        c.A3 = c.FP - 0x16u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0x2Cu));
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU16(m, (c.V1 + 0x4u), (ushort)c.V0);
        c.V0 = 0x0000000Cu;
        MemoryAccess.WriteU16(m, (c.V1 + 0x6u), (ushort)c.V0);
        c.V0 = 0x00000003u;
        MemoryAccess.WriteU16(m, (c.V1 + 0x2u), (ushort)c.S7);
        MemoryAccess.WriteU16(m, c.V1, (ushort)c.A2);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.V0 = 0x0000000Eu;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.RA = 0x8001BA68u;
        GranTurismo2ArcadePC.func_8006B898(c, m);
        L8001BA68: ;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4810u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x4800u;
        c.A2 = c.S6 + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x8001BA84u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001BA90u;
        GranTurismo2ArcadePC.func_8007CF34(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4810u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x47FCu;
        c.A2 = c.S6 + 0u;
        c.A3 = 0x00000080u;
        c.V0 = c.S3 - c.S5;
        c.S0 = 0x0000000Cu;
        MemoryAccess.WriteU16(m, c.V1, (ushort)c.S5);
        MemoryAccess.WriteU16(m, (c.V1 + 0x4u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x2u), (ushort)c.S7);
        MemoryAccess.WriteU16(m, (c.V1 + 0x6u), (ushort)c.S0);
        c.RA = 0x8001BAC8u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001BAD4u;
        GranTurismo2ArcadePC.func_8007CF34(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4810u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x4808u;
        c.A2 = c.S6 + 0u;
        MemoryAccess.WriteU16(m, c.V1, (ushort)c.S3);
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x30u));
        c.A3 = 0x00000080u;
        c.V0 = c.T1 - c.S3;
        MemoryAccess.WriteU16(m, (c.V1 + 0x4u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x2u), (ushort)c.S7);
        MemoryAccess.WriteU16(m, (c.V1 + 0x6u), (ushort)c.S0);
        c.RA = 0x8001BB0Cu;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001BB18u;
        GranTurismo2ArcadePC.func_8007CF34(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = c.S4 + 0u;
        c.A1 = 0u + 0u;
        c.V0 = 0x00000100u;
        MemoryAccess.WriteU16(m, c.V1, (ushort)c.S5);
        MemoryAccess.WriteU16(m, (c.V1 + 0x4u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x2u), (ushort)c.S7);
        MemoryAccess.WriteU16(m, (c.V1 + 0x6u), (ushort)c.S0);
        c.RA = 0x8001BB3Cu;
        GranTurismo2ArcadePC.func_8007D954(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x5Cu));
        c.FP = MemoryAccess.ReadU32(m, (c.SP + 0x58u));
        c.S7 = MemoryAccess.ReadU32(m, (c.SP + 0x54u));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x50u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x4Cu));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x48u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x44u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x40u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x3Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x38u));
        c.SP = c.SP + 0x60u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001BB6C_gt2_arcade_overlay_1(CpuContext c, IMemory m)
    {
        c.V1 = 0x801D0000u;
        c.V1 = c.V1 - 0x6CC0u;
        c.V0 = c.A0 << 2;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 1;
        c.V0 = c.V0 + c.V1;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x20u));
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001BB98(CpuContext c, IMemory m)
    {
        c.V1 = 0x801D0000u;
        c.V1 = c.V1 - 0x6CC0u;
        c.V0 = c.A0 << 2;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 1;
        c.V0 = c.V0 + c.V1;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x22u));
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001BBC4(CpuContext c, IMemory m)
    {
        c.V1 = 0x801D0000u;
        c.V1 = c.V1 - 0x6CC0u;
        c.V0 = c.A0 << 2;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 1;
        c.V0 = c.V0 + c.V1;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x23u));
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001BBF0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.RA = 0x8001BC00u;
        Dispatcher.Call(c, m, 0x8001BB6Cu);
        c.V1 = c.V0 + 0u;
        c.V0 = (int)c.V1 < 128 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = 0xFFFFFFFFu;
            goto L8001BC24;
        }
        c.V0 = 0xFFFFFFFFu;
        c.A0 = c.V1 - 0x80u;
        c.V1 = c.A0 < 0x00000004u ? 1u : 0u;
        if (c.V1 == 0u) {
            goto L8001BC24;
        }
        c.V0 = c.A0 + 0u;
        L8001BC24: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001BC34(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.RA = 0x8001BC44u;
        GranTurismo2ArcadePC.func_8001BB98(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = (int)c.V1 < 128 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = 0xFFFFFFFFu;
            goto L8001BC68;
        }
        c.V0 = 0xFFFFFFFFu;
        c.A0 = c.V1 - 0x80u;
        c.V1 = c.A0 < 0x00000004u ? 1u : 0u;
        if (c.V1 == 0u) {
            goto L8001BC68;
        }
        c.V0 = c.A0 + 0u;
        L8001BC68: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001BC78(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.RA = 0x8001BC88u;
        GranTurismo2ArcadePC.func_8001BBC4(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = (int)c.V1 < 128 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = 0xFFFFFFFFu;
            goto L8001BCAC;
        }
        c.V0 = 0xFFFFFFFFu;
        c.A0 = c.V1 - 0x80u;
        c.V1 = c.A0 < 0x00000004u ? 1u : 0u;
        if (c.V1 == 0u) {
            goto L8001BCAC;
        }
        c.V0 = c.A0 + 0u;
        L8001BCAC: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001BCBC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V1 = 0x800B0000u;
        c.V0 = c.A0 << 1;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.A0;
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 - 0x75A0u));
        c.V0 = c.V0 << 2;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V0 = c.V0 + c.V1;
        c.V1 = MemoryAccess.ReadU8(m, (c.V0 + 0xDEu));
        c.V0 = 0x00000002u;
        if (c.V1 != c.V0) {
            goto L8001BD08;
        }
        c.RA = 0x8001BCFCu;
        Dispatcher.Call(c, m, 0x8001BB6Cu);
        c.V0 = (int)c.V0 < 128 ? 1u : 0u;
        c.V0 = c.V0 ^ 0x0001u;
        goto L8001BD0C;
        L8001BD08: ;
        c.V0 = 0u + 0u;
        L8001BD0C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001BD1C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V1 = 0x800B0000u;
        c.V0 = c.A0 << 1;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.A0;
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 - 0x75A0u));
        c.V0 = c.V0 << 2;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V0 = c.V0 + c.V1;
        c.V1 = MemoryAccess.ReadU8(m, (c.V0 + 0xDEu));
        c.V0 = 0x00000002u;
        if (c.V1 != c.V0) {
            goto L8001BD68;
        }
        c.RA = 0x8001BD5Cu;
        GranTurismo2ArcadePC.func_8001BB98(c, m);
        c.V0 = (int)c.V0 < 128 ? 1u : 0u;
        c.V0 = c.V0 ^ 0x0001u;
        goto L8001BD6C;
        L8001BD68: ;
        c.V0 = 0u + 0u;
        L8001BD6C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001BD7C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V1 = 0x800B0000u;
        c.V0 = c.A0 << 1;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.A0;
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 - 0x75A0u));
        c.V0 = c.V0 << 2;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V0 = c.V0 + c.V1;
        c.V1 = MemoryAccess.ReadU8(m, (c.V0 + 0xDEu));
        c.V0 = 0x00000002u;
        if (c.V1 != c.V0) {
            goto L8001BDC8;
        }
        c.RA = 0x8001BDBCu;
        GranTurismo2ArcadePC.func_8001BBC4(c, m);
        c.V0 = (int)c.V0 < 128 ? 1u : 0u;
        c.V0 = c.V0 ^ 0x0001u;
        goto L8001BDCC;
        L8001BDC8: ;
        c.V0 = 0u + 0u;
        L8001BDCC: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001BDDC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        if (c.A2 != 0u) {
            c.V1 = c.A0 + 0u;
            goto L8001BE24;
        }
        c.V1 = c.A0 + 0u;
        c.A2 = 0u + 0u;
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 + 0x2u));
        c.A0 = MemoryAccess.ReadU16(m, (c.V1 + 0x4u));
        c.V1 = MemoryAccess.ReadU16(m, (c.V1 + 0x6u));
        c.V0 = c.V0 + c.A0;
        c.V0 = (uint)((int)c.V0 >> 1);
        c.V1 = c.V1 - c.V0;
        c.A0 = c.A0 - c.V0;
        MemoryAccess.WriteU32(m, (c.A1 + 0x8u), c.A0);
        c.A0 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, c.A1, c.V0);
        MemoryAccess.WriteU32(m, (c.A1 + 0x4u), c.V1);
        c.A1 = 0x000000FFu;
        c.RA = 0x8001BE24u;
        GranTurismo2ArcadePC.func_8001AA68(c, m);
        L8001BE24: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001BE34(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V1 = c.A0 + 0u;
        c.A2 = c.A2 - 0x1u;
        c.V0 = c.A2 < 0x00000003u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A0 = c.A1 + 0u;
            goto L8001BE6C;
        }
        c.A0 = c.A1 + 0u;
        c.V0 = c.A2 << 2;
        c.V0 = c.V1 + c.V0;
        c.V1 = MemoryAccess.ReadU16(m, (c.V0 + 0xAu));
        c.V0 = MemoryAccess.ReadU16(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU32(m, c.A0, c.V1);
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.V0);
        c.RA = 0x8001BE6Cu;
        GranTurismo2ArcadePC.func_8001B464(c, m);
        L8001BE6C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001BE7C(CpuContext c, IMemory m)
    {
        if (c.A2 != 0u) {
            goto L8001BED4;
        }
        c.V0 = MemoryAccess.ReadU32(m, c.A1);
        c.V1 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.V0 = c.V0 - c.V1;
        MemoryAccess.WriteU16(m, c.A0, (ushort)c.V0);
        c.V0 = MemoryAccess.ReadU32(m, c.A1);
        c.V1 = MemoryAccess.ReadU32(m, (c.A1 + 0x8u));
        c.V0 = c.V0 - c.V1;
        MemoryAccess.WriteU16(m, (c.A0 + 0x2u), (ushort)c.V0);
        c.V0 = MemoryAccess.ReadU32(m, c.A1);
        c.V1 = MemoryAccess.ReadU32(m, (c.A1 + 0x8u));
        c.V0 = c.V0 + c.V1;
        MemoryAccess.WriteU16(m, (c.A0 + 0x4u), (ushort)c.V0);
        c.V0 = MemoryAccess.ReadU32(m, c.A1);
        c.V1 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.V0 = c.V0 + c.V1;
        MemoryAccess.WriteU16(m, (c.A0 + 0x6u), (ushort)c.V0);
        L8001BED4: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001BEDC(CpuContext c, IMemory m)
    {
        c.A2 = c.A2 - 0x1u;
        c.V0 = c.A2 < 0x00000003u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.A2 << 2;
            goto L8001BF04;
        }
        c.V0 = c.A2 << 2;
        c.V1 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.V0 = c.A0 + c.V0;
        MemoryAccess.WriteU16(m, (c.V0 + 0x8u), (ushort)c.V1);
        c.V1 = MemoryAccess.ReadU32(m, c.A1);
        MemoryAccess.WriteU16(m, (c.V0 + 0xAu), (ushort)c.V1);
        L8001BF04: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001BF0C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x30u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        c.S4 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S5);
        c.S5 = 0x800B0000u;
        c.V1 = 0x800B0000u;
        c.V0 = c.S2 << 1;
        c.V0 = c.V0 + c.S2;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.S2;
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 - 0x75A0u));
        c.V0 = c.V0 << 2;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.S3 + 0x118Cu), 0u);
        MemoryAccess.WriteU32(m, (c.S4 + 0x1190u), 0u);
        MemoryAccess.WriteU32(m, (c.S5 + 0x1194u), 0u);
        c.V0 = c.V0 + c.V1;
        c.S1 = MemoryAccess.ReadU8(m, (c.V0 + 0xDEu));
        c.V0 = 0x00000002u;
        if (c.S1 == c.V0) {
            c.S0 = 0u + 0u;
            goto L8001BF88;
        }
        c.S0 = 0u + 0u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1188u), 0u);
        c.V0 = c.S0 + 0u;
        goto L8001C0D4;
        L8001BF88: ;
        c.A0 = c.S2 + 0u;
        c.RA = 0x8001BF90u;
        GranTurismo2ArcadePC.func_8001BCBC(c, m);
        if (c.V0 == 0u) {
            c.V1 = 0x800B0000u;
            goto L8001BFEC;
        }
        c.V1 = 0x800B0000u;
        c.S0 = 0x00000001u;
        c.A0 = c.V1 + 0x1108u;
        c.A1 = c.A0 + 0x8u;
        MemoryAccess.WriteU16(m, (c.A1 + 0x2u), (ushort)c.S0);
        c.S0 = 0x00000002u;
        c.A2 = c.A0 + 0x10u;
        MemoryAccess.WriteU16(m, (c.A2 + 0x2u), (ushort)c.S0);
        c.S0 = 0x00000003u;
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.S3 + 0x118Cu), c.V0);
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x60B8u;
        MemoryAccess.WriteU16(m, (c.V1 + 0x1108u), (ushort)0u);
        c.V1 = c.V0 + 0x1Bu;
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.V0);
        c.V0 = c.V0 + 0x3Du;
        MemoryAccess.WriteU16(m, (c.A0 + 0x2u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.A0 + 0x8u), (ushort)0u);
        MemoryAccess.WriteU32(m, (c.A1 + 0x4u), c.V1);
        MemoryAccess.WriteU16(m, (c.A0 + 0x10u), (ushort)0u);
        MemoryAccess.WriteU32(m, (c.A2 + 0x4u), c.V0);
        L8001BFEC: ;
        c.A0 = c.S2 + 0u;
        c.RA = 0x8001BFF4u;
        GranTurismo2ArcadePC.func_8001BD1C(c, m);
        if (c.V0 == 0u) {
            c.A0 = c.S0 << 3;
            goto L8001C044;
        }
        c.A0 = c.S0 << 3;
        c.S0 = c.S0 + 0x1u;
        c.A2 = c.S0 << 3;
        c.S0 = c.S0 + 0x1u;
        c.A1 = 0x00000001u;
        c.V1 = 0x800B0000u;
        c.V1 = c.V1 + 0x1108u;
        c.A0 = c.A0 + c.V1;
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x605Cu;
        c.A2 = c.A2 + c.V1;
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.V0);
        c.V0 = c.V0 + 0x26u;
        MemoryAccess.WriteU32(m, (c.S4 + 0x1190u), c.A1);
        MemoryAccess.WriteU16(m, c.A0, (ushort)c.A1);
        MemoryAccess.WriteU16(m, (c.A0 + 0x2u), (ushort)0u);
        MemoryAccess.WriteU16(m, c.A2, (ushort)c.A1);
        MemoryAccess.WriteU16(m, (c.A2 + 0x2u), (ushort)c.A1);
        MemoryAccess.WriteU32(m, (c.A2 + 0x4u), c.V0);
        L8001C044: ;
        c.A0 = c.S2 + 0u;
        c.RA = 0x8001C04Cu;
        GranTurismo2ArcadePC.func_8001BD7C(c, m);
        if (c.V0 == 0u) {
            c.A0 = c.S0 << 3;
            goto L8001C09C;
        }
        c.A0 = c.S0 << 3;
        c.S0 = c.S0 + 0x1u;
        c.A1 = c.S0 << 3;
        c.S0 = c.S0 + 0x1u;
        c.A2 = 0x00000001u;
        c.V1 = 0x800B0000u;
        c.V1 = c.V1 + 0x1108u;
        c.A0 = c.A0 + c.V1;
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6015u;
        c.A1 = c.A1 + c.V1;
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.V0);
        c.V0 = c.V0 + 0x23u;
        MemoryAccess.WriteU32(m, (c.S5 + 0x1194u), c.A2);
        MemoryAccess.WriteU16(m, c.A0, (ushort)c.S1);
        MemoryAccess.WriteU16(m, (c.A0 + 0x2u), (ushort)0u);
        MemoryAccess.WriteU16(m, c.A1, (ushort)c.S1);
        MemoryAccess.WriteU16(m, (c.A1 + 0x2u), (ushort)c.A2);
        MemoryAccess.WriteU32(m, (c.A1 + 0x4u), c.V0);
        L8001C09C: ;
        c.A0 = c.S0 << 3;
        c.S0 = c.S0 + 0x1u;
        c.V0 = (int)c.S0 < 2 ? 1u : 0u;
        c.V0 = c.V0 ^ 0x0001u;
        c.V1 = 0x800B0000u;
        c.V1 = c.V1 + 0x1108u;
        c.A0 = c.A0 + c.V1;
        c.V1 = 0x00000003u;
        MemoryAccess.WriteU16(m, c.A0, (ushort)c.V1);
        c.V1 = 0x801C0000u;
        c.V1 = c.V1 - 0x60C5u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.V1);
        c.V1 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x1188u), c.S0);
        L8001C0D4: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x30u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001C0F8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A1 + 0u;
        c.A0 = c.S1 + 0u;
        c.V0 = 0x00000003u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        MemoryAccess.WriteU16(m, (c.S2 + 0x4u), (ushort)c.V0);
        c.V0 = 0x801D0000u;
        c.V0 = c.V0 - 0x6CC0u;
        c.S0 = c.S1 << 2;
        c.S0 = c.S0 + c.S1;
        c.S0 = c.S0 << 3;
        c.S0 = c.S0 + c.S1;
        c.S0 = c.S0 << 1;
        c.S0 = c.S0 + c.V0;
        c.S0 = c.S0 + 0x48u;
        MemoryAccess.WriteU16(m, (c.S2 + 0x2u), (ushort)c.S1);
        c.RA = 0x8001C14Cu;
        GranTurismo2ArcadePC.func_8001BBF0(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S2 + 0x8u;
        c.A2 = c.V0 + 0u;
        c.RA = 0x8001C15Cu;
        GranTurismo2ArcadePC.func_8001BDDC(c, m);
        c.A0 = c.S1 + 0u;
        c.RA = 0x8001C164u;
        GranTurismo2ArcadePC.func_8001BC34(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S2 + 0x1Cu;
        c.A2 = c.V0 + 0u;
        c.RA = 0x8001C174u;
        GranTurismo2ArcadePC.func_8001BE34(c, m);
        c.A0 = c.S1 + 0u;
        c.RA = 0x8001C17Cu;
        GranTurismo2ArcadePC.func_8001BC78(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S2 + 0x24u;
        c.A2 = c.V0 + 0u;
        c.RA = 0x8001C18Cu;
        GranTurismo2ArcadePC.func_8001BE34(c, m);
        c.V0 = 0x80050000u;
        c.T1 = c.V0 - 0x47DCu;
        c.V1 = MemoryAccess.ReadU32(m, c.T1);
        c.A3 = MemoryAccess.ReadU32(m, (c.T1 + 0x4u));
        c.T0 = MemoryAccess.ReadU32(m, (c.T1 + 0x8u));
        MemoryAccess.WriteU32(m, (c.S2 + 0x2Cu), c.V1);
        MemoryAccess.WriteU32(m, (c.S2 + 0x30u), c.A3);
        MemoryAccess.WriteU32(m, (c.S2 + 0x34u), c.T0);
        c.V1 = MemoryAccess.ReadU32(m, (c.T1 + 0xCu));
        c.A3 = MemoryAccess.ReadU32(m, (c.T1 + 0x10u));
        c.T0 = MemoryAccess.ReadU32(m, (c.T1 + 0x14u));
        MemoryAccess.WriteU32(m, (c.S2 + 0x38u), c.V1);
        MemoryAccess.WriteU32(m, (c.S2 + 0x3Cu), c.A3);
        MemoryAccess.WriteU32(m, (c.S2 + 0x40u), c.T0);
        c.V1 = MemoryAccess.ReadU32(m, (c.T1 + 0x18u));
        MemoryAccess.WriteU32(m, (c.S2 + 0x44u), c.V1);
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU16(m, (c.S2 + 0x44u), (ushort)0u);
        c.T1 = c.V0 - 0x47C0u;
        c.V1 = MemoryAccess.ReadU32(m, c.T1);
        c.A3 = MemoryAccess.ReadU32(m, (c.T1 + 0x4u));
        c.T0 = MemoryAccess.ReadU32(m, (c.T1 + 0x8u));
        MemoryAccess.WriteU32(m, (c.S2 + 0x48u), c.V1);
        MemoryAccess.WriteU32(m, (c.S2 + 0x4Cu), c.A3);
        MemoryAccess.WriteU32(m, (c.S2 + 0x50u), c.T0);
        c.V1 = MemoryAccess.ReadU32(m, (c.T1 + 0xCu));
        c.A3 = MemoryAccess.ReadU32(m, (c.T1 + 0x10u));
        c.T0 = MemoryAccess.ReadU32(m, (c.T1 + 0x14u));
        MemoryAccess.WriteU32(m, (c.S2 + 0x54u), c.V1);
        MemoryAccess.WriteU32(m, (c.S2 + 0x58u), c.A3);
        MemoryAccess.WriteU32(m, (c.S2 + 0x5Cu), c.T0);
        c.V1 = MemoryAccess.ReadU32(m, (c.T1 + 0x18u));
        MemoryAccess.WriteU32(m, (c.S2 + 0x60u), c.V1);
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU16(m, (c.S2 + 0x60u), (ushort)0u);
        c.T1 = c.V0 - 0x47A4u;
        c.V1 = MemoryAccess.ReadU32(m, c.T1);
        c.A3 = MemoryAccess.ReadU32(m, (c.T1 + 0x4u));
        c.T0 = MemoryAccess.ReadU32(m, (c.T1 + 0x8u));
        MemoryAccess.WriteU32(m, (c.S2 + 0x64u), c.V1);
        MemoryAccess.WriteU32(m, (c.S2 + 0x68u), c.A3);
        MemoryAccess.WriteU32(m, (c.S2 + 0x6Cu), c.T0);
        c.V1 = MemoryAccess.ReadU32(m, (c.T1 + 0xCu));
        c.A3 = MemoryAccess.ReadU32(m, (c.T1 + 0x10u));
        c.T0 = MemoryAccess.ReadU32(m, (c.T1 + 0x14u));
        MemoryAccess.WriteU32(m, (c.S2 + 0x70u), c.V1);
        MemoryAccess.WriteU32(m, (c.S2 + 0x74u), c.A3);
        MemoryAccess.WriteU32(m, (c.S2 + 0x78u), c.T0);
        c.V1 = MemoryAccess.ReadU32(m, (c.T1 + 0x18u));
        MemoryAccess.WriteU32(m, (c.S2 + 0x7Cu), c.V1);
        MemoryAccess.WriteU16(m, (c.S2 + 0x7Cu), (ushort)0u);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001C27C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x2u));
        c.RA = 0x8001C298u;
        GranTurismo2ArcadePC.func_8001C0F8(c, m);
        MemoryAccess.WriteU16(m, (c.S0 + 0x44u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.S0 + 0x60u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.S0 + 0x7Cu), (ushort)0u);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001C2B4(CpuContext c, IMemory m)
    {
        c.V1 = c.A0 + 0x2Cu;
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 + 0x4u));
        c.V0 = ~(0u | c.V0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x18u), (ushort)c.V0);
        c.V1 = c.A0 + 0x48u;
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 + 0x4u));
        c.A0 = c.A0 + 0x64u;
        c.V0 = ~(0u | c.V0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x18u), (ushort)c.V0);
        c.V0 = MemoryAccess.ReadU16(m, (c.A0 + 0x4u));
        c.V0 = ~(0u | c.V0);
        MemoryAccess.WriteU16(m, (c.A0 + 0x18u), (ushort)c.V0);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001C2F0(CpuContext c, IMemory m)
    {
        c.V0 = 0x00000003u;
        MemoryAccess.WriteU16(m, (c.A0 + 0x4u), (ushort)c.V0);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001C2FC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x2u));
        c.RA = 0x8001C31Cu;
        GranTurismo2ArcadePC.func_8001BF0C(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 != 0u) {
            c.V0 = 0u + 0u;
            goto L8001C410;
        }
        c.V0 = 0u + 0u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x1198u), (ushort)0u);
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x119Au), (ushort)0u);
        c.V0 = 0x800B0000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x118Cu));
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU16(m, c.S1, (ushort)0u);
        if (c.V1 == 0u) {
            MemoryAccess.WriteU16(m, (c.V0 + 0x119Cu), (ushort)0u);
            goto L8001C368;
        }
        MemoryAccess.WriteU16(m, (c.V0 + 0x119Cu), (ushort)0u);
        c.V1 = 0x800B0000u;
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6121u;
        MemoryAccess.WriteU16(m, (c.S1 + 0x4u), (ushort)0u);
        MemoryAccess.WriteU32(m, (c.V1 + 0x1100u), c.V0);
        goto L8001C390;
        L8001C368: ;
        c.V0 = 0x00000002u;
        c.V1 = 0x800B0000u;
        MemoryAccess.WriteU16(m, (c.S1 + 0x4u), (ushort)c.V0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.S1);
        c.V1 = c.V1 + 0x1108u;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.V1;
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1100u), c.V1);
        L8001C390: ;
        c.RA = 0x8001C398u;
        GranTurismo2ArcadePC.func_8001A8E4(c, m);
        c.V0 = 0x801D0000u;
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x2u));
        c.V0 = c.V0 - 0x6CC0u;
        c.S0 = c.A0 << 2;
        c.S0 = c.S0 + c.A0;
        c.S0 = c.S0 << 3;
        c.S0 = c.S0 + c.A0;
        c.S0 = c.S0 << 1;
        c.S0 = c.S0 + c.V0;
        c.S0 = c.S0 + 0x48u;
        c.RA = 0x8001C3C4u;
        GranTurismo2ArcadePC.func_8001BBF0(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0x8u;
        c.A2 = c.V0 + 0u;
        c.RA = 0x8001C3D4u;
        GranTurismo2ArcadePC.func_8001BDDC(c, m);
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x2u));
        c.RA = 0x8001C3E0u;
        GranTurismo2ArcadePC.func_8001BC34(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0x1Cu;
        c.A2 = c.V0 + 0u;
        c.RA = 0x8001C3F0u;
        GranTurismo2ArcadePC.func_8001BE34(c, m);
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x2u));
        c.RA = 0x8001C3FCu;
        GranTurismo2ArcadePC.func_8001BC78(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0x24u;
        c.A2 = c.V0 + 0u;
        c.RA = 0x8001C40Cu;
        GranTurismo2ArcadePC.func_8001BE34(c, m);
        c.V0 = 0x00000001u;
        L8001C410: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001C424(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x38u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        c.A0 = c.S1 + 0x2Cu;
        c.V1 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        c.S4 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x2u));
        c.A1 = MemoryAccess.ReadU32(m, (c.V1 - 0x75A0u));
        c.V0 = c.A2 << 1;
        c.V0 = c.V0 + c.A2;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.A2;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + 0xDCu;
        c.S2 = c.A1 + c.V0;
        c.V1 = c.A2 << 4;
        c.V0 = c.V1 + 0x1A4u;
        c.S3 = c.A1 + c.V0;
        c.V0 = c.S4 - c.A2;
        c.V0 = c.V0 << 4;
        c.V0 = c.V0 + 0x1A4u;
        c.S6 = c.A1 + c.V0;
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 + 0x660u;
        c.V1 = c.V1 + c.A2;
        c.V1 = c.V1 << (int)(c.S4 & 31u);
        c.V1 = c.V1 + 0x28u;
        c.S0 = c.V1 + c.V0;
        c.RA = 0x8001C4B8u;
        GranTurismo2ArcadePC.func_8006BD74(c, m);
        c.A0 = c.S1 + 0x48u;
        c.RA = 0x8001C4C0u;
        GranTurismo2ArcadePC.func_8006BD74(c, m);
        c.A0 = c.S1 + 0x64u;
        c.RA = 0x8001C4C8u;
        GranTurismo2ArcadePC.func_8006BD74(c, m);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x4u));
        c.S5 = 0x00000003u;
        if (c.V0 == c.S5) {
            c.S7 = c.S6 + 0u;
            goto L8001CA64;
        }
        c.S7 = c.S6 + 0u;
        c.S2 = MemoryAccess.ReadU8(m, (c.S2 + 0x2u));
        c.V0 = 0x00000002u;
        if (c.S2 != c.V0) {
            c.A0 = 0u + 0u;
            goto L8001C628;
        }
        c.A0 = 0u + 0u;
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x2u));
        c.RA = 0x8001C4F4u;
        GranTurismo2ArcadePC.func_8001BBF0(c, m);
        if ((int)c.V0 < 0) {
            c.V0 = c.V0 + c.S0;
            goto L8001C508;
        }
        c.V0 = c.V0 + c.S0;
        c.V1 = MemoryAccess.ReadU8(m, (c.V0 + 0x4u));
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x1198u), (ushort)c.V1);
        L8001C508: ;
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x2u));
        c.RA = 0x8001C514u;
        GranTurismo2ArcadePC.func_8001BC34(c, m);
        if ((int)c.V0 < 0) {
            c.V0 = c.V0 + c.S0;
            goto L8001C528;
        }
        c.V0 = c.V0 + c.S0;
        c.V1 = MemoryAccess.ReadU8(m, (c.V0 + 0x4u));
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x119Au), (ushort)c.V1);
        L8001C528: ;
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x2u));
        c.RA = 0x8001C534u;
        GranTurismo2ArcadePC.func_8001BC78(c, m);
        if ((int)c.V0 < 0) {
            c.V0 = c.V0 + c.S0;
            goto L8001C548;
        }
        c.V0 = c.V0 + c.S0;
        c.V1 = MemoryAccess.ReadU8(m, (c.V0 + 0x4u));
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x119Cu), (ushort)c.V1);
        L8001C548: ;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x4u));
        if (c.V1 == c.S4) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L8001C5D4;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L8001C570;
        }
        if (c.V1 == 0u) {
            c.V0 = 0xFFFFFFFFu;
            goto L8001C580;
        }
        c.V0 = 0xFFFFFFFFu;
        goto L8001CA68;
        L8001C570: ;
        if (c.V1 == c.S2) {
            c.V0 = 0xFFFFFFFFu;
            goto L8001C63C;
        }
        c.V0 = 0xFFFFFFFFu;
        goto L8001CA68;
        L8001C580: ;
        c.V0 = 0x00010000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.S3 + 0x4u));
        c.V1 = MemoryAccess.ReadU32(m, (c.S6 + 0x4u));
        c.V0 = c.V0 | 0x1000u;
        c.A0 = c.A0 | c.V1;
        c.V0 = c.A0 & c.V0;
        if (c.V0 == 0u) {
            c.V1 = 0x800B0000u;
            goto L8001C61C;
        }
        c.V1 = 0x800B0000u;
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x60F4u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x1100u), c.V0);
        c.V0 = 0x800B0000u;
        c.V1 = MemoryAccess.ReadU16(m, (c.V0 + 0x1198u));
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU16(m, (c.S1 + 0x4u), (ushort)c.S4);
        MemoryAccess.WriteU32(m, (c.V0 + 0x11A0u), c.V1);
        c.RA = 0x8001C5C4u;
        GranTurismo2ArcadePC.func_8001A8E4(c, m);
        c.A0 = 0x00000001u;
        c.RA = 0x8001C5CCu;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.V0 = 0xFFFFFFFFu;
        goto L8001CA68;
        L8001C5D4: ;
        c.V0 = 0x00010000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.S3 + 0x4u));
        c.V1 = MemoryAccess.ReadU32(m, (c.S7 + 0x4u));
        c.V0 = c.V0 | 0x1000u;
        c.A0 = c.A0 | c.V1;
        c.V0 = c.A0 & c.V0;
        if (c.V0 == 0u) {
            c.V0 = 0x800B0000u;
            goto L8001C61C;
        }
        c.V0 = 0x800B0000u;
        c.A0 = c.S1 + 0x8u;
        c.V1 = 0x800B0000u;
        c.A2 = MemoryAccess.ReadU16(m, (c.V0 + 0x1198u));
        c.A1 = MemoryAccess.ReadU32(m, (c.V1 + 0x11A0u));
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V0 + 0x11A4u), c.A2);
        c.RA = 0x8001C610u;
        GranTurismo2ArcadePC.func_8001AA68(c, m);
        c.A0 = 0x00000001u;
        MemoryAccess.WriteU16(m, (c.S1 + 0x4u), (ushort)c.S2);
        goto L8001CA38;
        L8001C61C: ;
        c.V0 = c.A0 & 0x0500u;
        if (c.V0 == 0u) {
            c.A0 = 0x00000002u;
            goto L8001CA64;
        }
        c.A0 = 0x00000002u;
        L8001C628: ;
        c.RA = 0x8001C630u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        MemoryAccess.WriteU16(m, (c.S1 + 0x4u), (ushort)c.S5);
        c.V0 = 0xFFFFFFFEu;
        goto L8001CA68;
        L8001C63C: ;
        c.V1 = 0x800B0000u;
        c.S2 = (uint)(short)MemoryAccess.ReadU16(m, c.S1);
        c.V1 = c.V1 + 0x1108u;
        c.V0 = c.S2 << 3;
        c.V0 = c.V0 + c.V1;
        c.S4 = (uint)(short)MemoryAccess.ReadU16(m, c.V0);
        c.V1 = MemoryAccess.ReadU32(m, (c.S3 + 0x4u));
        if (c.S4 != c.S5) {
            c.S0 = 0u + 0u;
            goto L8001C6E8;
        }
        c.S0 = 0u + 0u;
        c.V0 = c.V1 & 0x0A00u;
        if (c.V0 == 0u) {
            c.V0 = 0x801D0000u;
            goto L8001C6E8;
        }
        c.V0 = 0x801D0000u;
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x2u));
        c.V0 = c.V0 - 0x6CC0u;
        c.S0 = c.A0 << 2;
        c.S0 = c.S0 + c.A0;
        c.S0 = c.S0 << 3;
        c.S0 = c.S0 + c.A0;
        c.S0 = c.S0 << 1;
        c.S0 = c.S0 + c.V0;
        c.S0 = c.S0 + 0x48u;
        c.RA = 0x8001C694u;
        GranTurismo2ArcadePC.func_8001BBF0(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0x8u;
        c.A2 = c.V0 + 0u;
        c.RA = 0x8001C6A4u;
        GranTurismo2ArcadePC.func_8001BE7C(c, m);
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x2u));
        c.RA = 0x8001C6B0u;
        GranTurismo2ArcadePC.func_8001BC34(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0x1Cu;
        c.A2 = c.V0 + 0u;
        c.RA = 0x8001C6C0u;
        GranTurismo2ArcadePC.func_8001BEDC(c, m);
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x2u));
        c.RA = 0x8001C6CCu;
        GranTurismo2ArcadePC.func_8001BC78(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0x24u;
        c.A2 = c.V0 + 0u;
        c.RA = 0x8001C6DCu;
        GranTurismo2ArcadePC.func_8001BEDC(c, m);
        c.V0 = 0u + 0u;
        MemoryAccess.WriteU16(m, (c.S1 + 0x4u), (ushort)c.S4);
        goto L8001CA68;
        L8001C6E8: ;
        c.V0 = 0x00010000u;
        c.V0 = c.V0 | 0x1000u;
        c.V0 = c.V1 & c.V0;
        if (c.V0 == 0u) {
            c.V1 = 0x800B0000u;
            goto L8001C85C;
        }
        c.V1 = 0x800B0000u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.S1);
        c.V1 = c.V1 + 0x1108u;
        c.V0 = c.V0 << 3;
        c.A2 = c.V0 + c.V1;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, c.A2);
        c.T0 = 0x00000001u;
        if (c.V1 == c.T0) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L8001C7C8;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L8001C734;
        }
        c.V0 = 0x00000002u;
        if (c.V1 == 0u) {
            c.V0 = 0x800B0000u;
            goto L8001C744;
        }
        c.V0 = 0x800B0000u;
        goto L8001C85C;
        L8001C734: ;
        if (c.V1 == c.V0) {
            goto L8001C80C;
        }
        goto L8001C85C;
        L8001C744: ;
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x8u));
        c.A3 = MemoryAccess.ReadU16(m, (c.V0 + 0x1198u));
        c.A1 = c.A3 - c.A1;
        if ((int)c.A1 >= 0) {
            c.A0 = c.S1 + 0x8u;
            goto L8001C760;
        }
        c.A0 = c.S1 + 0x8u;
        c.A1 = 0u - c.A1;
        L8001C760: ;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.A2 + 0x2u));
        if (c.V1 == c.T0) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L8001C7A8;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L8001C788;
        }
        c.V0 = 0x00000002u;
        if (c.V1 == 0u) {
            goto L8001C798;
        }
        goto L8001C854;
        L8001C788: ;
        if (c.V1 == c.V0) {
            goto L8001C7B8;
        }
        goto L8001C854;
        L8001C798: ;
        c.A1 = c.A3 + 0u;
        c.RA = 0x8001C7A0u;
        GranTurismo2ArcadePC.func_8001AA48(c, m);
        goto L8001C854;
        L8001C7A8: ;
        c.RA = 0x8001C7B0u;
        GranTurismo2ArcadePC.func_8001AA28(c, m);
        goto L8001C854;
        L8001C7B8: ;
        c.RA = 0x8001C7C0u;
        GranTurismo2ArcadePC.func_8001AA08(c, m);
        goto L8001C854;
        L8001C7C8: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A2 + 0x2u));
        if (c.V0 == 0u) {
            goto L8001C7E8;
        }
        if (c.V0 == c.T0) {
            c.V0 = 0x800B0000u;
            goto L8001C800;
        }
        c.V0 = 0x800B0000u;
        goto L8001C854;
        L8001C7E8: ;
        c.V0 = 0x800B0000u;
        c.A1 = MemoryAccess.ReadU16(m, (c.V0 + 0x119Au));
        c.A0 = c.S1 + 0x1Cu;
        c.RA = 0x8001C7F8u;
        GranTurismo2ArcadePC.func_8001B4EC(c, m);
        goto L8001C854;
        L8001C800: ;
        c.A1 = MemoryAccess.ReadU16(m, (c.V0 + 0x119Au));
        c.A0 = c.S1 + 0x1Cu;
        goto L8001C84C;
        L8001C80C: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A2 + 0x2u));
        if (c.V0 == 0u) {
            goto L8001C82C;
        }
        if (c.V0 == c.T0) {
            c.V0 = 0x800B0000u;
            goto L8001C844;
        }
        c.V0 = 0x800B0000u;
        goto L8001C854;
        L8001C82C: ;
        c.V0 = 0x800B0000u;
        c.A1 = MemoryAccess.ReadU16(m, (c.V0 + 0x119Cu));
        c.A0 = c.S1 + 0x24u;
        c.RA = 0x8001C83Cu;
        GranTurismo2ArcadePC.func_8001B4EC(c, m);
        goto L8001C854;
        L8001C844: ;
        c.A1 = MemoryAccess.ReadU16(m, (c.V0 + 0x119Cu));
        c.A0 = c.S1 + 0x24u;
        L8001C84C: ;
        c.RA = 0x8001C854u;
        GranTurismo2ArcadePC.func_8001B4CC(c, m);
        L8001C854: ;
        c.A0 = 0x00000001u;
        c.RA = 0x8001C85Cu;
        GranTurismo2ArcadePC.func_80060750(c, m);
        L8001C85C: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S3 + 0x4u));
        c.V1 = MemoryAccess.ReadU32(m, (c.S3 + 0xCu));
        c.V1 = c.V0 | c.V1;
        c.V0 = c.V1 & 0x0004u;
        if (c.V0 == 0u) {
            c.V0 = c.V1 & 0x0008u;
            goto L8001C87C;
        }
        c.V0 = c.V1 & 0x0008u;
        c.S0 = c.S0 - 0x1u;
        L8001C87C: ;
        if (c.V0 == 0u) {
            goto L8001C888;
        }
        c.S0 = c.S0 + 0x1u;
        L8001C888: ;
        if (c.S0 == 0u) {
            c.V1 = 0x800B0000u;
            goto L8001C9C8;
        }
        c.V1 = 0x800B0000u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.S1);
        c.V1 = c.V1 + 0x1108u;
        c.V0 = c.V0 << 3;
        c.A0 = c.V0 + c.V1;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, c.A0);
        c.A1 = 0x00000001u;
        if (c.V1 == c.A1) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L8001C950;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L8001C8C8;
        }
        c.V0 = 0x00000002u;
        if (c.V1 == 0u) {
            goto L8001C8D8;
        }
        goto L8001C9C8;
        L8001C8C8: ;
        if (c.V1 == c.V0) {
            goto L8001C97C;
        }
        goto L8001C9C8;
        L8001C8D8: ;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0x2u));
        if (c.V1 == c.A1) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L8001C924;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L8001C900;
        }
        c.V0 = 0x00000002u;
        if (c.V1 == 0u) {
            c.A0 = c.S1 + 0x8u;
            goto L8001C910;
        }
        c.A0 = c.S1 + 0x8u;
        goto L8001C9C0;
        L8001C900: ;
        if (c.V1 == c.V0) {
            c.A0 = c.S1 + 0x8u;
            goto L8001C93C;
        }
        c.A0 = c.S1 + 0x8u;
        goto L8001C9C0;
        L8001C910: ;
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x8u));
        c.A1 = c.A1 + c.S0;
        c.RA = 0x8001C91Cu;
        GranTurismo2ArcadePC.func_8001AA48(c, m);
        goto L8001C9C0;
        L8001C924: ;
        c.A0 = c.S1 + 0x8u;
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0x4u));
        c.A1 = c.A1 + c.S0;
        c.RA = 0x8001C934u;
        GranTurismo2ArcadePC.func_8001AA28(c, m);
        goto L8001C9C0;
        L8001C93C: ;
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.A1 = c.A1 + c.S0;
        c.RA = 0x8001C948u;
        GranTurismo2ArcadePC.func_8001AA08(c, m);
        goto L8001C9C0;
        L8001C950: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0x2u));
        if (c.V0 == 0u) {
            goto L8001C970;
        }
        if (c.V0 == c.A1) {
            c.A0 = c.S1 + 0x1Cu;
            goto L8001C9B4;
        }
        c.A0 = c.S1 + 0x1Cu;
        goto L8001C9C0;
        L8001C970: ;
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x1Cu));
        c.A0 = c.S1 + 0x1Cu;
        goto L8001C9A4;
        L8001C97C: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0x2u));
        if (c.V0 == 0u) {
            goto L8001C99C;
        }
        if (c.V0 == c.A1) {
            c.A0 = c.S1 + 0x24u;
            goto L8001C9B4;
        }
        c.A0 = c.S1 + 0x24u;
        goto L8001C9C0;
        L8001C99C: ;
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x24u));
        c.A0 = c.S1 + 0x24u;
        L8001C9A4: ;
        c.A1 = c.A1 + c.S0;
        c.RA = 0x8001C9ACu;
        GranTurismo2ArcadePC.func_8001B4EC(c, m);
        goto L8001C9C0;
        L8001C9B4: ;
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0x4u));
        c.A1 = c.A1 + c.S0;
        c.RA = 0x8001C9C0u;
        GranTurismo2ArcadePC.func_8001B4CC(c, m);
        L8001C9C0: ;
        c.A0 = 0x00000005u;
        c.RA = 0x8001C9C8u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        L8001C9C8: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S3 + 0x4u));
        c.V1 = MemoryAccess.ReadU32(m, (c.S3 + 0xCu));
        c.V1 = c.V0 | c.V1;
        c.V0 = c.V1 & 0x0001u;
        if (c.V0 == 0u) {
            c.V0 = c.V1 & 0x0002u;
            goto L8001CA04;
        }
        c.V0 = c.V1 & 0x0002u;
        c.S2 = c.S2 - 0x1u;
        if ((int)c.S2 >= 0) {
            goto L8001CA04;
        }
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1188u));
        c.S2 = c.V0 - 0x1u;
        c.V0 = c.V1 & 0x0002u;
        L8001CA04: ;
        if (c.V0 == 0u) {
            c.V0 = 0x800B0000u;
            goto L8001CA24;
        }
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1188u));
        c.S2 = c.S2 + 0x1u;
        c.V0 = (int)c.S2 < (int)c.V0 ? 1u : 0u;
        if (c.V0 != 0u) {
            goto L8001CA24;
        }
        c.S2 = 0u + 0u;
        L8001CA24: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.S1);
        if (c.V0 == c.S2) {
            c.A0 = 0x00000006u;
            goto L8001CA64;
        }
        c.A0 = 0x00000006u;
        MemoryAccess.WriteU16(m, c.S1, (ushort)c.S2);
        L8001CA38: ;
        c.RA = 0x8001CA40u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.V1 = 0x800B0000u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.S1);
        c.V1 = c.V1 + 0x1108u;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.V1;
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1100u), c.V1);
        c.RA = 0x8001CA64u;
        GranTurismo2ArcadePC.func_8001A8E4(c, m);
        L8001CA64: ;
        c.V0 = 0xFFFFFFFFu;
        L8001CA68: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x30u));
        c.S7 = MemoryAccess.ReadU32(m, (c.SP + 0x2Cu));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x38u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001CA94(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x98u;
        MemoryAccess.WriteU32(m, (c.SP + 0x78u), c.S2);
        c.S2 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x74u), c.S1);
        c.S1 = c.A1 + 0u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x84u), c.S5);
        c.S5 = MemoryAccess.ReadU16(m, (c.V0 + 0x1198u));
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU16(m, (c.V0 + 0x119Au));
        c.T0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x88u), c.S6);
        c.S6 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x90u), c.FP);
        c.FP = 0xFFFFFFFFu;
        MemoryAccess.WriteU32(m, (c.SP + 0x50u), c.T0);
        MemoryAccess.WriteU32(m, (c.SP + 0x54u), c.T0);
        MemoryAccess.WriteU32(m, (c.SP + 0x58u), c.T0);
        c.T0 = c.FP + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x7Cu), c.S3);
        c.S3 = c.FP + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x80u), c.S4);
        c.S4 = c.FP + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x68u), c.T0);
        MemoryAccess.WriteU32(m, (c.SP + 0x8Cu), c.S7);
        c.S7 = MemoryAccess.ReadU32(m, (c.SP + 0xA8u));
        c.A0 = c.SP + 0x30u;
        MemoryAccess.WriteU32(m, (c.SP + 0x60u), c.V0);
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU16(m, (c.V0 + 0x119Cu));
        c.A1 = 0x0000001Eu;
        MemoryAccess.WriteU32(m, (c.SP + 0x94u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x70u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0xA0u), c.A2);
        MemoryAccess.WriteU32(m, (c.SP + 0xA4u), c.A3);
        MemoryAccess.WriteU32(m, (c.SP + 0x5Cu), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x6Cu), c.T0);
        MemoryAccess.WriteU32(m, (c.SP + 0x64u), c.V0);
        c.RA = 0x8001CB30u;
        GranTurismo2ArcadePC.func_8006AB78(c, m);
        c.V1 = 0xFF9F0000u;
        c.S0 = c.SP + 0x30u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0xCu));
        c.V1 = c.V1 | 0xFFFFu;
        MemoryAccess.WriteU32(m, (c.S0 + 0x10u), c.S1);
        c.V0 = c.V0 & c.V1;
        c.V1 = 0x00200000u;
        c.V0 = c.V0 | c.V1;
        MemoryAccess.WriteU32(m, (c.S0 + 0xCu), c.V0);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x4u));
        c.V0 = 0x00000002u;
        if (c.V1 == c.V0) {
            c.V0 = (int)c.V1 < 3 ? 1u : 0u;
            goto L8001CB7C;
        }
        c.V0 = (int)c.V1 < 3 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L8001CBF8;
        }
        if ((int)c.V1 < 0) {
            c.V1 = 0x00740000u;
            goto L8001CBF8;
        }
        c.V1 = 0x00740000u;
        c.V1 = c.V1 | 0x7474u;
        goto L8001CBD4;
        L8001CB7C: ;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6D40u;
        c.RA = 0x8001CB8Cu;
        GranTurismo2ArcadePC.func_8007D990(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4810u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x47E0u;
        c.A2 = c.S7 + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x8001CBA8u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x5FD1u;
        c.A2 = 0x000000B0u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x58u));
        c.A3 = 0x00000084u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        c.RA = 0x8001CBCCu;
        GranTurismo2ArcadePC.func_8006ACC4(c, m);
        c.V1 = 0x00740000u;
        c.V1 = c.V1 | 0x7474u;
        L8001CBD4: ;
        c.A0 = c.S1 + 0u;
        c.A2 = 0x000000B0u;
        c.V0 = 0x800B0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1100u));
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x58u));
        c.A3 = 0x000001AEu;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        c.RA = 0x8001CBF8u;
        GranTurismo2ArcadePC.func_8001A7AC(c, m);
        L8001CBF8: ;
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x4u));
        c.V0 = 0x00000001u;
        if (c.A1 == c.V0) {
            c.V0 = (int)c.A1 < 2 ? 1u : 0u;
            goto L8001CC50;
        }
        c.V0 = (int)c.A1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L8001CC20;
        }
        c.V0 = 0x00000002u;
        if (c.A1 == 0u) {
            goto L8001CC38;
        }
        goto L8001CD1C;
        L8001CC20: ;
        if (c.A1 == c.V0) {
            c.V0 = 0x00000003u;
            goto L8001CC70;
        }
        c.V0 = 0x00000003u;
        if (c.A1 == c.V0) {
            c.T0 = 0xFFFFFFFFu;
            goto L8001CD10;
        }
        c.T0 = 0xFFFFFFFFu;
        goto L8001CD1C;
        L8001CC38: ;
        c.S6 = 0x00000002u;
        c.FP = c.S5 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x58u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x54u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x50u), 0u);
        goto L8001CD1C;
        L8001CC50: ;
        c.V0 = 0x800B0000u;
        c.FP = MemoryAccess.ReadU32(m, (c.V0 + 0x11A0u));
        c.S6 = 0x00000002u;
        MemoryAccess.WriteU32(m, (c.SP + 0x58u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x54u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x50u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x68u), c.S5);
        goto L8001CD1C;
        L8001CC70: ;
        c.T0 = 0x00000001u;
        c.V1 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x5Cu), c.T0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.V1 = c.V1 + 0x1108u;
        c.V0 = c.V0 << 3;
        c.A0 = c.V0 + c.V1;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, c.A0);
        if (c.V1 == c.T0) {
            c.S6 = c.T0 + 0u;
            goto L8001CCDC;
        }
        c.S6 = c.T0 + 0u;
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L8001CCB8;
        }
        if (c.V1 == 0u) {
            goto L8001CCD0;
        }
        goto L8001CD1C;
        L8001CCB8: ;
        if (c.V1 == c.A1) {
            c.V0 = 0x00000003u;
            goto L8001CCE8;
        }
        c.V0 = 0x00000003u;
        if (c.V1 == c.V0) {
            goto L8001CCFC;
        }
        goto L8001CD1C;
        L8001CCD0: ;
        c.S3 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0x2u));
        c.S4 = 0x00000040u;
        goto L8001CD04;
        L8001CCDC: ;
        c.S4 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0x2u));
        c.S3 = 0x00000040u;
        goto L8001CD04;
        L8001CCE8: ;
        c.S3 = 0x00000040u;
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0x2u));
        c.S4 = c.S3 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x6Cu), c.A0);
        goto L8001CD1C;
        L8001CCFC: ;
        c.S3 = 0x00000040u;
        c.S4 = c.S3 + 0u;
        L8001CD04: ;
        c.T0 = 0x00000040u;
        MemoryAccess.WriteU32(m, (c.SP + 0x6Cu), c.T0);
        goto L8001CD1C;
        L8001CD10: ;
        c.S5 = c.T0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x64u), c.T0);
        MemoryAccess.WriteU32(m, (c.SP + 0x60u), c.T0);
        L8001CD1C: ;
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x2u));
        c.RA = 0x8001CD28u;
        GranTurismo2ArcadePC.func_8001BCBC(c, m);
        if (c.V0 == 0u) {
            c.A0 = c.S2 + 0x8u;
            goto L8001CD70;
        }
        c.A0 = c.S2 + 0x8u;
        c.A1 = c.S1 + 0u;
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0xA0u));
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xA4u));
        c.A2 = 0x801C0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x68u));
        c.A2 = c.A2 - 0x61EDu;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.T0);
        c.RA = 0x8001CD68u;
        GranTurismo2ArcadePC.func_8001AF2C(c, m);
        goto L8001CD94;
        L8001CD70: ;
        c.A1 = c.S1 + 0u;
        c.V0 = 0x801C0000u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0xA0u));
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0xA4u));
        c.V0 = c.V0 - 0x61EDu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S3);
        c.RA = 0x8001CD94u;
        GranTurismo2ArcadePC.func_8001AAA0(c, m);
        L8001CD94: ;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x50u));
        if (c.T0 == 0u) {
            c.A0 = c.S2 + 0x2Cu;
            goto L8001CDCC;
        }
        c.A0 = c.S2 + 0x2Cu;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xA0u));
        c.A2 = c.T0 - 0x7Au;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xA4u));
        c.A1 = c.S1 + 0u;
        c.A3 = c.T0 + 0x14u;
        c.RA = 0x8001CDC0u;
        GranTurismo2ArcadePC.func_8006BE04(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = 0x00000220u;
        c.RA = 0x8001CDCCu;
        GranTurismo2ArcadePC.func_8007D954(c, m);
        L8001CDCC: ;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x54u));
        if (c.T0 == 0u) {
            goto L8001CF0C;
        }
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x2u));
        c.RA = 0x8001CDE8u;
        GranTurismo2ArcadePC.func_8001BD1C(c, m);
        if (c.V0 == 0u) {
            c.A0 = c.S2 + 0x1Cu;
            goto L8001CEBC;
        }
        c.A0 = c.S2 + 0x1Cu;
        c.A1 = c.S1 + 0u;
        c.A2 = 0x801C0000u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xA4u));
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0xA0u));
        c.V0 = c.T0 + 0x60u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x60u));
        c.A2 = c.A2 - 0x61E4u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.T0);
        c.RA = 0x8001CE20u;
        GranTurismo2ArcadePC.func_8001B8C8(c, m);
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x2u));
        c.RA = 0x8001CE2Cu;
        GranTurismo2ArcadePC.func_8001BB98(c, m);
        c.A0 = c.V0 + 0u;
        c.RA = 0x8001CE34u;
        GranTurismo2ArcadePC.func_80018F74(c, m);
        c.S0 = c.V0 + 0u;
        if (c.S0 == 0u) {
            c.A0 = c.S1 + 0u;
            goto L8001CEE4;
        }
        c.A0 = c.S1 + 0u;
        c.A1 = c.S7 << 8;
        c.A1 = c.S7 | c.A1;
        c.V0 = c.S7 << 16;
        c.A1 = c.A1 | c.V0;
        c.RA = 0x8001CE54u;
        GranTurismo2ArcadePC.func_80081388(c, m);
        c.A0 = MemoryAccess.ReadU16(m, (c.S0 + 0x4u));
        c.V1 = MemoryAccess.ReadU16(m, (c.S0 + 0x6u));
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xA0u));
        c.A0 = c.A0 << 16;
        c.A0 = (uint)((int)c.A0 >> 17);
        c.A0 = c.T0 - c.A0;
        c.V1 = c.V1 << 16;
        c.V1 = (uint)((int)c.V1 >> 17);
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xA4u));
        c.V1 = c.V1 - 0x84u;
        c.V1 = c.T0 - c.V1;
        c.V1 = c.V1 << 16;
        c.A0 = c.A0 + c.V1;
        MemoryAccess.WriteU32(m, c.V0, c.A0);
        c.V1 = MemoryAccess.ReadU32(m, c.S0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x4u), c.V1);
        c.V1 = MemoryAccess.ReadU32(m, (c.S0 + 0x4u));
        MemoryAccess.WriteU32(m, (c.V0 + 0x8u), c.V1);
        c.A1 = MemoryAccess.ReadU16(m, (c.S0 + 0x8u));
        c.A0 = c.S1 + 0u;
        c.A1 = c.A1 | 0x0020u;
        c.RA = 0x8001CEB4u;
        GranTurismo2ArcadePC.func_8007D954(c, m);
        goto L8001CEE4;
        L8001CEBC: ;
        c.A1 = c.S1 + 0u;
        c.V0 = 0x801C0000u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0xA0u));
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xA4u));
        c.V0 = c.V0 - 0x61E4u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S4);
        c.A3 = c.T0 + 0x60u;
        c.RA = 0x8001CEE4u;
        GranTurismo2ArcadePC.func_8001B50C(c, m);
        L8001CEE4: ;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xA0u));
        c.A0 = c.S2 + 0x48u;
        c.A2 = c.T0 - 0x7Au;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xA4u));
        c.A1 = c.S1 + 0u;
        c.A3 = c.T0 + 0x74u;
        c.RA = 0x8001CF00u;
        GranTurismo2ArcadePC.func_8006BE04(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = 0x00000220u;
        c.RA = 0x8001CF0Cu;
        GranTurismo2ArcadePC.func_8007D954(c, m);
        L8001CF0C: ;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x58u));
        if (c.T0 == 0u) {
            goto L8001D054;
        }
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x2u));
        c.RA = 0x8001CF28u;
        GranTurismo2ArcadePC.func_8001BD7C(c, m);
        if (c.V0 == 0u) {
            c.A0 = c.S2 + 0x24u;
            goto L8001D000;
        }
        c.A0 = c.S2 + 0x24u;
        c.A1 = c.S1 + 0u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xA4u));
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0xA0u));
        c.V0 = c.T0 + 0xB4u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x64u));
        c.A2 = 0x801C0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.T0);
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x6Cu));
        c.A2 = c.A2 - 0x61D7u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.T0);
        c.RA = 0x8001CF64u;
        GranTurismo2ArcadePC.func_8001B8C8(c, m);
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x2u));
        c.RA = 0x8001CF70u;
        GranTurismo2ArcadePC.func_8001BBC4(c, m);
        c.A0 = c.V0 + 0u;
        c.RA = 0x8001CF78u;
        GranTurismo2ArcadePC.func_80018F74(c, m);
        c.S0 = c.V0 + 0u;
        if (c.S0 == 0u) {
            c.A0 = c.S1 + 0u;
            goto L8001D02C;
        }
        c.A0 = c.S1 + 0u;
        c.A1 = c.S7 << 8;
        c.A1 = c.S7 | c.A1;
        c.V0 = c.S7 << 16;
        c.A1 = c.A1 | c.V0;
        c.RA = 0x8001CF98u;
        GranTurismo2ArcadePC.func_80081388(c, m);
        c.A0 = MemoryAccess.ReadU16(m, (c.S0 + 0x4u));
        c.V1 = MemoryAccess.ReadU16(m, (c.S0 + 0x6u));
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xA0u));
        c.A0 = c.A0 << 16;
        c.A0 = (uint)((int)c.A0 >> 17);
        c.A0 = c.T0 - c.A0;
        c.V1 = c.V1 << 16;
        c.V1 = (uint)((int)c.V1 >> 17);
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xA4u));
        c.V1 = c.V1 - 0xD8u;
        c.V1 = c.T0 - c.V1;
        c.V1 = c.V1 << 16;
        c.A0 = c.A0 + c.V1;
        MemoryAccess.WriteU32(m, c.V0, c.A0);
        c.V1 = MemoryAccess.ReadU32(m, c.S0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x4u), c.V1);
        c.V1 = MemoryAccess.ReadU32(m, (c.S0 + 0x4u));
        MemoryAccess.WriteU32(m, (c.V0 + 0x8u), c.V1);
        c.A1 = MemoryAccess.ReadU16(m, (c.S0 + 0x8u));
        c.A0 = c.S1 + 0u;
        c.A1 = c.A1 | 0x0020u;
        c.RA = 0x8001CFF8u;
        GranTurismo2ArcadePC.func_8007D954(c, m);
        goto L8001D02C;
        L8001D000: ;
        c.A1 = c.S1 + 0u;
        c.V0 = 0x801C0000u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xA4u));
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0xA0u));
        c.A3 = c.T0 + 0xB4u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x6Cu));
        c.V0 = c.V0 - 0x61D7u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.T0);
        c.RA = 0x8001D02Cu;
        GranTurismo2ArcadePC.func_8001B50C(c, m);
        L8001D02C: ;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xA0u));
        c.A0 = c.S2 + 0x64u;
        c.A2 = c.T0 - 0x7Au;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xA4u));
        c.A1 = c.S1 + 0u;
        c.A3 = c.T0 + 0xC8u;
        c.RA = 0x8001D048u;
        GranTurismo2ArcadePC.func_8006BE04(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = 0x00000220u;
        c.RA = 0x8001D054u;
        GranTurismo2ArcadePC.func_8007D954(c, m);
        L8001D054: ;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x5Cu));
        if (c.T0 == 0u) {
            c.V1 = 0x800B0000u;
            goto L8001D0EC;
        }
        c.V1 = 0x800B0000u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.V1 = c.V1 + 0x1108u;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.V1;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, c.V0);
        c.V0 = 0x00000003u;
        if (c.V1 != c.V0) {
            c.S0 = 0x00000180u;
            goto L8001D088;
        }
        c.S0 = 0x00000180u;
        c.S0 = 0x00000080u;
        L8001D088: ;
        c.A0 = c.SP + 0x30u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6D40u;
        c.RA = 0x8001D098u;
        GranTurismo2ArcadePC.func_8007D990(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4810u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x47ECu;
        c.A2 = c.S7 + 0u;
        c.A3 = c.S0 + 0u;
        c.RA = 0x8001D0B4u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.SP + 0x30u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xA0u));
        c.A1 = 0x801C0000u;
        c.A2 = c.T0 + 0x48u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xA4u));
        c.A1 = c.A1 - 0x61FAu;
        c.A3 = c.T0 - 0x10u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.V0);
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.V0 = 0xFFFFFFFEu;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.RA = 0x8001D0ECu;
        GranTurismo2ArcadePC.func_8006B094(c, m);
        L8001D0EC: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x94u));
        c.FP = MemoryAccess.ReadU32(m, (c.SP + 0x90u));
        c.S7 = MemoryAccess.ReadU32(m, (c.SP + 0x8Cu));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x88u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x84u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x80u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x7Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x78u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x74u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x70u));
        c.SP = c.SP + 0x98u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001D11C(CpuContext c, IMemory m)
    {
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU8(m, (c.A0 + 0x9u), (byte)0u);
        MemoryAccess.WriteU16(m, (c.A0 + 0xAu), (ushort)c.V0);
        MemoryAccess.WriteU8(m, (c.A0 + 0xFu), (byte)0u);
        MemoryAccess.WriteU8(m, (c.A0 + 0x10u), (byte)0u);
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.A1);
        MemoryAccess.WriteU32(m, (c.A0 + 0x18u), c.A2);
        MemoryAccess.WriteU8(m, (c.A0 + 0x11u), (byte)0u);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001D140(CpuContext c, IMemory m)
    {
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU16(m, (c.A0 + 0xAu), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.A0 + 0xCu), (ushort)0u);
        MemoryAccess.WriteU8(m, (c.A0 + 0xEu), (byte)c.V0);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001D154(CpuContext c, IMemory m)
    {
        c.V0 = 0xFFFFFFEFu;
        MemoryAccess.WriteU16(m, (c.A0 + 0xAu), (ushort)c.V0);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001D160(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x48u;
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S0);
        c.S0 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x3Cu), c.S5);
        c.S5 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x38u), c.S4);
        c.S4 = c.A3 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x34u), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0xAu));
        c.V1 = MemoryAccess.ReadU16(m, (c.S1 + 0xAu));
        if ((int)c.V0 >= 0) {
            c.S3 = 0xFFFFFFFEu;
            goto L8001D1BC;
        }
        c.S3 = 0xFFFFFFFEu;
        c.V0 = (int)c.V0 < -1 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.V1 + 0x1u;
            goto L8001D268;
        }
        c.V0 = c.V1 + 0x1u;
        MemoryAccess.WriteU16(m, (c.S1 + 0xAu), (ushort)c.V0);
        goto L8001D268;
        L8001D1BC: ;
        c.V0 = c.V1 + 0x1u;
        MemoryAccess.WriteU16(m, (c.S1 + 0xAu), (ushort)c.V0);
        c.V0 = c.V0 << 16;
        c.V0 = (uint)((int)c.V0 >> 16);
        c.V0 = (int)c.V0 < 57 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = 0x0000000Cu;
            goto L8001D1DC;
        }
        c.V0 = 0x0000000Cu;
        MemoryAccess.WriteU16(m, (c.S1 + 0xAu), (ushort)c.V0);
        L8001D1DC: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0xCu));
        c.V1 = MemoryAccess.ReadU16(m, (c.S1 + 0xCu));
        if ((int)c.V0 <= 0) {
            c.V0 = c.V1 - 0x1u;
            goto L8001D1F0;
        }
        c.V0 = c.V1 - 0x1u;
        MemoryAccess.WriteU16(m, (c.S1 + 0xCu), (ushort)c.V0);
        L8001D1F0: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0xCu));
        c.V1 = MemoryAccess.ReadU16(m, (c.S1 + 0xCu));
        if ((int)c.V0 >= 0) {
            goto L8001D210;
        }
        c.V0 = c.V1 + 0x1u;
        MemoryAccess.WriteU16(m, (c.S1 + 0xCu), (ushort)c.V0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0xCu));
        L8001D210: ;
        if (c.V0 != 0u) {
            c.V0 = 0xFFFFFFFFu;
            goto L8001D21C;
        }
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU8(m, (c.S1 + 0xEu), (byte)c.V0);
        L8001D21C: ;
        c.A2 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0xEu));
        if ((int)c.A2 < 0) {
            c.A0 = 0u + 0u;
            goto L8001D23C;
        }
        c.A0 = 0u + 0u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x14u));
        c.A1 = c.SP + 0x10u;
        c.RA = 0x8001D23Cu;
        Dispatcher.Call(c, m, c.V0);
        L8001D23C: ;
        c.A0 = 0u + 0u;
        c.A2 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0x9u));
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x14u));
        c.A1 = c.SP + 0x10u;
        c.RA = 0x8001D254u;
        Dispatcher.Call(c, m, c.V0);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0x11u));
        if (c.V0 == 0u) {
            c.V1 = 0u < c.S0 ? 1u : 0u;
            goto L8001D270;
        }
        c.V1 = 0u < c.S0 ? 1u : 0u;
        MemoryAccess.WriteU8(m, (c.S1 + 0x11u), (byte)0u);
        L8001D268: ;
        c.V0 = 0xFFFFFFFEu;
        goto L8001D39C;
        L8001D270: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0x10u));
        c.V0 = c.V0 < 0x00000001u ? 1u : 0u;
        c.V1 = c.V1 & c.V0;
        if (c.V1 == 0u) {
            MemoryAccess.WriteU8(m, (c.S1 + 0xFu), (byte)0u);
            goto L8001D398;
        }
        MemoryAccess.WriteU8(m, (c.S1 + 0xFu), (byte)0u);
        c.S2 = MemoryAccess.ReadU32(m, (c.S0 + 0x4u));
        c.V0 = c.S2 & 0x0500u;
        if (c.V0 != 0u) {
            c.V0 = 0xFFFFFFFFu;
            goto L8001D39C;
        }
        c.V0 = 0xFFFFFFFFu;
        if (c.S4 != 0u) {
            c.V0 = c.S2 & 0x0A00u;
            goto L8001D2B0;
        }
        c.V0 = c.S2 & 0x0A00u;
        c.S2 = MemoryAccess.ReadU32(m, (c.S5 + 0x4u));
        c.V0 = c.S2 & 0x0A00u;
        L8001D2B0: ;
        if (c.V0 == 0u) {
            c.A0 = 0x00000003u;
            goto L8001D2D8;
        }
        c.A0 = 0x00000003u;
        c.A2 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0x9u));
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x14u));
        c.A1 = c.SP + 0x10u;
        c.RA = 0x8001D2CCu;
        Dispatcher.Call(c, m, c.V0);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0x9u));
        goto L8001D39C;
        L8001D2D8: ;
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x4u));
        c.V1 = MemoryAccess.ReadU32(m, (c.S0 + 0xCu));
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0x8u));
        c.V0 = (int)c.V0 < 2 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S2 = c.A0 | c.V1;
            goto L8001D398;
        }
        c.S2 = c.A0 | c.V1;
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.S1 + 0xFu), (byte)c.V0);
        c.V0 = c.S2 & 0x0004u;
        c.S0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0x9u));
        if (c.V0 == 0u) {
            c.V0 = c.S2 & 0x0008u;
            goto L8001D350;
        }
        c.V0 = c.S2 & 0x0008u;
        MemoryAccess.WriteU8(m, (c.S1 + 0xEu), (byte)c.S0);
        c.S0 = c.S0 - 0x1u;
        if ((int)c.S0 >= 0) {
            c.S3 = 0xFFFFFFFDu;
            goto L8001D328;
        }
        c.S3 = 0xFFFFFFFDu;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0x8u));
        c.S0 = c.V0 - 0x1u;
        L8001D328: ;
        c.A0 = 0x00000004u;
        c.A1 = c.SP + 0x10u;
        c.A2 = c.S0 << 24;
        c.A2 = (uint)((int)c.A2 >> 24);
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x14u));
        c.V1 = 0xFFFFFFF4u;
        MemoryAccess.WriteU16(m, (c.S1 + 0xCu), (ushort)c.V1);
        MemoryAccess.WriteU8(m, (c.S1 + 0x9u), (byte)c.S0);
        c.RA = 0x8001D34Cu;
        Dispatcher.Call(c, m, c.V0);
        c.V0 = c.S2 & 0x0008u;
        L8001D350: ;
        if (c.V0 == 0u) {
            c.V0 = c.S3 + 0u;
            goto L8001D39C;
        }
        c.V0 = c.S3 + 0u;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0x8u));
        MemoryAccess.WriteU8(m, (c.S1 + 0xEu), (byte)c.S0);
        c.S0 = c.S0 + 0x1u;
        c.V0 = (int)c.S0 < (int)c.V0 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S3 = 0xFFFFFFFDu;
            goto L8001D374;
        }
        c.S3 = 0xFFFFFFFDu;
        c.S0 = 0u + 0u;
        L8001D374: ;
        c.A0 = 0x00000004u;
        c.A1 = c.SP + 0x10u;
        c.A2 = c.S0 << 24;
        c.A2 = (uint)((int)c.A2 >> 24);
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x14u));
        c.V1 = 0x0000000Cu;
        MemoryAccess.WriteU16(m, (c.S1 + 0xCu), (ushort)c.V1);
        MemoryAccess.WriteU8(m, (c.S1 + 0x9u), (byte)c.S0);
        c.RA = 0x8001D398u;
        Dispatcher.Call(c, m, c.V0);
        L8001D398: ;
        c.V0 = c.S3 + 0u;
        L8001D39C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x40u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x3Cu));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x38u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x34u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x30u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x2Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.SP = c.SP + 0x48u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001D3C0(CpuContext c, IMemory m)
    {
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0xAu));
        c.V0 = (int)c.A1 < -1 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V1 = 0u + 0u;
            goto L8001D3EC;
        }
        c.V1 = 0u + 0u;
        c.V0 = ~(0u | c.A1);
        c.V0 = c.V0 << 7;
        if ((int)c.V0 >= 0) {
            c.V1 = (uint)((int)c.V0 >> 4);
            goto L8001D3EC;
        }
        c.V1 = (uint)((int)c.V0 >> 4);
        c.V0 = c.V0 + 0xFu;
        c.V1 = (uint)((int)c.V0 >> 4);
        L8001D3EC: ;
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0xAu));
        if ((int)c.A0 <= 0) {
            c.V0 = (int)c.A0 < 12 ? 1u : 0u;
            goto L8001D424;
        }
        c.V0 = (int)c.A0 < 12 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V1 = 0x00000080u;
            goto L8001D424;
        }
        c.V1 = 0x00000080u;
        c.V1 = 0x2AAA0000u;
        c.V1 = c.V1 | 0xAAABu;
        c.V0 = c.A0 << 7;
        { var _r = (long)(int)c.V0 * (int)c.V1; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V0 = (uint)((int)c.V0 >> 31);
        c.A2 = c.HI;
        c.V1 = (uint)((int)c.A2 >> 1);
        c.V1 = c.V1 - c.V0;
        L8001D424: ;
        c.V0 = c.V1 + 0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001D42C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x68u;
        MemoryAccess.WriteU32(m, (c.SP + 0x4Cu), c.S3);
        c.S3 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x60u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x5Cu), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x58u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x54u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x50u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x48u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.S0);
        c.S7 = (uint)(short)MemoryAccess.ReadU16(m, c.S3);
        c.S5 = (uint)(short)MemoryAccess.ReadU16(m, (c.S3 + 0x2u));
        c.S6 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S6);
        MemoryAccess.WriteU16(m, (c.SP + 0x2Eu), (ushort)c.S5);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S3 + 0xAu));
        if ((int)c.V1 >= 0) {
            c.S4 = c.V1 - 0xCu;
            goto L8001D4AC;
        }
        c.S4 = c.V1 - 0xCu;
        c.V0 = (int)c.V1 < -1 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = ~(0u | c.V1);
            goto L8001D648;
        }
        c.V0 = ~(0u | c.V1);
        c.V0 = c.V0 << 7;
        if ((int)c.V0 >= 0) {
            c.S0 = (uint)((int)c.V0 >> 4);
            goto L8001D4A0;
        }
        c.S0 = (uint)((int)c.V0 >> 4);
        c.V0 = c.V0 + 0xFu;
        c.S0 = (uint)((int)c.V0 >> 4);
        L8001D4A0: ;
        c.A0 = 0x00000001u;
        MemoryAccess.WriteU16(m, (c.SP + 0x2Cu), (ushort)c.S7);
        goto L8001D630;
        L8001D4AC: ;
        if ((int)c.S4 < 0) {
            goto L8001D538;
        }
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S3 + 0xFu));
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u) {
            c.S0 = c.SP + 0x38u;
            goto L8001D538;
        }
        c.S0 = c.SP + 0x38u;
        c.A0 = c.S0 + 0u;
        c.A1 = c.S6 + 0u;
        c.A3 = c.S5 + 0u;
        c.V0 = 0x00000006u;
        c.V1 = MemoryAccess.ReadU16(m, (c.S3 + 0x4u));
        c.S1 = 0x0000000Au;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S4);
        c.V1 = c.V1 << 16;
        c.V1 = (uint)((int)c.V1 >> 17);
        c.S2 = c.V1 + 0x8u;
        c.A2 = c.S7 + c.S2;
        c.A2 = c.A2 + c.V0;
        c.RA = 0x8001D504u;
        GranTurismo2ArcadePC.func_8006B958(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S6 + 0u;
        c.A2 = c.S7 - c.S2;
        c.A2 = c.A2 - 0x6u;
        c.A3 = c.S5 + 0u;
        c.V0 = 0xFFFFFFFAu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S4);
        c.RA = 0x8001D52Cu;
        GranTurismo2ArcadePC.func_8006B958(c, m);
        c.A0 = c.S6 + 0u;
        c.A1 = 0x00000020u;
        c.RA = 0x8001D538u;
        GranTurismo2ArcadePC.func_8007D954(c, m);
        L8001D538: ;
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S3 + 0xAu));
        c.V0 = (int)c.A0 < 12 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.S0 = 0x00000080u;
            goto L8001D56C;
        }
        c.S0 = 0x00000080u;
        c.V1 = 0x2AAA0000u;
        c.V1 = c.V1 | 0xAAABu;
        c.V0 = c.A0 << 7;
        { var _r = (long)(int)c.V0 * (int)c.V1; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V0 = (uint)((int)c.V0 >> 31);
        c.T0 = c.HI;
        c.V1 = (uint)((int)c.T0 >> 1);
        c.S0 = c.V1 - c.V0;
        L8001D56C: ;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S3 + 0x4u));
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S3 + 0xCu));
        { var _r = (long)(int)c.A2 * (int)c.V1; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V0 = c.LO;
        c.A0 = 0x2AAA0000u;
        c.A0 = c.A0 | 0xAAABu;
        { var _r = (long)(int)c.V0 * (int)c.A0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.A1 = c.V1 + 0u;
        c.A3 = 0u - c.A2;
        c.V0 = (uint)((int)c.V0 >> 31);
        c.T1 = c.HI;
        c.V1 = (uint)((int)c.T1 >> 1);
        if ((int)c.A1 >= 0) {
            c.S2 = c.V1 - c.V0;
            goto L8001D5B0;
        }
        c.S2 = c.V1 - c.V0;
        c.A1 = 0u - c.A1;
        c.A3 = c.A2 + 0u;
        L8001D5B0: ;
        c.V0 = 0x0000000Cu;
        c.A1 = c.V0 - c.A1;
        { var _r = (long)(int)c.S0 * (int)c.A1; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V0 = c.LO;
        { var _r = (long)(int)c.V0 * (int)c.A0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V0 = (uint)((int)c.V0 >> 31);
        c.V1 = c.HI;
        c.A0 = (uint)((int)c.V1 >> 1);
        c.V1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S3 + 0xEu));
        if ((int)c.V1 < 0) {
            c.S0 = c.A0 - c.V0;
            goto L8001D618;
        }
        c.S0 = c.A0 - c.V0;
        c.A0 = 0x00000001u;
        c.V0 = c.S7 + c.S2;
        c.V0 = c.V0 + c.A3;
        MemoryAccess.WriteU16(m, (c.SP + 0x2Cu), (ushort)c.V0);
        c.V0 = 0x00000080u;
        c.V0 = c.V0 - c.S0;
        MemoryAccess.WriteU16(m, (c.SP + 0x30u), (ushort)c.V0);
        c.A2 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S3 + 0xEu));
        c.V0 = MemoryAccess.ReadU32(m, (c.S3 + 0x14u));
        c.A1 = c.SP + 0x20u;
        c.RA = 0x8001D618u;
        Dispatcher.Call(c, m, c.V0);
        L8001D618: ;
        c.A0 = 0x00000001u;
        c.V0 = c.S2 >> 31;
        c.V0 = c.S2 + c.V0;
        c.V0 = (uint)((int)c.V0 >> (int)(c.A0 & 31u));
        c.V0 = c.S7 + c.V0;
        MemoryAccess.WriteU16(m, (c.SP + 0x2Cu), (ushort)c.V0);
        L8001D630: ;
        MemoryAccess.WriteU16(m, (c.SP + 0x30u), (ushort)c.S0);
        c.A2 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S3 + 0x9u));
        c.V0 = MemoryAccess.ReadU32(m, (c.S3 + 0x14u));
        c.A1 = c.SP + 0x20u;
        c.RA = 0x8001D648u;
        Dispatcher.Call(c, m, c.V0);
        L8001D648: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x60u));
        c.S7 = MemoryAccess.ReadU32(m, (c.SP + 0x5Cu));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x58u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x54u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x50u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x4Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x48u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x44u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x40u));
        c.SP = c.SP + 0x68u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001D674(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x3B90u;
        c.A0 = 0x80020000u;
        c.A0 = c.A0 + 0x247Cu;
        MemoryAccess.WriteU32(m, (c.SP + 0x3B88u), c.RA);
        c.A1 = c.SP + 0x10u;
        c.RA = 0x8001D68Cu;
        GranTurismo2ArcadePC.func_80082EBC(c, m);
        c.A0 = 0x801F0000u;
        c.V0 = 0x801D0000u;
        c.V1 = MemoryAccess.ReadU8(m, (c.V0 - 0x6CC0u));
        c.A0 = c.A0 - 0xF20u;
        c.V0 = c.V1 << 4;
        c.V0 = c.V0 - c.V1;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 - c.V1;
        c.V0 = c.V0 << 4;
        c.V0 = c.V0 - c.V1;
        c.V1 = c.SP + 0x10u;
        c.V1 = c.V1 + c.V0;
        c.V0 = c.V1 | c.A0;
        c.V0 = c.V0 & 0x0003u;
        if (c.V0 == 0u) {
            c.V0 = c.V1 + 0x760u;
            goto L8001D720;
        }
        c.V0 = c.V1 + 0x760u;
        L8001D6CC: ;
        c.A2 = MemoryAccess.ReadWordLeft(m, c.A2, (c.V1 + 0x3u));
        c.A2 = MemoryAccess.ReadWordRight(m, c.A2, c.V1);
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V1 + 0x7u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, (c.V1 + 0x4u));
        c.T0 = MemoryAccess.ReadWordLeft(m, c.T0, (c.V1 + 0xBu));
        c.T0 = MemoryAccess.ReadWordRight(m, c.T0, (c.V1 + 0x8u));
        c.T1 = MemoryAccess.ReadWordLeft(m, c.T1, (c.V1 + 0xFu));
        c.T1 = MemoryAccess.ReadWordRight(m, c.T1, (c.V1 + 0xCu));
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0x3u), c.A2);
        MemoryAccess.WriteWordRight(m, c.A0, c.A2);
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0x7u), c.A3);
        MemoryAccess.WriteWordRight(m, (c.A0 + 0x4u), c.A3);
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0xBu), c.T0);
        MemoryAccess.WriteWordRight(m, (c.A0 + 0x8u), c.T0);
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0xFu), c.T1);
        MemoryAccess.WriteWordRight(m, (c.A0 + 0xCu), c.T1);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.V0) {
            c.A0 = c.A0 + 0x10u;
            goto L8001D6CC;
        }
        c.A0 = c.A0 + 0x10u;
        goto L8001D74C;
        L8001D720: ;
        c.A2 = MemoryAccess.ReadU32(m, c.V1);
        c.A3 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, c.A0, c.A2);
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.A3);
        MemoryAccess.WriteU32(m, (c.A0 + 0x8u), c.T0);
        MemoryAccess.WriteU32(m, (c.A0 + 0xCu), c.T1);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.V0) {
            c.A0 = c.A0 + 0x10u;
            goto L8001D720;
        }
        c.A0 = c.A0 + 0x10u;
        L8001D74C: ;
        c.A2 = MemoryAccess.ReadWordLeft(m, c.A2, (c.V1 + 0x3u));
        c.A2 = MemoryAccess.ReadWordRight(m, c.A2, c.V1);
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V1 + 0x7u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, (c.V1 + 0x4u));
        c.T0 = MemoryAccess.ReadWordLeft(m, c.T0, (c.V1 + 0xBu));
        c.T0 = MemoryAccess.ReadWordRight(m, c.T0, (c.V1 + 0x8u));
        c.T1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V1 + 0xCu));
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0x3u), c.A2);
        MemoryAccess.WriteWordRight(m, c.A0, c.A2);
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0x7u), c.A3);
        MemoryAccess.WriteWordRight(m, (c.A0 + 0x4u), c.A3);
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0xBu), c.T0);
        MemoryAccess.WriteWordRight(m, (c.A0 + 0x8u), c.T0);
        MemoryAccess.WriteU8(m, (c.A0 + 0xCu), (byte)c.T1);
        c.A2 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V1 + 0xDu));
        c.A3 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V1 + 0xEu));
        MemoryAccess.WriteU8(m, (c.A0 + 0xDu), (byte)c.A2);
        MemoryAccess.WriteU8(m, (c.A0 + 0xEu), (byte)c.A3);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x3B88u));
        c.SP = c.SP + 0x3B90u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001D7A4(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1C4u));
        c.V0 = 0x00000014u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x18u), c.A0);
        c.A0 = 0x00000007u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
        c.RA = 0x8001D7CCu;
        GranTurismo2ArcadePC.func_80012380(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001D7DC(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V1 = MemoryAccess.ReadU32(m, (c.A0 + 0x1C4u));
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x14u));
        if ((int)c.V0 <= 0) {
            c.A1 = 0u + 0u;
            goto L8001D830;
        }
        c.A1 = 0u + 0u;
        c.V0 = c.V0 - 0x1u;
        if (c.V0 != 0u) {
            MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
            goto L8001D830;
        }
        MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 + 0x18u));
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            c.A1 = 0x00000004u;
            goto L8001D830;
        }
        c.A1 = 0x00000004u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x45E4u;
        c.RA = 0x8001D82Cu;
        GranTurismo2ArcadePC.func_8001220C(c, m);
        c.A1 = 0x00000001u;
        L8001D830: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = c.A1 + 0u;
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001D840(CpuContext c, IMemory m)
    {
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001D848(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1C4u));
        c.V0 = 0x00000014u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
        c.RA = 0x8001D868u;
        GranTurismo2ArcadePC.func_800123A0(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001D878(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1C4u));
        c.V1 = MemoryAccess.ReadU32(m, (c.A0 + 0x14u));
        if ((int)c.V1 <= 0) {
            c.V0 = 0u + 0u;
            goto L8001D8AC;
        }
        c.V0 = 0u + 0u;
        c.V0 = c.V1 - 0x1u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.V0);
        c.V0 = c.V0 < 0x00000001u ? 1u : 0u;
        c.V0 = c.V0 << 2;
        L8001D8AC: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001D8B4(CpuContext c, IMemory m)
    {
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001D8BC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A2 + 0u;
        c.V0 = c.S1 << 2;
        c.V0 = c.V0 + c.S1;
        c.V0 = c.V0 << 2;
        c.V1 = 0x800B0000u;
        c.V1 = c.V1 + 0x11A8u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.V0 + c.V1;
        c.V0 = c.A0 < 0x00000009u ? 1u : 0u;
        if (c.V0 == 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
            goto L8001D990;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x32A4u;
        c.V1 = c.A0 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x8001D910u: goto L8001D910;
            case 0x8001D940u: goto L8001D940;
            case 0x8001D948u: goto L8001D948;
            case 0x8001D950u: goto L8001D950;
            case 0x8001D960u: goto L8001D960;
            case 0x8001D990u: goto L8001D990;
            case 0x8001D988u: goto L8001D988;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L8001D910: ;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x477Cu;
        c.RA = 0x8001D920u;
        GranTurismo2ArcadePC.func_800162EC(c, m);
        c.V0 = c.S1 << 1;
        c.V0 = c.V0 + c.S1;
        c.V0 = c.V0 << 2;
        c.V1 = 0x80050000u;
        c.V1 = c.V1 - 0x5958u;
        c.V0 = c.V0 + c.V1;
        MemoryAccess.WriteU32(m, (c.S0 + 0xCu), c.V0);
        goto L8001D990;
        L8001D940: ;
        MemoryAccess.WriteU16(m, (c.S0 + 0x10u), (ushort)0u);
        goto L8001D990;
        L8001D948: ;
        c.V0 = 0xFFFFFFF3u;
        goto L8001D98C;
        L8001D950: ;
        c.A0 = c.S0 + 0u;
        c.RA = 0x8001D958u;
        GranTurismo2ArcadePC.func_80016320(c, m);
        goto L8001D990;
        L8001D960: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0xCu));
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0xEu));
        c.A0 = c.S0 + 0u;
        MemoryAccess.WriteU16(m, (c.A0 + 0x4u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.A0 + 0x6u), (ushort)c.V1);
        c.A1 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.A2 = 0u + 0u;
        c.RA = 0x8001D980u;
        GranTurismo2ArcadePC.func_80016368(c, m);
        goto L8001D990;
        L8001D988: ;
        c.V0 = 0x0000000Cu;
        L8001D98C: ;
        MemoryAccess.WriteU16(m, (c.S0 + 0x10u), (ushort)c.V0);
        L8001D990: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = 0x00000001u;
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001D9A8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        c.V0 = 0x800B0000u;
        c.A1 = 0x80020000u;
        c.A1 = c.A1 - 0x2744u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        c.A2 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S2 = MemoryAccess.ReadU32(m, (c.V0 + 0x1C4u));
        c.V0 = 0x00000014u;
        MemoryAccess.WriteU32(m, (c.S2 + 0x14u), c.V0);
        c.V0 = 0x80050000u;
        c.S1 = c.V0 - 0x4778u;
        c.A0 = c.S1 + 0u;
        c.RA = 0x8001D9F0u;
        GranTurismo2ArcadePC.func_8006CCDC(c, m);
        if (c.S0 == 0u) {
            MemoryAccess.WriteU16(m, (c.S1 + 0x6u), (ushort)0u);
            goto L8001DA04;
        }
        MemoryAccess.WriteU16(m, (c.S1 + 0x6u), (ushort)0u);
        c.V0 = MemoryAccess.ReadU32(m, (c.S2 + 0x18u));
        MemoryAccess.WriteU16(m, (c.S1 + 0x6u), (ushort)c.V0);
        L8001DA04: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001DA1C(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1C4u));
        if (c.A0 != 0u) {
            c.S3 = c.V0 + 0x1A4u;
            goto L8001DA54;
        }
        c.S3 = c.V0 + 0x1A4u;
        c.S3 = c.S2 + 0u;
        L8001DA54: ;
        c.S0 = MemoryAccess.ReadU32(m, (c.S1 + 0x14u));
        if ((int)c.S0 <= 0) {
            goto L8001DA78;
        }
        c.S0 = c.S0 - 0x1u;
        if (c.S0 != 0u) {
            c.A0 = 0x80050000u;
            goto L8001DA78;
        }
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4778u;
        c.RA = 0x8001DA78u;
        GranTurismo2ArcadePC.func_8006CD80(c, m);
        L8001DA78: ;
        MemoryAccess.WriteU32(m, (c.S1 + 0x14u), c.S0);
        c.V0 = 0x80050000u;
        c.S4 = c.V0 - 0x4778u;
        c.A0 = c.S4 + 0u;
        c.A1 = c.S3 + 0u;
        c.RA = 0x8001DA90u;
        GranTurismo2ArcadePC.func_8006CED4(c, m);
        c.S0 = c.V0 + 0u;
        c.V0 = 0xFFFFFFFDu;
        if (c.S0 == c.V0) {
            c.V0 = (int)c.S0 < -2 ? 1u : 0u;
            goto L8001DAD4;
        }
        c.V0 = (int)c.S0 < -2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0xFFFFFFFCu;
            goto L8001DAB8;
        }
        c.V0 = 0xFFFFFFFCu;
        if (c.S0 == c.V0) {
            c.V0 = c.S2 + 0u;
            goto L8001DB3C;
        }
        c.V0 = c.S2 + 0u;
        goto L8001DAFC;
        L8001DAB8: ;
        c.V0 = 0xFFFFFFFEu;
        if (c.S0 == c.V0) {
            c.V0 = 0xFFFFFFFFu;
            goto L8001DB38;
        }
        c.V0 = 0xFFFFFFFFu;
        if (c.S0 == c.V0) {
            goto L8001DAE4;
        }
        goto L8001DAFC;
        L8001DAD4: ;
        c.A0 = 0x00000005u;
        c.RA = 0x8001DADCu;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.V0 = c.S2 + 0u;
        goto L8001DB3C;
        L8001DAE4: ;
        c.A0 = 0x00000004u;
        c.RA = 0x8001DAECu;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.A0 = c.S4 + 0u;
        c.RA = 0x8001DAF4u;
        GranTurismo2ArcadePC.func_8006CDE8(c, m);
        c.S2 = 0x00000002u;
        goto L8001DB38;
        L8001DAFC: ;
        c.A0 = 0x00000003u;
        c.RA = 0x8001DB04u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4778u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x18u), c.S0);
        c.RA = 0x8001DB14u;
        GranTurismo2ArcadePC.func_8006CDE8(c, m);
        c.V0 = 0x800B0000u;
        c.V1 = 0x80050000u;
        c.V1 = c.V1 - 0x4788u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        c.V0 = c.S0 << 2;
        c.V0 = c.V0 + c.V1;
        c.A1 = MemoryAccess.ReadU32(m, c.V0);
        c.S2 = 0x00000001u;
        c.RA = 0x8001DB38u;
        GranTurismo2ArcadePC.func_8001220C(c, m);
        L8001DB38: ;
        c.V0 = c.S2 + 0u;
        L8001DB3C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001DB5C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4778u;
        c.A1 = c.A1 + 0x10u;
        c.RA = 0x8001DB78u;
        GranTurismo2ArcadePC.func_8006D41C(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001DB88(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.A2 = 0x80050000u;
        c.A0 = c.A2 - 0x4744u;
        c.V1 = 0x800B0000u;
        c.A1 = 0x800B0000u;
        c.V0 = 0x800B0000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 - 0x759Cu));
        c.A1 = MemoryAccess.ReadU32(m, (c.A1 - 0x75A8u));
        c.V0 = c.V0 - 0x75A4u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU32(m, (c.A0 + 0x8u), c.V0);
        MemoryAccess.WriteU32(m, (c.A2 - 0x4744u), c.V1);
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.A1);
        c.RA = 0x8001DBC0u;
        GranTurismo2ArcadePC.func_8002040C(c, m);
        c.A0 = 0u + 0u;
        c.RA = 0x8001DBC8u;
        GranTurismo2ArcadePC.func_800204AC(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001DBD8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 - 0x75A0u));
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        if (c.A0 != 0u) {
            c.V0 = c.V0 + 0x1A4u;
            goto L8001DC00;
        }
        c.V0 = c.V0 + 0x1A4u;
        c.V0 = c.S0 + 0u;
        L8001DC00: ;
        c.A0 = c.V0 + 0u;
        c.RA = 0x8001DC08u;
        Dispatcher.Call(c, m, 0x800204ECu);
        c.V1 = c.V0 + 0u;
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L8001DC50;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = c.S0 + 0u;
            goto L8001DC60;
        }
        c.V0 = c.S0 + 0u;
        c.V0 = 0x00000002u;
        if (c.V1 != c.V0) {
            c.V0 = c.S0 + 0u;
            goto L8001DC60;
        }
        c.V0 = c.S0 + 0u;
        c.A0 = 0x00000003u;
        c.RA = 0x8001DC34u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.A0 = MemoryAccess.ReadU32(m, (c.S1 - 0x75A0u));
        c.S0 = 0x00000001u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x453Cu;
        c.RA = 0x8001DC48u;
        GranTurismo2ArcadePC.func_8001220C(c, m);
        c.V0 = c.S0 + 0u;
        goto L8001DC60;
        L8001DC50: ;
        c.A0 = 0x00000004u;
        c.RA = 0x8001DC58u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.S0 = 0x00000002u;
        c.V0 = c.S0 + 0u;
        L8001DC60: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001DC74(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.A0 = c.A0 + 0x10u;
        c.RA = 0x8001DC88u;
        GranTurismo2ArcadePC.func_800206C0(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001DC98(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.RA = 0x8001DCA8u;
        GranTurismo2ArcadePC.func_80020964(c, m);
        c.A2 = c.V0 + 0u;
        c.T1 = 0u + 0u;
        c.V0 = 0x801D0000u;
        c.T2 = c.V0 - 0x6CC0u;
        c.T0 = 0x00000218u;
        c.A3 = c.A2 + 0x20u;
        L8001DCC0: ;
        c.V0 = (int)c.T1 < 128 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L8001DD90;
        }
        c.A1 = MemoryAccess.ReadU32(m, c.A2);
        c.V0 = ~(0u | c.A1);
        if (c.V0 == 0u) {
            c.A0 = c.T0 + c.T2;
            goto L8001DD7C;
        }
        c.A0 = c.T0 + c.T2;
        c.V1 = MemoryAccess.ReadU32(m, c.A0);
        c.V0 = ~(0u | c.V1);
        if (c.V0 == 0u) {
            c.V0 = c.A1 < c.V1 ? 1u : 0u;
            goto L8001DD38;
        }
        c.V0 = c.A1 < c.V1 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V1 = c.A0 + 0u;
            goto L8001DD7C;
        }
        c.V1 = c.A0 + 0u;
        c.V0 = c.A2 + 0u;
        c.A0 = c.A3 + 0u;
        L8001DD04: ;
        c.T3 = MemoryAccess.ReadU32(m, c.V0);
        c.T4 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T5 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        c.T6 = MemoryAccess.ReadU32(m, (c.V0 + 0xCu));
        MemoryAccess.WriteU32(m, c.V1, c.T3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x4u), c.T4);
        MemoryAccess.WriteU32(m, (c.V1 + 0x8u), c.T5);
        MemoryAccess.WriteU32(m, (c.V1 + 0xCu), c.T6);
        c.V0 = c.V0 + 0x10u;
        if (c.V0 != c.A0) {
            c.V1 = c.V1 + 0x10u;
            goto L8001DD04;
        }
        c.V1 = c.V1 + 0x10u;
        goto L8001DD70;
        L8001DD38: ;
        c.V1 = c.A0 + 0u;
        c.V0 = c.A2 + 0u;
        c.A0 = c.A3 + 0u;
        L8001DD44: ;
        c.T3 = MemoryAccess.ReadU32(m, c.V0);
        c.T4 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T5 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        c.T6 = MemoryAccess.ReadU32(m, (c.V0 + 0xCu));
        MemoryAccess.WriteU32(m, c.V1, c.T3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x4u), c.T4);
        MemoryAccess.WriteU32(m, (c.V1 + 0x8u), c.T5);
        MemoryAccess.WriteU32(m, (c.V1 + 0xCu), c.T6);
        c.V0 = c.V0 + 0x10u;
        if (c.V0 != c.A0) {
            c.V1 = c.V1 + 0x10u;
            goto L8001DD44;
        }
        c.V1 = c.V1 + 0x10u;
        L8001DD70: ;
        c.T3 = MemoryAccess.ReadU32(m, c.V0);
        MemoryAccess.WriteU32(m, c.V1, c.T3);
        L8001DD7C: ;
        c.A3 = c.A3 + 0x24u;
        c.A2 = c.A2 + 0x24u;
        c.T0 = c.T0 + 0x24u;
        c.T1 = c.T1 + 0x1u;
        goto L8001DCC0;
        L8001DD90: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001DDA0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x38u;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S6);
        c.S6 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.FP);
        c.FP = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x34u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.V0 = MemoryAccess.ReadU32(m, c.S6);
        c.V0 = ~(0u | c.V0);
        if (c.V0 != 0u) {
            c.S7 = c.A2 + 0u;
            goto L8001DDF0;
        }
        c.S7 = c.A2 + 0u;
        c.V0 = 0xFFFFFFFFu;
        goto L8001DED8;
        L8001DDF0: ;
        c.S3 = 0u + 0u;
        c.S5 = 0x00000068u;
        c.S4 = 0x00000004u;
        L8001DDFC: ;
        c.V0 = (int)c.S3 < 5 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A0 = c.S7 + c.S4;
            goto L8001DED4;
        }
        c.A0 = c.S7 + c.S4;
        c.V1 = MemoryAccess.ReadU32(m, c.A0);
        c.V0 = ~(0u | c.V1);
        if (c.V0 == 0u) {
            c.V0 = c.S3 + 0u;
            goto L8001DED8;
        }
        c.V0 = c.S3 + 0u;
        c.A1 = MemoryAccess.ReadU32(m, c.S6);
        c.V0 = c.A1 < c.V1 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = c.S3 + 0u;
            goto L8001DED8;
        }
        c.V0 = c.S3 + 0u;
        if (c.V1 != c.A1) {
            c.S1 = c.A0 + 0u;
            goto L8001DEC4;
        }
        c.S1 = c.A0 + 0u;
        c.S0 = c.S6 + 0u;
        c.A0 = 0u + 0u;
        L8001DE40: ;
        c.V1 = MemoryAccess.ReadU8(m, c.S0);
        c.S0 = c.S0 + 0x1u;
        c.V0 = MemoryAccess.ReadU8(m, c.S1);
        if (c.V1 != c.V0) {
            c.S1 = c.S1 + 0x1u;
            goto L8001DEC4;
        }
        c.S1 = c.S1 + 0x1u;
        c.A0 = c.A0 + 0x1u;
        c.V0 = (int)c.A0 < 20 ? 1u : 0u;
        if (c.V0 != 0u) {
            goto L8001DE40;
        }
        c.S1 = c.S7 + c.S5;
        c.S0 = c.FP + 0u;
        c.A0 = c.S1 + 0u;
        c.RA = 0x8001DE78u;
        GranTurismo2ArcadePC.func_8008CED4(c, m);
        c.A0 = c.S0 + 0u;
        c.S2 = c.V0 + 0u;
        c.RA = 0x8001DE84u;
        GranTurismo2ArcadePC.func_8008CED4(c, m);
        if (c.S2 != c.V0) {
            goto L8001DEC4;
        }
        if ((int)c.S2 < 0) {
            c.A0 = 0u + 0u;
            goto L8001DED4;
        }
        c.A0 = 0u + 0u;
        L8001DE94: ;
        c.V1 = MemoryAccess.ReadU8(m, c.S0);
        c.S0 = c.S0 + 0x1u;
        c.V0 = MemoryAccess.ReadU8(m, c.S1);
        if (c.V1 != c.V0) {
            c.S1 = c.S1 + 0x1u;
            goto L8001DEC4;
        }
        c.S1 = c.S1 + 0x1u;
        c.A0 = c.A0 + 0x1u;
        c.V0 = (int)c.S2 < (int)c.A0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0xFFFFFFFFu;
            goto L8001DE94;
        }
        c.V0 = 0xFFFFFFFFu;
        goto L8001DED8;
        L8001DEC4: ;
        c.S5 = c.S5 + 0xCu;
        c.S4 = c.S4 + 0x14u;
        c.S3 = c.S3 + 0x1u;
        goto L8001DDFC;
        L8001DED4: ;
        c.V0 = 0xFFFFFFFFu;
        L8001DED8: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x34u));
        c.FP = MemoryAccess.ReadU32(m, (c.SP + 0x30u));
        c.S7 = MemoryAccess.ReadU32(m, (c.SP + 0x2Cu));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x38u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001DF08(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x40u;
        MemoryAccess.WriteU32(m, (c.SP + 0x3Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x38u), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0x34u), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S0);
        c.RA = 0x8001DF38u;
        GranTurismo2ArcadePC.func_80020990(c, m);
        c.S6 = c.V0 + 0u;
        c.FP = 0x00001418u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), 0u);
        L8001DF44: ;
        c.V1 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = (int)c.V1 < 6 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.S7 = 0u + 0u;
            goto L8001DFE0;
        }
        c.S7 = 0u + 0u;
        c.V1 = 0x801D0000u;
        c.V1 = c.V1 - 0x6CC0u;
        c.S3 = c.FP + c.V1;
        L8001DF64: ;
        c.V0 = (int)c.S7 < 10 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.S5 = 0u + 0u;
            goto L8001DFCC;
        }
        c.S5 = 0u + 0u;
        c.S4 = 0x00000068u;
        c.S2 = 0x00000004u;
        L8001DF78: ;
        c.V0 = (int)c.S5 < 5 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.S1 = c.S6 + c.S2;
            goto L8001DFBC;
        }
        c.S1 = c.S6 + c.S2;
        c.A0 = c.S1 + 0u;
        c.S0 = c.S6 + c.S4;
        c.A1 = c.S0 + 0u;
        c.A2 = c.S3 + 0u;
        c.RA = 0x8001DF98u;
        GranTurismo2ArcadePC.func_8001DDA0(c, m);
        if ((int)c.V0 < 0) {
            c.A0 = c.S3 + 0u;
            goto L8001DFAC;
        }
        c.A0 = c.S3 + 0u;
        c.A1 = c.S1 + 0u;
        c.A2 = c.S0 + 0u;
        c.RA = 0x8001DFACu;
        GranTurismo2ArcadePC.func_8005DE6C(c, m);
        L8001DFAC: ;
        c.S4 = c.S4 + 0xCu;
        c.S2 = c.S2 + 0x14u;
        c.S5 = c.S5 + 0x1u;
        goto L8001DF78;
        L8001DFBC: ;
        c.S6 = c.S6 + 0xA4u;
        c.S3 = c.S3 + 0xA4u;
        c.S7 = c.S7 + 0x1u;
        goto L8001DF64;
        L8001DFCC: ;
        c.V1 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.FP = c.FP + 0x668u;
        c.V1 = c.V1 + 0x1u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V1);
        goto L8001DF44;
        L8001DFE0: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x3Cu));
        c.FP = MemoryAccess.ReadU32(m, (c.SP + 0x38u));
        c.S7 = MemoryAccess.ReadU32(m, (c.SP + 0x34u));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x30u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x2Cu));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.SP = c.SP + 0x40u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001E010(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.RA = 0x8001E02Cu;
        GranTurismo2ArcadePC.func_800209BC(c, m);
        c.S2 = c.V0 + 0u;
        c.V0 = 0x801D0000u;
        c.V0 = c.V0 - 0x6CC0u;
        c.S3 = c.V0 + 0x3A88u;
        c.S1 = 0u + 0u;
        c.S0 = 0x00000004u;
        L8001E044: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, c.S2);
        c.V0 = (int)c.S1 < (int)c.V0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A0 = c.S3 + 0u;
            goto L8001E070;
        }
        c.A0 = c.S3 + 0u;
        c.A1 = c.S2 + c.S0;
        c.A2 = 0u + 0u;
        c.RA = 0x8001E064u;
        GranTurismo2ArcadePC.func_8005E000(c, m);
        c.S0 = c.S0 + 0x14u;
        c.S1 = c.S1 + 0x1u;
        goto L8001E044;
        L8001E070: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001E08C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.RA = 0x8001E0A8u;
        GranTurismo2ArcadePC.func_800209E8(c, m);
        c.S2 = c.V0 + 0u;
        c.V0 = 0x801D0000u;
        c.V0 = c.V0 - 0x6CC0u;
        c.S3 = c.V0 + 0x3B2Cu;
        c.S1 = 0u + 0u;
        c.S0 = 0x00000004u;
        L8001E0C0: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, c.S2);
        c.V0 = (int)c.S1 < (int)c.V0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A0 = c.S3 + 0u;
            goto L8001E0EC;
        }
        c.A0 = c.S3 + 0u;
        c.A1 = c.S2 + c.S0;
        c.A2 = 0u + 0u;
        c.RA = 0x8001E0E0u;
        GranTurismo2ArcadePC.func_8005E000(c, m);
        c.S0 = c.S0 + 0x14u;
        c.S1 = c.S1 + 0x1u;
        goto L8001E0C0;
        L8001E0EC: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001E108(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.RA = 0x8001E124u;
        GranTurismo2ArcadePC.func_80020A14(c, m);
        c.S2 = c.V0 + 0u;
        c.V0 = 0x801D0000u;
        c.V0 = c.V0 - 0x6CC0u;
        c.S3 = c.V0 + 0x3BD0u;
        c.S1 = 0u + 0u;
        c.S0 = 0x00000004u;
        L8001E13C: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, c.S2);
        c.V0 = (int)c.S1 < (int)c.V0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A0 = c.S3 + 0u;
            goto L8001E168;
        }
        c.A0 = c.S3 + 0u;
        c.A1 = c.S2 + c.S0;
        c.A2 = 0x00000001u;
        c.RA = 0x8001E15Cu;
        GranTurismo2ArcadePC.func_8005E000(c, m);
        c.S0 = c.S0 + 0x14u;
        c.S1 = c.S1 + 0x1u;
        goto L8001E13C;
        L8001E168: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001E184(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.A2 = 0x80050000u;
        c.A0 = c.A2 - 0x4744u;
        c.V1 = 0x800B0000u;
        c.A1 = 0x800B0000u;
        c.V0 = 0x800B0000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 - 0x759Cu));
        c.A1 = MemoryAccess.ReadU32(m, (c.A1 - 0x75A8u));
        c.V0 = c.V0 - 0x75A4u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU32(m, (c.A0 + 0x8u), c.V0);
        MemoryAccess.WriteU32(m, (c.A2 - 0x4744u), c.V1);
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.A1);
        c.RA = 0x8001E1BCu;
        GranTurismo2ArcadePC.func_8002040C(c, m);
        c.A0 = 0x00000001u;
        c.RA = 0x8001E1C4u;
        GranTurismo2ArcadePC.func_800204AC(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001E1D4(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        if (c.A0 != 0u) {
            c.V0 = c.V0 + 0x1A4u;
            goto L8001E1F8;
        }
        c.V0 = c.V0 + 0x1A4u;
        c.V0 = c.S0 + 0u;
        L8001E1F8: ;
        c.A0 = c.V0 + 0u;
        c.RA = 0x8001E200u;
        Dispatcher.Call(c, m, 0x800204ECu);
        c.V1 = c.V0 + 0u;
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L8001E25C;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = c.S0 + 0u;
            goto L8001E26C;
        }
        c.V0 = c.S0 + 0u;
        c.V0 = 0x00000002u;
        if (c.V1 != c.V0) {
            c.V0 = c.S0 + 0u;
            goto L8001E26C;
        }
        c.V0 = c.S0 + 0u;
        c.A0 = 0x00000003u;
        c.RA = 0x8001E22Cu;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.RA = 0x8001E234u;
        GranTurismo2ArcadePC.func_8001DC98(c, m);
        c.RA = 0x8001E23Cu;
        GranTurismo2ArcadePC.func_8001DF08(c, m);
        c.RA = 0x8001E244u;
        GranTurismo2ArcadePC.func_8001E010(c, m);
        c.RA = 0x8001E24Cu;
        GranTurismo2ArcadePC.func_8001E08C(c, m);
        c.RA = 0x8001E254u;
        GranTurismo2ArcadePC.func_8001E108(c, m);
        c.V0 = c.S0 + 0u;
        goto L8001E26C;
        L8001E25C: ;
        c.A0 = 0x00000004u;
        c.RA = 0x8001E264u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.S0 = 0x00000002u;
        c.V0 = c.S0 + 0u;
        L8001E26C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001E27C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.A0 = c.A0 + 0x10u;
        c.RA = 0x8001E290u;
        GranTurismo2ArcadePC.func_800206C0(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001E2A0(CpuContext c, IMemory m)
    {
        c.A1 = 0x00000001u;
        c.V1 = 0u + 0u;
        c.V0 = c.A0 + c.V1;
        L8001E2AC: ;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x2B5Cu));
        if (c.V0 != 0u) {
            goto L8001E2C0;
        }
        c.A1 = 0u + 0u;
        L8001E2C0: ;
        c.V1 = c.V1 + 0x1u;
        c.V0 = (int)c.V1 < 8 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = c.A0 + c.V1;
            goto L8001E2AC;
        }
        c.V0 = c.A0 + c.V1;
        if (c.A1 == 0u) {
            c.A1 = 0u + 0u;
            goto L8001E380;
        }
        c.A1 = 0u + 0u;
        c.A2 = 0x00000001u;
        c.V0 = 0x801D0000u;
        c.V1 = c.V0 - 0x38A0u;
        L8001E2E4: ;
        c.V0 = (int)c.A1 < 10 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L8001E310;
        }
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V1 + 0x1u));
        if (c.V0 != 0u) {
            goto L8001E304;
        }
        MemoryAccess.WriteU8(m, (c.V1 + 0x1u), (byte)c.A2);
        L8001E304: ;
        c.V1 = c.V1 + 0xA4u;
        c.A1 = c.A1 + 0x1u;
        goto L8001E2E4;
        L8001E310: ;
        c.A1 = 0x00000001u;
        c.V1 = 0u + 0u;
        c.V0 = c.A0 + c.V1;
        L8001E31C: ;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x2B64u));
        if (c.V0 != 0u) {
            goto L8001E330;
        }
        c.A1 = 0u + 0u;
        L8001E330: ;
        c.V1 = c.V1 + 0x1u;
        c.V0 = (int)c.V1 < 8 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = c.A0 + c.V1;
            goto L8001E31C;
        }
        c.V0 = c.A0 + c.V1;
        if (c.A1 == 0u) {
            c.A0 = 0u + 0u;
            goto L8001E380;
        }
        c.A0 = 0u + 0u;
        c.A1 = 0x00000001u;
        c.V0 = 0x801D0000u;
        c.V1 = c.V0 - 0x3F08u;
        L8001E354: ;
        c.V0 = (int)c.A0 < 10 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L8001E380;
        }
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V1 + 0x1u));
        if (c.V0 != 0u) {
            goto L8001E374;
        }
        MemoryAccess.WriteU8(m, (c.V1 + 0x1u), (byte)c.A1);
        L8001E374: ;
        c.V1 = c.V1 + 0xA4u;
        c.A0 = c.A0 + 0x1u;
        goto L8001E354;
        L8001E380: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001E388(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.A2 = 0x80050000u;
        c.A0 = c.A2 - 0x4744u;
        c.V1 = 0x800B0000u;
        c.A1 = 0x800B0000u;
        c.V0 = 0x800B0000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 - 0x759Cu));
        c.A1 = MemoryAccess.ReadU32(m, (c.A1 - 0x75A8u));
        c.V0 = c.V0 - 0x75A4u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU32(m, (c.A0 + 0x8u), c.V0);
        MemoryAccess.WriteU32(m, (c.A2 - 0x4744u), c.V1);
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.A1);
        c.RA = 0x8001E3C0u;
        GranTurismo2ArcadePC.func_8002040C(c, m);
        c.A0 = 0x00000002u;
        c.RA = 0x8001E3C8u;
        GranTurismo2ArcadePC.func_800204AC(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001E3D8(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        if (c.A0 != 0u) {
            c.V0 = c.V0 + 0x1A4u;
            goto L8001E3FC;
        }
        c.V0 = c.V0 + 0x1A4u;
        c.V0 = c.S0 + 0u;
        L8001E3FC: ;
        c.A0 = c.V0 + 0u;
        c.RA = 0x8001E404u;
        Dispatcher.Call(c, m, 0x800204ECu);
        c.V1 = c.V0 + 0u;
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L8001E448;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = c.S0 + 0u;
            goto L8001E458;
        }
        c.V0 = c.S0 + 0u;
        c.V0 = 0x00000002u;
        if (c.V1 != c.V0) {
            c.V0 = c.S0 + 0u;
            goto L8001E458;
        }
        c.V0 = c.S0 + 0u;
        c.A0 = 0x00000003u;
        c.RA = 0x8001E430u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.RA = 0x8001E438u;
        GranTurismo2ArcadePC.func_80020A40(c, m);
        c.A0 = c.V0 + 0u;
        c.RA = 0x8001E440u;
        GranTurismo2ArcadePC.func_8001E2A0(c, m);
        c.V0 = c.S0 + 0u;
        goto L8001E458;
        L8001E448: ;
        c.A0 = 0x00000004u;
        c.RA = 0x8001E450u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.S0 = 0x00000002u;
        c.V0 = c.S0 + 0u;
        L8001E458: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001E468(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.A0 = c.A0 + 0x10u;
        c.RA = 0x8001E47Cu;
        GranTurismo2ArcadePC.func_800206C0(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001E48C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x40u;
        c.A3 = 0x00980000u;
        c.A3 = c.A3 | 0x9680u;
        if (c.A1 != 0u) {
            c.T1 = 0u + 0u;
            goto L8001E4B0;
        }
        c.T1 = 0u + 0u;
        c.V0 = 0x00000030u;
        MemoryAccess.WriteU8(m, c.A0, (byte)c.V0);
        MemoryAccess.WriteU8(m, (c.A0 + 0x1u), (byte)0u);
        goto L8001E5A4;
        L8001E4B0: ;
        c.T0 = 0u + 0u;
        c.T3 = 0x00000001u;
        c.T2 = 0x66660000u;
        c.T2 = c.T2 | 0x6667u;
        L8001E4C0: ;
        if (c.A3 != 0u) { c.LO = c.A1 / c.A3; c.HI = c.A1 % c.A3; }
        c.A2 = c.LO;
        c.V0 = 0u < c.A2 ? 1u : 0u;
        c.V0 = c.V0 | c.T1;
        if (c.V0 == 0u) {
            { var _r = (long)(int)c.A2 * (int)c.A3; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
            goto L8001E4F4;
        }
        { var _r = (long)(int)c.A2 * (int)c.A3; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V1 = c.SP + c.T0;
        c.V0 = c.A2 + 0x30u;
        MemoryAccess.WriteU8(m, c.V1, (byte)c.V0);
        c.T1 = 0x00000001u;
        c.T0 = c.T0 + c.T1;
        c.T4 = c.LO;
        c.A1 = c.A1 - c.T4;
        L8001E4F4: ;
        if (c.A3 == c.T3) {
            { var _r = (long)(int)c.A3 * (int)c.T2; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
            goto L8001E510;
        }
        { var _r = (long)(int)c.A3 * (int)c.T2; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V0 = (uint)((int)c.A3 >> 31);
        c.T4 = c.HI;
        c.V1 = (uint)((int)c.T4 >> 2);
        c.A3 = c.V1 - c.V0;
        goto L8001E4C0;
        L8001E510: ;
        c.V0 = 0x55550000u;
        c.V0 = c.V0 | 0x5556u;
        { var _r = (long)(int)c.T0 * (int)c.V0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V0 = c.SP + c.T0;
        MemoryAccess.WriteU8(m, c.V0, (byte)0u);
        c.V0 = (uint)((int)c.T0 >> 31);
        c.T4 = c.HI;
        c.V1 = c.T4 - c.V0;
        c.V0 = c.V1 << 1;
        c.V0 = c.V0 + c.V1;
        c.V1 = c.T0 - c.V0;
        if (c.V1 != 0u) {
            goto L8001E548;
        }
        c.V1 = 0x00000003u;
        L8001E548: ;
        c.V0 = MemoryAccess.ReadU8(m, c.SP);
        MemoryAccess.WriteU8(m, c.A0, (byte)c.V0);
        c.V0 = MemoryAccess.ReadU8(m, (c.SP + 0x1u));
        c.T0 = 0u + 0u;
        goto L8001E598;
        L8001E560: ;
        c.V1 = c.V1 - 0x1u;
        if (c.V1 != 0u) {
            c.V0 = 0x0000002Cu;
            goto L8001E578;
        }
        c.V0 = 0x0000002Cu;
        MemoryAccess.WriteU8(m, c.A0, (byte)c.V0);
        c.A0 = c.A0 + 0x1u;
        c.V1 = 0x00000003u;
        L8001E578: ;
        c.T0 = c.T0 + 0x1u;
        c.V0 = c.SP + c.T0;
        c.V0 = MemoryAccess.ReadU8(m, c.V0);
        MemoryAccess.WriteU8(m, c.A0, (byte)c.V0);
        c.V0 = c.T0 + c.SP;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V0 + 0x1u));
        L8001E598: ;
        if (c.V0 != 0u) {
            c.A0 = c.A0 + 0x1u;
            goto L8001E560;
        }
        c.A0 = c.A0 + 0x1u;
        MemoryAccess.WriteU8(m, c.A0, (byte)0u);
        L8001E5A4: ;
        c.SP = c.SP + 0x40u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001E5AC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        c.V1 = 0x801D0000u;
        c.V1 = MemoryAccess.ReadU8(m, (c.V1 - 0x6CC0u));
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A8u));
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        if (c.V1 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
            goto L8001E610;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.A0 = c.S1 + 0u;
        c.RA = 0x8001E5D8u;
        GranTurismo2ArcadePC.func_8007F084(c, m);
        c.A0 = c.S1 + 0u;
        c.S0 = 0x801C0000u;
        c.S0 = c.S0 - 0x6463u;
        c.A1 = c.S0 + 0u;
        c.RA = 0x8001E5ECu;
        GranTurismo2ArcadePC.func_8006EBE8(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S0 + 0x14u;
        c.RA = 0x8001E5F8u;
        GranTurismo2ArcadePC.func_8006EBE8(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S0 + 0x30u;
        c.RA = 0x8001E604u;
        GranTurismo2ArcadePC.func_8006EBE8(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S0 + 0x4Fu;
        c.RA = 0x8001E610u;
        GranTurismo2ArcadePC.func_8006EBE8(c, m);
        L8001E610: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001E624(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x38u;
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.S7);
        c.S7 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = c.A3 + 0u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0x48u));
        c.V0 = 0x801D0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S6);
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x4Cu));
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6CC0u));
        c.V1 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        c.S4 = MemoryAccess.ReadU32(m, (c.V1 - 0x75A8u));
        c.A0 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        if (c.V0 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
            goto L8001E730;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        if (c.A2 == 0u) {
            c.S0 = c.A1 + 0u;
            goto L8001E6B0;
        }
        c.S0 = c.A1 + 0u;
        c.V0 = MemoryAccess.ReadU8(m, c.A1);
        if (c.V0 == 0u) {
            c.V1 = c.A1 + 0u;
            goto L8001E6A4;
        }
        c.V1 = c.A1 + 0u;
        L8001E690: ;
        c.V1 = c.V1 + 0x2u;
        c.V0 = MemoryAccess.ReadU8(m, c.V1);
        if (c.V0 != 0u) {
            c.A0 = c.A0 + 0xCu;
            goto L8001E690;
        }
        c.A0 = c.A0 + 0xCu;
        L8001E6A4: ;
        c.V0 = (uint)((int)c.A0 >> 1);
        c.S1 = c.S1 - c.V0;
        c.S0 = c.A1 + 0u;
        L8001E6B0: ;
        c.S5 = 0x02000000u;
        c.S2 = c.S3 + 0x1Fu;
        L8001E6B8: ;
        c.V0 = MemoryAccess.ReadU8(m, c.S0);
        if (c.V0 == 0u) {
            c.A0 = c.S4 + 0u;
            goto L8001E730;
        }
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.V0 = MemoryAccess.ReadU8(m, (c.S0 + 0x1u));
        c.A1 = c.A1 << 8;
        c.A1 = c.A1 | c.V0;
        c.RA = 0x8001E6DCu;
        GranTurismo2ArcadePC.func_8007F09C(c, m);
        if (c.V0 == 0u) {
            c.A0 = c.V0 + 0u;
            goto L8001E724;
        }
        c.A0 = c.V0 + 0u;
        c.A1 = c.S7 + 0u;
        c.A2 = c.S6 | c.S5;
        c.RA = 0x8001E6F0u;
        GranTurismo2ArcadePC.func_8007EF2C(c, m);
        c.V1 = MemoryAccess.ReadU16(m, (c.V0 + 0x12u));
        c.A0 = c.S1 + 0xFu;
        MemoryAccess.WriteU16(m, (c.V0 + 0x4u), (ushort)c.S1);
        MemoryAccess.WriteU16(m, (c.V0 + 0x6u), (ushort)c.S3);
        MemoryAccess.WriteU16(m, (c.V0 + 0xCu), (ushort)c.A0);
        MemoryAccess.WriteU16(m, (c.V0 + 0xEu), (ushort)c.S3);
        MemoryAccess.WriteU16(m, (c.V0 + 0x14u), (ushort)c.S1);
        MemoryAccess.WriteU16(m, (c.V0 + 0x16u), (ushort)c.S2);
        MemoryAccess.WriteU16(m, (c.V0 + 0x1Cu), (ushort)c.A0);
        MemoryAccess.WriteU16(m, (c.V0 + 0x1Eu), (ushort)c.S2);
        c.V1 = c.V1 & 0xFF9Fu;
        c.V1 = c.V1 | 0x0020u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x12u), (ushort)c.V1);
        L8001E724: ;
        c.S1 = c.S1 + 0xCu;
        c.S0 = c.S0 + 0x2u;
        goto L8001E6B8;
        L8001E730: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x30u));
        c.S7 = MemoryAccess.ReadU32(m, (c.SP + 0x2Cu));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x38u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001E75C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0x801D0000u;
        c.S0 = c.S0 - 0x6CC0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.RA = 0x8001E780u;
        GranTurismo2ArcadePC.func_80020938(c, m);
        c.A0 = 0x00000001u;
        c.S2 = c.S0 + 0x3C74u;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x3C74u));
        c.V1 = (int)c.V1 < 100 ? 1u : 0u;
        if (c.V1 == 0u) {
            c.A1 = c.V0 + 0u;
            goto L8001E7C8;
        }
        c.A1 = c.V0 + 0u;
        c.V0 = c.S1 << 2;
        c.V0 = c.V0 + c.S1;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.S1;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.A1;
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x94u));
        c.V0 = MemoryAccess.ReadU32(m, (c.S2 + 0x4014u));
        c.V0 = (int)c.V0 < (int)c.V1 ? 1u : 0u;
        c.A0 = c.V0 << (int)(c.A0 & 31u);
        L8001E7C8: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = c.A0 + 0u;
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001E7E4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0x801D0000u;
        c.S0 = c.S0 - 0x6CC0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.S0 = c.S0 + 0x3C74u;
        c.RA = 0x8001E808u;
        GranTurismo2ArcadePC.func_80020938(c, m);
        c.V1 = c.S1 << 2;
        c.V1 = c.V1 + c.S1;
        c.V1 = c.V1 << 3;
        c.V1 = c.V1 + c.S1;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + 0x4u;
        c.S1 = c.V0 + c.V1;
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x8001E830u;
        GranTurismo2ArcadePC.func_8005E700(c, m);
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x4014u));
        c.V1 = MemoryAccess.ReadU32(m, (c.S1 + 0x90u));
        c.V0 = c.V0 - c.V1;
        MemoryAccess.WriteU32(m, (c.S0 + 0x4014u), c.V0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001E858(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0xB8u;
        MemoryAccess.WriteU32(m, (c.SP + 0x98u), c.S2);
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0xC8u));
        MemoryAccess.WriteU32(m, (c.SP + 0xA0u), c.S4);
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        MemoryAccess.WriteU32(m, (c.SP + 0xA8u), c.S6);
        c.S6 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x90u), c.S0);
        c.S0 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0xA4u), c.S5);
        c.S5 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0xACu), c.S7);
        c.S7 = c.A3 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0xB0u), c.FP);
        c.FP = c.S7 + 0x8u;
        MemoryAccess.WriteU32(m, (c.SP + 0xB4u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x9Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x94u), c.S1);
        c.RA = 0x8001E8A4u;
        GranTurismo2ArcadePC.func_80020938(c, m);
        c.S1 = c.SP + 0x30u;
        c.A0 = c.S1 + 0u;
        c.A1 = 0x0000001Eu;
        c.V1 = c.S0 << 2;
        c.V1 = c.V1 + c.S0;
        c.V1 = c.V1 << 3;
        c.V1 = c.V1 + c.S0;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + 0x4u;
        c.S0 = c.V0 + c.V1;
        c.RA = 0x8001E8D0u;
        GranTurismo2ArcadePC.func_8006AB78(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6D40u;
        c.RA = 0x8001E8E0u;
        GranTurismo2ArcadePC.func_8007D990(c, m);
        c.V0 = 0xFF9F0000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.SP + 0x3Cu));
        c.V0 = c.V0 | 0xFFFFu;
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.S6);
        c.V1 = c.V1 & c.V0;
        c.V0 = 0x00200000u;
        c.V1 = c.V1 | c.V0;
        c.S3 = c.S2 + 0u;
        if ((int)c.S4 >= 0) {
            MemoryAccess.WriteU32(m, (c.SP + 0x3Cu), c.V1);
            goto L8001E914;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x3Cu), c.V1);
        c.V0 = c.S3 + 0x80u;
        c.S3 = c.V0 + c.S4;
        goto L8001E920;
        L8001E914: ;
        { var _r = (long)(int)c.S3 * (int)c.S4; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.T0 = c.LO;
        c.S3 = (uint)((int)c.T0 >> 7);
        L8001E920: ;
        c.V0 = (int)c.S3 < 256 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = 0x55550000u;
            goto L8001E930;
        }
        c.V0 = 0x55550000u;
        c.S3 = 0x000000FFu;
        L8001E930: ;
        c.V0 = c.V0 | 0x5556u;
        { var _r = (long)(int)c.S3 * (int)c.V0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V1 = (uint)((int)c.S3 >> 31);
        c.V0 = c.S3 - c.V1;
        c.V0 = (uint)((int)c.V0 >> 1);
        c.A0 = c.V0 << 8;
        c.V0 = c.V0 | c.A0;
        c.T0 = c.HI;
        c.V1 = c.T0 - c.V1;
        c.V1 = c.V1 << 16;
        c.V0 = c.V0 | c.V1;
        c.V1 = 0x02000000u;
        c.V0 = c.V0 | c.V1;
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.V0);
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x8Cu));
        c.S1 = 0x00000001u;
        c.RA = 0x8001E974u;
        GranTurismo2ArcadePC.func_800609F8(c, m);
        c.S2 = c.SP + 0x30u;
        c.A0 = c.S2 + 0u;
        c.A1 = c.V0 + 0u;
        c.A2 = c.S5 - 0x6Eu;
        c.A3 = c.FP + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S1);
        c.RA = 0x8001E990u;
        GranTurismo2ArcadePC.func_8006ABA0(c, m);
        c.V0 = c.S5 - 0x7Cu;
        MemoryAccess.WriteU16(m, (c.SP + 0x20u), (ushort)c.V0);
        c.V0 = c.S7 - 0x8u;
        MemoryAccess.WriteU16(m, (c.SP + 0x22u), (ushort)c.V0);
        c.V0 = 0x00000009u;
        MemoryAccess.WriteU16(m, (c.SP + 0x24u), (ushort)c.V0);
        c.V0 = 0x00000010u;
        MemoryAccess.WriteU16(m, (c.SP + 0x26u), (ushort)c.V0);
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x8Cu));
        c.A1 = MemoryAccess.ReadU32(m, (c.S0 + 0x4u));
        c.RA = 0x8001E9C0u;
        GranTurismo2ArcadePC.func_80060C38(c, m);
        c.A0 = c.S6 + 0u;
        c.A1 = c.SP + 0x20u;
        c.A2 = c.S3 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.V0);
        c.RA = 0x8001E9D4u;
        GranTurismo2ArcadePC.func_8006BA18(c, m);
        c.A1 = MemoryAccess.ReadU32(m, (c.S0 + 0x90u));
        c.S0 = c.SP + 0x50u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x8001E9E4u;
        GranTurismo2ArcadePC.func_8001E48C(c, m);
        c.A0 = c.S2 + 0u;
        c.A1 = c.S0 + 0u;
        c.A2 = c.S5 + 0x78u;
        c.A3 = c.FP + 0u;
        c.V0 = 0xFFFFFFFEu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.RA = 0x8001EA08u;
        GranTurismo2ArcadePC.func_8006B094(c, m);
        c.A0 = c.S6 + 0u;
        c.A1 = c.S3 << 8;
        c.A1 = c.S3 | c.A1;
        c.V0 = c.S3 << 16;
        c.A1 = c.A1 | c.V0;
        c.RA = 0x8001EA20u;
        GranTurismo2ArcadePC.func_80081388(c, m);
        c.A1 = 0x80050000u;
        c.A0 = MemoryAccess.ReadU16(m, (c.A1 - 0x512Cu));
        c.A1 = c.A1 - 0x512Cu;
        c.V1 = MemoryAccess.ReadU16(m, (c.A1 + 0x2u));
        c.A0 = c.A0 << 16;
        c.A0 = (uint)((int)c.A0 >> 17);
        c.A0 = c.S5 - c.A0;
        c.V1 = c.V1 << 16;
        c.V1 = (uint)((int)c.V1 >> 17);
        c.V1 = c.S7 - c.V1;
        c.V1 = c.V1 << 16;
        c.A0 = c.A0 + c.V1;
        MemoryAccess.WriteU32(m, c.V0, c.A0);
        c.V1 = MemoryAccess.ReadU32(m, (c.A1 - 0x4u));
        c.A1 = c.A1 - 0x4u;
        MemoryAccess.WriteU32(m, (c.V0 + 0x4u), c.V1);
        c.V1 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        MemoryAccess.WriteU32(m, (c.V0 + 0x8u), c.V1);
        c.A1 = MemoryAccess.ReadU16(m, (c.A1 + 0x8u));
        c.A0 = c.S6 + 0u;
        c.RA = 0x8001EA78u;
        GranTurismo2ArcadePC.func_8007D954(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0xB4u));
        c.FP = MemoryAccess.ReadU32(m, (c.SP + 0xB0u));
        c.S7 = MemoryAccess.ReadU32(m, (c.SP + 0xACu));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0xA8u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0xA4u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0xA0u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x9Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x98u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x94u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x90u));
        c.SP = c.SP + 0xB8u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001EAA8_gt2_arcade_overlay_1(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x58u;
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.S2);
        c.S2 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x3Cu), c.S1);
        c.S1 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x4Cu), c.S5);
        c.S5 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.S3);
        c.V0 = c.A0 < 0x00000009u ? 1u : 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x54u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x50u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x48u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x38u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, c.S2);
        c.S4 = MemoryAccess.ReadU32(m, (c.S2 + 0x4u));
        c.S6 = MemoryAccess.ReadU32(m, (c.S0 + 0x2Cu));
        if (c.V0 == 0u) {
            c.S3 = c.S5 + 0u;
            goto L8001ECDC;
        }
        c.S3 = c.S5 + 0u;
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x32CCu;
        c.V1 = c.A0 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x8001ECDCu: goto L8001ECDC;
            case 0x8001EB10u: goto L8001EB10;
            case 0x8001ECC8u: goto L8001ECC8;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L8001EB10: ;
        if ((int)c.S1 < 0) {
            c.V0 = c.S5 + 0u;
            goto L8001ECE0;
        }
        c.V0 = c.S5 + 0u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x24u));
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x14u));
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x6u));
        c.V0 = c.V0 << 7;
        if (c.V1 != 0u) { if ((int)c.V0 == int.MinValue && (int)c.V1 == -1) { c.LO = 0x80000000u; c.HI = 0u; } else { c.LO = (uint)((int)c.V0 / (int)c.V1); c.HI = (uint)((int)c.V0 % (int)c.V1); } }
        c.T0 = c.LO;
        if (c.S1 != c.A0) {
            goto L8001EB44;
        }
        c.A0 = c.S0 + 0u;
        c.RA = 0x8001EB40u;
        GranTurismo2ArcadePC.func_8006CE74(c, m);
        c.T0 = 0u - c.V0;
        L8001EB44: ;
        if (c.T0 == 0u) {
            c.V0 = c.S5 + 0u;
            goto L8001ECE0;
        }
        c.V0 = c.S5 + 0u;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x26u));
        c.V0 = ~(0u | c.V1);
        c.V0 = c.V0 >> 31;
        c.V1 = (int)c.V1 < -1 ? 1u : 0u;
        c.V0 = c.V0 | c.V1;
        if (c.V0 == 0u) {
            c.A0 = c.S4 + 0x4u;
            goto L8001EB88;
        }
        c.A0 = c.S4 + 0x4u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0xCu));
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0xEu));
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x10u));
        c.A1 = c.S1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.T0);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.RA = 0x8001EB88u;
        GranTurismo2ArcadePC.func_8001E858(c, m);
        L8001EB88: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x6u));
        if (c.S1 != c.V0) {
            c.V0 = c.S5 + 0u;
            goto L8001ECE0;
        }
        c.V0 = c.S5 + 0u;
        c.S2 = 0x00600000u;
        c.S2 = c.S2 | 0x6060u;
        c.A0 = c.S1 + 0u;
        c.RA = 0x8001EBA8u;
        GranTurismo2ArcadePC.func_8001E75C(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L8001EBE8;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L8001EBD0;
        }
        c.V0 = 0x00000002u;
        if (c.V1 == 0u) {
            c.V0 = 0x801C0000u;
            goto L8001EBE0;
        }
        c.V0 = 0x801C0000u;
        goto L8001EC00;
        L8001EBD0: ;
        if (c.V1 == c.V0) {
            c.V0 = 0x801C0000u;
            goto L8001EBF4;
        }
        c.V0 = 0x801C0000u;
        goto L8001EC00;
        L8001EBE0: ;
        c.S3 = c.V0 - 0x6463u;
        goto L8001EC00;
        L8001EBE8: ;
        c.V0 = 0x801C0000u;
        c.S3 = c.V0 - 0x6433u;
        goto L8001EBF8;
        L8001EBF4: ;
        c.S3 = c.V0 - 0x644Fu;
        L8001EBF8: ;
        c.S2 = 0x00140000u;
        c.S2 = c.S2 | 0x2864u;
        L8001EC00: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S6 + 0x14u));
        if (c.V0 != 0u) {
            c.V1 = 0u + 0u;
            goto L8001ECDC;
        }
        c.V1 = 0u + 0u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x26u));
        c.V0 = ~(0u | c.V0);
        if (c.S3 == 0u) {
            c.V0 = c.V0 >> 31;
            goto L8001EC28;
        }
        c.V0 = c.V0 >> 31;
        c.V1 = c.V0 & 0x0001u;
        L8001EC28: ;
        if (c.V1 == 0u) {
            c.V0 = 0x801D0000u;
            goto L8001ECDC;
        }
        c.V0 = 0x801D0000u;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6CC0u));
        if (c.V0 != 0u) {
            c.A0 = c.SP + 0x18u;
            goto L8001EC68;
        }
        c.A0 = c.SP + 0x18u;
        c.A0 = c.S4 + 0u;
        c.A1 = c.S3 + 0u;
        c.A2 = 0x000000B0u;
        c.A3 = 0x00000182u;
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S2);
        c.RA = 0x8001EC60u;
        GranTurismo2ArcadePC.func_8001E624(c, m);
        c.V0 = c.S5 + 0u;
        goto L8001ECE0;
        L8001EC68: ;
        c.A1 = 0x0000001Eu;
        c.RA = 0x8001EC70u;
        GranTurismo2ArcadePC.func_8006AB78(c, m);
        c.A0 = c.SP + 0x18u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6D40u;
        c.RA = 0x8001EC80u;
        GranTurismo2ArcadePC.func_8007D990(c, m);
        c.V0 = 0xFF9F0000u;
        c.V0 = c.V0 | 0xFFFFu;
        c.A0 = c.SP + 0x18u;
        c.A1 = c.S3 + 0u;
        c.A2 = 0x000000B0u;
        c.V1 = MemoryAccess.ReadU32(m, (c.A0 + 0xCu));
        c.A3 = 0x0000019Au;
        MemoryAccess.WriteU32(m, (c.A0 + 0x10u), c.S4);
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.S2);
        c.V1 = c.V1 & c.V0;
        c.V0 = 0x00200000u;
        c.V1 = c.V1 | c.V0;
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.A0 + 0xCu), c.V1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.RA = 0x8001ECC0u;
        GranTurismo2ArcadePC.func_8006ACC4(c, m);
        c.V0 = c.S5 + 0u;
        goto L8001ECE0;
        L8001ECC8: ;
        if ((int)c.S1 < 0) {
            c.S5 = 0u + 0u;
            goto L8001ECDC;
        }
        c.S5 = 0u + 0u;
        c.A0 = c.S1 + 0u;
        c.RA = 0x8001ECD8u;
        GranTurismo2ArcadePC.func_8001E75C(c, m);
        c.S5 = c.V0 < 0x00000001u ? 1u : 0u;
        L8001ECDC: ;
        c.V0 = c.S5 + 0u;
        L8001ECE0: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x54u));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x50u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x4Cu));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x48u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x44u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x40u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x3Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x38u));
        c.SP = c.SP + 0x58u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001ED08(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.RA = 0x8001ED1Cu;
        GranTurismo2ArcadePC.func_80020938(c, m);
        c.V1 = 0x800B0000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 - 0x75A0u));
        c.S0 = MemoryAccess.ReadU32(m, (c.V1 + 0x1C4u));
        c.S1 = c.V0 + 0u;
        c.RA = 0x8001ED34u;
        GranTurismo2ArcadePC.func_8001E5AC(c, m);
        c.A3 = 0x80050000u;
        c.A0 = c.A3 - 0x46F0u;
        c.A1 = 0x80020000u;
        c.A1 = c.A1 - 0x1558u;
        c.V0 = 0x80050000u;
        c.V1 = 0xFFFFFFFFu;
        MemoryAccess.WriteU16(m, (c.V0 - 0x4710u), (ushort)c.V1);
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU16(m, (c.V0 - 0x46F4u), (ushort)c.V1);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.S1);
        c.A2 = c.S0 + 0u;
        MemoryAccess.WriteU16(m, (c.A3 - 0x46F0u), (ushort)c.V0);
        c.RA = 0x8001ED68u;
        GranTurismo2ArcadePC.func_8006CCDC(c, m);
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x11E8u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x46BCu;
        c.A2 = 0u + 0u;
        c.RA = 0x8001ED80u;
        GranTurismo2ArcadePC.func_8006E0DC(c, m);
        c.V0 = 0x0000000Cu;
        MemoryAccess.WriteU32(m, (c.S0 + 0x14u), 0u);
        MemoryAccess.WriteU32(m, (c.S0 + 0x18u), c.V0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001EDA0(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        c.S4 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1C4u));
        if (c.A0 != 0u) {
            c.S0 = c.V0 + 0x1A4u;
            goto L8001EDD8;
        }
        c.S0 = c.V0 + 0x1A4u;
        c.S0 = c.S4 + 0u;
        L8001EDD8: ;
        c.V0 = 0x80050000u;
        c.S3 = c.V0 - 0x4728u;
        c.A0 = c.S3 + 0u;
        c.RA = 0x8001EDE8u;
        GranTurismo2ArcadePC.func_8006BD74(c, m);
        c.V0 = 0x80050000u;
        c.S2 = c.V0 - 0x470Cu;
        c.A0 = c.S2 + 0u;
        c.RA = 0x8001EDF8u;
        GranTurismo2ArcadePC.func_8006BD74(c, m);
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x18u));
        if ((int)c.V0 <= 0) {
            c.V0 = c.V0 - 0x1u;
            goto L8001EE24;
        }
        c.V0 = c.V0 - 0x1u;
        if (c.V0 != 0u) {
            MemoryAccess.WriteU32(m, (c.S1 + 0x18u), c.V0);
            goto L8001EE24;
        }
        MemoryAccess.WriteU32(m, (c.S1 + 0x18u), c.V0);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x46F0u;
        c.RA = 0x8001EE1Cu;
        GranTurismo2ArcadePC.func_8006CD80(c, m);
        MemoryAccess.WriteU16(m, (c.S3 + 0x18u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.S2 + 0x18u), (ushort)0u);
        L8001EE24: ;
        c.V1 = MemoryAccess.ReadU32(m, (c.S1 + 0x14u));
        if (c.V1 == 0u) {
            c.V0 = 0x00000001u;
            goto L8001EE44;
        }
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            c.V0 = c.S4 + 0u;
            goto L8001EF38;
        }
        c.V0 = c.S4 + 0u;
        goto L8001EFBC;
        L8001EE44: ;
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x11E8u;
        c.A1 = 0u + 0u;
        c.RA = 0x8001EE54u;
        GranTurismo2ArcadePC.func_8006E34C(c, m);
        c.V0 = 0x80050000u;
        c.S2 = c.V0 - 0x46F0u;
        c.A0 = c.S2 + 0u;
        c.A1 = c.S0 + 0u;
        c.RA = 0x8001EE68u;
        GranTurismo2ArcadePC.func_8006CED4(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = 0xFFFFFFFDu;
        if (c.V1 == c.V0) {
            c.V0 = (int)c.V1 < -2 ? 1u : 0u;
            goto L8001EEAC;
        }
        c.V0 = (int)c.V1 < -2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0xFFFFFFFCu;
            goto L8001EE90;
        }
        c.V0 = 0xFFFFFFFCu;
        if (c.V1 == c.V0) {
            goto L8001EEBC;
        }
        goto L8001EF10;
        L8001EE90: ;
        c.V0 = 0xFFFFFFFEu;
        if (c.V1 == c.V0) {
            c.V0 = 0xFFFFFFFFu;
            goto L8001EFB8;
        }
        c.V0 = 0xFFFFFFFFu;
        if (c.V1 == c.V0) {
            goto L8001EECC;
        }
        goto L8001EF10;
        L8001EEAC: ;
        c.A0 = 0x00000006u;
        c.RA = 0x8001EEB4u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.V0 = c.S4 + 0u;
        goto L8001EFBC;
        L8001EEBC: ;
        c.A0 = 0u + 0u;
        c.RA = 0x8001EEC4u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.V0 = c.S4 + 0u;
        goto L8001EFBC;
        L8001EECC: ;
        c.A0 = c.S2 + 0u;
        c.RA = 0x8001EED4u;
        GranTurismo2ArcadePC.func_8006CDE8(c, m);
        c.A0 = 0x00000004u;
        c.S4 = 0x00000002u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x4728u;
        c.A2 = 0x80050000u;
        c.A2 = c.A2 - 0x470Cu;
        c.V0 = MemoryAccess.ReadU16(m, (c.A1 + 0x4u));
        c.V1 = MemoryAccess.ReadU16(m, (c.A2 + 0x4u));
        c.V0 = ~(0u | c.V0);
        c.V1 = ~(0u | c.V1);
        MemoryAccess.WriteU16(m, (c.A1 + 0x18u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.A2 + 0x18u), (ushort)c.V1);
        c.RA = 0x8001EF08u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.V0 = c.S4 + 0u;
        goto L8001EFBC;
        L8001EF10: ;
        c.A0 = 0x00000001u;
        c.RA = 0x8001EF18u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.S0 = 0x800B0000u;
        c.S0 = c.S0 + 0x11E8u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x8001EF28u;
        GranTurismo2ArcadePC.func_8006E298(c, m);
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x7Du), (byte)c.V0);
        MemoryAccess.WriteU32(m, (c.S1 + 0x14u), c.V0);
        goto L8001EFB8;
        L8001EF38: ;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x46F0u;
        c.A1 = 0u + 0u;
        c.RA = 0x8001EF48u;
        GranTurismo2ArcadePC.func_8006CED4(c, m);
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x11E8u;
        c.A1 = c.S0 + 0u;
        c.RA = 0x8001EF58u;
        GranTurismo2ArcadePC.func_8006E34C(c, m);
        c.V1 = c.V0 + 0x3u;
        c.V0 = c.V1 < 0x00000005u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80020000u;
            goto L8001EFB8;
        }
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x32F4u;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x8001EFB8u: goto L8001EFB8;
            case 0x8001EF84u: goto L8001EF84;
            case 0x8001EF8Cu: goto L8001EF8C;
            case 0x8001EF9Cu: goto L8001EF9C;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L8001EF84: ;
        c.A0 = 0x00000002u;
        goto L8001EFA0;
        L8001EF8C: ;
        c.V0 = 0x80050000u;
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 - 0x46EAu));
        c.RA = 0x8001EF9Cu;
        GranTurismo2ArcadePC.func_8001E7E4(c, m);
        L8001EF9C: ;
        c.A0 = 0x00000001u;
        L8001EFA0: ;
        c.RA = 0x8001EFA8u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x11E8u;
        c.RA = 0x8001EFB4u;
        GranTurismo2ArcadePC.func_8006E30C(c, m);
        MemoryAccess.WriteU32(m, (c.S1 + 0x14u), 0u);
        L8001EFB8: ;
        c.V0 = c.S4 + 0u;
        L8001EFBC: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001EFDC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x160u;
        MemoryAccess.WriteU32(m, (c.SP + 0x15Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x158u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x154u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x150u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x14Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x148u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x144u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x140u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.A0 = c.SP + 0x20u;
        c.A1 = 0x0000001Eu;
        c.V1 = 0x801D0000u;
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x75A0u));
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1C4u));
        c.S6 = c.V1 - 0x6CC0u;
        c.RA = 0x8001F028u;
        GranTurismo2ArcadePC.func_8006AB78(c, m);
        c.A0 = c.SP + 0x20u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6D40u;
        c.RA = 0x8001F038u;
        GranTurismo2ArcadePC.func_8007D990(c, m);
        c.V1 = 0xFF9F0000u;
        c.V1 = c.V1 | 0xFFFFu;
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x11E8u;
        c.A1 = c.S0 + 0x10u;
        c.S3 = c.SP + 0x20u;
        c.A2 = c.S3 + 0u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S3 + 0xCu));
        c.S5 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.S3 + 0x10u), c.A1);
        MemoryAccess.WriteU8(m, (c.S3 + 0x1Cu), (byte)c.S5);
        c.V0 = c.V0 & c.V1;
        c.V1 = 0x00200000u;
        c.V0 = c.V0 | c.V1;
        MemoryAccess.WriteU32(m, (c.S3 + 0xCu), c.V0);
        c.RA = 0x8001F078u;
        GranTurismo2ArcadePC.func_8006E4C8(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x46F0u;
        c.S4 = c.S0 + 0x18u;
        c.A1 = c.S4 + 0u;
        c.RA = 0x8001F08Cu;
        GranTurismo2ArcadePC.func_8006D41C(c, m);
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x14u));
        if (c.V0 == c.S5) {
            c.S0 = c.S6 + 0x3C74u;
            goto L8001F164;
        }
        c.S0 = c.S6 + 0x3C74u;
        c.S1 = c.SP + 0x40u;
        c.A1 = MemoryAccess.ReadU32(m, (c.S0 + 0x4014u));
        c.A0 = c.S1 + 0u;
        MemoryAccess.WriteU32(m, (c.S3 + 0x10u), c.S4);
        c.RA = 0x8001F0B0u;
        GranTurismo2ArcadePC.func_8001E48C(c, m);
        c.V0 = 0x02600000u;
        c.V0 = c.V0 | 0x4214u;
        c.A0 = c.S3 + 0u;
        c.S0 = 0x801C0000u;
        c.S0 = c.S0 - 0x63FEu;
        c.A1 = c.S0 + 0u;
        c.A2 = 0x00000040u;
        c.A3 = 0x000001B8u;
        MemoryAccess.WriteU32(m, (c.S3 + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S5);
        c.RA = 0x8001F0DCu;
        GranTurismo2ArcadePC.func_8006AD38(c, m);
        c.A0 = c.S3 + 0u;
        c.A1 = c.S1 + 0u;
        c.A2 = 0x00000090u;
        c.A3 = 0x000001B8u;
        c.S2 = 0xFFFFFFFEu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.RA = 0x8001F100u;
        GranTurismo2ArcadePC.func_8006B094(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4728u;
        c.A1 = c.S4 + 0u;
        c.A2 = 0x00000020u;
        c.A3 = 0x000001A4u;
        c.RA = 0x8001F118u;
        GranTurismo2ArcadePC.func_8006BE04(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S0 + 0x4u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S6 + 0x3C74u));
        c.A3 = 0x00000064u;
        c.RA = 0x8001F12Cu;
        GranTurismo2ArcadePC.func_8008CE44(c, m);
        c.A0 = c.S3 + 0u;
        c.A1 = c.S1 + 0u;
        c.A2 = 0x00000136u;
        c.A3 = 0x000001B8u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.RA = 0x8001F14Cu;
        GranTurismo2ArcadePC.func_8006B094(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x470Cu;
        c.A1 = c.S4 + 0u;
        c.A2 = 0x00000146u;
        c.A3 = 0x000001A4u;
        c.RA = 0x8001F164u;
        GranTurismo2ArcadePC.func_8006BE04(c, m);
        L8001F164: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x15Cu));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x158u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x154u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x150u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x14Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x148u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x144u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x140u));
        c.SP = c.SP + 0x160u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001F18C(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V1 = 0x801D0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1280u));
        c.V1 = MemoryAccess.ReadU8(m, (c.V1 - 0x6CC0u));
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        if (c.V1 != 0u) {
            goto L8001F1E0;
        }
        c.A0 = c.S1 + 0u;
        c.RA = 0x8001F1C0u;
        GranTurismo2ArcadePC.func_8007F084(c, m);
        c.A0 = c.S1 + 0u;
        c.S0 = 0x801F0000u;
        c.S0 = c.S0 - 0xAA0u;
        c.A1 = c.S0 + 0u;
        c.RA = 0x8001F1D4u;
        GranTurismo2ArcadePC.func_8006EBE8(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S0 + 0x11u;
        c.RA = 0x8001F1E0u;
        GranTurismo2ArcadePC.func_8006EBE8(c, m);
        L8001F1E0: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001F1F4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V0 = 0x800B0000u;
        c.V1 = 0x801D0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1280u));
        c.V1 = MemoryAccess.ReadU8(m, (c.V1 - 0x6CC0u));
        c.A1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        if (c.V1 != 0u) {
            goto L8001F224;
        }
        c.RA = 0x8001F224u;
        GranTurismo2ArcadePC.func_8006EBE8(c, m);
        L8001F224: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001F234(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x38u;
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.S7);
        c.S7 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = c.A3 + 0u;
        c.V0 = 0x800B0000u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0x48u));
        c.V1 = 0x801D0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S6);
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x4Cu));
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1280u));
        c.V1 = MemoryAccess.ReadU8(m, (c.V1 - 0x6CC0u));
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S4 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        if (c.V1 != 0u) {
            c.A0 = 0u + 0u;
            goto L8001F344;
        }
        c.A0 = 0u + 0u;
        if (c.A2 == 0u) {
            c.S0 = c.A1 + 0u;
            goto L8001F2C4;
        }
        c.S0 = c.A1 + 0u;
        c.V0 = MemoryAccess.ReadU8(m, c.A1);
        if (c.V0 == 0u) {
            c.V1 = c.A1 + 0u;
            goto L8001F2B8;
        }
        c.V1 = c.A1 + 0u;
        L8001F2A4: ;
        c.V1 = c.V1 + 0x2u;
        c.V0 = MemoryAccess.ReadU8(m, c.V1);
        if (c.V0 != 0u) {
            c.A0 = c.A0 + 0xCu;
            goto L8001F2A4;
        }
        c.A0 = c.A0 + 0xCu;
        L8001F2B8: ;
        c.V0 = (uint)((int)c.A0 >> 1);
        c.S1 = c.S1 - c.V0;
        c.S0 = c.A1 + 0u;
        L8001F2C4: ;
        c.S5 = 0x02000000u;
        c.S2 = c.S3 + 0x1Fu;
        L8001F2CC: ;
        c.V0 = MemoryAccess.ReadU8(m, c.S0);
        if (c.V0 == 0u) {
            c.A0 = c.S4 + 0u;
            goto L8001F344;
        }
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.V0 = MemoryAccess.ReadU8(m, (c.S0 + 0x1u));
        c.A1 = c.A1 << 8;
        c.A1 = c.A1 | c.V0;
        c.RA = 0x8001F2F0u;
        GranTurismo2ArcadePC.func_8007F09C(c, m);
        if (c.V0 == 0u) {
            c.A0 = c.V0 + 0u;
            goto L8001F338;
        }
        c.A0 = c.V0 + 0u;
        c.A1 = c.S7 + 0u;
        c.A2 = c.S6 | c.S5;
        c.RA = 0x8001F304u;
        GranTurismo2ArcadePC.func_8007EF2C(c, m);
        c.V1 = MemoryAccess.ReadU16(m, (c.V0 + 0x12u));
        c.A0 = c.S1 + 0xFu;
        MemoryAccess.WriteU16(m, (c.V0 + 0x4u), (ushort)c.S1);
        MemoryAccess.WriteU16(m, (c.V0 + 0x6u), (ushort)c.S3);
        MemoryAccess.WriteU16(m, (c.V0 + 0xCu), (ushort)c.A0);
        MemoryAccess.WriteU16(m, (c.V0 + 0xEu), (ushort)c.S3);
        MemoryAccess.WriteU16(m, (c.V0 + 0x14u), (ushort)c.S1);
        MemoryAccess.WriteU16(m, (c.V0 + 0x16u), (ushort)c.S2);
        MemoryAccess.WriteU16(m, (c.V0 + 0x1Cu), (ushort)c.A0);
        MemoryAccess.WriteU16(m, (c.V0 + 0x1Eu), (ushort)c.S2);
        c.V1 = c.V1 & 0xFF9Fu;
        c.V1 = c.V1 | 0x0020u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x12u), (ushort)c.V1);
        L8001F338: ;
        c.S1 = c.S1 + 0xCu;
        c.S0 = c.S0 + 0x2u;
        goto L8001F2CC;
        L8001F344: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x30u));
        c.S7 = MemoryAccess.ReadU32(m, (c.SP + 0x2Cu));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x38u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001F370(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V1 = 0x801D0000u;
        c.V1 = MemoryAccess.ReadU8(m, (c.V1 - 0x6CC0u));
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1280u));
        if (c.V1 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
            goto L8001F3B0;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.RA = 0x8001F398u;
        GranTurismo2ArcadePC.func_8001F18C(c, m);
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x28u));
        c.RA = 0x8001F3A4u;
        GranTurismo2ArcadePC.func_8001F1F4(c, m);
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x2Cu));
        c.RA = 0x8001F3B0u;
        GranTurismo2ArcadePC.func_8001F1F4(c, m);
        L8001F3B0: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001F3C0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V1 = 0x801D0000u;
        c.V1 = MemoryAccess.ReadU8(m, (c.V1 - 0x6CC0u));
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1280u));
        if (c.V1 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
            goto L8001F3F4;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.RA = 0x8001F3E8u;
        GranTurismo2ArcadePC.func_8001F18C(c, m);
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x24u));
        c.RA = 0x8001F3F4u;
        GranTurismo2ArcadePC.func_8001F1F4(c, m);
        L8001F3F4: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001F404(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1280u));
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x344u));
        if (c.A0 == c.S3) {
            c.S2 = 0xFFFFFFFFu;
            goto L8001F478;
        }
        c.S2 = 0xFFFFFFFFu;
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L8001F500;
        }
        c.V0 = c.S2 + 0u;
        if (c.A0 != 0u) {
            c.S0 = c.S1 + 0x2B0u;
            goto L8001F500;
        }
        c.S0 = c.S1 + 0x2B0u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x8001F450u;
        GranTurismo2ArcadePC.func_8006E298(c, m);
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0xD49u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x7Du), (byte)c.S3);
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x330Cu;
        MemoryAccess.WriteU32(m, (c.S1 + 0x2Cu), c.V0);
        c.RA = 0x8001F470u;
        GranTurismo2ArcadePC.func_8001F370(c, m);
        c.V0 = c.S2 + 0u;
        goto L8001F500;
        L8001F478: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x10u));
        c.V1 = MemoryAccess.ReadU32(m, (c.S1 + 0xCu));
        c.V0 = c.V0 << 1;
        c.V1 = c.V1 + c.V0;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.V1);
        if (c.V0 == 0u) {
            goto L8001F4A8;
        }
        c.A0 = c.S1 + 0x2B0u;
        c.RA = 0x8001F4A0u;
        GranTurismo2ArcadePC.func_8006E30C(c, m);
        c.S2 = 0x00000003u;
        goto L8001F4FC;
        L8001F4A8: ;
        if (c.A1 == 0u) {
            goto L8001F4E0;
        }
        if ((int)c.A1 > 0) {
            goto L8001F4C8;
        }
        if (c.A1 == c.S2) {
            c.V0 = c.S2 + 0u;
            goto L8001F4D8;
        }
        c.V0 = c.S2 + 0u;
        goto L8001F500;
        L8001F4C8: ;
        if (c.A1 == c.A0) {
            c.V0 = c.S2 + 0u;
            goto L8001F4F0;
        }
        c.V0 = c.S2 + 0u;
        goto L8001F500;
        L8001F4D8: ;
        c.A0 = 0x00000002u;
        goto L8001F4F4;
        L8001F4E0: ;
        c.A0 = 0x00000001u;
        c.RA = 0x8001F4E8u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.S2 = 0x00000007u;
        goto L8001F4FC;
        L8001F4F0: ;
        c.A0 = 0x00000001u;
        L8001F4F4: ;
        c.S2 = 0x00000002u;
        c.RA = 0x8001F4FCu;
        GranTurismo2ArcadePC.func_80060750(c, m);
        L8001F4FC: ;
        c.V0 = c.S2 + 0u;
        L8001F500: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001F51C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = 0xFFFFFFFFu;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1280u));
        c.V0 = 0x00000001u;
        if (c.S2 == c.V0) {
            MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
            goto L8001F5D8;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        c.V0 = (int)c.S2 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L8001F564;
        }
        c.V0 = 0x00000002u;
        if (c.S2 == 0u) {
            c.V0 = c.S1 + 0u;
            goto L8001F574;
        }
        c.V0 = c.S1 + 0u;
        goto L8001F690;
        L8001F564: ;
        if (c.S2 == c.V0) {
            c.V0 = c.S1 + 0u;
            goto L8001F684;
        }
        c.V0 = c.S1 + 0u;
        goto L8001F690;
        L8001F574: ;
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x418u));
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x10u));
        c.A2 = c.A0 + 0u;
        c.RA = 0x8001F584u;
        GranTurismo2ArcadePC.func_8006A0C4(c, m);
        if (c.V0 == 0u) {
            c.V0 = 0x801F0000u;
            goto L8001F594;
        }
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0xD0Au;
        goto L8001F674;
        L8001F594: ;
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x418u));
        c.RA = 0x8001F5A0u;
        GranTurismo2ArcadePC.func_8006A03C(c, m);
        c.V1 = 0x00900000u;
        c.V1 = c.V1 | 0x500Cu;
        c.A0 = c.S0 + 0x3E0u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x10u), c.V0);
        MemoryAccess.WriteU32(m, (c.A0 + 0xCu), c.V1);
        c.RA = 0x8001F5B8u;
        GranTurismo2ArcadePC.func_8006BF5C(c, m);
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0xD34u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x28u), c.V0);
        c.V0 = c.V0 - 0x151u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x2Cu), c.V0);
        c.RA = 0x8001F5D0u;
        GranTurismo2ArcadePC.func_8001F370(c, m);
        c.V0 = c.S1 + 0u;
        goto L8001F690;
        L8001F5D8: ;
        c.RA = 0x8001F5E0u;
        GranTurismo2ArcadePC.func_8007D6DC(c, m);
        if (c.V0 == 0u) {
            goto L8001F608;
        }
        if (c.V0 != c.S2) {
            c.V0 = 0x801F0000u;
            goto L8001F66C;
        }
        c.V0 = 0x801F0000u;
        c.V0 = 0x801F0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0xC8u));
        c.A0 = c.S0 + 0x3E0u;
        c.RA = 0x8001F600u;
        GranTurismo2ArcadePC.func_8006BF84(c, m);
        c.V0 = c.S1 + 0u;
        goto L8001F690;
        L8001F608: ;
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x418u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x414u), (ushort)c.S1);
        c.A1 = c.A0 + 0u;
        c.RA = 0x8001F618u;
        GranTurismo2ArcadePC.func_8006A224(c, m);
        if (c.V0 == 0u) {
            c.V0 = 0x801F0000u;
            goto L8001F664;
        }
        c.V0 = 0x801F0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x418u));
        c.A1 = c.A0 + 0u;
        c.RA = 0x8001F62Cu;
        GranTurismo2ArcadePC.func_8006A1F4(c, m);
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, c.S0);
        if (c.A0 == 0u) {
            goto L8001F64C;
        }
        if (c.A0 != c.S2) {
            c.V0 = c.S1 + 0u;
            goto L8001F690;
        }
        c.V0 = c.S1 + 0u;
        c.S1 = 0x00000008u;
        goto L8001F68C;
        L8001F64C: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.V0);
        if ((int)c.V0 <= 0) {
            c.S1 = 0x00000009u;
            goto L8001F68C;
        }
        c.S1 = 0x00000009u;
        c.S1 = 0x00000008u;
        goto L8001F68C;
        L8001F664: ;
        c.V0 = c.V0 - 0xC0Cu;
        goto L8001F674;
        L8001F66C: ;
        c.V0 = c.V0 - 0xD0Au;
        MemoryAccess.WriteU16(m, (c.S0 + 0x414u), (ushort)c.S1);
        L8001F674: ;
        MemoryAccess.WriteU32(m, (c.S0 + 0x24u), c.V0);
        c.RA = 0x8001F67Cu;
        GranTurismo2ArcadePC.func_8001F3C0(c, m);
        c.S1 = 0x00000001u;
        goto L8001F68C;
        L8001F684: ;
        c.A0 = c.S0 + 0x3E0u;
        c.RA = 0x8001F68Cu;
        GranTurismo2ArcadePC.func_8006C084(c, m);
        L8001F68C: ;
        c.V0 = c.S1 + 0u;
        L8001F690: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001F6A8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1280u));
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x2ACu));
        if (c.A0 == c.S2) {
            c.S3 = 0xFFFFFFFFu;
            goto L8001F748;
        }
        c.S3 = 0xFFFFFFFFu;
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S3 + 0u;
            goto L8001F788;
        }
        c.V0 = c.S3 + 0u;
        if (c.A0 != 0u) {
            c.S0 = c.S1 + 0x218u;
            goto L8001F788;
        }
        c.S0 = c.S1 + 0x218u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x8001F6F4u;
        GranTurismo2ArcadePC.func_8006E298(c, m);
        MemoryAccess.WriteU8(m, (c.S0 + 0x7Du), (byte)c.S2);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, c.S1);
        if (c.V1 == c.S2) {
            c.V0 = 0x801C0000u;
            goto L8001F71C;
        }
        c.V0 = 0x801C0000u;
        c.V0 = 0x00000002u;
        if (c.V1 == c.V0) {
            c.V0 = c.S3 + 0u;
            goto L8001F724;
        }
        c.V0 = c.S3 + 0u;
        goto L8001F788;
        L8001F71C: ;
        c.V0 = c.V0 - 0x64A0u;
        goto L8001F72C;
        L8001F724: ;
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6482u;
        L8001F72C: ;
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x330Cu;
        MemoryAccess.WriteU32(m, (c.S1 + 0x2Cu), c.V0);
        c.RA = 0x8001F740u;
        GranTurismo2ArcadePC.func_8001F370(c, m);
        c.V0 = c.S3 + 0u;
        goto L8001F788;
        L8001F748: ;
        if (c.V0 == 0u) {
            goto L8001F778;
        }
        if ((int)c.V0 > 0) {
            goto L8001F768;
        }
        if (c.V0 == c.S3) {
            c.V0 = c.S3 + 0u;
            goto L8001F770;
        }
        c.V0 = c.S3 + 0u;
        goto L8001F788;
        L8001F768: ;
        if (c.V0 != c.A0) {
            c.V0 = c.S3 + 0u;
            goto L8001F788;
        }
        c.V0 = c.S3 + 0u;
        L8001F770: ;
        c.S3 = 0x0000000Du;
        goto L8001F784;
        L8001F778: ;
        c.A0 = 0x00000001u;
        c.RA = 0x8001F780u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.S3 = 0x00000002u;
        L8001F784: ;
        c.V0 = c.S3 + 0u;
        L8001F788: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001F7A4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1280u));
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x3DCu));
        if (c.A0 == c.S3) {
            c.S2 = 0xFFFFFFFFu;
            goto L8001F824;
        }
        c.S2 = 0xFFFFFFFFu;
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L8001F864;
        }
        c.V0 = c.S2 + 0u;
        if (c.A0 != 0u) {
            goto L8001F864;
        }
        c.A0 = 0u + 0u;
        c.RA = 0x8001F7F0u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.S0 = c.S1 + 0x348u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x8001F7FCu;
        GranTurismo2ArcadePC.func_8006E298(c, m);
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x64E0u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x7Du), (byte)c.S3);
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x330Cu;
        MemoryAccess.WriteU32(m, (c.S1 + 0x2Cu), c.V0);
        c.RA = 0x8001F81Cu;
        GranTurismo2ArcadePC.func_8001F370(c, m);
        c.V0 = c.S2 + 0u;
        goto L8001F864;
        L8001F824: ;
        if (c.V0 == 0u) {
            goto L8001F854;
        }
        if ((int)c.V0 > 0) {
            goto L8001F844;
        }
        if (c.V0 == c.S2) {
            c.V0 = c.S2 + 0u;
            goto L8001F84C;
        }
        c.V0 = c.S2 + 0u;
        goto L8001F864;
        L8001F844: ;
        if (c.V0 != c.A0) {
            c.V0 = c.S2 + 0u;
            goto L8001F864;
        }
        c.V0 = c.S2 + 0u;
        L8001F84C: ;
        c.S2 = 0x0000000Du;
        goto L8001F860;
        L8001F854: ;
        c.A0 = 0x00000001u;
        c.RA = 0x8001F85Cu;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.S2 = 0x00000002u;
        L8001F860: ;
        c.V0 = c.S2 + 0u;
        L8001F864: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001F880(CpuContext c, IMemory m)
    {
        c.T1 = 0u | 0xAAAAu;
        c.V1 = 0x00003770u;
        if (c.A1 == 0u) {
            c.T0 = 0u + 0u;
            goto L8001F8F0;
        }
        c.T0 = 0u + 0u;
        c.T3 = 0x00010000u;
        c.T2 = c.T3 + 0u;
        c.T2 = c.T2 | 0x1021u;
        L8001F89C: ;
        c.V0 = c.A0 + c.T0;
        c.A2 = MemoryAccess.ReadU8(m, c.V0);
        c.A3 = 0u + 0u;
        c.T1 = c.T1 + c.A2;
        c.V0 = c.A2 << 8;
        c.T1 = c.T1 ^ c.V0;
        L8001F8B4: ;
        c.V1 = c.V1 << 1;
        c.V0 = c.V1 & c.T3;
        if (c.V0 == 0u) {
            c.V0 = c.A2 >> 7;
            goto L8001F8C8;
        }
        c.V0 = c.A2 >> 7;
        c.V1 = c.V1 ^ c.T2;
        L8001F8C8: ;
        c.V0 = c.V0 & 0x0001u;
        c.V1 = c.V1 | c.V0;
        c.A3 = c.A3 + 0x1u;
        c.V0 = c.A3 < 0x00000008u ? 1u : 0u;
        if (c.V0 != 0u) {
            c.A2 = c.A2 << 1;
            goto L8001F8B4;
        }
        c.A2 = c.A2 << 1;
        c.T0 = c.T0 + 0x1u;
        c.V0 = c.T0 < c.A1 ? 1u : 0u;
        if (c.V0 != 0u) {
            goto L8001F89C;
        }
        L8001F8F0: ;
        c.V1 = c.V1 & 0xFFFFu;
        c.V0 = c.T1 << 16;
        c.V0 = c.V1 | c.V0;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001F900(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.A1 = 0x00006BA4u;
        c.RA = 0x8001F918u;
        GranTurismo2ArcadePC.func_8001F880(c, m);
        c.V1 = MemoryAccess.ReadU32(m, (c.S0 + 0x6BA4u));
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V1 = c.V1 ^ c.V0;
        c.V0 = c.V1 < 0x00000001u ? 1u : 0u;
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001F934(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1280u));
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x344u));
        if (c.A0 == c.S3) {
            c.S2 = 0xFFFFFFFFu;
            goto L8001F9A8;
        }
        c.S2 = 0xFFFFFFFFu;
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L8001FA30;
        }
        c.V0 = c.S2 + 0u;
        if (c.A0 != 0u) {
            c.S0 = c.S1 + 0x2B0u;
            goto L8001FA30;
        }
        c.S0 = c.S1 + 0x2B0u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x8001F980u;
        GranTurismo2ArcadePC.func_8006E298(c, m);
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0xD49u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x7Du), (byte)c.S3);
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x330Cu;
        MemoryAccess.WriteU32(m, (c.S1 + 0x2Cu), c.V0);
        c.RA = 0x8001F9A0u;
        GranTurismo2ArcadePC.func_8001F370(c, m);
        c.V0 = c.S2 + 0u;
        goto L8001FA30;
        L8001F9A8: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x10u));
        c.V1 = MemoryAccess.ReadU32(m, (c.S1 + 0xCu));
        c.V0 = c.V0 << 1;
        c.V1 = c.V1 + c.V0;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.V1);
        if (c.V0 == 0u) {
            goto L8001F9D8;
        }
        c.A0 = c.S1 + 0x2B0u;
        c.RA = 0x8001F9D0u;
        GranTurismo2ArcadePC.func_8006E30C(c, m);
        c.S2 = 0x00000003u;
        goto L8001FA2C;
        L8001F9D8: ;
        if (c.A1 == 0u) {
            goto L8001FA10;
        }
        if ((int)c.A1 > 0) {
            goto L8001F9F8;
        }
        if (c.A1 == c.S2) {
            c.V0 = c.S2 + 0u;
            goto L8001FA08;
        }
        c.V0 = c.S2 + 0u;
        goto L8001FA30;
        L8001F9F8: ;
        if (c.A1 == c.A0) {
            c.V0 = c.S2 + 0u;
            goto L8001FA20;
        }
        c.V0 = c.S2 + 0u;
        goto L8001FA30;
        L8001FA08: ;
        c.A0 = 0x00000002u;
        goto L8001FA24;
        L8001FA10: ;
        c.A0 = 0x00000001u;
        c.RA = 0x8001FA18u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.S2 = 0x0000000Bu;
        goto L8001FA2C;
        L8001FA20: ;
        c.A0 = 0x00000001u;
        L8001FA24: ;
        c.S2 = 0x00000002u;
        c.RA = 0x8001FA2Cu;
        GranTurismo2ArcadePC.func_80060750(c, m);
        L8001FA2C: ;
        c.V0 = c.S2 + 0u;
        L8001FA30: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001FA4C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x30u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S2);
        c.S2 = 0xFFFFFFFFu;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1280u));
        c.V0 = 0x00000001u;
        if (c.S1 == c.V0) {
            MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.RA);
            goto L8001FB14;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.RA);
        c.V0 = (int)c.S1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L8001FA94;
        }
        c.V0 = 0x00000002u;
        if (c.S1 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L8001FAA4;
        }
        c.V0 = c.S2 + 0u;
        goto L8001FB88;
        L8001FA94: ;
        if (c.S1 == c.V0) {
            c.V0 = c.S2 + 0u;
            goto L8001FB7C;
        }
        c.V0 = c.S2 + 0u;
        goto L8001FB88;
        L8001FAA4: ;
        c.A0 = 0x00000006u;
        c.S1 = 0u | 0xA000u;
        c.A2 = 0x80050000u;
        c.A2 = c.A2 - 0x4440u;
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x10u));
        c.A3 = c.S0 + 0x41Cu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.RA = 0x8001FACCu;
        GranTurismo2ArcadePC.func_8007D568(c, m);
        if (c.V0 == 0u) {
            c.V0 = 0x801F0000u;
            goto L8001FADC;
        }
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0xD0Au;
        goto L8001FB6C;
        L8001FADC: ;
        c.V0 = 0x00900000u;
        c.V0 = c.V0 | 0x500Cu;
        c.A0 = c.S0 + 0x3E0u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x10u), c.S1);
        MemoryAccess.WriteU32(m, (c.A0 + 0xCu), c.V0);
        c.RA = 0x8001FAF4u;
        GranTurismo2ArcadePC.func_8006BF5C(c, m);
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0xD34u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x28u), c.V0);
        c.V0 = c.V0 - 0x151u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x2Cu), c.V0);
        c.RA = 0x8001FB0Cu;
        GranTurismo2ArcadePC.func_8001F370(c, m);
        c.V0 = c.S2 + 0u;
        goto L8001FB88;
        L8001FB14: ;
        c.RA = 0x8001FB1Cu;
        GranTurismo2ArcadePC.func_8007D6DC(c, m);
        if (c.V0 == 0u) {
            goto L8001FB44;
        }
        if (c.V0 != c.S1) {
            c.V0 = 0x801F0000u;
            goto L8001FB64;
        }
        c.V0 = 0x801F0000u;
        c.V0 = 0x801F0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0xC8u));
        c.A0 = c.S0 + 0x3E0u;
        c.RA = 0x8001FB3Cu;
        GranTurismo2ArcadePC.func_8006BF84(c, m);
        c.V0 = c.S2 + 0u;
        goto L8001FB88;
        L8001FB44: ;
        MemoryAccess.WriteU16(m, (c.S0 + 0x414u), (ushort)c.S2);
        c.A0 = c.S0 + 0x61Cu;
        c.RA = 0x8001FB50u;
        GranTurismo2ArcadePC.func_8001F900(c, m);
        if (c.V0 != 0u) {
            c.S2 = 0x00000008u;
            goto L8001FB84;
        }
        c.S2 = 0x00000008u;
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x5FB0u;
        goto L8001FB6C;
        L8001FB64: ;
        c.V0 = c.V0 - 0xD0Au;
        MemoryAccess.WriteU16(m, (c.S0 + 0x414u), (ushort)c.S2);
        L8001FB6C: ;
        MemoryAccess.WriteU32(m, (c.S0 + 0x24u), c.V0);
        c.RA = 0x8001FB74u;
        GranTurismo2ArcadePC.func_8001F3C0(c, m);
        c.S2 = 0x00000001u;
        goto L8001FB84;
        L8001FB7C: ;
        c.A0 = c.S0 + 0x3E0u;
        c.RA = 0x8001FB84u;
        GranTurismo2ArcadePC.func_8006C084(c, m);
        L8001FB84: ;
        c.V0 = c.S2 + 0u;
        L8001FB88: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x2Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.SP = c.SP + 0x30u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001FBA0(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1280u));
        c.V0 = 0x00000001u;
        if (c.A0 == c.V0) {
            c.A1 = 0xFFFFFFFFu;
            goto L8001FBD0;
        }
        c.A1 = 0xFFFFFFFFu;
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L8001FBF0;
        }
        if (c.A0 != 0u) {
            c.V0 = 0x00000018u;
            goto L8001FBF0;
        }
        c.V0 = 0x00000018u;
        MemoryAccess.WriteU16(m, (c.V1 + 0x32u), (ushort)c.V0);
        goto L8001FBF0;
        L8001FBD0: ;
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 + 0x32u));
        c.V0 = c.V0 - 0x1u;
        MemoryAccess.WriteU16(m, (c.V1 + 0x32u), (ushort)c.V0);
        c.V0 = c.V0 << 16;
        if ((int)c.V0 >= 0) {
            goto L8001FBF0;
        }
        c.A1 = 0x00000002u;
        L8001FBF0: ;
        c.V0 = c.A1 + 0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001FBF8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x50u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x3Cu), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1280u));
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.S3);
        c.S3 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x38u), c.S0);
        c.S0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x48u), c.RA);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0xE4u));
        if (c.A0 == c.S0) {
            c.S2 = 0xFFFFFFFFu;
            goto L8001FC8C;
        }
        c.S2 = 0xFFFFFFFFu;
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L8001FC48;
        }
        c.V0 = 0x00000002u;
        if (c.A0 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L8001FC58;
        }
        c.V0 = c.S2 + 0u;
        goto L8001FD50;
        L8001FC48: ;
        if (c.A0 == c.V0) {
            c.V0 = c.S2 + 0u;
            goto L8001FCCC;
        }
        c.V0 = c.S2 + 0u;
        goto L8001FD50;
        L8001FC58: ;
        c.A0 = 0u + 0u;
        c.RA = 0x8001FC60u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.S0 = c.S1 + 0x50u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x8001FC6Cu;
        GranTurismo2ArcadePC.func_8006E298(c, m);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x330Cu;
        MemoryAccess.WriteU8(m, (c.S0 + 0x7Du), (byte)0u);
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        MemoryAccess.WriteU32(m, (c.S1 + 0x2Cu), c.V0);
        c.RA = 0x8001FC84u;
        GranTurismo2ArcadePC.func_8001F370(c, m);
        c.V0 = c.S2 + 0u;
        goto L8001FD50;
        L8001FC8C: ;
        if (c.V0 == 0u) {
            goto L8001FCBC;
        }
        if ((int)c.V0 > 0) {
            goto L8001FCAC;
        }
        if (c.V0 == c.S2) {
            c.V0 = c.S2 + 0u;
            goto L8001FCB4;
        }
        c.V0 = c.S2 + 0u;
        goto L8001FD50;
        L8001FCAC: ;
        if (c.V0 != c.S0) {
            c.V0 = c.S2 + 0u;
            goto L8001FD50;
        }
        c.V0 = c.S2 + 0u;
        L8001FCB4: ;
        c.S2 = 0x0000000Du;
        goto L8001FD4C;
        L8001FCBC: ;
        c.A0 = 0x00000001u;
        c.RA = 0x8001FCC4u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.S2 = 0x00000002u;
        goto L8001FD4C;
        L8001FCCC: ;
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x30u));
        c.A0 = c.SP + 0x18u;
        c.RA = 0x8001FCD8u;
        GranTurismo2ArcadePC.func_8006AB78(c, m);
        c.A0 = c.SP + 0x18u;
        c.A1 = 0x800B0000u;
        c.A1 = c.A1 + 0x1298u;
        c.RA = 0x8001FCE8u;
        GranTurismo2ArcadePC.func_8007D990(c, m);
        c.V1 = 0x006F0000u;
        c.V1 = c.V1 | 0x6F6Fu;
        c.V0 = 0x801D0000u;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6CC0u));
        c.A0 = c.SP + 0x18u;
        MemoryAccess.WriteU8(m, (c.A0 + 0x1Cu), (byte)c.S0);
        MemoryAccess.WriteU32(m, (c.A0 + 0x10u), c.S3);
        if (c.V0 != 0u) {
            MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.V1);
            goto L8001FD38;
        }
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.V1);
        c.V0 = 0x00740000u;
        c.V0 = c.V0 | 0x7474u;
        c.A0 = c.S3 + 0u;
        c.A2 = 0x000000B0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V0);
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x24u));
        c.A3 = 0x000000EAu;
        c.RA = 0x8001FD30u;
        GranTurismo2ArcadePC.func_8001F234(c, m);
        c.V0 = c.S2 + 0u;
        goto L8001FD50;
        L8001FD38: ;
        c.A2 = 0x000000B0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x24u));
        c.A3 = 0x000000FAu;
        c.RA = 0x8001FD4Cu;
        GranTurismo2ArcadePC.func_8006ACC4(c, m);
        L8001FD4C: ;
        c.V0 = c.S2 + 0u;
        L8001FD50: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x48u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x44u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x40u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x3Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x38u));
        c.SP = c.SP + 0x50u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001FD6C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1280u));
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x17Cu));
        if (c.A0 == c.S3) {
            c.S2 = 0xFFFFFFFFu;
            goto L8001FE78;
        }
        c.S2 = 0xFFFFFFFFu;
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L8001FEB0;
        }
        c.V0 = c.S2 + 0u;
        if (c.A0 != 0u) {
            c.V1 = c.S1 + 0x34u;
            goto L8001FEB0;
        }
        c.V1 = c.S1 + 0x34u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V1 + 0x18u));
        if ((int)c.V0 < 0) {
            goto L8001FDD0;
        }
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 + 0x4u));
        c.V0 = ~(0u | c.V0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x18u), (ushort)c.V0);
        L8001FDD0: ;
        c.A0 = c.S1 + 0xE8u;
        c.RA = 0x8001FDD8u;
        GranTurismo2ArcadePC.func_8006E298(c, m);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, c.S1);
        if (c.V1 == c.S3) {
            c.V0 = 0x801C0000u;
            goto L8001FE2C;
        }
        c.V0 = 0x801C0000u;
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L8001FE04;
        }
        if (c.V1 == 0u) {
            c.V0 = 0x801C0000u;
            goto L8001FE18;
        }
        c.V0 = 0x801C0000u;
        c.V0 = 0x801F0000u;
        goto L8001FE58;
        L8001FE04: ;
        c.V0 = 0x00000002u;
        if (c.V1 == c.V0) {
            goto L8001FE40;
        }
        c.V0 = 0x801F0000u;
        goto L8001FE58;
        L8001FE18: ;
        c.V0 = c.V0 - 0x65CEu;
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0xEC0u;
        goto L8001FE68;
        L8001FE2C: ;
        c.V0 = c.V0 - 0x65AAu;
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0xEC0u;
        goto L8001FE68;
        L8001FE40: ;
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6540u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0xEC0u;
        goto L8001FE68;
        L8001FE58: ;
        c.V0 = c.V0 - 0xEC0u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x330Cu;
        L8001FE68: ;
        MemoryAccess.WriteU32(m, (c.S1 + 0x2Cu), c.V0);
        c.RA = 0x8001FE70u;
        GranTurismo2ArcadePC.func_8001F370(c, m);
        c.V0 = c.S2 + 0u;
        goto L8001FEB0;
        L8001FE78: ;
        if (c.S0 != c.S2) {
            c.V0 = (int)c.S0 < -1 ? 1u : 0u;
            goto L8001FE88;
        }
        c.V0 = (int)c.S0 < -1 ? 1u : 0u;
        c.S2 = 0x0000000Du;
        goto L8001FEAC;
        L8001FE88: ;
        if (c.V0 != 0u) {
            c.V0 = c.S2 + 0u;
            goto L8001FEB0;
        }
        c.V0 = c.S2 + 0u;
        c.V0 = (int)c.S0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L8001FEB0;
        }
        c.V0 = c.S2 + 0u;
        c.A0 = 0x00000001u;
        c.RA = 0x8001FEA4u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        MemoryAccess.WriteU16(m, (c.S1 + 0x10u), (ushort)c.S0);
        c.S2 = 0x00000003u;
        L8001FEAC: ;
        c.V0 = c.S2 + 0u;
        L8001FEB0: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001FECC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1280u));
        c.A1 = 0x80050000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x10u));
        c.A1 = c.A1 - 0x4440u;
        c.RA = 0x8001FEECu;
        GranTurismo2ArcadePC.func_8007D2F8(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001FEFC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1280u));
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = 0xFFFFFFFFu;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = 0x00000001u;
        if (c.A0 == c.S2) {
            MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
            goto L8001FF58;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S1 + 0u;
            goto L8002001C;
        }
        c.V0 = c.S1 + 0u;
        if (c.A0 != 0u) {
            goto L8002001C;
        }
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0xEA3u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x28u), c.V0);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x330Cu;
        MemoryAccess.WriteU16(m, (c.S0 + 0x4Cu), (ushort)0u);
        MemoryAccess.WriteU32(m, (c.S0 + 0x2Cu), c.V0);
        c.RA = 0x8001FF58u;
        GranTurismo2ArcadePC.func_8001F370(c, m);
        L8001FF58: ;
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x10u));
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0xCu));
        c.V1 = c.A1 << 1;
        c.V0 = c.V0 + c.V1;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, c.V0);
        if (c.V1 == c.S2) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L80020018;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L8001FF90;
        }
        c.V0 = 0x00000002u;
        if (c.V1 == 0u) {
            goto L8001FFC8;
        }
        c.S1 = 0x00000005u;
        goto L80020018;
        L8001FF90: ;
        if (c.V1 == c.V0) {
            c.V0 = 0x00000004u;
            goto L8001FFA8;
        }
        c.V0 = 0x00000004u;
        if (c.V1 == c.V0) {
            c.S1 = 0x00000005u;
            goto L8001FFB0;
        }
        c.S1 = 0x00000005u;
        c.V0 = c.S1 + 0u;
        goto L8002001C;
        L8001FFA8: ;
        c.S1 = 0x00000004u;
        goto L80020018;
        L8001FFB0: ;
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0xE3Du;
        MemoryAccess.WriteU32(m, (c.S0 + 0x24u), c.V0);
        c.RA = 0x8001FFC0u;
        GranTurismo2ArcadePC.func_8001F3C0(c, m);
        c.S1 = 0x00000001u;
        goto L80020018;
        L8001FFC8: ;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, c.S0);
        if ((int)c.V1 < 0) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L80020018;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = 0x00000002u;
            goto L8001FFF0;
        }
        c.V0 = 0x00000002u;
        if (c.V1 == c.V0) {
            c.V0 = c.S1 + 0u;
            goto L80020004;
        }
        c.V0 = c.S1 + 0u;
        goto L8002001C;
        L8001FFF0: ;
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x418u));
        c.S1 = 0x00000006u;
        c.RA = 0x8001FFFCu;
        GranTurismo2ArcadePC.func_80069FE4(c, m);
        goto L8002000C;
        L80020004: ;
        c.S1 = 0x0000000Au;
        c.RA = 0x8002000Cu;
        GranTurismo2ArcadePC.func_8001FECC(c, m);
        L8002000C: ;
        if (c.V0 != 0u) {
            c.V0 = c.S1 + 0u;
            goto L8002001C;
        }
        c.V0 = c.S1 + 0u;
        c.S1 = 0x00000005u;
        L80020018: ;
        c.V0 = c.S1 + 0u;
        L8002001C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020034(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1280u));
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x214u));
        if (c.A0 == c.V0) {
            c.S1 = 0xFFFFFFFFu;
            goto L8002009C;
        }
        c.S1 = 0xFFFFFFFFu;
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S1 + 0u;
            goto L8002010C;
        }
        c.V0 = c.S1 + 0u;
        if (c.A0 != 0u) {
            goto L8002010C;
        }
        c.A0 = c.S0 + 0x180u;
        c.RA = 0x80020078u;
        GranTurismo2ArcadePC.func_8006E298(c, m);
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0xE5Au;
        MemoryAccess.WriteU32(m, (c.S0 + 0x28u), c.V0);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x330Cu;
        MemoryAccess.WriteU32(m, (c.S0 + 0x2Cu), c.V0);
        c.RA = 0x80020094u;
        GranTurismo2ArcadePC.func_8001F370(c, m);
        c.V0 = c.S1 + 0u;
        goto L8002010C;
        L8002009C: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x10u));
        c.V1 = MemoryAccess.ReadU32(m, (c.S0 + 0xCu));
        c.V0 = c.V0 << 1;
        c.V1 = c.V1 + c.V0;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, c.V1);
        c.V0 = 0x00000002u;
        if (c.V1 == c.V0) {
            goto L800200CC;
        }
        c.A0 = c.S0 + 0x180u;
        c.RA = 0x800200C4u;
        GranTurismo2ArcadePC.func_8006E30C(c, m);
        c.S1 = 0x00000003u;
        goto L80020108;
        L800200CC: ;
        if (c.A1 == 0u) {
            goto L800200FC;
        }
        if ((int)c.A1 > 0) {
            goto L800200EC;
        }
        if (c.A1 == c.S1) {
            c.V0 = c.S1 + 0u;
            goto L800200F4;
        }
        c.V0 = c.S1 + 0u;
        goto L8002010C;
        L800200EC: ;
        if (c.A1 != c.A0) {
            c.V0 = c.S1 + 0u;
            goto L8002010C;
        }
        c.V0 = c.S1 + 0u;
        L800200F4: ;
        c.S1 = 0x0000000Du;
        goto L80020108;
        L800200FC: ;
        c.A0 = 0x00000001u;
        c.RA = 0x80020104u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.S1 = 0x00000002u;
        L80020108: ;
        c.V0 = c.S1 + 0u;
        L8002010C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020120(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1280u));
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0xE4u));
        if (c.A0 == c.V0) {
            c.S2 = 0xFFFFFFFFu;
            goto L800201B4;
        }
        c.S2 = 0xFFFFFFFFu;
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L8002022C;
        }
        c.V0 = c.S2 + 0u;
        if (c.A0 != 0u) {
            goto L8002022C;
        }
        c.A0 = 0u + 0u;
        c.RA = 0x80020168u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.S0 = c.S1 + 0x50u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x80020174u;
        GranTurismo2ArcadePC.func_8006E298(c, m);
        MemoryAccess.WriteU8(m, (c.S0 + 0x7Du), (byte)0u);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, c.S1);
        c.V0 = 0x00000002u;
        if (c.V1 != c.V0) {
            c.V0 = 0x801C0000u;
            goto L80020190;
        }
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x650Du;
        goto L80020198;
        L80020190: ;
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0xC31u;
        L80020198: ;
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x330Cu;
        MemoryAccess.WriteU32(m, (c.S1 + 0x2Cu), c.V0);
        c.RA = 0x800201ACu;
        GranTurismo2ArcadePC.func_8001F370(c, m);
        c.V0 = c.S2 + 0u;
        goto L8002022C;
        L800201B4: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x10u));
        c.V1 = MemoryAccess.ReadU32(m, (c.S1 + 0xCu));
        c.V0 = c.V0 << 1;
        c.V1 = c.V1 + c.V0;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, c.V1);
        c.V0 = 0x00000002u;
        if (c.V1 == c.V0) {
            c.V0 = 0x00000004u;
            goto L800201DC;
        }
        c.V0 = 0x00000004u;
        if (c.V1 != c.V0) {
            goto L800201EC;
        }
        L800201DC: ;
        c.A0 = c.S1 + 0x50u;
        c.RA = 0x800201E4u;
        GranTurismo2ArcadePC.func_8006E30C(c, m);
        c.S2 = 0x00000003u;
        goto L80020228;
        L800201EC: ;
        if (c.A1 == 0u) {
            goto L8002021C;
        }
        if ((int)c.A1 > 0) {
            goto L8002020C;
        }
        if (c.A1 == c.S2) {
            c.V0 = c.S2 + 0u;
            goto L80020214;
        }
        c.V0 = c.S2 + 0u;
        goto L8002022C;
        L8002020C: ;
        if (c.A1 != c.A0) {
            c.V0 = c.S2 + 0u;
            goto L8002022C;
        }
        c.V0 = c.S2 + 0u;
        L80020214: ;
        c.S2 = 0x0000000Du;
        goto L80020228;
        L8002021C: ;
        c.A0 = 0x00000001u;
        c.RA = 0x80020224u;
        GranTurismo2ArcadePC.func_80060750(c, m);
        c.S2 = 0x00000002u;
        L80020228: ;
        c.V0 = c.S2 + 0u;
        L8002022C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020244(CpuContext c, IMemory m)
    {
        c.V0 = 0xFFFFFFFFu;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8002024C(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1280u));
        c.SP = c.SP - 0x18u;
        if ((int)c.A0 >= 0) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
            goto L8002026C;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x244u;
        goto L80020284;
        L8002026C: ;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 - 0x42B4u;
        c.V1 = c.A0 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        L80020284: ;
        MemoryAccess.WriteU32(m, (c.A1 + 0x4u), c.V0);
        MemoryAccess.WriteU16(m, (c.A1 + 0x2u), (ushort)c.A0);
        c.A0 = 0u + 0u;
        c.V0 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.A1 = c.A0 + 0u;
        c.RA = 0x800202A0u;
        Dispatcher.Call(c, m, c.V0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800202B0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x4414u;
        c.V0 = 0x800B0000u;
        c.A2 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1280u));
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.A0 = c.S1 + 0x50u;
        c.T2 = c.V0 - 0x4430u;
        c.A3 = MemoryAccess.ReadU32(m, c.T2);
        c.T0 = MemoryAccess.ReadU32(m, (c.T2 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.T2 + 0x8u));
        MemoryAccess.WriteU32(m, (c.S1 + 0x34u), c.A3);
        MemoryAccess.WriteU32(m, (c.S1 + 0x38u), c.T0);
        MemoryAccess.WriteU32(m, (c.S1 + 0x3Cu), c.T1);
        c.A3 = MemoryAccess.ReadU32(m, (c.T2 + 0xCu));
        c.T0 = MemoryAccess.ReadU32(m, (c.T2 + 0x10u));
        c.T1 = MemoryAccess.ReadU32(m, (c.T2 + 0x14u));
        MemoryAccess.WriteU32(m, (c.S1 + 0x40u), c.A3);
        MemoryAccess.WriteU32(m, (c.S1 + 0x44u), c.T0);
        MemoryAccess.WriteU32(m, (c.S1 + 0x48u), c.T1);
        c.A3 = MemoryAccess.ReadU32(m, (c.T2 + 0x18u));
        MemoryAccess.WriteU32(m, (c.S1 + 0x4Cu), c.A3);
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU16(m, (c.S1 + 0x4Cu), (ushort)c.V0);
        c.RA = 0x80020328u;
        GranTurismo2ArcadePC.func_8006E0DC(c, m);
        c.S0 = c.S1 + 0xE8u;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x43E4u;
        c.A2 = 0u + 0u;
        c.RA = 0x80020340u;
        GranTurismo2ArcadePC.func_8006E0DC(c, m);
        c.A0 = c.S1 + 0x180u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x43B4u;
        c.A2 = 0u + 0u;
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x7Du), (byte)c.V0);
        c.RA = 0x8002035Cu;
        GranTurismo2ArcadePC.func_8006E0DC(c, m);
        c.A0 = c.S1 + 0x218u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x4384u;
        c.A2 = 0u + 0u;
        c.RA = 0x80020370u;
        GranTurismo2ArcadePC.func_8006E0DC(c, m);
        c.A0 = c.S1 + 0x348u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x4354u;
        c.A2 = 0u + 0u;
        c.RA = 0x80020384u;
        GranTurismo2ArcadePC.func_8006E0DC(c, m);
        c.V1 = c.S1 + 0x3E0u;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 - 0x42F4u;
        c.A0 = c.V0 + 0x30u;
        L80020394: ;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V0 + 0xCu));
        MemoryAccess.WriteU32(m, c.V1, c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x4u), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x8u), c.T1);
        MemoryAccess.WriteU32(m, (c.V1 + 0xCu), c.T2);
        c.V0 = c.V0 + 0x10u;
        if (c.V0 != c.A0) {
            c.V1 = c.V1 + 0x10u;
            goto L80020394;
        }
        c.V1 = c.V1 + 0x10u;
        c.A0 = c.S1 + 0x2B0u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x4324u;
        c.A2 = 0u + 0u;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        MemoryAccess.WriteU32(m, c.V1, c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x4u), c.T0);
        c.RA = 0x800203E4u;
        GranTurismo2ArcadePC.func_8006E0DC(c, m);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x330Cu;
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        MemoryAccess.WriteU32(m, (c.S1 + 0x2Cu), c.V0);
        c.RA = 0x800203F8u;
        GranTurismo2ArcadePC.func_8001F370(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8002040C(CpuContext c, IMemory m)
    {
        c.V0 = MemoryAccess.ReadU32(m, c.A0);
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0xCu));
        c.V1 = 0x800B0000u;
        c.T1 = c.V1 + 0x12A8u;
        c.A2 = MemoryAccess.ReadU32(m, c.A1);
        c.A3 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.T0 = MemoryAccess.ReadU32(m, (c.A1 + 0x8u));
        MemoryAccess.WriteU32(m, c.T1, c.A2);
        MemoryAccess.WriteU32(m, (c.T1 + 0x4u), c.A3);
        MemoryAccess.WriteU32(m, (c.T1 + 0x8u), c.T0);
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0x10u));
        c.V1 = 0x800B0000u;
        c.T1 = c.V1 + 0x1288u;
        c.A2 = MemoryAccess.ReadU32(m, c.A1);
        c.A3 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.T0 = MemoryAccess.ReadU32(m, (c.A1 + 0x8u));
        MemoryAccess.WriteU32(m, c.T1, c.A2);
        MemoryAccess.WriteU32(m, (c.T1 + 0x4u), c.A3);
        MemoryAccess.WriteU32(m, (c.T1 + 0x8u), c.T0);
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0x14u));
        c.V1 = 0x800B0000u;
        c.T1 = c.V1 + 0x1298u;
        c.A2 = MemoryAccess.ReadU32(m, c.A1);
        c.A3 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.T0 = MemoryAccess.ReadU32(m, (c.A1 + 0x8u));
        MemoryAccess.WriteU32(m, c.T1, c.A2);
        MemoryAccess.WriteU32(m, (c.T1 + 0x4u), c.A3);
        MemoryAccess.WriteU32(m, (c.T1 + 0x8u), c.T0);
        c.V1 = MemoryAccess.ReadU32(m, (c.A0 + 0x4u));
        c.A1 = 0x00010000u;
        MemoryAccess.WriteU32(m, (c.V0 + 0x8u), c.V1);
        c.V1 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.A1 = c.A1 | 0x041Cu;
        MemoryAccess.WriteU32(m, (c.V0 + 0xCu), c.V1);
        c.A0 = MemoryAccess.ReadU16(m, (c.A0 + 0x18u));
        c.V1 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x1280u), c.V0);
        MemoryAccess.WriteU16(m, (c.V0 + 0x30u), (ushort)c.A0);
        c.V0 = c.V0 + c.A1;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800204AC(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1280u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU16(m, c.V0, (ushort)c.A0);
        c.A0 = c.V0 + 0x41Cu;
        MemoryAccess.WriteU32(m, (c.V0 + 0x418u), c.A0);
        c.RA = 0x800204CCu;
        GranTurismo2ArcadePC.func_80069F48(c, m);
        c.RA = 0x800204D4u;
        GranTurismo2ArcadePC.func_800202B0(c, m);
        c.A0 = 0u + 0u;
        c.RA = 0x800204DCu;
        GranTurismo2ArcadePC.func_8002024C(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800204EC_gt2_arcade_overlay_1(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1280u));
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        c.A0 = c.S1 + 0x34u;
        c.RA = 0x80020518u;
        GranTurismo2ArcadePC.func_8006BD74(c, m);
        c.A0 = c.S1 + 0x50u;
        c.A1 = c.S0 + 0u;
        c.RA = 0x80020524u;
        GranTurismo2ArcadePC.func_8006E34C(c, m);
        c.A0 = c.S1 + 0xE8u;
        c.A1 = c.S0 + 0u;
        MemoryAccess.WriteU16(m, (c.S1 + 0xE4u), (ushort)c.V0);
        c.RA = 0x80020534u;
        GranTurismo2ArcadePC.func_8006E34C(c, m);
        c.A0 = c.S1 + 0x180u;
        c.A1 = c.S0 + 0u;
        MemoryAccess.WriteU16(m, (c.S1 + 0x17Cu), (ushort)c.V0);
        c.RA = 0x80020544u;
        GranTurismo2ArcadePC.func_8006E34C(c, m);
        c.A0 = c.S1 + 0x218u;
        c.A1 = c.S0 + 0u;
        MemoryAccess.WriteU16(m, (c.S1 + 0x214u), (ushort)c.V0);
        c.RA = 0x80020554u;
        GranTurismo2ArcadePC.func_8006E34C(c, m);
        c.A0 = c.S1 + 0x348u;
        c.A1 = c.S0 + 0u;
        MemoryAccess.WriteU16(m, (c.S1 + 0x2ACu), (ushort)c.V0);
        c.RA = 0x80020564u;
        GranTurismo2ArcadePC.func_8006E34C(c, m);
        c.A0 = c.S1 + 0x2B0u;
        c.A1 = c.S0 + 0u;
        MemoryAccess.WriteU16(m, (c.S1 + 0x3DCu), (ushort)c.V0);
        c.RA = 0x80020574u;
        GranTurismo2ArcadePC.func_8006E34C(c, m);
        c.A0 = 0x00000001u;
        c.V1 = MemoryAccess.ReadU32(m, (c.S1 + 0x4u));
        c.A1 = c.S2 + 0u;
        MemoryAccess.WriteU16(m, (c.S1 + 0x344u), (ushort)c.V0);
        c.RA = 0x80020588u;
        Dispatcher.Call(c, m, c.V1);
        c.A0 = c.V0 + 0u;
        L8002058C: ;
        c.V0 = 0x00000008u;
        if (c.A0 == c.V0) {
            c.V0 = (int)c.A0 < 9 ? 1u : 0u;
            goto L800205C4;
        }
        c.V0 = (int)c.A0 < 9 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0xFFFFFFFFu;
            goto L800205B0;
        }
        c.V0 = 0xFFFFFFFFu;
        if (c.A0 == c.V0) {
            c.V0 = c.S2 + 0u;
            goto L800206A8;
        }
        c.V0 = c.S2 + 0u;
        goto L80020694;
        L800205B0: ;
        c.V0 = 0x0000000Du;
        if (c.A0 == c.V0) {
            c.V1 = c.S1 + 0x34u;
            goto L80020650;
        }
        c.V1 = c.S1 + 0x34u;
        goto L80020694;
        L800205C4: ;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, c.S1);
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L8002063C;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L800205EC;
        }
        c.V0 = 0x00000002u;
        if (c.V1 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L800205FC;
        }
        c.V0 = c.S2 + 0u;
        goto L800206A8;
        L800205EC: ;
        if (c.V1 == c.V0) {
            c.V0 = c.S2 + 0u;
            goto L8002063C;
        }
        c.V0 = c.S2 + 0u;
        goto L800206A8;
        L800205FC: ;
        c.V1 = c.S1 + 0x34u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V1 + 0x18u));
        if ((int)c.V0 < 0) {
            c.V0 = 0x80020000u;
            goto L80020624;
        }
        c.V0 = 0x80020000u;
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 + 0x4u));
        c.V0 = ~(0u | c.V0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x18u), (ushort)c.V0);
        c.V0 = 0x80020000u;
        L80020624: ;
        c.V0 = c.V0 + 0x330Cu;
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        MemoryAccess.WriteU32(m, (c.S1 + 0x2Cu), c.V0);
        c.RA = 0x80020634u;
        GranTurismo2ArcadePC.func_8001F370(c, m);
        c.A0 = 0xFFFFFFFFu;
        goto L80020640;
        L8002063C: ;
        c.A0 = 0x00000008u;
        L80020640: ;
        c.S2 = 0x00000002u;
        c.RA = 0x80020648u;
        GranTurismo2ArcadePC.func_8002024C(c, m);
        c.V0 = c.S2 + 0u;
        goto L800206A8;
        L80020650: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V1 + 0x18u));
        if ((int)c.V0 < 0) {
            c.V0 = 0x80020000u;
            goto L80020674;
        }
        c.V0 = 0x80020000u;
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 + 0x4u));
        c.V0 = ~(0u | c.V0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x18u), (ushort)c.V0);
        c.V0 = 0x80020000u;
        L80020674: ;
        c.V0 = c.V0 + 0x330Cu;
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        MemoryAccess.WriteU32(m, (c.S1 + 0x2Cu), c.V0);
        c.RA = 0x80020684u;
        GranTurismo2ArcadePC.func_8001F370(c, m);
        c.A0 = 0xFFFFFFFFu;
        c.RA = 0x8002068Cu;
        GranTurismo2ArcadePC.func_8002024C(c, m);
        c.S2 = 0x00000001u;
        goto L800206A4;
        L80020694: ;
        c.RA = 0x8002069Cu;
        GranTurismo2ArcadePC.func_8002024C(c, m);
        c.A0 = c.V0 + 0u;
        goto L8002058C;
        L800206A4: ;
        c.V0 = c.S2 + 0u;
        L800206A8: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800206C0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x58u;
        MemoryAccess.WriteU32(m, (c.SP + 0x4Cu), c.S3);
        c.S3 = c.A0 + 0u;
        c.V1 = 0x02420000u;
        c.V1 = c.V1 | 0x362Au;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1280u));
        c.A0 = c.SP + 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x50u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x48u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.S0);
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x30u));
        c.V0 = 0x02000000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x38u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x3Cu), c.V1);
        c.RA = 0x80020704u;
        GranTurismo2ArcadePC.func_8006AB78(c, m);
        c.A0 = c.SP + 0x18u;
        c.A1 = 0x800B0000u;
        c.A1 = c.A1 + 0x1298u;
        c.RA = 0x80020714u;
        GranTurismo2ArcadePC.func_8007D990(c, m);
        c.A1 = 0xFF9F0000u;
        c.A1 = c.A1 | 0xFFFFu;
        c.A0 = 0x025C0000u;
        c.A0 = c.A0 | 0x5248u;
        c.S0 = c.SP + 0x18u;
        c.V1 = MemoryAccess.ReadU32(m, (c.S0 + 0xCu));
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x1Cu), (byte)c.V0);
        c.V0 = 0x00200000u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x10u), c.S3);
        MemoryAccess.WriteU32(m, (c.S0 + 0x14u), c.A0);
        c.V1 = c.V1 & c.A1;
        c.V1 = c.V1 | c.V0;
        c.V0 = 0x801D0000u;
        MemoryAccess.WriteU32(m, (c.S0 + 0xCu), c.V1);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6CC0u));
        if (c.V0 != 0u) {
            c.S2 = 0x00000001u;
            goto L800207A8;
        }
        c.S2 = 0x00000001u;
        c.S0 = 0x00740000u;
        c.S0 = c.S0 | 0x7474u;
        c.A0 = c.S3 + 0u;
        c.A2 = 0x000000B0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S0);
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x28u));
        c.A3 = 0x000000C6u;
        c.RA = 0x80020784u;
        GranTurismo2ArcadePC.func_8001F234(c, m);
        c.A0 = c.S3 + 0u;
        c.A2 = 0x000000B0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S0);
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x2Cu));
        c.A3 = 0x000000EAu;
        c.RA = 0x800207A0u;
        GranTurismo2ArcadePC.func_8001F234(c, m);
        goto L800207D8;
        L800207A8: ;
        c.A0 = c.S0 + 0u;
        c.A2 = 0x000000B0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S2);
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x28u));
        c.A3 = 0x000000D6u;
        c.RA = 0x800207C0u;
        GranTurismo2ArcadePC.func_8006ACC4(c, m);
        c.A0 = c.S0 + 0u;
        c.A2 = 0x000000B0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S2);
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x2Cu));
        c.A3 = 0x000000FAu;
        c.RA = 0x800207D8u;
        GranTurismo2ArcadePC.func_8006ACC4(c, m);
        L800207D8: ;
        c.A0 = c.S1 + 0x34u;
        c.RA = 0x800207E0u;
        GranTurismo2ArcadePC.func_8006BDC4(c, m);
        c.V1 = c.V0 + 0u;
        if (c.V1 == 0u) {
            c.V0 = 0x801D0000u;
            goto L800208A8;
        }
        c.V0 = 0x801D0000u;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6CC0u));
        if (c.V0 != 0u) {
            c.A0 = c.SP + 0x38u;
            goto L80020844;
        }
        c.A0 = c.SP + 0x38u;
        c.A0 = c.S3 + 0u;
        c.A2 = 0x0000001Cu;
        c.V0 = c.V1 << 8;
        c.V0 = c.V1 | c.V0;
        c.V1 = c.V1 << 16;
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x10u));
        c.V0 = c.V0 | c.V1;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V0);
        c.V0 = 0x80050000u;
        c.V0 = c.V0 - 0x42BCu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), 0u);
        c.A1 = c.A1 << 2;
        c.A1 = c.A1 + c.V0;
        c.A1 = MemoryAccess.ReadU32(m, c.A1);
        c.A3 = 0x00000066u;
        c.RA = 0x8002083Cu;
        GranTurismo2ArcadePC.func_8001F234(c, m);
        c.A0 = c.S1 + 0x34u;
        goto L8002088C;
        L80020844: ;
        c.A1 = c.SP + 0x3Cu;
        c.A2 = c.V1 + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x80020854u;
        GranTurismo2ArcadePC.func_8006B458(c, m);
        c.A0 = c.SP + 0x18u;
        c.A2 = 0x0000001Cu;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.V0);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x10u));
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.V0 = 0x80050000u;
        c.V0 = c.V0 - 0x42BCu;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.A1 = MemoryAccess.ReadU32(m, c.V1);
        c.A3 = 0x0000007Eu;
        c.RA = 0x80020888u;
        GranTurismo2ArcadePC.func_8006ABA0(c, m);
        c.A0 = c.S1 + 0x34u;
        L8002088C: ;
        c.A1 = c.S3 + 0u;
        c.A2 = 0x00000018u;
        c.A3 = 0x0000006Eu;
        c.RA = 0x8002089Cu;
        GranTurismo2ArcadePC.func_8006BE04(c, m);
        c.A0 = c.S3 + 0u;
        c.A1 = 0x00000220u;
        c.RA = 0x800208A8u;
        GranTurismo2ArcadePC.func_8007D954(c, m);
        L800208A8: ;
        c.A0 = c.S1 + 0x50u;
        c.A1 = c.S3 + 0u;
        c.A2 = c.SP + 0x18u;
        c.RA = 0x800208B8u;
        GranTurismo2ArcadePC.func_8006E4C8(c, m);
        c.A0 = c.S1 + 0xE8u;
        c.A1 = c.S3 + 0u;
        c.A2 = c.SP + 0x18u;
        c.RA = 0x800208C8u;
        GranTurismo2ArcadePC.func_8006E4C8(c, m);
        c.A0 = c.S1 + 0x180u;
        c.A1 = c.S3 + 0u;
        c.A2 = c.SP + 0x18u;
        c.RA = 0x800208D8u;
        GranTurismo2ArcadePC.func_8006E4C8(c, m);
        c.A0 = c.S1 + 0x218u;
        c.A1 = c.S3 + 0u;
        c.A2 = c.SP + 0x18u;
        c.RA = 0x800208E8u;
        GranTurismo2ArcadePC.func_8006E4C8(c, m);
        c.A0 = c.S1 + 0x348u;
        c.A1 = c.S3 + 0u;
        c.A2 = c.SP + 0x18u;
        c.RA = 0x800208F8u;
        GranTurismo2ArcadePC.func_8006E4C8(c, m);
        c.A0 = c.S1 + 0x2B0u;
        c.A1 = c.S3 + 0u;
        c.A2 = c.SP + 0x18u;
        c.RA = 0x80020908u;
        GranTurismo2ArcadePC.func_8006E4C8(c, m);
        c.A0 = 0x00000002u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x4u));
        c.A1 = c.S3 + 0u;
        c.RA = 0x8002091Cu;
        Dispatcher.Call(c, m, c.V0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x50u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x4Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x48u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x44u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x40u));
        c.SP = c.SP + 0x58u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020938(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1280u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x418u));
        c.A1 = c.A0 + 0u;
        c.RA = 0x80020954u;
        GranTurismo2ArcadePC.func_8006A1F4(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020964(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1280u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x418u));
        c.A1 = c.A0 + 0u;
        c.RA = 0x80020980u;
        GranTurismo2ArcadePC.func_8006A1FC(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020990(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1280u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x418u));
        c.A1 = c.A0 + 0u;
        c.RA = 0x800209ACu;
        GranTurismo2ArcadePC.func_8006A204(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800209BC(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1280u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x418u));
        c.A1 = c.A0 + 0u;
        c.RA = 0x800209D8u;
        GranTurismo2ArcadePC.func_8006A20C(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800209E8(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1280u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x418u));
        c.A1 = c.A0 + 0u;
        c.RA = 0x80020A04u;
        GranTurismo2ArcadePC.func_8006A214(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020A14(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1280u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x418u));
        c.A1 = c.A0 + 0u;
        c.RA = 0x80020A30u;
        GranTurismo2ArcadePC.func_8006A21C(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020A40(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1280u));
        c.V0 = c.V0 + 0x61Cu;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020A50(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        c.V0 = 0x801D0000u;
        c.V1 = 0x80050000u;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6CC0u));
        c.V1 = c.V1 - 0x4284u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.V1;
        c.A0 = MemoryAccess.ReadU32(m, c.V0);
        c.A1 = c.S0 + 0u;
        c.RA = 0x80020A84u;
        GranTurismo2ArcadePC.func_8005D844(c, m);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x200u));
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020A98(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.A3 = c.A1 + 0u;
        c.T3 = c.A0 + 0x204u;
        c.T2 = c.A0 + 0x1580u;
        c.T0 = 0u + 0u;
        c.V0 = c.A2 << 1;
        c.V0 = c.V0 + c.A2;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 - c.A2;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + 0x988u;
        c.A0 = c.A0 + c.V0;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.T1 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0x52u));
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0x50u));
        L80020AD4: ;
        c.V0 = (int)c.T0 < (int)c.T1 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.A2 << 7;
            goto L80020B94;
        }
        c.V0 = c.A2 << 7;
        c.V1 = c.V0 + c.T2;
        c.V0 = c.V1 | c.A3;
        c.V0 = c.V0 & 0x0003u;
        if (c.V0 == 0u) {
            c.A0 = c.A3 + 0u;
            goto L80020B4C;
        }
        c.A0 = c.A3 + 0u;
        c.V0 = c.V1 + 0x80u;
        L80020AF8: ;
        c.T4 = MemoryAccess.ReadWordLeft(m, c.T4, (c.V1 + 0x3u));
        c.T4 = MemoryAccess.ReadWordRight(m, c.T4, c.V1);
        c.T5 = MemoryAccess.ReadWordLeft(m, c.T5, (c.V1 + 0x7u));
        c.T5 = MemoryAccess.ReadWordRight(m, c.T5, (c.V1 + 0x4u));
        c.T6 = MemoryAccess.ReadWordLeft(m, c.T6, (c.V1 + 0xBu));
        c.T6 = MemoryAccess.ReadWordRight(m, c.T6, (c.V1 + 0x8u));
        c.T7 = MemoryAccess.ReadWordLeft(m, c.T7, (c.V1 + 0xFu));
        c.T7 = MemoryAccess.ReadWordRight(m, c.T7, (c.V1 + 0xCu));
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0x3u), c.T4);
        MemoryAccess.WriteWordRight(m, c.A0, c.T4);
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0x7u), c.T5);
        MemoryAccess.WriteWordRight(m, (c.A0 + 0x4u), c.T5);
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0xBu), c.T6);
        MemoryAccess.WriteWordRight(m, (c.A0 + 0x8u), c.T6);
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0xFu), c.T7);
        MemoryAccess.WriteWordRight(m, (c.A0 + 0xCu), c.T7);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.V0) {
            c.A0 = c.A0 + 0x10u;
            goto L80020AF8;
        }
        c.A0 = c.A0 + 0x10u;
        c.A3 = c.A3 + 0x80u;
        goto L80020B80;
        L80020B4C: ;
        c.V0 = c.V1 + 0x80u;
        L80020B50: ;
        c.T4 = MemoryAccess.ReadU32(m, c.V1);
        c.T5 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T6 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T7 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, c.A0, c.T4);
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.T5);
        MemoryAccess.WriteU32(m, (c.A0 + 0x8u), c.T6);
        MemoryAccess.WriteU32(m, (c.A0 + 0xCu), c.T7);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.V0) {
            c.A0 = c.A0 + 0x10u;
            goto L80020B50;
        }
        c.A0 = c.A0 + 0x10u;
        c.A3 = c.A3 + 0x80u;
        L80020B80: ;
        c.V0 = c.A2 << 1;
        c.V0 = c.T3 + c.V0;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x2u));
        c.T0 = c.T0 + 0x1u;
        goto L80020AD4;
        L80020B94: ;
        c.A0 = c.A1 + 0u;
        c.A1 = 0u + 0u;
        c.RA = 0x80020BA0u;
        GranTurismo2ArcadePC.func_800699D4(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8004B2B4(CpuContext c, IMemory m)
    {
        Dispatcher.Call(c, m, 0u);
        return;
    }
}

public sealed class Gt2_arcade_overlay_1DispatchTable : IOverlay
{
    public string Name => "gt2_arcade_overlay_1";
    public int LbaStart => 401;
    public uint Base => 0x80010000u;
    public uint Size => 0x3BD98u;
    public uint ImageSize => 0xA4102Au;
    public bool Relocatable => false;
    public IReadOnlyDictionary<uint, Action<CpuContext, IMemory>> Functions { get; } =
        new Dictionary<uint, Action<CpuContext, IMemory>>
        {
            [0x80010000u] = GranTurismo2ArcadePC.func_80010000_gt2_arcade_overlay_1,
            [0x8001003Cu] = GranTurismo2ArcadePC.func_8001003C_gt2_arcade_overlay_1,
            [0x80010078u] = GranTurismo2ArcadePC.func_80010078_gt2_arcade_overlay_1,
            [0x800101C4u] = GranTurismo2ArcadePC.func_800101C4,
            [0x80010434u] = GranTurismo2ArcadePC.func_80010434,
            [0x800104E0u] = GranTurismo2ArcadePC.func_800104E0,
            [0x800109C0u] = GranTurismo2ArcadePC.func_800109C0,
            [0x80010B08u] = GranTurismo2ArcadePC.func_80010B08,
            [0x80010C50u] = GranTurismo2ArcadePC.func_80010C50,
            [0x80010DC4u] = GranTurismo2ArcadePC.func_80010DC4,
            [0x80010E6Cu] = GranTurismo2ArcadePC.func_80010E6C,
            [0x80010EDCu] = GranTurismo2ArcadePC.func_80010EDC,
            [0x800110C4u] = GranTurismo2ArcadePC.func_800110C4,
            [0x800110FCu] = GranTurismo2ArcadePC.func_800110FC,
            [0x80011178u] = GranTurismo2ArcadePC.func_80011178,
            [0x800111DCu] = GranTurismo2ArcadePC.func_800111DC,
            [0x800112D0u] = GranTurismo2ArcadePC.func_800112D0,
            [0x80011720u] = GranTurismo2ArcadePC.func_80011720,
            [0x800117D4u] = GranTurismo2ArcadePC.func_800117D4,
            [0x8001180Cu] = GranTurismo2ArcadePC.func_8001180C,
            [0x80011888u] = GranTurismo2ArcadePC.func_80011888,
            [0x80011AC8u] = GranTurismo2ArcadePC.func_80011AC8,
            [0x80011AFCu] = GranTurismo2ArcadePC.func_80011AFC_gt2_arcade_overlay_1,
            [0x80011B24u] = GranTurismo2ArcadePC.func_80011B24,
            [0x80011B30u] = GranTurismo2ArcadePC.func_80011B30,
            [0x80011E30u] = GranTurismo2ArcadePC.func_80011E30,
            [0x80012040u] = GranTurismo2ArcadePC.func_80012040,
            [0x80012114u] = GranTurismo2ArcadePC.func_80012114,
            [0x8001220Cu] = GranTurismo2ArcadePC.func_8001220C,
            [0x80012240u] = GranTurismo2ArcadePC.func_80012240,
            [0x80012280u] = GranTurismo2ArcadePC.func_80012280,
            [0x800122B4u] = GranTurismo2ArcadePC.func_800122B4,
            [0x80012310u] = GranTurismo2ArcadePC.func_80012310,
            [0x8001233Cu] = GranTurismo2ArcadePC.func_8001233C,
            [0x80012380u] = GranTurismo2ArcadePC.func_80012380,
            [0x800123A0u] = GranTurismo2ArcadePC.func_800123A0,
            [0x800123C0u] = GranTurismo2ArcadePC.func_800123C0,
            [0x80012410u] = GranTurismo2ArcadePC.func_80012410_gt2_arcade_overlay_1,
            [0x8001245Cu] = GranTurismo2ArcadePC.func_8001245C,
            [0x800124C0u] = GranTurismo2ArcadePC.func_800124C0,
            [0x800124C8u] = GranTurismo2ArcadePC.func_800124C8,
            [0x800124F8u] = GranTurismo2ArcadePC.func_800124F8,
            [0x80012534u] = GranTurismo2ArcadePC.func_80012534,
            [0x8001253Cu] = GranTurismo2ArcadePC.func_8001253C,
            [0x80012628u] = GranTurismo2ArcadePC.func_80012628,
            [0x8001269Cu] = GranTurismo2ArcadePC.func_8001269C,
            [0x800127DCu] = GranTurismo2ArcadePC.func_800127DC,
            [0x80012808u] = GranTurismo2ArcadePC.func_80012808,
            [0x80012864u] = GranTurismo2ArcadePC.func_80012864,
            [0x8001290Cu] = GranTurismo2ArcadePC.func_8001290C_gt2_arcade_overlay_1,
            [0x80012930u] = GranTurismo2ArcadePC.func_80012930,
            [0x8001298Cu] = GranTurismo2ArcadePC.func_8001298C,
            [0x800129ECu] = GranTurismo2ArcadePC.func_800129EC,
            [0x80012A10u] = GranTurismo2ArcadePC.func_80012A10,
            [0x80012A6Cu] = GranTurismo2ArcadePC.func_80012A6C,
            [0x80012ACCu] = GranTurismo2ArcadePC.func_80012ACC,
            [0x80012AF0u] = GranTurismo2ArcadePC.func_80012AF0,
            [0x80012C50u] = GranTurismo2ArcadePC.func_80012C50,
            [0x80012CA0u] = GranTurismo2ArcadePC.func_80012CA0,
            [0x80012DE0u] = GranTurismo2ArcadePC.func_80012DE0,
            [0x80012E0Cu] = GranTurismo2ArcadePC.func_80012E0C,
            [0x80012E30u] = GranTurismo2ArcadePC.func_80012E30,
            [0x80012E94u] = GranTurismo2ArcadePC.func_80012E94,
            [0x80012E9Cu] = GranTurismo2ArcadePC.func_80012E9C,
            [0x80012ED4u] = GranTurismo2ArcadePC.func_80012ED4,
            [0x80012F38u] = GranTurismo2ArcadePC.func_80012F38,
            [0x80012F40u] = GranTurismo2ArcadePC.func_80012F40,
            [0x80012F98u] = GranTurismo2ArcadePC.func_80012F98,
            [0x80012FF8u] = GranTurismo2ArcadePC.func_80012FF8,
            [0x8001301Cu] = GranTurismo2ArcadePC.func_8001301C,
            [0x80013054u] = GranTurismo2ArcadePC.func_80013054,
            [0x800130B8u] = GranTurismo2ArcadePC.func_800130B8,
            [0x800130C0u] = GranTurismo2ArcadePC.func_800130C0,
            [0x80013118u] = GranTurismo2ArcadePC.func_80013118,
            [0x80013178u] = GranTurismo2ArcadePC.func_80013178,
            [0x8001319Cu] = GranTurismo2ArcadePC.func_8001319C,
            [0x80013238u] = GranTurismo2ArcadePC.func_80013238,
            [0x80013278u] = GranTurismo2ArcadePC.func_80013278,
            [0x8001339Cu] = GranTurismo2ArcadePC.func_8001339C,
            [0x800133ECu] = GranTurismo2ArcadePC.func_800133EC,
            [0x80013430u] = GranTurismo2ArcadePC.func_80013430,
            [0x80013484u] = GranTurismo2ArcadePC.func_80013484,
            [0x800134C4u] = GranTurismo2ArcadePC.func_800134C4,
            [0x800134F8u] = GranTurismo2ArcadePC.func_800134F8,
            [0x80013534u] = GranTurismo2ArcadePC.func_80013534,
            [0x8001354Cu] = GranTurismo2ArcadePC.func_8001354C,
            [0x80013600u] = GranTurismo2ArcadePC.func_80013600,
            [0x80013630u] = GranTurismo2ArcadePC.func_80013630,
            [0x80013658u] = GranTurismo2ArcadePC.func_80013658_gt2_arcade_overlay_1,
            [0x80013694u] = GranTurismo2ArcadePC.func_80013694,
            [0x8001392Cu] = GranTurismo2ArcadePC.func_8001392C,
            [0x80013A10u] = GranTurismo2ArcadePC.func_80013A10,
            [0x80013D80u] = GranTurismo2ArcadePC.func_80013D80,
            [0x80013E0Cu] = GranTurismo2ArcadePC.func_80013E0C,
            [0x80013F90u] = GranTurismo2ArcadePC.func_80013F90,
            [0x80013FE8u] = GranTurismo2ArcadePC.func_80013FE8,
            [0x80014178u] = GranTurismo2ArcadePC.func_80014178,
            [0x80014390u] = GranTurismo2ArcadePC.func_80014390,
            [0x80014490u] = GranTurismo2ArcadePC.func_80014490,
            [0x80014554u] = GranTurismo2ArcadePC.func_80014554,
            [0x80014670u] = GranTurismo2ArcadePC.func_80014670,
            [0x8001476Cu] = GranTurismo2ArcadePC.func_8001476C,
            [0x80014834u] = GranTurismo2ArcadePC.func_80014834,
            [0x8001496Cu] = GranTurismo2ArcadePC.func_8001496C,
            [0x80014B04u] = GranTurismo2ArcadePC.func_80014B04,
            [0x80014CCCu] = GranTurismo2ArcadePC.func_80014CCC,
            [0x800150B8u] = GranTurismo2ArcadePC.func_800150B8,
            [0x80015264u] = GranTurismo2ArcadePC.func_80015264,
            [0x80015434u] = GranTurismo2ArcadePC.func_80015434,
            [0x8001549Cu] = GranTurismo2ArcadePC.func_8001549C,
            [0x8001550Cu] = GranTurismo2ArcadePC.func_8001550C,
            [0x800156E4u] = GranTurismo2ArcadePC.func_800156E4,
            [0x80015768u] = GranTurismo2ArcadePC.func_80015768,
            [0x8001589Cu] = GranTurismo2ArcadePC.func_8001589C,
            [0x80015990u] = GranTurismo2ArcadePC.func_80015990,
            [0x80015A60u] = GranTurismo2ArcadePC.func_80015A60,
            [0x80015A68u] = GranTurismo2ArcadePC.func_80015A68,
            [0x80015ACCu] = GranTurismo2ArcadePC.func_80015ACC,
            [0x80015C58u] = GranTurismo2ArcadePC.func_80015C58,
            [0x80015CF8u] = GranTurismo2ArcadePC.func_80015CF8,
            [0x80015D20u] = GranTurismo2ArcadePC.func_80015D20,
            [0x80015EACu] = GranTurismo2ArcadePC.func_80015EAC,
            [0x800161B4u] = GranTurismo2ArcadePC.func_800161B4,
            [0x800162ECu] = GranTurismo2ArcadePC.func_800162EC,
            [0x80016320u] = GranTurismo2ArcadePC.func_80016320,
            [0x80016368u] = GranTurismo2ArcadePC.func_80016368,
            [0x80016908u] = GranTurismo2ArcadePC.func_80016908,
            [0x80016968u] = GranTurismo2ArcadePC.func_80016968,
            [0x800169A0u] = GranTurismo2ArcadePC.func_800169A0,
            [0x800169ECu] = GranTurismo2ArcadePC.func_800169EC,
            [0x80016B30u] = GranTurismo2ArcadePC.func_80016B30,
            [0x80016B80u] = GranTurismo2ArcadePC.func_80016B80,
            [0x80016EC4u] = GranTurismo2ArcadePC.func_80016EC4,
            [0x80017174u] = GranTurismo2ArcadePC.func_80017174,
            [0x80017284u] = GranTurismo2ArcadePC.func_80017284,
            [0x800172C4u] = GranTurismo2ArcadePC.func_800172C4,
            [0x80017364u] = GranTurismo2ArcadePC.func_80017364,
            [0x8001738Cu] = GranTurismo2ArcadePC.func_8001738C,
            [0x80017408u] = GranTurismo2ArcadePC.func_80017408,
            [0x80017598u] = GranTurismo2ArcadePC.func_80017598,
            [0x800175F0u] = GranTurismo2ArcadePC.func_800175F0,
            [0x8001786Cu] = GranTurismo2ArcadePC.func_8001786C,
            [0x800179E0u] = GranTurismo2ArcadePC.func_800179E0,
            [0x80017AD4u] = GranTurismo2ArcadePC.func_80017AD4,
            [0x80017B98u] = GranTurismo2ArcadePC.func_80017B98,
            [0x80017CC8u] = GranTurismo2ArcadePC.func_80017CC8,
            [0x800181E0u] = GranTurismo2ArcadePC.func_800181E0,
            [0x80018350u] = GranTurismo2ArcadePC.func_80018350_gt2_arcade_overlay_1,
            [0x80018B68u] = GranTurismo2ArcadePC.func_80018B68,
            [0x80018CA4u] = GranTurismo2ArcadePC.func_80018CA4,
            [0x80018D5Cu] = GranTurismo2ArcadePC.func_80018D5C,
            [0x80018D84u] = GranTurismo2ArcadePC.func_80018D84,
            [0x80018DA4u] = GranTurismo2ArcadePC.func_80018DA4,
            [0x80018E08u] = GranTurismo2ArcadePC.func_80018E08,
            [0x80018E10u] = GranTurismo2ArcadePC.func_80018E10,
            [0x80018E40u] = GranTurismo2ArcadePC.func_80018E40,
            [0x80018E7Cu] = GranTurismo2ArcadePC.func_80018E7C,
            [0x80018E84u] = GranTurismo2ArcadePC.func_80018E84,
            [0x80018EBCu] = GranTurismo2ArcadePC.func_80018EBC,
            [0x80018EF4u] = GranTurismo2ArcadePC.func_80018EF4,
            [0x80018F74u] = GranTurismo2ArcadePC.func_80018F74,
            [0x80018FE8u] = GranTurismo2ArcadePC.func_80018FE8,
            [0x80018FF4u] = GranTurismo2ArcadePC.func_80018FF4,
            [0x80019104u] = GranTurismo2ArcadePC.func_80019104,
            [0x80019204u] = GranTurismo2ArcadePC.func_80019204,
            [0x80019354u] = GranTurismo2ArcadePC.func_80019354,
            [0x800195A0u] = GranTurismo2ArcadePC.func_800195A0,
            [0x80019790u] = GranTurismo2ArcadePC.func_80019790,
            [0x800198B8u] = GranTurismo2ArcadePC.func_800198B8,
            [0x800199C8u] = GranTurismo2ArcadePC.func_800199C8,
            [0x80019BB0u] = GranTurismo2ArcadePC.func_80019BB0,
            [0x8001A290u] = GranTurismo2ArcadePC.func_8001A290,
            [0x8001A44Cu] = GranTurismo2ArcadePC.func_8001A44C,
            [0x8001A4A0u] = GranTurismo2ArcadePC.func_8001A4A0,
            [0x8001A4B8u] = GranTurismo2ArcadePC.func_8001A4B8,
            [0x8001A4CCu] = GranTurismo2ArcadePC.func_8001A4CC,
            [0x8001A6F0u] = GranTurismo2ArcadePC.func_8001A6F0,
            [0x8001A740u] = GranTurismo2ArcadePC.func_8001A740,
            [0x8001A774u] = GranTurismo2ArcadePC.func_8001A774,
            [0x8001A7ACu] = GranTurismo2ArcadePC.func_8001A7AC,
            [0x8001A8E4u] = GranTurismo2ArcadePC.func_8001A8E4,
            [0x8001A920u] = GranTurismo2ArcadePC.func_8001A920,
            [0x8001AA08u] = GranTurismo2ArcadePC.func_8001AA08,
            [0x8001AA28u] = GranTurismo2ArcadePC.func_8001AA28,
            [0x8001AA48u] = GranTurismo2ArcadePC.func_8001AA48,
            [0x8001AA68u] = GranTurismo2ArcadePC.func_8001AA68,
            [0x8001AAA0u] = GranTurismo2ArcadePC.func_8001AAA0,
            [0x8001ABD8u] = GranTurismo2ArcadePC.func_8001ABD8_gt2_arcade_overlay_1,
            [0x8001AF2Cu] = GranTurismo2ArcadePC.func_8001AF2C,
            [0x8001B464u] = GranTurismo2ArcadePC.func_8001B464,
            [0x8001B4CCu] = GranTurismo2ArcadePC.func_8001B4CC,
            [0x8001B4ECu] = GranTurismo2ArcadePC.func_8001B4EC,
            [0x8001B50Cu] = GranTurismo2ArcadePC.func_8001B50C,
            [0x8001B644u] = GranTurismo2ArcadePC.func_8001B644,
            [0x8001B8C8u] = GranTurismo2ArcadePC.func_8001B8C8,
            [0x8001BB6Cu] = GranTurismo2ArcadePC.func_8001BB6C_gt2_arcade_overlay_1,
            [0x8001BB98u] = GranTurismo2ArcadePC.func_8001BB98,
            [0x8001BBC4u] = GranTurismo2ArcadePC.func_8001BBC4,
            [0x8001BBF0u] = GranTurismo2ArcadePC.func_8001BBF0,
            [0x8001BC34u] = GranTurismo2ArcadePC.func_8001BC34,
            [0x8001BC78u] = GranTurismo2ArcadePC.func_8001BC78,
            [0x8001BCBCu] = GranTurismo2ArcadePC.func_8001BCBC,
            [0x8001BD1Cu] = GranTurismo2ArcadePC.func_8001BD1C,
            [0x8001BD7Cu] = GranTurismo2ArcadePC.func_8001BD7C,
            [0x8001BDDCu] = GranTurismo2ArcadePC.func_8001BDDC,
            [0x8001BE34u] = GranTurismo2ArcadePC.func_8001BE34,
            [0x8001BE7Cu] = GranTurismo2ArcadePC.func_8001BE7C,
            [0x8001BEDCu] = GranTurismo2ArcadePC.func_8001BEDC,
            [0x8001BF0Cu] = GranTurismo2ArcadePC.func_8001BF0C,
            [0x8001C0F8u] = GranTurismo2ArcadePC.func_8001C0F8,
            [0x8001C27Cu] = GranTurismo2ArcadePC.func_8001C27C,
            [0x8001C2B4u] = GranTurismo2ArcadePC.func_8001C2B4,
            [0x8001C2F0u] = GranTurismo2ArcadePC.func_8001C2F0,
            [0x8001C2FCu] = GranTurismo2ArcadePC.func_8001C2FC,
            [0x8001C424u] = GranTurismo2ArcadePC.func_8001C424,
            [0x8001CA94u] = GranTurismo2ArcadePC.func_8001CA94,
            [0x8001D11Cu] = GranTurismo2ArcadePC.func_8001D11C,
            [0x8001D140u] = GranTurismo2ArcadePC.func_8001D140,
            [0x8001D154u] = GranTurismo2ArcadePC.func_8001D154,
            [0x8001D160u] = GranTurismo2ArcadePC.func_8001D160,
            [0x8001D3C0u] = GranTurismo2ArcadePC.func_8001D3C0,
            [0x8001D42Cu] = GranTurismo2ArcadePC.func_8001D42C,
            [0x8001D674u] = GranTurismo2ArcadePC.func_8001D674,
            [0x8001D7A4u] = GranTurismo2ArcadePC.func_8001D7A4,
            [0x8001D7DCu] = GranTurismo2ArcadePC.func_8001D7DC,
            [0x8001D840u] = GranTurismo2ArcadePC.func_8001D840,
            [0x8001D848u] = GranTurismo2ArcadePC.func_8001D848,
            [0x8001D878u] = GranTurismo2ArcadePC.func_8001D878,
            [0x8001D8B4u] = GranTurismo2ArcadePC.func_8001D8B4,
            [0x8001D8BCu] = GranTurismo2ArcadePC.func_8001D8BC,
            [0x8001D9A8u] = GranTurismo2ArcadePC.func_8001D9A8,
            [0x8001DA1Cu] = GranTurismo2ArcadePC.func_8001DA1C,
            [0x8001DB5Cu] = GranTurismo2ArcadePC.func_8001DB5C,
            [0x8001DB88u] = GranTurismo2ArcadePC.func_8001DB88,
            [0x8001DBD8u] = GranTurismo2ArcadePC.func_8001DBD8,
            [0x8001DC74u] = GranTurismo2ArcadePC.func_8001DC74,
            [0x8001DC98u] = GranTurismo2ArcadePC.func_8001DC98,
            [0x8001DDA0u] = GranTurismo2ArcadePC.func_8001DDA0,
            [0x8001DF08u] = GranTurismo2ArcadePC.func_8001DF08,
            [0x8001E010u] = GranTurismo2ArcadePC.func_8001E010,
            [0x8001E08Cu] = GranTurismo2ArcadePC.func_8001E08C,
            [0x8001E108u] = GranTurismo2ArcadePC.func_8001E108,
            [0x8001E184u] = GranTurismo2ArcadePC.func_8001E184,
            [0x8001E1D4u] = GranTurismo2ArcadePC.func_8001E1D4,
            [0x8001E27Cu] = GranTurismo2ArcadePC.func_8001E27C,
            [0x8001E2A0u] = GranTurismo2ArcadePC.func_8001E2A0,
            [0x8001E388u] = GranTurismo2ArcadePC.func_8001E388,
            [0x8001E3D8u] = GranTurismo2ArcadePC.func_8001E3D8,
            [0x8001E468u] = GranTurismo2ArcadePC.func_8001E468,
            [0x8001E48Cu] = GranTurismo2ArcadePC.func_8001E48C,
            [0x8001E5ACu] = GranTurismo2ArcadePC.func_8001E5AC,
            [0x8001E624u] = GranTurismo2ArcadePC.func_8001E624,
            [0x8001E75Cu] = GranTurismo2ArcadePC.func_8001E75C,
            [0x8001E7E4u] = GranTurismo2ArcadePC.func_8001E7E4,
            [0x8001E858u] = GranTurismo2ArcadePC.func_8001E858,
            [0x8001EAA8u] = GranTurismo2ArcadePC.func_8001EAA8_gt2_arcade_overlay_1,
            [0x8001ED08u] = GranTurismo2ArcadePC.func_8001ED08,
            [0x8001EDA0u] = GranTurismo2ArcadePC.func_8001EDA0,
            [0x8001EFDCu] = GranTurismo2ArcadePC.func_8001EFDC,
            [0x8001F18Cu] = GranTurismo2ArcadePC.func_8001F18C,
            [0x8001F1F4u] = GranTurismo2ArcadePC.func_8001F1F4,
            [0x8001F234u] = GranTurismo2ArcadePC.func_8001F234,
            [0x8001F370u] = GranTurismo2ArcadePC.func_8001F370,
            [0x8001F3C0u] = GranTurismo2ArcadePC.func_8001F3C0,
            [0x8001F404u] = GranTurismo2ArcadePC.func_8001F404,
            [0x8001F51Cu] = GranTurismo2ArcadePC.func_8001F51C,
            [0x8001F6A8u] = GranTurismo2ArcadePC.func_8001F6A8,
            [0x8001F7A4u] = GranTurismo2ArcadePC.func_8001F7A4,
            [0x8001F880u] = GranTurismo2ArcadePC.func_8001F880,
            [0x8001F900u] = GranTurismo2ArcadePC.func_8001F900,
            [0x8001F934u] = GranTurismo2ArcadePC.func_8001F934,
            [0x8001FA4Cu] = GranTurismo2ArcadePC.func_8001FA4C,
            [0x8001FBA0u] = GranTurismo2ArcadePC.func_8001FBA0,
            [0x8001FBF8u] = GranTurismo2ArcadePC.func_8001FBF8,
            [0x8001FD6Cu] = GranTurismo2ArcadePC.func_8001FD6C,
            [0x8001FECCu] = GranTurismo2ArcadePC.func_8001FECC,
            [0x8001FEFCu] = GranTurismo2ArcadePC.func_8001FEFC,
            [0x80020034u] = GranTurismo2ArcadePC.func_80020034,
            [0x80020120u] = GranTurismo2ArcadePC.func_80020120,
            [0x80020244u] = GranTurismo2ArcadePC.func_80020244,
            [0x8002024Cu] = GranTurismo2ArcadePC.func_8002024C,
            [0x800202B0u] = GranTurismo2ArcadePC.func_800202B0,
            [0x8002040Cu] = GranTurismo2ArcadePC.func_8002040C,
            [0x800204ACu] = GranTurismo2ArcadePC.func_800204AC,
            [0x800204ECu] = GranTurismo2ArcadePC.func_800204EC_gt2_arcade_overlay_1,
            [0x800206C0u] = GranTurismo2ArcadePC.func_800206C0,
            [0x80020938u] = GranTurismo2ArcadePC.func_80020938,
            [0x80020964u] = GranTurismo2ArcadePC.func_80020964,
            [0x80020990u] = GranTurismo2ArcadePC.func_80020990,
            [0x800209BCu] = GranTurismo2ArcadePC.func_800209BC,
            [0x800209E8u] = GranTurismo2ArcadePC.func_800209E8,
            [0x80020A14u] = GranTurismo2ArcadePC.func_80020A14,
            [0x80020A40u] = GranTurismo2ArcadePC.func_80020A40,
            [0x80020A50u] = GranTurismo2ArcadePC.func_80020A50,
            [0x80020A98u] = GranTurismo2ArcadePC.func_80020A98,
            [0x8004B2B4u] = GranTurismo2ArcadePC.func_8004B2B4,
        };
}
