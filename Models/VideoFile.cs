using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;

namespace BailianCoding.Models;

public class VideoFile : INotifyPropertyChanged
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public string Resolution { get; set; } = string.Empty;
    public string FileSize { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    
    private string _status = "Pending";
    public string Status
    {
        get => _status;
        set { _status = value; OnPropertyChanged(); }
    }

    private double _progress;
    public double Progress
    {
        get => _progress;
        set { _progress = value; OnPropertyChanged(); }
    }

    private bool _isSelected = true;
    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public static async Task<VideoFile> CreateFromFileAsync(StorageFile file)
    {
        var basicProperties = await file.GetBasicPropertiesAsync();
        var fileSize = FormatFileSize(basicProperties.Size);
        var extension = file.FileType.ToUpper().TrimStart('.');
        
        return new VideoFile
        {
            FilePath = file.Path,
            FileName = file.Name,
            Format = extension,
            Resolution = "Unknown",
            FileSize = fileSize,
            Duration = "Unknown",
            Status = "Pending"
        };
    }

    private static string FormatFileSize(ulong bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        decimal number = bytes;
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        return $"{number:n1} {suffixes[counter]}";
    }
}