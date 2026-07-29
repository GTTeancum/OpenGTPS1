param(
    [int]$StartPoll = 7000,
    [int]$EndPoll = 10600,
    [string]$ArtifactName = 'audio-health-current',
    [string]$Fixture = 'tests\fixtures\ai-autodrive-save-sunday-race.input',
    [switch]$VisibleMuted
)

$ErrorActionPreference = 'Stop'
if ($EndPoll -le $StartPoll) {
    throw 'EndPoll must be greater than StartPoll'
}

$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$deploy = (Resolve-Path (Join-Path $repo 'OpenGTPS1')).Path
$exe = Join-Path $deploy 'GranTurismo2PC.exe'
$fixturePath = if ([IO.Path]::IsPathRooted($Fixture)) {
    $Fixture
} else {
    Join-Path $repo $Fixture
}
$artifact = Join-Path $repo "artifacts\$ArtifactName"
$wav = Join-Path $artifact 'pre-volume.wav'
$flac = Join-Path $artifact 'pre-volume.flac'
$silenceLog = Join-Path $artifact 'silence-analysis.log'

if (-not (Test-Path -LiteralPath $exe -PathType Leaf)) {
    throw "Packaged executable is missing: $exe"
}
if (-not (Test-Path -LiteralPath $fixturePath -PathType Leaf)) {
    throw "Input fixture is missing: $fixturePath"
}
New-Item -ItemType Directory -Path $artifact -Force | Out-Null
foreach ($path in $wav, $flac, $silenceLog) {
    if (Test-Path -LiteralPath $path) {
        Remove-Item -LiteralPath $path -Force
    }
}

$settings = Join-Path $deploy 'settings.json'
$settingsHashBefore = (Get-FileHash -LiteralPath $settings -Algorithm SHA256).Hash
$start = [Diagnostics.ProcessStartInfo]::new()
$start.FileName = $exe
$start.WorkingDirectory = $deploy
$start.Arguments = if ($VisibleMuted) { '--mute' } else { '--headless' }
$start.UseShellExecute = $false
$start.CreateNoWindow = $true
$start.RedirectStandardOutput = $true
$start.RedirectStandardError = $true
$start.EnvironmentVariables['RECOMPONE_INPUT_FILE'] = $fixturePath
$start.EnvironmentVariables['RECOMPONE_DISABLE_LIVE_INPUT'] = '1'
$start.EnvironmentVariables['RECOMPONE_SUPPRESS_RUMBLE'] = '1'
$start.EnvironmentVariables['RECOMPONE_GT2_AI_AUTODRIVE'] = '1'
$start.EnvironmentVariables['RECOMPONE_TRACE_AUDIO'] = '1'
$start.EnvironmentVariables['RECOMPONE_AUDIO_CAPTURE'] = $wav
$start.EnvironmentVariables['RECOMPONE_AUDIO_CAPTURE_PRE_VOLUME'] = '1'
$start.EnvironmentVariables['RECOMPONE_AUDIO_CAPTURE_START_INPUT_POLL'] =
    $StartPoll.ToString()
$start.EnvironmentVariables['RECOMPONE_AUDIO_CAPTURE_END_INPUT_POLL'] =
    $EndPoll.ToString()
$start.EnvironmentVariables['RECOMPONE_EXIT_AFTER_INPUT_POLL'] =
    ($EndPoll + 50).ToString()

$process = [Diagnostics.Process]::Start($start)
$stdout = $process.StandardOutput.ReadToEndAsync()
$stderr = $process.StandardError.ReadToEndAsync()
if (-not $process.WaitForExit(600000)) {
    $process.Kill($true)
    $process.WaitForExit()
    throw 'Audio-health capture timed out'
}
$stdoutText = $stdout.Result
$stderrText = $stderr.Result
[IO.File]::WriteAllText((Join-Path $artifact 'stdout.log'), $stdoutText)
[IO.File]::WriteAllText((Join-Path $artifact 'stderr.log'), $stderrText)

if ($process.ExitCode -ne 0) {
    throw "Audio-health capture exited with code $($process.ExitCode)"
}
if ($VisibleMuted) {
    if ($stderrText -notmatch
            '\[Host\] --mute: audio output muted for this launch' -or
        $stderrText -notmatch
            'SDL audio ready: driver=(?!dummy\b)\S+') {
        throw 'Visible audio-health capture did not prove session mute and a real SDL driver'
    }
} elseif ($stderrText -notmatch 'headless audio backend=dummy' -or
    $stderrText -notmatch 'SDL audio ready: driver=dummy') {
    throw 'Headless audio-health capture did not prove SDL dummy audio'
}
if ($stderrText -notmatch
    '\[Host\] audio capture: .* tap=pre-volume') {
    throw 'Audio-health capture did not prove the pre-volume tap'
}
if ($stderrText -match 'Unhandled exception|Fatal error') {
    throw 'Audio-health capture logged a runtime failure'
}
if (-not (Test-Path -LiteralPath $wav -PathType Leaf)) {
    throw "Audio-health capture did not create $wav"
}
if ((Get-Item -LiteralPath $wav).Length -gt 25MB) {
    throw 'Temporary PCM capture exceeded the 25 MiB safety ceiling'
}

