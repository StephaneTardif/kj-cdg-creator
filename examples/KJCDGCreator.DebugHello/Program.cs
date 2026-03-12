using KJCDGCreator.Core.Cdg;
using System.Security.Cryptography;
using System.Text;

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
