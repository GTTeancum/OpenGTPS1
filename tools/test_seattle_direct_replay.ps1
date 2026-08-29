param(
    [int]$ExitPoll = 45000,
    [ValidateRange(1, 20000)]
    [int]$ReplayProofPoll = 300,
    [int]$RaceProofPoll = -1,
    [switch]$ExitAtReplayHandoff,
    [int]$DiagnosticRacePoll = -1,
    [int]$DiagnosticReplayPoll = -1,
    [switch]$Paced,
    [switch]$AuditGeometryContinuity,
    [switch]$AuditRawTrackVisibility,
    [switch]$AuditNativeOutputHash,
    [switch]$AuditNativeWorldHash,
    [switch]$AuditResidentEquivalence,
    [switch]$AuditResidentLod,
    [switch]$TraceTrackMesh,
    [switch]$TraceTextureInstances,
    [string]$TextureTraceObject = '',
    [string]$TextureTraceModel = '',
    [string]$TextureTraceTPage = '',
    [string]$TextureTraceClut = '',
    [int]$TextureTraceStartPoll = 0,
    [int]$TextureTraceEndPoll = -1,
    [ValidateRange(1, 1000000)]
    [int]$TextureTraceInterval = 1,
    [ValidateRange(1, 1000000)]
    [int]$NativeWorldTraceInterval = 600,
    [ValidateRange(0, 20000)]
    [int]$AuditReplayStartPoll = 0,
    [double]$CoverageTraceMinimum = 1.0,
    [int]$TimeoutSeconds = 1500,
    [string]$ArtifactStem = 'seattle-direct-replay',
    [string]$OutputResolution = '',
    [string]$DeployPath = 'tools\unified-host\bin\Release\net10.0\win-x64',
    [string]$DataPath = 'work\gt2-unified'
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path

function Resolve-RepoPath([string]$Path) {
    if ([IO.Path]::IsPathRooted($Path)) {
        return (Resolve-Path -LiteralPath $Path).Path
    }
    return (Resolve-Path -LiteralPath (Join-Path $repo $Path)).Path
}

function Require([bool]$Condition, [string]$Message) {
    if (-not $Condition) {
        throw $Message
    }
}

Require ($DiagnosticReplayPoll -eq -1 -or $DiagnosticReplayPoll -ge 1) (
    'DiagnosticReplayPoll must be -1 (disabled) or at least 1')
Require ($DiagnosticRacePoll -eq -1 -or $DiagnosticRacePoll -ge 1) (
    'DiagnosticRacePoll must be -1 (disabled) or at least 1')
Require ($RaceProofPoll -eq -1 -or $RaceProofPoll -ge 1) (
    'RaceProofPoll must be -1 (disabled) or at least 1')
$selectedExitModes =
    $(if ($ExitAtReplayHandoff) { 1 } else { 0 }) +
    [int]($RaceProofPoll -ge 0) +
    [int]($DiagnosticRacePoll -ge 0) +
    [int]($DiagnosticReplayPoll -ge 0)
Require ($selectedExitModes -le 1) (
    'Select only one diagnostic or replay-handoff exit mode')
Require ($CoverageTraceMinimum -ge 0.0) (
    'CoverageTraceMinimum cannot be negative')
Require (-not $TraceTextureInstances -or $TextureTraceEndPoll -lt 0 -or
    $TextureTraceStartPoll -le $TextureTraceEndPoll) (
    'TextureTraceStartPoll cannot exceed TextureTraceEndPoll')
Require ([string]::IsNullOrWhiteSpace($OutputResolution) -or
    $OutputResolution -match '^([1-9]\d*)[xX]([1-9]\d*)$') (
    'OutputResolution must be empty or WIDTHxHEIGHT')
Require ($DiagnosticReplayPoll -lt 0 -or
    $AuditReplayStartPoll -le $DiagnosticReplayPoll) (
    'AuditReplayStartPoll cannot exceed DiagnosticReplayPoll')
$diagnosticRace = $DiagnosticRacePoll -ge 0
$diagnosticReplay = $DiagnosticReplayPoll -ge 0
$raceProof = $RaceProofPoll -ge 0
$raceRun = $diagnosticRace -or $raceProof
$noCapture = $ExitAtReplayHandoff -or $diagnosticRace -or $diagnosticReplay
Require (-not $Paced -or
    $diagnosticRace -or
    $diagnosticReplay -or
    $raceProof -or
    $ExitAtReplayHandoff) (
    'Paced validation requires a bounded diagnostic or replay-handoff run')
$pacedStage = if ($diagnosticReplay) { 'replay_1' } else { 'race_1' }

$deploy = Resolve-RepoPath $DeployPath
$data = Resolve-RepoPath $DataPath
$exe = Join-Path $deploy 'GranTurismo2PC.exe'
$card = Join-Path $deploy 'carda.sav'
$runtimeAssembly = Join-Path $deploy 'RecompOne.Runtime.dll'
foreach ($required in @($exe, $card, $runtimeAssembly)) {
    Require (Test-Path -LiteralPath $required -PathType Leaf) (
        "Required file is missing: $required")
}

# A release-package build removes every diagnostic and CPU-driver control at
# module initialization. Detect that build identity before launching instead
# of waiting for a stage hook which can no longer be enabled.
$policyContext = [System.Runtime.Loader.AssemblyLoadContext]::new(
    'OpenGTSeattleHarnessPolicyProbe',
    $true)
try {
    $policyAssembly = $policyContext.LoadFromAssemblyPath($runtimeAssembly)
    $policyType = $policyAssembly.GetType(
        'RecompOne.Runtime.ReleasePolicy',
        $true)
    $releasePackage = [bool]$policyType.GetProperty(
        'IsReleasePackage').GetValue($null)
} finally {
    $policyContext.Unload()
}
Require (-not $releasePackage) (
    'Seattle diagnostics require a development build; the selected deploy ' +
    'contains a release-package runtime which strips all diagnostic controls')

$stdoutPath = Join-Path $repo "work\$ArtifactStem.stdout.log"
$stderrPath = Join-Path $repo "work\$ArtifactStem.stderr.log"
$capturePath = Join-Path $repo "work\$ArtifactStem.ppm"
$cardBackup = Join-Path $repo "work\$ArtifactStem.carda.before.sav"
$captureStage = if ($raceProof) { 'race_1' } else { 'replay_1' }
$capturePoll = if ($raceProof) { $RaceProofPoll } else { $ReplayProofPoll }
$captureLabel = "${captureStage}_$($capturePoll.ToString('0000'))"
$capturePattern = "recompone_present_${captureLabel}_*.ppm"
$allCapturePattern = 'recompone_present_*.ppm'
$existingCaptures = @{}
foreach ($file in Get-ChildItem -LiteralPath $deploy -Filter $allCapturePattern -File) {
    $existingCaptures[$file.FullName] =
        "$($file.LastWriteTimeUtc.Ticks):$($file.Length)"
}

Copy-Item -LiteralPath $card -Destination $cardBackup -Force
$cardHashBefore = (Get-FileHash -LiteralPath $card -Algorithm SHA256).Hash

$start = [Diagnostics.ProcessStartInfo]::new()
$start.FileName = $exe
$start.WorkingDirectory = $deploy
$directMode = if ($raceRun) { '--arcade-race' } else { '--arcade-replay' }
$start.Arguments =
    "--headless $directMode seattle-circuit `"" +
    $data.Replace('"', '\"') + '"'
$start.UseShellExecute = $false
$start.CreateNoWindow = $true
$start.RedirectStandardOutput = $true
$start.RedirectStandardError = $true
$environment = [ordered]@{
    SDL_AUDIODRIVER = 'dummy'
    RECOMPONE_PROCESS_PRIORITY = 'AboveNormal'
    RECOMPONE_DISABLE_LIVE_INPUT = '1'
    RECOMPONE_GT2_AI_AUTODRIVE = $(if ($raceRun) { '1' } else { $null })
    RECOMPONE_GT2_AI_AUTODRIVE_MAX_ENGAGEMENTS = $(
        if ($raceRun) { '2' } else { $null })
    RECOMPONE_SUPPRESS_RUMBLE = '1'
    RECOMPONE_GRAPHICS_PRESET_OVERRIDE = 'Enhanced'
    RECOMPONE_OUTPUT_RESOLUTION = $(
        if ([string]::IsNullOrWhiteSpace($OutputResolution)) {
            $null
        } else { $OutputResolution })
    RECOMPONE_EXIT_AFTER_INPUT_POLL = $ExitPoll.ToString()
    RECOMPONE_UNTHROTTLED = '1'
    RECOMPONE_THROTTLE_ON_SCRIPT_STAGE = $(if ($Paced) { $pacedStage } else { $null })
    RECOMPONE_PRESENTATION_CAPTURE = $(
        if ($noCapture) { $null } else { '1' })
    RECOMPONE_CAPTURE_INPUT_STAGE = $(
        if ($noCapture) { $null } else { $captureStage })
    RECOMPONE_CAPTURE_INPUT_STAGE_POLL = $(
        if ($noCapture) { $null } else { $capturePoll.ToString() })
    RECOMPONE_EXIT_AFTER_PRESENTATION_CAPTURE_LABEL = $(
        if ($noCapture) { $null } else { $captureLabel })
    RECOMPONE_NATIVE_WORLD_EXIT_AFTER_HANDOFF_STAGE = $(
        if ($ExitAtReplayHandoff) { 'replay_1' } else { $null })
    RECOMPONE_TEST_EXIT_INPUT_STAGE = $(
        if ($diagnosticRace) {
            'race_1'
        } elseif ($diagnosticReplay) {
            'replay_1'
        } else { $null })
    RECOMPONE_TEST_EXIT_INPUT_STAGE_POLL = $(
        if ($diagnosticRace) {
            $DiagnosticRacePoll.ToString()
        } elseif ($diagnosticReplay) {
            $DiagnosticReplayPoll.ToString()
        } else { $null })
    RECOMPONE_TRACE_GT2_TRACK_MESH = $(if ($TraceTrackMesh) { '1' } else { $null })
    RECOMPONE_AUDIT_GT2_RAW_TRACK_TEMPORAL_FACING = $(
        if ($AuditGeometryContinuity) { '1' } else { $null })
    RECOMPONE_AUDIT_GT2_RAW_TRACK_VISIBILITY = $(
        if ($AuditRawTrackVisibility) { '1' } else { $null })
    RECOMPONE_AUDIT_NATIVE_WORLD_OUTPUT_HASH = $(
        if ($AuditNativeOutputHash) { '1' } else { $null })
    RECOMPONE_AUDIT_NATIVE_WORLD_HASH = $(
        if ($AuditNativeWorldHash) { '1' } else { $null })
    RECOMPONE_AUDIT_GT2_RESIDENT_EQUIVALENCE = $(
        if ($AuditResidentEquivalence) { '1' } else { $null })
    RECOMPONE_AUDIT_NATIVE_RESIDENT_EQUIVALENCE = $(
        if ($AuditResidentEquivalence) { '1' } else { $null })
    OPENGT_RENDER_RESIDENT_LOD_DIAGNOSTICS = $(
        if ($AuditResidentLod) { '1' } else { $null })
    OPENGT_RENDER_TEXTURE_INSTANCE_DIAGNOSTICS = $(
        if ($TraceTextureInstances) { '1' } else { $null })
    OPENGT_RENDER_TEXTURE_INSTANCE_DIAGNOSTICS_OBJECT = $(
        if ($TraceTextureInstances -and
            -not [string]::IsNullOrWhiteSpace($TextureTraceObject)) {
            $TextureTraceObject
        } else { $null })
    OPENGT_RENDER_TEXTURE_INSTANCE_DIAGNOSTICS_MODEL = $(
        if ($TraceTextureInstances -and
            -not [string]::IsNullOrWhiteSpace($TextureTraceModel)) {
            $TextureTraceModel
        } else { $null })
    OPENGT_RENDER_TEXTURE_INSTANCE_DIAGNOSTICS_TPAGE = $(
        if ($TraceTextureInstances -and
            -not [string]::IsNullOrWhiteSpace($TextureTraceTPage)) {
            $TextureTraceTPage
        } else { $null })
    OPENGT_RENDER_TEXTURE_INSTANCE_DIAGNOSTICS_CLUT = $(
        if ($TraceTextureInstances -and
            -not [string]::IsNullOrWhiteSpace($TextureTraceClut)) {
            $TextureTraceClut
        } else { $null })
    OPENGT_RENDER_TEXTURE_INSTANCE_DIAGNOSTICS_START_POLL = $(
        if ($TraceTextureInstances) { $TextureTraceStartPoll.ToString() } else { $null })
    OPENGT_RENDER_TEXTURE_INSTANCE_DIAGNOSTICS_END_POLL = $(
        if ($TraceTextureInstances -and $TextureTraceEndPoll -ge 0) {
            $TextureTraceEndPoll.ToString()
        } else { $null })
    OPENGT_RENDER_TEXTURE_INSTANCE_DIAGNOSTICS_INTERVAL = $(
        if ($TraceTextureInstances) { $TextureTraceInterval.ToString() } else { $null })
    RECOMPONE_AUDIT_GT2_RAW_TRACK_TEMPORAL_COVERAGE = $(
        if ($AuditGeometryContinuity) { '1' } else { $null })
    RECOMPONE_AUDIT_GT2_RAW_TRACK_NEAR_CLIP = $(
        if ($AuditGeometryContinuity) { '1' } else { $null })
    RECOMPONE_AUDIT_GT2_RAW_TRACK_STAGE = $(
        if ($AuditGeometryContinuity -and $diagnosticRace) {
            'race_1'
        } elseif ($AuditGeometryContinuity) {
            'replay_1'
        } else { $null })
    RECOMPONE_AUDIT_GT2_RAW_TRACK_STAGE_START_POLL = $(
        if ($AuditGeometryContinuity) {
            $AuditReplayStartPoll.ToString()
        } else { $null })
    RECOMPONE_AUDIT_GT2_RAW_TRACK_STAGE_END_POLL = $(
        if ($AuditGeometryContinuity -and $diagnosticRace) {
            $DiagnosticRacePoll.ToString()
        } elseif ($AuditGeometryContinuity -and $diagnosticReplay) {
            $DiagnosticReplayPoll.ToString()
        } elseif ($AuditGeometryContinuity) {
            [int]::MaxValue.ToString()
        } else { $null })
    RECOMPONE_AUDIT_GT2_RAW_TRACK_TEMPORAL_COVERAGE_MIN_DELTA = $(
        if ($AuditGeometryContinuity) {
            $CoverageTraceMinimum.ToString(
                [Globalization.CultureInfo]::InvariantCulture)
        } else { $null })
    RECOMPONE_AUDIT_RENDERER = $(if ($Paced) { $null } else { '1' })
    RECOMPONE_TRACE_PERFORMANCE = '1'
    RECOMPONE_NATIVE_WORLD_TRACE_INTERVAL = $NativeWorldTraceInterval.ToString()
}
foreach ($entry in $environment.GetEnumerator()) {
    if ($null -ne $entry.Value) {
        $start.Environment[$entry.Key] = $entry.Value
    }
}

$process = $null
$stdout = ''
$stderr = ''
$timedOut = $false
try {
    $process = [Diagnostics.Process]::Start($start)
    try {
        $process.PriorityClass = [Diagnostics.ProcessPriorityClass]::AboveNormal
    } catch {
        Write-Warning "Unable to raise process priority: $($_.Exception.Message)"
    }
    $stdoutTask = $process.StandardOutput.ReadToEndAsync()
    $stderrTask = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
        $timedOut = $true
        try {
            $process.Kill($true)
        } catch [Management.Automation.MethodException] {
            Stop-Process -Id $process.Id -Force
        }
        $process.WaitForExit()
    }
    $stdout = $stdoutTask.Result
    $stderr = $stderrTask.Result
} finally {
    [IO.File]::WriteAllText($stdoutPath, $stdout)
    [IO.File]::WriteAllText($stderrPath, $stderr)
    Copy-Item -LiteralPath $cardBackup -Destination $card -Force
    Remove-Item -LiteralPath $cardBackup -Force
}

