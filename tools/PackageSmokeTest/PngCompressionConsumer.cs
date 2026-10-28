namespace Lokad.Image.Tooling;

internal static class PngCompressionConsumer
{
    internal static void Verify()
    {
        using var source = OriginalPngGradient.Create();
        using var flipped = ImageTransforms.FlipHorizontal(source); using var output = new MemoryStream();
        ImageCodec.EncodePng(flipped, output, new() { Limits = ImageLimits.Default with { MaxOutputBytes = 2 * 1024 * 1024 } });
        if (output.Length >= 2 * 1024 * 1024 || !output.CanWrite)
            throw new InvalidOperationException("Packaged predictable PNG compression or stream ownership differs.");
        IndependentRgbaPng.Verify(output.ToArray(), flipped);
        Console.WriteLine("Packaged predictable RGBA16 PNG passed independent lossless decoding below 2 MiB.");
    }
}
