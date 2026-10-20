using System.Buffers.Binary;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class WebpPrefixCodeTests
{
    [Fact]
    public void WeightedCanonicalCodesHaveIndependentKnownCostAndNoPrefixOverlap()
    {
        int[] frequencies = [5, 9, 12, 13, 16, 45];
        using var context = new OperationContext(ImageLimits.Default, default);
        var tree = new WebpPrefixCode(frequencies, context);
        Assert.Equal(224, frequencies.Select((n, i) => n * tree.Length(i)).Sum());
        AssertComplete(tree);
        for (var a = 0; a < frequencies.Length; a++)
        for (var b = 0; b < frequencies.Length; b++)
            if (a != b && tree.Length(a) <= tree.Length(b))
                Assert.NotEqual(tree.Code(a), tree.Code(b) & ((1 << tree.Length(a)) - 1));
    }

    [Theory] [InlineData(19, 7)] [InlineData(40, 15)] [InlineData(256, 15)]
    [InlineData(280, 15)] [InlineData(1304, 15)] [InlineData(2328, 15)]
    public void SparseSkewedAlphabetsAreCompleteBoundedAndDeterministic(int count, int maximum)
    {
        var frequencies = new int[count]; frequencies[0] = 1;
        var a = 1; var b = 1;
        for (var i = 1; i < Math.Min(count, 32); i++)
        { frequencies[i] = b; (a, b) = (b, a + b); }
        using var first = new OperationContext(ImageLimits.Default, default);
        using var second = new OperationContext(ImageLimits.Default, default);
        var x = new WebpPrefixCode(frequencies, first, maximum);
        var y = new WebpPrefixCode(frequencies, second, maximum);
        AssertComplete(x);
        for (var i = 0; i < count; i++)
        {
            Assert.InRange(x.Length(i), frequencies[i] == 0 ? 0 : 1, frequencies[i] == 0 ? 0 : maximum);
            Assert.Equal(x.Length(i), y.Length(i)); Assert.Equal(x.Code(i), y.Code(i));
        }
    }

    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(255)] [InlineData(1303)] [InlineData(2327)]
    public void SingleLeafAndEmptyUnusedAlphabetConsumeZeroDataBits(int symbol)
    {
        var frequencies = new int[Math.Max(256, symbol + 1)]; frequencies[symbol] = 3;
        using var context = new OperationContext(ImageLimits.Default, default);
        var tree = new WebpPrefixCode(frequencies, context);
        using var output = new MemoryStream(); var writer = new WebpBitWriter(output, context);
        tree.Write(writer, symbol); writer.Finish(); Assert.Equal(0, output.Length);
        var unused = new WebpPrefixCode(new int[40], context);
        unused.Write(writer, 0); writer.Finish(); Assert.Equal(0, output.Length);
        Assert.Equal(1, tree.Length(symbol)); Assert.Equal(0, tree.Code(symbol));
    }

    [Fact]
    public void SerializedCodeLengthRepeatsAndSkewDecodeThroughTheIndependentPixelDecoder()
    {
        var frequencies = new int[280]; frequencies[0] = 1; var a = 1; var b = 1;
        for (var i = 1; i < 32; i++) { frequencies[i] = b; (a, b) = (b, a + b); }
        var expected = Enumerable.Range(0, 32).Select(i => (byte)i).ToArray();
        var payload = Payload(frequencies, expected.Select(v => (int)v).ToArray(), 0);
        using var context = new OperationContext(ImageLimits.Default, default);
        using var image = new WebpLosslessDecoder(payload, expected.Length, 1, context).Decode();
        for (var i = 0; i < expected.Length; i++)
            Assert.Equal(new byte[] { 17, expected[i], 131, 0 }, image.GetRow8(0).Slice(i * 4, 4).ToArray());
    }

    [Fact]
    public void LargeSingleCacheSymbolUsesNormalHeaderAndPreservesTransparentZeroPixels()
    {
        var frequencies = new int[2328]; frequencies[2327] = 3;
        var payload = Payload(frequencies, [2327, 2327, 2327], 11);
        using var context = new OperationContext(ImageLimits.Default, default);
        using var image = new WebpLosslessDecoder(payload, 3, 1, context).Decode();
        Assert.Equal(new byte[12], image.GetRow8(0).ToArray());
    }

    [Fact]
    public void ScratchAndHeaderChargesEndPromptlyAndBorrowedRootSurvives()
    {
        using var root = new TestMemoryBudget(1_000_000);
        using (var context = new OperationContext(new() { MemoryBudget = root }, default))
        {
            var tree = new WebpPrefixCode(Enumerable.Range(0, 1304).Select(i => i % 31).ToArray(), context);
            Assert.Equal(tree.MemoryBytes, root.Bytes);
            Assert.True(root.Peak > root.Bytes);
            using var output = new MemoryStream(); var writer = new WebpBitWriter(output, context);
            tree.Header(writer, context); writer.Finish(); Assert.Equal(tree.MemoryBytes, root.Bytes);
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        using var subsequent = root.CreateScope(); Assert.True(subsequent.TryReserve(1_000_000, out var reservation));
        reservation.Dispose();
    }

    [Fact]
    public void AggregateScratchRefusalIsTerminalAndNextOperationSucceedsAfterRelease()
    {
        const int count = 1304;
        var persistent = count * 3 + 128; var scratch = (count * 2 - 1) * 12 + count * 4 + 256;
        using var root = new TestMemoryBudget(persistent + scratch);
        using var caller = root.CreateScope(); Assert.True(caller.TryReserve(1, out var held));
        var frequencies = Enumerable.Repeat(1, count).ToArray();
        using (var refused = new OperationContext(new() { MemoryBudget = root }, default))
        {
            var failure = Assert.Throws<ImageLimitException>(() => new WebpPrefixCode(frequencies, refused));
            Assert.Equal(ImageLimitKind.WorkingBytes, failure.Limit);
            Assert.Equal(ImageLimitScope.SharedBudget, failure.Scope);
            Assert.Equal(1, root.Refusals);
        }
        Assert.Equal(1, root.Bytes); Assert.False(root.IsDisposed);
        held.Dispose();
        using (var admitted = new OperationContext(new() { MemoryBudget = root }, default))
            AssertComplete(new WebpPrefixCode(frequencies, admitted));
        Assert.Equal(0, root.Bytes); Assert.Equal(1, root.Refusals);
    }

    [Fact]
    public void CancellationDuringAdmissionAndWorkRefusalReleaseAllOwnedStorage()
    {
        using var root = new TestMemoryBudget(1_000_000);
        using var cancellation = new CancellationTokenSource(); root.Admitted = _ => cancellation.Cancel();
        var frequencies = Enumerable.Repeat(1, 280).ToArray();
        using (var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token))
            Assert.Throws<OperationCanceledException>(() => new WebpPrefixCode(frequencies, context));
        Assert.Equal(0, root.Bytes); root.Admitted = null;
        using (var context = new OperationContext(new() { MemoryBudget = root, MaxWork = 281 }, default))
            Assert.Equal(ImageLimitKind.Work, Assert.Throws<ImageLimitException>(() => new WebpPrefixCode(frequencies, context)).Limit);
        Assert.Equal(0, root.Bytes);
        using (var context = new OperationContext(new() { MemoryBudget = root }, default))
            AssertComplete(new WebpPrefixCode(frequencies, context));
        Assert.Equal(0, root.Bytes);
    }

    [Fact]
    public void InvalidInputsAndAbsentSymbolsFailExplicitly()
    {
        using var context = new OperationContext(ImageLimits.Default, default);
        Assert.Throws<ArgumentOutOfRangeException>(() => new WebpPrefixCode([], context));
        Assert.Throws<ArgumentOutOfRangeException>(() => new WebpPrefixCode([1, -1], context));
        Assert.Throws<ArgumentOutOfRangeException>(() => new WebpPrefixCode([1, 1, 1], context, 1));
        var tree = new WebpPrefixCode([0, 2], context);
        using var output = new MemoryStream(); var writer = new WebpBitWriter(output, context);
        Assert.Throws<ArgumentOutOfRangeException>(() => tree.Write(writer, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => tree.Write(writer, 2));
    }

    private static void AssertComplete(WebpPrefixCode tree)
    { Assert.Equal(1 << 15, Enumerable.Range(0, tree.Alphabet).Where(i => tree.Length(i) != 0).Sum(i => 1 << (15 - tree.Length(i)))); }

    private static byte[] Payload(int[] frequencies, int[] symbols, int cacheBits)
    {
        using var context = new OperationContext(ImageLimits.Default, default);
        using var output = new MemoryStream();
        Span<byte> header = stackalloc byte[5]; header[0] = 0x2f;
        BinaryPrimitives.WriteUInt32LittleEndian(header[1..], (uint)(symbols.Length - 1) | 1u << 28); output.Write(header);
        var writer = new WebpBitWriter(output, context); writer.Write(0, 1); writer.Write(cacheBits == 0 ? 0 : 1, 1);
        if (cacheBits != 0) writer.Write(cacheBits, 4);
        writer.Write(0, 1);
        var green = new WebpPrefixCode(frequencies, context); green.Header(writer, context);
        var redFrequency = new int[256]; redFrequency[17] = 1;
        var blueFrequency = new int[256]; blueFrequency[131] = 1;
        var red = new WebpPrefixCode(redFrequency, context); red.Header(writer, context);
        var blue = new WebpPrefixCode(blueFrequency, context); blue.Header(writer, context);
        var alpha = new WebpPrefixCode(new int[256], context); alpha.Header(writer, context);
        new WebpPrefixCode(new int[40], context).Header(writer, context);
        foreach (var symbol in symbols)
        {
            green.Write(writer, symbol);
            if (symbol < 256) { red.Write(writer, 17); blue.Write(writer, 131); alpha.Write(writer, 0); }
        }
        writer.Finish(); return output.ToArray();
    }
}