$cardHashAfter = (Get-FileHash -LiteralPath $card -Algorithm SHA256).Hash
Require ($cardHashAfter -eq $cardHashBefore) 'Memory-card restore hash mismatch'
Require (-not $timedOut) "Seattle diagnostic exceeded ${TimeoutSeconds}s"
Require ($process.ExitCode -eq 0) (
    "Seattle diagnostic exited with code $($process.ExitCode)")
$expectedLaunch = if ($raceRun) { 'race' } else { 'natural replay' }
Require ($stdout -match
    ("\[Host\] direct Arcade " + [regex]::Escape($expectedLaunch) +
     ' requested: seattle-circuit')) (
    'The requested direct Seattle launch path was not selected')
Require ($stderr -match "\[Input\] stage 'race_1'") (
    'GT2 did not enter the CPU-driven Seattle race')
Require ($stderr -match
    '\[GT2-AI\] auto-drive engaged pass=1/\d+ phase=race car=0') (
    'GT2 did not assign its CPU driver to the player car during the race')
if (-not $raceRun) {
    Require ($stderr -match "\[Input\] stage 'replay_1'") (
        'GT2 did not instantiate its natural replay vehicles')
    Require ($stderr -match
        '\[GT2-AI\] auto-drive engaged pass=2/2 phase=replay car=0') (
        'GT2 did not enter its native replay driver phase')
}
Require ($stderr -notmatch '\[GT2-Soak\] quick-win') (
    'A synthetic finish transition was used')
