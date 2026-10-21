using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegPrecision444Tests
{
    [Theory]
    [InlineData(-1)] [InlineData(1)]
    public void AValueImmediatelyBelowAHalfTieDoesNotRoundOntoTheTie(int sign)
    {
        var source = new double[192]; source[0] = sign * Math.BitDecrement(4d);
        var values = new int[192]; var q = Enumerable.Repeat((ushort)1, 64).ToArray();
        using var context = new OperationContext(ImageLimits.Default, default);
        JpegEncoder.RefinePrecision444(source, q, q, values, 100, context); Assert.Equal(0, values[0]);
    }

    [Fact]
    public void ProgressiveEobRunCrossesIts32767BlockBoundaryWithoutLosingPixels()
    {
        const int width = 64, height = 32768, mcus = 32768;
        var coefficients = new uint[mcus * 192]; var q = Enumerable.Repeat((ushort)1, 64).ToArray();
        using var context = new OperationContext(ImageLimits.Default, default);
        context.ReserveBorrowed(coefficients.LongLength * sizeof(uint)); context.Reserve(65536);
        using var output = new MemoryStream();
        JpegEncoder.WriteProgressive444(width, height, output, new() { Quality = 69, Sampling = JpegSampling.YCbCr444 }, q, q, coefficients, mcus * 3, context);
        output.Write(new byte[] { 255, 217 });
        using var image = ImageCodec.Decode(output.ToArray()); Assert.Equal((width, height), (image.Width, image.Height));
        for (var y = 0; y < height; y++)
        {
            var row = image.GetRow8(y);
            for (var x = 0; x < width; x++) { Assert.Equal(128, row[x * 4]); Assert.Equal(128, row[x * 4 + 1]); Assert.Equal(128, row[x * 4 + 2]); Assert.Equal(255, row[x * 4 + 3]); }
        }
    }

    [Theory]
    [InlineData(1, 10, 3)] [InlineData(2, 10, 2)] [InlineData(3, 10, 1)]
    [InlineData(5, 10, 1)] [InlineData(6, 10, 0)] [InlineData(255, 10, 0)]
    public void PointTransformKeepsTheLargestAdmittedPowerOfTwo(int q, int precision, int expected)
        => Assert.Equal(expected, JpegEncoder.PointTransform444(q, precision));

    [Theory]
    [InlineData(-1)] [InlineData(1)]
    public void NearestSourceMultiplesReplaceOnlyFinerFrequencies(int sign)
    {
        var source = Enumerable.Repeat(sign * 11.5, 192).ToArray(); var values = Enumerable.Repeat(sign * 7, 192).ToArray();
        var luma = Enumerable.Repeat((ushort)3, 64).ToArray(); var chroma = Enumerable.Repeat((ushort)6, 64).ToArray();
        luma[0] = 1; chroma[0] = 2;
        using var context = new OperationContext(ImageLimits.Default, default);
        JpegEncoder.RefinePrecision444(source, luma, chroma, values, 100, context);
        Assert.Equal(2880, context.ConsumedWork);
        Assert.Equal(sign * 8, values[0]); Assert.Equal(sign * 4, values[1]);
        Assert.Equal(sign * 4, values[64]); Assert.Equal(sign * 7, values[65]);
        Assert.Equal(sign * 4, values[128]); Assert.Equal(sign * 7, values[129]);
        Assert.All(luma, q => Assert.True(q is 1 or 3)); Assert.All(chroma, q => Assert.True(q is 2 or 6));
    }

    [Theory]
    [InlineData(-1, -1024, -1016)] [InlineData(1, 1016, 1016)]
    public void LegalMultiplesRespectDistinctDcAndAcAmplitudeBounds(int sign, int dc, int ac)
    {
        var source = Enumerable.Repeat(sign * 100000d, 192).ToArray(); var values = new int[192];
        var q = Enumerable.Repeat((ushort)1, 64).ToArray(); using var context = new OperationContext(ImageLimits.Default, default);
        JpegEncoder.RefinePrecision444(source, q, q, values, 100, context);
        for (var c = 0; c < 3; c++) { Assert.Equal(dc, values[c * 64]); Assert.Equal(ac, values[c * 64 + 1]); }
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void RefusalOrCancellationOccursBeforeAnyCoefficientMutation(bool cancel)
    {
        var source = new double[192]; var values = Enumerable.Repeat(17, 192).ToArray(); var q = Enumerable.Repeat((ushort)1, 64).ToArray();
        using var token = new CancellationTokenSource(); using var context = new OperationContext(new() { MaxWork = 2879 }, token.Token);
        if (cancel) token.Cancel();
        if (cancel) Assert.Throws<OperationCanceledException>(() => JpegEncoder.RefinePrecision444(source, q, q, values, 100, context));
        else Assert.Equal(ImageLimitKind.Work, Assert.Throws<ImageLimitException>(() => JpegEncoder.RefinePrecision444(source, q, q, values, 100, context)).Limit);
        Assert.All(values, x => Assert.Equal(17, x)); Assert.Equal(0, context.ConsumedWork);
        using var next = new OperationContext(ImageLimits.Default, default);
        JpegEncoder.RefinePrecision444(source, q, q, values, 0, next); Assert.All(values, x => Assert.Equal(17, x));
        JpegEncoder.RefinePrecision444(source, q, q, values, 100, next); Assert.All(values, x => Assert.Equal(0, x));
    }
}
