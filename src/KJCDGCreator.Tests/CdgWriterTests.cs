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
    public void WriteText_CreatesDifferentOutputForDifferentText()
    {
        Directory.CreateDirectory(_outputDirectory);
        var firstPath = Path.Combine(_outputDirectory, "first.cdg");
        var secondPath = Path.Combine(_outputDirectory, "second.cdg");

        CdgWriter.WriteText(firstPath, "Hello");
        CdgWriter.WriteText(secondPath, "World");

        var firstBytes = File.ReadAllBytes(firstPath);
        var secondBytes = File.ReadAllBytes(secondPath);

        Assert.NotEqual(firstBytes, secondBytes);
    }

    [Fact]
    public void Inspector_ReadsGeneratedPacketsAndBuildsPreview()
    {
        Directory.CreateDirectory(_outputDirectory);
        var outputPath = Path.Combine(_outputDirectory, "inspect.cdg");

        CdgWriter.WriteText(outputPath, "Hello");

        var packets = CdgInspector.ReadPackets(outputPath);
        var preview = CdgInspector.RenderAsciiPreview(outputPath);

        Assert.Equal(6, packets.Count);
        Assert.Equal(CdgPacketType.MemoryPreset, packets[0].Type);
        Assert.Equal(CdgPacketType.TileBlockNormal, packets[1].Type);
        Assert.Contains("HELLO", preview);
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

    [Fact]
    public void RenderText_UpdatesScreenBufferTiles()
    {
        var screen = new CdgScreenBuffer(CdgScreenBuffer.CreateBlankTile(backgroundColor: 0));
        var renderer = new BitmapFontRenderer(screen, backgroundColor: 0);

        renderer.RenderText("Ab", tileX: 5, tileY: 6, color: 9);

        var firstTile = screen.GetTile(6, 5);
        var secondTile = screen.GetTile(6, 6);

        Assert.Equal((byte)9, firstTile.ForegroundColor);
        Assert.Equal((byte)9, secondTile.ForegroundColor);
        Assert.NotEqual(screen.DefaultTile, firstTile);
        Assert.NotEqual(screen.DefaultTile, secondTile);
        Assert.Contains(firstTile.Bitmap, row => row != 0);
        Assert.Contains(secondTile.Bitmap, row => row != 0);
    }

    [Fact]
    public void RenderText_GeneratesTileChangesForRenderedCharacters()
    {
        var previous = new CdgScreenBuffer(CdgScreenBuffer.CreateBlankTile(backgroundColor: 0));
        var current = previous.Clone();
        var renderer = new BitmapFontRenderer(current, backgroundColor: 0);

        renderer.RenderText("A-", tileX: 1, tileY: 1, color: 12);

        var packets = CdgScreenBufferRenderer.RenderTileChanges(previous, current);

        Assert.Equal(2, packets.Count);
        Assert.All(packets, packet => Assert.Equal(24, packet.ToBytes().Length));
    }

    public void Dispose()
    {
        if (Directory.Exists(_outputDirectory))
        {
            Directory.Delete(_outputDirectory, recursive: true);
        }
    }
}
