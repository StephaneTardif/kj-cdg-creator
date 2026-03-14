using KJCDGCreator.Core.Cdg;
using KJCDGCreator.Core.Lyrics;
using KJCDGCreator.Core.Timing;

namespace KJCDGCreator.Core.Packaging;

public static class KaraokeExportPipeline
{
    public static KaraokeExportPipelineResult ExportAndPackage(
        LyricsDocument lyricsDocument,
        TimingDocument timing,
        string sourceMp3Path,
        string outputDirectory,
        KaraokeExportPipelineOptions options)
    {
        ArgumentNullException.ThrowIfNull(lyricsDocument);
        ArgumentNullException.ThrowIfNull(timing);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceMp3Path);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        ArgumentNullException.ThrowIfNull(options);

        Directory.CreateDirectory(outputDirectory);

        var tempCdgPath = Path.Combine(outputDirectory, $"{options.PackageOptions.BaseFileName}.timeline.cdg");
        var exportResult = CdgTimelineExporter.Export(lyricsDocument, timing, tempCdgPath, options.TimelineOptions);
        var packageResult = KaraokePackageBuilder.BuildPackage(sourceMp3Path, tempCdgPath, outputDirectory, options.PackageOptions);

        return new KaraokeExportPipelineResult(exportResult, packageResult);
    }
}
