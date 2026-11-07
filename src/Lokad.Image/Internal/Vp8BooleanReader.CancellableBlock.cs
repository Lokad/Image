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
        var cancellation = context.Cancellation;
        var reader = BeginCoefficientWindow(data);
        var position = reader.Position; var bits = reader.Bits; var range = reader.Range; var value = reader.Value;
        try
        {
            var model = MemoryMarshal.Cast<byte, CoefficientModelRow>(probabilities);
            ReadOnlySpan<byte> bands = [0, 1, 2, 3, 6, 4, 5, 6, 6, 6, 6, 6, 6, 6, 6, 7];
            ReadOnlySpan<byte> zigzag = [0, 1, 4, 8, 5, 2, 3, 6, 9, 12, 13, 10, 7, 11, 14, 15];
            var previousZero = false; var predictor = initialContext;
            for (var c = plane == 0 ? 1 : 0; c < 16; c++)
            {
                ref readonly var p = ref model[(plane * 8 + bands[c]) * 3 + predictor];
                var token = CancellableBlockReader.Token(in data, in p, previousZero, cancellation,
                    ref position, ref bits, ref range, ref value);
                if (token == 11) break;
                var sample = token < 5 ? token : CancellableBlockReader.Category(in data, token - 5, cancellation,
                    ref position, ref bits, ref range, ref value);
                previousZero = sample == 0; predictor = Math.Min(sample, 2);
                if (sample == 0) continue;
                nonzero = 1;
                if (CancellableBlockReader.Read(in data, 128, cancellation,
                    ref position, ref bits, ref range, ref value) != 0) sample = -sample;
                values[zigzag[c]] = unchecked((short)(sample * (c == 0 ? dc : ac)));
            }
            return nonzero != 0;
        }
        finally
        {
            reader.Position = position; reader.Bits = bits; reader.Range = range; reader.Value = value;
            reader.Publish(ref this);
        }
    }

    internal static class CancellableBlockReader
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Read(in ReadOnlySpan<byte> data, int probability, CancellationToken cancellation,
            ref int position, ref int bits, ref int range, ref ulong value)
        {
            cancellation.ThrowIfCancellationRequested();
            return BlockReader.Read(in data, probability, ref position, ref bits, ref range, ref value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int TokenRead(in ReadOnlySpan<byte> data, int probability, CancellationToken cancellation,
            ref int position, ref int bits, ref int range, ref ulong value)
        {
            cancellation.ThrowIfCancellationRequested();
            return BlockReader.TokenRead(in data, probability, ref position, ref bits, ref range, ref value);
        }

        // RFC6386 section13.2, preserving the original probability/read order.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Token(in ReadOnlySpan<byte> data, in CoefficientModelRow p, bool previousZero,
            CancellationToken cancellation, ref int position, ref int bits, ref int range, ref ulong value)
        {
            if (!previousZero && TokenRead(in data, p[0], cancellation, ref position, ref bits, ref range, ref value) == 0) return 11;
            if (TokenRead(in data, p[1], cancellation, ref position, ref bits, ref range, ref value) == 0) return 0;
            if (TokenRead(in data, p[2], cancellation, ref position, ref bits, ref range, ref value) == 0) return 1;
            if (TokenRead(in data, p[3], cancellation, ref position, ref bits, ref range, ref value) == 0)
                return TokenRead(in data, p[4], cancellation, ref position, ref bits, ref range, ref value) == 0 ? 2
                    : 3 + TokenRead(in data, p[5], cancellation, ref position, ref bits, ref range, ref value);
            if (TokenRead(in data, p[6], cancellation, ref position, ref bits, ref range, ref value) == 0)
                return 5 + TokenRead(in data, p[7], cancellation, ref position, ref bits, ref range, ref value);
            var upper = TokenRead(in data, p[8], cancellation, ref position, ref bits, ref range, ref value);
            return 7 + 2 * upper + TokenRead(in data, p[9 + upper], cancellation, ref position, ref bits, ref range, ref value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Category(in ReadOnlySpan<byte> data, int category, CancellationToken cancellation,
            ref int position, ref int bits, ref int range, ref ulong value)
        {
            ReadOnlySpan<byte> p = category switch
            {
                0 => [159], 1 => [165, 145], 2 => [173, 148, 140],
                3 => [176, 155, 140, 135], 4 => [180, 157, 141, 134, 130],
                _ => [254, 254, 243, 230, 196, 177, 153, 140, 133, 130, 129]
            };
            ReadOnlySpan<int> bases = [5, 7, 11, 19, 35, 67]; var sample = 0;
            foreach (var probability in p) sample = sample * 2 + Read(in data, probability, cancellation,
                ref position, ref bits, ref range, ref value);
            return bases[category] + sample;
        }
    }
}
