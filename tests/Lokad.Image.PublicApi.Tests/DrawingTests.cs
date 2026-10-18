using System.Numerics;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class DrawingTests
{
    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void FillClipsReplacesAlphaAndPreservesOtherPixelsAndOwners(int depth)
    {
        using var canvas = OriginalPixels.Buffer(9, 7, depth); using var survivor = canvas.Clone();
        var saved = OriginalPixels.Bytes(survivor); var color = new RgbaColor(12345, 65535, 1, 0);
        ImageDrawing.FillRectangle(canvas, -2, 2, 5, 3, color);
        for (var y = 0; y < 7; y++) for (var x = 0; x < 9; x++) for (var c = 0; c < 4; c++)
        {
            var value = new int[] { color.R, color.G, color.B, color.A }[c];
            if (depth == 8) value = (value + 128) / 257;
            Assert.Equal(y is >= 2 and < 5 && x < 3 ? value : Sample(survivor, x, y, c), Sample(canvas, x, y, c));
        }
        Assert.Equal(saved, OriginalPixels.Bytes(survivor));
        Assert.Equal(new RgbaColor(257, 514, 65535, 1028), RgbaColor.From8(1, 2, 255, 4));
        ImageDrawing.FillRectangle(canvas, int.MinValue, int.MinValue, int.MaxValue, int.MaxValue, color);
        ImageDrawing.FillRectangle(canvas, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, color);
        ImageDrawing.FillRectangle(canvas, 0, 0, 0, 1, color);
    }

    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void CopyClipsDestinationAndKeepsEverySourceSampleAndOwner(int depth)
    {
        using var source = OriginalPixels.Buffer(9, 7, depth); using var canvas = ImageBuffer.Create(7, 5, depth);
        var saved = OriginalPixels.Bytes(source);
        ImageDrawing.CopyRegion(source, 2, 1, 5, 5, canvas, -2, 2);
        for (var y = 0; y < 5; y++) for (var x = 0; x < 7; x++) for (var c = 0; c < 4; c++)
            Assert.Equal(x < 3 && y >= 2 ? Sample(source, x + 4, y - 1, c) : 0, Sample(canvas, x, y, c));
        Assert.Equal(saved, OriginalPixels.Bytes(source));
        source.Dispose(); Assert.Equal(7 * 4, depth == 8 ? canvas.GetRow8(0).Length : canvas.GetRow16(0).Length);
    }

    [Theory]
    [InlineData(8, 1, 0)] [InlineData(8, -1, 0)] [InlineData(8, 0, 1)] [InlineData(8, 0, -1)]
    [InlineData(16, 1, 1)] [InlineData(16, -1, -1)] [InlineData(16, 1, -1)] [InlineData(16, -1, 1)]
    public void OverlappingCopyHasSnapshotSemanticsAcrossChunksAndRows(int depth, int dx, int dy)
    {
        using var canvas = OriginalPixels.Buffer(8203, 4, depth); using var snapshot = canvas.Clone();
        var sx = dx < 0 ? 1 : 0; var sy = dy < 0 ? 1 : 0; var x = sx + dx; var y = sy + dy;
        ImageDrawing.CopyRegion(canvas, sx, sy, 8202, 3, canvas, x, y);
        for (var row = 0; row < 4; row++) for (var col = 0; col < 8203; col++) for (var c = 0; c < 4; c++)
            Assert.Equal(col >= x && col < x + 8202 && row >= y && row < y + 3
                ? Sample(snapshot, col - dx, row - dy, c) : Sample(snapshot, col, row, c), Sample(canvas, col, row, c));
    }

    [Theory]
    [InlineData(8, 8)] [InlineData(8, 16)] [InlineData(16, 8)] [InlineData(16, 16)]
    public void CompositeMatchesIndependentExactNormalizedRationals(int sourceDepth, int canvasDepth)
    {
        using var source = OriginalPixels.Buffer(19, 11, sourceDepth); using var canvas = OriginalPixels.Buffer(19, 11, canvasDepth);
        var sourceMax = sourceDepth == 8 ? 255 : 65535; var canvasMax = canvasDepth == 8 ? 255 : 65535;
        for (var y = 0; y < 11; y++) for (var x = 0; x < 19; x++)
        {
            Set(source, x, y, 3, new[] { 0, 1, sourceMax / 2, sourceMax - 1, sourceMax }[(x + y) % 5]);
            Set(canvas, x, y, 3, new[] { canvasMax, canvasMax - 1, canvasMax / 2, 1, 0 }[(x * 3 + y) % 5]);
        }
        using var before = canvas.Clone(); var saved = OriginalPixels.Bytes(source);
        ImageDrawing.Composite(canvas, source, -2, 1);
        for (var y = 0; y < 11; y++) for (var x = 0; x < 19; x++)
        {
            var expected = x < 17 && y >= 1 ? Reference(source, x + 2, y - 1, before, x, y)
                : Enumerable.Range(0, 4).Select(c => Sample(before, x, y, c)).ToArray();
            for (var c = 0; c < 4; c++) Assert.Equal(expected[c], Sample(canvas, x, y, c));
        }
        Assert.Equal(saved, OriginalPixels.Bytes(source));
    }

    [Theory]
    [InlineData(8, 1, 0)] [InlineData(8, -1, 1)] [InlineData(8, 0, 0)]
    [InlineData(16, 1, 1)] [InlineData(16, -1, -1)] [InlineData(16, 0, 0)]
    public void CompositeSelfOverlapUsesOriginalSourceAndDestination(int depth, int dx, int dy)
    {
        using var canvas = OriginalPixels.Buffer(521, 4, depth); using var snapshot = canvas.Clone();
        ImageDrawing.Composite(canvas, canvas, dx, dy);
        for (var y = 0; y < 4; y++) for (var x = 0; x < 521; x++)
        {
            var expected = x - dx is >= 0 and < 521 && y - dy is >= 0 and < 4
                ? Reference(snapshot, x - dx, y - dy, snapshot, x, y)
                : Enumerable.Range(0, 4).Select(c => Sample(snapshot, x, y, c)).ToArray();
            for (var c = 0; c < 4; c++) Assert.Equal(expected[c], Sample(canvas, x, y, c));
        }
    }

    [Theory]
    [InlineData("fill", 8)] [InlineData("copy", 8)] [InlineData("composite", 8)]
    [InlineData("fill", 16)] [InlineData("copy", 16)] [InlineData("composite", 16)]
    public void ExactMemoryWorkAndGeometryAdmissionPrecedeMutation(string operation, int depth)
    {
        using var canvas = OriginalPixels.Buffer(9, 7, depth); using var source = canvas.Clone(); var saved = OriginalPixels.Bytes(canvas);
        var bytes = saved.Length * (operation == "fill" ? 1L : 2L);
        var work = 9 * 7 * (operation == "composite" ? 16L : 4L);
        var limits = new ImageLimits { MaxWorkingBytes = bytes, MaxWork = work };
        var failure = Assert.Throws<ImageLimitException>(() => Apply(operation, canvas, source, limits with { MaxWorkingBytes = bytes - 1 }));
        Assert.Equal(ImageLimitKind.WorkingBytes, failure.Limit); Assert.Equal(saved, OriginalPixels.Bytes(canvas));
        failure = Assert.Throws<ImageLimitException>(() => Apply(operation, canvas, source, limits with { MaxDimension = 8 }));
        Assert.Equal(ImageLimitKind.Dimension, failure.Limit); Assert.Equal(saved, OriginalPixels.Bytes(canvas));
        failure = Assert.Throws<ImageLimitException>(() => Apply(operation, canvas, source, limits with { MaxPixels = 62 }));
        Assert.Equal(ImageLimitKind.PixelCount, failure.Limit); Assert.Equal(saved, OriginalPixels.Bytes(canvas));
        using var token = new CancellationTokenSource(); token.Cancel();
        Assert.ThrowsAny<OperationCanceledException>(() => Apply(operation, canvas, source, limits, token.Token));
        Assert.Equal(saved, OriginalPixels.Bytes(canvas)); Apply(operation, canvas, source, limits);
        Assert.Equal(saved, OriginalPixels.Bytes(source));
    }

    [Theory]
    [InlineData("fill")] [InlineData("copy")] [InlineData("composite")]
    public void WorkLimitLeavesOnlyCompletedChunksAndSelfOperationsAdmitOneOwner(string operation)
    {
        using var canvas = ImageBuffer.Create(5000, 1, 8); using var source = ImageBuffer.Create(5000, 1, 8);
        ImageDrawing.FillRectangle(source, 0, 0, 5000, 1, RgbaColor.From8(1, 2, 3));
        var chunk = operation == "composite" ? 256 : 4096;
        var limits = new ImageLimits { MaxWorkingBytes = 40000, MaxWork = chunk * (operation == "composite" ? 16L : 4L) };
        var failure = Assert.Throws<ImageLimitException>(() => Apply(operation, canvas, source, limits));
        Assert.Equal(ImageLimitKind.Work, failure.Limit);
        for (var x = 0; x < 5000; x++) Assert.Equal(x < chunk ? 255 : 0, canvas.GetRow8(0)[x * 4 + 3]);
        limits = limits with { MaxWorkingBytes = 20000, MaxWork = 100000 };
        Apply(operation, source, source, limits);
    }

    [Fact]
    public void InvalidRectanglesDepthsAndDisposedOwnersDoNotMutateCanvas()
    {
        using var source = OriginalPixels.Buffer(9, 7, 8); using var canvas = source.Clone(); using var word = ImageBuffer.Create(9, 7, 16);
        var saved = OriginalPixels.Bytes(canvas);
        Assert.Throws<ArgumentOutOfRangeException>(() => ImageDrawing.FillRectangle(canvas, 0, 0, -1, 1, default));
        Assert.Throws<ArgumentOutOfRangeException>(() => ImageDrawing.CopyRegion(source, int.MaxValue, 0, int.MaxValue, 1, canvas, 0, 0));
        Assert.Throws<ArgumentException>(() => ImageDrawing.CopyRegion(word, 0, 0, 1, 1, canvas, 0, 0));
        source.Dispose(); Assert.Throws<ObjectDisposedException>(() => ImageDrawing.Composite(canvas, source, 0, 0));
        Assert.Equal(saved, OriginalPixels.Bytes(canvas));
    }

    private static void Apply(string operation, ImageBuffer canvas, ImageBuffer source, ImageLimits limits, CancellationToken token = default)
    {
        if (operation == "fill") ImageDrawing.FillRectangle(canvas, 0, 0, canvas.Width, canvas.Height, RgbaColor.From8(1, 2, 3), limits, token);
        else if (operation == "copy") ImageDrawing.CopyRegion(source, 0, 0, source.Width, source.Height, canvas, 0, 0, limits, token);
        else ImageDrawing.Composite(canvas, source, 0, 0, limits, token);
    }

    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void MaximumArithmeticAndExactHalfTiesDoNotLosePrecision(int depth)
    {
        using var source = ImageBuffer.Create(1, 1, depth); using var canvas = ImageBuffer.Create(1, 1, depth);
        var max = depth == 8 ? 255 : 65535;
        for (var c = 0; c < 4; c++) { Set(source, 0, 0, c, max); Set(canvas, 0, 0, c, max); }
        ImageDrawing.Composite(canvas, source, 0, 0);
        for (var c = 0; c < 4; c++) Assert.Equal(max, Sample(canvas, 0, 0, c));
        for (var c = 0; c < 3; c++) { Set(source, 0, 0, c, 1); Set(canvas, 0, 0, c, max); }
        Set(source, 0, 0, 3, 2); Set(canvas, 0, 0, 3, 2);
        ImageDrawing.Composite(canvas, source, 0, 0);
        for (var c = 0; c < 3; c++) Assert.Equal((max + 1) / 2, Sample(canvas, 0, 0, c));
        Assert.Equal(4, Sample(canvas, 0, 0, 3));
    }

    [Fact]
    public void CodedCoordinatesPreservePendingOrientation()
    {
        var data = OriginalExif.Attach(OriginalExif.Create(MetadataProfile.Png8, 9, 7), ImageOrientation.Rotate90, false);
        using var canvas = ImageCodec.Decode(data); using var source = ImageBuffer.Create(1, 1, 8);
        ImageDrawing.FillRectangle(source, 0, 0, 1, 1, RgbaColor.From8(1, 2, 3));
        ImageDrawing.FillRectangle(canvas, 0, 0, 1, 1, RgbaColor.From8(7, 8, 9));
        ImageDrawing.CopyRegion(source, 0, 0, 1, 1, canvas, 0, 0);
        ImageDrawing.Composite(canvas, source, 0, 0);
        Assert.Equal(new byte[] { 1, 2, 3, 255 }, canvas.GetRow8(0)[..4].ToArray());
        Assert.Equal((9, 7, ImageOrientation.Rotate90), (canvas.Width, canvas.Height, canvas.Orientation));
    }
    private static int Sample(ImageBuffer image, int x, int y, int c) => image.BitDepth == 8 ? image.GetRow8(y)[x * 4 + c] : image.GetRow16(y)[x * 4 + c];
    private static void Set(ImageBuffer image, int x, int y, int c, int value)
    { if (image.BitDepth == 8) image.GetRow8(y)[x * 4 + c] = (byte)value; else image.GetRow16(y)[x * 4 + c] = (ushort)value; }
    private static int[] Reference(ImageBuffer source, int sx, int sy, ImageBuffer canvas, int x, int y)
    {
        var s = Enumerable.Range(0, 4).Select(c => new BigInteger(Sample(source, sx, sy, c)) * (source.BitDepth == 8 ? 257 : 1)).ToArray();
        var d = Enumerable.Range(0, 4).Select(c => new BigInteger(Sample(canvas, x, y, c)) * (canvas.BitDepth == 8 ? 257 : 1)).ToArray();
        if (s[3] == 0) return Enumerable.Range(0, 4).Select(c => Sample(canvas, x, y, c)).ToArray();
        var max = canvas.BitDepth == 8 ? 255 : 65535; var coverage = s[3] * 65535 + d[3] * (65535 - s[3]);
        var output = new int[4];
        for (var c = 0; c < 3; c++) output[c] = Round((s[c] * s[3] * 65535 + d[c] * d[3] * (65535 - s[3])) * max, 65535 * coverage);
        output[3] = Round(coverage * max, (BigInteger)65535 * 65535); return output;
    }
    private static int Round(BigInteger n, BigInteger d)
    { var quotient = BigInteger.DivRem(n, d, out var remainder); return (int)(quotient + (2 * remainder >= d ? 1 : 0)); }
}
