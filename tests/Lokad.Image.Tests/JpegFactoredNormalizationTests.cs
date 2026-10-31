using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegFactoredNormalizationTests
{
    [Fact]
    public void EveryWordAlphaPreservesOriginalRationalRoundingNearByteTransitions()
    {
        const long maximum = 65535, denominator = maximum * maximum;
        var storage = new ushort[11 * 4 + 2]; var result = new int[13];
        foreach (var background in new[] { 0, 1, 127, 128, 254, 255 })
        {
            var color = (uint)(background << 16 | (255 - background) << 8 | background);
            for (var alpha = 0; alpha <= 65535; alpha++)
            {
                var middle = (32768L * alpha * 255 + background * (maximum - alpha) * maximum + denominator / 2) / denominator;
                for (var x = 0; x < 11; x++)
                {
                    var target = middle + x / 3 - 1;
                    var threshold = target * denominator - background * (maximum - alpha) * maximum - denominator / 2;
                    var sample = alpha == 0 ? 32768 : (threshold + alpha * 255L - 1) / (alpha * 255L);
                    var value = (ushort)Math.Clamp(sample + x % 3 - 1, 0, 65535);
                    storage[1 + x * 4] = value;
                    storage[2 + x * 4] = (ushort)(65535 - value);
                    storage[3 + x * 4] = x == 9 ? (ushort)65535 : x == 10 ? (ushort)0 : value;
                    storage[4 + x * 4] = (ushort)alpha;
                }
                result[0] = result[^1] = -1;
                JpegReferenceColor.NormalizeRgba16(storage.AsSpan(1, 44), result.AsSpan(1, 11), color);
                for (var x = 0; x < 11; x++)
                {
                    var packed = 0;
                    for (var c = 0; c < 3; c++)
                    {
                        var backdrop = c == 1 ? 255 - background : background;
                        var expected = (storage[1 + x * 4 + c] * (long)alpha * 255
                            + backdrop * (maximum - alpha) * maximum + denominator / 2) / denominator;
                        packed |= (int)expected << (c * 8);
                    }
                    if (result[x + 1] != packed)
                        Assert.Fail($"Alpha {alpha}, background {background}, pixel {x}: {result[x + 1]:x} != {packed:x}.");
                }
                if (result[0] != -1 || result[^1] != -1) Assert.Fail("Output guard modified.");
            }
        }
    }
}
