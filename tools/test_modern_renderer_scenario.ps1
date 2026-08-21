param(
    [ValidateSet('TahitiRoad', 'RedRock', 'SSR11', 'SupraTahiti')]
    [string]$Scenario,
    [string]$ArtifactName = '',
    [int]$TimeoutSeconds = 240,
    [switch]$AboveNormalPriority,
    [switch]$HighPriority,
    [double]$MaximumExternal3dPercent = 100.0,
    [string]$DeployPath = 'tools\unified-host\bin\Release\net10.0\win-x64\publish',
    [string]$DataPath = 'work\gt2-unified'
)

$ErrorActionPreference = 'Stop'
if ($AboveNormalPriority -and $HighPriority) {
    throw 'Choose either AboveNormalPriority or HighPriority, not both'
}
if ($IsWindows -or $env:OS -eq 'Windows_NT') {
    [Diagnostics.Process]::GetCurrentProcess().PriorityClass =
        [Diagnostics.ProcessPriorityClass]::BelowNormal
}
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
function Resolve-RepoPath([string]$Path) {
    if ([IO.Path]::IsPathRooted($Path)) {
        return (Resolve-Path -LiteralPath $Path).Path
    }
    return (Resolve-Path -LiteralPath (Join-Path $repo $Path)).Path
}

function Get-External3dUse([int]$ExcludedPid = 0) {
    $results = @()
    $counter = Get-Counter `
        '\GPU Engine(*)\Utilization Percentage' `
        -ErrorAction SilentlyContinue
    if ($null -eq $counter) {
        return $results
    }
    foreach ($sample in $counter.CounterSamples) {
        if ($sample.CookedValue -le 0 -or
            $sample.InstanceName -notmatch 'pid_(\d+).*engtype_3d$') {
            continue
        }
        $pidValue = [int]$Matches[1]
        if ($pidValue -eq $ExcludedPid) {
            continue
        }
        $name = try {
            (Get-Process -Id $pidValue -ErrorAction Stop).ProcessName
        } catch {
            "pid-$pidValue"
        }
        if ($name -notin @('dwm', 'System', 'Idle')) {
            $results += [pscustomobject]@{
                pid = $pidValue
                process = $name
                utilizationPercent = [math]::Round(
                    $sample.CookedValue, 3)
            }
        }
    }
    return $results
}

function Format-External3dUse([object[]]$Use) {
    return (($Use | ForEach-Object {
        "$($_.process)[$($_.pid)]=$($_.utilizationPercent)%"
    }) -join ', ')
}

$fixtureRelative = switch ($Scenario) {
    'TahitiRoad' { 'tests\fixtures\arcade-modern-renderer-soak-resilient.input' }
    'RedRock' { 'tests\fixtures\modern-renderer-replay-soak.input' }
    'SSR11' { 'tests\fixtures\unified-arcade-roadster-rs-ssr11-night-race.input' }
    'SupraTahiti' { 'tests\fixtures\unified-arcade-supra-rz-race.input' }
}
$exitPoll = switch ($Scenario) {
    'TahitiRoad' { 10500 }
    default { 12500 }
}
$arcade = $Scenario -ne 'RedRock'
$deploy = Resolve-RepoPath $DeployPath
$data = Resolve-RepoPath $DataPath
$fixture = Resolve-RepoPath $fixtureRelative
$exe = Join-Path $deploy 'GranTurismo2PC.exe'
$card = Join-Path $deploy 'carda.sav'
foreach ($required in @($exe, $card, $fixture)) {
    if (-not (Test-Path -LiteralPath $required -PathType Leaf)) {
        throw "Required scenario file is missing: $required"
    }
}