if ($ExitAtReplayHandoff) {
    Require ($stderr -match
        '\[Native-Present\] native-world handoff complete ' +
        'stage=replay_1 poll=') (
        'The full replay did not reach its native 3D-to-Results handoff')
} elseif ($diagnosticReplay) {
    Require ($stderr -match
        ("\[Input\] stage 'replay_1' diagnostic exit at poll " +
         [regex]::Escape($DiagnosticReplayPoll.ToString()) +
         ' \(absolute \d+\)')) (
        'The bounded replay diagnostic did not reach its requested stage poll')
} elseif ($diagnosticRace) {
    Require ($stderr -match
        ("\[Input\] stage 'race_1' diagnostic exit at poll " +
         [regex]::Escape($DiagnosticRacePoll.ToString()) +
         ' \(absolute \d+\)')) (
        'The bounded race diagnostic did not reach its requested stage poll')
} else {
    Require ($stderr -match
        ("\[Host\] native presentation capture=" +
         [regex]::Escape($captureLabel) +
         ' .*synthetic=False repeated=False')) (
        'The Seattle proof was not a fresh authored modern-renderer presentation')
}
if (-not $Paced -and $raceRun) {
    Require ($stderr -match
        '\[GT2-Renderer-Audit\] track raceCalls=([1-9]\d*) replayCalls=0 ' +
        'maximumCalls=([1-9]\d*) stockCalls=0 .*nullLists=0 invalidLists=0') (
        'Maximum course LOD was not proven in the bounded race')
} elseif (-not $Paced) {
    Require ($stderr -match
        '\[GT2-Renderer-Audit\] track raceCalls=([1-9]\d*) replayCalls=([1-9]\d*) ' +
        'maximumCalls=([1-9]\d*) stockCalls=0 .*nullLists=0 invalidLists=0') (
        'Maximum course LOD was not proven in both race and replay')
    $replayWorldFrames = [long]$Matches[2]
    if ($ExitAtReplayHandoff) {
        Require ($replayWorldFrames -ge 600) (
            "Replay handoff occurred after only $replayWorldFrames world frames")
    }
    if ($diagnosticReplay) {
        Require ($replayWorldFrames -ge [Math]::Max(1, $DiagnosticReplayPoll - 10)) (
            "Only $replayWorldFrames replay world frames preceded the bounded exit")
    }
}
if (-not $Paced) {
    Require ($stderr -match
        '\[GT2-Renderer-Audit\] expandedVisibility calls=([1-9]\d*) ' +
        'scope=bounded-sector-horizon radius=3 .*' +
        'maximumAdded=([1-9]\d*)') (
        'The bounded Seattle sector horizon was not exercised')
    $visibilityTransitions = [regex]::Match(
        $stderr,
        '\[GT2-Renderer-Audit\] visibilityTransitions ' +
        'sectors=(\d+) stockAdds=(\d+) stockRemoves=(\d+) ' +
        'stockAddsPrecovered=(\d+) stockRemovesRetained=(\d+) ' +
        'stockMaximum=(\d+) expandedAdds=(\d+) ' +
        'expandedRemoves=(\d+) expandedMaximum=(\d+)')
    Require $visibilityTransitions.Success (
        'The Seattle sector-transition residency audit did not complete')
    $stockAdds = [long]$visibilityTransitions.Groups[2].Value
    $stockRemoves = [long]$visibilityTransitions.Groups[3].Value
    $stockAddsPrecovered = [long]$visibilityTransitions.Groups[4].Value
    $stockRemovesRetained = [long]$visibilityTransitions.Groups[5].Value
    Require ($stockAddsPrecovered -eq $stockAdds) (
        "The bounded horizon missed $($stockAdds - $stockAddsPrecovered) " +
        'objects before their authored sector appeared')
    Require ($stockRemovesRetained -eq $stockRemoves) (
        "The bounded horizon dropped $($stockRemoves - $stockRemovesRetained) " +
        'objects at their authored sector boundary')
    Require ($stderr -match
        '\[GT2-Renderer-Audit\] vehicleIdentity .*fallbackCaptures=0') (
        'A vehicle transform fell back outside its exact GT2 identity scope')
    Require ($stderr -match
        '\[GT2-Renderer-Audit\] wheelGate .*mismatches=0') (
        'GT2 and the modern renderer disagreed on a wheel visibility gate')
}
$rawTrackSummary = [regex]::Match(
    $stderr,
    '\[GT2-Raw-Track-Summary\] frames=(\d+) coverageFrames=(\d+) ' +
    'objectsPerFrame=(\d+)\.\.(\d+) ' +
    'sourcePrimitivesPerFrame=(\d+)\.\.(\d+) .*' +
    'billboards=(\d+)/(\d+) duplicateCalls=0 .*decodeFailures=0 ' +
    'guestTrackFallbacks=0')
