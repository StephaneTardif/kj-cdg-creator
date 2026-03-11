namespace KJCDGCreator.Editor.ViewModels;

public sealed class MainWindowViewModel
{
    public string LyricsDraft { get; } = "Be|cause I'm hap|py";

    public string LyricsHint { get; } = "Use the | character to mark syllable boundaries before entering tap timing mode.";

    public string TimingHint { get; } = "Space-bar tap capture will be connected to audio playback in a later implementation step.";

    public string StatusMessage { get; } = "Solution scaffold created. Rendering, timing, and export logic are intentionally not implemented yet.";
}
