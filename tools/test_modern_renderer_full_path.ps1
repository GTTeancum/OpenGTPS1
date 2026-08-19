param(
    [string]$ArtifactName = 'modern-renderer-full-path-current',
    [int]$ExitPoll = 24500,
    [int]$TimeoutSeconds = 420,
    [switch]$CaptureEvidence,
    [switch]$AboveNormalPriority,
    [string]$Fixture = 'tests\fixtures\modern-renderer-replay-soak.input',
    [string]$DeployPath = 'tools\unified-host\bin\Release\net10.0',
    [string]$DataPath = 'work\gt2-unified'
)

$ErrorActionPreference = 'Stop'
if ($IsWindows -or $env:OS -eq 'Windows_NT') {
    [Diagnostics.Process]::GetCurrentProcess().PriorityClass =
        [Diagnostics.ProcessPriorityClass]::BelowNormal
}
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path

function Resolve-RepoPath([string]$Path) {
    if ([IO.Path]::IsPathRooted($Path)) {
        return (Resolve-Path -LiteralPath $Path).Path
    }
    return (Resolve-Path -LiteralPath (Join-Path $repo $Path)).Path
}

$deploy = Resolve-RepoPath $DeployPath
$data = Resolve-RepoPath $DataPath
$fixturePath = Resolve-RepoPath $Fixture
$exe = Join-Path $deploy 'GranTurismo2PC.exe'
$card = Join-Path $deploy 'carda.sav'
foreach ($required in @($exe, $card, $fixturePath)) {
    if (-not (Test-Path -LiteralPath $required -PathType Leaf)) {
        throw "Required file is missing: $required"
    }
}
if (-not (Test-Path -LiteralPath $data -PathType Container)) {
    throw "Unified data directory is missing: $data"
}

$artifact = Join-Path $repo "artifacts\$ArtifactName"
New-Item -ItemType Directory -Path $artifact -Force | Out-Null
$stdoutPath = Join-Path $artifact 'stdout.log'
$stderrPath = Join-Path $artifact 'stderr.log'
$cardBackup = Join-Path $artifact 'carda.before.sav'
Copy-Item -LiteralPath $card -Destination $cardBackup -Force
$cardHashBefore = (Get-FileHash -LiteralPath $card -Algorithm SHA256).Hash

$runtimeFixture = $fixturePath
if (-not $CaptureEvidence) {
    $runtimeFixture = Join-Path $artifact 'pacing-no-capture.input'
    Get-Content -LiteralPath $fixturePath |
        Where-Object {
            $_ -notmatch '^\s*\d+\s*\+\s*\d+\s*=\s*CAPTURE\s*(#.*)?$'
        } |
        Set-Content -LiteralPath $runtimeFixture -Encoding ASCII
}

$captureNames = @(
    'recompone_present_replay_1_7400_1368x1026_fxaa.ppm',
    'recompone_present_replay_1_7500_1368x1026_fxaa.ppm',
    'recompone_present_replay_1_7600_1368x1026_fxaa.ppm',
    'recompone_present_replay_1_7800_1368x1026_fxaa.ppm',
    'recompone_present_replay_1_8000_1368x1026_fxaa.ppm'
)
if ($CaptureEvidence) {
    foreach ($name in $captureNames) {
        $generated = Join-Path $deploy $name
        if (Test-Path -LiteralPath $generated -PathType Leaf) {
            Remove-Item -LiteralPath $generated -Force
        }
    }
}