Require $rawTrackSummary.Success (
    'Seattle bounded-course decode/residency was not exact on every world frame')
$rawTrackFrames = [long]$rawTrackSummary.Groups[1].Value
$rawTrackCoverageFrames = [long]$rawTrackSummary.Groups[2].Value
$minimumTrackObjects = [long]$rawTrackSummary.Groups[3].Value
$maximumTrackObjects = [long]$rawTrackSummary.Groups[4].Value
$minimumTrackPrimitives = [long]$rawTrackSummary.Groups[5].Value
$maximumTrackPrimitives = [long]$rawTrackSummary.Groups[6].Value
$billboardPrimitives = [long]$rawTrackSummary.Groups[7].Value
$billboardTriangles = [long]$rawTrackSummary.Groups[8].Value
Require ($rawTrackCoverageFrames -eq $rawTrackFrames) (
    'Not every Seattle world frame completed bounded course coverage')
Require ($minimumTrackObjects -gt 0 -and
    $maximumTrackObjects -lt 243 -and
    $minimumTrackPrimitives -gt 0 -and
    $maximumTrackPrimitives -lt 11191) (
    'The Seattle renderer admitted the rejected complete-course catalog')
Require ($billboardPrimitives -gt 0 -and
    $billboardTriangles -eq $billboardPrimitives * 2) (
    'Seattle billboard geometry was absent or not exactly two triangles per quad')
