namespace KJCDGCreator.Audio.Timing;

public interface IAudioPlaybackClock : IAudioTimeSource
{
    AudioPlaybackState State { get; }

    TimeSpan Position { get; }
}
