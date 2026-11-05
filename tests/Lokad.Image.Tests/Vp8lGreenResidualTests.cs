using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class Vp8lGreenResidualTests
{
    [Fact]
    public void EveryScalarPredictorMatchesTheIndependentPackedDecoderFor65536NeighbourControls()
    {
        for (var pair = 0; pair < 65536; pair++)
        {
            var left = pair & 255; var top = pair >> 8;
            var corner = (pair * 197 + 31) & 255; var right = (pair * 13 + (pair >> 3)) & 255;
            for (var mode = 0; mode < 14; mode++)
            {
                var expected = WebpLosslessDecoder.Predict(mode, (uint)left << 8, (uint)top << 8, (uint)corner << 8, (uint)right << 8);
                Assert.Equal((int)(expected >> 8 & 255), Vp8lGreenResiduals.Predict(mode, left, top, corner, right));
            }
        }
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    [InlineData(4)] [InlineData(5)] [InlineData(6)] [InlineData(7)]
    [InlineData(8)] [InlineData(9)] [InlineData(10)] [InlineData(11)]
    [InlineData(12)] [InlineData(13)]
    public void EveryModeKeepsFullResidualsAtBordersTilesVectorTailsAndLiveTokenFallback(int mode)
    {
        using var live = new CancellationTokenSource();
        foreach (var width in new[] { 1, 2, 7, 8, 9, 31, 32, 33, 67 })
        {
            var current = Enumerable.Range(0, width).Select(x => (uint)((x * 193 + 67) & 255) << 8).ToArray();
            var upper = Enumerable.Range(0, width).Select(x => (uint)((x * 29 + 241) & 255) << 8).ToArray();
            var currentCopy = current.ToArray(); var upperCopy = upper.ToArray();
            const int bits = 5;
            var modes = Enumerable.Repeat((byte)mode, (width + 31) >> bits).ToArray();
            foreach (var firstRow in new[] { true, false })
            {
                var expected = new uint[width];
                for (var x = 0; x < width; x++)
                {
                    var prediction = firstRow ? x == 0 ? 0xff000000u : current[x - 1]
                        : x == 0 ? upper[x] : WebpLosslessDecoder.Predict(mode, current[x - 1], upper[x],
                            upper[x - 1], x == width - 1 ? current[0] : upper[x + 1]);
                    for (var shift = 0; shift < 32; shift += 8)
                        expected[x] |= (unchecked((current[x] >> shift) - (prediction >> shift)) & 255) << shift;
                }
                var vector = Enumerable.Repeat(0xdeadbeefu, width).ToArray(); var scalar = vector.ToArray();
                Vp8lGreenResiduals.Row(current, upper, modes, bits, firstRow, vector, default);
                Vp8lGreenResiduals.Row(current, upper, modes, bits, firstRow, scalar, live.Token);
                Assert.Equal(expected, vector); Assert.Equal(expected, scalar);
                Assert.Equal(currentCopy, current); Assert.Equal(upperCopy, upper);
            }
        }
    }

    [Fact]
    public void MixedTileModesAndTheFinalTopRightUseOriginalRowsWithoutOverwritingNeighbours()
    {
        const int width = 453, bits = 5;
        var current = Enumerable.Range(0, width).Select(x => (uint)((x * 71 + 3) & 255) << 8).ToArray();
        var upper = Enumerable.Range(0, width).Select(x => (uint)((x * 163 + 239) & 255) << 8).ToArray();
        var modes = Enumerable.Range(0, (width + 31) >> bits).Select(i => (byte)(i % 14)).ToArray();
        var expected = new uint[width];
        for (var x = 0; x < width; x++)
        {
            var prediction = x == 0 ? upper[0] : WebpLosslessDecoder.Predict(modes[x >> bits],
                current[x - 1], upper[x], upper[x - 1], x == width - 1 ? current[0] : upper[x + 1]);
            for (var shift = 0; shift < 32; shift += 8)
                expected[x] |= (unchecked((current[x] >> shift) - (prediction >> shift)) & 255) << shift;
        }
        var output = new uint[width];
        Vp8lGreenResiduals.Row(current, upper, modes, bits, false, output, default);
        Assert.Equal(expected, output);
    }

    [Fact]
    public void CancellationPrecedesAnyWriteAndPreservesTheToken()
    {
        using var cancel = new CancellationTokenSource(); cancel.Cancel();
        uint[] current = [0x1200, 0xff00], upper = [0x3400, 0x100];
        var output = new uint[] { 0xdeadbeef, 0xdeadbeef };
        var error = Assert.Throws<OperationCanceledException>(() =>
            Vp8lGreenResiduals.Row(current, upper, new byte[1], 5, false, output, cancel.Token));
        Assert.Equal(cancel.Token, error.CancellationToken);
        Assert.Equal(new uint[] { 0xdeadbeef, 0xdeadbeef }, output);
    }
}