$external3d = @(Get-External3dUse)
$external3dTotal = @($external3d | Measure-Object `
    utilizationPercent -Sum).Sum
if ($null -eq $external3dTotal) {
    $external3dTotal = 0.0
}
if ($external3dTotal -gt $MaximumExternal3dPercent) {
    throw (
        "External 3D use is $([math]::Round($external3dTotal, 3))%, " +
        "above the $MaximumExternal3dPercent% scenario limit: " +
        (Format-External3dUse $external3d))
}

if ([string]::IsNullOrWhiteSpace($ArtifactName)) {
    $ArtifactName = "modern-renderer-scenario-$($Scenario.ToLowerInvariant())"
}
$artifact = Join-Path $repo "artifacts\$ArtifactName"
New-Item -ItemType Directory -Path $artifact -Force | Out-Null
$stdoutPath = Join-Path $artifact 'stdout.log'
$stderrPath = Join-Path $artifact 'stderr.log'
$cardBackup = Join-Path $artifact 'carda.before.sav'
Copy-Item -LiteralPath $card -Destination $cardBackup -Force
$cardHashBefore = (Get-FileHash -LiteralPath $card -Algorithm SHA256).Hash

$start = [Diagnostics.ProcessStartInfo]::new()
$start.FileName = $exe
$start.WorkingDirectory = $deploy
$arguments = @('--headless', '--mute')
if ($arcade) {
    $arguments += '--start-arcade'
}
$arguments += '"' + $data.Replace('"', '\"') + '"'
$start.Arguments = $arguments -join ' '
$start.UseShellExecute = $false
$start.CreateNoWindow = $true
$start.RedirectStandardOutput = $true
$start.RedirectStandardError = $true
$childPriority = if ($HighPriority) {
    'High'
} elseif ($AboveNormalPriority) {
    'AboveNormal'
} else {
    'BelowNormal'
}
$environment = [ordered]@{
    SDL_AUDIODRIVER = 'dummy'
    RECOMPONE_PROCESS_PRIORITY = $childPriority
    RECOMPONE_INPUT_FILE = $fixture
    RECOMPONE_DISABLE_LIVE_INPUT = '1'
    RECOMPONE_SUPPRESS_RUMBLE = '1'
    RECOMPONE_GT2_AI_AUTODRIVE = '1'
    # This harness validates the genuine per-VBlank engine.  Set the mode on
    # the child explicitly so an isolated invocation cannot silently exercise
    # the legacy 30 Hz + synthetic-midpoint path through inherited state.
    RECOMPONE_GT2_TRUE_60HZ = '1'
    RECOMPONE_GT2_AI_AUTODRIVE_MAX_ENGAGEMENTS = '2'
    RECOMPONE_GT2_CREATE_TEST_SAVE = $(if ($arcade) { $null } else { '1' })
    RECOMPONE_GT2_SOAK_QUICK_WIN_AFTER_AI_TICKS = $(if ($arcade) { $null } else { '600' })
    RECOMPONE_GRAPHICS_PRESET_OVERRIDE = 'Enhanced'
    RECOMPONE_EXIT_AFTER_INPUT_POLL = $exitPoll.ToString()
    RECOMPONE_UNTHROTTLED = '1'
    RECOMPONE_THROTTLE_ON_SCRIPT_STAGE = 'race_1'
    RECOMPONE_TRACE_PERFORMANCE = '1'
    RECOMPONE_NATIVE_WORLD_TRACE_INTERVAL = '300'
    RECOMPONE_AUDIT_RENDERER = '1'
    RECOMPONE_DISABLE_DISPLAY_CAPTURE = '1'
}
foreach ($entry in $environment.GetEnumerator()) {
    if ($null -eq $entry.Value) {
        $null = $start.Environment.Remove($entry.Key)
    } else {
        $start.Environment[$entry.Key] = $entry.Value
    }
}

$process = $null
$stdout = ''
$stderr = ''
$timedOut = $false
$contamination = ''
try {
    $process = [Diagnostics.Process]::Start($start)
    $process.PriorityClass =
        [Diagnostics.ProcessPriorityClass]$childPriority
    $stdoutTask = $process.StandardOutput.ReadToEndAsync()
    $stderrTask = $process.StandardError.ReadToEndAsync()
    $deadline = [DateTime]::UtcNow.AddSeconds($TimeoutSeconds)
    while (-not $process.WaitForExit(2000)) {
        if ([DateTime]::UtcNow -ge $deadline) {
            $timedOut = $true
            # Windows PowerShell exposes the .NET Framework Process API, which
            # does not provide Kill(bool entireProcessTree). This host has no
            # child process, so the portable parameterless overload is sufficient.
            $process.Kill()
            $process.WaitForExit()
            break
        }
        $runtimeExternal = @(Get-External3dUse $process.Id)
        $runtimeExternalTotal = @($runtimeExternal | Measure-Object `
            utilizationPercent -Sum).Sum
        if ($null -eq $runtimeExternalTotal) {
            $runtimeExternalTotal = 0.0
        }
        if ($runtimeExternalTotal -gt $MaximumExternal3dPercent) {
            $contamination =
                "External 3D use reached " +
                "$([math]::Round($runtimeExternalTotal, 3))%: " +
                (Format-External3dUse $runtimeExternal)
            $process.Kill()
            $process.WaitForExit()
            break
        }
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
    throw 'Scenario soak did not restore the memory card exactly'
}
if ($timedOut) { throw "$Scenario scenario exceeded ${TimeoutSeconds}s" }
if (-not [string]::IsNullOrWhiteSpace($contamination)) {
    throw "$Scenario scenario rejected: $contamination"
}
if ($process.ExitCode -ne 0 -or
    $stderr -notmatch '\[Runtime\] shutdown complete; exit=0') {
    throw (
        "$Scenario scenario did not exit cleanly: " +
        "processExit=$($process.ExitCode) " +
        "shutdownLogged=$($stderr -match '\[Runtime\] shutdown complete; exit=0')")
}
if ($stderr -notmatch 'headless audio backend=dummy' -or
    $stderr -notmatch 'SDL audio ready: driver=dummy') {
    throw "$Scenario scenario did not remain silent/headless"
}
if ($stderr -notmatch [regex]::Escape($fixture) -or
    $stderr -notmatch "\[Input\] stage 'race_1'" -or
    $stderr -notmatch
        "\[PERF\] throttle engaged at scripted stage 'race_1'") {
    throw "$Scenario scenario did not reach its release-paced race"
}
if ($stderr -match
    'Unhandled exception|Fatal error|unknown software exception|' +
    '\[Native-World\] disabled:|submission rejected|dropped truncated|' +
    'compositor fallback viewport=|unmapped call') {
    throw "$Scenario scenario logged a renderer/runtime failure"
}
if ($stderr -match
    '\[Host-Long-Render\]|\[Host-Long-Headless-Finish\]') {
    throw "$Scenario scenario logged a hidden display-rendering stall"
}

