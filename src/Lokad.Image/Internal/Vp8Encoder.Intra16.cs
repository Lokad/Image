namespace Lokad.Image.Internal;

internal static partial class Vp8Encoder
{
    private static int LargeEquivalent(int mode) => mode switch { 0 => 0, 1 => 2, 2 => 3, _ => 1 };

    private static int SmallMode(byte[] modes, int macroblock, int subblock)
        => (modes[macroblock * 16] & 128) != 0 ? LargeEquivalent(modes[macroblock * 16] & 3) : modes[macroblock * 16 + subblock];

    // Compare the established intra4 reconstruction with the best source-error
    // intra16 prediction. Stage only one macroblock; converted source samples
    // are cached only after its final mode and coefficients have been selected.
    private static bool PrepareLumaMacroblock(Vp8Planes source, Vp8Planes reconstructed, int x, int y,
        int q, int candidates, int index, int columns, byte[] modes, ReadOnlySpan<byte> above, ReadOnlySpan<byte> left,
        int y2Context, ReadOnlySpan<byte> probabilities, ReadOnlySpan<int> bitCosts, int lambda,
        Span<short> selected, Span<short> selectedY2, OperationContext context)
    {
        var bestLarge = 0; var bestError = long.MaxValue;
        for (var mode = 0; mode < 4; mode++)
        {
            context.Work(3072); Vp8Prediction.Large(reconstructed.Y, source.Stride, x, y, 16, mode);
            var error = Error(source.Y, reconstructed.Y, source.Stride, x, y, 16);
            if (error == 0)
            {
                selected.Clear(); selectedY2.Clear(); modes[index * 16] = (byte)(128 | mode);
                return true;
            }
            if (error < bestError) { bestError = error; bestLarge = mode; }
        }

        var dc = (int)Vp8Tables.DcQuantizers[q]; var ac = (int)Vp8Tables.AcQuantizers[q];
        Span<short> residual = stackalloc short[16]; Span<short> inverse = stackalloc short[16];
        Span<byte> rateTop = stackalloc byte[4]; Span<byte> rateSide = stackalloc byte[4];
        Span<byte> modeTop = stackalloc byte[4]; Span<byte> modeSide = stackalloc byte[4];
        above[..4].CopyTo(rateTop); left[..4].CopyTo(rateSide);
        for (var i = 0; i < 4; i++)
        {
            modeTop[i] = (byte)(index < columns ? 0 : SmallMode(modes, index - columns, 12 + i));
            modeSide[i] = (byte)(index % columns == 0 ? 0 : SmallMode(modes, index - 1, i * 4 + 3));
        }
        for (var b = 0; b < 16; b++)
        {
            var a = b % 4; var l = b / 4; var bx = x + a * 4; var by = y + l * 4;
            var values = selected.Slice(b * 16, 16);
            var mode = SelectRateLuma(source, reconstructed, bx, by, x, y, candidates, q,
                Vp8Tables.BlockModeProbabilities.Slice((modeTop[a] * 10 + modeSide[l]) * 9, 9),
                probabilities, rateTop[a] + rateSide[l], bitCosts, lambda, values, context, out var ready);
            modes[index * 16 + b] = modeTop[a] = modeSide[l] = (byte)mode;
            if (!ready)
            {
                context.Work(512);
                for (var row = 0; row < 4; row++)
                for (var column = 0; column < 4; column++)
                {
                    var at = (by + row) * source.Stride + bx + column;
                    residual[row * 4 + column] = (short)(source.Y[at] - reconstructed.Y[at]);
                }
                Vp8ForwardDct.Quantize(residual, values, dc, ac);
                ReconstructLumaBlock(reconstructed, bx, by, values, values[0] * dc, ac, inverse);
            }
            rateTop[a] = rateSide[l] = (byte)(values.IndexOfAnyExcept((short)0) < 0 ? 0 : 1);
        }
        context.Work(1024);
        var smallRate = LumaRate(selected, 3, above, left, probabilities, bitCosts)
            + SmallHeaderRate(modes, index, columns, bitCosts);
        var smallDistortion = Error(source.Y, reconstructed.Y, source.Stride, x, y, 16);
        var smallScore = smallDistortion * 65536 + (long)smallRate * lambda;
        Span<byte> smallPixels = stackalloc byte[256];
        for (var row = 0; row < 16; row++) reconstructed.Y.AsSpan((y + row) * source.Stride + x, 16).CopyTo(smallPixels.Slice(row * 16, 16));

        Vp8Prediction.Large(reconstructed.Y, source.Stride, x, y, 16, bestLarge);
        Span<short> trial = stackalloc short[256]; Span<int> dcSums = stackalloc int[16];
        Span<short> trialY2 = stackalloc short[16]; Span<short> secondDc = stackalloc short[16];
        for (var b = 0; b < 16; b++)
        {
            context.Work(512); var bx = x + b % 4 * 4; var by = y + b / 4 * 4; var sum = 0;
            for (var row = 0; row < 4; row++)
            for (var column = 0; column < 4; column++)
            {
                var at = (by + row) * source.Stride + bx + column;
                var value = source.Y[at] - reconstructed.Y[at]; residual[row * 4 + column] = (short)value; sum += value;
            }
            var values = trial.Slice(b * 16, 16);
            Vp8ForwardDct.Quantize(residual, values, dc, ac); values[0] = 0; dcSums[b] = sum;
        }
        context.Work(256);
        var dc2 = dc * 2; var ac2 = Math.Max(8, ac * 155 / 100);
        Vp8ForwardDct.QuantizeWalsh(dcSums, trialY2, dc2, ac2);
        for (var i = 0; i < 16; i++) secondDc[i] = checked((short)(trialY2[i] * (i == 0 ? dc2 : ac2)));
        Vp8Transforms.InverseWalsh(secondDc);
        for (var b = 0; b < 16; b++)
            ReconstructLumaBlock(reconstructed, x + b % 4 * 4, y + b / 4 * 4, trial.Slice(b * 16, 16), secondDc[b], ac, inverse);
        context.Work(1024);
        var largeRate = LumaRate(trial, 0, above, left, probabilities, bitCosts)
            + CoefficientRate(trialY2, 1, y2Context, probabilities, bitCosts, out _)
            + bitCosts[111] + bitCosts[bestLarge < 2 ? 156 : 100]
            + bitCosts[(bestLarge & 1) == 0 ? bestLarge < 2 ? 163 : 128 : bestLarge < 2 ? 93 : 128];
        var largeScore = Error(source.Y, reconstructed.Y, source.Stride, x, y, 16) * 65536 + (long)largeRate * lambda;
        if (largeScore < smallScore)
        {
            trial.CopyTo(selected); trialY2.CopyTo(selectedY2); modes[index * 16] = (byte)(128 | bestLarge); return true;
        }
        for (var row = 0; row < 16; row++) smallPixels.Slice(row * 16, 16).CopyTo(reconstructed.Y.AsSpan((y + row) * source.Stride + x, 16));
        return false;
    }

