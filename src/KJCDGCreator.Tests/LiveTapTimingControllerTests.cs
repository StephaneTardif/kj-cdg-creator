using KJCDGCreator.Audio.Timing;
using KJCDGCreator.Core.Lyrics;
using KJCDGCreator.Core.Timing;

namespace KJCDGCreator.Tests;

public sealed class LiveTapTimingControllerTests
{
    [Fact]
    public void Recording_UsesCurrentTimeFromAudioSource()
    {
        var controller = CreateController(out var source, out var timing);

        source.Play();
        source.Advance(TimeSpan.FromSeconds(2));
        var status = controller.HandleCommand(LiveTapTimingCommand.RecordTap);

        Assert.Equal(TimeSpan.FromSeconds(2), timing.Units[0].Timestamp);
        Assert.Contains("Recorded 00:00:02.000", status.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Undo_MovesBackCorrectly()
    {
        var controller = CreateController(out var source, out var timing);

        source.Play();
        source.Advance(TimeSpan.FromSeconds(1));
        controller.HandleCommand(LiveTapTimingCommand.RecordTap);
        var status = controller.HandleCommand(LiveTapTimingCommand.Undo);

        Assert.Null(timing.Units[0].Timestamp);
        Assert.Equal(0, status.CurrentUnitIndex);
        Assert.Equal("Be", status.CurrentUnitText);
    }

    [Fact]
    public void Reset_ClearsProgress()
    {
        var controller = CreateController(out var source, out var timing);

        source.Play();
        source.Advance(TimeSpan.FromSeconds(1));
        controller.HandleCommand(LiveTapTimingCommand.RecordTap);
        var status = controller.HandleCommand(LiveTapTimingCommand.Reset);

        Assert.All(timing.Units, unit => Assert.Null(unit.Timestamp));
        Assert.Equal(0, status.TimedCount);
        Assert.Equal(timing.Units.Count, status.UntimedCount);
    }

    [Fact]
    public void Completion_IsHandledClearly()
    {
        var controller = CreateController(out var source, out _);

        source.Play();
        source.Advance(TimeSpan.FromSeconds(1));
        controller.HandleCommand(LiveTapTimingCommand.RecordTap);
        source.Advance(TimeSpan.FromSeconds(1));
        controller.HandleCommand(LiveTapTimingCommand.RecordTap);
        var status = controller.HandleCommand(LiveTapTimingCommand.RecordTap);

        Assert.True(status.IsComplete);
        Assert.Contains("already timed", status.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Stop_ResetsAudioAndSession()
    {
        var controller = CreateController(out var source, out var timing);

        source.Play();
        source.Advance(TimeSpan.FromSeconds(1));
        controller.HandleCommand(LiveTapTimingCommand.RecordTap);
        var status = controller.HandleCommand(LiveTapTimingCommand.Stop);

        Assert.False(source.IsPlaying);
        Assert.Equal(TimeSpan.Zero, source.CurrentTime);
        Assert.All(timing.Units, unit => Assert.Null(unit.Timestamp));
        Assert.Equal(0, status.TimedCount);
    }

    private static LiveTapTimingController CreateController(out MockAudioTimeSource source, out TimingDocument timing)
    {
        var lyrics = LyricsParser.Parse("Be|cause");
        timing = TimingDocumentBuilder.FromLyrics(lyrics);
        source = new MockAudioTimeSource();
        return new LiveTapTimingController(timing, source);
    }
}
