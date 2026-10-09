using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace Lokad.Image.Internal;

/// <summary>Reverse complete pixels, keeping channel bytes and precision intact.</summary>
internal static class HorizontalFlip
{
    public static ImageBuffer Apply(ImageBuffer source, ImageLimits? limits, CancellationToken cancellationToken)
    {
        var context = new OperationContext(limits ?? ImageLimits.Default, cancellationToken);
        context.Reserve(source.ByteLength);
        var result = ImageBuffer.Allocate(source.Width, source.Height, source.BitDepth, context);
        var stride = source.Stride;
        for (var y = 0; y < source.Height; y++)
        {
            var input = source.Pixels.Slice(y * stride, stride);
            var output = result.Pixels.Slice(y * stride, stride);
            if (source.BitDepth == 8)
                Reverse(MemoryMarshal.Cast<byte, uint>(input), MemoryMarshal.Cast<byte, uint>(output), context);
            else
                Reverse(MemoryMarshal.Cast<byte, ulong>(input), MemoryMarshal.Cast<byte, ulong>(output), context);
        }
        result.Orientation = source.Orientation;
        return result;
    }

    private static void Reverse(ReadOnlySpan<uint> source, Span<uint> destination, OperationContext context)
    {
        for (var start = 0; start < destination.Length; start += 1024)
        {
            var count = Math.Min(1024, destination.Length - start);
            context.Work(count * 4L);
            var input = source.Slice(source.Length - start - count, count);
            var output = destination.Slice(start, count);
            var x = 0;
            if (Vector256.IsHardwareAccelerated)
                for (; x <= count - Vector256<uint>.Count; x += Vector256<uint>.Count)
                    Vector256.Shuffle(Vector256.Create(input.Slice(count - x - Vector256<uint>.Count)),
                        Vector256.Create(7u, 6u, 5u, 4u, 3u, 2u, 1u, 0u)).CopyTo(output.Slice(x));
            else if (Vector128.IsHardwareAccelerated)
                for (; x <= count - Vector128<uint>.Count; x += Vector128<uint>.Count)
                    Vector128.Shuffle(Vector128.Create(input.Slice(count - x - Vector128<uint>.Count)),
                        Vector128.Create(3u, 2u, 1u, 0u)).CopyTo(output.Slice(x));
            for (; x < count; x++) output[x] = input[count - x - 1];
        }
    }

    private static void Reverse(ReadOnlySpan<ulong> source, Span<ulong> destination, OperationContext context)
    {
        for (var start = 0; start < destination.Length; start += 1024)
        {
            var count = Math.Min(1024, destination.Length - start);
            context.Work(count * 4L);
            var input = source.Slice(source.Length - start - count, count);
            var output = destination.Slice(start, count);
            var x = 0;
            if (Vector256.IsHardwareAccelerated)
                for (; x <= count - Vector256<ulong>.Count; x += Vector256<ulong>.Count)
                    Vector256.Shuffle(Vector256.Create(input.Slice(count - x - Vector256<ulong>.Count)),
                        Vector256.Create(3ul, 2ul, 1ul, 0ul)).CopyTo(output.Slice(x));
            else if (Vector128.IsHardwareAccelerated)
                for (; x <= count - Vector128<ulong>.Count; x += Vector128<ulong>.Count)
                    Vector128.Shuffle(Vector128.Create(input.Slice(count - x - Vector128<ulong>.Count)),
                        Vector128.Create(1ul, 0ul)).CopyTo(output.Slice(x));
            for (; x < count; x++) output[x] = input[count - x - 1];
        }
    }
}
