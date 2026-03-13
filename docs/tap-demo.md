# Tap Demo

Run the timing session demo from the repository root:

```bash
dotnet run --project examples/KJCDGCreator.DebugHello/KJCDGCreator.DebugHello.csproj -- tap-demo
```

What it shows:

- the current lyrics unit at startup
- timestamps being recorded in reading order
- the session advancing after each timestamp
- undo moving the pointer back
- reset clearing all timing progress

This is a console-only walkthrough of the domain timing session. It does not use Avalonia, audio playback, or keyboard capture.
