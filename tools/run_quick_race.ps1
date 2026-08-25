param(
    [string]$InstallPath = 'OpenGTPS1'
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path

function Resolve-RepoPath([string]$Path) {
    if ([IO.Path]::IsPathRooted($Path)) {
        return (Resolve-Path -LiteralPath $Path).Path
    }
    return (Resolve-Path -LiteralPath (Join-Path $repo $Path)).Path
}

$install = Resolve-RepoPath $InstallPath
$exe = Join-Path $install 'GranTurismo2PC.exe'
$sessionRoot = Join-Path $repo 'work\quick-race'
New-Item -ItemType Directory -Path $sessionRoot -Force | Out-Null

foreach ($required in @(
    $exe,
    (Join-Path $install 'GT2.VOL'),
    (Join-Path $install 'MUSIC.DAT'),
    (Join-Path $install 'manifests\arcade.json')
)) {
    if (-not (Test-Path -LiteralPath $required -PathType Leaf)) {
        throw "Quick-race file is missing: $required"
    }
}

$environment = [ordered]@{
    RECOMPONE_INPUT_FILE = $null
    RECOMPONE_INPUT_SCRIPT = $null
    RECOMPONE_INPUT_END_POLL = $null
    RECOMPONE_LIVE_INPUT_START_POLL = $null
    RECOMPONE_DISABLE_LIVE_INPUT = $null
    RECOMPONE_GT2_AI_AUTODRIVE = $null
    RECOMPONE_GT2_AI_AUTODRIVE_MAX_ENGAGEMENTS = $null
    RECOMPONE_GT2_SOAK_QUICK_WIN_AFTER_AI_TICKS = $null
    RECOMPONE_GT2_ARCADE_UNLOCK_ALL_COURSES = $null
    RECOMPONE_CARD_A_PATH = Join-Path $sessionRoot 'carda.sav'
    RECOMPONE_CARD_B_PATH = Join-Path $sessionRoot 'cardb.sav'
    RECOMPONE_EXIT_AFTER_INPUT_POLL = $null
    RECOMPONE_UNTHROTTLED = $null
    RECOMPONE_THROTTLE_AFTER_INPUT_POLL = $null
    RECOMPONE_WINDOW_VISIBLE = '1'
    RECOMPONE_MUTE = $null
    RECOMPONE_SUPPRESS_RUMBLE = $null
    RECOMPONE_TRACE_INPUT = $null
    SDL_AUDIODRIVER = $null
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

    Write-Host 'Launching stock Seattle Circuit quick race.' -ForegroundColor Cyan
    Write-Host 'No menu automation: the native race path loads directly with live input.'
    Write-Host 'Xbox: A accelerate/confirm, X brake, stick/D-pad steer.'
    Write-Host 'Keyboard: Z accelerate/confirm, A brake, arrows steer.'

    $process = Start-Process `
        -FilePath $exe `
        -WorkingDirectory $install `
        -ArgumentList @($install, '--arcade-race', 'seattle-circuit') `
        -PassThru
    Write-Host "Gran Turismo 2 PC launched (PID $($process.Id))."
} finally {
    foreach ($entry in $previous.GetEnumerator()) {
        [Environment]::SetEnvironmentVariable(
            $entry.Key,
            $entry.Value,
            [EnvironmentVariableTarget]::Process)
    }
}
