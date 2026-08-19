param(
    [string]$ArtifactName =
        "modern-renderer-capture-profile-$((Get-Date).ToString('yyyyMMdd-HHmmss'))",
    [string[]]$Captures = @(
        'artifacts\modern-renderer-v275-full-profile-captures\track-1.ogtwcap',
        'artifacts\modern-renderer-v275-full-profile-captures\track-2.ogtwcap',
        'artifacts\modern-renderer-v275-full-profile-captures\track-3.ogtwcap',
        'artifacts\modern-renderer-v275-full-profile-captures\track-4.ogtwcap',
        'artifacts\perf-heavy-section.ogtwcap'
    ),
    [ValidateRange(30, 10000)]
    [int]$Frames = 600,
    [ValidateRange(1, 8)]
    [int]$Scale = 4,
    [double]$MaximumExternal3dPercent = 5.0
)

$ErrorActionPreference = 'Stop'
try { (Get-Process -Id $PID).PriorityClass = 'BelowNormal' } catch {}
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$benchmark = Join-Path $repo `
    'build\native\Release\opengt_live_benchmark.exe'
if (-not (Test-Path -LiteralPath $benchmark -PathType Leaf)) {
    throw "Native benchmark is missing: $benchmark"
}

$external3d = @()
$samples = (Get-Counter `
    '\GPU Engine(*)\Utilization Percentage' `
    -ErrorAction SilentlyContinue).CounterSamples
foreach ($sample in $samples) {
    if ($sample.CookedValue -le 0 -or
        $sample.InstanceName -notmatch 'pid_(\d+).*engtype_3d$') {
        continue
    }
    $pidValue = [int]$Matches[1]
    $name = try {
        (Get-Process -Id $pidValue -ErrorAction Stop).ProcessName
    } catch {
        "pid-$pidValue"
    }
    if ($name -notin @('dwm', 'System', 'Idle')) {
        $external3d += [pscustomobject]@{
            pid = $pidValue
            process = $name
            utilizationPercent = [math]::Round($sample.CookedValue, 3)
        }
    }
}
$external3dTotal = @($external3d | Measure-Object `
    utilizationPercent -Sum).Sum
if ($null -eq $external3dTotal) {
    $external3dTotal = 0.0
}
if ($external3dTotal -gt $MaximumExternal3dPercent) {
    throw (
        "External 3D use is $([math]::Round($external3dTotal, 3))%, " +
        "above the $MaximumExternal3dPercent% profiling limit: " +
        (($external3d | ForEach-Object {
            "$($_.process)[$($_.pid)]=$($_.utilizationPercent)%"
        }) -join ', '))
}

$artifact = Join-Path $repo "artifacts\$ArtifactName"
New-Item -ItemType Directory -Path $artifact -Force | Out-Null
$results = [Collections.Generic.List[object]]::new()
$resultPattern = [regex]::new(
    '^frames=(\d+) totalWallMs=([0-9.]+) ' +
    'wallMs=([0-9.]+)/([0-9.]+)/([0-9.]+) ' +
    'pipelineMs=([0-9.]+)/([0-9.]+)/([0-9.]+) ' +
    'gpuDriverMs=([0-9.]+)/([0-9.]+)/([0-9.]+) ' +
    'topologyMs=([0-9.]+)/([0-9.]+)/([0-9.]+) ')

foreach ($captureInput in $Captures) {
    $capture = if ([IO.Path]::IsPathRooted($captureInput)) {
        (Resolve-Path -LiteralPath $captureInput).Path
    } else {
        (Resolve-Path -LiteralPath (Join-Path $repo $captureInput)).Path
    }
    $name = [IO.Path]::GetFileNameWithoutExtension($capture)
    $logPath = Join-Path $artifact "$name.log"
    $output = @(& $benchmark $capture $Frames $Scale 2>&1)
    $output | Set-Content -LiteralPath $logPath -Encoding ASCII
    if ($LASTEXITCODE -ne 0) {
        throw "Benchmark failed for $capture with exit code $LASTEXITCODE"
    }
    if ($output[0] -notmatch
        'gpuMetric=driver-submit-completion-readback$') {
        throw "Benchmark did not identify its GPU-driver metric: $capture"
    }
    $match = $resultPattern.Match([string]$output[-1])
    if (-not $match.Success -or [int]$match.Groups[1].Value -ne $Frames) {
        throw "Benchmark summary is malformed: $($output[-1])"
    }
    $numbers = @(2..14 | ForEach-Object {
        [double]::Parse(
            $match.Groups[$_].Value,
            [Globalization.CultureInfo]::InvariantCulture)
    })
    foreach ($start in @(1, 4, 7, 10)) {
        if ($numbers[$start] -gt $numbers[$start + 1] -or
            $numbers[$start + 1] -gt $numbers[$start + 2]) {
            throw "Unordered percentile summary: $($output[-1])"
        }
    }
    $results.Add([ordered]@{
        capture = $capture
        frames = $Frames
        scale = $Scale
        output = [ordered]@{
            width = 320 * $Scale
            height = 240 * $Scale
        }
        totalWallMs = $numbers[0]
        p50p95p99Ms = [ordered]@{
            wall = @($numbers[1], $numbers[2], $numbers[3])
            pipeline = @($numbers[4], $numbers[5], $numbers[6])
            gpuDriverSubmitCompletionReadback = @(
                $numbers[7], $numbers[8], $numbers[9])
            topology = @($numbers[10], $numbers[11], $numbers[12])
        }
        rawLog = $logPath
    })
}

$summary = [ordered]@{
    metric = 'driver-submit-completion-readback'
    createdUtc = [DateTime]::UtcNow.ToString('o')
    framesPerCapture = $Frames
    scale = $Scale
    external3dPreflightPercent = [math]::Round($external3dTotal, 3)
    external3dPreflight = @($external3d)
    adapters = @(Get-CimInstance Win32_VideoController | ForEach-Object {
        [ordered]@{
            name = $_.Name
            driverVersion = $_.DriverVersion
            driverDate = $_.DriverDate
        }
    })
    captures = @($results)
}
$summaryPath = Join-Path $artifact 'summary.json'
$summary | ConvertTo-Json -Depth 8 |
    Set-Content -LiteralPath $summaryPath -Encoding UTF8
Write-Output (
    "modern_capture_profile=pass captures=$($results.Count) " +
    "frames=$Frames scale=$Scale metric=$($summary.metric) " +
    "external3d=$($summary.external3dPreflightPercent) " +
    "summary=$summaryPath")
