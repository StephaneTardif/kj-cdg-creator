using KJCDGCreator.Core.Cdg;
using KJCDGCreator.Core.Rendering;

namespace KJCDGCreator.Tests;

public sealed class HighlightedLyricsRendererTests
{
    [Fact]
    public void RenderPage_RendersBaseTextForLinesWithNoHighlights()
    {
        var screen = CreateScreen();
        var state = new PageHighlightState(
            new[]
            {
                new LineHighlightState("HELLO", Array.Empty<HighlightRange>(), null)
            });

        HighlightedLyricsRenderer.RenderPage(state, screen, CreateOptions());

        Assert.Equal((byte)5, screen.GetTile(2, 3).ForegroundColor);
        Assert.Equal((byte)5, screen.GetTile(2, 7).ForegroundColor);
    }

    [Fact]
    public void RenderPage_RendersCompletedRangesInHighlightColor()
    {
        var screen = CreateScreen();
        var state = new PageHighlightState(
            new[]
            {
                new LineHighlightState("HELLO", new[] { new HighlightRange(0, 2) }, null)
            });

        HighlightedLyricsRenderer.RenderPage(state, screen, CreateOptions());

        Assert.Equal((byte)12, screen.GetTile(2, 3).ForegroundColor);
        Assert.Equal((byte)12, screen.GetTile(2, 4).ForegroundColor);
        Assert.Equal((byte)5, screen.GetTile(2, 5).ForegroundColor);
    }

    [Fact]
    public void RenderPage_RendersActiveRangeInHighlightColor()
    {
        var screen = CreateScreen();
        var state = new PageHighlightState(
            new[]
            {
                new LineHighlightState("HELLO", Array.Empty<HighlightRange>(), new HighlightRange(2, 2))
            });

        HighlightedLyricsRenderer.RenderPage(state, screen, CreateOptions());

        Assert.Equal((byte)5, screen.GetTile(2, 4).ForegroundColor);
        Assert.Equal((byte)12, screen.GetTile(2, 5).ForegroundColor);
        Assert.Equal((byte)12, screen.GetTile(2, 6).ForegroundColor);
    }

    [Fact]
    public void RenderPage_RendersMultipleLinesAtDifferentRows()
    {
        var screen = CreateScreen();
        var state = new PageHighlightState(
            new[]
            {
                new LineHighlightState("ONE", Array.Empty<HighlightRange>(), null),
                new LineHighlightState("TWO", Array.Empty<HighlightRange>(), null)
            });

        HighlightedLyricsRenderer.RenderPage(
            state,
            screen,
            CreateOptions() with { StartRow = 1, LineSpacing = 2 });

        Assert.Equal((byte)5, screen.GetTile(1, 3).ForegroundColor);
        Assert.Equal((byte)5, screen.GetTile(3, 3).ForegroundColor);
    }

    [Fact]
    public void RenderPage_SafelyHandlesRangesOutsideVisibleBounds()
    {
        var screen = CreateScreen();
        var state = new PageHighlightState(
            new[]
            {
                new LineHighlightState("HELLO", new[] { new HighlightRange(-2, 10) }, new HighlightRange(100, 2))
            });

        var exception = Record.Exception(() =>
            HighlightedLyricsRenderer.RenderPage(
                state,
                screen,
                CreateOptions() with { StartColumn = CdgScreenBuffer.Columns - 2 }));

        Assert.Null(exception);
        Assert.Equal((byte)12, screen.GetTile(2, CdgScreenBuffer.Columns - 2).ForegroundColor);
        Assert.Equal((byte)12, screen.GetTile(2, CdgScreenBuffer.Columns - 1).ForegroundColor);
    }

    [Fact]
    public void RenderPage_DoesNotThrowOnEmptyLinesOrEmptyPages()
    {
        var screen = CreateScreen();
        var emptyPage = new PageHighlightState(Array.Empty<LineHighlightState>());
        var emptyLinePage = new PageHighlightState(
            new[]
            {
                new LineHighlightState(string.Empty, Array.Empty<HighlightRange>(), null)
            });

        var exception = Record.Exception(() =>
        {
            HighlightedLyricsRenderer.RenderPage(emptyPage, screen, CreateOptions());
            HighlightedLyricsRenderer.RenderPage(emptyLinePage, screen, CreateOptions());
        });

        Assert.Null(exception);
    }

    private static CdgScreenBuffer CreateScreen() =>
        new(CdgScreenBuffer.CreateBlankTile(backgroundColor: 0));

    private static HighlightedLyricsRenderOptions CreateOptions() =>
        new(
            StartRow: 2,
            StartColumn: 3,
            LineSpacing: 1,
            BackgroundColor: 0,
            BaseTextColor: 5,
            HighlightTextColor: 12,
            ClearScreenBeforeRender: true);
}
