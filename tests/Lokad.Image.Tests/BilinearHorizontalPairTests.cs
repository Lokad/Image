using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class BilinearHorizontalPairTests
{
    [Theory]
    [InlineData(9, 4)] [InlineData(19, 8)] [InlineData(37, 16)] [InlineData(513, 256)]
    public void PreparedWindowsPreserveLiveOffsetsAndKeepClippedOrUnequalTapsOnTheirOriginalPath(int sourceWidth, int width)
    {
        var eligible = 0; var fallback = 0;
        var scale = (double)sourceWidth / width;
        for (var tile = 0; tile < width; tile += 6)
        {
            var length = Math.Min(6, width - tile);
            var counts = new int[length]; var offsets = Enumerable.Repeat(-999, 60).ToArray();
            for (var x = 0; x < length; x++)
            {
                var center = (tile + x + .5) * sourceWidth / width - .5;
                var first = (int)Math.Ceiling(center - scale);
                counts[x] = (int)Math.Floor(center + scale) - first + 1;
                Assert.InRange(counts[x], 2, 6);
                for (var t = 0; t < counts[x]; t++) offsets[x * 10 + t] = Math.Clamp(first + t, 0, sourceWidth - 1);
            }
            var sourceFirst = offsets[0];
            for (var x = 0; x < length; x++)
                for (var t = 0; t < counts[x]; t++) offsets[x * 10 + t] -= sourceFirst;
            var saved = offsets.ToArray(); var savedCounts = counts.ToArray();
            BilinearHorizontalPairs.Prepare(counts, offsets);
            Assert.Equal(savedCounts, counts);
            for (var x = 0; x < length; x++)
                Assert.Equal(saved.AsSpan(x * 10, counts[x]).ToArray(), offsets.AsSpan(x * 10, counts[x]).ToArray());
            for (var x = 0; x + 1 < length; x += 2)
            {
                var first = saved[x * 10];
                var expected = counts[x] == 4 && counts[x + 1] == 4
                    && saved.AsSpan(x * 10, 4).ToArray().SequenceEqual(Enumerable.Range(first, 4))
                    && saved.AsSpan((x + 1) * 10, 4).ToArray().SequenceEqual(Enumerable.Range(first + 2, 4));
                Assert.Equal(expected ? first : -1, offsets[x * 10 + 9]);
                if (expected) eligible++; else fallback++;
            }
            for (var i = 0; i < offsets.Length; i++)
                if (!(i % 20 == 9 && i / 10 + 1 < length)) Assert.Equal(saved[i], offsets[i]);
        }
        if (sourceWidth == 513) { Assert.Equal(124, eligible); Assert.Equal(4, fallback); }
        else Assert.True(fallback > 0);
    }
}
