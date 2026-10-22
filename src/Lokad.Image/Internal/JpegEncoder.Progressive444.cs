using System.Buffers.Binary;

namespace Lokad.Image.Internal;

internal static partial class JpegEncoder
{
    // T.81 Annex G first scans, independently implemented. Every selected
    // coefficient is a multiple of its point transform; no nonzero refinement
    // bits are omitted. Original DQT values remain literal and unchanged.
    private static bool CanWriteProgressive444(JpegEncodeOptions options)
        => !options.BaselineOnly && options.Quality is >= 1 and <= 100 && options.RestartInterval == 0
            && options.Sampling == JpegSampling.YCbCr444
            && AcDecisionLambda(options.Quality, 3, options.Sampling, options.QuantizationPolicy) != 0;

    private static long Progressive444Work(long blocks, JpegEncodeOptions options)
        => CanWriteProgressive444(options) ? 192L * 65536 + blocks * 512 : 0;

    private static long JpegPacketBound(long blocks, JpegEncodeOptions options)
        => blocks * 432 + (CanWriteProgressive444(options) ? 65536 : 2048);

    internal static int PointTransform444(int quantizer, int precision)
    {
        var level = 0;
        while (level < 13 && (long)quantizer * (1 << (level + 1)) <= precision) level++;
        return level;
    }

    internal static void RefinePrecision444(ReadOnlySpan<double> source, ReadOnlySpan<ushort> luma,
        ReadOnlySpan<ushort> chroma, Span<int> values, int lambda, OperationContext context)
    {
        if (source.Length != 192 || values.Length != 192 || luma.Length != 64 || chroma.Length != 64 || lambda < 0)
            throw new ArgumentException("JPEG precision decisions require three complete blocks.");
        if (lambda == 0) return;
        // At most thirteen factor comparisons and one decision per coefficient.
        // Charge before mutation, including cancellation/refusal.
        context.Work(2880);
        var precision = (int)Math.Sqrt(lambda);
        for (var component = 0; component < 3; component++)
        for (var index = 0; index < 64; index++)
        {
            var q = component == 0 ? luma[index] : chroma[index];
            var level = PointTransform444(q, precision);
            if (level == 0) continue;
            var factor = 1 << level;
            var selected = (int)Math.Round(source[component * 64 + index] / (q * factor), MidpointRounding.AwayFromZero);
            var lower = index == 0 ? -1024 / factor : -(1023 / factor);
            values[component * 64 + index] = Math.Clamp(selected, lower, 1023 / factor) * factor;
        }
    }

    // Reuse the already admitted event capacity as coefficient storage. There
    // is no simultaneous second frame/event array and no borrowed input copy.
    private static int AppendProgressive444(ReadOnlySpan<int> values, uint[] cache, int count, OperationContext context)
    {
        context.Work(192);
        for (var component = 0; component < 3; component++)
        for (var scan = 0; scan < 64; scan++)
            cache[count++] = unchecked((uint)values[component * 64 + JpegTransform.Zigzag[scan]]);
        return count;
    }

