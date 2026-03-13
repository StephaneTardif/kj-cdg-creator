using KJCDGCreator.Core.Cdg;
using KJCDGCreator.Core.Lyrics;

namespace KJCDGCreator.Core.Rendering;

public static class PageLayoutEngine
{
    public static RenderedPageLayout LayoutPage(LyricsPage page, int pageIndex, PageLayoutOptions options)
    {
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(options);

        var startRow = options.CenterVertically
            ? CalculateCenteredStartRow(page.Lines.Count, options.LineSpacing)
            : options.StartRow;

        var lines = new List<RenderedLineLayout>(page.Lines.Count);

        for (var lineIndex = 0; lineIndex < page.Lines.Count; lineIndex++)
        {
            var line = page.Lines[lineIndex];
            var row = Math.Clamp(startRow + (lineIndex * options.LineSpacing), 0, CdgScreenBuffer.Rows - 1);
            var column = options.CenterHorizontally
                ? CalculateCenteredColumn(line.DisplayText.Length)
                : Math.Clamp(options.StartColumn, 0, CdgScreenBuffer.Columns - 1);

            lines.Add(new RenderedLineLayout(line.DisplayText, row, column));
        }

        return new RenderedPageLayout(pageIndex, lines);
    }

    private static int CalculateCenteredStartRow(int lineCount, int lineSpacing)
    {
        if (lineCount <= 0)
        {
            return 0;
        }

        var totalHeight = 1 + ((lineCount - 1) * lineSpacing);
        return Math.Max(0, (CdgScreenBuffer.Rows - totalHeight) / 2);
    }

    private static int CalculateCenteredColumn(int lineLength)
    {
        if (lineLength >= CdgScreenBuffer.Columns)
        {
            return 0;
        }

        return Math.Max(0, (CdgScreenBuffer.Columns - lineLength) / 2);
    }
}
