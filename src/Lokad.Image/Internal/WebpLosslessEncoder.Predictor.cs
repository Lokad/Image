namespace Lokad.Image.Internal;

internal static partial class WebpLosslessEncoder
{
    private static CachedPlan SelectCachedPlan(ImageBuffer image, int effort, uint[] cache, OperationContext context)
    {
        var raw = PrepareCachedTables(image, effort, cache, false, context);
        // Few literal misses can still leave expensive cache symbols. Retain
        // literal-heavy eligibility and also consider a residual when the
        // already gathered code cost exceeds one byte per source pixel.
        var count = (long)image.Width * image.Height;
        if (raw.Literals <= count / 4 && raw.BodyBits <= count * 8) return raw;
        var predicted = PrepareCachedTables(image, effort, cache, true, context) with { Alpha = raw.Alpha };
        var rawUpper = PayloadUpper(image, raw, context);
        var predictedUpper = PayloadUpper(image, predicted, context);
        // Header and body are rounded separately in this estimate. It exceeds
        // the actual payload by at most one byte. This gap also covers RIFF
        // padding, so selection cannot enlarge the original complete packet.
        if (predictedUpper + 2 < rawUpper)
        {
            ReleasePlan(raw, context);
            var baseline = SelectEntropyPlan(image, effort, cache, predicted, context);
            baseline = SelectAdaptivePlan(image, effort, cache, baseline, context);
            return SelectColorPlan(image, effort, cache, baseline, context);
        }
        ReleasePlan(predicted, context); return raw;
    }

    private static long PayloadUpper(ImageBuffer image, CachedPlan plan, OperationContext context)
    {
        // This measures only a bounded header, independently of the caller's
        // output limit. The selected full payload is counted with the real limit
        // before output; an unselected header must not reject a smaller packet.
        using var counter = new CountStream(int.MaxValue);
        var writer = CachedHeader(image, counter, plan, context); writer.Finish();
        return counter.Count + (plan.BodyBits + 7) / 8;
    }

    private static void ReleasePlan(CachedPlan plan, OperationContext context)
    {
        ReleaseColorPlan(plan, context);
        if (plan.Modes is { } modes) context.Release(modes.Length + 32L + plan.ModeTable!.MemoryBytes);
    }

    // Entropy-group replacement transfers the SAME predictor map to its result.
    // Release coding storage alone while this shared map remains live and charged.
    private static void ReleaseCodingPlan(CachedPlan plan, OperationContext context)
    {
        long bytes = plan.Tables.Length * 8L + 32;
        foreach (var table in plan.Tables) bytes += table.MemoryBytes;
        if (plan.Groups is { } groups) bytes += groups.Map.Length + 80L + groups.MapTable.MemoryBytes;
        context.Release(bytes);
    }

    private static void ReleaseColorPlan(CachedPlan plan, OperationContext context)
    {
        ReleaseCodingPlan(plan, context);
        if (plan.Color is { } color)
        {
            context.Release(color.Coefficients.Length + 64L);
            ReleaseTables(color.Tables, context);
        }
    }

    private static uint CachedPixel(ImageBuffer image, int position, bool predict, OperationContext context,
        byte[]? modes = null, ColorPlan? color = null)
    {
        var pixel = modes is not null ? AdaptiveResidual(image, position, modes, context)
            : predict ? LeftResidual(image, position, context) : GreenPixel(image, position, context);
        if (color is null) return pixel;
        var at = ((position / image.Width >> 3) * ((image.Width + 7) >> 3) + (position % image.Width >> 3)) * 3;
        context.Work(16);
        return ColorResidual(pixel, unchecked((sbyte)color.Coefficients[at]),
            unchecked((sbyte)color.Coefficients[at + 1]), unchecked((sbyte)color.Coefficients[at + 2]));
    }

    // RFC9649 section3.5.1: mode1 is L except for the first pixel (opaque black)
    // and first column (T). Subtract after green decorrelation, per channel mod256.
    internal static uint LeftResidual(ImageBuffer image, int position, OperationContext context)
    {
        var pixel = GreenPixel(image, position, context);
        var predicted = position == 0 ? 0xff000000u
            : GreenPixel(image, position % image.Width == 0 ? position - image.Width : position - 1, context);
        context.Work(4);
        return unchecked(((pixel - predicted) & 255)
            | (((pixel >> 8) - (predicted >> 8)) & 255) << 8
            | (((pixel >> 16) - (predicted >> 16)) & 255) << 16
            | (((pixel >> 24) - (predicted >> 24)) & 255) << 24);
    }

    private static void LeftPredictorHeader(ImageBuffer image, WebpBitWriter writer, OperationContext context)
    {
        writer.Write(1, 1); writer.Write(0, 2); writer.Write(7, 3); // Predictor,512x512tiles.
        writer.Write(0, 1); // Auxiliary image: no cache and no spatial-group flag.
        // The same mode1 fills every tile. Its five simple single-leaf trees
        // have green1/red0/blue0/alpha255/dummy-distance0 and zero data bits.
        SingleHeader(writer, 1); SingleHeader(writer, 0); SingleHeader(writer, 0);
        SingleHeader(writer, 255); SingleHeader(writer, 0);
        context.Work(4L * ((image.Width + 511) / 512) * ((image.Height + 511) / 512));
    }

    private static void SingleHeader(WebpBitWriter writer, int symbol)
    {
        writer.Write(1, 1); writer.Write(0, 1); writer.Write(symbol > 1 ? 1 : 0, 1);
        writer.Write(symbol, symbol > 1 ? 8 : 1);
    }
}
