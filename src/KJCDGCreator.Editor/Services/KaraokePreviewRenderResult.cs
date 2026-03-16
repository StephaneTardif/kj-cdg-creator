using Avalonia.Media;

namespace KJCDGCreator.Editor.Services;

public sealed record KaraokePreviewRenderResult(
    IImage? Image,
    string StatusMessage,
    bool HasContent,
    byte[] PixelData,
    int PixelWidth,
    int PixelHeight);
