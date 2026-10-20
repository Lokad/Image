namespace Lokad.Image.Internal;

internal static partial class Vp8Encoder
{
    // Reconstruct every admitted prediction, then compare actual
    // sample distortion plus context-dependent coefficient/mode bit cost.
    // All scratch is bounded stack storage within the existing 4 KiB allowance.
    private static int SelectRateLuma(Vp8Planes source, Vp8Planes reconstructed, int x, int y, int macroX, int macroY,
        int candidates, int q, ReadOnlySpan<byte> modeProbabilities, ReadOnlySpan<byte> coefficientProbabilities,
        int predictor, ReadOnlySpan<int> bitCosts, int lambda, Span<short> selected,
        OperationContext context, out bool ready)
    {
        ready = false; var first = 0; var firstError = long.MaxValue;
        Span<int> prepared = stackalloc int[13];
        var dcPrediction = Vp8Prediction.PrepareSmall(reconstructed.Y, source.Stride, x, y, macroX, macroY, prepared);
        for (var mode = 0; mode < candidates; mode++)
        {
            context.Work(192);
            Vp8Prediction.SmallPrepared(reconstructed.Y, source.Stride, x, y, mode, prepared, dcPrediction);
            var error = Error(source.Y, reconstructed.Y, source.Stride, x, y, 4);
            if (error == 0) return mode;
            if (error < firstError) { first = mode; firstError = error; }
        }
        Span<short> residual = stackalloc short[16]; Span<short> coefficients = stackalloc short[16];
        Span<short> inverse = stackalloc short[16]; Span<byte> bestPixels = stackalloc byte[16];
        var dc = (int)Vp8Tables.DcQuantizers[q]; var ac = (int)Vp8Tables.AcQuantizers[q];
        var best = first; var bestScore = long.MaxValue;
        for (var trial = 0; trial < candidates; trial++)
        {
            context.Work(768); var mode = trial == 0 ? first : trial - 1 < first ? trial - 1 : trial;
            Vp8Prediction.SmallPrepared(reconstructed.Y, source.Stride, x, y, mode, prepared, dcPrediction);
            for (var row = 0; row < 4; row++)
            for (var column = 0; column < 4; column++)
            {
                var at = (y + row) * source.Stride + x + column;
                residual[row * 4 + column] = (short)(source.Y[at] - reconstructed.Y[at]);
            }
            Vp8ForwardDct.Quantize(residual, coefficients, dc, ac);
            for (var i = 0; i < 16; i++) inverse[i] = checked((short)(coefficients[i] * (i == 0 ? dc : ac)));
            Vp8Transforms.InverseDct(inverse);
            long distortion = 0;
            for (var row = 0; row < 4; row++)
            for (var column = 0; column < 4; column++)
            {
                var at = (y + row) * source.Stride + x + column;
                var pixel = Math.Clamp(reconstructed.Y[at] + inverse[row * 4 + column], 0, 255);
                var error = source.Y[at] - pixel; distortion += error * error;
            }
            var rate = CoefficientRate(coefficients, 3, predictor, coefficientProbabilities, bitCosts, out _)
                + PathRate(ModePaths[mode], modeProbabilities, bitCosts);
            var score = distortion * 65536 + (long)rate * lambda;
            if (score >= bestScore) continue;
            bestScore = score; best = mode; coefficients.CopyTo(selected);
            for (var row = 0; row < 4; row++)
            for (var column = 0; column < 4; column++)
            {
                var at = (y + row) * source.Stride + x + column;
                bestPixels[row * 4 + column] = (byte)Math.Clamp(reconstructed.Y[at] + inverse[row * 4 + column], 0, 255);
            }
        }
        for (var row = 0; row < 4; row++) bestPixels.Slice(row * 4, 4).CopyTo(reconstructed.Y.AsSpan((y + row) * source.Stride + x, 4));
        ready = true; return best;
    }

}
