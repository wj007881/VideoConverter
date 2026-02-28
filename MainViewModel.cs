using BailianCoding.Models;
using BailianCoding.Services;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BailianCoding;

public class MainViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly TranscodeManager _transcodeManager;
    private readonly DispatcherQueue _dispatcherQueue;
    private string _outputFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "BailianOutput");

    public ObservableCollection<VideoFile> Files { get; } = new();
    public ObservableCollection<TranscodeTask> Tasks => _transcodeManager.Tasks;

    private int _completedCount;
    public int CompletedCount
    {
        get => _completedCount;
        set { _completedCount = value; OnPropertyChanged(); }
    }

    private int _processingCount;
    public int ProcessingCount
    {
        get => _processingCount;
        set { _processingCount = value; OnPropertyChanged(); }
    }

    private int _pendingCount;
    public int PendingCount
    {
        get => _pendingCount;
        set { _pendingCount = value; OnPropertyChanged(); }
    }

    public string OutputFolder
    {
        get => _outputFolder;
        set
        {
            _outputFolder = value;
            OnPropertyChanged();
        }
    }

    public MainViewModel()
    {
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        _transcodeManager = new TranscodeManager(_dispatcherQueue, Environment.ProcessorCount);
        
        _transcodeManager.TaskCompleted += OnTaskCompleted;
        _transcodeManager.TaskFailed += OnTaskFailed;
        _transcodeManager.AllTasksCompleted += OnAllTasksCompleted;

        if (!Directory.Exists(_outputFolder))
        {
            Directory.CreateDirectory(_outputFolder);
        }
    }

    public async Task StartTranscodingAsync(string format, string resolution, int quality)
    {
        var selectedFiles = Files.Where(f => f.IsSelected).ToList();
        
        if (!selectedFiles.Any()) return;

        foreach (var file in selectedFiles)
        {
            var task = new TranscodeTask
            {
                FilePath = file.FilePath,
                FileName = file.FileName,
                OutputPath = Path.Combine(_outputFolder, file.FileName),
                OutputFormat = format,
                Resolution = resolution,
                Quality = quality
            };

            _transcodeManager.AddTask(task);
            file.Status = "Queued";
        }

        PendingCount = _transcodeManager.Tasks.Count(t => t.Status == "Pending" || t.Status == "Queued");
        
        await _transcodeManager.StartAsync();
    }

    public void StopTranscoding()
    {
        _transcodeManager.Stop();
    }

    public void RemoveSelectedFiles()
    {
        var selected = Files.Where(f => f.IsSelected).ToList();
        foreach (var file in selected)
        {
            Files.Remove(file);
        }
        UpdateCounts();
    }

    public void ClearCompletedTasks()
    {
        _transcodeManager.ClearCompletedTasks();
        UpdateCounts();
    }

    private void OnTaskCompleted(object? sender, TranscodeTask task)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            var file = Files.FirstOrDefault(f => f.FilePath == task.FilePath);
            if (file != null)
            {
                file.Status = "Completed";
                file.Progress = 100;
            }
            UpdateCounts();
        });
    }

    private void OnTaskFailed(object? sender, TranscodeTask task)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            var file = Files.FirstOrDefault(f => f.FilePath == task.FilePath);
            if (file != null)
            {
                file.Status = "Failed";
            }
            UpdateCounts();
        });
    }

    private void OnAllTasksCompleted(object? sender, EventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() => UpdateCounts());
    }

    private void UpdateCounts()
    {
        CompletedCount = Files.Count(f => f.Status == "Completed");
        ProcessingCount = Files.Count(f => f.Status.Contains("Processing"));
        PendingCount = Files.Count(f => f.Status == "Pending" || f.Status == "Queued");
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Dispose()
    {
        _transcodeManager.Dispose();
    }
}