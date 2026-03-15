using KJCDGCreator.Editor.ViewModels;

namespace KJCDGCreator.Tests;

public sealed class MainWindowViewModelTests
{
    [Fact]
    public void LoadProject_UpdatesFieldsFromDisk()
    {
        var path = Path.Combine(Path.GetTempPath(), $"project-{Guid.NewGuid():N}.kjproj.json");
        try
        {
            File.WriteAllText(
                path,
                """
                {
                  "projectVersion": "1.0",
                  "title": "Demo Title",
                  "artist": "Demo Artist",
                  "rawLyricsText": "Be|cause I'm hap|py",
                  "timing": {
                    "units": [
                      { "unitIndex": 0, "text": "Be", "timestamp": "00:00:01" }
                    ]
                  },
                  "sourceMp3Path": "/music/demo.mp3"
                }
                """);

            var viewModel = new MainWindowViewModel();

            viewModel.LoadProject(path);

            Assert.Equal("Demo Title", viewModel.Title);
            Assert.Equal("Demo Artist", viewModel.Artist);
            Assert.Equal("/music/demo.mp3", viewModel.SourceMp3Path);
            Assert.Equal("Be|cause I'm hap|py", viewModel.RawLyricsText);
            Assert.False(viewModel.HasUnsavedChanges);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public void SaveProject_WritesSerializerOutput()
    {
        var path = Path.Combine(Path.GetTempPath(), $"project-{Guid.NewGuid():N}.kjproj.json");
        try
        {
            var viewModel = new MainWindowViewModel
            {
                Title = "Saved Title",
                Artist = "Saved Artist",
                SourceMp3Path = "/music/saved.mp3",
                RawLyricsText = "Clap a|long"
            };

            viewModel.SaveProject(path);

            var json = File.ReadAllText(path);
            Assert.Contains("\"title\": \"Saved Title\"", json, StringComparison.Ordinal);
            Assert.Contains("\"artist\": \"Saved Artist\"", json, StringComparison.Ordinal);
            Assert.Contains("\"rawLyricsText\": \"Clap a|long\"", json, StringComparison.Ordinal);
            Assert.False(viewModel.HasUnsavedChanges);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public void NewProject_ResetsFields()
    {
        var viewModel = new MainWindowViewModel
        {
            Title = "Existing",
            Artist = "Artist",
            SourceMp3Path = "/music/source.mp3",
            RawLyricsText = "Be cause"
        };

        viewModel.NewProject();

        Assert.Equal(string.Empty, viewModel.Title);
        Assert.Equal(string.Empty, viewModel.Artist);
        Assert.Equal(string.Empty, viewModel.SourceMp3Path);
        Assert.Equal(string.Empty, viewModel.RawLyricsText);
        Assert.False(viewModel.HasUnsavedChanges);
        Assert.Null(viewModel.CurrentProjectPath);
    }
}
