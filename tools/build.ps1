param(
    [switch]$Regenerate
)

$ErrorActionPreference = 'Stop'

$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
Push-Location $repo
try {
    if ($Regenerate) {
        Write-Host 'Regenerating recompiled sources from the archival disc input...'
        python tools\prepare_reference.py
        if ($LASTEXITCODE -ne 0) { throw "prepare_reference.py failed: $LASTEXITCODE" }

        dotnet run --project vendor\RecompOne\RecompOne.Recompiler -c Release -- `
            generated\gt2.recompone.json
        if ($LASTEXITCODE -ne 0) { throw "RecompOne failed: $LASTEXITCODE" }

        python tools\apply_gt2_enhancements.py
        if ($LASTEXITCODE -ne 0) { throw "apply_gt2_enhancements.py failed: $LASTEXITCODE" }
    }
    elseif (-not (Test-Path -LiteralPath 'generated\recompiled\GranTurismo2PC.csproj')) {
        throw 'Generated sources are missing. Use tools\build.ps1 -Regenerate with the archival source available.'
    }

    dotnet build generated\recompiled\GranTurismo2PC.csproj -c Release
    if ($LASTEXITCODE -ne 0) { throw "GT2 build failed: $LASTEXITCODE" }

    python tools\prepare_loose_install.py
    if ($LASTEXITCODE -ne 0) { throw "Loose-file preparation failed: $LASTEXITCODE" }

    $install = Join-Path $repo 'OpenGTPS1'
    dotnet publish generated\recompiled\GranTurismo2PC.csproj -c Release `
        -r win-x64 --self-contained true -p:PublishSingleFile=true `
        -p:DebugType=None -p:DebugSymbols=false `
        -o $install
    if ($LASTEXITCODE -ne 0) { throw "GT2 publish failed: $LASTEXITCODE" }

    # Convenience cards and the developer's settings file are intentionally
    # excluded from the public source tree. Seed them when present locally,
    # but allow a clean clone to create fresh runtime state.
    $optionalSeeds = @{
        'carda.sav'    = 'artifacts\GT2_All_Licenses_100k.sav'
        'cardb.sav'    = 'artifacts\carda.blank.sav'
        'settings.json' = 'settings.json'
    }
    foreach ($name in $optionalSeeds.Keys) {
        $source = Join-Path $repo $optionalSeeds[$name]
        $destination = Join-Path $install $name
        if (
            -not (Test-Path -LiteralPath $destination) -and
            (Test-Path -LiteralPath $source)
        ) {
            Copy-Item -LiteralPath $source -Destination $destination
        }
    }

    $interfaceDestination = Join-Path $install 'interface.ini'
    if (-not (Test-Path -LiteralPath $interfaceDestination)) {
        Copy-Item -LiteralPath (Join-Path $repo 'interface.ini') `
            -Destination $interfaceDestination
    }
}
finally {
    Pop-Location
}
