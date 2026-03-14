using KJCDGCreator.Audio.Timing;

namespace KJCDGCreator.Tests;

public sealed class AudioTimeSourceTests
{
    [Fact]
    public void MockTimeSource_AdvancesDeterministically()
    {
        var source = new MockAudioTimeSource();

        source.Play();
        source.Advance(TimeSpan.FromSeconds(1));
        source.Advance(TimeSpan.FromSeconds(2));

        Assert.Equal(TimeSpan.FromSeconds(3), source.CurrentTime);
        Assert.True(source.IsPlaying);
    }

    [Fact]
    public void Pause_StopsTimeAdvancement()
    {
        var source = new MockAudioTimeSource();

        source.Play();
        source.Advance(TimeSpan.FromSeconds(2));
        source.Pause();
        source.Advance(TimeSpan.FromSeconds(3));

        Assert.Equal(TimeSpan.FromSeconds(2), source.CurrentTime);
        Assert.False(source.IsPlaying);
    }

    [Fact]
    public void Stop_ResetsTime()
    {
        var source = new MockAudioTimeSource();

        source.Play();
        source.Advance(TimeSpan.FromSeconds(2));
        source.Stop();

        Assert.Equal(TimeSpan.Zero, source.CurrentTime);
        Assert.Equal(AudioPlaybackState.Stopped, source.State);
    }

    [Fact]
    public void CurrentTime_NeverDecreases()
    {
        var source = new MockAudioTimeSource();

        source.Play();
        source.Advance(TimeSpan.FromSeconds(2));
        var beforePause = source.CurrentTime;
        source.Pause();
        var afterPause = source.CurrentTime;
        source.Play();
        source.Advance(TimeSpan.FromSeconds(1));
        var afterResume = source.CurrentTime;

        Assert.True(afterPause >= beforePause);
        Assert.True(afterResume >= afterPause);
    }

    [Fact]
    public void InvalidMp3Paths_AreHandledClearly()
    {
        var exception = Assert.Throws<FileNotFoundException>(() =>
            new Mp3AudioTimeSource("/path/that/does/not/exist.mp3"));

        Assert.Contains("MP3", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}
