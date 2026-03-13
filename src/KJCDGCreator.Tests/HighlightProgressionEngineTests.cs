using KJCDGCreator.Core.Lyrics;
using KJCDGCreator.Core.Rendering;
using KJCDGCreator.Core.Timing;

namespace KJCDGCreator.Tests;

public sealed class HighlightProgressionEngineTests
{
    [Fact]
    public void NoHighlight_BeforeFirstTimestamp()
    {
        var (lyrics, timing) = CreateLyricsAndTiming("Be|cause I'm hap|py");
        timing.AssignTimestamp(0, TimeSpan.FromSeconds(1));
        timing.AssignTimestamp(1, TimeSpan.FromSeconds(2));
        timing.AssignTimestamp(2, TimeSpan.FromSeconds(3));

        var state = HighlightProgressionEngine.GetPageState(lyrics, 0, timing, TimeSpan.FromSeconds(0.5));
        var line = Assert.Single(state.Lines);

        Assert.Empty(line.CompletedRanges);
        Assert.Null(line.ActiveRange);
    }

    [Fact]
    public void FirstUnit_BecomesActiveAtFirstTimestamp()
    {
        var (lyrics, timing) = CreateLyricsAndTiming("Be|cause I'm hap|py");
        timing.AssignTimestamp(0, TimeSpan.FromSeconds(1));

        var state = HighlightProgressionEngine.GetPageState(lyrics, 0, timing, TimeSpan.FromSeconds(1));
        var line = Assert.Single(state.Lines);

        Assert.Empty(line.CompletedRanges);
        Assert.Equal(new HighlightRange(0, 2), line.ActiveRange);
    }

    [Fact]
    public void MultipleCompletedUnits_OnOneLine_AreMergedWhenAdjacent()
    {
        var (lyrics, timing) = CreateLyricsAndTiming("Be|cause I'm hap|py");
        timing.AssignTimestamp(0, TimeSpan.FromSeconds(1));
        timing.AssignTimestamp(1, TimeSpan.FromSeconds(2));
        timing.AssignTimestamp(2, TimeSpan.FromSeconds(3));

        var state = HighlightProgressionEngine.GetPageState(lyrics, 0, timing, TimeSpan.FromSeconds(3));
        var line = Assert.Single(state.Lines);

        Assert.Collection(
            line.CompletedRanges,
            range => Assert.Equal(new HighlightRange(0, 15), range));
        Assert.Equal(new HighlightRange(15, 2), line.ActiveRange);
    }

    [Fact]
    public void Progression_AcrossMultipleLines_PreservesLineStructure()
    {
        var (lyrics, timing) = CreateLyricsAndTiming("Be|cause I'm hap|py\nClap a|long");
        timing.AssignTimestamp(0, TimeSpan.FromSeconds(1));
        timing.AssignTimestamp(1, TimeSpan.FromSeconds(2));
        timing.AssignTimestamp(2, TimeSpan.FromSeconds(3));
        timing.AssignTimestamp(3, TimeSpan.FromSeconds(4));
        timing.AssignTimestamp(4, TimeSpan.FromSeconds(5));

        var state = HighlightProgressionEngine.GetPageState(lyrics, 0, timing, TimeSpan.FromSeconds(4));

        Assert.Equal(2, state.Lines.Count);
        Assert.Collection(
            state.Lines[0].CompletedRanges,
            range => Assert.Equal(new HighlightRange(0, 17), range));
        Assert.Null(state.Lines[0].ActiveRange);
        Assert.Equal(new HighlightRange(0, 6), state.Lines[1].ActiveRange);
    }

    [Fact]
    public void MissingTimestamps_AreIgnoredGracefully()
    {
        var (lyrics, timing) = CreateLyricsAndTiming("Be|cause I'm hap|py");
        timing.AssignTimestamp(0, TimeSpan.FromSeconds(1));
        timing.AssignTimestamp(2, TimeSpan.FromSeconds(3));

        var state = HighlightProgressionEngine.GetPageState(lyrics, 0, timing, TimeSpan.FromSeconds(3));
        var line = Assert.Single(state.Lines);

        Assert.Collection(
            line.CompletedRanges,
            range => Assert.Equal(new HighlightRange(0, 2), range));
        Assert.Equal(new HighlightRange(15, 2), line.ActiveRange);
    }

    [Fact]
    public void CompletedRanges_MergeAdjacentUnits_ButNotSeparatedUnits()
    {
        var (lyrics, timing) = CreateLyricsAndTiming("Hi| there| friend");
        timing.AssignTimestamp(0, TimeSpan.FromSeconds(1));
        timing.AssignTimestamp(1, TimeSpan.FromSeconds(2));
        timing.AssignTimestamp(2, TimeSpan.FromSeconds(3));

        var state = HighlightProgressionEngine.GetPageState(lyrics, 0, timing, TimeSpan.FromSeconds(2));
        var line = Assert.Single(state.Lines);

        Assert.Collection(
            line.CompletedRanges,
            range => Assert.Equal(new HighlightRange(0, 2), range));
        Assert.Equal(new HighlightRange(2, 6), line.ActiveRange);
    }

    private static (LyricsDocument Lyrics, TimingDocument Timing) CreateLyricsAndTiming(string text)
    {
        var lyrics = LyricsParser.Parse(text);
        var timing = TimingDocumentBuilder.FromLyrics(lyrics);
        return (lyrics, timing);
    }
}
