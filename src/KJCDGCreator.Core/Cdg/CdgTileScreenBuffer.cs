namespace KJCDGCreator.Core.Cdg;

internal sealed class CdgTileScreenBuffer
{
    private const int ColumnCount = 50;
    private const int RowCount = 18;

    private readonly Dictionary<(int Row, int Column), CdgTile> _tiles = new();

    public void SetTile(int row, int column, CdgTile tile)
    {
        if (row is < 0 or >= RowCount)
        {
            throw new ArgumentOutOfRangeException(nameof(row));
        }

        if (column is < 0 or >= ColumnCount)
        {
            throw new ArgumentOutOfRangeException(nameof(column));
        }

        _tiles[(row, column)] = tile;
    }

    public IEnumerable<CdgPacket> RenderPackets(byte backgroundColor)
    {
        yield return CreateMemoryPresetPacket(backgroundColor);

        foreach (var ((row, column), tile) in _tiles.OrderBy(entry => entry.Key.Row).ThenBy(entry => entry.Key.Column))
        {
            yield return CreateTileBlockPacket(row, column, tile);
        }
    }

    private static CdgPacket CreateMemoryPresetPacket(byte color)
    {
        var packet = new CdgPacket(CdgCommand.MemoryPreset);
        packet.Data[0] = color;
        packet.Data[1] = 0;
        return packet;
    }

    private static CdgPacket CreateTileBlockPacket(int row, int column, CdgTile tile)
    {
        var packet = new CdgPacket(CdgCommand.TileBlockNormal);
        packet.Data[0] = tile.BackgroundColor;
        packet.Data[1] = tile.ForegroundColor;
        packet.Data[2] = (byte)row;
        packet.Data[3] = (byte)column;

        for (var index = 0; index < tile.Rows.Length; index++)
        {
            packet.Data[index + 4] = tile.Rows[index];
        }

        return packet;
    }
}
