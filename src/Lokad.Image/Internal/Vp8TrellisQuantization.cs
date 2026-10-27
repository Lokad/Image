namespace Lokad.Image.Internal;

// Private coefficient-domain rate/distortion search. RFC6386 sections11/13/14
// define coefficient syntax, scan and quantizer geometry. Candidate selection
// is pinned encoder policy. All storage is borrowed or stack scratch, admitted
// once by the enclosing operation; no caches, heap or per-block scope.
internal static class Vp8TrellisQuantization
{
    internal const int ScratchBytes = 2048;

    // Independent equivalent importance model: a quarter-plane Gaussian of
    // variance3.5, amplitude26 and baseline4, rounded to nearest integer.
    // It reproduces the pinned profile; it does not assert its original origin.
    internal static void Weights(Span<ushort> destination)
    {
        if (destination.Length < 16) throw new ArgumentException("Trellis requires16weights.");
        for (var p = 0; p < 16; p++)
        {
            var x = p & 3; var y = p >> 2;
            destination[p] = (ushort)Math.Floor(4 + 26 * Math.Exp(-(x * x + y * y) / 7d) + .5);
        }
    }

    private static int Band(int position) => position switch
    { 0 => 0, 1 => 1, 2 => 2, 3 => 3, 5 => 4, 6 => 5, 15 => 7, _ => 6 };

    private static int Index(int type, int position, int context) => (type * 8 + Band(position)) * 3 + context;
    // Raster coefficients become dequantized raster coefficients; levels use
    // zigzag order. I16 AC preserves both DC slots. The transform domain here
    // is DCT only (not the larger Y2 Walsh domain). Tails remain untouched.
    internal static bool Apply(Span<short> coefficients, Span<short> levels,
        Vp8QuantizationMatrix matrix, Vp8RatePlan rate, ReadOnlySpan<ushort> weights,
        int type, int initialContext, int lambda, OperationContext operation)
    {
        if (coefficients.Length < 16 || levels.Length < 16 || matrix.Values.Length < 80 || weights.Length < 16
            || type is not (0 or 3) || (uint)initialContext > 2 || lambda < 1)
            throw new ArgumentException("Invalid VP8 luma trellis layout or policy.");
        if (coefficients.Overlaps(levels)) throw new ArgumentException("Coefficient and level buffers must not overlap.");
        for (var p = 0; p < 16; p++)
            if (coefficients[p] is < -2040 or > 2040) throw new ArgumentOutOfRangeException(nameof(coefficients), "Coefficient exceeds the VP8 DCT domain.");
        var sum = 0; foreach (var weight in weights[..16]) sum += weight;
        if (sum != 256) throw new ArgumentException("Trellis importance must sum to256.");
        _ = rate.Values; operation.Cancellation.ThrowIfCancellationRequested();
        var fixedLevels = rate.FixedLevels; var variableLevels = rate.VariableLevels; var endOfBlock = rate.EndOfBlock;
        var steps = matrix.Steps; var reciprocals = matrix.Reciprocals; var sharpening = matrix.Sharpening;
        // RFC6386 section13 diagonal coefficient order; read-only format data.
        ReadOnlySpan<byte> scan = [0, 1, 4, 8, 5, 2, 3, 6, 9, 12, 13, 10, 7, 11, 14, 15];
        Span<short> original = stackalloc short[16]; coefficients[..16].CopyTo(original);
        Span<short> nodes = stackalloc short[32]; Span<byte> predecessors = stackalloc byte[32];
        var first = type == 0 ? 1 : 0; var last = first - 1;
        var threshold = steps[1] * steps[1] / 4;
        for (var n = 15; n >= first; n--) if (original[scan[n]] * original[scan[n]] > threshold) { last = n; break; }
        if (last < 15) last++;
        var start = Index(type, first, initialContext);
        var best = (long)lambda * endOfBlock[start * 2]; var bestEnd = -1; var bestNode = 0;
        var score0 = initialContext == 0 ? (long)lambda * endOfBlock[start * 2 + 1] : 0; var score1 = score0;
        var context0 = initialContext; var context1 = initialContext;
        const long dead = 0x7fffffffffffffL;
        for (var n = first; n <= last; n++)
        {
            var p = scan[n]; var magnitude = Math.Abs((int)original[p]) + sharpening[p];
            var floor = Math.Min(2047, magnitude * reciprocals[p] >> 17);
            var ceiling = Math.Min(2047, (magnitude * reciprocals[p] + (128 << 9)) >> 17);
            // Both candidate levels use the same two predecessor rows. Borrow
            // each row offset once; keep the strict alternate/tie comparison.
            var band = (type * 8 + Band(n)) * 3;
            var row0 = (band + context0) * 68; var row1 = (band + context1) * 68;
            var nextScore0 = dead; var nextScore1 = dead;
            for (var choice = 0; choice < 2; choice++)
            {
                var level = floor + choice;
                if (level > ceiling) continue;
                var error = magnitude - level * steps[p];
                var delta = (long)weights[p] * (error * error - magnitude * magnitude) * 256;
                var winner = 0;
                var fixedCost = fixedLevels[level]; var variable = Math.Min(level, 67);
                var score = score0 + (long)lambda * (fixedCost + variableLevels[row0 + variable]);
                var alternate = score1 + (long)lambda * (fixedCost + variableLevels[row1 + variable]);
                if (alternate < score) { score = alternate; winner = 1; }
                score += delta;
                if (choice == 0) nextScore0 = score; else nextScore1 = score;
                nodes[n * 2 + choice] = (short)(original[p] < 0 ? -level : level);
                predecessors[n * 2 + choice] = (byte)winner;
                if (level != 0 && score < best)
                {
                    var end = n == 15 ? 0 : endOfBlock[Index(type, n + 1, Math.Min(level, 2)) * 2];
                    var terminal = score + (long)lambda * end;
                    if (terminal < best) { best = terminal; bestEnd = n; bestNode = choice; }
                }
            }
            score0 = nextScore0; score1 = nextScore1;
            context0 = Math.Min(floor, 2); context1 = Math.Min(floor + 1, 2);
        }
        coefficients.Slice(first, 16 - first).Clear(); levels.Slice(first, 16 - first).Clear();
        if (bestEnd < 0) return false;
        for (var n = bestEnd; n >= first; n--)
        {
            var level = nodes[n * 2 + bestNode]; var p = scan[n]; levels[n] = level;
            coefficients[p] = checked((short)(level * steps[p])); bestNode = predecessors[n * 2 + bestNode];
        }
        return true;
    }
}