$start = [Diagnostics.ProcessStartInfo]::new()
$start.FileName = $exe
$start.WorkingDirectory = $deploy
$start.Arguments = '--headless "' + $data.Replace('"', '\"') + '"'
$start.UseShellExecute = $false
$start.CreateNoWindow = $true
$start.RedirectStandardOutput = $true
$start.RedirectStandardError = $true
$childPriority = if ($AboveNormalPriority) { 'AboveNormal' } else { 'BelowNormal' }
$environment = [ordered]@{
    SDL_AUDIODRIVER = 'dummy'
    RECOMPONE_PROCESS_PRIORITY = $childPriority
    RECOMPONE_INPUT_FILE = $runtimeFixture
    RECOMPONE_DISABLE_LIVE_INPUT = '1'
    RECOMPONE_SUPPRESS_RUMBLE = '1'
    RECOMPONE_GT2_AI_AUTODRIVE = '1'
    RECOMPONE_GT2_AI_AUTODRIVE_MAX_ENGAGEMENTS = '2'
    RECOMPONE_GT2_SOAK_QUICK_WIN_AFTER_AI_TICKS = '600'
    RECOMPONE_GT2_CREATE_TEST_SAVE = '1'
    RECOMPONE_GRAPHICS_PRESET_OVERRIDE = 'Enhanced'
    RECOMPONE_EXIT_AFTER_INPUT_POLL = $ExitPoll.ToString()
    RECOMPONE_UNTHROTTLED = '1'
    # Fast-forward only the 2D setup. Engage release pacing before the first
    # authored 3D frame so the soak cannot manufacture an unlimited producer
    # load, leave a GPU backlog, and mislabel its recovery as 60 Hz health.
    RECOMPONE_THROTTLE_ON_SCRIPT_STAGE = 'race_1'
    RECOMPONE_TRACE_PERFORMANCE = '1'
    RECOMPONE_NATIVE_WORLD_TRACE_INTERVAL = '300'
    RECOMPONE_AUDIT_RENDERER = '1'
    # Full-resolution OpenGL readback is deliberately excluded from pacing
    # validation. It synchronizes glFinish/readback and can add 18-26 ms to a
    # host frame. Capture evidence is a separate, explicitly instrumented run.
    RECOMPONE_PRESENTATION_CAPTURE = $(if ($CaptureEvidence) { '1' } else { $null })
    RECOMPONE_DISABLE_DISPLAY_CAPTURE = $(if ($CaptureEvidence) { $null } else { '1' })
}
foreach ($entry in $environment.GetEnumerator()) {
    if ($null -eq $entry.Value) {
        $null = $start.Environment.Remove($entry.Key)
    } else {
        $start.Environment[$entry.Key] = $entry.Value
    }
}

