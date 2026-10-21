namespace Lokad.Image.Internal;

internal static partial class JpegEncoder
{
    // 384 original/current error + 192 initial AC prices, then 63 frequencies
    // * three component pairs * (8 setup + eight 128-unit trials + 2 commit).
    internal const int ComponentPairWorkPerMcu = 196002;

    // The componentwise DAG holds the other components fixed. One bounded tail
    // can jointly change two adjacent levels to account for RGB cross terms.
    // Keep the same source-error cap, lambda, price models, DQT and DC.
    internal static void RefineComponentPairs444(ReadOnlySpan<double> source, ReadOnlySpan<ushort> luma,
        ReadOnlySpan<ushort> chroma, Span<int> values, ReadOnlySpan<int> prices, int lambda,
        OperationContext context, double? errorLimit = null, ReadOnlySpan<ushort> amplitudeLimits = default)
    {
        if (source.Length != 192 || values.Length != 192 || luma.Length != 64 || chroma.Length != 64
            || prices.Length != 512 || lambda < 0 || amplitudeLimits.Length is not (0 or 128)
            || errorLimit.HasValue && (!double.IsFinite(errorLimit.Value) || errorLimit.Value < 0))
            throw new ArgumentException("JPEG component pairs require three complete blocks and two price models.");
        if (lambda == 0) return;
        context.Work(384);
        var originalError = 0d; var currentError = 0d;
        for (var scan = 0; scan < 64; scan++)
        {
            var i = JpegTransform.Zigzag[scan];
            var y = source[i]; var cb = source[64 + i]; var cr = source[128 + i];
            originalError += JointRgbError(y - (double)TrellisNearest(y, luma[i]) * luma[i],
                cb - (double)TrellisNearest(cb, chroma[i]) * chroma[i],
                cr - (double)TrellisNearest(cr, chroma[i]) * chroma[i]);
            currentError += JointRgbError(y - (double)values[i] * luma[i],
                cb - (double)values[64 + i] * chroma[i], cr - (double)values[128 + i] * chroma[i]);
        }
        var maximumError = errorLimit ?? (originalError + originalError / 32);
        // Twelve safe stack bytes once per MCU call, within the existing8KiB.
        // The preceding DAG's scratch is no longer live; no matrix or copy.
        Span<int> previous = stackalloc int[3];
        context.Work(192);
        for (var c = 0; c < 3; c++) previous[c] = JointAcPrice(values.Slice(c * 64, 64), prices.Slice(c == 0 ? 0 : 256, 256));
        for (var scan = 63; scan > 0; scan--)
        {
            var i = JpegTransform.Zigzag[scan];
            for (var a = 0; a < 2; a++)
            for (var b = a + 1; b < 3; b++)
            {
                context.Work(8);
                var ia = a * 64 + i; var ib = b * 64 + i;
                var va = values[ia]; var vb = values[ib];
                var qa = a == 0 ? luma[i] : chroma[i]; var qb = chroma[i];
                var y = source[i] - (double)values[i] * luma[i];
                var cb = source[64 + i] - (double)values[64 + i] * chroma[i];
                var cr = source[128 + i] - (double)values[128 + i] * chroma[i];
                var beforeError = JointRgbError(y, cb, cr); var beforeBits = previous[a] + previous[b];
                var bestA = va; var bestB = vb;
                var bestBitsA = previous[a]; var bestBitsB = previous[b]; var bestError = 0d; var bestCost = 0d;
                for (var oa = 0; oa < 3; oa++)
                for (var ob = 0; ob < 3; ob++)
                {
                    if (oa == 0 && ob == 0) continue;
                    var la = va + (oa == 1 ? -1 : oa == 2 ? 1 : 0);
                    var lb = vb + (ob == 1 ? -1 : ob == 2 ? 1 : 0);
                    var limitA = amplitudeLimits.IsEmpty ? 1023 : amplitudeLimits[(a == 0 ? 0 : 64) + i];
                    var limitB = amplitudeLimits.IsEmpty ? 1023 : amplitudeLimits[64 + i];
                    if (la < -limitA || la > limitA || lb < -limitB || lb > limitB) continue;
                    context.Work(128);
                    var bitsA = PairAcPrice(values.Slice(a * 64, 64), prices.Slice(a == 0 ? 0 : 256, 256), i, la);
                    var bitsB = PairAcPrice(values.Slice(b * 64, 64), prices.Slice(256, 256), i, lb);
                    var ea = source[ia] - (double)la * qa; var eb = source[ib] - (double)lb * qb;
                    var error = JointRgbError(a == 0 ? ea : y, a == 1 ? ea : b == 1 ? eb : cb,
                        b == 2 ? eb : cr) - beforeError;
                    var cost = error + (double)lambda * (bitsA + bitsB - beforeBits) / 256;
                    if (bitsA + bitsB <= beforeBits && cost < bestCost && currentError + error <= maximumError)
                    { bestA = la; bestB = lb; bestBitsA = bitsA; bestBitsB = bitsB; bestError = error; bestCost = cost; }
                }
                context.Work(2);
                // Trials only read coefficients. Refusal/cancellation cannot
                // commit a path which has not been fully selected and admitted.
                values[ia] = bestA; values[ib] = bestB;
                previous[a] = bestBitsA; previous[b] = bestBitsB; currentError += bestError;
            }
        }
    }

    private static int PairAcPrice(ReadOnlySpan<int> values, ReadOnlySpan<int> prices, int index, int replacement)
    {
        var zeros = 0; var bits = 0;
        for (var scan = 1; scan < 64; scan++)
        {
            var i = JpegTransform.Zigzag[scan]; var value = i == index ? replacement : values[i];
            if (value == 0) { zeros++; continue; }
            while (zeros >= 16) { bits += prices[0xF0]; zeros -= 16; }
            var size = Category(value); bits += prices[zeros * 16 + size] + size * 256; zeros = 0;
        }
        if (zeros != 0) bits += prices[0];
        return bits;
    }
}
