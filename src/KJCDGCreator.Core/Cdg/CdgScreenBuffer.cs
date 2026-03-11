namespace KJCDGCreator.Core.Cdg;

public sealed class CdgScreenBuffer
{
    public const int Columns = 50;
    public const int Rows = 18;

    private readonly CdgTile[,] _tiles = new CdgTile[Rows, Columns];

    public CdgScreenBuffer()
        : this(CreateBlankTile(backgroundColor: 0))
    {
    }

    public CdgScreenBuffer(CdgTile defaultTile)
    {
        ArgumentNullException.ThrowIfNull(defaultTile);

        DefaultTile = defaultTile;

        for (var row = 0; row < Rows; row++)
        {
            for (var column = 0; column < Columns; column++)
            {
                _tiles[row, column] = defaultTile;
            }
        }
    }

    public CdgTile DefaultTile { get; }

    public CdgTile GetTile(int row, int column)
    {
        ValidatePosition(row, column);
        return _tiles[row, column];
    }

    public void SetTile(int row, int column, CdgTile tile)
    {
        ValidatePosition(row, column);
        ArgumentNullException.ThrowIfNull(tile);
        _tiles[row, column] = tile;
    }

    public void Clear(CdgTile tile)
    {
        ArgumentNullException.ThrowIfNull(tile);

        for (var row = 0; row < Rows; row++)
        {
            for (var column = 0; column < Columns; column++)
            {
                _tiles[row, column] = tile;
            }
        }
    }

    public CdgScreenBuffer Clone()
    {
        var clone = new CdgScreenBuffer(DefaultTile);

        for (var row = 0; row < Rows; row++)
        {
            for (var column = 0; column < Columns; column++)
            {
                clone._tiles[row, column] = _tiles[row, column];
            }
        }

        return clone;
    }

    public static CdgTile CreateBlankTile(byte backgroundColor) =>
        new(backgroundColor, backgroundColor, Enumerable.Repeat((byte)0, CdgTile.Height).ToArray());

    private static void ValidatePosition(int row, int column)
    {
        if (row is < 0 or >= Rows)
        {
            throw new ArgumentOutOfRangeException(nameof(row));
        }

        if (column is < 0 or >= Columns)
        {
            throw new ArgumentOutOfRangeException(nameof(column));
        }
    }
}
