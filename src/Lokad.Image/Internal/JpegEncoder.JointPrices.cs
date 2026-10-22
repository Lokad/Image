namespace Lokad.Image.Internal;

internal static partial class JpegEncoder
{
    // Sparse counters become Q8 source prices in place. Arena, scan map and
    // component-local pending EOB runs fit the existing admitted 8 KiB workspace.
    private const int JointPriceBandSetupWork = 1920;
    private const int JointPriceSampleWork = 136129;

    private static long JointPriceTrainingWork(long mcus, int quality, int components,
        JpegSampling sampling, JpegQuantizationPolicy policy)
    {
        if (sampling != JpegSampling.YCbCr444 || AcDecisionLambda(quality, components, sampling, policy) == 0) return 0;
        var shape = SourcePriceShape444(PlannedSourcePriceBands444(quality));
        return Math.Min(64, mcus) * (JointPriceSampleWork + shape.ComponentBands)
            + shape.FinishWork + shape.ComponentBands + JointPriceBandSetupWork + JointPriceLayoutWork;
    }

    private static JointAcPrices TrainJoint444Prices(ImageBuffer? image, scoped JpegSampleInput samples, uint? background,
        int columns, int rows, scoped ReadOnlySpan<ushort> luma, scoped ReadOnlySpan<ushort> chroma,
        scoped Span<double> source, scoped Span<int> values, Span<ushort> prices, Span<ushort> offsets,
        int lambda, OperationContext context)
    {
        var bands = SourcePriceBands444(luma, chroma, lambda, context);
        context.Work(JointPriceLayoutWork);
        var shape = SourcePriceShape444(bands);
        if (prices.Length < shape.Slots) throw new ArgumentException("Source-price arena is too small.");
        SourcePriceOffsets444(bands, offsets);
        prices = prices[..shape.Slots];
        prices.Clear();
        Span<byte> pending = stackalloc byte[192]; pending.Clear();
        var mcus = checked(columns * rows); var count = Math.Min(64, mcus);
        Span<byte> pixels = stackalloc byte[64];
        for (var sample = 0; sample < count; sample++)
        {
            context.Work(1);
            var mcu = JointPriceSamplePosition(sample, count, mcus);
            var left = mcu % columns * 8; var top = mcu / columns * 8;
            for (var component = 0; component < 3; component++)
            {
                context.Work(768);
                if (image is null) SampleCallerBlock(samples, left, top, 1, 1, component, pixels);
                else
                {
                    for (var y = 0; y < 8; y++)
                    for (var x = 0; x < 8; x++)
                    {
                        ReadRgb(image, Math.Min(left + x, image.Width - 1), Math.Min(top + y, image.Height - 1),
                            background, out var r, out var g, out var b);
                        var value = component switch
                        {
                            0 => (0.299 * r + 0.587 * g) + 0.114 * b,
                            1 => ((128 - 0.168735892 * r) - 0.331264108 * g) + 0.5 * b,
                            _ => ((128 + 0.5 * r) - 0.418687589 * g) - 0.081312411 * b
                        };
                        pixels[y * 8 + x] = (byte)Math.Clamp(Math.Round(value, MidpointRounding.AwayFromZero), 0, 255);
                    }
                }
                PrepareJoint444Block(pixels, component == 0 ? luma : chroma, source, values,
                    component, luma, chroma, lambda, context);
            }
            context.Work(192 + shape.ComponentBands);
            for (var component = 0; component < 3; component++)
                CountSourceBandAc(values.Slice(component * 64, 64), prices,
                    offsets.Slice(component == 0 ? 0 : 64, 64), pending.Slice(component * 64, 64), bands.ForComponent(component));
        }
        context.Work(shape.ComponentBands);
        for (var component = 0; component < 3; component++)
            FlushSourceBandEobRuns(prices, offsets.Slice(component == 0 ? 0 : 64, 64),
                pending.Slice(component * 64, 64), bands.ForComponent(component));
        FinishSourceBandPrices(prices, bands, context);
        return new(prices, offsets);
    }

    internal static int JointPriceSamplePosition(int sample, int count, int mcus)
        => count == 1 ? 0 : (int)((long)sample * (mcus - 1) / (count - 1));

    internal static void CountJointAc(ReadOnlySpan<int> values, Span<int> counts, ulong bandEnds = 0)
    {
        var zeros = 0;
        for (var scan = 1; scan < 64; scan++)
        {
            var value = values[JpegTransform.Zigzag[scan]];
            if (value == 0) zeros++;
            else
            {
                while (zeros >= 16) { counts[0xF0]++; zeros -= 16; }
                var size = Category(value);
                if (size > 10) throw new ImageDataException("JPEG AC transform exceeds its coding range.");
                counts[zeros << 4 | size]++; zeros = 0;
            }
            if (scan == 63 || (bandEnds & (1UL << scan)) != 0)
            {
                if (zeros != 0) counts[0]++;
                zeros = 0;
            }
        }
    }

