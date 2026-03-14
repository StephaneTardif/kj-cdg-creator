using System.Text;

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
        var units = new List<LyricsUnit>();
        var displayText = line.Replace("|", string.Empty);

        var tokenStartIndex = -1;
        var token = new StringBuilder();
        var displayIndex = 0;

        foreach (var character in line)
        {
            if (character == '|')
            {
                FlushToken(units, token, ref tokenStartIndex);
                continue;
            }

            if (char.IsWhiteSpace(character))
            {
                FlushToken(units, token, ref tokenStartIndex);
                displayIndex++;
                continue;
            }

            if (tokenStartIndex < 0)
            {
                tokenStartIndex = displayIndex;
            }

            token.Append(character);
            displayIndex++;
        }

        FlushToken(units, token, ref tokenStartIndex);

        return new LyricsLine(
            RawText: line,
            DisplayText: displayText,
            Units: units);
    }

    private static void FlushToken(List<LyricsUnit> units, StringBuilder token, ref int tokenStartIndex)
    {
        if (token.Length == 0 || tokenStartIndex < 0)
        {
            token.Clear();
            tokenStartIndex = -1;
            return;
        }

        var text = token.ToString();
        units.Add(new LyricsUnit(text, tokenStartIndex, text.Length));
        token.Clear();
        tokenStartIndex = -1;
    }
}
