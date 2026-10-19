using Lokad.Image.Internal;

namespace Lokad.Image;

public static partial class ImageCodec
{
    /// <summary>Inspect container headers and EXIF using fixed workspace; leaves input open at EOF on success.</summary>
    /// <remarks>Validates framing and supported header/metadata fields, not PNG CRCs
    /// or compressed pixels. Uses seek to skip opaque payloads when available;
    /// otherwise drains them through bounded scratch. Full Identify is unchanged.</remarks>
    public static ImageInfo InspectHeaders(Stream input, ImageLimits? limits = null, CancellationToken cancellationToken = default)
        => InspectHeadersCore(input, limits, cancellationToken, null, false).GetAwaiter().GetResult();

    /// <summary>Inspect headers/EXIF with asynchronous reads and fixed workspace; leaves input open.</summary>
    /// <remarks>Same validation scope as InspectHeaders; seeking may skip opaque payloads.
    /// PNG CRCs and compressed pixel syntax are not validated.</remarks>
    public static ValueTask<ImageInfo> InspectHeadersAsync(Stream input, ImageLimits? limits = null, CancellationToken cancellationToken = default)
        => InspectHeadersCore(input, limits, cancellationToken, null, true);

    internal static async ValueTask<ImageInfo> InspectHeadersCore(Stream input, ImageLimits? limits, CancellationToken cancellationToken,
        ImageBudget? budget, bool asynchronous)
    {
        using var context = new OperationContext(limits ?? ImageLimits.Default, cancellationToken);
        ArgumentNullException.ThrowIfNull(input);
        var admission = budget?.Begin(context.Limits);
        try
        {
            using var reader = new InspectionReader(input, context, asynchronous);
            return await StreamInspection.InspectAsync(reader).ConfigureAwait(false);
        }
        finally { admission?.Complete(context.ConsumedWork); }
    }
}
