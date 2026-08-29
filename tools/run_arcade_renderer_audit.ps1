param(
    [string]$DeployPath = 'OpenGTPS1',
    [string]$DataPath = 'work\gt2-unified',
    [string]$CardPath = 'work\arcade-audit-save\carda.sav',
    [switch]$PreflightOnly
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$logRoot = Join-Path $repo 'work\arcade-renderer-audit'
New-Item -ItemType Directory -Path $logRoot -Force | Out-Null
$launcherLog = Join-Path $logRoot 'launcher-latest.log'
$lineBreak = [Environment]::NewLine
[IO.File]::WriteAllText(
    $launcherLog,
    "started=$([DateTime]::Now.ToString('o')) " +
    "powershell=$($PSVersionTable.PSVersion) script=$PSCommandPath" +
    $lineBreak)
trap {
    $failure = ($_ | Out-String).Trim()
    [IO.File]::AppendAllText(
        $launcherLog,
        "failed=$([DateTime]::Now.ToString('o')) error=$failure" +
        $lineBreak)
    [Console]::Error.WriteLine($failure)
    exit 1
}

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
$card = Resolve-RepoPath $CardPath -AllowMissing
$exe = Join-Path $deploy 'GranTurismo2PC.exe'
$creator = Resolve-RepoPath 'tools\create_arcade_audit_card.ps1'

if (-not (Test-Path -LiteralPath $card -PathType Leaf)) {
    & $creator -OutputCard $card -DeployPath $deploy -DataPath $data
}

$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$stdoutPath = Join-Path $logRoot "arcade-audit-$stamp.stdout.log"
$stderrPath = Join-Path $logRoot "arcade-audit-$stamp.stderr.log"
$cardB = Join-Path $logRoot 'cardb.sav'

foreach ($required in @(
    $exe,
    $card,
    (Join-Path $data 'GT2.VOL'),
    (Join-Path $data 'MUSIC.DAT'),
    (Join-Path $data 'manifests\arcade.json')
)) {
    if (-not (Test-Path -LiteralPath $required -PathType Leaf)) {
        throw "Arcade renderer-audit file is missing: $required"
    }
}

if ($PreflightOnly) {
    [IO.File]::AppendAllText(
        $launcherLog,
        "preflight=pass deploy=$deploy data=$data card=$card" +
        $lineBreak)
    Write-Output (
        "arcade_renderer_audit_preflight=pass " +
        "course=trial-mountain class=C deploy=$deploy data=$data card=$card")
    exit 0
}

# Use the dedicated unlocked card and the native no-menu Trial Mountain
# constructor. The constructor uses the verified Class C Xsara selection;
# driving remains fully live and no timing fixture or runtime course unlock is
# active.
$environment = [ordered]@{
    RECOMPONE_CARD_A_PATH = $card
    RECOMPONE_CARD_B_PATH = $cardB
    RECOMPONE_INPUT_FILE = $null
    RECOMPONE_INPUT_SCRIPT = $null
    RECOMPONE_INPUT_END_POLL = $null
    RECOMPONE_LIVE_INPUT_START_POLL = $null
    RECOMPONE_DISABLE_LIVE_INPUT = $null
    RECOMPONE_EXIT_AFTER_INPUT_POLL = $null
    RECOMPONE_UNTHROTTLED = $null
    RECOMPONE_WINDOW_VISIBLE = '1'
    RECOMPONE_DISABLE_DISPLAY_CAPTURE = '1'
    RECOMPONE_CAPTURE_AUTOMATIC_STAGE = '0'
    RECOMPONE_PRESENTATION_CAPTURE = $null
    RECOMPONE_SUPPRESS_RUMBLE = '1'
    RECOMPONE_GRAPHICS_PRESET_OVERRIDE = 'Enhanced'
    RECOMPONE_GT2_ARCADE_UNLOCK_ALL_COURSES = $null
    RECOMPONE_GT2_CREATE_TEST_SAVE = $null
    RECOMPONE_GT2_DIRECT_ARCADE_RACE = $null
    RECOMPONE_GT2_AI_AUTODRIVE = $null
    RECOMPONE_GT2_AI_AUTODRIVE_MAX_ENGAGEMENTS = $null
    RECOMPONE_GT2_SOAK_QUICK_WIN_AFTER_AI_TICKS = $null
    RECOMPONE_TRACE_GT2_VEHICLE_VISIBILITY = $null
    RECOMPONE_TRACE_GT2_VEHICLE_GTE_FLAGS = $null
    OPENGT_RENDER_VEHICLE_BOUNDARY_AUDIT = $null
    RECOMPONE_TRACE_PERFORMANCE = '1'
    RECOMPONE_NATIVE_WORLD_TRACE_INTERVAL = '300'
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
        -ArgumentList @('--arcade-race', 'trial-mountain', $data) `
        -RedirectStandardOutput $stdoutPath `
        -RedirectStandardError $stderrPath `
        -PassThru

    Start-Sleep -Milliseconds 1000
    $process.Refresh()
    if ($process.HasExited) {
        throw (
            "GranTurismo2PC exited during startup with code " +
            "$($process.ExitCode). stderr=$stderrPath stdout=$stdoutPath")
    }

    [IO.File]::AppendAllText(
        $launcherLog,
        "launched=$([DateTime]::Now.ToString('o')) pid=$($process.Id) " +
        "stdout=$stdoutPath stderr=$stderrPath" +
        $lineBreak)

    Write-Output (
        "arcade_renderer_audit=launched pid=$($process.Id) " +
        "course=trial-mountain class=C player=xsara card=$card " +
        "all_courses=true live_input=true runtime_unlock=false " +
        "captures=disabled stderr=$stderrPath stdout=$stdoutPath")
} finally {
    foreach ($entry in $previous.GetEnumerator()) {
        [Environment]::SetEnvironmentVariable(
            $entry.Key,
            $entry.Value,
            [EnvironmentVariableTarget]::Process)
    }
}
