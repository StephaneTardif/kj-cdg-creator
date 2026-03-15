using KJCDGCreator.Core.Timing;

namespace KJCDGCreator.Core.Rendering;

public static class IntroScreenSelector
{
    public static bool IsIntroActive(
        TimingDocument timing,
        TimeSpan playbackTime,
        KaraokeSongMetadata? metadata,
        IntroTitleScreenOptions? options)
    {
        ArgumentNullException.ThrowIfNull(timing);

        if (!HasRenderableIntro(metadata, options))
        {
            return false;
        }

        return playbackTime < GetEffectiveDuration(timing, options);
    }

    public static TimeSpan GetEffectiveDuration(TimingDocument timing, IntroTitleScreenOptions? options)
    {
        ArgumentNullException.ThrowIfNull(timing);

        if (options is null || !options.Enabled)
        {
            return TimeSpan.Zero;
        }

        var duration = options.FixedDuration ?? TimeSpan.Zero;
        if (duration < TimeSpan.Zero)
        {
            duration = TimeSpan.Zero;
        }

        if (!options.UseFirstLyricTimestampWhenLonger)
        {
            return duration;
        }

        var firstLyricTimestamp = timing.Units
            .Where(unit => unit.Timestamp.HasValue)
            .Select(unit => unit.Timestamp!.Value)
            .DefaultIfEmpty(TimeSpan.Zero)
            .Min();

        return firstLyricTimestamp > duration ? firstLyricTimestamp : duration;
    }

    public static bool HasRenderableIntro(KaraokeSongMetadata? metadata, IntroTitleScreenOptions? options)
    {
        if (options is null || !options.Enabled || metadata is null)
        {
            return false;
        }

        return !string.IsNullOrWhiteSpace(metadata.Title) || !string.IsNullOrWhiteSpace(metadata.Artist);
    }
}
