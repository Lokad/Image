namespace Lokad.Image.Internal;

/// <summary>Borrowed sample rows; normalization never exceeds sixteen RGB pixels.</summary>
internal readonly ref struct JpegSampleInput
{
    private readonly ReadOnlySpan<byte> samples;
    internal int Width { get; }
    internal int Height { get; }
    internal int Stride { get; }
    internal JpegSampleFormat Format { get; }

    internal JpegSampleInput(ReadOnlySpan<byte> samples, int width, int height,
        JpegSampleFormat format, int stride, OperationContext context)
    {
        if (!Enum.IsDefined(format)) throw new ArgumentOutOfRangeException(nameof(format));
        if (format == JpegSampleFormat.Cmyk)
            throw new UnsupportedImageException(UnsupportedImageFeature.ComponentLayout,
                "JPEG sample encoding accepts grayscale or RGB input.", ImageFormat.Jpeg);
        ArgumentOutOfRangeException.ThrowIfNegative(stride);
        context.PixelBytes(width, height, 8);
        if (width > 65535 || height > 65535)
            throw new ImageLimitException(ImageLimitKind.Dimension, "JPEG dimensions exceed 65535.");
        var rowBytes = (long)width * (int)format;
        if (rowBytes > int.MaxValue)
            throw new ImageLimitException(ImageLimitKind.BufferLength, "JPEG sample row is too large.");
        if (stride == 0) stride = (int)rowBytes;
        if (stride < rowBytes) throw new ArgumentOutOfRangeException(nameof(stride), "Stride must cover a complete sample row.");
        if ((long)(height - 1) * stride + rowBytes > samples.Length)
            throw new ArgumentException("Source does not cover all JPEG sample rows.", nameof(samples));
        this.samples = samples; Width = width; Height = height; Stride = stride; Format = format;
    }

    internal void CopyGrayBlock(int left, int top, Span<byte> block)
    {
        for (var y = 0; y < 8; y++)
        {
            var sourceY = Math.Min(top + y, Height - 1);
            var sourceX = Math.Min(left, Width - 1);
            var count = Math.Min(8, Width - sourceX);
            var row = block.Slice(y * 8, 8);
            samples.Slice(sourceY * Stride + sourceX, count).CopyTo(row);
            row[count..].Fill(row[count - 1]);
        }
    }

    internal void CopyRgbRow(int left, int top, Span<byte> rgb)
    {
        var row = samples.Slice(Math.Min(top, Height - 1) * Stride, Width * 3);
        var count = rgb.Length / 3;
        if (left <= Width - count) { row.Slice(left * 3, rgb.Length).CopyTo(rgb); return; }
        for (var x = 0; x < count; x++)
        {
            var offset = Math.Min(left + x, Width - 1) * 3;
            row.Slice(offset, 3).CopyTo(rgb.Slice(x * 3, 3));
        }
    }
}
