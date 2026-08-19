param(
    [int]$CapturePoll = 10000,
    [string]$ArtifactName = 'projection-isolated-current',
    [string]$PairedArtifactName = 'graphics-presets-paired-current'
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$deploy = Join-Path $repo 'OpenGTPS1'
$exe = Join-Path $deploy 'GranTurismo2PC.exe'
$fixture = Join-Path $repo 'tests\fixtures\ai-autodrive-save-sunday-race.input'
$pairedArtifact = Join-Path $repo "artifacts\$PairedArtifactName"
$artifact = Join-Path $repo "artifacts\$ArtifactName"
$capture = Join-Path $deploy "recompone_capture__$CapturePoll.ppm"

New-Item -ItemType Directory -Path $artifact -Force | Out-Null
Copy-Item -LiteralPath (
    Join-Path $pairedArtifact "Enhanced-$CapturePoll.ppm"
) -Destination (Join-Path $artifact "projection-on-$CapturePoll.ppm") -Force
if (Test-Path -LiteralPath (
        Join-Path $pairedArtifact "Enhanced-$CapturePoll.png"
    )) {
    Copy-Item -LiteralPath (
        Join-Path $pairedArtifact "Enhanced-$CapturePoll.png"
    ) -Destination (Join-Path $artifact "projection-on-$CapturePoll.png") -Force
}
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
# Negative regression: this retired downgrade variable must be ignored. The
# resulting capture must remain byte-identical to the fixed modern baseline.
$start.EnvironmentVariables['RECOMPONE_PERSPECTIVE_CORRECT_TEXTURES'] = '0'
$start.EnvironmentVariables['RECOMPONE_EXIT_AFTER_INPUT_POLL'] =
    ($CapturePoll + 50).ToString()

$process = [Diagnostics.Process]::Start($start)
$stdout = $process.StandardOutput.ReadToEndAsync()
$stderr = $process.StandardError.ReadToEndAsync()
if (-not $process.WaitForExit(600000)) {
    $process.Kill($true)
    $process.WaitForExit()
    throw 'Projection control capture timed out'
}

$stdoutText = $stdout.Result
$stderrText = $stderr.Result
[IO.File]::WriteAllText(
    (Join-Path $artifact 'projection-off.stdout.log'), $stdoutText)
[IO.File]::WriteAllText(
    (Join-Path $artifact 'projection-off.stderr.log'), $stderrText)
if ($process.ExitCode -ne 0) {
    throw "Projection control capture exited with code $($process.ExitCode)"
}
if ($stderrText -notmatch 'headless audio backend=dummy' -or
    $stderrText -notmatch 'SDL audio ready: driver=dummy') {
    throw 'Projection control capture did not prove the dummy audio backend'
}
if (-not (Test-Path -LiteralPath $capture)) {
    throw "Projection control did not produce $capture"
}

$offPpm = Join-Path $artifact "projection-off-$CapturePoll.ppm"
Copy-Item -LiteralPath $capture -Destination $offPpm -Force
$onPpm = Join-Path $artifact "projection-on-$CapturePoll.ppm"
if ((Get-FileHash -LiteralPath $onPpm -Algorithm SHA256).Hash -ne
        (Get-FileHash -LiteralPath $offPpm -Algorithm SHA256).Hash) {
    throw 'Retired projection downgrade variable changed modern output'
}
$ffmpeg = (Get-Command ffmpeg -ErrorAction SilentlyContinue).Source
if ($ffmpeg) {
    & $ffmpeg -hide_banner -loglevel error -y -i $offPpm `
        (Join-Path $artifact "projection-off-$CapturePoll.png")
    if ($LASTEXITCODE -ne 0) {
        throw 'ffmpeg failed to convert the projection control capture'
    }
}

Write-Output "retired projection override ignored=$artifact"
Write-Output 'modern baseline=byte-identical'
Write-Output 'audio safety=dummy backend proven'