Require ($stderr -match
    '\[GT2-Raw-Background-Summary\] .*decodeFailures=0 ' +
    'guestBackgroundFallbacks=0') (
    'Seattle background decode used a failure or guest-renderer fallback')
Require ($stderr -match
    '\[Render-UV\] commands=[1-9]\d* textured=[1-9]\d* .*' +
    'worldFallback=0 trackFallback=0 vehicleFallback=0 otherFallback=0 .*' +
    'projection=fixed-modern') (
    'A world texture missed the fixed perspective-correct projection path')
Require ($stderr -match
    '\[Render-HUD\] commands=[1-9]\d* components=[1-9]\d* ' +
    'anchors=[1-9]\d*/\d+/[1-9]\d* guest=320x240 ' +
    'policy=relative-edge-groups') (
    'The Seattle HUD did not exercise both relative widescreen margins')
if ($AuditGeometryContinuity) {
    Require ($stderr -match
        '\[GT2-Raw-Track-Summary\].*temporalFacing=\d+/\d+/\d+ ' +
        'temporalFacingVisible=\d+/\d+/\d+/\d+ .*' +
        'temporalCoverage=\d+/\d+/\d+/\d+ .*nearClip=\d+/\d+/\d+/\d+ ') (
        'The requested temporal/near-clip aggregate audit did not complete')
}
if ($TraceTrackMesh) {
    Require ($stderr -match
        '\[GT2-Track-Mesh-Summary\].*invalidIndices=0 invalidPointers=0 .*' +
        'commandMismatch=0 .*effects=0/0 effectPointerInvalid=0 ') (
        'The authored Seattle mesh/effect inventory was incomplete')
}
if (-not [string]::IsNullOrWhiteSpace($OutputResolution)) {
    $resolutionMatch = [regex]::Match(
        $OutputResolution, '^(\d+)[xX](\d+)$')
    $aspectWidth = [long]$resolutionMatch.Groups[1].Value
    $aspectHeight = [long]$resolutionMatch.Groups[2].Value
    $nativeWidth = [Math]::Max(
        320L,
        [long][Math]::Ceiling(240.0 * $aspectWidth / $aspectHeight))
    $expectedSize = "size=$($nativeWidth * 4)x960"
    Require ($stderr -match [regex]::Escape($expectedSize)) (
        "The renderer did not produce the requested Hor+ target $expectedSize")
}
Require ($stderr -match
    '\[Native-World\] shutdown .*synthetic=0 repeated=0 .*dropped=0') (
    'The native world pipeline synthesized, repeated, or dropped a frame')
