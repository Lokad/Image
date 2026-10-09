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
        // One work unit/block, all consumed bits, fixed-tree construction,
        // and one stored byte. Lookahead must never charge unused bits.
        var work = 720L + 9 * literals;
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
