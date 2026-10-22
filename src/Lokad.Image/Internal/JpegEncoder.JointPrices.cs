using System.Numerics;

namespace Lokad.Image.Internal;

internal static partial class JpegEncoder
{
    // The histogram becomes the Q8 prices in place: 2048 stack bytes, alongside
    // the 2304-byte MCU and existing scratch within the admitted 8 KiB workspace.
    private const int JointPriceFinishWork = 3632;
    private const int JointPriceBandSetupWork = 1920;
    private const int JointPriceSampleWork = 136129;

    private static long JointPriceTrainingWork(long mcus, int quality, int components,
        JpegSampling sampling, JpegQuantizationPolicy policy)
        => sampling == JpegSampling.YCbCr444 && AcDecisionLambda(quality, components, sampling, policy) != 0
            ? Math.Min(64, mcus) * JointPriceSampleWork + JointPriceFinishWork + JointPriceBandSetupWork : 0;

    private static void TrainJoint444Prices(ImageBuffer? image, JpegSampleInput samples, uint? background,
        int columns, int rows, ReadOnlySpan<ushort> luma, ReadOnlySpan<ushort> chroma,
        Span<double> source, Span<int> values, Span<int> prices, int lambda, OperationContext context)
    {
        var bands = SourcePriceBands444(luma, chroma, lambda, context);
        prices.Clear();
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
            context.Work(192);
            for (var component = 0; component < 3; component++)
                CountJointAc(values.Slice(component * 64, 64), prices.Slice(component == 0 ? 0 : 256, 256), bands.ForComponent(component));
        }
        FinishJointPrices(prices, context);
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

    internal static void FinishJointPrices(Span<int> prices, OperationContext context)
    {
        if (prices.Length != 512) throw new ArgumentException("Two complete JPEG AC models are required.");
        // 512 histogram visits + 512 output visits + eight squarings for each
        // of 324 legal symbols and the two totals, charged before mutation.
        context.Work(JointPriceFinishWork);
        for (var group = 0; group < 2; group++)
        {
            var model = prices.Slice(group * 256, 256); var total = 162;
            for (var i = 0; i < 256; i++) total += model[i];
            var totalLog = JointLog2Q8(total);
            for (var symbol = 0; symbol < 256; symbol++)
                model[symbol] = symbol is 0 or 0xF0 || (symbol & 15) is >= 1 and <= 10
                    ? Math.Clamp(totalLog - JointLog2Q8(model[symbol] + 1), 1, 4096) : 0;
        }
    }

    internal static int JointLog2Q8(int value)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
        var integral = BitOperations.Log2((uint)value);
        var normalized = (ulong)value << (31 - integral); var fraction = 0;
        for (var bit = 7; bit >= 0; bit--)
        {
            // normalized < 2^32, so its square fits UInt64 before the shift.
            normalized = normalized * normalized >> 31;
            if (normalized >= 1UL << 32) { normalized >>= 1; fraction |= 1 << bit; }
        }
        return integral * 256 + fraction;
    }

    internal static int JointAcPrice(ReadOnlySpan<int> coefficients, ReadOnlySpan<int> prices, ulong bandEnds = 0)
    {
        var zeros = 0; var result = 0;
        for (var scan = 1; scan < 64; scan++)
        {
            var value = coefficients[JpegTransform.Zigzag[scan]];
            if (value == 0) zeros++;
            else
            {
                while (zeros >= 16) { result += prices[0xF0]; zeros -= 16; }
                var size = Category(value); result += prices[zeros << 4 | size] + size * 256; zeros = 0;
            }
            if (scan == 63 || (bandEnds & (1UL << scan)) != 0)
            {
                if (zeros != 0) result += prices[0];
                zeros = 0;
            }
        }
        return result;
    }
}
