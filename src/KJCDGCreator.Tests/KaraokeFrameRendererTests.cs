using KJCDGCreator.Core.Cdg;
using KJCDGCreator.Core.Lyrics;
using KJCDGCreator.Core.Rendering;
using KJCDGCreator.Core.Timing;

namespace KJCDGCreator.Tests;

public sealed class KaraokeFrameRendererTests
{
    [Fact]
    public void RenderFrame_BeforeFirstLyricWithIntroEnabled_RendersIntroScreen()
    {
        var (lyrics, timing) = CreateLyricsAndTiming("Be|cause I'm hap|py");
        timing.AssignTimestamp(0, TimeSpan.FromSeconds(3));
        var screen = CreateScreen();

        var result = KaraokeFrameRenderer.RenderFrame(
            lyrics,
            timing,
            TimeSpan.FromSeconds(1),
            screen,
            CreateOptions() with
            {
                SongMetadata = new KaraokeSongMetadata("MY SONG", "THE ARTIST"),
                IntroOptions = CreateIntroOptions()
            });

        Assert.True(result.HasContent);
        Assert.Equal(-1, result.PageIndex);
        Assert.Equal((byte)10, screen.GetTile(2, 0).ForegroundColor);
        Assert.Equal((byte)14, screen.GetTile(4, 0).ForegroundColor);
    }

    [Fact]
    public void RenderFrame_BeforeFirstTimestamp_RendersFirstPage()
    {
        var (lyrics, timing) = CreateLyricsAndTiming("First| page\n\nSecond| page");
        timing.AssignTimestamp(0, TimeSpan.FromSeconds(1));
        timing.AssignTimestamp(1, TimeSpan.FromSeconds(2));
        timing.AssignTimestamp(2, TimeSpan.FromSeconds(3));
        timing.AssignTimestamp(3, TimeSpan.FromSeconds(4));
        var screen = CreateScreen();

        var result = KaraokeFrameRenderer.RenderFrame(lyrics, timing, TimeSpan.FromSeconds(0.5), screen, CreateOptions());

        Assert.True(result.HasContent);
        Assert.Equal(0, result.PageIndex);
        Assert.Equal((byte)15, screen.GetTile(2, 3).ForegroundColor);
    }

    [Fact]
    public void RenderFrame_AfterFirstTimestamp_RendersHighlightedContent()
    {
        var (lyrics, timing) = CreateLyricsAndTiming("Be|cause I'm hap|py");
        timing.AssignTimestamp(0, TimeSpan.FromSeconds(1));
        timing.AssignTimestamp(1, TimeSpan.FromSeconds(2));
        timing.AssignTimestamp(2, TimeSpan.FromSeconds(3));
        timing.AssignTimestamp(3, TimeSpan.FromSeconds(4));
        timing.AssignTimestamp(4, TimeSpan.FromSeconds(5));
        var screen = CreateScreen();

        var result = KaraokeFrameRenderer.RenderFrame(lyrics, timing, TimeSpan.FromSeconds(1), screen, CreateOptions());

        Assert.True(result.HasContent);
        Assert.Equal(0, result.PageIndex);
        Assert.Equal((byte)12, screen.GetTile(2, 3).ForegroundColor);
    }

    [Fact]
    public void RenderFrame_AfterPageAdvancement_RendersLaterPage()
    {
        var (lyrics, timing) = CreateLyricsAndTiming("First| page\n\nSecond| page");
        timing.AssignTimestamp(0, TimeSpan.FromSeconds(1));
        timing.AssignTimestamp(1, TimeSpan.FromSeconds(2));
        timing.AssignTimestamp(2, TimeSpan.FromSeconds(3));
        timing.AssignTimestamp(3, TimeSpan.FromSeconds(4));
        var screen = CreateScreen();

        var result = KaraokeFrameRenderer.RenderFrame(lyrics, timing, TimeSpan.FromSeconds(3.5), screen, CreateOptions());

        Assert.Equal(1, result.PageIndex);
        Assert.Equal((byte)12, screen.GetTile(2, 3).ForegroundColor);
    }

