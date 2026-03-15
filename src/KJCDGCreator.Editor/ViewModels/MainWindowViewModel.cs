using System.ComponentModel;
using System.Runtime.CompilerServices;
using KJCDGCreator.Core.Projects;
using KJCDGCreator.Core.Rendering;
using KJCDGCreator.Core.Timing;

namespace KJCDGCreator.Editor.ViewModels;

public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private const string DefaultWindowTitle = "KJ CDG Creator";

    private string _title = string.Empty;
    private string _artist = string.Empty;
    private string _sourceMp3Path = string.Empty;
    private string _rawLyricsText = string.Empty;
    private string? _currentProjectPath;
    private string _statusMessage = "Create a new project or open an existing .kjproj.json file.";
    private string _validationMessage = "Lyrics text is empty.";
    private bool _hasUnsavedChanges;
    private TimingDocument _timing = CreateBlankTiming();
    private IntroTitleScreenOptions? _introOptions;
    private KaraokeFrameRenderOptions? _frameRenderOptions;
    private Core.Cdg.CdgTimelineExportOptions? _exportOptions;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Title
    {
        get => _title;
        set => SetEditableField(ref _title, value);
    }

    public string Artist
    {
        get => _artist;
        set => SetEditableField(ref _artist, value);
    }

    public string SourceMp3Path
    {
        get => _sourceMp3Path;
        set => SetEditableField(ref _sourceMp3Path, value);
    }

    public string RawLyricsText
    {
        get => _rawLyricsText;
        set
        {
            if (SetEditableField(ref _rawLyricsText, value))
            {
                UpdateValidationMessage();
            }
        }
    }

    public string? CurrentProjectPath
    {
        get => _currentProjectPath;
        private set
        {
            if (SetProperty(ref _currentProjectPath, value))
            {
                OnPropertyChanged(nameof(ProjectPathDisplay));
                OnPropertyChanged(nameof(WindowTitle));
            }
        }
    }

    public string ProjectPathDisplay => string.IsNullOrWhiteSpace(CurrentProjectPath) ? "Unsaved project" : CurrentProjectPath;

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public string ValidationMessage
    {
        get => _validationMessage;
        private set => SetProperty(ref _validationMessage, value);
    }

    public bool HasUnsavedChanges
    {
        get => _hasUnsavedChanges;
        private set
        {
            if (SetProperty(ref _hasUnsavedChanges, value))
            {
                OnPropertyChanged(nameof(WindowTitle));
            }
        }
    }

    public string WindowTitle
    {
        get
        {
            var projectName = string.IsNullOrWhiteSpace(CurrentProjectPath)
                ? "Untitled Project"
                : Path.GetFileName(CurrentProjectPath);
            var dirtyMarker = HasUnsavedChanges ? "*" : string.Empty;
            return $"{dirtyMarker}{projectName} - {DefaultWindowTitle}";
        }
    }

    public void NewProject()
    {
        _timing = CreateBlankTiming();
        _introOptions = null;
        _frameRenderOptions = null;
        _exportOptions = null;
        CurrentProjectPath = null;
        _title = string.Empty;
        _artist = string.Empty;
        _sourceMp3Path = string.Empty;
        _rawLyricsText = string.Empty;
        RaiseEditablePropertiesChanged();
        UpdateValidationMessage();
        HasUnsavedChanges = false;
        StatusMessage = "New project ready.";
    }

    public void LoadProject(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var project = KaraokeProjectSerializer.Load(path);
        _timing = project.Timing;
        _introOptions = project.IntroOptions;
        _frameRenderOptions = project.FrameRenderOptions;
        _exportOptions = project.ExportOptions;
        _title = project.Title ?? string.Empty;
        _artist = project.Artist ?? string.Empty;
        _sourceMp3Path = project.SourceMp3Path ?? string.Empty;
        _rawLyricsText = project.RawLyricsText;
        CurrentProjectPath = path;
        RaiseEditablePropertiesChanged();
        UpdateValidationMessage();
        HasUnsavedChanges = false;
        StatusMessage = $"Opened project: {Path.GetFileName(path)}";
    }

    public void SaveProject(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var project = BuildProjectForSave(path);
        KaraokeProjectSerializer.Save(project, path);
        _timing = project.Timing;
        CurrentProjectPath = path;
        HasUnsavedChanges = false;
        UpdateValidationMessage();
        StatusMessage = $"Saved project: {Path.GetFileName(path)}";
    }

    public void SetStatusMessage(string message)
    {
        StatusMessage = message;
    }

    public KaraokeProject BuildProjectForSave(string? targetPath = null)
    {
        var effectivePath = string.IsNullOrWhiteSpace(targetPath) ? CurrentProjectPath : targetPath;
        var synchronizedTiming = SynchronizeTimingWithLyrics(_timing, RawLyricsText);

        return new KaraokeProject(
            ProjectVersion: KaraokeProjectSerializer.CurrentProjectVersion,
            Title: string.IsNullOrWhiteSpace(Title) ? null : Title,
            Artist: string.IsNullOrWhiteSpace(Artist) ? null : Artist,
            RawLyricsText: RawLyricsText,
            Timing: synchronizedTiming,
            SourceMp3Path: string.IsNullOrWhiteSpace(SourceMp3Path) ? null : SourceMp3Path,
            IntroOptions: _introOptions,
            FrameRenderOptions: BuildFrameOptions(),
            ExportOptions: BuildExportOptions());
    }

    private KaraokeFrameRenderOptions BuildFrameOptions()
    {
        var metadata = string.IsNullOrWhiteSpace(Title) && string.IsNullOrWhiteSpace(Artist)
            ? null
            : new KaraokeSongMetadata(Title, Artist);

        var baseOptions = _frameRenderOptions ?? new KaraokeFrameRenderOptions(
            new PageLayoutOptions(
                StartRow: 4,
                StartColumn: 4,
                LineSpacing: 2,
                CenterHorizontally: true,
                CenterVertically: true),
            new HighlightedLyricsRenderOptions(
                StartRow: 4,
                StartColumn: 4,
                LineSpacing: 2,
                BackgroundColor: 0,
                BaseTextColor: 15,
                HighlightTextColor: 12,
                ClearScreenBeforeRender: true),
            SongMetadata: metadata,
            IntroOptions: _introOptions);

        var updated = baseOptions with
        {
            SongMetadata = metadata,
            IntroOptions = _introOptions ?? baseOptions.IntroOptions
        };

        _frameRenderOptions = updated;
        return updated;
    }

    private Core.Cdg.CdgTimelineExportOptions? BuildExportOptions()
    {
        if (_exportOptions is null)
        {
            return null;
        }

        var updated = _exportOptions with
        {
            FrameRenderOptions = BuildFrameOptions()
        };

        _exportOptions = updated;
        return updated;
    }

    private static TimingDocument SynchronizeTimingWithLyrics(TimingDocument existingTiming, string rawLyricsText)
    {
        var rebuiltTiming = TimingDocumentBuilder.FromLyrics(Core.Lyrics.LyricsParser.Parse(rawLyricsText ?? string.Empty));
        if (existingTiming.Units.Count != rebuiltTiming.Units.Count)
        {
            return rebuiltTiming;
        }

        for (var index = 0; index < existingTiming.Units.Count; index++)
        {
            if (!string.Equals(existingTiming.Units[index].Text, rebuiltTiming.Units[index].Text, StringComparison.Ordinal))
            {
                return rebuiltTiming;
            }

            if (existingTiming.Units[index].Timestamp is TimeSpan timestamp)
            {
                rebuiltTiming.AssignTimestamp(rebuiltTiming.Units[index].UnitIndex, timestamp);
            }
        }

        return rebuiltTiming;
    }

    private void UpdateValidationMessage()
    {
        ValidationMessage = string.IsNullOrWhiteSpace(RawLyricsText)
            ? "Lyrics text is empty."
            : "Lyrics ready.";
    }

    private bool SetEditableField(ref string field, string value, [CallerMemberName] string? propertyName = null)
    {
        if (!SetProperty(ref field, value, propertyName))
        {
            return false;
        }

        HasUnsavedChanges = true;
        return true;
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void RaiseEditablePropertiesChanged()
    {
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Artist));
        OnPropertyChanged(nameof(SourceMp3Path));
        OnPropertyChanged(nameof(RawLyricsText));
        OnPropertyChanged(nameof(WindowTitle));
    }

    private static TimingDocument CreateBlankTiming() =>
        new(Array.Empty<TimedUnit>());

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
