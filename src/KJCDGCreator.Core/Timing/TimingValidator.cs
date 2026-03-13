namespace KJCDGCreator.Core.Timing;

public static class TimingValidator
{
    public static TimingValidationResult Validate(TimingDocument timing, bool requireAllTimestamps)
    {
        ArgumentNullException.ThrowIfNull(timing);

        var errors = new List<string>();
        TimeSpan? previousTimestamp = null;

        foreach (var unit in timing.Units)
        {
            if (!unit.Timestamp.HasValue)
            {
                if (requireAllTimestamps)
                {
                    errors.Add($"Unit {unit.UnitIndex} ({unit.Text}) is missing a timestamp.");
                }

                continue;
            }

            var timestamp = unit.Timestamp.Value;
            if (timestamp < TimeSpan.Zero)
            {
                errors.Add($"Unit {unit.UnitIndex} ({unit.Text}) has a negative timestamp.");
            }

            if (previousTimestamp.HasValue && timestamp < previousTimestamp.Value)
            {
                errors.Add($"Unit {unit.UnitIndex} ({unit.Text}) has a timestamp earlier than the previous timed unit.");
            }

            previousTimestamp = timestamp;
        }

        return new TimingValidationResult(errors.Count == 0, errors);
    }
}
