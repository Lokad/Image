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
        private readonly byte[]? macroModes, y2High;
        private bool disposed;
        internal byte[] Probabilities { get; }
        internal int PrefixMacroblocks { get; }
        internal int CacheMacroblocks { get; }
        internal bool Ready { get; private set; }
        internal bool Intra16 => macroModes is not null;
        internal bool Refine { get; }
        internal bool NearZero { get; }
        internal byte[] Y2Above { get; }

        internal static CoefficientModel? TryCreate(Vp8Planes source, int blocks, int lumaTrials, int chromaTrials, OperationContext context,
            byte[]? modes = null, bool fullFrame = false, bool segmented = false, long refinementWork = 0, long nearZeroWork = 0)
        {
            var samples = source.Y.Length + source.U.Length + source.V.Length;
            var prefix = Math.Min(blocks, 64);
            var extra = prefix * (384L / 4 + 24) + 16384;
            var maximumHeaderBytes = ((blocks * (1L + 16 * 9 + 3 + (segmented ? 2 : 0))
                + 1056L * 9 + (segmented ? 192 : 64)) * 7 + 7) / 8 + 3;
            if (maximumHeaderBytes > 0x7FFFF) return null;
            // Conservative optional admission includes both staged partitions,
            // transient doubling, and a header retry. No mandatory extra limit.
            // A boolean decision normalizes by at most seven bits, and a
            // coefficient has at most 24 token/category/sign decisions.
            var tokens = Math.Min(context.Limits.MaxOutputBytes, samples * 21L + 32);
            var header = Math.Min(0x7FFFF, context.Limits.MaxOutputBytes);
            var growth = 3 * tokens + 3L * header + 384;
            var predictionWork = blocks * (16L * lumaTrials * 192 + chromaTrials * 512L + 24L * 512 + 64);
            if (segmented) predictionWork += blocks * 32L + 2048;
            var work = predictionWork + samples * 128L + 65536;
            var intraExtra = prefix * 16L + source.Stride / 16 + 64;
            var intraWork = blocks * (4L * 3072 + 16 * 512 + 2048 + 256
                + 16L * ((RateLumaModes - lumaTrials) * 192 + RateLumaModes * 768 - 512) + 20480);
            if (modes is not null && extra + intraExtra <= source.MemoryBytes
                && extra + intraExtra + growth <= context.RemainingWorkingBytes && work + intraWork <= context.RemainingWork)
            {
                // Reuse converted samples for every selected coefficient. Keep
                // the original prefix model for prediction decisions, then fit
                // only the entropy model to the full frame. No second image pass.
                var fullExtra = blocks * 136L + 16384 + source.Stride / 16 + 64;
                var fullWork = (blocks - prefix) * (24L * 256 + 256) + 1056 * 16;
                if (fullExtra <= source.MemoryBytes && fullExtra + growth <= context.RemainingWorkingBytes
                    && work + intraWork + fullWork <= context.RemainingWork)
                    return new CoefficientModel(samples, blocks, context, modes, source.Stride / 16, fullFrame: true,
                        refine: refinementWork != 0 && work + intraWork + fullWork + refinementWork <= context.RemainingWork);
                return new CoefficientModel(samples, blocks, context, modes, source.Stride / 16,
                    refine: refinementWork != 0 && work + intraWork + refinementWork <= context.RemainingWork);
            }
            if (extra > source.MemoryBytes || extra + growth > context.RemainingWorkingBytes) return null;
            if (work > context.RemainingWork)
            {
                // A q0 prefix adds only bounded statistics/cache work. Retain
                // the search/transform/header envelope and minimum token work;
                // variable arithmetic carries/growth are charged as performed.
                // This eligibility estimate does not promise that the complete
                // operation fits. Exhaustion remains terminal, without retry.
                var prefixWork = prefix * 24L * 256 + 1056 * 16;
                if (!fullFrame || predictionWork + prefixWork + blocks * 24L > context.RemainingWork) return null;
                // Segmented4x4 calls use exact row-error pruning. A larger
                // bounded sample can spend that work on compression; actual
                // cumulative exhaustion remains terminal, without a retry.
                var extended = Math.Min(blocks, 128);
                var extendedExtra = extended * 120L + 16384;
                var extendedWork = extended * 24L * 256 + 1056 * 16;
                if (segmented && modes is not null && extended > prefix && extendedExtra <= source.MemoryBytes
                    && extendedExtra + growth <= context.RemainingWorkingBytes
                    && predictionWork + extendedWork + blocks * 24L <= context.RemainingWork)
                    return new CoefficientModel(samples, blocks, context, prefixMacroblocks: 128,
                        nearZero: nearZeroWork != 0 && predictionWork + extendedWork + blocks * 24L + nearZeroWork <= context.RemainingWork);
                return new CoefficientModel(samples, blocks, context, nearZero: nearZeroWork != 0
                    && predictionWork + prefixWork + blocks * 24L + nearZeroWork <= context.RemainingWork);
            }
            if (fullFrame && blocks > prefix)
            {
                // The q0 predictor does not consult the probability model, so
                // fitting its complete frame changes entropy alone. Reuse the
                // same private-plane cache without Y2 storage or another pass.
                var fullExtra = blocks * 120L + 16384;
                var fullWork = (blocks - prefix) * (24L * 256 + 256) + 1056 * 16;
                if (fullExtra <= source.MemoryBytes && fullExtra + growth <= context.RemainingWorkingBytes
                    && work + fullWork <= context.RemainingWork)
                    return new CoefficientModel(samples, blocks, context, fullFrame: true);
            }
            return new CoefficientModel(samples, blocks, context);
        }

        internal CoefficientModel(int samples, int blocks, OperationContext context, byte[]? modes = null, int columns = 0,
            bool fullFrame = false, bool refine = false, int prefixMacroblocks = 64, bool nearZero = false)
        {
            this.context = context; Refine = refine; NearZero = nearZero; PrefixMacroblocks = Math.Min(blocks, prefixMacroblocks);
            CacheMacroblocks = fullFrame ? blocks : PrefixMacroblocks;
            if (samples != blocks * 384) throw new ArgumentException("VP8 private plane geometry differs.");
            reservation = CacheMacroblocks * (384L / 4 + 24) + 16384 + (modes is null ? 0 : CacheMacroblocks * 16L + columns + 64);
            context.Reserve(reservation);
            counts = new int[1056 * 2]; Probabilities = Vp8Tables.CoefficientProbabilities.ToArray();
            side = new byte[CacheMacroblocks * 384 / 4]; last = new byte[CacheMacroblocks * 24];
            macroModes = modes;
            y2High = modes is null ? null : new byte[CacheMacroblocks * 16];
            Y2Above = modes is null ? [] : new byte[columns];
        }

        internal byte Record(byte[] source, int stride, int x, int y, int sampleOffset, int block,
            ReadOnlySpan<short> coefficients, int plane, int predictor)
        {
            // At most twelve token-tree decisions for each of sixteen values,
            // plus cache/classification work. Admit this bounded block together;
            // cancellation is checked before any histogram or sample mutation.
            context.Work(64 + 12 * 16);
            var first = plane == 0 ? 1 : 0; var end = 15; while (end >= first && coefficients[Zigzag[end]] == 0) end--;
            last[block] = (byte)(end + 1); var previousZero = false;
            for (var c = first; c <= Math.Min(Math.Max(end + 1, first), 15); c++)
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
            return end < first ? (byte)0 : (byte)1;
        }

        internal byte RecordY2(Vp8Planes source, int macroblock, int x, int y, ReadOnlySpan<short> coefficients, int predictor)
        {
            context.Work(256);
            var end = 15; while (end >= 0 && coefficients[Zigzag[end]] == 0) end--;
            var metadata = macroModes ?? throw new InvalidOperationException("The second DC cache was not admitted.");
            metadata[macroblock * 16] = (byte)((metadata[macroblock * 16] & 131) | (end + 1) << 2);
            var previousZero = false;
            for (var c = 0; c <= Math.Min(end + 1, 15); c++)
            {
                var at = ((8 + Bands[c]) * 3 + predictor) * 11;
                if (c > end) { Count(TokenPaths[11], at); break; }
                var z = Zigzag[c]; var value = coefficients[z]; var magnitude = Math.Abs((int)value);
                if (magnitude > 2040) throw new InvalidOperationException("VP8 second DC coefficient exceeds its proven range.");
                source.Y[(y + (z / 4) * 4) * source.Stride + x + (z % 4) * 4] = unchecked((byte)value);
                y2High![macroblock * 16 + z] = (byte)(unchecked((ushort)value) >> 8);
                var category = magnitude < 5 ? -1 : magnitude < 7 ? 0 : magnitude < 11 ? 1 : magnitude < 19 ? 2
                    : magnitude < 35 ? 3 : magnitude < 67 ? 4 : 5;
                var path = TokenPaths[category < 0 ? magnitude : category + 5];
                Count(previousZero ? path >> 5 : path, at); predictor = Math.Min(magnitude, 2); previousZero = magnitude == 0;
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

        internal void CompleteMacroblock(Vp8Planes source, Vp8BooleanWriter writer, int columns, int rows, int completed)
        {
            if (completed == PrefixMacroblocks) SelectProbabilities();
            if (completed != CacheMacroblocks) return;
            if (CacheMacroblocks > PrefixMacroblocks)
            {
                UseDefaults(); SelectProbabilities();
            }
            var above = new byte[Math.Min(columns, CacheMacroblocks) * 8];
            Span<byte> left = stackalloc byte[8];
            Emit(source, writer, above, left, columns, rows);
            Ready = true;
        }

        internal void Emit(Vp8Planes source, Vp8BooleanWriter writer, Span<byte> above, Span<byte> left, int columns, int rows)
        {
            above.Clear();
            Span<byte> aboveY2 = Intra16 ? stackalloc byte[Math.Min(columns, CacheMacroblocks)] : default;
            aboveY2.Clear();
            for (var my = 0; my < rows; my++)
            {
                left.Clear(); byte leftY2 = 0;
                for (var mx = 0; mx < columns; mx++)
                {
                    if (my * columns + mx >= CacheMacroblocks) return;
                    var macroblock = my * columns + mx; var block = macroblock * 24;
                    var large = Intra16 && (macroModes![macroblock * 16] & 128) != 0;
                    if (large) aboveY2[mx] = leftY2 = EmitY2(source, macroblock, mx * 16, my * 16, aboveY2[mx] + leftY2, writer);
                    for (var b = 0; b < 16; b++)
                    {
                        var a = mx * 8 + b % 4; var l = b / 4;
                        above[a] = left[l] = EmitBlock(source.Y, source.Stride, mx * 16 + b % 4 * 4, my * 16 + b / 4 * 4,
                            0, block + b, large ? 0 : 3, above[a] + left[l], writer);
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
            var first = plane == 0 ? 1 : 0; var end = last[block] - 1; var previousZero = false;
            for (var c = first; c <= Math.Min(Math.Max(end + 1, first), 15); c++)
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
            return end < first ? (byte)0 : (byte)1;
        }

        private byte EmitY2(Vp8Planes source, int macroblock, int x, int y, int predictor, Vp8BooleanWriter writer)
        {
            var end = (macroModes![macroblock * 16] >> 2 & 31) - 1;
            Span<short> values = stackalloc short[16]; values.Clear();
            for (var c = 0; c <= end; c++)
            {
                var z = Zigzag[c]; var low = source.Y[(y + z / 4 * 4) * source.Stride + x + z % 4 * 4];
                values[z] = unchecked((short)(low | y2High![macroblock * 16 + z] << 8));
            }
            return Coefficients(writer, values, 1, predictor, Probabilities);
        }

        public void Dispose()
        {
            if (disposed) return; disposed = true; context.Release(reservation);
        }
    }
}
