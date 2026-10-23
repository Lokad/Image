using Lokad.Image.Internal;
using Lokad.Image.Tooling;
using static Lokad.Image.Tests.Vp8lReferencePredictorTests;
using static Lokad.Image.Tests.Vp8lReferenceLzTests;

namespace Lokad.Image.Tests;

public sealed class Vp8lReferenceMainParsingTests
{
    private static uint[] Hidden(OperationContext context)
    {
        var pixels = Working(HiddenRgb(), false, context);
        using var prediction = Vp8lReferencePredictor.SelectAndApply(pixels, 19, 5, 5, false, context);
        using var color = Vp8lReferenceColorTransform.SelectAndApply(pixels, 19, 5, 75, context);
        return pixels;
    }

    [Fact]
    public void AsymmetricMainMatchesNativeCacheChoicesCostsAndAll110306Tokens()
    {
        var rgba = Asymmetric();
        using var root = new TestMemoryBudget(16_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var pixels = Working(rgba, true, context);
        using var prediction = Vp8lReferencePredictor.SelectAndApply(pixels, 513, 3, 3, false, context);
        using var color = Vp8lReferenceColorTransform.SelectAndApply(pixels, 513, 3, 75, context);
        using var matches = Vp8lReferenceMatcher.Build(pixels, 513, 75, false, context);
        using (var standard = Vp8lReferenceLz.Greedy(pixels, matches.Values, context))
        using (var repeated = Vp8lReferenceLz.Repeats(pixels, 513, context))
        {
            Assert.Equal((116487, 132843), (standard.Count, repeated.Count));
            Assert.Equal(10, Vp8lReferenceCache.BestBits(pixels, standard.Tokens, 10, context));
            Assert.Equal(10, Vp8lReferenceCache.BestBits(pixels, repeated.Tokens, 10, context));
            Vp8lReferenceCache.Apply(pixels, standard, 10, context); Vp8lReferenceCache.Apply(pixels, repeated, 10, context);
            Assert.Equal(13935641814781L, Vp8lReferenceLz.Estimate(standard.Tokens, 513, context, 10));
            Assert.Equal(15156842602059L, Vp8lReferenceLz.Estimate(repeated.Tokens, 513, context, 10));
            using var trace = Vp8lReferenceLz.Trace(pixels, matches.Values, 513, standard.Tokens, context, 10);
            Assert.Equal(13412584171091L, Vp8lReferenceLz.Estimate(trace.Tokens, 513, context, 10));
        }
        var baseline = root.Bytes;
        using var parsed = Vp8lReferenceMainParsing.Encode(pixels, matches.Values, 513, false, 10, context);
        Assert.Equal((10, 110306), (parsed.CacheBits, parsed.Count));
        Assert.Equal("060676025dd0b04254e06cc29988e5efbad7917b2a7e612407546e0851a1b574", SemanticHash(parsed.Tokens, 513));
        var literal = 0; var copy = 0; var cached = 0;
        foreach (var token in parsed.Tokens) { if (token.Code == 0) literal++; else if (token.Code > 0) copy++; else cached++; }
        Assert.Equal((52617, 6039, 51650), (literal, copy, cached));
        Assert.Equal(baseline + pixels.Length * 8L, root.Bytes);
        parsed.Dispose(); Assert.Equal(baseline, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void HiddenRgbKeepsTheNativeRepeatWinnerAndItsIndependentThreeBitCache()
    {
        using var root = new TestMemoryBudget(2_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var pixels = Hidden(context); var original = pixels.ToArray();
        using var matches = Vp8lReferenceMatcher.Build(pixels, 19, 75, false, context);
        using (var standard = Vp8lReferenceLz.Greedy(pixels, matches.Values, context))
        using (var repeated = Vp8lReferenceLz.Repeats(pixels, 19, context))
        {
            Assert.Equal(1, Vp8lReferenceCache.BestBits(pixels, standard.Tokens, 10, context));
            Assert.Equal(3, Vp8lReferenceCache.BestBits(pixels, repeated.Tokens, 10, context));
            Vp8lReferenceCache.Apply(pixels, standard, 1, context); Vp8lReferenceCache.Apply(pixels, repeated, 3, context);
            Assert.Equal(6048448706L, Vp8lReferenceLz.Estimate(standard.Tokens, 19, context, 1));
            Assert.Equal(6027254956L, Vp8lReferenceLz.Estimate(repeated.Tokens, 19, context, 3));
        }
        var baseline = root.Bytes;
        using var parsed = Vp8lReferenceMainParsing.Encode(pixels, matches.Values, 19, false, 10, context);
        Assert.Equal((3, 21), (parsed.CacheBits, parsed.Count));
        Assert.Equal("3c5a8db67dc0f3c54e0fa3c07e5be5960d33ec2c7b573137447c9a135a694165", SemanticHash(parsed.Tokens, 19));
        Assert.Equal(original, pixels); Assert.Equal(baseline + pixels.Length * 8, root.Bytes);
        parsed.Dispose(); Assert.Equal(baseline, root.Bytes);
    }

    [Theory]
    [InlineData(false, "3969596bfef449ef714ca9eb121dad19766fcda58cd711319f654b6f3d4305ee")]
    [InlineData(true, "84d67338630b1f42459c4ac0d242fe411cd9622838ea00348b7972b395346227")]
    public void OrdinaryBoxAndCacheDisabledPaletteCandidatesMatchTheExistingNativeStreams(bool spatial, string expected)
    {
        using var root = new TestMemoryBudget(2_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        context.Reserve(1026 * 4); var pixels = new uint[1026];
        Array.Fill(pixels, 0xff000000u); pixels[^1] = 0xff004000u;
        if (spatial)
        {
            using var prediction = Vp8lReferencePredictor.SelectAndApply(pixels, 513, 2, 4, false, context);
        }
        using var ordinary = Vp8lReferenceMatcher.Build(pixels, 513, 75, false, context);
        using var box = Vp8lReferenceMatcher.BuildBox(pixels, 513, ordinary.Values, context);
        var baseline = root.Bytes;
        using var first = Vp8lReferenceMainParsing.Encode(pixels, ordinary.Values, 513, false, 2, context);
        using var second = Vp8lReferenceMainParsing.EncodeBox(pixels, box.Values, 513, false, 2, context);
        using var third = Vp8lReferenceMainParsing.EncodeBox(pixels, box.Values, 513, true, 2, context);
        foreach (var parsed in new[] { first, second, third })
        {
            Assert.Equal((0, 3), (parsed.CacheBits, parsed.Count)); Assert.Equal(expected, SemanticHash(parsed.Tokens, 513));
        }
        Assert.Equal(baseline + 3 * pixels.Length * 8L, root.Bytes);
    }

    [Fact]
    public void ExistingEighteenColorMainKeepsTheNativeUncachedLiteralDecision()
    {
        using var root = new TestMemoryBudget(100_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var pixels = Working(PalettePackingInput.Create(23, 13, 18), false, context);
        using var palette = Vp8lReferencePalette.SelectAndPack(pixels, 23, 18, Vp8lPaletteOrder.MinDelta, context);
        var input = pixels.AsSpan(0, palette.PackedPixelCount);
        using var matches = Vp8lReferenceMatcher.Build(input, palette.CodedWidth, 75, false, context);
        using var parsed = Vp8lReferenceMainParsing.Encode(input, matches.Values, palette.CodedWidth, false, 5, context);
        Assert.Equal((0, 299), (parsed.CacheBits, parsed.Count));
        Assert.Equal("59001b045275b4976ef24a19b95103332eae1e5f764fb95636ac1c515d70ee07", SemanticHash(parsed.Tokens, 23));
    }

    [Fact]
    public void InPlaceConversionHonorsInitialZeroAndEveryCopiedPixel()
    {
        using var root = new TestMemoryBudget(10_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var original = Working(PalettePackingInput.Create(23, 13, 18), false, context);
        var choices = original.AsSpan(1, 17).ToArray().GroupBy(p => Vp8lReferenceCache.Key(p, 1)).First(g => g.Count() >= 3).Take(3).ToArray();
        context.Reserve(7 * 4); uint[] pixels = [0, choices[0], choices[1], choices[2], choices[0], choices[1], choices[1]];
        using var tokens = new Vp8lTokenBuffer(context, 7);
        tokens.Add(Vp8lToken.Literal(0)); tokens.Add(Vp8lToken.Literal(choices[0])); tokens.Add(Vp8lToken.Literal(choices[1]));
        tokens.Add(Vp8lToken.Literal(choices[2])); tokens.Add(Vp8lToken.Copy(2, 3)); tokens.Add(Vp8lToken.Literal(choices[1]));
        var baseline = root.Bytes;
        Vp8lReferenceCache.Apply(pixels, tokens, 1, context);
        Assert.Equal(-1, tokens.Tokens[0].Code);
        Assert.Equal(~Vp8lReferenceCache.Key(choices[1], 1), tokens.Tokens[^1].Code);
        Assert.Equal((6, 56L, 1), (tokens.Count, tokens.CapacityBytes, tokens.CacheBits)); Assert.Equal(baseline, root.Bytes);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(8)]
    public void CacheChoiceConversionAndHistogramRefusalsAreTerminalAndRefundCandidates(int phase)
    {
        using var root = new TestMemoryBudget(2_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var pixels = Hidden(context); var original = pixels.ToArray();
        using var matches = Vp8lReferenceMatcher.Build(pixels, 19, 75, false, context);
        var baseline = root.Bytes; root.RefuseAtGrowth = root.GrowthAttempts + phase;
        var error = Assert.Throws<ImageLimitException>(() => Vp8lReferenceMainParsing.Encode(pixels, matches.Values, 19, false, 10, context));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        Assert.Equal(1, root.Refusals); Assert.Equal(root.RefuseAtGrowth, root.GrowthAttempts);
        Assert.Equal(baseline, root.Bytes); Assert.Equal(original, pixels); root.RefuseAtGrowth = 0;
        using var next = Vp8lReferenceMainParsing.Encode(pixels, matches.Values, 19, false, 10, context);
        next.Dispose(); Assert.Equal(baseline, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void CancellationAfterConversionAdmissionCleansUpAndAllowsRecovery()
    {
        using var root = new TestMemoryBudget(2_000_000); using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
        var pixels = Hidden(context);
        using var matches = Vp8lReferenceMatcher.Build(pixels, 19, 75, false, context);
        var baseline = root.Bytes; var cancelAt = root.GrowthAttempts + 6;
        root.Admitted = _ => { if (root.GrowthAttempts == cancelAt) cancellation.Cancel(); };
        Assert.Throws<OperationCanceledException>(() => Vp8lReferenceMainParsing.Encode(pixels, matches.Values, 19, false, 10, context));
        Assert.True(cancellation.IsCancellationRequested); Assert.Equal(baseline, root.Bytes); root.Admitted = null;
        using var nextContext = new OperationContext(new() { MemoryBudget = root }, default);
        using var next = Vp8lReferenceMainParsing.Encode(pixels, matches.Values, 19, false, 10, nextContext);
        next.Dispose(); Assert.Equal(baseline, root.Bytes); Assert.Equal(0, root.Refusals);
    }

    [Fact]
    public void LateWorkFailureReleasesCachedCandidatesAndScratch()
    {
        uint[] pixels; uint[] index; long work;
        using (var calibration = new OperationContext(ImageLimits.Default, default))
        {
            pixels = Hidden(calibration);
            using var matches = Vp8lReferenceMatcher.Build(pixels, 19, 75, false, calibration); index = matches.Values.ToArray();
            var start = calibration.ConsumedWork;
            using var parsed = Vp8lReferenceMainParsing.Encode(pixels, index, 19, false, 10, calibration); work = calibration.ConsumedWork - start;
        }
        using var root = new TestMemoryBudget(100_000);
        using var context = new OperationContext(new() { MemoryBudget = root, MaxWork = work - 1 }, default);
        var error = Assert.Throws<ImageLimitException>(() => Vp8lReferenceMainParsing.Encode(pixels, index, 19, false, 10, context));
        Assert.Equal((ImageLimitKind.Work, ImageLimitScope.Operation), (error.Limit, error.Scope));
        Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Refusals);
    }
}
