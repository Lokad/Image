namespace Lokad.Image.Internal;

internal static partial class Vp8Encoder
{
    // Trial only the existing four chroma predictions. Reconstruct both planes
    // before scoring; retain one 8x8 U/V result and its selected coefficients.
    private static int SelectRateChroma(Vp8Planes source, Vp8Planes reconstructed, int x, int y, int q,
        ReadOnlySpan<byte> above, ReadOnlySpan<byte> left, ReadOnlySpan<byte> probabilities,
        ReadOnlySpan<int> bitCosts, int lambda, Span<short> selected, OperationContext context)
    {
        var stride = source.Stride / 2; var dc = Math.Min(132, (int)Vp8Tables.DcQuantizers[q]);
        var ac = (int)Vp8Tables.AcQuantizers[q]; var best = 0; var bestScore = long.MaxValue;
        Span<short> trial = stackalloc short[128]; Span<short> residual = stackalloc short[16];
        Span<short> inverse = stackalloc short[16]; Span<byte> bestPixels = stackalloc byte[128];
        Span<byte> top = stackalloc byte[4]; Span<byte> side = stackalloc byte[4];
        for (var mode = 0; mode < 4; mode++)
        {
            context.Work(512);
            Vp8Prediction.Large(reconstructed.U, stride, x, y, 8, mode);
            Vp8Prediction.Large(reconstructed.V, stride, x, y, 8, mode);
            above.Slice(4, 4).CopyTo(top); left.Slice(4, 4).CopyTo(side);
            var rate = bitCosts[mode == 0 ? 142 : 114];
            if (mode != 0) rate += bitCosts[mode == 1 ? 114 : 142];
            if (mode >= 2) rate += bitCosts[mode == 2 ? 183 : 73];
            for (var plane = 0; plane < 2; plane++)
            {
                var src = plane == 0 ? source.U : source.V;
                var dst = plane == 0 ? reconstructed.U : reconstructed.V;
                for (var b = 0; b < 4; b++)
                {
                    context.Work(768); var bx = x + b % 2 * 4; var by = y + b / 2 * 4;
                    for (var row = 0; row < 4; row++)
                    for (var column = 0; column < 4; column++)
                    {
                        var at = (by + row) * stride + bx + column;
                        residual[row * 4 + column] = (short)(src[at] - dst[at]);
                    }
                    var values = trial.Slice((plane * 4 + b) * 16, 16);
                    Vp8ForwardDct.Quantize(residual, values, dc, ac);
                    var a = plane * 2 + b % 2; var l = plane * 2 + b / 2;
                    rate += CoefficientRate(values, 2, top[a] + side[l], probabilities, bitCosts, out var nonzero);
                    top[a] = side[l] = nonzero;
                    for (var i = 0; i < 16; i++) inverse[i] = checked((short)(values[i] * (i == 0 ? dc : ac)));
                    Vp8Transforms.InverseDct(inverse);
                    for (var row = 0; row < 4; row++)
                    for (var column = 0; column < 4; column++)
                    {
                        var at = (by + row) * stride + bx + column;
                        dst[at] = (byte)Math.Clamp(dst[at] + inverse[row * 4 + column], 0, 255);
                    }
                }
            }
            var distortion = Error(source.U, reconstructed.U, stride, x, y, 8)
                + Error(source.V, reconstructed.V, stride, x, y, 8);
            var score = distortion * 65536 + (long)rate * lambda;
            if (score >= bestScore) continue;
            best = mode; bestScore = score; trial.CopyTo(selected);
            for (var row = 0; row < 8; row++)
            {
                reconstructed.U.AsSpan((y + row) * stride + x, 8).CopyTo(bestPixels.Slice(row * 8, 8));
                reconstructed.V.AsSpan((y + row) * stride + x, 8).CopyTo(bestPixels.Slice(64 + row * 8, 8));
            }
        }
        for (var row = 0; row < 8; row++)
        {
            bestPixels.Slice(row * 8, 8).CopyTo(reconstructed.U.AsSpan((y + row) * stride + x, 8));
            bestPixels.Slice(64 + row * 8, 8).CopyTo(reconstructed.V.AsSpan((y + row) * stride + x, 8));
        }
        return best;
    }
}
