using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

// Original staggered, coincident and curved contours. No external font input.
internal static class ActiveEdgeFixture
{
    internal static byte[] Font()
    {
        var contours = new List<(int X, int Y, bool On)[]>();
        for (var i = 0; i < 12; i++)
        {
            var x = i * 23 - 100; var y = i * 73 - 500;
            (int X, int Y, bool On)[] contour =
                [(x, y, true), (x - 70, y + 1000, true), (x + 420, y + 1110, true), (x + 510, y + 40, true)];
            if (i % 3 == 1) Array.Reverse(contour);
            contours.Add(contour);
            if (i == 2) { contours.Add(contour.ToArray()); contours.Add(contour.Reverse().ToArray()); }
        }
        contours.Add([(0, -100, true), (-120, 880, false), (500, 1200, true), (800, 100, false)]);
        return AuthoredTrueType.Build(glyphOverrides: new Dictionary<int, byte[]>
            { [2] = AuthoredTrueType.Outline(contours.ToArray()) });
    }

    internal static byte[] Render(int depth, ImageLimits? limits = null)
    {
        using var font = TrueTypeFont.Load(Font());
        using var layout = TextLayout.Create(font, "A A\nAg A", new() { FontSize = 19.375, LineSpacing = 1.125, WrapWidth = 55 });
        using var canvas = OriginalPixels.Buffer(73, 61, depth);
        ImageDrawing.DrawText(canvas, layout, -9.375, -14.625, new(12345, 54321, 32769, 45678), limits);
        ImageDrawing.DrawText(canvas, layout, 13.625, 8.375, new(54321, 12345, 45678, 26217), limits);
        return OriginalPixels.Bytes(canvas);
    }
}
