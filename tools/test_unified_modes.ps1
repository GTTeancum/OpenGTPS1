param(
    [string]$LoosePath = 'work\gt2-unified'
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$exe = Join-Path $repo (
    'tools\unified-host\bin\Release\net10.0\GranTurismo2PC.exe')
$runtimeDirectory = Split-Path -Parent $exe
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
        Name = 'simulation'
        Script = (
            '560+1=CAPTURE;' +
            '600+4=DOWN;' +
            '640+8=CROSS,START;' +
            '1400+1=CAPTURE;' +
            '2000+1=CAPTURE')
        ExitPoll = 2500
        Expected = '[GT2] title selection: Gran Turismo Mode'
    },
    @{
        Name = 'arcade'
        Script = (
            '560+1=CAPTURE;' +
            '600+8=CROSS,START;' +
            '900+1=CAPTURE;' +
            '1400+1=CAPTURE')
        # Remain idle well past Arcade overlay 1's stock 901-update attract
        # threshold. A unified PC menu must stay in the frontend instead of
        # silently launching the default Seattle demo.
        ExitPoll = 3200
        Expected = '[GT2] title selection: Arcade Mode'
    },
    @{
        Name = 'replay-theater'
        Script = (
            '600+4=DOWN;' +
            '680+4=DOWN;' +
            '760+8=CROSS,START;' +
            '1300+1=CAPTURE')
        ExitPoll = 1500
        Expected = '[GT2] title selection: Replay Theater'
    },
    @{
        Name = 'option'
        Script = (
            '600+4=DOWN;' +
            '680+4=DOWN;' +
            '760+4=DOWN;' +
            '840+8=CROSS,START;' +
            '1400+1=CAPTURE')
        ExitPoll = 1600
        Expected = '[GT2] title selection: Option'
    }
)

foreach ($case in $cases) {
    # Set the child-only controls on this short-lived PowerShell process.
    # Avoid ProcessStartInfo.EnvironmentVariables: some Windows launchers
    # provide both Path and PATH, which makes the .NET Framework dictionary
    # reject the inherited environment before the child can start.
    $env:RECOMPONE_INPUT_SCRIPT = $case.Script
    $env:RECOMPONE_EXIT_AFTER_INPUT_POLL = $case.ExitPoll.ToString()
    $env:RECOMPONE_DISABLE_LIVE_INPUT = '1'
    $env:RECOMPONE_SUPPRESS_RUMBLE = '1'
    $env:RECOMPONE_UNTHROTTLED = '1'

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
    if ($process.ExitCode -ne 0) {
        throw "Unified $($case.Name) exited $($process.ExitCode):`n$stderr"
    }
    if (-not $stdout.Contains($case.Expected)) {
        throw "Unified title did not select $($case.Name):`n$stdout"
    }
    if ($case.Name -eq 'arcade' -and
        $stdout -notmatch '\[Host\] native unified guest=arcade ') {
        throw "Unified title did not hand off to the Arcade guest:`n$stdout"
    }
    if ($case.Name -eq 'arcade' -and
        $stdout -notmatch (
            '\[Host\] seamless guest handoff: Simulation title -> ' +
            'Arcade START GAME destination')) {
        throw "Unified title did not use the seamless Arcade handoff:`n$stdout"
    }
    if ($case.Name -eq 'arcade' -and
        $stdout -notmatch (
            '\[GT2\] Arcade frontend handoff: ' +
            'entry=0x8005D650 START GAME overlay=1')) {
        throw "Unified title did not enter Arcade at START GAME:`n$stdout"
    }
    if ($case.Name -eq 'arcade' -and
        $stdout -match 'loaded overlay: gt2_arcade_overlay_5') {
        throw "Unified title incorrectly replayed the Arcade boot/title overlay:`n$stdout"
    }
    if ($case.Name -eq 'arcade' -and
        $stdout -notmatch 'loaded overlay: gt2_arcade_overlay_1') {
        throw "Unified title did not load the first Arcade frontend overlay:`n$stdout"
    }
    if ($case.Name -eq 'arcade' -and
        $stdout -notmatch (
            '\[GT2\] seamless Arcade frontend ready; ' +
            'transition cover released')) {
        throw "Unified title exposed an incomplete Arcade transition:`n$stdout"
    }
    if ($case.Name -eq 'arcade' -and
        $stdout -match 'loaded overlay: gt2_arcade_overlay_0') {
        throw "Idle unified Arcade frontend launched the Seattle attract race:`n$stdout"
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
        'RECOMPONE_DISABLE_LIVE_INPUT',
        'RECOMPONE_SUPPRESS_RUMBLE',
        'RECOMPONE_UNTHROTTLED')) {
    Remove-Item "Env:$name" -ErrorAction SilentlyContinue
}
