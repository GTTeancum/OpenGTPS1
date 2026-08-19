using RecompOne.Runtime.Config;
using System.Reflection;
using System.Runtime.InteropServices;

static void Require(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}

static string ReadRepoFile(string relativePath)
{
    foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
    {
        for (DirectoryInfo? directory = new(start); directory != null; directory = directory.Parent)
        {
            string candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
                return File.ReadAllText(candidate);
        }
    }
    throw new FileNotFoundException($"Could not locate repository file: {relativePath}");
}

static int Occurrences(string text, string value) =>
    text.Split(value, StringSplitOptions.None).Length - 1;

static void RequireModern(ViewConfig view, string context)
{
    Require(view.GraphicsPreset == "Enhanced", $"{context}: preset");
    Require(view.HighResolution3D, $"{context}: 4x source");
    Require(view.TextureSmoothing, $"{context}: smoothing");
    Require(view.PerspectiveCorrectTextures, $"{context}: projection");
    Require(view.StabilizeGeometrySeams, $"{context}: seams");
    Require(view.ExtendedDrawDistance, $"{context}: distance");
    Require(view.LevelOfDetail == "Maximum", $"{context}: LOD");
    Require(!view.Ps1Dithering, $"{context}: dithering");
}

var legacy = new ViewConfig
{
    GraphicsPreset = "PS1 Quality",
    HighResolution3D = false,
    TextureSmoothing = false,
    PerspectiveCorrectTextures = false,
    StabilizeGeometrySeams = false,
    ExtendedDrawDistance = false,
    LevelOfDetail = "Stock",
    Ps1Dithering = true,
};
legacy.EnforceGraphicsPreset();
RequireModern(legacy, "legacy migration");

var custom = new ViewConfig
{
    GraphicsPreset = "Custom",
    HighResolution3D = false,
    TextureSmoothing = false,
    PerspectiveCorrectTextures = false,
    StabilizeGeometrySeams = false,
    ExtendedDrawDistance = false,
    LevelOfDetail = "Stock",
    Ps1Dithering = true,
};
custom.EnforceGraphicsPreset();
RequireModern(custom, "custom migration");

var explicitLegacy = new ViewConfig();
explicitLegacy.ApplyGraphicsPreset("PS1 Quality");
RequireModern(explicitLegacy, "legacy preset request");

var runtimeDowngrade = new ViewConfig();
runtimeDowngrade.HighResolution3D = false;
runtimeDowngrade.TextureSmoothing = false;
runtimeDowngrade.PerspectiveCorrectTextures = false;
runtimeDowngrade.StabilizeGeometrySeams = false;
runtimeDowngrade.ExtendedDrawDistance = false;
runtimeDowngrade.LevelOfDetail = "Stock";
runtimeDowngrade.Ps1Dithering = true;
runtimeDowngrade.GraphicsPreset = "Custom";
RequireModern(runtimeDowngrade, "runtime downgrade request");

Environment.SetEnvironmentVariable("RECOMPONE_NATIVE_WORLD_RENDERER", "0");
Type rendererType = typeof(ViewConfig).Assembly.GetType(
    "RecompOne.Runtime.Hle.LiveWorldRenderer",
    throwOnError: true)!;
Type interpolationStatsType = typeof(ViewConfig).Assembly.GetType(
    "RecompOne.Runtime.Hle.LiveInterpolationStats",
    throwOnError: true)!;
Require(
    Marshal.SizeOf(interpolationStatsType) == 128,
    "managed native-interpolation ABI v4 layout is not 128 bytes");
