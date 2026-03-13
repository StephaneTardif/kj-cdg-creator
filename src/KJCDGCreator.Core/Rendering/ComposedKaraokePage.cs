namespace KJCDGCreator.Core.Rendering;

public sealed record ComposedKaraokePage(
    PageSelectionResult Selection,
    PageHighlightState? HighlightState,
    RenderedPageLayout? Layout);
