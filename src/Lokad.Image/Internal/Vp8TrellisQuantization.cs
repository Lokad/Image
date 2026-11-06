using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

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
    // Evaluate floor(4 + 26*exp(-(x*x+y*y)/7) + .5), x/y0..3, once as immutable
    // policy data. The checked copy and caller-supplied Apply weights remain.
    // This is 32 bytes of module data, not an input-dependent cache or owner.
    private static ReadOnlySpan<ushort> Importance =>
        [30, 27, 19, 11, 27, 24, 17, 10, 19, 17, 12, 8, 11, 10, 8, 6];

    internal static void Weights(Span<ushort> destination)
    {
        if (destination.Length < 16) throw new ArgumentException("Trellis requires16weights.");
        Importance.CopyTo(destination);
    }

    // Independent coefficient geometry is prepared before the serial predecessor
    // walk. The 320-byte caller scratch is local to one synchronous Search.
    // Full-width signed samples and UInt16 weights require signed64 deltas.
    internal static uint PrepareCandidates(ReadOnlySpan<short> original, in Execution execution,
        Span<int> floors, Span<long> delta0, Span<long> delta1)
    {
        var steps = execution.Steps; var reciprocals = execution.Reciprocals;
        var sharpening = execution.Sharpening; var weights = execution.Weights;
        uint upper = 0;
        if (Avx2.IsSupported)
        {
            var samples = Vector256.Create(original);
            for (var p = 0; p < 16; p += 8)
            {
                // Widen before Abs, including Int16.MinValue.
                var values = Avx2.ConvertToVector256Int32(p == 0 ? samples.GetLower() : samples.GetUpper());
                var magnitude = Avx2.Abs(values).AsInt32() + Vector256.Create(sharpening[p..]);
                var product = magnitude * Vector256.Create(reciprocals[p..]);
                var floor = Vector256.Min(product >> 17, Vector256.Create(2047));
                var ceiling = Vector256.Min((product + Vector256.Create(1 << 16)) >> 17, Vector256.Create(2047));
                floor.CopyTo(floors[p..]);
                upper |= Vector256.ExtractMostSignificantBits(Avx2.CompareGreaterThan(ceiling, floor)) << p;
                var step = Vector256.Create(steps[p..]);
                var error = magnitude - floor * step;
                var weight = Avx2.ConvertToVector256Int32(Vector128.Create(weights[p..]));
                var square = magnitude * magnitude;
                WriteDeltas(error * error - square, weight, delta0[p..]);
                error -= step;
                WriteDeltas(error * error - square, weight, delta1[p..]);
            }
        }
        else
        {
            for (var p = 0; p < 16; p++)
            {
                var magnitude = Math.Abs((int)original[p]) + sharpening[p];
                var product = magnitude * reciprocals[p];
                var floor = Math.Min(2047, product >> 17);
                var ceiling = Math.Min(2047, (product + (1 << 16)) >> 17);
                floors[p] = floor;
                if (ceiling > floor) upper |= 1u << p;
                var error = magnitude - floor * steps[p];
                delta0[p] = (long)weights[p] * (error * error - magnitude * magnitude) * 256;
                error -= steps[p];
                delta1[p] = (long)weights[p] * (error * error - magnitude * magnitude) * 256;
            }
        }
        return upper;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteDeltas(Vector256<int> differences, Vector256<int> weights, Span<long> destination)
    {
        var even = Avx2.Multiply(differences, weights) << 8;
        var odd = Avx2.Multiply(Avx2.ShiftRightLogical128BitLane(differences.AsByte(), 4).AsInt32(),
            Avx2.ShiftRightLogical128BitLane(weights.AsByte(), 4).AsInt32()) << 8;
        var low = Avx2.UnpackLow(even, odd); var high = Avx2.UnpackHigh(even, odd);
        Vector256.Create(low.GetLower(), high.GetLower()).CopyTo(destination);
        Vector256.Create(low.GetUpper(), high.GetUpper()).CopyTo(destination[4..]);
    }

    private static int Index(int type, int position, int context) => (type * 8 + Vp8Tables.CoefficientBand(position)) * 3 + context;
    // Prepared once for a borrowed matrix, then reused by the exact zero route.
    // Product in [0,65535] proves both existing candidate levels zero.
    internal readonly struct ZeroTest
    {
        private readonly Vector256<int> reciprocalsLow, reciprocalsHigh, sharpeningLow, sharpeningHigh;
        internal ZeroTest(ReadOnlySpan<int> reciprocals, ReadOnlySpan<int> sharpening)
        {
            reciprocalsLow = Vector256.Create(reciprocals); reciprocalsHigh = Vector256.Create(reciprocals[8..]);
            sharpeningLow = Vector256.Create(sharpening); sharpeningHigh = Vector256.Create(sharpening[8..]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool AreZero(ReadOnlySpan<short> coefficients, int first)
        {
            var absolute = Avx2.Abs(Vector256.Create(coefficients)).AsInt16();
            var low = (Avx2.ConvertToVector256Int32(absolute.GetLower()) + sharpeningLow) * reciprocalsLow;
            var high = (Avx2.ConvertToVector256Int32(absolute.GetUpper()) + sharpeningHigh) * reciprocalsHigh;
            var outside = low.AsUInt32() >>> 16;
            if (first != 0) outside = outside.WithElement(0, 0u);
            return Vector256.EqualsAll(outside | (high.AsUInt32() >>> 16), Vector256<uint>.Zero);
        }
    }

    // Synchronous borrowed execution state, confined to one matrix/rate epoch.
    // The owning frame keeps all storage charged through its actual users.
    // No state escapes a search sequence or survives a rate refresh/disposal.
    internal readonly ref struct Execution
    {
        private readonly Vp8RatePlan rate;
        private readonly CancellationToken cancellation;
        private readonly ZeroTest zero;
        internal readonly Span<short> Scratch;
        internal readonly ReadOnlySpan<int> Steps, Reciprocals, Sharpening;
        internal readonly ReadOnlySpan<ushort> FixedLevels, CombinedLevels, EndOfBlock, Weights;
        internal readonly int Threshold;

        internal Execution(Vp8QuantizationMatrix matrix, Vp8RatePlan rate, ReadOnlySpan<ushort> weights, OperationContext operation, Span<short> scratch)
        {
            cancellation = operation.Cancellation; cancellation.ThrowIfCancellationRequested();
            this.rate = rate; Scratch = scratch[..64];
            FixedLevels = rate.FixedLevels; CombinedLevels = rate.CombinedLevels; EndOfBlock = rate.EndOfBlock;
            Steps = matrix.Steps; Reciprocals = matrix.Reciprocals; Sharpening = matrix.Sharpening; Weights = weights;
            Threshold = Steps[1] * Steps[1] / 4;
            zero = Avx2.IsSupported ? new ZeroTest(Reciprocals, Sharpening) : default;
        }

        // Raster coefficients become dequantized raster; levels are zigzag.
        // I16 AC preserves both DC slots; borrowed tails remain untouched.
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal bool Apply(Span<short> coefficients, Span<short> levels, int type, int initialContext, int lambda)
        {
            cancellation.ThrowIfCancellationRequested();
            // Borrowed views never bypass a disposed owner's admission lifetime.
            _ = rate.Values;
            var first = type == 0 ? 1 : 0;
            if (Avx2.IsSupported && zero.AreZero(coefficients, first))
            {
                coefficients.Slice(first, 16 - first).Clear(); levels.Slice(first, 16 - first).Clear();
                return false;
            }
            return Search(coefficients, levels, in this, type, initialContext, lambda);
        }
    }

    // DP working prefixes are reused only after the preceding synchronous search.
    // Every reachable predecessor/node is written before it is read; stale tail
    // slots are never part of a selected path. Zero blocks do not touch scratch.
    // All spans remain borrowed inside the same synchronous operation/owner.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool Search(Span<short> coefficients, Span<short> levels,
        in Execution execution, int type, int initialContext, int lambda)
    {
        var fixedLevels = execution.FixedLevels; var combinedLevels = execution.CombinedLevels; var endOfBlock = execution.EndOfBlock;
        var steps = execution.Steps;
        var first = type == 0 ? 1 : 0;
        // RFC6386 section13 diagonal coefficient order; read-only format data.
        ReadOnlySpan<byte> scan = [0, 1, 4, 8, 5, 2, 3, 6, 9, 12, 13, 10, 7, 11, 14, 15];
        var original = execution.Scratch[..16]; coefficients[..16].CopyTo(original);
        var nodes = execution.Scratch.Slice(16, 32);
        var predecessors = MemoryMarshal.AsBytes(execution.Scratch.Slice(48, 16));
        var last = first - 1;
        var threshold = execution.Threshold;
        for (var n = 15; n >= first; n--) if (original[scan[n]] * original[scan[n]] > threshold) { last = n; break; }
        if (last < 15) last++;
        Span<int> floors = stackalloc int[16];
        Span<long> delta0 = stackalloc long[16]; Span<long> delta1 = stackalloc long[16];
        var upper = PrepareCandidates(original, in execution, floors, delta0, delta1);
        var start = Index(type, first, initialContext);
        var best = (long)lambda * endOfBlock[start * 2]; var bestEnd = -1; var bestNode = 0;
        var score0 = initialContext == 0 ? (long)lambda * endOfBlock[start * 2 + 1] : 0; var score1 = score0;
        var context0 = initialContext; var context1 = initialContext;
        const long dead = 0x7fffffffffffffL;
        for (var n = first; n <= last; n++)
        {
            var p = scan[n]; var floor = floors[p];
            // Both candidate levels use the same two predecessor rows. Borrow
            // each row offset once; keep the strict alternate/tie comparison.
            var band = (type * 8 + Vp8Tables.CoefficientBand(n)) * 3;
            var row0 = (band + context0) * 68; var row1 = (band + context1) * 68;
            // Preparation proves exactly one mandatory floor and at most one
            // following level. Constant choices share the scoring source without
            // a per-choice loop or a block-wide prepared price representation.
            var nextScore0 = ScoreCandidate(fixedLevels, combinedLevels, endOfBlock,
                row0, row1, score0, score1, delta0[p], type, n, 0, floor, original[p], lambda,
                nodes, predecessors, ref best, ref bestEnd, ref bestNode);
            var nextScore1 = dead;
            if (((upper >> p) & 1) != 0)
                nextScore1 = ScoreCandidate(fixedLevels, combinedLevels, endOfBlock,
                    row0, row1, score0, score1, delta1[p], type, n, 1, floor + 1, original[p], lambda,
                    nodes, predecessors, ref best, ref bestEnd, ref bestNode);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long ScoreCandidate(ReadOnlySpan<ushort> fixedLevels, ReadOnlySpan<ushort> combinedLevels,
        ReadOnlySpan<ushort> endOfBlock, int row0, int row1, long score0, long score1, long delta,
        int type, int position, int choice, int level, short original, int lambda,
        Span<short> nodes, Span<byte> predecessors, ref long best, ref int bestEnd, ref int bestNode)
    {
        var winner = 0;
        var stored = Math.Min(level, 67);
        var correction = level > 67 ? fixedLevels[level] - fixedLevels[67] : 0;
        var score = score0 + (long)lambda * (combinedLevels[row0 + stored] + correction);
        var alternate = score1 + (long)lambda * (combinedLevels[row1 + stored] + correction);
        if (alternate < score) { score = alternate; winner = 1; }
        score += delta;
        nodes[position * 2 + choice] = (short)(original < 0 ? -level : level);
        predecessors[position * 2 + choice] = (byte)winner;
        if (level != 0 && score < best)
        {
            var end = position == 15 ? 0 : endOfBlock[Index(type, position + 1, Math.Min(level, 2)) * 2];
            var terminal = score + (long)lambda * end;
            if (terminal < best) { best = terminal; bestEnd = position; bestNode = choice; }
        }
        return score;
    }
}
