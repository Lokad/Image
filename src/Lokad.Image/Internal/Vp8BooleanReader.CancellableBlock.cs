using System.Runtime.CompilerServices;

namespace Lokad.Image.Internal;

internal partial struct Vp8BooleanReader
{
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
            ReadOnlySpan<byte> bands = [0, 1, 2, 3, 6, 4, 5, 6, 6, 6, 6, 6, 6, 6, 6, 7];
            ReadOnlySpan<byte> zigzag = [0, 1, 4, 8, 5, 2, 3, 6, 9, 12, 13, 10, 7, 11, 14, 15];
            var previousZero = false; var predictor = initialContext;
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
            return nonzero != 0;
        }
        finally { reader.Publish(ref this, context); }
    }

    internal ref struct CancellableBlockReader
    {
        private BlockReader reader;
        private readonly CancellationToken cancellation;

        internal CancellableBlockReader(ReadOnlySpan<byte> data, Vp8BooleanReader initial, CancellationToken cancellation)
        {
            reader = new BlockReader(data, initial.position, initial.shift, initial.range, initial.value);
            this.cancellation = cancellation;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int Read(int probability)
        {
            cancellation.ThrowIfCancellationRequested();
            return reader.Read(probability);
        }

        internal void Publish(ref Vp8BooleanReader target, OperationContext context)
        {
            target.position = reader.Position - (reader.Bits + 7) / 8;
            target.shift = (reader.Bits - 1) & 7;
            target.range = reader.Range; target.value = (int)(reader.Value >> reader.Bits);
        }

        // RFC6386 section13.2, preserving the original probability/read order.
        // Keep the existing noncancelable hot methods untouched.
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
