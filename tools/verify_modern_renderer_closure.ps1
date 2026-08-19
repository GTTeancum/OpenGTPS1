param(
    [string]$DeployPath = 'artifacts\modern-renderer-track-visibility-r2r-v131\publish',
    [string]$SoakArtifact = 'modern-renderer-track-visibility-r2r-soak-v136',
    [string]$SupraLosslessArtifact = 'modern-renderer-lossless-supra-lap-v70',
    [string]$Ssr11LosslessArtifact = 'modern-renderer-lossless-ssr11-lap-v71',
    [string]$MotionArtifact = 'modern-renderer-ssr11-track-visibility-proof-v126',
    [string]$RejectedPolygonMotionArtifact =
        'modern-renderer-ssr11-polygon-explosion-capture-v122',
    [string]$PolygonSourceArtifact =
        'modern-renderer-ssr11-polygon-source-captures-v123',
    [string]$LongSsr11Artifact =
        'modern-renderer-track-visibility-r2r-ssr11-v132',
    [string]$FullPathArtifact =
        'modern-renderer-track-visibility-r2r-simulation-replay-v134',
    [string]$VisibleArtifact =
        'modern-renderer-track-visibility-visible-ssr11-v135',
    [string]$StreetlightSourceArtifact = 'modern-renderer-ssr11-flare-projected-v97',
    [string]$ReportArtifact = 'modern-renderer-closure-audit-v137'
)

$ErrorActionPreference = 'Stop'
try { (Get-Process -Id $PID).PriorityClass = 'BelowNormal' } catch {}
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$artifacts = Join-Path $repo 'artifacts'
$reportDirectory = Join-Path $artifacts $ReportArtifact
New-Item -ItemType Directory -Path $reportDirectory -Force | Out-Null

function Require([bool]$Condition, [string]$Message) {
    if (-not $Condition) {
        throw $Message
    }
}

function Resolve-RequiredPath([string]$Path, [string]$Description) {
    Require (Test-Path -LiteralPath $Path) "$Description is missing: $Path"
    return (Resolve-Path -LiteralPath $Path).Path
}

function Invoke-LoggedCommand(
    [string]$FilePath,
    [string[]]$Arguments,
    [string]$Name
) {
    $stdout = Join-Path $reportDirectory "$Name.stdout.log"
    $stderr = Join-Path $reportDirectory "$Name.stderr.log"
    $process = Start-Process `
        -FilePath $FilePath `
        -WorkingDirectory $repo `
        -ArgumentList $Arguments `
        -RedirectStandardOutput $stdout `
        -RedirectStandardError $stderr `
        -NoNewWindow `
        -Wait `
        -PassThru
    Require ($process.ExitCode -eq 0) (
        "$Name failed with exit code $($process.ExitCode); " +
        "see $stdout and $stderr")
    return [pscustomobject]@{
        Stdout = (Get-Content -LiteralPath $stdout -Raw -ErrorAction SilentlyContinue)
        Stderr = (Get-Content -LiteralPath $stderr -Raw -ErrorAction SilentlyContinue)
    }
}

$faultPattern = [regex]::new(
    'unknown software exception|application error|Unhandled exception|' +
    'Fatal error|access violation|segmentation fault|stack overflow|' +
    'native crash|unmapped call|unmapped function|missing implementation|' +
    'output wait timeout|render output timeout|\[Native-World\] disabled:|' +
    'submission rejected|dropped truncated|compositor fallback viewport=',
    [Text.RegularExpressions.RegexOptions]::IgnoreCase)

function Require-CleanLog([string]$Text, [string]$Name) {
    $fault = $faultPattern.Match($Text)
    Require (-not $fault.Success) (
        "$Name contains renderer/runtime failure marker '$($fault.Value)'")
    Require ($Text -match '\[Runtime\] shutdown complete; exit=0') (
        "$Name does not prove an orderly zero-exit shutdown")
}

function Read-ArtifactLogs([string]$Path) {
    $logs = @(Get-ChildItem -LiteralPath $Path -Filter '*.log' -File |
        Sort-Object Name)
    Require ($logs.Count -gt 0) "Artifact has no logs: $Path"
    return [string]::Join(
        "`n",
        @($logs | ForEach-Object {
            Get-Content -LiteralPath $_.FullName -Raw
        }))
}

$presentationPattern = [regex]::new(
    '\[Native-Present\] hostHz=([0-9.]+) uniqueHz=([0-9.]+) ' +
    'new=(\d+) actual=(\d+) synthetic=(\d+) repeated=(\d+) ' +
    'compositor=(\d+) worldMiss=(\d+) transitionHold=(\d+)')

function Get-PresentationMetrics([string]$Text) {
    return @($presentationPattern.Matches($Text) | ForEach-Object {
        [pscustomobject]@{
            HostHz = [double]::Parse(
                $_.Groups[1].Value,
                [Globalization.CultureInfo]::InvariantCulture)
            UniqueHz = [double]::Parse(
                $_.Groups[2].Value,
                [Globalization.CultureInfo]::InvariantCulture)
            New = [int]$_.Groups[3].Value
            Actual = [int]$_.Groups[4].Value
            Synthetic = [int]$_.Groups[5].Value
            Repeated = [int]$_.Groups[6].Value
            Compositor = [int]$_.Groups[7].Value
            WorldMiss = [int]$_.Groups[8].Value
            TransitionHold = [int]$_.Groups[9].Value
            Raw = $_.Value
        }
    })
}

