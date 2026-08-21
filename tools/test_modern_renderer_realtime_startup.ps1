param(
    [string]$ArtifactName = 'modern-renderer-realtime-startup-current',
    [int]$ExitPoll = 2400,
    [int]$TimeoutSeconds = 120,
    [string]$DeployPath = 'tools\unified-host\bin\Release\net10.0\win-x64\publish',
    [string]$DataPath = 'work\gt2-unified',
    [string]$Fixture = 'tests\fixtures\modern-renderer-replay-soak.input'
)

$ErrorActionPreference = 'Stop'
try { (Get-Process -Id $PID).PriorityClass = 'BelowNormal' } catch {}
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
$artifact = Join-Path $repo "artifacts\$ArtifactName"
New-Item -ItemType Directory -Path $artifact -Force | Out-Null
$stdoutPath = Join-Path $artifact 'stdout.log'
$stderrPath = Join-Path $artifact 'stderr.log'
$capturePath = Join-Path $artifact 'first-world.ogtwcap'
$cardBackup = Join-Path $artifact 'carda.before.sav'
Copy-Item -LiteralPath $card -Destination $cardBackup -Force
$cardHashBefore = (Get-FileHash -LiteralPath $card -Algorithm SHA256).Hash

$start = [Diagnostics.ProcessStartInfo]::new()
$start.FileName = $exe
$start.WorkingDirectory = $deploy
$start.Arguments = '--headless "' + $data.Replace('"', '\"') + '"'
$start.UseShellExecute = $false
$start.CreateNoWindow = $true
$start.RedirectStandardOutput = $true
$start.RedirectStandardError = $true
$environment = [ordered]@{
    SDL_AUDIODRIVER = 'dummy'
    RECOMPONE_INPUT_FILE = $fixturePath
    RECOMPONE_DISABLE_LIVE_INPUT = '1'
    RECOMPONE_SUPPRESS_RUMBLE = '1'
    RECOMPONE_GT2_CREATE_TEST_SAVE = '1'
    RECOMPONE_GRAPHICS_PRESET_OVERRIDE = 'Enhanced'
    RECOMPONE_EXIT_AFTER_INPUT_POLL = $ExitPoll.ToString()
    RECOMPONE_UNTHROTTLED = '0'
    RECOMPONE_TRACE_PERFORMANCE = '1'
    RECOMPONE_NATIVE_WORLD_TRACE_INTERVAL = '60'
    RECOMPONE_NATIVE_WORLD_DUMP_PATH = $capturePath
    RECOMPONE_PROCESS_PRIORITY = 'BelowNormal'
}
foreach ($entry in $environment.GetEnumerator()) {
    $start.Environment[$entry.Key] = $entry.Value
}

$process = $null
$stdout = ''
$stderr = ''
$timedOut = $false
try {
    $process = [Diagnostics.Process]::Start($start)
    try { $process.PriorityClass = 'BelowNormal' } catch {}
    $stdoutTask = $process.StandardOutput.ReadToEndAsync()
    $stderrTask = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
        $timedOut = $true
        $process.Kill($true)
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
    throw "Real-time startup test exceeded ${TimeoutSeconds}s"
}
if ($process.ExitCode -ne 0 -or
    $stderr -notmatch '\[Runtime\] shutdown complete; exit=0') {
    throw 'Real-time startup test did not exit cleanly'
}
if ($stderr -match
    'Unhandled exception|Fatal error|unknown software exception|' +
    '\[Native-World\] disabled:|submission rejected|dropped truncated|' +
    'compositor fallback viewport=|unmapped call') {
    throw 'Renderer/runtime failure marker was logged'
}
if (-not (Test-Path -LiteralPath $capturePath -PathType Leaf)) {
    throw 'The first real-time authored world capture was not produced'
}

$metricLines = [regex]::Matches(
    $stderr,
    '(?m)^\[Native-Present\].*$') | ForEach-Object { $_.Value }
$worldMiss = 0
$worldFrames = 0
$compositorFrames = 0
foreach ($line in $metricLines) {
    if ($line -match 'worldMiss=(\d+)') {
        $worldMiss += [int]$Matches[1]
    }
    if ($line -match 'new=(\d+)') {
        $worldFrames += [int]$Matches[1]
    }
    if ($line -match 'compositor=(\d+)') {
        $compositorFrames += [int]$Matches[1]
    }
}
if ($worldMiss -ne 0) {
    throw "Cold-start modern world missed $worldMiss presentation(s)"
}
if ($worldFrames -lt 60 -or $compositorFrames -lt 300) {
    throw (
        "Insufficient cold-start coverage: world=$worldFrames " +
        "compositor=$compositorFrames")
}

Write-Output (
    "modern_realtime_startup=pass world_frames=$worldFrames " +
    "compositor_frames=$compositorFrames world_miss=$worldMiss " +
    "card_hash=$cardHashAfter artifact=$artifact")
