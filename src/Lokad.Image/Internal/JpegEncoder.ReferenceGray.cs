namespace Lokad.Image.Internal;

internal static partial class JpegEncoder
{
    private const int ReferenceGrayBlockWork = 1 + 64 * 36 + 4096 + 128 + 192;

    private static void EncodeReferenceGray(ImageBuffer? image, JpegSampleInput source, Stream output,
        JpegEncodeOptions options, OperationContext context)
    {
        var width = image?.Width ?? source.Width; var height = image?.Height ?? source.Height;
        context.PixelBytes(width, height, image?.BitDepth ?? 8);
        if (width > 65535 || height > 65535)
            throw new ImageLimitException(ImageLimitKind.Dimension, "JPEG dimensions exceed 65535.");
        context.Work(0);
        var blocks = ((width + 7L) / 8) * ((height + 7L) / 8);
        var restartWork = options.RestartInterval == 0 ? 0 : (blocks - 1) / options.RestartInterval * 5;
        var plannedWork = 2 * blocks * ReferenceGrayBlockWork + blocks * 132 + 2L * JpegReferenceTables.HuffmanWork + restartWork + 4096;
        if (plannedWork > context.RemainingWork)
            throw new ImageLimitException(ImageLimitKind.Work, "Reference grayscale JPEG work exceeds the configured limit.");
        long borrowed = 0; var fixedReserved = image is null; var tablesReserved = false;
        try
        {
            if (image is not null)
            {
                context.ReserveBorrowed(image.ByteLength); borrowed = image.ByteLength;
                context.Reserve(ReferenceFixedBytes); fixedReserved = true;
            }
            context.Work(0);
            var quantization = JpegGrayReferenceTransform.Quantization(options.Quality);
            // Once per operation, inside the existing8KiB fixed workspace.
            Span<ushort> reciprocals = stackalloc ushort[64];
            JpegGrayQuantization.Prepare(quantization, reciprocals);
            var histograms = new int[4][]; histograms[0] = new int[256]; histograms[2] = new int[256];
            ReferenceGrayPass(image, source, width, height, options.BackgroundRgb, quantization, reciprocals, histograms, null, null, context, options.RestartInterval);
            context.Reserve(ReferenceTableBytes); tablesReserved = true; context.Work(0);
            var tables = new JpegEncodingTable[4];
            tables[0] = JpegGrayReferenceTables.Huffman(histograms[0], context);
            tables[2] = JpegGrayReferenceTables.Huffman(histograms[2], context);
            WriteReferenceGrayHeader(output, width, height, quantization, tables, context, options.RestartInterval);
            var writer = new JpegEntropyWriter(output, context);
            ReferenceGrayPass(image, source, width, height, options.BackgroundRgb, quantization, reciprocals, histograms, tables, writer, context, options.RestartInterval);
            writer.Align(); context.Work(2); output.WriteByte(255); output.WriteByte(217);
        }
        finally
        {
            if (tablesReserved) context.Release(ReferenceTableBytes);
            if (fixedReserved) context.Release(ReferenceFixedBytes);
            if (borrowed != 0) context.ReleaseBorrowed(borrowed);
        }
    }

    private static void ReferenceGrayPass(ImageBuffer? image, JpegSampleInput source, int width, int height,
        uint? background, ushort[] quantization, ReadOnlySpan<ushort> reciprocals, int[][] histograms, JpegEncodingTable[]? tables,
        JpegEntropyWriter? writer, OperationContext context, int restartInterval)
    {
        Span<byte> samples = stackalloc byte[64];
        Span<short> transformed = stackalloc short[64], coefficients = stackalloc short[64];
        Span<int> previous = stackalloc int[1]; previous.Clear();
        var mcu = 0;
        for (var top = 0; top < height; top += 8)
        for (var left = 0; left < width; left += 8)
        {
            // Single-component scans have one block per MCU. Both passes must
            // reset DC before collecting/encoding the first block of an interval.
            ReferenceRestart(mcu++, restartInterval, previous, writer, context);
            context.Work(ReferenceGrayBlockWork);
            if (image is null && source.Format == JpegSampleFormat.Grayscale) source.CopyGrayBlock(left, top, samples);
            else
                JpegGrayPreparation.CopyBlock(image, source, width, height, left, top, background, samples);
            JpegGrayReferenceTransform.Forward(samples, transformed);
            JpegGrayQuantization.Quantize(transformed, quantization, reciprocals, coefficients);
            ReferenceBlock(coefficients, 0, previous, histograms, tables, writer);
        }
    }

    private static void WriteReferenceGrayHeader(Stream output, int width, int height, ushort[] quantization,
        JpegEncodingTable[] tables, OperationContext context, int restartInterval)
    {
        context.Work(2); output.WriteByte(255); output.WriteByte(216);
        // JFIF APP0 version1.01 with unitless1:1 density, matching the pinned
        // grayscale reference. It has no encoder-attribution comment to replace.
        ReferenceSegment(output, 224, [74, 70, 73, 70, 0, 1, 1, 0, 0, 1, 0, 1, 0, 0], context);
        Span<byte> quant = stackalloc byte[65]; quant[0] = 0;
        for (var i = 0; i < 64; i++) quant[i + 1] = (byte)quantization[JpegTransform.Zigzag[i]];
        ReferenceSegment(output, 219, quant, context);
        Span<byte> frame = stackalloc byte[] { 8, (byte)(height >> 8), (byte)height, (byte)(width >> 8), (byte)width,
            1, 1, 17, 0 };
        ReferenceSegment(output, 192, frame, context);
        Span<byte> huffman = stackalloc byte[17 + 256];
        for (var index = 0; index <= 2; index += 2)
        {
            var table = tables[index]; huffman[0] = (byte)(index == 0 ? 0 : 16);
            table.Counts.CopyTo(huffman[1..]); table.Symbols.CopyTo(huffman[17..]);
            ReferenceSegment(output, 196, huffman[..(17 + table.Symbols.Length)], context);
        }
        if (restartInterval != 0)
            ReferenceSegment(output, 221, [(byte)(restartInterval >> 8), (byte)restartInterval], context);
        ReferenceSegment(output, 218, [1, 1, 0, 0, 63, 0], context);
    }
}
