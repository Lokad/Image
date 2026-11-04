using System.Runtime.CompilerServices;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegGrayRowTests
{
    [Fact]
    public void AllRgbTriplesPreserveSeparateBinary64Rounding()
    {
        var pixels = new int[256]; var output = new byte[256];
        for (var r = 0; r < 256; r++) for (var g = 0; g < 256; g++)
        {
            for (var b = 0; b < 256; b++) pixels[b] = r | g << 8 | b << 16 | unchecked((int)0xA5000000);
            JpegGrayPreparation.GrayRow(pixels, output);
            for (var b = 0; b < 256; b++)
            {
                var expected = Original(r, g, b);
                if (expected != output[b]) Assert.Fail($"RGB({r},{g},{b}): expected {expected}, got {output[b]}.");
            }
        }
    }

    [Fact]
    public void UnalignedRowsTailsAndOutputGuardsStayExact()
    {
        var random = new Random(269);
        foreach (var length in new[] { 0, 1, 3, 4, 7, 8, 9, 15, 16, 17, 31 })
        {
            var input = new int[length + 2]; var output = Enumerable.Repeat((byte)0xD7, length + 4).ToArray();
            for (var at = 0; at < input.Length; at++) input[at] = random.Next() ^ int.MinValue;
            var original = input.ToArray(); JpegGrayPreparation.GrayRow(input.AsSpan(1, length), output.AsSpan(1, length + 2));
            for (var at = 0; at < length; at++)
            {
                var rgb = input[at + 1]; Assert.Equal(Original(rgb & 255, rgb >> 8 & 255, rgb >> 16 & 255), output[at + 1]);
            }
            Assert.Equal(original, input); Assert.Equal(0xD7, output[0]);
            Assert.All(output.AsSpan(length + 1).ToArray(), value => Assert.Equal(0xD7, value));
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static byte Original(int r, int g, int b)
        => (byte)Math.Floor(((0.299 * r + 0.587 * g) + 0.114 * b) + 0.5);
}
