namespace KJCDGCreator.Core.Cdg;

public static class CdgScreenBufferRenderer
{
    public static IReadOnlyList<CdgPacket> RenderFullScreen(CdgScreenBuffer screen, byte backgroundColor)
    {
        ArgumentNullException.ThrowIfNull(screen);

        var packets = new List<CdgPacket>
        {
            CreateMemoryPresetPacket(backgroundColor)
        };

        packets.AddRange(RenderTileChanges(new CdgScreenBuffer(CdgScreenBuffer.CreateBlankTile(backgroundColor)), screen));
        return packets;
    }

    public static IReadOnlyList<CdgPacket> RenderTileChanges(CdgScreenBuffer previous, CdgScreenBuffer current)
    {
        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(current);

        var packets = new List<CdgPacket>();

        for (var row = 0; row < CdgScreenBuffer.Rows; row++)
        {
            for (var column = 0; column < CdgScreenBuffer.Columns; column++)
            {
                var oldTile = previous.GetTile(row, column);
                var newTile = current.GetTile(row, column);

                if (!newTile.Equals(oldTile))
                {
                    packets.Add(CreateTileBlockPacket(row, column, newTile));
                }
            }
        }

        return packets;
    }

    public static CdgPacket CreateMemoryPresetPacket(byte color)
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

        for (var index = 0; index < tile.Bitmap.Count; index++)
        {
            packet.Data[index + 4] = tile.Bitmap[index];
        }

        return packet;
    }
}
