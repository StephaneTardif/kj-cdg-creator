namespace KJCDGCreator.Core.Rendering;

public sealed record PageLayoutOptions(
    int StartRow,
    int StartColumn,
    int LineSpacing,
    bool CenterHorizontally,
    bool CenterVertically);
