using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Lokad.Image.Internal;

internal partial struct Vp8BooleanReader
{
    // Codec-local value types select storage at entry. Both modes consume
    // identical token/category/sign decisions and publish the same cursor.
    internal readonly struct PixelMode { }
    internal readonly struct ValidationMode { }

    // RFC6386 section13.2 assigns bands to coefficient positions. Prepare their
    // three contexts once, instead of mapping every coefficient in every block.
    // The header's96 encoded rows occupy the beginning of this192-row array.
    internal static void ExpandCoefficientModel(Span<CoefficientModelRow> rows)
    {
        ReadOnlySpan<byte> bands = [0, 1, 2, 3, 6, 4, 5, 6, 6, 6, 6, 6, 6, 6, 6, 7];
        Span<CoefficientModelRow> source = stackalloc CoefficientModelRow[24];
        // Descending planes protect still-unread encoded rows. Within a plane,
        // band order is not monotone, so preserve its384bytes before expansion.
        for (var plane = 3; plane >= 0; plane--)
        {
            rows.Slice(plane * 24, 24).CopyTo(source);
            for (var coefficient = 0; coefficient < 16; coefficient++)
                source.Slice(bands[coefficient] * 3, 3)
                    .CopyTo(rows.Slice((plane * 16 + coefficient) * 3, 3));
        }
    }

    // RFC6386 sections7.2/13.2: at most7token,11category and1sign decisions
    // for each of16coefficients. Each decision consumes at most7input bits.
    // The initial normalized range and32bit refills need at most269bytes,
    // within the unchanged272byte partition guard, including all lookahead.
    // Short inputs retain the bit-at-a-time reader. Cancelable blocks use the
    // separate polling wrapper and preserve its exact canceled prefix.
    internal bool CanReadCoefficientBlock(ReadOnlySpan<byte> data, OperationContext context)
        => CanReadCoefficientWindow(data, context, 1);

    // Consecutive blocks share their arithmetic state. Each block has the same
    // worst-case decision bound; this conservative window never borrows bytes
    // from a following partition. Live cancellation keeps its polling reader.
    internal bool CanReadCoefficientWindow(ReadOnlySpan<byte> data, OperationContext context, int blocks)
        => !context.Cancellation.CanBeCanceled
        && (uint)(blocks - 1) < 25
        && range is >= 128 and <= 255 && (uint)value < (uint)range && (uint)shift <= 7
        && end <= data.Length && end - position >= 272 * blocks;

    // One mode macroblock uses at most121 decisions, fewer than the304
    // covered by the existing272B coefficient lookahead proof. The declared
    // first-partition end still bounds every buffered input read.
    internal bool CanReadModeWindow(ReadOnlySpan<byte> data)
        => range is >= 128 and <= 255 && (uint)value < (uint)range && (uint)shift <= 7
        && end <= data.Length && end - position >= 272;

    internal readonly BlockReader BeginWindow(ReadOnlySpan<byte> data)
        => new(data, position, shift, range, value);

    // The decoder supplies its complete private model,16sample block and valid
    // plane/context. The selected input bounds exclude mid-block failures.
    // One small stack state lives inside the existing8192B coarse allowance.
    internal bool ReadCoefficientBlock<TMode>(ReadOnlySpan<byte> data, ReadOnlySpan<CoefficientModelRow> model,
        Span<short> values, int plane, int initialContext, int dc, int ac, OperationContext context, out byte nonzero)
        where TMode : struct
    {
        var reader = BeginWindow(data);
        var result = reader.ReadCoefficients<TMode>(model, values, plane, initialContext, dc, ac, out nonzero);
        reader.Publish(ref this);
        return result;
    }

