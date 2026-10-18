namespace Lokad.Image;

public sealed partial class ImageBudget
{
    /// <summary>Fixed-workspace header/EXIF inspection; PNG CRCs and compressed pixels are not validated.</summary>
    public ImageInfo InspectHeaders(Stream input, ImageLimits? limits = null, CancellationToken cancellationToken = default)
        => ImageCodec.InspectHeadersCore(input, limits, cancellationToken, this, false).GetAwaiter().GetResult();
    /// <summary>Fixed-workspace asynchronous header/EXIF inspection with reservation across I/O awaits.</summary>
    public ValueTask<ImageInfo> InspectHeadersAsync(Stream input, ImageLimits? limits = null, CancellationToken cancellationToken = default)
        => ImageCodec.InspectHeadersCore(input, limits, cancellationToken, this, true);
}
