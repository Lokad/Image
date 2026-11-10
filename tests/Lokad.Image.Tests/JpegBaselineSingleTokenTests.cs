using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed partial class JpegBaselineBlockTests
{
    [Fact]
    public void PreparedSinglesMatchCanonicalSymbolsSignsAndPairPriorityForEveryPrefix()
    {
        byte[] shortCounts = new byte[16]; shortCounts[1] = 3;
        byte[] longCounts = new byte[16]; longCounts[2] = 1; longCounts[9] = 2; longCounts[10] = 1;
        var singles = 0; var ends = 0; var misses = 0; var pairsSeen = 0;
        foreach (var (counts, symbols) in new (byte[], byte[])[]
        {
            (shortCounts, [0x0a, 1, 0]), (shortCounts, [0xfa, 1, 0]),
            (longCounts, [1, 0, 8, 1])
        })
        {
            using var context = Context(); var table = new JpegHuffman(counts, symbols, context);
            var codes = CanonicalPairCodes(counts, symbols); var before = context.RemainingWorkingBytes;
            Assert.True(table.CanPrepareBaselinePairs); table.PrepareBaselinePairs(context);
            try
            {
                Assert.Equal(before - JpegHuffman.BaselinePairBytes, context.RemainingWorkingBytes);
                for (var prefix = 0; prefix < 4096; prefix++)
                {
                    var bits = Convert.ToString(prefix, 2).PadLeft(12, '0'); var packed = table.BaselinePairs![prefix];
                    if (PairOracle(bits, 1, codes) is not null)
                    {
                        Assert.NotEqual(0u, packed); Assert.Equal(0u, packed & 0xc000); pairsSeen++; continue;
                    }
                    var expected = SingleTokenOracle(bits, codes);
                    if (expected is null) { Assert.Equal(0u, packed); misses++; continue; }
                    Assert.Equal(expected.Value.Bits, (int)(packed & 15));
                    Assert.Equal(expected.Value.End ? 0x4000u : 0x8000u, packed & 0xc000);
                    if (expected.Value.End) { ends++; continue; }
                    singles++; Assert.Equal(expected.Value.Run, (int)((packed >> 4) & 15));
                    var value = ((packed >> 8) & 63) == 63
                        ? unchecked((int)packed) >> 16 : unchecked((sbyte)(packed >> 16));
                    Assert.Equal(expected.Value.Value, value);
                }
            }
            finally { table.ReleaseBaselinePairs(context); }
            Assert.Null(table.BaselinePairs); Assert.Equal(before, context.RemainingWorkingBytes);
        }
        Assert.True(singles > 0 && ends > 0 && misses > 0 && pairsSeen > 0);
    }

    private static (int Bits, int Run, int Value, bool End)? SingleTokenOracle(string bits, Dictionary<string, byte> codes)
    {
        var code = "";
        for (var at = 0; at < Math.Min(10, bits.Length); at++)
        {
            code += bits[at];
            if (!codes.TryGetValue(code, out var token)) continue;
            if (token == 0) return (at + 1, 0, 0, true);
            var size = token & 15;
            if (size is < 1 or > 10 || at + 1 + size > bits.Length) return null;
            var value = Convert.ToInt32(bits.Substring(at + 1, size), 2);
            if (value < 1 << (size - 1)) value += 1 - (1 << size);
            return (at + 1 + size, token >> 4, value, false);
        }
        return null;
    }

    [Theory]
    [InlineData(false, 0)] [InlineData(true, 0)]
    [InlineData(false, 15)] [InlineData(true, 15)]
    public void PreparedSinglesPreserveSignedSamplesCursorAndFailureBoundaries(bool cancelable, int run)
    {
        using var tablesContext = Context();
        var dcCounts = new byte[16]; dcCounts[0] = 1;
        var acCounts = new byte[16]; acCounts[1] = 3;
        var dc = new JpegHuffman(dcCounts, [0], tablesContext);
        var ac = new JpegHuffman(acCounts, [(byte)(run << 4 | 10), 1, 0], tablesContext);
        ac.PrepareBaselinePairs(tablesContext);
        try
        {
            foreach (var token in new[] { "011", "000000000000", "001111111111", "001000000000" })
            foreach (var suffix in new[] { "10", "11", "" })
            foreach (var count in new[] { 1, 2, 4, 62, 63 })
            for (var phase = 0; phase < 8; phase++)
            {
                var data = Stuff(new string('0', phase) + "0" + string.Concat(Enumerable.Repeat(token, count))
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

}
