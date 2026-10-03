using RecompOne.Runtime.Context;
using RecompOne.Runtime.Dispatch;
using RecompOne.Runtime.Memory;

namespace Recompiled.Simulation;

public static partial class GranTurismo2PC
{
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010000_gt2_overlay_4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x30u;
        c.A1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S2);
        c.S2 = 0u + 0u;
        c.A0 = c.SP + 0x10u;
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S0);
        c.RA = 0x80010024u;
        GranTurismo2PC.func_8008CEDC(c, m);
        c.S0 = c.S2 + 0u;
        c.V0 = 0x80020000u;
        c.S1 = c.V0 + 0x4418u;
        MemoryAccess.WriteU8(m, (c.SP + 0x13u), (byte)0u);
        L80010034: ;
        c.A0 = MemoryAccess.ReadU32(m, c.S1);
        c.A1 = c.SP + 0x10u;
        c.RA = 0x80010040u;
        GranTurismo2PC.func_8008CF00(c, m);
        if (c.V0 != 0u) {
            goto L8001004C;
        }
        c.S2 = c.S0 + 0u;
        L8001004C: ;
        c.S0 = c.S0 + 0x1u;
        c.V0 = (int)c.S0 < 6 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S1 = c.S1 + 0x4u;
            goto L80010034;
        }
        c.S1 = c.S1 + 0x4u;
        c.V0 = c.S2 + 0u;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x2Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.SP = c.SP + 0x30u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010078_gt2_overlay_4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x30u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        c.V0 = 0x80090000u;
        c.A1 = c.S1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0x801D0000u;
        c.S0 = c.S0 - 0x6720u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = MemoryAccess.ReadU32(m, (c.V0 + 0x2E6Cu));
        c.V0 = 0u | 0xBF7Cu;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        c.S4 = c.S0 + c.V0;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.A0 = c.S3 + 0u;
        c.RA = 0x800100C0u;
        GranTurismo2PC.func_8007830C(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = 0u + 0u;
        c.A2 = 0x0000058Cu;
        c.S5 = c.V0 + 0u;
        c.RA = 0x800100D4u;
        GranTurismo2PC.func_8008CE30(c, m);
        c.S2 = 0x00000002u;
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.S0 + 0x4u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, (c.S0 + 0x1u));
        c.T0 = MemoryAccess.ReadWordLeft(m, c.T0, (c.S0 + 0x8u));
        c.T0 = MemoryAccess.ReadWordRight(m, c.T0, (c.S0 + 0x5u));
        MemoryAccess.WriteWordLeft(m, (c.S4 + 0x3u), c.A3);
        MemoryAccess.WriteWordRight(m, c.S4, c.A3);
        MemoryAccess.WriteWordLeft(m, (c.S4 + 0x7u), c.T0);
        MemoryAccess.WriteWordRight(m, (c.S4 + 0x4u), c.T0);
        c.S0 = 0x00000001u;
        c.V0 = 0x00000003u;
        MemoryAccess.WriteU8(m, (c.S4 + 0x8u), (byte)c.S2);
        MemoryAccess.WriteU8(m, (c.S4 + 0x9u), (byte)c.S0);
        MemoryAccess.WriteU8(m, (c.S4 + 0xAu), (byte)c.V0);
        c.V0 = MemoryAccess.ReadU8(m, (c.S5 + 0x44u));
        MemoryAccess.WriteU8(m, (c.S4 + 0xEu), (byte)0u);
        c.V0 = c.V0 < c.S0 ? 1u : 0u;
        MemoryAccess.WriteU8(m, (c.S4 + 0xDu), (byte)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, c.S5);
        c.A0 = c.S3 + 0u;
        c.RA = 0x80010128u;
        GranTurismo2PC.func_8007816C(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x80010134u;
        GranTurismo2PC.func_8005E548(c, m);
        c.A1 = MemoryAccess.ReadU16(m, (c.S5 + 0x2u));
        c.A0 = c.S3 + 0u;
        c.RA = 0x80010140u;
        GranTurismo2PC.func_8007816C(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001014Cu;
        GranTurismo2PC.func_8005E5F0(c, m);
        c.A1 = MemoryAccess.ReadU16(m, (c.S5 + 0x94u));
        c.A0 = c.S3 + 0u;
        c.RA = 0x80010158u;
        GranTurismo2PC.func_8007816C(c, m);
        c.A0 = c.S4 + 0x44u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x80010164u;
        GranTurismo2PC.func_8008CEDC(c, m);
        c.A0 = c.S1 + 0u;
        c.RA = 0x8001016Cu;
        Dispatcher.Call(c, m, 0x80010000u);
        MemoryAccess.WriteU8(m, (c.S4 + 0xBu), (byte)c.V0);
        c.V1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0x3u));
        c.V1 = c.V1 - 0x30u;
        c.V0 = c.V1 << (int)(c.S2 & 31u);
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << (int)(c.S0 & 31u);
        c.V1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0x4u));
        c.V0 = c.V0 - 0x30u;
        c.V0 = c.V0 + c.V1;
        MemoryAccess.WriteU8(m, (c.S4 + 0xCu), (byte)c.V0);
        c.A1 = MemoryAccess.ReadU8(m, (c.S5 + 0x45u));
        c.V1 = MemoryAccess.ReadU32(m, (c.S4 + 0x588u));
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU16(m, (c.S4 + 0x57Cu), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.S4 + 0x582u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.S4 + 0x584u), (ushort)c.V0);
        c.V0 = 0x80090000u;
        MemoryAccess.WriteU8(m, (c.S4 + 0x5Au), (byte)c.S0);
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x2878u));
        c.A0 = 0xFFFFFFF9u;
        MemoryAccess.WriteU16(m, (c.S4 + 0x586u), (ushort)0u);
        MemoryAccess.WriteU8(m, (c.S4 + 0x580u), (byte)0u);
        c.V1 = c.V1 | c.S0;
        c.V1 = c.V1 & c.A0;
        MemoryAccess.WriteU32(m, (c.S4 + 0x588u), c.V1);
        if (c.V0 != c.S2) {
            MemoryAccess.WriteU8(m, (c.S4 + 0xFu), (byte)c.A1);
            goto L800101EC;
        }
        MemoryAccess.WriteU8(m, (c.S4 + 0xFu), (byte)c.A1);
        c.V0 = 0x80090000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x2870u));
        c.A0 = c.V0 + 0x10u;
        goto L800101F0;
        L800101EC: ;
        c.A0 = c.S3 + 0x100u;
        L800101F0: ;
        c.V0 = 0x03FF0000u;
        c.S2 = MemoryAccess.ReadU32(m, (c.S5 + 0x4u));
        c.V0 = c.V0 | 0xFFFFu;
        c.S3 = c.S2 >> 26;
        c.S2 = c.S2 & c.V0;
        c.A1 = c.S2 - 0x1u;
        c.RA = 0x8001020Cu;
        GranTurismo2PC.func_80078038(c, m);
        c.S0 = c.S4 + 0x5Cu;
        c.A0 = c.S0 + 0u;
        c.A1 = 0u + 0u;
        c.A2 = 0x000000D0u;
        c.S1 = c.V0 + 0u;
        c.RA = 0x80010224u;
        GranTurismo2PC.func_8008CE30(c, m);
        c.V0 = MemoryAccess.ReadU32(m, c.S1);
        c.A0 = c.S2 + 0u;
        MemoryAccess.WriteU32(m, (c.S4 + 0x5Cu), c.V0);
        c.V0 = 0x80090000u;
        c.V0 = c.V0 + 0x1620u;
        c.S3 = c.S3 + c.V0;
        c.V1 = MemoryAccess.ReadU8(m, c.S3);
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x8Cu), (byte)c.V0);
        MemoryAccess.WriteU8(m, (c.S0 + 0x8Du), (byte)0u);
        MemoryAccess.WriteU32(m, (c.S0 + 0x4u), c.V1);
        c.RA = 0x80010254u;
        GranTurismo2PC.func_800768C0(c, m);
        c.A0 = c.V0 + 0u;
        c.A1 = c.S4 + 0x64u;
        c.RA = 0x80010260u;
        GranTurismo2PC.func_80076FC0(c, m);
        c.A1 = MemoryAccess.ReadU16(m, (c.S0 + 0x24u));
        c.V0 = 0x00000003u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x8Eu), (byte)c.V0);
        if ((int)c.A1 <= 0) {
            MemoryAccess.WriteU8(m, (c.S0 + 0x8Fu), (byte)0u);
            goto L80010288;
        }
        MemoryAccess.WriteU8(m, (c.S0 + 0x8Fu), (byte)0u);
        c.A0 = 0x00000005u;
        c.RA = 0x8001027Cu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU32(m, (c.S4 + 0x5Cu), c.V0);
        L80010288: ;
        c.V0 = 0x801D0000u;
        c.A0 = c.V0 - 0x6760u;
        c.V1 = c.S5 + 0x44u;
        c.V0 = c.V1 | c.A0;
        c.V0 = c.V0 & 0x0003u;
        if (c.V0 == 0u) {
            c.V0 = c.S5 + 0x84u;
            goto L800102F8;
        }
        c.V0 = c.S5 + 0x84u;
        L800102A4: ;
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V1 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.V1);
        c.T0 = MemoryAccess.ReadWordLeft(m, c.T0, (c.V1 + 0x7u));
        c.T0 = MemoryAccess.ReadWordRight(m, c.T0, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadWordLeft(m, c.T1, (c.V1 + 0xBu));
        c.T1 = MemoryAccess.ReadWordRight(m, c.T1, (c.V1 + 0x8u));
        c.T2 = MemoryAccess.ReadWordLeft(m, c.T2, (c.V1 + 0xFu));
        c.T2 = MemoryAccess.ReadWordRight(m, c.T2, (c.V1 + 0xCu));
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0x3u), c.A3);
        MemoryAccess.WriteWordRight(m, c.A0, c.A3);
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0x7u), c.T0);
        MemoryAccess.WriteWordRight(m, (c.A0 + 0x4u), c.T0);
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0xBu), c.T1);
        MemoryAccess.WriteWordRight(m, (c.A0 + 0x8u), c.T1);
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0xFu), c.T2);
        MemoryAccess.WriteWordRight(m, (c.A0 + 0xCu), c.T2);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.V0) {
            c.A0 = c.A0 + 0x10u;
            goto L800102A4;
        }
        c.A0 = c.A0 + 0x10u;
        c.V0 = 0x00010000u;
        goto L80010328;
        L800102F8: ;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, c.A0, c.A3);
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.T0);
        MemoryAccess.WriteU32(m, (c.A0 + 0x8u), c.T1);
        MemoryAccess.WriteU32(m, (c.A0 + 0xCu), c.T2);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.V0) {
            c.A0 = c.A0 + 0x10u;
            goto L800102F8;
        }
        c.A0 = c.A0 + 0x10u;
        c.V0 = 0x00010000u;
        L80010328: ;
        c.V0 = c.V0 | 0x4FDAu;
        c.A0 = c.S4 + 0x64u;
        c.A1 = 0x801D0000u;
        c.A1 = c.A1 - 0x6720u;
        c.A1 = c.A1 + c.V0;
        c.RA = 0x80010340u;
        GranTurismo2PC.func_800771AC(c, m);
        c.V0 = c.S4 + 0u;
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
    public static void func_80010368(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x60u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x54u), c.S7);
        c.S7 = c.V0 - 0x71F0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x50u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x60u), c.A0);
        c.S6 = c.A0 + 0u;
        c.V0 = 0x80020000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x4Cu), c.S5);
        c.S5 = c.V0 + 0x4400u;
        MemoryAccess.WriteU32(m, (c.SP + 0x5Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x58u), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0x48u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x3Cu), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x38u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x34u), 0u);
        L800103B4: ;
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0x30u));
        c.V0 = (int)c.A3 < 6 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.S1 = 0u + 0u;
            goto L800104DC;
        }
        c.S1 = 0u + 0u;
        c.S4 = c.SP + 0x10u;
        c.FP = 0x800B0000u;
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x34u));
        c.S2 = c.S1 + 0u;
        L800103D8: ;
        c.V0 = (int)c.S1 < 10 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A0 = c.S4 + 0u;
            goto L800104B8;
        }
        c.A0 = c.S4 + 0u;
        c.A1 = MemoryAccess.ReadU32(m, c.S5);
        c.A2 = c.S1 + 0u;
        c.RA = 0x800103F0u;
        GranTurismo2PC.func_8008CF34(c, m);
        c.V0 = 0x80090000u;
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 + 0x2E6Cu));
        c.A1 = c.S4 + 0u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x80010404u;
        GranTurismo2PC.func_8007830C(c, m);
        c.A1 = c.V0 + 0u;
        c.V0 = 0x80090000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x2878u));
        c.V0 = 0x00000002u;
        if (c.V1 != c.V0) {
            c.V0 = 0x80090000u;
            goto L80010428;
        }
        c.V0 = 0x80090000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x2870u));
        c.A0 = c.V0 + 0x10u;
        goto L8001042C;
        L80010428: ;
        c.A0 = c.S0 + 0x100u;
        L8001042C: ;
        c.V0 = 0x03FF0000u;
        c.S0 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.V0 = c.V0 | 0xFFFFu;
        c.S0 = c.S0 & c.V0;
        c.A1 = c.S0 - 0x1u;
        c.RA = 0x80010444u;
        GranTurismo2PC.func_80078038(c, m);
        c.A0 = c.S0 + 0u;
        c.S0 = c.V0 + 0u;
        c.RA = 0x80010450u;
        GranTurismo2PC.func_800768C0(c, m);
        c.A1 = 0x800B0000u;
        c.A1 = c.A1 - 0x7278u;
        c.A0 = c.V0 + 0u;
        c.RA = 0x80010460u;
        GranTurismo2PC.func_80076FC0(c, m);
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 - 0x7278u;
        c.A1 = c.S7 + 0u;
        c.RA = 0x80010470u;
        GranTurismo2PC.func_800771AC(c, m);
        c.A0 = c.S7 + 0u;
        c.A1 = c.FP - 0x7030u;
        c.RA = 0x8001047Cu;
        GranTurismo2PC.func_80075930(c, m);
        c.A0 = MemoryAccess.ReadU32(m, c.S0);
        c.S1 = c.S1 + 0x1u;
        c.RA = 0x80010488u;
        GranTurismo2PC.func_80060AE8(c, m);
        c.A0 = c.S6 + c.S2;
        c.A1 = c.V0 + 0u;
        c.RA = 0x80010494u;
        GranTurismo2PC.func_8008CEDC(c, m);
        c.S2 = c.S2 + 0x44u;
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0x60u));
        c.V0 = MemoryAccess.ReadU8(m, (c.S7 + 0x8Au));
        c.V1 = c.A3 + c.S3;
        MemoryAccess.WriteU16(m, (c.V1 + 0x42u), (ushort)c.V0);
        c.V0 = MemoryAccess.ReadU16(m, (c.FP - 0x7030u));
        c.S3 = c.S3 + 0x44u;
        MemoryAccess.WriteU16(m, (c.V1 + 0x40u), (ushort)c.V0);
        goto L800103D8;
        L800104B8: ;
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0x34u));
        c.S6 = c.S6 + 0x2A8u;
        c.A3 = c.A3 + 0x2A8u;
        MemoryAccess.WriteU32(m, (c.SP + 0x34u), c.A3);
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0x30u));
        c.S5 = c.S5 + 0x4u;
        c.A3 = c.A3 + 0x1u;
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.A3);
        goto L800103B4;
        L800104DC: ;
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
    public static void func_8001050C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V0 = 0x00010000u;
        c.V0 = c.V0 | 0x0BD8u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0x801D0000u;
        c.S0 = c.S0 - 0x6720u;
        c.S0 = c.S0 + c.V0;
        c.V0 = 0xFFFFFFFCu;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.S0 = c.S0 & c.V0;
        c.RA = 0x80010538u;
        Dispatcher.Call(c, m, 0x80010078u);
        c.A0 = c.S0 + 0u;
        c.RA = 0x80010540u;
        GranTurismo2PC.func_80010368(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010550(CpuContext c, IMemory m)
    {
        c.V0 = (int)c.A1 < (int)c.A0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = (int)c.A0 < (int)c.A2 ? 1u : 0u;
            goto L8001056C;
        }
        c.V0 = (int)c.A0 < (int)c.A2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.A0 + 0u;
            goto L80010584;
        }
        c.V0 = c.A0 + 0u;
        c.V0 = c.A2 + 0u;
        return;
        L8001056C: ;
        c.V0 = c.A2 + 0u;
        c.V1 = (int)c.A1 < (int)c.V0 ? 1u : 0u;
        if (c.V1 != 0u) {
            goto L80010584;
        }
        c.V0 = c.A1 + 0u;
        return;
        L80010584: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001058C(CpuContext c, IMemory m)
    {
        c.V0 = (int)c.A0 < (int)c.A1 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = (int)c.A2 < (int)c.A0 ? 1u : 0u;
            goto L800105A8;
        }
        c.V0 = (int)c.A2 < (int)c.A0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.A0 + 0u;
            goto L800105C0;
        }
        c.V0 = c.A0 + 0u;
        c.V0 = c.A2 + 0u;
        return;
        L800105A8: ;
        c.V0 = c.A2 + 0u;
        c.V1 = (int)c.V0 < (int)c.A1 ? 1u : 0u;
        if (c.V1 != 0u) {
            goto L800105C0;
        }
        c.V0 = c.A1 + 0u;
        return;
        L800105C0: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800105C8(CpuContext c, IMemory m)
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
        c.RA = 0x80010610u;
        GranTurismo2PC.func_80010550(c, m);
        c.A0 = c.S3 + 0u;
        c.A1 = c.S1 + 0u;
        c.A2 = c.S2 + 0u;
        c.S0 = c.V0 + 0u;
        c.RA = 0x80010624u;
        GranTurismo2PC.func_8001058C(c, m);
        if (c.S0 != 0u) {
            c.V1 = c.V0 + 0u;
            goto L80010634;
        }
        c.V1 = c.V0 + 0u;
        c.A2 = 0u + 0u;
        goto L80010644;
        L80010634: ;
        c.V0 = c.S0 - c.V1;
        c.V0 = c.V0 << 5;
        if (c.S0 != 0u) { if ((int)c.V0 == int.MinValue && (int)c.S0 == -1) { c.LO = 0x80000000u; c.HI = 0u; } else { c.LO = (uint)((int)c.V0 / (int)c.S0); c.HI = (uint)((int)c.V0 % (int)c.S0); } }
        c.A2 = c.LO;
        L80010644: ;
        if (c.A2 == 0u) {
            c.A0 = 0u + 0u;
            goto L800106E4;
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
            goto L8001068C;
        }
        c.A0 = c.V1 - c.A1;
        L8001068C: ;
        if (c.S1 != c.S0) {
            c.V0 = c.V1 - 0x40u;
            goto L80010698;
        }
        c.V0 = c.V1 - 0x40u;
        c.A0 = c.A3 - c.V0;
        L80010698: ;
        if (c.S2 != c.S0) {
            c.V0 = c.A0 << 4;
            goto L800106AC;
        }
        c.V0 = c.A0 << 4;
        c.V0 = c.A3 - 0x80u;
        c.A0 = c.A1 - c.V0;
        c.V0 = c.A0 << 4;
        L800106AC: ;
        c.V0 = c.V0 - c.A0;
        c.A0 = c.V0 << 2;
        if ((int)c.A0 >= 0) {
            c.V0 = (int)c.A0 < 11520 ? 1u : 0u;
            goto L800106D8;
        }
        c.V0 = (int)c.A0 < 11520 ? 1u : 0u;
        c.A0 = c.A0 + 0x2D00u;
        L800106C0: ;
        if ((int)c.A0 < 0) {
            c.A0 = c.A0 + 0x2D00u;
            goto L800106C0;
        }
        c.A0 = c.A0 + 0x2D00u;
        c.A0 = c.A0 - 0x2D00u;
        c.V0 = (int)c.A0 < 11520 ? 1u : 0u;
        goto L800106D8;
        L800106D4: ;
        c.V0 = (int)c.A0 < 11520 ? 1u : 0u;
        L800106D8: ;
        if (c.V0 == 0u) {
            c.A0 = c.A0 - 0x2D00u;
            goto L800106D4;
        }
        c.A0 = c.A0 - 0x2D00u;
        c.A0 = c.A0 + 0x2D00u;
        L800106E4: ;
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
    public static void func_80010714(CpuContext c, IMemory m)
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
            goto L8001076C;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x5Cu), c.A3);
        c.V0 = 0x80090000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x2870u));
        c.V0 = c.V0 + 0x10u;
        goto L80010770;
        L8001076C: ;
        c.V0 = c.A1 + 0x100u;
        L80010770: ;
        c.S7 = 0x00000040u;
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0x58u));
        c.S4 = c.V0 + 0u;
        c.RA = 0x80010780u;
        GranTurismo2PC.func_80078138(c, m);
        c.S6 = c.V0 + 0u;
        if ((int)c.S6 > 0) {
            c.V0 = 0u + 0u;
            goto L80010798;
        }
        c.V0 = 0u + 0u;
        goto L80010954;
        L80010794: ;
        c.S7 = c.S7 - 0x1u;
        L80010798: ;
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0x5Cu));
        c.RA = 0x800107A4u;
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
        c.RA = 0x800107D8u;
        GranTurismo2PC.func_80078038(c, m);
        c.A0 = MemoryAccess.ReadU32(m, c.V0);
        c.RA = 0x800107E4u;
        GranTurismo2PC.func_80060B70(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 != 0u) {
            c.S2 = 0u + 0u;
            goto L80010798;
        }
        c.S2 = 0u + 0u;
        c.S3 = c.S0 + 0u;
        c.FP = (int)c.S2 < (int)c.S7 ? 1u : 0u;
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x60u));
        L800107FC: ;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x64u));
        c.V0 = (int)c.S2 < (int)c.T0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A0 = c.S4 + 0u;
            goto L80010870;
        }
        c.A0 = c.S4 + 0u;
        c.S0 = MemoryAccess.ReadU16(m, (c.S1 + 0x8u));
        c.A1 = c.S3 - 0x1u;
        c.S0 = c.S0 ^ c.S3;
        c.S0 = c.S0 < 0x00000001u ? 1u : 0u;
        c.RA = 0x80010824u;
        GranTurismo2PC.func_80078038(c, m);
        c.V1 = MemoryAccess.ReadU32(m, c.V0);
        c.V0 = MemoryAccess.ReadU32(m, c.S1);
        c.V0 = c.V0 ^ c.V1;
        c.V0 = c.V0 < 0x00000001u ? 1u : 0u;
        c.S0 = c.S0 | c.V0;
        if (c.S0 == 0u) {
            goto L80010864;
        }
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0x5Cu));
        c.RA = 0x80010850u;
        GranTurismo2PC.func_80083AE0(c, m);
        c.V0 = c.V0 & 0x001Fu;
        c.V0 = (int)c.V0 < 29 ? 1u : 0u;
        c.V0 = c.V0 & c.FP;
        if (c.V0 != 0u) {
            goto L80010794;
        }
        L80010864: ;
        c.S1 = c.S1 + 0xCu;
        c.S2 = c.S2 + 0x1u;
        goto L800107FC;
        L80010870: ;
        c.A1 = c.S5 & 0xFFFFu;
        c.A1 = c.A1 - 0x1u;
        c.RA = 0x8001087Cu;
        GranTurismo2PC.func_80078038(c, m);
        c.V1 = MemoryAccess.ReadU16(m, (c.SP + 0x20u));
        c.S4 = MemoryAccess.ReadU32(m, c.V0);
        if (c.V1 == 0u) {
            c.V0 = 0x80090000u;
            goto L800108B4;
        }
        c.V0 = 0x80090000u;
        c.V0 = c.V0 + 0x1620u;
        c.V0 = c.V1 + c.V0;
        c.A1 = MemoryAccess.ReadU8(m, c.V0);
        c.A0 = c.S4 + 0u;
        c.S0 = c.A1 + 0u;
        c.A1 = c.A1 << 24;
        c.A1 = (uint)((int)c.A1 >> 24);
        c.RA = 0x800108ACu;
        GranTurismo2PC.func_80060D28(c, m);
        c.V1 = c.V0 + 0u;
        goto L80010930;
        L800108B4: ;
        c.A0 = c.S4 + 0u;
        c.A1 = c.SP + 0x18u;
        c.A2 = c.SP + 0x1Cu;
        c.RA = 0x800108C4u;
        GranTurismo2PC.func_80060BEC(c, m);
        c.S3 = c.V0 + 0u;
        c.S2 = 0u + 0u;
        L800108CC: ;
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0x5Cu));
        c.RA = 0x800108D8u;
        GranTurismo2PC.func_80083AE0(c, m);
        if (c.S3 != 0u) { c.LO = c.V0 / c.S3; c.HI = c.V0 % c.S3; }
        c.S0 = c.HI;
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = c.S0 << 1;
        c.V0 = c.S1 + c.V0;
        c.A0 = MemoryAccess.ReadU16(m, c.V0);
        c.A1 = c.SP + 0x10u;
        c.RA = 0x800108F8u;
        GranTurismo2PC.func_800105C8(c, m);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.SP + 0x12u));
        c.V0 = (int)c.V0 < 6 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.S2 = c.S2 + 0x1u;
            goto L80010918;
        }
        c.S2 = c.S2 + 0x1u;
        c.V0 = (int)c.S2 < 2 ? 1u : 0u;
        if (c.V0 != 0u) {
            goto L800108CC;
        }
        L80010918: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.V1 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.V0 = c.V0 + c.S0;
        c.V1 = c.S1 + c.V1;
        c.S0 = MemoryAccess.ReadU8(m, c.V0);
        c.V1 = MemoryAccess.ReadU16(m, c.V1);
        L80010930: ;
        c.T0 = MemoryAccess.ReadU16(m, (c.SP + 0x20u));
        c.V0 = c.T0 + 0u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x50u));
        MemoryAccess.WriteU32(m, c.T0, c.S4);
        MemoryAccess.WriteU8(m, (c.T0 + 0x5u), (byte)c.S0);
        MemoryAccess.WriteU16(m, (c.T0 + 0x6u), (ushort)c.V1);
        MemoryAccess.WriteU16(m, (c.T0 + 0x8u), (ushort)c.S5);
        L80010954: ;
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
    public static void func_80010984(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S1);
        c.S1 = c.A0 + 0u;
        c.A0 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S0);
        c.S0 = c.A3 + 0u;
        if (c.A2 == 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
            goto L800109D4;
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
        c.RA = 0x800109CCu;
        GranTurismo2PC.func_80060D28(c, m);
        MemoryAccess.WriteU8(m, (c.S1 + 0x5u), (byte)c.S0);
        goto L80010A18;
        L800109D4: ;
        c.A1 = c.SP + 0x10u;
        c.A2 = c.SP + 0x14u;
        c.RA = 0x800109E0u;
        GranTurismo2PC.func_80060BEC(c, m);
        c.A0 = c.S0 + 0u;
        c.S0 = c.V0 + 0u;
        c.RA = 0x800109ECu;
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
        L80010A18: ;
        MemoryAccess.WriteU16(m, (c.S1 + 0x6u), (ushort)c.V0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80010A30(CpuContext c, IMemory m)
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
            goto L80010A98;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0xACu), c.A3);
        c.A0 = c.V0 + 0u;
        L80010A98: ;
        if (c.V1 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x64u), c.A0);
            goto L80010DE8;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x64u), c.A0);
        c.A0 = 0u + 0u;
        MemoryAccess.WriteU16(m, (c.S5 + 0x58u), (ushort)0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x68u), c.FP);
        c.RA = 0x80010AB0u;
        GranTurismo2PC.func_8007D23C(c, m);
        c.A0 = c.S5 + 0u;
        c.A1 = 0u + 0u;
        c.A2 = 0x0000058Cu;
        MemoryAccess.WriteU32(m, (c.SP + 0x60u), c.V0);
        c.RA = 0x80010AC4u;
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
        c.RA = 0x80010B2Cu;
        GranTurismo2PC.func_8007816C(c, m);
        c.A0 = c.S5 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x80010B38u;
        GranTurismo2PC.func_8005E548(c, m);
        c.A1 = MemoryAccess.ReadU16(m, (c.FP + 0x2u));
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0x64u));
        c.RA = 0x80010B48u;
        GranTurismo2PC.func_8007816C(c, m);
        c.A0 = c.S5 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x80010B54u;
        GranTurismo2PC.func_8005E5F0(c, m);
        c.A1 = MemoryAccess.ReadU16(m, (c.FP + 0x94u));
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0x64u));
        c.RA = 0x80010B64u;
        GranTurismo2PC.func_8007816C(c, m);
        c.A0 = c.S5 + 0x44u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x80010B70u;
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
            goto L80010BCC;
        }
        c.S4 = 0x00000001u;
        c.T0 = MemoryAccess.ReadU32(m, c.T4);
        c.T1 = MemoryAccess.ReadU32(m, (c.T4 + 0x4u));
        c.T2 = MemoryAccess.ReadU32(m, (c.T4 + 0x8u));
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.T0);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.T1);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.T2);
        MemoryAccess.WriteU16(m, (c.SP + 0x20u), (ushort)0u);
        L80010BCC: ;
        c.T4 = MemoryAccess.ReadU32(m, (c.SP + 0xB8u));
        if (c.T4 == 0u) {
            c.V1 = c.S4 << 1;
            goto L80010C0C;
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
        L80010C0C: ;
        c.A0 = c.FP + 0u;
        c.RA = 0x80010C14u;
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
        L80010C38: ;
        if (c.V0 == 0u) {
            c.S7 = 0u + 0u;
            goto L80010DF8;
        }
        c.S7 = 0u + 0u;
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x70u));
        c.S2 = 0x00000001u;
        if (c.S3 == 0u) {
            c.S0 = c.S5 + c.T1;
            goto L80010C60;
        }
        c.S0 = c.S5 + c.T1;
        if (c.S3 == c.S2) {
            c.A0 = c.S0 + 0u;
            goto L80010C78;
        }
        c.A0 = c.S0 + 0u;
        c.A1 = 0u + 0u;
        goto L80010C94;
        L80010C60: ;
        c.T2 = MemoryAccess.ReadU32(m, (c.SP + 0xB0u));
        if (c.T2 == 0u) {
            c.A0 = c.S0 + 0u;
            goto L80010C90;
        }
        c.A0 = c.S0 + 0u;
        c.S2 = 0x00000003u;
        goto L80010C90;
        L80010C78: ;
        c.T3 = MemoryAccess.ReadU32(m, (c.SP + 0xB8u));
        if (c.T3 == 0u) {
            c.A1 = 0u + 0u;
            goto L80010C94;
        }
        c.A1 = 0u + 0u;
        c.S2 = 0x00000004u;
        c.A0 = c.S0 + 0u;
        L80010C90: ;
        c.A1 = 0u + 0u;
        L80010C94: ;
        c.A2 = 0x000000D0u;
        c.RA = 0x80010C9Cu;
        GranTurismo2PC.func_8008CE30(c, m);
        c.V0 = (int)c.S3 < (int)c.S4 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.A0 = c.S1 + 0u;
            goto L80010CD0;
        }
        c.A0 = c.S1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S4);
        c.S4 = c.S4 + 0x1u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0x64u));
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0x68u));
        c.T4 = MemoryAccess.ReadU32(m, (c.SP + 0x6Cu));
        c.A3 = c.SP + 0x60u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T4);
        c.RA = 0x80010CC8u;
        GranTurismo2PC.func_80010714(c, m);
        c.S7 = c.V0 + 0u;
        MemoryAccess.WriteU8(m, (c.S1 + 0x4u), (byte)0u);
        L80010CD0: ;
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
            goto L80010D34;
        }
        MemoryAccess.WriteU32(m, (c.S0 + 0x4u), c.V1);
        c.V0 = (int)c.S2 < 4 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = 0x00000004u;
            goto L80010D20;
        }
        c.V0 = 0x00000004u;
        if (c.S2 == c.V0) {
            goto L80010D24;
        }
        goto L80010D34;
        L80010D20: ;
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0xACu));
        L80010D24: ;
        c.A1 = c.S0 + 0x8u;
        c.RA = 0x80010D2Cu;
        GranTurismo2PC.func_80076FC0(c, m);
        goto L80010D8C;
        L80010D34: ;
        c.A0 = MemoryAccess.ReadU16(m, (c.S1 + 0x8u));
        c.RA = 0x80010D40u;
        GranTurismo2PC.func_800768C0(c, m);
        c.A0 = c.V0 + 0u;
        c.A1 = c.S0 + 0x8u;
        c.RA = 0x80010D4Cu;
        GranTurismo2PC.func_80076F5C(c, m);
        c.A1 = MemoryAccess.ReadU16(m, (c.S0 + 0x24u));
        c.A0 = 0x00000005u;
        c.RA = 0x80010D58u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = MemoryAccess.ReadU8(m, (c.V1 + 0xEu));
        if (c.V0 == 0u) {
            c.A0 = c.S1 + 0u;
            goto L80010D8C;
        }
        c.A0 = c.S1 + 0u;
        c.A2 = c.S7 + 0u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.A3 = c.SP + 0x60u;
        MemoryAccess.WriteU32(m, c.S0, c.A1);
        c.RA = 0x80010D80u;
        GranTurismo2PC.func_80010984(c, m);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0x5u));
        MemoryAccess.WriteU32(m, (c.S0 + 0x4u), c.V0);
        L80010D8C: ;
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0xA4u));
        if (c.T1 == 0u) {
            c.V1 = c.S0 + 0x8u;
            goto L80010DAC;
        }
        c.V1 = c.S0 + 0x8u;
        c.V0 = MemoryAccess.ReadU8(m, (c.V1 + 0x7Au));
        c.V0 = c.V0 | 0x0040u;
        MemoryAccess.WriteU8(m, (c.V1 + 0x7Au), (byte)c.V0);
        L80010DAC: ;
        MemoryAccess.WriteU8(m, (c.S0 + 0x8Eu), (byte)c.S2);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0x4u));
        c.T2 = MemoryAccess.ReadU32(m, (c.SP + 0x70u));
        c.S1 = c.S1 + 0xCu;
        c.T2 = c.T2 + 0xD0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x70u), c.T2);
        c.A0 = MemoryAccess.ReadU32(m, c.S0);
        c.S3 = c.S3 + 0x1u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x8Fu), (byte)c.V0);
        c.RA = 0x80010DD4u;
        GranTurismo2PC.func_80060AE8(c, m);
        c.A0 = c.S0 + 0x90u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x80010DE0u;
        GranTurismo2PC.func_8008CEDC(c, m);
        c.V0 = (int)c.S3 < (int)c.S6 ? 1u : 0u;
        goto L80010C38;
        L80010DE8: ;
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0x64u));
        c.A1 = c.S5 + 0x10u;
        c.RA = 0x80010DF4u;
        GranTurismo2PC.func_8007830C(c, m);
        c.FP = c.V0 + 0u;
        L80010DF8: ;
        c.V0 = 0x801D0000u;
        c.A1 = c.V0 - 0x6760u;
        c.V1 = c.FP + 0x44u;
        c.V0 = c.V1 | c.A1;
        c.V0 = c.V0 & 0x0003u;
        if (c.V0 == 0u) {
            c.V0 = c.FP + 0x84u;
            goto L80010E68;
        }
        c.V0 = c.FP + 0x84u;
        L80010E14: ;
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
            goto L80010E14;
        }
        c.A1 = c.A1 + 0x10u;
        c.S2 = 0u + 0u;
        goto L80010E98;
        L80010E68: ;
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
            goto L80010E68;
        }
        c.A1 = c.A1 + 0x10u;
        c.S2 = 0u + 0u;
        L80010E98: ;
        c.V0 = 0x801D0000u;
        c.S3 = c.V0 - 0x6720u;
        c.S1 = 0x00010000u;
        c.S1 = c.S1 | 0x4FDAu;
        c.S0 = 0x0000005Cu;
        L80010EAC: ;
        c.V0 = MemoryAccess.ReadU8(m, (c.S5 + 0x5Au));
        c.V0 = (int)c.S2 < (int)c.V0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A0 = c.S5 + c.S0;
            goto L80010EDC;
        }
        c.A0 = c.S5 + c.S0;
        c.A0 = c.A0 + 0x8u;
        c.A1 = c.S1 + c.S3;
        c.RA = 0x80010ECCu;
        GranTurismo2PC.func_800771AC(c, m);
        c.S1 = c.S1 + 0x1C0u;
        c.S0 = c.S0 + 0xD0u;
        c.S2 = c.S2 + 0x1u;
        goto L80010EAC;
        L80010EDC: ;
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
    public static void func_80010F10(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = c.A3 + 0u;
        c.V0 = c.S2 << 1;
        c.V0 = c.V0 + c.S2;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.S2;
        c.V0 = c.V0 << 4;
        c.V0 = c.V0 + 0x5Cu;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + c.V0;
        if ((int)c.S0 < 0) {
            MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
            goto L80010F70;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        c.A0 = c.S1 + 0u;
        c.A1 = 0u + 0u;
        c.A2 = 0x000000D0u;
        c.RA = 0x80010F64u;
        GranTurismo2PC.func_8008CE30(c, m);
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.S1 + 0x8Cu), (byte)c.V0);
        MemoryAccess.WriteU8(m, (c.S1 + 0x8Du), (byte)c.S0);
        L80010F70: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x3Cu));
        MemoryAccess.WriteU8(m, (c.S1 + 0x8Eu), (byte)c.S3);
        MemoryAccess.WriteU32(m, c.S1, c.V0);
        c.A0 = c.V0 + 0u;
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x38u));
        MemoryAccess.WriteU8(m, (c.S1 + 0x8Fu), (byte)c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x40u));
        MemoryAccess.WriteU32(m, (c.S1 + 0x4u), c.V0);
        c.RA = 0x80010F98u;
        GranTurismo2PC.func_80060AE8(c, m);
        c.A0 = c.S1 + 0x90u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x80010FA4u;
        GranTurismo2PC.func_8008CEDC(c, m);
        c.S0 = c.S1 + 0x8u;
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0x44u));
        c.A1 = c.S0 + 0u;
        c.RA = 0x80010FB4u;
        GranTurismo2PC.func_80076FC0(c, m);
        c.V1 = 0x00010000u;
        c.V1 = c.V1 | 0x4FDAu;
        c.A0 = c.S0 + 0u;
        c.V0 = 0x801D0000u;
        c.V0 = c.V0 - 0x6720u;
        c.A1 = c.S2 << 3;
        c.A1 = c.A1 - c.S2;
        c.A1 = c.A1 << 6;
        c.A1 = c.A1 + c.V1;
        c.A1 = c.A1 + c.V0;
        c.RA = 0x80010FE0u;
        GranTurismo2PC.func_800771AC(c, m);
        c.V0 = c.S1 + 0u;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80011000(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x30u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S5);
        c.S5 = c.A3 + 0u;
        c.A0 = 0x00010000u;
        c.A0 = c.A0 | 0x4FDAu;
        c.V0 = c.A1 << 1;
        c.V0 = c.V0 + c.A1;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.A1;
        c.V0 = c.V0 << 4;
        c.V0 = c.V0 + 0x5Cu;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.S2 + c.V0;
        c.V1 = 0x801D0000u;
        c.V1 = c.V1 - 0x6720u;
        c.V0 = c.A1 << 3;
        c.V0 = c.V0 - c.A1;
        c.V0 = c.V0 << 6;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x44u));
        c.V0 = c.V0 + c.A0;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        c.S4 = c.V0 + c.V1;
        if ((int)c.S3 < 0) {
            MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.RA);
            goto L80011090;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.RA);
        c.A0 = c.S1 + 0u;
        c.A1 = 0u + 0u;
        c.A2 = 0x000000D0u;
        c.RA = 0x80011084u;
        GranTurismo2PC.func_8008CE30(c, m);
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.S1 + 0x8Cu), (byte)c.V0);
        MemoryAccess.WriteU8(m, (c.S1 + 0x8Du), (byte)c.S3);
        L80011090: ;
        MemoryAccess.WriteU8(m, (c.S1 + 0x8Eu), (byte)c.S5);
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x40u));
        c.A0 = c.S1 + 0x8u;
        MemoryAccess.WriteU8(m, (c.S1 + 0x8Fu), (byte)c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x8Cu));
        c.V1 = c.S0 + 0x8u;
        MemoryAccess.WriteU32(m, c.S1, c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x4u));
        c.A1 = c.S0 + 0x88u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x4u), c.V0);
        L800110B8: ;
        c.T0 = MemoryAccess.ReadU32(m, c.V1);
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T3 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, c.A0, c.T0);
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.T1);
        MemoryAccess.WriteU32(m, (c.A0 + 0x8u), c.T2);
        MemoryAccess.WriteU32(m, (c.A0 + 0xCu), c.T3);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.A1) {
            c.A0 = c.A0 + 0x10u;
            goto L800110B8;
        }
        c.A0 = c.A0 + 0x10u;
        c.S0 = c.S1 + 0x8u;
        c.T0 = MemoryAccess.ReadU32(m, c.V1);
        MemoryAccess.WriteU32(m, c.A0, c.T0);
        c.V0 = MemoryAccess.ReadU8(m, (c.S0 + 0x7Au));
        c.V0 = c.V0 | 0x0040u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x7Au), (byte)c.V0);
        c.A0 = MemoryAccess.ReadU32(m, c.S1);
        c.RA = 0x80011110u;
        GranTurismo2PC.func_80060AE8(c, m);
        c.A0 = c.S1 + 0x90u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001111Cu;
        GranTurismo2PC.func_8008CEDC(c, m);
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x48u));
        c.A0 = c.S0 + 0u;
        MemoryAccess.WriteU16(m, (c.S2 + 0x582u), (ushort)c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x4Cu));
        c.A1 = c.S4 + 0u;
        MemoryAccess.WriteU16(m, (c.S2 + 0x584u), (ushort)c.V0);
        c.RA = 0x80011138u;
        GranTurismo2PC.func_800771AC(c, m);
        c.V0 = c.S1 + 0u;
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
    public static void func_80011160(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.A1 = 0xFFFFFFFFu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A2 = 0x000017E8u;
        c.RA = 0x80011174u;
        GranTurismo2PC.func_8008CE30(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80011184(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0xC8u;
        MemoryAccess.WriteU32(m, (c.SP + 0xB0u), c.S4);
        c.S4 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0xCCu), c.A1);
        c.A0 = c.A1 + 0u;
        c.A1 = c.SP + 0x10u;
        MemoryAccess.WriteU32(m, (c.SP + 0xC4u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0xC0u), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0xBCu), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0xB8u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0xB4u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0xACu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0xA8u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0xA4u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0xA0u), c.S0);
        c.RA = 0x800111C4u;
        GranTurismo2PC.func_80076954(c, m);
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x22u));
        c.A0 = 0x00000012u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x800111D4u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0x8u));
        c.A1 = c.V1 + 0x40u;
        c.V0 = c.A0 << 2;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 - c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.S4 + c.V0;
        c.V0 = c.V0 + 0x988u;
        L800111FC: ;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, c.V0, c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x4u), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x8u), c.T1);
        MemoryAccess.WriteU32(m, (c.V0 + 0xCu), c.T2);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.A1) {
            c.V0 = c.V0 + 0x10u;
            goto L800111FC;
        }
        c.V0 = c.V0 + 0x10u;
        c.S2 = 0u + 0u;
        c.S3 = 0x00000004u;
        c.S0 = 0x0000004Cu;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        MemoryAccess.WriteU32(m, c.V0, c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x4u), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x8u), c.T1);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0xAB8u), c.S1);
        L80011258: ;
        c.A0 = c.S2 + 0x23u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x80011268u;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x00000012u;
            goto L800112D4;
        }
        c.A0 = 0x00000012u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x8001127Cu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S0;
        c.V1 = c.V1 + 0x988u;
        c.A0 = c.V0 + 0x40u;
        L80011288: ;
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
            goto L80011288;
        }
        c.V1 = c.V1 + 0x10u;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU32(m, c.V1, c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x4u), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x8u), c.T1);
        c.V0 = c.S4 + c.S3;
        MemoryAccess.WriteU32(m, (c.V0 + 0xAB8u), c.S1);
        L800112D4: ;
        c.S3 = c.S3 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        c.V0 = (int)c.S2 < 3 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S0 = c.S0 + 0x4Cu;
            goto L80011258;
        }
        c.S0 = c.S0 + 0x4Cu;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x14u));
        c.A0 = 0u + 0u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x800112F8u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0x8u));
        c.S2 = 0u + 0u;
        c.V0 = c.A0 << 1;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.S4 + c.V0;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V0 + 0xAC8u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0xACCu), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0xAD0u), c.T1);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0xAE0u), c.S1);
        c.S0 = c.S2 + 0x1u;
        L8001133C: ;
        c.A0 = c.S0 + 0u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x8001134Cu;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0u + 0u;
            goto L80011394;
        }
        c.A0 = 0u + 0u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80011360u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S0 << 1;
        c.V1 = c.V1 + c.S0;
        c.V1 = c.V1 << 2;
        c.V1 = c.S4 + c.V1;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V1 + 0xAC8u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0xACCu), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0xAD0u), c.T1);
        c.V0 = c.S0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0xAE0u), c.S1);
        L80011394: ;
        c.S2 = c.S0 + 0u;
        if ((int)c.S2 <= 0) {
            c.S0 = c.S2 + 0x1u;
            goto L8001133C;
        }
        c.S0 = c.S2 + 0x1u;
        c.A0 = 0x00000001u;
        c.S2 = 0u + 0u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x16u));
        c.S3 = 0x00000004u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x800113B8u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0x8u));
        c.S0 = 0x00000010u;
        c.V0 = c.A0 << (int)(c.S3 & 31u);
        c.V0 = c.S4 + c.V0;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, (c.V0 + 0xAE8u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0xAECu), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0xAF0u), c.T1);
        MemoryAccess.WriteU32(m, (c.V0 + 0xAF4u), c.T2);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0xB08u), c.S1);
        L800113F8: ;
        c.A0 = c.S2 + 0x2u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x80011408u;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x00000001u;
            goto L80011448;
        }
        c.A0 = 0x00000001u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x8001141Cu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S0;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V0 + 0xCu));
        MemoryAccess.WriteU32(m, (c.V1 + 0xAE8u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0xAECu), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0xAF0u), c.T1);
        MemoryAccess.WriteU32(m, (c.V1 + 0xAF4u), c.T2);
        c.V0 = c.S4 + c.S3;
        MemoryAccess.WriteU32(m, (c.V0 + 0xB08u), c.S1);
        L80011448: ;
        c.S3 = c.S3 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        if ((int)c.S2 <= 0) {
            c.S0 = c.S0 + 0x10u;
            goto L800113F8;
        }
        c.S0 = c.S0 + 0x10u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x26u));
        c.A0 = 0x00000016u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80011468u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.S3 = c.V0 + 0u;
        c.A3 = MemoryAccess.ReadU32(m, c.S3);
        c.T0 = MemoryAccess.ReadU32(m, (c.S3 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.S3 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.S3 + 0xCu));
        MemoryAccess.WriteU32(m, (c.S4 + 0xB10u), c.A3);
        MemoryAccess.WriteU32(m, (c.S4 + 0xB14u), c.T0);
        MemoryAccess.WriteU32(m, (c.S4 + 0xB18u), c.T1);
        MemoryAccess.WriteU32(m, (c.S4 + 0xB1Cu), c.T2);
        MemoryAccess.WriteU32(m, (c.S4 + 0xBB0u), c.S1);
        c.A3 = MemoryAccess.ReadU32(m, c.S3);
        c.T0 = MemoryAccess.ReadU32(m, (c.S3 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.S3 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.S3 + 0xCu));
        MemoryAccess.WriteU32(m, (c.S4 + 0xB90u), c.A3);
        MemoryAccess.WriteU32(m, (c.S4 + 0xB94u), c.T0);
        MemoryAccess.WriteU32(m, (c.S4 + 0xB98u), c.T1);
        MemoryAccess.WriteU32(m, (c.S4 + 0xB9Cu), c.T2);
        MemoryAccess.WriteU32(m, (c.S4 + 0xBD0u), c.S1);
        c.A3 = MemoryAccess.ReadU32(m, c.S3);
        c.T0 = MemoryAccess.ReadU32(m, (c.S3 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.S3 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.S3 + 0xCu));
        MemoryAccess.WriteU32(m, (c.S4 + 0xBA0u), c.A3);
        MemoryAccess.WriteU32(m, (c.S4 + 0xBA4u), c.T0);
        MemoryAccess.WriteU32(m, (c.S4 + 0xBA8u), c.T1);
        MemoryAccess.WriteU32(m, (c.S4 + 0xBACu), c.T2);
        MemoryAccess.WriteU32(m, (c.S4 + 0xBD4u), c.S1);
        c.A1 = MemoryAccess.ReadU16(m, (c.S3 + 0xAu));
        c.A0 = 0x00000018u;
        c.RA = 0x800114E4u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V0 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.V0);
        MemoryAccess.WriteWordLeft(m, (c.S4 + 0xBDBu), c.A3);
        MemoryAccess.WriteWordRight(m, (c.S4 + 0xBD8u), c.A3);
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V0 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.V0);
        MemoryAccess.WriteWordLeft(m, (c.S4 + 0xBFBu), c.A3);
        MemoryAccess.WriteWordRight(m, (c.S4 + 0xBF8u), c.A3);
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V0 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.V0);
        MemoryAccess.WriteWordLeft(m, (c.S4 + 0xBFFu), c.A3);
        MemoryAccess.WriteWordRight(m, (c.S4 + 0xBFCu), c.A3);
        c.A1 = MemoryAccess.ReadU16(m, (c.S3 + 0xCu));
        c.A0 = 0x00000019u;
        c.RA = 0x8001152Cu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.A1 = c.S4 + 0xC00u;
        c.V1 = c.V0 + 0u;
        c.V0 = c.V1 | c.A1;
        c.V0 = c.V0 & 0x0003u;
        if (c.V0 == 0u) {
            c.A0 = c.V1 + 0u;
            goto L8001159C;
        }
        c.A0 = c.V1 + 0u;
        c.V0 = c.V1 + 0x40u;
        L80011548: ;
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.A0 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.A0);
        c.T0 = MemoryAccess.ReadWordLeft(m, c.T0, (c.A0 + 0x7u));
        c.T0 = MemoryAccess.ReadWordRight(m, c.T0, (c.A0 + 0x4u));
        c.T1 = MemoryAccess.ReadWordLeft(m, c.T1, (c.A0 + 0xBu));
        c.T1 = MemoryAccess.ReadWordRight(m, c.T1, (c.A0 + 0x8u));
        c.T2 = MemoryAccess.ReadWordLeft(m, c.T2, (c.A0 + 0xFu));
        c.T2 = MemoryAccess.ReadWordRight(m, c.T2, (c.A0 + 0xCu));
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0x3u), c.A3);
        MemoryAccess.WriteWordRight(m, c.A1, c.A3);
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0x7u), c.T0);
        MemoryAccess.WriteWordRight(m, (c.A1 + 0x4u), c.T0);
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0xBu), c.T1);
        MemoryAccess.WriteWordRight(m, (c.A1 + 0x8u), c.T1);
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0xFu), c.T2);
        MemoryAccess.WriteWordRight(m, (c.A1 + 0xCu), c.T2);
        c.A0 = c.A0 + 0x10u;
        if (c.A0 != c.V0) {
            c.A1 = c.A1 + 0x10u;
            goto L80011548;
        }
        c.A1 = c.A1 + 0x10u;
        c.A0 = 0x0000001Au;
        goto L800115D0;
        L8001159C: ;
        c.V0 = c.V1 + 0x40u;
        L800115A0: ;
        c.A3 = MemoryAccess.ReadU32(m, c.A0);
        c.T0 = MemoryAccess.ReadU32(m, (c.A0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.A0 + 0xCu));
        MemoryAccess.WriteU32(m, c.A1, c.A3);
        MemoryAccess.WriteU32(m, (c.A1 + 0x4u), c.T0);
        MemoryAccess.WriteU32(m, (c.A1 + 0x8u), c.T1);
        MemoryAccess.WriteU32(m, (c.A1 + 0xCu), c.T2);
        c.A0 = c.A0 + 0x10u;
        if (c.A0 != c.V0) {
            c.A1 = c.A1 + 0x10u;
            goto L800115A0;
        }
        c.A1 = c.A1 + 0x10u;
        c.A0 = 0x0000001Au;
        L800115D0: ;
        c.S2 = 0u + 0u;
        c.FP = 0x00000008u;
        c.S7 = 0x00000040u;
        c.S6 = 0x00000004u;
        c.A1 = MemoryAccess.ReadU16(m, (c.S3 + 0xEu));
        c.S5 = 0x00000010u;
        c.RA = 0x800115ECu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V0 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.V0);
        c.T0 = MemoryAccess.ReadWordLeft(m, c.T0, (c.V0 + 0x7u));
        c.T0 = MemoryAccess.ReadWordRight(m, c.T0, (c.V0 + 0x4u));
        MemoryAccess.WriteWordLeft(m, (c.S4 + 0xE83u), c.A3);
        MemoryAccess.WriteWordRight(m, (c.S4 + 0xE80u), c.A3);
        MemoryAccess.WriteWordLeft(m, (c.S4 + 0xE87u), c.T0);
        MemoryAccess.WriteWordRight(m, (c.S4 + 0xE84u), c.T0);
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V0 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.V0);
        c.T0 = MemoryAccess.ReadWordLeft(m, c.T0, (c.V0 + 0x7u));
        c.T0 = MemoryAccess.ReadWordRight(m, c.T0, (c.V0 + 0x4u));
        MemoryAccess.WriteWordLeft(m, (c.S4 + 0xEC3u), c.A3);
        MemoryAccess.WriteWordRight(m, (c.S4 + 0xEC0u), c.A3);
        MemoryAccess.WriteWordLeft(m, (c.S4 + 0xEC7u), c.T0);
        MemoryAccess.WriteWordRight(m, (c.S4 + 0xEC4u), c.T0);
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V0 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.V0);
        c.T0 = MemoryAccess.ReadWordLeft(m, c.T0, (c.V0 + 0x7u));
        c.T0 = MemoryAccess.ReadWordRight(m, c.T0, (c.V0 + 0x4u));
        MemoryAccess.WriteWordLeft(m, (c.S4 + 0xECBu), c.A3);
        MemoryAccess.WriteWordRight(m, (c.S4 + 0xEC8u), c.A3);
        MemoryAccess.WriteWordLeft(m, (c.S4 + 0xECFu), c.T0);
        MemoryAccess.WriteWordRight(m, (c.S4 + 0xECCu), c.T0);
        L8001164C: ;
        c.A0 = c.S2 + 0x27u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x8001165Cu;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            goto L8001179C;
        }
        c.A0 = 0x00000016u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80011674u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.S3 = c.V0 + 0u;
        c.V0 = c.S4 + c.S5;
        c.S0 = c.S4 + c.S6;
        c.A3 = MemoryAccess.ReadU32(m, c.S3);
        c.T0 = MemoryAccess.ReadU32(m, (c.S3 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.S3 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.S3 + 0xCu));
        MemoryAccess.WriteU32(m, (c.V0 + 0xB10u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0xB14u), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0xB18u), c.T1);
        MemoryAccess.WriteU32(m, (c.V0 + 0xB1Cu), c.T2);
        MemoryAccess.WriteU32(m, (c.S0 + 0xBB0u), c.S1);
        c.A1 = MemoryAccess.ReadU16(m, (c.S3 + 0xAu));
        c.A0 = 0x00000018u;
        c.RA = 0x800116B0u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V0 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.V0);
        MemoryAccess.WriteWordLeft(m, (c.S0 + 0xBDBu), c.A3);
        MemoryAccess.WriteWordRight(m, (c.S0 + 0xBD8u), c.A3);
        c.A1 = MemoryAccess.ReadU16(m, (c.S3 + 0xCu));
        c.A0 = 0x00000019u;
        c.RA = 0x800116D0u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S7;
        c.V1 = c.V1 + 0xC00u;
        c.A0 = c.V0 + 0u;
        c.V0 = c.A0 | c.V1;
        c.V0 = c.V0 & 0x0003u;
        if (c.V0 == 0u) {
            c.V0 = c.A0 + 0x40u;
            goto L80011740;
        }
        c.V0 = c.A0 + 0x40u;
        L800116EC: ;
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.A0 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.A0);
        c.T0 = MemoryAccess.ReadWordLeft(m, c.T0, (c.A0 + 0x7u));
        c.T0 = MemoryAccess.ReadWordRight(m, c.T0, (c.A0 + 0x4u));
        c.T1 = MemoryAccess.ReadWordLeft(m, c.T1, (c.A0 + 0xBu));
        c.T1 = MemoryAccess.ReadWordRight(m, c.T1, (c.A0 + 0x8u));
        c.T2 = MemoryAccess.ReadWordLeft(m, c.T2, (c.A0 + 0xFu));
        c.T2 = MemoryAccess.ReadWordRight(m, c.T2, (c.A0 + 0xCu));
        MemoryAccess.WriteWordLeft(m, (c.V1 + 0x3u), c.A3);
        MemoryAccess.WriteWordRight(m, c.V1, c.A3);
        MemoryAccess.WriteWordLeft(m, (c.V1 + 0x7u), c.T0);
        MemoryAccess.WriteWordRight(m, (c.V1 + 0x4u), c.T0);
        MemoryAccess.WriteWordLeft(m, (c.V1 + 0xBu), c.T1);
        MemoryAccess.WriteWordRight(m, (c.V1 + 0x8u), c.T1);
        MemoryAccess.WriteWordLeft(m, (c.V1 + 0xFu), c.T2);
        MemoryAccess.WriteWordRight(m, (c.V1 + 0xCu), c.T2);
        c.A0 = c.A0 + 0x10u;
        if (c.A0 != c.V0) {
            c.V1 = c.V1 + 0x10u;
            goto L800116EC;
        }
        c.V1 = c.V1 + 0x10u;
        goto L8001176C;
        L80011740: ;
        c.A3 = MemoryAccess.ReadU32(m, c.A0);
        c.T0 = MemoryAccess.ReadU32(m, (c.A0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.A0 + 0xCu));
        MemoryAccess.WriteU32(m, c.V1, c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x4u), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x8u), c.T1);
        MemoryAccess.WriteU32(m, (c.V1 + 0xCu), c.T2);
        c.A0 = c.A0 + 0x10u;
        if (c.A0 != c.V0) {
            c.V1 = c.V1 + 0x10u;
            goto L80011740;
        }
        c.V1 = c.V1 + 0x10u;
        L8001176C: ;
        c.A1 = MemoryAccess.ReadU16(m, (c.S3 + 0xEu));
        c.A0 = 0x0000001Au;
        c.RA = 0x80011778u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.FP;
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V0 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.V0);
        c.T0 = MemoryAccess.ReadWordLeft(m, c.T0, (c.V0 + 0x7u));
        c.T0 = MemoryAccess.ReadWordRight(m, c.T0, (c.V0 + 0x4u));
        MemoryAccess.WriteWordLeft(m, (c.V1 + 0xE83u), c.A3);
        MemoryAccess.WriteWordRight(m, (c.V1 + 0xE80u), c.A3);
        MemoryAccess.WriteWordLeft(m, (c.V1 + 0xE87u), c.T0);
        MemoryAccess.WriteWordRight(m, (c.V1 + 0xE84u), c.T0);
        L8001179C: ;
        c.FP = c.FP + 0x8u;
        c.S7 = c.S7 + 0x40u;
        c.S6 = c.S6 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        c.V0 = (int)c.S2 < 7 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S5 = c.S5 + 0x10u;
            goto L8001164C;
        }
        c.S5 = c.S5 + 0x10u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x28u));
        c.A0 = 0x00000017u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x800117C8u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.S3 = c.V0 + 0u;
        c.A3 = MemoryAccess.ReadU32(m, c.S3);
        c.T0 = MemoryAccess.ReadU32(m, (c.S3 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.S3 + 0x8u));
        MemoryAccess.WriteU32(m, (c.S4 + 0xED0u), c.A3);
        MemoryAccess.WriteU32(m, (c.S4 + 0xED4u), c.T0);
        MemoryAccess.WriteU32(m, (c.S4 + 0xED8u), c.T1);
        MemoryAccess.WriteU32(m, (c.S4 + 0xF48u), c.S1);
        c.A3 = MemoryAccess.ReadU32(m, c.S3);
        c.T0 = MemoryAccess.ReadU32(m, (c.S3 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.S3 + 0x8u));
        MemoryAccess.WriteU32(m, (c.S4 + 0xF30u), c.A3);
        MemoryAccess.WriteU32(m, (c.S4 + 0xF34u), c.T0);
        MemoryAccess.WriteU32(m, (c.S4 + 0xF38u), c.T1);
        MemoryAccess.WriteU32(m, (c.S4 + 0xF68u), c.S1);
        c.A3 = MemoryAccess.ReadU32(m, c.S3);
        c.T0 = MemoryAccess.ReadU32(m, (c.S3 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.S3 + 0x8u));
        MemoryAccess.WriteU32(m, (c.S4 + 0xF3Cu), c.A3);
        MemoryAccess.WriteU32(m, (c.S4 + 0xF40u), c.T0);
        MemoryAccess.WriteU32(m, (c.S4 + 0xF44u), c.T1);
        MemoryAccess.WriteU32(m, (c.S4 + 0xF6Cu), c.S1);
        c.A1 = MemoryAccess.ReadU16(m, (c.S3 + 0x6u));
        c.A0 = 0x00000018u;
        c.RA = 0x8001182Cu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V0 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.V0);
        MemoryAccess.WriteWordLeft(m, (c.S4 + 0xF73u), c.A3);
        MemoryAccess.WriteWordRight(m, (c.S4 + 0xF70u), c.A3);
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V0 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.V0);
        MemoryAccess.WriteWordLeft(m, (c.S4 + 0xF93u), c.A3);
        MemoryAccess.WriteWordRight(m, (c.S4 + 0xF90u), c.A3);
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V0 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.V0);
        MemoryAccess.WriteWordLeft(m, (c.S4 + 0xF97u), c.A3);
        MemoryAccess.WriteWordRight(m, (c.S4 + 0xF94u), c.A3);
        c.A1 = MemoryAccess.ReadU16(m, (c.S3 + 0x8u));
        c.A0 = 0x00000019u;
        c.RA = 0x80011874u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.A1 = c.S4 + 0xF98u;
        c.A0 = c.V0 + 0u;
        c.V0 = c.A0 | c.A1;
        c.V0 = c.V0 & 0x0003u;
        if (c.V0 == 0u) {
            c.V1 = c.A0 + 0u;
            goto L800118E4;
        }
        c.V1 = c.A0 + 0u;
        c.V0 = c.A0 + 0x40u;
        L80011890: ;
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V1 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.V1);
        c.T0 = MemoryAccess.ReadWordLeft(m, c.T0, (c.V1 + 0x7u));
        c.T0 = MemoryAccess.ReadWordRight(m, c.T0, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadWordLeft(m, c.T1, (c.V1 + 0xBu));
        c.T1 = MemoryAccess.ReadWordRight(m, c.T1, (c.V1 + 0x8u));
        c.T2 = MemoryAccess.ReadWordLeft(m, c.T2, (c.V1 + 0xFu));
        c.T2 = MemoryAccess.ReadWordRight(m, c.T2, (c.V1 + 0xCu));
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0x3u), c.A3);
        MemoryAccess.WriteWordRight(m, c.A1, c.A3);
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0x7u), c.T0);
        MemoryAccess.WriteWordRight(m, (c.A1 + 0x4u), c.T0);
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0xBu), c.T1);
        MemoryAccess.WriteWordRight(m, (c.A1 + 0x8u), c.T1);
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0xFu), c.T2);
        MemoryAccess.WriteWordRight(m, (c.A1 + 0xCu), c.T2);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.V0) {
            c.A1 = c.A1 + 0x10u;
            goto L80011890;
        }
        c.A1 = c.A1 + 0x10u;
        c.A1 = c.S4 + 0x1198u;
        goto L80011918;
        L800118E4: ;
        c.V0 = c.A0 + 0x40u;
        L800118E8: ;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, c.A1, c.A3);
        MemoryAccess.WriteU32(m, (c.A1 + 0x4u), c.T0);
        MemoryAccess.WriteU32(m, (c.A1 + 0x8u), c.T1);
        MemoryAccess.WriteU32(m, (c.A1 + 0xCu), c.T2);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.V0) {
            c.A1 = c.A1 + 0x10u;
            goto L800118E8;
        }
        c.A1 = c.A1 + 0x10u;
        c.A1 = c.S4 + 0x1198u;
        L80011918: ;
        c.V0 = c.A0 | c.A1;
        c.V0 = c.V0 & 0x0003u;
        if (c.V0 == 0u) {
            c.V1 = c.A0 + 0u;
            goto L80011980;
        }
        c.V1 = c.A0 + 0u;
        c.V0 = c.A0 + 0x40u;
        L8001192C: ;
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V1 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.V1);
        c.T0 = MemoryAccess.ReadWordLeft(m, c.T0, (c.V1 + 0x7u));
        c.T0 = MemoryAccess.ReadWordRight(m, c.T0, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadWordLeft(m, c.T1, (c.V1 + 0xBu));
        c.T1 = MemoryAccess.ReadWordRight(m, c.T1, (c.V1 + 0x8u));
        c.T2 = MemoryAccess.ReadWordLeft(m, c.T2, (c.V1 + 0xFu));
        c.T2 = MemoryAccess.ReadWordRight(m, c.T2, (c.V1 + 0xCu));
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0x3u), c.A3);
        MemoryAccess.WriteWordRight(m, c.A1, c.A3);
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0x7u), c.T0);
        MemoryAccess.WriteWordRight(m, (c.A1 + 0x4u), c.T0);
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0xBu), c.T1);
        MemoryAccess.WriteWordRight(m, (c.A1 + 0x8u), c.T1);
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0xFu), c.T2);
        MemoryAccess.WriteWordRight(m, (c.A1 + 0xCu), c.T2);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.V0) {
            c.A1 = c.A1 + 0x10u;
            goto L8001192C;
        }
        c.A1 = c.A1 + 0x10u;
        c.A1 = c.S4 + 0x11D8u;
        goto L800119B4;
        L80011980: ;
        c.V0 = c.A0 + 0x40u;
        L80011984: ;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, c.A1, c.A3);
        MemoryAccess.WriteU32(m, (c.A1 + 0x4u), c.T0);
        MemoryAccess.WriteU32(m, (c.A1 + 0x8u), c.T1);
        MemoryAccess.WriteU32(m, (c.A1 + 0xCu), c.T2);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.V0) {
            c.A1 = c.A1 + 0x10u;
            goto L80011984;
        }
        c.A1 = c.A1 + 0x10u;
        c.A1 = c.S4 + 0x11D8u;
        L800119B4: ;
        c.V0 = c.A0 | c.A1;
        c.V0 = c.V0 & 0x0003u;
        if (c.V0 == 0u) {
            c.V1 = c.A0 + 0u;
            goto L80011A1C;
        }
        c.V1 = c.A0 + 0u;
        c.V0 = c.A0 + 0x40u;
        L800119C8: ;
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V1 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.V1);
        c.T0 = MemoryAccess.ReadWordLeft(m, c.T0, (c.V1 + 0x7u));
        c.T0 = MemoryAccess.ReadWordRight(m, c.T0, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadWordLeft(m, c.T1, (c.V1 + 0xBu));
        c.T1 = MemoryAccess.ReadWordRight(m, c.T1, (c.V1 + 0x8u));
        c.T2 = MemoryAccess.ReadWordLeft(m, c.T2, (c.V1 + 0xFu));
        c.T2 = MemoryAccess.ReadWordRight(m, c.T2, (c.V1 + 0xCu));
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0x3u), c.A3);
        MemoryAccess.WriteWordRight(m, c.A1, c.A3);
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0x7u), c.T0);
        MemoryAccess.WriteWordRight(m, (c.A1 + 0x4u), c.T0);
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0xBu), c.T1);
        MemoryAccess.WriteWordRight(m, (c.A1 + 0x8u), c.T1);
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0xFu), c.T2);
        MemoryAccess.WriteWordRight(m, (c.A1 + 0xCu), c.T2);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.V0) {
            c.A1 = c.A1 + 0x10u;
            goto L800119C8;
        }
        c.A1 = c.A1 + 0x10u;
        c.S2 = 0u + 0u;
        goto L80011A50;
        L80011A1C: ;
        c.V0 = c.A0 + 0x40u;
        L80011A20: ;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, c.A1, c.A3);
        MemoryAccess.WriteU32(m, (c.A1 + 0x4u), c.T0);
        MemoryAccess.WriteU32(m, (c.A1 + 0x8u), c.T1);
        MemoryAccess.WriteU32(m, (c.A1 + 0xCu), c.T2);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.V0) {
            c.A1 = c.A1 + 0x10u;
            goto L80011A20;
        }
        c.A1 = c.A1 + 0x10u;
        c.S2 = 0u + 0u;
        L80011A50: ;
        c.S7 = 0x00000040u;
        c.S6 = 0x00000004u;
        c.S5 = 0x0000000Cu;
        L80011A5C: ;
        c.A0 = c.S2 + 0x27u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x80011A6Cu;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x00000017u;
            goto L80011B70;
        }
        c.A0 = 0x00000017u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80011A80u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.S3 = c.V0 + 0u;
        c.V0 = c.S4 + c.S5;
        c.S0 = c.S4 + c.S6;
        c.A3 = MemoryAccess.ReadU32(m, c.S3);
        c.T0 = MemoryAccess.ReadU32(m, (c.S3 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.S3 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V0 + 0xED0u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0xED4u), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0xED8u), c.T1);
        MemoryAccess.WriteU32(m, (c.S0 + 0xF48u), c.S1);
        c.A1 = MemoryAccess.ReadU16(m, (c.S3 + 0x6u));
        c.A0 = 0x00000018u;
        c.RA = 0x80011AB4u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V0 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.V0);
        MemoryAccess.WriteWordLeft(m, (c.S0 + 0xF73u), c.A3);
        MemoryAccess.WriteWordRight(m, (c.S0 + 0xF70u), c.A3);
        c.A1 = MemoryAccess.ReadU16(m, (c.S3 + 0x8u));
        c.A0 = 0x00000019u;
        c.RA = 0x80011AD4u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S7;
        c.V1 = c.V1 + 0xF98u;
        c.A0 = c.V0 + 0u;
        c.V0 = c.A0 | c.V1;
        c.V0 = c.V0 & 0x0003u;
        if (c.V0 == 0u) {
            c.V0 = c.A0 + 0x40u;
            goto L80011B44;
        }
        c.V0 = c.A0 + 0x40u;
        L80011AF0: ;
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.A0 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.A0);
        c.T0 = MemoryAccess.ReadWordLeft(m, c.T0, (c.A0 + 0x7u));
        c.T0 = MemoryAccess.ReadWordRight(m, c.T0, (c.A0 + 0x4u));
        c.T1 = MemoryAccess.ReadWordLeft(m, c.T1, (c.A0 + 0xBu));
        c.T1 = MemoryAccess.ReadWordRight(m, c.T1, (c.A0 + 0x8u));
        c.T2 = MemoryAccess.ReadWordLeft(m, c.T2, (c.A0 + 0xFu));
        c.T2 = MemoryAccess.ReadWordRight(m, c.T2, (c.A0 + 0xCu));
        MemoryAccess.WriteWordLeft(m, (c.V1 + 0x3u), c.A3);
        MemoryAccess.WriteWordRight(m, c.V1, c.A3);
        MemoryAccess.WriteWordLeft(m, (c.V1 + 0x7u), c.T0);
        MemoryAccess.WriteWordRight(m, (c.V1 + 0x4u), c.T0);
        MemoryAccess.WriteWordLeft(m, (c.V1 + 0xBu), c.T1);
        MemoryAccess.WriteWordRight(m, (c.V1 + 0x8u), c.T1);
        MemoryAccess.WriteWordLeft(m, (c.V1 + 0xFu), c.T2);
        MemoryAccess.WriteWordRight(m, (c.V1 + 0xCu), c.T2);
        c.A0 = c.A0 + 0x10u;
        if (c.A0 != c.V0) {
            c.V1 = c.V1 + 0x10u;
            goto L80011AF0;
        }
        c.V1 = c.V1 + 0x10u;
        c.S7 = c.S7 + 0x40u;
        goto L80011B74;
        L80011B44: ;
        c.A3 = MemoryAccess.ReadU32(m, c.A0);
        c.T0 = MemoryAccess.ReadU32(m, (c.A0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.A0 + 0xCu));
        MemoryAccess.WriteU32(m, c.V1, c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x4u), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x8u), c.T1);
        MemoryAccess.WriteU32(m, (c.V1 + 0xCu), c.T2);
        c.A0 = c.A0 + 0x10u;
        if (c.A0 != c.V0) {
            c.V1 = c.V1 + 0x10u;
            goto L80011B44;
        }
        c.V1 = c.V1 + 0x10u;
        L80011B70: ;
        c.S7 = c.S7 + 0x40u;
        L80011B74: ;
        c.S6 = c.S6 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        c.V0 = (int)c.S2 < 7 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S5 = c.S5 + 0xCu;
            goto L80011A5C;
        }
        c.S5 = c.S5 + 0xCu;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x20u));
        c.A0 = 0x00000011u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80011B98u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0x8u));
        c.A1 = c.V1 + 0x20u;
        c.V0 = c.A0 << 3;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.S4 + c.V0;
        c.V0 = c.V0 + 0x1218u;
        L80011BB8: ;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, c.V0, c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x4u), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x8u), c.T1);
        MemoryAccess.WriteU32(m, (c.V0 + 0xCu), c.T2);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.A1) {
            c.V0 = c.V0 + 0x10u;
            goto L80011BB8;
        }
        c.V0 = c.V0 + 0x10u;
        c.S2 = 0u + 0u;
        c.S3 = 0x00000004u;
        c.S0 = 0x00000024u;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        MemoryAccess.WriteU32(m, c.V0, c.A3);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0x12A8u), c.S1);
        L80011C08: ;
        c.A0 = c.S2 + 0xDu;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x80011C18u;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x00000011u;
            goto L80011C78;
        }
        c.A0 = 0x00000011u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80011C2Cu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S0;
        c.V1 = c.V1 + 0x1218u;
        c.A0 = c.V0 + 0x20u;
        L80011C38: ;
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
            goto L80011C38;
        }
        c.V1 = c.V1 + 0x10u;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        MemoryAccess.WriteU32(m, c.V1, c.A3);
        c.V0 = c.S4 + c.S3;
        MemoryAccess.WriteU32(m, (c.V0 + 0x12A8u), c.S1);
        L80011C78: ;
        c.S3 = c.S3 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        c.V0 = (int)c.S2 < 3 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S0 = c.S0 + 0x24u;
            goto L80011C08;
        }
        c.S0 = c.S0 + 0x24u;
        c.A0 = 0x0000000Fu;
        c.S2 = 0u + 0u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x3Cu));
        c.S3 = 0x00000004u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80011CA4u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0x8u));
        c.S0 = 0x00000010u;
        c.V0 = c.A0 << (int)(c.S3 & 31u);
        c.V0 = c.S4 + c.V0;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, (c.V0 + 0x12B8u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x12BCu), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x12C0u), c.T1);
        MemoryAccess.WriteU32(m, (c.V0 + 0x12C4u), c.T2);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0x12F8u), c.S1);
        L80011CE4: ;
        c.A0 = c.S2 + 0x3u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x80011CF4u;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x0000000Fu;
            goto L80011D34;
        }
        c.A0 = 0x0000000Fu;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80011D08u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S0;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V0 + 0xCu));
        MemoryAccess.WriteU32(m, (c.V1 + 0x12B8u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x12BCu), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x12C0u), c.T1);
        MemoryAccess.WriteU32(m, (c.V1 + 0x12C4u), c.T2);
        c.V0 = c.S4 + c.S3;
        MemoryAccess.WriteU32(m, (c.V0 + 0x12F8u), c.S1);
        L80011D34: ;
        c.S3 = c.S3 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        c.V0 = (int)c.S2 < 3 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S0 = c.S0 + 0x10u;
            goto L80011CE4;
        }
        c.S0 = c.S0 + 0x10u;
        c.A0 = 0x0000000Eu;
        c.S2 = 0u + 0u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x3Au));
        c.S3 = 0x00000004u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80011D60u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0x8u));
        c.S0 = 0x0000000Cu;
        c.V0 = c.A0 << 1;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.S4 + c.V0;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V0 + 0x1308u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x130Cu), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1310u), c.T1);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1338u), c.S1);
        L80011DA0: ;
        c.A0 = c.S2 + 0xAu;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x80011DB0u;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x0000000Eu;
            goto L80011DE8;
        }
        c.A0 = 0x0000000Eu;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80011DC4u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S0;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V1 + 0x1308u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x130Cu), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x1310u), c.T1);
        c.V0 = c.S4 + c.S3;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1338u), c.S1);
        L80011DE8: ;
        c.S3 = c.S3 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        c.V0 = (int)c.S2 < 3 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S0 = c.S0 + 0xCu;
            goto L80011DA0;
        }
        c.S0 = c.S0 + 0xCu;
        c.A0 = 0x00000010u;
        c.S2 = 0u + 0u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x3Eu));
        c.S3 = 0x00000004u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80011E14u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0x8u));
        c.S0 = 0x0000000Cu;
        c.V0 = c.A0 << 1;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.S4 + c.V0;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V0 + 0x1348u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x134Cu), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1350u), c.T1);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1360u), c.S1);
        L80011E54: ;
        c.A0 = c.S2 + 0x21u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x80011E64u;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x00000010u;
            goto L80011E9C;
        }
        c.A0 = 0x00000010u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80011E78u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S0;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V1 + 0x1348u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x134Cu), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x1350u), c.T1);
        c.V0 = c.S4 + c.S3;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1360u), c.S1);
        L80011E9C: ;
        c.S3 = c.S3 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        if ((int)c.S2 <= 0) {
            c.S0 = c.S0 + 0xCu;
            goto L80011E54;
        }
        c.S0 = c.S0 + 0xCu;
        c.A0 = 0x0000000Au;
        c.S2 = 0u + 0u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x34u));
        c.S3 = 0x00000004u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80011EC4u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0x8u));
        c.S0 = 0x0000000Cu;
        c.V0 = c.A0 << 1;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.S4 + c.V0;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V0 + 0x1368u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x136Cu), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1370u), c.T1);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1380u), c.S1);
        L80011F04: ;
        c.A0 = c.S2 + 0x6u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x80011F14u;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x0000000Au;
            goto L80011F4C;
        }
        c.A0 = 0x0000000Au;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80011F28u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S0;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V1 + 0x1368u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x136Cu), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x1370u), c.T1);
        c.V0 = c.S4 + c.S3;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1380u), c.S1);
        L80011F4C: ;
        c.S3 = c.S3 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        if ((int)c.S2 <= 0) {
            c.S0 = c.S0 + 0xCu;
            goto L80011F04;
        }
        c.S0 = c.S0 + 0xCu;
        c.A0 = 0x00000007u;
        c.S2 = 0u + 0u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x2Eu));
        c.S3 = 0x00000004u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80011F74u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0x8u));
        c.S0 = 0x0000000Cu;
        c.V0 = c.A0 << 1;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.S4 + c.V0;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V0 + 0x1388u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x138Cu), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1390u), c.T1);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0x13A0u), c.S1);
        L80011FB4: ;
        c.A0 = c.S2 + 0x20u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x80011FC4u;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x00000007u;
            goto L80011FFC;
        }
        c.A0 = 0x00000007u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80011FD8u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S0;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V1 + 0x1388u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x138Cu), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x1390u), c.T1);
        c.V0 = c.S4 + c.S3;
        MemoryAccess.WriteU32(m, (c.V0 + 0x13A0u), c.S1);
        L80011FFC: ;
        c.S3 = c.S3 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        if ((int)c.S2 <= 0) {
            c.S0 = c.S0 + 0xCu;
            goto L80011FB4;
        }
        c.S0 = c.S0 + 0xCu;
        c.A0 = 0x00000008u;
        c.S2 = 0u + 0u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x30u));
        c.S3 = 0x00000004u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80012024u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0x8u));
        c.S0 = 0x0000000Cu;
        c.V0 = c.A0 << 1;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.S4 + c.V0;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V0 + 0x13A8u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x13ACu), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x13B0u), c.T1);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0x13C0u), c.S1);
        L80012064: ;
        c.A0 = c.S2 + 0x9u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x80012074u;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x00000008u;
            goto L800120AC;
        }
        c.A0 = 0x00000008u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80012088u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S0;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V1 + 0x13A8u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x13ACu), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x13B0u), c.T1);
        c.V0 = c.S4 + c.S3;
        MemoryAccess.WriteU32(m, (c.V0 + 0x13C0u), c.S1);
        L800120AC: ;
        c.S3 = c.S3 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        if ((int)c.S2 <= 0) {
            c.S0 = c.S0 + 0xCu;
            goto L80012064;
        }
        c.S0 = c.S0 + 0xCu;
        c.A0 = 0x0000000Bu;
        c.S2 = 0u + 0u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x36u));
        c.S3 = 0x00000004u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x800120D4u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0x8u));
        c.S0 = 0x0000000Cu;
        c.V0 = c.A0 << 1;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.S4 + c.V0;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V0 + 0x13C8u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x13CCu), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x13D0u), c.T1);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0x13F8u), c.S1);
        L80012114: ;
        c.A0 = c.S2 + 0x1Du;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x80012124u;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x0000000Bu;
            goto L8001215C;
        }
        c.A0 = 0x0000000Bu;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80012138u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S0;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V1 + 0x13C8u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x13CCu), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x13D0u), c.T1);
        c.V0 = c.S4 + c.S3;
        MemoryAccess.WriteU32(m, (c.V0 + 0x13F8u), c.S1);
        L8001215C: ;
        c.S3 = c.S3 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        c.V0 = (int)c.S2 < 3 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S0 = c.S0 + 0xCu;
            goto L80012114;
        }
        c.S0 = c.S0 + 0xCu;
        c.A0 = 0x00000009u;
        c.S2 = 0u + 0u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x32u));
        c.S3 = 0x00000004u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80012188u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0x8u));
        c.S0 = 0x0000000Cu;
        c.V0 = c.A0 << 1;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.S4 + c.V0;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V0 + 0x1408u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x140Cu), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1410u), c.T1);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1420u), c.S1);
        L800121C8: ;
        c.A0 = c.S2 + 0x7u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x800121D8u;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x00000009u;
            goto L80012210;
        }
        c.A0 = 0x00000009u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x800121ECu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S0;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V1 + 0x1408u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x140Cu), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x1410u), c.T1);
        c.V0 = c.S4 + c.S3;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1420u), c.S1);
        L80012210: ;
        c.S3 = c.S3 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        if ((int)c.S2 <= 0) {
            c.S0 = c.S0 + 0xCu;
            goto L800121C8;
        }
        c.S0 = c.S0 + 0xCu;
        c.A0 = 0x0000000Cu;
        c.S2 = 0u + 0u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x38u));
        c.S3 = 0x00000014u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80012238u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.A1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.A1 + 0x8u));
        c.S0 = 0x00000004u;
        c.V1 = c.A0 << 2;
        c.V0 = c.V1 + c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.S4 + c.V0;
        c.V1 = c.S4 + c.V1;
        c.A3 = MemoryAccess.ReadU32(m, c.A1);
        c.T0 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.A1 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.A1 + 0xCu));
        MemoryAccess.WriteU32(m, (c.V0 + 0x1428u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x142Cu), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1430u), c.T1);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1434u), c.T2);
        c.A3 = MemoryAccess.ReadU32(m, (c.A1 + 0x10u));
        MemoryAccess.WriteU32(m, (c.V0 + 0x1438u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x148Cu), c.S1);
        L80012288: ;
        c.A0 = c.S2 + 0x2Eu;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x80012298u;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x0000000Cu;
            goto L800122E4;
        }
        c.A0 = 0x0000000Cu;
        c.A1 = c.S1 + 0u;
        c.RA = 0x800122ACu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S3;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V0 + 0xCu));
        MemoryAccess.WriteU32(m, (c.V1 + 0x1428u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x142Cu), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x1430u), c.T1);
        MemoryAccess.WriteU32(m, (c.V1 + 0x1434u), c.T2);
        c.A3 = MemoryAccess.ReadU32(m, (c.V0 + 0x10u));
        MemoryAccess.WriteU32(m, (c.V1 + 0x1438u), c.A3);
        c.V0 = c.S4 + c.S0;
        MemoryAccess.WriteU32(m, (c.V0 + 0x148Cu), c.S1);
        L800122E4: ;
        c.S3 = c.S3 + 0x14u;
        c.S2 = c.S2 + 0x1u;
        c.V0 = (int)c.S2 < 4 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S0 = c.S0 + 0x4u;
            goto L80012288;
        }
        c.S0 = c.S0 + 0x4u;
        c.A0 = 0x00000013u;
        c.S2 = 0u + 0u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x42u));
        c.S3 = 0x00000004u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80012310u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0x8u));
        c.S0 = 0x0000000Cu;
        c.V0 = c.A0 << 1;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.S4 + c.V0;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V0 + 0x14A0u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x14A4u), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x14A8u), c.T1);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0x14C4u), c.S1);
        L80012350: ;
        c.A0 = c.S2 + 0x10u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x80012360u;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x00000013u;
            goto L80012398;
        }
        c.A0 = 0x00000013u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80012374u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S0;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V1 + 0x14A0u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x14A4u), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x14A8u), c.T1);
        c.V0 = c.S4 + c.S3;
        MemoryAccess.WriteU32(m, (c.V0 + 0x14C4u), c.S1);
        L80012398: ;
        c.S3 = c.S3 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        c.V0 = (int)c.S2 < 2 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S0 = c.S0 + 0xCu;
            goto L80012350;
        }
        c.S0 = c.S0 + 0xCu;
        c.A0 = 0x00000014u;
        c.S2 = 0u + 0u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x40u));
        c.S3 = 0x00000004u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x800123C4u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0x8u));
        c.S0 = 0x0000000Cu;
        c.V0 = c.A0 << 1;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.S4 + c.V0;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V0 + 0x14D0u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x14D4u), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x14D8u), c.T1);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1500u), c.S1);
        L80012404: ;
        c.A0 = c.S2 + 0x1Au;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x80012414u;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x00000014u;
            goto L8001244C;
        }
        c.A0 = 0x00000014u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80012428u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S0;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V1 + 0x14D0u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x14D4u), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x14D8u), c.T1);
        c.V0 = c.S4 + c.S3;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1500u), c.S1);
        L8001244C: ;
        c.S3 = c.S3 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        c.V0 = (int)c.S2 < 3 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S0 = c.S0 + 0xCu;
            goto L80012404;
        }
        c.S0 = c.S0 + 0xCu;
        c.A0 = 0x00000004u;
        c.S2 = 0u + 0u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x2Au));
        c.S3 = c.A0 + 0u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80012478u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0xBu));
        c.S0 = 0x0000000Cu;
        c.V0 = c.A0 << 1;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.S4 + c.V0;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V0 + 0x1510u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1514u), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1518u), c.T1);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1540u), c.S1);
        L800124B8: ;
        c.A0 = c.S2 + 0x12u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x800124C8u;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x00000004u;
            goto L80012500;
        }
        c.A0 = 0x00000004u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x800124DCu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S0;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V1 + 0x1510u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x1514u), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x1518u), c.T1);
        c.V0 = c.S4 + c.S3;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1540u), c.S1);
        L80012500: ;
        c.S3 = c.S3 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        c.V0 = (int)c.S2 < 3 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S0 = c.S0 + 0xCu;
            goto L800124B8;
        }
        c.S0 = c.S0 + 0xCu;
        c.A0 = 0x00000005u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x2Cu));
        c.S2 = 0x00000001u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80012528u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.S3 = c.S4 + 0x4u;
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0xEu));
        c.S0 = c.S4 + 0x1Cu;
        c.V0 = c.A0 << 3;
        c.V0 = c.V0 - c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.S4 + c.V0;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, (c.V0 + 0x1550u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1554u), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1558u), c.T1);
        MemoryAccess.WriteU32(m, (c.V0 + 0x155Cu), c.T2);
        c.A3 = MemoryAccess.ReadU32(m, (c.V1 + 0x10u));
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x14u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x18u));
        MemoryAccess.WriteU32(m, (c.V0 + 0x1560u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1564u), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1568u), c.T1);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0x15DCu), c.S1);
        L8001258C: ;
        c.V0 = (int)c.S2 < 5 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A1 = c.S2 & 0x00FFu;
            goto L80012604;
        }
        c.A1 = c.S2 & 0x00FFu;
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = c.SP + 0x98u;
        c.RA = 0x800125A4u;
        GranTurismo2PC.func_80076500(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x00000005u;
            goto L800125F4;
        }
        c.A0 = 0x00000005u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x800125B8u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V0 + 0xCu));
        MemoryAccess.WriteU32(m, (c.S0 + 0x1550u), c.A3);
        MemoryAccess.WriteU32(m, (c.S0 + 0x1554u), c.T0);
        MemoryAccess.WriteU32(m, (c.S0 + 0x1558u), c.T1);
        MemoryAccess.WriteU32(m, (c.S0 + 0x155Cu), c.T2);
        c.A3 = MemoryAccess.ReadU32(m, (c.V0 + 0x10u));
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x14u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x18u));
        MemoryAccess.WriteU32(m, (c.S0 + 0x1560u), c.A3);
        MemoryAccess.WriteU32(m, (c.S0 + 0x1564u), c.T0);
        MemoryAccess.WriteU32(m, (c.S0 + 0x1568u), c.T1);
        MemoryAccess.WriteU32(m, (c.S3 + 0x15DCu), c.S1);
        L800125F4: ;
        c.S3 = c.S3 + 0x4u;
        c.S0 = c.S0 + 0x1Cu;
        c.S2 = c.S2 + 0x1u;
        goto L8001258C;
        L80012604: ;
        c.A0 = 0x0000001Bu;
        c.S2 = 0u + 0u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x44u));
        c.S3 = 0x00000004u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x8001261Cu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0x8u));
        c.S0 = 0x00000010u;
        c.V0 = c.A0 << (int)(c.S3 & 31u);
        c.V0 = c.S4 + c.V0;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, (c.V0 + 0x15F0u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x15F4u), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x15F8u), c.T1);
        MemoryAccess.WriteU32(m, (c.V0 + 0x15FCu), c.T2);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1610u), c.S1);
        L8001265C: ;
        c.A0 = c.S2 + 0u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x8001266Cu;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x0000001Bu;
            goto L800126AC;
        }
        c.A0 = 0x0000001Bu;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80012680u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S0;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V0 + 0xCu));
        MemoryAccess.WriteU32(m, (c.V1 + 0x15F0u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x15F4u), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x15F8u), c.T1);
        MemoryAccess.WriteU32(m, (c.V1 + 0x15FCu), c.T2);
        c.V0 = c.S4 + c.S3;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1610u), c.S1);
        L800126AC: ;
        c.S3 = c.S3 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        if ((int)c.S2 <= 0) {
            c.S0 = c.S0 + 0x10u;
            goto L8001265C;
        }
        c.S0 = c.S0 + 0x10u;
        c.A0 = 0x0000001Cu;
        c.S2 = 0u + 0u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x46u));
        c.S3 = 0x00000004u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x800126D4u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0x8u));
        c.S0 = 0x00000010u;
        c.V0 = c.A0 << (int)(c.S3 & 31u);
        c.V0 = c.S4 + c.V0;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, (c.V0 + 0x1618u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x161Cu), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1620u), c.T1);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1624u), c.T2);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1638u), c.S1);
        L80012714: ;
        c.A0 = c.S2 + 0x26u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x80012724u;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x0000001Cu;
            goto L80012764;
        }
        c.A0 = 0x0000001Cu;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80012738u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S0;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V0 + 0xCu));
        MemoryAccess.WriteU32(m, (c.V1 + 0x1618u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x161Cu), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x1620u), c.T1);
        MemoryAccess.WriteU32(m, (c.V1 + 0x1624u), c.T2);
        c.V0 = c.S4 + c.S3;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1638u), c.S1);
        L80012764: ;
        c.S3 = c.S3 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        if ((int)c.S2 <= 0) {
            c.S0 = c.S0 + 0x10u;
            goto L80012714;
        }
        c.S0 = c.S0 + 0x10u;
        c.A0 = 0x00000015u;
        c.S2 = 0u + 0u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x24u));
        c.S3 = 0x00000004u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x8001278Cu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0x8u));
        c.S0 = 0x00000020u;
        c.V0 = c.A0 << 5;
        c.V0 = c.S4 + c.V0;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, (c.V0 + 0x1640u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1644u), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1648u), c.T1);
        MemoryAccess.WriteU32(m, (c.V0 + 0x164Cu), c.T2);
        c.A3 = MemoryAccess.ReadU32(m, (c.V1 + 0x10u));
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x14u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x18u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V1 + 0x1Cu));
        MemoryAccess.WriteU32(m, (c.V0 + 0x1650u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1654u), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1658u), c.T1);
        MemoryAccess.WriteU32(m, (c.V0 + 0x165Cu), c.T2);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1700u), c.S1);
        L800127EC: ;
        c.A0 = c.S2 + 0x15u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x800127FCu;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x00000015u;
            goto L8001285C;
        }
        c.A0 = 0x00000015u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80012810u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S0;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V0 + 0xCu));
        MemoryAccess.WriteU32(m, (c.V1 + 0x1640u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x1644u), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x1648u), c.T1);
        MemoryAccess.WriteU32(m, (c.V1 + 0x164Cu), c.T2);
        c.A3 = MemoryAccess.ReadU32(m, (c.V0 + 0x10u));
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x14u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x18u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V0 + 0x1Cu));
        MemoryAccess.WriteU32(m, (c.V1 + 0x1650u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x1654u), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x1658u), c.T1);
        MemoryAccess.WriteU32(m, (c.V1 + 0x165Cu), c.T2);
        c.V0 = c.S4 + c.S3;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1700u), c.S1);
        L8001285C: ;
        c.S3 = c.S3 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        c.V0 = (int)c.S2 < 5 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S0 = c.S0 + 0x20u;
            goto L800127EC;
        }
        c.S0 = c.S0 + 0x20u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x18u));
        c.A0 = 0x00000002u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80012880u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V0 + 0xCu));
        MemoryAccess.WriteU32(m, (c.S4 + 0x1718u), c.A3);
        MemoryAccess.WriteU32(m, (c.S4 + 0x171Cu), c.T0);
        MemoryAccess.WriteU32(m, (c.S4 + 0x1720u), c.T1);
        MemoryAccess.WriteU32(m, (c.S4 + 0x1724u), c.T2);
        c.A3 = MemoryAccess.ReadU32(m, (c.V0 + 0x10u));
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x14u));
        MemoryAccess.WriteU32(m, (c.S4 + 0x1728u), c.A3);
        MemoryAccess.WriteU32(m, (c.S4 + 0x172Cu), c.T0);
        MemoryAccess.WriteU32(m, (c.S4 + 0x1730u), c.S1);
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x1Au));
        c.A0 = 0x00000003u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x800128C4u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V0 + 0xCu));
        MemoryAccess.WriteU32(m, (c.S4 + 0x1734u), c.A3);
        MemoryAccess.WriteU32(m, (c.S4 + 0x1738u), c.T0);
        MemoryAccess.WriteU32(m, (c.S4 + 0x173Cu), c.T1);
        MemoryAccess.WriteU32(m, (c.S4 + 0x1740u), c.T2);
        c.A3 = MemoryAccess.ReadU32(m, (c.V0 + 0x10u));
        MemoryAccess.WriteU32(m, (c.S4 + 0x1744u), c.A3);
        MemoryAccess.WriteU32(m, (c.S4 + 0x1748u), c.S1);
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x1Cu));
        c.A0 = 0x00000006u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80012904u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + 0x174Cu;
        c.A0 = c.V0 + 0x40u;
        L8001290C: ;
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
            goto L8001290C;
        }
        c.V1 = c.V1 + 0x10u;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU32(m, c.V1, c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x4u), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x8u), c.T1);
        MemoryAccess.WriteU32(m, (c.S4 + 0x1798u), c.S1);
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x1Eu));
        c.A0 = 0x0000000Du;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80012964u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V0 + 0xCu));
        MemoryAccess.WriteU32(m, (c.S4 + 0x179Cu), c.A3);
        MemoryAccess.WriteU32(m, (c.S4 + 0x17A0u), c.T0);
        MemoryAccess.WriteU32(m, (c.S4 + 0x17A4u), c.T1);
        MemoryAccess.WriteU32(m, (c.S4 + 0x17A8u), c.T2);
        MemoryAccess.WriteU32(m, (c.S4 + 0x17ACu), c.S1);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0xC4u));
        c.FP = MemoryAccess.ReadU32(m, (c.SP + 0xC0u));
        c.S7 = MemoryAccess.ReadU32(m, (c.SP + 0xBCu));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0xB8u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0xB4u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0xB0u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0xACu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0xA8u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0xA4u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0xA0u));
        c.SP = c.SP + 0xC8u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800129B8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A1 + 0u;
        c.V1 = c.S0 + 0u;
        c.V0 = c.S1 + 0u;
        c.A0 = c.S1 + 0x80u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        L800129DC: ;
        c.A2 = MemoryAccess.ReadU32(m, c.V0);
        c.A3 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0xCu));
        MemoryAccess.WriteU32(m, c.V1, c.A2);
        MemoryAccess.WriteU32(m, (c.V1 + 0x4u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x8u), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0xCu), c.T1);
        c.V0 = c.V0 + 0x10u;
        if (c.V0 != c.A0) {
            c.V1 = c.V1 + 0x10u;
            goto L800129DC;
        }
        c.V1 = c.V1 + 0x10u;
        c.A2 = MemoryAccess.ReadU32(m, c.V0);
        MemoryAccess.WriteU32(m, c.V1, c.A2);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x12u));
        c.A0 = 0x00000012u;
        c.RA = 0x80012A20u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17B0u), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x4u));
        c.A0 = 0u + 0u;
        c.RA = 0x80012A38u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17B2u), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x6u));
        c.A0 = 0x00000001u;
        c.RA = 0x80012A50u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17B4u), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x16u));
        c.A0 = 0x00000016u;
        c.RA = 0x80012A68u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17B6u), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x18u));
        c.A0 = 0x00000017u;
        c.RA = 0x80012A80u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x4u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17B8u), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x10u));
        c.A0 = 0x00000011u;
        c.RA = 0x80012A98u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17BCu), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x2Cu));
        c.A0 = 0x0000000Fu;
        c.RA = 0x80012AB0u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17BEu), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x2Au));
        c.A0 = 0x0000000Eu;
        c.RA = 0x80012AC8u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17C0u), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x2Eu));
        c.A0 = 0x00000010u;
        c.RA = 0x80012AE0u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17C2u), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x24u));
        c.A0 = 0x0000000Au;
        c.RA = 0x80012AF8u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17C4u), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x1Eu));
        c.A0 = 0x00000007u;
        c.RA = 0x80012B10u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17C6u), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x20u));
        c.A0 = 0x00000008u;
        c.RA = 0x80012B28u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17C8u), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x26u));
        c.A0 = 0x0000000Bu;
        c.RA = 0x80012B40u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17CAu), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x22u));
        c.A0 = 0x00000009u;
        c.RA = 0x80012B58u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17CCu), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x28u));
        c.A0 = 0x0000000Cu;
        c.RA = 0x80012B70u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17CEu), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x32u));
        c.A0 = 0x00000013u;
        c.RA = 0x80012B88u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17D0u), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x30u));
        c.A0 = 0x00000014u;
        c.RA = 0x80012BA0u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17D2u), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x1Au));
        c.A0 = 0x00000004u;
        c.RA = 0x80012BB8u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0xBu));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17D4u), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x1Cu));
        c.A0 = 0x00000005u;
        c.RA = 0x80012BD0u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0xEu));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17D6u), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x34u));
        c.A0 = 0x0000001Bu;
        c.RA = 0x80012BE8u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17D8u), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x36u));
        c.A0 = 0x0000001Cu;
        c.RA = 0x80012C00u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17DAu), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x14u));
        c.A0 = 0x00000015u;
        c.RA = 0x80012C18u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17DCu), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x8u));
        c.A0 = 0x00000002u;
        c.RA = 0x80012C30u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17E0u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.S0 + 0x17E2u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.S0 + 0x17DEu), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0xEu));
        c.A0 = 0x0000000Du;
        c.RA = 0x80012C4Cu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x4u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17E4u), (ushort)c.V0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80012C6C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x48u;
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.FP);
        c.FP = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x3Cu), c.S7);
        c.S7 = c.A1 + 0u;
        c.V0 = 0x80090000u;
        c.A1 = c.FP + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S0);
        c.S0 = 0x801D0000u;
        c.S0 = c.S0 - 0x6720u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 + 0x2E6Cu));
        c.V0 = 0u | 0xBF7Cu;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S2);
        c.S2 = c.S0 + c.V0;
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x38u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x34u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.S3);
        c.A0 = c.S1 + 0u;
        c.RA = 0x80012CC4u;
        GranTurismo2PC.func_8007830C(c, m);
        c.A0 = c.S2 + 0u;
        c.A1 = 0u + 0u;
        c.A2 = 0x0000058Cu;
        c.V1 = c.S0 + 0x3C74u;
        c.S4 = (uint)(short)MemoryAccess.ReadU16(m, (c.V1 + 0x4018u));
        c.S3 = c.V0 + 0u;
        c.V0 = c.S4 << 2;
        c.V0 = c.V0 + c.S4;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.S4;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + 0x4u;
        c.S5 = c.V0 + c.V1;
        c.RA = 0x80012CFCu;
        GranTurismo2PC.func_8008CE30(c, m);
        c.V0 = 0x00000002u;
        c.T0 = MemoryAccess.ReadWordLeft(m, c.T0, (c.S0 + 0x4u));
        c.T0 = MemoryAccess.ReadWordRight(m, c.T0, (c.S0 + 0x1u));
        c.T1 = MemoryAccess.ReadWordLeft(m, c.T1, (c.S0 + 0x8u));
        c.T1 = MemoryAccess.ReadWordRight(m, c.T1, (c.S0 + 0x5u));
        MemoryAccess.WriteWordLeft(m, (c.S2 + 0x3u), c.T0);
        MemoryAccess.WriteWordRight(m, c.S2, c.T0);
        MemoryAccess.WriteWordLeft(m, (c.S2 + 0x7u), c.T1);
        MemoryAccess.WriteWordRight(m, (c.S2 + 0x4u), c.T1);
        c.S0 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.S2 + 0x8u), (byte)c.V0);
        MemoryAccess.WriteU8(m, (c.S2 + 0x9u), (byte)c.S0);
        MemoryAccess.WriteU8(m, (c.S2 + 0xAu), (byte)c.S7);
        MemoryAccess.WriteU8(m, (c.S2 + 0xDu), (byte)c.S0);
        MemoryAccess.WriteU8(m, (c.S2 + 0xEu), (byte)0u);
        c.A1 = MemoryAccess.ReadU16(m, c.S3);
        c.A0 = c.S1 + 0u;
        c.RA = 0x80012D44u;
        GranTurismo2PC.func_8007816C(c, m);
        c.A0 = c.S2 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x80012D50u;
        GranTurismo2PC.func_8005E548(c, m);
        c.A1 = MemoryAccess.ReadU16(m, (c.S3 + 0x2u));
        c.A0 = c.S1 + 0u;
        c.RA = 0x80012D5Cu;
        GranTurismo2PC.func_8007816C(c, m);
        c.A0 = c.S2 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x80012D68u;
        GranTurismo2PC.func_8005E5F0(c, m);
        c.A1 = MemoryAccess.ReadU16(m, (c.S3 + 0x94u));
        c.A0 = c.S1 + 0u;
        c.RA = 0x80012D74u;
        GranTurismo2PC.func_8007816C(c, m);
        c.A0 = c.S2 + 0x44u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x80012D80u;
        GranTurismo2PC.func_8008CEDC(c, m);
        c.V0 = 0x801D0000u;
        c.A2 = c.V0 - 0x6760u;
        c.A1 = c.S3 + 0x44u;
        c.A0 = c.A1 | c.A2;
        c.A0 = c.A0 & 0x0003u;
        c.V1 = MemoryAccess.ReadU32(m, (c.S2 + 0x588u));
        c.V0 = 0x00000005u;
        MemoryAccess.WriteU8(m, (c.S2 + 0xBu), (byte)c.V0);
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU16(m, (c.S2 + 0x57Cu), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.S2 + 0x582u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.S2 + 0x584u), (ushort)c.V0);
        c.V0 = 0xFFFFFFF9u;
        MemoryAccess.WriteU8(m, (c.S2 + 0xCu), (byte)0u);
        MemoryAccess.WriteU8(m, (c.S2 + 0xFu), (byte)c.S0);
        MemoryAccess.WriteU8(m, (c.S2 + 0x5Au), (byte)c.S0);
        MemoryAccess.WriteU16(m, (c.S2 + 0x586u), (ushort)0u);
        MemoryAccess.WriteU8(m, (c.S2 + 0x580u), (byte)0u);
        c.V1 = c.V1 | c.S0;
        c.V1 = c.V1 & c.V0;
        c.S0 = 0u + 0u;
        if (c.A0 == 0u) {
            MemoryAccess.WriteU32(m, (c.S2 + 0x588u), c.V1);
            goto L80012E34;
        }
        MemoryAccess.WriteU32(m, (c.S2 + 0x588u), c.V1);
        c.V0 = c.S3 + 0x84u;
        L80012DE0: ;
        c.T0 = MemoryAccess.ReadWordLeft(m, c.T0, (c.A1 + 0x3u));
        c.T0 = MemoryAccess.ReadWordRight(m, c.T0, c.A1);
        c.T1 = MemoryAccess.ReadWordLeft(m, c.T1, (c.A1 + 0x7u));
        c.T1 = MemoryAccess.ReadWordRight(m, c.T1, (c.A1 + 0x4u));
        c.T2 = MemoryAccess.ReadWordLeft(m, c.T2, (c.A1 + 0xBu));
        c.T2 = MemoryAccess.ReadWordRight(m, c.T2, (c.A1 + 0x8u));
        c.T3 = MemoryAccess.ReadWordLeft(m, c.T3, (c.A1 + 0xFu));
        c.T3 = MemoryAccess.ReadWordRight(m, c.T3, (c.A1 + 0xCu));
        MemoryAccess.WriteWordLeft(m, (c.A2 + 0x3u), c.T0);
        MemoryAccess.WriteWordRight(m, c.A2, c.T0);
        MemoryAccess.WriteWordLeft(m, (c.A2 + 0x7u), c.T1);
        MemoryAccess.WriteWordRight(m, (c.A2 + 0x4u), c.T1);
        MemoryAccess.WriteWordLeft(m, (c.A2 + 0xBu), c.T2);
        MemoryAccess.WriteWordRight(m, (c.A2 + 0x8u), c.T2);
        MemoryAccess.WriteWordLeft(m, (c.A2 + 0xFu), c.T3);
        MemoryAccess.WriteWordRight(m, (c.A2 + 0xCu), c.T3);
        c.A1 = c.A1 + 0x10u;
        if (c.A1 != c.V0) {
            c.A2 = c.A2 + 0x10u;
            goto L80012DE0;
        }
        c.A2 = c.A2 + 0x10u;
        goto L80012E64;
        L80012E34: ;
        c.V0 = c.S3 + 0x84u;
        L80012E38: ;
        c.T0 = MemoryAccess.ReadU32(m, c.A1);
        c.T1 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.T2 = MemoryAccess.ReadU32(m, (c.A1 + 0x8u));
        c.T3 = MemoryAccess.ReadU32(m, (c.A1 + 0xCu));
        MemoryAccess.WriteU32(m, c.A2, c.T0);
        MemoryAccess.WriteU32(m, (c.A2 + 0x4u), c.T1);
        MemoryAccess.WriteU32(m, (c.A2 + 0x8u), c.T2);
        MemoryAccess.WriteU32(m, (c.A2 + 0xCu), c.T3);
        c.A1 = c.A1 + 0x10u;
        if (c.A1 != c.V0) {
            c.A2 = c.A2 + 0x10u;
            goto L80012E38;
        }
        c.A2 = c.A2 + 0x10u;
        L80012E64: ;
        c.A0 = c.FP + 0u;
        c.RA = 0x80012E6Cu;
        GranTurismo2PC.func_80019538(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.S0 + 0u;
        c.A2 = c.V0 + 0u;
        c.RA = 0x80012E7Cu;
        GranTurismo2PC.func_80018004(c, m);
        c.A0 = c.S2 + 0u;
        c.A1 = 0u + 0u;
        c.A2 = 0xFFFFFFFFu;
        c.A3 = 0x00000003u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S4);
        c.RA = 0x80012EA0u;
        GranTurismo2PC.func_80011000(c, m);
        c.V0 = 0x801D0000u;
        c.A1 = c.V0 - 0x6720u;
        c.V0 = 0u | 0xC6C0u;
        c.V0 = c.A1 + c.V0;
        c.V1 = 0xFFFFFFFCu;
        c.A0 = c.V0 & c.V1;
        c.V0 = 0x00000008u;
        if (c.S7 == c.V0) {
            c.V0 = (int)c.S7 < 9 ? 1u : 0u;
            goto L80012EF8;
        }
        c.V0 = (int)c.S7 < 9 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000007u;
            goto L80012EDC;
        }
        c.V0 = 0x00000007u;
        if (c.S7 == c.V0) {
            goto L80012EF0;
        }
        goto L80012F04;
        L80012EDC: ;
        c.V0 = 0x00000009u;
        if (c.S7 == c.V0) {
            goto L80012F00;
        }
        goto L80012F04;
        L80012EF0: ;
        c.S6 = c.A1 + 0x3A88u;
        goto L80012F04;
        L80012EF8: ;
        c.S6 = c.A1 + 0x3B2Cu;
        goto L80012F04;
        L80012F00: ;
        c.S6 = c.A1 + 0x3BD0u;
        L80012F04: ;
        c.A2 = MemoryAccess.ReadU32(m, (c.S5 + 0x8Cu));
        c.A1 = c.S6 + 0u;
        c.RA = 0x80012F10u;
        GranTurismo2PC.func_8001F124(c, m);
        c.V0 = 0x00010000u;
        c.V0 = c.V0 | 0x0BD8u;
        c.S0 = 0x801D0000u;
        c.S0 = c.S0 - 0x6720u;
        c.S0 = c.S0 + c.V0;
        c.V0 = 0xFFFFFFFCu;
        c.S0 = c.S0 & c.V0;
        c.A0 = c.S0 + 0u;
        c.RA = 0x80012F34u;
        GranTurismo2PC.func_80011160(c, m);
        c.A1 = MemoryAccess.ReadU32(m, c.S5);
        c.A0 = c.S0 + 0u;
        c.RA = 0x80012F40u;
        GranTurismo2PC.func_80011184(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S5 + 0x8u;
        c.RA = 0x80012F4Cu;
        GranTurismo2PC.func_800129B8(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x44u));
        c.FP = MemoryAccess.ReadU32(m, (c.SP + 0x40u));
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
    public static void func_80012F7C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x38u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S2);
        c.S2 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.S5);
        c.S5 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S4);
        c.S4 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S3);
        c.S3 = c.A3 + 0u;
        c.A1 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, c.S3);
        c.A2 = 0x000000D0u;
        c.RA = 0x80012FBCu;
        GranTurismo2PC.func_8008CE30(c, m);
        L80012FBC: ;
        c.A0 = c.S4 + 0u;
        c.RA = 0x80012FC4u;
        GranTurismo2PC.func_80083AE0(c, m);
        if (c.S0 != 0u) { c.LO = c.V0 / c.S0; c.HI = c.V0 % c.S0; }
        c.V1 = c.HI;
        if (c.S0 != 0u) { c.LO = c.V1 / c.S0; c.HI = c.V1 % c.S0; }
        c.V0 = c.HI;
        c.V0 = c.V0 << 1;
        c.V0 = c.S3 + c.V0;
        c.S1 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x4u));
        c.A0 = c.S1 + 0x1u;
        c.RA = 0x80012FF0u;
        GranTurismo2PC.func_800768C0(c, m);
        c.A0 = c.V0 + 0u;
        c.A1 = c.S2 + 0x8u;
        c.RA = 0x80012FFCu;
        GranTurismo2PC.func_80076F5C(c, m);
        c.A0 = 0x80090000u;
        c.V0 = 0x80090000u;
        c.V1 = 0x00000002u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x2878u));
        c.A0 = MemoryAccess.ReadU32(m, (c.A0 + 0x2E70u));
        if (c.V0 != c.V1) {
            c.V0 = 0x80090000u;
            goto L80013024;
        }
        c.V0 = 0x80090000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x2870u));
        c.A0 = c.V0 + 0x10u;
        goto L80013028;
        L80013024: ;
        c.A0 = c.A0 + 0x100u;
        L80013028: ;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80013030u;
        GranTurismo2PC.func_80078038(c, m);
        c.S1 = MemoryAccess.ReadU32(m, c.V0);
        c.A0 = c.S1 + 0u;
        c.RA = 0x8001303Cu;
        GranTurismo2PC.func_80060B70(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 != 0u) {
            goto L80012FBC;
        }
        c.A1 = MemoryAccess.ReadU16(m, (c.S2 + 0x24u));
        if ((int)c.A1 <= 0) {
            c.A0 = c.S1 + 0u;
            goto L8001306C;
        }
        c.A0 = c.S1 + 0u;
        c.A0 = 0x00000005u;
        c.RA = 0x80013060u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        c.A0 = c.S1 + 0u;
        L8001306C: ;
        c.A1 = c.SP + 0x10u;
        c.A2 = c.SP + 0x14u;
        c.RA = 0x80013078u;
        GranTurismo2PC.func_80060BEC(c, m);
        c.A0 = c.S4 + 0u;
        c.S0 = c.V0 + 0u;
        c.RA = 0x80013084u;
        GranTurismo2PC.func_80083AE0(c, m);
        if (c.S0 != 0u) { c.LO = c.V0 / c.S0; c.HI = c.V0 % c.S0; }
        c.V1 = c.HI;
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.A0 = c.S1 + 0u;
        c.V0 = c.V0 + c.V1;
        c.V0 = MemoryAccess.ReadU8(m, c.V0);
        c.V1 = 0x00000001u;
        MemoryAccess.WriteU32(m, c.S2, c.A0);
        MemoryAccess.WriteU8(m, (c.S2 + 0x8Cu), (byte)c.V1);
        MemoryAccess.WriteU8(m, (c.S2 + 0x8Du), (byte)c.S5);
        MemoryAccess.WriteU8(m, (c.S2 + 0x8Eu), (byte)c.V1);
        MemoryAccess.WriteU8(m, (c.S2 + 0x8Fu), (byte)0u);
        c.V0 = c.V0 << 24;
        c.V0 = (uint)((int)c.V0 >> 24);
        MemoryAccess.WriteU32(m, (c.S2 + 0x4u), c.V0);
        c.RA = 0x800130C4u;
        GranTurismo2PC.func_80060AE8(c, m);
        c.A0 = c.S2 + 0x90u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x800130D0u;
        GranTurismo2PC.func_8008CEDC(c, m);
        c.V1 = c.S2 + 0x8u;
        c.V0 = MemoryAccess.ReadU8(m, (c.V1 + 0x7Au));
        c.V0 = c.V0 | 0x0040u;
        MemoryAccess.WriteU8(m, (c.V1 + 0x7Au), (byte)c.V0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x30u));
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
    public static void func_80013108(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x50u;
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.S7);
        c.S7 = c.A0 + 0u;
        c.V0 = 0x80090000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V0 + 0x2E6Cu));
        MemoryAccess.WriteU32(m, (c.SP + 0x4Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x48u), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x3Cu), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x38u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x34u), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x54u), c.A1);
        MemoryAccess.WriteU32(m, (c.SP + 0x58u), c.A2);
        MemoryAccess.WriteU32(m, (c.SP + 0x5Cu), c.A3);
        c.RA = 0x80013150u;
        GranTurismo2PC.func_80019538(c, m);
        c.A0 = 0u + 0u;
        c.T0 = 0x801D0000u;
        c.T0 = c.T0 - 0x2AACu;
        c.S5 = (uint)(short)MemoryAccess.ReadU16(m, (c.T0 + 0x4018u));
        c.FP = c.V0 + 0u;
        c.RA = 0x80013168u;
        GranTurismo2PC.func_8007D23C(c, m);
        c.A0 = c.S7 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.V0);
        c.RA = 0x80013174u;
        GranTurismo2PC.func_80021A6C(c, m);
        c.A0 = 0u + 0u;
        c.A1 = 0x00000001u;
        c.S4 = c.V0 + 0u;
        c.A2 = c.S4 + 0u;
        c.A3 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.RA = 0x80013198u;
        GranTurismo2PC.func_80010A30(c, m);
        c.S3 = c.V0 + 0u;
        c.A1 = MemoryAccess.ReadU16(m, (c.S4 + 0x2u));
        c.A0 = c.S0 + 0u;
        c.RA = 0x800131A8u;
        GranTurismo2PC.func_8007816C(c, m);
        c.A0 = c.V0 + 0u;
        c.A1 = 0x80020000u;
        c.A1 = c.A1 + 0x29ACu;
        c.S0 = c.SP + 0x20u;
        c.RA = 0x800131BCu;
        GranTurismo2PC.func_8008CF00(c, m);
        if (c.V0 != 0u) {
            goto L800131D8;
        }
        c.A0 = c.S0 + 0u;
        c.RA = 0x800131CCu;
        GranTurismo2PC.func_8001907C(c, m);
        c.A0 = c.S3 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x800131D8u;
        GranTurismo2PC.func_8005E5F0(c, m);
        L800131D8: ;
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x5Cu));
        if (c.T1 == 0u) {
            goto L80013318;
        }
        c.T2 = MemoryAccess.ReadU32(m, (c.SP + 0x58u));
        c.V0 = c.T2 & 0x0001u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000006u;
            goto L80013318;
        }
        c.V0 = 0x00000006u;
        MemoryAccess.WriteU8(m, (c.S3 + 0x5Au), (byte)c.V0);
        c.S0 = 0u + 0u;
        c.S2 = 0x00000005u;
        c.S1 = 0x0000005Cu;
        L8001320C: ;
        c.V0 = (int)c.S0 < 6 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A0 = c.S3 + c.S1;
            goto L80013234;
        }
        c.A0 = c.S3 + c.S1;
        c.A1 = c.S2 - c.S0;
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0x5Cu));
        c.A2 = c.SP + 0x20u;
        c.RA = 0x80013228u;
        GranTurismo2PC.func_80012F7C(c, m);
        c.S1 = c.S1 + 0xD0u;
        c.S0 = c.S0 + 0x1u;
        goto L8001320C;
        L80013234: ;
        c.V0 = 0x801D0000u;
        c.A0 = c.V0 - 0x6760u;
        c.V1 = c.S4 + 0x44u;
        c.V0 = c.V1 | c.A0;
        c.V0 = c.V0 & 0x0003u;
        if (c.V0 == 0u) {
            c.V0 = c.S4 + 0x84u;
            goto L800132A4;
        }
        c.V0 = c.S4 + 0x84u;
        L80013250: ;
        c.T3 = MemoryAccess.ReadWordLeft(m, c.T3, (c.V1 + 0x3u));
        c.T3 = MemoryAccess.ReadWordRight(m, c.T3, c.V1);
        c.T0 = MemoryAccess.ReadWordLeft(m, c.T0, (c.V1 + 0x7u));
        c.T0 = MemoryAccess.ReadWordRight(m, c.T0, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadWordLeft(m, c.T1, (c.V1 + 0xBu));
        c.T1 = MemoryAccess.ReadWordRight(m, c.T1, (c.V1 + 0x8u));
        c.T2 = MemoryAccess.ReadWordLeft(m, c.T2, (c.V1 + 0xFu));
        c.T2 = MemoryAccess.ReadWordRight(m, c.T2, (c.V1 + 0xCu));
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0x3u), c.T3);
        MemoryAccess.WriteWordRight(m, c.A0, c.T3);
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0x7u), c.T0);
        MemoryAccess.WriteWordRight(m, (c.A0 + 0x4u), c.T0);
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0xBu), c.T1);
        MemoryAccess.WriteWordRight(m, (c.A0 + 0x8u), c.T1);
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0xFu), c.T2);
        MemoryAccess.WriteWordRight(m, (c.A0 + 0xCu), c.T2);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.V0) {
            c.A0 = c.A0 + 0x10u;
            goto L80013250;
        }
        c.A0 = c.A0 + 0x10u;
        c.S2 = 0u + 0u;
        goto L800132D4;
        L800132A4: ;
        c.T3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, c.A0, c.T3);
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.T0);
        MemoryAccess.WriteU32(m, (c.A0 + 0x8u), c.T1);
        MemoryAccess.WriteU32(m, (c.A0 + 0xCu), c.T2);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.V0) {
            c.A0 = c.A0 + 0x10u;
            goto L800132A4;
        }
        c.A0 = c.A0 + 0x10u;
        c.S2 = 0u + 0u;
        L800132D4: ;
        c.V0 = 0x801D0000u;
        c.S6 = c.V0 - 0x6720u;
        c.S1 = 0x00010000u;
        c.S1 = c.S1 | 0x4FDAu;
        c.S0 = 0x0000005Cu;
        L800132E8: ;
        c.V0 = MemoryAccess.ReadU8(m, (c.S3 + 0x5Au));
        c.V0 = (int)c.S2 < (int)c.V0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A0 = c.S3 + c.S0;
            goto L80013318;
        }
        c.A0 = c.S3 + c.S0;
        c.A0 = c.A0 + 0x8u;
        c.A1 = c.S1 + c.S6;
        c.RA = 0x80013308u;
        GranTurismo2PC.func_800771AC(c, m);
        c.S1 = c.S1 + 0x1C0u;
        c.S0 = c.S0 + 0xD0u;
        c.S2 = c.S2 + 0x1u;
        goto L800132E8;
        L80013318: ;
        if (c.FP == 0u) {
            c.V0 = 0x00000001u;
            goto L8001342C;
        }
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.S3 + 0x5Au), (byte)c.V0);
        c.A0 = c.S7 + 0u;
        c.RA = 0x8001332Cu;
        GranTurismo2PC.func_80019BF0(c, m);
        c.A0 = c.V0 + 0u;
        c.S0 = 0x800C0000u;
        c.S0 = c.S0 + 0x4050u;
        c.A1 = c.S0 + 0u;
        c.RA = 0x80013340u;
        GranTurismo2PC.func_8005D8D4(c, m);
        c.A0 = c.S0 + 0u;
        c.RA = 0x80013348u;
        GranTurismo2PC.func_80069DDC(c, m);
        c.V0 = 0x801D0000u;
        c.A0 = c.V0 - 0x6760u;
        c.V1 = c.S4 + 0x44u;
        c.V0 = c.V1 | c.A0;
        c.V0 = c.V0 & 0x0003u;
        if (c.V0 == 0u) {
            c.V0 = c.S4 + 0x84u;
            goto L800133B8;
        }
        c.V0 = c.S4 + 0x84u;
        L80013364: ;
        c.T3 = MemoryAccess.ReadWordLeft(m, c.T3, (c.V1 + 0x3u));
        c.T3 = MemoryAccess.ReadWordRight(m, c.T3, c.V1);
        c.T0 = MemoryAccess.ReadWordLeft(m, c.T0, (c.V1 + 0x7u));
        c.T0 = MemoryAccess.ReadWordRight(m, c.T0, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadWordLeft(m, c.T1, (c.V1 + 0xBu));
        c.T1 = MemoryAccess.ReadWordRight(m, c.T1, (c.V1 + 0x8u));
        c.T2 = MemoryAccess.ReadWordLeft(m, c.T2, (c.V1 + 0xFu));
        c.T2 = MemoryAccess.ReadWordRight(m, c.T2, (c.V1 + 0xCu));
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0x3u), c.T3);
        MemoryAccess.WriteWordRight(m, c.A0, c.T3);
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0x7u), c.T0);
        MemoryAccess.WriteWordRight(m, (c.A0 + 0x4u), c.T0);
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0xBu), c.T1);
        MemoryAccess.WriteWordRight(m, (c.A0 + 0x8u), c.T1);
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0xFu), c.T2);
        MemoryAccess.WriteWordRight(m, (c.A0 + 0xCu), c.T2);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.V0) {
            c.A0 = c.A0 + 0x10u;
            goto L80013364;
        }
        c.A0 = c.A0 + 0x10u;
        c.S2 = 0u + 0u;
        goto L800133E8;
        L800133B8: ;
        c.T3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, c.A0, c.T3);
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.T0);
        MemoryAccess.WriteU32(m, (c.A0 + 0x8u), c.T1);
        MemoryAccess.WriteU32(m, (c.A0 + 0xCu), c.T2);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.V0) {
            c.A0 = c.A0 + 0x10u;
            goto L800133B8;
        }
        c.A0 = c.A0 + 0x10u;
        c.S2 = 0u + 0u;
        L800133E8: ;
        c.V0 = 0x801D0000u;
        c.S4 = c.V0 - 0x6720u;
        c.S1 = 0x00010000u;
        c.S1 = c.S1 | 0x4FDAu;
        c.S0 = 0x0000005Cu;
        L800133FC: ;
        c.V0 = MemoryAccess.ReadU8(m, (c.S3 + 0x5Au));
        c.V0 = (int)c.S2 < (int)c.V0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A0 = c.S3 + c.S0;
            goto L8001342C;
        }
        c.A0 = c.S3 + c.S0;
        c.A0 = c.A0 + 0x8u;
        c.A1 = c.S1 + c.S4;
        c.RA = 0x8001341Cu;
        GranTurismo2PC.func_800771AC(c, m);
        c.S1 = c.S1 + 0x1C0u;
        c.S0 = c.S0 + 0xD0u;
        c.S2 = c.S2 + 0x1u;
        goto L800133FC;
        L8001342C: ;
        c.T3 = MemoryAccess.ReadU32(m, (c.SP + 0x54u));
        if (c.T3 != 0u) {
            c.V0 = 0x00000001u;
            goto L80013498;
        }
        c.V0 = 0x00000001u;
        c.A0 = c.S5 + 0u;
        c.A1 = 0u + 0u;
        c.A2 = c.FP + 0u;
        c.RA = 0x8001344Cu;
        GranTurismo2PC.func_80018004(c, m);
        c.A0 = c.S3 + 0u;
        c.A1 = 0u + 0u;
        c.A2 = 0xFFFFFFFFu;
        c.A3 = 0x00000003u;
        c.V0 = c.S5 << 2;
        c.V0 = c.V0 + c.S5;
        c.V0 = c.V0 << (int)(c.A3 & 31u);
        c.V0 = c.V0 + c.S5;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + 0x4u;
        c.T0 = 0x801D0000u;
        c.T0 = c.T0 - 0x2AACu;
        c.V0 = c.T0 + c.V0;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S5);
        c.RA = 0x80013494u;
        GranTurismo2PC.func_80011000(c, m);
        c.V0 = 0x00000001u;
        L80013498: ;
        MemoryAccess.WriteU8(m, (c.S3 + 0x9u), (byte)c.V0);
        if (c.FP == 0u) {
            MemoryAccess.WriteU8(m, (c.S3 + 0xAu), (byte)c.V0);
            goto L800134AC;
        }
        MemoryAccess.WriteU8(m, (c.S3 + 0xAu), (byte)c.V0);
        c.V0 = 0x0000000Au;
        MemoryAccess.WriteU8(m, (c.S3 + 0xAu), (byte)c.V0);
        L800134AC: ;
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x54u));
        if (c.T1 == 0u) {
            c.V0 = 0x0000000Cu;
            goto L800134C0;
        }
        c.V0 = 0x0000000Cu;
        MemoryAccess.WriteU8(m, (c.S3 + 0xAu), (byte)c.V0);
        L800134C0: ;
        c.A0 = c.S7 + 0u;
        c.RA = 0x800134C8u;
        GranTurismo2PC.func_800188B0(c, m);
        c.A0 = c.S7 + 0u;
        MemoryAccess.WriteU16(m, (c.S3 + 0x57Cu), (ushort)c.V0);
        c.RA = 0x800134D4u;
        GranTurismo2PC.func_800194FC(c, m);
        c.A0 = c.S7 + 0u;
        MemoryAccess.WriteU16(m, (c.S3 + 0x586u), (ushort)c.V0);
        c.RA = 0x800134E0u;
        GranTurismo2PC.func_80019578(c, m);
        c.A0 = c.S7 + 0u;
        c.A1 = 0xFFFFFFFEu;
        c.V1 = MemoryAccess.ReadU32(m, (c.S3 + 0x588u));
        c.V0 = 0u < c.V0 ? 1u : 0u;
        c.V1 = c.V1 & c.A1;
        c.V1 = c.V1 | c.V0;
        MemoryAccess.WriteU32(m, (c.S3 + 0x588u), c.V1);
        c.RA = 0x80013500u;
        GranTurismo2PC.func_800195BC(c, m);
        c.A0 = c.S7 + 0u;
        c.A1 = 0xFFFFFFF9u;
        c.V0 = c.V0 & 0x0003u;
        c.V1 = MemoryAccess.ReadU32(m, (c.S3 + 0x588u));
        c.V0 = c.V0 << 1;
        c.V1 = c.V1 & c.A1;
        c.V1 = c.V1 | c.V0;
        MemoryAccess.WriteU32(m, (c.S3 + 0x588u), c.V1);
        c.RA = 0x80013524u;
        GranTurismo2PC.func_8001859C(c, m);
        MemoryAccess.WriteU8(m, (c.S3 + 0x580u), (byte)c.V0);
        c.T2 = MemoryAccess.ReadU32(m, (c.SP + 0x5Cu));
        if (c.T2 == 0u) {
            c.V1 = 0x00010000u;
            goto L80013580;
        }
        c.V1 = 0x00010000u;
        c.T3 = MemoryAccess.ReadU32(m, (c.SP + 0x58u));
        c.V0 = c.T3 & 0x0001u;
        if (c.V0 == 0u) {
            c.V0 = c.S5 << 2;
            goto L80013580;
        }
        c.V0 = c.S5 << 2;
        c.V0 = c.V0 + c.S5;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.S5;
        c.V0 = c.V0 << 2;
        c.T0 = 0x801D0000u;
        c.T0 = c.T0 - 0x2AACu;
        c.V0 = c.V0 + c.T0;
        c.V0 = MemoryAccess.ReadU16(m, (c.V0 + 0x9Cu));
        c.V0 = c.V0 & 0x3FFFu;
        c.V0 = c.V0 + 0x5u;
        MemoryAccess.WriteU16(m, (c.S3 + 0x586u), (ushort)c.V0);
        c.V1 = 0x00010000u;
        L80013580: ;
        c.V1 = c.V1 | 0x0BD8u;
        c.V0 = 0x801D0000u;
        c.A1 = c.V0 - 0x6720u;
        c.V1 = c.A1 + c.V1;
        c.A0 = 0xFFFFFFFCu;
        if (c.FP == 0u) {
            c.S1 = c.V1 & c.A0;
            goto L800135A8;
        }
        c.S1 = c.V1 & c.A0;
        c.V0 = 0u | 0xC6C0u;
        c.V0 = c.A1 + c.V0;
        c.S1 = c.V0 & c.A0;
        L800135A8: ;
        c.A0 = c.S1 + 0u;
        c.RA = 0x800135B0u;
        GranTurismo2PC.func_80011160(c, m);
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x54u));
        if (c.T1 != 0u) {
            c.S0 = c.S5 << 2;
            goto L800135F8;
        }
        c.S0 = c.S5 << 2;
        c.S0 = c.S0 + c.S5;
        c.S0 = c.S0 << 3;
        c.S0 = c.S0 + c.S5;
        c.S0 = c.S0 << 2;
        c.S0 = c.S0 + 0x4u;
        c.T2 = 0x801D0000u;
        c.T2 = c.T2 - 0x2AACu;
        c.S0 = c.T2 + c.S0;
        c.A1 = MemoryAccess.ReadU32(m, c.S0);
        c.A0 = c.S1 + 0u;
        c.RA = 0x800135ECu;
        GranTurismo2PC.func_80011184(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S0 + 0x8u;
        c.RA = 0x800135F8u;
        GranTurismo2PC.func_800129B8(c, m);
        L800135F8: ;
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
    public static void func_80013628(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x888u;
        MemoryAccess.WriteU32(m, (c.SP + 0x884u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x880u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x87Cu), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x878u), c.S0);
        c.RA = 0x80013640u;
        GranTurismo2PC.func_80020C50(c, m);
        c.S0 = 0x800E0000u;
        c.RA = 0x80013648u;
        GranTurismo2PC.func_800187DC(c, m);
        c.A0 = 0x800D0000u;
        c.A0 = c.A0 + 0x850u;
        c.A1 = 0u | 0xC800u;
        c.RA = 0x80013658u;
        GranTurismo2PC.func_800609F8(c, m);
        c.A0 = 0x00000001u;
        c.A1 = 0x800C0000u;
        c.A1 = c.A1 + 0x4050u;
        c.A2 = 0u | 0xC800u;
        c.RA = 0x8001366Cu;
        GranTurismo2PC.func_800222E4(c, m);
        c.V0 = 0x801D0000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 - 0x6628u));
        c.V0 = 0xCCCC0000u;
        c.V0 = c.V0 | 0xCCCDu;
        { var _r = (ulong)c.V1 * c.V0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.T0 = c.HI;
        c.V0 = 0x88880000u;
        c.V0 = c.V0 | 0x8889u;
        c.V1 = c.T0 >> 3;
        { var _r = (ulong)c.V1 * c.V0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.S0 = c.S0 - 0x2FB0u;
        c.A1 = c.S0 + 0u;
        c.T0 = c.HI;
        c.V0 = c.T0 >> 5;
        c.A0 = c.V0 << 4;
        c.A0 = c.A0 - c.V0;
        c.A0 = c.A0 << 2;
        c.A0 = c.V1 - c.A0;
        c.RA = 0x800136B8u;
        GranTurismo2PC.func_800224E0(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x000C0000u;
        c.A1 = c.A1 | 0x8000u;
        c.RA = 0x800136C8u;
        GranTurismo2PC.func_80076D74(c, m);
        c.A0 = 0x80020000u;
        c.A0 = c.A0 + 0x4430u;
        c.A1 = 0x00020000u;
        c.A1 = c.A1 | 0xC4C0u;
        c.V1 = 0x80090000u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x2E6Cu), c.V0);
        c.RA = 0x800136E4u;
        GranTurismo2PC.func_80076CF8(c, m);
        c.V1 = 0x80090000u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x2E70u), c.V0);
        c.RA = 0x800136F0u;
        GranTurismo2PC.func_80019474(c, m);
        c.A0 = c.SP + 0x10u;
        c.RA = 0x800136F8u;
        GranTurismo2PC.func_80013BD4(c, m);
        L800136F8: ;
        c.A0 = c.SP + 0x10u;
        c.RA = 0x80013700u;
        GranTurismo2PC.func_800833E8(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L800136F8;
        }
        if ((int)c.V1 < 0) {
            c.A0 = c.SP + 0x10u;
            goto L800136F8;
        }
        c.A0 = c.SP + 0x10u;
        c.A1 = 0x00000002u;
        c.RA = 0x80013720u;
        GranTurismo2PC.func_80013CD0(c, m);
        c.A0 = c.SP + 0x10u;
        c.RA = 0x80013728u;
        GranTurismo2PC.func_80014E60(c, m);
        L80013728: ;
        c.A0 = c.SP + 0x10u;
        c.RA = 0x80013730u;
        GranTurismo2PC.func_800833E8(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L80013728;
        }
        if ((int)c.V1 < 0) {
            goto L80013728;
        }
        c.A0 = 0x00000001u;
        c.RA = 0x80013750u;
        GranTurismo2PC.func_8007F830(c, m);
        c.A0 = c.SP + 0x10u;
        c.A1 = 0x00000002u;
        c.RA = 0x8001375Cu;
        GranTurismo2PC.func_80014E9C(c, m);
        c.V0 = 0x801F0000u;
        c.S1 = c.V0 - 0xA10u;
        c.S0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0x5u));
        c.V0 = 0x00000003u;
        if (c.S0 == c.V0) {
            c.V0 = (int)c.S0 < 4 ? 1u : 0u;
            goto L800137B0;
        }
        c.V0 = (int)c.S0 < 4 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L8001378C;
        }
        c.V0 = 0x00000002u;
        if (c.S0 == c.V0) {
            c.A0 = 0x00000001u;
            goto L8001380C;
        }
        c.A0 = 0x00000001u;
        c.V0 = 0x801F0000u;
        goto L800139F8;
        L8001378C: ;
        c.V0 = 0x00000004u;
        if (c.S0 != c.V0) {
            c.A0 = 0x00000001u;
            goto L800139F4;
        }
        c.A0 = 0x00000001u;
        c.A1 = 0x80010000u;
        c.A1 = c.A1 + 0x172Cu;
        c.A2 = 0u + 0u;
        c.RA = 0x800137A8u;
        GranTurismo2PC.func_8005DA7C(c, m);
        goto L80013A08;
        L800137B0: ;
        c.A0 = 0x80020000u;
        c.A0 = c.A0 + 0x4430u;
        c.A1 = 0x00020000u;
        c.A1 = c.A1 | 0xC4C0u;
        c.RA = 0x800137C4u;
        GranTurismo2PC.func_80076CF8(c, m);
        c.A0 = 0x800C0000u;
        c.A0 = c.A0 + 0x1C10u;
        c.A1 = 0x00000001u;
        c.A2 = 0u + 0u;
        c.A3 = c.A2 + 0u;
        c.V1 = 0x80090000u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x2E70u), c.V0);
        c.RA = 0x800137E4u;
        GranTurismo2PC.func_80013108(c, m);
        c.A0 = 0u + 0u;
        c.A1 = 0x80010000u;
        c.A1 = c.A1 + 0x1F64u;
        c.A2 = c.A0 + 0u;
        c.V0 = 0x00000004u;
        MemoryAccess.WriteU8(m, (c.S1 + 0x1u), (byte)c.V0);
        MemoryAccess.WriteU8(m, (c.S1 + 0x2u), (byte)c.S0);
        c.RA = 0x80013804u;
        GranTurismo2PC.func_8005DA7C(c, m);
        goto L80013A08;
        L8001380C: ;
        c.V0 = 0x800C0000u;
        c.S2 = c.V0 + 0x1C10u;
        c.A0 = c.S2 + 0u;
        c.RA = 0x8001381Cu;
        GranTurismo2PC.func_80018608(c, m);
        if (c.V0 == 0u) {
            c.S1 = 0x80020000u;
            goto L80013894;
        }
        c.S1 = 0x80020000u;
        c.S1 = c.S1 + 0x4430u;
        c.A0 = c.S1 + 0u;
        c.A1 = 0x00020000u;
        c.S0 = 0x801D0000u;
        c.S0 = c.S0 - 0x6720u;
        c.V1 = c.S0 + 0xB8u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x40u));
        c.A1 = c.A1 | 0xC4C0u;
        c.V0 = c.V0 + 0x1u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x40u), c.V0);
        c.V0 = 0u | 0xBCCCu;
        c.S0 = c.S0 + c.V0;
        c.RA = 0x80013858u;
        GranTurismo2PC.func_80076CF8(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S2 + 0u;
        c.V1 = 0x80090000u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x2E70u), c.V0);
        c.RA = 0x8001386Cu;
        GranTurismo2PC.func_80018C8C(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = 0x00020000u;
        c.A1 = c.A1 | 0xC4C0u;
        c.RA = 0x8001387Cu;
        GranTurismo2PC.func_80076E88(c, m);
        c.A0 = c.S2 + 0u;
        c.V1 = 0x80090000u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x2E6Cu), c.V0);
        c.RA = 0x8001388Cu;
        GranTurismo2PC.func_8001050C(c, m);
        c.A0 = 0u + 0u;
        goto L800139C4;
        L80013894: ;
        c.A0 = c.S2 + 0u;
        c.RA = 0x8001389Cu;
        GranTurismo2PC.func_8001861C(c, m);
        c.S0 = c.V0 + 0u;
        if ((int)c.S0 < 0) {
            c.A1 = 0x00020000u;
            goto L800138D4;
        }
        c.A1 = 0x00020000u;
        c.A0 = 0x80020000u;
        c.A0 = c.A0 + 0x4430u;
        c.A1 = c.A1 | 0xC4C0u;
        c.RA = 0x800138B8u;
        GranTurismo2PC.func_80076CF8(c, m);
        c.V1 = 0x80090000u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x2E70u), c.V0);
        c.A0 = c.S2 + 0u;
        c.A1 = c.S0 + 0u;
        c.RA = 0x800138CCu;
        GranTurismo2PC.func_80012C6C(c, m);
        c.A0 = 0u + 0u;
        goto L800139C4;
        L800138D4: ;
        c.A0 = 0x80020000u;
        c.A0 = c.A0 + 0x4430u;
        c.S0 = 0x801D0000u;
        c.S0 = c.S0 - 0x6720u;
        c.V1 = c.S0 + 0xB8u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x40u));
        c.A1 = c.A1 | 0xC4C0u;
        c.V0 = c.V0 + 0x1u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x40u), c.V0);
        c.RA = 0x800138FCu;
        GranTurismo2PC.func_80076CF8(c, m);
        c.A0 = 0u | 0xC514u;
        c.A0 = c.S0 + c.A0;
        c.A1 = c.S2 + 0u;
        c.V1 = 0x80090000u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x2E70u), c.V0);
        c.RA = 0x80013914u;
        GranTurismo2PC.func_80018A84(c, m);
        c.A0 = 0u | 0xBCCCu;
        c.A0 = c.S0 + c.A0;
        c.A1 = c.S2 + 0u;
        c.RA = 0x80013924u;
        GranTurismo2PC.func_80018C8C(c, m);
        c.A0 = c.S2 + 0u;
        c.RA = 0x8001392Cu;
        GranTurismo2PC.func_80018690(c, m);
        if (c.V0 == 0u) {
            c.A1 = 0u + 0u;
            goto L800139AC;
        }
        c.A1 = 0u + 0u;
        c.A0 = c.S2 + 0u;
        c.RA = 0x8001393Cu;
        GranTurismo2PC.func_80018768(c, m);
        c.V1 = 0x80020000u;
        c.V1 = c.V1 + 0x43F0u;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.V1;
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, c.V0);
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x2u));
        c.RA = 0x8001395Cu;
        GranTurismo2PC.func_80015098(c, m);
        c.A0 = c.SP + 0x10u;
        c.RA = 0x80013964u;
        GranTurismo2PC.func_80015108(c, m);
        L80013964: ;
        c.A0 = c.SP + 0x10u;
        c.RA = 0x8001396Cu;
        GranTurismo2PC.func_800833E8(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L80013964;
        }
        if ((int)c.V1 < 0) {
            c.A0 = c.SP + 0x10u;
            goto L80013964;
        }
        c.A0 = c.SP + 0x10u;
        c.A1 = 0x00000002u;
        c.RA = 0x8001398Cu;
        GranTurismo2PC.func_80015144(c, m);
        c.RA = 0x80013994u;
        GranTurismo2PC.func_8001508C(c, m);
        c.A0 = 0x800C0000u;
        c.A0 = c.A0 + 0x1C10u;
        c.A1 = 0u + 0u;
        c.A2 = 0x00000001u;
        c.A3 = c.V0 + 0u;
        goto L800139B8;
        L800139AC: ;
        c.A0 = c.S2 + 0u;
        c.A2 = c.A1 + 0u;
        c.A3 = c.A1 + 0u;
        L800139B8: ;
        c.RA = 0x800139C0u;
        GranTurismo2PC.func_80013108(c, m);
        c.A0 = 0u + 0u;
        L800139C4: ;
        c.A1 = 0x80010000u;
        c.A1 = c.A1 + 0x1F64u;
        c.A2 = c.A0 + 0u;
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0xA10u;
        c.V1 = 0x00000004u;
        MemoryAccess.WriteU8(m, (c.V0 + 0x1u), (byte)c.V1);
        c.V1 = 0x00000003u;
        MemoryAccess.WriteU8(m, (c.V0 + 0x2u), (byte)c.V1);
        c.RA = 0x800139ECu;
        GranTurismo2PC.func_8005DA7C(c, m);
        goto L80013A08;
        L800139F4: ;
        c.V0 = 0x801F0000u;
        L800139F8: ;
        c.V0 = c.V0 - 0xA10u;
        MemoryAccess.WriteU8(m, (c.V0 + 0x1u), (byte)0u);
        MemoryAccess.WriteU8(m, (c.V0 + 0x2u), (byte)0u);
        c.RA = 0x80013A08u;
        GranTurismo2PC.func_8005DA3C(c, m);
        L80013A08: ;
        c.A0 = 0x00000001u;
        c.RA = 0x80013A10u;
        GranTurismo2PC.func_8005DA3C(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x884u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x880u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x87Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x878u));
        c.SP = c.SP + 0x888u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80013A28(CpuContext c, IMemory m)
    {
        c.A3 = 0u + 0u;
        c.A2 = c.A3 + 0u;
        c.T1 = 0x80050000u;
        c.A1 = MemoryAccess.ReadU16(m, (c.T1 + 0x904u));
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.A0 + 0x1u));
        c.V1 = (uint)(sbyte)MemoryAccess.ReadU8(m, c.A0);
        c.V0 = c.V0 << 8;
        if (c.A1 == 0u) {
            c.T0 = c.V1 | c.V0;
            goto L80013A68;
        }
        c.T0 = c.V1 | c.V0;
        c.V0 = c.T1 + 0x904u;
        L80013A50: ;
        if (c.A1 == c.T0) {
            c.V0 = c.V0 + 0x2u;
            goto L80013A68;
        }
        c.V0 = c.V0 + 0x2u;
        c.A1 = MemoryAccess.ReadU16(m, c.V0);
        if (c.A1 != 0u) {
            c.A2 = c.A2 + 0x1u;
            goto L80013A50;
        }
        c.A2 = c.A2 + 0x1u;
        L80013A68: ;
        c.V1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.A0 + 0x2u));
        c.V1 = c.V1 - 0x30u;
        c.V0 = c.V1 << 1;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.V1;
        c.V1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.A0 + 0x3u));
        c.A1 = c.V0 << 2;
        c.V1 = c.V1 - 0x30u;
        c.V0 = c.V1 << 2;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 1;
        c.A1 = c.A1 + c.V0;
        c.V1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.A0 + 0x4u));
        c.V0 = c.A1 - 0x30u;
        c.A1 = c.V0 + c.V1;
        c.V1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.A0 + 0x6u));
        c.V0 = 0x00000035u;
        if (c.V1 == c.V0) {
            c.T0 = c.A2 + 0u;
            goto L80013AF4;
        }
        c.T0 = c.A2 + 0u;
        c.V0 = (int)c.V1 < 54 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000034u;
            goto L80013AD8;
        }
        c.V0 = 0x00000034u;
        if (c.V1 == c.V0) {
            c.V1 = 0u + 0u;
            goto L80013AEC;
        }
        c.V1 = 0u + 0u;
        c.V0 = c.A3 << 4;
        goto L80013B04;
        L80013AD8: ;
        c.V0 = 0x00000036u;
        if (c.V1 == c.V0) {
            c.V1 = 0u + 0u;
            goto L80013AFC;
        }
        c.V1 = 0u + 0u;
        c.V0 = c.A3 << 4;
        goto L80013B04;
        L80013AEC: ;
        c.V1 = 0x00000001u;
        goto L80013B00;
        L80013AF4: ;
        c.V1 = 0x00000002u;
        goto L80013B00;
        L80013AFC: ;
        c.V1 = 0x00000003u;
        L80013B00: ;
        c.V0 = c.A3 << 4;
        L80013B04: ;
        c.A3 = c.V0 | c.T0;
        c.V0 = c.A3 << 12;
        c.A3 = c.V0 | c.A1;
        c.V0 = c.A3 << 3;
        c.A3 = c.V0 | c.V1;
        c.V1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.A0 + 0x7u));
        c.V0 = c.A3 << 13;
        c.V0 = c.V0 | c.V1;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80013B28_gt2_overlay_4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A1 + 0u;
        c.A0 = c.S0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.A1 = c.V0 + 0u;
        c.RA = 0x80013B48u;
        GranTurismo2PC.func_8005D8A0(c, m);
        c.A0 = c.S0 + 0u;
        c.RA = 0x80013B50u;
        GranTurismo2PC.func_8005D768(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80013B60(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        c.V1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.V0 = c.V0 >> 3;
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u) {
            c.A0 = c.V1 + 0x8u;
            goto L80013B90;
        }
        c.A0 = c.V1 + 0x8u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.A0 = c.A0 + c.V0;
        L80013B90: ;
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
        c.RA = 0x80013BC4u;
        GranTurismo2PC.func_8007BBD4(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80013BD4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.RA = 0x80013BF0u;
        GranTurismo2PC.func_8007FE8C(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x00000026u;
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x2CA4u;
        MemoryAccess.WriteU32(m, c.S0, c.V0);
        c.RA = 0x80013C08u;
        GranTurismo2PC.func_8007FF70(c, m);
        c.A0 = c.S0 + 0x58u;
        c.V0 = 0x80050000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8F0u));
        c.A2 = 0x00010000u;
        c.RA = 0x80013C1Cu;
        GranTurismo2PC.func_80080494(c, m);
        c.S1 = c.S0 + 0x38u;
        c.A0 = c.S1 + 0u;
        c.A1 = 0x800C0000u;
        c.A1 = c.A1 + 0x1D10u;
        c.A2 = 0x00002340u;
        c.RA = 0x80013C34u;
        GranTurismo2PC.func_80080088(c, m);
        c.A0 = c.S1 + 0u;
        c.S2 = 0x00000001u;
        c.V0 = 0x00000010u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x82u), (ushort)c.V0);
        c.V0 = 0x00000003u;
        MemoryAccess.WriteU16(m, (c.S0 + 0xA0u), (ushort)c.V0);
        c.V0 = 0x00000440u;
        MemoryAccess.WriteU16(m, (c.S0 + 0xA2u), (ushort)c.V0);
        c.V0 = 0x00000004u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x80u), (ushort)c.S2);
        MemoryAccess.WriteU16(m, (c.S0 + 0x90u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.S0 + 0x92u), (ushort)c.V0);
        c.RA = 0x80013C68u;
        GranTurismo2PC.func_80080100(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S0 + 0x80u;
        c.RA = 0x80013C74u;
        GranTurismo2PC.func_800800B4(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S0 + 0xA0u;
        c.RA = 0x80013C80u;
        GranTurismo2PC.func_800800B4(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S0 + 0x90u;
        c.RA = 0x80013C8Cu;
        GranTurismo2PC.func_800800B4(c, m);
        c.A0 = c.S1 + 0u;
        c.RA = 0x80013C94u;
        GranTurismo2PC.func_80080300(c, m);
        c.A0 = c.S1 + 0u;
        c.RA = 0x80013C9Cu;
        GranTurismo2PC.func_80080110(c, m);
        c.RA = 0x80013CA4u;
        GranTurismo2PC.func_8007C328(c, m);
        c.V0 = c.S0 + 0u;
        c.V1 = 0x800B0000u;
        MemoryAccess.WriteU8(m, (c.V0 + 0x70u), (byte)0u);
        MemoryAccess.WriteU32(m, (c.V1 - 0x72A4u), c.V0);
        MemoryAccess.WriteU8(m, (c.V0 + 0x21u), (byte)c.S2);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80013CD0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x2CA4u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU32(m, c.A0, c.V0);
        c.RA = 0x80013CE8u;
        GranTurismo2PC.func_8007FEC8(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80013CF8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x58u;
        MemoryAccess.WriteU32(m, (c.SP + 0x48u), c.S4);
        c.S4 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x50u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x4Cu), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x3Cu), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x38u), c.S0);
        c.RA = 0x80013D20u;
        GranTurismo2PC.func_800809B0(c, m);
        c.A1 = 0x0000002Cu;
        c.S0 = 0x80050000u;
        MemoryAccess.WriteU8(m, (c.S4 + 0x74u), (byte)0u);
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x8F0u));
        c.V0 = 0x00000002u;
        MemoryAccess.WriteU16(m, (c.S4 + 0x72u), (ushort)c.V0);
        c.RA = 0x80013D3Cu;
        Dispatcher.Call(c, m, 0x80013B28u);
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x8F0u));
        c.A1 = 0x00000009u;
        c.RA = 0x80013D48u;
        GranTurismo2PC.func_80013B60(c, m);
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x8F0u));
        c.A1 = 0x0000002Du;
        c.RA = 0x80013D54u;
        Dispatcher.Call(c, m, 0x80013B28u);
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x8F0u));
        c.A1 = 0x00000018u;
        c.RA = 0x80013D60u;
        GranTurismo2PC.func_80013B60(c, m);
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x8F0u));
        c.A1 = 0x0000002Eu;
        c.RA = 0x80013D6Cu;
        Dispatcher.Call(c, m, 0x80013B28u);
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x8F0u));
        c.A1 = 0x0000000Au;
        c.RA = 0x80013D78u;
        GranTurismo2PC.func_80013B60(c, m);
        c.S5 = c.S4 + 0x1D0u;
        c.RA = 0x80013D80u;
        GranTurismo2PC.func_8001F210(c, m);
        c.V0 = 0x80050000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x8F8u));
        c.A1 = 0x00006000u;
        c.RA = 0x80013D90u;
        GranTurismo2PC.func_80076A88(c, m);
        c.A0 = c.S5 + 0u;
        c.RA = 0x80013D98u;
        GranTurismo2PC.func_8001E22C(c, m);
        c.A0 = 0x801C0000u;
        c.A0 = c.A0 + 0x3080u;
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.S4 + 0x1F8u), c.V0);
        MemoryAccess.WriteU16(m, (c.S4 + 0x1CEu), (ushort)0u);
        c.RA = 0x80013DB0u;
        GranTurismo2PC.func_8001DA24(c, m);
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 - 0x72A0u;
        c.A1 = 0u + 0u;
        c.RA = 0x80013DC0u;
        GranTurismo2PC.func_80020490(c, m);
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 - 0x7298u;
        c.A1 = 0u + 0u;
        c.RA = 0x80013DD0u;
        GranTurismo2PC.func_800209B0(c, m);
        c.V0 = 0x801D0000u;
        MemoryAccess.WriteU8(m, (c.S4 + 0x71u), (byte)0u);
        MemoryAccess.WriteU8(m, (c.S4 + 0x1CCu), (byte)0u);
        MemoryAccess.WriteU8(m, (c.S4 + 0x1C4u), (byte)0u);
        MemoryAccess.WriteU8(m, (c.S4 + 0x1B0u), (byte)0u);
        MemoryAccess.WriteU16(m, (c.S4 + 0x19Cu), (ushort)0u);
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x156Cu));
        c.V0 = 0x800B0000u;
        if ((int)c.A0 < 0) {
            MemoryAccess.WriteU32(m, (c.V0 - 0x72A4u), c.S4);
            goto L80013E00;
        }
        MemoryAccess.WriteU32(m, (c.V0 - 0x72A4u), c.S4);
        c.A1 = 0u + 0u;
        c.RA = 0x80013E00u;
        GranTurismo2PC.func_80017480(c, m);
        L80013E00: ;
        c.S3 = 0x00000008u;
        c.RA = 0x80013E08u;
        GranTurismo2PC.func_80018F6C(c, m);
        c.A0 = c.S4 + 0x2FCu;
        c.A1 = c.SP + 0x10u;
        c.V0 = 0x801A0000u;
        c.V0 = c.V0 + 0x5050u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.V0);
        c.V0 = 0x800B0000u;
        c.V0 = c.V0 - 0x6FC0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.V0);
        c.V0 = 0x80050000u;
        c.V1 = 0x80050000u;
        c.A3 = 0x80050000u;
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x8FCu));
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 + 0x900u));
        c.V0 = MemoryAccess.ReadU32(m, (c.A3 + 0x8F4u));
        c.A2 = c.SP + 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.V0);
        c.V0 = c.V0 + 0x6000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.V0);
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V1);
        c.V1 = c.V0 + 0x918u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x918u));
        MemoryAccess.WriteU16(m, (c.SP + 0x2Cu), (ushort)c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        c.V1 = MemoryAccess.ReadU16(m, (c.V1 + 0x8u));
        c.V0 = c.V0 >> 16;
        MemoryAccess.WriteU16(m, (c.SP + 0x2Eu), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.SP + 0x30u), (ushort)c.V1);
        c.RA = 0x80013E7Cu;
        GranTurismo2PC.func_8001D118(c, m);
        c.S2 = c.S4 + 0xB0u;
        c.A0 = c.S2 + 0u;
        c.A1 = 0u + 0u;
        c.S0 = 0x800A0000u;
        c.S0 = c.S0 + 0x6F5Cu;
        c.A2 = c.S0 + 0u;
        c.RA = 0x80013E98u;
        GranTurismo2PC.func_8007FAB8(c, m);
        c.S1 = c.S4 + 0x114u;
        c.A0 = c.S1 + 0u;
        c.A1 = 0x00000001u;
        c.A2 = c.S0 + 0u;
        c.RA = 0x80013EACu;
        GranTurismo2PC.func_8007FAB8(c, m);
        c.A0 = c.S2 + 0u;
        c.RA = 0x80013EB4u;
        GranTurismo2PC.func_80083A04(c, m);
        c.A0 = c.S1 + 0u;
        c.RA = 0x80013EBCu;
        GranTurismo2PC.func_80083A04(c, m);
        c.A0 = c.S5 + 0u;
        c.RA = 0x80013EC4u;
        GranTurismo2PC.func_8001E924(c, m);
        MemoryAccess.WriteU16(m, (c.S4 + 0x19Cu), (ushort)c.S3);
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
    public static void func_80013EEC(CpuContext c, IMemory m)
    {
        RecompOne.Runtime.Sdk.GT2Compat.ReturnFromUnifiedGranTurismoRoot(
            c.A0, m);
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        c.S4 = c.S3 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.S0 + 0x178u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.RA = 0x80013F1Cu;
        GranTurismo2PC.func_80080C9C(c, m);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x72u));
        c.V1 = MemoryAccess.ReadU16(m, (c.S0 + 0x72u));
        if ((int)c.V0 <= 0) {
            c.V0 = c.V1 - 0x1u;
            goto L80013F30;
        }
        c.V0 = c.V1 - 0x1u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x72u), (ushort)c.V0);
        L80013F30: ;
        c.A0 = c.S0 + 0xBCu;
        c.V0 = MemoryAccess.ReadU8(m, (c.S0 + 0x70u));
        c.A1 = c.S2 + 0u;
        c.V0 = c.V0 + 0x1u;
        c.V0 = c.V0 & 0x0001u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x70u), (byte)c.V0);
        c.RA = 0x80013F4Cu;
        GranTurismo2PC.func_80083998(c, m);
        c.A0 = c.S0 + 0x120u;
        c.A1 = c.S0 + 0x188u;
        c.RA = 0x80013F58u;
        GranTurismo2PC.func_80083998(c, m);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x74u));
        if (c.V0 == 0u) {
            c.A1 = 0x00000001u;
            goto L80013F80;
        }
        c.A1 = 0x00000001u;
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x7Cu));
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x78u));
        c.A2 = c.A1 + 0u;
        c.RA = 0x80013F7Cu;
        Dispatcher.Call(c, m, c.V0);
        MemoryAccess.WriteU8(m, (c.S0 + 0x74u), (byte)c.V0);
        L80013F80: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x1C4u));
        c.V1 = MemoryAccess.ReadU8(m, (c.S0 + 0x1C4u));
        if ((int)c.V0 <= 0) {
            c.V0 = c.V1 - 0x1u;
            goto L80013F94;
        }
        c.V0 = c.V1 - 0x1u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x1C4u), (byte)c.V0);
        L80013F94: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x1CCu));
        c.V1 = MemoryAccess.ReadU8(m, (c.S0 + 0x1CCu));
        if ((int)c.V0 <= 0) {
            c.V0 = c.V1 - 0x1u;
            goto L80013FA8;
        }
        c.V0 = c.V1 - 0x1u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x1CCu), (byte)c.V0);
        L80013FA8: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x1C4u));
        c.A0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x1B0u));
        c.V1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x1CCu));
        c.V0 = c.V0 < 0x00000001u ? 1u : 0u;
        c.A0 = c.A0 < 0x00000001u ? 1u : 0u;
        c.V0 = c.V0 & c.A0;
        c.V1 = c.V1 < 0x00000001u ? 1u : 0u;
        c.V0 = c.V0 & c.V1;
        if (c.V0 == 0u) {
            c.S1 = c.S0 + 0x2FCu;
            goto L800141A4;
        }
        c.S1 = c.S0 + 0x2FCu;
        c.A0 = c.S1 + 0u;
        c.RA = 0x80013FD8u;
        GranTurismo2PC.func_8001D258(c, m);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x1CEu));
        if (c.V1 == c.S3) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L800140C0;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L80014000;
        }
        c.V0 = 0x00000002u;
        if (c.V1 == 0u) {
            goto L80014018;
        }
        goto L80014190;
        L80014000: ;
        if (c.V1 == c.V0) {
            c.V0 = 0x00000003u;
            goto L80014118;
        }
        c.V0 = 0x00000003u;
        if (c.V1 == c.V0) {
            c.A0 = c.S0 + 0x1D0u;
            goto L80014068;
        }
        c.A0 = c.S0 + 0x1D0u;
        goto L80014190;
        L80014018: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x19Cu));
        if ((int)c.V0 <= 0) {
            c.A0 = c.S0 + 0x1D0u;
            goto L8001402C;
        }
        c.A0 = c.S0 + 0x1D0u;
        c.S4 = 0u + 0u;
        L8001402C: ;
        c.A1 = c.S0 + 0u;
        c.A2 = c.S4 + 0u;
        c.RA = 0x80014038u;
        GranTurismo2PC.func_8001E328(c, m);
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 - 0x72A0u;
        c.A1 = 0u + 0u;
        c.A2 = c.A1 + 0u;
        c.RA = 0x8001404Cu;
        GranTurismo2PC.func_8002055C(c, m);
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 - 0x7298u;
        c.A1 = 0u + 0u;
        c.A2 = c.A1 + 0u;
        c.RA = 0x80014060u;
        GranTurismo2PC.func_80020A94(c, m);
        goto L80014190;
        L80014068: ;
        c.A1 = c.S0 + 0u;
        c.A2 = 0u + 0u;
        c.RA = 0x80014074u;
        GranTurismo2PC.func_8001E328(c, m);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0x552u));
        if (c.V0 != 0u) {
            c.A1 = 0x801D0000u;
            goto L80014190;
        }
        c.A1 = 0x801D0000u;
        c.A1 = c.A1 - 0x6720u;
        c.A1 = c.A1 + 0x3C74u;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0x4018u));
        c.A0 = 0x00000003u;
        c.V0 = c.V1 << 2;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << (int)(c.A0 & 31u);
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.A1;
        MemoryAccess.WriteU16(m, (c.V0 + 0xA6u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.S0 + 0x1CEu), (ushort)0u);
        c.RA = 0x800140B8u;
        GranTurismo2PC.func_80060840(c, m);
        goto L80014190;
        L800140C0: ;
        c.A0 = c.S0 + 0x1D0u;
        c.A1 = c.S0 + 0u;
        c.A2 = 0u + 0u;
        c.RA = 0x800140D0u;
        GranTurismo2PC.func_8001E328(c, m);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x19Cu));
        if ((int)c.V0 <= 0) {
            c.V0 = 0x800B0000u;
            goto L800140E4;
        }
        c.V0 = 0x800B0000u;
        c.S2 = 0u + 0u;
        L800140E4: ;
        c.S1 = c.V0 - 0x72A0u;
        c.A0 = c.S1 + 0u;
        c.A1 = c.S2 + 0u;
        c.A2 = 0x00000001u;
        c.RA = 0x800140F8u;
        GranTurismo2PC.func_8002055C(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = 0xFFFFFFFEu;
        if (c.V1 == c.V0) {
            c.V0 = 0xFFFFFFFFu;
            goto L80014170;
        }
        c.V0 = 0xFFFFFFFFu;
        if (c.V1 == c.V0) {
            c.A0 = c.S0 + 0u;
            goto L80014190;
        }
        c.A0 = c.S0 + 0u;
        goto L80014184;
        L80014118: ;
        c.A0 = c.S0 + 0x1D0u;
        c.A1 = c.S0 + 0u;
        c.A2 = 0u + 0u;
        c.RA = 0x80014128u;
        GranTurismo2PC.func_8001E328(c, m);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x19Cu));
        if ((int)c.V0 <= 0) {
            c.V0 = 0x800B0000u;
            goto L8001413C;
        }
        c.V0 = 0x800B0000u;
        c.S2 = 0u + 0u;
        L8001413C: ;
        c.S1 = c.V0 - 0x7298u;
        c.A0 = c.S1 + 0u;
        c.A1 = c.S2 + 0u;
        c.A2 = 0x00000001u;
        c.RA = 0x80014150u;
        GranTurismo2PC.func_80020A94(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = 0xFFFFFFFEu;
        if (c.V1 == c.V0) {
            c.V0 = 0xFFFFFFFFu;
            goto L80014170;
        }
        c.V0 = 0xFFFFFFFFu;
        if (c.V1 != c.V0) {
            c.A0 = c.S0 + 0u;
            goto L80014184;
        }
        c.A0 = c.S0 + 0u;
        goto L80014190;
        L80014170: ;
        MemoryAccess.WriteU16(m, (c.S0 + 0x1CEu), (ushort)0u);
        c.A0 = 0x00000002u;
        c.RA = 0x8001417Cu;
        GranTurismo2PC.func_80060840(c, m);
        goto L80014190;
        L80014184: ;
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x4u));
        c.A2 = 0u + 0u;
        c.RA = 0x80014190u;
        GranTurismo2PC.func_80014380(c, m);
        L80014190: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x19Cu));
        c.V1 = MemoryAccess.ReadU16(m, (c.S0 + 0x19Cu));
        if ((int)c.V0 <= 0) {
            c.V0 = c.V1 - 0x1u;
            goto L800141A4;
        }
        c.V0 = c.V1 - 0x1u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x19Cu), (ushort)c.V0);
        L800141A4: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x71u));
        if (c.V0 == 0u) {
            goto L800141BC;
        }
        c.S3 = 0u + 0u;
        c.RA = 0x800141BCu;
        GranTurismo2PC.func_80018FF0(c, m);
        L800141BC: ;
        if (c.S3 == 0u) {
            goto L800141CC;
        }
        c.RA = 0x800141CCu;
        GranTurismo2PC.func_80019028(c, m);
        L800141CC: ;
        c.RA = 0x800141D4u;
        GranTurismo2PC.func_80081EA8(c, m);
        MemoryAccess.WriteU32(m, (c.S0 + 0x85Cu), c.V0);
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
    public static void func_800141FC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.RA);
        c.S0 = c.A0 + 0u;
        c.RA = 0x80014210u;
        GranTurismo2PC.func_80080D04(c, m);
        c.V0 = 0x00000200u;
        MemoryAccess.WriteU16(m, (c.SP + 0x1Cu), (ushort)c.V0);
        c.V0 = 0x000001F8u;
        MemoryAccess.WriteU16(m, (c.SP + 0x18u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.SP + 0x1Au), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.SP + 0x1Eu), (ushort)c.V0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x1CEu));
        c.A1 = MemoryAccess.ReadU32(m, (c.S0 + 0x88u));
        if (c.V0 != 0u) {
            c.A0 = c.S0 + 0x80u;
            goto L80014244;
        }
        c.A0 = c.S0 + 0x80u;
        c.A0 = c.S0 + 0x1D0u;
        c.RA = 0x80014240u;
        GranTurismo2PC.func_8001EA24(c, m);
        c.A0 = c.S0 + 0x80u;
        L80014244: ;
        c.A1 = c.SP + 0x18u;
        c.RA = 0x8001424Cu;
        GranTurismo2PC.func_8008034C(c, m);
        c.A0 = c.S0 + 0x2FCu;
        c.A1 = MemoryAccess.ReadU32(m, (c.S0 + 0x98u));
        c.A3 = MemoryAccess.ReadU32(m, (c.S0 + 0x88u));
        c.A2 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x70u));
        c.V0 = c.S0 + 0xA0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.RA = 0x80014268u;
        GranTurismo2PC.func_8001B9AC(c, m);
        c.A0 = c.S0 + 0x90u;
        c.A1 = c.SP + 0x18u;
        c.RA = 0x80014274u;
        GranTurismo2PC.func_8008034C(c, m);
        c.RA = 0x8001427Cu;
        GranTurismo2PC.func_80081EA8(c, m);
        MemoryAccess.WriteU32(m, (c.S0 + 0x860u), c.V0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80014290(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.S0 = c.A0 + 0u;
        c.RA = 0x800142A4u;
        GranTurismo2PC.func_8007FEF0(c, m);
        c.A0 = c.S0 + 0xB0u;
        c.RA = 0x800142ACu;
        GranTurismo2PC.func_8007FB38(c, m);
        c.A0 = c.S0 + 0x114u;
        c.RA = 0x800142B4u;
        GranTurismo2PC.func_8007FB38(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800142C4(CpuContext c, IMemory m)
    {
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800142CC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        c.A0 = c.S1 + 0x2FCu;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.RA = 0x800142E8u;
        GranTurismo2PC.func_8001D68C(c, m);
        c.A0 = 0u + 0u;
        c.V1 = MemoryAccess.ReadU32(m, (c.S1 + 0x1F8u));
        if ((int)c.V0 < 0) {
            c.A1 = c.V0 + 0u;
            goto L800142FC;
        }
        c.A1 = c.V0 + 0u;
        c.A0 = c.V1 & 0x0001u;
        L800142FC: ;
        if (c.A0 == 0u) {
            c.A0 = 0x00000002u;
            goto L8001432C;
        }
        c.A0 = 0x00000002u;
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x1C0u), c.V0);
        c.V0 = 0x00000003u;
        c.S0 = 0x00000008u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x1BCu), c.A1);
        MemoryAccess.WriteU8(m, (c.S1 + 0x1C4u), (byte)c.V0);
        MemoryAccess.WriteU16(m, (c.S1 + 0x19Cu), (ushort)c.S0);
        c.RA = 0x80014324u;
        GranTurismo2PC.func_80060840(c, m);
        MemoryAccess.WriteU16(m, (c.S1 + 0x19Cu), (ushort)c.S0);
        goto L80014334;
        L8001432C: ;
        c.A0 = 0u + 0u;
        c.RA = 0x80014334u;
        GranTurismo2PC.func_80060840(c, m);
        L80014334: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80014348(CpuContext c, IMemory m)
    {
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.T2 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        MemoryAccess.WriteU8(m, (c.A0 + 0x1B0u), (byte)c.A1);
        MemoryAccess.WriteU32(m, (c.A0 + 0x1A0u), c.A2);
        MemoryAccess.WriteU32(m, (c.A0 + 0x1A4u), c.A3);
        MemoryAccess.WriteU32(m, (c.A0 + 0x1A8u), c.V0);
        MemoryAccess.WriteU32(m, (c.A0 + 0x1ACu), c.V1);
        MemoryAccess.WriteU8(m, (c.A0 + 0x1B1u), (byte)c.T0);
        MemoryAccess.WriteU32(m, (c.A0 + 0x1B4u), c.T1);
        MemoryAccess.WriteU32(m, (c.A0 + 0x1B8u), c.T2);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80014380(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x58u;
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.S5);
        c.S5 = c.A0 + 0u;
        c.T0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x54u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x50u), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0x4Cu), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x48u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x3Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x38u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x34u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x5Cu), c.A1);
        MemoryAccess.WriteU32(m, (c.SP + 0x60u), c.A2);
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.T0);
        c.V0 = MemoryAccess.ReadU32(m, (c.A1 + 0x8u));
        c.S2 = c.T0 + 0u;
        c.V1 = c.V0 >> 24;
        c.V1 = c.V1 & c.T0;
        c.T0 = 0x801D0000u;
        c.T0 = c.T0 - 0x2AACu;
        c.S7 = (uint)(short)MemoryAccess.ReadU16(m, (c.T0 + 0x4018u));
        if (c.V1 == 0u) {
            c.FP = c.V0 & 0xFFFFu;
            goto L8001482C;
        }
        c.FP = c.V0 & 0xFFFFu;
        c.V0 = 0x0000009Cu;
        if (c.FP == c.V0) {
            c.V0 = (int)c.FP < 157 ? 1u : 0u;
            goto L80014628;
        }
        c.V0 = (int)c.FP < 157 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000050u;
            goto L8001442C;
        }
        c.V0 = 0x00000050u;
        if (c.FP == c.V0) {
            c.V0 = (int)c.FP < 81 ? 1u : 0u;
            goto L80014464;
        }
        c.V0 = (int)c.FP < 81 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000009u;
            goto L80014418;
        }
        c.V0 = 0x00000009u;
        if (c.FP == c.V0) {
            goto L80014534;
        }
        goto L800147FC;
        L80014418: ;
        c.V0 = 0x00000098u;
        if (c.FP == c.V0) {
            goto L800146C8;
        }
        goto L800147FC;
        L8001442C: ;
        c.V0 = 0x000000AEu;
        if (c.FP == c.V0) {
            c.V0 = (int)c.FP < 175 ? 1u : 0u;
            goto L80014798;
        }
        c.V0 = (int)c.FP < 175 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x000000ADu;
            goto L80014450;
        }
        c.V0 = 0x000000ADu;
        if (c.FP == c.V0) {
            goto L80014718;
        }
        goto L800147FC;
        L80014450: ;
        c.V0 = 0x000000BBu;
        if (c.FP == c.V0) {
            goto L8001482C;
        }
        goto L800147FC;
        L80014464: ;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S5 + 0x1CEu));
        if (c.V1 == 0u) {
            c.V0 = 0x00000002u;
            goto L80014484;
        }
        c.V0 = 0x00000002u;
        if (c.V1 == c.V0) {
            c.S0 = 0x800B0000u;
            goto L800144AC;
        }
        c.S0 = 0x800B0000u;
        goto L80014808;
        L80014484: ;
        c.V0 = 0x800B0000u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 - 0x7298u));
        if ((int)c.V0 <= 0) {
            c.V0 = 0x00000002u;
            goto L80014584;
        }
        c.V0 = 0x00000002u;
        MemoryAccess.WriteU16(m, (c.S5 + 0x1CEu), (ushort)c.V0);
        c.A0 = 0x00000001u;
        c.RA = 0x800144A4u;
        GranTurismo2PC.func_80060840(c, m);
        goto L8001482C;
        L800144AC: ;
        c.S0 = c.S0 - 0x7298u;
        MemoryAccess.WriteU16(m, (c.S5 + 0x1CEu), (ushort)0u);
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x5Cu));
        c.S4 = MemoryAccess.ReadU32(m, (c.T0 + 0xCu));
        c.A0 = c.S0 + 0u;
        c.RA = 0x800144C8u;
        GranTurismo2PC.func_80020BDC(c, m);
        c.S3 = c.V0 + 0u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x800144D4u;
        GranTurismo2PC.func_80020C00(c, m);
        c.A0 = c.S3 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x800144E0u;
        GranTurismo2PC.func_80018350(c, m);
        c.A0 = c.S5 + 0u;
        c.A1 = 0x00000001u;
        c.A2 = c.S3 + 0u;
        c.A3 = c.S3 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.V0);
        c.RA = 0x8001450Cu;
        GranTurismo2PC.func_80014348(c, m);
        c.A0 = c.S0 + 0u;
        c.S0 = 0x801C0000u;
        c.S0 = c.S0 + 0x3080u;
        c.RA = 0x8001451Cu;
        GranTurismo2PC.func_80020C24(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S3 + 0u;
        c.A2 = c.V0 + 0u;
        c.RA = 0x8001452Cu;
        GranTurismo2PC.func_8001DAA8(c, m);
        goto L80014808;
        L80014534: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S5 + 0x1CEu));
        if (c.V0 == 0u) {
            goto L8001455C;
        }
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x2Cu));
        if (c.V0 == c.T0) {
            c.V0 = 0x800B0000u;
            goto L80014594;
        }
        c.V0 = 0x800B0000u;
        goto L80014808;
        L8001455C: ;
        c.V0 = 0x800B0000u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 - 0x72A0u));
        if ((int)c.V0 <= 0) {
            c.A0 = 0x00000001u;
            goto L80014584;
        }
        c.A0 = 0x00000001u;
        c.T0 = MemoryAccess.ReadU16(m, (c.SP + 0x2Cu));
        MemoryAccess.WriteU16(m, (c.S5 + 0x1CEu), (ushort)c.T0);
        c.RA = 0x8001457Cu;
        GranTurismo2PC.func_80060840(c, m);
        goto L8001482C;
        L80014584: ;
        c.A0 = 0u + 0u;
        c.RA = 0x8001458Cu;
        GranTurismo2PC.func_80060840(c, m);
        goto L8001482C;
        L80014594: ;
        MemoryAccess.WriteU16(m, (c.S5 + 0x1CEu), (ushort)0u);
        c.S1 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 - 0x729Eu));
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x5Cu));
        c.S0 = c.S1 << 2;
        c.S0 = c.S0 + c.S1;
        c.S0 = c.S0 << 3;
        c.S0 = c.S0 + c.S1;
        c.S0 = c.S0 << 2;
        c.S0 = c.S0 + 0x4u;
        c.S4 = MemoryAccess.ReadU32(m, (c.T0 + 0xCu));
        c.T0 = 0x801D0000u;
        c.T0 = c.T0 - 0x2AACu;
        c.S0 = c.S0 + c.T0;
        c.S3 = MemoryAccess.ReadU32(m, (c.S0 + 0x8Cu));
        c.S6 = MemoryAccess.ReadU32(m, c.S0);
        c.A1 = MemoryAccess.ReadU32(m, (c.S0 + 0x4u));
        c.S2 = MemoryAccess.ReadU32(m, (c.S0 + 0x8u));
        c.A0 = c.S3 + 0u;
        c.RA = 0x800145E0u;
        GranTurismo2PC.func_80018350(c, m);
        c.A0 = c.S5 + 0u;
        c.A1 = 0x00000001u;
        c.A2 = c.S3 + 0u;
        c.V1 = MemoryAccess.ReadU16(m, (c.S0 + 0xA2u));
        c.A3 = c.S6 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.V0 = 0xFFFFE0FFu;
        c.V0 = c.S2 & c.V0;
        c.V0 = 0u < c.V0 ? 1u : 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.V1);
        c.RA = 0x80014618u;
        GranTurismo2PC.func_80014348(c, m);
        c.A0 = c.S1 + 0u;
        c.RA = 0x80014620u;
        GranTurismo2PC.func_80017288(c, m);
        goto L80014808;
        L80014628: ;
        if ((int)c.S7 < 0) {
            c.A0 = c.S7 << 2;
            goto L800147F0;
        }
        c.A0 = c.S7 << 2;
        c.A0 = c.A0 + c.S7;
        c.A0 = c.A0 << 3;
        c.A0 = c.A0 + c.S7;
        c.A0 = c.A0 << 2;
        c.A0 = c.A0 + 0x4u;
        c.T0 = 0x801D0000u;
        c.T0 = c.T0 - 0x2AACu;
        c.A0 = c.A0 + c.T0;
        c.A1 = 0x00000022u;
        c.RA = 0x80014658u;
        GranTurismo2PC.func_8005E874(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 == 0u) {
            c.S4 = 0x80000000u;
            goto L800146C0;
        }
        c.S4 = 0x80000000u;
        c.RA = 0x8001466Cu;
        GranTurismo2PC.func_800174AC(c, m);
        if (c.V0 == 0u) {
            c.S4 = c.S4 | 0x0019u;
            goto L80014808;
        }
        c.S4 = c.S4 | 0x0019u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x5Cu));
        c.S4 = MemoryAccess.ReadU32(m, (c.T0 + 0xCu));
        c.RA = 0x80014688u;
        GranTurismo2PC.func_800174F4(c, m);
        c.S3 = c.V0 + 0u;
        c.A0 = c.S5 + 0u;
        c.A1 = 0x00000001u;
        c.A2 = c.S3 + 0u;
        c.A3 = c.S3 + 0u;
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.V0);
        c.RA = 0x800146B8u;
        GranTurismo2PC.func_80014348(c, m);
        goto L80014808;
        L800146C0: ;
        c.S4 = c.S4 | 0x001Au;
        goto L80014808;
        L800146C8: ;
        if ((int)c.S7 < 0) {
            c.V0 = c.S7 << 2;
            goto L800147E4;
        }
        c.V0 = c.S7 << 2;
        c.V0 = c.V0 + c.S7;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.S7;
        c.V0 = c.V0 << 2;
        c.T0 = 0x801D0000u;
        c.T0 = c.T0 - 0x2AACu;
        c.V0 = c.V0 + c.T0;
        c.V1 = 0x800B0000u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V0 + 0x85u));
        c.V0 = MemoryAccess.ReadU8(m, (c.V1 - 0x7288u));
        if (c.A0 == c.V0) {
            c.A0 = 0x801C0000u;
            goto L800147FC;
        }
        c.A0 = 0x801C0000u;
        c.A0 = c.A0 + 0x3080u;
        c.RA = 0x8001470Cu;
        GranTurismo2PC.func_8001DA30(c, m);
        c.S4 = 0x80000000u;
        c.S4 = c.S4 | 0x001Du;
        goto L80014808;
        L80014718: ;
        if ((int)c.S7 < 0) {
            c.S0 = c.S7 << 2;
            goto L800147E4;
        }
        c.S0 = c.S7 << 2;
        c.S0 = c.S0 + c.S7;
        c.S0 = c.S0 << 3;
        c.S0 = c.S0 + c.S7;
        c.S0 = c.S0 << 2;
        c.S0 = c.S0 + 0x4u;
        c.T0 = 0x801D0000u;
        c.T0 = c.T0 - 0x2AACu;
        c.S0 = c.S0 + c.T0;
        c.S3 = MemoryAccess.ReadU32(m, (c.S0 + 0x8Cu));
        c.S6 = MemoryAccess.ReadU32(m, c.S0);
        c.A1 = MemoryAccess.ReadU32(m, (c.S0 + 0x4u));
        c.S1 = MemoryAccess.ReadU32(m, (c.S0 + 0x8u));
        c.A0 = c.S3 + 0u;
        c.RA = 0x80014758u;
        GranTurismo2PC.func_80018350(c, m);
        c.A0 = c.S5 + 0u;
        c.A1 = 0x00000001u;
        c.A2 = c.S3 + 0u;
        c.V1 = MemoryAccess.ReadU16(m, (c.S0 + 0xA2u));
        c.A3 = c.S6 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.V0 = 0xFFFFE0FFu;
        c.V0 = c.S1 & c.V0;
        c.V0 = 0u < c.V0 ? 1u : 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.V1);
        c.RA = 0x80014790u;
        GranTurismo2PC.func_80014348(c, m);
        goto L800147FC;
        L80014798: ;
        if ((int)c.S7 < 0) {
            c.V0 = c.S7 << 2;
            goto L800147E4;
        }
        c.V0 = c.S7 << 2;
        c.V0 = c.V0 + c.S7;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.S7;
        c.V0 = c.V0 << 2;
        c.T0 = 0x801D0000u;
        c.T0 = c.T0 - 0x2AACu;
        c.V0 = c.V0 + c.T0;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.RA = 0x800147C8u;
        GranTurismo2PC.func_80018210(c, m);
        if (c.V0 != 0u) {
            c.A0 = 0x801C0000u;
            goto L800147FC;
        }
        c.A0 = 0x801C0000u;
        c.A0 = c.A0 + 0x3080u;
        c.RA = 0x800147D8u;
        GranTurismo2PC.func_8001DA30(c, m);
        c.S4 = 0x80000000u;
        c.S4 = c.S4 | 0x001Eu;
        goto L80014808;
        L800147E4: ;
        c.A0 = 0x801C0000u;
        c.A0 = c.A0 + 0x3080u;
        c.RA = 0x800147F0u;
        GranTurismo2PC.func_8001DA30(c, m);
        L800147F0: ;
        c.S4 = 0x80000000u;
        c.S4 = c.S4 | 0x0002u;
        goto L80014808;
        L800147FC: ;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x5Cu));
        c.S4 = MemoryAccess.ReadU32(m, (c.T0 + 0xCu));
        L80014808: ;
        c.A0 = 0x00000001u;
        c.RA = 0x80014810u;
        GranTurismo2PC.func_80060840(c, m);
        c.S2 = 0u + 0u;
        c.V0 = 0x00000003u;
        MemoryAccess.WriteU8(m, (c.S5 + 0x1C4u), (byte)c.V0);
        c.V0 = 0x00000008u;
        MemoryAccess.WriteU32(m, (c.S5 + 0x1BCu), c.S4);
        MemoryAccess.WriteU32(m, (c.S5 + 0x1C0u), 0u);
        MemoryAccess.WriteU16(m, (c.S5 + 0x19Cu), (ushort)c.V0);
        L8001482C: ;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x5Cu));
        c.V1 = MemoryAccess.ReadU32(m, (c.T0 + 0x8u));
        c.V0 = c.V1 >> 27;
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u) {
            c.V0 = c.V1 >> 23;
            goto L80014968;
        }
        c.V0 = c.V1 >> 23;
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 != 0u) {
            c.S0 = c.T0 + 0xCu;
            goto L80014968;
        }
        c.S0 = c.T0 + 0xCu;
        c.V0 = c.V1 >> 22;
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 != 0u) {
            c.V0 = c.V1 >> 21;
            goto L80014968;
        }
        c.V0 = c.V1 >> 21;
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 != 0u) {
            c.V0 = c.V1 >> 20;
            goto L80014968;
        }
        c.V0 = c.V1 >> 20;
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 != 0u) {
            c.V0 = c.V1 >> 30;
            goto L80014968;
        }
        c.V0 = c.V1 >> 30;
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u) {
            goto L800148B4;
        }
        c.A0 = 0x00000001u;
        c.RA = 0x80014894u;
        GranTurismo2PC.func_80060840(c, m);
        c.A0 = 0x800C0000u;
        c.A0 = c.A0 + 0x1C10u;
        c.A1 = c.S0 + 0u;
        c.RA = 0x800148A4u;
        GranTurismo2PC.func_8008CEDC(c, m);
        c.V1 = 0x801F0000u;
        c.V1 = c.V1 - 0xA10u;
        c.V0 = 0x00000003u;
        goto L80014938;
        L800148B4: ;
        c.A0 = c.S0 + 0u;
        c.RA = 0x800148BCu;
        GranTurismo2PC.func_80018608(c, m);
        if (c.V0 == 0u) {
            c.A0 = c.S0 + 0u;
            goto L800148D4;
        }
        c.A0 = c.S0 + 0u;
        c.A1 = c.SP + 0x28u;
        c.RA = 0x800148CCu;
        GranTurismo2PC.func_80019B88(c, m);
        c.V1 = 0x00000001u;
        goto L800148E0;
        L800148D4: ;
        c.A1 = c.SP + 0x28u;
        c.RA = 0x800148DCu;
        GranTurismo2PC.func_8001973C(c, m);
        c.V1 = 0x00000001u;
        L800148E0: ;
        if (c.V0 == c.V1) {
            goto L80014914;
        }
        c.A0 = 0u + 0u;
        c.RA = 0x800148F0u;
        GranTurismo2PC.func_80060840(c, m);
        c.S2 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.S5 + 0x1C0u), 0u);
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.V1 = 0x00000003u;
        MemoryAccess.WriteU8(m, (c.S5 + 0x1C4u), (byte)c.V1);
        c.V1 = 0x00000008u;
        MemoryAccess.WriteU16(m, (c.S5 + 0x19Cu), (ushort)c.V1);
        MemoryAccess.WriteU32(m, (c.S5 + 0x1BCu), c.V0);
        goto L80014960;
        L80014914: ;
        c.A0 = 0x00000001u;
        c.RA = 0x8001491Cu;
        GranTurismo2PC.func_80060840(c, m);
        c.A0 = 0x800C0000u;
        c.A0 = c.A0 + 0x1C10u;
        c.A1 = c.S0 + 0u;
        c.RA = 0x8001492Cu;
        GranTurismo2PC.func_8008CEDC(c, m);
        c.V1 = 0x801F0000u;
        c.V1 = c.V1 - 0xA10u;
        c.V0 = 0x00000002u;
        L80014938: ;
        c.A0 = c.S5 + 0x2FCu;
        MemoryAccess.WriteU8(m, (c.V1 + 0x5u), (byte)c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.A0 + 0x4u));
        MemoryAccess.WriteU32(m, (c.V1 + 0x8u), c.V0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0x550u));
        c.S2 = 0u + 0u;
        MemoryAccess.WriteU16(m, (c.V1 + 0xCu), (ushort)c.V0);
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.S5 + 0x71u), (byte)c.V0);
        L80014960: ;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x5Cu));
        L80014968: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.T0 + 0x8u));
        c.V0 = c.V0 >> 28;
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u) {
            c.V1 = c.FP - 0x2u;
            goto L80014A04;
        }
        c.V1 = c.FP - 0x2u;
        c.A0 = c.T0 + 0xCu;
        c.RA = 0x80014988u;
        GranTurismo2PC.func_80013A28(c, m);
        c.S0 = c.V0 + 0u;
        c.A0 = c.S0 + 0u;
        c.V0 = c.S7 << 2;
        c.V0 = c.V0 + c.S7;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.S7;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + 0x4u;
        c.T0 = 0x801D0000u;
        c.T0 = c.T0 - 0x2AACu;
        c.V0 = c.T0 + c.V0;
        c.A1 = MemoryAccess.ReadU32(m, c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x8Cu));
        c.S2 = 0u + 0u;
        c.A1 = c.A1 ^ c.V0;
        c.A1 = 0u < c.A1 ? 1u : 0u;
        c.RA = 0x800149CCu;
        GranTurismo2PC.func_80021BEC(c, m);
        c.A0 = 0x00000001u;
        c.S1 = c.V0 + 0u;
        c.RA = 0x800149D8u;
        GranTurismo2PC.func_80060840(c, m);
        c.A0 = 0x801C0000u;
        c.A0 = c.A0 + 0x3080u;
        c.A1 = c.S0 + 0u;
        c.RA = 0x800149E8u;
        GranTurismo2PC.func_8001DB0C(c, m);
        c.V0 = 0x00000003u;
        MemoryAccess.WriteU8(m, (c.S5 + 0x1CCu), (byte)c.V0);
        c.V0 = 0x00000008u;
        MemoryAccess.WriteU32(m, (c.S5 + 0x1C8u), c.S0);
        MemoryAccess.WriteU8(m, (c.S5 + 0x1CDu), (byte)c.S1);
        MemoryAccess.WriteU16(m, (c.S5 + 0x19Cu), (ushort)c.V0);
        c.V1 = c.FP - 0x2u;
        L80014A04: ;
        c.V0 = c.V1 < 0x000000AAu ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80020000u;
            goto L80014DD0;
        }
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x29B4u;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x80014A68u: goto L80014A68;
            case 0x80014A2Cu: goto L80014A2C;
            case 0x80014D40u: goto L80014D40;
            case 0x80014CDCu: goto L80014CDC;
            case 0x80014BA0u: goto L80014BA0;
            case 0x80014DD0u: goto L80014DD0;
            case 0x80014E2Cu: goto L80014E2C;
            case 0x80014D0Cu: goto L80014D0C;
            case 0x80014C78u: goto L80014C78;
            case 0x80014AC8u: goto L80014AC8;
            case 0x80014B2Cu: goto L80014B2C;
            case 0x80014D78u: goto L80014D78;
            case 0x80014DA0u: goto L80014DA0;
            case 0x80014B58u: goto L80014B58;
            case 0x80014BF0u: goto L80014BF0;
            case 0x80014C34u: goto L80014C34;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L80014A2C: ;
        c.A0 = 0x801C0000u;
        c.A0 = c.A0 + 0x3080u;
        c.RA = 0x80014A38u;
        GranTurismo2PC.func_8001DD3C(c, m);
        c.A0 = c.S5 + 0x2FCu;
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.S5 + 0x1F8u), c.V0);
        c.RA = 0x80014A48u;
        GranTurismo2PC.func_8001D68C(c, m);
        c.A0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.S5 + 0x1BCu), c.V0);
        c.V0 = 0x00000003u;
        MemoryAccess.WriteU8(m, (c.S5 + 0x1C4u), (byte)c.V0);
        c.V0 = 0x00000008u;
        MemoryAccess.WriteU32(m, (c.S5 + 0x1C0u), 0u);
        MemoryAccess.WriteU16(m, (c.S5 + 0x19Cu), (ushort)c.V0);
        goto L80014E24;
        L80014A68: ;
        c.A0 = c.S5 + 0x324u;
        c.S0 = 0x801C0000u;
        c.S0 = c.S0 + 0x3080u;
        c.S1 = c.S5 + 0x2FCu;
        c.RA = 0x80014A7Cu;
        GranTurismo2PC.func_8001A454(c, m);
        MemoryAccess.WriteU32(m, (c.S0 + 0x10u), c.V0);
        c.A0 = c.S0 + 0u;
        c.RA = 0x80014A88u;
        GranTurismo2PC.func_8001DDAC(c, m);
        c.S4 = c.V0 + 0u;
        if (c.S4 != 0u) {
            c.A0 = 0x00000001u;
            goto L80014AAC;
        }
        c.A0 = 0x00000001u;
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.S5 + 0x1F8u), c.V0);
        c.A0 = c.S1 + 0u;
        c.RA = 0x80014AA4u;
        GranTurismo2PC.func_8001D68C(c, m);
        c.S4 = c.V0 + 0u;
        c.A0 = 0x00000001u;
        L80014AAC: ;
        c.V0 = 0x00000003u;
        MemoryAccess.WriteU8(m, (c.S5 + 0x1C4u), (byte)c.V0);
        c.V0 = 0x00000008u;
        MemoryAccess.WriteU32(m, (c.S5 + 0x1BCu), c.S4);
        MemoryAccess.WriteU32(m, (c.S5 + 0x1C0u), 0u);
        MemoryAccess.WriteU16(m, (c.S5 + 0x19Cu), (ushort)c.V0);
        goto L80014E24;
        L80014AC8: ;
        c.S0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.S5 + 0x1F8u), c.S0);
        c.A0 = 0x801C0000u;
        c.A0 = c.A0 + 0x3080u;
        c.RA = 0x80014ADCu;
        GranTurismo2PC.func_8001DCD0(c, m);
        if (c.V0 == 0u) {
            goto L80014B04;
        }
        c.A0 = c.S5 + 0x2FCu;
        c.RA = 0x80014AECu;
        GranTurismo2PC.func_8001D680(c, m);
        MemoryAccess.WriteU32(m, (c.S5 + 0x1BCu), c.V0);
        c.V0 = 0x00000003u;
        MemoryAccess.WriteU8(m, (c.S5 + 0x1C4u), (byte)c.V0);
        c.V0 = 0x00000008u;
        MemoryAccess.WriteU32(m, (c.S5 + 0x1C0u), c.S0);
        goto L80014B20;
        L80014B04: ;
        c.A0 = c.S5 + 0x2FCu;
        c.RA = 0x80014B0Cu;
        GranTurismo2PC.func_8001D68C(c, m);
        MemoryAccess.WriteU32(m, (c.S5 + 0x1BCu), c.V0);
        c.V0 = 0x00000003u;
        MemoryAccess.WriteU8(m, (c.S5 + 0x1C4u), (byte)c.V0);
        c.V0 = 0x00000008u;
        MemoryAccess.WriteU32(m, (c.S5 + 0x1C0u), 0u);
        L80014B20: ;
        MemoryAccess.WriteU16(m, (c.S5 + 0x19Cu), (ushort)c.V0);
        c.A0 = 0x00000001u;
        goto L80014E24;
        L80014B2C: ;
        c.A0 = c.S5 + 0x2FCu;
        c.RA = 0x80014B34u;
        GranTurismo2PC.func_8001D68C(c, m);
        c.A0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.S5 + 0x1BCu), c.V0);
        c.V0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.S5 + 0x1C0u), c.V0);
        c.V0 = 0x00000003u;
        MemoryAccess.WriteU8(m, (c.S5 + 0x1C4u), (byte)c.V0);
        c.V0 = 0x00000008u;
        MemoryAccess.WriteU16(m, (c.S5 + 0x19Cu), (ushort)c.V0);
        goto L80014E24;
        L80014B58: ;
        c.RA = 0x80014B60u;
        GranTurismo2PC.func_80017530(c, m);
        c.S3 = c.V0 + 0u;
        c.A0 = c.S5 + 0u;
        c.A1 = 0x00000003u;
        c.A2 = c.S3 + 0u;
        c.A3 = c.A2 + 0u;
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.V0);
        c.RA = 0x80014B90u;
        GranTurismo2PC.func_80014348(c, m);
        c.A0 = 0x00000001u;
        c.V0 = 0x00000004u;
        MemoryAccess.WriteU16(m, (c.S5 + 0x19Cu), (ushort)c.V0);
        goto L80014E24;
        L80014BA0: ;
        c.A0 = 0x801C0000u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x5Cu));
        c.A0 = c.A0 + 0x3080u;
        c.S3 = MemoryAccess.ReadU32(m, (c.T0 + 0x10u));
        c.A2 = 0u + 0u;
        c.A1 = c.S3 + 0u;
        c.RA = 0x80014BBCu;
        GranTurismo2PC.func_8001DAA8(c, m);
        c.A0 = c.S5 + 0u;
        c.A1 = 0x00000001u;
        c.A2 = c.S3 + 0u;
        c.A3 = c.A2 + 0u;
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.V0);
        c.RA = 0x80014BE8u;
        GranTurismo2PC.func_80014348(c, m);
        goto L80014E2C;
        L80014BF0: ;
        c.V0 = 0x800B0000u;
        c.S0 = 0x801C0000u;
        c.S0 = c.S0 + 0x3080u;
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 - 0x729Eu));
        c.A0 = c.S0 + 0u;
        c.RA = 0x80014C08u;
        GranTurismo2PC.func_8001DA38(c, m);
        c.A0 = c.S0 + 0u;
        c.RA = 0x80014C10u;
        GranTurismo2PC.func_8001DFEC(c, m);
        c.A0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.S5 + 0x1BCu), c.V0);
        c.V0 = 0x00000003u;
        MemoryAccess.WriteU8(m, (c.S5 + 0x1C4u), (byte)c.V0);
        c.V0 = 0x00000008u;
        MemoryAccess.WriteU32(m, (c.S5 + 0x1F8u), 0u);
        MemoryAccess.WriteU32(m, (c.S5 + 0x1C0u), 0u);
        MemoryAccess.WriteU16(m, (c.S5 + 0x19Cu), (ushort)c.V0);
        goto L80014E24;
        L80014C34: ;
        c.V0 = 0x800B0000u;
        c.S0 = 0x801C0000u;
        c.S0 = c.S0 + 0x3080u;
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 - 0x729Eu));
        c.A0 = c.S0 + 0u;
        c.RA = 0x80014C4Cu;
        GranTurismo2PC.func_8001DA98(c, m);
        c.A0 = c.S0 + 0u;
        c.RA = 0x80014C54u;
        GranTurismo2PC.func_8001DFEC(c, m);
        c.A0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.S5 + 0x1BCu), c.V0);
        c.V0 = 0x00000003u;
        MemoryAccess.WriteU8(m, (c.S5 + 0x1C4u), (byte)c.V0);
        c.V0 = 0x00000008u;
        MemoryAccess.WriteU32(m, (c.S5 + 0x1F8u), 0u);
        MemoryAccess.WriteU32(m, (c.S5 + 0x1C0u), 0u);
        MemoryAccess.WriteU16(m, (c.S5 + 0x19Cu), (ushort)c.V0);
        goto L80014E24;
        L80014C78: ;
        c.RA = 0x80014C80u;
        GranTurismo2PC.func_80018274(c, m);
        if (c.V0 == 0u) {
            c.V0 = c.S7 << 2;
            goto L80014E20;
        }
        c.V0 = c.S7 << 2;
        c.V0 = c.V0 + c.S7;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.S7;
        c.V0 = c.V0 << 2;
        c.T0 = 0x801D0000u;
        c.T0 = c.T0 - 0x2AACu;
        c.V0 = c.V0 + c.T0;
        c.V0 = MemoryAccess.ReadU16(m, (c.V0 + 0xA6u));
        if (c.V0 == 0u) {
            c.T0 = 0x801D0000u;
            goto L80014E20;
        }
        c.T0 = 0x801D0000u;
        c.T0 = c.T0 - 0x2AACu;
        c.V0 = MemoryAccess.ReadU32(m, (c.T0 + 0x4014u));
        c.A0 = c.S5 + 0x2FCu;
        c.V0 = c.V0 - 0x32u;
        MemoryAccess.WriteU32(m, (c.T0 + 0x4014u), c.V0);
        c.V0 = 0x00000003u;
        MemoryAccess.WriteU16(m, (c.S5 + 0x1CEu), (ushort)c.V0);
        c.RA = 0x80014CD4u;
        GranTurismo2PC.func_8001D104(c, m);
        c.A0 = 0x00000001u;
        goto L80014E24;
        L80014CDC: ;
        c.A0 = c.S5 + 0x2FCu;
        c.RA = 0x80014CE4u;
        GranTurismo2PC.func_8001D698(c, m);
        c.S4 = c.V0 + 0u;
        if (c.S4 == 0u) {
            c.A0 = 0x00000001u;
            goto L80014E20;
        }
        c.A0 = 0x00000001u;
        c.V0 = 0x00000003u;
        MemoryAccess.WriteU8(m, (c.S5 + 0x1C4u), (byte)c.V0);
        c.V0 = 0x00000008u;
        MemoryAccess.WriteU32(m, (c.S5 + 0x1BCu), c.S4);
        MemoryAccess.WriteU32(m, (c.S5 + 0x1C0u), 0u);
        MemoryAccess.WriteU16(m, (c.S5 + 0x19Cu), (ushort)c.V0);
        goto L80014E24;
        L80014D0C: ;
        c.V1 = 0x801F0000u;
        c.V1 = c.V1 - 0xA10u;
        c.A0 = c.S5 + 0x2FCu;
        MemoryAccess.WriteU8(m, (c.V1 + 0x5u), (byte)0u);
        c.V0 = MemoryAccess.ReadU32(m, (c.A0 + 0x4u));
        MemoryAccess.WriteU32(m, (c.V1 + 0x8u), c.V0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0x550u));
        MemoryAccess.WriteU16(m, (c.V1 + 0xCu), (ushort)c.V0);
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.S5 + 0x71u), (byte)c.V0);
        goto L80014E2C;
        L80014D40: ;
        c.A0 = 0x801C0000u;
        c.A0 = c.A0 + 0x3080u;
        c.RA = 0x80014D4Cu;
        GranTurismo2PC.func_8001DFEC(c, m);
        c.S4 = c.V0 + 0u;
        if (c.S4 == 0u) {
            c.A0 = 0x00000001u;
            goto L80014E20;
        }
        c.A0 = 0x00000001u;
        c.V0 = 0x00000003u;
        MemoryAccess.WriteU8(m, (c.S5 + 0x1C4u), (byte)c.V0);
        c.V0 = 0x00000008u;
        MemoryAccess.WriteU32(m, (c.S5 + 0x1F8u), 0u);
        MemoryAccess.WriteU32(m, (c.S5 + 0x1BCu), c.S4);
        MemoryAccess.WriteU32(m, (c.S5 + 0x1C0u), 0u);
        MemoryAccess.WriteU16(m, (c.S5 + 0x19Cu), (ushort)c.V0);
        goto L80014E24;
        L80014D78: ;
        c.S4 = 0x80000000u;
        c.S4 = c.S4 | 0x002Eu;
        c.A0 = 0x00000001u;
        c.V0 = 0x00000003u;
        MemoryAccess.WriteU8(m, (c.S5 + 0x1C4u), (byte)c.V0);
        c.V0 = 0x00000008u;
        MemoryAccess.WriteU32(m, (c.S5 + 0x1BCu), c.S4);
        MemoryAccess.WriteU32(m, (c.S5 + 0x1C0u), 0u);
        MemoryAccess.WriteU16(m, (c.S5 + 0x19Cu), (ushort)c.V0);
        goto L80014E24;
        L80014DA0: ;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x60u));
        if (c.T0 == 0u) {
            c.A1 = 0x00000001u;
            goto L80014DB8;
        }
        c.A1 = 0x00000001u;
        c.A0 = c.S5 + 0x324u;
        goto L80014DC0;
        L80014DB8: ;
        c.A0 = c.S5 + 0x324u;
        c.A1 = 0xFFFFFFFFu;
        L80014DC0: ;
        c.RA = 0x80014DC8u;
        GranTurismo2PC.func_8001A530(c, m);
        c.A0 = 0x00000001u;
        goto L80014E24;
        L80014DD0: ;
        c.A1 = c.FP - 0x14u;
        c.V0 = c.A1 < 0x00000033u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.S0 = 0x801C0000u;
            goto L80014E18;
        }
        c.S0 = 0x801C0000u;
        c.S0 = c.S0 + 0x3080u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x80014DECu;
        GranTurismo2PC.func_8001DB24(c, m);
        c.A0 = c.S0 + 0u;
        c.RA = 0x80014DF4u;
        GranTurismo2PC.func_8001DFEC(c, m);
        c.A0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.S5 + 0x1BCu), c.V0);
        c.V0 = 0x00000003u;
        MemoryAccess.WriteU8(m, (c.S5 + 0x1C4u), (byte)c.V0);
        c.V0 = 0x00000008u;
        MemoryAccess.WriteU32(m, (c.S5 + 0x1F8u), 0u);
        MemoryAccess.WriteU32(m, (c.S5 + 0x1C0u), 0u);
        MemoryAccess.WriteU16(m, (c.S5 + 0x19Cu), (ushort)c.V0);
        goto L80014E24;
        L80014E18: ;
        if (c.S2 == 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), 0u);
            goto L80014E2C;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), 0u);
        L80014E20: ;
        c.A0 = 0u + 0u;
        L80014E24: ;
        c.RA = 0x80014E2Cu;
        GranTurismo2PC.func_80060840(c, m);
        L80014E2C: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x2Cu));
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
    public static void func_80014E60(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.S0 = c.A0 + 0u;
        c.RA = 0x80014E74u;
        GranTurismo2PC.func_8007FE8C(c, m);
        c.V0 = c.S0 + 0u;
        c.V1 = 0x80020000u;
        c.V1 = c.V1 + 0x2C5Cu;
        MemoryAccess.WriteU32(m, c.V0, c.V1);
        c.V1 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.V0 + 0x21u), (byte)c.V1);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80014E9C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x2C5Cu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU32(m, c.A0, c.V0);
        c.RA = 0x80014EB4u;
        GranTurismo2PC.func_8007FEC8(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80014EC4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.RA = 0x80014EDCu;
        GranTurismo2PC.func_800809B0(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x00000026u;
        c.RA = 0x80014EE8u;
        GranTurismo2PC.func_8007FF70(c, m);
        c.A0 = c.S0 + 0x58u;
        c.V0 = 0x80050000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8F0u));
        c.A2 = 0x00010000u;
        c.RA = 0x80014EFCu;
        GranTurismo2PC.func_80080494(c, m);
        c.S1 = c.S0 + 0x38u;
        c.A0 = c.S1 + 0u;
        c.A1 = 0x800C0000u;
        c.A1 = c.A1 + 0x1D10u;
        c.A2 = 0x00002340u;
        c.RA = 0x80014F14u;
        GranTurismo2PC.func_80080088(c, m);
        c.A0 = c.S1 + 0u;
        c.V0 = 0x00000020u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x70u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.S0 + 0x72u), (ushort)c.V0);
        c.RA = 0x80014F28u;
        GranTurismo2PC.func_80080100(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S0 + 0x70u;
        c.RA = 0x80014F34u;
        GranTurismo2PC.func_800800B4(c, m);
        c.V0 = 0x0000000Cu;
        MemoryAccess.WriteU16(m, (c.S0 + 0x80u), (ushort)c.V0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80014F50(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.S0 = c.A0 + 0u;
        c.RA = 0x80014F64u;
        GranTurismo2PC.func_80080C9C(c, m);
        c.V1 = MemoryAccess.ReadU16(m, (c.S0 + 0x80u));
        c.V1 = c.V1 - 0x1u;
        c.V0 = c.V1 << 16;
        c.V0 = (uint)((int)c.V0 >> 16);
        c.V0 = ~(0u | c.V0);
        MemoryAccess.WriteU16(m, (c.S0 + 0x80u), (ushort)c.V1);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = c.V0 >> 31;
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80014F94(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.S0 = c.A0 + 0u;
        c.RA = 0x80014FA8u;
        GranTurismo2PC.func_80080D04(c, m);
        c.A1 = 0x02200000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x78u));
        c.A1 = c.A1 | 0x2020u;
        c.RA = 0x80014FB8u;
        GranTurismo2PC.func_8007D024(c, m);
        c.V1 = 0x00000200u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x4u), (ushort)c.V1);
        c.V1 = 0x000001E0u;
        MemoryAccess.WriteU16(m, c.V0, (ushort)0u);
        MemoryAccess.WriteU16(m, (c.V0 + 0x2u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.V0 + 0x6u), (ushort)c.V1);
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x78u));
        c.A1 = 0x00000040u;
        c.RA = 0x80014FDCu;
        GranTurismo2PC.func_8007DA44(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80014FEC(CpuContext c, IMemory m)
    {
        c.V0 = 0x801C0000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x3050u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.V0 + 0x3050u;
        if (c.V1 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
            goto L80015028;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.RA = 0x80015010u;
        GranTurismo2PC.func_80080038(c, m);
        c.A1 = 0x80020000u;
        c.A2 = 0x801F0000u;
        c.A0 = c.S0 + 0u;
        c.A1 = c.A1 + 0x2CE8u;
        c.A2 = c.A2 + 0xCE0u;
        c.RA = 0x80015028u;
        GranTurismo2PC.func_80085B3C(c, m);
        L80015028: ;
        c.V0 = c.S0 + 0u;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001503C_gt2_overlay_4(CpuContext c, IMemory m)
    {
        c.V0 = 0x801C0000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x3060u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.V0 + 0x3060u;
        if (c.V1 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
            goto L80015078;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.RA = 0x80015060u;
        GranTurismo2PC.func_80080038(c, m);
        c.A1 = 0x80020000u;
        c.A2 = 0x801F0000u;
        c.A0 = c.S0 + 0u;
        c.A1 = c.A1 + 0x2CF8u;
        c.A2 = c.A2 + 0xCE0u;
        c.RA = 0x80015078u;
        GranTurismo2PC.func_80085B3C(c, m);
        L80015078: ;
        c.V0 = c.S0 + 0u;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001508C(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = c.V0 + 0x43E8u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80015098(CpuContext c, IMemory m)
    {
        c.V0 = 0x801D0000u;
        c.V0 = c.V0 - 0x6720u;
        c.A2 = c.V0 + 0x3C74u;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.A2 + 0x4018u));
        if ((int)c.V1 < 0) {
            c.V0 = 0x800B0000u;
            goto L80015100;
        }
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x448Cu), (ushort)c.A0);
        c.V0 = c.V1 << 2;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + 0x4u;
        c.V0 = c.V0 + c.A2;
        c.A0 = MemoryAccess.ReadU16(m, (c.V0 + 0x98u));
        c.V1 = 0x800B0000u;
        MemoryAccess.WriteU16(m, (c.V1 + 0x448Eu), (ushort)c.A1);
        c.V1 = 0x800B0000u;
        c.A0 = c.A0 & 0x3FFFu;
        MemoryAccess.WriteU16(m, (c.V1 + 0x43E0u), (ushort)c.A0);
        c.V0 = MemoryAccess.ReadU16(m, (c.V0 + 0x94u));
        c.V1 = c.V1 + 0x43E0u;
        MemoryAccess.WriteU8(m, (c.V1 + 0x4u), (byte)0u);
        c.V0 = c.V0 & 0x1FFFu;
        MemoryAccess.WriteU16(m, (c.V1 + 0x2u), (ushort)c.V0);
        L80015100: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80015108(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.S0 = c.A0 + 0u;
        c.RA = 0x8001511Cu;
        GranTurismo2PC.func_8007FE8C(c, m);
        c.V0 = c.S0 + 0u;
        c.V1 = 0x80020000u;
        c.V1 = c.V1 + 0x2D04u;
        MemoryAccess.WriteU32(m, c.V0, c.V1);
        c.V1 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.V0 + 0x21u), (byte)c.V1);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80015144(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x2D04u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU32(m, c.A0, c.V0);
        c.RA = 0x8001515Cu;
        GranTurismo2PC.func_8007FEC8(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001516C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.RA = 0x80015184u;
        GranTurismo2PC.func_800809B0(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = 0x00000064u;
        c.RA = 0x80015190u;
        GranTurismo2PC.func_8007FF70(c, m);
        c.A0 = c.S1 + 0x58u;
        c.A1 = 0x801A0000u;
        c.A1 = c.A1 + 0x5050u;
        c.A2 = 0u | 0x8C00u;
        c.RA = 0x800151A4u;
        GranTurismo2PC.func_80080494(c, m);
        c.S0 = c.S1 + 0x38u;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x800C0000u;
        c.A1 = c.A1 + 0x1D10u;
        c.A2 = 0x00002340u;
        c.RA = 0x800151BCu;
        GranTurismo2PC.func_80080088(c, m);
        c.A0 = c.S0 + 0u;
        c.V0 = 0x00000010u;
        MemoryAccess.WriteU16(m, (c.S1 + 0x74u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.S1 + 0x76u), (ushort)c.V0);
        c.RA = 0x800151D0u;
        GranTurismo2PC.func_80080100(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0x74u;
        c.RA = 0x800151DCu;
        GranTurismo2PC.func_800800B4(c, m);
        c.S0 = 0x801D0000u;
        c.S0 = c.S0 - 0x6720u;
        c.V0 = 0u | 0xC6C0u;
        c.S0 = c.S0 + c.V0;
        c.V0 = 0xFFFFFFFCu;
        c.S0 = c.S0 & c.V0;
        c.A0 = c.S0 + 0u;
        c.RA = 0x800151FCu;
        GranTurismo2PC.func_80019FE8(c, m);
        c.A0 = 0x80050000u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x232Au));
        c.A0 = c.A0 + 0xA78u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x10u), c.V0);
        c.RA = 0x80015210u;
        GranTurismo2PC.func_8006C04C(c, m);
        MemoryAccess.WriteU16(m, (c.S1 + 0x70u), (ushort)0u);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80015228(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S0);
        c.RA = 0x80015240u;
        GranTurismo2PC.func_80080C9C(c, m);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x70u));
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L800152CC;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L80015268;
        }
        c.V0 = 0x00000002u;
        if (c.V1 == 0u) {
            c.V0 = 0x00000001u;
            goto L80015278;
        }
        c.V0 = 0x00000001u;
        goto L80015344;
        L80015268: ;
        if (c.V1 == c.V0) {
            c.A0 = 0x801D0000u;
            goto L800152FC;
        }
        c.A0 = 0x801D0000u;
        c.V0 = 0x00000001u;
        goto L80015344;
        L80015278: ;
        c.V0 = 0x801D0000u;
        c.V0 = c.V0 - 0x6720u;
        c.V1 = 0u | 0xC6C0u;
        c.V0 = c.V0 + c.V1;
        c.V1 = 0xFFFFFFFCu;
        c.S0 = c.V0 & c.V1;
        c.A0 = c.S0 + 0u;
        c.RA = 0x80015298u;
        GranTurismo2PC.func_8001A000(c, m);
        if (c.V0 == 0u) {
            c.A0 = 0x80050000u;
            goto L800152B0;
        }
        c.A0 = 0x80050000u;
        c.V0 = MemoryAccess.ReadU16(m, (c.S1 + 0x70u));
        c.V0 = c.V0 + 0x1u;
        MemoryAccess.WriteU16(m, (c.S1 + 0x70u), (ushort)c.V0);
        L800152B0: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x232Au));
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x2328u));
        c.A0 = c.A0 + 0xA78u;
        c.A1 = c.V0 - c.A1;
        c.RA = 0x800152C4u;
        GranTurismo2PC.func_8006C074(c, m);
        c.V0 = 0x00000001u;
        goto L80015344;
        L800152CC: ;
        c.A0 = 0x801D0000u;
        c.A0 = c.A0 - 0x6720u;
        c.V0 = 0u | 0xC6C0u;
        c.A0 = c.A0 + c.V0;
        c.V0 = 0xFFFFFFFCu;
        c.A0 = c.A0 & c.V0;
        c.RA = 0x800152E8u;
        GranTurismo2PC.func_8001A1B4(c, m);
        c.V0 = MemoryAccess.ReadU16(m, (c.S1 + 0x70u));
        c.V0 = c.V0 + 0x1u;
        MemoryAccess.WriteU16(m, (c.S1 + 0x70u), (ushort)c.V0);
        goto L80015340;
        L800152FC: ;
        c.A0 = c.A0 - 0x6720u;
        c.V0 = 0u | 0xC6C0u;
        c.A0 = c.A0 + c.V0;
        c.V0 = 0xFFFFFFFCu;
        c.A0 = c.A0 & c.V0;
        c.A1 = 0x800B0000u;
        c.V0 = 0x800B0000u;
        c.A1 = c.A1 + 0x43E0u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x448Eu));
        c.A2 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.V0 = 0x800B0000u;
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x448Cu));
        c.A2 = c.A2 + 0x43E8u;
        c.RA = 0x80015338u;
        GranTurismo2PC.func_8001A24C(c, m);
        c.V0 = 0u + 0u;
        goto L80015344;
        L80015340: ;
        c.V0 = 0x00000001u;
        L80015344: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80015358(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.A0 + 0x7Cu));
        c.RA = 0x80015370u;
        GranTurismo2PC.func_80080D04(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 + 0xA78u;
        c.A1 = c.S0 + 0u;
        c.RA = 0x80015380u;
        GranTurismo2PC.func_8006C174(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0u + 0u;
        c.RA = 0x8001538Cu;
        GranTurismo2PC.func_8007D024(c, m);
        c.V1 = 0x00000200u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x4u), (ushort)c.V1);
        c.V1 = 0x000001E0u;
        MemoryAccess.WriteU16(m, c.V0, (ushort)0u);
        MemoryAccess.WriteU16(m, (c.V0 + 0x2u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.V0 + 0x6u), (ushort)c.V1);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800153B4(CpuContext c, IMemory m)
    {
        c.V0 = 0x801C0000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x3070u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.V0 + 0x3070u;
        if (c.V1 != 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
            goto L800153F0;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.RA = 0x800153D8u;
        GranTurismo2PC.func_80080038(c, m);
        c.A1 = 0x80020000u;
        c.A2 = 0x801F0000u;
        c.A0 = c.S0 + 0u;
        c.A1 = c.A1 + 0x2D48u;
        c.A2 = c.A2 + 0xCE0u;
        c.RA = 0x800153F0u;
        GranTurismo2PC.func_80085B3C(c, m);
        L800153F0: ;
        c.V0 = c.S0 + 0u;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80015404(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.A1 = 0xFFFFFFFFu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A2 = 0x000017E8u;
        c.RA = 0x80015418u;
        GranTurismo2PC.func_8008CE30(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80015428(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0xC8u;
        MemoryAccess.WriteU32(m, (c.SP + 0xB0u), c.S4);
        c.S4 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0xCCu), c.A1);
        c.A0 = c.A1 + 0u;
        c.A1 = c.SP + 0x10u;
        MemoryAccess.WriteU32(m, (c.SP + 0xC4u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0xC0u), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0xBCu), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0xB8u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0xB4u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0xACu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0xA8u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0xA4u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0xA0u), c.S0);
        c.RA = 0x80015468u;
        GranTurismo2PC.func_80076954(c, m);
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x22u));
        c.A0 = 0x00000012u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80015478u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0x8u));
        c.A1 = c.V1 + 0x40u;
        c.V0 = c.A0 << 2;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 - c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.S4 + c.V0;
        c.V0 = c.V0 + 0x988u;
        L800154A0: ;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, c.V0, c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x4u), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x8u), c.T1);
        MemoryAccess.WriteU32(m, (c.V0 + 0xCu), c.T2);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.A1) {
            c.V0 = c.V0 + 0x10u;
            goto L800154A0;
        }
        c.V0 = c.V0 + 0x10u;
        c.S2 = 0u + 0u;
        c.S3 = 0x00000004u;
        c.S0 = 0x0000004Cu;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        MemoryAccess.WriteU32(m, c.V0, c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x4u), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x8u), c.T1);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0xAB8u), c.S1);
        L800154FC: ;
        c.A0 = c.S2 + 0x23u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x8001550Cu;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x00000012u;
            goto L80015578;
        }
        c.A0 = 0x00000012u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80015520u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S0;
        c.V1 = c.V1 + 0x988u;
        c.A0 = c.V0 + 0x40u;
        L8001552C: ;
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
            goto L8001552C;
        }
        c.V1 = c.V1 + 0x10u;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU32(m, c.V1, c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x4u), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x8u), c.T1);
        c.V0 = c.S4 + c.S3;
        MemoryAccess.WriteU32(m, (c.V0 + 0xAB8u), c.S1);
        L80015578: ;
        c.S3 = c.S3 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        c.V0 = (int)c.S2 < 3 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S0 = c.S0 + 0x4Cu;
            goto L800154FC;
        }
        c.S0 = c.S0 + 0x4Cu;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x14u));
        c.A0 = 0u + 0u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x8001559Cu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0x8u));
        c.S2 = 0u + 0u;
        c.V0 = c.A0 << 1;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.S4 + c.V0;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V0 + 0xAC8u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0xACCu), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0xAD0u), c.T1);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0xAE0u), c.S1);
        c.S0 = c.S2 + 0x1u;
        L800155E0: ;
        c.A0 = c.S0 + 0u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x800155F0u;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0u + 0u;
            goto L80015638;
        }
        c.A0 = 0u + 0u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80015604u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S0 << 1;
        c.V1 = c.V1 + c.S0;
        c.V1 = c.V1 << 2;
        c.V1 = c.S4 + c.V1;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V1 + 0xAC8u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0xACCu), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0xAD0u), c.T1);
        c.V0 = c.S0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0xAE0u), c.S1);
        L80015638: ;
        c.S2 = c.S0 + 0u;
        if ((int)c.S2 <= 0) {
            c.S0 = c.S2 + 0x1u;
            goto L800155E0;
        }
        c.S0 = c.S2 + 0x1u;
        c.A0 = 0x00000001u;
        c.S2 = 0u + 0u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x16u));
        c.S3 = 0x00000004u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x8001565Cu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0x8u));
        c.S0 = 0x00000010u;
        c.V0 = c.A0 << (int)(c.S3 & 31u);
        c.V0 = c.S4 + c.V0;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, (c.V0 + 0xAE8u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0xAECu), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0xAF0u), c.T1);
        MemoryAccess.WriteU32(m, (c.V0 + 0xAF4u), c.T2);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0xB08u), c.S1);
        L8001569C: ;
        c.A0 = c.S2 + 0x2u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x800156ACu;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x00000001u;
            goto L800156EC;
        }
        c.A0 = 0x00000001u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x800156C0u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S0;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V0 + 0xCu));
        MemoryAccess.WriteU32(m, (c.V1 + 0xAE8u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0xAECu), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0xAF0u), c.T1);
        MemoryAccess.WriteU32(m, (c.V1 + 0xAF4u), c.T2);
        c.V0 = c.S4 + c.S3;
        MemoryAccess.WriteU32(m, (c.V0 + 0xB08u), c.S1);
        L800156EC: ;
        c.S3 = c.S3 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        if ((int)c.S2 <= 0) {
            c.S0 = c.S0 + 0x10u;
            goto L8001569C;
        }
        c.S0 = c.S0 + 0x10u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x26u));
        c.A0 = 0x00000016u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x8001570Cu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.S3 = c.V0 + 0u;
        c.A3 = MemoryAccess.ReadU32(m, c.S3);
        c.T0 = MemoryAccess.ReadU32(m, (c.S3 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.S3 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.S3 + 0xCu));
        MemoryAccess.WriteU32(m, (c.S4 + 0xB10u), c.A3);
        MemoryAccess.WriteU32(m, (c.S4 + 0xB14u), c.T0);
        MemoryAccess.WriteU32(m, (c.S4 + 0xB18u), c.T1);
        MemoryAccess.WriteU32(m, (c.S4 + 0xB1Cu), c.T2);
        MemoryAccess.WriteU32(m, (c.S4 + 0xBB0u), c.S1);
        c.A3 = MemoryAccess.ReadU32(m, c.S3);
        c.T0 = MemoryAccess.ReadU32(m, (c.S3 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.S3 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.S3 + 0xCu));
        MemoryAccess.WriteU32(m, (c.S4 + 0xB90u), c.A3);
        MemoryAccess.WriteU32(m, (c.S4 + 0xB94u), c.T0);
        MemoryAccess.WriteU32(m, (c.S4 + 0xB98u), c.T1);
        MemoryAccess.WriteU32(m, (c.S4 + 0xB9Cu), c.T2);
        MemoryAccess.WriteU32(m, (c.S4 + 0xBD0u), c.S1);
        c.A3 = MemoryAccess.ReadU32(m, c.S3);
        c.T0 = MemoryAccess.ReadU32(m, (c.S3 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.S3 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.S3 + 0xCu));
        MemoryAccess.WriteU32(m, (c.S4 + 0xBA0u), c.A3);
        MemoryAccess.WriteU32(m, (c.S4 + 0xBA4u), c.T0);
        MemoryAccess.WriteU32(m, (c.S4 + 0xBA8u), c.T1);
        MemoryAccess.WriteU32(m, (c.S4 + 0xBACu), c.T2);
        MemoryAccess.WriteU32(m, (c.S4 + 0xBD4u), c.S1);
        c.A1 = MemoryAccess.ReadU16(m, (c.S3 + 0xAu));
        c.A0 = 0x00000018u;
        c.RA = 0x80015788u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V0 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.V0);
        MemoryAccess.WriteWordLeft(m, (c.S4 + 0xBDBu), c.A3);
        MemoryAccess.WriteWordRight(m, (c.S4 + 0xBD8u), c.A3);
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V0 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.V0);
        MemoryAccess.WriteWordLeft(m, (c.S4 + 0xBFBu), c.A3);
        MemoryAccess.WriteWordRight(m, (c.S4 + 0xBF8u), c.A3);
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V0 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.V0);
        MemoryAccess.WriteWordLeft(m, (c.S4 + 0xBFFu), c.A3);
        MemoryAccess.WriteWordRight(m, (c.S4 + 0xBFCu), c.A3);
        c.A1 = MemoryAccess.ReadU16(m, (c.S3 + 0xCu));
        c.A0 = 0x00000019u;
        c.RA = 0x800157D0u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.A1 = c.S4 + 0xC00u;
        c.V1 = c.V0 + 0u;
        c.V0 = c.V1 | c.A1;
        c.V0 = c.V0 & 0x0003u;
        if (c.V0 == 0u) {
            c.A0 = c.V1 + 0u;
            goto L80015840;
        }
        c.A0 = c.V1 + 0u;
        c.V0 = c.V1 + 0x40u;
        L800157EC: ;
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.A0 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.A0);
        c.T0 = MemoryAccess.ReadWordLeft(m, c.T0, (c.A0 + 0x7u));
        c.T0 = MemoryAccess.ReadWordRight(m, c.T0, (c.A0 + 0x4u));
        c.T1 = MemoryAccess.ReadWordLeft(m, c.T1, (c.A0 + 0xBu));
        c.T1 = MemoryAccess.ReadWordRight(m, c.T1, (c.A0 + 0x8u));
        c.T2 = MemoryAccess.ReadWordLeft(m, c.T2, (c.A0 + 0xFu));
        c.T2 = MemoryAccess.ReadWordRight(m, c.T2, (c.A0 + 0xCu));
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0x3u), c.A3);
        MemoryAccess.WriteWordRight(m, c.A1, c.A3);
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0x7u), c.T0);
        MemoryAccess.WriteWordRight(m, (c.A1 + 0x4u), c.T0);
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0xBu), c.T1);
        MemoryAccess.WriteWordRight(m, (c.A1 + 0x8u), c.T1);
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0xFu), c.T2);
        MemoryAccess.WriteWordRight(m, (c.A1 + 0xCu), c.T2);
        c.A0 = c.A0 + 0x10u;
        if (c.A0 != c.V0) {
            c.A1 = c.A1 + 0x10u;
            goto L800157EC;
        }
        c.A1 = c.A1 + 0x10u;
        c.A0 = 0x0000001Au;
        goto L80015874;
        L80015840: ;
        c.V0 = c.V1 + 0x40u;
        L80015844: ;
        c.A3 = MemoryAccess.ReadU32(m, c.A0);
        c.T0 = MemoryAccess.ReadU32(m, (c.A0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.A0 + 0xCu));
        MemoryAccess.WriteU32(m, c.A1, c.A3);
        MemoryAccess.WriteU32(m, (c.A1 + 0x4u), c.T0);
        MemoryAccess.WriteU32(m, (c.A1 + 0x8u), c.T1);
        MemoryAccess.WriteU32(m, (c.A1 + 0xCu), c.T2);
        c.A0 = c.A0 + 0x10u;
        if (c.A0 != c.V0) {
            c.A1 = c.A1 + 0x10u;
            goto L80015844;
        }
        c.A1 = c.A1 + 0x10u;
        c.A0 = 0x0000001Au;
        L80015874: ;
        c.S2 = 0u + 0u;
        c.FP = 0x00000008u;
        c.S7 = 0x00000040u;
        c.S6 = 0x00000004u;
        c.A1 = MemoryAccess.ReadU16(m, (c.S3 + 0xEu));
        c.S5 = 0x00000010u;
        c.RA = 0x80015890u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V0 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.V0);
        c.T0 = MemoryAccess.ReadWordLeft(m, c.T0, (c.V0 + 0x7u));
        c.T0 = MemoryAccess.ReadWordRight(m, c.T0, (c.V0 + 0x4u));
        MemoryAccess.WriteWordLeft(m, (c.S4 + 0xE83u), c.A3);
        MemoryAccess.WriteWordRight(m, (c.S4 + 0xE80u), c.A3);
        MemoryAccess.WriteWordLeft(m, (c.S4 + 0xE87u), c.T0);
        MemoryAccess.WriteWordRight(m, (c.S4 + 0xE84u), c.T0);
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V0 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.V0);
        c.T0 = MemoryAccess.ReadWordLeft(m, c.T0, (c.V0 + 0x7u));
        c.T0 = MemoryAccess.ReadWordRight(m, c.T0, (c.V0 + 0x4u));
        MemoryAccess.WriteWordLeft(m, (c.S4 + 0xEC3u), c.A3);
        MemoryAccess.WriteWordRight(m, (c.S4 + 0xEC0u), c.A3);
        MemoryAccess.WriteWordLeft(m, (c.S4 + 0xEC7u), c.T0);
        MemoryAccess.WriteWordRight(m, (c.S4 + 0xEC4u), c.T0);
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V0 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.V0);
        c.T0 = MemoryAccess.ReadWordLeft(m, c.T0, (c.V0 + 0x7u));
        c.T0 = MemoryAccess.ReadWordRight(m, c.T0, (c.V0 + 0x4u));
        MemoryAccess.WriteWordLeft(m, (c.S4 + 0xECBu), c.A3);
        MemoryAccess.WriteWordRight(m, (c.S4 + 0xEC8u), c.A3);
        MemoryAccess.WriteWordLeft(m, (c.S4 + 0xECFu), c.T0);
        MemoryAccess.WriteWordRight(m, (c.S4 + 0xECCu), c.T0);
        L800158F0: ;
        c.A0 = c.S2 + 0x27u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x80015900u;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            goto L80015A40;
        }
        c.A0 = 0x00000016u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80015918u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.S3 = c.V0 + 0u;
        c.V0 = c.S4 + c.S5;
        c.S0 = c.S4 + c.S6;
        c.A3 = MemoryAccess.ReadU32(m, c.S3);
        c.T0 = MemoryAccess.ReadU32(m, (c.S3 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.S3 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.S3 + 0xCu));
        MemoryAccess.WriteU32(m, (c.V0 + 0xB10u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0xB14u), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0xB18u), c.T1);
        MemoryAccess.WriteU32(m, (c.V0 + 0xB1Cu), c.T2);
        MemoryAccess.WriteU32(m, (c.S0 + 0xBB0u), c.S1);
        c.A1 = MemoryAccess.ReadU16(m, (c.S3 + 0xAu));
        c.A0 = 0x00000018u;
        c.RA = 0x80015954u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V0 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.V0);
        MemoryAccess.WriteWordLeft(m, (c.S0 + 0xBDBu), c.A3);
        MemoryAccess.WriteWordRight(m, (c.S0 + 0xBD8u), c.A3);
        c.A1 = MemoryAccess.ReadU16(m, (c.S3 + 0xCu));
        c.A0 = 0x00000019u;
        c.RA = 0x80015974u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S7;
        c.V1 = c.V1 + 0xC00u;
        c.A0 = c.V0 + 0u;
        c.V0 = c.A0 | c.V1;
        c.V0 = c.V0 & 0x0003u;
        if (c.V0 == 0u) {
            c.V0 = c.A0 + 0x40u;
            goto L800159E4;
        }
        c.V0 = c.A0 + 0x40u;
        L80015990: ;
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.A0 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.A0);
        c.T0 = MemoryAccess.ReadWordLeft(m, c.T0, (c.A0 + 0x7u));
        c.T0 = MemoryAccess.ReadWordRight(m, c.T0, (c.A0 + 0x4u));
        c.T1 = MemoryAccess.ReadWordLeft(m, c.T1, (c.A0 + 0xBu));
        c.T1 = MemoryAccess.ReadWordRight(m, c.T1, (c.A0 + 0x8u));
        c.T2 = MemoryAccess.ReadWordLeft(m, c.T2, (c.A0 + 0xFu));
        c.T2 = MemoryAccess.ReadWordRight(m, c.T2, (c.A0 + 0xCu));
        MemoryAccess.WriteWordLeft(m, (c.V1 + 0x3u), c.A3);
        MemoryAccess.WriteWordRight(m, c.V1, c.A3);
        MemoryAccess.WriteWordLeft(m, (c.V1 + 0x7u), c.T0);
        MemoryAccess.WriteWordRight(m, (c.V1 + 0x4u), c.T0);
        MemoryAccess.WriteWordLeft(m, (c.V1 + 0xBu), c.T1);
        MemoryAccess.WriteWordRight(m, (c.V1 + 0x8u), c.T1);
        MemoryAccess.WriteWordLeft(m, (c.V1 + 0xFu), c.T2);
        MemoryAccess.WriteWordRight(m, (c.V1 + 0xCu), c.T2);
        c.A0 = c.A0 + 0x10u;
        if (c.A0 != c.V0) {
            c.V1 = c.V1 + 0x10u;
            goto L80015990;
        }
        c.V1 = c.V1 + 0x10u;
        goto L80015A10;
        L800159E4: ;
        c.A3 = MemoryAccess.ReadU32(m, c.A0);
        c.T0 = MemoryAccess.ReadU32(m, (c.A0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.A0 + 0xCu));
        MemoryAccess.WriteU32(m, c.V1, c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x4u), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x8u), c.T1);
        MemoryAccess.WriteU32(m, (c.V1 + 0xCu), c.T2);
        c.A0 = c.A0 + 0x10u;
        if (c.A0 != c.V0) {
            c.V1 = c.V1 + 0x10u;
            goto L800159E4;
        }
        c.V1 = c.V1 + 0x10u;
        L80015A10: ;
        c.A1 = MemoryAccess.ReadU16(m, (c.S3 + 0xEu));
        c.A0 = 0x0000001Au;
        c.RA = 0x80015A1Cu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.FP;
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V0 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.V0);
        c.T0 = MemoryAccess.ReadWordLeft(m, c.T0, (c.V0 + 0x7u));
        c.T0 = MemoryAccess.ReadWordRight(m, c.T0, (c.V0 + 0x4u));
        MemoryAccess.WriteWordLeft(m, (c.V1 + 0xE83u), c.A3);
        MemoryAccess.WriteWordRight(m, (c.V1 + 0xE80u), c.A3);
        MemoryAccess.WriteWordLeft(m, (c.V1 + 0xE87u), c.T0);
        MemoryAccess.WriteWordRight(m, (c.V1 + 0xE84u), c.T0);
        L80015A40: ;
        c.FP = c.FP + 0x8u;
        c.S7 = c.S7 + 0x40u;
        c.S6 = c.S6 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        c.V0 = (int)c.S2 < 7 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S5 = c.S5 + 0x10u;
            goto L800158F0;
        }
        c.S5 = c.S5 + 0x10u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x28u));
        c.A0 = 0x00000017u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80015A6Cu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.S3 = c.V0 + 0u;
        c.A3 = MemoryAccess.ReadU32(m, c.S3);
        c.T0 = MemoryAccess.ReadU32(m, (c.S3 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.S3 + 0x8u));
        MemoryAccess.WriteU32(m, (c.S4 + 0xED0u), c.A3);
        MemoryAccess.WriteU32(m, (c.S4 + 0xED4u), c.T0);
        MemoryAccess.WriteU32(m, (c.S4 + 0xED8u), c.T1);
        MemoryAccess.WriteU32(m, (c.S4 + 0xF48u), c.S1);
        c.A3 = MemoryAccess.ReadU32(m, c.S3);
        c.T0 = MemoryAccess.ReadU32(m, (c.S3 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.S3 + 0x8u));
        MemoryAccess.WriteU32(m, (c.S4 + 0xF30u), c.A3);
        MemoryAccess.WriteU32(m, (c.S4 + 0xF34u), c.T0);
        MemoryAccess.WriteU32(m, (c.S4 + 0xF38u), c.T1);
        MemoryAccess.WriteU32(m, (c.S4 + 0xF68u), c.S1);
        c.A3 = MemoryAccess.ReadU32(m, c.S3);
        c.T0 = MemoryAccess.ReadU32(m, (c.S3 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.S3 + 0x8u));
        MemoryAccess.WriteU32(m, (c.S4 + 0xF3Cu), c.A3);
        MemoryAccess.WriteU32(m, (c.S4 + 0xF40u), c.T0);
        MemoryAccess.WriteU32(m, (c.S4 + 0xF44u), c.T1);
        MemoryAccess.WriteU32(m, (c.S4 + 0xF6Cu), c.S1);
        c.A1 = MemoryAccess.ReadU16(m, (c.S3 + 0x6u));
        c.A0 = 0x00000018u;
        c.RA = 0x80015AD0u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V0 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.V0);
        MemoryAccess.WriteWordLeft(m, (c.S4 + 0xF73u), c.A3);
        MemoryAccess.WriteWordRight(m, (c.S4 + 0xF70u), c.A3);
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V0 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.V0);
        MemoryAccess.WriteWordLeft(m, (c.S4 + 0xF93u), c.A3);
        MemoryAccess.WriteWordRight(m, (c.S4 + 0xF90u), c.A3);
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V0 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.V0);
        MemoryAccess.WriteWordLeft(m, (c.S4 + 0xF97u), c.A3);
        MemoryAccess.WriteWordRight(m, (c.S4 + 0xF94u), c.A3);
        c.A1 = MemoryAccess.ReadU16(m, (c.S3 + 0x8u));
        c.A0 = 0x00000019u;
        c.RA = 0x80015B18u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.A1 = c.S4 + 0xF98u;
        c.A0 = c.V0 + 0u;
        c.V0 = c.A0 | c.A1;
        c.V0 = c.V0 & 0x0003u;
        if (c.V0 == 0u) {
            c.V1 = c.A0 + 0u;
            goto L80015B88;
        }
        c.V1 = c.A0 + 0u;
        c.V0 = c.A0 + 0x40u;
        L80015B34: ;
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V1 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.V1);
        c.T0 = MemoryAccess.ReadWordLeft(m, c.T0, (c.V1 + 0x7u));
        c.T0 = MemoryAccess.ReadWordRight(m, c.T0, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadWordLeft(m, c.T1, (c.V1 + 0xBu));
        c.T1 = MemoryAccess.ReadWordRight(m, c.T1, (c.V1 + 0x8u));
        c.T2 = MemoryAccess.ReadWordLeft(m, c.T2, (c.V1 + 0xFu));
        c.T2 = MemoryAccess.ReadWordRight(m, c.T2, (c.V1 + 0xCu));
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0x3u), c.A3);
        MemoryAccess.WriteWordRight(m, c.A1, c.A3);
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0x7u), c.T0);
        MemoryAccess.WriteWordRight(m, (c.A1 + 0x4u), c.T0);
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0xBu), c.T1);
        MemoryAccess.WriteWordRight(m, (c.A1 + 0x8u), c.T1);
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0xFu), c.T2);
        MemoryAccess.WriteWordRight(m, (c.A1 + 0xCu), c.T2);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.V0) {
            c.A1 = c.A1 + 0x10u;
            goto L80015B34;
        }
        c.A1 = c.A1 + 0x10u;
        c.A1 = c.S4 + 0x1198u;
        goto L80015BBC;
        L80015B88: ;
        c.V0 = c.A0 + 0x40u;
        L80015B8C: ;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, c.A1, c.A3);
        MemoryAccess.WriteU32(m, (c.A1 + 0x4u), c.T0);
        MemoryAccess.WriteU32(m, (c.A1 + 0x8u), c.T1);
        MemoryAccess.WriteU32(m, (c.A1 + 0xCu), c.T2);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.V0) {
            c.A1 = c.A1 + 0x10u;
            goto L80015B8C;
        }
        c.A1 = c.A1 + 0x10u;
        c.A1 = c.S4 + 0x1198u;
        L80015BBC: ;
        c.V0 = c.A0 | c.A1;
        c.V0 = c.V0 & 0x0003u;
        if (c.V0 == 0u) {
            c.V1 = c.A0 + 0u;
            goto L80015C24;
        }
        c.V1 = c.A0 + 0u;
        c.V0 = c.A0 + 0x40u;
        L80015BD0: ;
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V1 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.V1);
        c.T0 = MemoryAccess.ReadWordLeft(m, c.T0, (c.V1 + 0x7u));
        c.T0 = MemoryAccess.ReadWordRight(m, c.T0, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadWordLeft(m, c.T1, (c.V1 + 0xBu));
        c.T1 = MemoryAccess.ReadWordRight(m, c.T1, (c.V1 + 0x8u));
        c.T2 = MemoryAccess.ReadWordLeft(m, c.T2, (c.V1 + 0xFu));
        c.T2 = MemoryAccess.ReadWordRight(m, c.T2, (c.V1 + 0xCu));
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0x3u), c.A3);
        MemoryAccess.WriteWordRight(m, c.A1, c.A3);
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0x7u), c.T0);
        MemoryAccess.WriteWordRight(m, (c.A1 + 0x4u), c.T0);
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0xBu), c.T1);
        MemoryAccess.WriteWordRight(m, (c.A1 + 0x8u), c.T1);
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0xFu), c.T2);
        MemoryAccess.WriteWordRight(m, (c.A1 + 0xCu), c.T2);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.V0) {
            c.A1 = c.A1 + 0x10u;
            goto L80015BD0;
        }
        c.A1 = c.A1 + 0x10u;
        c.A1 = c.S4 + 0x11D8u;
        goto L80015C58;
        L80015C24: ;
        c.V0 = c.A0 + 0x40u;
        L80015C28: ;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, c.A1, c.A3);
        MemoryAccess.WriteU32(m, (c.A1 + 0x4u), c.T0);
        MemoryAccess.WriteU32(m, (c.A1 + 0x8u), c.T1);
        MemoryAccess.WriteU32(m, (c.A1 + 0xCu), c.T2);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.V0) {
            c.A1 = c.A1 + 0x10u;
            goto L80015C28;
        }
        c.A1 = c.A1 + 0x10u;
        c.A1 = c.S4 + 0x11D8u;
        L80015C58: ;
        c.V0 = c.A0 | c.A1;
        c.V0 = c.V0 & 0x0003u;
        if (c.V0 == 0u) {
            c.V1 = c.A0 + 0u;
            goto L80015CC0;
        }
        c.V1 = c.A0 + 0u;
        c.V0 = c.A0 + 0x40u;
        L80015C6C: ;
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V1 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.V1);
        c.T0 = MemoryAccess.ReadWordLeft(m, c.T0, (c.V1 + 0x7u));
        c.T0 = MemoryAccess.ReadWordRight(m, c.T0, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadWordLeft(m, c.T1, (c.V1 + 0xBu));
        c.T1 = MemoryAccess.ReadWordRight(m, c.T1, (c.V1 + 0x8u));
        c.T2 = MemoryAccess.ReadWordLeft(m, c.T2, (c.V1 + 0xFu));
        c.T2 = MemoryAccess.ReadWordRight(m, c.T2, (c.V1 + 0xCu));
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0x3u), c.A3);
        MemoryAccess.WriteWordRight(m, c.A1, c.A3);
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0x7u), c.T0);
        MemoryAccess.WriteWordRight(m, (c.A1 + 0x4u), c.T0);
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0xBu), c.T1);
        MemoryAccess.WriteWordRight(m, (c.A1 + 0x8u), c.T1);
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0xFu), c.T2);
        MemoryAccess.WriteWordRight(m, (c.A1 + 0xCu), c.T2);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.V0) {
            c.A1 = c.A1 + 0x10u;
            goto L80015C6C;
        }
        c.A1 = c.A1 + 0x10u;
        c.S2 = 0u + 0u;
        goto L80015CF4;
        L80015CC0: ;
        c.V0 = c.A0 + 0x40u;
        L80015CC4: ;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, c.A1, c.A3);
        MemoryAccess.WriteU32(m, (c.A1 + 0x4u), c.T0);
        MemoryAccess.WriteU32(m, (c.A1 + 0x8u), c.T1);
        MemoryAccess.WriteU32(m, (c.A1 + 0xCu), c.T2);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.V0) {
            c.A1 = c.A1 + 0x10u;
            goto L80015CC4;
        }
        c.A1 = c.A1 + 0x10u;
        c.S2 = 0u + 0u;
        L80015CF4: ;
        c.S7 = 0x00000040u;
        c.S6 = 0x00000004u;
        c.S5 = 0x0000000Cu;
        L80015D00: ;
        c.A0 = c.S2 + 0x27u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x80015D10u;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x00000017u;
            goto L80015E14;
        }
        c.A0 = 0x00000017u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80015D24u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.S3 = c.V0 + 0u;
        c.V0 = c.S4 + c.S5;
        c.S0 = c.S4 + c.S6;
        c.A3 = MemoryAccess.ReadU32(m, c.S3);
        c.T0 = MemoryAccess.ReadU32(m, (c.S3 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.S3 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V0 + 0xED0u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0xED4u), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0xED8u), c.T1);
        MemoryAccess.WriteU32(m, (c.S0 + 0xF48u), c.S1);
        c.A1 = MemoryAccess.ReadU16(m, (c.S3 + 0x6u));
        c.A0 = 0x00000018u;
        c.RA = 0x80015D58u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V0 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.V0);
        MemoryAccess.WriteWordLeft(m, (c.S0 + 0xF73u), c.A3);
        MemoryAccess.WriteWordRight(m, (c.S0 + 0xF70u), c.A3);
        c.A1 = MemoryAccess.ReadU16(m, (c.S3 + 0x8u));
        c.A0 = 0x00000019u;
        c.RA = 0x80015D78u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S7;
        c.V1 = c.V1 + 0xF98u;
        c.A0 = c.V0 + 0u;
        c.V0 = c.A0 | c.V1;
        c.V0 = c.V0 & 0x0003u;
        if (c.V0 == 0u) {
            c.V0 = c.A0 + 0x40u;
            goto L80015DE8;
        }
        c.V0 = c.A0 + 0x40u;
        L80015D94: ;
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.A0 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.A0);
        c.T0 = MemoryAccess.ReadWordLeft(m, c.T0, (c.A0 + 0x7u));
        c.T0 = MemoryAccess.ReadWordRight(m, c.T0, (c.A0 + 0x4u));
        c.T1 = MemoryAccess.ReadWordLeft(m, c.T1, (c.A0 + 0xBu));
        c.T1 = MemoryAccess.ReadWordRight(m, c.T1, (c.A0 + 0x8u));
        c.T2 = MemoryAccess.ReadWordLeft(m, c.T2, (c.A0 + 0xFu));
        c.T2 = MemoryAccess.ReadWordRight(m, c.T2, (c.A0 + 0xCu));
        MemoryAccess.WriteWordLeft(m, (c.V1 + 0x3u), c.A3);
        MemoryAccess.WriteWordRight(m, c.V1, c.A3);
        MemoryAccess.WriteWordLeft(m, (c.V1 + 0x7u), c.T0);
        MemoryAccess.WriteWordRight(m, (c.V1 + 0x4u), c.T0);
        MemoryAccess.WriteWordLeft(m, (c.V1 + 0xBu), c.T1);
        MemoryAccess.WriteWordRight(m, (c.V1 + 0x8u), c.T1);
        MemoryAccess.WriteWordLeft(m, (c.V1 + 0xFu), c.T2);
        MemoryAccess.WriteWordRight(m, (c.V1 + 0xCu), c.T2);
        c.A0 = c.A0 + 0x10u;
        if (c.A0 != c.V0) {
            c.V1 = c.V1 + 0x10u;
            goto L80015D94;
        }
        c.V1 = c.V1 + 0x10u;
        c.S7 = c.S7 + 0x40u;
        goto L80015E18;
        L80015DE8: ;
        c.A3 = MemoryAccess.ReadU32(m, c.A0);
        c.T0 = MemoryAccess.ReadU32(m, (c.A0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.A0 + 0xCu));
        MemoryAccess.WriteU32(m, c.V1, c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x4u), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x8u), c.T1);
        MemoryAccess.WriteU32(m, (c.V1 + 0xCu), c.T2);
        c.A0 = c.A0 + 0x10u;
        if (c.A0 != c.V0) {
            c.V1 = c.V1 + 0x10u;
            goto L80015DE8;
        }
        c.V1 = c.V1 + 0x10u;
        L80015E14: ;
        c.S7 = c.S7 + 0x40u;
        L80015E18: ;
        c.S6 = c.S6 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        c.V0 = (int)c.S2 < 7 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S5 = c.S5 + 0xCu;
            goto L80015D00;
        }
        c.S5 = c.S5 + 0xCu;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x20u));
        c.A0 = 0x00000011u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80015E3Cu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0x8u));
        c.A1 = c.V1 + 0x20u;
        c.V0 = c.A0 << 3;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.S4 + c.V0;
        c.V0 = c.V0 + 0x1218u;
        L80015E5C: ;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, c.V0, c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x4u), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x8u), c.T1);
        MemoryAccess.WriteU32(m, (c.V0 + 0xCu), c.T2);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.A1) {
            c.V0 = c.V0 + 0x10u;
            goto L80015E5C;
        }
        c.V0 = c.V0 + 0x10u;
        c.S2 = 0u + 0u;
        c.S3 = 0x00000004u;
        c.S0 = 0x00000024u;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        MemoryAccess.WriteU32(m, c.V0, c.A3);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0x12A8u), c.S1);
        L80015EAC: ;
        c.A0 = c.S2 + 0xDu;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x80015EBCu;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x00000011u;
            goto L80015F1C;
        }
        c.A0 = 0x00000011u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80015ED0u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S0;
        c.V1 = c.V1 + 0x1218u;
        c.A0 = c.V0 + 0x20u;
        L80015EDC: ;
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
            goto L80015EDC;
        }
        c.V1 = c.V1 + 0x10u;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        MemoryAccess.WriteU32(m, c.V1, c.A3);
        c.V0 = c.S4 + c.S3;
        MemoryAccess.WriteU32(m, (c.V0 + 0x12A8u), c.S1);
        L80015F1C: ;
        c.S3 = c.S3 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        c.V0 = (int)c.S2 < 3 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S0 = c.S0 + 0x24u;
            goto L80015EAC;
        }
        c.S0 = c.S0 + 0x24u;
        c.A0 = 0x0000000Fu;
        c.S2 = 0u + 0u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x3Cu));
        c.S3 = 0x00000004u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80015F48u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0x8u));
        c.S0 = 0x00000010u;
        c.V0 = c.A0 << (int)(c.S3 & 31u);
        c.V0 = c.S4 + c.V0;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, (c.V0 + 0x12B8u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x12BCu), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x12C0u), c.T1);
        MemoryAccess.WriteU32(m, (c.V0 + 0x12C4u), c.T2);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0x12F8u), c.S1);
        L80015F88: ;
        c.A0 = c.S2 + 0x3u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x80015F98u;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x0000000Fu;
            goto L80015FD8;
        }
        c.A0 = 0x0000000Fu;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80015FACu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S0;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V0 + 0xCu));
        MemoryAccess.WriteU32(m, (c.V1 + 0x12B8u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x12BCu), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x12C0u), c.T1);
        MemoryAccess.WriteU32(m, (c.V1 + 0x12C4u), c.T2);
        c.V0 = c.S4 + c.S3;
        MemoryAccess.WriteU32(m, (c.V0 + 0x12F8u), c.S1);
        L80015FD8: ;
        c.S3 = c.S3 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        c.V0 = (int)c.S2 < 3 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S0 = c.S0 + 0x10u;
            goto L80015F88;
        }
        c.S0 = c.S0 + 0x10u;
        c.A0 = 0x0000000Eu;
        c.S2 = 0u + 0u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x3Au));
        c.S3 = 0x00000004u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80016004u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0x8u));
        c.S0 = 0x0000000Cu;
        c.V0 = c.A0 << 1;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.S4 + c.V0;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V0 + 0x1308u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x130Cu), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1310u), c.T1);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1338u), c.S1);
        L80016044: ;
        c.A0 = c.S2 + 0xAu;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x80016054u;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x0000000Eu;
            goto L8001608C;
        }
        c.A0 = 0x0000000Eu;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80016068u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S0;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V1 + 0x1308u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x130Cu), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x1310u), c.T1);
        c.V0 = c.S4 + c.S3;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1338u), c.S1);
        L8001608C: ;
        c.S3 = c.S3 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        c.V0 = (int)c.S2 < 3 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S0 = c.S0 + 0xCu;
            goto L80016044;
        }
        c.S0 = c.S0 + 0xCu;
        c.A0 = 0x00000010u;
        c.S2 = 0u + 0u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x3Eu));
        c.S3 = 0x00000004u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x800160B8u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0x8u));
        c.S0 = 0x0000000Cu;
        c.V0 = c.A0 << 1;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.S4 + c.V0;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V0 + 0x1348u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x134Cu), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1350u), c.T1);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1360u), c.S1);
        L800160F8: ;
        c.A0 = c.S2 + 0x21u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x80016108u;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x00000010u;
            goto L80016140;
        }
        c.A0 = 0x00000010u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x8001611Cu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S0;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V1 + 0x1348u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x134Cu), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x1350u), c.T1);
        c.V0 = c.S4 + c.S3;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1360u), c.S1);
        L80016140: ;
        c.S3 = c.S3 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        if ((int)c.S2 <= 0) {
            c.S0 = c.S0 + 0xCu;
            goto L800160F8;
        }
        c.S0 = c.S0 + 0xCu;
        c.A0 = 0x0000000Au;
        c.S2 = 0u + 0u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x34u));
        c.S3 = 0x00000004u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80016168u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0x8u));
        c.S0 = 0x0000000Cu;
        c.V0 = c.A0 << 1;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.S4 + c.V0;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V0 + 0x1368u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x136Cu), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1370u), c.T1);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1380u), c.S1);
        L800161A8: ;
        c.A0 = c.S2 + 0x6u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x800161B8u;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x0000000Au;
            goto L800161F0;
        }
        c.A0 = 0x0000000Au;
        c.A1 = c.S1 + 0u;
        c.RA = 0x800161CCu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S0;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V1 + 0x1368u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x136Cu), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x1370u), c.T1);
        c.V0 = c.S4 + c.S3;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1380u), c.S1);
        L800161F0: ;
        c.S3 = c.S3 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        if ((int)c.S2 <= 0) {
            c.S0 = c.S0 + 0xCu;
            goto L800161A8;
        }
        c.S0 = c.S0 + 0xCu;
        c.A0 = 0x00000007u;
        c.S2 = 0u + 0u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x2Eu));
        c.S3 = 0x00000004u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80016218u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0x8u));
        c.S0 = 0x0000000Cu;
        c.V0 = c.A0 << 1;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.S4 + c.V0;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V0 + 0x1388u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x138Cu), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1390u), c.T1);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0x13A0u), c.S1);
        L80016258: ;
        c.A0 = c.S2 + 0x20u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x80016268u;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x00000007u;
            goto L800162A0;
        }
        c.A0 = 0x00000007u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x8001627Cu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S0;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V1 + 0x1388u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x138Cu), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x1390u), c.T1);
        c.V0 = c.S4 + c.S3;
        MemoryAccess.WriteU32(m, (c.V0 + 0x13A0u), c.S1);
        L800162A0: ;
        c.S3 = c.S3 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        if ((int)c.S2 <= 0) {
            c.S0 = c.S0 + 0xCu;
            goto L80016258;
        }
        c.S0 = c.S0 + 0xCu;
        c.A0 = 0x00000008u;
        c.S2 = 0u + 0u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x30u));
        c.S3 = 0x00000004u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x800162C8u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0x8u));
        c.S0 = 0x0000000Cu;
        c.V0 = c.A0 << 1;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.S4 + c.V0;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V0 + 0x13A8u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x13ACu), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x13B0u), c.T1);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0x13C0u), c.S1);
        L80016308: ;
        c.A0 = c.S2 + 0x9u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x80016318u;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x00000008u;
            goto L80016350;
        }
        c.A0 = 0x00000008u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x8001632Cu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S0;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V1 + 0x13A8u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x13ACu), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x13B0u), c.T1);
        c.V0 = c.S4 + c.S3;
        MemoryAccess.WriteU32(m, (c.V0 + 0x13C0u), c.S1);
        L80016350: ;
        c.S3 = c.S3 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        if ((int)c.S2 <= 0) {
            c.S0 = c.S0 + 0xCu;
            goto L80016308;
        }
        c.S0 = c.S0 + 0xCu;
        c.A0 = 0x0000000Bu;
        c.S2 = 0u + 0u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x36u));
        c.S3 = 0x00000004u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80016378u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0x8u));
        c.S0 = 0x0000000Cu;
        c.V0 = c.A0 << 1;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.S4 + c.V0;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V0 + 0x13C8u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x13CCu), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x13D0u), c.T1);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0x13F8u), c.S1);
        L800163B8: ;
        c.A0 = c.S2 + 0x1Du;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x800163C8u;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x0000000Bu;
            goto L80016400;
        }
        c.A0 = 0x0000000Bu;
        c.A1 = c.S1 + 0u;
        c.RA = 0x800163DCu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S0;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V1 + 0x13C8u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x13CCu), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x13D0u), c.T1);
        c.V0 = c.S4 + c.S3;
        MemoryAccess.WriteU32(m, (c.V0 + 0x13F8u), c.S1);
        L80016400: ;
        c.S3 = c.S3 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        c.V0 = (int)c.S2 < 3 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S0 = c.S0 + 0xCu;
            goto L800163B8;
        }
        c.S0 = c.S0 + 0xCu;
        c.A0 = 0x00000009u;
        c.S2 = 0u + 0u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x32u));
        c.S3 = 0x00000004u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x8001642Cu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0x8u));
        c.S0 = 0x0000000Cu;
        c.V0 = c.A0 << 1;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.S4 + c.V0;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V0 + 0x1408u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x140Cu), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1410u), c.T1);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1420u), c.S1);
        L8001646C: ;
        c.A0 = c.S2 + 0x7u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x8001647Cu;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x00000009u;
            goto L800164B4;
        }
        c.A0 = 0x00000009u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80016490u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S0;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V1 + 0x1408u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x140Cu), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x1410u), c.T1);
        c.V0 = c.S4 + c.S3;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1420u), c.S1);
        L800164B4: ;
        c.S3 = c.S3 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        if ((int)c.S2 <= 0) {
            c.S0 = c.S0 + 0xCu;
            goto L8001646C;
        }
        c.S0 = c.S0 + 0xCu;
        c.A0 = 0x0000000Cu;
        c.S2 = 0u + 0u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x38u));
        c.S3 = 0x00000014u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x800164DCu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.A1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.A1 + 0x8u));
        c.S0 = 0x00000004u;
        c.V1 = c.A0 << 2;
        c.V0 = c.V1 + c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.S4 + c.V0;
        c.V1 = c.S4 + c.V1;
        c.A3 = MemoryAccess.ReadU32(m, c.A1);
        c.T0 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.A1 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.A1 + 0xCu));
        MemoryAccess.WriteU32(m, (c.V0 + 0x1428u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x142Cu), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1430u), c.T1);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1434u), c.T2);
        c.A3 = MemoryAccess.ReadU32(m, (c.A1 + 0x10u));
        MemoryAccess.WriteU32(m, (c.V0 + 0x1438u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x148Cu), c.S1);
        L8001652C: ;
        c.A0 = c.S2 + 0x2Eu;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x8001653Cu;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x0000000Cu;
            goto L80016588;
        }
        c.A0 = 0x0000000Cu;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80016550u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S3;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V0 + 0xCu));
        MemoryAccess.WriteU32(m, (c.V1 + 0x1428u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x142Cu), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x1430u), c.T1);
        MemoryAccess.WriteU32(m, (c.V1 + 0x1434u), c.T2);
        c.A3 = MemoryAccess.ReadU32(m, (c.V0 + 0x10u));
        MemoryAccess.WriteU32(m, (c.V1 + 0x1438u), c.A3);
        c.V0 = c.S4 + c.S0;
        MemoryAccess.WriteU32(m, (c.V0 + 0x148Cu), c.S1);
        L80016588: ;
        c.S3 = c.S3 + 0x14u;
        c.S2 = c.S2 + 0x1u;
        c.V0 = (int)c.S2 < 4 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S0 = c.S0 + 0x4u;
            goto L8001652C;
        }
        c.S0 = c.S0 + 0x4u;
        c.A0 = 0x00000013u;
        c.S2 = 0u + 0u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x42u));
        c.S3 = 0x00000004u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x800165B4u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0x8u));
        c.S0 = 0x0000000Cu;
        c.V0 = c.A0 << 1;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.S4 + c.V0;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V0 + 0x14A0u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x14A4u), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x14A8u), c.T1);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0x14C4u), c.S1);
        L800165F4: ;
        c.A0 = c.S2 + 0x10u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x80016604u;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x00000013u;
            goto L8001663C;
        }
        c.A0 = 0x00000013u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80016618u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S0;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V1 + 0x14A0u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x14A4u), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x14A8u), c.T1);
        c.V0 = c.S4 + c.S3;
        MemoryAccess.WriteU32(m, (c.V0 + 0x14C4u), c.S1);
        L8001663C: ;
        c.S3 = c.S3 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        c.V0 = (int)c.S2 < 2 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S0 = c.S0 + 0xCu;
            goto L800165F4;
        }
        c.S0 = c.S0 + 0xCu;
        c.A0 = 0x00000014u;
        c.S2 = 0u + 0u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x40u));
        c.S3 = 0x00000004u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80016668u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0x8u));
        c.S0 = 0x0000000Cu;
        c.V0 = c.A0 << 1;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.S4 + c.V0;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V0 + 0x14D0u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x14D4u), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x14D8u), c.T1);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1500u), c.S1);
        L800166A8: ;
        c.A0 = c.S2 + 0x1Au;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x800166B8u;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x00000014u;
            goto L800166F0;
        }
        c.A0 = 0x00000014u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x800166CCu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S0;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V1 + 0x14D0u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x14D4u), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x14D8u), c.T1);
        c.V0 = c.S4 + c.S3;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1500u), c.S1);
        L800166F0: ;
        c.S3 = c.S3 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        c.V0 = (int)c.S2 < 3 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S0 = c.S0 + 0xCu;
            goto L800166A8;
        }
        c.S0 = c.S0 + 0xCu;
        c.A0 = 0x00000004u;
        c.S2 = 0u + 0u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x2Au));
        c.S3 = c.A0 + 0u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x8001671Cu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0xBu));
        c.S0 = 0x0000000Cu;
        c.V0 = c.A0 << 1;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.S4 + c.V0;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V0 + 0x1510u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1514u), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1518u), c.T1);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1540u), c.S1);
        L8001675C: ;
        c.A0 = c.S2 + 0x12u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x8001676Cu;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x00000004u;
            goto L800167A4;
        }
        c.A0 = 0x00000004u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80016780u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S0;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU32(m, (c.V1 + 0x1510u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x1514u), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x1518u), c.T1);
        c.V0 = c.S4 + c.S3;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1540u), c.S1);
        L800167A4: ;
        c.S3 = c.S3 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        c.V0 = (int)c.S2 < 3 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S0 = c.S0 + 0xCu;
            goto L8001675C;
        }
        c.S0 = c.S0 + 0xCu;
        c.A0 = 0x00000005u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x2Cu));
        c.S2 = 0x00000001u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x800167CCu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.S3 = c.S4 + 0x4u;
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0xEu));
        c.S0 = c.S4 + 0x1Cu;
        c.V0 = c.A0 << 3;
        c.V0 = c.V0 - c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.S4 + c.V0;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, (c.V0 + 0x1550u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1554u), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1558u), c.T1);
        MemoryAccess.WriteU32(m, (c.V0 + 0x155Cu), c.T2);
        c.A3 = MemoryAccess.ReadU32(m, (c.V1 + 0x10u));
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x14u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x18u));
        MemoryAccess.WriteU32(m, (c.V0 + 0x1560u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1564u), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1568u), c.T1);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0x15DCu), c.S1);
        L80016830: ;
        c.V0 = (int)c.S2 < 5 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A1 = c.S2 & 0x00FFu;
            goto L800168A8;
        }
        c.A1 = c.S2 & 0x00FFu;
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = c.SP + 0x98u;
        c.RA = 0x80016848u;
        GranTurismo2PC.func_80076500(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x00000005u;
            goto L80016898;
        }
        c.A0 = 0x00000005u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x8001685Cu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V0 + 0xCu));
        MemoryAccess.WriteU32(m, (c.S0 + 0x1550u), c.A3);
        MemoryAccess.WriteU32(m, (c.S0 + 0x1554u), c.T0);
        MemoryAccess.WriteU32(m, (c.S0 + 0x1558u), c.T1);
        MemoryAccess.WriteU32(m, (c.S0 + 0x155Cu), c.T2);
        c.A3 = MemoryAccess.ReadU32(m, (c.V0 + 0x10u));
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x14u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x18u));
        MemoryAccess.WriteU32(m, (c.S0 + 0x1560u), c.A3);
        MemoryAccess.WriteU32(m, (c.S0 + 0x1564u), c.T0);
        MemoryAccess.WriteU32(m, (c.S0 + 0x1568u), c.T1);
        MemoryAccess.WriteU32(m, (c.S3 + 0x15DCu), c.S1);
        L80016898: ;
        c.S3 = c.S3 + 0x4u;
        c.S0 = c.S0 + 0x1Cu;
        c.S2 = c.S2 + 0x1u;
        goto L80016830;
        L800168A8: ;
        c.A0 = 0x0000001Bu;
        c.S2 = 0u + 0u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x44u));
        c.S3 = 0x00000004u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x800168C0u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0x8u));
        c.S0 = 0x00000010u;
        c.V0 = c.A0 << (int)(c.S3 & 31u);
        c.V0 = c.S4 + c.V0;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, (c.V0 + 0x15F0u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x15F4u), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x15F8u), c.T1);
        MemoryAccess.WriteU32(m, (c.V0 + 0x15FCu), c.T2);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1610u), c.S1);
        L80016900: ;
        c.A0 = c.S2 + 0u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x80016910u;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x0000001Bu;
            goto L80016950;
        }
        c.A0 = 0x0000001Bu;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80016924u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S0;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V0 + 0xCu));
        MemoryAccess.WriteU32(m, (c.V1 + 0x15F0u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x15F4u), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x15F8u), c.T1);
        MemoryAccess.WriteU32(m, (c.V1 + 0x15FCu), c.T2);
        c.V0 = c.S4 + c.S3;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1610u), c.S1);
        L80016950: ;
        c.S3 = c.S3 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        if ((int)c.S2 <= 0) {
            c.S0 = c.S0 + 0x10u;
            goto L80016900;
        }
        c.S0 = c.S0 + 0x10u;
        c.A0 = 0x0000001Cu;
        c.S2 = 0u + 0u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x46u));
        c.S3 = 0x00000004u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80016978u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0x8u));
        c.S0 = 0x00000010u;
        c.V0 = c.A0 << (int)(c.S3 & 31u);
        c.V0 = c.S4 + c.V0;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, (c.V0 + 0x1618u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x161Cu), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1620u), c.T1);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1624u), c.T2);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1638u), c.S1);
        L800169B8: ;
        c.A0 = c.S2 + 0x26u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x800169C8u;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x0000001Cu;
            goto L80016A08;
        }
        c.A0 = 0x0000001Cu;
        c.A1 = c.S1 + 0u;
        c.RA = 0x800169DCu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S0;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V0 + 0xCu));
        MemoryAccess.WriteU32(m, (c.V1 + 0x1618u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x161Cu), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x1620u), c.T1);
        MemoryAccess.WriteU32(m, (c.V1 + 0x1624u), c.T2);
        c.V0 = c.S4 + c.S3;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1638u), c.S1);
        L80016A08: ;
        c.S3 = c.S3 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        if ((int)c.S2 <= 0) {
            c.S0 = c.S0 + 0x10u;
            goto L800169B8;
        }
        c.S0 = c.S0 + 0x10u;
        c.A0 = 0x00000015u;
        c.S2 = 0u + 0u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x24u));
        c.S3 = 0x00000004u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80016A30u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.V0 + 0u;
        c.A0 = MemoryAccess.ReadU8(m, (c.V1 + 0x8u));
        c.S0 = 0x00000020u;
        c.V0 = c.A0 << 5;
        c.V0 = c.S4 + c.V0;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, (c.V0 + 0x1640u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1644u), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1648u), c.T1);
        MemoryAccess.WriteU32(m, (c.V0 + 0x164Cu), c.T2);
        c.A3 = MemoryAccess.ReadU32(m, (c.V1 + 0x10u));
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x14u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x18u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V1 + 0x1Cu));
        MemoryAccess.WriteU32(m, (c.V0 + 0x1650u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1654u), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x1658u), c.T1);
        MemoryAccess.WriteU32(m, (c.V0 + 0x165Cu), c.T2);
        c.V0 = c.A0 << 2;
        c.V0 = c.S4 + c.V0;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1700u), c.S1);
        L80016A90: ;
        c.A0 = c.S2 + 0x15u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.A2 = 0u + 0u;
        c.RA = 0x80016AA0u;
        GranTurismo2PC.func_80076570(c, m);
        c.S1 = c.V0 + 0u;
        if ((int)c.S1 < 0) {
            c.A0 = 0x00000015u;
            goto L80016B00;
        }
        c.A0 = 0x00000015u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80016AB4u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + c.S0;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V0 + 0xCu));
        MemoryAccess.WriteU32(m, (c.V1 + 0x1640u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x1644u), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x1648u), c.T1);
        MemoryAccess.WriteU32(m, (c.V1 + 0x164Cu), c.T2);
        c.A3 = MemoryAccess.ReadU32(m, (c.V0 + 0x10u));
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x14u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x18u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V0 + 0x1Cu));
        MemoryAccess.WriteU32(m, (c.V1 + 0x1650u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x1654u), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x1658u), c.T1);
        MemoryAccess.WriteU32(m, (c.V1 + 0x165Cu), c.T2);
        c.V0 = c.S4 + c.S3;
        MemoryAccess.WriteU32(m, (c.V0 + 0x1700u), c.S1);
        L80016B00: ;
        c.S3 = c.S3 + 0x4u;
        c.S2 = c.S2 + 0x1u;
        c.V0 = (int)c.S2 < 5 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S0 = c.S0 + 0x20u;
            goto L80016A90;
        }
        c.S0 = c.S0 + 0x20u;
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x18u));
        c.A0 = 0x00000002u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80016B24u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V0 + 0xCu));
        MemoryAccess.WriteU32(m, (c.S4 + 0x1718u), c.A3);
        MemoryAccess.WriteU32(m, (c.S4 + 0x171Cu), c.T0);
        MemoryAccess.WriteU32(m, (c.S4 + 0x1720u), c.T1);
        MemoryAccess.WriteU32(m, (c.S4 + 0x1724u), c.T2);
        c.A3 = MemoryAccess.ReadU32(m, (c.V0 + 0x10u));
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x14u));
        MemoryAccess.WriteU32(m, (c.S4 + 0x1728u), c.A3);
        MemoryAccess.WriteU32(m, (c.S4 + 0x172Cu), c.T0);
        MemoryAccess.WriteU32(m, (c.S4 + 0x1730u), c.S1);
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x1Au));
        c.A0 = 0x00000003u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80016B68u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V0 + 0xCu));
        MemoryAccess.WriteU32(m, (c.S4 + 0x1734u), c.A3);
        MemoryAccess.WriteU32(m, (c.S4 + 0x1738u), c.T0);
        MemoryAccess.WriteU32(m, (c.S4 + 0x173Cu), c.T1);
        MemoryAccess.WriteU32(m, (c.S4 + 0x1740u), c.T2);
        c.A3 = MemoryAccess.ReadU32(m, (c.V0 + 0x10u));
        MemoryAccess.WriteU32(m, (c.S4 + 0x1744u), c.A3);
        MemoryAccess.WriteU32(m, (c.S4 + 0x1748u), c.S1);
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x1Cu));
        c.A0 = 0x00000006u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80016BA8u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = c.S4 + 0x174Cu;
        c.A0 = c.V0 + 0x40u;
        L80016BB0: ;
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
            goto L80016BB0;
        }
        c.V1 = c.V1 + 0x10u;
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU32(m, c.V1, c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x4u), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x8u), c.T1);
        MemoryAccess.WriteU32(m, (c.S4 + 0x1798u), c.S1);
        c.S1 = MemoryAccess.ReadU16(m, (c.SP + 0x1Eu));
        c.A0 = 0x0000000Du;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80016C08u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.A3 = MemoryAccess.ReadU32(m, c.V0);
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V0 + 0xCu));
        MemoryAccess.WriteU32(m, (c.S4 + 0x179Cu), c.A3);
        MemoryAccess.WriteU32(m, (c.S4 + 0x17A0u), c.T0);
        MemoryAccess.WriteU32(m, (c.S4 + 0x17A4u), c.T1);
        MemoryAccess.WriteU32(m, (c.S4 + 0x17A8u), c.T2);
        MemoryAccess.WriteU32(m, (c.S4 + 0x17ACu), c.S1);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0xC4u));
        c.FP = MemoryAccess.ReadU32(m, (c.SP + 0xC0u));
        c.S7 = MemoryAccess.ReadU32(m, (c.SP + 0xBCu));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0xB8u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0xB4u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0xB0u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0xACu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0xA8u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0xA4u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0xA0u));
        c.SP = c.SP + 0xC8u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80016C5C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A1 + 0u;
        c.V1 = c.S0 + 0u;
        c.V0 = c.S1 + 0u;
        c.A0 = c.S1 + 0x80u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        L80016C80: ;
        c.A2 = MemoryAccess.ReadU32(m, c.V0);
        c.A3 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0xCu));
        MemoryAccess.WriteU32(m, c.V1, c.A2);
        MemoryAccess.WriteU32(m, (c.V1 + 0x4u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x8u), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0xCu), c.T1);
        c.V0 = c.V0 + 0x10u;
        if (c.V0 != c.A0) {
            c.V1 = c.V1 + 0x10u;
            goto L80016C80;
        }
        c.V1 = c.V1 + 0x10u;
        c.A2 = MemoryAccess.ReadU32(m, c.V0);
        MemoryAccess.WriteU32(m, c.V1, c.A2);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x12u));
        c.A0 = 0x00000012u;
        c.RA = 0x80016CC4u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17B0u), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x4u));
        c.A0 = 0u + 0u;
        c.RA = 0x80016CDCu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17B2u), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x6u));
        c.A0 = 0x00000001u;
        c.RA = 0x80016CF4u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17B4u), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x16u));
        c.A0 = 0x00000016u;
        c.RA = 0x80016D0Cu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17B6u), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x18u));
        c.A0 = 0x00000017u;
        c.RA = 0x80016D24u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x4u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17B8u), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x10u));
        c.A0 = 0x00000011u;
        c.RA = 0x80016D3Cu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17BCu), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x2Cu));
        c.A0 = 0x0000000Fu;
        c.RA = 0x80016D54u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17BEu), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x2Au));
        c.A0 = 0x0000000Eu;
        c.RA = 0x80016D6Cu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17C0u), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x2Eu));
        c.A0 = 0x00000010u;
        c.RA = 0x80016D84u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17C2u), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x24u));
        c.A0 = 0x0000000Au;
        c.RA = 0x80016D9Cu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17C4u), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x1Eu));
        c.A0 = 0x00000007u;
        c.RA = 0x80016DB4u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17C6u), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x20u));
        c.A0 = 0x00000008u;
        c.RA = 0x80016DCCu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17C8u), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x26u));
        c.A0 = 0x0000000Bu;
        c.RA = 0x80016DE4u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17CAu), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x22u));
        c.A0 = 0x00000009u;
        c.RA = 0x80016DFCu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17CCu), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x28u));
        c.A0 = 0x0000000Cu;
        c.RA = 0x80016E14u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17CEu), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x32u));
        c.A0 = 0x00000013u;
        c.RA = 0x80016E2Cu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17D0u), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x30u));
        c.A0 = 0x00000014u;
        c.RA = 0x80016E44u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17D2u), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x1Au));
        c.A0 = 0x00000004u;
        c.RA = 0x80016E5Cu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0xBu));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17D4u), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x1Cu));
        c.A0 = 0x00000005u;
        c.RA = 0x80016E74u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0xEu));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17D6u), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x34u));
        c.A0 = 0x0000001Bu;
        c.RA = 0x80016E8Cu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17D8u), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x36u));
        c.A0 = 0x0000001Cu;
        c.RA = 0x80016EA4u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17DAu), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x14u));
        c.A0 = 0x00000015u;
        c.RA = 0x80016EBCu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17DCu), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x8u));
        c.A0 = 0x00000002u;
        c.RA = 0x80016ED4u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17E0u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.S0 + 0x17E2u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.S0 + 0x17DEu), (ushort)c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0xEu));
        c.A0 = 0x0000000Du;
        c.RA = 0x80016EF0u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x4u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x17E4u), (ushort)c.V0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80016F10(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S1);
        c.S1 = c.A1 + 0u;
        c.V1 = c.S0 + 0x8u;
        c.V0 = c.S1 + 0u;
        c.A0 = c.S1 + 0x80u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        L80016F34: ;
        c.A2 = MemoryAccess.ReadU32(m, c.V0);
        c.A3 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0xCu));
        MemoryAccess.WriteU32(m, c.V1, c.A2);
        MemoryAccess.WriteU32(m, (c.V1 + 0x4u), c.A3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x8u), c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0xCu), c.T1);
        c.V0 = c.V0 + 0x10u;
        if (c.V0 != c.A0) {
            c.V1 = c.V1 + 0x10u;
            goto L80016F34;
        }
        c.V1 = c.V1 + 0x10u;
        c.A0 = c.S1 + 0u;
        c.A1 = c.SP + 0x10u;
        c.A2 = MemoryAccess.ReadU32(m, c.V0);
        MemoryAccess.WriteU32(m, c.V1, c.A2);
        c.RA = 0x80016F74u;
        GranTurismo2PC.func_8005F958(c, m);
        c.V0 = MemoryAccess.ReadU16(m, (c.S0 + 0x98u));
        c.V1 = MemoryAccess.ReadU16(m, (c.SP + 0x12u));
        c.V0 = c.V0 & 0xC000u;
        c.V0 = c.V0 | c.V1;
        MemoryAccess.WriteU16(m, (c.S0 + 0x98u), (ushort)c.V0);
        c.V0 = MemoryAccess.ReadU16(m, (c.SP + 0x14u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x96u), (ushort)c.V0);
        c.V0 = MemoryAccess.ReadU16(m, (c.S0 + 0x94u));
        c.V1 = MemoryAccess.ReadU16(m, (c.SP + 0x10u));
        c.V0 = c.V0 & 0xE000u;
        c.V0 = c.V0 | c.V1;
        MemoryAccess.WriteU16(m, (c.S0 + 0x94u), (ushort)c.V0);
        c.V1 = MemoryAccess.ReadU8(m, (c.SP + 0x16u));
        c.V0 = c.V0 & 0x1FFFu;
        c.V1 = c.V1 << 13;
        c.V0 = c.V0 | c.V1;
        MemoryAccess.WriteU16(m, (c.S0 + 0x94u), (ushort)c.V0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x17D6u));
        c.V1 = MemoryAccess.ReadU16(m, (c.S0 + 0x98u));
        c.V0 = (int)0u < (int)c.V0 ? 1u : 0u;
        c.V1 = c.V1 & 0x7FFFu;
        c.V0 = c.V0 << 15;
        c.V1 = c.V1 | c.V0;
        MemoryAccess.WriteU16(m, (c.S0 + 0x98u), (ushort)c.V1);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80016FEC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x17BCu));
        c.A1 = 0x00000006u;
        c.RA = 0x80017010u;
        GranTurismo2PC.func_8005EAC0(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.S0 + 0x7Au));
        c.V1 = c.S0 + 0x80u;
        c.V0 = c.V0 | 0x00C0u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x7Au), (byte)c.V0);
        L80017020: ;
        c.A3 = MemoryAccess.ReadU32(m, c.S0);
        c.T0 = MemoryAccess.ReadU32(m, (c.S0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.S0 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.S0 + 0xCu));
        MemoryAccess.WriteU32(m, c.S1, c.A3);
        MemoryAccess.WriteU32(m, (c.S1 + 0x4u), c.T0);
        MemoryAccess.WriteU32(m, (c.S1 + 0x8u), c.T1);
        MemoryAccess.WriteU32(m, (c.S1 + 0xCu), c.T2);
        c.S0 = c.S0 + 0x10u;
        if (c.S0 != c.V1) {
            c.S1 = c.S1 + 0x10u;
            goto L80017020;
        }
        c.S1 = c.S1 + 0x10u;
        c.A3 = MemoryAccess.ReadU32(m, c.S0);
        MemoryAccess.WriteU32(m, c.S1, c.A3);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001706C(CpuContext c, IMemory m)
    {
        c.A3 = 0u + 0u;
        c.V0 = c.A0 < 0x00000032u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A2 = c.A3 + 0u;
            goto L80017188;
        }
        c.A2 = c.A3 + 0u;
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x2D5Cu;
        c.V1 = c.A0 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x80017168u: goto L80017168;
            case 0x800170A8u: goto L800170A8;
            case 0x800170B4u: goto L800170B4;
            case 0x800170CCu: goto L800170CC;
            case 0x800170F0u: goto L800170F0;
            case 0x80017120u: goto L80017120;
            case 0x80017188u: goto L80017188;
            case 0x80017108u: goto L80017108;
            case 0x800170D8u: goto L800170D8;
            case 0x800170C0u: goto L800170C0;
            case 0x80017138u: goto L80017138;
            case 0x80017150u: goto L80017150;
            case 0x80017180u: goto L80017180;
            case 0x80017144u: goto L80017144;
            case 0x80017114u: goto L80017114;
            case 0x800170FCu: goto L800170FC;
            case 0x800170E4u: goto L800170E4;
            case 0x8001715Cu: goto L8001715C;
            case 0x8001709Cu: goto L8001709C;
            case 0x80017174u: goto L80017174;
            case 0x8001712Cu: goto L8001712C;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L8001709C: ;
        c.A3 = 0x00000023u;
        c.A2 = 0u + 0u;
        goto L80017188;
        L800170A8: ;
        c.A3 = 0x00000001u;
        c.A2 = c.A3 + 0u;
        goto L80017188;
        L800170B4: ;
        c.A3 = 0x00000002u;
        c.A2 = c.A3 + 0u;
        goto L80017188;
        L800170C0: ;
        c.A3 = 0x0000000Du;
        c.A2 = 0x00000006u;
        goto L80017188;
        L800170CC: ;
        c.A3 = 0x00000003u;
        c.A2 = 0x00000007u;
        goto L80017188;
        L800170D8: ;
        c.A3 = 0x0000000Au;
        c.A2 = 0x00000008u;
        goto L80017188;
        L800170E4: ;
        c.A3 = 0x00000021u;
        c.A2 = 0x00000009u;
        goto L80017188;
        L800170F0: ;
        c.A3 = 0x00000006u;
        c.A2 = 0x0000000Au;
        goto L80017188;
        L800170FC: ;
        c.A3 = 0x00000020u;
        c.A2 = 0x0000000Bu;
        goto L80017188;
        L80017108: ;
        c.A3 = 0x00000009u;
        c.A2 = 0x0000000Cu;
        goto L80017188;
        L80017114: ;
        c.A3 = 0x0000001Du;
        c.A2 = 0x0000000Du;
        goto L80017188;
        L80017120: ;
        c.A3 = 0x00000007u;
        c.A2 = 0x0000000Eu;
        goto L80017188;
        L8001712C: ;
        c.A3 = 0x0000002Eu;
        c.A2 = 0x0000000Fu;
        goto L80017188;
        L80017138: ;
        c.A3 = 0x00000010u;
        c.A2 = c.A3 + 0u;
        goto L80017188;
        L80017144: ;
        c.A3 = 0x0000001Au;
        c.A2 = 0x00000011u;
        goto L80017188;
        L80017150: ;
        c.A3 = 0x00000012u;
        c.A2 = c.A3 + 0u;
        goto L80017188;
        L8001715C: ;
        c.A3 = 0x00000022u;
        c.A2 = 0x00000013u;
        goto L80017188;
        L80017168: ;
        c.A3 = 0u + 0u;
        c.A2 = 0x00000014u;
        goto L80017188;
        L80017174: ;
        c.A3 = 0x00000026u;
        c.A2 = 0x00000015u;
        goto L80017188;
        L80017180: ;
        c.A3 = 0x00000015u;
        c.A2 = 0x00000016u;
        L80017188: ;
        c.V0 = c.A0 - c.A3;
        c.V0 = c.V0 + 0x1u;
        MemoryAccess.WriteU16(m, (c.A1 + 0x2u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, c.A1, (ushort)c.A2);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001719C(CpuContext c, IMemory m)
    {
        c.V1 = 0u + 0u;
        c.V0 = 0x801D0000u;
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 - 0x2AACu));
        L800171AC: ;
        c.V0 = (int)c.V1 < (int)c.A0 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V1 = c.V1 + 0x1u;
            goto L800171AC;
        }
        c.V1 = c.V1 + 0x1u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800171C0(CpuContext c, IMemory m)
    {
        c.V0 = 0x801D0000u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x156Cu));
        c.SP = c.SP - 0x4F0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x4E8u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x4E4u), c.S1);
        if ((int)c.V0 < 0) {
            MemoryAccess.WriteU32(m, (c.SP + 0x4E0u), c.S0);
            goto L80017274;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x4E0u), c.S0);
        c.V0 = 0x00000083u;
        c.V0 = c.V0 - 0x1u;
        L800171E4: ;
        if ((int)c.V0 >= 0) {
            c.V0 = c.V0 - 0x1u;
            goto L800171E4;
        }
        c.V0 = c.V0 - 0x1u;
        c.V0 = c.V0 + 0x1u;
        c.S0 = 0x800B0000u;
        c.S0 = c.S0 + 0x4490u;
        c.A0 = c.S0 + 0u;
        c.S1 = c.SP + 0x390u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x80017208u;
        GranTurismo2PC.func_8005F410(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.SP + 0x1D0u;
        c.RA = 0x80017214u;
        GranTurismo2PC.func_80077214(c, m);
        c.V0 = c.SP + 0x458u;
        c.V1 = c.S0 + 0x80u;
        L8001721C: ;
        c.A2 = MemoryAccess.ReadU32(m, c.S0);
        c.A3 = MemoryAccess.ReadU32(m, (c.S0 + 0x4u));
        c.T0 = MemoryAccess.ReadU32(m, (c.S0 + 0x8u));
        c.T1 = MemoryAccess.ReadU32(m, (c.S0 + 0xCu));
        MemoryAccess.WriteU32(m, c.V0, c.A2);
        MemoryAccess.WriteU32(m, (c.V0 + 0x4u), c.A3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x8u), c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0xCu), c.T1);
        c.S0 = c.S0 + 0x10u;
        if (c.S0 != c.V1) {
            c.V0 = c.V0 + 0x10u;
            goto L8001721C;
        }
        c.V0 = c.V0 + 0x10u;
        c.A2 = MemoryAccess.ReadU32(m, c.S0);
        MemoryAccess.WriteU32(m, c.V0, c.A2);
        c.A0 = c.SP + 0x458u;
        c.A1 = c.SP + 0x10u;
        c.RA = 0x80017260u;
        GranTurismo2PC.func_800771AC(c, m);
        c.V0 = 0x000001BFu;
        c.V0 = c.V0 - 0x1u;
        L80017268: ;
        if ((int)c.V0 >= 0) {
            c.V0 = c.V0 - 0x1u;
            goto L80017268;
        }
        c.V0 = c.V0 - 0x1u;
        c.V0 = c.V0 + 0x1u;
        L80017274: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x4E8u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x4E4u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x4E0u));
        c.SP = c.SP + 0x4F0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80017288(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = 0x801D0000u;
        c.S2 = c.S2 - 0x6720u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = 0u | 0xC6C0u;
        c.S1 = c.S2 + c.S1;
        c.V0 = 0xFFFFFFFCu;
        c.S1 = c.S1 & c.V0;
        c.A0 = c.S1 + 0u;
        c.S2 = c.S2 + 0x3C74u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.RA = 0x800172C8u;
        GranTurismo2PC.func_80015404(c, m);
        c.S0 = c.S3 << 2;
        c.S0 = c.S0 + c.S3;
        c.S0 = c.S0 << 3;
        c.S0 = c.S0 + c.S3;
        c.S0 = c.S0 << 2;
        c.S0 = c.S0 + 0x4u;
        c.S0 = c.S0 + c.S2;
        c.A1 = MemoryAccess.ReadU32(m, c.S0);
        c.A0 = c.S1 + 0u;
        c.RA = 0x800172F0u;
        GranTurismo2PC.func_80015428(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S0 + 0x8u;
        c.RA = 0x800172FCu;
        GranTurismo2PC.func_80016C5C(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80017318(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x30u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S2);
        c.S2 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S3);
        c.S3 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S4);
        c.S4 = c.A2 + 0u;
        c.V0 = 0x801D0000u;
        c.V0 = c.V0 + 0x5E88u;
        c.V0 = c.V0 + 0x118u;
        c.V1 = 0xFFFFFFFCu;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S1);
        c.S1 = c.V0 & c.V1;
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S0);
        c.S0 = c.V0 + 0xAB0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.RA);
        L8001735C: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.S0);
        if ((int)c.V0 < 0) {
            c.V0 = c.V0 << 1;
            goto L800173C8;
        }
        c.V0 = c.V0 << 1;
        c.V0 = c.V0 + c.S1;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x17B0u));
        if ((int)c.V0 <= 0) {
            goto L800173C0;
        }
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x2u));
        c.A0 = c.A0 + c.V0;
        c.A0 = c.A0 - 0x1u;
        c.RA = 0x80017394u;
        GranTurismo2PC.func_800183B8(c, m);
        c.V1 = 0x00600000u;
        c.V1 = c.V1 | 0x6060u;
        c.A0 = c.S2 + 0u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x4u));
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x6u));
        c.A1 = c.V0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V1);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), 0u);
        c.A2 = c.S3 + c.A2;
        c.A3 = c.S4 + c.A3;
        c.RA = 0x800173C0u;
        GranTurismo2PC.func_8001FBEC(c, m);
        L800173C0: ;
        c.S0 = c.S0 + 0x8u;
        goto L8001735C;
        L800173C8: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x2Cu));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.SP = c.SP + 0x30u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800173E8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = 0x800B0000u;
        c.S1 = c.S1 + 0x4490u;
        c.A0 = c.S1 + 0u;
        c.V0 = 0x801D0000u;
        c.V0 = c.V0 - 0x6720u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A1 << 9;
        c.S0 = c.S0 + c.A1;
        c.S0 = c.S0 << 2;
        c.S0 = c.S0 + c.A1;
        c.S0 = c.S0 << 3;
        c.S0 = c.S0 + 0x3C74u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        c.S0 = c.S0 + c.V0;
        c.RA = 0x80017434u;
        GranTurismo2PC.func_80015404(c, m);
        c.V0 = c.S2 << 2;
        c.V0 = c.V0 + c.S2;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.S2;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + 0x4u;
        c.S0 = c.S0 + c.V0;
        c.A1 = MemoryAccess.ReadU32(m, c.S0);
        c.A0 = c.S1 + 0u;
        c.RA = 0x8001745Cu;
        GranTurismo2PC.func_80015428(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S0 + 0x8u;
        c.RA = 0x80017468u;
        GranTurismo2PC.func_80016C5C(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80017480(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.RA = 0x80017490u;
        GranTurismo2PC.func_800173E8(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800174A0(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = c.V0 + 0x4490u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800174AC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.A0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = c.A0 + 0x4490u;
        c.RA = 0x800174C0u;
        GranTurismo2PC.func_8005F858(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = (int)0u < (int)c.V0 ? 1u : 0u;
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800174D0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.A0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = c.A0 + 0x4490u;
        c.RA = 0x800174E4u;
        GranTurismo2PC.func_8005F858(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800174F4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0x800B0000u;
        c.S0 = c.S0 + 0x4490u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.A0 = c.S0 + 0u;
        c.RA = 0x80017510u;
        GranTurismo2PC.func_8005F858(c, m);
        c.A0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x1574u));
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V1 = 0x00000001u;
        MemoryAccess.WriteU16(m, (c.A0 + 0x5C78u), (ushort)c.V1);
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80017530(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.V0 + 0x4490u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.A0 = c.S0 + 0u;
        c.RA = 0x8001754Cu;
        GranTurismo2PC.func_8005F858(c, m);
        c.A0 = 0x800B0000u;
        c.V1 = MemoryAccess.ReadU16(m, (c.A0 + 0x5C78u));
        c.V1 = c.V1 + 0x1u;
        MemoryAccess.WriteU16(m, (c.A0 + 0x5C78u), (ushort)c.V1);
        c.V1 = c.V1 << 16;
        c.V1 = (uint)((int)c.V1 >> 16);
        c.V0 = (int)c.V0 < (int)c.V1 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000001u;
            goto L80017578;
        }
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU16(m, (c.A0 + 0x5C78u), (ushort)c.V0);
        L80017578: ;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0x5C78u));
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.V0 = c.V1 << 3;
        c.V0 = c.V0 - c.V1;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.S0;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1558u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800175A0(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x44E4u));
        c.V0 = c.V0 < 0x00000001u ? 1u : 0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800175B0(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x5C66u));
        c.V0 = (int)0u < (int)c.V0 ? 1u : 0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800175C0(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.A1 = c.V0 + 0x4490u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0x17B0u));
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0x17B2u));
        if ((int)c.V1 <= 0) {
            c.A0 = (int)c.V0 < 1 ? 1u : 0u;
            goto L800175E0;
        }
        c.A0 = (int)c.V0 < 1 ? 1u : 0u;
        c.A0 = 0u + 0u;
        L800175E0: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0x17B4u));
        if ((int)c.V0 <= 0) {
            goto L800175F4;
        }
        c.A0 = 0u + 0u;
        L800175F4: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0x17BCu));
        if ((int)c.V0 <= 0) {
            goto L80017608;
        }
        c.A0 = 0u + 0u;
        L80017608: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0x17BEu));
        if ((int)c.V0 <= 0) {
            goto L8001761C;
        }
        c.A0 = 0u + 0u;
        L8001761C: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0x17C0u));
        if ((int)c.V0 <= 0) {
            goto L80017630;
        }
        c.A0 = 0u + 0u;
        L80017630: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0x17C2u));
        if ((int)c.V0 <= 0) {
            goto L80017644;
        }
        c.A0 = 0u + 0u;
        L80017644: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0x17C4u));
        if ((int)c.V0 <= 0) {
            goto L80017658;
        }
        c.A0 = 0u + 0u;
        L80017658: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0x17C6u));
        if ((int)c.V0 <= 0) {
            goto L8001766C;
        }
        c.A0 = 0u + 0u;
        L8001766C: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0x17C8u));
        if ((int)c.V0 <= 0) {
            goto L80017680;
        }
        c.A0 = 0u + 0u;
        L80017680: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0x17CAu));
        if ((int)c.V0 <= 0) {
            goto L80017694;
        }
        c.A0 = 0u + 0u;
        L80017694: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0x17CCu));
        if ((int)c.V0 <= 0) {
            goto L800176A8;
        }
        c.A0 = 0u + 0u;
        L800176A8: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0x17CEu));
        if ((int)c.V0 <= 0) {
            goto L800176BC;
        }
        c.A0 = 0u + 0u;
        L800176BC: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0x17D0u));
        if ((int)c.V0 <= 0) {
            goto L800176D0;
        }
        c.A0 = 0u + 0u;
        L800176D0: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0x17D2u));
        if ((int)c.V0 <= 0) {
            goto L800176E4;
        }
        c.A0 = 0u + 0u;
        L800176E4: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0x17D4u));
        if ((int)c.V0 <= 0) {
            goto L800176F8;
        }
        c.A0 = 0u + 0u;
        L800176F8: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0x17D6u));
        if ((int)c.V0 <= 0) {
            goto L8001770C;
        }
        c.A0 = 0u + 0u;
        L8001770C: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0x17D8u));
        if ((int)c.V0 <= 0) {
            goto L80017720;
        }
        c.A0 = 0u + 0u;
        L80017720: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0x17DAu));
        if ((int)c.V0 <= 0) {
            goto L80017734;
        }
        c.A0 = 0u + 0u;
        L80017734: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0x17DCu));
        if ((int)c.V0 <= 0) {
            goto L80017748;
        }
        c.A0 = 0u + 0u;
        L80017748: ;
        c.V0 = c.A0 + 0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80017750(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A1 + 0u;
        c.V0 = 0x00010000u;
        c.V0 = c.V0 | 0x0BD8u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0x801D0000u;
        c.S0 = c.S0 - 0x6720u;
        c.S0 = c.S0 + c.V0;
        c.V0 = 0xFFFFFFFCu;
        c.S0 = c.S0 & c.V0;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        c.A0 = c.S0 + 0u;
        c.RA = 0x80017790u;
        GranTurismo2PC.func_80015404(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S2 + 0u;
        c.RA = 0x8001779Cu;
        GranTurismo2PC.func_80015428(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x800177A8u;
        GranTurismo2PC.func_80016C5C(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x800177B4u;
        GranTurismo2PC.func_80016FEC(c, m);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x17D6u));
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = (int)0u < (int)c.V0 ? 1u : 0u;
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800177D4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.S0 = c.A0 + 0u;
        c.RA = 0x800177E8u;
        GranTurismo2PC.func_80076F0C(c, m);
        c.A0 = c.V0 + 0u;
        c.A1 = 0x0000001Eu;
        c.A2 = c.S0 + 0u;
        c.RA = 0x800177F8u;
        GranTurismo2PC.func_80077DC4(c, m);
        if (c.V0 != 0u) {
            goto L80017808;
        }
        c.V0 = 0u + 0u;
        goto L8001780C;
        L80017808: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x44u));
        L8001780C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001781C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.RA = 0x80017840u;
        GranTurismo2PC.func_80076F0C(c, m);
        c.A0 = c.V0 + 0u;
        c.A1 = 0x0000001Eu;
        c.A2 = c.S3 + 0u;
        c.RA = 0x80017850u;
        GranTurismo2PC.func_80077DC4(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x40u));
        if (c.V0 != 0u) {
            c.V1 = 0x00000006u;
            goto L80017868;
        }
        c.V1 = 0x00000006u;
        c.V0 = 0u + 0u;
        goto L800178C8;
        L80017868: ;
        c.V0 = c.S2 + c.V1;
        L8001786C: ;
        MemoryAccess.WriteU8(m, c.V0, (byte)0u);
        c.V1 = c.V1 - 0x1u;
        if ((int)c.V1 >= 0) {
            c.V0 = c.V0 - 0x1u;
            goto L8001786C;
        }
        c.V0 = c.V0 - 0x1u;
        c.V0 = 0x80050000u;
        c.S0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0xB68u));
        if ((int)c.S0 < 0) {
            c.S1 = c.V0 + 0xB68u;
            goto L800178C4;
        }
        c.S1 = c.V0 + 0xB68u;
        c.A0 = c.S0 + 0u;
        L80017894: ;
        c.A1 = c.S3 + 0u;
        c.A2 = 0u + 0u;
        c.RA = 0x800178A0u;
        GranTurismo2PC.func_80076570(c, m);
        if ((int)c.V0 < 0) {
            c.A0 = c.S0 + 0u;
            goto L800178B0;
        }
        c.A0 = c.S0 + 0u;
        c.A1 = c.S2 + 0u;
        c.RA = 0x800178B0u;
        GranTurismo2PC.func_8005E900(c, m);
        L800178B0: ;
        c.S1 = c.S1 + 0x2u;
        c.S0 = (uint)(short)MemoryAccess.ReadU16(m, c.S1);
        if ((int)c.S0 >= 0) {
            c.A0 = c.S0 + 0u;
            goto L80017894;
        }
        c.A0 = c.S0 + 0u;
        L800178C4: ;
        c.V0 = c.S2 + 0u;
        L800178C8: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800178E4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0xA0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x98u), c.RA);
        c.A1 = c.SP + 0x10u;
        c.RA = 0x800178F4u;
        GranTurismo2PC.func_80076954(c, m);
        c.A1 = MemoryAccess.ReadU16(m, (c.SP + 0x20u));
        c.A0 = 0x00000011u;
        c.RA = 0x80017900u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x9u));
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x98u));
        c.V0 = c.V0 < 0x00000003u ? 1u : 0u;
        c.SP = c.SP + 0xA0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80017914(CpuContext c, IMemory m)
    {
        c.V1 = 0x801D0000u;
        c.V1 = c.V1 - 0x6720u;
        c.V0 = c.A2 << 9;
        c.V0 = c.V0 + c.A2;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.A2;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + 0x3C74u;
        c.V1 = c.V0 + c.V1;
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4014u));
        c.V0 = (int)c.V0 < (int)c.A1 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = 0xFFFFFFFFu;
            goto L80017964;
        }
        c.V0 = 0xFFFFFFFFu;
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, c.V1);
        c.V1 = 0x00000064u;
        if (c.A0 == c.V1) {
            c.V0 = 0xFFFFFFFEu;
            goto L80017964;
        }
        c.V0 = 0xFFFFFFFEu;
        c.V0 = 0x00000001u;
        return;
        L80017964: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001796C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0xD0u;
        MemoryAccess.WriteU32(m, (c.SP + 0xB8u), c.S2);
        c.S2 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0xC4u), c.S5);
        c.S5 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0xC0u), c.S4);
        c.S4 = c.A2 + 0u;
        c.A1 = c.SP + 0x20u;
        c.V1 = 0x801D0000u;
        c.V1 = c.V1 - 0x6720u;
        c.V0 = c.A3 << 9;
        c.V0 = c.V0 + c.A3;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.A3;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + 0x3C74u;
        MemoryAccess.WriteU32(m, (c.SP + 0xB4u), c.S1);
        c.S1 = c.V0 + c.V1;
        MemoryAccess.WriteU32(m, (c.SP + 0xC8u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0xBCu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0xB0u), c.S0);
        c.RA = 0x800179C4u;
        GranTurismo2PC.func_80076954(c, m);
        c.A0 = c.S2 + 0u;
        c.A1 = c.SP + 0x20u;
        c.RA = 0x800179D0u;
        GranTurismo2PC.func_80017750(c, m);
        c.V1 = MemoryAccess.ReadU32(m, (c.S1 + 0x4014u));
        c.V1 = (int)c.V1 < (int)c.S4 ? 1u : 0u;
        if (c.V1 == 0u) {
            c.S3 = c.V0 + 0u;
            goto L800179EC;
        }
        c.S3 = c.V0 + 0u;
        c.V0 = 0xFFFFFFFFu;
        goto L80017A44;
        L800179EC: ;
        c.A0 = c.S2 + 0u;
        c.RA = 0x800179F4u;
        GranTurismo2PC.func_800178E4(c, m);
        c.A0 = c.S2 + 0u;
        c.A1 = c.SP + 0xA8u;
        c.S0 = c.V0 + 0u;
        c.RA = 0x80017A04u;
        GranTurismo2PC.func_8001781C(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S2 + 0u;
        c.A2 = c.S5 + 0u;
        c.A3 = c.SP + 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.V0);
        c.RA = 0x80017A28u;
        GranTurismo2PC.func_8001EE78(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 != 0u) {
            c.V0 = 0xFFFFFFFEu;
            goto L80017A44;
        }
        c.V0 = 0xFFFFFFFEu;
        c.V1 = MemoryAccess.ReadU32(m, (c.S1 + 0x4014u));
        c.V0 = 0x00000001u;
        c.V1 = c.V1 - c.S4;
        MemoryAccess.WriteU32(m, (c.S1 + 0x4014u), c.V1);
        L80017A44: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0xC8u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0xC4u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0xC0u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0xBCu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0xB8u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0xB4u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0xB0u));
        c.SP = c.SP + 0xD0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80017A68(CpuContext c, IMemory m)
    {
        c.V0 = 0u + 0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80017A70(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        c.V1 = 0x801D0000u;
        c.V1 = c.V1 - 0x6720u;
        c.V0 = c.A1 << 9;
        c.V0 = c.V0 + c.A1;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.A1;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + 0x3C74u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.V0 + c.V1;
        c.V0 = c.S0 << 2;
        c.V0 = c.V0 + c.S0;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.S0;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.S1;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.RA = 0x80017ACCu;
        GranTurismo2PC.func_800177D4(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S0 + 0u;
        c.S0 = c.V0 + 0u;
        c.RA = 0x80017ADCu;
        GranTurismo2PC.func_8001EDAC(c, m);
        if ((int)c.S0 >= 0) {
            c.A0 = c.S1 + 0u;
            goto L80017AE8;
        }
        c.A0 = c.S1 + 0u;
        c.S0 = c.S0 + 0x3u;
        L80017AE8: ;
        c.A1 = (uint)((int)c.S0 >> 2);
        c.RA = 0x80017AF0u;
        GranTurismo2PC.func_8005E7B0(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80017B04(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.V0 = c.A0 + 0u;
        c.A0 = c.A1 + 0u;
        c.A1 = c.V0 + 0u;
        c.A2 = c.SP + 0x10u;
        c.RA = 0x80017B20u;
        GranTurismo2PC.func_80076570(c, m);
        if ((int)c.V0 >= 0) {
            c.V0 = 0xFFFFFFFFu;
            goto L80017B2C;
        }
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        L80017B2C: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80017B40(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A1 + 0u;
        c.V0 = 0x801D0000u;
        c.V0 = c.V0 - 0x6720u;
        c.V1 = c.A2 << 9;
        c.V1 = c.V1 + c.A2;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.A2;
        c.V1 = c.V1 << 3;
        c.V1 = c.V1 + 0x3C74u;
        c.V1 = c.V1 + c.V0;
        c.V0 = c.A0 << 2;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + 0x4u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.V1 + c.V0;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.A0 = MemoryAccess.ReadU32(m, c.S0);
        c.S3 = c.V1 + 0u;
        c.RA = 0x80017BA8u;
        GranTurismo2PC.func_80017B04(c, m);
        c.A0 = c.S2 + 0u;
        c.S1 = c.V0 + 0u;
        c.A1 = MemoryAccess.ReadU32(m, c.S0);
        c.A2 = 0u + 0u;
        c.RA = 0x80017BBCu;
        GranTurismo2PC.func_80076570(c, m);
        if ((int)c.V0 < 0) {
            c.V0 = 0xFFFFFFF8u;
            goto L80017C7C;
        }
        c.V0 = 0xFFFFFFF8u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S3 + 0x4014u));
        c.V0 = (int)c.V0 < (int)c.S1 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = 0xFFFFFFFFu;
            goto L80017C7C;
        }
        c.V0 = 0xFFFFFFFFu;
        c.A0 = c.S0 + 0u;
        c.A1 = c.S2 + 0u;
        c.RA = 0x80017BE4u;
        GranTurismo2PC.func_8005E874(c, m);
        if (c.V0 != 0u) {
            c.V0 = 0xFFFFFFFDu;
            goto L80017C7C;
        }
        c.V0 = 0xFFFFFFFDu;
        c.V0 = 0x00000014u;
        if (c.S2 == c.V0) {
            c.V0 = (int)c.S2 < 21 ? 1u : 0u;
            goto L80017C44;
        }
        c.V0 = (int)c.S2 < 21 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000013u;
            goto L80017C10;
        }
        c.V0 = 0x00000013u;
        if (c.S2 == c.V0) {
            c.V0 = 0x00000001u;
            goto L80017C24;
        }
        c.V0 = 0x00000001u;
        goto L80017C7C;
        L80017C10: ;
        c.V0 = 0x00000022u;
        if (c.S2 == c.V0) {
            c.A0 = c.S0 + 0u;
            goto L80017C64;
        }
        c.A0 = c.S0 + 0u;
        c.V0 = 0x00000001u;
        goto L80017C7C;
        L80017C24: ;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x00000012u;
        c.RA = 0x80017C30u;
        GranTurismo2PC.func_8005E874(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 == 0u) {
            c.V0 = 0xFFFFFFFBu;
            goto L80017C78;
        }
        c.V0 = 0xFFFFFFFBu;
        goto L80017C7C;
        L80017C44: ;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x00000013u;
        c.RA = 0x80017C50u;
        GranTurismo2PC.func_8005E874(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 == 0u) {
            c.V0 = 0xFFFFFFFAu;
            goto L80017C78;
        }
        c.V0 = 0xFFFFFFFAu;
        goto L80017C7C;
        L80017C64: ;
        c.A1 = 0x00000014u;
        c.RA = 0x80017C6Cu;
        GranTurismo2PC.func_8005E874(c, m);
        c.V1 = c.V0 ^ 0x0001u;
        if (c.V1 != 0u) {
            c.V0 = 0xFFFFFFF9u;
            goto L80017C7C;
        }
        c.V0 = 0xFFFFFFF9u;
        L80017C78: ;
        c.V0 = 0x00000001u;
        L80017C7C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80017C98(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A2 + 0u;
        c.V1 = 0x801D0000u;
        c.V1 = c.V1 - 0x6720u;
        c.V0 = c.S1 << 9;
        c.V0 = c.V0 + c.S1;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.S1;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + 0x3C74u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.V0 + c.V1;
        c.V0 = c.S0 << 2;
        c.V0 = c.V0 + c.S0;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.S0;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + 0x4u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = c.S2 + c.V0;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.RA);
        c.A0 = MemoryAccess.ReadU32(m, c.S3);
        c.S4 = c.A1 + 0u;
        c.RA = 0x80017D08u;
        GranTurismo2PC.func_80017B04(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S4 + 0u;
        c.A2 = c.S1 + 0u;
        c.S0 = c.V0 + 0u;
        c.RA = 0x80017D1Cu;
        GranTurismo2PC.func_80017B40(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = 0x00000001u;
        if (c.V1 != c.V0) {
            c.V0 = c.V1 + 0u;
            goto L80017D4C;
        }
        c.V0 = c.V1 + 0u;
        c.A0 = c.S3 + 0u;
        c.A1 = c.S4 + 0u;
        c.A2 = c.S0 + 0u;
        c.RA = 0x80017D3Cu;
        GranTurismo2PC.func_8005E8B0(c, m);
        c.V1 = MemoryAccess.ReadU32(m, (c.S2 + 0x4014u));
        c.V0 = 0x00000001u;
        c.V1 = c.V1 - c.S0;
        MemoryAccess.WriteU32(m, (c.S2 + 0x4014u), c.V1);
        L80017D4C: ;
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
    public static void func_80017D6C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x38u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S3);
        c.S3 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S1);
        c.S1 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.S5);
        c.S5 = c.A3 + 0u;
        c.V1 = 0x801D0000u;
        c.V1 = c.V1 - 0x6720u;
        c.V0 = c.A2 << 9;
        c.V0 = c.V0 + c.A2;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.A2;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + 0x3C74u;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S4);
        c.S4 = c.V0 + c.V1;
        c.V0 = c.S3 << 2;
        c.V0 = c.V0 + c.S3;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.S3;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + 0x4u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S2);
        c.S2 = c.S4 + c.V0;
        c.A0 = c.S2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S0);
        c.RA = 0x80017DE0u;
        GranTurismo2PC.func_8005E874(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 != 0u) {
            c.V0 = 0xFFFFFFFCu;
            goto L80017EF4;
        }
        c.V0 = 0xFFFFFFFCu;
        c.A0 = c.S1 + 0u;
        c.A1 = MemoryAccess.ReadU32(m, c.S2);
        c.A2 = 0u + 0u;
        c.RA = 0x80017DFCu;
        GranTurismo2PC.func_80076570(c, m);
        if ((int)c.V0 >= 0) {
            c.V0 = 0x00000022u;
            goto L80017E0C;
        }
        c.V0 = 0x00000022u;
        c.V0 = 0xFFFFFFF8u;
        goto L80017EF4;
        L80017E0C: ;
        if (c.S1 != c.V0) {
            c.V0 = (int)c.S1 < 34 ? 1u : 0u;
            goto L80017E5C;
        }
        c.V0 = (int)c.S1 < 34 ? 1u : 0u;
        c.S0 = 0x800B0000u;
        c.S0 = c.S0 + 0x4490u;
        c.A0 = c.S0 + 0u;
        c.S1 = 0x800B0000u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x5C78u));
        c.A1 = 0x00000013u;
        c.RA = 0x80017E30u;
        GranTurismo2PC.func_8005EAC0(c, m);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x5C78u));
        c.A0 = c.S2 + 0u;
        c.V0 = c.V1 << 3;
        c.V0 = c.V0 - c.V1;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.S0;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1558u));
        c.A1 = c.S0 + 0u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.S5);
        MemoryAccess.WriteU32(m, (c.A0 + 0x8Cu), c.V0);
        goto L80017EE8;
        L80017E5C: ;
        if (c.V0 != 0u) {
            c.A0 = c.S1 + 0u;
            goto L80017EA8;
        }
        c.A0 = c.S1 + 0u;
        c.V0 = (int)c.S1 < 46 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = (int)c.S1 < 39 ? 1u : 0u;
            goto L80017EA8;
        }
        c.V0 = (int)c.S1 < 39 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S0 = 0x800B0000u;
            goto L80017EA8;
        }
        c.S0 = 0x800B0000u;
        c.S0 = c.S0 + 0x4490u;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x00000003u;
        c.S1 = c.S1 - 0x26u;
        c.A2 = c.S1 + 0u;
        c.RA = 0x80017E90u;
        GranTurismo2PC.func_8005EAC0(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x00000004u;
        c.A2 = c.S1 + 0u;
        c.RA = 0x80017EA0u;
        GranTurismo2PC.func_8005EAC0(c, m);
        c.A0 = c.S2 + 0u;
        goto L80017EE4;
        L80017EA8: ;
        c.A1 = c.SP + 0x10u;
        c.RA = 0x80017EB0u;
        GranTurismo2PC.func_8001706C(c, m);
        c.S0 = 0x800B0000u;
        c.S0 = c.S0 + 0x4490u;
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.SP + 0x10u));
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.SP + 0x12u));
        c.A0 = c.S0 + 0u;
        c.RA = 0x80017EC8u;
        GranTurismo2PC.func_8005EAC0(c, m);
        c.A0 = c.S3 << 2;
        c.A0 = c.A0 + c.S3;
        c.A0 = c.A0 << 3;
        c.A0 = c.A0 + c.S3;
        c.A0 = c.A0 << 2;
        c.A0 = c.A0 + 0x4u;
        c.A0 = c.S4 + c.A0;
        L80017EE4: ;
        c.A1 = c.S0 + 0u;
        L80017EE8: ;
        c.RA = 0x80017EF0u;
        GranTurismo2PC.func_80016F10(c, m);
        c.V0 = 0x00000001u;
        L80017EF4: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x30u));
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
    public static void func_80017F18(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x30u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S2);
        c.S2 = c.A1 + 0u;
        c.V1 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S4);
        c.S4 = c.A3 + 0u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S3);
        c.S3 = c.V0 + 0x4490u;
        c.A0 = c.S3 + 0u;
        c.A1 = c.S4 + 0u;
        c.A2 = 0xFFFFFFFFu;
        c.A3 = 0u + 0u;
        c.V0 = 0x801D0000u;
        c.V0 = c.V0 - 0x6720u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S0);
        c.S0 = c.V1 << 9;
        c.S0 = c.S0 + c.V1;
        c.S0 = c.S0 << 2;
        c.S0 = c.S0 + c.V1;
        c.S0 = c.S0 << 3;
        c.S0 = c.S0 + 0x3C74u;
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.RA);
        c.S0 = c.S0 + c.V0;
        c.RA = 0x80017F84u;
        GranTurismo2PC.func_8005F044(c, m);
        c.A0 = c.S2 + 0u;
        c.V0 = c.S1 << 2;
        c.V0 = c.V0 + c.S1;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.S1;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.S0;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.A2 = 0u + 0u;
        c.RA = 0x80017FACu;
        GranTurismo2PC.func_80076570(c, m);
        if ((int)c.V0 < 0) {
            c.V0 = (int)c.S2 < 46 ? 1u : 0u;
            goto L80017FE4;
        }
        c.V0 = (int)c.S2 < 46 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = (int)c.S2 < 39 ? 1u : 0u;
            goto L80017FC4;
        }
        c.V0 = (int)c.S2 < 39 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L80017FE4;
        }
        L80017FC4: ;
        c.A0 = c.S2 + 0u;
        c.A1 = c.SP + 0x10u;
        c.RA = 0x80017FD0u;
        GranTurismo2PC.func_8001706C(c, m);
        c.A0 = c.S3 + 0u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.SP + 0x10u));
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.SP + 0x12u));
        c.A1 = c.S4 + 0u;
        c.RA = 0x80017FE4u;
        GranTurismo2PC.func_8005F044(c, m);
        L80017FE4: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x2Cu));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.SP = c.SP + 0x30u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80018004(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A2 + 0u;
        c.V0 = 0x801D0000u;
        c.V0 = c.V0 - 0x6720u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A1 << 9;
        c.S0 = c.S0 + c.A1;
        c.S0 = c.S0 << 2;
        c.S0 = c.S0 + c.A1;
        c.S0 = c.S0 << 3;
        c.S0 = c.S0 + 0x3C74u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        c.S0 = c.S0 + c.V0;
        c.RA = 0x80018048u;
        GranTurismo2PC.func_800173E8(c, m);
        c.V0 = c.S2 << 2;
        c.V0 = c.V0 + c.S2;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.S2;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + 0x4u;
        c.S0 = c.S0 + c.V0;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x0000002Du;
        c.RA = 0x80018070u;
        GranTurismo2PC.func_8005E874(c, m);
        c.S1 = c.S1 & c.V0;
        if (c.S1 == 0u) {
            c.A0 = c.S2 + 0u;
            goto L80018094;
        }
        c.A0 = c.S2 + 0u;
        c.A1 = 0x0000002Du;
        c.A2 = 0u + 0u;
        c.A3 = c.A2 + 0u;
        c.RA = 0x8001808Cu;
        GranTurismo2PC.func_80017D6C(c, m);
        goto L800180E8;
        L80018094: ;
        c.V0 = 0x800B0000u;
        c.S1 = c.V0 + 0x4490u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x17B6u));
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x17B8u));
        c.V0 = c.V0 ^ 0x0007u;
        c.V0 = c.V0 < 0x00000001u ? 1u : 0u;
        c.V1 = c.V1 ^ 0x0007u;
        c.V1 = c.V1 < 0x00000001u ? 1u : 0u;
        c.V0 = c.V0 | c.V1;
        if (c.V0 == 0u) {
            c.A0 = c.S1 + 0u;
            goto L800180E8;
        }
        c.A0 = c.S1 + 0u;
        c.A1 = 0x00000003u;
        c.A2 = 0u + 0u;
        c.RA = 0x800180CCu;
        GranTurismo2PC.func_8005EAC0(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = 0x00000004u;
        c.A2 = 0u + 0u;
        c.RA = 0x800180DCu;
        GranTurismo2PC.func_8005EAC0(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x800180E8u;
        GranTurismo2PC.func_80016F10(c, m);
        L800180E8: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80018100_gt2_overlay_4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = c.A2 + 0u;
        c.A1 = c.A3 + 0u;
        c.A0 = c.S3 + 0u;
        c.V1 = 0x801D0000u;
        c.V1 = c.V1 - 0x6720u;
        c.V0 = c.A1 << 9;
        c.V0 = c.V0 + c.A1;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.A1;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + 0x3C74u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        c.S1 = c.V0 + c.V1;
        c.RA = 0x80018154u;
        GranTurismo2PC.func_800181D0(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = 0x00000001u;
        if (c.V1 != c.V0) {
            c.V0 = c.V1 + 0u;
            goto L800181B4;
        }
        c.V0 = c.V1 + 0u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x8001816Cu;
        GranTurismo2PC.func_80021B0C(c, m);
        c.S0 = 0x800B0000u;
        c.S0 = c.S0 + 0x4490u;
        c.A0 = c.S0 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x80018180u;
        GranTurismo2PC.func_80021B38(c, m);
        c.A0 = c.S2 << 2;
        c.A0 = c.A0 + c.S2;
        c.A0 = c.A0 << 3;
        c.A0 = c.A0 + c.S2;
        c.A0 = c.A0 << 2;
        c.A0 = c.A0 + 0x4u;
        c.A0 = c.S1 + c.A0;
        c.A1 = c.S0 + 0u;
        c.RA = 0x800181A4u;
        GranTurismo2PC.func_80016F10(c, m);
        c.V1 = MemoryAccess.ReadU32(m, (c.S1 + 0x4014u));
        c.V0 = 0x00000001u;
        c.V1 = c.V1 - c.S3;
        MemoryAccess.WriteU32(m, (c.S1 + 0x4014u), c.V1);
        L800181B4: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800181D0(CpuContext c, IMemory m)
    {
        c.V1 = 0x801D0000u;
        c.V1 = c.V1 - 0x6720u;
        c.V0 = c.A1 << 9;
        c.V0 = c.V0 + c.A1;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.A1;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.V1;
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x7C88u));
        c.V1 = (int)c.V1 < (int)c.A0 ? 1u : 0u;
        if (c.V1 != 0u) {
            c.V0 = 0xFFFFFFFFu;
            goto L80018208;
        }
        c.V0 = 0xFFFFFFFFu;
        c.V0 = 0x00000001u;
        L80018208: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80018210(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        c.V0 = 0x80050000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0xB78u));
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        if (c.A0 == 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
            goto L8001825C;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.V0 + 0xB78u;
        L80018234: ;
        c.RA = 0x8001823Cu;
        GranTurismo2PC.func_80060924(c, m);
        if (c.S1 != c.V0) {
            c.S0 = c.S0 + 0x4u;
            goto L8001824C;
        }
        c.S0 = c.S0 + 0x4u;
        c.V0 = 0u + 0u;
        goto L80018260;
        L8001824C: ;
        c.A0 = MemoryAccess.ReadU32(m, c.S0);
        if (c.A0 != 0u) {
            goto L80018234;
        }
        L8001825C: ;
        c.V0 = 0x00000001u;
        L80018260: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80018274(CpuContext c, IMemory m)
    {
        c.V0 = 0x801D0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1568u));
        c.V0 = (int)c.V0 < 50 ? 1u : 0u;
        c.V0 = c.V0 ^ 0x0001u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001828C(CpuContext c, IMemory m)
    {
        c.V0 = 0x80050000u;
        if (c.A0 == 0u) {
            c.V0 = c.V0 + 0xBACu;
            goto L800182A0;
        }
        c.V0 = c.V0 + 0xBACu;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 + 0xBB0u;
        L800182A0: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800182A8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.S0 = c.A0 + 0u;
        c.RA = 0x800182BCu;
        GranTurismo2PC.func_80076F0C(c, m);
        c.A0 = c.V0 + 0u;
        c.A1 = 0x0000001Eu;
        c.A2 = c.S0 + 0u;
        c.RA = 0x800182CCu;
        GranTurismo2PC.func_80077DC4(c, m);
        if (c.V0 != 0u) {
            goto L800182E0;
        }
        c.V0 = 0x80050000u;
        c.V0 = c.V0 + 0xB9Cu;
        goto L800182EC;
        L800182E0: ;
        c.A0 = MemoryAccess.ReadU16(m, (c.V0 + 0x3Cu));
        c.RA = 0x800182ECu;
        GranTurismo2PC.func_80076C14(c, m);
        L800182EC: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800182FC_gt2_overlay_4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.S0 = c.A0 + 0u;
        c.RA = 0x80018310u;
        GranTurismo2PC.func_80076F0C(c, m);
        c.A0 = c.V0 + 0u;
        c.A1 = 0x0000001Eu;
        c.A2 = c.S0 + 0u;
        c.RA = 0x80018320u;
        GranTurismo2PC.func_80077DC4(c, m);
        if (c.V0 != 0u) {
            goto L80018334;
        }
        c.V0 = 0x80050000u;
        c.V0 = c.V0 + 0xB9Cu;
        goto L80018340;
        L80018334: ;
        c.A0 = MemoryAccess.ReadU16(m, (c.V0 + 0x3Eu));
        c.RA = 0x80018340u;
        GranTurismo2PC.func_80076C14(c, m);
        L80018340: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80018350(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S0);
        c.S0 = c.A1 + 0u;
        c.A1 = c.SP + 0x10u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        c.A2 = c.SP + 0x14u;
        c.RA = 0x8001836Cu;
        GranTurismo2PC.func_80060BEC(c, m);
        c.A0 = c.V0 + 0u;
        if ((int)c.A0 <= 0) {
            c.V1 = 0u + 0u;
            goto L800183A4;
        }
        c.V1 = 0u + 0u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.V0 = c.A1 + c.V1;
        L80018384: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, c.V0);
        if (c.V0 == c.S0) {
            c.V0 = c.V1 + 0u;
            goto L800183A8;
        }
        c.V0 = c.V1 + 0u;
        c.V1 = c.V1 + 0x1u;
        c.V0 = (int)c.V1 < (int)c.A0 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = c.A1 + c.V1;
            goto L80018384;
        }
        c.V0 = c.A1 + c.V1;
        L800183A4: ;
        c.V0 = 0u + 0u;
        L800183A8: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800183B8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 + 0x10FCu;
        c.A0 = c.A0 << 1;
        c.A0 = c.A0 + c.V0;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, c.A0);
        c.RA = 0x800183DCu;
        GranTurismo2PC.func_80076C14(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800183EC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        c.A2 = c.A0 + 0u;
        c.A1 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S0);
        c.S0 = c.SP + 0x10u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        c.A0 = c.A2 + c.A1;
        L80018408: ;
        c.V1 = MemoryAccess.ReadU8(m, c.A0);
        c.V0 = c.S0 + c.A1;
        MemoryAccess.WriteU8(m, c.V0, (byte)c.V1);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, c.A0);
        if (c.V0 == 0u) {
            c.V0 = c.S0 + c.A1;
            goto L80018438;
        }
        c.V0 = c.S0 + c.A1;
        c.A1 = c.A1 + 0x1u;
        c.V0 = (int)c.A1 < 3 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.A0 = c.A2 + c.A1;
            goto L80018408;
        }
        c.A0 = c.A2 + c.A1;
        c.V0 = c.S0 + c.A1;
        L80018438: ;
        MemoryAccess.WriteU8(m, c.V0, (byte)0u);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x80020000u;
        c.A1 = c.A1 + 0x2E64u;
        c.RA = 0x8001844Cu;
        GranTurismo2PC.func_8008CF00(c, m);
        if (c.V0 == 0u) {
            c.A0 = c.S0 + 0u;
            goto L80018468;
        }
        c.A0 = c.S0 + 0u;
        c.A1 = 0x80020000u;
        c.A1 = c.A1 + 0x2E68u;
        c.RA = 0x80018460u;
        GranTurismo2PC.func_8008CF00(c, m);
        c.V0 = c.V0 < 0x00000001u ? 1u : 0u;
        goto L8001846C;
        L80018468: ;
        c.V0 = 0x00000001u;
        L8001846C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001847C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = 0x00000001u;
        c.V1 = 0x80050000u;
        c.V0 = 0x801D0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V1 + 0xBB4u));
        c.V0 = c.V0 - 0x6720u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = c.V0 + 0xB8u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        if (c.A0 == 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
            goto L800184EC;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S2 = c.S1 + 0u;
        c.S0 = c.V1 + 0xBB4u;
        L800184B8: ;
        c.RA = 0x800184C0u;
        GranTurismo2PC.func_800188B0(c, m);
        c.A0 = c.S3 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x800184CCu;
        GranTurismo2PC.func_8005DB90(c, m);
        if (c.V0 == c.S2) {
            goto L800184D8;
        }
        c.S1 = 0u + 0u;
        L800184D8: ;
        c.S0 = c.S0 + 0x4u;
        c.A0 = MemoryAccess.ReadU32(m, c.S0);
        if (c.A0 != 0u) {
            goto L800184B8;
        }
        L800184EC: ;
        c.V0 = c.S1 + 0u;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001850C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = 0x00000001u;
        c.V1 = 0x80050000u;
        c.V0 = 0x801D0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V1 + 0xBF8u));
        c.V0 = c.V0 - 0x6720u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = c.V0 + 0xB8u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        if (c.A0 == 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
            goto L8001857C;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S2 = c.S1 + 0u;
        c.S0 = c.V1 + 0xBF8u;
        L80018548: ;
        c.RA = 0x80018550u;
        GranTurismo2PC.func_800188B0(c, m);
        c.A0 = c.S3 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001855Cu;
        GranTurismo2PC.func_8005DB90(c, m);
        if (c.V0 == c.S2) {
            goto L80018568;
        }
        c.S1 = 0u + 0u;
        L80018568: ;
        c.S0 = c.S0 + 0x4u;
        c.A0 = MemoryAccess.ReadU32(m, c.S0);
        if (c.A0 != 0u) {
            goto L80018548;
        }
        L8001857C: ;
        c.V0 = c.S1 + 0u;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001859C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.A3 = c.A0 + 0u;
        c.A1 = 0u + 0u;
        c.A2 = c.SP + 0x10u;
        c.A0 = c.A3 + c.A1;
        L800185B4: ;
        c.V1 = MemoryAccess.ReadU8(m, c.A0);
        c.V0 = c.A2 + c.A1;
        MemoryAccess.WriteU8(m, c.V0, (byte)c.V1);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, c.A0);
        if (c.V0 == 0u) {
            c.V0 = c.A2 + c.A1;
            goto L800185E4;
        }
        c.V0 = c.A2 + c.A1;
        c.A1 = c.A1 + 0x1u;
        c.V0 = (int)c.A1 < 3 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.A0 = c.A3 + c.A1;
            goto L800185B4;
        }
        c.A0 = c.A3 + c.A1;
        c.V0 = c.A2 + c.A1;
        L800185E4: ;
        c.A1 = 0x80020000u;
        c.A0 = c.A2 + 0u;
        c.A1 = c.A1 + 0x2F1Cu;
        MemoryAccess.WriteU8(m, c.V0, (byte)0u);
        c.RA = 0x800185F8u;
        GranTurismo2PC.func_8008CF00(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.V0 = c.V0 < 0x00000001u ? 1u : 0u;
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80018608(CpuContext c, IMemory m)
    {
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, c.A0);
        c.V0 = c.V0 ^ 0x004Cu;
        c.V0 = c.V0 < 0x00000001u ? 1u : 0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001861C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        c.A1 = 0x80020000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.A1 = c.A1 + 0x2F20u;
        c.RA = 0x80018638u;
        GranTurismo2PC.func_8008CF00(c, m);
        if (c.V0 != 0u) {
            c.A0 = c.S0 + 0u;
            goto L80018648;
        }
        c.A0 = c.S0 + 0u;
        c.V0 = 0x00000007u;
        goto L80018680;
        L80018648: ;
        c.A1 = 0x80020000u;
        c.A1 = c.A1 + 0x2F28u;
        c.RA = 0x80018654u;
        GranTurismo2PC.func_8008CF00(c, m);
        if (c.V0 == 0u) {
            c.A0 = c.S0 + 0u;
            goto L8001867C;
        }
        c.A0 = c.S0 + 0u;
        c.A1 = 0x80020000u;
        c.A1 = c.A1 + 0x2F30u;
        c.RA = 0x80018668u;
        GranTurismo2PC.func_8008CF00(c, m);
        c.V1 = c.V0 + 0u;
        if (c.V1 == 0u) {
            c.V0 = 0x00000009u;
            goto L80018680;
        }
        c.V0 = 0x00000009u;
        c.V0 = 0xFFFFFFFFu;
        goto L80018680;
        L8001867C: ;
        c.V0 = 0x00000008u;
        L80018680: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80018690(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x60u;
        MemoryAccess.WriteU32(m, (c.SP + 0x54u), c.S1);
        c.S1 = c.A0 + 0u;
        c.A1 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x50u), c.S0);
        c.S0 = c.SP + 0x10u;
        MemoryAccess.WriteU32(m, (c.SP + 0x58u), c.RA);
        c.A0 = c.S1 + c.A1;
        L800186B0: ;
        c.V1 = MemoryAccess.ReadU8(m, c.A0);
        c.V0 = c.S0 + c.A1;
        MemoryAccess.WriteU8(m, c.V0, (byte)c.V1);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, c.A0);
        if (c.V0 == 0u) {
            c.V0 = c.S0 + c.A1;
            goto L800186E0;
        }
        c.V0 = c.S0 + c.A1;
        c.A1 = c.A1 + 0x1u;
        c.V0 = (int)c.A1 < 8 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.A0 = c.S1 + c.A1;
            goto L800186B0;
        }
        c.A0 = c.S1 + c.A1;
        c.V0 = c.S0 + c.A1;
        L800186E0: ;
        MemoryAccess.WriteU8(m, c.V0, (byte)0u);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x80020000u;
        c.A1 = c.A1 + 0x2F38u;
        c.RA = 0x800186F4u;
        GranTurismo2PC.func_8008CF00(c, m);
        if (c.V0 != 0u) {
            c.A2 = c.S0 + 0u;
            goto L80018704;
        }
        c.A2 = c.S0 + 0u;
        c.V0 = 0x00000001u;
        goto L80018754;
        L80018704: ;
        c.A1 = 0u + 0u;
        c.A0 = c.S1 + c.A1;
        L8001870C: ;
        c.V1 = MemoryAccess.ReadU8(m, c.A0);
        c.V0 = c.A2 + c.A1;
        MemoryAccess.WriteU8(m, c.V0, (byte)c.V1);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, c.A0);
        if (c.V0 == 0u) {
            c.V0 = c.A2 + c.A1;
            goto L8001873C;
        }
        c.V0 = c.A2 + c.A1;
        c.A1 = c.A1 + 0x1u;
        c.V0 = (int)c.A1 < 9 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.A0 = c.S1 + c.A1;
            goto L8001870C;
        }
        c.A0 = c.S1 + c.A1;
        c.V0 = c.A2 + c.A1;
        L8001873C: ;
        MemoryAccess.WriteU8(m, c.V0, (byte)0u);
        c.A0 = c.A2 + 0u;
        c.A1 = 0x80020000u;
        c.A1 = c.A1 + 0x2F44u;
        c.RA = 0x80018750u;
        GranTurismo2PC.func_8008CF00(c, m);
        c.V0 = c.V0 < 0x00000001u ? 1u : 0u;
        L80018754: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x58u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x54u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x50u));
        c.SP = c.SP + 0x60u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80018768(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        c.A1 = 0x80020000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.A1 = c.A1 + 0x2F50u;
        c.RA = 0x80018784u;
        GranTurismo2PC.func_8008CF00(c, m);
        if (c.V0 != 0u) {
            c.A0 = c.S0 + 0u;
            goto L80018794;
        }
        c.A0 = c.S0 + 0u;
        c.V0 = 0u + 0u;
        goto L800187CC;
        L80018794: ;
        c.A1 = 0x80020000u;
        c.A1 = c.A1 + 0x2F5Cu;
        c.RA = 0x800187A0u;
        GranTurismo2PC.func_8008CF00(c, m);
        if (c.V0 == 0u) {
            c.A0 = c.S0 + 0u;
            goto L800187C8;
        }
        c.A0 = c.S0 + 0u;
        c.A1 = 0x80020000u;
        c.A1 = c.A1 + 0x2F68u;
        c.RA = 0x800187B4u;
        GranTurismo2PC.func_8008CF00(c, m);
        c.V1 = c.V0 + 0u;
        if (c.V1 == 0u) {
            c.V0 = 0x00000002u;
            goto L800187CC;
        }
        c.V0 = 0x00000002u;
        c.V0 = 0x00000003u;
        goto L800187CC;
        L800187C8: ;
        c.V0 = 0x00000001u;
        L800187CC: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800187DC(CpuContext c, IMemory m)
    {
        c.V0 = 0x80050000u;
        c.V1 = MemoryAccess.ReadU16(m, (c.V0 + 0x1160u));
        c.A0 = 0x000000FFu;
        c.V0 = 0x800B0000u;
        c.V0 = c.V0 + 0x5C80u;
        c.V0 = c.V0 + 0x1FEu;
        L800187F4: ;
        MemoryAccess.WriteU16(m, c.V0, (ushort)c.V1);
        c.A0 = c.A0 - 0x1u;
        if ((int)c.A0 >= 0) {
            c.V0 = c.V0 - 0x2u;
            goto L800187F4;
        }
        c.V0 = c.V0 - 0x2u;
        c.V0 = 0x80050000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x1160u));
        c.A0 = c.V0 - 0x1u;
        if ((int)c.A0 < 0) {
            c.V0 = 0x800B0000u;
            goto L8001886C;
        }
        c.V0 = 0x800B0000u;
        c.A2 = c.V0 + 0x5C80u;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 + 0xD1Cu;
        c.V1 = c.A0 << 2;
        c.V1 = c.V1 + c.V0;
        L80018830: ;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, c.V0);
        c.V0 = c.V0 << 1;
        c.A1 = c.V0 + c.A2;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.A1);
        c.V0 = (int)c.A0 < (int)c.V0 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L80018860;
        }
        MemoryAccess.WriteU16(m, c.A1, (ushort)c.A0);
        L80018860: ;
        c.A0 = c.A0 - 0x1u;
        if ((int)c.A0 >= 0) {
            c.V1 = c.V1 - 0x4u;
            goto L80018830;
        }
        c.V1 = c.V1 - 0x4u;
        L8001886C: ;
        c.A0 = 0u + 0u;
        c.V0 = 0x80050000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0x1160u));
        c.V0 = 0x800B0000u;
        c.V1 = c.V0 + 0x5C80u;
        L80018880: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.V1);
        c.V0 = (int)c.V0 < (int)c.A1 ? 1u : 0u;
        if (c.V0 != 0u) {
            goto L80018898;
        }
        MemoryAccess.WriteU16(m, c.V1, (ushort)0u);
        L80018898: ;
        c.A0 = c.A0 + 0x1u;
        c.V0 = (int)c.A0 < 256 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V1 = c.V1 + 0x2u;
            goto L80018880;
        }
        c.V1 = c.V1 + 0x2u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800188B0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A0 + 0u;
        c.V1 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, c.S2);
        c.V1 = c.V1 + 0x5C80u;
        c.V0 = c.V0 << 1;
        c.V0 = c.V0 + c.V1;
        c.V1 = 0x80050000u;
        c.S0 = (uint)(short)MemoryAccess.ReadU16(m, c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x1160u));
        c.V0 = (int)c.S0 < (int)c.V0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.S3 = c.V1 + 0u;
            goto L80018934;
        }
        c.S3 = c.V1 + 0u;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 + 0xD1Cu;
        c.V1 = c.S0 << 2;
        c.S1 = c.V1 + c.V0;
        L8001890C: ;
        c.A0 = MemoryAccess.ReadU32(m, c.S1);
        c.A1 = c.S2 + 0u;
        c.RA = 0x80018918u;
        GranTurismo2PC.func_8008CF00(c, m);
        if (c.V0 == 0u) {
            c.V0 = c.S0 + 0u;
            goto L80018938;
        }
        c.V0 = c.S0 + 0u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S3 + 0x1160u));
        c.S0 = c.S0 + 0x1u;
        c.V0 = (int)c.S0 < (int)c.V0 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S1 = c.S1 + 0x4u;
            goto L8001890C;
        }
        c.S1 = c.S1 + 0x4u;
        L80018934: ;
        c.V0 = 0u + 0u;
        L80018938: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80018954(CpuContext c, IMemory m)
    {
        c.A0 = c.A0 - 0x30u;
        c.V1 = c.A0 < 0x0000000Au ? 1u : 0u;
        if (c.V1 == 0u) {
            c.V0 = 0u + 0u;
            goto L80018968;
        }
        c.V0 = 0u + 0u;
        c.V0 = c.A0 + 0u;
        L80018968: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80018970(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.A0 = c.S0 + 0u;
        c.RA = 0x80018990u;
        GranTurismo2PC.func_8008CFC4(c, m);
        c.A0 = c.V0 - 0x2u;
        if ((int)c.A0 <= 0) {
            c.V1 = 0u + 0u;
            goto L800189B8;
        }
        c.V1 = 0u + 0u;
        L8001899C: ;
        c.V0 = MemoryAccess.ReadU8(m, c.S0);
        c.S0 = c.S0 + 0x1u;
        c.V1 = c.V1 + 0x1u;
        MemoryAccess.WriteU8(m, c.S1, (byte)c.V0);
        c.V0 = (int)c.V1 < (int)c.A0 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S1 = c.S1 + 0x1u;
            goto L8001899C;
        }
        c.S1 = c.S1 + 0x1u;
        L800189B8: ;
        MemoryAccess.WriteU8(m, c.S1, (byte)0u);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800189D0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V0 = c.A1 + 0u;
        c.A3 = c.A2 + 0u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 + 0xC14u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A2 = c.V0 + 0u;
        c.RA = 0x800189F0u;
        GranTurismo2PC.func_8008CF34(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80018A00(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.RA = 0x80018A1Cu;
        GranTurismo2PC.func_8008CFC4(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = 0u + 0u;
            goto L80018A6C;
        }
        c.V0 = 0u + 0u;
        c.V0 = c.S0 + c.V1;
        c.S2 = c.V0 - 0x2u;
        c.S0 = 0u + 0u;
        c.S1 = c.S0 + 0u;
        c.V0 = c.S0 << 2;
        L80018A40: ;
        c.V0 = c.V0 + c.S0;
        c.S0 = c.V0 << 1;
        c.A0 = (uint)(sbyte)MemoryAccess.ReadU8(m, c.S2);
        c.S2 = c.S2 + 0x1u;
        c.S1 = c.S1 + 0x1u;
        c.RA = 0x80018A58u;
        GranTurismo2PC.func_80018954(c, m);
        c.S0 = c.S0 + c.V0;
        c.V0 = (int)c.S1 < 2 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = c.S0 << 2;
            goto L80018A40;
        }
        c.V0 = c.S0 << 2;
        c.V0 = c.S0 + 0u;
        L80018A6C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80018A84(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0xC0u;
        MemoryAccess.WriteU32(m, (c.SP + 0xBCu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0xB8u), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0xB4u), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0xB0u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0xACu), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0xA8u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0xA4u), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0xA0u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x9Cu), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x98u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0xC0u), c.A0);
        MemoryAccess.WriteU32(m, (c.SP + 0xC4u), c.A1);
        c.RA = 0x80018ABCu;
        GranTurismo2PC.func_8005E658(c, m);
        c.V0 = 0x80090000u;
        c.FP = MemoryAccess.ReadU32(m, (c.V0 + 0x2E70u));
        c.A0 = 0u + 0u;
        c.RA = 0x80018ACCu;
        GranTurismo2PC.func_8007D23C(c, m);
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xC4u));
        c.A0 = c.SP + 0x10u;
        MemoryAccess.WriteU32(m, (c.SP + 0x90u), c.V0);
        c.RA = 0x80018ADCu;
        GranTurismo2PC.func_80018970(c, m);
        c.A0 = c.SP + 0x10u;
        c.RA = 0x80018AE4u;
        GranTurismo2PC.func_80018A00(c, m);
        c.S2 = 0u + 0u;
        c.V1 = MemoryAccess.ReadU32(m, (c.SP + 0xC0u));
        c.S6 = c.V0 + 0u;
        if ((int)c.S6 <= 0) {
            MemoryAccess.WriteU16(m, (c.V1 + 0x2u), (ushort)c.S6);
            goto L80018BC4;
        }
        MemoryAccess.WriteU16(m, (c.V1 + 0x2u), (ushort)c.S6);
        c.V1 = (int)c.S6 < 2 ? 1u : 0u;
        c.S7 = c.SP + 0x50u;
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0xC0u));
        c.S5 = 0x00000028u;
        MemoryAccess.WriteU32(m, (c.SP + 0x94u), c.V1);
        c.S4 = c.S3 + 0u;
        L80018B10: ;
        c.V1 = MemoryAccess.ReadU32(m, (c.SP + 0x94u));
        if (c.V1 != 0u) {
            goto L80018B38;
        }
        c.A0 = c.SP + 0x50u;
        c.A1 = c.SP + 0x10u;
        c.A2 = c.S2 + 0x1u;
        c.RA = 0x80018B30u;
        GranTurismo2PC.func_800189D0(c, m);
        c.A0 = c.FP + 0u;
        goto L80018B48;
        L80018B38: ;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xC4u));
        c.A0 = c.SP + 0x50u;
        c.RA = 0x80018B44u;
        GranTurismo2PC.func_8008CEDC(c, m);
        c.A0 = c.FP + 0u;
        L80018B48: ;
        c.A1 = c.S7 + 0u;
        c.RA = 0x80018B50u;
        GranTurismo2PC.func_8007830C(c, m);
        c.S1 = c.V0 + 0u;
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x2u));
        c.A0 = c.FP + 0u;
        c.RA = 0x80018B60u;
        GranTurismo2PC.func_8007816C(c, m);
        c.S0 = c.V0 + 0u;
        c.A0 = c.S0 + 0u;
        c.V1 = 0x80020000u;
        c.A1 = c.V1 + 0x2F74u;
        c.RA = 0x80018B74u;
        GranTurismo2PC.func_8008CF00(c, m);
        if (c.V0 != 0u) {
            goto L80018B88;
        }
        c.A0 = c.SP + 0x90u;
        c.RA = 0x80018B84u;
        GranTurismo2PC.func_8001907C(c, m);
        c.S0 = c.V0 + 0u;
        L80018B88: ;
        c.A0 = c.S0 + 0u;
        c.V0 = MemoryAccess.ReadU8(m, (c.S1 + 0x45u));
        c.S2 = c.S2 + 0x1u;
        MemoryAccess.WriteU16(m, (c.S3 + 0x1Cu), (ushort)c.V0);
        c.RA = 0x80018B9Cu;
        GranTurismo2PC.func_80083004(c, m);
        c.V1 = MemoryAccess.ReadU32(m, (c.SP + 0xC0u));
        c.A1 = c.S7 + 0u;
        MemoryAccess.WriteU32(m, (c.S4 + 0x4u), c.V0);
        c.A0 = c.V1 + c.S5;
        c.RA = 0x80018BB0u;
        GranTurismo2PC.func_8008CEDC(c, m);
        c.S5 = c.S5 + 0x10u;
        c.S4 = c.S4 + 0x4u;
        c.V0 = (int)c.S2 < (int)c.S6 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S3 = c.S3 + 0x2u;
            goto L80018B10;
        }
        c.S3 = c.S3 + 0x2u;
        L80018BC4: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0xBCu));
        c.FP = MemoryAccess.ReadU32(m, (c.SP + 0xB8u));
        c.S7 = MemoryAccess.ReadU32(m, (c.SP + 0xB4u));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0xB0u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0xACu));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0xA8u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0xA4u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0xA0u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x9Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x98u));
        c.SP = c.SP + 0xC0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80018BF4(CpuContext c, IMemory m)
    {
        c.V1 = 0x00000005u;
        c.V0 = c.A0 + 0x14u;
        L80018BFC: ;
        MemoryAccess.WriteU32(m, (c.V0 + 0x4u), 0u);
        c.V1 = c.V1 - 0x1u;
        if ((int)c.V1 >= 0) {
            c.V0 = c.V0 - 0x4u;
            goto L80018BFC;
        }
        c.V0 = c.V0 - 0x4u;
        MemoryAccess.WriteU8(m, (c.A0 + 0x1Cu), (byte)0u);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80018C14(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x30u;
        c.A1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S2);
        c.S2 = 0u + 0u;
        c.A0 = c.SP + 0x10u;
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S0);
        c.RA = 0x80018C38u;
        GranTurismo2PC.func_8008CEDC(c, m);
        c.S0 = c.S2 + 0u;
        c.V0 = 0x80050000u;
        c.S1 = c.V0 + 0xC34u;
        MemoryAccess.WriteU8(m, (c.SP + 0x13u), (byte)0u);
        L80018C48: ;
        c.A0 = MemoryAccess.ReadU32(m, c.S1);
        c.A1 = c.SP + 0x10u;
        c.RA = 0x80018C54u;
        GranTurismo2PC.func_8008CF00(c, m);
        if (c.V0 != 0u) {
            goto L80018C60;
        }
        c.S2 = c.S0 + 0u;
        L80018C60: ;
        c.S0 = c.S0 + 0x1u;
        c.V0 = (int)c.S0 < 6 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S1 = c.S1 + 0x4u;
            goto L80018C48;
        }
        c.S1 = c.S1 + 0x4u;
        c.V0 = c.S2 + 0u;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x2Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.SP = c.SP + 0x30u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80018C8C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x170u;
        MemoryAccess.WriteU32(m, (c.SP + 0x148u), c.S0);
        c.S0 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x170u), c.A0);
        c.A0 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x16Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x168u), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0x164u), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x160u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x15Cu), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x158u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x154u), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x150u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14Cu), c.S1);
        c.RA = 0x80018CC8u;
        GranTurismo2PC.func_8007D23C(c, m);
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0x170u));
        MemoryAccess.WriteU32(m, (c.SP + 0x138u), c.V0);
        c.RA = 0x80018CD4u;
        GranTurismo2PC.func_80018BF4(c, m);
        c.A0 = c.S0 + 0u;
        c.RA = 0x80018CDCu;
        GranTurismo2PC.func_80018608(c, m);
        if (c.V0 == 0u) {
            c.A0 = c.SP + 0x28u;
            goto L80018D14;
        }
        c.A0 = c.SP + 0x28u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x80018CECu;
        GranTurismo2PC.func_80018C14(c, m);
        c.A0 = c.SP + 0x68u;
        c.V1 = 0x80050000u;
        c.V1 = c.V1 + 0xC1Cu;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.V1;
        c.A1 = MemoryAccess.ReadU32(m, c.V0);
        c.A2 = 0u + 0u;
        c.RA = 0x80018D0Cu;
        GranTurismo2PC.func_8008CF34(c, m);
        goto L80018D4C;
        L80018D14: ;
        c.A1 = c.S0 + 0u;
        c.RA = 0x80018D1Cu;
        GranTurismo2PC.func_80018970(c, m);
        c.A0 = c.SP + 0x28u;
        c.RA = 0x80018D24u;
        GranTurismo2PC.func_80018A00(c, m);
        c.V0 = (int)c.V0 < 2 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.A0 = c.SP + 0x68u;
            goto L80018D44;
        }
        c.A0 = c.SP + 0x68u;
        c.A1 = c.SP + 0x28u;
        c.A2 = 0x00000001u;
        c.RA = 0x80018D3Cu;
        GranTurismo2PC.func_800189D0(c, m);
        goto L80018D4C;
        L80018D44: ;
        c.A1 = c.S0 + 0u;
        c.RA = 0x80018D4Cu;
        GranTurismo2PC.func_8008CEDC(c, m);
        L80018D4C: ;
        c.A0 = c.SP + 0x68u;
        c.RA = 0x80018D54u;
        GranTurismo2PC.func_80021A6C(c, m);
        c.A1 = c.V0 + 0u;
        c.V1 = MemoryAccess.ReadU16(m, (c.A1 + 0x98u));
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x170u));
        c.V0 = c.V1 << 1;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 2;
        MemoryAccess.WriteU32(m, c.T0, c.V0);
        c.V1 = MemoryAccess.ReadU16(m, (c.A1 + 0x78u));
        c.V0 = c.V1 << 1;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 2;
        MemoryAccess.WriteU32(m, (c.T0 + 0x4u), c.V0);
        c.V1 = MemoryAccess.ReadU16(m, (c.A1 + 0x7Au));
        c.V0 = c.V1 << 1;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 2;
        MemoryAccess.WriteU32(m, (c.T0 + 0x8u), c.V0);
        c.V1 = MemoryAccess.ReadU16(m, (c.A1 + 0x7Cu));
        c.V0 = c.V1 << 1;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 2;
        MemoryAccess.WriteU32(m, (c.T0 + 0xCu), c.V0);
        c.V1 = MemoryAccess.ReadU16(m, (c.A1 + 0x7Eu));
        c.V0 = c.V1 << 1;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 2;
        MemoryAccess.WriteU32(m, (c.T0 + 0x10u), c.V0);
        c.V1 = MemoryAccess.ReadU16(m, (c.A1 + 0x80u));
        c.V0 = c.V1 << 1;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 2;
        MemoryAccess.WriteU32(m, (c.T0 + 0x14u), c.V0);
        c.V1 = MemoryAccess.ReadU16(m, (c.A1 + 0x82u));
        c.A0 = 0x00000005u;
        c.V0 = c.V1 << 1;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 2;
        MemoryAccess.WriteU32(m, (c.T0 + 0x18u), c.V0);
        c.A0 = c.A0 - 0x1u;
        L80018E3C: ;
        if ((int)c.A0 >= 0) {
            c.A0 = c.A0 - 0x1u;
            goto L80018E3C;
        }
        c.A0 = c.A0 - 0x1u;
        c.FP = c.A1 + 0x84u;
        c.RA = 0x80018E4Cu;
        GranTurismo2PC.func_80076F0C(c, m);
        MemoryAccess.WriteU32(m, (c.SP + 0x144u), c.V0);
        c.S7 = 0u + 0u;
        c.S6 = c.SP + 0xA8u;
        c.S5 = 0x00000020u;
        L80018E5C: ;
        c.V0 = (int)c.S7 < 4 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L80018F3C;
        }
        c.S4 = MemoryAccess.ReadU32(m, c.FP);
        if (c.S4 == 0u) {
            c.FP = c.FP + 0x4u;
            goto L80018F3C;
        }
        c.FP = c.FP + 0x4u;
        c.A0 = c.S4 + 0u;
        c.A1 = c.SP + 0x13Cu;
        c.A2 = c.SP + 0x140u;
        c.RA = 0x80018E88u;
        GranTurismo2PC.func_80060BEC(c, m);
        c.A0 = c.SP + 0x138u;
        c.S0 = c.V0 + 0u;
        c.RA = 0x80018E94u;
        GranTurismo2PC.func_80083AE0(c, m);
        if (c.S0 != 0u) { c.LO = c.V0 / c.S0; c.HI = c.V0 % c.S0; }
        c.V1 = c.HI;
        c.A0 = c.S4 + 0u;
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x140u));
        c.V0 = c.V0 + c.V1;
        c.S3 = (uint)(sbyte)MemoryAccess.ReadU8(m, c.V0);
        c.A1 = c.S6 + 0u;
        c.RA = 0x80018EB8u;
        GranTurismo2PC.func_80076954(c, m);
        c.A1 = 0x0000001Eu;
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0x144u));
        c.A2 = c.S4 + 0u;
        c.RA = 0x80018EC8u;
        GranTurismo2PC.func_80077DC4(c, m);
        c.A0 = c.S4 + 0u;
        c.S2 = MemoryAccess.ReadU32(m, (c.V0 + 0x44u));
        c.A1 = c.S6 + 0u;
        c.RA = 0x80018ED8u;
        GranTurismo2PC.func_80017750(c, m);
        c.A0 = c.S4 + 0u;
        c.S1 = c.V0 + 0u;
        c.RA = 0x80018EE4u;
        GranTurismo2PC.func_800178E4(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.SP + 0x130u;
        c.S0 = c.V0 + 0u;
        c.RA = 0x80018EF4u;
        GranTurismo2PC.func_8001781C(c, m);
        c.A1 = c.S4 + 0u;
        c.A2 = c.A1 + 0u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x170u));
        c.A3 = c.S3 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.V0);
        c.A0 = c.T0 + c.S5;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S2);
        c.RA = 0x80018F20u;
        GranTurismo2PC.func_8001EC0C(c, m);
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x170u));
        c.S7 = c.S7 + 0x1u;
        c.V0 = MemoryAccess.ReadU8(m, (c.T0 + 0x1Cu));
        c.S5 = c.S5 + 0xA4u;
        c.V0 = c.V0 + 0x1u;
        MemoryAccess.WriteU8(m, (c.T0 + 0x1Cu), (byte)c.V0);
        goto L80018E5C;
        L80018F3C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x16Cu));
        c.FP = MemoryAccess.ReadU32(m, (c.SP + 0x168u));
        c.S7 = MemoryAccess.ReadU32(m, (c.SP + 0x164u));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x160u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x15Cu));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x158u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x154u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x150u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x148u));
        c.SP = c.SP + 0x170u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80018F6C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V1 = 0x800B0000u;
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU32(m, (c.V1 + 0x5E80u), c.V0);
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x5E84u), (ushort)0u);
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU16(m, (c.V0 + 0x5E86u), (ushort)0u);
        c.RA = 0x80018F94u;
        GranTurismo2PC.func_80022874(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80018FA4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x5E80u));
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        if (c.V0 == c.S0) {
            MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
            goto L80018FDC;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.RA = 0x80018FCCu;
        GranTurismo2PC.func_80022934(c, m);
        c.V1 = 0x800B0000u;
        c.V0 = 0x00000002u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x5E80u), c.S0);
        MemoryAccess.WriteU16(m, (c.V1 + 0x5E84u), (ushort)c.V0);
        L80018FDC: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80018FF0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0x800B0000u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x5E86u));
        if (c.V0 == 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
            goto L80019018;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.RA = 0x80019014u;
        GranTurismo2PC.func_80022934(c, m);
        MemoryAccess.WriteU16(m, (c.S0 + 0x5E86u), (ushort)0u);
        L80019018: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019028(CpuContext c, IMemory m)
    {
        c.A0 = 0x800B0000u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0x5E84u));
        c.V1 = MemoryAccess.ReadU16(m, (c.A0 + 0x5E84u));
        c.SP = c.SP - 0x18u;
        if ((int)c.V0 <= 0) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
            goto L8001906C;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V0 = c.V1 - 0x1u;
        MemoryAccess.WriteU16(m, (c.A0 + 0x5E84u), (ushort)c.V0);
        c.V0 = c.V0 << 16;
        if (c.V0 != 0u) {
            c.V0 = 0x800B0000u;
            goto L8001906C;
        }
        c.V0 = 0x800B0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x5E80u));
        c.RA = 0x80019060u;
        GranTurismo2PC.func_800228D4(c, m);
        c.V1 = 0x800B0000u;
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU16(m, (c.V1 + 0x5E86u), (ushort)c.V0);
        L8001906C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001907C(CpuContext c, IMemory m)
    {
        c.V1 = 0x80050000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0xC4Cu));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0u + 0u;
        if (c.V0 == 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
            goto L800190B0;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.V1 = c.V1 + 0xC4Cu;
        L8001909C: ;
        c.V1 = c.V1 + 0x4u;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        if (c.V0 != 0u) {
            c.S0 = c.S0 + 0x1u;
            goto L8001909C;
        }
        c.S0 = c.S0 + 0x1u;
        L800190B0: ;
        c.RA = 0x800190B8u;
        GranTurismo2PC.func_80083AE0(c, m);
        if (c.S0 != 0u) { c.LO = c.V0 / c.S0; c.HI = c.V0 % c.S0; }
        c.V1 = c.HI;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 + 0xC4Cu;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800190E4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x30u;
        c.A1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S2);
        c.S2 = 0u + 0u;
        c.A0 = c.SP + 0x10u;
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S0);
        c.RA = 0x80019108u;
        GranTurismo2PC.func_8008CEDC(c, m);
        c.S0 = c.S2 + 0u;
        c.V0 = 0x80050000u;
        c.S1 = c.V0 + 0xD04u;
        MemoryAccess.WriteU8(m, (c.SP + 0x13u), (byte)0u);
        L80019118: ;
        c.A0 = MemoryAccess.ReadU32(m, c.S1);
        c.A1 = c.SP + 0x10u;
        c.RA = 0x80019124u;
        GranTurismo2PC.func_8008CF00(c, m);
        if (c.V0 != 0u) {
            goto L80019130;
        }
        c.S2 = c.S0 + 0u;
        L80019130: ;
        c.S0 = c.S0 + 0x1u;
        c.V0 = (int)c.S0 < 6 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S1 = c.S1 + 0x4u;
            goto L80019118;
        }
        c.S1 = c.S1 + 0x4u;
        c.V0 = c.S2 + 0u;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x2Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.SP = c.SP + 0x30u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001915C(CpuContext c, IMemory m)
    {
        c.A3 = 0x00000001u;
        c.A2 = 0u + 0u;
        c.A1 = 0x801D0000u;
        c.A1 = c.A1 - 0x6720u;
        c.V0 = c.A0 << (int)(c.A3 & 31u);
        c.V0 = c.V0 + c.A0;
        c.V1 = c.V0 << 4;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + 0x1418u;
        c.V1 = c.V0 + c.A1;
        L80019190: ;
        c.V0 = (int)c.A2 < 10 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L800191BC;
        }
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V1 + 0x1u));
        if (c.V0 != 0u) {
            goto L800191B0;
        }
        c.A3 = 0u + 0u;
        L800191B0: ;
        c.V1 = c.V1 + 0xA4u;
        c.A2 = c.A2 + 0x1u;
        goto L80019190;
        L800191BC: ;
        c.V0 = c.A3 + 0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800191C4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = 0u + 0u;
        c.RA = 0x800191D4u;
        GranTurismo2PC.func_8001915C(c, m);
        if (c.V0 != 0u) {
            c.V0 = 0u + 0u;
            goto L8001923C;
        }
        c.V0 = 0u + 0u;
        c.A0 = 0x00000001u;
        c.RA = 0x800191E4u;
        GranTurismo2PC.func_8001915C(c, m);
        if (c.V0 != 0u) {
            c.V0 = 0x00000001u;
            goto L8001923C;
        }
        c.V0 = 0x00000001u;
        c.A0 = 0x00000002u;
        c.RA = 0x800191F4u;
        GranTurismo2PC.func_8001915C(c, m);
        if (c.V0 != 0u) {
            c.V0 = 0x00000002u;
            goto L8001923C;
        }
        c.V0 = 0x00000002u;
        c.A0 = 0x00000003u;
        c.RA = 0x80019204u;
        GranTurismo2PC.func_8001915C(c, m);
        if (c.V0 != 0u) {
            c.V0 = 0x00000003u;
            goto L8001923C;
        }
        c.V0 = 0x00000003u;
        c.A0 = 0x00000004u;
        c.RA = 0x80019214u;
        GranTurismo2PC.func_8001915C(c, m);
        if (c.V0 != 0u) {
            goto L80019238;
        }
        c.A0 = 0x00000005u;
        c.RA = 0x80019224u;
        GranTurismo2PC.func_8001915C(c, m);
        c.V1 = c.V0 + 0u;
        if (c.V1 != 0u) {
            c.V0 = 0x00000005u;
            goto L8001923C;
        }
        c.V0 = 0x00000005u;
        c.V0 = 0x00000006u;
        goto L8001923C;
        L80019238: ;
        c.V0 = 0x00000004u;
        L8001923C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001924C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V0 = 0x80090000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x2870u));
        c.A2 = c.A0 - 0x1u;
        if ((int)c.A2 < 0) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
            goto L80019278;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = c.V0 + 0u;
        c.A1 = 0x00000002u;
        c.RA = 0x80019270u;
        GranTurismo2PC.func_80077D5C(c, m);
        goto L8001927C;
        L80019278: ;
        c.V0 = 0u + 0u;
        L8001927C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001928C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        c.V0 = 0x80090000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x2E70u));
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A3 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.RA = 0x800192BCu;
        GranTurismo2PC.func_8007830C(c, m);
        c.A0 = MemoryAccess.ReadU16(m, (c.V0 + 0x98u));
        c.V1 = c.A0 << 1;
        c.V1 = c.V1 + c.A0;
        c.V1 = c.V1 << 3;
        c.V1 = c.V1 + c.A0;
        c.V1 = c.V1 << 2;
        MemoryAccess.WriteU32(m, c.S1, c.V1);
        c.A0 = MemoryAccess.ReadU16(m, (c.V0 + 0x78u));
        c.V1 = c.A0 << 1;
        c.V1 = c.V1 + c.A0;
        c.V1 = c.V1 << 3;
        c.V1 = c.V1 + c.A0;
        c.V1 = c.V1 << 2;
        MemoryAccess.WriteU32(m, (c.S1 + 0x4u), c.V1);
        c.A0 = MemoryAccess.ReadU16(m, (c.V0 + 0x7Au));
        c.V1 = c.A0 << 1;
        c.V1 = c.V1 + c.A0;
        c.V1 = c.V1 << 3;
        c.V1 = c.V1 + c.A0;
        c.V1 = c.V1 << 2;
        MemoryAccess.WriteU32(m, (c.S1 + 0x8u), c.V1);
        c.A0 = MemoryAccess.ReadU16(m, (c.V0 + 0x7Cu));
        c.V1 = c.A0 << 1;
        c.V1 = c.V1 + c.A0;
        c.V1 = c.V1 << 3;
        c.V1 = c.V1 + c.A0;
        c.V1 = c.V1 << 2;
        MemoryAccess.WriteU32(m, (c.S1 + 0xCu), c.V1);
        c.A0 = MemoryAccess.ReadU16(m, (c.V0 + 0x7Eu));
        c.V1 = c.A0 << 1;
        c.V1 = c.V1 + c.A0;
        c.V1 = c.V1 << 3;
        c.V1 = c.V1 + c.A0;
        c.V1 = c.V1 << 2;
        MemoryAccess.WriteU32(m, (c.S1 + 0x10u), c.V1);
        c.A0 = MemoryAccess.ReadU16(m, (c.V0 + 0x80u));
        c.V1 = c.A0 << 1;
        c.V1 = c.V1 + c.A0;
        c.V1 = c.V1 << 3;
        c.V1 = c.V1 + c.A0;
        c.V1 = c.V1 << 2;
        MemoryAccess.WriteU32(m, (c.S1 + 0x14u), c.V1);
        c.A0 = MemoryAccess.ReadU16(m, (c.V0 + 0x82u));
        c.V1 = c.A0 << 1;
        c.V1 = c.V1 + c.A0;
        c.V1 = c.V1 << 3;
        c.V1 = c.V1 + c.A0;
        c.V1 = c.V1 << 2;
        MemoryAccess.WriteU32(m, (c.S1 + 0x18u), c.V1);
        c.A0 = MemoryAccess.ReadU8(m, (c.V0 + 0x76u));
        c.T1 = MemoryAccess.ReadU8(m, (c.V0 + 0x75u));
        c.V1 = MemoryAccess.ReadU8(m, (c.V0 + 0x47u));
        c.A1 = MemoryAccess.ReadU8(m, (c.V0 + 0x77u));
        c.T0 = MemoryAccess.ReadU16(m, (c.V0 + 0x96u));
        c.A2 = MemoryAccess.ReadU8(m, (c.V0 + 0x9Au));
        c.A3 = MemoryAccess.ReadU8(m, (c.V0 + 0x9Bu));
        c.V0 = MemoryAccess.ReadU16(m, (c.S1 + 0x1Cu));
        c.S0 = 0u + 0u;
        MemoryAccess.WriteU16(m, (c.S1 + 0x20u), (ushort)c.S2);
        MemoryAccess.WriteU8(m, (c.S1 + 0x22u), (byte)0u);
        c.V0 = c.V0 & 0xFFFEu;
        c.V0 = c.V0 | c.T1;
        c.V0 = c.V0 & 0xFFF1u;
        c.V1 = c.V1 << 1;
        c.V0 = c.V0 | c.V1;
        c.V0 = c.V0 & 0xFF8Fu;
        c.A1 = c.A1 << 4;
        c.V0 = c.V0 | c.A1;
        c.V0 = c.V0 & 0xFE7Fu;
        c.A2 = c.A2 << 7;
        c.V0 = c.V0 | c.A2;
        c.V0 = c.V0 & 0xF9FFu;
        c.A3 = c.A3 << 9;
        c.V0 = c.V0 | c.A3;
        MemoryAccess.WriteU16(m, (c.S1 + 0x1Eu), (ushort)c.T0);
        MemoryAccess.WriteU16(m, (c.S1 + 0x1Cu), (ushort)c.V0);
        c.RA = 0x8001940Cu;
        GranTurismo2PC.func_8001924C(c, m);
        if (c.V0 == 0u) {
            goto L80019454;
        }
        c.A0 = c.S0 + 0u;
        c.V1 = c.V0 + 0u;
        c.V0 = c.S2 << 2;
        c.A2 = c.V0 + c.S3;
        L80019424: ;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        if (c.V0 == 0u) {
            goto L80019450;
        }
        MemoryAccess.WriteU32(m, c.A2, c.V0);
        c.A2 = c.A2 + 0x4u;
        c.S0 = c.S0 + 0x1u;
        c.A0 = c.A0 + 0x1u;
        c.V0 = (int)c.A0 < 32 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V1 = c.V1 + 0x4u;
            goto L80019424;
        }
        c.V1 = c.V1 + 0x4u;
        L80019450: ;
        MemoryAccess.WriteU8(m, (c.S1 + 0x22u), (byte)c.S0);
        L80019454: ;
        c.V0 = c.S0 + 0u;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019474(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.S3 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        c.S4 = 0x800C0000u;
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.V0 + 0xD1Cu;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.V0 + 0x5E88u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.RA);
        L800194AC: ;
        c.A1 = MemoryAccess.ReadU32(m, c.S1);
        c.S1 = c.S1 + 0x4u;
        c.A0 = c.S0 + 0u;
        c.S0 = c.S0 + 0x24u;
        c.S2 = c.S2 + 0x1u;
        c.A2 = c.S4 - 0x7E98u;
        c.A3 = c.S3 + 0u;
        c.RA = 0x800194CCu;
        GranTurismo2PC.func_8001928C(c, m);
        c.S3 = c.S3 + c.V0;
        c.V0 = (int)c.S2 < 248 ? 1u : 0u;
        if (c.V0 != 0u) {
            goto L800194AC;
        }
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
    public static void func_800194FC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.RA = 0x8001950Cu;
        GranTurismo2PC.func_800188B0(c, m);
        c.V1 = c.V0 << 3;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 2;
        c.V0 = 0x800B0000u;
        c.V0 = c.V0 + 0x5E88u;
        c.V1 = c.V1 + c.V0;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V1 + 0x1Eu));
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019538(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.RA = 0x80019548u;
        GranTurismo2PC.func_800188B0(c, m);
        c.V1 = c.V0 << 3;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 2;
        c.V0 = 0x800B0000u;
        c.V0 = c.V0 + 0x5E88u;
        c.V1 = c.V1 + c.V0;
        c.V1 = MemoryAccess.ReadU16(m, (c.V1 + 0x1Cu));
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.V0 = c.V1 & 0x0001u;
        MemoryAccess.WriteU16(m, (c.SP + 0x10u), (ushort)c.V1);
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019578(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.RA = 0x80019588u;
        GranTurismo2PC.func_800188B0(c, m);
        c.V1 = c.V0 << 3;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 2;
        c.V0 = 0x800B0000u;
        c.V0 = c.V0 + 0x5E88u;
        c.V1 = c.V1 + c.V0;
        c.V1 = MemoryAccess.ReadU16(m, (c.V1 + 0x1Cu));
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.V0 = c.V1 & 0x0001u;
        c.V0 = c.V0 ^ 0x0001u;
        MemoryAccess.WriteU16(m, (c.SP + 0x10u), (ushort)c.V1);
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800195BC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.RA = 0x800195CCu;
        GranTurismo2PC.func_800188B0(c, m);
        c.V1 = c.V0 << 3;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 2;
        c.V0 = 0x800B0000u;
        c.V0 = c.V0 + 0x5E88u;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 + 0x1Cu));
        MemoryAccess.WriteU16(m, (c.SP + 0x10u), (ushort)c.V0);
        c.V0 = c.V0 & 0x0180u;
        c.A0 = c.V0 >> 7;
        c.V0 = 0x00000001u;
        if (c.A0 == c.V0) {
            c.V1 = 0u + 0u;
            goto L80019618;
        }
        c.V1 = 0u + 0u;
        c.V0 = 0x00000002u;
        if (c.A0 == c.V0) {
            goto L80019620;
        }
        goto L80019624;
        L80019618: ;
        c.V1 = 0x00000001u;
        goto L80019624;
        L80019620: ;
        c.V1 = 0x00000002u;
        L80019624: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.V0 = c.V1 + 0u;
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019634(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x8u;
        c.V0 = c.A0 << 3;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V1 = 0x800B0000u;
        c.V1 = c.V1 + 0x5E88u;
        c.V0 = c.V0 + c.V1;
        c.V0 = MemoryAccess.ReadU16(m, (c.V0 + 0x1Cu));
        c.A0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU16(m, c.SP, (ushort)c.V0);
        c.V0 = c.V0 & 0x000Eu;
        c.V0 = c.V0 >> 1;
        c.V1 = c.V0 + c.A0;
        c.V0 = c.V1 < 0x00000006u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80020000u;
            goto L800196BC;
        }
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x39BCu;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x80019690u: goto L80019690;
            case 0x80019698u: goto L80019698;
            case 0x800196A0u: goto L800196A0;
            case 0x800196A8u: goto L800196A8;
            case 0x800196B0u: goto L800196B0;
            case 0x800196B8u: goto L800196B8;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L80019690: ;
        c.A0 = 0x00000005u;
        goto L800196BC;
        L80019698: ;
        c.A0 = 0x00000004u;
        goto L800196BC;
        L800196A0: ;
        c.A0 = 0x00000003u;
        goto L800196BC;
        L800196A8: ;
        c.A0 = 0x00000002u;
        goto L800196BC;
        L800196B0: ;
        c.A0 = 0x00000001u;
        goto L800196BC;
        L800196B8: ;
        c.A0 = 0u + 0u;
        L800196BC: ;
        c.V0 = c.A0 + 0u;
        c.SP = c.SP + 0x8u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800196C8(CpuContext c, IMemory m)
    {
        c.V0 = c.A0 << 3;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V1 = 0x800B0000u;
        c.V1 = c.V1 + 0x5E88u;
        c.V0 = c.V0 + c.V1;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x1Eu));
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800196EC(CpuContext c, IMemory m)
    {
        c.V0 = c.A0 << 3;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V1 = 0x800B0000u;
        c.V1 = c.V1 + 0x5E88u;
        c.V0 = c.V0 + c.V1;
        c.A1 = c.A1 << 2;
        c.V0 = c.V0 + c.A1;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019718(CpuContext c, IMemory m)
    {
        c.V0 = c.A0 << 3;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V1 = 0x800B0000u;
        c.V1 = c.V1 + 0x5E88u;
        c.V0 = c.V0 + c.V1;
        c.V0 = MemoryAccess.ReadU32(m, c.V0);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001973C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x48u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S2);
        c.S2 = c.A1 + 0u;
        c.V1 = 0x80000000u;
        c.V0 = 0x801D0000u;
        c.V0 = c.V0 - 0x6720u;
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.S4);
        c.S4 = c.V0 + 0x3C74u;
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0x3Cu), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x38u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x34u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S1);
        c.S1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S4 + 0x4018u));
        c.V1 = c.V1 | 0x0002u;
        if ((int)c.S1 >= 0) {
            MemoryAccess.WriteU32(m, c.S2, c.V1);
            goto L80019798;
        }
        MemoryAccess.WriteU32(m, c.S2, c.V1);
        c.V0 = 0xFFFFFFFFu;
        goto L80019B58;
        L80019798: ;
        c.A0 = c.S0 + 0u;
        c.RA = 0x800197A0u;
        GranTurismo2PC.func_800183EC(c, m);
        if (c.V0 == 0u) {
            c.V0 = 0x80000000u;
            goto L800197C0;
        }
        c.V0 = 0x80000000u;
        c.V0 = c.V0 | 0x002Fu;
        MemoryAccess.WriteU32(m, c.S2, c.V0);
        c.RA = 0x800197B4u;
        GranTurismo2PC.func_8001847C(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 != 0u) {
            c.V0 = 0xFFFFFFF7u;
            goto L80019B58;
        }
        c.V0 = 0xFFFFFFF7u;
        L800197C0: ;
        c.A0 = c.S0 + 0u;
        c.RA = 0x800197C8u;
        GranTurismo2PC.func_8001859C(c, m);
        if (c.V0 == 0u) {
            c.V0 = 0x80000000u;
            goto L800197E8;
        }
        c.V0 = 0x80000000u;
        c.V0 = c.V0 | 0x0030u;
        MemoryAccess.WriteU32(m, c.S2, c.V0);
        c.RA = 0x800197DCu;
        GranTurismo2PC.func_8001850C(c, m);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 != 0u) {
            c.V0 = 0xFFFFFFF6u;
            goto L80019B58;
        }
        c.V0 = 0xFFFFFFF6u;
        L800197E8: ;
        c.A0 = c.S0 + 0u;
        c.RA = 0x800197F0u;
        GranTurismo2PC.func_800188B0(c, m);
        c.V1 = c.V0 << 3;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 2;
        c.V0 = 0x800B0000u;
        c.V0 = c.V0 + 0x5E88u;
        c.S3 = c.V1 + c.V0;
        c.S0 = c.S1 << 2;
        c.S0 = c.S0 + c.S1;
        c.S0 = c.S0 << 3;
        c.S0 = c.S0 + c.S1;
        c.S0 = c.S0 << 2;
        c.S0 = c.S0 + 0x4u;
        c.S0 = c.S4 + c.S0;
        c.V0 = MemoryAccess.ReadU16(m, (c.S0 + 0x98u));
        c.V1 = MemoryAccess.ReadU16(m, (c.S0 + 0x94u));
        c.S7 = MemoryAccess.ReadU32(m, c.S0);
        c.FP = c.V0 & 0x3FFFu;
        c.S4 = c.V1 >> 13;
        c.RA = 0x8001983Cu;
        GranTurismo2PC.func_800191C4(c, m);
        c.V1 = 0x00000006u;
        c.S1 = c.V1 - c.V0;
        c.RA = 0x80019848u;
        GranTurismo2PC.func_800175B0(c, m);
        c.S6 = c.V0 + 0u;
        c.RA = 0x80019850u;
        GranTurismo2PC.func_800175A0(c, m);
        c.S5 = c.V0 + 0u;
        c.RA = 0x80019858u;
        GranTurismo2PC.func_800175C0(c, m);
        c.A0 = c.S0 + 0u;
        c.S0 = c.V0 + 0u;
        c.A1 = 0x0000002Du;
        c.RA = 0x80019868u;
        GranTurismo2PC.func_8005E874(c, m);
        c.A0 = c.V0 + 0u;
        c.V1 = MemoryAccess.ReadU16(m, (c.S3 + 0x1Cu));
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU16(m, (c.SP + 0x10u), (ushort)c.V1);
        c.V1 = c.V1 & 0x0001u;
        if (c.V1 != c.V0) {
            goto L8001989C;
        }
        if (c.A0 != 0u) {
            c.V0 = 0x80000000u;
            goto L8001989C;
        }
        c.V0 = 0x80000000u;
        c.V0 = c.V0 | 0x0025u;
        MemoryAccess.WriteU32(m, c.S2, c.V0);
        c.V0 = 0xFFFFFFFEu;
        goto L80019B58;
        L8001989C: ;
        c.V0 = MemoryAccess.ReadU16(m, (c.S3 + 0x1Cu));
        MemoryAccess.WriteU16(m, (c.SP + 0x12u), (ushort)c.V0);
        c.V0 = c.V0 & 0x000Eu;
        c.V1 = c.V0 >> 1;
        c.V0 = c.V1 < 0x00000007u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80020000u;
            goto L80019954;
        }
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x39D4u;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x80019954u: goto L80019954;
            case 0x800198D8u: goto L800198D8;
            case 0x800198E8u: goto L800198E8;
            case 0x800198FCu: goto L800198FC;
            case 0x80019910u: goto L80019910;
            case 0x80019924u: goto L80019924;
            case 0x80019938u: goto L80019938;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L800198D8: ;
        if ((int)c.S1 > 0) {
            c.V0 = 0x80000000u;
            goto L80019954;
        }
        c.V0 = 0x80000000u;
        c.V0 = c.V0 | 0x0013u;
        goto L80019948;
        L800198E8: ;
        c.V0 = (int)c.S1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80000000u;
            goto L80019954;
        }
        c.V0 = 0x80000000u;
        c.V0 = c.V0 | 0x0012u;
        goto L80019948;
        L800198FC: ;
        c.V0 = (int)c.S1 < 3 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80000000u;
            goto L80019954;
        }
        c.V0 = 0x80000000u;
        c.V0 = c.V0 | 0x0014u;
        goto L80019948;
        L80019910: ;
        c.V0 = (int)c.S1 < 4 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80000000u;
            goto L80019954;
        }
        c.V0 = 0x80000000u;
        c.V0 = c.V0 | 0x0016u;
        goto L80019948;
        L80019924: ;
        c.V0 = (int)c.S1 < 5 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80000000u;
            goto L80019954;
        }
        c.V0 = 0x80000000u;
        c.V0 = c.V0 | 0x0015u;
        goto L80019948;
        L80019938: ;
        c.V0 = (int)c.S1 < 6 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80000000u;
            goto L80019954;
        }
        c.V0 = 0x80000000u;
        c.V0 = c.V0 | 0x0017u;
        L80019948: ;
        MemoryAccess.WriteU32(m, c.S2, c.V0);
        c.V0 = 0xFFFFFFFDu;
        goto L80019B58;
        L80019954: ;
        c.V0 = 0x80000000u;
        c.V0 = c.V0 | 0x0023u;
        MemoryAccess.WriteU32(m, c.S2, c.V0);
        c.V0 = MemoryAccess.ReadU16(m, (c.S3 + 0x1Cu));
        MemoryAccess.WriteU16(m, (c.SP + 0x14u), (ushort)c.V0);
        c.V0 = c.V0 & 0x0070u;
        c.V1 = c.V0 >> 4;
        c.V0 = c.V1 < 0x00000006u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80020000u;
            goto L800199D0;
        }
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x39F4u;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x800199D0u: goto L800199D0;
            case 0x8001999Cu: goto L8001999C;
            case 0x800199A4u: goto L800199A4;
            case 0x800199B4u: goto L800199B4;
            case 0x800199BCu: goto L800199BC;
            case 0x800199C4u: goto L800199C4;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L8001999C: ;
        c.V0 = 0x00000001u;
        goto L800199C8;
        L800199A4: ;
        if (c.S4 == 0u) {
            c.V0 = 0xFFFFFFFCu;
            goto L800199D0;
        }
        c.V0 = 0xFFFFFFFCu;
        goto L80019B58;
        L800199B4: ;
        c.V0 = 0x00000003u;
        goto L800199C8;
        L800199BC: ;
        c.V0 = 0x00000004u;
        goto L800199C8;
        L800199C4: ;
        c.V0 = 0x00000002u;
        L800199C8: ;
        if (c.S4 != c.V0) {
            c.V0 = 0xFFFFFFFCu;
            goto L80019B58;
        }
        c.V0 = 0xFFFFFFFCu;
        L800199D0: ;
        c.V0 = MemoryAccess.ReadU16(m, (c.S3 + 0x1Cu));
        MemoryAccess.WriteU16(m, (c.SP + 0x16u), (ushort)c.V0);
        c.V0 = c.V0 & 0x0180u;
        c.V1 = c.V0 >> 7;
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L80019A0C;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = 0x80000000u;
            goto L80019A38;
        }
        c.V0 = 0x80000000u;
        c.V0 = 0x00000002u;
        if (c.V1 == c.V0) {
            c.V0 = 0x80000000u;
            goto L80019A24;
        }
        c.V0 = 0x80000000u;
        c.V0 = c.V0 | 0x001Eu;
        goto L80019A3C;
        L80019A0C: ;
        c.V0 = 0x80000000u;
        c.V0 = c.V0 | 0x0026u;
        if (c.S5 != 0u) {
            MemoryAccess.WriteU32(m, c.S2, c.V0);
            goto L80019A34;
        }
        MemoryAccess.WriteU32(m, c.S2, c.V0);
        c.V0 = 0xFFFFFFFBu;
        goto L80019B58;
        L80019A24: ;
        c.V0 = c.V0 | 0x0027u;
        MemoryAccess.WriteU32(m, c.S2, c.V0);
        if (c.S5 != 0u) {
            c.V0 = 0xFFFFFFFBu;
            goto L80019B58;
        }
        c.V0 = 0xFFFFFFFBu;
        L80019A34: ;
        c.V0 = 0x80000000u;
        L80019A38: ;
        c.V0 = c.V0 | 0x001Eu;
        L80019A3C: ;
        MemoryAccess.WriteU32(m, c.S2, c.V0);
        c.V0 = MemoryAccess.ReadU16(m, (c.S3 + 0x1Cu));
        MemoryAccess.WriteU16(m, (c.SP + 0x18u), (ushort)c.V0);
        c.V0 = c.V0 & 0x0600u;
        c.V1 = c.V0 >> 9;
        c.V0 = 0x00000001u;
        if (c.V1 == c.V0) {
            c.V0 = (int)c.V1 < 2 ? 1u : 0u;
            goto L80019A84;
        }
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = 0x80000000u;
            goto L80019AC8;
        }
        c.V0 = 0x80000000u;
        c.V0 = 0x00000002u;
        if (c.V1 == c.V0) {
            c.V0 = 0x00000003u;
            goto L80019A9C;
        }
        c.V0 = 0x00000003u;
        if (c.V1 == c.V0) {
            c.V0 = 0x80000000u;
            goto L80019AB4;
        }
        c.V0 = 0x80000000u;
        c.V0 = c.V0 | 0x002Bu;
        goto L80019ACC;
        L80019A84: ;
        c.V0 = 0x80000000u;
        c.V0 = c.V0 | 0x0029u;
        if (c.S6 == 0u) {
            MemoryAccess.WriteU32(m, c.S2, c.V0);
            goto L80019AC4;
        }
        MemoryAccess.WriteU32(m, c.S2, c.V0);
        c.V0 = 0xFFFFFFFAu;
        goto L80019B58;
        L80019A9C: ;
        c.V0 = 0x80000000u;
        c.V0 = c.V0 | 0x0028u;
        if (c.S6 != 0u) {
            MemoryAccess.WriteU32(m, c.S2, c.V0);
            goto L80019AC4;
        }
        MemoryAccess.WriteU32(m, c.S2, c.V0);
        c.V0 = 0xFFFFFFFAu;
        goto L80019B58;
        L80019AB4: ;
        c.V0 = c.V0 | 0x002Cu;
        MemoryAccess.WriteU32(m, c.S2, c.V0);
        if (c.S0 == 0u) {
            c.V0 = 0xFFFFFFFAu;
            goto L80019B58;
        }
        c.V0 = 0xFFFFFFFAu;
        L80019AC4: ;
        c.V0 = 0x80000000u;
        L80019AC8: ;
        c.V0 = c.V0 | 0x002Bu;
        L80019ACC: ;
        MemoryAccess.WriteU32(m, c.S2, c.V0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S3 + 0x1Eu));
        if ((int)c.V0 <= 0) {
            c.V0 = (int)c.V0 < (int)c.FP ? 1u : 0u;
            goto L80019AE8;
        }
        c.V0 = (int)c.V0 < (int)c.FP ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = 0xFFFFFFF9u;
            goto L80019B58;
        }
        c.V0 = 0xFFFFFFF9u;
        L80019AE8: ;
        c.V0 = 0x80000000u;
        c.V0 = c.V0 | 0x001Eu;
        MemoryAccess.WriteU32(m, c.S2, c.V0);
        c.V0 = MemoryAccess.ReadU8(m, (c.S3 + 0x22u));
        if ((int)c.V0 <= 0) {
            c.A0 = 0u + 0u;
            goto L80019B48;
        }
        c.A0 = 0u + 0u;
        c.V1 = MemoryAccess.ReadU16(m, (c.S3 + 0x20u));
        c.A1 = c.V0 + 0u;
        c.V0 = 0x800C0000u;
        c.A2 = c.V0 - 0x7E98u;
        c.V0 = (int)c.A0 < (int)c.A1 ? 1u : 0u;
        L80019B18: ;
        if (c.V0 == 0u) {
            c.V0 = c.V1 + c.A0;
            goto L80019B40;
        }
        c.V0 = c.V1 + c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.A2;
        c.V0 = MemoryAccess.ReadU32(m, c.V0);
        if (c.V0 == c.S7) {
            c.A0 = c.A0 + 0x1u;
            goto L80019B48;
        }
        c.A0 = c.A0 + 0x1u;
        c.V0 = (int)c.A0 < (int)c.A1 ? 1u : 0u;
        goto L80019B18;
        L80019B40: ;
        c.V0 = 0xFFFFFFF8u;
        goto L80019B58;
        L80019B48: ;
        c.V0 = 0x80000000u;
        c.V0 = c.V0 | 0x001Eu;
        MemoryAccess.WriteU32(m, c.S2, c.V0);
        c.V0 = 0x00000001u;
        L80019B58: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x44u));
        c.FP = MemoryAccess.ReadU32(m, (c.SP + 0x40u));
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
    public static void func_80019B88(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.S0 = c.A1 + 0u;
        c.RA = 0x80019B9Cu;
        GranTurismo2PC.func_800190E4(c, m);
        c.A0 = c.V0 + 0u;
        c.V1 = 0x80050000u;
        c.V1 = c.V1 + 0x1164u;
        c.V0 = c.A0 << 2;
        c.V0 = c.V0 + c.V1;
        c.V0 = MemoryAccess.ReadU32(m, c.V0);
        MemoryAccess.WriteU32(m, c.S0, c.V0);
        c.V0 = (int)c.A0 < 5 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L80019BDC;
        }
        c.A0 = c.A0 + 0x1u;
        c.RA = 0x80019BD0u;
        GranTurismo2PC.func_8001915C(c, m);
        c.V1 = c.V0 ^ 0x0001u;
        if (c.V1 != 0u) {
            c.V0 = 0xFFFFFFFDu;
            goto L80019BE0;
        }
        c.V0 = 0xFFFFFFFDu;
        L80019BDC: ;
        c.V0 = 0x00000001u;
        L80019BE0: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019BF0(CpuContext c, IMemory m)
    {
        c.A1 = 0x80050000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.A1 + 0x117Cu));
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        if (c.V0 == 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
            goto L80019C48;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A1 + 0x117Cu;
        L80019C14: ;
        c.A1 = MemoryAccess.ReadU32(m, c.S0);
        c.A0 = c.S1 + 0u;
        c.RA = 0x80019C20u;
        GranTurismo2PC.func_8008CF00(c, m);
        if (c.V0 != 0u) {
            goto L80019C34;
        }
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x4u));
        goto L80019C4C;
        L80019C34: ;
        c.S0 = c.S0 + 0x8u;
        c.V0 = MemoryAccess.ReadU32(m, c.S0);
        if (c.V0 != 0u) {
            goto L80019C14;
        }
        L80019C48: ;
        c.V0 = 0x000000A6u;
        L80019C4C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019C60(CpuContext c, IMemory m)
    {
        c.V0 = 0x801D0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x6620u));
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019C70(CpuContext c, IMemory m)
    {
        c.V0 = 0x801D0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x661Cu));
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019C80(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.RA = 0x80019C90u;
        GranTurismo2PC.func_80019C70(c, m);
        c.S0 = c.V0 + 0u;
        c.RA = 0x80019C98u;
        GranTurismo2PC.func_80019C60(c, m);
        c.V1 = c.V0 + 0u;
        if (c.V1 == 0u) {
            c.V0 = 0u + 0u;
            goto L80019CE4;
        }
        c.V0 = 0u + 0u;
        c.V0 = 0x00060000u;
        c.V0 = c.V0 | 0x8DB8u;
        c.V0 = c.V0 < c.S0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S0 << 2;
            goto L80019CC4;
        }
        c.V0 = c.S0 << 2;
        c.S0 = 0x00060000u;
        c.S0 = c.S0 | 0x8DB8u;
        c.V0 = c.S0 << 2;
        L80019CC4: ;
        c.V0 = c.V0 + c.S0;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 - c.S0;
        c.V0 = c.V0 << 4;
        c.V0 = c.V0 + c.S0;
        c.V0 = c.V0 << 4;
        if (c.V1 != 0u) { c.LO = c.V0 / c.V1; c.HI = c.V0 % c.V1; }
        c.V0 = c.LO;
        L80019CE4: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019CF4(CpuContext c, IMemory m)
    {
        c.V0 = 0x801D0000u;
        c.V0 = c.V0 - 0x6720u;
        c.V0 = c.V0 + 0xB8u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x54u));
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x50u));
        if (c.A0 == 0u) {
            c.V0 = c.V1 << 1;
            goto L80019D30;
        }
        c.V0 = c.V1 << 1;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 2;
        if (c.A0 != 0u) { c.LO = c.V0 / c.A0; c.HI = c.V0 % c.A0; }
        c.V0 = c.LO;
        return;
        L80019D30: ;
        c.V0 = 0u + 0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019D38(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.S0 + 0u;
        c.V0 = 0x801D0000u;
        c.A0 = 0x80050000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.A0 + 0x1160u));
        c.V0 = c.V0 - 0x6720u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        c.S4 = c.V0 + 0xB8u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        if ((int)c.V1 <= 0) {
            MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
            goto L80019DA8;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S3 = 0x00000001u;
        c.S2 = c.A0 + 0u;
        c.A0 = c.S4 + 0u;
        L80019D80: ;
        c.A1 = c.S0 + 0u;
        c.RA = 0x80019D88u;
        GranTurismo2PC.func_8005DB90(c, m);
        if (c.V0 != c.S3) {
            goto L80019D94;
        }
        c.S1 = c.S1 + 0x1u;
        L80019D94: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S2 + 0x1160u));
        c.S0 = c.S0 + 0x1u;
        c.V0 = (int)c.S0 < (int)c.V0 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.A0 = c.S4 + 0u;
            goto L80019D80;
        }
        c.A0 = c.S4 + 0u;
        L80019DA8: ;
        c.V0 = c.S1 << 2;
        c.V0 = c.V0 + c.S1;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 - c.S1;
        c.V0 = c.V0 << 4;
        c.V0 = c.V0 + c.S1;
        c.V1 = 0x80050000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 + 0x1160u));
        c.V0 = c.V0 << 4;
        c.V1 = c.V1 - 0x1Du;
        if (c.V1 != 0u) { if ((int)c.V0 == int.MinValue && (int)c.V1 == -1) { c.LO = 0x80000000u; c.HI = 0u; } else { c.LO = (uint)((int)c.V0 / (int)c.V1); c.HI = (uint)((int)c.V0 % (int)c.V1); } }
        c.V0 = c.LO;
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
    public static void func_80019DF8(CpuContext c, IMemory m)
    {
        c.V0 = 0x801D0000u;
        c.V0 = c.V0 - 0x6720u;
        c.V0 = c.V0 + 0xB8u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x58u));
        MemoryAccess.WriteU32(m, c.A0, c.V1);
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x5Cu));
        MemoryAccess.WriteU32(m, c.A1, c.V0);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019E1C(CpuContext c, IMemory m)
    {
        c.V0 = 0x801D0000u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 - 0x2AACu));
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019E2C(CpuContext c, IMemory m)
    {
        c.V0 = 0x801D0000u;
        c.V0 = c.V0 - 0x6720u;
        c.T3 = c.V0 + 0x3C74u;
        c.V1 = 0u + 0u;
        c.T1 = c.V1 + 0u;
        c.A3 = c.V1 + 0u;
        c.T2 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x3C74u));
        c.T0 = 0x05F50000u;
        c.T0 = c.T0 | 0xE100u;
        c.A2 = 0x00000004u;
        L80019E54: ;
        c.V0 = (int)c.A3 < (int)c.T2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.T3 + c.A2;
            goto L80019E8C;
        }
        c.V0 = c.T3 + c.A2;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x90u));
        c.V1 = c.V1 + c.V0;
        c.V0 = c.T0 < c.V1 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L80019E80;
        }
        c.T1 = c.T1 + 0x1u;
        c.V1 = c.V1 - c.T0;
        L80019E80: ;
        c.A2 = c.A2 + 0xA4u;
        c.A3 = c.A3 + 0x1u;
        goto L80019E54;
        L80019E8C: ;
        MemoryAccess.WriteU32(m, c.A0, c.T1);
        MemoryAccess.WriteU32(m, c.A1, c.V1);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019E98(CpuContext c, IMemory m)
    {
        c.V0 = 0x801D0000u;
        c.V0 = c.V0 - 0x6720u;
        c.T1 = c.V0 + 0x3C74u;
        c.A3 = 0xFFFFFFFFu;
        c.A2 = 0u + 0u;
        c.A0 = c.A2 + 0u;
        c.T0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x3C74u));
        c.A1 = 0x00000004u;
        L80019EB8: ;
        c.V0 = (int)c.A0 < (int)c.T0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.T1 + c.A1;
            goto L80019EF0;
        }
        c.V0 = c.T1 + c.A1;
        c.V0 = MemoryAccess.ReadU16(m, (c.V0 + 0x98u));
        c.V1 = c.V0 & 0x3FFFu;
        c.V0 = (int)c.A2 < (int)c.V1 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L80019EE4;
        }
        c.A2 = c.V1 + 0u;
        c.A3 = c.A0 + 0u;
        L80019EE4: ;
        c.A1 = c.A1 + 0xA4u;
        c.A0 = c.A0 + 0x1u;
        goto L80019EB8;
        L80019EF0: ;
        c.V0 = c.A3 + 0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019EF8(CpuContext c, IMemory m)
    {
        c.A2 = 0u + 0u;
        c.T0 = c.A2 + 0u;
        c.V0 = 0x801D0000u;
        c.T1 = c.V0 - 0x6720u;
        c.A3 = 0x00001418u;
        L80019F0C: ;
        c.V0 = (int)c.T0 < 6 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A1 = 0u + 0u;
            goto L80019F54;
        }
        c.A1 = 0u + 0u;
        c.V1 = c.A3 + c.T1;
        L80019F1C: ;
        c.V0 = (int)c.A1 < 10 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L80019F48;
        }
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V1 + 0x1u));
        if (c.V0 != c.A0) {
            goto L80019F3C;
        }
        c.A2 = c.A2 + 0x1u;
        L80019F3C: ;
        c.V1 = c.V1 + 0xA4u;
        c.A1 = c.A1 + 0x1u;
        goto L80019F1C;
        L80019F48: ;
        c.A3 = c.A3 + 0x668u;
        c.T0 = c.T0 + 0x1u;
        goto L80019F0C;
        L80019F54: ;
        c.V0 = c.A2 + 0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019F5C(CpuContext c, IMemory m)
    {
        { var _r = (long)(int)c.A0 * (int)c.A0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V1 = 0u | 0x88B9u;
        c.V0 = c.A1 << 3;
        c.V0 = c.V0 + c.A1;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.A1;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.A1;
        c.V0 = c.V0 << 5;
        c.V0 = c.V0 - c.A1;
        c.A0 = c.LO;
        if ((int)c.A0 <= 0) {
            c.V0 = c.V0 << 5;
            goto L80019F98;
        }
        c.V0 = c.V0 << 5;
        if (c.A0 != 0u) { if ((int)c.V0 == int.MinValue && (int)c.A0 == -1) { c.LO = 0x80000000u; c.HI = 0u; } else { c.LO = (uint)((int)c.V0 / (int)c.A0); c.HI = (uint)((int)c.V0 % (int)c.A0); } }
        c.V1 = c.LO;
        L80019F98: ;
        c.V0 = c.V1 + 0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019FA0(CpuContext c, IMemory m)
    {
        c.V0 = 0u | 0xFFFEu;
        c.V0 = c.V0 < c.A1 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L80019FB4;
        }
        c.A1 = 0u | 0xFFFEu;
        L80019FB4: ;
        MemoryAccess.WriteU16(m, (c.A0 + 0x2u), (ushort)c.A1);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019FBC(CpuContext c, IMemory m)
    {
        c.A2 = c.A2 << 1;
        c.V0 = MemoryAccess.ReadU8(m, (c.A0 + 0x4u));
        c.A3 = c.A3 << 4;
        c.V0 = c.V0 & 0x00FEu;
        c.V0 = c.V0 | c.A1;
        c.V0 = c.V0 & 0x00F1u;
        c.V0 = c.V0 | c.A2;
        c.V0 = c.V0 & 0x00EFu;
        c.V0 = c.V0 | c.A3;
        MemoryAccess.WriteU8(m, (c.A0 + 0x4u), (byte)c.V0);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80019FE8(CpuContext c, IMemory m)
    {
        c.V0 = 0x80090000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x2870u));
        MemoryAccess.WriteU16(m, (c.A0 + 0x2328u), (ushort)0u);
        c.V0 = MemoryAccess.ReadU16(m, (c.V0 + 0x16u));
        MemoryAccess.WriteU16(m, (c.A0 + 0x232Au), (ushort)c.V0);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001A000(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x2E0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x2D8u), c.S2);
        c.S2 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x2DCu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x2D4u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x2D0u), c.S0);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x2328u));
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x232Au));
        c.V0 = (int)c.V1 < (int)c.V0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A0 = 0x00000001u;
            goto L8001A0E0;
        }
        c.A0 = 0x00000001u;
        c.A0 = c.V1 + c.A0;
        c.S1 = c.V1 << 1;
        c.S1 = c.S1 + c.V1;
        c.S1 = c.S1 << 1;
        c.S1 = c.S2 + c.S1;
        c.RA = 0x8001A048u;
        GranTurismo2PC.func_800768C0(c, m);
        c.A0 = c.V0 + 0u;
        c.A1 = c.SP + 0x10u;
        c.RA = 0x8001A054u;
        GranTurismo2PC.func_80076F5C(c, m);
        c.A0 = c.SP + 0x10u;
        c.S0 = c.SP + 0x98u;
        c.A1 = c.S0 + 0u;
        c.RA = 0x8001A064u;
        GranTurismo2PC.func_800771AC(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.SP + 0x258u;
        c.RA = 0x8001A070u;
        GranTurismo2PC.func_80075930(c, m);
        c.A1 = MemoryAccess.ReadU16(m, (c.SP + 0x2Cu));
        c.A0 = 0x00000005u;
        c.RA = 0x8001A07Cu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.A0 = 0x66660000u;
        c.V1 = MemoryAccess.ReadU16(m, (c.SP + 0x258u));
        c.A0 = c.A0 | 0x6667u;
        { var _r = (long)(int)c.V1 * (int)c.A0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.S0 = MemoryAccess.ReadU8(m, (c.V0 + 0xEu));
        c.T0 = c.HI;
        c.V0 = (uint)((int)c.T0 >> 2);
        MemoryAccess.WriteU8(m, c.S1, (byte)c.V0);
        c.A0 = MemoryAccess.ReadU16(m, (c.SP + 0x258u));
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.SP + 0xF2u));
        c.S0 = 0u < c.S0 ? 1u : 0u;
        c.RA = 0x8001A0ACu;
        GranTurismo2PC.func_80019F5C(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001A0B8u;
        GranTurismo2PC.func_80019FA0(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S0 + 0u;
        c.A3 = MemoryAccess.ReadU8(m, (c.SP + 0x64u));
        c.A2 = MemoryAccess.ReadU8(m, (c.SP + 0x122u));
        c.A3 = c.A3 < 0x00000001u ? 1u : 0u;
        c.RA = 0x8001A0D0u;
        GranTurismo2PC.func_80019FBC(c, m);
        c.V0 = MemoryAccess.ReadU16(m, (c.S2 + 0x2328u));
        c.A0 = 0u + 0u;
        c.V0 = c.V0 + 0x1u;
        MemoryAccess.WriteU16(m, (c.S2 + 0x2328u), (ushort)c.V0);
        L8001A0E0: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x2DCu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x2D8u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x2D4u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x2D0u));
        c.V0 = c.A0 + 0u;
        c.SP = c.SP + 0x2E0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001A0FC(CpuContext c, IMemory m)
    {
        c.T0 = 0u + 0u;
        c.A3 = c.A2 + 0u;
        c.V0 = c.A1 << 1;
        c.V0 = c.V0 + c.A1;
        c.V0 = c.V0 << 1;
        c.V0 = c.V0 + c.A0;
        c.T2 = MemoryAccess.ReadU16(m, (c.V0 + 0x2u));
        if ((int)c.A3 <= 0) {
            c.T1 = c.A2 + 0u;
            goto L8001A198;
        }
        c.T1 = c.A2 + 0u;
        L8001A120: ;
        c.V0 = c.T0 + c.T1;
        L8001A124: ;
        c.A3 = (uint)((int)c.V0 >> 1);
        c.V0 = c.A3 << 1;
        c.V0 = c.A0 + c.V0;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x232Cu));
        c.V0 = c.V1 << 1;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 1;
        c.V0 = c.V0 + c.A0;
        c.V0 = MemoryAccess.ReadU16(m, (c.V0 + 0x2u));
        c.V0 = (int)c.T2 < (int)c.V0 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L8001A16C;
        }
        if (c.T0 == c.A3) {
            c.T1 = c.A3 + 0u;
            goto L8001A198;
        }
        c.T1 = c.A3 + 0u;
        c.V0 = c.T0 + c.T1;
        goto L8001A124;
        L8001A16C: ;
        if (c.T0 != c.A3) {
            c.T0 = c.A3 + 0u;
            goto L8001A120;
        }
        c.T0 = c.A3 + 0u;
        c.A3 = c.T1 + 0u;
        goto L8001A198;
        L8001A17C: ;
        c.V0 = c.A2 - 0x1u;
        c.A2 = c.V0 + 0u;
        c.V0 = c.A2 << 1;
        c.V0 = c.A0 + c.V0;
        c.V0 = MemoryAccess.ReadU16(m, (c.V0 + 0x232Cu));
        c.V1 = c.A0 + c.V1;
        MemoryAccess.WriteU16(m, (c.V1 + 0x232Cu), (ushort)c.V0);
        L8001A198: ;
        c.V0 = (int)c.A3 < (int)c.A2 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V1 = c.A2 << 1;
            goto L8001A17C;
        }
        c.V1 = c.A2 << 1;
        c.V0 = c.A3 << 1;
        c.V0 = c.A0 + c.V0;
        MemoryAccess.WriteU16(m, (c.V0 + 0x232Cu), (ushort)c.A1);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001A1B4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A0 + 0u;
        c.A0 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x232Au));
        if ((int)c.V0 <= 0) {
            c.S1 = c.A0 + 0u;
            goto L8001A208;
        }
        c.S1 = c.A0 + 0u;
        c.A1 = 0xFFFFFFFFu;
        c.V1 = c.S2 + 0u;
        L8001A1E8: ;
        MemoryAccess.WriteU16(m, (c.V1 + 0x232Cu), (ushort)c.A1);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x232Au));
        c.A0 = c.A0 + 0x1u;
        c.V0 = (int)c.A0 < (int)c.V0 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V1 = c.V1 + 0x2u;
            goto L8001A1E8;
        }
        c.V1 = c.V1 + 0x2u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x232Au));
        L8001A208: ;
        if ((int)c.V0 <= 0) {
            c.S0 = 0u + 0u;
            goto L8001A234;
        }
        c.S0 = 0u + 0u;
        L8001A210: ;
        c.A0 = c.S2 + 0u;
        c.A1 = c.S0 + 0u;
        c.A2 = c.S1 + 0u;
        c.RA = 0x8001A220u;
        GranTurismo2PC.func_8001A0FC(c, m);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x232Au));
        c.S0 = c.S0 + 0x1u;
        c.V0 = (int)c.S0 < (int)c.V0 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S1 = c.S1 + 0x1u;
            goto L8001A210;
        }
        c.S1 = c.S1 + 0x1u;
        L8001A234: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001A24C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        c.S4 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, c.S0);
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x2u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x38u));
        c.S1 = c.A3 + 0u;
        c.RA = 0x8001A288u;
        GranTurismo2PC.func_80019F5C(c, m);
        c.A2 = c.V0 + 0u;
        c.S1 = c.S1 - c.S3;
        c.A3 = 0u + 0u;
        c.A0 = c.S2 + 0u;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x4u));
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x232Au));
        c.T1 = c.A3 < c.V0 ? 1u : 0u;
        L8001A2A4: ;
        c.V0 = (int)c.A3 < (int)c.A1 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L8001A2E4;
        }
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0x232Cu));
        c.V1 = c.V0 << 1;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 1;
        c.V1 = c.V1 + c.S2;
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 + 0x2u));
        c.V0 = (int)c.A2 < (int)c.V0 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.A0 = c.A0 + 0x2u;
            goto L8001A2E4;
        }
        c.A0 = c.A0 + 0x2u;
        c.A3 = c.A3 + 0x1u;
        goto L8001A2A4;
        L8001A2E4: ;
        c.A3 = c.A3 - c.S3;
        if ((int)c.A3 >= 0) {
            goto L8001A2F4;
        }
        c.A3 = 0u + 0u;
        L8001A2F4: ;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x232Au));
        c.V0 = (int)c.A3 < (int)c.V1 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.A0 = 0u + 0u;
            goto L8001A30C;
        }
        c.A0 = 0u + 0u;
        c.A3 = c.V1 - 0x1u;
        L8001A30C: ;
        c.A2 = c.A3 + 0u;
        c.V0 = c.A2 << 1;
        c.T0 = c.V0 + c.S2;
        c.A1 = c.S4 + 0u;
        L8001A31C: ;
        if ((int)c.A2 < 0) {
            c.V0 = c.A3 << 1;
            goto L8001A398;
        }
        c.V0 = c.A3 << 1;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.T0 + 0x232Cu));
        if (c.T1 != 0u) {
            c.V0 = c.V1 << 1;
            goto L8001A358;
        }
        c.V0 = c.V1 << 1;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 1;
        c.V0 = c.V0 + c.S2;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x4u));
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 != 0u) {
            c.V0 = (int)c.A0 < (int)c.S1 ? 1u : 0u;
            goto L8001A388;
        }
        c.V0 = (int)c.A0 < (int)c.S1 ? 1u : 0u;
        MemoryAccess.WriteU16(m, (c.A1 + 0x4u), (ushort)c.V1);
        goto L8001A37C;
        L8001A358: ;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 1;
        c.V0 = c.V0 + c.S2;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x4u));
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u) {
            c.V0 = (int)c.A0 < (int)c.S1 ? 1u : 0u;
            goto L8001A388;
        }
        c.V0 = (int)c.A0 < (int)c.S1 ? 1u : 0u;
        MemoryAccess.WriteU16(m, (c.A1 + 0x4u), (ushort)c.V1);
        L8001A37C: ;
        c.A1 = c.A1 + 0x2u;
        c.A0 = c.A0 + 0x1u;
        c.V0 = (int)c.A0 < (int)c.S1 ? 1u : 0u;
        L8001A388: ;
        if (c.V0 == 0u) {
            c.T0 = c.T0 - 0x2u;
            goto L8001A430;
        }
        c.T0 = c.T0 - 0x2u;
        c.A2 = c.A2 - 0x1u;
        goto L8001A31C;
        L8001A398: ;
        c.A2 = c.A3 + 0u;
        c.A3 = c.V0 + c.S2;
        c.V0 = c.A0 << 1;
        c.A1 = c.V0 + c.S4;
        L8001A3A8: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x232Au));
        c.V0 = (int)c.A2 < (int)c.V0 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L8001A430;
        }
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.A3 + 0x232Cu));
        if (c.T1 != 0u) {
            c.V0 = c.V1 << 1;
            goto L8001A3F0;
        }
        c.V0 = c.V1 << 1;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 1;
        c.V0 = c.V0 + c.S2;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x4u));
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 != 0u) {
            c.V0 = (int)c.A0 < (int)c.S1 ? 1u : 0u;
            goto L8001A420;
        }
        c.V0 = (int)c.A0 < (int)c.S1 ? 1u : 0u;
        MemoryAccess.WriteU16(m, (c.A1 + 0x4u), (ushort)c.V1);
        goto L8001A414;
        L8001A3F0: ;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 1;
        c.V0 = c.V0 + c.S2;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x4u));
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u) {
            c.V0 = (int)c.A0 < (int)c.S1 ? 1u : 0u;
            goto L8001A420;
        }
        c.V0 = (int)c.A0 < (int)c.S1 ? 1u : 0u;
        MemoryAccess.WriteU16(m, (c.A1 + 0x4u), (ushort)c.V1);
        L8001A414: ;
        c.A1 = c.A1 + 0x2u;
        c.A0 = c.A0 + 0x1u;
        c.V0 = (int)c.A0 < (int)c.S1 ? 1u : 0u;
        L8001A420: ;
        if (c.V0 == 0u) {
            c.A3 = c.A3 + 0x2u;
            goto L8001A430;
        }
        c.A3 = c.A3 + 0x2u;
        c.A2 = c.A2 + 0x1u;
        goto L8001A3A8;
        L8001A430: ;
        MemoryAccess.WriteU32(m, c.S4, c.A0);
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
    public static void func_8001A454(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x1Cu));
        if (c.V0 == 0u) {
            c.V1 = 0x0000002Du;
            goto L8001A498;
        }
        c.V1 = 0x0000002Du;
        c.A1 = c.SP + 0x10u;
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0xCu));
        c.A2 = c.SP + 0x14u;
        c.RA = 0x8001A484u;
        GranTurismo2PC.func_80060BEC(c, m);
        c.V1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x11u));
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.V0 = c.V0 + c.V1;
        c.V1 = (uint)(sbyte)MemoryAccess.ReadU8(m, c.V0);
        L8001A498: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.V0 = c.V1 + 0u;
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001A4AC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        MemoryAccess.WriteU16(m, c.S1, (ushort)0u);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x1Cu));
        if (c.V0 == 0u) {
            goto L8001A4F0;
        }
        c.RA = 0x8001A4E0u;
        GranTurismo2PC.func_8001A454(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = MemoryAccess.ReadU32(m, (c.S0 + 0xCu));
        c.A2 = c.V0 + 0u;
        c.RA = 0x8001A4F0u;
        GranTurismo2PC.func_800223C4(c, m);
        L8001A4F0: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001A504(CpuContext c, IMemory m)
    {
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.A0 + 0x1Cu));
        if (c.V0 == 0u) {
            goto L8001A528;
        }
        c.V0 = MemoryAccess.ReadU32(m, (c.A0 + 0x24u));
        c.V0 = MemoryAccess.ReadU16(m, c.V0);
        return;
        L8001A528: ;
        c.V0 = 0xFFFFFFFFu;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001A530(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.S0 = c.A1 + 0u;
        c.RA = 0x8001A54Cu;
        GranTurismo2PC.func_8001A504(c, m);
        c.V1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0x1Cu));
        if (c.V1 == 0u) {
            c.A0 = c.V0 + 0u;
            goto L8001A5A4;
        }
        c.A0 = c.V0 + 0u;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0x11u));
        c.S0 = c.S0 + c.V0;
        if ((int)c.S0 >= 0) {
            c.V0 = (int)c.S0 < (int)c.A0 ? 1u : 0u;
            goto L8001A58C;
        }
        c.V0 = (int)c.S0 < (int)c.A0 ? 1u : 0u;
        c.S0 = c.S0 + c.A0;
        L8001A574: ;
        if ((int)c.S0 < 0) {
            c.S0 = c.S0 + c.A0;
            goto L8001A574;
        }
        c.S0 = c.S0 + c.A0;
        c.S0 = c.S0 - c.A0;
        c.V0 = (int)c.S0 < (int)c.A0 ? 1u : 0u;
        goto L8001A58C;
        L8001A588: ;
        c.V0 = (int)c.S0 < (int)c.A0 ? 1u : 0u;
        L8001A58C: ;
        if (c.V0 == 0u) {
            c.S0 = c.S0 - c.A0;
            goto L8001A588;
        }
        c.S0 = c.S0 - c.A0;
        c.S0 = c.S0 + c.A0;
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.S1 + 0x11u), (byte)c.S0);
        MemoryAccess.WriteU8(m, (c.S1 + 0x164u), (byte)c.V0);
        L8001A5A4: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001A5B8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x218u;
        MemoryAccess.WriteU32(m, (c.SP + 0x210u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x214u), c.RA);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x1Cu));
        if (c.V0 == 0u) {
            c.V1 = c.A1 + 0u;
            goto L8001A614;
        }
        c.V1 = c.A1 + 0u;
        c.A0 = c.SP + 0x10u;
        c.A2 = 0u + 0u;
        c.A1 = c.V1 << 3;
        c.A1 = c.A1 + c.V1;
        c.A1 = c.A1 << 6;
        c.A1 = c.A1 + 0x20u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x24u));
        c.A3 = c.A2 + 0u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x11u), (byte)c.V1);
        c.A1 = c.V0 + c.A1;
        c.RA = 0x8001A604u;
        GranTurismo2PC.func_80067960(c, m);
        c.A0 = c.SP + 0x10u;
        c.A1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x10u));
        c.A2 = 0u + 0u;
        c.RA = 0x8001A614u;
        GranTurismo2PC.func_80067824(c, m);
        L8001A614: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x214u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x210u));
        c.SP = c.SP + 0x218u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001A624_gt2_overlay_4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.A1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x11u));
        c.RA = 0x8001A640u;
        GranTurismo2PC.func_8001A5B8(c, m);
        MemoryAccess.WriteU8(m, (c.S0 + 0x164u), (byte)0u);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001A654(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        c.V0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        c.S4 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = c.A3 + 0u;
        c.A0 = c.S4 + 0u;
        c.A1 = 0x00800000u;
        c.A1 = c.A1 | 0x8080u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = MemoryAccess.ReadU16(m, (c.V0 + 0x18u));
        c.S1 = MemoryAccess.ReadU16(m, (c.V0 + 0x1Au));
        c.S0 = c.S0 << 16;
        c.S1 = c.S1 << 16;
        c.RA = 0x8001A6A0u;
        GranTurismo2PC.func_80081478(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = 0x00000009u;
        c.V1 = (uint)((int)c.S0 >> 17);
        c.S2 = c.S2 - c.V1;
        c.V1 = (uint)((int)c.S1 >> 17);
        c.S3 = c.S3 - c.V1;
        c.V1 = 0x00002924u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x6u), (ushort)c.V1);
        c.V1 = 0x000000A5u;
        c.S0 = (uint)((int)c.S0 >> 16);
        c.S1 = (uint)((int)c.S1 >> 16);
        MemoryAccess.WriteU16(m, c.V0, (ushort)c.S2);
        MemoryAccess.WriteU16(m, (c.V0 + 0x2u), (ushort)c.S3);
        MemoryAccess.WriteU8(m, (c.V0 + 0x4u), (byte)0u);
        MemoryAccess.WriteU8(m, (c.V0 + 0x5u), (byte)c.V1);
        MemoryAccess.WriteU16(m, (c.V0 + 0x8u), (ushort)c.S0);
        MemoryAccess.WriteU16(m, (c.V0 + 0xAu), (ushort)c.S1);
        c.RA = 0x8001A6E8u;
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
    public static void func_8001A708(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x58u;
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.S4);
        c.S4 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.S0);
        c.S0 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x54u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x50u), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0x4Cu), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x48u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x3Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x38u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x34u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x58u), c.A0);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.A0 + 0x1Cu));
        if (c.V0 == 0u) {
            c.S6 = c.A3 + 0u;
            goto L8001A874;
        }
        c.S6 = c.A3 + 0u;
        c.A1 = c.SP + 0x20u;
        c.S1 = 0u + 0u;
        c.S5 = c.A1 + 0u;
        c.FP = 0x0000000Au;
        c.S7 = 0x00000008u;
        c.A0 = MemoryAccess.ReadU32(m, (c.A0 + 0xCu));
        c.A2 = c.SP + 0x24u;
        c.RA = 0x8001A770u;
        GranTurismo2PC.func_80060BEC(c, m);
        c.S2 = c.S6 - 0x1u;
        c.S3 = c.S6 + 0x8u;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.V0);
        L8001A77C: ;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.V0 = (int)c.S1 < (int)c.T0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.S0 = c.S0 - 0xCu;
            goto L8001A868;
        }
        c.S0 = c.S0 - 0xCu;
        c.A0 = c.S4 + 0u;
        c.V0 = MemoryAccess.ReadU32(m, c.S5);
        c.A1 = c.SP + 0x10u;
        c.A2 = MemoryAccess.ReadU16(m, c.V0);
        c.V0 = c.V0 + 0x2u;
        MemoryAccess.WriteU32(m, c.S5, c.V0);
        MemoryAccess.WriteU16(m, (c.SP + 0x10u), (ushort)c.S0);
        MemoryAccess.WriteU16(m, (c.SP + 0x12u), (ushort)c.S6);
        MemoryAccess.WriteU16(m, (c.SP + 0x14u), (ushort)c.FP);
        MemoryAccess.WriteU16(m, (c.SP + 0x16u), (ushort)c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        c.V1 = c.A2 & 0x001Fu;
        c.V1 = c.V1 << 3;
        c.V0 = c.A2 & 0x03E0u;
        c.V0 = c.V0 << 6;
        c.V1 = c.V1 | c.V0;
        c.A2 = c.A2 & 0xF800u;
        c.A2 = c.A2 << 9;
        c.V1 = c.V1 | c.A2;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.V1);
        c.RA = 0x8001A7E4u;
        GranTurismo2PC.func_8006B6E4(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = 0u + 0u;
        c.RA = 0x8001A7F0u;
        GranTurismo2PC.func_8007D024(c, m);
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x58u));
        MemoryAccess.WriteU16(m, c.V0, (ushort)c.S0);
        MemoryAccess.WriteU16(m, (c.V0 + 0x2u), (ushort)c.S6);
        MemoryAccess.WriteU16(m, (c.V0 + 0x4u), (ushort)c.FP);
        MemoryAccess.WriteU16(m, (c.V0 + 0x6u), (ushort)c.S7);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.T0 + 0x11u));
        if (c.S1 != c.V0) {
            c.V1 = 0x00000046u;
            goto L8001A818;
        }
        c.V1 = 0x00000046u;
        c.V1 = 0x000000B4u;
        L8001A818: ;
        c.A0 = c.S4 + 0u;
        c.A1 = c.V1 << 8;
        c.A1 = c.V1 | c.A1;
        c.V0 = c.V1 << 16;
        c.A1 = c.A1 | c.V0;
        c.RA = 0x8001A830u;
        GranTurismo2PC.func_8007E738(c, m);
        c.S1 = c.S1 + 0x1u;
        c.V1 = c.S0 - 0x1u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x14u), (ushort)c.V1);
        MemoryAccess.WriteU16(m, (c.V0 + 0x10u), (ushort)c.V1);
        MemoryAccess.WriteU16(m, (c.V0 + 0x4u), (ushort)c.V1);
        c.V1 = c.S0 + 0xAu;
        MemoryAccess.WriteU16(m, (c.V0 + 0xCu), (ushort)c.V1);
        MemoryAccess.WriteU16(m, (c.V0 + 0x8u), (ushort)c.V1);
        MemoryAccess.WriteU16(m, (c.V0 + 0x16u), (ushort)c.S2);
        MemoryAccess.WriteU16(m, (c.V0 + 0xAu), (ushort)c.S2);
        MemoryAccess.WriteU16(m, (c.V0 + 0x6u), (ushort)c.S2);
        MemoryAccess.WriteU16(m, (c.V0 + 0x12u), (ushort)c.S3);
        MemoryAccess.WriteU16(m, (c.V0 + 0xEu), (ushort)c.S3);
        goto L8001A77C;
        L8001A868: ;
        c.A0 = c.S4 + 0u;
        c.A1 = 0x00000220u;
        c.RA = 0x8001A874u;
        GranTurismo2PC.func_8007DA44(c, m);
        L8001A874: ;
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
    public static void func_8001A8A4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0xF8u;
        MemoryAccess.WriteU32(m, (c.SP + 0xE8u), c.S2);
        c.S2 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0xECu), c.S3);
        c.S3 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0xF0u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0xE4u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0xE0u), c.S0);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S2 + 0x1Cu));
        if (c.V0 == 0u) {
            c.S1 = c.A2 + 0u;
            goto L8001AAD8;
        }
        c.S1 = c.A2 + 0u;
        c.A0 = c.SP + 0x20u;
        c.RA = 0x8001A8DCu;
        GranTurismo2PC.func_8007AF60(c, m);
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0xAAu));
        c.A0 = c.SP + 0x20u;
        c.A1 = 0u - c.A1;
        c.RA = 0x8001A8ECu;
        GranTurismo2PC.func_8007B14C(c, m);
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0xA8u));
        c.A0 = c.SP + 0x20u;
        c.A1 = 0u - c.A1;
        c.RA = 0x8001A8FCu;
        GranTurismo2PC.func_8007B0C4(c, m);
        c.S0 = c.SP + 0x20u;
        c.A0 = c.S0 + 0u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x9Cu));
        c.V1 = MemoryAccess.ReadU32(m, (c.S1 + 0xA0u));
        c.A2 = MemoryAccess.ReadU32(m, (c.S1 + 0xA4u));
        c.A1 = c.SP + 0x40u;
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.V1);
        MemoryAccess.WriteU32(m, (c.SP + 0x48u), c.A2);
        c.RA = 0x8001A924u;
        GranTurismo2PC.func_8007B050(c, m);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0xBCu));
        c.T0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0xBEu));
        c.T1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0xC0u));
        c.T2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0xC2u));
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0xC4u));
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0xC6u));
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0xC8u));
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0xCAu));
        MemoryAccess.WriteU16(m, c.S1, (ushort)c.V1);
        MemoryAccess.WriteU16(m, (c.S1 + 0x2u), (ushort)c.T0);
        MemoryAccess.WriteU16(m, (c.S1 + 0x4u), (ushort)c.T1);
        MemoryAccess.WriteU16(m, (c.S1 + 0x6u), (ushort)c.T2);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0xCCu));
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0xCEu));
        c.A0 = c.S1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.V0);
        c.RA = 0x8001A974u;
        GranTurismo2PC.func_8007B320(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S0 + 0u;
        c.A2 = MemoryAccess.ReadU32(m, (c.S3 + 0x8u));
        c.A3 = 0x00000010u;
        c.RA = 0x8001A988u;
        GranTurismo2PC.func_8007B374(c, m);
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.SP + 0x50u), (byte)c.V0);
        MemoryAccess.WriteU8(m, (c.SP + 0x51u), (byte)0u);
        MemoryAccess.WriteU8(m, (c.SP + 0x52u), (byte)0u);
        MemoryAccess.WriteU8(m, (c.SP + 0x53u), (byte)c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.S2 + 0x20u));
        MemoryAccess.WriteU32(m, (c.SP + 0x5Cu), c.V0);
        c.V0 = MemoryAccess.ReadU16(m, c.S2);
        MemoryAccess.WriteU16(m, (c.SP + 0x56u), (ushort)c.V0);
        c.V1 = MemoryAccess.ReadU16(m, (c.S2 + 0x2u));
        c.V0 = 0x00000040u;
        MemoryAccess.WriteU8(m, (c.SP + 0x54u), (byte)c.V0);
        MemoryAccess.WriteU16(m, (c.SP + 0x58u), (ushort)c.V1);
        c.V0 = MemoryAccess.ReadU32(m, (c.S2 + 0x14u));
        if ((int)c.V0 < 0) {
            c.V1 = c.V0 + 0u;
            goto L8001A9FC;
        }
        c.V1 = c.V0 + 0u;
        c.V0 = (int)c.V1 < 4097 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = 0x00001000u;
            goto L8001A9E4;
        }
        c.V0 = 0x00001000u;
        c.V1 = 0x00001000u;
        L8001A9E4: ;
        c.V0 = c.V0 - c.V1;
        if ((int)c.V0 >= 0) {
            goto L8001A9F4;
        }
        c.V0 = c.V0 + 0x3Fu;
        L8001A9F4: ;
        c.V0 = (uint)((int)c.V0 >> 6);
        MemoryAccess.WriteU8(m, (c.SP + 0x54u), (byte)c.V0);
        L8001A9FC: ;
        c.S0 = c.SP + 0x60u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x8001AA08u;
        GranTurismo2PC.func_8007AF60(c, m);
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0x5Cu));
        c.RA = 0x8001AA14u;
        GranTurismo2PC.func_80061544(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.SP + 0x40u;
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x48u), 0u);
        c.RA = 0x8001AA2Cu;
        GranTurismo2PC.func_8007B050(c, m);
        c.S0 = c.SP + 0x50u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x8001AA38u;
        GranTurismo2PC.func_80061490(c, m);
        c.A0 = c.SP + 0xC0u;
        c.RA = 0x8001AA40u;
        GranTurismo2PC.func_8007AF60(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S0 + 0u;
        c.A2 = 0u + 0u;
        c.RA = 0x8001AA50u;
        GranTurismo2PC.func_80067444(c, m);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S1 + 0xD0u));
        if (c.V0 == 0u) {
            goto L8001AAD8;
        }
        c.A0 = c.SP + 0x20u;
        c.RA = 0x8001AA68u;
        GranTurismo2PC.func_8007AF60(c, m);
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0xA8u));
        c.A0 = c.SP + 0x20u;
        c.A1 = 0u - c.A1;
        c.RA = 0x8001AA78u;
        GranTurismo2PC.func_8007B0C4(c, m);
        c.S0 = c.SP + 0x20u;
        c.A0 = c.S0 + 0u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x9Cu));
        c.V1 = MemoryAccess.ReadU32(m, (c.S1 + 0xA0u));
        c.A2 = MemoryAccess.ReadU32(m, (c.S1 + 0xA4u));
        c.A1 = c.SP + 0x40u;
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.V1);
        MemoryAccess.WriteU32(m, (c.SP + 0x48u), c.A2);
        c.RA = 0x8001AAA0u;
        GranTurismo2PC.func_8007B050(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S0 + 0u;
        c.A2 = MemoryAccess.ReadU32(m, (c.S3 + 0x8u));
        c.A3 = 0x00000010u;
        c.RA = 0x8001AAB4u;
        GranTurismo2PC.func_8007B374(c, m);
        c.A0 = c.S2 + 0x2Cu;
        c.A2 = c.S1 + 0u;
        c.A1 = MemoryAccess.ReadU32(m, (c.S3 + 0x8u));
        c.A3 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.A2 + 0xD1u));
        c.V0 = MemoryAccess.ReadU32(m, (c.A2 + 0xD4u));
        c.A1 = c.A1 + 0xA0u;
        c.A3 = 0u < c.A3 ? 1u : 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.RA = 0x8001AAD8u;
        GranTurismo2PC.func_8006C31C(c, m);
        L8001AAD8: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0xF0u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0xECu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0xE8u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0xE4u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0xE0u));
        c.SP = c.SP + 0xF8u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001AAF4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU8(m, (c.A0 + 0x1Du), (byte)0u);
        c.V0 = MemoryAccess.ReadU16(m, (c.A1 + 0xEu));
        MemoryAccess.WriteU16(m, c.A0, (ushort)c.V0);
        c.V0 = MemoryAccess.ReadU16(m, (c.A1 + 0x10u));
        MemoryAccess.WriteU16(m, (c.A0 + 0x2u), (ushort)c.V0);
        c.V0 = MemoryAccess.ReadU8(m, (c.A1 + 0xCu));
        MemoryAccess.WriteU8(m, (c.A0 + 0x10u), (byte)c.V0);
        c.V1 = MemoryAccess.ReadU32(m, c.A1);
        c.A2 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.A3 = MemoryAccess.ReadU32(m, (c.A1 + 0x8u));
        MemoryAccess.WriteU32(m, (c.A0 + 0x20u), c.V1);
        MemoryAccess.WriteU32(m, (c.A0 + 0x24u), c.A2);
        MemoryAccess.WriteU32(m, (c.A0 + 0x28u), c.A3);
        MemoryAccess.WriteU8(m, (c.A0 + 0x1Cu), (byte)0u);
        MemoryAccess.WriteU8(m, (c.A0 + 0x11u), (byte)0u);
        MemoryAccess.WriteU8(m, (c.A0 + 0x164u), (byte)0u);
        c.A0 = c.A0 + 0x2Cu;
        c.A1 = 0x00060000u;
        c.RA = 0x8001AB54u;
        GranTurismo2PC.func_8006C274(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001AB64(CpuContext c, IMemory m)
    {
        MemoryAccess.WriteU8(m, (c.A0 + 0x1Cu), (byte)0u);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001AB6C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x38u;
        c.V0 = 0x801D0000u;
        c.V0 = c.V0 - 0x6720u;
        c.V1 = c.V0 + 0x3C74u;
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S0);
        c.S2 = (uint)(short)MemoryAccess.ReadU16(m, (c.V1 + 0x4018u));
        if ((int)c.S2 < 0) {
            c.S3 = c.A0 + 0u;
            goto L8001AC04;
        }
        c.S3 = c.A0 + 0u;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S3 + 0x1Du));
        if (c.V0 != 0u) {
            c.S0 = c.S2 << 2;
            goto L8001AC04;
        }
        c.S0 = c.S2 << 2;
        c.S0 = c.S0 + c.S2;
        c.S0 = c.S0 << 3;
        c.S0 = c.S0 + c.S2;
        c.S0 = c.S0 << 2;
        c.S0 = c.S0 + 0x4u;
        c.S0 = c.S0 + c.V1;
        c.S1 = MemoryAccess.ReadU32(m, (c.S0 + 0x8Cu));
        c.A1 = MemoryAccess.ReadU32(m, (c.S0 + 0x4u));
        c.A0 = c.S1 + 0u;
        c.RA = 0x8001ABD8u;
        GranTurismo2PC.func_80018350(c, m);
        c.A0 = c.S3 + 0u;
        c.V1 = 0x00000001u;
        c.A2 = MemoryAccess.ReadU16(m, (c.S0 + 0xA2u));
        c.A1 = c.S1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V1);
        c.V1 = MemoryAccess.ReadU32(m, (c.S0 + 0x8u));
        c.A3 = c.S2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.A2);
        c.A2 = c.V0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V1);
        c.RA = 0x8001AC04u;
        GranTurismo2PC.func_8001AC20(c, m);
        L8001AC04: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x30u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x2Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.SP = c.SP + 0x38u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001AC20(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0xE0u;
        MemoryAccess.WriteU32(m, (c.SP + 0xB8u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0xF8u));
        MemoryAccess.WriteU32(m, (c.SP + 0xD0u), c.S6);
        c.S6 = c.A0 + 0u;
        RecompOne.Runtime.Sdk.GT2Compat.ResolveLiveryBodyAndPaletteA1A2(c);
        MemoryAccess.WriteU32(m, (c.SP + 0xBCu), c.S1);
        c.S1 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0xCCu), c.S5);
        c.S5 = c.A3 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0xE4u), c.A1);
        c.A0 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0xDCu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0xD8u), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0xD4u), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0xC8u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0xC4u), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0xC0u), c.S2);
        c.RA = 0x8001AC68u;
        GranTurismo2PC.func_8005D950(c, m);
        c.V1 = MemoryAccess.ReadU16(m, (c.V0 + 0x4u));
        c.S7 = MemoryAccess.ReadU32(m, (c.S6 + 0x20u));
        c.S4 = MemoryAccess.ReadU32(m, (c.S6 + 0x24u));
        c.FP = MemoryAccess.ReadU32(m, (c.S6 + 0x28u));
        MemoryAccess.WriteU32(m, (c.S6 + 0x4u), c.V1);
        c.V0 = MemoryAccess.ReadU16(m, (c.V0 + 0x6u));
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xE4u));
        MemoryAccess.WriteU8(m, (c.S6 + 0x11u), (byte)c.S1);
        c.S1 = MemoryAccess.ReadU8(m, (c.S6 + 0x10u));
        c.A1 = c.S7 + 0u;
        MemoryAccess.WriteU8(m, (c.S6 + 0x1Du), (byte)0u);
        MemoryAccess.WriteU32(m, (c.S6 + 0x14u), c.S0);
        MemoryAccess.WriteU32(m, (c.S6 + 0x8u), c.V0);
        c.A0 = c.V0 + 0u;
        c.S1 = c.S1 << 24;
        MemoryAccess.WriteU32(m, (c.S6 + 0xCu), c.T0);
        c.RA = 0x8001ACACu;
        GranTurismo2PC.func_8005D908(c, m);
        c.S2 = 0x00000240u;
        c.V0 = 0x000000A4u;
        c.S0 = c.S7 + 0x8u;
        c.A0 = c.S7 + 0u;
        MemoryAccess.WriteU16(m, (c.SP + 0x28u), (ushort)c.S2);
        MemoryAccess.WriteU16(m, (c.SP + 0x2Au), (ushort)c.V0);
        c.V0 = MemoryAccess.ReadU16(m, (c.S0 + 0x8u));
        c.S3 = c.SP + 0x28u;
        MemoryAccess.WriteU16(m, (c.SP + 0x2Cu), (ushort)c.V0);
        c.V0 = MemoryAccess.ReadU16(m, (c.S0 + 0xAu));
        c.A1 = c.S3 + 0u;
        MemoryAccess.WriteU16(m, (c.SP + 0x2Eu), (ushort)c.V0);
        c.RA = 0x8001ACE0u;
        GranTurismo2PC.func_8007BC1C(c, m);
        c.V1 = MemoryAccess.ReadU32(m, (c.S7 + 0x8u));
        c.V0 = 0x000000A5u;
        MemoryAccess.WriteU16(m, (c.SP + 0x28u), (ushort)c.S2);
        MemoryAccess.WriteU16(m, (c.SP + 0x2Au), (ushort)c.V0);
        c.S0 = c.S0 + c.V1;
        c.V0 = MemoryAccess.ReadU16(m, (c.S0 + 0x8u));
        c.A0 = c.S7 + 0u;
        MemoryAccess.WriteU16(m, (c.SP + 0x2Cu), (ushort)c.V0);
        c.V0 = MemoryAccess.ReadU16(m, (c.S0 + 0xAu));
        c.A1 = c.S3 + 0u;
        MemoryAccess.WriteU16(m, (c.SP + 0x2Eu), (ushort)c.V0);
        c.RA = 0x8001AD10u;
        GranTurismo2PC.func_8007BC58(c, m);
        c.V0 = MemoryAccess.ReadU16(m, (c.S0 + 0x8u));
        c.A0 = MemoryAccess.ReadU32(m, (c.S6 + 0x4u));
        c.V0 = c.V0 << 2;
        MemoryAccess.WriteU16(m, (c.S6 + 0x18u), (ushort)c.V0);
        c.V0 = MemoryAccess.ReadU16(m, (c.S0 + 0xAu));
        c.A1 = c.S7 + 0u;
        MemoryAccess.WriteU16(m, (c.S6 + 0x1Au), (ushort)c.V0);
        c.RA = 0x8001AD30u;
        GranTurismo2PC.func_8005D92C(c, m);
        c.A0 = MemoryAccess.ReadU32(m, (c.S6 + 0x4u));
        c.A1 = c.S4 + 0u;
        c.A0 = c.A0 + 0x1u;
        c.RA = 0x8001AD40u;
        GranTurismo2PC.func_8005D92C(c, m);
        c.A0 = c.S7 + 0u;
        c.S2 = (uint)((int)c.S1 >> 24);
        c.A1 = c.S2 + 0u;
        c.RA = 0x8001AD50u;
        GranTurismo2PC.func_8006101C(c, m);
        c.A0 = c.S7 + 0u;
        c.A1 = c.SP + 0x18u;
        c.S0 = c.SP + 0x20u;
        c.A2 = c.S0 + 0u;
        c.RA = 0x8001AD64u;
        GranTurismo2PC.func_80061504(c, m);
        c.A0 = c.S7 + 0x40u;
        c.A1 = c.SP + 0x18u;
        c.A2 = c.S0 + 0u;
        c.RA = 0x8001AD74u;
        GranTurismo2PC.func_80061308(c, m);
        c.A0 = c.S3 + 0u;
        c.A1 = c.S4 + 0x43A0u;
        c.A2 = 0u + 0u;
        c.V0 = c.S2 & 0x000Fu;
        c.V0 = c.V0 << 6;
        c.S1 = (uint)((int)c.S1 >> 20);
        c.S1 = c.S1 & 0x0100u;
        MemoryAccess.WriteU16(m, (c.SP + 0x28u), (ushort)c.V0);
        c.V0 = 0x00000040u;
        MemoryAccess.WriteU16(m, (c.SP + 0x2Cu), (ushort)c.V0);
        c.V0 = 0x000000E0u;
        MemoryAccess.WriteU16(m, (c.SP + 0x2Au), (ushort)c.S1);
        MemoryAccess.WriteU16(m, (c.SP + 0x2Eu), (ushort)c.V0);
        c.RA = 0x8001ADACu;
        GranTurismo2PC.func_8007BA70(c, m);
        c.A0 = c.S6 + 0u;
        c.A1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S6 + 0x11u));
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.S6 + 0x1Cu), (byte)c.V0);
        c.RA = 0x8001ADC0u;
        GranTurismo2PC.func_8001A5B8(c, m);
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xF0u));
        if (c.T0 == 0u) {
            c.V0 = 0xFFFFE0FFu;
            goto L8001AE0C;
        }
        c.V0 = 0xFFFFE0FFu;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0xF4u));
        c.V0 = c.T0 & c.V0;
        if (c.V0 == 0u) {
            c.A0 = c.T0 + 0u;
            goto L8001AE0C;
        }
        c.A0 = c.T0 + 0u;
        c.A1 = c.FP + 0u;
        c.RA = 0x8001ADECu;
        GranTurismo2PC.func_800615E8(c, m);
        c.A0 = c.FP + 0u;
        c.A1 = c.S2 + 0u;
        c.A2 = 0x00000001u;
        c.RA = 0x8001ADFCu;
        GranTurismo2PC.func_800678E8(c, m);
        c.A0 = c.S7 + 0u;
        c.A2 = MemoryAccess.ReadU16(m, (c.FP + 0x14u));
        c.A1 = 0u + 0u;
        c.RA = 0x8001AE0Cu;
        GranTurismo2PC.func_8006155C(c, m);
        L8001AE0C: ;
        if ((int)c.S5 >= 0) {
            c.S2 = 0x801D0000u;
            goto L8001AE54;
        }
        c.S2 = 0x801D0000u;
        c.S0 = c.SP + 0x30u;
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0xE4u));
        c.A1 = c.S0 + 0u;
        c.RA = 0x8001AE24u;
        GranTurismo2PC.func_80076954(c, m);
        c.A0 = c.S0 + 0u;
        c.S0 = c.S6 + 0x166u;
        c.A1 = c.S0 + 0u;
        c.RA = 0x8001AE34u;
        GranTurismo2PC.func_800771AC(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S6 + 0x326u;
        c.RA = 0x8001AE40u;
        GranTurismo2PC.func_80075930(c, m);
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0xE4u));
        c.A0 = c.S6 + 0x394u;
        c.RA = 0x8001AE4Cu;
        GranTurismo2PC.func_8001AFA8(c, m);
        goto L8001AEC8;
        L8001AE54: ;
        c.S2 = c.S2 - 0x6720u;
        c.S2 = c.S2 + 0x3C74u;
        c.S0 = c.S5 << 2;
        c.S0 = c.S0 + c.S5;
        c.S0 = c.S0 << 3;
        c.S0 = c.S0 + c.S5;
        c.S0 = c.S0 << 2;
        c.S0 = c.S0 + 0x4u;
        c.S0 = c.S0 + c.S2;
        c.S4 = c.S0 + 0x8u;
        c.A0 = c.S4 + 0u;
        c.S1 = c.S6 + 0x166u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x8001AE8Cu;
        GranTurismo2PC.func_800771AC(c, m);
        c.A0 = c.S1 + 0u;
        c.S3 = c.S6 + 0x326u;
        c.A1 = c.S3 + 0u;
        c.RA = 0x8001AE9Cu;
        GranTurismo2PC.func_80075930(c, m);
        c.A0 = c.S6 + 0x394u;
        c.A2 = c.S4 + 0u;
        c.A1 = MemoryAccess.ReadU32(m, c.S0);
        c.A3 = c.S1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S3);
        c.RA = 0x8001AEB4u;
        GranTurismo2PC.func_8001B10C(c, m);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x4018u));
        if (c.S5 != c.V0) {
            c.V0 = 0x00000001u;
            goto L8001AEC8;
        }
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.S6 + 0x1Du), (byte)c.V0);
        L8001AEC8: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0xDCu));
        c.FP = MemoryAccess.ReadU32(m, (c.SP + 0xD8u));
        c.S7 = MemoryAccess.ReadU32(m, (c.SP + 0xD4u));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0xD0u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0xC8u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0xC4u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0xC0u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0xBCu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0xB8u));
        c.SP = c.SP + 0xE0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001AEF8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x38u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S0);
        c.S0 = c.A0 + 0u;
        c.A0 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S2);
        c.S2 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.S0 + 0x28u));
        c.S3 = MemoryAccess.ReadU32(m, (c.S0 + 0x20u));
        c.A1 = c.S1 + 0u;
        c.RA = 0x8001AF2Cu;
        GranTurismo2PC.func_800615E8(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x10u));
        c.A2 = 0x00000001u;
        c.RA = 0x8001AF3Cu;
        GranTurismo2PC.func_800678E8(c, m);
        c.A0 = c.S3 + 0u;
        c.A2 = MemoryAccess.ReadU16(m, (c.S1 + 0x14u));
        c.A1 = 0u + 0u;
        c.RA = 0x8001AF4Cu;
        GranTurismo2PC.func_8006155C(c, m);
        c.A0 = c.S3 + 0u;
        c.A1 = c.SP + 0x10u;
        c.S1 = c.SP + 0x18u;
        c.V0 = 0x80090000u;
        c.V0 = c.V0 + 0x1A70u;
        c.S2 = c.S2 << 1;
        c.S2 = c.S2 + c.V0;
        c.S0 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.A2 = c.S1 + 0u;
        c.RA = 0x8001AF74u;
        GranTurismo2PC.func_80061504(c, m);
        c.A0 = c.S3 + 0x40u;
        c.A1 = c.SP + 0x10u;
        c.A2 = c.S1 + 0u;
        MemoryAccess.WriteU16(m, (c.SP + 0x1Eu), (ushort)c.S0);
        MemoryAccess.WriteU16(m, (c.SP + 0x16u), (ushort)c.S0);
        c.RA = 0x8001AF8Cu;
        GranTurismo2PC.func_80061308(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x30u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x2Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.SP = c.SP + 0x38u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001AFA8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0xB0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x98u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x9Cu), c.S1);
        c.S1 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0xACu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0xA8u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0xA4u), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0xA0u), c.S2);
        c.RA = 0x8001AFD0u;
        GranTurismo2PC.func_80076F0C(c, m);
        c.A0 = c.V0 + 0u;
        c.A1 = 0x0000001Eu;
        c.A2 = c.S1 + 0u;
        c.RA = 0x8001AFE0u;
        GranTurismo2PC.func_80077DC4(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.SP + 0x10u;
        c.S4 = c.V0 + 0u;
        c.RA = 0x8001AFF0u;
        GranTurismo2PC.func_80076954(c, m);
        c.A1 = MemoryAccess.ReadU16(m, (c.SP + 0x1Au));
        c.A0 = 0x00000003u;
        c.RA = 0x8001AFFCu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.A0 = 0x00000005u;
        c.A1 = MemoryAccess.ReadU16(m, (c.SP + 0x2Cu));
        c.S2 = c.V0 + 0u;
        c.RA = 0x8001B00Cu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.A0 = 0x00000006u;
        c.A1 = MemoryAccess.ReadU16(m, (c.SP + 0x1Cu));
        c.S3 = c.V0 + 0u;
        c.RA = 0x8001B01Cu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.A0 = 0x0000000Du;
        c.A1 = MemoryAccess.ReadU16(m, (c.SP + 0x1Eu));
        c.S1 = c.V0 + 0u;
        c.RA = 0x8001B02Cu;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = MemoryAccess.ReadU8(m, (c.S4 + 0x41u));
        MemoryAccess.WriteU16(m, c.S0, (ushort)c.V1);
        c.V1 = MemoryAccess.ReadU16(m, (c.S2 + 0x8u));
        MemoryAccess.WriteU32(m, (c.S0 + 0x4u), c.V1);
        c.V1 = MemoryAccess.ReadU16(m, (c.S2 + 0xAu));
        MemoryAccess.WriteU32(m, (c.S0 + 0x8u), c.V1);
        c.V1 = MemoryAccess.ReadU16(m, (c.S3 + 0x1Au));
        MemoryAccess.WriteU32(m, (c.S0 + 0xCu), c.V1);
        c.V1 = MemoryAccess.ReadU16(m, (c.S2 + 0xEu));
        MemoryAccess.WriteU32(m, (c.S0 + 0x10u), c.V1);
        c.V1 = MemoryAccess.ReadU16(m, (c.S1 + 0x2Cu));
        MemoryAccess.WriteU32(m, (c.S0 + 0x14u), c.V1);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU32(m, (c.S0 + 0x18u), c.V0);
        c.V0 = MemoryAccess.ReadU16(m, (c.S1 + 0x4u));
        MemoryAccess.WriteU32(m, (c.S0 + 0x1Cu), c.V0);
        c.V0 = MemoryAccess.ReadU16(m, (c.S1 + 0x8u));
        MemoryAccess.WriteU32(m, (c.S0 + 0x20u), c.V0);
        c.V0 = MemoryAccess.ReadU16(m, (c.S1 + 0x6u));
        MemoryAccess.WriteU32(m, (c.S0 + 0x24u), c.V0);
        c.V0 = MemoryAccess.ReadU16(m, (c.S1 + 0x2Eu));
        MemoryAccess.WriteU32(m, (c.S0 + 0x2Cu), c.V0);
        c.V1 = MemoryAccess.ReadU16(m, (c.S1 + 0x30u));
        c.V0 = c.V1 << 2;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 1;
        MemoryAccess.WriteU32(m, (c.S0 + 0x30u), c.V0);
        c.V0 = MemoryAccess.ReadU16(m, (c.S1 + 0x32u));
        MemoryAccess.WriteU32(m, (c.S0 + 0x34u), c.V0);
        c.V0 = MemoryAccess.ReadU16(m, (c.S1 + 0x34u));
        MemoryAccess.WriteU32(m, (c.S0 + 0x38u), c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.S4 + 0x44u));
        MemoryAccess.WriteU32(m, (c.S0 + 0x28u), c.V0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0xACu));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0xA8u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0xA4u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0xA0u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x9Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x98u));
        c.SP = c.SP + 0xB0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001B10C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x70u;
        MemoryAccess.WriteU32(m, (c.SP + 0x58u), c.S2);
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x80u));
        MemoryAccess.WriteU32(m, (c.SP + 0x64u), c.S5);
        c.S5 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x54u), c.S1);
        c.S1 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x50u), c.S0);
        c.S0 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x60u), c.S4);
        c.S4 = c.A3 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x6Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x68u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x5Cu), c.S3);
        c.RA = 0x8001B148u;
        GranTurismo2PC.func_80076F0C(c, m);
        c.A0 = c.V0 + 0u;
        c.A1 = 0x0000001Eu;
        c.A2 = c.S1 + 0u;
        c.RA = 0x8001B158u;
        GranTurismo2PC.func_80077DC4(c, m);
        c.A0 = 0x00000003u;
        c.A1 = MemoryAccess.ReadU16(m, (c.S0 + 0xAu));
        c.S6 = c.V0 + 0u;
        c.RA = 0x8001B168u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.A0 = 0x00000005u;
        c.A1 = MemoryAccess.ReadU16(m, (c.S0 + 0x1Cu));
        c.S1 = c.V0 + 0u;
        c.RA = 0x8001B178u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.A0 = 0x00000006u;
        c.A1 = MemoryAccess.ReadU16(m, (c.S0 + 0xCu));
        c.S3 = c.V0 + 0u;
        c.RA = 0x8001B188u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.A0 = 0x0000000Du;
        c.A1 = MemoryAccess.ReadU16(m, (c.S0 + 0xEu));
        c.S0 = c.V0 + 0u;
        c.RA = 0x8001B198u;
        GranTurismo2PC.func_80076F2C(c, m);
        c.V1 = MemoryAccess.ReadU8(m, (c.S6 + 0x41u));
        MemoryAccess.WriteU16(m, c.S5, (ushort)c.V1);
        c.V1 = MemoryAccess.ReadU16(m, (c.S1 + 0x8u));
        MemoryAccess.WriteU32(m, (c.S5 + 0x4u), c.V1);
        c.V1 = MemoryAccess.ReadU16(m, (c.S1 + 0xAu));
        MemoryAccess.WriteU32(m, (c.S5 + 0x8u), c.V1);
        c.V1 = MemoryAccess.ReadU16(m, (c.S3 + 0x1Au));
        MemoryAccess.WriteU32(m, (c.S5 + 0xCu), c.V1);
        c.V1 = MemoryAccess.ReadU16(m, (c.S0 + 0x2Cu));
        MemoryAccess.WriteU32(m, (c.S5 + 0x14u), c.V1);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x8u));
        MemoryAccess.WriteU32(m, (c.S5 + 0x18u), c.V0);
        c.V0 = MemoryAccess.ReadU16(m, (c.S0 + 0x4u));
        MemoryAccess.WriteU32(m, (c.S5 + 0x1Cu), c.V0);
        c.V0 = MemoryAccess.ReadU16(m, (c.S0 + 0x8u));
        MemoryAccess.WriteU32(m, (c.S5 + 0x20u), c.V0);
        c.V0 = MemoryAccess.ReadU16(m, (c.S0 + 0x6u));
        c.A1 = 0x801C0000u;
        MemoryAccess.WriteU32(m, (c.S5 + 0x24u), c.V0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S4 + 0x5Au));
        c.A1 = c.A1 + 0x3114u;
        MemoryAccess.WriteU32(m, (c.S5 + 0x10u), c.V0);
        c.V0 = MemoryAccess.ReadU16(m, c.S2);
        c.S1 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.S5 + 0x2Cu), c.V0);
        c.V0 = MemoryAccess.ReadU16(m, (c.S2 + 0x2u));
        c.S3 = c.SP + 0x10u;
        MemoryAccess.WriteU32(m, (c.S5 + 0x30u), c.V0);
        c.V1 = MemoryAccess.ReadU16(m, (c.S2 + 0x4u));
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU32(m, (c.S5 + 0x38u), c.V0);
        MemoryAccess.WriteU32(m, (c.S5 + 0x34u), c.V1);
        c.A2 = MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A0 = c.S3 + 0u;
        c.RA = 0x8001B244u;
        GranTurismo2PC.func_8008CF34(c, m);
        c.S0 = c.S5 + 0u;
        L8001B248: ;
        c.A0 = c.SP + 0x10u;
        c.RA = 0x8001B250u;
        GranTurismo2PC.func_8008CFC4(c, m);
        c.V0 = (int)c.S1 < (int)c.V0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S3 + c.S1;
            goto L8001B278;
        }
        c.V0 = c.S3 + c.S1;
        c.V0 = MemoryAccess.ReadU8(m, c.V0);
        c.S1 = c.S1 + 0x1u;
        c.V0 = c.V0 << 24;
        c.V0 = (uint)((int)c.V0 >> 24);
        MemoryAccess.WriteU16(m, (c.S0 + 0x3Cu), (ushort)c.V0);
        c.S0 = c.S0 + 0x2u;
        goto L8001B248;
        L8001B278: ;
        c.V0 = c.S1 << 1;
        c.V0 = c.S5 + c.V0;
        MemoryAccess.WriteU16(m, (c.V0 + 0x3Cu), (ushort)0u);
        c.V0 = MemoryAccess.ReadU32(m, (c.S6 + 0x44u));
        MemoryAccess.WriteU32(m, (c.S5 + 0x28u), c.V0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x6Cu));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x68u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x64u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x60u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x5Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x58u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x54u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x50u));
        c.SP = c.SP + 0x70u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001B2B8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.A0 + 0x1Cu));
        if (c.V0 == 0u) {
            c.V0 = 0x80050000u;
            goto L8001B2E4;
        }
        c.V0 = 0x80050000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.A0 + 0x3B0u));
        c.RA = 0x8001B2DCu;
        GranTurismo2PC.func_80076C14(c, m);
        goto L8001B2E8;
        L8001B2E4: ;
        c.V0 = c.V0 + 0x125Cu;
        L8001B2E8: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001B2F8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.A0 + 0x1Cu));
        if (c.V0 == 0u) {
            c.V0 = 0x80050000u;
            goto L8001B324;
        }
        c.V0 = 0x80050000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.A0 + 0x3B4u));
        c.RA = 0x8001B31Cu;
        GranTurismo2PC.func_80076C14(c, m);
        goto L8001B328;
        L8001B324: ;
        c.V0 = c.V0 + 0x125Cu;
        L8001B328: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001B338(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.A0 + 0x1Cu));
        if (c.V0 == 0u) {
            c.V0 = 0x80050000u;
            goto L8001B364;
        }
        c.V0 = 0x80050000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.A0 + 0x3B8u));
        c.RA = 0x8001B35Cu;
        GranTurismo2PC.func_80076C14(c, m);
        goto L8001B368;
        L8001B364: ;
        c.V0 = c.V0 + 0x125Cu;
        L8001B368: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001B378(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x3CCu));
        if ((int)c.A0 < 0) {
            c.V0 = c.V0 + 0x3D0u;
            goto L8001B39C;
        }
        c.V0 = c.V0 + 0x3D0u;
        c.RA = 0x8001B39Cu;
        GranTurismo2PC.func_80076C14(c, m);
        L8001B39C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001B3AC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x38u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S1);
        c.RA = 0x8001B3CCu;
        GranTurismo2PC.func_80081EA8(c, m);
        c.A0 = c.S0 + 0u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x854u), c.V0);
        c.RA = 0x8001B3D8u;
        GranTurismo2PC.func_80080C94(c, m);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x74u));
        if (c.V0 == 0u) {
            c.S2 = 0u + 0u;
            goto L8001B404;
        }
        c.S2 = 0u + 0u;
        c.A1 = 0x00000001u;
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x7Cu));
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x78u));
        c.A2 = c.S2 + 0u;
        c.RA = 0x8001B400u;
        Dispatcher.Call(c, m, c.V0);
        MemoryAccess.WriteU8(m, (c.S0 + 0x74u), (byte)c.V0);
        L8001B404: ;
        c.S1 = c.S0 + 0x2FCu;
        c.A1 = MemoryAccess.ReadU32(m, (c.S0 + 0x98u));
        c.A0 = c.S1 + 0u;
        c.RA = 0x8001B414u;
        GranTurismo2PC.func_8001D208(c, m);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x1CCu));
        c.S3 = 0x00000001u;
        if (c.V0 != c.S3) {
            c.A0 = c.S0 + 0x324u;
            goto L8001B434;
        }
        c.A0 = c.S0 + 0x324u;
        c.A1 = MemoryAccess.ReadU32(m, (c.S0 + 0x1C8u));
        c.A2 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x1CDu));
        c.S2 = c.S3 + 0u;
        c.RA = 0x8001B434u;
        GranTurismo2PC.func_8001AEF8(c, m);
        L8001B434: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x1C4u));
        if (c.V0 != c.S3) {
            goto L8001B554;
        }
        c.A1 = MemoryAccess.ReadU32(m, (c.S0 + 0x1BCu));
        c.A2 = MemoryAccess.ReadU32(m, (c.S0 + 0x1C0u));
        c.A0 = c.S1 + 0u;
        c.RA = 0x8001B454u;
        GranTurismo2PC.func_8001D2CC(c, m);
        c.A0 = c.S1 + 0u;
        c.RA = 0x8001B45Cu;
        Dispatcher.Call(c, m, 0x8001D4E8u);
        c.A0 = c.S1 + 0u;
        c.S1 = c.V0 + 0u;
        c.RA = 0x8001B468u;
        GranTurismo2PC.func_8001D554(c, m);
        c.V1 = 0x800B0000u;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.V1 - 0x72A0u));
        c.A1 = c.V0 + 0u;
        if (c.S1 == 0u) {
            c.V1 = (int)0u < (int)c.V1 ? 1u : 0u;
            goto L8001B488;
        }
        c.V1 = (int)0u < (int)c.V1 ? 1u : 0u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x1C0u));
        c.V0 = c.V0 & c.V1;
        goto L8001B48C;
        L8001B488: ;
        c.V0 = 0u + 0u;
        L8001B48C: ;
        if (c.V0 == 0u) {
            c.V0 = 0x00000001u;
            goto L8001B4AC;
        }
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x1CEu), (ushort)c.V0);
        c.A0 = c.S0 + 0x1D0u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x8001B4A4u;
        GranTurismo2PC.func_8001E9D0(c, m);
        goto L8001B4F0;
        L8001B4AC: ;
        c.V0 = 0x800B0000u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 - 0x7298u));
        if (c.A1 == 0u) {
            c.V1 = (int)0u < (int)c.V0 ? 1u : 0u;
            goto L8001B4C8;
        }
        c.V1 = (int)0u < (int)c.V0 ? 1u : 0u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x1C0u));
        c.V0 = c.V0 & c.V1;
        goto L8001B4CC;
        L8001B4C8: ;
        c.V0 = 0u + 0u;
        L8001B4CC: ;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L8001B4E8;
        }
        c.V0 = 0x00000002u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x1CEu), (ushort)c.V0);
        c.A0 = c.S0 + 0x1D0u;
        c.RA = 0x8001B4E0u;
        GranTurismo2PC.func_8001E9D0(c, m);
        goto L8001B4F0;
        L8001B4E8: ;
        c.A0 = c.S0 + 0x1D0u;
        c.RA = 0x8001B4F0u;
        GranTurismo2PC.func_8001E924(c, m);
        L8001B4F0: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x1B0u));
        c.S2 = 0x00000001u;
        if ((int)c.V0 <= 0) {
            MemoryAccess.WriteU8(m, (c.S0 + 0x1C4u), (byte)0u);
            goto L8001B548;
        }
        MemoryAccess.WriteU8(m, (c.S0 + 0x1C4u), (byte)0u);
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x1ACu));
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x1B1u));
        c.V0 = 0u < c.V0 ? 1u : 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x1B4u));
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x1B8u));
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.V0);
        c.A1 = MemoryAccess.ReadU32(m, (c.S0 + 0x1A0u));
        c.A2 = MemoryAccess.ReadU32(m, (c.S0 + 0x1A4u));
        c.A3 = MemoryAccess.ReadU32(m, (c.S0 + 0x1A8u));
        c.A0 = c.S0 + 0x2FCu;
        c.RA = 0x8001B548u;
        GranTurismo2PC.func_8001D5C8(c, m);
        L8001B548: ;
        MemoryAccess.WriteU8(m, (c.S0 + 0x1B0u), (byte)0u);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x1C4u));
        L8001B554: ;
        if (c.V0 != 0u) {
            goto L8001B5C8;
        }
        c.V1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x1B0u));
        c.A0 = MemoryAccess.ReadU8(m, (c.S0 + 0x1B0u));
        if (c.V1 == 0u) {
            c.V0 = 0x00000001u;
            goto L8001B5C8;
        }
        c.V0 = 0x00000001u;
        if (c.V1 != c.V0) {
            c.V0 = c.A0 - 0x1u;
            goto L8001B5C4;
        }
        c.V0 = c.A0 - 0x1u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x1ACu));
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x1B1u));
        c.V0 = 0u < c.V0 ? 1u : 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x1B4u));
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x1B8u));
        c.A0 = c.S0 + 0x2FCu;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.V0);
        c.A1 = MemoryAccess.ReadU32(m, (c.S0 + 0x1A0u));
        c.A2 = MemoryAccess.ReadU32(m, (c.S0 + 0x1A4u));
        c.A3 = MemoryAccess.ReadU32(m, (c.S0 + 0x1A8u));
        c.S2 = 0x00000001u;
        c.RA = 0x8001B5BCu;
        GranTurismo2PC.func_8001D5C8(c, m);
        MemoryAccess.WriteU8(m, (c.S0 + 0x1B0u), (byte)0u);
        goto L8001B5C8;
        L8001B5C4: ;
        MemoryAccess.WriteU8(m, (c.S0 + 0x1B0u), (byte)c.V0);
        L8001B5C8: ;
        if (c.S2 == 0u) {
            c.V0 = 0x00000004u;
            goto L8001B5D4;
        }
        c.V0 = 0x00000004u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x72u), (ushort)c.V0);
        L8001B5D4: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x72u));
        if ((int)c.V0 <= 0) {
            goto L8001B5E8;
        }
        MemoryAccess.WriteU32(m, (c.S0 + 0x40u), 0u);
        L8001B5E8: ;
        c.RA = 0x8001B5F0u;
        GranTurismo2PC.func_80081EA8(c, m);
        MemoryAccess.WriteU32(m, (c.S0 + 0x858u), c.V0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x30u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x2Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.SP = c.SP + 0x38u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001B610(CpuContext c, IMemory m)
    {
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.A0 + 0x74u));
        if (c.V0 != 0u) {
            c.V0 = 0x00000001u;
            goto L8001B634;
        }
        c.V0 = 0x00000001u;
        c.V1 = c.V0 + 0u;
        MemoryAccess.WriteU8(m, (c.A0 + 0x74u), (byte)c.V1);
        MemoryAccess.WriteU32(m, (c.A0 + 0x78u), c.A1);
        MemoryAccess.WriteU32(m, (c.A0 + 0x7Cu), c.A2);
        return;
        L8001B634: ;
        c.V0 = 0u + 0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001B63C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x74u));
        if (c.V0 == 0u) {
            c.A1 = 0u + 0u;
            goto L8001B670;
        }
        c.A1 = 0u + 0u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x78u));
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x7Cu));
        c.A2 = 0x00000002u;
        c.RA = 0x8001B66Cu;
        Dispatcher.Call(c, m, c.V0);
        MemoryAccess.WriteU8(m, (c.S0 + 0x74u), (byte)0u);
        L8001B670: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001B680_gt2_overlay_4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.V0 = c.A1 & 0xFFFFu;
        if (c.V0 != 0u) {
            c.V1 = 0x00000001u;
            goto L8001B6E0;
        }
        c.V1 = 0x00000001u;
        c.V0 = c.A1 >> 18;
        c.V0 = c.V0 & c.V1;
        if (c.V0 == 0u) {
            goto L8001B6E0;
        }
        c.A0 = MemoryAccess.ReadU8(m, (c.A0 + 0x4Au));
        if (c.A0 == 0u) {
            c.V1 = 0u + 0u;
            goto L8001B6D0;
        }
        c.V1 = 0u + 0u;
        c.V0 = 0x00000001u;
        if (c.A0 == c.V0) {
            c.A0 = 0u + 0u;
            goto L8001B6D4;
        }
        c.A0 = 0u + 0u;
        goto L8001B6E0;
        L8001B6D0: ;
        c.A0 = 0x00000001u;
        L8001B6D4: ;
        c.RA = 0x8001B6DCu;
        GranTurismo2PC.func_8001915C(c, m);
        c.V1 = c.V0 + 0u;
        L8001B6E0: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = c.V1 + 0u;
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001B6F0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.V1 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.V0 = c.V1 >> 27;
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u) {
            c.S0 = 0u + 0u;
            goto L8001B764;
        }
        c.S0 = 0u + 0u;
        c.V0 = c.V1 >> 19;
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 != 0u) {
            c.V0 = c.S0 + 0u;
            goto L8001B808;
        }
        c.V0 = c.S0 + 0u;
        c.V0 = c.V1 >> 23;
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 != 0u) {
            c.V0 = 0u + 0u;
            goto L8001B808;
        }
        c.V0 = 0u + 0u;
        c.V0 = c.V1 >> 22;
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u) {
            c.V0 = c.V1 >> 21;
            goto L8001B74C;
        }
        c.V0 = c.V1 >> 21;
        L8001B744: ;
        c.V0 = 0u + 0u;
        goto L8001B808;
        L8001B74C: ;
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 != 0u) {
            c.V0 = c.V1 >> 20;
            goto L8001B744;
        }
        c.V0 = c.V1 >> 20;
        c.V0 = c.V0 & 0x0001u;
        c.V0 = c.V0 ^ 0x0001u;
        goto L8001B808;
        L8001B764: ;
        c.V1 = c.V1 & 0xFFFFu;
        c.V0 = c.V1 < 0x000000BCu ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80020000u;
            goto L8001B804;
        }
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x3A0Cu;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x8001B7ACu: goto L8001B7AC;
            case 0x8001B800u: goto L8001B800;
            case 0x8001B804u: goto L8001B804;
            case 0x8001B790u: goto L8001B790;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L8001B790: ;
        c.RA = 0x8001B798u;
        GranTurismo2PC.func_800174D0(c, m);
        c.V0 = (int)c.V0 < 2 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = c.S0 + 0u;
            goto L8001B808;
        }
        c.V0 = c.S0 + 0u;
        c.V0 = 0x00000001u;
        goto L8001B808;
        L8001B7AC: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.V0 = c.V0 >> 18;
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000001u;
            goto L8001B808;
        }
        c.V0 = 0x00000001u;
        c.A0 = MemoryAccess.ReadU8(m, (c.A0 + 0x4Au));
        if (c.A0 == 0u) {
            goto L8001B7E4;
        }
        if (c.A0 == c.V0) {
            c.V0 = c.S0 + 0u;
            goto L8001B7EC;
        }
        c.V0 = c.S0 + 0u;
        goto L8001B808;
        L8001B7E4: ;
        c.A0 = 0x00000001u;
        goto L8001B7F0;
        L8001B7EC: ;
        c.A0 = 0u + 0u;
        L8001B7F0: ;
        c.RA = 0x8001B7F8u;
        GranTurismo2PC.func_8001915C(c, m);
        c.S0 = c.V0 + 0u;
        goto L8001B804;
        L8001B800: ;
        c.S0 = 0x00000001u;
        L8001B804: ;
        c.V0 = c.S0 + 0u;
        L8001B808: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001B818(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x40u;
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.S6);
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x50u));
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x54u));
        MemoryAccess.WriteU32(m, (c.SP + 0x38u), c.FP);
        c.FP = MemoryAccess.ReadU32(m, (c.SP + 0x58u));
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S3);
        c.S3 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S0);
        c.S0 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S2);
        c.S2 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S4);
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x60u));
        c.A0 = c.S0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.S5);
        c.S5 = c.A3 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x3Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x34u), c.S7);
        c.RA = 0x8001B86Cu;
        GranTurismo2PC.func_800182A8(c, m);
        c.A0 = c.S0 + 0u;
        c.S7 = c.V0 + 0u;
        c.RA = 0x8001B878u;
        Dispatcher.Call(c, m, 0x800182FCu);
        if (c.S1 == 0u) {
            c.S0 = c.V0 + 0u;
            goto L8001B900;
        }
        c.S0 = c.V0 + 0u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x5Cu));
        if (c.T0 == 0u) {
            c.A0 = c.S3 + 0u;
            goto L8001B8B0;
        }
        c.A0 = c.S3 + 0u;
        c.A1 = c.S0 + 0u;
        c.A2 = c.S2 + 0u;
        c.A3 = c.S5 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S6);
        c.RA = 0x8001B8A8u;
        GranTurismo2PC.func_8001FC28(c, m);
        c.V0 = c.V0 + 0x8u;
        c.S2 = c.S2 - c.V0;
        L8001B8B0: ;
        c.A0 = c.S3 + 0u;
        c.A1 = c.S7 + 0u;
        c.A2 = c.S2 + 0u;
        c.A3 = c.S5 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S6);
        c.RA = 0x8001B8CCu;
        GranTurismo2PC.func_8001FC28(c, m);
        if (c.FP == 0u) {
            c.S2 = c.S2 - c.V0;
            goto L8001B97C;
        }
        c.S2 = c.S2 - c.V0;
        c.A0 = 0x00000001u;
        c.RA = 0x8001B8DCu;
        GranTurismo2PC.func_8001828C(c, m);
        c.A0 = c.S3 + 0u;
        c.A1 = c.V0 + 0u;
        c.A2 = c.S2 + 0u;
        c.A3 = c.S5 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S6);
        c.RA = 0x8001B8F8u;
        GranTurismo2PC.func_8001FC28(c, m);
        goto L8001B97C;
        L8001B900: ;
        if (c.FP == 0u) {
            c.A0 = c.S3 + 0u;
            goto L8001B934;
        }
        c.A0 = c.S3 + 0u;
        c.A0 = 0x00000001u;
        c.RA = 0x8001B910u;
        GranTurismo2PC.func_8001828C(c, m);
        c.A0 = c.S3 + 0u;
        c.A1 = c.V0 + 0u;
        c.A2 = c.S2 + 0u;
        c.A3 = c.S5 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S6);
        c.RA = 0x8001B92Cu;
        GranTurismo2PC.func_8001FBEC(c, m);
        c.S2 = c.S2 + c.V0;
        c.A0 = c.S3 + 0u;
        L8001B934: ;
        c.A1 = c.S7 + 0u;
        c.A2 = c.S2 + 0u;
        c.A3 = c.S5 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S6);
        c.RA = 0x8001B94Cu;
        GranTurismo2PC.func_8001FBEC(c, m);
        c.V1 = c.S2 + 0x8u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x5Cu));
        if (c.T0 == 0u) {
            c.S2 = c.V1 + c.V0;
            goto L8001B97C;
        }
        c.S2 = c.V1 + c.V0;
        c.A0 = c.S3 + 0u;
        c.A1 = c.S0 + 0u;
        c.A2 = c.S2 + 0u;
        c.A3 = c.S5 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S6);
        c.RA = 0x8001B97Cu;
        GranTurismo2PC.func_8001FBEC(c, m);
        L8001B97C: ;
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
    public static void func_8001B9AC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x370u;
        MemoryAccess.WriteU32(m, (c.SP + 0x35Cu), c.S5);
        c.S5 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x360u), c.S6);
        c.S6 = 0x006E0000u;
        c.S6 = c.S6 | 0x6E6Eu;
        MemoryAccess.WriteU32(m, (c.SP + 0x364u), c.S7);
        c.S7 = c.SP + 0x228u;
        MemoryAccess.WriteU32(m, (c.SP + 0x370u), c.A0);
        c.T1 = c.A0 + 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x36Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x368u), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0x358u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x354u), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x350u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x34Cu), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x348u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x37Cu), c.A3);
        MemoryAccess.WriteU32(m, (c.SP + 0x340u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x344u), c.T1);
        L8001B9FC: ;
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x728Cu));
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x340u));
        c.V0 = (int)c.T1 < (int)c.V0 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L8001D010;
        }
        c.A0 = c.T1 + 0u;
        c.RA = 0x8001BA20u;
        GranTurismo2PC.func_800215C8(c, m);
        c.S2 = c.V0 + 0u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S2 + 0x8u));
        c.A3 = 0xFFFFFFFFu;
        c.A2 = 0x80000000u;
        c.S3 = c.V0 & 0xFFFFu;
        c.A0 = c.V0 + 0u;
        c.A1 = 0u + 0u;
        c.V0 = c.V0 >> 27;
        c.V0 = c.V0 & 0x0001u;
        c.A0 = c.A0 & c.A2;
        c.A1 = c.A1 & c.A3;
        c.A0 = c.A1 | c.A0;
        if (c.V0 == 0u) {
            c.S4 = 0u < c.A0 ? 1u : 0u;
            goto L8001BE8C;
        }
        c.S4 = 0u < c.A0 ? 1u : 0u;
        c.A0 = c.S2 + 0xCu;
        c.RA = 0x8001BA60u;
        GranTurismo2PC.func_800188B0(c, m);
        c.FP = c.V0 + 0u;
        c.V0 = 0x000000CFu;
        if (c.S3 != c.V0) {
            goto L8001BAA0;
        }
        c.A0 = c.FP + 0u;
        c.RA = 0x8001BA78u;
        GranTurismo2PC.func_80019718(c, m);
        c.A0 = c.SP + 0x28u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001BA84u;
        GranTurismo2PC.func_8001FCDC(c, m);
        c.A0 = c.S5 + 0u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x4u));
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A1 = c.SP + 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S4);
        c.RA = 0x8001BAA0u;
        GranTurismo2PC.func_8001FC28(c, m);
        L8001BAA0: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S2 + 0x8u));
        c.V0 = c.V0 >> 19;
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u) {
            c.T1 = 0x801D0000u;
            goto L8001BBD8;
        }
        c.T1 = 0x801D0000u;
        c.T1 = c.T1 - 0x6720u;
        c.A0 = c.T1 + 0xB8u;
        c.A1 = c.FP + 0u;
        c.RA = 0x8001BAC8u;
        GranTurismo2PC.func_8005DB90(c, m);
        c.A2 = c.V0 + 0u;
        if (c.A2 == 0u) {
            goto L8001BBD8;
        }
        if ((int)c.A2 < 0) {
            c.V0 = (int)c.A2 < 4 ? 1u : 0u;
            goto L8001BB8C;
        }
        c.V0 = (int)c.A2 < 4 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V1 = 0x00000004u;
            goto L8001BB8C;
        }
        c.V1 = 0x00000004u;
        c.V1 = c.V1 - c.A2;
        c.V0 = c.V1 << 1;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 2;
        c.T1 = 0x80050000u;
        c.T1 = c.T1 + 0x978u;
        c.S1 = c.V0 + c.T1;
        c.A0 = c.S5 + 0u;
        c.A1 = 0x00800000u;
        c.A1 = c.A1 | 0x8080u;
        c.RA = 0x8001BB10u;
        GranTurismo2PC.func_80081478(c, m);
        c.A2 = c.V0 + 0u;
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x4u));
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x2u));
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A1 = c.A1 + c.V1;
        c.A1 = (uint)((int)c.A1 >> 1);
        c.A0 = c.A0 + c.V0;
        c.A0 = (uint)((int)c.A0 >> 1);
        c.V1 = MemoryAccess.ReadU16(m, (c.S1 + 0x4u));
        c.V0 = MemoryAccess.ReadU16(m, (c.S1 + 0x6u));
        c.V1 = c.V1 << 16;
        c.V1 = (uint)((int)c.V1 >> 17);
        c.A1 = c.A1 - c.V1;
        c.V0 = c.V0 << 16;
        c.V0 = (uint)((int)c.V0 >> 17);
        c.A0 = c.A0 - c.V0;
        c.A0 = c.A0 << 16;
        c.A1 = c.A1 + c.A0;
        MemoryAccess.WriteU32(m, c.A2, c.A1);
        c.V0 = MemoryAccess.ReadU32(m, c.S1);
        MemoryAccess.WriteU32(m, (c.A2 + 0x4u), c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x4u));
        MemoryAccess.WriteU32(m, (c.A2 + 0x8u), c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x8u));
        c.A0 = c.S5 + 0u;
        c.RA = 0x8001BB84u;
        GranTurismo2PC.func_8007DA44(c, m);
        goto L8001BBD8;
        L8001BB8C: ;
        c.A0 = c.S7 + 0u;
        c.A1 = 0x80020000u;
        c.A1 = c.A1 + 0x3CFCu;
        c.RA = 0x8001BB9Cu;
        GranTurismo2PC.func_8008CF34(c, m);
        c.A0 = c.S5 + 0u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x4u));
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x2u));
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A1 = c.S7 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S4);
        c.A2 = c.A2 + c.V0;
        c.A2 = (uint)((int)c.A2 >> 1);
        c.A2 = c.A2 - 0x4u;
        c.A3 = c.A3 + c.V1;
        c.A3 = (uint)((int)c.A3 >> 1);
        c.A3 = c.A3 + 0x4u;
        c.RA = 0x8001BBD8u;
        GranTurismo2PC.func_8001FC64(c, m);
        L8001BBD8: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S2 + 0x8u));
        c.V0 = c.V0 >> 23;
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u) {
            c.T1 = 0x801D0000u;
            goto L8001BCD8;
        }
        c.T1 = 0x801D0000u;
        c.T1 = c.T1 - 0x6720u;
        c.A0 = c.T1 + 0xB8u;
        c.A1 = c.FP + 0u;
        c.RA = 0x8001BC00u;
        GranTurismo2PC.func_8005DB90(c, m);
        c.A2 = c.V0 + 0u;
        if (c.A2 == 0u) {
            c.V0 = 0x00000001u;
            goto L8001BCD8;
        }
        c.V0 = 0x00000001u;
        if (c.A2 != c.V0) {
            c.A0 = c.S7 + 0u;
            goto L8001BCB0;
        }
        c.A0 = c.S7 + 0u;
        c.S0 = 0x80050000u;
        c.S1 = c.S0 + 0xA5Cu;
        c.A0 = c.S5 + 0u;
        c.A1 = 0x00800000u;
        c.A1 = c.A1 | 0x8080u;
        c.RA = 0x8001BC2Cu;
        GranTurismo2PC.func_80081478(c, m);
        c.A2 = c.V0 + 0u;
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x4u));
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x2u));
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A1 = c.A1 + c.V1;
        c.A1 = (uint)((int)c.A1 >> 1);
        c.A0 = c.A0 + c.V0;
        c.A0 = (uint)((int)c.A0 >> 1);
        c.V1 = MemoryAccess.ReadU16(m, (c.S1 + 0x4u));
        c.V0 = MemoryAccess.ReadU16(m, (c.S1 + 0x6u));
        c.V1 = c.V1 << 16;
        c.V1 = (uint)((int)c.V1 >> 17);
        c.V1 = c.V1 + 0x2u;
        c.A1 = c.A1 - c.V1;
        c.V0 = c.V0 << 16;
        c.V0 = (uint)((int)c.V0 >> 17);
        c.V0 = c.V0 - 0x6u;
        c.A0 = c.A0 - c.V0;
        c.A0 = c.A0 << 16;
        c.A1 = c.A1 + c.A0;
        MemoryAccess.WriteU32(m, c.A2, c.A1);
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0xA5Cu));
        MemoryAccess.WriteU32(m, (c.A2 + 0x4u), c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x4u));
        MemoryAccess.WriteU32(m, (c.A2 + 0x8u), c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x8u));
        c.A0 = c.S5 + 0u;
        c.RA = 0x8001BCA8u;
        GranTurismo2PC.func_8007DA44(c, m);
        goto L8001BCD8;
        L8001BCB0: ;
        c.A1 = 0x80020000u;
        c.A1 = c.A1 + 0x3CFCu;
        c.RA = 0x8001BCBCu;
        GranTurismo2PC.func_8008CF34(c, m);
        c.A0 = c.S5 + 0u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A1 = c.S7 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S4);
        c.RA = 0x8001BCD8u;
        GranTurismo2PC.func_8001FC64(c, m);
        L8001BCD8: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S2 + 0x8u));
        c.V0 = c.V0 >> 22;
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u) {
            goto L8001BD34;
        }
        c.V0 = MemoryAccess.ReadU8(m, (c.S2 + 0x4Bu));
        c.A1 = c.V0 - 0x1u;
        if ((int)c.A1 < 0) {
            goto L8001BD34;
        }
        c.A0 = c.FP + 0u;
        c.RA = 0x8001BD0Cu;
        GranTurismo2PC.func_800196EC(c, m);
        c.A0 = c.SP + 0x28u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001BD18u;
        GranTurismo2PC.func_8001FCDC(c, m);
        c.A0 = c.S5 + 0u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x4u));
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A1 = c.SP + 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S4);
        c.RA = 0x8001BD34u;
        GranTurismo2PC.func_8001FC28(c, m);
        L8001BD34: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S2 + 0x8u));
        c.V0 = c.V0 >> 21;
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u) {
            goto L8001BDC8;
        }
        c.A0 = c.FP + 0u;
        c.RA = 0x8001BD54u;
        GranTurismo2PC.func_800196C8(c, m);
        c.A0 = c.S7 + 0u;
        c.V1 = 0x801C0000u;
        c.S1 = c.V1 + 0x30E2u;
        c.A1 = c.S1 + 0u;
        c.S0 = c.V0 + 0u;
        c.RA = 0x8001BD6Cu;
        GranTurismo2PC.func_8008CEDC(c, m);
        if ((int)c.S0 <= 0) {
            c.V0 = 0x20500000u;
            goto L8001BDAC;
        }
        c.V0 = 0x20500000u;
        c.V0 = c.V0 | 0xC9F9u;
        c.A2 = c.S0 << 5;
        c.A2 = c.A2 - c.S0;
        c.A2 = c.A2 << 2;
        c.A2 = c.A2 + c.S0;
        c.A2 = c.A2 << 3;
        { var _r = (long)(int)c.A2 * (int)c.V0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.A0 = c.S7 + 0u;
        c.A1 = c.S1 - 0x6u;
        c.A2 = (uint)((int)c.A2 >> 31);
        c.T1 = c.HI;
        c.V0 = (uint)((int)c.T1 >> 7);
        c.A2 = c.V0 - c.A2;
        c.RA = 0x8001BDACu;
        GranTurismo2PC.func_8008CF34(c, m);
        L8001BDAC: ;
        c.A0 = c.S5 + 0u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A1 = c.S7 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S4);
        c.RA = 0x8001BDC8u;
        GranTurismo2PC.func_8001FC64(c, m);
        L8001BDC8: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S2 + 0x8u));
        c.V0 = c.V0 >> 20;
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u) {
            c.V0 = c.S3 < 0x000000D1u ? 1u : 0u;
            goto L8001BE90;
        }
        c.V0 = c.S3 < 0x000000D1u ? 1u : 0u;
        c.A0 = c.FP + 0u;
        c.RA = 0x8001BDE8u;
        GranTurismo2PC.func_80019634(c, m);
        c.V0 = c.V0 + 0x1u;
        if ((int)c.V0 < 0) {
            c.V1 = c.V0 << 1;
            goto L8001BE8C;
        }
        c.V1 = c.V0 << 1;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 2;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 + 0x9FCu;
        c.S1 = c.V1 + c.V0;
        c.A0 = c.S5 + 0u;
        c.A1 = 0x00800000u;
        c.A1 = c.A1 | 0x8080u;
        c.RA = 0x8001BE18u;
        GranTurismo2PC.func_80081478(c, m);
        c.A2 = c.V0 + 0u;
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x4u));
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x2u));
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A1 = c.A1 + c.V1;
        c.A1 = (uint)((int)c.A1 >> 1);
        c.A0 = c.A0 + c.V0;
        c.A0 = (uint)((int)c.A0 >> 1);
        c.V1 = MemoryAccess.ReadU16(m, (c.S1 + 0x4u));
        c.V0 = MemoryAccess.ReadU16(m, (c.S1 + 0x6u));
        c.V1 = c.V1 << 16;
        c.V1 = (uint)((int)c.V1 >> 17);
        c.A1 = c.A1 - c.V1;
        c.V0 = c.V0 << 16;
        c.V0 = (uint)((int)c.V0 >> 17);
        c.A0 = c.A0 - c.V0;
        c.A0 = c.A0 << 16;
        c.A1 = c.A1 + c.A0;
        MemoryAccess.WriteU32(m, c.A2, c.A1);
        c.V0 = MemoryAccess.ReadU32(m, c.S1);
        MemoryAccess.WriteU32(m, (c.A2 + 0x4u), c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x4u));
        MemoryAccess.WriteU32(m, (c.A2 + 0x8u), c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x8u));
        c.A0 = c.S5 + 0u;
        c.RA = 0x8001BE8Cu;
        GranTurismo2PC.func_8007DA44(c, m);
        L8001BE8C: ;
        c.V0 = c.S3 < 0x000000D1u ? 1u : 0u;
        L8001BE90: ;
        if (c.V0 == 0u) {
            c.V0 = 0x80020000u;
            goto L8001CFFC;
        }
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x3D14u;
        c.V1 = c.S3 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x8001BEB4u: goto L8001BEB4;
            case 0x8001CFFCu: goto L8001CFFC;
            case 0x8001CBC4u: goto L8001CBC4;
            case 0x8001CD64u: goto L8001CD64;
            case 0x8001C3BCu: goto L8001C3BC;
            case 0x8001CD98u: goto L8001CD98;
            case 0x8001CD0Cu: goto L8001CD0C;
            case 0x8001C9F0u: goto L8001C9F0;
            case 0x8001CAECu: goto L8001CAEC;
            case 0x8001CA5Cu: goto L8001CA5C;
            case 0x8001C990u: goto L8001C990;
            case 0x8001C918u: goto L8001C918;
            case 0x8001C430u: goto L8001C430;
            case 0x8001CC2Cu: goto L8001CC2C;
            case 0x8001C414u: goto L8001C414;
            case 0x8001C3E8u: goto L8001C3E8;
            case 0x8001CEBCu: goto L8001CEBC;
            case 0x8001CE74u: goto L8001CE74;
            case 0x8001CE98u: goto L8001CE98;
            case 0x8001CEE0u: goto L8001CEE0;
            case 0x8001CF04u: goto L8001CF04;
            case 0x8001CF28u: goto L8001CF28;
            case 0x8001CD44u: goto L8001CD44;
            case 0x8001C8E8u: goto L8001C8E8;
            case 0x8001C890u: goto L8001C890;
            case 0x8001CB50u: goto L8001CB50;
            case 0x8001C35Cu: goto L8001C35C;
            case 0x8001C484u: goto L8001C484;
            case 0x8001C4B4u: goto L8001C4B4;
            case 0x8001C4E4u: goto L8001C4E4;
            case 0x8001C514u: goto L8001C514;
            case 0x8001C58Cu: goto L8001C58C;
            case 0x8001C610u: goto L8001C610;
            case 0x8001C64Cu: goto L8001C64C;
            case 0x8001C6ACu: goto L8001C6AC;
            case 0x8001C780u: goto L8001C780;
            case 0x8001BF78u: goto L8001BF78;
            case 0x8001BFCCu: goto L8001BFCC;
            case 0x8001BFF0u: goto L8001BFF0;
            case 0x8001C000u: goto L8001C000;
            case 0x8001C010u: goto L8001C010;
            case 0x8001C064u: goto L8001C064;
            case 0x8001C0B8u: goto L8001C0B8;
            case 0x8001C0C8u: goto L8001C0C8;
            case 0x8001C0ECu: goto L8001C0EC;
            case 0x8001C13Cu: goto L8001C13C;
            case 0x8001C210u: goto L8001C210;
            case 0x8001C290u: goto L8001C290;
            case 0x8001C32Cu: goto L8001C32C;
            case 0x8001C46Cu: goto L8001C46C;
            case 0x8001C44Cu: goto L8001C44C;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L8001BEB4: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S2 + 0x8u));
        c.V0 = c.V0 >> 18;
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u) {
            goto L8001CFFC;
        }
        c.A0 = MemoryAccess.ReadU8(m, (c.S2 + 0x4Au));
        if (c.A0 == 0u) {
            c.V1 = 0u + 0u;
            goto L8001BEF0;
        }
        c.V1 = 0u + 0u;
        c.V0 = 0x00000001u;
        if (c.A0 == c.V0) {
            c.A0 = 0u + 0u;
            goto L8001BEF4;
        }
        c.A0 = 0u + 0u;
        goto L8001BF00;
        L8001BEF0: ;
        c.A0 = 0x00000001u;
        L8001BEF4: ;
        c.RA = 0x8001BEFCu;
        GranTurismo2PC.func_8001915C(c, m);
        c.V1 = c.V0 + 0u;
        L8001BF00: ;
        if (c.V1 == 0u) {
            c.S0 = 0x80050000u;
            goto L8001CFFC;
        }
        c.S0 = 0x80050000u;
        c.S1 = c.S0 + 0xA50u;
        c.A0 = c.S5 + 0u;
        c.A1 = 0x00800000u;
        c.A1 = c.A1 | 0x8080u;
        c.RA = 0x8001BF1Cu;
        GranTurismo2PC.func_80081478(c, m);
        c.A2 = c.V0 + 0u;
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x4u));
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x2u));
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A1 = c.A1 + c.V1;
        c.A1 = (uint)((int)c.A1 >> 1);
        c.A0 = c.A0 + c.V0;
        c.A0 = (uint)((int)c.A0 >> 1);
        c.V1 = MemoryAccess.ReadU16(m, (c.S1 + 0x4u));
        c.V0 = MemoryAccess.ReadU16(m, (c.S1 + 0x6u));
        c.V1 = c.V1 << 16;
        c.V1 = (uint)((int)c.V1 >> 17);
        c.A1 = c.A1 - c.V1;
        c.V0 = c.V0 << 16;
        c.V0 = (uint)((int)c.V0 >> 17);
        c.A0 = c.A0 - c.V0;
        c.A0 = c.A0 << 16;
        c.A1 = c.A1 + c.A0;
        MemoryAccess.WriteU32(m, c.A2, c.A1);
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0xA50u));
        MemoryAccess.WriteU32(m, (c.A2 + 0x4u), c.V0);
        goto L8001CFE4;
        L8001BF78: ;
        c.RA = 0x8001BF80u;
        GranTurismo2PC.func_80019D38(c, m);
        c.V1 = 0x51EB0000u;
        c.V1 = c.V1 | 0x851Fu;
        { var _r = (long)(int)c.V0 * (int)c.V1; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.A0 = c.S7 + 0u;
        c.A1 = 0x80020000u;
        c.A1 = c.A1 + 0x3D00u;
        c.V1 = (uint)((int)c.V0 >> 31);
        c.T1 = c.HI;
        c.A2 = (uint)((int)c.T1 >> 5);
        c.A2 = c.A2 - c.V1;
        c.A3 = c.A2 << 1;
        c.A3 = c.A3 + c.A2;
        c.A3 = c.A3 << 3;
        c.A3 = c.A3 + c.A2;
        c.A3 = c.A3 << 2;
        c.A3 = c.V0 - c.A3;
        c.RA = 0x8001BFC4u;
        GranTurismo2PC.func_8008CF34(c, m);
        c.A0 = c.S5 + 0u;
        goto L8001C30C;
        L8001BFCC: ;
        c.A0 = c.SP + 0x330u;
        c.A1 = c.SP + 0x334u;
        c.RA = 0x8001BFD8u;
        GranTurismo2PC.func_80019DF8(c, m);
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0x330u));
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0x334u));
        c.A0 = c.SP + 0x28u;
        c.RA = 0x8001BFE8u;
        GranTurismo2PC.func_8001FE0C(c, m);
        c.A0 = c.S5 + 0u;
        goto L8001CD78;
        L8001BFF0: ;
        c.RA = 0x8001BFF8u;
        GranTurismo2PC.func_80019C60(c, m);
        c.A0 = c.S7 + 0u;
        goto L8001C2F8;
        L8001C000: ;
        c.RA = 0x8001C008u;
        GranTurismo2PC.func_80019C70(c, m);
        c.A0 = c.S7 + 0u;
        goto L8001C2F8;
        L8001C010: ;
        c.RA = 0x8001C018u;
        GranTurismo2PC.func_80019C80(c, m);
        c.V1 = 0x51EB0000u;
        c.V1 = c.V1 | 0x851Fu;
        { var _r = (long)(int)c.V0 * (int)c.V1; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.A0 = c.S7 + 0u;
        c.A1 = 0x80020000u;
        c.A1 = c.A1 + 0x3D00u;
        c.V1 = (uint)((int)c.V0 >> 31);
        c.T1 = c.HI;
        c.A2 = (uint)((int)c.T1 >> 5);
        c.A2 = c.A2 - c.V1;
        c.A3 = c.A2 << 1;
        c.A3 = c.A3 + c.A2;
        c.A3 = c.A3 << 3;
        c.A3 = c.A3 + c.A2;
        c.A3 = c.A3 << 2;
        c.A3 = c.V0 - c.A3;
        c.RA = 0x8001C05Cu;
        GranTurismo2PC.func_8008CF34(c, m);
        c.A0 = c.S5 + 0u;
        goto L8001C30C;
        L8001C064: ;
        c.RA = 0x8001C06Cu;
        GranTurismo2PC.func_80019CF4(c, m);
        c.V1 = 0x51EB0000u;
        c.V1 = c.V1 | 0x851Fu;
        { var _r = (long)(int)c.V0 * (int)c.V1; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.A0 = c.S7 + 0u;
        c.A1 = 0x80020000u;
        c.A1 = c.A1 + 0x3D00u;
        c.V1 = (uint)((int)c.V0 >> 31);
        c.T1 = c.HI;
        c.A2 = (uint)((int)c.T1 >> 5);
        c.A2 = c.A2 - c.V1;
        c.A3 = c.A2 << 1;
        c.A3 = c.A3 + c.A2;
        c.A3 = c.A3 << 3;
        c.A3 = c.A3 + c.A2;
        c.A3 = c.A3 << 2;
        c.A3 = c.V0 - c.A3;
        c.RA = 0x8001C0B0u;
        GranTurismo2PC.func_8008CF34(c, m);
        c.A0 = c.S5 + 0u;
        goto L8001C30C;
        L8001C0B8: ;
        c.RA = 0x8001C0C0u;
        GranTurismo2PC.func_80019E1C(c, m);
        c.A0 = c.S7 + 0u;
        goto L8001C2F8;
        L8001C0C8: ;
        c.A0 = c.SP + 0x338u;
        c.A1 = c.SP + 0x33Cu;
        c.RA = 0x8001C0D4u;
        GranTurismo2PC.func_80019E2C(c, m);
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0x338u));
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0x33Cu));
        c.A0 = c.SP + 0x28u;
        c.RA = 0x8001C0E4u;
        GranTurismo2PC.func_8001FE0C(c, m);
        c.A0 = c.S5 + 0u;
        goto L8001CD78;
        L8001C0EC: ;
        c.T1 = 0x801D0000u;
        c.T1 = c.T1 - 0x6720u;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.T1 + 0x3BD0u));
        if ((int)c.V0 <= 0) {
            goto L8001CFFC;
        }
        c.A1 = MemoryAccess.ReadU32(m, (c.T1 + 0x3BD8u));
        c.S1 = MemoryAccess.ReadU32(m, (c.T1 + 0x3BD4u));
        c.S0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x4u));
        c.A0 = c.SP + 0x28u;
        c.RA = 0x8001C118u;
        GranTurismo2PC.func_80020054(c, m);
        c.A0 = c.S5 + 0u;
        c.A1 = c.SP + 0x28u;
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A2 = c.S0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S4);
        c.RA = 0x8001C134u;
        GranTurismo2PC.func_8001FC28(c, m);
        c.A0 = c.S5 + 0u;
        goto L8001C25C;
        L8001C13C: ;
        c.S3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x4u));
        c.RA = 0x8001C148u;
        GranTurismo2PC.func_80019E98(c, m);
        if ((int)c.V0 < 0) {
            c.V1 = 0x20500000u;
            goto L8001CFFC;
        }
        c.V1 = 0x20500000u;
        c.S0 = c.V0 << 2;
        c.S0 = c.S0 + c.V0;
        c.S0 = c.S0 << 3;
        c.S0 = c.S0 + c.V0;
        c.S0 = c.S0 << 2;
        c.S0 = c.S0 + 0x4u;
        c.T1 = 0x801D0000u;
        c.T1 = c.T1 - 0x2AACu;
        c.S0 = c.S0 + c.T1;
        c.V0 = MemoryAccess.ReadU16(m, (c.S0 + 0x98u));
        c.V1 = c.V1 | 0xC9F9u;
        c.V0 = c.V0 & 0x3FFFu;
        c.A1 = c.V0 << 5;
        c.A1 = c.A1 - c.V0;
        c.A1 = c.A1 << 2;
        c.A1 = c.A1 + c.V0;
        c.A1 = c.A1 << 3;
        { var _r = (long)(int)c.A1 * (int)c.V1; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.A0 = c.SP + 0x28u;
        c.S1 = MemoryAccess.ReadU32(m, c.S0);
        c.A1 = (uint)((int)c.A1 >> 31);
        c.T1 = c.HI;
        c.V0 = (uint)((int)c.T1 >> 7);
        c.A1 = c.V0 - c.A1;
        c.RA = 0x8001C1B4u;
        GranTurismo2PC.func_8001FFCC(c, m);
        c.A0 = c.S5 + 0u;
        c.A1 = c.SP + 0x28u;
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A2 = c.S3 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S4);
        c.RA = 0x8001C1D0u;
        GranTurismo2PC.func_8001FC28(c, m);
        c.A0 = c.S5 + 0u;
        c.A1 = c.S1 + 0u;
        c.V0 = c.V0 + 0xCu;
        c.A2 = c.S3 - c.V0;
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.V1 = MemoryAccess.ReadU16(m, (c.S0 + 0x98u));
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S6);
        c.V1 = c.V1 >> 15;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.V1);
        c.RA = 0x8001C208u;
        GranTurismo2PC.func_8001B818(c, m);
        goto L8001CFFC;
        L8001C210: ;
        c.T1 = 0x801D0000u;
        c.T1 = c.T1 - 0x6720u;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.T1 + 0x3A88u));
        if ((int)c.V0 <= 0) {
            goto L8001CFFC;
        }
        c.A1 = MemoryAccess.ReadU32(m, (c.T1 + 0x3A90u));
        c.S1 = MemoryAccess.ReadU32(m, (c.T1 + 0x3A8Cu));
        c.S0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x4u));
        c.A0 = c.S7 + 0u;
        c.RA = 0x8001C23Cu;
        GranTurismo2PC.func_80068734(c, m);
        c.A0 = c.S5 + 0u;
        c.A1 = c.S7 + 0u;
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A2 = c.S0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S4);
        c.RA = 0x8001C258u;
        GranTurismo2PC.func_8001FCA0(c, m);
        c.A0 = c.S5 + 0u;
        L8001C25C: ;
        c.A1 = c.S1 + 0u;
        c.V0 = c.V0 + 0xCu;
        c.A2 = c.S0 - c.V0;
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S6);
        c.RA = 0x8001C288u;
        GranTurismo2PC.func_8001B818(c, m);
        goto L8001CFFC;
        L8001C290: ;
        c.V0 = 0x000000C9u;
        if (c.S3 == c.V0) {
            c.V0 = (int)c.S3 < 202 ? 1u : 0u;
            goto L8001C2D8;
        }
        c.V0 = (int)c.S3 < 202 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x000000C8u;
            goto L8001C2B4;
        }
        c.V0 = 0x000000C8u;
        if (c.S3 == c.V0) {
            goto L8001C2D0;
        }
        goto L8001C2EC;
        L8001C2B4: ;
        c.V0 = 0x000000CAu;
        if (c.S3 == c.V0) {
            c.V0 = 0x000000CBu;
            goto L8001C2E0;
        }
        c.V0 = 0x000000CBu;
        if (c.S3 == c.V0) {
            goto L8001C2E8;
        }
        goto L8001C2EC;
        L8001C2D0: ;
        c.S3 = 0x00000004u;
        goto L8001C2EC;
        L8001C2D8: ;
        c.S3 = 0x00000003u;
        goto L8001C2EC;
        L8001C2E0: ;
        c.S3 = 0x00000002u;
        goto L8001C2EC;
        L8001C2E8: ;
        c.S3 = 0x00000001u;
        L8001C2EC: ;
        c.A0 = c.S3 + 0u;
        c.RA = 0x8001C2F4u;
        GranTurismo2PC.func_80019EF8(c, m);
        c.A0 = c.S7 + 0u;
        L8001C2F8: ;
        c.A1 = 0x80020000u;
        c.A1 = c.A1 + 0x3CFCu;
        c.A2 = c.V0 + 0u;
        c.RA = 0x8001C308u;
        GranTurismo2PC.func_8008CF34(c, m);
        c.A0 = c.S5 + 0u;
        L8001C30C: ;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x4u));
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A1 = c.S7 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S4);
        c.RA = 0x8001C324u;
        GranTurismo2PC.func_8001FCA0(c, m);
        goto L8001CFFC;
        L8001C32C: ;
        c.RA = 0x8001C334u;
        GranTurismo2PC.func_800191C4(c, m);
        c.A0 = c.V0 + 0u;
        c.V0 = (int)c.A0 < 6 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V1 = c.A0 << 1;
            goto L8001CFFC;
        }
        c.V1 = c.A0 << 1;
        c.V1 = c.V1 + c.A0;
        c.V1 = c.V1 << 2;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 + 0x9B4u;
        c.S1 = c.V1 + c.V0;
        goto L8001CF74;
        L8001C35C: ;
        c.RA = 0x8001C364u;
        GranTurismo2PC.func_800174D0(c, m);
        c.V0 = (int)c.V0 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A0 = c.S5 + 0u;
            goto L8001CFFC;
        }
        c.A0 = c.S5 + 0u;
        c.A1 = 0u + 0u;
        c.RA = 0x8001C378u;
        GranTurismo2PC.func_8007D024(c, m);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        MemoryAccess.WriteU16(m, c.V0, (ushort)c.V1);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x2u));
        MemoryAccess.WriteU16(m, (c.V0 + 0x2u), (ushort)c.V1);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x4u));
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.V1 = c.V1 - c.A0;
        MemoryAccess.WriteU16(m, (c.V0 + 0x4u), (ushort)c.V1);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x2u));
        c.V1 = c.V1 - c.A0;
        MemoryAccess.WriteU16(m, (c.V0 + 0x6u), (ushort)c.V1);
        goto L8001CFFC;
        L8001C3BC: ;
        c.S0 = 0x800B0000u;
        c.S0 = c.S0 - 0x72A0u;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x00000100u;
        c.A2 = 0x000000B0u;
        c.RA = 0x8001C3D4u;
        GranTurismo2PC.func_800204D8(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S5 + 0u;
        c.RA = 0x8001C3E0u;
        GranTurismo2PC.func_8002068C(c, m);
        goto L8001CFFC;
        L8001C3E8: ;
        c.S0 = 0x800B0000u;
        c.S0 = c.S0 - 0x7298u;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x00000100u;
        c.A2 = 0x0000009Cu;
        c.RA = 0x8001C400u;
        GranTurismo2PC.func_800209F8(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S5 + 0u;
        c.RA = 0x8001C40Cu;
        GranTurismo2PC.func_80020B70(c, m);
        goto L8001CFFC;
        L8001C414: ;
        c.T1 = 0x801D0000u;
        c.T1 = c.T1 - 0x6668u;
        c.A1 = MemoryAccess.ReadU32(m, (c.T1 + 0x40u));
        c.A0 = c.SP + 0x28u;
        c.RA = 0x8001C428u;
        Dispatcher.Call(c, m, 0x80020110u);
        c.A0 = c.S5 + 0u;
        goto L8001CD78;
        L8001C430: ;
        c.T1 = 0x801D0000u;
        c.T1 = c.T1 - 0x2AACu;
        c.A1 = MemoryAccess.ReadU32(m, (c.T1 + 0x4014u));
        c.A0 = c.SP + 0x28u;
        c.RA = 0x8001C444u;
        GranTurismo2PC.func_8001FCDC(c, m);
        c.A0 = c.S5 + 0u;
        goto L8001CD78;
        L8001C44C: ;
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x344u));
        c.A1 = MemoryAccess.ReadU32(m, (c.T1 + 0x3BCu));
        c.A0 = c.SP + 0x28u;
        c.A1 = c.A1 >> 2;
        c.RA = 0x8001C464u;
        GranTurismo2PC.func_8001FCDC(c, m);
        c.A0 = c.S5 + 0u;
        goto L8001CD78;
        L8001C46C: ;
        c.A0 = c.S5 + 0u;
        c.A1 = 0x00000020u;
        c.A2 = 0x000000A0u;
        c.RA = 0x8001C47Cu;
        GranTurismo2PC.func_80017318(c, m);
        goto L8001CFFC;
        L8001C484: ;
        c.A0 = c.S7 + 0u;
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x344u));
        c.A1 = 0x801C0000u;
        c.A2 = MemoryAccess.ReadU32(m, (c.T1 + 0x398u));
        c.A1 = c.A1 + 0x311Au;
        c.RA = 0x8001C49Cu;
        GranTurismo2PC.func_8008CF34(c, m);
        c.A0 = c.S5 + 0u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A1 = c.S7 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S6);
        goto L8001CCFC;
        L8001C4B4: ;
        c.A0 = c.S7 + 0u;
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x344u));
        c.A1 = 0x801C0000u;
        c.A2 = MemoryAccess.ReadU32(m, (c.T1 + 0x39Cu));
        c.A1 = c.A1 + 0x311Au;
        c.RA = 0x8001C4CCu;
        GranTurismo2PC.func_8008CF34(c, m);
        c.A0 = c.S5 + 0u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A1 = c.S7 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S6);
        goto L8001CCFC;
        L8001C4E4: ;
        c.A0 = c.S7 + 0u;
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x344u));
        c.A1 = 0x801C0000u;
        c.A2 = MemoryAccess.ReadU32(m, (c.T1 + 0x3A0u));
        c.A1 = c.A1 + 0x311Au;
        c.RA = 0x8001C4FCu;
        GranTurismo2PC.func_8008CF34(c, m);
        c.A0 = c.S5 + 0u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A1 = c.S7 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S6);
        goto L8001CCFC;
        L8001C514: ;
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x344u));
        c.V1 = 0x68DB0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.T1 + 0x3A4u));
        c.V1 = c.V1 | 0x8BADu;
        c.A2 = c.V0 << 1;
        c.A2 = c.A2 + c.V0;
        c.A2 = c.A2 << 2;
        c.A2 = c.A2 - c.V0;
        c.A2 = c.A2 << 2;
        c.A2 = c.A2 - c.V0;
        c.A2 = c.A2 << 4;
        c.A2 = c.A2 + c.V0;
        c.A2 = c.A2 << 4;
        c.A2 = c.A2 - c.V0;
        c.A2 = c.A2 << 1;
        { var _r = (long)(int)c.A2 * (int)c.V1; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.A0 = c.S7 + 0u;
        c.A1 = 0x801F0000u;
        c.A1 = c.A1 - 0x93Fu;
        c.A2 = (uint)((int)c.A2 >> 31);
        c.T1 = c.HI;
        c.V0 = (uint)((int)c.T1 >> 12);
        c.A2 = c.V0 - c.A2;
        c.RA = 0x8001C574u;
        GranTurismo2PC.func_8008CF34(c, m);
        c.A0 = c.S5 + 0u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A1 = c.S7 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S6);
        goto L8001CCFC;
        L8001C58C: ;
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x344u));
        c.V1 = MemoryAccess.ReadU32(m, (c.T1 + 0x3A8u));
        if ((int)c.V1 > 0) {
            c.A2 = c.V1 + 0u;
            goto L8001C5BC;
        }
        c.A2 = c.V1 + 0u;
        c.A0 = c.SP + 0x228u;
        c.A1 = 0x80020000u;
        c.A1 = c.A1 + 0x3D08u;
        c.RA = 0x8001C5B4u;
        GranTurismo2PC.func_8008CF34(c, m);
        c.A0 = c.S5 + 0u;
        goto L8001C5FC;
        L8001C5BC: ;
        c.V0 = c.V1 & 0xE000u;
        c.A3 = (uint)((int)c.V0 >> 13);
        if (c.A3 == 0u) {
            c.A0 = c.SP + 0x228u;
            goto L8001C5EC;
        }
        c.A0 = c.SP + 0x228u;
        c.A2 = 0xFFFF0000u;
        c.A2 = c.A2 | 0x1FFFu;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 + 0x3140u;
        c.A2 = c.V1 & c.A2;
        c.RA = 0x8001C5E4u;
        GranTurismo2PC.func_8008CF34(c, m);
        c.A0 = c.S5 + 0u;
        goto L8001C5FC;
        L8001C5EC: ;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 + 0x311Fu;
        c.RA = 0x8001C5F8u;
        GranTurismo2PC.func_8008CF34(c, m);
        c.A0 = c.S5 + 0u;
        L8001C5FC: ;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A1 = c.SP + 0x228u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S6);
        goto L8001CCFC;
        L8001C610: ;
        c.V0 = 0x80050000u;
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x344u));
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.V1 = MemoryAccess.ReadU32(m, (c.T1 + 0x3ACu));
        c.V0 = c.V0 + 0x1260u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S4);
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.A1 = MemoryAccess.ReadU32(m, c.V1);
        c.A0 = c.S5 + 0u;
        c.RA = 0x8001C644u;
        GranTurismo2PC.func_8001FC64(c, m);
        goto L8001CFFC;
        L8001C64C: ;
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0x344u));
        c.RA = 0x8001C658u;
        GranTurismo2PC.func_8001B2B8(c, m);
        c.A0 = c.S5 + 0u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A1 = c.V0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S4);
        c.RA = 0x8001C674u;
        GranTurismo2PC.func_8001FBEC(c, m);
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0x344u));
        c.S0 = c.V0 + 0u;
        c.RA = 0x8001C680u;
        GranTurismo2PC.func_8001B338(c, m);
        c.A0 = c.S5 + 0u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A1 = c.V0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S4);
        c.A2 = c.A2 + c.S0;
        c.A2 = c.A2 + 0xCu;
        c.RA = 0x8001C6A4u;
        GranTurismo2PC.func_8001FBEC(c, m);
        goto L8001CFFC;
        L8001C6AC: ;
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x344u));
        c.V1 = MemoryAccess.ReadU32(m, (c.T1 + 0x3C0u));
        c.V0 = MemoryAccess.ReadU32(m, (c.T1 + 0x3C4u));
        c.A0 = c.V1 + 0u;
        if ((int)c.V1 > 0) {
            c.A3 = c.V0 + 0u;
            goto L8001C6E0;
        }
        c.A3 = c.V0 + 0u;
        c.A0 = c.SP + 0x228u;
        c.A1 = 0x80020000u;
        c.A1 = c.A1 + 0x3D08u;
        c.RA = 0x8001C6D8u;
        GranTurismo2PC.func_8008CF34(c, m);
        c.A0 = c.S5 + 0u;
        goto L8001C76C;
        L8001C6E0: ;
        if ((int)c.V0 > 0) {
            c.V0 = 0x20500000u;
            goto L8001C72C;
        }
        c.V0 = 0x20500000u;
        c.V0 = c.V0 | 0xC9F9u;
        c.A2 = c.V1 << 5;
        c.A2 = c.A2 - c.V1;
        c.A2 = c.A2 << 2;
        c.A2 = c.A2 + c.V1;
        c.A2 = c.A2 << 3;
        { var _r = (long)(int)c.A2 * (int)c.V0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.A0 = c.SP + 0x228u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 + 0x313Bu;
        c.A2 = (uint)((int)c.A2 >> 31);
        c.T1 = c.HI;
        c.V0 = (uint)((int)c.T1 >> 7);
        c.A2 = c.V0 - c.A2;
        c.RA = 0x8001C724u;
        GranTurismo2PC.func_8008CF34(c, m);
        c.A0 = c.S5 + 0u;
        goto L8001C76C;
        L8001C72C: ;
        c.V0 = c.V0 | 0xC9F9u;
        c.A2 = c.A0 << 5;
        c.A2 = c.A2 - c.A0;
        c.A2 = c.A2 << 2;
        c.A2 = c.A2 + c.A0;
        c.A2 = c.A2 << 3;
        { var _r = (long)(int)c.A2 * (int)c.V0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.A0 = c.SP + 0x228u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 + 0x30EDu;
        c.A2 = (uint)((int)c.A2 >> 31);
        c.T1 = c.HI;
        c.V0 = (uint)((int)c.T1 >> 7);
        c.A2 = c.V0 - c.A2;
        c.RA = 0x8001C768u;
        GranTurismo2PC.func_8008CF34(c, m);
        c.A0 = c.S5 + 0u;
        L8001C76C: ;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A1 = c.SP + 0x228u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S6);
        goto L8001CCFC;
        L8001C780: ;
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x370u));
        c.S0 = c.T1 + 0x28u;
        c.T0 = MemoryAccess.ReadU32(m, (c.S0 + 0x3C8u));
        if ((int)c.T0 > 0) {
            c.A0 = 0x68DB0000u;
            goto L8001C7C4;
        }
        c.A0 = 0x68DB0000u;
        c.A0 = c.S7 + 0u;
        c.A1 = 0x80020000u;
        c.A1 = c.A1 + 0x3D08u;
        c.RA = 0x8001C7ACu;
        GranTurismo2PC.func_8008CF34(c, m);
        c.A0 = c.S5 + 0u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A1 = c.S7 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S6);
        goto L8001CCFC;
        L8001C7C4: ;
        c.A0 = c.A0 | 0x8BADu;
        c.V0 = c.T0 << 3;
        c.V0 = c.V0 - c.T0;
        c.V0 = c.V0 << 4;
        c.V0 = c.V0 + c.T0;
        c.V1 = c.V0 << 2;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 4;
        c.V0 = c.V0 + c.T0;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.T0;
        { var _r = (long)(int)c.V0 * (int)c.A0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.A0 = 0x66660000u;
        c.A0 = c.A0 | 0x6667u;
        c.V0 = (uint)((int)c.V0 >> 31);
        c.T1 = c.HI;
        c.V1 = (uint)((int)c.T1 >> 12);
        c.T0 = c.V1 - c.V0;
        { var _r = (long)(int)c.T0 * (int)c.A0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 + 0x30FAu;
        c.A0 = c.S7 + 0u;
        c.V0 = (uint)((int)c.T0 >> 31);
        c.T1 = c.HI;
        c.A2 = (uint)((int)c.T1 >> 2);
        c.A2 = c.A2 - c.V0;
        c.A3 = c.A2 << 2;
        c.A3 = c.A3 + c.A2;
        c.A3 = c.A3 << 1;
        c.A3 = c.T0 - c.A3;
        c.RA = 0x8001C840u;
        GranTurismo2PC.func_8008CF34(c, m);
        c.A0 = c.S5 + 0u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A1 = c.S7 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S4);
        c.RA = 0x8001C85Cu;
        GranTurismo2PC.func_8001FC64(c, m);
        c.A0 = c.S0 + 0u;
        c.S0 = c.V0 + 0u;
        c.RA = 0x8001C868u;
        GranTurismo2PC.func_8001B378(c, m);
        c.A0 = c.S5 + 0u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A1 = c.V0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S4);
        c.A2 = c.A2 + c.S0;
        c.RA = 0x8001C888u;
        GranTurismo2PC.func_8001FBEC(c, m);
        goto L8001CFFC;
        L8001C890: ;
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x344u));
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.T1 + 0x394u));
        if ((int)c.V0 > 0) {
            c.A2 = c.V0 + 0u;
            goto L8001C8C0;
        }
        c.A2 = c.V0 + 0u;
        c.A0 = c.SP + 0x228u;
        c.A1 = 0x80020000u;
        c.A1 = c.A1 + 0x3D08u;
        c.RA = 0x8001C8B8u;
        GranTurismo2PC.func_8008CF34(c, m);
        c.A0 = c.S5 + 0u;
        goto L8001C8D4;
        L8001C8C0: ;
        c.A0 = c.SP + 0x228u;
        c.A1 = 0x80020000u;
        c.A1 = c.A1 + 0x3D10u;
        c.RA = 0x8001C8D0u;
        GranTurismo2PC.func_8008CF34(c, m);
        c.A0 = c.S5 + 0u;
        L8001C8D4: ;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A1 = c.SP + 0x228u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S6);
        goto L8001CCFC;
        L8001C8E8: ;
        c.A0 = c.S5 + 0u;
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x344u));
        c.A1 = 0x00800000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.T1 + 0x3ACu));
        c.A1 = c.A1 | 0x8080u;
        c.V1 = c.V0 << 1;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 2;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 + 0x93Cu;
        c.S1 = c.V1 + c.V0;
        goto L8001CF80;
        L8001C918: ;
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x344u));
        c.V1 = 0x68DB0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.T1 + 0x3A4u));
        c.V1 = c.V1 | 0x8BADu;
        c.A2 = c.V0 << 1;
        c.A2 = c.A2 + c.V0;
        c.A2 = c.A2 << 2;
        c.A2 = c.A2 - c.V0;
        c.A2 = c.A2 << 2;
        c.A2 = c.A2 - c.V0;
        c.A2 = c.A2 << 4;
        c.A2 = c.A2 + c.V0;
        c.A2 = c.A2 << 4;
        c.A2 = c.A2 - c.V0;
        c.A2 = c.A2 << 1;
        { var _r = (long)(int)c.A2 * (int)c.V1; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.A0 = c.S7 + 0u;
        c.A1 = 0x801F0000u;
        c.A1 = c.A1 - 0x93Fu;
        c.A2 = (uint)((int)c.A2 >> 31);
        c.T1 = c.HI;
        c.V0 = (uint)((int)c.T1 >> 12);
        c.A2 = c.V0 - c.A2;
        c.RA = 0x8001C978u;
        GranTurismo2PC.func_8008CF34(c, m);
        c.A0 = c.S5 + 0u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A1 = c.S7 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S6);
        goto L8001CCFC;
        L8001C990: ;
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x344u));
        c.V1 = 0x20500000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.T1 + 0x3C0u));
        c.V1 = c.V1 | 0xC9F9u;
        c.A2 = c.V0 << 5;
        c.A2 = c.A2 - c.V0;
        c.A2 = c.A2 << 2;
        c.A2 = c.A2 + c.V0;
        c.A2 = c.A2 << 3;
        { var _r = (long)(int)c.A2 * (int)c.V1; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.A0 = c.S7 + 0u;
        c.A1 = 0x801F0000u;
        c.A1 = c.A1 - 0x93Au;
        c.A2 = (uint)((int)c.A2 >> 31);
        c.T1 = c.HI;
        c.V0 = (uint)((int)c.T1 >> 7);
        c.A2 = c.V0 - c.A2;
        c.RA = 0x8001C9D8u;
        GranTurismo2PC.func_8008CF34(c, m);
        c.A0 = c.S5 + 0u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A1 = c.S7 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S6);
        goto L8001CCFC;
        L8001C9F0: ;
        c.T1 = 0x801D0000u;
        c.T1 = c.T1 - 0x2AACu;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.T1 + 0x4018u));
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        if ((int)c.V1 < 0) {
            c.V0 = c.V1 << 2;
            goto L8001CFFC;
        }
        c.V0 = c.V1 << 2;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + 0x4u;
        c.T1 = 0x801D0000u;
        c.T1 = c.T1 - 0x2AACu;
        c.V0 = c.V0 + c.T1;
        c.A1 = MemoryAccess.ReadU32(m, c.V0);
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.V0 = MemoryAccess.ReadU16(m, (c.V0 + 0x98u));
        c.A0 = c.S5 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S6);
        c.V0 = c.V0 >> 15;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.V0);
        c.RA = 0x8001CA54u;
        GranTurismo2PC.func_8001B818(c, m);
        goto L8001CFFC;
        L8001CA5C: ;
        c.T1 = 0x801C0000u;
        c.T1 = c.T1 + 0x3080u;
        c.V0 = MemoryAccess.ReadU32(m, (c.T1 + 0x20u));
        if ((int)c.V0 >= 0) {
            c.V1 = c.V0 + 0u;
            goto L8001CA94;
        }
        c.V1 = c.V0 + 0u;
        c.V0 = 0x00140000u;
        c.V0 = c.V0 | 0x5A78u;
        c.A0 = c.S5 + 0u;
        c.A1 = 0x80020000u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A1 = c.A1 + 0x3D08u;
        goto L8001CCF8;
        L8001CA94: ;
        c.V0 = 0x20500000u;
        c.V0 = c.V0 | 0xC9F9u;
        c.A1 = c.V1 << 5;
        c.A1 = c.A1 - c.V1;
        c.A1 = c.A1 << 2;
        c.A1 = c.A1 + c.V1;
        c.A1 = c.A1 << 3;
        { var _r = (long)(int)c.A1 * (int)c.V0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.A0 = c.SP + 0x28u;
        c.A1 = (uint)((int)c.A1 >> 31);
        c.T1 = c.HI;
        c.V0 = (uint)((int)c.T1 >> 7);
        c.A1 = c.V0 - c.A1;
        c.RA = 0x8001CACCu;
        GranTurismo2PC.func_8001FFCC(c, m);
        c.V0 = 0x00140000u;
        c.V0 = c.V0 | 0x5A78u;
        c.A0 = c.S5 + 0u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x4u));
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A1 = c.SP + 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        goto L8001CD88;
        L8001CAEC: ;
        c.V1 = 0x20500000u;
        c.T1 = 0x801C0000u;
        c.T1 = c.T1 + 0x3080u;
        c.V0 = MemoryAccess.ReadU32(m, (c.T1 + 0x1Cu));
        c.V1 = c.V1 | 0xC9F9u;
        c.A1 = c.V0 << 5;
        c.A1 = c.A1 - c.V0;
        c.A1 = c.A1 << 2;
        c.A1 = c.A1 + c.V0;
        c.A1 = c.A1 << 3;
        { var _r = (long)(int)c.A1 * (int)c.V1; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.A0 = c.SP + 0x28u;
        c.A1 = (uint)((int)c.A1 >> 31);
        c.T1 = c.HI;
        c.V0 = (uint)((int)c.T1 >> 7);
        c.A1 = c.V0 - c.A1;
        c.RA = 0x8001CB30u;
        GranTurismo2PC.func_8001FFCC(c, m);
        c.V0 = 0x00140000u;
        c.V0 = c.V0 | 0x5A78u;
        c.A0 = c.S5 + 0u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x4u));
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A1 = c.SP + 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        goto L8001CD88;
        L8001CB50: ;
        c.T1 = 0x801C0000u;
        c.V0 = MemoryAccess.ReadU16(m, (c.T1 + 0x3080u));
        c.V0 = c.V0 - 0x3u;
        c.V0 = c.V0 < 0x00000002u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.T1 = c.T1 + 0x3080u;
            goto L8001CFFC;
        }
        c.T1 = c.T1 + 0x3080u;
        c.A0 = MemoryAccess.ReadU32(m, (c.T1 + 0x18u));
        c.RA = 0x8001CB78u;
        GranTurismo2PC.func_800183B8(c, m);
        c.S1 = c.V0 + 0u;
        c.S0 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x4u));
        c.A0 = c.S1 + 0u;
        c.S0 = c.S0 + c.V0;
        c.S0 = (uint)((int)c.S0 >> 1);
        c.RA = 0x8001CB94u;
        GranTurismo2PC.func_8001F6CC(c, m);
        c.V1 = 0x00640000u;
        c.V1 = c.V1 | 0x6464u;
        c.A0 = c.S5 + 0u;
        c.A1 = c.S1 + 0u;
        c.V0 = (uint)((int)c.V0 >> 1);
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A2 = c.S0 - c.V0;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V1);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S4);
        c.RA = 0x8001CBBCu;
        GranTurismo2PC.func_8001FBEC(c, m);
        goto L8001CFFC;
        L8001CBC4: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S2 + 0x8u));
        c.V0 = c.V0 >> 29;
        c.V0 = c.V0 ^ 0x0001u;
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u) {
            goto L8001CFFC;
        }
        c.A0 = MemoryAccess.ReadU32(m, (c.S2 + 0x10u));
        c.RA = 0x8001CBECu;
        GranTurismo2PC.func_800177D4(c, m);
        c.A0 = c.SP + 0x28u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001CBF8u;
        GranTurismo2PC.func_8001FCDC(c, m);
        c.V0 = 0x02800000u;
        c.V0 = c.V0 | 0x8080u;
        c.A0 = c.S5 + 0u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x4u));
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A1 = c.SP + 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S4);
        c.A2 = c.A2 - 0x50u;
        c.A3 = c.A3 + 0x14u;
        c.RA = 0x8001CC24u;
        GranTurismo2PC.func_8001FC28(c, m);
        goto L8001CFFC;
        L8001CC2C: ;
        c.T1 = 0x801C0000u;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.T1 + 0x3080u));
        c.V0 = c.V1 ^ 0x0001u;
        if (c.V0 != 0u) {
            c.V0 = c.V1 ^ 0x0005u;
            goto L8001CC5C;
        }
        c.V0 = c.V1 ^ 0x0005u;
        c.T1 = c.T1 + 0x3080u;
        c.A1 = MemoryAccess.ReadU32(m, (c.T1 + 0x30u));
        c.A0 = c.SP + 0x28u;
        c.RA = 0x8001CC54u;
        GranTurismo2PC.func_8001FCDC(c, m);
        c.A0 = c.S5 + 0u;
        goto L8001CD78;
        L8001CC5C: ;
        if (c.V0 != 0u) {
            c.T1 = 0x801C0000u;
            goto L8001CC88;
        }
        c.T1 = 0x801C0000u;
        c.T1 = c.T1 + 0x3080u;
        c.A1 = MemoryAccess.ReadU32(m, (c.T1 + 0x30u));
        if ((int)c.A1 <= 0) {
            goto L8001CFFC;
        }
        c.A0 = c.SP + 0x28u;
        c.RA = 0x8001CC80u;
        GranTurismo2PC.func_8001FCDC(c, m);
        c.A0 = c.S5 + 0u;
        goto L8001CD78;
        L8001CC88: ;
        c.T1 = c.T1 + 0x3080u;
        c.V0 = MemoryAccess.ReadU32(m, (c.T1 + 0x24u));
        if (c.V0 == 0u) {
            c.V0 = 0x00F00000u;
            goto L8001CCB8;
        }
        c.V0 = 0x00F00000u;
        c.V0 = c.V0 | 0x5028u;
        c.A0 = c.S5 + 0u;
        c.A1 = 0x801C0000u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A1 = c.A1 + 0x30D0u;
        goto L8001CCF8;
        L8001CCB8: ;
        c.T1 = 0x801C0000u;
        c.T1 = c.T1 + 0x3080u;
        c.A1 = MemoryAccess.ReadU32(m, (c.T1 + 0x30u));
        if ((int)c.A1 < 0) {
            c.V0 = 0x00280000u;
            goto L8001CCE0;
        }
        c.V0 = 0x00280000u;
        c.A0 = c.SP + 0x28u;
        c.RA = 0x8001CCD8u;
        GranTurismo2PC.func_8001FCDC(c, m);
        c.A0 = c.S5 + 0u;
        goto L8001CD78;
        L8001CCE0: ;
        c.V0 = c.V0 | 0x50F0u;
        c.A0 = c.S5 + 0u;
        c.A1 = 0x801C0000u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A1 = c.A1 + 0x30C0u;
        L8001CCF8: ;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        L8001CCFC: ;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S4);
        c.RA = 0x8001CD04u;
        GranTurismo2PC.func_8001FC64(c, m);
        goto L8001CFFC;
        L8001CD0C: ;
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x370u));
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x4u));
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0x37Cu));
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x2u));
        c.A0 = c.T1 + 0x28u;
        c.A2 = c.A2 + c.V0;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A2 = (uint)((int)c.A2 >> 1);
        c.A3 = c.A3 + c.V0;
        c.A3 = (uint)((int)c.A3 >> 1);
        c.RA = 0x8001CD3Cu;
        GranTurismo2PC.func_8001A654(c, m);
        goto L8001CFFC;
        L8001CD44: ;
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x370u));
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x4u));
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x2u));
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0x37Cu));
        c.A0 = c.T1 + 0x28u;
        c.RA = 0x8001CD5Cu;
        GranTurismo2PC.func_8001A708(c, m);
        goto L8001CFFC;
        L8001CD64: ;
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x370u));
        c.A1 = c.SP + 0x28u;
        c.A0 = c.T1 + 0x28u;
        c.RA = 0x8001CD74u;
        GranTurismo2PC.func_8001A4AC(c, m);
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0x37Cu));
        L8001CD78: ;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x4u));
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A1 = c.SP + 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S6);
        L8001CD88: ;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S4);
        c.RA = 0x8001CD90u;
        GranTurismo2PC.func_8001FC28(c, m);
        goto L8001CFFC;
        L8001CD98: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x4u));
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x370u));
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x2u));
        c.A2 = c.T1 + 0x478u;
        c.V0 = c.V0 - c.A0;
        c.V1 = c.V1 - c.A1;
        MemoryAccess.WriteU16(m, (c.A2 + 0xBCu), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.A2 + 0xBEu), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.A2 + 0xC0u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.A2 + 0xC2u), (ushort)c.V1);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x4u));
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x2u));
        c.V1 = c.V1 - c.A1;
        c.V0 = c.V0 - c.A0;
        c.V1 = c.V1 << 8;
        if (c.V0 != 0u) { if ((int)c.V1 == int.MinValue && (int)c.V0 == -1) { c.LO = 0x80000000u; c.HI = 0u; } else { c.LO = (uint)((int)c.V1 / (int)c.V0); c.HI = (uint)((int)c.V1 % (int)c.V0); } }
        c.V1 = c.LO;
        c.A0 = c.T1 + 0x28u;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0x380u));
        c.V0 = 0x000000B3u;
        MemoryAccess.WriteU16(m, (c.A2 + 0xC8u), (ushort)c.V0);
        c.V0 = 0xFFFFFFCEu;
        MemoryAccess.WriteU16(m, (c.A2 + 0xCAu), (ushort)c.V0);
        c.V0 = 0x00000320u;
        MemoryAccess.WriteU16(m, (c.A2 + 0xCCu), (ushort)c.V0);
        c.V0 = 0x00007FFFu;
        MemoryAccess.WriteU16(m, (c.A2 + 0xCEu), (ushort)c.V0);
        c.V1 = (uint)((int)c.V1 >> 1);
        c.V0 = 0u - c.V1;
        MemoryAccess.WriteU16(m, (c.A2 + 0xC4u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.A2 + 0xC6u), (ushort)c.V1);
        c.RA = 0x8001CE28u;
        GranTurismo2PC.func_8001A8A4(c, m);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        MemoryAccess.WriteU16(m, (c.SP + 0x328u), (ushort)c.V0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x4u));
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.V0 = c.V0 - c.V1;
        MemoryAccess.WriteU16(m, (c.SP + 0x32Cu), (ushort)c.V0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x2u));
        c.A1 = c.SP + 0x328u;
        MemoryAccess.WriteU16(m, (c.SP + 0x32Au), (ushort)c.V0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x2u));
        c.A0 = MemoryAccess.ReadU32(m, (c.SP + 0x380u));
        c.V0 = c.V0 - c.V1;
        MemoryAccess.WriteU16(m, (c.SP + 0x32Eu), (ushort)c.V0);
        c.RA = 0x8001CE6Cu;
        GranTurismo2PC.func_8008034C(c, m);
        goto L8001CFFC;
        L8001CE74: ;
        c.V0 = c.S3 - 0x5Eu;
        c.V1 = c.V0 << 2;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 3;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 2;
        c.T1 = 0x801D0000u;
        c.T1 = c.T1 - 0x3300u;
        goto L8001CF48;
        L8001CE98: ;
        c.V0 = c.S3 - 0x69u;
        c.V1 = c.V0 << 2;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 3;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 2;
        c.T1 = 0x801D0000u;
        c.T1 = c.T1 - 0x3968u;
        goto L8001CF48;
        L8001CEBC: ;
        c.V0 = c.S3 - 0x53u;
        c.V1 = c.V0 << 2;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 3;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 2;
        c.T1 = 0x801D0000u;
        c.T1 = c.T1 - 0x3FD0u;
        goto L8001CF48;
        L8001CEE0: ;
        c.V0 = c.S3 - 0x74u;
        c.V1 = c.V0 << 2;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 3;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 2;
        c.T1 = 0x801D0000u;
        c.T1 = c.T1 - 0x4638u;
        goto L8001CF48;
        L8001CF04: ;
        c.V0 = c.S3 - 0x7Fu;
        c.V1 = c.V0 << 2;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 3;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 2;
        c.T1 = 0x801D0000u;
        c.T1 = c.T1 - 0x4CA0u;
        goto L8001CF48;
        L8001CF28: ;
        c.V0 = c.S3 - 0x8Au;
        c.V1 = c.V0 << 2;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 3;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 2;
        c.T1 = 0x801D0000u;
        c.T1 = c.T1 - 0x5308u;
        L8001CF48: ;
        c.V1 = c.V1 + c.T1;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V1 + 0x1u));
        c.V1 = c.V0 - 0x1u;
        if ((int)c.V1 < 0) {
            c.V0 = c.V1 << 1;
            goto L8001CFFC;
        }
        c.V0 = c.V1 << 1;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 2;
        c.T1 = 0x80050000u;
        c.T1 = c.T1 + 0x978u;
        c.S1 = c.V0 + c.T1;
        L8001CF74: ;
        c.A0 = c.S5 + 0u;
        c.A1 = 0x00800000u;
        c.A1 = c.A1 | 0x8080u;
        L8001CF80: ;
        c.RA = 0x8001CF88u;
        GranTurismo2PC.func_80081478(c, m);
        c.A2 = c.V0 + 0u;
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x4u));
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x2u));
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.A1 = c.A1 + c.V1;
        c.A1 = (uint)((int)c.A1 >> 1);
        c.A0 = c.A0 + c.V0;
        c.A0 = (uint)((int)c.A0 >> 1);
        c.V1 = MemoryAccess.ReadU16(m, (c.S1 + 0x4u));
        c.V0 = MemoryAccess.ReadU16(m, (c.S1 + 0x6u));
        c.V1 = c.V1 << 16;
        c.V1 = (uint)((int)c.V1 >> 17);
        c.A1 = c.A1 - c.V1;
        c.V0 = c.V0 << 16;
        c.V0 = (uint)((int)c.V0 >> 17);
        c.A0 = c.A0 - c.V0;
        c.A0 = c.A0 << 16;
        c.A1 = c.A1 + c.A0;
        MemoryAccess.WriteU32(m, c.A2, c.A1);
        c.V0 = MemoryAccess.ReadU32(m, c.S1);
        MemoryAccess.WriteU32(m, (c.A2 + 0x4u), c.V0);
        L8001CFE4: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x4u));
        MemoryAccess.WriteU32(m, (c.A2 + 0x8u), c.V0);
        c.A1 = MemoryAccess.ReadU16(m, (c.S1 + 0x8u));
        c.A0 = c.S5 + 0u;
        c.RA = 0x8001CFFCu;
        GranTurismo2PC.func_8007DA44(c, m);
        L8001CFFC: ;
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x340u));
        c.T1 = c.T1 + 0x1u;
        MemoryAccess.WriteU32(m, (c.SP + 0x340u), c.T1);
        goto L8001B9FC;
        L8001D010: ;
        c.T1 = MemoryAccess.ReadU32(m, (c.SP + 0x370u));
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, c.T1);
        if (c.V0 == 0u) {
            c.V0 = 0x800B0000u;
            goto L8001D060;
        }
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7288u));
        c.V0 = c.V0 >> 9;
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u) {
            c.A0 = c.S5 + 0xCu;
            goto L8001D060;
        }
        c.A0 = c.S5 + 0xCu;
        c.A1 = 0u + 0u;
        c.RA = 0x8001D048u;
        GranTurismo2PC.func_8007D024(c, m);
        c.V1 = 0x00000200u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x4u), (ushort)c.V1);
        c.V1 = 0x000001F8u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x2u), (ushort)0u);
        MemoryAccess.WriteU16(m, c.V0, (ushort)0u);
        MemoryAccess.WriteU16(m, (c.V0 + 0x6u), (ushort)c.V1);
        L8001D060: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x36Cu));
        c.FP = MemoryAccess.ReadU32(m, (c.SP + 0x368u));
        c.S7 = MemoryAccess.ReadU32(m, (c.SP + 0x364u));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x360u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x35Cu));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x358u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x354u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x350u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x34Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x348u));
        c.SP = c.SP + 0x370u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001D090(CpuContext c, IMemory m)
    {
        c.V1 = 0x00520000u;
        c.V1 = c.V1 | 0x5252u;
        c.A1 = 0x00090000u;
        c.A1 = c.A1 | 0x4CCCu;
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.A0 + 0xD0u), (byte)c.V0);
        c.V0 = 0x000000A0u;
        MemoryAccess.WriteU16(m, (c.A0 + 0xA8u), (ushort)c.V0);
        c.V0 = 0x00001500u;
        MemoryAccess.WriteU16(m, (c.A0 + 0xAAu), (ushort)c.V0);
        c.V0 = 0x00000190u;
        MemoryAccess.WriteU8(m, (c.A0 + 0xD1u), (byte)0u);
        MemoryAccess.WriteU32(m, (c.A0 + 0xD4u), c.V1);
        MemoryAccess.WriteU16(m, (c.A0 + 0xACu), (ushort)0u);
        MemoryAccess.WriteU32(m, (c.A0 + 0xB0u), 0u);
        MemoryAccess.WriteU32(m, (c.A0 + 0xB4u), 0u);
        MemoryAccess.WriteU32(m, (c.A0 + 0xB8u), 0u);
        MemoryAccess.WriteU32(m, (c.A0 + 0xA0u), 0u);
        MemoryAccess.WriteU32(m, (c.A0 + 0x9Cu), 0u);
        MemoryAccess.WriteU32(m, (c.A0 + 0xA4u), c.A1);
        MemoryAccess.WriteU16(m, (c.A0 + 0xCCu), (ushort)c.V0);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001D0E8(CpuContext c, IMemory m)
    {
        c.V0 = MemoryAccess.ReadU16(m, (c.A0 + 0xAAu));
        c.V1 = 0x0000006Eu;
        MemoryAccess.WriteU16(m, (c.A0 + 0xA8u), (ushort)c.V1);
        c.V0 = c.V0 + c.A1;
        c.V0 = c.V0 & 0x3FFFu;
        MemoryAccess.WriteU16(m, (c.A0 + 0xAAu), (ushort)c.V0);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001D104(CpuContext c, IMemory m)
    {
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.A0 + 0x552u), (byte)c.V0);
        c.V0 = 0x00000040u;
        MemoryAccess.WriteU16(m, (c.A0 + 0x554u), (ushort)c.V0);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001D118(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A1 + 0u;
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.A3 = MemoryAccess.ReadU32(m, c.S0);
        c.T0 = MemoryAccess.ReadU32(m, (c.S0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.S0 + 0x8u));
        MemoryAccess.WriteU32(m, (c.S1 + 0x1Cu), c.A3);
        MemoryAccess.WriteU32(m, (c.S1 + 0x20u), c.T0);
        MemoryAccess.WriteU32(m, (c.S1 + 0x24u), c.T1);
        MemoryAccess.WriteU32(m, (c.S1 + 0x8u), c.V0);
        MemoryAccess.WriteU32(m, (c.S1 + 0xCu), c.V0);
        c.V0 = 0x801F0000u;
        c.V0 = c.V0 - 0xA10u;
        MemoryAccess.WriteU8(m, c.S1, (byte)0u);
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        c.A0 = c.S1 + 0x28u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x4u), c.V1);
        c.V0 = MemoryAccess.ReadU16(m, (c.V0 + 0xCu));
        c.A1 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x10u), 0u);
        MemoryAccess.WriteU32(m, (c.S1 + 0x14u), 0u);
        MemoryAccess.WriteU8(m, (c.S1 + 0x552u), (byte)0u);
        MemoryAccess.WriteU16(m, (c.S1 + 0x554u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.S1 + 0x550u), (ushort)c.V0);
        c.RA = 0x8001D18Cu;
        GranTurismo2PC.func_8001AAF4(c, m);
        c.A0 = c.S1 + 0x478u;
        c.RA = 0x8001D194u;
        GranTurismo2PC.func_8001D090(c, m);
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x4u));
        c.A1 = 0x000058C0u;
        c.RA = 0x8001D1A0u;
        Dispatcher.Call(c, m, 0x80020D90u);
        c.A0 = MemoryAccess.ReadU32(m, c.S0);
        c.RA = 0x8001D1ACu;
        GranTurismo2PC.func_80020ECC(c, m);
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x8u));
        c.A1 = MemoryAccess.ReadU32(m, c.S0);
        c.RA = 0x8001D1BCu;
        GranTurismo2PC.func_80020D58(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x4u));
        c.A2 = 0u + 0u;
        c.RA = 0x8001D1CCu;
        GranTurismo2PC.func_8001D2CC(c, m);
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x550u));
        if ((int)c.A0 < 0) {
            c.V0 = 0x800B0000u;
            goto L8001D1E8;
        }
        c.V0 = 0x800B0000u;
        c.RA = 0x8001D1E4u;
        GranTurismo2PC.func_80018FA4(c, m);
        c.V0 = 0x800B0000u;
        L8001D1E8: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7290u));
        MemoryAccess.WriteU32(m, (c.S1 + 0x8u), c.V0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001D208(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        c.A0 = c.A1 + 0x4u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.A1 = MemoryAccess.ReadU32(m, (c.S0 + 0x24u));
        c.A2 = MemoryAccess.ReadU32(m, (c.S0 + 0x18u));
        c.RA = 0x8001D22Cu;
        GranTurismo2PC.func_800215B0(c, m);
        c.A0 = c.S0 + 0x28u;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.A0 + 0x164u));
        if (c.V0 == 0u) {
            goto L8001D248;
        }
        c.RA = 0x8001D248u;
        Dispatcher.Call(c, m, 0x8001A624u);
        L8001D248: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001D258(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.S0 + 0x478u;
        c.A0 = c.S1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.A1 = 0x0000000Cu;
        c.RA = 0x8001D27Cu;
        GranTurismo2PC.func_8001D0E8(c, m);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S0 + 0x552u));
        if (c.V0 == 0u) {
            goto L8001D2B8;
        }
        c.V0 = MemoryAccess.ReadU16(m, (c.S0 + 0x554u));
        c.V0 = c.V0 - 0x1u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x554u), (ushort)c.V0);
        c.V0 = c.V0 << 16;
        if ((int)c.V0 > 0) {
            c.A0 = c.S1 + 0u;
            goto L8001D2B0;
        }
        c.A0 = c.S1 + 0u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x552u), (byte)0u);
        MemoryAccess.WriteU32(m, (c.S0 + 0x3Cu), 0u);
        L8001D2B0: ;
        c.A1 = 0x00000071u;
        c.RA = 0x8001D2B8u;
        GranTurismo2PC.func_8001D0E8(c, m);
        L8001D2B8: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001D2CC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x38u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x34u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.A2);
        c.A1 = MemoryAccess.ReadU32(m, (c.S3 + 0x1Cu));
        c.S1 = MemoryAccess.ReadU32(m, (c.S3 + 0x4u));
        c.S4 = MemoryAccess.ReadU32(m, (c.S0 - 0x7288u));
        c.A0 = c.S2 + 0u;
        MemoryAccess.WriteU32(m, (c.S3 + 0x4u), c.S2);
        c.RA = 0x8001D320u;
        GranTurismo2PC.func_800211FC(c, m);
        c.A0 = MemoryAccess.ReadU32(m, (c.S3 + 0x1Cu));
        c.A1 = MemoryAccess.ReadU32(m, (c.S3 + 0x24u));
        c.RA = 0x8001D330u;
        GranTurismo2PC.func_80022278(c, m);
        c.V1 = 0x00000001u;
        MemoryAccess.WriteU8(m, c.S3, (byte)c.V1);
        c.V1 = MemoryAccess.ReadU32(m, (c.S0 - 0x7288u));
        c.V1 = c.V1 >> 8;
        c.V1 = c.V1 & 0x0001u;
        if (c.V1 == 0u) {
            MemoryAccess.WriteU32(m, (c.S3 + 0x18u), c.V0);
            goto L8001D36C;
        }
        MemoryAccess.WriteU32(m, (c.S3 + 0x18u), c.V0);
        c.V0 = c.S4 >> 8;
        c.V0 = c.V0 & 0x0001u;
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 == 0u) {
            c.S2 = 0u + 0u;
            goto L8001D3A8;
        }
        c.S2 = 0u + 0u;
        goto L8001D380;
        L8001D36C: ;
        if ((int)c.S2 >= 0) {
            c.V0 = 0x80000000u;
            goto L8001D390;
        }
        c.V0 = 0x80000000u;
        c.V0 = c.S1 & c.V0;
        if (c.V0 != 0u) {
            c.S2 = 0u + 0u;
            goto L8001D3A8;
        }
        c.S2 = 0u + 0u;
        L8001D380: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S3 + 0x8u));
        MemoryAccess.WriteU32(m, (c.S3 + 0x8u), c.S1);
        MemoryAccess.WriteU32(m, (c.S3 + 0xCu), c.V0);
        goto L8001D3A4;
        L8001D390: ;
        c.V0 = 0x800B0000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.S3 + 0x8u));
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7290u));
        MemoryAccess.WriteU32(m, (c.S3 + 0xCu), c.V1);
        MemoryAccess.WriteU32(m, (c.S3 + 0x8u), c.V0);
        L8001D3A4: ;
        c.S2 = 0u + 0u;
        L8001D3A8: ;
        c.FP = 0x800B0000u;
        c.S7 = 0x801C0000u;
        c.S6 = c.S7 + 0x3080u;
        c.S5 = 0x800B0000u;
        L8001D3B8: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.FP - 0x728Cu));
        c.V0 = (int)c.S2 < (int)c.V0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x800B0000u;
            goto L8001D488;
        }
        c.V0 = 0x800B0000u;
        c.A0 = c.S2 + 0u;
        c.RA = 0x8001D3D4u;
        GranTurismo2PC.func_800215C8(c, m);
        c.S1 = c.V0 + 0u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x8u));
        c.S0 = c.V0 & 0xFFFFu;
        c.V0 = c.V0 >> 28;
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 == 0u) {
            c.S4 = c.S1 + 0u;
            goto L8001D404;
        }
        c.S4 = c.S1 + 0u;
        c.A0 = c.S3 + 0x28u;
        c.RA = 0x8001D3FCu;
        GranTurismo2PC.func_8001AB6C(c, m);
        c.A0 = c.S7 + 0x3080u;
        c.RA = 0x8001D404u;
        GranTurismo2PC.func_8001DAF0(c, m);
        L8001D404: ;
        c.V1 = c.S0 - 0x14u;
        c.V0 = c.V1 < 0x00000033u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A0 = c.S6 + 0u;
            goto L8001D42C;
        }
        c.A0 = c.S6 + 0u;
        c.S0 = c.V1 + 0u;
        c.A1 = c.S0 + 0u;
        c.RA = 0x8001D420u;
        Dispatcher.Call(c, m, 0x8001DB90u);
        c.A0 = c.S6 + 0u;
        c.A1 = c.S0 + 0u;
        c.RA = 0x8001D42Cu;
        GranTurismo2PC.func_8001DB24(c, m);
        L8001D42C: ;
        c.V0 = 0x00000009u;
        if (c.S0 != c.V0) {
            c.V0 = 0x00000050u;
            goto L8001D44C;
        }
        c.V0 = 0x00000050u;
        c.A0 = 0x800B0000u;
        c.A0 = c.A0 - 0x72A0u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x8001D448u;
        GranTurismo2PC.func_800204EC(c, m);
        c.V0 = 0x00000050u;
        L8001D44C: ;
        if (c.S0 != c.V0) {
            goto L8001D480;
        }
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0x40u));
        if (c.A3 != 0u) {
            c.A0 = c.S5 - 0x7298u;
            goto L8001D470;
        }
        c.A0 = c.S5 - 0x7298u;
        c.A1 = 0u + 0u;
        c.RA = 0x8001D46Cu;
        GranTurismo2PC.func_800209B0(c, m);
        c.A0 = c.S5 - 0x7298u;
        L8001D470: ;
        c.V0 = 0x800B0000u;
        c.A2 = MemoryAccess.ReadU8(m, (c.V0 - 0x7288u));
        c.A1 = c.S4 + 0u;
        c.RA = 0x8001D480u;
        GranTurismo2PC.func_80020A0C(c, m);
        L8001D480: ;
        c.S2 = c.S2 + 0x1u;
        goto L8001D3B8;
        L8001D488: ;
        c.S0 = MemoryAccess.ReadU8(m, (c.V0 - 0x7286u));
        c.V0 = c.S0 ^ 0x00FFu;
        if (c.V0 == 0u) {
            goto L8001D4B8;
        }
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S3 + 0x550u));
        if (c.S0 == c.V0) {
            goto L8001D4B8;
        }
        c.A0 = c.S0 + 0u;
        c.RA = 0x8001D4B4u;
        GranTurismo2PC.func_80018FA4(c, m);
        MemoryAccess.WriteU16(m, (c.S3 + 0x550u), (ushort)c.S0);
        L8001D4B8: ;
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
    public static void func_8001D4E8_gt2_overlay_4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = 0x00000009u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        L8001D508: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S2 - 0x728Cu));
        c.V0 = (int)c.S0 < (int)c.V0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0u + 0u;
            goto L8001D53C;
        }
        c.V0 = 0u + 0u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x8001D524u;
        GranTurismo2PC.func_800215C8(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 + 0x8u));
        if (c.V0 != c.S1) {
            c.S0 = c.S0 + 0x1u;
            goto L8001D508;
        }
        c.S0 = c.S0 + 0x1u;
        c.V0 = c.V1 + 0u;
        L8001D53C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001D554(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = 0x00000050u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        L8001D574: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S2 - 0x728Cu));
        c.V0 = (int)c.S0 < (int)c.V0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0u + 0u;
            goto L8001D5A8;
        }
        c.V0 = 0u + 0u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x8001D590u;
        GranTurismo2PC.func_800215C8(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 + 0x8u));
        if (c.V0 != c.S1) {
            c.S0 = c.S0 + 0x1u;
            goto L8001D574;
        }
        c.S0 = c.S0 + 0x1u;
        c.V0 = c.V1 + 0u;
        L8001D5A8: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001D5C0(CpuContext c, IMemory m)
    {
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001D5C8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x40u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.S4);
        c.S4 = c.A1 + 0u;
        c.A0 = c.S0 + 0x478u;
        MemoryAccess.WriteU32(m, (c.SP + 0x34u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x3Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x38u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S1);
        MemoryAccess.WriteU32(m, (c.S0 + 0x10u), c.S4);
        MemoryAccess.WriteU32(m, (c.S0 + 0x14u), c.A2);
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x50u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x54u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x58u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x5Cu));
        c.S5 = c.A3 + 0u;
        c.RA = 0x8001D618u;
        GranTurismo2PC.func_8001D090(c, m);
        c.A0 = c.S0 + 0x28u;
        c.A1 = c.S4 + 0u;
        c.A2 = c.S5 + 0u;
        c.A3 = c.S6 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S3);
        c.RA = 0x8001D638u;
        GranTurismo2PC.func_8001AC20(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x3Cu));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0x38u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x34u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x30u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x2Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.SP = c.SP + 0x40u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001D660(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = c.A0 + 0x28u;
        c.RA = 0x8001D670u;
        GranTurismo2PC.func_8001AB64(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001D680(CpuContext c, IMemory m)
    {
        c.V0 = MemoryAccess.ReadU32(m, (c.A0 + 0xCu));
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001D68C(CpuContext c, IMemory m)
    {
        c.V0 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001D698(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, c.A0);
        if (c.V0 == 0u) {
            c.V0 = 0xFFFFFFFFu;
            goto L8001D6BC;
        }
        c.V0 = 0xFFFFFFFFu;
        c.A0 = MemoryAccess.ReadU32(m, (c.A0 + 0x14u));
        c.RA = 0x8001D6BCu;
        GranTurismo2PC.func_80020F54(c, m);
        L8001D6BC: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001D6CC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, c.A0);
        if (c.V0 != 0u) {
            c.S0 = 0u + 0u;
            goto L8001D6FC;
        }
        c.S0 = 0u + 0u;
        c.V0 = 0u + 0u;
        goto L8001D740;
        L8001D6F4: ;
        c.V0 = c.V1 + 0u;
        goto L8001D740;
        L8001D6FC: ;
        c.S1 = 0x800B0000u;
        L8001D700: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 - 0x728Cu));
        c.V0 = (int)c.S0 < (int)c.V0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0u + 0u;
            goto L8001D740;
        }
        c.V0 = 0u + 0u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x8001D71Cu;
        GranTurismo2PC.func_800215C8(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.V0 = c.V0 >> 25;
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 != 0u) {
            c.S0 = c.S0 + 0x1u;
            goto L8001D6F4;
        }
        c.S0 = c.S0 + 0x1u;
        goto L8001D700;
        L8001D740: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001D754_gt2_overlay_4(CpuContext c, IMemory m)
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
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.A1);
        MemoryAccess.WriteU32(m, (c.SP + 0x48u), c.A2);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, c.A0);
        c.FP = MemoryAccess.ReadU32(m, (c.SP + 0x50u));
        if (c.V0 != 0u) {
            c.S4 = 0u + 0u;
            goto L8001D7A0;
        }
        c.S4 = 0u + 0u;
        c.V0 = 0u + 0u;
        goto L8001D924;
        L8001D7A0: ;
        c.S5 = c.S4 + 0u;
        c.S3 = c.S4 + 0u;
        c.V1 = c.A3 << 2;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 + 0x1274u;
        c.V1 = c.V1 + c.V0;
        c.S7 = (uint)(short)MemoryAccess.ReadU16(m, c.V1);
        c.S6 = (uint)(short)MemoryAccess.ReadU16(m, (c.V1 + 0x2u));
        L8001D7C0: ;
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x728Cu));
        c.V0 = (int)c.S3 < (int)c.V0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S5 + 0u;
            goto L8001D924;
        }
        c.V0 = c.S5 + 0u;
        c.A0 = c.S3 + 0u;
        c.RA = 0x8001D7E0u;
        GranTurismo2PC.func_800215C8(c, m);
        c.A0 = c.V0 + 0u;
        if (c.A0 == c.FP) {
            c.S2 = c.A0 + 0u;
            goto L8001D91C;
        }
        c.S2 = c.A0 + 0u;
        c.V0 = MemoryAccess.ReadU8(m, (c.A0 + 0xBu));
        c.V0 = c.V0 & 0x0001u;
        if (c.V0 != 0u) {
            c.V1 = 0u + 0u;
            goto L8001D80C;
        }
        c.V1 = 0u + 0u;
        c.RA = 0x8001D808u;
        GranTurismo2PC.func_8001B6F0(c, m);
        c.V1 = c.V0 < 0x00000001u ? 1u : 0u;
        L8001D80C: ;
        if (c.V1 != 0u) {
            goto L8001D91C;
        }
        c.A0 = c.S2 + 0u;
        c.RA = 0x8001D81Cu;
        Dispatcher.Call(c, m, 0x8001B680u);
        c.V0 = c.V0 ^ 0x0001u;
        if (c.V0 != 0u) {
            c.A1 = 0u + 0u;
            goto L8001D91C;
        }
        c.A1 = 0u + 0u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x4u));
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x44u));
        c.V0 = c.V0 + c.V1;
        c.V0 = (uint)((int)c.V0 >> 1);
        c.S1 = c.V0 - c.T0;
        { var _r = (long)(int)c.S1 * (int)c.S1; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x2u));
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x6u));
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x48u));
        c.V0 = c.V0 + c.V1;
        c.A0 = c.LO;
        c.V0 = (uint)((int)c.V0 >> 1);
        c.S0 = c.V0 - c.T0;
        { var _r = (long)(int)c.S0 * (int)c.S0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V1 = c.LO;
        c.A0 = c.A0 + c.V1;
        c.RA = 0x8001D870u;
        GranTurismo2PC.func_80081288(c, m);
        c.A1 = c.V0 + 0u;
        if (c.A1 == 0u) {
            c.V1 = c.S1 << 12;
            goto L8001D91C;
        }
        c.V1 = c.S1 << 12;
        if (c.A1 != 0u) { if ((int)c.V1 == int.MinValue && (int)c.A1 == -1) { c.LO = 0x80000000u; c.HI = 0u; } else { c.LO = (uint)((int)c.V1 / (int)c.A1); c.HI = (uint)((int)c.V1 % (int)c.A1); } }
        c.V1 = c.LO;
        c.V0 = c.S0 << 12;
        if (c.A1 != 0u) { if ((int)c.V0 == int.MinValue && (int)c.A1 == -1) { c.LO = 0x80000000u; c.HI = 0u; } else { c.LO = (uint)((int)c.V0 / (int)c.A1); c.HI = (uint)((int)c.V0 % (int)c.A1); } }
        c.V0 = c.LO;
        { var _r = (long)(int)c.S7 * (int)c.V1; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V1 = c.LO;
        { var _r = (long)(int)c.S6 * (int)c.V0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V1 = (uint)((int)c.V1 >> 12);
        c.V0 = c.LO;
        c.V0 = (uint)((int)c.V0 >> 12);
        c.V1 = c.V1 + c.V0;
        c.V0 = 0x01000000u;
        if (c.A1 != 0u) { if ((int)c.V0 == int.MinValue && (int)c.A1 == -1) { c.LO = 0x80000000u; c.HI = 0u; } else { c.LO = (uint)((int)c.V0 / (int)c.A1); c.HI = (uint)((int)c.V0 % (int)c.A1); } }
        c.A0 = c.LO;
        if ((int)c.V1 < 0) {
            { var _r = (long)(int)c.V1 * (int)c.V1; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
            goto L8001D91C;
        }
        { var _r = (long)(int)c.V1 * (int)c.V1; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V1 = c.LO;
        c.V1 = (uint)((int)c.V1 >> 12);
        { var _r = (long)(int)c.A0 * (int)c.V1; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.T1 = c.HI;
        c.T0 = c.LO;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.T0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.T1);
        c.A0 = c.T0 >> 12;
        c.V0 = c.T1 << 20;
        c.A0 = c.A0 | c.V0;
        c.A1 = (uint)((int)c.T1 >> 12);
        c.V0 = (int)c.S4 < (int)c.A0 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L8001D91C;
        }
        c.S4 = c.A0 + 0u;
        c.S5 = c.S2 + 0u;
        L8001D91C: ;
        c.S3 = c.S3 + 0x1u;
        goto L8001D7C0;
        L8001D924: ;
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
    public static void func_8001D954(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, c.A0);
        if (c.V0 != 0u) {
            c.S3 = c.A2 + 0u;
            goto L8001D98C;
        }
        c.S3 = c.A2 + 0u;
        c.V0 = 0u + 0u;
        goto L8001DA04;
        L8001D98C: ;
        c.S1 = 0u + 0u;
        c.S4 = 0x800B0000u;
        L8001D994: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S4 - 0x728Cu));
        c.V0 = (int)c.S1 < (int)c.V0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0u + 0u;
            goto L8001DA04;
        }
        c.V0 = 0u + 0u;
        c.A0 = c.S1 + 0u;
        c.RA = 0x8001D9B0u;
        GranTurismo2PC.func_800215C8(c, m);
        c.S0 = c.V0 + 0u;
        c.A0 = c.S0 + 0u;
        c.RA = 0x8001D9BCu;
        GranTurismo2PC.func_8001B6F0(c, m);
        if (c.V0 == 0u) {
            goto L8001D9FC;
        }
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, c.S0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x4u));
        c.V1 = (int)c.V1 < (int)c.S2 ? 1u : 0u;
        c.V0 = (int)c.S2 < (int)c.V0 ? 1u : 0u;
        c.V1 = c.V1 & c.V0;
        if (c.V1 == 0u) {
            goto L8001D9FC;
        }
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x2u));
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x6u));
        c.V1 = (int)c.V1 < (int)c.S3 ? 1u : 0u;
        c.V0 = (int)c.S3 < (int)c.V0 ? 1u : 0u;
        c.V1 = c.V1 & c.V0;
        if (c.V1 != 0u) {
            c.V0 = c.S0 + 0u;
            goto L8001DA04;
        }
        c.V0 = c.S0 + 0u;
        L8001D9FC: ;
        c.S1 = c.S1 + 0x1u;
        goto L8001D994;
        L8001DA04: ;
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
    public static void func_8001DA24(CpuContext c, IMemory m)
    {
        MemoryAccess.WriteU16(m, c.A0, (ushort)0u);
        MemoryAccess.WriteU32(m, (c.A0 + 0x30u), 0u);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001DA30(CpuContext c, IMemory m)
    {
        MemoryAccess.WriteU16(m, c.A0, (ushort)0u);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001DA38(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        c.V0 = 0x00000006u;
        c.A0 = 0x801D0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        MemoryAccess.WriteU32(m, (c.S0 + 0x4u), c.A1);
        c.V1 = c.A1 + 0u;
        c.A0 = c.A0 - 0x6720u;
        MemoryAccess.WriteU16(m, c.S0, (ushort)c.V0);
        c.V0 = c.V1 << 2;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 2;
        c.A0 = c.A0 + c.V0;
        c.A0 = MemoryAccess.ReadU32(m, (c.A0 + 0x3C78u));
        c.RA = 0x8001DA84u;
        GranTurismo2PC.func_80017A68(c, m);
        MemoryAccess.WriteU32(m, (c.S0 + 0x30u), c.V0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001DA98(CpuContext c, IMemory m)
    {
        c.V0 = 0x00000007u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x8u), c.A1);
        MemoryAccess.WriteU16(m, c.A0, (ushort)c.V0);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001DAA8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        c.A0 = c.A1 + 0u;
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        MemoryAccess.WriteU16(m, c.S0, (ushort)c.V0);
        if (c.A2 != 0u) {
            MemoryAccess.WriteU32(m, (c.S0 + 0xCu), c.A0);
            goto L8001DADC;
        }
        MemoryAccess.WriteU32(m, (c.S0 + 0xCu), c.A0);
        c.RA = 0x8001DAD4u;
        GranTurismo2PC.func_800177D4(c, m);
        MemoryAccess.WriteU32(m, (c.S0 + 0x30u), c.V0);
        goto L8001DAE0;
        L8001DADC: ;
        MemoryAccess.WriteU32(m, (c.S0 + 0x30u), c.A2);
        L8001DAE0: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001DAF0(CpuContext c, IMemory m)
    {
        c.V0 = 0x801D0000u;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x156Cu));
        c.V0 = 0x00000005u;
        MemoryAccess.WriteU16(m, c.A0, (ushort)c.V0);
        MemoryAccess.WriteU32(m, (c.A0 + 0x30u), 0u);
        MemoryAccess.WriteU32(m, (c.A0 + 0x28u), c.V1);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001DB0C(CpuContext c, IMemory m)
    {
        c.V0 = 0x00000005u;
        MemoryAccess.WriteU16(m, c.A0, (ushort)c.V0);
        c.V0 = 0x000007D0u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x2Cu), c.A1);
        MemoryAccess.WriteU32(m, (c.A0 + 0x30u), c.V0);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001DB24(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        c.A0 = 0x801D0000u;
        c.A0 = c.A0 - 0x6720u;
        c.A0 = c.A0 + 0x3C74u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0x4018u));
        MemoryAccess.WriteU32(m, (c.S0 + 0x18u), c.A1);
        MemoryAccess.WriteU32(m, (c.S0 + 0x14u), c.V0);
        c.V1 = c.V0 + 0u;
        c.V0 = 0x00000003u;
        MemoryAccess.WriteU16(m, c.S0, (ushort)c.V0);
        c.V0 = c.V1 << 2;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.A0;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.RA = 0x8001DB7Cu;
        GranTurismo2PC.func_80017B04(c, m);
        MemoryAccess.WriteU32(m, (c.S0 + 0x30u), c.V0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001DB90_gt2_overlay_4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x3A8u;
        MemoryAccess.WriteU32(m, (c.SP + 0x39Cu), c.S3);
        c.S3 = c.A0 + 0u;
        c.V0 = 0x801D0000u;
        c.V0 = c.V0 - 0x6720u;
        c.V0 = c.V0 + 0x3C74u;
        MemoryAccess.WriteU32(m, (c.SP + 0x3A4u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x3A0u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x398u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x394u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x390u), c.S0);
        c.S1 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x4018u));
        c.S0 = c.S1 << 2;
        c.S0 = c.S0 + c.S1;
        c.S0 = c.S0 << 3;
        c.S0 = c.S0 + c.S1;
        c.S0 = c.S0 << 2;
        c.S0 = c.S0 + 0x4u;
        c.S0 = c.S0 + c.V0;
        c.A0 = MemoryAccess.ReadU32(m, c.S0);
        c.S4 = c.A1 + 0u;
        c.RA = 0x8001DBECu;
        GranTurismo2PC.func_80017B04(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S4 + 0u;
        MemoryAccess.WriteU32(m, (c.S3 + 0x30u), c.V0);
        c.RA = 0x8001DBFCu;
        GranTurismo2PC.func_8005E874(c, m);
        MemoryAccess.WriteU32(m, (c.S3 + 0x24u), c.V0);
        c.RA = 0x8001DC04u;
        GranTurismo2PC.func_800174A0(c, m);
        c.V1 = c.SP + 0x240u;
        c.A0 = c.V0 + 0x80u;
        L8001DC0C: ;
        c.T0 = MemoryAccess.ReadU32(m, c.V0);
        c.T1 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        c.T3 = MemoryAccess.ReadU32(m, (c.V0 + 0xCu));
        MemoryAccess.WriteU32(m, c.V1, c.T0);
        MemoryAccess.WriteU32(m, (c.V1 + 0x4u), c.T1);
        MemoryAccess.WriteU32(m, (c.V1 + 0x8u), c.T2);
        MemoryAccess.WriteU32(m, (c.V1 + 0xCu), c.T3);
        c.V0 = c.V0 + 0x10u;
        if (c.V0 != c.A0) {
            c.V1 = c.V1 + 0x10u;
            goto L8001DC0C;
        }
        c.V1 = c.V1 + 0x10u;
        c.A0 = c.SP + 0x240u;
        c.A1 = c.SP + 0x10u;
        c.T0 = MemoryAccess.ReadU32(m, c.V0);
        MemoryAccess.WriteU32(m, c.V1, c.T0);
        c.RA = 0x8001DC4Cu;
        GranTurismo2PC.func_800771AC(c, m);
        c.A0 = c.SP + 0x10u;
        c.S2 = c.SP + 0x1D0u;
        c.A1 = c.S2 + 0u;
        c.RA = 0x8001DC5Cu;
        GranTurismo2PC.func_80075930(c, m);
        c.V1 = MemoryAccess.ReadU16(m, (c.SP + 0x1D0u));
        c.A0 = MemoryAccess.ReadU32(m, (c.S3 + 0x30u));
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU32(m, (c.S3 + 0x20u), c.V0);
        if ((int)c.A0 < 0) {
            MemoryAccess.WriteU32(m, (c.S3 + 0x1Cu), c.V1);
            goto L8001DCB0;
        }
        MemoryAccess.WriteU32(m, (c.S3 + 0x1Cu), c.V1);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S4 + 0u;
        c.S0 = c.SP + 0x2C8u;
        c.A2 = 0u + 0u;
        c.A3 = c.S0 + 0u;
        c.RA = 0x8001DC8Cu;
        GranTurismo2PC.func_80017F18(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.SP + 0x10u;
        c.RA = 0x8001DC98u;
        GranTurismo2PC.func_80077214(c, m);
        c.A0 = c.SP + 0x10u;
        c.A1 = c.S2 + 0u;
        c.RA = 0x8001DCA4u;
        GranTurismo2PC.func_80075930(c, m);
        c.V0 = MemoryAccess.ReadU16(m, (c.SP + 0x1D0u));
        MemoryAccess.WriteU32(m, (c.S3 + 0x20u), c.V0);
        L8001DCB0: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x3A4u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x3A0u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x39Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x398u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x394u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x390u));
        c.SP = c.SP + 0x3A8u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001DCD0(CpuContext c, IMemory m)
    {
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, c.A0);
        c.V0 = c.V1 < 0x00000008u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A1 = 0u + 0u;
            goto L8001DD34;
        }
        c.A1 = 0u + 0u;
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x405Cu;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x8001DD34u: goto L8001DD34;
            case 0x8001DD1Cu: goto L8001DD1C;
            case 0x8001DD24u: goto L8001DD24;
            case 0x8001DD2Cu: goto L8001DD2C;
            case 0x8001DD04u: goto L8001DD04;
            case 0x8001DD14u: goto L8001DD14;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L8001DD04: ;
        c.V0 = 0x00000006u;
        MemoryAccess.WriteU16(m, c.A0, (ushort)c.V0);
        c.A1 = 0x00000001u;
        goto L8001DD34;
        L8001DD14: ;
        c.V0 = 0x00000007u;
        goto L8001DD30;
        L8001DD1C: ;
        c.V0 = 0x00000001u;
        goto L8001DD30;
        L8001DD24: ;
        c.V0 = 0x00000003u;
        goto L8001DD30;
        L8001DD2C: ;
        c.V0 = 0x00000005u;
        L8001DD30: ;
        MemoryAccess.WriteU16(m, c.A0, (ushort)c.V0);
        L8001DD34: ;
        c.V0 = c.A1 + 0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001DD3C(CpuContext c, IMemory m)
    {
        c.V0 = MemoryAccess.ReadU16(m, c.A0);
        c.V0 = c.V0 - 0x1u;
        c.V0 = c.V0 << 16;
        c.V1 = (uint)((int)c.V0 >> 16);
        c.V0 = c.V1 < 0x00000006u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80020000u;
            goto L8001DDA4;
        }
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x407Cu;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x8001DD84u: goto L8001DD84;
            case 0x8001DD90u: goto L8001DD90;
            case 0x8001DD9Cu: goto L8001DD9C;
            case 0x8001DD78u: goto L8001DD78;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L8001DD78: ;
        c.V0 = 0x00000006u;
        MemoryAccess.WriteU16(m, c.A0, (ushort)c.V0);
        return;
        L8001DD84: ;
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU16(m, c.A0, (ushort)c.V0);
        return;
        L8001DD90: ;
        c.V0 = 0x00000003u;
        MemoryAccess.WriteU16(m, c.A0, (ushort)c.V0);
        return;
        L8001DD9C: ;
        c.V0 = 0x00000005u;
        MemoryAccess.WriteU16(m, c.A0, (ushort)c.V0);
        L8001DDA4: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001DDAC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.V0 = 0x801D0000u;
        c.V0 = c.V0 - 0x6720u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.V1 = MemoryAccess.ReadU16(m, c.S1);
        c.S3 = c.V0 + 0x3C74u;
        c.V1 = c.V1 - 0x1u;
        c.V1 = c.V1 << 16;
        c.V1 = (uint)((int)c.V1 >> 16);
        c.V0 = c.V1 < 0x00000006u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.S0 = c.S2 + 0u;
            goto L8001DFC0;
        }
        c.S0 = c.S2 + 0u;
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x4094u;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x8001DE34u: goto L8001DE34;
            case 0x8001DE7Cu: goto L8001DE7C;
            case 0x8001DEA4u: goto L8001DEA4;
            case 0x8001DF48u: goto L8001DF48;
            case 0x8001DFA8u: goto L8001DFA8;
            case 0x8001DE14u: goto L8001DE14;
            case 0x8001DEE8u: goto L8001DEE8;
            case 0x8001DF38u: goto L8001DF38;
            case 0x8001DF10u: goto L8001DF10;
            case 0x8001DF94u: goto L8001DF94;
            case 0x8001DFA0u: goto L8001DFA0;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L8001DE14: ;
        c.A0 = MemoryAccess.ReadU32(m, (c.S1 + 0x4u));
        c.A1 = 0u + 0u;
        c.S2 = 0x00000006u;
        c.S0 = 0x80000000u;
        c.S0 = c.S0 | 0x001Cu;
        c.RA = 0x8001DE2Cu;
        GranTurismo2PC.func_80017A70(c, m);
        goto L8001DFC0;
        L8001DE34: ;
        c.A0 = MemoryAccess.ReadU32(m, (c.S1 + 0xCu));
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x10u));
        c.A2 = MemoryAccess.ReadU32(m, (c.S1 + 0x30u));
        c.A3 = 0u + 0u;
        c.RA = 0x8001DE48u;
        GranTurismo2PC.func_8001796C(c, m);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S3 + 0x4018u));
        if ((int)c.V0 < 0) {
            c.S2 = 0x00000002u;
            goto L8001DE64;
        }
        c.S2 = 0x00000002u;
        c.S0 = 0x80000000u;
        c.S0 = c.S0 | 0x0008u;
        goto L8001DFC0;
        L8001DE64: ;
        c.A1 = 0u + 0u;
        c.S2 = 0x00000001u;
        c.S0 = 0x80000000u;
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, c.S3);
        c.S0 = c.S0 | 0x0004u;
        goto L8001DE90;
        L8001DE7C: ;
        c.A1 = 0u + 0u;
        c.S2 = 0x00000001u;
        c.S0 = 0x80000000u;
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, c.S3);
        c.S0 = c.S0 | 0x0005u;
        L8001DE90: ;
        c.A0 = c.A0 - 0x1u;
        MemoryAccess.WriteU16(m, (c.S3 + 0x4018u), (ushort)c.A0);
        c.RA = 0x8001DE9Cu;
        GranTurismo2PC.func_80017480(c, m);
        goto L8001DFC0;
        L8001DEA4: ;
        c.A0 = MemoryAccess.ReadU32(m, (c.S1 + 0x14u));
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x18u));
        c.A2 = 0u + 0u;
        c.RA = 0x8001DEB4u;
        GranTurismo2PC.func_80017C98(c, m);
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x18u));
        c.V1 = c.V0 - 0x7u;
        c.V0 = c.V1 < 0x0000001Cu ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80020000u;
            goto L8001DF38;
        }
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x40ACu;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x8001DEE8u: goto L8001DEE8;
            case 0x8001DF38u: goto L8001DF38;
            case 0x8001DF10u: goto L8001DF10;
            case 0x8001DF94u: goto L8001DF94;
            case 0x8001DFA0u: goto L8001DFA0;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L8001DEE8: ;
        c.A2 = 0u + 0u;
        c.S2 = 0x00000003u;
        c.S0 = 0x80000000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.S1 + 0x14u));
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x18u));
        c.A3 = MemoryAccess.ReadU32(m, (c.S1 + 0x10u));
        c.S0 = c.S0 | 0x0006u;
        c.RA = 0x8001DF08u;
        GranTurismo2PC.func_80017D6C(c, m);
        goto L8001DFC0;
        L8001DF10: ;
        c.A2 = 0u + 0u;
        c.S2 = 0x00000003u;
        c.S0 = 0x80000000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.S1 + 0x14u));
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x18u));
        c.A3 = MemoryAccess.ReadU32(m, (c.S1 + 0x10u));
        c.S0 = c.S0 | 0x001Fu;
        c.RA = 0x8001DF30u;
        GranTurismo2PC.func_80017D6C(c, m);
        goto L8001DFC0;
        L8001DF38: ;
        c.S2 = 0x00000004u;
        c.S0 = 0x80000000u;
        c.S0 = c.S0 | 0x0007u;
        goto L8001DFC0;
        L8001DF48: ;
        c.A0 = MemoryAccess.ReadU32(m, (c.S1 + 0x14u));
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x18u));
        c.A3 = MemoryAccess.ReadU32(m, (c.S1 + 0x10u));
        c.A2 = 0u + 0u;
        c.RA = 0x8001DF5Cu;
        GranTurismo2PC.func_80017D6C(c, m);
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x18u));
        c.V1 = c.V0 - 0x6u;
        c.V0 = c.V1 < 0x0000002Cu ? 1u : 0u;
        if (c.V0 == 0u) {
            c.S2 = 0x00000003u;
            goto L8001DFA0;
        }
        c.S2 = 0x00000003u;
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x411Cu;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x8001DF94u: goto L8001DF94;
            case 0x8001DFA0u: goto L8001DFA0;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L8001DF94: ;
        c.S0 = 0x80000000u;
        c.S0 = c.S0 | 0x0009u;
        goto L8001DFC0;
        L8001DFA0: ;
        c.S0 = 0u + 0u;
        goto L8001DFC0;
        L8001DFA8: ;
        c.A3 = 0u + 0u;
        c.A0 = MemoryAccess.ReadU32(m, (c.S1 + 0x28u));
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x2Cu));
        c.A2 = MemoryAccess.ReadU32(m, (c.S1 + 0x30u));
        c.S0 = c.A3 + 0u;
        c.RA = 0x8001DFC0u;
        Dispatcher.Call(c, m, 0x80018100u);
        L8001DFC0: ;
        c.RA = 0x8001DFC8u;
        GranTurismo2PC.func_800171C0(c, m);
        c.V0 = c.S0 + 0u;
        MemoryAccess.WriteU16(m, c.S1, (ushort)c.S2);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001DFEC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.V0 = 0x801D0000u;
        c.V0 = c.V0 - 0x6720u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.V1 = MemoryAccess.ReadU16(m, c.S1);
        c.A0 = c.V0 + 0x3C74u;
        c.V1 = c.V1 - 0x1u;
        c.V1 = c.V1 << 16;
        c.V1 = (uint)((int)c.V1 >> 16);
        c.V0 = c.V1 < 0x00000007u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.S0 = 0u + 0u;
            goto L8001E214;
        }
        c.S0 = 0u + 0u;
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x41CCu;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x8001E0D0u: goto L8001E0D0;
            case 0x8001E214u: goto L8001E214;
            case 0x8001E130u: goto L8001E130;
            case 0x8001E048u: goto L8001E048;
            case 0x8001E090u: goto L8001E090;
            case 0x8001E0ACu: goto L8001E0AC;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L8001E048: ;
        c.A0 = MemoryAccess.ReadU32(m, (c.S1 + 0x30u));
        if (c.A0 == 0u) {
            c.V0 = 0u + 0u;
            goto L8001E218;
        }
        c.V0 = 0u + 0u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x28u));
        if ((int)c.V0 < 0) {
            goto L8001E140;
        }
        c.A1 = 0u + 0u;
        c.RA = 0x8001E070u;
        GranTurismo2PC.func_800181D0(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = 0xFFFFFFFFu;
        if (c.V1 == c.V0) {
            c.V0 = 0x00000001u;
            goto L8001E1C0;
        }
        c.V0 = 0x00000001u;
        if (c.V1 != c.V0) {
            c.V0 = c.S0 + 0u;
            goto L8001E218;
        }
        c.V0 = c.S0 + 0u;
        c.S0 = 0x80000000u;
        goto L8001E210;
        L8001E090: ;
        c.S0 = 0x80000000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x4u));
        if ((int)c.V0 >= 0) {
            c.S0 = c.S0 | 0x000Du;
            goto L8001E214;
        }
        c.S0 = c.S0 | 0x000Du;
        c.S0 = 0x80000000u;
        goto L8001E144;
        L8001E0AC: ;
        c.S0 = 0x80000000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x8u));
        c.S0 = c.S0 | 0x0005u;
        MemoryAccess.WriteU16(m, (c.A0 + 0x4018u), (ushort)c.V0);
        c.A0 = MemoryAccess.ReadU32(m, (c.S1 + 0x8u));
        c.A1 = 0u + 0u;
        c.RA = 0x8001E0C8u;
        GranTurismo2PC.func_80017480(c, m);
        c.V0 = c.S0 + 0u;
        goto L8001E218;
        L8001E0D0: ;
        c.A0 = MemoryAccess.ReadU32(m, (c.S1 + 0xCu));
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x30u));
        c.A2 = 0u + 0u;
        c.RA = 0x8001E0E0u;
        GranTurismo2PC.func_80017914(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = 0xFFFFFFFFu;
        if (c.V1 == c.V0) {
            goto L8001E1C0;
        }
        if ((int)c.V1 >= 0) {
            c.V0 = 0x00000001u;
            goto L8001E10C;
        }
        c.V0 = 0x00000001u;
        c.V0 = 0xFFFFFFFEu;
        if (c.V1 == c.V0) {
            c.V0 = c.S0 + 0u;
            goto L8001E11C;
        }
        c.V0 = c.S0 + 0u;
        goto L8001E218;
        L8001E10C: ;
        if (c.V1 == c.V0) {
            c.V0 = c.S0 + 0u;
            goto L8001E128;
        }
        c.V0 = c.S0 + 0u;
        goto L8001E218;
        L8001E11C: ;
        c.S0 = 0x80000000u;
        c.S0 = c.S0 | 0x000Eu;
        goto L8001E214;
        L8001E128: ;
        c.S0 = 0x80000000u;
        goto L8001E214;
        L8001E130: ;
        c.A0 = MemoryAccess.ReadU32(m, (c.S1 + 0x14u));
        if ((int)c.A0 >= 0) {
            goto L8001E14C;
        }
        L8001E140: ;
        c.S0 = 0x80000000u;
        L8001E144: ;
        c.S0 = c.S0 | 0x0002u;
        goto L8001E214;
        L8001E14C: ;
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x18u));
        c.A2 = 0u + 0u;
        c.RA = 0x8001E158u;
        GranTurismo2PC.func_80017B40(c, m);
        c.V1 = c.V0 + 0x8u;
        c.V0 = c.V1 < 0x0000000Au ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80020000u;
            goto L8001E214;
        }
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x41ECu;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x8001E1A8u: goto L8001E1A8;
            case 0x8001E19Cu: goto L8001E19C;
            case 0x8001E190u: goto L8001E190;
            case 0x8001E184u: goto L8001E184;
            case 0x8001E214u: goto L8001E214;
            case 0x8001E1B4u: goto L8001E1B4;
            case 0x8001E1C0u: goto L8001E1C0;
            case 0x8001E1CCu: goto L8001E1CC;
            case 0x8001E200u: goto L8001E200;
            case 0x8001E20Cu: goto L8001E20C;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L8001E184: ;
        c.S0 = 0x80000000u;
        c.S0 = c.S0 | 0x000Fu;
        goto L8001E214;
        L8001E190: ;
        c.S0 = 0x80000000u;
        c.S0 = c.S0 | 0x0010u;
        goto L8001E214;
        L8001E19C: ;
        c.S0 = 0x80000000u;
        c.S0 = c.S0 | 0x0011u;
        goto L8001E214;
        L8001E1A8: ;
        c.S0 = 0x80000000u;
        c.S0 = c.S0 | 0x0019u;
        goto L8001E214;
        L8001E1B4: ;
        c.S0 = 0x80000000u;
        c.S0 = c.S0 | 0x0003u;
        goto L8001E214;
        L8001E1C0: ;
        c.S0 = 0x80000000u;
        c.S0 = c.S0 | 0x0001u;
        goto L8001E214;
        L8001E1CC: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x18u));
        c.V1 = c.V0 - 0x7u;
        c.V0 = c.V1 < 0x0000001Cu ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x80020000u;
            goto L8001E20C;
        }
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x4214u;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x8001E200u: goto L8001E200;
            case 0x8001E20Cu: goto L8001E20C;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L8001E200: ;
        c.S0 = 0x80000000u;
        c.S0 = c.S0 | 0x000Bu;
        goto L8001E214;
        L8001E20C: ;
        c.S0 = 0x80000000u;
        L8001E210: ;
        c.S0 = c.S0 | 0x0020u;
        L8001E214: ;
        c.V0 = c.S0 + 0u;
        L8001E218: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001E22C(CpuContext c, IMemory m)
    {
        c.V0 = 0x00000100u;
        MemoryAccess.WriteU16(m, (c.A0 + 0xCu), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.A0 + 0x8u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, c.A0, (ushort)c.V0);
        c.V0 = 0x000000FCu;
        MemoryAccess.WriteU16(m, (c.A0 + 0xEu), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.A0 + 0xAu), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.A0 + 0x2u), (ushort)c.V0);
        MemoryAccess.WriteU8(m, (c.A0 + 0x4u), (byte)0u);
        MemoryAccess.WriteU32(m, (c.A0 + 0x18u), 0u);
        MemoryAccess.WriteU32(m, (c.A0 + 0x1Cu), 0u);
        MemoryAccess.WriteU32(m, (c.A0 + 0x10u), 0u);
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), 0u);
        MemoryAccess.WriteU8(m, (c.A0 + 0x20u), (byte)0u);
        MemoryAccess.WriteU32(m, (c.A0 + 0x24u), 0u);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001E26C(CpuContext c, IMemory m)
    {
        c.V0 = MemoryAccess.ReadU16(m, (c.A0 + 0xCu));
        c.V1 = MemoryAccess.ReadU16(m, (c.A0 + 0xEu));
        c.A3 = 0u + 0u;
        MemoryAccess.WriteU16(m, c.A0, (ushort)c.V0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.A0);
        MemoryAccess.WriteU16(m, (c.A0 + 0x2u), (ushort)c.V1);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0x2u));
        c.V0 = c.V0 << 8;
        c.V1 = c.V1 << 8;
        MemoryAccess.WriteU32(m, (c.A0 + 0x1Cu), c.V1);
        c.V1 = c.A1 - 0x2u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x18u), c.V0);
        c.V0 = c.V1 < 0x00000008u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A2 = c.A3 + 0u;
            goto L8001E31C;
        }
        c.A2 = c.A3 + 0u;
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x4284u;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        switch (c.V0)
        {
            case 0x8001E2E8u: goto L8001E2E8;
            case 0x8001E314u: goto L8001E314;
            case 0x8001E2C8u: goto L8001E2C8;
            case 0x8001E2F4u: goto L8001E2F4;
            case 0x8001E2E0u: goto L8001E2E0;
            case 0x8001E2D4u: goto L8001E2D4;
            case 0x8001E300u: goto L8001E300;
            case 0x8001E30Cu: goto L8001E30C;
            default: Dispatcher.Call(c, m, c.V0); return;
        }
        L8001E2C8: ;
        c.A3 = 0x00980000u;
        c.A3 = c.A3 | 0x9680u;
        goto L8001E31C;
        L8001E2D4: ;
        c.A3 = 0x00980000u;
        c.A3 = c.A3 | 0x9680u;
        goto L8001E2E8;
        L8001E2E0: ;
        c.A3 = 0xFF670000u;
        c.A3 = c.A3 | 0x6980u;
        L8001E2E8: ;
        c.A2 = 0x00980000u;
        c.A2 = c.A2 | 0x9680u;
        goto L8001E31C;
        L8001E2F4: ;
        c.A3 = 0xFF670000u;
        c.A3 = c.A3 | 0x6980u;
        goto L8001E31C;
        L8001E300: ;
        c.A3 = 0xFF670000u;
        c.A3 = c.A3 | 0x6980u;
        goto L8001E314;
        L8001E30C: ;
        c.A3 = 0x00980000u;
        c.A3 = c.A3 | 0x9680u;
        L8001E314: ;
        c.A2 = 0xFF670000u;
        c.A2 = c.A2 | 0x6980u;
        L8001E31C: ;
        MemoryAccess.WriteU32(m, (c.A0 + 0x10u), c.A3);
        MemoryAccess.WriteU32(m, (c.A0 + 0x14u), c.A2);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001E328(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x38u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S2);
        c.S2 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.S5);
        c.S5 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.S6);
        c.S6 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x34u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S0);
        c.V0 = MemoryAccess.ReadU8(m, (c.S2 + 0x20u));
        c.V0 = c.V0 + 0x1u;
        MemoryAccess.WriteU8(m, (c.S2 + 0x20u), (byte)c.V0);
        c.V0 = c.V0 << 24;
        c.V0 = (uint)((int)c.V0 >> 24);
        c.V0 = (int)c.V0 < 65 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S4 = c.S5 + 0x178u;
            goto L8001E380;
        }
        c.S4 = c.S5 + 0x178u;
        MemoryAccess.WriteU8(m, (c.S2 + 0x20u), (byte)0u);
        L8001E380: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S2 + 0x4u));
        if (c.V0 != 0u) {
            c.V0 = (int)c.V0 < 2 ? 1u : 0u;
            goto L8001E3CC;
        }
        c.V0 = (int)c.V0 < 2 ? 1u : 0u;
        c.V0 = MemoryAccess.ReadU16(m, (c.S2 + 0xCu));
        c.V1 = MemoryAccess.ReadU16(m, (c.S2 + 0xEu));
        MemoryAccess.WriteU16(m, c.S2, (ushort)c.V0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.S2);
        MemoryAccess.WriteU32(m, (c.S2 + 0x10u), 0u);
        MemoryAccess.WriteU32(m, (c.S2 + 0x14u), 0u);
        MemoryAccess.WriteU16(m, (c.S2 + 0x2u), (ushort)c.V1);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x2u));
        c.V0 = c.V0 << 8;
        c.V1 = c.V1 << 8;
        MemoryAccess.WriteU32(m, (c.S2 + 0x18u), c.V0);
        MemoryAccess.WriteU32(m, (c.S2 + 0x1Cu), c.V1);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S2 + 0x4u));
        c.V0 = (int)c.V0 < 2 ? 1u : 0u;
        L8001E3CC: ;
        if (c.V0 == 0u) {
            goto L8001E57C;
        }
        c.V1 = MemoryAccess.ReadU32(m, (c.S4 + 0x4u));
        c.V0 = MemoryAccess.ReadU32(m, (c.S4 + 0xCu));
        c.A0 = MemoryAccess.ReadU32(m, (c.S5 + 0x178u));
        if (c.S6 != 0u) {
            c.S0 = c.V1 | c.V0;
            goto L8001E3F0;
        }
        c.S0 = c.V1 | c.V0;
        c.A0 = 0u + 0u;
        c.S0 = c.A0 + 0u;
        L8001E3F0: ;
        c.V0 = c.S0 & 0x0010u;
        if (c.V0 == 0u) {
            c.S1 = 0u + 0u;
            goto L8001E400;
        }
        c.S1 = 0u + 0u;
        c.S1 = 0x00000007u;
        L8001E400: ;
        c.V0 = c.S0 & 0x0020u;
        if (c.V0 == 0u) {
            c.V0 = c.S0 & 0x1000u;
            goto L8001E410;
        }
        c.V0 = c.S0 & 0x1000u;
        c.S1 = 0x00000009u;
        L8001E410: ;
        if (c.V0 == 0u) {
            c.V0 = c.S0 & 0x2000u;
            goto L8001E41C;
        }
        c.V0 = c.S0 & 0x2000u;
        c.S1 = 0x00000006u;
        L8001E41C: ;
        if (c.V0 == 0u) {
            c.V0 = c.S0 & 0x0001u;
            goto L8001E428;
        }
        c.V0 = c.S0 & 0x0001u;
        c.S1 = 0x00000008u;
        L8001E428: ;
        if (c.V0 == 0u) {
            c.V0 = c.A0 & 0x0004u;
            goto L8001E44C;
        }
        c.V0 = c.A0 & 0x0004u;
        if (c.V0 == 0u) {
            c.S1 = 0x00000002u;
            goto L8001E43C;
        }
        c.S1 = 0x00000002u;
        c.S1 = 0x00000007u;
        L8001E43C: ;
        c.V0 = c.A0 & 0x0008u;
        if (c.V0 == 0u) {
            c.V0 = c.S0 & 0x0002u;
            goto L8001E450;
        }
        c.V0 = c.S0 & 0x0002u;
        c.S1 = 0x00000006u;
        L8001E44C: ;
        c.V0 = c.S0 & 0x0002u;
        L8001E450: ;
        if (c.V0 == 0u) {
            c.V0 = c.A0 & 0x0004u;
            goto L8001E474;
        }
        c.V0 = c.A0 & 0x0004u;
        if (c.V0 == 0u) {
            c.S1 = 0x00000003u;
            goto L8001E464;
        }
        c.S1 = 0x00000003u;
        c.S1 = 0x00000009u;
        L8001E464: ;
        c.V0 = c.A0 & 0x0008u;
        if (c.V0 == 0u) {
            c.V0 = c.S0 & 0x0004u;
            goto L8001E478;
        }
        c.V0 = c.S0 & 0x0004u;
        c.S1 = 0x00000008u;
        L8001E474: ;
        c.V0 = c.S0 & 0x0004u;
        L8001E478: ;
        if (c.V0 == 0u) {
            c.V0 = c.A0 & 0x0001u;
            goto L8001E49C;
        }
        c.V0 = c.A0 & 0x0001u;
        if (c.V0 == 0u) {
            c.S1 = 0x00000004u;
            goto L8001E48C;
        }
        c.S1 = 0x00000004u;
        c.S1 = 0x00000007u;
        L8001E48C: ;
        c.V0 = c.A0 & 0x0002u;
        if (c.V0 == 0u) {
            c.V0 = c.S0 & 0x0008u;
            goto L8001E4A0;
        }
        c.V0 = c.S0 & 0x0008u;
        c.S1 = 0x00000009u;
        L8001E49C: ;
        c.V0 = c.S0 & 0x0008u;
        L8001E4A0: ;
        if (c.V0 == 0u) {
            c.V0 = c.A0 & 0x0001u;
            goto L8001E4C4;
        }
        c.V0 = c.A0 & 0x0001u;
        if (c.V0 == 0u) {
            c.S1 = 0x00000005u;
            goto L8001E4B4;
        }
        c.S1 = 0x00000005u;
        c.S1 = 0x00000006u;
        L8001E4B4: ;
        c.V0 = c.A0 & 0x0002u;
        if (c.V0 == 0u) {
            goto L8001E4C4;
        }
        c.S1 = 0x00000008u;
        L8001E4C4: ;
        if (c.S1 == 0u) {
            c.A3 = c.S1 + 0u;
            goto L8001E57C;
        }
        c.A3 = c.S1 + 0u;
        c.A0 = c.S5 + 0x2FCu;
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0xCu));
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0xEu));
        c.V1 = MemoryAccess.ReadU32(m, (c.S2 + 0x24u));
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V1);
        c.RA = 0x8001E4ECu;
        Dispatcher.Call(c, m, 0x8001D754u);
        c.S0 = c.V0 + 0u;
        if (c.S0 != 0u) {
            MemoryAccess.WriteU8(m, (c.S2 + 0x6u), (byte)c.S1);
            goto L8001E524;
        }
        MemoryAccess.WriteU8(m, (c.S2 + 0x6u), (byte)c.S1);
        c.A0 = 0u + 0u;
        c.RA = 0x8001E500u;
        GranTurismo2PC.func_80060840(c, m);
        c.A0 = c.S2 + 0u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x8001E50Cu;
        GranTurismo2PC.func_8001E26C(c, m);
        c.V0 = MemoryAccess.ReadU16(m, (c.S2 + 0xCu));
        c.V1 = MemoryAccess.ReadU16(m, (c.S2 + 0xEu));
        c.S1 = 0x00000001u;
        MemoryAccess.WriteU16(m, (c.S2 + 0x8u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.S2 + 0xAu), (ushort)c.V1);
        goto L8001E570;
        L8001E524: ;
        c.A0 = 0x00000005u;
        c.RA = 0x8001E52Cu;
        GranTurismo2PC.func_80060840(c, m);
        c.V0 = MemoryAccess.ReadU16(m, (c.S2 + 0xCu));
        c.V1 = MemoryAccess.ReadU16(m, (c.S2 + 0xEu));
        MemoryAccess.WriteU16(m, (c.S2 + 0x8u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.S2 + 0xAu), (ushort)c.V1);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.S0);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x4u));
        c.V0 = c.V0 + c.V1;
        c.V0 = (uint)((int)c.V0 >> 1);
        MemoryAccess.WriteU16(m, (c.S2 + 0xCu), (ushort)c.V0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x2u));
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x6u));
        MemoryAccess.WriteU32(m, (c.S2 + 0x24u), c.S0);
        MemoryAccess.WriteU8(m, (c.S2 + 0x20u), (byte)0u);
        c.V0 = c.V0 + c.V1;
        c.V0 = (uint)((int)c.V0 >> 1);
        MemoryAccess.WriteU16(m, (c.S2 + 0xEu), (ushort)c.V0);
        L8001E570: ;
        c.V0 = 0x00000008u;
        MemoryAccess.WriteU8(m, (c.S2 + 0x5u), (byte)c.S1);
        MemoryAccess.WriteU8(m, (c.S2 + 0x4u), (byte)c.V0);
        L8001E57C: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S2 + 0x4u));
        c.V1 = MemoryAccess.ReadU8(m, (c.S2 + 0x4u));
        if (c.V0 == 0u) {
            c.V0 = c.V1 - 0x1u;
            goto L8001E704;
        }
        c.V0 = c.V1 - 0x1u;
        MemoryAccess.WriteU8(m, (c.S2 + 0x4u), (byte)c.V0);
        c.V0 = c.V0 << 24;
        c.V0 = (uint)((int)c.V0 >> 24);
        c.V0 = (int)c.V0 < 6 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.T0 = 0x88880000u;
            goto L8001E708;
        }
        c.T0 = 0x88880000u;
        c.S0 = MemoryAccess.ReadU32(m, c.S4);
        if (c.S6 != 0u) {
            goto L8001E5B4;
        }
        c.S0 = 0u + 0u;
        L8001E5B4: ;
        c.S1 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S2 + 0x6u));
        c.V0 = 0x00000003u;
        if (c.S1 == c.V0) {
            c.V0 = (int)c.S1 < 4 ? 1u : 0u;
            goto L8001E614;
        }
        c.V0 = (int)c.S1 < 4 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L8001E5DC;
        }
        c.V0 = 0x00000002u;
        if (c.S1 == c.V0) {
            c.V0 = c.S0 & 0x0004u;
            goto L8001E5F8;
        }
        c.V0 = c.S0 & 0x0004u;
        goto L8001E660;
        L8001E5DC: ;
        c.V0 = 0x00000004u;
        if (c.S1 == c.V0) {
            c.V0 = 0x00000005u;
            goto L8001E628;
        }
        c.V0 = 0x00000005u;
        if (c.S1 == c.V0) {
            c.V0 = c.S0 & 0x0001u;
            goto L8001E648;
        }
        c.V0 = c.S0 & 0x0001u;
        goto L8001E660;
        L8001E5F8: ;
        if (c.V0 == 0u) {
            c.V0 = c.S0 & 0x0008u;
            goto L8001E604;
        }
        c.V0 = c.S0 & 0x0008u;
        c.S1 = 0x00000007u;
        L8001E604: ;
        if (c.V0 == 0u) {
            goto L8001E660;
        }
        c.S1 = 0x00000006u;
        goto L8001E660;
        L8001E614: ;
        c.V0 = c.S0 & 0x0004u;
        if (c.V0 == 0u) {
            c.V0 = c.S0 & 0x0008u;
            goto L8001E654;
        }
        c.V0 = c.S0 & 0x0008u;
        c.S1 = 0x00000009u;
        goto L8001E654;
        L8001E628: ;
        c.V0 = c.S0 & 0x0001u;
        if (c.V0 == 0u) {
            c.V0 = c.S0 & 0x0002u;
            goto L8001E638;
        }
        c.V0 = c.S0 & 0x0002u;
        c.S1 = 0x00000007u;
        L8001E638: ;
        if (c.V0 == 0u) {
            goto L8001E660;
        }
        c.S1 = 0x00000009u;
        goto L8001E660;
        L8001E648: ;
        if (c.V0 == 0u) {
            c.V0 = c.S0 & 0x0002u;
            goto L8001E654;
        }
        c.V0 = c.S0 & 0x0002u;
        c.S1 = 0x00000006u;
        L8001E654: ;
        if (c.V0 == 0u) {
            goto L8001E660;
        }
        c.S1 = 0x00000008u;
        L8001E660: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S2 + 0x6u));
        if (c.S1 == c.V0) {
            c.A0 = c.S5 + 0x2FCu;
            goto L8001E704;
        }
        c.A0 = c.S5 + 0x2FCu;
        c.A3 = c.S1 + 0u;
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0x8u));
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0xAu));
        c.S3 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), 0u);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S3);
        c.RA = 0x8001E68Cu;
        Dispatcher.Call(c, m, 0x8001D754u);
        c.S0 = c.V0 + 0u;
        if (c.S0 == 0u) {
            goto L8001E6D8;
        }
        MemoryAccess.WriteU8(m, (c.S2 + 0x5u), (byte)c.S1);
        MemoryAccess.WriteU8(m, (c.S2 + 0x6u), (byte)c.S1);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.S0);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x4u));
        c.V0 = c.V0 + c.V1;
        c.V0 = (uint)((int)c.V0 >> (int)(c.S3 & 31u));
        MemoryAccess.WriteU16(m, (c.S2 + 0xCu), (ushort)c.V0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x2u));
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x6u));
        MemoryAccess.WriteU32(m, (c.S2 + 0x24u), c.S0);
        MemoryAccess.WriteU8(m, (c.S2 + 0x20u), (byte)0u);
        c.V0 = c.V0 + c.V1;
        c.V0 = (uint)((int)c.V0 >> (int)(c.S3 & 31u));
        MemoryAccess.WriteU16(m, (c.S2 + 0xEu), (ushort)c.V0);
        goto L8001E704;
        L8001E6D8: ;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.S2 + 0x5u));
        if (c.V0 != c.S3) {
            c.T0 = 0x88880000u;
            goto L8001E708;
        }
        c.T0 = 0x88880000u;
        c.A0 = c.S2 + 0u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x8001E6F4u;
        GranTurismo2PC.func_8001E26C(c, m);
        c.V0 = MemoryAccess.ReadU16(m, (c.S2 + 0xCu));
        c.V1 = MemoryAccess.ReadU16(m, (c.S2 + 0xEu));
        MemoryAccess.WriteU16(m, (c.S2 + 0x8u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.S2 + 0xAu), (ushort)c.V1);
        L8001E704: ;
        c.T0 = 0x88880000u;
        L8001E708: ;
        c.T0 = c.T0 | 0x8889u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0xCu));
        c.V1 = MemoryAccess.ReadU32(m, (c.S2 + 0x18u));
        c.A2 = MemoryAccess.ReadU32(m, (c.S2 + 0x10u));
        c.A1 = MemoryAccess.ReadU32(m, (c.S2 + 0x14u));
        c.V0 = c.V0 << 8;
        c.V0 = c.V0 - c.V1;
        c.A0 = c.V0 << 2;
        c.A0 = c.A0 + c.V0;
        c.A0 = c.A0 << 3;
        c.A0 = c.A0 - c.V0;
        c.A0 = c.A0 << 4;
        c.A0 = c.A0 + c.V0;
        c.A0 = c.A0 << 2;
        c.V0 = c.A2 << 3;
        c.V0 = c.V0 - c.A2;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 - c.A2;
        c.V0 = c.V0 << 1;
        c.A0 = c.A0 - c.V0;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S2 + 0xEu));
        c.V1 = MemoryAccess.ReadU32(m, (c.S2 + 0x1Cu));
        { var _r = (long)(int)c.A0 * (int)c.T0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V0 = c.V0 << 8;
        c.V0 = c.V0 - c.V1;
        c.V1 = c.V0 << 2;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 3;
        c.V1 = c.V1 - c.V0;
        c.V1 = c.V1 << 4;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 2;
        c.V0 = c.A1 << 3;
        c.V0 = c.V0 - c.A1;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 - c.A1;
        c.T1 = c.HI;
        c.V0 = c.V0 << 1;
        c.V1 = c.V1 - c.V0;
        { var _r = (long)(int)c.V1 * (int)c.T0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.A3 = 0xB60B0000u;
        c.A3 = c.A3 | 0x60B7u;
        c.V0 = c.T1 + c.A0;
        c.V0 = (uint)((int)c.V0 >> 5);
        c.A0 = (uint)((int)c.A0 >> 31);
        c.V0 = c.V0 - c.A0;
        c.A0 = c.V0 << 1;
        c.A0 = c.A0 + c.V0;
        c.A0 = c.A0 << 3;
        c.T3 = c.HI;
        c.A0 = c.A0 + c.V0;
        c.A0 = c.A0 << 2;
        { var _r = (long)(int)c.A0 * (int)c.A3; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V0 = c.T3 + c.V1;
        c.V0 = (uint)((int)c.V0 >> 5);
        c.V1 = (uint)((int)c.V1 >> 31);
        c.V0 = c.V0 - c.V1;
        c.V1 = c.V0 << 1;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 3;
        c.T1 = c.HI;
        c.V1 = c.V1 + c.V0;
        c.V1 = c.V1 << 2;
        { var _r = (long)(int)c.V1 * (int)c.A3; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V0 = c.T1 + c.A0;
        c.V0 = (uint)((int)c.V0 >> 7);
        c.A0 = (uint)((int)c.A0 >> 31);
        c.T3 = c.HI;
        c.V0 = c.V0 - c.A0;
        c.A2 = c.A2 + c.V0;
        { var _r = (long)(int)c.A2 * (int)c.T0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V0 = c.T3 + c.V1;
        c.V0 = (uint)((int)c.V0 >> 7);
        c.V1 = (uint)((int)c.V1 >> 31);
        c.V0 = c.V0 - c.V1;
        c.A0 = c.HI;
        c.A1 = c.A1 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, (c.S2 + 0x18u));
        { var _r = (long)(int)c.A1 * (int)c.T0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        MemoryAccess.WriteU32(m, (c.S2 + 0x10u), c.A2);
        MemoryAccess.WriteU32(m, (c.S2 + 0x14u), c.A1);
        c.V1 = c.A0 + c.A2;
        c.V1 = (uint)((int)c.V1 >> 5);
        c.A2 = (uint)((int)c.A2 >> 31);
        c.V1 = c.V1 - c.A2;
        c.V0 = c.V0 + c.V1;
        MemoryAccess.WriteU32(m, (c.S2 + 0x18u), c.V0);
        c.A0 = c.V0 + 0u;
        c.V0 = MemoryAccess.ReadU32(m, (c.S2 + 0x1Cu));
        c.T0 = c.HI;
        c.V1 = c.T0 + c.A1;
        c.V1 = (uint)((int)c.V1 >> 5);
        c.A1 = (uint)((int)c.A1 >> 31);
        c.V1 = c.V1 - c.A1;
        c.V0 = c.V0 + c.V1;
        if ((int)c.A0 >= 0) {
            MemoryAccess.WriteU32(m, (c.S2 + 0x1Cu), c.V0);
            goto L8001E890;
        }
        MemoryAccess.WriteU32(m, (c.S2 + 0x1Cu), c.V0);
        c.A0 = c.A0 + 0xFFu;
        L8001E890: ;
        c.V1 = MemoryAccess.ReadU32(m, (c.S2 + 0x1Cu));
        c.V0 = (uint)((int)c.A0 >> 8);
        if ((int)c.V1 >= 0) {
            MemoryAccess.WriteU16(m, c.S2, (ushort)c.V0);
            goto L8001E8A4;
        }
        MemoryAccess.WriteU16(m, c.S2, (ushort)c.V0);
        c.V1 = c.V1 + 0xFFu;
        L8001E8A4: ;
        c.V0 = (uint)((int)c.V1 >> 8);
        MemoryAccess.WriteU16(m, (c.S2 + 0x2u), (ushort)c.V0);
        c.S0 = MemoryAccess.ReadU32(m, (c.S4 + 0x4u));
        if (c.S6 != 0u) {
            c.V0 = c.S0 & 0x0A00u;
            goto L8001E8C0;
        }
        c.V0 = c.S0 & 0x0A00u;
        c.S0 = 0u + 0u;
        c.V0 = c.S0 & 0x0A00u;
        L8001E8C0: ;
        if (c.V0 == 0u) {
            c.V0 = c.S0 & 0x0500u;
            goto L8001E8EC;
        }
        c.V0 = c.S0 & 0x0500u;
        c.A1 = MemoryAccess.ReadU32(m, (c.S2 + 0x24u));
        if (c.A1 == 0u) {
            c.A0 = c.S5 + 0u;
            goto L8001E8EC;
        }
        c.A0 = c.S5 + 0u;
        c.A2 = c.S0 >> 9;
        c.A2 = c.A2 & 0x0001u;
        c.RA = 0x8001E8E4u;
        GranTurismo2PC.func_80014380(c, m);
        if (c.V0 != 0u) {
            c.V0 = c.S0 & 0x0500u;
            goto L8001E8FC;
        }
        c.V0 = c.S0 & 0x0500u;
        L8001E8EC: ;
        if (c.V0 == 0u) {
            goto L8001E8FC;
        }
        c.A0 = c.S5 + 0u;
        c.RA = 0x8001E8FCu;
        GranTurismo2PC.func_800142CC(c, m);
        L8001E8FC: ;
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
    public static void func_8001E924(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = 0x800B0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.S1 - 0x72A4u));
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.A0 = c.A0 + 0x2FCu;
        c.RA = 0x8001E948u;
        GranTurismo2PC.func_8001D6CC(c, m);
        if (c.V0 != 0u) {
            MemoryAccess.WriteU32(m, (c.S0 + 0x24u), c.V0);
            goto L8001E96C;
        }
        MemoryAccess.WriteU32(m, (c.S0 + 0x24u), c.V0);
        c.A0 = MemoryAccess.ReadU32(m, (c.S1 - 0x72A4u));
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0xCu));
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0xEu));
        c.A0 = c.A0 + 0x2FCu;
        c.RA = 0x8001E964u;
        GranTurismo2PC.func_8001D954(c, m);
        MemoryAccess.WriteU32(m, (c.S0 + 0x24u), c.V0);
        goto L8001E9B8;
        L8001E96C: ;
        c.V0 = MemoryAccess.ReadU16(m, (c.S0 + 0xCu));
        c.A0 = MemoryAccess.ReadU16(m, (c.S0 + 0xEu));
        c.V1 = MemoryAccess.ReadU32(m, (c.S0 + 0x24u));
        MemoryAccess.WriteU16(m, (c.S0 + 0x8u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.S0 + 0xAu), (ushort)c.A0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.V1);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.V1 + 0x4u));
        c.V0 = c.V0 + c.V1;
        c.V1 = MemoryAccess.ReadU32(m, (c.S0 + 0x24u));
        c.V0 = (uint)((int)c.V0 >> 1);
        MemoryAccess.WriteU16(m, (c.S0 + 0xCu), (ushort)c.V0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V1 + 0x2u));
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V1 + 0x6u));
        c.V1 = 0x00000008u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x4u), (byte)c.V1);
        c.V0 = c.V0 + c.A0;
        c.V0 = (uint)((int)c.V0 >> 1);
        MemoryAccess.WriteU16(m, (c.S0 + 0xEu), (ushort)c.V0);
        L8001E9B8: ;
        MemoryAccess.WriteU8(m, (c.S0 + 0x20u), (byte)0u);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001E9D0(CpuContext c, IMemory m)
    {
        c.V0 = MemoryAccess.ReadU16(m, (c.A0 + 0xCu));
        MemoryAccess.WriteU32(m, (c.A0 + 0x24u), c.A1);
        c.A1 = MemoryAccess.ReadU16(m, (c.A0 + 0xEu));
        c.V1 = MemoryAccess.ReadU32(m, (c.A0 + 0x24u));
        MemoryAccess.WriteU16(m, (c.A0 + 0x8u), (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.A0 + 0xAu), (ushort)c.A1);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.V1);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.V1 + 0x4u));
        c.A1 = MemoryAccess.ReadU32(m, (c.A0 + 0x24u));
        c.V0 = c.V0 + c.V1;
        c.V0 = (uint)((int)c.V0 >> 1);
        MemoryAccess.WriteU16(m, (c.A0 + 0xCu), (ushort)c.V0);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0x2u));
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0x6u));
        c.V0 = 0x00000008u;
        MemoryAccess.WriteU8(m, (c.A0 + 0x4u), (byte)c.V0);
        MemoryAccess.WriteU8(m, (c.A0 + 0x20u), (byte)0u);
        c.V1 = c.V1 + c.A1;
        c.V1 = (uint)((int)c.V1 >> 1);
        MemoryAccess.WriteU16(m, (c.A0 + 0xEu), (ushort)c.V1);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001EA24(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A1 + 0u;
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.V1 = MemoryAccess.ReadU32(m, (c.S1 + 0x24u));
        if (c.V1 == 0u) {
            c.S0 = c.V0 + 0x924u;
            goto L8001EA58;
        }
        c.S0 = c.V0 + 0x924u;
        c.S0 = c.S0 + 0xCu;
        L8001EA58: ;
        c.A0 = c.S2 + 0u;
        c.A1 = 0x00800000u;
        c.A1 = c.A1 | 0x8080u;
        c.RA = 0x8001EA68u;
        GranTurismo2PC.func_80081478(c, m);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x2u));
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, c.S1);
        c.V1 = c.V1 << 16;
        c.A0 = c.A0 + c.V1;
        MemoryAccess.WriteU32(m, c.V0, c.A0);
        c.V1 = MemoryAccess.ReadU32(m, c.S0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x4u), c.V1);
        c.V1 = MemoryAccess.ReadU32(m, (c.S0 + 0x4u));
        MemoryAccess.WriteU32(m, (c.V0 + 0x8u), c.V1);
        c.A1 = MemoryAccess.ReadU16(m, (c.S0 + 0x8u));
        c.A0 = c.S2 + 0u;
        c.RA = 0x8001EAA0u;
        GranTurismo2PC.func_8007DA44(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001EAB8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x38u;
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.S7);
        c.S7 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.FP);
        c.FP = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        c.S4 = c.S2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S6);
        c.S6 = c.S2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x34u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        L8001EAF8: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.S7);
        c.V0 = (int)c.S4 < (int)c.V0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S6 + c.S7;
            goto L8001EBDC;
        }
        c.V0 = c.S6 + c.S7;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x90u));
        c.S1 = 0u + 0u;
        c.S3 = c.S2 + 0u;
        c.S0 = c.S2 + 0u;
        c.RA = 0x8001EB20u;
        GranTurismo2PC.func_80060AE8(c, m);
        if ((int)c.S2 <= 0) {
            c.S5 = c.V0 + 0u;
            goto L8001EB94;
        }
        c.S5 = c.V0 + 0u;
        L8001EB28: ;
        c.V0 = c.S3 + c.S1;
        L8001EB2C: ;
        c.S0 = (uint)((int)c.V0 >> 1);
        c.V0 = c.S0 << 1;
        c.V0 = c.V0 + c.FP;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, c.V0);
        c.V0 = c.V1 << 2;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.S7;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x90u));
        c.RA = 0x8001EB64u;
        GranTurismo2PC.func_80060AE8(c, m);
        c.A0 = c.S5 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001EB70u;
        GranTurismo2PC.func_8008CF00(c, m);
        if ((int)c.V0 >= 0) {
            goto L8001EB88;
        }
        if (c.S1 == c.S0) {
            c.S3 = c.S0 + 0u;
            goto L8001EB94;
        }
        c.S3 = c.S0 + 0u;
        c.V0 = c.S3 + c.S1;
        goto L8001EB2C;
        L8001EB88: ;
        if (c.S1 != c.S0) {
            c.S1 = c.S0 + 0u;
            goto L8001EB28;
        }
        c.S1 = c.S0 + 0u;
        c.S0 = c.S3 + 0u;
        L8001EB94: ;
        c.V0 = (int)c.S0 < (int)c.S2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A0 = c.S2 + 0u;
            goto L8001EBC0;
        }
        c.A0 = c.S2 + 0u;
        c.V0 = c.S2 << 1;
        c.V1 = c.V0 + c.FP;
        L8001EBA8: ;
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 - 0x2u));
        c.A0 = c.A0 - 0x1u;
        MemoryAccess.WriteU16(m, c.V1, (ushort)c.V0);
        c.V0 = (int)c.S0 < (int)c.A0 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V1 = c.V1 - 0x2u;
            goto L8001EBA8;
        }
        c.V1 = c.V1 - 0x2u;
        L8001EBC0: ;
        c.V0 = c.S0 << 1;
        c.V0 = c.V0 + c.FP;
        MemoryAccess.WriteU16(m, c.V0, (ushort)c.S4);
        c.S2 = c.S2 + 0x1u;
        c.S6 = c.S6 + 0xA4u;
        c.S4 = c.S4 + 0x1u;
        goto L8001EAF8;
        L8001EBDC: ;
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
    public static void func_8001EC0C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x38u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        c.S4 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = c.A3 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.S7);
        c.S7 = 0x1F800000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S6);
        c.S6 = c.S7 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x48u));
        c.S6 = c.S6 | 0x01C0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S5);
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0x58u));
        c.A1 = c.S7 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.RA);
        c.A0 = c.S0 + 0u;
        c.RA = 0x8001EC64u;
        GranTurismo2PC.func_800771AC(c, m);
        c.A0 = c.S7 + 0u;
        c.A1 = c.S7 + 0u;
        c.A1 = c.A1 | 0x01C0u;
        c.RA = 0x8001EC74u;
        GranTurismo2PC.func_80075930(c, m);
        c.V0 = c.S4 + 0x8u;
        c.V1 = c.S0 + 0x80u;
        MemoryAccess.WriteU32(m, c.S4, c.S1);
        MemoryAccess.WriteU32(m, (c.S4 + 0x8Cu), c.S2);
        MemoryAccess.WriteU32(m, (c.S4 + 0x4u), c.S3);
        L8001EC88: ;
        c.T0 = MemoryAccess.ReadU32(m, c.S0);
        c.T1 = MemoryAccess.ReadU32(m, (c.S0 + 0x4u));
        c.T2 = MemoryAccess.ReadU32(m, (c.S0 + 0x8u));
        c.T3 = MemoryAccess.ReadU32(m, (c.S0 + 0xCu));
        MemoryAccess.WriteU32(m, c.V0, c.T0);
        MemoryAccess.WriteU32(m, (c.V0 + 0x4u), c.T1);
        MemoryAccess.WriteU32(m, (c.V0 + 0x8u), c.T2);
        MemoryAccess.WriteU32(m, (c.V0 + 0xCu), c.T3);
        c.S0 = c.S0 + 0x10u;
        if (c.S0 != c.V1) {
            c.V0 = c.V0 + 0x10u;
            goto L8001EC88;
        }
        c.V0 = c.V0 + 0x10u;
        c.A0 = 0x00000006u;
        c.T0 = MemoryAccess.ReadU32(m, c.S0);
        MemoryAccess.WriteU32(m, c.V0, c.T0);
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x4Cu));
        c.V1 = c.S4 + c.A0;
        MemoryAccess.WriteU32(m, (c.S4 + 0x90u), c.V0);
        L8001ECD0: ;
        MemoryAccess.WriteU8(m, (c.V1 + 0x9Au), (byte)0u);
        c.A0 = c.A0 - 0x1u;
        if ((int)c.A0 >= 0) {
            c.V1 = c.V1 - 0x1u;
            goto L8001ECD0;
        }
        c.V1 = c.V1 - 0x1u;
        if (c.S5 == 0u) {
            c.A0 = 0u + 0u;
            goto L8001ED08;
        }
        c.A0 = 0u + 0u;
        c.V1 = c.S4 + c.A0;
        L8001ECEC: ;
        c.V0 = c.S5 + c.A0;
        c.V0 = MemoryAccess.ReadU8(m, c.V0);
        c.A0 = c.A0 + 0x1u;
        MemoryAccess.WriteU8(m, (c.V1 + 0x9Au), (byte)c.V0);
        c.V0 = (int)c.A0 < 7 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V1 = c.S4 + c.A0;
            goto L8001ECEC;
        }
        c.V1 = c.S4 + c.A0;
        L8001ED08: ;
        c.V0 = MemoryAccess.ReadU16(m, (c.S4 + 0x98u));
        c.V1 = MemoryAccess.ReadU16(m, c.S6);
        c.A0 = MemoryAccess.ReadU16(m, (c.S4 + 0x94u));
        c.V0 = c.V0 & 0xC000u;
        c.V0 = c.V0 | c.V1;
        MemoryAccess.WriteU16(m, (c.S4 + 0x98u), (ushort)c.V0);
        c.V0 = MemoryAccess.ReadU16(m, (c.S6 + 0x4u));
        c.V1 = MemoryAccess.ReadU16(m, (c.S4 + 0x98u));
        c.A0 = c.A0 & 0xE000u;
        MemoryAccess.WriteU16(m, (c.S4 + 0x96u), (ushort)c.V0);
        c.V0 = MemoryAccess.ReadU16(m, (c.S7 + 0x5Au));
        c.V1 = c.V1 & 0x7FFFu;
        c.V0 = c.V0 << 16;
        c.V0 = (uint)((int)c.V0 >> 16);
        c.A0 = c.A0 | c.V0;
        MemoryAccess.WriteU16(m, (c.S4 + 0x94u), (ushort)c.A0);
        c.A0 = c.A0 & 0x1FFFu;
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x50u));
        c.A1 = MemoryAccess.ReadU8(m, (c.S7 + 0x8Au));
        c.V0 = c.V0 << 15;
        c.V1 = c.V1 | c.V0;
        c.V1 = c.V1 & 0xBFFFu;
        c.A1 = c.A1 << 13;
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x54u));
        c.A0 = c.A0 | c.A1;
        MemoryAccess.WriteU16(m, (c.S4 + 0xA2u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.S4 + 0x94u), (ushort)c.A0);
        c.V0 = c.V0 << 14;
        c.V1 = c.V1 | c.V0;
        MemoryAccess.WriteU16(m, (c.S4 + 0x98u), (ushort)c.V1);
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
    public static void func_8001EDAC(CpuContext c, IMemory m)
    {
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0x4018u));
        if (c.V0 != c.A1) {
            goto L8001EDC8;
        }
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU16(m, (c.A0 + 0x4018u), (ushort)c.V0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0x4018u));
        L8001EDC8: ;
        c.V1 = MemoryAccess.ReadU16(m, (c.A0 + 0x4018u));
        c.V0 = (int)c.A1 < (int)c.V0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.V1 - 0x1u;
            goto L8001EDDC;
        }
        c.V0 = c.V1 - 0x1u;
        MemoryAccess.WriteU16(m, (c.A0 + 0x4018u), (ushort)c.V0);
        L8001EDDC: ;
        c.V0 = c.A1 << 2;
        c.V0 = c.V0 + c.A1;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.A1;
        c.V0 = c.V0 << 2;
        c.T0 = c.V0 + 0xA4u;
        c.A3 = c.V0 + c.A0;
        L8001EDF8: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.A0);
        c.V0 = c.V0 - 0x1u;
        c.V0 = (int)c.A1 < (int)c.V0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A2 = c.A3 + 0x4u;
            goto L8001EE64;
        }
        c.A2 = c.A3 + 0x4u;
        c.V0 = c.A0 + c.T0;
        c.V1 = c.V0 + 0x4u;
        c.V0 = c.V0 + 0xA4u;
        L8001EE1C: ;
        c.T1 = MemoryAccess.ReadU32(m, c.V1);
        c.T2 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T3 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T4 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, c.A2, c.T1);
        MemoryAccess.WriteU32(m, (c.A2 + 0x4u), c.T2);
        MemoryAccess.WriteU32(m, (c.A2 + 0x8u), c.T3);
        MemoryAccess.WriteU32(m, (c.A2 + 0xCu), c.T4);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.V0) {
            c.A2 = c.A2 + 0x10u;
            goto L8001EE1C;
        }
        c.A2 = c.A2 + 0x10u;
        c.T1 = MemoryAccess.ReadU32(m, c.V1);
        MemoryAccess.WriteU32(m, c.A2, c.T1);
        c.T0 = c.T0 + 0xA4u;
        c.A3 = c.A3 + 0xA4u;
        c.A1 = c.A1 + 0x1u;
        goto L8001EDF8;
        L8001EE64: ;
        c.V0 = MemoryAccess.ReadU16(m, c.A0);
        c.V0 = c.V0 - 0x1u;
        MemoryAccess.WriteU16(m, c.A0, (ushort)c.V0);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001EE78(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x30u;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.RA);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, c.S0);
        c.V0 = (int)c.V1 < 100 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.T0 = c.A2 + 0u;
            goto L8001EEFC;
        }
        c.T0 = c.A2 + 0u;
        c.A0 = c.V1 << 2;
        c.A0 = c.A0 + c.V1;
        c.A0 = c.A0 << 3;
        c.A0 = c.A0 + c.V1;
        c.A0 = c.A0 << 2;
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x40u));
        c.A0 = c.A0 + 0x4u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x44u));
        c.A0 = c.S0 + c.A0;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x48u));
        c.A2 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.A3);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x4Cu));
        c.A3 = c.T0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.V0);
        c.RA = 0x8001EEE8u;
        GranTurismo2PC.func_8001EC0C(c, m);
        c.V1 = MemoryAccess.ReadU16(m, c.S0);
        c.V0 = 0x00000001u;
        c.V1 = c.V1 + c.V0;
        MemoryAccess.WriteU16(m, c.S0, (ushort)c.V1);
        goto L8001EF00;
        L8001EEFC: ;
        c.V0 = 0u + 0u;
        L8001EF00: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x2Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.SP = c.SP + 0x30u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001EF10(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0xA8u;
        c.A3 = c.SP + 0u;
        c.V0 = c.A1 << 2;
        c.V0 = c.V0 + c.A1;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.A1;
        c.V0 = c.V0 << 2;
        c.V0 = c.A0 + c.V0;
        c.V1 = c.V0 + 0x4u;
        c.V0 = c.V0 + 0xA4u;
        L8001EF38: ;
        c.T3 = MemoryAccess.ReadU32(m, c.V1);
        c.T4 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T5 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T6 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, c.A3, c.T3);
        MemoryAccess.WriteU32(m, (c.A3 + 0x4u), c.T4);
        MemoryAccess.WriteU32(m, (c.A3 + 0x8u), c.T5);
        MemoryAccess.WriteU32(m, (c.A3 + 0xCu), c.T6);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.V0) {
            c.A3 = c.A3 + 0x10u;
            goto L8001EF38;
        }
        c.A3 = c.A3 + 0x10u;
        c.T3 = MemoryAccess.ReadU32(m, c.V1);
        c.V0 = (int)c.A2 < (int)c.A1 ? 1u : 0u;
        if (c.V0 == 0u) {
            MemoryAccess.WriteU32(m, c.A3, c.T3);
            goto L8001F004;
        }
        MemoryAccess.WriteU32(m, c.A3, c.T3);
        c.T1 = c.A1 - 0x1u;
        c.V0 = (int)c.T1 < (int)c.A2 ? 1u : 0u;
        if (c.V0 != 0u) {
            goto L8001F088;
        }
        c.V0 = c.T1 << 2;
        c.V0 = c.V0 + c.T1;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.T1;
        c.V0 = c.V0 << 2;
        c.A3 = c.V0 + c.A0;
        c.T2 = c.V0 + 0xA4u;
        L8001EFA0: ;
        c.V0 = c.A0 + c.T2;
        c.V0 = c.V0 + 0x4u;
        c.V1 = c.A3 + 0x4u;
        c.T0 = c.A3 + 0xA4u;
        L8001EFB0: ;
        c.T3 = MemoryAccess.ReadU32(m, c.V1);
        c.T4 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T5 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T6 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, c.V0, c.T3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x4u), c.T4);
        MemoryAccess.WriteU32(m, (c.V0 + 0x8u), c.T5);
        MemoryAccess.WriteU32(m, (c.V0 + 0xCu), c.T6);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.T0) {
            c.V0 = c.V0 + 0x10u;
            goto L8001EFB0;
        }
        c.V0 = c.V0 + 0x10u;
        c.T3 = MemoryAccess.ReadU32(m, c.V1);
        MemoryAccess.WriteU32(m, c.V0, c.T3);
        c.A3 = c.A3 - 0xA4u;
        c.T1 = c.T1 - 0x1u;
        c.V0 = (int)c.T1 < (int)c.A2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.T2 = c.T2 - 0xA4u;
            goto L8001EFA0;
        }
        c.T2 = c.T2 - 0xA4u;
        c.V0 = c.A2 << 2;
        goto L8001F08C;
        L8001F004: ;
        c.V0 = (int)c.A1 < (int)c.A2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.T1 = c.A1 + 0u;
            goto L8001F088;
        }
        c.T1 = c.A1 + 0u;
        c.V0 = c.A1 << 2;
        c.V0 = c.V0 + c.A1;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.A1;
        c.V0 = c.V0 << 2;
        c.T2 = c.V0 + 0xA4u;
        c.T0 = c.V0 + c.A0;
        L8001F02C: ;
        c.A3 = c.T0 + 0x4u;
        c.V0 = c.A0 + c.T2;
        c.V1 = c.V0 + 0x4u;
        c.V0 = c.V0 + 0xA4u;
        L8001F03C: ;
        c.T3 = MemoryAccess.ReadU32(m, c.V1);
        c.T4 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T5 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T6 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, c.A3, c.T3);
        MemoryAccess.WriteU32(m, (c.A3 + 0x4u), c.T4);
        MemoryAccess.WriteU32(m, (c.A3 + 0x8u), c.T5);
        MemoryAccess.WriteU32(m, (c.A3 + 0xCu), c.T6);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.V0) {
            c.A3 = c.A3 + 0x10u;
            goto L8001F03C;
        }
        c.A3 = c.A3 + 0x10u;
        c.T3 = MemoryAccess.ReadU32(m, c.V1);
        MemoryAccess.WriteU32(m, c.A3, c.T3);
        c.T2 = c.T2 + 0xA4u;
        c.T1 = c.T1 + 0x1u;
        c.V0 = (int)c.T1 < (int)c.A2 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.T0 = c.T0 + 0xA4u;
            goto L8001F02C;
        }
        c.T0 = c.T0 + 0xA4u;
        L8001F088: ;
        c.V0 = c.A2 << 2;
        L8001F08C: ;
        c.V0 = c.V0 + c.A2;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.A2;
        c.V0 = c.V0 << 2;
        c.V0 = c.A0 + c.V0;
        c.V0 = c.V0 + 0x4u;
        c.V1 = c.SP + 0u;
        c.A3 = c.SP + 0xA0u;
        L8001F0AC: ;
        c.T3 = MemoryAccess.ReadU32(m, c.V1);
        c.T4 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T5 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T6 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, c.V0, c.T3);
        MemoryAccess.WriteU32(m, (c.V0 + 0x4u), c.T4);
        MemoryAccess.WriteU32(m, (c.V0 + 0x8u), c.T5);
        MemoryAccess.WriteU32(m, (c.V0 + 0xCu), c.T6);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.A3) {
            c.V0 = c.V0 + 0x10u;
            goto L8001F0AC;
        }
        c.V0 = c.V0 + 0x10u;
        c.T3 = MemoryAccess.ReadU32(m, c.V1);
        MemoryAccess.WriteU32(m, c.V0, c.T3);
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.A0 + 0x4018u));
        c.T0 = MemoryAccess.ReadU16(m, (c.A0 + 0x4018u));
        if ((int)c.A3 < 0) {
            c.V1 = (int)c.A3 < (int)c.A1 ? 1u : 0u;
            goto L8001F11C;
        }
        c.V1 = (int)c.A3 < (int)c.A1 ? 1u : 0u;
        c.V0 = (int)c.A3 < (int)c.A2 ? 1u : 0u;
        c.V0 = c.V0 ^ 0x0001u;
        c.V1 = c.V1 & c.V0;
        if (c.V1 == 0u) {
            c.V0 = c.T0 + 0x1u;
            goto L8001F110;
        }
        c.V0 = c.T0 + 0x1u;
        MemoryAccess.WriteU16(m, (c.A0 + 0x4018u), (ushort)c.V0);
        goto L8001F11C;
        L8001F110: ;
        if (c.A3 != c.A1) {
            goto L8001F11C;
        }
        MemoryAccess.WriteU16(m, (c.A0 + 0x4018u), (ushort)c.A2);
        L8001F11C: ;
        c.SP = c.SP + 0xA8u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001F124(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x38u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S5);
        c.S5 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S6);
        c.S6 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.S7);
        c.S7 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = 0x00000004u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.S5 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.S2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        c.S4 = (uint)(sbyte)MemoryAccess.ReadU8(m, c.S6);
        L8001F170: ;
        c.V0 = (int)c.S3 < (int)c.S4 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.S6 + c.S0;
            goto L8001F1AC;
        }
        c.V0 = c.S6 + c.S0;
        c.S0 = c.S0 + 0x14u;
        c.A0 = MemoryAccess.ReadU32(m, c.V0);
        c.S3 = c.S3 + 0x1u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x4u), c.A0);
        c.RA = 0x8001F190u;
        GranTurismo2PC.func_80060AE8(c, m);
        c.A0 = c.S5 + c.S2;
        c.A0 = c.A0 + 0x4u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001F1A0u;
        GranTurismo2PC.func_8008CEDC(c, m);
        c.S2 = c.S2 + 0x48u;
        c.S1 = c.S1 + 0x48u;
        goto L8001F170;
        L8001F1AC: ;
        c.A0 = c.S7 + 0u;
        c.S0 = c.S4 << 3;
        c.S0 = c.S0 + c.S4;
        c.S0 = c.S0 << 3;
        c.V0 = c.S5 + c.S0;
        MemoryAccess.WriteU32(m, (c.V0 + 0x4u), c.A0);
        c.RA = 0x8001F1C8u;
        GranTurismo2PC.func_80060AE8(c, m);
        c.S0 = c.S0 + 0x4u;
        c.S0 = c.S5 + c.S0;
        c.A0 = c.S0 + 0x4u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x8001F1DCu;
        GranTurismo2PC.func_8008CEDC(c, m);
        c.V0 = c.S4 + 0x1u;
        MemoryAccess.WriteU16(m, c.S5, (ushort)c.V0);
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
    public static void func_8001F210(CpuContext c, IMemory m)
    {
        c.A0 = 0u + 0u;
        c.V0 = 0x800C0000u;
        c.A1 = c.V0 - 0x6EF8u;
        c.V0 = 0x800C0000u;
        c.V1 = c.V0 - 0x6CF8u;
        L8001F224: ;
        MemoryAccess.WriteU16(m, c.V1, (ushort)0u);
        MemoryAccess.WriteU16(m, c.A1, (ushort)0u);
        c.A1 = c.A1 + 0x2u;
        c.A0 = c.A0 + 0x1u;
        c.V0 = (int)c.A0 < 256 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V1 = c.V1 + 0x2u;
            goto L8001F224;
        }
        c.V1 = c.V1 + 0x2u;
        c.A0 = 0u + 0u;
        c.V0 = 0x800C0000u;
        c.A2 = c.V0 - 0x6CF8u;
        c.V0 = 0x80050000u;
        c.A1 = c.V0 + 0x2100u;
        L8001F254: ;
        c.V1 = MemoryAccess.ReadU16(m, c.A1);
        c.V0 = (int)c.V1 < 256 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.V1 << 1;
            goto L8001F270;
        }
        c.V0 = c.V1 << 1;
        c.V0 = c.V0 + c.A2;
        MemoryAccess.WriteU16(m, c.V0, (ushort)c.A0);
        L8001F270: ;
        c.A0 = c.A0 + 0x1u;
        c.V0 = (int)c.A0 < 267 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.A1 = c.A1 + 0x8u;
            goto L8001F254;
        }
        c.A1 = c.A1 + 0x8u;
        c.A0 = 0u + 0u;
        c.V0 = 0x800C0000u;
        c.A2 = c.V0 - 0x6EF8u;
        c.V0 = 0x80050000u;
        c.A1 = c.V0 + 0x2040u;
        L8001F294: ;
        c.V1 = MemoryAccess.ReadU16(m, c.A1);
        c.V0 = (int)c.V1 < 256 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.V1 << 1;
            goto L8001F2B0;
        }
        c.V0 = c.V1 << 1;
        c.V0 = c.V0 + c.A2;
        MemoryAccess.WriteU16(m, c.V0, (ushort)c.A0);
        L8001F2B0: ;
        c.A0 = c.A0 + 0x1u;
        c.V0 = (int)c.A0 < 24 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.A1 = c.A1 + 0x8u;
            goto L8001F294;
        }
        c.A1 = c.A1 + 0x8u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001F2C8(CpuContext c, IMemory m)
    {
        c.V1 = 0u + 0u;
        c.V0 = c.A0 < 0x00000100u ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A3 = 0x0000010Bu;
            goto L8001F2F4;
        }
        c.A3 = 0x0000010Bu;
        c.V1 = 0x800C0000u;
        c.V1 = c.V1 - 0x6CF8u;
        c.V0 = c.A0 << 1;
        c.V0 = c.V0 + c.V1;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.V0);
        L8001F2EC: ;
        return;
        L8001F2F4: ;
        c.A2 = 0u + 0u;
        c.V0 = 0x80050000u;
        c.T0 = c.V0 + 0x2100u;
        c.V0 = c.A3 + c.V1;
        L8001F304: ;
        c.A1 = (uint)((int)c.V0 >> 1);
        c.V0 = c.A1 << 3;
        c.V0 = c.V0 + c.T0;
        c.V0 = MemoryAccess.ReadU16(m, c.V0);
        if (c.V0 == c.A0) {
            c.V0 = c.A0 < c.V0 ? 1u : 0u;
            goto L8001F358;
        }
        c.V0 = c.A0 < c.V0 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L8001F338;
        }
        if (c.V1 == c.A1) {
            c.A3 = c.A1 + 0u;
            goto L8001F360;
        }
        c.A3 = c.A1 + 0u;
        c.A2 = c.A2 + 0x1u;
        goto L8001F344;
        L8001F338: ;
        if (c.V1 == c.A1) {
            c.V1 = c.A1 + 0u;
            goto L8001F360;
        }
        c.V1 = c.A1 + 0u;
        c.A2 = c.A2 + 0x1u;
        L8001F344: ;
        c.V0 = (int)c.A2 < 16 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = c.A3 + c.V1;
            goto L8001F304;
        }
        c.V0 = c.A3 + c.V1;
        c.V0 = 0u + 0u;
        return;
        L8001F358: ;
        c.V0 = c.A1 + 0u;
        return;
        L8001F360: ;
        c.V0 = 0x80050000u;
        c.V0 = c.V0 + 0x2100u;
        c.V1 = c.A1 << 3;
        c.V1 = c.V1 + c.V0;
        c.V1 = MemoryAccess.ReadU16(m, c.V1);
        if (c.V1 == c.A0) {
            c.V0 = c.A1 + 0u;
            goto L8001F2EC;
        }
        c.V0 = c.A1 + 0u;
        c.V0 = 0u + 0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001F38C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x38u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S5);
        c.S5 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        c.S4 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x34u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.A3);
        c.A0 = MemoryAccess.ReadU16(m, c.S2);
        c.FP = MemoryAccess.ReadU32(m, (c.SP + 0x48u));
        if (c.A0 == 0u) {
            c.S2 = c.S2 + 0x2u;
            goto L8001F47C;
        }
        c.S2 = c.S2 + 0x2u;
        c.V0 = 0x80050000u;
        c.S7 = c.V0 + 0x2100u;
        c.V0 = 0x80050000u;
        c.S6 = c.V0 + 0x13BCu;
        L8001F3EC: ;
        c.RA = 0x8001F3F4u;
        GranTurismo2PC.func_8001F2C8(c, m);
        c.A0 = c.S5 + 0u;
        c.A1 = c.FP + 0u;
        c.S0 = c.V0 + 0u;
        c.RA = 0x8001F404u;
        GranTurismo2PC.func_80081478(c, m);
        c.S1 = c.S0 << 3;
        c.S1 = c.S1 + c.S7;
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x2u));
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x4u));
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x44u));
        c.A0 = c.S3 + c.A0;
        c.V1 = c.T0 + c.V1;
        c.V1 = c.V1 << 16;
        c.A0 = c.A0 + c.V1;
        c.V1 = c.S0 << 1;
        c.V1 = c.V1 + c.S0;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.S6;
        MemoryAccess.WriteU32(m, c.V0, c.A0);
        c.A0 = MemoryAccess.ReadU32(m, c.V1);
        MemoryAccess.WriteU32(m, (c.V0 + 0x4u), c.A0);
        c.A0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        MemoryAccess.WriteU32(m, (c.V0 + 0x8u), c.A0);
        c.A1 = MemoryAccess.ReadU16(m, (c.V1 + 0x8u));
        c.A0 = c.S5 + 0u;
        c.A1 = c.A1 | 0x0040u;
        c.RA = 0x8001F464u;
        GranTurismo2PC.func_8007DA44(c, m);
        c.A0 = MemoryAccess.ReadU16(m, c.S2);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x6u));
        c.S2 = c.S2 + 0x2u;
        c.S3 = c.S3 + c.V0;
        if (c.A0 != 0u) {
            c.S4 = c.S4 + c.V0;
            goto L8001F3EC;
        }
        c.S4 = c.S4 + c.V0;
        L8001F47C: ;
        c.V0 = c.S4 + 0u;
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
    public static void func_8001F4B0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x30u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S2);
        c.S2 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S1);
        c.S1 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S3);
        c.S3 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S4);
        c.S4 = c.A3 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x40u));
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.RA);
        c.A0 = c.S1 + 0u;
        c.RA = 0x8001F4E8u;
        GranTurismo2PC.func_8001F6CC(c, m);
        c.A0 = c.S2 + 0u;
        c.A1 = c.S1 + 0u;
        c.A2 = c.S3 - c.V0;
        c.A3 = c.S4 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.RA = 0x8001F500u;
        GranTurismo2PC.func_8001F38C(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x2Cu));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.SP = c.SP + 0x30u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001F520(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x38u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S5);
        c.S5 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        c.S4 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x34u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.A3);
        c.V0 = MemoryAccess.ReadU8(m, c.S2);
        c.FP = MemoryAccess.ReadU32(m, (c.SP + 0x48u));
        c.V0 = c.V0 << 24;
        c.A0 = (uint)((int)c.V0 >> 24);
        c.V0 = c.A0 & 0xFFFFu;
        if (c.V0 == 0u) {
            c.S2 = c.S2 + 0x1u;
            goto L8001F628;
        }
        c.S2 = c.S2 + 0x1u;
        c.V0 = 0x80050000u;
        c.S7 = c.V0 + 0x2100u;
        c.V0 = 0x80050000u;
        c.S6 = c.V0 + 0x13BCu;
        L8001F58C: ;
        c.A0 = c.A0 & 0xFFFFu;
        c.RA = 0x8001F594u;
        GranTurismo2PC.func_8001F2C8(c, m);
        c.A0 = c.S5 + 0u;
        c.A1 = c.FP + 0u;
        c.S0 = c.V0 + 0u;
        c.RA = 0x8001F5A4u;
        GranTurismo2PC.func_80081478(c, m);
        c.S1 = c.S0 << 3;
        c.S1 = c.S1 + c.S7;
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x2u));
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x4u));
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x44u));
        c.A0 = c.S3 + c.A0;
        c.V1 = c.T0 + c.V1;
        c.V1 = c.V1 << 16;
        c.A0 = c.A0 + c.V1;
        c.V1 = c.S0 << 1;
        c.V1 = c.V1 + c.S0;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.S6;
        MemoryAccess.WriteU32(m, c.V0, c.A0);
        c.A0 = MemoryAccess.ReadU32(m, c.V1);
        MemoryAccess.WriteU32(m, (c.V0 + 0x4u), c.A0);
        c.A0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        MemoryAccess.WriteU32(m, (c.V0 + 0x8u), c.A0);
        c.A1 = MemoryAccess.ReadU16(m, (c.V1 + 0x8u));
        c.A0 = c.S5 + 0u;
        c.A1 = c.A1 | 0x0040u;
        c.RA = 0x8001F604u;
        GranTurismo2PC.func_8007DA44(c, m);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x6u));
        c.V0 = MemoryAccess.ReadU8(m, c.S2);
        c.S2 = c.S2 + 0x1u;
        c.S3 = c.S3 + c.V1;
        c.V0 = c.V0 << 24;
        c.A0 = (uint)((int)c.V0 >> 24);
        c.V0 = c.A0 & 0xFFFFu;
        if (c.V0 != 0u) {
            c.S4 = c.S4 + c.V1;
            goto L8001F58C;
        }
        c.S4 = c.S4 + c.V1;
        L8001F628: ;
        c.V0 = c.S4 + 0u;
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
    public static void func_8001F65C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x30u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S2);
        c.S2 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S1);
        c.S1 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S3);
        c.S3 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S4);
        c.S4 = c.A3 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x40u));
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.RA);
        c.A0 = c.S1 + 0u;
        c.RA = 0x8001F694u;
        GranTurismo2PC.func_8001F740(c, m);
        c.A0 = c.S2 + 0u;
        c.A1 = c.S1 + 0u;
        c.A2 = c.S3 - c.V0;
        c.A3 = c.S4 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.RA = 0x8001F6ACu;
        GranTurismo2PC.func_8001F520(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x2Cu));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.SP = c.SP + 0x30u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001F6CC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.A0 = MemoryAccess.ReadU16(m, c.S0);
        if (c.A0 == 0u) {
            c.S0 = c.S0 + 0x2u;
            goto L8001F724;
        }
        c.S0 = c.S0 + 0x2u;
        c.V0 = 0x80050000u;
        c.S2 = c.V0 + 0x2100u;
        L8001F700: ;
        c.RA = 0x8001F708u;
        GranTurismo2PC.func_8001F2C8(c, m);
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.S2;
        c.A0 = MemoryAccess.ReadU16(m, c.S0);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x6u));
        c.S0 = c.S0 + 0x2u;
        if (c.A0 != 0u) {
            c.S1 = c.S1 + c.V0;
            goto L8001F700;
        }
        c.S1 = c.S1 + c.V0;
        L8001F724: ;
        c.V0 = c.S1 + 0u;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001F740(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.V0 = MemoryAccess.ReadU8(m, c.S0);
        c.V0 = c.V0 << 24;
        c.A0 = (uint)((int)c.V0 >> 24);
        c.V0 = c.A0 & 0xFFFFu;
        if (c.V0 == 0u) {
            c.S0 = c.S0 + 0x1u;
            goto L8001F7B0;
        }
        c.S0 = c.S0 + 0x1u;
        c.V0 = 0x80050000u;
        c.S2 = c.V0 + 0x2100u;
        L8001F780: ;
        c.A0 = c.A0 & 0xFFFFu;
        c.RA = 0x8001F788u;
        GranTurismo2PC.func_8001F2C8(c, m);
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.S2;
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x6u));
        c.V0 = MemoryAccess.ReadU8(m, c.S0);
        c.S0 = c.S0 + 0x1u;
        c.V0 = c.V0 << 24;
        c.A0 = (uint)((int)c.V0 >> 24);
        c.V0 = c.A0 & 0xFFFFu;
        if (c.V0 != 0u) {
            c.S1 = c.S1 + c.V1;
            goto L8001F780;
        }
        c.S1 = c.S1 + c.V1;
        L8001F7B0: ;
        c.V0 = c.S1 + 0u;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001F7CC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x38u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S5);
        c.S5 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        c.S4 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x34u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.A3);
        c.V1 = MemoryAccess.ReadU16(m, c.S2);
        if (c.V1 == 0u) {
            c.S2 = c.S2 + 0x2u;
            goto L8001F8C8;
        }
        c.S2 = c.S2 + 0x2u;
        c.V0 = 0x800C0000u;
        c.FP = c.V0 - 0x6EF8u;
        c.V0 = 0x80050000u;
        c.S7 = c.V0 + 0x2040u;
        c.V0 = 0x80050000u;
        c.S6 = c.V0 + 0x129Cu;
        L8001F834: ;
        c.V0 = c.V1 & 0x00FFu;
        c.V0 = c.V0 << 1;
        c.V0 = c.V0 + c.FP;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0x48u));
        c.S0 = (uint)(short)MemoryAccess.ReadU16(m, c.V0);
        c.A0 = c.S5 + 0u;
        c.RA = 0x8001F850u;
        GranTurismo2PC.func_80081478(c, m);
        c.S1 = c.S0 << 3;
        c.S1 = c.S1 + c.S7;
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x2u));
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x4u));
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x44u));
        c.A0 = c.S3 + c.A0;
        c.V1 = c.T0 + c.V1;
        c.V1 = c.V1 << 16;
        c.A0 = c.A0 + c.V1;
        c.V1 = c.S0 << 1;
        c.V1 = c.V1 + c.S0;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.S6;
        MemoryAccess.WriteU32(m, c.V0, c.A0);
        c.A0 = MemoryAccess.ReadU32(m, c.V1);
        MemoryAccess.WriteU32(m, (c.V0 + 0x4u), c.A0);
        c.A0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        MemoryAccess.WriteU32(m, (c.V0 + 0x8u), c.A0);
        c.A1 = MemoryAccess.ReadU16(m, (c.V1 + 0x8u));
        c.A0 = c.S5 + 0u;
        c.A1 = c.A1 | 0x0040u;
        c.RA = 0x8001F8B0u;
        GranTurismo2PC.func_8007DA44(c, m);
        c.V1 = MemoryAccess.ReadU16(m, c.S2);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x6u));
        c.S2 = c.S2 + 0x2u;
        c.S3 = c.S3 + c.V0;
        if (c.V1 != 0u) {
            c.S4 = c.S4 + c.V0;
            goto L8001F834;
        }
        c.S4 = c.S4 + c.V0;
        L8001F8C8: ;
        c.V0 = c.S4 + 0u;
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
    public static void func_8001F8FC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x30u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S2);
        c.S2 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S1);
        c.S1 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S3);
        c.S3 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S4);
        c.S4 = c.A3 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x40u));
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.RA);
        c.A0 = c.S1 + 0u;
        c.RA = 0x8001F934u;
        GranTurismo2PC.func_8001FB24(c, m);
        c.A0 = c.S2 + 0u;
        c.A1 = c.S1 + 0u;
        c.A2 = c.S3 - c.V0;
        c.A3 = c.S4 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.RA = 0x8001F94Cu;
        GranTurismo2PC.func_8001F7CC(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x2Cu));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.SP = c.SP + 0x30u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001F96C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x38u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S5);
        c.S5 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        c.S4 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x34u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.A3);
        c.V0 = MemoryAccess.ReadU8(m, c.S2);
        c.V0 = c.V0 << 24;
        c.V1 = (uint)((int)c.V0 >> 24);
        c.V0 = c.V1 & 0xFFFFu;
        if (c.V0 == 0u) {
            c.S2 = c.S2 + 0x1u;
            goto L8001FA80;
        }
        c.S2 = c.S2 + 0x1u;
        c.V0 = 0x800C0000u;
        c.FP = c.V0 - 0x6EF8u;
        c.V0 = 0x80050000u;
        c.S7 = c.V0 + 0x2040u;
        c.V0 = 0x80050000u;
        c.S6 = c.V0 + 0x129Cu;
        L8001F9E0: ;
        c.V0 = c.V1 & 0x00FFu;
        c.V0 = c.V0 << 1;
        c.V0 = c.V0 + c.FP;
        c.A1 = MemoryAccess.ReadU32(m, (c.SP + 0x48u));
        c.S0 = (uint)(short)MemoryAccess.ReadU16(m, c.V0);
        c.A0 = c.S5 + 0u;
        c.RA = 0x8001F9FCu;
        GranTurismo2PC.func_80081478(c, m);
        c.S1 = c.S0 << 3;
        c.S1 = c.S1 + c.S7;
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x2u));
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x4u));
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x44u));
        c.A0 = c.S3 + c.A0;
        c.V1 = c.T0 + c.V1;
        c.V1 = c.V1 << 16;
        c.A0 = c.A0 + c.V1;
        c.V1 = c.S0 << 1;
        c.V1 = c.V1 + c.S0;
        c.V1 = c.V1 << 2;
        c.V1 = c.V1 + c.S6;
        MemoryAccess.WriteU32(m, c.V0, c.A0);
        c.A0 = MemoryAccess.ReadU32(m, c.V1);
        MemoryAccess.WriteU32(m, (c.V0 + 0x4u), c.A0);
        c.A0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        MemoryAccess.WriteU32(m, (c.V0 + 0x8u), c.A0);
        c.A1 = MemoryAccess.ReadU16(m, (c.V1 + 0x8u));
        c.A0 = c.S5 + 0u;
        c.A1 = c.A1 | 0x0040u;
        c.RA = 0x8001FA5Cu;
        GranTurismo2PC.func_8007DA44(c, m);
        c.V1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x6u));
        c.V0 = MemoryAccess.ReadU8(m, c.S2);
        c.S3 = c.S3 + c.V1;
        c.S4 = c.S4 + c.V1;
        c.V0 = c.V0 << 24;
        c.V1 = (uint)((int)c.V0 >> 24);
        c.V0 = c.V1 & 0xFFFFu;
        if (c.V0 != 0u) {
            c.S2 = c.S2 + 0x1u;
            goto L8001F9E0;
        }
        c.S2 = c.S2 + 0x1u;
        L8001FA80: ;
        c.V0 = c.S4 + 0u;
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
    public static void func_8001FAB4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x30u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S2);
        c.S2 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S1);
        c.S1 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S3);
        c.S3 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S4);
        c.S4 = c.A3 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x40u));
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.RA);
        c.A0 = c.S1 + 0u;
        c.RA = 0x8001FAECu;
        GranTurismo2PC.func_8001FB7C(c, m);
        c.A0 = c.S2 + 0u;
        c.A1 = c.S1 + 0u;
        c.A2 = c.S3 - c.V0;
        c.A3 = c.S4 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.RA = 0x8001FB04u;
        GranTurismo2PC.func_8001F96C(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x2Cu));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x28u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.SP = c.SP + 0x30u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001FB24(CpuContext c, IMemory m)
    {
        c.A1 = 0u + 0u;
        c.V1 = MemoryAccess.ReadU16(m, c.A0);
        if (c.V1 == 0u) {
            c.A0 = c.A0 + 0x2u;
            goto L8001FB74;
        }
        c.A0 = c.A0 + 0x2u;
        c.V0 = 0x800C0000u;
        c.A3 = c.V0 - 0x6EF8u;
        c.V0 = 0x80050000u;
        c.A2 = c.V0 + 0x2040u;
        L8001FB48: ;
        c.V0 = c.V1 & 0x00FFu;
        c.V0 = c.V0 << 1;
        c.V0 = c.V0 + c.A3;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.V0);
        c.V1 = MemoryAccess.ReadU16(m, c.A0);
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.A2;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x6u));
        c.A0 = c.A0 + 0x2u;
        if (c.V1 != 0u) {
            c.A1 = c.A1 + c.V0;
            goto L8001FB48;
        }
        c.A1 = c.A1 + c.V0;
        L8001FB74: ;
        c.V0 = c.A1 + 0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001FB7C(CpuContext c, IMemory m)
    {
        c.A1 = 0u + 0u;
        c.V0 = MemoryAccess.ReadU8(m, c.A0);
        c.V0 = c.V0 << 24;
        c.V1 = (uint)((int)c.V0 >> 24);
        c.V0 = c.V1 & 0xFFFFu;
        if (c.V0 == 0u) {
            c.A0 = c.A0 + 0x1u;
            goto L8001FBE4;
        }
        c.A0 = c.A0 + 0x1u;
        c.V0 = 0x800C0000u;
        c.A3 = c.V0 - 0x6EF8u;
        c.V0 = 0x80050000u;
        c.A2 = c.V0 + 0x2040u;
        L8001FBAC: ;
        c.V0 = c.V1 & 0x00FFu;
        c.V0 = c.V0 << 1;
        c.V0 = c.V0 + c.A3;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.V0);
        c.V1 = MemoryAccess.ReadU8(m, c.A0);
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.A2;
        c.V1 = c.V1 << 24;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x6u));
        c.V1 = (uint)((int)c.V1 >> 24);
        c.A1 = c.A1 + c.V0;
        c.V0 = c.V1 & 0xFFFFu;
        if (c.V0 != 0u) {
            c.A0 = c.A0 + 0x1u;
            goto L8001FBAC;
        }
        c.A0 = c.A0 + 0x1u;
        L8001FBE4: ;
        c.V0 = c.A1 + 0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001FBEC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x34u));
        c.V1 = MemoryAccess.ReadU32(m, (c.SP + 0x30u));
        if (c.V0 == 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
            goto L8001FC10;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V1);
        c.RA = 0x8001FC08u;
        GranTurismo2PC.func_8001F7CC(c, m);
        goto L8001FC18;
        L8001FC10: ;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V1);
        c.RA = 0x8001FC18u;
        GranTurismo2PC.func_8001F38C(c, m);
        L8001FC18: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001FC28(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x34u));
        c.V1 = MemoryAccess.ReadU32(m, (c.SP + 0x30u));
        if (c.V0 == 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
            goto L8001FC4C;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V1);
        c.RA = 0x8001FC44u;
        GranTurismo2PC.func_8001F8FC(c, m);
        goto L8001FC54;
        L8001FC4C: ;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V1);
        c.RA = 0x8001FC54u;
        GranTurismo2PC.func_8001F4B0(c, m);
        L8001FC54: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001FC64(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x34u));
        c.V1 = MemoryAccess.ReadU32(m, (c.SP + 0x30u));
        if (c.V0 == 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
            goto L8001FC88;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V1);
        c.RA = 0x8001FC80u;
        GranTurismo2PC.func_8001F96C(c, m);
        goto L8001FC90;
        L8001FC88: ;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V1);
        c.RA = 0x8001FC90u;
        GranTurismo2PC.func_8001F520(c, m);
        L8001FC90: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001FCA0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        c.V0 = MemoryAccess.ReadU32(m, (c.SP + 0x34u));
        c.V1 = MemoryAccess.ReadU32(m, (c.SP + 0x30u));
        if (c.V0 == 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
            goto L8001FCC4;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V1);
        c.RA = 0x8001FCBCu;
        GranTurismo2PC.func_8001FAB4(c, m);
        goto L8001FCCC;
        L8001FCC4: ;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V1);
        c.RA = 0x8001FCCCu;
        GranTurismo2PC.func_8001F65C(c, m);
        L8001FCCC: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001FCDC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x40u;
        c.A3 = 0x00980000u;
        c.A3 = c.A3 | 0x9680u;
        if (c.A1 != 0u) {
            c.T1 = 0u + 0u;
            goto L8001FD00;
        }
        c.T1 = 0u + 0u;
        c.V0 = 0x00000030u;
        MemoryAccess.WriteU16(m, c.A0, (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.A0 + 0x2u), (ushort)0u);
        goto L8001FE04;
        L8001FD00: ;
        c.T0 = 0u + 0u;
        c.T3 = 0x00000001u;
        c.T2 = 0x66660000u;
        c.T2 = c.T2 | 0x6667u;
        L8001FD10: ;
        if (c.A3 != 0u) { c.LO = c.A1 / c.A3; c.HI = c.A1 % c.A3; }
        c.A2 = c.LO;
        c.V0 = 0u < c.A2 ? 1u : 0u;
        c.V0 = c.V0 | c.T1;
        if (c.V0 == 0u) {
            { var _r = (long)(int)c.A2 * (int)c.A3; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
            goto L8001FD44;
        }
        { var _r = (long)(int)c.A2 * (int)c.A3; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V1 = c.SP + c.T0;
        c.V0 = c.A2 + 0x30u;
        MemoryAccess.WriteU8(m, c.V1, (byte)c.V0);
        c.T1 = 0x00000001u;
        c.T0 = c.T0 + c.T1;
        c.T4 = c.LO;
        c.A1 = c.A1 - c.T4;
        L8001FD44: ;
        if (c.A3 == c.T3) {
            { var _r = (long)(int)c.A3 * (int)c.T2; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
            goto L8001FD60;
        }
        { var _r = (long)(int)c.A3 * (int)c.T2; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V0 = (uint)((int)c.A3 >> 31);
        c.T4 = c.HI;
        c.V1 = (uint)((int)c.T4 >> 2);
        c.A3 = c.V1 - c.V0;
        goto L8001FD10;
        L8001FD60: ;
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
            goto L8001FD98;
        }
        c.V1 = 0x00000003u;
        L8001FD98: ;
        c.V0 = MemoryAccess.ReadU8(m, c.SP);
        c.V0 = c.V0 << 24;
        c.V0 = (uint)((int)c.V0 >> 24);
        MemoryAccess.WriteU16(m, c.A0, (ushort)c.V0);
        c.V0 = MemoryAccess.ReadU8(m, (c.SP + 0x1u));
        c.T0 = 0u + 0u;
        goto L8001FDF8;
        L8001FDB8: ;
        c.V1 = c.V1 - 0x1u;
        if (c.V1 != 0u) {
            c.V0 = 0x0000002Cu;
            goto L8001FDD0;
        }
        c.V0 = 0x0000002Cu;
        MemoryAccess.WriteU16(m, c.A0, (ushort)c.V0);
        c.A0 = c.A0 + 0x2u;
        c.V1 = 0x00000003u;
        L8001FDD0: ;
        c.T0 = c.T0 + 0x1u;
        c.V0 = c.SP + c.T0;
        c.V0 = MemoryAccess.ReadU8(m, c.V0);
        c.V0 = c.V0 << 24;
        c.V0 = (uint)((int)c.V0 >> 24);
        MemoryAccess.WriteU16(m, c.A0, (ushort)c.V0);
        c.V0 = c.T0 + c.SP;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V0 + 0x1u));
        L8001FDF8: ;
        if (c.V0 != 0u) {
            c.A0 = c.A0 + 0x2u;
            goto L8001FDB8;
        }
        c.A0 = c.A0 + 0x2u;
        MemoryAccess.WriteU16(m, c.A0, (ushort)0u);
        L8001FE04: ;
        c.SP = c.SP + 0x40u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001FE0C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x98u;
        MemoryAccess.WriteU32(m, (c.SP + 0x90u), c.RA);
        c.T0 = 0x00980000u;
        c.T0 = c.T0 | 0x9680u;
        c.V0 = c.A1 < 0x00000001u ? 1u : 0u;
        c.V1 = c.A2 < 0x00000001u ? 1u : 0u;
        c.V0 = c.V0 & c.V1;
        if (c.V0 == 0u) {
            c.T2 = 0u + 0u;
            goto L8001FE40;
        }
        c.T2 = 0u + 0u;
        c.V0 = 0x00000030u;
        MemoryAccess.WriteU16(m, c.A0, (ushort)c.V0);
        MemoryAccess.WriteU16(m, (c.A0 + 0x2u), (ushort)0u);
        goto L8001FFBC;
        L8001FE40: ;
        if (c.A1 != 0u) {
            c.T1 = 0u + 0u;
            goto L8001FE58;
        }
        c.T1 = 0u + 0u;
        c.A1 = c.A2 + 0u;
        c.RA = 0x8001FE50u;
        GranTurismo2PC.func_8001FCDC(c, m);
        goto L8001FFBC;
        L8001FE58: ;
        c.T4 = c.SP + 0x10u;
        c.T5 = 0x00000001u;
        c.T3 = 0x66660000u;
        c.T3 = c.T3 | 0x6667u;
        L8001FE68: ;
        if (c.T0 != 0u) { c.LO = c.A1 / c.T0; c.HI = c.A1 % c.T0; }
        c.A3 = c.LO;
        c.V0 = 0u < c.A3 ? 1u : 0u;
        c.V0 = c.V0 | c.T2;
        if (c.V0 == 0u) {
            { var _r = (long)(int)c.A3 * (int)c.T0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
            goto L8001FE9C;
        }
        { var _r = (long)(int)c.A3 * (int)c.T0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V1 = c.T4 + c.T1;
        c.V0 = c.A3 + 0x30u;
        MemoryAccess.WriteU8(m, c.V1, (byte)c.V0);
        c.T2 = 0x00000001u;
        c.T1 = c.T1 + c.T2;
        c.T6 = c.LO;
        c.A1 = c.A1 - c.T6;
        L8001FE9C: ;
        if (c.T0 == c.T5) {
            { var _r = (long)(int)c.T0 * (int)c.T3; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
            goto L8001FEB8;
        }
        { var _r = (long)(int)c.T0 * (int)c.T3; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V0 = (uint)((int)c.T0 >> 31);
        c.T6 = c.HI;
        c.V1 = (uint)((int)c.T6 >> 2);
        c.T0 = c.V1 - c.V0;
        goto L8001FE68;
        L8001FEB8: ;
        c.T0 = 0x00980000u;
        c.T0 = c.T0 | 0x9680u;
        c.A1 = c.SP + 0x10u;
        c.T3 = 0x00000001u;
        c.T2 = 0x66660000u;
        c.T2 = c.T2 | 0x6667u;
        L8001FED0: ;
        if (c.T0 != 0u) { c.LO = c.A2 / c.T0; c.HI = c.A2 % c.T0; }
        c.A3 = c.LO;
        { var _r = (long)(int)c.A3 * (int)c.T0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V1 = c.A1 + c.T1;
        c.T1 = c.T1 + 0x1u;
        c.V0 = c.A3 + 0x30u;
        MemoryAccess.WriteU8(m, c.V1, (byte)c.V0);
        c.T4 = c.LO;
        if (c.T0 == c.T3) {
            c.A2 = c.A2 - c.T4;
            goto L8001FF18;
        }
        c.A2 = c.A2 - c.T4;
        { var _r = (long)(int)c.T0 * (int)c.T2; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V0 = (uint)((int)c.T0 >> 31);
        c.T6 = c.HI;
        c.V1 = (uint)((int)c.T6 >> 2);
        c.T0 = c.V1 - c.V0;
        goto L8001FED0;
        L8001FF18: ;
        c.V0 = 0x55550000u;
        c.V0 = c.V0 | 0x5556u;
        { var _r = (long)(int)c.T1 * (int)c.V0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.V0 = c.A1 + c.T1;
        MemoryAccess.WriteU8(m, c.V0, (byte)0u);
        c.V0 = (uint)((int)c.T1 >> 31);
        c.T6 = c.HI;
        c.A1 = c.T6 - c.V0;
        c.V0 = c.A1 << 1;
        c.V0 = c.V0 + c.A1;
        c.A1 = c.T1 - c.V0;
        if (c.A1 != 0u) {
            goto L8001FF50;
        }
        c.A1 = 0x00000003u;
        L8001FF50: ;
        c.V0 = MemoryAccess.ReadU8(m, (c.SP + 0x10u));
        c.V0 = c.V0 << 24;
        c.V0 = (uint)((int)c.V0 >> 24);
        MemoryAccess.WriteU16(m, c.A0, (ushort)c.V0);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.SP + 0x11u));
        c.T1 = 0u + 0u;
        goto L8001FFB0;
        L8001FF70: ;
        c.A1 = c.A1 - 0x1u;
        if (c.A1 != 0u) {
            c.V0 = 0x0000002Cu;
            goto L8001FF88;
        }
        c.V0 = 0x0000002Cu;
        MemoryAccess.WriteU16(m, c.A0, (ushort)c.V0);
        c.A0 = c.A0 + 0x2u;
        c.A1 = 0x00000003u;
        L8001FF88: ;
        c.T1 = c.T1 + 0x1u;
        c.V1 = c.SP + 0x10u;
        c.V0 = c.V1 + c.T1;
        c.V0 = MemoryAccess.ReadU8(m, c.V0);
        c.V1 = c.T1 + c.V1;
        c.V0 = c.V0 << 24;
        c.V0 = (uint)((int)c.V0 >> 24);
        MemoryAccess.WriteU16(m, c.A0, (ushort)c.V0);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V1 + 0x1u));
        L8001FFB0: ;
        if (c.V0 != 0u) {
            c.A0 = c.A0 + 0x2u;
            goto L8001FF70;
        }
        c.A0 = c.A0 + 0x2u;
        MemoryAccess.WriteU16(m, c.A0, (ushort)0u);
        L8001FFBC: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x90u));
        c.SP = c.SP + 0x98u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8001FFCC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x58u;
        MemoryAccess.WriteU32(m, (c.SP + 0x50u), c.S0);
        c.S0 = c.A0 + 0u;
        c.A2 = c.A1 + 0u;
        c.A1 = 0x801F0000u;
        c.A0 = c.SP + 0x10u;
        MemoryAccess.WriteU32(m, (c.SP + 0x54u), c.RA);
        c.A1 = c.A1 - 0x93Au;
        c.RA = 0x8001FFF0u;
        GranTurismo2PC.func_8008CF34(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.SP + 0x10u));
        c.A0 = 0u + 0u;
        c.V0 = c.V0 << 24;
        c.V0 = (uint)((int)c.V0 >> 24);
        MemoryAccess.WriteU16(m, c.S0, (ushort)c.V0);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.SP + 0x10u));
        if (c.V0 == 0u) {
            c.S0 = c.S0 + 0x2u;
            goto L80020044;
        }
        c.S0 = c.S0 + 0x2u;
        c.A1 = c.SP + 0x10u;
        L80020018: ;
        c.A0 = c.A0 + 0x1u;
        c.V0 = c.A1 + c.A0;
        c.V1 = MemoryAccess.ReadU8(m, c.V0);
        c.V1 = c.V1 << 24;
        c.V1 = (uint)((int)c.V1 >> 24);
        MemoryAccess.WriteU16(m, c.S0, (ushort)c.V1);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, c.V0);
        if (c.V0 != 0u) {
            c.S0 = c.S0 + 0x2u;
            goto L80020018;
        }
        c.S0 = c.S0 + 0x2u;
        L80020044: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x54u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x50u));
        c.SP = c.SP + 0x58u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020054(CpuContext c, IMemory m)
    {
        c.A3 = c.A1 + 0u;
        c.V0 = 0x51EB0000u;
        c.V0 = c.V0 | 0x851Fu;
        { var _r = (long)(int)c.A3 * (int)c.V0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.SP = c.SP - 0x58u;
        MemoryAccess.WriteU32(m, (c.SP + 0x50u), c.S0);
        c.S0 = c.A0 + 0u;
        c.A1 = 0x801C0000u;
        c.V0 = (uint)((int)c.A3 >> 31);
        c.A0 = c.SP + 0x10u;
        c.A1 = c.A1 + 0x3108u;
        MemoryAccess.WriteU32(m, (c.SP + 0x54u), c.RA);
        c.T0 = c.HI;
        c.A2 = (uint)((int)c.T0 >> 5);
        c.A2 = c.A2 - c.V0;
        c.V0 = c.A2 << 1;
        c.V0 = c.V0 + c.A2;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.A2;
        c.V0 = c.V0 << 2;
        c.A3 = c.A3 - c.V0;
        c.RA = 0x800200ACu;
        GranTurismo2PC.func_8008CF34(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.SP + 0x10u));
        c.A0 = 0u + 0u;
        c.V0 = c.V0 << 24;
        c.V0 = (uint)((int)c.V0 >> 24);
        MemoryAccess.WriteU16(m, c.S0, (ushort)c.V0);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.SP + 0x10u));
        if (c.V0 == 0u) {
            c.S0 = c.S0 + 0x2u;
            goto L80020100;
        }
        c.S0 = c.S0 + 0x2u;
        c.A1 = c.SP + 0x10u;
        L800200D4: ;
        c.A0 = c.A0 + 0x1u;
        c.V0 = c.A1 + c.A0;
        c.V1 = MemoryAccess.ReadU8(m, c.V0);
        c.V1 = c.V1 << 24;
        c.V1 = (uint)((int)c.V1 >> 24);
        MemoryAccess.WriteU16(m, c.S0, (ushort)c.V1);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, c.V0);
        if (c.V0 != 0u) {
            c.S0 = c.S0 + 0x2u;
            goto L800200D4;
        }
        c.S0 = c.S0 + 0x2u;
        L80020100: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x54u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x50u));
        c.SP = c.SP + 0x58u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020110_gt2_overlay_4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x58u;
        MemoryAccess.WriteU32(m, (c.SP + 0x50u), c.S0);
        c.S0 = c.A0 + 0u;
        c.A2 = c.A1 + 0u;
        c.A1 = 0x80020000u;
        c.A0 = c.SP + 0x10u;
        MemoryAccess.WriteU32(m, (c.SP + 0x54u), c.RA);
        c.A1 = c.A1 + 0x42A4u;
        c.RA = 0x80020134u;
        GranTurismo2PC.func_8008CF34(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.SP + 0x10u));
        c.A0 = 0u + 0u;
        c.V0 = c.V0 << 24;
        c.V0 = (uint)((int)c.V0 >> 24);
        MemoryAccess.WriteU16(m, c.S0, (ushort)c.V0);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.SP + 0x10u));
        if (c.V0 == 0u) {
            c.S0 = c.S0 + 0x2u;
            goto L80020188;
        }
        c.S0 = c.S0 + 0x2u;
        c.A1 = c.SP + 0x10u;
        L8002015C: ;
        c.A0 = c.A0 + 0x1u;
        c.V0 = c.A1 + c.A0;
        c.V1 = MemoryAccess.ReadU8(m, c.V0);
        c.V1 = c.V1 << 24;
        c.V1 = (uint)((int)c.V1 >> 24);
        MemoryAccess.WriteU16(m, c.S0, (ushort)c.V1);
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, c.V0);
        if (c.V0 != 0u) {
            c.S0 = c.S0 + 0x2u;
            goto L8002015C;
        }
        c.S0 = c.S0 + 0x2u;
        L80020188: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x54u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x50u));
        c.SP = c.SP + 0x58u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020198(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x58u;
        c.V0 = 0x00000004u;
        MemoryAccess.WriteU32(m, (c.SP + 0x54u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x50u), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0x4Cu), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x48u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x44u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x3Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x38u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x34u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x60u), c.A2);
        c.S5 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0xEu));
        c.V1 = MemoryAccess.ReadU32(m, c.A1);
        c.FP = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0x10u));
        c.S6 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.S7 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0xCu));
        if (c.A0 == c.V0) {
            c.S5 = c.S5 + 0x4u;
            goto L800201FC;
        }
        c.S5 = c.S5 + 0x4u;
        c.V0 = 0x00000006u;
        if (c.A0 == c.V0) {
            c.V0 = 0x800C0000u;
            goto L80020458;
        }
        c.V0 = 0x800C0000u;
        goto L8002045C;
        L800201FC: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V1 + 0x6u));
        c.V1 = 0x800C0000u;
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x60u));
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V1 - 0x6AF8u));
        c.V0 = c.T0 ^ c.V0;
        c.V0 = c.V0 < 0x00000001u ? 1u : 0u;
        c.V1 = ~(0u | c.A0);
        c.V1 = c.V1 >> 31;
        c.V0 = c.V0 & c.V1;
        if (c.V0 == 0u) {
            c.A3 = 0x00140000u;
            goto L8002029C;
        }
        c.A3 = 0x00140000u;
        c.A3 = c.A3 | 0x1414u;
        c.A2 = 0x00500000u;
        c.A2 = c.A2 | 0x5050u;
        c.V0 = 0x88880000u;
        c.V0 = c.V0 | 0x8889u;
        c.V1 = c.A0 << 7;
        { var _r = (long)(int)c.V1 * (int)c.V0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.A0 = c.S6 + 0u;
        c.A1 = c.SP + 0x18u;
        c.V0 = c.S5 - 0xEu;
        MemoryAccess.WriteU16(m, (c.SP + 0x1Au), (ushort)c.V0);
        c.V0 = 0x00000180u;
        MemoryAccess.WriteU16(m, (c.SP + 0x1Cu), (ushort)c.V0);
        c.V0 = 0x00000014u;
        MemoryAccess.WriteU16(m, (c.SP + 0x18u), (ushort)c.S7);
        MemoryAccess.WriteU16(m, (c.SP + 0x1Eu), (ushort)c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.A3);
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.A2);
        c.T0 = c.HI;
        c.A2 = c.T0 + c.V1;
        c.A2 = (uint)((int)c.A2 >> 5);
        c.V1 = (uint)((int)c.V1 >> 31);
        c.A2 = c.A2 - c.V1;
        c.RA = 0x80020288u;
        GranTurismo2PC.func_8006B814(c, m);
        c.A0 = c.S6 + 0u;
        c.A1 = 0x00000220u;
        c.RA = 0x80020294u;
        GranTurismo2PC.func_8007DA44(c, m);
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x60u));
        L8002029C: ;
        c.S1 = c.T0 << 2;
        c.S1 = c.S1 + c.T0;
        c.S1 = c.S1 << 3;
        c.S1 = c.S1 + c.T0;
        c.S1 = c.S1 << 2;
        c.S1 = c.S1 + 0x4u;
        c.T0 = 0x801D0000u;
        c.T0 = c.T0 - 0x2AACu;
        c.S1 = c.S1 + c.T0;
        c.A0 = MemoryAccess.ReadU16(m, (c.S1 + 0x98u));
        c.S0 = MemoryAccess.ReadU32(m, c.S1);
        c.A0 = c.A0 >> 15;
        c.RA = 0x800202D0u;
        GranTurismo2PC.func_8001828C(c, m);
        c.A0 = c.S0 + 0u;
        c.S2 = c.V0 + 0u;
        c.RA = 0x800202DCu;
        GranTurismo2PC.func_800182A8(c, m);
        c.A0 = c.S0 + 0u;
        c.S4 = c.V0 + 0u;
        c.RA = 0x800202E8u;
        Dispatcher.Call(c, m, 0x800182FCu);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 + 0x298Cu;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 + 0x2990u;
        c.A2 = c.FP + 0u;
        c.A3 = 0x00000080u;
        c.S3 = c.V0 + 0u;
        c.RA = 0x80020308u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S6 + 0u;
        c.A1 = c.S2 + 0u;
        c.S0 = c.S7 - 0x98u;
        c.A2 = c.S0 + 0u;
        c.A3 = c.S5 + 0u;
        c.S2 = c.V0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S2);
        c.RA = 0x80020328u;
        GranTurismo2PC.func_8001F38C(c, m);
        c.A0 = c.S6 + 0u;
        c.A1 = c.S4 + 0u;
        c.S0 = c.S0 + c.V0;
        c.A2 = c.S0 + 0u;
        c.A3 = c.S5 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S2);
        c.RA = 0x80020344u;
        GranTurismo2PC.func_8001F38C(c, m);
        c.A0 = c.S6 + 0u;
        c.A1 = c.S3 + 0u;
        c.S0 = c.S0 + c.V0;
        c.A2 = c.S0 + 0x5u;
        c.A3 = c.S5 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S2);
        c.RA = 0x80020360u;
        GranTurismo2PC.func_8001F38C(c, m);
        c.V0 = c.S7 - 0xA5u;
        MemoryAccess.WriteU16(m, (c.SP + 0x18u), (ushort)c.V0);
        c.V0 = c.S5 - 0x9u;
        MemoryAccess.WriteU16(m, (c.SP + 0x1Au), (ushort)c.V0);
        c.V0 = 0x00000009u;
        MemoryAccess.WriteU16(m, (c.SP + 0x1Cu), (ushort)c.V0);
        c.V0 = 0x0000000Cu;
        MemoryAccess.WriteU16(m, (c.SP + 0x1Eu), (ushort)c.V0);
        c.A0 = MemoryAccess.ReadU32(m, (c.S1 + 0x8Cu));
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x4u));
        c.RA = 0x80020390u;
        GranTurismo2PC.func_80060D28(c, m);
        c.A0 = c.S6 + 0u;
        c.A1 = c.SP + 0x18u;
        c.A2 = c.FP + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.V0);
        c.RA = 0x800203A4u;
        GranTurismo2PC.func_8006BB08(c, m);
        c.T0 = 0x801D0000u;
        c.T0 = c.T0 - 0x2AACu;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.T0 + 0x4018u));
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x60u));
        if (c.V0 != c.T0) {
            c.A1 = 0x80050000u;
            goto L80020404;
        }
        c.A1 = 0x80050000u;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 + 0x298Cu;
        c.A1 = c.A1 + 0x2998u;
        c.A2 = c.FP + 0u;
        c.A3 = 0x00000080u;
        c.RA = 0x800203D8u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S6 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x800203E4u;
        GranTurismo2PC.func_8007D024(c, m);
        c.V1 = c.S7 - 0xB8u;
        MemoryAccess.WriteU16(m, c.V0, (ushort)c.V1);
        c.V1 = c.S5 - 0x8u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x2u), (ushort)c.V1);
        c.V1 = 0x0000000Eu;
        MemoryAccess.WriteU16(m, (c.V0 + 0x4u), (ushort)c.V1);
        c.V1 = 0x00000008u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x6u), (ushort)c.V1);
        L80020404: ;
        c.A0 = 0x80050000u;
        c.A0 = c.A0 + 0x298Cu;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 + 0x2994u;
        c.A2 = c.FP + 0u;
        c.A3 = 0x00000080u;
        c.V0 = c.S7 - 0xC0u;
        MemoryAccess.WriteU16(m, (c.SP + 0x28u), (ushort)c.V0);
        c.V0 = 0x00000180u;
        MemoryAccess.WriteU16(m, (c.SP + 0x2Cu), (ushort)c.V0);
        c.V0 = c.S5 - 0xEu;
        MemoryAccess.WriteU16(m, (c.SP + 0x2Au), (ushort)c.V0);
        c.V0 = 0x00000014u;
        MemoryAccess.WriteU16(m, (c.SP + 0x2Eu), (ushort)c.V0);
        c.RA = 0x80020440u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.S6 + 0u;
        c.A1 = c.V0 + 0u;
        c.A2 = c.SP + 0x28u;
        c.RA = 0x80020450u;
        GranTurismo2PC.func_8007E780(c, m);
        goto L8002045C;
        L80020458: ;
        MemoryAccess.WriteU16(m, (c.V0 - 0x6AF8u), (ushort)0u);
        L8002045C: ;
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
        c.V0 = 0x00000001u;
        c.SP = c.SP + 0x58u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020490(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        c.A0 = 0x80050000u;
        c.A1 = 0x80020000u;
        c.A0 = c.A0 + 0x2958u;
        c.A1 = c.A1 + 0x198u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.A2 = 0u + 0u;
        c.RA = 0x800204B8u;
        GranTurismo2PC.func_8006CDCC(c, m);
        c.V1 = 0x800C0000u;
        MemoryAccess.WriteU16(m, c.S0, (ushort)0u);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU16(m, (c.V1 - 0x6AF8u), (ushort)c.V0);
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800204D8(CpuContext c, IMemory m)
    {
        c.V0 = 0x80050000u;
        c.V0 = c.V0 + 0x2958u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x10u), (ushort)c.A1);
        MemoryAccess.WriteU16(m, (c.V0 + 0x12u), (ushort)c.A2);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800204EC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        c.V0 = 0x801D0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 - 0x2AACu));
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        if ((int)c.S0 <= 0) {
            MemoryAccess.WriteU32(m, (c.S2 + 0x4u), c.A1);
            goto L80020540;
        }
        MemoryAccess.WriteU32(m, (c.S2 + 0x4u), c.A1);
        c.V0 = 0x80050000u;
        c.S1 = c.V0 + 0x2958u;
        c.A0 = c.S1 + 0u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x2958u), (ushort)c.S0);
        c.RA = 0x80020528u;
        GranTurismo2PC.func_8006CE70(c, m);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x6u));
        c.V0 = (int)c.V0 < (int)c.S0 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = c.S0 - 0x1u;
            goto L80020540;
        }
        c.V0 = c.S0 - 0x1u;
        MemoryAccess.WriteU16(m, (c.S1 + 0x6u), (ushort)c.V0);
        L80020540: ;
        MemoryAccess.WriteU16(m, c.S2, (ushort)c.S0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8002055C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        if (c.A2 == 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
            goto L800205B4;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.V1 = 0x800C0000u;
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 - 0x6AF8u));
        c.V0 = c.V0 + 0x1u;
        MemoryAccess.WriteU16(m, (c.V1 - 0x6AF8u), (ushort)c.V0);
        c.V0 = c.V0 << 16;
        c.V0 = (uint)((int)c.V0 >> 16);
        c.V0 = (int)c.V0 < 61 ? 1u : 0u;
        if (c.V0 != 0u) {
            goto L800205BC;
        }
        MemoryAccess.WriteU16(m, (c.V1 - 0x6AF8u), (ushort)0u);
        goto L800205BC;
        L800205B4: ;
        c.V0 = 0x800C0000u;
        MemoryAccess.WriteU16(m, (c.V0 - 0x6AF8u), (ushort)c.S0);
        L800205BC: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.S3);
        if ((int)c.V0 <= 0) {
            c.V0 = 0x80050000u;
            goto L8002066C;
        }
        c.V0 = 0x80050000u;
        c.S1 = c.V0 + 0x2958u;
        c.A0 = c.S1 + 0u;
        c.A1 = c.S2 + 0u;
        c.RA = 0x800205DCu;
        GranTurismo2PC.func_8006CFC4(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = 0xFFFFFFFDu;
        if (c.V1 == c.V0) {
            c.V0 = (int)c.V1 < -2 ? 1u : 0u;
            goto L80020620;
        }
        c.V0 = (int)c.V1 < -2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0xFFFFFFFCu;
            goto L80020604;
        }
        c.V0 = 0xFFFFFFFCu;
        if (c.V1 == c.V0) {
            c.V0 = c.S0 + 0u;
            goto L80020670;
        }
        c.V0 = c.S0 + 0u;
        MemoryAccess.WriteU16(m, (c.S3 + 0x2u), (ushort)c.V1);
        goto L80020668;
        L80020604: ;
        c.V0 = 0xFFFFFFFEu;
        if (c.V1 == c.V0) {
            c.V0 = 0xFFFFFFFFu;
            goto L80020630;
        }
        c.V0 = 0xFFFFFFFFu;
        if (c.V1 == c.V0) {
            c.S0 = 0xFFFFFFFEu;
            goto L8002066C;
        }
        c.S0 = 0xFFFFFFFEu;
        MemoryAccess.WriteU16(m, (c.S3 + 0x2u), (ushort)c.V1);
        goto L80020668;
        L80020620: ;
        c.A0 = 0x00000006u;
        c.RA = 0x80020628u;
        GranTurismo2PC.func_80060840(c, m);
        c.V0 = c.S0 + 0u;
        goto L80020670;
        L80020630: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S2 + 0x4u));
        c.V1 = 0x00010000u;
        c.V0 = c.V0 & c.V1;
        if (c.V0 == 0u) {
            c.A0 = 0x801D0000u;
            goto L8002066C;
        }
        c.A0 = 0x801D0000u;
        c.A0 = c.A0 - 0x6720u;
        c.A0 = c.A0 + 0x3C74u;
        c.A1 = (uint)(short)MemoryAccess.ReadU16(m, (c.S1 + 0x6u));
        c.A2 = 0u + 0u;
        c.RA = 0x80020658u;
        GranTurismo2PC.func_8001EF10(c, m);
        c.A0 = 0x00000001u;
        c.RA = 0x80020660u;
        GranTurismo2PC.func_80060840(c, m);
        c.V0 = c.S0 + 0u;
        goto L80020670;
        L80020668: ;
        c.S0 = c.V1 + 0u;
        L8002066C: ;
        c.V0 = c.S0 + 0u;
        L80020670: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8002068C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.A0);
        if ((int)c.V0 > 0) {
            c.A0 = 0x80050000u;
            goto L800206E0;
        }
        c.A0 = 0x80050000u;
        c.V0 = 0x00F00000u;
        c.V0 = c.V0 | 0x780Au;
        c.A0 = c.A1 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 + 0x30C0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.V0 = 0x80050000u;
        c.V0 = c.V0 + 0x2958u;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x10u));
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x12u));
        c.A2 = c.A2 - 0x98u;
        c.A3 = c.A3 + 0x40u;
        c.RA = 0x800206D8u;
        GranTurismo2PC.func_8001F520(c, m);
        goto L800206E8;
        L800206E0: ;
        c.A0 = c.A0 + 0x2958u;
        c.RA = 0x800206E8u;
        GranTurismo2PC.func_8006D50C(c, m);
        L800206E8: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800206F8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0xD8u;
        MemoryAccess.WriteU32(m, (c.SP + 0xB0u), c.S0);
        c.S0 = c.A2 + 0u;
        c.V0 = 0x00000004u;
        MemoryAccess.WriteU32(m, (c.SP + 0xD4u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0xD0u), c.FP);
        MemoryAccess.WriteU32(m, (c.SP + 0xCCu), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0xC8u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0xC4u), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0xC0u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0xBCu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0xB8u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0xB4u), c.S1);
        c.S7 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0xEu));
        c.V1 = MemoryAccess.ReadU32(m, c.A1);
        c.T0 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0x10u));
        MemoryAccess.WriteU32(m, (c.SP + 0xA8u), c.T0);
        c.FP = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.S2 = (uint)(short)MemoryAccess.ReadU16(m, (c.A1 + 0xCu));
        if (c.A0 == c.V0) {
            c.S7 = c.S7 + 0x4u;
            goto L80020764;
        }
        c.S7 = c.S7 + 0x4u;
        c.V0 = 0x00000006u;
        if (c.A0 == c.V0) {
            c.V0 = 0x800C0000u;
            goto L80020978;
        }
        c.V0 = 0x800C0000u;
        goto L8002097C;
        L80020764: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V1 + 0x6u));
        c.V1 = 0x800C0000u;
        c.A0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V1 - 0x6AF4u));
        c.V0 = c.S0 ^ c.V0;
        c.V0 = c.V0 < 0x00000001u ? 1u : 0u;
        c.V1 = ~(0u | c.A0);
        c.V1 = c.V1 >> 31;
        c.V0 = c.V0 & c.V1;
        if (c.V0 == 0u) {
            c.A3 = 0x00140000u;
            goto L800207F8;
        }
        c.A3 = 0x00140000u;
        c.A3 = c.A3 | 0x1414u;
        c.A2 = 0x00500000u;
        c.A2 = c.A2 | 0x5050u;
        c.V0 = 0x88880000u;
        c.V0 = c.V0 | 0x8889u;
        c.V1 = c.A0 << 7;
        { var _r = (long)(int)c.V1 * (int)c.V0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.A0 = c.FP + 0u;
        c.A1 = c.SP + 0x18u;
        c.V0 = c.S7 - 0x10u;
        MemoryAccess.WriteU16(m, (c.SP + 0x1Au), (ushort)c.V0);
        c.V0 = 0x000001A0u;
        MemoryAccess.WriteU16(m, (c.SP + 0x1Cu), (ushort)c.V0);
        c.V0 = 0x00000018u;
        MemoryAccess.WriteU16(m, (c.SP + 0x18u), (ushort)c.S2);
        MemoryAccess.WriteU16(m, (c.SP + 0x1Eu), (ushort)c.V0);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.A3);
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.A2);
        c.T0 = c.HI;
        c.A2 = c.T0 + c.V1;
        c.A2 = (uint)((int)c.A2 >> 5);
        c.V1 = (uint)((int)c.V1 >> 31);
        c.A2 = c.A2 - c.V1;
        c.RA = 0x800207ECu;
        GranTurismo2PC.func_8006B814(c, m);
        c.A0 = c.FP + 0u;
        c.A1 = 0x00000220u;
        c.RA = 0x800207F8u;
        GranTurismo2PC.func_8007DA44(c, m);
        L800207F8: ;
        c.A1 = 0x00FF0000u;
        c.A1 = c.A1 | 0xFFFFu;
        c.V0 = 0x800C0000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 - 0x6AF0u));
        c.V0 = c.S0 << 3;
        c.V1 = c.V1 + c.V0;
        c.S1 = MemoryAccess.ReadU32(m, c.V1);
        c.S4 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.S5 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V1 + 0x7u));
        c.A0 = c.S1 + 0u;
        c.S4 = c.S4 & c.A1;
        c.RA = 0x80020828u;
        GranTurismo2PC.func_800182A8(c, m);
        c.A0 = c.S1 + 0u;
        c.S0 = c.V0 + 0u;
        c.RA = 0x80020834u;
        Dispatcher.Call(c, m, 0x800182FCu);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 + 0x29D0u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 + 0x29D4u;
        c.A3 = 0x00000080u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0xA8u));
        c.S3 = c.V0 + 0u;
        c.RA = 0x80020854u;
        GranTurismo2PC.func_8006B548(c, m);
        c.S6 = c.V0 + 0u;
        c.A0 = c.FP + 0u;
        c.A1 = c.S0 + 0u;
        c.S0 = c.S2 - 0xBCu;
        c.A2 = c.S0 + 0u;
        c.A3 = c.S7 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S6);
        c.RA = 0x80020874u;
        GranTurismo2PC.func_8001F38C(c, m);
        c.A0 = c.FP + 0u;
        c.A1 = c.S3 + 0u;
        c.S0 = c.S0 + c.V0;
        c.A2 = c.S0 + 0x5u;
        c.A3 = c.S7 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S6);
        c.RA = 0x80020890u;
        GranTurismo2PC.func_8001F38C(c, m);
        c.A0 = c.S1 + 0u;
        c.A1 = c.S5 + 0u;
        c.V0 = c.S2 - 0xC9u;
        MemoryAccess.WriteU16(m, (c.SP + 0x18u), (ushort)c.V0);
        c.V0 = c.S7 - 0x9u;
        MemoryAccess.WriteU16(m, (c.SP + 0x1Au), (ushort)c.V0);
        c.V0 = 0x00000009u;
        MemoryAccess.WriteU16(m, (c.SP + 0x1Cu), (ushort)c.V0);
        c.V0 = 0x0000000Cu;
        MemoryAccess.WriteU16(m, (c.SP + 0x1Eu), (ushort)c.V0);
        c.RA = 0x800208BCu;
        GranTurismo2PC.func_80060D28(c, m);
        c.A0 = c.FP + 0u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0xA8u));
        c.A1 = c.SP + 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.V0);
        c.RA = 0x800208D0u;
        GranTurismo2PC.func_8006BB08(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 + 0x29D0u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 + 0x29D8u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0xA8u));
        c.A3 = 0x00000080u;
        c.RA = 0x800208ECu;
        GranTurismo2PC.func_8006B548(c, m);
        c.S6 = c.V0 + 0u;
        c.S0 = c.SP + 0x28u;
        c.A0 = c.S0 + 0u;
        c.A1 = c.S4 + 0u;
        c.RA = 0x80020900u;
        GranTurismo2PC.func_8001FCDC(c, m);
        c.A0 = c.S0 + 0u;
        c.RA = 0x80020908u;
        GranTurismo2PC.func_8001F6CC(c, m);
        c.A0 = c.FP + 0u;
        c.A1 = c.S0 + 0u;
        c.V0 = c.V0 - 0xC8u;
        c.A2 = c.S2 - c.V0;
        c.A3 = c.S7 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S6);
        c.RA = 0x80020924u;
        GranTurismo2PC.func_8001F38C(c, m);
        c.A0 = 0x80050000u;
        c.A0 = c.A0 + 0x29D0u;
        c.A1 = 0x80050000u;
        c.A1 = c.A1 + 0x29DCu;
        c.A3 = 0x00000080u;
        c.A2 = MemoryAccess.ReadU32(m, (c.SP + 0xA8u));
        c.V0 = c.S2 - 0xD0u;
        MemoryAccess.WriteU16(m, (c.SP + 0x28u), (ushort)c.V0);
        c.V0 = 0x000001A0u;
        MemoryAccess.WriteU16(m, (c.SP + 0x2Cu), (ushort)c.V0);
        c.V0 = c.S7 - 0x10u;
        MemoryAccess.WriteU16(m, (c.SP + 0x2Au), (ushort)c.V0);
        c.V0 = 0x00000018u;
        MemoryAccess.WriteU16(m, (c.SP + 0x2Eu), (ushort)c.V0);
        c.RA = 0x80020960u;
        GranTurismo2PC.func_8006B548(c, m);
        c.A0 = c.FP + 0u;
        c.A1 = c.V0 + 0u;
        c.A2 = c.S0 + 0u;
        c.RA = 0x80020970u;
        GranTurismo2PC.func_8007E780(c, m);
        goto L8002097C;
        L80020978: ;
        MemoryAccess.WriteU16(m, (c.V0 - 0x6AF4u), (ushort)0u);
        L8002097C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0xD4u));
        c.FP = MemoryAccess.ReadU32(m, (c.SP + 0xD0u));
        c.S7 = MemoryAccess.ReadU32(m, (c.SP + 0xCCu));
        c.S6 = MemoryAccess.ReadU32(m, (c.SP + 0xC8u));
        c.S5 = MemoryAccess.ReadU32(m, (c.SP + 0xC4u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0xC0u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0xBCu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0xB8u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0xB4u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0xB0u));
        c.V0 = 0x00000001u;
        c.SP = c.SP + 0xD8u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800209B0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        c.A0 = 0x80050000u;
        c.A1 = 0x80020000u;
        c.A0 = c.A0 + 0x299Cu;
        c.A1 = c.A1 + 0x6F8u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.A2 = 0u + 0u;
        c.RA = 0x800209D8u;
        GranTurismo2PC.func_8006CDCC(c, m);
        c.V1 = 0x800C0000u;
        MemoryAccess.WriteU16(m, c.S0, (ushort)0u);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU16(m, (c.V1 - 0x6AF4u), (ushort)c.V0);
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800209F8(CpuContext c, IMemory m)
    {
        c.V0 = 0x80050000u;
        c.V0 = c.V0 + 0x299Cu;
        MemoryAccess.WriteU16(m, (c.V0 + 0x10u), (ushort)c.A1);
        MemoryAccess.WriteU16(m, (c.V0 + 0x12u), (ushort)c.A2);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020A0C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S1);
        c.S1 = c.A0 + 0u;
        c.A0 = c.A2 + 0u;
        c.A2 = 0x800C0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S0);
        MemoryAccess.WriteU32(m, (c.S1 + 0x4u), c.A1);
        c.A1 = c.SP + 0x10u;
        c.A2 = c.A2 - 0x6AF0u;
        c.RA = 0x80020A38u;
        GranTurismo2PC.func_80022634(c, m);
        c.V1 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        if ((int)c.V1 <= 0) {
            c.V0 = 0x80050000u;
            goto L80020A74;
        }
        c.V0 = 0x80050000u;
        c.S0 = c.V0 + 0x299Cu;
        c.A0 = c.S0 + 0u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x299Cu), (ushort)c.V1);
        c.RA = 0x80020A58u;
        GranTurismo2PC.func_8006CE70(c, m);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.S0 + 0x6u));
        c.V1 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = (int)c.V0 < (int)c.V1 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = c.V1 - 0x1u;
            goto L80020A74;
        }
        c.V0 = c.V1 - 0x1u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x6u), (ushort)c.V0);
        L80020A74: ;
        c.V0 = MemoryAccess.ReadU16(m, (c.SP + 0x10u));
        MemoryAccess.WriteU16(m, c.S1, (ushort)c.V0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020A94(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0xFFFFFFFFu;
        if (c.A2 == 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
            goto L80020AE0;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.V1 = 0x800C0000u;
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 - 0x6AF4u));
        c.V0 = c.V0 + 0x1u;
        MemoryAccess.WriteU16(m, (c.V1 - 0x6AF4u), (ushort)c.V0);
        c.V0 = c.V0 << 16;
        c.V0 = (uint)((int)c.V0 >> 16);
        c.V0 = (int)c.V0 < 61 ? 1u : 0u;
        if (c.V0 != 0u) {
            goto L80020AE8;
        }
        MemoryAccess.WriteU16(m, (c.V1 - 0x6AF4u), (ushort)0u);
        goto L80020AE8;
        L80020AE0: ;
        c.V0 = 0x800C0000u;
        MemoryAccess.WriteU16(m, (c.V0 - 0x6AF4u), (ushort)c.S0);
        L80020AE8: ;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.S1);
        if ((int)c.V0 <= 0) {
            c.A0 = 0x80050000u;
            goto L80020B58;
        }
        c.A0 = 0x80050000u;
        c.A0 = c.A0 + 0x299Cu;
        c.RA = 0x80020B00u;
        GranTurismo2PC.func_8006CFC4(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = 0xFFFFFFFDu;
        if (c.V1 == c.V0) {
            c.V0 = (int)c.V1 < -2 ? 1u : 0u;
            goto L80020B44;
        }
        c.V0 = (int)c.V1 < -2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0xFFFFFFFCu;
            goto L80020B28;
        }
        c.V0 = 0xFFFFFFFCu;
        if (c.V1 == c.V0) {
            c.V0 = c.S0 + 0u;
            goto L80020B5C;
        }
        c.V0 = c.S0 + 0u;
        MemoryAccess.WriteU16(m, (c.S1 + 0x2u), (ushort)c.V1);
        goto L80020B54;
        L80020B28: ;
        c.V0 = 0xFFFFFFFEu;
        if (c.V1 == c.V0) {
            c.V0 = 0xFFFFFFFFu;
            goto L80020B58;
        }
        c.V0 = 0xFFFFFFFFu;
        if (c.V1 == c.V0) {
            c.S0 = 0xFFFFFFFEu;
            goto L80020B58;
        }
        c.S0 = 0xFFFFFFFEu;
        MemoryAccess.WriteU16(m, (c.S1 + 0x2u), (ushort)c.V1);
        goto L80020B54;
        L80020B44: ;
        c.A0 = 0x00000006u;
        c.RA = 0x80020B4Cu;
        GranTurismo2PC.func_80060840(c, m);
        c.V0 = c.S0 + 0u;
        goto L80020B5C;
        L80020B54: ;
        c.S0 = c.V1 + 0u;
        L80020B58: ;
        c.V0 = c.S0 + 0u;
        L80020B5C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020B70(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, c.A0);
        if ((int)c.V0 > 0) {
            c.A0 = 0x80050000u;
            goto L80020BC4;
        }
        c.A0 = 0x80050000u;
        c.V0 = 0x00F00000u;
        c.V0 = c.V0 | 0x780Au;
        c.A0 = c.A1 + 0u;
        c.A1 = 0x801C0000u;
        c.A1 = c.A1 + 0x30C0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        c.V0 = 0x80050000u;
        c.V0 = c.V0 + 0x299Cu;
        c.A2 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x10u));
        c.A3 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x12u));
        c.A2 = c.A2 - 0xBCu;
        c.A3 = c.A3 + 0x40u;
        c.RA = 0x80020BBCu;
        GranTurismo2PC.func_8001F520(c, m);
        goto L80020BCC;
        L80020BC4: ;
        c.A0 = c.A0 + 0x299Cu;
        c.RA = 0x80020BCCu;
        GranTurismo2PC.func_8006D50C(c, m);
        L80020BCC: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020BDC(CpuContext c, IMemory m)
    {
        c.V0 = 0x80050000u;
        c.V1 = 0x800C0000u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x29A2u));
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 - 0x6AF0u));
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.V1;
        c.V0 = MemoryAccess.ReadU32(m, c.V0);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020C00(CpuContext c, IMemory m)
    {
        c.V0 = 0x80050000u;
        c.V1 = 0x800C0000u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x29A2u));
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 - 0x6AF0u));
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.V1;
        c.V0 = (uint)(sbyte)MemoryAccess.ReadU8(m, (c.V0 + 0x7u));
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020C24(CpuContext c, IMemory m)
    {
        c.A0 = 0x00FF0000u;
        c.V0 = 0x80050000u;
        c.V1 = 0x800C0000u;
        c.V0 = (uint)(short)MemoryAccess.ReadU16(m, (c.V0 + 0x29A2u));
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 - 0x6AF0u));
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.V1;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x4u));
        c.A0 = c.A0 | 0xFFFFu;
        c.V0 = c.V0 & c.A0;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020C50(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x458u;
        c.A0 = 0x80020000u;
        c.A0 = c.A0 + 0x42A8u;
        MemoryAccess.WriteU32(m, (c.SP + 0x450u), c.RA);
        c.A1 = c.SP + 0x10u;
        c.RA = 0x80020C68u;
        GranTurismo2PC.func_80082FAC(c, m);
        c.A0 = 0x801C0000u;
        c.V0 = 0x801D0000u;
        c.V1 = MemoryAccess.ReadU8(m, (c.V0 - 0x6720u));
        c.A0 = c.A0 + 0x30C0u;
        c.V0 = c.V1 << 4;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 3;
        c.V1 = c.SP + 0x10u;
        c.V1 = c.V1 + c.V0;
        c.V0 = c.A0 & 0x0003u;
        if (c.V0 == 0u) {
            c.V0 = c.V1 + 0x80u;
            goto L80020CEC;
        }
        c.V0 = c.V1 + 0x80u;
        L80020C98: ;
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
            goto L80020C98;
        }
        c.A0 = c.A0 + 0x10u;
        goto L80020D18;
        L80020CEC: ;
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
            goto L80020CEC;
        }
        c.A0 = c.A0 + 0x10u;
        L80020D18: ;
        c.A2 = MemoryAccess.ReadWordLeft(m, c.A2, (c.V1 + 0x3u));
        c.A2 = MemoryAccess.ReadWordRight(m, c.A2, c.V1);
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V1 + 0x7u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, (c.V1 + 0x4u));
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0x3u), c.A2);
        MemoryAccess.WriteWordRight(m, c.A0, c.A2);
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0x7u), c.A3);
        MemoryAccess.WriteWordRight(m, (c.A0 + 0x4u), c.A3);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x450u));
        c.SP = c.SP + 0x458u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020D48(CpuContext c, IMemory m)
    {
        c.V0 = 0x80050000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x2A20u));
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020D58(CpuContext c, IMemory m)
    {
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU32(m, (c.V0 + 0x2A2Cu), c.A1);
        c.V0 = 0x80050000u;
        c.V1 = 0x80050000u;
        MemoryAccess.WriteU32(m, (c.V0 + 0x2A30u), c.A0);
        c.V0 = c.A1 + 0x4u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x2A38u), c.V0);
        c.V0 = 0x80050000u;
        c.A1 = c.A1 + 0x6000u;
        MemoryAccess.WriteU32(m, (c.V0 + 0x2A3Cu), c.A1);
        c.V0 = 0x80050000u;
        c.A1 = c.A1 + 0x2800u;
        MemoryAccess.WriteU32(m, (c.V0 + 0x2A40u), c.A1);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020D90_gt2_overlay_4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        c.V1 = 0x80050000u;
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU32(m, (c.V1 + 0x2A34u), c.V0);
        c.V0 = 0x801D0000u;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x6720u));
        c.A1 = 0x00001400u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = 0x80050000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.S2 + 0x29E0u), c.V0);
        c.RA = 0x80020DCCu;
        GranTurismo2PC.func_80020E4C(c, m);
        c.S1 = c.V0 + 0u;
        c.S0 = c.S0 + c.S1;
        c.A0 = 0x000000C4u;
        c.A1 = c.S0 + 0u;
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU32(m, (c.V0 + 0x2A24u), c.S0);
        c.RA = 0x80020DE8u;
        GranTurismo2PC.func_8005D8A0(c, m);
        c.V1 = 0x80050000u;
        c.V0 = MemoryAccess.ReadU32(m, c.S0);
        c.V1 = c.V1 + 0x29E4u;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + 0x8u;
        c.S1 = c.S1 + c.V0;
        c.S0 = c.S0 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, (c.S2 + 0x29E0u));
        c.A1 = c.S0 + 0u;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.V1;
        c.A0 = MemoryAccess.ReadU16(m, (c.V0 + 0x6u));
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU32(m, (c.V0 + 0x2A28u), c.S0);
        c.RA = 0x80020E24u;
        GranTurismo2PC.func_8005D8A0(c, m);
        c.V0 = MemoryAccess.ReadU32(m, c.S0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + 0x8u;
        c.V0 = c.S1 + c.V0;
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020E4C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        c.V1 = 0x80050000u;
        c.V0 = 0x80050000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x29E0u));
        c.V1 = c.V1 + 0x29E4u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.V1;
        c.A0 = MemoryAccess.ReadU16(m, (c.V0 + 0x4u));
        c.A1 = c.S0 + 0u;
        c.RA = 0x80020E80u;
        GranTurismo2PC.func_8005D8D4(c, m);
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU32(m, (c.V0 + 0x2A1Cu), c.S0);
        c.V0 = MemoryAccess.ReadU32(m, c.S0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.V1 = c.V0 + 0x1u;
        c.V0 = 0xFFFFFFFEu;
        c.V0 = c.V1 & c.V0;
        c.V1 = c.V0 << 1;
        c.V0 = c.V1 + 0x4u;
        c.S0 = c.S0 + c.V0;
        c.V0 = 0x80050000u;
        c.V1 = c.V1 + 0x8u;
        MemoryAccess.WriteU32(m, (c.V0 + 0x2A20u), c.S0);
        c.V0 = MemoryAccess.ReadU32(m, c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = c.V0 << 3;
        c.V0 = c.V1 + c.V0;
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020ECC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S0);
        c.S0 = c.A0 + 0u;
        c.A1 = c.S0 + 0u;
        c.V0 = 0x800B0000u;
        c.V1 = 0x80050000u;
        MemoryAccess.WriteU32(m, (c.V0 - 0x728Cu), 0u);
        c.V0 = 0x80050000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x29E0u));
        c.V1 = c.V1 + 0x29E4u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.V1;
        c.A0 = MemoryAccess.ReadU16(m, (c.V0 + 0x2u));
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V0 - 0x7288u), 0u);
        c.RA = 0x80020F10u;
        GranTurismo2PC.func_8005D8D4(c, m);
        c.A0 = c.SP + 0x10u;
        c.A1 = c.S0 + 0u;
        c.V0 = 0x000002C0u;
        MemoryAccess.WriteU16(m, (c.SP + 0x10u), (ushort)c.V0);
        c.V0 = 0x00000040u;
        MemoryAccess.WriteU16(m, (c.SP + 0x14u), (ushort)c.V0);
        c.V0 = 0x00000100u;
        c.A2 = 0u + 0u;
        MemoryAccess.WriteU16(m, (c.SP + 0x12u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.SP + 0x16u), (ushort)c.V0);
        c.RA = 0x80020F3Cu;
        GranTurismo2PC.func_8007BA70(c, m);
        c.RA = 0x80020F44u;
        GranTurismo2PC.func_8007AF30(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020F54(CpuContext c, IMemory m)
    {
        c.V0 = 0x80050000u;
        c.T0 = MemoryAccess.ReadU32(m, (c.V0 + 0x2A20u));
        c.A3 = 0xFFFFFFFFu;
        c.A2 = MemoryAccess.ReadU32(m, c.T0);
        c.T0 = c.T0 + 0x4u;
        c.V0 = c.A3 + c.A2;
        L80020F6C: ;
        c.A1 = (uint)((int)c.V0 >> 1);
        c.V0 = c.A1 << 3;
        c.V1 = c.T0 + c.V0;
        c.V0 = MemoryAccess.ReadU32(m, c.V1);
        if (c.V0 != c.A0) {
            c.V0 = c.V0 < c.A0 ? 1u : 0u;
            goto L80020F94;
        }
        c.V0 = c.V0 < c.A0 ? 1u : 0u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        return;
        L80020F94: ;
        if (c.V0 == 0u) {
            goto L80020FA4;
        }
        c.A3 = c.A1 + 0u;
        goto L80020FA8;
        L80020FA4: ;
        c.A2 = c.A1 + 0u;
        L80020FA8: ;
        c.V0 = c.A3 + 0x1u;
        if (c.V0 != c.A2) {
            c.V0 = c.A3 + c.A2;
            goto L80020F6C;
        }
        c.V0 = c.A3 + c.A2;
        c.V0 = 0u + 0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020FBC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V0 = c.A0 + 0u;
        c.A3 = c.A1 + 0u;
        c.V1 = 0x80050000u;
        c.A0 = 0x000000C3u;
        c.A2 = MemoryAccess.ReadU32(m, (c.V1 + 0x2A24u));
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A1 = c.V0 + 0u;
        c.RA = 0x80020FE0u;
        GranTurismo2PC.func_8002117C(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80020FF0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A3 = c.A1 + 0u;
        if (c.A3 == 0u) {
            c.T0 = c.A0 + 0u;
            goto L80021038;
        }
        c.T0 = c.A0 + 0u;
        c.V0 = 0x80050000u;
        c.V1 = 0x80050000u;
        c.A2 = MemoryAccess.ReadU32(m, (c.V0 + 0x2A28u));
        c.V0 = 0x80050000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x29E0u));
        c.V1 = c.V1 + 0x29E4u;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.V1;
        c.A0 = MemoryAccess.ReadU16(m, c.V0);
        c.A1 = c.T0 + 0u;
        c.RA = 0x80021030u;
        GranTurismo2PC.func_80021078(c, m);
        goto L80021068;
        L80021038: ;
        c.V0 = 0x80050000u;
        c.V1 = 0x80050000u;
        c.A2 = MemoryAccess.ReadU32(m, (c.V0 + 0x2A28u));
        c.V0 = 0x80050000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x29E0u));
        c.V1 = c.V1 + 0x29E4u;
        c.V0 = c.V0 << 3;
        c.V0 = c.V0 + c.V1;
        c.A0 = MemoryAccess.ReadU16(m, c.V0);
        c.A1 = c.T0 + 0u;
        c.RA = 0x80021064u;
        GranTurismo2PC.func_800210F8(c, m);
        c.V0 = 0u + 0u;
        L80021068: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80021078(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.A2 = c.A2 + 0x4u;
        c.A1 = c.A1 << 2;
        c.A1 = c.A1 + c.A2;
        c.V0 = 0xFFFFFFFCu;
        c.V1 = 0x801E0000u;
        c.A0 = c.A0 << 1;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.A2 = MemoryAccess.ReadU32(m, c.A1);
        c.S0 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.V1 = c.V1 + 0x35F0u;
        c.S0 = c.S0 - c.A2;
        c.A2 = c.A2 & c.V0;
        c.V0 = 0x801E0000u;
        c.V0 = c.V0 + 0x2EF0u;
        c.A0 = c.A0 + c.V0;
        c.V0 = MemoryAccess.ReadU16(m, c.A0);
        c.A0 = c.A3 + 0u;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.V1;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0x10u));
        c.V0 = 0xFFFFF800u;
        c.A1 = c.A1 & c.V0;
        c.A1 = c.A2 + c.A1;
        c.A2 = c.S0 + 0u;
        c.RA = 0x800210E4u;
        GranTurismo2PC.func_8005D810(c, m);
        c.V0 = c.S0 + 0u;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800210F8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.A3 = c.A0 << 1;
        c.A0 = 0u + 0u;
        c.A1 = c.A1 << 2;
        c.A2 = c.A2 + c.A1;
        c.V0 = 0xFFFFFFFCu;
        c.V1 = 0x801E0000u;
        c.V1 = c.V1 + 0x35F0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A1 = MemoryAccess.ReadU32(m, (c.A2 + 0x4u));
        c.A2 = c.A0 + 0u;
        c.A1 = c.A1 & c.V0;
        c.V0 = 0x801E0000u;
        c.V0 = c.V0 + 0x2EF0u;
        c.A3 = c.A3 + c.V0;
        c.V0 = MemoryAccess.ReadU16(m, c.A3);
        c.A3 = c.A0 + 0u;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.V1;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 + 0x10u));
        c.V1 = 0xFFFFF800u;
        c.V0 = c.V0 & c.V1;
        c.A1 = c.A1 + c.V0;
        c.A1 = c.A1 >> 11;
        c.RA = 0x8002115Cu;
        GranTurismo2PC.func_8007CFDC(c, m);
        c.A0 = 0x00000009u;
        c.V0 = 0x801F0000u;
        MemoryAccess.WriteU16(m, (c.V0 + 0x548u), (ushort)0u);
        c.RA = 0x8002116Cu;
        GranTurismo2PC.func_8007C4EC(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8002117C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.A2 = c.A2 + 0x4u;
        c.A1 = c.A1 << 2;
        c.A1 = c.A1 + c.A2;
        c.V0 = 0xFFFFFFFCu;
        c.V1 = 0x801E0000u;
        c.A0 = c.A0 << 1;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.A2 = MemoryAccess.ReadU32(m, c.A1);
        c.S0 = MemoryAccess.ReadU32(m, (c.A1 + 0x4u));
        c.V1 = c.V1 + 0x35F0u;
        c.S0 = c.S0 - c.A2;
        c.A2 = c.A2 & c.V0;
        c.V0 = 0x801E0000u;
        c.V0 = c.V0 + 0x2EF0u;
        c.A0 = c.A0 + c.V0;
        c.V0 = MemoryAccess.ReadU16(m, c.A0);
        c.A0 = c.A3 + 0u;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + c.V1;
        c.A1 = MemoryAccess.ReadU32(m, (c.V0 + 0x10u));
        c.V0 = 0xFFFFF800u;
        c.A1 = c.A1 & c.V0;
        c.A1 = c.A2 + c.A1;
        c.A2 = c.S0 + 0u;
        c.RA = 0x800211E8u;
        GranTurismo2PC.func_80021968(c, m);
        c.V0 = c.S0 + 0u;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800211FC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        if ((int)c.S2 >= 0) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
            goto L8002122C;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.V0 = 0x80050000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V0 + 0x2A1Cu));
        c.V0 = c.S2 << 1;
        c.V0 = c.V0 + c.V1;
        c.S2 = MemoryAccess.ReadU16(m, (c.V0 + 0x4u));
        L8002122C: ;
        c.S0 = 0x80050000u;
        c.RA = 0x80021234u;
        GranTurismo2PC.func_8007AF30(c, m);
        c.A1 = MemoryAccess.ReadU32(m, (c.S0 + 0x2A30u));
        c.A0 = c.S2 + 0u;
        c.RA = 0x80021240u;
        GranTurismo2PC.func_80020FF0(c, m);
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x2A30u));
        c.S0 = 0x80050000u;
        c.S1 = MemoryAccess.ReadU32(m, (c.S0 + 0x2A34u));
        c.RA = 0x80021254u;
        GranTurismo2PC.func_800213C4(c, m);
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x2A34u));
        if (c.S1 == c.V0) {
            c.A0 = c.S2 + 0u;
            goto L8002126C;
        }
        c.A0 = c.S2 + 0u;
        c.A1 = 0u + 0u;
        c.RA = 0x8002126Cu;
        GranTurismo2PC.func_80020FF0(c, m);
        L8002126C: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80021284(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S1);
        c.S1 = c.A0 + 0u;
        c.V1 = c.S1 + 0u;
        c.S1 = c.S1 + 0x4u;
        c.A1 = c.S1 + 0u;
        c.V0 = 0x80050000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S0);
        c.S0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x2A3Cu));
        c.S0 = c.S0 + 0x1u;
        c.S0 = c.S0 << 2;
        c.A2 = c.S0 + 0u;
        c.RA = 0x800212C4u;
        GranTurismo2PC.func_8008CFE0(c, m);
        c.S2 = c.SP + 0x10u;
        c.A0 = c.S2 + 0u;
        c.S0 = c.S1 + c.S0;
        c.A1 = c.S0 + 0u;
        c.A2 = 0u + 0u;
        c.V0 = 0x00000240u;
        MemoryAccess.WriteU16(m, (c.SP + 0x10u), (ushort)c.V0);
        c.V0 = 0x000000F8u;
        MemoryAccess.WriteU16(m, (c.SP + 0x12u), (ushort)c.V0);
        c.V0 = 0x00000040u;
        MemoryAccess.WriteU16(m, (c.SP + 0x14u), (ushort)c.V0);
        c.V0 = 0x00000008u;
        MemoryAccess.WriteU16(m, (c.SP + 0x16u), (ushort)c.V0);
        c.RA = 0x800212FCu;
        GranTurismo2PC.func_8007BA70(c, m);
        c.S1 = c.S0 + 0x404u;
        c.V1 = MemoryAccess.ReadU32(m, (c.S0 + 0x400u));
        c.V0 = 0x00000280u;
        MemoryAccess.WriteU16(m, (c.SP + 0x10u), (ushort)c.V0);
        c.V0 = 0x00000100u;
        MemoryAccess.WriteU16(m, (c.SP + 0x12u), (ushort)c.V0);
        c.V0 = 0x00000080u;
        MemoryAccess.WriteU16(m, (c.SP + 0x14u), (ushort)c.V0);
        c.V0 = c.V1 >> 5;
        c.V0 = c.V0 << 3;
        c.V1 = c.V1 & 0x001Fu;
        if (c.V1 == 0u) {
            MemoryAccess.WriteU16(m, (c.SP + 0x16u), (ushort)c.V0);
            goto L80021338;
        }
        MemoryAccess.WriteU16(m, (c.SP + 0x16u), (ushort)c.V0);
        c.V0 = c.V0 + 0x8u;
        MemoryAccess.WriteU16(m, (c.SP + 0x16u), (ushort)c.V0);
        L80021338: ;
        c.A0 = c.S2 + 0u;
        c.A1 = c.S1 + 0u;
        c.A2 = 0u + 0u;
        c.RA = 0x80021348u;
        GranTurismo2PC.func_8007BA70(c, m);
        c.RA = 0x80021350u;
        GranTurismo2PC.func_8007AF30(c, m);
        c.V0 = c.S0 + 0u;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8002136C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        c.V0 = 0x000001F8u;
        MemoryAccess.WriteU16(m, (c.SP + 0x12u), (ushort)c.V0);
        c.V0 = 0x00000200u;
        MemoryAccess.WriteU16(m, (c.SP + 0x14u), (ushort)c.V0);
        c.V0 = 0x00000008u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S0);
        c.S0 = c.A0 + 0x1F84u;
        c.A0 = c.SP + 0x10u;
        c.A1 = c.S0 + 0u;
        c.A2 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU16(m, (c.SP + 0x10u), (ushort)0u);
        MemoryAccess.WriteU16(m, (c.SP + 0x16u), (ushort)c.V0);
        c.RA = 0x800213A8u;
        GranTurismo2PC.func_8007BA70(c, m);
        c.RA = 0x800213B0u;
        GranTurismo2PC.func_8007AF30(c, m);
        c.V0 = c.S0 + 0u;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800213C4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        c.S4 = c.S1 + 0x4u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        MemoryAccess.WriteU32(m, (c.V0 - 0x728Cu), 0u);
        c.S3 = MemoryAccess.ReadU32(m, (c.S1 + 0x4u));
        c.S1 = c.S1 + 0x8u;
        c.S2 = 0u + 0u;
        L800213FC: ;
        c.V0 = c.S2 < c.S3 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.S2 = c.S2 + 0x1u;
            goto L80021450;
        }
        c.S2 = c.S2 + 0x1u;
        c.V1 = MemoryAccess.ReadU16(m, c.S1);
        c.S0 = MemoryAccess.ReadU16(m, (c.S1 + 0x2u));
        c.V0 = c.V1 << 1;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 + 0x4u;
        c.S1 = c.S1 + c.V0;
        c.A0 = c.S1 + 0u;
        c.S0 = c.S0 & 0xFFFFu;
        c.A1 = c.S0 + 0u;
        c.RA = 0x80021434u;
        GranTurismo2PC.func_8002150C(c, m);
        c.V0 = c.S0 << 2;
        c.V0 = c.V0 + c.S0;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 - c.S0;
        c.V0 = c.V0 << 2;
        c.S1 = c.S1 + c.V0;
        goto L800213FC;
        L80021450: ;
        c.A1 = c.S4 + 0u;
        c.V0 = 0x80050000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x2A40u));
        c.A2 = c.S1 - c.A1;
        c.RA = 0x80021464u;
        GranTurismo2PC.func_8008CFE0(c, m);
        c.S0 = MemoryAccess.ReadU32(m, c.S1);
        c.S1 = c.S1 + 0x4u;
        c.A0 = c.S1 + 0u;
        c.A1 = c.S0 + 0u;
        c.RA = 0x80021478u;
        GranTurismo2PC.func_8002150C(c, m);
        c.V0 = c.S0 << 2;
        c.V0 = c.V0 + c.S0;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 - c.S0;
        c.V0 = c.V0 << 2;
        c.S1 = c.S1 + c.V0;
        c.V1 = MemoryAccess.ReadU32(m, c.S1);
        c.S1 = c.S1 + 0x4u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V0 - 0x7288u), c.V1);
        c.V1 = MemoryAccess.ReadU32(m, c.S1);
        c.S1 = c.S1 + 0x4u;
        c.A0 = c.S1 + 0x4u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V0 - 0x7290u), c.V1);
        c.S1 = MemoryAccess.ReadU32(m, c.S1);
        c.S2 = 0x80050000u;
        c.RA = 0x800214C0u;
        GranTurismo2PC.func_80021284(c, m);
        c.V0 = MemoryAccess.ReadU32(m, (c.S2 + 0x2A34u));
        if (c.S1 == c.V0) {
            c.S0 = 0x80050000u;
            goto L800214E8;
        }
        c.S0 = 0x80050000u;
        c.A1 = MemoryAccess.ReadU32(m, (c.S0 + 0x2A2Cu));
        c.A0 = c.S1 + 0u;
        c.RA = 0x800214DCu;
        GranTurismo2PC.func_80020FBC(c, m);
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x2A2Cu));
        c.RA = 0x800214E8u;
        GranTurismo2PC.func_8002136C(c, m);
        L800214E8: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x24u));
        c.S4 = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        MemoryAccess.WriteU32(m, (c.S2 + 0x2A34u), c.S1);
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8002150C(CpuContext c, IMemory m)
    {
        if ((int)c.A1 <= 0) {
            c.A3 = 0x800B0000u;
            goto L800215A8;
        }
        c.A3 = 0x800B0000u;
        c.V0 = 0x801C0000u;
        c.T0 = c.V0 + 0x3150u;
        L8002151C: ;
        c.V1 = MemoryAccess.ReadU32(m, (c.A3 - 0x728Cu));
        c.V0 = (int)c.V1 < 64 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.V1 << 2;
            goto L800215A8;
        }
        c.V0 = c.V1 << 2;
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 - c.V1;
        c.V0 = c.V0 << 2;
        c.A2 = c.V0 + c.T0;
        c.V1 = c.A0 + 0u;
        c.V0 = c.A0 + 0x40u;
        L8002154C: ;
        c.T1 = MemoryAccess.ReadU32(m, c.V1);
        c.T2 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T3 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T4 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, c.A2, c.T1);
        MemoryAccess.WriteU32(m, (c.A2 + 0x4u), c.T2);
        MemoryAccess.WriteU32(m, (c.A2 + 0x8u), c.T3);
        MemoryAccess.WriteU32(m, (c.A2 + 0xCu), c.T4);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.V0) {
            c.A2 = c.A2 + 0x10u;
            goto L8002154C;
        }
        c.A2 = c.A2 + 0x10u;
        c.A0 = c.A0 + 0x4Cu;
        c.V0 = MemoryAccess.ReadU32(m, (c.A3 - 0x728Cu));
        c.A1 = c.A1 - 0x1u;
        c.T1 = MemoryAccess.ReadU32(m, c.V1);
        c.T2 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T3 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        MemoryAccess.WriteU32(m, c.A2, c.T1);
        MemoryAccess.WriteU32(m, (c.A2 + 0x4u), c.T2);
        MemoryAccess.WriteU32(m, (c.A2 + 0x8u), c.T3);
        c.V0 = c.V0 + 0x1u;
        if ((int)c.A1 > 0) {
            MemoryAccess.WriteU32(m, (c.A3 - 0x728Cu), c.V0);
            goto L8002151C;
        }
        MemoryAccess.WriteU32(m, (c.A3 - 0x728Cu), c.V0);
        L800215A8: ;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800215B0(CpuContext c, IMemory m)
    {
        c.V0 = MemoryAccess.ReadWordLeft(m, c.V0, (c.A0 + 0x2u));
        MemoryAccess.WriteWordLeft(m, (c.A2 + 0x2u), c.V0);
        c.A1 = c.A1 << 8;
        MemoryAccess.WriteWordLeft(m, (c.A0 + 0x2u), c.A1);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800215C8(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x728Cu));
        c.V0 = (int)c.A0 < (int)c.V0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.A0 << 2;
            goto L80021600;
        }
        c.V0 = c.A0 << 2;
        c.V0 = c.V0 + c.A0;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 - c.A0;
        c.V0 = c.V0 << 2;
        c.V1 = 0x801C0000u;
        c.V1 = c.V1 + 0x3150u;
        c.V0 = c.V0 + c.V1;
        return;
        L80021600: ;
        c.V0 = 0u + 0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80021608(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.A0 = MemoryAccess.ReadU32(m, c.S1);
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x4u));
        c.V1 = MemoryAccess.ReadU8(m, (c.S1 + 0x24u));
        c.A2 = MemoryAccess.ReadU32(m, (c.S1 + 0x8u));
        c.S0 = 0x00000001u;
        if (c.V1 == c.S0) {
            MemoryAccess.WriteU8(m, (c.S1 + 0x25u), (byte)c.V0);
            goto L80021698;
        }
        MemoryAccess.WriteU8(m, (c.S1 + 0x25u), (byte)c.V0);
        c.V0 = (int)c.V1 < 2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = 0x00000002u;
            goto L80021658;
        }
        c.V0 = 0x00000002u;
        if (c.V1 == 0u) {
            c.V0 = 0u + 0u;
            goto L80021670;
        }
        c.V0 = 0u + 0u;
        goto L800218A8;
        L80021658: ;
        if (c.V1 == c.V0) {
            c.V0 = 0x00000003u;
            goto L80021780;
        }
        c.V0 = 0x00000003u;
        if (c.V1 == c.V0) {
            c.V0 = 0u + 0u;
            goto L8002189C;
        }
        c.V0 = 0u + 0u;
        goto L800218A8;
        L80021670: ;
        c.V1 = MemoryAccess.ReadU32(m, (c.S1 + 0x10u));
        c.V0 = 0x00000800u;
        c.A0 = c.A0 + c.V1;
        c.A2 = c.V1 + 0u;
        c.A2 = c.V0 - c.A2;
        c.V0 = c.V0 - c.V1;
        MemoryAccess.WriteU32(m, (c.S1 + 0xCu), c.V0);
        c.RA = 0x80021690u;
        GranTurismo2PC.func_8008DFC4(c, m);
        MemoryAccess.WriteU8(m, (c.S1 + 0x24u), (byte)c.S0);
        goto L800218A4;
        L80021698: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0xCu));
        c.A1 = c.A1 + c.V0;
        c.V0 = c.A0 | c.A1;
        c.V0 = c.V0 & 0x0003u;
        if (c.V0 == 0u) {
            c.V1 = c.A0 + 0u;
            goto L8002170C;
        }
        c.V1 = c.A0 + 0u;
        c.V0 = c.A0 + 0x800u;
        L800216B8: ;
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.V1 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.V1);
        c.T0 = MemoryAccess.ReadWordLeft(m, c.T0, (c.V1 + 0x7u));
        c.T0 = MemoryAccess.ReadWordRight(m, c.T0, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadWordLeft(m, c.T1, (c.V1 + 0xBu));
        c.T1 = MemoryAccess.ReadWordRight(m, c.T1, (c.V1 + 0x8u));
        c.T2 = MemoryAccess.ReadWordLeft(m, c.T2, (c.V1 + 0xFu));
        c.T2 = MemoryAccess.ReadWordRight(m, c.T2, (c.V1 + 0xCu));
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0x3u), c.A3);
        MemoryAccess.WriteWordRight(m, c.A1, c.A3);
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0x7u), c.T0);
        MemoryAccess.WriteWordRight(m, (c.A1 + 0x4u), c.T0);
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0xBu), c.T1);
        MemoryAccess.WriteWordRight(m, (c.A1 + 0x8u), c.T1);
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0xFu), c.T2);
        MemoryAccess.WriteWordRight(m, (c.A1 + 0xCu), c.T2);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.V0) {
            c.A1 = c.A1 + 0x10u;
            goto L800216B8;
        }
        c.A1 = c.A1 + 0x10u;
        goto L8002173C;
        L8002170C: ;
        c.V0 = c.A0 + 0x800u;
        L80021710: ;
        c.A3 = MemoryAccess.ReadU32(m, c.V1);
        c.T0 = MemoryAccess.ReadU32(m, (c.V1 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.V1 + 0xCu));
        MemoryAccess.WriteU32(m, c.A1, c.A3);
        MemoryAccess.WriteU32(m, (c.A1 + 0x4u), c.T0);
        MemoryAccess.WriteU32(m, (c.A1 + 0x8u), c.T1);
        MemoryAccess.WriteU32(m, (c.A1 + 0xCu), c.T2);
        c.V1 = c.V1 + 0x10u;
        if (c.V1 != c.V0) {
            c.A1 = c.A1 + 0x10u;
            goto L80021710;
        }
        c.A1 = c.A1 + 0x10u;
        L8002173C: ;
        c.S0 = MemoryAccess.ReadU32(m, (c.S1 + 0xCu));
        c.V0 = c.S0 + 0x800u;
        MemoryAccess.WriteU32(m, (c.S1 + 0xCu), c.V0);
        c.V0 = c.V0 < 0x00004000u ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = 0u + 0u;
            goto L800218A8;
        }
        c.V0 = 0u + 0u;
        c.S0 = c.S0 - 0x3800u;
        c.A0 = c.A0 + 0x800u;
        c.A0 = c.A0 - c.S0;
        c.A1 = c.A2 + 0u;
        c.A2 = c.S0 + 0u;
        c.RA = 0x80021770u;
        GranTurismo2PC.func_8008DFC4(c, m);
        c.V0 = 0x00000002u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x18u), c.S0);
        MemoryAccess.WriteU8(m, (c.S1 + 0x24u), (byte)c.V0);
        goto L800218A4;
        L80021780: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x18u));
        c.A1 = c.A2 + c.V0;
        c.V0 = c.A0 | c.A1;
        c.V0 = c.V0 & 0x0003u;
        if (c.V0 == 0u) {
            c.V0 = c.A0 + 0x800u;
            goto L800217F0;
        }
        c.V0 = c.A0 + 0x800u;
        L8002179C: ;
        c.A3 = MemoryAccess.ReadWordLeft(m, c.A3, (c.A0 + 0x3u));
        c.A3 = MemoryAccess.ReadWordRight(m, c.A3, c.A0);
        c.T0 = MemoryAccess.ReadWordLeft(m, c.T0, (c.A0 + 0x7u));
        c.T0 = MemoryAccess.ReadWordRight(m, c.T0, (c.A0 + 0x4u));
        c.T1 = MemoryAccess.ReadWordLeft(m, c.T1, (c.A0 + 0xBu));
        c.T1 = MemoryAccess.ReadWordRight(m, c.T1, (c.A0 + 0x8u));
        c.T2 = MemoryAccess.ReadWordLeft(m, c.T2, (c.A0 + 0xFu));
        c.T2 = MemoryAccess.ReadWordRight(m, c.T2, (c.A0 + 0xCu));
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0x3u), c.A3);
        MemoryAccess.WriteWordRight(m, c.A1, c.A3);
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0x7u), c.T0);
        MemoryAccess.WriteWordRight(m, (c.A1 + 0x4u), c.T0);
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0xBu), c.T1);
        MemoryAccess.WriteWordRight(m, (c.A1 + 0x8u), c.T1);
        MemoryAccess.WriteWordLeft(m, (c.A1 + 0xFu), c.T2);
        MemoryAccess.WriteWordRight(m, (c.A1 + 0xCu), c.T2);
        c.A0 = c.A0 + 0x10u;
        if (c.A0 != c.V0) {
            c.A1 = c.A1 + 0x10u;
            goto L8002179C;
        }
        c.A1 = c.A1 + 0x10u;
        goto L8002181C;
        L800217F0: ;
        c.A3 = MemoryAccess.ReadU32(m, c.A0);
        c.T0 = MemoryAccess.ReadU32(m, (c.A0 + 0x4u));
        c.T1 = MemoryAccess.ReadU32(m, (c.A0 + 0x8u));
        c.T2 = MemoryAccess.ReadU32(m, (c.A0 + 0xCu));
        MemoryAccess.WriteU32(m, c.A1, c.A3);
        MemoryAccess.WriteU32(m, (c.A1 + 0x4u), c.T0);
        MemoryAccess.WriteU32(m, (c.A1 + 0x8u), c.T1);
        MemoryAccess.WriteU32(m, (c.A1 + 0xCu), c.T2);
        c.A0 = c.A0 + 0x10u;
        if (c.A0 != c.V0) {
            c.A1 = c.A1 + 0x10u;
            goto L800217F0;
        }
        c.A1 = c.A1 + 0x10u;
        L8002181C: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0xCu));
        c.V1 = MemoryAccess.ReadU32(m, (c.S1 + 0x18u));
        c.V0 = c.V0 + 0x800u;
        c.V1 = c.V1 + 0x800u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x18u), c.V1);
        c.V1 = c.V1 < 0x00001000u ? 1u : 0u;
        if (c.V1 != 0u) {
            MemoryAccess.WriteU32(m, (c.S1 + 0xCu), c.V0);
            goto L80021878;
        }
        MemoryAccess.WriteU32(m, (c.S1 + 0xCu), c.V0);
        c.A0 = c.S1 + 0x1Cu;
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x8u));
        c.A2 = 0u + 0u;
        c.RA = 0x8002184Cu;
        GranTurismo2PC.func_8007BA70(c, m);
        c.RA = 0x80021854u;
        GranTurismo2PC.func_8007AF30(c, m);
        c.A1 = MemoryAccess.ReadU32(m, (c.S1 + 0x8u));
        c.A2 = MemoryAccess.ReadU32(m, (c.S1 + 0x18u));
        c.V0 = MemoryAccess.ReadU16(m, (c.S1 + 0x1Eu));
        c.A0 = c.A1 + 0x1000u;
        c.A2 = c.A2 - 0x1000u;
        c.V0 = c.V0 + 0x8u;
        MemoryAccess.WriteU16(m, (c.S1 + 0x1Eu), (ushort)c.V0);
        MemoryAccess.WriteU32(m, (c.S1 + 0x18u), c.A2);
        c.RA = 0x80021878u;
        GranTurismo2PC.func_8008DFC4(c, m);
        L80021878: ;
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0xCu));
        c.V1 = MemoryAccess.ReadU32(m, (c.S1 + 0x14u));
        c.V0 = c.V0 < c.V1 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.V0 = 0u + 0u;
            goto L800218A8;
        }
        c.V0 = 0u + 0u;
        c.V0 = 0x00000003u;
        MemoryAccess.WriteU8(m, (c.S1 + 0x24u), (byte)c.V0);
        goto L800218A4;
        L8002189C: ;
        c.V0 = 0x00000001u;
        goto L800218A8;
        L800218A4: ;
        c.V0 = 0u + 0u;
        L800218A8: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800218BC(CpuContext c, IMemory m)
    {
        c.V0 = 0x800C0000u;
        c.V0 = c.V0 - 0x6AECu;
        MemoryAccess.WriteU32(m, (c.V0 + 0xCu), 0u);
        MemoryAccess.WriteU8(m, (c.V0 + 0x24u), (byte)0u);
        MemoryAccess.WriteU8(m, (c.V0 + 0x25u), (byte)0u);
        MemoryAccess.WriteU32(m, (c.V0 + 0x18u), 0u);
        MemoryAccess.WriteU16(m, (c.V0 + 0x1Eu), (ushort)0u);
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800218DC(CpuContext c, IMemory m)
    {
        c.A2 = c.A1 + 0u;
        c.A1 = c.A0 + 0u;
        c.A0 = 0x801F0000u;
        c.A0 = c.A0 + 0x510u;
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0x00000008u;
        c.A0 = 0x800C0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A0 - 0x6AECu;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        c.V0 = MemoryAccess.ReadU32(m, (c.S1 + 0x44u));
        c.V1 = c.A1 + 0x1u;
        if (c.A1 != c.V0) {
            MemoryAccess.WriteU32(m, (c.S1 + 0x60u), c.V0);
            goto L8002194C;
        }
        MemoryAccess.WriteU32(m, (c.S1 + 0x60u), c.V0);
        c.S0 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.S1 + 0x44u), c.V1);
        c.A0 = MemoryAccess.ReadU32(m, (c.A0 - 0x6AECu));
        c.A1 = 0x00000200u;
        c.RA = 0x80021934u;
        GranTurismo2PC.func_80089F18(c, m);
        c.A0 = c.S2 + 0u;
        c.RA = 0x8002193Cu;
        GranTurismo2PC.func_80021608(c, m);
        if (c.V0 == 0u) {
            c.V0 = c.S0 + 0u;
            goto L80021950;
        }
        c.V0 = c.S0 + 0u;
        MemoryAccess.WriteU8(m, (c.S1 + 0x167u), (byte)0u);
        c.S0 = 0x00000002u;
        L8002194C: ;
        c.V0 = c.S0 + 0u;
        L80021950: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80021968(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x2020u;
        MemoryAccess.WriteU32(m, (c.SP + 0x2018u), c.S2);
        c.S2 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x2014u), c.S1);
        c.S1 = c.A2 + 0u;
        c.V1 = 0x800C0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x2010u), c.S0);
        c.S0 = c.V1 - 0x6AECu;
        MemoryAccess.WriteU32(m, (c.SP + 0x201Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.S0 + 0x4u), c.A0);
        c.A0 = 0u + 0u;
        c.V0 = c.SP + 0x10u;
        MemoryAccess.WriteU32(m, (c.V1 - 0x6AECu), c.V0);
        c.V0 = c.SP + 0x810u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x8u), c.V0);
        c.V0 = 0x00000300u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x1Cu), (ushort)c.V0);
        c.V0 = 0x00000100u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x20u), (ushort)c.V0);
        c.V0 = 0x00000008u;
        MemoryAccess.WriteU16(m, (c.S0 + 0x22u), (ushort)c.V0);
        c.V0 = 0x801D0000u;
        c.V1 = c.S2 >> 11;
        MemoryAccess.WriteU8(m, (c.S0 + 0x24u), (byte)0u);
        MemoryAccess.WriteU8(m, (c.S0 + 0x25u), (byte)0u);
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x6C18u));
        c.A3 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.S0 + 0x18u), 0u);
        MemoryAccess.WriteU16(m, (c.S0 + 0x1Eu), (ushort)0u);
        MemoryAccess.WriteU32(m, (c.S0 + 0x14u), c.S1);
        c.V0 = c.V0 + c.V1;
        c.V0 = c.V0 << 11;
        c.V1 = c.S2 & 0x07FFu;
        c.S2 = c.V0 | c.V1;
        c.A1 = c.S2 >> 11;
        c.RA = 0x800219F8u;
        GranTurismo2PC.func_8007CFDC(c, m);
        c.A0 = 0x80020000u;
        c.A0 = c.A0 + 0x18BCu;
        c.A1 = c.S2 & 0x07FFu;
        c.V1 = 0x801F0000u;
        c.V1 = c.V1 + 0x510u;
        c.V0 = 0x80020000u;
        c.V0 = c.V0 + 0x18DCu;
        MemoryAccess.WriteU16(m, (c.V1 + 0x38u), (ushort)c.A1);
        MemoryAccess.WriteU32(m, (c.V1 + 0x2Cu), c.V0);
        MemoryAccess.WriteU32(m, (c.S0 + 0x10u), c.A1);
        c.RA = 0x80021A24u;
        GranTurismo2PC.func_8007AB14(c, m);
        c.V0 = c.S1 + 0u;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x201Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x2018u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x2014u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x2010u));
        c.SP = c.SP + 0x2020u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80021A40(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.A2 = c.A0 + 0u;
        c.V0 = 0x80090000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x2870u));
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A1 = 0u + 0u;
        c.RA = 0x80021A5Cu;
        GranTurismo2PC.func_80077D5C(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80021A6C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        c.V0 = 0x80090000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = MemoryAccess.ReadU32(m, (c.V0 + 0x2870u));
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        c.S4 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S2 = MemoryAccess.ReadU16(m, (c.S3 + 0xEu));
        if (c.S2 == 0u) {
            c.S0 = 0u + 0u;
            goto L80021AE8;
        }
        c.S0 = 0u + 0u;
        L80021AA4: ;
        c.A0 = c.S0 + 0u;
        c.RA = 0x80021AACu;
        GranTurismo2PC.func_80021A40(c, m);
        c.A0 = c.S3 + 0u;
        c.S1 = c.V0 + 0u;
        c.A2 = MemoryAccess.ReadU16(m, c.S1);
        c.A1 = 0u + 0u;
        c.RA = 0x80021AC0u;
        GranTurismo2PC.func_80077D88(c, m);
        c.A0 = c.S4 + 0u;
        c.A1 = c.V0 + 0u;
        c.RA = 0x80021ACCu;
        GranTurismo2PC.func_8008CF00(c, m);
        if (c.V0 != 0u) {
            c.S0 = c.S0 + 0x1u;
            goto L80021ADC;
        }
        c.S0 = c.S0 + 0x1u;
        c.V0 = c.S1 + 0u;
        goto L80021AEC;
        L80021ADC: ;
        c.V0 = (int)c.S0 < (int)c.S2 ? 1u : 0u;
        if (c.V0 != 0u) {
            goto L80021AA4;
        }
        L80021AE8: ;
        c.V0 = 0u + 0u;
        L80021AEC: ;
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
    public static void func_80021B0C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.A2 = c.A0 + 0u;
        c.V0 = 0x80090000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 + 0x286Cu));
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A1 = 0x0000001Du;
        c.RA = 0x80021B28u;
        GranTurismo2PC.func_80077E80(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80021B38(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x30u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        c.S4 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S5);
        c.S5 = c.A1 + 0u;
        c.A1 = 0x0000001Du;
        c.V0 = 0x80090000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 + 0x286Cu));
        c.A2 = c.S5 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.A0 = c.S1 + 0u;
        c.RA = 0x80021B78u;
        GranTurismo2PC.func_80077D5C(c, m);
        c.S0 = c.V0 + 0u;
        c.S3 = MemoryAccess.ReadU32(m, c.S0);
        c.S2 = MemoryAccess.ReadU8(m, (c.S0 + 0x7u));
        if (c.S3 != 0u) {
            c.V0 = c.S2 & 0x001Fu;
            goto L80021BB8;
        }
        c.V0 = c.S2 & 0x001Fu;
        c.A0 = c.S1 + 0u;
        c.A2 = MemoryAccess.ReadU16(m, (c.S4 + 0x1Cu));
        c.A1 = 0x00000005u;
        c.RA = 0x80021B9Cu;
        GranTurismo2PC.func_80077D5C(c, m);
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0xEu));
        if (c.V0 == 0u) {
            c.V0 = c.S2 & 0x001Fu;
            goto L80021BB8;
        }
        c.V0 = c.S2 & 0x001Fu;
        c.S2 = MemoryAccess.ReadU8(m, (c.S0 + 0x6u));
        c.V0 = c.S2 & 0x001Fu;
        L80021BB8: ;
        c.V0 = c.V0 << 8;
        c.V0 = c.S3 | c.V0;
        MemoryAccess.WriteU32(m, c.S4, c.V0);
        MemoryAccess.WriteU16(m, (c.S4 + 0x38u), (ushort)c.S5);
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
    public static void func_80021BEC(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.S0 = c.A1 + 0u;
        c.RA = 0x80021C00u;
        GranTurismo2PC.func_80021B0C(c, m);
        c.V1 = 0x80090000u;
        c.A1 = 0x0000001Du;
        c.A0 = MemoryAccess.ReadU32(m, (c.V1 + 0x286Cu));
        c.A2 = c.V0 + 0u;
        c.RA = 0x80021C14u;
        GranTurismo2PC.func_80077D5C(c, m);
        c.V1 = MemoryAccess.ReadU8(m, (c.V0 + 0x7u));
        if (c.S0 == 0u) {
            goto L80021C24;
        }
        c.V1 = MemoryAccess.ReadU8(m, (c.V0 + 0x6u));
        L80021C24: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = c.V1 + 0u;
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80021C38(CpuContext c, IMemory m)
    {
        c.V0 = 0x800C0000u;
        MemoryAccess.WriteU32(m, (c.V0 - 0x6AC4u), c.A0);
        c.V0 = 0x80050000u;
        c.V1 = 0x80050000u;
        MemoryAccess.WriteU8(m, (c.V0 + 0x2A48u), (byte)0u);
        c.V0 = 0xFFFFFFFFu;
        MemoryAccess.WriteU32(m, (c.V1 + 0x2A44u), c.V0);
        c.V0 = c.A0 + 0x4u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80021C5C(CpuContext c, IMemory m)
    {
        c.T0 = 0x80050000u;
        c.A2 = MemoryAccess.ReadU8(m, (c.T0 + 0x2A48u));
        c.V0 = c.A2 + c.A1;
        c.V0 = (int)c.V0 < 17 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.A3 = c.A0 + 0u;
            goto L80021CA4;
        }
        c.A3 = c.A0 + 0u;
        c.V1 = 0x800C0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 - 0x6AC4u));
        c.A0 = c.A3 << 8;
        MemoryAccess.WriteU8(m, (c.V0 + 0x3u), (byte)c.A2);
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 - 0x6AC4u));
        MemoryAccess.WriteWordLeft(m, (c.V0 + 0x2u), c.A0);
        c.A2 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.V1 - 0x6AC4u), c.A3);
        c.A3 = c.A3 + 0x4u;
        MemoryAccess.WriteU8(m, (c.T0 + 0x2A48u), (byte)0u);
        L80021CA4: ;
        c.V0 = c.A2 + c.A1;
        MemoryAccess.WriteU8(m, (c.T0 + 0x2A48u), (byte)c.V0);
        c.V0 = c.A3 + 0u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80021CB4(CpuContext c, IMemory m)
    {
        c.V1 = 0x80050000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 + 0x2A44u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A1 + 0u;
        if (c.V0 == c.S0) {
            MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
            goto L80021CEC;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.A1 = 0x00000001u;
        MemoryAccess.WriteU32(m, (c.V1 + 0x2A44u), c.S0);
        c.RA = 0x80021CDCu;
        GranTurismo2PC.func_80021C5C(c, m);
        c.V1 = c.V0 + 0u;
        c.V0 = c.V1 + 0x4u;
        MemoryAccess.WriteU32(m, c.V1, c.S0);
        goto L80021CF0;
        L80021CEC: ;
        c.V0 = c.A0 + 0u;
        L80021CF0: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80021D00(CpuContext c, IMemory m)
    {
        c.V1 = 0x800C0000u;
        c.V0 = 0x80050000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V1 - 0x6AC4u));
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 + 0x2A48u));
        c.A1 = 0xFFFFFF00u;
        MemoryAccess.WriteU8(m, (c.A0 + 0x3u), (byte)c.V0);
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 - 0x6AC4u));
        MemoryAccess.WriteWordLeft(m, (c.V0 + 0x2u), c.A1);
        c.V0 = MemoryAccess.ReadU32(m, (c.V1 - 0x6AC4u));
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80021D30(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        c.V0 = c.A1 & 0x003Fu;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.V0 << 4;
        c.V0 = c.A1 >> 5;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.V0 & 0x07F8u;
        c.A1 = c.A1 >> 16;
        c.V1 = c.A1 & 0x7FFFu;
        c.A1 = c.A1 & 0x001Fu;
        c.A1 = c.A1 << 3;
        c.V0 = c.V1 << 6;
        c.V1 = (uint)((int)c.V1 >> 7);
        c.V1 = c.V1 & 0x00F8u;
        c.V0 = c.V0 & 0xF800u;
        c.A1 = c.A1 | c.V0;
        c.V0 = 0x800B0000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7288u));
        c.V1 = c.V1 << 16;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A1 | c.V1;
        c.V0 = c.V0 & 0x0200u;
        if (c.V0 == 0u) {
            MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
            goto L80021D98;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        if (c.S0 == 0u) {
            c.V0 = c.A0 + 0u;
            goto L80021DD0;
        }
        c.V0 = c.A0 + 0u;
        L80021D98: ;
        c.A1 = 0x00000003u;
        c.RA = 0x80021DA0u;
        GranTurismo2PC.func_80021C5C(c, m);
        c.A0 = c.V0 + 0u;
        c.V0 = c.A0 + 0xCu;
        c.A0 = c.A0 - 0x4u;
        c.V1 = 0x60000000u;
        c.V1 = c.S0 | c.V1;
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.V1);
        c.V1 = 0x00000010u;
        MemoryAccess.WriteU16(m, (c.A0 + 0xCu), (ushort)c.V1);
        c.V1 = 0x00000008u;
        MemoryAccess.WriteU16(m, (c.A0 + 0x8u), (ushort)c.S2);
        MemoryAccess.WriteU16(m, (c.A0 + 0xAu), (ushort)c.S1);
        MemoryAccess.WriteU16(m, (c.A0 + 0xEu), (ushort)c.V1);
        L80021DD0: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80021DE8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A1 + 0u;
        c.V0 = 0xE1000000u;
        c.V0 = c.V0 | 0x001Au;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.S0 >> 16;
        c.A1 = c.S1 & 0x0010u;
        c.A1 = c.A1 >> 4;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.A1 = c.A1 | c.V0;
        c.RA = 0x80021E18u;
        GranTurismo2PC.func_80021CB4(c, m);
        c.A0 = c.V0 + 0u;
        c.A1 = 0x00000004u;
        c.RA = 0x80021E24u;
        GranTurismo2PC.func_80021C5C(c, m);
        c.V1 = 0x64800000u;
        c.V1 = c.V1 | 0x8080u;
        c.A0 = c.V0 + 0u;
        c.V0 = c.A0 + 0x10u;
        c.A0 = c.A0 - 0x4u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.V1);
        c.V1 = c.S0 & 0x00FFu;
        c.V1 = c.V1 << 4;
        MemoryAccess.WriteU16(m, (c.A0 + 0x8u), (ushort)c.V1);
        c.V1 = c.S0 >> 5;
        c.V1 = c.V1 & 0x07F8u;
        c.S1 = c.S1 << 4;
        c.S1 = c.S1 & 0x00F0u;
        MemoryAccess.WriteU16(m, (c.A0 + 0xAu), (ushort)c.V1);
        c.V1 = c.S0 >> 18;
        c.V1 = c.V1 & 0x00F8u;
        c.S0 = c.S0 >> 27;
        MemoryAccess.WriteU8(m, (c.A0 + 0xDu), (byte)c.V1);
        c.V1 = c.S0 & 0x0003u;
        c.V1 = c.V1 | 0x0024u;
        c.S0 = c.S0 & 0x001Cu;
        c.S0 = c.S0 | 0x03E0u;
        c.S0 = c.S0 << 4;
        c.V1 = c.V1 | c.S0;
        MemoryAccess.WriteU16(m, (c.A0 + 0xEu), (ushort)c.V1);
        c.V1 = 0x00000010u;
        MemoryAccess.WriteU16(m, (c.A0 + 0x10u), (ushort)c.V1);
        c.V1 = 0x00000008u;
        MemoryAccess.WriteU8(m, (c.A0 + 0xCu), (byte)c.S1);
        MemoryAccess.WriteU16(m, (c.A0 + 0x12u), (ushort)c.V1);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80021EB0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A1 + 0u;
        c.V1 = 0xE1000000u;
        c.V1 = c.V1 | 0x0080u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.S0 >> 16;
        c.A1 = c.S1 & 0x0400u;
        c.A1 = c.A1 >> 6;
        c.V0 = c.S1 & 0x0010u;
        c.V0 = c.V0 >> 3;
        c.V0 = c.V0 | c.V1;
        c.A1 = c.A1 | c.V0;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        c.A1 = c.A1 | 0x000Cu;
        c.RA = 0x80021EF0u;
        GranTurismo2PC.func_80021CB4(c, m);
        c.A0 = c.V0 + 0u;
        c.A1 = 0x00000004u;
        c.RA = 0x80021EFCu;
        GranTurismo2PC.func_80021C5C(c, m);
        c.V1 = 0x64800000u;
        c.V1 = c.V1 | 0x8080u;
        c.A0 = c.V0 + 0u;
        c.V0 = c.A0 + 0x10u;
        c.A0 = c.A0 - 0x4u;
        MemoryAccess.WriteU32(m, (c.A0 + 0x4u), c.V1);
        c.V1 = c.S0 & 0x00FFu;
        c.V1 = c.V1 << 4;
        MemoryAccess.WriteU16(m, (c.A0 + 0x8u), (ushort)c.V1);
        c.V1 = c.S0 >> 5;
        c.V1 = c.V1 & 0x07F8u;
        c.S1 = c.S1 << 4;
        c.S1 = c.S1 & 0x00F0u;
        MemoryAccess.WriteU16(m, (c.A0 + 0xAu), (ushort)c.V1);
        c.V1 = c.S0 >> 18;
        c.V1 = c.V1 & 0x00F8u;
        c.S0 = c.S0 >> 28;
        MemoryAccess.WriteU8(m, (c.A0 + 0xDu), (byte)c.V1);
        c.V1 = c.S0 & 0x0001u;
        c.V1 = c.V1 << 4;
        c.S0 = c.S0 & 0x000Eu;
        c.S0 = c.S0 | 0x03F0u;
        c.S0 = c.S0 << 5;
        c.V1 = c.V1 | c.S0;
        MemoryAccess.WriteU16(m, (c.A0 + 0xEu), (ushort)c.V1);
        c.V1 = 0x00000010u;
        MemoryAccess.WriteU16(m, (c.A0 + 0x10u), (ushort)c.V1);
        c.V1 = 0x00000008u;
        MemoryAccess.WriteU8(m, (c.A0 + 0xCu), (byte)c.S1);
        MemoryAccess.WriteU16(m, (c.A0 + 0x12u), (ushort)c.V1);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80021F88(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.A0);
        c.A0 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        L80021FA8: ;
        c.V0 = (int)c.S2 < 63 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.S0 = 0u + 0u;
            goto L80022010;
        }
        c.S0 = 0u + 0u;
        c.S1 = c.SP + 0x20u;
        L80021FB8: ;
        c.V0 = (int)c.S0 < 32 ? 1u : 0u;
        if (c.V0 == 0u) {
            goto L80022008;
        }
        c.V0 = MemoryAccess.ReadU32(m, c.S1);
        c.A1 = MemoryAccess.ReadU32(m, c.V0);
        c.V0 = c.V0 + 0x4u;
        MemoryAccess.WriteU32(m, c.S1, c.V0);
        c.V0 = c.A1 & 0x0080u;
        if (c.V0 == 0u) {
            goto L80021FF4;
        }
        c.RA = 0x80021FECu;
        GranTurismo2PC.func_80021D30(c, m);
        c.A0 = c.V0 + 0u;
        goto L80022000;
        L80021FF4: ;
        c.RA = 0x80021FFCu;
        GranTurismo2PC.func_80021EB0(c, m);
        c.A0 = c.V0 + 0u;
        L80022000: ;
        c.S0 = c.S0 + 0x1u;
        goto L80021FB8;
        L80022008: ;
        c.S2 = c.S2 + 0x1u;
        goto L80021FA8;
        L80022010: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = c.A0 + 0u;
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8002202C(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        c.V1 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.SP + 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.A0);
        c.V0 = MemoryAccess.ReadU32(m, c.A0);
        c.A0 = c.A0 + 0x4u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.A0);
        c.S2 = c.V0 + 0u;
        L80022060: ;
        c.V0 = (int)c.S0 < (int)c.S2 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.V0 = c.V1 + 0u;
            goto L800220B0;
        }
        c.V0 = c.V1 + 0u;
        c.V0 = MemoryAccess.ReadU32(m, c.S1);
        c.A1 = MemoryAccess.ReadU32(m, c.V0);
        c.V0 = c.V0 + 0x4u;
        MemoryAccess.WriteU32(m, c.S1, c.V0);
        c.V0 = c.A1 & 0x0080u;
        if (c.V0 == 0u) {
            goto L8002209C;
        }
        c.A0 = c.V1 + 0u;
        c.RA = 0x80022094u;
        GranTurismo2PC.func_80021D30(c, m);
        c.V1 = c.V0 + 0u;
        goto L800220A8;
        L8002209C: ;
        c.A0 = c.V1 + 0u;
        c.RA = 0x800220A4u;
        GranTurismo2PC.func_80021DE8(c, m);
        c.V1 = c.V0 + 0u;
        L800220A8: ;
        c.S0 = c.S0 + 0x1u;
        goto L80022060;
        L800220B0: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800220C8(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x40u;
        MemoryAccess.WriteU32(m, (c.SP + 0x38u), c.FP);
        c.FP = c.SP + 0x40u;
        MemoryAccess.WriteU32(m, (c.SP + 0x3Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x34u), c.S7);
        MemoryAccess.WriteU32(m, (c.SP + 0x30u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.S5);
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S1);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S0);
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.A0);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), 0u);
        c.V0 = MemoryAccess.ReadU32(m, c.A0);
        c.A0 = c.A0 + 0x4u;
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.A0);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.V0);
        L80022110: ;
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.T0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.V0 = (int)c.A3 < (int)c.T0 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.S5 = c.SP + 0x40u;
            goto L80022244;
        }
        c.S5 = c.SP + 0x40u;
        c.V1 = MemoryAccess.ReadU32(m, c.FP);
        c.A0 = MemoryAccess.ReadU16(m, c.V1);
        c.V0 = c.V1 + 0x2u;
        MemoryAccess.WriteU32(m, c.FP, c.V0);
        c.V0 = MemoryAccess.ReadU16(m, (c.V1 + 0x2u));
        c.V1 = c.V1 + 0x4u;
        MemoryAccess.WriteU32(m, c.FP, c.V1);
        c.S6 = c.A0 & 0xFFFFu;
        c.S7 = c.V0 & 0xFFFFu;
        L80022150: ;
        if (c.S6 == 0u) {
            c.A0 = c.A1 + 0u;
            goto L80022214;
        }
        c.A0 = c.A1 + 0u;
        c.S6 = c.S6 - 0x1u;
        c.V1 = MemoryAccess.ReadU32(m, c.S5);
        c.A1 = 0xE1000000u;
        c.S2 = MemoryAccess.ReadU16(m, c.V1);
        c.V0 = c.V1 + 0x2u;
        MemoryAccess.WriteU32(m, c.S5, c.V0);
        c.S0 = MemoryAccess.ReadU16(m, (c.V1 + 0x2u));
        c.V0 = c.V1 + 0x4u;
        MemoryAccess.WriteU32(m, c.S5, c.V0);
        c.S4 = MemoryAccess.ReadU16(m, (c.V1 + 0x4u));
        c.V0 = c.V1 + 0x6u;
        MemoryAccess.WriteU32(m, c.S5, c.V0);
        c.S1 = MemoryAccess.ReadU16(m, (c.V1 + 0x6u));
        c.V0 = c.V1 + 0x8u;
        MemoryAccess.WriteU32(m, c.S5, c.V0);
        c.A2 = MemoryAccess.ReadU16(m, (c.V1 + 0x8u));
        c.V0 = c.V1 + 0xAu;
        MemoryAccess.WriteU32(m, c.S5, c.V0);
        c.S3 = MemoryAccess.ReadU16(m, (c.V1 + 0xAu));
        c.V1 = c.V1 + 0xCu;
        MemoryAccess.WriteU32(m, c.S5, c.V1);
        c.A2 = c.A2 & 0x001Fu;
        c.A1 = c.A2 | c.A1;
        c.S2 = c.S2 & 0xFFFFu;
        c.S0 = c.S0 & 0xFFFFu;
        c.S1 = c.S1 & 0xFFFFu;
        c.RA = 0x800221C4u;
        GranTurismo2PC.func_80021CB4(c, m);
        c.A0 = c.V0 + 0u;
        c.A1 = 0x00000004u;
        c.RA = 0x800221D0u;
        GranTurismo2PC.func_80021C5C(c, m);
        c.A1 = c.V0 + 0u;
        c.V0 = 0x64800000u;
        c.V0 = c.V0 | 0x8080u;
        c.V1 = c.A1 - 0x4u;
        c.A1 = c.A1 + 0x10u;
        c.S0 = c.S0 << 16;
        c.S0 = c.S0 | c.S2;
        MemoryAccess.WriteU32(m, (c.V1 + 0x4u), c.V0);
        c.V0 = c.S1 & 0xFF00u;
        c.V0 = c.V0 << 8;
        c.S1 = c.S1 & 0x00FFu;
        c.V0 = c.V0 | c.S1;
        MemoryAccess.WriteU32(m, (c.V1 + 0x8u), c.S0);
        MemoryAccess.WriteU16(m, (c.V1 + 0xCu), (ushort)c.S4);
        MemoryAccess.WriteU16(m, (c.V1 + 0xEu), (ushort)c.S3);
        MemoryAccess.WriteU32(m, (c.V1 + 0x10u), c.V0);
        goto L80022150;
        L80022214: ;
        c.V0 = c.S7 << 2;
        c.V0 = c.V0 + c.S7;
        c.V0 = c.V0 << 2;
        c.V0 = c.V0 - c.S7;
        c.V0 = c.V0 << 2;
        c.A3 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.V1 = MemoryAccess.ReadU32(m, (c.SP + 0x40u));
        c.A3 = c.A3 + 0x1u;
        c.V1 = c.V1 + c.V0;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.A3);
        MemoryAccess.WriteU32(m, (c.SP + 0x40u), c.V1);
        goto L80022110;
        L80022244: ;
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
        c.V0 = c.A1 + 0u;
        c.SP = c.SP + 0x40u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80022278(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A0 = c.A1 + 0u;
        c.RA = 0x80022288u;
        GranTurismo2PC.func_80021C38(c, m);
        c.V1 = 0x80050000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V1 + 0x2A38u));
        c.A1 = c.V0 + 0u;
        c.RA = 0x80022298u;
        GranTurismo2PC.func_80021F88(c, m);
        c.V1 = 0x80050000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V1 + 0x2A3Cu));
        c.A1 = c.V0 + 0u;
        c.RA = 0x800222A8u;
        GranTurismo2PC.func_8002202C(c, m);
        c.V1 = 0x80050000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V1 + 0x2A40u));
        c.V1 = MemoryAccess.ReadU32(m, c.A0);
        if (c.V1 == 0u) {
            goto L800222CC;
        }
        c.A1 = c.V0 + 0u;
        c.RA = 0x800222CCu;
        GranTurismo2PC.func_800220C8(c, m);
        L800222CC: ;
        c.RA = 0x800222D4u;
        GranTurismo2PC.func_80021D00(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800222E4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A1 + 0u;
        c.A0 = 0u + 0u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.V0 - 0x727Cu), c.S0);
        MemoryAccess.WriteU32(m, (c.S2 - 0x7284u), c.S1);
        c.RA = 0x80022318u;
        GranTurismo2PC.func_8005D8A0(c, m);
        c.A0 = 0u + 0u;
        c.RA = 0x80022320u;
        GranTurismo2PC.func_8005D768(c, m);
        c.V0 = c.V0 + 0x3u;
        c.V1 = 0xFFFFFFFCu;
        c.V0 = c.V0 & c.V1;
        c.S1 = c.S1 + c.V0;
        if (c.S0 != 0u) {
            c.A0 = 0x00000005u;
            goto L8002233C;
        }
        c.A0 = 0x00000005u;
        c.A0 = 0x00000004u;
        L8002233C: ;
        c.A1 = c.S1 + 0u;
        c.V0 = 0x800B0000u;
        MemoryAccess.WriteU32(m, (c.V0 - 0x7280u), c.A1);
        c.RA = 0x8002234Cu;
        GranTurismo2PC.func_8005D8A0(c, m);
        c.V0 = MemoryAccess.ReadU32(m, (c.S2 - 0x7284u));
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80022368(CpuContext c, IMemory m)
    {
        c.A1 = c.A1 << 1;
        c.A1 = c.A0 + c.A1;
        c.V0 = MemoryAccess.ReadU16(m, (c.A1 + 0x8u));
        c.V0 = c.A0 + c.V0;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_8002237C(CpuContext c, IMemory m)
    {
        c.V0 = 0x800B0000u;
        c.A1 = c.A1 << 1;
        c.V1 = c.A0 + c.A1;
        c.A0 = MemoryAccess.ReadU32(m, (c.V0 - 0x7280u));
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.A1 = MemoryAccess.ReadU16(m, c.V1);
        c.RA = 0x800223A0u;
        GranTurismo2PC.func_800223B0(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800223B0(CpuContext c, IMemory m)
    {
        c.A1 = c.A1 << 1;
        c.A1 = c.A0 + c.A1;
        c.V0 = MemoryAccess.ReadU16(m, c.A1);
        c.V0 = c.A0 + c.V0;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800223C4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        c.A0 = c.S0 + 0u;
        c.RA = 0x800223ECu;
        GranTurismo2PC.func_80060A24(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0u;
        c.S0 = c.V0 + 0u;
        c.RA = 0x800223FCu;
        GranTurismo2PC.func_80060CEC(c, m);
        c.A1 = c.S0 + 0u;
        c.V1 = 0x800B0000u;
        c.A0 = MemoryAccess.ReadU32(m, (c.V1 - 0x7284u));
        c.S0 = c.V0 + 0u;
        c.RA = 0x80022410u;
        GranTurismo2PC.func_80022368(c, m);
        c.A0 = c.V0 + 0u;
        c.A1 = c.S0 + 0u;
        c.RA = 0x8002241Cu;
        GranTurismo2PC.func_8002237C(c, m);
        c.V1 = 0x800B0000u;
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 - 0x727Cu));
        if (c.V1 != 0u) {
            c.A0 = c.S2 + 0x2u;
            goto L8002246C;
        }
        c.A0 = c.S2 + 0x2u;
        c.A0 = c.V0 + 0x2u;
        c.V0 = MemoryAccess.ReadU16(m, c.V0);
        MemoryAccess.WriteU16(m, c.S2, (ushort)c.V0);
        c.V0 = c.V0 & 0xFFFFu;
        if (c.V0 == 0u) {
            c.V1 = c.S2 + 0x2u;
            goto L80022494;
        }
        c.V1 = c.S2 + 0x2u;
        L8002244C: ;
        c.V0 = MemoryAccess.ReadU16(m, c.A0);
        c.A0 = c.A0 + 0x2u;
        MemoryAccess.WriteU16(m, c.V1, (ushort)c.V0);
        c.V0 = c.V0 & 0xFFFFu;
        if (c.V0 != 0u) {
            c.V1 = c.V1 + 0x2u;
            goto L8002244C;
        }
        c.V1 = c.V1 + 0x2u;
        c.V0 = c.S2 + 0u;
        goto L80022498;
        L8002246C: ;
        c.V1 = c.V0 + 0u;
        c.V0 = MemoryAccess.ReadU8(m, c.V1);
        c.V1 = c.V1 + 0x1u;
        if (c.V0 == 0u) {
            MemoryAccess.WriteU16(m, c.S2, (ushort)c.V0);
            goto L80022494;
        }
        MemoryAccess.WriteU16(m, c.S2, (ushort)c.V0);
        L80022480: ;
        c.V0 = MemoryAccess.ReadU8(m, c.V1);
        c.V1 = c.V1 + 0x1u;
        MemoryAccess.WriteU16(m, c.A0, (ushort)c.V0);
        if (c.V0 != 0u) {
            c.A0 = c.A0 + 0x2u;
            goto L80022480;
        }
        c.A0 = c.A0 + 0x2u;
        L80022494: ;
        c.V0 = c.S2 + 0u;
        L80022498: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800224B0(CpuContext c, IMemory m)
    {
        c.A1 = c.A1 << 2;
        c.A1 = c.A0 + c.A1;
        c.V0 = MemoryAccess.ReadU16(m, c.A1);
        c.V0 = c.A0 + c.V0;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800224C4(CpuContext c, IMemory m)
    {
        c.A1 = c.A1 << 2;
        c.V0 = c.A0 + c.A1;
        c.V0 = c.V0 + 0x2u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800224D4(CpuContext c, IMemory m)
    {
        c.V0 = 0x800C0000u;
        c.V0 = c.V0 - 0x6ABCu;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800224E0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x30u;
        c.V1 = c.A0 + 0u;
        c.V0 = 0x88880000u;
        c.V0 = c.V0 | 0x8889u;
        { var _r = (ulong)c.V1 * c.V0; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A1 + 0u;
        c.A0 = 0x0000000Bu;
        MemoryAccess.WriteU32(m, (c.SP + 0x24u), c.S5);
        c.S5 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x2Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x28u), c.S6);
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.S4);
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.A3 = c.HI;
        c.S1 = c.A3 >> 5;
        c.V0 = c.S1 << 4;
        c.V0 = c.V0 - c.S1;
        c.V0 = c.V0 << 2;
        c.S1 = c.V1 - c.V0;
        c.RA = 0x8002253Cu;
        GranTurismo2PC.func_8005D8D4(c, m);
        c.A1 = 0x800C0000u;
        c.A1 = c.A1 - 0x6ABCu;
        c.V1 = c.S1 << 2;
        c.V1 = c.S0 + c.V1;
        c.V0 = c.S1 + 0x1u;
        c.V0 = c.V0 << 2;
        c.V0 = c.S0 + c.V0;
        c.V1 = MemoryAccess.ReadU32(m, (c.V1 + 0x8u));
        c.A2 = MemoryAccess.ReadU32(m, (c.V0 + 0x8u));
        c.A0 = c.S0 + c.V1;
        c.A2 = c.A2 - c.V1;
        c.RA = 0x8002256Cu;
        GranTurismo2PC.func_8008DFC4(c, m);
        c.RA = 0x80022574u;
        GranTurismo2PC.func_800224D4(c, m);
        c.S6 = c.V0 + 0u;
        L80022578: ;
        c.V0 = (int)c.S5 < 39 ? 1u : 0u;
        if (c.V0 == 0u) {
            c.A0 = c.S6 + 0u;
            goto L8002260C;
        }
        c.A0 = c.S6 + 0u;
        c.A1 = c.S5 + 0u;
        c.RA = 0x8002258Cu;
        GranTurismo2PC.func_800224B0(c, m);
        c.A0 = c.S6 + 0u;
        c.A1 = c.S5 + 0u;
        c.S0 = c.V0 + 0u;
        c.RA = 0x8002259Cu;
        GranTurismo2PC.func_800224C4(c, m);
        c.S3 = 0u + 0u;
        c.S4 = c.V0 + 0u;
        c.V0 = MemoryAccess.ReadU16(m, c.S4);
        if (c.V0 == 0u) {
            c.S1 = c.S3 + 0u;
            goto L80022600;
        }
        c.S1 = c.S3 + 0u;
        c.S2 = c.S0 + 0u;
        L800225B8: ;
        c.A0 = MemoryAccess.ReadU32(m, c.S0);
        c.RA = 0x800225C4u;
        GranTurismo2PC.func_80060B70(c, m);
        if (c.V0 == 0u) {
            goto L800225EC;
        }
        if (c.S3 == c.S1) {
            goto L800225E4;
        }
        c.T0 = MemoryAccess.ReadU32(m, c.S0);
        c.T1 = MemoryAccess.ReadU32(m, (c.S0 + 0x4u));
        MemoryAccess.WriteU32(m, c.S2, c.T0);
        MemoryAccess.WriteU32(m, (c.S2 + 0x4u), c.T1);
        L800225E4: ;
        c.S2 = c.S2 + 0x8u;
        c.S1 = c.S1 + 0x1u;
        L800225EC: ;
        c.V0 = MemoryAccess.ReadU16(m, c.S4);
        c.S3 = c.S3 + 0x1u;
        c.V0 = (int)c.S3 < (int)c.V0 ? 1u : 0u;
        if (c.V0 != 0u) {
            c.S0 = c.S0 + 0x8u;
            goto L800225B8;
        }
        c.S0 = c.S0 + 0x8u;
        L80022600: ;
        MemoryAccess.WriteU16(m, c.S4, (ushort)c.S1);
        c.S5 = c.S5 + 0x1u;
        goto L80022578;
        L8002260C: ;
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
    public static void func_80022634(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x28u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.S3);
        c.S3 = c.A2 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x20u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.RA = 0x8002265Cu;
        GranTurismo2PC.func_800224D4(c, m);
        c.S0 = c.V0 + 0u;
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0u;
        c.RA = 0x8002266Cu;
        GranTurismo2PC.func_800224C4(c, m);
        c.A0 = c.S0 + 0u;
        c.V0 = MemoryAccess.ReadU16(m, c.V0);
        c.A1 = c.S1 + 0u;
        MemoryAccess.WriteU32(m, c.S2, c.V0);
        c.RA = 0x80022680u;
        GranTurismo2PC.func_800224B0(c, m);
        MemoryAccess.WriteU32(m, c.S3, c.V0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x20u));
        c.S3 = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x28u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800226A0(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A2 + 0u;
        c.A1 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        c.A2 = 0x000002BCu;
        c.RA = 0x800226CCu;
        GranTurismo2PC.func_8008CE30(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = c.S1 + 0u;
        c.A2 = c.S2 + 0u;
        c.RA = 0x800226DCu;
        GranTurismo2PC.func_80078790(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800226F4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        c.RA = 0x80022704u;
        GranTurismo2PC.func_80022838(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80022714(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        c.A2 = 0u + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.A3 = 0x00000001u;
        c.RA = 0x80022730u;
        GranTurismo2PC.func_800787CC(c, m);
        c.V0 = c.V0 + 0x3u;
        c.A0 = 0xFFFFFFFCu;
        c.V1 = MemoryAccess.ReadU32(m, c.S0);
        c.V0 = c.V0 & c.A0;
        c.V1 = c.V1 + c.V0;
        MemoryAccess.WriteU32(m, (c.S0 + 0x2B4u), c.V1);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80022758(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        c.A0 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.A1 = MemoryAccess.ReadU32(m, (c.S0 + 0x2B4u));
        c.RA = 0x80022778u;
        GranTurismo2PC.func_8005D8A0(c, m);
        c.A0 = MemoryAccess.ReadU32(m, (c.S0 + 0x2B4u));
        c.RA = 0x80022784u;
        GranTurismo2PC.func_8007A1C4(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80022794(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.S2);
        c.S2 = c.A1 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x1Cu), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.RA = 0x800227B4u;
        GranTurismo2PC.func_80022838(c, m);
        c.V1 = 0x80800000u;
        c.V0 = 0x801D0000u;
        c.V0 = MemoryAccess.ReadU8(m, (c.V0 - 0x666Du));
        c.V1 = c.V1 | 0x8081u;
        c.V0 = c.V0 << 14;
        { var _r = (long)(int)c.V0 * (int)c.V1; c.LO = (uint)_r; c.HI = (uint)(_r >> 32); }
        c.A3 = c.HI;
        c.A0 = c.A3 + c.V0;
        c.A0 = (uint)((int)c.A0 >> 7);
        c.V0 = (uint)((int)c.V0 >> 31);
        c.A0 = c.A0 - c.V0;
        c.A0 = c.A0 & 0xFFFFu;
        c.RA = 0x800227E8u;
        GranTurismo2PC.func_8007A260(c, m);
        c.S1 = c.S0 + 0xCu;
        c.A0 = c.S1 + 0u;
        c.A1 = c.S2 << 3;
        c.A1 = c.A1 + c.S2;
        c.A1 = c.A1 << 3;
        c.A1 = c.A1 + 0xCu;
        c.V0 = MemoryAccess.ReadU32(m, (c.S0 + 0x2B4u));
        c.A2 = MemoryAccess.ReadU32(m, c.S0);
        c.A1 = c.V0 + c.A1;
        c.RA = 0x80022810u;
        GranTurismo2PC.func_8007A300(c, m);
        c.A0 = c.S1 + 0u;
        c.RA = 0x80022818u;
        GranTurismo2PC.func_8007A4A4(c, m);
        c.V0 = 0x00000001u;
        MemoryAccess.WriteU8(m, (c.S0 + 0x2B8u), (byte)c.V0);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x1Cu));
        c.S2 = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80022838(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.S0 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        c.V0 = MemoryAccess.ReadU8(m, (c.S0 + 0x2B8u));
        if (c.V0 == 0u) {
            goto L80022860;
        }
        c.A0 = c.S0 + 0xCu;
        c.RA = 0x80022860u;
        GranTurismo2PC.func_8007A4D8(c, m);
        L80022860: ;
        MemoryAccess.WriteU8(m, (c.S0 + 0x2B8u), (byte)0u);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80022874(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x20u;
        c.V0 = 0x80090000u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.S1);
        c.S1 = MemoryAccess.ReadU32(m, (c.V0 + 0x2E74u));
        MemoryAccess.WriteU32(m, (c.SP + 0x18u), c.RA);
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.RA = 0x80022890u;
        GranTurismo2PC.func_80078370(c, m);
        c.S0 = 0x800C0000u;
        c.S0 = c.S0 - 0x5EBCu;
        c.A0 = c.S0 + 0u;
        c.A1 = 0x800C0000u;
        c.A1 = c.A1 - 0x5BFCu;
        c.A2 = 0x00007800u;
        c.RA = 0x800228ACu;
        GranTurismo2PC.func_800226A0(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0x000000ECu;
        c.RA = 0x800228B8u;
        GranTurismo2PC.func_80022714(c, m);
        c.A0 = c.S1 + 0u;
        c.RA = 0x800228C0u;
        GranTurismo2PC.func_80078950(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x18u));
        c.S1 = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x20u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_800228D4(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.A3 = 0x80050000u;
        c.V0 = MemoryAccess.ReadU32(m, (c.A3 + 0x2A4Cu));
        c.A2 = c.A0 + 0u;
        MemoryAccess.WriteU32(m, (c.SP + 0x14u), c.RA);
        if (c.V0 == c.A2) {
            MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
            goto L80022924;
        }
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.S0);
        c.V0 = 0x80050000u;
        c.V0 = c.V0 + 0x2A50u;
        c.V1 = c.A2 << 2;
        c.V1 = c.V1 + c.V0;
        c.S0 = 0x800C0000u;
        c.S0 = c.S0 - 0x5EBCu;
        c.A1 = MemoryAccess.ReadU32(m, c.V1);
        c.A0 = c.S0 + 0u;
        MemoryAccess.WriteU32(m, (c.A3 + 0x2A4Cu), c.A2);
        c.RA = 0x80022918u;
        GranTurismo2PC.func_80022758(c, m);
        c.A0 = c.S0 + 0u;
        c.A1 = 0u + 0u;
        c.RA = 0x80022924u;
        GranTurismo2PC.func_80022794(c, m);
        L80022924: ;
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x14u));
        c.S0 = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public static void func_80022934(CpuContext c, IMemory m)
    {
        c.SP = c.SP - 0x18u;
        c.V0 = 0x80050000u;
        c.V1 = 0xFFFFFFFFu;
        c.A0 = 0x800C0000u;
        c.A0 = c.A0 - 0x5EBCu;
        MemoryAccess.WriteU32(m, (c.SP + 0x10u), c.RA);
        MemoryAccess.WriteU32(m, (c.V0 + 0x2A4Cu), c.V1);
        c.RA = 0x80022954u;
        GranTurismo2PC.func_80022838(c, m);
        c.RA = MemoryAccess.ReadU32(m, (c.SP + 0x10u));
        c.SP = c.SP + 0x18u;
        return;
    }
}

public sealed class Gt2_overlay_4DispatchTable : IOverlay
{
    public string Name => "gt2_overlay_4";
    public int LbaStart => 451;
    public uint Base => 0x80010000u;
    public uint Size => 0x42A74u;
    public uint ImageSize => 0x27BDFFD0u;
    public bool Relocatable => false;
    public IReadOnlyDictionary<uint, Action<CpuContext, IMemory>> Functions { get; } =
        new Dictionary<uint, Action<CpuContext, IMemory>>
        {
            [0x80010000u] = GranTurismo2PC.func_80010000_gt2_overlay_4,
            [0x80010078u] = GranTurismo2PC.func_80010078_gt2_overlay_4,
            [0x80010368u] = GranTurismo2PC.func_80010368,
            [0x8001050Cu] = GranTurismo2PC.func_8001050C,
            [0x80010550u] = GranTurismo2PC.func_80010550,
            [0x8001058Cu] = GranTurismo2PC.func_8001058C,
            [0x800105C8u] = GranTurismo2PC.func_800105C8,
            [0x80010714u] = GranTurismo2PC.func_80010714,
            [0x80010984u] = GranTurismo2PC.func_80010984,
            [0x80010A30u] = GranTurismo2PC.func_80010A30,
            [0x80010F10u] = GranTurismo2PC.func_80010F10,
            [0x80011000u] = GranTurismo2PC.func_80011000,
            [0x80011160u] = GranTurismo2PC.func_80011160,
            [0x80011184u] = GranTurismo2PC.func_80011184,
            [0x800129B8u] = GranTurismo2PC.func_800129B8,
            [0x80012C6Cu] = GranTurismo2PC.func_80012C6C,
            [0x80012F7Cu] = GranTurismo2PC.func_80012F7C,
            [0x80013108u] = GranTurismo2PC.func_80013108,
            [0x80013628u] = GranTurismo2PC.func_80013628,
            [0x80013A28u] = GranTurismo2PC.func_80013A28,
            [0x80013B28u] = GranTurismo2PC.func_80013B28_gt2_overlay_4,
            [0x80013B60u] = GranTurismo2PC.func_80013B60,
            [0x80013BD4u] = GranTurismo2PC.func_80013BD4,
            [0x80013CD0u] = GranTurismo2PC.func_80013CD0,
            [0x80013CF8u] = GranTurismo2PC.func_80013CF8,
            [0x80013EECu] = GranTurismo2PC.func_80013EEC,
            [0x800141FCu] = GranTurismo2PC.func_800141FC,
            [0x80014290u] = GranTurismo2PC.func_80014290,
            [0x800142C4u] = GranTurismo2PC.func_800142C4,
            [0x800142CCu] = GranTurismo2PC.func_800142CC,
            [0x80014348u] = GranTurismo2PC.func_80014348,
            [0x80014380u] = GranTurismo2PC.func_80014380,
            [0x80014E60u] = GranTurismo2PC.func_80014E60,
            [0x80014E9Cu] = GranTurismo2PC.func_80014E9C,
            [0x80014EC4u] = GranTurismo2PC.func_80014EC4,
            [0x80014F50u] = GranTurismo2PC.func_80014F50,
            [0x80014F94u] = GranTurismo2PC.func_80014F94,
            [0x80014FECu] = GranTurismo2PC.func_80014FEC,
            [0x8001503Cu] = GranTurismo2PC.func_8001503C_gt2_overlay_4,
            [0x8001508Cu] = GranTurismo2PC.func_8001508C,
            [0x80015098u] = GranTurismo2PC.func_80015098,
            [0x80015108u] = GranTurismo2PC.func_80015108,
            [0x80015144u] = GranTurismo2PC.func_80015144,
            [0x8001516Cu] = GranTurismo2PC.func_8001516C,
            [0x80015228u] = GranTurismo2PC.func_80015228,
            [0x80015358u] = GranTurismo2PC.func_80015358,
            [0x800153B4u] = GranTurismo2PC.func_800153B4,
            [0x80015404u] = GranTurismo2PC.func_80015404,
            [0x80015428u] = GranTurismo2PC.func_80015428,
            [0x80016C5Cu] = GranTurismo2PC.func_80016C5C,
            [0x80016F10u] = GranTurismo2PC.func_80016F10,
            [0x80016FECu] = GranTurismo2PC.func_80016FEC,
            [0x8001706Cu] = GranTurismo2PC.func_8001706C,
            [0x8001719Cu] = GranTurismo2PC.func_8001719C,
            [0x800171C0u] = GranTurismo2PC.func_800171C0,
            [0x80017288u] = GranTurismo2PC.func_80017288,
            [0x80017318u] = GranTurismo2PC.func_80017318,
            [0x800173E8u] = GranTurismo2PC.func_800173E8,
            [0x80017480u] = GranTurismo2PC.func_80017480,
            [0x800174A0u] = GranTurismo2PC.func_800174A0,
            [0x800174ACu] = GranTurismo2PC.func_800174AC,
            [0x800174D0u] = GranTurismo2PC.func_800174D0,
            [0x800174F4u] = GranTurismo2PC.func_800174F4,
            [0x80017530u] = GranTurismo2PC.func_80017530,
            [0x800175A0u] = GranTurismo2PC.func_800175A0,
            [0x800175B0u] = GranTurismo2PC.func_800175B0,
            [0x800175C0u] = GranTurismo2PC.func_800175C0,
            [0x80017750u] = GranTurismo2PC.func_80017750,
            [0x800177D4u] = GranTurismo2PC.func_800177D4,
            [0x8001781Cu] = GranTurismo2PC.func_8001781C,
            [0x800178E4u] = GranTurismo2PC.func_800178E4,
            [0x80017914u] = GranTurismo2PC.func_80017914,
            [0x8001796Cu] = GranTurismo2PC.func_8001796C,
            [0x80017A68u] = GranTurismo2PC.func_80017A68,
            [0x80017A70u] = GranTurismo2PC.func_80017A70,
            [0x80017B04u] = GranTurismo2PC.func_80017B04,
            [0x80017B40u] = GranTurismo2PC.func_80017B40,
            [0x80017C98u] = GranTurismo2PC.func_80017C98,
            [0x80017D6Cu] = GranTurismo2PC.func_80017D6C,
            [0x80017F18u] = GranTurismo2PC.func_80017F18,
            [0x80018004u] = GranTurismo2PC.func_80018004,
            [0x80018100u] = GranTurismo2PC.func_80018100_gt2_overlay_4,
            [0x800181D0u] = GranTurismo2PC.func_800181D0,
            [0x80018210u] = GranTurismo2PC.func_80018210,
            [0x80018274u] = GranTurismo2PC.func_80018274,
            [0x8001828Cu] = GranTurismo2PC.func_8001828C,
            [0x800182A8u] = GranTurismo2PC.func_800182A8,
            [0x800182FCu] = GranTurismo2PC.func_800182FC_gt2_overlay_4,
            [0x80018350u] = GranTurismo2PC.func_80018350,
            [0x800183B8u] = GranTurismo2PC.func_800183B8,
            [0x800183ECu] = GranTurismo2PC.func_800183EC,
            [0x8001847Cu] = GranTurismo2PC.func_8001847C,
            [0x8001850Cu] = GranTurismo2PC.func_8001850C,
            [0x8001859Cu] = GranTurismo2PC.func_8001859C,
            [0x80018608u] = GranTurismo2PC.func_80018608,
            [0x8001861Cu] = GranTurismo2PC.func_8001861C,
            [0x80018690u] = GranTurismo2PC.func_80018690,
            [0x80018768u] = GranTurismo2PC.func_80018768,
            [0x800187DCu] = GranTurismo2PC.func_800187DC,
            [0x800188B0u] = GranTurismo2PC.func_800188B0,
            [0x80018954u] = GranTurismo2PC.func_80018954,
            [0x80018970u] = GranTurismo2PC.func_80018970,
            [0x800189D0u] = GranTurismo2PC.func_800189D0,
            [0x80018A00u] = GranTurismo2PC.func_80018A00,
            [0x80018A84u] = GranTurismo2PC.func_80018A84,
            [0x80018BF4u] = GranTurismo2PC.func_80018BF4,
            [0x80018C14u] = GranTurismo2PC.func_80018C14,
            [0x80018C8Cu] = GranTurismo2PC.func_80018C8C,
            [0x80018F6Cu] = GranTurismo2PC.func_80018F6C,
            [0x80018FA4u] = GranTurismo2PC.func_80018FA4,
            [0x80018FF0u] = GranTurismo2PC.func_80018FF0,
            [0x80019028u] = GranTurismo2PC.func_80019028,
            [0x8001907Cu] = GranTurismo2PC.func_8001907C,
            [0x800190E4u] = GranTurismo2PC.func_800190E4,
            [0x8001915Cu] = GranTurismo2PC.func_8001915C,
            [0x800191C4u] = GranTurismo2PC.func_800191C4,
            [0x8001924Cu] = GranTurismo2PC.func_8001924C,
            [0x8001928Cu] = GranTurismo2PC.func_8001928C,
            [0x80019474u] = GranTurismo2PC.func_80019474,
            [0x800194FCu] = GranTurismo2PC.func_800194FC,
            [0x80019538u] = GranTurismo2PC.func_80019538,
            [0x80019578u] = GranTurismo2PC.func_80019578,
            [0x800195BCu] = GranTurismo2PC.func_800195BC,
            [0x80019634u] = GranTurismo2PC.func_80019634,
            [0x800196C8u] = GranTurismo2PC.func_800196C8,
            [0x800196ECu] = GranTurismo2PC.func_800196EC,
            [0x80019718u] = GranTurismo2PC.func_80019718,
            [0x8001973Cu] = GranTurismo2PC.func_8001973C,
            [0x80019B88u] = GranTurismo2PC.func_80019B88,
            [0x80019BF0u] = GranTurismo2PC.func_80019BF0,
            [0x80019C60u] = GranTurismo2PC.func_80019C60,
            [0x80019C70u] = GranTurismo2PC.func_80019C70,
            [0x80019C80u] = GranTurismo2PC.func_80019C80,
            [0x80019CF4u] = GranTurismo2PC.func_80019CF4,
            [0x80019D38u] = GranTurismo2PC.func_80019D38,
            [0x80019DF8u] = GranTurismo2PC.func_80019DF8,
            [0x80019E1Cu] = GranTurismo2PC.func_80019E1C,
            [0x80019E2Cu] = GranTurismo2PC.func_80019E2C,
            [0x80019E98u] = GranTurismo2PC.func_80019E98,
            [0x80019EF8u] = GranTurismo2PC.func_80019EF8,
            [0x80019F5Cu] = GranTurismo2PC.func_80019F5C,
            [0x80019FA0u] = GranTurismo2PC.func_80019FA0,
            [0x80019FBCu] = GranTurismo2PC.func_80019FBC,
            [0x80019FE8u] = GranTurismo2PC.func_80019FE8,
            [0x8001A000u] = GranTurismo2PC.func_8001A000,
            [0x8001A0FCu] = GranTurismo2PC.func_8001A0FC,
            [0x8001A1B4u] = GranTurismo2PC.func_8001A1B4,
            [0x8001A24Cu] = GranTurismo2PC.func_8001A24C,
            [0x8001A454u] = GranTurismo2PC.func_8001A454,
            [0x8001A4ACu] = GranTurismo2PC.func_8001A4AC,
            [0x8001A504u] = GranTurismo2PC.func_8001A504,
            [0x8001A530u] = GranTurismo2PC.func_8001A530,
            [0x8001A5B8u] = GranTurismo2PC.func_8001A5B8,
            [0x8001A624u] = GranTurismo2PC.func_8001A624_gt2_overlay_4,
            [0x8001A654u] = GranTurismo2PC.func_8001A654,
            [0x8001A708u] = GranTurismo2PC.func_8001A708,
            [0x8001A8A4u] = GranTurismo2PC.func_8001A8A4,
            [0x8001AAF4u] = GranTurismo2PC.func_8001AAF4,
            [0x8001AB64u] = GranTurismo2PC.func_8001AB64,
            [0x8001AB6Cu] = GranTurismo2PC.func_8001AB6C,
            [0x8001AC20u] = GranTurismo2PC.func_8001AC20,
            [0x8001AEF8u] = GranTurismo2PC.func_8001AEF8,
            [0x8001AFA8u] = GranTurismo2PC.func_8001AFA8,
            [0x8001B10Cu] = GranTurismo2PC.func_8001B10C,
            [0x8001B2B8u] = GranTurismo2PC.func_8001B2B8,
            [0x8001B2F8u] = GranTurismo2PC.func_8001B2F8,
            [0x8001B338u] = GranTurismo2PC.func_8001B338,
            [0x8001B378u] = GranTurismo2PC.func_8001B378,
            [0x8001B3ACu] = GranTurismo2PC.func_8001B3AC,
            [0x8001B610u] = GranTurismo2PC.func_8001B610,
            [0x8001B63Cu] = GranTurismo2PC.func_8001B63C,
            [0x8001B680u] = GranTurismo2PC.func_8001B680_gt2_overlay_4,
            [0x8001B6F0u] = GranTurismo2PC.func_8001B6F0,
            [0x8001B818u] = GranTurismo2PC.func_8001B818,
            [0x8001B9ACu] = GranTurismo2PC.func_8001B9AC,
            [0x8001D090u] = GranTurismo2PC.func_8001D090,
            [0x8001D0E8u] = GranTurismo2PC.func_8001D0E8,
            [0x8001D104u] = GranTurismo2PC.func_8001D104,
            [0x8001D118u] = GranTurismo2PC.func_8001D118,
            [0x8001D208u] = GranTurismo2PC.func_8001D208,
            [0x8001D258u] = GranTurismo2PC.func_8001D258,
            [0x8001D2CCu] = GranTurismo2PC.func_8001D2CC,
            [0x8001D4E8u] = GranTurismo2PC.func_8001D4E8_gt2_overlay_4,
            [0x8001D554u] = GranTurismo2PC.func_8001D554,
            [0x8001D5C0u] = GranTurismo2PC.func_8001D5C0,
            [0x8001D5C8u] = GranTurismo2PC.func_8001D5C8,
            [0x8001D660u] = GranTurismo2PC.func_8001D660,
            [0x8001D680u] = GranTurismo2PC.func_8001D680,
            [0x8001D68Cu] = GranTurismo2PC.func_8001D68C,
            [0x8001D698u] = GranTurismo2PC.func_8001D698,
            [0x8001D6CCu] = GranTurismo2PC.func_8001D6CC,
            [0x8001D754u] = GranTurismo2PC.func_8001D754_gt2_overlay_4,
            [0x8001D954u] = GranTurismo2PC.func_8001D954,
            [0x8001DA24u] = GranTurismo2PC.func_8001DA24,
            [0x8001DA30u] = GranTurismo2PC.func_8001DA30,
            [0x8001DA38u] = GranTurismo2PC.func_8001DA38,
            [0x8001DA98u] = GranTurismo2PC.func_8001DA98,
            [0x8001DAA8u] = GranTurismo2PC.func_8001DAA8,
            [0x8001DAF0u] = GranTurismo2PC.func_8001DAF0,
            [0x8001DB0Cu] = GranTurismo2PC.func_8001DB0C,
            [0x8001DB24u] = GranTurismo2PC.func_8001DB24,
            [0x8001DB90u] = GranTurismo2PC.func_8001DB90_gt2_overlay_4,
            [0x8001DCD0u] = GranTurismo2PC.func_8001DCD0,
            [0x8001DD3Cu] = GranTurismo2PC.func_8001DD3C,
            [0x8001DDACu] = GranTurismo2PC.func_8001DDAC,
            [0x8001DFECu] = GranTurismo2PC.func_8001DFEC,
            [0x8001E22Cu] = GranTurismo2PC.func_8001E22C,
            [0x8001E26Cu] = GranTurismo2PC.func_8001E26C,
            [0x8001E328u] = GranTurismo2PC.func_8001E328,
            [0x8001E924u] = GranTurismo2PC.func_8001E924,
            [0x8001E9D0u] = GranTurismo2PC.func_8001E9D0,
            [0x8001EA24u] = GranTurismo2PC.func_8001EA24,
            [0x8001EAB8u] = GranTurismo2PC.func_8001EAB8,
            [0x8001EC0Cu] = GranTurismo2PC.func_8001EC0C,
            [0x8001EDACu] = GranTurismo2PC.func_8001EDAC,
            [0x8001EE78u] = GranTurismo2PC.func_8001EE78,
            [0x8001EF10u] = GranTurismo2PC.func_8001EF10,
            [0x8001F124u] = GranTurismo2PC.func_8001F124,
            [0x8001F210u] = GranTurismo2PC.func_8001F210,
            [0x8001F2C8u] = GranTurismo2PC.func_8001F2C8,
            [0x8001F38Cu] = GranTurismo2PC.func_8001F38C,
            [0x8001F4B0u] = GranTurismo2PC.func_8001F4B0,
            [0x8001F520u] = GranTurismo2PC.func_8001F520,
            [0x8001F65Cu] = GranTurismo2PC.func_8001F65C,
            [0x8001F6CCu] = GranTurismo2PC.func_8001F6CC,
            [0x8001F740u] = GranTurismo2PC.func_8001F740,
            [0x8001F7CCu] = GranTurismo2PC.func_8001F7CC,
            [0x8001F8FCu] = GranTurismo2PC.func_8001F8FC,
            [0x8001F96Cu] = GranTurismo2PC.func_8001F96C,
            [0x8001FAB4u] = GranTurismo2PC.func_8001FAB4,
            [0x8001FB24u] = GranTurismo2PC.func_8001FB24,
            [0x8001FB7Cu] = GranTurismo2PC.func_8001FB7C,
            [0x8001FBECu] = GranTurismo2PC.func_8001FBEC,
            [0x8001FC28u] = GranTurismo2PC.func_8001FC28,
            [0x8001FC64u] = GranTurismo2PC.func_8001FC64,
            [0x8001FCA0u] = GranTurismo2PC.func_8001FCA0,
            [0x8001FCDCu] = GranTurismo2PC.func_8001FCDC,
            [0x8001FE0Cu] = GranTurismo2PC.func_8001FE0C,
            [0x8001FFCCu] = GranTurismo2PC.func_8001FFCC,
            [0x80020054u] = GranTurismo2PC.func_80020054,
            [0x80020110u] = GranTurismo2PC.func_80020110_gt2_overlay_4,
            [0x80020198u] = GranTurismo2PC.func_80020198,
            [0x80020490u] = GranTurismo2PC.func_80020490,
            [0x800204D8u] = GranTurismo2PC.func_800204D8,
            [0x800204ECu] = GranTurismo2PC.func_800204EC,
            [0x8002055Cu] = GranTurismo2PC.func_8002055C,
            [0x8002068Cu] = GranTurismo2PC.func_8002068C,
            [0x800206F8u] = GranTurismo2PC.func_800206F8,
            [0x800209B0u] = GranTurismo2PC.func_800209B0,
            [0x800209F8u] = GranTurismo2PC.func_800209F8,
            [0x80020A0Cu] = GranTurismo2PC.func_80020A0C,
            [0x80020A94u] = GranTurismo2PC.func_80020A94,
            [0x80020B70u] = GranTurismo2PC.func_80020B70,
            [0x80020BDCu] = GranTurismo2PC.func_80020BDC,
            [0x80020C00u] = GranTurismo2PC.func_80020C00,
            [0x80020C24u] = GranTurismo2PC.func_80020C24,
            [0x80020C50u] = GranTurismo2PC.func_80020C50,
            [0x80020D48u] = GranTurismo2PC.func_80020D48,
            [0x80020D58u] = GranTurismo2PC.func_80020D58,
            [0x80020D90u] = GranTurismo2PC.func_80020D90_gt2_overlay_4,
            [0x80020E4Cu] = GranTurismo2PC.func_80020E4C,
            [0x80020ECCu] = GranTurismo2PC.func_80020ECC,
            [0x80020F54u] = GranTurismo2PC.func_80020F54,
            [0x80020FBCu] = GranTurismo2PC.func_80020FBC,
            [0x80020FF0u] = GranTurismo2PC.func_80020FF0,
            [0x80021078u] = GranTurismo2PC.func_80021078,
            [0x800210F8u] = GranTurismo2PC.func_800210F8,
            [0x8002117Cu] = GranTurismo2PC.func_8002117C,
            [0x800211FCu] = GranTurismo2PC.func_800211FC,
            [0x80021284u] = GranTurismo2PC.func_80021284,
            [0x8002136Cu] = GranTurismo2PC.func_8002136C,
            [0x800213C4u] = GranTurismo2PC.func_800213C4,
            [0x8002150Cu] = GranTurismo2PC.func_8002150C,
            [0x800215B0u] = GranTurismo2PC.func_800215B0,
            [0x800215C8u] = GranTurismo2PC.func_800215C8,
            [0x80021608u] = GranTurismo2PC.func_80021608,
            [0x800218BCu] = GranTurismo2PC.func_800218BC,
            [0x800218DCu] = GranTurismo2PC.func_800218DC,
            [0x80021968u] = GranTurismo2PC.func_80021968,
            [0x80021A40u] = GranTurismo2PC.func_80021A40,
            [0x80021A6Cu] = GranTurismo2PC.func_80021A6C,
            [0x80021B0Cu] = GranTurismo2PC.func_80021B0C,
            [0x80021B38u] = GranTurismo2PC.func_80021B38,
            [0x80021BECu] = GranTurismo2PC.func_80021BEC,
            [0x80021C38u] = GranTurismo2PC.func_80021C38,
            [0x80021C5Cu] = GranTurismo2PC.func_80021C5C,
            [0x80021CB4u] = GranTurismo2PC.func_80021CB4,
            [0x80021D00u] = GranTurismo2PC.func_80021D00,
            [0x80021D30u] = GranTurismo2PC.func_80021D30,
            [0x80021DE8u] = GranTurismo2PC.func_80021DE8,
            [0x80021EB0u] = GranTurismo2PC.func_80021EB0,
            [0x80021F88u] = GranTurismo2PC.func_80021F88,
            [0x8002202Cu] = GranTurismo2PC.func_8002202C,
            [0x800220C8u] = GranTurismo2PC.func_800220C8,
            [0x80022278u] = GranTurismo2PC.func_80022278,
            [0x800222E4u] = GranTurismo2PC.func_800222E4,
            [0x80022368u] = GranTurismo2PC.func_80022368,
            [0x8002237Cu] = GranTurismo2PC.func_8002237C,
            [0x800223B0u] = GranTurismo2PC.func_800223B0,
            [0x800223C4u] = GranTurismo2PC.func_800223C4,
            [0x800224B0u] = GranTurismo2PC.func_800224B0,
            [0x800224C4u] = GranTurismo2PC.func_800224C4,
            [0x800224D4u] = GranTurismo2PC.func_800224D4,
            [0x800224E0u] = GranTurismo2PC.func_800224E0,
            [0x80022634u] = GranTurismo2PC.func_80022634,
            [0x800226A0u] = GranTurismo2PC.func_800226A0,
            [0x800226F4u] = GranTurismo2PC.func_800226F4,
            [0x80022714u] = GranTurismo2PC.func_80022714,
            [0x80022758u] = GranTurismo2PC.func_80022758,
            [0x80022794u] = GranTurismo2PC.func_80022794,
            [0x80022838u] = GranTurismo2PC.func_80022838,
            [0x80022874u] = GranTurismo2PC.func_80022874,
            [0x800228D4u] = GranTurismo2PC.func_800228D4,
            [0x80022934u] = GranTurismo2PC.func_80022934,
        };
}
