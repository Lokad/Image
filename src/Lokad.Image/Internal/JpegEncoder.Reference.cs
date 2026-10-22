namespace Lokad.Image.Internal;

internal static partial class JpegEncoder
{
    private const int ReferenceFixedBytes = 8192;
    private const int ReferenceTableBytes = 16384;
    private const int ReferenceMcuWork = 1 + 128 * 36 + 6 * (4096 + 128 + 192);

    internal static int ReferenceQScale(int quality)
    {
        if (quality is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(quality));
        return 1 + (100 - quality) * 30 / 99;
    }

    internal static void ValidateReferenceOptions(JpegEncodeOptions options, JpegSampleFormat format)
    {
        if (options.Quality is < 1 or > 100 || !Enum.IsDefined(options.Sampling)
            || options.RestartInterval is < 0 or > 65535 || options.BackgroundRgb > 0xFFFFFF)
            throw new ArgumentOutOfRangeException(nameof(options));
        if (format is not (JpegSampleFormat.Rgb or JpegSampleFormat.Grayscale))
            throw new UnsupportedImageException(UnsupportedImageFeature.ComponentLayout,
                "The reference JPEG profile accepts grayscale, RGB or RGBA input.", ImageFormat.Jpeg);
        if (options.RestartInterval != 0)
            throw new UnsupportedImageException(UnsupportedImageFeature.ControlMarker,
                "The reference JPEG profile currently requires restart markers disabled.", ImageFormat.Jpeg);
    }

    private static void EncodeReference(ImageBuffer? image, JpegSampleInput source, Stream output,
        JpegEncodeOptions options, OperationContext context)
    {
        ValidateReferenceOptions(options, image is null ? source.Format : JpegSampleFormat.Rgb);
        if (options.Sampling == JpegSampling.Grayscale || image is null && source.Format == JpegSampleFormat.Grayscale)
        { EncodeReferenceGray(image, source, output, options, context); return; }
        var width = image?.Width ?? source.Width; var height = image?.Height ?? source.Height;
        context.PixelBytes(width, height, image?.BitDepth ?? 8);
        if (width > 65535 || height > 65535)
            throw new ImageLimitException(ImageLimitKind.Dimension, "JPEG dimensions exceed 65535.");
        context.Work(0);
        var mcuWidth = options.Sampling == JpegSampling.YCbCr444 ? 8 : 16;
        var mcus = ((width + mcuWidth - 1L) / mcuWidth) * ((height + 15L) / 16);
        var mcuWork = options.Sampling == JpegSampling.YCbCr444 ? ReferenceMcuWork : ReferenceSubsampledMcuWork;
        var blocks = options.Sampling == JpegSampling.YCbCr422 ? 8 : 6;
        var plannedWork = 2 * mcus * mcuWork + mcus * blocks * 132 + 4L * JpegReferenceTables.HuffmanWork + 4096;
        if (plannedWork > context.RemainingWork)
            throw new ImageLimitException(ImageLimitKind.Work, "Reference JPEG work exceeds the configured limit.");
        long borrowed = 0; var fixedReserved = image is null; var tablesReserved = false;
        // Caller sample entry points already admitted their borrowed span and
        // 8 KiB fixed storage. This path consumes and releases that reservation.
        try
        {
            if (image is not null)
            {
                context.ReserveBorrowed(image.ByteLength); borrowed = image.ByteLength;
                context.Reserve(ReferenceFixedBytes); fixedReserved = true;
            }
            context.Work(0);
            var quantization = JpegReferenceTables.Quantization(ReferenceQScale(options.Quality));
            var histograms = new int[4][];
            for (var i = 0; i < 4; i++) histograms[i] = new int[256];
            ReferencePass(image, source, width, height, options.BackgroundRgb, options.Sampling, quantization, histograms, null, null, context);
            // Establish the owner of all four surviving table arrays before the
            // generators allocate them. Their graph scratch has a shorter owner.
            context.Reserve(ReferenceTableBytes); tablesReserved = true; context.Work(0);
            var tables = new JpegEncodingTable[4];
            for (var i = 0; i < 4; i++) tables[i] = JpegReferenceTables.Huffman(histograms[i], context);
            WriteReferenceHeader(output, width, height, options.Sampling, quantization, tables, context);
            var writer = new JpegEntropyWriter(output, context);
            ReferencePass(image, source, width, height, options.BackgroundRgb, options.Sampling, quantization, histograms, tables, writer, context);
            writer.Align(); context.Work(2); output.WriteByte(255); output.WriteByte(217);
        }
        finally
        {
            if (tablesReserved) context.Release(ReferenceTableBytes);
            if (fixedReserved) context.Release(ReferenceFixedBytes);
            if (borrowed != 0) context.ReleaseBorrowed(borrowed);
        }
    }

