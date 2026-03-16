using KJCDGCreator.Core.Projects;

namespace KJCDGCreator.Editor.Services;

public interface IKaraokePreviewRenderer
{
    KaraokePreviewRenderResult Render(KaraokeProject? project, TimeSpan playbackTime);
}
