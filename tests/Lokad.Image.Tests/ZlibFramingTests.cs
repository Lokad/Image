using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class ZlibFramingTests
{
    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    [InlineData(4)] [InlineData(5)] [InlineData(6)] [InlineData(7)]
    public void PrefixLookaheadPreservesEveryStoredBlockAlignmentAndExactWork(int literals)
    {
        // RFC 1951 sections 3.2.3/3.2.6: literal 144 has a nine-bit fixed
        // prefix, EOB has seven. Varying its count covers all bit alignments.
        var bits = new BitWriter();
        bits.Number(0, 1); bits.Number(1, 2); // Nonfinal fixed block.
        for (var i = 0; i < literals; i++) bits.Code(400, 9);
        bits.Code(0, 7); // End of fixed block.
        bits.Number(1, 1); bits.Number(0, 2); bits.Align(); // Final stored block.
        bits.Number(1, 16); bits.Number(65534, 16); bits.Number(7, 8);
        byte[] encoded = [0x78, 0x01, .. bits.Bytes(), 0, 0, 0, 0];
        // One work unit/block, all consumed bits, fixed-tree and lookup construction,
        // and one stored byte. Lookahead must never charge unused bits.
        var work = 720L + 8192 + 9 * literals;
        var limits = new ImageLimits { MaxWork = work, MaxWorkingBytes = 4096 };
        Assert.Equal(encoded.Length, ZlibFraming.Validate(encoded, literals + 1, new OperationContext(limits, default)));
        Assert.Equal(encoded.Length, ZlibFraming.Validate([.. encoded, 53, 97, 211], literals + 1, new OperationContext(limits, default)));
        Assert.Throws<ImageLimitException>(() => ZlibFraming.Validate(encoded, literals + 1,
            new OperationContext(limits with { MaxWork = work - 1 }, default)));
        Assert.Throws<ImageLimitException>(() => ZlibFraming.Validate(encoded, literals + 1,
            new OperationContext(limits with { MaxWorkingBytes = 4095 }, default)));
        for (var length = 0; length < encoded.Length; length++)
            Assert.Throws<ImageDataException>(() => ZlibFraming.Validate(encoded.AsSpan(0, length), literals + 1,
                new OperationContext(ImageLimits.Default, default)));
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)]
    [InlineData(3)] [InlineData(4)] [InlineData(5)]
    public void WordLookaheadPreservesStoredPayloadSkipsAndTheFinalChecksum(int storedLength)
    {
        // Original RFC 1951 stream: fixed literal, nonfinal stored payload,
        // final fixed literal. Payload lengths exercise every word boundary.
        var payload = Enumerable.Range(0, storedLength).Select(i => (byte)(53 * i + 1)).ToArray();
        var bits = new BitWriter();
        bits.Number(0, 1); bits.Number(1, 2); bits.Code(400, 9); bits.Code(0, 7);
        bits.Number(0, 1); bits.Number(0, 2); bits.Align();
        bits.Number(storedLength, 16); bits.Number(storedLength ^ 65535, 16);
        foreach (var value in payload) bits.Number(value, 8);
        bits.Number(1, 1); bits.Number(1, 2); bits.Code(48 + 17, 8); bits.Code(0, 7); bits.Align();
        byte[] expanded = [144, .. payload, 17];
        uint a = 1, b = 0;
        foreach (var value in expanded) { a += value; b += a; }
        var adler = b << 16 | a;
        byte[] encoded = [0x78, 0x01, .. bits.Bytes(), (byte)(adler >> 24), (byte)(adler >> 16), (byte)(adler >> 8), (byte)adler];
        var limits = new ImageLimits { MaxWork = 17803L + storedLength, MaxWorkingBytes = 4096 };
        for (var trailing = 0; trailing <= 9; trailing++)
            Assert.Equal(encoded.Length, ZlibFraming.Validate([.. encoded, .. Enumerable.Repeat((byte)211, trailing)], expanded.Length,
                new OperationContext(limits, default)));
        Assert.Throws<ImageLimitException>(() => ZlibFraming.Validate(encoded, expanded.Length,
            new OperationContext(limits with { MaxWork = limits.MaxWork - 1 }, default)));
        for (var length = 0; length < encoded.Length; length++)
            Assert.Throws<ImageDataException>(() => ZlibFraming.Validate(encoded.AsSpan(0, length), expanded.Length,
                new OperationContext(ImageLimits.Default, default)));
        using var inflater = new System.IO.Compression.ZLibStream(new MemoryStream(encoded), System.IO.Compression.CompressionMode.Decompress);
        var actual = new byte[expanded.Length]; inflater.ReadExactly(actual);
        Assert.Equal(expanded, actual); Assert.Equal(-1, inflater.ReadByte());
    }

    [Fact]
    public void IncompleteTreeAcceptsItsEndCodeAndRejectsTheUnassignedPrefix()
    {
        // RFC 1951 dynamic block: only EOB has a one-bit code (zero), and
        // distances are empty. Prefix one is explicitly unassigned.
        foreach (var unassigned in new[] { false, true })
        {
            var bits = new BitWriter();
            bits.Number(1, 1); bits.Number(2, 2);
            bits.Number(0, 5); bits.Number(0, 5); bits.Number(14, 4);
            ReadOnlySpan<int> order = [16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1];
            foreach (var symbol in order) bits.Number(symbol is 0 or 1 ? 1 : 0, 3);
            for (var symbol = 0; symbol < 257; symbol++) bits.Code(symbol == 256 ? 1 : 0, 1);
            bits.Code(0, 1); // Empty distance alphabet.
            bits.Code(unassigned ? 1 : 0, 1); bits.Align();
            byte[] encoded = [0x78, 0x01, .. bits.Bytes(), 0, 0, 0, 1];
            if (unassigned)
                Assert.Throws<ImageDataException>(() => ZlibFraming.Validate(encoded, 0, new OperationContext(ImageLimits.Default, default)));
            else
            {
                Assert.Equal(encoded.Length, ZlibFraming.Validate(encoded, 0, new OperationContext(ImageLimits.Default, default)));
                using var inflater = new System.IO.Compression.ZLibStream(new MemoryStream(encoded), System.IO.Compression.CompressionMode.Decompress);
                Assert.Equal(-1, inflater.ReadByte());
            }
        }
    }

    [Fact]
    public void FramingChecksCancellationOnAnAlreadyCreatedContext()
    {
        using var cancellation = new CancellationTokenSource();
        var context = new OperationContext(ImageLimits.Default, cancellation.Token);
        cancellation.Cancel();
        byte[] emptyFixedBlock = [0x78, 0x01, 3, 0, 0, 0, 0, 1];
        Assert.Throws<OperationCanceledException>(() => ZlibFraming.Validate(emptyFixedBlock, 0, context));
    }

    private sealed class BitWriter
    {
        private readonly List<byte> bytes = [];
        private int used;
        public void Number(int value, int count) { for (var i = 0; i < count; i++) Bit((value >> i) & 1); }
        public void Code(int value, int count) { for (var i = count - 1; i >= 0; i--) Bit((value >> i) & 1); }
        public void Align() { while ((used & 7) != 0) Bit(0); }
        private void Bit(int value)
        {
            if ((used & 7) == 0) bytes.Add(0);
            bytes[^1] |= (byte)(value << (used++ & 7));
        }
        public byte[] Bytes() => bytes.ToArray();
    }
}
