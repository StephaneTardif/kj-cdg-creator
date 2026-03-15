using KJCDGCreator.Core.Cdg;

namespace KJCDGCreator.Core.Rendering;

public static class IntroTitleScreenRenderer
{
    public static void Render(
        KaraokeSongMetadata metadata,
        CdgScreenBuffer screenBuffer,
        IntroTitleScreenOptions options)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(screenBuffer);
        ArgumentNullException.ThrowIfNull(options);

        screenBuffer.Clear(CdgScreenBuffer.CreateBlankTile(options.BackgroundColor));

        var fontRenderer = new BitmapFontRenderer(screenBuffer, backgroundColor: options.BackgroundColor);

        RenderLine(metadata.Title, options.TitleRow, options.TitleColor, fontRenderer, options);
        RenderLine(metadata.Artist, options.ArtistRow, options.ArtistColor, fontRenderer, options);
    }

    private static void RenderLine(
        string text,
        int row,
        byte color,
        BitmapFontRenderer fontRenderer,
        IntroTitleScreenOptions options)
    {
        if (string.IsNullOrWhiteSpace(text) || row < 0 || row >= CdgScreenBuffer.Rows)
        {
            return;
        }

        var trimmed = text.Trim();
        var column = options.CenterHorizontally
            ? CalculateCenteredColumn(trimmed.Length)
            : 0;

        fontRenderer.RenderText(trimmed, column, row, color);
    }

    private static int CalculateCenteredColumn(int textLength)
    {
        if (textLength >= CdgScreenBuffer.Columns)
        {
            return 0;
        }

        return Math.Max(0, (CdgScreenBuffer.Columns - textLength) / 2);
    }
}
