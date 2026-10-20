using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class LanczosFmaTests
{
    private static bool Supported => Avx512F.IsSupported && Avx512DQ.IsSupported && Avx512DQ.VL.IsSupported && Avx2.IsSupported && Fma.IsSupported && Sse41.IsSupported;
    private static int Round(BigInteger n, BigInteger d, int maximum)
        => n <= 0 ? 0 : (int)BigInteger.Min(maximum, (2 * n + d) / (2 * d));

    [Fact]
    public void SelectionUsesLocalBoundsWithoutQueryingSharedAvailability()
    {
        using var source = ImageBuffer.Create(9, 7, 16);
        using var context = new OperationContext(ImageLimits.Default, default);
        using var h = LanczosPlan.Create(9, 5, context); using var v = LanczosPlan.Create(7, 3, context);
        using var root = new TestMemoryBudget(0);
        using var admitted = new OperationContext(new() { MemoryBudget = root }, default);
        var work = admitted.RemainingWork; var bytes = admitted.RemainingWorkingBytes;
        Assert.Equal(Supported, LanczosFmaResize.CanUse(source, 5, 3, h, v, admitted));
        Assert.Equal(work, admitted.RemainingWork); Assert.Equal(bytes, admitted.RemainingWorkingBytes);
        Assert.Equal(0, root.GrowthAttempts); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        using var tightWork = new OperationContext(new() { MaxWork = 1 }, default);
        using var tightMemory = new OperationContext(new() { MaxWorkingBytes = 1 }, default);
        Assert.False(LanczosFmaResize.CanUse(source, 5, 3, h, v, tightWork));
        Assert.False(LanczosFmaResize.CanUse(source, 5, 3, h, v, tightMemory));
    }

    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void ActualFmaRowsAndPixelsStayInsideIndependentIntegerEnvelopes(int depth)
    {
        if (!Supported) return; // Existing public oracle controls qualify the hardware fallback.
        foreach (var (sw, dw) in new[] { (7, 13), (31, 9), (1, 5), (1031, 2) })
        {
            using var source = ImageBuffer.Create(sw, 3, depth); var maximum = depth == 8 ? 255 : 65535;
            for (var y = 0; y < 3; y++)
            for (var x = 0; x < sw; x++)
            {
                source.SetSample(x, y, 0, (x + y) % 2 == 0 ? maximum : 1);
                source.SetSample(x, y, 1, (x + y) % 3 == 0 ? maximum - 1 : maximum / 2);
                source.SetSample(x, y, 2, (x + y) % 5 == 0 ? maximum : 3);
                source.SetSample(x, y, 3, (x + y) % 5 switch { 0 => 0, 1 => 1, 2 => 16, _ => maximum });
            }
            var original = source.Pixels.ToArray();
            using var context = new OperationContext(ImageLimits.Default, default);
            using var h = LanczosPlan.Create(sw, dw, context); using var v = LanczosPlan.Create(3, 5, context);
            var rows = new double[3][]; var exact = new BigInteger[3][];
            for (var y = 0; y < 3; y++)
            {
                rows[y] = new double[dw * 4]; exact[y] = new BigInteger[dw * 4];
                var before = context.RemainingWork;
                LanczosFmaResize.Horizontal(source, y, h, rows[y], context);
                Assert.Equal(before - h.Weights.LongLength * 4 - rows[y].Length, context.RemainingWork);
                for (var x = 0; x < dw; x++)
                for (var c = 0; c < 4; c++)
                {
                    var window = h.Windows[x]; BigInteger expected = 0;
                    for (var i = 0; i < window.Count; i++)
                        expected += (BigInteger)source.Sample(window.First + i, y, 3)
                            * (c == 3 ? 1 : source.Sample(window.First + i, y, c)) * h.Weights[window.Offset + i];
                    exact[y][x * 4 + c] = expected;
                    Assert.True(BigInteger.Abs((BigInteger)rows[y][x * 4 + c] - expected)
                        <= (c == 3 ? 0 : (BigInteger)512));
                }
            }
            var errors = LanczosFmaResize.Errors(v.MaximumCount);
            for (var y = 0; y < 5; y++)
            {
                var sums = new double[dw * 4]; var integer = new BigInteger[dw * 4]; var window = v.Windows[y];
                for (var i = 0; i < window.Count; i++)
                {
                    var weight = v.Weights[window.Offset + i]; var sy = window.First + i;
                    LanczosFmaResize.Accumulate(rows[sy], sums, weight, context);
                    for (var c = 0; c < integer.Length; c++) integer[c] += exact[sy][c] * weight;
                }
                for (var x = 0; x < dw; x++)
                {
                    var at = x * 4; var alpha = Round(integer[at + 3], BigInteger.One << 60, maximum);
                    for (var c = 0; c < 4; c++) Assert.True(BigInteger.Abs((BigInteger)sums[at + c] - integer[at + c])
                        <= (BigInteger)(c == 3 ? errors.Alpha : errors.Rgb));
                    var expected = new int[4]; expected[3] = alpha;
                    for (var c = 0; c < 3; c++) expected[c] = alpha == 0 ? 0 : Round(integer[at + c], integer[at + 3], maximum);
                    var fallback = LanczosFmaResize.ExactPixel(source, x, y, h, v, context);
                    for (var c = 0; c < 4; c++) Assert.Equal(expected[c], fallback.GetElement(c));
                    if (LanczosFmaResize.TryNormalize(Vector256.Create(sums.AsSpan(at, 4)), errors.Rgb + 65535d * errors.Alpha,
                        errors.Alpha, maximum, out var pixel))
                        for (var c = 0; c < 4; c++) Assert.Equal(expected[c], pixel.GetElement(c));
                }
            }
            Assert.Equal(original, source.Pixels.ToArray());
        }
    }

    [Fact]
    public void ErrorEnvelopeKeepsAllInt32TapPrefixesInsideItsExponentBounds()
    {
        BigInteger m = 65535, b = BigInteger.One << 31, n = int.MaxValue;
        var e = LanczosFmaResize.Errors(int.MaxValue);
        Assert.Equal((1 + 2 * n) << 40, (BigInteger)e.Rgb);
        Assert.Equal(n << 25, (BigInteger)e.Alpha);
        Assert.True(512 < (BigInteger.One << 63) - m * m * b);
        Assert.True((BigInteger)e.Rgb < (BigInteger.One << 94) - m * m * b * b);
        Assert.True((BigInteger)e.Alpha < (BigInteger.One << 78) - m * b * b);
        Assert.True(m * b < BigInteger.One << 53);
    }

    [Theory]
    [InlineData(255)] [InlineData(65535)]
    public void PerturbedCertificatesCannotMoveClippedHalfOrAlphaBins(int maximum)
    {
        if (!Supported) return;
        Int128 one = (Int128)1 << 60; var e = LanczosFmaResize.Errors(1);
        foreach (var d in new[] { -one, (Int128)0, one / 2 - 1, one / 2, one / 2 + 1, one, 7 * one + 123, maximum * one + 1 })
        foreach (var q in new[] { -1, 0, 1, maximum / 2, maximum - 1, maximum, maximum + 1 })
        foreach (var delta in new[] { -1, 0, 1 })
        foreach (var sign in new[] { -1, 0, 1 })
        {
            Int128[] exact = [q * d + d / 2 + delta, maximum * d + delta, d / 3, d];
            var value = Vector256.Create((double)exact[0] + sign * e.Rgb / 2, (double)exact[1] - sign * e.Rgb / 2,
                (double)exact[2], (double)d + sign * e.Alpha / 4);
            for (var c = 0; c < 4; c++) Assert.True(BigInteger.Abs((BigInteger)value.GetElement(c) - (BigInteger)exact[c])
                <= (BigInteger)(c == 3 ? e.Alpha : e.Rgb));
            if (!LanczosFmaResize.TryNormalize(value, e.Rgb + 65535d * e.Alpha, e.Alpha, maximum, out var result)) continue;
            var alpha = Round((BigInteger)d, BigInteger.One << 60, maximum);
            for (var c = 0; c < 4; c++) Assert.Equal(c == 3 ? alpha : alpha == 0 ? 0
                : Round((BigInteger)exact[c], (BigInteger)d, maximum), result.GetElement(c));
        }
        Assert.False(LanczosFmaResize.TryNormalize(Vector256.Create(0d, 0, 0, (double)(one / 2)),
            e.Rgb + 65535d * e.Alpha, e.Alpha, maximum, out _));
    }

    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void HalfAlphaUsesExactHalfUpAndTightWorkKeepsTheOriginalPath(int depth)
    {
        using var source = ImageBuffer.Create(2, 1, depth);
        for (var c = 0; c < 3; c++) { source.SetSample(0, 0, c, depth == 8 ? 255 : 65535); source.SetSample(1, 0, c, c + 7); }
        source.SetSample(1, 0, 3, 1);
        using var full = ImageTransforms.Resize(source, 1, 1, ResizeFilter.Lanczos3);
        Assert.Equal(1, full.Sample(0, 0, 3));
        for (var c = 0; c < 3; c++) Assert.Equal(c + 7, full.Sample(0, 0, c));
        long low = 1, high = 10000;
        while (low < high)
        {
            var middle = low + (high - low) / 2;
            try { using var result = ImageTransforms.Resize(source, 1, 1, ResizeFilter.Lanczos3, new() { MaxWork = middle }); high = middle; }
            catch (ImageLimitException error) when (error.Limit == ImageLimitKind.Work) { low = middle + 1; }
        }
        using var tight = ImageTransforms.Resize(source, 1, 1, ResizeFilter.Lanczos3, new() { MaxWork = low });
        Assert.Equal(full.Pixels.ToArray(), tight.Pixels.ToArray());
    }
}
