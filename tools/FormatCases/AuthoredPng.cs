using System.Buffers.Binary;
using System.Text;

namespace Lokad.Image.Tooling;

// Original PNG authoring. No production codec helpers are used. Format facts:
// https://www.w3.org/TR/png/ sections 8, 9, 10, 11; RFC 1950 and RFC 1951.
internal static class AuthoredPng
{
    internal sealed record Frame(string Name, int ColorType, int Depth, int Width, int Height, bool Interlace, bool Transparency = true);

    public static IEnumerable<Frame> Cases()
    {
        foreach (var color in new[] { 0, 2, 3, 4, 6 })
        foreach (var depth in color switch { 0 => new[] { 1, 2, 4, 8, 16 }, 3 => new[] { 1, 2, 4, 8 }, _ => new[] { 8, 16 } })
        foreach (var interlace in new[] { false, true })
        {
            var stem = $"c{color}-d{depth}-{(interlace ? "adam7" : "plain")}";
            yield return new Frame(stem + "-all-filters", color, depth, 19, 17, interlace);
            yield return new Frame(stem + "-tiny", color, depth, 1, 1, interlace);
            if (color == 0 && depth < 8)
            {
                yield return new Frame(stem + "-opaque-all-filters", color, depth, 19, 17, interlace, false);
                yield return new Frame(stem + "-opaque-tiny", color, depth, 1, 1, interlace, false);
            }
        }
    }

    public static byte[] Build(Frame frame) => Wrap(frame, StoredZlib(FilteredRows(frame)));

