using Lokad.Image.Internal;

namespace Lokad.Image;

public static partial class ImageCodec
{
    /// <summary>Read through EOF asynchronously, then perform complete container/EXIF validation. Leaves input open.</summary>
    /// <remarks>CPU parsing is synchronous. Encoded input uses bounded private storage;
    /// this is the full Identify contract, not a header-only inspection.</remarks>
    public static ValueTask<ImageInfo> IdentifyAsync(Stream input, ImageLimits? limits = null,
        CancellationToken cancellationToken = default)
        => IdentifyAsync(input, limits, cancellationToken, ImageMetadataPolicy.Strict);

    /// <summary>Asynchronously acquire a complete image with explicit optional-metadata handling.</summary>
    public static ValueTask<ImageInfo> IdentifyAsync(Stream input, ImageLimits? limits,
        CancellationToken cancellationToken, ImageMetadataPolicy metadataPolicy)
    {
        var context = ReadContext(limits ?? ImageLimits.Default, cancellationToken, metadataPolicy);
        ArgumentNullException.ThrowIfNull(input);
        return IdentifyFromStreamAsync(input, context, metadataPolicy);
    }

    /// <summary>Read through EOF asynchronously and decode into an independently owned image. Leaves input open.</summary>
    /// <remarks>CPU decoding is synchronous. Encoded input is not copied into a final
    /// exact-sized array. Actual private capacities count toward working admission.</remarks>
    public static ValueTask<ImageBuffer> DecodeAsync(Stream input, DecodeOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new DecodeOptions();
        var context = ReadContext(options.Limits, cancellationToken, options.MetadataPolicy);
        ArgumentNullException.ThrowIfNull(input);
        return DecodeFromStreamAsync(input, options, context);
    }

    private static async ValueTask<ImageInfo> IdentifyFromStreamAsync(Stream input, OperationContext context, ImageMetadataPolicy metadataPolicy)
    {
        using var execution = context;
        using var data = await IdentificationInput.ReadAsync(input, context).ConfigureAwait(false);
        try { return IdentifyCore(data.Span, context, metadataPolicy); }
        finally { context.Release(data.Capacity); }
    }

    private static async ValueTask<ImageBuffer> DecodeFromStreamAsync(Stream input, DecodeOptions options, OperationContext context)
    {
        using var execution = context;
        using var data = await IdentificationInput.ReadAsync(input, context).ConfigureAwait(false);
        try { return DecodeCore(data.Span, options, context); }
        finally { context.Release(data.Capacity); }
    }

}
