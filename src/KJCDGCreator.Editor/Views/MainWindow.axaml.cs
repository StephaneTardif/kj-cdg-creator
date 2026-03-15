using Avalonia.Controls;
using KJCDGCreator.Editor.Services;
using KJCDGCreator.Editor.ViewModels;

namespace KJCDGCreator.Editor.Views;

public partial class MainWindow : Window
{
    private readonly IProjectFileDialogService _fileDialogService;

    public MainWindow()
    {
        InitializeComponent();
        _fileDialogService = new ProjectFileDialogService(this);
    }

    private MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;

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
}
