namespace KJCDGCreator.Core.Packaging;

public sealed record KaraokePackageOptions(
    string BaseFileName,
    bool CopyMp3,
    bool CopyCdg,
    bool CreateZip,
    bool OverwriteExisting);
