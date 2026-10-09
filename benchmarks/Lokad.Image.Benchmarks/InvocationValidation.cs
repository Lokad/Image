using System.Buffers.Binary;
using Lokad.Image;
using Lokad.Image.Tooling;

// Development-only checks for complete codec/edit invocations. Never timed.
internal static class InvocationValidation
{
    public const string FlipMethod = "Untimed validation of one encoded still file to one flipped PNG file, with one process and equivalent file input/output per subject. Native conversion and single-thread settings are explicit in the recorded arguments. Both PNG outputs are decoded independently by FFmpeg and the public API. Managed encoded output must equal the complete requested public decode/flip/PNG-encode workflow. Linux kernel child-memory samples run separately and must preserve each validated encoded output hash. No timing measurement or isolated codec-throughput claim.";

    public static byte[] Flip(byte[] source, int width, int height, int depth)
    {
        var pixelBytes = 4 * (depth / 8);
        if (source.Length != checked(width * height * pixelBytes)) throw new InvalidOperationException("Flip reference dimensions differ.");
        var result = new byte[source.Length];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
            source.AsSpan((y * width + x) * pixelBytes, pixelBytes)
                .CopyTo(result.AsSpan((y * width + width - 1 - x) * pixelBytes, pixelBytes));
        return result;
    }

    public static void RequirePngWorkflow(JpegBenchmarkCase state, byte[] expectedPixels, int width, int height, int depth)
    {
        using var decoded = ImageCodec.Decode(File.ReadAllBytes(state.Input));
        using var flipped = ImageTransforms.FlipHorizontal(decoded);
        if ((flipped.Width, flipped.Height, flipped.BitDepth) != (width, height, depth)
            || !OriginalPixels.Bytes(flipped).AsSpan().SequenceEqual(expectedPixels))
            throw new InvalidOperationException("Public flip differs from independent pixel placement.");
        using var output = new MemoryStream(); ImageCodec.EncodePng(flipped, output);
        if (!output.ToArray().AsSpan().SequenceEqual(File.ReadAllBytes(state.ManagedOutput)))
            throw new InvalidOperationException("ImageProbe PNG differs from the complete public API workflow.");
    }

    public static object Png(JpegBenchmarkCase state, string file, int width, int height, int depth,
        byte[] reference, bool allowRgbDifference = false)
    {
        var encoded = File.ReadAllBytes(file); var info = ImageCodec.Identify(encoded);
        using var decoded = ImageCodec.Decode(encoded); var managed = OriginalPixels.Bytes(decoded);
        var native = OracleProcess.Run(state.Ffmpeg, PngOracle.DecodeArguments(depth, encoded), encoded);
        if ((info.Format, info.Width, info.Height, info.BitDepth) != (ImageFormat.Png, width, height, depth)
            || (decoded.Width, decoded.Height, decoded.BitDepth) != (width, height, depth)
            || !native.AsSpan().SequenceEqual(managed))
            throw new InvalidOperationException("PNG invocation output geometry, precision or decoder samples differ.");
        object? difference = null;
        if (allowRgbDifference)
        {
            if (depth != 8) throw new InvalidOperationException("Lossy invocation references require RGBA8.");
            var agreement = Compare(reference, native); RequireAgreement(agreement);
            difference = JpegBenchmarkCase.Metrics(agreement);
        }
        else if (!native.AsSpan().SequenceEqual(reference))
            throw new InvalidOperationException("PNG invocation pixels differ from the exact reference.");
        return new { outputBytes = encoded.Length, outputSha256 = JpegBenchmarkCase.Hash(encoded),
            decodedWidth = decoded.Width, decodedHeight = decoded.Height, decodedDepth = decoded.BitDepth,
            nativeDecodedSha256 = JpegBenchmarkCase.Hash(native), managedDecodedSha256 = JpegBenchmarkCase.Hash(managed),
            referenceSha256 = JpegBenchmarkCase.Hash(reference), alphaExact = true, exact = !allowRgbDifference,
            pixelDifference = difference };
    }

