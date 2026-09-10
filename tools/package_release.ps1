param(
    [ValidatePattern('^0\.9b$')]
    [string]$Version = '0.9b',
    [string]$ArtifactName = 'release-0.9b'
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$artifact = Join-Path $repo "artifacts\$ArtifactName"
$folderName = "OpenGTPS1-$Version-win-x64"
$stage = Join-Path $artifact $folderName
$archive = Join-Path $artifact "$folderName.zip"
$scratchRoot = Join-Path $repo 'work\release-package-scratch'
$scratchRootFull = [IO.Path]::GetFullPath($scratchRoot).TrimEnd('\')
$nativeBuild = Join-Path $scratchRootFull (
    'OpenGTPS1-release-native-' + [guid]::NewGuid().ToString('N'))
$managedBuild = Join-Path $scratchRootFull (
    'OpenGTPS1-release-managed-' + [guid]::NewGuid().ToString('N'))

if (Test-Path -LiteralPath $artifact) {
    throw "Refusing to overwrite an existing release artifact: $artifact"
}

New-Item -ItemType Directory -Path $stage | Out-Null
New-Item -ItemType Directory -Path $scratchRootFull -Force | Out-Null

try {
    $policyEnvironment = @(
        'OPENGT_RELEASE_POLICY_SENTINEL',
        'RECOMPONE_AUDIT_RELEASE_POLICY_SENTINEL',
        'RECOMPONE_TRACE_RELEASE_POLICY_SENTINEL',
        'RECOMPONE_GT2_RELEASE_POLICY_SENTINEL',
        'RECOMPONE_NATIVE_WORLD_RELEASE_POLICY_SENTINEL',
        'RECOMPONE_DISABLE_LIVE_INPUT'
    )
    $savedPolicyEnvironment = @{}
    foreach ($name in $policyEnvironment) {
        $savedPolicyEnvironment[$name] =
            [Environment]::GetEnvironmentVariable(
                $name,
                [EnvironmentVariableTarget]::Process)
    }
    try {
        foreach ($name in $policyEnvironment[0..4]) {
            [Environment]::SetEnvironmentVariable(
                $name,
                'remove',
                [EnvironmentVariableTarget]::Process)
        }
        [Environment]::SetEnvironmentVariable(
            'RECOMPONE_DISABLE_LIVE_INPUT',
            'keep',
            [EnvironmentVariableTarget]::Process)
        $policyOutput = & dotnet run `
            --project (Join-Path $repo `
                'tools\modern-renderer-config-tests\ModernRendererConfigTests.csproj') `
            -c Release `
            --artifacts-path $managedBuild `
            -p:OpenGTReleasePackage=true `
            -- --verify-release-policy 2>&1
        if ($LASTEXITCODE -ne 0 -or
            ($policyOutput -join "`n") -notmatch
                'release_policy=pass .*compatibility_world_path=absent .*' +
                'native_failure=fail-closed') {
            throw (
                "Release policy verification failed:`n" +
                ($policyOutput -join "`n"))
        }
        $policyOutput | Write-Output
    }
    finally {
        foreach ($name in $policyEnvironment) {
            [Environment]::SetEnvironmentVariable(
                $name,
                $savedPolicyEnvironment[$name],
                [EnvironmentVariableTarget]::Process)
        }
    }

    cmake -S (Join-Path $repo 'native') -B $nativeBuild `
        -DOPENGT_BUILD_TESTS=OFF `
        -DOPENGT_BUILD_DEV_TOOLS=OFF
    if ($LASTEXITCODE -ne 0) {
        throw "Native renderer configuration failed: $LASTEXITCODE"
    }
    cmake --build $nativeBuild --config Release
    if ($LASTEXITCODE -ne 0) {
        throw "Native renderer build failed: $LASTEXITCODE"
    }

    $nativeRenderer = Join-Path $nativeBuild 'Release\opengt_live_renderer.dll'
    if (-not (Test-Path -LiteralPath $nativeRenderer -PathType Leaf)) {
        throw "Native renderer output is missing: $nativeRenderer"
    }

    dotnet publish (
        Join-Path $repo 'tools\unified-host\GranTurismo2PC.csproj') `
        -c Release `
        -r win-x64 `
        --artifacts-path $managedBuild `
        --self-contained true `
        -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -p:PublishReadyToRun=true `
        -p:OpenGTReleasePackage=true `
        "-p:OpenGTNativeLibraryPath=$nativeRenderer" `
        -p:Version=0.9.0 `
        -p:DebugType=None `
        -p:DebugSymbols=false `
        -o $stage
    if ($LASTEXITCODE -ne 0) {
        throw "Release publish failed: $LASTEXITCODE"
    }

    Copy-Item -LiteralPath (Join-Path $repo 'interface.ini') `
        -Destination (Join-Path $stage 'interface.ini')
    $manifestStage = Join-Path $stage 'manifests'
    New-Item -ItemType Directory -Path $manifestStage | Out-Null
    Copy-Item -LiteralPath (Join-Path $repo 'tools\recompone.simulation.unified.json') `
        -Destination (Join-Path $manifestStage 'simulation.json')
    Copy-Item -LiteralPath (Join-Path $repo 'tools\recompone.arcade.unified.json') `
        -Destination (Join-Path $manifestStage 'arcade.json')
    Copy-Item -LiteralPath (Join-Path $repo 'release\README.md') `
        -Destination (Join-Path $stage 'README.md')
    Copy-Item -LiteralPath (
        Join-Path $repo 'release\Setup-From-GT2-Discs.ps1') `
        -Destination (Join-Path $stage 'Setup-From-GT2-Discs.ps1')
    Copy-Item -LiteralPath (Join-Path $repo 'release\RELEASE_NOTES.md') `
        -Destination (Join-Path $stage 'RELEASE_NOTES.md')
    Copy-Item -LiteralPath (Join-Path $repo 'vendor\RecompOne\LICENSE') `
        -Destination (Join-Path $stage 'RECOMPONE-LICENSE.txt')

    $required = @(
        'GranTurismo2PC.exe',
        'interface.ini',
        'manifests\simulation.json',
        'manifests\arcade.json',
        'README.md',
        'RELEASE_NOTES.md',
        'Setup-From-GT2-Discs.ps1',
        'RECOMPONE-LICENSE.txt'
    )
    foreach ($name in $required) {
        if (-not (Test-Path -LiteralPath (Join-Path $stage $name) -PathType Leaf)) {
            throw "Release package is missing required file: $name"
        }
    }
    $looseLibraries = @(Get-ChildItem -LiteralPath $stage -Recurse -File `
        -Filter '*.dll')
    if ($looseLibraries.Count -ne 0) {
        throw (
            'Release exposed app-local DLL dependencies instead of ' +
            'bundling them: ' + ($looseLibraries.FullName -join ', ')
        )
    }

    $forbidden = @(Get-ChildItem -LiteralPath $stage -Recurse -File |
        Where-Object {
            $_.Name -in @('carda.sav', 'cardb.sav', 'settings.json') -or
            $_.Extension -match
                '^\.(bin|cue|ccd|img|sub|iso|chd|pbp|dat|ovl|vol|psx|sav|mcr|ogg|pdb|ppm|log)$'
        })
    if ($forbidden.Count -ne 0) {
        throw (
            "Release contains forbidden disc, user, debug, or QA files: " +
            ($forbidden.FullName -join ', ')
        )
    }

    $manifest = [Collections.Generic.List[string]]::new()
    foreach ($file in Get-ChildItem -LiteralPath $stage -Recurse -File |
            Sort-Object FullName) {
        $relative = $file.FullName.Substring($stage.Length + 1)
        $hash = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
        $manifest.Add("$hash *$relative")
    }
    [IO.File]::WriteAllLines((Join-Path $stage 'SHA256SUMS.txt'), $manifest)

    Compress-Archive -LiteralPath $stage -DestinationPath $archive `
        -CompressionLevel Optimal
    $archiveHash = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash
    [IO.File]::WriteAllText(
        "$archive.sha256",
        "$archiveHash *$([IO.Path]::GetFileName($archive))`r`n")

    $files = @(Get-ChildItem -LiteralPath $stage -Recurse -File)
    $bytes = ($files | Measure-Object -Property Length -Sum).Sum
    Write-Output "release=$Version files=$($files.Count) bytes=$bytes"
    Write-Output "folder=$stage"
    Write-Output "archive=$archive"
    Write-Output "sha256=$archiveHash"
}
catch {
    [IO.File]::WriteAllText(
        (Join-Path $artifact 'release-failure.txt'),
        $_.Exception.ToString())
    throw
}
finally {
    $nativeBuildFull = [IO.Path]::GetFullPath($nativeBuild).TrimEnd('\')
    $expectedPrefix = $scratchRootFull + '\OpenGTPS1-release-native-'
    if ($nativeBuildFull.StartsWith(
            $expectedPrefix,
            [StringComparison]::OrdinalIgnoreCase) -and
        (Test-Path -LiteralPath $nativeBuildFull)) {
        Get-ChildItem -LiteralPath $nativeBuildFull -File -Recurse -Force `
            -ErrorAction SilentlyContinue |
            ForEach-Object {
                if ($_.IsReadOnly) {
                    $_.IsReadOnly = $false
                }
            }
        [IO.Directory]::Delete($nativeBuildFull, $true)
    }
    $managedBuildFull = [IO.Path]::GetFullPath($managedBuild).TrimEnd('\')
    $expectedManagedPrefix = $scratchRootFull + '\OpenGTPS1-release-managed-'
    if ($managedBuildFull.StartsWith(
            $expectedManagedPrefix,
            [StringComparison]::OrdinalIgnoreCase) -and
        (Test-Path -LiteralPath $managedBuildFull)) {
        Get-ChildItem -LiteralPath $managedBuildFull -File -Recurse -Force `
            -ErrorAction SilentlyContinue |
            ForEach-Object {
                if ($_.IsReadOnly) {
                    $_.IsReadOnly = $false
                }
            }
        [IO.Directory]::Delete($managedBuildFull, $true)
    }
    if ((Test-Path -LiteralPath $scratchRootFull) -and
        @(Get-ChildItem -LiteralPath $scratchRootFull -Force).Count -eq 0) {
        [IO.Directory]::Delete($scratchRootFull, $false)
    }
}
