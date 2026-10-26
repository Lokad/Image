using System.Runtime.InteropServices;

namespace Lokad.Image.Internal;

internal readonly record struct Vp8MacroblockScore(long Distortion, long Texture, long Header, long Rate, long Total,
    uint Nonzero, int LumaMode, int ChromaMode, bool Intra16, int MaximumEdge);

// Composed private encoder decision. All image/state/result buffers are borrowed;
// the enclosing operation admits ScratchBytes once and checks cancellation below. Source
// pixels use compact Y16x16 followed by eight rows of U8|V8, all with stride16.
// This does not switch the public encoder. Method5 refines selected modes;
// method6 applies luma trellis while evaluating each candidate.
internal static class Vp8MacroblockDecision
{
    internal const int ScratchBytes = 16_384;
    private readonly record struct Metric(long D, long S, long H, long R, long Total = 0)
    {
        internal Metric Price(int lambda) => this with { Total = (R + H) * lambda + 256 * (D + S) };
        internal Metric Add(Metric other) => new(D + other.D, S + other.S, H + other.H, R + other.R, Total + other.Total);
    }

    private static int Block(int number) => (number / 4 * 4) * 16 + number % 4 * 4;
    private static int UvBlock(int number) => (number / 4) * 8 + (number % 4 / 2 * 4) * 16 + number % 2 * 4;

    internal static void Transform(ReadOnlySpan<byte> source, ReadOnlySpan<byte> prediction, int offset, Span<short> coefficients)
    {
        Span<short> residual = stackalloc short[16];
        for (var y = 0; y < 4; y++) for (var x = 0; x < 4; x++)
            residual[y * 4 + x] = (short)(source[offset + y * 16 + x] - prediction[offset + y * 16 + x]);
        Vp8ForwardTransform.Dct(residual, coefficients);
    }

    private static bool Flat(ReadOnlySpan<short> levels, int blocks, int limit)
    {
        var count = 0;
        for (var block = 0; block < blocks; block++) for (var p = 1; p < 16; p++)
            if (levels[block * 16 + p] != 0 && ++count > limit) return false;
        return true;
    }

    private static long Spectral(int weight, int distortion) => ((long)weight * distortion + 128) >> 8;

