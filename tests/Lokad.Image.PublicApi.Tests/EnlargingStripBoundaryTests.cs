using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class EnlargingStripBoundaryTests
{
    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void StripWrapAndOddTailsMatchDirectOrderAtBothDepths(int depth)
    {
        using var source = OriginalPixels.Buffer(9, 7, depth);
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++)
        {
            // Positive high alpha isolates strip wrap from exact tiny-alpha
            // workspace, allowing a4095-byte original-order reference.
            if (depth == 8) source.GetRow8(y)[x * 4 + 3] = (byte)(192 + (x * 7 + y * 11) % 64);
            else source.GetRow16(y)[x * 4 + 3] = (ushort)(49152 + (x * 701 + y * 1103) % 16384);
        }
        var input = OriginalPixels.Bytes(source);
        foreach (var width in new[] { 13, 14, 15, 27, 28, 29 })
        foreach (var height in new[] { 7, 15 })
        {
            var bytes = input.Length + (long)width * height * (depth / 2);
            var limits = ImageLimits.Default with { MaxWorkingBytes = bytes + 4096 };
            using var expected = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic,
                limits with { MaxWorkingBytes = bytes + 4095 });
            using var first = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic, limits);
            using var second = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic, limits);
            var frozen = OriginalPixels.Bytes(first);
            Assert.Equal(OriginalPixels.Bytes(expected), frozen); Assert.Equal(frozen, OriginalPixels.Bytes(second));
            if (depth == 8) first.GetRow8(0)[0] ^= 255; else first.GetRow16(0)[0] ^= 65535;
            Assert.Equal(frozen, OriginalPixels.Bytes(second)); Assert.Equal(input, OriginalPixels.Bytes(source));
        }
    }
}
