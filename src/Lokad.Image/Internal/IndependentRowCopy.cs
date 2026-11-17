using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace Lokad.Image.Internal;

// Bounded disjoint row copies; callers retain geometry, ownership and cancellation.
internal static class IndependentRowCopy
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Copy(ReadOnlySpan<byte> input, Span<byte> output)
    {
        // Callers prove these bounded row chunks are disjoint.
        if (Vector512.IsHardwareAccelerated && input.Length >= 256)
        {
            var source = MemoryMarshal.Cast<byte, CopyBlock>(input);
            var destination = MemoryMarshal.Cast<byte, CopyBlock>(output)[..source.Length];
            for (var i = 0; i < source.Length; i++)
            {
                ref readonly var from = ref source[i];
                ref var to = ref destination[i];
                var a = from[0]; var b = from[1];
                var c = from[2]; var d = from[3];
                to[0] = a; to[1] = b; to[2] = c; to[3] = d;
            }
            var copied = source.Length * (4 * Vector512<byte>.Count);
            input[copied..].CopyTo(output[copied..]);
        }
        else input.CopyTo(output);
    }

    // Selected once for independent rows within one existing cancellation chunk.
    // Public geometry and snapshot traversal remain with the caller.
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void CopyRows(ReadOnlySpan<byte> input, Span<byte> output,
        int inputOffset, int outputOffset, int rowBytes, int rows,
        int inputStep, int outputStep, CancellationToken cancellation)
    {
        // Keep one to four final vectors, overlapping the prior bulk by at
        // most 63 bytes. Every access stays inside this independent row.
        var copied = (rowBytes - 1) / (4 * Vector512<byte>.Count) * (4 * Vector512<byte>.Count);
        var tail = (rowBytes - copied + Vector512<byte>.Count - 1) / Vector512<byte>.Count * Vector512<byte>.Count;
        var tailOffset = rowBytes - tail;
        while (true)
        {
            cancellation.ThrowIfCancellationRequested();
            var source = MemoryMarshal.Cast<byte, CopyBlock>(input.Slice(inputOffset, copied));
            var destination = MemoryMarshal.Cast<byte, CopyBlock>(output.Slice(outputOffset, copied));
            for (var i = 0; i < source.Length; i++)
            {
                ref readonly var from = ref source[i]; ref var to = ref destination[i];
                var a = from[0]; var b = from[1]; var c = from[2]; var d = from[3];
                to[0] = a; to[1] = b; to[2] = c; to[3] = d;
            }
            var tailSource = MemoryMarshal.Cast<byte, Vector512<byte>>(input.Slice(inputOffset + tailOffset, tail));
            var tailDestination = MemoryMarshal.Cast<byte, Vector512<byte>>(output.Slice(outputOffset + tailOffset, tail));
            for (var i = 0; i < tailSource.Length; i++) tailDestination[i] = tailSource[i];
            if (--rows == 0) break;
            inputOffset += inputStep; outputOffset += outputStep;
        }
    }

    // A self-copy shares its backing and packed-row step. Keep snapshot
    // traversal and checkpoints while passing that execution state once.
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void CopySelfRows(Span<byte> pixels,
        int inputOffset, int outputOffset, int rowBytes, int rows,
        int rowStep, CancellationToken cancellation)
    {
        // Keep one to four final vectors, overlapping the prior bulk by at
        // most 63 bytes. Every access stays inside this independent row.
        var copied = (rowBytes - 1) / (4 * Vector512<byte>.Count) * (4 * Vector512<byte>.Count);
        var tail = (rowBytes - copied + Vector512<byte>.Count - 1) / Vector512<byte>.Count * Vector512<byte>.Count;
        var tailOffset = rowBytes - tail;
        while (true)
        {
            cancellation.ThrowIfCancellationRequested();
            var source = MemoryMarshal.Cast<byte, CopyBlock>(pixels.Slice(inputOffset, copied));
            var destination = MemoryMarshal.Cast<byte, CopyBlock>(pixels.Slice(outputOffset, copied));
            for (var i = 0; i < source.Length; i++)
            {
                ref readonly var from = ref source[i]; ref var to = ref destination[i];
                var a = from[0]; var b = from[1]; var c = from[2]; var d = from[3];
                to[0] = a; to[1] = b; to[2] = c; to[3] = d;
            }
            var tailSource = MemoryMarshal.Cast<byte, Vector512<byte>>(pixels.Slice(inputOffset + tailOffset, tail));
            var tailDestination = MemoryMarshal.Cast<byte, Vector512<byte>>(pixels.Slice(outputOffset + tailOffset, tail));
            for (var i = 0; i < tailSource.Length; i++) tailDestination[i] = tailSource[i];
            if (--rows == 0) break;
            inputOffset += rowStep; outputOffset += rowStep;
        }
    }

    // Fixed offsets retain the four-load snapshot before any group store.
    [InlineArray(4)]
    private struct CopyBlock
    {
        private Vector512<byte> value;
    }
}
