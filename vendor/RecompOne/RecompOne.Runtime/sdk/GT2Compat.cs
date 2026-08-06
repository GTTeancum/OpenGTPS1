using System.Buffers.Binary;
using System.Threading;
using RecompOne.Runtime.Context;
using RecompOne.Runtime.Memory;

namespace RecompOne.Runtime.Sdk;

public sealed class GT2VariantSwitch(string variant) : Exception
{
    public string Variant { get; } = variant;
}

/// <summary>
/// Narrow host bridges proven against Gran Turismo 2 SCUS-94488.
/// </summary>
public static class GT2Compat
{
    readonly record struct LiverySelection(
        uint BodyId, byte BodyPaletteIndex, byte ColorId);

    private sealed class NonLocalJump(
        uint target, bool returnTrampoline = false) : Exception
    {
        public uint Target { get; } = target;
        public bool ReturnTrampoline { get; } = returnTrampoline;
    }

    public static bool ExpandedPolygonBuffersEnabled =>
        Config.ConfigManager.View.ExtendedDrawDistance ||
        Config.ConfigManager.View.LevelOfDetail.Equals(
            "Maximum", StringComparison.OrdinalIgnoreCase);

    // Internal regression-isolation switches default to enabled. Setting one
    // to 0 separates the radial and replay distance gates during capture
    // without changing the user-facing Extended Draw Distance option.
    static bool ExtendedTrackFeatureEnabled(string overrideName) =>
        Config.ConfigManager.View.ExtendedDrawDistance &&
        Environment.GetEnvironmentVariable(overrideName) != "0";

    public static bool ExtendedReplayTrackDrawDistanceEnabled =>
        ExtendedTrackFeatureEnabled(
            "RECOMPONE_GT2_EXTENDED_REPLAY_TRACK_DRAW");

    /// <summary>
    /// Overlay 0 applies a second, camera-relative radial cutoff after walking
    /// the current sector visibility list. Replacing that list alone therefore
    /// cannot extend draw distance: distant objects remain absent until they
    /// cross the stock 0x0063FFFF threshold, which exposes whole section
    /// boundaries as visible pop-in. Disable only this redundant radial cutoff
    /// when Extended Draw Distance is selected; retain the authored
    /// current-sector potential-visibility set. Frustum/near-plane rejection
    /// and the game's ordinary polygon clipping still run unchanged.
    /// </summary>
    public static uint GetTrackDrawDistanceLimit() =>
        ExtendedTrackFeatureEnabled(
            "RECOMPONE_GT2_EXTENDED_TRACK_RADIAL_LIMIT")
            ? uint.MaxValue
            : 0x0063FFFFu;

