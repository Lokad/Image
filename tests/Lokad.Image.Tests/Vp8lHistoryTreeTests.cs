using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class Vp8lHistoryTreeTests
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void HistoryTraversalKeepsEveryHitMinimumAndFinalPrefixCacheWithGuardedSpans(bool forward)
    {
        uint random = 0x31415926;
        foreach (var bits in Enumerable.Range(0, 11))
        foreach (var size in new[] { 0, 1, 2, 63, 64, 65, 257, 4097 })
        foreach (var pattern in Enumerable.Range(0, 6))
        {
            var pixels = Enumerable.Repeat(0xa5a5a5a5u, size + 9).ToArray();
            for (var i = 0; i < size; i++)
            {
                random ^= random << 13; random ^= random >> 17; random ^= random << 5;
                pixels[i + 3] = pattern switch
                {
                    0 => 0u, 1 => 0x13579bdfu, 2 => (uint)(i & 1) * 0x31415926u,
                    3 => (uint)(i / 7 % 11) << 8, 4 => random & 255, _ => random
                };
            }
            var original = (uint[])pixels.Clone(); var length = (1 << (bits + 1)) - 2;
            var actualStore = Enumerable.Repeat(0xdeadbeefu, length + 7).ToArray(); actualStore.AsSpan(2, length).Clear();
            var minimumStore = Enumerable.Repeat((byte)0xa5, size + 7).ToArray();
            var expected = new uint[length]; var expectedMinimum = Enumerable.Repeat((byte)(bits + 1), size).ToArray();
            // Independent forward simulation: one cache width at a time, with
            // every pixel written, including no-op hits and initial pixel zero.
            for (var width = 1; width <= bits; width++)
            {
                var cache = new uint[1 << width];
                for (var position = 0; position < size; position++)
                {
                    var pixel = pixels[position + 3]; var key = Vp8lReferenceCache.Key(pixel, width);
                    if (cache[key] == pixel) expectedMinimum[position] = Math.Min(expectedMinimum[position], (byte)width);
                    cache[key] = pixel;
                }
                cache.CopyTo(expected, (1 << width) - 2);
            }
            if (forward)
                Vp8lReferenceCache.BuildForwardHistory(pixels.AsSpan(3, size), bits, actualStore.AsSpan(2, length), minimumStore.AsSpan(2, size));
            else
                Vp8lReferenceCache.BuildHistory(pixels.AsSpan(3, size), bits, actualStore.AsSpan(2, length), minimumStore.AsSpan(2, size));
            Assert.Equal(expectedMinimum, minimumStore.AsSpan(2, size).ToArray());
            Assert.Equal(expected, actualStore.AsSpan(2, length).ToArray()); Assert.Equal(original, pixels);
            Assert.All(actualStore.Take(2).Concat(actualStore.TakeLast(5)), value => Assert.Equal(0xdeadbeefu, value));
            Assert.All(minimumStore.Take(2).Concat(minimumStore.TakeLast(5)), value => Assert.Equal((byte)0xa5, value));
        }
    }
}
