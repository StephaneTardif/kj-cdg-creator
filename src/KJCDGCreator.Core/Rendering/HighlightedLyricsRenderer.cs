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

        var fontRenderer = new BitmapFontRenderer(screenBuffer, backgroundColor: options.BackgroundColor);

        for (var lineIndex = 0; lineIndex < pageState.Lines.Count; lineIndex++)
        {
            var row = options.StartRow + (lineIndex * options.LineSpacing);
            if (row < 0 || row >= CdgScreenBuffer.Rows)
            {
                continue;
            }

            RenderLine(pageState.Lines[lineIndex], row, fontRenderer, options);
        }
    }

    private static void RenderLine(
        LineHighlightState lineState,
        int row,
        BitmapFontRenderer fontRenderer,
        HighlightedLyricsRenderOptions options)
    {
        fontRenderer.RenderText(lineState.DisplayText, options.StartColumn, row, options.BaseTextColor);

        foreach (var range in lineState.CompletedRanges)
        {
            RenderRange(lineState.DisplayText, range, row, fontRenderer, options);
        }

        if (lineState.ActiveRange is not null)
        {
            RenderRange(lineState.DisplayText, lineState.ActiveRange, row, fontRenderer, options);
        }
    }

    private static void RenderRange(
        string displayText,
        HighlightRange range,
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
        var column = options.StartColumn + clipped.StartIndex;
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
