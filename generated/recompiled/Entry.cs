using RecompOne.Runtime.Cdrom;
using RecompOne.Runtime.Context;
using RecompOne.Runtime.Dispatch;
using RecompOne.Runtime.Memory;
using BiosKernel = RecompOne.Runtime.Bios.Bios;

namespace Recompiled.Simulation;

public static class Entry
{
    public static void Run(IMemory m, string? cuePath = null, string? loosePath = null)
    {
        RecompOne.Runtime.Runtime.Initialize("Gran Turismo 2 PC");
        loosePath ??= RecompOne.Runtime.Runtime.ResolveLoosePath();
        if (loosePath != null)
            RecompOne.Runtime.OggMusic.Initialize(loosePath);
        if (loosePath == null && cuePath == null)
        {
            RecompOne.Runtime.Runtime.WaitForValidDisc();
            cuePath = RecompOne.Runtime.Runtime.CdPath;
        }
        using var fs = loosePath != null
            ? CueFs.OpenLoose(loosePath)
            : CueFs.Open(cuePath!);
        var cd = new CdController(fs, m);
        m.SetCd(cd);
        Dispatcher.Register("main", new MainDispatchTable());
        Dispatcher.Register("gt2_overlay_0", new Gt2_overlay_0DispatchTable());
        Dispatcher.Register("gt2_overlay_1", new Gt2_overlay_1DispatchTable());
        Dispatcher.Register("gt2_overlay_2", new Gt2_overlay_2DispatchTable());
        Dispatcher.Register("gt2_overlay_3", new Gt2_overlay_3DispatchTable());
        Dispatcher.Register("gt2_overlay_4", new Gt2_overlay_4DispatchTable());
        Dispatcher.Register("gt2_overlay_5", new Gt2_overlay_5DispatchTable());
        RecompOne.Runtime.Modding.ModLoader.LoadAll();
        cd.LoadToMemory("SCUS_944.88", 0x80010000u, 0x800, 626688);
        Dispatcher.Load("main");
        var c = new CpuContext();
        c.GP = 0x00000000u;
        c.SP = 0x801FFF00u;
        c.FP = c.SP;
        c.RA = 0u;
        RecompOne.Runtime.Runtime.SetContext(c, m);
        BiosKernel.Init(m);
        RecompOne.Runtime.Sdk.GT2Compat.RunGuestLoop(
            c, m, 0x8005D600u);
    }
}
