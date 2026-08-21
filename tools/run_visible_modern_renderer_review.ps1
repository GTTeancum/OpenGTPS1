param(
    # Stock GT2 content is the default review path. Converted tracks remain
    # available only through an explicit fixture override after stock-track
    # acceptance is complete.
    [ValidateSet('Arcade', 'Simulation')]
    [string]$Mode = 'Simulation',
    [string]$Scenario = '',
    [int]$ExitPoll = 12000,
    [int]$TimeoutSeconds = 360,
    [string]$DeployPath = 'tools\unified-host\bin\Release\net10.0\win-x64\publish',
    [string]$DataPath = 'work\gt2-unified',
    [string]$Fixture =
        'tests\fixtures\modern-renderer-replay-soak.input',
    [string]$ArtifactName = 'modern-renderer-final-visible-red-rock-review',
    [switch]$Muted,
    [switch]$CaptureEvidence,
    [switch]$AboveNormalPriority
)
$arcade = $Mode -eq 'Arcade'
if ([string]::IsNullOrWhiteSpace($Scenario)) {
    $Scenario = if ($arcade) { 'TahitiRoad' } else { 'RedRock' }
}

$ErrorActionPreference = 'Stop'
try {
    (Get-Process -Id $PID).PriorityClass =
        $(if ($AboveNormalPriority) { 'AboveNormal' } else { 'BelowNormal' })
} catch {}
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$deploy = (Resolve-Path (Join-Path $repo $DeployPath)).Path
$exe = Join-Path $deploy 'GranTurismo2PC.exe'
$data = (Resolve-Path (Join-Path $repo $DataPath)).Path
$fixturePath = (Resolve-Path (Join-Path $repo $Fixture)).Path
$artifact = Join-Path $repo "artifacts\$ArtifactName"
New-Item -ItemType Directory -Path $artifact -Force | Out-Null
$stdout = Join-Path $artifact 'stdout.log'
$stderr = Join-Path $artifact 'stderr.log'
$cards = @('carda.sav', 'cardb.sav') | ForEach-Object {
    Join-Path $deploy $_
}
$cardBackups = @{}
$cardHashes = @{}
foreach ($card in $cards) {
    if (Test-Path -LiteralPath $card -PathType Leaf) {
        $backup = Join-Path $artifact ((Split-Path -Leaf $card) + '.before')
        Copy-Item -LiteralPath $card -Destination $backup -Force
        $cardBackups[$card] = $backup
        $cardHashes[$card] = (Get-FileHash -LiteralPath $card -Algorithm SHA256).Hash
    }
}
$launchTime = Get-Date

