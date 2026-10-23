namespace Lokad.Image.Internal;

internal readonly record struct Vp8FrameBlock(int LumaMode, int ChromaMode, int Segment, bool Intra16, bool Skipped);

// Operation-confined owner for the two-pass coefficient stream. Source pixels,
// analysis and matrices are borrowed only during creation. Retain coefficients,
// modes and final probabilities; reconstructed pixels need only rolling borders.
// The public encoder is not switched here.
internal sealed class Vp8FramePlan : IDisposable
{
    private const int MetadataBytes = 256, BlockBytes = 20;
    private readonly OperationContext context;
    private short[]? coefficients;
    private byte[]? blocks, probabilities;
    private int[]? parameters;
    private long capacity;
    internal int Width { get; }
    internal int Height { get; }
    internal int Columns { get; }
    internal int Rows { get; }
    internal int Count => Columns * Rows;
    internal int SegmentCount { get; }
    internal int HeaderLimit { get; private set; }
    internal int HeaderRetries { get; private set; }
    internal bool UseSkipProbability { get; private set; }
    internal byte SkipProbability { get; private set; } = 255;
    internal Vp8QuantizationPlan.Deltas Deltas { get; }
    internal (byte Root, byte Left, byte Right) SegmentProbabilities { get; private set; }
    internal bool UpdateMap => SegmentCount > 1 && SegmentProbabilities != ((byte)255, (byte)255, (byte)255);
    internal long CapacityBytes => capacity;
    internal ReadOnlySpan<short> Coefficients => coefficients ?? throw new ObjectDisposedException(nameof(Vp8FramePlan));
    internal ReadOnlySpan<byte> BlockData => blocks ?? throw new ObjectDisposedException(nameof(Vp8FramePlan));
    internal ReadOnlySpan<byte> Probabilities => probabilities ?? throw new ObjectDisposedException(nameof(Vp8FramePlan));
    private ReadOnlySpan<int> Parameters => parameters ?? throw new ObjectDisposedException(nameof(Vp8FramePlan));
    internal ReadOnlySpan<int> Quantizers => Parameters[..4];
    internal ReadOnlySpan<int> FilterLevels => Parameters.Slice(12, 4);

    private Vp8FramePlan(Vp8PreparedInput input, Vp8SourceSegments segments, Vp8QuantizationPlan quantization,
        int headerLimit, OperationContext context)
    {
        this.context = context; Width = input.Width; Height = input.Height; Columns = input.Stride / 16; Rows = input.StorageHeight / 16;
        SegmentCount = segments.SegmentCount; Deltas = quantization.QuantizerDeltas; HeaderLimit = headerLimit;
    }

    internal Vp8FrameBlock Block(int index)
    {
        if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
        var data = BlockData.Slice(index * BlockBytes, BlockBytes);
        return new(data[16], data[17], data[19], (data[18] & 1) != 0, (data[18] & 2) != 0);
    }

    internal ReadOnlySpan<byte> SmallModes(int index)
    {
        if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
        return BlockData.Slice(index * BlockBytes, 16);
    }

    internal ReadOnlySpan<short> Levels(int index)
    {
        if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
        return Coefficients.Slice(index * 400, 400);
    }

    private static byte Probability(int a, int b) => a + b == 0 ? (byte)255 : (byte)((255L * a + (a + b) / 2) / (a + b));

    // Advance logical edge contexts directly, without depending on the native
    // observer's packed internal representation. The Y2 edge survives I4 blocks.
    internal static void Record(ReadOnlySpan<short> levels, bool intra16, Span<byte> contexts, Span<uint> statistics, bool tokenBuffer = true)
    {
        var top = contexts[..9]; var left = contexts.Slice(9, 9);
        if (intra16) top[8] = left[8] = Vp8TokenStatistics.Record(levels[..16], 1, 0, top[8] + left[8], statistics, tokenBuffer) ? (byte)1 : (byte)0;
        for (var y = 0; y < 4; y++) for (var x = 0; x < 4; x++)
            top[x] = left[y] = Vp8TokenStatistics.Record(levels.Slice(16 + (y * 4 + x) * 16, 16), intra16 ? 0 : 3, intra16 ? 1 : 0, top[x] + left[y], statistics, tokenBuffer) ? (byte)1 : (byte)0;
        for (var channel = 0; channel < 2; channel++) for (var y = 0; y < 2; y++) for (var x = 0; x < 2; x++)
        {
            var a = 4 + channel * 2 + x; var b = 4 + channel * 2 + y;
            top[a] = left[b] = Vp8TokenStatistics.Record(levels.Slice(272 + (channel * 4 + y * 2 + x) * 16, 16), 2, 0, top[a] + left[b], statistics, tokenBuffer) ? (byte)1 : (byte)0;
        }
    }

