using Lokad.Image.Internal;

namespace Lokad.Image;

/// <summary>Still-image inspection and codecs. Supplied streams remain open.</summary>
public static partial class ImageCodec
{
    public static ImageInfo Identify(ReadOnlySpan<byte> input, ImageLimits? limits = null,
        CancellationToken cancellationToken = default)
    {
        using var context = InputContext(input.Length, limits, cancellationToken);
        return IdentifyCore(input, context);
    }

    private static ImageInfo IdentifyCore(ReadOnlySpan<byte> input, OperationContext context)
    {
        if (input.StartsWith(PngCodec.Signature)) return PngCodec.Identify(input, context);
        if (input.StartsWith(new byte[] { 255, 216 })) return JpegIdentifier.Identify(input, context);
        if (input.StartsWith("RIFF"u8)) return ContainerHeaders.WebP(input, context);
        throw new UnsupportedImageException(UnsupportedImageFeature.Format, "Expected a still PNG, JPEG or WebP image.");
    }

    public static ImageBuffer Decode(ReadOnlySpan<byte> input, DecodeOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new DecodeOptions();
        using var context = InputContext(input.Length, options.Limits, cancellationToken);
        return DecodeCore(input, options, context);
    }

    private static ImageBuffer DecodeCore(ReadOnlySpan<byte> input, DecodeOptions options, OperationContext context)
    {
        ImageBuffer result;
        if (input.StartsWith(PngCodec.Signature)) result = PngCodec.Decode(input, context);
        else if (input.StartsWith(new byte[] { 255, 216 })) result = new JpegDecoder(input, context).Decode();
        else if (input.StartsWith("RIFF"u8)) result = WebpCodec.Decode(input, context);
        else
        {
            var info = IdentifyCore(input, context);
            throw new UnsupportedImageException(UnsupportedImageFeature.PixelDecoding, $"{info.Format} pixel decoding is not implemented yet.", info.Format);
        }
        if (!options.ApplyOrientation || result.Orientation == ImageOrientation.Normal) return result;
        try
        {
            // Orientation is part of this decode's live-memory admission.
            context.ReserveBorrowed(result.ByteLength);
            context.Cancellation.ThrowIfCancellationRequested();
            return ImageTransforms.AutoOrient(result, options.Limits, context.Cancellation);
        }
        finally { result.Dispose(); }
    }

    public static ImageInfo Identify(Stream input, ImageLimits? limits = null, CancellationToken cancellationToken = default)
    {
        limits ??= ImageLimits.Default;
        using var context = new OperationContext(limits, cancellationToken);
        using var data = IdentificationInput.Read(input, context);
        return IdentifyCore(data.Span, context);
    }

    public static ImageBuffer Decode(Stream input, DecodeOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new DecodeOptions();
        using var context = new OperationContext(options.Limits, cancellationToken);
        return DecodeFromStream(input, options, context);
    }

    public static void EncodePng(ImageBuffer image, Stream output, PngEncodeOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(image); ArgumentNullException.ThrowIfNull(output);
        options ??= new PngEncodeOptions();
        using var context = new OperationContext(options.Limits, cancellationToken);
        using var bounded = new LimitedWriteStream(output, options.Limits.MaxOutputBytes, cancellationToken);
        PngCodec.Encode(image, bounded, options, context);
    }

    private static OperationContext InputContext(int length, ImageLimits? limits, CancellationToken cancellation)
    {
        var context = new OperationContext(limits ?? ImageLimits.Default, cancellation);
        if (length > context.Limits.MaxInputBytes) throw new ImageLimitException(ImageLimitKind.InputBytes, "Encoded input limit exceeded.");
        context.ReserveBorrowed(length);
        return context;
    }

    public static void EncodeJpeg(ImageBuffer image, Stream output, JpegEncodeOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(image); ArgumentNullException.ThrowIfNull(output);
        options ??= new JpegEncodeOptions();
        using var context = new OperationContext(options.Limits, cancellationToken);
        using var bounded = new LimitedWriteStream(output, options.Limits.MaxOutputBytes, cancellationToken);
        JpegEncoder.Encode(image, bounded, options, context);
    }

    public static void EncodeWebp(ImageBuffer image, Stream output, WebpEncodeOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(image); ArgumentNullException.ThrowIfNull(output);
        options ??= new WebpEncodeOptions();
        using var context = new OperationContext(options.Limits, cancellationToken);
        using var bounded = new LimitedWriteStream(output, options.Limits.MaxOutputBytes, cancellationToken);
        if (options.Lossless) WebpLosslessEncoder.Encode(image, bounded, options, context);
        else Vp8Encoder.Encode(image, bounded, options, context);
    }

    private static ImageBuffer DecodeFromStream(Stream input, DecodeOptions options, OperationContext context)
    {
        using var data = IdentificationInput.ReadForDecode(input, context);
        try { return DecodeCore(data.Span, options, context); }
        finally { context.Release(data.Capacity); }
    }
}
