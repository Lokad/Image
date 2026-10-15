using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class RetainedFlipTests
{
    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void ReleasedPairsAreOverwrittenAndLiveOwnersKeepExactAdmission(int depth)
    {
        using var source = OriginalPixels.Buffer(513, 259, depth);
        if (depth == 8) source.GetRow8(0)[3] = 0; else source.GetRow16(0)[3] = 0;
        var original = OriginalPixels.Bytes(source);
        var expected = OriginalExif.Permute(original, 513, 259, depth, ImageOrientation.MirrorHorizontal);
        using (var first = ImageTransforms.FlipHorizontal(source))
        using (var second = ImageTransforms.FlipHorizontal(source))
        {
            for (var y = 0; y < source.Height; y++)
            {
                if (depth == 8) { first.GetRow8(y).Fill(0xA5); second.GetRow8(y).Fill(0x5A); }
                else { first.GetRow16(y).Fill(0xA5A5); second.GetRow16(y).Fill(0x5A5A); }
            }
        }
        var limits = ImageLimits.Default with { MaxWorkingBytes = original.Length * 2L };
        using var held = ImageTransforms.FlipHorizontal(source, limits);
        Assert.Throws<ImageLimitException>(() => ImageTransforms.FlipHorizontal(source,
            limits with { MaxWorkingBytes = limits.MaxWorkingBytes - 1 }));
        using var next = ImageTransforms.FlipHorizontal(source, limits);
        Assert.Equal(expected, OriginalPixels.Bytes(held)); Assert.Equal(expected, OriginalPixels.Bytes(next));
        if (depth == 8) next.GetRow8(0)[0] ^= 255; else next.GetRow16(0)[0] ^= 65535;
        Assert.Equal(expected, OriginalPixels.Bytes(held)); Assert.Equal(original, OriginalPixels.Bytes(source));
    }
}
