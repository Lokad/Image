namespace Lokad.Image.Internal;

internal static partial class JpegEncoder
{
    // 384 initial-error visits, then three components. Each component charges
    // 392 setup/commit units and at most eight solves of 21681 units:
    // 64 setup + 63*15 bank entries + 2016*10 arcs + 64 termination +
    // 128 reconstruction + 256 evaluation + 64 candidate selection/copy.
    internal const int Trellis444WorkPerMcu = 521904;

    internal static void RefineJoint444(ReadOnlySpan<double> source, ReadOnlySpan<ushort> luma,
        ReadOnlySpan<ushort> chroma, Span<int> values, ReadOnlySpan<int> prices, int lambda,
        OperationContext context, double? errorLimit = null, ReadOnlySpan<ushort> amplitudeLimits = default)
    {
        if (source.Length != 192 || values.Length != 192 || luma.Length != 64 || chroma.Length != 64
            || prices.Length != 512 || lambda < 0 || amplitudeLimits.Length is not (0 or 128)
            || errorLimit.HasValue && (!double.IsFinite(errorLimit.Value) || errorLimit.Value < 0))
            throw new ArgumentException("JPEG trellis requires three complete co-sited blocks and two price models.");
        if (lambda == 0) return;
        context.Work(384);
        var originalError = 0d; var currentError = 0d;
        for (var scan = 0; scan < 64; scan++)
        {
            var i = JpegTransform.Zigzag[scan];
            var y = source[i]; var cb = source[64 + i]; var cr = source[128 + i];
            originalError += JointRgbError(y - (double)TrellisNearest(y, luma[i]) * luma[i],
                cb - (double)TrellisNearest(cb, chroma[i]) * chroma[i],
                cr - (double)TrellisNearest(cr, chroma[i]) * chroma[i]);
            currentError += JointRgbError(y - (double)values[i] * luma[i],
                cb - (double)values[64 + i] * chroma[i], cr - (double)values[128 + i] * chroma[i]);
        }
        // 1612 stack bytes, once per MCU call. A single frequency's bank is
        // rebuilt for each solve; there is no full bank matrix or frame copy.
        Span<double> doubles = stackalloc double[138];
        Span<short> shorts = stackalloc short[217];
        Span<byte> bytes = stackalloc byte[74];
        var trellis = new JointTrellis(source, luma, chroma, values, prices, lambda, context,
            errorLimit ?? (originalError + originalError / 32), currentError, doubles, shorts, bytes, amplitudeLimits);
        for (var component = 0; component < 3; component++) trellis.Run(component);
    }

    private static int TrellisNearest(double source, ushort quantizer)
        => (int)Math.Round(source / quantizer, MidpointRounding.AwayFromZero);

