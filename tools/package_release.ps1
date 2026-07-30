param(
    [ValidatePattern('^0\.8beta$')]
    [string]$Version = '0.8beta',
    [string]$ArtifactName = 'release-0.8beta'
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$artifact = Join-Path $repo "artifacts\$ArtifactName"
$folderName = "OpenGTPS1-$Version-win-x64"
$stage = Join-Path $artifact $folderName
$archive = Join-Path $artifact "$folderName.zip"
$nativeDll = Join-Path $repo 'build\native\Release\opengt_live_renderer.dll'

if (Test-Path -LiteralPath $artifact) {
    throw "Refusing to overwrite an existing release artifact: $artifact"
}

New-Item -ItemType Directory -Path $stage | Out-Null

try {
    cmake -S (Join-Path $repo 'native') -B (Join-Path $repo 'build\native')
    if ($LASTEXITCODE -ne 0) {
        throw "Native renderer configuration failed: $LASTEXITCODE"
    }
    cmake --build (Join-Path $repo 'build\native') --config Release
    if ($LASTEXITCODE -ne 0) {
        throw "Native renderer build failed: $LASTEXITCODE"
    }

    dotnet publish (
        Join-Path $repo 'generated\recompiled\GranTurismo2PC.csproj') `
        -c Release `
        -r win-x64 `
        --self-contained true `
        -p:PublishSingleFile=true `
        -p:Version=0.8.0-beta `
        -p:DebugType=None `
        -p:DebugSymbols=false `
        -o $stage
    if ($LASTEXITCODE -ne 0) {
        throw "Release publish failed: $LASTEXITCODE"
    }

    Copy-Item -LiteralPath $nativeDll `
        -Destination (Join-Path $stage 'opengt_live_renderer.dll')
    Copy-Item -LiteralPath (Join-Path $repo 'interface.ini') `
        -Destination (Join-Path $stage 'interface.ini')
    Copy-Item -LiteralPath (Join-Path $repo 'tools\recompone.loose.json') `
        -Destination (Join-Path $stage 'recompone.loose.json')
    Copy-Item -LiteralPath (Join-Path $repo 'release\README.md') `
        -Destination (Join-Path $stage 'README.md')
    Copy-Item -LiteralPath (
        Join-Path $repo 'release\Setup-From-Simulation-Disc.ps1') `
        -Destination (Join-Path $stage 'Setup-From-Simulation-Disc.ps1')
    Copy-Item -LiteralPath (Join-Path $repo 'release\RELEASE_NOTES.md') `
        -Destination (Join-Path $stage 'RELEASE_NOTES.md')
    Copy-Item -LiteralPath (Join-Path $repo 'vendor\RecompOne\LICENSE') `
        -Destination (Join-Path $stage 'RECOMPONE-LICENSE.txt')

    $required = @(
        'GranTurismo2PC.exe',
        'opengt_live_renderer.dll',
        'interface.ini',
        'recompone.loose.json',
        'README.md',
        'RELEASE_NOTES.md',
        'Setup-From-Simulation-Disc.ps1',
        'RECOMPONE-LICENSE.txt'
    )
    foreach ($name in $required) {
        if (-not (Test-Path -LiteralPath (Join-Path $stage $name) -PathType Leaf)) {
            throw "Release package is missing required file: $name"
        }
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
