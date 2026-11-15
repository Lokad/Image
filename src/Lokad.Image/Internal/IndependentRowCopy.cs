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

    // Fixed offsets retain the four-load snapshot before any group store.
    [InlineArray(4)]
    private struct CopyBlock
    {
        private Vector512<byte> value;
    }
}
