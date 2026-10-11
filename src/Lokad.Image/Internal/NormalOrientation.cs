namespace Lokad.Image.Internal;

/// <summary>Copy normal orientation in bounded rows, retaining complete channel bytes.</summary>
internal static class NormalOrientation
{
    public static ImageBuffer Apply(ImageBuffer source, ImageLimits? limits, CancellationToken cancellationToken)
    {
        var context = new OperationContext(limits ?? ImageLimits.Default, cancellationToken);
        context.Reserve(source.ByteLength);
        var result = ImageBuffer.AllocateForOverwrite(source.Width, source.Height, source.BitDepth, context);
        try
        {
            var stride = source.Stride;
            var pixelBytes = source.BitDepth / 8 * 4;
            for (var y = 0; y < source.Height; y++)
            {
                var input = source.Pixels.Slice(y * stride, stride);
                var output = result.Pixels.Slice(y * stride, stride);
                for (var start = 0; start < source.Width; start += 1024)
                {
                    var count = Math.Min(1024, source.Width - start);
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
