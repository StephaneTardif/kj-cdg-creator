using System.ComponentModel;
using System.Runtime.CompilerServices;
using KJCDGCreator.Audio.Timing;
using KJCDGCreator.Core.Projects;
using KJCDGCreator.Core.Rendering;
using KJCDGCreator.Core.Timing;
using KJCDGCreator.Editor.Services;

namespace KJCDGCreator.Editor.ViewModels;

public sealed class MainWindowViewModel : INotifyPropertyChanged, IDisposable
{
    private const string DefaultWindowTitle = "KJ CDG Creator";

    private readonly IAudioTimeSourceFactory _audioTimeSourceFactory;
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
    private IAudioTimeSource? _audioTimeSource;
    private LiveTapTimingController? _liveTapTimingController;
    private string? _loadedAudioSourcePath;
    private string _currentUnitText = "<no units>";
    private string _currentUnitProgress = "0 / 0";
    private string _timingCounts = "0 timed / 0 untimed";
    private string _currentAudioTimeDisplay = "00:00:00.000";
    private string _playPauseButtonText = "Play";
    private bool _canPlayPause;
    private bool _canStop;
    private bool _canTap;
    private bool _canUndo;
    private bool _canResetTiming;

    public MainWindowViewModel()
        : this(new Mp3AudioTimeSourceFactory())
    {
    }

    public MainWindowViewModel(IAudioTimeSourceFactory audioTimeSourceFactory)
    {
        _audioTimeSourceFactory = audioTimeSourceFactory ?? throw new ArgumentNullException(nameof(audioTimeSourceFactory));
        RefreshTimingSummary();
    }

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

    public string CurrentUnitText
    {
        get => _currentUnitText;
        private set => SetProperty(ref _currentUnitText, value);
    }

    public string CurrentUnitProgress
    {
        get => _currentUnitProgress;
        private set => SetProperty(ref _currentUnitProgress, value);
    }

    public string TimingCounts
    {
        get => _timingCounts;
        private set => SetProperty(ref _timingCounts, value);
    }

    public string CurrentAudioTimeDisplay
    {
        get => _currentAudioTimeDisplay;
        private set => SetProperty(ref _currentAudioTimeDisplay, value);
    }

    public string PlayPauseButtonText
    {
        get => _playPauseButtonText;
        private set => SetProperty(ref _playPauseButtonText, value);
    }

    public bool CanPlayPause
    {
        get => _canPlayPause;
        private set => SetProperty(ref _canPlayPause, value);
    }

    public bool CanStop
    {
        get => _canStop;
        private set => SetProperty(ref _canStop, value);
    }

    public bool CanTap
    {
        get => _canTap;
        private set => SetProperty(ref _canTap, value);
    }

    public bool CanUndo
    {
        get => _canUndo;
        private set => SetProperty(ref _canUndo, value);
    }

    public bool CanResetTiming
    {
        get => _canResetTiming;
        private set => SetProperty(ref _canResetTiming, value);
    }

    public void NewProject()
    {
        DisposeTapTimingRuntime();
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
        RefreshTimingSummary();
    }

    public void LoadProject(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        DisposeTapTimingRuntime();
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
        RefreshTimingSummary();
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
        RefreshTimingSummary();
    }

    public void SetStatusMessage(string message)
    {
        StatusMessage = message;
    }

    public void TogglePlayback()
    {
        if (!EnsureTapTimingSession())
        {
            return;
        }

        ApplyTapTimingStatus(_liveTapTimingController!.HandleCommand(LiveTapTimingCommand.TogglePlayPause), markDirty: false);
    }

    public void StopPlayback()
    {
        if (_liveTapTimingController is null)
        {
            SetStatusMessage("No timing session is active.");
            return;
        }

        ApplyTapTimingStatus(_liveTapTimingController.HandleCommand(LiveTapTimingCommand.Stop), markDirty: true);
    }

    public void TapCurrentUnit()
    {
        if (!EnsureTapTimingSession())
        {
            return;
        }

        var timedBefore = _timing.TimedCount;
        ApplyTapTimingStatus(_liveTapTimingController!.HandleCommand(LiveTapTimingCommand.RecordTap), markDirty: _timing.TimedCount != timedBefore);
    }

    public void UndoTiming()
    {
        if (_liveTapTimingController is null)
        {
            SetStatusMessage("No timing session is active.");
            return;
        }

        var timedBefore = _timing.TimedCount;
        ApplyTapTimingStatus(_liveTapTimingController.HandleCommand(LiveTapTimingCommand.Undo), markDirty: _timing.TimedCount != timedBefore);
    }

    public void ResetTiming()
    {
        if (!EnsureTapTimingSession())
        {
            return;
        }

        var timedBefore = _timing.TimedCount;
        ApplyTapTimingStatus(_liveTapTimingController!.HandleCommand(LiveTapTimingCommand.Reset), markDirty: timedBefore > 0);
    }

    public void RefreshTapTimingState()
    {
        if (_audioTimeSource is not null)
        {
            CurrentAudioTimeDisplay = _audioTimeSource.CurrentTime.ToString(@"hh\:mm\:ss\.fff");
        }

        UpdateTapTimingCommands();
    }

