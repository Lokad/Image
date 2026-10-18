namespace Lokad.Image.Internal;

internal static partial class Vp8Encoder
{
    // Original bounded-prefix empirical model and scratch reuse. RFC 6386 sections
    // 11.5/13 specify the probability-update syntax and coefficient contexts.
    internal sealed class CoefficientModel : IDisposable
    {
        private readonly OperationContext context;
        private readonly long reservation;
        private readonly int[] counts;
        private readonly byte[] side, last;
        private bool disposed;
        internal byte[] Probabilities { get; }
        internal int PrefixMacroblocks { get; }
        internal bool Ready { get; private set; }

        internal static CoefficientModel? TryCreate(Vp8Planes source, int blocks, int lumaTrials, int chromaTrials, OperationContext context)
        {
            var samples = source.Y.Length + source.U.Length + source.V.Length;
            var prefix = Math.Min(blocks, 64);
            var extra = prefix * (384L / 4 + 24) + 16384;
            var maximumHeaderBytes = ((blocks * (1L + 16 * 9 + 3) + 1056L * 9 + 64) * 7 + 7) / 8 + 3;
            if (maximumHeaderBytes > 0x7FFFF) return null;
            // Conservative optional admission includes both staged partitions,
            // transient doubling, and a header retry. No mandatory extra limit.
            // A boolean decision normalizes by at most seven bits, and a
            // coefficient has at most 24 token/category/sign decisions.
            var tokens = Math.Min(context.Limits.MaxOutputBytes, samples * 21L + 32);
            var header = Math.Min(0x7FFFF, context.Limits.MaxOutputBytes);
            var growth = 3 * tokens + 3L * header + 384;
            var work = blocks * (16L * lumaTrials * 192 + chromaTrials * 512L + 24L * 512 + 64)
                + samples * 128L + 65536;
            if (extra > source.MemoryBytes || extra + growth > context.RemainingWorkingBytes || work > context.RemainingWork) return null;
            return new CoefficientModel(samples, blocks, context);
        }

        internal CoefficientModel(int samples, int blocks, OperationContext context)
        {
            this.context = context; PrefixMacroblocks = Math.Min(blocks, 64);
            if (samples != blocks * 384) throw new ArgumentException("VP8 private plane geometry differs.");
            reservation = PrefixMacroblocks * (384L / 4 + 24) + 16384;
            context.Reserve(reservation);
            counts = new int[1056 * 2]; Probabilities = Vp8Tables.CoefficientProbabilities.ToArray();
            side = new byte[PrefixMacroblocks * 384 / 4]; last = new byte[PrefixMacroblocks * 24];
        }

        internal byte Record(byte[] source, int stride, int x, int y, int sampleOffset, int block,
            ReadOnlySpan<short> coefficients, int plane, int predictor)
        {
            // At most twelve token-tree decisions for each of sixteen values,
            // plus cache/classification work. Admit this bounded block together;
            // cancellation is checked before any histogram or sample mutation.
            context.Work(64 + 12 * 16);
            var end = 15; while (end >= 0 && coefficients[Zigzag[end]] == 0) end--;
            last[block] = (byte)(end + 1); var previousZero = false;
            for (var c = 0; c <= Math.Min(end + 1, 15); c++)
            {
                var at = ((plane * 8 + Bands[c]) * 3 + predictor) * 11;
                if (c > end) { Count(TokenPaths[11], at); break; }
                var z = Zigzag[c]; var value = coefficients[z]; var magnitude = Math.Abs((int)value);
                // The normalized 4x4 transform has absolute coefficient <=2040;
                // every quantizer is >=4. Nine magnitude bits cover rounding.
                if (magnitude > 511) throw new InvalidOperationException("VP8 quantized coefficient exceeds its proven cache range.");
                var index = (y + (z >> 2)) * stride + x + (z & 3);
                source[index] = (byte)magnitude;
                var global = block * 16 + z;
                var packed = (magnitude >> 8) | (value < 0 ? 2 : 0);
                side[global >> 2] |= (byte)(packed << ((global & 3) * 2));
                var category = magnitude < 5 ? -1 : magnitude < 7 ? 0 : magnitude < 11 ? 1 : magnitude < 19 ? 2
                    : magnitude < 35 ? 3 : magnitude < 67 ? 4 : 5;
                var path = TokenPaths[category < 0 ? magnitude : category + 5];
                Count(previousZero ? path >> 5 : path, at);
                predictor = Math.Min(magnitude, 2); previousZero = magnitude == 0;
            }
            return end < 0 ? (byte)0 : (byte)1;
        }

