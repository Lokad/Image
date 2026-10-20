namespace Lokad.Image.Internal;

internal static partial class WebpLosslessEncoder
{
    internal sealed partial class AlphaEncoding
    {
        private const int SingleHistorySize = 16384;
        private const int SingleHistoryBytes = SingleHistorySize * 4 + 32;
        private byte[]? stagedAlpha;
        private long singlePassBytes;

        // RFC 9649 sections 3.6/3.7. Recent positions and optional token-start
        // anchors, bounded prefix trees and overlapping copies. Anchors use a
        // four-residual key to distinguish recurring three-sample prefixes.
        // A rolling window avoids allocating a filtered alpha plane. Compute
        // the old run-only cost during the same traversal, then compare padded
        // sizes. Discarding a completed larger candidate is not limit recovery.
        private void PrepareSinglePass(ReadOnlySpan<byte> alpha, int width, bool adaptiveTokens, bool useAnchors)
        {
            var capacity = (adaptiveTokens ? 4L : 1L) * alpha.Length + 32;
            var historyBytes = (useAnchors ? 2L : 1L) * SingleHistoryBytes;
            var storage = historyBytes + capacity;
            context.Reserve(storage); singlePassBytes = storage; context.Work(0);
            var entries = new int[SingleHistorySize];
            var anchors = useAnchors ? new int[SingleHistorySize] : null;
            var tokens = adaptiveTokens ? new uint[alpha.Length] : null; var tokenCount = 0;
            if (!adaptiveTokens) stagedAlpha = new byte[alpha.Length];
            Span<int> frequencies = adaptiveTokens ? stackalloc int[280] : default;
            Span<int> distanceFrequencies = adaptiveTokens ? stackalloc int[40] : default;
            frequencies.Clear(); distanceFrequencies.Clear();
            context.Work(SingleHistorySize); entries.AsSpan().Fill(-1);
            if (anchors is not null) { context.Work(SingleHistorySize); anchors.AsSpan().Fill(-1); }
            using var fixedDestination = adaptiveTokens ? null : new AlphaStagingStream(stagedAlpha!);
            var writer = adaptiveTokens ? null : new WebpBitWriter(fixedDestination!, context);
            var distances = adaptiveTokens ? null : new Table(40);
            if (writer is not null)
            {
                writer.Write(0, 1); writer.Write(0, 1); writer.Write(0, 1);
                green.Header(writer); Singleton(writer, 0); Singleton(writer, 0); Singleton(writer, 0);
                distances!.Header(writer);
            }

            // Three flags, the fixed 280-leaf green header (325 bits), three
            // zero-channel singletons and the previous-pixel distance singleton.
            long originalBits = 3 + 325 + 3 * 4 + 4;
            var runValue = -1; var runLength = 0; var position = 0;
            var keyLength = useAnchors ? 4 : 3;
            context.Work(keyLength);
            var first = Residual(alpha, width, 0); var second = Residual(alpha, width, 1);
            var third = Residual(alpha, width, 2);
            var fourth = useAnchors ? Residual(alpha, width, 3) : 0;
            while (position < alpha.Length)
            {
                var length = 0; var distance = 0; var maximum = Math.Min(4096, alpha.Length - position);
                if (maximum >= keyLength)
                {
                    context.Work(1);
                    var hash = useAnchors ? SingleHash(first, second, third, fourth) : SingleHash(first, second, third);
                    var previous = entries[hash];
                    if (previous >= 0 && position - previous <= 1_048_456)
                    {
                        while (length < maximum)
                        {
                            context.Work(1);
                            if (Residual(alpha, width, previous + length) != Residual(alpha, width, position + length)) break;
                            length++;
                        }
                        distance = position - previous;
                    }
                    if (anchors is not null)
                    {
                        if (length < maximum)
                        {
                            context.Work(1); var anchor = anchors[hash];
                            if (anchor >= 0 && anchor != previous && position - anchor <= 1_048_456)
                            {
                                // An older token start can outlive newer entries
                                // inserted inside a copy. Reject a candidate at
                                // the current best boundary before rechecking
                                // its entire prefix; hash collisions stay exact.
                                context.Work(1);
                                if (Residual(alpha, width, anchor + length) == Residual(alpha, width, position + length))
                                {
                                    var matched = 0;
                                    while (matched < maximum)
                                    {
                                        context.Work(1);
                                        if (Residual(alpha, width, anchor + matched) != Residual(alpha, width, position + matched)) break;
                                        matched++;
                                    }
                                    if (matched > length) { length = matched; distance = position - anchor; }
                                }
                            }
                        }
                        context.Work(1); anchors[hash] = position;
                    }
                }
                var consumed = 1;
                if (length >= 3)
                {
                    Prefix(length, out var code, out var extra, out var bits);
                    if (tokens is not null)
                    {
                        context.Work(1); // Collect/classify one reusable token.
                        tokens[tokenCount++] = (uint)(distance - 1) << 12 | (uint)(length - 2);
                        frequencies[256 + code]++;
                        Distance(distance, width, out code, out _, out _); distanceFrequencies[code]++;
                    }
                    else
                    {
                        green.Symbol(writer!, 256 + code, extra, bits);
                        Distance(distance, width, out code, out extra, out bits);
                        distances!.Symbol(writer!, code, extra, bits);
                    }
                    consumed = length;
                }
                else if (tokens is not null)
                {
                    context.Work(1); tokens[tokenCount++] = (uint)first << 12; frequencies[first]++;
                }
                else green.Symbol(writer!, first);
                for (var i = 0; i < consumed; i++)
                {
                    context.Work(1); // Concurrent run-only candidate accounting.
                    if (first == runValue) runLength++;
                    else
                    {
                        if (runLength != 0) originalBits += RunBits(runValue, runLength);
                        runValue = first; runLength = 1;
                    }
                    if (position + i + keyLength - 1 < alpha.Length)
                    {
                        context.Work(1);
                        var hash = useAnchors ? SingleHash(first, second, third, fourth) : SingleHash(first, second, third);
                        entries[hash] = position + i;
                    }
                    first = second; second = third;
                    var next = 0;
                    if (position + i + keyLength < alpha.Length)
                    {
                        context.Work(1); next = Residual(alpha, width, position + i + keyLength);
                    }
                    if (useAnchors) { third = fourth; fourth = next; }
                    else third = next;
                }
                position += consumed;
            }
            originalBits += RunBits(runValue, runLength); writer?.Finish();
            context.Release(historyBytes); singlePassBytes -= historyBytes;
            using var adaptiveDestination = tokens is null ? null
                : StageTokens(tokens, tokenCount, frequencies, distanceFrequencies, alpha.Length, width);
            if (tokens is not null) { context.Release(capacity); singlePassBytes -= capacity; }
            var destination = adaptiveDestination ?? fixedDestination!;
            var original = checked((int)((originalBits + 7) / 8));
            var candidate = destination.Count + 1; var old = original + 1;
            if (!destination.Overflow && candidate + (candidate & 1) < old + (old & 1)) Length = destination.Count;
            else
            {
                stagedAlpha = null; context.Release(singlePassBytes); singlePassBytes = 0; Length = original;
            }
        }