    private static void ReferencePass(ImageBuffer? image, JpegSampleInput source, int width, int height, uint? background, JpegSampling sampling,
        ushort[] quantization, int[][] histograms, JpegEncodingTable[]? tables, JpegEntropyWriter? writer, OperationContext context)
    {
        if (sampling != JpegSampling.YCbCr444)
        {
            ReferenceSubsampledPass(image, source, width, height, background, sampling,
                quantization, histograms, tables, writer, context);
            return;
        }
        Span<byte> mcu = stackalloc byte[384]; Span<byte> row = stackalloc byte[24];
        Span<short> transformed = stackalloc short[64], coefficients = stackalloc short[64];
        Span<int> previous = stackalloc int[3]; previous.Clear();
        for (var top = 0; top < height; top += 16)
        for (var left = 0; left < width; left += 8)
        {
            context.Work(ReferenceMcuWork);
            for (var y = 0; y < 16; y++)
            {
                var sy = Math.Min(top + y, height - 1);
                if (image is null) source.CopyRgbRow(left, sy, row);
                for (var x = 0; x < 8; x++)
                {
                    int r, g, b;
                    if (image is null) { r = row[x * 3]; g = row[x * 3 + 1]; b = row[x * 3 + 2]; }
                    else ReadRgb(image, Math.Min(left + x, width - 1), sy, background, out r, out g, out b);
                    JpegReferenceColor.Convert(r, g, b, out var luma, out var cb, out var cr);
                    var position = y / 8 * 64 + y % 8 * 8 + x;
                    mcu[position] = luma; mcu[128 + position] = cb; mcu[256 + position] = cr;
                }
            }
            for (var component = 0; component < 3; component++)
            for (var vertical = 0; vertical < 2; vertical++)
            {
                JpegReferenceTransform.Forward(mcu.Slice(component * 128 + vertical * 64, 64), transformed);
                JpegReferenceTransform.Quantize(transformed, quantization, coefficients);
                ReferenceBlock(coefficients, component, previous, histograms, tables, writer);
            }
        }
    }

    private static void ReferenceBlock(ReadOnlySpan<short> coefficients, int component, Span<int> previous,
        int[][] histograms, JpegEncodingTable[]? tables, JpegEntropyWriter? writer)
    {
        var family = component == 0 ? 0 : 1;
        var difference = coefficients[0] - previous[component]; previous[component] = coefficients[0];
        ReferenceCoefficient(writer, tables, histograms, family, 0, difference);
        var run = 0;
        for (var k = 1; k < 64; k++)
        {
            var value = coefficients[JpegTransform.Zigzag[k]];
            if (value == 0) { run++; continue; }
            while (run >= 16) { ReferenceSymbol(writer, tables, histograms, family + 2, 240); run -= 16; }
            ReferenceCoefficient(writer, tables, histograms, family + 2, run, value); run = 0;
        }
        if (run != 0) ReferenceSymbol(writer, tables, histograms, family + 2, 0);
    }

    private static void ReferenceCoefficient(JpegEntropyWriter? writer, JpegEncodingTable[]? tables,
        int[][] histograms, int table, int run, int value)
    {
        var bits = value == 0 ? 0 : System.Numerics.BitOperations.Log2((uint)Math.Abs(value)) + 1;
        ReferenceSymbol(writer, tables, histograms, table, run * 16 + bits);
        if (writer is not null) writer.Signed(value, bits);
    }

    private static void ReferenceSymbol(JpegEntropyWriter? writer, JpegEncodingTable[]? tables,
        int[][] histograms, int table, int symbol)
    {
        if (writer is null) { histograms[table][symbol]++; return; }
        var coding = tables![table]; var length = coding.Lengths[symbol];
        if (length == 0) throw new ImageDataException("JPEG source changed between encoding passes.");
        writer.Write(coding.Codes[symbol], length);
    }

    private static void WriteReferenceHeader(Stream output, int width, int height, JpegSampling sampling, ushort[] quantization,
        JpegEncodingTable[] tables, OperationContext context)
    {
        context.Work(2); output.WriteByte(255); output.WriteByte(216);
        ReferenceSegment(output, 254, "Lokad.Image\0"u8, context);
        Span<byte> quant = stackalloc byte[65]; quant[0] = 0;
        for (var i = 0; i < 64; i++) quant[i + 1] = (byte)quantization[JpegTransform.Zigzag[i]];
        ReferenceSegment(output, 219, quant, context);
        Span<byte> huffman = stackalloc byte[4 * (17 + 256)]; var at = 0;
        for (var i = 0; i < 4; i++)
        {
            huffman[at++] = (byte)((i / 2 << 4) | i % 2);
            tables[i].Counts.CopyTo(huffman[at..]); at += 16;
            tables[i].Symbols.CopyTo(huffman[at..]); at += tables[i].Symbols.Length;
        }
        ReferenceSegment(output, 196, huffman[..at], context);
        Span<byte> frame = stackalloc byte[] { 8, (byte)(height >> 8), (byte)height, (byte)(width >> 8), (byte)width,
            3, 1, (byte)(sampling == JpegSampling.YCbCr444 ? 18 : 34), 0,
            2, (byte)(sampling == JpegSampling.YCbCr420 ? 17 : 18), 0,
            3, (byte)(sampling == JpegSampling.YCbCr420 ? 17 : 18), 0 };
        ReferenceSegment(output, 192, frame, context);
        ReferenceSegment(output, 218, [3, 1, 0, 2, 17, 3, 17, 0, 63, 0], context);
    }

    private static void ReferenceSegment(Stream output, byte marker, ReadOnlySpan<byte> payload, OperationContext context)
    {
        context.Work(payload.Length + 4); var length = payload.Length + 2;
        output.WriteByte(255); output.WriteByte(marker); output.WriteByte((byte)(length >> 8)); output.WriteByte((byte)length);
        output.Write(payload);
    }
}
