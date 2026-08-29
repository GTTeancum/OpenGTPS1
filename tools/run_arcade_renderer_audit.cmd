@echo off
setlocal
powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0run_arcade_renderer_audit.ps1" %*
set "launcher_exit=%errorlevel%"
if not "%launcher_exit%"=="0" (
    echo.
    echo Trial Mountain launcher failed with exit code %launcher_exit%.
    echo Diagnostic log:
    echo %~dp0..\work\arcade-renderer-audit\launcher-latest.log
    echo.
    pause
)
exit /b %launcher_exit%
