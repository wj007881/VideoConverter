using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using WinRT.Interop;
using BailianCoding.Services;

namespace BailianCoding.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
        LoadSettings();
    }

    private void LoadSettings()
    {
        OutputFolderTextBox.Text = AppConfiguration.DefaultOutputFolder;
        FfmpegPathTextBox.Text = AppConfiguration.FfmpegPath;
        MaxConcurrentSlider.Value = Environment.ProcessorCount;
    }

    private async void BrowseFfmpegButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".exe");
        
        var hWnd = WindowNative.GetWindowHandle(App.MainWindow!);
        InitializeWithWindow.Initialize(picker, hWnd);

        var file = await picker.PickSingleFileAsync();
        if (file != null)
        {
            FfmpegPathTextBox.Text = file.Path;
            AppConfiguration.FfmpegPath = file.Path;
        }
    }

    private async void TestFfmpegButton_Click(object sender, RoutedEventArgs e)
    {
        FfmpegStatusText.Visibility = Visibility.Visible;
        FfmpegStatusText.Text = "Testing...";
        
        var ffmpegService = new FFmpegService();
        var isAvailable = await ffmpegService.IsFFmpegAvailableAsync();
        
        FfmpegStatusText.Text = isAvailable 
            ? "FFmpeg is working correctly!" 
            : "FFmpeg not found or not working. Please check the path.";
        
        FfmpegStatusText.Foreground = isAvailable 
            ? (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["AccentBrush"]
            : new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
    }

    private async void BrowseOutputButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FolderPicker();
        picker.FileTypeFilter.Add("*");
        
        var hWnd = WindowNative.GetWindowHandle(App.MainWindow!);
        InitializeWithWindow.Initialize(picker, hWnd);

        var folder = await picker.PickSingleFolderAsync();
        if (folder != null)
        {
            OutputFolderTextBox.Text = folder.Path;
        }
    }

    private void MaxConcurrentSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        MaxConcurrentText.Text = ((int)e.NewValue).ToString();
    }
}