namespace KJCDGCreator.Core.Timing;

public sealed class TapTimingSession
{
    private readonly TimingDocument _timingDocument;
    private readonly Stack<int> _recordHistory = new();

    public TapTimingSession(TimingDocument timingDocument)
    {
        _timingDocument = timingDocument ?? throw new ArgumentNullException(nameof(timingDocument));
    }

    public bool IsComplete => GetCurrentPosition() >= _timingDocument.Units.Count;

    public int CurrentUnitIndex => IsComplete ? _timingDocument.Units.Count : _timingDocument.Units[GetCurrentPosition()].UnitIndex;

    public bool CanUndo => _recordHistory.Count > 0;

    public TimedUnit? GetCurrentUnit() => IsComplete ? null : _timingDocument.Units[GetCurrentPosition()];

    public void RecordTimestamp(TimeSpan timestamp)
    {
        if (IsComplete)
        {
            throw new InvalidOperationException("Cannot record a timestamp because the tap timing session is complete.");
        }

        if (timestamp < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timestamp), "Timestamp cannot be negative.");
        }

        var currentPosition = GetCurrentPosition();
        if (currentPosition > 0)
        {
            var previousUnit = _timingDocument.Units[currentPosition - 1];
            if (previousUnit.Timestamp.HasValue && timestamp < previousUnit.Timestamp.Value)
            {
                throw new InvalidOperationException("Cannot record a timestamp earlier than the previous recorded timestamp.");
            }
        }

        var currentUnit = _timingDocument.Units[currentPosition];
        _timingDocument.AssignTimestamp(currentUnit.UnitIndex, timestamp);
        _recordHistory.Push(currentUnit.UnitIndex);
    }

    public void Undo()
    {
        if (!CanUndo)
        {
            throw new InvalidOperationException("Cannot undo because no timestamps have been recorded in this session.");
        }

        var unitIndex = _recordHistory.Pop();
        _timingDocument.ClearTimestamp(unitIndex);
    }

    public void Reset()
    {
        _timingDocument.ResetAllTimestamps();
        _recordHistory.Clear();
    }

    private int GetCurrentPosition()
    {
        for (var index = 0; index < _timingDocument.Units.Count; index++)
        {
            if (!_timingDocument.Units[index].Timestamp.HasValue)
            {
                return index;
            }
        }

        return _timingDocument.Units.Count;
    }
}
