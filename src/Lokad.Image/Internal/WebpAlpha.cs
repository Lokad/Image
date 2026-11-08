using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

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
        {
            var horizontal = filter == 1;
            alpha = new WebpLosslessDecoder(payload[1..], width, height, context, header: false).DecodeGreen(ref result, horizontal);
            if (horizontal) return alpha;
        }
        Unfilter(alpha is null ? result!.Pixels[3..] : alpha, width, height, filter, context, alpha is null ? 4 : 1);
        return alpha;
    }

    // RFC 9649 horizontal inversion while the final VP8L greens are published.
    // An empty compact destination selects the already owned RGBA frame in pixels.
    // Every source word is read before its in-place alpha publication replaces it.
    internal static void PublishHorizontalGreen(Span<uint> pixels, Span<byte> compact,
        int width, int height, OperationContext context)
    {
        var inPlace = compact.IsEmpty;
        byte firstAbove = 0;
        for (var y = 0; y < height; y++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var row = pixels.Slice(y * width, width);
            var destination = inPlace ? default : compact.Slice(y * width, width);
            var first = unchecked((byte)((byte)(row[0] >> 8) + firstAbove));
            var previous = firstAbove; var x = 0;
            if (Sse2.IsSupported)
                for (; x <= width - 4; x += 4)
                {
                    var values = Sse2.ShiftRightLogical(MemoryMarshal.Read<Vector128<uint>>(MemoryMarshal.AsBytes(row.Slice(x, 4))), 8).AsByte();
                    // Four independent byte-lane additions form green prefixes;
                    // ignored R/B/A lanes never carry into the low green byte.
                    values = Sse2.Add(values, Sse2.ShiftLeftLogical128BitLane(values, 4));
                    values = Sse2.Add(values, Sse2.ShiftLeftLogical128BitLane(values, 8));
                    values = Sse2.Add(values, Vector128.Create((uint)previous).AsByte());
                    previous = (byte)values.AsUInt32().GetElement(3);
                    if (inPlace)
                    {
                        var alpha = Sse2.ShiftLeftLogical(values.AsUInt32(), 24);
                        MemoryMarshal.Write(MemoryMarshal.AsBytes(row.Slice(x, 4)), in alpha);
                    }
                    else
                    {
                        var samples = Sse2.And(values.AsUInt32(), Vector128.Create(255u)).AsInt32();
                        var words = Sse2.PackSignedSaturate(samples, Vector128<int>.Zero);
                        var packed = Sse2.PackUnsignedSaturate(words, Vector128<short>.Zero).AsUInt32().GetElement(0);
                        BinaryPrimitives.WriteUInt32LittleEndian(destination.Slice(x, 4), packed);
                    }
                }
            for (; x < width; x++)
            {
                previous = unchecked((byte)(previous + (byte)(row[x] >> 8)));
                if (inPlace) row[x] = (uint)previous << 24;
                else destination[x] = previous;
            }
            firstAbove = first;
            // Extraction and inverse filtering previously each polled per row.
            // Keep two bounded checkpoints while retiring the extra frame pass.
            context.Cancellation.ThrowIfCancellationRequested();
        }
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
