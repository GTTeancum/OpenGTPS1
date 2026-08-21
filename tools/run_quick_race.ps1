param(
    [string]$InstallPath = 'OpenGTPS1',
    [int]$LiveInputStartPoll = 5700
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
$fixture = Join-Path $repo 'tests\fixtures\arcade-direct-seattle-race.input'
$sessionRoot = Join-Path $repo 'work\quick-race'
New-Item -ItemType Directory -Path $sessionRoot -Force | Out-Null

foreach ($required in @(
    $exe,
    $fixture,
    (Join-Path $install 'GT2.VOL'),
    (Join-Path $install 'MUSIC.DAT'),
    (Join-Path $install 'manifests\arcade.json')
)) {
    if (-not (Test-Path -LiteralPath $required -PathType Leaf)) {
        throw "Quick-race file is missing: $required"
    }
}

if ($LiveInputStartPoll -lt 5664) {
    throw 'LiveInputStartPoll must follow the final scripted menu input (poll 5663)'
}

$environment = [ordered]@{
    RECOMPONE_INPUT_FILE = $fixture
    RECOMPONE_INPUT_SCRIPT = $null
    RECOMPONE_INPUT_END_POLL = $null
    RECOMPONE_LIVE_INPUT_START_POLL = $LiveInputStartPoll.ToString()
    RECOMPONE_DISABLE_LIVE_INPUT = $null
    RECOMPONE_GT2_AI_AUTODRIVE = $null
    RECOMPONE_GT2_AI_AUTODRIVE_MAX_ENGAGEMENTS = $null
    RECOMPONE_GT2_ARCADE_UNLOCK_ALL_COURSES = '1'
    RECOMPONE_CARD_A_PATH = Join-Path $sessionRoot 'carda.sav'
    RECOMPONE_CARD_B_PATH = Join-Path $sessionRoot 'cardb.sav'
    RECOMPONE_EXIT_AFTER_INPUT_POLL = $null
    # Fast-forward only the deterministic frontend. Normal 60 Hz pacing
    # engages before live keyboard/controller input is enabled.
    RECOMPONE_UNTHROTTLED = '1'
    RECOMPONE_THROTTLE_AFTER_INPUT_POLL = $LiveInputStartPoll.ToString()
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
    Write-Host 'Menus are automated; keyboard/controller control is handed to you before the race.'
    Write-Host 'Xbox: A accelerate/confirm, X brake, stick/D-pad steer.'
    Write-Host 'Keyboard: Z accelerate/confirm, A brake, arrows steer.'

    $process = Start-Process `
        -FilePath $exe `
        -WorkingDirectory $install `
        -ArgumentList @($install, '--start-arcade') `
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
