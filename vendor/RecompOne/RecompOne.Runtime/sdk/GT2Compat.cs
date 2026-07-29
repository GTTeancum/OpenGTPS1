using RecompOne.Runtime.Context;
using RecompOne.Runtime.Memory;

namespace RecompOne.Runtime.Sdk;

/// <summary>
/// Narrow host bridges proven against Gran Turismo 2 SCUS-94488.
/// </summary>
public static class GT2Compat
{
    private sealed class NonLocalJump(uint target) : Exception
    {
        public uint Target { get; } = target;
    }

    public static bool ExpandedPolygonBuffersEnabled =>
        Config.ConfigManager.View.ExtendedDrawDistance ||
        Config.ConfigManager.View.LevelOfDetail.Equals(
            "Maximum", StringComparison.OrdinalIgnoreCase);

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
    static readonly bool AiAutoDrive =
        Environment.GetEnvironmentVariable("RECOMPONE_GT2_AI_AUTODRIVE") == "1";
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
    readonly record struct TrackVisibilitySet(
        int SectorCount,
        ushort[] ObjectIndices);
    static readonly Dictionary<uint, TrackVisibilitySet> TrackVisibilitySets = [];
    static readonly Dictionary<uint, long> VehicleLodSelectorCounts = [];
    static readonly HashSet<uint> VehicleLodModelSets = [];
    static readonly HashSet<uint> VehicleLodModelPointers = [];
    const uint ExpandedVisibilityListAddress = 0x807F0000u;
    static int _bootObjectTraceCount;
    static int _overlayLoadTraceCount;
    static int _finiteCdReadTraceCount;
    static int _menuListTraceCount;
    static uint _overlayIndex;
    static long _vehicleLodRequests;
    static int _vehicleLodTraceRegistered;
    static int _forcedVehicleLodReported;
    static long _aiDriverTicks;
    static int _aiAutoDriveReported;

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
        if (!TraceVehicleLod)
            return;

        uint selector = m.ReadU8(renderRequest);
        uint modelSet = m.ReadU32(renderRequest + 0xCu);
        uint modelPointer = 0;
        if (selector is >= 1u and <= 3u && IsGuestRam(modelSet))
        {
            uint modelIndex = selector - 1u;
            modelPointer =
                m.ReadU32(modelSet + 0x870u + modelIndex * 8u);
        }

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
        // the player car.  The two-pass ceiling prevents a later menu/race
        // object that reuses mode 0 from being touched.
        int engagement = Volatile.Read(ref _aiAutoDriveReported);
        if (engagement >= 2)
            return;

