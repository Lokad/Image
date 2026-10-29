using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Lokad.Image.Internal;

internal partial struct Vp8BooleanReader
{
    // RFC6386 sections7.2/13.2: at most7token,11category and1sign decisions
    // for each of16coefficients. Each decision consumes at most7input bits.
    // 272partition bytes cover266consumed bytes,3lookahead bytes and slack.
    // Short inputs retain the bit-at-a-time reader. Cancelable blocks use the
    // separate polling wrapper and preserve its exact canceled prefix.
    internal bool CanReadCoefficientBlock(ReadOnlySpan<byte> data, OperationContext context)
        => !context.Cancellation.CanBeCanceled
        && range is >= 128 and <= 255 && (uint)value < (uint)range && (uint)shift <= 7
        && end <= data.Length && end - position >= 272;

    // The decoder supplies its complete private model,16sample block and valid
    // plane/context. The selected input bounds exclude mid-block failures.
    // One small stack state lives inside the existing8192B coarse allowance.
    internal bool ReadCoefficientBlock(ReadOnlySpan<byte> data, ReadOnlySpan<byte> probabilities,
        Span<short> values, int plane, int initialContext, int dc, int ac, OperationContext context, out byte nonzero)
    {
        var reader = new BlockReader(data, position, shift, range, value);
        ReadOnlySpan<byte> bands = [0, 1, 2, 3, 6, 4, 5, 6, 6, 6, 6, 6, 6, 6, 6, 7];
        ReadOnlySpan<byte> zigzag = [0, 1, 4, 8, 5, 2, 3, 6, 9, 12, 13, 10, 7, 11, 14, 15];
        var previousZero = false; var predictor = initialContext; nonzero = 0;
        for (var c = plane == 0 ? 1 : 0; c < 16; c++)
        {
            var p = probabilities.Slice(((plane * 8 + bands[c]) * 3 + predictor) * 11, 11);
            var token = reader.Token(p, previousZero);
            if (token == 11) break;
            var sample = token < 5 ? token : reader.Category(token - 5);
            previousZero = sample == 0; predictor = Math.Min(sample, 2);
            if (sample == 0) continue;
            nonzero = 1;
            if (reader.Read(128) != 0) sample = -sample;
            values[zigzag[c]] = unchecked((short)(sample * (c == 0 ? dc : ac)));
        }
        reader.Publish(ref this);
        return nonzero != 0;
    }

    private ref struct BlockReader
    {
        private readonly ReadOnlySpan<byte> data;
        internal int Position, Bits, Range;
        internal uint Value;

        internal BlockReader(ReadOnlySpan<byte> data, int position, int shift, int range, int value)
        {
            this.data = data; Position = position + 1; Bits = shift + 1; Range = range;
            Value = ((uint)value << 24) | (((uint)data[position] & ((1u << Bits) - 1)) << (24 - Bits));
            Fill();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int Read(int probability)
        {
            Normalize();
            var boundary = 1 + ((Range - 1) * probability >> 8);
            var bit = Value >= ((uint)boundary << 24) ? 1 : 0; var mask = -bit;
            Value -= (uint)(boundary & mask) << 24;
            Range = boundary + ((Range - 2 * boundary) & mask);
            return bit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int TokenRead(int probability)
        {
            Normalize();
            var boundary = 1 + ((Range - 1) * probability >> 8);
            var split = (uint)boundary << 24;
            if (Value < split) { Range = boundary; return 0; }
            Range -= boundary; Value -= split; return 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Normalize()
        {
            var count = BitOperations.LeadingZeroCount((uint)Range) - 24;
            Range <<= count; Value <<= count; Bits -= count; Fill();
        }

        internal void Publish(ref Vp8BooleanReader target)
        {
            // The previous decision began with >=8 known future bits and needs
            // at most7 to normalize. Publish without reading canceled input.
            var count = BitOperations.LeadingZeroCount((uint)Range) - 24;
            var bits = Bits - count;
            target.position = Position - (bits + 7) / 8; target.shift = (bits - 1) & 7;
            target.range = Range << count; target.value = (int)((Value << count) >> 24);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Fill()
        {
            // After normalization, Value<Range*2^24<2^32 and the low24-Bits
            // bits are zero. A16bit refill fits that zero region and returns
            // 17..23known future bits, before the next decision.
            if (Bits >= 8) return;
            Value |= (uint)BinaryPrimitives.ReadUInt16BigEndian(data.Slice(Position, 2)) << (8 - Bits);
            Position += 2; Bits += 16;
        }

        internal int Token(ReadOnlySpan<byte> p, bool previousZero)
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
