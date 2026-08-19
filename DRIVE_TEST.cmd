@echo off
setlocal
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0tools\run_drive_test.ps1"
if errorlevel 1 pause
