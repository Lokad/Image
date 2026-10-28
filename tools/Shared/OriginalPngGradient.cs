using Lokad.Image;

namespace Lokad.Image.Tooling;

// Original deterministic arithmetic fixture. No encoded input or upstream image
// is retained. The 3000-square RGBA16 recipe reproduces predictable-content PNG
// growth independently of the asymmetric packet-compatibility controls.
internal static class OriginalPngGradient
{
    internal static ImageBuffer Create(int size = 3000, int depth = 16, bool transparent = false, IMemoryBudget? budget = null)
    {
        var result = ImageBuffer.Create(size, size, depth, ImageLimits.Default with { MemoryBudget = budget });
        for (var y = 0; y < size; y++) for (var x = 0; x < size; x++)
        {
            if (depth == 16)
            {
                var row = result.GetRow16(y); row[x * 4] = (ushort)(16 * x); row[x * 4 + 1] = (ushort)(16 * y);
                row[x * 4 + 2] = (ushort)(8 * (x + y));
                row[x * 4 + 3] = transparent ? ((x + y) % 7 == 0 ? (ushort)0 : (ushort)(13 * x + 7 * y)) : ushort.MaxValue;
            }
            else
            {
                var row = result.GetRow8(y); row[x * 4] = (byte)x; row[x * 4 + 1] = (byte)y; row[x * 4 + 2] = (byte)((x + y) / 2);
                row[x * 4 + 3] = transparent ? ((x + y) % 7 == 0 ? (byte)0 : (byte)((3 * x + y) / 4)) : byte.MaxValue;
            }
        }
        return result;
    }
}
