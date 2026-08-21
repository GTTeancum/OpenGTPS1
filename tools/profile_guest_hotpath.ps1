param(
    [string]$ArtifactName = 'modern-renderer-guest-cpu-trace',
    [int]$ExitPoll = 5200,
    [int]$TraceSeconds = 12,
    [int]$TraceAfterPoll = 0,
    [ValidateSet('Arcade', 'Simulation')]
    [string]$Mode = 'Arcade',
    [int]$QuickWinAfterAiTicks = 0,
    [switch]$CreateTestSave,
    [switch]$True60Hz,
    [switch]$AboveNormalPriority,
    [string]$Fixture =
        'tests\fixtures\arcade-modern-renderer-soak-resilient.input',
    [switch]$DisableTieredCompilation,
    [string]$DeployPath = ''
)

$ErrorActionPreference = 'Stop'
try { (Get-Process -Id $PID).PriorityClass = 'BelowNormal' } catch {}
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$artifact = Join-Path $repo "artifacts\$ArtifactName"
$deploy = if ([string]::IsNullOrWhiteSpace($DeployPath)) {
    Join-Path $repo 'tools\unified-host\bin\Release\net10.0'
} else {
    (Resolve-Path -LiteralPath $DeployPath).Path
}
$exe = Join-Path $deploy 'GranTurismo2PC.exe'
$card = Join-Path $deploy 'carda.sav'
$fixture = if ([IO.Path]::IsPathRooted($Fixture)) {
    (Resolve-Path -LiteralPath $Fixture).Path
} else {
    (Resolve-Path -LiteralPath (Join-Path $repo $Fixture)).Path
}
$data = Join-Path $repo 'work\gt2-unified'
$traceTool = (Get-Command dotnet-trace -ErrorAction Stop).Source
foreach ($required in @($exe, $card, $fixture, $data, $traceTool)) {
    if (-not (Test-Path -LiteralPath $required)) {
        throw "Required profile input is missing: $required"
    }
}

New-Item -ItemType Directory -Path $artifact -Force | Out-Null
$stdoutPath = Join-Path $artifact 'stdout.log'
$stderrPath = Join-Path $artifact 'stderr.log'
$tracePath = Join-Path $artifact 'guest-hot.nettrace'
$runtimeFixture = Join-Path $artifact 'profile-no-capture.input'
Get-Content -LiteralPath $fixture |
    Where-Object {
        $_ -notmatch '^\s*\d+\s*\+\s*\d+\s*=\s*CAPTURE\s*(#.*)?$'
    } |
    Set-Content -LiteralPath $runtimeFixture -Encoding ASCII
$cardBackup = Join-Path $artifact 'carda.before.sav'
Copy-Item -LiteralPath $card -Destination $cardBackup -Force
$cardHashBefore = (Get-FileHash -LiteralPath $card -Algorithm SHA256).Hash

