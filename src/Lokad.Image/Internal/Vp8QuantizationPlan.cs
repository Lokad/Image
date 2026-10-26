namespace Lokad.Image.Internal;

internal enum Vp8CoefficientPlane { Luma, Walsh, Chroma }

// Borrowed views into the plan's admitted storage. Their lifetime stays inside
// the owner/operation; they introduce no cache, allocation or per-block scope.
internal readonly ref struct Vp8QuantizationMatrix
{
    private readonly ReadOnlySpan<int> values;
    internal Vp8QuantizationMatrix(ReadOnlySpan<int> values) => this.values = values;
    internal ReadOnlySpan<int> Steps => values[..16];
    internal ReadOnlySpan<int> Reciprocals => values.Slice(16, 16);
    internal ReadOnlySpan<int> Biases => values.Slice(32, 16);
    internal ReadOnlySpan<int> ZeroThresholds => values.Slice(48, 16);
    internal ReadOnlySpan<int> Sharpening => values.Slice(64, 16);
    internal ReadOnlySpan<int> Values => values;
}

internal readonly ref struct Vp8QuantizationCosts
{
    private readonly ReadOnlySpan<int> values;
    internal Vp8QuantizationCosts(ReadOnlySpan<int> values) => this.values = values;
    internal int Luma4 => values[0];
    internal int Luma16 => values[1];
    internal int Chroma => values[2];
    internal int Mode => values[3];
    internal int Luma4Trellis => values[4];
    internal int Luma16Trellis => values[5];
    internal int ChromaTrellis => values[6];
    internal int Texture => values[7];
    internal int MinimumDistortion => values[8];
    internal int Intra4Penalty => values[9];
    internal ReadOnlySpan<int> Values => values;
}

// Private preparation, confined to the SAME operation as its context. RFC6386
// section14.1 supplies steps and the Walsh scale; encoder policy supplies the
// rounding/dead-zone biases and rate/distortion weights. No upstream tables or
// source objects are retained. The public encoder borrows these admitted views.
internal sealed class Vp8QuantizationPlan : IDisposable
{
    private const int MatrixLength = 5 * 16, SegmentLength = 3 * MatrixLength + 10, MetadataBytes = 128;
    private readonly OperationContext context;
    private int[]? storage;
    private long retainedBytes;
    internal readonly record struct Deltas(int LumaDc, int WalshDc, int WalshAc, int ChromaDc, int ChromaAc);
    internal Deltas QuantizerDeltas { get; }
    internal int SegmentCount { get; }
    internal long CapacityBytes => retainedBytes;
    private ReadOnlySpan<int> Storage => storage ?? throw new ObjectDisposedException(nameof(Vp8QuantizationPlan));

    private Vp8QuantizationPlan(OperationContext context, int count, int chromaScore)
    {
        this.context = context; SegmentCount = count;
        // Current required analysis settings use sns50. Keep both signed
        // divisions separate: merging them changes negative rounding.
        var ac = Math.Clamp(((chromaScore - 64) * 10 / 70) * 50 / 100, -4, 6);
        QuantizerDeltas = new(0, 0, 0, -2, ac);
    }

    internal Vp8QuantizationMatrix Matrix(int segment, Vp8CoefficientPlane plane)
    {
        if ((uint)segment >= SegmentCount) throw new ArgumentOutOfRangeException(nameof(segment));
        if ((uint)plane > 2) throw new ArgumentOutOfRangeException(nameof(plane));
        return new(Storage.Slice(segment * SegmentLength + (int)plane * MatrixLength, MatrixLength));
    }

    internal Vp8QuantizationCosts Costs(int segment)
    {
        if ((uint)segment >= SegmentCount) throw new ArgumentOutOfRangeException(nameof(segment));
        return new(Storage.Slice(segment * SegmentLength + 3 * MatrixLength, 10));
    }

