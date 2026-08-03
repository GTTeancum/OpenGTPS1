param(
    [Parameter(Mandatory = $true)]
    [string]$Fixture,
    [Parameter(Mandatory = $true)]
    [string]$LoosePath,
    [Parameter(Mandatory = $true)]
    [int]$ExitPoll,
    [Parameter(Mandatory = $true)]
    [string]$ArtifactName,
    [ValidateSet('PS1 Quality', 'Enhanced', 'Custom')]
    [string]$Preset = 'Enhanced',
    [switch]$AiAutoDrive,
    [ValidateRange(2, 64)]
    [int]$AiAutoDriveMaxEngagements = 2
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$exe = Join-Path $repo (
    'tools\unified-host\bin\Release\net10.0\GranTurismo2PC.exe')
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
foreach ($capture in Get-ChildItem -LiteralPath $runtimeDirectory `
        -Filter 'recompone_capture*.ppm' -File) {
    Remove-Item -LiteralPath $capture.FullName -Force
}

$start = [Diagnostics.ProcessStartInfo]::new()
$start.FileName = $exe
$start.WorkingDirectory = $runtimeDirectory
$start.Arguments = "--headless `"$looseRoot`""
$start.UseShellExecute = $false
$start.CreateNoWindow = $true
$start.RedirectStandardOutput = $true
$start.RedirectStandardError = $true
$env:RECOMPONE_INPUT_FILE = $fixturePath
$env:RECOMPONE_DISABLE_LIVE_INPUT = '1'
$env:RECOMPONE_SUPPRESS_RUMBLE = '1'
$env:RECOMPONE_UNTHROTTLED = '1'
$env:RECOMPONE_GRAPHICS_PRESET_OVERRIDE = $Preset
$env:RECOMPONE_EXIT_AFTER_INPUT_POLL = $ExitPoll.ToString()
$env:RECOMPONE_NATIVE_WORLD_RENDERER = '0'
if ($AiAutoDrive) {
    $env:RECOMPONE_GT2_AI_AUTODRIVE = '1'
    $env:RECOMPONE_GT2_AI_AUTODRIVE_MAX_ENGAGEMENTS =
        $AiAutoDriveMaxEngagements.ToString()
}

$process = [Diagnostics.Process]::Start($start)
$stdoutTask = $process.StandardOutput.ReadToEndAsync()
$stderrTask = $process.StandardError.ReadToEndAsync()
if (-not $process.WaitForExit(600000)) {
    $process.Kill($true)
    $process.WaitForExit()
    throw 'Unified fixture capture timed out'
}
$stdout = $stdoutTask.Result
$stderr = $stderrTask.Result
[IO.File]::WriteAllText((Join-Path $artifact 'stdout.log'), $stdout)
[IO.File]::WriteAllText((Join-Path $artifact 'stderr.log'), $stderr)
if ($process.ExitCode -ne 0) {
    throw "Unified fixture capture exited with code $($process.ExitCode)"
}
if ($stdout -notmatch '\[Host\] native unified guest=arcade ') {
    throw 'Unified fixture did not hand off to the native Arcade guest'
}
if ($stderr -match 'unmapped call|Unhandled exception|unknown software exception') {
    throw "Unified fixture reported a runtime failure:`n$stderr"
}

$ffmpeg = (Get-Command ffmpeg -ErrorAction SilentlyContinue).Source
$copied = 0
$captureNames = [Collections.Generic.SortedSet[string]]::new()
foreach ($match in [regex]::Matches(
        $stdout, 'captured stage .*? to (recompone_capture\S+?\.ppm)')) {
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
        & $ffmpeg -hide_banner -loglevel error -y -i $ppm (
            Join-Path $artifact ([IO.Path]::ChangeExtension($captureName, '.png')))
        if ($LASTEXITCODE -ne 0) {
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
    "native_arcade=true auto_drive=$($AiAutoDrive.IsPresent) exit=$($process.ExitCode)")

foreach ($name in @(
        'RECOMPONE_INPUT_FILE',
        'RECOMPONE_DISABLE_LIVE_INPUT',
        'RECOMPONE_SUPPRESS_RUMBLE',
        'RECOMPONE_UNTHROTTLED',
        'RECOMPONE_GRAPHICS_PRESET_OVERRIDE',
        'RECOMPONE_EXIT_AFTER_INPUT_POLL',
        'RECOMPONE_NATIVE_WORLD_RENDERER',
        'RECOMPONE_GT2_AI_AUTODRIVE',
        'RECOMPONE_GT2_AI_AUTODRIVE_MAX_ENGAGEMENTS')) {
    Remove-Item "Env:$name" -ErrorAction SilentlyContinue
}
