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
        Vp8lEncodingAnalysis analysis, int effort, int paletteColors, int quality)
    {
        var tryNoCache = effort == 5 && quality >= 75 || effort == 6 && quality == 100;
        var low = effort == 0; var maximumCacheBits = quality <= 25 ? 0 : paletteColors == 0 ? 10 : BitOperations.Log2((uint)paletteColors) + 1;
        using var matches = Vp8lReferenceMatcher.Build(pixels, width, quality, low, context);
        if (low)
        {
            var tokens = Vp8lReferenceLz.Greedy(pixels, matches.Values, context);
            Consider(candidatePrefix, Vp8lImageCode.TakeMain(tokens, width, height, analysis.HistogramBits, true, context, quality));
            return;
        }
        var ordinary = Vp8lReferenceMainParsing.Encode(pixels, matches.Values, width, false, maximumCacheBits, context, quality >= 25);
        var compareOrdinary = tryNoCache && ordinary.CacheBits != 0;
        Consider(candidatePrefix, Vp8lImageCode.TakeMain(ordinary, width, height, analysis.HistogramBits, false, context, quality));
        // Native already considers this profile complete when caching chose0.
        if (compareOrdinary)
        {
            var uncached = Vp8lReferenceMainParsing.Encode(pixels, matches.Values, width, true, maximumCacheBits, context, quality >= 25);
            Consider(candidatePrefix, Vp8lImageCode.TakeMain(uncached, width, height, analysis.HistogramBits, false, context, quality));
        }
        if (paletteColors is >= 1 and <= 16)
        {
            using var box = Vp8lReferenceMatcher.BuildBox(pixels, width, matches.Values, context);
            var boxed = Vp8lReferenceMainParsing.EncodeBox(pixels, box.Values, width, false, maximumCacheBits, context, quality >= 25);
            var compareBox = tryNoCache && boxed.CacheBits != 0;
            Consider(candidatePrefix, Vp8lImageCode.TakeMain(boxed, width, height, analysis.HistogramBits, false, context, quality));
            if (compareBox)
            {
                var uncached = Vp8lReferenceMainParsing.EncodeBox(pixels, box.Values, width, true, maximumCacheBits, context, quality >= 25);
                Consider(candidatePrefix, Vp8lImageCode.TakeMain(uncached, width, height, analysis.HistogramBits, false, context, quality));
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
        Vp8lFramePrefix candidatePrefix, int quality)
    {
        var maximum = ClampPredictionBits(width, height, transformBits);
        var minimum = ClampPredictionBits(width, height, maximum - 2 * Math.Max(0, effort - 4));
        using var plan = Vp8lReferencePredictor.SelectAndApply(pixels, width, minimum, maximum, effort == 0, context);
        var reserved = plan.Modes.Length * 4L; context.Reserve(reserved);
        try
        {
            context.Work(0); var map = new uint[plan.Modes.Length];
            for (var i = 0; i < map.Length; i++) { context.Work(1); map[i] = 0xff000000u | (uint)plan.Modes[i] << 8; }
            prediction = Vp8lImageCode.Auxiliary(map, (width + (1 << plan.StoredBits) - 1) >> plan.StoredBits, quality, effort == 0, context);
        }
        finally { context.Release(reserved); }
        return candidatePrefix with { Prediction = prediction, PredictionBits = plan.StoredBits };
    }

    // working is the library's admitted normalization buffer and is modified in
    // place. Its owner keeps the full capacity charged, including palette tails.
    // Returned prepared state owns no reference to this buffer.
    internal static Vp8lPreparedEncoding Prepare(Span<uint> working, int width, int effort, OperationContext context, int quality = 75)
    {
        if (quality is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(quality));
        var analysis = Vp8lEncodingAnalysis.Select(working, width, effort, context);
        return PrepareCore(working, width, effort, context, quality, analysis, comparePalettePrediction: effort == 5 && quality >= 75);
    }

    // One explicit family from the method6/quality100 search. Analysis is shared
    // across families so the original global red/blue-zero decision is retained.
    // This owner holds its full transform/coding bundle independently of source
    // storage and other candidates. Alpha method6 routing remains separate.
    internal static Vp8lPreparedEncoding PrepareCandidate(Span<uint> working, int width,
        Vp8lEncodingAnalysis analysis, Vp8lTransformKind transform, OperationContext context, Vp8lPaletteOrder paletteOrder = Vp8lPaletteOrder.MinDelta)
    {
        if (width is < 1 or > 16384 || working.Length == 0 || working.Length % width != 0 || working.Length / width > 16384
            || (uint)transform > (uint)Vp8lTransformKind.PalettePrediction || analysis.HistogramBits is < 2 or > 9 || analysis.TransformBits is < 2 or > 9)
            throw new ArgumentException("Invalid VP8L search candidate layout.");
        if (transform is Vp8lTransformKind.Palette or Vp8lTransformKind.PalettePrediction && analysis.DistinctColorsCapped is < 1 or > 256)
            throw new ArgumentException("Palette candidate requires a representable source palette.");
        if ((uint)paletteOrder > (uint)Vp8lPaletteOrder.ModifiedZeng) throw new ArgumentOutOfRangeException(nameof(paletteOrder));
        context.Work(0);
        return PrepareCore(working, width, 6, context, 100, analysis with { Transform = transform }, comparePalettePrediction: false, paletteOrder);
    }

    private static Vp8lPreparedEncoding PrepareCore(Span<uint> working, int width, int effort,
        OperationContext context, int quality, Vp8lEncodingAnalysis analysis, bool comparePalettePrediction, Vp8lPaletteOrder? paletteOrder = null)
    {
        var height = working.Length / width; var result = new Vp8lPreparedEncoding(context);
        var candidatePrefix = new Vp8lFramePrefix(width, height, analysis.HasNonOpaqueAlpha);
        try
        {
            if (analysis.Transform is Vp8lTransformKind.Palette or Vp8lTransformKind.PalettePrediction)
            {
                using var plan = Vp8lReferencePalette.SelectAndPack(working, width, analysis.DistinctColorsCapped,
                    paletteOrder ?? (effort == 0 ? Vp8lPaletteOrder.Sorted : Vp8lPaletteOrder.MinDelta), context);
                result.palette = Vp8lImageCode.Auxiliary(plan.EncodedDeltas, plan.EncodedColorCount, 20, effort == 0, context);
                candidatePrefix = candidatePrefix with { Palette = result.palette, PaletteEntries = plan.EncodedColorCount };
                var input = working[..plan.PackedPixelCount];
                if (analysis.Transform == Vp8lTransformKind.PalettePrediction)
                    candidatePrefix = result.Predict(input, plan.CodedWidth, height, effort, analysis.TransformBits, candidatePrefix, quality);
                result.MainCandidates(input, plan.CodedWidth, height, candidatePrefix, analysis, effort, plan.ColorCount, quality);
                if (comparePalettePrediction)
                {
                    // Selected plain tokens contain all literal values and copy
                    // distances. They survive this in-place spatial transform.
                    candidatePrefix = result.Predict(input, plan.CodedWidth, height, effort, analysis.TransformBits, candidatePrefix, quality);
                    result.MainCandidates(input, plan.CodedWidth, height, candidatePrefix, analysis, effort, plan.ColorCount, quality);
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
                    candidatePrefix = result.Predict(working, width, height, effort, analysis.TransformBits, candidatePrefix, quality);
                    if (effort != 0 && !analysis.RedBlueZero)
                    {
                        using var plan = Vp8lReferenceColorTransform.SelectAndApply(working, width, analysis.TransformBits, quality, context);
                        result.color = Vp8lImageCode.Auxiliary(plan.Coefficients, (width + (1 << plan.StoredBits) - 1) >> plan.StoredBits, quality, false, context);
                        candidatePrefix = candidatePrefix with { Color = result.color, ColorBits = plan.StoredBits };
                    }
                }
                result.MainCandidates(working, width, height, candidatePrefix, analysis, effort, 0, quality);
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

    internal void WriteAlphaBody(Stream output)
        => Vp8lReferenceFrame.WritePayload(output, prefix, image ?? throw new ObjectDisposedException(nameof(Vp8lPreparedEncoding)), context, includeHeader: false);

    public void Dispose()
    {
        Interlocked.Exchange(ref image, null)?.Dispose(); Interlocked.Exchange(ref color, null)?.Dispose();
        Interlocked.Exchange(ref prediction, null)?.Dispose(); Interlocked.Exchange(ref palette, null)?.Dispose();
    }
}
