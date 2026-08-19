param(
    [string]$ArtifactName =
        "modern-renderer-adjacent-$((Get-Date).ToString('yyyyMMdd-HHmmss'))",
    [ValidateSet('Arcade', 'Simulation')]
    [string]$Mode = 'Simulation',
    [string]$Fixture = '',
    [ValidateRange(0, 1000000)]
    [int]$StartPoll = 8000,
    [ValidateRange(2, 64)]
    [int]$Count = 64,
    [ValidateRange(1, 3600)]
    [int]$Interval = 1,
    [ValidateRange(1, 1000000)]
    [int]$ExitPoll = 9000,
    [int]$TimeoutSeconds = 240,
    [double]$MaximumExternal3dPercent = 5.0,
    [string]$DeployPath = 'tools\unified-host\bin\Release\net10.0',
    [string]$DataPath = 'work\gt2-unified'
)

$ErrorActionPreference = 'Stop'
try { (Get-Process -Id $PID).PriorityClass = 'BelowNormal' } catch {}
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

if ($ExitPoll -le $StartPoll) {
    throw 'ExitPoll must be greater than StartPoll'
}
if ([string]::IsNullOrWhiteSpace($Fixture)) {
    $Fixture = if ($Mode -eq 'Arcade') {
        'tests\fixtures\arcade-modern-renderer-soak-resilient.input'
    } else {
        'tests\fixtures\modern-renderer-replay-soak.input'
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
        "above the $MaximumExternal3dPercent% capture limit: " +
        (Format-External3dUse $external3d))
}

$deploy = Resolve-RepoPath $DeployPath
$data = Resolve-RepoPath $DataPath
$fixturePath = Resolve-RepoPath $Fixture
$exe = Join-Path $deploy 'GranTurismo2PC.exe'
$card = Join-Path $deploy 'carda.sav'
$inspector = Join-Path $repo `
    'build\native\Release\opengt_world_capture_inspect.exe'
$pairInspector = Join-Path $repo `
    'build\native\Release\opengt_live_pair_inspect.exe'
foreach ($required in @(
    $exe, $card, $fixturePath, $inspector, $pairInspector)) {
    if (-not (Test-Path -LiteralPath $required -PathType Leaf)) {
        throw "Required adjacent-capture input is missing: $required"
    }
}

$artifact = Join-Path $repo "artifacts\$ArtifactName"
New-Item -ItemType Directory -Path $artifact -Force | Out-Null
$stdoutPath = Join-Path $artifact 'stdout.log'
$stderrPath = Join-Path $artifact 'stderr.log'
$inspectPath = Join-Path $artifact 'capture-inspect.log'
$pairInspectPath = Join-Path $artifact 'pair-inspect.log'
$pairOutputDirectory = Join-Path $artifact 'pair-output'
$captureBase = Join-Path $artifact 'world.ogtwcap'
$cardBackup = Join-Path $artifact 'carda.before.sav'
$runtimeFixture = Join-Path $artifact 'capture-no-display.input'
Get-Content -LiteralPath $fixturePath |
    Where-Object {
        $_ -notmatch '^\s*\d+\s*\+\s*\d+\s*=\s*CAPTURE\s*(#.*)?$'
    } |
    Set-Content -LiteralPath $runtimeFixture -Encoding ASCII
Copy-Item -LiteralPath $card -Destination $cardBackup -Force
$cardHashBefore = (Get-FileHash -LiteralPath $card -Algorithm SHA256).Hash

