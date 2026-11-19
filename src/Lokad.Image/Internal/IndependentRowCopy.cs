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

    // This hint only changes the interior schedule. Checked rows and
    // unaligned-safe transfers remain valid if the backing moves.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int AddressPhase(in byte value) =>
        (int)(Unsafe.ByteOffset(in Unsafe.NullRef<byte>(), in value) & 63);

    // Checking both endpoint rows also bounds every intermediate row.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void RequireStagedFootprints(int inputLength, int outputLength,
        int inputOffset, int outputOffset, int rowBytes, int rows, int inputStep, int outputStep)
    {
        if (rowBytes < 512 || rows < 1 || !Fits(inputLength, inputOffset, inputStep)
            || !Fits(outputLength, outputOffset, outputStep))
            throw new ArgumentOutOfRangeException(nameof(rowBytes));

        bool Fits(int length, int offset, int step)
        {
            var lastRow = offset + (long)(rows - 1) * step;
            return Math.Min(offset, lastRow) >= 0 && Math.Max(offset, lastRow) + rowBytes <= length;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void CopyStagedAlignedRows(ReadOnlySpan<byte> input, Span<byte> output,
        int inputOffset, int outputOffset, int rowBytes, int rows,
        int inputStep, int outputStep, CancellationToken cancellation)
    {
        const int groupBytes = 4 * 64;
        RequireStagedFootprints(input.Length, output.Length, inputOffset, outputOffset,
            rowBytes, rows, inputStep, outputStep);
        ref var source = ref Unsafe.Add(ref MemoryMarshal.GetReference(input), inputOffset);
        ref var destination = ref Unsafe.Add(ref MemoryMarshal.GetReference(output), outputOffset);
        var last = rowBytes - Vector512<byte>.Count;
        ref var sourceEnd = ref Unsafe.Add(ref source, last);
        ref var destinationEnd = ref Unsafe.Add(ref destination, last);
        nint lead = 64 - AddressPhase(in destination);
        nint stepPhase = outputStep & 63;
        nint inputAdvance = inputStep;
        nint outputAdvance = outputStep;
        nint rowLength = rowBytes;
        while (true)
        {
            cancellation.ThrowIfCancellationRequested();
            var head = Vector512.LoadUnsafe(ref source);
            var end = Vector512.LoadUnsafe(ref sourceEnd);
            ref var from = ref Unsafe.Add(ref source, lead);
            ref var to = ref Unsafe.Add(ref destination, lead);
            // Start with one aligned group and use ordinary full groups
            // through the last complete line. The first advance can overlap
            // part of the first group, avoiding a separate remainder path.
            var interior = (rowLength - lead) & -64;
            var advance = ((interior - 1) & 192) + 64;
            ref var lastGroup = ref Unsafe.Add(ref from, interior - groupBytes);
            do
            {
                var a = Vector512.LoadUnsafe(ref from);
                var b = Vector512.LoadUnsafe(ref from, 64);
                var c = Vector512.LoadUnsafe(ref from, 128);
                var d = Vector512.LoadUnsafe(ref from, 192);
                a.StoreUnsafe(ref to);
                b.StoreUnsafe(ref to, 64);
                c.StoreUnsafe(ref to, 128);
                d.StoreUnsafe(ref to, 192);
                from = ref Unsafe.Add(ref from, advance);
                to = ref Unsafe.Add(ref to, advance);
                advance = groupBytes;
            } while (!Unsafe.IsAddressGreaterThan(ref from, ref lastGroup));

            end.StoreUnsafe(ref destinationEnd);
            head.StoreUnsafe(ref destination);
            if (--rows == 0) break;
            source = ref Unsafe.Add(ref source, inputAdvance);
            sourceEnd = ref Unsafe.Add(ref sourceEnd, inputAdvance);
            destination = ref Unsafe.Add(ref destination, outputAdvance);
            destinationEnd = ref Unsafe.Add(ref destinationEnd, outputAdvance);
            lead = ((lead - 1 - stepPhase) & 63) + 1;
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
