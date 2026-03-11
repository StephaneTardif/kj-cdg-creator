using KJCDGCreator.Core.Cdg;

var repositoryRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
var outputDirectory = Path.Combine(repositoryRoot, "examples", "generated");
var outputPath = Path.Combine(outputDirectory, "hello.cdg");

CdgWriter.WriteHelloWorld(outputPath);

Console.WriteLine($"Generated CDG file: {outputPath}");
