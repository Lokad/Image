namespace Lokad.Image.Tooling;

// Original deterministic byte controls for streaming Deflate, not image inputs.
// Copies, overlap and first mismatches exercise logical/64KiB history edges.
internal static class OriginalDeflateMatch
{
    internal static byte[] Create(bool repeated)
    {
        var bytes = new byte[3 * 65536 + 313]; uint state = 0x947ac531;
        for (var i = 0; i < bytes.Length; i++)
        {
            state = unchecked(state * 1664525 + 1013904223);
            bytes[i] = repeated ? (i % 509 == 0 ? (byte)(i / 509) : (byte)0) : (byte)(state >> 24);
        }
        int[] boundaries = [32768, 65536, 98304, 131072, 163840]; int[] mismatch = [3, 7, 31, 127, 257];
        for (var i = 0; i < boundaries.Length; i++)
        {
            var edge = boundaries[i]; var target = edge - 11;
            bytes.AsSpan(edge - 5500, 258).CopyTo(bytes.AsSpan(target)); bytes[target + mismatch[i]] ^= 0x80;
            bytes.AsSpan(target, 258).CopyTo(bytes.AsSpan(edge + 1200)); bytes[edge + 1200 + mismatch[4 - i]] ^= 0x01;
        }
        return bytes;
    }
}