    internal static void WriteProgressive444(int width, int height, Stream output, JpegEncodeOptions options,
        ushort[] luma, ushort[] chroma, uint[] coefficients, int blocks, OperationContext context)
    {
        // The existing 64 KiB adaptive reservation covers three histograms,
        // three retained tables and a fitter (including queue replacement peaks).
        // At most 12 Original Y/shared-chroma bands imply <=39 fits, plus <64K
        // bookkeeping visits, within the existing 192*65536 fixed work plan.
        context.Work(2048);
        var lambda = AcDecisionLambda(options.Quality, 3, options.Sampling, options.QuantizationPolicy);
        var bands = SourcePriceBands444(luma, chroma, lambda, context);
        var mcus = blocks / 3;
        output.Write(new byte[] { 255, 216 });
        Segment(output, 0xE0, new byte[] { 74, 70, 73, 70, 0, 1, 1, 0, 0, 1, 0, 1, 0, 0 });
        WriteQuantization(output, luma, 0); WriteQuantization(output, chroma, 1);
        Span<byte> frame = stackalloc byte[15]; frame.Clear(); frame[0] = 8; frame[5] = 3;
        BinaryPrimitives.WriteUInt16BigEndian(frame[1..], (ushort)height);
        BinaryPrimitives.WriteUInt16BigEndian(frame[3..], (ushort)width);
        for (var c = 0; c < 3; c++) { frame[6 + c * 3] = (byte)(c + 1); frame[7 + c * 3] = 17; frame[8 + c * 3] = c == 0 ? (byte)0 : (byte)1; }
        Segment(output, 0xC2, frame);
        var precision = (int)Math.Sqrt(lambda);
        var histogram = new long[256];
        for (var c = 0; c < 3; c++)
        {
            var q = c == 0 ? luma : chroma;
            var al = PointTransform444(q[0], precision);
            WriteProgressiveScan(output, coefficients, mcus, c, 0, 0, al, histogram, context);
        }
        var ends = bands.Luma | (1UL << 63);
        for (var first = 1; first < 64;)
        {
            var last = System.Numerics.BitOperations.TrailingZeroCount(ends & (ulong.MaxValue << first));
            var al = PointTransform444(luma[JpegTransform.Zigzag[first]], precision);
            WriteProgressiveScan(output, coefficients, mcus, 0, first, last, al, histogram, context);
            first = last + 1;
        }
        var other = new long[256]; var pooled = new long[256];
        ends = bands.Chroma | (1UL << 63);
        for (var first = 1; first < 64;)
        {
            var last = System.Numerics.BitOperations.TrailingZeroCount(ends & (ulong.MaxValue << first));
            var al = PointTransform444(chroma[JpegTransform.Zigzag[first]], precision);
            context.Work(512); Array.Clear(histogram); Array.Clear(other);
            ProgressiveScanTokens(coefficients, mcus, 1, first, last, al, histogram, null, null, context);
            ProgressiveScanTokens(coefficients, mcus, 2, first, last, al, other, null, null, context);
            var cb = JpegHuffmanBuilder.Build(histogram, context);
            var cr = JpegHuffmanBuilder.Build(other, context);
            context.Work(256);
            for (var symbol = 0; symbol < 256; symbol++) pooled[symbol] = checked(histogram[symbol] + other[symbol]);
            var shared = JpegHuffmanBuilder.Build(pooled, context);
            var splitCost = checked(ProgressiveTableBitCost(histogram, cb, context) + ProgressiveTableBitCost(other, cr, context));
            if (ProgressiveTableBitCost(pooled, shared, context) < splitCost)
            {
                WriteProgressiveTable(output, shared, false, context);
                WriteProgressiveEntropyScan(output, coefficients, mcus, 1, first, last, al, shared, context);
                WriteProgressiveEntropyScan(output, coefficients, mcus, 2, first, last, al, shared, context);
            }
            else
            {
                WriteProgressiveTable(output, cb, false, context);
                WriteProgressiveEntropyScan(output, coefficients, mcus, 1, first, last, al, cb, context);
                WriteProgressiveTable(output, cr, false, context);
                WriteProgressiveEntropyScan(output, coefficients, mcus, 2, first, last, al, cr, context);
            }
            first = last + 1;
        }
    }

    internal static long ProgressiveTableBitCost(ReadOnlySpan<long> histogram, JpegEncodingTable table, OperationContext context)
    {
        if (histogram.Length != 256 || table.Lengths.Length != 256)
            throw new ArgumentException("A complete JPEG symbol histogram and table are required.");
        context.Work(256);
        // Marker/length4 + table selector1 + length counts16 + symbol bytes.
        var bits = 8L * (21 + table.Symbols.Length);
        for (var symbol = 0; symbol < 256; symbol++) bits = checked(bits + histogram[symbol] * table.Lengths[symbol]);
        return bits;
    }

