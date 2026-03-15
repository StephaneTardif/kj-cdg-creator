using System.Text.Json;
using System.Text.Json.Serialization;
using KJCDGCreator.Core.Cdg;
using KJCDGCreator.Core.Lyrics;
using KJCDGCreator.Core.Rendering;
using KJCDGCreator.Core.Timing;

namespace KJCDGCreator.Core.Projects;

public static class KaraokeProjectSerializer
{
    public const string CurrentProjectVersion = "1.0";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static void Save(KaraokeProject project, string path)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var dto = ProjectDto.FromProject(project);
        var json = JsonSerializer.Serialize(dto, JsonOptions);
        File.WriteAllText(path, json);
    }

    public static KaraokeProject Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var json = File.ReadAllText(path);
        var dto = JsonSerializer.Deserialize<ProjectDto>(json, JsonOptions)
            ?? throw new InvalidDataException("Project file could not be deserialized.");

        return dto.ToProject();
    }

    private sealed class ProjectDto
    {
        [JsonPropertyOrder(1)]
        public string? ProjectVersion { get; init; }

        [JsonPropertyOrder(2)]
        public string? Title { get; init; }

        [JsonPropertyOrder(3)]
        public string? Artist { get; init; }

        [JsonPropertyOrder(4)]
        public string? RawLyricsText { get; init; }

        [JsonPropertyOrder(5)]
        public TimingDocumentDto? Timing { get; init; }

        [JsonPropertyOrder(6)]
        public string? SourceMp3Path { get; init; }

        [JsonPropertyOrder(7)]
        public IntroTitleScreenOptionsDto? IntroOptions { get; init; }

        [JsonPropertyOrder(8)]
        public KaraokeFrameRenderOptionsDto? FrameRenderOptions { get; init; }

        [JsonPropertyOrder(9)]
        public CdgTimelineExportOptionsDto? ExportOptions { get; init; }

        public KaraokeProject ToProject()
        {
            var rawLyricsText = RawLyricsText ?? string.Empty;
            var timing = Timing?.ToTimingDocument() ?? TimingDocumentBuilder.FromLyrics(LyricsParser.Parse(rawLyricsText));

            return new KaraokeProject(
                ProjectVersion: string.IsNullOrWhiteSpace(ProjectVersion) ? CurrentProjectVersion : ProjectVersion,
                Title: Title,
                Artist: Artist,
                RawLyricsText: rawLyricsText,
                Timing: timing,
                SourceMp3Path: SourceMp3Path,
                IntroOptions: IntroOptions?.ToModel(),
                FrameRenderOptions: FrameRenderOptions?.ToModel(),
                ExportOptions: ExportOptions?.ToModel());
        }

        public static ProjectDto FromProject(KaraokeProject project) =>
            new()
            {
                ProjectVersion = string.IsNullOrWhiteSpace(project.ProjectVersion) ? CurrentProjectVersion : project.ProjectVersion,
                Title = project.Title,
                Artist = project.Artist,
                RawLyricsText = project.RawLyricsText,
                Timing = TimingDocumentDto.FromModel(project.Timing),
                SourceMp3Path = project.SourceMp3Path,
                IntroOptions = IntroTitleScreenOptionsDto.FromModel(project.IntroOptions),
                FrameRenderOptions = KaraokeFrameRenderOptionsDto.FromModel(project.FrameRenderOptions),
                ExportOptions = CdgTimelineExportOptionsDto.FromModel(project.ExportOptions)
            };
    }

    private sealed class TimingDocumentDto
    {
        [JsonPropertyOrder(1)]
        public List<TimedUnitDto> Units { get; init; } = new();

        public TimingDocument ToTimingDocument() =>
            new(Units.Select(unit => unit.ToModel()));

        public static TimingDocumentDto FromModel(TimingDocument timing) =>
            new()
            {
                Units = timing.Units.Select(TimedUnitDto.FromModel).ToList()
            };
    }

    private sealed class TimedUnitDto
    {
        [JsonPropertyOrder(1)]
        public int UnitIndex { get; init; }

        [JsonPropertyOrder(2)]
        public string Text { get; init; } = string.Empty;

        [JsonPropertyOrder(3)]
        public TimeSpan? Timestamp { get; init; }

        public TimedUnit ToModel() => new(UnitIndex, Text, Timestamp);

        public static TimedUnitDto FromModel(TimedUnit unit) =>
            new()
            {
                UnitIndex = unit.UnitIndex,
                Text = unit.Text,
                Timestamp = unit.Timestamp
            };
    }

    private sealed class IntroTitleScreenOptionsDto
    {
        [JsonPropertyOrder(1)]
        public bool Enabled { get; init; }

        [JsonPropertyOrder(2)]
        public TimeSpan? FixedDuration { get; init; }

        [JsonPropertyOrder(3)]
        public bool UseFirstLyricTimestampWhenLonger { get; init; }

        [JsonPropertyOrder(4)]
        public byte BackgroundColor { get; init; }

        [JsonPropertyOrder(5)]
        public byte TitleColor { get; init; }

        [JsonPropertyOrder(6)]
        public byte ArtistColor { get; init; }

        [JsonPropertyOrder(7)]
        public int TitleRow { get; init; }

        [JsonPropertyOrder(8)]
        public int ArtistRow { get; init; }

        [JsonPropertyOrder(9)]
        public bool CenterHorizontally { get; init; }

        public IntroTitleScreenOptions ToModel() =>
            new(
                Enabled,
                FixedDuration,
                UseFirstLyricTimestampWhenLonger,
                BackgroundColor,
                TitleColor,
                ArtistColor,
                TitleRow,
                ArtistRow,
                CenterHorizontally);

        public static IntroTitleScreenOptionsDto? FromModel(IntroTitleScreenOptions? options)
        {
            if (options is null)
            {
                return null;
            }

            return new IntroTitleScreenOptionsDto
            {
                Enabled = options.Enabled,
                FixedDuration = options.FixedDuration,
                UseFirstLyricTimestampWhenLonger = options.UseFirstLyricTimestampWhenLonger,
                BackgroundColor = options.BackgroundColor,
                TitleColor = options.TitleColor,
                ArtistColor = options.ArtistColor,
                TitleRow = options.TitleRow,
                ArtistRow = options.ArtistRow,
                CenterHorizontally = options.CenterHorizontally
            };
        }
    }

    private sealed class KaraokeFrameRenderOptionsDto
    {
        [JsonPropertyOrder(1)]
        public PageLayoutOptionsDto? LayoutOptions { get; init; }

        [JsonPropertyOrder(2)]
        public HighlightedLyricsRenderOptionsDto? HighlightOptions { get; init; }

        [JsonPropertyOrder(3)]
        public KaraokeSongMetadataDto? SongMetadata { get; init; }

        [JsonPropertyOrder(4)]
        public IntroTitleScreenOptionsDto? IntroOptions { get; init; }

        public KaraokeFrameRenderOptions? ToModel()
        {
            if (LayoutOptions is null || HighlightOptions is null)
            {
                return null;
            }

            return new KaraokeFrameRenderOptions(
                LayoutOptions.ToModel(),
                HighlightOptions.ToModel(),
                SongMetadata?.ToModel(),
                IntroOptions?.ToModel());
        }

        public static KaraokeFrameRenderOptionsDto? FromModel(KaraokeFrameRenderOptions? options)
        {
            if (options is null)
            {
                return null;
            }

            return new KaraokeFrameRenderOptionsDto
            {
                LayoutOptions = PageLayoutOptionsDto.FromModel(options.LayoutOptions),
                HighlightOptions = HighlightedLyricsRenderOptionsDto.FromModel(options.HighlightOptions),
                SongMetadata = KaraokeSongMetadataDto.FromModel(options.SongMetadata),
                IntroOptions = IntroTitleScreenOptionsDto.FromModel(options.IntroOptions)
            };
        }
    }

    private sealed class PageLayoutOptionsDto
    {
        [JsonPropertyOrder(1)]
        public int StartRow { get; init; }

        [JsonPropertyOrder(2)]
        public int StartColumn { get; init; }

        [JsonPropertyOrder(3)]
        public int LineSpacing { get; init; }

        [JsonPropertyOrder(4)]
        public bool CenterHorizontally { get; init; }

        [JsonPropertyOrder(5)]
        public bool CenterVertically { get; init; }

        public PageLayoutOptions ToModel() =>
            new(StartRow, StartColumn, LineSpacing, CenterHorizontally, CenterVertically);

        public static PageLayoutOptionsDto FromModel(PageLayoutOptions options) =>
            new()
            {
                StartRow = options.StartRow,
                StartColumn = options.StartColumn,
                LineSpacing = options.LineSpacing,
                CenterHorizontally = options.CenterHorizontally,
                CenterVertically = options.CenterVertically
            };
    }

    private sealed class HighlightedLyricsRenderOptionsDto
    {
        [JsonPropertyOrder(1)]
        public int StartRow { get; init; }

        [JsonPropertyOrder(2)]
        public int StartColumn { get; init; }

        [JsonPropertyOrder(3)]
        public int LineSpacing { get; init; }

        [JsonPropertyOrder(4)]
        public byte BackgroundColor { get; init; }

        [JsonPropertyOrder(5)]
        public byte BaseTextColor { get; init; }

        [JsonPropertyOrder(6)]
        public byte HighlightTextColor { get; init; }

        [JsonPropertyOrder(7)]
        public bool ClearScreenBeforeRender { get; init; }

        public HighlightedLyricsRenderOptions ToModel() =>
            new(StartRow, StartColumn, LineSpacing, BackgroundColor, BaseTextColor, HighlightTextColor, ClearScreenBeforeRender);

        public static HighlightedLyricsRenderOptionsDto FromModel(HighlightedLyricsRenderOptions options) =>
            new()
            {
                StartRow = options.StartRow,
                StartColumn = options.StartColumn,
                LineSpacing = options.LineSpacing,
                BackgroundColor = options.BackgroundColor,
                BaseTextColor = options.BaseTextColor,
                HighlightTextColor = options.HighlightTextColor,
                ClearScreenBeforeRender = options.ClearScreenBeforeRender
            };
    }

    private sealed class KaraokeSongMetadataDto
    {
        [JsonPropertyOrder(1)]
        public string? Title { get; init; }

        [JsonPropertyOrder(2)]
        public string? Artist { get; init; }

        public KaraokeSongMetadata ToModel() => new(Title ?? string.Empty, Artist ?? string.Empty);

        public static KaraokeSongMetadataDto? FromModel(KaraokeSongMetadata? metadata)
        {
            if (metadata is null)
            {
                return null;
            }

            return new KaraokeSongMetadataDto
            {
                Title = metadata.Title,
                Artist = metadata.Artist
            };
        }
    }

    private sealed class CdgTimelineExportOptionsDto
    {
        [JsonPropertyOrder(1)]
        public TimeSpan FrameStep { get; init; }

        [JsonPropertyOrder(2)]
        public KaraokeFrameRenderOptionsDto? FrameRenderOptions { get; init; }

        [JsonPropertyOrder(3)]
        public TimeSpan? EndPadding { get; init; }

        [JsonPropertyOrder(4)]
        public bool IncludeInitialClearFrame { get; init; }

        public CdgTimelineExportOptions? ToModel()
        {
            var frameRenderOptions = FrameRenderOptions?.ToModel();
            if (frameRenderOptions is null)
            {
                return null;
            }

            return new CdgTimelineExportOptions(
                FrameStep,
                frameRenderOptions,
                EndPadding,
                IncludeInitialClearFrame);
        }

        public static CdgTimelineExportOptionsDto? FromModel(CdgTimelineExportOptions? options)
        {
            if (options is null)
            {
                return null;
            }

            return new CdgTimelineExportOptionsDto
            {
                FrameStep = options.FrameStep,
                FrameRenderOptions = KaraokeFrameRenderOptionsDto.FromModel(options.FrameRenderOptions),
                EndPadding = options.EndPadding,
                IncludeInitialClearFrame = options.IncludeInitialClearFrame
            };
        }
    }
}
