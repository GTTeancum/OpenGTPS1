param(
    [Parameter(Mandatory=$true)][ValidatePattern('^[a-z0-9-]+$')][string]$Course,
    [ValidateRange(0,900)][int]$StartSeconds=3,
    [ValidateRange(1,900)][int]$EndSeconds=180,
    [ValidateRange(1,60)][int]$IntervalSeconds=3,
    [ValidatePattern('^[a-zA-Z0-9-]+$')][string]$Tag='review',
    [ValidatePattern('^$|^[A-Fa-f0-9]{64}$')][string]$ExpectedExeSha256='',
    [string]$ExecutablePath=''
)
# Isolated audit of the installed build or an explicitly selected candidate.
# Native capture and process-local AI only; installed game data stays read-only.
$ErrorActionPreference='Stop'
if ($StartSeconds -gt $EndSeconds) { throw 'Capture interval is reversed' }
$repo=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$installed=Join-Path $repo 'OpenGTPS1'
$selectedExe=if ($ExecutablePath) { (Resolve-Path -LiteralPath $ExecutablePath).Path } else { Join-Path $installed 'GranTurismo2PC.exe' }
if ($ExpectedExeSha256 -and
    (Get-FileHash -LiteralPath $selectedExe).Hash -ne $ExpectedExeSha256) {
    throw 'Selected executable does not match the requested review build'
}
# Full-scene equivalence audits can exceed 12 GiB of committed memory. Resolve
# the executable through Win32_Process: a name-only Get-Process check missed a
# still-running native capture in this environment. Never stop another game.
$existingGames=@(Get-CimInstance Win32_Process -Filter "Name='GranTurismo2PC.exe'")
if ($existingGames.Count) {
    $existingDescriptions=$existingGames | ForEach-Object { "$($_.ProcessId): $($_.ExecutablePath)" }
    throw "Native course audits must run serially; existing game process: $($existingDescriptions -join '; ')"
}
$stamp=Get-Date -Format 'yyyyMMdd-HHmmss'
$run=Join-Path $repo "artifacts/course-review/$Course/$Tag-$stamp"
$runtime=Join-Path $run 'runtime'
New-Item -ItemType Directory -Path $runtime -Force | Out-Null
Copy-Item -LiteralPath $selectedExe -Destination (Join-Path $runtime 'GranTurismo2PC.exe')
Copy-Item -LiteralPath (Join-Path $installed 'interface.ini') -Destination $runtime
foreach ($name in @('carda.sav','cardb.sav')) {
    $source=Join-Path $installed $name
    if (Test-Path -LiteralPath $source) { Copy-Item -LiteralPath $source -Destination $run }
}
$start=[Diagnostics.ProcessStartInfo]::new()
$start.FileName=Join-Path $runtime 'GranTurismo2PC.exe'
$start.WorkingDirectory=$runtime
$start.Arguments="--headless --mute --arcade-race $Course `"$installed`""
$start.UseShellExecute=$false
$start.CreateNoWindow=$true
$start.RedirectStandardOutput=$true
$start.RedirectStandardError=$true
$controls=@{
    SDL_AUDIODRIVER='dummy'; RECOMPONE_DISABLE_LIVE_INPUT='1'; RECOMPONE_SUPPRESS_RUMBLE='1'
    RECOMPONE_GT2_AI_AUTODRIVE='1'; RECOMPONE_GT2_AI_AUTODRIVE_MAX_ENGAGEMENTS='2'
    RECOMPONE_UNTHROTTLED='1'; RECOMPONE_PROCESS_PRIORITY='BelowNormal'
    RECOMPONE_GRAPHICS_PRESET_OVERRIDE='Enhanced'; RECOMPONE_GT2_TRUE_60HZ='1'
    RECOMPONE_OUTPUT_RESOLUTION='1920x1080'; RECOMPONE_PRESENTATION_RESOLUTION='1280x720'
    RECOMPONE_CAPTURE_AUTOMATIC_STAGE='0'; RECOMPONE_PRESENTATION_CAPTURE='1'
    RECOMPONE_CAPTURE_INPUT_STAGE='race_1'
    RECOMPONE_CAPTURE_INPUT_STAGE_POLL=[string]($StartSeconds*60)
    RECOMPONE_CAPTURE_INPUT_STAGE_INTERVAL_POLLS=[string]($IntervalSeconds*60)
    RECOMPONE_CAPTURE_INPUT_STAGE_END_POLL=[string]($EndSeconds*60)
    RECOMPONE_TEST_EXIT_INPUT_STAGE='race_1'
    RECOMPONE_TEST_EXIT_INPUT_STAGE_POLL=[string]($EndSeconds*60+10)
    RECOMPONE_EXIT_AFTER_INPUT_POLL=[string]($EndSeconds*60+10000)
    RECOMPONE_CARD_A_PATH=(Join-Path $run 'carda.sav')
    RECOMPONE_CARD_B_PATH=(Join-Path $run 'cardb.sav')
}
foreach ($entry in $controls.GetEnumerator()) { $start.Environment[$entry.Key]=$entry.Value }
$process=[Diagnostics.Process]::Start($start)
$stdout=$process.StandardOutput.ReadToEndAsync()
$stderr=$process.StandardError.ReadToEndAsync()
$timer=[Diagnostics.Stopwatch]::StartNew()
$peakPrivateBytes=0L
$peakWorkingSetBytes=0L
$watchdogTerminated=$false
while (-not $process.WaitForExit(1000)) {
    $process.Refresh()
    $peakPrivateBytes=[Math]::Max($peakPrivateBytes,$process.PrivateMemorySize64)
    $peakWorkingSetBytes=[Math]::Max($peakWorkingSetBytes,$process.WorkingSet64)
    if ($timer.Elapsed.TotalSeconds -gt 900) { $watchdogTerminated=$true; $process.Kill(); $process.WaitForExit(); break }
}
[IO.File]::WriteAllText((Join-Path $run 'stdout.log'),$stdout.Result)
[IO.File]::WriteAllText((Join-Path $run 'stderr.log'),$stderr.Result)
$failureReasons=@()
if ($process.ExitCode -ne 0 -or $stderr.Result -notmatch '\[Runtime\] shutdown complete; exit=0') {
    $failureReasons+='Audit did not exit cleanly'
}
if ($stderr.Result -match '\[Native-World\] disabled:') { $failureReasons+='Native renderer disabled during audit' }
$captures=@(Get-ChildItem -LiteralPath $runtime -Filter 'recompone_present_race_1_*.ppm' -File | Sort-Object Name)
$expected=[Math]::Floor(($EndSeconds-$StartSeconds)/$IntervalSeconds)+1
if ($captures.Count -ne $expected) { $failureReasons+="Expected $expected captures, found $($captures.Count)" }
$records=@()
foreach ($capture in $captures) {
    if ($capture.Name -notmatch '^recompone_present_race_1_(\d{6})_') { throw 'Unexpected capture name' }
    $stageLabel=$Matches[1]
    $mapping=[regex]::Match($stderr.Result,
        "native presentation capture=race_1_$stageLabel sourceFrame=(\d+) poll=(\d+) synthetic=False repeated=False")
    if (-not $mapping.Success) { $failureReasons+="Missing unique authored native frame mapping for $stageLabel" }
    $png=Join-Path $run ($capture.BaseName+'.png')
    $jpg=Join-Path $run ($capture.BaseName+'.jpg')
    & ffmpeg -hide_banner -loglevel error -i $capture.FullName -frames:v 1 $png
    if ($LASTEXITCODE -ne 0) { throw 'Lossless capture conversion failed' }
    & ffmpeg -hide_banner -loglevel error -i $png -vf 'scale=960:-2' -frames:v 1 -q:v 3 $jpg
    if ($LASTEXITCODE -ne 0) { throw 'Review preview conversion failed' }
    $records+=@{file=[IO.Path]::GetFileName($png); sha256=(Get-FileHash -LiteralPath $png).Hash; reviewed=$false;
        stagePoll=[int]$stageLabel;nativeMappingVerified=$mapping.Success;
        sourceFrame=$(if($mapping.Success){[int]$mapping.Groups[1].Value}else{$null});
        sourcePoll=$(if($mapping.Success){[int]$mapping.Groups[2].Value}else{$null})}
    # Exact file produced in this isolated run; lossless PNG is retained.
    Remove-Item -LiteralPath $capture.FullName
}
$record=@{course=$Course;status=$(if($failureReasons.Count){'capture-failed-evidence-retained'}else{'capture-complete-review-pending'});startSeconds=$StartSeconds;
    endSeconds=$EndSeconds;intervalSeconds=$IntervalSeconds;frameCount=$captures.Count;
    installedExeSha256=(Get-FileHash -LiteralPath (Join-Path $installed 'GranTurismo2PC.exe')).Hash;
    captureExeSha256=(Get-FileHash -LiteralPath $start.FileName).Hash;executableSource=$selectedExe;frames=$records;
    processLocalInputOnly=$true;nativeCapture=($failureReasons.Count -eq 0);
    failureReasons=$failureReasons;exitCode=$process.ExitCode;watchdogTerminated=$watchdogTerminated;
    peakPrivateBytes=$peakPrivateBytes;peakWorkingSetBytes=$peakWorkingSetBytes;elapsedSeconds=$timer.Elapsed.TotalSeconds}
[IO.File]::WriteAllText((Join-Path $run 'manifest.json'),($record | ConvertTo-Json -Depth 5))
# Remove only the disposable executable after verifying its retained source copy.
if ((Get-FileHash -LiteralPath $start.FileName).Hash -eq
    (Get-FileHash -LiteralPath $selectedExe).Hash) {
    Remove-Item -LiteralPath $start.FileName
}
Write-Output "Review capture: $run; frames=$($captures.Count); PNG evidence retained, raw PPMs and duplicate EXE removed."
if ($failureReasons.Count) { throw "Audit failed; diagnostic evidence retained and cleanup completed: $run ($($failureReasons -join '; '))" }
