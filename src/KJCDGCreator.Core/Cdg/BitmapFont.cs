namespace KJCDGCreator.Core.Cdg;

public sealed class BitmapFont
{
    public static BitmapFont Default { get; } = new(CreateDefaultGlyphs());

    private readonly IReadOnlyDictionary<char, byte[]> _glyphs;

    public BitmapFont(IReadOnlyDictionary<char, byte[]> glyphs)
    {
        ArgumentNullException.ThrowIfNull(glyphs);
        _glyphs = glyphs;
    }

    public IReadOnlyList<byte> GetGlyph(char value)
    {
        if (_glyphs.TryGetValue(value, out var glyph))
        {
            return glyph;
        }

        if (char.IsLetter(value))
        {
            var upper = char.ToUpperInvariant(value);
            if (_glyphs.TryGetValue(upper, out glyph))
            {
                return glyph;
            }
        }

        throw new InvalidOperationException($"No bitmap font glyph is defined for '{value}'.");
    }

    private static IReadOnlyDictionary<char, byte[]> CreateDefaultGlyphs() =>
        new Dictionary<char, byte[]>
        {
            ['A'] = BuildGlyph("01110", "10001", "10001", "11111", "10001", "10001", "10001"),
            ['B'] = BuildGlyph("11110", "10001", "10001", "11110", "10001", "10001", "11110"),
            ['C'] = BuildGlyph("01111", "10000", "10000", "10000", "10000", "10000", "01111"),
            ['D'] = BuildGlyph("11110", "10001", "10001", "10001", "10001", "10001", "11110"),
            ['E'] = BuildGlyph("11111", "10000", "10000", "11110", "10000", "10000", "11111"),
            ['F'] = BuildGlyph("11111", "10000", "10000", "11110", "10000", "10000", "10000"),
            ['G'] = BuildGlyph("01111", "10000", "10000", "10011", "10001", "10001", "01111"),
            ['H'] = BuildGlyph("10001", "10001", "10001", "11111", "10001", "10001", "10001"),
            ['I'] = BuildGlyph("11111", "00100", "00100", "00100", "00100", "00100", "11111"),
            ['J'] = BuildGlyph("00111", "00010", "00010", "00010", "10010", "10010", "01100"),
            ['K'] = BuildGlyph("10001", "10010", "10100", "11000", "10100", "10010", "10001"),
            ['L'] = BuildGlyph("10000", "10000", "10000", "10000", "10000", "10000", "11111"),
            ['M'] = BuildGlyph("10001", "11011", "10101", "10101", "10001", "10001", "10001"),
            ['N'] = BuildGlyph("10001", "11001", "10101", "10011", "10001", "10001", "10001"),
            ['O'] = BuildGlyph("01110", "10001", "10001", "10001", "10001", "10001", "01110"),
            ['P'] = BuildGlyph("11110", "10001", "10001", "11110", "10000", "10000", "10000"),
            ['Q'] = BuildGlyph("01110", "10001", "10001", "10001", "10101", "10010", "01101"),
            ['R'] = BuildGlyph("11110", "10001", "10001", "11110", "10100", "10010", "10001"),
            ['S'] = BuildGlyph("01111", "10000", "10000", "01110", "00001", "00001", "11110"),
            ['T'] = BuildGlyph("11111", "00100", "00100", "00100", "00100", "00100", "00100"),
            ['U'] = BuildGlyph("10001", "10001", "10001", "10001", "10001", "10001", "01110"),
            ['V'] = BuildGlyph("10001", "10001", "10001", "10001", "10001", "01010", "00100"),
            ['W'] = BuildGlyph("10001", "10001", "10001", "10101", "10101", "10101", "01010"),
            ['X'] = BuildGlyph("10001", "10001", "01010", "00100", "01010", "10001", "10001"),
            ['Y'] = BuildGlyph("10001", "10001", "01010", "00100", "00100", "00100", "00100"),
            ['Z'] = BuildGlyph("11111", "00001", "00010", "00100", "01000", "10000", "11111"),
            ['0'] = BuildGlyph("01110", "10001", "10011", "10101", "11001", "10001", "01110"),
            ['1'] = BuildGlyph("00100", "01100", "00100", "00100", "00100", "00100", "01110"),
            ['2'] = BuildGlyph("01110", "10001", "00001", "00010", "00100", "01000", "11111"),
            ['3'] = BuildGlyph("11110", "00001", "00001", "01110", "00001", "00001", "11110"),
            ['4'] = BuildGlyph("00010", "00110", "01010", "10010", "11111", "00010", "00010"),
            ['5'] = BuildGlyph("11111", "10000", "10000", "11110", "00001", "00001", "11110"),
            ['6'] = BuildGlyph("01110", "10000", "10000", "11110", "10001", "10001", "01110"),
            ['7'] = BuildGlyph("11111", "00001", "00010", "00100", "01000", "01000", "01000"),
            ['8'] = BuildGlyph("01110", "10001", "10001", "01110", "10001", "10001", "01110"),
            ['9'] = BuildGlyph("01110", "10001", "10001", "01111", "00001", "00001", "01110"),
            [' '] = BuildGlyph("00000", "00000", "00000", "00000", "00000", "00000", "00000"),
            ['\''] = BuildGlyph("00100", "00100", "00010", "00000", "00000", "00000", "00000"),
            ['.'] = BuildGlyph("00000", "00000", "00000", "00000", "00000", "00100", "00100"),
            [','] = BuildGlyph("00000", "00000", "00000", "00000", "00100", "00100", "00010"),
            ['-'] = BuildGlyph("00000", "00000", "00000", "01110", "00000", "00000", "00000")
        };

    private static byte[] BuildGlyph(params string[] rows)
    {
        if (rows.Length != 7)
        {
            throw new ArgumentException("The source glyph must contain exactly 7 rows.", nameof(rows));
        }

        var rowMap = new[] { 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 6 };
        var glyph = new byte[CdgTile.Height];

        for (var index = 0; index < glyph.Length; index++)
        {
            glyph[index] = ExpandRow(rows[rowMap[index]]);
        }

        return glyph;
    }

    private static byte ExpandRow(string row)
    {
        if (row.Length != 5)
        {
            throw new ArgumentException("Bitmap font rows must be 5 pixels wide.", nameof(row));
        }

        byte value = 0;

        for (var index = 0; index < row.Length; index++)
        {
            if (row[index] == '1')
            {
                value |= (byte)(1 << (4 - index));
            }
        }

        return (byte)(value << 1);
    }
}
