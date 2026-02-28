@echo off
setlocal enabledelayedexpansion

echo ========================================
echo Creating Distribution Package
echo ========================================
echo.

set VERSION=1.0.0
set PROJECT_NAME=BailianCoding
set BUILD_PATH=bin\Release\net9.0-windows10.0.19041.0
set OUTPUT_PATH=publish

:: Create output directory
if not exist "%OUTPUT_PATH%" mkdir "%OUTPUT_PATH%"

:: Create portable package
echo Creating portable package...
set PORTABLE_DIR=%OUTPUT_PATH%\%PROJECT_NAME%_%VERSION%_Portable
if exist "%PORTABLE_DIR%" rmdir /s /q "%PORTABLE_DIR%"
mkdir "%PORTABLE_DIR%"

:: Copy all files from build output
echo Copying application files...
xcopy /s /e /y "%BUILD_PATH%\*" "%PORTABLE_DIR%\"

:: Create README
echo Creating README...
echo Bailian Video Converter v%VERSION% > "%PORTABLE_DIR%\README.txt"
echo. >> "%PORTABLE_DIR%\README.txt"
echo A modern video transcoding application. >> "%PORTABLE_DIR%\README.txt"
echo. >> "%PORTABLE_DIR%\README.txt"
echo Features: >> "%PORTABLE_DIR%\README.txt"
echo - Multiple format support: MP4, WebM, AVI, MKV, MOV >> "%PORTABLE_DIR%\README.txt"
echo - Resolution presets: 4K, 2K, 1080p, 720p, 480p >> "%PORTABLE_DIR%\README.txt"
echo - Quality control with CRF >> "%PORTABLE_DIR%\README.txt"
echo - Multi-threaded transcoding >> "%PORTABLE_DIR%\README.txt"
echo - Real-time progress display >> "%PORTABLE_DIR%\README.txt"
echo. >> "%PORTABLE_DIR%\README.txt"
echo Requirements: >> "%PORTABLE_DIR%\README.txt"
echo - Windows 10 version 1809 or later >> "%PORTABLE_DIR%\README.txt"
echo. >> "%PORTABLE_DIR%\README.txt"
echo Usage: >> "%PORTABLE_DIR%\README.txt"
echo 1. Run BailianCoding.exe >> "%PORTABLE_DIR%\README.txt"
echo 2. Add video files >> "%PORTABLE_DIR%\README.txt"
echo 3. Select output format and resolution >> "%PORTABLE_DIR%\README.txt"
echo 4. Click Start to begin transcoding >> "%PORTABLE_DIR%\README.txt"

:: Create ZIP archive
echo Creating ZIP archive...
set ZIP_FILE=%OUTPUT_PATH%\%PROJECT_NAME%_%VERSION%_Portable.zip
if exist "%ZIP_FILE%" del "%ZIP_FILE%"
powershell -Command "Compress-Archive -Path '%PORTABLE_DIR%\*' -DestinationPath '%ZIP_FILE%' -Force"

echo.
echo ========================================
echo Package Created Successfully!
echo ========================================
echo.
echo Portable package: %ZIP_FILE%
echo.
pause