param(
    [string]$LoosePath = 'work\gt2-unified',
    [string]$DeployPath = 'tools\unified-host\bin\Release\net10.0',
    [string]$CaseFilter = '*'
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$exe = Join-Path (Join-Path $repo $DeployPath) 'GranTurismo2PC.exe'
$runtimeDirectory = Split-Path -Parent $exe
$evidenceRoot = Join-Path $repo 'artifacts\arcade-handoff-final\menu-smoke'
New-Item -ItemType Directory -Path $evidenceRoot -Force | Out-Null
$looseRoot = if ([IO.Path]::IsPathRooted($LoosePath)) {
    [IO.Path]::GetFullPath($LoosePath)
} else {
    [IO.Path]::GetFullPath((Join-Path $repo $LoosePath))
}

foreach ($required in @(
        $exe,
        (Join-Path $looseRoot 'GT2.VOL'),
        (Join-Path $looseRoot 'MUSIC.DAT'),
        (Join-Path $looseRoot 'manifests\simulation.json'),
        (Join-Path $looseRoot 'manifests\arcade.json'))) {
    if (-not (Test-Path -LiteralPath $required -PathType Leaf)) {
        throw "Unified smoke-test input is missing: $required"
    }
}

$cases = @(
    @{
        Name = 'simulation-return'
        Script = (
            '860+1=CAPTURE;' +
            '900+4=DOWN;' +
            '980+8=CROSS,START;' +
            '1500+1=CAPTURE;' +
            '1900+8=TRIANGLE;' +
            '2700+1=CAPTURE')
        ExitPoll = 3100
        Expected = '[GT2] title selection: Gran Turismo Mode'
    },
    @{
        Name = 'arcade'
        Script = (
            '860+1=CAPTURE;' +
            '900+8=CROSS,START;' +
            '1200+1=CAPTURE;' +
            '1400+1=CAPTURE')
        # Remain idle in the requested Arcade Mode menu to prove that the
        # skipped disc title never appears later as an attract transition.
        ExitPoll = 3200
        Expected = '[GT2] title selection: Arcade Mode'
    },
    @{
        Name = 'arcade-single-player'
        Script = (
            '900+8=CROSS,START;' +
            '1600+1=CAPTURE;' +
            '2000+8=CROSS,START;' +
            '3000+1=CAPTURE')
        ExitPoll = 3400
        Expected = '[GT2] title selection: Arcade Mode'
    },
    @{
        Name = 'arcade-return'
        Script = (
            '860+1=CAPTURE;' +
            '900+8=CROSS,START;' +
            '1600+1=CAPTURE;' +
            '2000+8=TRIANGLE;' +
            '2800+1=CAPTURE')
        ExitPoll = 3200
        Expected = '[GT2] Arcade Mode Back: returning to unified title'
    },
    @{
        Name = 'replay-theater'
        Script = (
            '900+4=DOWN;' +
            '980+4=DOWN;' +
            '1060+8=CROSS,START;' +
            '1600+1=CAPTURE')
        ExitPoll = 1800
        Expected = '[GT2] title selection: Replay Theater'
    },
    @{
        Name = 'option'
        Script = (
            '900+4=DOWN;' +
            '980+4=DOWN;' +
            '1060+4=DOWN;' +
            '1140+8=CROSS,START;' +
            '1700+1=CAPTURE')
        ExitPoll = 1900
        Expected = '[GT2] title selection: Option'
    }
)

foreach ($case in $cases) {
    if ($case.Name -notlike $CaseFilter) { continue }
    # Set the child-only controls on this short-lived PowerShell process.
    # Avoid ProcessStartInfo.EnvironmentVariables: some Windows launchers
    # provide both Path and PATH, which makes the .NET Framework dictionary
    # reject the inherited environment before the child can start.
    $env:RECOMPONE_INPUT_SCRIPT = $case.Script
    $env:RECOMPONE_EXIT_AFTER_INPUT_POLL = $case.ExitPoll.ToString()
    $env:RECOMPONE_GT2_SKIP_OPENING_PRELUDE = '1'
    $env:RECOMPONE_DISABLE_LIVE_INPUT = '1'
    $env:RECOMPONE_SUPPRESS_RUMBLE = '1'
    $env:RECOMPONE_UNTHROTTLED = '1'
    $env:RECOMPONE_CAPTURE_AUTOMATIC_STAGE = '0'
    $env:RECOMPONE_PRESENTATION_CAPTURE = '1'
    $env:RECOMPONE_OUTPUT_RESOLUTION = '1280x960'
    $env:RECOMPONE_PRESENTATION_RESOLUTION = '1280x960'

    $start = [Diagnostics.ProcessStartInfo]::new()
    $start.FileName = $exe
    $start.WorkingDirectory = $runtimeDirectory
    $start.Arguments = "--headless `"$looseRoot`""
    $start.UseShellExecute = $false
    $start.CreateNoWindow = $true
    $start.RedirectStandardOutput = $true
    $start.RedirectStandardError = $true

    $process = [Diagnostics.Process]::Start($start)
    $stdoutTask = $process.StandardOutput.ReadToEndAsync()
    $stderrTask = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit(120000)) {
        $process.Kill($true)
        $process.WaitForExit()
        throw "Unified $($case.Name) menu-driven smoke test timed out"
    }
    $stdout = $stdoutTask.Result
    $stderr = $stderrTask.Result
    $caseEvidence = Join-Path $evidenceRoot $case.Name
    New-Item -ItemType Directory -Path $caseEvidence -Force | Out-Null
    [IO.File]::WriteAllText((Join-Path $caseEvidence 'stdout.log'), $stdout)
    [IO.File]::WriteAllText((Join-Path $caseEvidence 'stderr.log'), $stderr)
    foreach ($captureMatch in [regex]::Matches(
            $stdout, 'to (recompone_present_[^\r\n]+\.ppm)')) {
        $capturePath = Join-Path $runtimeDirectory $captureMatch.Groups[1].Value
        if ((Get-Item -LiteralPath $capturePath).LastWriteTime -lt $process.StartTime) {
            throw "Stale smoke capture: $capturePath"
        }
        $pngPath = Join-Path $caseEvidence ([IO.Path]::ChangeExtension(
            $captureMatch.Groups[1].Value, '.png'))
        & ffmpeg -hide_banner -loglevel error -y -i $capturePath $pngPath
        if ($LASTEXITCODE -ne 0) { throw "Capture conversion failed: $capturePath" }
        Remove-Item -LiteralPath $capturePath
    }
    if ($process.ExitCode -ne 0) {
        throw "Unified $($case.Name) exited $($process.ExitCode):`n$stderr"
    }
    if (-not $stdout.Contains($case.Expected)) {
        throw "Unified title did not select $($case.Name):`n$stdout"
    }
    $arcadeCase = $case.Name.StartsWith(
        'arcade', [StringComparison]::Ordinal)
    if ($arcadeCase -and
        $stdout -notmatch '\[Host\] native unified guest=arcade ') {
        throw "Unified title did not hand off to the Arcade guest:`n$stdout"
    }
    if ($arcadeCase -and
        $stdout -notmatch (
            '\[Host\] seamless guest handoff: Simulation title -> ' +
            'Arcade Mode menu')) {
        throw "Unified title did not use the seamless Arcade handoff:`n$stdout"
    }
    if ($arcadeCase -and
        $stdout -notmatch (
            '\[GT2\] Arcade frontend handoff: ' +
            'entry=0x8005D650 Arcade Mode menu overlay=2')) {
        throw "Unified title did not enter the Arcade Mode menu:`n$stdout"
    }
    if ($arcadeCase -and
        $stdout -match 'loaded overlay: gt2_arcade_overlay_5') {
        throw "Unified title incorrectly replayed the Arcade boot/title overlay:`n$stdout"
    }
    if ($arcadeCase -and
        $stdout -match 'loaded overlay: gt2_arcade_overlay_1') {
        throw "Unified title exposed the skipped Arcade disc title:`n$stdout"
    }
    if ($arcadeCase -and
        $stdout -notmatch 'loaded overlay: gt2_arcade_overlay_2') {
        throw "Unified title did not load the Arcade Mode menu overlay:`n$stdout"
    }
    if ($arcadeCase -and
        $stdout -notmatch 'omitted duplicate timed boot panels') {
        throw "Unified Arcade still executes the hidden 310-tick boot panels:`n$stdout"
    }
    if ($arcadeCase -and
        ($stdout -notmatch 'selectionToFrontendPolls=(\d+)' -or
         [int]$Matches[1] -gt 60)) {
        throw "Unified Arcade frontend entry regressed to a long guest-frame wait:`n$stdout"
    }
    if ($arcadeCase -and
        $stdout -notmatch (
            '\[GT2\] seamless Arcade frontend entered; ' +
            'transition cover released')) {
        throw "Unified title exposed an incomplete Arcade transition:`n$stdout"
    }
    if ($arcadeCase -and
        $stdout -match 'loaded overlay: gt2_arcade_overlay_0') {
        throw "Unified Arcade menu unexpectedly launched a race:`n$stdout"
    }
    if ($case.Name -eq 'arcade-return' -and
        $stdout -notmatch (
            '\[Host\] seamless guest handoff: ' +
            'Arcade Mode Back -> unified title')) {
        throw "Arcade Mode Back did not return to the unified title:`n$stdout"
    }
    if ($case.Name -eq 'arcade-return' -and
        $stdout -notmatch (
            '\[GT2\] Simulation title handoff: ' +
            'entry=0x8005D6E0 unified title overlay=1')) {
        throw "Arcade Mode Back replayed the Simulation boot path:`n$stdout"
    }
    if ($case.Name -eq 'arcade-single-player' -and
        $stdout -match 'Arcade Mode Back|Gran Turismo Mode Back') {
        throw "Arcade Single Player escaped to the unified title:`n$stdout"
    }
    # Capture the actual shipping presentation, not the retired compatibility
    # framebuffer. The Game Selection image is reviewed with the batch proofs.
    if ($case.Name -eq 'arcade-single-player' -and
        [regex]::Matches($stdout, 'captured presentation').Count -lt 2) {
        throw "Arcade Single Player is missing its Game Selection visual proof:`n$stdout"
    }
    if ($case.Name -eq 'simulation-return' -and
        $stdout -notmatch (
            '\[GT2\] Gran Turismo Mode Back: returning to unified title')) {
        throw "Gran Turismo Mode Triangle did not request the unified title:`n$stdout"
    }
    if ($case.Name -eq 'simulation-return' -and
        $stdout -notmatch (
            '\[Host\] seamless guest handoff: ' +
            'Gran Turismo Mode Back -> unified title')) {
        throw "Gran Turismo Mode Back did not return to the unified title:`n$stdout"
    }
    if ($case.Name -eq 'simulation-return' -and
        $stdout -notmatch (
            '\[GT2\] Simulation title handoff: ' +
            'entry=0x8005D6E0 unified title overlay=1')) {
        throw "Gran Turismo Mode Back replayed the Simulation boot path:`n$stdout"
    }
    if ($stdout -notmatch '\[GPU\] display=True') {
        throw "Unified $($case.Name) did not enable its original display:`n$stdout"
    }
    if ($stderr -match 'unmapped call|Unhandled exception|unknown software exception') {
        throw "Unified $($case.Name) reported a runtime failure:`n$stderr"
    }

    Write-Output (
        "Unified $($case.Name) menu-driven smoke passed: " +
        "exit=$($process.ExitCode)")
}

foreach ($name in @(
        'RECOMPONE_INPUT_SCRIPT',
        'RECOMPONE_EXIT_AFTER_INPUT_POLL',
        'RECOMPONE_GT2_SKIP_OPENING_PRELUDE',
        'RECOMPONE_DISABLE_LIVE_INPUT',
        'RECOMPONE_SUPPRESS_RUMBLE',
        'RECOMPONE_CAPTURE_AUTOMATIC_STAGE',
        'RECOMPONE_PRESENTATION_CAPTURE',
        'RECOMPONE_OUTPUT_RESOLUTION',
        'RECOMPONE_PRESENTATION_RESOLUTION',
        'RECOMPONE_UNTHROTTLED')) {
    Remove-Item "Env:$name" -ErrorAction SilentlyContinue
}
