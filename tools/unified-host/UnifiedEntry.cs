using RecompOne.Runtime.Cdrom;
using RecompOne.Runtime.Context;
using RecompOne.Runtime.Dispatch;
using RecompOne.Runtime.Memory;
using RecompOne.Runtime.Sdk;
using BiosKernel = RecompOne.Runtime.Bios.Bios;

internal static class UnifiedEntry
{
    public static void Run(IMemory memory, string looseRoot)
    {
        RecompOne.Runtime.Runtime.Initialize("Gran Turismo 2 PC");
        RecompOne.Runtime.OggMusic.Initialize(looseRoot);
        RecompOne.Runtime.Modding.ModLoader.LoadAll();

        string variant = "simulation";
        while (true)
        {
            string manifest = Path.Combine("manifests", $"{variant}.json");
            Environment.SetEnvironmentVariable(
                "RECOMPONE_LOOSE_MANIFEST", manifest);
            Console.WriteLine(
                $"[Host] native unified guest={variant} data={looseRoot}");

            Dispatcher.ResetGuestImages();
            BiosKernel.ResetGuestState();
            using var fs = CueFs.OpenLoose(looseRoot);
            var cd = new CdController(fs, memory);
            memory.SetCd(cd);

            bool arcade = variant.Equals(
                "arcade", StringComparison.OrdinalIgnoreCase);
            GT2Compat.SetUnifiedArcadeTransition(arcade);
            if (arcade)
                RegisterArcade();
            else
                RegisterSimulation();

            cd.LoadToMemory(
                arcade ? "SCUS_944.55" : "SCUS_944.88",
                0x80010000u, 0x800, 626688);
            Dispatcher.Load("main");

            var context = new CpuContext
            {
                GP = 0,
                SP = 0x801FFF00u,
            };
            context.FP = context.SP;
            context.RA = 0;
            RecompOne.Runtime.Runtime.SetContext(context, memory);
            BiosKernel.Init(memory);
            RecompOne.Runtime.Sdk.LibCd.CdReset(context, memory);

            try
            {
                GT2Compat.RunGuestLoop(
                    context,
                    memory,
                    arcade ? 0x8005D570u : 0x8005D600u,
                    arcade ? "gt2_arcade_overlay" : "gt2_overlay");
                return;
            }
            catch (GT2VariantSwitch requested)
            {
                if (!requested.Variant.Equals(
                        "arcade", StringComparison.OrdinalIgnoreCase))
                    throw;
                variant = "arcade";
                Console.WriteLine(
                    "[Host] guest-controlled transition: " +
                    "Simulation title -> Arcade Mode");
            }
        }
    }

    static void RegisterSimulation()
    {
        Dispatcher.Register(
            "main", new Recompiled.Simulation.MainDispatchTable());
        Dispatcher.Register(
            "gt2_overlay_0",
            new Recompiled.Simulation.Gt2_overlay_0DispatchTable());
        Dispatcher.Register(
            "gt2_overlay_1",
            new Recompiled.Simulation.Gt2_overlay_1DispatchTable());
        Dispatcher.Register(
            "gt2_overlay_2",
            new Recompiled.Simulation.Gt2_overlay_2DispatchTable());
        Dispatcher.Register(
            "gt2_overlay_3",
            new Recompiled.Simulation.Gt2_overlay_3DispatchTable());
        Dispatcher.Register(
            "gt2_overlay_4",
            new Recompiled.Simulation.Gt2_overlay_4DispatchTable());
        Dispatcher.Register(
            "gt2_overlay_5",
            new Recompiled.Simulation.Gt2_overlay_5DispatchTable());
    }

    static void RegisterArcade()
    {
        Dispatcher.Register(
            "main", new Recompiled.Arcade.MainDispatchTable());
        Dispatcher.Register(
            "gt2_arcade_overlay_0",
            new Recompiled.Arcade.Gt2_arcade_overlay_0DispatchTable());
        Dispatcher.Register(
            "gt2_arcade_overlay_1",
            new Recompiled.Arcade.Gt2_arcade_overlay_1DispatchTable());
        Dispatcher.Register(
            "gt2_arcade_overlay_2",
            new Recompiled.Arcade.Gt2_arcade_overlay_2DispatchTable());
        Dispatcher.Register(
            "gt2_arcade_overlay_3",
            new Recompiled.Arcade.Gt2_arcade_overlay_3DispatchTable());
        Dispatcher.Register(
            "gt2_arcade_overlay_4",
            new Recompiled.Arcade.Gt2_arcade_overlay_4DispatchTable());
        Dispatcher.Register(
            "gt2_arcade_overlay_5",
            new Recompiled.Arcade.Gt2_arcade_overlay_5DispatchTable());
    }
}
