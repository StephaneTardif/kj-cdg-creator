namespace KJCDGCreator.Core.Timing;

public sealed class TimingDocument
{
    private readonly List<TimedUnit> _units;

    public TimingDocument(IEnumerable<TimedUnit> units)
    {
        ArgumentNullException.ThrowIfNull(units);
        _units = units.OrderBy(unit => unit.UnitIndex).ToList();
    }

    public IReadOnlyList<TimedUnit> Units => _units;

    public int TimedCount => _units.Count(unit => unit.Timestamp.HasValue);

    public int UntimedCount => _units.Count - TimedCount;

    public void AssignTimestamp(int unitIndex, TimeSpan timestamp)
    {
        if (timestamp < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timestamp), "Timestamp cannot be negative.");
        }

        var position = _units.FindIndex(unit => unit.UnitIndex == unitIndex);
        if (position < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(unitIndex), "No timed unit exists for the specified index.");
        }

        var existing = _units[position];
        _units[position] = existing with { Timestamp = timestamp };
    }

    public void ClearTimestamp(int unitIndex)
    {
        var position = _units.FindIndex(unit => unit.UnitIndex == unitIndex);
        if (position < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(unitIndex), "No timed unit exists for the specified index.");
        }

        var existing = _units[position];
        _units[position] = existing with { Timestamp = null };
    }

    public void ResetAllTimestamps()
    {
        for (var index = 0; index < _units.Count; index++)
        {
            var existing = _units[index];
            _units[index] = existing with { Timestamp = null };
        }
    }
}
