using System.Security.Cryptography;
using System.Text.Json;
using Lokad.Image;

// Original still-image control: fixed rectangles, data and supplied-font text.
// Font bytes are caller supplied, never copied into the fixture set or package.
internal static class CompressionFixtures
{
    internal static void Diagram(string output, string fontPath)
    {
        var pngPath = Path.Combine(output, "diagram-rgba8.png");
        var rawPath = Path.Combine(output, "diagram.rgba");
        if (File.Exists(pngPath) || File.Exists(rawPath)) throw new InvalidOperationException("Reuse the frozen diagram; do not overwrite it.");
        var fontBytes = File.ReadAllBytes(fontPath); var font = TrueTypeFont.Load(fontBytes);
        using var canvas = ImageBuffer.Create(513, 259, 8);
        var white = RgbaColor.From8(255, 255, 255); var dark = RgbaColor.From8(32, 58, 86);
        var teal = RgbaColor.From8(0, 148, 156); var orange = RgbaColor.From8(224, 126, 44);
        var muted = RgbaColor.From8(92, 108, 124); var grid = RgbaColor.From8(218, 225, 232);
        ImageDrawing.FillRectangle(canvas, 0, 0, 513, 259, white);
        ImageDrawing.FillRectangle(canvas, 0, 0, 513, 49, dark);
        Text("Weekly activity", 16, 6, 26, white);
        Text("Units per day", 16, 57, 12, muted);
        ImageDrawing.FillRectangle(canvas, 338, 59, 11, 11, teal); Text("North", 355, 55, 12, dark);
        ImageDrawing.FillRectangle(canvas, 420, 59, 11, 11, orange); Text("South", 437, 55, 12, dark);
        for (var y = 90; y <= 210; y += 30)
        {
            for (var x = 43; x < 493; x += 12) ImageDrawing.FillRectangle(canvas, x, y, 6, 1, grid);
            Text(((210 - y) / 3).ToString(System.Globalization.CultureInfo.InvariantCulture), 17, y - 8, 11, muted);
        }
        ImageDrawing.FillRectangle(canvas, 42, 82, 1, 129, dark);
        ImageDrawing.FillRectangle(canvas, 42, 210, 451, 1, dark);
        string[] days = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
        int[] north = [54, 96, 75, 120, 87, 108], south = [72, 63, 99, 90, 117, 81];
        for (var i = 0; i < days.Length; i++)
        {
            var x = 61 + i * 72;
            ImageDrawing.FillRectangle(canvas, x, 210 - north[i], 19, north[i], teal);
            ImageDrawing.FillRectangle(canvas, x + 23, 210 - south[i], 19, south[i], orange);
            Text(days[i], x + 5, 213, 12, dark);
        }
        Text("Two series  |  Six days  |  Fixed original control", 16, 239, 11, muted);
        var raw = new byte[513 * 259 * 4];
        for (var y = 0; y < 259; y++) canvas.GetRow8(y).CopyTo(raw.AsSpan(y * 513 * 4));
        if (Enumerable.Range(0, 513 * 259).Any(i => raw[i * 4 + 3] != 255)) throw new InvalidOperationException("Diagram must be opaque.");
        File.WriteAllBytes(rawPath, raw);
        using (var file = File.Create(pngPath)) ImageCodec.EncodePng(canvas, file);
        var record = new
        {
            schemaVersion = 1, recipe = "original-text-diagram-v1", width = 513, height = 259, depth = 8, opaque = true,
            fontFileName = Path.GetFileName(fontPath), fontSha256 = Hash(fontBytes), fontStyle = font.Style.ToString(),
            librarySha256 = Hash(File.ReadAllBytes(typeof(ImageBuffer).Assembly.Location)),
            pngSha256 = Hash(File.ReadAllBytes(pngPath)), rgbaSha256 = Hash(raw),
            source = "Original rectangles, fixed data/labels and Lokad.Image supplied-font text; no font file included or required for fixture consumption"
        };
        File.WriteAllText(Path.Combine(output, "diagram-provenance.json"), JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        Console.WriteLine(JsonSerializer.Serialize(record));

        void Text(string text, double x, double y, double size, RgbaColor color)
        {
            var layout = TextLayout.Create(font, text, new() { FontSize = size });
            if (!layout.Bounds.Translate(x, y).FitsInside(513, 259)) throw new InvalidOperationException("Original diagram text overflows.");
            ImageDrawing.DrawText(canvas, layout, x, y, color);
        }
    }
    private static string Hash(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));
}
