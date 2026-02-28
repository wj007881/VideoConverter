# Video Converter (BailianCoding)

A modern video transcoding application built with WinUI 3 for Windows 10/11.

## Features

- Batch video conversion with parallel processing
- Support for multiple output formats: MP4 (H.264), H.265/HEVC, VP9, AVI, MKV, MOV
- Resolution presets: 4K, 2K, 1080p, 720p, 480p
- Adjustable quality settings (CRF)
- Real-time progress tracking
- Drag and drop file support

## Requirements

- Windows 10 version 1809 (17763) or later
- .NET 9.0 Runtime
- FFmpeg (see setup instructions below)

## Setup

### 1. Clone the Repository

```bash
git clone https://github.com/wj007881/VideoConverter.git
cd VideoConverter
```

### 2. Download FFmpeg

This project requires FFmpeg for video processing. Due to file size limitations, FFmpeg binaries are not included in this repository.

1. Download FFmpeg Essentials build from: https://www.gyan.dev/ffmpeg/builds/#release-builds
   - Look for "ffmpeg-release-essentials.zip"
2. Extract the downloaded ZIP file

### 3. Place FFmpeg Files

Place the FFmpeg binaries in the following directory structure:

```
VideoConverter/
├── FFmpeg/
│   └── ffmpeg-2025-11-24-git-c732564d2e-essentials_build/
│       └── bin/
│           ├── ffmpeg.exe
│           ├── ffprobe.exe
│           └── ffplay.exe
├── BailianCoding.csproj
└── ...
```

**Important:** The folder name must match the path in `BailianCoding.csproj`. If your FFmpeg version has a different folder name, either:
- Rename the extracted folder to `ffmpeg-2025-11-24-git-c732564d2e-essentials_build`, or
- Update the paths in `BailianCoding.csproj` to match your folder name

### 4. Build and Run

Using Visual Studio 2022:
1. Open `BailianCoding.csproj`
2. Restore NuGet packages
3. Build and run the project

Using command line:
```bash
dotnet restore
dotnet build
dotnet run
```

### 5. Create Release Package

Run the build script to create a portable release:

```bash
build-release.bat
```

This will create a portable ZIP package in the `publish/` folder containing all necessary files including FFmpeg binaries.

## Project Structure

```
VideoConverter/
├── App.xaml(.cs)           # Application entry point
├── MainWindow.xaml(.cs)    # Main window
├── MainViewModel.cs        # Main view model (MVVM)
├── Models/
│   ├── VideoFile.cs        # Video file model
│   └── TranscodeTask.cs    # Transcoding task model
├── Services/
│   ├── FFmpegService.cs    # FFmpeg integration
│   ├── TranscodeManager.cs # Parallel transcoding management
│   └── AppConfiguration.cs # Application settings
├── Views/
│   └── SettingsPage.xaml   # Settings page
├── Converters/             # Value converters
├── FFmpeg/                 # FFmpeg binaries (not included)
├── build.bat               # Debug build script
├── build-release.bat       # Release build script
└── installer.iss           # Inno Setup installer script
```

## Supported Formats

| Format | Video Codec | Container |
|--------|-------------|-----------|
| MP4 (H.264) | libx264 | .mp4 |
| H.265/HEVC | libx265 | .mp4 |
| VP9 | libvpx-vp9 | .webm |
| AVI | mpeg4 | .avi |
| MKV | libx264 | .mkv |
| MOV | libx264 | .mov |

## License

This project is provided as-is. FFmpeg is licensed under the LGPL/GPL license. See FFmpeg's LICENSE file for details.

## Acknowledgments

- [FFmpeg](https://ffmpeg.org/) - Multimedia framework
- [WinUI 3](https://microsoft.github.io/microsoft-ui-xaml/) - Windows App SDK UI framework