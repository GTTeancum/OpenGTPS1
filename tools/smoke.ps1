param(
    [int]$Seconds = 15,
    [string]$Name = 'bootstrap',
    [int]$TailLines = 40,
    [switch]$NoTraceCd
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$artifactDirectory = Join-Path $repo "artifacts\$Name"
New-Item -ItemType Directory -Path $artifactDirectory -Force | Out-Null

$dll = Join-Path $repo 'generated\recompiled\bin\Release\net10.0\GranTurismo2PC.dll'
$cue = Join-Path $repo 'Gran Turismo 2 [Simulation Disc] [U] [SCUS-94488].cue'
if (-not (Test-Path -LiteralPath $dll)) { throw "build is missing: $dll" }
if (-not (Test-Path -LiteralPath $cue)) { throw "disc CUE is missing: $cue" }

$env:RECOMPONE_WINDOW_VISIBLE = '0'
$env:RECOMPONE_MUTE = '1'
$env:RECOMPONE_UNTHROTTLED = '1'
$env:RECOMPONE_TRACE_CD = if ($NoTraceCd) { '0' } else { '1' }
$env:RECOMPONE_TRACE_VSYNC = '1'

$start = New-Object Diagnostics.ProcessStartInfo
$start.FileName = 'dotnet'
$start.WorkingDirectory = $repo
$start.Arguments = "`"$dll`" `"$cue`""
$start.UseShellExecute = $false
$start.RedirectStandardOutput = $true
$start.RedirectStandardError = $true

$process = [Diagnostics.Process]::Start($start)
$stdout = $process.StandardOutput.ReadToEndAsync()
$stderr = $process.StandardError.ReadToEndAsync()
$timedOut = -not $process.WaitForExit($Seconds * 1000)
if ($timedOut) {
    $process.Kill()
    $process.WaitForExit()
}

$stdoutPath = Join-Path $artifactDirectory 'stdout.log'
$stderrPath = Join-Path $artifactDirectory 'stderr.log'
[IO.File]::WriteAllText($stdoutPath, $stdout.Result)
[IO.File]::WriteAllText($stderrPath, $stderr.Result)

Write-Output "timed_out=$timedOut exit=$($process.ExitCode)"
Write-Output "stdout=$stdoutPath"
Write-Output "stderr=$stderrPath"

if ($stdout.Result) {
    Write-Output '--- stdout tail ---'
    Write-Output (($stdout.Result -split "`r?`n" | Select-Object -Last $TailLines) -join "`n")
}
if ($stderr.Result) {
    Write-Output '--- stderr tail ---'
    Write-Output (($stderr.Result -split "`r?`n" | Select-Object -Last $TailLines) -join "`n")
}

if ($stdout.Result -match 'Unhandled exception' -or
    $stderr.Result -match 'Unhandled exception' -or
    (-not $timedOut -and $process.ExitCode -ne 0)) {
    throw "GT2 smoke process failed; inspect $artifactDirectory"
}
