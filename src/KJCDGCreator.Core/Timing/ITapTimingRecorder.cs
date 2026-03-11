namespace KJCDGCreator.Core.Timing;

public interface ITapTimingRecorder
{
    void Start();

    void RegisterTap(TimeSpan timestamp);

    TimingSession Stop();
}
