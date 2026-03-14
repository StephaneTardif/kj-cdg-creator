using KJCDGCreator.Core.Cdg;

namespace KJCDGCreator.Core.Packaging;

public sealed record KaraokeExportPipelineResult(
    CdgFrameSequenceResult ExportResult,
    KaraokePackageResult PackageResult);