    private static void ReconstructLumaBlock(Vp8Planes reconstructed, int x, int y, ReadOnlySpan<short> values,
        int dc, int ac, Span<short> inverse)
    {
        inverse[0] = checked((short)dc);
        for (var i = 1; i < 16; i++) inverse[i] = checked((short)(values[i] * ac));
        Vp8Transforms.InverseDct(inverse);
        for (var row = 0; row < 4; row++)
        for (var column = 0; column < 4; column++)
        {
            var at = (y + row) * reconstructed.Stride + x + column;
            reconstructed.Y[at] = (byte)Math.Clamp(reconstructed.Y[at] + inverse[row * 4 + column], 0, 255);
        }
    }

    private static int LumaRate(ReadOnlySpan<short> values, int plane, ReadOnlySpan<byte> above, ReadOnlySpan<byte> left,
        ReadOnlySpan<byte> probabilities, ReadOnlySpan<int> costs)
    {
        Span<byte> top = stackalloc byte[4]; Span<byte> side = stackalloc byte[4]; above[..4].CopyTo(top); left[..4].CopyTo(side);
        var result = 0;
        for (var b = 0; b < 16; b++)
        {
            result += CoefficientRate(values.Slice(b * 16, 16), plane, top[b % 4] + side[b / 4], probabilities, costs, out var nonzero);
            top[b % 4] = side[b / 4] = nonzero;
        }
        return result;
    }

