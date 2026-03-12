namespace KJCDGCreator.Core.Cdg;

public static class CdgInspector
{
    public static IReadOnlyList<CdgPacketInfo> ReadPackets(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var bytes = File.ReadAllBytes(path);
        if (bytes.Length % 24 != 0)
        {
            throw new InvalidDataException("A CDG file must be composed of 24-byte packets.");
        }

        var packets = new List<CdgPacketInfo>(bytes.Length / 24);

        for (var index = 0; index < bytes.Length; index += 24)
        {
            packets.Add(ParsePacket(bytes.AsSpan(index, 24), index / 24));
        }

        return packets;
    }

    public static CdgScreenBuffer BuildScreenBuffer(string path)
    {
        var screen = new CdgScreenBuffer();

        foreach (var packet in ReadPackets(path))
        {
            switch (packet.Type)
            {
                case CdgPacketType.MemoryPreset:
                    screen.Clear(CdgScreenBuffer.CreateBlankTile(packet.BackgroundColor));
                    break;
                case CdgPacketType.TileBlockNormal when packet.Row is not null && packet.Column is not null:
                    screen.SetTile(
                        packet.Row.Value,
                        packet.Column.Value,
                        new CdgTile(packet.BackgroundColor, packet.ForegroundColor, packet.Bitmap));
                    break;
            }
        }

        return screen;
    }

    public static string RenderAsciiPreview(string path, BitmapFont? font = null)
    {
        var screen = BuildScreenBuffer(path);
        var activeFont = font ?? BitmapFont.Default;
        var lines = new List<string>(CdgScreenBuffer.Rows);

        for (var row = 0; row < CdgScreenBuffer.Rows; row++)
        {
            var characters = new char[CdgScreenBuffer.Columns];

            for (var column = 0; column < CdgScreenBuffer.Columns; column++)
            {
                var tile = screen.GetTile(row, column);
                characters[column] = activeFont.TryGetCharacter(tile.Bitmap, out var value)
                    ? value
                    : tile.Bitmap.Any(pixelRow => pixelRow != 0) ? '#' : ' ';
            }

            lines.Add(new string(characters).TrimEnd());
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static CdgPacketInfo ParsePacket(ReadOnlySpan<byte> packet, int index)
    {
        var command = packet[0] & 0x3F;
        var instruction = packet[1] & 0x3F;
        var data = packet.Slice(4, 16).ToArray();

        if (command != 0x09)
        {
            return new CdgPacketInfo(index, CdgPacketType.Unknown, null, null, 0, 0, Array.Empty<byte>());
        }

        return (CdgPacketType)instruction switch
        {
            CdgPacketType.MemoryPreset => new CdgPacketInfo(
                index,
                CdgPacketType.MemoryPreset,
                null,
                null,
                BackgroundColor: (byte)(data[0] & 0x0F),
                ForegroundColor: 0,
                Bitmap: Array.Empty<byte>()),
            CdgPacketType.TileBlockNormal => new CdgPacketInfo(
                index,
                CdgPacketType.TileBlockNormal,
                Row: data[2] & 0x1F,
                Column: data[3] & 0x3F,
                BackgroundColor: (byte)(data[0] & 0x0F),
                ForegroundColor: (byte)(data[1] & 0x0F),
                Bitmap: data.Skip(4).Select(value => (byte)(value & 0x3F)).ToArray()),
            _ => new CdgPacketInfo(index, CdgPacketType.Unknown, null, null, 0, 0, Array.Empty<byte>())
        };
    }
}
