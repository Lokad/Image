using System.Runtime.CompilerServices;

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
            if (!TryCancelableWork(units)) throw new ImageLimitException("Computational work limit exceeded.");
            return;
        }
        if (units < 0 || units > remainingWork) throw new ImageLimitException("Computational work limit exceeded.");
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
                if (count == 0 || size is < 1 or > 10 || run >= 64 - k) return;
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
        var localBits = bits; var localAvailable = available; var localPosition = position;
        var budget = ~remainingWork; var cancellation = context.Cancellation; var k = coefficient;
        try
        {
            while (k <= 63 && data.Length - localPosition >= 2)
            {
                var pair = System.Buffers.Binary.BinaryPrimitives.ReadUInt16BigEndian(data.Slice(localPosition, 2));
                // Stuffing, markers, long codes and syntax failures use the
                // original traversal, without speculative state changes.
                if ((unchecked((uint)(~pair - 0x0101)) & pair & 0x8080) != 0) return;
                var cached = localBits << 16 | pair; var cachedCount = localAvailable + 16;
                var packed = shortCodes[(int)((cached >> (cachedCount - 8)) & 255)]; var count = packed & 15;
                var token = packed >> 4; var size = token & 15; var run = token >> 4;
                if (count == 0 || size is < 1 or > 10 || run >= 64 - k || count + size > cachedCount) return;
                cancellation.ThrowIfCancellationRequested();
                if (count > budget) return;
                budget -= count;
                // Admit and expose the prefix before the separate amplitude
                // checkpoint. The finally preserves it on cancellation/limit.
                if (cancellation.IsCancellationRequested || budget == 0)
                {
                    var prefixBytes = (count + 7 - localAvailable) >> 3;
                    localBits = cached >> ((2 - prefixBytes) * 8);
                    localAvailable += prefixBytes * 8 - count; localPosition += prefixBytes;
                    cancellation.ThrowIfCancellationRequested();
                    throw new ImageLimitException("Computational work limit exceeded.");
                }
                budget--;
                var pending = cachedCount - count - size;
                localBits = cached >> (pending & ~7); localAvailable = pending & 7; localPosition += 2 - (pending >> 3);
                var amplitude = (int)((cached >> pending) & ((1u << size) - 1)); var threshold = 1 << (size - 1);
                k += run;
                block[JpegTransform.Zigzag[k++]] = amplitude + (((amplitude - threshold) >> 31) & (1 - (threshold << 1)));
            }
        }
        finally
        {
            bits = localBits; available = localAvailable; position = localPosition; remainingWork = ~budget; coefficient = k;
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
