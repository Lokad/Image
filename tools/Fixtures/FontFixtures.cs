using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Lokad.Image;
using Lokad.Image.Tooling;

internal static class FontFixtures
{
    internal static void Generate(string output)
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
        Console.WriteLine("Generated three original font geometry controls; no image corpus changed.");
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
