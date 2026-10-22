// Original managed JPEG stage probe (T.81 B.2/B.3/F.1). DQT and DHT are explicit
// oracle-supplied inputs in Encode; EncodeGenerated uses independent generators.
// All samples, MCU traversal, coefficient decisions and other bytes are produced
// independently. This is development tooling, not a production codec path.
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

public static class JpegScanParity
{
    private static readonly int[] Zigzag = MakeZigzag();

    public sealed class Result
    {
        public byte[] Packet = Array.Empty<byte>();
        public int[][] Histograms = new int[4][];
        public int PhantomValues;
    }

    public sealed class CodeTable
    {
        public byte[] Counts = new byte[16];
        public byte[] Symbols = Array.Empty<byte>();
        public int[] Codes = new int[256];
        public int[] Lengths = new int[256];

        public void Prepare()
        {
            int code = 0, position = 0;
            for (int length = 1; length <= 16; length++)
            {
                for (int n = 0; n < Counts[length - 1]; n++)
                {
                    if (position >= Symbols.Length || code >= (1 << length) - 1)
                        throw new ArgumentException("Invalid pinned Huffman table.");
                    int value = Symbols[position++];
                    if (Lengths[value] != 0) throw new ArgumentException("Repeated Huffman symbol.");
                    Codes[value] = code++; Lengths[value] = length;
                }
                code <<= 1;
            }
            if (position != Symbols.Length) throw new ArgumentException("Huffman counts differ.");
        }
    }

    public static Result Encode(byte[] planes, byte[] referencePacket, byte[] expectedPhantoms, bool nativeAttribution)
    {
        ReadTables(referencePacket, out ushort[] quantization, out CodeTable[] tables);
        return EncodeWithTables(planes, quantization, tables, expectedPhantoms, nativeAttribution);
    }

    // Framing stage only: supplied DQT/DHT, frozen420 row interval33. Independent
    // standard-table generation and production admission are separate milestones.
    public static Result EncodeRowRestartStage(byte[] planes, byte[] referencePacket, bool nativeAttribution)
    {
        ReadTables(referencePacket, out ushort[] quantization, out CodeTable[] tables);
        return EncodeWithTables(planes, quantization, tables, null, nativeAttribution, 420, true);
    }

    public static Result EncodeGenerated(byte[] rgba, int depth, bool nativeAttribution)
        => EncodeGenerated(rgba, depth, nativeAttribution, 444);

    public static Result EncodeGenerated(byte[] rgba, int depth, bool nativeAttribution, int sampling)
    {
        byte[] planes = sampling == 444 ? JpegColorParity.Convert(rgba, depth)
            : JpegSubsampleParity.Convert(rgba, depth, sampling);
        ushort[] quantization = JpegReferenceTables.Quantization(5);
        Result statistics = EncodeWithTables(planes, quantization, null, null, nativeAttribution, sampling);
        var tables = new CodeTable[4];
        for (int i = 0; i < 4; i++) tables[i] = JpegReferenceTables.Huffman(statistics.Histograms[i]);
        return EncodeWithTables(planes, quantization, tables, null, nativeAttribution, sampling);
    }

