using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class Crc32Tests
{
    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(7)] [InlineData(15)] [InlineData(16)] [InlineData(17)]
    [InlineData(31)] [InlineData(32)] [InlineData(47)] [InlineData(48)] [InlineData(63)] [InlineData(64)]
    [InlineData(65)] [InlineData(79)] [InlineData(80)] [InlineData(95)] [InlineData(96)] [InlineData(127)]
    [InlineData(128)] [InlineData(129)] [InlineData(191)] [InlineData(192)] [InlineData(255)] [InlineData(256)]
    [InlineData(257)] [InlineData(4095)] [InlineData(4096)] [InlineData(4097)] [InlineData(65535)]
    [InlineData(65536)] [InlineData(65540)] [InlineData(65549)]
    public void IdentificationFoldingMatchesLiteralPolynomialAcrossLanesAndUnalignedTails(int length)
    {
        foreach (var maximum in new[] { false, true })
        {
            var bytes = Enumerable.Range(0, length + 15).Select(i => maximum ? (byte)255 : (byte)(i * 53 + 7)).ToArray();
            var original = bytes.ToArray();
            Assert.Equal(Bitwise(bytes.AsSpan(7, length)), PngIdentificationCrc32.Compute(bytes.AsSpan(7, length)));
            Assert.Equal(original, bytes);
        }
    }

    [Fact]
    public void IdentificationFourLaneImpulsesMatchTheLiteralPolynomial()
    {
        var bytes = new byte[128];
        for (var value = 0; value < 256; value++)
        for (var position = 0; position < bytes.Length; position++)
        {
            bytes[position] = (byte)value;
            Assert.Equal(Bitwise(bytes), PngIdentificationCrc32.Compute(bytes));
            bytes[position] = 0;
        }
    }

    [Fact]
    public void IdentificationCrcPreservesCancellationAndTheEmptyNoWorkCase()
    {
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        Assert.Equal(0u, PngIdentificationCrc32.Compute([], cancellation.Token));
        Assert.Throws<OperationCanceledException>(() => PngIdentificationCrc32.Compute(new byte[4097], cancellation.Token));
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(7)] [InlineData(8)]
    [InlineData(9)] [InlineData(15)] [InlineData(16)] [InlineData(17)]
    [InlineData(31)] [InlineData(32)] [InlineData(33)]
    [InlineData(47)] [InlineData(48)] [InlineData(49)]
    [InlineData(63)] [InlineData(64)] [InlineData(65)] [InlineData(4095)]
    [InlineData(4096)] [InlineData(4097)] [InlineData(8193)] [InlineData(65521)]
    public void WordAndCancellationBoundariesMatchTheBitwiseRecurrenceWithoutChangingSource(int length)
    {
        foreach (var maximum in new[] { false, true })
        {
            var bytes = Enumerable.Range(0, length + 8).Select(i => maximum ? (byte)255 : (byte)(53 * i + 7)).ToArray();
            var original = bytes.ToArray(); var data = bytes.AsSpan(3, length);
            Assert.Equal(Bitwise(data), Crc32.Compute(data));
            Assert.Equal(original, bytes);
        }
    }

    [Fact]
    public void EveryByteValueAtEveryWordPositionMatchesTheBitwiseRecurrence()
    {
        // Original impulse words exercise every generated polynomial power,
        // plus the byte tail independently of the wide path.
        for (var value = 0; value < 256; value++)
        {
            byte[] single = [(byte)value];
            Assert.Equal(Bitwise(single), Crc32.Compute(single));
            for (var position = 0; position < 8; position++)
            {
                var word = new byte[8]; word[position] = (byte)value;
                Assert.Equal(Bitwise(word), Crc32.Compute(word));
            }
        }
    }

    [Fact]
    public void PreCanceledChecksumsDoNotProcessOrChangeSource()
    {
        var bytes = Enumerable.Range(0, 4097).Select(i => (byte)i).ToArray(); var original = bytes.ToArray();
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => Crc32.Compute(bytes, cancellation.Token));
        Assert.Equal(original, bytes);
    }

    [Theory]
    [InlineData(16)] [InlineData(32)] [InlineData(48)]
    public void EveryByteValueAtEveryWidePositionMatchesTheBitwiseRecurrence(int length)
    {
        for (var value = 0; value < 256; value++)
        for (var position = 0; position < length; position++)
        {
            var bytes = new byte[length + 16]; bytes[position + 7] = (byte)value;
            var original = bytes.ToArray(); var data = bytes.AsSpan(7, length);
            Assert.Equal(Bitwise(data), Crc32.Compute(data));
            Assert.Equal(original, bytes);
        }
    }

    [Fact]
    public void EmptyChecksumKeepsTheOriginalNoWorkCancellationBoundary()
    {
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        Assert.Equal(0u, Crc32.Compute([], cancellation.Token));
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(7)] [InlineData(8)]
    [InlineData(15)] [InlineData(16)] [InlineData(17)] [InlineData(31)]
    [InlineData(32)] [InlineData(33)] [InlineData(4095)] [InlineData(4096)]
    [InlineData(4097)] [InlineData(65535)] [InlineData(65536)]
    public void ChunkTypeSeedAndPayloadTailsMatchIndependentRecurrence(int length)
    {
        foreach (var type in new[] { "IHDR"u8.ToArray(), "IDAT"u8.ToArray(), "IEND"u8.ToArray(), "tEXt"u8.ToArray() })
        {
            var bytes = Enumerable.Range(0, length + 9).Select(i => (byte)(i * 53 + 11)).ToArray();
            var original = bytes.ToArray(); var payload = bytes.AsSpan(3, length);
            var joined = new byte[type.Length + payload.Length]; type.CopyTo(joined, 0); payload.CopyTo(joined.AsSpan(type.Length));
            Assert.Equal(Bitwise(joined), Crc32.ComputeChunk(type, payload));
            Assert.Equal(original, bytes);
        }
    }

    private static uint Bitwise(ReadOnlySpan<byte> bytes)
    {
        // Independent literal PNG polynomial recurrence, with no production tables.
        var crc = uint.MaxValue;
        foreach (var value in bytes)
        {
            crc ^= value;
            for (var bit = 0; bit < 8; bit++)
                crc = crc >> 1 ^ ((crc & 1) == 0 ? 0 : 0xEDB88320u);
        }
        return ~crc;
    }
}
