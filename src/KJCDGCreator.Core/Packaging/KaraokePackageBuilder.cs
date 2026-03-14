using System.IO.Compression;

namespace KJCDGCreator.Core.Packaging;

public static class KaraokePackageBuilder
{
    public static KaraokePackageResult BuildPackage(
        string sourceMp3Path,
        string sourceCdgPath,
        string outputDirectory,
        KaraokePackageOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceMp3Path);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCdgPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.BaseFileName);

        if (!File.Exists(sourceMp3Path))
        {
            throw new FileNotFoundException("Source MP3 file was not found.", sourceMp3Path);
        }

        if (!File.Exists(sourceCdgPath))
        {
            throw new FileNotFoundException("Source CDG file was not found.", sourceCdgPath);
        }

        Directory.CreateDirectory(outputDirectory);

        var outputMp3Path = options.CopyMp3 ? Path.Combine(outputDirectory, $"{options.BaseFileName}.mp3") : null;
        var outputCdgPath = options.CopyCdg ? Path.Combine(outputDirectory, $"{options.BaseFileName}.cdg") : null;
        var outputZipPath = options.CreateZip ? Path.Combine(outputDirectory, $"{options.BaseFileName}.zip") : null;

        EnsureOverwriteAllowed(outputMp3Path, options.OverwriteExisting);
        EnsureOverwriteAllowed(outputCdgPath, options.OverwriteExisting);
        EnsureOverwriteAllowed(outputZipPath, options.OverwriteExisting);

        if (outputMp3Path is not null)
        {
            File.Copy(sourceMp3Path, outputMp3Path, overwrite: options.OverwriteExisting);
        }

        if (outputCdgPath is not null)
        {
            File.Copy(sourceCdgPath, outputCdgPath, overwrite: options.OverwriteExisting);
        }

        if (outputZipPath is not null)
        {
            BuildZip(outputZipPath, sourceMp3Path, sourceCdgPath, options.BaseFileName, options.OverwriteExisting);
        }

        return new KaraokePackageResult(outputMp3Path, outputCdgPath, outputZipPath);
    }

    private static void EnsureOverwriteAllowed(string? outputPath, bool overwriteExisting)
    {
        if (!string.IsNullOrWhiteSpace(outputPath) && File.Exists(outputPath) && !overwriteExisting)
        {
            throw new IOException($"Output file already exists: {outputPath}");
        }
    }

    private static void BuildZip(
        string outputZipPath,
        string sourceMp3Path,
        string sourceCdgPath,
        string baseFileName,
        bool overwriteExisting)
    {
        if (File.Exists(outputZipPath) && overwriteExisting)
        {
            File.Delete(outputZipPath);
        }

        using var archive = ZipFile.Open(outputZipPath, ZipArchiveMode.Create);
        AddEntry(archive, sourceMp3Path, $"{baseFileName}.mp3");
        AddEntry(archive, sourceCdgPath, $"{baseFileName}.cdg");
    }

    private static void AddEntry(ZipArchive archive, string sourcePath, string entryName)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.NoCompression);
        entry.LastWriteTime = new DateTimeOffset(1980, 1, 1, 0, 0, 0, TimeSpan.Zero);

        using var source = File.OpenRead(sourcePath);
        using var destination = entry.Open();
        source.CopyTo(destination);
    }
}
