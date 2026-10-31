using System.Buffers.Binary;
using System.Text;

namespace Lokad.Image.Tooling;

/// <summary>Original VP8L constructions with deliberately chosen symbols and pixels.</summary>
internal static class AuthoredWebp
{
    // Original RFC 9649 construction. One 12-bit lookup prefix has only longer
    // canonical leaves; its zero entry must still select canonical traversal.
    public static byte[] LongGreenCacheRaster()
    {
        const int width = 257, height = 33, alphabet = 280 + 1024;
        var bits = new Bits(); bits.Write(0, 1); // No transforms.
        bits.Write(1, 1); bits.Write(10, 4); bits.Write(0, 1); // Cache, one group.
        bits.Write(0, 1); bits.Write(15, 4); // Full code-length alphabet.
        int[] order = [17, 18, 0, 1, 2, 3, 4, 5, 16, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15];
        foreach (var symbol in order) bits.Write(symbol < 16 ? 4 : 0, 3);
        bits.Write(0, 1); // Encode every green-alphabet length.
        for (var symbol = 0; symbol < alphabet; symbol++)
        {
            var length = symbol < 1023 ? 10 : symbol <= 1028 ? Math.Min(15, symbol - 1012) : 0;
            bits.Write(Reverse(length, 4), 4);
        }
        Simple(bits, 0); Simple(bits, 0); Simple(bits, 0); Simple(bits, 0);
        // Last leaf: code32767/15bits, cache index748, initially ARGBzero.
        for (var pixel = 0; pixel < width * height; pixel++) bits.Write(32767, 15);
        return Container(width, height, bits.Finish(), true);
    }

    public static byte[] Predictor(int mode)
    {
        var bits = new Bits(); bits.Write(1, 1); bits.Write(0, 2); bits.Write(0, 3);
        ConstantImage(bits, (uint)(0xFF000000 | mode << 8), false);
        bits.Write(0, 1); ConstantImage(bits, 0x00010101, true);
        return Container(3, 3, bits.Finish(), false);
    }

    public static byte[] ColorTransform()
    {
        var bits = new Bits(); bits.Write(1, 1); bits.Write(1, 2); bits.Write(0, 3);
        ConstantImage(bits, 0xFFF010E0, false);
        bits.Write(0, 1); ConstantImage(bits, 0x8014C80A, true);
        return Container(5, 3, bits.Finish(), true);
    }

    public static byte[] SubtractGreen()
    {
        var bits = new Bits(); bits.Write(1, 1); bits.Write(2, 2); bits.Write(0, 1);
        ConstantImage(bits, 0x00FA14F5, true);
        return Container(3, 2, bits.Finish(), false); // Alpha hint deliberately unset.
    }

    public static byte[] Palette(int count, bool thenPredictor = false)
    {
        var packing = count <= 2 ? 3 : count <= 4 ? 2 : count <= 16 ? 1 : 0;
        var width = 9; var codedWidth = (width + (1 << packing) - 1) >> packing;
        var bits = new Bits(); bits.Write(1, 1); bits.Write(3, 2); bits.Write(count - 1, 8);
        ConstantImage(bits, 0xFF0A141E, false); // Palette entries are channel-wise accumulated deltas.
        if (thenPredictor)
        {
            bits.Write(1, 1); bits.Write(0, 2); bits.Write(0, 3);
            ConstantImage(bits, 0xFF000000, false);
        }
        bits.Write(0, 1); bits.Write(0, 1); bits.Write(0, 1);
        LiteralGreenTree(bits, 280); Simple(bits, 0); Simple(bits, 0); Simple(bits, thenPredictor ? 0 : 255); Simple(bits, 0);
        var previous = 0;
        for (var packed = 0; packed < codedWidth; packed++)
        {
            var value = 0;
            for (var part = 0; part < 1 << packing; part++)
            {
                var x = (packed << packing) + part;
                if (x < width) value |= PaletteIndex(x, count, packing) << (part * (8 >> packing));
            }
            bits.Write(Reverse(thenPredictor ? (value - previous) & 255 : value, 8), 8);
            previous = value;
        }
        return Container(width, 1, bits.Finish(), true);
    }

    public static byte[] ExpectedPalette(int count)
    {
        var packing = count <= 2 ? 3 : count <= 4 ? 2 : count <= 16 ? 1 : 0;
        var expected = new byte[9 * 4];
        for (var x = 0; x < 9; x++)
        {
            var index = PaletteIndex(x, count, packing);
            if (index >= count) continue;
            expected[x * 4] = (byte)(10 * (index + 1)); expected[x * 4 + 1] = (byte)(20 * (index + 1));
            expected[x * 4 + 2] = (byte)(30 * (index + 1)); expected[x * 4 + 3] = (byte)(255 * (index + 1));
        }
        return expected;
    }