function Get-PerfectWorldMetrics([object[]]$Metrics) {
    return @($Metrics | Where-Object {
        $_.New -eq 300 -and
        $_.Actual -eq 150 -and
        $_.Synthetic -eq 150 -and
        $_.Repeated -eq 0 -and
        $_.Compositor -eq 0 -and
        $_.WorldMiss -eq 0 -and
        $_.TransitionHold -eq 0 -and
        $_.UniqueHz -ge 59.5 -and
        $_.UniqueHz -le 60.5
    })
}

$profilePattern = [regex]::new(
    '\[Native-Profile\] scope=track-world samples=(\d+) ' +
    'pipelineP50Ms=([0-9.]+) pipelineP95Ms=([0-9.]+) ' +
    'pipelineP99Ms=([0-9.]+) submitP50Ms=([0-9.]+) ' +
    'submitP95Ms=([0-9.]+) submitP99Ms=([0-9.]+) ' +
    'topologyP50Ms=([0-9.]+) topologyP95Ms=([0-9.]+) ' +
    'topologyP99Ms=([0-9.]+)')

function Get-FinalProfile([string]$Text, [string]$Name) {
    $matches = $profilePattern.Matches($Text)
    Require ($matches.Count -gt 0) "$Name has no track-world percentile profile"
    $match = $matches[$matches.Count - 1]
    $values = @(2..10 | ForEach-Object {
        [double]::Parse(
            $match.Groups[$_].Value,
            [Globalization.CultureInfo]::InvariantCulture)
    })
    Require (
        $values[0] -le $values[1] -and $values[1] -le $values[2] -and
        $values[3] -le $values[4] -and $values[4] -le $values[5] -and
        $values[6] -le $values[7] -and $values[7] -le $values[8]
    ) "$Name contains unordered percentile telemetry"
    Require ($values[2] -le 33.37) (
        "$Name pipeline p99 $($values[2]) ms exceeds the authored-pair budget")
    return [pscustomobject]@{
        Samples = [int]$match.Groups[1].Value
        PipelineP50 = $values[0]
        PipelineP95 = $values[1]
        PipelineP99 = $values[2]
        SubmitP50 = $values[3]
        SubmitP95 = $values[4]
        SubmitP99 = $values[5]
        TopologyP50 = $values[6]
        TopologyP95 = $values[7]
        TopologyP99 = $values[8]
    }
}

function Require-NoBoundedResourceFailure([string]$Text, [string]$Name) {
    $shutdown = [regex]::Matches(
        $Text,
        '\[Native-World\] shutdown .*droppedPending=(\d+) ' +
        'droppedOutputPool=(\d+) droppedPublished=(\d+) ' +
        'outputWaits=(\d+) outputWaitTimeouts=(\d+)')
    Require ($shutdown.Count -gt 0) "$Name has no native shutdown telemetry"
    $last = $shutdown[$shutdown.Count - 1]
    Require (
        [int64]$last.Groups[1].Value -eq 0 -and
        [int64]$last.Groups[2].Value -eq 0 -and
        [int64]$last.Groups[5].Value -eq 0
    ) "$Name exhausted a capture/output resource or timed out"
}

# Prove the current source tree still builds and its fixed modern-only policy
# and native renderer regressions pass. These are intentionally rerun rather
# than trusting timestamps on prior output.
$managedBuild = Invoke-LoggedCommand 'dotnet' @(
    'build', '.\tools\unified-host\GranTurismo2PC.csproj',
    '-c', 'Release', '--no-restore', '-v:q') 'managed-build'
Require ($managedBuild.Stdout -match '0 Error\(s\)') (
    'Managed build did not report zero errors')

$nativeBuild = Invoke-LoggedCommand 'cmake' @(
    '--build', '.\build\native', '--config', 'Release') 'native-build'

