using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8lGreenPredictorHistogramTests
{
    [Fact]
    public void OriginalGreenPlaneAndEdgesKeepEveryFrozenHistogramAndCharge()
    {
        uint state = 0x6d2b79f5;
        uint Next() { state ^= state << 13; state ^= state >> 17; state ^= state << 5; return state; }
        var interior = new int[14 * 1024];
        for (var i = 0; i < 65536; i++)
        {
            var l = (int)(Next() & 255); var t = (int)(Next() & 255);
            var c = (int)(Next() & 255); var r = (int)(Next() & 255);
            var sample = i & 255;
            Vp8lGreenPredictorHistogram.AccumulateInterior(interior, sample, l, t, c, r);
            for (var mode = 0; mode < 14; mode++)
            {
                var prediction = (int)(WebpLosslessDecoder.Predict(mode, (uint)l << 8, (uint)t << 8, (uint)c << 8, (uint)r << 8) >> 8 & 255);
                var at = mode * 1024 + 512 + ((sample - prediction) & 255);
                Assert.Equal(1, interior[at]); interior[at] = 0;
            }
        }
        var original = OriginalPixels.Generate(513, 259, 8); original[3] = original[513 * 4 - 1] = 0;
        Assert.Equal("70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80", Convert.ToHexStringLower(SHA256.HashData(original)));
        var expected = new int[14 * 1024]; var actual = new int[14 * 1024]; var tiles = 0;
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        using var context = new OperationContext(ImageLimits.Default, default);
        foreach (var shape in new[] { (513, 259), (1, 1), (1, 17), (17, 1), (9, 7), (33, 19) })
        {
            var width = shape.Item1; var height = shape.Item2; var pixels = new uint[width * height];
            for (var i = 0; i < pixels.Length; i++) pixels[i] = (uint)(shape == (513, 259) ? original[i * 4 + 3] : (byte)Next()) << 8;
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
                        var prediction = y == 0 ? x == 0 ? 0xff000000u : pixels[at - 1]
                            : x == 0 ? pixels[at - width]
                            : WebpLosslessDecoder.Predict(mode, pixels[at - 1], pixels[at - width], pixels[at - width - 1], pixels[x == width - 1 ? y * width : at - width + 1]);
                        for (var plane = 0; plane < 4; plane++)
                        {
                            var shift = (3 - plane) * 8;
                            expected[mode * 1024 + plane * 256 + (int)(unchecked((pixels[at] >> shift) - (prediction >> shift)) & 255)]++;
                        }
                    }
                }
                Vp8lGreenPredictorHistogram.Build(pixels, width, height, x0, y0, bits, actual, context);
                Assert.True(expected.AsSpan().SequenceEqual(actual));
                hash.AppendData(MemoryMarshal.AsBytes(actual.AsSpan())); tiles++;
            }
            Assert.Equal(before, pixels);
        }
        Assert.Equal(8613, tiles);
        Assert.Equal("0fe435db47a1b0397a9502f72ddf651e5c2ef5e43c898e62a0c890f8f97e7689", Convert.ToHexStringLower(hash.GetHashAndReset()));
    }

    private static uint[] Input(OperationContext context)
    {
        var rgba = OriginalPixels.Generate(19, 7, 8); context.Reserve(rgba.Length);
        var result = new uint[19 * 7];
        for (var i = 0; i < result.Length; i++) result[i] = (uint)rgba[i * 4 + 3] << 8;
        return result;
    }

    [Theory]
    [InlineData(2, 3)]
    [InlineData(5, 5)]
    public void SpecializedAndGenericPlansKeepResidualsCostsAndLifetime(int minimum, int maximum)
    {
        using var root = new TestMemoryBudget(2_000_000);
        using var generic = new OperationContext(new() { MemoryBudget = root }, default);
        using var specialized = new OperationContext(new() { MemoryBudget = root }, default);
        var first = Input(generic); var second = Input(specialized);
        using var a = Vp8lReferencePredictor.SelectAndApply(first, 19, minimum, maximum, false, generic);
        using var b = Vp8lReferencePredictor.SelectAndApply(second, 19, minimum, maximum, false, specialized, greenOnly: true);
        Assert.Equal((a.SelectedBits, a.StoredBits, a.EntropyCost), (b.SelectedBits, b.StoredBits, b.EntropyCost));
        Assert.True(a.Modes.SequenceEqual(b.Modes)); Assert.Equal(first, second);
        Assert.Equal(2 * 19 * 7 * 4 + a.Modes.Length + b.Modes.Length, root.Bytes);
        a.Dispose(); b.Dispose(); Assert.Equal(2 * 19 * 7 * 4, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void RefusalAtAnalysisResultOrRowsIsTerminalAndAllowsRecovery(int phase)
    {
        using var root = new TestMemoryBudget(100_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var pixels = Input(context); var original = pixels.ToArray();
        root.RefuseAtGrowth = root.GrowthAttempts + phase;
        var error = Assert.Throws<ImageLimitException>(() => Vp8lReferencePredictor.SelectAndApply(pixels, 19, 5, 5, false, context, greenOnly: true));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        Assert.Equal(1, root.Refusals); Assert.Equal(root.RefuseAtGrowth, root.GrowthAttempts);
        Assert.Equal(pixels.Length * 4, root.Bytes); Assert.Equal(original, pixels);
        root.RefuseAtGrowth = 0;
        using var result = Vp8lReferencePredictor.SelectAndApply(pixels, 19, 5, 5, false, context, greenOnly: true);
        result.Dispose(); Assert.Equal(pixels.Length * 4, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void CancellationAtRowsReleasesModesAndAnalysisWithoutChangingSource()
    {
        using var root = new TestMemoryBudget(100_000); using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
        var pixels = Input(context); var original = pixels.ToArray(); var rowGrowth = root.GrowthAttempts + 3;
        root.Admitted = _ => { if (root.GrowthAttempts == rowGrowth) cancellation.Cancel(); };
        Assert.Throws<OperationCanceledException>(() => Vp8lReferencePredictor.SelectAndApply(pixels, 19, 5, 5, false, context, greenOnly: true));
        Assert.Equal(original, pixels); Assert.Equal(pixels.Length * 4, root.Bytes); Assert.Equal(0, root.Refusals);
        root.Admitted = null;
        using var nextContext = new OperationContext(new() { MemoryBudget = root }, default);
        using var next = Vp8lReferencePredictor.SelectAndApply(pixels, 19, 5, 5, false, nextContext, greenOnly: true);
        next.Dispose(); Assert.Equal(pixels.Length * 4, root.Bytes);
    }
}
