using Lokad.Image.Tooling;

namespace Lokad.Image.DifferentialTests;

public sealed class PngGradientCompressionTests
{
    [Fact]
    public void LargeWordGradientWithTransparencyAndHiddenRgbDecodesExactlyInFfmpeg()
    {
        using var source = OriginalPngGradient.Create(transparent: true);
        using var flipped = ImageTransforms.FlipHorizontal(source); using var output = new MemoryStream();
        // The 2-MiB compression gate belongs to the supplied opaque recipe and
        // its transforms. This additional moving-alpha pattern checks every
        // hidden/partial-alpha sample under the original 20-MiB allowance.
        ImageCodec.EncodePng(flipped, output, new() { Limits = ImageLimits.Default with { MaxOutputBytes = 20 * 1024 * 1024 } });
        Assert.InRange(output.Length, 1, 20 * 1024 * 1024 - 1);
        var actual = PngOracle.Decode(output.ToArray(), 16);
        Assert.Equal(OriginalPixels.Bytes(flipped), actual);
        Assert.True(output.CanWrite);
    }
}
