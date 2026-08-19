param(
    [ValidateSet('Arcade', 'Simulation')]
    [string]$Mode,
    [ValidateSet('Default', 'SSR11', 'SupraTahiti', 'Replay', 'ReplayExit')]
    [string]$Scenario = 'Default',
    [string]$ArtifactName = '',
    [int]$TimeoutSeconds = 240,
    [int]$CaptureFrames = 1800,
    [int]$VideoStartPoll = -1,
    [string]$DeployPath = 'tools\unified-host\bin\Release\net10.0',
    [string]$DataPath = 'work\gt2-unified',
    [string]$FixtureOverride = '',
    [int]$VideoWidth = 640,
    [int]$VideoHeight = 480
)

$ErrorActionPreference = 'Stop'
if ($IsWindows -or $env:OS -eq 'Windows_NT') {
    try {
        [Diagnostics.Process]::GetCurrentProcess().PriorityClass =
            [Diagnostics.ProcessPriorityClass]::BelowNormal
    } catch {
        Write-Warning (
            "Could not lower proof-script priority: $($_.Exception.Message)")
    }
}
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
function Resolve-RepoPath([string]$Path) {
    if ([IO.Path]::IsPathRooted($Path)) {
        return (Resolve-Path -LiteralPath $Path).Path
    }
    return (Resolve-Path -LiteralPath (Join-Path $repo $Path)).Path
}

$deploy = Resolve-RepoPath $DeployPath
$data = Resolve-RepoPath $DataPath
$exe = Join-Path $deploy 'GranTurismo2PC.exe'
$card = Join-Path $deploy 'carda.sav'
$arcade = if ($Scenario -eq 'SSR11') { $true } else { $Mode -eq 'Arcade' }
$fixtureRelative = switch ($Scenario) {
    'SSR11' {
        'tests\fixtures\unified-arcade-roadster-rs-ssr11-night-race.input'
    }
    'SupraTahiti' {
        'tests\fixtures\unified-arcade-supra-rz-race.input'
    }
    { $_ -in 'Replay', 'ReplayExit' } {
        'tests\fixtures\modern-renderer-replay-soak.input'
    }
    default {
        if ($arcade) {
            'tests\fixtures\arcade-modern-renderer-soak-resilient.input'
        } else {
            'tests\fixtures\modern-renderer-replay-soak.input'
        }
    }
}
$fixture = Resolve-RepoPath $fixtureRelative
if (-not [string]::IsNullOrWhiteSpace($FixtureOverride)) {
    $fixture = Resolve-RepoPath $FixtureOverride
}
$videoStart = switch ($Scenario) {
    'SSR11' { 7550 }
    'SupraTahiti' { 7900 }
    'Replay' { 16500 }
    'ReplayExit' { 22600 }
    default { if ($arcade) { 5000 } else { 8400 } }
}
if ($VideoStartPoll -ge 0) {
    $videoStart = $VideoStartPoll
}
$requestedVideoFrames = if ($Scenario -eq 'ReplayExit') { 1810 } else { $CaptureFrames }
# Exact captures stop on presentations written, not input polls. During native
# output stalls a poll can legitimately produce no presentation, so a poll-
# based video end can truncate a long capture even though the game keeps going.
$videoEnd = 0
# This remains only a hang failsafe. Successful runs exit as soon as the exact
# frame limit closes ffmpeg, well before this deliberately loose boundary.
$exitPoll = $videoStart + ($requestedVideoFrames * 2) + 600
$slug = $Mode.ToLowerInvariant()
$scenarioSlug = if ($Scenario -eq 'Default') {
    ''
} else {
    '-' + $Scenario.ToLowerInvariant()
}
if ([string]::IsNullOrWhiteSpace($ArtifactName)) {
    $ArtifactName = "modern-renderer-final-motion-$slug$scenarioSlug"
}
$artifact = Join-Path $repo "artifacts\$ArtifactName"
New-Item -ItemType Directory -Path $artifact -Force | Out-Null
$video = Join-Path $artifact "GT2_Modern_Final_${Mode}${scenarioSlug}.mp4"
$stdoutPath = Join-Path $artifact 'stdout.log'
$stderrPath = Join-Path $artifact 'stderr.log'
$cardBackup = Join-Path $artifact 'carda.before.sav'
$frameMd5 = Join-Path $artifact 'frames.framemd5'
$contactSheet = Join-Path $artifact 'contact-sheet.png'
$motionSheet = Join-Path $artifact 'motion-sheet.png'
$adjacentSsim = Join-Path $artifact 'adjacent-ssim.log'
$signalStats = Join-Path $artifact 'signalstats.log'
$worldBandSignalStats = Join-Path $artifact 'world-band-signalstats.log'
$temporalOutliers = Join-Path $artifact 'temporal-outliers.png'

