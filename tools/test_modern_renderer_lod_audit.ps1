param(
    [string]$ArtifactName = 'modern-renderer-lod-audit-current',
    [int]$ExitPoll = 8500,
    [int]$TimeoutSeconds = 120,
    [string]$DeployPath = 'tools\unified-host\bin\Release\net10.0',
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
    RECOMPONE_GT2_AI_AUTODRIVE = '1'
    RECOMPONE_GT2_AI_AUTODRIVE_MAX_ENGAGEMENTS = '2'
    RECOMPONE_GT2_CREATE_TEST_SAVE = '1'
    RECOMPONE_GRAPHICS_PRESET_OVERRIDE = 'Enhanced'
    RECOMPONE_EXIT_AFTER_INPUT_POLL = $ExitPoll.ToString()
    RECOMPONE_UNTHROTTLED = '1'
    RECOMPONE_AUDIT_RENDERER = '1'
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
if ($cardHashAfter -ne $cardHashBefore) { throw 'Card restore mismatch' }
if ($timedOut -or $process.ExitCode -ne 0 -or
    $stderr -notmatch '\[Runtime\] shutdown complete; exit=0') {
    throw 'LOD audit did not exit cleanly'
}
if ($stderr -match
    'Unhandled exception|Fatal error|unknown software exception|' +
    '\[Native-World\] disabled:|unmapped call') {
    throw 'LOD audit logged a runtime/renderer failure'
}

$track = [regex]::Match(
    $stderr,
    '\[GT2-Renderer-Audit\] track raceCalls=(\d+) replayCalls=(\d+) ' +
    'maximumCalls=(\d+) stockCalls=(\d+) entriesScanned=(\d+) ' +
    'nonzeroSelectors=(\d+) nullLists=(\d+) invalidLists=(\d+)')
if (-not $track.Success) { throw 'Track LOD audit summary is missing' }
$raceCalls = [long]$track.Groups[1].Value
$replayCalls = [long]$track.Groups[2].Value
$maximumCalls = [long]$track.Groups[3].Value
$stockCalls = [long]$track.Groups[4].Value
$entries = [long]$track.Groups[5].Value
$nonzero = [long]$track.Groups[6].Value
$nullLists = [long]$track.Groups[7].Value
$invalidLists = [long]$track.Groups[8].Value
if ($raceCalls -lt 1 -or $replayCalls -ne 0 -or
    $maximumCalls -ne $raceCalls -or $stockCalls -ne 0 -or
    $entries -lt 1 -or $nonzero -ne 0 -or $invalidLists -ne 0) {
    throw "Maximum race track LOD was not proven: $($track.Value)"
}
$vehicle = [regex]::Match(
    $stderr,
    '\[GT2-Renderer-Audit\] vehicles requests=(\d+) ' +
    'selectors=\[1:(\d+)\]')
if (-not $vehicle.Success -or
    [long]$vehicle.Groups[1].Value -ne [long]$vehicle.Groups[2].Value) {
    throw 'Maximum vehicle LOD was not proven'
}

Write-Output (
    "modern_lod_audit=pass race_calls=$raceCalls entries=$entries " +
    "nonzero=0 invalid=0 null=$nullLists " +
    "vehicle_requests=$($vehicle.Groups[1].Value) " +
    "card_hash=$cardHashAfter artifact=$artifact")
