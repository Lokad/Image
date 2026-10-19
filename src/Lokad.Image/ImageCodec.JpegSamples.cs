using Lokad.Image.Internal;

namespace Lokad.Image;

public static partial class ImageCodec
{
    /// <summary>Encode caller-owned, interleaved 8-bit grayscale or RGB samples.</summary>
    /// <remarks>Grayscale input always produces one JPEG component. Sampling applies
    /// to RGB input. Stride zero means packed; positive stride must cover a row.
    /// The complete source span counts toward working admission. Source storage
    /// must remain stable during the call; it is never retained or modified.
    /// The output remains open and can contain partial bytes on failure.</remarks>
    public static void EncodeJpeg(ReadOnlySpan<byte> samples, int width, int height,
        JpegSampleFormat format, Stream output, int stride = 0, JpegEncodeOptions? options = null,
        CancellationToken cancellationToken = default)
        => EncodeJpegSamples(samples, width, height, format, output, stride, options, cancellationToken, null);

    internal static void EncodeJpegSamples(ReadOnlySpan<byte> samples, int width, int height,
        JpegSampleFormat format, Stream output, int stride, JpegEncodeOptions? options,
        CancellationToken cancellationToken, ImageBudget? budget)
    {
        ArgumentNullException.ThrowIfNull(output);
        options ??= new JpegEncodeOptions();
        using var context = new OperationContext(options.Limits, cancellationToken);
        if (options.Quality is < 1 or > 100 || !Enum.IsDefined(options.Sampling)
            || options.RestartInterval is < 0 or > 65535 || options.BackgroundRgb > 0xFFFFFF)
            throw new ArgumentOutOfRangeException(nameof(options));
        var source = new JpegSampleInput(samples, width, height, format, stride, context);
        context.ReserveBorrowed(samples.Length); context.Reserve(8192);
        using var bounded = new LimitedWriteStream(output, options.Limits.MaxOutputBytes, cancellationToken);
        if (budget is null) { JpegEncoder.EncodeSamples(source, bounded, options, context); return; }
        var admission = budget.Begin(context.Limits);
        try { JpegEncoder.EncodeSamples(source, bounded, options, context); }
        finally { admission.Complete(context.ConsumedWork); }
    }

    /// <summary>Decode JPEG into caller-owned 8-bit grayscale, RGB or CMYK samples.</summary>
    /// <remarks>Stride zero means tightly packed; otherwise use a positive stride
    /// at least as large as one row. Padding and unused suffix bytes are untouched.
    /// Input and destination must not overlap. Failures can leave partial pixels.
    /// Grayscale uses Y for YCbCr input and the encoded-space luma matrix for RGB.
    /// No ICC conversion is applied. Returned dimensions describe written pixels.</remarks>
    public static ImageInfo DecodeJpeg(ReadOnlySpan<byte> input, Span<byte> destination,
        JpegSampleFormat format, int stride = 0, JpegDecodeOptions? options = null,
        CancellationToken cancellationToken = default)
        => DecodeJpegSamples(input, destination, format, stride, options, cancellationToken, null);

    internal static ImageInfo DecodeJpegSamples(ReadOnlySpan<byte> input, Span<byte> destination,
        JpegSampleFormat format, int stride, JpegDecodeOptions? options,
        CancellationToken cancellationToken, ImageBudget? budget)
    {
        options ??= new JpegDecodeOptions();
        using var context = InputContext(input.Length, options.Limits, cancellationToken);
        if (!Enum.IsDefined(format)) throw new ArgumentOutOfRangeException(nameof(format));
        if (!Enum.IsDefined(options.SourceColorSpace)) throw new ArgumentOutOfRangeException(nameof(options));
        if (!Enum.IsDefined(options.CmykInversion)) throw new ArgumentOutOfRangeException(nameof(options));
        ArgumentOutOfRangeException.ThrowIfNegative(stride);
        if (input.Overlaps(destination)) throw new ArgumentException("JPEG input and destination must not overlap.", nameof(destination));
        if (input.Length < 2 || input[0] != 255 || input[1] != 216)
            throw new UnsupportedImageException(UnsupportedImageFeature.Format, "Expected a JPEG image.");
        // The caller-supplied span counts toward the same live-memory allowance
        // as encoded input and codec workspace, including any supplied padding.
        context.ReserveBorrowed(destination.Length);
        if (budget is null)
            return new JpegDecoder(input, context).DecodeSamples(destination, format, stride, options);
        var admission = budget.Begin(context.Limits);
        try { return new JpegDecoder(input, context).DecodeSamples(destination, format, stride, options); }
        finally { admission.Complete(context.ConsumedWork); }
    }
}
