using KJCDGCreator.Core.Lyrics;

namespace KJCDGCreator.Core.Timing;

public static class TimingDocumentBuilder
{
    public static TimingDocument FromLyrics(LyricsDocument lyrics)
    {
        ArgumentNullException.ThrowIfNull(lyrics);

        var units = new List<TimedUnit>();
        var unitIndex = 0;

        foreach (var page in lyrics.Pages)
        {
            foreach (var line in page.Lines)
            {
                foreach (var unit in line.Units)
                {
                    units.Add(new TimedUnit(unitIndex, unit.Text, Timestamp: null));
                    unitIndex++;
                }
            }
        }

        return new TimingDocument(units);
    }
}
