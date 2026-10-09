using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class Adler32Tests
{
    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(15)] [InlineData(16)]
    [InlineData(31)] [InlineData(32)] [InlineData(33)] [InlineData(63)]
    [InlineData(64)] [InlineData(65)] [InlineData(4095)] [InlineData(4096)]
    [InlineData(4097)] [InlineData(8193)] [InlineData(65521)]
    public void ChunkAndVectorBoundariesAgreeWithTheRecurrenceAndPreserveSource(int length)
    {
        foreach (var maximum in new[] { false, true })
        {
            // Original bytes, with unaligned data and guards on both sides.
            var bytes = Enumerable.Range(0, length + 8).Select(i => maximum ? (byte)255 : (byte)(53 * i + 7)).ToArray();
            var original = bytes.ToArray();
            uint expectedA = 65520, expectedB = 65520;
            for (var i = 3; i < length + 3; i++)
            {
                expectedA = (expectedA + bytes[i]) % 65521;
                expectedB = (expectedB + expectedA) % 65521;
            }
            uint a = 65520, b = 65520;
            Adler32.Append(bytes.AsSpan(3, length), ref a, ref b);
            Assert.Equal(expectedA, a); Assert.Equal(expectedB, b); Assert.Equal(original, bytes);

            // Different call boundaries carry the same sums, including tails
            // that enter/leave the vector and modulo chunk paths.
            a = b = 65520;
            ReadOnlySpan<int> pieces = [1, 7, 31, 32, 33, 4095, 4096, 4097];
            int offset = 0, piece = 0;
            while (offset < length)
            {
                var count = Math.Min(pieces[piece++ % pieces.Length], length - offset);
                Adler32.Append(bytes.AsSpan(3 + offset, count), ref a, ref b); offset += count;
            }
            Assert.Equal(expectedA, a); Assert.Equal(expectedB, b); Assert.Equal(original, bytes);
        }
    }

    [Fact]
    public void CancellationPreservesStateAndSourceBeforeAnyProcessing()
    {
        var bytes = Enumerable.Range(0, 8193).Select(i => (byte)i).ToArray(); var original = bytes.ToArray();
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        uint a = 17, b = 23;
        Assert.Throws<OperationCanceledException>(() => Adler32.Append(bytes, ref a, ref b, cancellation.Token));
        Assert.Equal(17u, a); Assert.Equal(23u, b); Assert.Equal(original, bytes);
    }
}
