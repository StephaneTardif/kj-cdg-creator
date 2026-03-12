namespace KJCDGCreator.Core.Lyrics;

public static class LyricsParser
{
    public static LyricsDocument Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new LyricsDocument(Array.Empty<LyricsPage>());
        }

        var normalizedText = text.Replace("\r\n", "\n").Replace('\r', '\n');
        var sourceLines = normalizedText.Split('\n');
        var pages = new List<LyricsPage>();
        var currentLines = new List<LyricsLine>();

        foreach (var sourceLine in sourceLines)
        {
            var trimmedLine = sourceLine.Trim();

            if (trimmedLine.Length == 0)
            {
                if (currentLines.Count > 0)
                {
                    pages.Add(new LyricsPage(currentLines.ToArray()));
                    currentLines = new List<LyricsLine>();
                }

                continue;
            }

            currentLines.Add(ParseLine(trimmedLine));
        }

        if (currentLines.Count > 0)
        {
            pages.Add(new LyricsPage(currentLines.ToArray()));
        }

        return new LyricsDocument(pages);
    }

    private static LyricsLine ParseLine(string line)
    {
        var segments = line.Split('|');
        var units = new List<LyricsUnit>(segments.Length);
        var displayIndex = 0;

        foreach (var segment in segments)
        {
            units.Add(new LyricsUnit(segment, displayIndex, segment.Length));
            displayIndex += segment.Length;
        }

        return new LyricsLine(
            RawText: line,
            DisplayText: string.Concat(segments),
            Units: units);
    }
}
