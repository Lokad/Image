namespace Lokad.Image.Internal;

internal static partial class Vp8Encoder
{
    private const int NearZeroTableWork = 768 * 16 + 128;
    private const int NearZeroExtraStackBytes = 4096;

    // Compare each small nonzero token with the immediately smaller level. The
    // EOB prefix cancels; signs also cancel except in the zero/one comparison.
    // Existing transform-energy and rate scales give the normalized threshold
    // m-.5 + rateDifference*lambda/(32768*step^2). This local decision ignores the
    // rate of later coefficients; it is not a whole-block distortion bound.
    private static void PrepareZeroThresholds(ReadOnlySpan<byte> quantizers, ReadOnlySpan<byte> probabilities,
        ReadOnlySpan<int> costs, Span<double> thresholds, OperationContext context)
    {
        context.Work(NearZeroTableWork);
        for (var segment = 0; segment < 4; segment++)
        {
            var q = quantizers[segment]; var ac = (int)Vp8Tables.AcQuantizers[q];
            var lambda = (int)Math.Round(ac * ac * Math.Log(2) * 256 / 24, MidpointRounding.AwayFromZero);
            for (var channel = 0; channel < 2; channel++)
            for (var band = 0; band < 8; band++)
            for (var state = 0; state < 3; state++)
            {
                var plane = channel == 0 ? 3 : 2;
                var p = probabilities.Slice(((plane * 8 + band) * 3 + state) * 11, 11);
                var step = band == 0 ? (int)Vp8Tables.DcQuantizers[q] : ac;
                if (channel == 1 && band == 0) step = Math.Min(132, step);
                var offset = segment * 192 + channel * 96 + band * 12 + state * 4;
                for (var magnitude = 1; magnitude <= 4; magnitude++)
                {
                    var delta = magnitude switch
                    {
                        1 => costs[256 - p[1]] + costs[p[2]] + 256 - costs[p[1]],
                        2 => costs[256 - p[2]] + costs[p[3]] + costs[p[4]] - costs[p[2]],
                        3 => costs[256 - p[4]] + costs[p[5]] - costs[p[4]],
                        _ => costs[256 - p[5]] - costs[p[5]]
                    };
                    thresholds[offset + magnitude - 1] = Math.Max(magnitude - .5,
                        magnitude - .5 + (double)delta * lambda / (32768d * step * step));
                }
            }
        }
    }

    private static void QuantizeNearZero(ReadOnlySpan<short> residual, Span<short> coefficients,
        int dc, int ac, int predictor, ReadOnlySpan<double> thresholds, OperationContext context)
    {
        // One transform replaces ordinary quantization; there is no second DCT.
        Span<double> raw = stackalloc double[16]; Vp8ForwardDct.Transform(residual, raw);
        for (var c = 0; c < 16; c++)
        {
            var z = Zigzag[c]; var scaled = raw[z] / (z == 0 ? dc : ac);
            var level = checked((short)Math.Round(scaled, MidpointRounding.AwayFromZero));
            var magnitude = Math.Abs((int)level);
            if (magnitude is >= 1 and <= 4)
            {
                context.Work(1);
                if (Math.Abs(scaled) < thresholds[Bands[c] * 12 + predictor * 4 + magnitude - 1])
                    level = (short)(level - Math.Sign(level));
            }
            coefficients[z] = level; predictor = Math.Min(Math.Abs((int)level), 2);
        }
    }
}