$process = $null
$stdout = ''
$stderr = ''
$timedOut = $false
try {
    $process = [Diagnostics.Process]::Start($start)
    if ($AboveNormalPriority) {
        try {
            $process.PriorityClass =
                [Diagnostics.ProcessPriorityClass]::AboveNormal
        } catch {
            Write-Warning (
                "Unable to raise process priority: $($_.Exception.Message)")
        }
    } else {
        $process.PriorityClass =
            [Diagnostics.ProcessPriorityClass]::BelowNormal
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
}

$cardHashAfter = (Get-FileHash -LiteralPath $card -Algorithm SHA256).Hash
if ($cardHashAfter -ne $cardHashBefore) {
    throw 'Memory-card restore hash mismatch'
}
if ($timedOut) {
    throw "Modern-renderer full path exceeded ${TimeoutSeconds}s"
}
if ($process.ExitCode -ne 0) {
    throw "Modern-renderer full path exited with code $($process.ExitCode)"
}
if ($stderr -notmatch 'headless audio backend=dummy' -or
    $stderr -notmatch 'SDL audio ready: driver=dummy') {
    throw 'Dummy audio backend was not proven'
}
if ($stderr -notmatch "\[Input\] stage 'replay_1'") {
    throw 'Natural replay stage was not reached'
}
if ($stderr -notmatch
    "\[PERF\] throttle engaged at scripted stage 'race_1'") {
    throw 'Release pacing was not engaged before the first race frame'
}
if ($stderr -notmatch '\[Runtime\] shutdown complete; exit=0') {
    throw 'Orderly shutdown was not proven'
}
if ($stderr -match
    'Unhandled exception|Fatal error|unknown software exception|' +
    '\[Native-World\] disabled:|submission rejected|dropped truncated|' +
    'compositor fallback viewport=|unmapped call') {
    throw 'Renderer/runtime failure marker was logged'
}

$throttleMarker = '[PERF] throttle engaged at scripted stage'
$throttleOffset = $stderr.IndexOf(
    $throttleMarker,
    [StringComparison]::Ordinal)
if ($throttleOffset -lt 0) {
    throw 'Real-time replay measurement boundary was not reached'
}
$measuredStderr = $stderr.Substring($throttleOffset)
$metricLines = [regex]::Matches(
    $measuredStderr,
    '(?m)^\[Native-Present\].*$') | ForEach-Object { $_.Value }
if ($metricLines.Count -lt 2) {
    throw 'No native presentation telemetry was recorded'
}
# The marker is deliberately raised in the middle of the current 300-poll
# telemetry window when fast-forward hands off to real-time replay. That first
# partial window contains pre-boundary samples and is warmup, not an
# authoritative 300-presentation measurement.
$warmupMetricLine = $metricLines[0]
$metricLines = @($metricLines | Select-Object -Skip 1)
$worldMiss = 0
$resetRepeats = 0
$transitionHolds = 0
$perfectWorldWindows = 0
$pacedWorldWindows = 0
$worldPacingViolations = 0
$pureCompositorWindows = 0
$compositorFrames = 0
foreach ($line in $metricLines) {
    $metric = [regex]::Match(
        $line,
        'hostHz=([0-9.]+) uniqueHz=([0-9.]+) new=(\d+) ' +
        'actual=(\d+) synthetic=(\d+) repeated=(\d+) ' +
        'compositor=(\d+) worldMiss=(\d+) transitionHold=(\d+)')
    if (-not $metric.Success) {
        throw "Malformed native presentation telemetry: $line"
    }
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
    $lineWorldMiss = [int]$metric.Groups[8].Value
    $lineTransitionHolds = [int]$metric.Groups[9].Value
    $worldMiss += $lineWorldMiss
    $resetRepeats += $repeated
    $transitionHolds += $lineTransitionHolds
    $compositorFrames += $compositor
    $completeWorld =
        $new -eq 300 -and $compositor -eq 0 -and
        $lineTransitionHolds -eq 0
    if ($completeWorld) {
        if ($actual + $synthetic -ne 300 -or $repeated -ne 0 -or
            $lineWorldMiss -ne 0) {
            throw "Invalid complete modern-world window: $line"
        }
        $perfectWorldWindows++
        if (
            $hostRate -ge 59.5 -and $hostRate -le 60.5 -and
            $uniqueRate -ge 59.5 -and $uniqueRate -le 60.5
        ) {
            $pacedWorldWindows++
        }
        else {
            $worldPacingViolations++
        }
    }
    if ($new -eq 0 -and $actual -eq 0 -and $synthetic -eq 0 -and
        $repeated -eq 0 -and $compositor -eq 300 -and
        $lineWorldMiss -eq 0) {
        $pureCompositorWindows++
    }
}
if ($worldMiss -ne 0) {
    throw "Modern world missed $worldMiss presentation(s)"
}
if ($resetRepeats -ne 0) {
    throw "Modern world repeated $resetRepeats presentation(s)"
}
if ($transitionHolds -gt 30) {
    throw (
        "Expected at most 30 explicitly labelled 2D/3D ownership holds; " +
        "observed $transitionHolds")
}
if ($perfectWorldWindows -lt 20) {
    throw "Only $perfectWorldWindows complete 150/150 modern-world windows"
}
if (
    $pacedWorldWindows -ne $perfectWorldWindows -or
    $worldPacingViolations -ne 0
) {
    throw (
        "$worldPacingViolations complete world window(s) fell outside " +
        '59.5-60.5 host/unique Hz')
}
if ($compositorFrames -lt 100) {
    throw "Only $compositorFrames authored 2D Results compositor frames were proven"
}

$visibilityLod = [regex]::Match(
    $stderr,
    '\[GT2-Renderer-Audit\] track raceCalls=(\d+) replayCalls=(\d+) ' +
    'maximumCalls=(\d+) stockCalls=(\d+) entriesScanned=(\d+) ' +
    'nonzeroSelectors=(\d+) nullLists=(\d+) invalidLists=(\d+)')
if (-not $visibilityLod.Success) {
    throw 'Race/replay track-visibility LOD summary was not recorded'
}
$raceLodCalls = [long]$visibilityLod.Groups[1].Value
$replayLodCalls = [long]$visibilityLod.Groups[2].Value
$maximumLodCalls = [long]$visibilityLod.Groups[3].Value
$stockLodCalls = [long]$visibilityLod.Groups[4].Value
$lodEntries = [long]$visibilityLod.Groups[5].Value
$nonzeroLodSelectors = [long]$visibilityLod.Groups[6].Value
$nullLodLists = [long]$visibilityLod.Groups[7].Value
$invalidLodLists = [long]$visibilityLod.Groups[8].Value
if (
    $raceLodCalls -lt 1 -or $replayLodCalls -lt 1 -or
    $maximumLodCalls -ne ($raceLodCalls + $replayLodCalls) -or
    $stockLodCalls -ne 0 -or $lodEntries -lt 1 -or
    $nonzeroLodSelectors -ne 0 -or $invalidLodLists -ne 0
) {
    throw (
        'Maximum track LOD was not proven across race/replay: ' +
        $visibilityLod.Value)
}
$vehicleLod = [regex]::Match(
    $stderr,
    '\[GT2-Renderer-Audit\] vehicles requests=(\d+) ' +
    'selectors=\[1:(\d+)\]')
if (-not $vehicleLod.Success -or
    [long]$vehicleLod.Groups[1].Value -ne
        [long]$vehicleLod.Groups[2].Value) {
    throw 'Every rendered vehicle did not use highest-detail selector 1'
}

$profile = [regex]::Match(
    $stderr,
    '\[Native-Profile\] scope=track-world samples=(\d+) ' +
    'pipelineP50Ms=([0-9.]+) pipelineP95Ms=([0-9.]+) ' +
    'pipelineP99Ms=([0-9.]+) submitP50Ms=([0-9.]+) ' +
    'submitP95Ms=([0-9.]+) submitP99Ms=([0-9.]+) ' +
    'topologyP50Ms=([0-9.]+) topologyP95Ms=([0-9.]+) ' +
    'topologyP99Ms=([0-9.]+)')
if (-not $profile.Success) {
    throw 'Component-level renderer percentile summary was not recorded'
}
$profileSamples = [int]$profile.Groups[1].Value
$profileValues = @()
for ($index = 2; $index -le 10; $index++) {
    $profileValues += [double]::Parse(
        $profile.Groups[$index].Value,
        [Globalization.CultureInfo]::InvariantCulture)
}
$pipelineP50 = $profileValues[0]
$pipelineP95 = $profileValues[1]
$pipelineP99 = $profileValues[2]
$submitP50 = $profileValues[3]
$submitP95 = $profileValues[4]
$submitP99 = $profileValues[5]
$topologyP50 = $profileValues[6]
$topologyP95 = $profileValues[7]
$topologyP99 = $profileValues[8]
if ($profileSamples -ne 240 -or
    $pipelineP50 -gt $pipelineP95 -or $pipelineP95 -gt $pipelineP99 -or
    $submitP50 -gt $submitP95 -or $submitP95 -gt $submitP99 -or
    $topologyP50 -le 0 -or
    $topologyP50 -gt $topologyP95 -or $topologyP95 -gt $topologyP99 -or
    # The fixed output ring absorbs short pair-production bursts. Keep a hard
    # 40 ms p99 ceiling while the stricter 59.5-60.5 Hz presentation audit
    # above proves that those bursts do not slow or duplicate host output.
    $pipelineP99 -gt 40.0) {
    throw "Renderer percentile profile failed: $($profile.Value)"
}

$shutdownMatches = [regex]::Matches(
    $stderr,
    '\[Native-World\] shutdown submitted=(\d+) rendered=(\d+) ' +
    'actual=(\d+) synthetic=(\d+) repeated=(\d+) ' +
    'syntheticAttempts=(\d+) syntheticNoOutput=(\d+) ' +
    'consumed=(\d+) dropped=(\d+) droppedPending=(\d+) ' +
    'droppedOutputPool=(\d+) droppedPublished=(\d+) ' +
    'outputWaits=(\d+) outputWaitTimeouts=(\d+) ' +
    'outputWaitAvgMs=([0-9.]+) outputWaitMaxMs=([0-9.]+)')
if ($shutdownMatches.Count -lt 1) {
    throw 'Native output-wait shutdown telemetry was not recorded'
}
$shutdown = $shutdownMatches[$shutdownMatches.Count - 1]
$shutdownRepeatedOutputs = [long]$shutdown.Groups[5].Value
$droppedTotal = [long]$shutdown.Groups[9].Value
$droppedPending = [long]$shutdown.Groups[10].Value
$droppedOutputPool = [long]$shutdown.Groups[11].Value
$droppedPublished = [long]$shutdown.Groups[12].Value
$outputWaits = [long]$shutdown.Groups[13].Value
$outputWaitTimeouts = [long]$shutdown.Groups[14].Value
$outputWaitAverageMs = [double]::Parse(
    $shutdown.Groups[15].Value,
    [Globalization.CultureInfo]::InvariantCulture)
$outputWaitMaximumMs = [double]::Parse(
    $shutdown.Groups[16].Value,
    [Globalization.CultureInfo]::InvariantCulture)
$invalidWaitTelemetry = if ($outputWaits -eq 0) {
    $outputWaitTimeouts -ne 0 -or
        $outputWaitAverageMs -ne 0 -or
        $outputWaitMaximumMs -ne 0
} else {
    $outputWaitTimeouts -gt $outputWaits -or
        $outputWaitAverageMs -le 0 -or
        $outputWaitAverageMs -gt 8.5 -or
        $outputWaitMaximumMs -le 0 -or
        $outputWaitMaximumMs -gt 20.0
}
if (
    $droppedTotal -ne
        ($droppedPending + $droppedOutputPool + $droppedPublished) -or
    $droppedPending -ne 0 -or $droppedOutputPool -ne 0 -or
    $invalidWaitTelemetry -or
    $shutdownRepeatedOutputs -ne 0 -or
    $outputWaitTimeouts -ne 0
) {
    throw "Bounded native output-wait validation failed: $($shutdown.Value)"
}

if ($CaptureEvidence) {
    foreach ($name in $captureNames) {
        $generated = Join-Path $deploy $name
        if (-not (Test-Path -LiteralPath $generated -PathType Leaf)) {
            throw "Expected transition capture is missing: $generated"
        }
        Copy-Item -LiteralPath $generated `
            -Destination (Join-Path $artifact $name) -Force
    }
}

Write-Output (
    "modern_full_path=pass perfect_world_windows=$perfectWorldWindows " +
    "paced_world_windows=$pacedWorldWindows pacing_violations=0 " +
    "world_miss=$worldMiss reset_repeats=$resetRepeats transition_holds=$transitionHolds " +
    "results_compositor_frames=$compositorFrames " +
    "results_compositor_windows=$pureCompositorWindows " +
    "lod_race_calls=$raceLodCalls lod_replay_calls=$replayLodCalls " +
    "lod_entries=$lodEntries lod_nonzero=0 lod_null=$nullLodLists " +
    "profile_samples=$profileSamples " +
    "pipeline_ms=$pipelineP50/$pipelineP95/$pipelineP99 " +
    "submit_ms=$submitP50/$submitP95/$submitP99 " +
    "topology_ms=$topologyP50/$topologyP95/$topologyP99 " +
    "output_waits=$outputWaits timeouts=$outputWaitTimeouts " +
    "output_wait_ms=$outputWaitAverageMs/$outputWaitMaximumMs " +
    "drops=$droppedPending/$droppedOutputPool/$droppedPublished " +
    "warmup_metric='$warmupMetricLine' " +
    "card_hash=$cardHashAfter artifact=$artifact")
