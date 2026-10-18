using System.IO.Compression;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class ZlibMaterializationTests
{
    [Theory]
    [InlineData(1, 0, 0, 0)] [InlineData(2, 1, 0, 0)] [InlineData(3, 2, 0, 0)]
    [InlineData(6, 4, 1, 1)] [InlineData(64, 11, 15, 4)]
    [InlineData(257, 16, 0, 7)] [InlineData(32768, 29, 8191, 13)]
    public void OverlappingAndMaximumHistoryMatchBcl(int distance, int symbol, int extra, int extraBits)
    {
        foreach (var stored in new[] { false, true })
        {
            var seed = Enumerable.Range(0, distance).Select(i => (byte)(73 * i + 19)).ToArray();
            var raw = seed.Concat(Enumerable.Range(0, 258).Select(i => seed[i % distance])).ToArray();
            var bits = new Writer();
            if (stored)
            {
                bits.Number(0, 3); bits.Align(); bits.Number(distance, 16); bits.Number(distance ^ 65535, 16);
                foreach (var value in seed) bits.Number(value, 8);
            }
            bits.Number(1, 1); bits.Number(1, 2);
            if (!stored) foreach (var value in seed) bits.Literal(value);
            bits.Code(197, 8); // Fixed length symbol 285: 258 bytes.
            bits.Code(symbol, 5); bits.Number(extra, extraBits); bits.Code(0, 7);
            var encoded = Envelope(bits, raw);
            var context = new OperationContext(ImageLimits.Default, default);
            var output = new byte[raw.Length];
            Assert.Equal(encoded.Length, ZlibFraming.ValidateAndInflate(encoded, output, context, out var complete));
            Assert.True(complete); Assert.Equal(raw, output); Assert.Equal(Inflate(encoded), output);
            var counted = new OperationContext(ImageLimits.Default, default);
            Assert.Equal(encoded.Length, ZlibFraming.Validate(encoded, raw.Length, counted));
            Assert.Equal(counted.RemainingWork, context.RemainingWork);
            Assert.Equal(counted.RemainingWorkingBytes, context.RemainingWorkingBytes);
        }
    }

    [Fact]
    public void EveryLiteralTruncationRetainsAtomicWorkOutputBoundsAndChecksumPosition()
    {
        var raw = Enumerable.Range(0, 260).Select(i => (byte)i).ToArray();
        var bits = new Writer(); bits.Number(1, 1); bits.Number(1, 2);
        foreach (var value in raw) bits.Literal(value);
        bits.Code(0, 7); var encoded = Envelope(bits, raw);
        using var cancellation = new CancellationTokenSource();
        var completeWork = 9380 + raw.Sum(value => value < 144 ? 8 : 9) + 7;
        foreach (var expected in new[] { raw.Length - 1, raw.Length, raw.Length + 1 })
        for (var length = 0; length <= encoded.Length; length++)
        foreach (var work in new[] { 1, 9379, 9380, 9381, completeWork - 1, completeWork, completeWork + 1 })
        {
            var limits = ImageLimits.Default with { MaxWork = work, MaxWorkingBytes = 4096 };
            var materialized = new OperationContext(limits, default);
            var perSymbol = new OperationContext(limits, cancellation.Token);
            var input = encoded[..length]; var output = Enumerable.Repeat((byte)165, expected + 16).ToArray();
            int firstEnd = -1, secondEnd = -1;
            var first = Record.Exception(() => firstEnd = ZlibFraming.ValidateAndInflate(input, output.AsSpan(8, expected), materialized, out _));
            var second = Record.Exception(() => secondEnd = ZlibFraming.Validate(input, expected, perSymbol));
            Assert.Equal(second?.GetType(), first?.GetType()); Assert.Equal(second?.Message, first?.Message);
            Assert.Equal(secondEnd, firstEnd); Assert.Equal(perSymbol.RemainingWork, materialized.RemainingWork);
            Assert.Equal(perSymbol.RemainingWorkingBytes, materialized.RemainingWorkingBytes);
            Assert.All(output[..8], value => Assert.Equal(165, value));
            Assert.All(output[(expected + 8)..], value => Assert.Equal(165, value));
            if (first is null) Assert.Equal(raw, output.AsSpan(8, expected).ToArray());
        }
        byte[] trailing = [.. encoded, 222, 173, 190, 239];
        var final = new byte[raw.Length];
        Assert.Equal(encoded.Length, ZlibFraming.ValidateAndInflate(trailing, final,
            new OperationContext(ImageLimits.Default, default), out var compatible));
        Assert.True(compatible); Assert.Equal(raw, final);
    }

    [Fact]
    public void CompleteLongCodeTreesPreserveEveryAssignedLiteralValue()
    {
        // A complete canonical comb: one leaf at depths 1..14 and two at15.
        // EOB has length1, symbols1..13 have lengths2..14, and 0/17 have15.
        var raw = Enumerable.Range(0, 31).SelectMany(_ => new byte[] { 0, 17, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }).ToArray();
        var bits = new Writer(); bits.Number(1, 1); bits.Number(2, 2);
        bits.Number(0, 5); bits.Number(1, 5); bits.Number(15, 4);
        ReadOnlySpan<int> order = [16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15];
        foreach (var symbol in order) bits.Number(symbol < 16 ? 4 : 0, 3);
        for (var symbol = 0; symbol < 257; symbol++)
            bits.Code(symbol is 0 or 17 ? 15 : symbol == 256 ? 1 : symbol is >= 1 and <= 13 ? symbol + 1 : 0, 4);
        bits.Code(1, 4); bits.Code(1, 4); // Complete two-symbol distance tree.
        foreach (var value in raw)
            if (value is 0 or 17) bits.Code(value == 0 ? 32766 : 32767, 15);
            else bits.Code((1 << (value + 1)) - 2, value + 1);
        bits.Code(0, 1); var encoded = Envelope(bits, raw); var original = encoded.ToArray();
        var output = new byte[raw.Length];
        var materialized = new OperationContext(ImageLimits.Default, default);
        using var cancellation = new CancellationTokenSource();
        var perSymbol = new OperationContext(ImageLimits.Default, cancellation.Token);
        Assert.Equal(encoded.Length, ZlibFraming.ValidateAndInflate(encoded, output, materialized, out var compatible));
        Assert.True(compatible); Assert.Equal(raw, output); Assert.Equal(Inflate(encoded), output);
        Assert.Equal(encoded.Length, ZlibFraming.Validate(encoded, raw.Length, perSymbol));
        Assert.Equal(perSymbol.RemainingWork, materialized.RemainingWork);
        Assert.Equal(perSymbol.RemainingWorkingBytes, materialized.RemainingWorkingBytes);
        Assert.Equal(original, encoded);
    }

    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void RollingRgbaHistoryKeepsMaximumAndOverlappingDistancesAcrossSlides(int depth)
    {
        // Stored history followed by 300 maximum-length fixed matches crosses
        // several 64-KiB slides. Construct None rows with nonzero history,
        // independently expanding copies before removing filters/byte swapping.
        const int seed = 65520, repetitions = 300;
        foreach (var (distance, symbol, extra, extraBits) in new[]
        {
            (1, 0, 0, 0), (2, 1, 0, 0), (3, 2, 0, 0), (6, 4, 1, 1),
            (64, 11, 15, 4), (257, 16, 0, 7), (32768, 29, 8191, 13)
        })
        foreach (var cancelable in new[] { false, true })
        {
            var rowBytes = 257 * (depth / 8) * 4; var rowLength = rowBytes + 1;
            var matchEnd = seed + repetitions * 258;
            var raw = new byte[(matchEnd + rowLength - 1) / rowLength * rowLength];
            for (var i = 0; i < seed; i++) raw[i] = (byte)(73 * i + 19);
            // Every future filter position constrains its independently traced
            // seed ancestor. Other bytes keep a nonzero deterministic pattern.
            for (var i = 0; i < matchEnd; i += rowLength)
                raw[i < seed ? i : seed - distance + (i - seed) % distance] = 0;
            for (var i = seed; i < matchEnd; i++) raw[i] = raw[i - distance];
            var expected = new byte[raw.Length / rowLength * rowBytes];
            for (var y = 0; y < raw.Length / rowLength; y++)
            {
                var source = raw.AsSpan(y * rowLength + 1, rowBytes);
                source.CopyTo(expected.AsSpan(y * rowBytes));
                if (depth == 16 && BitConverter.IsLittleEndian)
                    for (var x = 0; x < rowBytes; x += 2)
                        (expected[y * rowBytes + x], expected[y * rowBytes + x + 1]) =
                            (expected[y * rowBytes + x + 1], expected[y * rowBytes + x]);
            }
            var bits = new Writer(); bits.Number(0, 3); bits.Align();
            bits.Number(seed, 16); bits.Number(seed ^ 65535, 16);
            foreach (var value in raw.AsSpan(0, seed)) bits.Number(value, 8);
            bits.Number(1, 1); bits.Number(1, 2);
            for (var i = 0; i < repetitions; i++)
            {
                bits.Code(197, 8); bits.Code(symbol, 5); bits.Number(extra, extraBits);
            }
            foreach (var value in raw.AsSpan(matchEnd)) bits.Literal(value);
            bits.Code(0, 7); var encoded = Envelope(bits, raw);
            using var cancellation = new CancellationTokenSource();
            var token = cancelable ? cancellation.Token : default;
            using var image = ImageBuffer.Create(257, raw.Length / rowLength, depth);
            var output = new PngRgbaInflation(image);
            var context = new OperationContext(ImageLimits.Default, token);
            var counted = new OperationContext(ImageLimits.Default, token);
            Assert.Equal(encoded.Length, ZlibFraming.ValidateAndInflateRgba(encoded, raw.Length, context, output, out var complete));
            Assert.True(complete); Assert.True(output.Finish(System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(encoded.AsSpan(encoded.Length - 4))));
            Assert.Equal(encoded.Length, ZlibFraming.Validate(encoded, raw.Length, counted));
            Assert.Equal(counted.RemainingWork, context.RemainingWork);
            Assert.Equal(counted.RemainingWorkingBytes, context.RemainingWorkingBytes);
            output.DecodeRows(new OperationContext(ImageLimits.Default, token));
            Assert.Equal(expected, image.Pixels.ToArray());
            Assert.Equal(raw, Inflate(encoded));
        }
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void RollingLiteralTruncationsPreserveOriginalChargesAndErrors(bool cancelable)
    {
        // Independent small fixed stream: every truncation and the nearby
        // atomic bit budgets must agree with the unchanged counting path.
        var raw = Enumerable.Range(0, 25).Select(i => (byte)(i % 5 == 0 ? 0 : i * 7)).ToArray();
        var bits = new Writer(); bits.Number(1, 1); bits.Number(1, 2);
        foreach (var value in raw) bits.Literal(value);
        bits.Code(0, 7); var encoded = Envelope(bits, raw);
        var full = 9380 + raw.Sum(value => value < 144 ? 8 : 9) + 7;
        using var cancellation = new CancellationTokenSource();
        var token = cancelable ? cancellation.Token : default;
        foreach (var length in Enumerable.Range(0, encoded.Length + 1))
        foreach (var work in new[] { 1, 9379, 9380, 9381, 9387, 9388, 9395, 9396, full - 8, full - 7, full - 1, full, full + 1 })
        {
            var limits = ImageLimits.Default with { MaxWork = work, MaxWorkingBytes = 4096 };
            var context = new OperationContext(limits, token);
            var counted = new OperationContext(limits, token);
            using var image = ImageBuffer.Create(1, 5, 8);
            var output = new PngRgbaInflation(image); var input = encoded[..length];
            int end = -1, countedEnd = -1;
            var actual = Record.Exception(() => end = ZlibFraming.ValidateAndInflateRgba(input, raw.Length, context, output, out _));
            var expected = Record.Exception(() => countedEnd = ZlibFraming.Validate(input, raw.Length, counted));
            Assert.Equal(expected?.GetType(), actual?.GetType()); Assert.Equal(expected?.Message, actual?.Message);
            Assert.Equal(countedEnd, end); Assert.Equal(counted.RemainingWork, context.RemainingWork);
            Assert.Equal(counted.RemainingWorkingBytes, context.RemainingWorkingBytes);
        }
    }

    [Fact]
    public void RollingRowsAdmitWorkBeforeEachPredictorOrByteConversion()
    {
        byte[] raw = [1, 1, 2, 3, 4, 5, 6, 7, 8, 2, 9, 10, 11, 12, 13, 14, 15, 16];
        var bits = new Writer(); bits.Number(1, 1); bits.Number(1, 2);
        foreach (var value in raw) bits.Literal(value);
        bits.Code(0, 7); var encoded = Envelope(bits, raw);
        const int rowWork = 8 * 6 + 2 * 4;
        foreach (var work in new[] { rowWork - 1, rowWork })
        {
            using var image = ImageBuffer.Create(2, 2, 8);
            var output = new PngRgbaInflation(image);
            ZlibFraming.ValidateAndInflateRgba(encoded, raw.Length,
                new OperationContext(ImageLimits.Default, default), output, out var complete);
            Assert.True(complete); Assert.True(output.Finish(System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(encoded.AsSpan(encoded.Length - 4))));
            var original = image.Pixels.ToArray();
            var context = new OperationContext(ImageLimits.Default with { MaxWork = work }, default);
            Assert.Throws<ImageLimitException>(() => output.DecodeRows(context));
            Assert.Equal(work < rowWork ? work : 0, context.RemainingWork);
            Assert.Equal(work < rowWork ? original[..8] : new byte[] { 1, 2, 3, 4, 6, 8, 10, 12 }, image.GetRow8(0).ToArray());
            Assert.Equal(original[8..], image.GetRow8(1).ToArray());
        }
    }

    private static byte[] Inflate(byte[] encoded)
    {
        using var zlib = new ZLibStream(new MemoryStream(encoded), CompressionMode.Decompress);
        using var output = new MemoryStream(); zlib.CopyTo(output); return output.ToArray();
    }

    private static byte[] Envelope(Writer bits, byte[] raw)
    {
        uint a = 1, b = 0;
        foreach (var value in raw) { a = (a + value) % 65521; b = (b + a) % 65521; }
        bits.Align(); return [0x78, 0x01, .. bits.Bytes, (byte)(b >> 8), (byte)b, (byte)(a >> 8), (byte)a];
    }

    // Original fixed prefixes and LSB-first fields from RFC 1951 sections 3.1/3.2.
    private sealed class Writer
    {
        public List<byte> Bytes { get; } = [];
        private int used;
        public void Number(int value, int count) { for (var i = 0; i < count; i++) Bit(value >> i & 1); }
        public void Code(int value, int count) { for (var i = count - 1; i >= 0; i--) Bit(value >> i & 1); }
        public void Literal(int value) => Code(value < 144 ? value + 48 : value + 256, value < 144 ? 8 : 9);
        public void Align() { while ((used & 7) != 0) Bit(0); }
        private void Bit(int value) { if ((used & 7) == 0) Bytes.Add(0); Bytes[^1] |= (byte)(value << (used++ & 7)); }
    }
}
