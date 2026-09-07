param([string]$Tag='main-intro',[int]$DumpStart=-1,[int]$DumpCount=16,
      [int]$VideoStart=350,[int]$VideoFrames=1080,[switch]$Diagnostics,
      [switch]$OverlayTint,[string]$PixelPoints='',
      [int]$TraceStart=595,[int]$TraceEnd=610,
      [string]$TraceModel='0x800b9a08')
# Read-only rendering audit: isolated main executable, native capture and
# process-local AI controls; never OS input or desktop/window capture.
$ErrorActionPreference='Stop'
$repo=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$out=Join-Path $repo ('artifacts/start-grid-audit/'+$Tag)
$runtime=Join-Path $out 'runtime'
New-Item -ItemType Directory -Force -Path $out,$runtime | Out-Null
Copy-Item -LiteralPath (Join-Path $repo 'OpenGTPS1/GranTurismo2PC.exe'),(Join-Path $repo 'OpenGTPS1/interface.ini') -Destination $runtime
$start=[Diagnostics.ProcessStartInfo]::new()
$start.FileName=Join-Path $runtime 'GranTurismo2PC.exe';$start.WorkingDirectory=$runtime
$start.Arguments='--headless --mute --arcade-race red-rock-valley-speedway "'+(Join-Path $repo 'OpenGTPS1')+'"'
$start.UseShellExecute=$false;$start.CreateNoWindow=$true
$start.RedirectStandardOutput=$true;$start.RedirectStandardError=$true
$controls=@{
 SDL_AUDIODRIVER='dummy';RECOMPONE_DISABLE_LIVE_INPUT='1';RECOMPONE_SUPPRESS_RUMBLE='1'
 RECOMPONE_UNTHROTTLED='1';RECOMPONE_THROTTLE_ON_SCRIPT_STAGE='race_1'
 RECOMPONE_GT2_AI_AUTODRIVE='1';RECOMPONE_GT2_TRUE_60HZ='1'
 RECOMPONE_CAPTURE_AUTOMATIC_STAGE='0';RECOMPONE_OUTPUT_RESOLUTION='1920x1080'
 RECOMPONE_PRESENTATION_RESOLUTION='1280x720';RECOMPONE_PROCESS_PRIORITY='BelowNormal'
 RECOMPONE_GRAPHICS_PRESET_OVERRIDE='Enhanced';RECOMPONE_TRACE_PERFORMANCE='1'
 RECOMPONE_CARD_A_PATH=(Join-Path $out 'carda.sav');RECOMPONE_CARD_B_PATH=(Join-Path $out 'cardb.sav')
}
if($DumpStart -lt 0){
 $controls.RECOMPONE_VIDEO_CAPTURE=Join-Path $out 'main-intro.mp4'
 $controls.RECOMPONE_VIDEO_START_INPUT_POLL=[string]$VideoStart
 $controls.RECOMPONE_VIDEO_CAPTURE_FRAMES=[string]$VideoFrames
 $controls.RECOMPONE_VIDEO_WIDTH='1280';$controls.RECOMPONE_VIDEO_HEIGHT='720'
 $controls.RECOMPONE_VIDEO_FPS='60';$controls.RECOMPONE_VIDEO_CRF='12'
 $controls.RECOMPONE_EXIT_AFTER_VIDEO_CAPTURE='1';$controls.RECOMPONE_EXIT_AFTER_INPUT_POLL='4200'
}else{
 $controls.RECOMPONE_NATIVE_WORLD_DUMP_PATH=Join-Path $out 'scene.ogtwcap'
 $controls.RECOMPONE_NATIVE_WORLD_DUMP_INPUT_POLL=[string]$DumpStart
 $controls.RECOMPONE_NATIVE_WORLD_DUMP_COUNT=[string]$DumpCount
 $controls.RECOMPONE_NATIVE_WORLD_DUMP_INTERVAL='1'
 $controls.RECOMPONE_AUDIT_GT2_RESIDENT_EQUIVALENCE='1'
 $controls.RECOMPONE_AUDIT_NATIVE_RESIDENT_EQUIVALENCE='1'
 $controls.RECOMPONE_EXIT_AFTER_INPUT_POLL=[string]($DumpStart+350)
}
if($Diagnostics){
 $controls.OPENGT_TRACE_RESIDENT_OVERLAYS='1'
 $controls.OPENGT_RENDER_WHEEL_DIAGNOSTICS='1'
 $controls.OPENGT_RENDER_WHEEL_DIAGNOSTICS_INTERVAL='1'
 $controls.OPENGT_RENDER_WHEEL_DIAGNOSTICS_RACE_ONLY='1'
 $controls.OPENGT_RENDER_TEXTURE_INSTANCE_DIAGNOSTICS='1'
 $controls.OPENGT_RENDER_TEXTURE_INSTANCE_DIAGNOSTICS_START_POLL=[string]$TraceStart
 $controls.OPENGT_RENDER_TEXTURE_INSTANCE_DIAGNOSTICS_END_POLL=[string]$TraceEnd
 $controls.OPENGT_RENDER_TEXTURE_INSTANCE_DIAGNOSTICS_INTERVAL='1'
 $controls.OPENGT_RENDER_TEXTURE_INSTANCE_DIAGNOSTICS_MODEL=$TraceModel
 $controls.OPENGT_TRACE_RESIDENT_PRIMITIVE_ADDRESS_MIN='0x800b9ea0'
 $controls.OPENGT_TRACE_RESIDENT_PRIMITIVE_ADDRESS_MAX='0x800ba05c'
}
if($PixelPoints){
 $controls.OPENGT_RENDER_PIXEL_PROVENANCE_POINTS=$PixelPoints
 $controls.OPENGT_RENDER_PIXEL_PROVENANCE_START_POLL=[string]$TraceStart
 $controls.OPENGT_RENDER_PIXEL_PROVENANCE_END_POLL=[string]$TraceEnd
}
if($OverlayTint){$controls.OPENGT_DEBUG_ROAD_OVERLAY_TINT='1'}
foreach($entry in $controls.GetEnumerator()){$start.Environment[$entry.Key]=$entry.Value}
$process=[Diagnostics.Process]::Start($start)
$stdout=$process.StandardOutput.ReadToEndAsync();$stderr=$process.StandardError.ReadToEndAsync()
$timer=[Diagnostics.Stopwatch]::StartNew()
while(-not $process.WaitForExit(1000)){
 if($timer.Elapsed.TotalSeconds -gt 180){$process.Kill();$process.WaitForExit();break}
}
[IO.File]::WriteAllText((Join-Path $out 'stdout.log'),$stdout.Result)
[IO.File]::WriteAllText((Join-Path $out 'stderr.log'),$stderr.Result)
if($process.ExitCode -ne 0 -or $stderr.Result -notmatch '\[Runtime\] shutdown complete; exit=0'){throw "Audit capture failed: $out"}
if($stderr.Result -match '\[Vehicle-Paint\]'){throw 'Wrong executable: paint code active'}
if($DumpStart -lt 0 -and $stderr.Result -notmatch ('video capture complete: frames='+$VideoFrames+'/'+$VideoFrames+' ffmpeg exit=0')){throw 'Incomplete native video'}
Write-Output "Audit evidence: $out"
