using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

// Independent RFC1950/1951 streaming stored/greedy/lazy encoder. Only a bounded history,
// dictionary, token block and tree workspace survive between input writes.
// The execution's borrowed context governs every substantial owned allocation.
internal sealed partial class PngDeflater : Stream
{
    internal const int WorkspaceBytes = 384 * 1024;
    private const int Window = 32768, Minimum = 3, Maximum = 258, Lookahead = 262, TokenCapacity = 16383;
    private readonly Stream output;
    private readonly OperationContext context;
    private readonly int level;
    private byte[] history = [], encoded = [];
    private ushort[] heads = [], older = [];
    private uint[] tokens = [];
    private DeflateHuffman.Workspace? treeWorkspace;
    private long position, filled, windowOrigin, deferredStart, blockStart;
    private int storedCount;
    private int deferredLength = Minimum - 1, tokenCount, encodedCount, pendingBits;
    private ulong pending;
    private uint adlerA = 1, adlerB;
    private bool hasDeferredByte, started, completed, disposed;

    internal PngDeflater(Stream output, OperationContext context, int level = 6)
    {
        if (level is not (0 or 1 or 6 or 9)) throw new ArgumentOutOfRangeException(nameof(level));
        this.output = output; this.context = context; this.level = level;
        // <261 KiB fixed arrays plus <64 KiB overlapping tree/code storage;
        // 384 KiB covers retained capacities, metadata and small owners.
        context.Reserve(WorkspaceBytes);
        try
        {
            history = new byte[2 * Window]; heads = new ushort[Window]; older = new ushort[Window];
            tokens = new uint[TokenCapacity]; encoded = new byte[4096];
            if (level != 0) treeWorkspace = new();
        }
        catch { context.Release(WorkspaceBytes); throw; }
    }

