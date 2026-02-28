using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using BailianCoding.Models;
using BailianCoding.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace BailianCoding;

public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; }

    public MainWindow()
    {
        InitializeComponent();
        ViewModel = new MainViewModel();
        UpdateFileCount();
        
        QualitySlider.ValueChanged += QualitySlider_ValueChanged;
        ViewModel.Files.CollectionChanged += (s, e) => UpdateFileCount();
    }

    private void QualitySlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        int crf = (int)e.NewValue;
        string quality = crf switch
        {
            <= 18 => "Lossless",
            <= 23 => "Good",
            <= 28 => "Medium",
            _ => "Low"
        };
        QualityText.Text = $"CRF: {crf} ({quality})";
    }

    private void UpdateFileCount()
    {
        FileCountText.Text = $"({ViewModel.Files.Count} files)";
        EmptyStatePanel.Visibility = ViewModel.Files.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void BrowseOutputButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FolderPicker();
        picker.FileTypeFilter.Add("*");
        
        var hWnd = WindowNative.GetWindowHandle(this);
        InitializeWithWindow.Initialize(picker, hWnd);

        var folder = await picker.PickSingleFolderAsync();
        if (folder != null)
        {
            ViewModel.OutputFolder = folder.Path;
        }
    }

    private async void AddFilesButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".mp4");
        picker.FileTypeFilter.Add(".mkv");
        picker.FileTypeFilter.Add(".avi");
        picker.FileTypeFilter.Add(".mov");
        picker.FileTypeFilter.Add(".wmv");
        picker.FileTypeFilter.Add(".flv");
        picker.FileTypeFilter.Add(".webm");
        picker.FileTypeFilter.Add(".m4v");
        picker.FileTypeFilter.Add("*");

        var hWnd = WindowNative.GetWindowHandle(this);
        InitializeWithWindow.Initialize(picker, hWnd);

        picker.ViewMode = PickerViewMode.Thumbnail;
        picker.SuggestedStartLocation = PickerLocationId.VideosLibrary;

        var files = await picker.PickMultipleFilesAsync();
        
        foreach (var file in files)
        {
            var videoFile = await VideoFile.CreateFromFileAsync(file);
            if (!ViewModel.Files.Contains(videoFile))
            {
                ViewModel.Files.Add(videoFile);
            }
        }
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        var format = FormatComboBox.SelectedItem as string ?? "MP4 (H.264)";
        var resolution = ResolutionComboBox.SelectedItem as string ?? "Original";
        var quality = (int)QualitySlider.Value;

        await ViewModel.StartTranscodingAsync(format, resolution, quality);
    }

    private void StopButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.StopTranscoding();
    }
}