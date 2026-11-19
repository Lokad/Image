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

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void CopySnapshotWordRows(ReadOnlySpan<byte> input, Span<byte> output,
        int inputOffset, int outputOffset, int rowBytes, int rows,
        int inputStep, int outputStep)
    {
        const int groupBytes = 2048;
        if (rowBytes < groupBytes || (rowBytes & (groupBytes - 1)) != 0)
            throw new ArgumentOutOfRangeException(nameof(rowBytes));
        RequireStagedFootprints(input.Length, output.Length, inputOffset, outputOffset,
            rowBytes, rows, inputStep, outputStep);

        ref var source = ref Unsafe.Add(ref MemoryMarshal.GetReference(input), inputOffset);
        ref var destination = ref Unsafe.Add(ref MemoryMarshal.GetReference(output), outputOffset);
        nint sourceAdvance = inputStep;
        nint destinationAdvance = outputStep;
        if (rowBytes == groupBytes)
        {
            var pairs = rows >> 1;
            if ((rows & 1) != 0)
            {
                CopySnapshotWordGroup(ref source, ref destination);
                if (pairs == 0) return;
                source = ref Unsafe.Add(ref source, sourceAdvance);
                destination = ref Unsafe.Add(ref destination, destinationAdvance);
            }

            // Complete each row before loading the next. The single group
            // needs one pair test and no inner loop.
            while (true)
            {
                CopySnapshotWordGroup(ref source, ref destination);
                source = ref Unsafe.Add(ref source, sourceAdvance);
                destination = ref Unsafe.Add(ref destination, destinationAdvance);
                CopySnapshotWordGroup(ref source, ref destination);
                if (--pairs == 0) return;
                source = ref Unsafe.Add(ref source, sourceAdvance);
                destination = ref Unsafe.Add(ref destination, destinationAdvance);
            }
        }

        nint bytes = rowBytes;
        while (true)
        {
            nint offset = 0;
            do
            {
                ref var from = ref Unsafe.Add(ref source, offset);
                ref var to = ref Unsafe.Add(ref destination, offset);
                CopySnapshotWordGroup(ref from, ref to);
            } while ((offset += groupBytes) < bytes);
            if (--rows == 0) break;
            source = ref Unsafe.Add(ref source, sourceAdvance);
            destination = ref Unsafe.Add(ref destination, destinationAdvance);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CopySnapshotWordGroup(ref byte from, ref byte to)
    {
        var samples0 = Vector512.LoadUnsafe(ref from);
        var samples1 = Vector512.LoadUnsafe(ref from, 64);
        var samples2 = Vector512.LoadUnsafe(ref from, 128);
        var samples3 = Vector512.LoadUnsafe(ref from, 192);
        var samples4 = Vector512.LoadUnsafe(ref from, 256);
        var samples5 = Vector512.LoadUnsafe(ref from, 320);
        var samples6 = Vector512.LoadUnsafe(ref from, 384);
        var samples7 = Vector512.LoadUnsafe(ref from, 448);
        var samples8 = Vector512.LoadUnsafe(ref from, 512);
        var samples9 = Vector512.LoadUnsafe(ref from, 576);
        var samples10 = Vector512.LoadUnsafe(ref from, 640);
        var samples11 = Vector512.LoadUnsafe(ref from, 704);
        var samples12 = Vector512.LoadUnsafe(ref from, 768);
        var samples13 = Vector512.LoadUnsafe(ref from, 832);
        var samples14 = Vector512.LoadUnsafe(ref from, 896);
        var samples15 = Vector512.LoadUnsafe(ref from, 960);
        var samples16 = Vector512.LoadUnsafe(ref from, 1024);
        var samples17 = Vector512.LoadUnsafe(ref from, 1088);
        var samples18 = Vector512.LoadUnsafe(ref from, 1152);
        var samples19 = Vector512.LoadUnsafe(ref from, 1216);
        var samples20 = Vector512.LoadUnsafe(ref from, 1280);
        var samples21 = Vector512.LoadUnsafe(ref from, 1344);
        var samples22 = Vector512.LoadUnsafe(ref from, 1408);
        var samples23 = Vector512.LoadUnsafe(ref from, 1472);
        var samples24 = Vector512.LoadUnsafe(ref from, 1536);
        var samples25 = Vector512.LoadUnsafe(ref from, 1600);
        var samples26 = Vector512.LoadUnsafe(ref from, 1664);
        var samples27 = Vector512.LoadUnsafe(ref from, 1728);
        var samples28 = Vector512.LoadUnsafe(ref from, 1792);
        var samples29 = Vector512.LoadUnsafe(ref from, 1856);
        var samples30 = Vector512.LoadUnsafe(ref from, 1920);
        var samples31 = Vector512.LoadUnsafe(ref from, 1984);
        samples0.StoreUnsafe(ref to);
        samples1.StoreUnsafe(ref to, 64);
        samples2.StoreUnsafe(ref to, 128);
        samples3.StoreUnsafe(ref to, 192);
        samples4.StoreUnsafe(ref to, 256);
        samples5.StoreUnsafe(ref to, 320);
        samples6.StoreUnsafe(ref to, 384);
        samples7.StoreUnsafe(ref to, 448);
        samples8.StoreUnsafe(ref to, 512);
        samples9.StoreUnsafe(ref to, 576);
        samples10.StoreUnsafe(ref to, 640);
        samples11.StoreUnsafe(ref to, 704);
        samples12.StoreUnsafe(ref to, 768);
        samples13.StoreUnsafe(ref to, 832);
        samples14.StoreUnsafe(ref to, 896);
        samples15.StoreUnsafe(ref to, 960);
        samples16.StoreUnsafe(ref to, 1024);
        samples17.StoreUnsafe(ref to, 1088);
        samples18.StoreUnsafe(ref to, 1152);
        samples19.StoreUnsafe(ref to, 1216);
        samples20.StoreUnsafe(ref to, 1280);
        samples21.StoreUnsafe(ref to, 1344);
        samples22.StoreUnsafe(ref to, 1408);
        samples23.StoreUnsafe(ref to, 1472);
        samples24.StoreUnsafe(ref to, 1536);
        samples25.StoreUnsafe(ref to, 1600);
        samples26.StoreUnsafe(ref to, 1664);
        samples27.StoreUnsafe(ref to, 1728);
        samples28.StoreUnsafe(ref to, 1792);
        samples29.StoreUnsafe(ref to, 1856);
        samples30.StoreUnsafe(ref to, 1920);
        samples31.StoreUnsafe(ref to, 1984);
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
