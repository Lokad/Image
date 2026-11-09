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

    internal static bool CanUse(int bitDepth, int width, int height, LanczosPlan horizontal,
        LanczosPlan vertical, OperationContext context)
    {
        if (!Avx512F.IsSupported || !Avx512DQ.IsSupported || !Avx512DQ.VL.IsSupported || !Avx2.IsSupported || !Fma.IsSupported || !Sse41.IsSupported) return false;
        var slots = Math.Min(vertical.MaximumCount, 64); var channels = width * 4L;
        var rows = channels * slots;
        // Keep the original local workspace boundary. Selection precedes any
        // new reservation; shared refusal is never caught to try another path.
        var originalBytes = 128 + rows * 8 + channels * 16 + slots * 4L
            + (bitDepth == 16 ? 32 + horizontal.SourceLength * 16L : 0);
        var pixels = (long)width * height;
        if (rows * 8 > Array.MaxLength || channels * 16 > Array.MaxLength
            || originalBytes + pixels * (bitDepth / 8) * 4 > context.RemainingWorkingBytes) return false;
        return true;
    }

    internal static void WriteRow(ImageBuffer source, ImageBuffer result, int y, LanczosPlan horizontal,
        LanczosPlan vertical, ReadOnlySpan<double> sums, double numeratorError, double alphaError,
        int maximum, OperationContext context)
    {
        Span<byte> output8 = default; Span<ushort> output16 = default;
        if (maximum == 255) output8 = result.GetRow8(y); else output16 = result.GetRow16(y);
        var x = 0;
        for (; x <= result.Width - 2; x += 2)
        {
            if ((x & 1023) == 0) context.Cancellation.ThrowIfCancellationRequested();
            var at = x * 4;
            var certified = NormalizePair(Vector512.Create(sums.Slice(at, 8)), numeratorError, alphaError, maximum, out var pixels);
            if ((certified & 1) == 0) pixels = Vector256.Create(ExactPixel(source, x, y, horizontal, vertical, context), pixels.GetUpper());
            if ((certified & 2) == 0) pixels = Vector256.Create(pixels.GetLower(), ExactPixel(source, x + 1, y, horizontal, vertical, context));
            var words = Vector128.Narrow(pixels.GetLower().AsUInt32(), pixels.GetUpper().AsUInt32());
            if (maximum == 255) Vector128.Narrow(words, Vector128<ushort>.Zero).GetLower().CopyTo(output8.Slice(at, 8));
            else words.CopyTo(output16.Slice(at, 8));
        }
        if (x < result.Width)
        {
            context.Cancellation.ThrowIfCancellationRequested(); var at = x * 4;
            if (!TryNormalize(Vector256.Create(sums.Slice(at, 4)), numeratorError, alphaError, maximum, out var pixel))
                pixel = ExactPixel(source, x, y, horizontal, vertical, context);
            // Both certification and exact repair return samples in [0,maximum].
            // Narrow and publish the complete RGBA tail in one store.
            var words = Vector128.Narrow(pixel.AsUInt32(), Vector128<uint>.Zero);
            if (maximum == 255)
            {
                var bytes = Vector128.Narrow(words, Vector128<ushort>.Zero);
                MemoryMarshal.Write(output8.Slice(at, 4), bytes.AsUInt32().GetElement(0));
            }
            else words.GetLower().CopyTo(output16.Slice(at, 4));
        }
    }

    // B=sum(abs(Q30 weights))<=2^31 and M<=65535. Horizontal RGB prefixes
    // stay below2^63; four word FMA chains and three recombination adds have
    // ERH<=(nH+3)*512. Alpha prefixes are exactly represented integers <2^47.
    // Byte rows retain the exact integer producer and ERH<=512 conversion.
    // Vertical FMA adds <=2^41RGB/2^25alpha per tap. Even Int32.MaxValue counts
    // keep approximate prefixes <2^94/2^78; the original certificate is strict.
    internal static (double Rgb, double Alpha) Errors(int verticalCount, int horizontalCount)
        => (((horizontalCount == 0 ? 1L : horizontalCount + 3L) + 2L * verticalCount) * 1099511627776d,
            verticalCount * 33554432d);

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

    // The same certificate in two independent RGBA lane groups. The mask records
    // each pixel separately: one uncertain half never forces repair of the other.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int NormalizePair(Vector512<double> value, double numeratorError, double alphaError,
        int maximum, out Vector256<int> pixels)
    {
        var a0 = value.GetElement(3); var a1 = value.GetElement(7);
        if (a0 < Q60 / 2 + alphaError || a1 < Q60 / 2 + alphaError)
        {
            var first = TryNormalize(value.GetLower(), numeratorError, alphaError, maximum, out var p0);
            var second = TryNormalize(value.GetUpper(), numeratorError, alphaError, maximum, out var p1);
            pixels = Vector256.Create(p0, p1); return (first ? 1 : 0) | (second ? 2 : 0);
        }
        // The alpha quotients scale normal bounded values by an exact power of
        // two. Reuse those division lanes for the two existing certificate radii;
        // RGB/radius operands and the certificate's operation order stay exact.
        var alphaLanes = Vector512.Create(0L, 0, 0, -1, 0, 0, 0, -1).AsDouble();
        var alphaIndices = Vector512.Create(3L, 3, 3, 3, 7, 7, 7, 7);
        var alphas = Vector512.Shuffle(value, alphaIndices);
        var divisors = Avx512F.Subtract(alphas, Vector512.ConditionalSelect(alphaLanes, Vector512.Create(alphaError), Vector512<double>.Zero));
        var numerators = Vector512.ConditionalSelect(alphaLanes, Vector512.Create(numeratorError), value);
        var divided = Avx512F.Divide(numerators, divisors);
        var quotients = Vector512.ConditionalSelect(alphaLanes, Avx512F.Multiply(value, Vector512.Create(1d / Q60)), divided);
        var estimate = Avx512F.Min(Avx512F.Max(quotients, Vector512<double>.Zero), Vector512.Create((double)maximum));
        var rounded = Avx512F.RoundScale(estimate, 0);
        var distance = Avx512F.AndNot(Vector512.Create(long.MinValue), Avx512F.Subtract(estimate, rounded).AsInt64()).AsDouble();
        var radii = Vector512.Shuffle(divided, alphaIndices);
        var rgbMargins = Avx512F.Subtract(Vector512.Create(.5), Avx512F.Add(radii, Vector512.Create(Guard)));
        var ma = .5 - alphaError / Q60 - Guard;
        var margins = Vector512.ConditionalSelect(alphaLanes, Vector512.Create(ma), rgbMargins);
        var mask = Vector512.ExtractMostSignificantBits(Avx512F.CompareLessThan(distance, margins));
        pixels = Avx512F.ConvertToVector256Int32WithTruncation(rounded);
        return ((mask & 15) == 15 ? 1 : 0) | ((mask & 240) == 240 ? 2 : 0);
    }

    internal static void Horizontal(ImageBuffer source, int y, LanczosPlan plan, Span<double> output,
        Span<uint> products, OperationContext context)
    {
        if (source.BitDepth == 16)
        {
            LanczosResize.PrepareWordProducts(source.GetRow16(y), products, context);
            HorizontalWord(products, plan, output, context);
            context.Cancellation.ThrowIfCancellationRequested(); return;
        }
        // Reuse one admitted backing store through two private views. No full
        // frame or extra row copy: exact signed sums are converted in place.
        var integer = MemoryMarshal.Cast<double, long>(output);
        LanczosResize.Horizontal(source, y, plan, integer, products, context);
        context.Cancellation.ThrowIfCancellationRequested();
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

    private static void HorizontalWord(ReadOnlySpan<uint> input, LanczosPlan plan,
        Span<double> output, OperationContext context)
    {
        var replicate01 = Vector512.Create(0L, 0, 0, 0, 1, 1, 1, 1);
        var replicate23 = Vector512.Create(2L, 2, 2, 2, 3, 3, 3, 3);
        for (var x = 0; x < plan.Windows.Length; x++)
        {
            context.Cancellation.ThrowIfCancellationRequested(); var window = plan.Windows[x];
            var first = Vector512<double>.Zero; var second = Vector512<double>.Zero; var i = 0;
            for (; i <= window.Count - 4; i += 4)
            {
                if ((i & 1023) == 0) context.Cancellation.ThrowIfCancellationRequested();
                var products = Vector512.Create(input.Slice((window.First + i) * 4, 16));
                var coefficients = Avx.ConvertToVector256Double(Vector128.Create(plan.Weights.AsSpan(window.Offset + i, 4)));
                var wide = Vector512.Create(coefficients, coefficients).AsInt64();
                var weights01 = Avx512F.PermuteVar8x64(wide, replicate01).AsDouble();
                var weights23 = Avx512F.PermuteVar8x64(wide, replicate23).AsDouble();
                first = Avx512F.FusedMultiplyAdd(Avx512F.ConvertToVector512Double(products.GetLower()), weights01, first);
                second = Avx512F.FusedMultiplyAdd(Avx512F.ConvertToVector512Double(products.GetUpper()), weights23, second);
            }
            var pair = Avx512F.Add(first, second); var sum = Avx.Add(pair.GetLower(), pair.GetUpper());
            for (; i < window.Count; i++)
            {
                if ((i & 1023) == 0) context.Cancellation.ThrowIfCancellationRequested();
                var products = Vector256.Create(Vector128.Create(input.Slice((window.First + i) * 4, 4)), Vector128<uint>.Zero);
                var values = Avx512F.ConvertToVector512Double(products).GetLower();
                sum = Fma.MultiplyAdd(values, Vector256.Create((double)plan.Weights[window.Offset + i]), sum);
            }
            sum.CopyTo(output.Slice(x * 4, 4));
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
        context.Cancellation.ThrowIfCancellationRequested();
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
