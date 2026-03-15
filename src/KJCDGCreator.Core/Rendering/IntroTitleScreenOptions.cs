namespace KJCDGCreator.Core.Rendering;

public sealed record IntroTitleScreenOptions(
    bool Enabled,
    TimeSpan? FixedDuration,
    bool UseFirstLyricTimestampWhenLonger,
    byte BackgroundColor,
    byte TitleColor,
    byte ArtistColor,
    int TitleRow,
    int ArtistRow,
    bool CenterHorizontally);