    internal static Vp8FramePlan Create(Vp8PreparedInput input, Vp8SourceSegments segments, Vp8QuantizationPlan quantization,
        OperationContext context, int headerLimit = 65536)
    {
        if (headerLimit is < 0 or > 65536 || segments.Width != input.Stride / 16 || segments.Height != input.StorageHeight / 16
            || quantization.SegmentCount != segments.SegmentCount) throw new ArgumentException("Inconsistent VP8 frame preparation.");
        context.Work(0); var result = new Vp8FramePlan(input, segments, quantization, headerLimit, context);
        try
        {
            result.Initialize(segments);
            var scratch = Vp8MacroblockDecision.ScratchBytes + 4096L + 49L * result.Columns + 4224 + 512;
            context.Reserve(scratch);
            try
            {
                context.Work(0); result.Prepare(input, segments, quantization);
            }
            finally { context.Release(scratch); }
            return result;
        }
        catch { result.Dispose(); throw; }
    }

    private void Initialize(Vp8SourceSegments segments)
    {
        var bytes = Count * 820L + 1056 + 16 * sizeof(int) + MetadataBytes;
        context.Reserve(bytes); capacity = bytes; context.Work(0);
        coefficients = new short[checked(Count * 400)]; blocks = new byte[checked(Count * BlockBytes)];
        probabilities = Vp8Tables.CoefficientProbabilities.ToArray(); parameters = new int[16];
        for (var s = 0; s < 4; s++) { parameters[s] = segments.Parameters[s].Quantizer; parameters[4 + s] = segments.Parameters[s].InitialFilter; }
    }

    internal static Vp8FramePlan CreateDistortion(Vp8PreparedInput input, Vp8SourceAnalysis analysis,
        Vp8SourceSegments segments, Vp8QuantizationPlan quantization, int method, OperationContext context, bool correctDc = true)
    {
        if (method is < 0 or > 2 || analysis.Width != input.Stride / 16 || analysis.Height != input.StorageHeight / 16
            || segments.Width != analysis.Width || segments.Height != analysis.Height || quantization.SegmentCount != segments.SegmentCount)
            throw new ArgumentException("Inconsistent VP8 distortion frame preparation.");
        context.Work(0); var result = new Vp8FramePlan(input, segments, quantization, 65536, context);
        try
        {
            result.Initialize(segments);
            var scratch = Vp8DistortionDecision.ScratchBytes + 4096L + 45L * result.Columns + 4224 + 512;
            context.Reserve(scratch);
            try { context.Work(0); result.PrepareDistortion(input, analysis, segments, quantization, method, correctDc); }
            finally { context.Release(scratch); }
            return result;
        }
        catch { result.Dispose(); throw; }
    }

