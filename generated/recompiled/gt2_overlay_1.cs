using RecompOne.Runtime.Context;
using RecompOne.Runtime.Dispatch;
using RecompOne.Runtime.Memory;

namespace Recompiled.Simulation;

public static partial class GranTurismo2PC
{
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010000_gt2_overlay_1(CpuContext c, IMemory m)
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
    public static void func_8001003C_gt2_overlay_1(CpuContext c, IMemory m)
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
    public static void func_80010078_gt2_overlay_1(CpuContext c, IMemory m)
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
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x2878u));
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
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x2870u));
        c.V0 = c.V0 + 0x10u;
        goto L80010220;
        L8001021C: ;
        c.V0 = c.A1 + 0x100u;
        L80010220: ;
        c.S7 = 0x00000040u;
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0x58u));
        c.S4 = c.V0 + 0u;
        c.RA = 0x80010230u;
        GranTurismo2PC.func_80078138(c, m);
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
        GranTurismo2PC.func_80083AE0(c, m);
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
        GranTurismo2PC.func_80078038(c, m);
        c.A0 = MemoryAccess.ReadU32(m, c.V0);
        c.RA = 0x80010294u;
        GranTurismo2PC.func_80060B70(c, m);
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
        GranTurismo2PC.func_80078038(c, m);
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
        GranTurismo2PC.func_80083AE0(c, m);
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
        GranTurismo2PC.func_80078038(c, m);
        c.V1 = MemoryAccess.ReadU16(m, (c.SP + 0x20u));
        c.S4 = MemoryAccess.ReadU32(m, c.V0);
        if (c.V1 == 0u) {
            c.V0 = 0x80090000u;
            goto L80010364;
        }
        c.V0 = 0x80090000u;
        c.V0 = c.V0 + 0x1620u;
        c.V0 = c.V1 + c.V0;
        c.A1 = MemoryAccess.ReadU8(m, c.V0);
        c.A0 = c.S4 + 0u;
        c.S0 = c.A1 + 0u;
        c.A1 = c.A1 << 24;
        c.A1 = (uint)((int)c.A1 >> 24);
        c.RA = 0x8001035Cu;
        GranTurismo2PC.func_80060D28(c, m);
        c.V1 = c.V0 + 0u;
        goto L800103E0;
        L80010364: ;
        c.A0 = c.S4 + 0u;
        c.A1 = c.SP + 0x18u;
        c.A2 = c.SP + 0x1Cu;
        c.RA = 0x80010374u;
        GranTurismo2PC.func_80060BEC(c, m);
        c.S3 = c.V0 + 0u;
        c.S2 = 0u + 0u;
        L8001037C: ;
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0x5Cu));
        c.RA = 0x80010388u;
        GranTurismo2PC.func_80083AE0(c, m);
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
        c.V0 = c.V0 + 0x1620u;
        c.V1 = c.A2 & 0x003Fu;
        c.V1 = c.V1 + c.V0;
        c.A1 = MemoryAccess.ReadU8(m, c.V1);
        c.S0 = c.A1 + 0u;
        c.A1 = c.A1 << 24;
        c.A1 = (uint)((int)c.A1 >> 24);
        c.RA = 0x8001047Cu;
        GranTurismo2PC.func_80060D28(c, m);
        MemoryAccess.WriteU8(m, (c.S1 + 0x5u), (byte)c.S0);
        goto L800104C8;
        L80010484: ;
        c.A1 = c.SP + 0x10u;
        c.A2 = c.SP + 0x14u;
        c.RA = 0x80010490u;
        GranTurismo2PC.func_80060BEC(c, m);
        c.A0 = c.S0 + 0u;
        c.S0 = c.V0 + 0u;
        c.RA = 0x8001049Cu;
        GranTurismo2PC.func_80083AE0(c, m);
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
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x2E6Cu));
        c.V0 = 0x801D0000u;
        c.V0 = c.V0 - 0x6720u;
        MemoryAccess.WriteU32(m, (c.SP + 0x78u), c.S0);
        c.S0 = c.V0 + 0u;
        c.V0 = 0u | 0xBF7Cu;
        MemoryAccess.WriteU32(m, (c.SP + 0x8Cu), c.S5);
        c.S5 = c.S0 + c.V0;
        c.V0 = 0x80090000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x2E70u));
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
        GranTurismo2PC.func_8007D23C(c, m);
        c.A0 = c.S5 + 0u;
        c.A1 = 0u + 0u;
        c.A2 = 0x0000058Cu;
        MemoryAccess.WriteU32(m, (c.SP + 0x60u), c.V0);
        c.RA = 0x80010574u;
        GranTurismo2PC.func_8008CE30(c, m);
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
        GranTurismo2PC.func_8007816C(c, m);
        c.A0 = c.S5 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x800105E8u;
        GranTurismo2PC.func_8005E548(c, m);
        c.A1 = MemoryAccess.ReadU16(m, (c.FP + 0x2u));
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0x64u));
        c.RA = 0x800105F8u;
        GranTurismo2PC.func_8007816C(c, m);
        c.A0 = c.S5 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x80010604u;
        GranTurismo2PC.func_8005E5F0(c, m);
        c.A1 = MemoryAccess.ReadU16(m, (c.FP + 0x94u));
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0x64u));
        c.RA = 0x80010614u;
        GranTurismo2PC.func_8007816C(c, m);
        c.A0 = c.S5 + 0x44u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x80010620u;
        GranTurismo2PC.func_8008CEDC(c, m);
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
        GranTurismo2PC.func_80078138(c, m);
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
        GranTurismo2PC.func_8008CE30(c, m);
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
        GranTurismo2PC.func_800101C4(c, m);
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
        GranTurismo2PC.func_80076FC0(c, m);
        goto L8001083C;
        L800107E4: ;
        c.A0 = MemoryAccess.ReadU16(m, (c.S1 + 0x8u));
        c.RA = 0x800107F0u;
        GranTurismo2PC.func_800768C0(c, m);
        c.A0 = c.V0 + 0u;
        c.A1 = c.S0 + 0x8u;
        c.RA = 0x800107FCu;
        GranTurismo2PC.func_80076F5C(c, m);
        c.A1 = MemoryAccess.ReadU16(m, (c.S0 + 0x24u));
        c.A0 = 0x00000005u;
        c.RA = 0x80010808u;
        GranTurismo2PC.func_80076F2C(c, m);
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
        GranTurismo2PC.func_80010434(c, m);
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
        GranTurismo2PC.func_80060AE8(c, m);
        c.A0 = c.S0 + 0x90u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x80010890u;
        GranTurismo2PC.func_8008CEDC(c, m);
        c.V0 = (int)c.S3 < (int)c.S6 ? 1u : 0u;
        goto L800106E8;
        L80010898: ;
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0x64u));
        c.A1 = c.S5 + 0x10u;
        c.RA = 0x800108A4u;
        GranTurismo2PC.func_8007830C(c, m);
        c.FP = c.V0 + 0u;
        L800108A8: ;
        c.V0 = 0x801D0000u;
        c.A1 = c.V0 - 0x6760u;
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
        c.S3 = c.V0 - 0x6720u;
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
        GranTurismo2PC.func_800771AC(c, m);
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
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x2E70u));
        c.V0 = 0x801D0000u;
        c.V0 = c.V0 - 0x6720u;
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
        GranTurismo2PC.func_8007830C(c, m);
        c.V1 = 0x801D0000u;
        c.V1 = c.V1 - 0x6760u;
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
        c.S4 = c.V0 - 0x6720u;
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
        GranTurismo2PC.func_800771AC(c, m);
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
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x2E6Cu));
        c.V0 = 0x801D0000u;
        c.V0 = c.V0 - 0x6720u;
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
        GranTurismo2PC.func_8007830C(c, m);
        c.V1 = 0x801D0000u;
        c.V1 = c.V1 - 0x6760u;
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
        c.S4 = c.V0 - 0x6720u;
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
        GranTurismo2PC.func_800771AC(c, m);
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
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x2E6Cu));
        c.V0 = 0x801D0000u;
        c.V0 = c.V0 - 0x6720u;
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
        GranTurismo2PC.func_8007830C(c, m);
        c.V1 = 0x801D0000u;
        c.V1 = c.V1 - 0x6760u;
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
        c.S5 = c.V0 - 0x6720u;
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
        GranTurismo2PC.func_800771AC(c, m);
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
        c.V0 = c.V0 - 0x6720u;
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
        GranTurismo2PC.func_800771AC(c, m);
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
        c.V0 = MemoryAccess.ReadU32(m, (c.A1 + 0x3D9Cu));
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        if (c.V0 == 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
            goto L80010EC4;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A1 + 0x3D9Cu;
        L80010E90: ;
        c.A1 = MemoryAccess.ReadU32(m, c.S0);
        c.A0 = c.S1 + 0u;
        c.RA = 0x80010E9Cu;
        GranTurismo2PC.func_8008CF00(c, m);
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
        c.V0 = 0x000000A6u;
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
        c.V0 = c.V0 - 0x6720u;
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
        c.A0 = c.A0 + 0x15C0u;
        c.A1 = 0x00030000u;
        c.RA = 0x80010F38u;
        GranTurismo2PC.func_80076E04(c, m);
        c.S0 = 0x80090000u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x2E6Cu), c.V0);
        c.RA = 0x80010F44u;
        GranTurismo2PC.func_80010C50(c, m);
        if (c.V0 == 0u) {
            c.A0 = 0x800E0000u;
            goto L800110A4;
        }
        c.A0 = 0x800E0000u;
        c.A0 = c.A0 + 0x15C0u;
        c.A1 = 0x000C0000u;
        c.A1 = c.A1 | 0x8000u;
        c.RA = 0x80010F5Cu;
        GranTurismo2PC.func_80076D74(c, m);
        MemoryAccess.WriteU32(m, (c.S0 + 0x2E6Cu), c.V0);
        c.RA = 0x80010F64u;
        GranTurismo2PC.func_80010DC4(c, m);
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
        c.V0 = c.V0 + 0x1004u;
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
        c.A0 = c.A0 + 0x15C0u;
        c.A1 = 0x00030000u;
        c.RA = 0x80010FB0u;
        GranTurismo2PC.func_80076CF8(c, m);
        c.A0 = 0x800E0000u;
        c.A0 = c.A0 + 0x15C0u;
        c.A1 = 0x000C0000u;
        c.A1 = c.A1 | 0x8000u;
        c.V1 = 0x80090000u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x2E70u), c.V0);
        c.RA = 0x80010FCCu;
        GranTurismo2PC.func_80076D74(c, m);
        c.V1 = 0x80090000u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x2E6Cu), c.V0);
        c.RA = 0x80010FD8u;
        GranTurismo2PC.func_800109C0(c, m);
        goto L800110A4;
        L80010FE0: ;
        c.A0 = c.S3 + 0x10u;
        c.RA = 0x80010FE8u;
        GranTurismo2PC.func_80010E6C(c, m);
        c.A0 = c.V0 + 0u;
        c.S0 = 0x800B0000u;
        c.S0 = c.S0 + 0x15C0u;
        c.A1 = c.S0 + 0u;
        c.RA = 0x80010FFCu;
        GranTurismo2PC.func_8005D8D4(c, m);
        c.A0 = c.S0 + 0u;
        c.RA = 0x80011004u;
        GranTurismo2PC.func_80069DDC(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x00030000u;
        c.RA = 0x80011010u;
        GranTurismo2PC.func_80076CF8(c, m);
        c.A0 = 0x800E0000u;
        c.A0 = c.A0 + 0x15C0u;
        c.A1 = 0x000C0000u;
        c.A1 = c.A1 | 0x8000u;
        c.V1 = 0x80090000u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x2E70u), c.V0);
        c.RA = 0x8001102Cu;
        GranTurismo2PC.func_80076D74(c, m);
        c.V1 = 0x80090000u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x2E6Cu), c.V0);
        c.RA = 0x80011038u;
        GranTurismo2PC.func_800109C0(c, m);
        c.S2 = 0u + 0u;
        c.V0 = 0x801D0000u;
        c.S4 = c.V0 - 0x6720u;
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
        GranTurismo2PC.func_800771AC(c, m);
        c.S1 = c.S1 + 0x1C0u;
        c.S0 = c.S0 + 0xD0u;
        c.S2 = c.S2 + 0x1u;
        goto L80011058;
        L80011088: ;
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x15C0u;
        c.A1 = 0x00030000u;
        c.RA = 0x80011098u;
        GranTurismo2PC.func_80076E88(c, m);
        c.V1 = 0x80090000u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x2E6Cu), c.V0);
        c.RA = 0x800110A4u;
        GranTurismo2PC.func_80010B08(c, m);
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
        GranTurismo2PC.func_8005D8A0(c, m);
        c.A0 = c.S0 + 0u;
        c.RA = 0x800110ECu;
        GranTurismo2PC.func_8005D768(c, m);
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
        GranTurismo2PC.func_8007BBD4(c, m);
        c.RA = 0x80011168u;
        GranTurismo2PC.func_8007AF30(c, m);
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
        c.S0 = c.S0 + 0x15C0u;
        c.A0 = c.S0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.A1 = 0x00000016u;
        c.RA = 0x80011198u;
        GranTurismo2PC.func_800110C4(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x0000001Du;
        c.RA = 0x800111A4u;
        GranTurismo2PC.func_800110FC(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x0000000Du;
        c.RA = 0x800111B0u;
        GranTurismo2PC.func_800110C4(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x0000001Eu;
        c.RA = 0x800111BCu;
        GranTurismo2PC.func_800110FC(c, m);
        c.A0 = 0x80020000u;
        c.A0 = c.A0 + 0x3EC4u;
        c.A1 = 0u | 0xC800u;
        c.RA = 0x800111CCu;
        GranTurismo2PC.func_800609F8(c, m);
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
        c.V1 = c.V1 + 0x3E7Cu;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S0);
        c.S0 = 0x800E0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S1);
        c.S1 = MemoryAccess.ReadU8(m, (c.V0 - 0x6720u));
        c.S0 = c.S0 + 0x15C0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S2);
        c.V0 = c.S1 << 1;
        c.V0 = c.V0 + c.V1;
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, c.V0);
        c.A0 = c.S0 + 0u;
        c.RA = 0x8001121Cu;
        GranTurismo2PC.func_800110C4(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x0000000Eu;
        c.RA = 0x80011228u;
        GranTurismo2PC.func_800110FC(c, m);
        c.A0 = 0x0000003Eu;
        c.A1 = c.S0 + 0u;
        c.RA = 0x80011234u;
        GranTurismo2PC.func_8005D8D4(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x0000000Cu;
        c.RA = 0x80011240u;
        GranTurismo2PC.func_800110FC(c, m);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x3EA8u;
        c.S1 = c.S1 << 2;
        c.S1 = c.S1 + c.V0;
        c.A0 = MemoryAccess.ReadU32(m, c.S1);
        c.A1 = c.S0 + 0u;
        c.RA = 0x8001125Cu;
        GranTurismo2PC.func_8005D8D4(c, m);
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
        GranTurismo2PC.func_8007BBD4(c, m);
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
        GranTurismo2PC.func_8007BBD4(c, m);
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
        c.SP = c.SP - 0x28u;
        c.A0 = 0x0000002Fu;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S0);
        c.S0 = 0x800E0000u;
        c.S0 = c.S0 + 0x15C0u;
        c.A1 = c.S0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S1);
        c.RA = 0x800112F8u;
        GranTurismo2PC.func_8005D8D4(c, m);
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
        c.RA = 0x80011328u;
        GranTurismo2PC.func_8007BBD4(c, m);
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.SP + 0x10u), (ushort)c.S2);
        MemoryAccess.WriteU16(m, (c.SP + 0x12u), (ushort)0u);
        c.A0 = c.A0 + c.S1;
        c.V0 = MemoryAccess.ReadU16(m, (c.A0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.SP + 0x14u), (ushort)c.V0);
        c.V0 = MemoryAccess.ReadU16(m, (c.A0 + 0xAu));
        c.A1 = c.SP + 0x10u;
        MemoryAccess.WriteU16(m, (c.SP + 0x16u), (ushort)c.V0);
        c.RA = 0x80011354u;
        GranTurismo2PC.func_8007BBD4(c, m);
        c.A0 = 0x00000030u;
        c.A1 = c.S0 + 0u;
        c.RA = 0x80011360u;
        GranTurismo2PC.func_8005D8D4(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x0000001Cu;
        c.RA = 0x8001136Cu;
        GranTurismo2PC.func_800110FC(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80011384(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x240u;
        MemoryAccess.WriteU32(m, (c.SP + 0x23Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x238u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x234u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x230u), c.S0);
        c.RA = 0x8001139Cu;
        GranTurismo2PC.func_80011178(c, m);
        c.S0 = 0x801F0000u;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 - 0xA10u));
        if (c.V0 != 0u) {
            goto L80011400;
        }
        c.RA = 0x800113B8u;
        GranTurismo2PC.func_8001636C(c, m);
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.S0 - 0xA10u), (byte)c.V0);
        c.A0 = c.SP + 0x10u;
        c.RA = 0x800113C8u;
        GranTurismo2PC.func_80011B5C(c, m);
        c.A0 = c.SP + 0x10u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x4700u;
        c.RA = 0x800113D8u;
        GranTurismo2PC.func_80011BB8(c, m);
        L800113D8: ;
        c.A0 = c.SP + 0x10u;
        c.RA = 0x800113E0u;
        GranTurismo2PC.func_800833E8(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L800113D8;
        }
        if ((int)c.V1 < 0) {
            c.A0 = c.SP + 0x10u;
            goto L800113D8;
        }
        c.A0 = c.SP + 0x10u;
        c.A1 = 0x00000002u;
        c.RA = 0x80011400u;
        GranTurismo2PC.func_80011B90(c, m);
        L80011400: ;
        c.S0 = 0x801D0000u;
        c.RA = 0x80011408u;
        GranTurismo2PC.func_8001636C(c, m);
        c.S0 = c.S0 - 0x6720u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x80011414u;
        GranTurismo2PC.func_800117B4(c, m);
        c.S0 = c.S0 + 0xB8u;
        c.RA = 0x8001141Cu;
        GranTurismo2PC.func_800111DC(c, m);
        c.V0 = 0x801F0000u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x44u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.S0 + 0x46u), (ushort)0u);
        c.V1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V0 - 0xA0Eu));
        c.V0 = 0x00000002u;
        if (c.V1 == c.V0) {
            c.V0 = (int)c.V1 < 3 ? 1u : 0u;
            goto L8001167C;
        }
        c.V0 = (int)c.V1 < 3 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A0 = 0x00000001u;
            goto L80011448;
        }
        c.A0 = 0x00000001u;
        if (c.V1 == 0u) {
            goto L80011458;
        }
        L80011448: ;
        c.A1 = 0x80010000u;
        c.A1 = c.A1 + 0x172Cu;
        c.A2 = 0u + 0u;
        c.RA = 0x80011458u;
        GranTurismo2PC.func_8005DA7C(c, m);
        L80011458: ;
        c.A0 = c.SP + 0x10u;
        c.RA = 0x80011460u;
        GranTurismo2PC.func_80011B5C(c, m);
        c.A0 = c.SP + 0x10u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x4388u;
        c.RA = 0x80011470u;
        GranTurismo2PC.func_80011BB8(c, m);
        L80011470: ;
        c.A0 = c.SP + 0x10u;
        c.RA = 0x80011478u;
        GranTurismo2PC.func_800833E8(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L80011470;
        }
        if ((int)c.V1 < 0) {
            c.A0 = c.SP + 0x10u;
            goto L80011470;
        }
        c.A0 = c.SP + 0x10u;
        c.A1 = 0x00000002u;
        c.RA = 0x80011498u;
        GranTurismo2PC.func_80011B90(c, m);
        c.A0 = 0x00000001u;
        c.RA = 0x800114A0u;
        GranTurismo2PC.func_8007F830(c, m);
        c.V0 = 0x801F0000u;
        c.V1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V0 - 0xA0Du));
        c.V0 = c.V1 < 0x00000007u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80020000u;
            goto L80011514;
        }
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x1034u;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x800114D4u: goto L800114D4;
            case 0x800114FCu: goto L800114FC;
            case 0x80011594u: goto L80011594;
            case 0x80011514u: goto L80011514;
            case 0x80011554u: goto L80011554;
            case 0x800115DCu: goto L800115DC;
            case 0x80011624u: goto L80011624;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L800114D4: ;
        c.A0 = 0x00000004u;
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0xA10u;
        c.V1 = c.A0 + 0u;
        MemoryAccess.WriteU8(m, (c.V0 + 0x1u), (byte)c.V1);
        c.V1 = 0x00000003u;
        MemoryAccess.WriteU8(m, (c.V0 + 0x2u), (byte)c.V1);
        c.RA = 0x800114F4u;
        GranTurismo2PC.func_8005DA3C(c, m);
        goto L80011514;
        L800114FC: ;
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0xA10u;
        c.V1 = 0x00000002u;
        MemoryAccess.WriteU8(m, (c.V0 + 0x1u), (byte)c.V1);
        MemoryAccess.WriteU8(m, (c.V0 + 0x2u), (byte)c.V1);
        goto L8001167C;
        L80011514: ;
        c.A0 = c.SP + 0x10u;
        c.RA = 0x8001151Cu;
        GranTurismo2PC.func_80011B5C(c, m);
        c.A0 = c.SP + 0x10u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x4B3Cu;
        c.RA = 0x8001152Cu;
        GranTurismo2PC.func_80011BB8(c, m);
        L8001152C: ;
        c.A0 = c.SP + 0x10u;
        c.RA = 0x80011534u;
        GranTurismo2PC.func_800833E8(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L8001152C;
        }
        if ((int)c.V1 < 0) {
            c.A0 = c.SP + 0x10u;
            goto L8001152C;
        }
        c.A0 = c.SP + 0x10u;
        goto L80011614;
        L80011554: ;
        c.A0 = c.SP + 0x10u;
        c.RA = 0x8001155Cu;
        GranTurismo2PC.func_80011B5C(c, m);
        c.A0 = c.SP + 0x10u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x4A94u;
        c.RA = 0x8001156Cu;
        GranTurismo2PC.func_80011BB8(c, m);
        L8001156C: ;
        c.A0 = c.SP + 0x10u;
        c.RA = 0x80011574u;
        GranTurismo2PC.func_800833E8(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L8001156C;
        }
        if ((int)c.V1 < 0) {
            c.A0 = c.SP + 0x10u;
            goto L8001156C;
        }
        c.A0 = c.SP + 0x10u;
        goto L80011614;
        L80011594: ;
        c.A0 = c.SP + 0x10u;
        c.RA = 0x8001159Cu;
        GranTurismo2PC.func_80011B5C(c, m);
        c.A0 = c.SP + 0x10u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x402Cu;
        c.RA = 0x800115ACu;
        GranTurismo2PC.func_80011BB8(c, m);
        L800115AC: ;
        c.A0 = c.SP + 0x10u;
        c.RA = 0x800115B4u;
        GranTurismo2PC.func_800833E8(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L800115AC;
        }
        if ((int)c.V1 < 0) {
            c.A0 = 0x801D0000u;
            goto L800115AC;
        }
        c.A0 = 0x801D0000u;
        c.A0 = c.A0 - 0x6720u;
        c.RA = 0x800115D4u;
        GranTurismo2PC.func_800117B4(c, m);
        c.A0 = c.SP + 0x10u;
        goto L80011614;
        L800115DC: ;
        c.A0 = c.SP + 0x10u;
        c.RA = 0x800115E4u;
        GranTurismo2PC.func_80011B5C(c, m);
        c.A0 = c.SP + 0x10u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x3B60u;
        c.RA = 0x800115F4u;
        GranTurismo2PC.func_80011BB8(c, m);
        L800115F4: ;
        c.A0 = c.SP + 0x10u;
        c.RA = 0x800115FCu;
        GranTurismo2PC.func_800833E8(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L800115F4;
        }
        if ((int)c.V1 < 0) {
            c.A0 = c.SP + 0x10u;
            goto L800115F4;
        }
        c.A0 = c.SP + 0x10u;
        L80011614: ;
        c.A1 = 0x00000002u;
        c.RA = 0x8001161Cu;
        GranTurismo2PC.func_80011B90(c, m);
        goto L80011458;
        L80011624: ;
        c.V0 = 0x801F0000u;
        c.S2 = c.V0 - 0xA10u;
        c.S0 = 0x800E0000u;
        c.S0 = c.S0 + 0x15C0u;
        c.S1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0xEu));
        c.A0 = c.S0 + 0u;
        MemoryAccess.WriteU8(m, (c.S2 + 0x1u), (byte)0u);
        MemoryAccess.WriteU8(m, (c.S2 + 0x2u), (byte)0u);
        c.RA = 0x80011648u;
        GranTurismo2PC.func_80020DCC(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x80100000u;
        c.A1 = c.A1 + 0x55C0u;
        c.S0 = c.V0 + 0u;
        c.A2 = c.S1 + 0u;
        c.RA = 0x80011660u;
        GranTurismo2PC.func_80020E14(c, m);
        c.S1 = c.S1 + 0x1u;
        c.S0 = (int)c.S1 < (int)c.S0 ? 1u : 0u;
        if (c.S0 != 0u) {
            goto L80011674;
        }
        c.S1 = 0u + 0u;
        L80011674: ;
        MemoryAccess.WriteU16(m, (c.S2 + 0xEu), (ushort)c.S1);
        goto L800116E8;
        L8001167C: ;
        c.A0 = c.SP + 0x10u;
        c.RA = 0x80011684u;
        GranTurismo2PC.func_80011B5C(c, m);
        c.A0 = c.SP + 0x10u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x4D34u;
        c.RA = 0x80011694u;
        GranTurismo2PC.func_80011BB8(c, m);
        L80011694: ;
        c.A0 = c.SP + 0x10u;
        c.RA = 0x8001169Cu;
        GranTurismo2PC.func_800833E8(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L80011694;
        }
        if ((int)c.V1 < 0) {
            c.A0 = c.SP + 0x10u;
            goto L80011694;
        }
        c.A0 = c.SP + 0x10u;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.A0 + 0x21Cu));
        if (c.V0 != 0u) {
            c.V0 = 0x801F0000u;
            goto L800116E0;
        }
        c.V0 = 0x801F0000u;
        c.A1 = 0x00000002u;
        c.V0 = c.V0 - 0xA10u;
        MemoryAccess.WriteU8(m, (c.V0 + 0x1u), (byte)0u);
        MemoryAccess.WriteU8(m, (c.V0 + 0x2u), (byte)0u);
        c.RA = 0x800116D8u;
        GranTurismo2PC.func_80011B90(c, m);
        goto L80011458;
        L800116E0: ;
        c.A1 = 0x00000002u;
        c.RA = 0x800116E8u;
        GranTurismo2PC.func_80011B90(c, m);
        L800116E8: ;
        c.RA = 0x800116F0u;
        GranTurismo2PC.func_80010EDC(c, m);
        c.A0 = 0x00000001u;
        c.RA = 0x800116F8u;
        GranTurismo2PC.func_8007D23C(c, m);
        c.A0 = 0x00000001u;
        c.RA = 0x80011700u;
        GranTurismo2PC.func_8007F830(c, m);
        c.A0 = 0x00000002u;
        c.RA = 0x80011708u;
        GranTurismo2PC.func_8007D23C(c, m);
        c.A0 = 0u + 0u;
        c.A1 = 0x80010000u;
        c.A1 = c.A1 + 0x1F64u;
        c.A2 = 0x00000001u;
        c.RA = 0x8001171Cu;
        GranTurismo2PC.func_8005DA7C(c, m);
        c.A0 = 0x00000001u;
        c.RA = 0x80011724u;
        GranTurismo2PC.func_8007F830(c, m);
        goto L80011458;
        c.SP = c.SP - 0x28u;
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x15C0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S0);
        c.S0 = 0x801F0000u;
        c.S0 = c.S0 - 0xA10u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.RA);
        c.V0 = MemoryAccess.ReadU16(m, (c.S0 + 0xEu));
        c.A1 = 0x00030000u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x2u), (byte)0u);
        c.V0 = c.V0 + 0x1u;
        MemoryAccess.WriteU16(m, (c.S0 + 0xEu), (ushort)c.V0);
        c.RA = 0x80011760u;
        GranTurismo2PC.func_80076E04(c, m);
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0xEu));
        c.V1 = 0x80090000u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x2E6Cu), c.V0);
        c.RA = 0x80011770u;
        Dispatcher.Call(c, m, 0x80060E44u);
        c.A0 = 0x00000001u;
        c.A1 = 0u + 0u;
        c.A2 = c.A1 + 0u;
        c.A3 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.RA = 0x80011790u;
        GranTurismo2PC.func_800104E0(c, m);
        c.A0 = 0u + 0u;
        c.A1 = 0x80010000u;
        c.A1 = c.A1 + 0x1F64u;
        c.A2 = 0x00000001u;
        c.RA = 0x800117A4u;
        GranTurismo2PC.func_8005DA7C(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800117B4(CpuContext c, IMemory m)
    {
        c.V0 = 0x800A0000u;
        c.A3 = c.V0 + 0x6EECu;
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
        c.V0 = c.V0 + 0x6EECu;
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
    public static void func_80011868(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A1 + 0u;
        c.A0 = c.S0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.A1 = c.V0 + 0u;
        c.RA = 0x80011888u;
        GranTurismo2PC.func_8005D8A0(c, m);
        c.A0 = c.S0 + 0u;
        c.RA = 0x80011890u;
        GranTurismo2PC.func_8005D768(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800118A0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        c.V1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.V0 = c.V0 >> 3;
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u) {
            c.A0 = c.V1 + 0x8u;
            goto L800118D0;
        }
        c.A0 = c.V1 + 0x8u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.A0 = c.A0 + c.V0;
        L800118D0: ;
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
        c.RA = 0x80011904u;
        GranTurismo2PC.func_8007BBD4(c, m);
        c.RA = 0x8001190Cu;
        GranTurismo2PC.func_8007AF30(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001191C(CpuContext c, IMemory m)
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
        c.S2 = c.V0 - 0x6A00u;
        if (c.FP == 0u) {
            c.S7 = c.V1 + 0x10u;
            goto L80011B2C;
        }
        c.S7 = c.V1 + 0x10u;
        c.V0 = MemoryAccess.ReadU32(m, (c.FP + 0x10u));
        if (c.V0 == 0u) {
            c.A0 = c.S2 + 0u;
            goto L80011B04;
        }
        c.A0 = c.S2 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6A10u;
        c.RA = 0x80011988u;
        GranTurismo2PC.func_8007DA80(c, m);
        c.A0 = c.S2 + 0u;
        c.V0 = (uint)((int)c.S6 >> 2);
        c.S1 = 0x00000022u;
        c.S1 = c.S1 - c.V0;
        MemoryAccess.WriteU32(m, (c.S2 + 0x10u), c.S7);
        c.A1 = MemoryAccess.ReadU32(m, (c.FP + 0x10u));
        c.A2 = c.S1 + 0u;
        c.RA = 0x800119A8u;
        GranTurismo2PC.func_8006AD3C(c, m);
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
        c.RA = 0x800119D8u;
        GranTurismo2PC.func_8007D024(c, m);
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
        c.RA = 0x80011A38u;
        GranTurismo2PC.func_8006AC90(c, m);
        c.A0 = c.S2 + 0u;
        c.A2 = c.S0 + 0x3u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S1);
        c.A1 = MemoryAccess.ReadU32(m, (c.FP + 0x10u));
        c.A3 = 0x0000005Du;
        c.RA = 0x80011A54u;
        GranTurismo2PC.func_8006AC90(c, m);
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
        c.RA = 0x80011AA0u;
        GranTurismo2PC.func_8007E0B0(c, m);
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
        c.RA = 0x80011B04u;
        GranTurismo2PC.func_8007DA44(c, m);
        L80011B04: ;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x40u));
        MemoryAccess.WriteU32(m, (c.T0 + 0x1C4u), c.FP);
        c.A1 = MemoryAccess.ReadU32(m, (c.FP + 0x8u));
        if (c.A1 == 0u) {
            goto L80011B2C;
        }
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0x48u));
        c.RA = 0x80011B2Cu;
        Dispatcher.Call(c, m, c.A1);
        L80011B2C: ;
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
    public static void func_80011B5C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.S0 = c.A0 + 0u;
        c.RA = 0x80011B70u;
        GranTurismo2PC.func_8007FE8C(c, m);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x106Cu;
        MemoryAccess.WriteU32(m, c.S0, c.V0);
        c.V0 = c.S0 + 0u;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80011B90(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x106Cu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU32(m, c.A0, c.V0);
        c.RA = 0x80011BA8u;
        GranTurismo2PC.func_8007FEC8(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80011BB8(CpuContext c, IMemory m)
    {
        MemoryAccess.WriteU8(m, (c.A0 + 0x20Cu), (byte)0u);
        MemoryAccess.WriteU32(m, (c.A0 + 0x1CCu), c.A1);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80011BC4(CpuContext c, IMemory m)
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
        c.RA = 0x80011BF4u;
        GranTurismo2PC.func_800809B0(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = 0x00000064u;
        MemoryAccess.WriteU8(m, (c.S1 + 0x21Cu), (byte)0u);
        c.RA = 0x80011C04u;
        GranTurismo2PC.func_8007FF70(c, m);
        c.A0 = c.S1 + 0x58u;
        c.A1 = 0x801B0000u;
        c.A1 = c.A1 - 0x6A40u;
        c.A2 = 0x00010000u;
        c.RA = 0x80011C18u;
        GranTurismo2PC.func_80080494(c, m);
        c.S0 = c.S1 + 0x38u;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x80040000u;
        c.A1 = c.A1 - 0x593Cu;
        c.A2 = 0x00010000u;
        c.A2 = c.A2 | 0x0A40u;
        c.RA = 0x80011C34u;
        GranTurismo2PC.func_80080088(c, m);
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
        c.RA = 0x80011C88u;
        GranTurismo2PC.func_80080100(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0x7Cu;
        c.RA = 0x80011C94u;
        GranTurismo2PC.func_800800B4(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0x9Cu;
        c.RA = 0x80011CA0u;
        GranTurismo2PC.func_800800B4(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0xACu;
        c.RA = 0x80011CACu;
        GranTurismo2PC.func_800800B4(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0xBCu;
        c.RA = 0x80011CB8u;
        GranTurismo2PC.func_800800B4(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0xCCu;
        c.RA = 0x80011CC4u;
        GranTurismo2PC.func_800800B4(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0x8Cu;
        c.RA = 0x80011CD0u;
        GranTurismo2PC.func_800800B4(c, m);
        c.S0 = 0x800E0000u;
        c.S0 = c.S0 + 0x15C0u;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x0000000Eu;
        MemoryAccess.WriteU8(m, (c.S1 + 0x70u), (byte)0u);
        c.RA = 0x80011CE8u;
        GranTurismo2PC.func_80011868(c, m);
        c.S2 = 0x801C0000u;
        c.S2 = c.S2 - 0x6A00u;
        c.A0 = c.S2 + 0u;
        c.A1 = 0x0000001Eu;
        c.V0 = c.V0 + 0x3u;
        c.V1 = 0xFFFFFFFCu;
        c.V0 = c.V0 & c.V1;
        c.V1 = 0x800B0000u;
        c.V0 = c.V0 + c.S0;
        MemoryAccess.WriteU32(m, (c.V1 - 0x7294u), c.V0);
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V0 - 0x7298u), c.S1);
        c.RA = 0x80011D1Cu;
        GranTurismo2PC.func_8006AC68(c, m);
        c.S4 = c.S1 + 0xDCu;
        c.A0 = c.S4 + 0u;
        c.A1 = 0u + 0u;
        c.S3 = 0x800A0000u;
        c.S3 = c.S3 + 0x6F5Cu;
        c.A2 = c.S3 + 0u;
        c.T3 = 0x801C0000u;
        c.A3 = c.T3 - 0x6A10u;
        c.V0 = 0x0000000Cu;
        c.T4 = 0x801C0000u;
        c.T0 = c.T4 - 0x69E0u;
        MemoryAccess.WriteU8(m, (c.S2 + 0x1Cu), (byte)c.S5);
        MemoryAccess.WriteU8(m, (c.A3 + 0x8u), (byte)c.V0);
        c.V0 = 0x00000007u;
        c.T5 = 0x801C0000u;
        c.T1 = c.T5 - 0x6A40u;
        c.T6 = 0x801C0000u;
        c.T2 = c.T6 - 0x6A30u;
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
        MemoryAccess.WriteU32(m, (c.T3 - 0x6A10u), c.V0);
        MemoryAccess.WriteU32(m, (c.A3 + 0x4u), c.V1);
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0xCu));
        c.V1 = MemoryAccess.ReadU32(m, (c.S0 + 0x10u));
        c.V0 = c.V0 + c.S0;
        c.V1 = c.V1 + c.S0;
        MemoryAccess.WriteU32(m, (c.T4 - 0x69E0u), c.V0);
        MemoryAccess.WriteU32(m, (c.T0 + 0x4u), c.V1);
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x14u));
        c.V1 = MemoryAccess.ReadU32(m, (c.S0 + 0x18u));
        c.V0 = c.V0 + c.S0;
        c.V1 = c.V1 + c.S0;
        MemoryAccess.WriteU32(m, (c.T5 - 0x6A40u), c.V0);
        MemoryAccess.WriteU32(m, (c.T1 + 0x4u), c.V1);
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x1Cu));
        c.V1 = MemoryAccess.ReadU32(m, (c.S0 + 0x20u));
        c.V0 = c.V0 + c.S0;
        c.V1 = c.V1 + c.S0;
        MemoryAccess.WriteU32(m, (c.T6 - 0x6A30u), c.V0);
        MemoryAccess.WriteU32(m, (c.T2 + 0x4u), c.V1);
        c.RA = 0x80011DE4u;
        GranTurismo2PC.func_8007FAB8(c, m);
        c.S0 = c.S1 + 0x140u;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x00000001u;
        c.A2 = c.S3 + 0u;
        c.RA = 0x80011DF8u;
        GranTurismo2PC.func_8007FAB8(c, m);
        c.A0 = c.S4 + 0u;
        c.RA = 0x80011E00u;
        GranTurismo2PC.func_80083A04(c, m);
        c.A0 = c.S0 + 0u;
        c.RA = 0x80011E08u;
        GranTurismo2PC.func_80083A04(c, m);
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 - 0x729Cu;
        c.RA = 0x80011E14u;
        GranTurismo2PC.func_8006EB54(c, m);
        c.A0 = 0x80030000u;
        c.A0 = c.A0 + 0x6C4u;
        c.A1 = 0x00000160u;
        c.A2 = 0u + 0u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V0 - 0x72A0u), c.A0);
        c.V0 = 0x000001E0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.V0 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V0);
        c.V0 = 0x000001E1u;
        c.A3 = 0x00000080u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.V0);
        c.RA = 0x80011E4Cu;
        GranTurismo2PC.func_8006EBF0(c, m);
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
            goto L80011E98;
        }
        c.A0 = 0u + 0u;
        c.RA = 0x80011E98u;
        Dispatcher.Call(c, m, c.V0);
        L80011E98: ;
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
    public static void func_80011EC4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.RA = 0x80011EDCu;
        GranTurismo2PC.func_80080C9C(c, m);
        c.A0 = c.S0 + 0xE8u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x214u));
        c.A1 = c.S0 + 0x1A4u;
        c.V0 = c.V0 + 0x1u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x214u), c.V0);
        c.RA = 0x80011EF4u;
        GranTurismo2PC.func_80083998(c, m);
        c.A0 = c.S0 + 0x14Cu;
        c.A1 = c.S0 + 0x1B4u;
        c.RA = 0x80011F00u;
        GranTurismo2PC.func_80083998(c, m);
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 - 0x729Cu;
        c.RA = 0x80011F0Cu;
        GranTurismo2PC.func_8006EB64(c, m);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x70u));
        if (c.V0 == 0u) {
            c.S1 = 0x00000001u;
            goto L80011F38;
        }
        c.S1 = 0x00000001u;
        c.A1 = c.S1 + 0u;
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x78u));
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x74u));
        c.A2 = c.S1 + 0u;
        c.RA = 0x80011F34u;
        Dispatcher.Call(c, m, c.V0);
        MemoryAccess.WriteU8(m, (c.S0 + 0x70u), (byte)c.V0);
        L80011F38: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x20Eu));
        c.V1 = MemoryAccess.ReadU16(m, (c.S0 + 0x20Eu));
        if ((int)c.V0 <= 0) {
            c.V0 = c.V1 - 0x1u;
            goto L80011F80;
        }
        c.V0 = c.V1 - 0x1u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x20Eu), (ushort)c.V0);
        c.V0 = c.V0 << 16;
        if ((int)c.V0 <= 0) {
            goto L80011F80;
        }
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x1C8u));
        if (c.V0 == 0u) {
            MemoryAccess.WriteU32(m, (c.S0 + 0x1C4u), c.V0);
            goto L80011F80;
        }
        MemoryAccess.WriteU32(m, (c.S0 + 0x1C4u), c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        if (c.V0 == 0u) {
            goto L80011F80;
        }
        c.A0 = 0u + 0u;
        c.RA = 0x80011F80u;
        Dispatcher.Call(c, m, c.V0);
        L80011F80: ;
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
            goto L80011FCC;
        }
        c.V1 = 0u + 0u;
        c.A0 = 0x00000001u;
        c.RA = 0x80011FC8u;
        Dispatcher.Call(c, m, c.V0);
        c.V1 = c.V0 + 0u;
        L80011FCC: ;
        c.V0 = c.V1 < 0x00000005u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80020000u;
            goto L800120BC;
        }
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x1054u;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x800120BCu: goto L800120BC;
            case 0x80011FF4u: goto L80011FF4;
            case 0x8001204Cu: goto L8001204C;
            case 0x800120B8u: goto L800120B8;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L80011FF4: ;
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
            goto L8001203C;
        }
        c.A0 = 0u + 0u;
        c.RA = 0x8001203Cu;
        Dispatcher.Call(c, m, c.V0);
        L8001203C: ;
        c.V0 = 0x00000010u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x20Eu), (ushort)c.V0);
        MemoryAccess.WriteU8(m, (c.S0 + 0x20Du), (byte)0u);
        goto L800120BC;
        L8001204C: ;
        c.A0 = c.S0 + 0u;
        c.RA = 0x80012054u;
        GranTurismo2PC.func_800122D4(c, m);
        if ((int)c.V0 < 0) {
            c.V0 = c.S1 + 0u;
            goto L800120C0;
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
            goto L800120A4;
        }
        c.A0 = 0x00000001u;
        c.RA = 0x800120A4u;
        Dispatcher.Call(c, m, c.V0);
        L800120A4: ;
        c.V0 = 0x00000010u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x20Eu), (ushort)c.V0);
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x20Du), (byte)c.V0);
        goto L800120BC;
        L800120B8: ;
        c.S1 = 0u + 0u;
        L800120BC: ;
        c.V0 = c.S1 + 0u;
        L800120C0: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800120D4(CpuContext c, IMemory m)
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
            goto L80012150;
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
            goto L8001212C;
        }
        c.V0 = c.V0 + 0xFu;
        L8001212C: ;
        c.V0 = (uint)((int)c.V0 >> 4);
        MemoryAccess.WriteU16(m, (c.A1 + 0x2u), (ushort)c.V0);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0x20Eu));
        c.V1 = c.V1 - 0x10u;
        c.V0 = c.V1 << 2;
        c.V0 = c.V0 + c.V1;
        MemoryAccess.WriteU16(m, (c.A2 + 0x2u), (ushort)c.V0);
        return;
        L80012150: ;
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
            goto L8001219C;
        }
        c.V0 = c.V0 + 0xFu;
        L8001219C: ;
        c.V0 = (uint)((int)c.V0 >> 4);
        MemoryAccess.WriteU16(m, (c.A1 + 0x2u), (ushort)c.V0);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800121A8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x38u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S0);
        c.RA = 0x800121C8u;
        GranTurismo2PC.func_80080D04(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.SP + 0x10u;
        c.S2 = c.SP + 0x18u;
        c.A2 = c.S2 + 0u;
        c.RA = 0x800121DCu;
        GranTurismo2PC.func_800120D4(c, m);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x20Eu));
        c.A3 = c.V0 << 7;
        if ((int)c.A3 >= 0) {
            goto L800121F4;
        }
        c.A3 = c.A3 + 0xFu;
        L800121F4: ;
        if ((int)c.V0 <= 0) {
            c.S3 = (uint)((int)c.A3 >> 4);
            goto L80012220;
        }
        c.S3 = (uint)((int)c.A3 >> 4);
        c.A0 = c.S1 + 0u;
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x1C8u));
        c.S0 = c.S1 + 0xACu;
        c.A2 = c.S0 + 0u;
        c.A3 = c.S3 + 0u;
        c.RA = 0x80012214u;
        GranTurismo2PC.func_8001191C(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S2 + 0u;
        c.RA = 0x80012220u;
        GranTurismo2PC.func_8008034C(c, m);
        L80012220: ;
        c.A0 = c.S1 + 0u;
        c.S0 = c.S1 + 0x9Cu;
        c.A2 = c.S0 + 0u;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0x20Cu));
        c.A3 = 0x00000080u;
        c.V0 = c.V0 << 2;
        c.V0 = c.S1 + c.V0;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1CCu));
        c.A3 = c.A3 - c.S3;
        c.RA = 0x80012248u;
        GranTurismo2PC.func_8001191C(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.SP + 0x10u;
        c.RA = 0x80012254u;
        GranTurismo2PC.func_8008034C(c, m);
        c.A0 = MemoryAccess.ReadU32(m, (c.S1 + 0x94u));
        c.A1 = 0u + 0u;
        c.RA = 0x80012260u;
        GranTurismo2PC.func_8007D024(c, m);
        c.V1 = 0x00000160u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x4u), (ushort)c.V1);
        c.V1 = 0x801F0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V1 + 0x698u));
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
    public static void func_800122A0(CpuContext c, IMemory m)
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
    public static void func_800122D4(CpuContext c, IMemory m)
    {
        c.A1 = c.A0 + 0u;
        c.V1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.A1 + 0x20Cu));
        if ((int)c.V1 <= 0) {
            c.V1 = c.V1 << 2;
            goto L8001230C;
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
        L8001230C: ;
        c.V0 = 0xFFFFFFFFu;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012314(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.S0 = c.A0 + 0u;
        c.RA = 0x80012328u;
        GranTurismo2PC.func_8007FEF0(c, m);
        c.A0 = c.S0 + 0xDCu;
        c.RA = 0x80012330u;
        GranTurismo2PC.func_8007FB38(c, m);
        c.A0 = c.S0 + 0x140u;
        c.RA = 0x80012338u;
        GranTurismo2PC.func_8007FB38(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012348(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.S0 = c.A0 + 0u;
        c.RA = 0x8001235Cu;
        GranTurismo2PC.func_80080C94(c, m);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x70u));
        if (c.V0 == 0u) {
            c.A1 = 0x00000001u;
            goto L80012384;
        }
        c.A1 = 0x00000001u;
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x78u));
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x74u));
        c.A2 = 0u + 0u;
        c.RA = 0x80012380u;
        Dispatcher.Call(c, m, c.V0);
        MemoryAccess.WriteU8(m, (c.S0 + 0x70u), (byte)c.V0);
        L80012384: ;
        c.V0 = 0x800B0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 - 0x72A0u));
        c.RA = 0x80012394u;
        GranTurismo2PC.func_8006ECAC(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800123A4(CpuContext c, IMemory m)
    {
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.A0 + 0x70u));
        if (c.V0 != 0u) {
            c.V0 = 0x00000001u;
            goto L800123C8;
        }
        c.V0 = 0x00000001u;
        c.V1 = c.V0 + 0u;
        MemoryAccess.WriteU8(m, (c.A0 + 0x70u), (byte)c.V1);
        MemoryAccess.WriteU32(m, (c.A0 + 0x74u), c.A1);
        MemoryAccess.WriteU32(m, (c.A0 + 0x78u), c.A2);
        return;
        L800123C8: ;
        c.V0 = 0u + 0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800123D0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x70u));
        if (c.V0 == 0u) {
            c.A1 = 0u + 0u;
            goto L80012404;
        }
        c.A1 = 0u + 0u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x74u));
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x78u));
        c.A2 = 0x00000002u;
        c.RA = 0x80012400u;
        Dispatcher.Call(c, m, c.V0);
        MemoryAccess.WriteU8(m, (c.S0 + 0x70u), (byte)0u);
        L80012404: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012414(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A1 = 0x00000001u;
        c.RA = 0x80012424u;
        GranTurismo2PC.func_80080F24(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012434(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.RA = 0x80012444u;
        GranTurismo2PC.func_8007C570(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012454(CpuContext c, IMemory m)
    {
        c.V0 = 0x801C0000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 - 0x6A20u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.V0 - 0x6A20u;
        if (c.V1 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
            goto L80012490;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.RA = 0x80012478u;
        GranTurismo2PC.func_80080038(c, m);
        c.A1 = 0x80020000u;
        c.A2 = 0x801F0000u;
        c.A0 = c.S0 + 0u;
        c.A1 = c.A1 + 0x10B0u;
        c.A2 = c.A2 + 0xCE0u;
        c.RA = 0x80012490u;
        GranTurismo2PC.func_80085B3C(c, m);
        L80012490: ;
        c.V0 = c.S0 + 0u;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800124A4(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1C4u));
        c.V0 = 0x00000014u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
        if (c.A0 != 0u) {
            MemoryAccess.WriteU32(m, (c.V1 + 0x18u), c.A0);
            goto L800124D8;
        }
        MemoryAccess.WriteU32(m, (c.V1 + 0x18u), c.A0);
        c.V0 = 0x800B0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7294u));
        c.RA = 0x800124D8u;
        GranTurismo2PC.func_80020DCC(c, m);
        L800124D8: ;
        c.A0 = 0u + 0u;
        c.RA = 0x800124E0u;
        GranTurismo2PC.func_80012414(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800124F0(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V1 = MemoryAccess.ReadU32(m, (c.A0 + 0x1C4u));
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x14u));
        if ((int)c.V0 <= 0) {
            c.A1 = 0u + 0u;
            goto L80012544;
        }
        c.A1 = 0u + 0u;
        c.V0 = c.V0 - 0x1u;
        if (c.V0 != 0u) {
            MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
            goto L80012544;
        }
        MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 + 0x18u));
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            c.A1 = 0x00000004u;
            goto L80012544;
        }
        c.A1 = 0x00000004u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x4C8Cu;
        c.RA = 0x80012540u;
        GranTurismo2PC.func_800122A0(c, m);
        c.A1 = 0x00000001u;
        L80012544: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = c.A1 + 0u;
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012554(CpuContext c, IMemory m)
    {
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001255C(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1C4u));
        c.V0 = 0x00000014u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
        c.RA = 0x8001257Cu;
        GranTurismo2PC.func_80012434(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001258C(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1C4u));
        c.V1 = MemoryAccess.ReadU32(m, (c.A0 + 0x14u));
        if ((int)c.V1 <= 0) {
            c.V0 = 0u + 0u;
            goto L800125C0;
        }
        c.V0 = 0u + 0u;
        c.V0 = c.V1 - 0x1u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.V0);
        c.V0 = c.V0 < 0x00000001u ? 1u : 0u;
        c.V0 = c.V0 << 2;
        L800125C0: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800125C8(CpuContext c, IMemory m)
    {
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800125D0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A2 + 0u;
        c.V0 = c.S1 << 2;
        c.V0 = c.V0 + c.S1;
        c.V0 = c.V0 << 2;
        c.V1 = 0x800B0000u;
        c.V1 = c.V1 - 0x7290u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.V0 + c.V1;
        c.V0 = c.A0 < 0x00000009u ? 1u : 0u;
        if (c.V0 == 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
            goto L800126A4;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x10BCu;
        c.V1 = c.A0 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x80012624u: goto L80012624;
            case 0x80012654u: goto L80012654;
            case 0x8001265Cu: goto L8001265C;
            case 0x80012664u: goto L80012664;
            case 0x80012674u: goto L80012674;
            case 0x800126A4u: goto L800126A4;
            case 0x8001269Cu: goto L8001269C;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L80012624: ;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x4E98u;
        c.RA = 0x80012634u;
        GranTurismo2PC.func_80016394(c, m);
        c.V0 = c.S1 << 1;
        c.V0 = c.V0 + c.S1;
        c.V0 = c.V0 << 2;
        c.V1 = 0x80050000u;
        c.V1 = c.V1 - 0x4EFCu;
        c.V0 = c.V0 + c.V1;
        MemoryAccess.WriteU32(m, (c.S0 + 0xCu), c.V0);
        goto L800126A4;
        L80012654: ;
        MemoryAccess.WriteU16(m, (c.S0 + 0x10u), (ushort)0u);
        goto L800126A4;
        L8001265C: ;
        c.V0 = 0xFFFFFFF3u;
        goto L800126A0;
        L80012664: ;
        c.A0 = c.S0 + 0u;
        c.RA = 0x8001266Cu;
        GranTurismo2PC.func_800163C8(c, m);
        goto L800126A4;
        L80012674: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0xCu));
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0xEu));
        c.A0 = c.S0 + 0u;
        MemoryAccess.WriteU16(m, (c.A0 + 0x4u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.A0 + 0x6u), (ushort)c.V1);
        c.A1 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.A2 = 0u + 0u;
        c.RA = 0x80012694u;
        GranTurismo2PC.func_80016410(c, m);
        goto L800126A4;
        L8001269C: ;
        c.V0 = 0x0000000Cu;
        L800126A0: ;
        MemoryAccess.WriteU16(m, (c.S0 + 0x10u), (ushort)c.V0);
        L800126A4: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = 0x00000001u;
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800126BC_gt2_overlay_1(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        c.V0 = 0x800B0000u;
        c.A1 = 0x80010000u;
        c.A1 = c.A1 + 0x25D0u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
        c.A2 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S2 = MemoryAccess.ReadU32(m, (c.V0 + 0x1C4u));
        c.V0 = 0x00000014u;
        MemoryAccess.WriteU32(m, (c.S2 + 0x14u), c.V0);
        c.V0 = 0x80050000u;
        c.S1 = c.V0 - 0x4E94u;
        c.A0 = c.S1 + 0u;
        c.RA = 0x80012704u;
        GranTurismo2PC.func_8006CDCC(c, m);
        if (c.S0 == 0u) {
            MemoryAccess.WriteU16(m, (c.S1 + 0x6u), (ushort)0u);
            goto L80012718;
        }
        MemoryAccess.WriteU16(m, (c.S1 + 0x6u), (ushort)0u);
        c.V0 = MemoryAccess.ReadU32(m, (c.S2 + 0x18u));
        MemoryAccess.WriteU16(m, (c.S1 + 0x6u), (ushort)c.V0);
        L80012718: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012730(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
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
            goto L80012768;
        }
        c.S3 = c.V0 + 0x1A4u;
        c.S3 = c.S2 + 0u;
        L80012768: ;
        c.S0 = MemoryAccess.ReadU32(m, (c.S1 + 0x14u));
        if ((int)c.S0 <= 0) {
            goto L8001278C;
        }
        c.S0 = c.S0 - 0x1u;
        if (c.S0 != 0u) {
            c.A0 = 0x80050000u;
            goto L8001278C;
        }
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4E94u;
        c.RA = 0x8001278Cu;
        GranTurismo2PC.func_8006CE70(c, m);
        L8001278C: ;
        MemoryAccess.WriteU32(m, (c.S1 + 0x14u), c.S0);
        c.V0 = 0x80050000u;
        c.S4 = c.V0 - 0x4E94u;
        c.A0 = c.S4 + 0u;
        c.A1 = c.S3 + 0u;
        c.RA = 0x800127A4u;
        GranTurismo2PC.func_8006CFC4(c, m);
        c.S0 = c.V0 + 0u;
        c.V0 = 0xFFFFFFFDu;
        if (c.S0 == c.V0) {
            c.V0 = (int)c.S0 < -2 ? 1u : 0u;
            goto L800127E8;
        }
        c.V0 = (int)c.S0 < -2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0xFFFFFFFCu;
            goto L800127CC;
        }
        c.V0 = 0xFFFFFFFCu;
        if (c.S0 == c.V0) {
            c.V0 = c.S2 + 0u;
            goto L80012850;
        }
        c.V0 = c.S2 + 0u;
        goto L80012810;
        L800127CC: ;
        c.V0 = 0xFFFFFFFEu;
        if (c.S0 == c.V0) {
            c.V0 = 0xFFFFFFFFu;
            goto L8001284C;
        }
        c.V0 = 0xFFFFFFFFu;
        if (c.S0 == c.V0) {
            goto L800127F8;
        }
        goto L80012810;
        L800127E8: ;
        c.A0 = 0x00000005u;
        c.RA = 0x800127F0u;
        GranTurismo2PC.func_80060840(c, m);
        c.V0 = c.S2 + 0u;
        goto L80012850;
        L800127F8: ;
        c.A0 = 0x00000004u;
        c.RA = 0x80012800u;
        GranTurismo2PC.func_80060840(c, m);
        c.A0 = c.S4 + 0u;
        c.RA = 0x80012808u;
        GranTurismo2PC.func_8006CED8(c, m);
        c.S2 = 0x00000002u;
        goto L8001284C;
        L80012810: ;
        c.A0 = 0x00000003u;
        c.RA = 0x80012818u;
        GranTurismo2PC.func_80060840(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4E94u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x18u), c.S0);
        c.RA = 0x80012828u;
        GranTurismo2PC.func_8006CED8(c, m);
        c.V0 = 0x800B0000u;
        c.V1 = 0x80050000u;
        c.V1 = c.V1 - 0x4EA8u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
        c.V0 = c.S0 << 2;
        c.V0 = c.V0 + c.V1;
        c.A1 = MemoryAccess.ReadU32(m, c.V0);
        c.S2 = 0x00000001u;
        c.RA = 0x8001284Cu;
        GranTurismo2PC.func_800122A0(c, m);
        L8001284C: ;
        c.V0 = c.S2 + 0u;
        L80012850: ;
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
    public static void func_80012870(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4E94u;
        c.A1 = c.A1 + 0x10u;
        c.RA = 0x8001288Cu;
        GranTurismo2PC.func_8006D50C(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001289C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.A2 = 0x80050000u;
        c.A0 = c.A2 - 0x4E60u;
        c.V0 = 0x800B0000u;
        c.V0 = c.V0 - 0x729Cu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU32(m, (c.A0 + 0x8u), c.V0);
        c.V0 = 0x800B0000u;
        c.A1 = 0x00040000u;
        c.V1 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7294u));
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 - 0x72A0u));
        c.V0 = c.V0 + c.A1;
        MemoryAccess.WriteU32(m, (c.A2 - 0x4E60u), c.V0);
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.V1);
        c.RA = 0x800128DCu;
        GranTurismo2PC.func_8007284C(c, m);
        c.V1 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V1 - 0x7240u), c.V0);
        c.RA = 0x800128E8u;
        GranTurismo2PC.func_80072EEC(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800128F8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 - 0x7298u));
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        if (c.A0 != 0u) {
            c.V0 = c.V0 + 0x1A4u;
            goto L80012920;
        }
        c.V0 = c.V0 + 0x1A4u;
        c.V0 = c.S0 + 0u;
        L80012920: ;
        c.A0 = c.V0 + 0u;
        c.RA = 0x80012928u;
        GranTurismo2PC.func_800728F0(c, m);
        c.V1 = c.V0 + 0u;
        c.A1 = 0x00000001u;
        if (c.V1 == c.A1) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L80012954;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = c.S0 + 0u;
            goto L8001298C;
        }
        c.V0 = c.S0 + 0u;
        c.V0 = 0x00000002u;
        if (c.V1 == c.V0) {
            c.V0 = c.S0 + 0u;
            goto L80012964;
        }
        c.V0 = c.S0 + 0u;
        goto L8001298C;
        L80012954: ;
        c.A0 = 0x00000004u;
        c.RA = 0x8001295Cu;
        GranTurismo2PC.func_80060840(c, m);
        c.S0 = 0x00000002u;
        goto L80012988;
        L80012964: ;
        c.A0 = 0x00000003u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 - 0x7298u));
        c.S0 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.V0 + 0x21Cu), (byte)c.A1);
        c.RA = 0x80012978u;
        GranTurismo2PC.func_80060840(c, m);
        c.A1 = 0x80050000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.S1 - 0x7298u));
        c.A1 = c.A1 - 0x4CE0u;
        c.RA = 0x80012988u;
        GranTurismo2PC.func_800122A0(c, m);
        L80012988: ;
        c.V0 = c.S0 + 0u;
        L8001298C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800129A0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.A0 = c.A0 + 0x10u;
        c.RA = 0x800129B4u;
        GranTurismo2PC.func_80072B78(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800129C4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.A2 = 0x80050000u;
        c.A0 = c.A2 - 0x4E60u;
        c.V0 = 0x800B0000u;
        c.V0 = c.V0 - 0x729Cu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU32(m, (c.A0 + 0x8u), c.V0);
        c.V0 = 0x800B0000u;
        c.A1 = 0x00040000u;
        c.V1 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7294u));
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 - 0x72A0u));
        c.V0 = c.V0 + c.A1;
        MemoryAccess.WriteU32(m, (c.A2 - 0x4E60u), c.V0);
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.V1);
        c.RA = 0x80012A04u;
        GranTurismo2PC.func_8007284C(c, m);
        c.V1 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V1 - 0x723Cu), c.V0);
        c.RA = 0x80012A10u;
        GranTurismo2PC.func_80072F20(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012A20(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        if (c.A0 != 0u) {
            c.V0 = c.V0 + 0x1A4u;
            goto L80012A44;
        }
        c.V0 = c.V0 + 0x1A4u;
        c.V0 = c.S0 + 0u;
        L80012A44: ;
        c.A0 = c.V0 + 0u;
        c.RA = 0x80012A4Cu;
        GranTurismo2PC.func_800728F0(c, m);
        c.V1 = c.V0 + 0u;
        if (c.V1 == 0u) {
            c.V0 = 0x00000001u;
            goto L80012A6C;
        }
        c.V0 = 0x00000001u;
        if (c.V1 != c.V0) {
            c.V0 = c.S0 + 0u;
            goto L80012A70;
        }
        c.V0 = c.S0 + 0u;
        c.A0 = 0x00000004u;
        c.RA = 0x80012A68u;
        GranTurismo2PC.func_80060840(c, m);
        c.S0 = 0x00000002u;
        L80012A6C: ;
        c.V0 = c.S0 + 0u;
        L80012A70: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012A80(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.A0 = c.A0 + 0x10u;
        c.RA = 0x80012A94u;
        GranTurismo2PC.func_80072B78(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012AA4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.A2 = 0x80050000u;
        c.A0 = c.A2 - 0x4E44u;
        c.V0 = 0x800B0000u;
        c.V0 = c.V0 - 0x729Cu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU32(m, (c.A0 + 0x8u), c.V0);
        c.V0 = 0x800B0000u;
        c.A1 = 0x00040000u;
        c.V1 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7294u));
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 - 0x72A0u));
        c.V0 = c.V0 + c.A1;
        MemoryAccess.WriteU32(m, (c.A2 - 0x4E44u), c.V0);
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.V1);
        c.RA = 0x80012AE4u;
        GranTurismo2PC.func_80015CF8(c, m);
        c.V1 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V1 - 0x7238u), c.V0);
        c.RA = 0x80012AF0u;
        GranTurismo2PC.func_80015D98(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012B00(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        if (c.A0 != 0u) {
            c.V0 = c.V0 + 0x1A4u;
            goto L80012B24;
        }
        c.V0 = c.V0 + 0x1A4u;
        c.V0 = c.S0 + 0u;
        L80012B24: ;
        c.A0 = c.V0 + 0u;
        c.RA = 0x80012B2Cu;
        GranTurismo2PC.func_80015DC0(c, m);
        c.V1 = c.V0 + 0u;
        if (c.V1 == 0u) {
            c.V0 = 0x00000001u;
            goto L80012B4C;
        }
        c.V0 = 0x00000001u;
        if (c.V1 != c.V0) {
            c.V0 = c.S0 + 0u;
            goto L80012B50;
        }
        c.V0 = c.S0 + 0u;
        c.A0 = 0x00000004u;
        c.RA = 0x80012B48u;
        GranTurismo2PC.func_80060840(c, m);
        c.S0 = 0x00000002u;
        L80012B4C: ;
        c.V0 = c.S0 + 0u;
        L80012B50: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012B60(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.A0 = c.A0 + 0x10u;
        c.RA = 0x80012B74u;
        GranTurismo2PC.func_80015F4C(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012B84(CpuContext c, IMemory m)
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
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 - 0x7294u));
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
        c.RA = 0x80012BF8u;
        GranTurismo2PC.func_8006AC68(c, m);
        c.S7 = 0x00000001u;
        c.A2 = c.SP + 0x20u;
        c.V0 = 0x00000004u;
        if (c.S0 != c.V0) {
            MemoryAccess.WriteU8(m, (c.A2 + 0x1Cu), (byte)c.S7);
            goto L80012CB4;
        }
        MemoryAccess.WriteU8(m, (c.A2 + 0x1Cu), (byte)c.S7);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x26u));
        c.V0 = ~(0u | c.V1);
        c.V0 = c.V0 >> 31;
        c.A0 = (int)c.V1 < -1 ? 1u : 0u;
        c.V0 = c.V0 | c.A0;
        if (c.V0 == 0u) {
            c.V0 = c.S7 + 0u;
            goto L80012CB8;
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
            goto L80012CB4;
        }
        c.V0 = 0x00000080u;
        c.V1 = c.V0 - c.V1;
        if ((int)c.V1 >= 0) {
            c.V0 = c.V1 << 1;
            goto L80012C70;
        }
        c.V0 = c.V1 << 1;
        c.V1 = 0u + 0u;
        c.V0 = c.V1 << 1;
        L80012C70: ;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 - c.V1;
        c.V0 = c.V0 << 4;
        if (c.A0 == 0u) {
            c.V1 = (uint)((int)c.V0 >> 7);
            goto L80012C8C;
        }
        c.V1 = (uint)((int)c.V0 >> 7);
        c.V1 = 0u + 0u;
        L80012C8C: ;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4DF4u;
        c.A1 = c.S6 + 0u;
        c.V0 = c.S3 + c.V1;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.A3);
        c.A3 = c.S5 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), 0u);
        c.RA = 0x80012CB4u;
        GranTurismo2PC.func_8006A4E4(c, m);
        L80012CB4: ;
        c.V0 = c.S7 + 0u;
        L80012CB8: ;
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
    public static void func_80012CE4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        if (c.A0 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
            goto L80012D24;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = 0x800B0000u;
        c.V0 = 0x800B0000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 - 0x7294u));
        c.V0 = 0x00000018u;
        MemoryAccess.WriteU16(m, (c.A0 - 0x7230u), (ushort)c.V0);
        c.A0 = 0x80050000u;
        c.A1 = 0x80010000u;
        c.A1 = c.A1 + 0x2B84u;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V1 + 0x200u));
        c.A2 = 0u + 0u;
        MemoryAccess.WriteU16(m, (c.A0 - 0x4E28u), (ushort)c.V0);
        c.A0 = c.A0 - 0x4E28u;
        c.RA = 0x80012D24u;
        GranTurismo2PC.func_8006CDCC(c, m);
        L80012D24: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012D34(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A0 + 0u;
        c.V0 = 0x800B0000u;
        c.A0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
        c.V1 = MemoryAccess.ReadU16(m, (c.A0 - 0x7230u));
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.V0 + 0x1A4u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 - 0x7230u));
        c.S1 = 0u + 0u;
        if ((int)c.V0 <= 0) {
            MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
            goto L80012D94;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        c.V0 = c.V1 - 0x1u;
        MemoryAccess.WriteU16(m, (c.A0 - 0x7230u), (ushort)c.V0);
        c.V0 = c.V0 << 16;
        if (c.V0 != 0u) {
            goto L80012D94;
        }
        c.A0 = 0x00000007u;
        c.RA = 0x80012D88u;
        GranTurismo2PC.func_80060840(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4E28u;
        c.RA = 0x80012D94u;
        GranTurismo2PC.func_8006CE70(c, m);
        L80012D94: ;
        if (c.S2 != 0u) {
            c.V0 = 0x80050000u;
            goto L80012DA0;
        }
        c.V0 = 0x80050000u;
        c.S0 = 0u + 0u;
        L80012DA0: ;
        c.S2 = c.V0 - 0x4E28u;
        c.A0 = c.S2 + 0u;
        c.A1 = c.S0 + 0u;
        c.RA = 0x80012DB0u;
        GranTurismo2PC.func_8006CFC4(c, m);
        c.S0 = c.V0 + 0u;
        c.V0 = 0xFFFFFFFDu;
        if (c.S0 == c.V0) {
            c.V0 = (int)c.S0 < -2 ? 1u : 0u;
            goto L80012E04;
        }
        c.V0 = (int)c.S0 < -2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0xFFFFFFFCu;
            goto L80012DD8;
        }
        c.V0 = 0xFFFFFFFCu;
        if (c.S0 == c.V0) {
            goto L80012E14;
        }
        goto L80012E24;
        L80012DD8: ;
        c.V0 = 0xFFFFFFFEu;
        if (c.S0 == c.V0) {
            c.V0 = 0xFFFFFFFFu;
            goto L80012E58;
        }
        c.V0 = 0xFFFFFFFFu;
        if (c.S0 != c.V0) {
            goto L80012E24;
        }
        c.A0 = 0x00000004u;
        c.RA = 0x80012DF4u;
        GranTurismo2PC.func_80060840(c, m);
        c.A0 = c.S2 + 0u;
        c.RA = 0x80012DFCu;
        GranTurismo2PC.func_8006CED8(c, m);
        c.S1 = 0x00000002u;
        goto L80012E58;
        L80012E04: ;
        c.A0 = 0x00000006u;
        c.RA = 0x80012E0Cu;
        GranTurismo2PC.func_80060840(c, m);
        c.V0 = c.S1 + 0u;
        goto L80012E5C;
        L80012E14: ;
        c.A0 = 0u + 0u;
        c.RA = 0x80012E1Cu;
        GranTurismo2PC.func_80060840(c, m);
        c.V0 = c.S1 + 0u;
        goto L80012E5C;
        L80012E24: ;
        c.A0 = 0x00000003u;
        c.RA = 0x80012E2Cu;
        GranTurismo2PC.func_80060840(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4E28u;
        c.RA = 0x80012E38u;
        GranTurismo2PC.func_8006CED8(c, m);
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x4D88u;
        c.S1 = 0x00000001u;
        c.V0 = 0x800B0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V0 - 0x7234u), c.S0);
        c.RA = 0x80012E58u;
        GranTurismo2PC.func_800122A0(c, m);
        L80012E58: ;
        c.V0 = c.S1 + 0u;
        L80012E5C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012E74(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4E28u;
        c.A1 = c.A1 + 0x10u;
        c.RA = 0x80012E90u;
        GranTurismo2PC.func_8006D50C(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012EA0(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
        c.V1 = MemoryAccess.ReadU32(m, (c.A0 + 0x1C4u));
        c.V0 = 0x00000014u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.A0 + 0x21Cu), (byte)c.V0);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012EC4(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1C4u));
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x14u));
        if ((int)c.V0 <= 0) {
            c.A0 = 0u + 0u;
            goto L80012F18;
        }
        c.A0 = 0u + 0u;
        c.V0 = c.V0 - 0x1u;
        if (c.V0 != 0u) {
            MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
            goto L80012F18;
        }
        MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
        c.V0 = 0x800B0000u;
        c.A1 = 0x00020000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7294u));
        c.V0 = 0x800B0000u;
        c.A2 = MemoryAccess.ReadU32(m, (c.V0 - 0x7234u));
        c.A1 = c.A0 + c.A1;
        c.RA = 0x80012F14u;
        GranTurismo2PC.func_80020E14(c, m);
        c.A0 = 0x00000004u;
        L80012F18: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = c.A0 + 0u;
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012F28(CpuContext c, IMemory m)
    {
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012F30(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1C4u));
        c.V0 = 0x00000014u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x18u), c.A0);
        c.A0 = 0x00000007u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
        c.RA = 0x80012F58u;
        GranTurismo2PC.func_80012414(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012F68(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V1 = MemoryAccess.ReadU32(m, (c.A0 + 0x1C4u));
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x14u));
        if ((int)c.V0 <= 0) {
            c.A1 = 0u + 0u;
            goto L80012FBC;
        }
        c.A1 = 0u + 0u;
        c.V0 = c.V0 - 0x1u;
        if (c.V0 != 0u) {
            MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
            goto L80012FBC;
        }
        MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 + 0x18u));
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            c.A1 = 0x00000004u;
            goto L80012FBC;
        }
        c.A1 = 0x00000004u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x4AE8u;
        c.RA = 0x80012FB8u;
        GranTurismo2PC.func_800122A0(c, m);
        c.A1 = 0x00000001u;
        L80012FBC: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = c.A1 + 0u;
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012FCC(CpuContext c, IMemory m)
    {
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012FD4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.A2 = 0x80050000u;
        c.A0 = c.A2 - 0x4E60u;
        c.V0 = 0x800B0000u;
        c.V0 = c.V0 - 0x729Cu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU32(m, (c.A0 + 0x8u), c.V0);
        c.V0 = 0x800B0000u;
        c.A1 = 0x00040000u;
        c.V1 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7294u));
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 - 0x72A0u));
        c.V0 = c.V0 + c.A1;
        MemoryAccess.WriteU32(m, (c.A2 - 0x4E60u), c.V0);
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.V1);
        c.RA = 0x80013014u;
        GranTurismo2PC.func_8007284C(c, m);
        c.A0 = 0u + 0u;
        c.RA = 0x8001301Cu;
        GranTurismo2PC.func_80072F9C(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001302C(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        if (c.A0 != 0u) {
            c.V0 = c.V0 + 0x1A4u;
            goto L80013050;
        }
        c.V0 = c.V0 + 0x1A4u;
        c.V0 = c.S0 + 0u;
        L80013050: ;
        c.A0 = c.V0 + 0u;
        c.RA = 0x80013058u;
        GranTurismo2PC.func_800728F0(c, m);
        c.V1 = c.V0 + 0u;
        if (c.V1 == 0u) {
            c.V0 = 0x00000001u;
            goto L80013078;
        }
        c.V0 = 0x00000001u;
        if (c.V1 != c.V0) {
            c.V0 = c.S0 + 0u;
            goto L8001307C;
        }
        c.V0 = c.S0 + 0u;
        c.A0 = 0x00000004u;
        c.RA = 0x80013074u;
        GranTurismo2PC.func_80060840(c, m);
        c.S0 = 0x00000002u;
        L80013078: ;
        c.V0 = c.S0 + 0u;
        L8001307C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001308C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.A0 = c.A0 + 0x10u;
        c.RA = 0x800130A0u;
        GranTurismo2PC.func_80072B78(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800130B0(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1C4u));
        c.V0 = 0x00000014u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x18u), c.A0);
        c.A0 = 0x00000007u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
        c.RA = 0x800130D8u;
        GranTurismo2PC.func_80012414(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800130E8(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V1 = MemoryAccess.ReadU32(m, (c.A0 + 0x1C4u));
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x14u));
        if ((int)c.V0 <= 0) {
            c.A1 = 0u + 0u;
            goto L8001313C;
        }
        c.A1 = 0u + 0u;
        c.V0 = c.V0 - 0x1u;
        if (c.V0 != 0u) {
            MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
            goto L8001313C;
        }
        MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 + 0x18u));
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            c.A1 = 0x00000004u;
            goto L8001313C;
        }
        c.A1 = 0x00000004u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x4A40u;
        c.RA = 0x80013138u;
        GranTurismo2PC.func_800122A0(c, m);
        c.A1 = 0x00000001u;
        L8001313C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = c.A1 + 0u;
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001314C(CpuContext c, IMemory m)
    {
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80013154(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.A2 = 0x80050000u;
        c.A0 = c.A2 - 0x4E60u;
        c.V0 = 0x800B0000u;
        c.V0 = c.V0 - 0x729Cu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU32(m, (c.A0 + 0x8u), c.V0);
        c.V0 = 0x800B0000u;
        c.A1 = 0x00040000u;
        c.V1 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7294u));
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 - 0x72A0u));
        c.V0 = c.V0 + c.A1;
        MemoryAccess.WriteU32(m, (c.A2 - 0x4E60u), c.V0);
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.V1);
        c.RA = 0x80013194u;
        GranTurismo2PC.func_8007284C(c, m);
        c.A0 = 0u + 0u;
        c.RA = 0x8001319Cu;
        GranTurismo2PC.func_80073010(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800131AC_gt2_overlay_1(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        if (c.A0 != 0u) {
            c.V0 = c.V0 + 0x1A4u;
            goto L800131D0;
        }
        c.V0 = c.V0 + 0x1A4u;
        c.V0 = c.S0 + 0u;
        L800131D0: ;
        c.A0 = c.V0 + 0u;
        c.RA = 0x800131D8u;
        GranTurismo2PC.func_800728F0(c, m);
        c.V1 = c.V0 + 0u;
        if (c.V1 == 0u) {
            c.V0 = 0x00000001u;
            goto L800131F8;
        }
        c.V0 = 0x00000001u;
        if (c.V1 != c.V0) {
            c.V0 = c.S0 + 0u;
            goto L800131FC;
        }
        c.V0 = c.S0 + 0u;
        c.A0 = 0x00000004u;
        c.RA = 0x800131F4u;
        GranTurismo2PC.func_80060840(c, m);
        c.S0 = 0x00000002u;
        L800131F8: ;
        c.V0 = c.S0 + 0u;
        L800131FC: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001320C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.A0 = c.A0 + 0x10u;
        c.RA = 0x80013220u;
        GranTurismo2PC.func_80072B78(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80013230(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V1 = 0x801D0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
        c.V1 = MemoryAccess.ReadU8(m, (c.V1 - 0x6720u));
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S2 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        if (c.V1 != 0u) {
            goto L800132B4;
        }
        c.A0 = c.S2 + 0u;
        c.RA = 0x80013268u;
        GranTurismo2PC.func_8007F174(c, m);
        c.A0 = c.S2 + 0u;
        c.S1 = 0x801C0000u;
        c.S1 = c.S1 - 0x5BF8u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x8001327Cu;
        GranTurismo2PC.func_8006ECD8(c, m);
        c.A0 = c.S2 + 0u;
        c.S0 = 0x801F0000u;
        c.S0 = c.S0 - 0x29Fu;
        c.A1 = c.S0 + 0u;
        c.RA = 0x80013290u;
        GranTurismo2PC.func_8006ECD8(c, m);
        c.A0 = c.S2 + 0u;
        c.A1 = c.S0 - 0x11u;
        c.RA = 0x8001329Cu;
        GranTurismo2PC.func_8006ECD8(c, m);
        c.A0 = c.S2 + 0u;
        c.A1 = c.S1 + 0xFu;
        c.RA = 0x800132A8u;
        GranTurismo2PC.func_8006ECD8(c, m);
        c.A0 = c.S2 + 0u;
        c.A1 = c.S1 + 0x24u;
        c.RA = 0x800132B4u;
        GranTurismo2PC.func_8006ECD8(c, m);
        L800132B4: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800132CC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V0 = 0x800B0000u;
        c.V1 = 0x801D0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
        c.V1 = MemoryAccess.ReadU8(m, (c.V1 - 0x6720u));
        c.A1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        if (c.V1 != 0u) {
            goto L800132FC;
        }
        c.RA = 0x800132FCu;
        GranTurismo2PC.func_8006ECD8(c, m);
        L800132FC: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001330C(CpuContext c, IMemory m)
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
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
        c.V1 = MemoryAccess.ReadU8(m, (c.V1 - 0x6720u));
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S4 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        if (c.V1 != 0u) {
            c.A0 = 0u + 0u;
            goto L80013408;
        }
        c.A0 = 0u + 0u;
        c.V0 = MemoryAccess.ReadU8(m, c.A1);
        if (c.V0 == 0u) {
            c.V1 = c.A1 + 0u;
            goto L80013380;
        }
        c.V1 = c.A1 + 0u;
        L8001336C: ;
        c.V1 = c.V1 + 0x2u;
        c.V0 = MemoryAccess.ReadU8(m, c.V1);
        if (c.V0 != 0u) {
            c.A0 = c.A0 + 0xCu;
            goto L8001336C;
        }
        c.A0 = c.A0 + 0xCu;
        L80013380: ;
        c.V0 = (uint)((int)c.A0 >> 1);
        c.S1 = c.S1 - c.V0;
        c.S0 = c.A1 + 0u;
        c.S2 = c.S3 + 0x1Fu;
        L80013390: ;
        c.V0 = MemoryAccess.ReadU8(m, c.S0);
        if (c.V0 == 0u) {
            c.A0 = c.S4 + 0u;
            goto L80013408;
        }
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.V0 = MemoryAccess.ReadU8(m, (c.S0 + 0x1u));
        c.A1 = c.A1 << 8;
        c.A1 = c.A1 | c.V0;
        c.RA = 0x800133B4u;
        GranTurismo2PC.func_8007F18C(c, m);
        if (c.V0 == 0u) {
            c.A0 = c.V0 + 0u;
            goto L800133FC;
        }
        c.A0 = c.V0 + 0u;
        c.A1 = c.S6 + 0u;
        c.A2 = c.S5 + 0u;
        c.RA = 0x800133C8u;
        GranTurismo2PC.func_8007F01C(c, m);
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
        L800133FC: ;
        c.S1 = c.S1 + 0xCu;
        c.S0 = c.S0 + 0x2u;
        goto L80013390;
        L80013408: ;
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
    public static void func_80013430(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V1 = 0x801D0000u;
        c.V1 = MemoryAccess.ReadU8(m, (c.V1 - 0x6720u));
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
        if (c.V1 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
            goto L80013470;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.RA = 0x80013458u;
        GranTurismo2PC.func_80013230(c, m);
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x24u));
        c.RA = 0x80013464u;
        GranTurismo2PC.func_800132CC(c, m);
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x28u));
        c.RA = 0x80013470u;
        GranTurismo2PC.func_800132CC(c, m);
        L80013470: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80013480(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V1 = 0x801D0000u;
        c.V1 = MemoryAccess.ReadU8(m, (c.V1 - 0x6720u));
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
        if (c.V1 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
            goto L800134B4;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.RA = 0x800134A8u;
        GranTurismo2PC.func_80013230(c, m);
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x20u));
        c.RA = 0x800134B4u;
        GranTurismo2PC.func_800132CC(c, m);
        L800134B4: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800134C4(CpuContext c, IMemory m)
    {
        c.A1 = 0u + 0u;
        c.V1 = c.A1 + 0u;
        c.V0 = 0x800B0000u;
        c.A2 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
        c.A0 = 0x00000988u;
        c.A3 = c.A2 + 0x1A1Cu;
        L800134DC: ;
        c.V0 = (int)c.V1 < 32 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.A2 + c.V1;
            goto L80013510;
        }
        c.V0 = c.A2 + c.V1;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V0 + 0x2FA8u));
        if (c.V0 == 0u) {
            c.V0 = c.A3 + c.A0;
            goto L80013504;
        }
        c.V0 = c.A3 + c.A0;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x52u));
        c.A1 = c.A1 + c.V0;
        L80013504: ;
        c.A0 = c.A0 + 0x5Cu;
        c.V1 = c.V1 + 0x1u;
        goto L800134DC;
        L80013510: ;
        c.V0 = c.A1 + 0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80013518(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
        c.A0 = 0u + 0u;
        c.V1 = c.A0 + 0u;
        c.V0 = c.A1 + c.V1;
        L8001352C: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V0 + 0x2FA8u));
        if (c.V0 == 0u) {
            goto L80013540;
        }
        c.A0 = c.A0 + 0x1u;
        L80013540: ;
        c.V1 = c.V1 + 0x1u;
        c.V0 = (int)c.V1 < 32 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = c.A1 + c.V1;
            goto L8001352C;
        }
        c.V0 = c.A1 + c.V1;
        c.V0 = c.A0 + 0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80013558(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V0 + 0x68Cu));
        c.RA = 0x80013578u;
        GranTurismo2PC.func_80013518(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.V0 = c.S0 + c.V0;
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001358C(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x68Eu));
        c.A0 = c.V0 + 0x690u;
        c.RA = 0x800135ACu;
        GranTurismo2PC.func_80068FE8(c, m);
        c.S0 = c.S0 - c.V0;
        c.RA = 0x800135B4u;
        GranTurismo2PC.func_800134C4(c, m);
        c.V0 = c.S0 - c.V0;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800135C8(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x68Eu));
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800135E0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        c.S1 = c.A1 + 0u;
        c.RA = 0x80013608u;
        GranTurismo2PC.func_8001358C(c, m);
        c.V1 = c.V0 + 0u;
        MemoryAccess.WriteU32(m, c.S1, 0u);
        c.V0 = c.S2 + c.S0;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V0 + 0x2FA8u));
        c.A0 = 0x00000001u;
        if (c.V0 != c.A0) {
            c.V0 = c.S0 << 1;
            goto L8001362C;
        }
        c.V0 = c.S0 << 1;
        c.V0 = 0x00000001u;
        goto L8001367C;
        L8001362C: ;
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
            goto L8001367C;
        }
        c.V0 = 0u + 0u;
        c.V0 = 0x00000002u;
        MemoryAccess.WriteU32(m, c.S1, c.V0);
        c.RA = 0x80013664u;
        GranTurismo2PC.func_80013558(c, m);
        c.V1 = 0x00000020u;
        if (c.V0 == c.V1) {
            c.V0 = 0x00000001u;
            goto L80013678;
        }
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, c.S1, 0u);
        goto L8001367C;
        L80013678: ;
        c.V0 = 0u + 0u;
        L8001367C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80013694(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
        c.V1 = c.V0 + c.A0;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V1 + 0x2FA8u));
        c.A0 = 0x00000001u;
        if (c.V0 != c.A0) {
            goto L800136BC;
        }
        MemoryAccess.WriteU8(m, (c.V1 + 0x2FA8u), (byte)0u);
        return;
        L800136BC: ;
        MemoryAccess.WriteU8(m, (c.V1 + 0x2FA8u), (byte)c.A0);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800136C4(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
        c.V1 = 0x00000020u;
        c.V0 = c.V0 + c.V1;
        L800136D4: ;
        MemoryAccess.WriteU8(m, (c.V0 + 0x2FA8u), (byte)0u);
        c.V1 = c.V1 - 0x1u;
        if ((int)c.V1 >= 0) {
            c.V0 = c.V0 - 0x1u;
            goto L800136D4;
        }
        c.V0 = c.V0 - 0x1u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800136EC(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0x68Eu));
        c.S0 = c.A0 + 0x1A18u;
        MemoryAccess.WriteU16(m, (c.A0 + 0x1A18u), (ushort)c.V0);
        c.A0 = c.A0 + 0x690u;
        c.RA = 0x80013714u;
        GranTurismo2PC.func_80068FE8(c, m);
        MemoryAccess.WriteU16(m, (c.S0 + 0x2u), (ushort)c.V0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80013728(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x168u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x15Cu), c.S7);
        c.S7 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
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
        c.RA = 0x80013770u;
        GranTurismo2PC.func_8006AC68(c, m);
        c.V1 = 0xFF9F0000u;
        c.V1 = c.V1 | 0xFFFFu;
        c.S6 = c.SP + 0x20u;
        c.A0 = c.S6 + 0u;
        c.A1 = 0x800B0000u;
        c.A1 = c.A1 - 0x7220u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S6 + 0xCu));
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x168u));
        c.V0 = c.V0 & c.V1;
        c.V1 = 0x00200000u;
        c.V0 = c.V0 | c.V1;
        MemoryAccess.WriteU32(m, (c.S6 + 0x10u), c.T0);
        MemoryAccess.WriteU32(m, (c.S6 + 0xCu), c.V0);
        c.RA = 0x800137A8u;
        GranTurismo2PC.func_8007DA80(c, m);
        c.V0 = 0x024A0000u;
        c.V0 = c.V0 | 0x4136u;
        c.FP = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.S6 + 0x1Cu), (byte)c.FP);
        if (c.S0 == 0u) {
            MemoryAccess.WriteU32(m, (c.S6 + 0x14u), c.V0);
            goto L800138AC;
        }
        MemoryAccess.WriteU32(m, (c.S6 + 0x14u), c.V0);
        c.S2 = c.SP + 0x40u;
        c.A0 = c.S2 + 0u;
        c.S0 = c.S7 + 0x48Cu;
        c.S1 = 0x801F0000u;
        c.S1 = c.S1 - 0x4A5u;
        c.A2 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x200u));
        c.A1 = c.S1 + 0u;
        c.RA = 0x800137E0u;
        GranTurismo2PC.func_8008CF34(c, m);
        c.A0 = c.S6 + 0u;
        c.A1 = c.S2 + 0u;
        c.A2 = c.FP + 0u;
        c.A3 = 0u + 0u;
        c.RA = 0x800137F4u;
        GranTurismo2PC.func_8006B044(c, m);
        c.A0 = c.S6 + 0u;
        c.A1 = c.S2 + 0u;
        c.S5 = 0x00000140u;
        c.A2 = c.S5 - c.V0;
        c.A3 = 0x0000008Au;
        c.S4 = 0xFFFFFFFEu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.RA = 0x8001381Cu;
        GranTurismo2PC.func_8006AF40(c, m);
        c.S3 = c.S7 + 0x690u;
        c.S0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x202u));
        c.A0 = c.S3 + 0u;
        c.RA = 0x8001382Cu;
        GranTurismo2PC.func_80068FE8(c, m);
        c.S0 = c.S0 - c.V0;
        c.RA = 0x80013834u;
        GranTurismo2PC.func_800135C8(c, m);
        c.A0 = c.S2 + 0u;
        c.A1 = c.S1 + 0x19u;
        c.A2 = c.S0 + 0u;
        c.A3 = c.V0 + 0u;
        c.RA = 0x80013848u;
        GranTurismo2PC.func_8008CF34(c, m);
        c.A0 = c.S6 + 0u;
        c.A1 = c.S2 + 0u;
        c.A2 = c.FP + 0u;
        c.A3 = 0u + 0u;
        c.RA = 0x8001385Cu;
        GranTurismo2PC.func_8006B044(c, m);
        c.A0 = c.S6 + 0u;
        c.A1 = c.S2 + 0u;
        c.A2 = c.S5 - c.V0;
        c.A3 = 0x0000009Eu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.RA = 0x8001387Cu;
        GranTurismo2PC.func_8006AF40(c, m);
        c.A0 = c.S3 + 0u;
        c.RA = 0x80013884u;
        GranTurismo2PC.func_80068FE8(c, m);
        c.A0 = c.S7 + 0x1A18u;
        c.A2 = 0x000000B0u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0x168u));
        c.A3 = 0x000000AAu;
        MemoryAccess.WriteU16(m, (c.A0 + 0x2u), (ushort)c.V0);
        c.V0 = 0x00000080u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        goto L80013988;
        L800138AC: ;
        c.S1 = 0x801F0000u;
        c.RA = 0x800138B4u;
        GranTurismo2PC.func_80013558(c, m);
        c.S2 = c.SP + 0x40u;
        c.A0 = c.S2 + 0u;
        c.S1 = c.S1 - 0x4A5u;
        c.A1 = c.S1 + 0u;
        c.A2 = c.V0 + 0u;
        c.RA = 0x800138CCu;
        GranTurismo2PC.func_8008CF34(c, m);
        c.A0 = c.S6 + 0u;
        c.A1 = c.S2 + 0u;
        c.A2 = 0x00000001u;
        c.A3 = 0u + 0u;
        c.RA = 0x800138E0u;
        GranTurismo2PC.func_8006B044(c, m);
        c.A0 = c.S6 + 0u;
        c.A1 = c.S2 + 0u;
        c.S4 = 0x00000140u;
        c.A2 = c.S4 - c.V0;
        c.A3 = 0x0000008Au;
        c.S3 = 0xFFFFFFFEu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.RA = 0x80013908u;
        GranTurismo2PC.func_8006AF40(c, m);
        c.RA = 0x80013910u;
        GranTurismo2PC.func_8001358C(c, m);
        c.S0 = c.V0 + 0u;
        c.RA = 0x80013918u;
        GranTurismo2PC.func_800135C8(c, m);
        c.A0 = c.S2 + 0u;
        c.A1 = c.S1 + 0x19u;
        c.A2 = c.S0 + 0u;
        c.A3 = c.V0 + 0u;
        c.RA = 0x8001392Cu;
        GranTurismo2PC.func_8008CF34(c, m);
        c.A0 = c.S6 + 0u;
        c.A1 = c.S2 + 0u;
        c.A2 = 0x00000001u;
        c.A3 = 0u + 0u;
        c.RA = 0x80013940u;
        GranTurismo2PC.func_8006B044(c, m);
        c.A0 = c.S6 + 0u;
        c.A1 = c.S2 + 0u;
        c.A2 = c.S4 - c.V0;
        c.A3 = 0x0000009Eu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.RA = 0x80013960u;
        GranTurismo2PC.func_8006AF40(c, m);
        c.RA = 0x80013968u;
        GranTurismo2PC.func_800134C4(c, m);
        c.A0 = c.S7 + 0x1A18u;
        c.A2 = 0x000000B0u;
        c.A3 = 0x000000AAu;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0x168u));
        c.V1 = 0x00000080u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V1);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.V0);
        L80013988: ;
        c.RA = 0x80013990u;
        GranTurismo2PC.func_8006AA68(c, m);
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
    public static void func_800139C0(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
        c.SP = c.SP - 0x150u;
        MemoryAccess.WriteU32(m, (c.SP + 0x144u), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x148u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x140u), c.S0);
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x2Cu));
        c.A0 = c.SP + 0x20u;
        c.RA = 0x800139ECu;
        GranTurismo2PC.func_8006AC68(c, m);
        c.V1 = 0xFF9F0000u;
        c.V1 = c.V1 | 0xFFFFu;
        c.S0 = c.SP + 0x20u;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0xCu));
        c.A1 = c.A1 - 0x7220u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x10u), c.S1);
        c.V0 = c.V0 & c.V1;
        c.V1 = 0x00200000u;
        c.V0 = c.V0 | c.V1;
        MemoryAccess.WriteU32(m, (c.S0 + 0xCu), c.V0);
        c.RA = 0x80013A20u;
        GranTurismo2PC.func_8007DA80(c, m);
        c.V0 = 0x024A0000u;
        c.V0 = c.V0 | 0x4136u;
        c.S2 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x1Cu), (byte)c.S2);
        MemoryAccess.WriteU32(m, (c.S0 + 0x14u), c.V0);
        c.RA = 0x80013A38u;
        GranTurismo2PC.func_80013518(c, m);
        c.S1 = c.SP + 0x40u;
        c.A0 = c.S1 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x65CCu;
        c.A2 = c.V0 + 0u;
        c.RA = 0x80013A50u;
        GranTurismo2PC.func_8008CF34(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0u;
        c.A2 = c.S2 + 0u;
        c.A3 = 0u + 0u;
        c.RA = 0x80013A64u;
        GranTurismo2PC.func_8006B044(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0u;
        c.A2 = 0x00000140u;
        c.A2 = c.A2 - c.V0;
        c.A3 = 0x000000E0u;
        c.V0 = 0xFFFFFFFEu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.RA = 0x80013A8Cu;
        GranTurismo2PC.func_8006AF40(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x148u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x144u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x140u));
        c.SP = c.SP + 0x150u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80013AA4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x98u;
        MemoryAccess.WriteU32(m, (c.SP + 0x78u), c.S2);
        c.S2 = c.A0 + 0u;
        c.V1 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x74u), c.S1);
        c.S1 = 0x00000001u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x88u), c.S6);
        c.S6 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
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
        c.RA = 0x80013B18u;
        GranTurismo2PC.func_8006AC68(c, m);
        c.S4 = c.SP + 0x20u;
        c.V0 = 0x00000004u;
        if (c.S2 == c.V0) {
            MemoryAccess.WriteU8(m, (c.S4 + 0x1Cu), (byte)c.S1);
            goto L80013B4C;
        }
        MemoryAccess.WriteU8(m, (c.S4 + 0x1Cu), (byte)c.S1);
        c.V0 = (int)c.S2 < 5 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = 0x00000006u;
            goto L80013DE0;
        }
        c.V0 = 0x00000006u;
        if (c.S2 == c.V0) {
            c.V0 = 0x00000008u;
            goto L80013DE0;
        }
        c.V0 = 0x00000008u;
        if (c.S2 == c.V0) {
            goto L80013DB4;
        }
        goto L80013DE0;
        L80013B4C: ;
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
            goto L80013DE0;
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
            goto L80013DE0;
        }
        c.V1 = 0x005A0000u;
        if (c.S1 != 0u) {
            c.V1 = c.V1 | 0x4A3Eu;
            goto L80013BD4;
        }
        c.V1 = c.V1 | 0x4A3Eu;
        c.V1 = 0x00080000u;
        c.V1 = c.V1 | 0x082Au;
        L80013BD4: ;
        c.T1 = 0x80050000u;
        c.T1 = c.T1 - 0x4840u;
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
            goto L80013C0C;
        }
        c.V0 = c.S3 << 1;
        c.S3 = 0u + 0u;
        c.V0 = c.S3 << 1;
        L80013C0C: ;
        c.V0 = c.V0 + c.S3;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 - c.S3;
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0xA0u));
        c.V0 = c.V0 << 4;
        c.V1 = c.S6 + c.T1;
        c.V1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V1 + 0x2FA8u));
        if (c.V1 == 0u) {
            c.S3 = (uint)((int)c.V0 >> 7);
            goto L80013D38;
        }
        c.S3 = (uint)((int)c.V0 >> 7);
        c.V0 = 0x005A0000u;
        c.V0 = c.V0 | 0x8C32u;
        c.A0 = c.S4 + 0u;
        c.A1 = 0x800B0000u;
        c.A1 = c.A1 - 0x7210u;
        MemoryAccess.WriteU32(m, (c.SP + 0x50u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x54u), c.V0);
        c.RA = 0x80013C54u;
        GranTurismo2PC.func_8007DA80(c, m);
        c.T0 = 0xFF9F0000u;
        c.T0 = c.T0 | 0xFFFFu;
        c.V0 = 0x020A0000u;
        c.V0 = c.V0 | 0x5A5Au;
        c.A0 = c.S4 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6579u;
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
        c.RA = 0x80013CA8u;
        GranTurismo2PC.func_8006AC90(c, m);
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
        c.RA = 0x80013CE4u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S2 + 0u;
        c.A2 = 0x00000020u;
        c.A3 = 0x00000080u;
        MemoryAccess.WriteU32(m, (c.SP + 0x54u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x48u), c.V0);
        c.RA = 0x80013D00u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S5 + 0u;
        c.S0 = c.SP + 0x40u;
        c.A1 = c.S0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x4Cu), c.V0);
        c.RA = 0x80013D14u;
        GranTurismo2PC.func_8006B6E4(c, m);
        c.A0 = c.S5 + 0u;
        c.A1 = 0x00000220u;
        c.RA = 0x80013D20u;
        GranTurismo2PC.func_8007DA44(c, m);
        c.A0 = c.S5 + 0u;
        c.A1 = c.S0 + 0u;
        c.RA = 0x80013D2Cu;
        GranTurismo2PC.func_8006B6E4(c, m);
        c.A0 = c.S5 + 0u;
        c.A1 = 0x00000240u;
        c.RA = 0x80013D38u;
        GranTurismo2PC.func_8007DA44(c, m);
        L80013D38: ;
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x68u));
        c.V0 = c.T1 + c.S6;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x398u));
        if (c.V1 != 0u) {
            goto L80013D7C;
        }
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x64u));
        c.V0 = c.T1 << 3;
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0xA0u));
        c.V0 = c.V0 - c.T1;
        c.V0 = c.V0 << 2;
        c.V0 = c.S6 + c.V0;
        c.V0 = c.V0 + 0x23A4u;
        MemoryAccess.WriteU32(m, (c.SP + 0x60u), c.V0);
        L80013D7C: ;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4840u;
        c.A1 = c.S5 + 0u;
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0x60u));
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x58u));
        c.A2 = c.S4 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.V1);
        c.V0 = c.T1 + c.S3;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.RA = 0x80013DACu;
        GranTurismo2PC.func_8006A4E4(c, m);
        goto L80013DE0;
        L80013DB4: ;
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0xA0u));
        c.V0 = c.T1 << 1;
        c.V0 = c.V0 + c.T1;
        c.V0 = c.V0 << 1;
        c.V0 = c.V0 + c.S6;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x39Au));
        c.V0 = c.V0 ^ 0x0001u;
        c.V0 = c.V0 < 0x00000001u ? 1u : 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x5Cu), c.V0);
        L80013DE0: ;
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
    public static void func_80013E14(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.V1 = MemoryAccess.ReadU32(m, (c.A1 + 0xCu));
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.V1);
        if (c.A0 == 0u) {
            c.S0 = c.V0 < 0x00000001u ? 1u : 0u;
            goto L80013E54;
        }
        c.S0 = c.V0 < 0x00000001u ? 1u : 0u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V1 + 0x2u));
        if (c.V0 == 0u) {
            c.S0 = c.S0 & 0x0001u;
            goto L80013E70;
        }
        c.S0 = c.S0 & 0x0001u;
        c.S0 = 0u + 0u;
        goto L80013E70;
        L80013E54: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V1 + 0x2u));
        c.V1 = c.V0 < 0x00000001u ? 1u : 0u;
        c.V0 = c.V0 ^ 0x0003u;
        c.V0 = c.V0 < 0x00000001u ? 1u : 0u;
        c.V1 = c.V1 | c.V0;
        c.S0 = c.S0 & c.V1;
        L80013E70: ;
        if (c.S0 == 0u) {
            c.A0 = c.A1 + 0x1A1Cu;
            goto L80013E8C;
        }
        c.A0 = c.A1 + 0x1A1Cu;
        c.A1 = 0u + 0u;
        c.RA = 0x80013E80u;
        GranTurismo2PC.func_800696C4(c, m);
        if (c.V0 != 0u) {
            c.S0 = c.S0 & 0x0001u;
            goto L80013E8C;
        }
        c.S0 = c.S0 & 0x0001u;
        c.S0 = 0u + 0u;
        L80013E8C: ;
        c.V0 = c.S0 + 0u;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80013EA0(CpuContext c, IMemory m)
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
        c.S2 = MemoryAccess.ReadU32(m, (c.V1 - 0x7228u));
        c.V0 = c.V0 + 0x10E4u;
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
            goto L80013F70;
        }
        c.S0 = c.S4 + 0u;
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x10ECu;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x80013F44u: goto L80013F44;
            case 0x80013F60u: goto L80013F60;
            case 0x80013F20u: goto L80013F20;
            case 0x80013F2Cu: goto L80013F2C;
            case 0x80013F38u: goto L80013F38;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L80013F20: ;
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x69D0u;
        goto L80013F68;
        L80013F2C: ;
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x679Au;
        goto L80013F68;
        L80013F38: ;
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6928u;
        goto L80013F68;
        L80013F44: ;
        c.A0 = c.S2 + 0x1A1Cu;
        c.A1 = 0u + 0u;
        c.RA = 0x80013F50u;
        GranTurismo2PC.func_800696C4(c, m);
        if (c.V0 != 0u) {
            c.V0 = 0x801C0000u;
            goto L80013F70;
        }
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x679Au;
        goto L80013F68;
        L80013F60: ;
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x68D2u;
        L80013F68: ;
        MemoryAccess.WriteU32(m, c.S1, c.V0);
        c.S0 = 0x00000001u;
        L80013F70: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S2 + 0xCu));
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x2u));
        c.V0 = 0x00000002u;
        if (c.V1 == c.V0) {
            c.A0 = c.V1 + 0u;
            goto L80013FC0;
        }
        c.A0 = c.V1 + 0u;
        c.V0 = (int)c.V1 < 3 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000001u;
            goto L80013FA4;
        }
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            c.V0 = 0x801C0000u;
            goto L80013FE4;
        }
        c.V0 = 0x801C0000u;
        goto L80013FF0;
        L80013FA4: ;
        c.V0 = 0x00000003u;
        if (c.A0 == c.V0) {
            c.V0 = 0x00000004u;
            goto L80013FCC;
        }
        c.V0 = 0x00000004u;
        if (c.A0 == c.V0) {
            c.V0 = 0x801C0000u;
            goto L80013FDC;
        }
        c.V0 = 0x801C0000u;
        goto L80013FF0;
        L80013FC0: ;
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x69ABu;
        goto L80013FE8;
        L80013FCC: ;
        if (c.S5 == 0u) {
            c.V0 = 0x801C0000u;
            goto L80013FF0;
        }
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6957u;
        goto L80013FE8;
        L80013FDC: ;
        c.V0 = c.V0 - 0x68FDu;
        goto L80013FE8;
        L80013FE4: ;
        c.V0 = c.V0 - 0x68AEu;
        L80013FE8: ;
        MemoryAccess.WriteU32(m, c.S3, c.V0);
        c.S0 = 0x00000001u;
        L80013FF0: ;
        if (c.S0 != 0u) {
            c.V0 = c.S4 + 0u;
            goto L80014000;
        }
        c.V0 = c.S4 + 0u;
        c.S4 = 0x00000001u;
        c.V0 = c.S4 + 0u;
        L80014000: ;
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
    public static void func_80014024(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
        c.V0 = 0x00000001u;
        if (c.A0 == c.V0) {
            c.A1 = 0xFFFFFFFFu;
            goto L80014054;
        }
        c.A1 = 0xFFFFFFFFu;
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L80014074;
        }
        if (c.A0 != 0u) {
            c.V0 = 0x00000018u;
            goto L80014074;
        }
        c.V0 = 0x00000018u;
        MemoryAccess.WriteU16(m, (c.V1 + 0x2Eu), (ushort)c.V0);
        goto L80014074;
        L80014054: ;
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 + 0x2Eu));
        c.V0 = c.V0 - 0x1u;
        MemoryAccess.WriteU16(m, (c.V1 + 0x2Eu), (ushort)c.V0);
        c.V0 = c.V0 << 16;
        if ((int)c.V0 >= 0) {
            goto L80014074;
        }
        c.A1 = 0x00000002u;
        L80014074: ;
        c.V0 = c.A1 + 0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001407C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x50u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x3Cu), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.S3);
        c.S3 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x38u), c.S0);
        c.S0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x48u), c.RA);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0xF4u));
        if (c.A0 == c.S0) {
            c.S2 = 0xFFFFFFFFu;
            goto L80014110;
        }
        c.S2 = 0xFFFFFFFFu;
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L800140CC;
        }
        c.V0 = 0x00000002u;
        if (c.A0 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L800140DC;
        }
        c.V0 = c.S2 + 0u;
        goto L800141F0;
        L800140CC: ;
        if (c.A0 == c.V0) {
            c.V0 = c.S2 + 0u;
            goto L80014150;
        }
        c.V0 = c.S2 + 0u;
        goto L800141F0;
        L800140DC: ;
        c.A0 = 0u + 0u;
        c.RA = 0x800140E4u;
        GranTurismo2PC.func_80060840(c, m);
        c.S0 = c.S1 + 0x60u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x800140F0u;
        GranTurismo2PC.func_8006E388(c, m);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x10E4u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x7Du), (byte)0u);
        MemoryAccess.WriteU32(m, (c.S1 + 0x24u), c.V0);
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        c.RA = 0x80014108u;
        GranTurismo2PC.func_80013430(c, m);
        c.V0 = c.S2 + 0u;
        goto L800141F0;
        L80014110: ;
        if (c.V0 == 0u) {
            goto L80014140;
        }
        if ((int)c.V0 > 0) {
            goto L80014130;
        }
        if (c.V0 == c.S2) {
            c.V0 = c.S2 + 0u;
            goto L80014138;
        }
        c.V0 = c.S2 + 0u;
        goto L800141F0;
        L80014130: ;
        if (c.V0 != c.S0) {
            c.V0 = c.S2 + 0u;
            goto L800141F0;
        }
        c.V0 = c.S2 + 0u;
        L80014138: ;
        c.S2 = 0x00000016u;
        goto L800141EC;
        L80014140: ;
        c.A0 = 0x00000001u;
        c.RA = 0x80014148u;
        GranTurismo2PC.func_80060840(c, m);
        c.S2 = 0x00000002u;
        goto L800141EC;
        L80014150: ;
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x2Cu));
        c.A0 = c.SP + 0x18u;
        c.RA = 0x8001415Cu;
        GranTurismo2PC.func_8006AC68(c, m);
        c.A0 = c.SP + 0x18u;
        c.A1 = 0x800B0000u;
        c.A1 = c.A1 - 0x7210u;
        c.RA = 0x8001416Cu;
        GranTurismo2PC.func_8007DA80(c, m);
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
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6720u));
        c.V1 = c.V1 & c.A0;
        c.A0 = 0x00200000u;
        c.V1 = c.V1 | c.A0;
        if (c.V0 != 0u) {
            MemoryAccess.WriteU32(m, (c.A1 + 0xCu), c.V1);
            goto L800141D4;
        }
        MemoryAccess.WriteU32(m, (c.A1 + 0xCu), c.V1);
        c.V0 = 0x02600000u;
        c.V0 = c.V0 | 0x6060u;
        c.A0 = c.S3 + 0u;
        c.A2 = 0x000000B0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x20u));
        c.A3 = 0x00000112u;
        c.RA = 0x800141CCu;
        GranTurismo2PC.func_8001330C(c, m);
        c.V0 = c.S2 + 0u;
        goto L800141F0;
        L800141D4: ;
        c.A0 = c.A1 + 0u;
        c.A2 = 0x000000B0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x20u));
        c.A3 = 0x00000122u;
        c.RA = 0x800141ECu;
        GranTurismo2PC.func_8006ADB4(c, m);
        L800141EC: ;
        c.V0 = c.S2 + 0u;
        L800141F0: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x48u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x44u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x40u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x3Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x38u));
        c.SP = c.SP + 0x50u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001420C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x30u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S1);
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S2);
        c.S3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x224u));
        if (c.A0 == c.V0) {
            c.S1 = 0xFFFFFFFFu;
            goto L800142AC;
        }
        c.S1 = 0xFFFFFFFFu;
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S1 + 0u;
            goto L80014408;
        }
        c.V0 = c.S1 + 0u;
        if (c.A0 != 0u) {
            c.V1 = c.S0 + 0x30u;
            goto L80014408;
        }
        c.V1 = c.S0 + 0x30u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V1 + 0x18u));
        if ((int)c.V0 < 0) {
            goto L80014284;
        }
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 + 0x4u));
        c.V0 = ~(0u | c.V0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x18u), (ushort)c.V0);
        c.V1 = c.S0 + 0x4Cu;
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 + 0x10u));
        c.V0 = ~(0u | c.V0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x12u), (ushort)c.V0);
        L80014284: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0xCu));
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x2u));
        c.V0 = 0x00000003u;
        if (c.V1 != c.V0) {
            goto L800142A4;
        }
        c.S1 = 0x00000003u;
        goto L80014404;
        L800142A4: ;
        c.A0 = c.S0 + 0x190u;
        c.RA = 0x800142ACu;
        GranTurismo2PC.func_8006E388(c, m);
        L800142AC: ;
        c.A0 = c.SP + 0x10u;
        c.A1 = c.SP + 0x14u;
        c.A2 = 0x00000001u;
        c.RA = 0x800142BCu;
        GranTurismo2PC.func_80013EA0(c, m);
        c.S2 = c.V0 + 0u;
        if (c.S2 != 0u) {
            c.V0 = 0x801C0000u;
            goto L800142DC;
        }
        c.V0 = 0x801C0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        MemoryAccess.WriteU32(m, (c.S0 + 0x24u), c.V0);
        MemoryAccess.WriteU32(m, (c.S0 + 0x28u), c.V1);
        goto L800142EC;
        L800142DC: ;
        c.V0 = c.V0 - 0x67DEu;
        MemoryAccess.WriteU32(m, (c.S0 + 0x24u), c.V0);
        c.V0 = c.V0 + 0x19u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x28u), c.V0);
        L800142EC: ;
        c.RA = 0x800142F4u;
        GranTurismo2PC.func_80013430(c, m);
        c.V0 = 0x02800000u;
        c.V0 = c.V0 | 0x400Cu;
        c.V1 = c.S0 + 0x190u;
        if (c.S2 != 0u) {
            MemoryAccess.WriteU32(m, (c.V1 + 0x8Cu), c.V0);
            goto L80014320;
        }
        MemoryAccess.WriteU32(m, (c.V1 + 0x8Cu), c.V0);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V1 + 0x7Du));
        if (c.V0 != 0u) {
            c.V0 = 0x02080000u;
            goto L80014320;
        }
        c.V0 = 0x02080000u;
        c.V0 = c.V0 | 0x0830u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x8Cu), c.V0);
        L80014320: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0xCu));
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x2u));
        if (c.V1 == 0u) {
            c.V0 = 0x00000003u;
            goto L80014358;
        }
        c.V0 = 0x00000003u;
        if (c.V1 != c.V0) {
            goto L8001437C;
        }
        c.A0 = 0x00000007u;
        c.RA = 0x80014348u;
        GranTurismo2PC.func_80060840(c, m);
        c.A0 = c.S0 + 0x190u;
        c.RA = 0x80014350u;
        GranTurismo2PC.func_8006E3FC(c, m);
        c.S1 = 0x00000003u;
        goto L80014404;
        L80014358: ;
        c.A0 = c.S0 + 0x48Cu;
        c.A1 = 0x00000001u;
        c.RA = 0x80014364u;
        GranTurismo2PC.func_800696C4(c, m);
        if (c.V0 != 0u) {
            goto L8001437C;
        }
        c.A0 = c.S0 + 0x190u;
        c.RA = 0x80014374u;
        GranTurismo2PC.func_8006E3FC(c, m);
        c.S1 = 0x00000005u;
        goto L80014404;
        L8001437C: ;
        if (c.S3 == 0u) {
            goto L800143B8;
        }
        if ((int)c.S3 > 0) {
            c.V0 = 0x00000001u;
            goto L800143A0;
        }
        c.V0 = 0x00000001u;
        c.V0 = 0xFFFFFFFFu;
        if (c.S3 == c.V0) {
            c.V0 = c.S1 + 0u;
            goto L800143A8;
        }
        c.V0 = c.S1 + 0u;
        goto L80014408;
        L800143A0: ;
        if (c.S3 != c.V0) {
            c.V0 = c.S1 + 0u;
            goto L80014408;
        }
        c.V0 = c.S1 + 0u;
        L800143A8: ;
        c.A0 = c.S0 + 0x190u;
        c.RA = 0x800143B0u;
        GranTurismo2PC.func_8006E3FC(c, m);
        c.S1 = 0x00000016u;
        goto L80014404;
        L800143B8: ;
        c.A0 = 0x00000001u;
        c.RA = 0x800143C0u;
        GranTurismo2PC.func_80013E14(c, m);
        if (c.V0 == 0u) {
            goto L800143FC;
        }
        c.A0 = 0x00000001u;
        c.RA = 0x800143D0u;
        GranTurismo2PC.func_80060840(c, m);
        c.A0 = c.S0 + 0x190u;
        c.RA = 0x800143D8u;
        GranTurismo2PC.func_8006E3FC(c, m);
        c.S1 = 0x00000009u;
        c.RA = 0x800143E0u;
        GranTurismo2PC.func_800136C4(c, m);
        c.A0 = c.S0 + 0x48Cu;
        c.A1 = 0x00000001u;
        c.RA = 0x800143ECu;
        GranTurismo2PC.func_800696C4(c, m);
        if (c.V0 != 0u) {
            c.V0 = c.S1 + 0u;
            goto L80014408;
        }
        c.V0 = c.S1 + 0u;
        c.S1 = 0x00000005u;
        goto L80014404;
        L800143FC: ;
        c.A0 = 0u + 0u;
        c.RA = 0x80014404u;
        GranTurismo2PC.func_80060840(c, m);
        L80014404: ;
        c.V0 = c.S1 + 0u;
        L80014408: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.SP = c.SP + 0x30u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80014424(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        c.S4 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x2BCu));
        if (c.A0 == c.S4) {
            c.S3 = 0xFFFFFFFFu;
            goto L8001449C;
        }
        c.S3 = 0xFFFFFFFFu;
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S3 + 0u;
            goto L80014504;
        }
        c.V0 = c.S3 + 0u;
        if (c.A0 != 0u) {
            c.S0 = c.S1 + 0x228u;
            goto L80014504;
        }
        c.S0 = c.S1 + 0x228u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x80014474u;
        GranTurismo2PC.func_8006E388(c, m);
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6740u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x7Du), (byte)c.S4);
        MemoryAccess.WriteU32(m, (c.S1 + 0x24u), c.V0);
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0x82Au;
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        c.RA = 0x80014494u;
        GranTurismo2PC.func_80013430(c, m);
        MemoryAccess.WriteU16(m, (c.S1 + 0x48u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.S1 + 0x5Eu), (ushort)0u);
        L8001449C: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0xCu));
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x2u));
        c.V0 = 0x00000003u;
        if (c.V1 == c.V0) {
            goto L800144C4;
        }
        c.A0 = c.S1 + 0x228u;
        c.RA = 0x800144BCu;
        GranTurismo2PC.func_8006E3FC(c, m);
        c.S3 = 0x00000002u;
        goto L80014500;
        L800144C4: ;
        if (c.S2 == 0u) {
            goto L800144F4;
        }
        if ((int)c.S2 > 0) {
            goto L800144E4;
        }
        if (c.S2 == c.S3) {
            c.V0 = c.S3 + 0u;
            goto L800144EC;
        }
        c.V0 = c.S3 + 0u;
        goto L80014504;
        L800144E4: ;
        if (c.S2 != c.S4) {
            c.V0 = c.S3 + 0u;
            goto L80014504;
        }
        c.V0 = c.S3 + 0u;
        L800144EC: ;
        c.S3 = 0x00000016u;
        goto L80014500;
        L800144F4: ;
        c.A0 = 0x00000001u;
        c.RA = 0x800144FCu;
        GranTurismo2PC.func_80060840(c, m);
        c.S3 = 0x00000004u;
        L80014500: ;
        c.V0 = c.S3 + 0u;
        L80014504: ;
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
    public static void func_80014524(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = 0xFFFFFFFFu;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
        c.V0 = 0x00000001u;
        if (c.S0 == c.V0) {
            MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
            goto L80014594;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        c.V0 = (int)c.S0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L800145D0;
        }
        c.V0 = c.S2 + 0u;
        if (c.S0 != 0u) {
            goto L800145D0;
        }
        c.A0 = 0x00000001u;
        c.RA = 0x8001456Cu;
        GranTurismo2PC.func_8007F5A8(c, m);
        if (c.V0 != 0u) {
            c.V0 = 0x801C0000u;
            goto L800145C8;
        }
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x602Fu;
        MemoryAccess.WriteU32(m, (c.S1 + 0x24u), c.V0);
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0x8BCu;
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        c.RA = 0x8001458Cu;
        GranTurismo2PC.func_80013430(c, m);
        c.V0 = c.S2 + 0u;
        goto L800145D0;
        L80014594: ;
        c.RA = 0x8001459Cu;
        GranTurismo2PC.func_8007D7CC(c, m);
        if (c.V0 == 0u) {
            goto L800145B4;
        }
        if (c.V0 != c.S0) {
            c.V0 = 0x801C0000u;
            goto L800145BC;
        }
        c.V0 = 0x801C0000u;
        c.V0 = c.S2 + 0u;
        goto L800145D0;
        L800145B4: ;
        c.S2 = 0x00000005u;
        goto L800145CC;
        L800145BC: ;
        c.V0 = c.V0 - 0x6004u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x20u), c.V0);
        c.RA = 0x800145C8u;
        GranTurismo2PC.func_80013480(c, m);
        L800145C8: ;
        c.S2 = 0x00000001u;
        L800145CC: ;
        c.V0 = c.S2 + 0u;
        L800145D0: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800145E8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x356u));
        if (c.A0 == c.V0) {
            c.S1 = 0xFFFFFFFFu;
            goto L80014684;
        }
        c.S1 = 0xFFFFFFFFu;
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S1 + 0u;
            goto L800146F0;
        }
        c.V0 = c.S1 + 0u;
        if (c.A0 != 0u) {
            goto L800146F0;
        }
        c.V0 = 0x801F0000u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V0 + 0xBE2u));
        c.V0 = (int)c.A0 < 3 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80050000u;
            goto L80014644;
        }
        c.V0 = 0x80050000u;
        c.S1 = 0x00000006u;
        goto L800146EC;
        L80014644: ;
        c.V0 = c.V0 - 0x48FCu;
        c.V1 = 0x00000003u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x6u), (ushort)c.V1);
        MemoryAccess.WriteU16(m, (c.V0 + 0x8u), (ushort)c.A0);
        MemoryAccess.WriteU16(m, (c.V0 + 0x4u), (ushort)c.V1);
        MemoryAccess.WriteU16(m, (c.V0 + 0x1Cu), (ushort)0u);
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x5FD5u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x24u), c.V0);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x10E4u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x28u), c.V0);
        c.RA = 0x80014678u;
        GranTurismo2PC.func_80013430(c, m);
        MemoryAccess.WriteU16(m, (c.S0 + 0x48u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.S0 + 0x5Eu), (ushort)0u);
        goto L800146EC;
        L80014684: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0xCu));
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x2u));
        if (c.V0 == 0u) {
            c.V0 = 0x80050000u;
            goto L800146A8;
        }
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU16(m, (c.V0 - 0x48E0u), (ushort)c.S1);
        c.S1 = 0x00000002u;
        goto L800146EC;
        L800146A8: ;
        c.V0 = (int)c.A1 < -3 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V1 = 0x80050000u;
            goto L800146D8;
        }
        c.V1 = 0x80050000u;
        c.V0 = (int)c.A1 < -1 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = c.S1 + 0u;
            goto L800146F0;
        }
        c.V0 = c.S1 + 0u;
        if (c.A1 != c.S1) {
            c.V0 = 0xFFFFFFFFu;
            goto L800146DC;
        }
        c.V0 = 0xFFFFFFFFu;
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU16(m, (c.V0 - 0x48E0u), (ushort)c.S1);
        c.S1 = 0x00000016u;
        goto L800146EC;
        L800146D8: ;
        c.V0 = 0xFFFFFFFFu;
        L800146DC: ;
        MemoryAccess.WriteU16(m, (c.V1 - 0x48E0u), (ushort)c.V0);
        c.A0 = c.S0 + 0x48Cu;
        c.RA = 0x800146E8u;
        GranTurismo2PC.func_8006911C(c, m);
        c.S1 = 0x00000007u;
        L800146EC: ;
        c.V0 = c.S1 + 0u;
        L800146F0: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80014704(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0xF4u));
        if (c.A0 == c.V0) {
            c.S2 = 0xFFFFFFFFu;
            goto L80014780;
        }
        c.S2 = 0xFFFFFFFFu;
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L800147E8;
        }
        c.V0 = c.S2 + 0u;
        if (c.A0 != 0u) {
            goto L800147E8;
        }
        c.A0 = 0u + 0u;
        c.RA = 0x8001474Cu;
        GranTurismo2PC.func_80060840(c, m);
        c.S0 = c.S1 + 0x60u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x80014758u;
        GranTurismo2PC.func_8006E388(c, m);
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6711u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x7Du), (byte)0u);
        MemoryAccess.WriteU32(m, (c.S1 + 0x24u), c.V0);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x10E4u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        c.RA = 0x80014778u;
        GranTurismo2PC.func_80013430(c, m);
        c.V0 = c.S2 + 0u;
        goto L800147E8;
        L80014780: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0xCu));
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x2u));
        if (c.V0 == 0u) {
            goto L800147A8;
        }
        c.A0 = c.S1 + 0x60u;
        c.RA = 0x800147A0u;
        GranTurismo2PC.func_8006E3FC(c, m);
        c.S2 = 0x00000002u;
        goto L800147E4;
        L800147A8: ;
        if (c.V1 == 0u) {
            goto L800147D8;
        }
        if ((int)c.V1 > 0) {
            goto L800147C8;
        }
        if (c.V1 == c.S2) {
            c.V0 = c.S2 + 0u;
            goto L800147D0;
        }
        c.V0 = c.S2 + 0u;
        goto L800147E8;
        L800147C8: ;
        if (c.V1 != c.A0) {
            c.V0 = c.S2 + 0u;
            goto L800147E8;
        }
        c.V0 = c.S2 + 0u;
        L800147D0: ;
        c.S2 = 0x00000016u;
        goto L800147E4;
        L800147D8: ;
        c.A0 = 0x00000001u;
        c.RA = 0x800147E0u;
        GranTurismo2PC.func_80060840(c, m);
        c.S2 = 0x00000002u;
        L800147E4: ;
        c.V0 = c.S2 + 0u;
        L800147E8: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80014800(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = 0xFFFFFFFFu;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
        c.V0 = 0x00000001u;
        if (c.S0 == c.V0) {
            MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
            goto L80014874;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        c.V0 = (int)c.S0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L800148B0;
        }
        c.V0 = c.S2 + 0u;
        if (c.S0 != 0u) {
            c.A0 = c.S1 + 0x48Cu;
            goto L800148B0;
        }
        c.A0 = c.S1 + 0x48Cu;
        c.A1 = 0x00000001u;
        c.RA = 0x80014848u;
        GranTurismo2PC.func_800696EC(c, m);
        if (c.V0 == 0u) {
            c.V0 = 0x801C0000u;
            goto L80014854;
        }
        c.V0 = 0x801C0000u;
        c.S2 = 0x00000001u;
        L80014854: ;
        c.V0 = c.V0 - 0x5FA4u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x24u), c.V0);
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0x8BCu;
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        c.RA = 0x8001486Cu;
        GranTurismo2PC.func_80013430(c, m);
        c.V0 = c.S2 + 0u;
        goto L800148B0;
        L80014874: ;
        c.RA = 0x8001487Cu;
        GranTurismo2PC.func_8007D7CC(c, m);
        if (c.V0 == 0u) {
            goto L80014894;
        }
        if (c.V0 != c.S0) {
            c.V0 = 0x801C0000u;
            goto L8001489C;
        }
        c.V0 = 0x801C0000u;
        c.V0 = c.S2 + 0u;
        goto L800148B0;
        L80014894: ;
        c.S2 = 0x00000008u;
        goto L800148AC;
        L8001489C: ;
        c.V0 = c.V0 - 0x5F6Fu;
        MemoryAccess.WriteU32(m, (c.S1 + 0x20u), c.V0);
        c.RA = 0x800148A8u;
        GranTurismo2PC.func_80013480(c, m);
        c.S2 = 0x00000001u;
        L800148AC: ;
        c.V0 = c.S2 + 0u;
        L800148B0: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800148C8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = 0xFFFFFFFFu;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
        c.V0 = 0x00000001u;
        if (c.S0 == c.V0) {
            MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
            goto L80014988;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        c.V0 = (int)c.S0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L80014910;
        }
        c.V0 = 0x00000002u;
        if (c.S0 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L80014920;
        }
        c.V0 = c.S2 + 0u;
        goto L800149E8;
        L80014910: ;
        if (c.S0 == c.V0) {
            c.V0 = c.S2 + 0u;
            goto L800149DC;
        }
        c.V0 = c.S2 + 0u;
        goto L800149E8;
        L80014920: ;
        c.S0 = c.S1 + 0x48Cu;
        c.A0 = c.S0 + 0u;
        c.RA = 0x8001492Cu;
        GranTurismo2PC.func_800693EC(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x00000001u;
        c.RA = 0x80014938u;
        GranTurismo2PC.func_8006971C(c, m);
        if (c.V0 == 0u) {
            c.V0 = 0x801C0000u;
            goto L80014948;
        }
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6769u;
        goto L800149CC;
        L80014948: ;
        c.V1 = 0x000C0000u;
        c.V1 = c.V1 | 0x5090u;
        c.A0 = c.S1 + 0x35Cu;
        c.V0 = 0x0000150Cu;
        MemoryAccess.WriteU32(m, (c.A0 + 0x10u), c.V0);
        MemoryAccess.WriteU32(m, (c.A0 + 0xCu), c.V1);
        c.RA = 0x80014964u;
        GranTurismo2PC.func_8006C04C(c, m);
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6055u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x24u), c.V0);
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0x8BCu;
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        c.RA = 0x80014980u;
        GranTurismo2PC.func_80013430(c, m);
        c.V0 = c.S2 + 0u;
        goto L800149E8;
        L80014988: ;
        c.RA = 0x80014990u;
        GranTurismo2PC.func_8007D7CC(c, m);
        if (c.V0 == 0u) {
            goto L800149B8;
        }
        if (c.V0 != c.S0) {
            c.V0 = 0x801C0000u;
            goto L800149C4;
        }
        c.V0 = 0x801C0000u;
        c.V0 = 0x801F0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0x6D8u));
        c.A0 = c.S1 + 0x35Cu;
        c.RA = 0x800149B0u;
        GranTurismo2PC.func_8006C074(c, m);
        c.V0 = c.S2 + 0u;
        goto L800149E8;
        L800149B8: ;
        MemoryAccess.WriteU16(m, (c.S1 + 0x390u), (ushort)c.S2);
        c.S2 = 0x00000002u;
        goto L800149E4;
        L800149C4: ;
        c.V0 = c.V0 - 0x6769u;
        MemoryAccess.WriteU16(m, (c.S1 + 0x390u), (ushort)c.S2);
        L800149CC: ;
        MemoryAccess.WriteU32(m, (c.S1 + 0x20u), c.V0);
        c.RA = 0x800149D4u;
        GranTurismo2PC.func_80013480(c, m);
        c.S2 = 0x00000001u;
        goto L800149E4;
        L800149DC: ;
        c.A0 = c.S1 + 0x35Cu;
        c.RA = 0x800149E4u;
        GranTurismo2PC.func_8006C174(c, m);
        L800149E4: ;
        c.V0 = c.S2 + 0u;
        L800149E8: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80014A00(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = 0xFFFFFFFFu;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = 0x00000001u;
        if (c.S2 == c.S3) {
            MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
            goto L80014AD8;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        c.V0 = (int)c.S2 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L80014A4C;
        }
        c.V0 = 0x00000002u;
        if (c.S2 == 0u) {
            c.V0 = c.S1 + 0u;
            goto L80014A5C;
        }
        c.V0 = c.S1 + 0u;
        goto L80014B7C;
        L80014A4C: ;
        if (c.S2 == c.V0) {
            c.V0 = c.S1 + 0u;
            goto L80014B70;
        }
        c.V0 = c.S1 + 0u;
        goto L80014B7C;
        L80014A5C: ;
        c.A0 = c.S0 + 0x48Cu;
        c.A1 = 0x00000001u;
        c.RA = 0x80014A68u;
        GranTurismo2PC.func_800697AC(c, m);
        if (c.V0 == 0u) {
            c.V0 = 0x801C0000u;
            goto L80014A78;
        }
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x685Fu;
        goto L80014B60;
        L80014A78: ;
        c.V1 = 0x00900000u;
        c.V1 = c.V1 | 0x500Cu;
        c.A0 = c.S0 + 0x35Cu;
        c.V0 = 0x0000150Cu;
        MemoryAccess.WriteU16(m, (c.S0 + 0x48u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.S0 + 0x5Eu), (ushort)0u);
        MemoryAccess.WriteU32(m, (c.A0 + 0x10u), c.V0);
        MemoryAccess.WriteU32(m, (c.A0 + 0xCu), c.V1);
        c.RA = 0x80014A9Cu;
        GranTurismo2PC.func_8006C04C(c, m);
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x68AEu;
        MemoryAccess.WriteU32(m, (c.S0 + 0x24u), c.V0);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x10E4u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x28u), c.V0);
        c.RA = 0x80014AB8u;
        GranTurismo2PC.func_80013430(c, m);
        c.A0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x394u), c.S3);
        c.RA = 0x80014AC4u;
        GranTurismo2PC.func_80013E14(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 == 0u) {
            c.V0 = c.S1 + 0u;
            goto L80014B7C;
        }
        c.V0 = c.S1 + 0u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x394u), 0u);
        goto L80014B7C;
        L80014AD8: ;
        c.A0 = 0x00000001u;
        c.RA = 0x80014AE0u;
        GranTurismo2PC.func_80013E14(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 == 0u) {
            goto L80014AF0;
        }
        MemoryAccess.WriteU32(m, (c.S0 + 0x394u), 0u);
        L80014AF0: ;
        c.RA = 0x80014AF8u;
        GranTurismo2PC.func_8007D7CC(c, m);
        if (c.V0 == 0u) {
            goto L80014B20;
        }
        if (c.V0 != c.S2) {
            c.V0 = 0x801C0000u;
            goto L80014B58;
        }
        c.V0 = 0x801C0000u;
        c.V0 = 0x801F0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0x6D8u));
        c.A0 = c.S0 + 0x35Cu;
        c.RA = 0x80014B18u;
        GranTurismo2PC.func_8006C074(c, m);
        c.V0 = c.S1 + 0u;
        goto L80014B7C;
        L80014B20: ;
        MemoryAccess.WriteU16(m, (c.S0 + 0x390u), (ushort)c.S1);
        c.A0 = c.S0 + 0x48Cu;
        c.RA = 0x80014B2Cu;
        GranTurismo2PC.func_800691DC(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 == 0u) {
            c.V0 = 0x801C0000u;
            goto L80014B40;
        }
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6809u;
        goto L80014B60;
        L80014B40: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x394u));
        if (c.V0 != 0u) {
            c.S1 = 0x0000000Au;
            goto L80014B78;
        }
        c.S1 = 0x0000000Au;
        c.S1 = 0x00000002u;
        goto L80014B78;
        L80014B58: ;
        c.V0 = c.V0 - 0x685Fu;
        MemoryAccess.WriteU16(m, (c.S0 + 0x390u), (ushort)c.S1);
        L80014B60: ;
        MemoryAccess.WriteU32(m, (c.S0 + 0x20u), c.V0);
        c.RA = 0x80014B68u;
        GranTurismo2PC.func_80013480(c, m);
        c.S1 = 0x00000001u;
        goto L80014B78;
        L80014B70: ;
        c.A0 = c.S0 + 0x35Cu;
        c.RA = 0x80014B78u;
        GranTurismo2PC.func_8006C174(c, m);
        L80014B78: ;
        c.V0 = c.S1 + 0u;
        L80014B7C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80014B98(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = 0xFFFFFFFFu;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
        c.V0 = 0x00000001u;
        if (c.A0 == c.V0) {
            MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
            goto L80014C68;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L80014BE0;
        }
        c.V0 = 0x00000002u;
        if (c.A0 == 0u) {
            c.V0 = c.S1 + 0u;
            goto L80014BF0;
        }
        c.V0 = c.S1 + 0u;
        goto L80014D48;
        L80014BE0: ;
        if (c.A0 == c.V0) {
            c.V0 = c.S1 + 0u;
            goto L80014D2C;
        }
        c.V0 = c.S1 + 0u;
        goto L80014D48;
        L80014BF0: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x394u));
        if (c.V0 == 0u) {
            c.A0 = c.S0 + 0x1A1Cu;
            goto L80014D08;
        }
        c.A0 = c.S0 + 0x1A1Cu;
        c.A1 = 0u + 0u;
        c.RA = 0x80014C08u;
        GranTurismo2PC.func_800697AC(c, m);
        if (c.V0 != 0u) {
            c.V0 = 0x801C0000u;
            goto L80014D18;
        }
        c.V0 = 0x801C0000u;
        c.RA = 0x80014C18u;
        GranTurismo2PC.func_800136EC(c, m);
        c.V1 = 0x00900000u;
        c.V1 = c.V1 | 0x500Cu;
        c.A0 = c.S0 + 0x35Cu;
        c.V0 = 0x0000150Cu;
        MemoryAccess.WriteU32(m, (c.A0 + 0x10u), c.V0);
        MemoryAccess.WriteU32(m, (c.A0 + 0xCu), c.V1);
        c.RA = 0x80014C34u;
        GranTurismo2PC.func_8006C04C(c, m);
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x68D2u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x24u), c.V0);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x10E4u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x28u), c.V0);
        c.RA = 0x80014C50u;
        GranTurismo2PC.func_80013430(c, m);
        c.A0 = 0x00000001u;
        c.RA = 0x80014C58u;
        GranTurismo2PC.func_80013E14(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 == 0u) {
            goto L80014C68;
        }
        MemoryAccess.WriteU32(m, (c.S0 + 0x394u), 0u);
        L80014C68: ;
        c.A0 = 0x00000001u;
        c.RA = 0x80014C70u;
        GranTurismo2PC.func_80013E14(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 == 0u) {
            goto L80014C80;
        }
        MemoryAccess.WriteU32(m, (c.S0 + 0x394u), 0u);
        L80014C80: ;
        c.RA = 0x80014C88u;
        GranTurismo2PC.func_8007D7CC(c, m);
        c.V1 = c.V0 + 0u;
        if (c.V1 == 0u) {
            c.V0 = 0x00000001u;
            goto L80014CB4;
        }
        c.V0 = 0x00000001u;
        if (c.V1 != c.V0) {
            c.V0 = 0xFFFFFFFFu;
            goto L80014D10;
        }
        c.V0 = 0xFFFFFFFFu;
        c.V0 = 0x801F0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0x6D8u));
        c.A0 = c.S0 + 0x35Cu;
        c.RA = 0x80014CACu;
        GranTurismo2PC.func_8006C074(c, m);
        c.V0 = c.S1 + 0u;
        goto L80014D48;
        L80014CB4: ;
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU16(m, (c.S0 + 0x390u), (ushort)c.V0);
        c.S1 = c.S0 + 0x1A1Cu;
        c.A0 = c.S1 + 0u;
        c.RA = 0x80014CC8u;
        GranTurismo2PC.func_800691DC(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 == 0u) {
            c.V0 = 0x801C0000u;
            goto L80014CDC;
        }
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6834u;
        goto L80014D1C;
        L80014CDC: ;
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x10u), (ushort)c.V0);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0x200u));
        if (c.V0 != 0u) {
            c.S1 = 0x0000000Bu;
            goto L80014CF8;
        }
        c.S1 = 0x0000000Bu;
        c.S1 = 0x00000014u;
        L80014CF8: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x394u));
        if (c.V0 != 0u) {
            MemoryAccess.WriteU16(m, (c.S0 + 0x48Au), (ushort)0u);
            goto L80014D44;
        }
        MemoryAccess.WriteU16(m, (c.S0 + 0x48Au), (ushort)0u);
        L80014D08: ;
        c.S1 = 0x00000002u;
        goto L80014D44;
        L80014D10: ;
        MemoryAccess.WriteU16(m, (c.S0 + 0x390u), (ushort)c.V0);
        c.V0 = 0x801C0000u;
        L80014D18: ;
        c.V0 = c.V0 - 0x688Au;
        L80014D1C: ;
        MemoryAccess.WriteU32(m, (c.S0 + 0x20u), c.V0);
        c.RA = 0x80014D24u;
        GranTurismo2PC.func_80013480(c, m);
        c.S1 = 0x00000001u;
        goto L80014D44;
        L80014D2C: ;
        c.A0 = c.S2 + 0u;
        c.A1 = 0u + 0u;
        c.RA = 0x80014D38u;
        GranTurismo2PC.func_80013728(c, m);
        c.A0 = c.S0 + 0x35Cu;
        c.A1 = c.S2 + 0u;
        c.RA = 0x80014D44u;
        GranTurismo2PC.func_8006C174(c, m);
        L80014D44: ;
        c.V0 = c.S1 + 0u;
        L80014D48: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80014D60(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x68u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x48u), c.S2);
        c.S2 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
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
            goto L80014E98;
        }
        c.S5 = 0xFFFFFFFFu;
        c.V0 = (int)c.S4 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L80014DC8;
        }
        c.V0 = 0x00000002u;
        if (c.S4 == 0u) {
            c.V0 = c.S5 + 0u;
            goto L80014DD8;
        }
        c.V0 = c.S5 + 0u;
        goto L8001512C;
        L80014DC8: ;
        if (c.S4 == c.V0) {
            c.V0 = c.S5 + 0u;
            goto L80014FE4;
        }
        c.V0 = c.S5 + 0u;
        goto L8001512C;
        L80014DD8: ;
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x10E4u;
        MemoryAccess.WriteU32(m, (c.S2 + 0x24u), c.V0);
        MemoryAccess.WriteU32(m, (c.S2 + 0x28u), c.V0);
        c.RA = 0x80014DECu;
        GranTurismo2PC.func_80013430(c, m);
        c.S1 = 0u + 0u;
        c.S0 = c.S1 + 0u;
        c.V0 = 0x80050000u;
        c.V1 = c.S3 + 0x1u;
        MemoryAccess.WriteU16(m, (c.V0 - 0x48DCu), (ushort)c.V1);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x48Au));
        c.V0 = c.V0 - 0x48DCu;
        MemoryAccess.WriteU16(m, (c.V0 + 0x6u), (ushort)c.V1);
        L80014E0C: ;
        c.V0 = (int)c.S1 < (int)c.S3 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S0 + c.S2;
            goto L80014E48;
        }
        c.V0 = c.S0 + c.S2;
        c.A0 = c.S1 + 0u;
        c.A1 = c.SP + 0x38u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x398u), (ushort)0u);
        c.RA = 0x80014E28u;
        GranTurismo2PC.func_800135E0(c, m);
        c.V1 = c.S0 + c.S2;
        c.A0 = c.V1 + 0u;
        c.S0 = c.S0 + 0x6u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0x38u));
        c.S1 = c.S1 + 0x1u;
        MemoryAccess.WriteU16(m, (c.V1 + 0x39Au), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.A0 + 0x39Cu), (ushort)c.A1);
        goto L80014E0C;
        L80014E48: ;
        c.S0 = c.S1 << 1;
        c.S0 = c.S0 + c.S1;
        c.S0 = c.S0 << 1;
        c.V0 = c.S0 + c.S2;
        c.V1 = 0x00000002u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x398u), (ushort)c.V1);
        c.RA = 0x80014E64u;
        GranTurismo2PC.func_80013518(c, m);
        c.V1 = c.S0 + c.S2;
        c.S0 = c.V1 + 0u;
        c.A0 = 0x00000001u;
        c.V0 = (int)0u < (int)c.V0 ? 1u : 0u;
        MemoryAccess.WriteU16(m, (c.V1 + 0x39Au), (ushort)c.V0);
        c.V0 = 0x00000003u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x39Cu), (ushort)c.V0);
        c.RA = 0x80014E84u;
        GranTurismo2PC.func_80013E14(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 == 0u) {
            c.V0 = c.S5 + 0u;
            goto L8001512C;
        }
        c.V0 = c.S5 + 0u;
        MemoryAccess.WriteU32(m, (c.S2 + 0x394u), 0u);
        goto L8001512C;
        L80014E98: ;
        c.A0 = 0x00000001u;
        c.RA = 0x80014EA0u;
        GranTurismo2PC.func_80013E14(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 == 0u) {
            goto L80014EB0;
        }
        MemoryAccess.WriteU32(m, (c.S2 + 0x394u), 0u);
        L80014EB0: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S2 + 0x394u));
        if (c.V0 != 0u) {
            c.A0 = 0x80050000u;
            goto L80014ED0;
        }
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x48DCu;
        c.RA = 0x80014EC8u;
        GranTurismo2PC.func_8006CED8(c, m);
        c.S5 = 0x00000002u;
        goto L80015128;
        L80014ED0: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x10u));
        c.V1 = MemoryAccess.ReadU16(m, (c.S2 + 0x10u));
        if ((int)c.V0 <= 0) {
            c.V0 = c.V1 - 0x1u;
            goto L80014F04;
        }
        c.V0 = c.V1 - 0x1u;
        MemoryAccess.WriteU16(m, (c.S2 + 0x10u), (ushort)c.V0);
        c.V0 = c.V0 << 16;
        if (c.V0 != 0u) {
            c.V0 = 0xFFFFFFFDu;
            goto L80014F08;
        }
        c.V0 = 0xFFFFFFFDu;
        c.A0 = 0x00000007u;
        c.RA = 0x80014EF8u;
        GranTurismo2PC.func_80060840(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x48DCu;
        c.RA = 0x80014F04u;
        GranTurismo2PC.func_8006CE70(c, m);
        L80014F04: ;
        c.V0 = 0xFFFFFFFDu;
        L80014F08: ;
        if (c.S0 == c.V0) {
            c.V0 = (int)c.S0 < -2 ? 1u : 0u;
            goto L80014F54;
        }
        c.V0 = (int)c.S0 < -2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0xFFFFFFFCu;
            goto L80014F28;
        }
        c.V0 = 0xFFFFFFFCu;
        if (c.S0 == c.V0) {
            goto L80014F64;
        }
        goto L80014F74;
        L80014F28: ;
        c.V0 = 0xFFFFFFFEu;
        if (c.S0 == c.V0) {
            c.V0 = 0xFFFFFFFFu;
            goto L80015128;
        }
        c.V0 = 0xFFFFFFFFu;
        if (c.S0 != c.V0) {
            c.A0 = 0x80050000u;
            goto L80014F74;
        }
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x48DCu;
        c.RA = 0x80014F44u;
        GranTurismo2PC.func_8006CED8(c, m);
        c.A0 = 0x00000002u;
        c.RA = 0x80014F4Cu;
        GranTurismo2PC.func_80060840(c, m);
        c.S5 = 0x00000002u;
        goto L80015128;
        L80014F54: ;
        c.A0 = 0x00000005u;
        c.RA = 0x80014F5Cu;
        GranTurismo2PC.func_80060840(c, m);
        c.V0 = c.S5 + 0u;
        goto L8001512C;
        L80014F64: ;
        c.A0 = 0u + 0u;
        c.RA = 0x80014F6Cu;
        GranTurismo2PC.func_80060840(c, m);
        c.V0 = c.S5 + 0u;
        goto L8001512C;
        L80014F74: ;
        c.A0 = 0x00000001u;
        c.RA = 0x80014F7Cu;
        GranTurismo2PC.func_80060840(c, m);
        c.V0 = c.S0 << 1;
        c.V0 = c.V0 + c.S0;
        c.V0 = c.V0 << 1;
        c.V0 = c.V0 + c.S2;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x398u));
        if (c.V0 != 0u) {
            c.V1 = 0xFFFFFFFFu;
            goto L80014FA0;
        }
        c.V1 = 0xFFFFFFFFu;
        c.V1 = c.S0 + 0u;
        L80014FA0: ;
        if ((int)c.V1 < 0) {
            goto L80014FC8;
        }
        c.A0 = 0x00000001u;
        c.RA = 0x80014FB0u;
        GranTurismo2PC.func_80060840(c, m);
        c.A0 = c.S0 + 0u;
        c.RA = 0x80014FB8u;
        GranTurismo2PC.func_80013694(c, m);
        c.S5 = 0x0000000Bu;
        MemoryAccess.WriteU16(m, (c.S2 + 0x10u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.S2 + 0x48Au), (ushort)c.S0);
        goto L80015128;
        L80014FC8: ;
        c.A0 = 0x00000001u;
        c.RA = 0x80014FD0u;
        GranTurismo2PC.func_80060840(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x48DCu;
        c.RA = 0x80014FDCu;
        GranTurismo2PC.func_8006CED8(c, m);
        c.S5 = 0x0000000Cu;
        goto L80015128;
        L80014FE4: ;
        c.A0 = c.S6 + 0u;
        c.A1 = 0u + 0u;
        c.RA = 0x80014FF0u;
        GranTurismo2PC.func_80013728(c, m);
        c.A0 = c.S6 + 0u;
        c.RA = 0x80014FF8u;
        GranTurismo2PC.func_800139C0(c, m);
        c.V0 = 0x80020000u;
        c.S1 = c.V0 + 0x10E4u;
        c.V0 = 0x80050000u;
        c.S0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 - 0x48D6u));
        c.S3 = 0x00640000u;
        c.V0 = c.S0 << 1;
        c.V0 = c.V0 + c.S0;
        c.V0 = c.V0 << 1;
        c.V0 = c.V0 + c.S2;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x39Cu));
        if (c.V1 == c.S7) {
            c.S3 = c.S3 | 0x6464u;
            goto L8001508C;
        }
        c.S3 = c.S3 | 0x6464u;
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L80015048;
        }
        if (c.V1 == 0u) {
            c.V0 = 0x801D0000u;
            goto L80015074;
        }
        c.V0 = 0x801D0000u;
        goto L80015098;
        L80015048: ;
        if (c.V1 == c.S4) {
            c.V0 = 0x00000003u;
            goto L80015080;
        }
        c.V0 = 0x00000003u;
        if (c.V1 != c.V0) {
            c.V0 = 0x801D0000u;
            goto L80015098;
        }
        c.V0 = 0x801D0000u;
        c.V0 = 0x801C0000u;
        c.S1 = c.V0 - 0x5BD4u;
        c.RA = 0x80015064u;
        GranTurismo2PC.func_80013518(c, m);
        if ((int)c.V0 <= 0) {
            c.V0 = 0x801D0000u;
            goto L80015098;
        }
        c.V0 = 0x801D0000u;
        c.S1 = c.S1 - 0x15u;
        goto L80015098;
        L80015074: ;
        c.V0 = 0x801C0000u;
        c.S1 = c.V0 - 0x5BF8u;
        goto L80015094;
        L80015080: ;
        c.V0 = 0x801F0000u;
        c.S1 = c.V0 - 0x2B0u;
        goto L80015094;
        L8001508C: ;
        c.V0 = 0x801F0000u;
        c.S1 = c.V0 - 0x29Fu;
        L80015094: ;
        c.V0 = 0x801D0000u;
        L80015098: ;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6720u));
        if (c.V0 != 0u) {
            c.A0 = c.S6 + 0u;
            goto L800150C4;
        }
        c.A0 = c.S6 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S3);
        c.A1 = c.S1 + 0u;
        c.A2 = 0x000000B0u;
        c.A3 = 0x000001A8u;
        c.RA = 0x800150BCu;
        GranTurismo2PC.func_8001330C(c, m);
        c.V0 = c.S5 + 0u;
        goto L8001512C;
        L800150C4: ;
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x2Cu));
        c.A0 = c.SP + 0x18u;
        c.RA = 0x800150D0u;
        GranTurismo2PC.func_8006AC68(c, m);
        c.A0 = c.SP + 0x18u;
        c.A1 = 0x800B0000u;
        c.A1 = c.A1 - 0x7220u;
        c.RA = 0x800150E0u;
        GranTurismo2PC.func_8007DA80(c, m);
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
        c.RA = 0x80015128u;
        GranTurismo2PC.func_8006ADB4(c, m);
        L80015128: ;
        c.V0 = c.S5 + 0u;
        L8001512C: ;
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
    public static void func_80015158(CpuContext c, IMemory m)
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
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
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
        L800151AC: ;
        c.V0 = (int)c.A1 < 32 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S0 + c.A1;
            goto L80015264;
        }
        c.V0 = c.S0 + c.A1;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V0 + 0x2FA8u));
        if (c.V0 == 0u) {
            c.V0 = c.T1 + c.T6;
            goto L80015258;
        }
        c.V0 = c.T1 + c.T6;
        MemoryAccess.WriteU16(m, (c.V0 + 0x2FD8u), (ushort)c.A1);
        c.V0 = c.T2 + c.T6;
        c.V0 = c.V0 + 0x3260u;
        c.V1 = c.T4 + c.A2;
        c.A0 = c.V1 + 0x50u;
        L800151DC: ;
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
            goto L800151DC;
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
        L80015258: ;
        c.A2 = c.A2 + 0x5Cu;
        c.A1 = c.A1 + 0x1u;
        goto L800151AC;
        L80015264: ;
        c.V0 = 0x00020000u;
        c.V0 = c.S0 + c.V0;
        MemoryAccess.WriteU16(m, (c.V0 + 0x2FD2u), (ushort)c.T0);
        if (c.T0 != 0u) {
            MemoryAccess.WriteU32(m, (c.V0 + 0x2FD4u), c.A3);
            goto L80015280;
        }
        MemoryAccess.WriteU32(m, (c.V0 + 0x2FD4u), c.A3);
        c.V0 = 0x00000002u;
        goto L800152EC;
        L80015280: ;
        if ((int)c.T0 <= 0) {
            c.A1 = 0u + 0u;
            goto L800152B4;
        }
        c.A1 = 0u + 0u;
        c.A2 = 0x00020000u;
        c.A0 = c.S0 + 0u;
        L80015290: ;
        c.V1 = c.A0 + c.A2;
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x30B0u));
        c.A1 = c.A1 + 0x1u;
        c.T5 = c.T5 + c.V0;
        c.V0 = c.A3 - c.T5;
        MemoryAccess.WriteU32(m, (c.V1 + 0x31D0u), c.V0);
        c.V0 = (int)c.A1 < (int)c.T0 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.A0 = c.A0 + 0x4u;
            goto L80015290;
        }
        c.A0 = c.A0 + 0x4u;
        L800152B4: ;
        c.V0 = 0x00500000u;
        c.V0 = c.V0 | 0x782Cu;
        c.A0 = c.S0 + 0x35Cu;
        MemoryAccess.WriteU32(m, (c.A0 + 0x10u), c.A3);
        MemoryAccess.WriteU32(m, (c.A0 + 0xCu), c.V0);
        c.RA = 0x800152CCu;
        GranTurismo2PC.func_8006C04C(c, m);
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x607Cu;
        MemoryAccess.WriteU32(m, (c.S0 + 0x24u), c.V0);
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0x8BCu;
        MemoryAccess.WriteU32(m, (c.S0 + 0x28u), c.V0);
        c.RA = 0x800152E8u;
        GranTurismo2PC.func_80013430(c, m);
        c.V0 = 0x0000000Du;
        L800152EC: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80015304(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x38u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S2);
        c.S2 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.S6);
        c.S6 = c.A1 + 0u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
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
            goto L80015408;
        }
        c.S1 = 0xFFFFFFFFu;
        c.V0 = (int)c.S2 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L80015398;
        }
        c.V0 = 0x00000002u;
        if (c.S2 == 0u) {
            c.V0 = c.S1 + 0u;
            goto L800153A8;
        }
        c.V0 = c.S1 + 0u;
        goto L800154AC;
        L80015398: ;
        if (c.S2 == c.V0) {
            c.V0 = c.S1 + 0u;
            goto L80015490;
        }
        c.V0 = c.S1 + 0u;
        goto L800154AC;
        L800153A8: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x394u));
        if (c.V0 != 0u) {
            c.V0 = 0x00020000u;
            goto L800153C4;
        }
        c.V0 = 0x00020000u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x390u), (ushort)c.S1);
        c.S1 = 0x00000002u;
        goto L800154A8;
        L800153C4: ;
        c.V0 = c.V0 | 0x3F50u;
        c.V0 = c.S0 + c.V0;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.A0 = c.S0 + 0x1A1Cu;
        c.A1 = 0u + 0u;
        c.A2 = c.S4 + 0u;
        c.A3 = c.S5 + 0u;
        c.RA = 0x800153E4u;
        GranTurismo2PC.func_800697E8(c, m);
        if (c.V0 != 0u) {
            c.V0 = 0x801C0000u;
            goto L80015478;
        }
        c.V0 = 0x801C0000u;
        c.A0 = 0x00000001u;
        c.RA = 0x800153F4u;
        GranTurismo2PC.func_80013E14(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 == 0u) {
            c.V0 = c.S1 + 0u;
            goto L800154AC;
        }
        c.V0 = c.S1 + 0u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x394u), 0u);
        goto L800154AC;
        L80015408: ;
        c.A0 = 0x00000001u;
        c.RA = 0x80015410u;
        GranTurismo2PC.func_80013E14(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 == 0u) {
            goto L80015420;
        }
        MemoryAccess.WriteU32(m, (c.S0 + 0x394u), 0u);
        L80015420: ;
        c.RA = 0x80015428u;
        GranTurismo2PC.func_8007D7CC(c, m);
        if (c.V0 == 0u) {
            c.A0 = c.S0 + 0x1A1Cu;
            goto L80015458;
        }
        c.A0 = c.S0 + 0x1A1Cu;
        if (c.V0 != c.S2) {
            c.V0 = 0x801C0000u;
            goto L80015478;
        }
        c.V0 = 0x801C0000u;
        c.V0 = 0x801F0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x6D8u));
        c.A1 = MemoryAccess.ReadU32(m, (c.S3 + 0x31D0u));
        c.A0 = c.S0 + 0x35Cu;
        c.A1 = c.V0 + c.A1;
        c.RA = 0x80015450u;
        GranTurismo2PC.func_8006C074(c, m);
        c.V0 = c.S1 + 0u;
        goto L800154AC;
        L80015458: ;
        c.A1 = c.S4 + 0u;
        c.A2 = c.S5 + 0u;
        c.RA = 0x80015464u;
        GranTurismo2PC.func_800692DC(c, m);
        if (c.V0 != 0u) {
            c.S1 = 0x0000000Eu;
            goto L800154A8;
        }
        c.S1 = 0x0000000Eu;
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6834u;
        goto L80015480;
        L80015478: ;
        c.V0 = c.V0 - 0x688Au;
        MemoryAccess.WriteU16(m, (c.S0 + 0x390u), (ushort)c.S1);
        L80015480: ;
        MemoryAccess.WriteU32(m, (c.S0 + 0x20u), c.V0);
        c.RA = 0x80015488u;
        GranTurismo2PC.func_80013480(c, m);
        c.S1 = 0x00000001u;
        goto L800154A8;
        L80015490: ;
        c.A0 = c.S6 + 0u;
        c.A1 = 0u + 0u;
        c.RA = 0x8001549Cu;
        GranTurismo2PC.func_80013728(c, m);
        c.A0 = c.S0 + 0x35Cu;
        c.A1 = c.S6 + 0u;
        c.RA = 0x800154A8u;
        GranTurismo2PC.func_8006C174(c, m);
        L800154A8: ;
        c.V0 = c.S1 + 0u;
        L800154AC: ;
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
    public static void func_800154D4(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x394u));
        if (c.V0 != 0u) {
            c.A0 = 0x0000000Fu;
            goto L80015500;
        }
        c.A0 = 0x0000000Fu;
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU16(m, (c.V1 + 0x390u), (ushort)c.V0);
        c.A0 = 0x00000002u;
        goto L80015534;
        L80015500: ;
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
            goto L80015534;
        }
        c.A0 = 0x0000000Du;
        L80015534: ;
        c.V0 = c.A0 + 0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001553C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V1 = 0x000C0000u;
        c.V1 = c.V1 | 0x5090u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
        c.V0 = 0x00020000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.V0 = c.S0 + c.V0;
        MemoryAccess.WriteU16(m, (c.V0 + 0x46D2u), (ushort)0u);
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x2FD4u));
        c.A0 = c.S0 + 0x35Cu;
        MemoryAccess.WriteU32(m, (c.A0 + 0xCu), c.V1);
        c.V0 = c.V0 + 0x150Cu;
        MemoryAccess.WriteU32(m, (c.A0 + 0x10u), c.V0);
        c.RA = 0x8001557Cu;
        GranTurismo2PC.func_8006C04C(c, m);
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6055u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x24u), c.V0);
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0x8BCu;
        MemoryAccess.WriteU32(m, (c.S0 + 0x28u), c.V0);
        c.RA = 0x80015598u;
        GranTurismo2PC.func_80013430(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = 0x00000010u;
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800155AC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x38u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S2);
        c.S2 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.S6);
        c.S6 = c.A1 + 0u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
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
            goto L800156E4;
        }
        c.S3 = 0xFFFFFFFFu;
        c.V0 = (int)c.S2 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L80015634;
        }
        c.V0 = 0x00000002u;
        if (c.S2 == 0u) {
            c.V0 = c.S3 + 0u;
            goto L80015644;
        }
        c.V0 = c.S3 + 0u;
        goto L8001575C;
        L80015634: ;
        if (c.S2 == c.V0) {
            c.V0 = c.S3 + 0u;
            goto L80015740;
        }
        c.V0 = c.S3 + 0u;
        goto L8001575C;
        L80015644: ;
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
        c.RA = 0x80015690u;
        GranTurismo2PC.func_80069418(c, m);
        if (c.V0 != 0u) {
            c.V0 = 0x801C0000u;
            goto L800156A0;
        }
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6809u;
        goto L80015730;
        L800156A0: ;
        c.A0 = c.S1 + 0u;
        c.RA = 0x800156A8u;
        GranTurismo2PC.func_800691DC(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 == 0u) {
            c.V0 = 0x801C0000u;
            goto L800156BC;
        }
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6809u;
        goto L80015730;
        L800156BC: ;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S2);
        c.A0 = c.S1 + 0u;
        c.A1 = 0x00000001u;
        c.A2 = c.S5 + 0u;
        c.A3 = c.S4 + 0u;
        c.RA = 0x800156D4u;
        GranTurismo2PC.func_80069758(c, m);
        if (c.V0 == 0u) {
            c.V0 = 0x801C0000u;
            goto L80015758;
        }
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6769u;
        goto L80015730;
        L800156E4: ;
        c.RA = 0x800156ECu;
        GranTurismo2PC.func_8007D7CC(c, m);
        if (c.V0 == 0u) {
            goto L80015720;
        }
        if (c.V0 != c.S2) {
            c.V0 = 0x801C0000u;
            goto L80015728;
        }
        c.V0 = 0x801C0000u;
        c.V0 = 0x801F0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0x6D8u));
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x31D0u));
        c.A0 = c.S0 + 0x35Cu;
        c.A1 = c.A1 + c.V0;
        c.A1 = c.A1 + 0x150Cu;
        c.RA = 0x80015718u;
        GranTurismo2PC.func_8006C074(c, m);
        c.V0 = c.S3 + 0u;
        goto L8001575C;
        L80015720: ;
        c.S3 = 0x00000011u;
        goto L80015758;
        L80015728: ;
        c.V0 = c.V0 - 0x6769u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x390u), (ushort)c.S3);
        L80015730: ;
        MemoryAccess.WriteU32(m, (c.S0 + 0x20u), c.V0);
        c.RA = 0x80015738u;
        GranTurismo2PC.func_80013480(c, m);
        c.S3 = 0x00000001u;
        goto L80015758;
        L80015740: ;
        c.A0 = c.S6 + 0u;
        c.A1 = 0x00000001u;
        c.RA = 0x8001574Cu;
        GranTurismo2PC.func_80013728(c, m);
        c.A0 = c.S0 + 0x35Cu;
        c.A1 = c.S6 + 0u;
        c.RA = 0x80015758u;
        GranTurismo2PC.func_8006C174(c, m);
        L80015758: ;
        c.V0 = c.S3 + 0u;
        L8001575C: ;
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
    public static void func_80015784(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x2u));
        if (c.V0 == 0u) {
            c.A0 = 0x00000012u;
            goto L800157C4;
        }
        c.A0 = 0x00000012u;
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6769u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x20u), c.V0);
        c.RA = 0x800157BCu;
        GranTurismo2PC.func_80013480(c, m);
        c.A0 = 0x00000001u;
        goto L800157F8;
        L800157C4: ;
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
            goto L800157F8;
        }
        c.A0 = 0x00000010u;
        L800157F8: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = c.A0 + 0u;
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80015808(CpuContext c, IMemory m)
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
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
        c.V0 = 0x00000001u;
        if (c.S1 == c.V0) {
            MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
            goto L800158B0;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        c.V0 = (int)c.S1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L80015858;
        }
        c.V0 = 0x00000002u;
        if (c.S1 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L80015868;
        }
        c.V0 = c.S2 + 0u;
        goto L80015920;
        L80015858: ;
        if (c.S1 == c.V0) {
            c.V0 = c.S2 + 0u;
            goto L80015904;
        }
        c.V0 = c.S2 + 0u;
        goto L80015920;
        L80015868: ;
        c.A0 = c.S0 + 0x48Cu;
        c.A1 = 0x00000001u;
        c.RA = 0x80015874u;
        GranTurismo2PC.func_8006971C(c, m);
        if (c.V0 == 0u) {
            c.V0 = 0x801C0000u;
            goto L8001588C;
        }
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6769u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x20u), c.V0);
        c.RA = 0x80015888u;
        GranTurismo2PC.func_80013480(c, m);
        c.S2 = 0x00000001u;
        L8001588C: ;
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6055u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x24u), c.V0);
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0x8BCu;
        MemoryAccess.WriteU32(m, (c.S0 + 0x28u), c.V0);
        c.RA = 0x800158A8u;
        GranTurismo2PC.func_80013430(c, m);
        c.V0 = c.S2 + 0u;
        goto L80015920;
        L800158B0: ;
        c.RA = 0x800158B8u;
        GranTurismo2PC.func_8007D7CC(c, m);
        if (c.V0 == 0u) {
            goto L800158E0;
        }
        if (c.V0 != c.S1) {
            c.V0 = 0x801C0000u;
            goto L800158EC;
        }
        c.V0 = 0x801C0000u;
        c.V0 = 0x801F0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0x6D8u));
        c.A0 = c.S0 + 0x35Cu;
        c.RA = 0x800158D8u;
        GranTurismo2PC.func_8006C074(c, m);
        c.V0 = c.S2 + 0u;
        goto L80015920;
        L800158E0: ;
        MemoryAccess.WriteU16(m, (c.S0 + 0x390u), (ushort)c.S2);
        c.S2 = 0x00000013u;
        goto L8001591C;
        L800158EC: ;
        c.V0 = c.V0 - 0x6769u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x390u), (ushort)c.S2);
        MemoryAccess.WriteU32(m, (c.S0 + 0x20u), c.V0);
        c.RA = 0x800158FCu;
        GranTurismo2PC.func_80013480(c, m);
        c.S2 = 0x00000001u;
        goto L8001591C;
        L80015904: ;
        c.A0 = c.S3 + 0u;
        c.A1 = 0x00000001u;
        c.RA = 0x80015910u;
        GranTurismo2PC.func_80013728(c, m);
        c.A0 = c.S0 + 0x35Cu;
        c.A1 = c.S3 + 0u;
        c.RA = 0x8001591Cu;
        GranTurismo2PC.func_8006C174(c, m);
        L8001591C: ;
        c.V0 = c.S2 + 0u;
        L80015920: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001593C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x354u));
        if (c.A0 == c.V0) {
            c.S2 = 0xFFFFFFFFu;
            goto L800159D8;
        }
        c.S2 = 0xFFFFFFFFu;
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L80015A18;
        }
        c.V0 = c.S2 + 0u;
        if (c.A0 != 0u) {
            c.V1 = c.S1 + 0x30u;
            goto L80015A18;
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
        c.RA = 0x800159A4u;
        GranTurismo2PC.func_80060840(c, m);
        c.S0 = c.S1 + 0x2C0u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x800159B0u;
        GranTurismo2PC.func_8006E388(c, m);
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x66E4u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x7Du), (byte)0u);
        MemoryAccess.WriteU32(m, (c.S1 + 0x24u), c.V0);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x10E4u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        c.RA = 0x800159D0u;
        GranTurismo2PC.func_80013430(c, m);
        c.V0 = c.S2 + 0u;
        goto L80015A18;
        L800159D8: ;
        if (c.V1 == 0u) {
            goto L80015A08;
        }
        if ((int)c.V1 > 0) {
            goto L800159F8;
        }
        if (c.V1 == c.S2) {
            c.V0 = c.S2 + 0u;
            goto L80015A00;
        }
        c.V0 = c.S2 + 0u;
        goto L80015A18;
        L800159F8: ;
        if (c.V1 != c.A0) {
            c.V0 = c.S2 + 0u;
            goto L80015A18;
        }
        c.V0 = c.S2 + 0u;
        L80015A00: ;
        c.S2 = 0x00000016u;
        goto L80015A14;
        L80015A08: ;
        c.A0 = 0x00000001u;
        c.RA = 0x80015A10u;
        GranTurismo2PC.func_80060840(c, m);
        c.S2 = 0x00000002u;
        L80015A14: ;
        c.V0 = c.S2 + 0u;
        L80015A18: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80015A30(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x18Cu));
        if (c.A0 == c.V0) {
            c.S2 = 0xFFFFFFFFu;
            goto L80015AA8;
        }
        c.S2 = 0xFFFFFFFFu;
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L80015AE8;
        }
        c.V0 = c.S2 + 0u;
        if (c.A0 != 0u) {
            goto L80015AE8;
        }
        c.A0 = 0u + 0u;
        c.RA = 0x80015A78u;
        GranTurismo2PC.func_80060840(c, m);
        c.S0 = c.S1 + 0xF8u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x80015A84u;
        GranTurismo2PC.func_8006E388(c, m);
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0x4C7u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x7Du), (byte)0u);
        MemoryAccess.WriteU32(m, (c.S1 + 0x24u), c.V0);
        c.V0 = c.V0 - 0x117u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        c.RA = 0x80015AA0u;
        GranTurismo2PC.func_80013430(c, m);
        c.V0 = c.S2 + 0u;
        goto L80015AE8;
        L80015AA8: ;
        if (c.V1 == 0u) {
            goto L80015AD8;
        }
        if ((int)c.V1 > 0) {
            goto L80015AC8;
        }
        if (c.V1 == c.S2) {
            c.V0 = c.S2 + 0u;
            goto L80015AD0;
        }
        c.V0 = c.S2 + 0u;
        goto L80015AE8;
        L80015AC8: ;
        if (c.V1 != c.A0) {
            c.V0 = c.S2 + 0u;
            goto L80015AE8;
        }
        c.V0 = c.S2 + 0u;
        L80015AD0: ;
        c.S2 = 0x00000016u;
        goto L80015AE4;
        L80015AD8: ;
        c.A0 = 0x00000001u;
        c.RA = 0x80015AE0u;
        GranTurismo2PC.func_80060840(c, m);
        c.S2 = 0x00000002u;
        L80015AE4: ;
        c.V0 = c.S2 + 0u;
        L80015AE8: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80015B00(CpuContext c, IMemory m)
    {
        c.V0 = 0xFFFFFFFFu;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80015B08(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
        c.SP = c.SP - 0x18u;
        if ((int)c.A0 >= 0) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
            goto L80015B28;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V0 = 0x80010000u;
        c.V0 = c.V0 + 0x5B00u;
        goto L80015B40;
        L80015B28: ;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 - 0x4828u;
        c.V1 = c.A0 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        L80015B40: ;
        MemoryAccess.WriteU32(m, (c.A1 + 0x4u), c.V0);
        MemoryAccess.WriteU16(m, c.A1, (ushort)c.A0);
        c.A0 = 0u + 0u;
        c.V0 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.A1 = c.A0 + 0u;
        c.RA = 0x80015B5Cu;
        Dispatcher.Call(c, m, c.V0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80015B6C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x49BCu;
        c.V0 = 0x800B0000u;
        c.A2 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
        c.V0 = 0x80050000u;
        c.V1 = 0xFFFFFFFFu;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.A0 = c.S0 + 0x60u;
        c.T2 = c.V0 - 0x49ECu;
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
        c.T2 = c.V0 - 0x49D0u;
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
        c.RA = 0x80015C14u;
        GranTurismo2PC.func_8006E1CC(c, m);
        c.A0 = c.S0 + 0xF8u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x498Cu;
        c.A2 = 0u + 0u;
        c.RA = 0x80015C28u;
        GranTurismo2PC.func_8006E1CC(c, m);
        c.A0 = c.S0 + 0x190u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x495Cu;
        c.A2 = 0u + 0u;
        c.RA = 0x80015C3Cu;
        GranTurismo2PC.func_8006E1CC(c, m);
        c.A0 = c.S0 + 0x228u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x492Cu;
        c.A2 = 0u + 0u;
        c.RA = 0x80015C50u;
        GranTurismo2PC.func_8006E1CC(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x48FCu;
        c.RA = 0x80015C5Cu;
        GranTurismo2PC.func_8006D9C8(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x48DCu;
        c.A1 = 0x80010000u;
        c.A1 = c.A1 + 0x3AA4u;
        c.A2 = 0u + 0u;
        c.RA = 0x80015C74u;
        GranTurismo2PC.func_8006CDCC(c, m);
        c.A0 = c.S0 + 0x2C0u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x48A8u;
        c.A2 = 0u + 0u;
        c.RA = 0x80015C88u;
        GranTurismo2PC.func_8006E1CC(c, m);
        c.V1 = c.S0 + 0x35Cu;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 - 0x4878u;
        c.A0 = c.V0 + 0x30u;
        L80015C98: ;
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
            goto L80015C98;
        }
        c.V1 = c.V1 + 0x10u;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        MemoryAccess.WriteU32(m, c.V1, c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x4u), c.T0);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x10E4u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x24u), c.V0);
        MemoryAccess.WriteU32(m, (c.S0 + 0x28u), c.V0);
        c.RA = 0x80015CE8u;
        GranTurismo2PC.func_80013430(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80015CF8(CpuContext c, IMemory m)
    {
        c.V0 = MemoryAccess.ReadU32(m, c.A0);
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0xCu));
        c.V1 = 0x800B0000u;
        c.T1 = c.V1 - 0x7200u;
        c.A2 = MemoryAccess.ReadU32(m, c.A1);
        c.A3 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.T0 = MemoryAccess.ReadU32(m, (c.A1 + 0x8u));
        MemoryAccess.WriteU32(m, c.T1, c.A2);
        MemoryAccess.WriteU32(m, (c.T1 + 0x4u), c.A3);
        MemoryAccess.WriteU32(m, (c.T1 + 0x8u), c.T0);
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0x10u));
        c.V1 = 0x800B0000u;
        c.T1 = c.V1 - 0x7220u;
        c.A2 = MemoryAccess.ReadU32(m, c.A1);
        c.A3 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.T0 = MemoryAccess.ReadU32(m, (c.A1 + 0x8u));
        MemoryAccess.WriteU32(m, c.T1, c.A2);
        MemoryAccess.WriteU32(m, (c.T1 + 0x4u), c.A3);
        MemoryAccess.WriteU32(m, (c.T1 + 0x8u), c.T0);
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0x14u));
        c.V1 = 0x800B0000u;
        c.T1 = c.V1 - 0x7210u;
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
        MemoryAccess.WriteU32(m, (c.V1 - 0x7228u), c.V0);
        MemoryAccess.WriteU16(m, (c.V0 + 0x2Cu), (ushort)c.A0);
        c.V0 = c.V0 + c.A1;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80015D98(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.RA = 0x80015DA8u;
        GranTurismo2PC.func_80015B6C(c, m);
        c.A0 = 0u + 0u;
        c.RA = 0x80015DB0u;
        GranTurismo2PC.func_80015B08(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80015DC0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.S0 + 0x1Cu), 0u);
        MemoryAccess.WriteU32(m, (c.S0 + 0x14u), 0u);
        if (c.S1 == 0u) {
            MemoryAccess.WriteU32(m, (c.S0 + 0x18u), 0u);
            goto L80015E18;
        }
        MemoryAccess.WriteU32(m, (c.S0 + 0x18u), 0u);
        c.V0 = MemoryAccess.ReadU32(m, c.S1);
        MemoryAccess.WriteU32(m, (c.S0 + 0x18u), c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x4u));
        MemoryAccess.WriteU32(m, (c.S0 + 0x14u), c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0xCu));
        MemoryAccess.WriteU32(m, (c.S0 + 0x1Cu), c.V0);
        L80015E18: ;
        c.A0 = c.S0 + 0x30u;
        c.RA = 0x80015E20u;
        GranTurismo2PC.func_8006BE64(c, m);
        c.A0 = c.S0 + 0x4Cu;
        c.RA = 0x80015E28u;
        GranTurismo2PC.func_8006BCB8(c, m);
        c.A0 = c.S0 + 0x60u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80015E34u;
        GranTurismo2PC.func_8006E43C(c, m);
        c.A0 = c.S0 + 0xF8u;
        c.A1 = c.S1 + 0u;
        MemoryAccess.WriteU16(m, (c.S0 + 0xF4u), (ushort)c.V0);
        c.RA = 0x80015E44u;
        GranTurismo2PC.func_8006E43C(c, m);
        c.A0 = c.S0 + 0x190u;
        c.A1 = c.S1 + 0u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x18Cu), (ushort)c.V0);
        c.RA = 0x80015E54u;
        GranTurismo2PC.func_8006E43C(c, m);
        c.A0 = c.S0 + 0x228u;
        c.A1 = c.S1 + 0u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x224u), (ushort)c.V0);
        c.RA = 0x80015E64u;
        GranTurismo2PC.func_8006E43C(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x48FCu;
        c.A1 = c.S1 + 0u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x2BCu), (ushort)c.V0);
        c.RA = 0x80015E78u;
        GranTurismo2PC.func_8006D9DC(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x48DCu;
        c.A1 = c.S1 + 0u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x356u), (ushort)c.V0);
        c.RA = 0x80015E8Cu;
        GranTurismo2PC.func_8006CFC4(c, m);
        c.A0 = c.S0 + 0x2C0u;
        c.A1 = c.S1 + 0u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x358u), (ushort)c.V0);
        c.RA = 0x80015E9Cu;
        GranTurismo2PC.func_8006E43C(c, m);
        c.A0 = 0x00000001u;
        c.V1 = MemoryAccess.ReadU32(m, (c.S0 + 0x4u));
        c.A1 = 0u + 0u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x354u), (ushort)c.V0);
        c.RA = 0x80015EB0u;
        Dispatcher.Call(c, m, c.V1);
        c.A0 = c.V0 + 0u;
        L80015EB4: ;
        c.V0 = 0xFFFFFFFFu;
        if (c.A0 == c.V0) {
            c.V0 = 0x00000016u;
            goto L80015F30;
        }
        c.V0 = 0x00000016u;
        if (c.A0 != c.V0) {
            c.V1 = c.S0 + 0x30u;
            goto L80015F20;
        }
        c.V1 = c.S0 + 0x30u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V1 + 0x18u));
        if ((int)c.V0 < 0) {
            c.V0 = 0x80020000u;
            goto L80015F00;
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
        L80015F00: ;
        c.V0 = c.V0 + 0x10E4u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x24u), c.V0);
        MemoryAccess.WriteU32(m, (c.S0 + 0x28u), c.V0);
        c.RA = 0x80015F10u;
        GranTurismo2PC.func_80013430(c, m);
        c.A0 = 0xFFFFFFFFu;
        c.RA = 0x80015F18u;
        GranTurismo2PC.func_80015B08(c, m);
        c.S2 = 0x00000001u;
        goto L80015F30;
        L80015F20: ;
        c.RA = 0x80015F28u;
        GranTurismo2PC.func_80015B08(c, m);
        c.A0 = c.V0 + 0u;
        goto L80015EB4;
        L80015F30: ;
        c.V0 = c.S2 + 0u;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80015F4C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x78u;
        MemoryAccess.WriteU32(m, (c.SP + 0x68u), c.S4);
        c.S4 = c.A0 + 0u;
        c.V1 = 0x02420000u;
        c.V1 = c.V1 | 0x362Au;
        c.A0 = c.SP + 0x18u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x64u), c.S3);
        c.S3 = MemoryAccess.ReadU32(m, (c.V0 - 0x7228u));
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
        c.RA = 0x80015FA0u;
        GranTurismo2PC.func_8006AC68(c, m);
        c.A0 = c.SP + 0x18u;
        c.A1 = 0x800B0000u;
        c.A1 = c.A1 - 0x7210u;
        c.RA = 0x80015FB0u;
        GranTurismo2PC.func_8007DA80(c, m);
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
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6720u));
        if (c.V0 != 0u) {
            c.S1 = 0x00000001u;
            goto L8001603C;
        }
        c.S1 = 0x00000001u;
        c.S0 = 0x02600000u;
        c.S0 = c.S0 | 0x6060u;
        c.A0 = c.S4 + 0u;
        c.A2 = 0x000000B0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.A1 = MemoryAccess.ReadU32(m, (c.S3 + 0x24u));
        c.A3 = 0x00000102u;
        c.RA = 0x8001601Cu;
        GranTurismo2PC.func_8001330C(c, m);
        c.A0 = c.S4 + 0u;
        c.A2 = 0x000000B0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.A1 = MemoryAccess.ReadU32(m, (c.S3 + 0x28u));
        c.A3 = 0x00000126u;
        c.RA = 0x80016034u;
        GranTurismo2PC.func_8001330C(c, m);
        c.S0 = c.S3 + 0x30u;
        goto L80016070;
        L8001603C: ;
        c.A0 = c.S0 + 0u;
        c.A2 = 0x000000B0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S1);
        c.A1 = MemoryAccess.ReadU32(m, (c.S3 + 0x24u));
        c.A3 = 0x00000112u;
        c.RA = 0x80016054u;
        GranTurismo2PC.func_8006ADB4(c, m);
        c.A0 = c.S0 + 0u;
        c.A2 = 0x000000B0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S1);
        c.A1 = MemoryAccess.ReadU32(m, (c.S3 + 0x28u));
        c.A3 = 0x00000136u;
        c.RA = 0x8001606Cu;
        GranTurismo2PC.func_8006ADB4(c, m);
        c.S0 = c.S3 + 0x30u;
        L80016070: ;
        c.A0 = c.S0 + 0u;
        c.RA = 0x80016078u;
        GranTurismo2PC.func_8006BEB4(c, m);
        c.S5 = c.V0 + 0u;
        if (c.S5 == 0u) {
            c.S2 = c.SP + 0x48u;
            goto L80016198;
        }
        c.S2 = c.SP + 0x48u;
        c.A0 = c.S2 + 0u;
        c.A1 = c.SP + 0x4Cu;
        c.A2 = c.S5 + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x80016098u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.SP + 0x18u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6594u;
        c.A2 = 0x00000014u;
        c.A3 = 0x0000008Au;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), 0u);
        c.RA = 0x800160B8u;
        GranTurismo2PC.func_8006AC90(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S4 + 0u;
        c.A2 = 0x00000010u;
        c.A3 = 0x0000007Au;
        c.RA = 0x800160CCu;
        GranTurismo2PC.func_8006BEF4(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = 0x00000220u;
        c.RA = 0x800160D8u;
        GranTurismo2PC.func_8007DA44(c, m);
        c.S0 = c.S3 + 0x4Cu;
        c.A0 = c.S0 + 0u;
        c.A1 = c.S4 + 0u;
        c.S1 = 0x000000B0u;
        c.V0 = 0x000000C6u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x8u), (ushort)c.S1);
        MemoryAccess.WriteU16(m, (c.S0 + 0xAu), (ushort)c.V0);
        c.RA = 0x800160F8u;
        GranTurismo2PC.func_8006BD08(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S4 + 0u;
        c.V0 = 0x0000006Eu;
        MemoryAccess.WriteU16(m, (c.A0 + 0x8u), (ushort)c.S1);
        MemoryAccess.WriteU16(m, (c.A0 + 0xAu), (ushort)c.V0);
        c.RA = 0x80016110u;
        GranTurismo2PC.func_8006BD08(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = 0x00000220u;
        c.RA = 0x8001611Cu;
        GranTurismo2PC.func_8007DA44(c, m);
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
        c.RA = 0x80016164u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S2 + 0u;
        c.A1 = c.SP + 0x54u;
        c.A2 = c.S5 + 0u;
        c.A3 = 0x00000080u;
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.V0);
        c.RA = 0x8001617Cu;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S6 + 0u;
        c.A1 = c.SP + 0x38u;
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.V0);
        c.RA = 0x8001618Cu;
        GranTurismo2PC.func_8006B6E4(c, m);
        c.A0 = c.S6 + 0u;
        c.A1 = 0x00000200u;
        c.RA = 0x80016198u;
        GranTurismo2PC.func_8007DA44(c, m);
        L80016198: ;
        c.A0 = c.S3 + 0x60u;
        c.A1 = c.S4 + 0u;
        c.A2 = c.SP + 0x18u;
        c.RA = 0x800161A8u;
        GranTurismo2PC.func_8006E5B8(c, m);
        c.A0 = c.S3 + 0xF8u;
        c.A1 = c.S4 + 0u;
        c.A2 = c.SP + 0x18u;
        c.RA = 0x800161B8u;
        GranTurismo2PC.func_8006E5B8(c, m);
        c.A0 = c.S3 + 0x190u;
        c.A1 = c.S4 + 0u;
        c.A2 = c.SP + 0x18u;
        c.RA = 0x800161C8u;
        GranTurismo2PC.func_8006E5B8(c, m);
        c.A0 = c.S3 + 0x228u;
        c.A1 = c.S4 + 0u;
        c.A2 = c.SP + 0x18u;
        c.RA = 0x800161D8u;
        GranTurismo2PC.func_8006E5B8(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x48FCu;
        c.A1 = c.S4 + 0u;
        c.A2 = c.SP + 0x18u;
        c.RA = 0x800161ECu;
        GranTurismo2PC.func_8006DAF8(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = 0x00000020u;
        c.RA = 0x800161F8u;
        GranTurismo2PC.func_8007DA44(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x48DCu;
        c.A1 = c.S6 + 0u;
        c.RA = 0x80016208u;
        GranTurismo2PC.func_8006D50C(c, m);
        c.A0 = c.S3 + 0x2C0u;
        c.A1 = c.S4 + 0u;
        c.A2 = c.SP + 0x18u;
        c.RA = 0x80016218u;
        GranTurismo2PC.func_8006E5B8(c, m);
        c.A0 = 0x00000002u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S3 + 0x4u));
        c.A1 = c.S4 + 0u;
        c.RA = 0x8001622Cu;
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
    public static void func_80016254(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x7250u;
        c.A0 = 0x80020000u;
        c.A0 = c.A0 + 0x1104u;
        MemoryAccess.WriteU32(m, (c.SP + 0x7248u), c.RA);
        c.A1 = c.SP + 0x10u;
        c.RA = 0x8001626Cu;
        GranTurismo2PC.func_80082FAC(c, m);
        c.A0 = 0x801C0000u;
        c.V0 = 0x801D0000u;
        c.V1 = MemoryAccess.ReadU8(m, (c.V0 - 0x6720u));
        c.A0 = c.A0 - 0x69D0u;
        c.V0 = c.V1 << 3;
        c.V0 = c.V0 - c.V1;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 - c.V1;
        c.V1 = c.SP + 0x10u;
        c.V1 = c.V1 + c.V0;
        c.V0 = c.V1 | c.A0;
        c.V0 = c.V0 & 0x0003u;
        if (c.V0 == 0u) {
            c.V0 = c.V1 + 0xE40u;
            goto L80016308;
        }
        c.V0 = c.V1 + 0xE40u;
        L800162B4: ;
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
            goto L800162B4;
        }
        c.A0 = c.A0 + 0x10u;
        goto L80016334;
        L80016308: ;
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
            goto L80016308;
        }
        c.A0 = c.A0 + 0x10u;
        L80016334: ;
        c.A2 = MemoryAccess.ReadWordLeft(m, c.A2, (c.V1 + 0x3u));
        c.A2 = MemoryAccess.ReadWordRight(m, c.A2, c.V1);
        c.A3 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V1 + 0x4u));
        c.T0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V1 + 0x5u));
        c.T1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V1 + 0x6u));
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0x3u), c.A2);
        MemoryAccess.WriteWordRight(m, c.A0, c.A2);
        MemoryAccess.WriteU8(m, (c.A0 + 0x4u), (byte)c.A3);
        MemoryAccess.WriteU8(m, (c.A0 + 0x5u), (byte)c.T0);
        MemoryAccess.WriteU8(m, (c.A0 + 0x6u), (byte)c.T1);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x7248u));
        c.SP = c.SP + 0x7250u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001636C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.RA = 0x8001637Cu;
        GranTurismo2PC.func_80016254(c, m);
        c.RA = 0x80016384u;
        GranTurismo2PC.func_8001DA08(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80016394(CpuContext c, IMemory m)
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
    public static void func_800163C8(CpuContext c, IMemory m)
    {
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0x10u));
        c.V1 = MemoryAccess.ReadU16(m, (c.A0 + 0x10u));
        if ((int)c.V0 < 0) {
            c.V0 = (int)c.V0 < -1 ? 1u : 0u;
            goto L800163FC;
        }
        c.V0 = (int)c.V0 < -1 ? 1u : 0u;
        c.V0 = c.V1 + 0x1u;
        MemoryAccess.WriteU16(m, (c.A0 + 0x10u), (ushort)c.V0);
        c.V0 = c.V0 << 16;
        c.V0 = (uint)((int)c.V0 >> 16);
        c.V0 = (int)c.V0 < 12 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = 0x0000000Cu;
            goto L80016408;
        }
        c.V0 = 0x0000000Cu;
        MemoryAccess.WriteU16(m, (c.A0 + 0x10u), (ushort)c.V0);
        return;
        L800163FC: ;
        if (c.V0 == 0u) {
            c.V0 = c.V1 + 0x1u;
            goto L80016408;
        }
        c.V0 = c.V1 + 0x1u;
        MemoryAccess.WriteU16(m, (c.A0 + 0x10u), (ushort)c.V0);
        L80016408: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80016410(CpuContext c, IMemory m)
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
            goto L80016980;
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
            goto L8001649C;
        }
        c.V0 = 0x00000040u;
        if (c.V1 == c.V0) {
            goto L800164A4;
        }
        goto L800164A8;
        L8001649C: ;
        c.S4 = c.S4 + c.T2;
        goto L800164A8;
        L800164A4: ;
        c.S4 = c.S4 - c.T2;
        L800164A8: ;
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S3 + 0x10u));
        if ((int)c.A1 >= 0) {
            c.V0 = (int)c.A1 < 12 ? 1u : 0u;
            goto L800165FC;
        }
        c.V0 = (int)c.A1 < 12 ? 1u : 0u;
        c.V0 = (int)c.A1 < -1 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A1 = c.A1 + 0xDu;
            goto L80016980;
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
            goto L80016598;
        }
        c.A0 = c.FP + 0u;
        c.S2 = 0x000000C0u;
        L80016598: ;
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
        c.RA = 0x800165D4u;
        GranTurismo2PC.func_8006B61C(c, m);
        c.A0 = c.FP + 0u;
        c.A1 = c.SP + 0x10u;
        c.S0 = c.S0 - c.S1;
        MemoryAccess.WriteU16(m, (c.SP + 0x10u), (ushort)c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), 0u);
        c.RA = 0x800165F0u;
        GranTurismo2PC.func_8006B61C(c, m);
        c.A0 = c.FP + 0u;
        c.A1 = 0x00000020u;
        goto L80016978;
        L800165FC: ;
        c.S2 = MemoryAccess.ReadU8(m, (c.S3 + 0x8u));
        if (c.V0 == 0u) {
            { var _r = (long)(int)c.S2 * (int)c.A1; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
            goto L80016664;
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
        L80016664: ;
        c.V1 = MemoryAccess.ReadU8(m, c.S3);
        c.V0 = c.V1 & 0x0004u;
        if (c.V0 != 0u) {
            goto L8001667C;
        }
        c.T0 = 0u + 0u;
        L8001667C: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S3 + 0x12u));
        { var _r = (long)(int)c.S2 * (int)c.V0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V0 = c.V1 & 0x0018u;
        c.S7 = c.V0 << 2;
        MemoryAccess.WriteU16(m, (c.SP + 0x20u), (ushort)c.S7);
        c.T4 = c.LO;
        c.S2 = (uint)((int)c.T4 >> 7);
        if (c.T0 == 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.T4);
            goto L80016774;
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
        c.RA = 0x800166C8u;
        GranTurismo2PC.func_80081478(c, m);
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
        c.RA = 0x80016718u;
        GranTurismo2PC.func_80081478(c, m);
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
        c.RA = 0x80016774u;
        GranTurismo2PC.func_8007DA44(c, m);
        L80016774: ;
        c.V0 = MemoryAccess.ReadU8(m, c.S3);
        c.V0 = c.V0 & 0x0002u;
        if (c.V0 == 0u) {
            c.A0 = c.FP + 0u;
            goto L8001690C;
        }
        c.A0 = c.FP + 0u;
        c.A1 = c.S2 << 8;
        c.A1 = c.S2 | c.A1;
        c.V0 = c.S2 << 16;
        c.A1 = c.A1 | c.V0;
        c.V0 = 0x02000000u;
        c.A1 = c.A1 | c.V0;
        c.RA = 0x800167A4u;
        GranTurismo2PC.func_80081478(c, m);
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
        c.RA = 0x800167FCu;
        GranTurismo2PC.func_8007DA44(c, m);
        c.V0 = MemoryAccess.ReadU8(m, c.S3);
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u) {
            goto L80016968;
        }
        c.T4 = MemoryAccess.ReadU32(m, (c.SP + 0x60u));
        if (c.T4 == 0u) {
            c.A0 = c.FP + 0u;
            goto L80016884;
        }
        c.A0 = c.FP + 0u;
        c.A1 = 0x02000000u;
        c.RA = 0x80016828u;
        GranTurismo2PC.func_80081478(c, m);
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
        c.RA = 0x8001687Cu;
        GranTurismo2PC.func_8007DA44(c, m);
        c.A0 = c.FP + 0u;
        goto L80016890;
        L80016884: ;
        c.T4 = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.S2 = (uint)((int)c.T4 >> 8);
        L80016890: ;
        c.A1 = c.S2 << 8;
        c.A1 = c.S2 | c.A1;
        c.V0 = c.S2 << 16;
        c.A1 = c.A1 | c.V0;
        c.V0 = 0x02000000u;
        c.A1 = c.A1 | c.V0;
        c.RA = 0x800168ACu;
        GranTurismo2PC.func_80081478(c, m);
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
        c.RA = 0x80016904u;
        GranTurismo2PC.func_8007DA44(c, m);
        goto L80016968;
        L8001690C: ;
        c.A1 = c.S2 << 8;
        c.A1 = c.S2 | c.A1;
        c.V0 = c.S2 << 16;
        c.A1 = c.A1 | c.V0;
        c.RA = 0x80016920u;
        GranTurismo2PC.func_80081478(c, m);
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
        L80016968: ;
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x8u));
        c.T4 = MemoryAccess.ReadU16(m, (c.SP + 0x20u));
        c.A0 = c.FP + 0u;
        c.A1 = c.A1 | c.T4;
        L80016978: ;
        c.RA = 0x80016980u;
        GranTurismo2PC.func_8007DA44(c, m);
        L80016980: ;
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
    public static void func_800169B0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        c.V1 = 0x801D0000u;
        c.V1 = MemoryAccess.ReadU8(m, (c.V1 - 0x6720u));
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 - 0x72A0u));
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        if (c.V1 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
            goto L800169FC;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.A0 = c.S1 + 0u;
        c.RA = 0x800169DCu;
        GranTurismo2PC.func_8007F174(c, m);
        c.A0 = c.S1 + 0u;
        c.S0 = 0x801F0000u;
        c.S0 = c.S0 - 0x4C7u;
        c.A1 = c.S0 + 0u;
        c.RA = 0x800169F0u;
        GranTurismo2PC.func_8006ECD8(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S0 + 0x11u;
        c.RA = 0x800169FCu;
        GranTurismo2PC.func_8006ECD8(c, m);
        L800169FC: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80016A10_gt2_overlay_1(CpuContext c, IMemory m)
    {
        c.A1 = c.A0 + 0u;
        c.V0 = 0x800B0000u;
        c.V1 = 0x801D0000u;
        c.V1 = MemoryAccess.ReadU8(m, (c.V1 - 0x6720u));
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 - 0x72A0u));
        c.SP = c.SP - 0x18u;
        if (c.V1 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
            goto L80016A38;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.RA = 0x80016A38u;
        GranTurismo2PC.func_8006ECD8(c, m);
        L80016A38: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80016A48(CpuContext c, IMemory m)
    {
        c.V0 = 0x801D0000u;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6720u));
        c.SP = c.SP - 0x18u;
        if (c.V0 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
            goto L80016A84;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.RA = 0x80016A64u;
        GranTurismo2PC.func_800169B0(c, m);
        c.V0 = 0x800B0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x121Cu));
        c.RA = 0x80016A74u;
        Dispatcher.Call(c, m, 0x80016A10u);
        c.V0 = 0x800B0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1220u));
        c.RA = 0x80016A84u;
        Dispatcher.Call(c, m, 0x80016A10u);
        L80016A84: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80016A94(CpuContext c, IMemory m)
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
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6720u));
        c.V1 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S5);
        c.S5 = MemoryAccess.ReadU32(m, (c.V1 - 0x72A0u));
        c.A0 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        if (c.V0 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
            goto L80016BAC;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        if (c.A2 == 0u) {
            c.S0 = c.A1 + 0u;
            goto L80016B20;
        }
        c.S0 = c.A1 + 0u;
        c.V0 = MemoryAccess.ReadU8(m, c.A1);
        if (c.V0 == 0u) {
            c.V1 = c.A1 + 0u;
            goto L80016B14;
        }
        c.V1 = c.A1 + 0u;
        L80016B00: ;
        c.V1 = c.V1 + 0x2u;
        c.V0 = MemoryAccess.ReadU8(m, c.V1);
        if (c.V0 != 0u) {
            c.A0 = c.A0 + 0xCu;
            goto L80016B00;
        }
        c.A0 = c.A0 + 0xCu;
        L80016B14: ;
        c.V0 = (uint)((int)c.A0 >> 1);
        c.S1 = c.S1 - c.V0;
        c.S0 = c.A1 + 0u;
        L80016B20: ;
        c.S6 = 0x02000000u;
        c.S2 = c.S3 + 0x1Fu;
        L80016B28: ;
        c.V0 = MemoryAccess.ReadU8(m, c.S0);
        if (c.V0 == 0u) {
            c.A0 = c.S5 + 0u;
            goto L80016BA0;
        }
        c.A0 = c.S5 + 0u;
        c.A1 = c.V0 + 0u;
        c.V0 = MemoryAccess.ReadU8(m, (c.S0 + 0x1u));
        c.A1 = c.A1 << 8;
        c.A1 = c.A1 | c.V0;
        c.RA = 0x80016B4Cu;
        GranTurismo2PC.func_8007F18C(c, m);
        if (c.V0 == 0u) {
            c.A0 = c.V0 + 0u;
            goto L80016B94;
        }
        c.A0 = c.V0 + 0u;
        c.A1 = c.S4 + 0u;
        c.A2 = c.S7 | c.S6;
        c.RA = 0x80016B60u;
        GranTurismo2PC.func_8007F01C(c, m);
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
        L80016B94: ;
        c.S1 = c.S1 + 0xCu;
        c.S0 = c.S0 + 0x2u;
        goto L80016B28;
        L80016BA0: ;
        c.A0 = c.S4 + 0u;
        c.A1 = 0x00000220u;
        c.RA = 0x80016BACu;
        GranTurismo2PC.func_8007DA44(c, m);
        L80016BAC: ;
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
    public static void func_80016BD8_gt2_overlay_1(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1210u), 0u);
        c.V0 = 0x800B0000u;
        c.V1 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1218u), 0u);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x2BD4u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x121Cu), c.V0);
        c.V1 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU32(m, (c.V1 + 0x1220u), c.V0);
        c.RA = 0x80016C0Cu;
        GranTurismo2PC.func_80016A48(c, m);
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 - 0x71F0u;
        c.RA = 0x80016C18u;
        GranTurismo2PC.func_8006A038(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80016C28(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.V1 = 0x800B0000u;
        c.V0 = 0x800B0000u;
        c.A0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1218u));
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0x1210u));
        c.V1 = c.V1 - 0x729Cu;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.V0 = c.V0 << 1;
        c.V0 = c.V0 + c.V1;
        c.V1 = 0x800B0000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 - 0x7298u));
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, c.V0);
        c.A2 = c.V1 + 0x1A4u;
        c.V1 = c.A1 < 0x0000000Bu ? 1u : 0u;
        if (c.V1 == 0u) {
            c.S1 = 0u + 0u;
            goto L80016F54;
        }
        c.S1 = 0u + 0u;
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x2BDCu;
        c.V1 = c.A1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x80016C94u: goto L80016C94;
            case 0x80016F0Cu: goto L80016F0C;
            case 0x80016D08u: goto L80016D08;
            case 0x80016D88u: goto L80016D88;
            case 0x80016DF4u: goto L80016DF4;
            case 0x80016ED4u: goto L80016ED4;
            case 0x80016E88u: goto L80016E88;
            case 0x80016F1Cu: goto L80016F1C;
            case 0x80016F38u: goto L80016F38;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L80016C94: ;
        if (c.A0 == 0u) {
            c.V0 = 0x00000001u;
            goto L80016CAC;
        }
        c.V0 = 0x00000001u;
        if (c.A0 == c.V0) {
            c.A1 = 0x800B0000u;
            goto L80016F54;
        }
        c.A1 = 0x800B0000u;
        c.A0 = 0x800B0000u;
        goto L80016CF0;
        L80016CAC: ;
        c.S0 = 0x800B0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.S0 + 0x1218u));
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 - 0x71F0u;
        c.RA = 0x80016CC0u;
        GranTurismo2PC.func_8006A0D4(c, m);
        if (c.V0 == 0u) {
            c.V1 = 0x800B0000u;
            goto L80016CD4;
        }
        c.V1 = 0x800B0000u;
        c.V0 = 0x00000002u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x1210u), c.V0);
        goto L80016F54;
        L80016CD4: ;
        c.V0 = 0x800B0000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.S0 + 0x1218u));
        c.A0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1210u), c.A0);
        c.V1 = c.V1 + c.A0;
        MemoryAccess.WriteU32(m, (c.S0 + 0x1218u), c.V1);
        goto L80016F54;
        L80016CF0: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.A0 + 0x1218u));
        c.V1 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.A1 + 0x1210u), c.V1);
        c.V0 = c.V0 + c.V1;
        MemoryAccess.WriteU32(m, (c.A0 + 0x1218u), c.V0);
        goto L80016F54;
        L80016D08: ;
        if (c.A0 != 0u) {
            c.V1 = 0x800B0000u;
            goto L80016DE8;
        }
        c.V1 = 0x800B0000u;
        c.V0 = 0x800B0000u;
        c.S0 = c.V0 - 0x71F0u;
        c.A0 = c.S0 + 0u;
        c.V0 = 0x800B0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1218u));
        c.A2 = c.S0 + 0u;
        c.RA = 0x80016D2Cu;
        GranTurismo2PC.func_8006A1B4(c, m);
        if (c.V0 == 0u) {
            c.V1 = 0x800B0000u;
            goto L80016D40;
        }
        c.V1 = 0x800B0000u;
        c.V0 = 0x00000007u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x1210u), c.V0);
        goto L80016D7C;
        L80016D40: ;
        c.V1 = 0x800B0000u;
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0x76Bu;
        MemoryAccess.WriteU32(m, (c.V1 + 0x121Cu), c.V0);
        c.V1 = 0x800B0000u;
        c.V0 = c.V0 - 0x151u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x1220u), c.V0);
        c.RA = 0x80016D60u;
        GranTurismo2PC.func_80016A48(c, m);
        c.A0 = c.S0 + 0u;
        c.RA = 0x80016D68u;
        GranTurismo2PC.func_8006A12C(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x47D4u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x10u), c.V0);
        c.RA = 0x80016D78u;
        GranTurismo2PC.func_8006C04C(c, m);
        c.V1 = 0x800B0000u;
        L80016D7C: ;
        c.V0 = 0x00000003u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x1210u), c.V0);
        goto L80016F54;
        L80016D88: ;
        c.RA = 0x80016D90u;
        GranTurismo2PC.func_8007D7CC(c, m);
        c.V1 = c.V0 + 0u;
        if (c.V1 == 0u) {
            c.V0 = 0x00000001u;
            goto L80016DC0;
        }
        c.V0 = 0x00000001u;
        if (c.V1 != c.V0) {
            c.V1 = 0x80050000u;
            goto L80016DDC;
        }
        c.V1 = 0x80050000u;
        c.V0 = 0x801F0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0x6D8u));
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x47D4u;
        c.RA = 0x80016DB8u;
        GranTurismo2PC.func_8006C074(c, m);
        c.V0 = c.S1 + 0u;
        goto L80016F58;
        L80016DC0: ;
        c.V1 = 0x80050000u;
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU16(m, (c.V1 - 0x47A0u), (ushort)c.V0);
        c.V1 = 0x800B0000u;
        c.V0 = 0x00000004u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x1210u), c.V0);
        goto L80016F54;
        L80016DDC: ;
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU16(m, (c.V1 - 0x47A0u), (ushort)c.V0);
        c.V1 = 0x800B0000u;
        L80016DE8: ;
        c.V0 = 0x00000007u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x1210u), c.V0);
        goto L80016F54;
        L80016DF4: ;
        c.V0 = 0x800B0000u;
        c.S0 = c.V0 - 0x71F0u;
        c.A0 = c.S0 + 0u;
        c.A1 = c.S0 + 0u;
        c.RA = 0x80016E08u;
        GranTurismo2PC.func_8006A314(c, m);
        if (c.V0 == 0u) {
            c.A0 = c.S0 + 0u;
            goto L80016E50;
        }
        c.A0 = c.S0 + 0u;
        c.A1 = c.A0 + 0u;
        c.RA = 0x80016E18u;
        GranTurismo2PC.func_8006A278(c, m);
        c.V1 = 0x800B0000u;
        c.V0 = 0x000000B4u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x1214u), c.V0);
        c.V1 = 0x800B0000u;
        c.V0 = 0x00000005u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x1210u), c.V0);
        c.V1 = 0x800B0000u;
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x2BD4u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x121Cu), c.V0);
        c.V1 = 0x800B0000u;
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0x758u;
        goto L80016EC4;
        L80016E50: ;
        c.V1 = 0x800B0000u;
        c.V0 = 0x000000F0u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x1214u), c.V0);
        c.V1 = 0x800B0000u;
        c.V0 = 0x00000006u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x1210u), c.V0);
        c.V1 = 0x800B0000u;
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x2BD4u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x121Cu), c.V0);
        c.V1 = 0x800B0000u;
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0x641u;
        goto L80016EC4;
        L80016E88: ;
        c.A0 = 0u + 0u;
        c.RA = 0x80016E90u;
        GranTurismo2PC.func_80060840(c, m);
        c.V1 = 0x800B0000u;
        c.V0 = 0x000000F0u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x1214u), c.V0);
        c.V1 = 0x800B0000u;
        c.V0 = 0x00000008u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x1210u), c.V0);
        c.V1 = 0x800B0000u;
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x2BD4u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x121Cu), c.V0);
        c.V1 = 0x800B0000u;
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0x741u;
        L80016EC4: ;
        MemoryAccess.WriteU32(m, (c.V1 + 0x1220u), c.V0);
        c.RA = 0x80016ECCu;
        GranTurismo2PC.func_80016A48(c, m);
        c.V0 = c.S1 + 0u;
        goto L80016F58;
        L80016ED4: ;
        c.S0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x1214u));
        c.V1 = MemoryAccess.ReadU32(m, (c.A2 + 0x4u));
        c.V0 = c.V0 - 0x1u;
        c.V1 = c.V1 & 0x0F00u;
        if (c.V1 == 0u) {
            MemoryAccess.WriteU32(m, (c.S0 + 0x1214u), c.V0);
            goto L80016EFC;
        }
        MemoryAccess.WriteU32(m, (c.S0 + 0x1214u), c.V0);
        MemoryAccess.WriteU32(m, (c.S0 + 0x1214u), 0u);
        c.A0 = 0x00000001u;
        c.RA = 0x80016EFCu;
        GranTurismo2PC.func_80060840(c, m);
        L80016EFC: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x1214u));
        if (c.V0 != 0u) {
            c.V0 = c.S1 + 0u;
            goto L80016F58;
        }
        c.V0 = c.S1 + 0u;
        L80016F0C: ;
        c.V1 = 0x800B0000u;
        c.V0 = 0x00000009u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x1210u), c.V0);
        goto L80016F54;
        L80016F1C: ;
        c.V1 = 0x800B0000u;
        c.V0 = 0x00000006u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x1214u), c.V0);
        c.V1 = 0x800B0000u;
        c.V0 = 0x0000000Au;
        MemoryAccess.WriteU32(m, (c.V1 + 0x1210u), c.V0);
        goto L80016F54;
        L80016F38: ;
        c.V1 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x1214u));
        c.V0 = c.V0 - 0x1u;
        if (c.V0 != 0u) {
            MemoryAccess.WriteU32(m, (c.V1 + 0x1214u), c.V0);
            goto L80016F54;
        }
        MemoryAccess.WriteU32(m, (c.V1 + 0x1214u), c.V0);
        c.S1 = 0x00000004u;
        L80016F54: ;
        c.V0 = c.S1 + 0u;
        L80016F58: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80016F6C(CpuContext c, IMemory m)
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
        c.RA = 0x80016FA0u;
        GranTurismo2PC.func_8006AC68(c, m);
        c.S0 = c.SP + 0x18u;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x69E0u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.S0 + 0x10u), c.S3);
        c.RA = 0x80016FBCu;
        GranTurismo2PC.func_8007DA80(c, m);
        c.V0 = 0x800B0000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1210u));
        c.V0 = 0x00000005u;
        if (c.V1 == c.V0) {
            c.V0 = (int)c.V1 < 6 ? 1u : 0u;
            goto L800170A8;
        }
        c.V0 = (int)c.V1 < 6 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000003u;
            goto L80016FE8;
        }
        c.V0 = 0x00000003u;
        if (c.V1 == c.V0) {
            c.V0 = 0x801D0000u;
            goto L80017004;
        }
        c.V0 = 0x801D0000u;
        goto L80017194;
        L80016FE8: ;
        c.V0 = 0x00000006u;
        if (c.V1 == c.V0) {
            c.V0 = 0x00000008u;
            goto L80017108;
        }
        c.V0 = 0x00000008u;
        if (c.V1 == c.V0) {
            c.S1 = 0x020A0000u;
            goto L800170E4;
        }
        c.S1 = 0x020A0000u;
        goto L80017194;
        L80017004: ;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6720u));
        if (c.V0 != 0u) {
            c.V0 = 0x00000001u;
            goto L80017074;
        }
        c.V0 = 0x00000001u;
        c.S1 = 0x026E0000u;
        c.S1 = c.S1 | 0x5A50u;
        c.A0 = c.S3 + 0u;
        c.A2 = 0x000000B0u;
        c.A3 = 0x000000D0u;
        c.V0 = 0x800B0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0x121Cu));
        c.S0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.RA = 0x80017040u;
        GranTurismo2PC.func_80016A94(c, m);
        c.A0 = c.S3 + 0u;
        c.A2 = 0x000000B0u;
        c.V0 = 0x800B0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1220u));
        c.A3 = 0x000000F0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.RA = 0x80017060u;
        GranTurismo2PC.func_80016A94(c, m);
        c.A0 = c.S3 + 0u;
        c.A1 = 0x00000220u;
        c.RA = 0x8001706Cu;
        GranTurismo2PC.func_8007DA44(c, m);
        c.A0 = 0x80050000u;
        goto L80017094;
        L80017074: ;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x66CCu;
        c.A2 = 0x000000B0u;
        c.A3 = 0x000000F0u;
        c.RA = 0x80017090u;
        GranTurismo2PC.func_8006ADB4(c, m);
        c.A0 = 0x80050000u;
        L80017094: ;
        c.A0 = c.A0 - 0x47D4u;
        c.A1 = c.S3 + 0u;
        c.RA = 0x800170A0u;
        GranTurismo2PC.func_8006C174(c, m);
        c.S2 = 0x00000001u;
        goto L80017194;
        L800170A8: ;
        c.V0 = 0x801D0000u;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6720u));
        if (c.V0 != 0u) {
            c.V0 = 0x00000001u;
            goto L800170C8;
        }
        c.V0 = 0x00000001u;
        c.S1 = 0x026E0000u;
        c.S1 = c.S1 | 0x5A50u;
        goto L80017120;
        L800170C8: ;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x66AAu;
        c.A2 = 0x000000B0u;
        c.A3 = 0x000000F0u;
        goto L8001718C;
        L800170E4: ;
        c.V0 = 0x801D0000u;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6720u));
        if (c.V0 == 0u) {
            c.S1 = c.S1 | 0x50F0u;
            goto L80017120;
        }
        c.S1 = c.S1 | 0x50F0u;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x668Fu;
        goto L80017178;
        L80017108: ;
        c.S1 = 0x020A0000u;
        c.V0 = 0x801D0000u;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6720u));
        if (c.V0 != 0u) {
            c.S1 = c.S1 | 0x50F0u;
            goto L8001716C;
        }
        c.S1 = c.S1 | 0x50F0u;
        L80017120: ;
        c.A0 = c.S3 + 0u;
        c.A2 = 0x000000B0u;
        c.A3 = 0x000000D0u;
        c.V0 = 0x800B0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0x121Cu));
        c.S0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.RA = 0x80017144u;
        GranTurismo2PC.func_80016A94(c, m);
        c.A0 = c.S3 + 0u;
        c.A2 = 0x000000B0u;
        c.V0 = 0x800B0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1220u));
        c.A3 = 0x000000F0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.RA = 0x80017164u;
        GranTurismo2PC.func_80016A94(c, m);
        c.S2 = 0x00000001u;
        goto L80017194;
        L8001716C: ;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6673u;
        L80017178: ;
        c.A2 = 0x000000B0u;
        c.A3 = 0x000000F0u;
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        L8001718C: ;
        c.S2 = 0x00000001u;
        c.RA = 0x80017194u;
        GranTurismo2PC.func_8006ADB4(c, m);
        L80017194: ;
        if (c.S2 == 0u) {
            c.S1 = 0x026E0000u;
            goto L80017200;
        }
        c.S1 = 0x026E0000u;
        c.V0 = 0x801F0000u;
        c.A1 = c.V0 - 0x4C7u;
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1218u));
        c.V1 = 0x00000001u;
        if (c.V0 != c.V1) {
            c.S1 = c.S1 | 0x6460u;
            goto L800171BC;
        }
        c.S1 = c.S1 | 0x6460u;
        c.A1 = c.A1 + 0x11u;
        L800171BC: ;
        c.V0 = 0x801D0000u;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6720u));
        if (c.V0 != 0u) {
            c.A0 = c.SP + 0x18u;
            goto L800171F0;
        }
        c.A0 = c.SP + 0x18u;
        c.A0 = c.S3 + 0u;
        c.A2 = 0x000000B0u;
        c.A3 = 0x00000094u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V1);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.RA = 0x800171E8u;
        GranTurismo2PC.func_80016A94(c, m);
        goto L80017200;
        L800171F0: ;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V1);
        c.A2 = 0x000000B0u;
        c.A3 = 0x000000B4u;
        c.RA = 0x80017200u;
        GranTurismo2PC.func_8006ADB4(c, m);
        L80017200: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x48u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x44u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x40u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x3Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x38u));
        c.SP = c.SP + 0x50u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001721C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.V0 = c.A0 < 0x00000009u ? 1u : 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.T0 = MemoryAccess.ReadU32(m, c.A1);
        c.S1 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.S3 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0xCu));
        c.S4 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0xEu));
        if (c.V0 == 0u) {
            c.S2 = 0u + 0u;
            goto L8001734C;
        }
        c.S2 = 0u + 0u;
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x2C0Cu;
        c.V1 = c.A0 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x8001734Cu: goto L8001734C;
            case 0x80017274u: goto L80017274;
            case 0x80017348u: goto L80017348;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L80017274: ;
        c.V1 = 0x66660000u;
        c.V0 = 0x80050000u;
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 - 0x479Cu));
        c.V1 = c.V1 | 0x6667u;
        c.A0 = c.A0 << 7;
        { var _r = (long)(int)c.A0 * (int)c.V1; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V0 = 0x80050000u;
        c.V0 = c.V0 - 0x475Cu;
        c.V1 = c.A2 << 1;
        c.V1 = c.V1 + c.V0;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.V1);
        c.A0 = (uint)((int)c.A0 >> 31);
        c.V1 = c.V0 << 1;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 2;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 - 0x4798u;
        c.T1 = c.HI;
        c.A1 = (uint)((int)c.T1 >> 3);
        c.A3 = c.A1 - c.A0;
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.T0 + 0x6u));
        if (c.A2 == c.A0) {
            c.S0 = c.V1 + c.V0;
            goto L800172D8;
        }
        c.S0 = c.V1 + c.V0;
        c.A3 = (uint)((int)c.A3 >> 1);
        L800172D8: ;
        c.A0 = c.S1 + 0u;
        c.A1 = c.A3 << 8;
        c.A1 = c.A3 | c.A1;
        c.V0 = c.A3 << 16;
        c.A1 = c.A1 | c.V0;
        c.RA = 0x800172F0u;
        GranTurismo2PC.func_80081478(c, m);
        c.A0 = MemoryAccess.ReadU16(m, (c.S0 + 0x4u));
        c.V1 = MemoryAccess.ReadU16(m, (c.S0 + 0x6u));
        c.A0 = c.A0 << 16;
        c.A0 = (uint)((int)c.A0 >> 17);
        c.A0 = c.S3 - c.A0;
        c.V1 = c.V1 << 16;
        c.V1 = (uint)((int)c.V1 >> 17);
        c.V1 = c.S4 - c.V1;
        c.V1 = c.V1 << 16;
        c.A0 = c.A0 + c.V1;
        MemoryAccess.WriteU32(m, c.V0, c.A0);
        c.V1 = MemoryAccess.ReadU32(m, c.S0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x4u), c.V1);
        c.V1 = MemoryAccess.ReadU32(m, (c.S0 + 0x4u));
        MemoryAccess.WriteU32(m, (c.V0 + 0x8u), c.V1);
        c.A1 = MemoryAccess.ReadU16(m, (c.S0 + 0x8u));
        c.A0 = c.S1 + 0u;
        c.RA = 0x80017340u;
        GranTurismo2PC.func_8007DA44(c, m);
        c.V0 = c.S2 + 0u;
        goto L80017350;
        L80017348: ;
        c.S2 = 0x00000001u;
        L8001734C: ;
        c.V0 = c.S2 + 0u;
        L80017350: ;
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
    public static void func_80017370(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.V0 - 0x4734u;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x80010000u;
        c.A1 = c.A1 + 0x721Cu;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.A2 = 0u + 0u;
        c.RA = 0x80017398u;
        GranTurismo2PC.func_8006CDCC(c, m);
        c.A0 = c.S0 + 0u;
        c.RA = 0x800173A0u;
        GranTurismo2PC.func_8006CE70(c, m);
        c.V0 = 0x801D0000u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6720u));
        if ((int)c.A0 >= 0) {
            c.V0 = (int)c.A0 < 7 ? 1u : 0u;
            goto L800173BC;
        }
        c.V0 = (int)c.A0 < 7 ? 1u : 0u;
        c.A0 = 0u + 0u;
        c.V0 = (int)c.A0 < 7 ? 1u : 0u;
        L800173BC: ;
        if (c.V0 != 0u) {
            c.V0 = 0x80050000u;
            goto L800173C8;
        }
        c.V0 = 0x80050000u;
        c.A0 = 0u + 0u;
        L800173C8: ;
        c.V1 = 0x80050000u;
        c.V1 = c.V1 - 0x4744u;
        MemoryAccess.WriteU16(m, (c.V0 - 0x479Cu), (ushort)0u);
        c.V0 = c.A0 << 1;
        c.V0 = c.V0 + c.V1;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, c.V0);
        MemoryAccess.WriteU16(m, (c.S0 + 0x6u), (ushort)c.V1);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x1224u), (ushort)0u);
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80017400(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V1 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.V1 + 0x1224u));
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        if (c.V1 != 0u) {
            c.A1 = c.V0 + 0x1A4u;
            goto L80017464;
        }
        c.A1 = c.V0 + 0x1A4u;
        c.A2 = 0x80050000u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A2 - 0x479Cu));
        c.V1 = MemoryAccess.ReadU16(m, (c.A2 - 0x479Cu));
        c.V0 = (int)c.V0 < 20 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.V1 + 0x1u;
            goto L8001745C;
        }
        c.V0 = c.V1 + 0x1u;
        MemoryAccess.WriteU16(m, (c.A2 - 0x479Cu), (ushort)c.V0);
        c.V0 = c.V0 << 16;
        c.V0 = (uint)((int)c.V0 >> 16);
        c.V0 = (int)c.V0 < 20 ? 1u : 0u;
        if (c.V0 != 0u) {
            goto L80017494;
        }
        L8001745C: ;
        c.V0 = 0x00000014u;
        goto L80017478;
        L80017464: ;
        c.A2 = 0x80050000u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A2 - 0x479Cu));
        c.V1 = MemoryAccess.ReadU16(m, (c.A2 - 0x479Cu));
        if ((int)c.V0 <= 0) {
            c.V0 = c.V1 - 0x1u;
            goto L8001747C;
        }
        c.V0 = c.V1 - 0x1u;
        L80017478: ;
        MemoryAccess.WriteU16(m, (c.A2 - 0x479Cu), (ushort)c.V0);
        L8001747C: ;
        c.V0 = 0x80050000u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 - 0x479Cu));
        c.V0 = (int)c.V0 < 20 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L80017498;
        }
        L80017494: ;
        c.A1 = 0u + 0u;
        L80017498: ;
        if (c.A0 != 0u) {
            goto L800174A4;
        }
        c.A1 = 0u + 0u;
        L800174A4: ;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4734u;
        c.RA = 0x800174B0u;
        GranTurismo2PC.func_8006CFC4(c, m);
        c.S0 = c.V0 + 0u;
        c.V0 = 0xFFFFFFFDu;
        if (c.S0 == c.V0) {
            c.V0 = (int)c.S0 < -2 ? 1u : 0u;
            goto L800174E8;
        }
        c.V0 = (int)c.S0 < -2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0xFFFFFFFCu;
            goto L800174D8;
        }
        c.V0 = 0xFFFFFFFCu;
        if (c.S0 == c.V0) {
            c.A0 = 0x00000003u;
            goto L8001754C;
        }
        c.A0 = 0x00000003u;
        c.S1 = 0x00000001u;
        goto L800174FC;
        L800174D8: ;
        if ((int)c.S0 >= 0) {
            c.A0 = 0x00000003u;
            goto L800174F8;
        }
        c.A0 = 0x00000003u;
        c.V0 = c.S1 + 0u;
        goto L80017550;
        L800174E8: ;
        c.A0 = 0x00000006u;
        c.RA = 0x800174F0u;
        GranTurismo2PC.func_80060840(c, m);
        c.V0 = c.S1 + 0u;
        goto L80017550;
        L800174F8: ;
        c.S1 = 0x00000001u;
        L800174FC: ;
        c.A2 = 0x800B0000u;
        c.A1 = 0x80050000u;
        c.V0 = MemoryAccess.ReadU16(m, (c.A1 - 0x479Cu));
        c.V1 = c.S1 + 0u;
        MemoryAccess.WriteU16(m, (c.A2 + 0x1224u), (ushort)c.V1);
        c.V0 = c.V0 - 0x1u;
        MemoryAccess.WriteU16(m, (c.A1 - 0x479Cu), (ushort)c.V0);
        c.RA = 0x8001751Cu;
        GranTurismo2PC.func_80060840(c, m);
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x4658u;
        c.V0 = 0x800B0000u;
        c.V1 = 0x80050000u;
        c.V1 = c.V1 - 0x4750u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
        c.V0 = c.S0 << (int)(c.S1 & 31u);
        c.V0 = c.V0 + c.V1;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, c.V0);
        c.V0 = 0x801D0000u;
        MemoryAccess.WriteU8(m, (c.V0 - 0x6720u), (byte)c.V1);
        c.RA = 0x8001754Cu;
        GranTurismo2PC.func_800122A0(c, m);
        L8001754C: ;
        c.V0 = c.S1 + 0u;
        L80017550: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80017564(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0x80050000u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 - 0x479Cu));
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S4 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.V0 = (int)c.V0 < 20 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.A0 = 0x80050000u;
            goto L800176C4;
        }
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4734u;
        c.A1 = c.S4 + 0u;
        c.RA = 0x800175A4u;
        GranTurismo2PC.func_8006D50C(c, m);
        c.V0 = 0x66660000u;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 - 0x479Cu));
        c.V0 = c.V0 | 0x6667u;
        c.V1 = c.V1 << 7;
        { var _r = (long)(int)c.V1 * (int)c.V0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.A0 = c.S4 + 0u;
        c.V1 = (uint)((int)c.V1 >> 31);
        c.A2 = c.HI;
        c.V0 = (uint)((int)c.A2 >> 3);
        c.V0 = c.V0 - c.V1;
        c.S0 = c.V0 << 8;
        c.S0 = c.V0 | c.S0;
        c.V0 = c.V0 << 16;
        c.S0 = c.S0 | c.V0;
        c.A1 = c.S0 + 0u;
        c.RA = 0x800175E4u;
        GranTurismo2PC.func_80080450(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = c.S4 + 0u;
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
        c.RA = 0x80017620u;
        GranTurismo2PC.func_80080450(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = c.S4 + 0u;
        c.A1 = c.S0 + 0u;
        c.V0 = 0x00000096u;
        c.S3 = 0x000000E0u;
        MemoryAccess.WriteU16(m, c.V1, (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x8u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.V1 + 0xAu), (ushort)c.S1);
        MemoryAccess.WriteU8(m, (c.V1 + 0xCu), (byte)0u);
        MemoryAccess.WriteU8(m, (c.V1 + 0xDu), (byte)0u);
        MemoryAccess.WriteU16(m, (c.V1 + 0x10u), (ushort)c.S1);
        MemoryAccess.WriteU16(m, (c.V1 + 0x12u), (ushort)c.S3);
        MemoryAccess.WriteU16(m, (c.V1 + 0xEu), (ushort)c.S2);
        c.RA = 0x80017658u;
        GranTurismo2PC.func_80080450(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = c.S4 + 0u;
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
        c.RA = 0x80017690u;
        GranTurismo2PC.func_80080450(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = c.S4 + 0u;
        c.A1 = 0x00000280u;
        c.V0 = 0x00000098u;
        MemoryAccess.WriteU16(m, c.V1, (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x8u), (ushort)c.S1);
        MemoryAccess.WriteU16(m, (c.V1 + 0xAu), (ushort)c.S1);
        MemoryAccess.WriteU8(m, (c.V1 + 0xCu), (byte)0u);
        MemoryAccess.WriteU8(m, (c.V1 + 0xDu), (byte)0u);
        MemoryAccess.WriteU16(m, (c.V1 + 0x10u), (ushort)c.S0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x12u), (ushort)c.S3);
        MemoryAccess.WriteU16(m, (c.V1 + 0xEu), (ushort)c.S2);
        c.RA = 0x800176C4u;
        GranTurismo2PC.func_8007DA44(c, m);
        L800176C4: ;
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
    public static void func_800176E4(CpuContext c, IMemory m)
    {
        c.V1 = 0x800B0000u;
        c.V0 = 0x0000000Cu;
        MemoryAccess.WriteU16(m, (c.V1 + 0x1226u), (ushort)c.V0);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800176F4(CpuContext c, IMemory m)
    {
        c.V1 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 + 0x1226u));
        c.V0 = c.V0 - 0x1u;
        MemoryAccess.WriteU16(m, (c.V1 + 0x1226u), (ushort)c.V0);
        c.V0 = c.V0 << 16;
        c.V0 = (int)c.V0 < 1 ? 1u : 0u;
        c.V0 = c.V0 << 2;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80017718(CpuContext c, IMemory m)
    {
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80017720(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        c.V1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.V0 = c.V0 >> 3;
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u) {
            c.A0 = c.V1 + 0x8u;
            goto L80017750;
        }
        c.A0 = c.V1 + 0x8u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.A0 = c.A0 + c.V0;
        L80017750: ;
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
        c.RA = 0x80017784u;
        GranTurismo2PC.func_8007BBD4(c, m);
        c.RA = 0x8001778Cu;
        GranTurismo2PC.func_8007AF30(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001779C(CpuContext c, IMemory m)
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
        c.V0 = c.V0 + 0x1230u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.V1 + c.V0;
        c.A0 = 0x801D0000u;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 - 0x43FCu;
        c.V1 = c.S1 << 1;
        c.V1 = c.V1 + c.V0;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0x10u));
        c.A2 = MemoryAccess.ReadU32(m, c.A1);
        c.V0 = (uint)RecompOne.Runtime.Sdk.GT2Compat.UnifiedTitleItemValidity(c.S1);
        c.S2 = MemoryAccess.ReadU8(m, (c.A0 - 0x6720u));
        if ((int)c.V0 < 0) {
            c.S3 = 0u + 0u;
            goto L8001790C;
        }
        c.S3 = 0u + 0u;
        c.V0 = c.T0 < 0x00000009u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80020000u;
            goto L8001790C;
        }
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x2C34u;
        c.V1 = c.T0 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x80017828u: goto L80017828;
            case 0x80017878u: goto L80017878;
            case 0x8001790Cu: goto L8001790C;
            case 0x80017880u: goto L80017880;
            case 0x80017890u: goto L80017890;
            case 0x800178FCu: goto L800178FC;
            case 0x80017908u: goto L80017908;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L80017828: ;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x43DCu;
        c.RA = 0x80017838u;
        GranTurismo2PC.func_80016394(c, m);
        c.V0 = RecompOne.Runtime.Sdk.GT2Compat.UnifiedTitleDescriptor(
            m, c.S1, c.S2);
        MemoryAccess.WriteU32(m, (c.S0 + 0xCu), c.V0);
        goto L8001790C;
        L80017878: ;
        MemoryAccess.WriteU16(m, (c.S0 + 0x10u), (ushort)0u);
        goto L8001790C;
        L80017880: ;
        c.A0 = c.S0 + 0u;
        c.RA = 0x80017888u;
        GranTurismo2PC.func_800163C8(c, m);
        c.V0 = c.S3 + 0u;
        goto L80017910;
        L80017890: ;
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
            goto L800178D0;
        }
        c.A3 = (uint)((int)c.V1 >> 7);
        c.A3 = (uint)((int)c.V1 >> 9);
        L800178D0: ;
        MemoryAccess.WriteU16(m, (c.S0 + 0x12u), (ushort)c.A3);
        MemoryAccess.WriteU32(m, 
            (c.S0 + 0xCu),
            RecompOne.Runtime.Sdk.GT2Compat.UnifiedTitleDescriptorForState(
                c.S1, c.A2));
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0xCu));
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0xEu));
        c.A0 = c.S0 + 0u;
        MemoryAccess.WriteU16(m, (c.A0 + 0x4u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.A0 + 0x6u), (ushort)c.V1);
        RecompOne.Runtime.Sdk.GT2Compat.PositionUnifiedTitleItem(
            m, c.A0, c.S1);
        c.A1 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.RA = 0x800178F4u;
        GranTurismo2PC.func_80016410(c, m);
        c.V0 = c.S3 + 0u;
        goto L80017910;
        L800178FC: ;
        c.V0 = 0x0000000Cu;
        MemoryAccess.WriteU16(m, (c.S0 + 0x10u), (ushort)c.V0);
        goto L8001790C;
        L80017908: ;
        c.S3 = 0x00000001u;
        L8001790C: ;
        c.V0 = c.S3 + 0u;
        L80017910: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001792C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        RecompOne.Runtime.Sdk.GT2Compat.InstallUnifiedTitleMenu(m);
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU16(m, (c.V0 - 0x4400u), (ushort)0u);
        c.V0 = 0x80050000u;
        c.V1 = 0x00000010u;
        MemoryAccess.WriteU16(m, (c.V0 - 0x43FEu), (ushort)c.V1);
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x122Cu), (ushort)0u);
        c.V0 = 0x800B0000u;
        c.A0 = 0x80050000u;
        c.A1 = 0x80010000u;
        c.A0 = c.A0 - 0x43D8u;
        c.A1 = c.A1 + 0x779Cu;
        c.A2 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU16(m, (c.V0 + 0x122Eu), (ushort)c.V1);
        c.RA = 0x80017970u;
        GranTurismo2PC.func_8006CDCC(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1228u), 0u);
        RecompOne.Runtime.Sdk.GT2Compat.CompleteUnifiedTitleMenuInitialization(m);
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80017984(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        c.S4 = 0u + 0u;
        c.A2 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.A2 + 0x122Cu));
        c.A1 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        if (c.V1 == c.A1) {
            c.S3 = c.V0 + 0x1A4u;
            goto L80017A28;
        }
        c.S3 = c.V0 + 0x1A4u;
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L800179E0;
        }
        c.V0 = 0x00000002u;
        if (c.V1 == 0u) {
            c.V1 = 0x800B0000u;
            goto L800179F0;
        }
        c.V1 = 0x800B0000u;
        goto L80017A9C;
        L800179E0: ;
        if (c.V1 == c.V0) {
            c.V1 = 0x80050000u;
            goto L80017A58;
        }
        c.V1 = 0x80050000u;
        goto L80017A9C;
        L800179F0: ;
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 + 0x122Eu));
        c.V0 = c.V0 - 0x1u;
        MemoryAccess.WriteU16(m, (c.V1 + 0x122Eu), (ushort)c.V0);
        c.V0 = c.V0 << 16;
        if (c.V0 != 0u) {
            c.V1 = 0x80050000u;
            goto L80017A2C;
        }
        c.V1 = 0x80050000u;
        c.S0 = 0x80050000u;
        c.S0 = c.S0 - 0x43D8u;
        c.A0 = c.S0 + 0u;
        MemoryAccess.WriteU16(m, (c.A2 + 0x122Cu), (ushort)c.A1);
        c.RA = 0x80017A20u;
        GranTurismo2PC.func_8006CE70(c, m);
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU16(m, (c.S0 + 0x1Cu), (ushort)c.V0);
        L80017A28: ;
        c.V1 = 0x80050000u;
        L80017A2C: ;
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 - 0x4400u));
        c.V0 = c.V0 + 0x1u;
        MemoryAccess.WriteU16(m, (c.V1 - 0x4400u), (ushort)c.V0);
        c.V0 = c.V0 << 16;
        c.V0 = (uint)((int)c.V0 >> 16);
        c.V0 = (int)c.V0 < 13 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = 0x0000000Cu;
            goto L80017A9C;
        }
        c.V0 = 0x0000000Cu;
        MemoryAccess.WriteU16(m, (c.V1 - 0x4400u), (ushort)c.V0);
        goto L80017A9C;
        L80017A58: ;
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 - 0x4400u));
        c.V0 = c.V0 - 0x1u;
        MemoryAccess.WriteU16(m, (c.V1 - 0x4400u), (ushort)c.V0);
        c.V0 = c.V0 << 16;
        if ((int)c.V0 >= 0) {
            goto L80017A78;
        }
        MemoryAccess.WriteU16(m, (c.V1 - 0x4400u), (ushort)0u);
        L80017A78: ;
        c.V1 = 0x80050000u;
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 - 0x43FEu));
        c.V0 = c.V0 - 0x1u;
        MemoryAccess.WriteU16(m, (c.V1 - 0x43FEu), (ushort)c.V0);
        c.V0 = c.V0 << 16;
        if ((int)c.V0 >= 0) {
            goto L80017A9C;
        }
        c.S4 = 0x00000004u;
        L80017A9C: ;
        RecompOne.Runtime.Sdk.GT2Compat.BufferUnifiedTitleInput(c.S3, m);
        c.V0 = MemoryAccess.ReadU32(m, c.S3);
        if (c.V0 == 0u) {
            c.V0 = 0x800B0000u;
            goto L80017AB4;
        }
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1228u), 0u);
        goto L80017AF4;
        L80017AB4: ;
        c.V1 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x1228u));
        c.V0 = c.V0 + 0x1u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x1228u), c.V0);
        c.V0 = (int)c.V0 < 901 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.A0 = 0x80050000u;
            goto L80017AF4;
        }
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x43D8u;
        c.RA = 0x80017ADCu;
        GranTurismo2PC.func_8006CED8(c, m);
        c.V1 = 0x800B0000u;
        c.V0 = 0x00000002u;
        MemoryAccess.WriteU16(m, (c.V1 + 0x122Cu), (ushort)c.V0);
        c.V1 = 0x801F0000u;
        c.V0 = 0x00000006u;
        MemoryAccess.WriteU8(m, (c.V1 - 0xA0Du), (byte)c.V0);
        L80017AF4: ;
        if (c.S1 != 0u) {
            c.V0 = 0x80050000u;
            goto L80017B00;
        }
        c.V0 = 0x80050000u;
        c.S3 = 0u + 0u;
        L80017B00: ;
        c.S1 = c.V0 - 0x43D8u;
        c.A0 = c.S1 + 0u;
        c.S2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x6u));
        c.A1 = c.S3 + 0u;
        c.RA = 0x80017B14u;
        GranTurismo2PC.func_8006CFC4(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = 0x00000001u;
        c.S0 = c.V0 + 0u;
        c.A2 = 0x00000004u;
        c.RA = 0x80017B28u;
        GranTurismo2PC.func_8006D4B0(c, m);
        c.V0 = 0xFFFFFFFDu;
        if (c.S0 == c.V0) {
            c.V0 = (int)c.S0 < -2 ? 1u : 0u;
            goto L80017B5C;
        }
        c.V0 = (int)c.S0 < -2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0xFFFFFFFCu;
            goto L80017B4C;
        }
        c.V0 = 0xFFFFFFFCu;
        if (c.S0 == c.V0) {
            c.V0 = c.S4 + 0u;
            goto L80017BE0;
        }
        c.V0 = c.S4 + 0u;
        goto L80017B7C;
        L80017B4C: ;
        if ((int)c.S0 >= 0) {
            c.V0 = c.S4 + 0u;
            goto L80017B7C;
        }
        c.V0 = c.S4 + 0u;
        goto L80017BE0;
        L80017B5C: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x6u));
        if (c.S2 == c.V0) {
            c.V0 = c.S4 + 0u;
            goto L80017BE0;
        }
        c.V0 = c.S4 + 0u;
        c.A0 = 0x00000006u;
        c.RA = 0x80017B74u;
        GranTurismo2PC.func_80060840(c, m);
        c.V0 = c.S4 + 0u;
        goto L80017BE0;
        L80017B7C: ;
        RecompOne.Runtime.Sdk.GT2Compat.BeginUnifiedTitleConfirmationAudio(
            c.S0);
        c.A0 = 0x00000003u;
        c.RA = 0x80017B84u;
        GranTurismo2PC.func_80060840(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x43D8u;
        c.RA = 0x80017B90u;
        GranTurismo2PC.func_8006CED8(c, m);
        c.V1 = 0x800B0000u;
        c.V0 = 0x00000002u;
        MemoryAccess.WriteU16(m, (c.V1 + 0x122Cu), (ushort)c.V0);
        c.V0 = 0x801F0000u;
        c.A0 = c.V0 - 0xA10u;
        MemoryAccess.WriteU8(m, (c.A0 + 0x11u), (byte)0u);
        c.V0 = MemoryAccess.ReadU32(m, c.S3);
        c.V1 = 0x00010000u;
        c.V0 = c.V0 & c.V1;
        if (c.V0 == 0u) {
            c.V0 = 0x00000001u;
            goto L80017BC0;
        }
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.A0 + 0x11u), (byte)c.V0);
        L80017BC0: ;
        RecompOne.Runtime.Sdk.GT2Compat.CommitUnifiedTitleSelection(
            m, c.S0, c.A0);
        c.V0 = c.S4 + 0u;
        L80017BE0: ;
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
    public static void func_80017C00(CpuContext c, IMemory m)
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
        c.A0 = c.V0 - 0x43D8u;
        c.A1 = c.S3 + 0u;
        c.RA = 0x80017C30u;
        GranTurismo2PC.func_8006D50C(c, m);
        c.A0 = 0x2AAA0000u;
        c.V0 = 0x80050000u;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 - 0x4400u));
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
        c.RA = 0x80017C74u;
        GranTurismo2PC.func_80080450(c, m);
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
        c.RA = 0x80017CB0u;
        GranTurismo2PC.func_80080450(c, m);
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
        c.RA = 0x80017CE8u;
        GranTurismo2PC.func_80080450(c, m);
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
        c.RA = 0x80017D20u;
        GranTurismo2PC.func_80080450(c, m);
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
        c.RA = 0x80017D54u;
        GranTurismo2PC.func_8007DA44(c, m);
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
    public static void func_80017D74(CpuContext c, IMemory m)
    {
        c.V0 = 0x801D0000u;
        c.V1 = (uint)(sbyte)MemoryAccess.ReadU8(m, c.A0);
        c.A0 = c.V0 - 0x6720u;
        c.V0 = c.V1 < 0x0000000Fu ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A1 = 0u + 0u;
            goto L80017E60;
        }
        c.A1 = 0u + 0u;
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x2C5Cu;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x80017DACu: goto L80017DAC;
            case 0x80017DB8u: goto L80017DB8;
            case 0x80017DC4u: goto L80017DC4;
            case 0x80017DD0u: goto L80017DD0;
            case 0x80017DDCu: goto L80017DDC;
            case 0x80017DE8u: goto L80017DE8;
            case 0x80017DF4u: goto L80017DF4;
            case 0x80017E00u: goto L80017E00;
            case 0x80017E0Cu: goto L80017E0C;
            case 0x80017E18u: goto L80017E18;
            case 0x80017E24u: goto L80017E24;
            case 0x80017E30u: goto L80017E30;
            case 0x80017E3Cu: goto L80017E3C;
            case 0x80017E48u: goto L80017E48;
            case 0x80017E54u: goto L80017E54;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L80017DAC: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.A0 + 0x2u));
        c.V0 = c.A1 + 0u;
        return;
        L80017DB8: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.A0 + 0x3u));
        c.V0 = c.A1 + 0u;
        return;
        L80017DC4: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.A0 + 0x4u));
        c.V0 = c.A1 + 0u;
        return;
        L80017DD0: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.A0 + 0x5u));
        c.V0 = c.A1 + 0u;
        return;
        L80017DDC: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.A0 + 0x6u));
        c.V0 = c.A1 + 0u;
        return;
        L80017DE8: ;
        c.A1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.A0 + 0x7u));
        c.V0 = c.A1 + 0u;
        return;
        L80017DF4: ;
        c.A1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.A0 + 0x8u));
        c.V0 = c.A1 + 0u;
        return;
        L80017E00: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.A0 + 0xAEu));
        c.V0 = c.A1 + 0u;
        return;
        L80017E0C: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.A0 + 0xAFu));
        c.V0 = c.A1 + 0u;
        return;
        L80017E18: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.A0 + 0xB0u));
        c.V0 = c.A1 + 0u;
        return;
        L80017E24: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.A0 + 0xB1u));
        c.V0 = c.A1 + 0u;
        return;
        L80017E30: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.A0 + 0xB2u));
        c.V0 = c.A1 + 0u;
        return;
        L80017E3C: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.A0 + 0xB3u));
        c.V0 = c.A1 + 0u;
        return;
        L80017E48: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.A0 + 0xB4u));
        c.V0 = c.A1 + 0u;
        return;
        L80017E54: ;
        c.V0 = MemoryAccess.ReadU8(m, (c.A0 + 0x36u));
        c.A1 = c.V0 < 0x00000001u ? 1u : 0u;
        L80017E60: ;
        c.V0 = c.A1 + 0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80017E68(CpuContext c, IMemory m)
    {
        c.V0 = 0x801D0000u;
        c.V1 = (uint)(sbyte)MemoryAccess.ReadU8(m, c.A0);
        c.A0 = c.V0 - 0x6720u;
        c.V0 = c.V1 < 0x0000000Fu ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80020000u;
            goto L80017F24;
        }
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x2C9Cu;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x80017E9Cu: goto L80017E9C;
            case 0x80017EA8u: goto L80017EA8;
            case 0x80017EB0u: goto L80017EB0;
            case 0x80017EB8u: goto L80017EB8;
            case 0x80017EC4u: goto L80017EC4;
            case 0x80017ECCu: goto L80017ECC;
            case 0x80017ED4u: goto L80017ED4;
            case 0x80017EDCu: goto L80017EDC;
            case 0x80017EE4u: goto L80017EE4;
            case 0x80017EECu: goto L80017EEC;
            case 0x80017EF4u: goto L80017EF4;
            case 0x80017EFCu: goto L80017EFC;
            case 0x80017F04u: goto L80017F04;
            case 0x80017F0Cu: goto L80017F0C;
            case 0x80017F14u: goto L80017F14;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L80017E9C: ;
        c.V0 = 0u < c.A1 ? 1u : 0u;
        MemoryAccess.WriteU8(m, (c.A0 + 0x2u), (byte)c.V0);
        return;
        L80017EA8: ;
        MemoryAccess.WriteU8(m, (c.A0 + 0x3u), (byte)c.A1);
        return;
        L80017EB0: ;
        MemoryAccess.WriteU8(m, (c.A0 + 0x4u), (byte)c.A1);
        return;
        L80017EB8: ;
        c.V0 = 0u < c.A1 ? 1u : 0u;
        MemoryAccess.WriteU8(m, (c.A0 + 0x5u), (byte)c.V0);
        return;
        L80017EC4: ;
        MemoryAccess.WriteU8(m, (c.A0 + 0x6u), (byte)c.A1);
        return;
        L80017ECC: ;
        MemoryAccess.WriteU8(m, (c.A0 + 0x7u), (byte)c.A1);
        return;
        L80017ED4: ;
        MemoryAccess.WriteU8(m, (c.A0 + 0x8u), (byte)c.A1);
        return;
        L80017EDC: ;
        MemoryAccess.WriteU8(m, (c.A0 + 0xAEu), (byte)c.A1);
        return;
        L80017EE4: ;
        MemoryAccess.WriteU8(m, (c.A0 + 0xAFu), (byte)c.A1);
        return;
        L80017EEC: ;
        MemoryAccess.WriteU8(m, (c.A0 + 0xB0u), (byte)c.A1);
        return;
        L80017EF4: ;
        MemoryAccess.WriteU8(m, (c.A0 + 0xB1u), (byte)c.A1);
        return;
        L80017EFC: ;
        MemoryAccess.WriteU8(m, (c.A0 + 0xB2u), (byte)c.A1);
        return;
        L80017F04: ;
        MemoryAccess.WriteU8(m, (c.A0 + 0xB3u), (byte)c.A1);
        return;
        L80017F0C: ;
        MemoryAccess.WriteU8(m, (c.A0 + 0xB4u), (byte)c.A1);
        return;
        L80017F14: ;
        c.V0 = c.A1 ^ 0x0001u;
        c.V0 = 0u < c.V0 ? 1u : 0u;
        MemoryAccess.WriteU8(m, (c.A0 + 0x36u), (byte)c.V0);
        MemoryAccess.WriteU8(m, (c.A0 + 0x88u), (byte)c.V0);
        L80017F24: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80017F2C(CpuContext c, IMemory m)
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
            goto L80017F90;
        }
        c.S4 = c.A2 + 0u;
        c.V0 = (int)c.V1 < 3 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000001u;
            goto L80017F7C;
        }
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            goto L80017FB0;
        }
        goto L80017FEC;
        L80017F7C: ;
        c.V0 = 0x00000003u;
        if (c.V1 == c.V0) {
            goto L80017FE8;
        }
        goto L80017FEC;
        L80017F90: ;
        c.A0 = c.S2 + 0u;
        c.RA = 0x80017F98u;
        GranTurismo2PC.func_80017D74(c, m);
        c.S0 = c.V0 + 0u;
        c.S3 = c.S0 + 0u;
        c.V0 = c.S1 + c.S4;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x2u));
        c.S0 = c.S0 + c.V0;
        goto L80018004;
        L80017FB0: ;
        c.A0 = c.S2 + 0u;
        c.RA = 0x80017FB8u;
        GranTurismo2PC.func_80017D74(c, m);
        c.S1 = c.S1 << 4;
        c.A0 = c.S2 + 0u;
        c.S3 = c.V0 + 0u;
        c.RA = 0x80017FC8u;
        GranTurismo2PC.func_80017D74(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = c.V1 + 0xFu;
        if ((int)c.V0 >= 0) {
            c.V0 = (uint)((int)c.V0 >> 4);
            goto L80017FE0;
        }
        c.V0 = (uint)((int)c.V0 >> 4);
        c.V0 = c.V1 + 0x1Eu;
        c.V0 = (uint)((int)c.V0 >> 4);
        L80017FE0: ;
        c.S0 = c.V0 << 4;
        goto L80017FFC;
        L80017FE8: ;
        c.S1 = c.S1 + c.S4;
        L80017FEC: ;
        c.A0 = c.S2 + 0u;
        c.RA = 0x80017FF4u;
        GranTurismo2PC.func_80017D74(c, m);
        c.S0 = c.V0 + 0u;
        c.S3 = c.S0 + 0u;
        L80017FFC: ;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x2u));
        c.S0 = c.S0 + c.S1;
        L80018004: ;
        c.V0 = (int)c.S0 < (int)c.V1 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L80018014;
        }
        c.S0 = c.V1 + 0u;
        L80018014: ;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x4u));
        c.V0 = (int)c.S0 < (int)c.V1 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.A0 = c.S2 + 0u;
            goto L8001802C;
        }
        c.A0 = c.S2 + 0u;
        c.S0 = c.V1 - 0x1u;
        L8001802C: ;
        c.A1 = c.S0 + 0u;
        c.RA = 0x80018034u;
        GranTurismo2PC.func_80017E68(c, m);
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
    public static void func_8001805C_gt2_overlay_1(CpuContext c, IMemory m)
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
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x4330u));
        MemoryAccess.WriteU32(m, (c.SP + 0xF4u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0xE4u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0xE0u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0xDCu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0xD8u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0xD4u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0xFCu), c.A1);
        MemoryAccess.WriteU32(m, (c.SP + 0x104u), c.A3);
        MemoryAccess.WriteU32(m, (c.SP + 0xC0u), c.V0);
        c.RA = 0x800180B0u;
        GranTurismo2PC.func_80017D74(c, m);
        if (c.S0 == 0u) {
            c.S4 = c.V0 + 0u;
            goto L800180C8;
        }
        c.S4 = c.V0 + 0u;
        c.V0 = 0x80050000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x432Cu));
        MemoryAccess.WriteU32(m, (c.SP + 0xC0u), c.V0);
        L800180C8: ;
        c.A0 = c.SP + 0x20u;
        c.A1 = 0x0000001Eu;
        c.RA = 0x800180D4u;
        GranTurismo2PC.func_8006AC68(c, m);
        c.V1 = 0xFF9F0000u;
        c.V1 = c.V1 | 0xFFFFu;
        c.S1 = c.SP + 0x20u;
        c.A0 = c.S1 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6A40u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0xCu));
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xFCu));
        c.V0 = c.V0 & c.V1;
        c.V1 = 0x00200000u;
        c.V0 = c.V0 | c.V1;
        MemoryAccess.WriteU32(m, (c.S1 + 0x10u), c.T0);
        MemoryAccess.WriteU32(m, (c.S1 + 0xCu), c.V0);
        c.RA = 0x8001810Cu;
        GranTurismo2PC.func_8007DA80(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4334u;
        c.S5 = c.SP + 0xC0u;
        c.A1 = c.S5 + 0u;
        c.A2 = c.FP + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x80018128u;
        GranTurismo2PC.func_8006B548(c, m);
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
        c.RA = 0x80018158u;
        GranTurismo2PC.func_8006AF40(c, m);
        c.S2 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S7 + 0x1u));
        if (c.S2 == c.S3) {
            c.V0 = (int)c.S2 < 2 ? 1u : 0u;
            goto L8001823C;
        }
        c.V0 = (int)c.S2 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L80018180;
        }
        if (c.S2 == 0u) {
            c.S0 = 0u + 0u;
            goto L8001819C;
        }
        c.S0 = 0u + 0u;
        goto L80018544;
        L80018180: ;
        c.V0 = 0x00000002u;
        if (c.S2 == c.V0) {
            c.V0 = 0x00000003u;
            goto L800182D0;
        }
        c.V0 = 0x00000003u;
        if (c.S2 == c.V0) {
            c.S0 = c.SP + 0x80u;
            goto L800184C8;
        }
        c.S0 = c.SP + 0x80u;
        goto L80018544;
        L8001819C: ;
        c.S2 = c.S1 + 0u;
        c.S5 = 0x00000001u;
        c.S3 = 0xFFFFFFFEu;
        L800181A8: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S7 + 0x4u));
        c.V0 = (int)c.S0 < (int)c.V0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.T0 = 0x80050000u;
            goto L80018544;
        }
        c.T0 = 0x80050000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.T0 - 0x4328u));
        MemoryAccess.WriteU32(m, (c.SP + 0xC4u), c.V0);
        c.V1 = MemoryAccess.ReadU32(m, (c.S7 + 0x10u));
        c.V0 = c.S0 << 2;
        c.V0 = c.V0 + c.V1;
        c.S1 = MemoryAccess.ReadU32(m, c.V0);
        if (c.S0 != c.S4) {
            c.T0 = 0x80050000u;
            goto L800181F4;
        }
        c.T0 = 0x80050000u;
        c.T0 = 0x80050000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.T0 - 0x4324u));
        MemoryAccess.WriteU32(m, (c.SP + 0xC4u), c.V0);
        c.T0 = 0x80050000u;
        L800181F4: ;
        c.A0 = c.T0 - 0x4334u;
        c.A1 = c.SP + 0xC4u;
        c.A2 = c.FP + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x80018208u;
        GranTurismo2PC.func_8006B548(c, m);
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
        c.RA = 0x80018234u;
        GranTurismo2PC.func_8006AF40(c, m);
        c.S6 = c.S6 + 0x3Cu;
        goto L800181A8;
        L8001823C: ;
        c.S0 = 0u + 0u;
        c.S5 = 0x80050000u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x104u));
        c.S2 = 0x00000005u;
        c.S1 = 0x00000014u;
        c.S3 = c.T0 - 0x14u;
        L80018254: ;
        c.T0 = 0x80050000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.T0 - 0x4318u));
        MemoryAccess.WriteU32(m, (c.SP + 0xC0u), c.V0);
        c.V0 = c.S0 << 4;
        c.V0 = (int)c.V0 < (int)c.S4 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.A0 = c.S5 - 0x4334u;
            goto L80018284;
        }
        c.A0 = c.S5 - 0x4334u;
        c.T0 = 0x80050000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.T0 - 0x431Cu));
        MemoryAccess.WriteU32(m, (c.SP + 0xC0u), c.V0);
        L80018284: ;
        c.A1 = c.SP + 0xC0u;
        c.A2 = c.FP + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x80018294u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0xFCu));
        c.A1 = c.V0 + 0u;
        c.RA = 0x800182A0u;
        GranTurismo2PC.func_8007D024(c, m);
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
            goto L80018254;
        }
        c.S6 = c.S6 + 0x6u;
        goto L80018544;
        L800182D0: ;
        c.V0 = 0x02780000u;
        c.V0 = c.V0 | 0x5030u;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4334u;
        c.A1 = c.S5 + 0u;
        c.A2 = c.FP + 0u;
        c.A3 = 0x00000080u;
        MemoryAccess.WriteU32(m, (c.SP + 0xC0u), c.V0);
        c.RA = 0x800182F4u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0xFCu));
        c.A1 = c.V0 + 0u;
        c.RA = 0x80018300u;
        GranTurismo2PC.func_8007D024(c, m);
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
        c.A0 = c.A0 - 0x4334u;
        c.S0 = c.S6 + c.S0;
        MemoryAccess.WriteU32(m, (c.SP + 0xC0u), c.V0);
        c.S0 = c.S0 + c.V1;
        c.RA = 0x80018394u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0xFCu));
        c.A1 = c.V0 + 0u;
        c.RA = 0x800183A0u;
        GranTurismo2PC.func_8007D024(c, m);
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
            goto L800183E4;
        }
        MemoryAccess.WriteU16(m, (c.A0 + 0x6u), (ushort)c.V0);
        c.V1 = 0u - c.S4;
        c.V0 = c.V1 << 2;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 1;
        MemoryAccess.WriteU32(m, (c.SP + 0xC8u), c.V0);
        goto L800183F0;
        L800183E4: ;
        c.V0 = c.S4 << 2;
        c.V0 = c.V0 + c.S4;
        c.FP = c.V0 << 1;
        L800183F0: ;
        c.A0 = c.SP + 0x20u;
        c.S0 = 0x801C0000u;
        c.S0 = c.S0 - 0x6353u;
        c.A1 = c.S0 + 0u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x104u));
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S7 + 0x8u));
        c.S2 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S2);
        c.S3 = c.T0 + 0x12u;
        c.A3 = c.S3 + 0u;
        c.A2 = c.S6 + c.A2;
        c.A2 = c.A2 + 0x10u;
        c.RA = 0x80018424u;
        GranTurismo2PC.func_8006AE28(c, m);
        c.S1 = c.SP + 0x40u;
        c.A0 = c.S1 + 0u;
        c.S5 = c.S0 + 0xAu;
        c.A1 = c.S5 + 0u;
        c.A2 = c.FP + 0u;
        c.RA = 0x8001843Cu;
        GranTurismo2PC.func_8008CF34(c, m);
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
        c.RA = 0x80018468u;
        GranTurismo2PC.func_8006B184(c, m);
        c.A0 = c.SP + 0x20u;
        c.A1 = c.S0 + 0x5u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S7 + 0x8u));
        c.A3 = c.S3 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S2);
        c.A2 = c.S6 + c.A2;
        c.A2 = c.A2 + 0x60u;
        c.RA = 0x80018488u;
        GranTurismo2PC.func_8006AE28(c, m);
        c.A0 = c.S1 + 0u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0xC8u));
        c.A1 = c.S5 + 0u;
        c.RA = 0x80018498u;
        GranTurismo2PC.func_8008CF34(c, m);
        c.A0 = c.SP + 0x20u;
        c.A1 = c.S1 + 0u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S7 + 0x8u));
        c.A3 = c.S3 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.A2 = c.S6 + c.A2;
        c.A2 = c.A2 + 0x88u;
        c.RA = 0x800184C0u;
        GranTurismo2PC.func_8006B184(c, m);
        goto L80018544;
        L800184C8: ;
        c.A0 = c.S0 + 0u;
        c.V0 = 0x801C0000u;
        c.S2 = c.V0 - 0x64D7u;
        c.A1 = c.S2 + 0u;
        c.RA = 0x800184DCu;
        GranTurismo2PC.func_8008CEDC(c, m);
        c.V0 = (int)c.S4 < 2 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.A0 = c.S0 + 0u;
            goto L800184F4;
        }
        c.A0 = c.S0 + 0u;
        c.A1 = c.S2 + 0x9u;
        c.A2 = c.S4 + 0u;
        c.RA = 0x800184F4u;
        GranTurismo2PC.func_8008CF34(c, m);
        L800184F4: ;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4334u;
        c.A1 = c.S5 + 0u;
        c.A2 = c.FP + 0u;
        c.V0 = 0x80050000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x4318u));
        c.A3 = 0x00000080u;
        MemoryAccess.WriteU32(m, (c.SP + 0xC0u), c.V0);
        c.RA = 0x80018518u;
        GranTurismo2PC.func_8006B548(c, m);
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
        c.RA = 0x80018544u;
        GranTurismo2PC.func_8006AF40(c, m);
        L80018544: ;
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
    public static void func_80018574(CpuContext c, IMemory m)
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
            goto L80018688;
        }
        c.T5 = (uint)((int)c.T4 >> 7);
        c.V0 = (int)c.T1 < 5 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000003u;
            GranTurismo2PC.func_800186D0(c, m);
            return;
        }
        c.V0 = 0x00000003u;
        if (c.T1 != c.V0) {
            GranTurismo2PC.func_800186D0(c, m);
            return;
        }
        if (c.T0 != c.V1) {
            GranTurismo2PC.func_800186D0(c, m);
            return;
        }
        if (c.A1 == 0u) {
            c.S0 = 0u + 0u;
            GranTurismo2PC.func_800186D0(c, m);
            return;
        }
        c.S0 = 0u + 0u;
        c.V1 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.V0 = MemoryAccess.ReadU32(m, (c.A1 + 0xCu));
        c.V1 = c.V1 | c.V0;
        c.V0 = c.V1 & 0x0004u;
        if (c.V0 == 0u) {
            c.A2 = c.S0 + 0u;
            goto L8001861C;
        }
        c.A2 = c.S0 + 0u;
        c.S0 = 0xFFFFFFFFu;
        L8001861C: ;
        c.V0 = c.V1 & 0x0008u;
        if (c.V0 == 0u) {
            goto L8001862C;
        }
        c.S0 = 0x00000001u;
        L8001862C: ;
        c.V1 = MemoryAccess.ReadU32(m, c.A1);
        c.V0 = c.V1 & 0x0010u;
        if (c.V0 == 0u) {
            c.V0 = c.V1 & 0x1000u;
            goto L80018644;
        }
        c.V0 = c.V1 & 0x1000u;
        c.A2 = 0xFFFFFFFFu;
        L80018644: ;
        if (c.V0 == 0u) {
            c.V0 = 0u < c.S0 ? 1u : 0u;
            goto L80018650;
        }
        c.V0 = 0u < c.S0 ? 1u : 0u;
        c.A2 = 0x00000001u;
        L80018650: ;
        c.V1 = 0u < c.A2 ? 1u : 0u;
        c.V0 = c.V0 | c.V1;
        if (c.V0 == 0u) {
            c.A0 = c.T6 + 0u;
            GranTurismo2PC.func_800186D0(c, m);
            return;
        }
        c.A0 = c.T6 + 0u;
        c.A1 = c.S0 + 0u;
        c.RA = 0x80018668u;
        GranTurismo2PC.func_80017F2C(c, m);
        if (c.V0 == 0u) {
            GranTurismo2PC.func_800186D0(c, m);
            return;
        }
        if (c.S0 == 0u) {
            GranTurismo2PC.func_800186D0(c, m);
            return;
        }
        c.A0 = 0x00000005u;
        c.RA = 0x80018680u;
        GranTurismo2PC.func_80060840(c, m);
        GranTurismo2PC.func_800186D0(c, m);
        return;
        L80018688: ;
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
            goto L800186B8;
        }
        c.A0 = c.T6 + 0u;
        c.T5 = (uint)((int)c.T4 >> 8);
        L800186B8: ;
        c.A1 = c.T2 + 0u;
        GranTurismo2PC.func_800186BC(c, m);
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800186BC(CpuContext c, IMemory m)
    {
        c.A2 = c.T3 + 0u;
        c.V0 = c.T0 < 0x00000001u ? 1u : 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T5);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V0);
        c.RA = 0x800186D0u;
        Dispatcher.Call(c, m, 0x8001805Cu);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.V0 = 0x00000001u;
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800186D0(CpuContext c, IMemory m)
    {
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.V0 = 0x00000001u;
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800186E4(CpuContext c, IMemory m)
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
        c.RA = 0x80018740u;
        GranTurismo2PC.func_8006AC68(c, m);
        c.V1 = 0xFF9F0000u;
        c.V1 = c.V1 | 0xFFFFu;
        c.S1 = c.SP + 0x18u;
        c.A0 = c.S1 + 0u;
        c.A1 = 0x801C0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0xCu));
        c.A1 = c.A1 - 0x6A10u;
        c.V0 = c.V0 & c.V1;
        c.V1 = 0x00200000u;
        c.V0 = c.V0 | c.V1;
        MemoryAccess.WriteU32(m, (c.S1 + 0x10u), c.S7);
        MemoryAccess.WriteU32(m, (c.S1 + 0xCu), c.V0);
        c.RA = 0x80018774u;
        GranTurismo2PC.func_8007DA80(c, m);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x9u));
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x68u));
        if (c.V0 == c.T0) {
            c.S3 = 0x00000001u;
            goto L8001878C;
        }
        c.S3 = 0x00000001u;
        c.S2 = 0u + 0u;
        L8001878C: ;
        if (c.S6 == c.S3) {
            c.V0 = (int)c.S6 < 2 ? 1u : 0u;
            goto L80018B88;
        }
        c.V0 = (int)c.S6 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000003u;
            goto L800187AC;
        }
        c.V0 = 0x00000003u;
        if (c.S6 == 0u) {
            goto L800189B0;
        }
        goto L80018EC8;
        L800187AC: ;
        if (c.S6 == c.V0) {
            c.V0 = 0x00000004u;
            goto L800187C4;
        }
        c.V0 = 0x00000004u;
        if (c.S6 == c.V0) {
            goto L80018888;
        }
        goto L80018EC8;
        L800187C4: ;
        MemoryAccess.WriteU8(m, (c.S0 + 0x10u), (byte)c.S3);
        MemoryAccess.WriteU8(m, (c.S0 + 0x11u), (byte)c.S3);
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x68u));
        c.V0 = c.T0 < 0x00000005u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80020000u;
            goto L80018EC8;
        }
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x2CDCu;
        c.V1 = c.T0 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x800187FCu: goto L800187FC;
            case 0x80018808u: goto L80018808;
            case 0x80018814u: goto L80018814;
            case 0x80018828u: goto L80018828;
            case 0x80018848u: goto L80018848;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L800187FC: ;
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU16(m, (c.V0 - 0x421Eu), (ushort)0u);
        goto L80018878;
        L80018808: ;
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU16(m, (c.V0 - 0x40E6u), (ushort)0u);
        goto L80018878;
        L80018814: ;
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x12D0u;
        c.RA = 0x80018820u;
        GranTurismo2PC.func_8001A834(c, m);
        goto L80018878;
        L80018828: ;
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x12D8u;
        c.RA = 0x80018834u;
        GranTurismo2PC.func_8001C690(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 == 0u) {
            goto L80018878;
        }
        goto L80018860;
        L80018848: ;
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x1358u;
        c.RA = 0x80018854u;
        GranTurismo2PC.func_8001C690(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 == 0u) {
            goto L80018878;
        }
        L80018860: ;
        c.A0 = 0u + 0u;
        c.RA = 0x80018868u;
        GranTurismo2PC.func_80060840(c, m);
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x10u), (byte)0u);
        MemoryAccess.WriteU8(m, (c.S0 + 0x11u), (byte)c.V0);
        goto L80018EC8;
        L80018878: ;
        c.A0 = 0x00000001u;
        c.RA = 0x80018880u;
        GranTurismo2PC.func_80060840(c, m);
        goto L80018EC8;
        L80018888: ;
        c.V1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0xEu));
        c.V0 = c.V1 < 0x00000005u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80020000u;
            goto L80018928;
        }
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x2CF4u;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x800188B8u: goto L800188B8;
            case 0x800188E0u: goto L800188E0;
            case 0x80018928u: goto L80018928;
            case 0x8001890Cu: goto L8001890C;
            case 0x80018918u: goto L80018918;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L800188B8: ;
        c.V1 = 0x80050000u;
        c.V1 = c.V1 - 0x41CCu;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x41B0u;
        c.A1 = 0x80050000u;
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 + 0x4u));
        c.A1 = c.A1 - 0x4194u;
        c.V0 = ~(0u | c.V0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x18u), (ushort)c.V0);
        goto L800188F0;
        L800188E0: ;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4094u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x4078u;
        L800188F0: ;
        c.V0 = MemoryAccess.ReadU16(m, (c.A0 + 0x4u));
        c.V1 = MemoryAccess.ReadU16(m, (c.A1 + 0x4u));
        c.V0 = ~(0u | c.V0);
        c.V1 = ~(0u | c.V1);
        MemoryAccess.WriteU16(m, (c.A0 + 0x18u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.A1 + 0x18u), (ushort)c.V1);
        goto L80018928;
        L8001890C: ;
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x12D8u;
        goto L80018920;
        L80018918: ;
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x1358u;
        L80018920: ;
        c.RA = 0x80018928u;
        GranTurismo2PC.func_8001C648(c, m);
        L80018928: ;
        c.V1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x9u));
        c.V0 = c.V1 < 0x00000005u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80020000u;
            goto L80018EC8;
        }
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x2D0Cu;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x80018958u: goto L80018958;
            case 0x80018974u: goto L80018974;
            case 0x80018EC8u: goto L80018EC8;
            case 0x80018988u: goto L80018988;
            case 0x8001899Cu: goto L8001899C;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L80018958: ;
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU16(m, (c.V0 - 0x41B4u), (ushort)0u);
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU16(m, (c.V0 - 0x4198u), (ushort)0u);
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU16(m, (c.V0 - 0x417Cu), (ushort)0u);
        goto L80018EC8;
        L80018974: ;
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU16(m, (c.V0 - 0x407Cu), (ushort)0u);
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU16(m, (c.V0 - 0x4060u), (ushort)0u);
        goto L80018EC8;
        L80018988: ;
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x12D8u;
        c.RA = 0x80018994u;
        GranTurismo2PC.func_8001C610(c, m);
        goto L80018EC8;
        L8001899C: ;
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x1358u;
        c.RA = 0x800189A8u;
        GranTurismo2PC.func_8001C610(c, m);
        goto L80018EC8;
        L800189B0: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x10u));
        if (c.V0 != 0u) {
            goto L800189C4;
        }
        c.S2 = 0u + 0u;
        L800189C4: ;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x68u));
        c.V0 = c.T0 < 0x00000005u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80020000u;
            goto L80018B44;
        }
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x2D24u;
        c.V1 = c.T0 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x800189F4u: goto L800189F4;
            case 0x80018A3Cu: goto L80018A3C;
            case 0x80018A94u: goto L80018A94;
            case 0x80018AD8u: goto L80018AD8;
            case 0x80018AF4u: goto L80018AF4;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L800189F4: ;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4224u;
        c.A1 = c.S2 + 0u;
        c.RA = 0x80018A04u;
        GranTurismo2PC.func_8006CFC4(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = 0xFFFFFFFEu;
        if (c.V1 == c.V0) {
            c.V0 = (int)c.V1 < -1 ? 1u : 0u;
            goto L80018B44;
        }
        c.V0 = (int)c.V1 < -1 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0xFFFFFFFDu;
            goto L80018A24;
        }
        c.V0 = 0xFFFFFFFDu;
        if (c.V1 == c.V0) {
            goto L80018A6C;
        }
        L80018A24: ;
        c.A0 = 0x00000001u;
        c.RA = 0x80018A2Cu;
        GranTurismo2PC.func_80060840(c, m);
        c.V0 = 0x80050000u;
        c.V1 = 0xFFFFFFFFu;
        MemoryAccess.WriteU16(m, (c.V0 - 0x421Eu), (ushort)c.V1);
        goto L80018B38;
        L80018A3C: ;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x40ECu;
        c.A1 = c.S2 + 0u;
        c.RA = 0x80018A4Cu;
        GranTurismo2PC.func_8006CFC4(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = 0xFFFFFFFEu;
        if (c.V1 == c.V0) {
            c.V0 = (int)c.V1 < -1 ? 1u : 0u;
            goto L80018B44;
        }
        c.V0 = (int)c.V1 < -1 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0xFFFFFFFDu;
            goto L80018A7C;
        }
        c.V0 = 0xFFFFFFFDu;
        if (c.V1 != c.V0) {
            goto L80018A7C;
        }
        L80018A6C: ;
        c.A0 = 0x00000006u;
        c.RA = 0x80018A74u;
        GranTurismo2PC.func_80060840(c, m);
        c.A0 = 0x80050000u;
        goto L80018B48;
        L80018A7C: ;
        c.A0 = 0x00000001u;
        c.RA = 0x80018A84u;
        GranTurismo2PC.func_80060840(c, m);
        c.V0 = 0x80050000u;
        c.V1 = 0xFFFFFFFFu;
        MemoryAccess.WriteU16(m, (c.V0 - 0x40E6u), (ushort)c.V1);
        goto L80018B38;
        L80018A94: ;
        c.V0 = 0x800B0000u;
        c.S1 = c.V0 + 0x12D0u;
        c.A0 = c.S1 + 0u;
        c.A1 = c.S2 + 0u;
        c.RA = 0x80018AA8u;
        GranTurismo2PC.func_8001A860(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = 0xFFFFFFFFu;
        if (c.V1 == c.V0) {
            c.A0 = 0x80050000u;
            goto L80018B48;
        }
        c.A0 = 0x80050000u;
        if (c.V1 != 0u) {
            goto L80018B48;
        }
        c.A0 = 0x00000001u;
        c.RA = 0x80018AC8u;
        GranTurismo2PC.func_80060840(c, m);
        c.A0 = c.S1 + 0u;
        c.RA = 0x80018AD0u;
        GranTurismo2PC.func_8001A84C(c, m);
        c.V0 = 0x00000001u;
        goto L80018B3C;
        L80018AD8: ;
        c.V0 = 0x800B0000u;
        c.S1 = c.V0 + 0x12D8u;
        goto L80018AFC;
        L80018AE4: ;
        if (c.V1 == c.V0) {
            c.A0 = 0x80050000u;
            goto L80018B30;
        }
        c.A0 = 0x80050000u;
        goto L80018B48;
        L80018AF4: ;
        c.V0 = 0x800B0000u;
        c.S1 = c.V0 + 0x1358u;
        L80018AFC: ;
        c.A0 = c.S1 + 0u;
        c.A1 = c.S2 + 0u;
        c.RA = 0x80018B08u;
        GranTurismo2PC.func_8001C7B8(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = 0xFFFFFFFFu;
        if (c.V1 == c.V0) {
            c.A0 = 0x80050000u;
            goto L80018B48;
        }
        c.A0 = 0x80050000u;
        if ((int)c.V1 < 0) {
            c.V0 = 0xFFFFFFFEu;
            goto L80018AE4;
        }
        c.V0 = 0xFFFFFFFEu;
        if (c.V1 != 0u) {
            goto L80018B48;
        }
        c.A0 = 0x00000001u;
        c.RA = 0x80018B30u;
        GranTurismo2PC.func_80060840(c, m);
        L80018B30: ;
        c.A0 = c.S1 + 0u;
        c.RA = 0x80018B38u;
        GranTurismo2PC.func_8001C684(c, m);
        L80018B38: ;
        c.V0 = 0x00000001u;
        L80018B3C: ;
        MemoryAccess.WriteU8(m, (c.S0 + 0x10u), (byte)0u);
        MemoryAccess.WriteU8(m, (c.S0 + 0x11u), (byte)c.V0);
        L80018B44: ;
        c.A0 = 0x80050000u;
        L80018B48: ;
        c.A0 = c.A0 - 0x41CCu;
        c.RA = 0x80018B50u;
        GranTurismo2PC.func_8006BE64(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x41B0u;
        c.RA = 0x80018B5Cu;
        GranTurismo2PC.func_8006BE64(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4194u;
        c.RA = 0x80018B68u;
        GranTurismo2PC.func_8006BE64(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4094u;
        c.RA = 0x80018B74u;
        GranTurismo2PC.func_8006BE64(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4078u;
        c.RA = 0x80018B80u;
        GranTurismo2PC.func_8006BE64(c, m);
        goto L80018EC8;
        L80018B88: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x10u));
        if (c.V0 == 0u) {
            c.A3 = 0x00000080u;
            goto L80018B9C;
        }
        c.A3 = 0x00000080u;
        c.A3 = 0x00000200u;
        L80018B9C: ;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4334u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x4320u;
        c.A2 = c.S5 + 0u;
        c.RA = 0x80018BB4u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S1 + 0u;
        c.A2 = c.S4 + 0u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.V0);
        c.V0 = 0x80050000u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x68u));
        c.V0 = c.V0 - 0x405Cu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), 0u);
        c.S0 = c.T0 << 2;
        c.V0 = c.S0 + c.V0;
        c.A1 = MemoryAccess.ReadU32(m, c.V0);
        c.A3 = c.FP + 0x14u;
        c.RA = 0x80018BE4u;
        GranTurismo2PC.func_8006ADB4(c, m);
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x68u));
        c.V0 = c.T0 < 0x00000005u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80020000u;
            goto L80018EC8;
        }
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x2D3Cu;
        c.V0 = c.S0 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V0);
        switch (c.V0)
        {
            case 0x80018C10u: goto L80018C10;
            case 0x80018D64u: goto L80018D64;
            case 0x80018E78u: goto L80018E78;
            case 0x80018E9Cu: goto L80018E9C;
            case 0x80018EACu: goto L80018EAC;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L80018C10: ;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4224u;
        c.A1 = c.S7 + 0u;
        c.V0 = c.FP + 0x2Eu;
        MemoryAccess.WriteU16(m, (c.A0 + 0x12u), (ushort)c.V0);
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU16(m, (c.A0 + 0x10u), (ushort)c.S4);
        MemoryAccess.WriteU32(m, (c.V0 - 0x41F0u), c.S5);
        c.RA = 0x80018C34u;
        GranTurismo2PC.func_8006D50C(c, m);
        c.A0 = c.SP + 0x18u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x69E0u;
        c.RA = 0x80018C44u;
        GranTurismo2PC.func_8007DA80(c, m);
        c.S3 = 0x80050000u;
        c.S3 = c.S3 - 0x4334u;
        c.A0 = c.S3 + 0u;
        c.S2 = 0x80050000u;
        c.S2 = c.S2 - 0x4314u;
        c.A1 = c.S2 + 0u;
        c.A2 = c.S5 + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x80018C68u;
        GranTurismo2PC.func_8006B548(c, m);
        c.S1 = c.SP + 0x18u;
        c.A0 = c.S1 + 0u;
        c.S0 = 0x801C0000u;
        c.S0 = c.S0 - 0x6541u;
        c.A1 = c.S0 + 0u;
        c.S6 = c.S4 - 0x8Eu;
        c.A2 = c.S6 + 0u;
        c.A3 = c.FP + 0x3Au;
        MemoryAccess.WriteU32(m, (c.S1 + 0x14u), c.V0);
        c.T0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        c.RA = 0x80018C98u;
        GranTurismo2PC.func_8006AC90(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x41CCu;
        c.A1 = c.S7 + 0u;
        c.S4 = c.S4 - 0x94u;
        c.A2 = c.S4 + 0u;
        c.A3 = c.FP + 0x21u;
        c.RA = 0x80018CB4u;
        GranTurismo2PC.func_8006BEF4(c, m);
        c.A0 = c.S7 + 0u;
        c.A1 = 0x00000220u;
        c.RA = 0x80018CC0u;
        GranTurismo2PC.func_8007DA44(c, m);
        c.A0 = c.S3 + 0u;
        c.A1 = c.S2 + 0u;
        c.A2 = c.S5 + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x80018CD4u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S0 + 0x12u;
        c.A2 = c.S6 + 0u;
        c.A3 = c.FP + 0xD2u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x14u), c.V0);
        c.T0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        c.RA = 0x80018CF4u;
        GranTurismo2PC.func_8006AC90(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x41B0u;
        c.A1 = c.S7 + 0u;
        c.A2 = c.S4 + 0u;
        c.A3 = c.FP + 0xB9u;
        c.RA = 0x80018D0Cu;
        GranTurismo2PC.func_8006BEF4(c, m);
        c.A0 = c.S7 + 0u;
        c.A1 = 0x00000220u;
        c.RA = 0x80018D18u;
        GranTurismo2PC.func_8007DA44(c, m);
        c.A0 = c.S3 + 0u;
        c.A1 = c.S2 + 0u;
        c.A2 = c.S5 + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x80018D2Cu;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S0 + 0x19u;
        c.A2 = c.S6 + 0u;
        c.A3 = c.FP + 0x122u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.V0);
        c.T0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        c.RA = 0x80018D4Cu;
        GranTurismo2PC.func_8006AC90(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4194u;
        c.A1 = c.S7 + 0u;
        c.A2 = c.S4 + 0u;
        c.A3 = c.FP + 0x109u;
        goto L80018E5C;
        L80018D64: ;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x40ECu;
        c.A1 = c.S7 + 0u;
        c.V0 = c.FP + 0x2Eu;
        MemoryAccess.WriteU16(m, (c.A0 + 0x12u), (ushort)c.V0);
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU16(m, (c.A0 + 0x10u), (ushort)c.S4);
        MemoryAccess.WriteU32(m, (c.V0 - 0x40B8u), c.S5);
        c.RA = 0x80018D88u;
        GranTurismo2PC.func_8006D50C(c, m);
        c.A0 = c.SP + 0x18u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x69E0u;
        c.RA = 0x80018D98u;
        GranTurismo2PC.func_8007DA80(c, m);
        c.S2 = 0x80050000u;
        c.S2 = c.S2 - 0x4334u;
        c.A0 = c.S2 + 0u;
        c.S1 = 0x80050000u;
        c.S1 = c.S1 - 0x4314u;
        c.A1 = c.S1 + 0u;
        c.A2 = c.S5 + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x80018DBCu;
        GranTurismo2PC.func_8006B548(c, m);
        c.S3 = c.SP + 0x18u;
        c.A0 = c.S3 + 0u;
        c.S0 = 0x801C0000u;
        c.S0 = c.S0 - 0x64FAu;
        c.A1 = c.S0 + 0u;
        c.S6 = c.S4 - 0x8Eu;
        c.A2 = c.S6 + 0u;
        c.A3 = c.FP + 0x3Au;
        MemoryAccess.WriteU32(m, (c.S3 + 0x14u), c.V0);
        c.T0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        c.RA = 0x80018DECu;
        GranTurismo2PC.func_8006AC90(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4094u;
        c.A1 = c.S7 + 0u;
        c.S4 = c.S4 - 0x94u;
        c.A2 = c.S4 + 0u;
        c.A3 = c.FP + 0x21u;
        c.RA = 0x80018E08u;
        GranTurismo2PC.func_8006BEF4(c, m);
        c.A0 = c.S7 + 0u;
        c.A1 = 0x00000220u;
        c.RA = 0x80018E14u;
        GranTurismo2PC.func_8007DA44(c, m);
        c.A0 = c.S2 + 0u;
        c.A1 = c.S1 + 0u;
        c.A2 = c.S5 + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x80018E28u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S3 + 0u;
        c.A1 = c.S0 + 0xFu;
        c.A2 = c.S6 + 0u;
        c.A3 = c.FP + 0x8Au;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.V0);
        c.T0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        c.RA = 0x80018E48u;
        GranTurismo2PC.func_8006AC90(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4078u;
        c.A1 = c.S7 + 0u;
        c.A2 = c.S4 + 0u;
        c.A3 = c.FP + 0x71u;
        L80018E5C: ;
        c.RA = 0x80018E64u;
        GranTurismo2PC.func_8006BEF4(c, m);
        c.A0 = c.S7 + 0u;
        c.A1 = 0x00000220u;
        c.RA = 0x80018E70u;
        GranTurismo2PC.func_8007DA44(c, m);
        goto L80018EC8;
        L80018E78: ;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S5);
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x12D0u;
        c.A1 = c.S7 + 0u;
        c.A2 = c.S4 + 0u;
        c.A3 = c.FP + 0x40u;
        c.RA = 0x80018E94u;
        GranTurismo2PC.func_8001AA84(c, m);
        goto L80018EC8;
        L80018E9C: ;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S5);
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x12D8u;
        goto L80018EB8;
        L80018EAC: ;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S5);
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x1358u;
        L80018EB8: ;
        c.A1 = c.S7 + 0u;
        c.A2 = c.S4 + 0u;
        c.A3 = c.FP + 0x2Eu;
        c.RA = 0x80018EC8u;
        GranTurismo2PC.func_8001CE28(c, m);
        L80018EC8: ;
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
    public static void func_80018EFC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0x80050000u;
        c.S0 = c.S0 - 0x4048u;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x80020000u;
        c.A1 = c.A1 - 0x791Cu;
        c.A2 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.RA = 0x80018F2Cu;
        GranTurismo2PC.func_8001D4B0(c, m);
        c.A0 = c.S0 + 0u;
        c.RA = 0x80018F34u;
        GranTurismo2PC.func_8001D4D4(c, m);
        c.S1 = 0x80050000u;
        c.S1 = c.S1 - 0x4224u;
        c.A0 = c.S1 + 0u;
        c.S0 = 0x80020000u;
        c.S0 = c.S0 - 0x7A8Cu;
        c.A1 = c.S0 + 0u;
        c.A2 = 0x80050000u;
        c.A2 = c.A2 - 0x42C4u;
        c.RA = 0x80018F58u;
        GranTurismo2PC.func_8006CDCC(c, m);
        c.A0 = c.S1 + 0u;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 - 0x41F0u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x30u), c.V0);
        c.RA = 0x80018F6Cu;
        GranTurismo2PC.func_8006CE70(c, m);
        c.S2 = 0x80050000u;
        c.S2 = c.S2 - 0x40ECu;
        c.A0 = c.S2 + 0u;
        c.A1 = c.S0 + 0u;
        c.A2 = 0x80050000u;
        c.A2 = c.A2 - 0x4178u;
        c.S0 = 0xFFFFFFFFu;
        c.T0 = 0x80050000u;
        c.T0 = c.T0 - 0x41CCu;
        c.A3 = c.S0 + 0u;
        c.V1 = 0x80050000u;
        c.V1 = c.V1 - 0x41B0u;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 - 0x4194u;
        MemoryAccess.WriteU16(m, (c.S1 + 0x6u), (ushort)c.S0);
        MemoryAccess.WriteU16(m, (c.T0 + 0x18u), (ushort)c.A3);
        MemoryAccess.WriteU16(m, (c.V1 + 0x18u), (ushort)c.A3);
        MemoryAccess.WriteU16(m, (c.V0 + 0x18u), (ushort)c.A3);
        MemoryAccess.WriteU16(m, (c.T0 + 0x18u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.V1 + 0x18u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.V0 + 0x18u), (ushort)0u);
        c.RA = 0x80018FC4u;
        GranTurismo2PC.func_8006CDCC(c, m);
        c.A0 = c.S2 + 0u;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 - 0x40B8u;
        MemoryAccess.WriteU32(m, (c.S2 + 0x30u), c.V0);
        c.RA = 0x80018FD8u;
        GranTurismo2PC.func_8006CE70(c, m);
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x12D0u;
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU16(m, (c.S2 + 0x6u), (ushort)c.S0);
        MemoryAccess.WriteU16(m, (c.V0 - 0x407Cu), (ushort)c.S0);
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU16(m, (c.V0 - 0x4060u), (ushort)c.S0);
        c.RA = 0x80018FF8u;
        GranTurismo2PC.func_8001A7E0(c, m);
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x12D8u;
        c.A1 = 0u + 0u;
        c.RA = 0x80019008u;
        GranTurismo2PC.func_8001C48C(c, m);
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x1358u;
        c.A1 = 0x00000001u;
        c.RA = 0x80019018u;
        GranTurismo2PC.func_8001C48C(c, m);
        c.A0 = 0x00000006u;
        c.RA = 0x80019020u;
        GranTurismo2PC.func_80012414(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019038(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = 0u + 0u;
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.V0 - 0x4048u;
        c.A0 = c.S0 + 0u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.A3 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x9u));
        c.A2 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
        c.A3 = c.A3 ^ 0x0004u;
        c.A1 = c.A2 + 0x1A4u;
        c.A2 = c.A2 + 0x1B4u;
        c.A3 = 0u < c.A3 ? 1u : 0u;
        c.RA = 0x80019078u;
        GranTurismo2PC.func_8001D4F4(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = 0xFFFFFFFEu;
        if (c.V1 == c.V0) {
            c.V0 = (int)c.V1 < -1 ? 1u : 0u;
            goto L800190D8;
        }
        c.V0 = (int)c.V1 < -1 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0xFFFFFFFDu;
            goto L800190A0;
        }
        c.V0 = 0xFFFFFFFDu;
        if (c.V1 == c.V0) {
            c.V0 = c.S1 + 0u;
            goto L800190B4;
        }
        c.V0 = c.S1 + 0u;
        goto L800190DC;
        L800190A0: ;
        c.V0 = 0xFFFFFFFFu;
        if (c.V1 == c.V0) {
            c.V0 = c.S1 + 0u;
            goto L800190C4;
        }
        c.V0 = c.S1 + 0u;
        goto L800190DC;
        L800190B4: ;
        c.A0 = 0x00000007u;
        c.RA = 0x800190BCu;
        GranTurismo2PC.func_80060840(c, m);
        c.V0 = c.S1 + 0u;
        goto L800190DC;
        L800190C4: ;
        c.A0 = c.S0 + 0u;
        c.RA = 0x800190CCu;
        Dispatcher.Call(c, m, 0x8001D4E8u);
        c.A0 = 0x00000004u;
        c.RA = 0x800190D4u;
        GranTurismo2PC.func_80060840(c, m);
        c.S1 = 0x00000002u;
        L800190D8: ;
        c.V0 = c.S1 + 0u;
        L800190DC: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800190F0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x4048u;
        c.RA = 0x80019108u;
        GranTurismo2PC.func_8001D7C0(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019118(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1C4u));
        c.V0 = 0x00000010u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x18u), c.A0);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019138(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V1 = MemoryAccess.ReadU32(m, (c.A0 + 0x1C4u));
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x14u));
        if ((int)c.V0 <= 0) {
            c.A1 = 0u + 0u;
            goto L8001918C;
        }
        c.A1 = 0u + 0u;
        c.V0 = c.V0 - 0x1u;
        if (c.V0 != 0u) {
            MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
            goto L8001918C;
        }
        MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 + 0x18u));
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            c.A1 = 0x00000004u;
            goto L8001918C;
        }
        c.A1 = 0x00000004u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x3F84u;
        c.RA = 0x80019188u;
        GranTurismo2PC.func_800122A0(c, m);
        c.A1 = 0x00000001u;
        L8001918C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = c.A1 + 0u;
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001919C(CpuContext c, IMemory m)
    {
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800191A4(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1C4u));
        c.V0 = 0x00000010u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
        c.RA = 0x800191C4u;
        GranTurismo2PC.func_80012434(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800191D4(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1C4u));
        c.V1 = MemoryAccess.ReadU32(m, (c.A0 + 0x14u));
        if ((int)c.V1 <= 0) {
            c.V0 = 0u + 0u;
            goto L80019208;
        }
        c.V0 = 0u + 0u;
        c.V0 = c.V1 - 0x1u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.V0);
        c.V0 = c.V0 < 0x00000001u ? 1u : 0u;
        c.V0 = c.V0 << 2;
        L80019208: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019210_gt2_overlay_1(CpuContext c, IMemory m)
    {
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019218(CpuContext c, IMemory m)
    {
        c.V0 = 0x00000009u;
        if (c.A0 != c.V0) {
            c.V0 = 0x0000000Au;
            goto L8001922C;
        }
        c.V0 = 0x0000000Au;
        c.V0 = 0x00000081u;
        return;
        L8001922C: ;
        if (c.A0 == c.V0) {
            c.V1 = 0x00000004u;
            goto L80019244;
        }
        c.V1 = 0x00000004u;
        if (c.A0 == c.V1) {
            c.V0 = 0x00000083u;
            goto L80019248;
        }
        c.V0 = 0x00000083u;
        c.V0 = c.A0 + 0u;
        return;
        L80019244: ;
        c.V0 = 0x00000082u;
        L80019248: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019250(CpuContext c, IMemory m)
    {
        c.V0 = 0x00000081u;
        if (c.A0 != c.V0) {
            c.V0 = 0x00000082u;
            goto L80019264;
        }
        c.V0 = 0x00000082u;
        c.V0 = 0x00000009u;
        return;
        L80019264: ;
        if (c.A0 == c.V0) {
            c.V1 = 0x00000083u;
            goto L8001927C;
        }
        c.V1 = 0x00000083u;
        if (c.A0 == c.V1) {
            c.V0 = 0x00000004u;
            goto L80019280;
        }
        c.V0 = 0x00000004u;
        c.V0 = c.A0 + 0u;
        return;
        L8001927C: ;
        c.V0 = 0x0000000Au;
        L80019280: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019288(CpuContext c, IMemory m)
    {
        c.V0 = 0x000000C3u;
        if (c.A0 != c.V0) {
            c.V0 = 0x000000A3u;
            goto L8001929C;
        }
        c.V0 = 0x000000A3u;
        c.V0 = 0x00000003u;
        return;
        L8001929C: ;
        if (c.A0 != c.V0) {
            c.V0 = 0x000000C2u;
            goto L800192AC;
        }
        c.V0 = 0x000000C2u;
        c.V0 = 0x00000002u;
        return;
        L800192AC: ;
        if (c.A0 != c.V0) {
            c.V0 = 0x000000A2u;
            goto L800192BC;
        }
        c.V0 = 0x000000A2u;
        c.V0 = 0x00000001u;
        return;
        L800192BC: ;
        if (c.A0 != c.V0) {
            c.V0 = 0x000000C1u;
            goto L800192CC;
        }
        c.V0 = 0x000000C1u;
        c.V0 = 0u + 0u;
        return;
        L800192CC: ;
        if (c.A0 != c.V0) {
            c.V0 = 0x000000A1u;
            goto L800192DC;
        }
        c.V0 = 0x000000A1u;
        c.V0 = 0x00000007u;
        return;
        L800192DC: ;
        if (c.A0 != c.V0) {
            c.V0 = 0x000000C0u;
            goto L800192EC;
        }
        c.V0 = 0x000000C0u;
        c.V0 = 0x00000006u;
        return;
        L800192EC: ;
        if (c.A0 == c.V0) {
            c.V0 = c.A0 ^ 0x00A0u;
            goto L80019300;
        }
        c.V0 = c.A0 ^ 0x00A0u;
        c.V0 = c.V0 < 0x00000001u ? 1u : 0u;
        c.V0 = c.V0 << 2;
        return;
        L80019300: ;
        c.V0 = 0x00000005u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019308(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.RA = 0x80019318u;
        GranTurismo2PC.func_80019250(c, m);
        c.A0 = c.V0 + 0u;
        c.V1 = c.A0 - 0x9u;
        c.V1 = c.V1 < 0x00000002u ? 1u : 0u;
        c.V0 = c.A0 ^ 0x0004u;
        c.V0 = c.V0 < 0x00000001u ? 1u : 0u;
        c.V1 = c.V1 | c.V0;
        if (c.V1 != 0u) {
            c.V1 = 0x80050000u;
            goto L80019340;
        }
        c.V1 = 0x80050000u;
        c.V0 = 0u + 0u;
        goto L8001936C;
        L80019340: ;
        c.V1 = c.V1 - 0x3D04u;
        c.V0 = c.A0 << 1;
        c.V0 = c.V0 + c.V1;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.V0);
        c.V1 = c.V0 << 1;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 2;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 - 0x3E2Cu;
        c.V0 = c.V1 + c.V0;
        L8001936C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001937C(CpuContext c, IMemory m)
    {
        c.V0 = 0x00000003u;
        MemoryAccess.WriteU32(m, c.A0, c.V0);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019388(CpuContext c, IMemory m)
    {
        c.T0 = 0x00000003u;
        c.A3 = 0u + 0u;
        c.V1 = 0x801D0000u;
        c.V1 = c.V1 - 0x6720u;
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
            goto L800193FC;
        }
        c.V1 = c.A1 + 0u;
        c.V0 = (int)c.A1 < 5 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L800193E0;
        }
        c.V0 = 0x00000002u;
        if (c.A1 == c.V0) {
            goto L80019404;
        }
        MemoryAccess.WriteU32(m, c.A0, c.T0);
        goto L80019418;
        L800193E0: ;
        c.V0 = 0x00000005u;
        if (c.V1 == c.V0) {
            c.V0 = 0x00000007u;
            goto L8001940C;
        }
        c.V0 = 0x00000007u;
        if (c.V1 == c.V0) {
            goto L8001940C;
        }
        MemoryAccess.WriteU32(m, c.A0, c.T0);
        goto L80019418;
        L800193FC: ;
        c.T0 = 0u + 0u;
        goto L80019414;
        L80019404: ;
        c.T0 = 0x00000002u;
        goto L80019410;
        L8001940C: ;
        c.T0 = 0x00000001u;
        L80019410: ;
        c.A3 = c.T0 + 0u;
        L80019414: ;
        MemoryAccess.WriteU32(m, c.A0, c.T0);
        L80019418: ;
        c.V0 = 0x00000003u;
        if (c.T0 == c.V0) {
            c.V0 = c.A3 << 1;
            goto L80019490;
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
        L80019490: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019498(CpuContext c, IMemory m)
    {
        c.A3 = 0x00000003u;
        c.A2 = 0u + 0u;
        c.V1 = 0x801D0000u;
        c.V1 = c.V1 - 0x6720u;
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
            goto L80019510;
        }
        c.V1 = c.A1 + 0u;
        c.V0 = (int)c.A1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L800194F0;
        }
        c.V0 = 0x00000002u;
        if (c.A1 == 0u) {
            c.V0 = 0x00000003u;
            goto L80019500;
        }
        c.V0 = 0x00000003u;
        goto L8001951C;
        L800194F0: ;
        if (c.V1 == c.V0) {
            c.V0 = 0x00000003u;
            goto L80019508;
        }
        c.V0 = 0x00000003u;
        goto L8001951C;
        L80019500: ;
        c.A3 = 0u + 0u;
        goto L80019518;
        L80019508: ;
        c.A3 = 0x00000002u;
        goto L80019514;
        L80019510: ;
        c.A3 = 0x00000001u;
        L80019514: ;
        c.A2 = c.A3 + 0u;
        L80019518: ;
        c.V0 = 0x00000003u;
        L8001951C: ;
        if (c.A3 == c.V0) {
            c.V0 = c.A2 << 1;
            goto L80019590;
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
        L80019590: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019598(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        c.A0 = c.A1 + 0u;
        c.V1 = 0x00000004u;
        c.A1 = c.S0 + 0x8u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        L800195B4: ;
        c.V0 = (int)c.V1 < 10 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.A1 + c.V1;
            goto L800195D8;
        }
        c.V0 = c.A1 + c.V1;
        c.V0 = MemoryAccess.ReadU8(m, c.V0);
        if (c.V0 == c.A0) {
            c.V0 = c.V1 + 0u;
            goto L800196D8;
        }
        c.V0 = c.V1 + 0u;
        c.V1 = c.V1 + 0x1u;
        goto L800195B4;
        L800195D8: ;
        c.V1 = MemoryAccess.ReadU32(m, c.S0);
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L80019678;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L80019600;
        }
        c.V0 = 0x00000002u;
        if (c.V1 == 0u) {
            c.V0 = 0xFFFFFFFFu;
            goto L80019610;
        }
        c.V0 = 0xFFFFFFFFu;
        goto L800196D8;
        L80019600: ;
        if (c.V1 == c.V0) {
            c.V0 = 0xFFFFFFFFu;
            goto L80019628;
        }
        c.V0 = 0xFFFFFFFFu;
        goto L800196D8;
        L80019610: ;
        c.V0 = MemoryAccess.ReadU8(m, (c.S0 + 0xAu));
        if (c.V0 == c.A0) {
            c.V0 = 0x00000002u;
            goto L800196D8;
        }
        c.V0 = 0x00000002u;
        goto L80019660;
        L80019628: ;
        c.V0 = MemoryAccess.ReadU8(m, (c.S0 + 0xAu));
        if (c.V0 == c.A0) {
            c.V0 = 0x00000002u;
            goto L800196D8;
        }
        c.V0 = 0x00000002u;
        c.V0 = MemoryAccess.ReadU8(m, (c.S0 + 0xBu));
        if (c.V0 == c.A0) {
            goto L80019670;
        }
        c.RA = 0x80019650u;
        GranTurismo2PC.func_80019218(c, m);
        c.V1 = MemoryAccess.ReadU8(m, (c.S0 + 0xAu));
        c.A0 = c.V0 + 0u;
        if (c.V1 == c.A0) {
            c.V0 = 0x00000002u;
            goto L800196D8;
        }
        c.V0 = 0x00000002u;
        L80019660: ;
        c.V0 = MemoryAccess.ReadU8(m, (c.S0 + 0xBu));
        if (c.V0 != c.A0) {
            c.V0 = 0xFFFFFFFFu;
            goto L800196D8;
        }
        c.V0 = 0xFFFFFFFFu;
        L80019670: ;
        c.V0 = 0x00000003u;
        goto L800196D8;
        L80019678: ;
        c.V1 = MemoryAccess.ReadU8(m, (c.S0 + 0xAu));
        c.V0 = c.V1 & 0x00FFu;
        if (c.V0 == c.A0) {
            c.V0 = 0x00000002u;
            goto L800196D8;
        }
        c.V0 = 0x00000002u;
        c.V0 = MemoryAccess.ReadU8(m, (c.S0 + 0xBu));
        if (c.V0 == c.A0) {
            c.V0 = c.V1 << 24;
            goto L80019670;
        }
        c.V0 = c.V1 << 24;
        if ((int)c.V0 >= 0) {
            goto L800196B4;
        }
        c.V0 = MemoryAccess.ReadU8(m, (c.S0 + 0x6u));
        if (c.V0 == c.A0) {
            c.V0 = 0x00000002u;
            goto L800196D8;
        }
        c.V0 = 0x00000002u;
        L800196B4: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0xBu));
        if ((int)c.V0 >= 0) {
            c.V0 = 0xFFFFFFFFu;
            goto L800196D8;
        }
        c.V0 = 0xFFFFFFFFu;
        c.V1 = MemoryAccess.ReadU8(m, (c.S0 + 0x7u));
        if (c.V1 == c.A0) {
            c.V0 = 0x00000003u;
            goto L800196D8;
        }
        c.V0 = 0x00000003u;
        c.V0 = 0xFFFFFFFFu;
        L800196D8: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800196E8(CpuContext c, IMemory m)
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
        c.RA = 0x80019714u;
        GranTurismo2PC.func_80019598(c, m);
        c.A1 = c.V0 + 0u;
        c.V1 = MemoryAccess.ReadU32(m, c.S1);
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L800197E8;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L80019740;
        }
        if (c.V1 == 0u) {
            goto L80019754;
        }
        goto L80019918;
        L80019740: ;
        c.V0 = 0x00000002u;
        if (c.V1 == c.V0) {
            goto L80019770;
        }
        goto L80019918;
        L80019754: ;
        if ((int)c.A1 < 0) {
            c.V0 = c.S1 + 0x8u;
            goto L80019910;
        }
        c.V0 = c.S1 + 0x8u;
        c.V1 = c.V0 + c.S2;
        c.V1 = MemoryAccess.ReadU8(m, c.V1);
        c.V0 = c.V0 + c.A1;
        MemoryAccess.WriteU8(m, c.V0, (byte)c.V1);
        goto L80019910;
        L80019770: ;
        if ((int)c.A1 < 0) {
            c.V0 = (int)c.A1 < 4 ? 1u : 0u;
            goto L800197BC;
        }
        c.V0 = (int)c.A1 < 4 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = (int)c.A1 < 2 ? 1u : 0u;
            goto L800197A0;
        }
        c.V0 = (int)c.A1 < 2 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S0 = c.S1 + 0x8u;
            goto L800197A0;
        }
        c.S0 = c.S1 + 0x8u;
        c.V0 = c.S0 + c.S2;
        c.A0 = MemoryAccess.ReadU8(m, c.V0);
        c.S0 = c.S0 + c.A1;
        c.RA = 0x80019798u;
        GranTurismo2PC.func_80019218(c, m);
        MemoryAccess.WriteU8(m, c.S0, (byte)c.V0);
        goto L800197BC;
        L800197A0: ;
        if ((int)c.A1 < 0) {
            c.S0 = c.S1 + 0x8u;
            goto L800197BC;
        }
        c.S0 = c.S1 + 0x8u;
        c.V0 = c.S0 + c.S2;
        c.A0 = MemoryAccess.ReadU8(m, c.V0);
        c.S0 = c.S0 + c.A1;
        c.RA = 0x800197B8u;
        GranTurismo2PC.func_80019250(c, m);
        MemoryAccess.WriteU8(m, c.S0, (byte)c.V0);
        L800197BC: ;
        c.V0 = (int)c.S2 < 4 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = (int)c.S2 < 2 ? 1u : 0u;
            goto L80019910;
        }
        c.V0 = (int)c.S2 < 2 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = c.S1 + c.S2;
            goto L80019914;
        }
        c.V0 = c.S1 + c.S2;
        c.A0 = c.S3 + 0u;
        c.S0 = c.S1 + 0x8u;
        c.S0 = c.S0 + c.S2;
        c.RA = 0x800197E0u;
        GranTurismo2PC.func_80019218(c, m);
        MemoryAccess.WriteU8(m, c.S0, (byte)c.V0);
        goto L80019918;
        L800197E8: ;
        if ((int)c.A1 < 0) {
            c.V0 = c.S1 + c.S2;
            goto L800198A4;
        }
        c.V0 = c.S1 + c.S2;
        c.V1 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        c.V0 = 0x00000002u;
        if (c.S2 == c.V0) {
            c.V0 = 0x00000003u;
            goto L80019810;
        }
        c.V0 = 0x00000003u;
        if (c.S2 == c.V0) {
            c.V0 = 0x00000002u;
            goto L8001982C;
        }
        c.V0 = 0x00000002u;
        goto L80019840;
        L80019810: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0xAu));
        if ((int)c.V0 >= 0) {
            c.V0 = 0x00000002u;
            goto L80019840;
        }
        c.V0 = 0x00000002u;
        c.V1 = MemoryAccess.ReadU8(m, (c.S1 + 0x6u));
        goto L80019840;
        L8001982C: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0xBu));
        if ((int)c.V0 >= 0) {
            c.V0 = 0x00000002u;
            goto L80019840;
        }
        c.V0 = 0x00000002u;
        c.V1 = MemoryAccess.ReadU8(m, (c.S1 + 0x7u));
        L80019840: ;
        if (c.A1 == c.V0) {
            c.V0 = 0x00000003u;
            goto L80019858;
        }
        c.V0 = 0x00000003u;
        if (c.A1 == c.V0) {
            goto L80019878;
        }
        goto L80019898;
        L80019858: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0xAu));
        if ((int)c.V0 >= 0) {
            goto L80019870;
        }
        MemoryAccess.WriteU8(m, (c.S1 + 0x6u), (byte)c.V1);
        goto L800198A4;
        L80019870: ;
        MemoryAccess.WriteU8(m, (c.S1 + 0xAu), (byte)c.V1);
        goto L800198A4;
        L80019878: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0xBu));
        if ((int)c.V0 >= 0) {
            goto L80019890;
        }
        MemoryAccess.WriteU8(m, (c.S1 + 0x7u), (byte)c.V1);
        goto L800198A4;
        L80019890: ;
        MemoryAccess.WriteU8(m, (c.S1 + 0xBu), (byte)c.V1);
        goto L800198A4;
        L80019898: ;
        if ((int)c.A1 < 0) {
            c.V0 = c.S1 + c.A1;
            goto L800198A4;
        }
        c.V0 = c.S1 + c.A1;
        MemoryAccess.WriteU8(m, (c.V0 + 0x8u), (byte)c.V1);
        L800198A4: ;
        c.V0 = 0x00000002u;
        if (c.S2 == c.V0) {
            c.V0 = 0x00000003u;
            goto L800198C0;
        }
        c.V0 = 0x00000003u;
        if (c.S2 == c.V0) {
            c.V0 = c.S1 + c.S2;
            goto L800198E8;
        }
        c.V0 = c.S1 + c.S2;
        MemoryAccess.WriteU8(m, (c.V0 + 0x8u), (byte)c.S3);
        goto L80019918;
        L800198C0: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0xBu));
        if ((int)c.V0 >= 0) {
            c.V1 = c.S1 + 0x4u;
            goto L800198E0;
        }
        c.V1 = c.S1 + 0x4u;
        c.V0 = MemoryAccess.ReadU8(m, (c.V1 + 0x3u));
        MemoryAccess.WriteU8(m, (c.S1 + 0xBu), (byte)c.V0);
        MemoryAccess.WriteU8(m, (c.V1 + 0x1u), (byte)0u);
        L800198E0: ;
        MemoryAccess.WriteU8(m, (c.S1 + 0xAu), (byte)c.S3);
        goto L80019918;
        L800198E8: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0xAu));
        if ((int)c.V0 >= 0) {
            c.V1 = c.S1 + 0x4u;
            goto L80019908;
        }
        c.V1 = c.S1 + 0x4u;
        c.V0 = MemoryAccess.ReadU8(m, (c.V1 + 0x2u));
        MemoryAccess.WriteU8(m, (c.S1 + 0xAu), (byte)c.V0);
        MemoryAccess.WriteU8(m, (c.V1 + 0x1u), (byte)0u);
        L80019908: ;
        MemoryAccess.WriteU8(m, (c.S1 + 0xBu), (byte)c.S3);
        goto L80019918;
        L80019910: ;
        c.V0 = c.S1 + c.S2;
        L80019914: ;
        MemoryAccess.WriteU8(m, (c.V0 + 0x8u), (byte)c.S3);
        L80019918: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019934(CpuContext c, IMemory m)
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
            goto L80019980;
        }
        c.S2 = 0x00000001u;
        c.V0 = 0xFFFE0000u;
        c.V0 = c.V0 | 0xFFFFu;
        c.V0 = c.A0 & c.V0;
        if (c.V0 == 0u) {
            c.V0 = 0u + 0u;
            goto L80019B0C;
        }
        c.V0 = 0u + 0u;
        goto L80019A60;
        L80019980: ;
        c.V1 = MemoryAccess.ReadU32(m, c.S1);
        if (c.V1 == c.S2) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L80019A50;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L800199A8;
        }
        c.V0 = 0x00000002u;
        if (c.V1 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L800199B8;
        }
        c.V0 = c.S2 + 0u;
        goto L80019B0C;
        L800199A8: ;
        if (c.V1 == c.V0) {
            c.V0 = c.S2 + 0u;
            goto L800199D8;
        }
        c.V0 = c.S2 + 0u;
        goto L80019B0C;
        L800199B8: ;
        c.V0 = 0xFFFFFFFCu;
        c.V0 = c.A0 & c.V0;
        if (c.V0 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L80019B0C;
        }
        c.V0 = c.S2 + 0u;
        c.A0 = 0u + 0u;
        c.RA = 0x800199D0u;
        GranTurismo2PC.func_80060840(c, m);
        c.S2 = 0u + 0u;
        goto L80019B08;
        L800199D8: ;
        c.V0 = 0xFFFFFFF0u;
        c.V0 = c.A0 & c.V0;
        if (c.V0 != 0u) {
            c.V0 = c.A0 & 0x0004u;
            goto L80019A60;
        }
        c.V0 = c.A0 & 0x0004u;
        c.S0 = MemoryAccess.ReadU8(m, (c.S1 + 0x4u));
        if (c.V0 == 0u) {
            c.V0 = c.A0 & 0x0008u;
            goto L80019A04;
        }
        c.V0 = c.A0 & 0x0008u;
        c.S0 = c.S0 - 0x1u;
        if ((int)c.S0 >= 0) {
            goto L80019A04;
        }
        c.S0 = 0x00000001u;
        L80019A04: ;
        if (c.V0 == 0u) {
            goto L80019A20;
        }
        c.S0 = c.S0 + 0x1u;
        c.V0 = (int)c.S0 < 2 ? 1u : 0u;
        if (c.V0 != 0u) {
            goto L80019A20;
        }
        c.S0 = 0u + 0u;
        L80019A20: ;
        c.V0 = MemoryAccess.ReadU8(m, (c.S1 + 0x4u));
        if (c.S0 == c.V0) {
            c.V0 = c.S2 + 0u;
            goto L80019B0C;
        }
        c.V0 = c.S2 + 0u;
        c.S2 = 0u + 0u;
        c.A0 = 0x00000005u;
        c.RA = 0x80019A3Cu;
        GranTurismo2PC.func_80060840(c, m);
        if (c.S0 != 0u) {
            MemoryAccess.WriteU8(m, (c.S1 + 0x4u), (byte)c.S0);
            goto L80019AF8;
        }
        MemoryAccess.WriteU8(m, (c.S1 + 0x4u), (byte)c.S0);
        c.V0 = 0x00000080u;
        MemoryAccess.WriteU8(m, (c.S1 + 0x8u), (byte)c.V0);
        goto L80019B04;
        L80019A50: ;
        c.V0 = 0xFFFFFFF0u;
        c.V0 = c.A0 & c.V0;
        if (c.V0 == 0u) {
            c.V0 = c.A0 & 0x0004u;
            goto L80019A70;
        }
        c.V0 = c.A0 & 0x0004u;
        L80019A60: ;
        c.A0 = 0u + 0u;
        c.RA = 0x80019A68u;
        GranTurismo2PC.func_80060840(c, m);
        c.V0 = 0u + 0u;
        goto L80019B0C;
        L80019A70: ;
        c.S0 = MemoryAccess.ReadU8(m, (c.S1 + 0x4u));
        if (c.V0 == 0u) {
            c.V0 = c.A0 & 0x0008u;
            goto L80019A8C;
        }
        c.V0 = c.A0 & 0x0008u;
        c.S0 = c.S0 - 0x1u;
        if ((int)c.S0 >= 0) {
            goto L80019A8C;
        }
        c.S0 = 0x00000002u;
        L80019A8C: ;
        if (c.V0 == 0u) {
            goto L80019AA8;
        }
        c.S0 = c.S0 + 0x1u;
        c.V0 = (int)c.S0 < 3 ? 1u : 0u;
        if (c.V0 != 0u) {
            goto L80019AA8;
        }
        c.S0 = 0u + 0u;
        L80019AA8: ;
        c.V0 = MemoryAccess.ReadU8(m, (c.S1 + 0x4u));
        if (c.S0 == c.V0) {
            c.V0 = c.S2 + 0u;
            goto L80019B0C;
        }
        c.V0 = c.S2 + 0u;
        c.S2 = 0u + 0u;
        c.A0 = 0x00000005u;
        c.RA = 0x80019AC4u;
        GranTurismo2PC.func_80060840(c, m);
        if (c.S0 == 0u) {
            MemoryAccess.WriteU8(m, (c.S1 + 0x4u), (byte)c.S0);
            goto L80019AE0;
        }
        MemoryAccess.WriteU8(m, (c.S1 + 0x4u), (byte)c.S0);
        c.V0 = 0x00000001u;
        if (c.S0 == c.V0) {
            c.V0 = 0x00000002u;
            goto L80019AEC;
        }
        c.V0 = 0x00000002u;
        MemoryAccess.WriteU8(m, (c.S1 + 0x8u), (byte)c.V0);
        goto L80019B00;
        L80019AE0: ;
        c.V0 = 0x00000082u;
        MemoryAccess.WriteU8(m, (c.S1 + 0x8u), (byte)c.V0);
        goto L80019B04;
        L80019AEC: ;
        c.V0 = 0x00000080u;
        MemoryAccess.WriteU8(m, (c.S1 + 0x8u), (byte)c.V0);
        goto L80019B04;
        L80019AF8: ;
        c.V0 = 0x00000002u;
        MemoryAccess.WriteU8(m, (c.S1 + 0x8u), (byte)c.V0);
        L80019B00: ;
        c.V0 = 0x00000003u;
        L80019B04: ;
        MemoryAccess.WriteU8(m, (c.S1 + 0x9u), (byte)c.V0);
        L80019B08: ;
        c.V0 = c.S2 + 0u;
        L80019B0C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019B24(CpuContext c, IMemory m)
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
            goto L80019C30;
        }
        c.A0 = 0x00000001u;
        c.V0 = MemoryAccess.ReadU32(m, c.S2);
        if (c.V0 != c.A0) {
            c.V0 = c.A0 + 0u;
            goto L80019C34;
        }
        c.V0 = c.A0 + 0u;
        c.V0 = c.V1 & 0x000Cu;
        if (c.V0 == 0u) {
            c.V0 = c.V1 & 0x0004u;
            goto L80019C30;
        }
        c.V0 = c.V1 & 0x0004u;
        c.S1 = MemoryAccess.ReadU8(m, (c.S2 + 0x5u));
        if (c.V0 == 0u) {
            c.V0 = c.V1 & 0x0008u;
            goto L80019B8C;
        }
        c.V0 = c.V1 & 0x0008u;
        c.S1 = c.S1 - 0x1u;
        if ((int)c.S1 >= 0) {
            goto L80019B8C;
        }
        c.S1 = 0x00000008u;
        L80019B8C: ;
        if (c.V0 == 0u) {
            c.S0 = c.S2 + 0x4u;
            goto L80019BA8;
        }
        c.S0 = c.S2 + 0x4u;
        c.S1 = c.S1 + 0x1u;
        c.V0 = (int)c.S1 < 9 ? 1u : 0u;
        if (c.V0 != 0u) {
            goto L80019BA8;
        }
        c.S1 = 0u + 0u;
        L80019BA8: ;
        c.V0 = MemoryAccess.ReadU8(m, (c.S0 + 0x1u));
        if (c.S1 == c.V0) {
            c.V0 = c.A0 + 0u;
            goto L80019C34;
        }
        c.V0 = c.A0 + 0u;
        c.A0 = 0x00000005u;
        c.RA = 0x80019BC0u;
        GranTurismo2PC.func_80060840(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.S0 + 0x1u));
        if (c.V0 != 0u) {
            c.A0 = 0u + 0u;
            goto L80019BE8;
        }
        c.A0 = 0u + 0u;
        c.V0 = MemoryAccess.ReadU8(m, (c.S2 + 0xAu));
        MemoryAccess.WriteU8(m, (c.S0 + 0x2u), (byte)c.V0);
        c.V0 = MemoryAccess.ReadU8(m, (c.S2 + 0xBu));
        MemoryAccess.WriteU8(m, (c.S0 + 0x3u), (byte)c.V0);
        L80019BE8: ;
        if (c.S1 != 0u) {
            MemoryAccess.WriteU8(m, (c.S0 + 0x1u), (byte)c.S1);
            goto L80019C08;
        }
        MemoryAccess.WriteU8(m, (c.S0 + 0x1u), (byte)c.S1);
        c.V0 = MemoryAccess.ReadU8(m, (c.S0 + 0x2u));
        MemoryAccess.WriteU8(m, (c.S2 + 0xAu), (byte)c.V0);
        c.V0 = MemoryAccess.ReadU8(m, (c.S0 + 0x3u));
        MemoryAccess.WriteU8(m, (c.S2 + 0xBu), (byte)c.V0);
        goto L80019C30;
        L80019C08: ;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 - 0x3EE0u;
        c.V1 = c.S1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU8(m, c.V1);
        MemoryAccess.WriteU8(m, (c.S2 + 0xAu), (byte)c.V0);
        c.V0 = MemoryAccess.ReadU8(m, (c.V1 + 0x2u));
        MemoryAccess.WriteU8(m, (c.S2 + 0xBu), (byte)c.V0);
        L80019C30: ;
        c.V0 = c.A0 + 0u;
        L80019C34: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019C4C(CpuContext c, IMemory m)
    {
        c.V1 = MemoryAccess.ReadU32(m, c.A0);
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            c.A1 = c.V1 + 0u;
            goto L80019D10;
        }
        c.A1 = c.V1 + 0u;
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L80019C78;
        }
        if (c.V1 == 0u) {
            c.V0 = 0x80090000u;
            goto L80019C8C;
        }
        c.V0 = 0x80090000u;
        return;
        L80019C78: ;
        c.V0 = 0x00000002u;
        if (c.A1 == c.V0) {
            c.V0 = 0x80090000u;
            goto L80019CCC;
        }
        c.V0 = 0x80090000u;
        return;
        L80019C8C: ;
        c.T1 = c.V0 + 0x1570u;
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
        L80019CCC: ;
        c.T1 = c.V0 + 0x1588u;
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
        L80019D10: ;
        c.V0 = 0x80090000u;
        c.T1 = c.V0 + 0x157Cu;
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
    public static void func_80019D5C(CpuContext c, IMemory m)
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
        c.V0 = c.V0 - 0x3F24u;
        c.A2 = c.A2 << 1;
        c.A2 = c.A2 + c.V0;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S2 = (uint)(short)MemoryAccess.ReadU16(m, c.A2);
        c.S1 = MemoryAccess.ReadU32(m, (c.S3 + 0x4u));
        if (c.S2 == c.S0) {
            c.S4 = 0u + 0u;
            goto L80019DE0;
        }
        c.S4 = 0u + 0u;
        if ((int)c.S2 >= 0) {
            c.V0 = 0xFFFFFFFEu;
            goto L80019DC0;
        }
        c.V0 = 0xFFFFFFFEu;
        if (c.S2 == c.V0) {
            c.V0 = c.S1 & 0x0A00u;
            goto L80019DD0;
        }
        c.V0 = c.S1 & 0x0A00u;
        goto L80019E1C;
        L80019DC0: ;
        if (c.S2 == 0u) {
            c.A0 = c.S5 + 0u;
            goto L80019E04;
        }
        c.A0 = c.S5 + 0u;
        goto L80019E1C;
        L80019DD0: ;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L80019F1C;
        }
        c.V0 = 0x00000002u;
        goto L80019F20;
        L80019DE0: ;
        c.V0 = c.S1 & 0x0A00u;
        if (c.V0 == 0u) {
            c.V0 = c.S4 + 0u;
            goto L80019F20;
        }
        c.V0 = c.S4 + 0u;
        c.A0 = c.S5 + 0u;
        c.RA = 0x80019DF4u;
        GranTurismo2PC.func_80019C4C(c, m);
        c.A0 = 0x00000001u;
        c.RA = 0x80019DFCu;
        GranTurismo2PC.func_80060840(c, m);
        c.V0 = 0x00000001u;
        goto L80019F20;
        L80019E04: ;
        c.A1 = c.S3 + 0u;
        c.RA = 0x80019E0Cu;
        GranTurismo2PC.func_80019934(c, m);
        if (c.V0 != 0u) {
            c.V0 = c.S4 + 0u;
            goto L80019F20;
        }
        c.V0 = c.S4 + 0u;
        c.S4 = 0x00000001u;
        goto L80019F1C;
        L80019E1C: ;
        c.V1 = MemoryAccess.ReadU32(m, c.S5);
        c.V0 = 0x00000001u;
        if (c.V1 != c.V0) {
            c.V0 = (int)c.S2 < 4 ? 1u : 0u;
            goto L80019E4C;
        }
        c.V0 = (int)c.S2 < 4 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = (int)c.S2 < 2 ? 1u : 0u;
            goto L80019E4C;
        }
        c.V0 = (int)c.S2 < 2 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.A0 = c.S5 + 0u;
            goto L80019E4C;
        }
        c.A0 = c.S5 + 0u;
        c.A1 = c.S3 + 0u;
        c.RA = 0x80019E44u;
        GranTurismo2PC.func_80019B24(c, m);
        if (c.V0 == 0u) {
            c.V0 = 0x00000001u;
            goto L80019F20;
        }
        c.V0 = 0x00000001u;
        L80019E4C: ;
        c.V0 = MemoryAccess.ReadU32(m, c.S3);
        c.V1 = 0x00010000u;
        c.V0 = c.V0 & c.V1;
        if (c.V0 == 0u) {
            c.V0 = c.S1 & 0x0001u;
            goto L80019E94;
        }
        c.V0 = c.S1 & 0x0001u;
        if (c.V0 == 0u) {
            c.V0 = c.S1 & 0x0002u;
            goto L80019E6C;
        }
        c.V0 = c.S1 & 0x0002u;
        c.S0 = 0u + 0u;
        L80019E6C: ;
        if (c.V0 == 0u) {
            c.V0 = c.S1 & 0x0004u;
            goto L80019E78;
        }
        c.V0 = c.S1 & 0x0004u;
        c.S0 = 0x00000001u;
        L80019E78: ;
        if (c.V0 == 0u) {
            c.V0 = c.S1 & 0x0008u;
            goto L80019E84;
        }
        c.V0 = c.S1 & 0x0008u;
        c.S0 = 0x00000002u;
        L80019E84: ;
        if (c.V0 == 0u) {
            goto L80019EF8;
        }
        c.S0 = 0x00000003u;
        goto L80019EF8;
        L80019E94: ;
        c.V0 = c.S1 & 0x0010u;
        if (c.V0 == 0u) {
            c.V0 = c.S1 & 0x0020u;
            goto L80019EA4;
        }
        c.V0 = c.S1 & 0x0020u;
        c.S0 = 0x00000004u;
        L80019EA4: ;
        if (c.V0 == 0u) {
            c.V0 = c.S1 & 0x1000u;
            goto L80019EB0;
        }
        c.V0 = c.S1 & 0x1000u;
        c.S0 = 0x00000005u;
        L80019EB0: ;
        if (c.V0 == 0u) {
            c.V0 = c.S1 & 0x2000u;
            goto L80019EBC;
        }
        c.V0 = c.S1 & 0x2000u;
        c.S0 = 0x0000000Cu;
        L80019EBC: ;
        if (c.V0 == 0u) {
            c.V0 = c.S1 & 0x0200u;
            goto L80019EC8;
        }
        c.V0 = c.S1 & 0x0200u;
        c.S0 = 0x0000000Du;
        L80019EC8: ;
        if (c.V0 == 0u) {
            c.V0 = c.S1 & 0x0100u;
            goto L80019ED4;
        }
        c.V0 = c.S1 & 0x0100u;
        c.S0 = 0x00000009u;
        L80019ED4: ;
        if (c.V0 == 0u) {
            c.V0 = c.S1 & 0x0400u;
            goto L80019EE0;
        }
        c.V0 = c.S1 & 0x0400u;
        c.S0 = 0x00000008u;
        L80019EE0: ;
        if (c.V0 == 0u) {
            c.V0 = c.S1 & 0x0800u;
            goto L80019EEC;
        }
        c.V0 = c.S1 & 0x0800u;
        c.S0 = 0x0000000Au;
        L80019EEC: ;
        if (c.V0 == 0u) {
            goto L80019EF8;
        }
        c.S0 = 0x0000000Bu;
        L80019EF8: ;
        if ((int)c.S0 < 0) {
            c.V0 = c.S4 + 0u;
            goto L80019F20;
        }
        c.V0 = c.S4 + 0u;
        c.A0 = 0x00000001u;
        c.RA = 0x80019F08u;
        GranTurismo2PC.func_80060840(c, m);
        c.A0 = c.S5 + 0u;
        c.A1 = c.S2 + 0u;
        c.A2 = c.S0 + 0u;
        c.RA = 0x80019F18u;
        GranTurismo2PC.func_800196E8(c, m);
        c.S4 = 0x00000001u;
        L80019F1C: ;
        c.V0 = c.S4 + 0u;
        L80019F20: ;
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
    public static void func_80019F44(CpuContext c, IMemory m)
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
        c.RA = 0x80019F98u;
        GranTurismo2PC.func_8006AC68(c, m);
        c.V1 = 0xFF9F0000u;
        c.V1 = c.V1 | 0xFFFFu;
        c.S0 = c.SP + 0x20u;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6A40u;
        c.T0 = 0x80050000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0xCu));
        c.S3 = c.T0 - 0x3D54u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x10u), c.S5);
        c.V0 = c.V0 & c.V1;
        c.V1 = 0x00200000u;
        c.V0 = c.V0 | c.V1;
        MemoryAccess.WriteU32(m, (c.S0 + 0xCu), c.V0);
        c.RA = 0x80019FD4u;
        GranTurismo2PC.func_8007DA80(c, m);
        c.V0 = 0x00140000u;
        c.V0 = c.V0 | 0x2864u;
        c.A1 = 0x00000030u;
        c.S7 = 0x00000001u;
        if (c.S6 != c.S2) {
            MemoryAccess.WriteU32(m, (c.S0 + 0x14u), c.V0);
            goto L80019FF0;
        }
        MemoryAccess.WriteU32(m, (c.S0 + 0x14u), c.V0);
        c.A1 = 0x00000080u;
        L80019FF0: ;
        if ((int)c.S6 >= 0) {
            { var _r = (long)(int)c.S4 * (int)c.A1; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
            goto L8001A000;
        }
        { var _r = (long)(int)c.S4 * (int)c.A1; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.A1 = 0x00000080u;
        { var _r = (long)(int)c.S4 * (int)c.A1; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        L8001A000: ;
        c.V1 = MemoryAccess.ReadU32(m, c.S1);
        c.V0 = 0x00000003u;
        c.T0 = c.LO;
        if (c.V1 == c.V0) {
            c.S4 = (uint)((int)c.T0 >> 7);
            goto L8001A5F4;
        }
        c.S4 = (uint)((int)c.T0 >> 7);
        c.V0 = 0x00000009u;
        if (c.S2 == c.V0) {
            c.V0 = (int)c.S2 < 10 ? 1u : 0u;
            goto L8001A088;
        }
        c.V0 = (int)c.S2 < 10 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x0000000Au;
            goto L8001A038;
        }
        c.V0 = 0x0000000Au;
        if (c.S2 == 0u) {
            c.A1 = 0u + 0u;
            goto L8001A0E4;
        }
        c.A1 = 0u + 0u;
        c.V1 = c.S2 - 0x1u;
        goto L8001A370;
        L8001A038: ;
        if (c.S2 != c.V0) {
            c.A1 = 0u + 0u;
            goto L8001A36C;
        }
        c.A1 = 0u + 0u;
        if ((int)c.S6 < 0) {
            c.A0 = c.S0 + 0u;
            goto L8001A5F4;
        }
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6A30u;
        c.RA = 0x8001A054u;
        GranTurismo2PC.func_8007DA80(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x3F30u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x3F28u;
        c.A2 = c.S4 + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x8001A070u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x5F12u;
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0x7Cu));
        c.A2 = c.FP + 0u;
        goto L8001A0D0;
        L8001A088: ;
        if ((int)c.S6 < 0) {
            c.A0 = c.S0 + 0u;
            goto L8001A5F4;
        }
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6A30u;
        c.RA = 0x8001A09Cu;
        GranTurismo2PC.func_8007DA80(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x3F30u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x3F28u;
        c.A2 = c.S4 + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x8001A0B8u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x5F0Au;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x7Cu));
        c.A2 = c.FP + 0u;
        c.A3 = c.T0 + 0x6u;
        L8001A0D0: ;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S7);
        c.RA = 0x8001A0DCu;
        GranTurismo2PC.func_8006ADB4(c, m);
        goto L8001A5F4;
        L8001A0E4: ;
        if (c.V1 == c.S7) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L8001A260;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L8001A104;
        }
        if (c.V1 == 0u) {
            c.A0 = c.S5 + 0u;
            goto L8001A118;
        }
        c.A0 = c.S5 + 0u;
        goto L8001A5F4;
        L8001A104: ;
        c.V0 = 0x00000002u;
        if (c.V1 == c.V0) {
            goto L8001A170;
        }
        goto L8001A5F4;
        L8001A118: ;
        c.A1 = c.S4 << 8;
        c.A1 = c.S4 | c.A1;
        c.V0 = c.S4 << 16;
        c.A1 = c.A1 | c.V0;
        c.RA = 0x8001A12Cu;
        GranTurismo2PC.func_80081478(c, m);
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
        c.V0 = MemoryAccess.ReadU32(m, (c.T0 - 0x3D54u));
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.V0);
        goto L8001A5D4;
        L8001A170: ;
        c.V0 = MemoryAccess.ReadU8(m, (c.S1 + 0x4u));
        if (c.V0 != 0u) {
            c.A0 = c.S5 + 0u;
            goto L8001A188;
        }
        c.A0 = c.S5 + 0u;
        c.V0 = 0x80050000u;
        c.S3 = c.V0 - 0x3D30u;
        L8001A188: ;
        c.A1 = c.S4 << 8;
        c.A1 = c.S4 | c.A1;
        c.V0 = c.S4 << 16;
        c.A1 = c.A1 | c.V0;
        c.RA = 0x8001A19Cu;
        GranTurismo2PC.func_80081478(c, m);
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
        c.RA = 0x8001A1F8u;
        GranTurismo2PC.func_8007DA44(c, m);
        if (c.S6 != 0u) {
            c.S1 = c.SP + 0x40u;
            goto L8001A5F4;
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
        c.RA = 0x8001A22Cu;
        GranTurismo2PC.func_8006BA48(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S5 + 0u;
        c.A2 = c.FP - 0x10u;
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0x7Cu));
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x8Cu));
        c.V0 = 0xFFFFFFFAu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.T0);
        c.RA = 0x8001A254u;
        GranTurismo2PC.func_8006BA48(c, m);
        c.A0 = c.S5 + 0u;
        c.A1 = 0x00000020u;
        goto L8001A5EC;
        L8001A260: ;
        c.A0 = MemoryAccess.ReadU8(m, (c.S1 + 0x4u));
        if (c.A0 == 0u) {
            c.V0 = 0x80050000u;
            goto L8001A280;
        }
        c.V0 = 0x80050000u;
        if (c.A0 == c.V1) {
            c.A0 = c.S5 + 0u;
            goto L8001A288;
        }
        c.A0 = c.S5 + 0u;
        c.A1 = c.S4 << 8;
        goto L8001A298;
        L8001A280: ;
        c.S3 = c.V0 - 0x3D48u;
        goto L8001A290;
        L8001A288: ;
        c.V0 = 0x80050000u;
        c.S3 = c.V0 - 0x3D3Cu;
        L8001A290: ;
        c.A0 = c.S5 + 0u;
        c.A1 = c.S4 << 8;
        L8001A298: ;
        c.A1 = c.S4 | c.A1;
        c.V0 = c.S4 << 16;
        c.A1 = c.A1 | c.V0;
        c.RA = 0x8001A2A8u;
        GranTurismo2PC.func_80081478(c, m);
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
        c.RA = 0x8001A304u;
        GranTurismo2PC.func_8007DA44(c, m);
        if (c.S6 != 0u) {
            c.S1 = c.SP + 0x40u;
            goto L8001A5F4;
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
        c.RA = 0x8001A338u;
        GranTurismo2PC.func_8006BA48(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S5 + 0u;
        c.A2 = c.FP - 0x10u;
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0x7Cu));
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x8Cu));
        c.V0 = 0xFFFFFFFAu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.T0);
        c.RA = 0x8001A360u;
        GranTurismo2PC.func_8006BA48(c, m);
        c.A0 = c.S5 + 0u;
        c.A1 = 0x00000020u;
        goto L8001A5EC;
        L8001A36C: ;
        c.V1 = c.S2 - 0x1u;
        L8001A370: ;
        c.V0 = c.V1 < 0x00000008u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80020000u;
            goto L8001A3F0;
        }
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x2D5Cu;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x8001A398u: goto L8001A398;
            case 0x8001A3A4u: goto L8001A3A4;
            case 0x8001A3B0u: goto L8001A3B0;
            case 0x8001A3BCu: goto L8001A3BC;
            case 0x8001A3C8u: goto L8001A3C8;
            case 0x8001A3D4u: goto L8001A3D4;
            case 0x8001A3E0u: goto L8001A3E0;
            case 0x8001A3ECu: goto L8001A3EC;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L8001A398: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.S1 + 0xAu));
        goto L8001A3F0;
        L8001A3A4: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.S1 + 0xBu));
        goto L8001A3F0;
        L8001A3B0: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.S1 + 0xDu));
        goto L8001A3F0;
        L8001A3BC: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.S1 + 0xCu));
        goto L8001A3F0;
        L8001A3C8: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.S1 + 0xEu));
        goto L8001A3F0;
        L8001A3D4: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.S1 + 0xFu));
        goto L8001A3F0;
        L8001A3E0: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.S1 + 0x11u));
        goto L8001A3F0;
        L8001A3EC: ;
        c.A1 = MemoryAccess.ReadU8(m, (c.S1 + 0x10u));
        L8001A3F0: ;
        c.A0 = MemoryAccess.ReadU32(m, c.S1);
        c.V0 = 0x00000001u;
        if (c.A0 == c.V0) {
            c.V1 = c.A0 + 0u;
            goto L8001A460;
        }
        c.V1 = c.A0 + 0u;
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L8001A41C;
        }
        if (c.A0 == 0u) {
            c.A0 = c.S5 + 0u;
            goto L8001A430;
        }
        c.A0 = c.S5 + 0u;
        c.A1 = c.S4 << 8;
        goto L8001A584;
        L8001A41C: ;
        c.V0 = 0x00000002u;
        if (c.V1 == c.V0) {
            c.A0 = c.S5 + 0u;
            goto L8001A544;
        }
        c.A0 = c.S5 + 0u;
        c.A1 = c.S4 << 8;
        goto L8001A584;
        L8001A430: ;
        c.V1 = 0x80050000u;
        c.V1 = c.V1 - 0x3D24u;
        c.V0 = c.A1 << 1;
        c.V0 = c.V0 + c.V1;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.V0);
        c.V1 = c.V0 << 1;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 2;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 - 0x3EBCu;
        goto L8001A578;
        L8001A460: ;
        c.V0 = (int)c.A1 < 128 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V1 = 0x80050000u;
            goto L8001A48C;
        }
        c.V1 = 0x80050000u;
        c.A0 = c.A1 + 0u;
        c.RA = 0x8001A474u;
        GranTurismo2PC.func_80019288(c, m);
        c.V1 = c.V0 << 1;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 2;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 - 0x3DB4u;
        goto L8001A4B4;
        L8001A48C: ;
        c.V1 = c.V1 - 0x3D24u;
        c.V0 = c.A1 << 1;
        c.V0 = c.V0 + c.V1;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.V0);
        c.V1 = c.V0 << 1;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 2;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 - 0x3EBCu;
        L8001A4B4: ;
        c.S3 = c.V1 + c.V0;
        c.V1 = c.S6 - 0x1u;
        c.V1 = c.V1 < 0x00000002u ? 1u : 0u;
        c.V0 = c.S2 ^ 0x0001u;
        c.V0 = c.V0 < 0x00000001u ? 1u : 0u;
        c.V1 = c.V1 & c.V0;
        if (c.V1 == 0u) {
            c.S2 = c.SP + 0x40u;
            goto L8001A57C;
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
        c.RA = 0x8001A508u;
        GranTurismo2PC.func_8006BA48(c, m);
        c.A0 = c.S2 + 0u;
        c.A1 = c.S5 + 0u;
        c.A2 = c.FP - 0x10u;
        c.A3 = c.S1 + 0u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x8Cu));
        c.V0 = 0xFFFFFFFAu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.T0);
        c.RA = 0x8001A530u;
        GranTurismo2PC.func_8006BA48(c, m);
        c.A0 = c.S5 + 0u;
        c.A1 = 0x00000020u;
        c.RA = 0x8001A53Cu;
        GranTurismo2PC.func_8007DA44(c, m);
        c.A0 = c.S5 + 0u;
        goto L8001A580;
        L8001A544: ;
        c.A0 = c.A1 + 0u;
        c.S0 = 0x80050000u;
        c.S0 = c.S0 - 0x3D04u;
        c.RA = 0x8001A554u;
        GranTurismo2PC.func_80019250(c, m);
        c.V0 = c.V0 << 1;
        c.V0 = c.V0 + c.S0;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.V0);
        c.V1 = c.V0 << 1;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 2;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 - 0x3E2Cu;
        L8001A578: ;
        c.S3 = c.V1 + c.V0;
        L8001A57C: ;
        c.A0 = c.S5 + 0u;
        L8001A580: ;
        c.A1 = c.S4 << 8;
        L8001A584: ;
        c.A1 = c.S4 | c.A1;
        c.V0 = c.S4 << 16;
        c.A1 = c.A1 | c.V0;
        c.RA = 0x8001A594u;
        GranTurismo2PC.func_80081478(c, m);
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
        L8001A5D4: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S3 + 0x4u));
        MemoryAccess.WriteU32(m, (c.A0 + 0x8u), c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S3 + 0x8u));
        c.A0 = c.S5 + 0u;
        c.A1 = c.A1 | 0x0020u;
        L8001A5EC: ;
        c.RA = 0x8001A5F4u;
        GranTurismo2PC.func_8007DA44(c, m);
        L8001A5F4: ;
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
    public static void func_8001A624_gt2_overlay_1(CpuContext c, IMemory m)
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
        c.RA = 0x8001A674u;
        GranTurismo2PC.func_8006AC68(c, m);
        c.V1 = 0xFF9F0000u;
        c.V1 = c.V1 | 0xFFFFu;
        c.A0 = c.SP + 0x20u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6A40u;
        c.S1 = 0u + 0u;
        c.V0 = MemoryAccess.ReadU32(m, (c.A0 + 0xCu));
        c.S5 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x10u), c.S7);
        c.V0 = c.V0 & c.V1;
        c.V1 = 0x00200000u;
        c.V0 = c.V0 | c.V1;
        MemoryAccess.WriteU32(m, (c.A0 + 0xCu), c.V0);
        c.RA = 0x8001A6ACu;
        GranTurismo2PC.func_8007DA80(c, m);
        c.V0 = 0x80050000u;
        c.S3 = c.V0 - 0x3F0Cu;
        L8001A6B4: ;
        c.V0 = (int)c.S1 < 11 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.T0 = 0x80050000u;
            goto L8001A764;
        }
        c.T0 = 0x80050000u;
        c.A0 = c.T0 - 0x3F30u;
        c.T0 = 0x80050000u;
        c.A1 = c.T0 - 0x3F2Cu;
        c.A2 = c.S6 + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x8001A6D8u;
        GranTurismo2PC.func_8006B548(c, m);
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
        c.RA = 0x8001A704u;
        GranTurismo2PC.func_8006ADB4(c, m);
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x13D8u;
        c.A1 = c.S7 + 0u;
        c.A2 = c.S4 - 0x50u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x78u));
        c.A3 = c.S0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.T0);
        c.RA = 0x8001A730u;
        GranTurismo2PC.func_80019F44(c, m);
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x13F0u;
        c.A1 = c.S7 + 0u;
        c.A2 = c.S4 + 0x50u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x7Cu));
        c.A3 = c.S0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.T0);
        c.RA = 0x8001A75Cu;
        GranTurismo2PC.func_80019F44(c, m);
        c.S1 = c.S1 + 0x1u;
        goto L8001A6B4;
        L8001A764: ;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x78u));
        if ((int)c.T0 < 0) {
            c.A0 = c.SP + 0x20u;
            goto L8001A7B0;
        }
        c.A0 = c.SP + 0x20u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6A30u;
        c.RA = 0x8001A780u;
        GranTurismo2PC.func_8007DA80(c, m);
        c.V0 = 0x000B0000u;
        c.V0 = c.V0 | 0x3060u;
        c.A0 = c.SP + 0x20u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x5E6Au;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x70u));
        c.A2 = c.S4 + 0u;
        c.A3 = c.T0 - 0x18u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.V0);
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.RA = 0x8001A7B0u;
        GranTurismo2PC.func_8006ADB4(c, m);
        L8001A7B0: ;
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
    public static void func_8001A7E0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x13D8u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        MemoryAccess.WriteU16(m, c.S1, (ushort)c.S0);
        MemoryAccess.WriteU16(m, (c.S1 + 0x2u), (ushort)c.S0);
        MemoryAccess.WriteU16(m, (c.S1 + 0x4u), (ushort)0u);
        c.RA = 0x8001A810u;
        GranTurismo2PC.func_8001937C(c, m);
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x13F0u;
        c.RA = 0x8001A81Cu;
        GranTurismo2PC.func_8001937C(c, m);
        MemoryAccess.WriteU16(m, (c.S1 + 0x6u), (ushort)c.S0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001A834(CpuContext c, IMemory m)
    {
        c.V0 = 0x0000000Au;
        MemoryAccess.WriteU16(m, (c.A0 + 0x2u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, c.A0, (ushort)c.V0);
        c.V0 = 0x00000004u;
        MemoryAccess.WriteU16(m, (c.A0 + 0x6u), (ushort)c.V0);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001A84C(CpuContext c, IMemory m)
    {
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU16(m, (c.A0 + 0x2u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, c.A0, (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.A0 + 0x6u), (ushort)c.V0);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001A860(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x30u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S5);
        c.S5 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
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
            goto L8001A8B4;
        }
        c.S6 = c.S4 + 0u;
        c.S2 = 0u + 0u;
        c.V0 = c.V1 - 0x1u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x6u), (ushort)c.V0);
        L8001A8B4: ;
        c.V0 = MemoryAccess.ReadU16(m, (c.S0 + 0x4u));
        c.V0 = c.V0 + 0x1u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x4u), (ushort)c.V0);
        c.V0 = c.V0 << 16;
        c.V0 = (uint)((int)c.V0 >> 16);
        c.V0 = (int)c.V0 < 45 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = 0x800B0000u;
            goto L8001A8DC;
        }
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x4u), (ushort)0u);
        L8001A8DC: ;
        c.S3 = c.V0 + 0x13D8u;
        c.A0 = c.S3 + 0u;
        c.A1 = c.S5 + 0xDCu;
        c.A2 = 0u + 0u;
        c.RA = 0x8001A8F0u;
        GranTurismo2PC.func_80019388(c, m);
        c.V0 = 0x800B0000u;
        c.S1 = c.V0 + 0x13F0u;
        c.A0 = c.S1 + 0u;
        c.A1 = c.S5 + 0x140u;
        c.A2 = 0x00000001u;
        c.RA = 0x8001A908u;
        GranTurismo2PC.func_80019388(c, m);
        if (c.S2 == 0u) {
            c.A0 = c.S3 + 0u;
            goto L8001A934;
        }
        c.A0 = c.S3 + 0u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, c.S0);
        c.A1 = c.S5 + 0x1A4u;
        c.RA = 0x8001A91Cu;
        GranTurismo2PC.func_80019D5C(c, m);
        c.S4 = c.V0 + 0u;
        c.A0 = c.S1 + 0u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x2u));
        c.A1 = c.S5 + 0x1B4u;
        c.RA = 0x8001A930u;
        GranTurismo2PC.func_80019D5C(c, m);
        c.S6 = c.V0 + 0u;
        L8001A934: ;
        c.A0 = c.S3 + 0u;
        c.A1 = 0u + 0u;
        c.RA = 0x8001A940u;
        GranTurismo2PC.func_80019498(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = 0x00000001u;
        c.RA = 0x8001A94Cu;
        GranTurismo2PC.func_80019498(c, m);
        c.V1 = c.S4 ^ 0x0002u;
        c.V1 = c.V1 < 0x00000001u ? 1u : 0u;
        c.V0 = c.S6 ^ 0x0002u;
        c.V0 = c.V0 < 0x00000001u ? 1u : 0u;
        c.V1 = c.V1 | c.V0;
        if (c.V1 != 0u) {
            c.V0 = 0u + 0u;
            goto L8001AA5C;
        }
        c.V0 = 0u + 0u;
        c.V0 = 0u < c.S2 ? 1u : 0u;
        c.V1 = c.S4 < 0x00000001u ? 1u : 0u;
        c.V0 = c.V0 & c.V1;
        if (c.V0 == 0u) {
            c.V0 = 0u < c.S2 ? 1u : 0u;
            goto L8001A9E4;
        }
        c.V0 = 0u < c.S2 ? 1u : 0u;
        c.V1 = MemoryAccess.ReadU32(m, (c.S2 + 0x4u));
        c.V0 = MemoryAccess.ReadU32(m, (c.S2 + 0xCu));
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, c.S0);
        c.V1 = c.V1 | c.V0;
        c.V0 = c.V1 & 0x0001u;
        if (c.V0 == 0u) {
            c.V0 = c.V1 & 0x0002u;
            goto L8001A9A8;
        }
        c.V0 = c.V1 & 0x0002u;
        c.A0 = c.A0 - 0x1u;
        if ((int)c.A0 >= 0) {
            goto L8001A9A8;
        }
        c.A0 = 0x0000000Au;
        L8001A9A8: ;
        if (c.V0 == 0u) {
            goto L8001A9C4;
        }
        c.A0 = c.A0 + 0x1u;
        c.V0 = (int)c.A0 < 11 ? 1u : 0u;
        if (c.V0 != 0u) {
            goto L8001A9C4;
        }
        c.A0 = 0u + 0u;
        L8001A9C4: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.S0);
        if (c.A0 == c.V0) {
            c.V0 = 0u < c.S2 ? 1u : 0u;
            goto L8001A9E4;
        }
        c.V0 = 0u < c.S2 ? 1u : 0u;
        MemoryAccess.WriteU16(m, c.S0, (ushort)c.A0);
        c.A0 = 0x00000006u;
        c.RA = 0x8001A9E0u;
        GranTurismo2PC.func_80060840(c, m);
        c.V0 = 0u < c.S2 ? 1u : 0u;
        L8001A9E4: ;
        c.V1 = c.S6 < 0x00000001u ? 1u : 0u;
        c.V0 = c.V0 & c.V1;
        if (c.V0 == 0u) {
            c.V0 = c.S5 + 0x1B4u;
            goto L8001AA58;
        }
        c.V0 = c.S5 + 0x1B4u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0xCu));
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x2u));
        c.V1 = c.V1 | c.V0;
        c.V0 = c.V1 & 0x0001u;
        if (c.V0 == 0u) {
            c.V0 = c.V1 & 0x0002u;
            goto L8001AA20;
        }
        c.V0 = c.V1 & 0x0002u;
        c.A0 = c.A0 - 0x1u;
        if ((int)c.A0 >= 0) {
            goto L8001AA20;
        }
        c.A0 = 0x0000000Au;
        L8001AA20: ;
        if (c.V0 == 0u) {
            goto L8001AA3C;
        }
        c.A0 = c.A0 + 0x1u;
        c.V0 = (int)c.A0 < 11 ? 1u : 0u;
        if (c.V0 != 0u) {
            goto L8001AA3C;
        }
        c.A0 = 0u + 0u;
        L8001AA3C: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x2u));
        if (c.A0 == c.V0) {
            c.V0 = 0xFFFFFFFFu;
            goto L8001AA5C;
        }
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU16(m, (c.S0 + 0x2u), (ushort)c.A0);
        c.A0 = 0x00000006u;
        c.RA = 0x8001AA58u;
        GranTurismo2PC.func_80060840(c, m);
        L8001AA58: ;
        c.V0 = 0xFFFFFFFFu;
        L8001AA5C: ;
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
    public static void func_8001AA84(CpuContext c, IMemory m)
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
        c.RA = 0x8001AAC4u;
        Dispatcher.Call(c, m, 0x8001A624u);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001AAD4(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V1 = 0x801D0000u;
        c.V1 = MemoryAccess.ReadU8(m, (c.V1 - 0x6720u));
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 - 0x72A0u));
        c.SP = c.SP - 0x18u;
        if (c.V1 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
            goto L8001AAF8;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.RA = 0x8001AAF8u;
        GranTurismo2PC.func_8007F174(c, m);
        L8001AAF8: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001AB08(CpuContext c, IMemory m)
    {
        c.A1 = c.A0 + 0u;
        c.V0 = 0x800B0000u;
        c.V1 = 0x801D0000u;
        c.V1 = MemoryAccess.ReadU8(m, (c.V1 - 0x6720u));
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 - 0x72A0u));
        c.SP = c.SP - 0x18u;
        if (c.V1 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
            goto L8001AB30;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.RA = 0x8001AB30u;
        GranTurismo2PC.func_8006ECD8(c, m);
        L8001AB30: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001AB40(CpuContext c, IMemory m)
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
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6720u));
        c.V1 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        c.S4 = MemoryAccess.ReadU32(m, (c.V1 - 0x72A0u));
        c.A0 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        if (c.V0 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
            goto L8001AC4C;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        if (c.A2 == 0u) {
            c.S0 = c.A1 + 0u;
            goto L8001ABCC;
        }
        c.S0 = c.A1 + 0u;
        c.V0 = MemoryAccess.ReadU8(m, c.A1);
        if (c.V0 == 0u) {
            c.V1 = c.A1 + 0u;
            goto L8001ABC0;
        }
        c.V1 = c.A1 + 0u;
        L8001ABAC: ;
        c.V1 = c.V1 + 0x2u;
        c.V0 = MemoryAccess.ReadU8(m, c.V1);
        if (c.V0 != 0u) {
            c.A0 = c.A0 + 0xCu;
            goto L8001ABAC;
        }
        c.A0 = c.A0 + 0xCu;
        L8001ABC0: ;
        c.V0 = (uint)((int)c.A0 >> 1);
        c.S1 = c.S1 - c.V0;
        c.S0 = c.A1 + 0u;
        L8001ABCC: ;
        c.S5 = 0x02000000u;
        c.S2 = c.S3 + 0x1Fu;
        L8001ABD4: ;
        c.V0 = MemoryAccess.ReadU8(m, c.S0);
        if (c.V0 == 0u) {
            c.A0 = c.S4 + 0u;
            goto L8001AC4C;
        }
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.V0 = MemoryAccess.ReadU8(m, (c.S0 + 0x1u));
        c.A1 = c.A1 << 8;
        c.A1 = c.A1 | c.V0;
        c.RA = 0x8001ABF8u;
        GranTurismo2PC.func_8007F18C(c, m);
        if (c.V0 == 0u) {
            c.A0 = c.V0 + 0u;
            goto L8001AC40;
        }
        c.A0 = c.V0 + 0u;
        c.A1 = c.S7 + 0u;
        c.A2 = c.S6 | c.S5;
        c.RA = 0x8001AC0Cu;
        GranTurismo2PC.func_8007F01C(c, m);
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
        L8001AC40: ;
        c.S1 = c.S1 + 0xCu;
        c.S0 = c.S0 + 0x2u;
        goto L8001ABD4;
        L8001AC4C: ;
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
    public static void func_8001AC78(CpuContext c, IMemory m)
    {
        c.V0 = 0x801D0000u;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6720u));
        c.SP = c.SP - 0x18u;
        if (c.V0 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
            goto L8001ACA4;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.RA = 0x8001AC94u;
        GranTurismo2PC.func_8001AAD4(c, m);
        c.V0 = 0x800B0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1408u));
        c.RA = 0x8001ACA4u;
        GranTurismo2PC.func_8001AB08(c, m);
        L8001ACA4: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001ACB4(CpuContext c, IMemory m)
    {
        c.V1 = MemoryAccess.ReadU32(m, c.A0);
        c.V0 = (int)c.V1 < 64 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = (int)c.V1 < 193 ? 1u : 0u;
            goto L8001ACD0;
        }
        c.V0 = (int)c.V1 < 193 ? 1u : 0u;
        c.V1 = 0x00000040u;
        c.V0 = (int)c.V1 < 193 ? 1u : 0u;
        L8001ACD0: ;
        if (c.V0 != 0u) {
            goto L8001ACDC;
        }
        c.V1 = 0x000000C0u;
        L8001ACDC: ;
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0x10u));
        c.V0 = (int)c.V1 < (int)c.A1 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L8001ACF4;
        }
        c.V1 = c.A1 + 0u;
        L8001ACF4: ;
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0xCu));
        c.V0 = (int)c.A1 < (int)c.V1 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L8001AD0C;
        }
        c.V1 = c.A1 + 0u;
        L8001AD0C: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.A0 + 0x4u));
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0x10u));
        c.A2 = c.V1 + 0u;
        MemoryAccess.WriteU32(m, c.A0, c.V1);
        c.V1 = c.V1 - c.V0;
        c.V0 = (int)c.V1 < (int)c.A1 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V1 = c.A2 - c.A1;
            goto L8001AD30;
        }
        c.V1 = c.A2 - c.A1;
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.V1);
        L8001AD30: ;
        c.A2 = MemoryAccess.ReadU32(m, c.A0);
        c.V0 = MemoryAccess.ReadU32(m, (c.A0 + 0x4u));
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0xCu));
        c.V1 = c.A2 + c.V0;
        c.V0 = (int)c.A1 < (int)c.V1 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V1 = c.A1 - c.A2;
            goto L8001AD50;
        }
        c.V1 = c.A1 - c.A2;
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.V1);
        L8001AD50: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.A0 + 0x4u));
        if ((int)c.V0 >= 0) {
            goto L8001AD64;
        }
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), 0u);
        L8001AD64: ;
        c.V1 = MemoryAccess.ReadU32(m, (c.A0 + 0x4u));
        c.V0 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.V0 = (int)c.V1 < (int)c.V0 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L8001AD80;
        }
        MemoryAccess.WriteU32(m, (c.A0 + 0x8u), c.V1);
        L8001AD80: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        if ((int)c.V0 >= 0) {
            goto L8001AD94;
        }
        MemoryAccess.WriteU32(m, (c.A0 + 0x8u), 0u);
        L8001AD94: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001AD9C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU32(m, (c.A0 + 0x8u), c.A1);
        c.RA = 0x8001ADACu;
        GranTurismo2PC.func_8001ACB4(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001ADBC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.A1);
        c.RA = 0x8001ADCCu;
        GranTurismo2PC.func_8001ACB4(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001ADDC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU32(m, c.A0, c.A1);
        c.RA = 0x8001ADECu;
        GranTurismo2PC.func_8001ACB4(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001ADFC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V0 = (int)c.A1 < (int)c.A2 ? 1u : 0u;
        if (c.V0 == 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
            goto L8001AE18;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V0 = c.A1 + 0u;
        c.A1 = c.A2 + 0u;
        c.A2 = c.V0 + 0u;
        L8001AE18: ;
        MemoryAccess.WriteU32(m, (c.A0 + 0xCu), c.A1);
        MemoryAccess.WriteU32(m, (c.A0 + 0x10u), c.A2);
        c.RA = 0x8001AE24u;
        GranTurismo2PC.func_8001ACB4(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001AE34(CpuContext c, IMemory m)
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
            goto L8001AE78;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x38u), c.S0);
        c.S4 = 0x00000180u;
        L8001AE78: ;
        c.A0 = c.SP + 0x18u;
        c.A1 = 0x0000001Eu;
        c.RA = 0x8001AE84u;
        GranTurismo2PC.func_8006AC68(c, m);
        c.V1 = 0xFF9F0000u;
        c.V1 = c.V1 | 0xFFFFu;
        c.S0 = c.SP + 0x18u;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0xCu));
        c.A1 = c.A1 - 0x69E0u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x10u), c.S1);
        c.V0 = c.V0 & c.V1;
        c.V1 = 0x00200000u;
        c.V0 = c.V0 | c.V1;
        MemoryAccess.WriteU32(m, (c.S0 + 0xCu), c.V0);
        c.RA = 0x8001AEB8u;
        GranTurismo2PC.func_8007DA80(c, m);
        c.S1 = 0x80050000u;
        c.S1 = c.S1 - 0x3CE4u;
        c.A0 = c.S1 + 0u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x3CB8u;
        c.A2 = c.S5 + 0u;
        c.A3 = c.S4 + 0u;
        c.RA = 0x8001AED8u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S0 + 0u;
        c.A2 = c.S6 - 0x76u;
        c.S3 = c.S3 + 0x2Cu;
        c.A3 = c.S3 + 0u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0x6Cu));
        c.S2 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S2);
        c.RA = 0x8001AEFCu;
        GranTurismo2PC.func_8006AC90(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6A40u;
        c.RA = 0x8001AF0Cu;
        GranTurismo2PC.func_8007DA80(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x3CC0u;
        c.A2 = c.S5 + 0u;
        c.A3 = c.S4 + 0u;
        c.RA = 0x8001AF24u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x5DEEu;
        c.A2 = c.S6 + 0x6Eu;
        c.A3 = c.S3 + 0u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S2);
        c.RA = 0x8001AF44u;
        GranTurismo2PC.func_8006AE28(c, m);
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
    public static void func_8001AF6C(CpuContext c, IMemory m)
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
            goto L8001B020;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x80u), c.T0);
        c.T0 = 0x00000180u;
        c.S5 = c.T0 + 0u;
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x84u), c.T0);
        if (c.V1 == c.V0) {
            MemoryAccess.WriteU32(m, (c.SP + 0x80u), c.T0);
            goto L8001B00C;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x80u), c.T0);
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L8001AFF4;
        }
        c.V0 = 0x00000002u;
        if (c.V1 == 0u) {
            c.A0 = c.SP + 0x20u;
            goto L8001B004;
        }
        c.A0 = c.SP + 0x20u;
        goto L8001B024;
        L8001AFF4: ;
        if (c.V1 == c.V0) {
            c.A0 = c.SP + 0x20u;
            goto L8001B018;
        }
        c.A0 = c.SP + 0x20u;
        goto L8001B024;
        L8001B004: ;
        c.S5 = 0x00000080u;
        goto L8001B020;
        L8001B00C: ;
        c.T0 = 0x00000080u;
        MemoryAccess.WriteU32(m, (c.SP + 0x80u), c.T0);
        goto L8001B020;
        L8001B018: ;
        c.T0 = 0x00000080u;
        MemoryAccess.WriteU32(m, (c.SP + 0x84u), c.T0);
        L8001B020: ;
        c.A0 = c.SP + 0x20u;
        L8001B024: ;
        c.A1 = 0x0000001Eu;
        c.RA = 0x8001B02Cu;
        GranTurismo2PC.func_8006AC68(c, m);
        c.V1 = 0xFF9F0000u;
        c.V1 = c.V1 | 0xFFFFu;
        c.S0 = c.SP + 0x20u;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0xCu));
        c.A1 = c.A1 - 0x69E0u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x10u), c.S1);
        c.V0 = c.V0 & c.V1;
        c.V1 = 0x00200000u;
        c.V0 = c.V0 | c.V1;
        MemoryAccess.WriteU32(m, (c.S0 + 0xCu), c.V0);
        c.RA = 0x8001B060u;
        GranTurismo2PC.func_8007DA80(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x3CE4u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x3CB8u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0xC0u));
        c.A3 = 0x00000080u;
        c.RA = 0x8001B07Cu;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S0 + 0u;
        c.A2 = c.S4 - 0x76u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xC8u));
        c.A3 = c.S6 + 0x2Cu;
        MemoryAccess.WriteU32(m, (c.S0 + 0x14u), c.V0);
        c.T0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        c.RA = 0x8001B09Cu;
        GranTurismo2PC.func_8006AC90(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6A40u;
        c.RA = 0x8001B0ACu;
        GranTurismo2PC.func_8007DA80(c, m);
        c.FP = c.S4 + 0x48u;
        c.S6 = c.S6 + 0x1Eu;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x3CE4u;
        c.S2 = 0x80050000u;
        c.S2 = c.S2 - 0x3CC0u;
        c.A1 = c.S2 + 0u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0xC0u));
        c.A3 = c.S5 + 0u;
        c.RA = 0x8001B0D4u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S0 + 0u;
        c.S1 = 0x801C0000u;
        c.S1 = c.S1 - 0x5E02u;
        c.A1 = c.S1 + 0u;
        c.A2 = c.FP + 0u;
        c.A3 = c.S6 + 0u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x14u), c.V0);
        c.T0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        c.RA = 0x8001B0FCu;
        GranTurismo2PC.func_8006AE28(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x3CE4u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0xC0u));
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0x80u));
        c.A1 = c.S2 + 0u;
        c.RA = 0x8001B114u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0x7u;
        c.A2 = c.FP + 0u;
        c.S7 = c.S6 + 0x14u;
        c.A3 = c.S7 + 0u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x14u), c.V0);
        c.T0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        c.RA = 0x8001B138u;
        GranTurismo2PC.func_8006AE28(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x3CE4u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0xC0u));
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0x84u));
        c.A1 = c.S2 + 0u;
        c.RA = 0x8001B150u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0xCu;
        c.A2 = c.FP + 0u;
        c.S3 = c.S6 + 0x28u;
        c.A3 = c.S3 + 0u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x14u), c.V0);
        c.T0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        c.RA = 0x8001B174u;
        GranTurismo2PC.func_8006AE28(c, m);
        c.FP = c.S4 + 0x6Eu;
        c.S4 = c.SP + 0x40u;
        c.A0 = c.S4 + 0u;
        c.S2 = 0x80020000u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xB0u));
        c.S2 = c.S2 + 0x2D7Cu;
        c.A2 = MemoryAccess.ReadU32(m, c.T0);
        c.A1 = c.S2 + 0u;
        c.A2 = c.A2 - 0x80u;
        c.RA = 0x8001B19Cu;
        GranTurismo2PC.func_8008CF34(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x3CE4u;
        c.S1 = 0x80050000u;
        c.S1 = c.S1 - 0x3CBCu;
        c.A1 = c.S1 + 0u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0xC0u));
        c.A3 = c.S5 + 0u;
        c.RA = 0x8001B1BCu;
        GranTurismo2PC.func_8006B548(c, m);
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
        c.RA = 0x8001B1E8u;
        GranTurismo2PC.func_8006B184(c, m);
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xB0u));
        c.A0 = c.S4 + 0u;
        c.A2 = MemoryAccess.ReadU32(m, (c.T0 + 0x4u));
        c.A1 = c.S2 + 0u;
        c.RA = 0x8001B1FCu;
        GranTurismo2PC.func_8008CF34(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x3CE4u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0xC0u));
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0x80u));
        c.A1 = c.S1 + 0u;
        c.RA = 0x8001B214u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S4 + 0u;
        c.A2 = c.FP + 0u;
        c.A3 = c.S7 + 0u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x14u), c.V0);
        c.T0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.RA = 0x8001B23Cu;
        GranTurismo2PC.func_8006B184(c, m);
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xB0u));
        c.A0 = c.S4 + 0u;
        c.A2 = MemoryAccess.ReadU32(m, (c.T0 + 0x8u));
        c.A1 = c.S2 + 0u;
        c.RA = 0x8001B250u;
        GranTurismo2PC.func_8008CF34(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x3CE4u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0xC0u));
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0x84u));
        c.A1 = c.S1 + 0u;
        c.RA = 0x8001B268u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S4 + 0u;
        c.A2 = c.FP + 0u;
        c.A3 = c.S3 + 0u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.V0);
        c.T0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.RA = 0x8001B290u;
        GranTurismo2PC.func_8006B184(c, m);
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
    public static void func_8001B2C0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x78u;
        MemoryAccess.WriteU32(m, (c.SP + 0x64u), c.S5);
        c.S5 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x60u), c.S4);
        c.S4 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x70u), c.FP);
        c.FP = c.A3 + 0u;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x3CE4u;
        MemoryAccess.WriteU32(m, (c.SP + 0x50u), c.S0);
        c.S0 = 0x80050000u;
        c.S0 = c.S0 - 0x3CD8u;
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
        c.RA = 0x8001B3B4u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.S1 = c.SP + 0x20u;
        c.A2 = c.S1 + 0u;
        c.RA = 0x8001B3C8u;
        GranTurismo2PC.func_8007E780(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x3CE4u;
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
        c.RA = 0x8001B400u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.A2 = c.S1 + 0u;
        c.RA = 0x8001B410u;
        GranTurismo2PC.func_8007E780(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = 0x00000020u;
        c.RA = 0x8001B41Cu;
        GranTurismo2PC.func_8007DA44(c, m);
        c.T4 = MemoryAccess.ReadU32(m, (c.SP + 0x98u));
        if ((int)c.T4 < 0) {
            c.V0 = (int)c.T4 < 2 ? 1u : 0u;
            goto L8001B778;
        }
        c.V0 = (int)c.T4 < 2 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.A0 = c.S5 + 0u;
            goto L8001B4EC;
        }
        c.A0 = c.S5 + 0u;
        c.V0 = 0x00000002u;
        if (c.T4 != c.V0) {
            c.A0 = 0x80050000u;
            goto L8001B77C;
        }
        c.A0 = 0x80050000u;
        c.T5 = MemoryAccess.ReadU32(m, (c.SP + 0x9Cu));
        if ((int)c.T5 < 0) {
            c.S0 = c.S2 + c.T5;
            goto L8001B494;
        }
        c.S0 = c.S2 + c.T5;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x3CE0u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x3CC4u;
        c.A2 = c.S6 + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x8001B46Cu;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001B478u;
        GranTurismo2PC.func_8007D024(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU16(m, (c.V1 + 0x4u), (ushort)c.V0);
        c.V0 = 0x0000000Cu;
        MemoryAccess.WriteU16(m, c.V1, (ushort)c.S0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x2u), (ushort)c.S3);
        MemoryAccess.WriteU16(m, (c.V1 + 0x6u), (ushort)c.V0);
        L8001B494: ;
        c.T4 = MemoryAccess.ReadU32(m, (c.SP + 0xA0u));
        if ((int)c.T4 < 0) {
            c.S0 = c.S2 + c.T4;
            goto L8001B778;
        }
        c.S0 = c.S2 + c.T4;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x3CE0u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x3CC4u;
        c.A2 = c.S6 + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x8001B4C0u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001B4CCu;
        GranTurismo2PC.func_8007D024(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU16(m, (c.V1 + 0x4u), (ushort)c.V0);
        c.V0 = 0x0000000Cu;
        MemoryAccess.WriteU16(m, c.V1, (ushort)c.S0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x2u), (ushort)c.S3);
        MemoryAccess.WriteU16(m, (c.V1 + 0x6u), (ushort)c.V0);
        goto L8001B778;
        L8001B4EC: ;
        c.A1 = c.S4 + 0u;
        c.A2 = c.FP + 0u;
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x94u));
        c.T5 = MemoryAccess.ReadU32(m, (c.SP + 0x80u));
        c.A3 = c.S7 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.T5);
        c.RA = 0x8001B510u;
        GranTurismo2PC.func_8001AF6C(c, m);
        c.T4 = MemoryAccess.ReadU32(m, (c.SP + 0x90u));
        if ((int)c.T4 < 0) {
            c.A0 = 0x80050000u;
            goto L8001B588;
        }
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x3CE0u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x3CC4u;
        c.A2 = c.S6 + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x8001B538u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001B544u;
        GranTurismo2PC.func_8007D024(c, m);
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
        c.RA = 0x8001B588u;
        GranTurismo2PC.func_8006B988(c, m);
        L8001B588: ;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x3CE0u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x3CC8u;
        c.A2 = c.S6 + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x8001B5A4u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001B5B0u;
        GranTurismo2PC.func_8007D024(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x3CE4u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x3CD4u;
        c.A2 = c.S6 + 0u;
        c.A3 = 0x00000080u;
        c.T5 = MemoryAccess.ReadU16(m, (c.SP + 0x30u));
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU16(m, (c.V1 + 0x4u), (ushort)c.V0);
        c.V0 = 0x00000006u;
        MemoryAccess.WriteU16(m, (c.V1 + 0x2u), (ushort)c.S3);
        MemoryAccess.WriteU16(m, (c.V1 + 0x6u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, c.V1, (ushort)c.T5);
        c.RA = 0x8001B5ECu;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001B5F8u;
        GranTurismo2PC.func_8007D024(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x3CE4u;
        c.S0 = 0x80050000u;
        c.S0 = c.S0 - 0x3CD0u;
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
        c.RA = 0x8001B644u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001B650u;
        GranTurismo2PC.func_8007D024(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x3CE4u;
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
        c.RA = 0x8001B690u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001B69Cu;
        GranTurismo2PC.func_8007D024(c, m);
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
            goto L8001B77C;
        }
        c.A0 = 0x80050000u;
        c.T4 = MemoryAccess.ReadU32(m, (c.SP + 0x44u));
        c.V0 = 0x00000100u;
        c.V1 = c.T4 - c.S2;
        c.S0 = c.V0 - c.V1;
        if ((int)c.S0 <= 0) {
            c.A2 = c.S6 + 0u;
            goto L8001B72C;
        }
        c.A2 = c.S6 + 0u;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x3CE4u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x3CCCu;
        c.A3 = 0x00000080u;
        c.RA = 0x8001B708u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001B714u;
        GranTurismo2PC.func_8007D024(c, m);
        c.V1 = c.V0 + 0u;
        c.T5 = MemoryAccess.ReadU16(m, (c.SP + 0x44u));
        MemoryAccess.WriteU16(m, (c.V1 + 0x4u), (ushort)c.S0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x2u), (ushort)c.S3);
        MemoryAccess.WriteU16(m, (c.V1 + 0x6u), (ushort)c.S1);
        MemoryAccess.WriteU16(m, c.V1, (ushort)c.T5);
        L8001B72C: ;
        c.T4 = MemoryAccess.ReadU32(m, (c.SP + 0x48u));
        c.S0 = c.T4 - c.S2;
        if ((int)c.S0 <= 0) {
            c.A2 = c.S6 + 0u;
            goto L8001B778;
        }
        c.A2 = c.S6 + 0u;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x3CE4u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x3CCCu;
        c.A3 = 0x00000080u;
        c.RA = 0x8001B758u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001B764u;
        GranTurismo2PC.func_8007D024(c, m);
        c.V1 = c.V0 + 0u;
        MemoryAccess.WriteU16(m, c.V1, (ushort)c.S2);
        MemoryAccess.WriteU16(m, (c.V1 + 0x4u), (ushort)c.S0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x2u), (ushort)c.S3);
        MemoryAccess.WriteU16(m, (c.V1 + 0x6u), (ushort)c.S1);
        L8001B778: ;
        c.A0 = 0x80050000u;
        L8001B77C: ;
        c.A0 = c.A0 - 0x3CE4u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x3CDCu;
        c.A2 = c.S6 + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x8001B794u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001B7A0u;
        GranTurismo2PC.func_8007D024(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = c.S4 + 0u;
        c.A1 = 0u + 0u;
        c.V0 = 0x00000100u;
        MemoryAccess.WriteU16(m, (c.V1 + 0x4u), (ushort)c.V0);
        c.V0 = 0x0000000Cu;
        MemoryAccess.WriteU16(m, c.V1, (ushort)c.S2);
        MemoryAccess.WriteU16(m, (c.V1 + 0x2u), (ushort)c.S3);
        MemoryAccess.WriteU16(m, (c.V1 + 0x6u), (ushort)c.V0);
        c.RA = 0x8001B7C8u;
        GranTurismo2PC.func_8007DA44(c, m);
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
    public static void func_8001B7F8(CpuContext c, IMemory m)
    {
        c.V0 = MemoryAccess.ReadU32(m, c.A0);
        c.V0 = (int)c.V0 < 256 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = 0x000000FFu;
            goto L8001B810;
        }
        c.V0 = 0x000000FFu;
        MemoryAccess.WriteU32(m, c.A0, c.V0);
        L8001B810: ;
        c.V0 = MemoryAccess.ReadU32(m, c.A0);
        c.V0 = (int)c.V0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L8001B828;
        }
        c.V0 = 0x00000002u;
        MemoryAccess.WriteU32(m, c.A0, c.V0);
        L8001B828: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.A0 + 0x4u));
        c.V1 = MemoryAccess.ReadU32(m, c.A0);
        c.V0 = (int)c.V0 < (int)c.V1 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = c.V1 - 0x1u;
            goto L8001B844;
        }
        c.V0 = c.V1 - 0x1u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.V0);
        L8001B844: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.A0 + 0x4u));
        if ((int)c.V0 > 0) {
            c.V0 = 0x00000001u;
            goto L8001B858;
        }
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.V0);
        L8001B858: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001B860(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.A1);
        c.RA = 0x8001B870u;
        GranTurismo2PC.func_8001B7F8(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001B880(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU32(m, c.A0, c.A1);
        c.RA = 0x8001B890u;
        GranTurismo2PC.func_8001B7F8(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001B8A0(CpuContext c, IMemory m)
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
            goto L8001B8E4;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x38u), c.S0);
        c.S4 = 0x00000180u;
        L8001B8E4: ;
        c.A0 = c.SP + 0x18u;
        c.A1 = 0x0000001Eu;
        c.RA = 0x8001B8F0u;
        GranTurismo2PC.func_8006AC68(c, m);
        c.V1 = 0xFF9F0000u;
        c.V1 = c.V1 | 0xFFFFu;
        c.S0 = c.SP + 0x18u;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0xCu));
        c.A1 = c.A1 - 0x69E0u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x10u), c.S1);
        c.V0 = c.V0 & c.V1;
        c.V1 = 0x00200000u;
        c.V0 = c.V0 | c.V1;
        MemoryAccess.WriteU32(m, (c.S0 + 0xCu), c.V0);
        c.RA = 0x8001B924u;
        GranTurismo2PC.func_8007DA80(c, m);
        c.S1 = 0x80050000u;
        c.S1 = c.S1 - 0x3CE4u;
        c.A0 = c.S1 + 0u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x3CB8u;
        c.A2 = c.S5 + 0u;
        c.A3 = c.S4 + 0u;
        c.RA = 0x8001B944u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S0 + 0u;
        c.A2 = c.S6 - 0x76u;
        c.S3 = c.S3 + 0x2Cu;
        c.A3 = c.S3 + 0u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0x6Cu));
        c.S2 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S2);
        c.RA = 0x8001B968u;
        GranTurismo2PC.func_8006AC90(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6A40u;
        c.RA = 0x8001B978u;
        GranTurismo2PC.func_8007DA80(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x3CC0u;
        c.A2 = c.S5 + 0u;
        c.A3 = c.S4 + 0u;
        c.RA = 0x8001B990u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x5DEEu;
        c.A2 = c.S6 + 0x6Eu;
        c.A3 = c.S3 + 0u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S2);
        c.RA = 0x8001B9B0u;
        GranTurismo2PC.func_8006AE28(c, m);
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
    public static void func_8001B9D8(CpuContext c, IMemory m)
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
            goto L8001BA5C;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x80u), c.T0);
        c.T0 = 0x00000180u;
        MemoryAccess.WriteU32(m, (c.SP + 0x80u), c.T0);
        if (c.V1 == 0u) {
            c.S6 = c.T0 + 0u;
            goto L8001BA4C;
        }
        c.S6 = c.T0 + 0u;
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            c.A0 = c.SP + 0x20u;
            goto L8001BA54;
        }
        c.A0 = c.SP + 0x20u;
        goto L8001BA60;
        L8001BA4C: ;
        c.S6 = 0x00000080u;
        goto L8001BA5C;
        L8001BA54: ;
        c.T0 = 0x00000080u;
        MemoryAccess.WriteU32(m, (c.SP + 0x80u), c.T0);
        L8001BA5C: ;
        c.A0 = c.SP + 0x20u;
        L8001BA60: ;
        c.A1 = 0x0000001Eu;
        c.RA = 0x8001BA68u;
        GranTurismo2PC.func_8006AC68(c, m);
        c.V1 = 0xFF9F0000u;
        c.V1 = c.V1 | 0xFFFFu;
        c.S2 = c.SP + 0x20u;
        c.A0 = c.S2 + 0u;
        c.A1 = 0x801C0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S2 + 0xCu));
        c.A1 = c.A1 - 0x69E0u;
        MemoryAccess.WriteU32(m, (c.S2 + 0x10u), c.S0);
        c.V0 = c.V0 & c.V1;
        c.V1 = 0x00200000u;
        c.V0 = c.V0 | c.V1;
        MemoryAccess.WriteU32(m, (c.S2 + 0xCu), c.V0);
        c.RA = 0x8001BA9Cu;
        GranTurismo2PC.func_8007DA80(c, m);
        c.S3 = 0x80050000u;
        c.S3 = c.S3 - 0x3CE4u;
        c.A0 = c.S3 + 0u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x3CB8u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0xC0u));
        c.A3 = 0x00000080u;
        c.RA = 0x8001BABCu;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S2 + 0u;
        c.A2 = c.S4 - 0x76u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xC8u));
        c.A3 = c.S5 + 0x2Cu;
        MemoryAccess.WriteU32(m, (c.S2 + 0x14u), c.V0);
        c.T0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        c.RA = 0x8001BADCu;
        GranTurismo2PC.func_8006AC90(c, m);
        c.A0 = c.S2 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6A40u;
        c.RA = 0x8001BAECu;
        GranTurismo2PC.func_8007DA80(c, m);
        c.FP = c.S4 + 0x48u;
        c.S5 = c.S5 + 0x1Eu;
        c.A0 = c.S3 + 0u;
        c.S1 = 0x80050000u;
        c.S1 = c.S1 - 0x3CC0u;
        c.A1 = c.S1 + 0u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0xC0u));
        c.A3 = c.S6 + 0u;
        c.RA = 0x8001BB10u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S2 + 0u;
        c.S0 = 0x801C0000u;
        c.S0 = c.S0 - 0x5DFBu;
        c.A1 = c.S0 + 0u;
        c.A2 = c.FP + 0u;
        c.A3 = c.S5 + 0u;
        MemoryAccess.WriteU32(m, (c.S2 + 0x14u), c.V0);
        c.T0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        c.RA = 0x8001BB38u;
        GranTurismo2PC.func_8006AE28(c, m);
        c.A0 = c.S3 + 0u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0xC0u));
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0x80u));
        c.A1 = c.S1 + 0u;
        c.RA = 0x8001BB4Cu;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S2 + 0u;
        c.A1 = c.S0 + 0x5u;
        c.A2 = c.FP + 0u;
        c.S7 = c.S5 + 0x14u;
        c.A3 = c.S7 + 0u;
        MemoryAccess.WriteU32(m, (c.S2 + 0x14u), c.V0);
        c.T0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        c.RA = 0x8001BB70u;
        GranTurismo2PC.func_8006AE28(c, m);
        c.FP = c.S4 + 0x6Eu;
        c.S4 = c.SP + 0x40u;
        c.A0 = c.S4 + 0u;
        c.S1 = 0x80020000u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xB0u));
        c.S1 = c.S1 + 0x2D7Cu;
        c.A2 = MemoryAccess.ReadU32(m, c.T0);
        c.A1 = c.S1 + 0u;
        c.RA = 0x8001BB94u;
        GranTurismo2PC.func_8008CF34(c, m);
        c.A0 = c.S3 + 0u;
        c.S0 = 0x80050000u;
        c.S0 = c.S0 - 0x3CBCu;
        c.A1 = c.S0 + 0u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0xC0u));
        c.A3 = c.S6 + 0u;
        c.RA = 0x8001BBB0u;
        GranTurismo2PC.func_8006B548(c, m);
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
        c.RA = 0x8001BBDCu;
        GranTurismo2PC.func_8006B184(c, m);
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xB0u));
        c.A0 = c.S4 + 0u;
        c.A2 = MemoryAccess.ReadU32(m, (c.T0 + 0x4u));
        c.A1 = c.S1 + 0u;
        c.RA = 0x8001BBF0u;
        GranTurismo2PC.func_8008CF34(c, m);
        c.A0 = c.S3 + 0u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0xC0u));
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0x80u));
        c.A1 = c.S0 + 0u;
        c.RA = 0x8001BC04u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S2 + 0u;
        c.A1 = c.S4 + 0u;
        c.A2 = c.FP + 0u;
        c.A3 = c.S7 + 0u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.V0);
        c.T0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.RA = 0x8001BC2Cu;
        GranTurismo2PC.func_8006B184(c, m);
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
    public static void func_8001BC5C(CpuContext c, IMemory m)
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
        c.RA = 0x8001BCDCu;
        GranTurismo2PC.func_8001B9D8(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x3CE4u;
        c.S0 = 0x80050000u;
        c.S0 = c.S0 - 0x3CD8u;
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
        c.RA = 0x8001BD1Cu;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.S2 = c.SP + 0x20u;
        c.A2 = c.S2 + 0u;
        c.RA = 0x8001BD30u;
        GranTurismo2PC.func_8007E780(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x3CE4u;
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
        c.RA = 0x8001BD68u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.A2 = c.S2 + 0u;
        c.RA = 0x8001BD78u;
        GranTurismo2PC.func_8007E780(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = 0x00000020u;
        c.RA = 0x8001BD84u;
        GranTurismo2PC.func_8007DA44(c, m);
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x78u));
        if ((int)c.T1 < 0) {
            c.A0 = 0x80050000u;
            goto L8001BDFC;
        }
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x3CE0u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x3CC4u;
        c.A2 = c.S6 + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x8001BDACu;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001BDB8u;
        GranTurismo2PC.func_8007D024(c, m);
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
        c.RA = 0x8001BDFCu;
        GranTurismo2PC.func_8006B988(c, m);
        L8001BDFC: ;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x3CE4u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x3CD4u;
        c.A2 = c.S6 + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x8001BE18u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001BE24u;
        GranTurismo2PC.func_8007D024(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x3CE4u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x3CD0u;
        c.A2 = c.S6 + 0u;
        c.A3 = 0x00000080u;
        c.V0 = c.S3 - c.S5;
        c.S0 = 0x0000000Cu;
        MemoryAccess.WriteU16(m, c.V1, (ushort)c.S5);
        MemoryAccess.WriteU16(m, (c.V1 + 0x4u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x2u), (ushort)c.S7);
        MemoryAccess.WriteU16(m, (c.V1 + 0x6u), (ushort)c.S0);
        c.RA = 0x8001BE5Cu;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001BE68u;
        GranTurismo2PC.func_8007D024(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x3CE4u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x3CDCu;
        c.A2 = c.S6 + 0u;
        MemoryAccess.WriteU16(m, c.V1, (ushort)c.S3);
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x30u));
        c.A3 = 0x00000080u;
        c.V0 = c.T1 - c.S3;
        MemoryAccess.WriteU16(m, (c.V1 + 0x4u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x2u), (ushort)c.S7);
        MemoryAccess.WriteU16(m, (c.V1 + 0x6u), (ushort)c.S0);
        c.RA = 0x8001BEA0u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001BEACu;
        GranTurismo2PC.func_8007D024(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = c.S4 + 0u;
        c.A1 = 0u + 0u;
        c.V0 = 0x00000100u;
        MemoryAccess.WriteU16(m, c.V1, (ushort)c.S5);
        MemoryAccess.WriteU16(m, (c.V1 + 0x4u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x2u), (ushort)c.S7);
        MemoryAccess.WriteU16(m, (c.V1 + 0x6u), (ushort)c.S0);
        c.RA = 0x8001BED0u;
        GranTurismo2PC.func_8007DA44(c, m);
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
    public static void func_8001BF00(CpuContext c, IMemory m)
    {
        c.V1 = 0x801D0000u;
        c.V1 = c.V1 - 0x6720u;
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
    public static void func_8001BF2C(CpuContext c, IMemory m)
    {
        c.V1 = 0x801D0000u;
        c.V1 = c.V1 - 0x6720u;
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
    public static void func_8001BF58(CpuContext c, IMemory m)
    {
        c.V1 = 0x801D0000u;
        c.V1 = c.V1 - 0x6720u;
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
    public static void func_8001BF84(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.RA = 0x8001BF94u;
        GranTurismo2PC.func_8001BF00(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = (int)c.V1 < 128 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = 0xFFFFFFFFu;
            goto L8001BFB8;
        }
        c.V0 = 0xFFFFFFFFu;
        c.A0 = c.V1 - 0x80u;
        c.V1 = c.A0 < 0x00000004u ? 1u : 0u;
        if (c.V1 == 0u) {
            goto L8001BFB8;
        }
        c.V0 = c.A0 + 0u;
        L8001BFB8: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001BFC8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.RA = 0x8001BFD8u;
        GranTurismo2PC.func_8001BF2C(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = (int)c.V1 < 128 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = 0xFFFFFFFFu;
            goto L8001BFFC;
        }
        c.V0 = 0xFFFFFFFFu;
        c.A0 = c.V1 - 0x80u;
        c.V1 = c.A0 < 0x00000004u ? 1u : 0u;
        if (c.V1 == 0u) {
            goto L8001BFFC;
        }
        c.V0 = c.A0 + 0u;
        L8001BFFC: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001C00C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.RA = 0x8001C01Cu;
        GranTurismo2PC.func_8001BF58(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = (int)c.V1 < 128 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = 0xFFFFFFFFu;
            goto L8001C040;
        }
        c.V0 = 0xFFFFFFFFu;
        c.A0 = c.V1 - 0x80u;
        c.V1 = c.A0 < 0x00000004u ? 1u : 0u;
        if (c.V1 == 0u) {
            goto L8001C040;
        }
        c.V0 = c.A0 + 0u;
        L8001C040: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001C050(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V1 = 0x800B0000u;
        c.V0 = c.A0 << 1;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.A0;
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 - 0x7298u));
        c.V0 = c.V0 << 2;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V0 = c.V0 + c.V1;
        c.V1 = MemoryAccess.ReadU8(m, (c.V0 + 0xDEu));
        c.V0 = 0x00000002u;
        if (c.V1 != c.V0) {
            goto L8001C09C;
        }
        c.RA = 0x8001C090u;
        GranTurismo2PC.func_8001BF00(c, m);
        c.V0 = (int)c.V0 < 128 ? 1u : 0u;
        c.V0 = c.V0 ^ 0x0001u;
        goto L8001C0A0;
        L8001C09C: ;
        c.V0 = 0u + 0u;
        L8001C0A0: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001C0B0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V1 = 0x800B0000u;
        c.V0 = c.A0 << 1;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.A0;
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 - 0x7298u));
        c.V0 = c.V0 << 2;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V0 = c.V0 + c.V1;
        c.V1 = MemoryAccess.ReadU8(m, (c.V0 + 0xDEu));
        c.V0 = 0x00000002u;
        if (c.V1 != c.V0) {
            goto L8001C0FC;
        }
        c.RA = 0x8001C0F0u;
        GranTurismo2PC.func_8001BF2C(c, m);
        c.V0 = (int)c.V0 < 128 ? 1u : 0u;
        c.V0 = c.V0 ^ 0x0001u;
        goto L8001C100;
        L8001C0FC: ;
        c.V0 = 0u + 0u;
        L8001C100: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001C110(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V1 = 0x800B0000u;
        c.V0 = c.A0 << 1;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.A0;
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 - 0x7298u));
        c.V0 = c.V0 << 2;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V0 = c.V0 + c.V1;
        c.V1 = MemoryAccess.ReadU8(m, (c.V0 + 0xDEu));
        c.V0 = 0x00000002u;
        if (c.V1 != c.V0) {
            goto L8001C15C;
        }
        c.RA = 0x8001C150u;
        GranTurismo2PC.func_8001BF58(c, m);
        c.V0 = (int)c.V0 < 128 ? 1u : 0u;
        c.V0 = c.V0 ^ 0x0001u;
        goto L8001C160;
        L8001C15C: ;
        c.V0 = 0u + 0u;
        L8001C160: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001C170(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        if (c.A2 != 0u) {
            c.V1 = c.A0 + 0u;
            goto L8001C1B8;
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
        c.RA = 0x8001C1B8u;
        GranTurismo2PC.func_8001ADFC(c, m);
        L8001C1B8: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001C1C8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V1 = c.A0 + 0u;
        c.A2 = c.A2 - 0x1u;
        c.V0 = c.A2 < 0x00000003u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A0 = c.A1 + 0u;
            goto L8001C200;
        }
        c.A0 = c.A1 + 0u;
        c.V0 = c.A2 << 2;
        c.V0 = c.V1 + c.V0;
        c.V1 = MemoryAccess.ReadU16(m, (c.V0 + 0xAu));
        c.V0 = MemoryAccess.ReadU16(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU32(m, c.A0, c.V1);
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.V0);
        c.RA = 0x8001C200u;
        GranTurismo2PC.func_8001B7F8(c, m);
        L8001C200: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001C210(CpuContext c, IMemory m)
    {
        if (c.A2 != 0u) {
            goto L8001C268;
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
        L8001C268: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001C270(CpuContext c, IMemory m)
    {
        c.A2 = c.A2 - 0x1u;
        c.V0 = c.A2 < 0x00000003u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.A2 << 2;
            goto L8001C298;
        }
        c.V0 = c.A2 << 2;
        c.V1 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.V0 = c.A0 + c.V0;
        MemoryAccess.WriteU16(m, (c.V0 + 0x8u), (ushort)c.V1);
        c.V1 = MemoryAccess.ReadU32(m, c.A1);
        MemoryAccess.WriteU16(m, (c.V0 + 0xAu), (ushort)c.V1);
        L8001C298: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001C2A0(CpuContext c, IMemory m)
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
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 - 0x7298u));
        c.V0 = c.V0 << 2;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.S3 + 0x1494u), 0u);
        MemoryAccess.WriteU32(m, (c.S4 + 0x1498u), 0u);
        MemoryAccess.WriteU32(m, (c.S5 + 0x149Cu), 0u);
        c.V0 = c.V0 + c.V1;
        c.S1 = MemoryAccess.ReadU8(m, (c.V0 + 0xDEu));
        c.V0 = 0x00000002u;
        if (c.S1 == c.V0) {
            c.S0 = 0u + 0u;
            goto L8001C31C;
        }
        c.S0 = 0u + 0u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1490u), 0u);
        c.V0 = c.S0 + 0u;
        goto L8001C468;
        L8001C31C: ;
        c.A0 = c.S2 + 0u;
        c.RA = 0x8001C324u;
        GranTurismo2PC.func_8001C050(c, m);
        if (c.V0 == 0u) {
            c.V1 = 0x800B0000u;
            goto L8001C380;
        }
        c.V1 = 0x800B0000u;
        c.S0 = 0x00000001u;
        c.A0 = c.V1 + 0x1410u;
        c.A1 = c.A0 + 0x8u;
        MemoryAccess.WriteU16(m, (c.A1 + 0x2u), (ushort)c.S0);
        c.S0 = 0x00000002u;
        c.A2 = c.A0 + 0x10u;
        MemoryAccess.WriteU16(m, (c.A2 + 0x2u), (ushort)c.S0);
        c.S0 = 0x00000003u;
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.S3 + 0x1494u), c.V0);
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x5D61u;
        MemoryAccess.WriteU16(m, (c.V1 + 0x1410u), (ushort)0u);
        c.V1 = c.V0 + 0x21u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.V0);
        c.V0 = c.V0 + 0x50u;
        MemoryAccess.WriteU16(m, (c.A0 + 0x2u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.A0 + 0x8u), (ushort)0u);
        MemoryAccess.WriteU32(m, (c.A1 + 0x4u), c.V1);
        MemoryAccess.WriteU16(m, (c.A0 + 0x10u), (ushort)0u);
        MemoryAccess.WriteU32(m, (c.A2 + 0x4u), c.V0);
        L8001C380: ;
        c.A0 = c.S2 + 0u;
        c.RA = 0x8001C388u;
        GranTurismo2PC.func_8001C0B0(c, m);
        if (c.V0 == 0u) {
            c.A0 = c.S0 << 3;
            goto L8001C3D8;
        }
        c.A0 = c.S0 << 3;
        c.S0 = c.S0 + 0x1u;
        c.A2 = c.S0 << 3;
        c.S0 = c.S0 + 0x1u;
        c.A1 = 0x00000001u;
        c.V1 = 0x800B0000u;
        c.V1 = c.V1 + 0x1410u;
        c.A0 = c.A0 + c.V1;
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x5CF1u;
        c.A2 = c.A2 + c.V1;
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.V0);
        c.V0 = c.V0 + 0x31u;
        MemoryAccess.WriteU32(m, (c.S4 + 0x1498u), c.A1);
        MemoryAccess.WriteU16(m, c.A0, (ushort)c.A1);
        MemoryAccess.WriteU16(m, (c.A0 + 0x2u), (ushort)0u);
        MemoryAccess.WriteU16(m, c.A2, (ushort)c.A1);
        MemoryAccess.WriteU16(m, (c.A2 + 0x2u), (ushort)c.A1);
        MemoryAccess.WriteU32(m, (c.A2 + 0x4u), c.V0);
        L8001C3D8: ;
        c.A0 = c.S2 + 0u;
        c.RA = 0x8001C3E0u;
        GranTurismo2PC.func_8001C110(c, m);
        if (c.V0 == 0u) {
            c.A0 = c.S0 << 3;
            goto L8001C430;
        }
        c.A0 = c.S0 << 3;
        c.S0 = c.S0 + 0x1u;
        c.A1 = c.S0 << 3;
        c.S0 = c.S0 + 0x1u;
        c.A2 = 0x00000001u;
        c.V1 = 0x800B0000u;
        c.V1 = c.V1 + 0x1410u;
        c.A0 = c.A0 + c.V1;
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x5C9Cu;
        c.A1 = c.A1 + c.V1;
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.V0);
        c.V0 = c.V0 + 0x2Bu;
        MemoryAccess.WriteU32(m, (c.S5 + 0x149Cu), c.A2);
        MemoryAccess.WriteU16(m, c.A0, (ushort)c.S1);
        MemoryAccess.WriteU16(m, (c.A0 + 0x2u), (ushort)0u);
        MemoryAccess.WriteU16(m, c.A1, (ushort)c.S1);
        MemoryAccess.WriteU16(m, (c.A1 + 0x2u), (ushort)c.A2);
        MemoryAccess.WriteU32(m, (c.A1 + 0x4u), c.V0);
        L8001C430: ;
        c.A0 = c.S0 << 3;
        c.S0 = c.S0 + 0x1u;
        c.V0 = (int)c.S0 < 2 ? 1u : 0u;
        c.V0 = c.V0 ^ 0x0001u;
        c.V1 = 0x800B0000u;
        c.V1 = c.V1 + 0x1410u;
        c.A0 = c.A0 + c.V1;
        c.V1 = 0x00000003u;
        MemoryAccess.WriteU16(m, c.A0, (ushort)c.V1);
        c.V1 = 0x801C0000u;
        c.V1 = c.V1 - 0x5D6Eu;
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.V1);
        c.V1 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x1490u), c.S0);
        L8001C468: ;
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
    public static void func_8001C48C(CpuContext c, IMemory m)
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
        c.V0 = c.V0 - 0x6720u;
        c.S0 = c.S1 << 2;
        c.S0 = c.S0 + c.S1;
        c.S0 = c.S0 << 3;
        c.S0 = c.S0 + c.S1;
        c.S0 = c.S0 << 1;
        c.S0 = c.S0 + c.V0;
        c.S0 = c.S0 + 0x48u;
        MemoryAccess.WriteU16(m, (c.S2 + 0x2u), (ushort)c.S1);
        c.RA = 0x8001C4E0u;
        GranTurismo2PC.func_8001BF84(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S2 + 0x8u;
        c.A2 = c.V0 + 0u;
        c.RA = 0x8001C4F0u;
        GranTurismo2PC.func_8001C170(c, m);
        c.A0 = c.S1 + 0u;
        c.RA = 0x8001C4F8u;
        GranTurismo2PC.func_8001BFC8(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S2 + 0x1Cu;
        c.A2 = c.V0 + 0u;
        c.RA = 0x8001C508u;
        GranTurismo2PC.func_8001C1C8(c, m);
        c.A0 = c.S1 + 0u;
        c.RA = 0x8001C510u;
        GranTurismo2PC.func_8001C00C(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S2 + 0x24u;
        c.A2 = c.V0 + 0u;
        c.RA = 0x8001C520u;
        GranTurismo2PC.func_8001C1C8(c, m);
        c.V0 = 0x80050000u;
        c.T1 = c.V0 - 0x3CB0u;
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
        c.T1 = c.V0 - 0x3C94u;
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
        c.T1 = c.V0 - 0x3C78u;
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
    public static void func_8001C610(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x2u));
        c.RA = 0x8001C62Cu;
        GranTurismo2PC.func_8001C48C(c, m);
        MemoryAccess.WriteU16(m, (c.S0 + 0x44u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.S0 + 0x60u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.S0 + 0x7Cu), (ushort)0u);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001C648(CpuContext c, IMemory m)
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
    public static void func_8001C684(CpuContext c, IMemory m)
    {
        c.V0 = 0x00000003u;
        MemoryAccess.WriteU16(m, (c.A0 + 0x4u), (ushort)c.V0);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001C690(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x2u));
        c.RA = 0x8001C6B0u;
        GranTurismo2PC.func_8001C2A0(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 != 0u) {
            c.V0 = 0u + 0u;
            goto L8001C7A4;
        }
        c.V0 = 0u + 0u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x14A0u), (ushort)0u);
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x14A2u), (ushort)0u);
        c.V0 = 0x800B0000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1494u));
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU16(m, c.S1, (ushort)0u);
        if (c.V1 == 0u) {
            MemoryAccess.WriteU16(m, (c.V0 + 0x14A4u), (ushort)0u);
            goto L8001C6FC;
        }
        MemoryAccess.WriteU16(m, (c.V0 + 0x14A4u), (ushort)0u);
        c.V1 = 0x800B0000u;
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x5DDEu;
        MemoryAccess.WriteU16(m, (c.S1 + 0x4u), (ushort)0u);
        MemoryAccess.WriteU32(m, (c.V1 + 0x1408u), c.V0);
        goto L8001C724;
        L8001C6FC: ;
        c.V0 = 0x00000002u;
        c.V1 = 0x800B0000u;
        MemoryAccess.WriteU16(m, (c.S1 + 0x4u), (ushort)c.V0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.S1);
        c.V1 = c.V1 + 0x1410u;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.V1;
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1408u), c.V1);
        L8001C724: ;
        c.RA = 0x8001C72Cu;
        GranTurismo2PC.func_8001AC78(c, m);
        c.V0 = 0x801D0000u;
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x2u));
        c.V0 = c.V0 - 0x6720u;
        c.S0 = c.A0 << 2;
        c.S0 = c.S0 + c.A0;
        c.S0 = c.S0 << 3;
        c.S0 = c.S0 + c.A0;
        c.S0 = c.S0 << 1;
        c.S0 = c.S0 + c.V0;
        c.S0 = c.S0 + 0x48u;
        c.RA = 0x8001C758u;
        GranTurismo2PC.func_8001BF84(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0x8u;
        c.A2 = c.V0 + 0u;
        c.RA = 0x8001C768u;
        GranTurismo2PC.func_8001C170(c, m);
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x2u));
        c.RA = 0x8001C774u;
        GranTurismo2PC.func_8001BFC8(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0x1Cu;
        c.A2 = c.V0 + 0u;
        c.RA = 0x8001C784u;
        GranTurismo2PC.func_8001C1C8(c, m);
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x2u));
        c.RA = 0x8001C790u;
        GranTurismo2PC.func_8001C00C(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0x24u;
        c.A2 = c.V0 + 0u;
        c.RA = 0x8001C7A0u;
        GranTurismo2PC.func_8001C1C8(c, m);
        c.V0 = 0x00000001u;
        L8001C7A4: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001C7B8(CpuContext c, IMemory m)
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
        c.A1 = MemoryAccess.ReadU32(m, (c.V1 - 0x7298u));
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
        c.V0 = c.V0 + 0xC70u;
        c.V1 = c.V1 + c.A2;
        c.V1 = c.V1 << (int)(c.S4 & 31u);
        c.V1 = c.V1 + 0x28u;
        c.S0 = c.V1 + c.V0;
        c.RA = 0x8001C84Cu;
        GranTurismo2PC.func_8006BE64(c, m);
        c.A0 = c.S1 + 0x48u;
        c.RA = 0x8001C854u;
        GranTurismo2PC.func_8006BE64(c, m);
        c.A0 = c.S1 + 0x64u;
        c.RA = 0x8001C85Cu;
        GranTurismo2PC.func_8006BE64(c, m);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x4u));
        c.S5 = 0x00000003u;
        if (c.V0 == c.S5) {
            c.S7 = c.S6 + 0u;
            goto L8001CDF8;
        }
        c.S7 = c.S6 + 0u;
        c.S2 = MemoryAccess.ReadU8(m, (c.S2 + 0x2u));
        c.V0 = 0x00000002u;
        if (c.S2 != c.V0) {
            c.A0 = 0u + 0u;
            goto L8001C9BC;
        }
        c.A0 = 0u + 0u;
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x2u));
        c.RA = 0x8001C888u;
        GranTurismo2PC.func_8001BF84(c, m);
        if ((int)c.V0 < 0) {
            c.V0 = c.V0 + c.S0;
            goto L8001C89C;
        }
        c.V0 = c.V0 + c.S0;
        c.V1 = MemoryAccess.ReadU8(m, (c.V0 + 0x4u));
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x14A0u), (ushort)c.V1);
        L8001C89C: ;
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x2u));
        c.RA = 0x8001C8A8u;
        GranTurismo2PC.func_8001BFC8(c, m);
        if ((int)c.V0 < 0) {
            c.V0 = c.V0 + c.S0;
            goto L8001C8BC;
        }
        c.V0 = c.V0 + c.S0;
        c.V1 = MemoryAccess.ReadU8(m, (c.V0 + 0x4u));
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x14A2u), (ushort)c.V1);
        L8001C8BC: ;
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x2u));
        c.RA = 0x8001C8C8u;
        GranTurismo2PC.func_8001C00C(c, m);
        if ((int)c.V0 < 0) {
            c.V0 = c.V0 + c.S0;
            goto L8001C8DC;
        }
        c.V0 = c.V0 + c.S0;
        c.V1 = MemoryAccess.ReadU8(m, (c.V0 + 0x4u));
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x14A4u), (ushort)c.V1);
        L8001C8DC: ;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x4u));
        if (c.V1 == c.S4) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L8001C968;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L8001C904;
        }
        if (c.V1 == 0u) {
            c.V0 = 0xFFFFFFFFu;
            goto L8001C914;
        }
        c.V0 = 0xFFFFFFFFu;
        goto L8001CDFC;
        L8001C904: ;
        if (c.V1 == c.S2) {
            c.V0 = 0xFFFFFFFFu;
            goto L8001C9D0;
        }
        c.V0 = 0xFFFFFFFFu;
        goto L8001CDFC;
        L8001C914: ;
        c.V0 = 0x00010000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.S3 + 0x4u));
        c.V1 = MemoryAccess.ReadU32(m, (c.S6 + 0x4u));
        c.V0 = c.V0 | 0x1000u;
        c.A0 = c.A0 | c.V1;
        c.V0 = c.A0 & c.V0;
        if (c.V0 == 0u) {
            c.V1 = 0x800B0000u;
            goto L8001C9B0;
        }
        c.V1 = 0x800B0000u;
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x5DA7u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x1408u), c.V0);
        c.V0 = 0x800B0000u;
        c.V1 = MemoryAccess.ReadU16(m, (c.V0 + 0x14A0u));
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU16(m, (c.S1 + 0x4u), (ushort)c.S4);
        MemoryAccess.WriteU32(m, (c.V0 + 0x14A8u), c.V1);
        c.RA = 0x8001C958u;
        GranTurismo2PC.func_8001AC78(c, m);
        c.A0 = 0x00000001u;
        c.RA = 0x8001C960u;
        GranTurismo2PC.func_80060840(c, m);
        c.V0 = 0xFFFFFFFFu;
        goto L8001CDFC;
        L8001C968: ;
        c.V0 = 0x00010000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.S3 + 0x4u));
        c.V1 = MemoryAccess.ReadU32(m, (c.S7 + 0x4u));
        c.V0 = c.V0 | 0x1000u;
        c.A0 = c.A0 | c.V1;
        c.V0 = c.A0 & c.V0;
        if (c.V0 == 0u) {
            c.V0 = 0x800B0000u;
            goto L8001C9B0;
        }
        c.V0 = 0x800B0000u;
        c.A0 = c.S1 + 0x8u;
        c.V1 = 0x800B0000u;
        c.A2 = MemoryAccess.ReadU16(m, (c.V0 + 0x14A0u));
        c.A1 = MemoryAccess.ReadU32(m, (c.V1 + 0x14A8u));
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V0 + 0x14ACu), c.A2);
        c.RA = 0x8001C9A4u;
        GranTurismo2PC.func_8001ADFC(c, m);
        c.A0 = 0x00000001u;
        MemoryAccess.WriteU16(m, (c.S1 + 0x4u), (ushort)c.S2);
        goto L8001CDCC;
        L8001C9B0: ;
        c.V0 = c.A0 & 0x0500u;
        if (c.V0 == 0u) {
            c.A0 = 0x00000002u;
            goto L8001CDF8;
        }
        c.A0 = 0x00000002u;
        L8001C9BC: ;
        c.RA = 0x8001C9C4u;
        GranTurismo2PC.func_80060840(c, m);
        MemoryAccess.WriteU16(m, (c.S1 + 0x4u), (ushort)c.S5);
        c.V0 = 0xFFFFFFFEu;
        goto L8001CDFC;
        L8001C9D0: ;
        c.V1 = 0x800B0000u;
        c.S2 = (uint)(short)MemoryAccess.ReadU16(m, c.S1);
        c.V1 = c.V1 + 0x1410u;
        c.V0 = c.S2 << 3;
        c.V0 = c.V0 + c.V1;
        c.S4 = (uint)(short)MemoryAccess.ReadU16(m, c.V0);
        c.V1 = MemoryAccess.ReadU32(m, (c.S3 + 0x4u));
        if (c.S4 != c.S5) {
            c.S0 = 0u + 0u;
            goto L8001CA7C;
        }
        c.S0 = 0u + 0u;
        c.V0 = c.V1 & 0x0A00u;
        if (c.V0 == 0u) {
            c.V0 = 0x801D0000u;
            goto L8001CA7C;
        }
        c.V0 = 0x801D0000u;
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x2u));
        c.V0 = c.V0 - 0x6720u;
        c.S0 = c.A0 << 2;
        c.S0 = c.S0 + c.A0;
        c.S0 = c.S0 << 3;
        c.S0 = c.S0 + c.A0;
        c.S0 = c.S0 << 1;
        c.S0 = c.S0 + c.V0;
        c.S0 = c.S0 + 0x48u;
        c.RA = 0x8001CA28u;
        GranTurismo2PC.func_8001BF84(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0x8u;
        c.A2 = c.V0 + 0u;
        c.RA = 0x8001CA38u;
        GranTurismo2PC.func_8001C210(c, m);
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x2u));
        c.RA = 0x8001CA44u;
        GranTurismo2PC.func_8001BFC8(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0x1Cu;
        c.A2 = c.V0 + 0u;
        c.RA = 0x8001CA54u;
        GranTurismo2PC.func_8001C270(c, m);
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x2u));
        c.RA = 0x8001CA60u;
        GranTurismo2PC.func_8001C00C(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0x24u;
        c.A2 = c.V0 + 0u;
        c.RA = 0x8001CA70u;
        GranTurismo2PC.func_8001C270(c, m);
        c.V0 = 0u + 0u;
        MemoryAccess.WriteU16(m, (c.S1 + 0x4u), (ushort)c.S4);
        goto L8001CDFC;
        L8001CA7C: ;
        c.V0 = 0x00010000u;
        c.V0 = c.V0 | 0x1000u;
        c.V0 = c.V1 & c.V0;
        if (c.V0 == 0u) {
            c.V1 = 0x800B0000u;
            goto L8001CBF0;
        }
        c.V1 = 0x800B0000u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.S1);
        c.V1 = c.V1 + 0x1410u;
        c.V0 = c.V0 << 3;
        c.A2 = c.V0 + c.V1;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, c.A2);
        c.T0 = 0x00000001u;
        if (c.V1 == c.T0) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L8001CB5C;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L8001CAC8;
        }
        c.V0 = 0x00000002u;
        if (c.V1 == 0u) {
            c.V0 = 0x800B0000u;
            goto L8001CAD8;
        }
        c.V0 = 0x800B0000u;
        goto L8001CBF0;
        L8001CAC8: ;
        if (c.V1 == c.V0) {
            goto L8001CBA0;
        }
        goto L8001CBF0;
        L8001CAD8: ;
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x8u));
        c.A3 = MemoryAccess.ReadU16(m, (c.V0 + 0x14A0u));
        c.A1 = c.A3 - c.A1;
        if ((int)c.A1 >= 0) {
            c.A0 = c.S1 + 0x8u;
            goto L8001CAF4;
        }
        c.A0 = c.S1 + 0x8u;
        c.A1 = 0u - c.A1;
        L8001CAF4: ;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.A2 + 0x2u));
        if (c.V1 == c.T0) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L8001CB3C;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L8001CB1C;
        }
        c.V0 = 0x00000002u;
        if (c.V1 == 0u) {
            goto L8001CB2C;
        }
        goto L8001CBE8;
        L8001CB1C: ;
        if (c.V1 == c.V0) {
            goto L8001CB4C;
        }
        goto L8001CBE8;
        L8001CB2C: ;
        c.A1 = c.A3 + 0u;
        c.RA = 0x8001CB34u;
        GranTurismo2PC.func_8001ADDC(c, m);
        goto L8001CBE8;
        L8001CB3C: ;
        c.RA = 0x8001CB44u;
        GranTurismo2PC.func_8001ADBC(c, m);
        goto L8001CBE8;
        L8001CB4C: ;
        c.RA = 0x8001CB54u;
        GranTurismo2PC.func_8001AD9C(c, m);
        goto L8001CBE8;
        L8001CB5C: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A2 + 0x2u));
        if (c.V0 == 0u) {
            goto L8001CB7C;
        }
        if (c.V0 == c.T0) {
            c.V0 = 0x800B0000u;
            goto L8001CB94;
        }
        c.V0 = 0x800B0000u;
        goto L8001CBE8;
        L8001CB7C: ;
        c.V0 = 0x800B0000u;
        c.A1 = MemoryAccess.ReadU16(m, (c.V0 + 0x14A2u));
        c.A0 = c.S1 + 0x1Cu;
        c.RA = 0x8001CB8Cu;
        GranTurismo2PC.func_8001B880(c, m);
        goto L8001CBE8;
        L8001CB94: ;
        c.A1 = MemoryAccess.ReadU16(m, (c.V0 + 0x14A2u));
        c.A0 = c.S1 + 0x1Cu;
        goto L8001CBE0;
        L8001CBA0: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A2 + 0x2u));
        if (c.V0 == 0u) {
            goto L8001CBC0;
        }
        if (c.V0 == c.T0) {
            c.V0 = 0x800B0000u;
            goto L8001CBD8;
        }
        c.V0 = 0x800B0000u;
        goto L8001CBE8;
        L8001CBC0: ;
        c.V0 = 0x800B0000u;
        c.A1 = MemoryAccess.ReadU16(m, (c.V0 + 0x14A4u));
        c.A0 = c.S1 + 0x24u;
        c.RA = 0x8001CBD0u;
        GranTurismo2PC.func_8001B880(c, m);
        goto L8001CBE8;
        L8001CBD8: ;
        c.A1 = MemoryAccess.ReadU16(m, (c.V0 + 0x14A4u));
        c.A0 = c.S1 + 0x24u;
        L8001CBE0: ;
        c.RA = 0x8001CBE8u;
        GranTurismo2PC.func_8001B860(c, m);
        L8001CBE8: ;
        c.A0 = 0x00000001u;
        c.RA = 0x8001CBF0u;
        GranTurismo2PC.func_80060840(c, m);
        L8001CBF0: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S3 + 0x4u));
        c.V1 = MemoryAccess.ReadU32(m, (c.S3 + 0xCu));
        c.V1 = c.V0 | c.V1;
        c.V0 = c.V1 & 0x0004u;
        if (c.V0 == 0u) {
            c.V0 = c.V1 & 0x0008u;
            goto L8001CC10;
        }
        c.V0 = c.V1 & 0x0008u;
        c.S0 = c.S0 - 0x1u;
        L8001CC10: ;
        if (c.V0 == 0u) {
            goto L8001CC1C;
        }
        c.S0 = c.S0 + 0x1u;
        L8001CC1C: ;
        if (c.S0 == 0u) {
            c.V1 = 0x800B0000u;
            goto L8001CD5C;
        }
        c.V1 = 0x800B0000u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.S1);
        c.V1 = c.V1 + 0x1410u;
        c.V0 = c.V0 << 3;
        c.A0 = c.V0 + c.V1;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, c.A0);
        c.A1 = 0x00000001u;
        if (c.V1 == c.A1) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L8001CCE4;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L8001CC5C;
        }
        c.V0 = 0x00000002u;
        if (c.V1 == 0u) {
            goto L8001CC6C;
        }
        goto L8001CD5C;
        L8001CC5C: ;
        if (c.V1 == c.V0) {
            goto L8001CD10;
        }
        goto L8001CD5C;
        L8001CC6C: ;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0x2u));
        if (c.V1 == c.A1) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L8001CCB8;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L8001CC94;
        }
        c.V0 = 0x00000002u;
        if (c.V1 == 0u) {
            c.A0 = c.S1 + 0x8u;
            goto L8001CCA4;
        }
        c.A0 = c.S1 + 0x8u;
        goto L8001CD54;
        L8001CC94: ;
        if (c.V1 == c.V0) {
            c.A0 = c.S1 + 0x8u;
            goto L8001CCD0;
        }
        c.A0 = c.S1 + 0x8u;
        goto L8001CD54;
        L8001CCA4: ;
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x8u));
        c.A1 = c.A1 + c.S0;
        c.RA = 0x8001CCB0u;
        GranTurismo2PC.func_8001ADDC(c, m);
        goto L8001CD54;
        L8001CCB8: ;
        c.A0 = c.S1 + 0x8u;
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0x4u));
        c.A1 = c.A1 + c.S0;
        c.RA = 0x8001CCC8u;
        GranTurismo2PC.func_8001ADBC(c, m);
        goto L8001CD54;
        L8001CCD0: ;
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.A1 = c.A1 + c.S0;
        c.RA = 0x8001CCDCu;
        GranTurismo2PC.func_8001AD9C(c, m);
        goto L8001CD54;
        L8001CCE4: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0x2u));
        if (c.V0 == 0u) {
            goto L8001CD04;
        }
        if (c.V0 == c.A1) {
            c.A0 = c.S1 + 0x1Cu;
            goto L8001CD48;
        }
        c.A0 = c.S1 + 0x1Cu;
        goto L8001CD54;
        L8001CD04: ;
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x1Cu));
        c.A0 = c.S1 + 0x1Cu;
        goto L8001CD38;
        L8001CD10: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0x2u));
        if (c.V0 == 0u) {
            goto L8001CD30;
        }
        if (c.V0 == c.A1) {
            c.A0 = c.S1 + 0x24u;
            goto L8001CD48;
        }
        c.A0 = c.S1 + 0x24u;
        goto L8001CD54;
        L8001CD30: ;
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x24u));
        c.A0 = c.S1 + 0x24u;
        L8001CD38: ;
        c.A1 = c.A1 + c.S0;
        c.RA = 0x8001CD40u;
        GranTurismo2PC.func_8001B880(c, m);
        goto L8001CD54;
        L8001CD48: ;
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0x4u));
        c.A1 = c.A1 + c.S0;
        c.RA = 0x8001CD54u;
        GranTurismo2PC.func_8001B860(c, m);
        L8001CD54: ;
        c.A0 = 0x00000005u;
        c.RA = 0x8001CD5Cu;
        GranTurismo2PC.func_80060840(c, m);
        L8001CD5C: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S3 + 0x4u));
        c.V1 = MemoryAccess.ReadU32(m, (c.S3 + 0xCu));
        c.V1 = c.V0 | c.V1;
        c.V0 = c.V1 & 0x0001u;
        if (c.V0 == 0u) {
            c.V0 = c.V1 & 0x0002u;
            goto L8001CD98;
        }
        c.V0 = c.V1 & 0x0002u;
        c.S2 = c.S2 - 0x1u;
        if ((int)c.S2 >= 0) {
            goto L8001CD98;
        }
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1490u));
        c.S2 = c.V0 - 0x1u;
        c.V0 = c.V1 & 0x0002u;
        L8001CD98: ;
        if (c.V0 == 0u) {
            c.V0 = 0x800B0000u;
            goto L8001CDB8;
        }
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1490u));
        c.S2 = c.S2 + 0x1u;
        c.V0 = (int)c.S2 < (int)c.V0 ? 1u : 0u;
        if (c.V0 != 0u) {
            goto L8001CDB8;
        }
        c.S2 = 0u + 0u;
        L8001CDB8: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.S1);
        if (c.V0 == c.S2) {
            c.A0 = 0x00000006u;
            goto L8001CDF8;
        }
        c.A0 = 0x00000006u;
        MemoryAccess.WriteU16(m, c.S1, (ushort)c.S2);
        L8001CDCC: ;
        c.RA = 0x8001CDD4u;
        GranTurismo2PC.func_80060840(c, m);
        c.V1 = 0x800B0000u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.S1);
        c.V1 = c.V1 + 0x1410u;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.V1;
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1408u), c.V1);
        c.RA = 0x8001CDF8u;
        GranTurismo2PC.func_8001AC78(c, m);
        L8001CDF8: ;
        c.V0 = 0xFFFFFFFFu;
        L8001CDFC: ;
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
    public static void func_8001CE28(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x98u;
        MemoryAccess.WriteU32(m, (c.SP + 0x78u), c.S2);
        c.S2 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x74u), c.S1);
        c.S1 = c.A1 + 0u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x84u), c.S5);
        c.S5 = MemoryAccess.ReadU16(m, (c.V0 + 0x14A0u));
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU16(m, (c.V0 + 0x14A2u));
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
        c.V0 = MemoryAccess.ReadU16(m, (c.V0 + 0x14A4u));
        c.A1 = 0x0000001Eu;
        MemoryAccess.WriteU32(m, (c.SP + 0x94u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x70u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0xA0u), c.A2);
        MemoryAccess.WriteU32(m, (c.SP + 0xA4u), c.A3);
        MemoryAccess.WriteU32(m, (c.SP + 0x5Cu), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x6Cu), c.T0);
        MemoryAccess.WriteU32(m, (c.SP + 0x64u), c.V0);
        c.RA = 0x8001CEC4u;
        GranTurismo2PC.func_8006AC68(c, m);
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
            goto L8001CF10;
        }
        c.V0 = (int)c.V1 < 3 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L8001CF8C;
        }
        if ((int)c.V1 < 0) {
            c.V1 = 0x00740000u;
            goto L8001CF8C;
        }
        c.V1 = 0x00740000u;
        c.V1 = c.V1 | 0x7474u;
        goto L8001CF68;
        L8001CF10: ;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6A40u;
        c.RA = 0x8001CF20u;
        GranTurismo2PC.func_8007DA80(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x3CE4u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x3CB4u;
        c.A2 = c.S7 + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x8001CF3Cu;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x5C50u;
        c.A2 = 0x000000B0u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x58u));
        c.A3 = 0x00000084u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        c.RA = 0x8001CF60u;
        GranTurismo2PC.func_8006ADB4(c, m);
        c.V1 = 0x00740000u;
        c.V1 = c.V1 | 0x7474u;
        L8001CF68: ;
        c.A0 = c.S1 + 0u;
        c.A2 = 0x000000B0u;
        c.V0 = 0x800B0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1408u));
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x58u));
        c.A3 = 0x000001AEu;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        c.RA = 0x8001CF8Cu;
        GranTurismo2PC.func_8001AB40(c, m);
        L8001CF8C: ;
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x4u));
        c.V0 = 0x00000001u;
        if (c.A1 == c.V0) {
            c.V0 = (int)c.A1 < 2 ? 1u : 0u;
            goto L8001CFE4;
        }
        c.V0 = (int)c.A1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L8001CFB4;
        }
        c.V0 = 0x00000002u;
        if (c.A1 == 0u) {
            goto L8001CFCC;
        }
        goto L8001D0B0;
        L8001CFB4: ;
        if (c.A1 == c.V0) {
            c.V0 = 0x00000003u;
            goto L8001D004;
        }
        c.V0 = 0x00000003u;
        if (c.A1 == c.V0) {
            c.T0 = 0xFFFFFFFFu;
            goto L8001D0A4;
        }
        c.T0 = 0xFFFFFFFFu;
        goto L8001D0B0;
        L8001CFCC: ;
        c.S6 = 0x00000002u;
        c.FP = c.S5 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x58u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x54u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x50u), 0u);
        goto L8001D0B0;
        L8001CFE4: ;
        c.V0 = 0x800B0000u;
        c.FP = MemoryAccess.ReadU32(m, (c.V0 + 0x14A8u));
        c.S6 = 0x00000002u;
        MemoryAccess.WriteU32(m, (c.SP + 0x58u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x54u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x50u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x68u), c.S5);
        goto L8001D0B0;
        L8001D004: ;
        c.T0 = 0x00000001u;
        c.V1 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x5Cu), c.T0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.V1 = c.V1 + 0x1410u;
        c.V0 = c.V0 << 3;
        c.A0 = c.V0 + c.V1;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, c.A0);
        if (c.V1 == c.T0) {
            c.S6 = c.T0 + 0u;
            goto L8001D070;
        }
        c.S6 = c.T0 + 0u;
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L8001D04C;
        }
        if (c.V1 == 0u) {
            goto L8001D064;
        }
        goto L8001D0B0;
        L8001D04C: ;
        if (c.V1 == c.A1) {
            c.V0 = 0x00000003u;
            goto L8001D07C;
        }
        c.V0 = 0x00000003u;
        if (c.V1 == c.V0) {
            goto L8001D090;
        }
        goto L8001D0B0;
        L8001D064: ;
        c.S3 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0x2u));
        c.S4 = 0x00000040u;
        goto L8001D098;
        L8001D070: ;
        c.S4 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0x2u));
        c.S3 = 0x00000040u;
        goto L8001D098;
        L8001D07C: ;
        c.S3 = 0x00000040u;
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0x2u));
        c.S4 = c.S3 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x6Cu), c.A0);
        goto L8001D0B0;
        L8001D090: ;
        c.S3 = 0x00000040u;
        c.S4 = c.S3 + 0u;
        L8001D098: ;
        c.T0 = 0x00000040u;
        MemoryAccess.WriteU32(m, (c.SP + 0x6Cu), c.T0);
        goto L8001D0B0;
        L8001D0A4: ;
        c.S5 = c.T0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x64u), c.T0);
        MemoryAccess.WriteU32(m, (c.SP + 0x60u), c.T0);
        L8001D0B0: ;
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x2u));
        c.RA = 0x8001D0BCu;
        GranTurismo2PC.func_8001C050(c, m);
        if (c.V0 == 0u) {
            c.A0 = c.S2 + 0x8u;
            goto L8001D104;
        }
        c.A0 = c.S2 + 0x8u;
        c.A1 = c.S1 + 0u;
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0xA0u));
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xA4u));
        c.A2 = 0x801C0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x68u));
        c.A2 = c.A2 - 0x5EF9u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.T0);
        c.RA = 0x8001D0FCu;
        GranTurismo2PC.func_8001B2C0(c, m);
        goto L8001D128;
        L8001D104: ;
        c.A1 = c.S1 + 0u;
        c.V0 = 0x801C0000u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0xA0u));
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0xA4u));
        c.V0 = c.V0 - 0x5EF9u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S3);
        c.RA = 0x8001D128u;
        GranTurismo2PC.func_8001AE34(c, m);
        L8001D128: ;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x50u));
        if (c.T0 == 0u) {
            c.A0 = c.S2 + 0x2Cu;
            goto L8001D160;
        }
        c.A0 = c.S2 + 0x2Cu;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xA0u));
        c.A2 = c.T0 - 0x7Au;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xA4u));
        c.A1 = c.S1 + 0u;
        c.A3 = c.T0 + 0x14u;
        c.RA = 0x8001D154u;
        GranTurismo2PC.func_8006BEF4(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = 0x00000220u;
        c.RA = 0x8001D160u;
        GranTurismo2PC.func_8007DA44(c, m);
        L8001D160: ;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x54u));
        if (c.T0 == 0u) {
            goto L8001D2A0;
        }
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x2u));
        c.RA = 0x8001D17Cu;
        GranTurismo2PC.func_8001C0B0(c, m);
        if (c.V0 == 0u) {
            c.A0 = c.S2 + 0x1Cu;
            goto L8001D250;
        }
        c.A0 = c.S2 + 0x1Cu;
        c.A1 = c.S1 + 0u;
        c.A2 = 0x801C0000u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xA4u));
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0xA0u));
        c.V0 = c.T0 + 0x60u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x60u));
        c.A2 = c.A2 - 0x5EEFu;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.T0);
        c.RA = 0x8001D1B4u;
        GranTurismo2PC.func_8001BC5C(c, m);
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x2u));
        c.RA = 0x8001D1C0u;
        GranTurismo2PC.func_8001BF2C(c, m);
        c.A0 = c.V0 + 0u;
        c.RA = 0x8001D1C8u;
        GranTurismo2PC.func_80019308(c, m);
        c.S0 = c.V0 + 0u;
        if (c.S0 == 0u) {
            c.A0 = c.S1 + 0u;
            goto L8001D278;
        }
        c.A0 = c.S1 + 0u;
        c.A1 = c.S7 << 8;
        c.A1 = c.S7 | c.A1;
        c.V0 = c.S7 << 16;
        c.A1 = c.A1 | c.V0;
        c.RA = 0x8001D1E8u;
        GranTurismo2PC.func_80081478(c, m);
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
        c.RA = 0x8001D248u;
        GranTurismo2PC.func_8007DA44(c, m);
        goto L8001D278;
        L8001D250: ;
        c.A1 = c.S1 + 0u;
        c.V0 = 0x801C0000u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0xA0u));
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xA4u));
        c.V0 = c.V0 - 0x5EEFu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S4);
        c.A3 = c.T0 + 0x60u;
        c.RA = 0x8001D278u;
        GranTurismo2PC.func_8001B8A0(c, m);
        L8001D278: ;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xA0u));
        c.A0 = c.S2 + 0x48u;
        c.A2 = c.T0 - 0x7Au;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xA4u));
        c.A1 = c.S1 + 0u;
        c.A3 = c.T0 + 0x74u;
        c.RA = 0x8001D294u;
        GranTurismo2PC.func_8006BEF4(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = 0x00000220u;
        c.RA = 0x8001D2A0u;
        GranTurismo2PC.func_8007DA44(c, m);
        L8001D2A0: ;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x58u));
        if (c.T0 == 0u) {
            goto L8001D3E8;
        }
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x2u));
        c.RA = 0x8001D2BCu;
        GranTurismo2PC.func_8001C110(c, m);
        if (c.V0 == 0u) {
            c.A0 = c.S2 + 0x24u;
            goto L8001D394;
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
        c.A2 = c.A2 - 0x5EE0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.T0);
        c.RA = 0x8001D2F8u;
        GranTurismo2PC.func_8001BC5C(c, m);
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x2u));
        c.RA = 0x8001D304u;
        GranTurismo2PC.func_8001BF58(c, m);
        c.A0 = c.V0 + 0u;
        c.RA = 0x8001D30Cu;
        GranTurismo2PC.func_80019308(c, m);
        c.S0 = c.V0 + 0u;
        if (c.S0 == 0u) {
            c.A0 = c.S1 + 0u;
            goto L8001D3C0;
        }
        c.A0 = c.S1 + 0u;
        c.A1 = c.S7 << 8;
        c.A1 = c.S7 | c.A1;
        c.V0 = c.S7 << 16;
        c.A1 = c.A1 | c.V0;
        c.RA = 0x8001D32Cu;
        GranTurismo2PC.func_80081478(c, m);
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
        c.RA = 0x8001D38Cu;
        GranTurismo2PC.func_8007DA44(c, m);
        goto L8001D3C0;
        L8001D394: ;
        c.A1 = c.S1 + 0u;
        c.V0 = 0x801C0000u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xA4u));
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0xA0u));
        c.A3 = c.T0 + 0xB4u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x6Cu));
        c.V0 = c.V0 - 0x5EE0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.T0);
        c.RA = 0x8001D3C0u;
        GranTurismo2PC.func_8001B8A0(c, m);
        L8001D3C0: ;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xA0u));
        c.A0 = c.S2 + 0x64u;
        c.A2 = c.T0 - 0x7Au;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xA4u));
        c.A1 = c.S1 + 0u;
        c.A3 = c.T0 + 0xC8u;
        c.RA = 0x8001D3DCu;
        GranTurismo2PC.func_8006BEF4(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = 0x00000220u;
        c.RA = 0x8001D3E8u;
        GranTurismo2PC.func_8007DA44(c, m);
        L8001D3E8: ;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x5Cu));
        if (c.T0 == 0u) {
            c.V1 = 0x800B0000u;
            goto L8001D480;
        }
        c.V1 = 0x800B0000u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.V1 = c.V1 + 0x1410u;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.V1;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, c.V0);
        c.V0 = 0x00000003u;
        if (c.V1 != c.V0) {
            c.S0 = 0x00000180u;
            goto L8001D41C;
        }
        c.S0 = 0x00000180u;
        c.S0 = 0x00000080u;
        L8001D41C: ;
        c.A0 = c.SP + 0x30u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6A40u;
        c.RA = 0x8001D42Cu;
        GranTurismo2PC.func_8007DA80(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x3CE4u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x3CC0u;
        c.A2 = c.S7 + 0u;
        c.A3 = c.S0 + 0u;
        c.RA = 0x8001D448u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.SP + 0x30u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xA0u));
        c.A1 = 0x801C0000u;
        c.A2 = c.T0 + 0x48u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xA4u));
        c.A1 = c.A1 - 0x5F12u;
        c.A3 = c.T0 - 0x10u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.V0);
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.V0 = 0xFFFFFFFEu;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.RA = 0x8001D480u;
        GranTurismo2PC.func_8006B184(c, m);
        L8001D480: ;
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
    public static void func_8001D4B0(CpuContext c, IMemory m)
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
    public static void func_8001D4D4(CpuContext c, IMemory m)
    {
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU16(m, (c.A0 + 0xAu), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.A0 + 0xCu), (ushort)0u);
        MemoryAccess.WriteU8(m, (c.A0 + 0xEu), (byte)c.V0);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001D4E8_gt2_overlay_1(CpuContext c, IMemory m)
    {
        c.V0 = 0xFFFFFFEFu;
        MemoryAccess.WriteU16(m, (c.A0 + 0xAu), (ushort)c.V0);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001D4F4(CpuContext c, IMemory m)
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
            goto L8001D550;
        }
        c.S3 = 0xFFFFFFFEu;
        c.V0 = (int)c.V0 < -1 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.V1 + 0x1u;
            goto L8001D5FC;
        }
        c.V0 = c.V1 + 0x1u;
        MemoryAccess.WriteU16(m, (c.S1 + 0xAu), (ushort)c.V0);
        goto L8001D5FC;
        L8001D550: ;
        c.V0 = c.V1 + 0x1u;
        MemoryAccess.WriteU16(m, (c.S1 + 0xAu), (ushort)c.V0);
        c.V0 = c.V0 << 16;
        c.V0 = (uint)((int)c.V0 >> 16);
        c.V0 = (int)c.V0 < 57 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = 0x0000000Cu;
            goto L8001D570;
        }
        c.V0 = 0x0000000Cu;
        MemoryAccess.WriteU16(m, (c.S1 + 0xAu), (ushort)c.V0);
        L8001D570: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0xCu));
        c.V1 = MemoryAccess.ReadU16(m, (c.S1 + 0xCu));
        if ((int)c.V0 <= 0) {
            c.V0 = c.V1 - 0x1u;
            goto L8001D584;
        }
        c.V0 = c.V1 - 0x1u;
        MemoryAccess.WriteU16(m, (c.S1 + 0xCu), (ushort)c.V0);
        L8001D584: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0xCu));
        c.V1 = MemoryAccess.ReadU16(m, (c.S1 + 0xCu));
        if ((int)c.V0 >= 0) {
            goto L8001D5A4;
        }
        c.V0 = c.V1 + 0x1u;
        MemoryAccess.WriteU16(m, (c.S1 + 0xCu), (ushort)c.V0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0xCu));
        L8001D5A4: ;
        if (c.V0 != 0u) {
            c.V0 = 0xFFFFFFFFu;
            goto L8001D5B0;
        }
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU8(m, (c.S1 + 0xEu), (byte)c.V0);
        L8001D5B0: ;
        c.A2 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0xEu));
        if ((int)c.A2 < 0) {
            c.A0 = 0u + 0u;
            goto L8001D5D0;
        }
        c.A0 = 0u + 0u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x14u));
        c.A1 = c.SP + 0x10u;
        c.RA = 0x8001D5D0u;
        Dispatcher.Call(c, m, c.V0);
        L8001D5D0: ;
        c.A0 = 0u + 0u;
        c.A2 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0x9u));
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x14u));
        c.A1 = c.SP + 0x10u;
        c.RA = 0x8001D5E8u;
        Dispatcher.Call(c, m, c.V0);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0x11u));
        if (c.V0 == 0u) {
            c.V1 = 0u < c.S0 ? 1u : 0u;
            goto L8001D604;
        }
        c.V1 = 0u < c.S0 ? 1u : 0u;
        MemoryAccess.WriteU8(m, (c.S1 + 0x11u), (byte)0u);
        L8001D5FC: ;
        c.V0 = 0xFFFFFFFEu;
        goto L8001D730;
        L8001D604: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0x10u));
        c.V0 = c.V0 < 0x00000001u ? 1u : 0u;
        c.V1 = c.V1 & c.V0;
        if (c.V1 == 0u) {
            MemoryAccess.WriteU8(m, (c.S1 + 0xFu), (byte)0u);
            goto L8001D72C;
        }
        MemoryAccess.WriteU8(m, (c.S1 + 0xFu), (byte)0u);
        c.S2 = MemoryAccess.ReadU32(m, (c.S0 + 0x4u));
        c.V0 = c.S2 & 0x0500u;
        if (c.V0 != 0u) {
            c.V0 = 0xFFFFFFFFu;
            goto L8001D730;
        }
        c.V0 = 0xFFFFFFFFu;
        if (c.S4 != 0u) {
            c.V0 = c.S2 & 0x0A00u;
            goto L8001D644;
        }
        c.V0 = c.S2 & 0x0A00u;
        c.S2 = MemoryAccess.ReadU32(m, (c.S5 + 0x4u));
        c.V0 = c.S2 & 0x0A00u;
        L8001D644: ;
        if (c.V0 == 0u) {
            c.A0 = 0x00000003u;
            goto L8001D66C;
        }
        c.A0 = 0x00000003u;
        c.A2 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0x9u));
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x14u));
        c.A1 = c.SP + 0x10u;
        c.RA = 0x8001D660u;
        Dispatcher.Call(c, m, c.V0);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0x9u));
        goto L8001D730;
        L8001D66C: ;
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x4u));
        c.V1 = MemoryAccess.ReadU32(m, (c.S0 + 0xCu));
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0x8u));
        c.V0 = (int)c.V0 < 2 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S2 = c.A0 | c.V1;
            goto L8001D72C;
        }
        c.S2 = c.A0 | c.V1;
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.S1 + 0xFu), (byte)c.V0);
        c.V0 = c.S2 & 0x0004u;
        c.S0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0x9u));
        if (c.V0 == 0u) {
            c.V0 = c.S2 & 0x0008u;
            goto L8001D6E4;
        }
        c.V0 = c.S2 & 0x0008u;
        MemoryAccess.WriteU8(m, (c.S1 + 0xEu), (byte)c.S0);
        c.S0 = c.S0 - 0x1u;
        if ((int)c.S0 >= 0) {
            c.S3 = 0xFFFFFFFDu;
            goto L8001D6BC;
        }
        c.S3 = 0xFFFFFFFDu;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0x8u));
        c.S0 = c.V0 - 0x1u;
        L8001D6BC: ;
        c.A0 = 0x00000004u;
        c.A1 = c.SP + 0x10u;
        c.A2 = c.S0 << 24;
        c.A2 = (uint)((int)c.A2 >> 24);
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x14u));
        c.V1 = 0xFFFFFFF4u;
        MemoryAccess.WriteU16(m, (c.S1 + 0xCu), (ushort)c.V1);
        MemoryAccess.WriteU8(m, (c.S1 + 0x9u), (byte)c.S0);
        c.RA = 0x8001D6E0u;
        Dispatcher.Call(c, m, c.V0);
        c.V0 = c.S2 & 0x0008u;
        L8001D6E4: ;
        if (c.V0 == 0u) {
            c.V0 = c.S3 + 0u;
            goto L8001D730;
        }
        c.V0 = c.S3 + 0u;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0x8u));
        MemoryAccess.WriteU8(m, (c.S1 + 0xEu), (byte)c.S0);
        c.S0 = c.S0 + 0x1u;
        c.V0 = (int)c.S0 < (int)c.V0 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S3 = 0xFFFFFFFDu;
            goto L8001D708;
        }
        c.S3 = 0xFFFFFFFDu;
        c.S0 = 0u + 0u;
        L8001D708: ;
        c.A0 = 0x00000004u;
        c.A1 = c.SP + 0x10u;
        c.A2 = c.S0 << 24;
        c.A2 = (uint)((int)c.A2 >> 24);
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x14u));
        c.V1 = 0x0000000Cu;
        MemoryAccess.WriteU16(m, (c.S1 + 0xCu), (ushort)c.V1);
        MemoryAccess.WriteU8(m, (c.S1 + 0x9u), (byte)c.S0);
        c.RA = 0x8001D72Cu;
        Dispatcher.Call(c, m, c.V0);
        L8001D72C: ;
        c.V0 = c.S3 + 0u;
        L8001D730: ;
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
    public static void func_8001D754_gt2_overlay_1(CpuContext c, IMemory m)
    {
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0xAu));
        c.V0 = (int)c.A1 < -1 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V1 = 0u + 0u;
            goto L8001D780;
        }
        c.V1 = 0u + 0u;
        c.V0 = ~(0u | c.A1);
        c.V0 = c.V0 << 7;
        if ((int)c.V0 >= 0) {
            c.V1 = (uint)((int)c.V0 >> 4);
            goto L8001D780;
        }
        c.V1 = (uint)((int)c.V0 >> 4);
        c.V0 = c.V0 + 0xFu;
        c.V1 = (uint)((int)c.V0 >> 4);
        L8001D780: ;
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0xAu));
        if ((int)c.A0 <= 0) {
            c.V0 = (int)c.A0 < 12 ? 1u : 0u;
            goto L8001D7B8;
        }
        c.V0 = (int)c.A0 < 12 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V1 = 0x00000080u;
            goto L8001D7B8;
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
        L8001D7B8: ;
        c.V0 = c.V1 + 0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001D7C0(CpuContext c, IMemory m)
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
            goto L8001D840;
        }
        c.S4 = c.V1 - 0xCu;
        c.V0 = (int)c.V1 < -1 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = ~(0u | c.V1);
            goto L8001D9DC;
        }
        c.V0 = ~(0u | c.V1);
        c.V0 = c.V0 << 7;
        if ((int)c.V0 >= 0) {
            c.S0 = (uint)((int)c.V0 >> 4);
            goto L8001D834;
        }
        c.S0 = (uint)((int)c.V0 >> 4);
        c.V0 = c.V0 + 0xFu;
        c.S0 = (uint)((int)c.V0 >> 4);
        L8001D834: ;
        c.A0 = 0x00000001u;
        MemoryAccess.WriteU16(m, (c.SP + 0x2Cu), (ushort)c.S7);
        goto L8001D9C4;
        L8001D840: ;
        if ((int)c.S4 < 0) {
            goto L8001D8CC;
        }
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S3 + 0xFu));
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u) {
            c.S0 = c.SP + 0x38u;
            goto L8001D8CC;
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
        c.RA = 0x8001D898u;
        GranTurismo2PC.func_8006BA48(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S6 + 0u;
        c.A2 = c.S7 - c.S2;
        c.A2 = c.A2 - 0x6u;
        c.A3 = c.S5 + 0u;
        c.V0 = 0xFFFFFFFAu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S4);
        c.RA = 0x8001D8C0u;
        GranTurismo2PC.func_8006BA48(c, m);
        c.A0 = c.S6 + 0u;
        c.A1 = 0x00000020u;
        c.RA = 0x8001D8CCu;
        GranTurismo2PC.func_8007DA44(c, m);
        L8001D8CC: ;
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S3 + 0xAu));
        c.V0 = (int)c.A0 < 12 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.S0 = 0x00000080u;
            goto L8001D900;
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
        L8001D900: ;
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
            goto L8001D944;
        }
        c.S2 = c.V1 - c.V0;
        c.A1 = 0u - c.A1;
        c.A3 = c.A2 + 0u;
        L8001D944: ;
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
            goto L8001D9AC;
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
        c.RA = 0x8001D9ACu;
        Dispatcher.Call(c, m, c.V0);
        L8001D9AC: ;
        c.A0 = 0x00000001u;
        c.V0 = c.S2 >> 31;
        c.V0 = c.S2 + c.V0;
        c.V0 = (uint)((int)c.V0 >> (int)(c.A0 & 31u));
        c.V0 = c.S7 + c.V0;
        MemoryAccess.WriteU16(m, (c.SP + 0x2Cu), (ushort)c.V0);
        L8001D9C4: ;
        MemoryAccess.WriteU16(m, (c.SP + 0x30u), (ushort)c.S0);
        c.A2 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S3 + 0x9u));
        c.V0 = MemoryAccess.ReadU32(m, (c.S3 + 0x14u));
        c.A1 = c.SP + 0x20u;
        c.RA = 0x8001D9DCu;
        Dispatcher.Call(c, m, c.V0);
        L8001D9DC: ;
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
    public static void func_8001DA08(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x3D60u;
        c.A0 = 0x80020000u;
        c.A0 = c.A0 + 0x2D80u;
        MemoryAccess.WriteU32(m, (c.SP + 0x3D58u), c.RA);
        c.A1 = c.SP + 0x10u;
        c.RA = 0x8001DA20u;
        GranTurismo2PC.func_80082FAC(c, m);
        c.A0 = 0x801F0000u;
        c.V0 = 0x801D0000u;
        c.V1 = MemoryAccess.ReadU8(m, (c.V0 - 0x6720u));
        c.A0 = c.A0 - 0x950u;
        c.V0 = c.V1 << 4;
        c.V0 = c.V0 - c.V1;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.V1;
        c.V1 = c.SP + 0x10u;
        c.V1 = c.V1 + c.V0;
        c.V0 = c.V1 | c.A0;
        c.V0 = c.V0 & 0x0003u;
        if (c.V0 == 0u) {
            c.V0 = c.V1 + 0x7A0u;
            goto L8001DABC;
        }
        c.V0 = c.V1 + 0x7A0u;
        L8001DA68: ;
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
            goto L8001DA68;
        }
        c.A0 = c.A0 + 0x10u;
        goto L8001DAE8;
        L8001DABC: ;
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
            goto L8001DABC;
        }
        c.A0 = c.A0 + 0x10u;
        L8001DAE8: ;
        c.A2 = MemoryAccess.ReadWordLeft(m, c.A2, (c.V1 + 0x3u));
        c.A2 = MemoryAccess.ReadWordRight(m, c.A2, c.V1);
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V1 + 0x7u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, (c.V1 + 0x4u));
        c.T0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V1 + 0x8u));
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0x3u), c.A2);
        MemoryAccess.WriteWordRight(m, c.A0, c.A2);
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0x7u), c.A3);
        MemoryAccess.WriteWordRight(m, (c.A0 + 0x4u), c.A3);
        MemoryAccess.WriteU8(m, (c.A0 + 0x8u), (byte)c.T0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x3D58u));
        c.SP = c.SP + 0x3D60u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001DB20(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1C4u));
        c.V0 = 0x00000014u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x18u), c.A0);
        c.A0 = 0x00000007u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
        c.RA = 0x8001DB48u;
        GranTurismo2PC.func_80012414(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001DB58(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V1 = MemoryAccess.ReadU32(m, (c.A0 + 0x1C4u));
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x14u));
        if ((int)c.V0 <= 0) {
            c.A1 = 0u + 0u;
            goto L8001DBAC;
        }
        c.A1 = 0u + 0u;
        c.V0 = c.V0 - 0x1u;
        if (c.V0 != 0u) {
            MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
            goto L8001DBAC;
        }
        MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 + 0x18u));
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            c.A1 = 0x00000004u;
            goto L8001DBAC;
        }
        c.A1 = 0x00000004u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x3AB8u;
        c.RA = 0x8001DBA8u;
        GranTurismo2PC.func_800122A0(c, m);
        c.A1 = 0x00000001u;
        L8001DBAC: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = c.A1 + 0u;
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001DBBC(CpuContext c, IMemory m)
    {
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001DBC4(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1C4u));
        c.V0 = 0x00000014u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x14u), c.V0);
        c.RA = 0x8001DBE4u;
        GranTurismo2PC.func_80012434(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001DBF4(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1C4u));
        c.V1 = MemoryAccess.ReadU32(m, (c.A0 + 0x14u));
        if ((int)c.V1 <= 0) {
            c.V0 = 0u + 0u;
            goto L8001DC28;
        }
        c.V0 = 0u + 0u;
        c.V0 = c.V1 - 0x1u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.V0);
        c.V0 = c.V0 < 0x00000001u ? 1u : 0u;
        c.V0 = c.V0 << 2;
        L8001DC28: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001DC30(CpuContext c, IMemory m)
    {
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001DC38(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A2 + 0u;
        c.V0 = c.S1 << 2;
        c.V0 = c.V0 + c.S1;
        c.V0 = c.V0 << 2;
        c.V1 = 0x800B0000u;
        c.V1 = c.V1 + 0x14B0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.V0 + c.V1;
        c.V0 = c.A0 < 0x00000009u ? 1u : 0u;
        if (c.V0 == 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
            goto L8001DD0C;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x3D30u;
        c.V1 = c.A0 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x8001DC8Cu: goto L8001DC8C;
            case 0x8001DCBCu: goto L8001DCBC;
            case 0x8001DCC4u: goto L8001DCC4;
            case 0x8001DCCCu: goto L8001DCCC;
            case 0x8001DCDCu: goto L8001DCDC;
            case 0x8001DD0Cu: goto L8001DD0C;
            case 0x8001DD04u: goto L8001DD04;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L8001DC8C: ;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x3C50u;
        c.RA = 0x8001DC9Cu;
        GranTurismo2PC.func_80016394(c, m);
        c.V0 = c.S1 << 1;
        c.V0 = c.V0 + c.S1;
        c.V0 = c.V0 << 2;
        c.V1 = 0x80050000u;
        c.V1 = c.V1 - 0x4ECCu;
        c.V0 = c.V0 + c.V1;
        MemoryAccess.WriteU32(m, (c.S0 + 0xCu), c.V0);
        goto L8001DD0C;
        L8001DCBC: ;
        MemoryAccess.WriteU16(m, (c.S0 + 0x10u), (ushort)0u);
        goto L8001DD0C;
        L8001DCC4: ;
        c.V0 = 0xFFFFFFF3u;
        goto L8001DD08;
        L8001DCCC: ;
        c.A0 = c.S0 + 0u;
        c.RA = 0x8001DCD4u;
        GranTurismo2PC.func_800163C8(c, m);
        goto L8001DD0C;
        L8001DCDC: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0xCu));
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0xEu));
        c.A0 = c.S0 + 0u;
        MemoryAccess.WriteU16(m, (c.A0 + 0x4u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.A0 + 0x6u), (ushort)c.V1);
        c.A1 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.A2 = 0u + 0u;
        c.RA = 0x8001DCFCu;
        GranTurismo2PC.func_80016410(c, m);
        goto L8001DD0C;
        L8001DD04: ;
        c.V0 = 0x0000000Cu;
        L8001DD08: ;
        MemoryAccess.WriteU16(m, (c.S0 + 0x10u), (ushort)c.V0);
        L8001DD0C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = 0x00000001u;
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001DD24(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        c.V0 = 0x800B0000u;
        c.A1 = 0x80020000u;
        c.A1 = c.A1 - 0x23C8u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
        c.A2 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S2 = MemoryAccess.ReadU32(m, (c.V0 + 0x1C4u));
        c.V0 = 0x00000014u;
        MemoryAccess.WriteU32(m, (c.S2 + 0x14u), c.V0);
        c.V0 = 0x80050000u;
        c.S1 = c.V0 - 0x3C4Cu;
        c.A0 = c.S1 + 0u;
        c.RA = 0x8001DD6Cu;
        GranTurismo2PC.func_8006CDCC(c, m);
        if (c.S0 == 0u) {
            MemoryAccess.WriteU16(m, (c.S1 + 0x6u), (ushort)0u);
            goto L8001DD80;
        }
        MemoryAccess.WriteU16(m, (c.S1 + 0x6u), (ushort)0u);
        c.V0 = MemoryAccess.ReadU32(m, (c.S2 + 0x18u));
        MemoryAccess.WriteU16(m, (c.S1 + 0x6u), (ushort)c.V0);
        L8001DD80: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001DD98(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
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
            goto L8001DDD0;
        }
        c.S3 = c.V0 + 0x1A4u;
        c.S3 = c.S2 + 0u;
        L8001DDD0: ;
        c.S0 = MemoryAccess.ReadU32(m, (c.S1 + 0x14u));
        if ((int)c.S0 <= 0) {
            goto L8001DDF4;
        }
        c.S0 = c.S0 - 0x1u;
        if (c.S0 != 0u) {
            c.A0 = 0x80050000u;
            goto L8001DDF4;
        }
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x3C4Cu;
        c.RA = 0x8001DDF4u;
        GranTurismo2PC.func_8006CE70(c, m);
        L8001DDF4: ;
        MemoryAccess.WriteU32(m, (c.S1 + 0x14u), c.S0);
        c.V0 = 0x80050000u;
        c.S4 = c.V0 - 0x3C4Cu;
        c.A0 = c.S4 + 0u;
        c.A1 = c.S3 + 0u;
        c.RA = 0x8001DE0Cu;
        GranTurismo2PC.func_8006CFC4(c, m);
        c.S0 = c.V0 + 0u;
        c.V0 = 0xFFFFFFFDu;
        if (c.S0 == c.V0) {
            c.V0 = (int)c.S0 < -2 ? 1u : 0u;
            goto L8001DE50;
        }
        c.V0 = (int)c.S0 < -2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0xFFFFFFFCu;
            goto L8001DE34;
        }
        c.V0 = 0xFFFFFFFCu;
        if (c.S0 == c.V0) {
            c.V0 = c.S2 + 0u;
            goto L8001DEB8;
        }
        c.V0 = c.S2 + 0u;
        goto L8001DE78;
        L8001DE34: ;
        c.V0 = 0xFFFFFFFEu;
        if (c.S0 == c.V0) {
            c.V0 = 0xFFFFFFFFu;
            goto L8001DEB4;
        }
        c.V0 = 0xFFFFFFFFu;
        if (c.S0 == c.V0) {
            goto L8001DE60;
        }
        goto L8001DE78;
        L8001DE50: ;
        c.A0 = 0x00000005u;
        c.RA = 0x8001DE58u;
        GranTurismo2PC.func_80060840(c, m);
        c.V0 = c.S2 + 0u;
        goto L8001DEB8;
        L8001DE60: ;
        c.A0 = 0x00000004u;
        c.RA = 0x8001DE68u;
        GranTurismo2PC.func_80060840(c, m);
        c.A0 = c.S4 + 0u;
        c.RA = 0x8001DE70u;
        GranTurismo2PC.func_8006CED8(c, m);
        c.S2 = 0x00000002u;
        goto L8001DEB4;
        L8001DE78: ;
        c.A0 = 0x00000003u;
        c.RA = 0x8001DE80u;
        GranTurismo2PC.func_80060840(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x3C4Cu;
        MemoryAccess.WriteU32(m, (c.S1 + 0x18u), c.S0);
        c.RA = 0x8001DE90u;
        GranTurismo2PC.func_8006CED8(c, m);
        c.V0 = 0x800B0000u;
        c.V1 = 0x80050000u;
        c.V1 = c.V1 - 0x3C5Cu;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
        c.V0 = c.S0 << 2;
        c.V0 = c.V0 + c.V1;
        c.A1 = MemoryAccess.ReadU32(m, c.V0);
        c.S2 = 0x00000001u;
        c.RA = 0x8001DEB4u;
        GranTurismo2PC.func_800122A0(c, m);
        L8001DEB4: ;
        c.V0 = c.S2 + 0u;
        L8001DEB8: ;
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
    public static void func_8001DED8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x3C4Cu;
        c.A1 = c.A1 + 0x10u;
        c.RA = 0x8001DEF4u;
        GranTurismo2PC.func_8006D50C(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001DF04(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.A2 = 0x80050000u;
        c.A0 = c.A2 - 0x3C18u;
        c.V1 = 0x800B0000u;
        c.A1 = 0x800B0000u;
        c.V0 = 0x800B0000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 - 0x7294u));
        c.A1 = MemoryAccess.ReadU32(m, (c.A1 - 0x72A0u));
        c.V0 = c.V0 - 0x729Cu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU32(m, (c.A0 + 0x8u), c.V0);
        MemoryAccess.WriteU32(m, (c.A2 - 0x3C18u), c.V1);
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.A1);
        c.RA = 0x8001DF3Cu;
        GranTurismo2PC.func_80020788(c, m);
        c.A0 = 0u + 0u;
        c.RA = 0x8001DF44u;
        GranTurismo2PC.func_80020828(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001DF54(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 - 0x7298u));
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        if (c.A0 != 0u) {
            c.V0 = c.V0 + 0x1A4u;
            goto L8001DF7C;
        }
        c.V0 = c.V0 + 0x1A4u;
        c.V0 = c.S0 + 0u;
        L8001DF7C: ;
        c.A0 = c.V0 + 0u;
        c.RA = 0x8001DF84u;
        GranTurismo2PC.func_80020868(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L8001DFCC;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = c.S0 + 0u;
            goto L8001DFDC;
        }
        c.V0 = c.S0 + 0u;
        c.V0 = 0x00000002u;
        if (c.V1 != c.V0) {
            c.V0 = c.S0 + 0u;
            goto L8001DFDC;
        }
        c.V0 = c.S0 + 0u;
        c.A0 = 0x00000003u;
        c.RA = 0x8001DFB0u;
        GranTurismo2PC.func_80060840(c, m);
        c.A0 = MemoryAccess.ReadU32(m, (c.S1 - 0x7298u));
        c.S0 = 0x00000001u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x3A10u;
        c.RA = 0x8001DFC4u;
        GranTurismo2PC.func_800122A0(c, m);
        c.V0 = c.S0 + 0u;
        goto L8001DFDC;
        L8001DFCC: ;
        c.A0 = 0x00000004u;
        c.RA = 0x8001DFD4u;
        GranTurismo2PC.func_80060840(c, m);
        c.S0 = 0x00000002u;
        c.V0 = c.S0 + 0u;
        L8001DFDC: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001DFF0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.A0 = c.A0 + 0x10u;
        c.RA = 0x8001E004u;
        GranTurismo2PC.func_80020A3C(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001E014(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.RA = 0x8001E024u;
        GranTurismo2PC.func_80020CE0(c, m);
        c.A2 = c.V0 + 0u;
        c.T1 = 0u + 0u;
        c.V0 = 0x801D0000u;
        c.T2 = c.V0 - 0x6720u;
        c.T0 = 0x00000218u;
        c.A3 = c.A2 + 0x20u;
        L8001E03C: ;
        c.V0 = (int)c.T1 < 128 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L8001E10C;
        }
        c.A1 = MemoryAccess.ReadU32(m, c.A2);
        c.V0 = ~(0u | c.A1);
        if (c.V0 == 0u) {
            c.A0 = c.T0 + c.T2;
            goto L8001E0F8;
        }
        c.A0 = c.T0 + c.T2;
        c.V1 = MemoryAccess.ReadU32(m, c.A0);
        c.V0 = ~(0u | c.V1);
        if (c.V0 == 0u) {
            c.V0 = c.A1 < c.V1 ? 1u : 0u;
            goto L8001E0B4;
        }
        c.V0 = c.A1 < c.V1 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V1 = c.A0 + 0u;
            goto L8001E0F8;
        }
        c.V1 = c.A0 + 0u;
        c.V0 = c.A2 + 0u;
        c.A0 = c.A3 + 0u;
        L8001E080: ;
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
            goto L8001E080;
        }
        c.V1 = c.V1 + 0x10u;
        goto L8001E0EC;
        L8001E0B4: ;
        c.V1 = c.A0 + 0u;
        c.V0 = c.A2 + 0u;
        c.A0 = c.A3 + 0u;
        L8001E0C0: ;
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
            goto L8001E0C0;
        }
        c.V1 = c.V1 + 0x10u;
        L8001E0EC: ;
        c.T3 = MemoryAccess.ReadU32(m, c.V0);
        MemoryAccess.WriteU32(m, c.V1, c.T3);
        L8001E0F8: ;
        c.A3 = c.A3 + 0x24u;
        c.A2 = c.A2 + 0x24u;
        c.T0 = c.T0 + 0x24u;
        c.T1 = c.T1 + 0x1u;
        goto L8001E03C;
        L8001E10C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001E11C(CpuContext c, IMemory m)
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
            goto L8001E16C;
        }
        c.S7 = c.A2 + 0u;
        c.V0 = 0xFFFFFFFFu;
        goto L8001E254;
        L8001E16C: ;
        c.S3 = 0u + 0u;
        c.S5 = 0x00000068u;
        c.S4 = 0x00000004u;
        L8001E178: ;
        c.V0 = (int)c.S3 < 5 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A0 = c.S7 + c.S4;
            goto L8001E250;
        }
        c.A0 = c.S7 + c.S4;
        c.V1 = MemoryAccess.ReadU32(m, c.A0);
        c.V0 = ~(0u | c.V1);
        if (c.V0 == 0u) {
            c.V0 = c.S3 + 0u;
            goto L8001E254;
        }
        c.V0 = c.S3 + 0u;
        c.A1 = MemoryAccess.ReadU32(m, c.S6);
        c.V0 = c.A1 < c.V1 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = c.S3 + 0u;
            goto L8001E254;
        }
        c.V0 = c.S3 + 0u;
        if (c.V1 != c.A1) {
            c.S1 = c.A0 + 0u;
            goto L8001E240;
        }
        c.S1 = c.A0 + 0u;
        c.S0 = c.S6 + 0u;
        c.A0 = 0u + 0u;
        L8001E1BC: ;
        c.V1 = MemoryAccess.ReadU8(m, c.S0);
        c.S0 = c.S0 + 0x1u;
        c.V0 = MemoryAccess.ReadU8(m, c.S1);
        if (c.V1 != c.V0) {
            c.S1 = c.S1 + 0x1u;
            goto L8001E240;
        }
        c.S1 = c.S1 + 0x1u;
        c.A0 = c.A0 + 0x1u;
        c.V0 = (int)c.A0 < 20 ? 1u : 0u;
        if (c.V0 != 0u) {
            goto L8001E1BC;
        }
        c.S1 = c.S7 + c.S5;
        c.S0 = c.FP + 0u;
        c.A0 = c.S1 + 0u;
        c.RA = 0x8001E1F4u;
        GranTurismo2PC.func_8008CFC4(c, m);
        c.A0 = c.S0 + 0u;
        c.S2 = c.V0 + 0u;
        c.RA = 0x8001E200u;
        GranTurismo2PC.func_8008CFC4(c, m);
        if (c.S2 != c.V0) {
            goto L8001E240;
        }
        if ((int)c.S2 < 0) {
            c.A0 = 0u + 0u;
            goto L8001E250;
        }
        c.A0 = 0u + 0u;
        L8001E210: ;
        c.V1 = MemoryAccess.ReadU8(m, c.S0);
        c.S0 = c.S0 + 0x1u;
        c.V0 = MemoryAccess.ReadU8(m, c.S1);
        if (c.V1 != c.V0) {
            c.S1 = c.S1 + 0x1u;
            goto L8001E240;
        }
        c.S1 = c.S1 + 0x1u;
        c.A0 = c.A0 + 0x1u;
        c.V0 = (int)c.S2 < (int)c.A0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0xFFFFFFFFu;
            goto L8001E210;
        }
        c.V0 = 0xFFFFFFFFu;
        goto L8001E254;
        L8001E240: ;
        c.S5 = c.S5 + 0xCu;
        c.S4 = c.S4 + 0x14u;
        c.S3 = c.S3 + 0x1u;
        goto L8001E178;
        L8001E250: ;
        c.V0 = 0xFFFFFFFFu;
        L8001E254: ;
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
    public static void func_8001E284(CpuContext c, IMemory m)
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
        c.RA = 0x8001E2B4u;
        GranTurismo2PC.func_80020D0C(c, m);
        c.S6 = c.V0 + 0u;
        c.FP = 0x00001418u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), 0u);
        L8001E2C0: ;
        c.V1 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = (int)c.V1 < 6 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.S7 = 0u + 0u;
            goto L8001E35C;
        }
        c.S7 = 0u + 0u;
        c.V1 = 0x801D0000u;
        c.V1 = c.V1 - 0x6720u;
        c.S3 = c.FP + c.V1;
        L8001E2E0: ;
        c.V0 = (int)c.S7 < 10 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.S5 = 0u + 0u;
            goto L8001E348;
        }
        c.S5 = 0u + 0u;
        c.S4 = 0x00000068u;
        c.S2 = 0x00000004u;
        L8001E2F4: ;
        c.V0 = (int)c.S5 < 5 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.S1 = c.S6 + c.S2;
            goto L8001E338;
        }
        c.S1 = c.S6 + c.S2;
        c.A0 = c.S1 + 0u;
        c.S0 = c.S6 + c.S4;
        c.A1 = c.S0 + 0u;
        c.A2 = c.S3 + 0u;
        c.RA = 0x8001E314u;
        GranTurismo2PC.func_8001E11C(c, m);
        if ((int)c.V0 < 0) {
            c.A0 = c.S3 + 0u;
            goto L8001E328;
        }
        c.A0 = c.S3 + 0u;
        c.A1 = c.S1 + 0u;
        c.A2 = c.S0 + 0u;
        c.RA = 0x8001E328u;
        GranTurismo2PC.func_8005DEFC(c, m);
        L8001E328: ;
        c.S4 = c.S4 + 0xCu;
        c.S2 = c.S2 + 0x14u;
        c.S5 = c.S5 + 0x1u;
        goto L8001E2F4;
        L8001E338: ;
        c.S6 = c.S6 + 0xA4u;
        c.S3 = c.S3 + 0xA4u;
        c.S7 = c.S7 + 0x1u;
        goto L8001E2E0;
        L8001E348: ;
        c.V1 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.FP = c.FP + 0x668u;
        c.V1 = c.V1 + 0x1u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V1);
        goto L8001E2C0;
        L8001E35C: ;
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
    public static void func_8001E38C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.RA = 0x8001E3A8u;
        GranTurismo2PC.func_80020D38(c, m);
        c.S2 = c.V0 + 0u;
        c.V0 = 0x801D0000u;
        c.V0 = c.V0 - 0x6720u;
        c.S3 = c.V0 + 0x3A88u;
        c.S1 = 0u + 0u;
        c.S0 = 0x00000004u;
        L8001E3C0: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, c.S2);
        c.V0 = (int)c.S1 < (int)c.V0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A0 = c.S3 + 0u;
            goto L8001E3EC;
        }
        c.A0 = c.S3 + 0u;
        c.A1 = c.S2 + c.S0;
        c.A2 = 0u + 0u;
        c.RA = 0x8001E3E0u;
        GranTurismo2PC.func_8005E0D0(c, m);
        c.S0 = c.S0 + 0x14u;
        c.S1 = c.S1 + 0x1u;
        goto L8001E3C0;
        L8001E3EC: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001E408(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.RA = 0x8001E424u;
        GranTurismo2PC.func_80020D64(c, m);
        c.S2 = c.V0 + 0u;
        c.V0 = 0x801D0000u;
        c.V0 = c.V0 - 0x6720u;
        c.S3 = c.V0 + 0x3B2Cu;
        c.S1 = 0u + 0u;
        c.S0 = 0x00000004u;
        L8001E43C: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, c.S2);
        c.V0 = (int)c.S1 < (int)c.V0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A0 = c.S3 + 0u;
            goto L8001E468;
        }
        c.A0 = c.S3 + 0u;
        c.A1 = c.S2 + c.S0;
        c.A2 = 0u + 0u;
        c.RA = 0x8001E45Cu;
        GranTurismo2PC.func_8005E0D0(c, m);
        c.S0 = c.S0 + 0x14u;
        c.S1 = c.S1 + 0x1u;
        goto L8001E43C;
        L8001E468: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001E484(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.RA = 0x8001E4A0u;
        Dispatcher.Call(c, m, 0x80020D90u);
        c.S2 = c.V0 + 0u;
        c.V0 = 0x801D0000u;
        c.V0 = c.V0 - 0x6720u;
        c.S3 = c.V0 + 0x3BD0u;
        c.S1 = 0u + 0u;
        c.S0 = 0x00000004u;
        L8001E4B8: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, c.S2);
        c.V0 = (int)c.S1 < (int)c.V0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A0 = c.S3 + 0u;
            goto L8001E4E4;
        }
        c.A0 = c.S3 + 0u;
        c.A1 = c.S2 + c.S0;
        c.A2 = 0x00000001u;
        c.RA = 0x8001E4D8u;
        GranTurismo2PC.func_8005E0D0(c, m);
        c.S0 = c.S0 + 0x14u;
        c.S1 = c.S1 + 0x1u;
        goto L8001E4B8;
        L8001E4E4: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001E500(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.A2 = 0x80050000u;
        c.A0 = c.A2 - 0x3C18u;
        c.V1 = 0x800B0000u;
        c.A1 = 0x800B0000u;
        c.V0 = 0x800B0000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 - 0x7294u));
        c.A1 = MemoryAccess.ReadU32(m, (c.A1 - 0x72A0u));
        c.V0 = c.V0 - 0x729Cu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU32(m, (c.A0 + 0x8u), c.V0);
        MemoryAccess.WriteU32(m, (c.A2 - 0x3C18u), c.V1);
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.A1);
        c.RA = 0x8001E538u;
        GranTurismo2PC.func_80020788(c, m);
        c.A0 = 0x00000001u;
        c.RA = 0x8001E540u;
        GranTurismo2PC.func_80020828(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001E550(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        if (c.A0 != 0u) {
            c.V0 = c.V0 + 0x1A4u;
            goto L8001E574;
        }
        c.V0 = c.V0 + 0x1A4u;
        c.V0 = c.S0 + 0u;
        L8001E574: ;
        c.A0 = c.V0 + 0u;
        c.RA = 0x8001E57Cu;
        GranTurismo2PC.func_80020868(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L8001E5D8;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = c.S0 + 0u;
            goto L8001E5E8;
        }
        c.V0 = c.S0 + 0u;
        c.V0 = 0x00000002u;
        if (c.V1 != c.V0) {
            c.V0 = c.S0 + 0u;
            goto L8001E5E8;
        }
        c.V0 = c.S0 + 0u;
        c.A0 = 0x00000003u;
        c.RA = 0x8001E5A8u;
        GranTurismo2PC.func_80060840(c, m);
        c.RA = 0x8001E5B0u;
        GranTurismo2PC.func_8001E014(c, m);
        c.RA = 0x8001E5B8u;
        GranTurismo2PC.func_8001E284(c, m);
        c.RA = 0x8001E5C0u;
        GranTurismo2PC.func_8001E38C(c, m);
        c.RA = 0x8001E5C8u;
        GranTurismo2PC.func_8001E408(c, m);
        c.RA = 0x8001E5D0u;
        GranTurismo2PC.func_8001E484(c, m);
        c.V0 = c.S0 + 0u;
        goto L8001E5E8;
        L8001E5D8: ;
        c.A0 = 0x00000004u;
        c.RA = 0x8001E5E0u;
        GranTurismo2PC.func_80060840(c, m);
        c.S0 = 0x00000002u;
        c.V0 = c.S0 + 0u;
        L8001E5E8: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001E5F8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.A0 = c.A0 + 0x10u;
        c.RA = 0x8001E60Cu;
        GranTurismo2PC.func_80020A3C(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001E61C(CpuContext c, IMemory m)
    {
        c.A1 = 0x00000001u;
        c.V1 = 0u + 0u;
        c.V0 = c.A0 + c.V1;
        L8001E628: ;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x2B5Cu));
        if (c.V0 != 0u) {
            goto L8001E63C;
        }
        c.A1 = 0u + 0u;
        L8001E63C: ;
        c.V1 = c.V1 + 0x1u;
        c.V0 = (int)c.V1 < 8 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = c.A0 + c.V1;
            goto L8001E628;
        }
        c.V0 = c.A0 + c.V1;
        if (c.A1 == 0u) {
            c.A1 = 0u + 0u;
            goto L8001E6FC;
        }
        c.A1 = 0u + 0u;
        c.A2 = 0x00000001u;
        c.V0 = 0x801D0000u;
        c.V1 = c.V0 - 0x3300u;
        L8001E660: ;
        c.V0 = (int)c.A1 < 10 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L8001E68C;
        }
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V1 + 0x1u));
        if (c.V0 != 0u) {
            goto L8001E680;
        }
        MemoryAccess.WriteU8(m, (c.V1 + 0x1u), (byte)c.A2);
        L8001E680: ;
        c.V1 = c.V1 + 0xA4u;
        c.A1 = c.A1 + 0x1u;
        goto L8001E660;
        L8001E68C: ;
        c.A1 = 0x00000001u;
        c.V1 = 0u + 0u;
        c.V0 = c.A0 + c.V1;
        L8001E698: ;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x2B64u));
        if (c.V0 != 0u) {
            goto L8001E6AC;
        }
        c.A1 = 0u + 0u;
        L8001E6AC: ;
        c.V1 = c.V1 + 0x1u;
        c.V0 = (int)c.V1 < 8 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = c.A0 + c.V1;
            goto L8001E698;
        }
        c.V0 = c.A0 + c.V1;
        if (c.A1 == 0u) {
            c.A0 = 0u + 0u;
            goto L8001E6FC;
        }
        c.A0 = 0u + 0u;
        c.A1 = 0x00000001u;
        c.V0 = 0x801D0000u;
        c.V1 = c.V0 - 0x3968u;
        L8001E6D0: ;
        c.V0 = (int)c.A0 < 10 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L8001E6FC;
        }
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V1 + 0x1u));
        if (c.V0 != 0u) {
            goto L8001E6F0;
        }
        MemoryAccess.WriteU8(m, (c.V1 + 0x1u), (byte)c.A1);
        L8001E6F0: ;
        c.V1 = c.V1 + 0xA4u;
        c.A0 = c.A0 + 0x1u;
        goto L8001E6D0;
        L8001E6FC: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001E704(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.A2 = 0x80050000u;
        c.A0 = c.A2 - 0x3C18u;
        c.V1 = 0x800B0000u;
        c.A1 = 0x800B0000u;
        c.V0 = 0x800B0000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 - 0x7294u));
        c.A1 = MemoryAccess.ReadU32(m, (c.A1 - 0x72A0u));
        c.V0 = c.V0 - 0x729Cu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU32(m, (c.A0 + 0x8u), c.V0);
        MemoryAccess.WriteU32(m, (c.A2 - 0x3C18u), c.V1);
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.A1);
        c.RA = 0x8001E73Cu;
        GranTurismo2PC.func_80020788(c, m);
        c.A0 = 0x00000002u;
        c.RA = 0x8001E744u;
        GranTurismo2PC.func_80020828(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001E754(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        if (c.A0 != 0u) {
            c.V0 = c.V0 + 0x1A4u;
            goto L8001E778;
        }
        c.V0 = c.V0 + 0x1A4u;
        c.V0 = c.S0 + 0u;
        L8001E778: ;
        c.A0 = c.V0 + 0u;
        c.RA = 0x8001E780u;
        GranTurismo2PC.func_80020868(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L8001E7C4;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = c.S0 + 0u;
            goto L8001E7D4;
        }
        c.V0 = c.S0 + 0u;
        c.V0 = 0x00000002u;
        if (c.V1 != c.V0) {
            c.V0 = c.S0 + 0u;
            goto L8001E7D4;
        }
        c.V0 = c.S0 + 0u;
        c.A0 = 0x00000003u;
        c.RA = 0x8001E7ACu;
        GranTurismo2PC.func_80060840(c, m);
        c.RA = 0x8001E7B4u;
        GranTurismo2PC.func_80020DBC(c, m);
        c.A0 = c.V0 + 0u;
        c.RA = 0x8001E7BCu;
        GranTurismo2PC.func_8001E61C(c, m);
        c.V0 = c.S0 + 0u;
        goto L8001E7D4;
        L8001E7C4: ;
        c.A0 = 0x00000004u;
        c.RA = 0x8001E7CCu;
        GranTurismo2PC.func_80060840(c, m);
        c.S0 = 0x00000002u;
        c.V0 = c.S0 + 0u;
        L8001E7D4: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001E7E4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.A0 = c.A0 + 0x10u;
        c.RA = 0x8001E7F8u;
        GranTurismo2PC.func_80020A3C(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001E808(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x40u;
        c.A3 = 0x00980000u;
        c.A3 = c.A3 | 0x9680u;
        if (c.A1 != 0u) {
            c.T1 = 0u + 0u;
            goto L8001E82C;
        }
        c.T1 = 0u + 0u;
        c.V0 = 0x00000030u;
        MemoryAccess.WriteU8(m, c.A0, (byte)c.V0);
        MemoryAccess.WriteU8(m, (c.A0 + 0x1u), (byte)0u);
        goto L8001E920;
        L8001E82C: ;
        c.T0 = 0u + 0u;
        c.T3 = 0x00000001u;
        c.T2 = 0x66660000u;
        c.T2 = c.T2 | 0x6667u;
        L8001E83C: ;
        if (c.A3 != 0u) { c.LO = c.A1 / c.A3; c.HI = c.A1 % c.A3; }
        c.A2 = c.LO;
        c.V0 = 0u < c.A2 ? 1u : 0u;
        c.V0 = c.V0 | c.T1;
        if (c.V0 == 0u) {
            { var _r = (long)(int)c.A2 * (int)c.A3; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
            goto L8001E870;
        }
        { var _r = (long)(int)c.A2 * (int)c.A3; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V1 = c.SP + c.T0;
        c.V0 = c.A2 + 0x30u;
        MemoryAccess.WriteU8(m, c.V1, (byte)c.V0);
        c.T1 = 0x00000001u;
        c.T0 = c.T0 + c.T1;
        c.T4 = c.LO;
        c.A1 = c.A1 - c.T4;
        L8001E870: ;
        if (c.A3 == c.T3) {
            { var _r = (long)(int)c.A3 * (int)c.T2; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
            goto L8001E88C;
        }
        { var _r = (long)(int)c.A3 * (int)c.T2; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V0 = (uint)((int)c.A3 >> 31);
        c.T4 = c.HI;
        c.V1 = (uint)((int)c.T4 >> 2);
        c.A3 = c.V1 - c.V0;
        goto L8001E83C;
        L8001E88C: ;
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
            goto L8001E8C4;
        }
        c.V1 = 0x00000003u;
        L8001E8C4: ;
        c.V0 = MemoryAccess.ReadU8(m, c.SP);
        MemoryAccess.WriteU8(m, c.A0, (byte)c.V0);
        c.V0 = MemoryAccess.ReadU8(m, (c.SP + 0x1u));
        c.T0 = 0u + 0u;
        goto L8001E914;
        L8001E8DC: ;
        c.V1 = c.V1 - 0x1u;
        if (c.V1 != 0u) {
            c.V0 = 0x0000002Cu;
            goto L8001E8F4;
        }
        c.V0 = 0x0000002Cu;
        MemoryAccess.WriteU8(m, c.A0, (byte)c.V0);
        c.A0 = c.A0 + 0x1u;
        c.V1 = 0x00000003u;
        L8001E8F4: ;
        c.T0 = c.T0 + 0x1u;
        c.V0 = c.SP + c.T0;
        c.V0 = MemoryAccess.ReadU8(m, c.V0);
        MemoryAccess.WriteU8(m, c.A0, (byte)c.V0);
        c.V0 = c.T0 + c.SP;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V0 + 0x1u));
        L8001E914: ;
        if (c.V0 != 0u) {
            c.A0 = c.A0 + 0x1u;
            goto L8001E8DC;
        }
        c.A0 = c.A0 + 0x1u;
        MemoryAccess.WriteU8(m, c.A0, (byte)0u);
        L8001E920: ;
        c.SP = c.SP + 0x40u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001E928(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        c.V1 = 0x801D0000u;
        c.V1 = MemoryAccess.ReadU8(m, (c.V1 - 0x6720u));
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 - 0x72A0u));
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        if (c.V1 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
            goto L8001E98C;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.A0 = c.S1 + 0u;
        c.RA = 0x8001E954u;
        GranTurismo2PC.func_8007F174(c, m);
        c.A0 = c.S1 + 0u;
        c.S0 = 0x801C0000u;
        c.S0 = c.S0 - 0x6190u;
        c.A1 = c.S0 + 0u;
        c.RA = 0x8001E968u;
        GranTurismo2PC.func_8006ECD8(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S0 + 0x14u;
        c.RA = 0x8001E974u;
        GranTurismo2PC.func_8006ECD8(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S0 + 0x30u;
        c.RA = 0x8001E980u;
        GranTurismo2PC.func_8006ECD8(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S0 + 0x4Fu;
        c.RA = 0x8001E98Cu;
        GranTurismo2PC.func_8006ECD8(c, m);
        L8001E98C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001E9A0(CpuContext c, IMemory m)
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
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6720u));
        c.V1 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        c.S4 = MemoryAccess.ReadU32(m, (c.V1 - 0x72A0u));
        c.A0 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        if (c.V0 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
            goto L8001EAAC;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        if (c.A2 == 0u) {
            c.S0 = c.A1 + 0u;
            goto L8001EA2C;
        }
        c.S0 = c.A1 + 0u;
        c.V0 = MemoryAccess.ReadU8(m, c.A1);
        if (c.V0 == 0u) {
            c.V1 = c.A1 + 0u;
            goto L8001EA20;
        }
        c.V1 = c.A1 + 0u;
        L8001EA0C: ;
        c.V1 = c.V1 + 0x2u;
        c.V0 = MemoryAccess.ReadU8(m, c.V1);
        if (c.V0 != 0u) {
            c.A0 = c.A0 + 0xCu;
            goto L8001EA0C;
        }
        c.A0 = c.A0 + 0xCu;
        L8001EA20: ;
        c.V0 = (uint)((int)c.A0 >> 1);
        c.S1 = c.S1 - c.V0;
        c.S0 = c.A1 + 0u;
        L8001EA2C: ;
        c.S5 = 0x02000000u;
        c.S2 = c.S3 + 0x1Fu;
        L8001EA34: ;
        c.V0 = MemoryAccess.ReadU8(m, c.S0);
        if (c.V0 == 0u) {
            c.A0 = c.S4 + 0u;
            goto L8001EAAC;
        }
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.V0 = MemoryAccess.ReadU8(m, (c.S0 + 0x1u));
        c.A1 = c.A1 << 8;
        c.A1 = c.A1 | c.V0;
        c.RA = 0x8001EA58u;
        GranTurismo2PC.func_8007F18C(c, m);
        if (c.V0 == 0u) {
            c.A0 = c.V0 + 0u;
            goto L8001EAA0;
        }
        c.A0 = c.V0 + 0u;
        c.A1 = c.S7 + 0u;
        c.A2 = c.S6 | c.S5;
        c.RA = 0x8001EA6Cu;
        GranTurismo2PC.func_8007F01C(c, m);
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
        L8001EAA0: ;
        c.S1 = c.S1 + 0xCu;
        c.S0 = c.S0 + 0x2u;
        goto L8001EA34;
        L8001EAAC: ;
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
    public static void func_8001EAD8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0x801D0000u;
        c.S0 = c.S0 - 0x6720u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.RA = 0x8001EAFCu;
        GranTurismo2PC.func_80020CB4(c, m);
        c.A0 = 0x00000001u;
        c.S2 = c.S0 + 0x3C74u;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x3C74u));
        c.V1 = (int)c.V1 < 100 ? 1u : 0u;
        if (c.V1 == 0u) {
            c.A1 = c.V0 + 0u;
            goto L8001EB44;
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
        L8001EB44: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = c.A0 + 0u;
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001EB60(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0x801D0000u;
        c.S0 = c.S0 - 0x6720u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.S0 = c.S0 + 0x3C74u;
        c.RA = 0x8001EB84u;
        GranTurismo2PC.func_80020CB4(c, m);
        c.V1 = c.S1 << 2;
        c.V1 = c.V1 + c.S1;
        c.V1 = c.V1 << 3;
        c.V1 = c.V1 + c.S1;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + 0x4u;
        c.S1 = c.V0 + c.V1;
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x8001EBACu;
        GranTurismo2PC.func_8005E7F0(c, m);
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
    public static void func_8001EBD4(CpuContext c, IMemory m)
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
        c.RA = 0x8001EC20u;
        GranTurismo2PC.func_80020CB4(c, m);
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
        c.RA = 0x8001EC4Cu;
        GranTurismo2PC.func_8006AC68(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6A40u;
        c.RA = 0x8001EC5Cu;
        GranTurismo2PC.func_8007DA80(c, m);
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
            goto L8001EC90;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x3Cu), c.V1);
        c.V0 = c.S3 + 0x80u;
        c.S3 = c.V0 + c.S4;
        goto L8001EC9C;
        L8001EC90: ;
        { var _r = (long)(int)c.S3 * (int)c.S4; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.T0 = c.LO;
        c.S3 = (uint)((int)c.T0 >> 7);
        L8001EC9C: ;
        c.V0 = (int)c.S3 < 256 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = 0x55550000u;
            goto L8001ECAC;
        }
        c.V0 = 0x55550000u;
        c.S3 = 0x000000FFu;
        L8001ECAC: ;
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
        c.RA = 0x8001ECF0u;
        GranTurismo2PC.func_80060AE8(c, m);
        c.S2 = c.SP + 0x30u;
        c.A0 = c.S2 + 0u;
        c.A1 = c.V0 + 0u;
        c.A2 = c.S5 - 0x6Eu;
        c.A3 = c.FP + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S1);
        c.RA = 0x8001ED0Cu;
        GranTurismo2PC.func_8006AC90(c, m);
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
        c.RA = 0x8001ED3Cu;
        GranTurismo2PC.func_80060D28(c, m);
        c.A0 = c.S6 + 0u;
        c.A1 = c.SP + 0x20u;
        c.A2 = c.S3 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.V0);
        c.RA = 0x8001ED50u;
        GranTurismo2PC.func_8006BB08(c, m);
        c.A1 = MemoryAccess.ReadU32(m, (c.S0 + 0x90u));
        c.S0 = c.SP + 0x50u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x8001ED60u;
        GranTurismo2PC.func_8001E808(c, m);
        c.A0 = c.S2 + 0u;
        c.A1 = c.S0 + 0u;
        c.A2 = c.S5 + 0x78u;
        c.A3 = c.FP + 0u;
        c.V0 = 0xFFFFFFFEu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.RA = 0x8001ED84u;
        GranTurismo2PC.func_8006B184(c, m);
        c.A0 = c.S6 + 0u;
        c.A1 = c.S3 << 8;
        c.A1 = c.S3 | c.A1;
        c.V0 = c.S3 << 16;
        c.A1 = c.A1 | c.V0;
        c.RA = 0x8001ED9Cu;
        GranTurismo2PC.func_80081478(c, m);
        c.A1 = 0x80050000u;
        c.A0 = MemoryAccess.ReadU16(m, (c.A1 - 0x4600u));
        c.A1 = c.A1 - 0x4600u;
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
        c.RA = 0x8001EDF4u;
        GranTurismo2PC.func_8007DA44(c, m);
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
    public static void func_8001EE24(CpuContext c, IMemory m)
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
            goto L8001F058;
        }
        c.S3 = c.S5 + 0u;
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x3D58u;
        c.V1 = c.A0 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x8001F058u: goto L8001F058;
            case 0x8001EE8Cu: goto L8001EE8C;
            case 0x8001F044u: goto L8001F044;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L8001EE8C: ;
        if ((int)c.S1 < 0) {
            c.V0 = c.S5 + 0u;
            goto L8001F05C;
        }
        c.V0 = c.S5 + 0u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x24u));
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x14u));
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x6u));
        c.V0 = c.V0 << 7;
        if (c.V1 != 0u) { if ((int)c.V0 == int.MinValue && (int)c.V1 == -1) { c.LO = 0x80000000u; c.HI = 0u; } else { c.LO = (uint)((int)c.V0 / (int)c.V1); c.HI = (uint)((int)c.V0 % (int)c.V1); } }
        c.T0 = c.LO;
        if (c.S1 != c.A0) {
            goto L8001EEC0;
        }
        c.A0 = c.S0 + 0u;
        c.RA = 0x8001EEBCu;
        GranTurismo2PC.func_8006CF64(c, m);
        c.T0 = 0u - c.V0;
        L8001EEC0: ;
        if (c.T0 == 0u) {
            c.V0 = c.S5 + 0u;
            goto L8001F05C;
        }
        c.V0 = c.S5 + 0u;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x26u));
        c.V0 = ~(0u | c.V1);
        c.V0 = c.V0 >> 31;
        c.V1 = (int)c.V1 < -1 ? 1u : 0u;
        c.V0 = c.V0 | c.V1;
        if (c.V0 == 0u) {
            c.A0 = c.S4 + 0x4u;
            goto L8001EF04;
        }
        c.A0 = c.S4 + 0x4u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0xCu));
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0xEu));
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x10u));
        c.A1 = c.S1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.T0);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.RA = 0x8001EF04u;
        GranTurismo2PC.func_8001EBD4(c, m);
        L8001EF04: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x6u));
        if (c.S1 != c.V0) {
            c.V0 = c.S5 + 0u;
            goto L8001F05C;
        }
        c.V0 = c.S5 + 0u;
        c.S2 = 0x00600000u;
        c.S2 = c.S2 | 0x6060u;
        c.A0 = c.S1 + 0u;
        c.RA = 0x8001EF24u;
        GranTurismo2PC.func_8001EAD8(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L8001EF64;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L8001EF4C;
        }
        c.V0 = 0x00000002u;
        if (c.V1 == 0u) {
            c.V0 = 0x801C0000u;
            goto L8001EF5C;
        }
        c.V0 = 0x801C0000u;
        goto L8001EF7C;
        L8001EF4C: ;
        if (c.V1 == c.V0) {
            c.V0 = 0x801C0000u;
            goto L8001EF70;
        }
        c.V0 = 0x801C0000u;
        goto L8001EF7C;
        L8001EF5C: ;
        c.S3 = c.V0 - 0x6190u;
        goto L8001EF7C;
        L8001EF64: ;
        c.V0 = 0x801C0000u;
        c.S3 = c.V0 - 0x6160u;
        goto L8001EF74;
        L8001EF70: ;
        c.S3 = c.V0 - 0x617Cu;
        L8001EF74: ;
        c.S2 = 0x00140000u;
        c.S2 = c.S2 | 0x2864u;
        L8001EF7C: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S6 + 0x14u));
        if (c.V0 != 0u) {
            c.V1 = 0u + 0u;
            goto L8001F058;
        }
        c.V1 = 0u + 0u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x26u));
        c.V0 = ~(0u | c.V0);
        if (c.S3 == 0u) {
            c.V0 = c.V0 >> 31;
            goto L8001EFA4;
        }
        c.V0 = c.V0 >> 31;
        c.V1 = c.V0 & 0x0001u;
        L8001EFA4: ;
        if (c.V1 == 0u) {
            c.V0 = 0x801D0000u;
            goto L8001F058;
        }
        c.V0 = 0x801D0000u;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6720u));
        if (c.V0 != 0u) {
            c.A0 = c.SP + 0x18u;
            goto L8001EFE4;
        }
        c.A0 = c.SP + 0x18u;
        c.A0 = c.S4 + 0u;
        c.A1 = c.S3 + 0u;
        c.A2 = 0x000000B0u;
        c.A3 = 0x00000182u;
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S2);
        c.RA = 0x8001EFDCu;
        GranTurismo2PC.func_8001E9A0(c, m);
        c.V0 = c.S5 + 0u;
        goto L8001F05C;
        L8001EFE4: ;
        c.A1 = 0x0000001Eu;
        c.RA = 0x8001EFECu;
        GranTurismo2PC.func_8006AC68(c, m);
        c.A0 = c.SP + 0x18u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6A40u;
        c.RA = 0x8001EFFCu;
        GranTurismo2PC.func_8007DA80(c, m);
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
        c.RA = 0x8001F03Cu;
        GranTurismo2PC.func_8006ADB4(c, m);
        c.V0 = c.S5 + 0u;
        goto L8001F05C;
        L8001F044: ;
        if ((int)c.S1 < 0) {
            c.S5 = 0u + 0u;
            goto L8001F058;
        }
        c.S5 = 0u + 0u;
        c.A0 = c.S1 + 0u;
        c.RA = 0x8001F054u;
        GranTurismo2PC.func_8001EAD8(c, m);
        c.S5 = c.V0 < 0x00000001u ? 1u : 0u;
        L8001F058: ;
        c.V0 = c.S5 + 0u;
        L8001F05C: ;
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
    public static void func_8001F084(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.RA = 0x8001F098u;
        GranTurismo2PC.func_80020CB4(c, m);
        c.V1 = 0x800B0000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 - 0x7298u));
        c.S0 = MemoryAccess.ReadU32(m, (c.V1 + 0x1C4u));
        c.S1 = c.V0 + 0u;
        c.RA = 0x8001F0B0u;
        GranTurismo2PC.func_8001E928(c, m);
        c.A3 = 0x80050000u;
        c.A0 = c.A3 - 0x3BC4u;
        c.A1 = 0x80020000u;
        c.A1 = c.A1 - 0x11DCu;
        c.V0 = 0x80050000u;
        c.V1 = 0xFFFFFFFFu;
        MemoryAccess.WriteU16(m, (c.V0 - 0x3BE4u), (ushort)c.V1);
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU16(m, (c.V0 - 0x3BC8u), (ushort)c.V1);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.S1);
        c.A2 = c.S0 + 0u;
        MemoryAccess.WriteU16(m, (c.A3 - 0x3BC4u), (ushort)c.V0);
        c.RA = 0x8001F0E4u;
        GranTurismo2PC.func_8006CDCC(c, m);
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x14F0u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x3B90u;
        c.A2 = 0u + 0u;
        c.RA = 0x8001F0FCu;
        GranTurismo2PC.func_8006E1CC(c, m);
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
    public static void func_8001F11C(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
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
            goto L8001F154;
        }
        c.S0 = c.V0 + 0x1A4u;
        c.S0 = c.S4 + 0u;
        L8001F154: ;
        c.V0 = 0x80050000u;
        c.S3 = c.V0 - 0x3BFCu;
        c.A0 = c.S3 + 0u;
        c.RA = 0x8001F164u;
        GranTurismo2PC.func_8006BE64(c, m);
        c.V0 = 0x80050000u;
        c.S2 = c.V0 - 0x3BE0u;
        c.A0 = c.S2 + 0u;
        c.RA = 0x8001F174u;
        GranTurismo2PC.func_8006BE64(c, m);
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x18u));
        if ((int)c.V0 <= 0) {
            c.V0 = c.V0 - 0x1u;
            goto L8001F1A0;
        }
        c.V0 = c.V0 - 0x1u;
        if (c.V0 != 0u) {
            MemoryAccess.WriteU32(m, (c.S1 + 0x18u), c.V0);
            goto L8001F1A0;
        }
        MemoryAccess.WriteU32(m, (c.S1 + 0x18u), c.V0);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x3BC4u;
        c.RA = 0x8001F198u;
        GranTurismo2PC.func_8006CE70(c, m);
        MemoryAccess.WriteU16(m, (c.S3 + 0x18u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.S2 + 0x18u), (ushort)0u);
        L8001F1A0: ;
        c.V1 = MemoryAccess.ReadU32(m, (c.S1 + 0x14u));
        if (c.V1 == 0u) {
            c.V0 = 0x00000001u;
            goto L8001F1C0;
        }
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            c.V0 = c.S4 + 0u;
            goto L8001F2B4;
        }
        c.V0 = c.S4 + 0u;
        goto L8001F338;
        L8001F1C0: ;
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x14F0u;
        c.A1 = 0u + 0u;
        c.RA = 0x8001F1D0u;
        GranTurismo2PC.func_8006E43C(c, m);
        c.V0 = 0x80050000u;
        c.S2 = c.V0 - 0x3BC4u;
        c.A0 = c.S2 + 0u;
        c.A1 = c.S0 + 0u;
        c.RA = 0x8001F1E4u;
        GranTurismo2PC.func_8006CFC4(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = 0xFFFFFFFDu;
        if (c.V1 == c.V0) {
            c.V0 = (int)c.V1 < -2 ? 1u : 0u;
            goto L8001F228;
        }
        c.V0 = (int)c.V1 < -2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0xFFFFFFFCu;
            goto L8001F20C;
        }
        c.V0 = 0xFFFFFFFCu;
        if (c.V1 == c.V0) {
            goto L8001F238;
        }
        goto L8001F28C;
        L8001F20C: ;
        c.V0 = 0xFFFFFFFEu;
        if (c.V1 == c.V0) {
            c.V0 = 0xFFFFFFFFu;
            goto L8001F334;
        }
        c.V0 = 0xFFFFFFFFu;
        if (c.V1 == c.V0) {
            goto L8001F248;
        }
        goto L8001F28C;
        L8001F228: ;
        c.A0 = 0x00000006u;
        c.RA = 0x8001F230u;
        GranTurismo2PC.func_80060840(c, m);
        c.V0 = c.S4 + 0u;
        goto L8001F338;
        L8001F238: ;
        c.A0 = 0u + 0u;
        c.RA = 0x8001F240u;
        GranTurismo2PC.func_80060840(c, m);
        c.V0 = c.S4 + 0u;
        goto L8001F338;
        L8001F248: ;
        c.A0 = c.S2 + 0u;
        c.RA = 0x8001F250u;
        GranTurismo2PC.func_8006CED8(c, m);
        c.A0 = 0x00000004u;
        c.S4 = 0x00000002u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x3BFCu;
        c.A2 = 0x80050000u;
        c.A2 = c.A2 - 0x3BE0u;
        c.V0 = MemoryAccess.ReadU16(m, (c.A1 + 0x4u));
        c.V1 = MemoryAccess.ReadU16(m, (c.A2 + 0x4u));
        c.V0 = ~(0u | c.V0);
        c.V1 = ~(0u | c.V1);
        MemoryAccess.WriteU16(m, (c.A1 + 0x18u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.A2 + 0x18u), (ushort)c.V1);
        c.RA = 0x8001F284u;
        GranTurismo2PC.func_80060840(c, m);
        c.V0 = c.S4 + 0u;
        goto L8001F338;
        L8001F28C: ;
        c.A0 = 0x00000001u;
        c.RA = 0x8001F294u;
        GranTurismo2PC.func_80060840(c, m);
        c.S0 = 0x800B0000u;
        c.S0 = c.S0 + 0x14F0u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x8001F2A4u;
        GranTurismo2PC.func_8006E388(c, m);
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x7Du), (byte)c.V0);
        MemoryAccess.WriteU32(m, (c.S1 + 0x14u), c.V0);
        goto L8001F334;
        L8001F2B4: ;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x3BC4u;
        c.A1 = 0u + 0u;
        c.RA = 0x8001F2C4u;
        GranTurismo2PC.func_8006CFC4(c, m);
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x14F0u;
        c.A1 = c.S0 + 0u;
        c.RA = 0x8001F2D4u;
        GranTurismo2PC.func_8006E43C(c, m);
        c.V1 = c.V0 + 0x3u;
        c.V0 = c.V1 < 0x00000005u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80020000u;
            goto L8001F334;
        }
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x3D80u;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x8001F334u: goto L8001F334;
            case 0x8001F300u: goto L8001F300;
            case 0x8001F308u: goto L8001F308;
            case 0x8001F318u: goto L8001F318;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L8001F300: ;
        c.A0 = 0x00000002u;
        goto L8001F31C;
        L8001F308: ;
        c.V0 = 0x80050000u;
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 - 0x3BBEu));
        c.RA = 0x8001F318u;
        GranTurismo2PC.func_8001EB60(c, m);
        L8001F318: ;
        c.A0 = 0x00000001u;
        L8001F31C: ;
        c.RA = 0x8001F324u;
        GranTurismo2PC.func_80060840(c, m);
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x14F0u;
        c.RA = 0x8001F330u;
        GranTurismo2PC.func_8006E3FC(c, m);
        MemoryAccess.WriteU32(m, (c.S1 + 0x14u), 0u);
        L8001F334: ;
        c.V0 = c.S4 + 0u;
        L8001F338: ;
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
    public static void func_8001F358(CpuContext c, IMemory m)
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
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7298u));
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1C4u));
        c.S6 = c.V1 - 0x6720u;
        c.RA = 0x8001F3A4u;
        GranTurismo2PC.func_8006AC68(c, m);
        c.A0 = c.SP + 0x20u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 - 0x6A40u;
        c.RA = 0x8001F3B4u;
        GranTurismo2PC.func_8007DA80(c, m);
        c.V1 = 0xFF9F0000u;
        c.V1 = c.V1 | 0xFFFFu;
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 + 0x14F0u;
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
        c.RA = 0x8001F3F4u;
        GranTurismo2PC.func_8006E5B8(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x3BC4u;
        c.S4 = c.S0 + 0x18u;
        c.A1 = c.S4 + 0u;
        c.RA = 0x8001F408u;
        GranTurismo2PC.func_8006D50C(c, m);
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x14u));
        if (c.V0 == c.S5) {
            c.S0 = c.S6 + 0x3C74u;
            goto L8001F4E0;
        }
        c.S0 = c.S6 + 0x3C74u;
        c.S1 = c.SP + 0x40u;
        c.A1 = MemoryAccess.ReadU32(m, (c.S0 + 0x4014u));
        c.A0 = c.S1 + 0u;
        MemoryAccess.WriteU32(m, (c.S3 + 0x10u), c.S4);
        c.RA = 0x8001F42Cu;
        GranTurismo2PC.func_8001E808(c, m);
        c.V0 = 0x02600000u;
        c.V0 = c.V0 | 0x4214u;
        c.A0 = c.S3 + 0u;
        c.S0 = 0x801C0000u;
        c.S0 = c.S0 - 0x612Bu;
        c.A1 = c.S0 + 0u;
        c.A2 = 0x00000040u;
        c.A3 = 0x000001B8u;
        MemoryAccess.WriteU32(m, (c.S3 + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S5);
        c.RA = 0x8001F458u;
        GranTurismo2PC.func_8006AE28(c, m);
        c.A0 = c.S3 + 0u;
        c.A1 = c.S1 + 0u;
        c.A2 = 0x00000090u;
        c.A3 = 0x000001B8u;
        c.S2 = 0xFFFFFFFEu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.RA = 0x8001F47Cu;
        GranTurismo2PC.func_8006B184(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x3BFCu;
        c.A1 = c.S4 + 0u;
        c.A2 = 0x00000020u;
        c.A3 = 0x000001A4u;
        c.RA = 0x8001F494u;
        GranTurismo2PC.func_8006BEF4(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S0 + 0x4u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S6 + 0x3C74u));
        c.A3 = 0x00000064u;
        c.RA = 0x8001F4A8u;
        GranTurismo2PC.func_8008CF34(c, m);
        c.A0 = c.S3 + 0u;
        c.A1 = c.S1 + 0u;
        c.A2 = 0x00000136u;
        c.A3 = 0x000001B8u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.RA = 0x8001F4C8u;
        GranTurismo2PC.func_8006B184(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 - 0x3BE0u;
        c.A1 = c.S4 + 0u;
        c.A2 = 0x00000146u;
        c.A3 = 0x000001A4u;
        c.RA = 0x8001F4E0u;
        GranTurismo2PC.func_8006BEF4(c, m);
        L8001F4E0: ;
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
    public static void func_8001F508(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V1 = 0x801D0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1588u));
        c.V1 = MemoryAccess.ReadU8(m, (c.V1 - 0x6720u));
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        if (c.V1 != 0u) {
            goto L8001F55C;
        }
        c.A0 = c.S1 + 0u;
        c.RA = 0x8001F53Cu;
        GranTurismo2PC.func_8007F174(c, m);
        c.A0 = c.S1 + 0u;
        c.S0 = 0x801F0000u;
        c.S0 = c.S0 - 0x4C7u;
        c.A1 = c.S0 + 0u;
        c.RA = 0x8001F550u;
        GranTurismo2PC.func_8006ECD8(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S0 + 0x11u;
        c.RA = 0x8001F55Cu;
        GranTurismo2PC.func_8006ECD8(c, m);
        L8001F55C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001F570(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V0 = 0x800B0000u;
        c.V1 = 0x801D0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1588u));
        c.V1 = MemoryAccess.ReadU8(m, (c.V1 - 0x6720u));
        c.A1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        if (c.V1 != 0u) {
            goto L8001F5A0;
        }
        c.RA = 0x8001F5A0u;
        GranTurismo2PC.func_8006ECD8(c, m);
        L8001F5A0: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001F5B0(CpuContext c, IMemory m)
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
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1588u));
        c.V1 = MemoryAccess.ReadU8(m, (c.V1 - 0x6720u));
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S4 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        if (c.V1 != 0u) {
            c.A0 = 0u + 0u;
            goto L8001F6C0;
        }
        c.A0 = 0u + 0u;
        if (c.A2 == 0u) {
            c.S0 = c.A1 + 0u;
            goto L8001F640;
        }
        c.S0 = c.A1 + 0u;
        c.V0 = MemoryAccess.ReadU8(m, c.A1);
        if (c.V0 == 0u) {
            c.V1 = c.A1 + 0u;
            goto L8001F634;
        }
        c.V1 = c.A1 + 0u;
        L8001F620: ;
        c.V1 = c.V1 + 0x2u;
        c.V0 = MemoryAccess.ReadU8(m, c.V1);
        if (c.V0 != 0u) {
            c.A0 = c.A0 + 0xCu;
            goto L8001F620;
        }
        c.A0 = c.A0 + 0xCu;
        L8001F634: ;
        c.V0 = (uint)((int)c.A0 >> 1);
        c.S1 = c.S1 - c.V0;
        c.S0 = c.A1 + 0u;
        L8001F640: ;
        c.S5 = 0x02000000u;
        c.S2 = c.S3 + 0x1Fu;
        L8001F648: ;
        c.V0 = MemoryAccess.ReadU8(m, c.S0);
        if (c.V0 == 0u) {
            c.A0 = c.S4 + 0u;
            goto L8001F6C0;
        }
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.V0 = MemoryAccess.ReadU8(m, (c.S0 + 0x1u));
        c.A1 = c.A1 << 8;
        c.A1 = c.A1 | c.V0;
        c.RA = 0x8001F66Cu;
        GranTurismo2PC.func_8007F18C(c, m);
        if (c.V0 == 0u) {
            c.A0 = c.V0 + 0u;
            goto L8001F6B4;
        }
        c.A0 = c.V0 + 0u;
        c.A1 = c.S7 + 0u;
        c.A2 = c.S6 | c.S5;
        c.RA = 0x8001F680u;
        GranTurismo2PC.func_8007F01C(c, m);
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
        L8001F6B4: ;
        c.S1 = c.S1 + 0xCu;
        c.S0 = c.S0 + 0x2u;
        goto L8001F648;
        L8001F6C0: ;
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
    public static void func_8001F6EC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V1 = 0x801D0000u;
        c.V1 = MemoryAccess.ReadU8(m, (c.V1 - 0x6720u));
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1588u));
        if (c.V1 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
            goto L8001F72C;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.RA = 0x8001F714u;
        GranTurismo2PC.func_8001F508(c, m);
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x28u));
        c.RA = 0x8001F720u;
        GranTurismo2PC.func_8001F570(c, m);
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x2Cu));
        c.RA = 0x8001F72Cu;
        GranTurismo2PC.func_8001F570(c, m);
        L8001F72C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001F73C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V1 = 0x801D0000u;
        c.V1 = MemoryAccess.ReadU8(m, (c.V1 - 0x6720u));
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1588u));
        if (c.V1 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
            goto L8001F770;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.RA = 0x8001F764u;
        GranTurismo2PC.func_8001F508(c, m);
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x24u));
        c.RA = 0x8001F770u;
        GranTurismo2PC.func_8001F570(c, m);
        L8001F770: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001F780(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1588u));
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x344u));
        if (c.A0 == c.S3) {
            c.S2 = 0xFFFFFFFFu;
            goto L8001F7F4;
        }
        c.S2 = 0xFFFFFFFFu;
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L8001F87C;
        }
        c.V0 = c.S2 + 0u;
        if (c.A0 != 0u) {
            c.S0 = c.S1 + 0x2B0u;
            goto L8001F87C;
        }
        c.S0 = c.S1 + 0x2B0u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x8001F7CCu;
        GranTurismo2PC.func_8006E388(c, m);
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0x780u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x7Du), (byte)c.S3);
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x3D98u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x2Cu), c.V0);
        c.RA = 0x8001F7ECu;
        GranTurismo2PC.func_8001F6EC(c, m);
        c.V0 = c.S2 + 0u;
        goto L8001F87C;
        L8001F7F4: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x10u));
        c.V1 = MemoryAccess.ReadU32(m, (c.S1 + 0xCu));
        c.V0 = c.V0 << 1;
        c.V1 = c.V1 + c.V0;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.V1);
        if (c.V0 == 0u) {
            goto L8001F824;
        }
        c.A0 = c.S1 + 0x2B0u;
        c.RA = 0x8001F81Cu;
        GranTurismo2PC.func_8006E3FC(c, m);
        c.S2 = 0x00000003u;
        goto L8001F878;
        L8001F824: ;
        if (c.A1 == 0u) {
            goto L8001F85C;
        }
        if ((int)c.A1 > 0) {
            goto L8001F844;
        }
        if (c.A1 == c.S2) {
            c.V0 = c.S2 + 0u;
            goto L8001F854;
        }
        c.V0 = c.S2 + 0u;
        goto L8001F87C;
        L8001F844: ;
        if (c.A1 == c.A0) {
            c.V0 = c.S2 + 0u;
            goto L8001F86C;
        }
        c.V0 = c.S2 + 0u;
        goto L8001F87C;
        L8001F854: ;
        c.A0 = 0x00000002u;
        goto L8001F870;
        L8001F85C: ;
        c.A0 = 0x00000001u;
        c.RA = 0x8001F864u;
        GranTurismo2PC.func_80060840(c, m);
        c.S2 = 0x00000007u;
        goto L8001F878;
        L8001F86C: ;
        c.A0 = 0x00000001u;
        L8001F870: ;
        c.S2 = 0x00000002u;
        c.RA = 0x8001F878u;
        GranTurismo2PC.func_80060840(c, m);
        L8001F878: ;
        c.V0 = c.S2 + 0u;
        L8001F87C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001F898(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = 0xFFFFFFFFu;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1588u));
        c.V0 = 0x00000001u;
        if (c.S2 == c.V0) {
            MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
            goto L8001F954;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        c.V0 = (int)c.S2 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L8001F8E0;
        }
        c.V0 = 0x00000002u;
        if (c.S2 == 0u) {
            c.V0 = c.S1 + 0u;
            goto L8001F8F0;
        }
        c.V0 = c.S1 + 0u;
        goto L8001FA0C;
        L8001F8E0: ;
        if (c.S2 == c.V0) {
            c.V0 = c.S1 + 0u;
            goto L8001FA00;
        }
        c.V0 = c.S1 + 0u;
        goto L8001FA0C;
        L8001F8F0: ;
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x418u));
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x10u));
        c.A2 = c.A0 + 0u;
        c.RA = 0x8001F900u;
        GranTurismo2PC.func_8006A1B4(c, m);
        if (c.V0 == 0u) {
            c.V0 = 0x801F0000u;
            goto L8001F910;
        }
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0x741u;
        goto L8001F9F0;
        L8001F910: ;
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x418u));
        c.RA = 0x8001F91Cu;
        GranTurismo2PC.func_8006A12C(c, m);
        c.V1 = 0x00900000u;
        c.V1 = c.V1 | 0x500Cu;
        c.A0 = c.S0 + 0x3E0u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x10u), c.V0);
        MemoryAccess.WriteU32(m, (c.A0 + 0xCu), c.V1);
        c.RA = 0x8001F934u;
        GranTurismo2PC.func_8006C04C(c, m);
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0x76Bu;
        MemoryAccess.WriteU32(m, (c.S0 + 0x28u), c.V0);
        c.V0 = c.V0 - 0x151u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x2Cu), c.V0);
        c.RA = 0x8001F94Cu;
        GranTurismo2PC.func_8001F6EC(c, m);
        c.V0 = c.S1 + 0u;
        goto L8001FA0C;
        L8001F954: ;
        c.RA = 0x8001F95Cu;
        GranTurismo2PC.func_8007D7CC(c, m);
        if (c.V0 == 0u) {
            goto L8001F984;
        }
        if (c.V0 != c.S2) {
            c.V0 = 0x801F0000u;
            goto L8001F9E8;
        }
        c.V0 = 0x801F0000u;
        c.V0 = 0x801F0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0x6D8u));
        c.A0 = c.S0 + 0x3E0u;
        c.RA = 0x8001F97Cu;
        GranTurismo2PC.func_8006C074(c, m);
        c.V0 = c.S1 + 0u;
        goto L8001FA0C;
        L8001F984: ;
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x418u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x414u), (ushort)c.S1);
        c.A1 = c.A0 + 0u;
        c.RA = 0x8001F994u;
        GranTurismo2PC.func_8006A314(c, m);
        if (c.V0 == 0u) {
            c.V0 = 0x801F0000u;
            goto L8001F9E0;
        }
        c.V0 = 0x801F0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x418u));
        c.A1 = c.A0 + 0u;
        c.RA = 0x8001F9A8u;
        GranTurismo2PC.func_8006A2E4(c, m);
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, c.S0);
        if (c.A0 == 0u) {
            goto L8001F9C8;
        }
        if (c.A0 != c.S2) {
            c.V0 = c.S1 + 0u;
            goto L8001FA0C;
        }
        c.V0 = c.S1 + 0u;
        c.S1 = 0x00000008u;
        goto L8001FA08;
        L8001F9C8: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.V0);
        if ((int)c.V0 <= 0) {
            c.S1 = 0x00000009u;
            goto L8001FA08;
        }
        c.S1 = 0x00000009u;
        c.S1 = 0x00000008u;
        goto L8001FA08;
        L8001F9E0: ;
        c.V0 = c.V0 - 0x641u;
        goto L8001F9F0;
        L8001F9E8: ;
        c.V0 = c.V0 - 0x741u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x414u), (ushort)c.S1);
        L8001F9F0: ;
        MemoryAccess.WriteU32(m, (c.S0 + 0x24u), c.V0);
        c.RA = 0x8001F9F8u;
        GranTurismo2PC.func_8001F73C(c, m);
        c.S1 = 0x00000001u;
        goto L8001FA08;
        L8001FA00: ;
        c.A0 = c.S0 + 0x3E0u;
        c.RA = 0x8001FA08u;
        GranTurismo2PC.func_8006C174(c, m);
        L8001FA08: ;
        c.V0 = c.S1 + 0u;
        L8001FA0C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001FA24(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1588u));
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x2ACu));
        if (c.A0 == c.S2) {
            c.S3 = 0xFFFFFFFFu;
            goto L8001FAC4;
        }
        c.S3 = 0xFFFFFFFFu;
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S3 + 0u;
            goto L8001FB04;
        }
        c.V0 = c.S3 + 0u;
        if (c.A0 != 0u) {
            c.S0 = c.S1 + 0x218u;
            goto L8001FB04;
        }
        c.S0 = c.S1 + 0x218u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x8001FA70u;
        GranTurismo2PC.func_8006E388(c, m);
        MemoryAccess.WriteU8(m, (c.S0 + 0x7Du), (byte)c.S2);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, c.S1);
        if (c.V1 == c.S2) {
            c.V0 = 0x801C0000u;
            goto L8001FA98;
        }
        c.V0 = 0x801C0000u;
        c.V0 = 0x00000002u;
        if (c.V1 == c.V0) {
            c.V0 = c.S3 + 0u;
            goto L8001FAA0;
        }
        c.V0 = c.S3 + 0u;
        goto L8001FB04;
        L8001FA98: ;
        c.V0 = c.V0 - 0x61CDu;
        goto L8001FAA8;
        L8001FAA0: ;
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x61AFu;
        L8001FAA8: ;
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x3D98u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x2Cu), c.V0);
        c.RA = 0x8001FABCu;
        GranTurismo2PC.func_8001F6EC(c, m);
        c.V0 = c.S3 + 0u;
        goto L8001FB04;
        L8001FAC4: ;
        if (c.V0 == 0u) {
            goto L8001FAF4;
        }
        if ((int)c.V0 > 0) {
            goto L8001FAE4;
        }
        if (c.V0 == c.S3) {
            c.V0 = c.S3 + 0u;
            goto L8001FAEC;
        }
        c.V0 = c.S3 + 0u;
        goto L8001FB04;
        L8001FAE4: ;
        if (c.V0 != c.A0) {
            c.V0 = c.S3 + 0u;
            goto L8001FB04;
        }
        c.V0 = c.S3 + 0u;
        L8001FAEC: ;
        c.S3 = 0x0000000Du;
        goto L8001FB00;
        L8001FAF4: ;
        c.A0 = 0x00000001u;
        c.RA = 0x8001FAFCu;
        GranTurismo2PC.func_80060840(c, m);
        c.S3 = 0x00000002u;
        L8001FB00: ;
        c.V0 = c.S3 + 0u;
        L8001FB04: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001FB20(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1588u));
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x3DCu));
        if (c.A0 == c.S3) {
            c.S2 = 0xFFFFFFFFu;
            goto L8001FBA0;
        }
        c.S2 = 0xFFFFFFFFu;
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L8001FBE0;
        }
        c.V0 = c.S2 + 0u;
        if (c.A0 != 0u) {
            goto L8001FBE0;
        }
        c.A0 = 0u + 0u;
        c.RA = 0x8001FB6Cu;
        GranTurismo2PC.func_80060840(c, m);
        c.S0 = c.S1 + 0x348u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x8001FB78u;
        GranTurismo2PC.func_8006E388(c, m);
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x620Au;
        MemoryAccess.WriteU8(m, (c.S0 + 0x7Du), (byte)c.S3);
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x3D98u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x2Cu), c.V0);
        c.RA = 0x8001FB98u;
        GranTurismo2PC.func_8001F6EC(c, m);
        c.V0 = c.S2 + 0u;
        goto L8001FBE0;
        L8001FBA0: ;
        if (c.V0 == 0u) {
            goto L8001FBD0;
        }
        if ((int)c.V0 > 0) {
            goto L8001FBC0;
        }
        if (c.V0 == c.S2) {
            c.V0 = c.S2 + 0u;
            goto L8001FBC8;
        }
        c.V0 = c.S2 + 0u;
        goto L8001FBE0;
        L8001FBC0: ;
        if (c.V0 != c.A0) {
            c.V0 = c.S2 + 0u;
            goto L8001FBE0;
        }
        c.V0 = c.S2 + 0u;
        L8001FBC8: ;
        c.S2 = 0x0000000Du;
        goto L8001FBDC;
        L8001FBD0: ;
        c.A0 = 0x00000001u;
        c.RA = 0x8001FBD8u;
        GranTurismo2PC.func_80060840(c, m);
        c.S2 = 0x00000002u;
        L8001FBDC: ;
        c.V0 = c.S2 + 0u;
        L8001FBE0: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001FBFC(CpuContext c, IMemory m)
    {
        c.T1 = 0u | 0xAAAAu;
        c.V1 = 0x00003770u;
        if (c.A1 == 0u) {
            c.T0 = 0u + 0u;
            goto L8001FC6C;
        }
        c.T0 = 0u + 0u;
        c.T3 = 0x00010000u;
        c.T2 = c.T3 + 0u;
        c.T2 = c.T2 | 0x1021u;
        L8001FC18: ;
        c.V0 = c.A0 + c.T0;
        c.A2 = MemoryAccess.ReadU8(m, c.V0);
        c.A3 = 0u + 0u;
        c.T1 = c.T1 + c.A2;
        c.V0 = c.A2 << 8;
        c.T1 = c.T1 ^ c.V0;
        L8001FC30: ;
        c.V1 = c.V1 << 1;
        c.V0 = c.V1 & c.T3;
        if (c.V0 == 0u) {
            c.V0 = c.A2 >> 7;
            goto L8001FC44;
        }
        c.V0 = c.A2 >> 7;
        c.V1 = c.V1 ^ c.T2;
        L8001FC44: ;
        c.V0 = c.V0 & 0x0001u;
        c.V1 = c.V1 | c.V0;
        c.A3 = c.A3 + 0x1u;
        c.V0 = c.A3 < 0x00000008u ? 1u : 0u;
        if (c.V0 != 0u) {
            c.A2 = c.A2 << 1;
            goto L8001FC30;
        }
        c.A2 = c.A2 << 1;
        c.T0 = c.T0 + 0x1u;
        c.V0 = c.T0 < c.A1 ? 1u : 0u;
        if (c.V0 != 0u) {
            goto L8001FC18;
        }
        L8001FC6C: ;
        c.V1 = c.V1 & 0xFFFFu;
        c.V0 = c.T1 << 16;
        c.V0 = c.V1 | c.V0;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001FC7C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.A1 = 0x00006BA4u;
        c.RA = 0x8001FC94u;
        GranTurismo2PC.func_8001FBFC(c, m);
        c.V1 = MemoryAccess.ReadU32(m, (c.S0 + 0x6BA4u));
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V1 = c.V1 ^ c.V0;
        c.V0 = c.V1 < 0x00000001u ? 1u : 0u;
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001FCB0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1588u));
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x344u));
        if (c.A0 == c.S3) {
            c.S2 = 0xFFFFFFFFu;
            goto L8001FD24;
        }
        c.S2 = 0xFFFFFFFFu;
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L8001FDAC;
        }
        c.V0 = c.S2 + 0u;
        if (c.A0 != 0u) {
            c.S0 = c.S1 + 0x2B0u;
            goto L8001FDAC;
        }
        c.S0 = c.S1 + 0x2B0u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x8001FCFCu;
        GranTurismo2PC.func_8006E388(c, m);
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0x780u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x7Du), (byte)c.S3);
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x3D98u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x2Cu), c.V0);
        c.RA = 0x8001FD1Cu;
        GranTurismo2PC.func_8001F6EC(c, m);
        c.V0 = c.S2 + 0u;
        goto L8001FDAC;
        L8001FD24: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x10u));
        c.V1 = MemoryAccess.ReadU32(m, (c.S1 + 0xCu));
        c.V0 = c.V0 << 1;
        c.V1 = c.V1 + c.V0;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.V1);
        if (c.V0 == 0u) {
            goto L8001FD54;
        }
        c.A0 = c.S1 + 0x2B0u;
        c.RA = 0x8001FD4Cu;
        GranTurismo2PC.func_8006E3FC(c, m);
        c.S2 = 0x00000003u;
        goto L8001FDA8;
        L8001FD54: ;
        if (c.A1 == 0u) {
            goto L8001FD8C;
        }
        if ((int)c.A1 > 0) {
            goto L8001FD74;
        }
        if (c.A1 == c.S2) {
            c.V0 = c.S2 + 0u;
            goto L8001FD84;
        }
        c.V0 = c.S2 + 0u;
        goto L8001FDAC;
        L8001FD74: ;
        if (c.A1 == c.A0) {
            c.V0 = c.S2 + 0u;
            goto L8001FD9C;
        }
        c.V0 = c.S2 + 0u;
        goto L8001FDAC;
        L8001FD84: ;
        c.A0 = 0x00000002u;
        goto L8001FDA0;
        L8001FD8C: ;
        c.A0 = 0x00000001u;
        c.RA = 0x8001FD94u;
        GranTurismo2PC.func_80060840(c, m);
        c.S2 = 0x0000000Bu;
        goto L8001FDA8;
        L8001FD9C: ;
        c.A0 = 0x00000001u;
        L8001FDA0: ;
        c.S2 = 0x00000002u;
        c.RA = 0x8001FDA8u;
        GranTurismo2PC.func_80060840(c, m);
        L8001FDA8: ;
        c.V0 = c.S2 + 0u;
        L8001FDAC: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001FDC8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x30u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S2);
        c.S2 = 0xFFFFFFFFu;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1588u));
        c.V0 = 0x00000001u;
        if (c.S1 == c.V0) {
            MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.RA);
            goto L8001FE90;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.RA);
        c.V0 = (int)c.S1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L8001FE10;
        }
        c.V0 = 0x00000002u;
        if (c.S1 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L8001FE20;
        }
        c.V0 = c.S2 + 0u;
        goto L8001FF04;
        L8001FE10: ;
        if (c.S1 == c.V0) {
            c.V0 = c.S2 + 0u;
            goto L8001FEF8;
        }
        c.V0 = c.S2 + 0u;
        goto L8001FF04;
        L8001FE20: ;
        c.A0 = 0x00000006u;
        c.S1 = 0u | 0xA000u;
        c.A2 = 0x80050000u;
        c.A2 = c.A2 - 0x3914u;
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x10u));
        c.A3 = c.S0 + 0x41Cu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.RA = 0x8001FE48u;
        GranTurismo2PC.func_8007D658(c, m);
        if (c.V0 == 0u) {
            c.V0 = 0x801F0000u;
            goto L8001FE58;
        }
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0x741u;
        goto L8001FEE8;
        L8001FE58: ;
        c.V0 = 0x00900000u;
        c.V0 = c.V0 | 0x500Cu;
        c.A0 = c.S0 + 0x3E0u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x10u), c.S1);
        MemoryAccess.WriteU32(m, (c.A0 + 0xCu), c.V0);
        c.RA = 0x8001FE70u;
        GranTurismo2PC.func_8006C04C(c, m);
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0x76Bu;
        MemoryAccess.WriteU32(m, (c.S0 + 0x28u), c.V0);
        c.V0 = c.V0 - 0x151u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x2Cu), c.V0);
        c.RA = 0x8001FE88u;
        GranTurismo2PC.func_8001F6EC(c, m);
        c.V0 = c.S2 + 0u;
        goto L8001FF04;
        L8001FE90: ;
        c.RA = 0x8001FE98u;
        GranTurismo2PC.func_8007D7CC(c, m);
        if (c.V0 == 0u) {
            goto L8001FEC0;
        }
        if (c.V0 != c.S1) {
            c.V0 = 0x801F0000u;
            goto L8001FEE0;
        }
        c.V0 = 0x801F0000u;
        c.V0 = 0x801F0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0x6D8u));
        c.A0 = c.S0 + 0x3E0u;
        c.RA = 0x8001FEB8u;
        GranTurismo2PC.func_8006C074(c, m);
        c.V0 = c.S2 + 0u;
        goto L8001FF04;
        L8001FEC0: ;
        MemoryAccess.WriteU16(m, (c.S0 + 0x414u), (ushort)c.S2);
        c.A0 = c.S0 + 0x61Cu;
        c.RA = 0x8001FECCu;
        GranTurismo2PC.func_8001FC7C(c, m);
        if (c.V0 != 0u) {
            c.S2 = 0x00000008u;
            goto L8001FF00;
        }
        c.S2 = 0x00000008u;
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x5C1Fu;
        goto L8001FEE8;
        L8001FEE0: ;
        c.V0 = c.V0 - 0x741u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x414u), (ushort)c.S2);
        L8001FEE8: ;
        MemoryAccess.WriteU32(m, (c.S0 + 0x24u), c.V0);
        c.RA = 0x8001FEF0u;
        GranTurismo2PC.func_8001F73C(c, m);
        c.S2 = 0x00000001u;
        goto L8001FF00;
        L8001FEF8: ;
        c.A0 = c.S0 + 0x3E0u;
        c.RA = 0x8001FF00u;
        GranTurismo2PC.func_8006C174(c, m);
        L8001FF00: ;
        c.V0 = c.S2 + 0u;
        L8001FF04: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x2Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.SP = c.SP + 0x30u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001FF1C(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1588u));
        c.V0 = 0x00000001u;
        if (c.A0 == c.V0) {
            c.A1 = 0xFFFFFFFFu;
            goto L8001FF4C;
        }
        c.A1 = 0xFFFFFFFFu;
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L8001FF6C;
        }
        if (c.A0 != 0u) {
            c.V0 = 0x00000018u;
            goto L8001FF6C;
        }
        c.V0 = 0x00000018u;
        MemoryAccess.WriteU16(m, (c.V1 + 0x32u), (ushort)c.V0);
        goto L8001FF6C;
        L8001FF4C: ;
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 + 0x32u));
        c.V0 = c.V0 - 0x1u;
        MemoryAccess.WriteU16(m, (c.V1 + 0x32u), (ushort)c.V0);
        c.V0 = c.V0 << 16;
        if ((int)c.V0 >= 0) {
            goto L8001FF6C;
        }
        c.A1 = 0x00000002u;
        L8001FF6C: ;
        c.V0 = c.A1 + 0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001FF74(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x50u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x3Cu), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1588u));
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.S3);
        c.S3 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x38u), c.S0);
        c.S0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x48u), c.RA);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0xE4u));
        if (c.A0 == c.S0) {
            c.S2 = 0xFFFFFFFFu;
            goto L80020008;
        }
        c.S2 = 0xFFFFFFFFu;
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L8001FFC4;
        }
        c.V0 = 0x00000002u;
        if (c.A0 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L8001FFD4;
        }
        c.V0 = c.S2 + 0u;
        goto L800200CC;
        L8001FFC4: ;
        if (c.A0 == c.V0) {
            c.V0 = c.S2 + 0u;
            goto L80020048;
        }
        c.V0 = c.S2 + 0u;
        goto L800200CC;
        L8001FFD4: ;
        c.A0 = 0u + 0u;
        c.RA = 0x8001FFDCu;
        GranTurismo2PC.func_80060840(c, m);
        c.S0 = c.S1 + 0x50u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x8001FFE8u;
        GranTurismo2PC.func_8006E388(c, m);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x3D98u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x7Du), (byte)0u);
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        MemoryAccess.WriteU32(m, (c.S1 + 0x2Cu), c.V0);
        c.RA = 0x80020000u;
        GranTurismo2PC.func_8001F6EC(c, m);
        c.V0 = c.S2 + 0u;
        goto L800200CC;
        L80020008: ;
        if (c.V0 == 0u) {
            goto L80020038;
        }
        if ((int)c.V0 > 0) {
            goto L80020028;
        }
        if (c.V0 == c.S2) {
            c.V0 = c.S2 + 0u;
            goto L80020030;
        }
        c.V0 = c.S2 + 0u;
        goto L800200CC;
        L80020028: ;
        if (c.V0 != c.S0) {
            c.V0 = c.S2 + 0u;
            goto L800200CC;
        }
        c.V0 = c.S2 + 0u;
        L80020030: ;
        c.S2 = 0x0000000Du;
        goto L800200C8;
        L80020038: ;
        c.A0 = 0x00000001u;
        c.RA = 0x80020040u;
        GranTurismo2PC.func_80060840(c, m);
        c.S2 = 0x00000002u;
        goto L800200C8;
        L80020048: ;
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x30u));
        c.A0 = c.SP + 0x18u;
        c.RA = 0x80020054u;
        GranTurismo2PC.func_8006AC68(c, m);
        c.A0 = c.SP + 0x18u;
        c.A1 = 0x800B0000u;
        c.A1 = c.A1 + 0x15A0u;
        c.RA = 0x80020064u;
        GranTurismo2PC.func_8007DA80(c, m);
        c.V1 = 0x006F0000u;
        c.V1 = c.V1 | 0x6F6Fu;
        c.V0 = 0x801D0000u;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6720u));
        c.A0 = c.SP + 0x18u;
        MemoryAccess.WriteU8(m, (c.A0 + 0x1Cu), (byte)c.S0);
        MemoryAccess.WriteU32(m, (c.A0 + 0x10u), c.S3);
        if (c.V0 != 0u) {
            MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.V1);
            goto L800200B4;
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
        c.RA = 0x800200ACu;
        GranTurismo2PC.func_8001F5B0(c, m);
        c.V0 = c.S2 + 0u;
        goto L800200CC;
        L800200B4: ;
        c.A2 = 0x000000B0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x24u));
        c.A3 = 0x000000FAu;
        c.RA = 0x800200C8u;
        GranTurismo2PC.func_8006ADB4(c, m);
        L800200C8: ;
        c.V0 = c.S2 + 0u;
        L800200CC: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x48u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x44u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x40u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x3Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x38u));
        c.SP = c.SP + 0x50u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800200E8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1588u));
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x17Cu));
        if (c.A0 == c.S3) {
            c.S2 = 0xFFFFFFFFu;
            goto L800201F4;
        }
        c.S2 = 0xFFFFFFFFu;
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L8002022C;
        }
        c.V0 = c.S2 + 0u;
        if (c.A0 != 0u) {
            c.V1 = c.S1 + 0x34u;
            goto L8002022C;
        }
        c.V1 = c.S1 + 0x34u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V1 + 0x18u));
        if ((int)c.V0 < 0) {
            goto L8002014C;
        }
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 + 0x4u));
        c.V0 = ~(0u | c.V0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x18u), (ushort)c.V0);
        L8002014C: ;
        c.A0 = c.S1 + 0xE8u;
        c.RA = 0x80020154u;
        GranTurismo2PC.func_8006E388(c, m);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, c.S1);
        if (c.V1 == c.S3) {
            c.V0 = 0x801C0000u;
            goto L800201A8;
        }
        c.V0 = 0x801C0000u;
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L80020180;
        }
        if (c.V1 == 0u) {
            c.V0 = 0x801C0000u;
            goto L80020194;
        }
        c.V0 = 0x801C0000u;
        c.V0 = 0x801F0000u;
        goto L800201D4;
        L80020180: ;
        c.V0 = 0x00000002u;
        if (c.V1 == c.V0) {
            goto L800201BC;
        }
        c.V0 = 0x801F0000u;
        goto L800201D4;
        L80020194: ;
        c.V0 = c.V0 - 0x62F8u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0x8F7u;
        goto L800201E4;
        L800201A8: ;
        c.V0 = c.V0 - 0x62D4u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0x8F7u;
        goto L800201E4;
        L800201BC: ;
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x626Au;
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0x8F7u;
        goto L800201E4;
        L800201D4: ;
        c.V0 = c.V0 - 0x8F7u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x3D98u;
        L800201E4: ;
        MemoryAccess.WriteU32(m, (c.S1 + 0x2Cu), c.V0);
        c.RA = 0x800201ECu;
        GranTurismo2PC.func_8001F6EC(c, m);
        c.V0 = c.S2 + 0u;
        goto L8002022C;
        L800201F4: ;
        if (c.S0 != c.S2) {
            c.V0 = (int)c.S0 < -1 ? 1u : 0u;
            goto L80020204;
        }
        c.V0 = (int)c.S0 < -1 ? 1u : 0u;
        c.S2 = 0x0000000Du;
        goto L80020228;
        L80020204: ;
        if (c.V0 != 0u) {
            c.V0 = c.S2 + 0u;
            goto L8002022C;
        }
        c.V0 = c.S2 + 0u;
        c.V0 = (int)c.S0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L8002022C;
        }
        c.V0 = c.S2 + 0u;
        c.A0 = 0x00000001u;
        c.RA = 0x80020220u;
        GranTurismo2PC.func_80060840(c, m);
        MemoryAccess.WriteU16(m, (c.S1 + 0x10u), (ushort)c.S0);
        c.S2 = 0x00000003u;
        L80020228: ;
        c.V0 = c.S2 + 0u;
        L8002022C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020248(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1588u));
        c.A1 = 0x80050000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x10u));
        c.A1 = c.A1 - 0x3914u;
        c.RA = 0x80020268u;
        GranTurismo2PC.func_8007D3E8(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020278(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1588u));
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = 0xFFFFFFFFu;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = 0x00000001u;
        if (c.A0 == c.S2) {
            MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
            goto L800202D4;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S1 + 0u;
            goto L80020398;
        }
        c.V0 = c.S1 + 0u;
        if (c.A0 != 0u) {
            goto L80020398;
        }
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0x8DAu;
        MemoryAccess.WriteU32(m, (c.S0 + 0x28u), c.V0);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x3D98u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x4Cu), (ushort)0u);
        MemoryAccess.WriteU32(m, (c.S0 + 0x2Cu), c.V0);
        c.RA = 0x800202D4u;
        GranTurismo2PC.func_8001F6EC(c, m);
        L800202D4: ;
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x10u));
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0xCu));
        c.V1 = c.A1 << 1;
        c.V0 = c.V0 + c.V1;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, c.V0);
        if (c.V1 == c.S2) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L80020394;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L8002030C;
        }
        c.V0 = 0x00000002u;
        if (c.V1 == 0u) {
            goto L80020344;
        }
        c.S1 = 0x00000005u;
        goto L80020394;
        L8002030C: ;
        if (c.V1 == c.V0) {
            c.V0 = 0x00000004u;
            goto L80020324;
        }
        c.V0 = 0x00000004u;
        if (c.V1 == c.V0) {
            c.S1 = 0x00000005u;
            goto L8002032C;
        }
        c.S1 = 0x00000005u;
        c.V0 = c.S1 + 0u;
        goto L80020398;
        L80020324: ;
        c.S1 = 0x00000004u;
        goto L80020394;
        L8002032C: ;
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0x874u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x24u), c.V0);
        c.RA = 0x8002033Cu;
        GranTurismo2PC.func_8001F73C(c, m);
        c.S1 = 0x00000001u;
        goto L80020394;
        L80020344: ;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, c.S0);
        if ((int)c.V1 < 0) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L80020394;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = 0x00000002u;
            goto L8002036C;
        }
        c.V0 = 0x00000002u;
        if (c.V1 == c.V0) {
            c.V0 = c.S1 + 0u;
            goto L80020380;
        }
        c.V0 = c.S1 + 0u;
        goto L80020398;
        L8002036C: ;
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x418u));
        c.S1 = 0x00000006u;
        c.RA = 0x80020378u;
        GranTurismo2PC.func_8006A0D4(c, m);
        goto L80020388;
        L80020380: ;
        c.S1 = 0x0000000Au;
        c.RA = 0x80020388u;
        GranTurismo2PC.func_80020248(c, m);
        L80020388: ;
        if (c.V0 != 0u) {
            c.V0 = c.S1 + 0u;
            goto L80020398;
        }
        c.V0 = c.S1 + 0u;
        c.S1 = 0x00000005u;
        L80020394: ;
        c.V0 = c.S1 + 0u;
        L80020398: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800203B0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1588u));
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x214u));
        if (c.A0 == c.V0) {
            c.S1 = 0xFFFFFFFFu;
            goto L80020418;
        }
        c.S1 = 0xFFFFFFFFu;
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S1 + 0u;
            goto L80020488;
        }
        c.V0 = c.S1 + 0u;
        if (c.A0 != 0u) {
            goto L80020488;
        }
        c.A0 = c.S0 + 0x180u;
        c.RA = 0x800203F4u;
        GranTurismo2PC.func_8006E388(c, m);
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0x891u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x28u), c.V0);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x3D98u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x2Cu), c.V0);
        c.RA = 0x80020410u;
        GranTurismo2PC.func_8001F6EC(c, m);
        c.V0 = c.S1 + 0u;
        goto L80020488;
        L80020418: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x10u));
        c.V1 = MemoryAccess.ReadU32(m, (c.S0 + 0xCu));
        c.V0 = c.V0 << 1;
        c.V1 = c.V1 + c.V0;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, c.V1);
        c.V0 = 0x00000002u;
        if (c.V1 == c.V0) {
            goto L80020448;
        }
        c.A0 = c.S0 + 0x180u;
        c.RA = 0x80020440u;
        GranTurismo2PC.func_8006E3FC(c, m);
        c.S1 = 0x00000003u;
        goto L80020484;
        L80020448: ;
        if (c.A1 == 0u) {
            goto L80020478;
        }
        if ((int)c.A1 > 0) {
            goto L80020468;
        }
        if (c.A1 == c.S1) {
            c.V0 = c.S1 + 0u;
            goto L80020470;
        }
        c.V0 = c.S1 + 0u;
        goto L80020488;
        L80020468: ;
        if (c.A1 != c.A0) {
            c.V0 = c.S1 + 0u;
            goto L80020488;
        }
        c.V0 = c.S1 + 0u;
        L80020470: ;
        c.S1 = 0x0000000Du;
        goto L80020484;
        L80020478: ;
        c.A0 = 0x00000001u;
        c.RA = 0x80020480u;
        GranTurismo2PC.func_80060840(c, m);
        c.S1 = 0x00000002u;
        L80020484: ;
        c.V0 = c.S1 + 0u;
        L80020488: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8002049C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1588u));
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0xE4u));
        if (c.A0 == c.V0) {
            c.S2 = 0xFFFFFFFFu;
            goto L80020530;
        }
        c.S2 = 0xFFFFFFFFu;
        c.V0 = (int)c.A0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L800205A8;
        }
        c.V0 = c.S2 + 0u;
        if (c.A0 != 0u) {
            goto L800205A8;
        }
        c.A0 = 0u + 0u;
        c.RA = 0x800204E4u;
        GranTurismo2PC.func_80060840(c, m);
        c.S0 = c.S1 + 0x50u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x800204F0u;
        GranTurismo2PC.func_8006E388(c, m);
        MemoryAccess.WriteU8(m, (c.S0 + 0x7Du), (byte)0u);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, c.S1);
        c.V0 = 0x00000002u;
        if (c.V1 != c.V0) {
            c.V0 = 0x801C0000u;
            goto L8002050C;
        }
        c.V0 = 0x801C0000u;
        c.V0 = c.V0 - 0x6237u;
        goto L80020514;
        L8002050C: ;
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0x669u;
        L80020514: ;
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x3D98u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x2Cu), c.V0);
        c.RA = 0x80020528u;
        GranTurismo2PC.func_8001F6EC(c, m);
        c.V0 = c.S2 + 0u;
        goto L800205A8;
        L80020530: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x10u));
        c.V1 = MemoryAccess.ReadU32(m, (c.S1 + 0xCu));
        c.V0 = c.V0 << 1;
        c.V1 = c.V1 + c.V0;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, c.V1);
        c.V0 = 0x00000002u;
        if (c.V1 == c.V0) {
            c.V0 = 0x00000004u;
            goto L80020558;
        }
        c.V0 = 0x00000004u;
        if (c.V1 != c.V0) {
            goto L80020568;
        }
        L80020558: ;
        c.A0 = c.S1 + 0x50u;
        c.RA = 0x80020560u;
        GranTurismo2PC.func_8006E3FC(c, m);
        c.S2 = 0x00000003u;
        goto L800205A4;
        L80020568: ;
        if (c.A1 == 0u) {
            goto L80020598;
        }
        if ((int)c.A1 > 0) {
            goto L80020588;
        }
        if (c.A1 == c.S2) {
            c.V0 = c.S2 + 0u;
            goto L80020590;
        }
        c.V0 = c.S2 + 0u;
        goto L800205A8;
        L80020588: ;
        if (c.A1 != c.A0) {
            c.V0 = c.S2 + 0u;
            goto L800205A8;
        }
        c.V0 = c.S2 + 0u;
        L80020590: ;
        c.S2 = 0x0000000Du;
        goto L800205A4;
        L80020598: ;
        c.A0 = 0x00000001u;
        c.RA = 0x800205A0u;
        GranTurismo2PC.func_80060840(c, m);
        c.S2 = 0x00000002u;
        L800205A4: ;
        c.V0 = c.S2 + 0u;
        L800205A8: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800205C0(CpuContext c, IMemory m)
    {
        c.V0 = 0xFFFFFFFFu;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800205C8(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1588u));
        c.SP = c.SP - 0x18u;
        if ((int)c.A0 >= 0) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
            goto L800205E8;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x5C0u;
        goto L80020600;
        L800205E8: ;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 - 0x3788u;
        c.V1 = c.A0 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        L80020600: ;
        MemoryAccess.WriteU32(m, (c.A1 + 0x4u), c.V0);
        MemoryAccess.WriteU16(m, (c.A1 + 0x2u), (ushort)c.A0);
        c.A0 = 0u + 0u;
        c.V0 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.A1 = c.A0 + 0u;
        c.RA = 0x8002061Cu;
        Dispatcher.Call(c, m, c.V0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8002062C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x38E8u;
        c.V0 = 0x800B0000u;
        c.A2 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1588u));
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.A0 = c.S1 + 0x50u;
        c.T2 = c.V0 - 0x3904u;
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
        c.RA = 0x800206A4u;
        GranTurismo2PC.func_8006E1CC(c, m);
        c.S0 = c.S1 + 0xE8u;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x38B8u;
        c.A2 = 0u + 0u;
        c.RA = 0x800206BCu;
        GranTurismo2PC.func_8006E1CC(c, m);
        c.A0 = c.S1 + 0x180u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x3888u;
        c.A2 = 0u + 0u;
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x7Du), (byte)c.V0);
        c.RA = 0x800206D8u;
        GranTurismo2PC.func_8006E1CC(c, m);
        c.A0 = c.S1 + 0x218u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x3858u;
        c.A2 = 0u + 0u;
        c.RA = 0x800206ECu;
        GranTurismo2PC.func_8006E1CC(c, m);
        c.A0 = c.S1 + 0x348u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x3828u;
        c.A2 = 0u + 0u;
        c.RA = 0x80020700u;
        GranTurismo2PC.func_8006E1CC(c, m);
        c.V1 = c.S1 + 0x3E0u;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 - 0x37C8u;
        c.A0 = c.V0 + 0x30u;
        L80020710: ;
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
            goto L80020710;
        }
        c.V1 = c.V1 + 0x10u;
        c.A0 = c.S1 + 0x2B0u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 - 0x37F8u;
        c.A2 = 0u + 0u;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        MemoryAccess.WriteU32(m, c.V1, c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x4u), c.T0);
        c.RA = 0x80020760u;
        GranTurismo2PC.func_8006E1CC(c, m);
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x3D98u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        MemoryAccess.WriteU32(m, (c.S1 + 0x2Cu), c.V0);
        c.RA = 0x80020774u;
        GranTurismo2PC.func_8001F6EC(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020788(CpuContext c, IMemory m)
    {
        c.V0 = MemoryAccess.ReadU32(m, c.A0);
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0xCu));
        c.V1 = 0x800B0000u;
        c.T1 = c.V1 + 0x15B0u;
        c.A2 = MemoryAccess.ReadU32(m, c.A1);
        c.A3 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.T0 = MemoryAccess.ReadU32(m, (c.A1 + 0x8u));
        MemoryAccess.WriteU32(m, c.T1, c.A2);
        MemoryAccess.WriteU32(m, (c.T1 + 0x4u), c.A3);
        MemoryAccess.WriteU32(m, (c.T1 + 0x8u), c.T0);
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0x10u));
        c.V1 = 0x800B0000u;
        c.T1 = c.V1 + 0x1590u;
        c.A2 = MemoryAccess.ReadU32(m, c.A1);
        c.A3 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.T0 = MemoryAccess.ReadU32(m, (c.A1 + 0x8u));
        MemoryAccess.WriteU32(m, c.T1, c.A2);
        MemoryAccess.WriteU32(m, (c.T1 + 0x4u), c.A3);
        MemoryAccess.WriteU32(m, (c.T1 + 0x8u), c.T0);
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0x14u));
        c.V1 = 0x800B0000u;
        c.T1 = c.V1 + 0x15A0u;
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
        MemoryAccess.WriteU32(m, (c.V1 + 0x1588u), c.V0);
        MemoryAccess.WriteU16(m, (c.V0 + 0x30u), (ushort)c.A0);
        c.V0 = c.V0 + c.A1;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020828(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1588u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU16(m, c.V0, (ushort)c.A0);
        c.A0 = c.V0 + 0x41Cu;
        MemoryAccess.WriteU32(m, (c.V0 + 0x418u), c.A0);
        c.RA = 0x80020848u;
        GranTurismo2PC.func_8006A038(c, m);
        c.RA = 0x80020850u;
        GranTurismo2PC.func_8002062C(c, m);
        c.A0 = 0u + 0u;
        c.RA = 0x80020858u;
        GranTurismo2PC.func_800205C8(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020868(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1588u));
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        c.A0 = c.S1 + 0x34u;
        c.RA = 0x80020894u;
        GranTurismo2PC.func_8006BE64(c, m);
        c.A0 = c.S1 + 0x50u;
        c.A1 = c.S0 + 0u;
        c.RA = 0x800208A0u;
        GranTurismo2PC.func_8006E43C(c, m);
        c.A0 = c.S1 + 0xE8u;
        c.A1 = c.S0 + 0u;
        MemoryAccess.WriteU16(m, (c.S1 + 0xE4u), (ushort)c.V0);
        c.RA = 0x800208B0u;
        GranTurismo2PC.func_8006E43C(c, m);
        c.A0 = c.S1 + 0x180u;
        c.A1 = c.S0 + 0u;
        MemoryAccess.WriteU16(m, (c.S1 + 0x17Cu), (ushort)c.V0);
        c.RA = 0x800208C0u;
        GranTurismo2PC.func_8006E43C(c, m);
        c.A0 = c.S1 + 0x218u;
        c.A1 = c.S0 + 0u;
        MemoryAccess.WriteU16(m, (c.S1 + 0x214u), (ushort)c.V0);
        c.RA = 0x800208D0u;
        GranTurismo2PC.func_8006E43C(c, m);
        c.A0 = c.S1 + 0x348u;
        c.A1 = c.S0 + 0u;
        MemoryAccess.WriteU16(m, (c.S1 + 0x2ACu), (ushort)c.V0);
        c.RA = 0x800208E0u;
        GranTurismo2PC.func_8006E43C(c, m);
        c.A0 = c.S1 + 0x2B0u;
        c.A1 = c.S0 + 0u;
        MemoryAccess.WriteU16(m, (c.S1 + 0x3DCu), (ushort)c.V0);
        c.RA = 0x800208F0u;
        GranTurismo2PC.func_8006E43C(c, m);
        c.A0 = 0x00000001u;
        c.V1 = MemoryAccess.ReadU32(m, (c.S1 + 0x4u));
        c.A1 = c.S2 + 0u;
        MemoryAccess.WriteU16(m, (c.S1 + 0x344u), (ushort)c.V0);
        c.RA = 0x80020904u;
        Dispatcher.Call(c, m, c.V1);
        c.A0 = c.V0 + 0u;
        L80020908: ;
        c.V0 = 0x00000008u;
        if (c.A0 == c.V0) {
            c.V0 = (int)c.A0 < 9 ? 1u : 0u;
            goto L80020940;
        }
        c.V0 = (int)c.A0 < 9 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0xFFFFFFFFu;
            goto L8002092C;
        }
        c.V0 = 0xFFFFFFFFu;
        if (c.A0 == c.V0) {
            c.V0 = c.S2 + 0u;
            goto L80020A24;
        }
        c.V0 = c.S2 + 0u;
        goto L80020A10;
        L8002092C: ;
        c.V0 = 0x0000000Du;
        if (c.A0 == c.V0) {
            c.V1 = c.S1 + 0x34u;
            goto L800209CC;
        }
        c.V1 = c.S1 + 0x34u;
        goto L80020A10;
        L80020940: ;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, c.S1);
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L800209B8;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L80020968;
        }
        c.V0 = 0x00000002u;
        if (c.V1 == 0u) {
            c.V0 = c.S2 + 0u;
            goto L80020978;
        }
        c.V0 = c.S2 + 0u;
        goto L80020A24;
        L80020968: ;
        if (c.V1 == c.V0) {
            c.V0 = c.S2 + 0u;
            goto L800209B8;
        }
        c.V0 = c.S2 + 0u;
        goto L80020A24;
        L80020978: ;
        c.V1 = c.S1 + 0x34u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V1 + 0x18u));
        if ((int)c.V0 < 0) {
            c.V0 = 0x80020000u;
            goto L800209A0;
        }
        c.V0 = 0x80020000u;
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 + 0x4u));
        c.V0 = ~(0u | c.V0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x18u), (ushort)c.V0);
        c.V0 = 0x80020000u;
        L800209A0: ;
        c.V0 = c.V0 + 0x3D98u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        MemoryAccess.WriteU32(m, (c.S1 + 0x2Cu), c.V0);
        c.RA = 0x800209B0u;
        GranTurismo2PC.func_8001F6EC(c, m);
        c.A0 = 0xFFFFFFFFu;
        goto L800209BC;
        L800209B8: ;
        c.A0 = 0x00000008u;
        L800209BC: ;
        c.S2 = 0x00000002u;
        c.RA = 0x800209C4u;
        GranTurismo2PC.func_800205C8(c, m);
        c.V0 = c.S2 + 0u;
        goto L80020A24;
        L800209CC: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V1 + 0x18u));
        if ((int)c.V0 < 0) {
            c.V0 = 0x80020000u;
            goto L800209F0;
        }
        c.V0 = 0x80020000u;
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 + 0x4u));
        c.V0 = ~(0u | c.V0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x18u), (ushort)c.V0);
        c.V0 = 0x80020000u;
        L800209F0: ;
        c.V0 = c.V0 + 0x3D98u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x28u), c.V0);
        MemoryAccess.WriteU32(m, (c.S1 + 0x2Cu), c.V0);
        c.RA = 0x80020A00u;
        GranTurismo2PC.func_8001F6EC(c, m);
        c.A0 = 0xFFFFFFFFu;
        c.RA = 0x80020A08u;
        GranTurismo2PC.func_800205C8(c, m);
        c.S2 = 0x00000001u;
        goto L80020A20;
        L80020A10: ;
        c.RA = 0x80020A18u;
        GranTurismo2PC.func_800205C8(c, m);
        c.A0 = c.V0 + 0u;
        goto L80020908;
        L80020A20: ;
        c.V0 = c.S2 + 0u;
        L80020A24: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020A3C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x58u;
        MemoryAccess.WriteU32(m, (c.SP + 0x4Cu), c.S3);
        c.S3 = c.A0 + 0u;
        c.V1 = 0x02420000u;
        c.V1 = c.V1 | 0x362Au;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1588u));
        c.A0 = c.SP + 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x50u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x48u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.S0);
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x30u));
        c.V0 = 0x02000000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x38u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x3Cu), c.V1);
        c.RA = 0x80020A80u;
        GranTurismo2PC.func_8006AC68(c, m);
        c.A0 = c.SP + 0x18u;
        c.A1 = 0x800B0000u;
        c.A1 = c.A1 + 0x15A0u;
        c.RA = 0x80020A90u;
        GranTurismo2PC.func_8007DA80(c, m);
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
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6720u));
        if (c.V0 != 0u) {
            c.S2 = 0x00000001u;
            goto L80020B24;
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
        c.RA = 0x80020B00u;
        GranTurismo2PC.func_8001F5B0(c, m);
        c.A0 = c.S3 + 0u;
        c.A2 = 0x000000B0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S0);
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x2Cu));
        c.A3 = 0x000000EAu;
        c.RA = 0x80020B1Cu;
        GranTurismo2PC.func_8001F5B0(c, m);
        goto L80020B54;
        L80020B24: ;
        c.A0 = c.S0 + 0u;
        c.A2 = 0x000000B0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S2);
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x28u));
        c.A3 = 0x000000D6u;
        c.RA = 0x80020B3Cu;
        GranTurismo2PC.func_8006ADB4(c, m);
        c.A0 = c.S0 + 0u;
        c.A2 = 0x000000B0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S2);
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x2Cu));
        c.A3 = 0x000000FAu;
        c.RA = 0x80020B54u;
        GranTurismo2PC.func_8006ADB4(c, m);
        L80020B54: ;
        c.A0 = c.S1 + 0x34u;
        c.RA = 0x80020B5Cu;
        GranTurismo2PC.func_8006BEB4(c, m);
        c.V1 = c.V0 + 0u;
        if (c.V1 == 0u) {
            c.V0 = 0x801D0000u;
            goto L80020C24;
        }
        c.V0 = 0x801D0000u;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6720u));
        if (c.V0 != 0u) {
            c.A0 = c.SP + 0x38u;
            goto L80020BC0;
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
        c.V0 = c.V0 - 0x3790u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), 0u);
        c.A1 = c.A1 << 2;
        c.A1 = c.A1 + c.V0;
        c.A1 = MemoryAccess.ReadU32(m, c.A1);
        c.A3 = 0x00000066u;
        c.RA = 0x80020BB8u;
        GranTurismo2PC.func_8001F5B0(c, m);
        c.A0 = c.S1 + 0x34u;
        goto L80020C08;
        L80020BC0: ;
        c.A1 = c.SP + 0x3Cu;
        c.A2 = c.V1 + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x80020BD0u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.SP + 0x18u;
        c.A2 = 0x0000001Cu;
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.V0);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x10u));
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.V0 = 0x80050000u;
        c.V0 = c.V0 - 0x3790u;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.A1 = MemoryAccess.ReadU32(m, c.V1);
        c.A3 = 0x0000007Eu;
        c.RA = 0x80020C04u;
        GranTurismo2PC.func_8006AC90(c, m);
        c.A0 = c.S1 + 0x34u;
        L80020C08: ;
        c.A1 = c.S3 + 0u;
        c.A2 = 0x00000018u;
        c.A3 = 0x0000006Eu;
        c.RA = 0x80020C18u;
        GranTurismo2PC.func_8006BEF4(c, m);
        c.A0 = c.S3 + 0u;
        c.A1 = 0x00000220u;
        c.RA = 0x80020C24u;
        GranTurismo2PC.func_8007DA44(c, m);
        L80020C24: ;
        c.A0 = c.S1 + 0x50u;
        c.A1 = c.S3 + 0u;
        c.A2 = c.SP + 0x18u;
        c.RA = 0x80020C34u;
        GranTurismo2PC.func_8006E5B8(c, m);
        c.A0 = c.S1 + 0xE8u;
        c.A1 = c.S3 + 0u;
        c.A2 = c.SP + 0x18u;
        c.RA = 0x80020C44u;
        GranTurismo2PC.func_8006E5B8(c, m);
        c.A0 = c.S1 + 0x180u;
        c.A1 = c.S3 + 0u;
        c.A2 = c.SP + 0x18u;
        c.RA = 0x80020C54u;
        GranTurismo2PC.func_8006E5B8(c, m);
        c.A0 = c.S1 + 0x218u;
        c.A1 = c.S3 + 0u;
        c.A2 = c.SP + 0x18u;
        c.RA = 0x80020C64u;
        GranTurismo2PC.func_8006E5B8(c, m);
        c.A0 = c.S1 + 0x348u;
        c.A1 = c.S3 + 0u;
        c.A2 = c.SP + 0x18u;
        c.RA = 0x80020C74u;
        GranTurismo2PC.func_8006E5B8(c, m);
        c.A0 = c.S1 + 0x2B0u;
        c.A1 = c.S3 + 0u;
        c.A2 = c.SP + 0x18u;
        c.RA = 0x80020C84u;
        GranTurismo2PC.func_8006E5B8(c, m);
        c.A0 = 0x00000002u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x4u));
        c.A1 = c.S3 + 0u;
        c.RA = 0x80020C98u;
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
    public static void func_80020CB4(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1588u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x418u));
        c.A1 = c.A0 + 0u;
        c.RA = 0x80020CD0u;
        GranTurismo2PC.func_8006A2E4(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020CE0(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1588u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x418u));
        c.A1 = c.A0 + 0u;
        c.RA = 0x80020CFCu;
        GranTurismo2PC.func_8006A2EC(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020D0C(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1588u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x418u));
        c.A1 = c.A0 + 0u;
        c.RA = 0x80020D28u;
        GranTurismo2PC.func_8006A2F4(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020D38(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1588u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x418u));
        c.A1 = c.A0 + 0u;
        c.RA = 0x80020D54u;
        GranTurismo2PC.func_8006A2FC(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020D64(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1588u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x418u));
        c.A1 = c.A0 + 0u;
        c.RA = 0x80020D80u;
        GranTurismo2PC.func_8006A304(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020D90_gt2_overlay_1(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1588u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x418u));
        c.A1 = c.A0 + 0u;
        c.RA = 0x80020DACu;
        GranTurismo2PC.func_8006A30C(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020DBC(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1588u));
        c.V0 = c.V0 + 0x61Cu;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020DCC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        c.V0 = 0x801D0000u;
        c.V1 = 0x80050000u;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6720u));
        c.V1 = c.V1 - 0x3758u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.V1;
        c.A0 = MemoryAccess.ReadU32(m, c.V0);
        c.A1 = c.S0 + 0u;
        c.RA = 0x80020E00u;
        GranTurismo2PC.func_8005D8D4(c, m);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x200u));
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020E14(CpuContext c, IMemory m)
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
        L80020E50: ;
        c.V0 = (int)c.T0 < (int)c.T1 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.A2 << 7;
            goto L80020F10;
        }
        c.V0 = c.A2 << 7;
        c.V1 = c.V0 + c.T2;
        c.V0 = c.V1 | c.A3;
        c.V0 = c.V0 & 0x0003u;
        if (c.V0 == 0u) {
            c.A0 = c.A3 + 0u;
            goto L80020EC8;
        }
        c.A0 = c.A3 + 0u;
        c.V0 = c.V1 + 0x80u;
        L80020E74: ;
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
            goto L80020E74;
        }
        c.A0 = c.A0 + 0x10u;
        c.A3 = c.A3 + 0x80u;
        goto L80020EFC;
        L80020EC8: ;
        c.V0 = c.V1 + 0x80u;
        L80020ECC: ;
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
            goto L80020ECC;
        }
        c.A0 = c.A0 + 0x10u;
        c.A3 = c.A3 + 0x80u;
        L80020EFC: ;
        c.V0 = c.A2 << 1;
        c.V0 = c.T3 + c.V0;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x2u));
        c.T0 = c.T0 + 0x1u;
        goto L80020E50;
        L80020F10: ;
        c.A0 = c.A1 + 0u;
        c.A1 = 0u + 0u;
        c.RA = 0x80020F1Cu;
        GranTurismo2PC.func_80069AC4(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8004BDE0(CpuContext c, IMemory m)
    {
        Dispatcher.Call(c, m, 0u);
        return;
    }
}

public sealed class Gt2_overlay_1DispatchTable : IOverlay
{
    public string Name => "gt2_overlay_1";
    public int LbaStart => 401;
    public uint Base => 0x80010000u;
    public uint Size => 0x3C8C4u;
    public uint ImageSize => 0xA4102Au;
    public bool Relocatable => false;
    public IReadOnlyDictionary<uint, Action<CpuContext, IMemory>> Functions { get; } =
        new Dictionary<uint, Action<CpuContext, IMemory>>
        {
            [0x80010000u] = GranTurismo2PC.func_80010000_gt2_overlay_1,
            [0x8001003Cu] = GranTurismo2PC.func_8001003C_gt2_overlay_1,
            [0x80010078u] = GranTurismo2PC.func_80010078_gt2_overlay_1,
            [0x800101C4u] = GranTurismo2PC.func_800101C4,
            [0x80010434u] = GranTurismo2PC.func_80010434,
            [0x800104E0u] = GranTurismo2PC.func_800104E0,
            [0x800109C0u] = GranTurismo2PC.func_800109C0,
            [0x80010B08u] = GranTurismo2PC.func_80010B08,
            [0x80010C50u] = GranTurismo2PC.func_80010C50,
            [0x80010DC4u] = GranTurismo2PC.func_80010DC4,
            [0x80010E6Cu] = GranTurismo2PC.func_80010E6C,
            [0x80010EDCu] = GranTurismo2PC.func_80010EDC,
            [0x800110C4u] = GranTurismo2PC.func_800110C4,
            [0x800110FCu] = GranTurismo2PC.func_800110FC,
            [0x80011178u] = GranTurismo2PC.func_80011178,
            [0x800111DCu] = GranTurismo2PC.func_800111DC,
            [0x800112D0u] = GranTurismo2PC.func_800112D0,
            [0x80011384u] = GranTurismo2PC.func_80011384,
            [0x800117B4u] = GranTurismo2PC.func_800117B4,
            [0x80011868u] = GranTurismo2PC.func_80011868,
            [0x800118A0u] = GranTurismo2PC.func_800118A0,
            [0x8001191Cu] = GranTurismo2PC.func_8001191C,
            [0x80011B5Cu] = GranTurismo2PC.func_80011B5C,
            [0x80011B90u] = GranTurismo2PC.func_80011B90,
            [0x80011BB8u] = GranTurismo2PC.func_80011BB8,
            [0x80011BC4u] = GranTurismo2PC.func_80011BC4,
            [0x80011EC4u] = GranTurismo2PC.func_80011EC4,
            [0x800120D4u] = GranTurismo2PC.func_800120D4,
            [0x800121A8u] = GranTurismo2PC.func_800121A8,
            [0x800122A0u] = GranTurismo2PC.func_800122A0,
            [0x800122D4u] = GranTurismo2PC.func_800122D4,
            [0x80012314u] = GranTurismo2PC.func_80012314,
            [0x80012348u] = GranTurismo2PC.func_80012348,
            [0x800123A4u] = GranTurismo2PC.func_800123A4,
            [0x800123D0u] = GranTurismo2PC.func_800123D0,
            [0x80012414u] = GranTurismo2PC.func_80012414,
            [0x80012434u] = GranTurismo2PC.func_80012434,
            [0x80012454u] = GranTurismo2PC.func_80012454,
            [0x800124A4u] = GranTurismo2PC.func_800124A4,
            [0x800124F0u] = GranTurismo2PC.func_800124F0,
            [0x80012554u] = GranTurismo2PC.func_80012554,
            [0x8001255Cu] = GranTurismo2PC.func_8001255C,
            [0x8001258Cu] = GranTurismo2PC.func_8001258C,
            [0x800125C8u] = GranTurismo2PC.func_800125C8,
            [0x800125D0u] = GranTurismo2PC.func_800125D0,
            [0x800126BCu] = GranTurismo2PC.func_800126BC_gt2_overlay_1,
            [0x80012730u] = GranTurismo2PC.func_80012730,
            [0x80012870u] = GranTurismo2PC.func_80012870,
            [0x8001289Cu] = GranTurismo2PC.func_8001289C,
            [0x800128F8u] = GranTurismo2PC.func_800128F8,
            [0x800129A0u] = GranTurismo2PC.func_800129A0,
            [0x800129C4u] = GranTurismo2PC.func_800129C4,
            [0x80012A20u] = GranTurismo2PC.func_80012A20,
            [0x80012A80u] = GranTurismo2PC.func_80012A80,
            [0x80012AA4u] = GranTurismo2PC.func_80012AA4,
            [0x80012B00u] = GranTurismo2PC.func_80012B00,
            [0x80012B60u] = GranTurismo2PC.func_80012B60,
            [0x80012B84u] = GranTurismo2PC.func_80012B84,
            [0x80012CE4u] = GranTurismo2PC.func_80012CE4,
            [0x80012D34u] = GranTurismo2PC.func_80012D34,
            [0x80012E74u] = GranTurismo2PC.func_80012E74,
            [0x80012EA0u] = GranTurismo2PC.func_80012EA0,
            [0x80012EC4u] = GranTurismo2PC.func_80012EC4,
            [0x80012F28u] = GranTurismo2PC.func_80012F28,
            [0x80012F30u] = GranTurismo2PC.func_80012F30,
            [0x80012F68u] = GranTurismo2PC.func_80012F68,
            [0x80012FCCu] = GranTurismo2PC.func_80012FCC,
            [0x80012FD4u] = GranTurismo2PC.func_80012FD4,
            [0x8001302Cu] = GranTurismo2PC.func_8001302C,
            [0x8001308Cu] = GranTurismo2PC.func_8001308C,
            [0x800130B0u] = GranTurismo2PC.func_800130B0,
            [0x800130E8u] = GranTurismo2PC.func_800130E8,
            [0x8001314Cu] = GranTurismo2PC.func_8001314C,
            [0x80013154u] = GranTurismo2PC.func_80013154,
            [0x800131ACu] = GranTurismo2PC.func_800131AC_gt2_overlay_1,
            [0x8001320Cu] = GranTurismo2PC.func_8001320C,
            [0x80013230u] = GranTurismo2PC.func_80013230,
            [0x800132CCu] = GranTurismo2PC.func_800132CC,
            [0x8001330Cu] = GranTurismo2PC.func_8001330C,
            [0x80013430u] = GranTurismo2PC.func_80013430,
            [0x80013480u] = GranTurismo2PC.func_80013480,
            [0x800134C4u] = GranTurismo2PC.func_800134C4,
            [0x80013518u] = GranTurismo2PC.func_80013518,
            [0x80013558u] = GranTurismo2PC.func_80013558,
            [0x8001358Cu] = GranTurismo2PC.func_8001358C,
            [0x800135C8u] = GranTurismo2PC.func_800135C8,
            [0x800135E0u] = GranTurismo2PC.func_800135E0,
            [0x80013694u] = GranTurismo2PC.func_80013694,
            [0x800136C4u] = GranTurismo2PC.func_800136C4,
            [0x800136ECu] = GranTurismo2PC.func_800136EC,
            [0x80013728u] = GranTurismo2PC.func_80013728,
            [0x800139C0u] = GranTurismo2PC.func_800139C0,
            [0x80013AA4u] = GranTurismo2PC.func_80013AA4,
            [0x80013E14u] = GranTurismo2PC.func_80013E14,
            [0x80013EA0u] = GranTurismo2PC.func_80013EA0,
            [0x80014024u] = GranTurismo2PC.func_80014024,
            [0x8001407Cu] = GranTurismo2PC.func_8001407C,
            [0x8001420Cu] = GranTurismo2PC.func_8001420C,
            [0x80014424u] = GranTurismo2PC.func_80014424,
            [0x80014524u] = GranTurismo2PC.func_80014524,
            [0x800145E8u] = GranTurismo2PC.func_800145E8,
            [0x80014704u] = GranTurismo2PC.func_80014704,
            [0x80014800u] = GranTurismo2PC.func_80014800,
            [0x800148C8u] = GranTurismo2PC.func_800148C8,
            [0x80014A00u] = GranTurismo2PC.func_80014A00,
            [0x80014B98u] = GranTurismo2PC.func_80014B98,
            [0x80014D60u] = GranTurismo2PC.func_80014D60,
            [0x80015158u] = GranTurismo2PC.func_80015158,
            [0x80015304u] = GranTurismo2PC.func_80015304,
            [0x800154D4u] = GranTurismo2PC.func_800154D4,
            [0x8001553Cu] = GranTurismo2PC.func_8001553C,
            [0x800155ACu] = GranTurismo2PC.func_800155AC,
            [0x80015784u] = GranTurismo2PC.func_80015784,
            [0x80015808u] = GranTurismo2PC.func_80015808,
            [0x8001593Cu] = GranTurismo2PC.func_8001593C,
            [0x80015A30u] = GranTurismo2PC.func_80015A30,
            [0x80015B00u] = GranTurismo2PC.func_80015B00,
            [0x80015B08u] = GranTurismo2PC.func_80015B08,
            [0x80015B6Cu] = GranTurismo2PC.func_80015B6C,
            [0x80015CF8u] = GranTurismo2PC.func_80015CF8,
            [0x80015D98u] = GranTurismo2PC.func_80015D98,
            [0x80015DC0u] = GranTurismo2PC.func_80015DC0,
            [0x80015F4Cu] = GranTurismo2PC.func_80015F4C,
            [0x80016254u] = GranTurismo2PC.func_80016254,
            [0x8001636Cu] = GranTurismo2PC.func_8001636C,
            [0x80016394u] = GranTurismo2PC.func_80016394,
            [0x800163C8u] = GranTurismo2PC.func_800163C8,
            [0x80016410u] = GranTurismo2PC.func_80016410,
            [0x800169B0u] = GranTurismo2PC.func_800169B0,
            [0x80016A10u] = GranTurismo2PC.func_80016A10_gt2_overlay_1,
            [0x80016A48u] = GranTurismo2PC.func_80016A48,
            [0x80016A94u] = GranTurismo2PC.func_80016A94,
            [0x80016BD8u] = GranTurismo2PC.func_80016BD8_gt2_overlay_1,
            [0x80016C28u] = GranTurismo2PC.func_80016C28,
            [0x80016F6Cu] = GranTurismo2PC.func_80016F6C,
            [0x8001721Cu] = GranTurismo2PC.func_8001721C,
            [0x80017370u] = GranTurismo2PC.func_80017370,
            [0x80017400u] = GranTurismo2PC.func_80017400,
            [0x80017564u] = GranTurismo2PC.func_80017564,
            [0x800176E4u] = GranTurismo2PC.func_800176E4,
            [0x800176F4u] = GranTurismo2PC.func_800176F4,
            [0x80017718u] = GranTurismo2PC.func_80017718,
            [0x80017720u] = GranTurismo2PC.func_80017720,
            [0x8001779Cu] = GranTurismo2PC.func_8001779C,
            [0x8001792Cu] = GranTurismo2PC.func_8001792C,
            [0x80017984u] = GranTurismo2PC.func_80017984,
            [0x80017C00u] = GranTurismo2PC.func_80017C00,
            [0x80017D74u] = GranTurismo2PC.func_80017D74,
            [0x80017E68u] = GranTurismo2PC.func_80017E68,
            [0x80017F2Cu] = GranTurismo2PC.func_80017F2C,
            [0x8001805Cu] = GranTurismo2PC.func_8001805C_gt2_overlay_1,
            [0x80018574u] = GranTurismo2PC.func_80018574,
            [0x800186BCu] = GranTurismo2PC.func_800186BC,
            [0x800186D0u] = GranTurismo2PC.func_800186D0,
            [0x800186E4u] = GranTurismo2PC.func_800186E4,
            [0x80018EFCu] = GranTurismo2PC.func_80018EFC,
            [0x80019038u] = GranTurismo2PC.func_80019038,
            [0x800190F0u] = GranTurismo2PC.func_800190F0,
            [0x80019118u] = GranTurismo2PC.func_80019118,
            [0x80019138u] = GranTurismo2PC.func_80019138,
            [0x8001919Cu] = GranTurismo2PC.func_8001919C,
            [0x800191A4u] = GranTurismo2PC.func_800191A4,
            [0x800191D4u] = GranTurismo2PC.func_800191D4,
            [0x80019210u] = GranTurismo2PC.func_80019210_gt2_overlay_1,
            [0x80019218u] = GranTurismo2PC.func_80019218,
            [0x80019250u] = GranTurismo2PC.func_80019250,
            [0x80019288u] = GranTurismo2PC.func_80019288,
            [0x80019308u] = GranTurismo2PC.func_80019308,
            [0x8001937Cu] = GranTurismo2PC.func_8001937C,
            [0x80019388u] = GranTurismo2PC.func_80019388,
            [0x80019498u] = GranTurismo2PC.func_80019498,
            [0x80019598u] = GranTurismo2PC.func_80019598,
            [0x800196E8u] = GranTurismo2PC.func_800196E8,
            [0x80019934u] = GranTurismo2PC.func_80019934,
            [0x80019B24u] = GranTurismo2PC.func_80019B24,
            [0x80019C4Cu] = GranTurismo2PC.func_80019C4C,
            [0x80019D5Cu] = GranTurismo2PC.func_80019D5C,
            [0x80019F44u] = GranTurismo2PC.func_80019F44,
            [0x8001A624u] = GranTurismo2PC.func_8001A624_gt2_overlay_1,
            [0x8001A7E0u] = GranTurismo2PC.func_8001A7E0,
            [0x8001A834u] = GranTurismo2PC.func_8001A834,
            [0x8001A84Cu] = GranTurismo2PC.func_8001A84C,
            [0x8001A860u] = GranTurismo2PC.func_8001A860,
            [0x8001AA84u] = GranTurismo2PC.func_8001AA84,
            [0x8001AAD4u] = GranTurismo2PC.func_8001AAD4,
            [0x8001AB08u] = GranTurismo2PC.func_8001AB08,
            [0x8001AB40u] = GranTurismo2PC.func_8001AB40,
            [0x8001AC78u] = GranTurismo2PC.func_8001AC78,
            [0x8001ACB4u] = GranTurismo2PC.func_8001ACB4,
            [0x8001AD9Cu] = GranTurismo2PC.func_8001AD9C,
            [0x8001ADBCu] = GranTurismo2PC.func_8001ADBC,
            [0x8001ADDCu] = GranTurismo2PC.func_8001ADDC,
            [0x8001ADFCu] = GranTurismo2PC.func_8001ADFC,
            [0x8001AE34u] = GranTurismo2PC.func_8001AE34,
            [0x8001AF6Cu] = GranTurismo2PC.func_8001AF6C,
            [0x8001B2C0u] = GranTurismo2PC.func_8001B2C0,
            [0x8001B7F8u] = GranTurismo2PC.func_8001B7F8,
            [0x8001B860u] = GranTurismo2PC.func_8001B860,
            [0x8001B880u] = GranTurismo2PC.func_8001B880,
            [0x8001B8A0u] = GranTurismo2PC.func_8001B8A0,
            [0x8001B9D8u] = GranTurismo2PC.func_8001B9D8,
            [0x8001BC5Cu] = GranTurismo2PC.func_8001BC5C,
            [0x8001BF00u] = GranTurismo2PC.func_8001BF00,
            [0x8001BF2Cu] = GranTurismo2PC.func_8001BF2C,
            [0x8001BF58u] = GranTurismo2PC.func_8001BF58,
            [0x8001BF84u] = GranTurismo2PC.func_8001BF84,
            [0x8001BFC8u] = GranTurismo2PC.func_8001BFC8,
            [0x8001C00Cu] = GranTurismo2PC.func_8001C00C,
            [0x8001C050u] = GranTurismo2PC.func_8001C050,
            [0x8001C0B0u] = GranTurismo2PC.func_8001C0B0,
            [0x8001C110u] = GranTurismo2PC.func_8001C110,
            [0x8001C170u] = GranTurismo2PC.func_8001C170,
            [0x8001C1C8u] = GranTurismo2PC.func_8001C1C8,
            [0x8001C210u] = GranTurismo2PC.func_8001C210,
            [0x8001C270u] = GranTurismo2PC.func_8001C270,
            [0x8001C2A0u] = GranTurismo2PC.func_8001C2A0,
            [0x8001C48Cu] = GranTurismo2PC.func_8001C48C,
            [0x8001C610u] = GranTurismo2PC.func_8001C610,
            [0x8001C648u] = GranTurismo2PC.func_8001C648,
            [0x8001C684u] = GranTurismo2PC.func_8001C684,
            [0x8001C690u] = GranTurismo2PC.func_8001C690,
            [0x8001C7B8u] = GranTurismo2PC.func_8001C7B8,
            [0x8001CE28u] = GranTurismo2PC.func_8001CE28,
            [0x8001D4B0u] = GranTurismo2PC.func_8001D4B0,
            [0x8001D4D4u] = GranTurismo2PC.func_8001D4D4,
            [0x8001D4E8u] = GranTurismo2PC.func_8001D4E8_gt2_overlay_1,
            [0x8001D4F4u] = GranTurismo2PC.func_8001D4F4,
            [0x8001D754u] = GranTurismo2PC.func_8001D754_gt2_overlay_1,
            [0x8001D7C0u] = GranTurismo2PC.func_8001D7C0,
            [0x8001DA08u] = GranTurismo2PC.func_8001DA08,
            [0x8001DB20u] = GranTurismo2PC.func_8001DB20,
            [0x8001DB58u] = GranTurismo2PC.func_8001DB58,
            [0x8001DBBCu] = GranTurismo2PC.func_8001DBBC,
            [0x8001DBC4u] = GranTurismo2PC.func_8001DBC4,
            [0x8001DBF4u] = GranTurismo2PC.func_8001DBF4,
            [0x8001DC30u] = GranTurismo2PC.func_8001DC30,
            [0x8001DC38u] = GranTurismo2PC.func_8001DC38,
            [0x8001DD24u] = GranTurismo2PC.func_8001DD24,
            [0x8001DD98u] = GranTurismo2PC.func_8001DD98,
            [0x8001DED8u] = GranTurismo2PC.func_8001DED8,
            [0x8001DF04u] = GranTurismo2PC.func_8001DF04,
            [0x8001DF54u] = GranTurismo2PC.func_8001DF54,
            [0x8001DFF0u] = GranTurismo2PC.func_8001DFF0,
            [0x8001E014u] = GranTurismo2PC.func_8001E014,
            [0x8001E11Cu] = GranTurismo2PC.func_8001E11C,
            [0x8001E284u] = GranTurismo2PC.func_8001E284,
            [0x8001E38Cu] = GranTurismo2PC.func_8001E38C,
            [0x8001E408u] = GranTurismo2PC.func_8001E408,
            [0x8001E484u] = GranTurismo2PC.func_8001E484,
            [0x8001E500u] = GranTurismo2PC.func_8001E500,
            [0x8001E550u] = GranTurismo2PC.func_8001E550,
            [0x8001E5F8u] = GranTurismo2PC.func_8001E5F8,
            [0x8001E61Cu] = GranTurismo2PC.func_8001E61C,
            [0x8001E704u] = GranTurismo2PC.func_8001E704,
            [0x8001E754u] = GranTurismo2PC.func_8001E754,
            [0x8001E7E4u] = GranTurismo2PC.func_8001E7E4,
            [0x8001E808u] = GranTurismo2PC.func_8001E808,
            [0x8001E928u] = GranTurismo2PC.func_8001E928,
            [0x8001E9A0u] = GranTurismo2PC.func_8001E9A0,
            [0x8001EAD8u] = GranTurismo2PC.func_8001EAD8,
            [0x8001EB60u] = GranTurismo2PC.func_8001EB60,
            [0x8001EBD4u] = GranTurismo2PC.func_8001EBD4,
            [0x8001EE24u] = GranTurismo2PC.func_8001EE24,
            [0x8001F084u] = GranTurismo2PC.func_8001F084,
            [0x8001F11Cu] = GranTurismo2PC.func_8001F11C,
            [0x8001F358u] = GranTurismo2PC.func_8001F358,
            [0x8001F508u] = GranTurismo2PC.func_8001F508,
            [0x8001F570u] = GranTurismo2PC.func_8001F570,
            [0x8001F5B0u] = GranTurismo2PC.func_8001F5B0,
            [0x8001F6ECu] = GranTurismo2PC.func_8001F6EC,
            [0x8001F73Cu] = GranTurismo2PC.func_8001F73C,
            [0x8001F780u] = GranTurismo2PC.func_8001F780,
            [0x8001F898u] = GranTurismo2PC.func_8001F898,
            [0x8001FA24u] = GranTurismo2PC.func_8001FA24,
            [0x8001FB20u] = GranTurismo2PC.func_8001FB20,
            [0x8001FBFCu] = GranTurismo2PC.func_8001FBFC,
            [0x8001FC7Cu] = GranTurismo2PC.func_8001FC7C,
            [0x8001FCB0u] = GranTurismo2PC.func_8001FCB0,
            [0x8001FDC8u] = GranTurismo2PC.func_8001FDC8,
            [0x8001FF1Cu] = GranTurismo2PC.func_8001FF1C,
            [0x8001FF74u] = GranTurismo2PC.func_8001FF74,
            [0x800200E8u] = GranTurismo2PC.func_800200E8,
            [0x80020248u] = GranTurismo2PC.func_80020248,
            [0x80020278u] = GranTurismo2PC.func_80020278,
            [0x800203B0u] = GranTurismo2PC.func_800203B0,
            [0x8002049Cu] = GranTurismo2PC.func_8002049C,
            [0x800205C0u] = GranTurismo2PC.func_800205C0,
            [0x800205C8u] = GranTurismo2PC.func_800205C8,
            [0x8002062Cu] = GranTurismo2PC.func_8002062C,
            [0x80020788u] = GranTurismo2PC.func_80020788,
            [0x80020828u] = GranTurismo2PC.func_80020828,
            [0x80020868u] = GranTurismo2PC.func_80020868,
            [0x80020A3Cu] = GranTurismo2PC.func_80020A3C,
            [0x80020CB4u] = GranTurismo2PC.func_80020CB4,
            [0x80020CE0u] = GranTurismo2PC.func_80020CE0,
            [0x80020D0Cu] = GranTurismo2PC.func_80020D0C,
            [0x80020D38u] = GranTurismo2PC.func_80020D38,
            [0x80020D64u] = GranTurismo2PC.func_80020D64,
            [0x80020D90u] = GranTurismo2PC.func_80020D90_gt2_overlay_1,
            [0x80020DBCu] = GranTurismo2PC.func_80020DBC,
            [0x80020DCCu] = GranTurismo2PC.func_80020DCC,
            [0x80020E14u] = GranTurismo2PC.func_80020E14,
            [0x8004BDE0u] = GranTurismo2PC.func_8004BDE0,
        };
}