$nativeTests = Invoke-LoggedCommand 'ctest' @(
    '--test-dir', '.\build\native', '--output-on-failure', '-C', 'Release') `
    'native-tests'
Require ($nativeTests.Stdout -match '100% tests passed, 0 tests failed out of 6') (
    'Native CTest output did not prove all six targets')

$policyTest = Invoke-LoggedCommand 'dotnet' @(
    'run', '--project',
    '.\tools\modern-renderer-config-tests\ModernRendererConfigTests.csproj',
    '-c', 'Release', '--no-build') 'modern-policy'
Require ($policyTest.Stdout -match 'modern_renderer_config=pass') (
    'Modern-only configuration/presentation policy regression failed')

$diffCheck = Invoke-LoggedCommand 'git' @('diff', '--check') 'git-diff-check'

$deploy = Resolve-RequiredPath (Join-Path $repo $DeployPath) 'ReadyToRun deployment'
foreach ($file in @(
    'GranTurismo2PC.exe',
    'GranTurismo2PC.dll',
    'RecompOne.Runtime.dll',
    'opengt_live_renderer.dll',
    'carda.sav',
    'cardb.sav'
)) {
    $null = Resolve-RequiredPath (Join-Path $deploy $file) "ReadyToRun $file"
}
$verificationPublish = Join-Path $reportDirectory 'current-r2r-publish'
$freshPublish = Invoke-LoggedCommand 'dotnet' @(
    'publish', '.\tools\unified-host\GranTurismo2PC.csproj',
    '-c', 'Release', '-r', 'win-x64', '--self-contained', 'true',
    '-p:PublishReadyToRun=true', '--no-restore', '-v:q',
    '-o', $verificationPublish) 'managed-r2r-publish'
$managedAssemblyHashes = [ordered]@{}
foreach ($assembly in @('GranTurismo2PC.dll', 'RecompOne.Runtime.dll')) {
    $freshAssembly = Resolve-RequiredPath (
        Join-Path $verificationPublish $assembly) "Fresh ReadyToRun $assembly"
    $deployedAssembly = Resolve-RequiredPath (
        Join-Path $deploy $assembly) "Deployed ReadyToRun $assembly"
    $freshHash = (Get-FileHash -LiteralPath $freshAssembly -Algorithm SHA256).Hash
    $deployedHash = (
        Get-FileHash -LiteralPath $deployedAssembly -Algorithm SHA256).Hash
    Require ($freshHash -eq $deployedHash) (
        "ReadyToRun deployment does not contain current $assembly")
    $managedAssemblyHashes[$assembly] = $deployedHash
}
$builtNative = Resolve-RequiredPath (
    Join-Path $repo 'build\native\Release\opengt_live_renderer.dll') `
    'Freshly built native renderer'
$builtNativeHash = (Get-FileHash -LiteralPath $builtNative -Algorithm SHA256).Hash
$deployedNativeHash = (Get-FileHash -LiteralPath (
    Join-Path $deploy 'opengt_live_renderer.dll') -Algorithm SHA256).Hash
Require ($builtNativeHash -eq $deployedNativeHash) (
    'ReadyToRun deployment does not contain the freshly built native renderer')

# Extended soak: validate both the harness status and each underlying runtime
# log, including cadence, LOD proof, profile budget, card hash, and resources.
$soakPath = Resolve-RequiredPath (Join-Path $artifacts $SoakArtifact) 'Extended soak'
$soakStatusPath = Resolve-RequiredPath (Join-Path $soakPath 'status.json') `
    'Extended soak status'
$soakStatus = Get-Content -LiteralPath $soakStatusPath -Raw | ConvertFrom-Json
Require ($soakStatus.state -eq 'passed') 'Extended soak status is not passed'
Require ($soakStatus.completedRuns -ge 6) (
    'Extended soak did not complete at least two full three-scenario rounds')
$scenarioCounts = @{ Arcade = 0; SSR11 = 0; SupraTahiti = 0 }
$soakPerfectWindows = 0
$soakWorstPipelineP99 = 0.0
$soakWorstSubmitP99 = 0.0
$soakWorstTopologyP99 = 0.0
$soakCardHashes = [Collections.Generic.HashSet[string]]::new()
foreach ($pass in $soakStatus.passes) {
    Require ($scenarioCounts.ContainsKey($pass.scenario)) (
        "Unexpected extended-soak scenario '$($pass.scenario)'")
    $scenarioCounts[$pass.scenario]++
    Require ($pass.result -match '^modern_scenario=pass ') (
        "Extended-soak run $($pass.run) lacks a strict pass record")
    Require ($pass.result -match 'perfect_world_windows=(\d+)') (
        "Extended-soak run $($pass.run) lacks a window count")
    $expectedWindows = [int]$Matches[1]
    Require ($pass.result -match 'maximum_track_calls=([1-9]\d*)') (
        "Extended-soak run $($pass.run) lacks Maximum track LOD proof")
    Require ($pass.result -match 'maximum_vehicle_requests=([1-9]\d*)') (
        "Extended-soak run $($pass.run) lacks Maximum vehicle LOD proof")
    Require ($pass.result -match 'card_hash=([0-9A-F]{64})') (
        "Extended-soak run $($pass.run) lacks an exact card hash")
    $null = $soakCardHashes.Add($Matches[1])
    Require ($pass.result -match 'artifact=(.+)$') (
        "Extended-soak run $($pass.run) lacks its artifact path")
    $runArtifact = Resolve-RequiredPath $Matches[1] (
        "Extended-soak run $($pass.run) artifact")
    $runText = Read-ArtifactLogs $runArtifact
    Require-CleanLog $runText "Extended-soak run $($pass.run)"
    Require-NoBoundedResourceFailure $runText "Extended-soak run $($pass.run)"
    $metrics = Get-PresentationMetrics $runText
    $perfect = Get-PerfectWorldMetrics $metrics
    Require ($perfect.Count -eq $expectedWindows) (
        "Extended-soak run $($pass.run) retained $($perfect.Count)/$expectedWindows perfect windows")
    Require ((@($metrics | Measure-Object WorldMiss -Sum).Sum) -eq 0) (
        "Extended-soak run $($pass.run) contains a world miss")
    $profile = Get-FinalProfile $runText "Extended-soak run $($pass.run)"
    $soakWorstPipelineP99 = [math]::Max($soakWorstPipelineP99, $profile.PipelineP99)
    $soakWorstSubmitP99 = [math]::Max(
        $soakWorstSubmitP99,
        $profile.SubmitP99)
    $soakWorstTopologyP99 = [math]::Max(
        $soakWorstTopologyP99,
        $profile.TopologyP99)
    $soakPerfectWindows += $perfect.Count
}
Require (
    $scenarioCounts.Arcade -ge 2 -and
    $scenarioCounts.SSR11 -ge 2 -and
    $scenarioCounts.SupraTahiti -ge 2
) 'Extended soak did not cover every scenario in at least two fresh processes'
Require ($soakPerfectWindows -ge 100) (
    "Extended soak retained only $soakPerfectWindows perfect five-second windows")
Require ($soakCardHashes.Count -eq 1) 'Extended soak card hashes differ between runs'

# Exact transparent-hole audit. Convert every PNG to 32-bit BGRA in memory and
# count the impossible FF00FF clear sentinel over the full 640x480 frame.
Add-Type -AssemblyName System.Drawing
# The severe vertical beam under SSR11's Toyota bridge is a captured source
# regression, not accepted content. The raw projected stream preserves the
# exact signed-coordinate wrap; the native draw-list test proves that modern
# presentation now rejects it under the PS1 1023x511 polygon-span rule.
$streetlightPath = Resolve-RequiredPath (
    Join-Path $artifacts $StreetlightSourceArtifact) `
    'SSR11 wrapped-streetlight source reference'
