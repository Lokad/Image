using System.Buffers.Binary;

namespace Lokad.Image.Internal;

internal static class Crc32
{
    // PNG section 5.5 specifies the reflected polynomial 0xEDB88320.
    // Generate every entry from that polynomial. Slice k represents a byte
    // followed by k zero bytes; linearity combines eight contributions.
    private static readonly uint[] Table = BuildTable();

    private static uint[] BuildTable()
    {
        var table = new uint[8 * 256];
        for (var i = 0; i < 256; i++)
        {
            var value = (uint)i;
            for (var bit = 0; bit < 8; bit++) value = value >> 1 ^ ((value & 1) == 0 ? 0 : 0xEDB88320u);
            table[i] = value;
        }
        for (var i = 0; i < 256; i++)
        {
            var value = table[i];
            for (var slice = 1; slice < 8; slice++)
            {
                value = table[value & 255] ^ (value >> 8); table[slice * 256 + i] = value;
            }
        }
        return table;
    }
    public static uint Compute(ReadOnlySpan<byte> bytes, CancellationToken cancellation = default)
    {
        var crc = uint.MaxValue; var i = 0;
        for (; i <= bytes.Length - 8; i += 8)
        {
            if ((i & 4095) == 0) cancellation.ThrowIfCancellationRequested();
            var word = BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(i, 8)) ^ crc;
            crc = Table[7 * 256 + (int)(word & 255)] ^ Table[6 * 256 + (int)(word >> 8 & 255)]
                ^ Table[5 * 256 + (int)(word >> 16 & 255)] ^ Table[4 * 256 + (int)(word >> 24 & 255)]
                ^ Table[3 * 256 + (int)(word >> 32 & 255)] ^ Table[2 * 256 + (int)(word >> 40 & 255)]
                ^ Table[256 + (int)(word >> 48 & 255)] ^ Table[(int)(word >> 56)];
        }
        for (; i < bytes.Length; i++)
        {
            if ((i & 4095) == 0) cancellation.ThrowIfCancellationRequested();
            crc = Table[(crc ^ bytes[i]) & 255] ^ (crc >> 8);
        }
        return ~crc;
    }
}
