namespace KJCDGCreator.Core.Cdg;

internal sealed class CdgPacket
{
    private const byte CommandMarker = 0x09;
    private const int DataLength = 16;

    private readonly byte[] _data = new byte[DataLength];

    public CdgPacket(CdgCommand command)
    {
        Command = command;
    }

    public CdgCommand Command { get; }

    public Span<byte> Data => _data;

    public byte[] ToBytes()
    {
        var packet = new byte[24];
        packet[0] = CommandMarker;
        packet[1] = (byte)Command;

        for (var index = 0; index < DataLength; index++)
        {
            packet[index + 4] = (byte)(_data[index] & 0x3F);
        }

        return packet;
    }
}
