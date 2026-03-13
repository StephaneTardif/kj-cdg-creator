using KJCDGCreator.Core.Lyrics;
using KJCDGCreator.Core.Timing;

namespace KJCDGCreator.Core.Rendering;

public static class HighlightProgressionEngine
{
    public static PageHighlightState GetPageState(
        LyricsDocument lyricsDocument,
        int pageIndex,
        TimingDocument timing,
        TimeSpan playbackTime)
    {
        ArgumentNullException.ThrowIfNull(lyricsDocument);
        ArgumentNullException.ThrowIfNull(timing);

        if (pageIndex < 0 || pageIndex >= lyricsDocument.Pages.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(pageIndex));
        }

        var page = lyricsDocument.Pages[pageIndex];
        var pageStartUnitIndex = GetPageStartUnitIndex(lyricsDocument, pageIndex);
        var latestReachedUnitIndex = GetLatestReachedUnitIndex(timing, pageStartUnitIndex, CountUnits(page), playbackTime);

        var lines = new List<LineHighlightState>(page.Lines.Count);
        var globalUnitIndex = pageStartUnitIndex;

        foreach (var line in page.Lines)
        {
            var completedRanges = new List<HighlightRange>();
            HighlightRange? activeRange = null;

            foreach (var unit in line.Units)
            {
                var timedUnit = FindTimedUnit(timing, globalUnitIndex);
                var unitRange = new HighlightRange(unit.DisplayStartIndex, unit.DisplayLength);

                if (timedUnit?.Timestamp is TimeSpan timestamp && timestamp <= playbackTime)
                {
                    if (latestReachedUnitIndex == globalUnitIndex)
                    {
                        activeRange = unitRange;
                    }
                    else
                    {
                        completedRanges.Add(unitRange);
                    }
                }

                globalUnitIndex++;
            }

            lines.Add(new LineHighlightState(
                line.DisplayText,
                MergeAdjacentRanges(completedRanges),
                activeRange));
        }

        return new PageHighlightState(lines);
    }

    private static int GetPageStartUnitIndex(LyricsDocument lyricsDocument, int pageIndex)
    {
        var start = 0;

        for (var index = 0; index < pageIndex; index++)
        {
            start += CountUnits(lyricsDocument.Pages[index]);
        }

        return start;
    }

    private static int CountUnits(LyricsPage page) =>
        page.Lines.Sum(line => line.Units.Count);

    private static int? GetLatestReachedUnitIndex(TimingDocument timing, int pageStartUnitIndex, int pageUnitCount, TimeSpan playbackTime)
    {
        var pageEndUnitIndex = pageStartUnitIndex + pageUnitCount;
        int? latest = null;

        foreach (var unit in timing.Units)
        {
            if (unit.UnitIndex < pageStartUnitIndex || unit.UnitIndex >= pageEndUnitIndex)
            {
                continue;
            }

            if (unit.Timestamp is TimeSpan timestamp && timestamp <= playbackTime)
            {
                latest = unit.UnitIndex;
            }
        }

        return latest;
    }

    private static TimedUnit? FindTimedUnit(TimingDocument timing, int unitIndex) =>
        timing.Units.FirstOrDefault(unit => unit.UnitIndex == unitIndex);

    private static IReadOnlyList<HighlightRange> MergeAdjacentRanges(IReadOnlyList<HighlightRange> ranges)
    {
        if (ranges.Count <= 1)
        {
            return ranges;
        }

        var ordered = ranges.OrderBy(range => range.StartIndex).ToList();
        var merged = new List<HighlightRange> { ordered[0] };

        for (var index = 1; index < ordered.Count; index++)
        {
            var current = ordered[index];
            var previous = merged[^1];

            if (previous.StartIndex + previous.Length == current.StartIndex)
            {
                merged[^1] = previous with { Length = previous.Length + current.Length };
                continue;
            }

            merged.Add(current);
        }

        return merged;
    }
}
