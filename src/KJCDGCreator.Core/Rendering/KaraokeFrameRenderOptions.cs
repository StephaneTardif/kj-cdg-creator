namespace KJCDGCreator.Core.Rendering;

public sealed record KaraokeFrameRenderOptions(
    PageLayoutOptions LayoutOptions,
    HighlightedLyricsRenderOptions HighlightOptions,
    KaraokeSongMetadata? SongMetadata = null,
    IntroTitleScreenOptions? IntroOptions = null);
