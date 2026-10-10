using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal static class JpegEncoder
{
    public static void Encode(ImageBuffer image, Stream output, JpegEncodeOptions options, OperationContext context)
    {
        if (options.Quality is < 1 or > 100 || !Enum.IsDefined(options.Sampling)
            || options.RestartInterval is < 0 or > 65535 || options.BackgroundRgb > 0xFFFFFF)
            throw new ArgumentOutOfRangeException(nameof(options));
        context.PixelBytes(image.Width, image.Height, image.BitDepth);
        if (image.Width > 65535 || image.Height > 65535) throw new ImageLimitException("JPEG dimensions exceed 65535.");
        context.Reserve(image.ByteLength);
        // Fixed table/scratch allocation is admitted before any header is written.
        context.Reserve(8192);
        var components = options.Sampling == JpegSampling.Grayscale ? 1 : 3;
        var h = options.Sampling is JpegSampling.YCbCr420 or JpegSampling.YCbCr422 ? 2 : 1;
        var v = options.Sampling == JpegSampling.YCbCr420 ? 2 : 1;
        var columns = (image.Width + h * 8 - 1) / (h * 8); var rows = (image.Height + v * 8 - 1) / (v * 8);
        var luma = Quantization(options.Quality, false); var chroma = Quantization(options.Quality, true);
        var dcSymbols = Enumerable.Range(0, 12).Select(value => (byte)value).ToArray();
        var acSymbols = new byte[162]; acSymbols[0] = 0;
        for (var run = 0; run < 16; run++)
        for (var size = 1; size <= 10; size++) acSymbols[1 + run * 10 + size - 1] = (byte)(run << 4 | size);
        acSymbols[^1] = 0xF0;
        var acCodes = new int[256]; Array.Fill(acCodes, -1);
        for (var i = 0; i < acSymbols.Length; i++) acCodes[acSymbols[i]] = i;
        var pixels = new byte[64]; var transformed = new double[64]; var predictors = new int[components];
        // The existing quantization destination fits in the admitted fixed
        // workspace. Keep it on the safe stack when passed to the SIMD helper.
        Span<int> quantized = stackalloc int[64];
        output.Write(new byte[] { 255, 216 });
        Segment(output, 0xE0, new byte[] { 74, 70, 73, 70, 0, 1, 1, 0, 0, 1, 0, 1, 0, 0 });
        WriteQuantization(output, luma, 0);
        if (components == 3) WriteQuantization(output, chroma, 1);
        var frame = new byte[6 + 3 * components]; frame[0] = 8;
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(1), (ushort)image.Height);
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(3), (ushort)image.Width); frame[5] = (byte)components;
        for (var component = 0; component < components; component++)
        {
            frame[6 + component * 3] = (byte)(component + 1);
            frame[7 + component * 3] = component == 0 ? (byte)(h << 4 | v) : (byte)0x11;
            frame[8 + component * 3] = component == 0 ? (byte)0 : (byte)1;
        }
        Segment(output, 0xC0, frame);
        WriteHuffman(output, 0, 4, dcSymbols); WriteHuffman(output, 0x10, 8, acSymbols);
        if (options.RestartInterval != 0)
        {
            Span<byte> interval = stackalloc byte[2]; BinaryPrimitives.WriteUInt16BigEndian(interval, (ushort)options.RestartInterval);
            Segment(output, 0xDD, interval);
        }
        var scan = new byte[4 + components * 2]; scan[0] = (byte)components;
        for (var component = 0; component < components; component++) scan[1 + component * 2] = (byte)(component + 1);
        scan[2 + components * 2] = 63; Segment(output, 0xDA, scan);
        var writer = new JpegEntropyWriter(output, context); var restart = 0;
        for (var mcu = 0; mcu < columns * rows; mcu++)
        {
            context.Work(1);
            if (options.RestartInterval != 0 && mcu != 0 && mcu % options.RestartInterval == 0)
            {
                writer.Restart(restart++); Array.Clear(predictors);
            }
            for (var component = 0; component < components; component++)
            {
                var blockH = component == 0 ? h : 1; var blockV = component == 0 ? v : 1;
                var q = component == 0 ? luma : chroma;
                for (var by = 0; by < blockV; by++)
                for (var bx = 0; bx < blockH; bx++)
                {
                    context.Work(64L * (component == 0 ? 1 : h * v) * 12);
                    // Geometry is constant for this block. Preserve sample
                    // traversal and arithmetic while deriving its origin once.
                    var samplesX = component == 0 ? 1 : h; var samplesY = component == 0 ? 1 : v;
                    var sourceLeft = mcu % columns * h * 8 + bx * 8;
                    var sourceTop = mcu / columns * v * 8 + by * 8;
                    if (image.BitDepth == 8 && h == 1 && v == 1 && sourceLeft <= image.Width - 8
                        && Avx512F.IsSupported && Avx512F.VL.IsSupported && Avx2.IsSupported)
                    {
                        for (var y = 0; y < 8; y++)
                            SampleComponentRow8(image.GetRow8(Math.Min(sourceTop + y, image.Height - 1)).Slice(sourceLeft * 4, 32),
                                component, options.BackgroundRgb, pixels.AsSpan(y * 8, 8));
                    }
                    else
                    {
                        for (var y = 0; y < 8; y++)
                        for (var x = 0; x < 8; x++)
                        {
                            double sum = 0;
                            for (var dy = 0; dy < samplesY; dy++)
                            for (var dx = 0; dx < samplesX; dx++)
                            {
                                var sourceX = sourceLeft + x * samplesX + dx;
                                var sourceY = sourceTop + y * samplesY + dy;
                                sourceX = Math.Min(sourceX, image.Width - 1); sourceY = Math.Min(sourceY, image.Height - 1);
                                ReadRgb(image, sourceX, sourceY, options.BackgroundRgb, out var r, out var g, out var b);
                                sum += component switch
                                {
                                    0 => 0.299 * r + 0.587 * g + 0.114 * b,
                                    1 => 128 - 0.168735892 * r - 0.331264108 * g + 0.5 * b,
                                    _ => 128 + 0.5 * r - 0.418687589 * g - 0.081312411 * b
                                };
                            }
                            pixels[y * 8 + x] = (byte)Math.Clamp(Math.Round(sum / (samplesX * samplesY), MidpointRounding.AwayFromZero), 0, 255);
                        }
                    }
                    JpegTransform.Forward(pixels, transformed, context);
                    Quantize(transformed, q, quantized);
                    var difference = quantized[0] - predictors[component]; predictors[component] = quantized[0];
                    var category = Category(difference);
                    if (category > 11) throw new ImageDataException("JPEG DC transform exceeds its coding range.");
                    writer.Write(category, 4); writer.Signed(difference, category);
                    var zeros = 0;
                    for (var i = 1; i < 64; i++)
                    {
                        var value = quantized[JpegTransform.Zigzag[i]];
                        if (value == 0) { zeros++; continue; }
                        while (zeros >= 16) { writer.Write(acCodes[0xF0], 8); zeros -= 16; }
                        category = Category(value);
                        if (category > 10) throw new ImageDataException("JPEG AC transform exceeds its coding range.");
                        writer.Write(acCodes[zeros << 4 | category], 8); writer.Signed(value, category); zeros = 0;
                    }
                    if (zeros != 0) writer.Write(acCodes[0], 8);
                }
            }
        }
        writer.Align(); context.Cancellation.ThrowIfCancellationRequested(); output.Write(new byte[] { 255, 217 });
    }

    internal static void SampleComponentRow8(ReadOnlySpan<byte> rgba, int component, uint? background, Span<byte> pixels)
    {
        if (rgba.Length != 32 || pixels.Length != 8 || (uint)component > 2)
            throw new ArgumentException("JPEG component rows have eight RGBA pixels and a valid component.");
        var color = background ?? 0;
        if (Avx512F.IsSupported && Avx512F.VL.IsSupported && Avx2.IsSupported)
        {
            // T.81 component samples retain the owned encoded-space RGB blend
            // and binary64 color matrix. One complete row is acquired, without
            // overread or pixel caching. Each lane preserves separate operations.
            var packed = Vector256.Create(rgba).AsInt32(); var mask = Vector256.Create(255);
            var alpha = Avx2.ShiftRightLogical(packed.AsUInt32(), 24).AsInt32();
            if (background is null && Avx2.MoveMask(Avx2.CompareEqual(alpha, mask).AsByte()) != -1)
                throw new ArgumentException("JPEG requires opaque pixels or an explicit RGB background.", "image");
            var inverseAlpha = Avx2.Subtract(mask, alpha);
            var r = Avx512F.ConvertToVector512Double(BlendRow8(Avx2.And(packed, mask), alpha, inverseAlpha, (int)(color >> 16 & 255)));
            var g = Avx512F.ConvertToVector512Double(BlendRow8(Avx2.And(Avx2.ShiftRightLogical(packed.AsUInt32(), 8).AsInt32(), mask), alpha, inverseAlpha, (int)(color >> 8 & 255)));
            var b = Avx512F.ConvertToVector512Double(BlendRow8(Avx2.And(Avx2.ShiftRightLogical(packed.AsUInt32(), 16).AsInt32(), mask), alpha, inverseAlpha, (int)(color & 255)));
            var samples = component switch
            {
                0 => Avx512F.Add(Avx512F.Add(Avx512F.Multiply(Vector512.Create(0.299), r), Avx512F.Multiply(Vector512.Create(0.587), g)), Avx512F.Multiply(Vector512.Create(0.114), b)),
                1 => Avx512F.Add(Avx512F.Subtract(Avx512F.Subtract(Vector512.Create(128d), Avx512F.Multiply(Vector512.Create(0.168735892), r)), Avx512F.Multiply(Vector512.Create(0.331264108), g)), Avx512F.Multiply(Vector512.Create(0.5), b)),
                _ => Avx512F.Subtract(Avx512F.Subtract(Avx512F.Add(Vector512.Create(128d), Avx512F.Multiply(Vector512.Create(0.5), r)), Avx512F.Multiply(Vector512.Create(0.418687589), g)), Avx512F.Multiply(Vector512.Create(0.081312411), b))
            };
            JpegTransform.RoundSamples(samples, pixels);
            return;
        }
        for (var x = 0; x < 8; x++)
        {
            var pixel = rgba.Slice(x * 4, 4); var alpha = pixel[3];
            if (alpha != 255 && background is null)
                throw new ArgumentException("JPEG requires opaque pixels or an explicit RGB background.", "image");
            var r = Flatten8(pixel[0], alpha, (int)(color >> 16 & 255));
            var g = Flatten8(pixel[1], alpha, (int)(color >> 8 & 255));
            var b = Flatten8(pixel[2], alpha, (int)(color & 255));
            var sample = component switch
            {
                0 => 0.299 * r + 0.587 * g + 0.114 * b,
                1 => 128 - 0.168735892 * r - 0.331264108 * g + 0.5 * b,
                _ => 128 + 0.5 * r - 0.418687589 * g - 0.081312411 * b
            };
            pixels[x] = (byte)Math.Clamp(Math.Round(sample, MidpointRounding.AwayFromZero), 0, 255);
        }
    }

    private static Vector256<int> BlendRow8(Vector256<int> value, Vector256<int> alpha, Vector256<int> inverseAlpha, int background)
    {
        var numerator = Avx2.Add(Avx2.Add(Avx2.MultiplyLow(value, alpha), Avx2.MultiplyLow(Vector256.Create(background), inverseAlpha)), Vector256.Create(127));
        // For 0 <= n <= 65152, floor(n/255) = (n + 1 + (n >> 8)) >> 8.
        // Byte products bound n to this range; all arithmetic is exact Int32.
        return Avx2.ShiftRightLogical(Avx2.Add(Avx2.Add(numerator, Vector256.Create(1)), Avx2.ShiftRightLogical(numerator.AsUInt32(), 8).AsInt32()).AsUInt32(), 8).AsInt32();
    }

    private static void ReadRgb(ImageBuffer image, int x, int y, uint? background, out int r, out int g, out int b)
    {
        var color = background ?? 0;
        // Acquire/validate the row once, then bound all four channel reads to
        // one complete pixel. No row or converted-pixel cache is retained.
        if (image.BitDepth == 8)
        {
            var pixel = image.GetRow8(y).Slice(x * 4, 4); var alpha = pixel[3];
            if (alpha != 255 && background is null) throw new ArgumentException("JPEG requires opaque pixels or an explicit RGB background.", nameof(image));
            r = Flatten8(pixel[0], alpha, (int)((color >> 16) & 255));
            g = Flatten8(pixel[1], alpha, (int)((color >> 8) & 255));
            b = Flatten8(pixel[2], alpha, (int)(color & 255));
            return;
        }
        var sample = image.GetRow16(y).Slice(x * 4, 4); var opacity = sample[3];
        if (opacity != 65535 && background is null) throw new ArgumentException("JPEG requires opaque pixels or an explicit RGB background.", nameof(image));
        r = Flatten(sample[0], opacity, (int)((color >> 16) & 255), 65535);
        g = Flatten(sample[1], opacity, (int)((color >> 8) & 255), 65535);
        b = Flatten(sample[2], opacity, (int)(color & 255), 65535);
    }

    // The specified encoded-space blend at maximum 255 cancels one factor
    // 255. Its odd denominator has no half ties; nearest rounding is +127.
    // Byte inputs bound the numerator to 65025, so int arithmetic is exact.
    internal static int Flatten8(int value, int alpha, int background)
        => (value * alpha + background * (255 - alpha) + 127) / 255;

    private static int Flatten(int value, int alpha, int background, int maximum)
    {
        var numerator = value * (long)alpha * 255 + background * (long)(maximum - alpha) * maximum;
        var denominator = (long)maximum * maximum;
        return (int)((numerator + denominator / 2) / denominator);
    }

    // Original frequency-weighted quantization policy. Quality controls this
    // formula; it does not promise another encoder's quality numbering.
    private static ushort[] Quantization(int quality, bool chroma)
    {
        var result = new ushort[64]; var scale = quality < 50 ? 5000 / quality : 200 - 2 * quality;
        for (var v = 0; v < 8; v++)
        for (var u = 0; u < 8; u++)
        {
            var basis = chroma ? 2 + 3 * (u + v) + u * v : 1 + 2 * (u + v) + u * v;
            result[v * 8 + u] = (ushort)Math.Clamp((basis * scale + 50) / 100, 1, 255);
        }
        return result;
    }

    internal static void Quantize(ReadOnlySpan<double> coefficients, ReadOnlySpan<ushort> quantization, Span<int> result)
    {
        if (coefficients.Length != 64 || quantization.Length != 64 || result.Length != 64)
            throw new ArgumentException("JPEG quantization operates on complete 8x8 blocks.");
        // T.81 F.1.1.4: retain the original binary64 quotient and nearest,
        // away-from-zero rounding. Encoder coefficients are finite and bounded
        // by the 8-bit transform; generated quantizers are in [1,255].
        // No reciprocal approximation, reordered transform, new scratch or work.
        if (Avx512F.IsSupported && Avx2.IsSupported)
        {
            var magnitudeMask = Vector512.Create(long.MaxValue); var signMask = Vector512.Create(long.MinValue);
            var half = Vector512.Create(0.5); var one = Vector512.Create(1.0).AsInt64();
            for (var i = 0; i < 64; i += 8)
            {
                var integers = Avx2.ConvertToVector256Int32(Vector128.Create(quantization.Slice(i, 8)));
                var divisors = Avx512F.ConvertToVector512Double(integers);
                var quotient = Avx512F.Divide(Vector512.Create(coefficients.Slice(i, 8)), divisors);
                var magnitude = Avx512F.And(quotient.AsInt64(), magnitudeMask).AsDouble();
                var integral = Avx512F.RoundScale(magnitude, 0x09); // floor, suppress precision exception
                var atLeastHalf = Avx512F.CompareGreaterThanOrEqual(Avx512F.Subtract(magnitude, integral), half);
                var rounded = Avx512F.Add(integral, Avx512F.And(atLeastHalf.AsInt64(), one).AsDouble());
                var signed = Avx512F.Or(rounded.AsInt64(), Avx512F.And(quotient.AsInt64(), signMask)).AsDouble();
                Avx512F.ConvertToVector256Int32WithTruncation(signed).CopyTo(result.Slice(i, 8));
            }
            return;
        }
        for (var i = 0; i < 64; i++)
            result[i] = (int)Math.Round(coefficients[i] / quantization[i], MidpointRounding.AwayFromZero);
    }

    // T.81 F.1.2.1 SIZE is the magnitude's binary width, with zero in category
    // zero. Keep Math.Abs's full-Int32 overflow behavior; the BCL provides
    // hardware leading-zero counting where available and a safe fallback.
    internal static int Category(int value)
        => 32 - BitOperations.LeadingZeroCount((uint)Math.Abs(value));

    private static void WriteQuantization(Stream output, ushort[] values, byte id)
    {
        Span<byte> payload = stackalloc byte[65]; payload[0] = id;
        for (var i = 0; i < 64; i++) payload[i + 1] = (byte)values[JpegTransform.Zigzag[i]];
        Segment(output, 0xDB, payload);
    }

    private static void WriteHuffman(Stream output, byte selector, int length, byte[] symbols)
    {
        var payload = new byte[17 + symbols.Length]; payload[0] = selector; payload[length] = (byte)symbols.Length;
        symbols.CopyTo(payload, 17); Segment(output, 0xC4, payload);
    }

    internal static void Segment(Stream output, byte marker, ReadOnlySpan<byte> payload)
    {
        if (payload.Length > 65533) throw new ArgumentOutOfRangeException(nameof(payload));
        Span<byte> header = stackalloc byte[4]; header[0] = 255; header[1] = marker;
        BinaryPrimitives.WriteUInt16BigEndian(header[2..], (ushort)(payload.Length + 2));
        output.Write(header); output.Write(payload);
    }
}
