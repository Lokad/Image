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

    public ValueTask EncodePngAsync(ImageBuffer image, Stream output, PngEncodeOptions? options = null,
        CancellationToken cancellationToken = default)
        => ImageCodec.EncodePngWithBudgetAsync(image, output, this, options, cancellationToken);
    public ValueTask EncodeJpegAsync(ImageBuffer image, Stream output, JpegEncodeOptions? options = null,
        CancellationToken cancellationToken = default)
        => ImageCodec.EncodeJpegWithBudgetAsync(image, output, this, options, cancellationToken);
    public ValueTask EncodeWebpAsync(ImageBuffer image, Stream output, WebpEncodeOptions? options = null,
        CancellationToken cancellationToken = default)
        => ImageCodec.EncodeWebpWithBudgetAsync(image, output, this, options, cancellationToken);
    public ValueTask EncodeJpegAsync(ReadOnlyMemory<byte> samples, int width, int height, JpegSampleFormat format,
        Stream output, int stride = 0, JpegEncodeOptions? options = null, CancellationToken cancellationToken = default)
        => ImageCodec.EncodeJpegSamplesWithBudgetAsync(samples, width, height, format, output, stride, this, options, cancellationToken);
}
