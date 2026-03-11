using KJCDGCreator.Core.Cdg;

namespace KJCDGCreator.Tests;

public sealed class CdgWriterTests : IDisposable
{
    private readonly string _outputDirectory = Path.Combine(Path.GetTempPath(), "kj-cdg-creator-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public void WriteHelloWorld_CreatesCdgFileWithWholePackets()
    {
        Directory.CreateDirectory(_outputDirectory);
        var outputPath = Path.Combine(_outputDirectory, "hello.cdg");

        CdgWriter.WriteHelloWorld(outputPath);

        Assert.True(File.Exists(outputPath));

        var fileLength = new FileInfo(outputPath).Length;
        Assert.True(fileLength > 0);
        Assert.Equal(0, fileLength % 24);
    }

    public void Dispose()
    {
        if (Directory.Exists(_outputDirectory))
        {
            Directory.Delete(_outputDirectory, recursive: true);
        }
    }
}
