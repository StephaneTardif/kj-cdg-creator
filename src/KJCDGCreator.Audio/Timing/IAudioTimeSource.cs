namespace KJCDGCreator.Audio.Timing;

public interface IAudioTimeSource
{
    TimeSpan CurrentTime { get; }

    bool IsPlaying { get; }

    void Play();

    void Pause();

    void Stop();
}
