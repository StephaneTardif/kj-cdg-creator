using KJCDGCreator.Core.Lyrics;
using KJCDGCreator.Core.Timing;

namespace KJCDGCreator.Core.Rendering;

public static class ActivePageSelector
{
    public static PageSelectionResult SelectPage(LyricsDocument lyricsDocument, TimingDocument timing, TimeSpan playbackTime)
    {
        ArgumentNullException.ThrowIfNull(lyricsDocument);
        ArgumentNullException.ThrowIfNull(timing);

        if (lyricsDocument.Pages.Count == 0)
        {
            return new PageSelectionResult(PageIndex: -1, HasActivePage: false);
        }

        var pageFirstTimestamps = GetPageFirstTimestamps(lyricsDocument, timing);
        var firstTimedPage = pageFirstTimestamps.FirstOrDefault(entry => entry.FirstTimestamp.HasValue);

        if (firstTimedPage.FirstTimestamp is null || playbackTime < firstTimedPage.FirstTimestamp.Value)
        {
            return new PageSelectionResult(PageIndex: 0, HasActivePage: true);
        }

        var activePageIndex = 0;

        foreach (var page in pageFirstTimestamps)
        {
            if (page.FirstTimestamp is TimeSpan firstTimestamp && firstTimestamp <= playbackTime)
            {
                activePageIndex = page.PageIndex;
            }
        }

        return new PageSelectionResult(activePageIndex, HasActivePage: true);
    }

    private static IReadOnlyList<(int PageIndex, TimeSpan? FirstTimestamp)> GetPageFirstTimestamps(
        LyricsDocument lyricsDocument,
        TimingDocument timing)
    {
        var results = new List<(int PageIndex, TimeSpan? FirstTimestamp)>(lyricsDocument.Pages.Count);
        var unitOffset = 0;

        for (var pageIndex = 0; pageIndex < lyricsDocument.Pages.Count; pageIndex++)
        {
            var page = lyricsDocument.Pages[pageIndex];
            var pageUnitCount = page.Lines.Sum(line => line.Units.Count);
            TimeSpan? firstTimestamp = null;

            foreach (var unit in timing.Units)
            {
                if (unit.UnitIndex < unitOffset || unit.UnitIndex >= unitOffset + pageUnitCount)
                {
                    continue;
                }

                if (unit.Timestamp.HasValue)
                {
                    firstTimestamp = unit.Timestamp.Value;
                    break;
                }
            }

            results.Add((pageIndex, firstTimestamp));
            unitOffset += pageUnitCount;
        }

        return results;
    }
}
