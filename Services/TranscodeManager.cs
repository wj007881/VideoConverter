using BailianCoding.Models;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BailianCoding.Services;

public class TranscodeManager : IDisposable
{
    private readonly FFmpegService _ffmpegService;
    private readonly ConcurrentQueue<TranscodeTask> _taskQueue = new();
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _activeTasks = new();
    private readonly SemaphoreSlim _semaphore;
    private readonly DispatcherQueue _dispatcherQueue;
    private int _maxConcurrentTasks = 4;
    private int _runningTasks = 0;

    public ObservableCollection<TranscodeTask> Tasks { get; } = new();
    public int MaxConcurrentTasks
    {
        get => _maxConcurrentTasks;
        set => _maxConcurrentTasks = Math.Max(1, Math.Min(Environment.ProcessorCount, value));
    }

    public event EventHandler<TranscodeTask>? TaskCompleted;
    public event EventHandler<TranscodeTask>? TaskFailed;
    public event EventHandler? AllTasksCompleted;

    public TranscodeManager(DispatcherQueue dispatcherQueue, int maxConcurrentTasks = 4)
    {
        _dispatcherQueue = dispatcherQueue;
        _ffmpegService = new FFmpegService();
        _semaphore = new SemaphoreSlim(maxConcurrentTasks);
        _maxConcurrentTasks = maxConcurrentTasks;
    }

    public void AddTask(TranscodeTask task)
    {
        _taskQueue.Enqueue(task);
        _dispatcherQueue.TryEnqueue(() => Tasks.Add(task));
    }

    public void AddTasks(IEnumerable<TranscodeTask> tasks)
    {
        foreach (var task in tasks)
        {
            AddTask(task);
        }
    }

    public async Task StartAsync()
    {
        var tasksToProcess = new List<TranscodeTask>();
        while (_taskQueue.TryDequeue(out var task))
        {
            tasksToProcess.Add(task);
        }

        if (tasksToProcess.Count == 0) return;

        var tasks = tasksToProcess.Select(t => ProcessTaskAsync(t)).ToArray();
        await Task.WhenAll(tasks);

        AllTasksCompleted?.Invoke(this, EventArgs.Empty);
    }

    private async Task ProcessTaskAsync(TranscodeTask task)
    {
        await _semaphore.WaitAsync();

        try
        {
            Interlocked.Increment(ref _runningTasks);
            
            var cts = new CancellationTokenSource();
            _activeTasks[task.Id] = cts;

            _dispatcherQueue.TryEnqueue(() => task.Status = "Processing");

            var progress = new Progress<double>(p =>
            {
                _dispatcherQueue.TryEnqueue(() =>
                {
                    task.Progress = p;
                    task.Status = $"Processing ({p:F1}%)";
                });
            });

            var result = await _ffmpegService.TranscodeAsync(
                task.FilePath,
                task.OutputPath,
                task.OutputFormat,
                task.Resolution,
                task.Quality,
                progress,
                cts.Token);

            _dispatcherQueue.TryEnqueue(() =>
            {
                if (result.Success)
                {
                    task.Status = "Completed";
                    task.Progress = 100;
                    TaskCompleted?.Invoke(this, task);
                }
                else
                {
                    task.Status = cts.Token.IsCancellationRequested ? "Cancelled" : "Failed";
                    TaskFailed?.Invoke(this, task);
                }
            });

            _activeTasks.TryRemove(task.Id, out _);
        }
        finally
        {
            Interlocked.Decrement(ref _runningTasks);
            _semaphore.Release();
        }
    }

    public void Stop()
    {
        foreach (var cts in _activeTasks.Values)
        {
            cts.Cancel();
        }

        _taskQueue.Clear();
    }

    public void CancelTask(string taskId)
    {
        if (_activeTasks.TryGetValue(taskId, out var cts))
        {
            cts.Cancel();
        }
    }

    public void ClearCompletedTasks()
    {
        var completedTasks = Tasks.Where(t => t.Status == "Completed" || t.Status == "Failed" || t.Status == "Cancelled").ToList();
        
        _dispatcherQueue.TryEnqueue(() =>
        {
            foreach (var task in completedTasks)
            {
                Tasks.Remove(task);
            }
        });
    }

    public void Dispose()
    {
        Stop();
        _semaphore.Dispose();
        foreach (var cts in _activeTasks.Values)
        {
            cts.Dispose();
        }
    }
}