$streetlightCapture = Resolve-RequiredPath (
    Join-Path $streetlightPath 'race-frame.ogtcap') `
    'SSR11 wrapped-streetlight projected capture'
$streetlightLog = Get-Content -LiteralPath (
    Join-Path $streetlightPath 'render-perspective.log') -Raw
Require ($streetlightLog -match
    'frame=9033 poll=9032 display=320x240 triangles=4150 ' +
    'rasterized=2211 .*projection=perspective') (
    'SSR11 street-light audit is not the exact wrapped projected packet')
Require ((Get-Item -LiteralPath $streetlightCapture).Length -gt 1MB) (
    'SSR11 wrapped-streetlight packet is unexpectedly small')

if (-not ('ExactRgbCounter' -as [type])) {
    Add-Type -ReferencedAssemblies System.Drawing -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

public static class ExactRgbCounter
{
    public static long Count(string path, byte red, byte green, byte blue)
    {
        using (Bitmap source = new Bitmap(path))
        using (Bitmap image = source.Clone(
            new Rectangle(0, 0, source.Width, source.Height),
            PixelFormat.Format32bppArgb))
        {
            Rectangle bounds = new Rectangle(0, 0, image.Width, image.Height);
            BitmapData data = image.LockBits(
                bounds,
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);
            try
            {
                int stride = Math.Abs(data.Stride);
                byte[] pixels = new byte[stride * image.Height];
                Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);
                long count = 0;
                for (int y = 0; y < image.Height; ++y)
                {
                    int row = data.Stride >= 0
                        ? y * stride
                        : (image.Height - 1 - y) * stride;
                    for (int x = 0; x < image.Width; ++x)
                    {
                        int offset = row + x * 4;
                        if (pixels[offset] == blue &&
                            pixels[offset + 1] == green &&
                            pixels[offset + 2] == red)
                            ++count;
                    }
                }
                return count;
            }
            finally
            {
                image.UnlockBits(data);
            }
        }
    }
}
'@
}

$losslessFrames = 0
$clearSentinelPixels = 0L
foreach ($artifactName in @($SupraLosslessArtifact, $Ssr11LosslessArtifact)) {
    $path = Resolve-RequiredPath (Join-Path $artifacts $artifactName) `
        "$artifactName lossless audit"
    $images = @(Get-ChildItem -LiteralPath $path -Filter 'lap-*-sentinel.png' -File)
    Require ($images.Count -eq 15) "$artifactName does not contain 15 sentinel renders"
    foreach ($image in $images) {
        $clearSentinelPixels += [ExactRgbCounter]::Count(
            $image.FullName,
            255,
            0,
            255)
    }
    $losslessFrames += $images.Count
    $text = Read-ArtifactLogs $path
    Require-CleanLog $text $artifactName
}
Require ($losslessFrames -eq 30) 'Lossless audit did not cover 30 distributed frames'
Require ($clearSentinelPixels -eq 0) (
    "Lossless audit found $clearSentinelPixels exposed clear-sentinel pixels")

# Exact polygon-explosion regression. Preserve both authored endpoints around
# the rejected midpoint sequence and require a native-detail rerender for every
# adjacent pair. This does not infer correctness from the long video alone.
$polygonSourcePath = Resolve-RequiredPath (
    Join-Path $artifacts $PolygonSourceArtifact) `
    'SSR11 polygon-explosion source packets'
$polygonPackets = @(
    Get-ChildItem -LiteralPath $polygonSourcePath -Filter 'ssr11-*.ogtwcap' -File)
Require ($polygonPackets.Count -eq 64) (
    "Polygon source artifact retains $($polygonPackets.Count)/64 authored packets")
$fixedMidpointPath = Resolve-RequiredPath (
    Join-Path $polygonSourcePath 'midpoints-fixed-v124') `
    'Corrected SSR11 midpoint rerenders'
