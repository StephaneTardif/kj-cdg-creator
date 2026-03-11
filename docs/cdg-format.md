# CDG Format Notes

CDG files are composed of 24-byte packets written at 300 packets per second alongside MP3 audio.

For KJ CDG Creator v1, the renderer is expected to target the classic constraints used by karaoke players:

- 6x12 tile cells
- 16-color palette
- separate background, lyric, and highlight colors
- progressive syllable highlighting driven by tap timing data

This document is intentionally high level during scaffolding. Detailed packet encoding notes can be added once the rendering engine is implemented.
