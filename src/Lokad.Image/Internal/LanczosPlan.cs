namespace Lokad.Image.Internal;

// Independently derived radius-three windowed sinc. Exact rational sample
// locations; coefficients quantize the cumulative normalized weights to Q30.
// This distributes quantization error rather than adding a residual to one tap.
internal sealed class LanczosPlan : IDisposable
{
    internal const long One = 1L << 30;
    internal readonly record struct Window(int First, int Count, int Offset);
    internal Window[] Windows = [];
    internal int[] Weights = [];
    internal int MaximumCount;
    private readonly OperationContext context;
    private long storage;
    private bool disposed;
    private LanczosPlan(OperationContext context) { this.context = context; context.Reserve(128); storage = 128; }

    internal static LanczosPlan Create(int source, int target, OperationContext context)
    {
        var plan = new LanczosPlan(context); long temporary = 0;
        try
        {
            var windowsBytes = 32 + target * 16L;
            Buffer(windowsBytes); context.Reserve(windowsBytes); plan.storage += windowsBytes;
            plan.Windows = new Window[target]; long total = 0, kernelWork = 0;
            for (var x = 0; x < target; x++)
            {
                context.Work(1); var (first, last) = Support(x, source, target);
                var begin = (int)Math.Clamp(first, 0, source - 1L); var finish = (int)Math.Clamp(last, 0, source - 1L);
                var count = finish - begin + 1;
                if (total > (Array.MaxLength - 32) / 4L - count) throw Length();
                plan.Windows[x] = new(begin, count, (int)total); total += count;
                plan.MaximumCount = Math.Max(plan.MaximumCount, count);
                kernelWork += (last - first + 1) * 2 + count * 2L;
                if (kernelWork > context.RemainingWork) throw new ImageLimitException(ImageLimitKind.Work, "Lanczos coefficient work limit exceeded.");
            }
            var weightsBytes = 32 + total * 4; context.Reserve(weightsBytes); plan.storage += weightsBytes;
            plan.Weights = new int[(int)total];
            var scratch = 32 + plan.MaximumCount * 8L; Buffer(scratch); context.Reserve(scratch); temporary = scratch;
            var raw = new double[plan.MaximumCount];
            for (var x = 0; x < target; x++)
            {
                var window = plan.Windows[x]; var (first, last) = Support(x, source, target);
                context.Work((last - first + 1) * 2 + window.Count * 2L);
                raw.AsSpan(0, window.Count).Clear();
                var numerator = (2L * x + 1) * source - target;
                var denominator = 2d * Math.Max(source, target);
                for (var sample = first; sample <= last; sample++)
                {
                    if (((sample - first) & 1023) == 0) context.Cancellation.ThrowIfCancellationRequested();
                    var distance = (2L * target * sample - numerator) / denominator;
                    raw[(int)Math.Clamp(sample, 0, source - 1L) - window.First] += Kernel(distance);
                }
                double normalizer = 0;
                for (var i = 0; i < window.Count; i++)
                { if ((i & 1023) == 0) context.Cancellation.ThrowIfCancellationRequested(); normalizer += raw[i]; }
                if (!(normalizer > 0) || !double.IsFinite(normalizer)) throw new InvalidOperationException("Invalid Lanczos normalization.");
                double cumulative = 0; long previous = 0, magnitude = 0;
                for (var i = 0; i < window.Count; i++)
                {
                    if ((i & 1023) == 0) context.Cancellation.ThrowIfCancellationRequested();
                    cumulative += raw[i] / normalizer;
                    var next = i + 1 == window.Count ? One : (long)Math.Round(cumulative * One, MidpointRounding.AwayFromZero);
                    var weight = checked((int)(next - previous)); previous = next;
                    plan.Weights[window.Offset + i] = weight; magnitude += Math.Abs((long)weight);
                }
                // Lanczos3 has bounded total variation. This also certifies that
                // every horizontal RGBA16 premultiplied sum fits signed Int64:
                // 65535^2 * (2 * 2^30) < Int64.MaxValue.
                if (magnitude > 2 * One) throw new InvalidOperationException("Lanczos coefficient arithmetic bound exceeded.");
            }
            return plan;
        }
        catch { plan.Dispose(); throw; }
        finally { if (temporary != 0) context.Release(temporary); }
    }

    internal static (long First, long Last) Support(int destination, int source, int target)
    {
        var center = (2L * destination + 1) * source - target;
        var radius = 6L * Math.Max(source, target); var divisor = 2L * target;
        return (-Floor(-(center - radius), divisor), Floor(center + radius, divisor));
    }
    private static long Floor(long value, long divisor)
    {
        var quotient = value / divisor; return value % divisor < 0 ? quotient - 1 : quotient;
    }
    internal static double Kernel(double distance)
    {
        var x = Math.Abs(distance);
        if (x >= 3 || x is 1 or 2) return 0;
        if (x == 0) return 1;
        var a = Math.PI * x; var b = a / 3;
        return Math.Sin(a) / a * (Math.Sin(b) / b);
    }
    private static void Buffer(long bytes) { if (bytes > Array.MaxLength) throw Length(); }
    private static ImageLimitException Length() => new(ImageLimitKind.BufferLength, "Lanczos coefficient storage exceeds the managed limit.");
    public void Dispose()
    {
        if (disposed) return; disposed = true; Windows = []; Weights = [];
        context.Release(storage); storage = 0;
    }
}