$fixedMidpointImages = @(
    Get-ChildItem -LiteralPath $fixedMidpointPath -Filter 'pair-*.png' -File)
Require ($fixedMidpointImages.Count -eq 26) (
    "Corrected midpoint artifact retains $($fixedMidpointImages.Count)/26 pairs")
foreach ($image in $fixedMidpointImages) {
    $bitmap = [Drawing.Bitmap]::new($image.FullName)
    try {
        Require ($bitmap.Width -eq 1280 -and $bitmap.Height -eq 960) (
            "$($image.Name) is not a full 1280x960 rerender")
    } finally {
        $bitmap.Dispose()
    }
}
$fixedMidpointSummary = Resolve-RequiredPath (
    Join-Path $fixedMidpointPath 'summary.txt') 'Corrected midpoint summary'
$fixedMidpointSummaryLines = @(
    Get-Content -LiteralPath $fixedMidpointSummary |
        Where-Object { $_ -match '^previousFrame=' })
Require ($fixedMidpointSummaryLines.Count -eq 26) (
    'Corrected midpoint summary does not describe all 26 exact pairs')
$fixedMidpointSheetPath = Resolve-RequiredPath (
    Join-Path $fixedMidpointPath 'midpoint-sequence-fixed.png') `
    'Corrected midpoint sequence sheet'
$fixedMidpointSheet = [Drawing.Bitmap]::new($fixedMidpointSheetPath)
try {
    Require (
        $fixedMidpointSheet.Width -eq 3840 -and
        $fixedMidpointSheet.Height -eq 2400
    ) 'Corrected midpoint sheet does not retain 640x480 tiles'
} finally {
    $fixedMidpointSheet.Dispose()
}

$rejectedMotionPath = Resolve-RequiredPath (
    Join-Path $artifacts $RejectedPolygonMotionArtifact) `
    'Rejected SSR11 polygon-explosion motion proof'
$rejectedMotionText = Read-ArtifactLogs $rejectedMotionPath
Require-CleanLog $rejectedMotionText 'Rejected SSR11 polygon-explosion motion proof'
$rejectedLumaValues = @(
    Get-Content (Join-Path $rejectedMotionPath 'signalstats.log') |
        ForEach-Object {
            if ($_ -match 'lavfi.signalstats.YAVG=([0-9.]+)') {
                [double]::Parse(
                    $Matches[1],
                    [Globalization.CultureInfo]::InvariantCulture)
            }
        })
Require ($rejectedLumaValues.Count -eq 5400) (
    'Rejected polygon-explosion proof lacks 5,400 luminance samples')
$rejectedLumaDeltas = for (
    $index = 1;
    $index -lt $rejectedLumaValues.Count;
    ++$index
) {
    [math]::Abs(
        $rejectedLumaValues[$index] - $rejectedLumaValues[$index - 1])
}
$rejectedPolygonLumaMaximum = (@($rejectedLumaDeltas | Sort-Object))[-1]
Require ($rejectedPolygonLumaMaximum -gt 10.0) (
    'Rejected capture no longer reproduces the polygon-explosion signal')

