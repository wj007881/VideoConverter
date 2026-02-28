@echo off
echo ========================================
echo Installing Windows App SDK Runtime
echo ========================================
echo.

:: Check if Windows App Runtime is already installed
powershell -Command "Get-AppxPackage -Name 'Microsoft.WindowsAppRuntime.1.6' | Select-Object Name, Version"

echo.
echo If Windows App Runtime 1.6 is not installed, please download from:
echo https://aka.ms/windowsappsdk/1.6/latest/windowsappruntimeinstall-x64.exe
echo.
pause