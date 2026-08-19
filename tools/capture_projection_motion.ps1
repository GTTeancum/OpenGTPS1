param(
    [ValidateSet('Modern')]
    [string]$Feature = 'Modern',
    [int]$StartPoll = 9400,
    [int]$EndPoll = 10300,
    [string]$ArtifactName = 'projection-motion-current',
    [string]$Fixture = 'tests\fixtures\ai-autodrive-save-sunday-race.input',
    [ValidateSet('Fixed')]
    [string]$Setting = 'Fixed'
)

$ErrorActionPreference = 'Stop'
if ($EndPoll -le $StartPoll) {
    throw 'EndPoll must be greater than StartPoll'
}

$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$deploy = Join-Path $repo 'OpenGTPS1'
$exe = Join-Path $deploy 'GranTurismo2PC.exe'
$fixture = if ([IO.Path]::IsPathRooted($Fixture)) {
    $Fixture
} else {
    Join-Path $repo $Fixture
}
if (-not (Test-Path -LiteralPath $fixture -PathType Leaf)) {
    throw "Input fixture not found: $fixture"
}
$artifact = Join-Path $repo "artifacts\$ArtifactName"
$settings = Join-Path $deploy 'interface.ini'
$settingsHashBefore = (Get-FileHash -LiteralPath $settings -Algorithm SHA256).Hash
New-Item -ItemType Directory -Path $artifact -Force | Out-Null
$featureSlug = $Feature.ToLowerInvariant()
foreach ($setting in @($Setting)) {
    $settingSlug = $setting.ToLowerInvariant()
    $video = Join-Path $artifact "$featureSlug-$settingSlug.mp4"
    if (Test-Path -LiteralPath $video) {
        Remove-Item -LiteralPath $video -Force
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
    $start.EnvironmentVariables['SDL_AUDIODRIVER'] = 'dummy'
    $start.EnvironmentVariables['RECOMPONE_GT2_AI_AUTODRIVE'] = '1'
    $start.EnvironmentVariables['RECOMPONE_TRACE_GPU_PRIMITIVES'] = '1'
    $start.EnvironmentVariables['RECOMPONE_GRAPHICS_PRESET_OVERRIDE'] = 'Enhanced'
    $start.EnvironmentVariables['RECOMPONE_VIDEO_CAPTURE'] = $video
    $start.EnvironmentVariables['RECOMPONE_VIDEO_START_INPUT_POLL'] = $StartPoll.ToString()
    $start.EnvironmentVariables['RECOMPONE_VIDEO_END_INPUT_POLL'] = $EndPoll.ToString()
    $start.EnvironmentVariables['RECOMPONE_VIDEO_WIDTH'] = '640'
    $start.EnvironmentVariables['RECOMPONE_VIDEO_HEIGHT'] = '480'
    $start.EnvironmentVariables['RECOMPONE_EXIT_AFTER_VIDEO_CAPTURE'] = '1'
    $start.EnvironmentVariables['RECOMPONE_EXIT_AFTER_INPUT_POLL'] =
        ($EndPoll + 100).ToString()

    $process = [Diagnostics.Process]::Start($start)
    $stdout = $process.StandardOutput.ReadToEndAsync()
    $stderr = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit(600000)) {
        $process.Kill($true)
        $process.WaitForExit()
        throw "$Feature $setting motion capture timed out"
    }

    $stdoutText = $stdout.Result
    $stderrText = $stderr.Result
    [IO.File]::WriteAllText(
        (Join-Path $artifact "$featureSlug-$settingSlug.stdout.log"),
        $stdoutText)
    [IO.File]::WriteAllText(
        (Join-Path $artifact "$featureSlug-$settingSlug.stderr.log"),
        $stderrText)

    if ($process.ExitCode -ne 0) {
        throw "$Feature $setting motion capture exited with code $($process.ExitCode)"
    }
    if ($stderrText -notmatch 'headless audio backend=dummy' -or
        $stderrText -notmatch 'SDL audio ready: driver=dummy') {
        throw "$Feature $setting motion capture did not prove dummy audio"
    }
    if ($stderrText -notmatch 'video capture complete.*ffmpeg exit=0') {
        throw "$Feature $setting video encoder did not finish successfully"
    }
    if (-not (Test-Path -LiteralPath $video)) {
        throw "$Feature $setting did not create $video"
    }
    $videoBytes = (Get-Item -LiteralPath $video).Length
    if ($videoBytes -gt 25MB) {
        throw "$Feature $setting video exceeds the 25 MiB safety limit: $videoBytes"
    }
    Write-Output "renderer=$Feature setting=$setting video=$video bytes=$videoBytes audio=dummy"
}

$settingsHashAfter = (Get-FileHash -LiteralPath $settings -Algorithm SHA256).Hash
if ($settingsHashAfter -ne $settingsHashBefore) {
    throw 'Motion capture changed persistent wrapper settings'
}

Write-Output "artifact=$artifact settings_unchanged=$settingsHashAfter"
