using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

// Q30 plans are unchanged. Double/FMA sums propose pixels, and a certified
// interval must fit strictly inside each rounding bin. Uncertain pixels use
// the original signed64/Int128 computation. No approximate pixel is returned.
internal static class LanczosFmaResize
{
    private const double Q60 = 1152921504606846976d;
    private const double Guard = 1d / 268435456;

    internal static bool CanUse(ImageBuffer source, int width, int height, LanczosPlan horizontal,
        LanczosPlan vertical, OperationContext context)
    {
        if (!Avx512F.IsSupported || !Avx512DQ.IsSupported || !Avx512DQ.VL.IsSupported || !Avx2.IsSupported || !Fma.IsSupported || !Sse41.IsSupported) return false;
        var slots = Math.Min(vertical.MaximumCount, 64); var channels = width * 4L;
        var rows = channels * slots;
        // Keep the original local workspace boundary. Selection precedes any
        // new reservation; shared refusal is never caught to try another path.
        var originalBytes = 128 + rows * 8 + channels * 16 + slots * 4L;
        var pixels = (long)width * height;
        if (rows * 8 > Array.MaxLength || channels * 16 > Array.MaxLength
            || originalBytes + pixels * (source.BitDepth / 8) * 4 > context.RemainingWorkingBytes) return false;
        // Every row-cache miss, vertical pass and possible exact pixel is covered.
        // Int128 avoids overflow for geometry whose allocation/work will refuse.
        var fallback = ((Int128)horizontal.MaximumCount * vertical.MaximumCount + vertical.MaximumCount + 1) * 4;
        var work = (Int128)channels * (2L * height + vertical.Weights.LongLength)
            + vertical.Weights.LongLength * (1 + (Int128)horizontal.Weights.LongLength * 4 + channels) + pixels * fallback;
        return work <= context.RemainingWork;
    }

    internal static ImageBuffer Apply(ImageBuffer source, int width, int height, LanczosPlan horizontal,
        LanczosPlan vertical, OperationContext context)
    {
        var slots = Math.Min(vertical.MaximumCount, 64); var channels = width * 4;
        var bytes = 128 + channels * (long)slots * 8 + channels * 8L + slots * 4L;
        context.Reserve(bytes);
        try
        {
            var rows = new double[channels * slots]; var sums = new double[channels];
            var rowIds = new int[slots]; Array.Fill(rowIds, -1);
            var result = ImageBuffer.AllocateForOverwrite(width, height, source.BitDepth, context);
            try
            {
                var maximum = source.BitDepth == 8 ? 255 : 65535;
                var errors = Errors(vertical.MaximumCount);
                var numeratorError = errors.Rgb + 65535d * errors.Alpha;
                for (var y = 0; y < height; y++)
                {
                    context.Work(channels); sums.AsSpan().Clear(); var window = vertical.Windows[y];
                    for (var tap = 0; tap < window.Count; tap++)
                    {
                        context.Work(1); var weight = vertical.Weights[window.Offset + tap]; if (weight == 0) continue;
                        var sy = window.First + tap; var slot = sy % slots;
                        var row = rows.AsSpan(slot * channels, channels);
                        if (rowIds[slot] != sy) { Horizontal(source, sy, horizontal, row, context); rowIds[slot] = sy; }
                        context.Work(channels); Accumulate(row, sums, weight, context);
                    }
                    context.Work(channels);
                    Span<byte> output8 = default; Span<ushort> output16 = default;
                    if (source.BitDepth == 8) output8 = result.GetRow8(y); else output16 = result.GetRow16(y);
                    for (var x = 0; x < width; x++)
                    {
                        if ((x & 1023) == 0) context.Cancellation.ThrowIfCancellationRequested();
                        var at = x * 4;
                        if (!TryNormalize(Vector256.Create(sums.AsSpan(at, 4)), numeratorError, errors.Alpha, maximum, out var pixel))
                            pixel = ExactPixel(source, x, y, horizontal, vertical, context);
                        if (source.BitDepth == 8)
                        {
                            output8[at] = (byte)pixel.GetElement(0); output8[at + 1] = (byte)pixel.GetElement(1);
                            output8[at + 2] = (byte)pixel.GetElement(2); output8[at + 3] = (byte)pixel.GetElement(3);
                        }
                        else
                        {
                            output16[at] = (ushort)pixel.GetElement(0); output16[at + 1] = (ushort)pixel.GetElement(1);
                            output16[at + 2] = (ushort)pixel.GetElement(2); output16[at + 3] = (ushort)pixel.GetElement(3);
                        }
                    }
                }
                result.Orientation = source.Orientation; return result;
            }
            catch { result.Dispose(); throw; }
        }
        finally { context.Release(bytes); }
    }

