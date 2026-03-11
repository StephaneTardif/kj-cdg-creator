namespace KJCDGCreator.Core.Lyrics;

public interface ILyricsParser
{
    IReadOnlyList<LyricLine> Parse(string text);
}
