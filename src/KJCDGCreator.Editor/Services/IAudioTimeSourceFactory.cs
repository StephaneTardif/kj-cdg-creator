using KJCDGCreator.Audio.Timing;

namespace KJCDGCreator.Editor.Services;

public interface IAudioTimeSourceFactory
{
    IAudioTimeSource Create(string sourceMp3Path);
}
