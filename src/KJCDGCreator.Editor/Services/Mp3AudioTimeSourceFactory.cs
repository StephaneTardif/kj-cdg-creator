using KJCDGCreator.Audio.Timing;

namespace KJCDGCreator.Editor.Services;

public sealed class Mp3AudioTimeSourceFactory : IAudioTimeSourceFactory
{
    public IAudioTimeSource Create(string sourceMp3Path) => new Mp3AudioTimeSource(sourceMp3Path);
}
