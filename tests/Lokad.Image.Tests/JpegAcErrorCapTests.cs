using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class JpegAcErrorCapTests
{
    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(4)]
    public void ExactSourceQuantizationCannotSpendErrorEvenWhenRateWouldImprove(int area)
    {
        var source = new double[64]; source[0] = 40; source[JpegTransform.Zigzag[63]] = 5;
        var quantizers = Enumerable.Repeat((ushort)5, 64).ToArray(); var values = new int[64];
        JpegEncoder.Quantize(source, quantizers, values); var before = values.ToArray();
        using var context = new OperationContext(ImageLimits.Default, default);
        JpegEncoder.DecideAc(source, quantizers, values, 100, area, context, limitBlockError: true);
        Assert.Equal(before, values); Assert.Equal(192, context.ConsumedWork);
    }

    [Fact]
    public void CumulativeErrorCapRejectsASecondIndividuallyAffordableChangeAndKeepsDc()
    {
        var source = new double[64]; source[0] = 40;
        source[JpegTransform.Zigzag[61]] = source[JpegTransform.Zigzag[63]] = 2.52;
        var quantizers = Enumerable.Repeat((ushort)5, 64).ToArray(); var values = new int[64];
        JpegEncoder.Quantize(source, quantizers, values); var before = values.ToArray();
        var original = source.ToArray(); var divisors = quantizers.ToArray();
        using var context = new OperationContext(ImageLimits.Default, default);
        JpegEncoder.DecideAc(source, quantizers, values, 100, 1, context, limitBlockError: true);
        // Each removal costs0.2, but the whole block may spend only0.3844.
        Assert.Equal(0, values[JpegTransform.Zigzag[63]]);
        Assert.Equal(1, values[JpegTransform.Zigzag[61]]); Assert.Equal(8, values[0]);
        Assert.Equal(original, source); Assert.Equal(divisors, quantizers); Assert.Equal(256, context.ConsumedWork);
        var initialError = SquaredError(source, quantizers, before);
        Assert.InRange(SquaredError(source, quantizers, values) - initialError, 0, initialError / 32);
    }

    [Fact]
    public void ZeroErrorChangesArePermittedWhenTheyStrictlySaveBits()
    {
        var source = new double[64]; source[JpegTransform.Zigzag[63]] = 2.5;
        var quantizers = Enumerable.Repeat((ushort)5, 64).ToArray(); var values = new int[64];
        JpegEncoder.Quantize(source, quantizers, values); Assert.Equal(1, values[JpegTransform.Zigzag[63]]);
        using var context = new OperationContext(ImageLimits.Default, default);
        JpegEncoder.DecideAc(source, quantizers, values, 100, 1, context, limitBlockError: true);
        Assert.Equal(0, values[JpegTransform.Zigzag[63]]); Assert.Equal(6.25, SquaredError(source, quantizers, values));
    }

    [Fact]
    public void ZeroLambdaDoesNotComputeOrChargeTheErrorCap()
    {
        var source = Enumerable.Range(0, 64).Select(i => i * 7d).ToArray();
        var quantizers = Enumerable.Repeat((ushort)3, 64).ToArray(); var values = new int[64];
        JpegEncoder.Quantize(source, quantizers, values); var before = values.ToArray();
        using var context = new OperationContext(ImageLimits.Default with { MaxWork = 1 }, default);
        JpegEncoder.DecideAc(source, quantizers, values, 0, 1, context, limitBlockError: true);
        Assert.Equal(before, values); Assert.Equal(0, context.ConsumedWork);
    }

    [Theory]
    [InlineData(63)] [InlineData(127)] [InlineData(191)]
    public void WorkRefusalPrecedesCapPricingOrCandidateMutation(int allowance)
    {
        var source = new double[64]; source[JpegTransform.Zigzag[63]] = 2.5;
        var quantizers = Enumerable.Repeat((ushort)5, 64).ToArray(); var values = new int[64];
        JpegEncoder.Quantize(source, quantizers, values); var before = values.ToArray();
        using var context = new OperationContext(ImageLimits.Default with { MaxWork = allowance }, default);
        var failure = Assert.Throws<ImageLimitException>(() =>
            JpegEncoder.DecideAc(source, quantizers, values, 100, 1, context, limitBlockError: true));
        Assert.Equal(ImageLimitKind.Work, failure.Limit); Assert.Equal(before, values);
        Assert.Equal(allowance / 64 * 64, context.ConsumedWork);
    }

    [Fact]
    public void FullBlockFitsTheDeclared4160WorkBound()
    {
        var source = Enumerable.Repeat(5d, 64).ToArray();
        var quantizers = Enumerable.Repeat((ushort)5, 64).ToArray(); var values = Enumerable.Repeat(1, 64).ToArray();
        using var context = new OperationContext(ImageLimits.Default with { MaxWork = 4160 }, default);
        JpegEncoder.DecideAc(source, quantizers, values, 100, 1, context, limitBlockError: true);
        Assert.All(values, v => Assert.Equal(1, v)); Assert.Equal(4160, context.ConsumedWork);
    }

    [Fact]
    public void CancellationPrecedesTheCapAndLeavesCoefficientsUntouched()
    {
        var source = new double[64]; var quantizers = Enumerable.Repeat((ushort)1, 64).ToArray();
        var values = new int[64]; values[63] = 1;
        using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(ImageLimits.Default, cancellation.Token); cancellation.Cancel();
        var error = Assert.Throws<OperationCanceledException>(() =>
            JpegEncoder.DecideAc(source, quantizers, values, 100, 1, context, limitBlockError: true));
        Assert.Equal(cancellation.Token, error.CancellationToken); Assert.Equal(1, values[63]); Assert.Equal(0, context.ConsumedWork);
    }

    [Theory]
    [InlineData(JpegSampling.YCbCr444, false)] [InlineData(JpegSampling.YCbCr444, true)]
    [InlineData(JpegSampling.YCbCr420, false)] [InlineData(JpegSampling.YCbCr420, true)]
    public void AsyncCallerPlanningIncludesDecisionWorkBeforeAdmittingContiguousStorage(JpegSampling sampling, bool exact)
    {
        const int side = 129;
        var h = sampling == JpegSampling.YCbCr420 ? 2 : 1; var v = h;
        var mcus = ((side + h * 8 - 1) / (h * 8)) * ((side + v * 8 - 1) / (v * 8));
        var blocks = mcus * (h * v + 2);
        var bound = 2L * mcus + mcus * (h * v * (4096 + 768) + 2 * (4096 + 768 * h * v))
            + blocks * (192L + (sampling == JpegSampling.YCbCr444 ? 48512 : 4096))
            + (sampling == JpegSampling.YCbCr444 ? Math.Min(64, mcus) * 136129L + 3632 : 0) + 262145;
        using var root = new TestMemoryBudget(2 * 1024 * 1024);
        using (var context = new OperationContext(new() { MemoryBudget = root, MaxWork = bound - (exact ? 0 : 1) }, default))
        {
            var samples = new byte[side * side * 3];
            context.ReserveBorrowed(samples.Length); context.Reserve(8192);
            var input = new JpegSampleInput(samples, side, side, JpegSampleFormat.Rgb, 0, context);
            using (var output = JpegEncoder.AsyncSampleOutput(input, new() { Sampling = sampling, Quality = 69 }, context))
            {
                Assert.Equal(exact, output.Target is MemoryStream);
                Assert.Equal(exact ? 8192L + blocks * 432L + 2048 + EncodedOutput.Overhead : 8192, root.Bytes);
            }
            Assert.Equal(8192, root.Bytes);
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    private static double SquaredError(double[] source, ushort[] quantizers, int[] values)
        => source.Select((value, i) => Math.Pow(value - values[i] * (double)quantizers[i], 2)).Sum();
}
