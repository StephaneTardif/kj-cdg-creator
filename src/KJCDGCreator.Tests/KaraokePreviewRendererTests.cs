using System.Security.Cryptography;
using KJCDGCreator.Core.Cdg;
using KJCDGCreator.Core.Projects;
using KJCDGCreator.Core.Rendering;
using KJCDGCreator.Core.Timing;
using KJCDGCreator.Editor.Services;

namespace KJCDGCreator.Tests;

public sealed class KaraokePreviewRendererTests
{
    [Fact]
    public void Render_HandlesEmptyProjectSafely()
    {
        var renderer = new KaraokePreviewRenderer();

        var result = renderer.Render(null, TimeSpan.Zero);

        Assert.False(result.HasContent);
        Assert.Contains("Preview unavailable", result.StatusMessage, StringComparison.Ordinal);
        Assert.NotEmpty(result.PixelData);
    }

    [Fact]
    public void Render_IsDeterministicForSameInputs()
    {
        var renderer = new KaraokePreviewRenderer();
        var project = CreateProject("Be|cause I'm hap|py");

        var first = renderer.Render(project, TimeSpan.FromSeconds(2));
        var second = renderer.Render(project, TimeSpan.FromSeconds(2));

        Assert.Equal(HashPixels(first.PixelData), HashPixels(second.PixelData));
    }

    [Fact]
    public void Render_IntroTitleScreenWorks()
    {
        var renderer = new KaraokePreviewRenderer();
        var project = CreateProject(
            "Be|cause",
            introOptions: new IntroTitleScreenOptions(
                Enabled: true,
                FixedDuration: TimeSpan.FromSeconds(2),
                UseFirstLyricTimestampWhenLonger: true,
                BackgroundColor: 0,
                TitleColor: 10,
                ArtistColor: 14,
                TitleRow: 4,
                ArtistRow: 7,
                CenterHorizontally: true));
        project.Timing.AssignTimestamp(0, TimeSpan.FromSeconds(3));

        var intro = renderer.Render(project, TimeSpan.FromSeconds(1));
        var afterIntro = renderer.Render(project, TimeSpan.FromSeconds(3.5));

        Assert.NotEqual(HashPixels(intro.PixelData), HashPixels(afterIntro.PixelData));
        Assert.True(intro.HasContent);
    }

    [Fact]
    public void Render_ChangesAsTimingAdvances()
    {
        var renderer = new KaraokePreviewRenderer();
        var project = CreateProject("Be|cause I'm hap|py");
        project.Timing.AssignTimestamp(0, TimeSpan.FromSeconds(1));
        project.Timing.AssignTimestamp(1, TimeSpan.FromSeconds(2));
        project.Timing.AssignTimestamp(2, TimeSpan.FromSeconds(3));

        var before = renderer.Render(project, TimeSpan.FromSeconds(0.5));
        var after = renderer.Render(project, TimeSpan.FromSeconds(2.5));

        Assert.NotEqual(HashPixels(before.PixelData), HashPixels(after.PixelData));
    }

    private static KaraokeProject CreateProject(string rawLyricsText, IntroTitleScreenOptions? introOptions = null)
    {
        var lyrics = Core.Lyrics.LyricsParser.Parse(rawLyricsText);
        var timing = TimingDocumentBuilder.FromLyrics(lyrics);

        return new KaraokeProject(
            ProjectVersion: KaraokeProjectSerializer.CurrentProjectVersion,
            Title: "Preview Song",
            Artist: "Preview Artist",
            RawLyricsText: rawLyricsText,
            Timing: timing,
            SourceMp3Path: null,
            IntroOptions: introOptions,
            FrameRenderOptions: new KaraokeFrameRenderOptions(
                new PageLayoutOptions(
                    StartRow: 4,
                    StartColumn: 4,
                    LineSpacing: 2,
                    CenterHorizontally: true,
                    CenterVertically: true),
                new HighlightedLyricsRenderOptions(
                    StartRow: 4,
                    StartColumn: 4,
                    LineSpacing: 2,
                    BackgroundColor: 0,
                    BaseTextColor: 15,
                    HighlightTextColor: 12,
                    ClearScreenBeforeRender: true),
                SongMetadata: new KaraokeSongMetadata("Preview Song", "Preview Artist"),
                IntroOptions: introOptions),
            ExportOptions: null);
    }

    private static string HashPixels(byte[] pixels) => Convert.ToHexString(SHA256.HashData(pixels));
}
