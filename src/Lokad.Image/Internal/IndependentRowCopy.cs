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
            var source = MemoryMarshal.Cast<byte, Vector512<byte>>(input);
            var destination = MemoryMarshal.Cast<byte, Vector512<byte>>(output);
            var i = 0;
            for (; i <= source.Length - 4; i += 4)
            {
                var a = source[i]; var b = source[i + 1];
                var c = source[i + 2]; var d = source[i + 3];
                destination[i] = a; destination[i + 1] = b;
                destination[i + 2] = c; destination[i + 3] = d;
            }
            for (; i < source.Length; i++) destination[i] = source[i];
            var copied = source.Length * Vector512<byte>.Count;
            input[copied..].CopyTo(output[copied..]);
        }
        else input.CopyTo(output);
    }
}