    private static int SmallHeaderRate(byte[] modes, int index, int columns, ReadOnlySpan<int> costs)
    {
        Span<byte> above = stackalloc byte[4]; Span<byte> left = stackalloc byte[4];
        for (var i = 0; i < 4; i++)
        {
            above[i] = (byte)(index < columns ? 0 : SmallMode(modes, index - columns, 12 + i));
            left[i] = (byte)(index % columns == 0 ? 0 : SmallMode(modes, index - 1, i * 4 + 3));
        }
        var result = costs[145];
        for (var b = 0; b < 16; b++)
        {
            var mode = modes[index * 16 + b]; var at = (above[b % 4] * 10 + left[b / 4]) * 9;
            result += PathRate(ModePaths[mode], Vp8Tables.BlockModeProbabilities.Slice(at, 9), costs);
            above[b % 4] = left[b / 4] = mode;
        }
        return result;
    }

    private static int PathRate(ulong path, ReadOnlySpan<byte> probabilities, ReadOnlySpan<int> costs)
    {
        var result = 0;
        while (path != 1)
        {
            var step = (int)(path & 31); var p = probabilities[step >> 1]; result += costs[(step & 1) == 0 ? p : 256 - p]; path >>= 5;
        }
        return result;
    }

    private static int CoefficientRate(ReadOnlySpan<short> coefficients, int plane, int predictor,
        ReadOnlySpan<byte> probabilities, ReadOnlySpan<int> costs, out byte nonzero)
    {
        var first = plane == 0 ? 1 : 0; var last = 15;
        while (last >= first && coefficients[Zigzag[last]] == 0) last--;
        nonzero = last < first ? (byte)0 : (byte)1;
        var previousZero = false; var result = 0;
        for (var c = first; c <= Math.Min(Math.Max(last + 1, first), 15); c++)
        {
            var p = probabilities.Slice(((plane * 8 + Bands[c]) * 3 + predictor) * 11, 11);
            if (c > last) { result += PathRate(TokenPaths[11], p, costs); break; }
            var value = coefficients[Zigzag[c]]; var magnitude = Math.Abs((int)value);
            var category = magnitude < 5 ? -1 : magnitude < 7 ? 0 : magnitude < 11 ? 1 : magnitude < 19 ? 2
                : magnitude < 35 ? 3 : magnitude < 67 ? 4 : 5;
            var path = TokenPaths[category < 0 ? magnitude : category + 5];
            result += PathRate(previousZero ? path >> 5 : path, p, costs);
            if (category >= 0)
            {
                ReadOnlySpan<byte> categoryProbabilities = category switch
                {
                    0 => [159], 1 => [165, 145], 2 => [173, 148, 140], 3 => [176, 155, 140, 135],
                    4 => [180, 157, 141, 134, 130], _ => [254, 254, 243, 230, 196, 177, 153, 140, 133, 130, 129]
                };
                ReadOnlySpan<int> bases = [5, 7, 11, 19, 35, 67]; var extra = magnitude - bases[category];
                for (var bit = 0; bit < categoryProbabilities.Length; bit++)
                {
                    var probability = categoryProbabilities[bit];
                    result += costs[(extra >> (categoryProbabilities.Length - 1 - bit) & 1) == 0 ? probability : 256 - probability];
                }
            }
            if (magnitude != 0) result += 256;
            predictor = Math.Min(magnitude, 2); previousZero = magnitude == 0;
        }
        return result;
    }
}
