using System.Reflection;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8NearZeroTests
{
    private delegate void Prepare(ReadOnlySpan<byte> quantizers, ReadOnlySpan<byte> probabilities,
        ReadOnlySpan<int> costs, Span<double> thresholds, OperationContext context);
    private delegate void Quantize(ReadOnlySpan<short> residual, Span<short> coefficients,
        int dc, int ac, int predictor, ReadOnlySpan<double> thresholds, OperationContext context);
    private static readonly Prepare PrepareThresholds = typeof(Vp8Encoder)
        .GetMethod("PrepareZeroThresholds", BindingFlags.Static | BindingFlags.NonPublic)!.CreateDelegate<Prepare>();
    private static readonly Quantize QuantizeThresholds = typeof(Vp8Encoder)
        .GetMethod("QuantizeNearZero", BindingFlags.Static | BindingFlags.NonPublic)!.CreateDelegate<Quantize>();

    [Theory]
    [InlineData(1, .5, 1)] [InlineData(-1, .5, -1)]
    [InlineData(1, .75, 0)] [InlineData(-1, .75, 0)]
    public void ExactHalfSeedsRetainTheirSignUnlessTheZeroThresholdWins(int sign, double threshold, int expected)
    {
        var residual = Enumerable.Repeat((short)sign, 16).ToArray(); var unchanged = residual.ToArray();
        var coefficients = new short[16]; var thresholds = Enumerable.Repeat(threshold, 24).ToArray();
        using var context = new OperationContext(ImageLimits.Default, default);
        QuantizeThresholds(residual, coefficients, 16, 16, 0, thresholds, context);
        Assert.Equal(expected, coefficients[0]); Assert.All(coefficients[1..], value => Assert.Equal(0, value));
        Assert.Equal(1, context.ConsumedWork); Assert.Equal(unchanged, residual);
    }

    [Fact]
    public void ThresholdsAgreeWithIndependentZeroAndOneCodewordScoresAndCancelTheSharedPrefix()
    {
        static int BitCost(bool bit, int probability) => (int)Math.Round(
            -Math.Log2((bit ? 256 - probability : probability) / 256d) * 256, MidpointRounding.AwayFromZero);
        var costs = Enumerable.Range(0, 257).Select(p => (int)Math.Round(
            -Math.Log2(Math.Max(1, p) / 256d) * 256, MidpointRounding.AwayFromZero)).ToArray();
        byte[] quantizers = [0, 28, 90, 127]; var probabilities = Vp8Tables.CoefficientProbabilities.ToArray();
        var thresholds = new double[192]; var alternate = new double[192];
        using var root = new TestMemoryBudget(0);
        using (var context = new OperationContext(new ImageLimits { MemoryBudget = root }, default))
        {
            PrepareThresholds(quantizers, probabilities, costs, thresholds, context);
            for (var segment = 0; segment < 4; segment++)
            for (var channel = 0; channel < 2; channel++)
            for (var band = 0; band < 8; band++)
            for (var state = 0; state < 3; state++)
            {
                var q = quantizers[segment]; var ac = (int)Vp8Tables.AcQuantizers[q];
                var step = band == 0 ? (int)Vp8Tables.DcQuantizers[q] : ac;
                if (channel == 1 && band == 0) step = Math.Min(132, step);
                var p = ((channel == 0 ? 3 : 2) * 8 + band) * 33 + state * 11;
                // RFC6386 zero: [EOB=1,0]; one: [EOB=1,1,0,sign].
                var zeroRate = BitCost(false, probabilities[p + 1]);
                var oneRate = BitCost(true, probabilities[p + 1]) + BitCost(false, probabilities[p + 2]) + 256;
                var lambda = (int)Math.Round(ac * ac * Math.Log(2) * 256 / 24, MidpointRounding.AwayFromZero);
                foreach (var sample in new[] { .5, .625, .75, 1, 1.25, 1.499 })
                {
                    var zero = sample * sample * step * step * 16384 + zeroRate * (double)lambda;
                    var one = (sample - 1) * (sample - 1) * step * step * 16384 + oneRate * (double)lambda;
                    Assert.Equal(zero < one, sample < thresholds[segment * 48 + channel * 24 + band * 3 + state]);
                }
            }
            for (var p = 0; p < probabilities.Length; p += 11) probabilities[p] = (byte)(256 - probabilities[p]);
            PrepareThresholds(quantizers, probabilities, costs, alternate, context);
            Assert.Equal(thresholds, alternate); Assert.Equal(6400, context.ConsumedWork);
        }
        Assert.Equal(0, root.Attempts); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public void RefusalOrCancellationAtTheFirstDecisionPrecedesMutationAndAllowsSubsequentWork(bool cancel)
    {
        var residual = Enumerable.Repeat((short)1, 16).ToArray(); var original = residual.ToArray();
        var coefficients = Enumerable.Repeat((short)123, 16).ToArray(); var thresholds = Enumerable.Repeat(.75, 24).ToArray();
        using var root = new TestMemoryBudget(0); using var cancellation = new CancellationTokenSource();
        using (var context = new OperationContext(new ImageLimits { MaxWork = 1, MemoryBudget = root }, cancellation.Token))
        {
            if (cancel) cancellation.Cancel(); else context.Work(1);
            if (cancel) Assert.Throws<OperationCanceledException>(() => QuantizeThresholds(residual, coefficients, 16, 16, 0, thresholds, context));
            else
            {
                var error = Assert.Throws<ImageLimitException>(() => QuantizeThresholds(residual, coefficients, 16, 16, 0, thresholds, context));
                Assert.Equal((ImageLimitKind.Work, ImageLimitScope.Operation), (error.Limit, error.Scope));
            }
            Assert.All(coefficients, value => Assert.Equal(123, value));
        }
        using (var next = new OperationContext(new ImageLimits { MemoryBudget = root }, default))
            QuantizeThresholds(residual, coefficients, 16, 16, 0, thresholds, next);
        Assert.All(coefficients, value => Assert.Equal(0, value)); Assert.Equal(original, residual);
        Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Attempts); Assert.False(root.IsDisposed);
    }

    private const int Blocks = 129, Capacity = 128 * 120 + 16384;
    private const long Eligibility = Blocks * (16L * 6 * 192 + 4L * 512 + 24L * 512 + 64 + 32)
        + 2048 + 128 * 24L * 256 + 1056 * 16 + Blocks * 24L;
    private const long AdditionalWork = Blocks * 384L + 2 * 3200 + 257;
    private static Vp8Planes Source() => new(Blocks * 16, 16, Blocks * 16,
        new byte[Blocks * 256], new byte[Blocks * 64], new byte[Blocks * 64]);
    private static Vp8Encoder.CoefficientModel? Model(OperationContext context)
        => Vp8Encoder.CoefficientModel.TryCreate(Source(), Blocks, 6, 4, context,
            new byte[Blocks * 16], fullFrame: true, segmented: true, nearZeroWork: AdditionalWork);

    [Theory] [InlineData(false)] [InlineData(true)]
    public void LocalWorkSelectsTheDecisionWithoutChangingTheAdmittedPrefixOwner(bool enabled)
    {
        using var root = new TestMemoryBudget(Capacity);
        using (var context = new OperationContext(new ImageLimits { MemoryBudget = root,
            MaxWork = Eligibility + AdditionalWork - (enabled ? 0 : 1) }, default))
        using (var model = Model(context))
        {
            Assert.NotNull(model); Assert.Equal(enabled, model.NearZero); Assert.False(model.Intra16);
            Assert.Equal(128, model.CacheMacroblocks); Assert.Equal(Capacity, root.Bytes); Assert.Equal(1, root.Attempts);
            Assert.Equal(0, context.ConsumedWork);
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void SelectedPrefixSharedRefusalIsTerminalAndReleasePermitsSuccess()
    {
        using var root = new TestMemoryBudget(Capacity); Assert.True(root.TryReserve(1, out var other));
        using (var context = new OperationContext(new ImageLimits { MemoryBudget = root, MaxWork = Eligibility + AdditionalWork }, default))
        {
            var error = Assert.Throws<ImageLimitException>(() => Model(context));
            Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        }
        Assert.Equal(1, root.Refusals); Assert.Equal(1, root.Bytes); other.Dispose();
        using (var context = new OperationContext(new ImageLimits { MemoryBudget = root, MaxWork = Eligibility + AdditionalWork }, default))
        using (var model = Model(context)) { Assert.NotNull(model); Assert.True(model.NearZero); }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
