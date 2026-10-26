using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;
using static Lokad.Image.Tests.Vp8lReferencePredictorTests;
using static Lokad.Image.Tests.Vp8lReferenceLzTests;

namespace Lokad.Image.Tests;

public sealed class Vp8lReferenceHuffmanTests
{
    private static string Hash(ReadOnlySpan<byte> bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));

    private static string TreeHash(Vp8lTokenBuffer tokens, int width, OperationContext context)
    {
        var layout = new Vp8lHistogramLayout(tokens.CacheBits); var counts = new int[layout.Total];
        layout.Fill(tokens.Tokens, width, true, counts, context);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        ReadOnlySpan<(int Start, int Count)> slices = [(0, layout.GreenSize), (layout.Red, 256), (layout.Blue, 256), (layout.Alpha, 256), (layout.Distance, 40)];
        foreach (var (start, count) in slices)
        {
            using var code = Vp8lHuffmanCode.Build(counts.AsSpan(start, count), context);
            // An unused alphabet serializes as the simple symbol-zero tree.
            // Compare the tree visible to the packet reader, including that
            // placeholder; the owned construction lengths remain all zero.
            if (code.Lengths.IndexOfAnyExcept((byte)0) < 0)
            { hash.AppendData(new byte[] { 1 }); hash.AppendData(code.Lengths[1..]); }
            else hash.AppendData(code.Lengths);
        }
        return Convert.ToHexStringLower(hash.GetHashAndReset());
    }

    private static void AssertBits(Vp8lTokenBuffer tokens, int width, int count, string expected, OperationContext context, TestMemoryBudget root)
    {
        var baseline = root.Bytes; var original = tokens.Tokens.ToArray();
        using var output = new MemoryStream(); var writer = new WebpBitWriter(output, context);
        Vp8lReferenceHuffman.WriteAuxiliary(writer, tokens.Tokens, width, context);
        Assert.Equal(count, output.Length * 8 + writer.PendingBits); writer.Finish();
        Assert.Equal(expected, Hash(output.ToArray())); Assert.Equal(baseline, root.Bytes);
        Assert.Equal(original, tokens.Tokens.ToArray());
    }

    // Native goldens are from the two transform images of the original
    // 513x259 packet, not imported encoder tables (see VP8L-COMPLETE-PACKET.md).
    [Theory]
    [InlineData(false, 2492, "de899d765b739f0e5406b44a664c8f98c72cf39eadb35c192979faa45027c712", "20baeada36f61f67839646df238e1cff5c5cad8f0b06bf6b5f9cdf976680bec4")]
    [InlineData(true, 3459, "7e5ad9061195679132376aac5c57ecca0e3b167c632c537959eecba03c97e34c", "1a23dcb5fdcfb8989e2d527022d0b55fad6af4d3c147698d6414fb7d300a82c3")]
    public void AuxiliaryTreesAndEveryEncodedBitMatchTheirFrozenNativePacket(bool color, int bits, string trees, string encoded)
    {
        using var root = new TestMemoryBudget(2_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var pixels = Map(color, context);
        using var matches = Vp8lReferenceMatcher.Build(pixels, 65, 75, false, context);
        using var parsed = Vp8lReferenceLz.Encode(pixels, matches.Values, 65, true, context);
        var baseline = root.Bytes; Assert.Equal(trees, TreeHash(parsed, 65, context)); Assert.Equal(baseline, root.Bytes);
        AssertBits(parsed, 65, bits, encoded, context, root);
    }

    [Theory]
    [InlineData(true, 340, "1d22037fef78e954247abf4f3d656b3b257aca031ec677871e2ef6b52c48b1fd", "92390a67158a0fcfe1ac55025c215d301675338bd85ea9899eeccbb28188032f")]
    [InlineData(false, 550, "f119399f9240ab76216a35f6a9662f1eed6204ca0f4c23de3b8b28d5c324685d", "ea6b661edfa122386abfa6f2eff1f9c2b46ab3d195ebde2e2f086570a369e41f")]
    public void GreedyAndNumericPaletteAuxiliariesMatchNativeCompleteBits(bool greedy, int bits, string trees, string encoded)
    {
        using var root = new TestMemoryBudget(100_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var pixels = Working(PalettePackingInput.Create(23, 13, 18), false, context);
        using var palette = Vp8lReferencePalette.SelectAndPack(pixels, 23, 18, (greedy ? Vp8lPaletteOrder.MinDelta : Vp8lPaletteOrder.Sorted), context);
        using var matches = Vp8lReferenceMatcher.Build(palette.EncodedDeltas, palette.EncodedColorCount, 20, !greedy, context);
        using var parsed = Vp8lReferenceLz.Encode(palette.EncodedDeltas, matches.Values, palette.EncodedColorCount, false, context);
        Assert.Equal(trees, TreeHash(parsed, palette.EncodedColorCount, context));
        AssertBits(parsed, palette.EncodedColorCount, bits, encoded, context, root);
    }

    [Fact]
    public void CachedHiddenRgbMainMatchesAllFiveNativeTrees()
    {
        using var root = new TestMemoryBudget(2_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var pixels = Working(HiddenRgb(), false, context);
        using var prediction = Vp8lReferencePredictor.SelectAndApply(pixels, 19, 5, 5, false, context);
        using var color = Vp8lReferenceColorTransform.SelectAndApply(pixels, 19, 5, 75, context);
        using var matches = Vp8lReferenceMatcher.Build(pixels, 19, 75, false, context);
        using var parsed = Vp8lReferenceMainParsing.Encode(pixels, matches.Values, 19, false, 10, context);
        Assert.Equal(3, parsed.CacheBits);
        Assert.Equal("3dae0a4fdbb5964977a9652fa78c44184e92030e8b45e4f5d84f050f7f5cfe8b", TreeHash(parsed, 19, context));
    }

    [Theory]
    [InlineData(15, 32)]
    [InlineData(7, 19)]
    public void SkewedPopulationsFitBothFormatDepthBoundsWithoutChangingInput(int maximum, int symbols)
    {
        var populations = new int[symbols]; populations[0] = populations[1] = 1;
        for (var i = 2; i < symbols; i++) populations[i] = populations[i - 1] + populations[i - 2];
        var original = populations.ToArray();
        using var root = new TestMemoryBudget(100_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var code = Vp8lHuffmanCode.Build(populations, context, maximum);
        var sum = 0;
        foreach (var length in code.Lengths) { Assert.InRange(length, 1, maximum); sum += 1 << (maximum - length); }
        Assert.Equal(1 << maximum, sum); Assert.Equal(original, populations); Assert.Equal(symbols * 3L, root.Bytes);
    }

    [Fact]
    public void EmptySingleAndEqualWeightTreesKeepTheirExplicitFormatSemantics()
    {
        using var context = new OperationContext(ImageLimits.Default, default);
        using var empty = Vp8lHuffmanCode.Build([], context); Assert.Empty(empty.Lengths.ToArray());
        using var zero = Vp8lHuffmanCode.Build([0, 0, 0], context); Assert.Equal(new byte[3], zero.Lengths.ToArray());
        using var single = Vp8lHuffmanCode.Build([0, 0, 7], context); Assert.Equal(new byte[] { 0, 0, 1 }, single.Lengths.ToArray());
        using var tied = Vp8lHuffmanCode.Build([1, 1, 1], context); Assert.Equal(new byte[] { 1, 2, 2 }, tied.Lengths.ToArray());
        using var output = new MemoryStream(); var writer = new WebpBitWriter(output, context);
        single.Write(writer, 2); Assert.Equal(0, writer.PendingBits); Assert.Equal(0, output.Length);
        Vp8lReferenceHuffman.WriteTree(writer, zero.Lengths, context); writer.Finish(); Assert.Equal(new byte[] { 1 }, output.ToArray());
        Assert.Throws<ArgumentOutOfRangeException>(() => Vp8lHuffmanCode.Build([-1], context));
        Assert.Throws<ArgumentOutOfRangeException>(() => Vp8lHuffmanCode.Build([1], context, 0));
        Assert.Throws<ArgumentException>(() => Vp8lHuffmanCode.Build([1, 1, 1], context, 1));
    }

    [Fact]
    public void RetainedAlphabetsAndOverlappingConstructionChargeCapacityUntilDisposal()
    {
        var populations = Enumerable.Repeat(1, 1304).ToArray();
        using var root = new TestMemoryBudget(200_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var first = Vp8lHuffmanCode.Build(populations, context); var hash = Hash(first.Lengths);
        using var second = Vp8lHuffmanCode.Build(populations, context);
        Assert.Equal(1304 * 6L, root.Bytes); Assert.Equal(1304 * 3L + 1304 * 54L + 512, root.Peak);
        first.Dispose(); first.Dispose(); Assert.Equal(1304 * 3L, root.Bytes);
        Assert.Throws<ObjectDisposedException>(() => first.Lengths.Length); Assert.Equal(hash, Hash(second.Lengths));
        second.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void LengthScratchAndCanonicalWordRefusalIsTerminalAndAllowsRecovery(int phase)
    {
        using var root = new TestMemoryBudget(100_000) { RefuseAtGrowth = phase };
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var populations = Enumerable.Repeat(1, 1304).ToArray();
        var error = Assert.Throws<ImageLimitException>(() => Vp8lHuffmanCode.Build(populations, context));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        Assert.Equal(phase, root.GrowthAttempts); Assert.Equal(1, root.Refusals); Assert.Equal(0, root.Bytes);
        root.RefuseAtGrowth = 0; using var next = Vp8lHuffmanCode.Build(populations, context);
        next.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void CancellationAfterScratchAdmissionRefundsBothOwnersAndAllowsRecovery()
    {
        using var root = new TestMemoryBudget(100_000); using var cancellation = new CancellationTokenSource();
        root.Admitted = _ => { if (root.GrowthAttempts == 2) cancellation.Cancel(); };
        using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
        Assert.Throws<OperationCanceledException>(() => Vp8lHuffmanCode.Build([1, 2, 5, 9], context));
        Assert.True(cancellation.IsCancellationRequested); Assert.Equal(0, root.Bytes); root.Admitted = null;
        using var nextContext = new OperationContext(new() { MemoryBudget = root }, default);
        using var next = Vp8lHuffmanCode.Build([1, 2, 5, 9], nextContext);
        next.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
