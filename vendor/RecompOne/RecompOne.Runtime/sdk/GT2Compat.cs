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
    static readonly bool TraceWheelTransforms =
        Environment.GetEnvironmentVariable(
            "RECOMPONE_TRACE_GT2_WHEEL_TRANSFORMS") == "1";
    static readonly string? WheelTransformTracePath =
        Environment.GetEnvironmentVariable(
            "RECOMPONE_GT2_WHEEL_TRANSFORM_TRACE_PATH");
    static readonly bool AuditRenderer =
        Environment.GetEnvironmentVariable("RECOMPONE_AUDIT_RENDERER") == "1";
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
    static int _trackVisibilitySamples;
    static long _trackVisibilityFunctionCalls;
    static long _trackVisibilityRaceCalls;
    static long _trackVisibilityReplayCalls;
    static long _trackVisibilityRawEntries;
    static long _trackVisibilityRawNonzeroSelectors;
    static int _renderSchedulerSamples;
    static bool _trackExitTraceRegistered;
    static readonly HashSet<uint> TrackVisibilitySectors = [];
    static long _visibilityLodRaceCalls;
    static long _visibilityLodReplayCalls;
    static long _visibilityLodMaximumCalls;
    static long _visibilityLodStockCalls;
    static long _visibilityLodEntriesScanned;
    static long _visibilityLodNonzeroSelectors;
    static long _visibilityLodNullLists;
    static long _visibilityLodInvalidLists;
    static long _visibilityExpandedCalls;
    static long _visibilityExpandedStockEntries;
    static long _visibilityExpandedOutputEntries;
    static int _visibilityExpandedMaximumAdded;
    static long _visibilitySectorTransitions;
    static long _visibilityStockTransitionAdds;
    static long _visibilityStockTransitionRemoves;
    static long _visibilityStockAddsPrecovered;
    static long _visibilityStockRemovesRetained;
    static long _visibilityExpandedTransitionAdds;
    static long _visibilityExpandedTransitionRemoves;
    static int _visibilityStockMaximumTransition;
    static int _visibilityExpandedMaximumTransition;
    static readonly HashSet<uint> VisibilityLodInvalidListSamples = [];
    static int _visibilityLodExitTraceRegistered;
    static int _wheelTransformTraceEnabledReported;
    static int _wheelTransformTraceInvalidIndexReported;
    static int _wheelDispatchTraceSamples;
    static readonly Dictionary<uint, long> VehicleLodSelectorCounts = [];
    static readonly HashSet<uint> VehicleLodModelSets = [];
    static readonly HashSet<uint> VehicleLodModelPointers = [];
    sealed class WheelTransformTraceState
    {
        public long Samples;
        public readonly int[] Previous = new int[3];
        public readonly long[] Changes = new long[3];
        public readonly int[] MinimumShortestDelta =
            [int.MaxValue, int.MaxValue, int.MaxValue];
        public readonly int[] MaximumShortestDelta =
            [int.MinValue, int.MinValue, int.MinValue];
    }
    static readonly object WheelTransformTraceLock = new();
    static readonly Dictionary<ulong, WheelTransformTraceState>
        WheelTransformTraceStates = [];
    static readonly System.Text.StringBuilder WheelTransformTraceCsv = new();
    static bool _wheelTransformTraceExitRegistered;
    const uint ExpandedVisibilityListAddress = 0x807F0000u;
    const int ExtendedVisibilitySectorRadius = 3;
    static readonly int[] VisibilityEntryGenerations = new int[0x4000];
    static readonly int[] VisibilityEntryPositions = new int[0x4000];
    static readonly ushort[] ExpandedVisibilityEntries = new ushort[0x4000];
    static readonly bool[] PreviousStockVisibilityEntries = new bool[0x4000];
    static readonly bool[] PreviousExpandedVisibilityEntries = new bool[0x4000];
    static readonly bool[] CurrentStockVisibilityEntries = new bool[0x4000];
    static readonly bool[] CurrentExpandedVisibilityEntries = new bool[0x4000];
    static int _visibilityEntryGeneration;
    static uint _visibilityTransitionTrackRoot;
    static int _visibilityTransitionSector = -1;
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
    const uint UnifiedTitleDescriptorBase = 0x803FF000u;
    const uint UnifiedTitleBlankDescriptor = UnifiedTitleDescriptorBase + 0x30u;
    static bool _unifiedTitleMenuActive;
    static ushort[]? _unifiedTitlePanels;
    const int UnifiedTitleWidth = 512;
    const int UnifiedTitleHeight = 480;

    public static bool UnifiedTitleMenuActive => _unifiedTitleMenuActive;

    public static void ConfigureUnifiedTitlePanels(string path)
    {
        byte[] data = File.ReadAllBytes(path);
        const int headerSize = 20;
        const int panelPixels = UnifiedTitleWidth * UnifiedTitleHeight;
        const int panelCount = 4;
        if (data.Length != headerSize + panelPixels * panelCount * 2 ||
            System.Text.Encoding.ASCII.GetString(data, 0, 8) != "GT2TITLE" ||
            BitConverter.ToInt32(data, 8) != UnifiedTitleWidth ||
            BitConverter.ToInt32(data, 12) != UnifiedTitleHeight ||
            BitConverter.ToInt32(data, 16) != panelCount)
        {
            throw new InvalidDataException(
                $"invalid exact GT2 title panel asset: {path}");
        }
        var pixels = new ushort[panelPixels * panelCount];
        Buffer.BlockCopy(data, headerSize, pixels, 0, pixels.Length * 2);
        _unifiedTitlePanels = pixels;
        Console.WriteLine(
            "[GT2] loaded exact Sony title panels: " +
            $"{panelCount}x{UnifiedTitleWidth}x{UnifiedTitleHeight} 15-bit");
    }

    public static void CompositeUnifiedTitlePanel(
        global::RecompOne.Runtime.Gpu gpu, IMemory m)
    {
        ushort[]? panels = _unifiedTitlePanels;
        if (!_unifiedTitleMenuActive || panels == null)
            return;
        EnableExactTitleDisplay();
        // Retail overlay 1 starts an attract-mode race after 901 idle ticks.
        // The unified frontend is a persistent PC main menu, so keep that
        // private idle counter at zero while preserving ordinary pad input.
        m.WriteU32(0x800B1228u, 0u);
        int selected = m.ReadU16(UnifiedTitleList + 6u);
        if (selected is < 1 or > 4)
            selected = 1;
        int panelPixels = UnifiedTitleWidth * UnifiedTitleHeight;
        int source = (selected - 1) * panelPixels;
        ReadOnlySpan<ushort> frame = panels.AsSpan(source, panelPixels);
        for (int y = 0; y < UnifiedTitleHeight; y++)
            frame.Slice(y * UnifiedTitleWidth, UnifiedTitleWidth).CopyTo(
                gpu.Vram.AsSpan(y * RecompOne.Runtime.Hle.VramShadow.Width,
                    UnifiedTitleWidth));
        if (RecompOne.Runtime.Hle.GpuHle.Backend?.Ready == true)
            RecompOne.Runtime.Hle.GpuHle.Backend.WriteVram(
                0, 0, UnifiedTitleWidth, UnifiedTitleHeight, frame);
    }

    public static bool SuppressUnifiedTitleListDecorations(uint list) =>
        _unifiedTitleMenuActive && list == UnifiedTitleList;

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
    /// Reproduce the Arcade executable's authored BSS state without invoking
    /// its top-level bootstrap. The range is the exact clear performed by
    /// SCUS-94455 at 0x8005D570 before it initializes services and selects its
    /// first overlay.
    /// </summary>
    public static void PrepareArcadeFrontendHandoff(
        CpuContext c, IMemory m)
    {
        const uint arcadeBssStart = 0x801C8E10u;
        const uint arcadeBssEnd = 0x801F0750u;
        const uint saved = 0x80090E88u;
        m.WriteU32(saved - 8u, c.A0);
        m.WriteU32(saved - 4u, c.A1);
        m.WriteU32(saved, c.S0);
        m.WriteU32(saved + 4u, c.S1);
        m.WriteU32(saved + 8u, c.S2);
        m.WriteU32(saved + 12u, c.S3);
        m.WriteU32(saved + 16u, c.S4);
        m.WriteU32(saved + 20u, c.S5);
        m.WriteU32(saved + 24u, c.S6);
        m.WriteU32(saved + 28u, c.S7);
        m.WriteU32(saved + 32u, c.GP);
        m.WriteU32(saved + 36u, c.SP);
        m.WriteU32(saved + 40u, c.FP);
        m.WriteU32(saved + 44u, c.RA);
        c.SP -= 0x18u;
        m.ZeroRange(arcadeBssStart, arcadeBssEnd - arcadeBssStart);
        Console.WriteLine(
            "[GT2] Arcade handoff prepared: native BSS initialized; " +
            "boot/title overlay omitted");
    }

    /// <summary>
    /// Enter the Arcade frontend through the post-bootstrap initializer used
    /// immediately before the original disc requests overlay 1. This retains
    /// the native Arcade executable, frontend, overlays and data while avoiding
    /// a second game boot in the unified player-facing flow.
    /// </summary>
    public static void RunArcadeFrontendHandoff(CpuContext c, IMemory m)
    {
        const string arcadePrefix = "gt2_arcade_overlay";
        string previousOverlayPrefix = _overlayPrefix;
        _overlayPrefix = arcadePrefix;
        try
        {
            // This is the sole pre-initializer call made by the stock Arcade
            // entry after clearing BSS and before entering func_8005D650.
            c.RA = 0x8005D5F0u;
            Dispatch.Dispatcher.Call(c, m, 0x8008CD18u);
            // SCUS-94455 saves these at 0x80090E80/84 before the initializer
            // and restores them for func_8005D650. Preserve that ABI without
            // running the top-level boot function.
            c.A0 = m.ReadU32(0x80090E80u);
            c.A1 = m.ReadU32(0x80090E84u);
            c.RA = 0x8005D608u;
            Console.WriteLine(
                "[GT2] Arcade frontend handoff: " +
                "entry=0x8005D650 START GAME overlay=1");
            RunGuestLoop(c, m, 0x8005D650u, arcadePrefix);
        }
        finally
        {
            _overlayPrefix = previousOverlayPrefix;
        }
    }

    /// <summary>
    /// Extend the original Simulation-disc title list in guest memory.  The
    /// list engine, cursor, fading and draw path remain GT2's; only its item
    /// count and Sony-authored demo TIM descriptors are supplied here. The
    /// obsolete vertical-list boundary triangles are suppressed on this exact
    /// list because the authored 2x2 panel does not use them.
    /// </summary>
    public static void InstallUnifiedTitleMenu(IMemory m)
    {
        _unifiedTitleMenuActive = true;
        EnableExactTitleDisplay();
        // Two sentinels plus the four complete Sony-authored demo entries:
        // Arcade Mode, Gran Turismo, Replay Theater and Option.
        m.WriteU16(UnifiedTitleList, 6);
        // The final 15-bit panel is composited verbatim at presentation time.
        // Keep the guest list's visual primitives empty while retaining its
        // input, selection, animation timing, and destination dispatch.
        for (uint item = 0; item < 4u; item++)
            WriteTitleDescriptor(
                m, UnifiedTitleDescriptorBase + item * 12u,
                u: 0, v: 0,
                width: 0, height: 0, tpage: 0, clut: 0);
        WriteTitleDescriptor(
            m, UnifiedTitleBlankDescriptor,
            u: 0, v: 0,
            width: 0, height: 0, tpage: 0, clut: 0);

        if (_unifiedTitleInstalled)
            return;
        _unifiedTitleInstalled = true;
        if (Environment.GetEnvironmentVariable(
                "RECOMPONE_TRACE_GT2_TITLE") == "1")
        {
            Console.WriteLine(
                "[GT2-Title] list=" +
                string.Join(' ', Enumerable.Range(0, 12).Select(index =>
                    $"{index * 4:X2}:{m.ReadU32(UnifiedTitleList + (uint)index * 4u):X8}")));
            uint language = Math.Min(m.ReadU8(0x801C98E0u), (byte)6);
            uint table = m.ReadU32(0x8004BC5Cu + language * 4u);
            for (uint item = 0; item < 8u; item++)
            {
                int label = (short)m.ReadU16(0x8004BC14u + item * 2u);
                uint descriptor = table + (uint)Math.Max(0, label) * 12u;
                Console.WriteLine(
                    $"[GT2-Title] item={item} label={label} " +
                    $"descriptor=0x{descriptor:X8} " +
                    $"uv=0x{m.ReadU16(descriptor):X4} " +
                    $"clut=0x{m.ReadU16(descriptor + 2u):X4} " +
                    $"size={m.ReadU16(descriptor + 4u)}x" +
                    $"{m.ReadU16(descriptor + 6u)} " +
                    $"tpage={m.ReadU16(descriptor + 8u)}");
            }
        }
        string palette = Runtime.Gpu == null
            ? "unavailable"
            : string.Join(
                ',',
                Enumerable.Range(0, 16).Select(index =>
                    $"{Runtime.Gpu.Vram[252 * 1024 + 848 + index]:X4}"));
        Console.WriteLine(
            "[GT2] native unified title menu installed: " +
            "Arcade Mode, Gran Turismo, Replay Theater, Option; " +
            $"language={m.ReadU8(0x801C98E0u)} 16bpp " +
            $"palette={palette}");
    }

    static void EnableExactTitleDisplay()
    {
        var gpu = Runtime.Gpu;
        if (gpu == null)
            return;
        const int hStart = 0x260;
        const int hEnd = hStart + 2550;
        gpu.WriteGp1(
            0x06000000u | ((uint)hEnd << 12) | (uint)hStart);
        gpu.WriteGp1(0x08000026u); // 512 horizontal, 480-line interlaced NTSC.
        RecompOne.Runtime.Hle.GpuHle.NotifyDisplay(
            gpu.DisplayX, gpu.DisplayY, UnifiedTitleWidth, UnifiedTitleHeight);
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

    public static int UnifiedTitleSelectionValue(uint index)
    {
        return index switch
        {
            1u => 0, // Arcade Mode: intercepted by CommitUnifiedTitleSelection.
            2u => 0, // Gran Turismo: retail START GAME destination.
            3u => 1, // Retail Replay Theater destination.
            4u => 2, // Retail Option destination.
            _ => -1,
        };
    }

    public static int UnifiedTitleItemValidity(uint index) =>
        index is >= 1u and <= 4u ? 0 : -1;

    public static uint UnifiedTitleDescriptor(
        IMemory m, uint index, uint language)
    {
        if (index is >= 1u and <= 4u)
            return UnifiedTitleDescriptorBase + (index - 1u) * 12u;

        uint languageIndex = Math.Min(language, 6u);
        uint descriptorTable =
            m.ReadU32(0x8004BC5Cu + languageIndex * 4u);
        int label = (short)m.ReadU16(0x8004BC14u);
        return descriptorTable + (uint)Math.Max(0, label) * 12u;
    }

    public static uint UnifiedTitleDescriptorForState(
        uint index, uint selected) =>
        selected != 0u && index is >= 1u and <= 4u
            ? UnifiedTitleDescriptorBase + (index - 1u) * 12u
            : UnifiedTitleBlankDescriptor;

    public static void PositionUnifiedTitleItem(
        IMemory m, uint itemRecord, uint index)
    {
        (int x, int y) = index switch
        {
            // The retail list primitive applies a fixed (-44,-8) title-panel
            // origin adjustment. These anchors place the archive's unscaled
            // 140x28 rectangles at (124,284), (124,312), (264,284),
            // and (264,312), respectively.
            1u => (168, 292),
            2u => (168, 320),
            3u => (308, 292),
            4u => (308, 320),
            _ => (0, 0),
        };
        m.WriteU16(itemRecord + 4u, (ushort)x);
        m.WriteU16(itemRecord + 6u, (ushort)y);
    }

    public static void CommitUnifiedTitleSelection(
        IMemory m, uint index, uint titleState)
    {
        _unifiedTitleMenuActive = false;
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
        else if (index == 3u)
            Console.WriteLine("[GT2] title selection: Replay Theater");
        else if (index == 4u)
            Console.WriteLine("[GT2] title selection: Option");
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
        if (TraceWheelTransforms &&
            Interlocked.Exchange(
                ref _wheelTransformTraceEnabledReported, 1) == 0)
            Console.Error.WriteLine(
                "[GT2-WHEEL] guest transform trace enabled");
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

        if (!TraceVehicleLod && !AuditRenderer)
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
            if (TraceVehicleLod && !AuditRenderer &&
                _vehicleLodTraceRegistered == 0)
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
        if (!TraceTrackRendering && !AuditRenderer)
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
            if (_trackRenderSamples++ < 240)
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
        if (!TraceTrackRendering && !AuditRenderer)
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
            _trackVisibilityFunctionCalls++;
            if (Volatile.Read(ref _aiAutoDriveReported) >= 2)
                _trackVisibilityReplayCalls++;
            else
                _trackVisibilityRaceCalls++;
            if (IsGuestRam(visibilityList))
            {
                int rawCount = Math.Min(
                    (int)m.ReadU16(visibilityList), 0x4000);
                for (int index = 0; index < rawCount; index++)
                {
                    ushort item = m.ReadU16(
                        visibilityList + 2u + (uint)index * 2u);
                    _trackVisibilityRawEntries++;
                    if ((item & 0xC000) != 0)
                        _trackVisibilityRawNonzeroSelectors++;
                }
            }
            if (!TraceTrackRendering)
                return;
            if (_trackVisibilitySamples++ < 120)
                Console.Error.WriteLine(
                    $"[GT2-Render-Cadence] " +
                    $"poll={Host.InputManager.CurrentPoll} " +
                    $"mode={c.A2} sector={sector}");
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
    /// distance therefore keeps the current list first and adds only the
    /// authored sets for a bounded three-sector horizon in each direction.
    /// This moves whole-section pop-in beyond long sightlines without exposing
    /// the rest of a looping track. Maximum LOD clears selector bits after the
    /// bounded union is deduplicated.
    /// </summary>
    public static uint GetTrackVisibilityList(
        IMemory m, uint trackRoot, uint stockList)
    {
        bool extended =
            Config.ConfigManager.View.ExtendedDrawDistance;
        bool maximumLod =
            Config.ConfigManager.View.LevelOfDetail.Equals(
                "Maximum", StringComparison.OrdinalIgnoreCase);
        if (!extended && !maximumLod)
        {
            TraceTrackVisibilityLod(m, stockList, maximumLod);
            return stockList;
        }

        if (!IsGuestRam(stockList))
        {
            TraceTrackVisibilityLod(m, stockList, maximumLod);
            return stockList;
        }
        int stockCount = Math.Min((int)m.ReadU16(stockList), 0x4000);
        int visibleCount;
        if (extended && TryLocateVisibilitySector(
                m,
                trackRoot,
                stockList,
                out int sectorCount,
                out int currentSector))
        {
            visibleCount = BuildExtendedVisibilityList(
                m,
                trackRoot,
                stockList,
                stockCount,
                sectorCount,
                currentSector,
                maximumLod);
            _visibilityExpandedCalls++;
            _visibilityExpandedStockEntries += stockCount;
            _visibilityExpandedOutputEntries += visibleCount;
            _visibilityExpandedMaximumAdded = Math.Max(
                _visibilityExpandedMaximumAdded,
                visibleCount - stockCount);
            if (AuditRenderer)
                AuditVisibilityTransition(
                    m,
                    trackRoot,
                    stockList,
                    stockCount,
                    currentSector,
                    visibleCount);
        }
        else
        {
            visibleCount = stockCount;
            for (int item = 0; item < visibleCount; item++)
            {
                ushort entry = m.ReadU16(
                    stockList + 2u + (uint)item * 2u);
                ExpandedVisibilityEntries[item] = maximumLod
                    ? (ushort)(entry & 0x3FFF)
                    : entry;
            }
        }

        uint output = ExpandedVisibilityListAddress;
        m.WriteU16(output, (ushort)visibleCount);
        for (int item = 0; item < visibleCount; item++)
            m.WriteU16(
                output + 2u + (uint)item * 2u,
                ExpandedVisibilityEntries[item]);
        TraceTrackVisibilityLod(m, output, maximumLod);
        return output;
    }

    static bool TryLocateVisibilitySector(
        IMemory m,
        uint trackRoot,
        uint stockList,
        out int sectorCount,
        out int currentSector)
    {
        sectorCount = 0;
        currentSector = -1;
        if (!IsGuestRam(trackRoot))
            return false;

        uint tableStart = trackRoot + 0xCu;
        uint firstDescriptor = m.ReadU32(tableStart);
        uint tableBytes = unchecked(firstDescriptor - tableStart);
        if (!IsGuestRam(firstDescriptor) ||
            firstDescriptor <= tableStart ||
            (tableBytes & 3u) != 0)
            return false;
        sectorCount = checked((int)(tableBytes / 4u));
        if (sectorCount is < 1 or > 0x1000)
            return false;

        for (int sector = 0; sector < sectorCount; sector++)
        {
            uint descriptor = m.ReadU32(
                tableStart + (uint)sector * 4u);
            if (!IsGuestRam(descriptor))
                return false;
            if (m.ReadU32(descriptor + 0xA0u) == stockList)
                currentSector = sector;
        }
        return currentSector >= 0;
    }

    static int BuildExtendedVisibilityList(
        IMemory m,
        uint trackRoot,
        uint stockList,
        int stockCount,
        int sectorCount,
        int currentSector,
        bool maximumLod)
    {
        int generation = unchecked(++_visibilityEntryGeneration);
        if (generation == 0)
        {
            Array.Clear(VisibilityEntryGenerations);
            generation = ++_visibilityEntryGeneration;
        }
        int outputCount = 0;

        void AddList(uint list, int count)
        {
            for (int item = 0; item < count; item++)
            {
                ushort entry = m.ReadU16(
                    list + 2u + (uint)item * 2u);
                int objectIndex = entry & 0x3FFF;
                ushort outputEntry = maximumLod
                    ? (ushort)objectIndex
                    : entry;
                if (VisibilityEntryGenerations[objectIndex] != generation)
                {
                    if (outputCount >= ExpandedVisibilityEntries.Length)
                        return;
                    VisibilityEntryGenerations[objectIndex] = generation;
                    VisibilityEntryPositions[objectIndex] = outputCount;
                    ExpandedVisibilityEntries[outputCount++] = outputEntry;
                }
                else if (!maximumLod)
                {
                    int position = VisibilityEntryPositions[objectIndex];
                    if ((outputEntry >> 14) <
                        (ExpandedVisibilityEntries[position] >> 14))
                        ExpandedVisibilityEntries[position] = outputEntry;
                }
            }
        }

        AddList(stockList, stockCount);
        uint tableStart = trackRoot + 0xCu;
        for (int distance = 1;
             distance <= ExtendedVisibilitySectorRadius;
             distance++)
        {
            int forward = (currentSector + distance) % sectorCount;
            int backward =
                (currentSector - distance + sectorCount) % sectorCount;
            AddSector(forward);
            if (backward != forward)
                AddSector(backward);
        }
        return outputCount;

        void AddSector(int sector)
        {
            uint descriptor = m.ReadU32(
                tableStart + (uint)sector * 4u);
            if (!IsGuestRam(descriptor))
                return;
            uint list = m.ReadU32(descriptor + 0xA0u);
            if (!IsGuestRam(list))
                return;
            int count = m.ReadU16(list);
            if (count is < 1 or > 0x400)
                return;
            AddList(list, count);
        }
    }

    static int WheelAngle(uint address, IMemory m) =>
        m.ReadU16(address) & 0x0FFF;

    static int ShortestWheelAngleDelta(int current, int previous) =>
        ((current - previous + 0x800) & 0x0FFF) - 0x800;

    /// <summary>
    /// Samples the exact per-wheel transform record consumed by GT2's native
    /// wheel primitive renderer. This hook is deliberately placed before the
    /// guest Euler-to-matrix conversion, so its output cannot be affected by
    /// native scene capture, transform fitting, interpolation, or D3D drawing.
    /// </summary>
    public static void TraceWheelTransform(
        uint carState, uint wheelRecord, uint wheelIndex, IMemory m)
    {
        if (!TraceWheelTransforms)
            return;
        if (wheelIndex > 3u)
        {
            if (Interlocked.Exchange(
                    ref _wheelTransformTraceInvalidIndexReported, 1) == 0)
                Console.Error.WriteLine(
                    $"[GT2-WHEEL] rejected invalid wheel index " +
                    $"{wheelIndex} record=0x{wheelRecord:X8}");
            return;
        }

        int[] angles =
        [
            WheelAngle(wheelRecord + 0x8u, m),
            WheelAngle(wheelRecord + 0xAu, m),
            WheelAngle(wheelRecord + 0xCu, m),
        ];
        short x = (short)m.ReadU16(wheelRecord);
        short y = (short)m.ReadU16(wheelRecord + 0x2u);
        short z = (short)m.ReadU16(wheelRecord + 0x4u);
        ulong key = ((ulong)carState << 2) | wheelIndex;

        lock (WheelTransformTraceLock)
        {
            if (!_wheelTransformTraceExitRegistered)
            {
                _wheelTransformTraceExitRegistered = true;
                WheelTransformTraceCsv.AppendLine(
                    "car_state,wheel,sample,x,y,z,angle_8,angle_a,angle_c," +
                    "delta_8,delta_a,delta_c");
                AppDomain.CurrentDomain.ProcessExit += (_, _) =>
                {
                    lock (WheelTransformTraceLock)
                    {
                        if (!string.IsNullOrWhiteSpace(
                                WheelTransformTracePath))
                        {
                            string path = Path.GetFullPath(
                                WheelTransformTracePath);
                            Directory.CreateDirectory(
                                Path.GetDirectoryName(path)!);
                            File.WriteAllText(
                                path, WheelTransformTraceCsv.ToString());
                        }
                        foreach (var pair in WheelTransformTraceStates)
                        {
                            uint tracedCar = (uint)(pair.Key >> 2);
                            uint tracedWheel = (uint)(pair.Key & 3u);
                            WheelTransformTraceState state = pair.Value;
                            string Axis(int index) =>
                                state.Changes[index] == 0
                                    ? "static"
                                    : $"changes={state.Changes[index]} " +
                                      $"shortest=[{state.MinimumShortestDelta[index]}," +
                                      $"{state.MaximumShortestDelta[index]}]";
                            Console.Error.WriteLine(
                                $"[GT2-WHEEL] car=0x{tracedCar:X8} " +
                                $"wheel={tracedWheel} samples={state.Samples} " +
                                $"angle8({Axis(0)}) angleA({Axis(1)}) " +
                                $"angleC({Axis(2)})");
                        }
                    }
                };
            }

            if (!WheelTransformTraceStates.TryGetValue(
                    key, out WheelTransformTraceState? state))
            {
                state = new WheelTransformTraceState();
                WheelTransformTraceStates.Add(key, state);
            }

            int[] deltas = [0, 0, 0];
            if (state.Samples != 0)
            {
                for (int axis = 0; axis < 3; ++axis)
                {
                    int delta = ShortestWheelAngleDelta(
                        angles[axis], state.Previous[axis]);
                    deltas[axis] = delta;
                    if (delta == 0)
                        continue;
                    ++state.Changes[axis];
                    state.MinimumShortestDelta[axis] = Math.Min(
                        state.MinimumShortestDelta[axis], delta);
                    state.MaximumShortestDelta[axis] = Math.Max(
                        state.MaximumShortestDelta[axis], delta);
                }
            }

            ++state.Samples;
            WheelTransformTraceCsv.Append(
                $"0x{carState:X8},{wheelIndex},{state.Samples}," +
                $"{x},{y},{z},{angles[0]},{angles[1]},{angles[2]}," +
                $"{deltas[0]},{deltas[1]},{deltas[2]}\n");
            Array.Copy(angles, state.Previous, angles.Length);
        }
    }

    /// <summary>
    /// Records the inputs to GT2's conditional four-wheel dispatch gate. The
    /// guest only calls the standalone wheel renderer when distance and render
    /// mode pass this gate; otherwise the visible wheel geometry comes from a
    /// different vehicle-model submission path.
    /// </summary>
    public static void TraceVehicleWheelDispatch(
        uint carState, uint distance, uint renderMode)
    {
        if (!TraceWheelTransforms ||
            Interlocked.Increment(ref _wheelDispatchTraceSamples) > 64)
            return;
        bool distanceEligible = distance < 0x2400u;
        bool modeEligible = (int)renderMode < 3;
        bool detailedDistanceEligible = distance < 0x900u;
        Console.Error.WriteLine(
            $"[GT2-WHEEL-GATE] car=0x{carState:X8} " +
            $"distance=0x{distance:X} mode={renderMode} " +
            $"distanceEligible={distanceEligible} " +
            $"modeEligible={modeEligible} " +
            $"detailEligible={detailedDistanceEligible}");
    }

    static void AuditVisibilityTransition(
        IMemory m,
        uint trackRoot,
        uint stockList,
        int stockCount,
        int currentSector,
        int expandedCount)
    {
        if (_visibilityTransitionTrackRoot == trackRoot &&
            _visibilityTransitionSector == currentSector)
            return;

        Array.Clear(CurrentStockVisibilityEntries);
        Array.Clear(CurrentExpandedVisibilityEntries);
        for (int item = 0; item < stockCount; item++)
        {
            int objectIndex = m.ReadU16(
                stockList + 2u + (uint)item * 2u) & 0x3FFF;
            CurrentStockVisibilityEntries[objectIndex] = true;
        }
        for (int item = 0; item < expandedCount; item++)
        {
            int objectIndex = ExpandedVisibilityEntries[item] & 0x3FFF;
            CurrentExpandedVisibilityEntries[objectIndex] = true;
        }

        if (_visibilityTransitionTrackRoot == trackRoot &&
            _visibilityTransitionSector >= 0)
        {
            int stockAdds = 0;
            int stockRemoves = 0;
            int expandedAdds = 0;
            int expandedRemoves = 0;
            for (int objectIndex = 0;
                 objectIndex < CurrentStockVisibilityEntries.Length;
                 objectIndex++)
            {
                if (CurrentStockVisibilityEntries[objectIndex] &&
                    !PreviousStockVisibilityEntries[objectIndex])
                {
                    stockAdds++;
                    if (PreviousExpandedVisibilityEntries[objectIndex])
                        _visibilityStockAddsPrecovered++;
                }
                else if (!CurrentStockVisibilityEntries[objectIndex] &&
                    PreviousStockVisibilityEntries[objectIndex])
                {
                    stockRemoves++;
                    if (CurrentExpandedVisibilityEntries[objectIndex])
                        _visibilityStockRemovesRetained++;
                }
                if (CurrentExpandedVisibilityEntries[objectIndex] &&
                    !PreviousExpandedVisibilityEntries[objectIndex])
                    expandedAdds++;
                else if (!CurrentExpandedVisibilityEntries[objectIndex] &&
                    PreviousExpandedVisibilityEntries[objectIndex])
                    expandedRemoves++;
            }
            _visibilitySectorTransitions++;
            _visibilityStockTransitionAdds += stockAdds;
            _visibilityStockTransitionRemoves += stockRemoves;
            _visibilityExpandedTransitionAdds += expandedAdds;
            _visibilityExpandedTransitionRemoves += expandedRemoves;
            _visibilityStockMaximumTransition = Math.Max(
                _visibilityStockMaximumTransition,
                stockAdds + stockRemoves);
            _visibilityExpandedMaximumTransition = Math.Max(
                _visibilityExpandedMaximumTransition,
                expandedAdds + expandedRemoves);
        }

        CurrentStockVisibilityEntries.CopyTo(
            PreviousStockVisibilityEntries, 0);
        CurrentExpandedVisibilityEntries.CopyTo(
            PreviousExpandedVisibilityEntries, 0);
        _visibilityTransitionTrackRoot = trackRoot;
        _visibilityTransitionSector = currentSector;
    }

    static void TraceTrackVisibilityLod(
        IMemory m, uint visibilityList, bool maximumLod)
    {
        if (!TraceTrackRendering && !AuditRenderer)
            return;

        if (Volatile.Read(ref _aiAutoDriveReported) >= 2)
            _visibilityLodReplayCalls++;
        else
            _visibilityLodRaceCalls++;
        if (maximumLod)
            _visibilityLodMaximumCalls++;
        else
            _visibilityLodStockCalls++;

        if (visibilityList == 0)
        {
            _visibilityLodNullLists++;
        }
        else if (IsGuestRam(visibilityList))
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
        else
        {
            _visibilityLodInvalidLists++;
            if (VisibilityLodInvalidListSamples.Count < 8)
                VisibilityLodInvalidListSamples.Add(visibilityList);
        }

        if (TraceTrackRendering && !AuditRenderer &&
            Interlocked.Exchange(
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

    /// <summary>
    /// Emits renderer acceptance counters at the runtime's deterministic
    /// shutdown boundary. ProcessExit callbacks are unsuitable as completion
    /// evidence because host-driven termination can occur after redirected
    /// log capture has already completed.
    /// </summary>
    public static void DumpRendererAudit()
    {
        if (!AuditRenderer)
            return;
        Console.Error.WriteLine(
            $"[GT2-Renderer-Audit] track raceCalls={_visibilityLodRaceCalls} " +
            $"replayCalls={_visibilityLodReplayCalls} " +
            $"maximumCalls={_visibilityLodMaximumCalls} " +
            $"stockCalls={_visibilityLodStockCalls} " +
            $"entriesScanned={_visibilityLodEntriesScanned} " +
            $"nonzeroSelectors={_visibilityLodNonzeroSelectors} " +
            $"nullLists={_visibilityLodNullLists} " +
            $"invalidLists={_visibilityLodInvalidLists} " +
            $"invalidSamples=[{string.Join(',', VisibilityLodInvalidListSamples.Select(value => $"0x{value:X8}"))}]");
        Console.Error.WriteLine(
            $"[GT2-Renderer-Audit] visibility functionCalls={_trackVisibilityFunctionCalls} " +
            $"raceCalls={_trackVisibilityRaceCalls} " +
            $"replayCalls={_trackVisibilityReplayCalls} " +
            $"rawEntries={_trackVisibilityRawEntries} " +
            $"rawNonzeroSelectors={_trackVisibilityRawNonzeroSelectors}");
        Console.Error.WriteLine(
            $"[GT2-Renderer-Audit] expandedVisibility calls={_visibilityExpandedCalls} " +
            $"radius={ExtendedVisibilitySectorRadius} " +
            $"stockEntries={_visibilityExpandedStockEntries} " +
            $"outputEntries={_visibilityExpandedOutputEntries} " +
            $"maximumAdded={_visibilityExpandedMaximumAdded}");
        Console.Error.WriteLine(
            $"[GT2-Renderer-Audit] visibilityTransitions " +
            $"sectors={_visibilitySectorTransitions} " +
            $"stockAdds={_visibilityStockTransitionAdds} " +
            $"stockRemoves={_visibilityStockTransitionRemoves} " +
            $"stockAddsPrecovered={_visibilityStockAddsPrecovered} " +
            $"stockRemovesRetained={_visibilityStockRemovesRetained} " +
            $"stockMaximum={_visibilityStockMaximumTransition} " +
            $"expandedAdds={_visibilityExpandedTransitionAdds} " +
            $"expandedRemoves={_visibilityExpandedTransitionRemoves} " +
            $"expandedMaximum={_visibilityExpandedMaximumTransition}");
        lock (VehicleLodSelectorCounts)
        {
            string selectors = string.Join(
                ',',
                VehicleLodSelectorCounts
                    .OrderBy(pair => pair.Key)
                    .Select(pair => $"{pair.Key}:{pair.Value}"));
            Console.Error.WriteLine(
                $"[GT2-Renderer-Audit] vehicles requests={_vehicleLodRequests} " +
                $"selectors=[{selectors}] modelSets={VehicleLodModelSets.Count} " +
                $"selectedModelPointers={VehicleLodModelPointers.Count}");
        }
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

    public static void TraceRenderSchedulerEntry(CpuContext c)
    {
        if (!TraceTrackRendering || _renderSchedulerSamples++ >= 120)
            return;

        Console.Error.WriteLine(
            $"[GT2-Render-Scheduler] poll={Host.InputManager.CurrentPoll} " +
            $"ra=0x{c.RA:X8} a0=0x{c.A0:X8}");
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
        // Widen the display and drawing areas before the title overlay builds
        // its environments.  The Sony demo panel is authored at 512x480.
        if (_overlayPrefix == "gt2_overlay")
        {
            if (index == 1u)
                _unifiedTitleMenuActive = true;
            else if (_unifiedTitleMenuActive)
            {
                _unifiedTitleMenuActive = false;
                Console.WriteLine(
                    $"[GT2] exact title compositor disabled before overlay {index}");
            }
        }
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
            $"callerRA=0x{c.RA:X8} " +
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
