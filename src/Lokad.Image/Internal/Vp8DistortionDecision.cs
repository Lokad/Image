using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

// Distortion-only method0/1/2 policy. Uses the same compact borrowed macroblock
// and neighbor layout as basic RDO, but prices original prediction errors before
// reconstructing selected modes. The enclosing operation admits scratch once.
internal static class Vp8DistortionDecision
{
    internal const int ScratchBytes = 16_384;
    private const long Unselected = 1L << 56;
    private static int Block(int b) => (b / 4 * 4) * 16 + b % 4 * 4;
    private static int UvBlock(int b) => b / 4 * 8 + b % 4 / 2 * 64 + b % 2 * 4;
    private static byte Top(ReadOnlySpan<byte> neighbors, int x) => x < 16 ? neighbors[35 + x] : neighbors[67 + x - 16];

    // Search-local preparation: each angular mode projects the same rounded
    // border. The shared predictor and its strided/RDO consumers remain separate.
    internal readonly struct RoundedBorder(Vector128<byte> half, Vector128<byte> smooth)
    {
        internal readonly Vector128<byte> Half = half;
        internal readonly Vector128<byte> Smooth = smooth;
    }

    internal static RoundedBorder PrepareRoundedBorder(ReadOnlySpan<int> prepared)
    {
        // PrepareSmallBlock supplies thirteen byte-valued integers. Pack them as
        // L3 L2 L1 L0 X T0 ... T7 T7 T7 T7 before rounding; widening is required
        // for the three-tap sum (nested rounded byte averages are not equivalent).
        var first = Sse2.PackSignedSaturate(Vector128.Create(prepared[..4]), Vector128.Create(prepared.Slice(4, 4)));
        var last = Sse2.PackSignedSaturate(Vector128.Create(prepared.Slice(8, 4)), Vector128.CreateScalar(prepared[12]));
        var border = Ssse3.Shuffle(Sse2.PackUnsignedSaturate(first, last),
            Vector128.Create((byte)11, 10, 9, 8, 12, 0, 1, 2, 3, 4, 5, 6, 7, 7, 7, 7));
        var previous = Ssse3.Shuffle(border, Vector128.Create((byte)0, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14));
        var next = Ssse3.Shuffle(border, Vector128.Create((byte)1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 15));
        var lo = Sse2.Add(Sse2.Add(Sse2.UnpackLow(previous, Vector128<byte>.Zero).AsUInt16(),
            Sse2.UnpackLow(next, Vector128<byte>.Zero).AsUInt16()),
            Sse2.ShiftLeftLogical(Sse2.UnpackLow(border, Vector128<byte>.Zero).AsUInt16(), 1));
        var hi = Sse2.Add(Sse2.Add(Sse2.UnpackHigh(previous, Vector128<byte>.Zero).AsUInt16(),
            Sse2.UnpackHigh(next, Vector128<byte>.Zero).AsUInt16()),
            Sse2.ShiftLeftLogical(Sse2.UnpackHigh(border, Vector128<byte>.Zero).AsUInt16(), 1));
        var smooth = Sse2.PackUnsignedSaturate(Sse2.ShiftRightLogical(Sse2.Add(lo, Vector128.Create((ushort)2)), 2).AsInt16(),
            Sse2.ShiftRightLogical(Sse2.Add(hi, Vector128.Create((ushort)2)), 2).AsInt16());
        // Angular projections need only S0..S12; reserve S13 for the raw last
        // left sample used by horizontal-up's replicated bottom edge.
        smooth = Sse41.Insert(smooth, (byte)prepared[11], 13);
        return new(Sse2.Average(border, next), smooth);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<byte> PackedPrediction(int mode, ReadOnlySpan<int> prepared, int dc, in RoundedBorder rounded)
    {
        switch (mode)
        {
            case 0: return Vector128.Create((byte)dc);
            case 1:
                var top = Vector128.Create(prepared[..4]);
                var a = Sse2.Add(top, Vector128.Create(prepared[8] - prepared[12]));
                var b = Sse2.Add(top, Vector128.Create(prepared[9] - prepared[12]));
                var c = Sse2.Add(top, Vector128.Create(prepared[10] - prepared[12]));
                var d = Sse2.Add(top, Vector128.Create(prepared[11] - prepared[12]));
                return Sse2.PackUnsignedSaturate(Sse2.PackSignedSaturate(a, b), Sse2.PackSignedSaturate(c, d));
            case 2: return Ssse3.Shuffle(rounded.Smooth, Vector128.Create((byte)5, 6, 7, 8, 5, 6, 7, 8, 5, 6, 7, 8, 5, 6, 7, 8));
            case 3: return Ssse3.Shuffle(rounded.Smooth, Vector128.Create((byte)3, 3, 3, 3, 2, 2, 2, 2, 1, 1, 1, 1, 0, 0, 0, 0));
            case 4: return Ssse3.Shuffle(rounded.Smooth, Vector128.Create((byte)4, 5, 6, 7, 3, 4, 5, 6, 2, 3, 4, 5, 1, 2, 3, 4));
            case 5: return Sse2.Or(
                Ssse3.Shuffle(rounded.Half, Vector128.Create((byte)4, 5, 6, 7, 128, 128, 128, 128, 128, 4, 5, 6, 128, 128, 128, 128)),
                Ssse3.Shuffle(rounded.Smooth, Vector128.Create((byte)128, 128, 128, 128, 4, 5, 6, 7, 3, 128, 128, 128, 2, 4, 5, 6)));
            case 6: return Ssse3.Shuffle(rounded.Smooth, Vector128.Create((byte)6, 7, 8, 9, 7, 8, 9, 10, 8, 9, 10, 11, 9, 10, 11, 12));
            case 7: return Sse2.Or(
                Ssse3.Shuffle(rounded.Half, Vector128.Create((byte)5, 6, 7, 8, 128, 128, 128, 128, 6, 7, 8, 128, 128, 128, 128, 128)),
                Ssse3.Shuffle(rounded.Smooth, Vector128.Create((byte)128, 128, 128, 128, 6, 7, 8, 9, 128, 128, 128, 10, 7, 8, 9, 11)));
            case 8: return Sse2.Or(
                Ssse3.Shuffle(rounded.Half, Vector128.Create((byte)3, 128, 128, 128, 2, 128, 3, 128, 1, 128, 2, 128, 0, 128, 1, 128)),
                Ssse3.Shuffle(rounded.Smooth, Vector128.Create((byte)128, 4, 5, 6, 128, 3, 128, 4, 128, 2, 128, 3, 128, 1, 128, 2)));
            case 9: return Sse2.Or(
                Ssse3.Shuffle(rounded.Half, Vector128.Create((byte)2, 128, 1, 128, 1, 128, 0, 128, 0, 128, 128, 128, 128, 128, 128, 128)),
                Ssse3.Shuffle(rounded.Smooth, Vector128.Create((byte)128, 2, 128, 1, 128, 1, 128, 0, 128, 0, 13, 13, 13, 13, 13, 13)));
            default: throw new ArgumentOutOfRangeException(nameof(mode));
        }
    }

    private static void Predict16(ReadOnlySpan<byte> neighbors, bool top, bool left, int mode, Span<byte> prediction)
        => Vp8BlockPrediction.Fill(16, (Vp8IntraMode)mode, top ? neighbors.Slice(35, 16) : default,
            left ? neighbors.Slice(1, 16) : default, neighbors[0], prediction);

    private static void PredictUv(ReadOnlySpan<byte> neighbors, bool top, bool left, int mode, Span<byte> prediction)
    {
        Span<byte> plane = stackalloc byte[64];
        for (var channel = 0; channel < 2; channel++)
        {
            var corner = channel == 0 ? 17 : 26;
            Vp8BlockPrediction.Fill(8, (Vp8IntraMode)mode, top ? neighbors.Slice(51 + channel * 8, 8) : default,
                left ? neighbors.Slice(corner + 1, 8) : default, neighbors[corner], plane);
            for (var y = 0; y < 8; y++) plane.Slice(y * 8, 8).CopyTo(prediction.Slice(y * 16 + channel * 8, 8));
        }
    }

    private static uint Reconstruct16(ReadOnlySpan<byte> source, ReadOnlySpan<byte> neighbors, bool top, bool left, int mode,
        Vp8QuantizationMatrix luma, Vp8QuantizationMatrix walshMatrix, Span<byte> pixels, Span<short> levels, OperationContext context)
    {
        context.Cancellation.ThrowIfCancellationRequested(); Predict16(neighbors, top, left, mode, pixels);
        Span<short> coefficients = stackalloc short[256]; Span<short> dc = stackalloc short[16]; Span<short> walsh = stackalloc short[16];
        for (var b = 0; b < 16; b += 4)
            Vp8ForwardTransform.DctFourBlocks(source[Block(b)..], pixels[Block(b)..], 16, 16, coefficients.Slice(b * 16, 64));
        for (var b = 0; b < 16; b++) dc[b] = coefficients[b * 16];
        Vp8ForwardTransform.WalshDc(dc, walsh);
        var nz = Vp8BlockQuantization.Apply(walsh, levels[..16], walshMatrix) ? 1u << 24 : 0;
        for (var b = 0; b < 16; b++) coefficients[b * 16] = 0;
        nz |= Vp8BlockQuantization.ApplyBlocks(coefficients, levels[16..], luma, 16);
        Vp8BlockReconstruction.WriteWalshDc(walsh, coefficients);
        for (var b = 0; b < 16; b++) Vp8BlockReconstruction.Add(coefficients.Slice(b * 16, 16), pixels[Block(b)..], 16);
        return nz;
    }

    private static bool Try4(ReadOnlySpan<byte> source, ReadOnlySpan<byte> neighbors, ReadOnlySpan<byte> contexts,
        Vp8QuantizationMatrix matrix, int penalty, ReadOnlySpan<ushort> prices, long headerLimit, long previous,
        Span<byte> pixels, Span<short> levels, Span<byte> modes, OperationContext context, out long score, out uint nz)
    {
        score = penalty; nz = 0; long header = 0;
        Span<byte> top = stackalloc byte[8]; Span<byte> left = stackalloc byte[4];
        Span<byte> trial = stackalloc byte[16]; Span<byte> selected = stackalloc byte[16];
        Span<short> coefficients = stackalloc short[16]; Span<short> residual = stackalloc short[16];
        Span<int> prepared = stackalloc int[13];
        var packedPixels = Avx2.IsSupported;
        var packedPrediction = packedPixels && Ssse3.IsSupported && Sse41.IsSupported;
        for (var b = 0; b < 16; b++)
        {
            var bx = b % 4; var by = b / 4; var x = bx * 4; var y = by * 4; var offset = Block(b);
            for (var p = 0; p < 8; p++) top[p] = by == 0 || bx == 3 && p >= 4 ? Top(neighbors, x + p) : pixels[(y - 1) * 16 + x + p];
            for (var p = 0; p < 4; p++) left[p] = bx == 0 ? neighbors[1 + y + p] : pixels[(y + p) * 16 + x - 1];
            var corner = bx == 0 ? neighbors[y] : by == 0 ? Top(neighbors, x - 1) : pixels[(y - 1) * 16 + x - 1];
            var dc = Vp8Prediction.PrepareSmallBlock(top, left, corner, prepared);
            var sourcePixels = packedPixels ? Vp8Distortion.PackFourRows(source[offset..], 16) : default;
            var above = by == 0 ? contexts[bx] : modes[b - 4]; var side = bx == 0 ? contexts[4 + by] : modes[b - 1];
            var costs = prices.Slice(8 + (above * 10 + side) * 10, 10); var bestMode = 0; var best = Unselected;
            RoundedBorder rounded = default; var roundedReady = false;
            Vector128<byte> selectedPixels = default;
            for (var mode = 0; mode < 10; mode++)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                // Pixel error is nonnegative; an equal header cannot win the
                // strict-less decision either. Keep polling before every mode.
                var modeHeader = costs[mode] * 11L;
                if (modeHeader >= best) continue;
                Vector128<byte> trialPixels = default; int distortion;
                if (packedPrediction)
                {
                    if (mode >= 2 && !roundedReady) { rounded = PrepareRoundedBorder(prepared); roundedReady = true; }
                    trialPixels = PackedPrediction(mode, prepared, dc, rounded);
                    distortion = Vp8Distortion.Squared(sourcePixels, trialPixels);
                }
                else
                {
                    Vp8Prediction.SmallBlock((Vp8SmallIntraMode)mode, prepared, dc, trial);
                    distortion = packedPixels ? Vp8Distortion.Squared(sourcePixels, Vector128.Create(trial))
                        : Vp8Distortion.Squared(source[offset..], 16, trial, 4, 4, 4);
                }
                var current = 256L * distortion + modeHeader;
                if (current < best)
                {
                    best = current; bestMode = mode;
                    if (packedPrediction) selectedPixels = trialPixels; else trial.CopyTo(selected);
                }
            }
            modes[b] = (byte)bestMode; score += best; header += costs[bestMode];
            if (score >= previous || header > headerLimit) return false;
            context.Cancellation.ThrowIfCancellationRequested();
            if (packedPrediction) selectedPixels.CopyTo(selected);
            for (var yy = 0; yy < 4; yy++) for (var xx = 0; xx < 4; xx++)
                residual[yy * 4 + xx] = (short)(source[offset + yy * 16 + xx] - selected[yy * 4 + xx]);
            Vp8ForwardTransform.Dct(residual, coefficients);
            if (Vp8BlockQuantization.Apply(coefficients, levels.Slice(16 + b * 16, 16), matrix)) nz |= 1u << b;
            Vp8BlockReconstruction.Add(coefficients, selected, 4);
            for (var yy = 0; yy < 4; yy++) selected.Slice(yy * 4, 4).CopyTo(pixels.Slice(offset + yy * 16, 4));
        }
        return true;
    }

