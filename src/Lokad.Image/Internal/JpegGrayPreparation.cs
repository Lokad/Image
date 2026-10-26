namespace Lokad.Image.Internal;

// Borrowed image/RGB samples to one gray8 block. The outer grayscale pass
// validates geometry and admits its8KiB fixed workspace. Row scratch
// stays bounded at32packedRGB bytes or24callerRGB bytes, with no owned storage.
internal static class JpegGrayPreparation
{
    private static byte Gray(int r, int g, int b)
    {
        // Keep separate binary64 products/additions and nearest rounding at
        // the pinned RGB-to-gray boundary; do not round an intermediate luma.
        var luma = (0.299 * r + 0.587 * g) + 0.114 * b;
        return (byte)Math.Floor(luma + 0.5);
    }

    internal static void CopyBlock(ImageBuffer? image, JpegSampleInput source, int width, int height,
        int left, int top, uint? background, Span<byte> output)
    {
        if (output.Length != 64) throw new ArgumentException("Complete8x8 grayscale block required.", nameof(output));
        var count = Math.Min(8, width - left);
        if (image is null)
        {
            Span<byte> row = stackalloc byte[24];
            for (var y = 0; y < 8; y++)
            {
                source.CopyRgbRow(left, Math.Min(top + y, height - 1), row[..(count * 3)]);
                for (var x = 0; x < 8; x++)
                {
                    var at = Math.Min(x, count - 1) * 3;
                    output[y * 8 + x] = Gray(row[at], row[at + 1], row[at + 2]);
                }
            }
            return;
        }
        Span<int> packed = stackalloc int[8];
        for (var y = 0; y < 8; y++)
        {
            var sourceY = Math.Min(top + y, height - 1);
            if (image.BitDepth == 8)
                JpegReferenceColor.NormalizeRgba8(image.GetRow8(sourceY).Slice(left * 4, count * 4), packed[..count], background);
            else
                JpegReferenceColor.NormalizeRgba16(image.GetRow16(sourceY).Slice(left * 4, count * 4), packed[..count], background);
            for (var x = 0; x < 8; x++)
            {
                var rgb = packed[Math.Min(x, count - 1)];
                output[y * 8 + x] = Gray(rgb & 255, rgb >> 8 & 255, rgb >> 16 & 255);
            }
        }
    }
}
