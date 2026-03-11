namespace KJCDGCreator.Audio.Timing;

public interface IAudioPlaybackClock
{
    AudioPlaybackState State { get; }

    TimeSpan Position { get; }
}