Require ($stderr -match
    '\[Native-World-Classification\] frames=[1-9]\d* ' +
    'classifiedWorldCommands=[1-9]\d* unclassifiedWorldCommands=0 ' +
    'framesWithUnclassifiedWorld=0 maximumUnclassifiedWorld=0') (
    'A rendered Seattle frame retained ownerless 3D geometry')
Require ($stderr -match '\[Runtime\] shutdown complete; exit=0') (
    'Orderly shutdown was not proven')
Require ($stderr -notmatch
    'Unhandled exception|Fatal error|unknown software exception|' +
    '\[Native-World\] disabled:|submission rejected|dropped truncated|' +
    'compositor fallback viewport=|unmapped call') (
    'The Seattle diagnostic logged a renderer/runtime failure')

if ($Paced) {
    $throttleMarker = "[PERF] throttle engaged at scripted stage '$pacedStage'"
    $throttleOffset = $stderr.IndexOf(
        $throttleMarker,
        [StringComparison]::Ordinal)
    Require ($throttleOffset -ge 0) (
        "Real-time pacing was not engaged at Seattle stage '$pacedStage'")
    $measuredStderr = $stderr.Substring($throttleOffset)
    $metricLines = @([regex]::Matches(
        $measuredStderr,
        '(?m)^\[Native-Present\] hostHz=.*$') |
        ForEach-Object { $_.Value })
    Require ($metricLines.Count -ge 4) (
        "Only $($metricLines.Count) Seattle presentation windows were recorded")

    # The first line straddles the unthrottled setup/paced-stage boundary. Only
    # full 300-presentation windows after that boundary are authoritative.
    $metricLines = @($metricLines | Select-Object -Skip 1)
    $pacedWorldWindows = 0
    foreach ($line in $metricLines) {
        $metric = [regex]::Match(
            $line,
            'hostHz=([0-9.]+) uniqueHz=([0-9.]+) new=(\d+) ' +
            'actual=(\d+) synthetic=(\d+) repeated=(\d+) ' +
            'compositor=(\d+) worldMiss=(\d+) transitionHold=(\d+)')
        Require $metric.Success "Malformed Seattle presentation telemetry: $line"
        $hostRate = [double]::Parse(
            $metric.Groups[1].Value,
            [Globalization.CultureInfo]::InvariantCulture)
        $uniqueRate = [double]::Parse(
            $metric.Groups[2].Value,
            [Globalization.CultureInfo]::InvariantCulture)
        $new = [int]$metric.Groups[3].Value
        $actual = [int]$metric.Groups[4].Value
        $synthetic = [int]$metric.Groups[5].Value
        $repeated = [int]$metric.Groups[6].Value
        $compositor = [int]$metric.Groups[7].Value
        $worldMiss = [int]$metric.Groups[8].Value
        $transitionHold = [int]$metric.Groups[9].Value
        Require (
            $new -eq 300 -and $actual -eq 300 -and $synthetic -eq 0 -and
            $repeated -eq 0 -and $compositor -eq 0 -and $worldMiss -eq 0 -and
            $transitionHold -eq 0
        ) "Incomplete paced Seattle world window: $line"
        Require (
            $hostRate -ge 59.5 -and $hostRate -le 60.5 -and
            $uniqueRate -ge 59.5 -and $uniqueRate -le 60.5
        ) "Seattle presentation cadence fell outside 59.5-60.5 Hz: $line"
        $pacedWorldWindows++
    }
    Require ($pacedWorldWindows -ge 3) (
        "Only $pacedWorldWindows complete paced Seattle world windows were proven")
}

