param(
    [ValidateSet('simulation', 'arcade')]
    [string]$Variant = 'simulation',
    [string]$DeployPath = 'OpenGTPS1',
    [string]$DataPath = 'OpenGTPS1',
    [int]$TimeoutSeconds = 30,
    [string]$EvidenceName = 'current',
    [switch]$CaptureVideo,
    [int]$CaptureFrames = 360,
    [switch]$CollectDump
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$deploy = [IO.Path]::GetFullPath((Join-Path $repo $DeployPath))
$data = [IO.Path]::GetFullPath((Join-Path $repo $DataPath))
$evidence = Join-Path $repo "artifacts\intro-playback\$EvidenceName"
New-Item -ItemType Directory -Path $evidence -Force | Out-Null

$exe = Join-Path $deploy 'GranTurismo2PC.exe'
$stdoutPath = Join-Path $evidence 'stdout.log'
$stderrPath = Join-Path $evidence 'stderr.log'
$videoPath = Join-Path $evidence 'intro.mp4'
$start = [Diagnostics.ProcessStartInfo]::new()
$start.FileName = $exe
$start.WorkingDirectory = $evidence
$start.UseShellExecute = $false
$start.CreateNoWindow = $true
$start.RedirectStandardOutput = $true
$start.RedirectStandardError = $true
$start.EnvironmentVariables['SDL_AUDIODRIVER'] = 'dummy'
$start.EnvironmentVariables['RECOMPONE_DISABLE_LIVE_INPUT'] = '1'
$start.EnvironmentVariables['RECOMPONE_SUPPRESS_RUMBLE'] = '1'
$start.EnvironmentVariables['RECOMPONE_TRACE_INPUT'] = '1'
$start.EnvironmentVariables['RECOMPONE_TRACE_MDEC'] = '1'
$start.EnvironmentVariables['RECOMPONE_TRACE_CD'] = '1'
$start.EnvironmentVariables['RECOMPONE_TRACE_GT2_BOOT'] = '1'
$start.EnvironmentVariables['RECOMPONE_CAPTURE_AUTOMATIC_STAGE'] = '0'
$start.EnvironmentVariables['RECOMPONE_EXIT_AFTER_INPUT_POLL'] = '1200'
if ($CaptureVideo) {
    $start.EnvironmentVariables['RECOMPONE_VIDEO_CAPTURE'] = $videoPath
    $start.EnvironmentVariables['RECOMPONE_VIDEO_CAPTURE_FRAMES'] =
        $CaptureFrames.ToString()
    $start.EnvironmentVariables['RECOMPONE_VIDEO_WIDTH'] = '640'
    $start.EnvironmentVariables['RECOMPONE_VIDEO_HEIGHT'] = '480'
    $start.EnvironmentVariables['RECOMPONE_VIDEO_CRF'] = '12'
    $start.EnvironmentVariables['RECOMPONE_EXIT_AFTER_VIDEO_CAPTURE'] = '1'
}
$start.ArgumentList.Add('--headless')
if ($Variant -eq 'arcade') {
    $start.ArgumentList.Add('--start-arcade')
}
$start.ArgumentList.Add($data)

$process = [Diagnostics.Process]::Start($start)
$stdoutTask = $process.StandardOutput.ReadToEndAsync()
$stderrTask = $process.StandardError.ReadToEndAsync()
$completed = $process.WaitForExit($TimeoutSeconds * 1000)
if (-not $completed) {
    if ($CollectDump) {
        & dotnet-dump collect -p $process.Id `
            -o (Join-Path $evidence 'process.dmp')
    }
    $process.Kill($true)
    $process.WaitForExit()
}
$stdout = $stdoutTask.Result
$stderr = $stderrTask.Result
Set-Content -LiteralPath $stdoutPath -Value $stdout -NoNewline
Set-Content -LiteralPath $stderrPath -Value $stderr -NoNewline

if ($process.ExitCode -ne 0 -and $completed) {
    throw "Intro probe exited with code $($process.ExitCode)"
}
if ($stderr -match '(?i)fatal|unmapped|unhandled exception') {
    throw 'Intro probe reported a runtime failure'
}

$mdecMatches = [regex]::Matches(
    $stdout,
    '\[MDEC\] decode .*?hash=0x([0-9A-F]+)')
$mdecHashes = @($mdecMatches | ForEach-Object { $_.Groups[1].Value } |
    Sort-Object -Unique)
if ($Variant -eq 'arcade') {
    $streamBase = 280504
    $streamEnd = 444084
    $readStarts = @([regex]::Matches(
            $stderr,
            '\[LibCd\] ReadS start LBA=(-?\d+)') |
        ForEach-Object { [int]$_.Groups[1].Value })
    if ($readStarts.Count -eq 0) {
        throw 'Arcade intro never started its original ReadS movie stream'
    }
    foreach ($lba in $readStarts) {
        if ($lba -lt $streamBase -or $lba -ge $streamEnd) {
            throw "Arcade intro issued ReadS outside STREAM.DAT: LBA $lba"
        }
    }
    if ($mdecMatches.Count -lt 10 -or $mdecHashes.Count -lt 3) {
        throw (
            'Arcade intro did not produce enough distinct decoded movie frames: ' +
            "decodes=$($mdecMatches.Count) hashes=$($mdecHashes.Count)")
    }
}
if ($CaptureVideo) {
    if (-not $completed) {
        throw 'Intro video capture did not complete before the timeout'
    }
    if ($stderr -notmatch (
            'video capture complete: frames=' + $CaptureFrames + '/' +
            $CaptureFrames + ' ffmpeg exit=0')) {
        throw 'Intro video encoder did not complete the requested frame count'
    }
    if (-not (Test-Path -LiteralPath $videoPath -PathType Leaf)) {
        throw "Intro video capture is missing: $videoPath"
    }
}

Write-Output "intro_probe variant=$Variant completed=$completed exit=$($process.ExitCode)"
Write-Output (
    "mdec_decodes=$($mdecMatches.Count) distinct_hashes=$($mdecHashes.Count) " +
    "video=$($CaptureVideo.IsPresent)")
Write-Output ($stdout -split "`n" | Where-Object {
        $_ -match 'Dispatcher|overlay|MDEC|CD|title|display='
    })
Write-Output ($stderr -split "`n" | Where-Object {
        $_ -match 'MDEC|CD|stage|unmapped|exception'
    })
