using System;
using System.IO;
using System.Reflection;

namespace BailianCoding.Services;

public static class AppConfiguration
{
    private static string _ffmpegPath = string.Empty;
    private static string _ffprobePath = string.Empty;

    public static string AppName => "Bailian Video Converter";
    public static string AppVersion => GetVersion();

    public static string FfmpegPath
    {
        get => _ffmpegPath;
        set
        {
            _ffmpegPath = value;
            _ffprobePath = Path.Combine(Path.GetDirectoryName(value) ?? "", "ffprobe.exe");
        }
    }

    public static string FfprobePath => _ffprobePath;

    public static string DefaultOutputFolder =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "BailianOutput");

    public static string TempFolder =>
        Path.Combine(Path.GetTempPath(), "BailianCoding");

    public static string[] SupportedFormats => new[]
    {
        ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".webm", ".m4v", ".ts", ".mts", ".m2ts"
    };

    public static string[] OutputFormats => new[]
    {
        "MP4 (H.264)",
        "MP4 (H.265/HEVC)",
        "WebM (VP9)",
        "AVI",
        "MKV",
        "MOV"
    };

    public static string[] Resolutions => new[]
    {
        "Original",
        "4K (3840x2160)",
        "2K (2560x1440)",
        "1080p (1920x1080)",
        "720p (1280x720)",
        "480p (854x480)"
    };

    private static string GetVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return version != null ? $"v{version.Major}.{version.Minor}.{version.Build}" : "v1.0.0";
    }

    public static void EnsureDirectoriesExist()
    {
        if (!Directory.Exists(DefaultOutputFolder))
        {
            Directory.CreateDirectory(DefaultOutputFolder);
        }

        if (!Directory.Exists(TempFolder))
        {
            Directory.CreateDirectory(TempFolder);
        }
    }

    public static bool ValidateFFmpeg()
    {
        if (string.IsNullOrEmpty(_ffmpegPath))
        {
            _ffmpegPath = "ffmpeg";
            _ffprobePath = "ffprobe";
        }
        return File.Exists(_ffmpegPath) || IsInPath("ffmpeg");
    }

    private static bool IsInPath(string executable)
    {
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathEnv)) return false;

        var paths = pathEnv.Split(Path.PathSeparator);
        foreach (var path in paths)
        {
            var fullPath = Path.Combine(path, executable + ".exe");
            if (File.Exists(fullPath)) return true;
        }
        return false;
    }
}