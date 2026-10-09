namespace Lokad.Image.Internal;

internal static class Crc32
{
    // PNG specifies the reflected polynomial 0xEDB88320. Generate the lookup
    // values from that polynomial; no implementation table is imported.
    private static readonly uint[] Table = BuildTable();

    private static uint[] BuildTable()
    {
        var table = new uint[256];
        for (uint i = 0; i < table.Length; i++)
        {
            var value = i;
            for (var bit = 0; bit < 8; bit++)
                value = (value >> 1) ^ ((value & 1) == 0 ? 0 : 0xEDB88320u);
            table[i] = value;
        }
        return table;
    }

    public static uint Compute(ReadOnlySpan<byte> bytes, CancellationToken cancellation = default)
    {
        var crc = uint.MaxValue;
        for (var i = 0; i < bytes.Length; i++)
        {
            if ((i & 4095) == 0) cancellation.ThrowIfCancellationRequested();
            crc = Table[(crc ^ bytes[i]) & 255] ^ (crc >> 8);
        }
        return ~crc;
    }
}
