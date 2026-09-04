param(
    [string]$LoosePath = 'work\gt2-cerbera-native-newcar-final-smoke'
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$exe = Join-Path $repo (
    'tools\unified-host\bin\Release\net10.0\GranTurismo2PC.exe')
$looseRoot = if ([IO.Path]::IsPathRooted($LoosePath)) {
    (Resolve-Path $LoosePath).Path
} else {
    (Resolve-Path (Join-Path $repo $LoosePath)).Path
}

if (-not (Test-Path -LiteralPath $exe -PathType Leaf)) {
    throw "Drive-test executable is missing: $exe"
}
foreach ($required in @(
    'GT2.VOL',
    'MUSIC.DAT',
    'manifests\simulation.json',
    'manifests\arcade.json'
)) {
    $path = Join-Path $looseRoot $required
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Drive-test data is incomplete: $path"
    }
}
$environment = [ordered]@{
    # Interactive drive tests never attach fixture or AI input. Starting the
    # Arcade guest at overlay 2 skips its duplicate disc title presentation but
    # leaves every native Arcade Mode menu and race choice under player control.
    'RECOMPONE_INPUT_FILE' = $null
    'RECOMPONE_INPUT_END_POLL' = $null
    'RECOMPONE_LIVE_INPUT_START_POLL' = $null
    'RECOMPONE_DISABLE_LIVE_INPUT' = $null
    'RECOMPONE_GT2_AI_AUTODRIVE' = $null
    'RECOMPONE_GT2_AI_AUTODRIVE_MAX_ENGAGEMENTS' = $null
    'RECOMPONE_EXIT_AFTER_INPUT_POLL' = $null
    'RECOMPONE_UNTHROTTLED' = $null
    'RECOMPONE_THROTTLE_AFTER_INPUT_POLL' = $null
    'RECOMPONE_WINDOW_VISIBLE' = $null
    'RECOMPONE_MUTE' = $null
    'RECOMPONE_SUPPRESS_RUMBLE' = $null
    # Per-poll pad tracing writes thousands of console lines during a race and
    # materially distorts interactive frame-pacing measurements.
    'RECOMPONE_TRACE_INPUT' = $null
    'RECOMPONE_TRACE_PERFORMANCE' = '1'
    'SDL_AUDIODRIVER' = $null
}
$previous = @{}

Write-Host ''
Write-Host 'Gran Turismo 2 manual drive test' -ForegroundColor Cyan
Write-Host 'Native Arcade frontend. No navigation fixture or AI driver is attached.'
Write-Host 'Every menu and race input comes from your controller or keyboard.'
Write-Host ''
Write-Host 'Xbox controller: A = accelerate/confirm, X = brake, left stick/D-pad = steer'
Write-Host 'Keyboard: Z = accelerate/confirm, A = brake, arrows = steer, Enter = Start'
Write-Host 'F1 toggles the wrapper menu. F11 toggles fullscreen.'
Write-Host ''

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
    $launchArguments = @($looseRoot, '--start-arcade')
    & $exe @launchArguments
    if ($LASTEXITCODE -ne 0) {
        throw "GranTurismo2PC exited with code $LASTEXITCODE"
    }
} finally {
    foreach ($entry in $previous.GetEnumerator()) {
        [Environment]::SetEnvironmentVariable(
            $entry.Key,
            $entry.Value,
            [EnvironmentVariableTarget]::Process)
    }
}
