param(
    [Parameter(Mandatory = $true)]
    [string]$Archive,
    [Parameter(Mandatory = $true)]
    [string]$ImagePath,
    [string]$ArtifactName = 'release-package-validation',
    [ValidateRange(600, 10000)]
    [int]$ExitPoll = 1200
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$archivePath = (Resolve-Path -LiteralPath $Archive).Path
$image = (Resolve-Path -LiteralPath $ImagePath).Path
$artifact = Join-Path $repo "artifacts\$ArtifactName"
$tempRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\')
$scratch = Join-Path $tempRoot (
    'OpenGTPS1-release-audit-' + [guid]::NewGuid().ToString('N'))

if (Test-Path -LiteralPath $artifact) {
    throw "Refusing to overwrite existing validation artifact: $artifact"
}
if (Test-Path -LiteralPath $scratch) {
    throw "Unexpected existing validation directory: $scratch"
}

New-Item -ItemType Directory -Path $artifact | Out-Null
New-Item -ItemType Directory -Path $scratch | Out-Null

try {
    Expand-Archive -LiteralPath $archivePath -DestinationPath $scratch
    $roots = @(Get-ChildItem -LiteralPath $scratch -Directory)
    if ($roots.Count -ne 1) {
        throw "Release archive must contain exactly one root folder"
    }
    $install = $roots[0].FullName

    $forbiddenBeforeSetup = @(Get-ChildItem -LiteralPath $install -Recurse -File |
        Where-Object {
            $_.Name -in @('carda.sav', 'cardb.sav', 'settings.json') -or
            $_.Extension -match
                '^\.(bin|cue|ccd|img|sub|iso|chd|pbp|dat|ovl|vol|psx|sav|mcr|ogg|pdb|ppm|log)$'
        })
    if ($forbiddenBeforeSetup.Count -ne 0) {
        throw (
            "Unpacked public archive contains game, user, debug, or QA data: " +
            ($forbiddenBeforeSetup.FullName -join ', ')
        )
    }

    $readme = Join-Path $install 'README.md'
    $setup = Join-Path $install 'Setup-From-Simulation-Disc.ps1'
    if (-not (Test-Path -LiteralPath $readme) -or
        -not (Test-Path -LiteralPath $setup)) {
        throw 'Release archive is missing its README or Simulation Disc setup utility'
    }
    $readmeText = Get-Content -LiteralPath $readme -Raw
    if ($readmeText -notmatch '0\.8beta' -or
        $readmeText -notmatch 'Simulation Disc only' -or
        $readmeText -notmatch 'SCUS-94488') {
        throw 'Release README does not clearly identify version and supported disc'
    }

    $setupOutput = & powershell.exe -NoProfile -ExecutionPolicy Bypass `
        -File $setup -ImagePath $image 2>&1
    $setupExit = $LASTEXITCODE
    [IO.File]::WriteAllLines(
        (Join-Path $artifact 'setup.log'),
        [string[]]$setupOutput)
    if ($setupExit -ne 0) {
        throw "Simulation Disc setup exited with code $setupExit"
    }
    if (($setupOutput -join "`n") -notmatch 'Installation complete') {
        throw 'Simulation Disc setup did not report completion'
    }

    $exe = Join-Path $install 'GranTurismo2PC.exe'
    $start = [Diagnostics.ProcessStartInfo]::new()
    $start.FileName = $exe
    $start.WorkingDirectory = $install
    $start.Arguments = '--headless'
    $start.UseShellExecute = $false
    $start.CreateNoWindow = $true
    $start.RedirectStandardOutput = $true
    $start.RedirectStandardError = $true
    $start.EnvironmentVariables['RECOMPONE_DISABLE_LIVE_INPUT'] = '1'
    $start.EnvironmentVariables['RECOMPONE_SUPPRESS_RUMBLE'] = '1'
    $start.EnvironmentVariables['SDL_AUDIODRIVER'] = 'dummy'
    $start.EnvironmentVariables['RECOMPONE_EXIT_AFTER_INPUT_POLL'] =
        $ExitPoll.ToString()

    $process = [Diagnostics.Process]::Start($start)
    $stdout = $process.StandardOutput.ReadToEndAsync()
    $stderr = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit(180000)) {
        $process.Kill($true)
        $process.WaitForExit()
        throw 'Installed release boot timed out'
    }
    $stdoutText = $stdout.Result
    $stderrText = $stderr.Result
    [IO.File]::WriteAllText((Join-Path $artifact 'stdout.log'), $stdoutText)
    [IO.File]::WriteAllText((Join-Path $artifact 'stderr.log'), $stderrText)

    if ($process.ExitCode -ne 0) {
        throw "Installed release boot exited with code $($process.ExitCode)"
    }
    if ($stdoutText -notmatch
        '\[CD\] standalone loose files=7 volume=GRANTURISMO2') {
        throw 'Installed release did not boot through the seven-file loose layout'
    }
    if ($stderrText -notmatch 'headless audio backend=dummy' -or
        $stderrText -notmatch 'SDL audio ready: driver=dummy') {
        throw 'Installed release did not prove dummy-audio isolation'
    }
    if ($stderrText -notmatch '\[Runtime\] shutdown complete; exit=0') {
        throw 'Installed release did not complete a clean shutdown'
    }
    if ("$stdoutText`n$stderrText" -match
        '(?i)unmapped call:|unmapped address:|unhandled exception|access violation|fatal error') {
        throw 'Installed release logged a crash signature'
    }

    $manifestLines = [Collections.Generic.List[string]]::new()
    foreach ($file in Get-ChildItem -LiteralPath $install -Recurse -File |
            Sort-Object FullName) {
        $relative = $file.FullName.Substring($install.Length + 1)
        $hash = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
        $manifestLines.Add("$relative|$($file.Length)|$hash")
    }
    [IO.File]::WriteAllLines(
        (Join-Path $artifact 'installed-manifest.txt'),
        $manifestLines)
    $archiveHash = (Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash
    [IO.File]::WriteAllText(
        (Join-Path $artifact 'archive-sha256.txt'),
        "$archiveHash *$([IO.Path]::GetFileName($archivePath))`r`n")

    Write-Output "release-package=passed files=$($manifestLines.Count)"
    Write-Output "archive_sha256=$archiveHash"
    Write-Output "disc=SCUS-94488-NTSC-U-revision-2 audio=dummy"
    Write-Output "evidence=$artifact"
}
finally {
    $scratchFull = [IO.Path]::GetFullPath($scratch)
    if ($scratchFull.StartsWith(
            $tempRoot + '\OpenGTPS1-release-audit-',
            [StringComparison]::OrdinalIgnoreCase) -and
        (Test-Path -LiteralPath $scratchFull)) {
        [IO.Directory]::Delete($scratchFull, $true)
    }
}
