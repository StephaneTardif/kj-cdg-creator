using KJCDGCreator.Audio.Timing;
using KJCDGCreator.Core.Projects;
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

    [Fact]
    public void TogglePlayback_StartsTimingSession()
    {
        var audio = new MockAudioTimeSource();
        var viewModel = new MainWindowViewModel(new TestAudioTimeSourceFactory(_ => audio))
        {
            RawLyricsText = "Be|cause I'm hap|py",
            SourceMp3Path = "/music/demo.mp3"
        };

        viewModel.TogglePlayback();

        Assert.True(audio.IsPlaying);
        Assert.Equal("Pause", viewModel.PlayPauseButtonText);
        Assert.Equal("Be", viewModel.CurrentUnitText);
    }

    [Fact]
    public void Tap_AdvancesCurrentUnit()
    {
        var audio = new MockAudioTimeSource();
        var viewModel = new MainWindowViewModel(new TestAudioTimeSourceFactory(_ => audio))
        {
            RawLyricsText = "Be|cause",
            SourceMp3Path = "/music/demo.mp3"
        };

        viewModel.TogglePlayback();
        audio.Advance(TimeSpan.FromSeconds(2));
        viewModel.TapCurrentUnit();

        Assert.Equal("cause", viewModel.CurrentUnitText);
        Assert.Equal("1 timed / 1 untimed", viewModel.TimingCounts);
    }

    [Fact]
    public void UndoAndReset_UpdateCountsCorrectly()
    {
        var audio = new MockAudioTimeSource();
        var viewModel = new MainWindowViewModel(new TestAudioTimeSourceFactory(_ => audio))
        {
            RawLyricsText = "Be|cause",
            SourceMp3Path = "/music/demo.mp3"
        };

        viewModel.TogglePlayback();
        audio.Advance(TimeSpan.FromSeconds(1));
        viewModel.TapCurrentUnit();
        viewModel.UndoTiming();

        Assert.Equal("0 timed / 2 untimed", viewModel.TimingCounts);

        audio.Advance(TimeSpan.FromSeconds(2));
        viewModel.TapCurrentUnit();
        viewModel.ResetTiming();

        Assert.Equal("0 timed / 2 untimed", viewModel.TimingCounts);
    }

    [Fact]
    public void InvalidMp3Path_SurfacesErrorState()
    {
        var viewModel = new MainWindowViewModel(new TestAudioTimeSourceFactory(_ => throw new FileNotFoundException("MP3 file was not found.")))
        {
            RawLyricsText = "Be cause",
            SourceMp3Path = "/missing/demo.mp3"
        };

        viewModel.TogglePlayback();

        Assert.Contains("Unable to initialize audio", viewModel.StatusMessage, StringComparison.Ordinal);
        Assert.Equal("Play", viewModel.PlayPauseButtonText);
    }

    [Fact]
    public void SaveProject_PersistsUpdatedTiming()
    {
        var path = Path.Combine(Path.GetTempPath(), $"project-{Guid.NewGuid():N}.kjproj.json");
        try
        {
            var audio = new MockAudioTimeSource();
            var viewModel = new MainWindowViewModel(new TestAudioTimeSourceFactory(_ => audio))
            {
                RawLyricsText = "Be|cause",
                SourceMp3Path = "/music/demo.mp3"
            };

            viewModel.TogglePlayback();
            audio.Advance(TimeSpan.FromSeconds(2));
            viewModel.TapCurrentUnit();
            viewModel.SaveProject(path);

            var loaded = KaraokeProjectSerializer.Load(path);
            Assert.Contains(loaded.Timing.Units, unit => unit.Timestamp == TimeSpan.FromSeconds(2));
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
