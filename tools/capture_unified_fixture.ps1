param(
    [Parameter(Mandatory = $true)]
    [string]$Fixture,
    [Parameter(Mandatory = $true)]
    [string]$LoosePath,
    [Parameter(Mandatory = $true)]
    [int]$ExitPoll,
    [Parameter(Mandatory = $true)]
    [string]$ArtifactName,
    [ValidateSet('Enhanced')]
    [string]$Preset = 'Enhanced',
    [ValidateSet('simulation', 'arcade')]
    [string]$ExpectedGuest = 'arcade',
    [string]$DeployPath = 'tools\unified-host\bin\Release\net10.0',
    [switch]$StartArcade,
    [string]$ThrottleOnScriptStage = '',
    [switch]$AiAutoDrive,
    [ValidateRange(2, 64)]
    [int]$AiAutoDriveMaxEngagements = 2,
    [switch]$CreateTestSave,
    [switch]$CaptureCompatibilityUi
)

$ErrorActionPreference = 'Stop'
if ($IsWindows -or $env:OS -eq 'Windows_NT') {
    try {
        [Diagnostics.Process]::GetCurrentProcess().PriorityClass =
            [Diagnostics.ProcessPriorityClass]::BelowNormal
    } catch {
        Write-Warning (
            "Could not lower fixture-script priority: $($_.Exception.Message)")
    }
}
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$deployRoot = if ([IO.Path]::IsPathRooted($DeployPath)) {
    (Resolve-Path -LiteralPath $DeployPath).Path
} else {
    (Resolve-Path -LiteralPath (Join-Path $repo $DeployPath)).Path
}
$exe = Join-Path $deployRoot 'GranTurismo2PC.exe'
$runtimeDirectory = Split-Path -Parent $exe
$looseRoot = if ([IO.Path]::IsPathRooted($LoosePath)) {
    (Resolve-Path $LoosePath).Path
} else {
    (Resolve-Path (Join-Path $repo $LoosePath)).Path
}
$fixturePath = if ([IO.Path]::IsPathRooted($Fixture)) {
    (Resolve-Path $Fixture).Path
} else {
    (Resolve-Path (Join-Path $repo $Fixture)).Path
}
$artifact = Join-Path $repo "artifacts\$ArtifactName"

$capturePolls = [Collections.Generic.SortedSet[int]]::new()
$sectionOffset = 0
foreach ($line in Get-Content -LiteralPath $fixturePath) {
    if ($line -match '^\s*\[[^\]]+\]\s*$') {
        # Fixture section clocks restart when the native game changes phase.
        # Captures keep the section-relative poll in their generated filename.
        $sectionOffset++
        continue
    }
    if ($line -match '^\s*(\d+)\+\d+=.*\bCAPTURE\b') {
        $poll = [int]$Matches[1]
        if ($poll -le $ExitPoll) {
            [void]$capturePolls.Add($poll)
        }
    }
}
if ($capturePolls.Count -eq 0) {
    throw "No CAPTURE markers at or before poll $ExitPoll in $fixturePath"
}

