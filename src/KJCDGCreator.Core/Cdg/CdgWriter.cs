namespace KJCDGCreator.Core.Cdg;

public static class CdgWriter
{
    private const byte BackgroundColor = 0;
    private const byte ForegroundColor = 15;

    private static readonly IReadOnlyDictionary<char, byte[]> Glyphs = new Dictionary<char, byte[]>
    {
        ['H'] = Glyph(
            "100001",
            "100001",
            "100001",
            "100001",
            "111111",
            "100001",
            "100001",
            "100001",
            "100001",
            "100001",
            "000000",
            "000000"),
        ['e'] = Glyph(
            "000000",
            "000000",
            "011110",
            "100001",
            "111111",
            "100000",
            "100000",
            "111110",
            "000000",
            "000000",
            "000000",
            "000000"),
        ['l'] = Glyph(
            "001100",
            "001100",
            "001100",
            "001100",
            "001100",
            "001100",
            "001100",
            "001100",
            "001100",
            "000111",
            "000000",
            "000000"),
        ['o'] = Glyph(
            "000000",
            "000000",
            "011110",
            "100001",
            "100001",
            "100001",
            "100001",
            "011110",
            "000000",
            "000000",
            "000000",
            "000000"),
        [' '] = Glyph(
            "000000",
            "000000",
            "000000",
            "000000",
            "000000",
            "000000",
            "000000",
            "000000",
            "000000",
            "000000",
            "000000",
            "000000"),
        ['C'] = Glyph(
            "001111",
            "010000",
            "100000",
            "100000",
            "100000",
            "100000",
            "100000",
            "010000",
            "001111",
            "000000",
            "000000",
            "000000"),
        ['D'] = Glyph(
            "111100",
            "100010",
            "100001",
            "100001",
            "100001",
            "100001",
            "100001",
            "100010",
            "111100",
            "000000",
            "000000",
            "000000"),
        ['G'] = Glyph(
            "001111",
            "010000",
            "100000",
            "100000",
            "100111",
            "100001",
            "100001",
            "010001",
            "001111",
            "000000",
            "000000",
            "000000")
    };

    public static void WriteHelloWorld(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var screen = new CdgScreenBuffer(CdgScreenBuffer.CreateBlankTile(BackgroundColor));
        var message = "Hello CDG";
        const int row = 4;
        const int startColumn = 10;

        for (var index = 0; index < message.Length; index++)
        {
            var glyph = GetGlyph(message[index]);
            screen.SetTile(row, startColumn + index, new CdgTile(BackgroundColor, ForegroundColor, glyph));
        }

        using var stream = File.Create(path);
        foreach (var packet in CdgScreenBufferRenderer.RenderFullScreen(screen, BackgroundColor))
        {
            var bytes = packet.ToBytes();
            stream.Write(bytes, 0, bytes.Length);
        }
    }

    private static byte[] GetGlyph(char value)
    {
        if (Glyphs.TryGetValue(value, out var glyph))
        {
            return glyph;
        }

        throw new InvalidOperationException($"No CDG glyph is defined for '{value}'.");
    }

    private static byte[] Glyph(params string[] rows)
    {
        if (rows.Length != 12)
        {
            throw new ArgumentException("A glyph must contain exactly 12 rows.", nameof(rows));
        }

        return rows.Select(ParseRow).ToArray();
    }

    private static byte ParseRow(string row)
    {
        if (row.Length != 6)
        {
            throw new ArgumentException("A CDG glyph row must be 6 pixels wide.", nameof(row));
        }

        byte value = 0;

        for (var index = 0; index < row.Length; index++)
        {
            if (row[index] == '1')
            {
                value |= (byte)(1 << (5 - index));
            }
        }

        return value;
    }
}
