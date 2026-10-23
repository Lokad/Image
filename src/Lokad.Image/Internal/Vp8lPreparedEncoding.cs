using System.Numerics;

namespace Lokad.Image.Internal;

// Owns the selected main coding state and the few transform images shared by
// candidate comparisons. No source pixels or encoded-frame buffer are retained.
internal sealed class Vp8lPreparedEncoding : IDisposable
{
    private readonly OperationContext context;
    private Vp8lImageCode? palette, prediction, color, image;
    private Vp8lFramePrefix prefix;
    internal int PayloadBytes { get; private set; } = int.MaxValue;
    private Vp8lPreparedEncoding(OperationContext context) => this.context = context;

    private void Consider(Vp8lFramePrefix candidatePrefix, Vp8lImageCode candidate)
    {
        try
        {
            var bytes = Vp8lReferenceFrame.CountPayload(candidatePrefix, candidate, context);
            if (bytes < PayloadBytes)
            {
                image?.Dispose(); image = candidate; prefix = candidatePrefix; PayloadBytes = bytes;
                candidate = null!;
            }
        }
        finally { candidate?.Dispose(); }
    }

    private void MainCandidates(ReadOnlySpan<uint> pixels, int width, int height, Vp8lFramePrefix candidatePrefix,
        Vp8lEncodingAnalysis analysis, int effort, int paletteColors)
    {
        var low = effort == 0; var maximumCacheBits = paletteColors == 0 ? 10 : BitOperations.Log2((uint)paletteColors) + 1;
        using var matches = Vp8lReferenceMatcher.Build(pixels, width, 75, low, context);
        if (low)
        {
            var tokens = Vp8lReferenceLz.Greedy(pixels, matches.Values, context);
            Consider(candidatePrefix, Vp8lImageCode.TakeMain(tokens, width, height, analysis.HistogramBits, true, context));
            return;
        }
        var ordinary = Vp8lReferenceMainParsing.Encode(pixels, matches.Values, width, false, maximumCacheBits, context);
        Consider(candidatePrefix, Vp8lImageCode.TakeMain(ordinary, width, height, analysis.HistogramBits, false, context));
        if (effort == 5)
        {
            var uncached = Vp8lReferenceMainParsing.Encode(pixels, matches.Values, width, true, maximumCacheBits, context);
            Consider(candidatePrefix, Vp8lImageCode.TakeMain(uncached, width, height, analysis.HistogramBits, false, context));
        }
        if (paletteColors is >= 1 and <= 16)
        {
            using var box = Vp8lReferenceMatcher.BuildBox(pixels, width, matches.Values, context);
            var boxed = Vp8lReferenceMainParsing.EncodeBox(pixels, box.Values, width, false, maximumCacheBits, context);
            Consider(candidatePrefix, Vp8lImageCode.TakeMain(boxed, width, height, analysis.HistogramBits, false, context));
            if (effort == 5)
            {
                var uncached = Vp8lReferenceMainParsing.EncodeBox(pixels, box.Values, width, true, maximumCacheBits, context);
                Consider(candidatePrefix, Vp8lImageCode.TakeMain(uncached, width, height, analysis.HistogramBits, false, context));
            }
        }
    }

    private static int ClampPredictionBits(int width, int height, int bits)
    {
        bits = Math.Clamp(bits, 2, 9);
        int Tiles(int b) => ((width + (1 << b) - 1) >> b) * ((height + (1 << b) - 1) >> b);
        while (bits < 9 && Tiles(bits) > 16384) bits++;
        while (bits > 2 && Tiles(bits - 1) == 1) bits--;
        return bits;
    }

    private Vp8lFramePrefix Predict(Span<uint> pixels, int width, int height, int effort, int transformBits,
        Vp8lFramePrefix candidatePrefix)
    {
        var maximum = ClampPredictionBits(width, height, transformBits);
        var minimum = ClampPredictionBits(width, height, maximum - 2 * Math.Max(0, effort - 4));
        using var plan = Vp8lReferencePredictor.SelectAndApply(pixels, width, minimum, maximum, effort == 0, context);
        var reserved = plan.Modes.Length * 4L; context.Reserve(reserved);
        try
        {
            context.Work(0); var map = new uint[plan.Modes.Length];
            for (var i = 0; i < map.Length; i++) { context.Work(1); map[i] = 0xff000000u | (uint)plan.Modes[i] << 8; }
            prediction = Vp8lImageCode.Auxiliary(map, (width + (1 << plan.StoredBits) - 1) >> plan.StoredBits, 75, effort == 0, context);
        }
        finally { context.Release(reserved); }
        return candidatePrefix with { Prediction = prediction, PredictionBits = plan.StoredBits };
    }