    private static Result EncodeWithTables(byte[] planes, ushort[] quantization, CodeTable[] tables,
        byte[] expectedPhantoms, bool nativeAttribution, int sampling = 444, bool rowRestart = false)
    {
        const int width = 513, height = 259, plane = width * height;
        int chromaWidth = sampling == 444 ? width : 257;
        int chromaHeight = sampling == 420 ? 130 : height;
        if (sampling != 444 && sampling != 422 && sampling != 420
            || planes.Length != plane + chromaWidth * chromaHeight * 2
            || expectedPhantoms != null && (sampling != 444 || expectedPhantoms.Length != 195 * 128))
            throw new ArgumentException("Frozen full-resolution planes and phantom coefficients required.");
        var result = new Result();
        for (int i = 0; i < 4; i++) result.Histograms[i] = new int[256];
        if (rowRestart && sampling != 420) throw new ArgumentException("Frozen row-restart420 stage required.");
        var packet = tables == null ? new List<byte>() : Headers(quantization, tables, nativeAttribution, sampling, rowRestart);
        var writer = new ScanWriter(packet, tables, result.Histograms);
        Span<byte> samples = stackalloc byte[64];
        Span<short> transformed = stackalloc short[64];
        Span<short> coefficients = stackalloc short[64];
        Span<int> previous = stackalloc int[3]; previous.Clear();
        for (int top = 0; top < height; top += 16)
        {
            if (rowRestart && top != 0)
            {
                writer.Finish(); previous.Clear();
                if (tables != null) { packet.Add(255); packet.Add((byte)(208 + ((top / 16 - 1) & 7))); }
            }
            for (int left = 0; left < width; left += sampling == 444 ? 8 : 16)
                for (int component = 0; component < 3; component++)
                    for (int vertical = 0; vertical < (component != 0 && sampling == 420 ? 1 : 2); vertical++)
                    for (int horizontal = 0; horizontal < (component == 0 && sampling != 444 ? 2 : 1); horizontal++)
                    {
                        int componentWidth = component == 0 ? width : chromaWidth;
                        int componentHeight = component == 0 ? height : chromaHeight;
                        int offset = component == 0 ? 0 : plane + (component - 1) * chromaWidth * chromaHeight;
                        int blockTop = (component != 0 && sampling == 420 ? top / 2 : top) + vertical * 8;
                        int blockLeft = (component != 0 && sampling != 444 ? left / 2 : left) + horizontal * 8;
                        for (int y = 0; y < 8; y++)
                            for (int x = 0; x < 8; x++)
                                samples[y * 8 + x] = planes[offset
                                    + Math.Min(blockTop + y, componentHeight - 1) * componentWidth
                                    + Math.Min(blockLeft + x, componentWidth - 1)];
                        JpegForwardParity.Forward(samples, transformed);
                        JpegForwardParity.Quantize(transformed, quantization, coefficients);
                        if (blockTop >= componentHeight)
                            for (int k = 0; k < 64; k++, result.PhantomValues++)
                            {
                                if (expectedPhantoms != null)
                                {
                                    int position = result.PhantomValues * 2;
                                    short expected = (short)(expectedPhantoms[position] | expectedPhantoms[position + 1] << 8);
                                    if (coefficients[k] != expected)
                                        throw new InvalidOperationException($"Phantom differs: component {component}, column {left / 8}, frequency {k}, actual {coefficients[k]}, expected {expected}.");
                                }
                            }
                        int family = component == 0 ? 0 : 1;
                        int difference = coefficients[0] - previous[component];
                        previous[component] = coefficients[0];
                        writer.Coefficient(family, 0, difference);
                        int run = 0;
                        for (int k = 1; k < 64; k++)
                        {
                            int value = coefficients[Zigzag[k]];
                            if (value == 0) { run++; continue; }
                            while (run >= 16) { writer.Symbol(family + 2, 240); run -= 16; }
                            writer.Coefficient(family + 2, run, value); run = 0;
                        }
                        if (run != 0) writer.Symbol(family + 2, 0);
                    }
        }
        writer.Finish();
        if (tables != null) { packet.Add(255); packet.Add(217); }
        result.Packet = packet.ToArray();
        return result;
    }

    public static void ReadTables(byte[] packet, out ushort[] quantization, out CodeTable[] tables)
    {
        if (packet.Length < 4 || packet.Length > 1024 * 1024 || packet[0] != 255 || packet[1] != 216)
            throw new ArgumentException("Bounded frozen JPEG required.");
        quantization = new ushort[64]; tables = new CodeTable[4];
        int position = 2;
        while (position < packet.Length - 4)
        {
            if (packet[position++] != 255) throw new ArgumentException("Expected JPEG marker.");
            int marker = packet[position++];
            int end = position + (packet[position] << 8 | packet[position + 1]); position += 2;
            if (end > packet.Length || end < position) throw new ArgumentException("Invalid segment.");
            if (marker == 219)
            {
                if (end - position != 65 || packet[position++] != 0) throw new ArgumentException("One 8-bit quantization table required.");
                for (int k = 0; k < 64; k++) quantization[Zigzag[k]] = packet[position++];
            }
            else if (marker == 196)
            {
                while (position < end)
                {
                    int info = packet[position++], index = (info >> 4) * 2 + (info & 15);
                    if ((info & 15) > 1 || index > 3 || tables[index] != null || end - position < 16)
                        throw new ArgumentException("Unexpected Huffman family.");
                    var table = new CodeTable(); int symbols = 0;
                    for (int k = 0; k < 16; k++) { table.Counts[k] = packet[position++]; symbols += table.Counts[k]; }
                    if (symbols > 256 || position + symbols > end) throw new ArgumentException("Invalid Huffman symbol count.");
                    table.Symbols = packet.AsSpan(position, symbols).ToArray(); position += symbols;
                    table.Prepare(); tables[index] = table;
                }
            }
            else if (marker == 218) break;
            position = end;
        }
        if (quantization[0] != 8 || Array.Exists(tables, table => table == null))
            throw new ArgumentException("Incomplete pinned tables.");
    }

