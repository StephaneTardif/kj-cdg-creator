using KJCDGCreator.Core.Timing;

namespace KJCDGCreator.Audio.Timing;

public sealed class LiveTapTimingController
{
    private readonly TimingDocument _timingDocument;
    private readonly IAudioTimeSource _audioTimeSource;
    private readonly TapTimingSession _session;

    public LiveTapTimingController(TimingDocument timingDocument, IAudioTimeSource audioTimeSource)
    {
        _timingDocument = timingDocument ?? throw new ArgumentNullException(nameof(timingDocument));
        _audioTimeSource = audioTimeSource ?? throw new ArgumentNullException(nameof(audioTimeSource));
        _session = new TapTimingSession(timingDocument);
    }

    public LiveTapTimingStatus GetStatus(string? message = null)
    {
        var currentUnit = _session.GetCurrentUnit();

        return new LiveTapTimingStatus(
            CurrentUnitIndex: _session.CurrentUnitIndex,
            CurrentUnitText: currentUnit?.Text,
            IsComplete: _session.IsComplete,
            TimedCount: _timingDocument.TimedCount,
            UntimedCount: _timingDocument.UntimedCount,
            IsPlaying: _audioTimeSource.IsPlaying,
            Message: message ?? string.Empty);
    }

    public LiveTapTimingStatus HandleCommand(LiveTapTimingCommand command)
    {
        return command switch
        {
            LiveTapTimingCommand.RecordTap => RecordTap(),
            LiveTapTimingCommand.Undo => Undo(),
            LiveTapTimingCommand.Reset => Reset(),
            LiveTapTimingCommand.TogglePlayPause => TogglePlayPause(),
            LiveTapTimingCommand.Stop => Stop(),
            LiveTapTimingCommand.Quit => Quit(),
            _ => GetStatus("Unknown command.")
        };
    }

    private LiveTapTimingStatus RecordTap()
    {
        if (_session.IsComplete)
        {
            return GetStatus("All units are already timed. No more taps can be recorded.");
        }

        try
        {
            var recordedTime = _audioTimeSource.CurrentTime;
            _session.RecordTimestamp(recordedTime);
            var nextUnit = _session.GetCurrentUnit()?.Text ?? "<complete>";
            return GetStatus($"Recorded {recordedTime:hh\\:mm\\:ss\\.fff}. Next unit: {nextUnit}");
        }
        catch (Exception exception) when (exception is InvalidOperationException or ArgumentOutOfRangeException)
        {
            return GetStatus(exception.Message);
        }
    }

    private LiveTapTimingStatus Undo()
    {
        if (!_session.CanUndo)
        {
            return GetStatus("Nothing to undo.");
        }

        _session.Undo();
        return GetStatus($"Undo complete. Current unit: {_session.GetCurrentUnit()?.Text ?? "<complete>"}");
    }

    private LiveTapTimingStatus Reset()
    {
        _session.Reset();
        return GetStatus("All taps cleared.");
    }

    private LiveTapTimingStatus TogglePlayPause()
    {
        if (_audioTimeSource.IsPlaying)
        {
            _audioTimeSource.Pause();
            return GetStatus("Playback paused.");
        }

        _audioTimeSource.Play();
        return GetStatus("Playback started.");
    }

    private LiveTapTimingStatus Stop()
    {
        _audioTimeSource.Stop();
        _session.Reset();
        return GetStatus("Playback stopped and tap timing reset.");
    }

    private LiveTapTimingStatus Quit()
    {
        _audioTimeSource.Stop();
        return GetStatus("Quitting live tap timing.");
    }
}
