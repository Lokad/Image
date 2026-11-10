using System.Numerics;
using System.Reflection;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class TextRasterArithmeticTests
{
    // Exercise the actual production arithmetic without a production test hook.
    private static readonly Action<ImageBuffer, int, int, RgbaColor, uint> Blend = typeof(TextRasterizer)
        .GetMethod("Blend", BindingFlags.NonPublic | BindingFlags.Static)!
        .CreateDelegate<Action<ImageBuffer, int, int, RgbaColor, uint>>();

    [Theory] [InlineData(8)] [InlineData(16)]
    public void FullPrecisionCompositionMatchesTheExactRational(int depth)
    {
        using var canvas = ImageBuffer.Create(1, 1, depth); var maximum = depth == 8 ? 255 : 65535;
        void Check(ushort alpha, uint coverage, int destinationAlpha, ushort source, int destination)
        {
            int[] before = [destination, maximum - destination, destination / 2, destinationAlpha];
            for (var c = 0; c < 4; c++) Set(canvas, 0, 0, c, before[c]);
            var color = new RgbaColor(source, (ushort)(65535 - source), (ushort)(source / 2), alpha);
            Blend(canvas, 0, 0, color, coverage);
            BigInteger denominator = 65535L * 65535, sa = (long)alpha * coverage;
            var composedAlpha = sa * maximum + destinationAlpha * (denominator - sa);
            var divisor = composedAlpha * 65535;
            for (var c = 0; c < 3; c++)
            {
                var sample = c == 0 ? color.R : c == 1 ? color.G : color.B;
                var numerator = sample * sa * maximum * maximum + before[c] * (BigInteger)destinationAlpha * (denominator - sa) * 65535;
                var expected = sa.IsZero ? before[c] : (int)((numerator + divisor / 2) / divisor);
                Assert.Equal(expected, Sample(canvas, 0, 0, c));
            }
            Assert.Equal(sa.IsZero ? destinationAlpha : (int)((composedAlpha + denominator / 2) / denominator), Sample(canvas, 0, 0, 3));
        }
        ushort[] edges = [0, 1, 2, 127, 128, 255, 256, 257, 32767, 32768, 65534, 65535];
        foreach (var alpha in edges) foreach (var coverage in edges)
        foreach (var destinationAlpha in new[] { 0, 1, maximum / 2, maximum })
        {
            Check(alpha, coverage, destinationAlpha, 0, 0); Check(alpha, coverage, destinationAlpha, 65535, maximum);
            Check(alpha, coverage, destinationAlpha, 65535, 0); Check(alpha, coverage, destinationAlpha, 0, maximum);
        }
        var random = new Random(0x267 + depth);
        for (var i = 0; i < 20000; i++)
            Check((ushort)random.Next(65536), (uint)random.Next(65536), random.Next(maximum + 1), (ushort)random.Next(65536), random.Next(maximum + 1));
    }

    [Theory] [InlineData(8)] [InlineData(16)]
    public void SparseAndOverlappingGlyphsMatchIndependentFreshSingleGlyphDraws(int depth)
    {
        using var font = Face(); var options = new TextOptions { FontSize = 10, LineSpacing = 1.25 };
        using var layout = TextLayout.Create(font, "A A AA\n A A", options); using var single = TextLayout.Create(font, "A", options);
        using var canvas = OriginalPixels.Buffer(64, 32, depth); using var expected = canvas.Clone();
        using var root = new TestMemoryBudget(1_000_000); using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var raster = new TextRasterizer(layout, context, canvas.Width);
        raster.Draw(canvas, -2.375, -1.625, Ink, 0, 0, canvas.Width, canvas.Height, default);
        var first = single.Placements[0];
        foreach (var glyph in layout.Placements)
            if (glyph.GlyphIndex == first.GlyphIndex)
                ImageDrawing.DrawText(expected, single, -2.375 + glyph.X - first.X, -1.625 + glyph.Y - first.Y, Ink);
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(canvas));
        raster.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory] [InlineData(8)] [InlineData(16)]
    public void DirtyRowFromFailedDrawingClearsBeforeTheSameOwnerDrawsElsewhere(int depth)
    {
        using var font = Face(); using var layout = TextLayout.Create(font, "AA\nA", new() { FontSize = 10 });
        using var root = new TestMemoryBudget(1_000_000); using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var raster = new TextRasterizer(layout, context, 64); var retained = root.Bytes;
        using var disposed = ImageBuffer.Create(64, 32, depth); disposed.Dispose();
        Assert.Throws<ObjectDisposedException>(() => raster.Draw(disposed, 4.375, .625, Ink, 0, 0, 64, 32, default));
        Assert.Equal(retained, root.Bytes);
        using var actual = OriginalPixels.Buffer(64, 32, depth); using var expected = actual.Clone();
        raster.Draw(actual, 29.625, 3.375, Ink, 0, 0, 64, 32, default);
        ImageDrawing.DrawText(expected, layout, 29.625, 3.375, Ink);
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(actual));
        raster.Dispose(); Assert.Equal(0, root.Bytes);
        ImageDrawing.DrawText(actual, layout, -.125, 1.25, Ink, new() { MemoryBudget = root });
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    private static readonly RgbaColor Ink = new(12345, 54321, 32769, 45678);
    private static TrueTypeFont Face() => TrueTypeFont.Load(AuthoredTrueType.Build(glyphOverrides: new Dictionary<int, byte[]>()
    { [2] = AuthoredTrueType.Outline([[(0, 0, true), (0, 600, true), (1000, 600, true), (1000, 0, true)]]) }));
    private static int Sample(ImageBuffer image, int x, int y, int c)
        => image.BitDepth == 8 ? image.GetRow8(y)[x * 4 + c] : image.GetRow16(y)[x * 4 + c];
    private static void Set(ImageBuffer image, int x, int y, int c, int value)
    { if (image.BitDepth == 8) image.GetRow8(y)[x * 4 + c] = (byte)value; else image.GetRow16(y)[x * 4 + c] = (ushort)value; }
}
