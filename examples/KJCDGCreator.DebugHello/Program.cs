using KJCDGCreator.Core.Cdg;
using System.Security.Cryptography;
using System.Text;

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
