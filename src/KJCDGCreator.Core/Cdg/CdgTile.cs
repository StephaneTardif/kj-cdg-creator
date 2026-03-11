namespace KJCDGCreator.Core.Cdg;

internal sealed class CdgTile
{
    public CdgTile(byte backgroundColor, byte foregroundColor, byte[] rows)
    {
        if (rows.Length != 12)
        {
            throw new ArgumentException("A CDG tile must contain exactly 12 rows.", nameof(rows));
        }

        BackgroundColor = backgroundColor;
        ForegroundColor = foregroundColor;
        Rows = rows;
    }

    public byte BackgroundColor { get; }

    public byte ForegroundColor { get; }

    public byte[] Rows { get; }
}
