namespace Lokad.Image.Internal;

internal static partial class JpegEncoder
{
    // Apply the quality-derived precision before the one priced decision pass.
    // The Original DQT remains the encoded quantizer. Changing the decision grid
    // must not itself grant an additional fractional source-error allowance.
    internal static void DecidePrecisionGrid444(ReadOnlySpan<double> source, ReadOnlySpan<ushort> luma,
        ReadOnlySpan<ushort> chroma, Span<int> values, ReadOnlySpan<int> prices, int lambda, OperationContext context)
    {
        if (source.Length != 192 || values.Length != 192 || luma.Length != 64 || chroma.Length != 64
            || prices.Length != 512 || lambda < 0)
            throw new ArgumentException("JPEG precision decisions require three complete blocks and two price models.");
        if (lambda == 0) return;
        // Replace the former late precision pass's 2880 units, before mutation:
        // <=128*13 factor steps + 128 setup + 384 error visits + 192 quantize
        // and 192 rescale visits = 2560. No second DCT or full decision pass.
        context.Work(2880);
        var fineError = PrecisionGridError444(source, luma, chroma, values);
        var precision = (int)Math.Sqrt(lambda);
        // 512 stack bytes, alongside 4352 bytes of caller joint state and the
        // sequential 1612-byte DAG scratch, within the admitted 8 KiB workspace.
        Span<ushort> quantizers = stackalloc ushort[128];
        Span<ushort> limits = stackalloc ushort[128];
        for (var i = 0; i < 128; i++)
        {
            var q = i < 64 ? luma[i] : chroma[i - 64];
            var factor = 1 << PointTransform444(q, precision);
            quantizers[i] = checked((ushort)(q * factor)); limits[i] = (ushort)(1023 / factor);
        }
        for (var c = 0; c < 3; c++)
        for (var i = 0; i < 64; i++)
        {
            var offset = (c == 0 ? 0 : 64) + i;
            var q = quantizers[offset]; var factor = q / (c == 0 ? luma[i] : chroma[i]);
            var selected = (int)Math.Round(source[c * 64 + i] / q, MidpointRounding.AwayFromZero);
            values[c * 64 + i] = Math.Clamp(selected, i == 0 ? -1024 / factor : -limits[offset], limits[offset]);
        }
        var coarseError = PrecisionGridError444(source, quantizers[..64], quantizers[64..], values);
        var maximumError = PrecisionGridErrorLimit444(fineError, coarseError);
        DecideJoint444(source, quantizers[..64], quantizers[64..], values, lambda, context, prices, maximumError, limits);
        RefineJoint444(source, quantizers[..64], quantizers[64..], values, prices, lambda, context, maximumError, limits);
        var state = RefineComponentPairs444(source, quantizers[..64], quantizers[64..], values, prices, lambda, context, maximumError, limits);
        RefineComponentTriplets444(source, quantizers[..64], quantizers[64..], values, prices, lambda, context, maximumError, state, limits);
        for (var c = 0; c < 3; c++)
        for (var i = 0; i < 64; i++)
            values[c * 64 + i] *= quantizers[(c == 0 ? 0 : 64) + i] / (c == 0 ? luma[i] : chroma[i]);
    }

    internal static double PrecisionGridErrorLimit444(double fineError, double coarseError)
        => Math.Max(coarseError, fineError + fineError / 32);

    private static double PrecisionGridError444(ReadOnlySpan<double> source, ReadOnlySpan<ushort> luma,
        ReadOnlySpan<ushort> chroma, ReadOnlySpan<int> values)
    {
        var error = 0d;
        for (var scan = 0; scan < 64; scan++)
        {
            var i = JpegTransform.Zigzag[scan];
            error += JointRgbError(source[i] - (double)values[i] * luma[i],
                source[64 + i] - (double)values[64 + i] * chroma[i],
                source[128 + i] - (double)values[128 + i] * chroma[i]);
        }
        return error;
    }
}
