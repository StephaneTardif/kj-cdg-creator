using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using KJCDGCreator.Editor.Services;
using KJCDGCreator.Editor.ViewModels;

namespace KJCDGCreator.Editor.Views;

public partial class MainWindow : Window
{
    private readonly IProjectFileDialogService _fileDialogService;
    private readonly DispatcherTimer _tapTimingRefreshTimer;

    public MainWindow()
    {
        InitializeComponent();
        _fileDialogService = new ProjectFileDialogService(this);
        _tapTimingRefreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(200)
        };
        _tapTimingRefreshTimer.Tick += TapTimingRefreshTimer_Tick;
        _tapTimingRefreshTimer.Start();
        Closed += MainWindow_Closed;
    }

    private MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;

    private void TapTimingRefreshTimer_Tick(object? sender, EventArgs e)
    {
        ViewModel?.RefreshTapTimingState();
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        _tapTimingRefreshTimer.Stop();
        ViewModel?.Dispose();
    }

    private void NewProject_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ViewModel?.NewProject();
    }

    private async void OpenProject_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        try
        {
            var path = await _fileDialogService.OpenProjectPathAsync();
            if (!string.IsNullOrWhiteSpace(path))
            {
                ViewModel.LoadProject(path);
            }
        }
        catch (Exception exception)
        {
            ViewModel.SetStatusMessage($"Open failed: {exception.Message}");
        }
    }

    private async void SaveProject_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        try
        {
            var targetPath = ViewModel.CurrentProjectPath;
            if (string.IsNullOrWhiteSpace(targetPath))
            {
                targetPath = await _fileDialogService.SaveProjectPathAsync(null, "project.kjproj.json");
            }

            if (!string.IsNullOrWhiteSpace(targetPath))
            {
                ViewModel.SaveProject(targetPath);
            }
        }
        catch (Exception exception)
        {
            ViewModel.SetStatusMessage($"Save failed: {exception.Message}");
        }
    }

    private async void SaveProjectAs_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        try
        {
            var suggestedName = string.IsNullOrWhiteSpace(ViewModel.Title)
                ? "project.kjproj.json"
                : $"{ViewModel.Title.Trim().Replace(' ', '-').ToLowerInvariant()}.kjproj.json";
            var targetPath = await _fileDialogService.SaveProjectPathAsync(ViewModel.CurrentProjectPath, suggestedName);

            if (!string.IsNullOrWhiteSpace(targetPath))
            {
                ViewModel.SaveProject(targetPath);
            }
        }
        catch (Exception exception)
        {
            ViewModel.SetStatusMessage($"Save As failed: {exception.Message}");
        }
    }

    private void PlayPauseTiming_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ViewModel?.TogglePlayback();
    }

    private void StopTiming_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ViewModel?.StopPlayback();
    }

    private void TapTiming_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ViewModel?.TapCurrentUnit();
    }

    private void UndoTiming_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ViewModel?.UndoTiming();
    }

    private void ResetTiming_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ViewModel?.ResetTiming();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        var focusedElement = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement();
        if (e.Key == Key.Space && focusedElement is not TextBox)
        {
            ViewModel?.TapCurrentUnit();
            e.Handled = true;
        }
    }
}
