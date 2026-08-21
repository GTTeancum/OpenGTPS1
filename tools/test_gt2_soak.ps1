param(
    [string]$DeployPath = 'C:\Programming\GitHub\OpenGTPS1\OpenGTPS1',
    [ValidateRange(10, 180)]
    [int]$MaxMinutes = 175,
    [switch]$ChampionshipOnly,
    [string]$ArtifactName =
        "gt2-soak-$((Get-Date).ToString('yyyyMMdd-HHmmss'))"
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$deploy = (Resolve-Path $DeployPath).Path
$exe = Join-Path $deploy 'GranTurismo2PC.exe'
$artifact = Join-Path $repo "artifacts\$ArtifactName"
$baseline = Join-Path $artifact 'persistent-baseline'
$deadline = [DateTime]::UtcNow.AddMinutes($MaxMinutes)
$activeProcess = $null
$completedStages = [Collections.Generic.List[string]]::new()
$persistentFiles = @('carda.sav', 'cardb.sav', 'interface.ini', 'settings.json')

function Write-SoakStatus {
    param(
        [string]$State,
        [string]$Stage = '',
        [string]$Detail = ''
    )
    $status = [ordered]@{
        state = $State
        stage = $Stage
        detail = $Detail
        updatedUtc = [DateTime]::UtcNow.ToString('o')
        deadlineUtc = $deadline.ToString('o')
        completedStages = @($completedStages)
    }
    $status | ConvertTo-Json -Depth 4 |
        Set-Content -LiteralPath (Join-Path $artifact 'status.json')
}

function Restore-PersistentBaseline {
    foreach ($name in $persistentFiles) {
        $source = Join-Path $baseline $name
        if (Test-Path -LiteralPath $source) {
            Copy-Item -LiteralPath $source -Destination (Join-Path $deploy $name) -Force
        }
    }
}

function Invoke-SoakStage {
    param(
        [string]$Name,
        [string]$Fixture,
        [int]$ExitPoll,
        [int]$StageTimeoutMinutes,
        [hashtable]$Environment = @{}
    )

    $remaining = $deadline - [DateTime]::UtcNow
    if ($remaining.TotalSeconds -le 30) {
        throw 'Global soak deadline reached before the next stage'
    }
    $stageTimeout = [TimeSpan]::FromMinutes($StageTimeoutMinutes)
    if ($stageTimeout -gt $remaining) {
        $stageTimeout = $remaining
    }

    $fixturePath = (Resolve-Path (Join-Path $repo $Fixture)).Path
    $stageArtifact = Join-Path $artifact $Name
    New-Item -ItemType Directory -Path $stageArtifact -Force | Out-Null
    Write-SoakStatus -State 'running' -Stage $Name -Detail "fixture=$Fixture"

    $start = [Diagnostics.ProcessStartInfo]::new()
    $start.FileName = $exe
    $start.WorkingDirectory = $deploy
    $start.Arguments = '--headless'
    $start.UseShellExecute = $false
    $start.CreateNoWindow = $true
    $start.RedirectStandardOutput = $true
    $start.RedirectStandardError = $true
    $start.EnvironmentVariables['RECOMPONE_INPUT_FILE'] = $fixturePath
    $start.EnvironmentVariables['RECOMPONE_DISABLE_LIVE_INPUT'] = '1'
    $start.EnvironmentVariables['RECOMPONE_SUPPRESS_RUMBLE'] = '1'
    $start.EnvironmentVariables['RECOMPONE_UNTHROTTLED'] = '1'
    $start.EnvironmentVariables['RECOMPONE_GRAPHICS_PRESET_OVERRIDE'] = 'Enhanced'
    $start.EnvironmentVariables['RECOMPONE_EXIT_AFTER_INPUT_POLL'] =
        $ExitPoll.ToString()
    foreach ($key in $Environment.Keys) {
        $start.EnvironmentVariables[$key] = [string]$Environment[$key]
    }

    $stageStarted = [DateTime]::UtcNow
    $script:activeProcess = [Diagnostics.Process]::Start($start)
    $stdout = $script:activeProcess.StandardOutput.ReadToEndAsync()
    $stderr = $script:activeProcess.StandardError.ReadToEndAsync()
    $timedOut = $false
    while (-not $script:activeProcess.WaitForExit(1000)) {
        if ([DateTime]::UtcNow -ge $deadline -or
            [DateTime]::UtcNow - $stageStarted -ge $stageTimeout) {
            $timedOut = $true
            $script:activeProcess.Kill($true)
            $script:activeProcess.WaitForExit()
            break
        }
    }

    $stdoutText = $stdout.Result
    $stderrText = $stderr.Result
    [IO.File]::WriteAllText((Join-Path $stageArtifact 'stdout.log'), $stdoutText)
    [IO.File]::WriteAllText((Join-Path $stageArtifact 'stderr.log'), $stderrText)
    [IO.File]::WriteAllText(
        (Join-Path $stageArtifact 'duration.txt'),
        ([DateTime]::UtcNow - $stageStarted).ToString())

    foreach ($capture in Get-ChildItem -LiteralPath $deploy -Filter 'recompone_capture_*.ppm') {
        if ($capture.LastWriteTimeUtc -ge $stageStarted.AddSeconds(-2)) {
            Copy-Item -LiteralPath $capture.FullName -Destination (
                Join-Path $stageArtifact $capture.Name) -Force
        }
    }

    if ($timedOut) {
        throw "$Name exceeded its bounded stage/global deadline"
    }
    if ($script:activeProcess.ExitCode -ne 0) {
        throw "$Name exited with code $($script:activeProcess.ExitCode)"
    }
    if ($stderrText -match
        '(?i)unmapped call:|unmapped address:|unhandled exception|access violation|fatal error') {
        throw "$Name logged a crash signature"
    }
    if ($stderrText -notmatch 'headless audio backend=dummy' -or
        $stderrText -notmatch 'SDL audio ready: driver=dummy') {
        throw "$Name did not prove dummy-audio isolation"
    }
    if ($stderrText -notmatch '\[Runtime\] shutdown complete; exit=0') {
        throw "$Name did not complete a clean runtime shutdown"
    }

    $completedStages.Add($Name)
    Write-SoakStatus -State 'running' -Stage $Name -Detail 'passed'
    $script:activeProcess = $null
}

if (-not (Test-Path -LiteralPath $exe)) {
    throw "Deployment executable not found: $exe"
}

New-Item -ItemType Directory -Path $baseline -Force | Out-Null
foreach ($name in $persistentFiles) {
    $source = Join-Path $deploy $name
    if (Test-Path -LiteralPath $source) {
        Copy-Item -LiteralPath $source -Destination (Join-Path $baseline $name) -Force
    }
}
$beforeHashes = [ordered]@{}
foreach ($name in $persistentFiles) {
    $path = Join-Path $baseline $name
    if (Test-Path -LiteralPath $path) {
        $beforeHashes[$name] = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
    }
}
$beforeHashes | ConvertTo-Json |
    Set-Content -LiteralPath (Join-Path $artifact 'persistent-before.json')

$succeeded = $false
try {
    Invoke-SoakStage `
        -Name '01-world-league-championship' `
        -Fixture 'tests\fixtures\soak-world-league-championship.input' `
        -ExitPoll 100000 `
        -StageTimeoutMinutes 75 `
        -Environment @{
            RECOMPONE_GT2_AI_AUTODRIVE = '1'
            RECOMPONE_GT2_AI_AUTODRIVE_MAX_ENGAGEMENTS = '10'
            RECOMPONE_GT2_SOAK_QUICK_WIN_AFTER_AI_TICKS = '600'
            RECOMPONE_GT2_SOAK_UNLOCK_ALL_RACES = '1'
        }

    if (-not $ChampionshipOnly) {
        # The blank second card provides a deterministic fresh economy for each
        # purchase path without mutating the user's convenience save.
        Copy-Item -LiteralPath (Join-Path $baseline 'cardb.sav') `
            -Destination (Join-Path $deploy 'carda.sav') -Force
        Invoke-SoakStage `
            -Name '02-purchase-upgrade-race-vehicle-a' `
            -Fixture 'tests\fixtures\ai-autodrive-sunday-race.input' `
            -ExitPoll 62000 `
            -StageTimeoutMinutes 45 `
            -Environment @{
                RECOMPONE_GT2_AI_AUTODRIVE = '1'
            }

        Copy-Item -LiteralPath (Join-Path $baseline 'cardb.sav') `
            -Destination (Join-Path $deploy 'carda.sav') -Force
        Invoke-SoakStage `
            -Name '03-purchase-upgrade-vehicle-b' `
            -Fixture 'tests\fixtures\soak-purchase-upgrade-nissan.input' `
            -ExitPoll 4300 `
            -StageTimeoutMinutes 15 `
            -Environment @{
                RECOMPONE_GT2_CREATE_TEST_SAVE = '1'
            }
    }

    $succeeded = $true
    Write-SoakStatus -State 'passed' -Detail 'all stages completed without crash signatures'
}
catch {
    Write-SoakStatus -State 'failed' -Detail $_.Exception.Message
    [IO.File]::WriteAllText((Join-Path $artifact 'failure.txt'), $_.Exception.ToString())
    throw
}
finally {
    if ($activeProcess -and -not $activeProcess.HasExited) {
        $activeProcess.Kill($true)
        $activeProcess.WaitForExit()
    }
    Restore-PersistentBaseline
    $afterHashes = [ordered]@{}
    foreach ($name in $persistentFiles) {
        $path = Join-Path $deploy $name
        if (Test-Path -LiteralPath $path) {
            $afterHashes[$name] =
                (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
        }
    }
    $afterHashes | ConvertTo-Json |
        Set-Content -LiteralPath (Join-Path $artifact 'persistent-after.json')
    if (($beforeHashes | ConvertTo-Json -Compress) -ne
        ($afterHashes | ConvertTo-Json -Compress)) {
        [IO.File]::WriteAllText(
            (Join-Path $artifact 'persistent-restore-mismatch.txt'),
            "before=$($beforeHashes | ConvertTo-Json -Compress)`r`n" +
            "after=$($afterHashes | ConvertTo-Json -Compress)")
        if ($succeeded) {
            Write-SoakStatus -State 'failed' -Detail 'persistent-state restore hash mismatch'
            throw 'Persistent-state restore hash mismatch'
        }
    }
}

Write-Output "soak=passed stages=$($completedStages.Count) artifact=$artifact"
Write-Output "deadline=$($deadline.ToString('o')) persistent_state=restored"
