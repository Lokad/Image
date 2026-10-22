namespace Lokad.Image.Internal;

internal static partial class JpegEncoder
{
    // Three co-sited blocks require 2304 stack bytes within the fixed 8 KiB
    // workspace. All source sampling/flattening remains at the original boundary.
    private static bool PrepareJoint444Block(ReadOnlySpan<byte> pixels, ReadOnlySpan<ushort> quantization,
        Span<double> source, Span<int> values, int component, ReadOnlySpan<ushort> luma,
        ReadOnlySpan<ushort> chroma, int lambda, OperationContext context, JointAcPrices prices = default)
    {
        var block = source.Slice(component * 64, 64);
        JpegTransform.Forward(pixels, block, context);
        Quantize(block, quantization, values.Slice(component * 64, 64));
        if (component != 2) return false;
        if (prices.IsEmpty) DecideJoint444(source, luma, chroma, values, lambda, context);
        else DecidePrecisionGrid444(source, luma, chroma, values, prices, lambda, context);
        return true;
    }

    // T.81 amplitude categories supply the closest smaller-category magnitude.
    // RGB error uses the library's inverse BT.601 before clipping/rounding; actual
    // matched-reconstruction quality is qualified separately. DC/DQT are retained.
    internal static void DecideJoint444(ReadOnlySpan<double> source, ReadOnlySpan<ushort> luma,
        ReadOnlySpan<ushort> chroma, Span<int> values, int lambda, OperationContext context, JointAcPrices prices = default,
        double? errorLimit = null, ReadOnlySpan<ushort> amplitudeLimits = default, JointAcBands bands = default)
    {
        if (source.Length != 192 || values.Length != 192 || luma.Length != 64 || chroma.Length != 64 || lambda < 0
            || (!prices.IsEmpty && prices.Families != 2) || amplitudeLimits.Length is not (0 or 128)
            || errorLimit.HasValue && (!double.IsFinite(errorLimit.Value) || errorLimit.Value < 0))
            throw new ArgumentException("Joint JPEG decisions require three complete co-sited blocks.");
        if (lambda == 0) return;
        context.Work(192);
        var initialError = 0d;
        for (var scan = 0; scan < 64; scan++)
        {
            var index = JpegTransform.Zigzag[scan];
            initialError += JointRgbError(source[index] - (double)values[index] * luma[index],
                source[64 + index] - (double)values[64 + index] * chroma[index],
                source[128 + index] - (double)values[128 + index] * chroma[index]);
        }
        var maximumError = initialError / 32; var additionalError = 0d; var currentError = initialError;
        var lengths = FixedAc.Table.Lengths;
        context.Work(192);
        Span<int> previousBits = stackalloc int[3];
        Span<int> trialLevels = stackalloc int[12];
        for (var component = 0; component < 3; component++)
            previousBits[component] = prices.IsEmpty ? AcDecisionBits(values.Slice(component * 64, 64), lengths)
                : JointAcPrice(values.Slice(component * 64, 64), prices.ForComponent(component), bands.ForComponent(component));
        for (var scan = 63; scan > 0; scan--)
        {
            var index = JpegTransform.Zigzag[scan];
            for (var component = 0; component < 3; component++)
            {
                var at = component * 64 + index; var original = values[at];
                if (original == 0 && prices.IsEmpty) continue;
                var ey = source[index] - (double)values[index] * luma[index];
                var ecb = source[64 + index] - (double)values[64 + index] * chroma[index];
                var ecr = source[128 + index] - (double)values[128 + index] * chroma[index];
                var before = JointRgbError(ey, ecb, ecr);
                var bestValue = original; var bestBits = previousBits[component];
                var bestError = 0d; var bestCost = 0d;
                var trialCount = 0;
                for (var category = Category(original) - 1; category >= 0; category--)
                {
                    var magnitude = (1 << category) - 1;
                    trialLevels[trialCount++] = original > 0 ? magnitude : -magnitude;
                }
                // Preserve the geometric source trainer. Only the final priced
                // pass adds adjacent levels, after the existing stable choices.
                if (!prices.IsEmpty)
                {
                    for (var direction = -1; direction <= 1; direction += 2)
                    {
                        var value = original + direction;
                        var limit = amplitudeLimits.IsEmpty ? 1023 : amplitudeLimits[(component == 0 ? 0 : 64) + index];
                        if (value < -limit || value > limit || trialLevels[..trialCount].Contains(value)) continue;
                        trialLevels[trialCount++] = value;
                    }
                }
                try
                {
                    for (var trial = 0; trial < trialCount; trial++)
                    {
                        context.Work(64);
                        var value = trialLevels[trial];
                        values[at] = value;
                        var nextBits = prices.IsEmpty ? AcDecisionBits(values.Slice(component * 64, 64), lengths)
                            : JointAcPrice(values.Slice(component * 64, 64), prices.ForComponent(component), bands.ForComponent(component));
                        var residual = source[at] - (double)value * (component == 0 ? luma[index] : chroma[index]);
                        var error = JointRgbError(component == 0 ? residual : ey,
                            component == 1 ? residual : ecb, component == 2 ? residual : ecr) - before;
                        var cost = error + (long)lambda * (nextBits - previousBits[component]) / (prices.IsEmpty ? 1d : 256d);
                        if ((errorLimit.HasValue ? currentError + error <= errorLimit.Value : additionalError + error <= maximumError)
                            && cost < bestCost)
                        { bestValue = value; bestBits = nextBits; bestError = error; bestCost = cost; }
                    }
                }
                finally { values[at] = bestValue; }
                previousBits[component] = bestBits; additionalError += bestError; currentError += bestError;
            }
        }
    }

    private static double JointRgbError(double y, double cb, double cr)
    {
        var red = y + 1.402 * cr; var green = (y - 0.344136286 * cb) - 0.714136286 * cr;
        var blue = y + 1.772 * cb;
        return ((red * red + green * green) + blue * blue) / 3;
    }

    private static int AppendJoint444Events(ReadOnlySpan<int> values, int[] predictors,
        long[][] frequencies, uint[] cache, int eventCount, OperationContext context)
    {
        for (var component = 0; component < 3; component++)
        {
            context.Work(64); var block = values.Slice(component * 64, 64); var group = component == 0 ? 0 : 1;
            var difference = block[0] - predictors[component]; predictors[component] = block[0];
            var category = Category(difference);
            if (category > 11) throw new ImageDataException("JPEG DC transform exceeds its coding range.");
            frequencies[group][category]++;
            cache[eventCount++] = PackEntropyEvent(group, category, difference, category);
            eventCount = AppendAdaptiveAc(block, frequencies[group + 2], cache, eventCount, group + 2);
        }
        return eventCount;
    }
}
