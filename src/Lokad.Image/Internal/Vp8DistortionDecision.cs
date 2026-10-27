using System.Runtime.InteropServices;

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
        for (var b = 0; b < 16; b++)
        {
            coefficients[b * 16] = 0;
            if (Vp8BlockQuantization.Apply(coefficients.Slice(b * 16, 16), levels.Slice(16 + b * 16, 16), luma)) nz |= 1u << b;
        }
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
        for (var b = 0; b < 16; b++)
        {
            var bx = b % 4; var by = b / 4; var x = bx * 4; var y = by * 4; var offset = Block(b);
            for (var p = 0; p < 8; p++) top[p] = by == 0 || bx == 3 && p >= 4 ? Top(neighbors, x + p) : pixels[(y - 1) * 16 + x + p];
            for (var p = 0; p < 4; p++) left[p] = bx == 0 ? neighbors[1 + y + p] : pixels[(y + p) * 16 + x - 1];
            var corner = bx == 0 ? neighbors[y] : by == 0 ? Top(neighbors, x - 1) : pixels[(y - 1) * 16 + x - 1];
            var above = by == 0 ? contexts[bx] : modes[b - 4]; var side = bx == 0 ? contexts[4 + by] : modes[b - 1];
            var costs = prices.Slice(8 + (above * 10 + side) * 10, 10); var bestMode = 0; var best = Unselected;
            for (var mode = 0; mode < 10; mode++)
            {
                context.Cancellation.ThrowIfCancellationRequested(); Vp8Prediction.SmallBlock((Vp8SmallIntraMode)mode, top, left, corner, trial);
                var current = 256L * Vp8Distortion.Squared(source[offset..], 16, trial, 4, 4, 4) + costs[mode] * 11L;
                if (current < best) { best = current; bestMode = mode; trial.CopyTo(selected); }
            }
            modes[b] = (byte)bestMode; score += best; header += costs[bestMode];
            if (score >= previous || header > headerLimit) return false;
            context.Cancellation.ThrowIfCancellationRequested();
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
        diffusion[..8].CopyTo(errors[6..]); uint nz = 0;
        for (var b = 0; b < 8; b++)
        {
            if (Vp8BlockQuantization.Apply(coefficients.Slice(b * 16, 16), levels.Slice(b * 16, 16), matrix)) nz |= 1u << (16 + b);
            Vp8BlockReconstruction.Add(coefficients.Slice(b * 16, 16), pixels[UvBlock(b)..], 16);
        }
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
