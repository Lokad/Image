using System.Buffers.Binary;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;
using static Lokad.Image.Tests.Vp8lReferencePredictorTests;

namespace Lokad.Image.Tests;

public sealed class Vp8lReferenceLzTests
{
    internal static string SemanticHash(ReadOnlySpan<Vp8lToken> tokens, int width)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        Span<byte> record = stackalloc byte[16]; var position = 0;
        foreach (var token in tokens)
        {
            var kind = token.Code > 0 ? 1u : token.Code < 0 ? 2u : 0u;
            var value = kind == 1 ? (uint)Vp8lSymbols.Plane((int)token.Value, width) : kind == 2 ? (uint)~token.Code : token.Value;
            BinaryPrimitives.WriteUInt32LittleEndian(record, (uint)position); BinaryPrimitives.WriteUInt32LittleEndian(record[4..], kind);
            BinaryPrimitives.WriteUInt32LittleEndian(record[8..], value); BinaryPrimitives.WriteUInt32LittleEndian(record[12..], (uint)token.Length);
            hash.AppendData(record); position += token.Length;
        }
        return Convert.ToHexStringLower(hash.GetHashAndReset());
    }

    internal static uint[] Map(bool color, OperationContext context)
    {
        var rgba = Asymmetric(); var working = Working(rgba, true, context);
        try
        {
            using var prediction = Vp8lReferencePredictor.SelectAndApply(working, 513, 3, 3, false, context);
            if (color)
            {
                using var transform = Vp8lReferenceColorTransform.SelectAndApply(working, 513, 3, 75, context);
                context.Reserve(transform.Coefficients.Length * 4L);
                return transform.Coefficients.ToArray();
            }
            context.Reserve(prediction.Modes.Length * 4L);
            var pixels = new uint[prediction.Modes.Length];
            for (var i = 0; i < pixels.Length; i++) pixels[i] = 0xff000000u | (uint)prediction.Modes[i] << 8;
            return pixels;
        }
        finally { context.Release(rgba.Length); }
    }

    [Theory]
    [InlineData(false, 337, "debb87da51e373c24b3691df944d8b9fe3685535eb4b5c8a0f3628a44ce38a4f")]
    [InlineData(true, 713, "2cadb3d69050f48e7a9574ff049b2b6e3af2af21b9b9cdd96d90827c2c36bb85")]
    public void ExistingAuxiliaryMapsMatchEveryNativeSemanticToken(bool color, int count, string expected)
    {
        using var root = new TestMemoryBudget(2_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var pixels = Map(color, context); var original = pixels.ToArray();
        using var matches = Vp8lReferenceMatcher.Build(pixels, 65, 75, false, context);
        var baseline = root.Bytes;
        using var parsed = Vp8lReferenceLz.Encode(pixels, matches.Values, 65, true, context);
        Assert.Equal(count, parsed.Count); Assert.Equal(0, parsed.CacheBits); Assert.Equal(expected, SemanticHash(parsed.Tokens, 65));
        Assert.Equal(pixels.Length * 8, parsed.CapacityBytes); Assert.Equal(baseline + parsed.CapacityBytes, root.Bytes);
        Assert.Equal(original, pixels);
        matches.Dispose(); Assert.Equal(pixels.Length * 12, root.Bytes);
        Assert.Equal(expected, SemanticHash(parsed.Tokens, 65));
        parsed.Dispose(); parsed.Dispose(); Assert.Equal(pixels.Length * 4, root.Bytes);
        Assert.Throws<ObjectDisposedException>(() => parsed.Tokens.Length); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void GreedyRepeatAndTraceRetainTheFrozenNativePredictorDecisionCosts()
    {
        using var context = new OperationContext(ImageLimits.Default, default);
        var pixels = Map(false, context);
        using var matches = Vp8lReferenceMatcher.Build(pixels, 65, 75, false, context);
        using var standard = Vp8lReferenceLz.Greedy(pixels, matches.Values, context);
        using var repeated = Vp8lReferenceLz.Repeats(pixels, 65, context);
        using var trace = Vp8lReferenceLz.Trace(pixels, matches.Values, 65, standard.Tokens, context);
        Assert.Equal((295, 549, 337), (standard.Count, repeated.Count, trace.Count));
        Assert.Equal(30482148066L, Vp8lReferenceLz.Estimate(standard.Tokens, 65, context));
        Assert.Equal(31477728268L, Vp8lReferenceLz.Estimate(repeated.Tokens, 65, context));
        Assert.Equal(27436962299L, Vp8lReferenceLz.Estimate(trace.Tokens, 65, context));
    }

    [Theory]
    [InlineData(true, 14, "0aa1136ac4530d3552e48d810e7e5cf2793c88e0807c44845645f936c3eed23f")]
    [InlineData(false, 17, "725ae70c9e8af32caf329eee86664b92700ba027007cab3955c89867725e740f")]
    public void ExistingPaletteAuxiliariesKeepTheirQuality20AndLowEffortTokenChoices(bool greedy, int count, string expected)
    {
        var rgba = PalettePackingInput.Create(23, 13, 18);
        using var root = new TestMemoryBudget(100_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var pixels = Working(rgba, false, context);
        using var palette = Vp8lReferencePalette.SelectAndPack(pixels, 23, 18, greedy, context);
        using var matches = Vp8lReferenceMatcher.Build(palette.EncodedDeltas, palette.EncodedColorCount, 20, !greedy, context);
        var baseline = root.Bytes;
        using var parsed = Vp8lReferenceLz.Encode(palette.EncodedDeltas, matches.Values, palette.EncodedColorCount, false, context);
        Assert.Equal(count, parsed.Count); Assert.Equal(expected, SemanticHash(parsed.Tokens, palette.EncodedColorCount));
        Assert.Equal(baseline + palette.EncodedColorCount * 8, root.Bytes);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(6)]
    public void CandidateHistogramAndTraceAdmissionRefusalCleanUpWithoutFallback(int phase)
    {
        using var root = new TestMemoryBudget(2_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var pixels = Map(false, context); var original = pixels.ToArray();
        using var matches = Vp8lReferenceMatcher.Build(pixels, 65, 75, false, context);
        var baseline = root.Bytes; root.RefuseAtGrowth = root.GrowthAttempts + phase;
        var error = Assert.Throws<ImageLimitException>(() => Vp8lReferenceLz.Encode(pixels, matches.Values, 65, true, context));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        Assert.Equal(1, root.Refusals); Assert.Equal(root.RefuseAtGrowth, root.GrowthAttempts);
        Assert.Equal(baseline, root.Bytes); Assert.Equal(original, pixels);
        root.RefuseAtGrowth = 0;
        using var next = Vp8lReferenceLz.Encode(pixels, matches.Values, 65, true, context);
        Assert.Equal(337, next.Count); next.Dispose(); Assert.Equal(baseline, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void CancellationAfterTraceScratchAdmissionRefundsCandidatesAndAllowsSubsequentParsing()
    {
        using var root = new TestMemoryBudget(2_000_000); using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
        var pixels = Map(false, context);
        using var matches = Vp8lReferenceMatcher.Build(pixels, 65, 75, false, context);
        var baseline = root.Bytes; var cancelAt = root.GrowthAttempts + 6;
        root.Admitted = _ => { if (root.GrowthAttempts == cancelAt) cancellation.Cancel(); };
        Assert.Throws<OperationCanceledException>(() => Vp8lReferenceLz.Encode(pixels, matches.Values, 65, true, context));
        Assert.True(cancellation.IsCancellationRequested); Assert.Equal(baseline, root.Bytes); Assert.Equal(0, root.Refusals);
        root.Admitted = null;
        using var nextContext = new OperationContext(new() { MemoryBudget = root }, default);
        using var next = Vp8lReferenceLz.Encode(pixels, matches.Values, 65, true, nextContext);
        next.Dispose(); Assert.Equal(baseline, root.Bytes);
    }

    [Fact]
    public void LateWorkFailureReleasesTraceScratchAndUnreturnedTokens()
    {
        uint[] pixels; uint[] matches; long completeWork;
        using (var calibration = new OperationContext(ImageLimits.Default, default))
        {
            pixels = Map(false, calibration);
            using var index = Vp8lReferenceMatcher.Build(pixels, 65, 75, false, calibration); matches = index.Values.ToArray();
            var before = calibration.ConsumedWork;
            using var result = Vp8lReferenceLz.Encode(pixels, matches, 65, true, calibration);
            completeWork = calibration.ConsumedWork - before;
        }
        using var root = new TestMemoryBudget(1_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root, MaxWork = completeWork - 1 }, default);
        var error = Assert.Throws<ImageLimitException>(() => Vp8lReferenceLz.Encode(pixels, matches, 65, true, context));
        Assert.Equal((ImageLimitKind.Work, ImageLimitScope.Operation), (error.Limit, error.Scope));
        Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Refusals); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void TokenOwnerAcceptsZeroRejectsNegativeAndKeepsEncodedTokenKindsTyped()
    {
        using var root = new TestMemoryBudget(24);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        Assert.Throws<ArgumentOutOfRangeException>(() => new Vp8lTokenBuffer(context, -1));
        using var empty = new Vp8lTokenBuffer(context, 0); Assert.Empty(empty.Tokens.ToArray()); Assert.Equal(0, root.Bytes);
        using var tokens = new Vp8lTokenBuffer(context, 3);
        tokens.Add(Vp8lToken.Literal(0xff102030)); tokens.Add(Vp8lToken.Copy(4095, 65)); tokens.Add(Vp8lToken.Cached(0));
        Assert.Equal((1, 4095, 1), (tokens.Tokens[0].Length, tokens.Tokens[1].Length, tokens.Tokens[2].Length));
        tokens.Reverse(); Assert.Equal(-1, tokens.Tokens[0].Code); Assert.Equal(24, root.Bytes);
        tokens.Dispose(); tokens.Dispose(); Assert.Equal(0, root.Bytes);
    }
}
