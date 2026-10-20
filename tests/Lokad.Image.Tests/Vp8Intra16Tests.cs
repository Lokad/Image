using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class Vp8Intra16Tests
{
    [Theory] [InlineData(false)] [InlineData(true)]
    public void FullFrameCacheFreezesPredictionProbabilitiesAtTheOriginalPrefix(bool mixed)
    {
        using var context = new OperationContext(ImageLimits.Default, default);
        const int columns = 13, rows = 5, blocks = columns * rows;
        var planes = new Vp8Planes(columns * 16, rows * 16, columns * 16,
            new byte[blocks * 256], new byte[blocks * 64], new byte[blocks * 64]);
        using var model = new Vp8Encoder.CoefficientModel(blocks * 384, blocks, context,
            mixed ? new byte[blocks * 16] : null, mixed ? columns : 0, fullFrame: true);
        Assert.Equal(64, model.PrefixMacroblocks); Assert.Equal(blocks, model.CacheMacroblocks);
        using var writer = new Vp8BooleanWriter(context, 65536);
        var zero = new short[16]; var dense = Enumerable.Repeat((short)1, 16).ToArray();
        byte[]? prefix = null;
        for (var mb = 0; mb < blocks; mb++)
        {
            for (var b = 0; b < 24; b++)
            {
                var plane = b < 16 ? 0 : 1 + (b - 16) / 4;
                var width = plane == 0 ? columns * 16 : columns * 8;
                var x = mb % columns * (plane == 0 ? 16 : 8);
                var y = mb / columns * (plane == 0 ? 16 : 8);
                var local = b < 16 ? b : (b - 16) % 4;
                var across = plane == 0 ? 4 : 2;
                model.Record(plane == 0 ? planes.Y : plane == 1 ? planes.U : planes.V,
                    width, x + local % across * 4, y + local / across * 4,
                    plane == 0 ? 0 : planes.Y.Length + (plane - 1) * planes.U.Length,
                    mb * 24 + b, mb == 64 ? dense : zero, plane == 0 ? 3 : 2, 0);
            }
            if (mb == 63)
            {
                model.CompleteMacroblock(planes, writer, columns, rows, mb + 1);
                prefix = model.Probabilities.ToArray(); Assert.False(model.Ready);
            }
        }
        // Additional statistics must not alter prediction costs before the last
        // block. Only final entropy selection may change the probability array.
        Assert.Equal(prefix, model.Probabilities); Assert.False(model.Ready);
        model.CompleteMacroblock(planes, writer, columns, rows, blocks);
        Assert.True(model.Ready); Assert.False(prefix!.AsSpan().SequenceEqual(model.Probabilities));
        writer.Finish(); Assert.NotEmpty(writer.Bytes.ToArray());
    }

    [Fact]
    public void ForwardWalshMatchesAnIndependentMatrixAtEveryFormatQuantizer()
    {
        // RFC 6386 section 14: the inverse Walsh basis has H*H=4I.
        // Inputs are sums of sixteen residual samples, twice the 4x4 DC.
        int[,] h = { { 1, 1, 1, 1 }, { 1, 1, -1, -1 }, { 1, -1, -1, 1 }, { 1, -1, 1, -1 } };
        var input = new int[16]; var actual = new short[18];
        for (var q = 0; q < 128; q++)
        for (var vector = 0; vector < 48; vector++)
        {
            for (var i = 0; i < 16; i++)
                input[i] = vector < 32
                    ? 4080 * (vector % 2 == 0 ? -1 : 1) * h[vector / 8, i / 4] * h[i % 4, vector / 2 % 4]
                    : i == vector - 32 ? 1 : 0;
            var dc = 2 * Vp8Tables.DcQuantizers[q]; var ac = Math.Max(8, Vp8Tables.AcQuantizers[q] * 155 / 100);
            Array.Fill(actual, (short)12345);
            Vp8ForwardDct.QuantizeWalsh(input, actual.AsSpan(1, 16), dc, ac);
            for (var v = 0; v < 4; v++)
            for (var u = 0; u < 4; u++)
            {
                var numerator = 0;
                for (var y = 0; y < 4; y++)
                for (var x = 0; x < 4; x++) numerator += h[v, y] * input[y * 4 + x] * h[x, u];
                var divisor = v + u == 0 ? dc : ac;
                var expected = (short)Math.Round(numerator / (4d * divisor), MidpointRounding.AwayFromZero);
                Assert.Equal(expected, actual[1 + v * 4 + u]);
                Assert.InRange(actual[1 + v * 4 + u], (short)-2040, (short)2040);
            }
            Assert.Equal(12345, actual[0]); Assert.Equal(12345, actual[17]);
        }
    }

    [Fact]
    public void SecondDcCachePreservesSignedHighBytesAndAcReplay()
    {
        using var context = new OperationContext(ImageLimits.Default, default);
        var modes = new byte[16]; modes[0] = 128;
        using var model = new Vp8Encoder.CoefficientModel(384, 1, context, modes, 1);
        Assert.True(model.Intra16);
        var planes = new Vp8Planes(16, 16, 16, new byte[256], new byte[64], new byte[64]);
        short[] dc = [2040, -2040, 1024, -1024, 511, -511, 256, -256, 1, -1, 0, 67, -67, 2, -2, 0];
        short[] ac = [0, -510, 510, -256, 256, -1, 1, 0, 67, -67, 0, 2, -2, 0, 0, 0];
        var zero = new short[16];
        model.RecordY2(planes, 0, 0, 0, dc, 0);
        var top = new byte[8]; var left = new byte[8];
        for (var b = 0; b < 16; b++)
        {
            var a = b % 4; var l = b / 4;
            top[a] = left[l] = model.Record(planes.Y, 16, a * 4, l * 4, 0, b, ac, 0, top[a] + left[l]);
        }
        for (var plane = 0; plane < 2; plane++)
        for (var b = 0; b < 4; b++)
            model.Record(plane == 0 ? planes.U : planes.V, 8, b % 2 * 4, b / 2 * 4,
                256 + plane * 64, 16 + plane * 4 + b, zero, 2, 0);
        using var cached = new Vp8BooleanWriter(context, 4096);
        model.Emit(planes, cached, new byte[8], new byte[8], 1, 1); cached.Finish();
        using var direct = new Vp8BooleanWriter(context, 4096);
        Vp8Encoder.Coefficients(direct, dc, 1, 0); Array.Clear(top); Array.Clear(left);
        for (var b = 0; b < 16; b++)
        {
            var a = b % 4; var l = b / 4;
            top[a] = left[l] = Vp8Encoder.Coefficients(direct, ac, 0, top[a] + left[l]);
        }
        for (var b = 0; b < 8; b++) Vp8Encoder.Coefficients(direct, zero, 2, 0);
        direct.Finish(); Assert.Equal(direct.Bytes.ToArray(), cached.Bytes.ToArray());
    }
}
