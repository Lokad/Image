using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8lPredictorHistogramTests
{
    [Fact]
    public void SharedPredictionsPreserveAllChannelRoundingClampsAndSelectionTies()
    {
        uint state = 0x6d2b79f5;
        uint Next() { state ^= state << 13; state ^= state >> 17; state ^= state << 5; return state; }
        var actual = new int[14 * 1024]; var expected = new int[14 * 1024];
        foreach (var l in new uint[] { 0, 0x01010101, 0x7f7f7f7f, 0x80808080, 0xfefefefe, uint.MaxValue })
        foreach (var t in new uint[] { 0, 0x01020304, 0x807f807f, uint.MaxValue })
        foreach (var c in new uint[] { 0, 0x01010101, 0x7f807f80, uint.MaxValue })
            Check(Next(), l, t, c, Next());
        for (var i = 0; i < 32768; i++) Check(Next(), Next(), Next(), Next(), Next());
        // Mode eleven compares the summed four-channel errors, with top winning ties.
        Check(0x01234567, 0xff0000ff, 0x00ffff00, 0x80808080, 0);
        Check(0x89abcdef, 0x01020304, 0x04030201, 0, uint.MaxValue);

        void Check(uint sample, uint left, uint top, uint corner, uint right)
        {
            Array.Clear(actual); Array.Clear(expected);
            Vp8lPredictorHistogram.AccumulateInterior(actual, sample, left, top, corner, right);
            for (var mode = 0; mode < 14; mode++)
                Add(expected, mode, sample, WebpLosslessDecoder.Predict(mode, left, top, corner, right));
            Assert.True(expected.AsSpan().SequenceEqual(actual));
        }
    }

    [Fact]
    public void TileEdgesAndWrappedTopRightMatchTheProductionPredictorsWithoutOwningStorage()
    {
        using var root = new TestMemoryBudget(0);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var expected = new int[14 * 1024]; var actual = new int[14 * 1024]; uint state = 0x12345678;
        foreach (var (width, height) in new[] { (1, 1), (1, 17), (17, 1), (9, 7), (33, 19) })
        {
            var pixels = new uint[width * height];
            for (var at = 0; at < pixels.Length; at++)
            { state ^= state << 13; state ^= state >> 17; state ^= state << 5; pixels[at] = state; }
            var before = pixels.ToArray();
            foreach (var bits in new[] { 2, 5, 9 })
            for (var y0 = 0; y0 < height; y0 += 1 << bits)
            for (var x0 = 0; x0 < width; x0 += 1 << bits)
            {
                Array.Clear(expected); Array.Clear(actual);
                var endX = Math.Min(x0 + (1 << bits), width); var endY = Math.Min(y0 + (1 << bits), height);
                for (var y = y0; y < endY; y++) for (var x = x0; x < endX; x++)
                {
                    var at = y * width + x;
                    for (var mode = 0; mode < 14; mode++)
                    {
                        var prediction = y == 0 ? x == 0 ? 0xff000000u : pixels[at - 1] : x == 0 ? pixels[at - width]
                            : WebpLosslessDecoder.Predict(mode, pixels[at - 1], pixels[at - width],
                                pixels[at - width - 1], pixels[x == width - 1 ? y * width : at - width + 1]);
                        Add(expected, mode, pixels[at], prediction);
                    }
                }
                Vp8lPredictorHistogram.Build(pixels, width, height, x0, y0, bits, actual, context);
                Assert.True(expected.AsSpan().SequenceEqual(actual));
            }
            Assert.Equal(before, pixels);
        }
        Assert.Equal(0, root.Bytes); Assert.Equal(0, root.GrowthAttempts);
    }

    [Fact]
    public void CanceledPreparationDoesNotReadInputOrWriteBorrowedHistograms()
    {
        using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(ImageLimits.Default, cancellation.Token);
        cancellation.Cancel();
        var histogram = Enumerable.Repeat(12345, 14 * 1024).ToArray();
        Assert.Throws<OperationCanceledException>(() => Vp8lPredictorHistogram.Build([], 1, 1, 0, 0, 2, histogram, context));
        Assert.All(histogram, count => Assert.Equal(12345, count));
    }

    private static void Add(Span<int> histogram, int mode, uint sample, uint prediction)
    {
        for (var plane = 0; plane < 4; plane++)
        {
            var shift = (3 - plane) * 8;
            histogram[mode * 1024 + plane * 256 + (int)(unchecked((sample >> shift) - (prediction >> shift)) & 255)]++;
        }
    }
}
