using RecompOne.Runtime.Cdrom;
using RecompOne.Runtime.Context;
using RecompOne.Runtime.Dispatch;
using RecompOne.Runtime.Memory;
using RecompOne.Runtime.Sdk;
using BiosKernel = RecompOne.Runtime.Bios.Bios;

internal static class UnifiedEntry
{
    public static void Run(
        IMemory memory,
        string looseRoot,
        string initialVariant = "simulation")
    {
        GT2Compat.ConfigureUnifiedTitlePanels(
            Path.Combine(looseRoot, "TITLE_EXACT.DAT"));
        RecompOne.Runtime.Runtime.Initialize("Gran Turismo 2 PC");
        RecompOne.Runtime.OggMusic.Initialize(looseRoot);
        RecompOne.Runtime.Modding.ModLoader.LoadAll();

        string variant = initialVariant.Equals(
            "arcade", StringComparison.OrdinalIgnoreCase)
            ? "arcade"
            : "simulation";
        bool seamlessArcadeHandoff = false;
        bool seamlessSimulationHandoff = false;
        while (true)
        {
            string manifest = Path.Combine("manifests", $"{variant}.json");
            Environment.SetEnvironmentVariable(
                "RECOMPONE_LOOSE_MANIFEST", manifest);
            Console.WriteLine(
                $"[Host] native unified guest={variant} data={looseRoot}");

            bool traceArcadeHandoff =
                variant.Equals("arcade", StringComparison.OrdinalIgnoreCase) &&
                seamlessArcadeHandoff;
            long handoffPhaseStarted = System.Diagnostics.Stopwatch.GetTimestamp();
            Dispatcher.ResetGuestImages();
            BiosKernel.ResetGuestState();
            handoffPhaseStarted = ReportHandoffPhase(
                traceArcadeHandoff, "guest-reset", handoffPhaseStarted);
            using var fs = CueFs.OpenLoose(looseRoot);
            handoffPhaseStarted = ReportHandoffPhase(
                traceArcadeHandoff, "disc-open", handoffPhaseStarted);
            var cd = new CdController(fs, memory);
            memory.SetCd(cd);

            bool arcade = variant.Equals(
                "arcade", StringComparison.OrdinalIgnoreCase);
            // --start-arcade is a standalone diagnostic boot and must retain
            // the stock Arcade-disc sequence. Only a selector-driven switch
            // from the unified Simulation title enables the seamless path.
            GT2Compat.SetUnifiedArcadeTransition(
                arcade && seamlessArcadeHandoff);
            if (arcade)
                RegisterArcade();
            else
                RegisterSimulation();
            handoffPhaseStarted = ReportHandoffPhase(
                traceArcadeHandoff, "dispatch-register", handoffPhaseStarted);

            cd.LoadToMemory(
                arcade ? "SCUS_944.55" : "SCUS_944.88",
                0x80010000u, 0x800, 626688);
            handoffPhaseStarted = ReportHandoffPhase(
                traceArcadeHandoff, "executable-read", handoffPhaseStarted);
            Dispatcher.Load("main");
            handoffPhaseStarted = ReportHandoffPhase(
                traceArcadeHandoff, "main-map", handoffPhaseStarted);

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
            handoffPhaseStarted = ReportHandoffPhase(
                traceArcadeHandoff, "bios-cd-reset", handoffPhaseStarted);

            try
            {
                if (arcade && seamlessArcadeHandoff)
                {
                    // The Simulation title has already supplied the unified
                    // legal/title presentation. Prepare the Arcade program's
                    // own BSS and runtime services, then enter the same native
                    // overlay requested by START GAME. Do not rerun Arcade's
                    // executable bootstrap or opening/title overlay.
                    GT2Compat.PrepareArcadeFrontendHandoff(context, memory);
                    GT2Compat.RunArcadeFrontendHandoff(context, memory);
                }
                else if (!arcade && seamlessSimulationHandoff)
                {
                    // Arcade's native Back destination is its own disc title.
                    // The unified product replaces that skipped screen with
                    // the original Simulation title menu, without replaying
                    // either disc's boot/legal presentation.
                    GT2Compat.PrepareSimulationTitleHandoff(context, memory);
                    GT2Compat.RunSimulationTitleHandoff(context, memory);
                }
                else
                {
                    GT2Compat.RunGuestLoop(
                        context,
                        memory,
                        arcade ? 0x8005D570u : 0x8005D600u,
                        arcade ? "gt2_arcade_overlay" : "gt2_overlay");
                }
                return;
            }
            catch (GT2VariantSwitch requested)
            {
                if (requested.Variant.Equals(
                        "arcade", StringComparison.OrdinalIgnoreCase))
                {
                    GT2Compat.CompleteUnifiedTitleConfirmationAudio();
                    GT2Compat.PreserveUnifiedArcadeProgress(memory);
                    variant = "arcade";
                    seamlessArcadeHandoff = true;
                    seamlessSimulationHandoff = false;
                    Console.WriteLine(
                        "[Host] seamless guest handoff: " +
                        "Simulation title -> Arcade Mode menu");
                }
                else if (requested.Variant.Equals(
                             "simulation", StringComparison.OrdinalIgnoreCase) ||
                         requested.Variant.Equals(
                             "simulation-title", StringComparison.OrdinalIgnoreCase))
                {
                    variant = "simulation";
                    seamlessArcadeHandoff = false;
                    seamlessSimulationHandoff = true;
                    string source = requested.Variant.Equals(
                        "simulation-title", StringComparison.OrdinalIgnoreCase)
                            ? "Gran Turismo Mode Back"
                            : "Arcade Mode Back";
                    Console.WriteLine(
                        "[Host] seamless guest handoff: " +
                        $"{source} -> unified title");
                }
                else
                {
                    throw;
                }
            }
        }
    }

    static long ReportHandoffPhase(bool enabled, string phase, long started)
    {
        long finished = System.Diagnostics.Stopwatch.GetTimestamp();
        if (enabled)
        {
            Console.WriteLine(
                $"[Host-Handoff] phase={phase} " +
                $"elapsedMs={System.Diagnostics.Stopwatch.GetElapsedTime(started, finished).TotalMilliseconds:F3}");
        }
        return finished;
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
