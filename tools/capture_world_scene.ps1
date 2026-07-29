param(
    [string]$Fixture = 'tests\fixtures\ai-autodrive-save-sunday-race.input',
    [int]$CapturePoll = 10000,
    [int]$ExitPoll = 10300,
    [string]$ArtifactName = 'native-world-scene',
    [string]$LoosePath,
    [switch]$AiAutoDrive
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$artifact = Join-Path $repo "artifacts\$ArtifactName"
New-Item -ItemType Directory -Path $artifact -Force | Out-Null
$capture = Join-Path $artifact 'race-frame.ogtwcap'
$objectFile = Join-Path $artifact 'race-frame-world.obj'
$inspectLog = Join-Path $artifact 'world-inspect.log'
$inspector = Join-Path $repo (
    'build\native\Release\opengt_world_capture_inspect.exe')
if (-not (Test-Path -LiteralPath $inspector)) {
    throw "Native world-capture inspector is missing: $inspector"
}
foreach ($path in @($capture, "$capture.tmp", $objectFile, $inspectLog)) {
    if (Test-Path -LiteralPath $path) {
        Remove-Item -LiteralPath $path -Force
    }
}

$worldPathBefore = [Environment]::GetEnvironmentVariable(
    'RECOMPONE_WORLD_CAPTURE_PATH',
    [EnvironmentVariableTarget]::Process)
$worldPollBefore = [Environment]::GetEnvironmentVariable(
    'RECOMPONE_WORLD_CAPTURE_INPUT_POLL',
    [EnvironmentVariableTarget]::Process)
try {
    [Environment]::SetEnvironmentVariable(
        'RECOMPONE_WORLD_CAPTURE_PATH',
        $capture,
        [EnvironmentVariableTarget]::Process)
    [Environment]::SetEnvironmentVariable(
        'RECOMPONE_WORLD_CAPTURE_INPUT_POLL',
        $CapturePoll.ToString(),
        [EnvironmentVariableTarget]::Process)
    $arguments = @{
        Fixture = $Fixture
        CapturePoll = $CapturePoll
        ExitPoll = $ExitPoll
        ArtifactName = $ArtifactName
    }
    if (-not [string]::IsNullOrWhiteSpace($LoosePath)) {
        $arguments['LoosePath'] = $LoosePath
    }
    if ($AiAutoDrive) {
        $arguments['AiAutoDrive'] = $true
    }
    & (Join-Path $PSScriptRoot 'capture_projected_scene.ps1') @arguments
} finally {
    [Environment]::SetEnvironmentVariable(
        'RECOMPONE_WORLD_CAPTURE_PATH',
        $worldPathBefore,
        [EnvironmentVariableTarget]::Process)
    [Environment]::SetEnvironmentVariable(
        'RECOMPONE_WORLD_CAPTURE_INPUT_POLL',
        $worldPollBefore,
        [EnvironmentVariableTarget]::Process)
}

if (-not (Test-Path -LiteralPath $capture)) {
    throw "World capture was not created: $capture"
}
$captureLength = (Get-Item -LiteralPath $capture).Length
if ($captureLength -lt 1MB -or $captureLength -gt 48MB) {
    throw "World capture size is outside the bounded range: $captureLength"
}
$stderr = Get-Content -LiteralPath (
    Join-Path $artifact 'stderr.log') -Raw
if (
    $stderr -notmatch '\[World-Capture\] complete' -or
    $stderr -notmatch '\[World-Capture\].*truncated=False'
) {
    throw 'Runtime did not report a complete, untruncated world capture'
}

$inspection = & $inspector $capture $objectFile 2>&1
if ($LASTEXITCODE -ne 0) {
    throw "Native world validation failed: $inspection"
}
$inspection | Set-Content -LiteralPath $inspectLog
if (-not (Test-Path -LiteralPath $objectFile)) {
    throw 'Native world validation did not create an OBJ'
}

Write-Output (
    "world_capture=$capture bytes=$captureLength " +
    "audio=dummy settings_unchanged=true")
Write-Output "world_validation=$inspection"