$start = [Diagnostics.ProcessStartInfo]::new()
$start.FileName = $exe
$start.WorkingDirectory = $deploy
$arguments = @('--headless', '--mute')
if ($Mode -eq 'Arcade') {
    $arguments += '--start-arcade'
}
$arguments += '"' + $data.Replace('"', '\"') + '"'
$start.Arguments = $arguments -join ' '
$start.UseShellExecute = $false
$start.CreateNoWindow = $true
$start.RedirectStandardOutput = $true
$start.RedirectStandardError = $true
$environment = [ordered]@{
    SDL_AUDIODRIVER = 'dummy'
    RECOMPONE_PROCESS_PRIORITY = 'BelowNormal'
    RECOMPONE_INPUT_FILE = $runtimeFixture
    RECOMPONE_DISABLE_LIVE_INPUT = '1'
    RECOMPONE_SUPPRESS_RUMBLE = '1'
    RECOMPONE_GT2_AI_AUTODRIVE = '1'
    RECOMPONE_GT2_AI_AUTODRIVE_MAX_ENGAGEMENTS = '2'
    RECOMPONE_GT2_CREATE_TEST_SAVE = $(if ($Mode -eq 'Simulation') { '1' } else { $null })
    RECOMPONE_GRAPHICS_PRESET_OVERRIDE = 'Enhanced'
    RECOMPONE_EXIT_AFTER_INPUT_POLL = $ExitPoll.ToString()
    RECOMPONE_UNTHROTTLED = '1'
    RECOMPONE_THROTTLE_ON_SCRIPT_STAGE = 'race_1'
    RECOMPONE_TRACE_PERFORMANCE = '1'
    RECOMPONE_NATIVE_WORLD_TRACE_INTERVAL = '60'
    RECOMPONE_AUDIT_RENDERER = '1'
    RECOMPONE_DISABLE_DISPLAY_CAPTURE = '1'
    RECOMPONE_NATIVE_WORLD_DUMP_PATH = $captureBase
    RECOMPONE_NATIVE_WORLD_DUMP_INPUT_POLL = $StartPoll.ToString()
    RECOMPONE_NATIVE_WORLD_DUMP_COUNT = $Count.ToString()
    RECOMPONE_NATIVE_WORLD_DUMP_INTERVAL = $Interval.ToString()
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
    $stdoutTask = $process.StandardOutput.ReadToEndAsync()
    $stderrTask = $process.StandardError.ReadToEndAsync()
    $deadline = [DateTime]::UtcNow.AddSeconds($TimeoutSeconds)
    while (-not $process.WaitForExit(2000)) {
        if ([DateTime]::UtcNow -ge $deadline) {
            $timedOut = $true
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
    throw 'Memory-card restore hash mismatch'
}
if ($timedOut) {
    throw "Adjacent world capture exceeded ${TimeoutSeconds}s"
}
if (-not [string]::IsNullOrWhiteSpace($contamination)) {
    throw "Adjacent world capture rejected: $contamination"
}
if ($process.ExitCode -ne 0 -or
    $stderr -notmatch '\[Runtime\] shutdown complete; exit=0') {
    throw 'Adjacent world capture did not exit cleanly'
}
if ($stderr -match
    'Unhandled exception|Fatal error|unknown software exception|' +
    '\[Native-World\] disabled:|submission rejected|dropped truncated|' +
    'compositor fallback viewport=|unmapped call') {
    throw 'Adjacent world capture logged a renderer/runtime failure marker'
}

$captureFiles = @(Get-ChildItem -LiteralPath $artifact -File |
    Where-Object { $_.Name -match '^world-(\d+)\.ogtwcap$' } |
    Sort-Object { [int]([regex]::Match($_.Name, '\d+').Value) })
if ($captureFiles.Count -ne $Count) {
    throw "Expected $Count adjacent captures, found $($captureFiles.Count)"
}
$dumpMatches = [regex]::Matches(
    $stderr,
    '(?m)^\[Native-World\] dumped first live capture bytes=(\d+) ' +
        'poll=(\d+) interval=(\d+) path=(.+)$')
if ($dumpMatches.Count -ne $Count) {
    throw "Expected $Count dump records, found $($dumpMatches.Count)"
}
$lastPoll = -1
foreach ($match in $dumpMatches) {
    $poll = [int]$match.Groups[2].Value
    if ($poll -lt $StartPoll -or $poll -le $lastPoll) {
        throw "Capture polls are not strictly increasing at poll $poll"
    }
    $lastPoll = $poll
}

$inspection = [Collections.Generic.List[string]]::new()
foreach ($captureFile in $captureFiles) {
    $output = @(& $inspector $captureFile.FullName 2>&1)
    if ($output.Count -ne 1 -or
        $output[0] -notmatch '^version=6 frame=\d+ poll=\d+ ' -or
        $output[0] -notmatch 'screenErrorOver1=0 ' -or
        $output[0] -notmatch 'trackErrorOver2=0 ' -or
        $output[0] -notmatch 'vehicleErrorOver2=0 ' -or
        $output[0] -notmatch 'topologyResult=success ') {
        throw "Capture inspection failed: $($captureFile.FullName)"
    }
    $inspection.Add("=== $($captureFile.Name) ===")
    foreach ($line in $output) {
        $inspection.Add([string]$line)
    }
}
$inspection | Set-Content -LiteralPath $inspectPath -Encoding ASCII

$pairExternalBefore = @(Get-External3dUse)
$pairExternalBeforeTotal = @($pairExternalBefore | Measure-Object `
    utilizationPercent -Sum).Sum
if ($null -eq $pairExternalBeforeTotal) {
    $pairExternalBeforeTotal = 0.0
}
if ($pairExternalBeforeTotal -gt $MaximumExternal3dPercent) {
    throw (
        'Adjacent pair profile was contaminated before launch: ' +
        (Format-External3dUse $pairExternalBefore))
}

$pairStart = [Diagnostics.ProcessStartInfo]::new()
$pairStart.FileName = $pairInspector
$pairStart.WorkingDirectory = $repo
$pairArguments = @('--metrics-only', $pairOutputDirectory) +
    @($captureFiles | ForEach-Object { $_.FullName })
$pairStart.Arguments = @($pairArguments | ForEach-Object {
    '"' + $_.Replace('"', '\"') + '"'
}) -join ' '
$pairStart.UseShellExecute = $false
$pairStart.CreateNoWindow = $true
$pairStart.RedirectStandardOutput = $true
$pairStart.RedirectStandardError = $true
$pairStart.Environment['OPENGT_TOPOLOGY_PHASE_DIAGNOSTICS'] = '1'
$pairStart.Environment['OPENGT_INTERPOLATION_PHASE_DIAGNOSTICS'] = '1'
$pairProcess = [Diagnostics.Process]::Start($pairStart)
try { $pairProcess.PriorityClass = 'BelowNormal' } catch {}
$pairStdoutTask = $pairProcess.StandardOutput.ReadToEndAsync()
$pairStderrTask = $pairProcess.StandardError.ReadToEndAsync()
$pairProcess.WaitForExit()
$pairStdout = $pairStdoutTask.Result
$pairStderr = $pairStderrTask.Result
$pairText = $pairStdout + "`n" + $pairStderr
[IO.File]::WriteAllText($pairInspectPath, $pairText)
if ($pairProcess.ExitCode -ne 0) {
    throw (
        'Adjacent pair inspection failed with exit code ' +
        $pairProcess.ExitCode)
}
$pairExternalAfter = @(Get-External3dUse)
$pairExternalAfterTotal = @($pairExternalAfter | Measure-Object `
    utilizationPercent -Sum).Sum
if ($null -eq $pairExternalAfterTotal) {
    $pairExternalAfterTotal = 0.0
}
if ($pairExternalAfterTotal -gt $MaximumExternal3dPercent) {
    throw (
        'Adjacent pair profile was contaminated after completion: ' +
        (Format-External3dUse $pairExternalAfter))
}
$pairSummary = [regex]::Match(
    $pairText,
    'captures=(\d+) temporalPairs=(\d+) outputs=(\d+) images=(\d+) ' +
        'p50PairMs=([0-9.]+) p95PairMs=([0-9.]+) ' +
        'p99PairMs=([0-9.]+)')
if (-not $pairSummary.Success -or
    [int]$pairSummary.Groups[1].Value -ne $Count -or
    [int]$pairSummary.Groups[2].Value -ne ($Count - 1) -or
    [int]$pairSummary.Groups[4].Value -ne 0) {
    throw 'Adjacent pair summary is incomplete or contains reset samples'
}
$topologyPhases = [regex]::Matches(
    $pairText,
    '(?m)^\[Topology-Phases\] ')
$interpolationPhases = [regex]::Matches(
    $pairText,
    '(?m)^\[Interpolation-Phases\] ')
if ($topologyPhases.Count -ne $Count -or
    $interpolationPhases.Count -ne ($Count - 1)) {
    throw (
        'Adjacent phase diagnostics are incomplete: ' +
        "topology=$($topologyPhases.Count)/$Count " +
        "interpolation=$($interpolationPhases.Count)/$($Count - 1)")
}
$uncachedInterpolationPhases = [regex]::Matches(
    $pairText,
    '(?m)^\[Interpolation-Phases\].* cache=0/0\s*$')
$cachedInterpolationPhases = [regex]::Matches(
    $pairText,
    '(?m)^\[Interpolation-Phases\].* cache=1/0\s*$')
if ($uncachedInterpolationPhases.Count -ne 1 -or
    $cachedInterpolationPhases.Count -ne ($Count - 2)) {
    throw (
        'Adjacent interpolation cache coverage is incomplete: ' +
        "cold=$($uncachedInterpolationPhases.Count)/1 " +
        "steady=$($cachedInterpolationPhases.Count)/$($Count - 2)")
}

Write-Output (
    "adjacent captures passed mode=$Mode count=$Count " +
    "polls=$($dumpMatches[0].Groups[2].Value)-$lastPoll " +
    "pairMs=$($pairSummary.Groups[5].Value)/" +
        "$($pairSummary.Groups[6].Value)/" +
        "$($pairSummary.Groups[7].Value) " +
    "external3d=$([math]::Round($external3dTotal, 3))% artifact=$artifact")