$environment = [ordered]@{
    SDL_AUDIODRIVER = 'dummy'
    RECOMPONE_PROCESS_PRIORITY = if ($AboveNormalPriority) {
        'AboveNormal'
    } else {
        'BelowNormal'
    }
    RECOMPONE_INPUT_FILE = $runtimeFixture
    RECOMPONE_DISABLE_LIVE_INPUT = '1'
    RECOMPONE_SUPPRESS_RUMBLE = '1'
    RECOMPONE_GT2_AI_AUTODRIVE = '1'
    RECOMPONE_GT2_AI_AUTODRIVE_MAX_ENGAGEMENTS = '2'
    RECOMPONE_GRAPHICS_PRESET_OVERRIDE = 'Enhanced'
    RECOMPONE_EXIT_AFTER_INPUT_POLL = $ExitPoll.ToString()
    RECOMPONE_UNTHROTTLED = '1'
    RECOMPONE_THROTTLE_ON_SCRIPT_STAGE = 'race_1'
    RECOMPONE_TRACE_PERFORMANCE = '1'
    RECOMPONE_DISABLE_DISPLAY_CAPTURE = '1'
}
if ($True60Hz) {
    $environment.RECOMPONE_GT2_TRUE_60HZ = '1'
}
if ($DisableTieredCompilation) {
    $environment.DOTNET_TieredCompilation = '0'
}
if ($QuickWinAfterAiTicks -gt 0) {
    $environment.RECOMPONE_GT2_SOAK_QUICK_WIN_AFTER_AI_TICKS =
        $QuickWinAfterAiTicks.ToString()
}
if ($CreateTestSave) {
    $environment.RECOMPONE_GT2_CREATE_TEST_SAVE = '1'
}
$originalEnvironment = @{}
$process = $null
try {
    foreach ($entry in $environment.GetEnumerator()) {
        $originalEnvironment[$entry.Key] =
            [Environment]::GetEnvironmentVariable(
                $entry.Key,
                [EnvironmentVariableTarget]::Process)
        [Environment]::SetEnvironmentVariable(
            $entry.Key,
            $entry.Value,
            [EnvironmentVariableTarget]::Process)
    }

    $arguments = @('--headless', '--mute')
    if ($Mode -eq 'Arcade') {
        $arguments += '--start-arcade'
    }
    $arguments += $data
    $process = Start-Process `
        -FilePath $exe `
        -ArgumentList $arguments `
        -WorkingDirectory $deploy `
        -PassThru `
        -RedirectStandardOutput $stdoutPath `
        -RedirectStandardError $stderrPath `
        -WindowStyle Hidden
    try {
        $process.PriorityClass = if ($AboveNormalPriority) {
            'AboveNormal'
        } else {
            'BelowNormal'
        }
    } catch {}
    $deadline = [DateTime]::UtcNow.AddSeconds(240)
    $triggerPattern = if ($TraceAfterPoll -gt 0) {
        "^\[PERF\] poll=$TraceAfterPoll "
    } else {
        "throttle engaged at scripted stage 'race_1'"
    }
    while ([DateTime]::UtcNow -lt $deadline) {
        if (
            (Test-Path -LiteralPath $stderrPath) -and
            (Select-String `
                -LiteralPath $stderrPath `
                -Pattern $triggerPattern `
                -Quiet)
        ) {
            break
        }
        if ($process.HasExited) {
            throw "Game exited before the race profile: $($process.ExitCode)"
        }
        Start-Sleep -Milliseconds 100
    }
    if (-not (Select-String `
            -LiteralPath $stderrPath `
            -Pattern $triggerPattern `
            -Quiet)) {
        throw "Race profile trigger timed out: $triggerPattern"
    }

    & $traceTool collect `
        --process-id $process.Id `
        --profile dotnet-sampled-thread-time `
        --duration ([TimeSpan]::FromSeconds($TraceSeconds).ToString(
            'dd\:hh\:mm\:ss')) `
        --output $tracePath
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet-trace failed with exit code $LASTEXITCODE"
    }
    $process.WaitForExit()
    $process.Refresh()
    $cleanShutdown = Select-String `
        -LiteralPath $stderrPath `
        -Pattern '^\[Runtime\] shutdown complete; exit=0$' `
        -Quiet
    if ($null -ne $process.ExitCode -and $process.ExitCode -ne 0) {
        throw "Profiled game exited with code $($process.ExitCode)"
    }
    if ($null -eq $process.ExitCode -and -not $cleanShutdown) {
        throw 'Profiled game did not report a clean runtime shutdown'
    }
} finally {
    if ($process -and -not $process.HasExited) {
        $process.Kill()
        $process.WaitForExit()
    }
    Copy-Item -LiteralPath $cardBackup -Destination $card -Force
    foreach ($entry in $originalEnvironment.GetEnumerator()) {
        [Environment]::SetEnvironmentVariable(
            $entry.Key,
            $entry.Value,
            [EnvironmentVariableTarget]::Process)
    }
}

$cardHashAfter = (Get-FileHash -LiteralPath $card -Algorithm SHA256).Hash
if ($cardHashAfter -ne $cardHashBefore) {
    throw 'Profile run did not restore the memory card exactly'
}
if (-not (Test-Path -LiteralPath $tracePath -PathType Leaf)) {
    throw 'CPU sampling trace was not created'
}
Write-Output (
    "guest_profile=complete trace=$tracePath " +
    "bytes=$((Get-Item -LiteralPath $tracePath).Length) " +
    "card_hash=$cardHashBefore")
