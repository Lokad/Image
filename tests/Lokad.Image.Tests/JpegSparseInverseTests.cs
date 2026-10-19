using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegSparseInverseTests
{
    [Fact]
    public void EverySingleFrequencyAndSignedMixturesMatchTheQualifiedGeneralTransform()
    {
        var coefficients = new int[64]; var quantization = Enumerable.Repeat((ushort)1, 64).ToArray(); var expected = new byte[64]; var output = new byte[66];
        void Check()
        {
            var context = new OperationContext(ImageLimits.Default, default); Array.Fill(output, (byte)173);
            JpegTransform.Inverse(coefficients, quantization, expected, new(ImageLimits.Default, default));
            Assert.True(JpegTransform.TryInverseSparse(coefficients, quantization, output.AsSpan(1, 64), context));
            Assert.Equal(expected, output.AsSpan(1, 64).ToArray()); Assert.Equal(4096, context.ConsumedWork);
            Assert.Equal(173, output[0]); Assert.Equal(173, output[^1]);
        }
        foreach (var q in new ushort[] { 1, 255 })
        foreach (var value in new[] { -1024, -4, -1, 1, 4, 1024 })
        for (var frequency = 0; frequency < 64; frequency++)
        { coefficients[frequency] = value; quantization[frequency] = q; Check(); coefficients[frequency] = 0; quantization[frequency] = 1; }
        for (var seed = 0; seed < 128; seed++)
        {
            Array.Clear(coefficients);
            for (var i = 0; i < 8; i++) { var at = (seed + i * 7) % 64; coefficients[at] = (seed * 73 + i * 37) % 129 - 64; }
            Check();
        }
    }

    [Fact]
    public void DenseBlocksRejectWithoutWritesOrCharges()
    {
        var coefficients = new int[64]; var quantization = new ushort[64]; var output = Enumerable.Repeat((byte)173, 64).ToArray();
        for (var i = 0; i < 9; i++) coefficients[i * 7] = i + 1;
        var context = new OperationContext(ImageLimits.Default, default);
        Assert.False(JpegTransform.TryInverseSparse(coefficients, quantization, output, context)); Assert.Equal(0, context.ConsumedWork);
        Assert.All(output, b => Assert.Equal(173, b));
    }

    [Fact]
    public void MalformedCanceledAndLimitedCallsLeavePixelsUntouched()
    {
        var coefficients = new int[64]; coefficients[1] = 4; var quantization = Enumerable.Repeat((ushort)1, 64).ToArray(); var output = Enumerable.Repeat((byte)173, 64).ToArray();
        Assert.Throws<ArgumentException>(() => JpegTransform.TryInverseSparse(new int[63], quantization, output, new(ImageLimits.Default, default)));
        var limited = new OperationContext(ImageLimits.Default with { MaxWork = 4095 }, default);
        Assert.Equal(ImageLimitKind.Work, Assert.Throws<ImageLimitException>(() => JpegTransform.TryInverseSparse(coefficients, quantization, output, limited)).Limit);
        using var token = new CancellationTokenSource(); var canceled = new OperationContext(ImageLimits.Default, token.Token); token.Cancel();
        Assert.Equal(token.Token, Assert.Throws<OperationCanceledException>(() => JpegTransform.TryInverseSparse(coefficients, quantization, output, canceled)).CancellationToken);
        Assert.Equal(0, limited.ConsumedWork); Assert.Equal(0, canceled.ConsumedWork); Assert.All(output, b => Assert.Equal(173, b));
    }
}