    public static byte[] CachedColor(int cacheBits = 1)
    {
        if (cacheBits is < 1 or > 11) throw new ArgumentOutOfRangeException(nameof(cacheBits));
        const uint color = 0xFF140C1E;
        var cacheIndex = (int)(unchecked(color * 0x1E35A7BDu) >> (32 - cacheBits));
        var bits = new Bits(); bits.Write(0, 1); bits.Write(1, 1); bits.Write(cacheBits, 4); bits.Write(0, 1);
        SparseTree(bits, 280 + (1 << cacheBits), 12, 280 + cacheIndex); Simple(bits, 20); Simple(bits, 30); Simple(bits, 255); Simple(bits, 0);
        bits.Write(0, 1); bits.Write(1, 1);
        return Container(2, 1, bits.Finish(), false);
    }

    public static byte[] BackwardReference(int width = 5, bool missingLiteral = false)
    {
        var bits = new Bits(); bits.Write(0, 1); bits.Write(0, 1); bits.Write(0, 1);
        SparseTree(bits, 280, missingLiteral ? 259 : 12, 259);
        Simple(bits, 20); Simple(bits, 30); Simple(bits, 255); Simple(bits, 1); // Distance code 2 maps to one previous pixel.
        if (!missingLiteral) { bits.Write(0, 1); bits.Write(1, 1); }
        return Container(width, 1, bits.Finish(), false);
    }

    public static byte[] EntropyGroups(int groupCount = 2)
    {
        if (groupCount is < 2 or > 256) throw new ArgumentOutOfRangeException(nameof(groupCount));
        var bits = new Bits(); bits.Write(0, 1); bits.Write(0, 1); bits.Write(1, 1); bits.Write(0, 3);
        bits.Write(0, 1); Simple(bits, 0, groupCount - 1); Simple(bits, 0); Simple(bits, 0); Simple(bits, 255); Simple(bits, 0);
        bits.Write(0, 1); bits.Write(1, 1); // Entropy-image green values 0 and groupCount-1.
        for (var group = 0; group < groupCount; group++)
        {
            var color = group == 0 ? 0xFF0A141Eu : 0xFF28323Cu;
            Simple(bits, (int)(color >> 8 & 255)); Simple(bits, (int)(color >> 16 & 255));
            Simple(bits, (int)(color & 255)); Simple(bits, (int)(color >> 24)); Simple(bits, 0);
        }
        return Container(8, 1, bits.Finish(), false);
    }

    public static byte[] EntropyGroupsWithCrossingCopy()
    {
        // A 3x2 map of four-pixel tiles, including a partial rightmost tile.
        var bits = new Bits(); bits.Write(0, 1); bits.Write(0, 1); bits.Write(1, 1); bits.Write(0, 3);
        bits.Write(0, 1); Simple(bits, 0, 1); Simple(bits, 0); Simple(bits, 0); Simple(bits, 255); Simple(bits, 0);
        foreach (var group in new[] { 0, 1, 0, 1, 0, 1 }) bits.Write(group, 1);
        SparseTree(bits, 280, 12, 266); Simple(bits, 20); Simple(bits, 30); Simple(bits, 255); Simple(bits, 1);
        Simple(bits, 50); Simple(bits, 40); Simple(bits, 60); Simple(bits, 255); Simple(bits, 1);
        bits.Write(0, 1); bits.Write(0, 1); bits.Write(0, 1); // Three literal pixels.
        bits.Write(1, 1); bits.Write(3, 4); // Length prefix 10: copy 36 previous pixels, ending at (3,4).
        for (var position = 39; position < 54; position++)
            if (((position / 9 / 4 * 3 + position % 9 / 4) & 1) == 0) bits.Write(0, 1);
        return Container(9, 6, bits.Finish(), false);
    }

    public static byte[] DuplicateSimpleSymbols()
    {
        var bits = new Bits(); bits.Write(0, 1); bits.Write(0, 1); bits.Write(0, 1);
        Simple(bits, 12, 12); Simple(bits, 20); Simple(bits, 30); Simple(bits, 255); Simple(bits, 0);
        return Container(1, 1, bits.Finish(), false);
    }