    private static uint ReconstructUv(ReadOnlySpan<byte> source, ReadOnlySpan<byte> neighbors, ReadOnlySpan<sbyte> diffusion,
        bool top, bool left, int mode, bool correctDc, Vp8QuantizationMatrix matrix, Span<byte> pixels,
        Span<short> levels, Span<sbyte> errors, OperationContext context)
    {
        context.Cancellation.ThrowIfCancellationRequested(); PredictUv(neighbors, top, left, mode, pixels);
        Span<short> coefficients = stackalloc short[128];
        Vp8ForwardTransform.DctFourBlocks(source, pixels, 16, 16, coefficients, 64);
        Vp8ForwardTransform.DctFourBlocks(source[64..], pixels[64..], 16, 16, coefficients[32..], 64);
        errors[..14].Clear();
        if (correctDc) Vp8MacroblockDecision.CorrectDc(coefficients, diffusion, errors, matrix);
        // Unlike the RDO UV selector, distortion-only reconstruction does not
        // publish diffusion to neighboring macroblocks. Preserve incoming edges.
        diffusion[..8].CopyTo(errors[6..]);
        var nz = Vp8BlockQuantization.ApplyBlocks(coefficients, levels, matrix, 8) << 16;
        for (var b = 0; b < 8; b++)
            Vp8BlockReconstruction.Add(coefficients.Slice(b * 16, 16), pixels[UvBlock(b)..], 16);
        return nz;
    }

