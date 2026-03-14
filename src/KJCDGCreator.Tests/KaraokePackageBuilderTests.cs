using KJCDGCreator.Core.Packaging;
using System.IO.Compression;

namespace KJCDGCreator.Tests;

public sealed class KaraokePackageBuilderTests : IDisposable
{
    private readonly string _workspace = Path.Combine(Path.GetTempPath(), "kj-cdg-creator-package-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public void BuildPackage_CopiesMp3AndCdgToMatchingBaseNames()
    {
        var (mp3Path, cdgPath) = CreateSourceFiles();
        var outputDirectory = Path.Combine(_workspace, "output");

        var result = KaraokePackageBuilder.BuildPackage(
            mp3Path,
            cdgPath,
            outputDirectory,
            new KaraokePackageOptions("demo-track", CopyMp3: true, CopyCdg: true, CreateZip: false, OverwriteExisting: true));

        Assert.Equal(Path.Combine(outputDirectory, "demo-track.mp3"), result.OutputMp3Path);
        Assert.Equal(Path.Combine(outputDirectory, "demo-track.cdg"), result.OutputCdgPath);
        Assert.True(File.Exists(result.OutputMp3Path));
        Assert.True(File.Exists(result.OutputCdgPath));
        Assert.Equal(File.ReadAllBytes(mp3Path), File.ReadAllBytes(result.OutputMp3Path!));
        Assert.Equal(File.ReadAllBytes(cdgPath), File.ReadAllBytes(result.OutputCdgPath!));
    }

    [Fact]
    public void BuildPackage_CreatesZipContainingBothFiles()
    {
        var (mp3Path, cdgPath) = CreateSourceFiles();
        var outputDirectory = Path.Combine(_workspace, "zip-output");

        var result = KaraokePackageBuilder.BuildPackage(
            mp3Path,
            cdgPath,
            outputDirectory,
            new KaraokePackageOptions("demo-track", CopyMp3: false, CopyCdg: false, CreateZip: true, OverwriteExisting: true));

        Assert.True(File.Exists(result.OutputZipPath));

        using var archive = ZipFile.OpenRead(result.OutputZipPath!);
        Assert.NotNull(archive.GetEntry("demo-track.mp3"));
        Assert.NotNull(archive.GetEntry("demo-track.cdg"));
    }

    [Fact]
    public void BuildPackage_RespectsOverwriteRules()
    {
        var (mp3Path, cdgPath) = CreateSourceFiles();
        var outputDirectory = Path.Combine(_workspace, "overwrite-output");

        KaraokePackageBuilder.BuildPackage(
            mp3Path,
            cdgPath,
            outputDirectory,
            new KaraokePackageOptions("demo-track", CopyMp3: true, CopyCdg: true, CreateZip: true, OverwriteExisting: true));

        var exception = Assert.Throws<IOException>(() =>
            KaraokePackageBuilder.BuildPackage(
                mp3Path,
                cdgPath,
                outputDirectory,
                new KaraokePackageOptions("demo-track", CopyMp3: true, CopyCdg: true, CreateZip: true, OverwriteExisting: false)));

        Assert.Contains("already exists", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildPackage_CreatesOutputDirectoryWhenMissing()
    {
        var (mp3Path, cdgPath) = CreateSourceFiles();
        var outputDirectory = Path.Combine(_workspace, "nested", "package");

        KaraokePackageBuilder.BuildPackage(
            mp3Path,
            cdgPath,
            outputDirectory,
            new KaraokePackageOptions("demo-track", CopyMp3: true, CopyCdg: true, CreateZip: false, OverwriteExisting: true));

        Assert.True(Directory.Exists(outputDirectory));
    }

    [Fact]
    public void BuildPackage_FailsClearlyWhenSourceFilesAreMissing()
    {
        var outputDirectory = Path.Combine(_workspace, "missing-output");

        var mp3Exception = Assert.Throws<FileNotFoundException>(() =>
            KaraokePackageBuilder.BuildPackage(
                Path.Combine(_workspace, "missing.mp3"),
                Path.Combine(_workspace, "missing.cdg"),
                outputDirectory,
                new KaraokePackageOptions("demo-track", CopyMp3: true, CopyCdg: true, CreateZip: false, OverwriteExisting: true)));

        Assert.Contains("MP3", mp3Exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildPackage_ReturnsDeterministicOutputPaths()
    {
        var (mp3Path, cdgPath) = CreateSourceFiles();
        var outputDirectory = Path.Combine(_workspace, "deterministic-output");
        var options = new KaraokePackageOptions("demo-track", CopyMp3: true, CopyCdg: true, CreateZip: true, OverwriteExisting: true);

        var first = KaraokePackageBuilder.BuildPackage(mp3Path, cdgPath, outputDirectory, options);
        var second = KaraokePackageBuilder.BuildPackage(mp3Path, cdgPath, outputDirectory, options);

        Assert.Equal(first.OutputMp3Path, second.OutputMp3Path);
        Assert.Equal(first.OutputCdgPath, second.OutputCdgPath);
        Assert.Equal(first.OutputZipPath, second.OutputZipPath);
    }

    public void Dispose()
    {
        if (Directory.Exists(_workspace))
        {
            Directory.Delete(_workspace, recursive: true);
        }
    }

    private (string Mp3Path, string CdgPath) CreateSourceFiles()
    {
        Directory.CreateDirectory(_workspace);
        var mp3Path = Path.Combine(_workspace, "source.mp3");
        var cdgPath = Path.Combine(_workspace, "source.cdg");

        File.WriteAllBytes(mp3Path, new byte[] { 0x49, 0x44, 0x33, 0x04, 0x00, 0x00 });
        File.WriteAllBytes(cdgPath, Enumerable.Range(0, 24).Select(value => (byte)value).ToArray());

        return (mp3Path, cdgPath);
    }
}
