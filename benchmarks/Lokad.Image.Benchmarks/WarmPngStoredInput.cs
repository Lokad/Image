using System.Buffers.Binary;

// Original structural check; stored blocks follow RFC 1951 section 3.2.4:
// https://www.rfc-editor.org/rfc/rfc1951#section-3.2.4
internal static class WarmPngStoredInput
{
    internal static void Validate(ReadOnlySpan<byte> zlib, ReadOnlySpan<byte> rgba)
    {
        if (zlib.Length < 11 || zlib[0] != 0x78 || (zlib[1] & 0xe0) != 0
            || ((zlib[0] << 8) | zlib[1]) % 31 != 0)
            throw new InvalidOperationException("Stored PNG zlib header differs.");
        var position = 2; var rawPosition = 0; var final = false;
        const int rowBytes = HorizontalFlipInput.Width * 4;
        while (!final)
        {
            if (position + 5 > zlib.Length - 4 || (zlib[position] & 6) != 0)
                throw new InvalidOperationException("NoCompression PNG contains a non-stored Deflate block.");
            final = (zlib[position++] & 1) != 0;
            var length = BinaryPrimitives.ReadUInt16LittleEndian(zlib[position..]);
            var complement = BinaryPrimitives.ReadUInt16LittleEndian(zlib[(position + 2)..]);
            position += 4;
            if ((length ^ complement) != ushort.MaxValue || position + length > zlib.Length - 4)
                throw new InvalidOperationException("Stored Deflate length/complement differs.");
            foreach (var value in zlib.Slice(position, length))
            {
                var row = rawPosition / (rowBytes + 1); var column = rawPosition % (rowBytes + 1);
                if (row >= HorizontalFlipInput.Height
                    || value != (column == 0 ? 0 : rgba[row * rowBytes + column - 1]))
                    throw new InvalidOperationException("Stored Deflate scanlines differ.");
                rawPosition++;
            }
            position += length;
        }
        if (position != zlib.Length - 4 || rawPosition != (rowBytes + 1) * HorizontalFlipInput.Height)
            throw new InvalidOperationException("Stored Deflate boundary/size differs.");
    }
}
