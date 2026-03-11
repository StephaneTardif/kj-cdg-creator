namespace KJCDGCreator.Core.Cdg;

public sealed record CdgRenderFrame(TimeSpan Timestamp, IReadOnlyList<string> VisibleLines);