    static readonly bool TraceBoot =
        Environment.GetEnvironmentVariable("RECOMPONE_TRACE_GT2_BOOT") == "1";
    static readonly bool TraceMenu =
        Environment.GetEnvironmentVariable("RECOMPONE_TRACE_GT2_MENU") == "1";
    static readonly bool TraceRaceScheduler =
        Environment.GetEnvironmentVariable("RECOMPONE_TRACE_GT2_SCHEDULER") == "1";
    static readonly bool TraceTrackRendering =
        Environment.GetEnvironmentVariable("RECOMPONE_TRACE_GT2_TRACK_RENDERING") == "1";
    static readonly bool TraceVehicleLod =
        Environment.GetEnvironmentVariable("RECOMPONE_TRACE_GT2_VEHICLE_LOD") == "1";
    static readonly bool TraceAiDrivers =
        Environment.GetEnvironmentVariable("RECOMPONE_TRACE_GT2_AI_DRIVERS") == "1";
    static readonly bool TraceLiveries =
        Environment.GetEnvironmentVariable("RECOMPONE_TRACE_GT2_LIVERIES") == "1";
    static readonly bool AiAutoDrive =
        Environment.GetEnvironmentVariable("RECOMPONE_GT2_AI_AUTODRIVE") == "1";
    static readonly int AiAutoDriveMaxEngagements =
        ParseAiAutoDriveMaxEngagements(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_GT2_AI_AUTODRIVE_MAX_ENGAGEMENTS"));
    static readonly int AiAutoDriveQuickWinAfterTicks =
        ParseAiAutoDriveQuickWinAfterTicks(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_GT2_SOAK_QUICK_WIN_AFTER_AI_TICKS"));
    static readonly uint DiagnosticVehicleLodSelector =
        ParseVehicleLodSelector(
            Environment.GetEnvironmentVariable(
                "RECOMPONE_GT2_VEHICLE_LOD_SELECTOR_OVERRIDE"));
    static readonly HashSet<string> RaceSchedulerStates = [];
    static readonly HashSet<uint> TrackObjects = [];
    static readonly HashSet<uint> TrackViewIndices = [];
    static readonly HashSet<uint> TrackMeshIndices = [];
    static readonly HashSet<uint> TrackPasses = [];
    static readonly Dictionary<int, long> ReplayTrackSelectorCounts = [];
    static readonly HashSet<uint> ReplayTrackModelPointers = [];
    static readonly HashSet<uint> ReplayTrackModelIds = [];
    static long _trackRenderRequests;
    static int _trackRenderSamples;
    static bool _trackExitTraceRegistered;
    static readonly HashSet<uint> TrackVisibilitySectors = [];
    static long _visibilityLodRaceCalls;
    static long _visibilityLodReplayCalls;
    static long _visibilityLodMaximumCalls;
    static long _visibilityLodStockCalls;
    static long _visibilityLodEntriesScanned;
    static long _visibilityLodNonzeroSelectors;
    static int _visibilityLodExitTraceRegistered;
    static readonly Dictionary<uint, long> VehicleLodSelectorCounts = [];
    static readonly HashSet<uint> VehicleLodModelSets = [];
    static readonly HashSet<uint> VehicleLodModelPointers = [];
    const uint ExpandedVisibilityListAddress = 0x807F0000u;
    static int _bootObjectTraceCount;
    static int _overlayLoadTraceCount;
    static int _finiteCdReadTraceCount;
    static int _menuListTraceCount;
    static uint _overlayIndex;
    static string _overlayPrefix = "gt2_overlay";
    static bool _unifiedTitleInstalled;
    static bool _unifiedArcadeTransition;
    static readonly object LiveryTableLock = new();
    static Dictionary<ulong, LiverySelection>? _liveriesByPalette;
    static Dictionary<ulong, LiverySelection>? _liveriesByColorId;
    static Dictionary<uint, uint>? _liveryChoiceCounts;
    static int _liveryPaletteTraceCount;
    static int _liveryColorTraceCount;

    static ulong LiveryKey(uint bodyId, uint selector) =>
        ((ulong)bodyId << 32) | selector;

    static void EnsureLiveryTable()
    {
        if (Volatile.Read(ref _liveriesByPalette) != null)
            return;
        lock (LiveryTableLock)
        {
            if (Volatile.Read(ref _liveriesByPalette) != null)
                return;

            var byPalette = new Dictionary<ulong, LiverySelection>();
            var byColorId = new Dictionary<ulong, LiverySelection>();
            var choiceCounts = new Dictionary<uint, uint>();
            var ambiguousColorIds = new HashSet<ulong>();
            string? root = Runtime.ResolveLoosePath();
            string? path = root == null
                ? null
                : Path.Combine(root, "GTLIVERY.BIN");
            if (path != null && File.Exists(path))
            {
                byte[] data = File.ReadAllBytes(path);
                if (data.Length < 8 ||
                    !data.AsSpan(0, 4).SequenceEqual("GTLV"u8))
                    throw new InvalidDataException(
                        $"Invalid GT2 livery table header: {path}");
                ushort version = BinaryPrimitives.ReadUInt16LittleEndian(
                    data.AsSpan(4, 2));
                ushort count = BinaryPrimitives.ReadUInt16LittleEndian(
                    data.AsSpan(6, 2));
                if (version != 3 || data.Length != 8 + count * 12)
                    throw new InvalidDataException(
                        "Unsupported GT2 livery table: " +
                        $"version={version}, records={count}, " +
                        $"bytes={data.Length}");

                for (int index = 0; index < count; index++)
                {
                    int offset = 8 + index * 12;
                    uint targetBody =
                        BinaryPrimitives.ReadUInt32LittleEndian(
                            data.AsSpan(offset, 4));
                    uint alternateBody =
                        BinaryPrimitives.ReadUInt32LittleEndian(
                            data.AsSpan(offset + 4, 4));
                    byte targetPalette = data[offset + 8];
                    byte bodyPalette = data[offset + 9];
                    ushort encodedColorId =
                        BinaryPrimitives.ReadUInt16LittleEndian(
                            data.AsSpan(offset + 10, 2));
                    if (encodedColorId > byte.MaxValue)
                        throw new InvalidDataException(
                            $"GT2 livery record {index} has an invalid color ID");
                    var selection = new LiverySelection(
                        alternateBody, bodyPalette, (byte)encodedColorId);
                    if (!byPalette.TryAdd(
                            LiveryKey(targetBody, targetPalette), selection))
                        throw new InvalidDataException(
                            $"GT2 livery record {index} is duplicated");
                    choiceCounts[targetBody] = Math.Max(
                        choiceCounts.GetValueOrDefault(targetBody),
                        (uint)targetPalette + 1);
                    ulong colorKey =
                        LiveryKey(targetBody, encodedColorId);
                    if (!ambiguousColorIds.Contains(colorKey))
                    {
                        if (byColorId.TryGetValue(
                                colorKey, out var existing) &&
                            existing != selection)
                        {
                            // Retail assumes a color ID uniquely identifies a
                            // palette. GT1's alternate Castrol Supra packages
                            // disprove that assumption: the same authored ID
                            // selects different native bodies. Palette-index
                            // hooks retain the exact choice; ID-only fallback
                            // deliberately becomes a no-op for ambiguity.
                            byColorId.Remove(colorKey);
                            ambiguousColorIds.Add(colorKey);
                        }
                        else
                        {
                            byColorId[colorKey] = selection;
                        }
                    }
                }
                Console.WriteLine(
                    "[GT2] native alternate-livery table loaded: " +
                    $"{count} body/palette mappings, " +
                    $"{ambiguousColorIds.Count} ambiguous color IDs");
            }
            _liveryChoiceCounts = choiceCounts;
            _liveriesByColorId = byColorId;
            // Publish the palette table last. Readers use it as the
            // initialization sentinel, so observing it also makes the
            // color-ID table and all populated dictionary entries visible.
            Volatile.Write(ref _liveriesByPalette, byPalette);
        }
    }

    /// <summary>
    /// Resolve an added color choice to its native alternate car-object body
    /// and that body's original palette. The low 32 bits are the body ID and
    /// the high 32 bits are the palette index.
    /// </summary>
    public static ulong ResolveLiveryBodyAndPalette(
        uint bodyId, uint paletteIndex)
    {
        EnsureLiveryTable();
        var byPalette = Volatile.Read(ref _liveriesByPalette)!;
        bool mapped = byPalette.TryGetValue(
            LiveryKey(bodyId, paletteIndex), out var selection);
        uint resolvedBody = mapped ? selection.BodyId : bodyId;
        uint resolvedPalette = mapped
            ? selection.BodyPaletteIndex
            : paletteIndex;
        if (TraceLiveries &&
            Interlocked.Increment(ref _liveryPaletteTraceCount) <= 256)
            Console.WriteLine(
                "[GT2-Livery] palette " +
                $"body=0x{bodyId:X8} index={paletteIndex} " +
                $"mapped={mapped} -> body=0x{resolvedBody:X8} " +
                $"index={resolvedPalette}");
        return resolvedBody | ((ulong)resolvedPalette << 32);
    }

    public static void ResolveLiveryBodyAndPaletteA0S2(CpuContext c)
    {
        ulong resolved = ResolveLiveryBodyAndPalette(c.A0, c.S2);
        c.A0 = (uint)resolved;
        c.S2 = (uint)(resolved >> 32);
    }

    public static void ResolveLiveryBodyAndPaletteA1A2(CpuContext c)
    {
        ulong resolved = ResolveLiveryBodyAndPalette(c.A1, c.A2);
        c.A1 = (uint)resolved;
        c.A2 = (uint)(resolved >> 32);
    }

    /// <summary>
    /// Resolve only the body half of a palette-index choice while a frontend
    /// record is being authored. Keeping that resolved body in the record
    /// preserves packages that intentionally reuse another package's color ID.
    /// </summary>
    public static uint ResolveLiveryBodyForPalette(
        uint bodyId, uint paletteIndex) =>
        (uint)ResolveLiveryBodyAndPalette(bodyId, paletteIndex);

    public static uint ResolveLiveryChoiceCount(
        uint bodyId, uint nativeCount)
    {
        EnsureLiveryTable();
        return Math.Max(
            nativeCount,
            _liveryChoiceCounts!.GetValueOrDefault(bodyId));
    }

    /// <summary>
    /// Frontend records carry the database color ID rather than its palette
    /// slot. Swap only the native body here; the original frontend then finds
    /// that same color ID in the hidden body's carinfo record and derives its
    /// correct local palette normally.
    /// </summary>
    public static uint ResolveLiveryBodyForColorId(
        uint bodyId, uint colorId)
    {
        EnsureLiveryTable();
        bool mapped = _liveriesByColorId!.TryGetValue(
            LiveryKey(bodyId, colorId), out var selection);
        uint resolvedBody = mapped ? selection.BodyId : bodyId;
        if (TraceLiveries &&
            Interlocked.Increment(ref _liveryColorTraceCount) <= 256)
            Console.WriteLine(
                "[GT2-Livery] color " +
                $"body=0x{bodyId:X8} id={colorId} mapped={mapped} " +
                $"-> body=0x{resolvedBody:X8}");
        return resolvedBody;
    }

    const uint UnifiedTitleList = 0x8004BC28u;
    const uint UnifiedArcadeDescriptor = 0x803FF000u;
    const uint UnifiedGtDescriptor = UnifiedArcadeDescriptor + 0xCu;

    public static bool ArcadeVariant =>
        _overlayPrefix.Equals(
            "gt2_arcade_overlay", StringComparison.Ordinal);

    public static uint CdDriveStateAddress =>
        ArcadeVariant ? 0x801EFF00u : 0x801F0510u;

    public static uint CdReadyCallbackAddress =>
        ArcadeVariant ? 0x8007CD04u : 0x8007CDF4u;

    public static uint CdFiniteReadHandlerAddress =>
        ArcadeVariant ? 0x8007DE28u : 0x8007DF18u;

    public static void SetUnifiedArcadeTransition(bool enabled) =>
        _unifiedArcadeTransition = enabled;

    /// <summary>
    /// A unified-menu handoff has already shown the Simulation-disc legal and
    /// opening presentation.  Preserve Arcade's normal bootstrap, but ask its
    /// original overlay loader to enter the native Arcade frontend directly.
    /// Standalone diagnostics retain the stock Arcade-disc opening overlay.
    /// </summary>
    public static uint InitialArcadeOverlayIndex() =>
        _unifiedArcadeTransition ? 1u : 5u;

    /// <summary>
    /// Extend the original Simulation-disc title list in guest memory.  The
    /// list engine, cursor, arrows, fading and draw path remain GT2's; only its
    /// item count and two additional native TIM descriptors are supplied here.
    /// </summary>
    public static void InstallUnifiedTitleMenu(IMemory m)
    {
        m.WriteU16(UnifiedTitleList, 9);
        ushort clut = m.ReadU16(0x8004BA52u);
        WriteTitleDescriptor(
            m, UnifiedArcadeDescriptor, u: 0, v: 24,
            width: 132, height: 22, tpage: 12, clut);
        WriteTitleDescriptor(
            m, UnifiedGtDescriptor, u: 0, v: 48,
            width: 177, height: 22, tpage: 12, clut);

        if (_unifiedTitleInstalled)
            return;
        _unifiedTitleInstalled = true;
        string palette = Runtime.Gpu == null
            ? "unavailable"
            : string.Join(
                ',',
                Enumerable.Range(0, 16).Select(index =>
                    $"{Runtime.Gpu.Vram[252 * 1024 + 848 + index]:X4}"));
        Console.WriteLine(
            "[GT2] native unified title menu installed: " +
            "Arcade Mode, Gran Turismo Mode; " +
            $"language={m.ReadU8(0x801C98E0u)} clut=0x{clut:X4} " +
            $"palette={palette}");
    }

    static void WriteTitleDescriptor(
        IMemory m, uint address, byte u, byte v,
        ushort width, ushort height, ushort tpage, ushort clut)
    {
        m.WriteU16(address, (ushort)(u | (v << 8)));
        m.WriteU16(address + 2u, clut);
        m.WriteU16(address + 4u, width);
        m.WriteU16(address + 6u, height);
        m.WriteU16(address + 8u, tpage);
        m.WriteU16(address + 10u, 0);
    }

    static int MapUnifiedTitleIndex(int index) => index switch
    {
        0 => 0,
        1 => 1,
        2 => 1,
        >= 3 and <= 8 => index - 1,
        _ => -1,
    };

    public static int UnifiedTitleSelectionValue(uint index)
    {
        int item = (int)index;
        if (item is 0 or 8)
            return -1;
        if (item is 1 or 2)
            return 0;
        return item is >= 3 and <= 7 ? item - 2 : -1;
    }

    public static uint UnifiedTitleDescriptor(
        IMemory m, uint index, uint language)
    {
        if (index == 1u)
            return UnifiedArcadeDescriptor;
        if (index == 2u)
            return UnifiedGtDescriptor;

        int original = MapUnifiedTitleIndex((int)index);
        if (original < 0)
            original = 0;
        uint languageIndex = Math.Min(language, 6u);
        uint descriptorTable =
            m.ReadU32(0x8004BC5Cu + languageIndex * 4u);
        int label = (short)m.ReadU16(
            0x8004BC14u + (uint)original * 2u);
        return descriptorTable + (uint)Math.Max(0, label) * 12u;
    }

    public static void CommitUnifiedTitleSelection(
        IMemory m, uint index, uint titleState)
    {
        if (index == 1u)
        {
            Console.WriteLine("[GT2] title selection: Arcade Mode");
            throw new GT2VariantSwitch("arcade");
        }

        int value = UnifiedTitleSelectionValue(index);
        if (value < 0)
            return;
        if (index == 2u)
            Console.WriteLine("[GT2] title selection: Gran Turismo Mode");
        m.WriteU8(titleState + 3u, (byte)value);
    }

    static long _vehicleLodRequests;
    static int _vehicleLodTraceRegistered;
    static int _forcedVehicleLodReported;
    static long _aiDriverTicks;
    static int _aiAutoDriveReported;
    static int _aiAutoDriveRaceTicks;
    static int _aiAutoDriveQuickWinApplied;

    static int ParseAiAutoDriveMaxEngagements(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 2;
        if (!int.TryParse(text, out int value) || value is < 2 or > 64)
            throw new InvalidOperationException(
                "RECOMPONE_GT2_AI_AUTODRIVE_MAX_ENGAGEMENTS must be from 2 through 64");
        return value;
    }

    static int ParseAiAutoDriveQuickWinAfterTicks(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;
        if (!int.TryParse(text, out int value) || value is < 1 or > 60_000)
            throw new InvalidOperationException(
                "RECOMPONE_GT2_SOAK_QUICK_WIN_AFTER_AI_TICKS must be from 1 through 60000");
        return value;
    }

    static uint ParseVehicleLodSelector(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;
        if (!uint.TryParse(text, out uint selector) ||
            selector is < 1u or > 3u)
            throw new InvalidOperationException(
                "RECOMPONE_GT2_VEHICLE_LOD_SELECTOR_OVERRIDE must be 1, 2, or 3");
        return selector;
    }

    /// <summary>
    /// GT2 stores a one-based forced vehicle-detail selector in the first byte
    /// of its render request. The common renderer subtracts one before indexing
    /// the model table, so selector 1 is model-table index 0 (highest detail).
    /// A diagnostic override is intentionally separate from wrapper settings so
    /// fixed-scene tests can isolate vehicle LOD from track-object LOD.
    /// </summary>
    public static uint GetForcedVehicleLodSelector()
    {
        bool maximum =
            Config.ConfigManager.View.LevelOfDetail.Equals(
                "Maximum", StringComparison.OrdinalIgnoreCase);
        uint selector = maximum ? 1u : DiagnosticVehicleLodSelector;
        if (selector != 0u &&
            Interlocked.Exchange(ref _forcedVehicleLodReported, 1) == 0)
            Console.Error.WriteLine(
                $"[GT2-LOD] vehicle selector forced={selector} " +
                $"model-index={selector - 1u} " +
                $"source={(maximum ? "wrapper Maximum" : "diagnostic override")}");
        return selector;
    }

    public static void TraceVehicleLodSelection(
        uint carState, uint renderRequest, IMemory m)
    {
        uint selector = m.ReadU8(renderRequest);
        uint modelSet = m.ReadU32(renderRequest + 0xCu);
        uint modelPointer = 0;
        if (selector is >= 1u and <= 3u && IsGuestRam(modelSet))
        {
            uint modelIndex = selector - 1u;
            modelPointer =
                m.ReadU32(modelSet + 0x870u + modelIndex * 8u);
        }
        WorldCaptureContext.BeginVehicle(carState, modelPointer);

        if (!TraceVehicleLod)
            return;

        lock (VehicleLodSelectorCounts)
        {
            _vehicleLodRequests++;
            VehicleLodSelectorCounts.TryGetValue(selector, out long count);
            VehicleLodSelectorCounts[selector] = count + 1;
            if (IsGuestRam(modelSet))
                VehicleLodModelSets.Add(modelSet);
            if (IsGuestRam(modelPointer))
                VehicleLodModelPointers.Add(modelPointer);
            if (_vehicleLodTraceRegistered == 0)
            {
                _vehicleLodTraceRegistered = 1;
                AppDomain.CurrentDomain.ProcessExit += (_, _) =>
                {
                    lock (VehicleLodSelectorCounts)
                    {
                        string selectors = string.Join(
                            ',',
                            VehicleLodSelectorCounts
                                .OrderBy(pair => pair.Key)
                                .Select(pair =>
                                    $"{pair.Key}:{pair.Value}"));
                        Console.Error.WriteLine(
                            $"[GT2-LOD] vehicle requests={_vehicleLodRequests} " +
                            $"selectors=[{selectors}] " +
                            $"modelSets={VehicleLodModelSets.Count} " +
                            $"selectedModelPointers={VehicleLodModelPointers.Count}");
                    }
                };
            }
        }
    }

    /// <summary>
    /// Records the driver-mode dispatch and the small set of live fields that
    /// surround GT2's racing-line/controller handoff. This is intentionally
    /// read-only: the trace establishes which original AI path and outputs can
    /// safely be reused by the validation auto-drive harness.
    /// </summary>
    public static void TraceAiDriverTable(
        uint carArray, uint carCount, IMemory m)
    {
        if (!TraceAiDrivers ||
            !IsGuestRam(carArray) ||
            carCount is < 1u or > 16u)
            return;

        long tick = Interlocked.Increment(ref _aiDriverTicks);
        if (tick > 12 && tick % 120 != 0)
            return;

        for (uint index = 0; index < carCount; index++)
        {
            uint car = carArray + index * 0xB40u + 0x2Cu;
            int mode = (sbyte)m.ReadU8(car + 0x45Du);
            Console.Error.WriteLine(
                $"[GT2-AI] tick={tick} car={index} mode={mode} " +
                $"line={m.ReadU16(car + 0x610u)} " +
                $"lineNext={m.ReadU16(car + 0x612u)} " +
                $"speedTarget={m.ReadU16(car + 0x640u)} " +
                $"speed={m.ReadU32(car + 0x64Cu)} " +
                $"progress={m.ReadU16(car + 0x6FEu)} " +
                $"control0={m.ReadU16(car + 0x708u)} " +
                $"control1={m.ReadU32(car + 0x710u)} " +
                $"control2={m.ReadU32(car + 0x714u)} " +
                $"flags={m.ReadU8(car + 0x718u):X2}");
        }
    }

    /// <summary>
    /// Diagnostic race harness: route the human-controlled car through GT2's
    /// original per-car AI driver (mode 2). The player keeps its own complete
    /// vehicle/upgrade/physics object; only the native driver dispatch changes.
    /// </summary>
    public static void ApplyAiAutoDrive(
        uint carArray, uint carCount, IMemory m)
    {
        if (!AiAutoDrive ||
            !IsGuestRam(carArray) ||
            carCount is < 1u or > 16u)
            return;

        // GT2 records the human pad stream, not the output of a mode-2 driver.
        // Permit one engagement for the live race and one when replay rebuilds
        // the player car. The default two-pass ceiling prevents a later
        // menu/race object that reuses mode 0 from being touched. Long,
        // explicitly opted-in soak sessions can raise the ceiling for
        // subsequent championship races and replays.
        int engagement = Volatile.Read(ref _aiAutoDriveReported);
        if (AiAutoDriveQuickWinAfterTicks > 0 &&
            engagement > 0 &&
            engagement % 2 == 1)
        {
            int ticks = Interlocked.Increment(ref _aiAutoDriveRaceTicks);
            if (ticks >= AiAutoDriveQuickWinAfterTicks &&
                Interlocked.Exchange(ref _aiAutoDriveQuickWinApplied, 1) == 0)
            {
                // NTSC-U v1.2 live-race working data. This test-only transition
                // is equivalent to the established quick-win diagnostic: it
                // leaves boot, loading, vehicle setup, physics, and the grace
                // window untouched, then asks the original race flow to finish.
                m.WriteU8(0x801D586Bu, 1);
                if (m.ReadU16(0x800A9CBCu) == 0)
                    m.WriteU16(0x800A9CBCu, 1);
                m.WriteU16(0x801D5944u, 1);
                m.WriteU16(0x801D5D54u, 0x0500);
                Console.Error.WriteLine(
                    $"[GT2-Soak] quick-win transition applied after {ticks} AI ticks " +
                    $"engagement={engagement}");
            }
        }
        if (engagement >= AiAutoDriveMaxEngagements)
            return;

        for (uint index = 0; index < carCount; index++)
        {
            uint car = carArray + index * 0xB40u + 0x2Cu;
            if ((sbyte)m.ReadU8(car + 0x45Du) != 0)
                continue;

            m.WriteU8(car + 0x45Du, 2);
            int pass = Interlocked.Increment(ref _aiAutoDriveReported);
            string phase = pass % 2 == 1 ? "race" : "replay";
            if (pass % 2 == 1)
            {
                Interlocked.Exchange(ref _aiAutoDriveRaceTicks, 0);
                Interlocked.Exchange(ref _aiAutoDriveQuickWinApplied, 0);
            }
            int session = (pass + 1) / 2;
            Host.InputManager.SignalScriptStage($"{phase}_{session}");
            Console.Error.WriteLine(
                $"[GT2-AI] auto-drive engaged pass={pass}/{AiAutoDriveMaxEngagements} " +
                $"phase={phase} car={index}: " +
                "native driver mode 0 -> 2; vehicle physics unchanged");
            return;
        }
    }

    public static void TraceTrackRenderRequest(CpuContext c, IMemory m)
    {
        if (!TraceTrackRendering)
            return;

        lock (TrackObjects)
        {
            _trackRenderRequests++;
            TrackObjects.Add(c.A0);
            TrackViewIndices.Add(c.A1);
            TrackMeshIndices.Add(c.A2);
            TrackPasses.Add(c.A3);
            int selector = (int)c.A3;
            ReplayTrackSelectorCounts.TryGetValue(
                selector, out long selectorCount);
            ReplayTrackSelectorCounts[selector] = selectorCount + 1;

            ushort modelIndex = 0;
            uint modelPointer = 0;
            uint modelId = 0;
            if (IsGuestRam(c.A0))
            {
                modelIndex = m.ReadU16(c.A0 + 0x10u);
                uint tableEntry = unchecked(
                    (uint)(0x800520E8L + (long)selector * 4L));
                if (IsGuestRam(tableEntry))
                {
                    uint table = m.ReadU32(tableEntry);
                    uint modelEntry = unchecked(
                        table + (uint)modelIndex * 4u);
                    if (IsGuestRam(table) && IsGuestRam(modelEntry))
                    {
                        modelPointer = m.ReadU32(modelEntry);
                        if (IsGuestRam(modelPointer))
                        {
                            ReplayTrackModelPointers.Add(modelPointer);
                            modelId = DecodeTrackModelId(m, modelPointer);
                            ReplayTrackModelIds.Add(modelId);
                        }
                    }
                }
            }
            if (_trackRenderSamples++ < 24)
                Console.Error.WriteLine(
                    $"[GT2-Track-LOD] poll={Host.InputManager.CurrentPoll} " +
                    $"object=0x{c.A0:X8} view={c.A1} mesh={c.A2} " +
                    $"selector={selector} modelIndex={modelIndex} " +
                    $"model=0x{modelPointer:X8} id=0x{modelId:X8}");
            if (!_trackExitTraceRegistered)
            {
                _trackExitTraceRegistered = true;
                AppDomain.CurrentDomain.ProcessExit += (_, _) =>
                {
                    lock (TrackObjects)
                        Console.Error.WriteLine(
                            $"[GT2-Track] requests={_trackRenderRequests} " +
                            $"objects={TrackObjects.Count} " +
                            $"viewIndices={TrackViewIndices.Count} " +
                            $"meshIndices={TrackMeshIndices.Count} " +
                            $"passes={TrackPasses.Count} " +
                            $"viewRange={Range(TrackViewIndices)} " +
                            $"meshRange={Range(TrackMeshIndices)} " +
                            $"passRange={Range(TrackPasses)} " +
                            $"selectors=[{string.Join(
                                ',',
                                ReplayTrackSelectorCounts
                                    .OrderBy(pair => pair.Key)
                                    .Select(pair =>
                                        $"{pair.Key}:{pair.Value}"))}] " +
                            $"modelPointers={ReplayTrackModelPointers.Count} " +
                            $"modelIds={ReplayTrackModelIds.Count}");
                };
            }
        }
    }

    static uint DecodeTrackModelId(IMemory m, uint pointer)
    {
        const uint lookup = 0x801EF630u;
        uint result = 0;
        for (int index = 0; index < 5; index++)
        {
            int offset = (sbyte)m.ReadU8(pointer + (uint)index);
            uint value = m.ReadU8(unchecked(lookup + (uint)offset));
            result |= value << ((4 - index) * 6);
        }
        return result;
    }

    static string Range(HashSet<uint> values)
    {
        if (values.Count == 0)
            return "none";
        return $"{values.Min():X8}-{values.Max():X8}";
    }

    public static void TraceTrackVisibility(CpuContext c, IMemory m)
    {
        if (!TraceTrackRendering)
            return;

        uint renderState = c.A0;
        uint cameraState = c.A1;
        uint trackRoot = m.ReadU32(cameraState + 0x10u);
        uint sector = m.ReadU32(renderState + 0xA0u);
        uint sectorEntry = m.ReadU32(trackRoot + sector * 4u + 0xCu);
        uint visibilityList = sectorEntry == 0
            ? 0
            : m.ReadU32(sectorEntry + 0xA0u);

        lock (TrackVisibilitySectors)
        {
            if (!TrackVisibilitySectors.Add(sector))
                return;
            ushort count = visibilityList == 0
                ? (ushort)0
                : m.ReadU16(visibilityList);
            string items = visibilityList == 0
                ? ""
                : string.Join(
                    ',',
                    Enumerable.Range(0, Math.Min(12, (int)count))
                        .Select(index =>
                            $"{m.ReadU16(visibilityList + 2u + (uint)index * 2u):X4}"));
            Console.Error.WriteLine(
                $"[GT2-Visibility] render=0x{renderState:X8} " +
                $"camera=0x{cameraState:X8} mode={c.A2} " +
                $"root=0x{trackRoot:X8} sector={sector} " +
                $"entry=0x{sectorEntry:X8} list=0x{visibilityList:X8} " +
                $"count={count} items=[{items}]");
        }
    }

    /// <summary>
    /// Overlay 0 renders the packed visibility list authored for the camera's
    /// current track sector. Those lists are potential-visibility sets, not
    /// independent slices of a global object list: combining every sector
    /// exposes mutually exclusive or occluded road surfaces. Extended draw
    /// distance therefore retains the current authored set and changes only
    /// the later radial limit. Maximum LOD copies that same set and clears its
    /// two selector bits independently.
    /// </summary>
    public static uint GetTrackVisibilityList(
        IMemory m, uint _trackRoot, uint stockList)
    {
        bool maximumLod =
            Config.ConfigManager.View.LevelOfDetail.Equals(
                "Maximum", StringComparison.OrdinalIgnoreCase);
        if (!maximumLod)
        {
            TraceTrackVisibilityLod(m, stockList, maximumLod);
            return stockList;
        }

        if (!IsGuestRam(stockList))
            return stockList;
        int visibleCount = Math.Min((int)m.ReadU16(stockList), 0x4000);
        uint output = ExpandedVisibilityListAddress;
        m.WriteU16(output, (ushort)visibleCount);
        for (int item = 0; item < visibleCount; item++)
            m.WriteU16(
                output + 2u + (uint)item * 2u,
                (ushort)(m.ReadU16(
                    stockList + 2u + (uint)item * 2u) & 0x3FFF));
        TraceTrackVisibilityLod(m, output, maximumLod);
        return output;
    }

    static void TraceTrackVisibilityLod(
        IMemory m, uint visibilityList, bool maximumLod)
    {
        if (!TraceTrackRendering)
            return;

        if (Volatile.Read(ref _aiAutoDriveReported) >= 2)
            _visibilityLodReplayCalls++;
        else
            _visibilityLodRaceCalls++;
        if (maximumLod)
            _visibilityLodMaximumCalls++;
        else
            _visibilityLodStockCalls++;

        if (IsGuestRam(visibilityList))
        {
            int count = Math.Min((int)m.ReadU16(visibilityList), 0x4000);
            for (int index = 0; index < count; index++)
            {
                ushort entry = m.ReadU16(
                    visibilityList + 2u + (uint)index * 2u);
                _visibilityLodEntriesScanned++;
                if ((entry & 0xC000) != 0)
                    _visibilityLodNonzeroSelectors++;
            }
        }

        if (Interlocked.Exchange(
                ref _visibilityLodExitTraceRegistered, 1) == 0)
            AppDomain.CurrentDomain.ProcessExit += (_, _) =>
                Console.Error.WriteLine(
                    $"[GT2-Visibility-LOD] raceCalls={_visibilityLodRaceCalls} " +
                    $"replayCalls={_visibilityLodReplayCalls} " +
                    $"maximumCalls={_visibilityLodMaximumCalls} " +
                    $"stockCalls={_visibilityLodStockCalls} " +
                    $"entriesScanned={_visibilityLodEntriesScanned} " +
                    $"nonzeroSelectors={_visibilityLodNonzeroSelectors}");
    }

    static bool IsGuestRam(uint address) =>
        address is >= 0x80000000u and < 0x807FFF00u;

    public static void TraceRaceSchedulerDispatch(CpuContext c, IMemory m, uint address)
    {
        if (!TraceRaceScheduler) return;
        bool schedulerTarget = address is >= 0x80015FF8u and <= 0x80016258u;
        bool schedulerCaller = c.RA is >= 0x80016000u and <= 0x80016258u;
        if (!schedulerTarget && !schedulerCaller) return;

        uint raceObject = c.S2;
        uint state = raceObject is >= 0x80000000u and < 0x80800000u
            ? m.ReadU32(raceObject + 8u)
            : uint.MaxValue;
        string key =
            $"{address:X8}:{c.RA:X8}:{raceObject:X8}:{state:X8}:{c.A0:X8}";
        lock (RaceSchedulerStates)
        {
            if (!RaceSchedulerStates.Add(key)) return;
        }
        Console.WriteLine(
            $"[GT2Scheduler] call=0x{address:X8} ra=0x{c.RA:X8} " +
            $"object=0x{raceObject:X8} state=0x{state:X8} a0=0x{c.A0:X8} " +
            $"sp=0x{c.SP:X8}");
    }
    /// <summary>
    /// The original routine at 0x80010954 installs the callback at 0x80010928,
    /// spins until four VBlank interrupts increment 0x80011DF4, and removes
    /// the callback. A statically executing host cannot receive those
    /// asynchronous callbacks while it remains inside the spin loop, so
    /// advance the same four presentation/interrupt intervals explicitly.
    /// </summary>
    public static void WaitForInitialVBlanks(CpuContext c, IMemory m)
    {
        uint counterAddress = ArcadeVariant ? 0x80011DECu : 0x80011DF4u;
        m.WriteU32(counterAddress, 0u);
        for (uint count = 1; count <= 4; count++)
        {
            Runtime.PresentFrame();
            m.WriteU32(counterAddress, count);
        }
        c.V0 = 0u;
    }

    /// <summary>
    /// GT2's wrapper at 0x8007D23C waits for callback 0x8007D294 to advance
    /// the total and interval counters at 0x801F0680/0x801F0684. Presenting
    /// frames inside the host bridge lets the already-installed original
    /// callback run instead of deadlocking in the guest spin loop.
    /// </summary>
    public static void VSync(CpuContext c, IMemory m)
    {
        uint totalCounterAddress = CdDriveStateAddress + 0x170u;
        uint intervalCounterAddress = CdDriveStateAddress + 0x174u;

        int requested = (int)c.A0;
        if (requested < 0)
        {
            m.WriteU32(intervalCounterAddress, 0u);
            requested = 1;
        }
        else if (requested == 0)
        {
            // VSync(0) is a non-blocking counter query on real hardware, where
            // VBlank interrupts continue asynchronously. Recompiled guest code
            // runs synchronously, so polling loops would otherwise prevent the
            // callback that advances this counter from ever executing.
            Runtime.PresentFrame();
        }

        for (int frame = 0; frame < requested; frame++)
            Runtime.PresentFrame();

        if (requested > 0)
            m.WriteU32(intervalCounterAddress, 0u);
        c.V0 = m.ReadU32(totalCounterAddress);
    }

    public static void WaitForCdCommand(CpuContext c, IMemory m)
    {
        uint state = CdDriveStateAddress;
        uint busyAddress = state + 0x166u;
        uint deferredScriptAddress = state + 0x84u;
        uint currentScriptAddress = state + 0x7Cu;

        int attempt = 0;
        for (; m.ReadU8(busyAddress) != 0 && attempt < 65536; attempt++)
        {
            LibCd.Tick();
            uint deferredScript = m.ReadU32(deferredScriptAddress);
            bool transferInactive = m.ReadU8(state + 0x167u) == 0;
            bool readReachedEnd =
                m.ReadU32(state + 0x48u) != 0 &&
                m.ReadU32(state + 0x60u) >= m.ReadU32(state + 0x48u) &&
                (transferInactive || m.ReadU32(state + 0x4Cu) == 0);
            if (m.ReadU8(busyAddress) == 3 &&
                ((transferInactive && deferredScript != 0) || readReachedEnd) &&
                LibCd.QueueReadCompletion())
            {
                Console.Error.WriteLine(
                    $"[GT2Compat] queued deferred ReadN completion " +
                    $"script=0x{(deferredScript != 0 ? deferredScript : m.ReadU32(currentScriptAddress)):X8}");
            }
            if ((attempt & 63) == 63)
                Runtime.PresentFrame();
        }

        byte busy = m.ReadU8(busyAddress);
        if (busy != 0)
            throw new InvalidOperationException(
                $"GT2 CD command queue remained busy ({busy}) after {attempt} " +
                $"device ticks at 0x8007C550; current={m.ReadU32(state + 0x60u)} " +
                $"target={m.ReadU32(state + 0x48u)} handler=0x{m.ReadU32(state + 0x4Cu):X8}");
    }

    /// <summary>
    /// Give the virtual CD device the interrupt boundary that original PS1
    /// hardware supplied between consecutive low-level status polls.
    /// </summary>
    public static void ServiceCdDevice(CpuContext c, IMemory m)
    {
        LibCd.Tick();
        Runtime.DrainDeferredIrqs();
        // The original MDEC/CD producer-consumer pipeline continues from
        // interrupts while the foreground code polls the opening movie state.
        // Deliver only those pending interrupts here. Presenting a complete
        // host frame would re-enter the same guest object through its VBlank
        // scheduler while it is already updating.
        Runtime.DispatchIrq(0);

        // A decoded STR frame is emitted through a chain of MDEC-out and GPU
        // DMA slices. Each slice can raise another completion while the
        // previous callback is still unwinding, so drain the chain exactly as
        // the hardware interrupt controller would.
        const uint dmaInterruptControl = 0x1F8010F4u;
        for (int guard = 0;
             guard < 64 &&
             (m.ReadU32(dmaInterruptControl) & 0x7F000000u) != 0;
             guard++)
        {
            Runtime.DispatchIrq(0);
        }
    }

    /// <summary>
    /// GT2's ring-buffer accessor at 0x80082054 waits for its ready callback
    /// to increment the buffered-sector count at +0x5A. Service the virtual
    /// drive while that original asynchronous producer is starved by static
    /// execution, then return the same current ring-slot pointer as 0x80082000.
    /// </summary>
    public static void WaitForCdBuffer(CpuContext c, IMemory m)
    {
        uint state = CdDriveStateAddress;
        int attempt = 0;
        while (m.ReadU16(state + 0x5Au) == 0 && attempt < 65536)
        {
            LibCd.Tick();
            if ((++attempt & 63) == 0)
                Runtime.PresentFrame();
        }

        if (m.ReadU16(state + 0x5Au) == 0)
            throw new InvalidOperationException(
                $"GT2 CD ring remained empty after {attempt} device ticks; " +
                $"busy={m.ReadU8(state + 0x166u)} current={m.ReadU32(state + 0x44u)}");

        c.V0 = m.ReadU32(state + 0x50u) +
               ((uint)m.ReadU16(state + 0x5Cu) << 11);
    }

    public static void TraceBootObject(CpuContext c, IMemory m)
    {
        if (!TraceBoot)
            return;

        int invocation = _bootObjectTraceCount++;
        if (invocation >= 8 && invocation % 60 != 0)
            return;
        uint instance = c.A0;
        uint table = m.ReadU32(instance);
        var entries = new string[13];
        for (uint index = 0; index < entries.Length; index++)
            entries[index] = $"0x{m.ReadU32(table + index * 4u):X8}";
        Console.Error.WriteLine(
            $"[GT2Compat] boot-object frame={invocation} instance=0x{instance:X8} " +
            $"clock={m.ReadU32(instance + 0x80u)}/{m.ReadU32(instance + 0x84u)} " +
            $"flags=0x{m.ReadU32(instance + 0x88u):X8}/0x{m.ReadU32(instance + 0x90u):X8} " +
            $"vtable=0x{table:X8} entries={string.Join(',', entries)}");
    }

    public static void TraceOverlayLoad(CpuContext c, IMemory m)
    {
        uint index = c.A0;
        _overlayIndex = index;
        if (!TraceBoot)
            return;

        int invocation = _overlayLoadTraceCount++;
        if (invocation >= 16)
            return;

        uint tableBase = 0x801EF610u;
        uint entry = index < 4096u
            ? tableBase + index * 8u + 0xCu
            : 0u;
        Console.Error.WriteLine(
            $"[GT2Compat] overlay-load invocation={invocation} index={index} " +
            $"returnTarget=0x{c.S0:X8} " +
            $"archiveBase={m.ReadU32(0x801C93D0u)} " +
            $"archiveSize={m.ReadU32(0x801C93E0u)} loadedBase={m.ReadU32(tableBase + 8u):X8} " +
            $"entry=0x{entry:X8} offset=0x{(entry != 0 ? m.ReadU32(entry) : 0):X8} " +
            $"flags=0x{(entry != 0 ? m.ReadU32(entry + 4u) : 0):X8}");
    }

    public static void TraceFiniteCdRead(CpuContext c, IMemory m)
    {
        int invocation = _finiteCdReadTraceCount++;
        if (invocation >= 40)
            return;

        uint drive = CdDriveStateAddress;
        const uint transfer = 0x801C9500u;
        Console.Error.WriteLine(
            $"[GT2Compat] finite-read invocation={invocation} sector={c.A0} " +
            $"expected={m.ReadU32(drive + 0x44u)} end={m.ReadU32(drive + 0x48u)} " +
            $"dest=0x{m.ReadU32(transfer):X8} offset={m.ReadU16(transfer + 0x6u)} " +
            $"stride={m.ReadU16(transfer + 0x10u)} remaining={m.ReadU16(transfer + 0x12u)} " +
            $"active={m.ReadU8(drive + 0x167u)} busy={m.ReadU8(drive + 0x166u)}");
    }

    public static void TraceMenuListInput(CpuContext c, IMemory m)
    {
        if (!TraceMenu || c.A1 == 0)
            return;

        int invocation = _menuListTraceCount++;
        uint input = c.A1;
        uint pressed = m.ReadU32(input + 0x4u);
        uint repeated = m.ReadU32(input + 0xCu);
        if (pressed == 0 && repeated == 0 && invocation >= 12)
            return;

        Console.Error.WriteLine(
            $"[GT2Compat] menu-list call={invocation} input=0x{input:X8} " +
            $"pressed=0x{pressed:X8} repeated=0x{repeated:X8} " +
            $"list=0x{c.A0:X8} selected={m.ReadU16(c.A0 + 0x6u)}");
    }

    /// <summary>
    /// Run GT2 through a trampoline that gives its setjmp/longjmp overlay
    /// loader genuine non-local control-flow semantics. Every jump unwinds the
    /// abandoned managed guest call stack before the selected overlay target
    /// is dispatched. Calling the target directly from Longjmp would retain
    /// every prior overlay on the CLR stack; replay exit performs enough
    /// transitions for those stale frames to re-enter old loaders.
    /// </summary>
    public static void RunGuestLoop(
        CpuContext c, IMemory m, uint entry,
        string overlayPrefix = "gt2_overlay")
    {
        string previousOverlayPrefix = _overlayPrefix;
        _overlayPrefix = overlayPrefix;
        try
        {
            uint target = entry;
            int transition = 0;
            bool returnTrampoline = false;
            while (true)
            {
                try
                {
                    Dispatch.Dispatcher.Call(c, m, target);
                    if (!returnTrampoline)
                        return;
                    target = c.RA;
                    if (TraceBoot)
                        Console.Error.WriteLine(
                            $"[GT2Compat] coroutine return target=0x{target:X8}");
                    if (target == 0u)
                        throw new InvalidOperationException(
                            "GT2 coroutine return trampoline reached a null return address");
                }
                catch (NonLocalJump jump)
                {
                    target = jump.Target;
                    returnTrampoline = jump.ReturnTrampoline;
                    transition++;
                    if (TraceBoot)
                        Console.Error.WriteLine(
                            $"[GT2Compat] non-local overlay transition={transition} " +
                            $"target=0x{target:X8}");
                }
            }
        }
        finally
        {
            _overlayPrefix = previousOverlayPrefix;
        }
    }

    /// <summary>
    /// GT2 uses setjmp/longjmp to transfer control from its bootstrap overlay
    /// loader into the newly decompressed image. Restore the saved MIPS
    /// register context, then signal RunGuestLoop to abandon the current
    /// managed guest call stack and dispatch the selected overlay target.
    /// </summary>
    public static void Longjmp(CpuContext c, IMemory m)
    {
        uint context = c.A0;
        uint value = c.A1 == 0u ? 1u : c.A1;
        bool overlayTransition = value >= 0x80000000u;
        if (overlayTransition)
        {
            if (_overlayIndex > 5)
                throw new InvalidOperationException(
                    $"GT2 longjmp selected unknown overlay {_overlayIndex}");
            Dispatch.Dispatcher.Load($"{_overlayPrefix}_{_overlayIndex}");
        }

        c.RA = m.ReadU32(context);
        c.SP = m.ReadU32(context + 0x4u);
        c.FP = m.ReadU32(context + 0x8u);
        c.S0 = m.ReadU32(context + 0xCu);
        c.S1 = m.ReadU32(context + 0x10u);
        c.S2 = m.ReadU32(context + 0x14u);
        c.S3 = m.ReadU32(context + 0x18u);
        c.S4 = m.ReadU32(context + 0x1Cu);
        c.S5 = m.ReadU32(context + 0x20u);
        c.S6 = m.ReadU32(context + 0x24u);
        c.S7 = m.ReadU32(context + 0x28u);
        c.GP = m.ReadU32(context + 0x2Cu);
        c.V0 = value;

        c.A0 = m.ReadU32(0x801C945Cu);
        c.A1 = m.ReadU32(0x801C9460u);
        c.A2 = m.ReadU32(0x801C9464u);
        c.A3 = m.ReadU32(0x801C9468u);
        uint target = overlayTransition ? value : c.RA;
        if (TraceBoot)
            Console.Error.WriteLine(
                overlayTransition
                    ? $"[GT2Compat] longjmp overlay={_overlayIndex} target=0x{target:X8}"
                    : $"[GT2Compat] longjmp coroutine target=0x{target:X8} value={value}");
        throw new NonLocalJump(target, returnTrampoline: !overlayTransition);
    }
}
