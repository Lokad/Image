using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8lMatchIndexBoundaryTests
{
    private static uint[] Samples(int count, int pattern)
    {
        var pixels = new uint[count]; uint state = 0x6d2b79f5;
        for (var i = 0; i < count; i++)
        {
            state ^= state << 13; state ^= state >> 17; state ^= state << 5;
            pixels[i] = pattern switch
            {
                0 => 0xff000000u | (state & 511) << 8,
                1 => i % 8193 < 4100 ? 0xabcdef01u : (uint)(i % 17) * 0x01020304u,
                _ => i % 29 < 11 ? (uint)(i % 7) * 0x1234567u : state
            };
        }
        pixels[^1] ^= 0x80000000u; return pixels;
    }

    [Theory]
    [InlineData(131070, 510, 25, false, 0)]
    [InlineData(131072, 512, 26, false, 0)]
    [InlineData(131328, 513, 50, true, 0)]
    [InlineData(131072, 16, 51, false, 1)]
    [InlineData(131072, 512, 75, false, 2)]
    [InlineData(131328, 513, 76, true, 2)]
    [InlineData(131072, 512, 100, false, 1)]
    [InlineData(131072, 8, 0, true, 0)]
    public void ReusedTokenHeadsKeepAllIndexWordsAcrossWindowsRunsAndCollisions(
        int count, int width, int quality, bool low, int pattern)
    {
        var pixels = Samples(count, pattern); var original = pixels.ToArray();
        using var root = new TestMemoryBudget(16_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var expected = Vp8lReferenceMatcher.Build(pixels, width, quality, low, context);
        using var tokens = new Vp8lTokenBuffer(context, Math.Max(count, Vp8lReferenceMatcher.HeadCount / 2));
        var retained = root.Bytes;
        using var actual = Vp8lReferenceMatcher.Build(pixels, width, quality, low, context, tokens);
        Assert.True(expected.Values.SequenceEqual(actual.Values));
        Assert.Equal(retained + count * 4L, root.Bytes);
        Assert.Equal(0, tokens.Count); Assert.Equal(0, tokens.CacheBits);
        tokens.Dispose(); actual.Dispose();
        Assert.Equal(count * 4L, root.Bytes);
        Assert.True(expected.Values.Length == count); // Independent surviving index.
        expected.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        Assert.Equal(original, pixels);
    }

}
