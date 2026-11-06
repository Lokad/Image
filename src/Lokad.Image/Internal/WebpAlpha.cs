namespace Lokad.Image.Internal;

/// <summary>Owned RFC 9649 section 2.7.1.2 alpha decoding and inverse filtering.</summary>
internal static class WebpAlpha
{
    internal static void ValidatePixels(ReadOnlySpan<byte> payload, int width, int height, OperationContext context)
    {
        if (payload.Length == 0) throw new ImageDataException("Missing WebP alpha header.");
        var compression = payload[0] & 3; var preprocessing = payload[0] >> 4 & 3;
        if (compression > 1 || preprocessing > 1) throw new UnsupportedImageException(UnsupportedImageFeature.AlphaCoding, "Unsupported WebP alpha coding.", ImageFormat.WebP);
        if (compression == 0)
        {
            if (payload.Length != (long)width * height + 1) throw new ImageDataException("WebP raw alpha size differs from the raster.");
        }
        else new WebpLosslessDecoder(payload[1..], width, height, context, header: false).ValidatePixels();
        // All two-bit filter values are defined; inverse filtering is total byte
        // arithmetic and cannot make successfully decoded residues invalid.
        context.Cancellation.ThrowIfCancellationRequested();
    }

    public static byte[]? Decode(ReadOnlySpan<byte> payload, int width, int height, OperationContext context,
        scoped ref ImageBuffer? result)
    {
        if (payload.Length == 0) throw new ImageDataException("Missing WebP alpha header.");
        var compression = payload[0] & 3; var filter = payload[0] >> 2 & 3;
        var preprocessing = payload[0] >> 4 & 3;
        if (compression > 1 || preprocessing > 1) throw new UnsupportedImageException(UnsupportedImageFeature.AlphaCoding, "Unsupported WebP alpha coding.", ImageFormat.WebP);
        byte[]? alpha;
        if (compression == 0)
        {
            var count = width * height;
            context.Reserve(count + 32L); alpha = new byte[count];
            if (payload.Length != count + 1) throw new ImageDataException("WebP raw alpha size differs from the raster.");
            CopyRaw(payload[1..], alpha, context);
        }
        else
            alpha = new WebpLosslessDecoder(payload[1..], width, height, context, header: false).DecodeGreen(ref result);
        Unfilter(alpha is null ? result!.Pixels[3..] : alpha, width, height, filter, context, alpha is null ? 4 : 1);
        return alpha;
    }

    internal static void Unfilter(Span<byte> alpha, int width, int height, int filter, OperationContext context,
        int sampleStride = 1)
    {
        if (filter == 0) return;
        var rowStride = width * sampleStride;
        for (var y = 0; y < height; y++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            for (var x = 0; x < width; x++)
            {
                var position = y * rowStride + x * sampleStride; int predictor;
                if (position == 0) predictor = 0;
                else if (y == 0) predictor = alpha[position - sampleStride];
                else if (x == 0) predictor = alpha[position - rowStride];
                else predictor = filter switch
                {
                    1 => alpha[position - sampleStride],
                    2 => alpha[position - rowStride],
                    3 => Math.Clamp(alpha[position - sampleStride] + alpha[position - rowStride] - alpha[position - rowStride - sampleStride], 0, 255),
                    _ => throw new ImageDataException("Invalid WebP alpha filter.")
                };
                alpha[position] = unchecked((byte)(alpha[position] + predictor));
            }
        }
    }

    internal static void CopyRaw(ReadOnlySpan<byte> source, Span<byte> destination, OperationContext context)
    {
        for (var offset = 0; offset < source.Length;)
        {
            var count = Math.Min(65536, source.Length - offset);
            context.Cancellation.ThrowIfCancellationRequested(); source.Slice(offset, count).CopyTo(destination.Slice(offset, count));
            offset += count;
        }
    }
}
