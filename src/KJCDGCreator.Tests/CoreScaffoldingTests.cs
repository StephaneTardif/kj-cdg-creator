using KJCDGCreator.Core.Cdg;

namespace KJCDGCreator.Tests;

public sealed class CoreScaffoldingTests
{
    [Fact]
    public void CdgPalette_StoresConfiguredColors()
    {
        var palette = new CdgPalette(Background: 0, Lyrics: 15, Highlight: 12);

        Assert.Equal((byte)0, palette.Background);
        Assert.Equal((byte)15, palette.Lyrics);
        Assert.Equal((byte)12, palette.Highlight);
    }
}