    public static object Vp8(JpegBenchmarkCase state, string file, int width, int height, byte[] source, bool alpha)
    {
        var encoded = File.ReadAllBytes(file); var info = ImageCodec.Identify(encoded);
        RequireVp8(encoded);
        var native = WebpOracle.Decode(encoded, width, height, alpha, state.Ffmpeg);
        using var decoded = ImageCodec.Decode(encoded); var managed = OriginalPixels.Bytes(decoded);
        if ((info.Format, info.Width, info.Height, info.BitDepth, info.HasAlpha) != (ImageFormat.WebP, width, height, 8, alpha)
            || (decoded.Width, decoded.Height, decoded.BitDepth) != (width, height, 8))
            throw new InvalidOperationException("VP8 invocation output geometry, precision or alpha profile differs.");
        var agreement = Compare(native, managed); RequireAgreement(agreement);
        var quality = Compare(source, native); // Also proves exact source alpha.
        return new { outputBytes = encoded.Length, outputSha256 = JpegBenchmarkCase.Hash(encoded),
            decodedWidth = decoded.Width, decodedHeight = decoded.Height, decodedDepth = decoded.BitDepth,
            codec = "VP8", alphaExact = true, nativeDecodedSha256 = JpegBenchmarkCase.Hash(native),
            managedDecodedSha256 = JpegBenchmarkCase.Hash(managed), sourceSha256 = JpegBenchmarkCase.Hash(source),
            sourceError = JpegBenchmarkCase.Metrics(quality), decoderDifference = JpegBenchmarkCase.Metrics(agreement) };
    }

    public static int LosslessInput(byte[] source, byte[] decoded)
    {
        if (source.Length != decoded.Length || source.Length % 4 != 0) throw new InvalidOperationException("Lossless invocation source sizes differ.");
        var changedHiddenPixels = 0;
        for (var offset = 0; offset < source.Length; offset += 4)
        {
            if (source[offset + 3] != decoded[offset + 3]) throw new InvalidOperationException("Lossless input alpha differs.");
            if (source.AsSpan(offset, 3).SequenceEqual(decoded.AsSpan(offset, 3))) continue;
            if (source[offset + 3] != 0) throw new InvalidOperationException("Lossless input visible RGB differs.");
            changedHiddenPixels++;
        }
        return changedHiddenPixels;
    }

    private static (int Maximum, double Rmse, double? Psnr) Compare(byte[] reference, byte[] actual)
    {
        if (reference.Length != actual.Length || actual.Length == 0 || actual.Length % 4 != 0)
            throw new InvalidOperationException("Invocation reference raster sizes differ.");
        double squares = 0; var maximum = 0;
        for (var index = 0; index < actual.Length; index++)
        {
            if (index % 4 == 3)
            {
                if (reference[index] != actual[index]) throw new InvalidOperationException("Invocation alpha differs.");
                continue;
            }
            var error = Math.Abs(reference[index] - actual[index]); maximum = Math.Max(maximum, error); squares += error * error;
        }
        var rmse = Math.Sqrt(squares / (actual.Length / 4 * 3));
        return (maximum, rmse, rmse == 0 ? null : 20 * Math.Log10(255 / rmse));
    }

    private static void RequireAgreement((int Maximum, double Rmse, double? Psnr) difference)
    {
        if (difference.Maximum > 4 || difference.Rmse > 1.5)
            throw new InvalidOperationException($"Invocation RGB disagreement: maximum {difference.Maximum}, RMSE {difference.Rmse}.");
    }

    private static void RequireVp8(ReadOnlySpan<byte> encoded)
    {
        var found = false;
        for (var offset = 12; offset < encoded.Length;)
        {
            if (encoded.Length - offset < 8) throw new InvalidOperationException("Truncated WebP output chunk.");
            var size = BinaryPrimitives.ReadUInt32LittleEndian(encoded[(offset + 4)..]);
            var total = 8L + size + (size & 1);
            if (total > encoded.Length - offset) throw new InvalidOperationException("Invalid WebP output chunk length.");
            if (encoded.Slice(offset, 4).SequenceEqual("VP8L"u8)) throw new InvalidOperationException("Lossy invocation emitted VP8L.");
            if (encoded.Slice(offset, 4).SequenceEqual("VP8 "u8)) found = true;
            offset += (int)total;
        }
        if (!found) throw new InvalidOperationException("Lossy invocation emitted no VP8 image.");
    }
}