New-Item -ItemType Directory -Path $artifact -Force | Out-Null
foreach ($filter in @('recompone_capture*.ppm', 'recompone_present*.ppm')) {
    foreach ($capture in Get-ChildItem -LiteralPath $runtimeDirectory `
            -Filter $filter -File) {
        Remove-Item -LiteralPath $capture.FullName -Force
    }
}

$start = [Diagnostics.ProcessStartInfo]::new()
$start.FileName = $exe
$start.WorkingDirectory = $runtimeDirectory
$start.Arguments = (
    '--headless ' +
    $(if ($StartArcade) { '--start-arcade ' } else { '' }) +
    "`"$looseRoot`"")
$start.UseShellExecute = $false
$start.CreateNoWindow = $true
$start.RedirectStandardOutput = $true
$start.RedirectStandardError = $true
$env:RECOMPONE_INPUT_FILE = $fixturePath
$env:RECOMPONE_DISABLE_LIVE_INPUT = '1'
$env:RECOMPONE_SUPPRESS_RUMBLE = '1'
$env:RECOMPONE_UNTHROTTLED = '1'
$env:RECOMPONE_PROCESS_PRIORITY = 'BelowNormal'
if (-not $CaptureCompatibilityUi) {
    $env:RECOMPONE_PRESENTATION_CAPTURE = '1'
}
$env:RECOMPONE_GRAPHICS_PRESET_OVERRIDE = $Preset
$env:RECOMPONE_EXIT_AFTER_INPUT_POLL = $ExitPoll.ToString()
if (-not [string]::IsNullOrWhiteSpace($ThrottleOnScriptStage)) {
    $env:RECOMPONE_THROTTLE_ON_SCRIPT_STAGE = $ThrottleOnScriptStage
}
if ($CreateTestSave) {
    $env:RECOMPONE_GT2_CREATE_TEST_SAVE = '1'
}
if ($AiAutoDrive) {
    $env:RECOMPONE_GT2_AI_AUTODRIVE = '1'
    $env:RECOMPONE_GT2_AI_AUTODRIVE_MAX_ENGAGEMENTS =
        $AiAutoDriveMaxEngagements.ToString()
}

$cardAPath = Join-Path $runtimeDirectory 'carda.sav'
$cardAOriginal = if ($CreateTestSave -and
    (Test-Path -LiteralPath $cardAPath -PathType Leaf)) {
    [IO.File]::ReadAllBytes($cardAPath)
} else {
    $null
}
$process = [Diagnostics.Process]::Start($start)
try {
    $process.PriorityClass = [Diagnostics.ProcessPriorityClass]::BelowNormal
} catch {
    Write-Warning "Could not lower unified fixture priority: $($_.Exception.Message)"
}
$stdoutTask = $process.StandardOutput.ReadToEndAsync()
$stderrTask = $process.StandardError.ReadToEndAsync()
if (-not $process.WaitForExit(600000)) {
    $process.Kill($true)
    $process.WaitForExit()
    if ($null -ne $cardAOriginal) {
        [IO.File]::WriteAllBytes($cardAPath, $cardAOriginal)
    }
    throw 'Unified fixture capture timed out'
}
$stdout = $stdoutTask.Result
$stderr = $stderrTask.Result
if ($null -ne $cardAOriginal) {
    [IO.File]::WriteAllBytes($cardAPath, $cardAOriginal)
}
[IO.File]::WriteAllText((Join-Path $artifact 'stdout.log'), $stdout)
[IO.File]::WriteAllText((Join-Path $artifact 'stderr.log'), $stderr)
if ($process.ExitCode -ne 0) {
    throw "Unified fixture capture exited with code $($process.ExitCode)"
}
if ($stdout -notmatch (
        '\[Host\] native unified guest=' + [regex]::Escape($ExpectedGuest) + ' ')) {
    throw (
        "Unified fixture did not hand off to the expected $ExpectedGuest guest")
}
if ($ExpectedGuest -eq 'arcade') {
    if (-not $StartArcade) {
        if ($stdout -notmatch (
                '\[Host\] seamless guest handoff: Simulation title -> ' +
                'Arcade Mode menu')) {
            throw 'Unified Arcade fixture did not use the seamless handoff'
        }
        if ($stdout -notmatch (
                '\[GT2\] Arcade frontend handoff: ' +
                'entry=0x8005D650 Arcade Mode menu overlay=2')) {
            throw 'Unified Arcade fixture did not enter the Arcade Mode menu'
        }
    }
    if ($stdout -match 'loaded overlay: gt2_arcade_overlay_5') {
        throw 'Unified Arcade fixture replayed the boot/title overlay'
    }
    if ($stdout -match 'loaded overlay: gt2_arcade_overlay_1') {
        throw 'Unified Arcade fixture exposed the skipped Arcade disc title'
    }
    if ($stdout -notmatch 'loaded overlay: gt2_arcade_overlay_2') {
        throw 'Unified Arcade fixture did not load the Arcade Mode menu overlay'
    }
}
if ($stderr -match 'unmapped call|Unhandled exception|unknown software exception') {
    throw "Unified fixture reported a runtime failure:`n$stderr"
}

$ffmpeg = (Get-Command ffmpeg -ErrorAction SilentlyContinue).Source
$copied = 0
$captureNames = [Collections.Generic.SortedSet[string]]::new()
foreach ($match in [regex]::Matches(
        $stdout,
        'captured (?:stage|presentation) .*? to ' +
        '((?:recompone_capture|recompone_present)\S+?\.ppm)')) {
    [void]$captureNames.Add($match.Groups[1].Value)
}
foreach ($captureName in $captureNames) {
    $capture = Join-Path $runtimeDirectory $captureName
    if (-not (Test-Path -LiteralPath $capture -PathType Leaf)) {
        continue
    }
    $ppm = Join-Path $artifact $captureName
    Copy-Item -LiteralPath $capture -Destination $ppm -Force
    if ($ffmpeg) {
        $png = Join-Path $artifact (
            [IO.Path]::ChangeExtension($captureName, '.png'))
        $ffmpegStart = [Diagnostics.ProcessStartInfo]::new()
        $ffmpegStart.FileName = $ffmpeg
        $ffmpegStart.UseShellExecute = $false
        $ffmpegStart.CreateNoWindow = $true
        $escapedPpm = $ppm.Replace('"', '\"')
        $escapedPng = $png.Replace('"', '\"')
        $ffmpegStart.Arguments = (
            "-hide_banner -loglevel error -y -i `"$escapedPpm`" " +
            "`"$escapedPng`"")
        $ffmpegProcess = [Diagnostics.Process]::Start($ffmpegStart)
        try {
            $ffmpegProcess.PriorityClass =
                [Diagnostics.ProcessPriorityClass]::BelowNormal
        } catch {
            Write-Warning (
                "Could not lower ffmpeg priority: $($_.Exception.Message)")
        }
        $ffmpegProcess.WaitForExit()
        if ($ffmpegProcess.ExitCode -ne 0) {
            throw "ffmpeg failed to convert capture $captureName"
        }
    }
    $copied++
}
if ($copied -eq 0) {
    throw 'Unified fixture produced none of its requested captures'
}

Write-Output (
    "fixture=$fixturePath preset=$Preset captures=$copied artifact=$artifact")
Write-Output (
    "native_$ExpectedGuest=true auto_drive=$($AiAutoDrive.IsPresent) " +
    "start_arcade=$($StartArcade.IsPresent) " +
    "test_save=$($CreateTestSave.IsPresent) exit=$($process.ExitCode)")

foreach ($name in @(
        'RECOMPONE_INPUT_FILE',
        'RECOMPONE_DISABLE_LIVE_INPUT',
        'RECOMPONE_SUPPRESS_RUMBLE',
        'RECOMPONE_UNTHROTTLED',
        'RECOMPONE_PROCESS_PRIORITY',
        'RECOMPONE_PRESENTATION_CAPTURE',
        'RECOMPONE_GRAPHICS_PRESET_OVERRIDE',
        'RECOMPONE_EXIT_AFTER_INPUT_POLL',
        'RECOMPONE_THROTTLE_ON_SCRIPT_STAGE',
        'RECOMPONE_GT2_CREATE_TEST_SAVE',
        'RECOMPONE_GT2_AI_AUTODRIVE',
        'RECOMPONE_GT2_AI_AUTODRIVE_MAX_ENGAGEMENTS')) {
    Remove-Item "Env:$name" -ErrorAction SilentlyContinue
}
