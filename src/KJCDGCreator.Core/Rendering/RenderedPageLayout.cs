namespace KJCDGCreator.Core.Rendering;

public sealed record RenderedPageLayout(int PageIndex, IReadOnlyList<RenderedLineLayout> Lines);
