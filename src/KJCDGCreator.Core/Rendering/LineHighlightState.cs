namespace KJCDGCreator.Core.Rendering;

public sealed record LineHighlightState(
    string DisplayText,
    IReadOnlyList<HighlightRange> CompletedRanges,
    HighlightRange? ActiveRange);
