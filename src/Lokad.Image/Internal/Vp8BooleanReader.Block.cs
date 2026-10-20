using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Lokad.Image.Internal;

internal partial struct Vp8BooleanReader
{
    // RFC6386 sections7.2/13.2: at most7token,11category and1sign decisions
    // for each of16coefficients. Each decision consumes at most7input bits.
    // 272partition bytes cover266consumed bytes,3lookahead bytes and slack.
    // Selection is before work; short inputs, tight work and live cancellation
    // retain the original bit-at-a-time reader and its partial failure state.
    internal bool CanReadCoefficientBlock(ReadOnlySpan<byte> data, OperationContext context)
        => !context.Cancellation.CanBeCanceled && context.RemainingWork >= 304
        && range is >= 128 and <= 255 && (uint)value < (uint)range && (uint)shift <= 7
        && end <= data.Length && end - position >= 272;

    // The decoder supplies its complete private model,16sample block and valid
    // plane/context. The selected input/work bounds exclude mid-block failures.
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
        context.CommitAdmittedWork(reader.Work);
        // The buffered word contains the original8bit value followed by Bits
        // future bits. Discarding lookahead restores the exact public cursor.
        position = reader.Position - (reader.Bits + 7) / 8;
        shift = (reader.Bits - 1) & 7; range = reader.Range; value = (int)(reader.Value >> reader.Bits);
        return nonzero != 0;
    }

    private ref struct BlockReader
    {
        private readonly ReadOnlySpan<byte> data;
        internal int Position, Bits, Range, Work;
        internal uint Value;

        internal BlockReader(ReadOnlySpan<byte> data, int position, int shift, int range, int value)
        {
            this.data = data; Position = position + 1; Bits = shift + 1; Range = range; Work = 0;
            Value = ((uint)value << Bits) | ((uint)data[position] & ((1u << Bits) - 1));
            Fill();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int Read(int probability)
        {
            Work++;
            var boundary = 1 + ((Range - 1) * probability >> 8);
            var bit = Value >> Bits >= (uint)boundary ? 1 : 0; var mask = -bit;
            Value -= (uint)(boundary & mask) << Bits;
            Range = boundary + ((Range - 2 * boundary) & mask);
            var count = BitOperations.LeadingZeroCount((uint)Range) - 24;
            Range <<= count; Bits -= count; Fill(); return bit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Fill()
        {
            // Before a decision,8<=Bits<=24 and Value<Range*2^Bits<2^32.
            // Normalization removes at most7bits. A16bit refill when Bits<8
            // returns17..23future bits and cannot overflow the32bit word.
            if (Bits >= 8) return;
            Value = Value << 16 | BinaryPrimitives.ReadUInt16BigEndian(data.Slice(Position, 2));
            Position += 2; Bits += 16;
        }

        internal int Token(ReadOnlySpan<byte> p, bool previousZero)
        {
            if (!previousZero && Read(p[0]) == 0) return 11;
            if (Read(p[1]) == 0) return 0;
            if (Read(p[2]) == 0) return 1;
            if (Read(p[3]) == 0) return Read(p[4]) == 0 ? 2 : 3 + Read(p[5]);
            if (Read(p[6]) == 0) return 5 + Read(p[7]);
            var upper = Read(p[8]); return 7 + 2 * upper + Read(p[9 + upper]);
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