    public static List<byte> Headers(ushort[] quantization, CodeTable[] tables, bool nativeAttribution, int sampling = 444, bool rowRestart = false)
    {
        var packet = new List<byte>(150000) { 255, 216 };
        Segment(packet, 254, Encoding.ASCII.GetBytes(nativeAttribution ? "Lavc62.28.100\0" : "Lokad.Image\0"));
        var quantizationBytes = new byte[65];
        for (int k = 0; k < 64; k++) quantizationBytes[k + 1] = checked((byte)quantization[Zigzag[k]]);
        Segment(packet, 219, quantizationBytes);
        if (rowRestart) Segment(packet, 221, new byte[] { 0, 33 });
        var huffman = new List<byte>();
        for (int i = 0; i < 4; i++)
        {
            huffman.Add((byte)((i / 2 << 4) | i % 2));
            huffman.AddRange(tables[i].Counts); huffman.AddRange(tables[i].Symbols);
        }
        Segment(packet, 196, huffman.ToArray());
        byte lumaSampling = (byte)(sampling == 444 ? 18 : 34);
        byte chromaSampling = (byte)(sampling == 420 ? 17 : 18);
        Segment(packet, 192, new byte[] { 8, 1, 3, 2, 1, 3, 1, lumaSampling, 0,
            2, chromaSampling, 0, 3, chromaSampling, 0 });
        Segment(packet, 218, new byte[] { 3, 1, 0, 2, 17, 3, 17, 0, 63, 0 });
        return packet;
    }

    private static void Segment(List<byte> output, byte marker, byte[] payload)
    {
        int length = payload.Length + 2;
        output.Add(255); output.Add(marker); output.Add((byte)(length >> 8)); output.Add((byte)length); output.AddRange(payload);
    }

    internal sealed class ScanWriter
    {
        private readonly List<byte> output;
        private readonly CodeTable[] tables;
        private readonly int[][] histograms;
        private int partial, used;
        public ScanWriter(List<byte> output, CodeTable[] tables, int[][] histograms)
        { this.output = output; this.tables = tables; this.histograms = histograms; }
        public void Symbol(int table, int symbol)
        {
            histograms[table][symbol]++;
            if (tables == null) return;
            int length = tables[table].Lengths[symbol];
            if (length == 0) throw new InvalidOperationException($"Undefined symbol {symbol} in table {table}.");
            Bits(tables[table].Codes[symbol], length);
        }
        public void Coefficient(int table, int run, int value)
        {
            int bits = value == 0 ? 0 : BitOperations.Log2((uint)Math.Abs(value)) + 1;
            Symbol(table, run * 16 + bits);
            Bits(value < 0 ? value + (1 << bits) - 1 : value, bits);
        }
        private void Bits(int value, int bits)
        {
            if (tables == null) return;
            for (int bit = bits - 1; bit >= 0; bit--)
            {
                partial = partial * 2 + ((value >> bit) & 1);
                if (++used != 8) continue;
                output.Add((byte)partial); if (partial == 255) output.Add(0);
                partial = 0; used = 0;
            }
        }
        public void Finish() { if (used != 0) Bits(255, 8 - used); }
    }

    internal static int[] MakeZigzag()
    {
        var result = new int[64]; int i = 0;
        for (int diagonal = 0; diagonal <= 14; diagonal++)
        {
            int first = Math.Max(0, diagonal - 7), last = Math.Min(7, diagonal);
            for (int p = first; p <= last; p++)
            {
                int y = diagonal % 2 == 0 ? last - p + first : p;
                result[i++] = y * 8 + diagonal - y;
            }
        }
        return result;
    }
}
