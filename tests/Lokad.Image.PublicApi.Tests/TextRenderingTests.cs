using System.Numerics;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class TextRenderingTests
{
    private static readonly RgbaColor Ink = new(12345, 23456, 34567, 45678);
    private static (int X, int Y, bool On)[] Rectangle(int x, int y, int width, int height)
        => [(x, y, true), (x, y + height, true), (x + width, y + height, true), (x + width, y, true)];
    private static TextLayout Layout(params (int X, int Y, bool On)[][] contours)
    {
        var font = TrueTypeFont.Load(AuthoredTrueType.Build(glyphOverrides: new Dictionary<int, byte[]> { [2] = AuthoredTrueType.Outline(contours) }));
        return TextLayout.Create(font, "A", new TextOptions { FontSize = 10 });
    }
    private static void Seed(ImageBuffer canvas)
    {
        for (var y = 0; y < canvas.Height; y++)
        for (var x = 0; x < canvas.Width; x++)
        for (var c = 0; c < 4; c++) Set(canvas, x, y, c, canvas.BitDepth == 8 ? 17 + c * 31 : 1234 + c * 9876);
    }
    private static int Sample(ImageBuffer image, int x, int y, int c) => image.BitDepth == 8 ? image.GetRow8(y)[x * 4 + c] : image.GetRow16(y)[x * 4 + c];
    private static void Set(ImageBuffer image, int x, int y, int c, int value)
    { if (image.BitDepth == 8) image.GetRow8(y)[x * 4 + c] = (byte)value; else image.GetRow16(y)[x * 4 + c] = (ushort)value; }
    private static int Round(BigInteger numerator, BigInteger denominator) => (int)((2 * numerator + denominator) / (2 * denominator));
    private static void ExpectedPixel(ImageBuffer actual, ImageBuffer before, int x, int y, uint coverage, RgbaColor color)
    {
        var maximum = actual.BitDepth == 8 ? 255 : 65535;
        BigInteger sourceDenominator = 65535L * 65535, sourceAlpha = (long)color.A * coverage;
        var destinationAlpha = Sample(before, x, y, 3);
        var composedAlpha = sourceAlpha * maximum + destinationAlpha * (sourceDenominator - sourceAlpha);
        for (var c = 0; c < 3; c++)
        {
            var source = c == 0 ? color.R : c == 1 ? color.G : color.B;
            var expected = sourceAlpha == 0 ? Sample(before, x, y, c) : Round(source * sourceAlpha * maximum * maximum
                + Sample(before, x, y, c) * destinationAlpha * (sourceDenominator - sourceAlpha) * 65535, composedAlpha * 65535);
            Assert.Equal(expected, Sample(actual, x, y, c));
        }
        Assert.Equal(sourceAlpha == 0 ? destinationAlpha : Round(composedAlpha, sourceDenominator), Sample(actual, x, y, 3));
    }

    [Theory] [InlineData(8)] [InlineData(16)]
    public void FractionalRectangleHasAnalyticalCoverageAndOneFinalColorRounding(int depth)
    {
        var layout = Layout(Rectangle(0, 0, 1000, 600));
        using var canvas = ImageBuffer.Create(14, 12, depth); Seed(canvas); using var before = canvas.Clone();
        ImageDrawing.DrawText(canvas, layout, .25, .5, Ink);
        for (var y = 0; y < canvas.Height; y++)
        for (var x = 0; x < canvas.Width; x++)
        {
            // The authored rectangle occupies [.25,10.25] x [2.5,8.5].
            var width = Math.Max(0, Math.Min(x + 1, 10.25) - Math.Max(x, .25));
            var height = Math.Max(0, Math.Min(y + 1, 8.5) - Math.Max(y, 2.5));
            ExpectedPixel(canvas, before, x, y, (uint)Math.Floor(width * height * 65535 + .5), Ink);
        }
    }

    [Theory] [InlineData(8)] [InlineData(16)]
    public void FullyCoveredTextAgreesWithExistingSourceOverWithoutDepthLoss(int depth)
    {
        var layout = Layout(Rectangle(0, 0, 1000, 600));
        using var canvas = ImageBuffer.Create(14, 12, depth); Seed(canvas); using var expected = canvas.Clone();
        using var source = ImageBuffer.Create(10, 6, 16); ImageDrawing.FillRectangle(source, 0, 0, 10, 6, Ink);
        ImageDrawing.Composite(expected, source, 0, 2); ImageDrawing.DrawText(canvas, layout, 0, 0, Ink);
        for (var y = 0; y < canvas.Height; y++)
            if (depth == 8) Assert.True(expected.GetRow8(y).SequenceEqual(canvas.GetRow8(y)));
            else Assert.True(expected.GetRow16(y).SequenceEqual(canvas.GetRow16(y)));
    }

    [Theory] [InlineData(8)] [InlineData(16)]
    public void NonzeroWindingPreservesHolesAndUnionsSameDirectionOverlaps(int depth)
    {
        var outer = Rectangle(0, 0, 1000, 600); var inner = Rectangle(200, 200, 600, 200);
        var hole = Layout(outer, inner.Reverse().ToArray()); var filled = Layout(outer, inner);
        using var a = ImageBuffer.Create(12, 10, depth); using var b = ImageBuffer.Create(12, 10, depth);
        ImageDrawing.DrawText(a, hole, 0, 0, RgbaColor.From8(255, 0, 0)); ImageDrawing.DrawText(b, filled, 0, 0, RgbaColor.From8(255, 0, 0));
        var maximum = depth == 8 ? 255 : 65535;
        Assert.Equal(0, Sample(a, 3, 5, 3)); Assert.Equal(maximum, Sample(a, 1, 5, 3)); Assert.Equal(maximum, Sample(b, 3, 5, 3));
        // Two partly overlapping contours must have union coverage, not summed coverage.
        var overlap = Layout(Rectangle(0, 0, 100, 100), Rectangle(50, 0, 100, 100));
        using var c = ImageBuffer.Create(4, 10, depth);
        ImageDrawing.DrawText(c, overlap, .25, 0, RgbaColor.From8(255, 0, 0));
        Assert.Equal(Round((BigInteger)maximum * 49151, 65535), Sample(c, 0, 7, 3));
        Assert.Equal(Round((BigInteger)maximum * 49151, 65535), Sample(c, 1, 7, 3));
    }

    [Theory] [InlineData(8)] [InlineData(16)]
    public void CrossingEdgesSplitBandsAndIntegrateBothSidesOfTheIntersection(int depth)
    {
        var layout = Layout([(0, 0, true), (600, 600, true), (0, 600, true), (600, 0, true)]);
        using var canvas = ImageBuffer.Create(9, 11, depth);
        ImageDrawing.DrawText(canvas, layout, .5, .5, RgbaColor.From8(255, 255, 255));
        // A bow-tie's crossing lies at the center of this pixel: two quarter triangles.
        Assert.Equal(depth == 8 ? 128 : 32768, Sample(canvas, 3, 5, 3));
        Assert.Equal(0, Sample(canvas, 0, 5, 3));
    }

    [Theory] [InlineData(8)] [InlineData(16)]
    public void QuadraticCoveragePreservesAreaWithoutReducingTheCanvasToEightBits(int depth)
    {
        var font = TrueTypeFont.Load(AuthoredTrueType.Build()); var layout = TextLayout.Create(font, "g", new TextOptions { FontSize = 10 });
        using var canvas = ImageBuffer.Create(9, 12, depth);
        ImageDrawing.DrawText(canvas, layout, 1, 0, new(12345, 23456, 34567));
        var maximum = depth == 8 ? 255 : 65535; long alpha = 0; var fractional = 0;
        for (var y = 0; y < canvas.Height; y++)
        for (var x = 0; x < canvas.Width; x++)
        {
            var a = Sample(canvas, x, y, 3); alpha += a;
            if (a > 0 && a < maximum) fractional++;
            if (a != 0) Assert.Equal(depth == 8 ? (12345 + 128) / 257 : 12345, Sample(canvas, x, y, 0));
        }
        // Green's theorem gives exactly 200000 design-unit² = 20 pixel².
        Assert.InRange(alpha / (double)maximum, 19.97, 20.03); Assert.True(fractional > 5);
    }

    [Theory] [InlineData(8)] [InlineData(16)]
    public void ClippedRenderingMatchesAnIndependentLargerCanvasAndLayoutIsReusable(int depth)
    {
        var fontBytes = AuthoredTrueType.Build(style: FontStyle.Bold | FontStyle.Italic); var font = TrueTypeFont.Load(fontBytes);
        var layout = TextLayout.Create(font, "AV g\nÁ", new TextOptions { FontSize = 16, WrapWidth = 20, LineSpacing = 1.25 });
        fontBytes.AsSpan().Fill(0xA7); var bounds = layout.Bounds;
        using var canvas = ImageBuffer.Create(13, 18, depth); using var large = ImageBuffer.Create(25, 30, depth);
        ImageDrawing.DrawText(canvas, layout, -3.25, -2.5, Ink); ImageDrawing.DrawText(large, layout, 2.75, 3.5, Ink);
        for (var y = 0; y < canvas.Height; y++)
        for (var x = 0; x < canvas.Width; x++)
        for (var c = 0; c < 4; c++) Assert.Equal(Sample(large, x + 6, y + 6, c), Sample(canvas, x, y, c));
        using var second = ImageBuffer.Create(13, 18, depth); ImageDrawing.DrawText(second, layout, -3.25, -2.5, Ink);
        Assert.Equal(bounds, layout.Bounds); canvas.Dispose();
        Assert.Contains(Enumerable.Range(0, second.Height), y => Enumerable.Range(0, second.Width).Any(x => Sample(second, x, y, 3) > 0));
    }

    [Theory] [InlineData(ImageLimitKind.WorkingBytes)] [InlineData(ImageLimitKind.ContainerItems)]
    [InlineData(ImageLimitKind.Dimension)] [InlineData(ImageLimitKind.PixelCount)]
    public void AdmissionAndGeometryFailuresPrecedeMutation(ImageLimitKind kind)
    {
        var layout = Layout(Rectangle(0, 0, 1000, 600)); using var canvas = ImageBuffer.Create(14, 12, 8); Seed(canvas);
        using var before = canvas.Clone();
        var limits = kind switch
        {
            ImageLimitKind.WorkingBytes => new ImageLimits { MaxWorkingBytes = layout.StorageBytes + canvas.Width * canvas.Height * 4 + 100 },
            ImageLimitKind.ContainerItems => new ImageLimits { MaxContainerItems = 1 },
            ImageLimitKind.Dimension => new ImageLimits { MaxDimension = 9 },
            _ => new ImageLimits { MaxPixels = 59 }
        };
        var failure = Assert.Throws<ImageLimitException>(() => ImageDrawing.DrawText(canvas, layout, 0, 0, Ink, limits));
        Assert.Equal(kind, failure.Limit); Assert.Equal(ImageLimitScope.Operation, failure.Scope);
        for (var y = 0; y < canvas.Height; y++) Assert.True(before.GetRow8(y).SequenceEqual(canvas.GetRow8(y)));
    }

    [Fact]
    public void EmptyTransparentOutsideAndPreCancelledDrawsPreserveEverySample()
    {
        var layout = Layout(Rectangle(0, 0, 1000, 600)); using var canvas = ImageBuffer.Create(14, 12, 8); Seed(canvas); using var before = canvas.Clone();
        ImageDrawing.DrawText(canvas, layout, 0, 0, Ink with { A = 0 });
        ImageDrawing.DrawText(canvas, layout, double.MaxValue, 0, Ink); ImageDrawing.DrawText(canvas, layout, 0, -double.MaxValue, Ink);
        ImageDrawing.DrawText(canvas, TextLayout.Create(layout.Font, ""), 0, 0, Ink);
        using var cts = new CancellationTokenSource(); cts.Cancel();
        var failure = Assert.ThrowsAny<OperationCanceledException>(() => ImageDrawing.DrawText(canvas, layout, 0, 0, Ink, cancellationToken: cts.Token));
        Assert.Equal(cts.Token, failure.CancellationToken);
        for (var y = 0; y < canvas.Height; y++) Assert.True(before.GetRow8(y).SequenceEqual(canvas.GetRow8(y)));
        Assert.Throws<ArgumentOutOfRangeException>(() => ImageDrawing.DrawText(canvas, layout, double.NaN, 0, Ink));
        Assert.Throws<ArgumentOutOfRangeException>(() => ImageDrawing.DrawText(canvas, layout, 0, double.PositiveInfinity, Ink));
        Assert.Throws<ArgumentNullException>(() => ImageDrawing.DrawText(canvas, null!, 0, 0, Ink));
        canvas.Dispose(); Assert.Throws<ObjectDisposedException>(() => ImageDrawing.DrawText(canvas, layout, 0, 0, Ink));
    }
}
