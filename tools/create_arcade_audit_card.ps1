param(
    [string]$OutputCard = 'work\arcade-audit-save\carda.sav',
    [string]$DeployPath = 'OpenGTPS1',
    [string]$DataPath = 'work\gt2-unified',
    [switch]$Regenerate
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path

function Resolve-RepoPath([string]$Path, [switch]$AllowMissing) {
    $candidate = if ([IO.Path]::IsPathRooted($Path)) {
        $Path
    } else {
        Join-Path $repo $Path
    }
    if ($AllowMissing) {
        return [IO.Path]::GetFullPath($candidate)
    }
    return (Resolve-Path -LiteralPath $candidate).Path
}

$deploy = Resolve-RepoPath $DeployPath
$data = Resolve-RepoPath $DataPath
$output = Resolve-RepoPath $OutputCard -AllowMissing
$outputDirectory = Split-Path -Parent $output
$exe = Join-Path $deploy 'GranTurismo2PC.exe'
$fixture = Resolve-RepoPath 'tests\fixtures\create-arcade-audit-save.input'
$patcher = Resolve-RepoPath 'tools\patch_gt2_arcade_audit_card.py'

foreach ($required in @(
    $exe,
    $fixture,
    $patcher,
    (Join-Path $data 'GT2.VOL'),
    (Join-Path $data 'MUSIC.DAT'),
    (Join-Path $data 'manifests\arcade.json')
)) {
    if (-not (Test-Path -LiteralPath $required -PathType Leaf)) {
        throw "Arcade audit-card file is missing: $required"
    }
}
if (-not (Get-Command python -ErrorAction SilentlyContinue)) {
    throw 'Python is required to patch and checksum the GT2 audit card'
}

New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
if ($Regenerate -and (Test-Path -LiteralPath $output -PathType Leaf)) {
    Remove-Item -LiteralPath $output -Force
}
$buildRoot = Join-Path $outputDirectory (
    'native-base-' + [Guid]::NewGuid().ToString('N'))
$nativeStdout = Join-Path $outputDirectory 'create-native-base.stdout.log'
$nativeStderr = Join-Path $outputDirectory 'create-native-base.stderr.log'
$validationStdout = Join-Path $outputDirectory 'clean-validation.stdout.log'
$validationStderr = Join-Path $outputDirectory 'clean-validation.stderr.log'

function Invoke-IsolatedArcade {
    param(
        [string]$CardA,
        [string]$CardB,
        [string]$Stdout,
        [string]$Stderr,
        [string]$InputFixture,
        [int]$ExitPoll,
        [bool]$TraceSave,
        [bool]$CreateTestSave
    )

    $environment = [ordered]@{
        RECOMPONE_CARD_A_PATH = $CardA
        RECOMPONE_CARD_B_PATH = $CardB
        RECOMPONE_INPUT_FILE = $InputFixture
        RECOMPONE_INPUT_SCRIPT = $null
        RECOMPONE_DISABLE_LIVE_INPUT = '1'
        RECOMPONE_SUPPRESS_RUMBLE = '1'
        RECOMPONE_UNTHROTTLED = '1'
        RECOMPONE_PROCESS_PRIORITY = 'BelowNormal'
        RECOMPONE_EXIT_AFTER_INPUT_POLL = $ExitPoll.ToString()
        RECOMPONE_TRACE_GT2_SAVE = $(if ($TraceSave) { '1' } else { $null })
        RECOMPONE_GT2_CREATE_TEST_SAVE = $(
            if ($CreateTestSave) { '1' } else { $null })
        RECOMPONE_GT2_ARCADE_UNLOCK_ALL_COURSES = $null
        RECOMPONE_DISABLE_DISPLAY_CAPTURE = '1'
        RECOMPONE_CAPTURE_AUTOMATIC_STAGE = '0'
        RECOMPONE_PRESENTATION_CAPTURE = $null
    }
    $previous = @{}
    try {
        foreach ($entry in $environment.GetEnumerator()) {
            $previous[$entry.Key] = [Environment]::GetEnvironmentVariable(
                $entry.Key,
                [EnvironmentVariableTarget]::Process)
            [Environment]::SetEnvironmentVariable(
                $entry.Key,
                $entry.Value,
                [EnvironmentVariableTarget]::Process)
        }
        $process = Start-Process `
            -FilePath $exe `
            -WorkingDirectory $deploy `
            -ArgumentList @('--headless', '--start-arcade', $data) `
            -RedirectStandardOutput $Stdout `
            -RedirectStandardError $Stderr `
            -Wait `
            -PassThru `
            -WindowStyle Hidden
        if ($process.ExitCode -ne 0) {
            throw "Arcade audit-card process exited with code $($process.ExitCode)"
        }
    } finally {
        foreach ($entry in $previous.GetEnumerator()) {
            [Environment]::SetEnvironmentVariable(
                $entry.Key,
                $entry.Value,
                [EnvironmentVariableTarget]::Process)
        }
    }
}

try {
    if (-not (Test-Path -LiteralPath $output -PathType Leaf)) {
        New-Item -ItemType Directory -Path $buildRoot -Force | Out-Null
        $baseCard = Join-Path $buildRoot 'carda.sav'
        $baseCardB = Join-Path $buildRoot 'cardb.sav'
        Invoke-IsolatedArcade `
            -CardA $baseCard `
            -CardB $baseCardB `
            -Stdout $nativeStdout `
            -Stderr $nativeStderr `
            -InputFixture $fixture `
            -ExitPoll 3100 `
            -TraceSave $false `
            -CreateTestSave $true
        if (-not (Test-Path -LiteralPath $baseCard -PathType Leaf)) {
            throw 'GT2 did not create the native base memory card'
        }
        $patchOutput = & python $patcher $baseCard --output $output
    } else {
        $patchOutput = & python $patcher $output
    }
    if ($LASTEXITCODE -ne 0) {
        throw "Arcade audit-card patcher exited with code $LASTEXITCODE"
    }

    $beforeValidation = (Get-FileHash -LiteralPath $output -Algorithm SHA256).Hash
    $validationCardB = Join-Path $buildRoot 'validation-cardb.sav'
    New-Item -ItemType Directory -Path $buildRoot -Force | Out-Null
    Invoke-IsolatedArcade `
        -CardA $output `
        -CardB $validationCardB `
        -Stdout $validationStdout `
        -Stderr $validationStderr `
        -InputFixture $null `
        -ExitPoll 1300 `
        -TraceSave $true `
        -CreateTestSave $false
    $validationLog = Get-Content -LiteralPath $validationStderr -Raw
    if ($validationLog -notmatch
        '\[GT2-Save\].*licenseTestsCompleted=60/60.*arcadeDifficult=21/21') {
        throw 'Clean Arcade launch did not load all licenses and all 21 Difficult results'
    }
    if ($validationLog -match
        'test-save patch active|diagnostic Arcade course unlock active') {
        throw 'Clean validation unexpectedly used a runtime unlock patch'
    }
    $afterValidation = (Get-FileHash -LiteralPath $output -Algorithm SHA256).Hash
    if ($afterValidation -ne $beforeValidation) {
        throw 'Clean validation changed the dedicated audit card'
    }

    Write-Output $patchOutput
    Write-Output (
        "clean_arcade_validation=licenses 60/60 arcade_difficult 21/21 " +
        "runtime_unlock=disabled card_unchanged=true sha256=$afterValidation")
} finally {
    if (Test-Path -LiteralPath $buildRoot -PathType Container) {
        Remove-Item -LiteralPath $buildRoot -Recurse -Force
    }
}
