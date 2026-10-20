namespace Lokad.Image.Internal;

internal static partial class WebpLosslessEncoder
{
    internal sealed partial class AlphaEncoding
    {
        private const int SingleHistorySize = 16384;
        private const int SingleHistoryBytes = SingleHistorySize * 4 + 32;
        private byte[]? stagedAlpha;
        private long singlePassBytes;

        // RFC 9649 sections 3.6/3.7. One recent position per three-residual
        // hash, fixed complete prefix trees and overlapping copies. A rolling
        // three-sample window avoids allocating a filtered alpha plane. Compute
        // the old run-only cost during the same traversal, then compare padded
        // sizes. Discarding a completed larger candidate is not limit recovery.
        private void PrepareSinglePass(ReadOnlySpan<byte> alpha, int width)
        {
            var storage = SingleHistoryBytes + alpha.Length + 32L;
            context.Reserve(storage); singlePassBytes = storage; context.Work(0);
            var entries = new int[SingleHistorySize];
            stagedAlpha = new byte[alpha.Length];
            context.Work(SingleHistorySize); entries.AsSpan().Fill(-1);
            using var destination = new AlphaStagingStream(stagedAlpha);
            var writer = new WebpBitWriter(destination, context); var distances = new Table(40);
            writer.Write(0, 1); writer.Write(0, 1); writer.Write(0, 1);
            green.Header(writer); Singleton(writer, 0); Singleton(writer, 0); Singleton(writer, 0);
            distances.Header(writer);

            // Three flags, the fixed 280-leaf green header (325 bits), three
            // zero-channel singletons and the previous-pixel distance singleton.
            long originalBits = 3 + 325 + 3 * 4 + 4;
            var runValue = -1; var runLength = 0; var position = 0;
            context.Work(3);
            var first = Residual(alpha, width, 0); var second = Residual(alpha, width, 1);
            var third = Residual(alpha, width, 2);
            while (position < alpha.Length)
            {
                var length = 0; var distance = 0; var maximum = Math.Min(4096, alpha.Length - position);
                if (maximum >= 3)
                {
                    context.Work(1); var previous = entries[SingleHash(first, second, third)];
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
                }
                var consumed = 1;
                if (length >= 3)
                {
                    Prefix(length, out var code, out var extra, out var bits);
                    green.Symbol(writer, 256 + code, extra, bits);
                    Distance(distance, width, out code, out extra, out bits);
                    distances.Symbol(writer, code, extra, bits);
                    consumed = length;
                }
                else green.Symbol(writer, first);
                for (var i = 0; i < consumed; i++)
                {
                    context.Work(1); // Concurrent run-only candidate accounting.
                    if (first == runValue) runLength++;
                    else
                    {
                        if (runLength != 0) originalBits += RunBits(runValue, runLength);
                        runValue = first; runLength = 1;
                    }
                    if (position + i + 2 < alpha.Length)
                    {
                        context.Work(1); entries[SingleHash(first, second, third)] = position + i;
                    }
                    first = second; second = third;
                    if (position + i + 3 < alpha.Length)
                    {
                        context.Work(1); third = Residual(alpha, width, position + i + 3);
                    }
                    else third = 0;
                }
                position += consumed;
            }
            originalBits += RunBits(runValue, runLength); writer.Finish();
            context.Release(SingleHistoryBytes); singlePassBytes -= SingleHistoryBytes;
            var original = checked((int)((originalBits + 7) / 8));
            var candidate = destination.Count + 1; var old = original + 1;
            if (!destination.Overflow && candidate + (candidate & 1) < old + (old & 1)) Length = destination.Count;
            else
            {
                stagedAlpha = null; context.Release(singlePassBytes); singlePassBytes = 0; Length = original;
            }
        }

        private static int SingleHash(int first, int second, int third) => ((first * 251 + second) * 251 + third) & 16383;

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
