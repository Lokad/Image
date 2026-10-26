namespace Lokad.Image.Internal;

// Private source-analysis continuation. Segment geometry/quantizer steps come
// from RFC6386; centroid selection and quality modulation are encoder policies.
// Final reconstruction-dependent filter adjustment belongs to the later RDO.
internal sealed class Vp8SourceSegments : IDisposable
{
    private const int MetadataBytes = 128, ScratchBytes = 1024;
    internal readonly record struct Segment(int Center, int Alpha, int Beta, int Quantizer, int InitialFilter);
    internal readonly record struct Clustering(int Mean, int Iterations);
    private readonly OperationContext context;
    private byte[]? map;
    private Segment[]? parameters;
    private long retainedBytes;
    internal int Width { get; }
    internal int Height { get; }
    internal int SegmentCount { get; private set; }
    internal Clustering Statistics { get; private set; }
    internal long CapacityBytes => retainedBytes;
    internal ReadOnlySpan<byte> Map => map ?? throw new ObjectDisposedException(nameof(Vp8SourceSegments));
    internal ReadOnlySpan<Segment> Parameters => parameters ?? throw new ObjectDisposedException(nameof(Vp8SourceSegments));
    private Vp8SourceSegments(OperationContext context, int width, int height)
    { this.context = context; Width = width; Height = height; }

    internal static int FilterStrength(int quantizer, int beta)
    {
        var strength = (Vp8Tables.AcQuantizers[quantizer] >> 2) * 300 / (256 + beta);
        return strength < 2 ? 0 : Math.Min(63, strength);
    }

    // The histogram is a nonempty source distribution. Scratch/output are
    // borrowed; the enclosing owner admits their capacity once for the stage.
    internal static Clustering Cluster(ReadOnlySpan<int> histogram, Span<byte> assignment,
        Span<Segment> segments, OperationContext context)
    {
        var low = 0; var high = 255;
        context.Cancellation.ThrowIfCancellationRequested();
        while (low < 255 && histogram[low] == 0) low++;
        while (high > low && histogram[high] == 0) high--;
        var count = segments.Length;
        for (var k = 0; k < count; k++) segments[k] = new(low + (2 * k + 1) * (high - low) / (2 * count), 0, 0, 0, 0);
        Span<long> weights = stackalloc long[4]; Span<long> moments = stackalloc long[4];
        var mean = low; var iterations = 0;
        for (var iteration = 0; iteration < 6; iteration++)
        {
            context.Cancellation.ThrowIfCancellationRequested(); weights.Clear(); moments.Clear(); var nearest = 0;
            for (var value = low; value <= high; value++)
            {
                var frequency = histogram[value]; if (frequency == 0) continue;
                while (nearest + 1 < count && Math.Abs(value - segments[nearest + 1].Center) < Math.Abs(value - segments[nearest].Center)) nearest++;
                assignment[value] = (byte)nearest; weights[nearest] += frequency; moments[nearest] += value * (long)frequency;
            }
            var displacement = 0; long totalWeight = 0, totalMoment = 0;
            for (var k = 0; k < count; k++)
            {
                if (weights[k] == 0) continue;
                var center = (int)((moments[k] + weights[k] / 2) / weights[k]);
                displacement += Math.Abs(center - segments[k].Center);
                segments[k] = segments[k] with { Center = center };
                totalWeight += weights[k]; totalMoment += center * weights[k];
            }
            mean = (int)((totalMoment + totalWeight / 2) / totalWeight); iterations++;
            if (displacement < 5) break;
        }
        var minimum = segments[0].Center; var maximum = minimum;
        for (var k = 1; k < count; k++) { minimum = Math.Min(minimum, segments[k].Center); maximum = Math.Max(maximum, segments[k].Center); }
        var range = Math.Max(1, maximum - minimum);
        for (var k = 0; k < count; k++)
            segments[k] = segments[k] with
            {
                Alpha = Math.Clamp(255 * (segments[k].Center - mean) / range, -127, 127),
                Beta = Math.Clamp(255 * (segments[k].Center - minimum) / range, 0, 255)
            };
        // Assignment intentionally precedes the LAST centroid move. A new
        // nearest-centroid pass here would change the pinned encoder policy.
        return new(mean, iterations);
    }