    [Fact]
    public void RenderFrame_HandlesMissingTimestampsSafely()
    {
        var (lyrics, timing) = CreateLyricsAndTiming("First| page\n\nSecond| page");
        timing.AssignTimestamp(0, TimeSpan.FromSeconds(1));
        var screen = CreateScreen();

        var exception = Record.Exception(() =>
            KaraokeFrameRenderer.RenderFrame(lyrics, timing, TimeSpan.FromSeconds(10), screen, CreateOptions()));

        Assert.Null(exception);
    }

    [Fact]
    public void RenderFrame_EmptyDocument_IsSafe()
    {
        var lyrics = new LyricsDocument(Array.Empty<LyricsPage>());
        var timing = new TimingDocument(Array.Empty<TimedUnit>());
        var screen = CreateScreen();

        var result = KaraokeFrameRenderer.RenderFrame(lyrics, timing, TimeSpan.Zero, screen, CreateOptions());

        Assert.False(result.HasContent);
        Assert.Equal(-1, result.PageIndex);
    }

    [Fact]
    public void RenderFrame_IsDeterministicForSameInputs()
    {
        var (lyrics, timing) = CreateLyricsAndTiming("Be|cause I'm hap|py");
        timing.AssignTimestamp(0, TimeSpan.FromSeconds(1));
        timing.AssignTimestamp(1, TimeSpan.FromSeconds(2));
        timing.AssignTimestamp(2, TimeSpan.FromSeconds(3));
        timing.AssignTimestamp(3, TimeSpan.FromSeconds(4));
        timing.AssignTimestamp(4, TimeSpan.FromSeconds(5));
        var screenA = CreateScreen();
        var screenB = CreateScreen();

        KaraokeFrameRenderer.RenderFrame(lyrics, timing, TimeSpan.FromSeconds(2), screenA, CreateOptions());
        KaraokeFrameRenderer.RenderFrame(lyrics, timing, TimeSpan.FromSeconds(2), screenB, CreateOptions());

        var packetsA = CdgScreenBufferRenderer.RenderFullScreen(screenA, 0).SelectMany(packet => packet.ToBytes()).ToArray();
        var packetsB = CdgScreenBufferRenderer.RenderFullScreen(screenB, 0).SelectMany(packet => packet.ToBytes()).ToArray();

        Assert.Equal(packetsA, packetsB);
    }

    [Fact]
    public void RenderFrame_AfterIntroEnds_RendersLyrics()
    {
        var (lyrics, timing) = CreateLyricsAndTiming("Be|cause I'm hap|py");
        timing.AssignTimestamp(0, TimeSpan.FromSeconds(3));
        timing.AssignTimestamp(1, TimeSpan.FromSeconds(4));
        var screen = CreateScreen();

        var result = KaraokeFrameRenderer.RenderFrame(
            lyrics,
            timing,
            TimeSpan.FromSeconds(3.1),
            screen,
            CreateOptions() with
            {
                SongMetadata = new KaraokeSongMetadata("MY SONG", "THE ARTIST"),
                IntroOptions = CreateIntroOptions()
            });

        Assert.True(result.HasContent);
        Assert.Equal(0, result.PageIndex);
        Assert.Equal((byte)12, screen.GetTile(2, 3).ForegroundColor);
    }

    [Fact]
    public void RenderFrame_FixedIntroDurationWorksWithoutLyrics()
    {
        var lyrics = new LyricsDocument(Array.Empty<LyricsPage>());
        var timing = new TimingDocument(Array.Empty<TimedUnit>());
        var screen = CreateScreen();

        var introResult = KaraokeFrameRenderer.RenderFrame(
            lyrics,
            timing,
            TimeSpan.FromSeconds(1),
            screen,
            CreateOptions() with
            {
                SongMetadata = new KaraokeSongMetadata("TITLE", "ARTIST"),
                IntroOptions = CreateIntroOptions() with { FixedDuration = TimeSpan.FromSeconds(2) }
            });

        var afterResult = KaraokeFrameRenderer.RenderFrame(
            lyrics,
            timing,
            TimeSpan.FromSeconds(3),
            screen,
            CreateOptions() with
            {
                SongMetadata = new KaraokeSongMetadata("TITLE", "ARTIST"),
                IntroOptions = CreateIntroOptions() with { FixedDuration = TimeSpan.FromSeconds(2) }
            });

        Assert.True(introResult.HasContent);
        Assert.False(afterResult.HasContent);
    }

