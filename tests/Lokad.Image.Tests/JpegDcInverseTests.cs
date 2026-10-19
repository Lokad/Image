using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegDcInverseTests
{
    [Fact]
    public void DcExtremesQuantizersAndEverySampleHalfKeepTheQualifiedGeneralPixels()
    {
        var coefficients = new int[64]; var quantization = Enumerable.Repeat((ushort)1, 64).ToArray();
        var actual = new byte[66]; var expected = new byte[64];
        foreach (var q in new ushort[] { 1, 2, 3, 7, 15, 127, 255, 256, 32768, 65535 })
        {
            quantization[0] = q;
            var values = Enumerable.Range(-1032, 2065).Where(dc => q == 1 || dc is >= -4 and <= 4).ToHashSet();
            for (var sample = 0; sample < 256; sample++)
            {
                var nearHalf = (sample * 8 - 1020) / q;
                for (var delta = -1; delta <= 1; delta++) values.Add(nearHalf + delta);
            }
            values.UnionWith([int.MinValue, int.MaxValue, -65535, 65535]);
            foreach (var dc in values)
            {
                coefficients[0] = dc; Array.Fill(actual, (byte)173);
                var oldContext = new OperationContext(ImageLimits.Default, default); var context = new OperationContext(ImageLimits.Default, default);
                JpegTransform.Inverse(coefficients, quantization, expected, oldContext);
                Assert.True(JpegTransform.TryInverseDc(coefficients, quantization, actual.AsSpan(1, 64), context));
                Assert.Equal(expected, actual.AsSpan(1, 64).ToArray()); Assert.Equal(oldContext.ConsumedWork, context.ConsumedWork); Assert.Equal(4096, context.ConsumedWork);
                Assert.Equal(173, actual[0]); Assert.Equal(173, actual[^1]);
            }
        }
        coefficients[0] = 740; quantization[0] = 1;
        Assert.True(JpegTransform.TryInverseDc(coefficients, quantization, expected, new(ImageLimits.Default, default)));
        Assert.All(expected, b => Assert.Equal(220, b)); // Nominal 740/8+128 would round to221.
    }

    [Fact]
    public void EveryAcPositionRejectsWithoutWritingOrCharging()
    {
        var coefficients = new int[64]; var quantization = Enumerable.Repeat((ushort)1, 64).ToArray(); var output = Enumerable.Repeat((byte)173, 64).ToArray();
        for (var position = 1; position < 64; position++)
        {
            coefficients[position] = position % 2 == 0 ? int.MinValue : int.MaxValue;
            var context = new OperationContext(ImageLimits.Default, default);
            Assert.False(JpegTransform.TryInverseDc(coefficients, quantization, output, context)); Assert.Equal(0, context.ConsumedWork);
            Assert.All(output, b => Assert.Equal(173, b)); coefficients[position] = 0;
        }
    }

    [Theory]
    [InlineData(63, 64, 64)] [InlineData(64, 63, 64)] [InlineData(64, 64, 63)]
    public void InvalidSpansFailBeforeChargesOrWrites(int coefficients, int quantization, int pixels)
    {
        var output = Enumerable.Repeat((byte)173, pixels).ToArray(); var context = new OperationContext(ImageLimits.Default, default);
        Assert.Throws<ArgumentException>(() => JpegTransform.TryInverseDc(new int[coefficients], new ushort[quantization], output, context));
        Assert.Equal(0, context.ConsumedWork); Assert.All(output, b => Assert.Equal(173, b));
    }

    [Fact]
    public void OriginalCancellationAndWorkLimitPrecedeEveryPixelWrite()
    {
        var coefficients = new int[64]; var quantization = Enumerable.Repeat((ushort)1, 64).ToArray(); var output = Enumerable.Repeat((byte)173, 64).ToArray();
        var limited = new OperationContext(ImageLimits.Default with { MaxWork = 4095 }, default);
        var error = Assert.Throws<ImageLimitException>(() => JpegTransform.TryInverseDc(coefficients, quantization, output, limited));
        Assert.Equal(ImageLimitKind.Work, error.Limit); Assert.Equal(0, limited.ConsumedWork); Assert.All(output, b => Assert.Equal(173, b));
        using var token = new CancellationTokenSource(); var canceled = new OperationContext(ImageLimits.Default, token.Token); token.Cancel();
        var cancellation = Assert.Throws<OperationCanceledException>(() => JpegTransform.TryInverseDc(coefficients, quantization, output, canceled));
        Assert.Equal(token.Token, cancellation.CancellationToken); Assert.Equal(0, canceled.ConsumedWork); Assert.All(output, b => Assert.Equal(173, b));
    }
}
