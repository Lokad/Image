using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class LanczosTests
{
    private static ImageBuffer Source(int width, int height, int depth, bool opaque = false, ImageLimits? limits = null)
    {
        var image = ImageBuffer.Create(width, height, depth, limits); uint state = 0x13929a7;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        for (var c = 0; c < 4; c++)
        {
            state = unchecked(state * 1664525 + 1013904223); var maximum = depth == 8 ? 255 : 65535;
            var value = c == 3 ? opaque ? maximum : (x + y) % 5 switch { 0 => 0, 1 => 1, 2 => 123, _ => maximum }
                : (int)(state >> 16) & maximum;
            image.SetSample(x, y, c, value);
        }
        return image;
    }
    private static byte[] Bytes(ImageBuffer source) => source.Pixels.ToArray();
    public static IEnumerable<object[]> Cases()
    {
        yield return [8, 5, 3, 11, 7, true];
        yield return [8, 9, 7, 5, 3, false];
        yield return [16, 5, 3, 11, 7, false];
        yield return [16, 9, 7, 5, 3, false];
        yield return [8, 9, 7, 13, 3, false];
        yield return [8, 1, 5, 3, 9, false];
        yield return [16, 5, 1, 9, 3, false];
        yield return [16, 9, 131, 5, 3, false]; // Greater than the bounded row cache.
    }
    [Theory]
    [MemberData(nameof(Cases))]
    public void ExactPixelsMatchIndependentDecimalAndBigIntegerOracle(int depth, int sw, int sh, int dw, int dh, bool opaque)
    {
        using var source = Source(sw, sh, depth, opaque); var before = Bytes(source);
        var expected = LanczosReference.Resize(before, sw, sh, depth, dw, dh);
        using var result = ImageTransforms.Resize(source, dw, dh, ResizeFilter.Lanczos3);
        Assert.Equal((dw, dh, depth), (result.Width, result.Height, result.BitDepth));
        Assert.Equal(expected, Bytes(result)); Assert.Equal(before, Bytes(source));
    }
    [Theory]
    [InlineData(1, 7)] [InlineData(7, 1)] [InlineData(3, 5)] [InlineData(5, 3)]
    [InlineData(9, 13)] [InlineData(13, 9)] [InlineData(131, 3)]
    public void AxisCoefficientsHaveExactNormalizationAndIndependentValues(int source, int target)
    {
        using var context = new OperationContext(ImageLimits.Default, default);
        using (var plan = LanczosPlan.Create(source, target, context))
        {
            var expected = LanczosReference.Axis(source, target);
            for (var i = 0; i < target; i++)
            {
                var window = plan.Windows[i]; var actual = new int[source];
                plan.Weights.AsSpan(window.Offset, window.Count).CopyTo(actual.AsSpan(window.First));
                Assert.Equal(expected[i], actual); Assert.Equal(LanczosPlan.One, actual.Sum(x => (long)x));
                Assert.InRange(actual.Sum(x => Math.Abs((long)x)), LanczosPlan.One, 2 * LanczosPlan.One);
            }
        }
        Assert.Equal(ImageLimits.Default.MaxWorkingBytes, context.RemainingWorkingBytes);
    }
    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void ConstantsHiddenRgbAndSinglePixelsRetainTheirDeclaredSemantics(int depth)
    {
        foreach (var alpha in new[] { 0, 1, depth == 8 ? 255 : 65535 })
        {
            using var source = ImageBuffer.Create(1, 1, depth);
            for (var c = 0; c < 3; c++) source.SetSample(0, 0, c, depth == 8 ? 123 + c : 12345 + c);
            source.SetSample(0, 0, 3, alpha);
            using var result = ImageTransforms.Resize(source, 5, 3, ResizeFilter.Lanczos3);
            for (var y = 0; y < result.Height; y++)
            for (var x = 0; x < result.Width; x++)
            for (var c = 0; c < 4; c++) Assert.Equal(alpha == 0 ? 0 : source.Sample(0, 0, c), result.Sample(x, y, c));
            using var identity = ImageTransforms.Resize(source, 1, 1, ResizeFilter.Lanczos3);
            Assert.Equal(Bytes(source), Bytes(identity));
        }
    }
    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void EdgeImpulsesAndNegativeLobesHaveExactClippingAndRounding(int depth)
    {
        using var source = ImageBuffer.Create(8, 1, depth); var max = depth == 8 ? 255 : 65535;
        for (var x = 0; x < source.Width; x++) source.SetSample(x, 0, 3, max);
        source.SetSample(0, 0, 0, max); source.SetSample(7, 0, 2, max);
        using var result = ImageTransforms.Resize(source, 13, 1, ResizeFilter.Lanczos3);
        Assert.Equal(LanczosReference.Resize(Bytes(source), 8, 1, depth, 13, 1), Bytes(result));
        Assert.Equal(0, LanczosResize.Round(-1, 2, max));
        Assert.Equal(1, LanczosResize.Round(1, 2, max));
        Assert.Equal(max, LanczosResize.Round((Int128)max * 4, 2, max));
    }
    [Fact]
    public void AspectFitOwnershipAndOrientationAreIndependentOfTheSourceLifetime()
    {
        var source = Source(33, 21, 16); source.Orientation = ImageOrientation.Rotate90;
        var expected = LanczosReference.Resize(Bytes(source), 33, 21, 16, 20, 13);
        using var result = ImageTransforms.ResizeToFit(source, 20, 17, false, ResizeFilter.Lanczos3);
        Assert.Equal((20, 13, 16), (result.Width, result.Height, result.BitDepth));
        Assert.Equal(source.Orientation, result.Orientation); source.Dispose(); Assert.NotEmpty(Bytes(result));
        Assert.Equal(expected, Bytes(result));
    }
    [Fact]
    public void RetainedResultsAndReplacementPeaksShareTheCallerRoot()
    {
        using var probeBudget = new TestMemoryBudget(1_000_000);
        var probeLimits = new ImageLimits { MemoryBudget = probeBudget };
        using (var source = Source(9, 7, 16, limits: probeLimits))
        using (var result = ImageTransforms.Resize(source, 5, 3, ResizeFilter.Lanczos3, probeLimits)) { }
        var peak = probeBudget.Peak;
        using var budget = new TestMemoryBudget(peak); var limits = new ImageLimits { MemoryBudget = budget };
        var original = Source(9, 7, 16, limits: limits);
        var retained = ImageTransforms.Resize(original, 5, 3, ResizeFilter.Lanczos3, limits);
        Assert.Equal(original.ByteLength + retained.ByteLength, budget.Bytes);
        var error = Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(original, 5, 3, ResizeFilter.Lanczos3, limits));
        Assert.Equal(ImageLimitScope.SharedBudget, error.Scope);
        Assert.Equal(original.ByteLength + retained.ByteLength, budget.Bytes);
        retained.Dispose();
        using var replacement = ImageTransforms.Resize(original, 5, 3, ResizeFilter.Lanczos3, limits);
        original.Dispose(); Assert.Equal(replacement.ByteLength, budget.Bytes); Assert.NotEmpty(Bytes(replacement));
        replacement.Dispose(); Assert.Equal(0, budget.Bytes); Assert.False(budget.IsDisposed);
    }
    [Fact]
    public void RationalSupportEndpointsRemainOrderedAtTheLargestIntegerGeometry()
    {
        var maximum = int.MaxValue;
        Assert.Equal(((long)maximum - 4, (long)maximum + 2), LanczosPlan.Support(maximum - 1, maximum, maximum));
        Assert.Equal((-3L, 3L), LanczosPlan.Support(0, maximum, maximum));
        var (first, last) = LanczosPlan.Support(0, maximum, 1);
        Assert.Equal(-(5L * maximum + 1) / 2, first); Assert.Equal((7L * maximum - 1) / 2, last);
    }
    [Fact]
    public void EveryGrowingAdmissionCanRefuseWithCompleteCleanupAndSubsequentSuccess()
    {
        using var budget = new TestMemoryBudget(1_000_000); var limits = new ImageLimits { MemoryBudget = budget };
        using var source = Source(9, 7, 16, limits: limits); var owned = budget.Bytes; var original = Bytes(source);
        var start = budget.GrowthAttempts;
        using (var result = ImageTransforms.Resize(source, 5, 3, ResizeFilter.Lanczos3, limits)) Assert.True(budget.Bytes > owned);
        var boundaries = budget.GrowthAttempts - start; Assert.Equal(owned, budget.Bytes);
        for (var i = 1; i <= boundaries; i++)
        {
            budget.RefuseAtGrowth = budget.GrowthAttempts + i;
            var error = Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 5, 3, ResizeFilter.Lanczos3, limits));
            Assert.Equal(ImageLimitScope.SharedBudget, error.Scope); Assert.Equal(owned, budget.Bytes);
            Assert.Equal(budget.RefuseAtGrowth, budget.GrowthAttempts); Assert.Equal(original, Bytes(source));
        }
        budget.RefuseAtGrowth = 0;
        using (var result = ImageTransforms.Resize(source, 5, 3, ResizeFilter.Lanczos3, limits)) Assert.True(budget.Bytes > owned);
        Assert.Equal(owned, budget.Bytes); Assert.False(budget.IsDisposed);
    }
    [Fact]
    public void CancellationAfterResultAdmissionRefundsTemporaryAndResultStorage()
    {
        using var source = Source(9, 7, 16); using var budget = new TestMemoryBudget(1_000_000);
        var limits = new ImageLimits { MemoryBudget = budget };
        using (var probe = ImageTransforms.Resize(source, 5, 3, ResizeFilter.Lanczos3, limits)) { }
        var lastGrowth = budget.GrowthAttempts; var entered = 0; using var cancellation = new CancellationTokenSource();
        budget.Admitted = _ => { if (++entered == lastGrowth) cancellation.Cancel(); };
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 5, 3, ResizeFilter.Lanczos3, limits, cancellation.Token));
        Assert.Equal(lastGrowth, entered); Assert.Equal(0, budget.Bytes); budget.Admitted = null;
        using (var next = ImageTransforms.Resize(source, 5, 3, ResizeFilter.Lanczos3, limits)) { }
        Assert.Equal(0, budget.Bytes);
    }
}
