param(
    [string]$ArtifactName = 'clean-deployment-current',
    [int]$ExitPoll = 1200
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$deploy = (Resolve-Path (Join-Path $repo 'OpenGTPS1')).Path
$artifact = Join-Path $repo "artifacts\$ArtifactName"
$tempRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\')
$scratch = Join-Path $tempRoot ("OpenGTPS1-clean-audit-" + [guid]::NewGuid().ToString('N'))

if ($deploy.TrimEnd('\') -ne
    'C:\Programming\GitHub\OpenGTPS1\OpenGTPS1') {
    throw "Unexpected deployment path: $deploy"
}
if ($ExitPoll -lt 600) {
    throw 'ExitPoll must allow the loose executable to reach the title flow'
}
if (Test-Path -LiteralPath $artifact) {
    throw "Refusing to overwrite existing audit artifact: $artifact"
}
if (Test-Path -LiteralPath $scratch) {
    throw "Unexpected existing scratch directory: $scratch"
}

New-Item -ItemType Directory -Path $artifact | Out-Null
New-Item -ItemType Directory -Path $scratch | Out-Null

try {
    Copy-Item -Path (Join-Path $deploy '*') -Destination $scratch -Recurse
    $forbidden = @(Get-ChildItem -LiteralPath $scratch -Recurse -File |
        Where-Object {
            $_.Extension -match '^\.(bin|cue|ccd|img|sub)$'
        })
    if ($forbidden.Count -ne 0) {
        throw "Clean copy contains forbidden image files: $($forbidden.FullName -join ', ')"
    }

    $manifestLines = [Collections.Generic.List[string]]::new()
    [int64]$manifestBytes = 0
    foreach ($file in Get-ChildItem -LiteralPath $scratch -Recurse -File |
            Sort-Object FullName) {
        $relative = $file.FullName.Substring($scratch.Length + 1)
        $hash = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
        $manifestLines.Add("$relative|$($file.Length)|$hash")
        $manifestBytes += $file.Length
    }
    [IO.File]::WriteAllLines(
        (Join-Path $artifact 'clean-copy-manifest.txt'),
        $manifestLines)

    $exe = Join-Path $scratch 'GranTurismo2PC.exe'
    $start = [Diagnostics.ProcessStartInfo]::new()
    $start.FileName = $exe
    $start.WorkingDirectory = $scratch
    $start.Arguments = '--headless'
    $start.UseShellExecute = $false
    $start.CreateNoWindow = $true
    $start.RedirectStandardOutput = $true
    $start.RedirectStandardError = $true
    $start.EnvironmentVariables['RECOMPONE_DISABLE_LIVE_INPUT'] = '1'
    $start.EnvironmentVariables['RECOMPONE_SUPPRESS_RUMBLE'] = '1'
    $start.EnvironmentVariables['SDL_AUDIODRIVER'] = 'dummy'
    $start.EnvironmentVariables['RECOMPONE_EXIT_AFTER_INPUT_POLL'] =
        $ExitPoll.ToString()

    $process = [Diagnostics.Process]::Start($start)
    $stdout = $process.StandardOutput.ReadToEndAsync()
    $stderr = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit(180000)) {
        $process.Kill($true)
        $process.WaitForExit()
        throw 'Clean-copy boot timed out'
    }
    $stdoutText = $stdout.Result
    $stderrText = $stderr.Result
    [IO.File]::WriteAllText(
        (Join-Path $artifact 'stdout.log'), $stdoutText)
    [IO.File]::WriteAllText(
        (Join-Path $artifact 'stderr.log'), $stderrText)

    if ($process.ExitCode -ne 0) {
        throw "Clean-copy boot exited with code $($process.ExitCode)"
    }
    if ($stderrText -notmatch 'headless audio backend=dummy' -or
        $stderrText -notmatch 'SDL audio ready: driver=dummy') {
        throw 'Clean-copy boot did not prove the SDL dummy backend'
    }
    if ($stdoutText -notmatch
        '\[CD\] standalone loose files=7 volume=GRANTURISMO2') {
        throw 'Clean-copy boot did not identify the standalone loose layout'
    }
    if ("$stdoutText`n$stderrText" -match
        '(?i)\.(?:bin|cue|ccd|img|sub)\b') {
        throw 'Clean-copy boot referenced a forbidden image path'
    }
    if ($stdoutText -match [regex]::Escape($repo + '\generated') -or
        $stdoutText -match [regex]::Escape($repo + '\vendor') -or
        $stderrText -match [regex]::Escape($repo + '\generated') -or
        $stderrText -match [regex]::Escape($repo + '\vendor')) {
        throw 'Clean-copy boot referenced a development-tree dependency'
    }
    if ($stderrText -match 'Unhandled exception|Fatal error') {
        throw 'Clean-copy boot logged a runtime failure'
    }

    $count = $manifestLines.Count
    Write-Output "clean-copy=$scratch"
    Write-Output "files=$count bytes=$manifestBytes forbidden-images=0"
    Write-Output "standalone-loose=7 development-dependencies=0"
    Write-Output "audio=dummy exitPoll=$ExitPoll"
    Write-Output "evidence=$artifact"
}
finally {
    $scratchFull = [IO.Path]::GetFullPath($scratch)
    if ($scratchFull.StartsWith(
            $tempRoot + '\OpenGTPS1-clean-audit-',
            [StringComparison]::OrdinalIgnoreCase) -and
        (Test-Path -LiteralPath $scratchFull)) {
        [IO.Directory]::Delete($scratchFull, $true)
    }
}
