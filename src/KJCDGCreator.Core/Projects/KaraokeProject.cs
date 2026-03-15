using KJCDGCreator.Core.Cdg;
using KJCDGCreator.Core.Lyrics;
using KJCDGCreator.Core.Rendering;
using KJCDGCreator.Core.Timing;

namespace KJCDGCreator.Core.Projects;

public sealed record KaraokeProject(
    string ProjectVersion,
    string? Title,
    string? Artist,
    string RawLyricsText,
    TimingDocument Timing,
    string? SourceMp3Path,
    IntroTitleScreenOptions? IntroOptions,
    KaraokeFrameRenderOptions? FrameRenderOptions,
    CdgTimelineExportOptions? ExportOptions)
{
    public LyricsDocument BuildLyricsDocument() => LyricsParser.Parse(RawLyricsText);
}
