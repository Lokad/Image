using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class JpegLowFrequencyRowsTests
{
    [Theory] [InlineData(false)] [InlineData(true)]
    public void LowRowsAndSupportBoundariesKeepIndependentScalarPixels(bool scanOrder)
    {
        int[] edges = [int.MinValue, int.MaxValue, -65537, -32769, -32768, -1025,
            -1024, -33, -32, -1, 0, 1, 31, 32, 1023, 4095, 32767, 65535, 65536];
        ushort[] scales = [0, 1, 255, 32767, 32768, 65534, 65535];
        var coefficients = new int[66]; var quantizers = new ushort[66];
        coefficients[0] = 739519; coefficients[^1] = -739519;
        quantizers[0] = quantizers[^1] = 7351;
        var scan = new int[64]; var output = new byte[80];
        var expected = new byte[64]; var kernel = BuildKernel(); var random = new Random(774);
        using var root = new TestMemoryBudget(0);
        using var context = new OperationContext(new() { MemoryBudget = root, MaxWorkingBytes = 1 }, default);
        for (var sample = 0; sample < 4096; sample++)
        {
            Array.Clear(coefficients, 1, 64);
            for (var k = 0; k < 64; k++) quantizers[1 + k] = scales[(sample + k) % scales.Length];
            foreach (var at in new[] { 0, 1, 8, 9 })
                coefficients[1 + at] = sample < 2048 ? edges[(sample + at) % edges.Length] : random.Next(int.MinValue, int.MaxValue);
            // Exercise both constant-row wrap boundaries and nonconstant-row
            // saturation, including each side of the low-support eligibility.
            if (sample % 4 == 0) coefficients[2] = 0;
            if (sample % 4 <= 1) coefficients[10] = 0;
            if (sample % 8 == 0) coefficients[9] = 0;
            if (sample % 7 == 0) coefficients[1 + 2 + sample % 62] = edges[(sample + 3) % edges.Length];
            var original = coefficients.ToArray(); var originalQuantizers = quantizers.ToArray();
            for (var k = 0; k < 64; k++) scan[k] = coefficients[1 + JpegTransform.Zigzag[k]];
            var originalScan = scan.ToArray();
            Reference(coefficients.AsSpan(1, 64), quantizers.AsSpan(1, 64), expected, kernel);
            Array.Fill(output, (byte)0xa5);
            JpegInverseTransform.Inverse(scanOrder ? scan : coefficients.AsSpan(1, 64),
                quantizers.AsSpan(1, 64), output.AsSpan(8, 64), context, scanOrder);
            Assert.Equal(expected, output.AsSpan(8, 64).ToArray());
            Assert.All(output.Take(8).Concat(output.Skip(72)), value => Assert.Equal(0xa5, value));
            Assert.Equal(original, coefficients); Assert.Equal(originalQuantizers, quantizers);
            Assert.Equal(originalScan, scan);
        }
        Assert.Equal(0, root.Attempts); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void LowRowsBorrowCapacityWithoutAllocationsAndCancelBeforeMutation()
    {
        var coefficients = new int[64]; var quantizers = Enumerable.Repeat((ushort)1, 64).ToArray();
        coefficients[0] = -1024; coefficients[1] = 4; coefficients[8] = 3; coefficients[9] = -7;
        var output = new byte[64];
        using var root = new TestMemoryBudget(0); using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(new() { MemoryBudget = root, MaxWorkingBytes = 1 }, cancellation.Token);
        for (var i = 0; i < 8; i++) JpegInverseTransform.Inverse(coefficients, quantizers, output, context);
        var allocated = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 128; i++) JpegInverseTransform.Inverse(coefficients, quantizers, output, context);
        Assert.Equal(allocated, GC.GetAllocatedBytesForCurrentThread());
        cancellation.Cancel(); Array.Fill(output, (byte)0xa5);
        var error = Assert.Throws<OperationCanceledException>(() => JpegInverseTransform.Inverse(coefficients, quantizers, output, context));
        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.All(output, value => Assert.Equal(0xa5, value));
        Assert.Equal(0, root.Attempts); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    private static int[] BuildKernel()
    {
        var weights = new int[64];
        for (var u = 0; u < 8; u++) for (var x = 0; x < 8; x++)
        {
            var cosine = u == 0 ? 1 : Math.Sqrt(2) * Math.Cos((2 * x + 1) * u * Math.PI / 16);
            weights[u * 8 + x] = u is 0 or 4 ? Math.Sign(cosine) * 16383
                : (int)Math.Round(cosine * 16384, MidpointRounding.AwayFromZero);
        }
        return weights;
    }

    private static void Reference(ReadOnlySpan<int> coefficients, ReadOnlySpan<ushort> quantizers,
        Span<byte> output, int[] kernel)
    {
        Span<short> input = stackalloc short[64]; Span<short> rows = stackalloc short[64];
        for (var i = 0; i < 64; i++) input[i] = unchecked((short)((long)coefficients[i] * quantizers[i]));
        input[0] = (short)Math.Clamp((long)coefficients[0] * quantizers[0] + 1024, short.MinValue, short.MaxValue);
        for (var y = 0; y < 8; y++)
        {
            var constant = true;
            for (var u = 1; u < 8; u++) constant &= input[y * 8 + u] == 0;
            for (var x = 0; x < 8; x++)
            {
                if (constant) rows[y * 8 + x] = unchecked((short)(input[y * 8] * 8));
                else
                {
                    var sum = 1024;
                    for (var u = 0; u < 8; u++) sum = unchecked(sum + input[y * 8 + u] * kernel[u * 8 + x]);
                    rows[y * 8 + x] = (short)Math.Clamp(sum >> 11, short.MinValue, short.MaxValue);
                }
            }
        }
        for (var y = 0; y < 8; y++) for (var x = 0; x < 8; x++)
        {
            var sum = unchecked((short)(rows[x] + 32)) * kernel[y];
            for (var v = 1; v < 8; v++) sum = unchecked(sum + rows[v * 8 + x] * kernel[v * 8 + y]);
            output[y * 8 + x] = (byte)Math.Clamp(sum >> 20, 0, 255);
        }
    }
}
