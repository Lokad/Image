namespace Lokad.Image.Internal;

// Private source-only analysis, before reconstruction and rate/distortion search.
// RFC6386 supplies block geometry and prediction. The pinned encoder policy
// evaluates DC/TM, mixes unquantized histogram scores, then inverts susceptibility.
// This owner escapes preparation within the SAME operation, never its context.
internal sealed class Vp8SourceAnalysis : IDisposable
{
    private const int MetadataBytes = 128, ScratchBytes = 1024;
    private readonly OperationContext context;
    private byte[]? records;
    private int[]? histogram;
    private long retainedBytes;
    internal int Width { get; }
    internal int Height { get; }
    internal int Count => Width * Height;
    internal long CapacityBytes => retainedBytes;
    internal int MeanAlpha { get; private set; }
    internal int MeanChromaScore { get; private set; }
    private ReadOnlySpan<byte> Records => records ?? throw new ObjectDisposedException(nameof(Vp8SourceAnalysis));
    internal ReadOnlySpan<byte> Alpha => Records[..Count];
    internal ReadOnlySpan<byte> LumaModes => Records.Slice(Count, Count);
    internal ReadOnlySpan<byte> ChromaModes => Records.Slice(2 * Count, Count);
    internal ReadOnlySpan<int> Histogram => histogram ?? throw new ObjectDisposedException(nameof(Vp8SourceAnalysis));

    private Vp8SourceAnalysis(OperationContext context, int width, int height)
    { this.context = context; Width = width; Height = height; }

    internal readonly record struct Candidate(Vp8SourceHistogram.Shape Luma, Vp8SourceHistogram.Shape Chroma);

    private static void CollectPlane(ReadOnlySpan<byte> plane, int stride, int x, int y, int size,
        Vp8IntraMode mode, Span<byte> prediction, Span<int> population, OperationContext context)
    {
        context.Work(size * size);
        ReadOnlySpan<byte> top = y == 0 ? default : plane.Slice((y - 1) * stride + x, size);
        Span<byte> left = stackalloc byte[16]; left = left[..size];
        if (x == 0) left = default;
        else for (var p = 0; p < size; p++) left[p] = plane[(y + p) * stride + x - 1];
        var corner = x != 0 && y != 0 ? plane[(y - 1) * stride + x - 1] : (byte)0;
        Vp8BlockPrediction.Fill(size, mode, top, left, corner, prediction);
        Span<short> residual = stackalloc short[16]; Span<short> coefficients = stackalloc short[16];
        for (var by = 0; by < size; by += 4)
        for (var bx = 0; bx < size; bx += 4)
        {
            context.Work(1024);
            for (var py = 0; py < 4; py++)
            for (var px = 0; px < 4; px++)
                residual[py * 4 + px] = (short)(plane[(y + by + py) * stride + x + bx + px]
                    - prediction[(by + py) * size + bx + px]);
            Vp8ForwardTransform.Dct(residual, coefficients);
            Vp8SourceHistogram.AddBlock(coefficients, population);
        }
    }

    // Borrowed source planes, caller scratch and result-by-value only. The
    // enclosing analysis reserves coarse scratch once, rather than per block.
    internal static Candidate Measure(Vp8PreparedInput source, int x, int y, Vp8IntraMode mode,
        Span<byte> prediction, Span<int> population, OperationContext context)
    {
        population.Clear();
        CollectPlane(source.Y, source.Stride, x * 16, y * 16, 16, mode, prediction, population, context);
        var luma = Vp8SourceHistogram.Describe(population);
        population.Clear();
        CollectPlane(source.U, source.Stride / 2, x * 8, y * 8, 8, mode, prediction, population, context);
        CollectPlane(source.V, source.Stride / 2, x * 8, y * 8, 8, mode, prediction, population, context);
        return new(luma, Vp8SourceHistogram.Describe(population));
    }

    internal static Vp8SourceAnalysis Create(Vp8PreparedInput source, OperationContext context)
    {
        context.Work(0);
        var result = new Vp8SourceAnalysis(context, source.Stride / 16, source.StorageHeight / 16);
        try
        {
            var capacity = checked(result.Count * 3L + 256 * sizeof(int) + MetadataBytes);
            context.Reserve(capacity); result.retainedBytes = capacity;
            context.Work(0); result.records = new byte[checked(result.Count * 3)]; result.histogram = new int[256];
            context.Reserve(ScratchBytes);
            try
            {
                context.Work(0);
                Span<byte> prediction = stackalloc byte[256]; Span<int> population = stackalloc int[32];
                long sumAlpha = 0, sumChroma = 0;
                for (var y = 0; y < result.Height; y++)
                for (var x = 0; x < result.Width; x++)
                {
                    var dc = Measure(source, x, y, Vp8IntraMode.Dc, prediction, population, context);
                    var tm = Measure(source, x, y, Vp8IntraMode.TrueMotion, prediction, population, context);
                    var dcY = dc.Luma.RawScore; var tmY = tm.Luma.RawScore;
                    var dcUv = dc.Chroma.RawScore; var tmUv = tm.Chroma.RawScore;
                    var uv = Math.Max(dcUv, tmUv);
                    var alpha = (byte)Math.Clamp(255 - ((3 * Math.Max(dcY, tmY) + uv + 2) >> 2), 0, 255);
                    var at = y * result.Width + x;
                    result.records[at] = alpha;
                    result.records[result.Count + at] = tmY > dcY ? (byte)1 : (byte)0;
                    result.records[2 * result.Count + at] = tmUv < dcUv ? (byte)1 : (byte)0;
                    result.histogram[alpha]++; sumAlpha += alpha; sumChroma += uv;
                }
                result.MeanAlpha = (int)(sumAlpha / result.Count);
                result.MeanChromaScore = (int)(sumChroma / result.Count);
            }
            finally { context.Release(ScratchBytes); }
            return result;
        }
        catch { result.Dispose(); throw; }
    }

    public void Dispose()
    {
        Interlocked.Exchange(ref records, null); Interlocked.Exchange(ref histogram, null);
        var owned = Interlocked.Exchange(ref retainedBytes, 0);
        if (owned != 0) context.Release(owned);
    }
}
