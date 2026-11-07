using System.Runtime.InteropServices;

namespace Lokad.Image.Internal;

// Operation-owned RFC6386 residual instructions. One word carries an adaptive
// base, class/remainder, sign and EOB scheduling, rather than one word per bit.
// Final probability selection happens after construction; no source is retained.
internal sealed class Vp8CoefficientProgram : IDisposable
{
    private const uint BlockEnd = 1u << 28;
    private const uint Literal = 1u << 29;
    private readonly OperationContext context;
    private Chunk? first, last;
    private int nextLength = 256;
    private long capacity;
    internal long CapacityBytes => capacity;

    private sealed class Chunk(uint[] values)
    {
        internal readonly uint[] Values = values;
        internal int Count;
        internal Chunk? Next;
    }

    internal Vp8CoefficientProgram(OperationContext context)
    {
        this.context = context;
        context.Reserve(128); capacity = 128;
        try { context.Cancellation.ThrowIfCancellationRequested(); }
        catch { capacity = 0; context.Release(128); throw; }
    }

    private void Append(ReadOnlySpan<uint> values)
    {
        if (capacity == 0) throw new ObjectDisposedException(nameof(Vp8CoefficientProgram));
        context.Cancellation.ThrowIfCancellationRequested();
        if (last is null || values.Length > last.Values.Length - last.Count)
        {
            var bytes = nextLength * 4L + 64; // Array and linked chunk metadata.
            context.Reserve(bytes);
            Chunk next;
            try
            {
                context.Cancellation.ThrowIfCancellationRequested();
                next = new Chunk(new uint[nextLength]);
            }
            catch { context.Release(bytes); throw; }
            if (last is null) first = next; else last.Next = next;
            last = next; capacity += bytes; nextLength = Math.Min(4096, nextLength * 2);
        }
        values.CopyTo(last.Values.AsSpan(last.Count)); last.Count += values.Length;
    }

    internal void EndBlock() => Append([BlockEnd]);

    // Direct method0-2 counter policy. In particular, category5/6 statistics
    // address node10, unlike the ordinary encoder's token-buffer policy.
    internal bool Record(ReadOnlySpan<short> levels, int type, int first, int initialContext, Span<uint> statistics)
    {
        if (levels.Length < 16 || statistics.Length < 1056 || (uint)type > 3 || (uint)first > 1 || (uint)initialContext > 2
            || first == 1 && levels[0] != 0) throw new ArgumentException("Invalid VP8 coefficient program input.");
        if (MemoryMarshal.AsBytes(levels[..16]).Overlaps(MemoryMarshal.AsBytes(statistics[..1056])))
            throw new ArgumentException("Coefficient inputs must not overlap writable statistics.");
        var last = Vp8ResidualSummary.Last(levels, first, validate: true);
        context.Cancellation.ThrowIfCancellationRequested();
        Span<uint> prepared = stackalloc uint[17]; var count = 0;
        var at = ((type * 8 + Vp8Tables.CoefficientBand(first)) * 3 + initialContext) * 11;
        Vp8TokenStatistics.Event(statistics, at, last >= 0);
        if (last < 0) { prepared[count++] = (uint)(at | 15 << 11); Append(prepared[..count]); return false; }
        var before = true;
        for (var n = first; n <= last; n++)
        {
            var level = Math.Abs((int)levels[n]); var kind = level; var remainder = 0;
            Vp8TokenStatistics.Event(statistics, at + 1, level != 0);
            if (level != 0)
            {
                Vp8TokenStatistics.Event(statistics, at + 2, level > 1);
                if (level > 1)
                {
                    Vp8TokenStatistics.Event(statistics, at + 3, level > 4);
                    if (level <= 4)
                    {
                        Vp8TokenStatistics.Event(statistics, at + 4, level != 2);
                        if (level != 2) Vp8TokenStatistics.Event(statistics, at + 5, level == 4);
                    }
                    else
                    {
                        Vp8TokenStatistics.Event(statistics, at + 6, level > 10);
                        if (level <= 10) Vp8TokenStatistics.Event(statistics, at + 7, level > 6);
                        else
                        {
                            Vp8TokenStatistics.Event(statistics, at + 8, level >= 35);
                            Vp8TokenStatistics.Event(statistics, at + (level < 35 ? 9 : 10), level >= (level < 35 ? 19 : 67));
                        }
                        var category = 0; var start = 5; var bits = 1;
                        while (category < 5 && level >= start + (1 << bits)) { start += 1 << bits; bits++; category++; }
                        kind = 5 + category; remainder = level - start;
                    }
                }
            }
            prepared[count++] = (uint)(at | kind << 11 | remainder << 15
                | (levels[n] < 0 ? 1 << 26 : 0) | (before ? 1 << 27 : 0));
            if (n == 15) continue;
            at = ((type * 8 + Vp8Tables.CoefficientBand(n + 1)) * 3 + Math.Min(level, 2)) * 11;
            before = level != 0;
            if (level != 0) Vp8TokenStatistics.Event(statistics, at, n < last);
            if (n == last) prepared[count++] = (uint)(at | 15 << 11);
        }
        if (count > 9)
        {
            // A dense residual's original levels plus its context cost nine
            // words. Select by physical representation size before admission,
            // independently of shared availability; refusal still terminates.
            Span<uint> literal = stackalloc uint[9];
            literal[0] = Literal | (uint)(type | first << 2 | initialContext << 3);
            levels[..16].CopyTo(MemoryMarshal.Cast<uint, short>(literal[1..]));
            Append(literal);
        }
        else Append(prepared[..count]);
        return true;
    }

