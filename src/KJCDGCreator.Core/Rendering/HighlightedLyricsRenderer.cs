using KJCDGCreator.Core.Cdg;

namespace KJCDGCreator.Core.Rendering;

public static class HighlightedLyricsRenderer
{
    public static void RenderPage(
        PageHighlightState pageState,
        CdgScreenBuffer screenBuffer,
        HighlightedLyricsRenderOptions options)
    {
        ArgumentNullException.ThrowIfNull(pageState);
        ArgumentNullException.ThrowIfNull(screenBuffer);
        ArgumentNullException.ThrowIfNull(options);

        if (options.ClearScreenBeforeRender)
        {
            screenBuffer.Clear(CdgScreenBuffer.CreateBlankTile(options.BackgroundColor));
        }

        var renderedLines = pageState.Lines.Select((line, lineIndex) => new RenderedLineLayout(
            line.DisplayText,
            options.StartRow + (lineIndex * options.LineSpacing),
            options.StartColumn)).ToArray();

        RenderPage(pageState, new RenderedPageLayout(PageIndex: 0, Lines: renderedLines), screenBuffer, options, clearScreen: false);
    }

    public static void RenderPage(
        PageHighlightState pageState,
        RenderedPageLayout layout,
        CdgScreenBuffer screenBuffer,
        HighlightedLyricsRenderOptions options)
    {
        ArgumentNullException.ThrowIfNull(pageState);
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(screenBuffer);
        ArgumentNullException.ThrowIfNull(options);

        RenderPage(pageState, layout, screenBuffer, options, clearScreen: options.ClearScreenBeforeRender);
    }

    private static void RenderPage(
        PageHighlightState pageState,
        RenderedPageLayout layout,
        CdgScreenBuffer screenBuffer,
        HighlightedLyricsRenderOptions options,
        bool clearScreen)
    {
        if (clearScreen)
        {
            screenBuffer.Clear(CdgScreenBuffer.CreateBlankTile(options.BackgroundColor));
        }

        var fontRenderer = new BitmapFontRenderer(screenBuffer, backgroundColor: options.BackgroundColor);
        var count = Math.Min(pageState.Lines.Count, layout.Lines.Count);

        for (var lineIndex = 0; lineIndex < count; lineIndex++)
        {
            var renderedLine = layout.Lines[lineIndex];
            var row = renderedLine.Row;
            if (row < 0 || row >= CdgScreenBuffer.Rows)
            {
                continue;
            }

            RenderLine(pageState.Lines[lineIndex], renderedLine.Column, row, fontRenderer, options);
        }
    }

    private static void RenderLine(
        LineHighlightState lineState,
        int column,
        int row,
        BitmapFontRenderer fontRenderer,
        HighlightedLyricsRenderOptions options)
    {
        fontRenderer.RenderText(lineState.DisplayText, column, row, options.BaseTextColor);

        foreach (var range in lineState.CompletedRanges)
        {
            RenderRange(lineState.DisplayText, range, column, row, fontRenderer, options);
        }

        if (lineState.ActiveRange is not null)
        {
            RenderRange(lineState.DisplayText, lineState.ActiveRange, column, row, fontRenderer, options);
        }
    }

    private static void RenderRange(
        string displayText,
        HighlightRange range,
        int lineColumn,
        int row,
        BitmapFontRenderer fontRenderer,
        HighlightedLyricsRenderOptions options)
    {
        var clipped = ClipRangeToLine(displayText, range);
        if (clipped is null)
        {
            return;
        }

        var text = displayText.Substring(clipped.StartIndex, clipped.Length);
        var column = lineColumn + clipped.StartIndex;
        fontRenderer.RenderText(text, column, row, options.HighlightTextColor);
    }

    private static HighlightRange? ClipRangeToLine(string displayText, HighlightRange range)
    {
        if (string.IsNullOrEmpty(displayText) || range.Length <= 0)
        {
            return null;
        }

        var start = Math.Max(0, range.StartIndex);
        var end = Math.Min(displayText.Length, range.StartIndex + range.Length);

        if (start >= end)
        {
            return null;
        }

        return new HighlightRange(start, end - start);
    }
}
