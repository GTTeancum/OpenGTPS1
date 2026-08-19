param(
    [ValidateRange(10, 175)]
    [int]$MaxMinutes = 30,
    [string]$DeployPath = 'tools\unified-host\bin\Release\net10.0',
    [string]$DataPath = 'work\gt2-unified',
    [double]$MaximumExternal3dPercent = 5.0,
    [ValidateSet('Arcade', 'SSR11', 'SupraTahiti')]
    [string[]]$Scenarios = @('Arcade', 'SSR11', 'SupraTahiti'),
    [switch]$AboveNormalPriority,
    [string]$ArtifactName =
        "modern-renderer-extended-soak-$((Get-Date).ToString('yyyyMMdd-HHmmss'))"
)

$ErrorActionPreference = 'Stop'
if ($IsWindows -or $env:OS -eq 'Windows_NT') {
    [Diagnostics.Process]::GetCurrentProcess().PriorityClass =
        [Diagnostics.ProcessPriorityClass]::BelowNormal
}
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$scenarioHarness = Join-Path $PSScriptRoot 'test_modern_renderer_scenario.ps1'
$artifact = Join-Path $repo "artifacts\$ArtifactName"
$deadline = [DateTime]::UtcNow.AddMinutes($MaxMinutes)
$passes = [Collections.Generic.List[object]]::new()
New-Item -ItemType Directory -Path $artifact -Force | Out-Null

function Write-Status([string]$State, [string]$Detail) {
    [ordered]@{
        state = $State
        detail = $Detail
        updatedUtc = [DateTime]::UtcNow.ToString('o')
        deadlineUtc = $deadline.ToString('o')
        completedRuns = $passes.Count
        passes = @($passes)
    } | ConvertTo-Json -Depth 5 |
        Set-Content -LiteralPath (Join-Path $artifact 'status.json')
}

$run = 0
Write-Status 'running' 'starting round-robin renderer scenarios'
try {
    while ($true) {
        $remaining = $deadline - [DateTime]::UtcNow
        # A strict scenario normally needs about two minutes. Do not begin a
        # run that cannot finish and restore its memory-card baseline cleanly.
        if ($remaining.TotalSeconds -lt 150) {
            break
        }
        $scenario = $scenarios[$run % $scenarios.Count]
        $run++
        $childName = "$ArtifactName-run-$($run.ToString('D2'))-$($scenario.ToLowerInvariant())"
        Write-Status 'running' "run=$run scenario=$scenario"
        $started = [DateTime]::UtcNow
        if ($AboveNormalPriority) {
            $output = & $scenarioHarness `
                -Scenario $scenario `
                -ArtifactName $childName `
                -DeployPath $DeployPath `
                -DataPath $DataPath `
                -MaximumExternal3dPercent $MaximumExternal3dPercent `
                -TimeoutSeconds ([Math]::Min(
                    300,
                    [int]$remaining.TotalSeconds)) `
                -AboveNormalPriority
        } else {
            $output = & $scenarioHarness `
                -Scenario $scenario `
                -ArtifactName $childName `
                -DeployPath $DeployPath `
                -DataPath $DataPath `
                -MaximumExternal3dPercent $MaximumExternal3dPercent `
                -TimeoutSeconds ([Math]::Min(
                    300,
                    [int]$remaining.TotalSeconds))
        }
        $line = @($output | Where-Object {
            $_ -match '^modern_scenario=pass '
        })[-1]
        if ([string]::IsNullOrWhiteSpace($line)) {
            throw "scenario=$scenario did not return a pass record"
        }
        $passes.Add([ordered]@{
            run = $run
            scenario = $scenario
            durationSeconds = [Math]::Round(
                ([DateTime]::UtcNow - $started).TotalSeconds, 3)
            result = $line
        })
        [IO.File]::WriteAllLines(
            (Join-Path $artifact "run-$($run.ToString('D2')).log"),
            [string[]]$output)
    }
    if ($passes.Count -lt $scenarios.Count) {
        throw "extended soak completed only $($passes.Count) runs"
    }
    $covered = @($passes.scenario | Sort-Object -Unique)
    if ($covered.Count -ne $scenarios.Count) {
        throw "extended soak did not cover every scenario: $($covered -join ',')"
    }
    Write-Status 'passed' 'all completed scenarios passed strict validation'
}
catch {
    Write-Status 'failed' $_.Exception.Message
    throw
}

Write-Output (
    "modern_extended_soak=pass runs=$($passes.Count) " +
    "scenarios=$((@($passes.scenario | Sort-Object -Unique)) -join ',') " +
    "artifact=$artifact")
