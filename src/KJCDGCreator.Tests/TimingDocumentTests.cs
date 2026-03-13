using KJCDGCreator.Core.Lyrics;
using KJCDGCreator.Core.Timing;

namespace KJCDGCreator.Tests;

public sealed class TimingDocumentTests
{
    [Fact]
    public void Builder_CreatesOneTimedUnitPerLyricsUnit()
    {
        var lyrics = LyricsParser.Parse("Be|cause\nhap|py");

        var timing = TimingDocumentBuilder.FromLyrics(lyrics);

        Assert.Equal(4, timing.Units.Count);
        Assert.All(timing.Units, unit => Assert.Null(unit.Timestamp));
    }

    [Fact]
    public void Builder_FlattensUnitsInReadingOrder()
    {
        var lyrics = LyricsParser.Parse("Be|cause\nhap|py");

        var timing = TimingDocumentBuilder.FromLyrics(lyrics);

        Assert.Collection(
            timing.Units,
            unit => Assert.Equal("Be", unit.Text),
            unit => Assert.Equal("cause", unit.Text),
            unit => Assert.Equal("hap", unit.Text),
            unit => Assert.Equal("py", unit.Text));
    }

    [Fact]
    public void AssignTimestamp_UpdatesTimedUnitAndCounts()
    {
        var timing = TimingDocumentBuilder.FromLyrics(LyricsParser.Parse("Be|cause"));

        timing.AssignTimestamp(1, TimeSpan.FromSeconds(2));

        Assert.Equal(TimeSpan.FromSeconds(2), timing.Units[1].Timestamp);
        Assert.Equal(1, timing.TimedCount);
        Assert.Equal(1, timing.UntimedCount);
    }

    [Fact]
    public void Validation_PassesForIncreasingTimestamps()
    {
        var timing = TimingDocumentBuilder.FromLyrics(LyricsParser.Parse("Be|cause"));
        timing.AssignTimestamp(0, TimeSpan.FromSeconds(1));
        timing.AssignTimestamp(1, TimeSpan.FromSeconds(2));

        var result = TimingValidator.Validate(timing, requireAllTimestamps: true);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validation_FailsForDecreasingTimestamps()
    {
        var timing = TimingDocumentBuilder.FromLyrics(LyricsParser.Parse("Be|cause"));
        timing.AssignTimestamp(0, TimeSpan.FromSeconds(2));
        timing.AssignTimestamp(1, TimeSpan.FromSeconds(1));

        var result = TimingValidator.Validate(timing, requireAllTimestamps: true);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("earlier than the previous timed unit", StringComparison.Ordinal));
    }

    [Fact]
    public void Validation_DetectsMissingTimestampsWhenRequired()
    {
        var timing = TimingDocumentBuilder.FromLyrics(LyricsParser.Parse("Be|cause"));
        timing.AssignTimestamp(0, TimeSpan.FromSeconds(1));

        var result = TimingValidator.Validate(timing, requireAllTimestamps: true);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("missing a timestamp", StringComparison.Ordinal));
    }

    [Fact]
    public void Validation_AllowsMissingTimestampsWhenNotRequired()
    {
        var timing = TimingDocumentBuilder.FromLyrics(LyricsParser.Parse("Be|cause"));
        timing.AssignTimestamp(0, TimeSpan.FromSeconds(1));

        var result = TimingValidator.Validate(timing, requireAllTimestamps: false);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
