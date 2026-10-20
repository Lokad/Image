using System.Buffers.Binary;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal static partial class JpegEncoder
{
    // Per-operation source and 8 KiB fixed workspace were admitted by the public
    // boundary. Original RGBA encoder bodies/dispatch remain unchanged.
    internal static void EncodeSamples(JpegSampleInput source, Stream output, JpegEncodeOptions options, OperationContext context)
    {
        var sampling = source.Format == JpegSampleFormat.Grayscale ? JpegSampling.Grayscale : options.Sampling;
        var components = sampling == JpegSampling.Grayscale ? 1 : 3;
        var h = sampling is JpegSampling.YCbCr420 or JpegSampling.YCbCr422 ? 2 : 1;
        var v = sampling == JpegSampling.YCbCr420 ? 2 : 1;
        var columns = (source.Width + h * 8 - 1) / (h * 8); var rows = (source.Height + v * 8 - 1) / (v * 8);
        var mcus = (long)columns * rows; var blocks = mcus * (h * v + components - 1);
        var events = blocks * 64 + (options.RestartInterval == 0 ? 0 : mcus);
        var work = 2 * mcus + mcus * (h * v * (4096 + 768) + (components - 1) * (4096 + 768 * h * v))
            + blocks * 192 + 262145;
        var adaptive = !context.Cancellation.CanBeCanceled && blocks >= 256 && events <= Array.MaxLength
            && work <= context.RemainingWork && events * sizeof(uint) + 65536 <= context.RemainingWorkingBytes
            && output is LimitedWriteStream limited && limited.SupportsAdaptiveJpeg(blocks * 432 + 2048);
        // Whole-operation work was admitted above and this path has no caller
        // callbacks/cancellation. Its 384-byte MCU fits the admitted 8 KiB.
        var prepareMcu = adaptive && source.Format == JpegSampleFormat.Rgb && h == 2 && v == 2
            && Avx512F.IsSupported && Avx512F.VL.IsSupported && Avx2.IsSupported && Ssse3.IsSupported;
        Span<byte> preparedMcu = prepareMcu ? stackalloc byte[384] : default;
        var policy = components == 1 ? JpegQuantizationPolicy.Original : options.QuantizationPolicy;
        var luma = Quantization(options.Quality, false, policy); var chroma = Quantization(options.Quality, true, policy);
        var pixels = new byte[64]; var transformed = new double[64]; var predictors = new int[components];
        Span<int> quantized = stackalloc int[64];
        uint[]? cache = null; long[][]? frequencies = null; var eventCount = 0;
        if (adaptive)
        {
            context.Reserve(events * sizeof(uint) + 65536);
            cache = adaptiveEvents; adaptiveEvents = null;
            if (cache is null || cache.Length != events) cache = new uint[(int)events];
        }
        try
        {
            if (adaptive)
            {
                frequencies = new long[4][];
                for (var t = 0; t < 4; t++) frequencies[t] = new long[256];
            }
            else WriteSampleHeader(source, output, options, components, h, v, luma, chroma, null);
            var writer = adaptive ? null : new JpegEntropyWriter(output, context); var restart = 0;
            for (var mcu = 0; mcu < columns * rows; mcu++)
            {
                context.Work(1);
                if (options.RestartInterval != 0 && mcu != 0 && mcu % options.RestartInterval == 0)
                {
                    if (adaptive) cache![eventCount++] = 1u << 24; else writer!.Restart(restart++);
                    Array.Clear(predictors);
                }
                if (prepareMcu) SampleCaller420Mcu(source, mcu % columns * 16, mcu / columns * 16, preparedMcu);
                for (var component = 0; component < components; component++)
                {
                    var blockH = component == 0 ? h : 1; var blockV = component == 0 ? v : 1;
                    var q = component == 0 ? luma : chroma;
                    for (var by = 0; by < blockV; by++)
                    for (var bx = 0; bx < blockH; bx++)
                    {
                        context.Work(64L * (component == 0 ? 1 : h * v) * 12);
                        var samplesX = component == 0 ? 1 : h; var samplesY = component == 0 ? 1 : v;
                        scoped ReadOnlySpan<byte> blockPixels;
                        if (prepareMcu) blockPixels = preparedMcu.Slice((component == 0 ? by * 2 + bx : component + 3) * 64, 64);
                        else
                        {
                            SampleCallerBlock(source, mcu % columns * h * 8 + bx * 8,
                                mcu / columns * v * 8 + by * 8, samplesX, samplesY, component, pixels);
                            blockPixels = pixels;
                        }
                        if (!JpegTransform.TryForwardQuantized(blockPixels, q, quantized, context))
                        {
                            JpegTransform.Forward(blockPixels, transformed, context); Quantize(transformed, q, quantized);
                        }
                        if (!adaptive) { WriteFixedBlock(quantized, ref predictors[component], writer!, FixedAc.Words); continue; }
                        context.Work(64);
                        var group = component == 0 ? 0 : 1;
                        var difference = quantized[0] - predictors[component]; predictors[component] = quantized[0];
                        var category = Category(difference);
                        if (category > 11) throw new ImageDataException("JPEG DC transform exceeds its coding range.");
                        frequencies![group][category]++;
                        cache![eventCount++] = PackEntropyEvent(group, category, difference, category);
                        eventCount = AppendAdaptiveAc(quantized, frequencies[group + 2], cache, eventCount, group + 2);
                    }
                }
            }
            if (!adaptive) writer!.Align();
            else
            {
                var tables = new JpegEncodingTable?[4];
                for (var t = 0; t < 4; t++)
                    if (frequencies![t].Any(value => value != 0)) tables[t] = JpegHuffmanBuilder.Build(frequencies[t], context);
                WriteSampleHeader(source, output, options, components, h, v, luma, chroma, tables);
                WriteSampleEvents(output, context, tables, cache!, eventCount, (int)blocks);
            }
            context.Cancellation.ThrowIfCancellationRequested(); output.Write(new byte[] { 255, 217 });
        }
        finally
        {
            if (cache is not null && cache.Length <= MaximumRetainedEvents) adaptiveEvents = cache;
        }
    }

    private static void SampleCallerBlock(JpegSampleInput source, int left, int top,
        int samplesX, int samplesY, int component, Span<byte> pixels)
    {
        if (source.Format == JpegSampleFormat.Grayscale) { source.CopyGrayBlock(left, top, pixels); return; }
        // Normalize one row only, including clamped edge taps. This workspace
        // fits the existing 8 KiB allowance and never becomes a pixel owner.
        Span<byte> topRow = stackalloc byte[48]; Span<byte> bottomRow = stackalloc byte[48];
        if (samplesX == 1)
        {
            for (var y = 0; y < 8; y++)
            {
                source.CopyRgbRow(left, top + y, topRow[..24]);
                SamplePackedRgbRow(topRow[..24], [], 1, 1, component, pixels.Slice(y * 8, 8));
            }
        }
        else
        {
            for (var y = 0; y < 8; y++)
            {
                source.CopyRgbRow(left, top + y * samplesY, topRow);
                if (samplesY == 2) source.CopyRgbRow(left, top + y * samplesY + 1, bottomRow);
                SamplePackedRgbRow(topRow, samplesY == 2 ? bottomRow : [], 2, samplesY,
                    component, pixels.Slice(y * 8, 8));
            }
        }
    }

    private static void SamplePackedRgbRow(ReadOnlySpan<byte> top, ReadOnlySpan<byte> bottom,
        int columns, int rows, int component, Span<byte> output)
    {
        if (Avx512F.IsSupported && Avx512F.VL.IsSupported && Avx2.IsSupported && Ssse3.IsSupported)
        {
            if (columns == 1) { JpegTransform.RoundSamples(PackedRgbValues8(top, component), output); return; }
            var even = Vector512.Create(0L, 2, 4, 6, 8, 10, 12, 14);
            var odd = Vector512.Create(1L, 3, 5, 7, 9, 11, 13, 15);
            var left = PackedRgbValues8(top[..24], component).AsInt64();
            var right = PackedRgbValues8(top.Slice(24, 24), component).AsInt64();
            // Preserve the owned dy/dx binary64 order, including the initial zero
            // addition. Round only after all unclipped chroma taps are averaged.
            var sum = Avx512F.Add(Vector512<double>.Zero, Avx512F.PermuteVar8x64x2(left, even, right).AsDouble());
            sum = Avx512F.Add(sum, Avx512F.PermuteVar8x64x2(left, odd, right).AsDouble());
            if (rows == 2)
            {
                left = PackedRgbValues8(bottom[..24], component).AsInt64();
                right = PackedRgbValues8(bottom.Slice(24, 24), component).AsInt64();
                sum = Avx512F.Add(sum, Avx512F.PermuteVar8x64x2(left, even, right).AsDouble());
                sum = Avx512F.Add(sum, Avx512F.PermuteVar8x64x2(left, odd, right).AsDouble());
            }
            JpegTransform.RoundSamples(Avx512F.Divide(sum, Vector512.Create(rows * 2d)), output);
            return;
        }
        for (var x = 0; x < 8; x++)
        {
            double sum = 0;
            for (var dy = 0; dy < rows; dy++)
            for (var dx = 0; dx < columns; dx++)
            {
                var row = dy == 0 ? top : bottom; var offset = (x * columns + dx) * 3;
                var r = row[offset]; var g = row[offset + 1]; var b = row[offset + 2];
                sum += component switch
                {
                    0 => 0.299 * r + 0.587 * g + 0.114 * b,
                    1 => 128 - 0.168735892 * r - 0.331264108 * g + 0.5 * b,
                    _ => 128 + 0.5 * r - 0.418687589 * g - 0.081312411 * b
                };
            }
            output[x] = (byte)Math.Clamp(Math.Round(sum / (rows * columns), MidpointRounding.AwayFromZero), 0, 255);
        }
    }

    private static Vector512<double> PackedRgbValues8(ReadOnlySpan<byte> rgb, int component)
    {
        // Two overlapping, in-bounds 16-byte loads cover exactly eight RGB triples.
        var first = Vector128.Create(rgb[..16]); var last = Vector128.Create(rgb.Slice(8, 16));
        var r = PackedChannel8(first, last, 0); var g = PackedChannel8(first, last, 1); var b = PackedChannel8(first, last, 2);
        return component switch
        {
            0 => Avx512F.Add(Avx512F.Add(Avx512F.Multiply(Vector512.Create(0.299), r), Avx512F.Multiply(Vector512.Create(0.587), g)), Avx512F.Multiply(Vector512.Create(0.114), b)),
            1 => Avx512F.Add(Avx512F.Subtract(Avx512F.Subtract(Vector512.Create(128d), Avx512F.Multiply(Vector512.Create(0.168735892), r)), Avx512F.Multiply(Vector512.Create(0.331264108), g)), Avx512F.Multiply(Vector512.Create(0.5), b)),
            _ => Avx512F.Subtract(Avx512F.Subtract(Avx512F.Add(Vector512.Create(128d), Avx512F.Multiply(Vector512.Create(0.5), r)), Avx512F.Multiply(Vector512.Create(0.418687589), g)), Avx512F.Multiply(Vector512.Create(0.081312411), b))
        };
    }

    private static Vector512<double> PackedChannel8(Vector128<byte> first, Vector128<byte> last, int channel)
    {
        var low = Ssse3.Shuffle(first, Vector128.Create((byte)channel, (byte)(channel + 3), (byte)(channel + 6), (byte)(channel + 9),
            128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128));
        var high = Ssse3.Shuffle(last, Vector128.Create((byte)(channel + 4), (byte)(channel + 7), (byte)(channel + 10), (byte)(channel + 13),
            128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128));
        var samples = Sse2.UnpackLow(low.AsUInt32(), high.AsUInt32()).AsByte();
        return Avx512F.ConvertToVector512Double(Avx2.ConvertToVector256Int32(samples));
    }

    private static void WriteSampleHeader(JpegSampleInput source, Stream output, JpegEncodeOptions options,
        int components, int h, int v, ushort[] luma, ushort[] chroma, JpegEncodingTable?[]? tables)
    {
        output.Write(new byte[] { 255, 216 });
        Segment(output, 0xE0, new byte[] { 74, 70, 73, 70, 0, 1, 1, 0, 0, 1, 0, 1, 0, 0 });
        WriteQuantization(output, luma, 0); if (components == 3) WriteQuantization(output, chroma, 1);
        var frame = new byte[6 + 3 * components]; frame[0] = 8;
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(1), (ushort)source.Height);
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(3), (ushort)source.Width); frame[5] = (byte)components;
        for (var c = 0; c < components; c++)
        {
            frame[6 + c * 3] = (byte)(c + 1); frame[7 + c * 3] = c == 0 ? (byte)(h << 4 | v) : (byte)17;
            frame[8 + c * 3] = c == 0 ? (byte)0 : (byte)1;
        }
        Segment(output, 0xC0, frame);
        if (tables is null)
        {
            WriteHuffman(output, 0, 4, Enumerable.Range(0, 12).Select(value => (byte)value).ToArray());
            var ac = FixedAc.Table; var payload = new byte[17 + ac.Symbols.Length]; payload[0] = 0x10;
            ac.Counts.CopyTo(payload, 1); ac.Symbols.CopyTo(payload, 17); Segment(output, 0xC4, payload);
        }
        else
        {
            for (var t = 0; t < 4; t++)
            {
                if (tables[t] is not { } table) continue;
                var payload = new byte[17 + table.Symbols.Length]; payload[0] = (byte)((t >= 2 ? 16 : 0) | (t & 1));
                table.Counts.CopyTo(payload, 1); table.Symbols.CopyTo(payload, 17); Segment(output, 0xC4, payload);
            }
        }
        if (options.RestartInterval != 0)
        {
            Span<byte> interval = stackalloc byte[2]; BinaryPrimitives.WriteUInt16BigEndian(interval, (ushort)options.RestartInterval);
            Segment(output, 0xDD, interval);
        }
        var scan = new byte[4 + components * 2]; scan[0] = (byte)components;
        for (var c = 0; c < components; c++) { scan[1 + c * 2] = (byte)(c + 1); scan[2 + c * 2] = c == 0 || tables is null ? (byte)0 : (byte)17; }
        scan[2 + components * 2] = 63; Segment(output, 0xDA, scan);
    }

    private static void WriteSampleEvents(Stream output, OperationContext context,
        JpegEncodingTable?[] tables, uint[] events, int count, int blocks)
    {
        var combineWords = (long)context.Limits.MaxOutputBytes >= blocks * 432L + 2048;
        var writer = new JpegEntropyWriter(output, context, combineWords ? blocks * 432L + 2048 : 0); var restart = 0;
        for (var i = 0; i < count; i++)
        {
            var entry = events[i];
            if ((entry & (1u << 24)) != 0) { writer.RestartAdmitted(restart++); continue; }
            var tableIndex = (int)(entry >> 30); var symbol = (int)(entry >> 16 & 255); var amplitude = (int)(entry >> 26 & 15);
            var table = tables[tableIndex]!; var length = table.Lengths[symbol];
            var hasAmplitude = tableIndex < 2 || amplitude != 0;
            if (combineWords && hasAmplitude && length + amplitude <= 16)
            {
                context.Work(1);
                writer.WriteAdmitted(table.Codes[symbol] << amplitude | (int)(entry & 65535), length + amplitude);
            }
            else
            {
                writer.WriteAdmitted(table.Codes[symbol], length);
                if (hasAmplitude) writer.WriteAdmitted((int)(entry & 65535), amplitude);
            }
        }
        writer.AlignAdmitted();
    }
}
