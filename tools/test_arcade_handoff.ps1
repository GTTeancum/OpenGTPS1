param(
    [string]$DeployPath = 'OpenGTPS1',
    [string]$DataPath = 'OpenGTPS1',
    [int]$ConfirmPoll = 2,
    [string]$EvidenceName = 'cross-only'
)
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$deploy = [IO.Path]::GetFullPath((Join-Path $repo $DeployPath))
$data = [IO.Path]::GetFullPath((Join-Path $repo $DataPath))
$evidence = Join-Path $repo "artifacts\arcade-handoff-final\$EvidenceName"
New-Item -ItemType Directory -Path $evidence -Force | Out-Null
foreach ($card in @('carda.sav', 'cardb.sav')) {
    Copy-Item -LiteralPath (Join-Path $deploy $card) -Destination $evidence
}
$env:RECOMPONE_CARD_A_PATH = Join-Path $evidence 'carda.sav'
$env:RECOMPONE_CARD_B_PATH = Join-Path $evidence 'cardb.sav'
$env:RECOMPONE_INPUT_SCRIPT = "[unified_title];1+1=CAPTURE;$ConfirmPoll+8=CROSS;[arcade_frontend];120+1=CAPTURE"
$env:RECOMPONE_EXIT_AFTER_INPUT_POLL = '1600'
$env:RECOMPONE_TEST_EXIT_INPUT_STAGE = 'arcade_frontend'
$env:RECOMPONE_TEST_EXIT_INPUT_STAGE_POLL = '150'
$env:RECOMPONE_DISABLE_LIVE_INPUT = '1'
$env:RECOMPONE_SUPPRESS_RUMBLE = '1'
$env:RECOMPONE_UNTHROTTLED = '1'
$env:RECOMPONE_CAPTURE_AUTOMATIC_STAGE = '0'
$env:RECOMPONE_PRESENTATION_CAPTURE = '1'
$env:RECOMPONE_OUTPUT_RESOLUTION = '1280x960'
$env:RECOMPONE_PRESENTATION_RESOLUTION = '1280x960'
$env:RECOMPONE_AUDIO_CAPTURE = Join-Path $evidence 'confirmation.wav'
$env:RECOMPONE_AUDIO_CAPTURE_START_INPUT_POLL = '770'
$env:RECOMPONE_AUDIO_CAPTURE_PRE_VOLUME = '1'
$exe = Join-Path $deploy 'GranTurismo2PC.exe'
$stdoutPath = Join-Path $evidence 'stdout.log'
$stderrPath = Join-Path $evidence 'stderr.log'
$process = Start-Process -FilePath $exe -ArgumentList @('--headless', "`"$data`"") `
    -WorkingDirectory $evidence -WindowStyle Hidden -PassThru `
    -RedirectStandardOutput $stdoutPath -RedirectStandardError $stderrPath
$null = $process.Handle # retain exit status after Windows closes the process
if (-not $process.WaitForExit(120000)) {
    $process.Kill()
    throw 'Packaged Arcade handoff timed out'
}
$process.Refresh()
if ($process.ExitCode -ne 0) { throw "Packaged handoff exited $($process.ExitCode)" }
$stdout = Get-Content -LiteralPath $stdoutPath -Raw
foreach ($marker in @(
    'voiceCompleted=True tailCompleted=True',
    'omitted duplicate timed boot panels',
    'seamless Arcade frontend entered')) {
    if (-not $stdout.Contains($marker)) { throw "Missing handoff proof: $marker" }
}
if ($ConfirmPoll -lt 16 -and $stdout -notmatch 'buffered unified-title input accepted') {
    throw 'Early X press was not buffered'
}
if ($stdout -notmatch 'selectionToFrontendPolls=(\d+)' -or [int]$Matches[1] -gt 60) {
    throw 'Packaged Arcade handoff exceeded its frontend-entry poll budget'
}
foreach ($capture in @(
    @('recompone_present_unified_title_0001_1280x960_fxaa.ppm', 'title.png'),
    @('recompone_present_arcade_frontend_0120_1280x960_fxaa.ppm', 'arcade-menu.png'))) {
    # The host deliberately sets its working directory to the executable's
    # directory during startup, including for hidden process-local tests.
    $source = Join-Path $deploy $capture[0]
    if ((Get-Item -LiteralPath $source).LastWriteTime -lt $process.StartTime) {
        throw "Stale capture: $source"
    }
    & ffmpeg -hide_banner -loglevel error -y -i $source (Join-Path $evidence $capture[1])
    if ($LASTEXITCODE -ne 0) { throw "Capture conversion failed: $source" }
    Remove-Item -LiteralPath $source
}
Get-FileHash -Algorithm SHA256 -LiteralPath $exe
Write-Output ($stdout -split "`n" | Where-Object { $_ -match 'confirmation audio|selectionToFrontend|phase=disc-open' })
Write-Output "packaged_arcade_handoff=pass evidence=$evidence"
