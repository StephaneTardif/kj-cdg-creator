using KJCDGCreator.Core.Cdg;
using KJCDGCreator.Core.Lyrics;
using KJCDGCreator.Core.Timing;

namespace KJCDGCreator.Core.Rendering;

public interface ICdgRenderer
{
    IReadOnlyList<CdgRenderFrame> Render(
        IReadOnlyList<LyricLine> lyrics,
        TimingSession timing,
        CdgPalette palette);
}
