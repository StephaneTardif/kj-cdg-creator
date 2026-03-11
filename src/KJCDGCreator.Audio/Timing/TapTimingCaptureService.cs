using KJCDGCreator.Core.Timing;

namespace KJCDGCreator.Audio.Timing;

public sealed class TapTimingCaptureService
{
    private readonly ITapTimingRecorder _recorder;
    private readonly IAudioPlaybackClock _clock;

    public TapTimingCaptureService(ITapTimingRecorder recorder, IAudioPlaybackClock clock)
    {
        _recorder = recorder;
        _clock = clock;
    }

    public void CaptureTap()
    {
        _recorder.RegisterTap(_clock.Position);
    }
}
