using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

// Reverse complete pixels; callers own placement, admission and result lifetime.
internal static class PixelReversal
{
    internal static void Reverse(ReadOnlySpan<uint> source, Span<uint> destination, CancellationToken cancellationToken)
    {
        for (var start = 0; start < destination.Length; start += 1024)
        {
            var count = Math.Min(1024, destination.Length - start);
            cancellationToken.ThrowIfCancellationRequested();
            var input = source.Slice(source.Length - start - count, count);
            var output = destination.Slice(start, count);
            var x = 0;
            if (Avx512F.IsSupported)
                for (; x <= count - Vector512<uint>.Count; x += Vector512<uint>.Count)
                    Avx512F.PermuteVar16x32(Vector512.Create(input.Slice(count - x - Vector512<uint>.Count, Vector512<uint>.Count)),
                        Vector512.Create(15u, 14u, 13u, 12u, 11u, 10u, 9u, 8u, 7u, 6u, 5u, 4u, 3u, 2u, 1u, 0u)).CopyTo(output.Slice(x));
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

    internal static void Reverse(ReadOnlySpan<ulong> source, Span<ulong> destination, CancellationToken cancellationToken)
    {
        for (var start = 0; start < destination.Length; start += 1024)
        {
            var count = Math.Min(1024, destination.Length - start);
            cancellationToken.ThrowIfCancellationRequested();
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