    private void PrepareDistortion(Vp8PreparedInput input, Vp8SourceAnalysis analysis, Vp8SourceSegments segments,
        Vp8QuantizationPlan quantization, int method, bool correctDc)
    {
        var yTop = new byte[Columns * 16]; var uvTop = new byte[Columns * 16]; var modeTop = new byte[Columns * 4];
        var coefficientTop = new byte[Columns * 9]; var statistics = new uint[1056];
        Span<byte> source = stackalloc byte[384]; Span<byte> neighbors = stackalloc byte[71]; Span<byte> modeContexts = stackalloc byte[8];
        Span<byte> contexts = stackalloc byte[18]; Span<byte> modeLeft = stackalloc byte[4]; Span<sbyte> diffusion = stackalloc sbyte[8]; diffusion.Clear();
        Span<sbyte> errors = stackalloc sbyte[14]; Span<byte> pixels = stackalloc byte[384]; Span<int> counts = stackalloc int[4]; counts.Clear();
        context.Work(Count * 24L);
        for (var index = 0; index < Count; index++)
        {
            var data = blocks!.AsSpan(index * BlockBytes, BlockBytes); var small = analysis.LumaModes[index] == 2;
            data[16] = small ? (byte)0 : analysis.LumaModes[index]; data[17] = analysis.ChromaModes[index];
            data[18] = small ? (byte)0 : (byte)1; data[19] = segments.Map[index]; counts[data[19]]++;
        }
        SegmentProbabilities = (Probability(counts[0] + counts[1], counts[2] + counts[3]), Probability(counts[0], counts[1]), Probability(counts[2], counts[3]));
        using var rate = Vp8RatePlan.Create(probabilities!, context);
        var headerCost = SegmentHeaderCost(counts, rate.Entropy); var skipped = 0;
        var statisticsCount = method == 0 ? Math.Min(Count, Count > 200 ? Count >> 2 : 50) : Count;
        for (var pass = 0; pass < 2; pass++)
        {
            context.Work(Columns * 45L); yTop.AsSpan().Fill(127); uvTop.AsSpan().Fill(127); modeTop.AsSpan().Clear();
            coefficientTop.AsSpan().Clear(); contexts.Clear(); var count = pass == 0 ? statisticsCount : Count;
            for (var index = 0; index < count; index++)
            {
                context.Work(32768); var x = index % Columns; var y = index / Columns;
                if (x == 0)
                {
                    neighbors[..35].Fill(129); neighbors[0] = neighbors[17] = neighbors[26] = y == 0 ? (byte)127 : (byte)129;
                    modeLeft.Clear(); contexts[9..].Clear();
                }
                for (var yy = 0; yy < 16; yy++) input.Y.Slice((y * 16 + yy) * input.Stride + x * 16, 16).CopyTo(source.Slice(yy * 16, 16));
                for (var yy = 0; yy < 8; yy++)
                {
                    input.U.Slice((y * 8 + yy) * (input.Stride / 2) + x * 8, 8).CopyTo(source.Slice(256 + yy * 16, 8));
                    input.V.Slice((y * 8 + yy) * (input.Stride / 2) + x * 8, 8).CopyTo(source.Slice(264 + yy * 16, 8));
                }
                yTop.AsSpan(x * 16, 16).CopyTo(neighbors[35..]); uvTop.AsSpan(x * 16, 16).CopyTo(neighbors[51..]);
                for (var p = 0; p < 4; p++) neighbors[67 + p] = x + 1 < Columns ? yTop[(x + 1) * 16 + p] : yTop[x * 16 + 15];
                modeTop.AsSpan(x * 4, 4).CopyTo(modeContexts); modeLeft.CopyTo(modeContexts[4..]);
                var data = blocks!.AsSpan(index * BlockBytes, BlockBytes); var levels = coefficients!.AsSpan(index * 400, 400); var segment = data[19];
                var score = Vp8DistortionDecision.Select(source, neighbors, modeContexts, diffusion, y != 0, x != 0,
                    method, (data[18] & 1) != 0, data[17], 256L * 510 * 8 * 1024 / Count, correctDc,
                    quantization.Matrix(segment, Vp8CoefficientPlane.Luma), quantization.Matrix(segment, Vp8CoefficientPlane.Walsh), quantization.Matrix(segment, Vp8CoefficientPlane.Chroma),
                    quantization.Costs(segment), rate, pixels, levels, data[..16], errors, context);
                data[16] = (byte)score.LumaMode; data[17] = (byte)score.ChromaMode;
                data[18] = (byte)((score.Intra16 ? 1 : 0) | (score.Nonzero == 0 ? 2 : 0));
                if (pass == 0)
                {
                    if (score.Nonzero == 0) skipped++;
                    coefficientTop.AsSpan(x * 9, 9).CopyTo(contexts); Record(levels, score.Intra16, contexts, statistics, tokenBuffer: false);
                    contexts[..9].CopyTo(coefficientTop.AsSpan(x * 9, 9));
                }
                for (var p = 0; p < 4; p++)
                {
                    modeTop[x * 4 + p] = score.Intra16 ? (byte)score.LumaMode : data[12 + p];
                    modeLeft[p] = score.Intra16 ? (byte)score.LumaMode : data[p * 4 + 3];
                }
                if (x + 1 < Columns)
                {
                    neighbors[0] = neighbors[50]; neighbors[17] = neighbors[58]; neighbors[26] = neighbors[66];
                    for (var p = 0; p < 16; p++) neighbors[1 + p] = pixels[p * 16 + 15];
                    for (var p = 0; p < 8; p++) { neighbors[18 + p] = pixels[256 + p * 16 + 7]; neighbors[27 + p] = pixels[256 + p * 16 + 15]; }
                }
                if (y + 1 < Rows) { pixels.Slice(240, 16).CopyTo(yTop.AsSpan(x * 16, 16)); pixels.Slice(368, 16).CopyTo(uvTop.AsSpan(x * 16, 16)); }
            }
            if (pass == 0 && headerCost != 0)
            {
                // Native distortion-only scores have H=0. A zero map cost makes
                // its statistics loop return before finalizing either profile;
                // the final encoder still runs with defaults in that case.
                context.Work(1056 * 16L); Vp8TokenStatistics.Update(statistics, rate.Entropy, probabilities!, out _);
                SkipProbability = (byte)((Count - skipped) * 255L / Count); UseSkipProbability = SkipProbability < 250;
            }
        }
        for (var s = 0; s < 4; s++) parameters![12 + s] = parameters[4 + s];
    }

