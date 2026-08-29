@echo off
setlocal
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0tools\run_quick_race.ps1"
if errorlevel 1 pause
