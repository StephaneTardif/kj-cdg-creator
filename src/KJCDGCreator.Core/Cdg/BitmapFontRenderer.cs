namespace KJCDGCreator.Core.Cdg;

public sealed class BitmapFontRenderer
{
    private readonly CdgScreenBuffer _screenBuffer;
    private readonly BitmapFont _font;
    private readonly byte _backgroundColor;

    public BitmapFontRenderer(CdgScreenBuffer screenBuffer, BitmapFont? font = null, byte backgroundColor = 0)
    {
        _screenBuffer = screenBuffer ?? throw new ArgumentNullException(nameof(screenBuffer));
        _font = font ?? BitmapFont.Default;
        _backgroundColor = backgroundColor;
    }

    public void RenderText(string text, int tileX, int tileY, byte color)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (tileY < 0 || tileY >= CdgScreenBuffer.Rows)
        {
            return;
        }

        for (var index = 0; index < text.Length; index++)
        {
            var column = tileX + index;
            if (column < 0 || column >= CdgScreenBuffer.Columns)
            {
                continue;
            }

            var glyph = _font.GetGlyph(text[index]);
            var tile = new CdgTile(_backgroundColor, color, glyph);
            _screenBuffer.SetTile(tileY, column, tile);
        }
    }
}
