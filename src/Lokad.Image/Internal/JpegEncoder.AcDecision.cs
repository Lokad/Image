namespace Lokad.Image.Internal;

internal static partial class JpegEncoder
{
    private static int AcDecisionLambda(int quality, int components, JpegSampling sampling, JpegQuantizationPolicy policy)
    {
        if (components != 3 || sampling is not (JpegSampling.YCbCr444 or JpegSampling.YCbCr422 or JpegSampling.YCbCr420)
            || policy != JpegQuantizationPolicy.Original) return 0;
        var scale = quality < 50 ? 5000 / quality : 200 - 2 * quality;
        var precision = (16 * scale + 50) / 100;
        return precision * precision;
    }

    private static int AcDecisionWork(int quality, int components, JpegSampling sampling, JpegQuantizationPolicy policy)
        => AcDecisionLambda(quality, components, sampling, policy) == 0 ? 0
            : sampling == JpegSampling.YCbCr444 ? 48512 + (Trellis444WorkPerMcu + ComponentPairWorkPerMcu) / 3
            : sampling == JpegSampling.YCbCr420 ? 4096 + Trellis420WorkPerBlock : 4096;

    private static void TransformEncoderBlock(ReadOnlySpan<byte> pixels, ReadOnlySpan<ushort> quantization,
        Span<int> quantized, Span<double> transformed, int lambda, int samplingArea, OperationContext context,
        bool limitBlockError, bool refine420 = false, bool discardAc = false)
    {
        if (lambda == 0 && JpegTransform.TryForwardQuantized(pixels, quantization, quantized, context)) return;
        JpegTransform.Forward(pixels, transformed, context);
        Quantize(transformed, quantization, quantized);
        if (discardAc)
        {
            // Entirely invisible interleaved luma blocks still carry DC for
            // prediction, but their AC values cannot contribute output pixels.
            context.Work(64); quantized[1..].Clear(); return;
        }
        if (lambda != 0) DecideAc(transformed, quantization, quantized, lambda, samplingArea, context, limitBlockError);
        if (refine420) Refine420(transformed, quantization, quantized, lambda, samplingArea, context);
    }

    // T.81 permits encoder coefficient decisions independently of DQT. Keep the
    // original quantizers, initial nearest-away rounding and DC. One reverse scan
    // considers zero for each nonzero AC coefficient. The optional component cap
    // remains an uncoupled reference; production Original444 uses DecideJoint444.
    // The existing immutable
    // geometric AC model makes decisions independent of stream/entropy/admission.
    // No image search, reference-derived constants, frame cache or new heap storage.
    internal static void DecideAc(ReadOnlySpan<double> transformed, ReadOnlySpan<ushort> quantization,
        Span<int> quantized, int lambda, int samplingArea, OperationContext context, bool limitBlockError = false)
    {
        if (transformed.Length != 64 || quantization.Length != 64 || quantized.Length != 64
            || lambda < 0 || samplingArea is < 1 or > 4)
            throw new ArgumentException("JPEG AC decisions require complete blocks and valid weights.");
        if (lambda == 0) return;
        var maximumError = double.PositiveInfinity; var additionalError = 0d;
        if (limitBlockError)
        {
            context.Work(64);
            var originalError = 0d;
            for (var scan = 0; scan < 64; scan++)
            {
                var index = JpegTransform.Zigzag[scan];
                var residual = transformed[index] - (double)quantized[index] * quantization[index];
                originalError += residual * residual;
            }
            maximumError = originalError * samplingArea / 32;
        }
        var lengths = FixedAc.Table.Lengths;
        context.Work(64);
        var previousBits = AcDecisionBits(quantized, lengths);
        for (var scan = 63; scan > 0; scan--)
        {
            var index = JpegTransform.Zigzag[scan]; var value = quantized[index];
            if (value == 0) continue;
            context.Work(64);
            quantized[index] = 0;
            var nextBits = AcDecisionBits(quantized, lengths);
            var source = transformed[index]; var residual = source - (double)value * quantization[index];
            var error = samplingArea * (source * source - residual * residual);
            if (error < (long)lambda * (previousBits - nextBits)
                && (!limitBlockError || additionalError + error <= maximumError))
            {
                previousBits = nextBits;
                if (limitBlockError) additionalError += error;
            }
            else quantized[index] = value; // Stable old coefficient on ties.
        }
    }

    private static int AcDecisionBits(ReadOnlySpan<int> coefficients, ReadOnlySpan<byte> lengths)
    {
        var zeros = 0; var bits = 0;
        for (var scan = 1; scan < 64; scan++)
        {
            var value = coefficients[JpegTransform.Zigzag[scan]];
            if (value == 0) { zeros++; continue; }
            while (zeros >= 16) { bits += lengths[0xF0]; zeros -= 16; }
            var size = Category(value); bits += lengths[zeros << 4 | size] + size; zeros = 0;
        }
        if (zeros != 0) bits += lengths[0];
        return bits;
    }
}
