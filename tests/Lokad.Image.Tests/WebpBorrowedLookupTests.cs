using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class WebpBorrowedLookupTests
{
    [Fact]
    public void WideRootsKeepEveryCanonicalSymbolAndLongCodeFallback()
    {
        var lengths = Enumerable.Repeat((byte)9, 256).Concat(Enumerable.Repeat((byte)12, 1024))
            .Concat(Enumerable.Repeat((byte)13, 2048)).ToArray();
        var codes = Codes(lengths); var construction = Context(); var tree = new WebpHuffman(lengths, construction);
        var remaining = construction.RemainingWork; var lookup = new uint[4096]; tree.BuildWideGreen(lookup);
        Assert.Equal(remaining, construction.RemainingWork);
        for (var phase = 0; phase < 8; phase++)
        {
            var input = Pack(new string('0', phase) + string.Concat(codes.Select(c => c.Bits)) + "101001011");
            var reader = new WebpBitReader(input, Context()); reader.Read(phase); var consumed = phase;
            foreach (var code in codes)
            {
                var position = reader.Position;
                if (!reader.TryReadWideGreen(lookup, out var symbol))
                {
                    Assert.Equal(13, code.Bits.Length); Assert.Equal(position, reader.Position);
                    symbol = tree.Read(ref reader);
                }
                Assert.Equal(code.Symbol, symbol); consumed += code.Bits.Length;
                Assert.Equal((consumed + 7) / 8, reader.Position);
            }
            Assert.Equal(0x1a5, reader.Read(9));
        }
    }

    [Fact]
    public void WideLookupRetainsPartialBudgetsTruncationAndCancellation()
    {
        var tree = new WebpHuffman(Enumerable.Repeat((byte)12, 4096).ToArray(), Context());
        var lookup = new uint[4096]; tree.BuildWideGreen(lookup);
        for (var phase = 0; phase < 8; phase++)
        foreach (var size in new[] { 1, 2, 3 })
        foreach (var budget in Enumerable.Range(0, 13).Append(18))
        {
            var input = Pack(new string('0', phase) + "101010111100" + "10101")[..size];
            var actualContext = Context(budget + 1); var expectedContext = Context(budget + 1);
            var actual = new WebpBitReader(input, actualContext); var expected = new WebpBitReader(input, expectedContext);
            actual.Read(phase); expected.Read(phase);
            Assert.Equal(Read(tree, lookup, ref expected, false), Read(tree, lookup, ref actual, true));
            Assert.Equal(expected.Position, actual.Position); Assert.Equal(expectedContext.RemainingWork, actualContext.RemainingWork);
        }
        using var cancellation = new CancellationTokenSource();
        var context = new OperationContext(ImageLimits.Default, cancellation.Token);
        var canceled = new WebpBitReader(new byte[] { 255, 255 }, context); cancellation.Cancel();
        try { canceled.TryReadWideGreen(lookup, out _); Assert.Fail("Canceled lookup must fail."); }
        catch (OperationCanceledException) { }
        Assert.Equal(0, canceled.Position); Assert.Equal(ImageLimits.Default.MaxWork, context.RemainingWork);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void AuthoredCacheAndCopyPacketKeepsPixelsWorkAndExactMemory(bool crossingCopy)
    {
        var (payload, expected) = Packet(crossingCopy); var source = payload.ToArray();
        const long memory = 128 * 128 * 8 + 544;
        var limits = ImageLimits.Default with { MaxWorkingBytes = memory };
        var context = new OperationContext(limits, default);
        using var actual = new WebpLosslessDecoder(payload, 128, 128, context, header: false).Decode();
        for (var y = 0; y < 128; y++) Assert.Equal(expected.AsSpan(y * 512, 512).ToArray(), actual.GetRow8(y).ToArray());
        Assert.Equal(crossingCopy ? 281242 : 282519, limits.MaxWork - context.RemainingWork);
        Assert.Equal(65536, memory - context.RemainingWorkingBytes);
        Assert.Equal(source, payload);
        Assert.Throws<ImageLimitException>(() => DecodeAtLimit(payload, memory - 1));
    }

    private static void DecodeAtLimit(byte[] payload, long memory)
    {
        var context = new OperationContext(ImageLimits.Default with { MaxWorkingBytes = memory }, default);
        using var decoded = new WebpLosslessDecoder(payload, 128, 128, context, header: false).Decode();
    }

    private static OperationContext Context(long work = long.MaxValue)
        => new(ImageLimits.Default with { MaxWork = work }, default);
    private static (int? Value, Type? Error, string? Message) Read(WebpHuffman tree, uint[] lookup, ref WebpBitReader reader, bool wide)
    {
        try
        {
            return (wide && reader.TryReadWideGreen(lookup, out var value) ? value : tree.Read(ref reader), null, null);
        }
        catch (Exception error) when (error is ImageDataException or ImageLimitException)
        { return (null, error.GetType(), error.Message); }
    }
    private static (string Bits, int Symbol)[] Codes(byte[] lengths)
    {
        var result = new List<(string, int)>(); var code = 0;
        for (var width = 1; width <= 15; width++)
        {
            for (var symbol = 0; symbol < lengths.Length; symbol++)
                if (lengths[symbol] == width) result.Add((Convert.ToString(code++, 2).PadLeft(width, '0'), symbol));
            code <<= 1;
        }
        return result.ToArray();
    }
    private static byte[] Pack(string source)
    {
        var writer = new Bits(); foreach (var bit in source) writer.Field(bit == '1' ? 1u : 0u, 1);
        return writer.Bytes();
    }

    // Original RFC9649 fields: uniform ten-bit green/cache alphabet, three
    // zero-bit channels, literal/cache tokens and an explicit distance-one copy.
    // Blue11 ensures overwritten lookup words resemble valid nonzero code lengths.
    private static (byte[] Payload, byte[] Rgba) Packet(bool crossingCopy)
    {
        var writer = new Bits(); writer.Field(0, 1); // No transforms.
        writer.Field(1, 1); writer.Field(10, 4); writer.Field(0, 1); // Cache, no spatial groups.
        writer.Field(0, 1); writer.Field(10, 4); // Normal Huffman; fourteen code-length entries.
        int[] order = [17, 18, 0, 1, 2, 3, 4, 5, 16, 6, 7, 8, 9, 10];
        foreach (var symbol in order) writer.Field(symbol is 0 or 10 ? 1u : 0u, 3);
        writer.Field(0, 1); // All1304 lengths: first1024 are10, the rest zero.
        for (var i = 0; i < 1304; i++) writer.Field(i < 1024 ? 1u : 0u, 1);
        Leaf(255); Leaf(11); Leaf(255); Leaf(13);
        var pixels = new uint[128 * 128]; var cache = new uint[1024]; var position = 0;
        while (position < pixels.Length)
        {
            if (crossingCopy && position == 12224)
            {
                writer.Code(270, 10); writer.Field(0, 6); writer.Field(24, 5); // Length129, distance121=>1.
                for (var i = 0; i < 129; i++) Emit(pixels[position - 1]);
            }
            else if (position == 12354)
            {
                writer.Code(280, 10); Emit(cache[0]);
            }
            else
            {
                var green = position & 255; writer.Code(green, 10); Emit(0xffff000bu | (uint)green << 8);
            }
        }
        var rgba = new byte[pixels.Length * 4];
        for (var i = 0; i < pixels.Length; i++)
        {
            rgba[i * 4] = (byte)(pixels[i] >> 16); rgba[i * 4 + 1] = (byte)(pixels[i] >> 8);
            rgba[i * 4 + 2] = (byte)pixels[i]; rgba[i * 4 + 3] = (byte)(pixels[i] >> 24);
        }
        return (writer.Bytes(), rgba);

        void Leaf(int symbol)
        {
            writer.Field(1, 1); writer.Field(0, 1); writer.Field(1, 1); writer.Field((uint)symbol, 8);
        }
        void Emit(uint pixel)
        {
            pixels[position++] = pixel; cache[unchecked(pixel * 0x1e35a7bdu) >> 22] = pixel;
        }
    }

    private sealed class Bits
    {
        private readonly List<byte> bytes = []; private int count;
        public void Field(uint value, int width)
        {
            for (var bit = 0; bit < width; bit++)
            {
                if ((count & 7) == 0) bytes.Add(0);
                bytes[^1] |= (byte)((value >> bit & 1) << (count & 7)); count++;
            }
        }
        public void Code(int value, int width)
        {
            for (var bit = width - 1; bit >= 0; bit--) Field((uint)(value >> bit) & 1, 1);
        }
        public byte[] Bytes() => bytes.ToArray();
    }
}