    private static int FillMatrix(Span<int> values, int dc, int ac, Vp8CoefficientPlane plane)
    {
        // These six bias choices are scalar encoder policies. Frequency
        // sharpening is generated from the two-dimensional coefficient order.
        var dcBias = plane == Vp8CoefficientPlane.Chroma ? 110 : 96;
        var acBias = plane == Vp8CoefficientPlane.Luma ? 110 : plane == Vp8CoefficientPlane.Walsh ? 108 : 115;
        for (var p = 0; p < 16; p++)
        {
            var step = p == 0 ? dc : ac; var reciprocal = (1 << 17) / step;
            var bias = (p == 0 ? dcBias : acBias) << 9;
            values[p] = step; values[16 + p] = reciprocal; values[32 + p] = bias;
            values[48 + p] = ((1 << 17) - 1 - bias) / reciprocal;
            values[64 + p] = plane == Vp8CoefficientPlane.Luma ? (30 * Math.Min(3, (p & 3) + (p >> 2)) * step) >> 11 : 0;
        }
        return (dc + 15 * ac + 8) >> 4;
    }

    internal static Vp8QuantizationPlan Create(Vp8SourceSegments source, int chromaScore, OperationContext context, int method = 4)
    {
        if (method is < 0 or > 6) throw new ArgumentOutOfRangeException(nameof(method));
        if (chromaScore is < 0 or > 7905) throw new ArgumentOutOfRangeException(nameof(chromaScore));
        context.Cancellation.ThrowIfCancellationRequested();
        var result = new Vp8QuantizationPlan(context, source.SegmentCount, chromaScore);
        try
        {
            var length = checked(result.SegmentCount * SegmentLength);
            var capacity = length * (long)sizeof(int) + MetadataBytes;
            context.Reserve(capacity); result.retainedBytes = capacity;
            context.Cancellation.ThrowIfCancellationRequested(); result.storage = new int[length];
            // The twelve-byte temporary and small value locals fit within the
            // coarse metadata allowance; no separate minor-allocation scope.
            Span<int> average = stackalloc int[3];
            for (var segment = 0; segment < result.SegmentCount; segment++)
            {
                var q = source.Parameters[segment].Quantizer;
                var values = result.storage.AsSpan(segment * SegmentLength, SegmentLength);
                for (var plane = 0; plane < 3; plane++)
                {
                    context.Cancellation.ThrowIfCancellationRequested();
                    var dc = plane == 0 ? Vp8Tables.DcQuantizers[q]
                        : plane == 1 ? 2 * Vp8Tables.DcQuantizers[q]
                        : Vp8Tables.DcQuantizers[Math.Clamp(q + result.QuantizerDeltas.ChromaDc, 0, 117)];
                    var ac = plane == 0 ? Vp8Tables.AcQuantizers[q]
                        : plane == 1 ? Math.Max(8, Vp8Tables.AcQuantizers[q] * 155 / 100)
                        : Vp8Tables.AcQuantizers[Math.Clamp(q + result.QuantizerDeltas.ChromaAc, 0, 127)];
                    average[plane] = FillMatrix(values.Slice(plane * MatrixLength, MatrixLength), dc, ac, (Vp8CoefficientPlane)plane);
                }
                context.Cancellation.ThrowIfCancellationRequested();
                var y = average[0]; var y2 = average[1]; var uv = average[2];
                var costs = values[(3 * MatrixLength)..];
                costs[0] = Math.Max(1, 3 * y * y >> 7); costs[1] = Math.Max(1, 3 * y2 * y2);
                costs[2] = Math.Max(1, 3 * uv * uv >> 6); costs[3] = Math.Max(1, y * y >> 7);
                costs[4] = Math.Max(1, 7 * y * y >> 3); costs[5] = Math.Max(1, y2 * y2 >> 2);
                costs[6] = Math.Max(1, uv * uv << 1); costs[7] = Math.Max(1, (method >= 4 ? 50 : 0) * y >> 5);
                costs[8] = 20 * Vp8Tables.DcQuantizers[q];
                // All VP8 steps bound the average to276, so this policy's
                // penalty fits Int32. RDO combines scores with wide arithmetic.
                costs[9] = 1000 * y * y;
            }
            return result;
        }
        catch { result.Dispose(); throw; }
    }

    public void Dispose()
    {
        Interlocked.Exchange(ref storage, null);
        var owned = Interlocked.Exchange(ref retainedBytes, 0);
        if (owned != 0) context.Release(owned);
    }
}
