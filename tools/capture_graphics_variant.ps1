param(
    [ValidateSet('Modern')]
    [string]$Variant = 'Modern',
    [int]$CapturePoll = 10000,
    [string]$ArtifactName = 'graphics-variant-current',
    [string]$Fixture = 'tests\fixtures\ai-autodrive-save-sunday-race.input',
    [switch]$TraceProjection,
    [switch]$TracePacketWrites,
    [switch]$TraceTrackRendering
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$deploy = Join-Path $repo 'OpenGTPS1'
$exe = Join-Path $deploy 'GranTurismo2PC.exe'
$fixture = if ([IO.Path]::IsPathRooted($Fixture)) {
    $Fixture
} else {
    Join-Path $repo $Fixture
}
$artifact = Join-Path $repo "artifacts\$ArtifactName"
$capture = Join-Path $deploy "recompone_capture__$CapturePoll.ppm"
New-Item -ItemType Directory -Path $artifact -Force | Out-Null
if (Test-Path -LiteralPath $capture) {
    Remove-Item -LiteralPath $capture -Force
}

$start = [Diagnostics.ProcessStartInfo]::new()
$start.FileName = $exe
$start.WorkingDirectory = $deploy
$start.Arguments = '--headless'
$start.UseShellExecute = $false
$start.CreateNoWindow = $true
$start.RedirectStandardOutput = $true
$start.RedirectStandardError = $true
$start.EnvironmentVariables['RECOMPONE_INPUT_FILE'] = $fixture
$start.EnvironmentVariables['RECOMPONE_DISABLE_LIVE_INPUT'] = '1'
$start.EnvironmentVariables['RECOMPONE_SUPPRESS_RUMBLE'] = '1'
$start.EnvironmentVariables['RECOMPONE_UNTHROTTLED'] = '1'
$start.EnvironmentVariables['RECOMPONE_GT2_AI_AUTODRIVE'] = '1'
$start.EnvironmentVariables['RECOMPONE_TRACE_GPU_PRIMITIVES'] = '1'
$start.EnvironmentVariables['RECOMPONE_GRAPHICS_PRESET_OVERRIDE'] = 'Enhanced'
$tracePath = Join-Path $artifact 'projection-trace.jsonl'
if ($TraceProjection) {
    $start.EnvironmentVariables['RECOMPONE_PROJECTION_TRACE_PATH'] = $tracePath
    $start.EnvironmentVariables['RECOMPONE_PROJECTION_TRACE_START_POLL'] =
        ($CapturePoll - 1).ToString()
    $start.EnvironmentVariables['RECOMPONE_PROJECTION_TRACE_END_POLL'] =
        ($CapturePoll + 1).ToString()
    $start.EnvironmentVariables['RECOMPONE_PROJECTION_TRACE_SUMMARY_INTERVAL'] = '120'
}
if ($TracePacketWrites) {
    $start.EnvironmentVariables['RECOMPONE_TRACE_GTE_PACKET_WRITES'] = '1'
    $start.EnvironmentVariables['RECOMPONE_TRACE_GTE_PACKET_WRITES_START_POLL'] =
        ($CapturePoll - 2).ToString()
}
if ($TraceTrackRendering) {
    $start.EnvironmentVariables['RECOMPONE_TRACE_GT2_TRACK_RENDERING'] = '1'
}
$start.EnvironmentVariables['RECOMPONE_EXIT_AFTER_INPUT_POLL'] =
    ($CapturePoll + 50).ToString()

$process = [Diagnostics.Process]::Start($start)
$stdout = $process.StandardOutput.ReadToEndAsync()
$stderr = $process.StandardError.ReadToEndAsync()
if (-not $process.WaitForExit(600000)) {
    $process.Kill($true)
    $process.WaitForExit()
    throw "$Variant capture timed out"
}

$stdoutText = $stdout.Result
$stderrText = $stderr.Result
[IO.File]::WriteAllText((Join-Path $artifact "$Variant.stdout.log"), $stdoutText)
[IO.File]::WriteAllText((Join-Path $artifact "$Variant.stderr.log"), $stderrText)
if ($process.ExitCode -ne 0) {
    throw "$Variant capture exited with code $($process.ExitCode)"
}
if ($stderrText -notmatch 'headless audio backend=dummy' -or
    $stderrText -notmatch 'SDL audio ready: driver=dummy') {
    throw "$Variant capture did not prove the dummy audio backend"
}
if (-not (Test-Path -LiteralPath $capture)) {
    throw "$Variant did not produce $capture"
}

$ppm = Join-Path $artifact "$Variant-$CapturePoll.ppm"
Copy-Item -LiteralPath $capture -Destination $ppm -Force
$ffmpeg = (Get-Command ffmpeg -ErrorAction SilentlyContinue).Source
if ($ffmpeg) {
    & $ffmpeg -hide_banner -loglevel error -y -i $ppm `
        (Join-Path $artifact "$Variant-$CapturePoll.png")
    if ($LASTEXITCODE -ne 0) {
        throw "ffmpeg failed to convert the $Variant capture"
    }
}

Write-Output "graphics variant=$Variant artifact=$artifact"
Write-Output 'renderer=fixed modern; legacy quality variants removed'
Write-Output 'audio safety=dummy backend proven'
