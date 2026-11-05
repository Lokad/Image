using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8PreparedWordTileTests
{
    [Fact]
    public void EveryWordNormalizesExactlyWithIndependentCanariesAndNoAllocation()
    {
        var source = new ushort[65538];
        for (var i = 0; i < 65536; i++) source[i + 1] = (ushort)i;
        source[0] = 9876; source[^1] = 1234;
        var output = Enumerable.Repeat((byte)197, 65542).ToArray();
        Vp8PreparedInput.Normalize16(source.AsSpan(1, 65536), output.AsSpan(3));
        for (var i = 0; i < 65536; i++) Assert.Equal((byte)((i + 128) / 257), output[i + 3]);
        // Force the32-sample path even on a512-bit machine; the full pass above
        // exercises64-sample batches. Scalar qualification covers both too.
        for (var i = 0; i < 65536; i += 32)
            Vp8PreparedInput.Normalize16(source.AsSpan(i + 1, 32), output.AsSpan(i + 3, 32));
        for (var i = 0; i < 65536; i++) Assert.Equal((byte)((i + 128) / 257), output[i + 3]);
        Vp8PreparedInput.Normalize16(source.AsSpan(65400, 17), output.AsSpan(3, 17));
        for (var i = 0; i < 17; i++) Assert.Equal((byte)((source[65400 + i] + 128) / 257), output[i + 3]);
        Assert.All(output[..3], v => Assert.Equal((byte)197, v));
        Assert.All(output[65539..], v => Assert.Equal((byte)197, v));
        Assert.Equal((ushort)9876, source[0]); Assert.Equal((ushort)1234, source[^1]);
        for (var i = 0; i < 65536; i++) Assert.Equal((ushort)i, source[i + 1]);
        var rejected = Enumerable.Repeat((byte)93, 63).ToArray();
        Assert.Throws<ArgumentException>(() => Vp8PreparedInput.Normalize16(source.AsSpan(1, 64), rejected));
        Assert.All(rejected, v => Assert.Equal((byte)93, v));
        for (var i = 0; i < 1000; i++) Vp8PreparedInput.Normalize16(source.AsSpan(1, 64), output);
        var allocated = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 10000; i++) Vp8PreparedInput.Normalize16(source.AsSpan(1, 64), output);
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - allocated);
    }

    [Theory]
    [InlineData(1, 1)] // Scalar-only singleton/edge replication.
    [InlineData(15, 3)] // Short row below one full tile.
    [InlineData(16, 2)] // Exactly one complete row-pair tile.
    [InlineData(17, 3)] // Both horizontal and vertical tails.
    [InlineData(33, 5)] // Multiple tiles, odd rows and a final column.
    public void WordTilesMatchIndependentScalarColorAndGammaOracle(int width, int height)
        => VerifyColorOracle(width, height, 16, false);

    [Theory]
    [InlineData(8)]
    [InlineData(16)]
    public void WeightedTilesMatchIndependentScalarOracleIncludingHiddenColorsAndAlphaThresholds(int depth)
        => VerifyColorOracle(33, 5, depth, true);

    private static void VerifyColorOracle(int width, int height, int depth, bool weighted)
    {
        using var root = new TestMemoryBudget(1_000_000);
        using var source = ImageBuffer.Create(width, height, depth, new() { MemoryBudget = root });
        uint state = 0xa372bf1d;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        for (var channel = 0; channel < 4; channel++)
        {
            state = unchecked(state * 1664525 + 1013904223); var sample = (ushort)(state >> 16);
            if (channel == 3)
            {
                var lane = (y % 2) * 2 + x % 2;
                sample = !weighted ? (ushort)(65407 + ((x * 4 + channel + y) % 129))
                    : ((x / 2 + y / 2) % 4) switch
                    {
                        0 => (ushort)(lane % 2 == 0 ? 0 : 128),
                        1 => (ushort)(lane % 2 == 0 ? 65407 : 65535),
                        2 => (ushort)(lane is 0 or 3 ? 65535 : 0),
                        _ => lane switch { 0 => (ushort)129, 1 => (ushort)65406, 2 => (ushort)(127 * 257), _ => (ushort)(128 * 257) }
                    };
            }
            if (depth == 8) source.GetRow8(y)[x * 4 + channel] = (byte)((sample + 128) / 257);
            else source.GetRow16(y)[x * 4 + channel] = sample;
        }
        var original = OriginalPixels.Bytes(source); var sourceBytes = root.Bytes;
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        context.ReserveBorrowed(sourceBytes);
        using var actual = Vp8PreparedInput.Create(source, context);
        Assert.Equal(weighted, actual.HasAlpha);
        if (weighted)
        {
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++) Assert.Equal((byte)Sample(x, y, 3), actual.Alpha[y * width + x]);
        }
        else Assert.Empty(actual.Alpha.ToArray());
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var r = Sample(x, y, 0); var g = Sample(x, y, 1); var b = Sample(x, y, 2);
            var expected = ((299 * (220 * 256) / 1000) * r + (587 * (220 * 256) / 1000) * g
                + (114 * (220 * 256) / 1000) * b + 16 * 65536 + 32768) / 65536;
            Assert.Equal((byte)expected, actual.Y[y * actual.Stride + x]);
        }
        for (var y = 0; y < (height + 1) / 2; y++)
        for (var x = 0; x < (width + 1) / 2; x++)
        {
            var r = LinearSum(x, y, 0); var g = LinearSum(x, y, 1); var b = LinearSum(x, y, 2);
            var ur = -(299 * (225 * 256) + 886) / 1772;
            var ug = -(587 * (225 * 256) + 886) / 1772;
            var vg = -(587 * (225 * 256) + 701) / 1402;
            var vb = -(114 * (225 * 256) + 701) / 1402;
            var u = Math.Clamp((ur * r + ug * g + (225 * 256 / 2) * b + 128 * 262144 + 131072) >> 18, 0, 255);
            var v = Math.Clamp(((225 * 256 / 2) * r + vg * g + vb * b + 128 * 262144 + 131072) >> 18, 0, 255);
            Assert.Equal((byte)u, actual.U[y * actual.Stride / 2 + x]);
            Assert.Equal((byte)v, actual.V[y * actual.Stride / 2 + x]);
        }
        Vp8PreparedInputTests.Padding(actual.Y, actual.Stride, width, height);
        Vp8PreparedInputTests.Padding(actual.U, actual.Stride / 2, (width + 1) / 2, (height + 1) / 2);
        Vp8PreparedInputTests.Padding(actual.V, actual.Stride / 2, (width + 1) / 2, (height + 1) / 2);
        Assert.Equal(original, OriginalPixels.Bytes(source)); Assert.Equal(sourceBytes + actual.CapacityBytes, root.Bytes);
        var result = Vp8PreparedInputTests.Hash(actual);
        source.Dispose(); context.ReleaseBorrowed(sourceBytes);
        Assert.Equal(actual.CapacityBytes, root.Bytes); Assert.Equal(result, Vp8PreparedInputTests.Hash(actual));
        actual.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);

        int Sample(int x, int y, int channel)
            => depth == 8 ? source.GetRow8(Math.Min(y, height - 1))[Math.Min(x, width - 1) * 4 + channel]
                : (source.GetRow16(Math.Min(y, height - 1))[Math.Min(x, width - 1) * 4 + channel] + 128) / 257;
        int LinearSum(int x, int y, int channel)
        {
            var sum = 0; var opacity = 0; long weightedSum = 0;
            for (var dy = 0; dy < 2; dy++)
            for (var dx = 0; dx < 2; dx++)
            {
                var value = (int)Math.Floor(Math.Pow(Sample(2 * x + dx, 2 * y + dy, channel) / 255d, .8) * 4095 + .5);
                var alpha = Sample(2 * x + dx, 2 * y + dy, 3);
                sum += value; opacity += alpha; weightedSum += (long)value * alpha;
            }
            if (opacity is not (0 or 1020)) sum = (int)(weightedSum * ((1 << 19) / opacity) / (1 << 17));
            var index = sum / 512; var fraction = sum % 512;
            int Knot(int i) => (int)Math.Floor(255 * Math.Pow(i * 128d / 4095, 1 / .8) + .5);
            return (Knot(index) * (512 - fraction) + Knot(index + 1) * fraction + 64) >> 7;
        }
    }
}