$throttleOffset = $stderr.IndexOf(
    "[PERF] throttle engaged at scripted stage 'race_1'",
    [StringComparison]::Ordinal)
$measured = $stderr.Substring($throttleOffset)
$metricPattern =
    '\[Native-Present\] hostHz=([0-9.]+) uniqueHz=([0-9.]+) ' +
    'new=(\d+) actual=(\d+) synthetic=(\d+) repeated=(\d+) ' +
    'compositor=(\d+) worldMiss=(\d+) transitionHold=(\d+)'
$metrics = [regex]::Matches($measured, $metricPattern)
if ($metrics.Count -lt 6) {
    throw "$Scenario scenario did not record enough paced renderer windows"
}
$perfectWindows = 0
$worldMisses = 0L
$repeatedFrames = 0L
$transitionHolds = 0L
$true60Hz = $environment.RECOMPONE_GT2_TRUE_60HZ -eq '1'
$completeHostRates = [Collections.Generic.List[double]]::new()
$completeUniqueRates = [Collections.Generic.List[double]]::new()
foreach ($metric in $metrics) {
    $hostHz = [double]::Parse(
        $metric.Groups[1].Value,
        [Globalization.CultureInfo]::InvariantCulture)
    $uniqueHz = [double]::Parse(
        $metric.Groups[2].Value,
        [Globalization.CultureInfo]::InvariantCulture)
    $new = [int]$metric.Groups[3].Value
    $actual = [int]$metric.Groups[4].Value
    $synthetic = [int]$metric.Groups[5].Value
    $repeated = [int]$metric.Groups[6].Value
    $compositor = [int]$metric.Groups[7].Value
    $worldMiss = [int]$metric.Groups[8].Value
    $transitionHold = [int]$metric.Groups[9].Value
    $worldMisses += $worldMiss
    $repeatedFrames += $repeated
    $transitionHolds += $transitionHold
    $completeWorld =
        $new -eq 300 -and $compositor -eq 0 -and $transitionHold -eq 0
    if ($completeWorld) {
        $invalidAuthorship = if ($true60Hz) {
            $actual -ne 300 -or $synthetic -ne 0
        } else {
            $actual + $synthetic -ne 300
        }
        if ($invalidAuthorship -or $repeated -ne 0 -or
            $worldMiss -ne 0 -or
            # Keep a hard five-second guard for genuine slowdowns. Ordinary
            # scheduler jitter can straddle a fixed telemetry boundary and is
            # repaid by the next absolute FrameClock deadline, so sustained
            # cadence is checked over every adjacent ten-second pair below.
            $hostHz -lt 58.5 -or $hostHz -gt 61.5 -or
            $uniqueHz -lt 58.5 -or $uniqueHz -gt 61.5) {
            throw "$Scenario scenario has an invalid complete world window: $($metric.Value)"
        }
        $completeHostRates.Add($hostHz)
        $completeUniqueRates.Add($uniqueHz)
        $perfectWindows++
    }
}
if ($perfectWindows -lt 6 -or $worldMisses -ne 0 -or
    $repeatedFrames -ne 0 -or $transitionHolds -gt 30) {
    throw (
        "$Scenario scenario world integrity failed: perfect=$perfectWindows " +
        "misses=$worldMisses repeated=$repeatedFrames " +
        "transitionHolds=$transitionHolds")
}

function Get-CombinedRate([double]$Left, [double]$Right) {
    return 600.0 / ((300.0 / $Left) + (300.0 / $Right))
}

