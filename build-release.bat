@echo off
setlocal enabledelayedexpansion

echo ========================================
echo Bailian Video Converter Build Script
echo ========================================
echo.

:: Configuration
set PROJECT_NAME=BailianCoding
set VERSION=1.0.0
set CONFIGURATION=Release
set RUNTIME=win-x64

:: Check for required tools
echo Checking prerequisites...

dotnet --version >nul 2>&1
if errorlevel 1 (
    echo [ERROR] .NET SDK not found!
    echo Please install .NET 8 SDK from: https://dotnet.microsoft.com/download
    pause
    exit /b 1
)
echo [OK] .NET SDK found

:: Create directories
if not exist "installer" mkdir "installer"
if not exist "publish" mkdir "publish"

:: Clean previous builds
echo.
echo Cleaning previous builds...
if exist "bin" rmdir /s /q "bin"
if exist "obj" rmdir /s /q "obj"

:: Restore packages
echo.
echo Restoring NuGet packages...
dotnet restore
if errorlevel 1 (
    echo [ERROR] Failed to restore packages!
    pause
    exit /b 1
)

:: Build project
echo.
echo Building project...
dotnet build -c %CONFIGURATION% --no-restore
if errorlevel 1 (
    echo [ERROR] Build failed!
    pause
    exit /b 1
)

:: Publish self-contained application
echo.
echo Publishing self-contained application...
dotnet publish -c %CONFIGURATION% -r %RUNTIME% --self-contained true ^
    -p:PublishSingleFile=false ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -p:WindowsAppSDKSelfContained=true ^
    -p:WindowsPackageType=None ^
    -p:AppxPackageSigningEnabled=false ^
    -p:PublishTrimmed=false ^
    --no-build

if errorlevel 1 (
    echo [ERROR] Publish failed!
    pause
    exit /b 1
)

:: Set output path
set PUBLISH_PATH=bin\%CONFIGURATION%\net8.0-windows10.0.22621.0\%RUNTIME%\publish

:: Verify FFmpeg binaries are included
echo.
echo Verifying FFmpeg binaries...
if not exist "%PUBLISH_PATH%\ffmpeg.exe" (
    echo [WARNING] ffmpeg.exe not found in publish directory!
    echo Copying from FFmpeg folder...
    copy /y "FFmpeg\ffmpeg-2025-11-24-git-c732564d2e-essentials_build\bin\ffmpeg.exe" "%PUBLISH_PATH%\"
    copy /y "FFmpeg\ffmpeg-2025-11-24-git-c732564d2e-essentials_build\bin\ffprobe.exe" "%PUBLISH_PATH%\"
    copy /y "FFmpeg\ffmpeg-2025-11-24-git-c732564d2e-essentials_build\bin\ffplay.exe" "%PUBLISH_PATH%\"
)

if exist "%PUBLISH_PATH%\ffmpeg.exe" (
    echo [OK] FFmpeg binaries included
) else (
    echo [ERROR] FFmpeg binaries not found!
    pause
    exit /b 1
)

:: Create portable package
echo.
echo Creating portable package...
set PORTABLE_ZIP=publish\%PROJECT_NAME%_%VERSION%_Portable.zip
if exist "%PORTABLE_ZIP%" del "%PORTABLE_ZIP%"
powershell -Command "Compress-Archive -Path '%PUBLISH_PATH%\*' -DestinationPath '%PORTABLE_ZIP%' -Force"
echo [OK] Portable package created: %PORTABLE_ZIP%

:: Check for Inno Setup
echo.
echo Checking for Inno Setup...
set ISCC_PATH=
if exist "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" (
    set ISCC_PATH=C:\Program Files (x86)\Inno Setup 6\ISCC.exe
) else if exist "C:\Program Files\Inno Setup 6\ISCC.exe" (
    set ISCC_PATH=C:\Program Files\Inno Setup 6\ISCC.exe
)

if defined ISCC_PATH (
    echo [OK] Inno Setup found at: %ISCC_PATH%
    echo.
    echo Creating installer...
    "%ISCC_PATH%" installer.iss
    if errorlevel 1 (
        echo [WARNING] Installer creation failed!
    ) else (
        echo [OK] Installer created in 'installer' folder
    )
) else (
    echo [WARNING] Inno Setup not found. Skipping installer creation.
    echo To create an installer, install Inno Setup 6 from: https://jrsoftware.org/isinfo.php
)

echo.
echo ========================================
echo Build Complete!
echo ========================================
echo.
echo Output files:
echo   - Portable: %PORTABLE_ZIP%
if exist "installer\%PROJECT_NAME%_Setup_%VERSION%.exe" (
    echo   - Installer: installer\%PROJECT_NAME%_Setup_%VERSION%.exe
)
echo.
echo Publish folder: %PUBLISH_PATH%
echo.
pause