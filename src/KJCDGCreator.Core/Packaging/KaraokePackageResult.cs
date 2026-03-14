namespace KJCDGCreator.Core.Packaging;

public sealed record KaraokePackageResult(
    string? OutputMp3Path,
    string? OutputCdgPath,
    string? OutputZipPath);
