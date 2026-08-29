param(
    [ValidateSet('Race', 'Replay')]
    [string]$LaunchMode = 'Replay',
    [int]$ObjectId = 2,
    [int]$Poll = 1419,
    [int]$ExitPoll = 1421,
    [int]$TimeoutSeconds = 180,
    [string]$ArtifactStem = 'seattle-vehicle-provenance',
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

$deploy = Resolve-RepoPath $DeployPath
$data = Resolve-RepoPath $DataPath
$exe = Join-Path $deploy 'GranTurismo2PC.exe'
$card = Join-Path $deploy 'carda.sav'
foreach ($required in @($exe, $card)) {
    if (-not (Test-Path -LiteralPath $required -PathType Leaf)) {
        throw "Required file is missing: $required"
    }
}

$stdoutPath = Join-Path $repo "work\$ArtifactStem.stdout.log"
$stderrPath = Join-Path $repo "work\$ArtifactStem.stderr.log"
$wheelPath = Join-Path $repo "work\$ArtifactStem.wheels.csv"
$cardBackup = Join-Path $repo "work\$ArtifactStem.carda.before.sav"
Copy-Item -LiteralPath $card -Destination $cardBackup -Force
$cardHashBefore = (Get-FileHash -LiteralPath $card -Algorithm SHA256).Hash

$start = [Diagnostics.ProcessStartInfo]::new()
$start.FileName = $exe
$start.WorkingDirectory = $deploy
$launchArgument = if ($LaunchMode -eq 'Replay') {
    '--arcade-replay'
} else {
    '--arcade-race'
}
$start.Arguments =
    "--headless $launchArgument seattle-circuit `"" +
    $data.Replace('"', '\"') + '"'
$start.UseShellExecute = $false
$start.CreateNoWindow = $true
$start.RedirectStandardOutput = $true
$start.RedirectStandardError = $true
$environment = [ordered]@{
    SDL_AUDIODRIVER = 'dummy'
    RECOMPONE_PROCESS_PRIORITY = 'AboveNormal'
    RECOMPONE_DISABLE_LIVE_INPUT = '1'
    RECOMPONE_SUPPRESS_RUMBLE = '1'
    RECOMPONE_GT2_AI_AUTODRIVE = '1'
    RECOMPONE_GT2_AI_AUTODRIVE_MAX_ENGAGEMENTS = '2'
    RECOMPONE_GT2_SOAK_QUICK_WIN_AFTER_AI_TICKS = '600'
    RECOMPONE_GT2_CREATE_TEST_SAVE = '1'
    RECOMPONE_GRAPHICS_PRESET_OVERRIDE = 'Enhanced'
    RECOMPONE_EXIT_AFTER_INPUT_POLL = $ExitPoll.ToString()
    RECOMPONE_UNTHROTTLED = '1'
    RECOMPONE_THROTTLE_ON_SCRIPT_STAGE = 'race_1'
    RECOMPONE_DISABLE_DISPLAY_CAPTURE = '1'
    RECOMPONE_AUDIT_RENDERER = '1'
    RECOMPONE_TRACE_GT2_WHEEL_TRANSFORMS = '1'
    RECOMPONE_GT2_WHEEL_TRANSFORM_TRACE_PATH = $wheelPath
    RECOMPONE_TRACE_GT2_VEHICLE_VISIBILITY = '1'
    OPENGT_RENDER_VEHICLE_DIAGNOSTICS = '1'
    OPENGT_RENDER_VEHICLE_DIAGNOSTICS_START_POLL = $Poll.ToString()
    OPENGT_RENDER_VEHICLE_DIAGNOSTICS_END_POLL = $Poll.ToString()
    OPENGT_RENDER_VEHICLE_DIAGNOSTICS_INTERVAL = '1'
    OPENGT_RENDER_VEHICLE_DIAGNOSTICS_OBJECT = $ObjectId.ToString()
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
if ($cardHashAfter -ne $cardHashBefore) {
    throw 'Memory-card restore hash mismatch'
}
if ($timedOut) {
    throw "Seattle vehicle trace exceeded ${TimeoutSeconds}s"
}
if ($process.ExitCode -ne 0) {
    throw "Seattle vehicle trace exited with code $($process.ExitCode)"
}
if ($stderr -notmatch "\[Render-Vehicle-Group\].*object=$ObjectId ") {
    throw "Vehicle object $ObjectId was not traced at input poll $Poll"
}
if ($stderr -notmatch '\[Runtime\] shutdown complete; exit=0') {
    throw 'Orderly shutdown was not proven'
}

Write-Host (
    "Seattle vehicle trace passed: mode=$LaunchMode " +
    "object=$ObjectId poll=$Poll")
Write-Host "  stderr: $stderrPath"
Write-Host "  wheels: $wheelPath"
