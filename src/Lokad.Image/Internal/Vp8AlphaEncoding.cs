namespace Lokad.Image.Internal;

// Exact ALPH (RFC 9649 section 2.2.1), for pinned method0 and method4 policies.
// Alpha is borrowed only during preparation. The selected payload owns its
// admitted capacity; no source, filtered plane or encoded frame is retained.
internal sealed class Vp8AlphaEncoding : IDisposable
{
    private readonly OperationContext context;
    private byte[]? payload;
    private long capacity;
    internal int Width { get; }
    internal int Height { get; }
    internal long CapacityBytes => capacity;
    internal ReadOnlyMemory<byte> Memory => payload ?? throw new ObjectDisposedException(nameof(Vp8AlphaEncoding));
    internal ReadOnlySpan<byte> Bytes => Memory.Span;
    private Vp8AlphaEncoding(int width, int height, OperationContext context)
    { Width = width; Height = height; this.context = context; }

    private static int Predict(ReadOnlySpan<byte> alpha, int width, int at, int filter)
    {
        if (filter == 0 || at == 0) return 0;
        if (at < width) return alpha[at - 1];
        if (at % width == 0) return alpha[at - width];
        return filter switch
        {
            1 => alpha[at - 1], 2 => alpha[at - width],
            _ => Math.Clamp(alpha[at - 1] + alpha[at - width] - alpha[at - width - 1], 0, 255)
        };
    }

    // Occupied residual-magnitude bins estimate a second candidate. Ties retain
    // the earlier filter. The frozen FAST filter policy is used here;
    // other alpha effort policies remain separate qualification steps.
    private static int Candidates(ReadOnlySpan<byte> alpha, int width, OperationContext context, int method)
    {
        Span<byte> colors = stackalloc byte[256]; colors.Clear();
        for (var i = 0; i < alpha.Length; i++) { context.Work(1); colors[alpha[i]] = 1; }
        var count = 0; foreach (var color in colors) count += color;
        if (count <= 16) return 1;
        Span<ushort> bins = stackalloc ushort[4]; bins.Clear();
        for (var y = 2; y < alpha.Length / width - 1; y += 2)
        {
            var mean = (int)alpha[y * width];
            for (var x = 2; x < width - 1; x += 2)
            {
                context.Work(8); var at = y * width + x; var value = alpha[at];
                bins[0] |= (ushort)(1 << (Math.Abs(value - mean) >> 4));
                for (var f = 1; f < 4; f++) bins[f] |= (ushort)(1 << (Math.Abs(value - Predict(alpha, width, at, f)) >> 4));
                mean = (3 * mean + value + 2) >> 2;
            }
        }
        var best = 0; var score = int.MaxValue;
        for (var f = 0; f < 4; f++)
        {
            var sum = 0; for (var b = 0; b < 16; b++) if ((bins[f] & (1 << b)) != 0) sum += b;
            if (sum < score) { score = sum; best = f; }
        }
        return (method > 3 || count > 192 ? 1 : 0) | (1 << best);
    }

    internal static Vp8AlphaEncoding Create(ReadOnlySpan<byte> alpha, int width, OperationContext context, int method = 4)
    {
        if (method is not (0 or 4)) throw new ArgumentOutOfRangeException(nameof(method));
        if (width is < 1 or > 16383 || alpha.Length == 0 || alpha.Length % width != 0 || alpha.Length / width > 16383)
            throw new ArgumentException("Expected complete VP8 alpha rows.");
        context.Work(0); context.PixelBytes(width, alpha.Length / width, 8);
        var result = new Vp8AlphaEncoding(width, alpha.Length / width, context);
        try
        {
            context.Reserve(128); result.capacity = 128; context.Work(0);
            var scratch = alpha.Length * 4L + 512; context.Reserve(scratch);
            try
            {
                context.Work(0); var working = new uint[alpha.Length]; var candidates = Candidates(alpha, width, context, method);
                for (var filter = 0; filter < 4; filter++)
                {
                    if ((candidates & (1 << filter)) == 0) continue;
                    for (var i = 0; i < alpha.Length; i++)
                    {
                        context.Work(4);
                        // A/R/B are deliberately zero, including when alpha is
                        // carried in green: this is an internal exact image.
                        working[i] = (uint)((alpha[i] - Predict(alpha, width, i, filter)) & 255) << 8;
                    }
                    using var prepared = Vp8lPreparedEncoding.Prepare(working, width, method, context, quality: method == 0 ? 0 : 32);
                    var body = prepared.PayloadBytes - 5; var compressed = body <= alpha.Length;
                    var length = 1 + (compressed ? body : alpha.Length);
                    if (result.payload is not null && length >= result.payload.Length) continue;
                    // Old and new capacities overlap until emission succeeds.
                    context.Reserve(length);
                    try
                    {
                        context.Work(0); var next = new byte[length]; next[0] = (byte)((filter << 2) | (compressed ? 1 : 0));
                        if (compressed)
                        {
                            using var output = new MemoryStream(next, 1, length - 1, writable: true);
                            prepared.WriteAlphaBody(output);
                            if (output.Position != length - 1) throw new InvalidOperationException("ALPH size changed after preparation.");
                        }
                        else
                        {
                            for (var i = 0; i < alpha.Length; i++)
                            { context.Work(1); next[i + 1] = unchecked((byte)(alpha[i] - Predict(alpha, width, i, filter))); }
                        }
                        var old = result.payload; result.payload = next; result.capacity += length;
                        if (old is not null) { result.capacity -= old.Length; context.Release(old.Length); }
                    }
                    catch { context.Release(length); throw; }
                }
            }
            finally { context.Release(scratch); }
            if (result.Bytes.Length > context.Limits.MaxOutputBytes)
                throw new ImageLimitException(ImageLimitKind.OutputBytes, "Encoded ALPH exceeds its output limit.");
            return result;
        }
        catch { result.Dispose(); throw; }
    }

    public void Dispose()
    {
        payload = null; var owned = Interlocked.Exchange(ref capacity, 0); if (owned != 0) context.Release(owned);
    }
}
