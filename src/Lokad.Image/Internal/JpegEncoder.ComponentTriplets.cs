namespace Lokad.Image.Internal;

internal static partial class JpegEncoder
{
    // The pair pass already computed this state. Reuse it to admit the new
    // compression tail without another error scan or three full-block prices.
    internal readonly record struct Joint444State(double Error, int YPrice, int CbPrice, int CrPrice);
    internal const int ComponentTripletWorkPerMcu = 63 * (6 * 64 + 26 + 26 * 8 + 3);

    internal static void RefineComponentTriplets444(ReadOnlySpan<double> source, ReadOnlySpan<ushort> luma,
        ReadOnlySpan<ushort> chroma, Span<int> values, ReadOnlySpan<int> prices, int lambda,
        OperationContext context, double maximumError, Joint444State state, ReadOnlySpan<ushort> amplitudeLimits = default, JointAcBands bands = default)
    {
        if (source.Length != 192 || values.Length != 192 || luma.Length != 64 || chroma.Length != 64
            || prices.Length != 512 || lambda < 0 || amplitudeLimits.Length is not (0 or 128)
            || !double.IsFinite(maximumError) || maximumError < 0 || !double.IsFinite(state.Error))
            throw new ArgumentException("JPEG component triplets require three complete blocks and their current decision state.");
        if (lambda == 0) return;
        // 48 stack bytes. The previous DAG scratch is no longer live; the
        // effective-grid arrays and this bank fit the same admitted 8 KiB.
        Span<int> previous = stackalloc int[3] { state.YPrice, state.CbPrice, state.CrPrice };
        Span<int> cached = stackalloc int[9];
        var currentError = state.Error;
        for (var scan = 63; scan > 0; scan--)
        {
            context.Work(26);
            var i = JpegTransform.Zigzag[scan];
            var yLevel = values[i]; var cbLevel = values[64 + i]; var crLevel = values[128 + i];
            for (var c = 0; c < 3; c++)
            {
                cached[c * 3] = previous[c];
                var original = values[c * 64 + i];
                var limit = amplitudeLimits.IsEmpty ? 1023 : amplitudeLimits[(c == 0 ? 0 : 64) + i];
                for (var choice = 1; choice < 3; choice++)
                {
                    var value = original + (choice == 1 ? -1 : 1);
                    cached[c * 3 + choice] = int.MaxValue;
                    if (value < -limit || value > limit) continue;
                    context.Work(64);
                    cached[c * 3 + choice] = PairAcPrice(values.Slice(c * 64, 64),
                        prices.Slice(c == 0 ? 0 : 256, 256), i, value, bands.ForComponent(c));
                }
            }
            var beforeBits = previous[0] + previous[1] + previous[2];
            var beforeError = JointRgbError(source[i] - (double)yLevel * luma[i],
                source[64 + i] - (double)cbLevel * chroma[i], source[128 + i] - (double)crLevel * chroma[i]);
            var bestY = 0; var bestCb = 0; var bestCr = 0; var bestError = 0d; var bestCost = 0d;
            for (var a = 0; a < 3; a++)
            for (var b = 0; b < 3; b++)
            for (var c = 0; c < 3; c++)
            {
                if (a == 0 && b == 0 && c == 0) continue;
                context.Work(8);
                var py = cached[a]; var pcb = cached[3 + b]; var pcr = cached[6 + c];
                // Invalid neighbors never enter a sum, including the sentinel.
                if (py == int.MaxValue || pcb == int.MaxValue || pcr == int.MaxValue) continue;
                var bits = py + pcb + pcr;
                var y = yLevel + (a == 1 ? -1 : a == 2 ? 1 : 0);
                var cb = cbLevel + (b == 1 ? -1 : b == 2 ? 1 : 0);
                var cr = crLevel + (c == 1 ? -1 : c == 2 ? 1 : 0);
                var error = JointRgbError(source[i] - (double)y * luma[i],
                    source[64 + i] - (double)cb * chroma[i], source[128 + i] - (double)cr * chroma[i]) - beforeError;
                var cost = error + (double)lambda * (bits - beforeBits) / 256;
                if (bits <= beforeBits && cost < bestCost && currentError + error <= maximumError)
                { bestY = a; bestCb = b; bestCr = c; bestError = error; bestCost = cost; }
            }
            context.Work(3);
            values[i] = yLevel + (bestY == 1 ? -1 : bestY == 2 ? 1 : 0);
            values[64 + i] = cbLevel + (bestCb == 1 ? -1 : bestCb == 2 ? 1 : 0);
            values[128 + i] = crLevel + (bestCr == 1 ? -1 : bestCr == 2 ? 1 : 0);
            previous[0] = cached[bestY]; previous[1] = cached[3 + bestCb]; previous[2] = cached[6 + bestCr];
            currentError += bestError;
        }
    }
}