    [Fact]
    public void RenderFrame_FirstLyricTimestampCanExtendIntro()
    {
        var (lyrics, timing) = CreateLyricsAndTiming("Be cause");
        timing.AssignTimestamp(0, TimeSpan.FromSeconds(4));
        timing.AssignTimestamp(1, TimeSpan.FromSeconds(5));
        var screen = CreateScreen();
        var options = CreateOptions() with
        {
            SongMetadata = new KaraokeSongMetadata("TITLE", "ARTIST"),
            IntroOptions = CreateIntroOptions() with
            {
                FixedDuration = TimeSpan.FromSeconds(2),
                UseFirstLyricTimestampWhenLonger = true
            }
        };

        var duringExtendedIntro = KaraokeFrameRenderer.RenderFrame(lyrics, timing, TimeSpan.FromSeconds(3), screen, options);
        var afterExtendedIntro = KaraokeFrameRenderer.RenderFrame(lyrics, timing, TimeSpan.FromSeconds(4.1), screen, options);

        Assert.Equal(-1, duringExtendedIntro.PageIndex);
        Assert.Equal(0, afterExtendedIntro.PageIndex);
    }

    [Fact]
    public void RenderFrame_IntroRenderingIsDeterministic()
    {
        var lyrics = new LyricsDocument(Array.Empty<LyricsPage>());
        var timing = new TimingDocument(Array.Empty<TimedUnit>());
        var screenA = CreateScreen();
        var screenB = CreateScreen();
        var options = CreateOptions() with
        {
            SongMetadata = new KaraokeSongMetadata("TITLE", "ARTIST"),
            IntroOptions = CreateIntroOptions()
        };

        KaraokeFrameRenderer.RenderFrame(lyrics, timing, TimeSpan.FromSeconds(1), screenA, options);
        KaraokeFrameRenderer.RenderFrame(lyrics, timing, TimeSpan.FromSeconds(1), screenB, options);

        var packetsA = CdgScreenBufferRenderer.RenderFullScreen(screenA, 0).SelectMany(packet => packet.ToBytes()).ToArray();
        var packetsB = CdgScreenBufferRenderer.RenderFullScreen(screenB, 0).SelectMany(packet => packet.ToBytes()).ToArray();

        Assert.Equal(packetsA, packetsB);
    }

    private static CdgScreenBuffer CreateScreen() =>
        new(CdgScreenBuffer.CreateBlankTile(backgroundColor: 0));

    private static KaraokeFrameRenderOptions CreateOptions() =>
        new(
            new PageLayoutOptions(
                StartRow: 2,
                StartColumn: 3,
                LineSpacing: 1,
                CenterHorizontally: false,
                CenterVertically: false),
            new HighlightedLyricsRenderOptions(
                StartRow: 2,
                StartColumn: 3,
                LineSpacing: 1,
                BackgroundColor: 0,
                BaseTextColor: 15,
                HighlightTextColor: 12,
                ClearScreenBeforeRender: true));

    private static IntroTitleScreenOptions CreateIntroOptions() =>
        new(
            Enabled: true,
            FixedDuration: TimeSpan.FromSeconds(2),
            UseFirstLyricTimestampWhenLonger: true,
            BackgroundColor: 0,
            TitleColor: 10,
            ArtistColor: 14,
            TitleRow: 2,
            ArtistRow: 4,
            CenterHorizontally: false);

    private static (LyricsDocument Lyrics, TimingDocument Timing) CreateLyricsAndTiming(string text)
    {
        var lyrics = LyricsParser.Parse(text);
        var timing = TimingDocumentBuilder.FromLyrics(lyrics);
        return (lyrics, timing);
    }
}