    public static byte[] ConstantRaster(int width, int height)
    {
        if (width is < 1 or > 16384 || height is < 1 or > 16384) throw new ArgumentOutOfRangeException(nameof(width));
        var bits = new Bits(); bits.Write(0, 1); ConstantImage(bits, 0xFF140C1E, true);
        return Container(width, height, bits.Finish(), false);
    }

    public static byte[] CodeLengthRepeats(bool defaultEight)
    {
        var bits = new Bits(); bits.Write(0, 1); bits.Write(0, 1); bits.Write(0, 1);
        bits.Write(0, 1);
        if (defaultEight)
        {
            bits.Write(8, 4);
            for (var i = 0; i < 12; i++) bits.Write(i == 8 ? 1 : i is 1 or 11 ? 2 : 0, 3);
            bits.Write(1, 1); bits.Write(3, 3); bits.Write(83, 8); // 85 code-length tokens, not 85 expanded positions.
            for (var token = 0; token < 85; token++) { bits.Write(0, 1); bits.Write(token == 0 ? 1 : 0, 2); }
            Simple(bits, 20); Simple(bits, 30); Simple(bits, 255); Simple(bits, 0);
            bits.Write(Reverse(128, 8), 8);
        }
        else
        {
            bits.Write(0, 4); // Code-length symbols: 1 -> 0; 17 -> 10; 18 -> 11.
            foreach (var value in new[] { 2, 2, 0, 1 }) bits.Write(value, 3);
            bits.Write(0, 1);
            bits.Write(3, 2); bits.Write(1, 7); // 12 zeros.
            bits.Write(0, 1); // symbol 12 has length 1.
            bits.Write(3, 2); bits.Write(127, 7); bits.Write(3, 2); bits.Write(97, 7); // 246 zeros.
            bits.Write(0, 1); // symbol 259 has length 1.
            bits.Write(1, 2); bits.Write(7, 3); bits.Write(1, 2); bits.Write(7, 3); // 20 zeros (code 17 = canonical 10).
            Simple(bits, 20); Simple(bits, 30); Simple(bits, 255); Simple(bits, 1);
            bits.Write(0, 1); bits.Write(1, 1); // Literal followed by an overlapping length-4 copy.
        }
        return Container(defaultEight ? 1 : 5, 1, bits.Finish(), false);
    }

    public static byte[] InvalidCache(int size)
    {
        var bits = new Bits(); bits.Write(0, 1); bits.Write(1, 1); bits.Write(size, 4);
        return Container(1, 1, bits.Finish(), false);
    }

    public static byte[] OrientedPalette(bool metadataFirst)
    {
        var original = Palette(17);
        var length = BinaryPrimitives.ReadInt32LittleEndian(original.AsSpan(16));
        byte[] canvas = [0x18, 0, 0, 0, 8, 0, 0, 0, 0, 0];
        byte[] exif = [73, 73, 42, 0, 8, 0, 0, 0, 1, 0, 18, 1, 3, 0, 1, 0, 0, 0, 6, 0, 0, 0, 0, 0, 0, 0];
        using var body = new MemoryStream(); body.Write("WEBP"u8);
        Chunk(body, "VP8X", canvas);
        if (metadataFirst) Chunk(body, "EXIF", exif);
        Chunk(body, "VP8L", original.AsSpan(20, length).ToArray());
        if (!metadataFirst) Chunk(body, "EXIF", exif);
        using var result = new MemoryStream(); result.Write("RIFF"u8);
        Span<byte> size = stackalloc byte[4]; BinaryPrimitives.WriteInt32LittleEndian(size, (int)body.Length);
        result.Write(size); result.Write(body.ToArray()); return result.ToArray();

        static void Chunk(Stream output, string name, byte[] payload)
        {
            output.Write(Encoding.ASCII.GetBytes(name));
            Span<byte> size = stackalloc byte[4]; BinaryPrimitives.WriteInt32LittleEndian(size, payload.Length);
            output.Write(size); output.Write(payload); if ((payload.Length & 1) != 0) output.WriteByte(0);
        }
    }

    public static byte[] WrapPayload(byte[] payload)
    {
        var result = new byte[20 + payload.Length + (payload.Length & 1)];
        "RIFF"u8.CopyTo(result); BinaryPrimitives.WriteInt32LittleEndian(result.AsSpan(4), result.Length - 8);
        "WEBPVP8L"u8.CopyTo(result.AsSpan(8)); BinaryPrimitives.WriteInt32LittleEndian(result.AsSpan(16), payload.Length);
        payload.CopyTo(result, 20); return result;
    }

