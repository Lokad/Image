using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class Crc32Tests
{
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
