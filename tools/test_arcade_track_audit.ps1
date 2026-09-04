param(
    [Parameter(Mandatory = $true)]
    [ValidateSet(
        'tahiti-road',
        'midfield-raceway',
        'high-speed-ring',
        'super-speedway',
        'seattle-short-course',
        'rome-short-course',
        'red-rock-valley-speedway',
        'seattle-circuit',
        'rome-circuit',
        'grindelwald',
        'laguna-seca-raceway',
        'apricot-hill-speedway',
        'motor-sports-land',
        'trial-mountain',
        'clubman-stage-route-5',
        'grand-valley-east-section',
        'grand-valley-speedway',
        'special-stage-route-5',
        'autumn-ring',
        'test-course',
        'deep-forest-raceway',
        'rome-night',
        'autumn-ring-mini',
        'green-forest-roadway',
        'pikes-peak-downhill',
        'pikes-peak-hill-climb',
        'smokey-mountain-north',
        'smokey-mountain-south',
        'tahiti-dirt-route-3',
        'tahiti-maze',
        'apricot-hill-speedway-reverse',
        'autumn-ring-reverse',
        'autumn-ring-mini-reverse',
        'clubman-stage-route-5-reverse',
        'deep-forest-raceway-reverse',
        'grand-valley-east-section-reverse',
        'grand-valley-speedway-reverse',
        'grindelwald-reverse',
        'high-speed-ring-reverse',
        'midfield-raceway-reverse',
        'red-rock-valley-speedway-reverse',
        'rome-circuit-reverse',
        'rome-short-course-reverse',
        'rome-night-reverse',
        'seattle-circuit-reverse',
        'seattle-short-course-reverse',
        'smokey-mountain-north-reverse',
        'smokey-mountain-south-reverse',
        'special-stage-route-5-reverse',
        'tahiti-dirt-route-3-reverse',
        'tahiti-road-reverse',
        'trial-mountain-reverse',
        'special-stage-route-11',
        'special-stage-route-11-reverse')]
    [string]$Course,
    [ValidateRange(1, 30)]
    [int]$CaptureEverySeconds = 3,
    [ValidateRange(0, 1800)]
    [int]$CaptureEveryPolls = 0,
    [ValidateRange(0, 900)]
    [int]$CaptureStartSeconds = 3,
    [ValidateRange(12, 900)]
    [int]$AuditSeconds = 180,
    [ValidateRange(30, 3600)]
    [int]$TimeoutSeconds = 1200,
    [ValidatePattern('^[1-9]\d*[xX][1-9]\d*$')]
    [string]$OutputResolution = '1920x1080',
    [ValidateRange(-1, 1000000)]
    [int]$WorldDumpInputPoll = -1,
    [ValidateRange(1, 64)]
    [int]$WorldDumpCount = 1,
    [ValidateRange(1, 3600)]
    [int]$WorldDumpInterval = 1,
    [ValidatePattern('^(|0[xX][0-9A-Fa-f]+|[0-9]+)$')]
    [string]$RawTrackCorrelationModel = '',
    [ValidateRange(-1, 1000000)]
    [int]$RawTrackCorrelationStartPoll = -1,
    [ValidateRange(-1, 1000000)]
    [int]$RawTrackCorrelationEndPoll = -1,
    [ValidateRange(-1, 1000000)]
    [int]$OrderingTableTracePoll = -1,
    [string]$DeployPath = 'tools\unified-host\bin\Release\net10.0',
    [string]$DataPath = 'work\gt2-unified',
    [string]$AuditRoot = 'work\arcade-track-audit',
    [switch]$DeepDiagnostics
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$isReverse = $Course.EndsWith(
    '-reverse',
    [StringComparison]::OrdinalIgnoreCase)
$baseCourse = if ($isReverse) {
    $Course.Substring(0, $Course.Length - '-reverse'.Length)
} else {
    $Course
}
$dirtCourses = @(
    'green-forest-roadway',
    'pikes-peak-downhill',
    'pikes-peak-hill-climb',
    'smokey-mountain-north',
    'smokey-mountain-south',
    'tahiti-dirt-route-3',
    'tahiti-maze')
$surface = if ($dirtCourses -contains $baseCourse) { 'dirt' } else { 'tarmac' }

function Resolve-RepoPath([string]$Path) {
    $candidate = if ([IO.Path]::IsPathRooted($Path)) {
        $Path
    } else {
        Join-Path $repo $Path
    }
    return [IO.Path]::GetFullPath($candidate)
}

function Require([bool]$Condition, [string]$Message) {
    if (-not $Condition) {
        throw $Message
    }
}

$pollsPerSecond = 60
$captureIntervalPolls = if ($CaptureEveryPolls -gt 0) {
    $CaptureEveryPolls
} else {
    $CaptureEverySeconds * $pollsPerSecond
}
$captureIntervalSeconds = $captureIntervalPolls / [double]$pollsPerSecond
$captureStartPoll = $CaptureStartSeconds * $pollsPerSecond
$captureEndPoll = $AuditSeconds * $pollsPerSecond
Require ($captureStartPoll -le $captureEndPoll) (
    'CaptureStartSeconds cannot exceed AuditSeconds')
$expectedCaptureCount =
    [Math]::Floor(($captureEndPoll - $captureStartPoll) /
        $captureIntervalPolls) + 1

$deploy = Resolve-RepoPath $DeployPath
$data = Resolve-RepoPath $DataPath
$auditBase = Resolve-RepoPath $AuditRoot
$exe = Join-Path $deploy 'GranTurismo2PC.exe'
$runtimeAssembly = Join-Path $deploy 'RecompOne.Runtime.dll'
$nativeRenderer = Join-Path $deploy 'opengt_live_renderer.dll'
$builtNativeRenderer = Resolve-RepoPath (
    'build\native\Release\opengt_live_renderer.dll')
$sourceCard = Join-Path $deploy 'carda.sav'
foreach ($required in @(
        $exe,
        $runtimeAssembly,
        $nativeRenderer,
        $builtNativeRenderer,
        $sourceCard,
        (Join-Path $data 'GT2.VOL'),
        (Join-Path $data 'MUSIC.DAT'),
        (Join-Path $data 'manifests\arcade.json'))) {
    Require (Test-Path -LiteralPath $required -PathType Leaf) (
        "Required Arcade audit file is missing: $required")
}

# A native-only build does not refresh the framework-dependent host directory.
# Refuse to generate visual evidence from an older renderer: this exact stale
# deployment failure can otherwise make a real source fix look unchanged.
$builtRendererHash = (Get-FileHash -Algorithm SHA256 `
    -LiteralPath $builtNativeRenderer).Hash
$deployedRendererHash = (Get-FileHash -Algorithm SHA256 `
    -LiteralPath $nativeRenderer).Hash
Require ($builtRendererHash -eq $deployedRendererHash) (
    "Arcade audit renderer is stale: $nativeRenderer does not match " +
    "$builtNativeRenderer. Rebuild tools\unified-host before testing.")

# Diagnostic controls are intentionally absent from a release-package runtime.
# Fail before launch if the selected deployment cannot perform an auditable run.
$policyContext = [System.Runtime.Loader.AssemblyLoadContext]::new(
    'OpenGTArcadeTrackAuditPolicyProbe',
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
    'Arcade track audits require a development runtime with bounded ' +
    'process-local diagnostics')

$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$runRoot = Join-Path (Join-Path $auditBase $Course) $stamp
$frameRoot = Join-Path $runRoot 'frames'
New-Item -ItemType Directory -Path $frameRoot -Force | Out-Null
$stdoutPath = Join-Path $runRoot 'stdout.log'
$stderrPath = Join-Path $runRoot 'stderr.log'
$worldDumpPath = Join-Path $runRoot 'world.ogtwcap'
$runCardA = Join-Path $runRoot 'carda.sav'
$runCardB = Join-Path $runRoot 'cardb.sav'
Copy-Item -LiteralPath $sourceCard -Destination $runCardA
Copy-Item -LiteralPath $sourceCard -Destination $runCardB

$capturePattern = 'recompone_present_race_1_*.ppm'
$existingCaptures = @{}
foreach ($file in Get-ChildItem -LiteralPath $deploy `
        -Filter $capturePattern -File -ErrorAction SilentlyContinue) {
    $existingCaptures[$file.FullName] =
        "$($file.LastWriteTimeUtc.Ticks):$($file.Length)"
}

$start = [Diagnostics.ProcessStartInfo]::new()
$start.FileName = $exe
$start.WorkingDirectory = $deploy
$escapedData = $data.Replace('"', '\"')
$start.Arguments = "--headless --arcade-race $Course `"$escapedData`""
$start.UseShellExecute = $false
$start.CreateNoWindow = $true
$start.RedirectStandardOutput = $true
$start.RedirectStandardError = $true

$environment = [ordered]@{
    SDL_AUDIODRIVER = 'dummy'
    RECOMPONE_PROCESS_PRIORITY = 'BelowNormal'
    RECOMPONE_CARD_A_PATH = $runCardA
    RECOMPONE_CARD_B_PATH = $runCardB
    RECOMPONE_DISABLE_LIVE_INPUT = '1'
    RECOMPONE_GT2_AI_AUTODRIVE = '1'
    RECOMPONE_GT2_AI_AUTODRIVE_MAX_ENGAGEMENTS = '2'
    RECOMPONE_SUPPRESS_RUMBLE = '1'
    RECOMPONE_GRAPHICS_PRESET_OVERRIDE = 'Enhanced'
    RECOMPONE_OUTPUT_RESOLUTION = $OutputResolution
    RECOMPONE_PRESENTATION_RESOLUTION = $OutputResolution
    RECOMPONE_UNTHROTTLED = '1'
    RECOMPONE_CAPTURE_AUTOMATIC_STAGE = '0'
    RECOMPONE_PRESENTATION_CAPTURE = '1'
    RECOMPONE_CAPTURE_INPUT_STAGE = 'race_1'
    RECOMPONE_CAPTURE_INPUT_STAGE_POLL = $captureStartPoll.ToString()
    RECOMPONE_CAPTURE_INPUT_STAGE_INTERVAL_POLLS =
        $captureIntervalPolls.ToString()
    RECOMPONE_CAPTURE_INPUT_STAGE_END_POLL = $captureEndPoll.ToString()
    RECOMPONE_TEST_EXIT_INPUT_STAGE = 'race_1'
    RECOMPONE_TEST_EXIT_INPUT_STAGE_POLL =
        ($captureEndPoll + [Math]::Max(2, $captureIntervalPolls / 6)).ToString()
    RECOMPONE_EXIT_AFTER_INPUT_POLL =
        ($captureEndPoll + 10000).ToString()
    RECOMPONE_TRACE_PERFORMANCE = '1'
    RECOMPONE_NATIVE_WORLD_TRACE_INTERVAL = '300'
    RECOMPONE_TRACE_GT2_TRACK_MESH = $(
        if ($DeepDiagnostics) { '1' } else { $null })
    RECOMPONE_AUDIT_GT2_RAW_TRACK_VISIBILITY = $(
        if ($DeepDiagnostics) { '1' } else { $null })
    RECOMPONE_AUDIT_GT2_RESIDENT_EQUIVALENCE = $(
        if ($DeepDiagnostics) { '1' } else { $null })
    RECOMPONE_AUDIT_NATIVE_RESIDENT_EQUIVALENCE = $(
        if ($DeepDiagnostics) { '1' } else { $null })
    RECOMPONE_TRACE_GT2_RAW_TRACK_CORRELATION = $(
        if ($RawTrackCorrelationModel.Length -gt 0) { '1' } else { $null })
    RECOMPONE_TRACE_GT2_RAW_TRACK_CORRELATION_MODEL = $(
        if ($RawTrackCorrelationModel.Length -gt 0) {
            $RawTrackCorrelationModel
        } else { $null })
    RECOMPONE_TRACE_GT2_RAW_TRACK_CORRELATION_START_POLL = $(
        if ($RawTrackCorrelationStartPoll -ge 0) {
            $RawTrackCorrelationStartPoll.ToString()
        } else { $null })
    RECOMPONE_TRACE_GT2_RAW_TRACK_CORRELATION_END_POLL = $(
        if ($RawTrackCorrelationEndPoll -ge 0) {
            $RawTrackCorrelationEndPoll.ToString()
        } else { $null })
    RECOMPONE_TRACE_GT2_OT_WALK_POLL = $(
        if ($OrderingTableTracePoll -ge 0) {
            $OrderingTableTracePoll.ToString()
        } else { $null })
    OPENGT_RENDER_RESIDENT_LOD_DIAGNOSTICS = $(
        if ($DeepDiagnostics) { '1' } else { $null })
    RECOMPONE_NATIVE_WORLD_DUMP_PATH = $(
        if ($WorldDumpInputPoll -ge 0) { $worldDumpPath } else { $null })
    RECOMPONE_NATIVE_WORLD_DUMP_INPUT_POLL = $(
        if ($WorldDumpInputPoll -ge 0) {
            $WorldDumpInputPoll.ToString()
        } else { $null })
    RECOMPONE_NATIVE_WORLD_DUMP_COUNT = $(
        if ($WorldDumpInputPoll -ge 0) {
            $WorldDumpCount.ToString()
        } else { $null })
    RECOMPONE_NATIVE_WORLD_DUMP_INTERVAL = $(
        if ($WorldDumpInputPoll -ge 0) {
            $WorldDumpInterval.ToString()
        } else { $null })
}
foreach ($entry in $environment.GetEnumerator()) {
    if ($null -eq $entry.Value) {
        $start.Environment.Remove($entry.Key) | Out-Null
    } else {
        $start.Environment[$entry.Key] = [string]$entry.Value
    }
}

$process = [Diagnostics.Process]::Start($start)
try {
    $process.PriorityClass = [Diagnostics.ProcessPriorityClass]::BelowNormal
} catch {
    Write-Warning "Could not lower audit process priority: $($_.Exception.Message)"
}
$stdoutTask = $process.StandardOutput.ReadToEndAsync()
$stderrTask = $process.StandardError.ReadToEndAsync()
if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
    $process.Kill($true)
    $process.WaitForExit()
    throw "Arcade $Course audit exceeded ${TimeoutSeconds}s"
}
$stdout = $stdoutTask.Result
$stderr = $stderrTask.Result
[IO.File]::WriteAllText($stdoutPath, $stdout)
[IO.File]::WriteAllText($stderrPath, $stderr)
Require ($process.ExitCode -eq 0) (
    "Arcade $Course audit exited with code $($process.ExitCode)")

$combined = $stdout + [Environment]::NewLine + $stderr
foreach ($requiredMarker in @(
        "[Host] direct Arcade race requested: $Course",
        '[GT2-Direct] native Arcade frontend setup complete',
        "[GT2-Direct] native ",
        '[Dispatcher] loaded overlay: gt2_arcade_overlay_0',
        '[Input] periodic stage capture armed: stage=race_1',
        '[Runtime] shutdown complete; exit=0')) {
    Require ($combined.Contains($requiredMarker)) (
        "Arcade $Course audit is missing runtime proof: $requiredMarker")
}
Require ($combined -notmatch
    '(?im)\b(fatal|unhandled exception|construction differs|unmapped guest)\b') (
    "Arcade $Course audit contains a fatal runtime marker")
Require ($combined -notmatch
    '(?im)native presentation capture=.*(?:synthetic=True|repeated=True)') (
    "Arcade $Course audit captured a synthetic or repeated world frame")

# Stage capture labels use a race-relative input clock, while renderer
# diagnostics use the process-wide input-poll clock. Retain the host's exact
# presentation mapping for every screenshot so a later defect trace cannot
# accidentally diagnose the same numeric poll in the wrong clock domain.
$captureMappings = @{}
$captureMappingPattern =
    '\[Host\] native presentation capture=race_1_(?<stagePoll>\d{6}) ' +
    'sourceFrame=(?<sourceFrame>\d+) poll=(?<sourcePoll>\d+) ' +
    'synthetic=(?<synthetic>True|False) repeated=(?<repeated>True|False)'
foreach ($match in [regex]::Matches($combined, $captureMappingPattern)) {
    $stagePoll = [int]$match.Groups['stagePoll'].Value
    Require (-not $captureMappings.ContainsKey($stagePoll)) (
        "Arcade $Course audit logged duplicate presentation mappings " +
        "for race-stage poll $stagePoll")
    Require (
        $match.Groups['synthetic'].Value -eq 'False' -and
        $match.Groups['repeated'].Value -eq 'False') (
        "Arcade $Course audit mapped race-stage poll $stagePoll to a " +
        'synthetic or repeated presentation')
    $captureMappings[$stagePoll] = [ordered]@{
        sourceFrame = [int]$match.Groups['sourceFrame'].Value
        sourceInputPoll = [int]$match.Groups['sourcePoll'].Value
    }
}

$newCaptures = @(
    Get-ChildItem -LiteralPath $deploy -Filter $capturePattern -File |
    Where-Object {
        $signature = "$($_.LastWriteTimeUtc.Ticks):$($_.Length)"
        -not $existingCaptures.ContainsKey($_.FullName) -or
            $existingCaptures[$_.FullName] -ne $signature
    } |
    Sort-Object Name)
Require ($newCaptures.Count -eq $expectedCaptureCount) (
    "Arcade $Course audit expected $expectedCaptureCount three-second " +
    "captures, found $($newCaptures.Count)")

$ffmpeg = (Get-Command ffmpeg -ErrorAction SilentlyContinue).Source
Require (-not [string]::IsNullOrWhiteSpace($ffmpeg)) (
    'ffmpeg is required to normalize audit screenshots to PNG')
$frameRecords = @()
foreach ($capture in $newCaptures) {
    Require ($capture.Name -match
        '^recompone_present_race_1_(\d{6})_') (
        "Unexpected Arcade audit capture name: $($capture.Name)")
    $poll = [int]$Matches[1]
    Require ($captureMappings.ContainsKey($poll)) (
        "Arcade $Course audit is missing the presentation mapping for " +
        "race-stage poll $poll")
    $mapping = $captureMappings[$poll]
    $seconds = $poll / [double]$pollsPerSecond
    $pngName = "frame-$($poll.ToString('000000'))-$($seconds.ToString('000.0')).png"
    $pngPath = Join-Path $frameRoot $pngName
    & $ffmpeg -hide_banner -loglevel error -y `
        -i $capture.FullName $pngPath
    if ($LASTEXITCODE -ne 0) {
        throw "ffmpeg failed to convert Arcade audit capture $($capture.Name)"
    }
    $frameRecords += [ordered]@{
        ordinal = $frameRecords.Count + 1
        stagePoll = $poll
        sourceInputPoll = $mapping.sourceInputPoll
        sourceFrame = $mapping.sourceFrame
        raceSeconds = $seconds
        file = $pngName
        sha256 = (Get-FileHash -LiteralPath $pngPath -Algorithm SHA256).Hash
        reviewed = $false
        finding = $null
    }
    Remove-Item -LiteralPath $capture.FullName -Force
}

$manifest = [ordered]@{
    formatVersion = 2
    course = $Course
    baseCourse = $baseCourse
    layoutDirection = $(if ($isReverse) { 'reverse' } else { 'forward' })
    surface = $surface
    status = 'capture-complete-review-pending'
    capturedAt = [DateTime]::UtcNow.ToString('o')
    stageClock = 'race_1-relative-input-polls'
    sourceClock = 'process-wide-input-polls'
    captureEverySeconds = $captureIntervalSeconds
    captureEveryPolls = $captureIntervalPolls
    captureStartSeconds = $CaptureStartSeconds
    auditSeconds = $AuditSeconds
    outputResolution = $OutputResolution.ToLowerInvariant()
    graphicsPreset = 'Enhanced'
    worldDumpInputPoll = $(
        if ($WorldDumpInputPoll -ge 0) { $WorldDumpInputPoll }
        else { $null })
    worldDumpCount = $(
        if ($WorldDumpInputPoll -ge 0) { $WorldDumpCount }
        else { 0 })
    nativeClass = 'C'
    nativePlayer = 'Citroen Xsara 1.8i 16V'
    aiAutoDrive = $true
    processLocalInputOnly = $true
    deepDiagnostics = $DeepDiagnostics.IsPresent
    frameCount = $frameRecords.Count
    frames = $frameRecords
}
$manifestPath = Join-Path $runRoot 'manifest.json'
[IO.File]::WriteAllText(
    $manifestPath,
    ($manifest | ConvertTo-Json -Depth 6))

Write-Output (
    "arcade_track_audit_capture=pass course=$Course " +
    "frames=$($frameRecords.Count) interval=${captureIntervalSeconds}s " +
    "duration=${AuditSeconds}s diagnostics=$($DeepDiagnostics.IsPresent)")
Write-Output "run=$runRoot"
Write-Output "manifest=$manifestPath"