    private static byte[] Container(int width, int height, byte[] stream, bool alpha)
    {
        var payload = new byte[5 + stream.Length]; payload[0] = 0x2F;
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(1), (uint)(width - 1) | (uint)(height - 1) << 14 | (alpha ? 1u << 28 : 0));
        stream.CopyTo(payload, 5); return WrapPayload(payload);
    }

    private static void ConstantImage(Bits bits, uint color, bool spatial)
    {
        bits.Write(0, 1); if (spatial) bits.Write(0, 1);
        Simple(bits, (int)(color >> 8 & 255)); Simple(bits, (int)(color >> 16 & 255));
        Simple(bits, (int)(color & 255)); Simple(bits, (int)(color >> 24)); Simple(bits, 0);
    }

    private static void Simple(Bits bits, int first, int? second = null)
    {
        bits.Write(1, 1); bits.Write(second is null ? 0 : 1, 1);
        bits.Write(first <= 1 ? 0 : 1, 1); bits.Write(first, first <= 1 ? 1 : 8);
        if (second is not null) bits.Write(second.Value, 8);
    }

    private static void SparseTree(Bits bits, int alphabet, int first, int second)
    {
        bits.Write(0, 1); bits.Write(0, 4);
        foreach (var value in new[] { 0, 0, 1, 1 }) bits.Write(value, 3); // Code-length symbols 0 and 1.
        bits.Write(0, 1);
        for (var symbol = 0; symbol < alphabet; symbol++) bits.Write(symbol == first || symbol == second ? 1 : 0, 1);
    }

    private static void LiteralGreenTree(Bits bits, int alphabet)
    {
        bits.Write(0, 1); bits.Write(8, 4); // 12 code-length symbols, through length value 8.
        for (var i = 0; i < 12; i++) bits.Write(i is 2 or 11 ? 1 : 0, 3);
        bits.Write(0, 1);
        for (var i = 0; i < alphabet; i++) bits.Write(i < 256 ? 1 : 0, 1);
    }

    // Original fixed full-alphabet ALPH control, not a production encoder call.
    // RFC 9649 3.7.2.1: complete canonical trees and normal code-length coding.
    internal static byte[] FixedAlphaResiduals(ReadOnlySpan<byte> residuals)
    {
        var bits = new Bits(); bits.Write(0, 1); bits.Write(0, 1); bits.Write(0, 1);
        FixedTree(bits, 280); FixedTree(bits, 256); FixedTree(bits, 256); FixedTree(bits, 256); FixedTree(bits, 40);
        foreach (var green in residuals)
        {
            // 232 eight-bit leaves, followed by48 nine-bit leaves, are complete.
            bits.Write(Reverse(green < 232 ? green : green + 232, green < 232 ? 8 : 9), green < 232 ? 8 : 9);
            bits.Write(0, 8); bits.Write(0, 8); bits.Write(255, 8);
        }
        return bits.Finish();
    }

    private static void FixedTree(Bits bits, int alphabet)
    {
        var lower = (int)Math.Floor(Math.Log2(alphabet));
        var shortLeaves = (1 << (lower + 1)) - alphabet;
        var upper = shortLeaves == alphabet ? lower : lower + 1;
        ReadOnlySpan<byte> order = [17, 18, 0, 1, 2, 3, 4, 5, 16, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15];
        bits.Write(0, 1); bits.Write(15, 4); // All19 code-length positions; no minimum-header dependency.
        foreach (var value in order) bits.Write(value == lower || value == upper ? 1 : 0, 3);
        bits.Write(0, 1);
        if (lower != upper)
            for (var symbol = 0; symbol < alphabet; symbol++) bits.Write(symbol < shortLeaves ? 0 : 1, 1);
    }

    private static int PaletteIndex(int x, int count, int packing) => x % (count + 1) & ((1 << (8 >> packing)) - 1);
    private static int Reverse(int value, int count)
    {
        var reversed = 0;
        for (var i = 0; i < count; i++) { reversed = reversed * 2 + (value & 1); value >>= 1; }
        return reversed;
    }

    private sealed class Bits
    {
        private readonly MemoryStream stream = new();
        private int value, count;
        public void Write(int encoded, int length)
        {
            for (var i = 0; i < length; i++)
            {
                value |= (encoded >> i & 1) << count;
                if (++count == 8) { stream.WriteByte((byte)value); value = 0; count = 0; }
            }
        }
        public byte[] Finish() { if (count != 0) stream.WriteByte((byte)value); return stream.ToArray(); }
    }
}
