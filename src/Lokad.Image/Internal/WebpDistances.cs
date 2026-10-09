namespace Lokad.Image.Internal;

internal static class WebpDistances
{
    // RFC 9649 section 3.6.2.2.1 orders the 120-neighbor geometry by squared
    // radius, preferring larger row offsets and positive columns at ties.
    // Generate that order instead of importing an implementation's lookup table.
    private static readonly (int X, int Y)[] Neighbors = Build();

    private static (int X, int Y)[] Build()
    {
        var points = new List<(int X, int Y)>();
        for (var y = 0; y <= 7; y++)
        for (var x = -7; x <= 8; x++)
            if (y != 0 || x > 0) points.Add((x, y));
        return points.OrderBy(p => p.X * p.X + p.Y * p.Y).ThenByDescending(p => p.Y)
            .ThenBy(p => p.X < 0 ? 1 : 0).ToArray();
    }

    public static int Resolve(int code, int width)
    {
        if (code <= 0 || width <= 0) throw new ImageDataException("Invalid VP8L backward distance.");
        if (code > 120) return code - 120;
        var (x, y) = Neighbors[code - 1]; return Math.Max(1, x + y * width);
    }
}
