using KJCDGCreator.Core.Cdg;
using KJCDGCreator.Core.Lyrics;
using KJCDGCreator.Core.Rendering;
using KJCDGCreator.Core.Timing;
using System.Security.Cryptography;
using System.Text;

if (args.Length >= 1 && string.Equals(args[0], "highlight-demo", StringComparison.OrdinalIgnoreCase))
{
    RunHighlightDemo();
    return;
}

if (args.Length >= 1 && string.Equals(args[0], "export-demo", StringComparison.OrdinalIgnoreCase))
{
    RunExportDemo();
    return;
}

if (args.Length >= 1 && string.Equals(args[0], "frame-demo", StringComparison.OrdinalIgnoreCase))
{
    RunFrameDemo();
    return;
}

if (args.Length >= 1 && string.Equals(args[0], "tap-demo", StringComparison.OrdinalIgnoreCase))
{
    RunTapDemo();
    return;
}

if (args.Length >= 2 && string.Equals(args[0], "inspect", StringComparison.OrdinalIgnoreCase))
{
    InspectCdg(args[1], args.Skip(2).Any(arg => string.Equals(arg, "--preview", StringComparison.OrdinalIgnoreCase)));
    return;
}

var repositoryRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
var outputDirectory = Path.Combine(repositoryRoot, "examples", "generated");
var text = args.Length > 0 ? args[0] : "Hello CDG";
var fileName = $"rendered-{CreateTextHash(text)}.cdg";
var outputPath = Path.Combine(outputDirectory, fileName);

CdgWriter.WriteText(outputPath, text);

Console.WriteLine($"Generated CDG file: {outputPath}");
Console.WriteLine($"Rendered text: {text}");

static string CreateTextHash(string text)
{
    var hash = SHA256.HashData(Encoding.UTF8.GetBytes(text));
    return Convert.ToHexString(hash[..6]).ToLowerInvariant();
}

static void InspectCdg(string path, bool preview)
{
    var packets = CdgInspector.ReadPackets(path);

    foreach (var packet in packets)
    {
        Console.WriteLine(
            $"[{packet.Index:D4}] {packet.Type,-15} row={packet.Row?.ToString() ?? "-",-2} col={packet.Column?.ToString() ?? "-",-2} bg={packet.BackgroundColor,-2} fg={packet.ForegroundColor,-2}");
    }

    if (preview)
    {
        Console.WriteLine();
        Console.WriteLine("ASCII preview:");
        Console.WriteLine(CdgInspector.RenderAsciiPreview(path));
    }
}

static void RunTapDemo()
{
    const string sampleLyrics = """
        Be|cause I'm hap|py
        Clap a|long
        """;

    var lyrics = LyricsParser.Parse(sampleLyrics);
    var timing = TimingDocumentBuilder.FromLyrics(lyrics);
    var session = new TapTimingSession(timing);

    Console.WriteLine("Tap demo");
    Console.WriteLine("Lyrics:");
    Console.WriteLine(sampleLyrics);
    Console.WriteLine();

    PrintSessionState("Startup", session, timing);

    session.RecordTimestamp(TimeSpan.FromSeconds(0.50));
    PrintSessionState("After recording 00:00:00.500", session, timing);

    session.RecordTimestamp(TimeSpan.FromSeconds(1.15));
    PrintSessionState("After recording 00:00:01.150", session, timing);

    session.RecordTimestamp(TimeSpan.FromSeconds(2.05));
    PrintSessionState("After recording 00:00:02.050", session, timing);

    session.Undo();
    PrintSessionState("After undo", session, timing);

    session.RecordTimestamp(TimeSpan.FromSeconds(2.30));
    PrintSessionState("After recording 00:00:02.300", session, timing);

    session.Reset();
    PrintSessionState("After reset", session, timing);
}

static void PrintSessionState(string label, TapTimingSession session, TimingDocument timing)
{
    var currentUnit = session.GetCurrentUnit();

    Console.WriteLine(label);
    Console.WriteLine($"  Current unit index: {session.CurrentUnitIndex}");
    Console.WriteLine($"  Current unit text:  {currentUnit?.Text ?? "<complete>"}");
    Console.WriteLine($"  Is complete:        {session.IsComplete}");
    Console.WriteLine($"  Can undo:           {session.CanUndo}");
    Console.WriteLine($"  Timed / untimed:    {timing.TimedCount} / {timing.UntimedCount}");
    Console.WriteLine();
}

static void RunHighlightDemo()
{
    const string sampleLyrics = """
        Be|cause I'm hap|py
        Clap a|long
        """;

    var lyrics = LyricsParser.Parse(sampleLyrics);
    var timing = TimingDocumentBuilder.FromLyrics(lyrics);
    timing.AssignTimestamp(0, TimeSpan.FromSeconds(0.50));
    timing.AssignTimestamp(1, TimeSpan.FromSeconds(1.15));
    timing.AssignTimestamp(2, TimeSpan.FromSeconds(2.05));
    timing.AssignTimestamp(3, TimeSpan.FromSeconds(2.80));
    timing.AssignTimestamp(4, TimeSpan.FromSeconds(3.50));

    var playbackTime = TimeSpan.FromSeconds(2.80);
    var pageState = HighlightProgressionEngine.GetPageState(lyrics, 0, timing, playbackTime);
    var screen = new CdgScreenBuffer(CdgScreenBuffer.CreateBlankTile(backgroundColor: 0));
    var options = new HighlightedLyricsRenderOptions(
        StartRow: 5,
        StartColumn: 4,
        LineSpacing: 2,
        BackgroundColor: 0,
        BaseTextColor: 15,
        HighlightTextColor: 12,
        ClearScreenBeforeRender: true);

    HighlightedLyricsRenderer.RenderPage(pageState, screen, options);

    var repositoryRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var outputDirectory = Path.Combine(repositoryRoot, "examples", "generated");
    Directory.CreateDirectory(outputDirectory);
    var outputPath = Path.Combine(outputDirectory, "highlight-demo.cdg");

    using (var stream = File.Create(outputPath))
    {
        foreach (var packet in CdgScreenBufferRenderer.RenderFullScreen(screen, options.BackgroundColor))
        {
            var bytes = packet.ToBytes();
            stream.Write(bytes, 0, bytes.Length);
        }
    }

    Console.WriteLine($"Generated highlighted lyrics CDG: {outputPath}");
    Console.WriteLine($"Playback time: {playbackTime}");
    Console.WriteLine("ASCII preview:");
    Console.WriteLine(CdgInspector.RenderAsciiPreview(outputPath));
}

