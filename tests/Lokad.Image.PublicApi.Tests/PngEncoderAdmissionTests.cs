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
        // Stored RGBA uses compressor history directly. Other paths retain a
        // packed row; all keep 4 KiB minor state and their compressor allowance.
        var workspace = compression == CompressionLevel.NoCompression ? 72 * 1024 : 384 * 1024;
        var packedRow = compression == CompressionLevel.NoCompression && color == PngColorType.Rgba
            ? 0 : 9L * channels * (depth / 8);
        var peak = sourcePixels.Length + packedRow + 4096 + workspace;
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
