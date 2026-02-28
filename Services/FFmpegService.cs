using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BailianCoding.Services;

public class FFmpegService
{
    private readonly string _ffmpegPath;
    private readonly string _ffprobePath;

    public FFmpegService()
    {
        var appDir = AppContext.BaseDirectory;
        _ffmpegPath = Path.Combine(appDir, "ffmpeg.exe");
        _ffprobePath = Path.Combine(appDir, "ffprobe.exe");
        
        if (!File.Exists(_ffmpegPath))
        {
            _ffmpegPath = "ffmpeg";
            _ffprobePath = "ffprobe";
        }
    }

    public string FFmpegPath => _ffmpegPath;
    public string FFprobePath => _ffprobePath;

    public async Task<bool> IsFFmpegAvailableAsync()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _ffmpegPath,
                    Arguments = "-version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<VideoInfo?> GetVideoInfoAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var arguments = $"-v quiet -print_format json -show_format -show_streams \"{filePath}\"";
        
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _ffprobePath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0) return null;

        try
        {
            var json = System.Text.Json.JsonDocument.Parse(output);
            var root = json.RootElement;

            var duration = TimeSpan.Zero;
            var width = 0;
            var height = 0;

            if (root.TryGetProperty("format", out var format) && format.TryGetProperty("duration", out var durationStr))
            {
                duration = TimeSpan.FromSeconds(double.Parse(durationStr.GetString() ?? "0"));
            }

            if (root.TryGetProperty("streams", out var streams))
            {
                foreach (var stream in streams.EnumerateArray())
                {
                    if (stream.TryGetProperty("codec_type", out var codecType) && codecType.GetString() == "video")
                    {
                        if (stream.TryGetProperty("width", out var w)) width = w.GetInt32();
                        if (stream.TryGetProperty("height", out var h)) height = h.GetInt32();
                        break;
                    }
                }
            }

            return new VideoInfo
            {
                Duration = duration,
                Width = width,
                Height = height
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<TranscodeResult> TranscodeAsync(
        string inputPath,
        string outputPath,
        string format,
        string? resolution = null,
        int quality = 23,
        IProgress<double>? progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        var videoInfo = await GetVideoInfoAsync(inputPath, cancellationToken);
        if (videoInfo == null)
        {
            return new TranscodeResult { Success = false, ErrorMessage = "Unable to read video file" };
        }

        var arguments = BuildFFmpegArguments(inputPath, outputPath, format, resolution, quality, videoInfo);
        
        return await RunFFmpegAsync(arguments, videoInfo.Duration, progressCallback, cancellationToken);
    }

    private string BuildFFmpegArguments(string inputPath, string outputPath, string format, string? resolution, int quality, VideoInfo videoInfo)
    {
        var args = new List<string> { "-i", $"\"{inputPath}\"", "-y" };

        var (codec, outputExt) = GetCodecAndExtension(format);
        args.AddRange(new[] { "-c:v", codec });

        if (codec == "libx264" || codec == "libx265")
        {
            args.AddRange(new[] { "-crf", quality.ToString() });
            args.AddRange(new[] { "-preset", "medium" });
        }

        if (!string.IsNullOrEmpty(resolution) && resolution != "Original")
        {
            var res = ParseResolution(resolution, videoInfo);
            if (res != null)
            {
                args.AddRange(new[] { "-vf", $"scale={res.Value.width}:{res.Value.height}:force_original_aspect_ratio=decrease" });
            }
        }

        args.AddRange(new[] { "-c:a", "aac", "-b:a", "192k" });
        args.Add($"\"{Path.ChangeExtension(outputPath, outputExt)}\"");

        return string.Join(" ", args);
    }

    private (string codec, string extension) GetCodecAndExtension(string format)
    {
        return format switch
        {
            var f when f.Contains("H.265") || f.Contains("HEVC") => ("libx265", "mp4"),
            var f when f.Contains("VP9") => ("libvpx-vp9", "webm"),
            var f when f.Contains("AVI") => ("mpeg4", "avi"),
            var f when f.Contains("MKV") => ("libx264", "mkv"),
            var f when f.Contains("MOV") => ("libx264", "mov"),
            _ => ("libx264", "mp4")
        };
    }

    private (int width, int height)? ParseResolution(string resolution, VideoInfo videoInfo)
    {
        return resolution switch
        {
            "4K (3840x2160)" => (3840, 2160),
            "2K (2560x1440)" => (2560, 1440),
            "1080p (1920x1080)" => (1920, 1080),
            "720p (1280x720)" => (1280, 720),
            "480p (854x480)" => (854, 480),
            _ => null
        };
    }

    private async Task<TranscodeResult> RunFFmpegAsync(
        string arguments,
        TimeSpan duration,
        IProgress<double>? progressCallback,
        CancellationToken cancellationToken)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        var errorOutput = string.Empty;
        var timeRegex = new Regex(@"time=(\d+):(\d+):(\d+\.?\d*)", RegexOptions.Compiled);

        process.Start();

        var errorTask = Task.Run(async () =>
        {
            while (!process.HasExited && !cancellationToken.IsCancellationRequested)
            {
                var line = await process.StandardError.ReadLineAsync(cancellationToken);
                if (line == null) break;
                errorOutput += line + "\n";

                var match = timeRegex.Match(line);
                if (match.Success && duration.TotalSeconds > 0)
                {
                    var hours = double.Parse(match.Groups[1].Value);
                    var minutes = double.Parse(match.Groups[2].Value);
                    var seconds = double.Parse(match.Groups[3].Value);
                    var currentTime = hours * 3600 + minutes * 60 + seconds;
                    var progress = Math.Min(100, (currentTime / duration.TotalSeconds) * 100);
                    progressCallback?.Report(progress);
                }
            }
        }, cancellationToken);

        try
        {
            await process.WaitForExitAsync(cancellationToken);
            await errorTask;
        }
        catch (OperationCanceledException)
        {
            try { process.Kill(); } catch { }
            return new TranscodeResult { Success = false, ErrorMessage = "Cancelled" };
        }

        return process.ExitCode == 0
            ? new TranscodeResult { Success = true }
            : new TranscodeResult { Success = false, ErrorMessage = errorOutput };
    }
}

public record VideoInfo
{
    public TimeSpan Duration { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

public record TranscodeResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}