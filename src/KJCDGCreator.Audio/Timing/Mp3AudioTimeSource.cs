using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace KJCDGCreator.Audio.Timing;

public sealed class Mp3AudioTimeSource : IAudioPlaybackClock, IDisposable
{
    private static readonly object ActiveSourcesLock = new();
    private static readonly HashSet<Mp3AudioTimeSource> ActiveSources = new();
    private static bool _exitHandlersRegistered;

    private readonly string _mp3Path;
    private readonly TimeSpan? _duration;
    private readonly Stopwatch _stopwatch = new();
    private Process? _playbackProcess;
    private TimeSpan _offset;
    private TimeSpan _lastReportedTime;

    public Mp3AudioTimeSource(string mp3Path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mp3Path);

        if (!File.Exists(mp3Path))
        {
            throw new FileNotFoundException("MP3 file was not found.", mp3Path);
        }

        _mp3Path = mp3Path;
        _duration = TryReadDuration(mp3Path);
        RegisterExitHandlers();
    }

    public AudioPlaybackState State { get; private set; } = AudioPlaybackState.Stopped;

    public TimeSpan Position => CurrentTime;

    public TimeSpan CurrentTime
    {
        get
        {
            SynchronizePlaybackState();

            var current = State == AudioPlaybackState.Playing
                ? _offset + _stopwatch.Elapsed
                : _offset;

            if (_duration.HasValue && current > _duration.Value)
            {
                current = _duration.Value;
            }

            if (current < _lastReportedTime)
            {
                current = _lastReportedTime;
            }

            _lastReportedTime = current;
            return current;
        }
    }

    public bool IsPlaying => State == AudioPlaybackState.Playing;

    public void Play()
    {
        if (State == AudioPlaybackState.Playing)
        {
            return;
        }

        if (_duration.HasValue && _offset >= _duration.Value)
        {
            _offset = TimeSpan.Zero;
            _lastReportedTime = TimeSpan.Zero;
        }

        StartPlaybackProcess();
        _stopwatch.Restart();
        State = AudioPlaybackState.Playing;
    }

    public void Pause()
    {
        if (State != AudioPlaybackState.Playing)
        {
            return;
        }

        _offset = CurrentTime;
        _stopwatch.Reset();
        StopPlaybackProcess();
        State = AudioPlaybackState.Paused;
    }

    public void Stop()
    {
        StopPlaybackProcess();
        _stopwatch.Reset();
        _offset = TimeSpan.Zero;
        _lastReportedTime = TimeSpan.Zero;
        State = AudioPlaybackState.Stopped;
    }

    public void Dispose()
    {
        StopPlaybackProcess();
        _stopwatch.Stop();
        UnregisterActiveSource(this);
    }

    private void StartPlaybackProcess()
    {
        var executable = ResolveExecutable(
            "ffplay",
            "/opt/homebrew/bin/ffplay",
            "/usr/local/bin/ffplay");

        var startInfo = new ProcessStartInfo
        {
            FileName = executable,
            Arguments = BuildPlaybackArguments(),
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = false,
            RedirectStandardOutput = false
        };

        try
        {
            _playbackProcess = Process.Start(startInfo)
                ?? throw new InvalidOperationException("Failed to start MP3 playback process.");
            RegisterActiveSource(this);
        }
        catch (Exception exception) when (exception is Win32Exception or InvalidOperationException)
        {
            throw new InvalidOperationException("Unable to start MP3 playback. Ensure ffplay is installed and the MP3 path is valid.", exception);
        }
    }

    private string BuildPlaybackArguments()
    {
        var offsetSeconds = _offset.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture);
        return $"-nodisp -autoexit -loglevel quiet -ss {offsetSeconds} \"{_mp3Path}\"";
    }

    private void SynchronizePlaybackState()
    {
        if (State == AudioPlaybackState.Playing && _playbackProcess is not null && _playbackProcess.HasExited)
        {
            _offset += _stopwatch.Elapsed;

            if (_duration.HasValue && _offset > _duration.Value)
            {
                _offset = _duration.Value;
            }

            _stopwatch.Reset();
            _playbackProcess.Dispose();
            _playbackProcess = null;
            UnregisterActiveSource(this);
            State = AudioPlaybackState.Stopped;
        }
    }

    private void StopPlaybackProcess()
    {
        if (_playbackProcess is null)
        {
            return;
        }

        try
        {
            if (!_playbackProcess.HasExited)
            {
                _playbackProcess.Kill(entireProcessTree: true);
                _playbackProcess.WaitForExit(1000);
            }
        }
        catch
        {
            // Best effort only; the clock state is still reset locally.
        }
        finally
        {
            _playbackProcess.Dispose();
            _playbackProcess = null;
            UnregisterActiveSource(this);
        }
    }

    private static void RegisterExitHandlers()
    {
        lock (ActiveSourcesLock)
        {
            if (_exitHandlersRegistered)
            {
                return;
            }

            AppDomain.CurrentDomain.ProcessExit += (_, _) => StopAllActivePlayback();
            AppDomain.CurrentDomain.UnhandledException += (_, _) => StopAllActivePlayback();

            try
            {
                Console.CancelKeyPress += (_, _) => StopAllActivePlayback();
            }
            catch
            {
                // Some UI hosts may not expose a console. ProcessExit still covers normal shutdown.
            }

            _exitHandlersRegistered = true;
        }
    }

    private static void RegisterActiveSource(Mp3AudioTimeSource source)
    {
        lock (ActiveSourcesLock)
        {
            ActiveSources.Add(source);
        }
    }

    private static void UnregisterActiveSource(Mp3AudioTimeSource source)
    {
        lock (ActiveSourcesLock)
        {
            ActiveSources.Remove(source);
        }
    }

    private static void StopAllActivePlayback()
    {
        Mp3AudioTimeSource[] sources;

        lock (ActiveSourcesLock)
        {
            sources = ActiveSources.ToArray();
        }

        foreach (var source in sources)
        {
            source.StopPlaybackProcess();
        }
    }

    private static TimeSpan? TryReadDuration(string mp3Path)
    {
        var executable = ResolveExecutable(
            "ffprobe",
            "/opt/homebrew/bin/ffprobe",
            "/usr/local/bin/ffprobe");

        var startInfo = new ProcessStartInfo
        {
            FileName = executable,
            Arguments = $"-v error -show_entries format=duration -of default=nw=1:nk=1 \"{mp3Path}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        try
        {
            using var process = Process.Start(startInfo);
            if (process is null)
            {
                return null;
            }

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(1000);

            return double.TryParse(output.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds)
                ? TimeSpan.FromSeconds(seconds)
                : null;
        }
        catch
        {
            return null;
        }
    }

    private static string ResolveExecutable(string fallbackName, params string[] candidates)
    {
        foreach (var candidate in candidates)
        {
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return fallbackName;
    }
}
