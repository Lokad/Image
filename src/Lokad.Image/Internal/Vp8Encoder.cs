namespace Lokad.Image.Internal;

// Independently implemented VP8 key-frame encoding, qualified against pinned
// complete reference packets. Preparation owns partitions rather than a joined
// encoded-frame buffer; source storage is borrowed only until preparation ends.
internal static class Vp8Encoder
{
    private sealed class Prepared(Vp8FrameEncoding packet, Vp8AlphaEncoding? alpha) : IDisposable
    {
        internal void Write(Stream output)
        {
            if (alpha is null) packet.WriteOpaqueWebp(output);
            else packet.WriteWebp(output, alpha);
        }

        internal ValueTask WriteAsync(Stream output) => alpha is null
            ? packet.WriteOpaqueWebpAsync(output) : packet.WriteWebpAsync(output, alpha);

        public void Dispose() { packet.Dispose(); alpha?.Dispose(); }
    }

    private static Prepared Prepare(ImageBuffer image, WebpEncodeOptions options, OperationContext context)
    {
        if (options.Quality is < 1 or > 100 || options.Effort is < 0 or > 6) throw new ArgumentOutOfRangeException(nameof(options));
        context.Work(0); context.PixelBytes(image.Width, image.Height, image.BitDepth);
        if (image.Width > 16383 || image.Height > 16383)
            throw new ImageLimitException(ImageLimitKind.Dimension, "VP8 dimensions exceed 16383.");
        var borrowed = image.ByteLength; context.ReserveBorrowed(borrowed);
        Vp8AlphaEncoding? alpha = null; Vp8FrameEncoding? packet = null;
        try
        {
            using var input = Vp8PreparedInput.Create(image, context);
            if (input.HasAlpha) alpha = Vp8AlphaEncoding.Create(input.Alpha, input.Width, context, options.Effort);
            input.CleanupTransparentArea();
            using var analysis = Vp8SourceAnalysis.Create(input, context, options.Effort, options.Quality);
            using var segments = Vp8SourceSegments.Create(analysis, options.Quality, false, context);
            using var quantization = Vp8QuantizationPlan.Create(segments, analysis.MeanChromaScore, context, options.Effort);
            using var frame = options.Effort <= 2
                ? Vp8FramePlan.CreateDistortion(input, analysis, segments, quantization, options.Effort, context, options.Quality <= 98)
                : Vp8FramePlan.Create(input, segments, quantization, context, correctDc: options.Quality <= 98, method: options.Effort);
            packet = Vp8FrameEncoding.Create(frame, context);
            // Validate the complete transparent container before any caller I/O.
            // Opaque size was checked when its partitions were prepared.
            if (alpha is not null) packet.WebpLengthWithAlpha(alpha);
            return new Prepared(packet, alpha);
        }
        catch { packet?.Dispose(); alpha?.Dispose(); throw; }
        finally { context.ReleaseBorrowed(borrowed); }
    }

    internal static void Encode(ImageBuffer image, Stream output, WebpEncodeOptions options, OperationContext context)
    {
        using var prepared = Prepare(image, options, context);
        prepared.Write(output);
    }

    internal static async ValueTask EncodeAsync(ImageBuffer image, Stream output, WebpEncodeOptions options, OperationContext context)
    {
        using var prepared = Prepare(image, options, context);
        // Await actual stream users before ending partition/alpha ownership.
        await prepared.WriteAsync(output).ConfigureAwait(false);
    }
}
