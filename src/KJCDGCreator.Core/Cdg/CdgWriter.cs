namespace KJCDGCreator.Core.Cdg;

public static class CdgWriter
{
    private const byte BackgroundColor = 0;
    private const byte ForegroundColor = 15;

    public static void WriteHelloWorld(string path)
    {
        WriteText(path, "Hello CDG");
    }

    public static void WriteText(string path, string text, int tileX = 0, int tileY = 7, byte color = ForegroundColor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        if (tileX < 0 || tileX >= CdgScreenBuffer.Columns)
        {
            throw new ArgumentOutOfRangeException(nameof(tileX));
        }

        if (tileY < 0 || tileY >= CdgScreenBuffer.Rows)
        {
            throw new ArgumentOutOfRangeException(nameof(tileY));
        }

        if (tileX + text.Length > CdgScreenBuffer.Columns)
        {
            throw new ArgumentException("Text does not fit within the CDG tile grid.", nameof(text));
        }

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var screen = new CdgScreenBuffer(CdgScreenBuffer.CreateBlankTile(BackgroundColor));
        var fontRenderer = new BitmapFontRenderer(screen, backgroundColor: BackgroundColor);
        fontRenderer.RenderText(text, tileX, tileY, color);

        using var stream = File.Create(path);
        foreach (var packet in CdgScreenBufferRenderer.RenderFullScreen(screen, BackgroundColor))
        {
            var bytes = packet.ToBytes();
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}
