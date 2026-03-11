namespace KJCDGCreator.Core.Lyrics;

public sealed record LyricLine(IReadOnlyList<LyricSyllable> Syllables)
{
    public string ToDisplayText() => string.Concat(Syllables.Select(syllable => syllable.Text));
}
