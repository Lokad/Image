using Lokad.Image.Internal;

namespace Lokad.Image;

public static partial class ImageCodec
{
    /// <summary>Validate complete container and pixel coding without returning a raster.</summary>
    /// <remarks>Returns coded source metadata; no color conversion or reorientation is applied.
    /// Uses the decoder's applicable validity checks, limits and optional-metadata policy.
    /// PNG retains compressed input and row scratch, JPEG retains coefficients, and WebP
    /// retains required planes/history. This is not a fixed-workspace inspection.</remarks>
    public static ImageInfo ValidatePixels(ReadOnlySpan<byte> input, ImageLimits? limits = null,
        CancellationToken cancellationToken = default, ImageMetadataPolicy metadataPolicy = ImageMetadataPolicy.Strict)
    {
        using var context = InputContext(input.Length, limits, cancellationToken, metadataPolicy);
        return ValidatePixelsCore(input, context, metadataPolicy);
    }

    /// <summary>Read through EOF and validate pixel coding; leaves the borrowed stream open at EOF.</summary>
    public static ImageInfo ValidatePixels(Stream input, ImageLimits? limits = null,
        CancellationToken cancellationToken = default, ImageMetadataPolicy metadataPolicy = ImageMetadataPolicy.Strict)
    {
        ArgumentNullException.ThrowIfNull(input);
        using var context = ReadContext(limits ?? ImageLimits.Default, cancellationToken, metadataPolicy);
        using var data = IdentificationInput.ReadForDecode(input, context);
        try { return ValidatePixelsCore(data.Span, context, metadataPolicy); }
        finally { context.Release(data.Capacity); }
    }

    /// <summary>Acquire input through actual asynchronous reads, then validate pixel coding synchronously.</summary>
    /// <remarks>Leaves the stream open at EOF. Acquired storage stays charged until its actual users finish.</remarks>
    public static ValueTask<ImageInfo> ValidatePixelsAsync(Stream input, ImageLimits? limits = null,
        CancellationToken cancellationToken = default, ImageMetadataPolicy metadataPolicy = ImageMetadataPolicy.Strict)
    {
        ArgumentNullException.ThrowIfNull(input);
        var context = ReadContext(limits ?? ImageLimits.Default, cancellationToken, metadataPolicy);
        return ValidatePixelsFromStreamAsync(input, context, metadataPolicy);
    }

    private static async ValueTask<ImageInfo> ValidatePixelsFromStreamAsync(Stream input, OperationContext context,
        ImageMetadataPolicy metadataPolicy)
    {
        using var execution = context;
        using var data = await IdentificationInput.ReadAsync(input, context).ConfigureAwait(false);
        try { return ValidatePixelsCore(data.Span, context, metadataPolicy); }
        finally { context.Release(data.Capacity); }
    }

    private static ImageInfo ValidatePixelsCore(ReadOnlySpan<byte> input, OperationContext context, ImageMetadataPolicy metadataPolicy)
    {
        if (input.StartsWith(PngCodec.Signature)) return PngCodec.ValidatePixels(input, context, metadataPolicy);
        if (input.StartsWith(new byte[] { 255, 216 })) return new JpegDecoder(input, context, metadataPolicy).ValidatePixels();
        if (input.StartsWith("RIFF"u8)) return WebpCodec.ValidatePixels(input, context, metadataPolicy);
        throw new UnsupportedImageException(UnsupportedImageFeature.Format, "Expected a still PNG, JPEG or WebP image.");
    }
}
