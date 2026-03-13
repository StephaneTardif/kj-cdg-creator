using KJCDGCreator.Core.Cdg;
using KJCDGCreator.Core.Lyrics;
using KJCDGCreator.Core.Timing;

namespace KJCDGCreator.Core.Rendering;

public static class KaraokeFrameRenderer
{
    public static RenderedFrameResult RenderFrame(
        LyricsDocument lyricsDocument,
        TimingDocument timing,
        TimeSpan playbackTime,
        CdgScreenBuffer screenBuffer,
        KaraokeFrameRenderOptions options)
    {
        ArgumentNullException.ThrowIfNull(lyricsDocument);
        ArgumentNullException.ThrowIfNull(timing);
        ArgumentNullException.ThrowIfNull(screenBuffer);
        ArgumentNullException.ThrowIfNull(options);

        if (lyricsDocument.Pages.Count == 0)
        {
            if (options.HighlightOptions.ClearScreenBeforeRender)
            {
                screenBuffer.Clear(CdgScreenBuffer.CreateBlankTile(options.HighlightOptions.BackgroundColor));
            }

            return new RenderedFrameResult(PageIndex: -1, HasContent: false);
        }

        var composed = KaraokePageComposer.Compose(lyricsDocument, timing, playbackTime, options.LayoutOptions);
        if (!composed.Selection.HasActivePage || composed.HighlightState is null || composed.Layout is null)
        {
            if (options.HighlightOptions.ClearScreenBeforeRender)
            {
                screenBuffer.Clear(CdgScreenBuffer.CreateBlankTile(options.HighlightOptions.BackgroundColor));
            }

            return new RenderedFrameResult(PageIndex: -1, HasContent: false);
        }

        HighlightedLyricsRenderer.RenderPage(
            composed.HighlightState,
            composed.Layout,
            screenBuffer,
            options.HighlightOptions);

        var hasContent = composed.Layout.Lines.Count > 0;
        return new RenderedFrameResult(composed.Selection.PageIndex, hasContent);
    }
}
