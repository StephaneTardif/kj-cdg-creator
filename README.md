# KJ CDG Creator

KJ CDG Creator is a cross-platform desktop application for building classic karaoke tracks in MP3 + CDG format using syllable-level tap timing.

## Stack

- C#
- .NET 8
- Avalonia
- xUnit

## Planned v1 scope

1. Lyrics editor with `|` syllable separators.
2. Tap timing mode that records syllable timestamps with the Space key.
3. CDG rendering engine for classic karaoke highlighting.
4. Export pipeline for standard karaoke players such as VirtualDJ.

## Solution layout

```text
src/
  KJCDGCreator.Core    Domain models and application contracts
  KJCDGCreator.Audio   Audio-facing infrastructure and timing input services
  KJCDGCreator.Editor  Avalonia desktop UI and composition root
  KJCDGCreator.Tests   Test project for core scaffolding
docs/
  cdg-format.md        Notes about the CDG packet format and rendering constraints
examples/
  demo-project.json    Example project payload for future import/export work
```

## Status

Implemented so far:

1. Solution scaffolding for `Core`, `Audio`, `Editor`, `Tests`, and a debug example runner.
2. Avalonia desktop shell with placeholder lyrics and timing views.
3. Minimal CDG packet writer that generates a valid `hello.cdg` file.
4. Tile-based CDG screen buffer covering the full 50x18 display grid with 6x12 pixel tiles.
5. Screen-buffer renderer that converts tile changes into `Tile Block Normal` packets and supports memory preset clears.
6. Bitmap font rendering for lyric-safe ASCII characters, written into the CDG tile screen buffer.
7. CDG packet inspection and ASCII preview tooling for verifying generated files without external karaoke software.
8. Basic automated tests for CDG file creation, packet sizing, tile-buffer updates, changed-tile rendering, and packet inspection.
9. ✔ Lyrics parser (syllable-level structure)
10. ✔ Tap timing domain model
11. ✔ Tap timing capture session logic
12. ✔ Highlight progression engine

Still intentionally deferred:

- Karaoke highlighting and syllable timing playback
- Real lyrics layout and rendering
- MP3 synchronization and full export workflow