    // working is the library's admitted normalization buffer and is modified in
    // place. Its owner keeps the full capacity charged, including palette tails.
    // Returned prepared state owns no reference to this buffer.
    internal static Vp8lPreparedEncoding Prepare(Span<uint> working, int width, int effort, OperationContext context)
    {
        var analysis = Vp8lEncodingAnalysis.Select(working, width, effort, context);
        var height = working.Length / width; var result = new Vp8lPreparedEncoding(context);
        var candidatePrefix = new Vp8lFramePrefix(width, height, analysis.HasNonOpaqueAlpha);
        try
        {
            if (analysis.Transform == Vp8lTransformKind.Palette)
            {
                using var plan = Vp8lReferencePalette.SelectAndPack(working, width, analysis.DistinctColorsCapped, effort != 0, context);
                result.palette = Vp8lImageCode.Auxiliary(plan.EncodedDeltas, plan.EncodedColorCount, 20, effort == 0, context);
                candidatePrefix = candidatePrefix with { Palette = result.palette, PaletteEntries = plan.EncodedColorCount };
                var input = working[..plan.PackedPixelCount];
                result.MainCandidates(input, plan.CodedWidth, height, candidatePrefix, analysis, effort, plan.ColorCount);
                if (effort == 5)
                {
                    // Selected plain tokens contain all literal values and copy
                    // distances. They survive this in-place spatial transform.
                    candidatePrefix = result.Predict(input, plan.CodedWidth, height, effort, analysis.TransformBits, candidatePrefix);
                    result.MainCandidates(input, plan.CodedWidth, height, candidatePrefix, analysis, effort, plan.ColorCount);
                }
            }
            else
            {
                var subtractGreen = analysis.Transform is Vp8lTransformKind.SubtractGreen or Vp8lTransformKind.PredictionSubtractGreen;
                var predict = analysis.Transform is Vp8lTransformKind.Prediction or Vp8lTransformKind.PredictionSubtractGreen;
                if (subtractGreen)
                { Vp8lReferenceColorTransform.SubtractGreen(working, context); candidatePrefix = candidatePrefix with { SubtractGreen = true }; }
                if (predict)
                {
                    candidatePrefix = result.Predict(working, width, height, effort, analysis.TransformBits, candidatePrefix);
                    if (effort != 0 && !analysis.RedBlueZero)
                    {
                        using var plan = Vp8lReferenceColorTransform.SelectAndApply(working, width, analysis.TransformBits, 75, context);
                        result.color = Vp8lImageCode.Auxiliary(plan.Coefficients, (width + (1 << plan.StoredBits) - 1) >> plan.StoredBits, 75, false, context);
                        candidatePrefix = candidatePrefix with { Color = result.color, ColorBits = plan.StoredBits };
                    }
                }
                result.MainCandidates(working, width, height, candidatePrefix, analysis, effort, 0);
            }
            if (!ReferenceEquals(result.prefix.Prediction, result.prediction))
            { result.prediction?.Dispose(); result.prediction = null; }
            if (!ReferenceEquals(result.prefix.Color, result.color))
            { result.color?.Dispose(); result.color = null; }
            return result;
        }
        catch { result.Dispose(); throw; }
    }

    internal void Write(Stream output)
        => Vp8lReferenceFrame.Write(output, prefix, image ?? throw new ObjectDisposedException(nameof(Vp8lPreparedEncoding)), PayloadBytes, context);

    public void Dispose()
    {
        Interlocked.Exchange(ref image, null)?.Dispose(); Interlocked.Exchange(ref color, null)?.Dispose();
        Interlocked.Exchange(ref prediction, null)?.Dispose(); Interlocked.Exchange(ref palette, null)?.Dispose();
    }
}
