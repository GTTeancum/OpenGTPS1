param(
    [int]$CapturePoll = 10000,
    [string]$ArtifactName = 'graphics-presets-paired-current',
    [string]$Fixture = 'tests\fixtures\ai-autodrive-save-sunday-race.input'
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$deploy = Join-Path $repo 'OpenGTPS1'
$exe = Join-Path $deploy 'GranTurismo2PC.exe'
$fixture = if ([IO.Path]::IsPathRooted($Fixture)) {
    $Fixture
} else {
    Join-Path $repo $Fixture
}
$artifact = Join-Path $repo "artifacts\$ArtifactName"
$capture = Join-Path $deploy "recompone_capture__$CapturePoll.ppm"

if (-not (Test-Path -LiteralPath $exe)) {
    throw "Packaged executable is missing: $exe"
}
if (-not (Test-Path -LiteralPath $fixture)) {
    throw "Input fixture is missing: $fixture"
}
New-Item -ItemType Directory -Path $artifact -Force | Out-Null

$interfacePath = Join-Path $deploy 'interface.ini'
$settingsPath = Join-Path $deploy 'settings.json'
$interfaceHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $interfacePath).Hash
$settingsHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $settingsPath).Hash

foreach ($preset in @('PS1 Quality', 'Enhanced')) {
    $slug = $preset.Replace(' ', '-')
    if (Test-Path -LiteralPath $capture) {
        Remove-Item -LiteralPath $capture -Force
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
    $start.EnvironmentVariables['RECOMPONE_GT2_AI_AUTODRIVE'] = '1'
    $start.EnvironmentVariables['RECOMPONE_TRACE_GPU_PRIMITIVES'] = '1'
    $start.EnvironmentVariables['RECOMPONE_GRAPHICS_PRESET_OVERRIDE'] = $preset
    $start.EnvironmentVariables['RECOMPONE_EXIT_AFTER_INPUT_POLL'] =
        ($CapturePoll + 50).ToString()

    $process = [Diagnostics.Process]::Start($start)
    $stdout = $process.StandardOutput.ReadToEndAsync()
    $stderr = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit(600000)) {
        $process.Kill($true)
        $process.WaitForExit()
        throw "$preset capture timed out"
    }

    $stdoutPath = Join-Path $artifact "$slug.stdout.log"
    $stderrPath = Join-Path $artifact "$slug.stderr.log"
    [IO.File]::WriteAllText($stdoutPath, $stdout.Result)
    [IO.File]::WriteAllText($stderrPath, $stderr.Result)
    if ($process.ExitCode -ne 0) {
        throw "$preset capture exited with code $($process.ExitCode)"
    }
    if ($stderr.Result -notmatch 'headless audio backend=dummy' -or
        $stderr.Result -notmatch 'SDL audio ready: driver=dummy') {
        throw "$preset capture did not prove the dummy audio backend"
    }
    if ($stderr.Result -match 'Unhandled exception|Fatal error') {
        throw "$preset capture logged a runtime failure"
    }
    if (-not (Test-Path -LiteralPath $capture)) {
        throw "$preset did not produce the expected capture: $capture"
    }

    $ppmPath = Join-Path $artifact "$slug-$CapturePoll.ppm"
    Copy-Item -LiteralPath $capture -Destination $ppmPath -Force
    $ffmpeg = (Get-Command ffmpeg -ErrorAction SilentlyContinue).Source
    if ($ffmpeg) {
        & $ffmpeg -hide_banner -loglevel error -y -i $ppmPath `
            (Join-Path $artifact "$slug-$CapturePoll.png")
        if ($LASTEXITCODE -ne 0) {
            throw "ffmpeg failed to convert the $preset capture"
        }
    }
}

if ((Get-FileHash -Algorithm SHA256 -LiteralPath $interfacePath).Hash -ne
        $interfaceHash) {
    throw 'Transient preset captures changed interface.ini'
}
if ((Get-FileHash -Algorithm SHA256 -LiteralPath $settingsPath).Hash -ne
        $settingsHash) {
    throw 'Transient preset captures changed settings.json'
}

Write-Output "paired captures=$artifact"
Write-Output 'audio safety=dummy backend proven for both runs'
Write-Output 'persistent settings=unchanged'
