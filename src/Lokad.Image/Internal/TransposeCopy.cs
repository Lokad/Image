using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Runtime.InteropServices;

namespace Lokad.Image.Internal;

/// <summary>Transpose complete pixels in bounded cache-local tiles.</summary>
internal static class TransposeCopy
{
    internal readonly struct ForwardRows { }
    internal readonly struct ReverseRows { }
    internal readonly struct ForwardColumns { }
    internal readonly struct ReverseColumns { }

    public static ImageBuffer Apply<TRows, TColumns>(ImageBuffer source, ImageLimits? limits, CancellationToken cancellationToken)
        where TRows : struct where TColumns : struct
    {
        using var context = new OperationContext(limits ?? ImageLimits.Default, cancellationToken);
        context.ReserveBorrowed(source.ByteLength);
        var result = ImageBuffer.AllocateForOverwrite(source.Height, source.Width, source.BitDepth, context);
        try
        {
            if (source.BitDepth == 8)
                Copy<uint, TRows, TColumns>(MemoryMarshal.Cast<byte, uint>(source.Pixels), MemoryMarshal.Cast<byte, uint>(result.Pixels),
                    source.Width, source.Height, context);
            else if (Avx2.IsSupported)
                CopyWords<TRows, TColumns>(MemoryMarshal.Cast<byte, ulong>(source.Pixels), MemoryMarshal.Cast<byte, ulong>(result.Pixels),
                    source.Width, source.Height, context);
            else
                Copy<ulong, TRows, TColumns>(MemoryMarshal.Cast<byte, ulong>(source.Pixels), MemoryMarshal.Cast<byte, ulong>(result.Pixels),
                    source.Width, source.Height, context);
            result.Orientation = source.Orientation;
            return result;
        }
        catch { result.Dispose(); throw; }
    }