# Motion proof: recompute exact frame hashes and adjacent uniqueness rather
# than accepting a prewritten summary.
$motionPath = Resolve-RequiredPath (Join-Path $artifacts $MotionArtifact) `
    'SSR11 motion artifact'
$motionText = Read-ArtifactLogs $motionPath
Require-CleanLog $motionText 'SSR11 motion artifact'
$motionProfile = Get-FinalProfile $motionText 'SSR11 motion artifact'
foreach ($name in @(
    'GT2_Modern_Final_Arcade-ssr11.mp4',
    'frames.framemd5',
    'adjacent-ssim.log',
    'signalstats.log',
    'contact-sheet.png',
    'motion-sheet.png',
    'temporal-outliers.png',
    'previous-failure-frames-fixed.png'
)) {
    $null = Resolve-RequiredPath (Join-Path $motionPath $name) "Motion $name"
}
$proofDimensions = [ordered]@{}
$expectedProofDimensions = [ordered]@{
    'contact-sheet.png' = [Drawing.Size]::new(3200, 1440)
    'motion-sheet.png' = [Drawing.Size]::new(3840, 1920)
    'temporal-outliers.png' = [Drawing.Size]::new(2560, 1440)
    'previous-failure-frames-fixed.png' = [Drawing.Size]::new(3200, 960)
}
foreach ($proof in $expectedProofDimensions.GetEnumerator()) {
    $bitmap = [Drawing.Bitmap]::new((Join-Path $motionPath $proof.Key))
    try {
        Require (
            $bitmap.Width -eq $proof.Value.Width -and
            $bitmap.Height -eq $proof.Value.Height
        ) "$($proof.Key) does not retain 640x480 proof tiles"
        $proofDimensions[$proof.Key] =
            "$($bitmap.Width)x$($bitmap.Height)"
    } finally {
        $bitmap.Dispose()
    }
}
$motionHashes = @(Get-Content (Join-Path $motionPath 'frames.framemd5') |
    Where-Object { $_ -notmatch '^#' } |
    ForEach-Object { ($_ -split ',')[-1].Trim() })
$motionAdjacentDuplicates = 0
for ($index = 1; $index -lt $motionHashes.Count; ++$index) {
    if ($motionHashes[$index] -eq $motionHashes[$index - 1]) {
        $motionAdjacentDuplicates++
    }
}
Require ($motionHashes.Count -ge 5400) 'SSR11 motion proof is shorter than 90 seconds'
Require ((@($motionHashes | Sort-Object -Unique).Count) -eq $motionHashes.Count) (
    'SSR11 motion proof contains non-adjacent duplicate images')
Require ($motionAdjacentDuplicates -eq 0) (
    'SSR11 motion proof contains an adjacent duplicate image')
$ssimValues = @(Get-Content (Join-Path $motionPath 'adjacent-ssim.log') |
    ForEach-Object {
        if ($_ -match '^n:\d+.*All:([0-9.]+)') {
            [double]::Parse(
                $Matches[1],
                [Globalization.CultureInfo]::InvariantCulture)
        }
    })
Require ($ssimValues.Count -eq ($motionHashes.Count - 1)) (
    'SSR11 motion proof lacks one SSIM value per adjacent frame pair')
$orderedSsim = @($ssimValues | Sort-Object)
$ssimP1 = $orderedSsim[[math]::Floor(($orderedSsim.Count - 1) * 0.01)]
$ssimMedian = $orderedSsim[[math]::Floor(($orderedSsim.Count - 1) * 0.5)]
$lumaValues = @(Get-Content (Join-Path $motionPath 'signalstats.log') |
    ForEach-Object {
        if ($_ -match 'lavfi.signalstats.YAVG=([0-9.]+)') {
            [double]::Parse(
                $Matches[1],
                [Globalization.CultureInfo]::InvariantCulture)
        }
    })
Require ($lumaValues.Count -eq $motionHashes.Count) (
    'SSR11 motion proof lacks one luminance sample per frame')
$lumaDeltas = for ($index = 1; $index -lt $lumaValues.Count; ++$index) {
    [math]::Abs($lumaValues[$index] - $lumaValues[$index - 1])
}
$orderedLumaDeltas = @($lumaDeltas | Sort-Object)
$lumaDeltaP99 = $orderedLumaDeltas[
    [math]::Floor(($orderedLumaDeltas.Count - 1) * 0.99)]
$lumaDeltaMaximum = $orderedLumaDeltas[-1]
Require ($lumaDeltaMaximum -lt 10.0) (
    "SSR11 motion retains a street-light-sized luminance spike: $lumaDeltaMaximum")
Require ($lumaDeltaMaximum -lt ($rejectedPolygonLumaMaximum * 0.5)) (
    'Corrected motion did not materially reduce the reproduced explosion signal')

# Longer SSR11 course coverage proves that the bounded horizon advances every
# stock visibility event without restoring the false surfaces of the rejected
# all-sector union.
$longSsr11Path = Resolve-RequiredPath (
    Join-Path $artifacts $LongSsr11Artifact) 'Long SSR11 horizon artifact'
$longSsr11Text = Read-ArtifactLogs $longSsr11Path
Require-CleanLog $longSsr11Text 'Long SSR11 horizon artifact'
Require-NoBoundedResourceFailure $longSsr11Text 'Long SSR11 horizon artifact'
$longMetrics = Get-PresentationMetrics $longSsr11Text
$longPerfect = Get-PerfectWorldMetrics $longMetrics
Require ($longPerfect.Count -eq 17) (
    "Long SSR11 horizon retained $($longPerfect.Count)/17 perfect windows")
Require ((@($longMetrics | Measure-Object WorldMiss -Sum).Sum) -eq 0) (
    'Long SSR11 horizon contains a world miss')
Require ($longSsr11Text -match
    '\[GT2-Renderer-Audit\] expandedVisibility calls=(\d+) radius=3 ' +
    'stockEntries=(\d+) outputEntries=(\d+) maximumAdded=(\d+)') (
    'Long SSR11 horizon lacks bounded visibility audit')
$longVisibilityCalls = [long]$Matches[1]
$longMaximumAdded = [int]$Matches[4]
Require ($longVisibilityCalls -ge 5000 -and $longMaximumAdded -gt 0) (
    'Long SSR11 horizon did not exercise expanded visibility')
Require ($longSsr11Text -match
    '\[GT2-Renderer-Audit\] visibilityTransitions sectors=(\d+) ' +
    'stockAdds=(\d+) stockRemoves=(\d+) stockAddsPrecovered=(\d+) ' +
    'stockRemovesRetained=(\d+) stockMaximum=(\d+) ' +
    'expandedAdds=(\d+) expandedRemoves=(\d+) expandedMaximum=(\d+)') (
    'Long SSR11 horizon lacks sector-transition audit')
$longSectors = [int]$Matches[1]
$longStockAdds = [int]$Matches[2]
$longStockRemoves = [int]$Matches[3]
$longAddsPrecovered = [int]$Matches[4]
$longRemovesRetained = [int]$Matches[5]
Require (
    $longSectors -ge 80 -and
    $longStockAdds -eq $longAddsPrecovered -and
    $longStockRemoves -eq $longRemovesRetained
) 'Long SSR11 horizon leaves a stock sector pop/removal uncovered'
$longProfile = Get-FinalProfile $longSsr11Text 'Long SSR11 horizon artifact'
Require ($longProfile.PipelineP99 -lt 33.37) (
    'Long SSR11 horizon exceeds the authored-pair render budget')

# Gran Turismo Mode full path: race, authored Results handoff, natural replay,
# Maximum LOD, unique paced motion, exact card restore, and clean resources.
$fullPath = Resolve-RequiredPath (Join-Path $artifacts $FullPathArtifact) `
    'Gran Turismo Mode full-path artifact'
