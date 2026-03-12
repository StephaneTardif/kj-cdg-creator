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

        for (var index = 0; index < text.Length; index++)
        {
            var glyph = _font.GetGlyph(text[index]);
            var tile = new CdgTile(_backgroundColor, color, glyph);
            _screenBuffer.SetTile(tileY, tileX + index, tile);
        }
    }
}