    // B=sum(abs(Q30 weights))<=2^31 and M<=65535. The original horizontal
    // kernel is exact: RGB magnitude <2^63 and alpha <2^47. One conversion
    // to double contributes ERH<=2^9; horizontal alpha remains exact. Vertical
    // FMA prefixes are <2^94 RGB/<2^78 alpha with errors <=2^41/2^25 per tap.
    // Thus ER=(1+2*nV)*2^40 and EA=nV*2^25. Int32.MaxValue tap counts still
    // leave approximate prefixes strictly inside the certified exponent bounds.
    internal static (double Rgb, double Alpha) Errors(int verticalCount)
        => ((1 + 2L * verticalCount) * 1099511627776d, verticalCount * 33554432d);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool TryNormalize(Vector256<double> value, double numeratorError, double alphaError,
        int maximum, out Vector128<int> pixel)
    {
        pixel = default; var alpha = value.GetElement(3);
        if (alpha < Q60 / 2 - alphaError) return true;
        if (alpha < Q60 / 2 + alphaError) return false;
        var divisor = Vector256.Create(alpha, alpha, alpha, Q60);
        var estimate = Avx.Min(Avx.Max(Avx.Divide(value, divisor), Vector256<double>.Zero), Vector256.Create((double)maximum));
        var rounded = Avx.RoundToNearestInteger(estimate);
        var distance = Avx.AndNot(Vector256.Create(-0d), Avx.Subtract(estimate, rounded));
        // Clipping to [0,M] bounds quotient perturbation by
        // (ER+M*EA)/(Ahat-EA). The stored numerator uses the larger M=65535.
        // Its <=49 significant bits are exact. Guard covers subsequent double
        // division/comparison errors (the theoretical radius is <20480).
        var radius = numeratorError / (alpha - alphaError) + Guard;
        var margins = Vector256.Create(.5 - radius, .5 - radius, .5 - radius, .5 - alphaError / Q60 - Guard);
        if (Avx.MoveMask(Avx.CompareLessThan(distance, margins)) != 15) return false;
        pixel = Avx.ConvertToVector128Int32WithTruncation(rounded); return true;
    }

    internal static void Horizontal(ImageBuffer source, int y, LanczosPlan plan, Span<double> output, OperationContext context)
    {
        // Reuse one admitted backing store through two private views. No full
        // frame or extra row copy: exact signed sums are converted in place.
        var integer = MemoryMarshal.Cast<double, long>(output);
        LanczosResize.Horizontal(source, y, plan, integer, context);
        context.Work(output.Length);
        var i = 0;
        for (; i <= output.Length - 8; i += 8)
        {
            if ((i & 4095) == 0) context.Cancellation.ThrowIfCancellationRequested();
            Avx512DQ.ConvertToVector512Double(Vector512.Create(integer.Slice(i, 8))).CopyTo(output.Slice(i, 8));
        }
        if (i < output.Length)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            Avx512DQ.VL.ConvertToVector256Double(Vector256.Create(integer.Slice(i, 4))).CopyTo(output.Slice(i, 4));
        }
    }

    internal static void Accumulate(ReadOnlySpan<double> row, Span<double> sums, int weight, OperationContext context)
    {
        var factor = Vector512.Create((double)weight); var i = 0;
        for (; i <= sums.Length - 8; i += 8)
        {
            if ((i & 4095) == 0) context.Cancellation.ThrowIfCancellationRequested();
            Avx512F.FusedMultiplyAdd(Vector512.Create(row.Slice(i, 8)), factor, Vector512.Create(sums.Slice(i, 8))).CopyTo(sums.Slice(i, 8));
        }
        if (i < sums.Length)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            Fma.MultiplyAdd(Vector256.Create(row.Slice(i, 4)), Vector256.Create((double)weight), Vector256.Create(sums.Slice(i, 4))).CopyTo(sums.Slice(i, 4));
        }
    }

    internal static Vector128<int> ExactPixel(ImageBuffer source, int x, int y, LanczosPlan horizontal,
        LanczosPlan vertical, OperationContext context)
    {
        var hw = horizontal.Windows[x]; var vw = vertical.Windows[y];
        context.Work(((long)hw.Count * vw.Count + vw.Count + 1) * 4);
        Int128 r = 0, g = 0, b = 0, a = 0;
        for (var v = 0; v < vw.Count; v++)
        {
            if ((v & 1023) == 0) context.Cancellation.ThrowIfCancellationRequested();
            var weight = vertical.Weights[vw.Offset + v];
            ReadOnlySpan<byte> input8 = default; ReadOnlySpan<ushort> input16 = default;
            if (source.BitDepth == 8) input8 = source.GetRow8(vw.First + v); else input16 = source.GetRow16(vw.First + v);
            long hr = 0, hg = 0, hb = 0, ha = 0;
            for (var h = 0; h < hw.Count; h++)
            {
                if ((h & 1023) == 0) context.Cancellation.ThrowIfCancellationRequested(); var at = (hw.First + h) * 4;
                var alpha = source.BitDepth == 8 ? input8[at + 3] : input16[at + 3];
                var weighted = (long)alpha * horizontal.Weights[hw.Offset + h];
                hr += weighted * (source.BitDepth == 8 ? input8[at] : input16[at]);
                hg += weighted * (source.BitDepth == 8 ? input8[at + 1] : input16[at + 1]);
                hb += weighted * (source.BitDepth == 8 ? input8[at + 2] : input16[at + 2]); ha += weighted;
            }
            r += (Int128)hr * weight; g += (Int128)hg * weight; b += (Int128)hb * weight; a += (Int128)ha * weight;
        }
        var maximum = source.BitDepth == 8 ? 255 : 65535;
        var alphaResult = LanczosResize.Round(a, (Int128)LanczosPlan.One * LanczosPlan.One, maximum);
        return alphaResult == 0 ? Vector128<int>.Zero : Vector128.Create(LanczosResize.Round(r, a, maximum),
            LanczosResize.Round(g, a, maximum), LanczosResize.Round(b, a, maximum), alphaResult);
    }
}
