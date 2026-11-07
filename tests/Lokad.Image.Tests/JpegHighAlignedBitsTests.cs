using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegHighAlignedBitsTests
{
    [Fact]
    public void EveryBaselineAmplitudeIgnoresBitsFollowingItsField()
    {
        for (var count = 1; count <= 11; count++)
        for (var field = 0; field < 1 << count; field++)
        {
            var expected = field < 1 << (count - 1) ? field + 1 - (1 << count) : field;
            var cache = (ulong)field << (64 - count);
            Assert.Equal(expected, JpegEntropyReader.ReadHighAlignedSigned(cache, count));
            Assert.Equal(expected, JpegEntropyReader.ReadHighAlignedSigned(cache | (ulong.MaxValue >> count), count));
        }
    }
}
