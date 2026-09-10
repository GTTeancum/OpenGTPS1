param(
    [switch]$Regenerate
)

$ErrorActionPreference = 'Stop'
$packageVersion = '0.9.0'

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
    elseif (
        -not (Test-Path -LiteralPath 'generated\recompiled\GranTurismo2PC.csproj') -or
        -not (Test-Path -LiteralPath 'generated\arcade-recompiled\GranTurismo2ArcadePC.csproj')
    ) {
        throw 'Generated Simulation/Arcade sources are missing. Use tools\build.ps1 -Regenerate with the archival sources available.'
    }

    cmake -S native -B build\native
    if ($LASTEXITCODE -ne 0) {
        throw "Native renderer configuration failed: $LASTEXITCODE"
    }
    cmake --build build\native --config Release
    if ($LASTEXITCODE -ne 0) {
        throw "Native renderer build failed: $LASTEXITCODE"
    }

    dotnet build tools\unified-host\GranTurismo2PC.csproj -c Release `
        -p:Version=$packageVersion
    if ($LASTEXITCODE -ne 0) { throw "Unified GT2 build failed: $LASTEXITCODE" }

    $install = Join-Path $repo 'OpenGTPS1'
    $unifiedVolume = Join-Path $install 'GT2.VOL'
    if (-not (Test-Path -LiteralPath $unifiedVolume -PathType Leaf)) {
        throw "Unified GT2.VOL is missing: $unifiedVolume"
    }
    $unifiedVolumeBytes = (Get-Item -LiteralPath $unifiedVolume).Length
    foreach ($variant in @('simulation', 'arcade')) {
        $manifestPath = Join-Path $install "manifests\$variant.json"
        if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
            throw "Unified $variant manifest is missing: $manifestPath"
        }
        $manifest = Get-Content -LiteralPath $manifestPath -Raw |
            ConvertFrom-Json
        $overlayEntries = @($manifest.files | Where-Object path -eq 'GT2.OVL')
        if ($overlayEntries.Count -ne 1) {
            throw "Unified $variant manifest does not contain one GT2.OVL entry"
        }
        $overlaySource = Join-Path $install $overlayEntries[0].source
        if (-not (Test-Path -LiteralPath $overlaySource -PathType Leaf)) {
            throw "Unified $variant GT2.OVL source is missing: $overlaySource"
        }
        $overlayBytes = (Get-Item -LiteralPath $overlaySource).Length
        if ([long]$overlayEntries[0].size -ne $overlayBytes) {
            throw (
                "Unified $variant GT2.OVL manifest is stale: " +
                "$($overlayEntries[0].size) != $overlayBytes. " +
                'Re-run tools\prepare_unified_install.py before publishing.'
            )
        }
        $volumeEntries = @($manifest.files | Where-Object path -eq 'GT2.VOL')
        if ($volumeEntries.Count -ne 1) {
            throw "Unified $variant manifest does not contain one GT2.VOL entry"
        }
        $entry = $volumeEntries[0]
        $sourceOffset = if ($null -eq $entry.sourceOffset) {
            0L
        } else {
            [long]$entry.sourceOffset
        }
        $sourceLength = if ($null -eq $entry.sourceLength) {
            [long]$entry.size
        } else {
            [long]$entry.sourceLength
        }
        if ($sourceOffset + $sourceLength -gt $unifiedVolumeBytes) {
            throw (
                "Unified $variant GT2.VOL range exceeds ${unifiedVolume}: " +
                "$sourceOffset+$sourceLength > $unifiedVolumeBytes. " +
                'Rebuild the combined Simulation/Arcade data with ' +
                'tools\prepare_unified_install.py before publishing.'
            )
        }
    }

    dotnet publish tools\unified-host\GranTurismo2PC.csproj -c Release `
        -r win-x64 --self-contained true -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -p:PublishReadyToRun=true `
        -p:Version=$packageVersion `
        -p:DebugType=None -p:DebugSymbols=false `
        -o $install
    if ($LASTEXITCODE -ne 0) { throw "Unified GT2 publish failed: $LASTEXITCODE" }

    # A single-file publish does not overwrite framework-dependent sidecars
    # left by an older deployment. Remove only top-level runtime DLLs and the
    # app-specific sidecars that are now embedded in GranTurismo2PC.exe.
    foreach ($legacyDll in Get-ChildItem -LiteralPath $install `
            -Filter '*.dll' -File -ErrorAction SilentlyContinue) {
        Remove-Item -LiteralPath $legacyDll.FullName -Force
    }
    foreach ($legacyName in @(
            'GranTurismo2PC.deps.json',
            'GranTurismo2PC.dll',
            'GranTurismo2PC.pdb',
            'GranTurismo2PC.runtimeconfig.json',
            'RecompOne.Runtime.dll',
            'RecompOne.Runtime.pdb')) {
        $legacyPath = Join-Path $install $legacyName
        if (Test-Path -LiteralPath $legacyPath -PathType Leaf) {
            Remove-Item -LiteralPath $legacyPath -Force
        }
    }

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
