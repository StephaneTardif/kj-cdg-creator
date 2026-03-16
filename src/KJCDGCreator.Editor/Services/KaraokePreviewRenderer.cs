using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;
using KJCDGCreator.Core.Cdg;
using KJCDGCreator.Core.Projects;
using KJCDGCreator.Core.Rendering;

namespace KJCDGCreator.Editor.Services;

public sealed class KaraokePreviewRenderer : IKaraokePreviewRenderer
{
    private const int TileScale = 2;

    private static readonly uint[] Palette =
    [
        0xFF000000,
        0xFF0000AA,
        0xFF00AA00,
        0xFF00AAAA,
        0xFFAA0000,
        0xFFAA00AA,
        0xFFAA5500,
        0xFFAAAAAA,
        0xFF555555,
        0xFF5555FF,
        0xFF55FF55,
        0xFF55FFFF,
        0xFFFF5555,
        0xFFFF55FF,
        0xFFFFFF55,
        0xFFFFFFFF
    ];

    public KaraokePreviewRenderResult Render(KaraokeProject? project, TimeSpan playbackTime)
    {
        if (project is null || string.IsNullOrWhiteSpace(project.RawLyricsText))
        {
            var emptyScreen = new CdgScreenBuffer();
            var emptyPixels = RenderPixelBuffer(emptyScreen, out var emptyWidth, out var emptyHeight);

            return new KaraokePreviewRenderResult(
                TryCreateBitmap(emptyPixels, emptyWidth, emptyHeight),
                "Preview unavailable. Add lyrics to render a frame.",
                HasContent: false,
                PixelData: emptyPixels,
                PixelWidth: emptyWidth,
                PixelHeight: emptyHeight);
        }

        var frameOptions = project.FrameRenderOptions ?? CreateDefaultFrameOptions(project);
        var screen = new CdgScreenBuffer(CdgScreenBuffer.CreateBlankTile(frameOptions.HighlightOptions.BackgroundColor));
        var lyrics = project.BuildLyricsDocument();
        var result = KaraokeFrameRenderer.RenderFrame(lyrics, project.Timing, playbackTime, screen, frameOptions);

        var message = result.HasContent
            ? $"Preview at {playbackTime:hh\\:mm\\:ss\\.fff}"
            : "Preview unavailable for the current project state.";

        var pixels = RenderPixelBuffer(screen, out var width, out var height);

        return new KaraokePreviewRenderResult(
            TryCreateBitmap(pixels, width, height),
            message,
            result.HasContent,
            pixels,
            width,
            height);
    }

    private static byte[] RenderPixelBuffer(CdgScreenBuffer screen, out int width, out int height)
    {
        width = CdgScreenBuffer.Columns * CdgTile.Width * TileScale;
        height = CdgScreenBuffer.Rows * CdgTile.Height * TileScale;
        var rowBytes = width * 4;
        var bytes = new byte[rowBytes * height];

        for (var tileRow = 0; tileRow < CdgScreenBuffer.Rows; tileRow++)
        {
            for (var tileColumn = 0; tileColumn < CdgScreenBuffer.Columns; tileColumn++)
            {
                var tile = screen.GetTile(tileRow, tileColumn);
                WriteTile(bytes, rowBytes, tileRow, tileColumn, tile);
            }
        }

        return bytes;
    }

    private static WriteableBitmap? TryCreateBitmap(byte[] pixelData, int width, int height)
    {
        try
        {
            var bitmap = new WriteableBitmap(
                new PixelSize(width, height),
                new Vector(96, 96),
                Avalonia.Platform.PixelFormat.Bgra8888,
                Avalonia.Platform.AlphaFormat.Opaque);

            using var framebuffer = bitmap.Lock();
            Marshal.Copy(pixelData, 0, framebuffer.Address, pixelData.Length);
            return bitmap;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private static void WriteTile(byte[] bytes, int rowBytes, int tileRow, int tileColumn, CdgTile tile)
    {
        for (var pixelRow = 0; pixelRow < CdgTile.Height; pixelRow++)
        {
            var rowBits = tile.Bitmap[pixelRow];

            for (var pixelColumn = 0; pixelColumn < CdgTile.Width; pixelColumn++)
            {
                var mask = 1 << (CdgTile.Width - 1 - pixelColumn);
                var color = (rowBits & mask) != 0
                    ? Palette[tile.ForegroundColor & 0x0F]
                    : Palette[tile.BackgroundColor & 0x0F];

                for (var scaleY = 0; scaleY < TileScale; scaleY++)
                {
                    var destinationY = ((tileRow * CdgTile.Height) + pixelRow) * TileScale + scaleY;

                    for (var scaleX = 0; scaleX < TileScale; scaleX++)
                    {
                        var destinationX = ((tileColumn * CdgTile.Width) + pixelColumn) * TileScale + scaleX;
                        var offset = (destinationY * rowBytes) + (destinationX * 4);

                        bytes[offset] = (byte)(color & 0xFF);
                        bytes[offset + 1] = (byte)((color >> 8) & 0xFF);
                        bytes[offset + 2] = (byte)((color >> 16) & 0xFF);
                        bytes[offset + 3] = (byte)((color >> 24) & 0xFF);
                    }
                }
            }
        }
    }

    private static KaraokeFrameRenderOptions CreateDefaultFrameOptions(KaraokeProject project)
    {
        var metadata = string.IsNullOrWhiteSpace(project.Title) && string.IsNullOrWhiteSpace(project.Artist)
            ? null
            : new KaraokeSongMetadata(project.Title ?? string.Empty, project.Artist ?? string.Empty);

        return new KaraokeFrameRenderOptions(
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
            metadata,
            project.IntroOptions);
    }
}
