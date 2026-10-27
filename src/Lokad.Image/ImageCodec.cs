using Lokad.Image.Internal;

namespace Lokad.Image;

/// <summary>Still-image inspection and codecs. Supplied streams remain open.</summary>
public static partial class ImageCodec
{
    public static ImageInfo Identify(ReadOnlySpan<byte> input, ImageLimits? limits = null,
        CancellationToken cancellationToken = default)
        => Identify(input, limits, cancellationToken, ImageMetadataPolicy.Strict);

    /// <summary>Identify with an explicit optional-metadata policy; container checks remain enabled.</summary>
    public static ImageInfo Identify(ReadOnlySpan<byte> input, ImageLimits? limits,
        CancellationToken cancellationToken, ImageMetadataPolicy metadataPolicy)
    {
        using var context = InputContext(input.Length, limits, cancellationToken, metadataPolicy);
        return IdentifyCore(input, context, metadataPolicy);
    }

    private static ImageInfo IdentifyCore(ReadOnlySpan<byte> input, OperationContext context,
        ImageMetadataPolicy metadataPolicy = ImageMetadataPolicy.Strict)
    {
        if (input.StartsWith(PngCodec.Signature)) return PngCodec.Identify(input, context, metadataPolicy);
        if (input.StartsWith(new byte[] { 255, 216 })) return JpegIdentifier.Identify(input, context, metadataPolicy);
        if (input.StartsWith("RIFF"u8)) return ContainerHeaders.WebP(input, context, metadataPolicy);
        throw new UnsupportedImageException(UnsupportedImageFeature.Format, "Expected a still PNG, JPEG or WebP image.");
    }

    public static ImageBuffer Decode(ReadOnlySpan<byte> input, DecodeOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new DecodeOptions();
        using var context = InputContext(input.Length, options.Limits, cancellationToken, options.MetadataPolicy);
        return DecodeCore(input, options, context);
    }

    private static ImageBuffer DecodeCore(ReadOnlySpan<byte> input, DecodeOptions options, OperationContext context)
    {
        ImageBuffer result;
        if (input.StartsWith(PngCodec.Signature)) result = PngCodec.Decode(input, context, options.MetadataPolicy);
        else if (input.StartsWith(new byte[] { 255, 216 })) result = new JpegDecoder(input, context, options.MetadataPolicy).Decode();
        else if (input.StartsWith("RIFF"u8)) result = WebpCodec.Decode(input, context, options.MetadataPolicy);
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
        => Identify(input, limits, cancellationToken, ImageMetadataPolicy.Strict);

    /// <summary>Identify a complete stream using an explicit optional-metadata policy.</summary>
    public static ImageInfo Identify(Stream input, ImageLimits? limits,
        CancellationToken cancellationToken, ImageMetadataPolicy metadataPolicy)
    {
        limits ??= ImageLimits.Default;
        using var context = ReadContext(limits, cancellationToken, metadataPolicy);
        using var data = IdentificationInput.Read(input, context);
        return IdentifyCore(data.Span, context, metadataPolicy);
    }

    public static ImageBuffer Decode(Stream input, DecodeOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new DecodeOptions();
        using var context = ReadContext(options.Limits, cancellationToken, options.MetadataPolicy);
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

    private static OperationContext InputContext(int length, ImageLimits? limits, CancellationToken cancellation,
        ImageMetadataPolicy metadataPolicy = ImageMetadataPolicy.Strict)
    {
        var context = ReadContext(limits ?? ImageLimits.Default, cancellation, metadataPolicy);
        if (length > context.Limits.MaxInputBytes) throw new ImageLimitException(ImageLimitKind.InputBytes, "Encoded input limit exceeded.");
        context.ReserveBorrowed(length);
        return context;
    }

    private static OperationContext ReadContext(ImageLimits limits, CancellationToken cancellation, ImageMetadataPolicy metadataPolicy)
    {
        ExifOrientation.ValidatePolicy(metadataPolicy);
        return new OperationContext(limits, cancellation);
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