    private static Metric Luma16(ReadOnlySpan<byte> source, ReadOnlySpan<byte> neighbors, ReadOnlySpan<byte> contexts,
        bool hasTop, bool hasLeft, Vp8QuantizationMatrix y1, Vp8QuantizationMatrix y2, Vp8QuantizationCosts costs,
        Vp8RatePlan rate, ReadOnlySpan<ushort> weights, Span<byte> pixels, Span<short> levels,
        OperationContext context, bool trellis, out int bestMode, out uint nonzero, out int maximumEdge)
    {
        Span<byte> trial = stackalloc byte[256]; Span<short> coefficients = stackalloc short[256];
        Span<short> trialLevels = stackalloc short[272]; Span<short> dc = stackalloc short[16]; Span<short> walsh = stackalloc short[16];
        Span<byte> top = stackalloc byte[4]; Span<byte> left = stackalloc byte[4];
        Span<ushort> trellisWeights = stackalloc ushort[16]; if (trellis) Vp8TrellisQuantization.Weights(trellisWeights);
        var flat = true; foreach (var value in source[..256]) if (value != source[0]) { flat = false; break; }
        Metric best = default; bestMode = 0; nonzero = 0;
        for (var mode = 0; mode < 4; mode++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            Vp8BlockPrediction.Fill(16, (Vp8IntraMode)mode, hasTop ? neighbors.Slice(35, 16) : default,
                hasLeft ? neighbors.Slice(1, 16) : default, neighbors[0], trial);
            for (var b = 0; b < 16; b += 4)
                Vp8ForwardTransform.DctFourBlocks(source[Block(b)..], trial[Block(b)..], 16, 16, coefficients.Slice(b * 16, 64));
            for (var b = 0; b < 16; b++) dc[b] = coefficients[b * 16];
            Vp8ForwardTransform.WalshDc(dc, walsh);
            var nz = Vp8BlockQuantization.Apply(walsh, trialLevels[..16], y2) ? 1u << 24 : 0;
            if (trellis) { contexts[..4].CopyTo(top); contexts.Slice(9, 4).CopyTo(left); }
            for (var b = 0; b < 16; b++)
            {
                bool any;
                if (trellis)
                {
                    var x = b & 3; var y = b >> 2;
                    any = Vp8TrellisQuantization.Apply(coefficients.Slice(b * 16, 16), trialLevels.Slice(16 + b * 16, 16),
                        y1, rate, trellisWeights, 0, top[x] + left[y], costs.Luma16Trellis, context);
                    top[x] = left[y] = any ? (byte)1 : (byte)0; trialLevels[16 + b * 16] = 0;
                }
                else
                {
                    coefficients[b * 16] = 0;
                    any = Vp8BlockQuantization.Apply(coefficients.Slice(b * 16, 16), trialLevels.Slice(16 + b * 16, 16), y1);
                }
                if (any) nz |= 1u << b;
            }
            Vp8BlockReconstruction.WriteWalshDc(walsh, coefficients);
            for (var b = 0; b < 16; b++) Vp8BlockReconstruction.Add(coefficients.Slice(b * 16, 16), trial[Block(b)..], 16);
            long distortion = Vp8Distortion.Squared(source, 16, trial, 16, 16, 16);
            var texture = costs.Texture == 0 ? 0 : Spectral(costs.Texture, Vp8Distortion.Texture16(source, 16, trial, 16, weights));
            long bits = rate.Residual(trialLevels[..16], 1, 0, contexts[8] + contexts[17], out _);
            contexts[..4].CopyTo(top); contexts.Slice(9, 4).CopyTo(left);
            for (var by = 0; by < 4; by++) for (var bx = 0; bx < 4; bx++)
            {
                bits += rate.Residual(trialLevels.Slice(16 + (by * 4 + bx) * 16, 16), 0, 1, top[bx] + left[by], out var any);
                top[bx] = left[by] = any ? (byte)1 : (byte)0;
            }
            if (flat) { flat = Flat(trialLevels[16..], 16, 0); if (flat) { distortion *= 2; texture *= 2; } }
            var score = new Metric(distortion, texture, rate.Modes[mode], bits).Price(costs.Luma16);
            if (mode != 0 && score.Total >= best.Total) continue;
            best = score; bestMode = mode; nonzero = nz; trial.CopyTo(pixels); trialLevels.CopyTo(levels);
        }
        maximumEdge = nonzero == 1u << 24 && best.D > costs.MinimumDistortion
            ? Math.Max(Math.Abs((int)levels[1]), Math.Max(Math.Abs((int)levels[2]), Math.Abs((int)levels[4]))) : 0;
        return best.Price(costs.Mode);
    }

    private static byte Top(ReadOnlySpan<byte> neighbors, int x) => x < 16 ? neighbors[35 + x] : neighbors[67 + x - 16];

