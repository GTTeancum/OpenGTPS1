param(
    [Parameter(Mandatory = $true)]
    [string]$Capture,
    [string]$ArtifactName = 'world-renderer-validation'
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$capturePath = (Resolve-Path -LiteralPath $Capture).Path
$viewer = Join-Path $repo (
    'build\native\Release\opengt_world_viewer.exe')
if (-not (Test-Path -LiteralPath $viewer)) {
    throw "Native world viewer is missing: $viewer"
}

$artifact = Join-Path $repo "artifacts\$ArtifactName"
New-Item -ItemType Directory -Path $artifact -Force | Out-Null
$gpu = Join-Path $artifact 'world-gpu-depth.png'
$oracle = Join-Path $artifact 'world-compatibility-oracle.png'
$repeatGpu = Join-Path $artifact 'world-gpu-depth-repeat.png'
$repeatOracle = Join-Path $artifact 'world-compatibility-oracle-repeat.png'
$log = Join-Path $artifact 'validation.log'

$first = & $viewer $capturePath $gpu --warp --oracle $oracle 2>&1
if ($LASTEXITCODE -ne 0) {
    throw "First WARP render failed: $first"
}
$second = & $viewer $capturePath $repeatGpu --warp --oracle $repeatOracle 2>&1
if ($LASTEXITCODE -ne 0) {
    throw "Repeat WARP render failed: $second"
}
$firstText = $first -join [Environment]::NewLine
$secondText = $second -join [Environment]::NewLine
$firstGpu = [regex]::Match($firstText, 'gpuHash=([0-9a-f]{16})').Groups[1].Value
$secondGpu = [regex]::Match($secondText, 'gpuHash=([0-9a-f]{16})').Groups[1].Value
$firstOracle = [regex]::Match(
    $firstText,
    'oracleHash=([0-9a-f]{16})').Groups[1].Value
$secondOracle = [regex]::Match(
    $secondText,
    'oracleHash=([0-9a-f]{16})').Groups[1].Value
if (
    [string]::IsNullOrWhiteSpace($firstGpu) -or
    $firstGpu -ne $secondGpu -or
    [string]::IsNullOrWhiteSpace($firstOracle) -or
    $firstOracle -ne $secondOracle
) {
    throw (
        "Renderer is not deterministic: " +
        "gpu=$firstGpu/$secondGpu oracle=$firstOracle/$secondOracle")
}
if (
    -not (Test-Path -LiteralPath $gpu) -or
    -not (Test-Path -LiteralPath $oracle)
) {
    throw 'Renderer did not produce both bounded PNG artifacts'
}
foreach ($path in @($gpu, $oracle)) {
    $length = (Get-Item -LiteralPath $path).Length
    if ($length -lt 1KB -or $length -gt 2MB) {
        throw "PNG artifact is outside the bounded range: $path ($length)"
    }
}
foreach ($path in @($repeatGpu, $repeatOracle)) {
    if (Test-Path -LiteralPath $path) {
        Remove-Item -LiteralPath $path -Force
    }
}

@(
    $firstText
    "repeatGpuHash=$secondGpu repeatOracleHash=$secondOracle"
    'deterministic=true repeatArtifacts=removed'
) | Set-Content -LiteralPath $log

Write-Output $firstText
Write-Output (
    "deterministic=true gpuHash=$firstGpu oracleHash=$firstOracle " +
    "gpu=$gpu oracle=$oracle log=$log")
