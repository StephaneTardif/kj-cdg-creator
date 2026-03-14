namespace KJCDGCreator.Audio.Timing;

public sealed record LiveTapTimingStatus(
    int CurrentUnitIndex,
    string? CurrentUnitText,
    bool IsComplete,
    int TimedCount,
    int UntimedCount,
    bool IsPlaying,
    string Message);