    private static bool Luma4(ReadOnlySpan<byte> source, ReadOnlySpan<byte> neighbors, ReadOnlySpan<byte> modeContexts,
        ReadOnlySpan<byte> contexts, Vp8QuantizationMatrix matrix, Vp8QuantizationCosts costs, Vp8RatePlan rate,
        ReadOnlySpan<ushort> weights, int headerLimit, long previousScore, Span<byte> pixels, Span<short> levels,
        Span<byte> modes, OperationContext context, bool trellis, out Metric result, out uint nonzero)
    {
        result = new Metric(0, 0, rate.Entropy[145], 0).Price(costs.Mode); nonzero = 0;
        if (headerLimit == 0) return false;
        Span<byte> top = stackalloc byte[8]; Span<byte> left = stackalloc byte[4];
        Span<byte> trial = stackalloc byte[16]; Span<byte> bestPixels = stackalloc byte[16];
        Span<short> coefficients = stackalloc short[16]; Span<short> trialLevels = stackalloc short[16]; Span<short> residual = stackalloc short[16];
        Span<byte> topNz = stackalloc byte[4]; Span<byte> leftNz = stackalloc byte[4];
        Span<ushort> trellisWeights = stackalloc ushort[16]; if (trellis) Vp8TrellisQuantization.Weights(trellisWeights);
        Span<byte> predictions = stackalloc byte[192]; Span<short> transforms = stackalloc short[192];
        contexts[..4].CopyTo(topNz); contexts.Slice(9, 4).CopyTo(leftNz); long headers = 0;
        for (var b = 0; b < 16; b++)
        {
            var bx = b % 4; var by = b / 4; var x = bx * 4; var y = by * 4; var offset = Block(b);
            for (var p = 0; p < 8; p++) top[p] = by == 0 || bx == 3 && p >= 4 ? Top(neighbors, x + p) : pixels[(y - 1) * 16 + x + p];
            for (var p = 0; p < 4; p++) left[p] = bx == 0 ? neighbors[1 + y + p] : pixels[(y + p) * 16 + x - 1];
            var corner = bx == 0 ? neighbors[y] : by == 0 ? Top(neighbors, x - 1) : pixels[(y - 1) * 16 + x - 1];
            var aboveMode = by == 0 ? modeContexts[bx] : modes[b - 4];
            var leftMode = bx == 0 ? modeContexts[4 + by] : modes[b - 1];
            var bestMode = -1; Metric best = default; var bestNonzero = false;
            // Equal boundary samples make all ten predictions the same constant.
            // Only header prices and the non-DC flat-level surcharge differ.
            // Uniform-score reuse stays disabled for cancellable/trellis searches.
            var pure = !trellis && !context.Cancellation.CanBeCanceled;
            var uniform = pure && top.IndexOfAnyExcept(corner) < 0 && left.IndexOfAnyExcept(corner) < 0;
            Metric uniformScore = default; var uniformFlat = false;
            // Prediction/forward math can batch before ordered trellis decisions.
            // Its extra 576 stack bytes fit ScratchBytes above.
            var batch = Vp8ForwardTransform.SupportsFour && !context.Cancellation.CanBeCanceled && !uniform;
            if (batch)
            {
                for (var mode = 0; mode < 10; mode++)
                    Vp8Prediction.SmallBlock((Vp8SmallIntraMode)mode, top, left, corner, predictions.Slice(mode * 16, 16));
                predictions.Slice(128, 32).CopyTo(predictions[160..]);
                for (var mode = 0; mode < 12; mode += 4)
                    Vp8ForwardTransform.DctFourPredictions(source[offset..], 16,
                        predictions.Slice(mode * 16, 64), transforms.Slice(mode * 16, 64));
            }
            for (var mode = 0; mode < 10; mode++)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                if (uniform && mode != 0)
                {
                    var modeHeader = rate.Modes[8 + (aboveMode * 10 + leftMode) * 10 + mode];
                    var repeatedScore = (uniformScore with { H = modeHeader, R = uniformScore.R + (uniformFlat ? 140 : 0) }).Price(costs.Luma4);
                    if (repeatedScore.Total < best.Total) { bestMode = mode; best = repeatedScore; }
                    continue;
                }
                var candidate = batch ? transforms.Slice(mode * 16, 16) : coefficients;
                if (batch) predictions.Slice(mode * 16, 16).CopyTo(trial);
                else
                {
                    Vp8Prediction.SmallBlock((Vp8SmallIntraMode)mode, top, left, corner, trial);
                    for (var yy = 0; yy < 4; yy++) for (var xx = 0; xx < 4; xx++) residual[yy * 4 + xx] = (short)(source[offset + yy * 16 + xx] - trial[yy * 4 + xx]);
                    Vp8ForwardTransform.Dct(residual, candidate);
                }
                var any = trellis
                    ? Vp8TrellisQuantization.Apply(candidate, trialLevels, matrix, rate, trellisWeights, 3, topNz[bx] + leftNz[by], costs.Luma4Trellis, context)
                    : Vp8BlockQuantization.Apply(candidate, trialLevels, matrix);
                var header = rate.Modes[8 + (aboveMode * 10 + leftMode) * 10 + mode];
                long rateCost = mode > 0 && Flat(trialLevels, 1, 3) ? 140 : 0;
                rateCost += rate.Residual(trialLevels, 3, 0, topNz[bx] + leftNz[by], out _);
                // Reconstruction and texture add nonnegative distortion. This
                // rate/header bound cannot discard a strictly better candidate.
                if (bestMode >= 0 && (rateCost + header) * costs.Luma4 >= best.Total) continue;
                Vp8BlockReconstruction.Add(candidate, trial, 4);
                var distortion = Vp8Distortion.Squared(source[offset..], 16, trial, 4, 4, 4);
                var texture = costs.Texture == 0 ? 0 : Spectral(costs.Texture, Vp8Distortion.Texture4(source[offset..], 16, trial, 4, weights));
                var score = new Metric(distortion, texture, header, rateCost).Price(costs.Luma4);
                if (uniform) { uniformScore = score; uniformFlat = Flat(trialLevels, 1, 3); }
                if (bestMode >= 0 && score.Total >= best.Total) continue;
                bestMode = mode; best = score; bestNonzero = any; trial.CopyTo(bestPixels); trialLevels.CopyTo(levels.Slice(b * 16, 16));
            }
            result = result.Add(best.Price(costs.Mode)); headers += best.H;
            if (result.Total >= previousScore || headers > headerLimit) return false;
            modes[b] = (byte)bestMode;
            for (var yy = 0; yy < 4; yy++) bestPixels.Slice(yy * 4, 4).CopyTo(pixels.Slice(offset + yy * 16, 4));
            topNz[bx] = leftNz[by] = bestNonzero ? (byte)1 : (byte)0;
            if (bestNonzero) nonzero |= 1u << b;
        }
        return true;
    }

    private static int Diffuse(ref short coefficient, Vp8QuantizationMatrix matrix)
    {
        var original = (int)coefficient; var magnitude = Math.Abs(original); var reconstructed = 0;
        if (magnitude > matrix.ZeroThresholds[0])
            reconstructed = ((magnitude * matrix.Reciprocals[0] + matrix.Biases[0]) >> 17) * matrix.Steps[0];
        coefficient = checked((short)(original < 0 ? -reconstructed : reconstructed));
        return (original - coefficient) >> 1;
    }

    internal static void CorrectDc(Span<short> coefficients, ReadOnlySpan<sbyte> input, Span<sbyte> errors, Vp8QuantizationMatrix matrix)
    {
        for (var channel = 0; channel < 2; channel++)
        {
            var c = coefficients.Slice(channel * 64, 64); var at = channel * 2;
            c[0] += (short)((7 * input[at] + 8 * input[4 + at]) >> 3); var e0 = Diffuse(ref c[0], matrix);
            c[16] += (short)((7 * input[at + 1] + 8 * e0) >> 3); var e1 = Diffuse(ref c[16], matrix);
            c[32] += (short)((7 * e0 + 8 * input[5 + at]) >> 3); var e2 = Diffuse(ref c[32], matrix);
            c[48] += (short)((7 * e1 + 8 * e2) >> 3); var e3 = Diffuse(ref c[48], matrix);
            errors[channel * 3] = checked((sbyte)e1); errors[channel * 3 + 1] = checked((sbyte)e2); errors[channel * 3 + 2] = checked((sbyte)e3);
        }
    }

    private static Metric Chroma(ReadOnlySpan<byte> source, ReadOnlySpan<byte> neighbors, ReadOnlySpan<byte> contexts,
        ReadOnlySpan<sbyte> diffusion, bool hasTop, bool hasLeft, Vp8QuantizationMatrix matrix, Vp8QuantizationCosts costs,
        Vp8RatePlan rate, Span<byte> pixels, Span<short> levels, Span<sbyte> errors, OperationContext context,
        out int bestMode, out uint nonzero, bool correctDc)
    {
        Span<byte> trial = stackalloc byte[128]; Span<byte> prediction = stackalloc byte[64];
        Span<short> coefficients = stackalloc short[128]; Span<short> trialLevels = stackalloc short[128];
        Span<sbyte> trialErrors = stackalloc sbyte[6]; Span<byte> topNz = stackalloc byte[4]; Span<byte> leftNz = stackalloc byte[4];
        Metric best = default; bestMode = 0; nonzero = 0;
        for (var mode = 0; mode < 4; mode++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            for (var plane = 0; plane < 2; plane++)
            {
                var left = plane == 0 ? 17 : 26;
                Vp8BlockPrediction.Fill(8, (Vp8IntraMode)mode, hasTop ? neighbors.Slice(51 + plane * 8, 8) : default,
                    hasLeft ? neighbors.Slice(left + 1, 8) : default, neighbors[left], prediction);
                for (var y = 0; y < 8; y++) prediction.Slice(y * 8, 8).CopyTo(trial.Slice(y * 16 + plane * 8, 8));
            }
            Vp8ForwardTransform.DctFourBlocks(source, trial, 16, 16, coefficients, 64);
            Vp8ForwardTransform.DctFourBlocks(source[64..], trial[64..], 16, 16, coefficients[32..], 64);
            if (correctDc) CorrectDc(coefficients, diffusion, trialErrors, matrix);
            else trialErrors.Clear();
            uint nz = 0;
            for (var b = 0; b < 8; b++)
            {
                if (Vp8BlockQuantization.Apply(coefficients.Slice(b * 16, 16), trialLevels.Slice(b * 16, 16), matrix)) nz |= 1u << (16 + b);
                Vp8BlockReconstruction.Add(coefficients.Slice(b * 16, 16), trial[UvBlock(b)..], 16);
            }
            contexts.Slice(4, 4).CopyTo(topNz); contexts.Slice(13, 4).CopyTo(leftNz); long bits = 0;
            for (var plane = 0; plane < 2; plane++) for (var y = 0; y < 2; y++) for (var x = 0; x < 2; x++)
            {
                var above = plane * 2 + x; var side = plane * 2 + y;
                bits += rate.Residual(trialLevels.Slice((plane * 4 + y * 2 + x) * 16, 16), 2, 0, topNz[above] + leftNz[side], out var any);
                topNz[above] = leftNz[side] = any ? (byte)1 : (byte)0;
            }
            if (mode > 0 && Flat(trialLevels, 8, 2)) bits += 1120;
            var score = new Metric(Vp8Distortion.Squared(source, 16, trial, 16, 16, 8), 0, rate.Modes[4 + mode], bits).Price(costs.Chroma);
            if (mode != 0 && score.Total >= best.Total) continue;
            best = score; bestMode = mode; nonzero = nz; trial.CopyTo(pixels); trialLevels.CopyTo(levels); trialErrors.CopyTo(errors);
        }
        for (var channel = 0; channel < 2; channel++)
        {
            var at = channel * 2; var e3 = errors[channel * 3 + 2]; var side = 3 * e3 >> 2;
            errors[6 + at] = errors[channel * 3 + 1]; errors[7 + at] = checked((sbyte)(e3 - side));
            errors[10 + at] = errors[channel * 3]; errors[11 + at] = checked((sbyte)side);
        }
        return best;
    }

    // Method5 keeps the original RDO score/modes/filter statistic, then repeats
    // reconstruction for those modes with luma trellis. Chroma remains ordinary
    // quantization but uses the FIRST selected diffusion edges. Its second six
    // errors replace only the local result, never the edges retained for next MB.
    private static uint ReconstructSelected(ReadOnlySpan<byte> source, ReadOnlySpan<byte> neighbors,
        ReadOnlySpan<byte> contexts, bool hasTop, bool hasLeft, bool intra16, int lumaMode, int uvMode,
        ReadOnlySpan<byte> modes, Vp8QuantizationMatrix luma, Vp8QuantizationMatrix walsh,
        Vp8QuantizationMatrix chroma, Vp8QuantizationCosts costs, Vp8RatePlan rate, bool correctDc,
        Span<byte> pixels, Span<short> levels, Span<sbyte> errors, OperationContext context)
    {
        context.Cancellation.ThrowIfCancellationRequested();
        Span<short> coefficients = stackalloc short[256]; Span<short> dc = stackalloc short[16]; Span<short> y2 = stackalloc short[16];
        Span<ushort> weights = stackalloc ushort[16]; Vp8TrellisQuantization.Weights(weights);
        Span<byte> topNz = stackalloc byte[4]; Span<byte> leftNz = stackalloc byte[4];
        contexts[..4].CopyTo(topNz); contexts.Slice(9, 4).CopyTo(leftNz); uint nz = 0;
        if (intra16)
        {
            Vp8BlockPrediction.Fill(16, (Vp8IntraMode)lumaMode, hasTop ? neighbors.Slice(35, 16) : default,
                hasLeft ? neighbors.Slice(1, 16) : default, neighbors[0], pixels[..256]);
            for (var b = 0; b < 16; b += 4)
                Vp8ForwardTransform.DctFourBlocks(source[Block(b)..], pixels[Block(b)..], 16, 16, coefficients.Slice(b * 16, 64));
            for (var b = 0; b < 16; b++) dc[b] = coefficients[b * 16];
            Vp8ForwardTransform.WalshDc(dc, y2);
            if (Vp8BlockQuantization.Apply(y2, levels[..16], walsh)) nz |= 1u << 24;
            for (var b = 0; b < 16; b++)
            {
                var x = b & 3; var y = b >> 2;
                var any = Vp8TrellisQuantization.Apply(coefficients.Slice(b * 16, 16), levels.Slice(16 + b * 16, 16),
                    luma, rate, weights, 0, topNz[x] + leftNz[y], costs.Luma16Trellis, context);
                topNz[x] = leftNz[y] = any ? (byte)1 : (byte)0; levels[16 + b * 16] = 0;
                if (any) nz |= 1u << b;
            }
            Vp8BlockReconstruction.WriteWalshDc(y2, coefficients);
            for (var b = 0; b < 16; b++) Vp8BlockReconstruction.Add(coefficients.Slice(b * 16, 16), pixels[Block(b)..], 16);
        }
        else
        {
            Span<byte> top = stackalloc byte[8]; Span<byte> left = stackalloc byte[4]; Span<byte> trial = stackalloc byte[16];
            Span<short> residual = stackalloc short[16];
            for (var b = 0; b < 16; b++)
            {
                var bx = b & 3; var by = b >> 2; var x = bx * 4; var y = by * 4; var offset = Block(b);
                for (var p = 0; p < 8; p++) top[p] = by == 0 || bx == 3 && p >= 4 ? Top(neighbors, x + p) : pixels[(y - 1) * 16 + x + p];
                for (var p = 0; p < 4; p++) left[p] = bx == 0 ? neighbors[1 + y + p] : pixels[(y + p) * 16 + x - 1];
                var corner = bx == 0 ? neighbors[y] : by == 0 ? Top(neighbors, x - 1) : pixels[(y - 1) * 16 + x - 1];
                Vp8Prediction.SmallBlock((Vp8SmallIntraMode)modes[b], top, left, corner, trial);
                for (var yy = 0; yy < 4; yy++) for (var xx = 0; xx < 4; xx++) residual[yy * 4 + xx] = (short)(source[offset + yy * 16 + xx] - trial[yy * 4 + xx]);
                Vp8ForwardTransform.Dct(residual, coefficients);
                var any = Vp8TrellisQuantization.Apply(coefficients[..16], levels.Slice(16 + b * 16, 16),
                    luma, rate, weights, 3, topNz[bx] + leftNz[by], costs.Luma4Trellis, context);
                Vp8BlockReconstruction.Add(coefficients[..16], trial, 4);
                for (var yy = 0; yy < 4; yy++) trial.Slice(yy * 4, 4).CopyTo(pixels.Slice(offset + yy * 16, 4));
                // Selected-mode I4 refinement reads the original edge contexts
                // for every block. Unlike I16/candidate RDO it does not advance
                // them after each reconstructed block.
                if (any) nz |= 1u << b;
            }
        }
        Span<byte> prediction = stackalloc byte[64]; var uvPixels = pixels.Slice(256, 128);
        for (var plane = 0; plane < 2; plane++)
        {
            var left = plane == 0 ? 17 : 26;
            Vp8BlockPrediction.Fill(8, (Vp8IntraMode)uvMode, hasTop ? neighbors.Slice(51 + plane * 8, 8) : default,
                hasLeft ? neighbors.Slice(left + 1, 8) : default, neighbors[left], prediction);
            for (var y = 0; y < 8; y++) prediction.Slice(y * 8, 8).CopyTo(uvPixels.Slice(y * 16 + plane * 8, 8));
        }
        Vp8ForwardTransform.DctFourBlocks(source[256..], uvPixels, 16, 16, coefficients, 64);
        Vp8ForwardTransform.DctFourBlocks(source[320..], uvPixels[64..], 16, 16, coefficients[32..], 64);
        if (correctDc) CorrectDc(coefficients[..128], errors.Slice(6, 8), errors[..6], chroma);
        else errors[..6].Clear();
        for (var b = 0; b < 8; b++)
        {
            if (Vp8BlockQuantization.Apply(coefficients.Slice(b * 16, 16), levels.Slice(272 + b * 16, 16), chroma)) nz |= 1u << (16 + b);
            Vp8BlockReconstruction.Add(coefficients.Slice(b * 16, 16), uvPixels[UvBlock(b)..], 16);
        }
        return nz;
    }

    // Neighbors: Yleft(corner+16), Uleft(corner+8), Vleft(corner+8), Ytop16,
    // UVtop16 and Ytop-right4. Modes: top4/left4. NZ contexts: top9/left9.
    // Diffusion input: topU2V2/leftU2V2; output: selected6errors/top4/left4.
    internal static Vp8MacroblockScore Select(ReadOnlySpan<byte> source, ReadOnlySpan<byte> neighbors,
        ReadOnlySpan<byte> modeContexts, ReadOnlySpan<byte> coefficientContexts, ReadOnlySpan<sbyte> diffusion,
        bool hasTop, bool hasLeft, Vp8QuantizationMatrix luma, Vp8QuantizationMatrix walsh, Vp8QuantizationMatrix chroma,
        Vp8QuantizationCosts costs, Vp8RatePlan rate, ReadOnlySpan<ushort> weights, int headerLimit,
        Span<byte> pixels, Span<short> levels, Span<byte> modes, Span<sbyte> errors, OperationContext context, bool correctDc = true, int method = 4)
    {
        if (method is < 3 or > 6) throw new ArgumentOutOfRangeException(nameof(method));
        if (source.Length < 384 || neighbors.Length < 71 || modeContexts.Length < 8 || coefficientContexts.Length < 18
            || diffusion.Length < 8 || weights.Length < 16 || headerLimit < 0 || pixels.Length < 384 || levels.Length < 400 || modes.Length < 16 || errors.Length < 14)
            throw new ArgumentException("Incomplete VP8 macroblock state or result layout.");
        foreach (var mode in modeContexts[..8]) if (mode > 9) throw new ArgumentException("Invalid neighbor prediction mode.");
        foreach (var value in coefficientContexts[..18]) if (value > 1) throw new ArgumentException("Invalid nonzero context.");
        var sum = 0; foreach (var weight in weights[..16]) sum += weight;
        if (sum != 256) throw new ArgumentException("Texture importance must sum to 256.");
        var levelBytes = MemoryMarshal.AsBytes(levels[..400]); var errorBytes = MemoryMarshal.AsBytes(errors[..14]);
        if (pixels[..384].Overlaps(levelBytes) || pixels[..384].Overlaps(modes[..16]) || pixels[..384].Overlaps(errorBytes)
            || levelBytes.Overlaps(modes[..16]) || levelBytes.Overlaps(errorBytes) || modes[..16].Overlaps(errorBytes))
            throw new ArgumentException("Macroblock result buffers must not overlap.");
        context.Cancellation.ThrowIfCancellationRequested();
        // Snapshot all small borrowed inputs before any result write. Caller
        // image/border/context views may alias result storage without feedback.
        Span<byte> snapshot = stackalloc byte[481]; source[..384].CopyTo(snapshot); neighbors[..71].CopyTo(snapshot[384..]);
        modeContexts[..8].CopyTo(snapshot[455..]); coefficientContexts[..18].CopyTo(snapshot[463..]);
        Span<sbyte> errorInput = stackalloc sbyte[8]; diffusion[..8].CopyTo(errorInput);
        Span<ushort> importance = stackalloc ushort[16]; weights[..16].CopyTo(importance);
        var sourceCopy = snapshot[..384]; var neighborsCopy = snapshot.Slice(384, 71); var modeCopy = snapshot.Slice(455, 8); var coefficientCopy = snapshot.Slice(463, 18);
        Span<byte> smallPixels = stackalloc byte[256]; Span<short> smallLevels = stackalloc short[256]; Span<byte> smallModes = stackalloc byte[16];
        var best = Luma16(sourceCopy, neighborsCopy, coefficientCopy, hasTop, hasLeft, luma, walsh, costs, rate, importance,
            pixels[..256], levels[..272], context, method == 6, out var lumaMode, out var nz, out var maxEdge);
        var intra16 = !Luma4(sourceCopy, neighborsCopy, modeCopy, coefficientCopy, luma, costs, rate, importance, headerLimit,
            best.Total, smallPixels, smallLevels, smallModes, context, method == 6, out var smallScore, out var smallNz);
        if (intra16) modes[..16].Clear();
        else
        {
            best = smallScore; nz = smallNz; smallPixels.CopyTo(pixels); smallLevels.CopyTo(levels[16..]); levels[..16].Clear(); smallModes.CopyTo(modes);
        }
        var uv = Chroma(sourceCopy[256..], neighborsCopy, coefficientCopy, errorInput, hasTop, hasLeft, chroma, costs, rate,
            pixels.Slice(256, 128), levels.Slice(272, 128), errors[..14], context, out var uvMode, out var uvNz, correctDc);
        best = best.Add(uv);
        nz |= uvNz;
        if (method == 5) nz = ReconstructSelected(sourceCopy, neighborsCopy, coefficientCopy, hasTop, hasLeft,
            intra16, lumaMode, uvMode, modes[..16], luma, walsh, chroma, costs, rate, correctDc, pixels[..384], levels[..400], errors[..14], context);
        return new(best.D, best.S, best.H, best.R, best.Total, nz, lumaMode, uvMode, intra16, maxEdge);
    }
}
