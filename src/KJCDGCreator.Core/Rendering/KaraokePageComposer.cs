using KJCDGCreator.Core.Lyrics;
using KJCDGCreator.Core.Timing;

namespace KJCDGCreator.Core.Rendering;

public static class KaraokePageComposer
{
    public static ComposedKaraokePage Compose(
        LyricsDocument lyricsDocument,
        TimingDocument timing,
        TimeSpan playbackTime,
        PageLayoutOptions options)
    {
        ArgumentNullException.ThrowIfNull(lyricsDocument);
        ArgumentNullException.ThrowIfNull(timing);
        ArgumentNullException.ThrowIfNull(options);

        var selection = ActivePageSelector.SelectPage(lyricsDocument, timing, playbackTime);
        if (!selection.HasActivePage)
        {
            return new ComposedKaraokePage(selection, HighlightState: null, Layout: null);
        }

        var page = lyricsDocument.Pages[selection.PageIndex];
        var highlightState = HighlightProgressionEngine.GetPageState(lyricsDocument, selection.PageIndex, timing, playbackTime);
        var layout = PageLayoutEngine.LayoutPage(page, selection.PageIndex, options);

        return new ComposedKaraokePage(selection, highlightState, layout);
    }
}
