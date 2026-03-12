using KJCDGCreator.Core.Lyrics;

namespace KJCDGCreator.Tests;

public sealed class LyricsParserTests
{
    [Fact]
    public void Parse_SplitsSimpleSyllablesInReadingOrder()
    {
        var document = LyricsParser.Parse("Be|cause I'm hap|py");
        var line = Assert.Single(Assert.Single(document.Pages).Lines);

        Assert.Equal("Be|cause I'm hap|py", line.RawText);
        Assert.Equal("Because I'm happy", line.DisplayText);
        Assert.Collection(
            line.Units,
            unit =>
            {
                Assert.Equal("Be", unit.Text);
                Assert.Equal(0, unit.DisplayStartIndex);
                Assert.Equal(2, unit.DisplayLength);
            },
            unit =>
            {
                Assert.Equal("cause I'm hap", unit.Text);
                Assert.Equal(2, unit.DisplayStartIndex);
                Assert.Equal(13, unit.DisplayLength);
            },
            unit =>
            {
                Assert.Equal("py", unit.Text);
                Assert.Equal(15, unit.DisplayStartIndex);
                Assert.Equal(2, unit.DisplayLength);
            });
    }

    [Fact]
    public void Parse_KeepsPunctuationAttachedToUnits()
    {
        var document = LyricsParser.Parse("hap|py,");
        var line = Assert.Single(Assert.Single(document.Pages).Lines);

        Assert.Equal("happy,", line.DisplayText);
        Assert.Collection(
            line.Units,
            unit => Assert.Equal("hap", unit.Text),
            unit => Assert.Equal("py,", unit.Text));
    }

    [Fact]
    public void Parse_SupportsMultipleLinesOnOnePage()
    {
        var document = LyricsParser.Parse("Be|cause I'm hap|py\nClap a|long if you feel");
        var page = Assert.Single(document.Pages);

        Assert.Equal(2, page.Lines.Count);
        Assert.Equal("Because I'm happy", page.Lines[0].DisplayText);
        Assert.Equal("Clap along if you feel", page.Lines[1].DisplayText);
    }

    [Fact]
    public void Parse_UsesBlankLinesAsPageBreaks()
    {
        var document = LyricsParser.Parse("First| line\nSecond line\n\nThird| page");

        Assert.Equal(2, document.Pages.Count);
        Assert.Equal(2, document.Pages[0].Lines.Count);
        Assert.Single(document.Pages[1].Lines);
        Assert.Equal("Third page", document.Pages[1].Lines[0].DisplayText);
    }

    [Fact]
    public void Parse_TrimsLeadingAndTrailingWhitespacePerLine()
    {
        var document = LyricsParser.Parse("   Be|cause I'm hap|py   ");
        var line = Assert.Single(Assert.Single(document.Pages).Lines);

        Assert.Equal("Be|cause I'm hap|py", line.RawText);
        Assert.Equal("Because I'm happy", line.DisplayText);
    }
}
