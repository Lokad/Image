namespace Lokad.Image.Tests;

// Original deterministic primitive inputs. No raster or external corpus.
internal static class JpegInverseBlocks
{
    public static IEnumerable<(int[] Coefficients, ushort[] Quantizers)> Generate(int family)
    {
        uint state = 0xC0DE1234u + (uint)family;
        uint Next() { state ^= state << 13; state ^= state >> 17; state ^= state << 5; return state; }
        var count = family < 3 ? 128 : family == 3 ? 515 : 512;
        for (var block = 0; block < count; block++)
        {
            var coefficients = new int[64]; var quantizers = new ushort[64];
            for (var i = 0; i < 64; i++)
            {
                quantizers[i] = (ushort)(family == 2 ? 65535 : family <= 3 ? 1
                    : family == 7 ? Next() % 2 : 1 + Next() % 65535);
                coefficients[i] = family switch
                {
                    0 or 1 or 2 => i == block / 2 ? ((block & 1) == 0 ? 1 : -1) * (family == 0 ? 1 : family == 1 ? 4 : 2047) : 0,
                    3 => i == 0 ? (block - 257) * 4 : 0,
                    4 => (int)(Next() % 255) - 127,
                    5 => Next() % 8 == 0 ? (int)(Next() % 8191) - 4095 : 0,
                    6 => (int)Next(),
                    7 => (block + i) % 3 == 0 ? int.MinValue : (block + i) % 3 == 1 ? int.MaxValue : (int)(Next() % 1023) - 511,
                    _ => throw new ArgumentOutOfRangeException(nameof(family))
                };
            }
            yield return (coefficients, quantizers);
        }
    }
}
