using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

// RFC 9649 section 3.5.1 applied to normalized ALPH samples (A/R/B zero).
// Rows remain borrowed from the shared predictor scheduler. No state escapes.
internal static class Vp8lGreenResiduals
{
    private const uint Green = 0xff00;

    internal static void Row(ReadOnlySpan<uint> current, ReadOnlySpan<uint> upper,
        ReadOnlySpan<byte> modes, int bits, bool firstRow, Span<uint> destination, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        destination[0] = firstRow ? current[0] | 0x01000000u : Difference(current[0], upper[0]);
        if (firstRow)
        {
            Copy(current[1..], current[..^1], destination[1..], cancellation);
            return;
        }
        var width = current.Length;
        for (var x = 1; x < width - 1;)
        {
            var end = Math.Min(width - 1, ((x >> bits) + 1) << bits);
            var count = end - x;
            Run(modes[x >> bits], current.Slice(x, count), current.Slice(x - 1, count),
                upper.Slice(x, count), upper.Slice(x - 1, count), upper.Slice(x + 1, count),
                destination.Slice(x, count), cancellation);
            x = end;
        }
        if (width > 1)
        {
            // The last pixel's top-right is the original current-row origin.
            cancellation.ThrowIfCancellationRequested();
            var x = width - 1; var mode = modes[x >> bits];
            var predicted = Predict(mode, (int)(current[x - 1] >> 8), (int)(upper[x] >> 8),
                (int)(upper[x - 1] >> 8), (int)(current[0] >> 8));
            destination[x] = Difference(current[x], (uint)predicted << 8) | (mode == 0 ? 0x01000000u : 0);
        }
    }

    private static void Run(int mode, ReadOnlySpan<uint> current, ReadOnlySpan<uint> left,
        ReadOnlySpan<uint> top, ReadOnlySpan<uint> corner, ReadOnlySpan<uint> right,
        Span<uint> destination, CancellationToken cancellation)
    {
        // Select once per tile-row run, not once per sample. Related modes
        // share the same checked kernel with their original neighbour spans.
        switch (mode)
        {
            case 1: Copy(current, left, destination, cancellation); return;
            case 2: Copy(current, top, destination, cancellation); return;
            case 3: Copy(current, right, destination, cancellation); return;
            case 4: Copy(current, corner, destination, cancellation); return;
            case 6: Average(current, left, corner, destination, cancellation); return;
            case 7: Average(current, left, top, destination, cancellation); return;
            case 8: Average(current, corner, top, destination, cancellation); return;
            case 9: Average(current, top, right, destination, cancellation); return;
            case 12: Clamp(current, left, top, corner, destination, cancellation); return;
        }
        for (var i = 0; i < current.Length; i++)
        {
            cancellation.ThrowIfCancellationRequested();
            var predicted = Predict(mode, (int)(left[i] >> 8), (int)(top[i] >> 8),
                (int)(corner[i] >> 8), (int)(right[i] >> 8));
            destination[i] = Difference(current[i], (uint)predicted << 8) | (mode == 0 ? 0x01000000u : 0);
        }
    }

    private static uint Difference(uint actual, uint predicted) => unchecked(actual - predicted) & Green;

    private static Vector256<uint> Load(ReadOnlySpan<uint> values, int at)
        => MemoryMarshal.Read<Vector256<uint>>(MemoryMarshal.AsBytes(values.Slice(at, 8)));

    private static void Copy(ReadOnlySpan<uint> current, ReadOnlySpan<uint> prior,
        Span<uint> destination, CancellationToken cancellation)
    {
        var i = 0;
        // A token that cannot be cancelled has no observable per-pixel poll.
        // Live tokens keep the scalar checkpoint at every original sample.
        if (Avx2.IsSupported && !cancellation.CanBeCanceled)
            for (; i <= current.Length - 8; i += 8)
                Avx2.And(Avx2.Subtract(Load(current, i), Load(prior, i)), Vector256.Create(Green)).CopyTo(destination[i..]);
        for (; i < current.Length; i++)
        {
            cancellation.ThrowIfCancellationRequested();
            destination[i] = Difference(current[i], prior[i]);
        }
    }

    private static void Average(ReadOnlySpan<uint> current, ReadOnlySpan<uint> first,
        ReadOnlySpan<uint> second, Span<uint> destination, CancellationToken cancellation)
    {
        var i = 0;
        if (Avx2.IsSupported && !cancellation.CanBeCanceled)
            for (; i <= current.Length - 8; i += 8)
            {
                // Mask the half-bit before subtraction, so an odd sum cannot
                // borrow from green through the unused lower byte.
                var predicted = Avx2.And(Avx2.ShiftRightLogical(Avx2.Add(Load(first, i), Load(second, i)), 1), Vector256.Create(Green));
                Avx2.And(Avx2.Subtract(Load(current, i), predicted), Vector256.Create(Green)).CopyTo(destination[i..]);
            }
        for (; i < current.Length; i++)
        {
            cancellation.ThrowIfCancellationRequested();
            destination[i] = Difference(current[i], ((first[i] + second[i]) >> 1) & Green);
        }
    }

    private static void Clamp(ReadOnlySpan<uint> current, ReadOnlySpan<uint> left,
        ReadOnlySpan<uint> top, ReadOnlySpan<uint> corner, Span<uint> destination, CancellationToken cancellation)
    {
        var i = 0;
        if (Avx2.IsSupported && !cancellation.CanBeCanceled)
            for (; i <= current.Length - 8; i += 8)
            {
                var predicted = Avx2.Subtract(Avx2.Add(Load(left, i).AsInt32(), Load(top, i).AsInt32()), Load(corner, i).AsInt32());
                predicted = Avx2.Min(Avx2.Max(predicted, Vector256<int>.Zero), Vector256.Create((int)Green));
                Avx2.And(Avx2.Subtract(Load(current, i), predicted.AsUInt32()), Vector256.Create(Green)).CopyTo(destination[i..]);
            }
        for (; i < current.Length; i++)
        {
            cancellation.ThrowIfCancellationRequested();
            destination[i] = Difference(current[i], (uint)Math.Clamp((int)left[i] + (int)top[i] - (int)corner[i], 0, (int)Green));
        }
    }

    // The scalar fallback retains all fourteen predictors, including signed
    // division toward zero in mode13. Normalized neighbours are in0..255.
    internal static int Predict(int mode, int left, int top, int corner, int right)
    {
        var average = (left + top) / 2;
        return mode switch
        {
            0 => 0, 1 => left, 2 => top, 3 => right, 4 => corner,
            5 => ((left + right) / 2 + top) / 2,
            6 => (left + corner) / 2, 7 => average, 8 => (corner + top) / 2,
            9 => (top + right) / 2, 10 => ((left + corner) / 2 + (top + right) / 2) / 2,
            11 => Math.Abs(top - corner) < Math.Abs(left - corner) ? left : top,
            12 => Math.Clamp(left + top - corner, 0, 255),
            13 => Math.Clamp(average + (average - corner) / 2, 0, 255),
            _ => throw new ImageDataException("Invalid VP8L predictor.")
        };
    }
}

