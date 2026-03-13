using KJCDGCreator.Core.Rendering;

namespace KJCDGCreator.Core.Cdg;

public sealed record CdgTimelineExportOptions(
    TimeSpan FrameStep,
    KaraokeFrameRenderOptions FrameRenderOptions,
    TimeSpan? EndPadding,
    bool IncludeInitialClearFrame);
