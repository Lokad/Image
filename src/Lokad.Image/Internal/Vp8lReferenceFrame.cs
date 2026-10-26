using System.Buffers.Binary;

namespace Lokad.Image.Internal;

// Borrows prepared transform codes, which can be shared by coding candidates.
internal readonly record struct Vp8lFramePrefix(int Width, int Height, bool Alpha)
{
    internal bool SubtractGreen { get; init; }
    internal Vp8lImageCode? Palette { get; init; }
    internal int PaletteEntries { get; init; }
    internal Vp8lImageCode? Prediction { get; init; }
    internal int PredictionBits { get; init; }
    internal Vp8lImageCode? Color { get; init; }
    internal int ColorBits { get; init; }

    internal void Write(WebpBitWriter writer, bool includeHeader = true)
    {
        if (Width is < 1 or > 16384 || Height is < 1 or > 16384) throw new ArgumentOutOfRangeException(nameof(Width));
        if (Palette is not null && PaletteEntries is < 1 or > 256) throw new ArgumentOutOfRangeException(nameof(PaletteEntries));
        if (Prediction is not null && PredictionBits is < 2 or > 9) throw new ArgumentOutOfRangeException(nameof(PredictionBits));
        if (Color is not null && ColorBits is < 2 or > 9) throw new ArgumentOutOfRangeException(nameof(ColorBits));
        if (includeHeader)
        {
            writer.Write(0x2f, 8); writer.Write(Width - 1, 14); writer.Write(Height - 1, 14);
            writer.Write(Alpha ? 1 : 0, 1); writer.Write(0, 3);
        }
        if (SubtractGreen) { writer.Write(1, 1); writer.Write(2, 2); }
        if (Palette is not null) { writer.Write(1, 1); writer.Write(3, 2); writer.Write(PaletteEntries - 1, 8); Palette.Write(writer); }
        if (Prediction is not null) { writer.Write(1, 1); writer.Write(0, 2); writer.Write(PredictionBits - 2, 3); Prediction.Write(writer); }
        if (Color is not null) { writer.Write(1, 1); writer.Write(1, 2); writer.Write(ColorBits - 2, 3); Color.Write(writer); }
    }
}

internal static class Vp8lReferenceFrame
{
    // Counting prepared state allocates no encoded-frame buffer and repeats no
    // normalization, transforms, matching or parsing. Compare this payload size
    // before RIFF padding; native candidate ties retain the earlier candidate.
    internal static int CountPayload(Vp8lFramePrefix prefix, Vp8lImageCode image, OperationContext context)
    {
        using var count = new WebpLosslessEncoder.CountStream(int.MaxValue);
        WritePayload(count, prefix, image, context); return count.Count;
    }

    internal static void WritePayload(Stream output, Vp8lFramePrefix prefix, Vp8lImageCode image, OperationContext context, bool includeHeader = true)
    {
        var writer = new WebpBitWriter(output, context); prefix.Write(writer, includeHeader); image.Write(writer); writer.Finish();
    }

    internal static void Write(Stream output, Vp8lFramePrefix prefix, Vp8lImageCode image, int payloadBytes, OperationContext context)
    {
        var length = payloadBytes + 20L + (payloadBytes & 1);
        if (payloadBytes < 0 || length > context.Limits.MaxOutputBytes)
            throw new ImageLimitException(ImageLimitKind.OutputBytes, "WebP encoded output limit exceeded.");
        context.Cancellation.ThrowIfCancellationRequested(); Span<byte> header = stackalloc byte[20];
        "RIFF"u8.CopyTo(header); BinaryPrimitives.WriteUInt32LittleEndian(header[4..], (uint)(length - 8));
        "WEBPVP8L"u8.CopyTo(header[8..]); BinaryPrimitives.WriteInt32LittleEndian(header[16..], payloadBytes);
        output.Write(header); WritePayload(output, prefix, image, context);
        if ((payloadBytes & 1) != 0) { context.Cancellation.ThrowIfCancellationRequested(); output.WriteByte(0); }
    }
}
