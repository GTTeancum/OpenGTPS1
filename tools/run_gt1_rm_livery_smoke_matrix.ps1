param(
    [string[]]$Targets = @(),
    [ValidateSet(0, 1)]
    [int[]]$Paints = @(0, 1),
    [string]$ArtifactRoot = 'artifacts\gt1-rm-livery-smoke-matrix-20260906',
    [string]$RuntimeRoot = 'work\gt1-rm-livery-smoke-runtime',
    [string]$DeployPath = 'OpenGTPS1',
    [ValidateRange(301, 9000)]
    [int]$RaceExitPoll = 600,
    [ValidatePattern('^[A-Fa-f0-9]{64}$')]
    [string]$ExpectedExeSha256 =
        'D4A1826606AA70ACF8E42DC6E675DA9D278CFA03FAEA24058EA80748CD071160'
)

# Rebuild one isolated developer selector per RM identity, then load both of
# its authored paints through the normal selector and race hand-off.  Input is
# process-local; this script never sends host keyboard, mouse, or controller
# events.  Completed native runs are resumable from their retained manifests.
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$converter = Join-Path $PSScriptRoot 'gt1_convert.py'
$prepare = Join-Path $PSScriptRoot 'prepare_unified_install.py'
$capture = Join-Path $PSScriptRoot 'capture_unified_fixture.ps1'
$simulationRoot = Join-Path $repo 'work\disc'
$arcadeRoot = Join-Path $repo 'work\arcade-unified'
$arcadeVolume = Join-Path $repo 'work\arcade-disc\GT2.VOL'
$arcadeOverlay = Join-Path $repo 'work\arcade-disc\GT2.OVL'
$deployRoot = if ([IO.Path]::IsPathRooted($DeployPath)) {
    (Resolve-Path -LiteralPath $DeployPath).Path
} else {
    (Resolve-Path -LiteralPath (Join-Path $repo $DeployPath)).Path
}
$exe = Join-Path $deployRoot 'GranTurismo2PC.exe'
if ((Get-FileHash -LiteralPath $exe -Algorithm SHA256).Hash -ne
    $ExpectedExeSha256) {
    throw 'RM smoke executable does not match the requested reviewed build'
}
if (@(Get-CimInstance Win32_Process `
        -Filter "Name='GranTurismo2PC.exe'").Count) {
    throw 'RM smoke matrix requires serial execution; a game process is running'
}

$allSources = @(
    'dvprr', 'hpnvr', 'mgnor', 'mgntr', 'mgoor', 'mgotr', 'mgtmr',
    'mgtor', 'mgttr', 'mlnnr', 'mlnor', 'mmgor', 'mmgrr', 'nn32r',
    'nplor', 'nr02r', 'nr32r', 'ns13r', 'nv12r', 'nv22r', 'nzx2r',
    'nzx3r', 'nzxsr', 'nzxvr', 'tcelr', 'tmrlr', 'tsonr', 'tsorr',
    'tspnr', 'tsprr', 'vcrbr'
)
$sourceByTarget = @{}
foreach ($source in $allSources) {
    $sourceByTarget[$source.Substring(0, 4) + 'n'] = $source
}
if ($Targets.Count -eq 0) {
    $Targets = @($sourceByTarget.Keys | Sort-Object)
}
foreach ($target in $Targets) {
    if (-not $sourceByTarget.ContainsKey($target)) {
        throw "Unsupported archive-proven RM smoke target: $target"
    }
}
$Paints = @($Paints | Sort-Object -Unique)

$artifact = if ([IO.Path]::IsPathRooted($ArtifactRoot)) {
    [IO.Path]::GetFullPath($ArtifactRoot)
} else {
    [IO.Path]::GetFullPath((Join-Path $repo $ArtifactRoot))
}
$runtime = if ([IO.Path]::IsPathRooted($RuntimeRoot)) {
    [IO.Path]::GetFullPath($RuntimeRoot)
} else {
    [IO.Path]::GetFullPath((Join-Path $repo $RuntimeRoot))
}
$repoPrefix = $repo.TrimEnd('\') + '\'
foreach ($path in @($artifact, $runtime)) {
    if (-not $path.StartsWith($repoPrefix,
            [StringComparison]::OrdinalIgnoreCase)) {
        throw "RM smoke path escaped the repository: $path"
    }
}
[IO.Directory]::CreateDirectory($artifact) | Out-Null
[IO.Directory]::CreateDirectory($runtime) | Out-Null

function ConvertTo-Gt2CarId([string]$Stem) {
    $characters = '-0123456789abcdefghijklmnopqrstuvwxyz'
    if ($Stem.Length -ne 5) { throw "Invalid GT2 car stem: $Stem" }
    [uint32]$value = 0
    foreach ($character in $Stem.ToCharArray()) {
        $index = $characters.IndexOf($character)
        if ($index -lt 0) { throw "Invalid GT2 car stem: $Stem" }
        $value = [uint32](($value -shl 6) -bor $index)
    }
    return $value
}

function New-RmFixture(
    [string]$Path,
    [int]$ReplacementIndex,
    [int]$ImportedPaint
) {
    $lines = [Collections.Generic.List[string]]::new()
    $lines.Add('# Process-local GT1 RM selector/race smoke.')
    $lines.Add('600+8=CROSS,START')
    $lines.Add('1380+4=CROSS')
    $lines.Add('1980+4=CROSS')
    $lines.Add('2380+4=CROSS')
    $lines.Add('2780+4=CROSS')
    $lines.Add('3660+4=CROSS')
    for ($index = 0; $index -lt $ReplacementIndex; $index++) {
        $lines.Add("$([int](4100 + $index * 120))+4=RIGHT")
    }
    # The developer roster preloads the two-paint alternate body directly.
    # From deterministic palette 0, one Up wraps to paint 1 and two Ups return
    # to paint 0 without invoking an unsupported mid-selector body load.
    $upCount = 2 - $ImportedPaint
    for ($index = 0; $index -lt $upCount; $index++) {
        $lines.Add("$([int](6060 + $index * 520))+4=UP")
    }
    $selectorCapture = 6380 + ($upCount - 1) * 520
    $select = $selectorCapture + 200
    $transmission = $select + 600
    $course = $transmission + 600
    $start = $course + 120
    $lines.Add("$selectorCapture+1=CAPTURE")
    $lines.Add("$select+4=CROSS")
    $lines.Add("$transmission+4=CROSS")
    $lines.Add("$course+4=CROSS")
    $lines.Add("$start+4=CROSS")
    $lines.Add('[race_1]')
    $lines.Add("0+$RaceExitPoll=CROSS")
    $lines.Add('300+1=CAPTURE')
    [IO.File]::WriteAllLines($Path, $lines)
    return $selectorCapture
}

function Test-CompletedRun([string]$ManifestPath) {
    if (-not [IO.File]::Exists($ManifestPath)) { return $false }
    try {
        $record = Get-Content -Raw -LiteralPath $ManifestPath | ConvertFrom-Json
        return ($record.status -eq 'native-smoke-passed' -and
            $record.exeSha256 -eq $ExpectedExeSha256)
    } catch {
        return $false
    }
}

$completed = 0
$skipped = 0
foreach ($target in $Targets) {
    $source = [string]$sourceByTarget[$target]
    $targetEvidence = Join-Path $artifact $target
    [IO.Directory]::CreateDirectory($targetEvidence) | Out-Null
    $missingPaints = @()
    foreach ($paint in $Paints) {
        $runManifest = Join-Path $targetEvidence "paint-$paint\manifest.json"
        if (Test-CompletedRun $runManifest) {
            $skipped++
        } else {
            $missingPaints += $paint
        }
    }
    if ($missingPaints.Count -eq 0) { continue }

    $converted = Join-Path $runtime 'converted'
    $unified = Join-Path $runtime 'unified'
    $convertedManifestPath = Join-Path $converted 'manifest.json'
    $conversionManifest = $null
    if ([IO.File]::Exists($convertedManifestPath)) {
        try {
            $candidateManifest = Get-Content -Raw -LiteralPath `
                $convertedManifestPath | ConvertFrom-Json
            $candidateSmoke = @($candidateManifest.arcadeCars |
                Where-Object {
                    $_.developerLiverySmoke -and
                    $_.targetStem -eq $target -and
                    -not [string]::IsNullOrEmpty($_.alternateBodyStem)
                })
            $candidateRoster = @(
                $candidateManifest.ssr11ArcadeOverlay.carRosters |
                Where-Object {
                    $candidateSmoke.Count -eq 1 -and
                    $_.stem -eq $candidateSmoke[0].stem -and
                    $_.developerReplacement -and
                    [int]$_.replacementIndex -eq 0
                })
            if ($candidateManifest.smokeArcadeLivery -eq $target -and
                $candidateSmoke.Count -eq 1 -and
                $candidateRoster.Count -eq 1) {
                $conversionManifest = $candidateManifest
                'Reused matching deterministic conversion output.' |
                    Set-Content -LiteralPath `
                    (Join-Path $targetEvidence 'conversion.log')
            }
        } catch {
            $conversionManifest = $null
        }
    }
    if ($null -eq $conversionManifest) {
        $conversionOutput = & python $converter `
            --output $converted `
            --gt2-simulation-volume (Join-Path $simulationRoot 'GT2.VOL') `
            --gt2-arcade-volume $arcadeVolume `
            --gt2-arcade-overlay $arcadeOverlay `
            --smoke-arcade-livery $target 2>&1
        $conversionOutput | Set-Content -LiteralPath `
            (Join-Path $targetEvidence 'conversion.log')
        if ($LASTEXITCODE -ne 0) {
            throw "GT1 conversion failed for $target"
        }
        $conversionManifest = Get-Content -Raw -LiteralPath `
            $convertedManifestPath | ConvertFrom-Json
    }
    $fold = @($conversionManifest.liveryFolds.arcade | Where-Object {
        $_.sourceStem -eq $source -and $_.targetStem -eq $target
    })
    if ($fold.Count -ne 1 -or [int]$fold[0].colorCount -ne 2) {
        throw "RM smoke expected one two-paint fold for $source -> $target"
    }

    $oldEnvironment = @{}
    $environment = @{
        GT2_SIMULATION_LOOSE_ROOT = $simulationRoot
        GT2_ARCADE_LOOSE_ROOT = $arcadeRoot
        GT2_ARCADE_ORIGINAL_VOL = $arcadeVolume
        GT2_UNIFIED_LOOSE_ROOT = $unified
        GT2_SIMULATION_PATCH_VOLUMES = (
            Join-Path $converted 'GTPATCH.LIVERY.SIMULATION.VOL')
        GT2_ARCADE_PATCH_VOLUMES = (
            (Join-Path $converted 'GTPATCH.ARCADE.VOL') +
            [IO.Path]::PathSeparator +
            (Join-Path $converted 'GTPATCH.LIVERY.ARCADE.VOL'))
        GT2_ARCADE_PATCHED_OVL = (Join-Path $converted 'GT2.OVL')
    }
    foreach ($item in $environment.GetEnumerator()) {
        $oldEnvironment[$item.Key] = [Environment]::GetEnvironmentVariable(
            $item.Key, 'Process')
        [Environment]::SetEnvironmentVariable(
            $item.Key, [string]$item.Value, 'Process')
    }
    try {
        $prepareOutput = & python $prepare 2>&1
        $prepareOutput | Set-Content -LiteralPath `
            (Join-Path $targetEvidence 'prepare.log')
        if ($LASTEXITCODE -ne 0) {
            throw "Unified materialization failed for $target"
        }
    } finally {
        foreach ($item in $oldEnvironment.GetEnumerator()) {
            [Environment]::SetEnvironmentVariable(
                $item.Key, $item.Value, 'Process')
        }
    }

    $firstColorIndex = [int]$fold[0].firstColorIndex
    $bodyStem = [string]$fold[0].bodyStem
    $smokeCar = @($conversionManifest.arcadeCars | Where-Object {
        $_.developerLiverySmoke -and $_.targetStem -eq $target
    })
    if ($smokeCar.Count -ne 1 -or $smokeCar[0].stem -ne $bodyStem -or
        $smokeCar[0].alternateBodyStem -ne $bodyStem) {
        throw "RM smoke did not preload alternate body $bodyStem for $target"
    }
    $roster = @($conversionManifest.ssr11ArcadeOverlay.carRosters |
        Where-Object {
            $_.stem -eq $bodyStem -and $_.developerReplacement
        })
    if ($roster.Count -ne 1) {
        throw "RM smoke expected one developer roster replacement for $target"
    }
    $replacementIndex = [int]$roster[0].replacementIndex
    $targetId = ConvertTo-Gt2CarId $target
    $bodyId = ConvertTo-Gt2CarId $bodyStem
    foreach ($paint in $missingPaints) {
        if (@(Get-CimInstance Win32_Process `
                -Filter "Name='GranTurismo2PC.exe'").Count) {
            throw "A game process appeared before $target paint $paint"
        }
        $runEvidence = Join-Path $targetEvidence "paint-$paint"
        [IO.Directory]::CreateDirectory($runEvidence) | Out-Null
        $fixture = Join-Path $runtime "$target-paint-$paint.input"
        $paletteIndex = $firstColorIndex + $paint
        $selectorCapture = New-RmFixture `
            $fixture $replacementIndex $paint
        $artifactRelative = [IO.Path]::GetRelativePath(
            (Join-Path $repo 'artifacts'), $runEvidence)
        $oldTrace = [Environment]::GetEnvironmentVariable(
            'RECOMPONE_TRACE_GT2_LIVERIES', 'Process')
        $oldStageExit = [Environment]::GetEnvironmentVariable(
            'RECOMPONE_TEST_EXIT_INPUT_STAGE', 'Process')
        $oldStageExitPoll = [Environment]::GetEnvironmentVariable(
            'RECOMPONE_TEST_EXIT_INPUT_STAGE_POLL', 'Process')
        [Environment]::SetEnvironmentVariable(
            'RECOMPONE_TRACE_GT2_LIVERIES', '1', 'Process')
        [Environment]::SetEnvironmentVariable(
            'RECOMPONE_TEST_EXIT_INPUT_STAGE', 'race_1', 'Process')
        [Environment]::SetEnvironmentVariable(
            'RECOMPONE_TEST_EXIT_INPUT_STAGE_POLL',
            $RaceExitPoll.ToString(),
            'Process')
        try {
            $captureOutput = & $capture `
                -Fixture $fixture `
                -LoosePath $unified `
                -ExitPoll 9000 `
                -ArtifactName $artifactRelative `
                -ExpectedGuest arcade `
                -DeployPath $deployRoot `
                -AiAutoDrive `
                -AiAutoDriveMaxEngagements 2 2>&1
            $captureOutput | Set-Content -LiteralPath `
                (Join-Path $runEvidence 'capture.log')
        } finally {
            [Environment]::SetEnvironmentVariable(
                'RECOMPONE_TRACE_GT2_LIVERIES', $oldTrace, 'Process')
            [Environment]::SetEnvironmentVariable(
                'RECOMPONE_TEST_EXIT_INPUT_STAGE', $oldStageExit, 'Process')
            [Environment]::SetEnvironmentVariable(
                'RECOMPONE_TEST_EXIT_INPUT_STAGE_POLL',
                $oldStageExitPoll,
                'Process')
        }
        $stdout = Get-Content -Raw -LiteralPath `
            (Join-Path $runEvidence 'stdout.log')
        $stderr = Get-Content -Raw -LiteralPath `
            (Join-Path $runEvidence 'stderr.log')
        $expectedTrace = ('[GT2-Livery] palette body=0x{0:X8} index={1} ' +
            'mapped=False -> body=0x{0:X8} index={1}') -f
            $bodyId, $paint
        if ($stdout -notmatch [regex]::Escape($expectedTrace)) {
            throw "Missing exact resolver trace for $target paint $paint"
        }
        $customerMapping = @($fold[0].bodyMappings | Where-Object {
            [int]$_.targetColorIndex -eq $paletteIndex -and
            [int]$_.bodyPaletteIndex -eq $paint
        })
        if ($customerMapping.Count -ne 1) {
            throw "Missing customer target/body mapping for $target paint $paint"
        }
        if ($stderr -match
            'unmapped call|Unhandled exception|unknown software exception') {
            throw "Runtime failure for $target paint $paint"
        }
        $selectorPngs = @(Get-ChildItem -LiteralPath $runEvidence `
            -Filter 'recompone_present__*.png' -File)
        $racePngs = @(Get-ChildItem -LiteralPath $runEvidence `
            -Filter 'recompone_present_race_1_0300_*.png' -File)
        if ($selectorPngs.Count -ne 1 -or $racePngs.Count -ne 1) {
            throw "Expected selector and race PNG for $target paint $paint"
        }
        $pngs = @($selectorPngs[0], $racePngs[0])
        foreach ($extraPng in @(Get-ChildItem -LiteralPath $runEvidence `
                -Filter '*.png' -File | Where-Object {
                    $_.FullName -notin $pngs.FullName
                })) {
            [IO.File]::Delete($extraPng.FullName)
        }
        foreach ($ppm in @(Get-ChildItem -LiteralPath $runEvidence `
                -Filter '*.ppm' -File)) {
            [IO.File]::Delete($ppm.FullName)
        }
        $record = [ordered]@{
            status = 'native-smoke-passed'
            sourceStem = $source
            targetStem = $target
            alternateBodyStem = $bodyStem
            importedPaint = $paint
            targetPaletteIndex = $paletteIndex
            alternateBodyPaletteIndex = $paint
            selectorCapturePoll = $selectorCapture
            raceCapturePoll = 300
            raceExitPoll = $RaceExitPoll
            expectedResolverTrace = $expectedTrace
            nativePreloadedBody = $true
            customerMappingValidated = $true
            exeSha256 = $ExpectedExeSha256
            processLocalInputOnly = $true
            reviewed = $false
            captures = @($pngs | Sort-Object Name | ForEach-Object {
                [ordered]@{
                    file = $_.Name
                    sha256 = (Get-FileHash -LiteralPath $_.FullName `
                        -Algorithm SHA256).Hash
                }
            })
        }
        [IO.File]::WriteAllText(
            (Join-Path $runEvidence 'manifest.json'),
            ($record | ConvertTo-Json -Depth 5))
        $completed++
        Write-Output (
            "GT1 RM native smoke passed: $source -> $target " +
            "paint=$paint palette=$paletteIndex body=$bodyStem")
    }
}

$runManifests = @(Get-ChildItem -LiteralPath $artifact `
    -Filter 'manifest.json' -File -Recurse | Where-Object {
        $_.DirectoryName -ne $artifact
    })
$conversionEvidence = Join-Path $artifact 'conversion-manifest.json'
if ([IO.File]::Exists((Join-Path $runtime 'converted\manifest.json'))) {
    [IO.File]::Copy(
        (Join-Path $runtime 'converted\manifest.json'),
        $conversionEvidence,
        $true)
}
$summary = [ordered]@{
    status = if ($runManifests.Count -eq 62) {
        'native-matrix-complete-review-pending'
    } else {
        'native-matrix-partial'
    }
    expectedBodies = 31
    expectedPaintRuns = 62
    completedPaintRuns = @($runManifests | Where-Object {
        (Get-Content -Raw -LiteralPath $_.FullName | ConvertFrom-Json).status -eq
            'native-smoke-passed'
    }).Count
    executedThisRun = $completed
    skippedThisRun = $skipped
    exeSha256 = $ExpectedExeSha256
    processLocalInputOnly = $true
    conversionManifest = if ([IO.File]::Exists($conversionEvidence)) {
        'conversion-manifest.json'
    } else {
        $null
    }
    conversionManifestSha256 = if ([IO.File]::Exists($conversionEvidence)) {
        (Get-FileHash -LiteralPath $conversionEvidence `
            -Algorithm SHA256).Hash
    } else {
        $null
    }
}
[IO.File]::WriteAllText(
    (Join-Path $artifact 'manifest.json'),
    ($summary | ConvertTo-Json -Depth 4))
Write-Output (
    "GT1 RM matrix progress: $($summary.completedPaintRuns)/62 native runs; " +
    "artifact=$artifact")
