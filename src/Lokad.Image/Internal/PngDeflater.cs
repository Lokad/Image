namespace Lokad.Image.Internal;

// Independent RFC1950/1951 streaming level6 encoder. Only a bounded history,
// dictionary, token block and tree workspace survive between input writes.
// The execution's borrowed context governs every substantial owned allocation.
internal sealed partial class PngDeflater : Stream
{
    internal const int WorkspaceBytes = 1024 * 1024;
    private const int Window = 32768, Minimum = 3, Maximum = 258, Lookahead = 262, TokenCapacity = 16383;
    private readonly Stream output;
    private readonly OperationContext context;
    private byte[] history = [], encoded = [];
    private long[] heads = [], older = [];
    private uint[] tokens = [];
    private long position, filled, windowOrigin, deferredStart, blockStart;
    private int deferredLength = Minimum - 1, tokenCount, encodedCount, pendingBits;
    private uint pending, adlerA = 1, adlerB;
    private bool hasDeferredByte, started, completed, disposed;

    internal PngDeflater(Stream output, OperationContext context)
    {
        this.output = output; this.context = context;
        // ~660 KiB fixed arrays plus <64 KiB of overlapping tree/code storage;
        // 1 MiB conservatively covers actual retained capacities and minor owners.
        context.Reserve(WorkspaceBytes);
        try
        {
            history = new byte[2 * Window]; heads = new long[Window]; older = new long[Window];
            tokens = new uint[TokenCapacity]; encoded = new byte[4096];
        }
        catch { context.Release(WorkspaceBytes); throw; }
    }

    public override void Write(ReadOnlySpan<byte> data)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (completed) throw new InvalidOperationException("PNG compression is complete.");
        context.Work(0); Start();
        while (!data.IsEmpty)
        {
            AdvanceOrigin();
            var offset = (int)(filled & (2 * Window - 1));
            var count = (int)Math.Min(data.Length, 2 * Window - (filled - windowOrigin));
            count = Math.Min(count, history.Length - offset);
            if (count == 0) throw new InvalidOperationException("PNG history did not advance.");
            data[..count].CopyTo(history.AsSpan(offset));
            foreach (var value in data[..count])
            { adlerA = (adlerA + value) % 65521; adlerB = (adlerB + adlerA) % 65521; }
            filled += count; data = data[count..]; Process(false);
        }
    }

    public override void WriteByte(byte value) { Span<byte> one = stackalloc byte[1]; one[0] = value; Write(one); }
    public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

    private void AdvanceOrigin()
    {
        // Absolute dictionary indices avoid sliding/copying byte history. Index0
        // in each virtual window remains reserved, matching deterministic ties.
        if (position >= windowOrigin + 2 * Window - Lookahead + 1) windowOrigin += Window;
    }
    private byte Byte(long at) => history[(int)(at & (2 * Window - 1))];
    private long Insert(long at)
    {
        var key = ((Byte(at) << 10) ^ (Byte(at + 1) << 5) ^ Byte(at + 2)) & (Window - 1);
        var prior = heads[key]; older[(int)(at & (Window - 1))] = prior; heads[key] = at;
        return prior;
    }

    private void Process(bool final)
    {
        while (position < filled && (final || filled - position >= Lookahead))
        {
            context.Work(1); AdvanceOrigin();
            var prior = position + Minimum <= filled ? Insert(position) : 0;
            var length = Minimum - 1; long start = 0;
            if (prior > windowOrigin && position - prior <= Window - Lookahead && deferredLength < 16)
            {
                var budget = deferredLength >= 8 ? 32 : 128;
                var best = deferredLength; var limit = Math.Max(windowOrigin, position - (Window - Lookahead));
                var available = (int)Math.Min(Maximum, filled - position); var enough = Math.Min(128, available);
                do
                {
                    context.Work(1);
                    if (best < available && Byte(prior + best) == Byte(position + best)
                        && Byte(prior + best - 1) == Byte(position + best - 1)
                        && Byte(prior) == Byte(position) && Byte(prior + 1) == Byte(position + 1))
                    {
                        var found = 2;
                        while (found < available && Byte(prior + found) == Byte(position + found)) found++;
                        context.Work(found);
                        if (found > best) { best = found; start = prior; if (found >= enough) break; }
                    }
                    prior = older[(int)(prior & (Window - 1))];
                } while (prior > limit && --budget != 0);
                length = Math.Min(best, available);
                if (length == Minimum && position - start > Window / 8) length = Minimum - 1;
            }
            if (deferredLength >= Minimum && deferredLength >= length)
            {
                Emit((uint)deferredLength | (uint)(position - 1 - deferredStart) << 9);
                var after = position + deferredLength - 1;
                for (var at = position + 1; at < after && at + Minimum <= filled; at++) Insert(at);
                position = after; deferredLength = Minimum - 1; hasDeferredByte = false;
            }
            else
            {
                if (hasDeferredByte) Emit(Byte(position - 1));
                hasDeferredByte = true; deferredLength = length; deferredStart = start; position++;
            }
        }
    }

    private void Emit(uint token)
    {
        tokens[tokenCount++] = token;
        if (tokenCount == TokenCapacity) Block(false);
    }

    internal void Complete()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (completed) throw new InvalidOperationException("PNG compression is complete.");
        context.Work(0); Start(); Process(true);
        if (hasDeferredByte) { Emit(Byte(position - 1)); hasDeferredByte = false; }
        Block(true); Align();
        var adler = adlerB << 16 | adlerA;
        for (var shift = 24; shift >= 0; shift -= 8) OutputByte((byte)(adler >> shift));
        Flush(); completed = true;
    }

    private void Start()
    {
        if (started) return;
        started = true; OutputByte(0x78); OutputByte(0x9c);
    }
    private void Put(int value, int width)
    {
        pending |= (uint)value << pendingBits; pendingBits += width;
        while (pendingBits >= 8) { OutputByte((byte)pending); pending >>= 8; pendingBits -= 8; }
    }
    private void Align() { if (pendingBits != 0) OutputByte((byte)pending); pendingBits = 0; pending = 0; }
    private void OutputByte(byte value)
    {
        encoded[encodedCount++] = value;
        if (encodedCount == encoded.Length) Flush();
    }
    public override void Flush()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        context.Work(0);
        if (encodedCount == 0) return;
        output.Write(encoded.AsSpan(0, encodedCount)); encodedCount = 0;
        context.Work(0);
    }
    protected override void Dispose(bool disposing)
    {
        // Completion is explicit. Failure cleanup must not write more output or
        // replace cancellation/admission/writer failures with a flush failure.
        if (disposing && !disposed)
        {
            disposed = true; history = []; heads = []; older = []; tokens = []; encoded = [];
            context.Release(WorkspaceBytes);
        }
        base.Dispose(disposing);
    }
    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => !disposed && !completed;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
}