    internal Cursor Borrow() => capacity != 0 ? new(this) : throw new ObjectDisposedException(nameof(Vp8CoefficientProgram));

    internal struct Cursor
    {
        private Chunk? chunk;
        private int position;
        internal Cursor(Vp8CoefficientProgram program) { chunk = program.first; position = 0; }

        internal void WriteBlock(ref Vp8PartitionWriter.Cursor writer, ReadOnlySpan<byte> probabilities, bool skip)
        {
            var current = chunk; var offset = position;
            while (current is not null)
            {
                while (offset < current.Count)
                {
                    var instruction = current.Values[offset++];
                    if (instruction == BlockEnd) { chunk = current; position = offset; return; }
                    if ((instruction & Literal) != 0)
                    {
                        // Append keeps a literal and its operands in one chunk.
                        if (!skip) Vp8FrameEncoding.Coefficients(ref writer,
                            MemoryMarshal.Cast<uint, short>(current.Values.AsSpan(offset, 8)),
                            (int)(instruction & 3), (int)(instruction >> 2 & 1), (int)(instruction >> 3 & 3), probabilities);
                        offset += 8;
                    }
                    else if (!skip) WriteInstruction(ref writer, probabilities, instruction);
                }
                current = current.Next; offset = 0;
            }
            throw new InvalidOperationException("Incomplete coefficient program.");
        }
    }

    private static void WriteInstruction(ref Vp8PartitionWriter.Cursor writer, ReadOnlySpan<byte> probabilities, uint instruction)
    {
        var at = (int)(instruction & 2047); var kind = (int)(instruction >> 11 & 15);
        if (kind == 15) { writer.Write(false, probabilities[at]); return; }
        if ((instruction & 1u << 27) != 0) writer.Write(true, probabilities[at]);
        writer.Write(kind != 0, probabilities[at + 1]); if (kind == 0) return;
        writer.Write(kind > 1, probabilities[at + 2]);
        if (kind > 1)
        {
            writer.Write(kind > 4, probabilities[at + 3]);
            if (kind <= 4)
            {
                writer.Write(kind != 2, probabilities[at + 4]);
                if (kind != 2) writer.Write(kind == 4, probabilities[at + 5]);
            }
            else
            {
                writer.Write(kind >= 7, probabilities[at + 6]);
                if (kind < 7) writer.Write(kind == 6, probabilities[at + 7]);
                else
                {
                    writer.Write(kind >= 9, probabilities[at + 8]);
                    writer.Write(kind is 8 or 10, probabilities[at + (kind < 9 ? 9 : 10)]);
                }
                var category = Vp8FrameEncoding.Category(kind - 5); var remainder = (int)(instruction >> 15 & 2047);
                for (var b = 0; b < category.Length; b++)
                    writer.Write((remainder & (1 << (category.Length - b - 1))) != 0, category[b]);
            }
        }
        writer.Write((instruction & 1u << 26) != 0);
    }

    public void Dispose()
    {
        first = last = null;
        var bytes = Interlocked.Exchange(ref capacity, 0); if (bytes != 0) context.Release(bytes);
    }
}
