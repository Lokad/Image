// Original source-only grayscale JPEG prototype. T.81 B.2/C/F.1 and the JFIF
// APP0 layout; every transform, histogram, code table and packet byte is produced
// independently. No packet, coefficient or native-table inputs are accepted.
using System;
using System.Collections.Generic;

public static class JpegGrayPacketParity
{
    public sealed class Result
    {
        public byte[] Packet = Array.Empty<byte>();
        public int[][] Histograms = Array.Empty<int[]>();
    }

    public static Result Encode(byte[] gray) => Encode(gray, 0);

    public static Result Encode(byte[] gray, int restartInterval)
    {
        if (gray.Length != 513 * 259) throw new ArgumentException("Frozen grayscale source required.");
        if (restartInterval < 0 || restartInterval > 65535) throw new ArgumentOutOfRangeException(nameof(restartInterval));
        ushort[] quantization = JpegGrayParity.Quantization(69);
        int[][] frequencies = Histograms();
        Pass(gray, quantization, frequencies, null, new List<byte>(), restartInterval);
        var tables = new JpegScanParity.CodeTable[4];
        tables[0] = JpegGrayReferenceTables.Huffman(frequencies[0]);
        tables[2] = JpegGrayReferenceTables.Huffman(frequencies[2]);
        var packet = Headers(quantization, tables, restartInterval);
        Pass(gray, quantization, Histograms(), tables, packet, restartInterval);
        packet.Add(255); packet.Add(217);
        return new Result { Packet = packet.ToArray(), Histograms = frequencies };
    }

    private static int[][] Histograms()
    {
        var result = new int[4][];
        for (int i = 0; i < 4; i++) result[i] = new int[256];
        return result;
    }

    private static void Pass(byte[] gray, ushort[] quantization, int[][] frequencies,
        JpegScanParity.CodeTable[] tables, List<byte> packet, int restartInterval)
    {
        Span<byte> block = stackalloc byte[64];
        Span<short> transformed = stackalloc short[64], coefficients = stackalloc short[64];
        int previous = 0; int[] zigzag = JpegScanParity.MakeZigzag();
        var writer = new JpegScanParity.ScanWriter(packet, tables, frequencies);
        int mcu = 0;
        for (int top = 0; top < 259; top += 8)
            for (int left = 0; left < 513; left += 8)
            {
                // One block per MCU. Reset in both the histogram and coding
                // passes; entropy padding precedes each non-final restart.
                if (restartInterval != 0 && mcu != 0 && mcu % restartInterval == 0)
                {
                    previous = 0;
                    if (tables != null)
                    {
                        writer.Finish(); packet.Add(255);
                        packet.Add((byte)(208 + ((mcu / restartInterval - 1) & 7)));
                    }
                }
                mcu++;
                for (int y = 0; y < 8; y++)
                    for (int x = 0; x < 8; x++)
                        block[y * 8 + x] = gray[Math.Min(top + y, 258) * 513 + Math.Min(left + x, 512)];
                JpegGrayParity.Forward(block, transformed);
                JpegGrayParity.Quantize(transformed, quantization, coefficients);
                writer.Coefficient(0, 0, coefficients[0] - previous); previous = coefficients[0];
                int run = 0;
                for (int k = 1; k < 64; k++)
                {
                    int value = coefficients[zigzag[k]];
                    if (value == 0) { run++; continue; }
                    while (run >= 16) { writer.Symbol(2, 240); run -= 16; }
                    writer.Coefficient(2, run, value); run = 0;
                }
                if (run != 0) writer.Symbol(2, 0);
            }
        writer.Finish();
    }

    private static List<byte> Headers(ushort[] quantization, JpegScanParity.CodeTable[] tables, int restartInterval)
    {
        var packet = new List<byte>(65000) { 255, 216 };
        // Normative JFIF APP0 fields; version1.01, unitless density1:1, no thumbnail.
        Segment(packet, 224, new byte[] { 74, 70, 73, 70, 0, 1, 1, 0, 0, 1, 0, 1, 0, 0 });
        var quant = new byte[65]; int[] zigzag = JpegScanParity.MakeZigzag();
        for (int i = 0; i < 64; i++) quant[i + 1] = (byte)quantization[zigzag[i]];
        Segment(packet, 219, quant);
        Segment(packet, 192, new byte[] { 8, 1, 3, 2, 1, 1, 1, 17, 0 });
        foreach (int index in new[] { 0, 2 })
        {
            JpegScanParity.CodeTable table = tables[index];
            var payload = new byte[17 + table.Symbols.Length]; payload[0] = (byte)(index == 0 ? 0 : 16);
            table.Counts.CopyTo(payload, 1); table.Symbols.CopyTo(payload, 17);
            Segment(packet, 196, payload);
        }
        if (restartInterval != 0)
            Segment(packet, 221, new byte[] { (byte)(restartInterval >> 8), (byte)restartInterval });
        Segment(packet, 218, new byte[] { 1, 1, 0, 0, 63, 0 });
        return packet;
    }

    private static void Segment(List<byte> packet, byte marker, byte[] payload)
    {
        packet.Add(255); packet.Add(marker);
        packet.Add((byte)((payload.Length + 2) >> 8)); packet.Add((byte)(payload.Length + 2));
        packet.AddRange(payload);
    }
}
