namespace KJCDGCreator.Core.Cdg;

public sealed record CdgPacketInfo(
    int Index,
    CdgPacketType Type,
    int? Row,
    int? Column,
    byte BackgroundColor,
    byte ForegroundColor,
    IReadOnlyList<byte> Bitmap);
