param(
    [string]$LoosePath = 'work\arcade-unified',
    [int]$CaptureFrame = 1
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$dll = Join-Path $repo (
    'generated\arcade-recompiled\bin\Release\net10.0\GranTurismo2ArcadePC.dll')
$runtimeDirectory = Split-Path -Parent $dll
$nativeRenderer = Join-Path $repo 'build\native\Release\opengt_live_renderer.dll'
$looseRoot = if ([IO.Path]::IsPathRooted($LoosePath)) {
    [IO.Path]::GetFullPath($LoosePath)
} else {
    [IO.Path]::GetFullPath((Join-Path $repo $LoosePath))
}

foreach ($required in @(
        $dll,
        $nativeRenderer,
        (Join-Path $looseRoot 'recompone.loose.json'),
        (Join-Path $looseRoot 'SCUS_944.55'),
        (Join-Path $looseRoot 'GT2.OVL'),
        (Join-Path $looseRoot 'GT2.VOL'))) {
    if (-not (Test-Path -LiteralPath $required -PathType Leaf)) {
        throw "Arcade smoke-test input is missing: $required"
    }
}

Copy-Item -LiteralPath $nativeRenderer -Destination $runtimeDirectory -Force
$capturePattern = 'recompone_present_frame_{0:D6}_*.ppm' -f $CaptureFrame
foreach ($oldCapture in Get-ChildItem -LiteralPath $runtimeDirectory `
        -Filter $capturePattern -ErrorAction SilentlyContinue) {
    Remove-Item -LiteralPath $oldCapture.FullName -Force
}

$start = [Diagnostics.ProcessStartInfo]::new()
$start.FileName = 'dotnet'
$start.WorkingDirectory = $runtimeDirectory
$start.Arguments = "`"$dll`" --headless `"$looseRoot`""
$start.UseShellExecute = $false
$start.CreateNoWindow = $true
$start.RedirectStandardOutput = $true
$start.RedirectStandardError = $true
$start.EnvironmentVariables['SDL_AUDIODRIVER'] = 'dummy'
$start.EnvironmentVariables['RECOMPONE_DISABLE_LIVE_INPUT'] = '1'
$start.EnvironmentVariables['RECOMPONE_SUPPRESS_RUMBLE'] = '1'
$start.EnvironmentVariables['RECOMPONE_PRESENTATION_CAPTURE'] = '1'
$start.EnvironmentVariables['RECOMPONE_PRESENTATION_CAPTURE_FRAME'] =
    $CaptureFrame.ToString()
$start.EnvironmentVariables['RECOMPONE_EXIT_AFTER_PRESENTATION_CAPTURE'] = '1'

$process = [Diagnostics.Process]::Start($start)
$stdoutTask = $process.StandardOutput.ReadToEndAsync()
$stderrTask = $process.StandardError.ReadToEndAsync()
if (-not $process.WaitForExit(30000)) {
    throw 'Arcade boot smoke did not reach its first presentation in 30 seconds'
}
$stdout = $stdoutTask.Result
$stderr = $stderrTask.Result
$capture = Get-ChildItem -LiteralPath $runtimeDirectory `
    -Filter $capturePattern -ErrorAction SilentlyContinue |
    Select-Object -First 1
if ($process.ExitCode -ne 0) {
    throw "Arcade boot smoke exited $($process.ExitCode):`n$stderr"
}
if ($stderr -match 'unmapped call|Unhandled exception') {
    throw "Arcade boot smoke reported a runtime failure:`n$stderr"
}
if ($stdout -notmatch '\[GPU\] display=True') {
    throw "Arcade boot smoke did not enable the original display:`n$stdout"
}
if ($stderr -notmatch '\[Native-World\] enabled') {
    throw "Arcade boot smoke did not enable the native world renderer:`n$stderr"
}
if (-not $capture) {
    throw "Arcade boot smoke did not capture presentation frame $CaptureFrame"
}

Write-Output (
    "Arcade native boot passed: capture=$($capture.FullName) " +
    "bytes=$($capture.Length) presentation_frame=$CaptureFrame")