$environment = [ordered]@{
    SDL_AUDIODRIVER = $(if ($Muted) { 'dummy' } else { $null })
    RECOMPONE_INPUT_FILE = $fixturePath
    RECOMPONE_DISABLE_LIVE_INPUT = '1'
    RECOMPONE_SUPPRESS_RUMBLE = '1'
    RECOMPONE_GT2_AI_AUTODRIVE = '1'
    RECOMPONE_GT2_AI_AUTODRIVE_MAX_ENGAGEMENTS = '2'
    RECOMPONE_GT2_CREATE_TEST_SAVE = $(if ($arcade) { $null } else { '1' })
    RECOMPONE_GRAPHICS_PRESET_OVERRIDE = 'Enhanced'
    RECOMPONE_EXIT_AFTER_INPUT_POLL = $ExitPoll.ToString()
    RECOMPONE_TRACE_PERFORMANCE = '1'
    RECOMPONE_NATIVE_WORLD_TRACE_INTERVAL = '300'
    RECOMPONE_WINDOW_VISIBLE = '1'
    RECOMPONE_PROCESS_PRIORITY =
        $(if ($AboveNormalPriority) { 'AboveNormal' } else { 'BelowNormal' })
    # Full-resolution OpenGL readback is intentionally opt-in. It is useful
    # for static visual evidence but can stall a visible frame; the default
    # user smoothness review must measure the uninstrumented shipping path.
    RECOMPONE_PRESENTATION_CAPTURE = $(if ($CaptureEvidence) { '1' } else { $null })
    RECOMPONE_UNTHROTTLED = $null
}
$previous = @{}
$process = $null
$timedOut = $false
$exitCode = $null
try {
    foreach ($entry in $environment.GetEnumerator()) {
        $previous[$entry.Key] = [Environment]::GetEnvironmentVariable(
            $entry.Key,
            [EnvironmentVariableTarget]::Process)
        [Environment]::SetEnvironmentVariable(
            $entry.Key,
            $entry.Value,
            [EnvironmentVariableTarget]::Process)
    }
    $arguments = @($data)
    if ($arcade) {
        $arguments += '--start-arcade'
    }
    if ($Muted) {
        $arguments += '--mute'
    }
    $process = Start-Process `
        -FilePath $exe `
        -WorkingDirectory $deploy `
        -ArgumentList $arguments `
        -WindowStyle Maximized `
        -RedirectStandardOutput $stdout `
        -RedirectStandardError $stderr `
        -PassThru
    try {
        $process.PriorityClass =
            $(if ($AboveNormalPriority) { 'AboveNormal' } else { 'BelowNormal' })
    } catch {}

    Write-Output (
        "visible_modern_review=launched pid=$($process.Id) " +
        "scenario=$Scenario audio=$(if ($Muted) { 'muted' } else { 'audible' }) " +
        "exit_poll=$ExitPoll artifact=$artifact")

    if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
        $timedOut = $true
        Stop-Process -Id $process.Id -Force
        $process.WaitForExit()
        $exitCode = -1
    } else {
        # Complete redirected-stream draining and refresh the native process
        # handle before reading ExitCode. Windows PowerShell can otherwise
        # expose a blank property even after WaitForExit(timeout) succeeds.
        $process.WaitForExit()
        $process.Refresh()
        $exitCode = [int]$process.ExitCode
    }
} finally {
    foreach ($entry in $previous.GetEnumerator()) {
        [Environment]::SetEnvironmentVariable(
            $entry.Key,
            $entry.Value,
            [EnvironmentVariableTarget]::Process)
    }
    foreach ($entry in $cardBackups.GetEnumerator()) {
        Copy-Item -LiteralPath $entry.Value -Destination $entry.Key -Force
    }
}

foreach ($card in $cardBackups.Keys) {
    $restoredHash = (Get-FileHash -LiteralPath $card -Algorithm SHA256).Hash
    if ($restoredHash -ne $cardHashes[$card]) {
        throw "Memory-card restore hash mismatch: $card"
    }
}

if ($CaptureEvidence) {
    Get-ChildItem -LiteralPath $deploy -Filter 'recompone_present_*.ppm' |
        Where-Object { $_.LastWriteTime -ge $launchTime.AddSeconds(-1) } |
        Copy-Item -Destination $artifact -Force
}

if ($timedOut) {
    throw "Visible $Scenario review exceeded ${TimeoutSeconds}s"
}
if ($null -eq $process -or $exitCode -ne 0) {
    throw "Visible $Scenario review exited with code $exitCode"
}
$log = Get-Content -LiteralPath $stderr -Raw
if ($log -notmatch "\[Input\] stage 'race_1'" -or
    $log -notmatch 'auto-drive engaged pass=1/2 phase=race' -or
    $log -notmatch '\[Runtime\] shutdown complete; exit=0') {
    throw "Visible $Scenario review did not reach auto-drive and exit cleanly"
}
if ($log -match
    'Unhandled exception|Fatal error|unknown software exception|' +
    '\[Native-World\] disabled:|submission rejected|dropped truncated|' +
    'compositor fallback viewport=|unmapped call') {
    throw "Visible $Scenario review logged a renderer/runtime failure"
}
$raceLog = $log.Substring($log.IndexOf("[Input] stage 'race_1'"))
$windowPattern =
    '\[Native-Present\] hostHz=([0-9.]+) uniqueHz=([0-9.]+) ' +
    'new=([0-9]+) actual=([0-9]+) synthetic=([0-9]+) ' +
    'repeated=([0-9]+) compositor=([0-9]+) worldMiss=([0-9]+) ' +
    'transitionHold=([0-9]+)'
$windows = @([regex]::Matches($raceLog, $windowPattern))
$measuredWindows = @($windows | Select-Object -Skip 1)
$firstSteadyIndex = -1
for ($index = 0; $index -lt $measuredWindows.Count; $index++) {
    $window = $measuredWindows[$index]
    $hostHz = [double]$window.Groups[1].Value
    $uniqueHz = [double]$window.Groups[2].Value
    if ($hostHz -ge 59.5 -and $hostHz -le 60.5 -and
        $uniqueHz -ge 59.5 -and $uniqueHz -le 60.5 -and
        $window.Groups[3].Value -eq '300' -and
        $window.Groups[4].Value -eq '150' -and
        $window.Groups[5].Value -eq '150' -and
        $window.Groups[6].Value -eq '0' -and
        $window.Groups[7].Value -eq '0' -and
        $window.Groups[8].Value -eq '0' -and
        $window.Groups[9].Value -eq '0') {
        $firstSteadyIndex = $index
        break
    }
}
if ($firstSteadyIndex -lt 0) {
    throw "Visible $Scenario review never reached a perfect steady race window"
}
$steadyWindows = @($measuredWindows | Select-Object -Skip $firstSteadyIndex)
if ($steadyWindows.Count -lt 15) {
    throw "Visible $Scenario review recorded only $($steadyWindows.Count) steady race windows"
}
$badWindows = @($steadyWindows | Where-Object {
    $hostHz = [double]$_.Groups[1].Value
    $uniqueHz = [double]$_.Groups[2].Value
    return $hostHz -lt 59.5 -or $hostHz -gt 60.5 -or
        $uniqueHz -lt 59.5 -or $uniqueHz -gt 60.5 -or
        $_.Groups[3].Value -ne '300' -or
        $_.Groups[4].Value -ne '150' -or
        $_.Groups[5].Value -ne '150' -or
        $_.Groups[6].Value -ne '0' -or
        $_.Groups[7].Value -ne '0' -or
        $_.Groups[8].Value -ne '0' -or
        $_.Groups[9].Value -ne '0'
})
if ($badWindows.Count -ne 0) {
    throw (
        "Visible $Scenario review retained $($badWindows.Count) non-perfect " +
        'steady race window(s): ' +
        (($badWindows | ForEach-Object { $_.Value.Trim() }) -join ' | '))
}

$captures = @()
if ($CaptureEvidence) {
    $captures = @(
        Get-ChildItem -LiteralPath $artifact -Filter 'recompone_present_*.ppm')
    if ($captures.Count -lt 5) {
        throw "Visible $Scenario review saved only $($captures.Count) presentation captures"
    }
    $undersized = @($captures | Where-Object {
        if ($_.Name -notmatch '_(\d+)x(\d+)_') { return $true }
        return [int]$Matches[1] -lt 640 -or [int]$Matches[2] -lt 480
    })
    if ($undersized.Count -ne 0) {
        throw (
            "Visible $Scenario review produced undersized captures: " +
            (($undersized | Select-Object -ExpandProperty Name) -join ', '))
    }
}
Write-Output (
    "visible_modern_review=passed steady_windows=$($steadyWindows.Count) " +
    "captures=$($captures.Count) " +
    "cards_restored=true artifact=$artifact")