foreach ($required in @($exe, $card, $fixture)) {
    if (-not (Test-Path -LiteralPath $required -PathType Leaf)) {
        throw "Required file is missing: $required"
    }
}
Copy-Item -LiteralPath $card -Destination $cardBackup -Force
$cardHashBefore = (Get-FileHash -LiteralPath $card -Algorithm SHA256).Hash

$start = [Diagnostics.ProcessStartInfo]::new()
$start.FileName = $exe
$start.WorkingDirectory = $deploy
$start.Arguments =
    '--headless --mute ' +
    $(if ($arcade) { '--start-arcade ' } else { '' }) +
    '"' + $data.Replace('"', '\"') + '"'
$start.UseShellExecute = $false
$start.CreateNoWindow = $true
$start.RedirectStandardOutput = $true
$start.RedirectStandardError = $true
$environment = [ordered]@{
    SDL_AUDIODRIVER = 'dummy'
    RECOMPONE_PROCESS_PRIORITY = 'BelowNormal'
    RECOMPONE_INPUT_FILE = $fixture
    RECOMPONE_DISABLE_LIVE_INPUT = '1'
    RECOMPONE_SUPPRESS_RUMBLE = '1'
    RECOMPONE_GT2_AI_AUTODRIVE = '1'
    RECOMPONE_GT2_AI_AUTODRIVE_MAX_ENGAGEMENTS = '2'
    RECOMPONE_GT2_SOAK_QUICK_WIN_AFTER_AI_TICKS = $(
        if ($Scenario -in 'Replay', 'ReplayExit') { '600' } else { $null })
    RECOMPONE_GT2_CREATE_TEST_SAVE = $(if ($arcade) { $null } else { '1' })
    RECOMPONE_GRAPHICS_PRESET_OVERRIDE = 'Enhanced'
    RECOMPONE_EXIT_AFTER_INPUT_POLL = $exitPoll.ToString()
    RECOMPONE_UNTHROTTLED = '1'
    RECOMPONE_THROTTLE_ON_SCRIPT_STAGE = $(
        if ($Scenario -in 'Replay', 'ReplayExit') {
            'replay_1'
        } else {
            'race_1'
        })
    RECOMPONE_TRACE_PERFORMANCE = '1'
    RECOMPONE_NATIVE_WORLD_TRACE_INTERVAL = '300'
    RECOMPONE_VIDEO_CAPTURE = $video
    RECOMPONE_EXIT_AFTER_VIDEO_CAPTURE = '1'
    RECOMPONE_VIDEO_START_INPUT_POLL = $videoStart.ToString()
    RECOMPONE_VIDEO_END_INPUT_POLL = $videoEnd.ToString()
    RECOMPONE_VIDEO_CAPTURE_FRAMES = $requestedVideoFrames.ToString()
    RECOMPONE_VIDEO_WIDTH = $VideoWidth.ToString()
    RECOMPONE_VIDEO_HEIGHT = $VideoHeight.ToString()
    RECOMPONE_VIDEO_FPS = '60'
}
foreach ($entry in $environment.GetEnumerator()) {
    if ($null -ne $entry.Value) {
        $start.Environment[$entry.Key] = $entry.Value
    }
}

