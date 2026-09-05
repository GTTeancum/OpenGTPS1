param(
    [string]$DeployPath='artifacts/paint-specular/test-build',
    [string]$Tag='live-redrock',
    [switch]$Original
)
# Input and capture remain entirely inside this specifically launched process.
$ErrorActionPreference='Stop'
$repo=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$deploy=(Resolve-Path (Join-Path $repo $DeployPath)).Path
$evidence=Join-Path $repo ('artifacts/paint-specular/'+$Tag)
New-Item -ItemType Directory -Force -Path $evidence | Out-Null
$start=[Diagnostics.ProcessStartInfo]::new()
$start.FileName=Join-Path $deploy 'GranTurismo2PC.exe'
$start.WorkingDirectory=$deploy
$start.Arguments='--headless --mute --arcade-race red-rock-valley-speedway "'+(Join-Path $repo 'OpenGTPS1')+'"'
$start.UseShellExecute=$false; $start.CreateNoWindow=$true
$start.RedirectStandardOutput=$true; $start.RedirectStandardError=$true
$controls=@{
 RECOMPONE_DISABLE_LIVE_INPUT='1'; RECOMPONE_SUPPRESS_RUMBLE='1'; SDL_AUDIODRIVER='dummy'
 RECOMPONE_UNTHROTTLED='1'; RECOMPONE_GT2_AI_AUTODRIVE='1'; RECOMPONE_CAPTURE_AUTOMATIC_STAGE='0'
 RECOMPONE_NATIVE_WORLD_DUMP_PATH=(Join-Path $evidence 'scene.ogtwcap')
 RECOMPONE_NATIVE_WORLD_DUMP_INPUT_POLL='700'; RECOMPONE_NATIVE_WORLD_DUMP_COUNT='1'
 RECOMPONE_PRESENTATION_CAPTURE='1'; RECOMPONE_PRESENTATION_CAPTURE_FRAME='400'
 RECOMPONE_EXIT_AFTER_PRESENTATION_CAPTURE='1'; RECOMPONE_OUTPUT_RESOLUTION='1920x1080'
 RECOMPONE_PROCESS_PRIORITY='BelowNormal'; RECOMPONE_AUDIT_GT2_RESIDENT_EQUIVALENCE='1'
 RECOMPONE_AUDIT_NATIVE_RESIDENT_EQUIVALENCE='1'
 RECOMPONE_CARD_A_PATH=(Join-Path $evidence 'carda.sav'); RECOMPONE_CARD_B_PATH=(Join-Path $evidence 'cardb.sav')
 OPENGT_VEHICLE_PAINT_DISABLE=$(if($Original){'1'}else{'0'})
}
foreach($entry in $controls.GetEnumerator()){$start.Environment[$entry.Key]=$entry.Value}
$process=[Diagnostics.Process]::Start($start)
$stdout=$process.StandardOutput.ReadToEndAsync(); $stderr=$process.StandardError.ReadToEndAsync()
$finished=$process.WaitForExit(60000)
if(-not $finished){$process.Kill();$process.WaitForExit()}
[IO.File]::WriteAllText((Join-Path $evidence 'stdout.log'),$stdout.Result)
[IO.File]::WriteAllText((Join-Path $evidence 'stderr.log'),$stderr.Result)
if(-not $finished){throw 'Native paint test timed out'}
if($process.ExitCode -ne 0){throw ('Native paint test exit '+$process.ExitCode)}
if(-not $Original -and $stderr.Result -notmatch '\[Vehicle-Paint\] matched vehicles=1'){
 throw 'Paint pack did not match the player car in the actual executable'
}
$captures=Get-ChildItem -LiteralPath $deploy -Filter 'recompone_present_frame_000400*.ppm' |
 Where-Object LastWriteTime -ge $process.StartTime
if(-not $captures){throw 'Native presentation capture missing'}
foreach($capture in $captures){
 & ffmpeg -hide_banner -loglevel error -y -i $capture.FullName (Join-Path $evidence ($capture.BaseName+'.png'))
 if($LASTEXITCODE -ne 0){throw 'Native capture PNG conversion failed'}
}
Write-Output "Native Red Rock paint test passed: $Tag"
