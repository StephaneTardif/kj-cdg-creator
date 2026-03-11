namespace KJCDGCreator.Core.Cdg;

public sealed class CdgTile : IEquatable<CdgTile>
{
    public const int Width = 6;
    public const int Height = 12;

    private readonly byte[] _bitmap;

    public CdgTile(byte backgroundColor, byte foregroundColor, IReadOnlyList<byte> bitmap)
    {
        ArgumentNullException.ThrowIfNull(bitmap);

        if (bitmap.Count != Height)
        {
            throw new ArgumentException("A CDG tile must contain exactly 12 rows.", nameof(bitmap));
        }

        BackgroundColor = backgroundColor;
        ForegroundColor = foregroundColor;
        _bitmap = bitmap.Select(row => (byte)(row & 0x3F)).ToArray();
    }

    public byte BackgroundColor { get; }

    public byte ForegroundColor { get; }

    public IReadOnlyList<byte> Bitmap => _bitmap;

    public bool Equals(CdgTile? other)
    {
        if (other is null)
        {
            return false;
        }

        return BackgroundColor == other.BackgroundColor
            && ForegroundColor == other.ForegroundColor
            && _bitmap.SequenceEqual(other._bitmap);
    }

    public override bool Equals(object? obj) => Equals(obj as CdgTile);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(BackgroundColor);
        hash.Add(ForegroundColor);

        foreach (var row in _bitmap)
        {
            hash.Add(row);
        }

        return hash.ToHashCode();
    }
}
