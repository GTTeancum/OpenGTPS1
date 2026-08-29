param(
    [int]$ExitPoll = 461,
    [int]$TimeoutSeconds = 180,
    [switch]$KeepLogs,
    [string]$DeployPath = 'work\publish-seattle'
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$work = (Resolve-Path (Join-Path $repo 'work')).Path.TrimEnd('\')
$deploy = if ([IO.Path]::IsPathRooted($DeployPath)) {
    (Resolve-Path -LiteralPath $DeployPath).Path
} else {
    (Resolve-Path -LiteralPath (Join-Path $repo $DeployPath)).Path
}
$sourceExe = Join-Path $deploy 'GranTurismo2PC.exe'
$smokeDeploy = Join-Path $work 'release-smoke-run'
$exe = Join-Path $smokeDeploy 'GranTurismo2PC.exe'
$cardA = Join-Path $work 'release-smoke-carda.sav'
$cardB = Join-Path $work 'release-smoke-cardb.sav'
$stdoutPath = Join-Path $work 'release-smoke.stdout.log'
$stderrPath = Join-Path $work 'release-smoke.stderr.log'
$bundleExtract = Join-Path $work 'release-smoke-bundle-extract'

if (-not (Test-Path -LiteralPath $sourceExe -PathType Leaf)) {
    throw "Single-file release executable is missing: $sourceExe"
}
if (Test-Path -LiteralPath $smokeDeploy) {
    throw "Refusing to overwrite a stale release-smoke directory: $smokeDeploy"
}
if (Test-Path -LiteralPath $bundleExtract) {
    throw "Refusing to overwrite stale bundle extraction: $bundleExtract"
}
foreach ($path in @($cardA, $cardB)) {
    $full = [IO.Path]::GetFullPath($path)
    if (-not $full.StartsWith(
            $work + '\', [StringComparison]::OrdinalIgnoreCase)) {
        throw "Temporary card escaped the work directory: $full"
    }
}

Copy-Item -LiteralPath (Join-Path $repo 'carda.sav') -Destination $cardA -Force
Copy-Item -LiteralPath (Join-Path $repo 'cardb.sav') -Destination $cardB -Force
New-Item -ItemType Directory -Path $smokeDeploy | Out-Null
Copy-Item -LiteralPath $sourceExe -Destination $exe

$start = [Diagnostics.ProcessStartInfo]::new()
$start.FileName = $exe
$start.WorkingDirectory = $deploy
$start.Arguments = '--headless --arcade-race seattle-circuit'
$start.UseShellExecute = $false
$start.CreateNoWindow = $true
$start.RedirectStandardOutput = $true
$start.RedirectStandardError = $true
$environment = [ordered]@{
    SDL_AUDIODRIVER = 'dummy'
    RECOMPONE_WINDOW_VISIBLE = '0'
    RECOMPONE_DISABLE_LIVE_INPUT = '1'
    RECOMPONE_SUPPRESS_RUMBLE = '1'
    RECOMPONE_EXIT_AFTER_INPUT_POLL = $ExitPoll.ToString()
    RECOMPONE_CARD_A_PATH = $cardA
    RECOMPONE_CARD_B_PATH = $cardB
    DOTNET_BUNDLE_EXTRACT_BASE_DIR = $bundleExtract
}
foreach ($entry in $environment.GetEnumerator()) {
    $start.Environment[$entry.Key] = $entry.Value
}

$process = $null
$stdout = ''
$stderr = ''
$passed = $false
try {
    $process = [Diagnostics.Process]::Start($start)
    $stdoutTask = $process.StandardOutput.ReadToEndAsync()
    $stderrTask = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
        try {
            $process.Kill($true)
        } catch [Management.Automation.MethodException] {
            Stop-Process -Id $process.Id -Force
        }
        $process.WaitForExit()
        throw "Single-file release smoke exceeded ${TimeoutSeconds}s"
    }
    $stdout = $stdoutTask.Result
    $stderr = $stderrTask.Result
    $fullText = $stdout + [Environment]::NewLine + $stderr
    [IO.File]::WriteAllText($stdoutPath, $stdout)
    [IO.File]::WriteAllText($stderrPath, $stderr)

    if ($process.ExitCode -ne 0) {
        throw "Single-file release smoke exited with code $($process.ExitCode)"
    }
    if ($stdout -notmatch
        '\[Host\] direct Arcade race requested: seattle-circuit') {
        throw 'Direct Seattle race marker is missing'
    }
    if ($stdout -notmatch '\[Host\] auto-resolved unified game data:') {
        throw 'Unified game data was not auto-resolved from the release build'
    }
    if ($fullText -notmatch
        '\[GT2-Direct\] native Seattle construction verified') {
        throw 'Native Seattle construction was not proven'
    }
    if ($fullText -notmatch
        '\[Render-UV\].*worldFallback=0 trackFallback=0 ' +
        'vehicleFallback=0 otherFallback=0') {
        throw 'The release build used a modern-projection fallback'
    }
    if ($fullText -notmatch
        '\[Native-World-Classification\] frames=[1-9]\d* ' +
        'classifiedWorldCommands=[1-9]\d* unclassifiedWorldCommands=0 ' +
        'framesWithUnclassifiedWorld=0 maximumUnclassifiedWorld=0') {
        throw 'The release build retained ownerless 3D geometry'
    }
    if ($fullText -notmatch '\[Runtime\] shutdown complete; exit=0') {
        throw 'The release build did not shut down cleanly'
    }
    $passed = $true
} finally {
    Remove-Item -LiteralPath $cardA -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $cardB -Force -ErrorAction SilentlyContinue
    if (Test-Path -LiteralPath $smokeDeploy -PathType Container) {
        $resolvedSmoke = (Resolve-Path -LiteralPath $smokeDeploy).Path
        if (-not (Split-Path $resolvedSmoke -Parent).Equals(
                $work, [StringComparison]::OrdinalIgnoreCase)) {
            throw "Release-smoke cleanup escaped the work directory: $resolvedSmoke"
        }
        Remove-Item -LiteralPath $resolvedSmoke -Recurse -Force
    }
    if (Test-Path -LiteralPath $bundleExtract -PathType Container) {
        $resolvedExtract = (Resolve-Path -LiteralPath $bundleExtract).Path
        if (-not (Split-Path $resolvedExtract -Parent).Equals(
                $work, [StringComparison]::OrdinalIgnoreCase)) {
            throw "Bundle-extract cleanup escaped the work directory: $resolvedExtract"
        }
        Remove-Item -LiteralPath $resolvedExtract -Recurse -Force
    }
}

if ($passed -and -not $KeepLogs) {
    Remove-Item -LiteralPath $stdoutPath -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $stderrPath -Force -ErrorAction SilentlyContinue
}

Write-Host 'Single-file Seattle release smoke passed'
Write-Host "  executable: $sourceExe"
