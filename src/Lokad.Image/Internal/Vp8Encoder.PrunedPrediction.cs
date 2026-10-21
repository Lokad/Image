namespace Lokad.Image.Internal;

internal static partial class Vp8Encoder
{
    // Nonnegative partial squared error is a lower bound on the full candidate.
    // Equal candidates keep the first mode. Full evaluation retains192 units:
    // prediction/preparation96 and four24-unit error rows. Only skipped error
    // rows end their work early; every prediction and actual row is charged.
    private static int SelectPrunedLuma(Vp8Planes source, Vp8Planes reconstructed, int x, int y,
        int macroX, int macroY, int candidates, OperationContext context)
    {
        var best = 0; var score = long.MaxValue;
        Span<int> prepared = stackalloc int[13]; var dc = 0;
        for (var mode = 0; mode < candidates; mode++)
        {
            context.Work(96);
            if (mode == 0) dc = Vp8Prediction.PrepareSmall(reconstructed.Y, source.Stride, x, y, macroX, macroY, prepared);
            Vp8Prediction.SmallPrepared(reconstructed.Y, source.Stride, x, y, mode, prepared, dc);
            long error = 0; var row = 0;
            for (; row < 4; row++)
            {
                context.Work(24);
                for (var column = 0; column < 4; column++)
                {
                    var at = (y + row) * source.Stride + x + column;
                    var difference = source.Y[at] - reconstructed.Y[at]; error += difference * difference;
                }
                if (error >= score) break;
            }
            if (row != 4) continue;
            if (error == 0) return mode;
            best = mode; score = error;
        }
        Vp8Prediction.SmallPrepared(reconstructed.Y, source.Stride, x, y, best, prepared, dc);
        return best;
    }
}