    public static byte[] Wrap(Frame frame, byte[] compressed, params (string Type, byte[] Data)[] ancillary)
    {
        using var result = new MemoryStream(); result.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        var header = new byte[13];
        BinaryPrimitives.WriteInt32BigEndian(header, frame.Width); BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), frame.Height);
        header[8] = (byte)frame.Depth; header[9] = (byte)frame.ColorType; header[12] = frame.Interlace ? (byte)1 : (byte)0;
        Chunk(result, "IHDR", header);
        if (frame.ColorType == 3)
        {
            var count = 1 << frame.Depth; var palette = new byte[count * 3];
            for (var i = 0; i < count; i++) Palette(i).CopyTo(palette, i * 3);
            Chunk(result, "PLTE", palette);
            // The omitted tail defaults to opaque, rather than repeating the last alpha.
            if (frame.Transparency) Chunk(result, "tRNS", Enumerable.Range(0, Math.Max(1, count / 2)).Select(Alpha).ToArray());
        }
        else if (frame.ColorType is 0 or 2 && frame.Transparency)
        {
            var samples = Samples(frame, 0, 0); var transparency = new byte[samples.Length * 2];
            for (var i = 0; i < samples.Length; i++) BinaryPrimitives.WriteUInt16BigEndian(transparency.AsSpan(i * 2), (ushort)samples[i]);
            Chunk(result, "tRNS", transparency);
        }
        foreach (var (type, data) in ancillary) Chunk(result, type, data);
        // Empty chunks and boundaries within the zlib header, raster and trailer.
        Chunk(result, "IDAT", []); Chunk(result, "IDAT", compressed.AsSpan(0, Math.Min(1, compressed.Length)));
        for (var position = 1; position < compressed.Length;)
        {
            var length = Math.Min(97, compressed.Length - position); Chunk(result, "IDAT", compressed.AsSpan(position, length)); position += length;
        }
        Chunk(result, "IDAT", []); Chunk(result, "IEND", []); return result.ToArray();
    }

    public static byte[] Expected(Frame frame)
    {
        var bytes = frame.Depth == 16 ? 2 : 1; var result = new byte[frame.Width * frame.Height * 4 * bytes];
        var transparent = Samples(frame, 0, 0);
        for (var y = 0; y < frame.Height; y++)
        for (var x = 0; x < frame.Width; x++)
        {
            var samples = Samples(frame, x, y); int r, g, b, a = bytes == 2 ? 65535 : 255;
            if (frame.ColorType == 3)
            {
                var palette = Palette(samples[0]); r = palette[0]; g = palette[1]; b = palette[2];
                if (frame.Transparency && samples[0] < Math.Max(1, (1 << frame.Depth) / 2)) a = Alpha(samples[0]);
            }
            else
            {
                var maximum = (1 << frame.Depth) - 1;
                r = (int)((long)samples[0] * a / maximum);
                g = frame.ColorType is 0 or 4 ? r : (int)((long)samples[1] * a / maximum);
                b = frame.ColorType is 0 or 4 ? r : (int)((long)samples[2] * a / maximum);
                if (frame.ColorType is 4 or 6) a = samples[^1];
                else if (frame.Transparency && samples.SequenceEqual(transparent)) a = 0;
            }
            var offset = (y * frame.Width + x) * 4 * bytes;
            var rgba = new[] { r, g, b, a };
            for (var channel = 0; channel < 4; channel++)
                if (bytes == 1) result[offset + channel] = (byte)rgba[channel];
                else BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(offset + channel * 2), (ushort)rgba[channel]);
        }
        return result;
    }

    public static byte[] FilteredRows(Frame frame)
    {
        using var result = new MemoryStream();
        if (!frame.Interlace) Pass(0, 0, 1, 1);
        else foreach (var (x, y, dx, dy) in new[] { (0, 0, 8, 8), (4, 0, 8, 8), (0, 4, 4, 8), (2, 0, 4, 4), (0, 2, 2, 4), (1, 0, 2, 2), (0, 1, 1, 2) }) Pass(x, y, dx, dy);
        return result.ToArray();

        void Pass(int firstX, int firstY, int dx, int dy)
        {
            if (firstX >= frame.Width || firstY >= frame.Height) return;
            var width = (frame.Width - firstX + dx - 1) / dx; var channels = frame.ColorType switch { 2 => 3, 4 => 2, 6 => 4, _ => 1 };
            var rowLength = (width * channels * frame.Depth + 7) / 8; var previous = new byte[rowLength];
            var stride = Math.Max(1, channels * frame.Depth / 8); var rowNumber = 0;
            for (var y = firstY; y < frame.Height; y += dy, rowNumber++)
            {
                var row = new byte[rowLength]; var sampleIndex = 0;
                for (var x = firstX; x < frame.Width; x += dx)
                foreach (var sample in Samples(frame, x, y))
                {
                    if (frame.Depth == 16) BinaryPrimitives.WriteUInt16BigEndian(row.AsSpan(sampleIndex * 2), (ushort)sample);
                    else if (frame.Depth == 8) row[sampleIndex] = (byte)sample;
                    else row[sampleIndex * frame.Depth / 8] |= (byte)(sample << (8 - frame.Depth - sampleIndex * frame.Depth % 8));
                    sampleIndex++;
                }
                // Nonzero unused low packed bits are not pixel samples.
                var used = width * channels * frame.Depth % 8;
                if (used != 0) row[^1] |= (byte)((1 << (8 - used)) - 1);
                var filter = rowNumber % 5; result.WriteByte((byte)filter);
                for (var i = 0; i < row.Length; i++)
                {
                    var left = i < stride ? 0 : row[i - stride]; var above = previous[i]; var corner = i < stride ? 0 : previous[i - stride];
                    var predictor = filter switch { 0 => 0, 1 => left, 2 => above, 3 => (left + above) / 2, _ => Paeth(left, above, corner) };
                    result.WriteByte(unchecked((byte)(row[i] - predictor)));
                }
                previous = row;
            }
        }
    }

    public static byte[] StoredZlib(byte[] bytes)
    {
        using var result = new MemoryStream(); result.Write(new byte[] { 0x78, 0x01 });
        Span<byte> lengths = stackalloc byte[4];
        for (var position = 0; position < bytes.Length || position == 0;)
        {
            var count = Math.Min(32768, bytes.Length - position); result.WriteByte(position + count == bytes.Length ? (byte)1 : (byte)0);
            BinaryPrimitives.WriteUInt16LittleEndian(lengths, (ushort)count);
            BinaryPrimitives.WriteUInt16LittleEndian(lengths[2..], unchecked((ushort)~count)); result.Write(lengths); result.Write(bytes.AsSpan(position, count));
            position += count; if (position == bytes.Length) break;
        }
        uint first = 1, second = 0;
        foreach (var value in bytes) { first = (first + value) % 65521; second = (second + first) % 65521; }
        Span<byte> checksum = stackalloc byte[4]; BinaryPrimitives.WriteUInt32BigEndian(checksum, second << 16 | first); result.Write(checksum);
        return result.ToArray();
    }

    public static void Chunk(Stream output, string name, ReadOnlySpan<byte> payload)
    {
        var type = Encoding.ASCII.GetBytes(name); Span<byte> number = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(number, payload.Length); output.Write(number); output.Write(type); output.Write(payload);
        uint remainder = uint.MaxValue;
        foreach (var value in type) Update(value);
        foreach (var value in payload) Update(value);
        BinaryPrimitives.WriteUInt32BigEndian(number, ~remainder); output.Write(number);
        void Update(byte value) { remainder ^= value; for (var bit = 0; bit < 8; bit++) remainder = remainder >> 1 ^ ((remainder & 1) == 0 ? 0 : 0xEDB88320u); }
    }

    private static int[] Samples(Frame frame, int x, int y)
    {
        var maximum = (1 << frame.Depth) - 1; var seed = x * 997 + y * 3701 + x * y * 43;
        var gray = (17 + seed) & maximum;
        return frame.ColorType switch
        {
            0 => [gray], 2 => [gray, (83 + seed * 3) & maximum, (191 + seed * 7) & maximum],
            3 => [(x + frame.Width * y) & maximum], 4 => [gray, (x + y) % 5 == 0 ? 0 : (91 + seed * 11) & maximum],
            _ => [gray, (83 + seed * 3) & maximum, (191 + seed * 7) & maximum, (x + y) % 5 == 0 ? 0 : (91 + seed * 11) & maximum]
        };
    }

    private static byte[] Palette(int index) => [(byte)(index * 73 + 17), (byte)(index * 41 + 83), (byte)(index * 101 + 191)];
    private static byte Alpha(int index) => (byte)(index * 127);
    private static int Paeth(int left, int above, int corner)
    {
        var prediction = left + above - corner;
        return new[] { left, above, corner }.OrderBy(value => Math.Abs(prediction - value)).First();
    }
}
