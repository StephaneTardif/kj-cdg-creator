using KJCDGCreator.Core.Lyrics;
using KJCDGCreator.Core.Rendering;
using KJCDGCreator.Core.Timing;

namespace KJCDGCreator.Core.Cdg;

public static class CdgTimelineExporter
{
    public static CdgFrameSequenceResult Export(
        LyricsDocument lyricsDocument,
        TimingDocument timing,
        string outputPath,
        CdgTimelineExportOptions options)
    {
        ArgumentNullException.ThrowIfNull(lyricsDocument);
        ArgumentNullException.ThrowIfNull(timing);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentNullException.ThrowIfNull(options);

        if (options.FrameStep <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "FrameStep must be greater than zero.");
        }

        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var backgroundColor = options.FrameRenderOptions.HighlightOptions.BackgroundColor;
        var previousScreen = new CdgScreenBuffer(CdgScreenBuffer.CreateBlankTile(backgroundColor));
        var endPadding = options.EndPadding ?? TimeSpan.Zero;
        var endTime = GetEndTime(timing, endPadding, options.FrameRenderOptions);
        var frameTimes = GetFrameTimes(endTime, options.FrameStep);
        var packetCount = 0;

        using var stream = File.Create(outputPath);

        if (options.IncludeInitialClearFrame)
        {
            var clearPacket = CdgScreenBufferRenderer.CreateMemoryPresetPacket(backgroundColor);
            WritePacket(stream, clearPacket);
            packetCount++;
        }

        var frameCount = 0;

        foreach (var frameTime in frameTimes)
        {
            var currentScreen = new CdgScreenBuffer(CdgScreenBuffer.CreateBlankTile(backgroundColor));
            KaraokeFrameRenderer.RenderFrame(lyricsDocument, timing, frameTime, currentScreen, options.FrameRenderOptions);

            var packets = CdgScreenBufferRenderer.RenderTileChanges(previousScreen, currentScreen);
            foreach (var packet in packets)
            {
                WritePacket(stream, packet);
                packetCount++;
            }

            previousScreen = currentScreen;
            frameCount++;
        }

        return new CdgFrameSequenceResult(outputPath, frameCount, packetCount);
    }

    private static TimeSpan GetEndTime(TimingDocument timing, TimeSpan endPadding, KaraokeFrameRenderOptions frameRenderOptions)
    {
        var latestTimestamp = timing.Units
            .Where(unit => unit.Timestamp.HasValue)
            .Select(unit => unit.Timestamp!.Value)
            .DefaultIfEmpty(TimeSpan.Zero)
            .Max();

        var introDuration = IntroScreenSelector.GetEffectiveDuration(timing, frameRenderOptions.IntroOptions);
        var lyricEndTime = latestTimestamp + endPadding;
        return introDuration > lyricEndTime ? introDuration : lyricEndTime;
    }

    private static IReadOnlyList<TimeSpan> GetFrameTimes(TimeSpan endTime, TimeSpan frameStep)
    {
        var times = new List<TimeSpan>();

        for (var current = TimeSpan.Zero; current <= endTime; current += frameStep)
        {
            times.Add(current);
        }

        if (times.Count == 0 || times[^1] != endTime)
        {
            times.Add(endTime);
        }

        return times;
    }

    private static void WritePacket(Stream stream, CdgPacket packet)
    {
        var bytes = packet.ToBytes();
        stream.Write(bytes, 0, bytes.Length);
    }
}
