using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8lColorHistogramBoundaryTests
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void BinsAndFirstNonzeroOrderMatchSignedFloorOracleAcrossPartialTiles(bool redPlane)
    {
        const int width = 40, height = 19;
        using var root = new TestMemoryBudget(1);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        static int Signed(int value) => (value & 255) < 128 ? value & 255 : (value & 255) - 256;
        foreach (var repeated in new[] { false, true })
        {
            var pixels = Enumerable.Range(0, width * height).Select(i => unchecked((uint)(i + 1) * 0x9e3779b9u) ^ (uint)(i * 37) << 8).ToArray();
            pixels[41] = 0; pixels[42] = 0x7fffffffu; pixels[43] = 0x80000000u; pixels[44] = uint.MaxValue;
            if (repeated) for (var i = 0; i < pixels.Length; i++) pixels[i] = pixels[i % 11];
            var original = pixels.ToArray();
            foreach (var shape in new[] { (1, 1), (7, 7), (8, 8), (9, 8), (16, 16), (17, 9) })
            {
                var histogram = new int[264]; var expected = new int[256]; var active = new int[264];
                for (var coefficient = 0; coefficient < 256; coefficient++)
                {
                    var gr = coefficient - 384; var gb = coefficient * 73 - 257; var rb = 257 - coefficient * 11;
                    Array.Fill(histogram, -719); Array.Fill(active, -719);
                    for (var i = 0; i < 256; i++) histogram[i + 4] = expected[i] = i % 3;
                    var order = new List<int>();
                    for (var y = 1; y <= shape.Item2; y++)
                    for (var x = 1; x <= shape.Item1; x++)
                    {
                        var pixel = pixels[y * width + x]; var green = Signed((int)(pixel >> 8));
                        var transformed = redPlane ? (int)(pixel >> 16 & 255) - (int)Math.Floor(Signed(gr) * green / 32d)
                            : (int)(pixel & 255) - (int)Math.Floor(Signed(gb) * green / 32d)
                                - (int)Math.Floor(Signed(rb) * Signed((int)(pixel >> 16)) / 32d);
                        var bin = (transformed % 256 + 256) % 256;
                        if (expected[bin]++ == 0) order.Add(bin);
                    }
                    var activeCount = Vp8lReferenceColorTransform.Histogram(pixels, width, 1, 1, 1 + shape.Item1, 1 + shape.Item2,
                        redPlane, gr, gb, rb, histogram.AsSpan(4, 256), active.AsSpan(4, 256), context);
                    Assert.Equal(expected, histogram.AsSpan(4, 256).ToArray()); Assert.Equal(order, active.AsSpan(4, activeCount).ToArray());
                    Assert.All(histogram.Take(4).Concat(histogram.TakeLast(4)), value => Assert.Equal(-719, value));
                    Assert.All(active.Take(4).Concat(active.Skip(4 + activeCount)), value => Assert.Equal(-719, value));
                }
            }
            Assert.Equal(original, pixels);
        }
        Assert.Equal(0, root.GrowthAttempts); Assert.Equal(0, root.Bytes);
    }

    [Theory]
    [InlineData(1, 1, 9)] [InlineData(513, 2, 3)]
    [InlineData(17, 17, 4)] [InlineData(33, 33, 5)]
    public void SmallAndLargeTileSearchAdmitsOwnedCapacityAndRefusalCleansBeforeMutation(int width, int height, int bits)
    {
        var pixels = new uint[width * height];
        var mapBytes = ((width + (1 << bits) - 1) >> bits) * ((height + (1 << bits) - 1) >> bits) * 4;
        const int scratch = 7296;
        using var root = new TestMemoryBudget(mapBytes + scratch);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        Assert.True(root.TryReserve(1, out var held));
        var failure = Assert.Throws<ImageLimitException>(() => Vp8lReferenceColorTransform.SelectAndApply(pixels, width, bits, 75, context));
        Assert.Equal(ImageLimitScope.SharedBudget, failure.Scope); Assert.Equal(1, root.Refusals); Assert.Equal(1, root.Bytes);
        Assert.All(pixels, value => Assert.Equal(0u, value)); held!.Dispose();
        using var result = Vp8lReferenceColorTransform.SelectAndApply(pixels, width, bits, 75, context);
        Assert.Equal(mapBytes + scratch, root.Peak); Assert.Equal(mapBytes, root.Bytes);
        result.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
