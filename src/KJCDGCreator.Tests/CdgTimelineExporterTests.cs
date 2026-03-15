using KJCDGCreator.Core.Cdg;
using KJCDGCreator.Core.Lyrics;
using KJCDGCreator.Core.Rendering;
using KJCDGCreator.Core.Timing;

namespace KJCDGCreator.Tests;

public sealed class CdgTimelineExporterTests : IDisposable
{
    private readonly string _outputDirectory = Path.Combine(Path.GetTempPath(), "kj-cdg-creator-export-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public void Export_CreatesNonEmptyCdgFile()
    {
        var outputPath = CreateOutputPath("timeline.cdg");
        var (lyrics, timing) = CreateLyricsAndTiming("Be|cause I'm hap|py");
        timing.AssignTimestamp(0, TimeSpan.FromSeconds(1));
        timing.AssignTimestamp(1, TimeSpan.FromSeconds(2));
        timing.AssignTimestamp(2, TimeSpan.FromSeconds(3));

        var result = CdgTimelineExporter.Export(lyrics, timing, outputPath, CreateOptions());

        Assert.True(File.Exists(outputPath));
        Assert.True(new FileInfo(outputPath).Length > 0);
        Assert.True(result.PacketCount > 0);
    }

    [Fact]
    public void Export_IncludesInitialClearStateWhenRequested()
    {
        var outputPath = CreateOutputPath("initial-clear.cdg");
        var (lyrics, timing) = CreateLyricsAndTiming("Be|cause");
        timing.AssignTimestamp(0, TimeSpan.FromSeconds(1));
        timing.AssignTimestamp(1, TimeSpan.FromSeconds(2));

        CdgTimelineExporter.Export(
            lyrics,
            timing,
            outputPath,
            CreateOptions() with { IncludeInitialClearFrame = true });

        var packets = CdgInspector.ReadPackets(outputPath);
        Assert.Equal(CdgPacketType.MemoryPreset, packets[0].Type);
    }

    [Fact]
    public void Export_ProducesOutputForSimpleTimedLyrics()
    {
        var outputPath = CreateOutputPath("simple.cdg");
        var (lyrics, timing) = CreateLyricsAndTiming("Be|cause I'm hap|py");
        timing.AssignTimestamp(0, TimeSpan.FromSeconds(1));
        timing.AssignTimestamp(1, TimeSpan.FromSeconds(2));
        timing.AssignTimestamp(2, TimeSpan.FromSeconds(3));

        var result = CdgTimelineExporter.Export(lyrics, timing, outputPath, CreateOptions());

        Assert.True(result.FrameCount >= 4);
        Assert.True(result.PacketCount >= 1);
    }

    [Fact]
    public void Export_IsDeterministicForSameInputs()
    {
        var firstPath = CreateOutputPath("first.cdg");
        var secondPath = CreateOutputPath("second.cdg");
        var (lyrics, timing) = CreateLyricsAndTiming("Be|cause I'm hap|py");
        timing.AssignTimestamp(0, TimeSpan.FromSeconds(1));
        timing.AssignTimestamp(1, TimeSpan.FromSeconds(2));
        timing.AssignTimestamp(2, TimeSpan.FromSeconds(3));

        CdgTimelineExporter.Export(lyrics, timing, firstPath, CreateOptions());
        CdgTimelineExporter.Export(lyrics, timing, secondPath, CreateOptions());

        Assert.Equal(File.ReadAllBytes(firstPath), File.ReadAllBytes(secondPath));
    }

    [Fact]
    public void Export_HandlesMissingTimestampsSafely()
    {
        var outputPath = CreateOutputPath("missing.cdg");
        var (lyrics, timing) = CreateLyricsAndTiming("Be|cause I'm hap|py");
        timing.AssignTimestamp(0, TimeSpan.FromSeconds(1));

        var exception = Record.Exception(() =>
            CdgTimelineExporter.Export(lyrics, timing, outputPath, CreateOptions()));

        Assert.Null(exception);
        Assert.True(File.Exists(outputPath));
    }

    [Fact]
    public void Export_DoesNotThrowOnEmptyLyricsDocuments()
    {
        var outputPath = CreateOutputPath("empty.cdg");
        var lyrics = new LyricsDocument(Array.Empty<LyricsPage>());
        var timing = new TimingDocument(Array.Empty<TimedUnit>());

        var exception = Record.Exception(() =>
            CdgTimelineExporter.Export(lyrics, timing, outputPath, CreateOptions()));

        Assert.Null(exception);
        Assert.True(File.Exists(outputPath));
    }

    [Fact]
    public void Export_IncludesIntroScreenPacketsWhenEnabled()
    {
        var outputPath = CreateOutputPath("intro.cdg");
        var lyrics = new LyricsDocument(Array.Empty<LyricsPage>());
        var timing = new TimingDocument(Array.Empty<TimedUnit>());

        var result = CdgTimelineExporter.Export(
            lyrics,
            timing,
            outputPath,
            CreateOptions() with
            {
                EndPadding = TimeSpan.Zero,
                FrameRenderOptions = CreateOptions().FrameRenderOptions with
                {
                    SongMetadata = new KaraokeSongMetadata("INTRO TITLE", "INTRO ARTIST"),
                    IntroOptions = new IntroTitleScreenOptions(
                        Enabled: true,
                        FixedDuration: TimeSpan.FromSeconds(2),
                        UseFirstLyricTimestampWhenLonger: false,
                        BackgroundColor: 0,
                        TitleColor: 10,
                        ArtistColor: 14,
                        TitleRow: 2,
                        ArtistRow: 4,
                        CenterHorizontally: false)
                }
            });

        var packets = CdgInspector.ReadPackets(outputPath);

        Assert.True(result.FrameCount >= 3);
        Assert.Contains(
            packets,
            packet => packet.Type == CdgPacketType.TileBlockNormal
                && packet.Row == 2
                && packet.ForegroundColor == 10);
    }

    public void Dispose()
    {
        if (Directory.Exists(_outputDirectory))
        {
            Directory.Delete(_outputDirectory, recursive: true);
        }
    }

    private string CreateOutputPath(string name)
    {
        Directory.CreateDirectory(_outputDirectory);
        return Path.Combine(_outputDirectory, name);
    }

    private static CdgTimelineExportOptions CreateOptions() =>
        new(
            FrameStep: TimeSpan.FromSeconds(1),
            FrameRenderOptions: new KaraokeFrameRenderOptions(
                new PageLayoutOptions(
                    StartRow: 2,
                    StartColumn: 3,
                    LineSpacing: 1,
                    CenterHorizontally: false,
                    CenterVertically: false),
                new HighlightedLyricsRenderOptions(
                    StartRow: 2,
                    StartColumn: 3,
                    LineSpacing: 1,
                    BackgroundColor: 0,
                    BaseTextColor: 15,
                    HighlightTextColor: 12,
                    ClearScreenBeforeRender: true)),
            EndPadding: TimeSpan.FromSeconds(1),
            IncludeInitialClearFrame: true);

    private static (LyricsDocument Lyrics, TimingDocument Timing) CreateLyricsAndTiming(string text)
    {
        var lyrics = LyricsParser.Parse(text);
        var timing = TimingDocumentBuilder.FromLyrics(lyrics);
        return (lyrics, timing);
    }
}
