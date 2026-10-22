using System.IO.Compression;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class PngEncoderAdmissionTests
{
    public static IEnumerable<object[]> Profiles()
    {
        foreach (var depth in new[] { 8, 16 })
        foreach (var color in Enum.GetValues<PngColorType>())
        foreach (var compression in Enum.GetValues<CompressionLevel>()) yield return [depth, color, compression];
    }

    [Theory]
    [MemberData(nameof(Profiles))]
    public void WorkspaceIsAdmittedBeforeWritingAndTheBoundaryRoundTrips(int depth, PngColorType color, CompressionLevel compression)
    {
        using var source = OriginalPixels.Buffer(9, 2, depth,
            grayscale: color is PngColorType.Grayscale or PngColorType.GrayscaleAlpha,
            opaque: color is PngColorType.Grayscale or PngColorType.Rgb);
        var sourcePixels = OriginalPixels.Bytes(source);
        var channels = color switch { PngColorType.Grayscale => 1, PngColorType.Rgb => 3, PngColorType.GrayscaleAlpha => 2, _ => 4 };
        // Public admission policy: supplied pixels + one packed scanline + 64 KiB
        // IDAT transport buffer + 1 MiB managed compressor workspace. The inflater
        // has a separate 256 KiB allowance; this test only constrains encoding.
        var peak = sourcePixels.Length + 9L * channels * (depth / 8) + 65536 + 1024 * 1024;
        var options = new PngEncodeOptions { ColorType = color, Compression = compression, Limits = new ImageLimits { MaxWorkingBytes = peak } };
        using var rejected = new MemoryStream();
        Assert.Throws<ImageLimitException>(() => ImageCodec.EncodePng(source, rejected, options with { Limits = options.Limits with { MaxWorkingBytes = peak - 1 } }));
        Assert.Equal(0, rejected.Length); Assert.True(rejected.CanWrite);
        using var encoded = new MemoryStream(); ImageCodec.EncodePng(source, encoded, options);
        Assert.True(encoded.CanWrite);
        using var decoded = ImageCodec.Decode(encoded.ToArray());
        Assert.Equal((9, 2, depth), (decoded.Width, decoded.Height, decoded.BitDepth));
        Assert.Equal(sourcePixels, OriginalPixels.Bytes(decoded));
        Assert.Equal(sourcePixels, OriginalPixels.Bytes(source));
        using var unrestricted = new MemoryStream();
        ImageCodec.EncodePng(source, unrestricted, options with { Limits = ImageLimits.Default });
        Assert.Equal(unrestricted.ToArray(), encoded.ToArray());
    }
}