        // RFC9649 exact references are collected once. Replaying compact token
        // records fits adaptive prefix codes without another matching/input pass.
        private AlphaStagingStream StageTokens(uint[] tokens, int count, ReadOnlySpan<int> frequencies,
            ReadOnlySpan<int> distances, int capacity, int width)
        {
            adaptiveGreen = new WebpPrefixCode(frequencies, context);
            adaptiveDistance = new WebpPrefixCode(distances, context);
            context.Reserve(capacity + 32L); singlePassBytes += capacity + 32L; context.Work(0);
            stagedAlpha = new byte[capacity]; var destination = new AlphaStagingStream(stagedAlpha);
            var writer = new WebpBitWriter(destination, context);
            writer.Write(0, 1); writer.Write(0, 1); writer.Write(0, 1);
            adaptiveGreen.Header(writer, context); Singleton(writer, 0); Singleton(writer, 0); Singleton(writer, 0);
            adaptiveDistance.Header(writer, context);
            for (var i = 0; i < count; i++)
            {
                var token = tokens[i]; var length = (int)(token & 4095);
                ulong value; int bits;
                if (length == 0)
                {
                    var literal = (int)(token >> 12); bits = adaptiveGreen.BitLength(literal);
                    value = (uint)adaptiveGreen.Code(literal);
                }
                else
                {
                    Prefix(length + 2, out var code, out var extra, out var extraBits);
                    bits = adaptiveGreen.BitLength(256 + code);
                    value = (uint)adaptiveGreen.Code(256 + code) | (ulong)(uint)extra << bits; bits += extraBits;
                    Distance((int)(token >> 12) + 1, width, out code, out extra, out extraBits);
                    var distanceBits = adaptiveDistance.BitLength(code);
                    var distanceValue = (uint)adaptiveDistance.Code(code) | (ulong)(uint)extra << distanceBits;
                    value |= distanceValue << bits; bits += distanceBits + extraBits;
                }
                // Two prefix+extra fields total at most58 bits. Physical writer
                // calls carry at most24 bits and retain their actual work charge.
                while (bits > 24) { writer.Write((int)(value & 0xFFFFFF), 24); value >>= 24; bits -= 24; }
                writer.Write((int)value, bits);
            }
            writer.Finish();
            context.Release(adaptiveGreen.MemoryBytes); adaptiveGreen = null;
            context.Release(adaptiveDistance.MemoryBytes); adaptiveDistance = null;
            return destination;
        }

        private static int SingleHash(int first, int second, int third) => ((first * 251 + second) * 251 + third) & 16383;

        // Four byte terms fit UInt32, but can exceed Int32 in the checked build.
        // Mask before conversion; hash collisions still require exact matching.
        private static int SingleHash(int first, int second, int third, int fourth)
            => (int)(((((uint)first * 251 + (uint)second) * 251 + (uint)third) * 251 + (uint)fourth) & 16383);

        private long RunBits(int value, int length)
        {
            long bits = green.Bits(value); var remaining = length - 1;
            while (remaining != 0)
            {
                var count = Math.Min(4096, remaining);
                if (count == 1) bits += green.Bits(value);
                else { Prefix(count, out _, out _, out var extra); bits += 9 + extra; }
                remaining -= count;
            }
            return bits;
        }

        // A candidate may exceed its raw-sized staging capacity. Keep counting
        // without storing the excess; this is an ordinary no-gain result, not
        // an admission/allocation/work exception converted to a fallback.
        private sealed class AlphaStagingStream(byte[] storage) : Stream
        {
            public int Count { get; private set; }
            public bool Overflow => Count > storage.Length;
            public override bool CanRead => false;
            public override bool CanSeek => false;
            public override bool CanWrite => true;
            public override long Length => Count;
            public override long Position { get => Count; set => throw new NotSupportedException(); }
            public override void Flush() { }
            public override void WriteByte(byte value)
            {
                if (Count < storage.Length) storage[Count] = value;
                Count++;
            }
            public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));
            public override void Write(ReadOnlySpan<byte> buffer) { foreach (var value in buffer) WriteByte(value); }
            public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
        }
    }
}
