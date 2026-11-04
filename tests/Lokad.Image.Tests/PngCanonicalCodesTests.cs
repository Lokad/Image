using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class PngCanonicalCodesTests
{
    [Fact]
    public void Rfc1951ExampleHasTheSpecifiedWireCodes()
    {
        // RFC1951 section3.2.2's A..H example, with wire-order bit reversal.
        byte[] lengths = [3, 3, 3, 3, 3, 2, 4, 4];
        Assert.Equal(new[] { 2, 6, 1, 5, 3, 0, 7, 15 }, Codes(lengths));
    }

    [Fact]
    public void SparsePairsCoverEveryLegalWidthAndPreserveAbsentSymbols()
    {
        var lengths = new byte[288];
        for (var width = 1; width <= 15; width++)
        {
            lengths[0] = lengths[^1] = (byte)width;
            var codes = Codes(lengths);
            Assert.Equal(0, codes[0]); Assert.Equal(1 << (width - 1), codes[^1]);
            Assert.All(codes[1..^1], value => Assert.Equal(0, value));
        }
        Array.Clear(lengths); Assert.All(Codes(lengths), value => Assert.Equal(0, value));
        Assert.Empty(Codes([]));
    }

    [Fact]
    public void CompleteMixedWidthTreesMatchIndependentBinaryIntervals()
    {
        foreach (var phase in new[] { 0, 17, 151 })
        {
            var lengths = new byte[288];
            for (var i = 0; i < 16; i++) lengths[(i * 73 + phase) % lengths.Length] = (byte)Math.Min(i + 1, 15);
            var original = lengths.ToArray(); var codes = Codes(lengths);
            // The complete comb tree partitions [0,32768) into aligned binary
            // intervals. This oracle uses their positions, without width counts
            // or canonical-code prefix recurrences.
            var position = 0;
            foreach (var symbol in Enumerable.Range(0, lengths.Length).Where(i => lengths[i] != 0)
                .OrderBy(i => lengths[i]).ThenBy(i => i))
            {
                var width = lengths[symbol]; var forward = position >> (15 - width); var reverse = 0;
                for (var bit = 0; bit < width; bit++) { reverse = reverse * 2 + (forward & 1); forward >>= 1; }
                Assert.Equal(reverse, codes[symbol]); Assert.Equal(0, position % (1 << (15 - width)));
                position += 1 << (15 - width);
            }
            Assert.Equal(32768, position); Assert.Equal(original, lengths);
            for (var i = 0; i < lengths.Length; i++) if (lengths[i] == 0) Assert.Equal(0, codes[i]);
        }
    }

    [Fact]
    public void DirtyCodeStorageClearsAbsentSymbolsAndPreservesTheBorrowedViewBoundary()
    {
        byte[] lengths = [3, 3, 3, 3, 3, 2, 4, 4];
        var storage = Enumerable.Repeat(-12345, 320).ToArray(); var expected = Codes(lengths);
        DeflateHuffman.Codes(lengths, storage.AsSpan(7, 29));
        Assert.Equal(expected, storage.AsSpan(7, lengths.Length).ToArray());
        Assert.All(storage[..7], value => Assert.Equal(-12345, value));
        Assert.All(storage[(7 + lengths.Length)..], value => Assert.Equal(-12345, value));
        Array.Clear(lengths); DeflateHuffman.Codes(lengths, storage.AsSpan(7, 29));
        Assert.All(storage.AsSpan(7, lengths.Length).ToArray(), value => Assert.Equal(0, value));
        var before = storage.ToArray(); DeflateHuffman.Codes([], storage);
        Assert.Throws<ArgumentOutOfRangeException>(() => DeflateHuffman.Codes(lengths, storage.AsSpan(0, lengths.Length - 1)));
        Assert.Equal(before, storage);
    }

    private static int[] Codes(byte[] lengths)
    { var result = new int[lengths.Length]; DeflateHuffman.Codes(lengths, result); return result; }
}
