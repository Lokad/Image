using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Lokad.Image.Internal;

internal partial struct Vp8BooleanReader
{
    // RFC6386 section13.2 stores eleven probabilities per coefficient tree.
    // Borrow rows from the complete private model; no copy or separate owner.
    [InlineArray(11)]
    internal struct CoefficientModelRow
    {
        private byte first;
    }

    internal bool CanReadCancellableCoefficientBlock(ReadOnlySpan<byte> data, OperationContext context)
        => context.Cancellation.CanBeCanceled
        && range is >= 128 and <= 255 && (uint)value < (uint)range && (uint)shift <= 7
        && end <= data.Length && end - position >= 272;

    // Same RFC6386 block/input bound as the noncancelable window. Preserve
    // its arithmetic unchanged, but poll before EVERY decision and publish the
    // exact completed prefix even when that poll throws. No new storage/owner.
    internal bool ReadCancellableCoefficientBlock(ReadOnlySpan<byte> data, ReadOnlySpan<byte> probabilities,
        Span<short> values, int plane, int initialContext, int dc, int ac, OperationContext context, out byte nonzero)
    {
        nonzero = 0;
        context.Cancellation.ThrowIfCancellationRequested();
        var reader = new CancellableBlockReader(data, this, context.Cancellation);
        try
        {
            var model = MemoryMarshal.Cast<byte, CoefficientModelRow>(probabilities);
            ReadOnlySpan<byte> bands = [0, 1, 2, 3, 6, 4, 5, 6, 6, 6, 6, 6, 6, 6, 6, 7];
            ReadOnlySpan<byte> zigzag = [0, 1, 4, 8, 5, 2, 3, 6, 9, 12, 13, 10, 7, 11, 14, 15];
            var previousZero = false; var predictor = initialContext;
            for (var c = plane == 0 ? 1 : 0; c < 16; c++)
            {
                ref readonly var p = ref model[(plane * 8 + bands[c]) * 3 + predictor];
                var token = reader.Token(p, previousZero);
                if (token == 11) break;
                var sample = token < 5 ? token : reader.Category(token - 5);
                previousZero = sample == 0; predictor = Math.Min(sample, 2);
                if (sample == 0) continue;
                nonzero = 1;
                if (reader.Read(128) != 0) sample = -sample;
                values[zigzag[c]] = unchecked((short)(sample * (c == 0 ? dc : ac)));
            }
            return nonzero != 0;
        }
        finally { reader.Publish(ref this); }
    }

    internal ref struct CancellableBlockReader
    {
        private readonly ReadOnlySpan<byte> data;
        private int position, bits, range;
        private ulong value;
        private readonly CancellationToken cancellation;

        internal CancellableBlockReader(ReadOnlySpan<byte> data, Vp8BooleanReader initial, CancellationToken cancellation)
        {
            this.data = data;
            position = initial.position + 1; bits = initial.shift + 1; range = initial.range;
            value = ((ulong)initial.value << bits) | ((uint)data[initial.position] & ((1u << bits) - 1));
            this.cancellation = cancellation;
            Fill();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int Read(int probability)
        {
            cancellation.ThrowIfCancellationRequested();
            Normalize();
            var boundary = 1 + ((range - 1) * probability >> 8);
            var result = value >= ((ulong)boundary << bits) ? 1 : 0; var mask = -result;
            value -= (ulong)(boundary & mask) << bits;
            range = boundary + ((range - 2 * boundary) & mask);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int TokenRead(int probability)
        {
            cancellation.ThrowIfCancellationRequested();
            Normalize();
            var boundary = 1 + ((range - 1) * probability >> 8);
            var split = (ulong)boundary << bits;
            if (value < split) { range = boundary; return 0; }
            range -= boundary; value -= split; return 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Normalize()
        {
            // RFC6386 section7.2: the code's binary point moves with the
            // interval. Keep the reservoir fixed instead of shifting it too.
            var count = BitOperations.LeadingZeroCount((uint)range) - 24;
            range <<= count; bits -= count; Fill();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Fill()
        {
            // At most7 low bits plus8 interval bits precede a32-bit append,
            // so the complete reservoir fits in47bits. The admitted block's
            // 272-byte bound includes this four-byte lookahead.
            if (bits >= 8) return;
            value = (value << 32) | BinaryPrimitives.ReadUInt32BigEndian(data.Slice(position, 4));
            position += 4; bits += 32;
        }

        internal void Publish(ref Vp8BooleanReader target)
        {
            // A canceled poll does not normalize or consume input. Publish
            // only the completed prefix, without another read or poll.
            var count = BitOperations.LeadingZeroCount((uint)range) - 24;
            var remaining = bits - count;
            target.position = position - (remaining + 7) / 8;
            target.shift = (remaining - 1) & 7;
            target.range = range << count;
            target.value = (int)(value >> remaining);
        }

        // RFC6386 section13.2, preserving the original probability/read order.
        internal int Token(in CoefficientModelRow p, bool previousZero)
        {
            if (!previousZero && TokenRead(p[0]) == 0) return 11;
            if (TokenRead(p[1]) == 0) return 0;
            if (TokenRead(p[2]) == 0) return 1;
            if (TokenRead(p[3]) == 0) return TokenRead(p[4]) == 0 ? 2 : 3 + TokenRead(p[5]);
            if (TokenRead(p[6]) == 0) return 5 + TokenRead(p[7]);
            var upper = TokenRead(p[8]); return 7 + 2 * upper + TokenRead(p[9 + upper]);
        }

        internal int Category(int category)
        {
            ReadOnlySpan<byte> p = category switch
            {
                0 => [159], 1 => [165, 145], 2 => [173, 148, 140],
                3 => [176, 155, 140, 135], 4 => [180, 157, 141, 134, 130],
                _ => [254, 254, 243, 230, 196, 177, 153, 140, 133, 130, 129]
            };
            ReadOnlySpan<int> bases = [5, 7, 11, 19, 35, 67]; var value = 0;
            foreach (var probability in p) value = value * 2 + Read(probability);
            return bases[category] + value;
        }
    }
}