$process = $null
$stdout = ''
$stderr = ''
$timedOut = $false
try {
    $process = [Diagnostics.Process]::Start($start)
    $stdoutTask = $process.StandardOutput.ReadToEndAsync()
    $stderrTask = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
        $timedOut = $true
        # Windows PowerShell exposes the .NET Framework Process API, which
        # lacks Kill(bool). The host does not create a child process until
        # video encoding, and this timeout occurs before finalization.
        $process.Kill()
        $process.WaitForExit()
    }
    $stdout = $stdoutTask.Result
    $stderr = $stderrTask.Result
} finally {
    [IO.File]::WriteAllText($stdoutPath, $stdout)
    [IO.File]::WriteAllText($stderrPath, $stderr)
    Copy-Item -LiteralPath $cardBackup -Destination $card -Force
}

$cardHashAfter = (Get-FileHash -LiteralPath $card -Algorithm SHA256).Hash
if ($cardHashAfter -ne $cardHashBefore) {
    throw 'Memory-card restore hash mismatch'
}
if ($timedOut) {
    throw "$Mode motion capture exceeded ${TimeoutSeconds}s"
}
if ($process.ExitCode -ne 0 -or
    $stderr -notmatch '\[Runtime\] shutdown complete; exit=0') {
    throw "$Mode motion capture did not exit cleanly"
}
if ($stderr -notmatch "\[Input\] stage 'race_1'" -or
    $stderr -notmatch '\[PERF\] throttle engaged at scripted stage') {
    throw "$Mode motion capture did not reach a real-time race"
}
if ($stderr -notmatch [regex]::Escape($fixture)) {
    throw "$Mode/$Scenario motion capture did not load the requested fixture"
}
if ($Scenario -in 'Replay', 'ReplayExit' -and
    $stderr -notmatch "\[Input\] stage 'replay_1'") {
    throw 'Replay motion capture did not reach the natural replay controller'
}
if ($stderr -notmatch '\[Host\] video capture complete.*ffmpeg exit=0' -or
    -not (Test-Path -LiteralPath $video -PathType Leaf)) {
    throw "$Mode video did not finalize successfully"
}
if ($stderr -notmatch
    "\[Host\] video capture complete.*frames=$requestedVideoFrames/$requestedVideoFrames") {
    throw "$Mode video recorder did not write the exact requested frame count"
}
if ($stderr -match
    'Unhandled exception|Fatal error|unknown software exception|' +
    '\[Native-World\] disabled:|submission rejected|dropped truncated|' +
    'compositor fallback viewport=|unmapped call') {
    throw "$Mode motion capture logged a renderer/runtime failure"
}

if ($Scenario -eq 'ReplayExit') {
    # Recorder startup crosses a process/thread boundary and can begin three or
    # four input polls after the request. Capture a short deterministic tail,
    # then normalize to exactly the first 1,800 presentations so the proof is
    # always 30.000 seconds rather than accepting a 1,799-frame near miss.
    $normalizedVideo = Join-Path $artifact 'replayexit-normalized.mp4'
    & ffmpeg -hide_banner -loglevel error -y -i $video `
        -vf 'trim=end_frame=1800,setpts=PTS-STARTPTS' -an -r 60 `
        -c:v libx264 -preset medium -crf 12 -pix_fmt yuv420p $normalizedVideo
    if ($LASTEXITCODE -ne 0) { throw 'ReplayExit frame normalization failed' }
    Move-Item -LiteralPath $normalizedVideo -Destination $video -Force
}

& ffmpeg -hide_banner -loglevel error -y -i $video -f framemd5 $frameMd5
if ($LASTEXITCODE -ne 0) { throw 'framemd5 extraction failed' }
$hashes = Get-Content -LiteralPath $frameMd5 |
    Where-Object { $_ -notmatch '^#' } |
    ForEach-Object { ($_ -split ',')[-1].Trim() }