$newAllCaptures = @(Get-ChildItem -LiteralPath $deploy `
    -Filter $allCapturePattern -File |
    Where-Object {
        $signature = "$($_.LastWriteTimeUtc.Ticks):$($_.Length)"
        -not $existingCaptures.ContainsKey($_.FullName) -or
            $existingCaptures[$_.FullName] -ne $signature
    })
if ($noCapture) {
    Require ($newAllCaptures.Count -eq 0) (
        "No-image diagnostic emitted $($newAllCaptures.Count) unsolicited capture(s)")
} else {
    $newCaptures = @($newAllCaptures | Where-Object {
        $_.Name -like $capturePattern
    })
    Require ($newCaptures.Count -eq 1) (
        "Expected one new Seattle proof, found $($newCaptures.Count)")
    Copy-Item -LiteralPath $newCaptures[0].FullName `
        -Destination $capturePath -Force
    foreach ($file in $newAllCaptures) {
        Remove-Item -LiteralPath $file.FullName -Force
    }
}

Write-Host $(if ($ExitAtReplayHandoff) {
    'Full direct Seattle natural replay passed through native-world handoff'
} elseif ($diagnosticReplay) {
    "Direct Seattle replay diagnostic passed through stage poll $DiagnosticReplayPoll"
} elseif ($diagnosticRace) {
    "Direct Seattle race diagnostic passed through stage poll $DiagnosticRacePoll"
} elseif ($raceProof) {
    "Direct Seattle race captured at stage poll $RaceProofPoll"
} else {
    'Direct Seattle natural replay passed'
})
Write-Host "  log: $stderrPath"
if ($Paced) {
    Write-Host "  paced world windows: $pacedWorldWindows"
}
if (-not $noCapture) {
    Write-Host "  single proof: $capturePath"
}
