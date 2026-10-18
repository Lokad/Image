using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal static partial class JpegEncoder
{

    public static void Encode(ImageBuffer image, Stream output, JpegEncodeOptions options, OperationContext context)
    {
        if (CanUseAdaptive(image, output, options, context))
        { EncodeAdaptive(image, output, options, context); return; }
        if (options.Quality is < 1 or > 100 || !Enum.IsDefined(options.Sampling)
            || options.RestartInterval is < 0 or > 65535 || options.BackgroundRgb > 0xFFFFFF)
            throw new ArgumentOutOfRangeException(nameof(options));
        context.PixelBytes(image.Width, image.Height, image.BitDepth);
        if (image.Width > 65535 || image.Height > 65535) throw new ImageLimitException(ImageLimitKind.Dimension, "JPEG dimensions exceed 65535.");
        context.Reserve(image.ByteLength);
        // Fixed table/scratch allocation is admitted before any header is written.
        context.Reserve(8192);
        var components = options.Sampling == JpegSampling.Grayscale ? 1 : 3;
        var h = options.Sampling is JpegSampling.YCbCr420 or JpegSampling.YCbCr422 ? 2 : 1;
        var v = options.Sampling == JpegSampling.YCbCr420 ? 2 : 1;
        var columns = (image.Width + h * 8 - 1) / (h * 8); var rows = (image.Height + v * 8 - 1) / (v * 8);
        var luma = Quantization(options.Quality, false); var chroma = Quantization(options.Quality, true);
        var dcSymbols = Enumerable.Range(0, 12).Select(value => (byte)value).ToArray();
        var ac = FixedAc.Table;
        var acWords = FixedAc.Words;
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
        WriteHuffman(output, 0, 4, dcSymbols);
        var acPayload = new byte[17 + ac.Symbols.Length]; acPayload[0] = 0x10;
        ac.Counts.CopyTo(acPayload, 1); ac.Symbols.CopyTo(acPayload, 17); Segment(output, 0xC4, acPayload);
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
                    if (image.BitDepth == 8 && samplesX == 1 && samplesY == 1 && sourceLeft <= image.Width - 8
                        && Avx512F.IsSupported && Avx512F.VL.IsSupported && Avx2.IsSupported)
                    {
                        for (var y = 0; y < 8; y++)
                            SampleComponentRow8(image.GetRow8(Math.Min(sourceTop + y, image.Height - 1)).Slice(sourceLeft * 4, 32),
                                component, options.BackgroundRgb, pixels.AsSpan(y * 8, 8));
                    }
                    else if (image.BitDepth == 16 && samplesX == 1 && samplesY == 1 && sourceLeft <= image.Width - 8
                        && Avx512F.IsSupported && Avx512DQ.IsSupported)
                    {
                        for (var y = 0; y < 8; y++)
                            SampleComponentRow16(image.GetRow16(Math.Min(sourceTop + y, image.Height - 1)).Slice(sourceLeft * 4, 32),
                                component, options.BackgroundRgb, pixels.AsSpan(y * 8, 8));
                    }
                    else if (image.BitDepth == 8 && samplesX == 2 && samplesY is 1 or 2 && sourceLeft <= image.Width - 16
                        && Avx512F.IsSupported && Avx512F.VL.IsSupported && Avx2.IsSupported)
                    {
                        for (var y = 0; y < 8; y++)
                        {
                            var top = image.GetRow8(Math.Min(sourceTop + y * samplesY, image.Height - 1)).Slice(sourceLeft * 4, 64);
                            ReadOnlySpan<byte> bottom = samplesY == 2
                                ? image.GetRow8(Math.Min(sourceTop + y * samplesY + 1, image.Height - 1)).Slice(sourceLeft * 4, 64)
                                : [];
                            SampleSubsampledComponentRow8(top, bottom, samplesY, component, options.BackgroundRgb, pixels.AsSpan(y * 8, 8));
                        }
                    }
                    else if (image.BitDepth == 16 && samplesX == 2 && samplesY is 1 or 2 && sourceLeft <= image.Width - 16
                        && Avx512F.IsSupported && Avx512DQ.IsSupported)
                    {
                        for (var y = 0; y < 8; y++)
                        {
                            var top = image.GetRow16(Math.Min(sourceTop + y * samplesY, image.Height - 1)).Slice(sourceLeft * 4, 64);
                            ReadOnlySpan<ushort> bottom = samplesY == 2
                                ? image.GetRow16(Math.Min(sourceTop + y * samplesY + 1, image.Height - 1)).Slice(sourceLeft * 4, 64)
                                : [];
                            SampleSubsampledComponentRow16(top, bottom, samplesY, component, options.BackgroundRgb, pixels.AsSpan(y * 8, 8));
                        }
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
                    if (!JpegTransform.TryForwardQuantized(pixels, q, quantized, context))
                    {
                        JpegTransform.Forward(pixels, transformed, context);
                        Quantize(transformed, q, quantized);
                    }
                    WriteFixedBlock(quantized, ref predictors[component], writer, acWords);
                }
            }
        }
        writer.Align(); context.Cancellation.ThrowIfCancellationRequested(); output.Write(new byte[] { 255, 217 });
    }

    internal static void WriteFixedBlock(ReadOnlySpan<int> quantized, ref int predictor, JpegEntropyWriter writer, uint[] words)
    {
        var difference = quantized[0] - predictor; predictor = quantized[0];
        var category = Category(difference);
        if (category > 11) throw new ImageDataException("JPEG DC transform exceeds its coding range.");
        writer.WriteCodeword((uint)(4 << 16 | category)); writer.Signed(difference, category);
        if (Avx2.IsSupported && Bmi2.X64.IsSupported)
        {
            // T.81's zigzag visits each raster row in increasing column order.
            // Deposit its eight nonzero flags into their scan positions. This
            // skips zero coefficients without gathering or reordering samples.
            ulong occupied = 0;
            var masks = FixedScanMasks.Rows;
            for (var row = 0; row < 8; row++)
            {
                var values = Vector256.Create(quantized.Slice(row * 8, 8));
                var nonzero = ~Vector256.ExtractMostSignificantBits(Vector256.Equals(values, Vector256<int>.Zero)) & 255;
                occupied |= Bmi2.X64.ParallelBitDeposit(nonzero, masks[row]);
            }
            occupied &= ~1UL; // DC was emitted above.
            writer.WriteFixedAc(quantized, occupied, words);
            return;
        }
        var zeros = 0;
        for (var i = 1; i < 64; i++)
        {
            var value = quantized[JpegTransform.Zigzag[i]];
            if (value == 0) { zeros++; continue; }
            while (zeros >= 16) { var word = words[0xF0]; writer.WriteCodeword(word); zeros -= 16; }
            category = Category(value);
            if (category > 10) throw new ImageDataException("JPEG AC transform exceeds its coding range.");
            var symbol = zeros << 4 | category;
            var codeword = words[symbol];
            writer.WriteCodeword(codeword); writer.Signed(value, category); zeros = 0;
        }
        if (zeros != 0) { var word = words[0]; writer.WriteCodeword(word); }
    }

    private static class FixedScanMasks
    {
        // Generated from the independently constructed T.81 zigzag, not an
        // implementation table. Eight format constants, no per-image storage.
        internal static readonly ulong[] Rows = Build();
        private static ulong[] Build()
        {
            var rows = new ulong[8];
            for (var scan = 0; scan < 64; scan++) rows[JpegTransform.Zigzag[scan] / 8] |= 1UL << scan;
            return rows;
        }
    }

    private static bool CanUseAdaptive(ImageBuffer image, Stream output, JpegEncodeOptions options, OperationContext context)
    {
        // The original path retains tight-limit, cancellable and caller-stream
        // failure sequencing. This first cache path requires explicit flattening.
        if (context.Cancellation.CanBeCanceled || options.BackgroundRgb is null or > 0xFFFFFF
            || options.Quality is < 1 or > 100 || !Enum.IsDefined(options.Sampling)
            || options.RestartInterval is < 0 or > 65535) return false;
        var width = image.Width; var height = image.Height;
        if (width > 65535 || height > 65535 || width > context.Limits.MaxDimension
            || height > context.Limits.MaxDimension || (long)width * height > context.Limits.MaxPixels) return false;
        var h = options.Sampling is JpegSampling.YCbCr420 or JpegSampling.YCbCr422 ? 2 : 1;
        var v = options.Sampling == JpegSampling.YCbCr420 ? 2 : 1;
        var components = options.Sampling == JpegSampling.Grayscale ? 1 : 3;
        var mcus = ((width + h * 8L - 1) / (h * 8)) * ((height + v * 8L - 1) / (v * 8));
        var blocks = mcus * (h * v + components - 1);
        // Small images do not amortize per-image table construction and caching.
        if (blocks < 256) return false;
        var cacheSamples = blocks * 64 + (options.RestartInterval == 0 ? 0 : mcus);
        var work = 2 * mcus + mcus * (h * v * (4096 + 768) + (components - 1) * (4096 + 768 * h * v))
            + blocks * 192 + 262145;
        var memory = image.ByteLength + 8192L + cacheSamples * sizeof(uint) + 65536;
        var outputMaximum = blocks * 432 + 2048;
        return cacheSamples <= Array.MaxLength && work <= context.RemainingWork && memory <= context.RemainingWorkingBytes
            && output is LimitedWriteStream limited && limited.SupportsAdaptiveJpeg(outputMaximum);
    }

    private static void EncodeAdaptive(ImageBuffer image, Stream output, JpegEncodeOptions options, OperationContext context)
    {
        context.PixelBytes(image.Width, image.Height, image.BitDepth);
        context.Reserve(image.ByteLength); context.Reserve(8192);
        var components = options.Sampling == JpegSampling.Grayscale ? 1 : 3;
        var h = options.Sampling is JpegSampling.YCbCr420 or JpegSampling.YCbCr422 ? 2 : 1;
        var v = options.Sampling == JpegSampling.YCbCr420 ? 2 : 1;
        var columns = (image.Width + h * 8 - 1) / (h * 8); var rows = (image.Height + v * 8 - 1) / (v * 8);
        var blockCount = checked(columns * rows * (h * v + components - 1));
        var eventCapacity = checked(blockCount * 64 + (options.RestartInterval == 0 ? 0 : columns * rows));
        context.Reserve(eventCapacity * 4L + 65536);
        var cache = adaptiveEvents; adaptiveEvents = null;
        if (cache is null || cache.Length != eventCapacity) cache = new uint[eventCapacity];
        // Optional scratch leaves the original adaptive admission boundary intact.
        var pairedChroma = image.BitDepth == 8 && image.Width >= 16 && h == 2 && v == 2
            && Avx512F.IsSupported && Avx512F.VL.IsSupported && Avx2.IsSupported
            && context.RemainingWorkingBytes >= 64;
        if (pairedChroma) context.Reserve(64);
        Span<byte> crPixels = pairedChroma ? stackalloc byte[64] : [];

        try
        {
            var frequencies = new long[4][]; for (var t = 0; t < 4; t++) frequencies[t] = new long[256];
            var luma = Quantization(options.Quality, false); var chroma = Quantization(options.Quality, true);
            var pixels = new byte[64]; var transformed = new double[64]; var predictors = new int[components];
            Span<int> quantized = stackalloc int[64]; var eventCount = 0;
            for (var mcu = 0; mcu < columns * rows; mcu++)
            {
                context.Work(1);
                var crReady = false;
                if (options.RestartInterval != 0 && mcu != 0 && mcu % options.RestartInterval == 0)
                { cache[eventCount++] = 1u << 24; Array.Clear(predictors); }
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
                        if (component == 2 && crReady) crPixels.CopyTo(pixels);
                        else if (image.BitDepth == 8 && samplesX == 1 && samplesY == 1 && sourceLeft <= image.Width - 8
                            && Avx512F.IsSupported && Avx512F.VL.IsSupported && Avx2.IsSupported)
                        {
                            for (var y = 0; y < 8; y++)
                                SampleComponentRow8(image.GetRow8(Math.Min(sourceTop + y, image.Height - 1)).Slice(sourceLeft * 4, 32),
                                    component, options.BackgroundRgb, pixels.AsSpan(y * 8, 8));
                        }
                        else if (image.BitDepth == 16 && samplesX == 1 && samplesY == 1 && sourceLeft <= image.Width - 8
                            && Avx512F.IsSupported && Avx512DQ.IsSupported)
                        {
                            for (var y = 0; y < 8; y++)
                                SampleComponentRow16(image.GetRow16(Math.Min(sourceTop + y, image.Height - 1)).Slice(sourceLeft * 4, 32),
                                    component, options.BackgroundRgb, pixels.AsSpan(y * 8, 8));
                        }
                        else if (image.BitDepth == 8 && samplesX == 2 && samplesY is 1 or 2 && sourceLeft <= image.Width - 16
                            && Avx512F.IsSupported && Avx512F.VL.IsSupported && Avx2.IsSupported)
                        {
                            for (var y = 0; y < 8; y++)
                            {
                                var top = image.GetRow8(Math.Min(sourceTop + y * samplesY, image.Height - 1)).Slice(sourceLeft * 4, 64);
                                ReadOnlySpan<byte> bottom = samplesY == 2
                                    ? image.GetRow8(Math.Min(sourceTop + y * samplesY + 1, image.Height - 1)).Slice(sourceLeft * 4, 64)
                                    : [];
                                if (pairedChroma && component == 1)
                                {
                                    SampleSubsampledChromaPairRow8(top, bottom, samplesY, options.BackgroundRgb,
                                        pixels.AsSpan(y * 8, 8), crPixels.Slice(y * 8, 8));
                                    crReady = true;
                                }
                                else SampleSubsampledComponentRow8(top, bottom, samplesY, component, options.BackgroundRgb, pixels.AsSpan(y * 8, 8));
                            }
                        }
                        else if (image.BitDepth == 16 && samplesX == 2 && samplesY is 1 or 2 && sourceLeft <= image.Width - 16
                            && Avx512F.IsSupported && Avx512DQ.IsSupported)
                        {
                            for (var y = 0; y < 8; y++)
                            {
                                var top = image.GetRow16(Math.Min(sourceTop + y * samplesY, image.Height - 1)).Slice(sourceLeft * 4, 64);
                                ReadOnlySpan<ushort> bottom = samplesY == 2
                                    ? image.GetRow16(Math.Min(sourceTop + y * samplesY + 1, image.Height - 1)).Slice(sourceLeft * 4, 64)
                                    : [];
                                SampleSubsampledComponentRow16(top, bottom, samplesY, component, options.BackgroundRgb, pixels.AsSpan(y * 8, 8));
                            }
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
                        if (!JpegTransform.TryForwardQuantized(pixels, q, quantized, context))
                        {
                            JpegTransform.Forward(pixels, transformed, context);
                            Quantize(transformed, q, quantized);
                        }
                        context.Work(64);
                        var group = component == 0 ? 0 : 1;
                        var difference = quantized[0] - predictors[component]; predictors[component] = quantized[0];
                        var category = Category(difference);
                        if (category > 11) throw new ImageDataException("JPEG DC transform exceeds its coding range.");
                        frequencies[group][category]++;
                        cache[eventCount++] = PackEntropyEvent(group, category, difference, category);
                        eventCount = AppendAdaptiveAc(quantized, frequencies[group + 2], cache, eventCount, group + 2);
                    }
                }
            }
            var tables = new JpegEncodingTable?[4];
            for (var t = 0; t < 4; t++) if (frequencies[t].Any(x => x != 0)) tables[t] = JpegHuffmanBuilder.Build(frequencies[t], context);
            output.Write(new byte[] { 255, 216 });
            Segment(output, 0xE0, new byte[] { 74, 70, 73, 70, 0, 1, 1, 0, 0, 1, 0, 1, 0, 0 });
            WriteQuantization(output, luma, 0); if (components == 3) WriteQuantization(output, chroma, 1);
            var frame = new byte[6 + 3 * components]; frame[0] = 8;
            BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(1), (ushort)image.Height);
            BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(3), (ushort)image.Width); frame[5] = (byte)components;
            for (var c = 0; c < components; c++)
            {
                frame[6 + c * 3] = (byte)(c + 1); frame[7 + c * 3] = c == 0 ? (byte)(h << 4 | v) : (byte)17;
                frame[8 + c * 3] = c == 0 ? (byte)0 : (byte)1;
            }
            Segment(output, 0xC0, frame);
            for (var t = 0; t < 4; t++)
            {
                if (tables[t] is not { } table) continue;
                var payload = new byte[17 + table.Symbols.Length]; payload[0] = (byte)((t >= 2 ? 16 : 0) | (t & 1));
                table.Counts.CopyTo(payload, 1); table.Symbols.CopyTo(payload, 17); Segment(output, 0xC4, payload);
            }
            if (options.RestartInterval != 0)
            {
                Span<byte> interval = stackalloc byte[2]; BinaryPrimitives.WriteUInt16BigEndian(interval, (ushort)options.RestartInterval);
                Segment(output, 0xDD, interval);
            }
            var scan = new byte[4 + components * 2]; scan[0] = (byte)components;
            for (var c = 0; c < components; c++) { scan[1 + c * 2] = (byte)(c + 1); scan[2 + c * 2] = c == 0 ? (byte)0 : (byte)17; }
            scan[2 + components * 2] = 63; Segment(output, 0xDA, scan);
            var combineWords = (long)context.Limits.MaxOutputBytes >= blockCount * 432L + 2048;
            var writer = new JpegEntropyWriter(output, context, combineWords ? blockCount * 432L + 2048 : 0); var restart = 0;
            for (var i = 0; i < eventCount; i++)
            {
                var entry = cache[i];
                if ((entry & (1u << 24)) != 0) { writer.RestartAdmitted(restart++); continue; }
                var tableIndex = (int)(entry >> 30); var symbol = (int)(entry >> 16 & 255); var count = (int)(entry >> 26 & 15);
                var table = tables[tableIndex]!; var length = table.Lengths[symbol];
                var hasAmplitude = tableIndex < 2 || count != 0;
                if (combineWords && hasAmplitude && length + count <= 16)
                {
                    // Admission already guarantees both original work charges
                    // and output capacity on a noncancellable stock stream.
                    // Append one complete symbol without extra per-symbol guards.
                    context.Work(1);
                    writer.WriteAdmitted(table.Codes[symbol] << count | (int)(entry & 65535), length + count);
                }
                else
                {
                    writer.WriteAdmitted(table.Codes[symbol], length);
                    if (hasAmplitude) writer.WriteAdmitted((int)(entry & 65535), count);
                }
            }
            writer.AlignAdmitted(); context.Cancellation.ThrowIfCancellationRequested(); output.Write(new byte[] { 255, 217 });
        }
        finally
        {
            if (pairedChroma) context.Release(64);
            if (cache.Length <= MaximumRetainedEvents) adaptiveEvents = cache;
        }
    }

    // Keep T.81 F.1.2.2 run planning separate from sampling and transforms.
    // Its histogram, scan order and event cursor then stay local to this loop.
    // Work was charged for the whole block before entering; range checks still
    // occur after preceding zero-run events, in the original traversal order.
    private static int AppendAdaptiveAc(ReadOnlySpan<int> quantized, long[] frequencies, uint[] events, int eventCount, int table)
    {
        if (Avx512F.IsSupported && Avx512CD.IsSupported && Bmi2.X64.IsSupported
            && events.Length - eventCount >= 128)
            return AppendPreparedAdaptiveAc(quantized, frequencies, events, eventCount, table);
        var order = JpegTransform.Zigzag;
        if (Avx2.IsSupported && Bmi2.X64.IsSupported)
        {
            // Reuse the fixed encoder's T.81 scan occupancy: skip zero runs
            // while retaining histogram/event order, including range failures.
            ulong occupied = 0;
            var masks = FixedScanMasks.Rows;
            for (var row = 0; row < 8; row++)
            {
                var values = Vector256.Create(quantized.Slice(row * 8, 8));
                var nonzero = ~Vector256.ExtractMostSignificantBits(Vector256.Equals(values, Vector256<int>.Zero)) & 255;
                occupied |= Bmi2.X64.ParallelBitDeposit(nonzero, masks[row]);
            }
            occupied &= ~1UL;
            var previous = 0;
            while (occupied != 0)
            {
                var scan = BitOperations.TrailingZeroCount(occupied);
                var value = quantized[order[scan]];
                var run = scan - previous - 1;
                while (run >= 16)
                {
                    frequencies[240]++; events[eventCount++] = PackEntropyEvent(table, 240, 0, 0); run -= 16;
                }
                var category = Category(value);
                if (category > 10) throw new ImageDataException("JPEG AC transform exceeds its coding range.");
                var symbol = run << 4 | category;
                frequencies[symbol]++;
                events[eventCount++] = PackEntropyEvent(table, symbol, value, category);
                previous = scan; occupied &= occupied - 1;
            }
            if (previous != 63) { frequencies[0]++; events[eventCount++] = PackEntropyEvent(table, 0, 0, 0); }
            return eventCount;
        }
        var zeros = 0;
        for (var i = 1; i < 64; i++)
        {
            var value = quantized[order[i]];
            if (value == 0) { zeros++; continue; }
            while (zeros >= 16)
            {
                frequencies[240]++; events[eventCount++] = PackEntropyEvent(table, 240, 0, 0); zeros -= 16;
            }
            var category = Category(value);
            if (category > 10) throw new ImageDataException("JPEG AC transform exceeds its coding range.");
            var symbol = zeros << 4 | category;
            frequencies[symbol]++;
            events[eventCount++] = PackEntropyEvent(table, symbol, value, category); zeros = 0;
        }
        if (zeros != 0) { frequencies[0]++; events[eventCount++] = PackEntropyEvent(table, 0, 0, 0); }
        return eventCount;
    }

    private static int AppendPreparedAdaptiveAc(ReadOnlySpan<int> quantized, long[] frequencies,
        uint[] events, int eventCount, int table)
    {
        // Each AC event consumes at least one of63 scan positions. The caller
        // admits128 free entries:64 prepared words cannot overlap appended events.
        var scratch = events.AsSpan(events.Length - 64, 64);
        var masks = FixedScanMasks.Rows; ulong occupied = 0;
        for (var chunk = 0; chunk < 4; chunk++)
        {
            var values = Vector512.Create(quantized.Slice(chunk * 16, 16));
            var categories = Vector512.Subtract(Vector512.Create(32u),
                Avx512CD.LeadingZeroCount(Vector512.Abs(values).AsUInt32()));
            var complements = Vector512.Subtract(Avx512F.ShiftLeftLogicalVariable(Vector512.Create(1u), categories), Vector512.Create(1u));
            var amplitudes = Vector512.Add(values.AsUInt32(), Vector512.BitwiseAnd(complements,
                Vector512.LessThan(values, Vector512<int>.Zero).AsUInt32()));
            var prepared = Vector512.BitwiseOr(Vector512.ShiftLeft(categories, 26),
                Vector512.BitwiseAnd(amplitudes, Vector512.Create(65535u)));
            prepared.CopyTo(scratch.Slice(chunk * 16, 16));
            var nonzero = ~Vector512.ExtractMostSignificantBits(Vector512.Equals(values, Vector512<int>.Zero)) & 65535u;
            occupied |= Bmi2.X64.ParallelBitDeposit(nonzero & 255u, masks[chunk * 2]);
            occupied |= Bmi2.X64.ParallelBitDeposit(nonzero >> 8, masks[chunk * 2 + 1]);
        }
        occupied &= ~1UL; var previous = 0; var order = JpegTransform.Zigzag;
        while (occupied != 0)
        {
            var scan = BitOperations.TrailingZeroCount(occupied); var natural = order[scan];
            var word = scratch[natural]; var category = (int)(word >> 26);
            var run = scan - previous - 1;
            while (run >= 16)
            {
                frequencies[240]++; events[eventCount++] = PackEntropyEvent(table, 240, 0, 0); run -= 16;
            }
            if (category > 10)
            {
                // Retain the original absolute-value overflow and range error
                // after all preceding zero-run effects, not during vector prep.
                _ = Category(quantized[natural]);
                throw new ImageDataException("JPEG AC transform exceeds its coding range.");
            }
            var symbol = run << 4 | category; frequencies[symbol]++;
            events[eventCount++] = (uint)table << 30 | (uint)symbol << 16 | word;
            previous = scan; occupied &= occupied - 1;
        }
        if (previous != 63) { frequencies[0]++; events[eventCount++] = PackEntropyEvent(table, 0, 0, 0); }
        return eventCount;
    }

    private static uint PackEntropyEvent(int table, int symbol, int value, int count)
        => (uint)table << 30 | (uint)count << 26 | (uint)symbol << 16
            | (uint)(value < 0 ? value + ((1 << count) - 1) : value);

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

    internal static void SampleComponentRow16(ReadOnlySpan<ushort> rgba, int component, uint? background, Span<byte> pixels)
    {
        if (rgba.Length != 32 || pixels.Length != 8 || (uint)component > 2)
            throw new ArgumentException("JPEG component rows have eight RGBA pixels and a valid component.");
        var color = background ?? 0;
        if (Avx512F.IsSupported && Avx512DQ.IsSupported)
        {
            var packed = Vector512.Create(rgba).AsUInt64(); var mask = Vector512.Create(65535UL);
            var alphaWords = Avx512F.ShiftRightLogical(packed, 48);
            if (background is null && !Vector512.EqualsAll(alphaWords, mask))
                throw new ArgumentException("JPEG requires opaque pixels or an explicit RGB background.", "image");
            var alpha = Avx512DQ.ConvertToVector512Double(alphaWords);
            var inverseScaled = Avx512F.Multiply(Avx512F.Subtract(Vector512.Create(65535d), alpha), Vector512.Create(65535d));
            var r = BlendRow16(Avx512F.And(packed, mask), alpha, inverseScaled, (int)(color >> 16 & 255));
            var g = BlendRow16(Avx512F.And(Avx512F.ShiftRightLogical(packed, 16), mask), alpha, inverseScaled, (int)(color >> 8 & 255));
            var b = BlendRow16(Avx512F.And(Avx512F.ShiftRightLogical(packed, 32), mask), alpha, inverseScaled, (int)(color & 255));
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
            if (alpha != 65535 && background is null)
                throw new ArgumentException("JPEG requires opaque pixels or an explicit RGB background.", "image");
            var r = Flatten(pixel[0], alpha, (int)(color >> 16 & 255), 65535);
            var g = Flatten(pixel[1], alpha, (int)(color >> 8 & 255), 65535);
            var b = Flatten(pixel[2], alpha, (int)(color & 255), 65535);
            var sample = component switch
            {
                0 => 0.299 * r + 0.587 * g + 0.114 * b,
                1 => 128 - 0.168735892 * r - 0.331264108 * g + 0.5 * b,
                _ => 128 + 0.5 * r - 0.418687589 * g - 0.081312411 * b
            };
            pixels[x] = (byte)Math.Clamp(Math.Round(sample, MidpointRounding.AwayFromZero), 0, 255);
        }
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static Vector512<double> BlendRow16(Vector512<ulong> values, Vector512<double> alpha,
        Vector512<double> inverseScaled, int background)
    {
        // SPEC.md: one rounding after the full encoded-space blend. Every
        // product/sum is an integer below 2^40, hence exact in binary64. The
        // adjusted numerator divided by odd 65535^2 is at least 1/65535^2
        // from an integer unless exact; division error is below 2^-43.
        // Flooring therefore reproduces the integer quotient for every input.
        var weighted = Avx512F.Multiply(Avx512DQ.ConvertToVector512Double(values), alpha);
        var numerator = Avx512F.Add(Avx512F.Multiply(weighted, Vector512.Create(255d)),
            Avx512F.Multiply(inverseScaled, Vector512.Create((double)background)));
        return Avx512F.RoundScale(Avx512F.Divide(Avx512F.Add(numerator, Vector512.Create(2147418112d)),
            Vector512.Create(4294836225d)), 1);
    }

    internal static void SampleSubsampledComponentRow16(ReadOnlySpan<ushort> top, ReadOnlySpan<ushort> bottom,
        int rows, int component, uint? background, Span<byte> pixels)
    {
        if (top.Length != 64 || pixels.Length != 8 || rows is not (1 or 2) || component is not (1 or 2)
            || (rows == 2 ? bottom.Length != 64 : !bottom.IsEmpty))
            throw new ArgumentException("JPEG chroma rows have sixteen RGBA pixels, one or two source rows and eight outputs.");
        if (Avx512F.IsSupported && Avx512DQ.IsSupported)
        {
            var even = Vector512.Create(0L, 2, 4, 6, 8, 10, 12, 14);
            var odd = Vector512.Create(1L, 3, 5, 7, 9, 11, 13, 15);
            var left = ChromaValuesRow16(top[..32], component, background).AsInt64();
            var right = ChromaValuesRow16(top[32..], component, background).AsInt64();
            // Preserve dy/dx traversal, binary64 matrix order and the original
            // zero-initialized sum. Round only after all unrounded taps average.
            var sum = Avx512F.Add(Vector512<double>.Zero, Avx512F.PermuteVar8x64x2(left, even, right).AsDouble());
            sum = Avx512F.Add(sum, Avx512F.PermuteVar8x64x2(left, odd, right).AsDouble());
            if (rows == 2)
            {
                left = ChromaValuesRow16(bottom[..32], component, background).AsInt64();
                right = ChromaValuesRow16(bottom[32..], component, background).AsInt64();
                sum = Avx512F.Add(sum, Avx512F.PermuteVar8x64x2(left, even, right).AsDouble());
                sum = Avx512F.Add(sum, Avx512F.PermuteVar8x64x2(left, odd, right).AsDouble());
            }
            JpegTransform.RoundSamples(Avx512F.Divide(sum, Vector512.Create(rows * 2d)), pixels);
            return;
        }
        var color = background ?? 0;
        for (var x = 0; x < 8; x++)
        {
            double sum = 0;
            for (var dy = 0; dy < rows; dy++)
            for (var dx = 0; dx < 2; dx++)
            {
                var pixel = (dy == 0 ? top : bottom).Slice((x * 2 + dx) * 4, 4);
                var alpha = pixel[3];
                if (alpha != 65535 && background is null)
                    throw new ArgumentException("JPEG requires opaque pixels or an explicit RGB background.", "image");
                var r = Flatten(pixel[0], alpha, (int)(color >> 16 & 255), 65535);
                var g = Flatten(pixel[1], alpha, (int)(color >> 8 & 255), 65535);
                var b = Flatten(pixel[2], alpha, (int)(color & 255), 65535);
                sum += component == 1 ? 128 - 0.168735892 * r - 0.331264108 * g + 0.5 * b
                    : 128 + 0.5 * r - 0.418687589 * g - 0.081312411 * b;
            }
            pixels[x] = (byte)Math.Clamp(Math.Round(sum / (rows * 2), MidpointRounding.AwayFromZero), 0, 255);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector512<double> ChromaValuesRow16(ReadOnlySpan<ushort> rgba, int component, uint? background)
    {
        var color = background ?? 0;
        var packed = Vector512.Create(rgba).AsUInt64(); var mask = Vector512.Create(65535UL);
        var alphaWords = Avx512F.ShiftRightLogical(packed, 48);
        if (background is null && !Vector512.EqualsAll(alphaWords, mask))
            throw new ArgumentException("JPEG requires opaque pixels or an explicit RGB background.", "image");
        var alpha = Avx512DQ.ConvertToVector512Double(alphaWords);
        var inverseScaled = Avx512F.Multiply(Avx512F.Subtract(Vector512.Create(65535d), alpha), Vector512.Create(65535d));
        var r = BlendRow16(Avx512F.And(packed, mask), alpha, inverseScaled, (int)(color >> 16 & 255));
        var g = BlendRow16(Avx512F.And(Avx512F.ShiftRightLogical(packed, 16), mask), alpha, inverseScaled, (int)(color >> 8 & 255));
        var b = BlendRow16(Avx512F.And(Avx512F.ShiftRightLogical(packed, 32), mask), alpha, inverseScaled, (int)(color & 255));
        return component == 1
            ? Avx512F.Add(Avx512F.Subtract(Avx512F.Subtract(Vector512.Create(128d), Avx512F.Multiply(Vector512.Create(0.168735892), r)), Avx512F.Multiply(Vector512.Create(0.331264108), g)), Avx512F.Multiply(Vector512.Create(0.5), b))
            : Avx512F.Subtract(Avx512F.Subtract(Avx512F.Add(Vector512.Create(128d), Avx512F.Multiply(Vector512.Create(0.5), r)), Avx512F.Multiply(Vector512.Create(0.418687589), g)), Avx512F.Multiply(Vector512.Create(0.081312411), b));
    }

    internal static void SampleSubsampledComponentRow8(ReadOnlySpan<byte> top, ReadOnlySpan<byte> bottom,
        int rows, int component, uint? background, Span<byte> pixels)
    {
        if (top.Length != 64 || pixels.Length != 8 || rows is not (1 or 2) || component is not (1 or 2)
            || (rows == 2 ? bottom.Length != 64 : !bottom.IsEmpty))
            throw new ArgumentException("JPEG chroma rows have sixteen RGBA pixels, one or two source rows and eight outputs.");
        if (Avx512F.IsSupported && Avx512F.VL.IsSupported && Avx2.IsSupported)
        {
            var even = Vector512.Create(0L, 2, 4, 6, 8, 10, 12, 14);
            var odd = Vector512.Create(1L, 3, 5, 7, 9, 11, 13, 15);
            var left = ChromaValuesRow8(top[..32], component, background).AsInt64();
            var right = ChromaValuesRow8(top[32..], component, background).AsInt64();
            // Preserve dy/dx traversal, binary64 matrix order and the original
            // zero-initialized sum. Round only after all unrounded taps average.
            var sum = Avx512F.Add(Vector512<double>.Zero, Avx512F.PermuteVar8x64x2(left, even, right).AsDouble());
            sum = Avx512F.Add(sum, Avx512F.PermuteVar8x64x2(left, odd, right).AsDouble());
            if (rows == 2)
            {
                left = ChromaValuesRow8(bottom[..32], component, background).AsInt64();
                right = ChromaValuesRow8(bottom[32..], component, background).AsInt64();
                sum = Avx512F.Add(sum, Avx512F.PermuteVar8x64x2(left, even, right).AsDouble());
                sum = Avx512F.Add(sum, Avx512F.PermuteVar8x64x2(left, odd, right).AsDouble());
            }
            JpegTransform.RoundSamples(Avx512F.Divide(sum, Vector512.Create(rows * 2d)), pixels);
            return;
        }
        var color = background ?? 0;
        for (var x = 0; x < 8; x++)
        {
            double sum = 0;
            for (var dy = 0; dy < rows; dy++)
            for (var dx = 0; dx < 2; dx++)
            {
                var pixel = (dy == 0 ? top : bottom).Slice((x * 2 + dx) * 4, 4);
                var alpha = pixel[3];
                if (alpha != 255 && background is null)
                    throw new ArgumentException("JPEG requires opaque pixels or an explicit RGB background.", "image");
                var r = Flatten8(pixel[0], alpha, (int)(color >> 16 & 255));
                var g = Flatten8(pixel[1], alpha, (int)(color >> 8 & 255));
                var b = Flatten8(pixel[2], alpha, (int)(color & 255));
                sum += component == 1 ? 128 - 0.168735892 * r - 0.331264108 * g + 0.5 * b
                    : 128 + 0.5 * r - 0.418687589 * g - 0.081312411 * b;
            }
            pixels[x] = (byte)Math.Clamp(Math.Round(sum / (rows * 2), MidpointRounding.AwayFromZero), 0, 255);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector512<double> ChromaValuesRow8(ReadOnlySpan<byte> rgba, int component, uint? background)
    {
        var color = background ?? 0;
        var packed = Vector256.Create(rgba).AsInt32(); var mask = Vector256.Create(255);
        var alpha = Avx2.ShiftRightLogical(packed.AsUInt32(), 24).AsInt32();
        if (background is null && Avx2.MoveMask(Avx2.CompareEqual(alpha, mask).AsByte()) != -1)
            throw new ArgumentException("JPEG requires opaque pixels or an explicit RGB background.", "image");
        var inverse = Avx2.Subtract(mask, alpha);
        var r = Avx512F.ConvertToVector512Double(BlendRow8(Avx2.And(packed, mask), alpha, inverse, (int)(color >> 16 & 255)));
        var g = Avx512F.ConvertToVector512Double(BlendRow8(Avx2.And(Avx2.ShiftRightLogical(packed.AsUInt32(), 8).AsInt32(), mask), alpha, inverse, (int)(color >> 8 & 255)));
        var b = Avx512F.ConvertToVector512Double(BlendRow8(Avx2.And(Avx2.ShiftRightLogical(packed.AsUInt32(), 16).AsInt32(), mask), alpha, inverse, (int)(color & 255)));
        return component == 1
            ? Avx512F.Add(Avx512F.Subtract(Avx512F.Subtract(Vector512.Create(128d), Avx512F.Multiply(Vector512.Create(0.168735892), r)), Avx512F.Multiply(Vector512.Create(0.331264108), g)), Avx512F.Multiply(Vector512.Create(0.5), b))
            : Avx512F.Subtract(Avx512F.Subtract(Avx512F.Add(Vector512.Create(128d), Avx512F.Multiply(Vector512.Create(0.5), r)), Avx512F.Multiply(Vector512.Create(0.418687589), g)), Avx512F.Multiply(Vector512.Create(0.081312411), b));
    }

    internal static void SampleSubsampledChromaPairRow8(ReadOnlySpan<byte> top, ReadOnlySpan<byte> bottom,
        int rows, uint? background, Span<byte> cb, Span<byte> cr)
    {
        if (top.Length != 64 || cb.Length != 8 || cr.Length != 8 || rows is not (1 or 2)
            || (rows == 2 ? bottom.Length != 64 : !bottom.IsEmpty))
            throw new ArgumentException("JPEG chroma pairs require sixteen RGBA pixels per row and two eight-sample outputs.");
        if (Avx512F.IsSupported && Avx512F.VL.IsSupported && Avx2.IsSupported)
        {
            var even = Vector512.Create(0L, 2, 4, 6, 8, 10, 12, 14);
            var odd = Vector512.Create(1L, 3, 5, 7, 9, 11, 13, 15);
            ChromaPairValuesRow8(top[..32], background, out var cbLeft, out var crLeft);
            ChromaPairValuesRow8(top[32..], background, out var cbRight, out var crRight);
            var cbSum = Avx512F.Add(Vector512<double>.Zero, Avx512F.PermuteVar8x64x2(cbLeft.AsInt64(), even, cbRight.AsInt64()).AsDouble());
            cbSum = Avx512F.Add(cbSum, Avx512F.PermuteVar8x64x2(cbLeft.AsInt64(), odd, cbRight.AsInt64()).AsDouble());
            var crSum = Avx512F.Add(Vector512<double>.Zero, Avx512F.PermuteVar8x64x2(crLeft.AsInt64(), even, crRight.AsInt64()).AsDouble());
            crSum = Avx512F.Add(crSum, Avx512F.PermuteVar8x64x2(crLeft.AsInt64(), odd, crRight.AsInt64()).AsDouble());
            if (rows == 2)
            {
                ChromaPairValuesRow8(bottom[..32], background, out cbLeft, out crLeft);
                ChromaPairValuesRow8(bottom[32..], background, out cbRight, out crRight);
                cbSum = Avx512F.Add(cbSum, Avx512F.PermuteVar8x64x2(cbLeft.AsInt64(), even, cbRight.AsInt64()).AsDouble());
                cbSum = Avx512F.Add(cbSum, Avx512F.PermuteVar8x64x2(cbLeft.AsInt64(), odd, cbRight.AsInt64()).AsDouble());
                crSum = Avx512F.Add(crSum, Avx512F.PermuteVar8x64x2(crLeft.AsInt64(), even, crRight.AsInt64()).AsDouble());
                crSum = Avx512F.Add(crSum, Avx512F.PermuteVar8x64x2(crLeft.AsInt64(), odd, crRight.AsInt64()).AsDouble());
            }
            JpegTransform.RoundSamples(Avx512F.Divide(cbSum, Vector512.Create(rows * 2d)), cb);
            JpegTransform.RoundSamples(Avx512F.Divide(crSum, Vector512.Create(rows * 2d)), cr);
            return;
        }
        var color = background ?? 0;
        for (var x = 0; x < 8; x++)
        {
            double cbSum = 0, crSum = 0;
            for (var dy = 0; dy < rows; dy++)
            for (var dx = 0; dx < 2; dx++)
            {
                var pixel = (dy == 0 ? top : bottom).Slice((x * 2 + dx) * 4, 4); var alpha = pixel[3];
                if (alpha != 255 && background is null)
                    throw new ArgumentException("JPEG requires opaque pixels or an explicit RGB background.", "image");
                var r = Flatten8(pixel[0], alpha, (int)(color >> 16 & 255));
                var g = Flatten8(pixel[1], alpha, (int)(color >> 8 & 255));
                var b = Flatten8(pixel[2], alpha, (int)(color & 255));
                cbSum += 128 - 0.168735892 * r - 0.331264108 * g + 0.5 * b;
                crSum += 128 + 0.5 * r - 0.418687589 * g - 0.081312411 * b;
            }
            cb[x] = (byte)Math.Clamp(Math.Round(cbSum / (rows * 2), MidpointRounding.AwayFromZero), 0, 255);
            cr[x] = (byte)Math.Clamp(Math.Round(crSum / (rows * 2), MidpointRounding.AwayFromZero), 0, 255);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ChromaPairValuesRow8(ReadOnlySpan<byte> rgba, uint? background,
        out Vector512<double> cb, out Vector512<double> cr)
    {
        var color = background ?? 0;
        var packed = Vector256.Create(rgba).AsInt32(); var mask = Vector256.Create(255);
        var alpha = Avx2.ShiftRightLogical(packed.AsUInt32(), 24).AsInt32();
        if (background is null && Avx2.MoveMask(Avx2.CompareEqual(alpha, mask).AsByte()) != -1)
            throw new ArgumentException("JPEG requires opaque pixels or an explicit RGB background.", "image");
        var inverse = Avx2.Subtract(mask, alpha);
        var r = Avx512F.ConvertToVector512Double(BlendRow8(Avx2.And(packed, mask), alpha, inverse, (int)(color >> 16 & 255)));
        var g = Avx512F.ConvertToVector512Double(BlendRow8(Avx2.And(Avx2.ShiftRightLogical(packed.AsUInt32(), 8).AsInt32(), mask), alpha, inverse, (int)(color >> 8 & 255)));
        var b = Avx512F.ConvertToVector512Double(BlendRow8(Avx2.And(Avx2.ShiftRightLogical(packed.AsUInt32(), 16).AsInt32(), mask), alpha, inverse, (int)(color & 255)));
        cb = Avx512F.Add(Avx512F.Subtract(Avx512F.Subtract(Vector512.Create(128d), Avx512F.Multiply(Vector512.Create(0.168735892), r)), Avx512F.Multiply(Vector512.Create(0.331264108), g)), Avx512F.Multiply(Vector512.Create(0.5), b));
        cr = Avx512F.Subtract(Avx512F.Subtract(Avx512F.Add(Vector512.Create(128d), Avx512F.Multiply(Vector512.Create(0.5), r)), Avx512F.Multiply(Vector512.Create(0.418687589), g)), Avx512F.Multiply(Vector512.Create(0.081312411), b));
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

    private static class FixedAc
    {
        internal static readonly JpegEncodingTable Table = Create();
        internal static readonly uint[] Words = Pack();

        private static uint[] Pack()
        {
            var words = new uint[256];
            for (var symbol = 0; symbol < words.Length; symbol++) words[symbol] = (uint)(Table.Codes[symbol] | Table.Lengths[symbol] << 16);
            return words;
        }

        private static JpegEncodingTable Create()
        {
            // Original, image-independent geometric model over T.81 AC symbols.
            // Long zero runs and large amplitudes receive less weight. No
            // image statistics, upstream implementation table or fitted corpus.
            var weights = new long[256]; weights[0] = 1 << 18; weights[0xF0] = 1 << 4;
            for (var run = 0; run < 16; run++)
            for (var size = 1; size <= 10; size++) weights[run << 4 | size] = 1 << Math.Max(0, 20 - run - size);
            return JpegHuffmanBuilder.Build(weights, new OperationContext(ImageLimits.Default, default));
        }
    }

    internal static void Segment(Stream output, byte marker, ReadOnlySpan<byte> payload)
    {
        if (payload.Length > 65533) throw new ArgumentOutOfRangeException(nameof(payload));
        Span<byte> header = stackalloc byte[4]; header[0] = 255; header[1] = marker;
        BinaryPrimitives.WriteUInt16BigEndian(header[2..], (ushort)(payload.Length + 2));
        output.Write(header); output.Write(payload);
    }
    [ThreadStatic] private static uint[]? adaptiveEvents;
    private const int MaximumRetainedEvents = 1024 * 1024;

}
