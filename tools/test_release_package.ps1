param(
    [Parameter(Mandatory = $true)]
    [string]$Archive,
    [Parameter(Mandatory = $true)]
    [string]$SimulationImagePath,
    [Parameter(Mandatory = $true)]
    [string]$ArcadeImagePath,
    [string]$Gt1ImagePath = '',
    [string]$ArtifactName = 'release-package-validation',
    [ValidateRange(600, 50000)]
    [int]$ExitPoll = 17000,
    [ValidateRange(180, 1200)]
    [int]$TimeoutSeconds = 360
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$archivePath = (Resolve-Path -LiteralPath $Archive).Path
$simulationImage = (Resolve-Path -LiteralPath $SimulationImagePath).Path
$arcadeImage = (Resolve-Path -LiteralPath $ArcadeImagePath).Path
$gt1Image = if ([string]::IsNullOrWhiteSpace($Gt1ImagePath)) {
    $null
} else {
    (Resolve-Path -LiteralPath $Gt1ImagePath).Path
}
$artifact = Join-Path $repo "artifacts\$ArtifactName"
$scratchRoot = Join-Path $repo 'work\release-audit-scratch'
$scratchRootFull = [IO.Path]::GetFullPath($scratchRoot).TrimEnd('\')
$scratch = Join-Path $scratchRootFull (
    'OpenGTPS1-release-audit-' + [guid]::NewGuid().ToString('N'))

function Stop-InstalledProcesses([string]$InstallRoot) {
    $prefix = [IO.Path]::GetFullPath($InstallRoot).TrimEnd('\') + '\'
    $processIds = @(
        Get-CimInstance Win32_Process -Filter "Name='GranTurismo2PC.exe'" |
            Where-Object {
                -not [string]::IsNullOrWhiteSpace($_.ExecutablePath) -and
                $_.ExecutablePath.StartsWith(
                    $prefix,
                    [StringComparison]::OrdinalIgnoreCase)
            } |
            ForEach-Object { [int]$_.ProcessId })
    foreach ($processId in $processIds) {
        Stop-Process -Id $processId -Force -ErrorAction SilentlyContinue
    }
    foreach ($processId in $processIds) {
        Wait-Process -Id $processId -Timeout 10 -ErrorAction SilentlyContinue
    }
}

if (Test-Path -LiteralPath $artifact) {
    throw "Refusing to overwrite existing validation artifact: $artifact"
}
if (Test-Path -LiteralPath $scratch) {
    throw "Unexpected existing validation directory: $scratch"
}

New-Item -ItemType Directory -Path $artifact | Out-Null
New-Item -ItemType Directory -Path $scratchRootFull -Force | Out-Null
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
                '^\.(bin|cue|ccd|img|sub|iso|chd|pbp|dat|dll|ovl|vol|psx|sav|mcr|ogg|pdb|ppm|log)$'
        })
    if ($forbiddenBeforeSetup.Count -ne 0) {
        throw (
            "Unpacked public archive contains a loose dependency, game, " +
            "user, debug, or QA file: " +
            ($forbiddenBeforeSetup.FullName -join ', ')
        )
    }

    $readme = Join-Path $install 'README.md'
    $setup = Join-Path $install 'OpenGTPS1-Setup.exe'
    if (-not (Test-Path -LiteralPath $readme) -or
        -not (Test-Path -LiteralPath $setup)) {
        throw 'Release archive is missing its README or GUI setup executable'
    }
    $readmeText = Get-Content -LiteralPath $readme -Raw
    if ($readmeText -notmatch '0\.9b' -or
        $readmeText -notmatch 'byte-exact US Gran Turismo 2 two-disc set' -or
        $readmeText -notmatch 'SCUS-94488' -or
        $readmeText -notmatch 'SCUS-94455' -or
        $readmeText -notmatch 'SCUS-94194' -or
        $readmeText -notmatch 'graphical first-run installer') {
        throw 'Release README does not clearly identify version and supported discs'
    }

    $setupStart = [Diagnostics.ProcessStartInfo]::new()
    $setupStart.FileName = $setup
    $setupStart.WorkingDirectory = $install
    $setupStart.UseShellExecute = $false
    $setupStart.CreateNoWindow = $true
    foreach ($argument in @(
            '--headless',
            '--simulation', $simulationImage,
            '--arcade', $arcadeImage,
            '--install-root', $install)) {
        $setupStart.ArgumentList.Add($argument)
    }
    if ($null -ne $gt1Image) {
        $setupStart.ArgumentList.Add('--gt1')
        $setupStart.ArgumentList.Add($gt1Image)
    }
    $setupProcess = [Diagnostics.Process]::Start($setupStart)
    if (-not $setupProcess.WaitForExit($TimeoutSeconds * 1000)) {
        $setupProcess.Kill($true)
        $setupProcess.WaitForExit()
        throw "GUI setup headless validation exceeded ${TimeoutSeconds}s"
    }
    $setupExit = $setupProcess.ExitCode
    $setupLog = Join-Path $install 'OpenGTPS1-Setup.log'
    $setupOutput = if (Test-Path -LiteralPath $setupLog) {
        Get-Content -LiteralPath $setupLog
    } else {
        @()
    }
    [IO.File]::WriteAllLines(
        (Join-Path $artifact 'setup.log'),
        [string[]]$setupOutput)
    if ($setupExit -ne 0) {
        throw "Unified GT2 setup exited with code $setupExit"
    }
    if (($setupOutput -join "`n") -notmatch 'Installation complete') {
        throw 'Self-contained setup did not report completion'
    }
    if ($null -ne $gt1Image) {
        if (($setupOutput -join "`n") -notmatch
                'Gran Turismo 1 merge complete' -or
            -not (Test-Path -LiteralPath (
                Join-Path $install 'GT1_CONTENT.json') -PathType Leaf) -or
            -not (Test-Path -LiteralPath (
                Join-Path $install 'GTLIVERY.BIN') -PathType Leaf)) {
            throw 'Optional Gran Turismo 1 content was not merged completely'
        }
    } elseif ((Test-Path -LiteralPath (
            Join-Path $install 'GT1_CONTENT.json')) -or
        (Test-Path -LiteralPath (
            Join-Path $install 'GTLIVERY.BIN'))) {
        throw 'GT2-only setup retained optional Gran Turismo 1 content'
    }

    $arcadeManifest = Get-Content -LiteralPath (
        Join-Path $install 'manifests\arcade.json') -Raw | ConvertFrom-Json
    $arcadeVolumes = @(
        $arcadeManifest.files | Where-Object path -eq 'GT2.VOL')
    if ($arcadeVolumes.Count -ne 1 -or [int]$arcadeVolumes[0].lba -ne 473) {
        throw 'Installed Arcade GT2.VOL is not mapped at unified LBA 473'
    }

    $exe = Join-Path $install 'GranTurismo2PC.exe'
    $bundleExtract = Join-Path $scratch 'bundle-extract'
    $start = [Diagnostics.ProcessStartInfo]::new()
    $start.FileName = $exe
    $start.WorkingDirectory = $install
    $start.Arguments = '--headless --arcade-replay seattle-circuit'
    $start.UseShellExecute = $false
    $start.CreateNoWindow = $true
    $start.RedirectStandardOutput = $true
    $start.RedirectStandardError = $true
    $start.EnvironmentVariables['RECOMPONE_DISABLE_LIVE_INPUT'] = '1'
    $start.EnvironmentVariables['RECOMPONE_SUPPRESS_RUMBLE'] = '1'
    $start.EnvironmentVariables['SDL_AUDIODRIVER'] = 'dummy'
    $start.EnvironmentVariables['DOTNET_BUNDLE_EXTRACT_BASE_DIR'] =
        $bundleExtract
    $start.EnvironmentVariables['RECOMPONE_EXIT_AFTER_INPUT_POLL'] =
        $ExitPoll.ToString()

    $process = [Diagnostics.Process]::Start($start)
    $stdout = $process.StandardOutput.ReadToEndAsync()
    $stderr = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
        try {
            $process.Kill($true)
            $process.WaitForExit()
        } catch {
            # The single-file host can re-exec after native extraction. The
            # exact scratch-root sweep below owns any such descendant.
        } finally {
            Stop-InstalledProcesses -InstallRoot $install
        }
        throw "Installed release boot exceeded ${TimeoutSeconds}s"
    }
    $stdoutText = $stdout.Result
    $stderrText = $stderr.Result
    [IO.File]::WriteAllText((Join-Path $artifact 'stdout.log'), $stdoutText)
    [IO.File]::WriteAllText((Join-Path $artifact 'stderr.log'), $stderrText)

    if ($process.ExitCode -ne 0) {
        throw "Installed release boot exited with code $($process.ExitCode)"
    }
    if ($stdoutText -notmatch
            '\[CD\] standalone loose files=7 volume=GRANTURISMO2' -or
        $stdoutText -notmatch
            '\[Host\] direct Arcade natural replay requested: seattle-circuit' -or
        $stdoutText -notmatch
            '\[Host\] native unified guest=arcade') {
        throw 'Installed release did not boot the direct unified Seattle replay path'
    }
    $replayStage = [regex]::Match(
        $stderrText,
        "\[Input\] stage 'replay_1' at absolute poll (\d+)")
    if (-not $replayStage.Success) {
        throw 'Installed release exited before GT2 instantiated its natural replay'
    }
    $replayStageAbsolutePoll = [int64]$replayStage.Groups[1].Value
    $replayProofPolls = [int64]$ExitPoll - $replayStageAbsolutePoll
    if ($replayProofPolls -lt 300) {
        throw (
            'Installed release rendered only ' + $replayProofPolls +
            ' replay-stage polls; at least 300 are required')
    }
    if ($stderrText -notmatch
            '\[GT2-AI\] auto-drive engaged pass=2/2 phase=replay car=0') {
        throw "Installed release did not enter GT2's native replay driver phase"
    }
    if ($stderrText -notmatch 'headless audio backend=dummy' -or
        $stderrText -notmatch 'SDL audio ready: driver=dummy') {
        throw 'Installed release did not prove dummy-audio isolation'
    }
    if ($stderrText -notmatch '\[Runtime\] shutdown complete; exit=0') {
        throw 'Installed release did not complete a clean shutdown'
    }
    if ("$stdoutText`n$stderrText" -match
        '(?i)unmapped call:|unmapped address:|unhandled exception|' +
        'access violation|fatal error|\[Native-World\] disabled:|' +
        'development compositor-only|compositor fallback|' +
        'stock-sector fallback|first-output seed exceeded') {
        throw 'Installed release logged a crash signature'
    }

    $runtimeText = "$stdoutText`n$stderrText"
    if ($runtimeText -notmatch
            '\[Native-World\] enabled .*mode=authored-only ' +
            'syntheticPath=absent' -or
        $runtimeText -notmatch
            '\[GT2-Raw-Track\] enabled mode=replace .*' +
            'guestTrackFallback=disabled') {
        throw 'Installed release did not activate the sole modern authored-world path'
    }
    $replayTextOffset = $stderrText.IndexOf(
        $replayStage.Value,
        [StringComparison]::Ordinal)
    $replayText = $stderrText.Substring($replayTextOffset)
    $replayWorldFrames = @([regex]::Matches(
        $replayText,
        '(?m)^\[Native-World\] frame=.* mode=authored .*' +
        'triangles=[1-9]\d* commands=[1-9]\d* '))
    if ($replayWorldFrames.Count -lt 3) {
        throw (
            'Installed release logged only ' + $replayWorldFrames.Count +
            ' authored modern-renderer frames after entering natural replay')
    }
    if ($runtimeText -notmatch '\[Native-World\] first-output seed ready') {
        throw 'Installed release did not synchronously seed initial world ownership'
    }
    if ($runtimeText -notmatch
            '\[Native-World\] frame=.*mode=authored .*' +
            'size=1708x960 ') {
        throw 'Installed release did not prove 4x true Hor+ 16:9 world output'
    }
    if ($runtimeText -notmatch
            '\[Render-UV\] commands=\d+ textured=\d+ ' +
            'individualPerspective=\d+ islandPerspective=\d+ ' +
            'worldFallback=0 trackFallback=0 vehicleFallback=0 ' +
            'otherFallback=0 .*projection=fixed-modern') {
        throw 'Installed release did not prove fixed perspective UVs for every world texture'
    }
    if ($runtimeText -notmatch
            '\[Render-Batches\] background=\d+/\d+/\d+/max\d+ ' +
            'track=\d+/\d+/\d+/max\d+ ' +
            'vehicle=\d+/\d+/\d+/max\d+ ' +
            'unclassified=\d+/\d+/\d+/max\d+ ' +
            'screen=\d+/\d+/\d+/max\d+ .*' +
            'depth=track\+vehicle ') {
        throw 'Installed release did not prove explicit world/effect/depth layering'
    }
    if ($runtimeText -notmatch
            '\[Render-HUD\] commands=\d+ components=\d+ ' +
            'anchors=\d+/\d+/\d+ guest=320x240 ' +
            'policy=relative-edge-groups') {
        throw 'Installed release did not prove relative-margin Hor+ HUD placement'
    }
    if ($runtimeText -notmatch
            '\[Native-World-Classification\] frames=[1-9]\d* ' +
            'classifiedWorldCommands=[1-9]\d* ' +
            'unclassifiedWorldCommands=0 framesWithUnclassifiedWorld=0 ' +
            'maximumUnclassifiedWorld=0') {
        throw 'Installed release retained ownerless 3D geometry'
    }

    $residency = [regex]::Match(
        $runtimeText,
        '\[GT2-Raw-Track-Summary\] frames=(\d+) coverageFrames=(\d+) ' +
        'objectsPerFrame=(\d+)\.\.(\d+) ' +
        'sourcePrimitivesPerFrame=(\d+)\.\.(\d+) ' +
        'trianglesPerFrame=(\d+)\.\.(\d+) .*' +
        'decodeFailures=(\d+) guestTrackFallbacks=(\d+)')
    if (-not $residency.Success) {
        throw 'Installed release did not report Seattle residency telemetry'
    }
    $residencyValues = for ($index = 1; $index -le 10; $index++) {
        [int64]$residency.Groups[$index].Value
    }
    if ($residencyValues[0] -le 0 -or
        $residencyValues[1] -ne $residencyValues[0] -or
        $residencyValues[2] -le 0 -or
        $residencyValues[3] -le $residencyValues[2] -or
        $residencyValues[3] -ge 243 -or
        $residencyValues[4] -le 0 -or
        $residencyValues[5] -le $residencyValues[4] -or
        $residencyValues[5] -ge 11191 -or
        $residencyValues[6] -le 0 -or
        $residencyValues[7] -lt $residencyValues[6] -or
        $residencyValues[8] -ne 0 -or
        $residencyValues[9] -ne 0) {
        throw (
            'Installed release did not preserve Seattle''s bounded, varying ' +
            'authored course selector on every covered frame: ' +
            $residency.Value)
    }

    $nativeShutdown = [regex]::Match(
        $runtimeText,
        '\[Native-World\] shutdown submitted=(\d+) rendered=(\d+) ' +
        'actual=(\d+) synthetic=(\d+) repeated=(\d+) ' +
        'syntheticPath=absent authoredNoOutput=(\d+) consumed=(\d+) ' +
        'dropped=(\d+)')
    if (-not $nativeShutdown.Success) {
        throw 'Installed release did not report authored-world shutdown telemetry'
    }
    $shutdownValues = for ($index = 1; $index -le 8; $index++) {
        [int64]$nativeShutdown.Groups[$index].Value
    }
    if ($shutdownValues[0] -le 0 -or
        $shutdownValues[1] -le 0 -or
        $shutdownValues[2] -ne $shutdownValues[1] -or
        $shutdownValues[3] -ne 0 -or
        $shutdownValues[4] -ne 0 -or
        $shutdownValues[7] -ne 0) {
        throw (
            'Installed release did not remain authored-only and lossless: ' +
            $nativeShutdown.Value)
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
    Write-Output (
        'discs=SCUS-94488-NTSC-U-revision-2+' +
        'SCUS-94455-NTSC-U' +
        $(if ($null -ne $gt1Image) { '+SCUS-94194-NTSC-U' } else { '' }) +
        ' audio=dummy direct=seattle-replay')
    Write-Output "evidence=$artifact"
}
finally {
    if (Test-Path -LiteralPath $scratch -PathType Container) {
        $installedRoots = @(Get-ChildItem -LiteralPath $scratch -Directory)
        foreach ($installedRoot in $installedRoots) {
            Stop-InstalledProcesses -InstallRoot $installedRoot.FullName
        }
    }
    $scratchFull = [IO.Path]::GetFullPath($scratch)
    if ($scratchFull.StartsWith(
            $scratchRootFull + '\OpenGTPS1-release-audit-',
            [StringComparison]::OrdinalIgnoreCase) -and
        (Test-Path -LiteralPath $scratchFull)) {
        [IO.Directory]::Delete($scratchFull, $true)
    }
    if ((Test-Path -LiteralPath $scratchRootFull) -and
        @(Get-ChildItem -LiteralPath $scratchRootFull -Force).Count -eq 0) {
        [IO.Directory]::Delete($scratchRootFull, $false)
    }
}
