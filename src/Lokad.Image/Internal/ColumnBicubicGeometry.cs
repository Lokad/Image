using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace Lokad.Image.Internal;

// Borrows already admitted, unpublished result-row tails. Prefix strips finish
// before those tails are written; tail strips then rebuild geometry directly.
// This view never outlives the result's synchronous overwrite operation.
internal readonly ref struct ColumnBicubicGeometry
{
    private readonly Span<byte> storage;
    private readonly int stride, offset, sourceHeight, height;
    private readonly double scale;
    internal int PrefixColumns { get; }

    private ColumnBicubicGeometry(Span<byte> storage, int stride, int offset,
        int sourceHeight, int height, double scale, int prefixColumns)
    {
        this.storage = storage; this.stride = stride; this.offset = offset;
        this.sourceHeight = sourceHeight; this.height = height; this.scale = scale;
        PrefixColumns = prefixColumns;
    }

    internal static ColumnBicubicGeometry Prepare(ImageBuffer result, int sourceHeight,
        double scale, int maximumTaps, int stripColumns, CancellationToken cancellation)
    {
        var bytes = 16 + maximumTaps * sizeof(double);
        var pixelBytes = result.BitDepth / 8 * 4;
        var footerColumns = (bytes + pixelBytes - 1) / pixelBytes;
        var prefix = (result.Width - footerColumns) / stripColumns * stripColumns;
        if (prefix < 2 * stripColumns)
            return new(default, result.Stride, 0, sourceHeight, result.Height, scale, 0);
        var resultView = new ColumnBicubicGeometry(result.Pixels, result.Stride,
            result.Width * pixelBytes - bytes, sourceHeight, result.Height, scale, prefix);
        for (var y = 0; y < result.Height; y++)
        {
            cancellation.ThrowIfCancellationRequested();
            var entry = resultView.storage.Slice(y * resultView.stride + resultView.offset, bytes);
            var weights = MemoryMarshal.Cast<byte, double>(entry[16..]);
            resultView.Build(y, weights, out var first, out var count, out var normalizer);
            BinaryPrimitives.WriteInt32LittleEndian(entry, first);
            BinaryPrimitives.WriteInt32LittleEndian(entry[4..], count);
            BinaryPrimitives.WriteDoubleLittleEndian(entry[8..], normalizer);
        }
        return resultView;
    }

    // Only unpublished tail rows are rebuilt after their cache users return.
    // The original whole-image preparation remains unchanged for other callers.
    internal static ColumnBicubicGeometry PrepareTail(ImageBuffer result, int sourceHeight,
        double scale, int maximumTaps, int stripColumns, int firstRow, CancellationToken cancellation)
    {
        if ((uint)firstRow > (uint)result.Height) throw new ArgumentOutOfRangeException(nameof(firstRow));
        var bytes = 16 + maximumTaps * sizeof(double);
        var pixelBytes = result.BitDepth / 8 * 4;
        var footerColumns = (bytes + pixelBytes - 1) / pixelBytes;
        var prefix = (result.Width - footerColumns) / stripColumns * stripColumns;
        if (prefix < 2 * stripColumns)
            return new(default, result.Stride, 0, sourceHeight, result.Height, scale, 0);
        var resultView = new ColumnBicubicGeometry(result.Pixels, result.Stride,
            result.Width * pixelBytes - bytes, sourceHeight, result.Height, scale, prefix);
        for (var y = firstRow; y < result.Height; y++)
        {
            cancellation.ThrowIfCancellationRequested();
            var entry = resultView.storage.Slice(y * resultView.stride + resultView.offset, bytes);
            var weights = MemoryMarshal.Cast<byte, double>(entry[16..]);
            resultView.Build(y, weights, out var first, out var count, out var normalizer);
            BinaryPrimitives.WriteInt32LittleEndian(entry, first);
            BinaryPrimitives.WriteInt32LittleEndian(entry[4..], count);
            BinaryPrimitives.WriteDoubleLittleEndian(entry[8..], normalizer);
        }
        return resultView;
    }

    internal void Read(int y, bool prepared, Span<double> weights,
        out int first, out int count, out double normalizer)
    {
        if (!prepared) { Build(y, weights, out first, out count, out normalizer); return; }
        var entry = storage.Slice(y * stride + offset);
        first = BinaryPrimitives.ReadInt32LittleEndian(entry);
        count = BinaryPrimitives.ReadInt32LittleEndian(entry[4..]);
        normalizer = BinaryPrimitives.ReadDoubleLittleEndian(entry[8..]);
        MemoryMarshal.Cast<byte, double>(entry.Slice(16, count * sizeof(double))).CopyTo(weights);
    }

    private void Build(int y, Span<double> weights, out int first, out int count, out double normalizer)
    {
        var center = (y + .5) * sourceHeight / height - .5;
        first = (int)Math.Ceiling(center - 2 * scale);
        count = (int)Math.Floor(center + 2 * scale) - first + 1;
        normalizer = 0;
        for (var t = 0; t < count; t++)
        {
            var weight = ImageTransforms.Kernel((first + t - center) / scale, ResizeFilter.Bicubic);
            weights[t] = weight; normalizer += weight;
        }
    }
}
