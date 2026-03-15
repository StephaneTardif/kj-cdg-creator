namespace KJCDGCreator.Editor.Services;

public interface IProjectFileDialogService
{
    Task<string?> OpenProjectPathAsync();

    Task<string?> SaveProjectPathAsync(string? currentPath, string suggestedFileName);
}
