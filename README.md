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

This repository currently contains scaffolding only. Feature implementation is intentionally deferred.
