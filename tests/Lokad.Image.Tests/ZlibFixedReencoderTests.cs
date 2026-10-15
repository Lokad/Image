using System.IO.Compression;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class ZlibFixedReencoderTests
{
    [Fact]
    public void OutputArrayBoundRefusesWithoutGrowingTheCallerStream()
    {
        using var output = new MemoryStream(); output.Position = Array.MaxLength - 100L;
        using var bounded = new LimitedWriteStream(output, int.MaxValue, default);
        Assert.False(bounded.TryPrepareBufferedPng(101));
        Assert.Equal(0, output.Capacity); Assert.Equal(0, output.Length); Assert.Equal(Array.MaxLength - 100L, output.Position);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void EveryLengthDistanceAndOverlappingReferenceKeepsExactBytes(bool multipleBlocks)
    {
        var g = new Grammar(); g.Header(!multipleBlocks);
        for (var i = 0; i < 100000; i++) g.Literal(0);
        for (var i = 0; i < 32768; i++) g.Literal((byte)(i * 73 + (i >> 5)));
        if (multipleBlocks) { g.End(); g.Header(true); }
        for (var distance = 0; distance < 30; distance++)
        for (var length = 257; length <= 285; length++)
        foreach (var maximum in new[] { false, true }) g.Match(length, distance, maximum);
        g.End(); var original = g.Finish(); var output = new byte[original.Length];
        var work = ZlibFixedReencoder.RequiredWork(original.Length);
        var context = new OperationContext(ImageLimits.Default with { MaxWork = work }, default);
        Assert.True(ZlibFixedReencoder.TryRecode(original, output, g.Bytes.Count, context, out var written));
        Assert.True(written < original.Length); Assert.Equal(0, context.RemainingWork);
        Assert.Equal(original[..2], output[..2]); Assert.Equal(original[^4..], output[(written - 4)..written]);
        Assert.Equal(g.Bytes.ToArray(), Inflate(original)); Assert.Equal(g.Bytes.ToArray(), Inflate(output[..written]));
    }

    [Fact]
    public void AllTruncationsAndInsufficientCapacityLeaveDestinationUntouched()
    {
        var g = new Grammar(); g.Header(true); for (var i = 0; i < 1000; i++) g.Literal((byte)(i & 1)); g.End();
        var original = g.Finish(); var context = new OperationContext(ImageLimits.Default, default); var scratch = new byte[original.Length];
        Assert.True(ZlibFixedReencoder.TryRecode(original, scratch, 1000, context, out var size));
        Assert.Equal(g.Bytes.ToArray(), Inflate(scratch[..size]));
        for (var n = 0; n < original.Length; n++) Check(original[..n], original.Length, 1000);
        for (var capacity = 0; capacity < size; capacity++) Check(original, capacity, 1000);
        Check(original, original.Length, 999); Check(original, original.Length, 1001);

        static void Check(byte[] input, int capacity, int expected)
        {
            var target = Enumerable.Repeat((byte)0xA5, capacity).ToArray(); var before = target.ToArray();
            Assert.False(ZlibFixedReencoder.TryRecode(input, target, expected, new OperationContext(ImageLimits.Default, default), out var written));
            Assert.Equal(0, written); Assert.Equal(before, target);
        }
    }

    [Fact]
    public void WorkCancellationOverlapAndUnsupportedBlocksRefuseWithoutMutation()
    {
        var g = new Grammar(); g.Header(true); for (var i = 0; i < 1000; i++) g.Literal(0); g.End(); var input = g.Finish();
        var output = Enumerable.Repeat((byte)0xAC, input.Length).ToArray(); var before = output.ToArray();
        var budget = ZlibFixedReencoder.RequiredWork(input.Length) - 1;
        var limited = new OperationContext(ImageLimits.Default with { MaxWork = budget }, default);
        Assert.False(ZlibFixedReencoder.TryRecode(input, output, 1000, limited, out _)); Assert.Equal(budget, limited.RemainingWork);
        using var cancellation = new CancellationTokenSource(); var canceled = new OperationContext(ImageLimits.Default, cancellation.Token); cancellation.Cancel();
        Assert.False(ZlibFixedReencoder.TryRecode(input, output, 1000, canceled, out _)); Assert.Equal(ImageLimits.Default.MaxWork, canceled.RemainingWork);
        Assert.False(ZlibFixedReencoder.TryRecode(input, input, 1000, new OperationContext(ImageLimits.Default, default), out _));
        foreach (var type in new[] { 0, 2, 3 })
        {
            var unsupported = input.ToArray(); unsupported[2] = (byte)((unsupported[2] & ~6) | type << 1);
            Assert.False(ZlibFixedReencoder.TryRecode(unsupported, output, 1000, new OperationContext(ImageLimits.Default, default), out _));
        }
        Assert.Equal(before, output);
    }

    [Theory]
    [InlineData(286)] [InlineData(287)] [InlineData(30)] [InlineData(31)] [InlineData(0)]
    public void ReservedCodesAndUnavailableHistoryRefuse(int symbol)
    {
        var g = new Grammar(); g.Header(true);
        if (symbol >= 286) g.Huffman(symbol);
        else { g.Huffman(257); g.Huffman(symbol, distance: true); }
        g.End(); var input = g.Finish();
        Assert.False(ZlibFixedReencoder.TryRecode(input, new byte[1000], 3, new OperationContext(ImageLimits.Default, default), out _));
    }

    [Fact]
    public void DegenerateAndOverlongHuffmanTreesHaveExplicitFallbacks()
    {
        var frequencies = new int[286]; var lengths = new byte[286];
        Assert.True(ZlibFixedReencoder.BuildLengths(frequencies, lengths)); Assert.Equal(1, lengths[0]);
        frequencies[256] = 100; Assert.True(ZlibFixedReencoder.BuildLengths(frequencies, lengths)); Assert.Equal(1, lengths[256]);
        frequencies.AsSpan().Clear(); frequencies[0] = frequencies[1] = 1;
        for (var i = 2; i < 24; i++) frequencies[i] = frequencies[i - 1] + frequencies[i - 2];
        Assert.False(ZlibFixedReencoder.BuildLengths(frequencies, lengths));
        var g = new Grammar(); g.Header(true); g.End(); var empty = g.Finish();
        Assert.False(ZlibFixedReencoder.TryRecode(empty, new byte[1000], 0, new OperationContext(ImageLimits.Default, default), out _));
    }

    [Fact]
    public void SingleDistanceAlphabetKeepsLongOverlappingMatches()
    {
        var g = new Grammar(); g.Header(true); g.Literal(17);
        for (var i = 0; i < 1000; i++) g.Match(285, 0, false);
        g.End(); var input = g.Finish(); var output = new byte[input.Length];
        Assert.True(ZlibFixedReencoder.TryRecode(input, output, g.Bytes.Count, new OperationContext(ImageLimits.Default, default), out var size));
        Assert.Equal(g.Bytes.ToArray(), Inflate(output[..size]));
    }

    private static byte[] Inflate(byte[] input)
    {
        using var zlib = new ZLibStream(new MemoryStream(input), CompressionMode.Decompress); using var output = new MemoryStream(); zlib.CopyTo(output); return output.ToArray();
    }

    // Original independent fixed-code grammars from RFC1951. Huffman bits are
    // emitted in chronological MSB order; integer fields are emitted LSB first.
    private sealed class Grammar
    {
        private readonly List<byte> encoded = []; private int bits, count;
        internal List<byte> Bytes { get; } = [];
        private void Bit(int value) { bits |= value << count++; if (count == 8) { encoded.Add((byte)bits); bits = count = 0; } }
        private void Field(int value, int width) { for (var i = 0; i < width; i++) Bit((value >> i) & 1); }
        internal void Header(bool final) { Field(final ? 1 : 0, 1); Field(1, 2); }
        internal void End() => Huffman(256);
        internal void Literal(byte value) { Huffman(value); Bytes.Add(value); }
        internal void Huffman(int value, bool distance = false)
        {
            var width = distance ? 5 : value <= 143 ? 8 : value <= 255 ? 9 : value <= 279 ? 7 : 8;
            var code = distance ? value : value <= 143 ? value + 48 : value <= 255 ? value + 256 : value <= 279 ? value - 256 : value - 88;
            for (var i = width - 1; i >= 0; i--) Bit((code >> i) & 1);
        }
        internal void Match(int length, int distance, bool maximum)
        {
            ReadOnlySpan<int> lb = [3,4,5,6,7,8,9,10,11,13,15,17,19,23,27,31,35,43,51,59,67,83,99,115,131,163,195,227,258];
            ReadOnlySpan<int> le = [0,0,0,0,0,0,0,0,1,1,1,1,2,2,2,2,3,3,3,3,4,4,4,4,5,5,5,5,0];
            ReadOnlySpan<int> db = [1,2,3,4,5,7,9,13,17,25,33,49,65,97,129,193,257,385,513,769,1025,1537,2049,3073,4097,6145,8193,12289,16385,24577];
            ReadOnlySpan<int> de = [0,0,0,0,1,1,2,2,3,3,4,4,5,5,6,6,7,7,8,8,9,9,10,10,11,11,12,12,13,13];
            var lx = maximum ? (1 << le[length - 257]) - 1 : 0; var dx = maximum ? (1 << de[distance]) - 1 : 0;
            Huffman(length); Field(lx, le[length - 257]); Huffman(distance, true); Field(dx, de[distance]);
            for (var i = 0; i < lb[length - 257] + lx; i++) Bytes.Add(Bytes[Bytes.Count - db[distance] - dx]);
        }
        internal byte[] Finish()
        {
            if (count != 0) encoded.Add((byte)bits);
            uint a = 1, b = 0; foreach (var value in Bytes) { a = (a + value) % 65521; b = (b + a) % 65521; }
            return [0x78, 1, .. encoded, (byte)(b >> 8), (byte)b, (byte)(a >> 8), (byte)a];
        }
    }
}