    private void Prepare(Vp8PreparedInput input, Vp8SourceSegments segments, Vp8QuantizationPlan quantization)
    {
        var ownedParameters = parameters!;
        var yTop = new byte[Columns * 16]; var uvTop = new byte[Columns * 16]; var modeTop = new byte[Columns * 4];
        var coefficientTop = new byte[Columns * 9]; var errorTop = new sbyte[Columns * 4]; var statistics = new uint[1056];
        Span<byte> source = stackalloc byte[384]; Span<byte> neighbors = stackalloc byte[71]; Span<byte> modeContexts = stackalloc byte[8];
        Span<byte> contexts = stackalloc byte[18]; Span<byte> modeLeft = stackalloc byte[4]; Span<sbyte> errorLeft = stackalloc sbyte[4];
        Span<sbyte> errorInput = stackalloc sbyte[8]; Span<sbyte> errors = stackalloc sbyte[14]; Span<byte> pixels = stackalloc byte[384];
        Span<ushort> weights = stackalloc ushort[16]; Span<int> counts = stackalloc int[4];
        Vp8Distortion.TextureWeights(weights); counts.Clear();
        foreach (var segment in segments.Map) counts[segment]++;
        SegmentProbabilities = (Probability(counts[0] + counts[1], counts[2] + counts[3]), Probability(counts[0], counts[1]), Probability(counts[2], counts[3]));
        while (true)
        {
            context.Work(Count); yTop.AsSpan().Fill(127); uvTop.AsSpan().Fill(127); modeTop.AsSpan().Clear();
            coefficientTop.AsSpan().Clear(); errorTop.AsSpan().Clear(); statistics.AsSpan().Clear(); ownedParameters.AsSpan(8, 4).Clear();
            Vp8RatePlan? rate = null;
            try
            {
                // On a header-budget retry, native prices initially retain the
                // previous pass's last mid-frame update. Counters restart empty.
                rate = Vp8RatePlan.Create(probabilities, context); long headerCost = SegmentHeaderCost(counts, rate.Entropy);
                for (var index = 0; index < Count; index++)
                {
                    context.Work(32768); var x = index % Columns; var y = index / Columns;
                    if (x == 0)
                    {
                        neighbors[..35].Fill(129); neighbors[0] = neighbors[17] = neighbors[26] = y == 0 ? (byte)127 : (byte)129;
                        modeLeft.Clear(); errorLeft.Clear(); contexts[9..].Clear();
                    }
                    if (index != 0 && (index + 1) % (Math.Max(Count >> 3, 96) + 1) == 0)
                    {
                        Vp8TokenStatistics.Update(statistics, rate.Entropy, probabilities, out _);
                        var next = Vp8RatePlan.Create(probabilities, context); rate.Dispose(); rate = next;
                    }
                    for (var yy = 0; yy < 16; yy++) input.Y.Slice((y * 16 + yy) * input.Stride + x * 16, 16).CopyTo(source.Slice(yy * 16, 16));
                    for (var yy = 0; yy < 8; yy++)
                    {
                        input.U.Slice((y * 8 + yy) * (input.Stride / 2) + x * 8, 8).CopyTo(source.Slice(256 + yy * 16, 8));
                        input.V.Slice((y * 8 + yy) * (input.Stride / 2) + x * 8, 8).CopyTo(source.Slice(264 + yy * 16, 8));
                    }
                    yTop.AsSpan(x * 16, 16).CopyTo(neighbors[35..]); uvTop.AsSpan(x * 16, 16).CopyTo(neighbors[51..]);
                    for (var p = 0; p < 4; p++) neighbors[67 + p] = x + 1 < Columns ? yTop[(x + 1) * 16 + p] : yTop[x * 16 + 15];
                    modeTop.AsSpan(x * 4, 4).CopyTo(modeContexts); modeLeft.CopyTo(modeContexts[4..]);
                    coefficientTop.AsSpan(x * 9, 9).CopyTo(contexts); errorTop.AsSpan(x * 4, 4).CopyTo(errorInput); errorLeft.CopyTo(errorInput[4..]);
                    var segment = segments.Map[index]; var data = blocks!.AsSpan(index * BlockBytes, BlockBytes); var levels = coefficients!.AsSpan(index * 400, 400);
                    var score = Vp8MacroblockDecision.Select(source, neighbors, modeContexts, contexts, errorInput, y != 0, x != 0,
                        quantization.Matrix(segment, Vp8CoefficientPlane.Luma), quantization.Matrix(segment, Vp8CoefficientPlane.Walsh), quantization.Matrix(segment, Vp8CoefficientPlane.Chroma),
                        quantization.Costs(segment), rate, weights, HeaderLimit, pixels, levels, data[..16], errors, context);
                    data[16] = (byte)score.LumaMode; data[17] = (byte)score.ChromaMode; data[18] = (byte)((score.Intra16 ? 1 : 0) | (score.Nonzero == 0 ? 2 : 0)); data[19] = segment;
                    headerCost += score.Header; ownedParameters[8 + segment] = Math.Max(ownedParameters[8 + segment], score.MaximumEdge);
                    Record(levels, score.Intra16, contexts, statistics); contexts[..9].CopyTo(coefficientTop.AsSpan(x * 9, 9));
                    for (var p = 0; p < 4; p++)
                    {
                        modeTop[x * 4 + p] = score.Intra16 ? (byte)score.LumaMode : data[12 + p];
                        modeLeft[p] = score.Intra16 ? (byte)score.LumaMode : data[p * 4 + 3];
                    }
                    if (x + 1 < Columns)
                    {
                        neighbors[0] = neighbors[50]; neighbors[17] = neighbors[58]; neighbors[26] = neighbors[66];
                        for (var p = 0; p < 16; p++) neighbors[1 + p] = pixels[p * 16 + 15];
                        for (var p = 0; p < 8; p++) { neighbors[18 + p] = pixels[256 + p * 16 + 7]; neighbors[27 + p] = pixels[256 + p * 16 + 15]; }
                    }
                    if (y + 1 < Rows) { pixels.Slice(240, 16).CopyTo(yTop.AsSpan(x * 16, 16)); pixels.Slice(368, 16).CopyTo(uvTop.AsSpan(x * 16, 16)); }
                    errors.Slice(6, 4).CopyTo(errorTop.AsSpan(x * 4, 4)); errors.Slice(10, 4).CopyTo(errorLeft);
                }
                // Header-size strengthening is a format policy, not a resource
                // refusal fallback. All reservations/work failures remain terminal.
                if (HeaderLimit > 0 && headerCost > ((524288L - 2048) << 11)) { HeaderLimit >>= 1; HeaderRetries++; continue; }
                Vp8TokenStatistics.Update(statistics, rate.Entropy, probabilities, out _);
                for (var s = 0; s < 4; s++)
                {
                    // Required settings use sharpness0: inner-edge threshold is
                    // level itself, so its inverse is the clamped edge step.
                    var delta = s < SegmentCount ? (ownedParameters[8 + s] * quantization.Matrix(s, Vp8CoefficientPlane.Walsh).Steps[1]) >> 3 : 0;
                    ownedParameters[12 + s] = Math.Max(ownedParameters[4 + s], Math.Min(63, delta));
                }
                return;
            }
            finally { rate?.Dispose(); }
        }
    }

    private long SegmentHeaderCost(ReadOnlySpan<int> counts, ReadOnlySpan<ushort> entropy)
    {
        if (SegmentCount <= 1) return 0;
        var p = SegmentProbabilities; long cost = 0;
        for (var s = 0; s < 4; s++) cost += (long)counts[s] * (entropy[s >= 2 ? 255 - p.Root : p.Root]
            + entropy[(s & 1) != 0 ? 255 - (s < 2 ? p.Left : p.Right) : s < 2 ? p.Left : p.Right]);
        return cost;
    }

    public void Dispose()
    {
        Interlocked.Exchange(ref coefficients, null); Interlocked.Exchange(ref blocks, null);
        Interlocked.Exchange(ref probabilities, null); Interlocked.Exchange(ref parameters, null);
        var bytes = Interlocked.Exchange(ref capacity, 0); if (bytes != 0) context.Release(bytes);
    }
}
