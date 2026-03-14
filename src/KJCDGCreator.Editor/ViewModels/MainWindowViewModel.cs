namespace KJCDGCreator.Editor.ViewModels;

public sealed class MainWindowViewModel
{
    public string LyricsDraft { get; } = "Be|cause I'm hap|py";

    public string LyricsHint { get; } = "Spaces create word timing units automatically. Use | only to split inside a word, for example: Be|cause I'm hap|py.";

    public string TimingHint { get; } = "Space-bar tap capture will be connected to audio playback in a later implementation step.";

    public string StatusMessage { get; } = "Solution scaffold created. Rendering, timing, and export logic are intentionally not implemented yet.";
}
