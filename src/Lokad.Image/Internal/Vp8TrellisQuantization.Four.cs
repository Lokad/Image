using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal static partial class Vp8TrellisQuantization
{
    // Parallel independent modes, not parallel coefficient positions. RFC6386
    // sections13/14 supply the scan/bands; each lane keeps the pinned serial
    // recurrence and strict ties of the single-search production kernel.
    private static uint SearchFourLuma4(Span<short> coefficients, Span<short> levels,
        in Execution execution, int initialContext, int lambda, uint zeroMask, CancellationToken cancellation)
    {
        ReadOnlySpan<byte> scan = [0, 1, 4, 8, 5, 2, 3, 6, 9, 12, 13, 10, 7, 11, 14, 15];
        Span<int> lasts = stackalloc int[4];
        var last = -1;
        for (var lane = 0; lane < 4; lane++)
        {
            var offset = lane * 16;
            if ((zeroMask & (1u << lane)) != 0)
            {
                lasts[lane] = -1; continue;
            }
            var original = coefficients.Slice(offset, 16);
            var end = -1;
            for (var n = 15; n >= 0; n--)
                if (original[scan[n]] * original[scan[n]] > execution.Threshold) { end = n; break; }
            if (end < 15) end++;
            lasts[lane] = end; last = Math.Max(last, end);
        }
        Span<short> nodes = stackalloc short[128];
        Span<byte> predecessors = stackalloc byte[32];
        var endOfBlock = execution.EndOfBlock;
        var start = Index(3, 0, initialContext);
        var best = Vector256.Create((long)lambda * endOfBlock[start * 2]);
        var bestEnd = Vector256.Create(-1L); var bestNode = Vector256<long>.Zero;
        var score0 = Vector256.Create(initialContext == 0 ? (long)lambda * endOfBlock[start * 2 + 1] : 0);
        var score1 = score0;
        var context0 = Vector128.Create(initialContext); var context1 = context0;
        var lastVector = Vector128.Create(lasts[0], lasts[1], lasts[2], lasts[3]);
        for (var n = 0; n <= last; n++)
        {
            cancellation.ThrowIfCancellationRequested();
            var p = scan[n];
            var original = Vector128.Create((int)coefficients[p], (int)coefficients[16 + p],
                (int)coefficients[32 + p], (int)coefficients[48 + p]);
            var magnitude = Vector128.Abs(original) + Vector128.Create(execution.Sharpening[p]);
            var product = magnitude * Vector128.Create(execution.Reciprocals[p]);
            var floor = Vector128.Min(product >> 17, Vector128.Create(2047));
            var ceiling = Vector128.Min((product + Vector128.Create(1 << 16)) >> 17, Vector128.Create(2047));
            var optional = Vector128.ExtractMostSignificantBits(Sse2.CompareGreaterThan(ceiling, floor));
            var step = Vector128.Create(execution.Steps[p]);
            var error = magnitude - floor * step; var square = magnitude * magnitude;
            var weight = execution.Weights[p];
            var d0 = FourScaled(error * error - square, weight) << 8;
            error -= step;
            var d1 = FourScaled(error * error - square, weight) << 8;
            var active = Avx2.ConvertToVector256Int64(Sse2.CompareGreaterThan(lastVector, Vector128.Create(n - 1)));
            var band = Vector128.Create((3 * 8 + Vp8Tables.CoefficientBand(n)) * 3);
            var row0 = (band + context0) * Vector128.Create(68);
            var row1 = (band + context1) * Vector128.Create(68);
            var next0 = ScoreFour(execution.FixedLevels, execution.CombinedLevels, row0, row1,
                score0, score1, floor, d0, lambda, out var winner0);
            predecessors[n * 2] = (byte)winner0;
            UpdateFourBest(endOfBlock, next0, floor, active, n, 0, lambda, ref best, ref bestEnd, ref bestNode);

            var next1 = Vector256.Create(0x7fffffffffffffL);
            if (optional != 0)
            {
                var nextLevel = Vector128.Min(floor + Vector128.Create(1), Vector128.Create(2047));
                var candidate1 = ScoreFour(execution.FixedLevels, execution.CombinedLevels, row0, row1,
                    score0, score1, nextLevel, d1, lambda, out var winner1);
                predecessors[n * 2 + 1] = (byte)winner1;
                var mask = FourMask(optional);
                next1 = Vector256.ConditionalSelect(mask, candidate1, next1);
                UpdateFourBest(endOfBlock, candidate1, nextLevel, active & mask, n, 1, lambda, ref best, ref bestEnd, ref bestNode);
            }
            // Both candidates fit Int16 exactly (floor <=2047). Absent upper
            // nodes are harmless, since their score keeps the dead sentinel.
            var negative = Sse2.CompareGreaterThan(Vector128<int>.Zero, original);
            var node0 = Vector128.ConditionalSelect(negative, -floor, floor);
            var upperLevel = floor + Vector128.Create(1);
            var node1 = Vector128.ConditionalSelect(negative, -upperLevel, upperLevel);
            Sse2.PackSignedSaturate(node0, node1).CopyTo(nodes.Slice(n * 8, 8));
            score0 = next0; score1 = next1;
            context0 = Vector128.Min(floor, Vector128.Create(2));
            context1 = Vector128.Min(floor + Vector128.Create(1), Vector128.Create(2));
        }
        uint nonzero = 0;
        for (var lane = 0; lane < 4; lane++)
        {
            var output = coefficients.Slice(lane * 16, 16); var outputLevels = levels.Slice(lane * 16, 16);
            output.Clear(); outputLevels.Clear();
            var end = (int)bestEnd.GetElement(lane); var node = (int)bestNode.GetElement(lane);
            if (end < 0) continue;
            nonzero |= 1u << lane;
            for (var n = end; n >= 0; n--)
            {
                var level = nodes[(n * 2 + node) * 4 + lane]; var p = scan[n]; outputLevels[n] = level;
                output[p] = checked((short)(level * execution.Steps[p]));
                node = (predecessors[n * 2 + node] >> lane) & 1;
            }
        }
        return nonzero;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<long> FourMask(uint bits) => Vector256.Create(
        (bits & 1) != 0 ? -1L : 0, (bits & 2) != 0 ? -1L : 0,
        (bits & 4) != 0 ? -1L : 0, (bits & 8) != 0 ? -1L : 0);

    // Explicit bounded managed reads; no unsafe gather or borrowed storage escape.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<int> FourPrices(ReadOnlySpan<ushort> values, Vector128<int> offsets) => Vector128.Create(
        (int)values[offsets.GetElement(0)], (int)values[offsets.GetElement(1)],
        (int)values[offsets.GetElement(2)], (int)values[offsets.GetElement(3)]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<long> FourScaled(Vector128<int> values, int lambda) => Avx2.Multiply(
        Vector256.Create(Sse2.UnpackLow(values, Vector128<int>.Zero), Sse2.UnpackHigh(values, Vector128<int>.Zero)),
        Vector256.Create(lambda));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<long> ScoreFour(ReadOnlySpan<ushort> fixedLevels, ReadOnlySpan<ushort> combinedLevels,
        Vector128<int> row0, Vector128<int> row1, Vector256<long> score0, Vector256<long> score1,
        Vector128<int> level, Vector256<long> delta, int lambda, out uint winners)
    {
        var l0 = level.GetElement(0); var l1 = level.GetElement(1); var l2 = level.GetElement(2); var l3 = level.GetElement(3);
        var correction = Vector128.Create(l0 > 67 ? fixedLevels[l0] - fixedLevels[67] : 0,
            l1 > 67 ? fixedLevels[l1] - fixedLevels[67] : 0, l2 > 67 ? fixedLevels[l2] - fixedLevels[67] : 0,
            l3 > 67 ? fixedLevels[l3] - fixedLevels[67] : 0);
        var stored = Vector128.Min(level, Vector128.Create(67));
        var score = score0 + FourScaled(FourPrices(combinedLevels, row0 + stored) + correction, lambda);
        var alternate = score1 + FourScaled(FourPrices(combinedLevels, row1 + stored) + correction, lambda);
        var mask = Avx2.CompareGreaterThan(score, alternate);
        winners = Vector256.ExtractMostSignificantBits(mask);
        return Vector256.ConditionalSelect(mask, alternate, score) + delta;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void UpdateFourBest(ReadOnlySpan<ushort> endOfBlock, Vector256<long> score, Vector128<int> level,
        Vector256<long> active, int position, int choice, int lambda,
        ref Vector256<long> best, ref Vector256<long> bestEnd, ref Vector256<long> bestNode)
    {
        var nonzero = ~Avx2.ConvertToVector256Int64(Sse2.CompareEqual(level, Vector128<int>.Zero));
        var eligible = active & nonzero & Avx2.CompareGreaterThan(best, score);
        if (Vector256.EqualsAll(eligible, Vector256<long>.Zero)) return;
        var end = position == 15 ? Vector256<long>.Zero : FourScaled(FourPrices(endOfBlock,
            (Vector128.Create(Index(3, position + 1, 0)) + Vector128.Min(level, Vector128.Create(2))) * Vector128.Create(2)), lambda);
        var terminal = score + end;
        var mask = eligible & Avx2.CompareGreaterThan(best, terminal);
        best = Vector256.ConditionalSelect(mask, terminal, best);
        bestEnd = Vector256.ConditionalSelect(mask, Vector256.Create((long)position), bestEnd);
        bestNode = Vector256.ConditionalSelect(mask, Vector256.Create((long)choice), bestNode);
    }
}