$fullText = Read-ArtifactLogs $fullPath
Require-CleanLog $fullText 'Gran Turismo Mode full-path artifact'
Require-NoBoundedResourceFailure $fullText 'Gran Turismo Mode full-path artifact'
$fullMetrics = Get-PresentationMetrics $fullText
$fullPerfect = Get-PerfectWorldMetrics $fullMetrics
Require ($fullPerfect.Count -eq 50) (
    "Gran Turismo Mode full path retained $($fullPerfect.Count)/50 perfect windows")
Require ((@($fullMetrics | Measure-Object WorldMiss -Sum).Sum) -eq 0) (
    'Gran Turismo Mode full path contains a world miss')
Require ($fullText -match "\[Input\] stage 'race_1'") (
    'Gran Turismo Mode full path did not reach its race')
Require ($fullText -match "\[Input\] stage 'replay_1'") (
    'Gran Turismo Mode full path did not reach natural replay')
Require ($fullText -match
    '\[GT2-Renderer-Audit\] track raceCalls=(\d+) replayCalls=(\d+) ' +
    'maximumCalls=(\d+) stockCalls=0 entriesScanned=(\d+) ' +
    'nonzeroSelectors=0 nullLists=0 invalidLists=0') (
    'Gran Turismo Mode full path lacks exact Maximum track/scenery LOD proof')
$fullRaceLodCalls = [int]$Matches[1]
$fullReplayLodCalls = [int]$Matches[2]
$fullLodEntries = [int]$Matches[4]
Require ($fullRaceLodCalls -gt 0 -and $fullReplayLodCalls -gt 0) (
    'Gran Turismo Mode LOD proof does not cover race and replay')
Require ($fullText -match
    '\[GT2-Renderer-Audit\] vehicles requests=(\d+) selectors=\[1:\1\]') (
    'Gran Turismo Mode full path lacks Maximum vehicle LOD proof')
$fullProfile = Get-FinalProfile $fullText 'Gran Turismo Mode full-path artifact'