$minimumPairHz = [double]::PositiveInfinity
$maximumPairHz = 0.0
for ($index = 1; $index -lt $completeHostRates.Count; ++$index) {
    $hostPairHz = Get-CombinedRate `
        $completeHostRates[$index - 1] $completeHostRates[$index]
    $uniquePairHz = Get-CombinedRate `
        $completeUniqueRates[$index - 1] $completeUniqueRates[$index]
    $minimumPairHz = [math]::Min(
        $minimumPairHz,
        [math]::Min($hostPairHz, $uniquePairHz))
    $maximumPairHz = [math]::Max(
        $maximumPairHz,
        [math]::Max($hostPairHz, $uniquePairHz))
    if ($hostPairHz -lt 59.5 -or $hostPairHz -gt 60.5 -or
        $uniquePairHz -lt 59.5 -or $uniquePairHz -gt 60.5) {
        throw (
            "$Scenario scenario has sustained cadence outside 59.5-60.5 Hz: " +
            "host=$($hostPairHz.ToString('F3', [Globalization.CultureInfo]::InvariantCulture)) " +
            "unique=$($uniquePairHz.ToString('F3', [Globalization.CultureInfo]::InvariantCulture))")
    }
}
$hostSeconds = @($completeHostRates | ForEach-Object { 300.0 / $_ } |
    Measure-Object -Sum).Sum
$uniqueSeconds = @($completeUniqueRates | ForEach-Object { 300.0 / $_ } |
    Measure-Object -Sum).Sum
$aggregateHostHz = 300.0 * $completeHostRates.Count / $hostSeconds
$aggregateUniqueHz = 300.0 * $completeUniqueRates.Count / $uniqueSeconds
if ($aggregateHostHz -lt 59.5 -or $aggregateHostHz -gt 60.5 -or
    $aggregateUniqueHz -lt 59.5 -or $aggregateUniqueHz -gt 60.5) {
    throw "$Scenario scenario aggregate cadence is outside 59.5-60.5 Hz"
}

$trackAudit = [regex]::Match(
    $stderr,
    '\[GT2-Renderer-Audit\] track raceCalls=(\d+) replayCalls=(\d+) ' +
    'maximumCalls=(\d+) stockCalls=(\d+) entriesScanned=(\d+) ' +
    'nonzeroSelectors=(\d+) nullLists=(\d+) invalidLists=(\d+)')
if (-not $trackAudit.Success -or
    [long]$trackAudit.Groups[1].Value -lt 1 -or
    [long]$trackAudit.Groups[3].Value -lt 1 -or
    [long]$trackAudit.Groups[4].Value -ne 0 -or
    [long]$trackAudit.Groups[6].Value -ne 0 -or
    [long]$trackAudit.Groups[7].Value -ne 0 -or
    [long]$trackAudit.Groups[8].Value -ne 0) {
    throw "$Scenario scenario did not prove maximum, valid track LOD"
}
$vehicleAudit = [regex]::Match(
    $stderr,
    '\[GT2-Renderer-Audit\] vehicles requests=(\d+) selectors=\[1:(\d+)\]')
if (-not $vehicleAudit.Success -or
    [long]$vehicleAudit.Groups[1].Value -lt 1 -or
    $vehicleAudit.Groups[1].Value -ne $vehicleAudit.Groups[2].Value) {
    throw "$Scenario scenario did not prove maximum vehicle LOD"
}

$shutdown = [regex]::Match(
    $stderr,
    '\[Native-World\] shutdown .*repeated=(\d+) .*' +
    'droppedPending=(\d+) droppedOutputPool=(\d+) ' +
    'droppedPublished=(\d+) .*outputWaitTimeouts=(\d+)')
if (-not $shutdown.Success -or
    [long]$shutdown.Groups[1].Value -ne 0 -or
    [long]$shutdown.Groups[2].Value -ne 0 -or
    [long]$shutdown.Groups[3].Value -ne 0 -or
    [long]$shutdown.Groups[5].Value -ne 0) {
    throw "$Scenario scenario exhausted a bounded renderer resource: $($shutdown.Value)"
}

Write-Output (
    "modern_scenario=pass scenario=$Scenario perfect_world_windows=$perfectWindows " +
    "world_miss=$worldMisses repeated=0 transition_holds=$transitionHolds " +
    "aggregate_hz=$($aggregateUniqueHz.ToString('F3', [Globalization.CultureInfo]::InvariantCulture)) " +
    "pair_hz=$($minimumPairHz.ToString('F3', [Globalization.CultureInfo]::InvariantCulture))-$($maximumPairHz.ToString('F3', [Globalization.CultureInfo]::InvariantCulture)) " +
    "maximum_track_calls=$($trackAudit.Groups[3].Value) " +
    "maximum_vehicle_requests=$($vehicleAudit.Groups[1].Value) " +
    "published_transition_drops=$($shutdown.Groups[4].Value) " +
    "card_hash=$cardHashBefore artifact=$artifact")