        for (uint index = 0; index < carCount; index++)
        {
            uint car = carArray + index * 0xB40u + 0x2Cu;
            if ((sbyte)m.ReadU8(car + 0x45Du) != 0)
                continue;

            m.WriteU8(car + 0x45Du, 2);
            int pass = Interlocked.Increment(ref _aiAutoDriveReported);
            Console.Error.WriteLine(
                $"[GT2-AI] auto-drive engaged pass={pass}/2 " +
                $"phase={(pass == 1 ? "race" : "replay")} car={index}: " +
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
    /// Overlay 0 normally renders the packed visibility list attached to the
    /// camera's current track sector. Enhanced distance replaces that narrow
    /// list with the union of object indices found in every native sector
    /// visibility list. The original sector's LOD selectors are retained when
    /// maximum LOD is off; objects outside the stock list start at the lowest
    /// detail. Maximum LOD clears the selector bits independently.
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

        // Maximum LOD without extended distance is still a distinct option:
        // copy only the stock visible set and clear its two selector bits.
        if (!extended)
        {
            if (!IsGuestRam(stockList))
                return stockList;
            int visibleCount = Math.Min((int)m.ReadU16(stockList), 0x4000);
            m.WriteU16(ExpandedVisibilityListAddress, (ushort)visibleCount);
            for (int item = 0; item < visibleCount; item++)
                m.WriteU16(
                    ExpandedVisibilityListAddress + 2u + (uint)item * 2u,
                    (ushort)(m.ReadU16(
                        stockList + 2u + (uint)item * 2u) & 0x3FFF));
            TraceTrackVisibilityLod(
                m, ExpandedVisibilityListAddress, maximumLod);
            return ExpandedVisibilityListAddress;
        }

        if (!TrackVisibilitySets.TryGetValue(
                trackRoot, out TrackVisibilitySet visibility))
        {
            visibility = DiscoverTrackVisibility(m, trackRoot);
            TrackVisibilitySets[trackRoot] = visibility;
            ushort first = visibility.ObjectIndices.Length == 0
                ? (ushort)0
                : visibility.ObjectIndices[0];
            ushort last = visibility.ObjectIndices.Length == 0
                ? (ushort)0
                : visibility.ObjectIndices[^1];
            bool contiguous =
                visibility.ObjectIndices.Length > 0 &&
                last - first + 1 == visibility.ObjectIndices.Length;
            Console.Error.WriteLine(
                $"[GT2-Visibility] discovered sectors={visibility.SectorCount} " +
                $"objects={visibility.ObjectIndices.Length} " +
                $"indexRange={first}-{last} contiguous={contiguous} " +
                $"root=0x{trackRoot:X8}");
        }

        int objectCount = visibility.ObjectIndices.Length;
        if (objectCount <= 0 || objectCount > 0x3FFF)
        {
            TraceTrackVisibilityLod(m, stockList, maximumLod);
            return stockList;
        }

        uint output = ExpandedVisibilityListAddress;
        m.WriteU16(output, (ushort)objectCount);
        for (int item = 0; item < objectCount; item++)
        {
            ushort entry = visibility.ObjectIndices[item];
            if (!maximumLod)
                entry |= 0x8000;
            m.WriteU16(output + 2u + (uint)item * 2u, entry);
        }

        if (!maximumLod && IsGuestRam(stockList))
        {
            int stockCount = Math.Min((int)m.ReadU16(stockList), 0x4000);
            for (int item = 0; item < stockCount; item++)
            {
                ushort entry =
                    m.ReadU16(stockList + 2u + (uint)item * 2u);
                ushort index = (ushort)(entry & 0x3FFF);
                int outputIndex = Array.BinarySearch(
                    visibility.ObjectIndices, index);
                if (outputIndex >= 0)
                {
                    m.WriteU16(
                        output + 2u + (uint)outputIndex * 2u, entry);
                }
            }
        }
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

    static TrackVisibilitySet DiscoverTrackVisibility(
        IMemory m, uint trackRoot)
    {
        if (!IsGuestRam(trackRoot))
            return new TrackVisibilitySet(0, []);

        // The sector-pointer table begins at root+0xC and is immediately
        // followed by its first descriptor. Therefore the first descriptor's
        // address defines the table length exactly; do not assume later
        // descriptors are monotonically allocated, which is not a format
        // guarantee and could silently truncate a valid track.
        uint tableStart = trackRoot + 0xCu;
        uint firstDescriptor = m.ReadU32(tableStart);
        uint tableBytes = unchecked(firstDescriptor - tableStart);
        if (!IsGuestRam(firstDescriptor) ||
            firstDescriptor <= tableStart ||
            (tableBytes & 3u) != 0)
            return new TrackVisibilitySet(0, []);
        int sectorCount = checked((int)(tableBytes / 4u));
        if (sectorCount is < 1 or > 0x1000)
            return new TrackVisibilitySet(0, []);

        var objectIndices = new SortedSet<ushort>();
        for (int index = 0; index < sectorCount; index++)
        {
            uint descriptor =
                m.ReadU32(tableStart + (uint)index * 4u);
            if (!IsGuestRam(descriptor))
                return new TrackVisibilitySet(0, []);

            uint visibility = m.ReadU32(descriptor + 0xA0u);
            if (!IsGuestRam(visibility))
                return new TrackVisibilitySet(0, []);
            int visibleCount = m.ReadU16(visibility);
            if (visibleCount <= 0 || visibleCount > 0x400)
                return new TrackVisibilitySet(0, []);

            for (int item = 0; item < visibleCount; item++)
                objectIndices.Add(
                    (ushort)(m.ReadU16(
                        visibility + 2u + (uint)item * 2u) & 0x3FFF));
        }
        return new TrackVisibilitySet(
            sectorCount,
            objectIndices.ToArray());
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
        const uint counterAddress = 0x80011DF4u;
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
        const uint totalCounterAddress = 0x801F0680u;
        const uint intervalCounterAddress = 0x801F0684u;

        int requested = (int)c.A0;
        if (requested < 0)
        {
            m.WriteU32(intervalCounterAddress, 0u);
            requested = 1;
        }

        for (int frame = 0; frame < requested; frame++)
            Runtime.PresentFrame();

        if (requested > 0)
            m.WriteU32(intervalCounterAddress, 0u);
        c.V0 = m.ReadU32(totalCounterAddress);
    }

    public static void WaitForCdCommand(CpuContext c, IMemory m)
    {
        const uint busyAddress = 0x801F0676u;
        const uint state = 0x801F0510u;
        const uint deferredScriptAddress = state + 0x84u;
        const uint currentScriptAddress = state + 0x7Cu;

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
    /// GT2's ring-buffer accessor at 0x80082054 waits for its ready callback
    /// to increment the buffered-sector count at +0x5A. Service the virtual
    /// drive while that original asynchronous producer is starved by static
    /// execution, then return the same current ring-slot pointer as 0x80082000.
    /// </summary>
    public static void WaitForCdBuffer(CpuContext c, IMemory m)
    {
        const uint state = 0x801F0510u;
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

        const uint drive = 0x801F0510u;
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
    public static void RunGuestLoop(CpuContext c, IMemory m, uint entry)
    {
        uint target = entry;
        int transition = 0;
        while (true)
        {
            try
            {
                Dispatch.Dispatcher.Call(c, m, target);
                return;
            }
            catch (NonLocalJump jump)
            {
                target = jump.Target;
                transition++;
                if (TraceBoot)
                    Console.Error.WriteLine(
                        $"[GT2Compat] non-local overlay transition={transition} " +
                        $"target=0x{target:X8}");
            }
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
        uint target = c.A1;
        if (_overlayIndex > 5)
            throw new InvalidOperationException(
                $"GT2 longjmp selected unknown overlay {_overlayIndex}");

        Dispatch.Dispatcher.Load($"gt2_overlay_{_overlayIndex}");

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
        c.V0 = target;

        c.A0 = m.ReadU32(0x801C945Cu);
        c.A1 = m.ReadU32(0x801C9460u);
        c.A2 = m.ReadU32(0x801C9464u);
        c.A3 = m.ReadU32(0x801C9468u);
        if (TraceBoot)
            Console.Error.WriteLine(
                $"[GT2Compat] longjmp overlay={_overlayIndex} target=0x{target:X8}");
        throw new NonLocalJump(target);
    }
}
