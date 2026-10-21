using System.Reflection;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8HistogramTests
{
    private static readonly Action<Vp8Encoder.CoefficientModel, int, int, bool> Token = typeof(Vp8Encoder.CoefficientModel)
        .GetMethod("RecordToken", BindingFlags.NonPublic | BindingFlags.Instance)!
        .CreateDelegate<Action<Vp8Encoder.CoefficientModel, int, int, bool>>();
    private static void Populate(Vp8Encoder.CoefficientModel model, bool omitted)
    {
        for (var group = 0; group < 96; group++)
        for (var token = 0; token < 12; token++)
        for (var count = 0; count <= (group * 17 + token * 13) % 31; count++)
            Token(model, token, group, omitted && token != 11);
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public void EveryTokenAndContextMatchesOriginalBranchStatisticsIncludingOmittedEob(bool omitted)
    {
        using var root = new TestMemoryBudget(100000);
        using (var first = new OperationContext(new ImageLimits { MemoryBudget = root }, default))
        using (var second = new OperationContext(new ImageLimits { MemoryBudget = root }, default))
        using (var original = new Vp8Encoder.CoefficientModel(384, 1, first, prefixMacroblocks: 1))
        using (var histogram = new Vp8Encoder.CoefficientModel(384, 1, second, prefixMacroblocks: 1, histogram: true))
        {
            Assert.Equal(768, root.Bytes - 2 * (120 + 16384));
            Populate(original, omitted); Populate(histogram, omitted);
            original.SelectProbabilities(); histogram.SelectProbabilities();
            Assert.Equal(original.Probabilities, histogram.Probabilities);
            Assert.Equal(16896, first.ConsumedWork); Assert.Equal(44544, second.ConsumedWork);
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public void FitWorkRefusalOrCancellationPrecedesProbabilityMutationAndAllowsLaterWork(bool cancel)
    {
        using var root = new TestMemoryBudget(100000); using var cancellation = new CancellationTokenSource();
        using (var context = new OperationContext(new ImageLimits { MaxWork = 44543, MemoryBudget = root }, cancellation.Token))
        using (var model = new Vp8Encoder.CoefficientModel(384, 1, context, prefixMacroblocks: 1, histogram: true))
        {
            Populate(model, true); var original = model.Probabilities.ToArray();
            if (cancel) cancellation.Cancel();
            if (cancel) Assert.Throws<OperationCanceledException>(() => model.SelectProbabilities());
            else
            {
                var error = Assert.Throws<ImageLimitException>(() => model.SelectProbabilities());
                Assert.Equal((ImageLimitKind.Work, ImageLimitScope.Operation), (error.Limit, error.Scope));
            }
            Assert.Equal(original, model.Probabilities);
        }
        Assert.Equal(0, root.Bytes);
        using (var context = new OperationContext(new ImageLimits { MemoryBudget = root }, default))
        using (var model = new Vp8Encoder.CoefficientModel(384, 1, context, prefixMacroblocks: 1, histogram: true))
        { Populate(model, true); model.SelectProbabilities(); Assert.Equal(44544, context.ConsumedWork); }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    private const int Blocks = 400, Capacity = 384 * 120 + 16384 + 768;
    private const long Prediction = Blocks * (16L * 6 * 192 + 4L * 512 + 24L * 512 + 64 + 32) + 2048;
    private const long NearWork = Blocks * 384L + 2 * 12416 + 257;
    private const long Eligibility = Prediction + 384 * 24L * 80 + 2 * 44544 + Blocks * 24L + NearWork;
    private const long Growth = 3 * (Blocks * 384L * 21 + 32) + 3L * 0x7FFFF + 384;
    private static Vp8Encoder.CoefficientModel? Model(OperationContext context)
    {
        var source = new Vp8Planes(320, 320, 320, new byte[Blocks * 256], new byte[Blocks * 64], new byte[Blocks * 64]);
        return Vp8Encoder.CoefficientModel.TryCreate(source, Blocks, 6, 4, context,
            new byte[Blocks * 16], fullFrame: true, segmented: true, nearZeroWork: NearWork);
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public void LocalWorkKeepsTheOriginalCacheUntilTheLargerStrategyFits(bool enabled)
    {
        using var root = new TestMemoryBudget(Capacity);
        using (var context = new OperationContext(new ImageLimits { MemoryBudget = root, MaxWork = Eligibility - (enabled ? 0 : 1) }, default))
        using (var model = Model(context))
        {
            Assert.NotNull(model); Assert.Equal(enabled, model.Histogram); Assert.True(model.NearZero);
            Assert.Equal(128, model.PrefixMacroblocks); Assert.Equal(enabled ? 384 : 128, model.CacheMacroblocks);
            Assert.Equal(enabled ? Capacity : 31744, root.Bytes); Assert.Equal(1, root.Attempts); Assert.Equal(0, context.ConsumedWork);
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public void LocalMemoryAccountsForTheWholeCacheAndScratchBeforeSharedAdmission(bool enabled)
    {
        using var root = new TestMemoryBudget(Capacity);
        using (var context = new OperationContext(new ImageLimits { MemoryBudget = root, MaxWork = Eligibility,
            MaxWorkingBytes = Capacity + Growth + 4096 - (enabled ? 0 : 1) }, default))
        using (var model = Model(context))
        {
            Assert.NotNull(model); Assert.Equal(enabled, model.Histogram);
            Assert.Equal(enabled ? Capacity : 31744, root.Bytes); Assert.Equal(1, root.Attempts);
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void SelectedLargerOwnerRefusalDoesNotRetryAndReleaseAllowsSuccess()
    {
        using var root = new TestMemoryBudget(Capacity); Assert.True(root.TryReserve(1, out var other));
        using (var context = new OperationContext(new ImageLimits { MemoryBudget = root, MaxWork = Eligibility }, default))
        {
            var error = Assert.Throws<ImageLimitException>(() => Model(context));
            Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        }
        Assert.Equal(1, root.Bytes); Assert.Equal(1, root.Refusals); other.Dispose();
        using (var context = new OperationContext(new ImageLimits { MemoryBudget = root, MaxWork = Eligibility }, default))
        using (var model = Model(context))
        {
            Assert.NotNull(model); Assert.True(model.Histogram); Assert.Equal(Capacity, root.Bytes);
            model.Dispose(); model.Dispose(); Assert.Equal(0, root.Bytes);
        }
        Assert.False(root.IsDisposed);
    }

    [Fact]
    public void CachedCoefficientPackingAndEmissionStayExactAcrossStatisticsRepresentations()
    {
        byte[] zigzag = [0, 1, 4, 8, 5, 2, 3, 6, 9, 12, 13, 10, 7, 11, 14, 15];
        short[] levels = [0, 1, -1, 2, -4, 5, -8, 17, -36, 70, -255, 511];
        using var root = new TestMemoryBudget(100000);
        byte[] Emit(bool histogram)
        {
            var source = new Vp8Planes(32, 16, 32, new byte[512], new byte[128], new byte[128]);
            using var context = new OperationContext(new ImageLimits { MemoryBudget = root }, default);
            using var model = new Vp8Encoder.CoefficientModel(768, 2, context,
                prefixMacroblocks: 1, cacheMacroblocks: 2, histogram: histogram);
            using var writer = new Vp8BooleanWriter(context, ImageLimits.Default.MaxOutputBytes);
            var above = new byte[16]; var left = new byte[8];
            for (var mx = 0; mx < 2; mx++)
            {
                for (var b = 0; b < 24; b++)
                {
                    var coefficients = new short[16]; var number = mx * 24 + b;
                    if (number % 5 != 0)
                        for (var c = 0; c <= number % 16; c++) coefficients[zigzag[c]] = levels[(number * 7 + c * 3) % levels.Length];
                    var plane = b < 16 ? 3 : 2; var side = b < 16 ? 4 : 2; var local = b < 16 ? b : (b - 16) % 4;
                    var channel = b < 16 ? 0 : b < 20 ? 1 : 2; var offset = b < 16 ? 0 : 4 + (channel - 1) * 2;
                    var a = mx * 8 + offset + local % side; var l = offset + local / side;
                    var samples = channel == 0 ? source.Y : channel == 1 ? source.U : source.V;
                    above[a] = left[l] = model.Record(samples, channel == 0 ? 32 : 16,
                        mx * (channel == 0 ? 16 : 8) + local % side * 4, local / side * 4,
                        channel == 0 ? 0 : 512 + (channel - 1) * 128, number, coefficients, plane, above[a] + left[l]);
                }
                if (mx == 0) Assert.Equal(24 * (histogram ? 80 : 256), context.ConsumedWork);
                model.CompleteMacroblock(source, writer, 2, 1, mx + 1);
            }
            writer.Finish(); return writer.Bytes.ToArray();
        }
        var original = Emit(false); Assert.Equal(0, root.Bytes);
        Assert.Equal(original, Emit(true)); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
