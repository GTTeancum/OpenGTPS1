param(
    [string]$DeployPath = 'OpenGTPS1',
    [string]$Tag = 'before',
    [int]$EndPoll = 7210,
    [int]$FirstCapturePoll = 600,
    [int]$CaptureInterval = 600,
    [int]$DumpPoll = 938,
    [int]$DumpCount = 1,
    [switch]$FullGeometry
)
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$deploy = (Resolve-Path (Join-Path $repo $DeployPath)).Path
$evidence = Join-Path $repo "artifacts/ssr11-rendering/$Tag"
New-Item -ItemType Directory -Path $evidence -Force | Out-Null
$start = [Diagnostics.ProcessStartInfo]::new()
$start.FileName = Join-Path $deploy 'GranTurismo2PC.exe'
$start.WorkingDirectory = $deploy
$start.Arguments = '--headless --mute --arcade-race special-stage-route-11 "' + (Join-Path $repo 'OpenGTPS1') + '"'
$start.UseShellExecute = $false
$start.CreateNoWindow = $true
$start.RedirectStandardOutput = $true
$start.RedirectStandardError = $true
$controls = @{
    SDL_AUDIODRIVER = 'dummy'
    RECOMPONE_DISABLE_LIVE_INPUT = '1'
    RECOMPONE_SUPPRESS_RUMBLE = '1'
    RECOMPONE_GT2_AI_AUTODRIVE = '1'
    RECOMPONE_GT2_AI_AUTODRIVE_MAX_ENGAGEMENTS = '2'
    RECOMPONE_UNTHROTTLED = '1'
    RECOMPONE_CAPTURE_AUTOMATIC_STAGE = '0'
    RECOMPONE_PRESENTATION_CAPTURE = '1'
    RECOMPONE_CAPTURE_INPUT_STAGE = 'race_1'
    RECOMPONE_CAPTURE_INPUT_STAGE_POLL = $FirstCapturePoll.ToString()
    RECOMPONE_CAPTURE_INPUT_STAGE_INTERVAL_POLLS = $CaptureInterval.ToString()
    RECOMPONE_CAPTURE_INPUT_STAGE_END_POLL = ($EndPoll - 10).ToString()
    RECOMPONE_TEST_EXIT_INPUT_STAGE = 'race_1'
    RECOMPONE_TEST_EXIT_INPUT_STAGE_POLL = $EndPoll.ToString()
    RECOMPONE_EXIT_AFTER_INPUT_POLL = ($EndPoll + 10000).ToString()
    RECOMPONE_OUTPUT_RESOLUTION = '1280x960'
    RECOMPONE_PRESENTATION_RESOLUTION = '1280x960'
    RECOMPONE_PROCESS_PRIORITY = 'BelowNormal'
    RECOMPONE_CARD_A_PATH = (Join-Path $evidence 'carda.sav')
    RECOMPONE_CARD_B_PATH = (Join-Path $evidence 'cardb.sav')
    RECOMPONE_NATIVE_WORLD_DUMP_PATH = (Join-Path $evidence 'scene.ogtwcap')
    RECOMPONE_NATIVE_WORLD_DUMP_INPUT_POLL = $DumpPoll.ToString()
    RECOMPONE_NATIVE_WORLD_DUMP_COUNT = $DumpCount.ToString()
    RECOMPONE_GPU_CAPTURE_PATH = (Join-Path $evidence 'guest.ogtcap')
    RECOMPONE_GPU_CAPTURE_INPUT_POLL = $DumpPoll.ToString()
    RECOMPONE_AUDIT_GT2_RESIDENT_EQUIVALENCE = $(if ($FullGeometry) {'1'} else {'0'})
    RECOMPONE_AUDIT_NATIVE_RESIDENT_EQUIVALENCE = $(if ($FullGeometry) {'1'} else {'0'})
}
foreach ($card in @('carda.sav', 'cardb.sav')) {
    Copy-Item -LiteralPath (Join-Path $repo "OpenGTPS1/$card") -Destination $evidence
}
foreach ($entry in $controls.GetEnumerator()) { $start.Environment[$entry.Key] = $entry.Value }
$process = [Diagnostics.Process]::Start($start)
$stdout = $process.StandardOutput.ReadToEndAsync()
$stderr = $process.StandardError.ReadToEndAsync()
$finished = $process.WaitForExit(300000)
if (-not $finished) { $process.Kill(); $process.WaitForExit() }
[IO.File]::WriteAllText((Join-Path $evidence 'stdout.log'), $stdout.Result)
[IO.File]::WriteAllText((Join-Path $evidence 'stderr.log'), $stderr.Result)
if (-not $finished) { throw 'SSR11 diagnostic timed out' }
if ($process.ExitCode -ne 0) { throw "SSR11 exited $($process.ExitCode)" }
if ($stdout.Result -notmatch 'native Special Stage Route 11 construction verified') {
    throw 'SSR11 native course construction was not verified'
}
if (($stdout.Result + $stderr.Result) -match 'Unhandled exception|Fatal error|unmapped call|submission rejected') {
    throw 'SSR11 diagnostic reported a runtime/rendering failure'
}
if ($FullGeometry -and $stderr.Result -match '(?:unmatched|geometryOrderMismatch|materialOrderMismatch)=[1-9]') {
    throw 'SSR11 native geometry/material equivalence audit failed'
}
for ($poll = $FirstCapturePoll; $poll -le ($EndPoll - 10); $poll += $CaptureInterval) {
    $source = Join-Path $deploy ('recompone_present_race_1_' + $poll.ToString('000000') + '_1280x960_fxaa.ppm')
    if ((Get-Item -LiteralPath $source).LastWriteTime -lt $process.StartTime) { throw "Stale capture: $source" }
    & ffmpeg -hide_banner -loglevel error -y -i $source (Join-Path $evidence "race-$poll.png")
    if ($LASTEXITCODE -ne 0) { throw 'Capture conversion failed' }
    Remove-Item -LiteralPath $source
}
Write-Output "SSR11 capture complete: $Tag exit=0 evidence=$evidence"
