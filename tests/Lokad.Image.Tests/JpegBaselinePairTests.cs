using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed partial class JpegBaselineBlockTests
{
    [Fact]
    public void PreparedPairsMatchIndependentCanonicalGrammarAtEveryPrefixAndPosition()
    {
        byte[] counts = [1, 1, 0, 0, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
        byte[] symbols = [7, 0, 1, 0x11, 0xf1, 0xf0, 8, 0x0b];
        using var context = Context(); var table = new JpegHuffman(counts, symbols, context);
        var before = context.RemainingWorkingBytes;
        table.PrepareBaselinePairs(context);
        try
        {
            Assert.Equal(before - JpegHuffman.BaselinePairBytes, context.RemainingWorkingBytes);
            var codes = CanonicalPairCodes(counts, symbols);
            var pairs = table.BaselinePairs!; var valid = 0;
            for (var prefix = 0; prefix < 4096; prefix++)
            for (var k = 1; k <= 63; k++)
            {
                var expected = PairOracle(Convert.ToString(prefix, 2).PadLeft(12, '0'), k, codes);
                var packed = pairs[prefix]; var secondIndex = k + (int)((packed >> 8) & 31);
                if (packed == 0 || secondIndex > 63) { Assert.Null(expected); continue; }
                Assert.NotNull(expected); valid++;
                Assert.Equal(expected.Value.Bits, (int)(packed & 15));
                Assert.Equal(expected.Value.First, (k + (int)((packed >> 4) & 15), (int)unchecked((sbyte)(packed >> 16))));
                var second = unchecked((int)packed) >> 24;
                Assert.Equal(expected.Value.Second, second == 0 ? null : ((int, int)?)(secondIndex, second));
            }
            Assert.True(valid > 0);
        }
        finally { table.ReleaseBaselinePairs(context); }
        Assert.Null(table.BaselinePairs); Assert.Equal(before, context.RemainingWorkingBytes);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void PreparedPairsPreserveActualBlocksFollowingReadsAndMalformedTails(bool cancelable)
    {
        using var tablesContext = Context();
        var dcCounts = new byte[16]; dcCounts[0] = 1;
        var acCounts = new byte[16]; acCounts[1] = 2; acCounts[3] = 3;
        var dc = new JpegHuffman(dcCounts, [0], tablesContext);
        var ac = new JpegHuffman(acCounts, [1, 0, 0xf1, 0xf0, 0x0b], tablesContext);
        ac.PrepareBaselinePairs(tablesContext);
        try
        {
            foreach (var suffix in new[] { "01", "10000", "10001", "1001", "1010", "" })
            foreach (var count in new[] { 2, 31, 62, 63 })
            for (var phase = 0; phase < 8; phase++)
            {
                var data = Stuff(new string('0', phase) + "0" + string.Concat(Enumerable.Repeat("001", count))
                    + suffix + "10110100101" + new string('0', 32));
                foreach (var length in new[] { data.Length, data.Length / 2, Math.Max(0, data.Length - 3) }.Distinct())
                {
                    var input = data.AsSpan(0, length).ToArray(); var expected = Run(true); var actual = Run(false);
                    Assert.Equal(expected.Error, actual.Error); Assert.Equal(expected.Message, actual.Message);
                    Assert.Equal(expected.Predictor, actual.Predictor); Assert.Equal(expected.Block, actual.Block);
                    Assert.Equal(expected.Position, actual.Position); Assert.Equal(expected.Tail, actual.Tail);

                    (Type? Error, string? Message, int Predictor, int[] Block, int Position, int Tail) Run(bool scalar)
                    {
                        using var cancellation = new CancellationTokenSource();
                        using var context = new OperationContext(ImageLimits.Default, cancelable ? cancellation.Token : default);
                        var reader = new JpegEntropyReader(input, 0, context);
                        var block = Enumerable.Repeat(12345, 66).ToArray(); var predictor = 17; var tail = -1;
                        Exception? error = null;
                        try
                        {
                            if (phase != 0) reader.ReadBits(phase);
                            if (scalar) ScalarBaseline(ref reader, block.AsSpan(1, 64), ref predictor, dc, ac, context);
                            else JpegDecoder.ReadBaselineBlock(ref reader, block.AsSpan(1, 64), ref predictor, dc, ac, context);
                            tail = reader.ReadBits(11);
                        }
                        catch (ImageDataException failure) { error = failure; }
                        return (error?.GetType(), error?.Message, predictor, block, reader.Position, tail);
                    }
                }
            }
        }
        finally { ac.ReleaseBaselinePairs(tablesContext); }
    }

    [Fact]
    public void PairAdmissionAndCancellationRefundOnlyOwnedStorage()
    {
        using var tables = Context(); var counts = new byte[16]; counts[1] = 2;
        var ac = new JpegHuffman(counts, [1, 0], tables);
        using var root = new TestMemoryBudget(JpegHuffman.BaselinePairBytes);
        using var blocker = root.CreateScope(); Assert.True(blocker.TryReserve(1, out var held));
        var limits = ImageLimits.Default with { MemoryBudget = root };
        using (var refused = new OperationContext(limits, default))
        {
            var error = Assert.Throws<ImageLimitException>(() => ac.PrepareBaselinePairs(refused));
            Assert.Equal(ImageLimitScope.SharedBudget, error.Scope); Assert.Equal(1, root.Refusals);
            Assert.Null(ac.BaselinePairs); Assert.Equal(1, root.Bytes);
        }
        held!.Dispose();
        using var canceled = new CancellationTokenSource();
        using (var context = new OperationContext(limits, canceled.Token))
        {
            root.Admitted = _ => canceled.Cancel();
            Assert.Throws<OperationCanceledException>(() => ac.PrepareBaselinePairs(context));
            Assert.Null(ac.BaselinePairs); Assert.Equal(0, root.Bytes);
        }
        root.Admitted = null;
        using (var context = new OperationContext(limits, default))
        {
            ac.PrepareBaselinePairs(context); Assert.Equal(JpegHuffman.BaselinePairBytes, root.Bytes);
            ac.ReleaseBaselinePairs(context); Assert.Equal(0, root.Bytes);
        }
        Assert.False(root.IsDisposed);
    }

    [Fact]
    public void CanonicallyEmptyPairPlansKeepTheBorrowedAllowanceUntouched()
    {
        using var tables = Context();
        var counts = new byte[16]; counts[0] = 1;
        var onlyEob = new JpegHuffman(counts, [0], tables);
        using var root = new TestMemoryBudget(0);
        using var context = new OperationContext(ImageLimits.Default with { MemoryBudget = root }, default);
        Assert.False(onlyEob.CanPrepareBaselinePairs);
        onlyEob.PrepareBaselinePairs(context);
        Assert.Null(onlyEob.BaselinePairs); Assert.Equal(0, root.Attempts); Assert.Equal(0, root.Bytes);
        onlyEob.ReleaseBaselinePairs(context); Assert.False(root.IsDisposed);
    }

    private static Dictionary<string, byte> CanonicalPairCodes(byte[] counts, byte[] symbols)
    {
        var result = new Dictionary<string, byte>(); var code = 0; var at = 0;
        for (var n = 1; n <= 16; n++)
        {
            for (var i = 0; i < counts[n - 1]; i++) result.Add(Convert.ToString(code++, 2).PadLeft(n, '0'), symbols[at++]);
            code *= 2;
        }
        return result;
    }

    private static (int Bits, (int, int) First, (int, int)? Second)? PairOracle(string bits, int k, Dictionary<string, byte> codes)
    {
        var at = 0; var first = (0, 0);
        for (var i = 0; i < 2; i++)
        {
            if (k > 63) return null;
            var code = ""; byte token = 0; var found = false;
            while (at < bits.Length)
            {
                code += bits[at++];
                if (codes.TryGetValue(code, out token)) { found = true; break; }
            }
            if (!found) return null;
            if (token == 0) return i == 1 ? (at, first, null) : null;
            var size = token & 15; k += token >> 4;
            if (k > 63 || size is < 1 or > 7 || at + size > bits.Length) return null;
            var value = Convert.ToInt32(bits.Substring(at, size), 2); at += size;
            if (value < 1 << (size - 1)) value += 1 - (1 << size);
            if (i == 0) first = (k++, value);
            else return (at, first, (k, value));
        }
        return null;
    }
}
