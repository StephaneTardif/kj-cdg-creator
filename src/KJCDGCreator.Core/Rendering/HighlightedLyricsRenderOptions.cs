namespace KJCDGCreator.Core.Rendering;

public sealed record HighlightedLyricsRenderOptions(
    int StartRow,
    int StartColumn,
    int LineSpacing,
    byte BackgroundColor,
    byte BaseTextColor,
    byte HighlightTextColor,
    bool ClearScreenBeforeRender);
