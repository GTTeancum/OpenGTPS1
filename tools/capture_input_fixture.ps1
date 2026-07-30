param(
    [Parameter(Mandatory = $true)]
    [string]$Fixture,
    [Parameter(Mandatory = $true)]
    [int]$ExitPoll,
    [Parameter(Mandatory = $true)]
    [string]$ArtifactName,
    [string]$DeployPath,
    [ValidateSet('PS1 Quality', 'Enhanced', 'Custom')]
    [string]$Preset = 'Enhanced',
    [switch]$AiAutoDrive,
    [ValidateRange(2, 64)]
    [int]$AiAutoDriveMaxEngagements = 2,
    [ValidateRange(0, 60000)]
    [int]$SoakQuickWinAfterAiTicks = 0,
    [switch]$SoakUnlockAllRaces,
    [switch]$CreateTestSave
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
if ($DeployPath) {
    $deploy = (Resolve-Path $DeployPath).Path
} else {
    $deploy = Join-Path $repo 'OpenGTPS1'
}
$exe = Join-Path $deploy 'GranTurismo2PC.exe'
if ([IO.Path]::IsPathRooted($Fixture)) {
    $fixtureCandidate = $Fixture
} else {
    $fixtureCandidate = Join-Path $repo $Fixture
}
$fixturePath = (Resolve-Path $fixtureCandidate).Path
$artifact = Join-Path $repo "artifacts\$ArtifactName"
$settings = Join-Path $deploy 'interface.ini'
$settingsHashBefore = (Get-FileHash -LiteralPath $settings -Algorithm SHA256).Hash

$capturePolls = [Collections.Generic.SortedSet[int]]::new()
foreach ($line in Get-Content -LiteralPath $fixturePath) {
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
foreach ($poll in $capturePolls) {
    $capture = Join-Path $deploy "recompone_capture__$poll.ppm"
    if (Test-Path -LiteralPath $capture) {
        Remove-Item -LiteralPath $capture -Force
    }
}

$start = [Diagnostics.ProcessStartInfo]::new()
$start.FileName = $exe
$start.WorkingDirectory = $deploy
$start.Arguments = '--headless'
$start.UseShellExecute = $false
$start.CreateNoWindow = $true
$start.RedirectStandardOutput = $true
$start.RedirectStandardError = $true
$start.EnvironmentVariables['RECOMPONE_INPUT_FILE'] = $fixturePath
$start.EnvironmentVariables['RECOMPONE_DISABLE_LIVE_INPUT'] = '1'
$start.EnvironmentVariables['RECOMPONE_SUPPRESS_RUMBLE'] = '1'
$start.EnvironmentVariables['RECOMPONE_UNTHROTTLED'] = '1'
$start.EnvironmentVariables['RECOMPONE_GRAPHICS_PRESET_OVERRIDE'] = $Preset
$start.EnvironmentVariables['RECOMPONE_EXIT_AFTER_INPUT_POLL'] = $ExitPoll.ToString()
if ($AiAutoDrive) {
    $start.EnvironmentVariables['RECOMPONE_GT2_AI_AUTODRIVE'] = '1'
    if ($AiAutoDriveMaxEngagements -ne 2) {
        $start.EnvironmentVariables['RECOMPONE_GT2_AI_AUTODRIVE_MAX_ENGAGEMENTS'] =
            $AiAutoDriveMaxEngagements.ToString()
    }
    if ($SoakQuickWinAfterAiTicks -gt 0) {
        $start.EnvironmentVariables['RECOMPONE_GT2_SOAK_QUICK_WIN_AFTER_AI_TICKS'] =
            $SoakQuickWinAfterAiTicks.ToString()
    }
}
if ($SoakUnlockAllRaces) {
    $start.EnvironmentVariables['RECOMPONE_GT2_SOAK_UNLOCK_ALL_RACES'] = '1'
}
if ($CreateTestSave) {
    $start.EnvironmentVariables['RECOMPONE_GT2_CREATE_TEST_SAVE'] = '1'
}

$process = [Diagnostics.Process]::Start($start)
$stdout = $process.StandardOutput.ReadToEndAsync()
$stderr = $process.StandardError.ReadToEndAsync()
if (-not $process.WaitForExit(600000)) {
    $process.Kill($true)
    $process.WaitForExit()
    throw 'Fixture capture timed out'
}

$stdoutText = $stdout.Result
$stderrText = $stderr.Result
[IO.File]::WriteAllText((Join-Path $artifact 'stdout.log'), $stdoutText)
[IO.File]::WriteAllText((Join-Path $artifact 'stderr.log'), $stderrText)
if ($process.ExitCode -ne 0) {
    throw "Fixture capture exited with code $($process.ExitCode)"
}
if ($stderrText -notmatch 'headless audio backend=dummy' -or
    $stderrText -notmatch 'SDL audio ready: driver=dummy') {
    throw 'Fixture capture did not prove the dummy audio backend'
}

$ffmpeg = (Get-Command ffmpeg -ErrorAction SilentlyContinue).Source
$copied = 0
foreach ($poll in $capturePolls) {
    $capture = Join-Path $deploy "recompone_capture__$poll.ppm"
    if (-not (Test-Path -LiteralPath $capture)) {
        continue
    }
    $ppm = Join-Path $artifact "capture-$poll.ppm"
    Copy-Item -LiteralPath $capture -Destination $ppm -Force
    if ($ffmpeg) {
        & $ffmpeg -hide_banner -loglevel error -y -i $ppm (
            Join-Path $artifact "capture-$poll.png")
        if ($LASTEXITCODE -ne 0) {
            throw "ffmpeg failed to convert capture poll $poll"
        }
    }
    $copied++
}
if ($copied -eq 0) {
    throw 'Fixture produced none of its requested captures'
}

$settingsHashAfter = (Get-FileHash -LiteralPath $settings -Algorithm SHA256).Hash
if ($settingsHashAfter -ne $settingsHashBefore) {
    throw 'Fixture capture changed persistent wrapper settings'
}

Write-Output "fixture=$fixturePath preset=$Preset captures=$copied artifact=$artifact"
Write-Output "audio safety=dummy settings_unchanged=$settingsHashAfter"
