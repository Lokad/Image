namespace Lokad.Image.Internal;

internal static class JpegTransform
{
    // T.81 diagonal scan order, generated from its geometry.
    public static readonly int[] Zigzag = BuildZigzag();

    private static int[] BuildZigzag()
    {
        var result = new int[64]; var index = 0;
        for (var diagonal = 0; diagonal <= 14; diagonal++)
        {
            var first = Math.Max(0, diagonal - 7); var last = Math.Min(7, diagonal);
            for (var position = first; position <= last; position++)
            {
                var row = (diagonal & 1) == 0 ? last - (position - first) : position;
                var column = diagonal - row;
                result[index++] = row * 8 + column;
            }
        }
        return result;
    }

}
