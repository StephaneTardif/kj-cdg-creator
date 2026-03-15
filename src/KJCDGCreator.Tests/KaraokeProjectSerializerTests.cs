using KJCDGCreator.Core.Cdg;
using KJCDGCreator.Core.Lyrics;
using KJCDGCreator.Core.Projects;
using KJCDGCreator.Core.Rendering;
using KJCDGCreator.Core.Timing;

namespace KJCDGCreator.Tests;

public sealed class KaraokeProjectSerializerTests : IDisposable
{
    private readonly string _outputDirectory = Path.Combine(Path.GetTempPath(), "kj-cdg-creator-project-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public void SaveThenLoad_RoundTripsProject()
    {
        var path = CreateOutputPath("roundtrip.kjproj.json");
        var project = CreateProject();

        KaraokeProjectSerializer.Save(project, path);
        var loaded = KaraokeProjectSerializer.Load(path);

        Assert.Equal(project.ProjectVersion, loaded.ProjectVersion);
        Assert.Equal(project.Title, loaded.Title);
        Assert.Equal(project.Artist, loaded.Artist);
        Assert.Equal(project.SourceMp3Path, loaded.SourceMp3Path);
        Assert.Equal(project.RawLyricsText, loaded.RawLyricsText);
    }

    [Fact]
    public void SaveThenLoad_PreservesTimestamps()
    {
        var path = CreateOutputPath("timing.kjproj.json");
        var project = CreateProject();

        KaraokeProjectSerializer.Save(project, path);
        var loaded = KaraokeProjectSerializer.Load(path);

        Assert.Equal(project.Timing.Units.Select(unit => unit.Timestamp), loaded.Timing.Units.Select(unit => unit.Timestamp));
    }

    [Fact]
    public void SaveThenLoad_PreservesRawLyricsTextExactly()
    {
        var path = CreateOutputPath("lyrics.kjproj.json");
        var rawLyrics = "Be|cause I'm hap|py\r\nClap a|long\r\n\r\nSing a song";
        var project = CreateProject() with { RawLyricsText = rawLyrics };

        KaraokeProjectSerializer.Save(project, path);
        var loaded = KaraokeProjectSerializer.Load(path);

        Assert.Equal(rawLyrics, loaded.RawLyricsText);
    }

    [Fact]
    public void SaveThenLoad_PreservesIntroOptions()
    {
        var path = CreateOutputPath("intro.kjproj.json");
        var project = CreateProject();

        KaraokeProjectSerializer.Save(project, path);
        var loaded = KaraokeProjectSerializer.Load(path);

        Assert.Equal(project.IntroOptions, loaded.IntroOptions);
        Assert.Equal(project.FrameRenderOptions?.IntroOptions, loaded.FrameRenderOptions?.IntroOptions);
    }

    [Fact]
    public void SaveThenLoad_PreservesOptionalNullFieldsSafely()
    {
        var path = CreateOutputPath("nulls.kjproj.json");
        var project = new KaraokeProject(
            ProjectVersion: KaraokeProjectSerializer.CurrentProjectVersion,
            Title: null,
            Artist: null,
            RawLyricsText: "Be cause",
            Timing: TimingDocumentBuilder.FromLyrics(LyricsParser.Parse("Be cause")),
            SourceMp3Path: null,
            IntroOptions: null,
            FrameRenderOptions: null,
            ExportOptions: null);

        KaraokeProjectSerializer.Save(project, path);
        var loaded = KaraokeProjectSerializer.Load(path);

        Assert.Null(loaded.Title);
        Assert.Null(loaded.Artist);
        Assert.Null(loaded.SourceMp3Path);
        Assert.Null(loaded.IntroOptions);
        Assert.Null(loaded.FrameRenderOptions);
        Assert.Null(loaded.ExportOptions);
    }

    [Fact]
    public void Save_IsDeterministicForSameProject()
    {
        var firstPath = CreateOutputPath("first.kjproj.json");
        var secondPath = CreateOutputPath("second.kjproj.json");
        var project = CreateProject();

        KaraokeProjectSerializer.Save(project, firstPath);
        KaraokeProjectSerializer.Save(project, secondPath);

        Assert.Equal(File.ReadAllText(firstPath), File.ReadAllText(secondPath));
    }

    [Fact]
    public void Load_HandlesMissingOptionalFieldsForBackwardCompatibility()
    {
        var path = CreateOutputPath("backward-compatible.kjproj.json");
        File.WriteAllText(
            path,
            """
            {
              "rawLyricsText": "Be|cause I'm hap|py"
            }
            """);

        var loaded = KaraokeProjectSerializer.Load(path);

        Assert.Equal(KaraokeProjectSerializer.CurrentProjectVersion, loaded.ProjectVersion);
        Assert.Equal("Be|cause I'm hap|py", loaded.RawLyricsText);
        Assert.Null(loaded.Title);
        Assert.Null(loaded.Artist);
        Assert.Null(loaded.IntroOptions);
        Assert.Null(loaded.FrameRenderOptions);
        Assert.Null(loaded.ExportOptions);
        Assert.Equal(5, loaded.Timing.Units.Count);
    }

    public void Dispose()
    {
        if (Directory.Exists(_outputDirectory))
        {
            Directory.Delete(_outputDirectory, recursive: true);
        }
    }

    private string CreateOutputPath(string name)
    {
        Directory.CreateDirectory(_outputDirectory);
        return Path.Combine(_outputDirectory, name);
    }

    private static KaraokeProject CreateProject()
    {
        const string rawLyrics = """
            Be|cause I'm hap|py
            Clap a|long
            """;

        var timing = TimingDocumentBuilder.FromLyrics(LyricsParser.Parse(rawLyrics));
        timing.AssignTimestamp(0, TimeSpan.FromSeconds(1));
        timing.AssignTimestamp(1, TimeSpan.FromSeconds(2));
        timing.AssignTimestamp(2, TimeSpan.FromSeconds(3));

        var introOptions = new IntroTitleScreenOptions(
            Enabled: true,
            FixedDuration: TimeSpan.FromSeconds(2),
            UseFirstLyricTimestampWhenLonger: true,
            BackgroundColor: 0,
            TitleColor: 10,
            ArtistColor: 14,
            TitleRow: 4,
            ArtistRow: 7,
            CenterHorizontally: true);

        var frameOptions = new KaraokeFrameRenderOptions(
            new PageLayoutOptions(
                StartRow: 6,
                StartColumn: 4,
                LineSpacing: 2,
                CenterHorizontally: true,
                CenterVertically: true),
            new HighlightedLyricsRenderOptions(
                StartRow: 6,
                StartColumn: 4,
                LineSpacing: 2,
                BackgroundColor: 0,
                BaseTextColor: 15,
                HighlightTextColor: 12,
                ClearScreenBeforeRender: true),
            SongMetadata: new KaraokeSongMetadata("Happy Demo", "KJ CDG Creator"),
            IntroOptions: introOptions);

        return new KaraokeProject(
            ProjectVersion: KaraokeProjectSerializer.CurrentProjectVersion,
            Title: "Happy Demo",
            Artist: "KJ CDG Creator",
            RawLyricsText: rawLyrics,
            Timing: timing,
            SourceMp3Path: "/music/happy-demo.mp3",
            IntroOptions: introOptions,
            FrameRenderOptions: frameOptions,
            ExportOptions: new CdgTimelineExportOptions(
                FrameStep: TimeSpan.FromSeconds(1),
                FrameRenderOptions: frameOptions,
                EndPadding: TimeSpan.FromSeconds(1),
                IncludeInitialClearFrame: true));
    }
}