    private ref struct JointTrellis
    {
        private readonly ReadOnlySpan<double> source;
        private readonly ReadOnlySpan<ushort> luma, chroma;
        private readonly Span<int> values;
        private readonly ReadOnlySpan<int> prices;
        private readonly ReadOnlySpan<ushort> amplitudeLimits;
        private readonly int lambda;
        private readonly OperationContext context;
        private readonly double maximumError;
        private double currentError;
        private readonly Span<double> zeroPrefix, costs, levelErrors;
        private readonly Span<short> chosen, candidate, best, bank, levels;
        private readonly Span<byte> previous, categories;
        private int component;

        internal JointTrellis(ReadOnlySpan<double> source, ReadOnlySpan<ushort> luma,
            ReadOnlySpan<ushort> chroma, Span<int> values, ReadOnlySpan<int> prices, int lambda,
            OperationContext context, double maximumError, double currentError,
            Span<double> doubles, Span<short> shorts, Span<byte> bytes, ReadOnlySpan<ushort> amplitudeLimits)
        {
            this.source = source; this.luma = luma; this.chroma = chroma; this.values = values;
            this.prices = prices; this.lambda = lambda; this.context = context;
            this.amplitudeLimits = amplitudeLimits;
            this.maximumError = maximumError; this.currentError = currentError;
            zeroPrefix = doubles[..64]; costs = doubles.Slice(64, 64); levelErrors = doubles.Slice(128, 10);
            chosen = shorts[..64]; candidate = shorts.Slice(64, 64); best = shorts.Slice(128, 64);
            bank = shorts.Slice(192, 15); levels = shorts.Slice(207, 10);
            previous = bytes[..64]; categories = bytes.Slice(64, 10); component = 0;
        }

        private ReadOnlySpan<int> Model => prices.Slice(component == 0 ? 0 : 256, 256);

        internal void Run(int component)
        {
            this.component = component;
            context.Work(192);
            zeroPrefix[0] = 0;
            for (var scan = 1; scan < 64; scan++) zeroPrefix[scan] = zeroPrefix[scan - 1] + ErrorChange(scan, 0);
            context.Work(64);
            var beforeBits = PriceCurrent();
            context.Work(64);
            for (var scan = 0; scan < 64; scan++) best[scan] = (short)values[component * 64 + JpegTransform.Zigzag[scan]];
            var bestBits = beforeBits; var bestError = 0d;
            context.Work(8);
            Solve(lambda, out var bits, out var error);
            Select(bits, error, beforeBits, ref bestBits, ref bestError);
            if (currentError + error > maximumError)
            {
                Solve(0, out bits, out error);
                Select(bits, error, beforeBits, ref bestBits, ref bestError);
                var low = 0d; var high = (double)lambda;
                for (var probe = 0; probe < 6; probe++)
                {
                    var weight = (low + high) / 2;
                    Solve(weight, out bits, out error);
                    Select(bits, error, beforeBits, ref bestBits, ref bestError);
                    if (currentError + error <= maximumError) low = weight; else high = weight;
                }
            }
            context.Work(64);
            if (bestBits >= beforeBits) return;
            // No source coefficients are changed while a path is being tried.
            // A refusal/cancellation cannot leak an unselected path.
            for (var scan = 1; scan < 64; scan++) values[component * 64 + JpegTransform.Zigzag[scan]] = best[scan];
            currentError += bestError;
        }

        private void Select(int bits, double error, int beforeBits, ref int bestBits, ref double bestError)
        {
            context.Work(64);
            if (bits >= bestBits || error + (double)lambda * (bits - beforeBits) / 256 >= 0
                || currentError + error > maximumError) return;
            candidate.CopyTo(best); bestBits = bits; bestError = error;
        }

        private void Solve(double weight, out int bits, out double error)
        {
            context.Work(64);
            costs[0] = 0;
            var model = Model;
            for (var end = 1; end < 64; end++)
            {
                context.Work(15);
                var count = BuildLevels(end);
                var bestCost = double.PositiveInfinity; var bestPrevious = 0; short bestLevel = 0;
                var endPrefix = zeroPrefix[end - 1];
                for (var start = 0; start < end; start++)
                {
                    context.Work(count);
                    var prefix = costs[start] + endPrefix - zeroPrefix[start];
                    var run = end - start - 1;
                    for (var option = 0; option < count; option++)
                    {
                        var category = categories[option];
                        var price = run / 16 * model[0xF0] + model[(run % 16) * 16 + category] + category * 256;
                        var cost = prefix + levelErrors[option] + weight * price / 256;
                        if (cost < bestCost) { bestCost = cost; bestPrevious = start; bestLevel = levels[option]; }
                    }
                }
                costs[end] = bestCost; previous[end] = (byte)bestPrevious; chosen[end] = bestLevel;
            }
            context.Work(64);
            var eob = weight * model[0] / 256;
            var finalCost = zeroPrefix[63] + eob; var last = 0;
            for (var end = 1; end < 64; end++)
            {
                var cost = costs[end] + zeroPrefix[63] - zeroPrefix[end] + (end < 63 ? eob : 0);
                if (cost < finalCost) { finalCost = cost; last = end; }
            }
            context.Work(64);
            candidate.Clear();
            candidate[0] = (short)values[component * 64];
            context.Work(64);
            while (last != 0) { candidate[last] = chosen[last]; last = previous[last]; }
            context.Work(256);
            bits = PriceCandidate(); error = 0;
            for (var scan = 1; scan < 64; scan++) error += ErrorChange(scan, candidate[scan]);
        }

        private int BuildLevels(int scan)
        {
            var index = JpegTransform.Zigzag[scan]; var at = component * 64 + index;
            var original = TrellisNearest(source[at], component == 0 ? luma[index] : chroma[index]);
            var limit = amplitudeLimits.IsEmpty ? 1023 : amplitudeLimits[(component == 0 ? 0 : 64) + index];
            if (!amplitudeLimits.IsEmpty) original = Math.Clamp(original, -limit, limit);
            bank[0] = (short)values[at]; bank[1] = (short)original; bank[2] = 0; var used = 3;
            for (var category = Category(original) - 1; category >= 0; category--)
            {
                var magnitude = (1 << category) - 1;
                bank[used++] = (short)(original > 0 ? magnitude : -magnitude);
            }
            bank[used++] = (short)(original - 1); bank[used++] = (short)(original + 1);
            var count = 0;
            for (var i = 0; i < used; i++)
            {
                var value = bank[i];
                if (value == 0 || value < -limit || value > limit) continue;
                var category = (byte)Category(value); var slot = categories[..count].IndexOf(category);
                var error = ErrorChange(scan, value);
                if (slot < 0)
                { categories[count] = category; levels[count] = value; levelErrors[count++] = error; }
                else if (error < levelErrors[slot]) { levels[slot] = value; levelErrors[slot] = error; }
            }
            return count;
        }

        private double ErrorChange(int scan, int value)
        {
            var i = JpegTransform.Zigzag[scan];
            var y = source[i] - (double)values[i] * luma[i];
            var cb = source[64 + i] - (double)values[64 + i] * chroma[i];
            var cr = source[128 + i] - (double)values[128 + i] * chroma[i];
            var before = JointRgbError(y, cb, cr);
            var residual = source[component * 64 + i] - (double)value * (component == 0 ? luma[i] : chroma[i]);
            return JointRgbError(component == 0 ? residual : y, component == 1 ? residual : cb,
                component == 2 ? residual : cr) - before;
        }

        private int PriceCurrent()
            => JointAcPrice(values.Slice(component * 64, 64), Model);

        private int PriceCandidate()
        {
            var model = Model; var zeros = 0; var result = 0;
            for (var scan = 1; scan < 64; scan++)
            {
                var value = candidate[scan];
                if (value == 0) { zeros++; continue; }
                while (zeros >= 16) { result += model[0xF0]; zeros -= 16; }
                var size = Category(value); result += model[zeros * 16 + size] + size * 256; zeros = 0;
            }
            if (zeros != 0) result += model[0];
            return result;
        }
    }
}
