param(
    [string]$Fixture = 'tests\fixtures\ai-autodrive-save-sunday-race.input',
    [int]$CapturePoll = 10000,
    [int]$ExitPoll = 10300,
    [string]$ArtifactName = 'native-projected-scene',
    [string]$LoosePath,
    [switch]$AiAutoDrive
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$fixtureCandidate = if ([IO.Path]::IsPathRooted($Fixture)) {
    $Fixture
} else {
    Join-Path $repo $Fixture
}
$fixturePath = (Resolve-Path $fixtureCandidate).Path
$dll = Join-Path $repo (
    'generated\recompiled\bin\Release\net10.0\GranTurismo2PC.dll')
$renderer = Join-Path $repo (
    'build\native\Release\opengt_capture_render.exe')
if (-not (Test-Path -LiteralPath $dll)) {
    throw "Development build is missing: $dll"
}
if (-not (Test-Path -LiteralPath $renderer)) {
    throw "Native capture renderer is missing: $renderer"
}
if ([string]::IsNullOrWhiteSpace($LoosePath)) {
    $LoosePath = Join-Path $repo 'OpenGTPS1'
}
$LoosePath = if ([IO.Path]::IsPathRooted($LoosePath)) {
    [IO.Path]::GetFullPath($LoosePath)
} else {
    [IO.Path]::GetFullPath((Join-Path $repo $LoosePath))
}
if (-not (Test-Path -LiteralPath (
        Join-Path $LoosePath 'recompone.loose.json'))) {
    throw "Loose runtime is missing: $LoosePath"
}
if ($ExitPoll -le $CapturePoll) {
    throw 'ExitPoll must be greater than CapturePoll'
}

$artifact = Join-Path $repo "artifacts\$ArtifactName"
New-Item -ItemType Directory -Path $artifact -Force | Out-Null
$capture = Join-Path $artifact 'race-frame.ogtcap'
$perspectiveImage = Join-Path $artifact 'race-frame-perspective.png'
$affineImage = Join-Path $artifact 'race-frame-affine.png'
$referenceImage = Join-Path $artifact 'race-frame-vram-reference.png'
$generatedFiles = @(
    $capture,
    "$capture.tmp",
    $perspectiveImage,
    $affineImage,
    $referenceImage,
    (Join-Path $artifact 'race-frame-perspective.qoi'),
    (Join-Path $artifact 'race-frame-affine.qoi'),
    (Join-Path $artifact 'stdout.log'),
    (Join-Path $artifact 'stderr.log'),
    (Join-Path $artifact 'render-perspective.log'),
    (Join-Path $artifact 'render-affine.log'),
    (Join-Path $artifact 'render-vram-reference.log')
)
foreach ($path in $generatedFiles) {
    if (Test-Path -LiteralPath $path) {
        Remove-Item -LiteralPath $path -Force
    }
}

$runtimeDirectory = Split-Path -Parent $dll
foreach ($runtimeFile in @(
        'carda.sav',
        'cardb.sav',
        'interface.ini',
        'settings.json')) {
    $source = Join-Path $LoosePath $runtimeFile
    $destination = Join-Path $runtimeDirectory $runtimeFile
    if (
        (Test-Path -LiteralPath $source) -and
        -not (Test-Path -LiteralPath $destination)
    ) {
        Copy-Item -LiteralPath $source -Destination $destination
    }
}

$settings = Join-Path $runtimeDirectory 'interface.ini'
$settingsHashBefore = if (Test-Path -LiteralPath $settings) {
    (Get-FileHash -LiteralPath $settings -Algorithm SHA256).Hash
} else {
    $null
}

$start = New-Object Diagnostics.ProcessStartInfo
$start.FileName = 'dotnet'
$start.WorkingDirectory = Split-Path -Parent $dll
$start.Arguments = "`"$dll`" --headless `"$LoosePath`""
$start.UseShellExecute = $false
$start.CreateNoWindow = $true
$start.RedirectStandardOutput = $true
$start.RedirectStandardError = $true

$launchEnvironment = [ordered]@{
    # This is intentionally redundant with --headless. Automated capture must
    # never be able to open a physical audio backend.
    'SDL_AUDIODRIVER' = 'dummy'
    'RECOMPONE_INPUT_FILE' = $fixturePath
    'RECOMPONE_DISABLE_LIVE_INPUT' = '1'
    'RECOMPONE_SUPPRESS_RUMBLE' = '1'
    'RECOMPONE_UNTHROTTLED' = '1'
    'RECOMPONE_GRAPHICS_PRESET_OVERRIDE' = 'Enhanced'
    'RECOMPONE_EXIT_AFTER_INPUT_POLL' = $ExitPoll.ToString()
    'RECOMPONE_GPU_CAPTURE_PATH' = $capture
    'RECOMPONE_GPU_CAPTURE_INPUT_POLL' = $CapturePoll.ToString()
    'RECOMPONE_GT2_AI_AUTODRIVE' = if ($AiAutoDrive) { '1' } else { $null }
}
$originalEnvironment = @{}
try {
    foreach ($entry in $launchEnvironment.GetEnumerator()) {
        $originalEnvironment[$entry.Key] = [Environment]::GetEnvironmentVariable(
            $entry.Key,
            [EnvironmentVariableTarget]::Process)
        [Environment]::SetEnvironmentVariable(
            $entry.Key,
            $entry.Value,
            [EnvironmentVariableTarget]::Process)
    }
    $process = [Diagnostics.Process]::Start($start)
} finally {
    foreach ($entry in $originalEnvironment.GetEnumerator()) {
        [Environment]::SetEnvironmentVariable(
            $entry.Key,
            $entry.Value,
            [EnvironmentVariableTarget]::Process)
    }
}
$stdoutTask = $process.StandardOutput.ReadToEndAsync()
$stderrTask = $process.StandardError.ReadToEndAsync()
if (-not $process.WaitForExit(600000)) {
    $process.Kill($true)
    $process.WaitForExit()
    throw 'Projected-scene capture timed out'
}
$stdout = $stdoutTask.Result
$stderr = $stderrTask.Result
[IO.File]::WriteAllText((Join-Path $artifact 'stdout.log'), $stdout)
[IO.File]::WriteAllText((Join-Path $artifact 'stderr.log'), $stderr)

if ($process.ExitCode -ne 0) {
    throw "Projected-scene capture exited with code $($process.ExitCode)"
}
if (
    $stderr -notmatch 'headless audio backend=dummy' -or
    $stderr -notmatch 'SDL audio ready: driver=dummy'
) {
    throw 'Capture did not prove both dummy-audio safety checks'
}
if ($stderr -notmatch '\[GPU-Capture\] complete') {
    throw 'Runtime did not report a completed projected-scene capture'
}
if (-not (Test-Path -LiteralPath $capture)) {
    throw "Runtime did not create capture: $capture"
}
$captureLength = (Get-Item -LiteralPath $capture).Length
if ($captureLength -lt 1MB -or $captureLength -gt 32MB) {
    throw "Capture size is outside the bounded range: $captureLength bytes"
}

$perspectiveLog = & $renderer $capture $perspectiveImage 2>&1
if ($LASTEXITCODE -ne 0) {
    throw "Native perspective render failed: $perspectiveLog"
}
$perspectiveLog | Set-Content -LiteralPath (
    Join-Path $artifact 'render-perspective.log')

$affineLog = & $renderer $capture $affineImage --affine 2>&1
if ($LASTEXITCODE -ne 0) {
    throw "Native affine render failed: $affineLog"
}
$affineLog | Set-Content -LiteralPath (
    Join-Path $artifact 'render-affine.log')

$referenceLog = & $renderer $capture $referenceImage --vram 2>&1
if ($LASTEXITCODE -ne 0) {
    throw "Native VRAM-reference extraction failed: $referenceLog"
}
$referenceLog | Set-Content -LiteralPath (
    Join-Path $artifact 'render-vram-reference.log')

$settingsHashAfter = if (Test-Path -LiteralPath $settings) {
    (Get-FileHash -LiteralPath $settings -Algorithm SHA256).Hash
} else {
    $null
}
if ($settingsHashAfter -ne $settingsHashBefore) {
    throw 'Capture changed persistent wrapper settings'
}

Write-Output (
    "capture=$capture bytes=$captureLength poll=$CapturePoll " +
    "audio=dummy settings_unchanged=true")
Write-Output "perspective=$perspectiveLog"
Write-Output "affine=$affineLog"
Write-Output "vram_reference=$referenceLog"
