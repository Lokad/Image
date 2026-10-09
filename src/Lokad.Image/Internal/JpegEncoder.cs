using System.Buffers.Binary;

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
        var pixels = new byte[64]; var transformed = new double[64]; var quantized = new int[64]; var predictors = new int[components];
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
                    for (var y = 0; y < 8; y++)
                    for (var x = 0; x < 8; x++)
                    {
                        double sum = 0;
                        var samplesX = component == 0 ? 1 : h; var samplesY = component == 0 ? 1 : v;
                        for (var dy = 0; dy < samplesY; dy++)
                        for (var dx = 0; dx < samplesX; dx++)
                        {
                            var sourceX = mcu % columns * h * 8 + bx * 8 + x * samplesX + dx;
                            var sourceY = mcu / columns * v * 8 + by * 8 + y * samplesY + dy;
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
                    JpegTransform.Forward(pixels, transformed, context);
                    for (var i = 0; i < 64; i++) quantized[i] = (int)Math.Round(transformed[i] / q[i], MidpointRounding.AwayFromZero);
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

    private static void ReadRgb(ImageBuffer image, int x, int y, uint? background, out int r, out int g, out int b)
    {
        var maximum = image.BitDepth == 8 ? 255 : 65535;
        var alpha = image.Sample(x, y, 3);
        if (alpha != maximum && background is null) throw new ArgumentException("JPEG requires opaque pixels or an explicit RGB background.", nameof(image));
        var color = background ?? 0;
        r = Flatten(image.Sample(x, y, 0), alpha, (int)((color >> 16) & 255), maximum);
        g = Flatten(image.Sample(x, y, 1), alpha, (int)((color >> 8) & 255), maximum);
        b = Flatten(image.Sample(x, y, 2), alpha, (int)(color & 255), maximum);
    }

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

    private static int Category(int value)
    {
        var magnitude = Math.Abs(value); var size = 0;
        while (magnitude != 0) { size++; magnitude >>= 1; }
        return size;
    }

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