    public KaraokeProject BuildProjectForSave(string? targetPath = null)
    {
        var synchronizedTiming = SynchronizeTimingWithLyrics(_timing, RawLyricsText);
        _timing = synchronizedTiming;

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

    public void Dispose()
    {
        DisposeTapTimingRuntime();
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

    private bool EnsureTapTimingSession()
    {
        if (string.IsNullOrWhiteSpace(RawLyricsText))
        {
            SetStatusMessage("Enter lyrics before starting tap timing.");
            RefreshTimingSummary();
            return false;
        }

        if (string.IsNullOrWhiteSpace(SourceMp3Path))
        {
            SetStatusMessage("Set a source MP3 path before starting tap timing.");
            RefreshTimingSummary();
            return false;
        }

        try
        {
            if (_audioTimeSource is null || !string.Equals(_loadedAudioSourcePath, SourceMp3Path, StringComparison.Ordinal))
            {
                DisposeAudioOnly();
                _audioTimeSource = _audioTimeSourceFactory.Create(SourceMp3Path);
                _loadedAudioSourcePath = SourceMp3Path;
            }
        }
        catch (Exception exception)
        {
            DisposeAudioOnly();
            SetStatusMessage($"Unable to initialize audio: {exception.Message}");
            RefreshTimingSummary();
            return false;
        }

        if (_liveTapTimingController is null)
        {
            _timing = SynchronizeTimingWithLyrics(_timing, RawLyricsText);
            _liveTapTimingController = new LiveTapTimingController(_timing, _audioTimeSource);
        }

        ApplyTapTimingStatus(_liveTapTimingController.GetStatus("Tap timing ready."), markDirty: false);
        return true;
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

    private void ApplyTapTimingStatus(LiveTapTimingStatus status, bool markDirty)
    {
        CurrentUnitText = status.CurrentUnitText ?? "<complete>";
        CurrentUnitProgress = _timing.Units.Count == 0
            ? "0 / 0"
            : $"{Math.Min(status.CurrentUnitIndex + 1, _timing.Units.Count)} / {_timing.Units.Count}";
        TimingCounts = $"{status.TimedCount} timed / {status.UntimedCount} untimed";
        CurrentAudioTimeDisplay = _audioTimeSource?.CurrentTime.ToString(@"hh\:mm\:ss\.fff") ?? "00:00:00.000";
        PlayPauseButtonText = status.IsPlaying ? "Pause" : "Play";
        UpdateTapTimingCommands();

        if (!string.IsNullOrWhiteSpace(status.Message))
        {
            StatusMessage = status.Message;
        }

        if (markDirty)
        {
            HasUnsavedChanges = true;
        }
    }

    private void RefreshTimingSummary()
    {
        var currentUntimed = _timing.Units.FirstOrDefault(unit => !unit.Timestamp.HasValue);
        CurrentUnitText = currentUntimed?.Text ?? (_timing.Units.Count == 0 ? "<no units>" : "<complete>");
        CurrentUnitProgress = _timing.Units.Count == 0
            ? "0 / 0"
            : $"{Math.Min(_timing.TimedCount + 1, _timing.Units.Count)} / {_timing.Units.Count}";
        TimingCounts = $"{_timing.TimedCount} timed / {_timing.UntimedCount} untimed";
        CurrentAudioTimeDisplay = _audioTimeSource?.CurrentTime.ToString(@"hh\:mm\:ss\.fff") ?? "00:00:00.000";
        PlayPauseButtonText = _audioTimeSource?.IsPlaying == true ? "Pause" : "Play";
        UpdateTapTimingCommands();
    }

    private void UpdateTapTimingCommands()
    {
        var hasUnits = _timing.Units.Count > 0;
        var hasAudioPath = !string.IsNullOrWhiteSpace(SourceMp3Path);
        var isComplete = _timing.UntimedCount == 0 && hasUnits;

        CanPlayPause = hasUnits && hasAudioPath;
        CanStop = _audioTimeSource is not null;
        CanTap = _audioTimeSource is not null && !isComplete;
        CanUndo = _timing.TimedCount > 0;
        CanResetTiming = _timing.Units.Count > 0;
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

        if (propertyName == nameof(RawLyricsText))
        {
            _liveTapTimingController = null;
            RefreshTimingSummary();
        }

        if (propertyName == nameof(SourceMp3Path))
        {
            DisposeAudioOnly();
            _liveTapTimingController = null;
            RefreshTimingSummary();
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

    private void DisposeTapTimingRuntime()
    {
        _liveTapTimingController = null;
        DisposeAudioOnly();
        RefreshTimingSummary();
    }

    private void DisposeAudioOnly()
    {
        if (_audioTimeSource is IDisposable disposable)
        {
            disposable.Dispose();
        }

        _audioTimeSource = null;
        _loadedAudioSourcePath = null;
    }

    private static TimingDocument CreateBlankTiming() =>
        new(Array.Empty<TimedUnit>());

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
