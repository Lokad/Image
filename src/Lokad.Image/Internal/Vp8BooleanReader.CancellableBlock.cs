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
    // completed-block prefix. A failed private decoder is abandoned by its caller;
    // no cursor is resumed or exposed after an exception. No new storage/owner.
    internal bool ReadCancellableCoefficientBlock<TMode>(ReadOnlySpan<byte> data, ReadOnlySpan<byte> probabilities,
        Span<short> values, int plane, int initialContext, int dc, int ac, OperationContext context, out byte nonzero)
        where TMode : struct
    {
        nonzero = 0;
        context.Cancellation.ThrowIfCancellationRequested();
        var cancellation = context.Cancellation;
        var reader = BeginCoefficientWindow(data);
        var position = reader.Position; var bits = reader.Bits; var range = reader.Range; var value = reader.Value;
        var model = MemoryMarshal.Cast<byte, CoefficientModelRow>(probabilities);
        ReadOnlySpan<byte> bands = [0, 1, 2, 3, 6, 4, 5, 6, 6, 6, 6, 6, 6, 6, 6, 7];
        ReadOnlySpan<byte> zigzag = [0, 1, 4, 8, 5, 2, 3, 6, 9, 12, 13, 10, 7, 11, 14, 15];
        var previousZero = false; var predictor = initialContext; var planeBase = plane * 24;
        for (var c = plane == 0 ? 1 : 0; c < 16; c++)
        {
            ref readonly var p = ref model[planeBase + bands[c] * 3 + predictor];
            // RFC6386 section13.2. Traverse coefficient classes directly;
            // zero skips the next EOB decision, while nonzero classes already
            // determine the next context before sign and quantization.
            if (!previousZero && CancellableBlockReader.TokenRead(in data, p[0], cancellation,
                ref position, ref bits, ref range, ref value) == 0) break;
            if (CancellableBlockReader.TokenRead(in data, p[1], cancellation,
                ref position, ref bits, ref range, ref value) == 0)
            {
                previousZero = true; predictor = 0;
                continue;
            }
            previousZero = false;
            int sample;
            if (CancellableBlockReader.TokenRead(in data, p[2], cancellation,
                ref position, ref bits, ref range, ref value) == 0)
            {
                sample = 1; predictor = 1;
            }
            else
            {
                predictor = 2;
                if (CancellableBlockReader.TokenRead(in data, p[3], cancellation,
                    ref position, ref bits, ref range, ref value) == 0)
                    sample = CancellableBlockReader.TokenRead(in data, p[4], cancellation,
                        ref position, ref bits, ref range, ref value) == 0 ? 2
                        : 3 + CancellableBlockReader.TokenRead(in data, p[5], cancellation,
                            ref position, ref bits, ref range, ref value);
                else
                {
                    int category;
                    if (CancellableBlockReader.TokenRead(in data, p[6], cancellation,
                        ref position, ref bits, ref range, ref value) == 0)
                        category = CancellableBlockReader.TokenRead(in data, p[7], cancellation,
                            ref position, ref bits, ref range, ref value);
                    else
                    {
                        var upper = CancellableBlockReader.TokenRead(in data, p[8], cancellation,
                            ref position, ref bits, ref range, ref value);
                        category = 2 + 2 * upper + CancellableBlockReader.TokenRead(in data, p[9 + upper], cancellation,
                            ref position, ref bits, ref range, ref value);
                    }
                    sample = CancellableBlockReader.Category(in data, category, cancellation,
                        ref position, ref bits, ref range, ref value);
                }
            }
            nonzero = 1;
            var sign = CancellableBlockReader.Read(in data, 128, cancellation,
                    ref position, ref bits, ref range, ref value);
            if (typeof(TMode) == typeof(PixelMode))
            {
                if (sign != 0) sample = -sample;
                values[zigzag[c]] = unchecked((short)(sample * (c == 0 ? dc : ac)));
            }
        }
        reader.Position = position; reader.Bits = bits; reader.Range = range; reader.Value = value;
        reader.Publish(ref this);
        return nonzero != 0;
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
        internal static int TokenRead(in ReadOnlySpan<byte> data, int probability, CancellationToken cancellation,
            ref int position, ref int bits, ref int range, ref ulong value)
        {
            cancellation.ThrowIfCancellationRequested();
            return BlockReader.TokenRead(in data, probability, ref position, ref bits, ref range, ref value);
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
