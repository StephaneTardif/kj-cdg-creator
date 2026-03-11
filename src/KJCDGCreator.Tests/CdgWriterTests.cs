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

    [Fact]
    public void RenderTileChanges_EmitsOnePacketPerChangedTile()
    {
        var previous = new CdgScreenBuffer(CdgScreenBuffer.CreateBlankTile(backgroundColor: 0));
        var current = previous.Clone();
        var tile = new CdgTile(
            backgroundColor: 0,
            foregroundColor: 15,
            bitmap: new byte[]
            {
                0b110000,
                0b110000,
                0b110000,
                0b110000,
                0b110000,
                0b110000,
                0b110000,
                0b110000,
                0b110000,
                0b110000,
                0b000000,
                0b000000
            });

        current.SetTile(2, 3, tile);

        var packets = CdgScreenBufferRenderer.RenderTileChanges(previous, current);

        Assert.Single(packets);
        Assert.Equal(24, packets[0].ToBytes().Length);
    }

    public void Dispose()
    {
        if (Directory.Exists(_outputDirectory))
        {
            Directory.Delete(_outputDirectory, recursive: true);
        }
    }
}