$adjacentDuplicates = 0
for ($index = 1; $index -lt $hashes.Count; ++$index) {
    if ($hashes[$index] -eq $hashes[$index - 1]) {
        $adjacentDuplicates++
    }
}
$wholeVideoUnique = @($hashes | Sort-Object -Unique).Count
$validatedFrameCount = $hashes.Count
$validatedUnique = $wholeVideoUnique
$validatedAdjacentDuplicates = $adjacentDuplicates
if ($Scenario -eq 'ReplayExit') {
    # Replay relinquishes modern-world ownership at absolute poll 23,340. The
    # remaining frames are authored world-free Results/loading composition and
    # may be intentionally static. The first 740 presentations, from poll
    # 22,600 through the final replay-owned vblank, must remain wholly unique.
    $validatedFrameCount = 740
    $validatedHashes = @($hashes | Select-Object -First $validatedFrameCount)
    $validatedUnique = @($validatedHashes | Sort-Object -Unique).Count
    $validatedAdjacentDuplicates = 0
    for ($index = 1; $index -lt $validatedHashes.Count; ++$index) {
        if ($validatedHashes[$index] -eq $validatedHashes[$index - 1]) {
            $validatedAdjacentDuplicates++
        }
    }
}
$expectedFrameCount = if ($Scenario -eq 'ReplayExit') { 1810 } else { $CaptureFrames }
if ($hashes.Count -ne $expectedFrameCount -or
    $validatedUnique -ne $validatedFrameCount -or
    $validatedAdjacentDuplicates -ne 0) {
    throw (
        "Expected unique validated video frames; " +
        "frames=$($hashes.Count) " +
        "validated=$validatedUnique/$validatedFrameCount " +
        "validatedAdjacentDuplicates=$validatedAdjacentDuplicates " +
        "wholeVideoUnique=$wholeVideoUnique " +
        "wholeVideoAdjacentDuplicates=$adjacentDuplicates")
}

# Preserve the full 640x480 proof frame in every tile. Small 320x240 tiles
# concealed narrow road seams, wheel faults, and single-prop pop-in during
# review even though the encoded motion proof itself was 640x480.
& ffmpeg -hide_banner -loglevel error -y -i $video `
    -vf 'fps=0.5,scale=640:480:flags=neighbor,tile=5x3' `
    -frames:v 1 $contactSheet
if ($LASTEXITCODE -ne 0) { throw 'contact-sheet generation failed' }
& ffmpeg -hide_banner -loglevel error -y -ss 10 -t 4 -i $video `
    -vf 'fps=6,scale=640:480:flags=neighbor,tile=6x4' `
    -frames:v 1 $motionSheet
if ($LASTEXITCODE -ne 0) { throw 'motion-sheet generation failed' }

