namespace Lokad.Image;

public sealed partial class ImageBudget
{
    /// <summary>Full container/EXIF validation after asynchronous stream acquisition; input remains open.</summary>
    public ValueTask<ImageInfo> IdentifyAsync(Stream input, ImageLimits? limits = null,
        CancellationToken cancellationToken = default)
        => ImageCodec.IdentifyWithBudgetAsync(input, this, limits, cancellationToken);

    /// <summary>Asynchronously acquire input and decode with retained pixel admission; input remains open.</summary>
    public ValueTask<BudgetedImage> DecodeAsync(Stream input, DecodeOptions? options = null,
        CancellationToken cancellationToken = default)
        => ImageCodec.DecodeWithBudgetAsync(input, this, options, cancellationToken);
}
