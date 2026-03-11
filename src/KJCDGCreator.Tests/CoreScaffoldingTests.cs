using KJCDGCreator.Core.Cdg;
using KJCDGCreator.Core.Lyrics;

namespace KJCDGCreator.Tests;

public sealed class CoreScaffoldingTests
{
    [Fact]
    public void LyricLine_ToDisplayText_ConcatenatesSyllables()
    {
        var line = new LyricLine(
            new[]
            {
                new LyricSyllable("Be", 0),
                new LyricSyllable("cause", 1),
                new LyricSyllable(" happy", 2)
            });

        Assert.Equal("Because happy", line.ToDisplayText());
    }

    [Fact]
    public void CdgPalette_StoresConfiguredColors()
    {
        var palette = new CdgPalette(Background: 0, Lyrics: 15, Highlight: 12);

        Assert.Equal((byte)0, palette.Background);
        Assert.Equal((byte)15, palette.Lyrics);
        Assert.Equal((byte)12, palette.Highlight);
    }
}
