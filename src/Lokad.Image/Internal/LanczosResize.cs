using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

// Separable, alpha-aware fixed-point resampling. No intermediate rounding:
// horizontal Int64 rows feed Int128 vertical sums, followed by one exact divide.
internal static class LanczosResize
{
    internal static ImageBuffer Apply(ImageBuffer source, int width, int height, OperationContext context)
    {
        using var horizontal = LanczosPlan.Create(source.Width, width, context);
        using var vertical = LanczosPlan.Create(source.Height, height, context);
        var slots = Math.Min(vertical.MaximumCount, 64);
        var channels = width * 4L; var rowElements = channels * slots;
        var bytes = 128 + rowElements * 8 + channels * 16 + slots * 4L;
        if (rowElements * 8 > Array.MaxLength || channels * 16 > Array.MaxLength)
            throw new ImageLimitException(ImageLimitKind.BufferLength, "Lanczos row storage exceeds the managed limit.");
        context.Reserve(bytes);
        try
        {
            var rows = new long[(int)rowElements]; var rowIds = new int[slots]; Array.Fill(rowIds, -1);
            var sums = new Int128[(int)channels];
            var result = ImageBuffer.AllocateForOverwrite(width, height, source.BitDepth, context);
            try
            {
                var maximum = source.BitDepth == 8 ? 255 : 65535;
                for (var y = 0; y < height; y++)
                {
                    context.Work(channels); sums.AsSpan().Clear();
                    var window = vertical.Windows[y];
                    for (var tap = 0; tap < window.Count; tap++)
                    {
                        context.Work(1); var weight = vertical.Weights[window.Offset + tap]; if (weight == 0) continue;
                        var sy = window.First + tap; var slot = sy % slots;
                        var row = rows.AsSpan(slot * (int)channels, (int)channels);
                        if (rowIds[slot] != sy) { Horizontal(source, sy, horizontal, row, context); rowIds[slot] = sy; }
                        context.Work(channels);
                        AccumulateVertical(row, sums, weight, context);
                    }
                    context.Work(channels);
                    Span<byte> destination8 = default; Span<ushort> destination16 = default;
                    if (source.BitDepth == 8) destination8 = result.GetRow8(y); else destination16 = result.GetRow16(y);
                    for (var x = 0; x < width; x++)
                    {
                        if ((x & 1023) == 0) context.Cancellation.ThrowIfCancellationRequested();
                        var at = x * 4; var pixel = Normalize(sums, at, maximum);
                        var r = pixel.GetElement(0); var g = pixel.GetElement(1);
                        var b = pixel.GetElement(2); var a = pixel.GetElement(3);
                        if (source.BitDepth == 8)
                        {
                            destination8[at] = (byte)r; destination8[at + 1] = (byte)g;
                            destination8[at + 2] = (byte)b; destination8[at + 3] = (byte)a;
                        }
                        else
                        {
                            destination16[at] = (ushort)r; destination16[at + 1] = (ushort)g;
                            destination16[at + 2] = (ushort)b; destination16[at + 3] = (ushort)a;
                        }
                    }
                }
                result.Orientation = source.Orientation; return result;
            }
            catch { result.Dispose(); throw; }
        }
        finally { context.Release(bytes); }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<int> Normalize(ReadOnlySpan<Int128> sums, int index, int maximum)
    {
        if (Avx2.IsSupported && Avx512DQ.VL.IsSupported)
        {
            var limbs = MemoryMarshal.Cast<Int128, long>(sums);
            var low = Avx512DQ.VL.ConvertToVector256Double(Vector256.Create(limbs.Slice(index, 4)));
            var high = Avx512DQ.VL.ConvertToVector256Double(Vector256.Create(limbs.Slice(sums.Length + index, 4)));
            var value = Avx.Add(Avx.Multiply(high, Vector256.Create(4294967296d)), low);
            var denominator = value.GetElement(3);
            // Certified limb bounds give conversion/combination error <2^44
            // for RGB and <2^28 for alpha. Below half Q60, all samples clear.
            const double alphaError = 268435456d, half = 576460752303423488d;
            if (denominator < half - alphaError) return Vector128<int>.Zero;
            if (denominator >= half + alphaError)
            {
                var divisor = Vector256.Create(denominator, denominator, denominator, 1152921504606846976d);
                var estimate = Avx.Min(Avx.Max(Avx.Divide(value, divisor), Vector256<double>.Zero), Vector256.Create((double)maximum));
                var rounded = Avx.RoundToNearestInteger(estimate);
                var distance = Avx.AndNot(Vector256.Create(-0d), Avx.Subtract(estimate, rounded));
                // Clipping is Lipschitz in numerator and positive denominator.
                // Including division/comparison errors, every clipped quotient
                // differs by <2^-10. Uncertain bins retain exact arithmetic.
                if (Avx.MoveMask(Avx.CompareLessThan(distance, Vector256.Create(.5 - 1d / 1024))) == 15)
                    return Avx.ConvertToVector128Int32WithTruncation(rounded);
            }
        }
        var alpha = ReadSum(sums, index + 3);
        var a = Round(alpha, (Int128)LanczosPlan.One * LanczosPlan.One, maximum);
        if (a == 0) return Vector128<int>.Zero;
        return Vector128.Create(Round(ReadSum(sums, index), alpha, maximum),
            Round(ReadSum(sums, index + 1), alpha, maximum), Round(ReadSum(sums, index + 2), alpha, maximum), a);
    }

    internal static void AccumulateVertical(ReadOnlySpan<long> row, Span<Int128> sums, int weight, OperationContext context)
    {
        if (Avx2.IsSupported)
        {
            // Each plan has sum(abs(weights)) <= 2^31. Split row = hi*2^32+lo
            // with signed Int32 limbs. The certified horizontal bound keeps hi
            // in Int32 even at 16 bits; either limb's every partial sum fits
            // Int64 (absolute value <= 2^62). Recombine exactly only at output.
            // Two contiguous planes reuse the original 16 bytes per channel.
            var limbs = MemoryMarshal.Cast<Int128, long>(sums);
            var low = limbs[..sums.Length]; var high = limbs[sums.Length..];
            var i = 0;
            if (Avx512F.IsSupported && sums.Length >= 8)
            {
                // Eight independent channels retain the same per-lane order
                // and prefix bounds; the four-channel tail remains below.
                var factor512 = Vector512.Create(weight);
                for (; i <= sums.Length - 8; i += 8)
                {
                    if ((i & 4095) == 0) context.Cancellation.ThrowIfCancellationRequested();
                    var value = Vector512.Create(row.Slice(i, 8));
                    var upper = Avx512F.Add(Avx512F.ShiftRightLogical(value.AsUInt64(), 32).AsInt32(),
                        Avx512F.ShiftRightLogical(value.AsUInt32(), 31).AsInt32());
                    Avx512F.Add(Vector512.Create(low.Slice(i, 8)), Avx512F.Multiply(value.AsInt32(), factor512)).CopyTo(low.Slice(i, 8));
                    Avx512F.Add(Vector512.Create(high.Slice(i, 8)), Avx512F.Multiply(upper, factor512)).CopyTo(high.Slice(i, 8));
                }
            }
            var factor = Vector256.Create(weight);
            for (; i < sums.Length; i += 4)
            {
                if ((i & 4095) == 0) context.Cancellation.ThrowIfCancellationRequested();
                var value = Vector256.Create(row.Slice(i, 4));
                var upper = Avx2.Add(Avx2.ShiftRightLogical(value.AsUInt64(), 32).AsInt32(),
                    Avx2.ShiftRightLogical(value.AsUInt32(), 31).AsInt32());
                Avx2.Add(Vector256.Create(low.Slice(i, 4)), Avx2.Multiply(value.AsInt32(), factor)).CopyTo(low.Slice(i, 4));
                Avx2.Add(Vector256.Create(high.Slice(i, 4)), Avx2.Multiply(upper, factor)).CopyTo(high.Slice(i, 4));
            }
        }
        else
            for (var i = 0; i < sums.Length; i++)
            { if ((i & 4095) == 0) context.Cancellation.ThrowIfCancellationRequested(); sums[i] += (Int128)row[i] * weight; }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Int128 ReadSum(ReadOnlySpan<Int128> sums, int index)
    {
        if (!Avx2.IsSupported) return sums[index];
        var limbs = MemoryMarshal.Cast<Int128, long>(sums);
        return ((Int128)limbs[sums.Length + index] << 32) + limbs[index];
    }

    internal static void Horizontal(ImageBuffer source, int y, LanczosPlan plan, Span<long> output, OperationContext context)
    {
        ReadOnlySpan<byte> input8 = default; ReadOnlySpan<ushort> input16 = default;
        if (source.BitDepth == 8) input8 = source.GetRow8(y); else input16 = source.GetRow16(y);
        if (source.BitDepth == 8 && Avx2.IsSupported)
        { Horizontal8(input8, plan, output, context); return; }
        for (var x = 0; x < plan.Windows.Length; x++)
        {
            var window = plan.Windows[x]; context.Work(window.Count * 4L);
            if (Avx2.IsSupported && Avx512DQ.VL.IsSupported)
            {
                // UInt32 premultiplied samples can exceed Int32 at 16 bits.
                // Zero extension before signed64 weighting is essential. The
                // axis magnitude bound keeps every partial sum inside Int64.
                var sum = Vector256<long>.Zero;
                for (var i = 0; i < window.Count; i++)
                {
                    if ((i & 1023) == 0) context.Cancellation.ThrowIfCancellationRequested();
                    var at = (window.First + i) * 4;
                    var samples = source.BitDepth == 8
                        ? Sse41.ConvertToVector128Int32(Vector128.CreateScalar(MemoryMarshal.Read<uint>(input8.Slice(at, 4))).AsByte())
                        : Sse41.ConvertToVector128Int32(Vector128.CreateScalar(MemoryMarshal.Read<ulong>(MemoryMarshal.AsBytes(input16.Slice(at, 4)))).AsUInt16());
                    var premultiplied = Sse41.MultiplyLow(Sse41.Insert(samples, 1, 3), Sse2.Shuffle(samples, 255)).AsUInt32();
                    var weight = Vector256.Create((long)plan.Weights[window.Offset + i]);
                    sum = Avx2.Add(sum, Avx512DQ.VL.MultiplyLow(Avx2.ConvertToVector256Int64(premultiplied), weight));
                }
                sum.CopyTo(output.Slice(x * 4, 4)); continue;
            }
            long r = 0, g = 0, b = 0, alpha = 0;
            for (var i = 0; i < window.Count; i++)
            {
                if ((i & 1023) == 0) context.Cancellation.ThrowIfCancellationRequested();
                var weight = plan.Weights[window.Offset + i]; var at = (window.First + i) * 4;
                int a, red, green, blue;
                if (source.BitDepth == 8) { a = input8[at + 3]; red = input8[at]; green = input8[at + 1]; blue = input8[at + 2]; }
                else { a = input16[at + 3]; red = input16[at]; green = input16[at + 1]; blue = input16[at + 2]; }
                r += (long)a * red * weight; g += (long)a * green * weight; b += (long)a * blue * weight;
                alpha += (long)a * weight;
            }
            var destination = x * 4; output[destination] = r; output[destination + 1] = g;
            output[destination + 2] = b; output[destination + 3] = alpha;
        }
    }
    private static void Horizontal8(ReadOnlySpan<byte> input, LanczosPlan plan, Span<long> output, OperationContext context)
    {
        for (var x = 0; x < plan.Windows.Length; x++)
        {
            var window = plan.Windows[x]; context.Work(window.Count * 4L);
            var sum = Vector256<long>.Zero;
            for (var i = 0; i < window.Count; i++)
            {
                if ((i & 1023) == 0) context.Cancellation.ThrowIfCancellationRequested();
                var samples = Sse41.ConvertToVector128Int32(Vector128.CreateScalar(MemoryMarshal.Read<uint>(input.Slice((window.First + i) * 4, 4))).AsByte());
                var premultiplied = Sse41.MultiplyLow(Sse41.Insert(samples, 1, 3), Sse2.Shuffle(samples, 255));
                // Byte products are <=65025, so signed32 operands suffice.
                // Even lanes of the widened values give four exact Int64
                // products; the axis bound keeps all prefixes below2^47.
                var values = Avx2.ConvertToVector256Int64(premultiplied).AsInt32();
                sum = Avx2.Add(sum, Avx2.Multiply(values, Vector256.Create(plan.Weights[window.Offset + i])));
            }
            sum.CopyTo(output.Slice(x * 4, 4));
        }
    }

    internal static int Round(Int128 numerator, Int128 denominator, int maximum)
    {
        if (numerator <= 0) return 0;
        if (numerator >= denominator * maximum) return maximum;
        // The image arithmetic fits 96 bits. Drop 32 low bits to cheaply
        // estimate using UInt64; no discarded bit decides a pixel. Clamp the
        // candidate so its product fits the already checked sample range, then
        // certify its exact remainder. This also handles wider synthetic inputs.
        var projectedDenominator = (ulong)(denominator >> 32);
        if (projectedDenominator != 0)
        {
            var estimate = (double)(ulong)(numerator >> 32) / projectedDenominator;
            // In this domain, truncation contributes at most
            // (maximum+1)/(2^27-1); binary64 conversion/division add <2^-34.
            // Their sum is <2^-10. Accept only a complete rounding bin;
            // uncertain half boundaries keep the exact remainder below.
            if (maximum <= 65535 && denominator >= (Int128)1 << 59 && denominator < (Int128)1 << 80)
            {
                var rounded = (int)(estimate + .5);
                if (Math.Abs(estimate - rounded) < .5 - 1d / 1024) return rounded;
            }
            var quotient = Math.Clamp((int)estimate, 0, maximum);
            var remainder = numerator - denominator * quotient;
            if (remainder < 0) { quotient--; remainder += denominator; }
            else if (remainder >= denominator) { quotient++; remainder -= denominator; }
            if (remainder >= 0 && remainder < denominator)
                return quotient + (remainder * 2 >= denominator ? 1 : 0);
        }
        var (exact, rest) = Int128.DivRem(numerator, denominator);
        return (int)(exact + (rest * 2 >= denominator ? 1 : 0));
    }
}
