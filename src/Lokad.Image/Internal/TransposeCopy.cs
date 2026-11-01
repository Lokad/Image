using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Runtime.InteropServices;

namespace Lokad.Image.Internal;

/// <summary>Transpose complete pixels in bounded cache-local tiles.</summary>
internal static class TransposeCopy
{
    public static ImageBuffer Apply(ImageBuffer source, ImageLimits? limits, CancellationToken cancellationToken)
    {
        using var context = new OperationContext(limits ?? ImageLimits.Default, cancellationToken);
        context.ReserveBorrowed(source.ByteLength);
        var result = ImageBuffer.AllocateForOverwrite(source.Height, source.Width, source.BitDepth, context);
        try
        {
            if (source.BitDepth == 8)
                Copy(MemoryMarshal.Cast<byte, uint>(source.Pixels), MemoryMarshal.Cast<byte, uint>(result.Pixels),
                    source.Width, source.Height, context);
            else if (Avx2.IsSupported)
                CopyWords(MemoryMarshal.Cast<byte, ulong>(source.Pixels), MemoryMarshal.Cast<byte, ulong>(result.Pixels),
                    source.Width, source.Height, context);
            else
                Copy(MemoryMarshal.Cast<byte, ulong>(source.Pixels), MemoryMarshal.Cast<byte, ulong>(result.Pixels),
                    source.Width, source.Height, context);
            result.Orientation = source.Orientation;
            return result;
        }
        catch { result.Dispose(); throw; }
    }

    private static void CopyWords(ReadOnlySpan<ulong> source, Span<ulong> destination, int width, int height,
        OperationContext context)
    {
        // Four complete pixels per vector; unpacks and lane selection transpose
        // each 4x4 matrix without interpreting any channel or alpha sample.
        for (var y = 0; y < height; y += 32)
        for (var x = 0; x < width; x += 32)
        {
            var rows = Math.Min(32, height - y);
            var columns = Math.Min(32, width - x);
            context.Cancellation.ThrowIfCancellationRequested();
            var vectorRows = rows & ~3; var vectorColumns = columns & ~3;
            for (var row = 0; row < vectorRows; row += 4)
            for (var column = 0; column < vectorColumns; column += 4)
            {
                var input = (y + row) * width + x + column;
                var r0 = Vector256.Create(source.Slice(input, 4));
                var r1 = Vector256.Create(source.Slice(input + width, 4));
                var r2 = Vector256.Create(source.Slice(input + 2 * width, 4));
                var r3 = Vector256.Create(source.Slice(input + 3 * width, 4));
                var low01 = Avx2.UnpackLow(r0, r1); var high01 = Avx2.UnpackHigh(r0, r1);
                var low23 = Avx2.UnpackLow(r2, r3); var high23 = Avx2.UnpackHigh(r2, r3);
                var output = (x + column) * height + y + row;
                Avx2.Permute2x128(low01, low23, 0x20).CopyTo(destination.Slice(output, 4));
                Avx2.Permute2x128(high01, high23, 0x20).CopyTo(destination.Slice(output + height, 4));
                Avx2.Permute2x128(low01, low23, 0x31).CopyTo(destination.Slice(output + 2 * height, 4));
                Avx2.Permute2x128(high01, high23, 0x31).CopyTo(destination.Slice(output + 3 * height, 4));
            }
            for (var column = 0; column < columns; column++)
            {
                var firstRow = column < vectorColumns ? vectorRows : 0;
                var input = (y + firstRow) * width + x + column;
                var output = destination.Slice((x + column) * height + y + firstRow, rows - firstRow);
                for (var row = 0; row < output.Length; row++)
                {
                    output[row] = source[input];
                    input += width;
                }
            }
        }
    }

    private static void Copy<T>(ReadOnlySpan<T> source, Span<T> destination, int width, int height,
        OperationContext context) where T : unmanaged
    {
        // A tile contains at most 1024 pixels; both working regions stay small.
        // Full-pixel assignments retain channel byte order at either sample depth.
        for (var y = 0; y < height; y += 32)
        for (var x = 0; x < width; x += 32)
        {
            var rows = Math.Min(32, height - y);
            var columns = Math.Min(32, width - x);
            context.Cancellation.ThrowIfCancellationRequested();
            for (var column = x; column < x + columns; column++)
            {
                var input = y * width + column;
                var output = destination.Slice(column * height + y, rows);
                for (var row = 0; row < rows; row++)
                {
                    output[row] = source[input];
                    input += width;
                }
            }
        }
    }
}
