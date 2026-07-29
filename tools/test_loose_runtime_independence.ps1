param(
    [string]$ArtifactName = 'loose-runtime-independence-current',
    [switch]$FullRace
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$deploy = (Resolve-Path (Join-Path $repo 'OpenGTPS1')).Path
$exe = Join-Path $deploy 'GranTurismo2PC.exe'
if ($FullRace) {
    $fixtureRelative = 'tests\fixtures\ai-autodrive-save-sunday-race.input'
} else {
    $fixtureRelative = 'tests\fixtures\validate-test-save.input'
}
$fixture = Join-Path $repo $fixtureRelative
$artifact = Join-Path $repo "artifacts\$ArtifactName"
$heldImages = Join-Path $artifact 'held-source-images'
$missingHeld = Join-Path $artifact 'held-DISC_META.DAT'
$missingAsset = Join-Path $deploy 'DISC_META.DAT'
$imageNames = @(
    'Gran Turismo 2 [Simulation Disc] [U] [SCUS-94488].ccd',
    'Gran Turismo 2 [Simulation Disc] [U] [SCUS-94488].cue',
    'Gran Turismo 2 [Simulation Disc] [U] [SCUS-94488].img',
    'Gran Turismo 2 [Simulation Disc] [U] [SCUS-94488].sub'
)

if (Get-Process GranTurismo2PC -ErrorAction SilentlyContinue) {
    throw 'Refusing to move source images while GranTurismo2PC is running'
}
if ([IO.Path]::GetFullPath($deploy).TrimEnd('\') -ne
    (Join-Path $repo 'OpenGTPS1').TrimEnd('\')) {
    throw "Unexpected deployment path: $deploy"
}

New-Item -ItemType Directory -Path $artifact -Force | Out-Null
New-Item -ItemType Directory -Path $heldImages -Force | Out-Null
$movedImages = [Collections.Generic.List[object]]::new()
$missingMoved = $false

function Invoke-AuditRun {
    param(
        [string]$Name,
        [bool]$ExpectSuccess
    )

    $start = [Diagnostics.ProcessStartInfo]::new()
    $start.FileName = $exe
    $start.WorkingDirectory = $deploy
    $start.Arguments = '--headless'
    $start.UseShellExecute = $false
    $start.CreateNoWindow = $true
    $start.RedirectStandardOutput = $true
    $start.RedirectStandardError = $true
    $start.EnvironmentVariables['RECOMPONE_INPUT_FILE'] = $fixture
    $start.EnvironmentVariables['RECOMPONE_DISABLE_LIVE_INPUT'] = '1'
    $start.EnvironmentVariables['RECOMPONE_SUPPRESS_RUMBLE'] = '1'
    $start.EnvironmentVariables['RECOMPONE_UNTHROTTLED'] = '1'
    if ($ExpectSuccess -and $FullRace) {
        $exitPoll = '45050'
    } else {
        $exitPoll = '1150'
    }
    $start.EnvironmentVariables['RECOMPONE_EXIT_AFTER_INPUT_POLL'] = $exitPoll
    if ($ExpectSuccess -and $FullRace) {
        $start.EnvironmentVariables['RECOMPONE_GT2_AI_AUTODRIVE'] = '1'
    }

    $process = [Diagnostics.Process]::Start($start)
    $stdout = $process.StandardOutput.ReadToEndAsync()
    $stderr = $process.StandardError.ReadToEndAsync()
    $timeout = if ($ExpectSuccess -and $FullRace) { 600000 } else { 120000 }
    if (-not $process.WaitForExit($timeout)) {
        $process.Kill($true)
        $process.WaitForExit()
        throw "$Name timed out"
    }
    $stdoutText = $stdout.Result
    $stderrText = $stderr.Result
    [IO.File]::WriteAllText((Join-Path $artifact "$Name.stdout.log"), $stdoutText)
    [IO.File]::WriteAllText((Join-Path $artifact "$Name.stderr.log"), $stderrText)

    if ($stderrText -notmatch 'headless audio backend=dummy' -or
        $stderrText -notmatch 'SDL audio ready: driver=dummy') {
        throw "$Name did not prove the dummy audio backend"
    }
    if ($ExpectSuccess -and $process.ExitCode -ne 0) {
        throw "$Name exited with code $($process.ExitCode)"
    }
    if (-not $ExpectSuccess -and $process.ExitCode -eq 0) {
        throw "$Name unexpectedly succeeded"
    }
    return "$stdoutText`n$stderrText"
}

try {
    foreach ($name in $imageNames) {
        $source = (Resolve-Path -LiteralPath (Join-Path $repo $name)).Path
        if ((Split-Path -Parent $source).TrimEnd('\') -ne $repo.TrimEnd('\')) {
            throw "Source image escaped the repository root: $source"
        }
        $destination = Join-Path $heldImages $name
        if (Test-Path -LiteralPath $destination) {
            throw "Held-image target already exists: $destination"
        }
        Move-Item -LiteralPath $source -Destination $destination
        $movedImages.Add([pscustomobject]@{
            Source = $source
            Destination = $destination
        })
    }

    $successLog = Invoke-AuditRun -Name 'source-images-unavailable' -ExpectSuccess $true
    if ($successLog -notmatch
        '\[CD\] standalone loose files=7 volume=GRANTURISMO2') {
        throw 'The successful audit did not identify the standalone loose layout'
    }
    if ($successLog -match
        '(?i)Gran Turismo 2.*\.(?:ccd|cue|img|sub)') {
        throw 'The successful audit referenced an archival disc image'
    }
    if ($FullRace) {
        if ($successLog -notmatch
            'auto-drive engaged pass=1/2 phase=race' -or
            $successLog -notmatch
            'auto-drive engaged pass=2/2 phase=replay') {
            throw 'Full loose audit did not exercise both race and replay AI'
        }
        $capture = Join-Path $deploy 'recompone_capture__45000.ppm'
        if (-not (Test-Path -LiteralPath $capture)) {
            throw 'Full loose audit did not capture the post-replay state'
        }
        $ppm = Join-Path $artifact 'post-replay-45000.ppm'
        Copy-Item -LiteralPath $capture -Destination $ppm -Force
        & ffmpeg -hide_banner -loglevel error -y -i $ppm (
            Join-Path $artifact 'post-replay-45000.png')
        if ($LASTEXITCODE -ne 0) {
            throw 'ffmpeg failed to convert the post-replay capture'
        }
    }

    Move-Item -LiteralPath $missingAsset -Destination $missingHeld
    $missingMoved = $true
    $missingLog = Invoke-AuditRun -Name 'missing-loose-file' -ExpectSuccess $false
    if ($missingLog -notmatch 'DISC_META\.DAT' -or
        $missingLog -notmatch '(?i)missing|not found|required') {
        throw 'Missing-file failure did not clearly name DISC_META.DAT'
    }

    Write-Output "portable runtime=$exe"
    Write-Output 'archival CCD/CUE/IMG/SUB unavailable during successful boot'
    if ($FullRace) {
        Write-Output 'full race and natural replay=completed with source images unavailable'
    }
    Write-Output 'missing-file diagnostic=DISC_META.DAT named explicitly'
    Write-Output "evidence=$artifact audio=dummy for both runs"
}
finally {
    if ($missingMoved -and (Test-Path -LiteralPath $missingHeld)) {
        Move-Item -LiteralPath $missingHeld -Destination $missingAsset
    }
    foreach ($entry in $movedImages) {
        if (Test-Path -LiteralPath $entry.Destination) {
            Move-Item -LiteralPath $entry.Destination -Destination $entry.Source
        }
    }
}