    internal static void CountSourceBandAc(ReadOnlySpan<int> values, Span<ushort> counts,
        ReadOnlySpan<ushort> offsets, Span<byte> pending, ulong bandEnds)
    {
        var zeros = 0; var first = 1; var hasValues = false;
        for (var scan = 1; scan < 64; scan++)
        {
            var at = offsets[scan] & 1023; var value = values[JpegTransform.Zigzag[scan]];
            if (value == 0) zeros++;
            else
            {
                if (!hasValues) { FlushSourceEobRun(counts, at, ref pending[first]); hasValues = true; }
                while (zeros >= 16) { counts[at + 1]++; zeros -= 16; }
                var size = Category(value);
                if (size > 10) throw new ImageDataException("JPEG AC transform exceeds its coding range.");
                counts[at + SourcePriceSlot(zeros << 4 | size)]++; zeros = 0;
            }
            if (scan == 63 || (bandEnds & (1UL << scan)) != 0)
            {
                if (zeros != 0) { pending[first]++; counts[at + 16]++; }
                zeros = 0; first = scan + 1; hasValues = false;
            }
        }
    }

    private static void FlushSourceEobRun(Span<ushort> counts, int at, ref byte pending)
    {
        if (pending == 0) return;
        counts[at + SourcePriceSlot(System.Numerics.BitOperations.Log2((uint)pending) << 4)]++;
        pending = 0;
    }

    internal static void FlushSourceBandEobRuns(Span<ushort> counts, ReadOnlySpan<ushort> offsets,
        Span<byte> pending, ulong bandEnds)
    {
        bandEnds |= 1UL << 63;
        for (var first = 1; first < 64;)
        {
            FlushSourceEobRun(counts, offsets[first] & 1023, ref pending[first]);
            first = System.Numerics.BitOperations.TrailingZeroCount(bandEnds & (ulong.MaxValue << first)) + 1;
        }
    }

    internal static void FinishSourceBandPrices(Span<ushort> prices, JointAcBands bands, OperationContext context)
    {
        var shape = SourcePriceShape444(bands);
        if (prices.Length != shape.Slots || shape.Slots > SourcePriceCapacity)
            throw new ArgumentException("Invalid bounded source-price arena.");
        context.Work(shape.FinishWork);
        // Validate all models before mutation. Each source component contributes
        // at most 64 terminal blocks; even shared Cb/Cr runs coalesce separately.
        for (var pass = 0; pass < 2; pass++)
        {
            var at = 0;
            for (var family = 0; family < 2; family++)
            {
                var ends = bands.ForComponent(family) | (1UL << 63);
                for (var first = 1; first < 64;)
                {
                    var last = System.Numerics.BitOperations.TrailingZeroCount(ends & (ulong.MaxValue << first));
                    var length = last - first + 1; var size = 17 + 10 * Math.Min(16, length);
                    var model = prices.Slice(at, size);
                    if (pass == 0)
                    {
                        var total = 0;
                        for (var i = 0; i < model.Length; i++) if (i != 16) total += model[i];
                        var eobBlocks = 0; var maximumEobBlocks = 0;
                        for (var exponent = 0; exponent < 15; exponent++)
                        {
                            var count = model[SourcePriceSlot(exponent << 4)];
                            if (exponent > 6 && count != 0)
                                throw new ArgumentOutOfRangeException(nameof(prices), "Source EOB run exceeds its sample count.");
                            eobBlocks += count << exponent;
                            maximumEobBlocks += count * ((1 << (exponent + 1)) - 1);
                        }
                        if (total > 8064 || length <= 16 && model[1] != 0 || model[16] < eobBlocks || model[16] > maximumEobBlocks || model[16] > (family == 0 ? 64 : 128))
                            throw new ArgumentOutOfRangeException(nameof(prices), "Invalid source AC histogram.");
                    }
                    else SourceHuffmanPrices(model);
                    at += size; first = last + 1;
                }
            }
        }
    }

    internal static int JointAcPrice(ReadOnlySpan<int> coefficients, JointAcPrices prices, ulong bandEnds = 0)
    {
        var zeros = 0; var result = 0;
        for (var scan = 1; scan < 64; scan++)
        {
            var value = coefficients[JpegTransform.Zigzag[scan]];
            if (value == 0) zeros++;
            else
            {
                while (zeros >= 16) { result += prices.At(0xF0, scan); zeros -= 16; }
                var size = Category(value); result += prices.At(zeros << 4 | size, scan) + size * 256; zeros = 0;
            }
            if (scan == 63 || (bandEnds & (1UL << scan)) != 0)
            {
                if (zeros != 0) result += prices.At(0, scan);
                zeros = 0;
            }
        }
        return result;
    }
}
