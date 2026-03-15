using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace KJCDGCreator.Editor.Services;

public sealed class ProjectFileDialogService(Window owner) : IProjectFileDialogService
{
    public async Task<string?> OpenProjectPathAsync()
    {
        var files = await owner.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                AllowMultiple = false,
                Title = "Open Karaoke Project",
                FileTypeFilter =
                [
                    new FilePickerFileType("KJ CDG Creator Project")
                    {
                        Patterns = ["*.kjproj.json"]
                    },
                    new FilePickerFileType("JSON")
                    {
                        Patterns = ["*.json"]
                    }
                ]
            });

        return files.Count > 0 ? files[0].TryGetLocalPath() : null;
    }

    public async Task<string?> SaveProjectPathAsync(string? currentPath, string suggestedFileName)
    {
        var file = await owner.StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title = "Save Karaoke Project",
                SuggestedFileName = suggestedFileName,
                DefaultExtension = "json",
                FileTypeChoices =
                [
                    new FilePickerFileType("KJ CDG Creator Project")
                    {
                        Patterns = ["*.kjproj.json"]
                    }
                ],
                SuggestedStartLocation = await GetSuggestedStartLocationAsync(currentPath)
            });

        return file?.TryGetLocalPath();
    }

    private async Task<IStorageFolder?> GetSuggestedStartLocationAsync(string? currentPath)
    {
        var directory = string.IsNullOrWhiteSpace(currentPath)
            ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            : Path.GetDirectoryName(currentPath);

        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
        {
            return null;
        }

        return await owner.StorageProvider.TryGetFolderFromPathAsync(directory);
    }
}
