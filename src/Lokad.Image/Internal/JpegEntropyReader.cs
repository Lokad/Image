using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Numerics;

namespace Lokad.Image.Internal;

/// <summary>MSB-first JPEG entropy bits with marker-aware byte unstuffing.</summary>
internal ref struct JpegEntropyReader
{
    private readonly ReadOnlySpan<byte> data;
    private readonly OperationContext context;
    private int position;
    private uint bits;
    private int available;
    private long remainingWork, initialWork;
    public int Position => position;
    internal bool HasCancelableToken => context.Cancellation.CanBeCanceled;

    public JpegEntropyReader(ReadOnlySpan<byte> data, int start, OperationContext context)
    {
        if ((uint)start > (uint)data.Length) throw new ArgumentOutOfRangeException(nameof(start));
        this.data = data; this.context = context; position = start; bits = 0; available = 0; remainingWork = -1; initialWork = -1;
    }

    internal void BeginLocalWork()
    {
        // Baseline block traversal makes no other parent-context charges in
        // this scope. Keep each exact entropy admission locally, then publish
        // successful charges before any further parent work, on every exit.
        // A cancelable budget is complemented, retaining the original hot
        // noncancelable branch. Negative initialWork means no local scope;
        // a zero remaining budget still has an unambiguous representation.
        initialWork = context.RemainingWork;
        remainingWork = context.Cancellation.CanBeCanceled ? ~initialWork : initialWork;
    }

    internal void EndLocalWork()
    {
        if (initialWork < 0) return;
        var remaining = remainingWork < 0 ? ~remainingWork : remainingWork;
        // Do not add a cancellation check while publishing completed work:
        // it would lose earlier charges or replace the original exception.
        context.CommitAdmittedWork(initialWork - remaining);
        remainingWork = initialWork = -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Work(int units)
    {
        if (remainingWork < 0)
        {
            if (initialWork < 0) { context.Work(units); return; }
            if (!TryCancelableWork(units)) throw new ImageLimitException(ImageLimitKind.Work, "Computational work limit exceeded.");
            return;
        }
        if (units < 0 || units > remainingWork) throw new ImageLimitException(ImageLimitKind.Work, "Computational work limit exceeded.");
        remainingWork -= units;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryWork(int units)
    {
        if (remainingWork < 0) return initialWork < 0 ? context.TryWork(units) : TryCancelableWork(units);
        if (units < 0 || units > remainingWork) return false;
        remainingWork -= units; return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryCancelableWork(int units)
    {
        context.Cancellation.ThrowIfCancellationRequested();
        var budget = ~remainingWork;
        if (units < 0 || units > budget) return false;
        remainingWork = ~(budget - units); return true;
    }

    public int ReadBits(int count)
    {
        if ((uint)count > 16) throw new ArgumentOutOfRangeException(nameof(count));
        Work(1);
        while (available < count)
        {
            if (position >= data.Length) throw new ImageDataException("Truncated JPEG entropy data.");
            var next = data[position++];
            if (next == 255)
            {
                if (position >= data.Length || data[position++] != 0)
                    throw new ImageDataException("Unexpected marker in JPEG entropy data.");
            }
            bits = bits << 8 | next;
            available += 8;
        }
        available -= count;
        return (int)((bits >> available) & ((1u << count) - 1));
    }

    public int ReadSigned(int count)
    {
        if (count == 0) return 0;
        var value = ReadBits(count);
        // T.81 F.2.2.1: a missing leading one subtracts 2^count - 1.
        // ReadBits admits only 1..16 here, so the difference fits Int32;
        // its sign gives an exact mask without a data-dependent branch.
        var threshold = 1 << (count - 1);
        return value + (((value - threshold) >> 31) & (1 - (threshold << 1)));
    }

    internal bool TryReadCorrectionBits(int count, out int value)
    {
        value = 0;
        if (count is < 1 or > 16) throw new ArgumentOutOfRangeException(nameof(count));
        // Correction bits each charge one unit. Speculate only through bytes
        // required by this group, then admit its exact cost atomically. Any
        // marker, truncation, tight budget or cancellable token uses ReadBits(1).
        if (context.Cancellation.CanBeCanceled) return false;
        var candidateBits = bits; var candidateAvailable = available; var candidatePosition = position;
        while (candidateAvailable < count)
        {
            if (candidatePosition >= data.Length) return false;
            var next = data[candidatePosition++];
            if (next == 255 && (candidatePosition >= data.Length || data[candidatePosition++] != 0)) return false;
            candidateBits = candidateBits << 8 | next; candidateAvailable += 8;
        }
        if (!TryWork(count)) return false;
        bits = candidateBits; available = candidateAvailable - count; position = candidatePosition;
        value = (int)((bits >> available) & ((1u << count) - 1)); return true;
    }

    internal bool TryReadBufferedCancelableRefinementBlock(ReadOnlySpan<ushort> shortCodes, JpegHuffman ac,
        Span<int> block, int start, int end, int low, ref int eobRun)
    {
        // T.81 G.1.2 has the same <2048-unit bound as the original fused
        // traversal. Keep speculative ordinary bytes separate from the logical
        // cursor. Every fallible/cancellation path publishes before leaving;
        // normal locals need no address-exposed ref helpers or enclosing EH.
        if (initialWork >= 0 || !context.Cancellation.CanBeCanceled || context.RemainingWork < 2048
            || !Avx2.IsSupported || !Bmi2.X64.IsSupported || shortCodes.Length != 256
            || available is < 0 or > 7
            || block.Length != 64 || start < 1 || start > end || end > 63 || low is < 0 or > 13)
            return false;
        var cancellation = context.Cancellation;
        var input = data;
        var cursor = position; ulong cache = bits; var pending = available;
        var work = 0; var index = start; var increment = 1 << low;
        ulong nonzero = 0;
        for (var offset = 0; offset < 64; offset += 8)
        {
            var values = Vector256.Create(block.Slice(offset, 8));
            var zeros = (uint)Avx.MoveMask(Avx2.CompareEqual(values, Vector256<int>.Zero).AsSingle());
            nonzero |= (ulong)(~zeros & 255) << offset;
        }
        var bandEnd = end == 63 ? ulong.MaxValue : (1UL << (end + 1)) - 1;
        while (index <= end)
        {
            var finish = eobRun > 0;
            var run = 0; var inserted = 0; var stop = end + 1;
            ulong correctionPositions, zeroPrefix = 0;
            if (!finish)
            {
                // Ordinary lookahead is read-only and cannot surface a future
                // marker or truncation. Return every unused whole byte when
                // publishing; escaped/short tails use the original reader.
                if (pending < 16 && input.Length - cursor >= 4)
                {
                    var word = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(input.Slice(cursor, 4));
                    if ((unchecked(~word - 0x01010101u) & word & 0x80808080u) == 0)
                    { cache = cache << 32 | word; pending += 32; cursor += 4; }
                }
                var prefix = pending >= 8 ? (int)((cache >> (pending - 8)) & 255)
                    : (int)((cache & ((1UL << pending) - 1)) << (8 - pending));
                var packed = shortCodes[prefix]; var length = packed & 15; int symbol;
                if (length is > 0 and <= 8 && length <= pending)
                {
                    if (cancellation.IsCancellationRequested)
                    {
                        PublishCancelableRefinementLookahead(cache, pending, cursor, work);
                        cancellation.ThrowIfCancellationRequested();
                    }
                    work += length; pending -= length; symbol = packed >> 4;
                }
                else
                {
                    PublishCancelableRefinementLookahead(cache, pending, cursor, work); work = 0;
                    symbol = ac.Read(ref this);
                    cache = bits; pending = available; cursor = position;
                }
                run = symbol >> 4; var size = symbol & 15;
                if (size != 0 && size != 1)
                {
                    PublishCancelableRefinementLookahead(cache, pending, cursor, work);
                    throw new ImageDataException("Invalid JPEG refinement symbol.");
                }
                if (size != 0 || run != 15)
                {
                    var count = size != 0 ? 1 : run; int field;
                    if (pending < count && input.Length - cursor >= 4)
                    {
                        var word = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(input.Slice(cursor, 4));
                        if ((unchecked(~word - 0x01010101u) & word & 0x80808080u) == 0)
                        { cache = cache << 32 | word; pending += 32; cursor += 4; }
                    }
                    if (pending >= count)
                    {
                        if (cancellation.IsCancellationRequested)
                        {
                            PublishCancelableRefinementLookahead(cache, pending, cursor, work);
                            cancellation.ThrowIfCancellationRequested();
                        }
                        work++; pending -= count; field = (int)((cache >> pending) & ((1u << count) - 1));
                    }
                    else
                    {
                        PublishCancelableRefinementLookahead(cache, pending, cursor, work); work = 0;
                        field = ReadBits(count);
                        cache = bits; pending = available; cursor = position;
                    }
                    if (size != 0) inserted = field == 0 ? -increment : increment;
                    else { eobRun = (1 << run) + field; finish = true; }
                }
                else run = 16;
            }
            var remaining = bandEnd & (ulong.MaxValue << index);
            if (finish) correctionPositions = nonzero & remaining;
            else
            {
                var zeros = ~nonzero & remaining;
                var target = Bmi2.X64.ParallelBitDeposit(1UL << (inserted != 0 ? run : run - 1), zeros);
                stop = target == 0 ? end + 1 : BitOperations.TrailingZeroCount(target) + (inserted != 0 ? 0 : 1);
                var prefix = (stop == 64 ? ulong.MaxValue : (1UL << stop) - 1) & remaining;
                zeroPrefix = zeros & prefix; correctionPositions = nonzero & prefix;
            }
            while (correctionPositions != 0)
            {
                ref var value = ref block[BitOperations.TrailingZeroCount(correctionPositions)];
                if (pending == 0 && input.Length - cursor >= 4)
                {
                    var word = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(input.Slice(cursor, 4));
                    if ((unchecked(~word - 0x01010101u) & word & 0x80808080u) == 0)
                    { cache = cache << 32 | word; pending += 32; cursor += 4; }
                }
                int bit;
                if (pending != 0)
                {
                    if (cancellation.IsCancellationRequested)
                    {
                        PublishCancelableRefinementLookahead(cache, pending, cursor, work);
                        cancellation.ThrowIfCancellationRequested();
                    }
                    work++; bit = (int)((cache >> --pending) & 1);
                }
                else
                {
                    PublishCancelableRefinementLookahead(cache, pending, cursor, work); work = 0;
                    bit = ReadBits(1);
                    cache = bits; pending = available; cursor = position;
                }
                if (bit != 0)
                {
                    // ApplyCorrection can throw only for Int32.MinValue. Its
                    // already-consumed correction bit must survive that error.
                    if (value == int.MinValue) PublishCancelableRefinementLookahead(cache, pending, cursor, work);
                    JpegDecoder.ApplyCorrection(ref value, increment);
                }
                correctionPositions &= correctionPositions - 1;
            }
            if (finish)
            {
                eobRun--; PublishCancelableRefinementLookahead(cache, pending, cursor, work); return true;
            }
            run -= BitOperations.PopCount(zeroPrefix); index = stop;
            if (run != 0 || (inserted != 0 && index > end))
            {
                PublishCancelableRefinementLookahead(cache, pending, cursor, work);
                throw new ImageDataException("JPEG refinement run exceeds band.");
            }
            if (inserted != 0) { block[index] = inserted; nonzero |= 1UL << index; index++; }
        }
        PublishCancelableRefinementLookahead(cache, pending, cursor, work); return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void PublishCancelableRefinementLookahead(ulong cache, int pending, int cursor, int work)
    {
        var wholeBytes = pending >> 3;
        bits = unchecked((uint)(cache >> (wholeBytes * 8))); available = pending & 7;
        position = cursor - wholeBytes;
        context.CommitAdmittedWork(work);
    }

    internal bool TryReadCancelableRefinementBlock(ReadOnlySpan<ushort> shortCodes, JpegHuffman ac,
        Span<int> block, int start, int end, int low, ref int eobRun)
    {
        // T.81 G.1.2 visits at most 63 coefficients. Even 64 sixteen-bit
        // Huffman codes, signs, corrections and an EOB field cost <2048 units.
        // Tight budgets retain the original traversal; every logical read below
        // preserves its original live-token cancellation check.
        if (initialWork >= 0 || !context.Cancellation.CanBeCanceled || context.RemainingWork < 2048
            || !Avx2.IsSupported || !Bmi2.X64.IsSupported
            || block.Length != 64 || start < 1 || start > end || end > 63 || low is < 0 or > 13)
            return false;
        var cancellation = context.Cancellation;
        var input = data;
        var cursor = position; var cache = bits; var pending = available;
        var work = 0; var index = start; var increment = 1 << low;
        ulong nonzero = 0;
        for (var offset = 0; offset < 64; offset += 8)
        {
            var values = Vector256.Create(block.Slice(offset, 8));
            var zeros = (uint)Avx.MoveMask(
                Avx2.CompareEqual(values, Vector256<int>.Zero).AsSingle());
            nonzero |= (ulong)(~zeros & 255) << offset;
        }
        var bandEnd = end == 63 ? ulong.MaxValue : (1UL << (end + 1)) - 1;
        try
        {
            while (index <= end)
            {
                var finish = eobRun > 0;
                var run = 0; var inserted = 0; var stop = end + 1;
                ulong correctionPositions, zeroPrefix = 0;
                if (!finish)
                {
                    // Complete short prefixes commit only their required byte.
                    // Canonical/marker tails retain the existing Huffman reader.
                    var candidateCache = cache; var candidatePending = pending; var candidateCursor = cursor;
                    var prefix = (int)((cache & ((1u << pending) - 1)) << (8 - pending));
                    var packed = shortCodes[prefix]; var length = packed & 15;
                    if (length == 0 || length > candidatePending)
                    {
                        if (candidateCursor < input.Length)
                        {
                            var next = input[candidateCursor++];
                            if (next != 255 || (candidateCursor < input.Length && input[candidateCursor++] == 0))
                            {
                                candidateCache = candidateCache << 8 | next; candidatePending += 8;
                                prefix = (int)((candidateCache >> (candidatePending - 8)) & 255);
                                packed = shortCodes[prefix]; length = packed & 15;
                            }
                            else length = 0;
                        }
                        else length = 0;
                    }
                    int symbol;
                    if (length != 0 && length <= candidatePending)
                    {
                        cancellation.ThrowIfCancellationRequested();
                        work += length; cache = candidateCache; pending = candidatePending - length;
                        cursor = candidateCursor; symbol = packed >> 4;
                    }
                    else
                    {
                        bits = cache; available = pending; position = cursor;
                        context.CommitAdmittedWork(work); work = 0;
                        try { symbol = ac.Read(ref this); }
                        finally { cache = bits; pending = available; cursor = position; }
                    }
                    run = symbol >> 4; var size = symbol & 15;
                    if (size != 0)
                    {
                        if (size != 1) throw new ImageDataException("Invalid JPEG refinement symbol.");
                        inserted = ReadCancelableRefinementBits(cancellation, input, ref cursor, ref cache, ref pending, 1, ref work) == 0 ? -increment : increment;
                    }
                    else if (run != 15)
                    {
                        eobRun = (1 << run) + ReadCancelableRefinementBits(cancellation, input, ref cursor, ref cache, ref pending, run, ref work);
                        finish = true;
                    }
                    else run = 16;
                }
                var remaining = bandEnd & (ulong.MaxValue << index);
                if (finish) correctionPositions = nonzero & remaining;
                else
                {
                    var zeros = ~nonzero & remaining;
                    var target = Bmi2.X64.ParallelBitDeposit(1UL << (inserted != 0 ? run : run - 1), zeros);
                    stop = target == 0 ? end + 1 : BitOperations.TrailingZeroCount(target) + (inserted != 0 ? 0 : 1);
                    var prefix = (stop == 64 ? ulong.MaxValue : (1UL << stop) - 1) & remaining;
                    zeroPrefix = zeros & prefix;
                    correctionPositions = nonzero & prefix;
                }
                while (correctionPositions != 0)
                {
                    ref var value = ref block[BitOperations.TrailingZeroCount(correctionPositions)];
                    if (ReadCancelableRefinementBits(cancellation, input, ref cursor, ref cache, ref pending, 1, ref work) != 0)
                        JpegDecoder.ApplyCorrection(ref value, increment);
                    correctionPositions &= correctionPositions - 1;
                }
                if (finish) { eobRun--; return true; }
                run -= BitOperations.PopCount(zeroPrefix); index = stop;
                if (run != 0 || (inserted != 0 && index > end))
                    throw new ImageDataException("JPEG refinement run exceeds band.");
                if (inserted != 0) { block[index] = inserted; nonzero |= 1UL << index; index++; }
            }
            return true;
        }
        finally
        {
            bits = cache; available = pending; position = cursor;
            context.CommitAdmittedWork(work);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ReadCancelableRefinementBits(CancellationToken cancellation, ReadOnlySpan<byte> input, ref int cursor,
        ref uint cache, ref int pending, int count, ref int work)
    {
        cancellation.ThrowIfCancellationRequested();
        work++;
        while (pending < count)
        {
            if (cursor >= input.Length) throw new ImageDataException("Truncated JPEG entropy data.");
            var next = input[cursor++];
            if (next == 255 && (cursor >= input.Length || input[cursor++] != 0))
                throw new ImageDataException("Unexpected marker in JPEG entropy data.");
            cache = cache << 8 | next; pending += 8;
        }
        pending -= count;
        return (int)((cache >> pending) & ((1u << count) - 1));
    }

    internal bool TryReadRefinementBlock(ReadOnlySpan<ushort> shortCodes, JpegHuffman ac,
        Span<int> block, int start, int end, int low, ref int eobRun)
    {
        // T.81 G.1.2 visits at most 63 coefficients. Even 64 sixteen-bit
        // Huffman codes, signs, corrections and an EOB field cost <2048 units.
        // Tight budgets and live cancellation retain the preceding traversal.
        if (initialWork >= 0 || context.Cancellation.CanBeCanceled || context.RemainingWork < 2048
            || !Avx2.IsSupported || !Bmi2.X64.IsSupported
            || block.Length != 64 || start < 1 || start > end || end > 63 || low is < 0 or > 13)
            return false;
        var input = data;
        var cursor = position; var cache = bits; var pending = available;
        var work = 0; var index = start; var increment = 1 << low;
        ulong nonzero = 0;
        for (var offset = 0; offset < 64; offset += 8)
        {
            var values = Vector256.Create(block.Slice(offset, 8));
            var zeros = (uint)Avx.MoveMask(
                Avx2.CompareEqual(values, Vector256<int>.Zero).AsSingle());
            nonzero |= (ulong)(~zeros & 255) << offset;
        }
        var bandEnd = end == 63 ? ulong.MaxValue : (1UL << (end + 1)) - 1;
        try
        {
            while (index <= end)
            {
                var finish = eobRun > 0;
                var run = 0; var inserted = 0; var stop = end + 1;
                ulong correctionPositions, zeroPrefix = 0;
                if (!finish)
                {
                    // Complete short prefixes commit only their required byte.
                    // Canonical/marker tails retain the existing Huffman reader.
                    var candidateCache = cache; var candidatePending = pending; var candidateCursor = cursor;
                    var prefix = (int)((cache & ((1u << pending) - 1)) << (8 - pending));
                    var packed = shortCodes[prefix]; var length = packed & 15;
                    if (length == 0 || length > candidatePending)
                    {
                        if (candidateCursor < input.Length)
                        {
                            var next = input[candidateCursor++];
                            if (next != 255 || (candidateCursor < input.Length && input[candidateCursor++] == 0))
                            {
                                candidateCache = candidateCache << 8 | next; candidatePending += 8;
                                prefix = (int)((candidateCache >> (candidatePending - 8)) & 255);
                                packed = shortCodes[prefix]; length = packed & 15;
                            }
                            else length = 0;
                        }
                        else length = 0;
                    }
                    int symbol;
                    if (length != 0 && length <= candidatePending)
                    {
                        work += length; cache = candidateCache; pending = candidatePending - length;
                        cursor = candidateCursor; symbol = packed >> 4;
                    }
                    else
                    {
                        bits = cache; available = pending; position = cursor;
                        context.CommitAdmittedWork(work); work = 0;
                        try { symbol = ac.Read(ref this); }
                        finally { cache = bits; pending = available; cursor = position; }
                    }
                    run = symbol >> 4; var size = symbol & 15;
                    if (size != 0)
                    {
                        if (size != 1) throw new ImageDataException("Invalid JPEG refinement symbol.");
                        inserted = ReadRefinementBits(input, ref cursor, ref cache, ref pending, 1, ref work) == 0 ? -increment : increment;
                    }
                    else if (run != 15)
                    {
                        eobRun = (1 << run) + ReadRefinementBits(input, ref cursor, ref cache, ref pending, run, ref work);
                        finish = true;
                    }
                    else run = 16;
                }
                var remaining = bandEnd & (ulong.MaxValue << index);
                if (finish) correctionPositions = nonzero & remaining;
                else
                {
                    var zeros = ~nonzero & remaining;
                    var target = Bmi2.X64.ParallelBitDeposit(1UL << (inserted != 0 ? run : run - 1), zeros);
                    stop = target == 0 ? end + 1 : BitOperations.TrailingZeroCount(target) + (inserted != 0 ? 0 : 1);
                    var prefix = (stop == 64 ? ulong.MaxValue : (1UL << stop) - 1) & remaining;
                    zeroPrefix = zeros & prefix;
                    correctionPositions = nonzero & prefix;
                }
                while (correctionPositions != 0)
                {
                    var remainingCount = BitOperations.PopCount(correctionPositions);
                    var count = Math.Min(16, remainingCount);
                    var selected = remainingCount <= 16 ? correctionPositions
                        : Bmi2.X64.ParallelBitDeposit((1UL << count) - 1, correctionPositions);
                    if (!TryReadRefinementField(input, ref cursor, ref cache, ref pending, count, out var rawField))
                    {
                        // A marker/truncation inside the field retains every
                        // preceding correction and per-bit work charge.
                        for (var scalar = selected; scalar != 0; scalar &= scalar - 1)
                            if (ReadRefinementBits(input, ref cursor, ref cache, ref pending, 1, ref work) != 0)
                                JpegDecoder.ApplyCorrection(ref block[BitOperations.TrailingZeroCount(scalar)], increment);
                        correctionPositions &= ~selected;
                        continue;
                    }
                    work += count;
                    var field = (uint)rawField;
                    if (field != 0)
                    {
                        uint reversed;
                        if (count <= 4)
                        {
                            // Nibble v contains the reversal of v's four bits.
                            // This immediate is generated by that bit geometry.
                            reversed = (uint)((0xF7B3D591E6A2C480UL >> ((int)field * 4)) & 15) >> (4 - count);
                        }
                        else
                        {
                            reversed = field << (16 - count);
                            reversed = ((reversed & 0x5555u) << 1) | ((reversed >> 1) & 0x5555u);
                            reversed = ((reversed & 0x3333u) << 2) | ((reversed >> 2) & 0x3333u);
                            reversed = ((reversed & 0x0F0Fu) << 4) | ((reversed >> 4) & 0x0F0Fu);
                            reversed = ((reversed & 255u) << 8) | (reversed >> 8);
                        }
                        var corrections = Bmi2.X64.ParallelBitDeposit(reversed, selected);
                        while (corrections != 0)
                        {
                            ref var value = ref block[BitOperations.TrailingZeroCount(corrections)];
                            JpegDecoder.ApplyCorrection(ref value, increment);
                            corrections &= corrections - 1;
                        }
                    }
                    correctionPositions &= ~selected;
                }
                if (finish) { eobRun--; return true; }
                run -= BitOperations.PopCount(zeroPrefix); index = stop;
                if (run != 0 || (inserted != 0 && index > end))
                    throw new ImageDataException("JPEG refinement run exceeds band.");
                if (inserted != 0) { block[index] = inserted; nonzero |= 1UL << index; index++; }
            }
            return true;
        }
        finally
        {
            bits = cache; available = pending; position = cursor;
            context.CommitAdmittedWork(work);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryReadRefinementField(ReadOnlySpan<byte> input, ref int cursor,
        ref uint cache, ref int pending, int count, out int value)
    {
        value = 0;
        var candidateCursor = cursor; var candidateCache = cache; var candidatePending = pending;
        while (candidatePending < count)
        {
            if (candidateCursor >= input.Length) return false;
            var next = input[candidateCursor++];
            if (next == 255 && (candidateCursor >= input.Length || input[candidateCursor++] != 0)) return false;
            candidateCache = candidateCache << 8 | next; candidatePending += 8;
        }
        pending = candidatePending - count; cache = candidateCache; cursor = candidateCursor;
        value = (int)((cache >> pending) & ((1u << count) - 1));
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ReadRefinementBits(ReadOnlySpan<byte> input, ref int cursor,
        ref uint cache, ref int pending, int count, ref int work)
    {
        work++;
        while (pending < count)
        {
            if (cursor >= input.Length) throw new ImageDataException("Truncated JPEG entropy data.");
            var next = input[cursor++];
            if (next == 255 && (cursor >= input.Length || input[cursor++] != 0))
                throw new ImageDataException("Unexpected marker in JPEG entropy data.");
            cache = cache << 8 | next; pending += 8;
        }
        pending -= count;
        return (int)((cache >> pending) & ((1u << count) - 1));
    }

    internal bool TryReadHuffman(ReadOnlySpan<ushort> shortCodes, out int symbol)
    {
        symbol = 0;
        var candidateBits = bits; var candidateAvailable = available; var candidatePosition = position;
        // Complete cached short codes need no additional input inspection.
        var prefix = (int)((bits & ((1u << available) - 1)) << (8 - available));
        var packed = shortCodes[prefix]; var count = packed & 15;
        if (count == 0 || count > available)
        {
            // Inspect at most one unstuffed byte without changing reader state
            // or raising format errors. Markers/truncation use the scalar path.
            if (candidatePosition >= data.Length) return false;
            var next = data[candidatePosition++];
            if (next == 255 && (candidatePosition >= data.Length || data[candidatePosition++] != 0)) return false;
            candidateBits = candidateBits << 8 | next; candidateAvailable += 8;
            prefix = (int)((candidateBits >> (candidateAvailable - 8)) & 255);
            packed = shortCodes[prefix]; count = packed & 15;
            if (count == 0) return false;
        }
        // Preserve exact logical work. Near the limit, fall back without any
        // mutation to retain the original per-bit failure position/precedence.
        if (!TryWork(count)) return false;
        bits = candidateBits; available = candidateAvailable - count; position = candidatePosition;
        symbol = packed >> 4; return true;
    }

    internal bool TryReadBaselineAc(ReadOnlySpan<ushort> shortCodes, int remainingCoefficients, out int symbol, out int value)
    {
        symbol = value = 0;
        // T.81 F.2.2: a valid nonzero AC symbol is immediately followed by its
        // amplitude. Inspect their required bytes together and publish the bit
        // cache once. Only the noncancelable baseline local-work scope qualifies.
        if (remainingWork < 0) return false;
        var candidateBits = bits; var candidateAvailable = available; var candidatePosition = position;
        var prefix = (int)((bits & ((1u << available) - 1)) << (8 - available));
        var packed = shortCodes[prefix]; var count = packed & 15;
        if (count == 0 || count > available)
        {
            if (candidatePosition >= data.Length) return false;
            var next = data[candidatePosition++];
            if (next == 255 && (candidatePosition >= data.Length || data[candidatePosition++] != 0)) return false;
            candidateBits = candidateBits << 8 | next; candidateAvailable += 8;
            prefix = (int)((candidateBits >> (candidateAvailable - 8)) & 255);
            packed = shortCodes[prefix]; count = packed & 15;
            if (count == 0) return false;
        }
        var token = packed >> 4; var size = token & 15;
        // These syntax failures precede amplitude admission. EOB/zero runs,
        // long codes and every speculative failure retain the original path.
        if (size is < 1 or > 10 || token >> 4 >= remainingCoefficients) return false;
        candidateAvailable -= count;
        while (candidateAvailable < size)
        {
            if (candidatePosition >= data.Length) return false;
            var next = data[candidatePosition++];
            if (next == 255 && (candidatePosition >= data.Length || data[candidatePosition++] != 0)) return false;
            candidateBits = candidateBits << 8 | next; candidateAvailable += 8;
        }
        // Prefix bits cost count, and ReadBits(size) costs one. If the combined
        // charge cannot fit, the original sequence preserves partial admission.
        if (!TryWork(count + 1)) return false;
        bits = candidateBits; available = candidateAvailable - size; position = candidatePosition;
        var amplitude = (int)((bits >> available) & ((1u << size) - 1));
        var threshold = 1 << (size - 1);
        value = amplitude + (((amplitude - threshold) >> 31) & (1 - (threshold << 1)));
        symbol = token; return true;
    }

    internal void ReadPlainBaselineAc(ReadOnlySpan<ushort> shortCodes, scoped Span<int> block, ref int coefficient)
    {
        var budget = remainingWork;
        if (budget < 0) return;
        ulong localBits = bits; var localAvailable = available; var localPosition = position; var k = coefficient;
        try
        {
            while (k <= 63)
            {
                // Eight prefix bits plus at most ten amplitude bits. Reuse
                // unused lookahead across symbols instead of refilling each one.
                if (localAvailable < 18)
                {
                    if (data.Length - localPosition < 4) return;
                    var word = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(data.Slice(localPosition, 4));
                    if ((unchecked(~word - 0x01010101u) & word & 0x80808080u) != 0) return;
                    localBits = localBits << 32 | word; localAvailable += 32; localPosition += 4;
                }
                var packed = shortCodes[(int)((localBits >> (localAvailable - 8)) & 255)]; var count = packed & 15;
                var token = packed >> 4; var size = token & 15; var run = token >> 4;
                if (count == 0) return;
                if (token == 0)
                {
                    // EOB has no amplitude. Complete it here without probing
                    // the same prefix again through the scalar wrappers.
                    if (count > budget) return;
                    budget -= count; localAvailable -= count; k = 64; return;
                }
                if (size is < 1 or > 10 || run >= 64 - k) return;
                // Preserve the existing atomic fused prefix/amplitude charge;
                // insufficient work falls back before any speculative mutation.
                if (count + 1 > budget) return;
                budget -= count + 1;
                localAvailable -= count + size;
                var amplitude = (int)((localBits >> localAvailable) & ((1u << size) - 1)); var threshold = 1 << (size - 1);
                k += run;
                block[JpegTransform.Zigzag[k++]] = amplitude + (((amplitude - threshold) >> 31) & (1 - (threshold << 1)));
            }
        }
        finally
        {
            // Speculative whole bytes are not consumed. Publish the original
            // reader's minimum byte position and only its pending fractional bits.
            var wholeBytes = localAvailable >> 3;
            bits = unchecked((uint)(localBits >> (wholeBytes * 8))); available = localAvailable & 7;
            position = localPosition - wholeBytes; remainingWork = budget; coefficient = k;
        }
    }

    internal void ReadPlainCancelableAc(ReadOnlySpan<ushort> shortCodes, scoped Span<int> block, ref int coefficient)
    {
        ulong localBits = bits; var localAvailable = available; var localPosition = position;
        var budget = ~remainingWork; var cancellation = context.Cancellation; var k = coefficient;
        try
        {
            while (k <= 63)
            {
                // At most eight prefix and ten amplitude bits are needed.
                // Keep ordinary lookahead across symbols; every exit returns
                // unused whole bytes before publishing the reader state.
                if (localAvailable < 18)
                {
                    if (data.Length - localPosition < 4) return;
                    var word = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(data.Slice(localPosition, 4));
                    if ((unchecked(~word - 0x01010101u) & word & 0x80808080u) != 0) return;
                    localBits = localBits << 32 | word; localAvailable += 32; localPosition += 4;
                }
                var packed = shortCodes[(int)((localBits >> (localAvailable - 8)) & 255)]; var count = packed & 15;
                var token = packed >> 4; var size = token & 15; var run = token >> 4;
                if (count == 0 || size is < 1 or > 10 || run >= 64 - k) return;
                cancellation.ThrowIfCancellationRequested();
                if (count > budget) return;
                budget -= count;
                localAvailable -= count;
                // Admit and expose the prefix before the separate amplitude
                // checkpoint. The finally preserves it on cancellation/limit.
                if (cancellation.IsCancellationRequested || budget == 0)
                {
                    cancellation.ThrowIfCancellationRequested();
                    throw new ImageLimitException(ImageLimitKind.Work, "Computational work limit exceeded.");
                }
                budget--;
                localAvailable -= size;
                var amplitude = (int)((localBits >> localAvailable) & ((1u << size) - 1)); var threshold = 1 << (size - 1);
                k += run;
                block[JpegTransform.Zigzag[k++]] = amplitude + (((amplitude - threshold) >> 31) & (1 - (threshold << 1)));
            }
        }
        finally
        {
            var wholeBytes = localAvailable >> 3;
            bits = unchecked((uint)(localBits >> (wholeBytes * 8))); available = localAvailable & 7;
            position = localPosition - wholeBytes; remainingWork = ~budget; coefficient = k;
        }
    }

    internal bool TryReadLongHuffman(ReadOnlySpan<ushort> longCodes, out int symbol)
    {
        symbol = 0;
        var candidateBits = bits; var candidateAvailable = available; var candidatePosition = position;
        while (true)
        {
            var prefix = candidateAvailable >= 10 ? (int)((candidateBits >> (candidateAvailable - 10)) & 1023)
                : (int)((candidateBits & ((1u << candidateAvailable) - 1)) << (10 - candidateAvailable));
            var packed = longCodes[prefix]; var count = packed & 15;
            if (count != 0 && count <= candidateAvailable)
            {
                if (!TryWork(count)) return false;
                bits = candidateBits; available = candidateAvailable - count; position = candidatePosition;
                symbol = packed >> 4; return true;
            }
            if (candidateAvailable >= 10 || candidatePosition >= data.Length) return false;
            // Inspect only while the complete code is unavailable. Never
            // commit bytes beyond its last required byte, even at padding.
            var next = data[candidatePosition++];
            if (next == 255 && (candidatePosition >= data.Length || data[candidatePosition++] != 0)) return false;
            candidateBits = candidateBits << 8 | next; candidateAvailable += 8;
        }
    }

    internal bool TryReadCanonicalPrefix(ReadOnlySpan<ushort> shortCodes, out int prefix)
    {
        prefix = 0;
        var candidateBits = bits; var candidateAvailable = available; var candidatePosition = position;
        if (candidateAvailable < 8)
        {
            if (candidatePosition >= data.Length) return false;
            var next = data[candidatePosition++];
            if (next == 255 && (candidatePosition >= data.Length || data[candidatePosition++] != 0)) return false;
            candidateBits = candidateBits << 8 | next; candidateAvailable += 8;
        }
        var value = (int)((candidateBits >> (candidateAvailable - 8)) & 255);
        // T.81 C.2: a complete prefix with no assigned code of length <=8
        // can start canonical traversal at length nine. Admit the same eight
        // logical bits together; tight work budgets, markers and truncation
        // retain the original one-bit traversal without any state mutation.
        if (shortCodes[value] != 0 || !TryWork(8)) return false;
        bits = candidateBits; available = candidateAvailable - 8; position = candidatePosition;
        prefix = value; return true;
    }

    public void Restart(int expected)
    {
        if ((uint)expected > 7) throw new ArgumentOutOfRangeException(nameof(expected));
        Align();
        if (ReadMarker() != 0xD0 + expected) throw new ImageDataException("Unexpected JPEG restart marker sequence.");
    }

    public void Align()
    {
        // T.81 pads entropy segments with 1 bits. ReadBits never speculates past
        // the next required byte, so all remaining bits belong to that byte.
        if ((bits & ((1u << available) - 1)) != (1u << available) - 1)
            throw new ImageDataException("Invalid JPEG entropy padding.");
        bits = 0; available = 0;
    }

    private int ReadMarker()
    {
        if (position >= data.Length || data[position++] != 255) throw new ImageDataException("Missing JPEG restart marker.");
        while (position < data.Length && data[position] == 255) { context.Work(1); position++; }
        if (position >= data.Length) throw new ImageDataException("Truncated JPEG restart marker.");
        return data[position++];
    }
}
