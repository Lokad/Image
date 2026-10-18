using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class Vp8CoefficientCacheTests
{
    [Fact]
    public void CachedSignsHighBitsAndSparseBlocksReplayTheOriginalArithmeticBytes()
    {
        var context = new OperationContext(ImageLimits.Default, default);
        using var model = new Vp8Encoder.CoefficientModel(384, 1, context);
        var planes = new Vp8Planes(16, 16, 16, Enumerable.Repeat((byte)206, 256).ToArray(), new byte[64], new byte[64]);
        short[] values = [510, -510, 256, -256, 255, -255, 1, -1, 0, 2, -2, 67, -67, 511, -511, 0];
        model.Record(planes.Y, 16, 0, 0, 0, 0, values, 3, 0);
        for (var y = 0; y < 16; y++)
        for (var x = 0; x < 16; x++) if (x >= 4 || y >= 4) Assert.Equal(206, planes.Y[y * 16 + x]);
        var zero = new short[16]; var unchanged = planes.Y.ToArray();
        model.Record(planes.Y, 16, 4, 0, 0, 1, zero, 3, 1);
        Assert.Equal(unchanged, planes.Y); // Zero blocks never overwrite private sample bytes.
        using var cached = new Vp8BooleanWriter(context, 4096);
        model.Emit(planes, cached, new byte[8], new byte[8], 1, 1); cached.Finish();
        using var original = new Vp8BooleanWriter(context, 4096);
        var above = new byte[8]; var left = new byte[8];
        for (var b = 0; b < 16; b++)
        {
            var a = b % 4; var l = b / 4;
            above[a] = left[l] = Vp8Encoder.Coefficients(original, b == 0 ? values : zero, 3, above[a] + left[l]);
        }
        for (var plane = 0; plane < 2; plane++)
        for (var b = 0; b < 4; b++)
        {
            var a = 4 + plane * 2 + b % 2; var l = 4 + plane * 2 + b / 2;
            above[a] = left[l] = Vp8Encoder.Coefficients(original, zero, 2, above[a] + left[l]);
        }
        original.Finish(); Assert.Equal(original.Bytes.ToArray(), cached.Bytes.ToArray());
    }

    [Fact]
    public void TransformExtremaFitNineMagnitudeBitsAtEveryResidualBoxVertex()
    {
        var residual = new short[16]; var coefficients = new short[16];
        // Each pre-rounding coefficient is linear: extrema over [-255,255]^16
        // occur at vertices. This also exercises the actual floating transform.
        for (var mask = 0; mask < 65536; mask++)
        {
            for (var i = 0; i < 16; i++) residual[i] = (short)((mask & (1 << i)) == 0 ? -255 : 255);
            Vp8ForwardDct.Quantize(residual, coefficients, 4, 4);
            foreach (var value in coefficients) Assert.InRange(value, (short)-510, (short)510);
        }
    }
}
