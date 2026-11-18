using System.Runtime.Intrinsics;

namespace Lokad.Image.Internal;

/// <summary>Copy admitted crop rows without per-pixel coordinate callbacks.</summary>
internal static class CropCopy
{
    public static ImageBuffer Apply(ImageBuffer source, int x, int y, int width, int height,
        ImageLimits? limits, CancellationToken cancellationToken)
    {
        var result = ImageBuffer.AllocateCopyResult(source, width, height, limits, cancellationToken);
        try
        {
            var pixelBytes = source.BitDepth / 8 * 4;
            var sourceStride = source.Stride;
            var rowBytes = result.Stride;
            if (pixelBytes == 4 && Vector512.IsHardwareAccelerated && width <= 1024 && rowBytes >= 256)
            {
                // The fresh result cannot alias the source. Each row fits
                // in one original cancellation chunk.
                IndependentRowCopy.CopyRows(source.Pixels, result.Pixels, y * sourceStride + x * pixelBytes,
                    0, rowBytes, height, sourceStride, rowBytes, cancellationToken);
            }
            else
            {
                for (var row = 0; row < height; row++)
                {
                    var input = source.Pixels.Slice((row + y) * sourceStride + x * pixelBytes, rowBytes);
                    var output = result.Pixels.Slice(row * rowBytes, rowBytes);
                    for (var start = 0; start < width; start += 1024)
                    {
                        var count = Math.Min(1024, width - start);
                        cancellationToken.ThrowIfCancellationRequested();
                        input.Slice(start * pixelBytes, count * pixelBytes)
                            .CopyTo(output.Slice(start * pixelBytes, count * pixelBytes));
                    }
                }
            }
            result.Orientation = source.Orientation;
            return result;
        }
        catch { result.Dispose(); throw; }
    }
}