    private static void CopyWords<TRows, TColumns>(ReadOnlySpan<ulong> source, Span<ulong> destination, int width, int height,
        OperationContext context) where TRows : struct where TColumns : struct
    {
        // Four complete pixels per vector; unpacks and lane selection transpose
        // each 4x4 matrix without interpreting any channel or alpha sample.
        // Closed row/column order selects placement and optional lane reversal
        // without interpreting pixel bits or adding a dynamic pixel-loop policy.
        var step = typeof(TRows) == typeof(ReverseRows) ? -height : height;
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
                var outputRow = typeof(TRows) == typeof(ReverseRows) ? width - 1 - x - column : x + column;
                var outputColumn = typeof(TColumns) == typeof(ReverseColumns) ? height - y - row - 4 : y + row;
                var output = outputRow * height + outputColumn;
                if (typeof(TColumns) == typeof(ReverseColumns))
                {
                    Avx2.Permute4x64(Avx2.Permute2x128(low01, low23, 0x20), 0x1b).CopyTo(destination.Slice(output, 4));
                    Avx2.Permute4x64(Avx2.Permute2x128(high01, high23, 0x20), 0x1b).CopyTo(destination.Slice(output + step, 4));
                    Avx2.Permute4x64(Avx2.Permute2x128(low01, low23, 0x31), 0x1b).CopyTo(destination.Slice(output + 2 * step, 4));
                    Avx2.Permute4x64(Avx2.Permute2x128(high01, high23, 0x31), 0x1b).CopyTo(destination.Slice(output + 3 * step, 4));
                }
                else
                {
                    Avx2.Permute2x128(low01, low23, 0x20).CopyTo(destination.Slice(output, 4));
                    Avx2.Permute2x128(high01, high23, 0x20).CopyTo(destination.Slice(output + step, 4));
                    Avx2.Permute2x128(low01, low23, 0x31).CopyTo(destination.Slice(output + 2 * step, 4));
                    Avx2.Permute2x128(high01, high23, 0x31).CopyTo(destination.Slice(output + 3 * step, 4));
                }
            }
            for (var column = 0; column < columns; column++)
            {
                var firstRow = column < vectorColumns ? vectorRows : 0;
                var input = (y + firstRow) * width + x + column;
                var outputRow = typeof(TRows) == typeof(ReverseRows) ? width - 1 - x - column : x + column;
                var outputColumn = typeof(TColumns) == typeof(ReverseColumns) ? height - y - rows : y + firstRow;
                var output = destination.Slice(outputRow * height + outputColumn, rows - firstRow);
                if (typeof(TColumns) == typeof(ReverseColumns))
                {
                    for (var row = output.Length - 1; row >= 0; row--)
                    {
                        output[row] = source[input];
                        input += width;
                    }
                }
                else
                {
                    for (var row = 0; row < output.Length; row++)
                    {
                        output[row] = source[input];
                        input += width;
                    }
                }
            }
        }
    }

    private static void Copy<T, TRows, TColumns>(ReadOnlySpan<T> source, Span<T> destination, int width, int height,
        OperationContext context) where T : unmanaged where TRows : struct where TColumns : struct
    {
        // Reified generic types keep the UInt64 portable fallback scalar.
        var vectorized = typeof(T) == typeof(uint) && Sse2.IsSupported;
        var step = typeof(TRows) == typeof(ReverseRows) ? -height : height;
        for (var y = 0; y < height; y += 32)
        for (var x = 0; x < width; x += 32)
        {
            var rows = Math.Min(32, height - y);
            var columns = Math.Min(32, width - x);
            context.Cancellation.ThrowIfCancellationRequested();
            var vectorRows = vectorized ? rows & ~3 : 0;
            var vectorColumns = vectorized ? columns & ~3 : 0;
            if (vectorized)
            {
                var inputPixels = MemoryMarshal.Cast<T, uint>(source);
                var outputPixels = MemoryMarshal.Cast<T, uint>(destination);
                for (var row = 0; row < vectorRows; row += 4)
                for (var column = 0; column < vectorColumns; column += 4)
                {
                    var input = (y + row) * width + x + column;
                    var r0 = Vector128.Create(inputPixels.Slice(input, 4));
                    var r1 = Vector128.Create(inputPixels.Slice(input + width, 4));
                    var r2 = Vector128.Create(inputPixels.Slice(input + 2 * width, 4));
                    var r3 = Vector128.Create(inputPixels.Slice(input + 3 * width, 4));
                    var low01 = Sse2.UnpackLow(r0, r1).AsUInt64();
                    var high01 = Sse2.UnpackHigh(r0, r1).AsUInt64();
                    var low23 = Sse2.UnpackLow(r2, r3).AsUInt64();
                    var high23 = Sse2.UnpackHigh(r2, r3).AsUInt64();
                    var outputRow = typeof(TRows) == typeof(ReverseRows) ? width - 1 - x - column : x + column;
                    var outputColumn = typeof(TColumns) == typeof(ReverseColumns) ? height - y - row - 4 : y + row;
                    var output = outputRow * height + outputColumn;
                    if (typeof(TColumns) == typeof(ReverseColumns))
                    {
                        Sse2.Shuffle(Sse2.UnpackLow(low01, low23).AsUInt32(), 0x1b).CopyTo(outputPixels.Slice(output, 4));
                        Sse2.Shuffle(Sse2.UnpackHigh(low01, low23).AsUInt32(), 0x1b).CopyTo(outputPixels.Slice(output + step, 4));
                        Sse2.Shuffle(Sse2.UnpackLow(high01, high23).AsUInt32(), 0x1b).CopyTo(outputPixels.Slice(output + 2 * step, 4));
                        Sse2.Shuffle(Sse2.UnpackHigh(high01, high23).AsUInt32(), 0x1b).CopyTo(outputPixels.Slice(output + 3 * step, 4));
                    }
                    else
                    {
                        Sse2.UnpackLow(low01, low23).AsUInt32().CopyTo(outputPixels.Slice(output, 4));
                        Sse2.UnpackHigh(low01, low23).AsUInt32().CopyTo(outputPixels.Slice(output + step, 4));
                        Sse2.UnpackLow(high01, high23).AsUInt32().CopyTo(outputPixels.Slice(output + 2 * step, 4));
                        Sse2.UnpackHigh(high01, high23).AsUInt32().CopyTo(outputPixels.Slice(output + 3 * step, 4));
                    }
                }
            }
            for (var column = 0; column < columns; column++)
            {
                var firstRow = column < vectorColumns ? vectorRows : 0;
                var input = (y + firstRow) * width + x + column;
                var outputRow = typeof(TRows) == typeof(ReverseRows) ? width - 1 - x - column : x + column;
                var outputColumn = typeof(TColumns) == typeof(ReverseColumns) ? height - y - rows : y + firstRow;
                var output = destination.Slice(outputRow * height + outputColumn, rows - firstRow);
                if (typeof(TColumns) == typeof(ReverseColumns))
                {
                    for (var row = output.Length - 1; row >= 0; row--)
                    {
                        output[row] = source[input];
                        input += width;
                    }
                }
                else
                {
                    for (var row = 0; row < output.Length; row++)
                    {
                        output[row] = source[input];
                        input += width;
                    }
                }
            }
        }
    }
}
