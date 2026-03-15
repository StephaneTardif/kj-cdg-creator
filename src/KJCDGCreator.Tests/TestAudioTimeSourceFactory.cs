using KJCDGCreator.Audio.Timing;
using KJCDGCreator.Editor.Services;

namespace KJCDGCreator.Tests;

internal sealed class TestAudioTimeSourceFactory : IAudioTimeSourceFactory
{
    private readonly Func<string, IAudioTimeSource> _factory;

    public TestAudioTimeSourceFactory(Func<string, IAudioTimeSource> factory)
    {
        _factory = factory;
    }

    public IAudioTimeSource Create(string sourceMp3Path) => _factory(sourceMp3Path);
}
