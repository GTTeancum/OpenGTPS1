param(
    [switch]$AutoDrive,
    [string]$DeployPath = 'OpenGTPS1',
    [string]$DataPath = 'work\gt2-unified'
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path

function Resolve-RepoPath([string]$Path) {
    if ([IO.Path]::IsPathRooted($Path)) {
        return (Resolve-Path -LiteralPath $Path).Path
    }
    return (Resolve-Path -LiteralPath (Join-Path $repo $Path)).Path
}

$deploy = Resolve-RepoPath $DeployPath
$data = Resolve-RepoPath $DataPath
$exe = Join-Path $deploy 'GranTurismo2PC.exe'
$logRoot = Join-Path $repo 'work\vehicle-boundary-live'
New-Item -ItemType Directory -Path $logRoot -Force | Out-Null
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$stdoutPath = Join-Path $logRoot "ssr5-vehicle-boundary-$stamp.stdout.log"
$stderrPath = Join-Path $logRoot "ssr5-vehicle-boundary-$stamp.stderr.log"

foreach ($required in @(
    $exe,
    (Join-Path $data 'GT2.VOL'),
    (Join-Path $data 'MUSIC.DAT'),
    (Join-Path $data 'manifests\arcade.json')
)) {
    if (-not (Test-Path -LiteralPath $required -PathType Leaf)) {
        throw "SSR5 boundary-trace file is missing: $required"
    }
}

# Keep the launch isolated from previous automated captures. This is a live,
# image-free run: stderr records the exact vehicle submission and clipping
# state while the user drives.
$environment = [ordered]@{
    RECOMPONE_INPUT_FILE = $null
    RECOMPONE_INPUT_SCRIPT = $null
    RECOMPONE_INPUT_END_POLL = $null
    RECOMPONE_LIVE_INPUT_START_POLL = $null
    RECOMPONE_DISABLE_LIVE_INPUT = $null
    RECOMPONE_EXIT_AFTER_INPUT_POLL = $null
    RECOMPONE_UNTHROTTLED = $null
    RECOMPONE_THROTTLE_AFTER_INPUT_POLL = $null
    RECOMPONE_WINDOW_VISIBLE = '1'
    RECOMPONE_DISABLE_DISPLAY_CAPTURE = '1'
    RECOMPONE_CAPTURE_AUTOMATIC_STAGE = '0'
    RECOMPONE_PRESENTATION_CAPTURE = $null
    RECOMPONE_SUPPRESS_RUMBLE = '1'
    RECOMPONE_GRAPHICS_PRESET_OVERRIDE = 'Enhanced'
    RECOMPONE_GT2_AI_AUTODRIVE = $(if ($AutoDrive) { '1' } else { $null })
    RECOMPONE_GT2_AI_AUTODRIVE_MAX_ENGAGEMENTS = $(
        if ($AutoDrive) { '2' } else { $null })
    RECOMPONE_TRACE_GT2_VEHICLE_VISIBILITY = '1'
    RECOMPONE_TRACE_GT2_VEHICLE_VISIBILITY_START_POLL = '0'
    RECOMPONE_TRACE_GT2_VEHICLE_VISIBILITY_END_POLL = '2147483647'
    RECOMPONE_TRACE_GT2_VEHICLE_VISIBILITY_LIMIT = '1000000'
    RECOMPONE_TRACE_GT2_VEHICLE_GTE_FLAGS = '1'
    OPENGT_RENDER_VEHICLE_BOUNDARY_AUDIT = '1'
    OPENGT_RENDER_SCENE_DIAGNOSTICS_ALL_SAMPLES = $null
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
        -ArgumentList @(
            '--arcade-race',
            'special-stage-route-5',
            $data) `
        -RedirectStandardOutput $stdoutPath `
        -RedirectStandardError $stderrPath `
        -PassThru

    Write-Output (
        "ssr5_vehicle_boundary_trace=launched pid=$($process.Id) " +
        "autodrive=$($AutoDrive.IsPresent) " +
        "stderr=$stderrPath stdout=$stdoutPath")
} finally {
    foreach ($entry in $previous.GetEnumerator()) {
        [Environment]::SetEnvironmentVariable(
            $entry.Key,
            $entry.Value,
            [EnvironmentVariableTarget]::Process)
    }
}
