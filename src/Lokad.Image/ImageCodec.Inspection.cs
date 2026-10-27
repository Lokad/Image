using Lokad.Image.Internal;

namespace Lokad.Image;

public static partial class ImageCodec
{
    /// <summary>Inspect container headers and EXIF using fixed workspace; leaves input open at EOF on success.</summary>
    /// <remarks>Validates framing and supported header/metadata fields, not PNG CRCs
    /// or compressed pixels. Uses seek to skip opaque payloads when available;
    /// otherwise drains them through bounded scratch. Identify keeps its stronger container checks.</remarks>
    public static ImageInfo InspectHeaders(Stream input, ImageLimits? limits = null, CancellationToken cancellationToken = default)
        => InspectHeadersCore(input, limits, cancellationToken, false, ImageMetadataPolicy.Strict).GetAwaiter().GetResult();

    /// <summary>Inspect complete container framing with an explicit optional-metadata policy.</summary>
    public static ImageInfo InspectHeaders(Stream input, ImageLimits? limits,
        CancellationToken cancellationToken, ImageMetadataPolicy metadataPolicy)
        => InspectHeadersCore(input, limits, cancellationToken, false, metadataPolicy).GetAwaiter().GetResult();

    /// <summary>Inspect headers/EXIF with asynchronous reads and fixed workspace; leaves input open.</summary>
    /// <remarks>Same validation scope as InspectHeaders; seeking may skip opaque payloads.
    /// PNG CRCs and compressed pixel syntax are not validated.</remarks>
    public static ValueTask<ImageInfo> InspectHeadersAsync(Stream input, ImageLimits? limits = null, CancellationToken cancellationToken = default)
        => InspectHeadersCore(input, limits, cancellationToken, true, ImageMetadataPolicy.Strict);

    /// <summary>Asynchronously inspect framing with an explicit optional-metadata policy.</summary>
    public static ValueTask<ImageInfo> InspectHeadersAsync(Stream input, ImageLimits? limits,
        CancellationToken cancellationToken, ImageMetadataPolicy metadataPolicy)
        => InspectHeadersCore(input, limits, cancellationToken, true, metadataPolicy);

    private static async ValueTask<ImageInfo> InspectHeadersCore(Stream input, ImageLimits? limits, CancellationToken cancellationToken,
        bool asynchronous, ImageMetadataPolicy metadataPolicy)
    {
        using var context = ReadContext(limits ?? ImageLimits.Default, cancellationToken, metadataPolicy);
        ArgumentNullException.ThrowIfNull(input);
        using var reader = new InspectionReader(input, context, asynchronous);
        return await StreamInspection.InspectAsync(reader, metadataPolicy: metadataPolicy).ConfigureAwait(false);
    }
}
