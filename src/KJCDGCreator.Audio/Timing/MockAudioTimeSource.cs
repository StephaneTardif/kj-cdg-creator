namespace KJCDGCreator.Audio.Timing;

public sealed class MockAudioTimeSource : IAudioPlaybackClock
{
    private TimeSpan _currentTime;

    public AudioPlaybackState State { get; private set; } = AudioPlaybackState.Stopped;

    public TimeSpan Position => CurrentTime;

    public TimeSpan CurrentTime => _currentTime;

    public bool IsPlaying => State == AudioPlaybackState.Playing;

    public void Play()
    {
        State = AudioPlaybackState.Playing;
    }

    public void Pause()
    {
        if (State == AudioPlaybackState.Playing)
        {
            State = AudioPlaybackState.Paused;
        }
    }

    public void Stop()
    {
        State = AudioPlaybackState.Stopped;
        _currentTime = TimeSpan.Zero;
    }

    public void Advance(TimeSpan delta)
    {
        if (delta < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(delta), "Advance delta cannot be negative.");
        }

        if (!IsPlaying)
        {
            return;
        }

        _currentTime += delta;
    }
}
