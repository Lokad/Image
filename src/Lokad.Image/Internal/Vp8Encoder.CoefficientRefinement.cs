namespace Lokad.Image.Internal;

internal static partial class Vp8Encoder
{
    private const int RefinementWork = 4736;

    // Independently derived finite-state selection for RFC 6386 section13
    // coefficient tokens. Adjacent quantization levels and terminal EOBs compete
    // using our existing token costs and the mathematical transform's energy.
    // No source image, native implementation, weight table or tuning grid is used.
    private static bool RefineCoefficients(ReadOnlySpan<short> residual, Span<short> coefficients,
        int dc, int ac, int plane, int predictor, ReadOnlySpan<byte> probabilities,
        ReadOnlySpan<int> costs, int lambda, OperationContext context)
    {
        // Bound: transform256,16*(six32-unit transitions +three16-unit EOBs),
        // seed rate512 and128 tail/seed/backtracking units. Charge before mutation.
        context.Work(RefinementWork);
        Span<double> raw = stackalloc double[16]; Vp8ForwardDct.Transform(residual, raw);
        Span<double> tail = stackalloc double[17]; tail[16] = 0;
        for (var c = 15; c >= 0; c--) { var v = raw[Zigzag[c]]; tail[c] = tail[c + 1] + v * v * 16384; }
        Span<double> current = stackalloc double[3]; Span<double> next = stackalloc double[3];
        Span<sbyte> previous = stackalloc sbyte[48]; Span<short> levels = stackalloc short[48];
        current.Fill(double.PositiveInfinity); current[predictor] = 0;
        var first = plane == 0 ? 1 : 0;
        var bestScore = (double)CoefficientRate(coefficients, plane, predictor, probabilities, costs, out _) * lambda;
        for (var c = first; c < 16; c++)
        {
            var z = Zigzag[c]; var error = raw[z] - coefficients[z] * (z == 0 ? dc : ac);
            bestScore += error * error * 16384;
        }
        var bestEnd = -1; var bestContext = -1;
        for (var c = first; c <= 16; c++)
        {
            next.Fill(double.PositiveInfinity);
            for (var state = 0; state < 3; state++)
            {
                if (double.IsPositiveInfinity(current[state])) continue;
                if (c == 16)
                {
                    if (state != 0 && current[state] < bestScore)
                    { bestScore = current[state]; bestEnd = c; bestContext = state; }
                    continue;
                }
                var p = probabilities.Slice(((plane * 8 + Bands[c]) * 3 + state) * 11, 11);
                // A zero token implies another coefficient; EOB can only
                // terminate at block start or immediately after a nonzero.
                if (c == first || state != 0)
                {
                    var score = current[state] + tail[c] + (double)PathRate(TokenPaths[11], p, costs) * lambda;
                    if (score < bestScore) { bestScore = score; bestEnd = c; bestContext = state; }
                }
                var z = Zigzag[c]; var quantizer = z == 0 ? dc : ac;
                var magnitude = Math.Abs(raw[z]); var lower = (int)(magnitude / quantizer);
                for (var candidate = 0; candidate < 2; candidate++)
                {
                    var level = lower + candidate;
                    if (level > 511) continue;
                    var error = magnitude - level * quantizer;
                    var rate = RefinementTokenRate(level, c != first && state == 0, p, costs);
                    var score = current[state] + error * error * 16384 + (double)rate * lambda;
                    var destination = Math.Min(level, 2);
                    if (score >= next[destination]) continue;
                    next[destination] = score;
                    previous[c * 3 + destination] = (sbyte)state;
                    levels[c * 3 + destination] = checked((short)(raw[z] < 0 ? -level : level));
                }
            }
            next.CopyTo(current);
        }
        if (bestEnd < 0) return false;
        for (var c = first; c < 16; c++) coefficients[Zigzag[c]] = 0;
        for (var c = bestEnd - 1; c >= first; c--)
        {
            coefficients[Zigzag[c]] = levels[c * 3 + bestContext];
            bestContext = previous[c * 3 + bestContext];
        }
        return true;
    }

    private static int RefinementTokenRate(int magnitude, bool previousZero,
        ReadOnlySpan<byte> probabilities, ReadOnlySpan<int> costs)
    {
        var category = magnitude < 5 ? -1 : magnitude < 7 ? 0 : magnitude < 11 ? 1 : magnitude < 19 ? 2
            : magnitude < 35 ? 3 : magnitude < 67 ? 4 : 5;
        var path = TokenPaths[category < 0 ? magnitude : category + 5];
        var result = PathRate(previousZero ? path >> 5 : path, probabilities, costs);
        if (category >= 0)
        {
            // Normative RFC6386 category probabilities, already used by the
            // owned writer and mode scorer. They are format constants.
            ReadOnlySpan<byte> p = category switch
            {
                0 => [159], 1 => [165, 145], 2 => [173, 148, 140], 3 => [176, 155, 140, 135],
                4 => [180, 157, 141, 134, 130], _ => [254, 254, 243, 230, 196, 177, 153, 140, 133, 130, 129]
            };
            ReadOnlySpan<int> bases = [5, 7, 11, 19, 35, 67]; var extra = magnitude - bases[category];
            for (var bit = 0; bit < p.Length; bit++) result += costs[(extra >> (p.Length - 1 - bit) & 1) == 0 ? p[bit] : 256 - p[bit]];
        }
        return result + (magnitude == 0 ? 0 : 256);
    }
}