static void RunFrameDemo()
{
    const string sampleLyrics = """
        Be|cause I'm hap|py
        Clap a|long

        Sing|ing loud to|gether
        All night| long
        """;

    var lyrics = LyricsParser.Parse(sampleLyrics);
    var timing = TimingDocumentBuilder.FromLyrics(lyrics);
    var timestamps = new[]
    {
        0.50, 1.10, 2.00, 2.80, 4.20, 5.10, 6.00
    };

    for (var index = 0; index < timestamps.Length; index++)
    {
        timing.AssignTimestamp(index, TimeSpan.FromSeconds(timestamps[index]));
    }

    var renderOptions = new KaraokeFrameRenderOptions(
        new PageLayoutOptions(
            StartRow: 4,
            StartColumn: 4,
            LineSpacing: 2,
            CenterHorizontally: true,
            CenterVertically: true),
        new HighlightedLyricsRenderOptions(
            StartRow: 4,
            StartColumn: 4,
            LineSpacing: 2,
            BackgroundColor: 0,
            BaseTextColor: 15,
            HighlightTextColor: 12,
            ClearScreenBeforeRender: true));

    var playbackTimes = new[]
    {
        TimeSpan.FromSeconds(0.25),
        TimeSpan.FromSeconds(2.80),
        TimeSpan.FromSeconds(5.20)
    };

    var repositoryRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var outputDirectory = Path.Combine(repositoryRoot, "examples", "generated");
    Directory.CreateDirectory(outputDirectory);

    for (var index = 0; index < playbackTimes.Length; index++)
    {
        var screen = new CdgScreenBuffer(CdgScreenBuffer.CreateBlankTile(backgroundColor: 0));
        var playbackTime = playbackTimes[index];
        var result = KaraokeFrameRenderer.RenderFrame(lyrics, timing, playbackTime, screen, renderOptions);
        var outputPath = Path.Combine(outputDirectory, $"frame-demo-{index + 1}.cdg");

        using (var stream = File.Create(outputPath))
        {
            foreach (var packet in CdgScreenBufferRenderer.RenderFullScreen(screen, renderOptions.HighlightOptions.BackgroundColor))
            {
                var bytes = packet.ToBytes();
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        Console.WriteLine($"Frame {index + 1}: {outputPath}");
        Console.WriteLine($"  Playback time: {playbackTime}");
        Console.WriteLine($"  Page index:    {result.PageIndex}");
        Console.WriteLine("  ASCII preview:");
        Console.WriteLine(CdgInspector.RenderAsciiPreview(outputPath));
        Console.WriteLine();
    }
}

static void RunExportDemo()
{
    const string sampleLyrics = """
        Be|cause I'm hap|py
        Clap a|long

        Sing|ing loud to|gether
        All night| long
        """;

    var lyrics = LyricsParser.Parse(sampleLyrics);
    var timing = TimingDocumentBuilder.FromLyrics(lyrics);
    var timestamps = new[]
    {
        0.50, 1.10, 2.00, 2.80, 4.20, 5.10, 6.00
    };

    for (var index = 0; index < timestamps.Length; index++)
    {
        timing.AssignTimestamp(index, TimeSpan.FromSeconds(timestamps[index]));
    }

    var repositoryRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    var outputDirectory = Path.Combine(repositoryRoot, "examples", "generated");
    Directory.CreateDirectory(outputDirectory);
    var outputPath = Path.Combine(outputDirectory, "export-demo.cdg");

    var result = CdgTimelineExporter.Export(
        lyrics,
        timing,
        outputPath,
        new CdgTimelineExportOptions(
            FrameStep: TimeSpan.FromSeconds(1),
            FrameRenderOptions: new KaraokeFrameRenderOptions(
                new PageLayoutOptions(
                    StartRow: 4,
                    StartColumn: 4,
                    LineSpacing: 2,
                    CenterHorizontally: true,
                    CenterVertically: true),
                new HighlightedLyricsRenderOptions(
                    StartRow: 4,
                    StartColumn: 4,
                    LineSpacing: 2,
                    BackgroundColor: 0,
                    BaseTextColor: 15,
                    HighlightTextColor: 12,
                    ClearScreenBeforeRender: true)),
            EndPadding: TimeSpan.FromSeconds(1),
            IncludeInitialClearFrame: true));

    Console.WriteLine($"Exported CDG timeline: {result.OutputPath}");
    Console.WriteLine($"Frames rendered:       {result.FrameCount}");
    Console.WriteLine($"Packets written:       {result.PacketCount}");
}
