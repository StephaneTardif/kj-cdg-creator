namespace KJCDGCreator.Core.Lyrics;

public sealed record LyricsLine(
    string RawText,
    string DisplayText,
    IReadOnlyList<LyricsUnit> Units);