    // Source/neighbors/modes/diffusion can alias results; snapshot before writes.
    // Matrix/rate owners remain borrowed and do not share result storage.
    internal static Vp8MacroblockScore Select(ReadOnlySpan<byte> source, ReadOnlySpan<byte> neighbors,
        ReadOnlySpan<byte> modeContexts, ReadOnlySpan<sbyte> diffusion, bool hasTop, bool hasLeft,
        int method, bool sourceIntra16, int sourceUvMode, long headerLimit, bool correctDc,
        Vp8QuantizationMatrix luma, Vp8QuantizationMatrix walsh, Vp8QuantizationMatrix chroma,
        Vp8QuantizationCosts costs, Vp8RatePlan rate, Span<byte> pixels, Span<short> levels,
        Span<byte> modes, Span<sbyte> errors, OperationContext context)
    {
        if (method is < 0 or > 2 || (uint)sourceUvMode > 3 || headerLimit < 0) throw new ArgumentOutOfRangeException(nameof(method));
        if (source.Length < 384 || neighbors.Length < 71 || modeContexts.Length < 8 || diffusion.Length < 8
            || pixels.Length < 384 || levels.Length < 400 || modes.Length < 16 || errors.Length < 14)
            throw new ArgumentException("Incomplete VP8 distortion decision state or result.");
        foreach (var mode in modeContexts[..8]) if (mode > 9) throw new ArgumentException("Invalid neighbor mode.");
        var levelBytes = MemoryMarshal.AsBytes(levels[..400]); var errorBytes = MemoryMarshal.AsBytes(errors[..14]);
        if (pixels[..384].Overlaps(levelBytes) || pixels[..384].Overlaps(modes[..16]) || pixels[..384].Overlaps(errorBytes)
            || levelBytes.Overlaps(modes[..16]) || levelBytes.Overlaps(errorBytes) || modes[..16].Overlaps(errorBytes))
            throw new ArgumentException("Macroblock result buffers must not overlap.");
        context.Cancellation.ThrowIfCancellationRequested(); Span<byte> snapshot = stackalloc byte[463]; source[..384].CopyTo(snapshot);
        neighbors[..71].CopyTo(snapshot[384..]); modeContexts[..8].CopyTo(snapshot[455..]);
        Span<sbyte> errorInput = stackalloc sbyte[8]; diffusion[..8].CopyTo(errorInput);
        var src = snapshot[..384]; var edges = snapshot.Slice(384, 71); var incomingModes = snapshot.Slice(455, 8);
        var prices = rate.Modes; var tryBoth = method == 2; var intra16 = tryBoth || sourceIntra16;
        var limit = tryBoth ? headerLimit : Unselected; var best = Unselected; var lumaMode = 0;
        Span<byte> prediction = stackalloc byte[256]; levels[..400].Clear(); modes[..16].Clear();
        if (intra16)
        {
            for (var mode = 0; mode < 4; mode++)
            {
                context.Cancellation.ThrowIfCancellationRequested(); Predict16(edges, hasTop, hasLeft, mode, prediction);
                var score = 256L * Vp8Distortion.Squared(src, 16, prediction, 16, 16, 16) + prices[mode] * 106L;
                if (mode != 0 && prices[mode] > limit) continue;
                if (score < best) { best = score; lumaMode = mode; }
            }
            if (!hasTop || !hasLeft)
            {
                var flat = true; foreach (var value in src[..256]) if (value != src[0]) { flat = false; break; }
                if (flat) { lumaMode = hasLeft ? 2 : 0; tryBoth = false; }
            }
        }
        uint nz = 0;
        if (tryBoth || !intra16)
        {
            intra16 = !Try4(src, edges, incomingModes, luma, costs.Intra4Penalty, prices, limit, best,
                pixels[..256], levels[..272], modes[..16], context, out var smallScore, out nz);
            if (!intra16) { best = smallScore; lumaMode = 0; }
        }
        if (intra16)
        {
            modes[..16].Clear(); nz = Reconstruct16(src, edges, hasTop, hasLeft, lumaMode, luma, walsh, pixels[..256], levels[..272], context);
        }
        var uvMode = sourceUvMode;
        if (method >= 1)
        {
            var bestUv = Unselected;
            for (var mode = 0; mode < 4; mode++)
            {
                context.Cancellation.ThrowIfCancellationRequested(); PredictUv(edges, hasTop, hasLeft, mode, prediction[..128]);
                var score = 256L * Vp8Distortion.Squared(src[256..], 16, prediction, 16, 16, 8) + prices[4 + mode] * 120L;
                if (score < bestUv) { bestUv = score; uvMode = mode; }
            }
        }
        nz |= ReconstructUv(src[256..], edges, errorInput, hasTop, hasLeft, uvMode, correctDc, chroma,
            pixels.Slice(256, 128), levels.Slice(272, 128), errors[..14], context);
        return new(0, 0, 0, 0, best, nz, lumaMode, uvMode, intra16, 0);
    }
}
