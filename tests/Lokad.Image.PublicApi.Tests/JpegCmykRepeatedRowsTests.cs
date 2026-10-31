using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class JpegCmykRepeatedRowsTests
{
    [Theory]
    [InlineData(false, 0)] [InlineData(true, 0)]
    [InlineData(false, 1)] [InlineData(true, 1)]
    [InlineData(false, 2)] [InlineData(true, 2)]
    [InlineData(false, 3)] [InlineData(true, 3)]
    public void RepeatedRowsAndSingleComponentChangesAtRightEdgeKeepExactIndependentPixels(bool adobe, int changedComponent)
    {
        const int width = 17, height = 17;
        int Sample(int component, int bx, int by)
            => 64 + component * 32 + bx * 3 + (component == changedComponent && bx == 2 ? by * 20 : 0);
        var input = AuthoredJpegCompatibility.Create(width, height, [0, 0, 0, 0],
            adobe: adobe ? 0 : null, blockSample: Sample);
        var original = input.ToArray();
        using var first = ImageCodec.Decode(input);
        using var survivor = ImageCodec.Decode(input);
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var black = Sample(3, x / 8, y / 8); if (!adobe) black = 255 - black;
            for (var c = 0; c < 3; c++)
            {
                var color = Sample(c, x / 8, y / 8); if (!adobe) color = 255 - color;
                var expected = (byte)((long)color * black * 257 / 65536);
                Assert.Equal(expected, first.GetRow8(y)[x * 4 + c]);
                Assert.Equal(expected, survivor.GetRow8(y)[x * 4 + c]);
            }
            Assert.Equal(255, first.GetRow8(y)[x * 4 + 3]);
            Assert.Equal(255, survivor.GetRow8(y)[x * 4 + 3]);
        }
        var nextRow = first.GetRow8(1).ToArray();
        var expectedSurvivor = OriginalPixels.Bytes(survivor);
        first.GetRow8(0).Fill(173);
        Assert.Equal(nextRow, first.GetRow8(1).ToArray());
        Assert.Equal(original, input);
        first.Dispose(); Array.Clear(input);
        Assert.Equal(expectedSurvivor, OriginalPixels.Bytes(survivor));
    }
}