        private void Count(ulong path, int at)
        {
            while (path != 1)
            {
                var step = (int)(path & 31);
                counts[(at + (step >> 1)) * 2 + (step & 1)]++; path >>= 5;
            }
        }

        internal void SelectProbabilities()
        {
            context.Work(1056 * 16);
            for (var i = 0; i < 1056; i++)
            {
                var zero = counts[i * 2]; var one = counts[i * 2 + 1]; var total = (long)zero + one;
                if (total == 0) continue;
                var candidate = (int)Math.Clamp((zero * 256L + total / 2) / total, 1, 255);
                var old = Probabilities[i]; if (candidate == old) continue;
                var update = Vp8Tables.CoefficientUpdateProbabilities[i];
                var overhead = 8 + Math.Log2(update / (256d - update));
                if (Cost(zero, one, old) - Cost(zero, one, candidate) > overhead) Probabilities[i] = (byte)candidate;
            }
        }

        private static double Cost(int zero, int one, int probability)
            => -zero * Math.Log2(probability / 256d) - one * Math.Log2((256 - probability) / 256d);

        internal void UseDefaults() => Vp8Tables.CoefficientProbabilities.CopyTo(Probabilities);

        internal void CompletePrefix(Vp8Planes source, Vp8BooleanWriter writer, int columns, int rows)
        {
            SelectProbabilities();
            var above = new byte[Math.Min(columns, PrefixMacroblocks) * 8];
            Span<byte> left = stackalloc byte[8];
            Emit(source, writer, above, left, columns, rows);
            Ready = true;
        }

        internal void Emit(Vp8Planes source, Vp8BooleanWriter writer, Span<byte> above, Span<byte> left, int columns, int rows)
        {
            above.Clear();
            for (var my = 0; my < rows; my++)
            {
                left.Clear();
                for (var mx = 0; mx < columns; mx++)
                {
                    if (my * columns + mx >= PrefixMacroblocks) return;
                    var block = (my * columns + mx) * 24;
                    for (var b = 0; b < 16; b++)
                    {
                        var a = mx * 8 + b % 4; var l = b / 4;
                        above[a] = left[l] = EmitBlock(source.Y, source.Stride, mx * 16 + b % 4 * 4, my * 16 + b / 4 * 4,
                            0, block + b, 3, above[a] + left[l], writer);
                    }
                    for (var plane = 0; plane < 2; plane++)
                    for (var b = 0; b < 4; b++)
                    {
                        var a = mx * 8 + 4 + plane * 2 + b % 2; var l = 4 + plane * 2 + b / 2;
                        above[a] = left[l] = EmitBlock(plane == 0 ? source.U : source.V, source.Stride / 2,
                            mx * 8 + b % 2 * 4, my * 8 + b / 2 * 4, source.Y.Length + plane * source.U.Length,
                            block + 16 + plane * 4 + b, 2, above[a] + left[l], writer);
                    }
                }
            }
        }

        private byte EmitBlock(byte[] source, int stride, int x, int y, int sampleOffset, int block, int plane, int predictor, Vp8BooleanWriter writer)
        {
            var end = last[block] - 1; var previousZero = false;
            for (var c = 0; c <= Math.Min(end + 1, 15); c++)
            {
                var p = Probabilities.AsSpan(((plane * 8 + Bands[c]) * 3 + predictor) * 11, 11);
                if (c > end) { writer.Codeword(TokenPaths[11], p); break; }
                var z = Zigzag[c]; var index = (y + (z >> 2)) * stride + x + (z & 3); var global = block * 16 + z;
                var packed = side[global >> 2] >> ((global & 3) * 2);
                var magnitude = source[index] | ((packed & 1) << 8);
                var category = magnitude < 5 ? -1 : magnitude < 7 ? 0 : magnitude < 11 ? 1 : magnitude < 19 ? 2
                    : magnitude < 35 ? 3 : magnitude < 67 ? 4 : 5;
                var path = TokenPaths[category < 0 ? magnitude : category + 5];
                writer.Codeword(previousZero ? path >> 5 : path, p);
                if (category >= 0) Category(writer, category, magnitude);
                if (magnitude != 0) writer.Write(packed >> 1 & 1);
                predictor = Math.Min(magnitude, 2); previousZero = magnitude == 0;
            }
            return end < 0 ? (byte)0 : (byte)1;
        }

        public void Dispose()
        {
            if (disposed) return; disposed = true; context.Release(reservation);
        }
    }
}