# Visible swap-path proof: validate only the real-time race interval beginning
# at AI engagement, not earlier menu/loading transitions.
$visiblePath = Resolve-RequiredPath (Join-Path $artifacts $VisibleArtifact) `
    'Visible SSR11 review artifact'
$visibleText = Read-ArtifactLogs $visiblePath
Require-CleanLog $visibleText 'Visible SSR11 review artifact'
Require-NoBoundedResourceFailure $visibleText 'Visible SSR11 review artifact'
$raceMarker = $visibleText.IndexOf('auto-drive engaged pass=1/2 phase=race')
Require ($raceMarker -ge 0) 'Visible SSR11 review did not reach auto-drive race'
$visibleRaceText = $visibleText.Substring($raceMarker)
$visibleMetrics = Get-PresentationMetrics $visibleRaceText
$visibleWorldMetrics = @($visibleMetrics | Where-Object {
    $_.New -gt 0 -or $_.Actual -gt 0 -or $_.Synthetic -gt 0 -or
    $_.Repeated -gt 0 -or $_.WorldMiss -gt 0
})
$visibleSteadyMetrics = @($visibleWorldMetrics | Select-Object -Skip 1)
$visiblePerfect = Get-PerfectWorldMetrics $visibleSteadyMetrics
Require ($visibleSteadyMetrics.Count -ge 16) (
    'Visible SSR11 review did not retain at least 80 seconds of race telemetry')
Require ($visiblePerfect.Count -eq $visibleSteadyMetrics.Count) (
    "Visible SSR11 review retained $($visiblePerfect.Count)/$($visibleSteadyMetrics.Count) perfect steady world windows")
Require ((@($visibleMetrics | Measure-Object WorldMiss -Sum).Sum) -eq 0) (
    'Visible SSR11 review contains a world miss')
Require ($visibleRaceText -notmatch '\[Host-Long-Swap\]') (
    'Visible SSR11 review contains a 40+ ms surface swap')
$visibleProfile = Get-FinalProfile $visibleRaceText 'Visible SSR11 review artifact'

$report = [ordered]@{
    state = 'machine-pass-user-visual-pending'
    generatedUtc = [DateTime]::UtcNow.ToString('o')
    currentTree = [ordered]@{
        managedBuildErrors = 0
        nativeTests = '6/6'
        modernOnlyPolicy = 'pass'
        diffCheck = 'pass'
        readyToRunDeployment = $deploy
        managedAssemblySha256 = $managedAssemblyHashes
        nativeRendererSha256 = $deployedNativeHash
    }
    extendedSoak = [ordered]@{
        runs = $soakStatus.completedRuns
        scenarios = [ordered]@{
            Arcade = $scenarioCounts.Arcade
            SSR11 = $scenarioCounts.SSR11
            SupraTahiti = $scenarioCounts.SupraTahiti
        }
        perfectWorldWindows = $soakPerfectWindows
        cardHashes = @($soakCardHashes)
        worstP99Ms = [ordered]@{
            pipeline = $soakWorstPipelineP99
            submit = $soakWorstSubmitP99
            topology = $soakWorstTopologyP99
        }
    }
    losslessGeometry = [ordered]@{
        distributedFrames = $losslessFrames
        clearSentinelPixels = $clearSentinelPixels
    }
    ssr11PolygonExplosionRegression = [ordered]@{
        authoredPackets = $polygonPackets.Count
        correctedMidpointPairs = $fixedMidpointImages.Count
        midpointDimensions = '1280x960'
        sequenceSheetDimensions = '3840x2400'
        cause = 'stale culled track triangles incorrectly received whole-group rigid interpolation'
        fix = 'exact provenance interpolation for track commands; whole-group rigid motion is vehicle-only'
        regression = 'opengt_world_interpolation_tests'
    }
    ssr11Motion = [ordered]@{
        frames = $motionHashes.Count
        uniqueFrames = @($motionHashes | Sort-Object -Unique).Count
        adjacentDuplicates = $motionAdjacentDuplicates
        ssim = [ordered]@{
            minimum = $orderedSsim[0]
            p1 = $ssimP1
            median = $ssimMedian
        }
        luminanceDelta = [ordered]@{
            p99 = $lumaDeltaP99
            maximum = $lumaDeltaMaximum
            rejectedStreetlightMaximum = 28.2055
            rejectedPolygonExplosionMaximum = $rejectedPolygonLumaMaximum
        }
        proofDimensions = $proofDimensions
        profile = $motionProfile
    }
    ssr11VisibilityHorizon = [ordered]@{
        radiusSectors = 3
        calls = $longVisibilityCalls
        sectorTransitions = $longSectors
        stockAdds = $longStockAdds
        stockAddsPrecovered = $longAddsPrecovered
        stockRemoves = $longStockRemoves
        stockRemovesRetained = $longRemovesRetained
        maximumAdditionalObjects = $longMaximumAdded
        perfectWorldWindows = $longPerfect.Count
        minimumUniqueHz = ($longPerfect.UniqueHz | Measure-Object -Minimum).Minimum
        maximumUniqueHz = ($longPerfect.UniqueHz | Measure-Object -Maximum).Maximum
        profile = $longProfile
    }
    granTurismoFullPath = [ordered]@{
        perfectWorldWindows = $fullPerfect.Count
        raceLodCalls = $fullRaceLodCalls
        replayLodCalls = $fullReplayLodCalls
        lodEntries = $fullLodEntries
        profile = $fullProfile
    }
    visibleSsr11 = [ordered]@{
        perfectWorldWindows = $visiblePerfect.Count
        minimumUniqueHz = ($visiblePerfect.UniqueHz | Measure-Object -Minimum).Minimum
        maximumUniqueHz = ($visiblePerfect.UniqueHz | Measure-Object -Maximum).Maximum
        longSwapIntervals = 0
        profile = $visibleProfile
    }
    rejectedSsr11StreetlightWrap = [ordered]@{
        source = 'pre-render projected triangle stream'
        frame = 9033
        poll = 9032
        triangles = 4150
        rasterized = 2211
        cause = 'signed 11-bit screen-coordinate wrap exceeded PS1 1023x511 polygon span'
        regression = 'opengt_world_draw_list_tests'
        capture = $streetlightCapture
    }
    crashOrUnmappedSignatures = 0
    remainingGate = 'User visual acceptance of the final visible review'
}
$reportPath = Join-Path $reportDirectory 'closure-report.json'
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportPath -Encoding UTF8

Write-Output (
    'modern_renderer_machine_closure=pass ' +
    "soak_runs=$($soakStatus.completedRuns) soak_windows=$soakPerfectWindows " +
    "lossless_frames=$losslessFrames sentinel_pixels=$clearSentinelPixels " +
    "polygon_pairs=$($fixedMidpointImages.Count)/26 " +
    "motion_unique=$($motionHashes.Count)/$($motionHashes.Count) " +
    "motion_adjacent_duplicates=$motionAdjacentDuplicates " +
    "motion_luma_delta_max=$lumaDeltaMaximum " +
    "ssr11_horizon_precovered=$longAddsPrecovered/$longStockAdds " +
    "ssr11_horizon_retained=$longRemovesRetained/$longStockRemoves " +
    "simulation_windows=$($fullPerfect.Count) " +
    "visible_windows=$($visiblePerfect.Count) " +
    "visible_unique_hz=$(($visiblePerfect.UniqueHz | Measure-Object -Minimum).Minimum)/" +
    "$(($visiblePerfect.UniqueHz | Measure-Object -Maximum).Maximum) " +
    "user_visual_acceptance=pending report=$reportPath")
