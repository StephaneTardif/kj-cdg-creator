using KJCDGCreator.Core.Lyrics;
using KJCDGCreator.Core.Rendering;
using KJCDGCreator.Core.Timing;

namespace KJCDGCreator.Tests;

public sealed class PageCompositionTests
{
    [Fact]
    public void SelectsFirstPage_BeforeTimingStarts()
    {
        var (lyrics, timing) = CreateLyricsAndTiming("First| page\n\nSecond| page");
        timing.AssignTimestamp(0, TimeSpan.FromSeconds(1));
        timing.AssignTimestamp(1, TimeSpan.FromSeconds(2));
        timing.AssignTimestamp(2, TimeSpan.FromSeconds(3));
        timing.AssignTimestamp(3, TimeSpan.FromSeconds(4));

        var selection = ActivePageSelector.SelectPage(lyrics, timing, TimeSpan.FromSeconds(0.5));

        Assert.True(selection.HasActivePage);
        Assert.Equal(0, selection.PageIndex);
    }

    [Fact]
    public void SelectsCorrectPage_AsPlaybackProgresses()
    {
        var (lyrics, timing) = CreateLyricsAndTiming("First| page\n\nSecond| page");
        timing.AssignTimestamp(0, TimeSpan.FromSeconds(1));
        timing.AssignTimestamp(1, TimeSpan.FromSeconds(2));
        timing.AssignTimestamp(2, TimeSpan.FromSeconds(3));
        timing.AssignTimestamp(3, TimeSpan.FromSeconds(4));

        var selection = ActivePageSelector.SelectPage(lyrics, timing, TimeSpan.FromSeconds(3.5));

        Assert.Equal(1, selection.PageIndex);
    }

    [Fact]
    public void HandlesMissingTimestamps_Safely()
    {
        var (lyrics, timing) = CreateLyricsAndTiming("First| page\n\nSecond| page");
        timing.AssignTimestamp(0, TimeSpan.FromSeconds(1));

        var exception = Record.Exception(() =>
            ActivePageSelector.SelectPage(lyrics, timing, TimeSpan.FromSeconds(10)));

        Assert.Null(exception);
        Assert.Equal(0, ActivePageSelector.SelectPage(lyrics, timing, TimeSpan.FromSeconds(10)).PageIndex);
    }

    [Fact]
    public void LeftAlignedLayout_PositionsLinesCorrectly()
    {
        var lyrics = LyricsParser.Parse("Line one\nLine two");
        var layout = PageLayoutEngine.LayoutPage(
            lyrics.Pages[0],
            pageIndex: 0,
            new PageLayoutOptions(
                StartRow: 3,
                StartColumn: 4,
                LineSpacing: 2,
                CenterHorizontally: false,
                CenterVertically: false));

        Assert.Collection(
            layout.Lines,
            line =>
            {
                Assert.Equal("Line one", line.DisplayText);
                Assert.Equal(3, line.Row);
                Assert.Equal(4, line.Column);
            },
            line =>
            {
                Assert.Equal("Line two", line.DisplayText);
                Assert.Equal(5, line.Row);
                Assert.Equal(4, line.Column);
            });
    }

    [Fact]
    public void CenteredLayout_PositionsLinesReasonably()
    {
        var lyrics = LyricsParser.Parse("Hello\nWorld");
        var layout = PageLayoutEngine.LayoutPage(
            lyrics.Pages[0],
            pageIndex: 0,
            new PageLayoutOptions(
                StartRow: 0,
                StartColumn: 0,
                LineSpacing: 1,
                CenterHorizontally: true,
                CenterVertically: true));

        Assert.Equal(2, layout.Lines.Count);
        Assert.All(layout.Lines, line => Assert.InRange(line.Column, 0, 49));
        Assert.All(layout.Lines, line => Assert.InRange(line.Row, 0, 17));
        Assert.True(layout.Lines[0].Column > 0);
        Assert.True(layout.Lines[0].Row >= 0);
    }

    [Fact]
    public void Composer_ReturnsExpectedPageAndLineCount()
    {
        var (lyrics, timing) = CreateLyricsAndTiming("First| page\n\nSecond| page");
        timing.AssignTimestamp(0, TimeSpan.FromSeconds(1));
        timing.AssignTimestamp(1, TimeSpan.FromSeconds(2));
        timing.AssignTimestamp(2, TimeSpan.FromSeconds(3));
        timing.AssignTimestamp(3, TimeSpan.FromSeconds(4));

        var composed = KaraokePageComposer.Compose(
            lyrics,
            timing,
            TimeSpan.FromSeconds(3.5),
            new PageLayoutOptions(
                StartRow: 2,
                StartColumn: 3,
                LineSpacing: 1,
                CenterHorizontally: false,
                CenterVertically: false));

        Assert.True(composed.Selection.HasActivePage);
        Assert.Equal(1, composed.Selection.PageIndex);
        Assert.NotNull(composed.HighlightState);
        Assert.NotNull(composed.Layout);
        Assert.Single(composed.Layout!.Lines);
    }

    private static (LyricsDocument Lyrics, TimingDocument Timing) CreateLyricsAndTiming(string text)
    {
        var lyrics = LyricsParser.Parse(text);
        var timing = TimingDocumentBuilder.FromLyrics(lyrics);
        return (lyrics, timing);
    }
}