$ssimPath = $adjacentSsim.Replace('\', '/').Replace(':', '\:')
$ssimPairCount = $expectedFrameCount - 1
& ffmpeg -hide_banner -loglevel error -y -i $video `
    -filter_complex (
        '[0:v]split=2[a][b];' +
        "[a]trim=end_frame=$ssimPairCount,setpts=PTS-STARTPTS[a0];" +
        '[b]trim=start_frame=1,setpts=PTS-STARTPTS[b0];' +
        "[a0][b0]ssim=stats_file='$ssimPath'") `
    -f null NUL
if ($LASTEXITCODE -ne 0) { throw 'adjacent-frame SSIM failed' }
$ssimEntries = @(
    Get-Content -LiteralPath $adjacentSsim |
        ForEach-Object {
            if ($_ -match '^n:(\d+).*All:([0-9.]+)') {
                [pscustomobject]@{
                    Frame = [int]$Matches[1]
                    Value = [double]::Parse(
                        $Matches[2],
                        [Globalization.CultureInfo]::InvariantCulture)
                }
            }
        })
if ($ssimEntries.Count -ne $ssimPairCount) {
    throw "Expected $ssimPairCount adjacent SSIM pairs; got $($ssimEntries.Count)"
}
$orderedSsim = @($ssimEntries | Sort-Object Value)
$ssimP1 = $orderedSsim[
    [math]::Floor(($orderedSsim.Count - 1) * 0.01)].Value
$ssimMedian = $orderedSsim[
    [math]::Floor(($orderedSsim.Count - 1) * 0.50)].Value

# Retain per-frame luminance alongside SSIM. A wrapped SSR11 street-light
# polygon produced a one-frame full-height streak that was much easier to
# distinguish as a global luminance spike than as a low SSIM value alone.
$signalStatsPath = $signalStats.Replace('\', '/').Replace(':', '\:')
& ffmpeg -hide_banner -loglevel error -y -i $video `
    -vf "signalstats,metadata=print:file='$signalStatsPath'" -f null NUL
if ($LASTEXITCODE -ne 0) { throw 'signalstats extraction failed' }
$luma = @(
    Get-Content -LiteralPath $signalStats |
        ForEach-Object {
            if ($_ -match 'lavfi.signalstats.YAVG=([0-9.]+)') {
                [double]::Parse(
                    $Matches[1],
                    [Globalization.CultureInfo]::InvariantCulture)
            }
        })
if ($luma.Count -ne $expectedFrameCount) {
    throw "Expected $expectedFrameCount luminance samples; got $($luma.Count)"
}
$lumaDeltas = for ($index = 1; $index -lt $luma.Count; ++$index) {
    [math]::Abs($luma[$index] - $luma[$index - 1])
}
$maximumLumaDelta = ($lumaDeltas | Measure-Object -Maximum).Maximum
if ($Scenario -eq 'SSR11' -and $maximumLumaDelta -ge 10.0) {
    throw "SSR11 motion contains a street-light-sized luminance spike: $maximumLumaDelta"
}
if ($Scenario -eq 'SSR11') {
    $worldBandPath = $worldBandSignalStats.Replace('\', '/').Replace(':', '\:')
    & ffmpeg -hide_banner -loglevel error -y -i $video `
        -vf "crop=320:120:0:80,signalstats,metadata=print:file='$worldBandPath'" `
        -f null NUL
    if ($LASTEXITCODE -ne 0) { throw 'SSR11 world-band analysis failed' }
    $worldBandLuma = @(
        Get-Content -LiteralPath $worldBandSignalStats |
            ForEach-Object {
                if ($_ -match 'lavfi.signalstats.YAVG=([0-9.]+)') {
                    [double]::Parse(
                        $Matches[1],
                        [Globalization.CultureInfo]::InvariantCulture)
                }
            })
    $hudOnlyFrames = @($worldBandLuma | Where-Object { $_ -lt 25.0 }).Count
    if ($worldBandLuma.Count -ne $expectedFrameCount -or $hudOnlyFrames -ne 0) {
        throw (
            "SSR11 motion contains HUD-only world dropouts: " +
            "samples=$($worldBandLuma.Count)/$expectedFrameCount " +
            "dropouts=$hudOnlyFrames")
    }
}
$outlierFrames = @(
    $orderedSsim | Select-Object -First 12 |
        ForEach-Object { $_.Frame } | Sort-Object)
$selectExpression = @(
    $outlierFrames | ForEach-Object { "eq(n\,$_)" }) -join '+'
& ffmpeg -hide_banner -loglevel error -y -i $video `
    -vf "select='$selectExpression',scale=640:480:flags=neighbor,tile=4x3" `
    -frames:v 1 $temporalOutliers
if ($LASTEXITCODE -ne 0) { throw 'temporal-outlier sheet failed' }

Write-Output (
    "modern_motion=pass mode=$Mode scenario=$Scenario " +
    "frames=$($hashes.Count) " +
    "validated_unique=$validatedUnique/$validatedFrameCount " +
    "validated_adjacent_duplicates=$validatedAdjacentDuplicates " +
    "whole_video_unique=$wholeVideoUnique " +
    "whole_video_adjacent_duplicates=$adjacentDuplicates " +
    "ssim_min=$($orderedSsim[0].Value) ssim_p1=$ssimP1 " +
    "ssim_median=$ssimMedian luma_delta_max=$maximumLumaDelta " +
    "card_hash=$cardHashAfter artifact=$artifact")
