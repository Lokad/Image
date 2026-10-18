using Lokad.Image.Internal;

namespace Lokad.Image;

public static partial class ImageCodec
{
    /// <summary>Decode JPEG into caller-owned 8-bit grayscale or RGB samples.</summary>
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
        var context = InputContext(input.Length, options.Limits, cancellationToken);
        if (!Enum.IsDefined(format)) throw new ArgumentOutOfRangeException(nameof(format));
        if (!Enum.IsDefined(options.SourceColorSpace)) throw new ArgumentOutOfRangeException(nameof(options));
        ArgumentOutOfRangeException.ThrowIfNegative(stride);
        if (input.Overlaps(destination)) throw new ArgumentException("JPEG input and destination must not overlap.", nameof(destination));
        if (input.Length < 2 || input[0] != 255 || input[1] != 216)
            throw new UnsupportedImageException(UnsupportedImageFeature.Format, "Expected a JPEG image.");
        // The caller-supplied span counts toward the same live-memory allowance
        // as encoded input and codec workspace, including any supplied padding.
        context.Reserve(destination.Length);
        if (budget is null)
            return new JpegDecoder(input, context).DecodeSamples(destination, format, stride, options);
        var admission = budget.Begin(context.Limits);
        try { return new JpegDecoder(input, context).DecodeSamples(destination, format, stride, options); }
        finally { admission.Complete(context.ConsumedWork); }
    }
}
