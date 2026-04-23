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

        var pageTimings = GetPageTimings(lyricsDocument, timing);
        var firstTimedPage = pageTimings.FirstOrDefault(entry => entry.FirstTimestamp.HasValue);

        if (firstTimedPage?.FirstTimestamp is null || playbackTime < firstTimedPage.FirstTimestamp.Value)
        {
            return new PageSelectionResult(PageIndex: 0, HasActivePage: true);
        }

        var activePageIndex = 0;

        foreach (var page in pageTimings)
        {
            if (page.FirstTimestamp is TimeSpan firstTimestamp && firstTimestamp <= playbackTime)
            {
                activePageIndex = page.PageIndex;
            }

            if (page.IsFullyTimed
                && page.LastTimestamp is TimeSpan lastTimestamp
                && lastTimestamp <= playbackTime
                && page.PageIndex < lyricsDocument.Pages.Count - 1)
            {
                activePageIndex = Math.Max(activePageIndex, page.PageIndex + 1);
            }
        }

        return new PageSelectionResult(activePageIndex, HasActivePage: true);
    }

    private static IReadOnlyList<PageTimingInfo> GetPageTimings(
        LyricsDocument lyricsDocument,
        TimingDocument timing)
    {
        var results = new List<PageTimingInfo>(lyricsDocument.Pages.Count);
        var unitOffset = 0;

        for (var pageIndex = 0; pageIndex < lyricsDocument.Pages.Count; pageIndex++)
        {
            var page = lyricsDocument.Pages[pageIndex];
            var pageUnitCount = page.Lines.Sum(line => line.Units.Count);
            var pageUnits = timing.Units
                .Where(unit => unit.UnitIndex >= unitOffset && unit.UnitIndex < unitOffset + pageUnitCount)
                .ToArray();

            var timestamps = pageUnits
                .Where(unit => unit.Timestamp.HasValue)
                .Select(unit => unit.Timestamp!.Value)
                .ToArray();

            results.Add(new PageTimingInfo(
                PageIndex: pageIndex,
                FirstTimestamp: timestamps.Length == 0 ? null : timestamps.Min(),
                LastTimestamp: timestamps.Length == 0 ? null : timestamps.Max(),
                IsFullyTimed: pageUnitCount > 0 && pageUnits.Length == pageUnitCount && pageUnits.All(unit => unit.Timestamp.HasValue)));
            unitOffset += pageUnitCount;
        }

        return results;
    }

    private sealed record PageTimingInfo(
        int PageIndex,
        TimeSpan? FirstTimestamp,
        TimeSpan? LastTimestamp,
        bool IsFullyTimed);
}
