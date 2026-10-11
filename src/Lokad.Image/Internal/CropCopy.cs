namespace Lokad.Image.Internal;

/// <summary>Copy admitted crop rows without per-pixel coordinate callbacks.</summary>
internal static class CropCopy
{
    public static ImageBuffer Apply(ImageBuffer source, int x, int y, int width, int height,
        ImageLimits? limits, CancellationToken cancellationToken)
    {
        var context = new OperationContext(limits ?? ImageLimits.Default, cancellationToken);
        context.Reserve(source.ByteLength);
        var result = ImageBuffer.AllocateForOverwrite(width, height, source.BitDepth, context);
        try
        {
            var pixelBytes = source.BitDepth / 8 * 4;
            var sourceStride = source.Stride;
            var rowBytes = result.Stride;
            for (var row = 0; row < height; row++)
            {
                var input = source.Pixels.Slice((row + y) * sourceStride + x * pixelBytes, rowBytes);
                var output = result.Pixels.Slice(row * rowBytes, rowBytes);
                for (var start = 0; start < width; start += 1024)
                {
                    var count = Math.Min(1024, width - start);
                    context.Work(count * 4L);
                    input.Slice(start * pixelBytes, count * pixelBytes)
                        .CopyTo(output.Slice(start * pixelBytes, count * pixelBytes));
                }
            }
            result.Orientation = source.Orientation;
            return result;
        }
        catch { result.Dispose(); throw; }
    }
}
