@echo off
echo ========================================
echo Building Bailian Video Converter
echo ========================================

:: Check for .NET SDK
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET SDK not found. Please install .NET 8 SDK.
    pause
    exit /b 1
)

:: Restore packages
echo.
echo Restoring NuGet packages...
dotnet restore

:: Build in Release mode
echo.
echo Building Release version...
dotnet build -c Release

:: Publish as self-contained
echo.
echo Publishing self-contained application...
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -p:IncludeNativeLibrariesForSelfExtract=true -p:WindowsAppSDKSelfContained=true -p:WindowsPackageType=None -p:AppxPackageSigningEnabled=false

echo.
echo ========================================
echo Build Complete!
echo ========================================
echo.
echo Output location: bin\Release\net8.0-windows10.0.22621.0\win-x64\publish\
echo.
pause