bool rendererRequested = (bool)rendererType.GetProperty(
    "Requested",
    BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;
Require(
    !OperatingSystem.IsWindows() || rendererRequested,
    "former disable environment variable still disabled native rendering");

foreach (string generatedOverlay in new[]
{
    @"generated\recompiled\gt2_overlay_0.cs",
    @"generated\arcade-recompiled\gt2_arcade_overlay_0.cs",
})
{
    string source = ReadRepoFile(generatedOverlay);
    Require(
        Occurrences(source, "Gte.BeginDerivedScreenProjection(") == 2 &&
        Occurrences(source, "0x31525353u,\n            c.V1)") == 2 &&
        Occurrences(source, "Gte.EndDerivedScreenProjection();") == 2,
        $"{generatedOverlay}: auxiliary billboard projection scopes are not balanced");
}
int outputReadyWaitMilliseconds = (int)rendererType.GetField(
    "OutputReadyWaitMilliseconds",
    BindingFlags.NonPublic | BindingFlags.Static)!.GetRawConstantValue()!;
Require(
    outputReadyWaitMilliseconds is >= 1 and <= 12,
    "native output wait exceeded the bounded NTSC idle margin");
int liveTriangleStride = (int)rendererType.GetField(
    "TriangleStride",
    BindingFlags.NonPublic | BindingFlags.Static)!.GetRawConstantValue()!;
Require(
    liveTriangleStride == 384,
    "live world capture is not using the v6 per-vertex transform stride");
int outputBufferCount = (int)rendererType.GetField(
    "OutputBufferCount",
    BindingFlags.NonPublic | BindingFlags.Static)!.GetRawConstantValue()!;
int publishedOutputCapacity = (int)rendererType.GetField(
    "PublishedOutputCapacity",
    BindingFlags.NonPublic | BindingFlags.Static)!.GetRawConstantValue()!;
Require(
    outputBufferCount == 11 && publishedOutputCapacity == 8,
    "native output ring lacks host + worker ownership beside eight outputs");

Type recorderType = rendererType.Assembly.GetType(
    "RecompOne.Runtime.Hle.LiveWorldFrameRecorder",
    throwOnError: true)!;
int reservationRecords = (int)recorderType.GetField(
    "TriangleReservationRecords",
    BindingFlags.NonPublic | BindingFlags.Static)!.GetRawConstantValue()!;
Require(
    reservationRecords is >= 16 and <= 256,
    "direct capture writer reservation is not bounded in useful batches");
MethodInfo ensureTriangleStorage = recorderType.GetMethod(
    "EnsureTriangleStorage",
    BindingFlags.NonPublic | BindingFlags.Static)!;
const int captureHeaderBytes = 160;
const int triangleRecordBytes = 224;
byte[] captureBacking = new byte[128 * 1024];
using (var captureStream = new MemoryStream(
    captureBacking,
    0,
    captureBacking.Length,
    writable: true,
    publiclyVisible: true))
{
    captureStream.SetLength(captureHeaderBytes);
    captureBacking[0] = 0x4F;
    captureBacking[captureHeaderBytes - 1] = 0xA5;
    long triangleEnd = captureHeaderBytes + triangleRecordBytes;
    ensureTriangleStorage.Invoke(
        null,
        [captureStream, triangleEnd, (long)captureBacking.Length]);
    Require(
        captureStream.Length >= triangleEnd,
        "direct triangle backing bytes were not exposed to MemoryStream");
    captureBacking[captureHeaderBytes] = 0x71;
    captureBacking[triangleEnd - 1] = 0xE3;
    captureStream.Position = triangleEnd;
    // The frame finalizer grows/shrinks to the exact triangle + VRAM extent.
    // Both ends of the directly written record must survive that operation.
    captureStream.SetLength(64 * 1024);
    Require(
        captureBacking[0] == 0x4F &&
        captureBacking[captureHeaderBytes - 1] == 0xA5 &&
        captureBacking[captureHeaderBytes] == 0x71 &&
        captureBacking[triangleEnd - 1] == 0xE3,
        "MemoryStream finalization erased direct triangle capture bytes");
}

Type gpuType = rendererType.Assembly.GetType(
    "RecompOne.Runtime.Gpu",
    throwOnError: true)!;
MethodInfo compatibilityRasterDecision = gpuType
    .GetMethod(
        "ShouldRasterizeCompatibilityTriangle",
        BindingFlags.NonPublic | BindingFlags.Static)!;
bool rasterizesWorld = (bool)compatibilityRasterDecision.Invoke(
    null,
    [true])!;
bool rasterizesScreen = (bool)compatibilityRasterDecision.Invoke(
    null,
    [false])!;
Require(
    !OperatingSystem.IsWindows() || !rasterizesWorld,
    "provenance-backed 3D can still reach a compatibility rasterizer");
Require(
    rasterizesScreen,
    "authored screen-space command composition was disabled");
MethodInfo commandMatchRate = rendererType.GetMethod(
    "CommandMatchRate",
    BindingFlags.NonPublic | BindingFlags.Static)!;
Require(
    Math.Abs((double)commandMatchRate.Invoke(null, [90U, 100U])! - 90.0) <
        0.001 &&
    (double)commandMatchRate.Invoke(null, [0U, 0U])! == 0.0,
    "interpolation diagnostics use a partial world-only denominator");
Type gteType = rendererType.Assembly.GetType(
    "RecompOne.Runtime.Gte",
    throwOnError: true)!;
MethodInfo packetOriginCoordinatesMatch = gteType.GetMethod(
    "PacketOriginCoordinatesMatch",
    BindingFlags.NonPublic | BindingFlags.Static)!;
const int packetX = 287;
const int packetY = 38;
uint packedPacketXy =
    (uint)((ushort)(short)packetX |
        ((uint)(ushort)(short)packetY << 16));
Require(
    (bool)packetOriginCoordinatesMatch.Invoke(
        null,
        [packedPacketXy, packetX, packetY])! &&
    !(bool)packetOriginCoordinatesMatch.Invoke(
        null,
        [packedPacketXy, packetX + 1, packetY])! &&
    !(bool)packetOriginCoordinatesMatch.Invoke(
        null,
        [packedPacketXy, packetX, packetY + 1])!,
    "reused packet words can inherit stale world projection origins");
MethodInfo liveWorldExpectedFromHistory = gpuType.GetMethod(
    "IsLiveWorldExpectedFromHistory",
    BindingFlags.NonPublic | BindingFlags.Static)!;
Require(
    !(bool)liveWorldExpectedFromHistory.Invoke(null, [false, 0])! &&
    (bool)liveWorldExpectedFromHistory.Invoke(null, [true, 0])! &&
    (bool)liveWorldExpectedFromHistory.Invoke(null, [true, 1])! &&
    (bool)liveWorldExpectedFromHistory.Invoke(null, [true, 2])! &&
    (bool)liveWorldExpectedFromHistory.Invoke(null, [true, 6])! &&
    !(bool)liveWorldExpectedFromHistory.Invoke(null, [false, 6])!,
    "native world ownership still mistakes SSR11 packet gaps for scene handoffs");

Type hostWindowType = rendererType.Assembly.GetType(
    "RecompOne.Runtime.Host.HostWindow",
    throwOnError: true)!;
Type configManagerType = rendererType.Assembly.GetType(
    "RecompOne.Runtime.Config.ConfigManager",
    throwOnError: true)!;
MethodInfo hasDockedOutputLayout = configManagerType.GetMethod(
    "HasDockedOutputLayout",
    BindingFlags.NonPublic | BindingFlags.Static)!;
const string incompleteLayout =
    "[Window][Debug##Default]\nPos=60,60\n\n[Docking][Data]\n";
const string dockedOutputLayout =
    "[Window][Output]\nDockId=0xBBE43958\n\n[Docking][Data]\n" +
    "DockSpace ID=0xBBE43958 Size=1920,1042 CentralNode=1\n";
Require(
    !(bool)hasDockedOutputLayout.Invoke(null, [incompleteLayout])! &&
    (bool)hasDockedOutputLayout.Invoke(null, [dockedOutputLayout])!,
    "incomplete interface layout can suppress the full-window Output dock");
Type frameClockType = rendererType.Assembly.GetType(
    "RecompOne.Runtime.Host.FrameClock",
    throwOnError: true)!;
double maximumCatchUpDebtMs = (double)frameClockType.GetField(
    "MaximumCatchUpDebtMs",
    BindingFlags.NonPublic | BindingFlags.Static)!.GetRawConstantValue()!;
double frameMs = (double)frameClockType.GetField(
    "FrameMs",
    BindingFlags.NonPublic | BindingFlags.Static)!.GetRawConstantValue()!;
Require(
    maximumCatchUpDebtMs >= frameMs * 2.0 &&
    maximumCatchUpDebtMs <= frameMs * 2.01,
    "frame clock can accelerate gameplay to repay long scheduler stalls");
MethodInfo shouldResetDeadline = frameClockType.GetMethod(
    "ShouldResetDeadline",
    BindingFlags.NonPublic | BindingFlags.Static)!;
Require(
    !(bool)shouldResetDeadline.Invoke(null, [-frameMs * 1.99])! &&
    (bool)shouldResetDeadline.Invoke(null, [-frameMs * 2.01])!,
    "frame clock recovery boundary does not enforce two NTSC intervals");
MethodInfo shouldThrottleForStage = frameClockType.GetMethod(
    "ShouldThrottleForStage",
    BindingFlags.NonPublic | BindingFlags.Static)!;
Require(
    !(bool)shouldThrottleForStage.Invoke(
        null,
        [false, "setup", "race_1"])! &&
    (bool)shouldThrottleForStage.Invoke(
        null,
        [false, "race_1", "race_1"])! &&
    (bool)shouldThrottleForStage.Invoke(
        null,
        [true, "replay_1", "race_1"])!,
    "script-stage release pacing does not remain latched after scene change");
int prebufferOutputs = (int)hostWindowType.GetField(
    "NativeWorldPrebufferOutputs",
    BindingFlags.NonPublic | BindingFlags.Static)!.GetRawConstantValue()!;
int initialPrebufferOutputs = (int)hostWindowType.GetField(
    "NativeWorldInitialPrebufferOutputs",
    BindingFlags.NonPublic | BindingFlags.Static)!.GetRawConstantValue()!;
Require(
    prebufferOutputs == 8 && initialPrebufferOutputs == 8,
    "native presentation reserve does not retain four unique frames");
MethodInfo nativePrebufferTarget = hostWindowType.GetMethod(
    "SelectNativeWorldPrebufferTarget",
    BindingFlags.NonPublic | BindingFlags.Static)!;
MethodInfo nativeStaleDiscardBeforePoll = hostWindowType.GetMethod(
    "SelectNativeWorldStaleDiscardBeforePoll",
    BindingFlags.NonPublic | BindingFlags.Static)!;
MethodInfo nativeShouldStartPrebuffer = hostWindowType.GetMethod(
    "ShouldStartNativeWorldPrebuffer",
    BindingFlags.NonPublic | BindingFlags.Static)!;
MethodInfo nativeOutputWaitMilliseconds = hostWindowType.GetMethod(
    "SelectNativeWorldOutputWaitMilliseconds",
    BindingFlags.NonPublic | BindingFlags.Static)!;
MethodInfo nativePendingOutputHold = hostWindowType.GetMethod(
    "ShouldHoldForPendingNativeWorldOutput",
    BindingFlags.NonPublic | BindingFlags.Static)!;
Require(
    (int)nativePrebufferTarget.Invoke(null, [true, true, true, 0])! == 8 &&
    (int)nativePrebufferTarget.Invoke(null, [false, true, false, 4])! == 8 &&
    (int)nativePrebufferTarget.Invoke(null, [false, true, true, 2])! == 6 &&
    (int)nativePrebufferTarget.Invoke(null, [false, true, true, 7])! == 7 &&
    (int)nativePrebufferTarget.Invoke(null, [false, true, true, 10])! == 8,
    "recent native ownership-gap prebuffer target is not bounded");
Require(
    (int)nativeStaleDiscardBeforePoll.Invoke(null, [1000])! == 994,
    "new native world segments can still count stale queued outputs");
Require(
    (bool)nativeShouldStartPrebuffer.Invoke(
        null,
        [true, true, true, 0])! &&
    !(bool)nativeShouldStartPrebuffer.Invoke(
        null,
        [false, false, true, 0])! &&
    !(bool)nativeShouldStartPrebuffer.Invoke(
        null,
        [true, false, true, 0])! &&
    !(bool)nativeShouldStartPrebuffer.Invoke(
        null,
        [true, false, true, 10])! &&
    (bool)nativeShouldStartPrebuffer.Invoke(
        null,
        [true, false, true, 11])! &&
    (bool)nativeShouldStartPrebuffer.Invoke(
        null,
        [true, false, false, 1])!,
    "short in-race native ownership gaps still restart the output prebuffer");
Require(
    (int)nativeOutputWaitMilliseconds.Invoke(null, [true, true])! == 8 &&
    (int)nativeOutputWaitMilliseconds.Invoke(null, [true, false])! == 0 &&
    (int)nativeOutputWaitMilliseconds.Invoke(null, [false, true])! == 0,
    "native output wait can still run outside paced 60 Hz world evidence");
Require(
    (bool)nativePendingOutputHold.Invoke(
        null,
        [true, false, false, true, 7])! &&
    (bool)nativePendingOutputHold.Invoke(
        null,
        [true, false, false, true, 16])! &&
    !(bool)nativePendingOutputHold.Invoke(
        null,
        [true, false, false, true, 17])! &&
    !(bool)nativePendingOutputHold.Invoke(
        null,
        [true, false, false, false, 7])! &&
    !(bool)nativePendingOutputHold.Invoke(
        null,
        [true, true, false, true, 7])!,
    "pending native output hold no longer stays bounded to short producer tails");
MethodInfo nativePresentationDecision = hostWindowType.GetMethod(
    "ShouldPresentNativeWorld",
    BindingFlags.NonPublic | BindingFlags.Static)!;
bool presentsNewWorld = (bool)nativePresentationDecision.Invoke(
    null,
    [true, true, false, true])!;
bool presentsRecentWorld = (bool)nativePresentationDecision.Invoke(
    null,
    [true, false, true, false])!;
bool suppressesResetBoundaryReuse = (bool)nativePresentationDecision.Invoke(
    null,
    [true, false, true, true])!;
bool presentsStaleWorld = (bool)nativePresentationDecision.Invoke(
    null,
    [true, false, false, false])!;
bool presentsAfterWorldHandoff = (bool)nativePresentationDecision.Invoke(
    null,
    [false, true, true, false])!;
Require(presentsNewWorld, "new native output was suppressed");
Require(presentsRecentWorld, "bounded current-segment native output was suppressed");
Require(
    !suppressesResetBoundaryReuse,
    "single-output temporal reset can still be reused as native motion");
Require(!presentsStaleWorld, "stale native texture can leak across a scene transition");
Require(
    !presentsAfterWorldHandoff,
    "native output can outlive authored world ownership");
MethodInfo nativeTemporalResetBoundaryHold = hostWindowType.GetMethod(
    "ShouldHoldTemporalResetBoundary",
    BindingFlags.NonPublic | BindingFlags.Static)!;
Require(
    (bool)nativeTemporalResetBoundaryHold.Invoke(
        null,
        [true, false, true, true])! &&
    !(bool)nativeTemporalResetBoundaryHold.Invoke(
        null,
        [true, true, true, true])! &&
    !(bool)nativeTemporalResetBoundaryHold.Invoke(
        null,
        [true, false, true, false])!,
    "single-output temporal reset is not reported as a transition hold");
MethodInfo nativeOutputAgeEligible = hostWindowType.GetMethod(
    "IsNativeWorldOutputAgeEligible",
    BindingFlags.NonPublic | BindingFlags.Static)!;
Require(
    !(bool)nativeOutputAgeEligible.Invoke(null, [-1])! &&
    (bool)nativeOutputAgeEligible.Invoke(null, [0])! &&
    (bool)nativeOutputAgeEligible.Invoke(null, [6])! &&
    !(bool)nativeOutputAgeEligible.Invoke(null, [7])!,
    "native texture reuse age guard no longer rejects stale frames");

Console.WriteLine(
    "modern_renderer_config=pass legacy_migration=pass " +
    "custom_migration=pass runtime_downgrade_removed=pass " +
    "native_disable_removed=pass legacy_world_rasterization_removed=pass " +
    "screen_compositor_retained=pass stale_world_transition_blocked=pass " +
    "bounded_output_wait=pass native_pair_prebuffer=pass " +
    "native_output_ring=pass capture_stream_reservation=pass " +
    "native_capture_v6=pass " +
    "native_interpolation_abi=pass " +
    "auxiliary_billboard_projection_scopes=pass " +
    "stage_throttle_latch=pass output_dock_validation=pass " +
    "native_reuse_age_guard=pass packet_origin_coordinate_guard=pass " +
    "bounded_world_ownership=pass");
