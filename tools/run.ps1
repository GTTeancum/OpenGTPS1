param(
    [string]$LoosePath,
    [switch]$Build,
    [switch]$Headless
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$defaultLoose = Join-Path $repo 'OpenGTPS1'
$dll = Join-Path $repo 'generated\recompiled\bin\Release\net10.0\GranTurismo2PC.dll'

if ($Build -or -not (Test-Path -LiteralPath $dll)) {
    & (Join-Path $PSScriptRoot 'build.ps1')
    if ($LASTEXITCODE -ne 0) {
        throw "GT2 build failed: $LASTEXITCODE"
    }
}

if ([string]::IsNullOrWhiteSpace($LoosePath)) {
    $LoosePath = $defaultLoose
}
$LoosePath = [IO.Path]::GetFullPath($LoosePath, $repo)
if (-not (Test-Path -LiteralPath (Join-Path $LoosePath 'recompone.loose.json'))) {
    python (Join-Path $PSScriptRoot 'prepare_loose_install.py')
    if ($LASTEXITCODE -ne 0) {
        throw "Loose-file preparation failed: $LASTEXITCODE"
    }
}

Push-Location $repo
try {
    $runtimeArgs = @($dll)
    if ($Headless) { $runtimeArgs += '--headless' }
    $runtimeArgs += $LoosePath
    dotnet @runtimeArgs
    if ($LASTEXITCODE -ne 0) {
        throw "GranTurismo2PC exited with code $LASTEXITCODE"
    }
}
finally {
    Pop-Location
}
