using KJCDGCreator.Core.Cdg;

namespace KJCDGCreator.Core.Packaging;

public sealed record KaraokeExportPipelineOptions(
    CdgTimelineExportOptions TimelineOptions,
    KaraokePackageOptions PackageOptions);
