namespace Lokad.Image.Internal;

internal static partial class JpegEncoder
{
    // 192 prefix/error + 64 current pricing + 64 solve setup + 63*15 bank
    // entries + 2016*10 arcs + 64 termination + 128 reconstruction +
    // 256 evaluation + 64 commit. The preceding zero-only pass is separate.
    internal const int Trellis420WorkPerBlock = 21937;

    // T.81 coefficient selection with the existing geometric AC prices and
    // sampling-cell weighted source-DCT squared error. DQT and DC stay intact.
    // One last-nonzero DAG replaces local decisions; no reference image or
    // quality-matching data enters the encoder. Stable old values on ties.
    internal static void Refine420(ReadOnlySpan<double> source, ReadOnlySpan<ushort> quantization,
        Span<int> values, int lambda, int samplingArea, OperationContext context)
    {
        if (source.Length != 64 || quantization.Length != 64 || values.Length != 64
            || lambda < 0 || samplingArea is < 1 or > 4)
            throw new ArgumentException("JPEG trellis requires a complete block and valid weights.");
        if (lambda == 0) return;
        // 1484 safe stack bytes, once per block call. Rebuild one frequency's
        // finite bank rather than retaining a matrix or full-frame storage.
        Span<double> doubles = stackalloc double[138];
        Span<short> shorts = stackalloc short[153];
        Span<byte> bytes = stackalloc byte[74];
        var trellis = new ScalarTrellis(source, quantization, values, lambda, samplingArea,
            context, doubles, shorts, bytes);
        trellis.Run();
    }

    private ref struct ScalarTrellis
    {
        private readonly ReadOnlySpan<double> source;
        private readonly ReadOnlySpan<ushort> quantization;
        private readonly Span<int> values;
        private readonly int lambda, samplingArea;
        private readonly OperationContext context;
        private readonly Span<double> zeroPrefix, costs, levelErrors;
        private readonly Span<short> chosen, candidate, bank, levels;
        private readonly Span<byte> previous, categories;

        internal ScalarTrellis(ReadOnlySpan<double> source, ReadOnlySpan<ushort> quantization,
            Span<int> values, int lambda, int samplingArea, OperationContext context,
            Span<double> doubles, Span<short> shorts, Span<byte> bytes)
        {
            this.source = source; this.quantization = quantization; this.values = values;
            this.lambda = lambda; this.samplingArea = samplingArea; this.context = context;
            zeroPrefix = doubles[..64]; costs = doubles.Slice(64, 64); levelErrors = doubles.Slice(128, 10);
            chosen = shorts[..64]; candidate = shorts.Slice(64, 64);
            bank = shorts.Slice(128, 15); levels = shorts.Slice(143, 10);
            previous = bytes[..64]; categories = bytes.Slice(64, 10);
        }

        internal void Run()
        {
            context.Work(192); zeroPrefix[0] = 0;
            for (var scan = 1; scan < 64; scan++) zeroPrefix[scan] = zeroPrefix[scan - 1] + ErrorChange(scan, 0);
            context.Work(64); var beforeBits = AcDecisionBits(values, FixedAc.Table.Lengths);
            context.Work(64); costs[0] = 0;
            var model = FixedAc.Table.Lengths;
            for (var end = 1; end < 64; end++)
            {
                context.Work(15); var count = BuildLevels(end);
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
                        var bits = run / 16 * model[0xF0] + model[(run % 16) * 16 + category] + category;
                        var cost = prefix + levelErrors[option] + (double)lambda * bits;
                        if (cost < bestCost) { bestCost = cost; bestPrevious = start; bestLevel = levels[option]; }
                    }
                }
                costs[end] = bestCost; previous[end] = (byte)bestPrevious; chosen[end] = bestLevel;
            }
            context.Work(64);
            var eob = (double)lambda * model[0];
            var finalCost = zeroPrefix[63] + eob; var last = 0;
            for (var end = 1; end < 64; end++)
            {
                var cost = costs[end] + zeroPrefix[63] - zeroPrefix[end] + (end < 63 ? eob : 0);
                if (cost < finalCost) { finalCost = cost; last = end; }
            }
            context.Work(64); candidate.Clear(); candidate[0] = (short)values[0];
            context.Work(64);
            while (last != 0) { candidate[last] = chosen[last]; last = previous[last]; }
            context.Work(256); var afterBits = PriceCandidate(); var error = 0d;
            for (var scan = 1; scan < 64; scan++) error += ErrorChange(scan, candidate[scan]);
            context.Work(64);
            if (afterBits >= beforeBits || error + (double)lambda * (afterBits - beforeBits) >= 0) return;
            // No coefficient changes until a complete path has been selected
            // and all work/cancellation checks have succeeded.
            for (var scan = 1; scan < 64; scan++) values[JpegTransform.Zigzag[scan]] = candidate[scan];
        }

        private int BuildLevels(int scan)
        {
            var index = JpegTransform.Zigzag[scan];
            var original = TrellisNearest(source[index], quantization[index]);
            bank[0] = (short)values[index]; bank[1] = (short)original; bank[2] = 0; var used = 3;
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
                if (value is 0 or < -1023 or > 1023) continue;
                var category = (byte)Category(value); var slot = categories[..count].IndexOf(category);
                var error = ErrorChange(scan, value);
                if (slot < 0) { categories[count] = category; levels[count] = value; levelErrors[count++] = error; }
                else if (error < levelErrors[slot]) { levels[slot] = value; levelErrors[slot] = error; }
            }
            return count;
        }

        private double ErrorChange(int scan, int value)
        {
            var i = JpegTransform.Zigzag[scan];
            var before = source[i] - (double)values[i] * quantization[i];
            var after = source[i] - (double)value * quantization[i];
            return samplingArea * (after * after - before * before);
        }

        private int PriceCandidate()
        {
            var model = FixedAc.Table.Lengths; var zeros = 0; var result = 0;
            for (var scan = 1; scan < 64; scan++)
            {
                var value = candidate[scan];
                if (value == 0) { zeros++; continue; }
                while (zeros >= 16) { result += model[0xF0]; zeros -= 16; }
                var size = Category(value); result += model[zeros * 16 + size] + size; zeros = 0;
            }
            if (zeros != 0) result += model[0];
            return result;
        }
    }
}
