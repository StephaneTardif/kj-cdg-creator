using KJCDGCreator.Core.Cdg;
using KJCDGCreator.Core.Lyrics;
using KJCDGCreator.Core.Timing;
using System.Security.Cryptography;
using System.Text;

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
