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

    public static byte[] Decode(ReadOnlySpan<byte> payload, int width, int height, OperationContext context)
    {
        if (payload.Length == 0) throw new ImageDataException("Missing WebP alpha header.");
        var compression = payload[0] & 3; var filter = payload[0] >> 2 & 3;
        var preprocessing = payload[0] >> 4 & 3;
        if (compression > 1 || preprocessing > 1) throw new UnsupportedImageException(UnsupportedImageFeature.AlphaCoding, "Unsupported WebP alpha coding.", ImageFormat.WebP);
        var count = width * height;
        context.Reserve(count + 32L); var alpha = new byte[count];
        if (compression == 0)
        {
            if (payload.Length != count + 1) throw new ImageDataException("WebP raw alpha size differs from the raster.");
            CopyRaw(payload[1..], alpha, context);
        }
        else
            new WebpLosslessDecoder(payload[1..], width, height, context, header: false).DecodeGreen(alpha);
        Unfilter(alpha, width, height, filter, context);
        return alpha;
    }

    internal static void Unfilter(Span<byte> alpha, int width, int height, int filter, OperationContext context)
    {
        if (filter == 0) return;
        for (var y = 0; y < height; y++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            for (var x = 0; x < width; x++)
            {
                var position = y * width + x; int predictor;
                if (position == 0) predictor = 0;
                else if (y == 0) predictor = alpha[position - 1];
                else if (x == 0) predictor = alpha[position - width];
                else predictor = filter switch
                {
                    1 => alpha[position - 1],
                    2 => alpha[position - width],
                    3 => Math.Clamp(alpha[position - 1] + alpha[position - width] - alpha[position - width - 1], 0, 255),
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
