using KJCDGCreator.Core.Lyrics;
using KJCDGCreator.Core.Timing;

namespace KJCDGCreator.Tests;

public sealed class TapTimingSessionTests
{
    [Fact]
    public void Session_StartsAtFirstUnit()
    {
        var session = CreateSession("Be|cause");

        Assert.False(session.IsComplete);
        Assert.Equal(0, session.CurrentUnitIndex);
        Assert.Equal("Be", session.GetCurrentUnit()!.Text);
    }

    [Fact]
    public void Record_AdvancesThroughUnits()
    {
        var session = CreateSession("Be|cause");

        session.RecordTimestamp(TimeSpan.FromSeconds(1));

        Assert.Equal(1, session.CurrentUnitIndex);
        Assert.Equal("cause", session.GetCurrentUnit()!.Text);
    }

    [Fact]
    public void CompleteState_WorksAfterFinalUnitIsTimed()
    {
        var session = CreateSession("Be|cause");

        session.RecordTimestamp(TimeSpan.FromSeconds(1));
        session.RecordTimestamp(TimeSpan.FromSeconds(2));

        Assert.True(session.IsComplete);
        Assert.Null(session.GetCurrentUnit());
        Assert.Equal(2, session.CurrentUnitIndex);
    }

    [Fact]
    public void Undo_RestoresPreviousCurrentUnit()
    {
        var session = CreateSession("Be|cause");

        session.RecordTimestamp(TimeSpan.FromSeconds(1));
        session.RecordTimestamp(TimeSpan.FromSeconds(2));
        session.Undo();

        Assert.False(session.IsComplete);
        Assert.True(session.CanUndo);
        Assert.Equal(1, session.CurrentUnitIndex);
        Assert.Equal("cause", session.GetCurrentUnit()!.Text);
    }

    [Fact]
    public void Reset_ClearsProgress()
    {
        var timing = TimingDocumentBuilder.FromLyrics(LyricsParser.Parse("Be|cause"));
        var session = new TapTimingSession(timing);

        session.RecordTimestamp(TimeSpan.FromSeconds(1));
        session.RecordTimestamp(TimeSpan.FromSeconds(2));
        session.Reset();

        Assert.Equal(0, session.CurrentUnitIndex);
        Assert.False(session.IsComplete);
        Assert.False(session.CanUndo);
        Assert.All(timing.Units, unit => Assert.Null(unit.Timestamp));
    }

    [Fact]
    public void DecreasingTimestamps_AreRejected()
    {
        var session = CreateSession("Be|cause");

        session.RecordTimestamp(TimeSpan.FromSeconds(2));

        var exception = Assert.Throws<InvalidOperationException>(() => session.RecordTimestamp(TimeSpan.FromSeconds(1)));

        Assert.Contains("earlier than the previous recorded timestamp", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RecordingAfterCompletion_IsRejected()
    {
        var session = CreateSession("Be|cause");

        session.RecordTimestamp(TimeSpan.FromSeconds(1));
        session.RecordTimestamp(TimeSpan.FromSeconds(2));

        var exception = Assert.Throws<InvalidOperationException>(() => session.RecordTimestamp(TimeSpan.FromSeconds(3)));

        Assert.Contains("session is complete", exception.Message, StringComparison.Ordinal);
    }

    private static TapTimingSession CreateSession(string lyricsText)
    {
        var lyrics = LyricsParser.Parse(lyricsText);
        var timing = TimingDocumentBuilder.FromLyrics(lyrics);
        return new TapTimingSession(timing);
    }
}