    private static void WriteProgressiveScan(Stream output, uint[] coefficients, int mcus, int component,
        int first, int last, int al, long[] histogram, OperationContext context)
    {
        context.Work(256); Array.Clear(histogram);
        ProgressiveScanTokens(coefficients, mcus, component, first, last, al, histogram, null, null, context);
        var table = JpegHuffmanBuilder.Build(histogram, context);
        WriteProgressiveTable(output, table, first == 0, context);
        WriteProgressiveEntropyScan(output, coefficients, mcus, component, first, last, al, table, context);
    }

    private static void WriteProgressiveTable(Stream output, JpegEncodingTable table, bool dc, OperationContext context)
    {
        context.Work(512);
        var payload = new byte[17 + table.Symbols.Length]; payload[0] = dc ? (byte)0 : (byte)16;
        table.Counts.CopyTo(payload, 1); table.Symbols.CopyTo(payload, 17); Segment(output, 0xC4, payload);
    }

    private static void WriteProgressiveEntropyScan(Stream output, uint[] coefficients, int mcus, int component,
        int first, int last, int al, JpegEncodingTable table, OperationContext context)
    {
        Span<byte> header = stackalloc byte[6] { 1, (byte)(component + 1), 0, (byte)first, (byte)last, (byte)al };
        Segment(output, 0xDA, header);
        var writer = new JpegEntropyWriter(output, context);
        ProgressiveScanTokens(coefficients, mcus, component, first, last, al, null, table, writer, context);
        writer.Align();
    }

    private static void ProgressiveScanTokens(uint[] coefficients, int mcus, int component, int first, int last,
        int al, long[]? histogram, JpegEncodingTable? table, JpegEntropyWriter? writer, OperationContext context)
    {
        var predictor = 0; var pending = 0;
        for (var mcu = 0; mcu < mcus; mcu++)
        {
            context.Work(last - first + 1);
            var offset = (mcu * 3 + component) * 64;
            if (first == 0)
            {
                var value = unchecked((int)coefficients[offset]) >> al; var difference = value - predictor; predictor = value;
                var n = Category(difference); ProgressiveToken(n, difference < 0 ? difference + (1 << n) - 1 : difference, n, histogram, table, writer);
                continue;
            }
            var occupied = last;
            while (occupied >= first && coefficients[offset + occupied] == 0) occupied--;
            if (occupied >= first)
            {
                FlushProgressiveEob(ref pending, histogram, table, writer);
                var zeros = 0;
                for (var scan = first; scan <= occupied; scan++)
                {
                    var value = unchecked((int)coefficients[offset + scan]) >> al;
                    if (value == 0) { zeros++; continue; }
                    while (zeros >= 16) { ProgressiveToken(240, 0, 0, histogram, table, writer); zeros -= 16; }
                    var n = Category(value);
                    ProgressiveToken(zeros * 16 + n, value < 0 ? value + (1 << n) - 1 : value, n, histogram, table, writer); zeros = 0;
                }
            }
            if (occupied < last) pending++;
            if (pending == 32767) FlushProgressiveEob(ref pending, histogram, table, writer);
        }
        FlushProgressiveEob(ref pending, histogram, table, writer);
    }

    private static void FlushProgressiveEob(ref int pending, long[]? histogram, JpegEncodingTable? table, JpegEntropyWriter? writer)
    {
        if (pending == 0) return;
        var n = Category(pending) - 1;
        ProgressiveToken(n << 4, pending - (1 << n), n, histogram, table, writer); pending = 0;
    }

    private static void ProgressiveToken(int symbol, int extra, int n, long[]? histogram,
        JpegEncodingTable? table, JpegEntropyWriter? writer)
    {
        if (histogram is not null) { histogram[symbol]++; return; }
        writer!.Write(table!.Codes[symbol], table.Lengths[symbol]); writer.Write(extra, n);
    }
}