    internal ref struct BlockReader
    {
        private readonly ReadOnlySpan<byte> data;
        internal int Position, Bits, Range;
        internal ulong Value;

        internal BlockReader(ReadOnlySpan<byte> data, int position, int shift, int range, int value)
        {
            this.data = data; Position = position + 1; Bits = shift + 1; Range = range;
            // Keep the code byte at a fixed position; future input follows it.
            Value = ((ulong)value << 56)
                | ((ulong)((uint)data[position] & ((1u << Bits) - 1)) << (56 - Bits));
            Fill(in data, ref Position, ref Bits, ref Value);
        }

        internal bool ReadCoefficients<TMode>(ReadOnlySpan<CoefficientModelRow> model, scoped Span<short> values,
            int plane, int initialContext, int dc, int ac, out byte nonzero)
            where TMode : struct
        {
            ReadOnlySpan<byte> zigzag = [0, 1, 4, 8, 5, 2, 3, 6, 9, 12, 13, 10, 7, 11, 14, 15];
            // The admitted window/full private model exclude mid-block failure.
            // Only the completed block publishes arithmetic state to its window.
            var position = Position; var bits = Bits; var range = Range; var value = Value;
            var planeBase = plane * 48;
            var previousZero = false; var predictor = initialContext; nonzero = 0;
            for (var c = plane == 0 ? 1 : 0; c < 16; c++)
            {
                ref readonly var p = ref model[planeBase + c * 3 + predictor];
                if (!previousZero && TokenRead(in data, p[0], ref position, ref bits, ref range, ref value) == 0) break;
                if (TokenRead(in data, p[1], ref position, ref bits, ref range, ref value) == 0)
                {
                    previousZero = true; predictor = 0;
                    continue;
                }
                previousZero = false;
                int sample;
                if (TokenRead(in data, p[2], ref position, ref bits, ref range, ref value) == 0)
                {
                    sample = 1; predictor = 1;
                }
                else
                {
                    predictor = 2;
                    if (TokenRead(in data, p[3], ref position, ref bits, ref range, ref value) == 0)
                        sample = TokenRead(in data, p[4], ref position, ref bits, ref range, ref value) == 0 ? 2
                            : 3 + TokenRead(in data, p[5], ref position, ref bits, ref range, ref value);
                    else
                    {
                        int category;
                        if (TokenRead(in data, p[6], ref position, ref bits, ref range, ref value) == 0)
                            category = TokenRead(in data, p[7], ref position, ref bits, ref range, ref value);
                        else
                        {
                            var upper = TokenRead(in data, p[8], ref position, ref bits, ref range, ref value);
                            category = 2 + 2 * upper + TokenRead(in data, p[9 + upper], ref position, ref bits, ref range, ref value);
                        }
                        sample = Category(in data, category, ref position, ref bits, ref range, ref value);
                    }
                }
                nonzero = 1;
                var sign = Read(in data, 128, ref position, ref bits, ref range, ref value);
                if (typeof(TMode) == typeof(PixelMode))
                {
                    if (sign != 0) sample = -sample;
                    values[zigzag[c]] = unchecked((short)(sample * (c == 0 ? dc : ac)));
                }
            }
            Position = position; Bits = bits; Range = range; Value = value;
            return nonzero != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Read(in ReadOnlySpan<byte> data, int probability,
            ref int position, ref int bits, ref int range, ref ulong value)
        {
            Normalize(in data, ref position, ref bits, ref range, ref value);
            var boundary = 1 + ((range - 1) * probability >> 8);
            var bit = value >= ((ulong)boundary << 56) ? 1 : 0; var mask = -bit;
            value -= (ulong)(boundary & mask) << 56;
            range = boundary + ((range - 2 * boundary) & mask);
            return bit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int TokenRead(in ReadOnlySpan<byte> data, int probability,
            ref int position, ref int bits, ref int range, ref ulong value)
        {
            Normalize(in data, ref position, ref bits, ref range, ref value);
            var boundary = 1 + ((range - 1) * probability >> 8);
            var split = (ulong)boundary << 56;
            if (value < split) { range = boundary; return 0; }
            range -= boundary; value -= split; return 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Normalize(in ReadOnlySpan<byte> data,
            ref int position, ref int bits, ref int range, ref ulong value)
        {
            var count = BitOperations.LeadingZeroCount((uint)range) - 24;
            range <<= count; bits -= count; value <<= count;
            Fill(in data, ref position, ref bits, ref value);
        }

        internal void Publish(ref Vp8BooleanReader target)
        {
            // The previous decision began with >=8 known future bits and needs
            // at most7 to normalize. Publish without reading following input.
            var count = BitOperations.LeadingZeroCount((uint)Range) - 24;
            var bits = Bits - count;
            target.position = Position - (bits + 7) / 8; target.shift = (bits - 1) & 7;
            target.range = Range << count; target.value = (int)((Value >> (56 - count)) & 255);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Fill(in ReadOnlySpan<byte> data, ref int position, ref int bits, ref ulong value)
        {
            // With1..7 future bits, insert32 below the code and known input.
            // The low insertion space is zero;33..39 future bits then remain.
            // Refill positions and the original272B lookahead stay unchanged.
            if (bits >= 8) return;
            value |= (ulong)BinaryPrimitives.ReadUInt32BigEndian(data.Slice(position, 4)) << (24 - bits);
            position += 4; bits += 32;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Category(in ReadOnlySpan<byte> data, int category,
            ref int position, ref int bits, ref int range, ref ulong value)
        {
            ReadOnlySpan<byte> p = category switch
            {
                0 => [159], 1 => [165, 145], 2 => [173, 148, 140],
                3 => [176, 155, 140, 135], 4 => [180, 157, 141, 134, 130],
                _ => [254, 254, 243, 230, 196, 177, 153, 140, 133, 130, 129]
            };
            ReadOnlySpan<int> bases = [5, 7, 11, 19, 35, 67]; var sample = 0;
            foreach (var probability in p)
                sample = sample * 2 + Read(in data, probability, ref position, ref bits, ref range, ref value);
            return bases[category] + sample;
        }
    }
}
