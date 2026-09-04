param(
    [string]$DeployPath = 'OpenGTPS1',
    [string]$Tag = 'before-full',
    [int]$EndPoll = 610,
    [int]$DumpPoll = 938,
    [switch]$FullGeometry
)
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$deploy = (Resolve-Path (Join-Path $repo $DeployPath)).Path
$evidence = Join-Path $repo 'artifacts/ssr5-depth-fix'
New-Item -ItemType Directory -Path $evidence -Force | Out-Null
$start = [Diagnostics.ProcessStartInfo]::new()
$start.FileName = Join-Path $deploy 'GranTurismo2PC.exe'
$start.WorkingDirectory = $deploy
$start.Arguments = '--headless --arcade-race special-stage-route-5 "' + (Join-Path $repo 'OpenGTPS1') + '"'
$start.UseShellExecute = $false
$start.CreateNoWindow = $true
$start.RedirectStandardOutput = $true
$start.RedirectStandardError = $true
$controls = @{
    RECOMPONE_DISABLE_LIVE_INPUT = '1'
    RECOMPONE_SUPPRESS_RUMBLE = '1'
    RECOMPONE_GT2_AI_AUTODRIVE = '1'
    RECOMPONE_GT2_AI_AUTODRIVE_MAX_ENGAGEMENTS = '2'
    RECOMPONE_UNTHROTTLED = '1'
    RECOMPONE_CAPTURE_AUTOMATIC_STAGE = '0'
    RECOMPONE_PRESENTATION_CAPTURE = '1'
    RECOMPONE_CAPTURE_INPUT_STAGE = 'race_1'
    RECOMPONE_CAPTURE_INPUT_STAGE_POLL = '600'
    RECOMPONE_CAPTURE_INPUT_STAGE_INTERVAL_POLLS = '300'
    RECOMPONE_CAPTURE_INPUT_STAGE_END_POLL = ($EndPoll - 10).ToString()
    RECOMPONE_TEST_EXIT_INPUT_STAGE = 'race_1'
    RECOMPONE_TEST_EXIT_INPUT_STAGE_POLL = $EndPoll.ToString()
    RECOMPONE_EXIT_AFTER_INPUT_POLL = ($EndPoll + 10000).ToString()
    RECOMPONE_OUTPUT_RESOLUTION = '1280x960'
    RECOMPONE_PRESENTATION_RESOLUTION = '1280x960'
    RECOMPONE_PROCESS_PRIORITY = 'BelowNormal'
    RECOMPONE_CARD_A_PATH = (Join-Path $repo 'OpenGTPS1/carda.sav')
    RECOMPONE_CARD_B_PATH = (Join-Path $repo 'OpenGTPS1/cardb.sav')
    RECOMPONE_NATIVE_WORLD_DUMP_PATH = (Join-Path $evidence "$Tag.ogtwcap")
    RECOMPONE_NATIVE_WORLD_DUMP_INPUT_POLL = $DumpPoll.ToString()
    RECOMPONE_NATIVE_WORLD_DUMP_COUNT = '1'
    RECOMPONE_AUDIT_GT2_RESIDENT_EQUIVALENCE = $(if ($FullGeometry) {'1'} else {'0'})
    RECOMPONE_AUDIT_NATIVE_RESIDENT_EQUIVALENCE = $(if ($FullGeometry) {'1'} else {'0'})
}
foreach ($entry in $controls.GetEnumerator()) { $start.Environment[$entry.Key] = $entry.Value }
$process = [Diagnostics.Process]::Start($start)
$stdout = $process.StandardOutput.ReadToEndAsync()
$stderr = $process.StandardError.ReadToEndAsync()
if (-not $process.WaitForExit(300000)) {
    $process.Kill()
    throw 'SSR5 diagnostic timed out'
}
[IO.File]::WriteAllText((Join-Path $evidence "$Tag-stdout.log"), $stdout.Result)
[IO.File]::WriteAllText((Join-Path $evidence "$Tag-stderr.log"), $stderr.Result)
if ($process.ExitCode -ne 0) { throw "SSR5 exited $($process.ExitCode)" }
foreach ($poll in 600..($EndPoll - 10) | Where-Object { $_ % 300 -eq 0 }) {
    $source = Join-Path $deploy ('recompone_present_race_1_' + $poll.ToString('000000') + '_1280x960_fxaa.ppm')
    if ((Get-Item $source).LastWriteTime -lt $process.StartTime) { throw "Stale capture: $source" }
    & ffmpeg -hide_banner -loglevel error -y -i $source (Join-Path $evidence "$Tag-$poll.png")
    if ($LASTEXITCODE -ne 0) { throw 'Capture conversion failed' }
    Remove-Item -LiteralPath $source
}
Write-Output "SSR5 capture complete: $Tag exit=0"
