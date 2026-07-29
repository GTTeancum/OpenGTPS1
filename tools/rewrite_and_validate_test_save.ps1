param(
    [string]$ArtifactName = 'convenience-save-packaged-current'
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$deploy = Join-Path $repo 'OpenGTPS1'
$exe = Join-Path $deploy 'GranTurismo2PC.exe'
$card = Join-Path $deploy 'carda.sav'
$rewriteFixture = Join-Path $repo 'tests\fixtures\rewrite-packaged-save.input'
$validateFixture = Join-Path $repo 'tests\fixtures\validate-test-save.input'
$artifact = Join-Path $repo "artifacts\$ArtifactName"
$backup = Join-Path $artifact 'carda-before-rewrite.sav'
$validated = Join-Path $artifact 'carda-validated.sav'

New-Item -ItemType Directory -Path $artifact -Force | Out-Null
Copy-Item -LiteralPath $card -Destination $backup -Force
$beforeHash = (Get-FileHash -LiteralPath $card -Algorithm SHA256).Hash
$succeeded = $false

function Invoke-SafeSaveRun {
    param(
        [string]$Name,
        [string]$Fixture,
        [int]$ExitPoll,
        [bool]$CreateSave
    )

    $start = [Diagnostics.ProcessStartInfo]::new()
    $start.FileName = $exe
    $start.WorkingDirectory = $deploy
    $start.Arguments = '--headless'
    $start.UseShellExecute = $false
    $start.CreateNoWindow = $true
    $start.RedirectStandardOutput = $true
    $start.RedirectStandardError = $true
    $start.EnvironmentVariables['RECOMPONE_INPUT_FILE'] = $Fixture
    $start.EnvironmentVariables['RECOMPONE_DISABLE_LIVE_INPUT'] = '1'
    $start.EnvironmentVariables['RECOMPONE_SUPPRESS_RUMBLE'] = '1'
    $start.EnvironmentVariables['RECOMPONE_UNTHROTTLED'] = '1'
    $start.EnvironmentVariables['RECOMPONE_TRACE_GT2_SAVE'] = '1'
    $start.EnvironmentVariables['RECOMPONE_EXIT_AFTER_INPUT_POLL'] = $ExitPoll.ToString()
    if ($CreateSave) {
        $start.EnvironmentVariables['RECOMPONE_GT2_CREATE_TEST_SAVE'] = '1'
    }

    $process = [Diagnostics.Process]::Start($start)
    $stdout = $process.StandardOutput.ReadToEndAsync()
    $stderr = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit(600000)) {
        $process.Kill($true)
        $process.WaitForExit()
        throw "$Name timed out"
    }
    $stdoutText = $stdout.Result
    $stderrText = $stderr.Result
    [IO.File]::WriteAllText((Join-Path $artifact "$Name.stdout.log"), $stdoutText)
    [IO.File]::WriteAllText((Join-Path $artifact "$Name.stderr.log"), $stderrText)
    if ($process.ExitCode -ne 0) {
        throw "$Name exited with code $($process.ExitCode)"
    }
    if ($stderrText -notmatch 'headless audio backend=dummy' -or
        $stderrText -notmatch 'SDL audio ready: driver=dummy') {
        throw "$Name did not prove the dummy audio backend"
    }
    return $stderrText
}

try {
    $rewriteLog = Invoke-SafeSaveRun `
        -Name 'rewrite' `
        -Fixture $rewriteFixture `
        -ExitPoll 3250 `
        -CreateSave $true
    if ($rewriteLog -notmatch
        'test-save patch active: all 60 licenses bronze, credits=100000') {
        throw 'The opt-in convenience-save patch did not engage'
    }

    $validateLog = Invoke-SafeSaveRun `
        -Name 'clean-reload' `
        -Fixture $validateFixture `
        -ExitPoll 1950 `
        -CreateSave $false
    if ($validateLog -notmatch
        '\[GT2-Save\] credits=100000 licenseTestsCompleted=60/60 bronze=60/60') {
        throw 'Clean reload did not recover 100,000 credits and all 60 licenses'
    }
    if ($validateLog -match 'test-save patch active') {
        throw 'Clean reload unexpectedly used the save-creation patch'
    }

    foreach ($poll in @(1100, 1600, 1900)) {
        $capture = Join-Path $deploy "recompone_capture__$poll.ppm"
        if (-not (Test-Path -LiteralPath $capture)) {
            throw "Clean reload did not produce UI capture poll $poll"
        }
        $ppm = Join-Path $artifact "clean-reload-$poll.ppm"
        Copy-Item -LiteralPath $capture -Destination $ppm -Force
        & ffmpeg -hide_banner -loglevel error -y -i $ppm (
            Join-Path $artifact "clean-reload-$poll.png")
        if ($LASTEXITCODE -ne 0) {
            throw "ffmpeg failed to convert save capture poll $poll"
        }
    }

    Copy-Item -LiteralPath $card -Destination $validated -Force
    $afterHash = (Get-FileHash -LiteralPath $card -Algorithm SHA256).Hash
    $succeeded = $true
    Write-Output "save=$card bytes=$((Get-Item -LiteralPath $card).Length)"
    Write-Output "before_sha256=$beforeHash after_sha256=$afterHash"
    Write-Output 'clean reload=credits 100000, licenses 60/60 bronze'
    Write-Output "evidence=$artifact audio=dummy for both runs"
}
finally {
    if (-not $succeeded -and (Test-Path -LiteralPath $backup)) {
        Copy-Item -LiteralPath $backup -Destination $card -Force
        Write-Warning 'Validation failed; restored the pre-rewrite carda.sav'
    }
}