    internal static void SmoothMap(Span<byte> map, int width, int height, Span<byte> replacement, OperationContext context)
    {
        Span<int> neighbors = stackalloc int[4];
        for (var y = 1; y < height - 1; y++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            for (var x = 1; x < width - 1; x++)
            {
                neighbors.Clear(); var at = y * width + x; var selected = map[at];
                for (var dy = -1; dy <= 1; dy++)
                for (var dx = -1; dx <= 1; dx++)
                    if (dx != 0 || dy != 0) neighbors[map[at + dy * width + dx]]++;
                for (var k = 0; k < 4; k++) if (neighbors[k] >= 5) { selected = (byte)k; break; }
                replacement[at] = selected;
            }
        }
        // Every vote reads the original map. Copy only the computed interiors;
        // border cells need no duplicate copy and remain exactly unchanged.
        for (var y = 1; y < height - 1; y++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            if (width > 2) replacement.Slice(y * width + 1, width - 2).CopyTo(map.Slice(y * width + 1, width - 2));
        }
    }

    internal static Vp8SourceSegments Create(Vp8SourceAnalysis source, int quality, bool smooth, OperationContext context)
    {
        if (quality is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(quality));
        context.Cancellation.ThrowIfCancellationRequested();
        var result = new Vp8SourceSegments(context, source.Width, source.Height);
        try
        {
            var capacity = source.Count + 4L * 5 * sizeof(int) + MetadataBytes;
            context.Reserve(capacity); result.retainedBytes = capacity;
            context.Cancellation.ThrowIfCancellationRequested(); result.map = new byte[source.Count]; result.parameters = new Segment[4];
            context.Reserve(ScratchBytes);
            try
            {
                context.Cancellation.ThrowIfCancellationRequested(); Span<byte> assignment = stackalloc byte[256];
                result.Statistics = Cluster(source.Histogram, assignment, result.parameters, context);
                context.Cancellation.ThrowIfCancellationRequested();
                for (var at = 0; at < source.Count; at++) result.map[at] = assignment[source.Alpha[at]];
                if (smooth && source.Width > 2 && source.Height > 2)
                {
                    context.Reserve(source.Count);
                    try
                    {
                        context.Cancellation.ThrowIfCancellationRequested(); var replacement = new byte[source.Count];
                        SmoothMap(result.map, source.Width, source.Height, replacement, context);
                    }
                    finally { context.Release(source.Count); }
                }
                // Current required settings use sns50/filter60/sharpness0.
                // The sharpness-zero minimum strength equals the edge delta;
                // no upstream filter lookup table is needed. Quality is explicit,
                // with frozen75 qualifying this private stage (no sweep claim).
                var q = quality / 100d;
                var compression = Math.Pow(q < 0.75 ? q * (2d / 3) : 2 * q - 1, 1d / 3);
                const double amplitude = 0.9 * 50 / 100 / 128;
                context.Cancellation.ThrowIfCancellationRequested();
                for (var k = 0; k < 4; k++)
                {
                    var segment = result.parameters[k];
                    var quantizer = Math.Clamp((int)(127 * (1 - Math.Pow(compression, 1 - amplitude * segment.Alpha))), 0, 127);
                    result.parameters[k] = segment with { Quantizer = quantizer, InitialFilter = FilterStrength(quantizer, segment.Beta) };
                }
                Span<byte> remap = stackalloc byte[4]; remap[0] = 0; var unique = 1;
                for (var k = 1; k < 4; k++)
                {
                    var segment = result.parameters[k]; var match = 0;
                    while (match < unique && (result.parameters[match].Quantizer != segment.Quantizer || result.parameters[match].InitialFilter != segment.InitialFilter)) match++;
                    remap[k] = (byte)match;
                    if (match == unique) result.parameters[unique++] = segment;
                }
                context.Cancellation.ThrowIfCancellationRequested();
                for (var at = 0; at < source.Count; at++) result.map[at] = remap[result.map[at]];
                for (var k = unique; k < 4; k++) result.parameters[k] = result.parameters[unique - 1];
                result.SegmentCount = unique;
            }
            finally { context.Release(ScratchBytes); }
            return result;
        }
        catch { result.Dispose(); throw; }
    }

    public void Dispose()
    {
        Interlocked.Exchange(ref map, null); Interlocked.Exchange(ref parameters, null);
        var owned = Interlocked.Exchange(ref retainedBytes, 0);
        if (owned != 0) context.Release(owned);
    }
}