    public override void Write(ReadOnlySpan<byte> data)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (completed) throw new InvalidOperationException("PNG compression is complete.");
        context.Cancellation.ThrowIfCancellationRequested(); Start();
        if (level == 0) { WriteStored(data); return; }
        while (!data.IsEmpty)
        {
            AdvanceOrigin();
            var offset = (int)(filled - windowOrigin);
            var count = Math.Min(data.Length, history.Length - offset);
            if (count == 0) throw new InvalidOperationException("PNG history did not advance.");
            data[..count].CopyTo(history.AsSpan(offset));
            Adler32.Append(data[..count], ref adlerA, ref adlerB, context.Cancellation);
            filled += count; data = data[count..]; Process(false);
        }
    }

    internal void WriteRow(ReadOnlySpan<byte> row, byte filter = 0)
    {
        WriteByte(filter); Write(row);
        // A PNG scanline, including its filter byte, is one reference input
        // boundary. Flush stored data only after the entire row, never after
        // its separately written filter byte. Generic writes fill fixed blocks.
        if (level == 0 && storedCount >= Window) StoredBlock(false);
    }

    private void WriteStored(ReadOnlySpan<byte> data)
    {
        context.Cancellation.ThrowIfCancellationRequested();
        while (!data.IsEmpty)
        {
            var count = Math.Min(data.Length, 65531 - storedCount);
            data[..count].CopyTo(history.AsSpan(storedCount));
            Adler32.Append(data[..count], ref adlerA, ref adlerB, context.Cancellation);
            storedCount += count; data = data[count..];
            if (storedCount == 65531) StoredBlock(false);
        }
    }
    private void StoredBlock(bool final)
    {
        Put(final ? 1 : 0, 3); Align();
        OutputByte((byte)storedCount); OutputByte((byte)(storedCount >> 8));
        OutputByte((byte)~storedCount); OutputByte((byte)(~storedCount >> 8));
        OutputBytes(history.AsSpan(0, storedCount));
        storedCount = 0;
    }

    public override void WriteByte(byte value) { Span<byte> one = stackalloc byte[1]; one[0] = value; Write(one); }
    public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

    private void AdvanceOrigin()
    {
        if(position<windowOrigin+2*Window-Lookahead+1)return;
        // Byte history and dictionary links share the same relative origin.
        // The oldest eligible match is above Window at this boundary, including
        // a deferred match. Retain the upper half without allocating new storage.
        context.Cancellation.ThrowIfCancellationRequested();
        history.AsSpan(Window, Window).CopyTo(history);
        SlideLinks(heads);SlideLinks(older);windowOrigin+=Window;
    }
    private void SlideLinks(ushort[] links)
    {
        for(var start=0;start<links.Length;start+=4096)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var end=Math.Min(start+4096,links.Length);var i=start;
            if(Avx2.IsSupported)
                for(;i<=end-16;i+=16)
                    Avx2.SubtractSaturate(Vector256.Create<ushort>(links.AsSpan(i)),
                        Vector256.Create((ushort)Window)).CopyTo(links.AsSpan(i));
            for(;i<end;i++)links[i]=(ushort)Math.Max(0,links[i]-Window);
        }
    }
    private byte Byte(int at) => history[at];
    // Input history is contiguous in the current dictionary coordinate system.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ushort Pair(int at) => BinaryPrimitives.ReadUInt16LittleEndian(history.AsSpan(at, 2));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Insert(ReadOnlySpan<byte> bytes, Span<ushort> chainHeads, Span<ushort> links, int at)
    {
        var key = ((bytes[at] << 10) ^ (bytes[at + 1] << 5) ^ bytes[at + 2]) & (Window - 1);
        var prior = chainHeads[key]; links[at & (Window - 1)] = prior;
        // Even insertion through a deferred 258-byte match remains below 65536.
        chainHeads[key] = checked((ushort)at);
        return prior;
    }

    // Consecutive input triples share their first two bytes. Shifting the
    // previous15-bit key drops its oldest byte and inserts only the new byte.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int InsertSequential(int at, ref int key, ref int next)
    {
        if (at != next) key = (Byte(at) << 5) ^ Byte(at + 1);
        key = ((key << 5) ^ Byte(at + 2)) & (Window - 1); next = at + 1;
        var prior = heads[key]; older[at & (Window - 1)] = prior;
        heads[key] = checked((ushort)at); return prior;
    }

    private void Process(bool final)
    {
        if (level == 1) ProcessFast(final); else ProcessLazy(final);
    }

    private void ProcessFast(bool final)
    {
        // The acquired history extent and owned arrays survive this sweep.
        // Slides change their contents, not their identity. Keep bounded window
        // coordinates here and publish logical coordinates before callbacks.
        var origin = windowOrigin;
        var current = checked((int)(position - origin));
        var inputEnd = checked((int)(filled - origin));
        ReadOnlySpan<byte> bytes = history;
        Span<ushort> chainHeads = heads, links = older;
        Span<uint> outputTokens = tokens;
        var cancellation = context.Cancellation;
        var usedTokens = tokenCount;
        try
        {
            while (current < inputEnd && (final || inputEnd - current >= Lookahead))
            {
                cancellation.ThrowIfCancellationRequested();
                if (current >= 2 * Window - Lookahead + 1)
                {
                    PublishFast(origin + current, usedTokens); AdvanceOrigin();
                    origin = windowOrigin; current -= Window; inputEnd -= Window;
                }
                var prior = current + Minimum <= inputEnd ? Insert(bytes, chainHeads, links, current) : 0;
                var length = Minimum - 1; var start = 0;
                var available = Math.Min(Maximum, inputEnd - current);
                if (prior > 0 && current - prior <= Window - Lookahead)
                {
                    var searches = 4; var limit = Math.Max(0, current - (Window - Lookahead));
                    var prefixPair = BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(current, 2));
                    var tail = bytes[current + length];
                    do
                    {
                        cancellation.ThrowIfCancellationRequested();
                        if (length < available && bytes[prior + length] == tail
                            && BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(prior, 2)) == prefixPair)
                        {
                            var found = 2;
                            while (found < available && bytes[prior + found] == bytes[current + found]) found++;
                            cancellation.ThrowIfCancellationRequested();
                            if (found > length)
                            {
                                length = found; start = prior; if (found >= Math.Min(8, available)) break;
                                tail = bytes[current + length];
                            }
                        }
                        prior = links[prior & (Window - 1)];
                    } while (prior > limit && --searches != 0);
                }
                var copied = length >= Minimum;
                outputTokens[usedTokens++] = copied ? (uint)length | (uint)(current - start) << 9 : bytes[current];
                if (usedTokens == TokenCapacity)
                {
                    PublishFast(origin + current, usedTokens); Block(false); usedTokens = tokenCount;
                }
                if (copied)
                {
                    if (length <= 4 && inputEnd - current - length >= Minimum)
                        for (var at = current + 1; at < current + length; at++) Insert(bytes, chainHeads, links, at);
                    current += length;
                }
                else current++;
            }
        }
        finally { PublishFast(origin + current, usedTokens); }
    }

    private void PublishFast(long cursorPosition, int usedTokens)
    {
        position = cursorPosition; tokenCount = usedTokens;
    }

    private void ProcessLazy(bool final)
    {
        // Keep execution state local while consuming this immutable input extent.
        // Sliding and block publication use the same persistent state as Write
        // and Complete; callbacks and exceptional exits see a committed cursor.
        var key = 0; var nextInsertion = -1;
        var cursorPosition = position; var inputEnd = filled; var origin = windowOrigin;
        var pendingLength = deferredLength; var pendingStart = deferredStart;
        var deferredByte = hasDeferredByte; var usedTokens = tokenCount;
        try
        {
            while (cursorPosition < inputEnd && (final || inputEnd - cursorPosition >= Lookahead))
            {
                context.Cancellation.ThrowIfCancellationRequested();
                if (cursorPosition >= origin + 2 * Window - Lookahead + 1)
                {
                    PublishLazy(cursorPosition, pendingLength, pendingStart, deferredByte, usedTokens);
                    AdvanceOrigin(); origin = windowOrigin; nextInsertion = -1;
                }
                var current = (int)(cursorPosition - origin);
                var prior = cursorPosition + Minimum <= inputEnd ? InsertSequential(current, ref key, ref nextInsertion) : 0;
                var length = Minimum - 1; var start = 0;
                if (prior > 0 && current - prior <= Window - Lookahead && pendingLength < (level == 9 ? 258 : 16))
                {
                    var searches = level == 9 ? (pendingLength >= 32 ? 1024 : 4096) : pendingLength >= 8 ? 32 : 128;
                    var best = pendingLength; var limit = Math.Max(0, current - (Window - Lookahead));
                    var available = (int)Math.Min(Maximum, inputEnd - cursorPosition); var enough = Math.Min(level == 9 ? 258 : 128, available);
                    var prefixPair = Pair(current);
                    var tailPair = best < available ? Pair(current + best - 1) : (ushort)0;
                    do
                    {
                        context.Cancellation.ThrowIfCancellationRequested();
                        if (best < available && Pair(prior + best - 1) == tailPair && Pair(prior) == prefixPair)
                        {
                            var found = 2;
                            var prefix = Math.Min(8, available);
                            while (found < prefix && Byte(prior + found) == Byte(current + found)) found++;
                            if (found == 8 && found < available) found = MatchLength(history, prior, current, found, available);
                            context.Cancellation.ThrowIfCancellationRequested();
                            if (found > best)
                            {
                                best = found; start = prior; if (found >= enough) break;
                                tailPair = Pair(current + best - 1);
                            }
                        }
                        prior = older[prior & (Window - 1)];
                    } while (prior > limit && --searches != 0);
                    length = Math.Min(best, available);
                    if (length == Minimum && current - start > Window / 8) length = Minimum - 1;
                }
                if (pendingLength >= Minimum && pendingLength >= length)
                {
                    tokens[usedTokens++] = (uint)pendingLength | (uint)(cursorPosition - 1 - pendingStart) << 9;
                    if (usedTokens == TokenCapacity)
                    {
                        PublishLazy(cursorPosition, pendingLength, pendingStart, deferredByte, usedTokens);
                        Block(false); usedTokens = tokenCount;
                    }
                    var after = current + pendingLength - 1;
                    var lastInsert = (int)(inputEnd - origin) - Minimum;
                    for (var at = current + 1; at < after && at <= lastInsert; at++) InsertSequential(at, ref key, ref nextInsertion);
                    cursorPosition = origin + after; pendingLength = Minimum - 1; deferredByte = false;
                }
                else
                {
                    if (deferredByte)
                    {
                        tokens[usedTokens++] = Byte(current - 1);
                        if (usedTokens == TokenCapacity)
                        {
                            PublishLazy(cursorPosition, pendingLength, pendingStart, deferredByte, usedTokens);
                            Block(false); usedTokens = tokenCount;
                        }
                    }
                    deferredByte = true; pendingLength = length; pendingStart = origin + start; cursorPosition++;
                }
            }
        }
        finally { PublishLazy(cursorPosition, pendingLength, pendingStart, deferredByte, usedTokens); }
    }

    private void PublishLazy(long cursorPosition, int pendingLength, long pendingStart, bool deferredByte, int usedTokens)
    {
        position = cursorPosition; deferredLength = pendingLength; deferredStart = pendingStart;
        hasDeferredByte = deferredByte; tokenCount = usedTokens;
    }

    private void Emit(uint token)
    {
        tokens[tokenCount++] = token;
        if (tokenCount == TokenCapacity) Block(false);
    }

    // The caller verified the prefix. Both ranges are inside acquired immutable
    // history, including overlapping matches. Every word stays inside lookahead.
    internal static int MatchLength(ReadOnlySpan<byte> history, int prior, int current, int found, int available)
    {
        var left = history.Slice(prior, available); var right = history.Slice(current, available);
        for (; found <= available - 8; found += 8)
        {
            var different = BinaryPrimitives.ReadUInt64LittleEndian(left.Slice(found, 8))
                ^ BinaryPrimitives.ReadUInt64LittleEndian(right.Slice(found, 8));
            if (different != 0) return found + (BitOperations.TrailingZeroCount(different) >> 3);
        }
        for (; found < available; found++) if (left[found] != right[found]) return found;
        return found;
    }

    internal void Complete()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (completed) throw new InvalidOperationException("PNG compression is complete.");
        context.Cancellation.ThrowIfCancellationRequested(); Start();
        if (level == 0) StoredBlock(true);
        else
        {
            Process(true);
            if (hasDeferredByte) { Emit(Byte((int)(position - windowOrigin) - 1)); hasDeferredByte = false; }
            Block(true);
        }
        Align();
        var adler = adlerB << 16 | adlerA;
        for (var shift = 24; shift >= 0; shift -= 8) OutputByte((byte)(adler >> shift));
        Flush(); completed = true;
    }

    private void Start()
    {
        if (started) return;
        started = true; OutputByte(0x78); OutputByte(level == 9 ? (byte)0xda : level is 0 or 1 ? (byte)0x01 : (byte)0x9c);
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
    private void OutputBytes(ReadOnlySpan<byte> bytes)
    {
        while (!bytes.IsEmpty)
        {
            var count = Math.Min(bytes.Length, encoded.Length - encodedCount);
            bytes[..count].CopyTo(encoded.AsSpan(encodedCount));
            encodedCount += count; bytes = bytes[count..];
            // Keep the same publication/cancellation boundary as OutputByte,
            // including a full pending buffer when the writer throws.
            if (encodedCount == encoded.Length) Flush();
        }
    }
    public override void Flush()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        context.Cancellation.ThrowIfCancellationRequested();
        if (encodedCount == 0) return;
        output.Write(encoded.AsSpan(0, encodedCount)); encodedCount = 0;
        context.Cancellation.ThrowIfCancellationRequested();
    }
    protected override void Dispose(bool disposing)
    {
        // Completion is explicit. Failure cleanup must not write more output or
        // replace cancellation/admission/writer failures with a flush failure.
        if (disposing && !disposed)
        {
            disposed = true; history = []; heads = []; older = []; tokens = []; encoded = [];
            treeWorkspace = null;
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
