using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Lokad.Image;
using Lokad.Image.Tooling;

internal static class FontFixtures
{
    internal static void Generate(string output, bool render = false)
    {
        var ordinary = AuthoredTrueType.Build();
        var components = AuthoredTrueType.Build(glyphOverrides: new Dictionary<int, byte[]>
        {
            [5] = AuthoredTrueType.Compound([new(2, 100, 0, Flags: 3 | 8 | 512, Xx: 8192)], xmin: 125, ymin: 0, xmax: 375, ymax: 350),
            [6] = AuthoredTrueType.Compound([new(2, 0, 0), new(8, 1, 0, Flags: 1 | 8, Xx: 8192)], xmin: 50, ymin: 0, xmax: 550, ymax: 735),
            [7] = AuthoredTrueType.Compound([new(2, 0, 0, Flags: 3 | 128, Xx: 16384, Yx: -8192, Xy: 8192, Yy: 16384)], xmin: 50, ymin: -275, xmax: 650, ymax: 550),
            [8] = AuthoredTrueType.Outline([[(200, 780, true), (200, 850, true), (400, 850, true), (400, 780, true)]])
        });
        AuthoredTrueType.W16(components, AuthoredTrueType.Offset(components, "head") + 16, 0);
        AuthoredTrueType.W16(components, AuthoredTrueType.Offset(components, "hmtx") + 10, 24);
        Profile(components, 7, 2, 2);
        var packed = AuthoredTrueType.Build(longLocations: false, unicode12: false, glyphOverrides: new Dictionary<int, byte[]>
        {
            [2] = AuthoredTrueType.Outline([[(0, 0, false), (0, 200, false), (200, 200, false), (200, 0, false)]]),
            [5] = AuthoredTrueType.Compound([new(2, 100, 100, Flags: 3 | 8 | 0x800, Xx: 8192)], xmin: 50, ymin: 50, xmax: 150, ymax: 150)
        });
        Profile(packed, 4, 1, 1);
        var fonts = new List<object>();
        foreach (var (name, bytes) in new[] { ("original", ordinary), ("components", components), ("packed", packed) })
        {
            var font = TrueTypeFont.Load(bytes); var glyphs = new List<object>();
            foreach (var code in new[] { 65, 103, 193, 769, 0xE000 })
            {
                var layout = TextLayout.Create(font, new Rune(code).ToString(), new() { FontSize = 10, Kerning = false });
                var ink = layout.InkBounds; var scale = layout.FontSize / font.UnitsPerEm; var baseline = layout.GetGlyph(0).BaselineY;
                glyphs.Add(new { codePoint = code, glyphIndex = layout.GetGlyph(0).GlyphIndex,
                    advance = layout.GetGlyph(0).AdvanceWidth / scale,
                    bbox = new[] { ink.X / scale, (baseline - ink.Bottom) / scale, ink.Right / scale, (baseline - ink.Y) / scale } });
            }
            File.WriteAllBytes(Path.Combine(output, name + ".ttf"), bytes);
            fonts.Add(new { name, recipe = "original-truetype-layout-v1", sha256 = Convert.ToHexStringLower(SHA256.HashData(bytes)), bytes = bytes.Length, glyphs });
        }
        var assemblySha256 = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(typeof(TextLayout).Assembly.Location)));
        File.WriteAllText(Path.Combine(output, "managed.json"), JsonSerializer.Serialize(new { schemaVersion = 1, assemblySha256, fonts }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        if (render) Render(output, assemblySha256);
        Console.WriteLine("Generated three original font geometry controls; no image corpus changed.");
    }

    private static void Render(string output, string assemblySha256)
    {
        var rows = new List<object>();
        foreach (var (name, file, code) in new[]
        {
            ("triangle", "original", 65), ("quadratic", "original", 103), ("accent", "original", 193),
            ("implied", "packed", 65), ("metrics", "components", 193), ("matrix", "components", 0xE000)
        })
        {
            var font = TrueTypeFont.Load(File.ReadAllBytes(Path.Combine(output, file + ".ttf")));
            var layout = TextLayout.Create(font, new Rune(code).ToString(), new() { FontSize = 10, Kerning = false });
            foreach (var depth in new[] { 8, 16 })
            {
                using var canvas = ImageBuffer.Create(20, 16, depth);
                ImageDrawing.DrawText(canvas, layout, .25, .5, RgbaColor.From8(255, 255, 255));
                var alpha = new byte[20 * 16 * (depth / 8)];
                for (var y = 0; y < 16; y++)
                for (var x = 0; x < 20; x++)
                    if (depth == 8) alpha[y * 20 + x] = canvas.GetRow8(y)[x * 4 + 3];
                    else System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(alpha.AsSpan((y * 20 + x) * 2), canvas.GetRow16(y)[x * 4 + 3]);
                File.WriteAllBytes(Path.Combine(output, name + ".alpha" + depth), alpha);
            }
            rows.Add(new { name, font = file, codePoint = code, size = 10, width = 20, height = 16, x = .25, y = .5, baseline = layout.GetGlyph(0).BaselineY });
        }
        File.WriteAllText(Path.Combine(output, "managed-render.json"), JsonSerializer.Serialize(new { schemaVersion = 1, assemblySha256, rows }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
    }

    private static void Profile(byte[] bytes, int compositePoints, int compositeContours, int components)
    {
        var maxp = AuthoredTrueType.Offset(bytes, "maxp");
        AuthoredTrueType.W16(bytes, maxp + 6, 4); AuthoredTrueType.W16(bytes, maxp + 8, 1);
        AuthoredTrueType.W16(bytes, maxp + 10, compositePoints); AuthoredTrueType.W16(bytes, maxp + 12, compositeContours);
        AuthoredTrueType.W16(bytes, maxp + 28, components); AuthoredTrueType.W16(bytes, maxp + 30, 1);
        AuthoredTrueType.NormalizeChecksums(bytes);
    }
}
