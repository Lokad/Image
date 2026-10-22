// Original source-only packet prototype for ONE frozen RGBA8/native level6 control.
// RFC1950/1951 and the PNG specification define framing, checksums and canonical
// coding. This is development tooling, not an admitted production encoder.
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;

public static class PngPacketParity
{
    // RFC1951 section3.2.7 transmission order, not an implementation table.
    private static readonly int[] Order = { 16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15 };
    public sealed record Result(byte[] Packet, int[] BlockEndBits);

    private sealed class Bits
    {
        private readonly List<byte> bytes = new();
        private uint pending;
        private int used;
        public int Position { get; private set; }
        public void Put(int value, int width)
        {
            pending |= (uint)value << used; used += width; Position += width;
            while (used >= 8) { bytes.Add((byte)pending); pending >>= 8; used -= 8; }
        }
        public byte[] Finish() { if (used != 0) bytes.Add((byte)pending); return bytes.ToArray(); }
    }

    private static int[] Codes(byte[] lengths)
    {
        var codes = new int[lengths.Length]; int next = 0;
        for (int width = 1; width <= 15; width++)
        {
            for (int symbol = 0; symbol < lengths.Length; symbol++)
            {
                if (lengths[symbol] != width) continue;
                int reverse = 0, value = next++;
                for (int bit = 0; bit < width; bit++) { reverse = reverse << 1 | value & 1; value >>= 1; }
                codes[symbol] = reverse;
            }
            next <<= 1;
        }
        return codes;
    }

    public static Result Encode(byte[] source)
    {
        byte[] filtered = PngDeflateParity.FilteredRows(source);
        byte[] tokens = PngDeflateParity.EncodeTokens(source);
        PngHuffmanParity.Block[] blocks = PngHuffmanParity.Generate(source);
        var bits = new Bits(); var ends = new int[blocks.Length];
        for (int index = 0; index < blocks.Length; index++)
        {
            PngHuffmanParity.Block block = blocks[index];
            int[] literal = Codes(block.Literals), distance = Codes(block.Distances), lengths = Codes(block.Lengths);
            int firstBit = bits.Position, codeCount = 19;
            while (codeCount > 4 && block.Lengths[Order[codeCount - 1]] == 0) codeCount--;
            bits.Put(index == blocks.Length - 1 ? 1 : 0, 1); bits.Put(2, 2);
            bits.Put(block.Literals.Length - 257, 5); bits.Put(block.Distances.Length - 1, 5); bits.Put(codeCount - 4, 4);
            for (int i = 0; i < codeCount; i++) bits.Put(block.Lengths[Order[i]], 3);
            foreach (PngHuffmanParity.Run run in block.Runs)
            { bits.Put(lengths[run.Symbol], block.Lengths[run.Symbol]); bits.Put(run.Extra, run.ExtraBits); }
            int first = index * 16383, end = Math.Min(first + 16383, tokens.Length / 6);
            int fixedBits = 3 + 7, expanded = 0;
            for (int token = first; token < end; token++)
            {
                int at = token * 6, length = tokens[at + 2] | tokens[at + 3] << 8;
                if (length == 0)
                {
                    int value = tokens[at]; bits.Put(literal[value], block.Literals[value]); expanded++;
                    fixedBits += value < 144 ? 8 : 9;
                    continue;
                }
                int back = tokens[at + 4] | tokens[at + 5] << 8;
                int ls = PngHuffmanParity.LengthSymbol(length), ds = PngHuffmanParity.DistanceSymbol(back);
                int lengthExtra = ls is >= 265 and < 285 ? (ls - 261) / 4 : 0;
                int lengthBase = ls < 265 ? ls - 254 : ls == 285 ? 258 : 3 + ((4 + (ls - 265) % 4) << lengthExtra);
                int distanceExtra = ds < 4 ? 0 : ds / 2 - 1;
                int distanceBase = ds < 4 ? ds + 1 : 1 + ((2 + ds % 2) << distanceExtra);
                bits.Put(literal[ls], block.Literals[ls]); bits.Put(length - lengthBase, lengthExtra);
                bits.Put(distance[ds], block.Distances[ds]); bits.Put(back - distanceBase, distanceExtra);
                fixedBits += (ls < 280 ? 7 : 8) + 5 + lengthExtra + distanceExtra; expanded += length;
            }
            bits.Put(literal[256], block.Literals[256]); ends[index] = bits.Position;
            int dynamicBytes = (bits.Position - firstBit + 7) / 8;
            // Decide from generated coding cost. Other block forms are a later
            // required stage, not a silent assumption about arbitrary inputs.
            if ((fixedBits + 7) / 8 <= dynamicBytes || expanded + 4 <= dynamicBytes)
                throw new NotSupportedException("This bounded prototype requires the cheaper dynamic block.");
        }
        byte[] deflate = bits.Finish();
        using var zlib = new MemoryStream();
        int cmf = 7 << 4 | 8, flags = 2 << 6;
        flags += (31 - ((cmf << 8 | flags) % 31)) % 31;
        zlib.WriteByte((byte)cmf); zlib.WriteByte((byte)flags); zlib.Write(deflate);
        uint a = 1, b = 0;
        foreach (byte value in filtered) { a = (a + value) % 65521; b = (b + a) % 65521; }
        Write32(zlib, b << 16 | a);

        using var png = new MemoryStream();
        png.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        var header = new byte[13];
        BinaryPrimitives.WriteInt32BigEndian(header, 513); BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), 259);
        header[8] = 8; header[9] = 6; Chunk(png, "IHDR", header);
        var physical = new byte[9]; physical[7] = 1; Chunk(png, "pHYs", physical);
        byte[] payload = zlib.ToArray();
        for (int at = 0; at < payload.Length; at += 4096)
            Chunk(png, "IDAT", payload.AsSpan(at, Math.Min(4096, payload.Length - at)));
        Chunk(png, "IEND", ReadOnlySpan<byte>.Empty);
        return new(png.ToArray(), ends);
    }

    private static void Write32(Stream stream, uint value)
    { Span<byte> bytes = stackalloc byte[4]; BinaryPrimitives.WriteUInt32BigEndian(bytes, value); stream.Write(bytes); }

    private static void Chunk(Stream output, string kind, ReadOnlySpan<byte> payload)
    {
        byte[] type = Encoding.ASCII.GetBytes(kind); Write32(output, (uint)payload.Length);
        output.Write(type); output.Write(payload);
        uint crc = uint.MaxValue;
        foreach (byte value in type) crc = Update(crc, value);
        foreach (byte value in payload) crc = Update(crc, value);
        Write32(output, ~crc);
    }

    private static uint Update(uint crc, byte value)
    {
        crc ^= value;
        for (int i = 0; i < 8; i++) crc = crc >> 1 ^ ((crc & 1) != 0 ? 0xEDB88320U : 0);
        return crc;
    }
}
