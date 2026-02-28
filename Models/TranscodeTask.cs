using Microsoft.UI.Xaml;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BailianCoding.Models;

public class TranscodeTask : INotifyPropertyChanged
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;
    public string OutputFormat { get; set; } = string.Empty;
    public string Resolution { get; set; } = string.Empty;
    public int Quality { get; set; } = 23;

    private double _progress;
    public double Progress
    {
        get => _progress;
        set { _progress = value; OnPropertyChanged(); OnPropertyChanged(nameof(ProgressText)); }
    }

    private string _status = "Pending";
    public string Status
    {
        get => _status;
        set { _status = value; OnPropertyChanged(); }
    }

    private TimeSpan _elapsedTime;
    public TimeSpan ElapsedTime
    {
        get => _elapsedTime;
        set { _elapsedTime = value; OnPropertyChanged(); }
    }

    private TimeSpan? _estimatedTimeRemaining;
    public TimeSpan? EstimatedTimeRemaining
    {
        get => _estimatedTimeRemaining;
        set { _estimatedTimeRemaining = value; OnPropertyChanged(); }
    }

    public string ProgressText => $"{Progress:F1}%";

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public enum TranscodeStatus
{
    Pending,
    Queued,
    Processing,
    Completed,
    Failed,
    Cancelled
}