$queueFailures = [Collections.Generic.List[string]]::new()
$sourceFailures = [Collections.Generic.List[string]]::new()
foreach ($line in ($stderrText -split "`r?`n")) {
    if ($line -match
        '^\[AUDIO-QUEUE\] poll=(\d+).*starvations=(\d+)') {
        $poll = [int]$Matches[1]
        $starvations = [int]$Matches[2]
        if ($poll -ge $StartPoll -and $poll -lt $EndPoll -and
            $starvations -ne 0) {
            $queueFailures.Add($line)
        }
    }
    if ($line -match
        '^\[AUDIO\] poll=(\d+).*xaUnderflow=(\d+).*xaRefills=(\d+)') {
        $poll = [int]$Matches[1]
        $underflows = [int64]$Matches[2]
        $refills = [int]$Matches[3]
        if ($poll -ge $StartPoll -and $poll -lt $EndPoll -and
            ($underflows -ne 0 -or $refills -ne 0)) {
            $sourceFailures.Add($line)
        }
    }
}
if ($queueFailures.Count -ne 0) {
    throw "SDL queue starvation in capture window:`n$($queueFailures -join "`n")"
}
if ($sourceFailures.Count -ne 0) {
    throw "XA source underflow in capture window:`n$($sourceFailures -join "`n")"
}

& ffmpeg -hide_banner -loglevel warning -y -i $wav -c:a flac $flac
if ($LASTEXITCODE -ne 0 -or -not (Test-Path -LiteralPath $flac)) {
    throw 'ffmpeg failed to encode the bounded FLAC evidence'
}
$silenceStart = [Diagnostics.ProcessStartInfo]::new()
$silenceStart.FileName = (Get-Command ffmpeg).Source
$silenceStart.UseShellExecute = $false
$silenceStart.CreateNoWindow = $true
$silenceStart.RedirectStandardOutput = $true
$silenceStart.RedirectStandardError = $true
$quotedWav = '"' + $wav.Replace('"', '\"') + '"'
$silenceStart.Arguments =
    "-hide_banner -i $quotedWav " +
    "-af silencedetect=noise=-70dB:d=0.020 -f null NUL"
$silenceProcess = [Diagnostics.Process]::Start($silenceStart)
$silenceStdout = $silenceProcess.StandardOutput.ReadToEndAsync()
$silenceStderr = $silenceProcess.StandardError.ReadToEndAsync()
$silenceProcess.WaitForExit()
$silenceText = "$($silenceStdout.Result)`n$($silenceStderr.Result)"
[IO.File]::WriteAllText($silenceLog, $silenceText)
if ($silenceProcess.ExitCode -ne 0) {
    throw 'ffmpeg silence analysis failed'
}
$longSilences = [Collections.Generic.List[string]]::new()
foreach ($match in [regex]::Matches(
        $silenceText, 'silence_duration: ([0-9.]+)')) {
    if ([double]$match.Groups[1].Value -ge 0.100) {
        $longSilences.Add($match.Value)
    }
}

# The WAV is a generated intermediate. Keep only losslessly compressed evidence.
$wavFull = [IO.Path]::GetFullPath($wav)
$artifactFull = [IO.Path]::GetFullPath($artifact).TrimEnd('\') + '\'
if (-not $wavFull.StartsWith(
        $artifactFull, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Refusing to remove PCM outside the artifact directory: $wavFull"
}
Remove-Item -LiteralPath $wavFull -Force

$settingsHashAfter = (Get-FileHash -LiteralPath $settings -Algorithm SHA256).Hash
if ($settingsHashAfter -ne $settingsHashBefore) {
    throw 'Headless audio-health capture changed persistent audio settings'
}
$probe = & ffprobe -v error -show_entries `
    format=duration,size:stream=codec_name,sample_rate,channels `
    -of default=noprint_wrappers=1 $flac
if ($LASTEXITCODE -ne 0) {
    throw 'ffprobe failed on the FLAC evidence'
}

Write-Output "artifact=$artifact"
$audioSafety = if ($VisibleMuted) {
    'session-muted physical driver'
} else {
    'headless dummy'
}
Write-Output "poll-window=$StartPoll-$EndPoll audio=$audioSafety tap=pre-volume"
Write-Output 'SDL-starvation=0 XA-underflow=0 XA-refills=0'
Write-Output "silences-at-least-100ms=$($longSilences.Count)"
Write-Output $probe
Write-Output "settings-unchanged=$settingsHashAfter"
