param(
    [string]$ArtifactName = 'ogg-queue-packaged-current',
    [int]$ExitPoll = 5000
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$deploy = Join-Path $repo 'OpenGTPS1'
$music = Join-Path $deploy 'music'
$exe = Join-Path $deploy 'GranTurismo2PC.exe'
$fixture = Join-Path $repo 'tests\fixtures\ai-autodrive-save-sunday-race.input'
$artifact = Join-Path $repo "artifacts\$ArtifactName"
$ffmpeg = (Get-Command ffmpeg -ErrorAction Stop).Source

New-Item -ItemType Directory -Path $music -Force | Out-Null
New-Item -ItemType Directory -Path $artifact -Force | Out-Null

$testTracks = @(
    @{ Name = 'Codex QA - Queue Track.ogg'; Frequency = 440 },
    @{ Name = 'Codex QA  - Queue Track.ogg'; Frequency = 660 },
    @{ Name = 'Malformed Queue Track.ogg'; Frequency = 880 }
)

foreach ($track in $testTracks) {
    $path = Join-Path $music $track.Name
    if (Test-Path -LiteralPath $path) {
        throw "Refusing to replace an existing music file: $path"
    }
}

try {
    foreach ($track in $testTracks) {
        $path = Join-Path $music $track.Name
        & $ffmpeg -hide_banner -loglevel error -y `
            -f lavfi -i "sine=frequency=$($track.Frequency):sample_rate=22050" `
            -t 0.25 -c:a libvorbis -q:a 2 $path
        if ($LASTEXITCODE -ne 0) {
            throw "ffmpeg failed to create $($track.Name)"
        }
    }

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
    $start.EnvironmentVariables['RECOMPONE_TRACE_MUSIC'] = '1'
    $start.EnvironmentVariables['RECOMPONE_TRACE_AUDIO'] = '1'
    $start.EnvironmentVariables['RECOMPONE_EXIT_AFTER_INPUT_POLL'] =
        $ExitPoll.ToString()

    $process = [Diagnostics.Process]::Start($start)
    $stdout = $process.StandardOutput.ReadToEndAsync()
    $stderr = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit(600000)) {
        $process.Kill($true)
        $process.WaitForExit()
        throw 'Packaged OGG queue test timed out'
    }

    $stdoutText = $stdout.Result
    $stderrText = $stderr.Result
    [IO.File]::WriteAllText((Join-Path $artifact 'stdout.log'), $stdoutText)
    [IO.File]::WriteAllText((Join-Path $artifact 'stderr.log'), $stderrText)

    if ($process.ExitCode -ne 0) {
        throw "Packaged OGG queue test exited with code $($process.ExitCode)"
    }
    if ($stderrText -notmatch 'headless audio backend=dummy' -or
        $stderrText -notmatch 'SDL audio ready: driver=dummy') {
        throw 'Packaged OGG queue test did not prove the dummy audio backend'
    }

    $combined = "$stdoutText`n$stderrText"
    foreach ($required in @(
        '[Music] indexed 3 OGG track(s)',
        '[Music] queued: Codex QA - Queue Track',
        '[Music] queued: Codex QA - Queue Track (2)',
        '[Music] queued: Unknown Artist - Malformed Queue Track',
        '[Music] stream active; queue=3',
        '[MUSIC-QUEUE] completed=',
        'loop=1'
    )) {
        if (-not $combined.Contains($required)) {
            throw "Missing expected OGG queue evidence: $required"
        }
    }
    if ($combined -match '\[AUDIO-QUEUE\].*starvations=[1-9]') {
        throw 'SDL queue starvation occurred during the OGG queue test'
    }

    Write-Output "OGG queue evidence=$artifact"
    Write-Output 'audio safety=dummy backend proven'
    Write-Output 'SDL queue starvation=zero'
    Write-Output 'filename labels=valid, malformed, and duplicate behavior proven'
}
finally {
    foreach ($track in $testTracks) {
        $path = Join-Path $music $track.Name
        if (Test-Path -LiteralPath $path) {
            Move-Item -LiteralPath $path `
                -Destination (Join-Path $artifact $track.Name) -Force
        }
    }
}
