param(
    [int]$ExitPoll = 10050,
    [int]$TimeoutSeconds = 180,
    [string]$ArtifactName = 'live-native-smoke',
    [string]$Fixture =
        'tests\fixtures\ai-autodrive-save-sunday-race.input',
    [string]$DeployPath =
        'C:\Programming\GitHub\OpenGTPS1\OpenGTPS1',
    [ValidateSet('Enhanced')]
    [string]$GraphicsPreset = 'Enhanced',
    [string]$VideoCapture = '',
    [int]$VideoStartPoll = 0,
    [int]$VideoEndPoll = 0,
    [ValidateSet(30, 60)]
    [int]$VideoFps = 30,
    [switch]$CapturePresentation,
    [switch]$DumpNativeCapture,
    [switch]$Unthrottled
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$exe = Join-Path $DeployPath 'GranTurismo2PC.exe'
$fixturePath = if ([IO.Path]::IsPathRooted($Fixture)) {
    $Fixture
} else {
    Join-Path $repo $Fixture
}
if (-not (Test-Path -LiteralPath $exe)) {
    throw "Packaged executable is missing: $exe"
}
if (-not (Test-Path -LiteralPath $fixturePath)) {
    throw "Input fixture is missing: $fixturePath"
}
if (
    -not [string]::IsNullOrWhiteSpace($VideoCapture) -and
    $VideoEndPoll -le $VideoStartPoll
) {
    throw 'VideoEndPoll must be greater than VideoStartPoll'
}

$artifact = Join-Path $repo "artifacts\$ArtifactName"
New-Item -ItemType Directory -Path $artifact -Force | Out-Null
$stdoutPath = Join-Path $artifact 'stdout.log'
$stderrPath = Join-Path $artifact 'stderr.log'
$nativeDumpPath = Join-Path $artifact 'first-live-frame.ogtwcap'
$videoCapturePath = if ([string]::IsNullOrWhiteSpace($VideoCapture)) {
    $null
} elseif ([IO.Path]::IsPathRooted($VideoCapture)) {
    $VideoCapture
} else {
    Join-Path $artifact $VideoCapture
}
foreach ($path in @($stdoutPath, $stderrPath)) {
    if (Test-Path -LiteralPath $path) {
        Remove-Item -LiteralPath $path -Force
    }
}

$environment = [ordered]@{
    # This harness is intentionally incapable of opening physical audio.
    'SDL_AUDIODRIVER' = 'dummy'
    'RECOMPONE_INPUT_FILE' = $fixturePath
    'RECOMPONE_DISABLE_LIVE_INPUT' = '1'
    'RECOMPONE_SUPPRESS_RUMBLE' = '1'
    'RECOMPONE_GT2_AI_AUTODRIVE' = '1'
    'RECOMPONE_GRAPHICS_PRESET_OVERRIDE' = $GraphicsPreset
    'RECOMPONE_EXIT_AFTER_INPUT_POLL' = $ExitPoll.ToString()
    'RECOMPONE_NATIVE_WORLD_TRACE_INTERVAL' = '30'
    'RECOMPONE_VIDEO_CAPTURE' = $videoCapturePath
    'RECOMPONE_VIDEO_START_INPUT_POLL' =
        if ($videoCapturePath) { $VideoStartPoll.ToString() } else { $null }
    'RECOMPONE_VIDEO_END_INPUT_POLL' =
        if ($videoCapturePath) { $VideoEndPoll.ToString() } else { $null }
    'RECOMPONE_VIDEO_WIDTH' = if ($videoCapturePath) { '640' } else { $null }
    'RECOMPONE_VIDEO_HEIGHT' = if ($videoCapturePath) { '480' } else { $null }
    'RECOMPONE_VIDEO_FPS' = if ($videoCapturePath) { $VideoFps.ToString() } else { $null }
    'RECOMPONE_PRESENTATION_CAPTURE' =
        if ($CapturePresentation) { '1' } else { $null }
    'RECOMPONE_NATIVE_WORLD_DUMP_PATH' =
        if ($DumpNativeCapture) { $nativeDumpPath } else { $null }
    'RECOMPONE_UNTHROTTLED' = if ($Unthrottled) { '1' } else { $null }
}
$previous = @{}
try {
    foreach ($entry in $environment.GetEnumerator()) {
        $previous[$entry.Key] = [Environment]::GetEnvironmentVariable(
            $entry.Key,
            [EnvironmentVariableTarget]::Process)
        [Environment]::SetEnvironmentVariable(
            $entry.Key,
            $entry.Value,
            [EnvironmentVariableTarget]::Process)
    }
    # Some managed launch environments expose both Windows' conventional
    # "Path" spelling and a duplicate uppercase "PATH". Start-Process builds
    # a case-insensitive environment dictionary and rejects that duplicate.
    $pathKeys = @(
        [Environment]::GetEnvironmentVariables().Keys |
            Where-Object {
                $_.ToString().ToLowerInvariant() -eq 'path'
            }
    )
    if ($pathKeys.Count -gt 1 -and ($pathKeys -ccontains 'PATH')) {
        [Environment]::SetEnvironmentVariable(
            'PATH',
            $null,
            [EnvironmentVariableTarget]::Process)
    }
    $process = Start-Process `
        -FilePath $exe `
        -WorkingDirectory $DeployPath `
        -ArgumentList '--headless' `
        -WindowStyle Hidden `
        -RedirectStandardOutput $stdoutPath `
        -RedirectStandardError $stderrPath `
        -PassThru
} finally {
    foreach ($entry in $previous.GetEnumerator()) {
        [Environment]::SetEnvironmentVariable(
            $entry.Key,
            $entry.Value,
            [EnvironmentVariableTarget]::Process)
    }
}

$deadline = [DateTime]::UtcNow.AddSeconds($TimeoutSeconds)
$completed = $false
while ([DateTime]::UtcNow -lt $deadline) {
    if (-not (Get-Process -Id $process.Id -ErrorAction SilentlyContinue)) {
        $completed = $true
        break
    }
    Start-Sleep -Milliseconds 250
}
if (-not $completed) {
    Stop-Process -Id $process.Id -Force
}
$stderr = Get-Content -LiteralPath $stderrPath -Raw
if (
    $stderr -notmatch 'headless audio backend=dummy' -or
    $stderr -notmatch 'SDL audio ready: driver=dummy'
) {
    throw 'Live-native smoke did not prove both dummy-audio checks'
}
if (-not $completed) {
    throw "Live-native smoke exceeded ${TimeoutSeconds}s; logs=$artifact"
}
$exitCode = $process.ExitCode
if ($null -ne $exitCode -and $exitCode -ne 0) {
    throw "Live-native smoke exited with code $exitCode"
}
if ($stderr -match 'Unhandled exception|Fatal error') {
    throw 'Live-native smoke logged a fatal runtime failure'
}
if (
    $stderr -notmatch '\[Native-World\] enabled' -or
    $stderr -notmatch '\[Native-World\] frame='
) {
    throw 'Live-native smoke did not prove native rendering'
}
if ($stderr -match '\[Native-World\] disabled:') {
    throw 'Live-native smoke logged a native renderer failure'
}
if ($stderr -notmatch '\[Runtime\] shutdown complete; exit=0') {
    throw 'Live-native smoke did not complete orderly shutdown'
}
if ($videoCapturePath) {
    if ($stderr -match '\[Host\] video capture (failed|finalization failed):') {
        throw 'Live-native smoke logged a video encoder failure'
    }
    if ($stderr -notmatch '\[Host\] video capture complete.*ffmpeg exit=0') {
        throw 'Live-native smoke did not prove successful video finalization'
    }
    if (-not (Test-Path -LiteralPath $videoCapturePath -PathType Leaf)) {
        throw "Live-native smoke did not create video: $videoCapturePath"
    }
}

Write-Output (
    "live_native=pass renderer=native " +
    "preset=$GraphicsPreset " +
    "exitPoll=$ExitPoll " +
    "audio=dummy " +
    "$(if ($videoCapturePath) { "video=$videoCapturePath " } else { '' })" +
    "logs=$artifact")
