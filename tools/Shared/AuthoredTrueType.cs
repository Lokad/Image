using System.Buffers.Binary;
using System.Text;

namespace Lokad.Image.Tooling;

// Development-only original font: rectangles, triangles, a quadratic contour and
// a two-component accent. No upstream font, outline, table or source is included.
// Format facts: Microsoft OpenType otff/head/hhea/hmtx/maxp/loca/glyf/cmap/name/OS2/post/kern.
internal static class AuthoredTrueType
{
    internal static byte[] Build(bool longLocations = true, bool unicode12 = true, bool glyphArray = true,
        FontStyle style = FontStyle.Regular, bool shortMetrics = false)
    {
        var glyphs = new byte[8][];
        glyphs[0] = Simple([(0, 0, true), (0, 600, true), (400, 600, true), (400, 0, true)], style);
        glyphs[1] = [];
        glyphs[2] = Simple([(50, 0, true), (300, 700, true), (550, 0, true)], style);
        glyphs[3] = Simple([(50, 700, true), (300, 0, true), (550, 700, true)], style);
        glyphs[4] = Simple([(0, 0, true), (0, 800, false), (600, 0, true), (300, -200, false)], style);
        glyphs[6] = Simple([(200, 780, true), (200, 850, true), (400, 850, true), (400, 780, true)], style);
        glyphs[5] = Composite(glyphs[2], glyphs[6]);
        glyphs[7] = Simple([(100, 100, true), (100, 500, true), (500, 500, true), (500, 100, true)], style);
        using var outline = new MemoryStream(); var offsets = new int[9];
        for (var i = 0; i < glyphs.Length; i++)
        {
            offsets[i] = (int)outline.Length; outline.Write(glyphs[i]);
            while (outline.Length % 4 != 0) outline.WriteByte(0);
        }
        offsets[^1] = (int)outline.Length;
        var locations = new byte[9 * (longLocations ? 4 : 2)];
        for (var i = 0; i < offsets.Length; i++)
            if (longLocations) W32(locations, i * 4, (uint)offsets[i]); else W16(locations, i * 2, offsets[i] / 2);
        var head = new byte[54]; W32(head, 0, 0x00010000); W32(head, 4, 0x00010000); W32(head, 12, 0x5F0F3CF5);
        W16(head, 16, 2); W16(head, 18, 1000); W16(head, 44, (int)style); W16(head, 46, 8); W16(head, 48, 2); W16(head, 50, longLocations ? 1 : 0);
        var nonempty = glyphs.Where(g => g.Length != 0).ToArray();
        W16(head, 36, nonempty.Min(g => I16(g, 2))); W16(head, 38, nonempty.Min(g => I16(g, 4)));
        W16(head, 40, nonempty.Max(g => I16(g, 6))); W16(head, 42, nonempty.Max(g => I16(g, 8)));
        var hhea = new byte[36]; W32(hhea, 0, 0x00010000); W16(hhea, 4, 800); W16(hhea, 6, -200); W16(hhea, 8, 200);
        W16(hhea, 10, 660); W16(hhea, 18, 1); var metricCount = shortMetrics ? 3 : 8; W16(hhea, 34, metricCount);
        var metrics = new byte[metricCount * 4 + (8 - metricCount) * 2];
        int[] advances = [500, 300, 600, 600, 600, 600, 0, 600];
        for (var i = 0; i < 8; i++)
        {
            var bearing = glyphs[i].Length == 0 ? 0 : I16(glyphs[i], 2);
            var advance = (style & FontStyle.Bold) != 0 ? advances[i] * 11 / 10 : advances[i];
            if (i < metricCount) { W16(metrics, i * 4, advance); W16(metrics, i * 4 + 2, bearing); }
            else W16(metrics, metricCount * 4 + (i - metricCount) * 2, bearing);
        }
        var maxp = new byte[32]; W32(maxp, 0, 0x00010000); W16(maxp, 4, 8); W16(maxp, 6, 4); W16(maxp, 8, 1);
        W16(maxp, 10, 7); W16(maxp, 12, 2); W16(maxp, 14, 1); W16(maxp, 28, 2); W16(maxp, 30, 2);
        var os2 = new byte[96]; W16(os2, 0, 4); W16(os2, 2, 500); W16(os2, 4, (style & FontStyle.Bold) != 0 ? 700 : 400); W16(os2, 6, 5);
        W32(os2, 42, 1); "LKIM"u8.CopyTo(os2.AsSpan(58));
        W16(os2, 62, 128 | (style == FontStyle.Regular ? 64 : (style & FontStyle.Bold) != 0 ? 32 : 0) | ((style & FontStyle.Italic) != 0 ? 1 : 0));
        W16(os2, 64, 32); W16(os2, 66, 0xFFFF); W16(os2, 68, 800); W16(os2, 70, -200); W16(os2, 72, 200); W16(os2, 74, 900); W16(os2, 76, 200);
        W16(os2, 86, 500); W16(os2, 88, 700); W16(os2, 94, 1);
        var post = new byte[32]; W32(post, 0, 0x00030000); W16(post, 8, -100); W16(post, 10, 50);
        var kern = new byte[24]; W16(kern, 2, 1); W16(kern, 6, 20); W16(kern, 8, 1); W16(kern, 10, 1); W16(kern, 12, 6);
        W16(kern, 18, 2); W16(kern, 20, 3); W16(kern, 22, -80);
        return Wrap(new SortedDictionary<string, byte[]>(StringComparer.Ordinal)
        {
            ["head"] = head, ["hhea"] = hhea, ["hmtx"] = metrics, ["maxp"] = maxp, ["loca"] = locations,
            ["glyf"] = outline.ToArray(), ["cmap"] = Mapping(unicode12, glyphArray), ["name"] = Names(style),
            ["OS/2"] = os2, ["post"] = post, ["kern"] = kern
        });
    }

    private static byte[] Simple((int X, int Y, bool On)[] points, FontStyle style)
    {
        points = points.Select(p => (p.X * ((style & FontStyle.Bold) != 0 ? 11 : 10) / 10 + ((style & FontStyle.Italic) != 0 ? p.Y / 5 : 0), p.Y, p.On)).ToArray();
        var result = new byte[14 + points.Length * 5]; W16(result, 0, 1);
        W16(result, 2, points.Min(p => p.X)); W16(result, 4, points.Min(p => p.Y)); W16(result, 6, points.Max(p => p.X)); W16(result, 8, points.Max(p => p.Y));
        W16(result, 10, points.Length - 1);
        int x = 0, y = 0;
        for (var i = 0; i < points.Length; i++)
        {
            result[14 + i] = points[i].On ? (byte)1 : (byte)0;
            W16(result, 14 + points.Length + i * 2, points[i].X - x); W16(result, 14 + points.Length * 3 + i * 2, points[i].Y - y);
            x = points[i].X; y = points[i].Y;
        }
        return result;
    }
    private static byte[] Composite(byte[] first, byte[] accent)
    {
        var result = new byte[26]; W16(result, 0, -1);
        W16(result, 2, Math.Min(I16(first, 2), I16(accent, 2))); W16(result, 4, Math.Min(I16(first, 4), I16(accent, 4)));
        W16(result, 6, Math.Max(I16(first, 6), I16(accent, 6))); W16(result, 8, Math.Max(I16(first, 8), I16(accent, 8)));
        W16(result, 10, 35); W16(result, 12, 2); W16(result, 18, 3); W16(result, 20, 6); return result;
    }
    private static byte[] Mapping(bool full, bool glyphArray)
    {
        (int Code, int Glyph)[] pairs = [(32, 1), (65, 2), (86, 3), (88, 0), (103, 4), (193, 5), (769, 6), (0xE000, 7), (0xFFFF, 0)];
        var n = pairs.Length; var four = new byte[16 + n * 8 + (glyphArray ? n * 2 : 0)];
        W16(four, 0, 4); W16(four, 2, four.Length); W16(four, 6, n * 2);
        var power = 1; var log = 0; while (power * 2 <= n) { power *= 2; log++; }
        W16(four, 8, power * 2); W16(four, 10, log); W16(four, 12, n * 2 - power * 2);
        for (var i = 0; i < n; i++)
        {
            var (code, glyph) = pairs[i]; W16(four, 14 + i * 2, code); W16(four, 16 + n * 2 + i * 2, code);
            if (glyphArray && code != 0xFFFF)
            {
                var delta = glyph != 1 ? 1 : 0; W16(four, 16 + n * 4 + i * 2, delta); W16(four, 16 + n * 6 + i * 2, n * 2);
                W16(four, 16 + n * 8 + i * 2, glyph == 0 ? 0 : glyph - delta);
            }
            else W16(four, 16 + n * 4 + i * 2, glyph - code);
        }
        var groups = pairs[..^1].Select(p => (First: p.Code, Last: p.Code, Glyph: p.Glyph))
            .Concat(new[] { (66, 67, 2), (0x1F642, 0x1F642, 7) }).OrderBy(g => g.Item1).ToArray();
        var twelve = new byte[16 + groups.Length * 12]; W16(twelve, 0, 12); W32(twelve, 4, (uint)twelve.Length); W32(twelve, 12, (uint)groups.Length);
        for (var i = 0; i < groups.Length; i++)
        { W32(twelve, 16 + i * 12, (uint)groups[i].Item1); W32(twelve, 20 + i * 12, (uint)groups[i].Item2); W32(twelve, 24 + i * 12, (uint)groups[i].Item3); }
        var header = full ? 20 : 12; var output = new byte[header + four.Length + (full ? twelve.Length : 0)];
        W16(output, 2, full ? 2 : 1); W16(output, 4, 0); W16(output, 6, 3); W32(output, 8, (uint)header); four.CopyTo(output, header);
        if (full) { W16(output, 12, 3); W16(output, 14, 10); W32(output, 16, (uint)(header + four.Length)); twelve.CopyTo(output, header + four.Length); }
        return output;
    }
    private static byte[] Names(FontStyle style)
    {
        var face = style == FontStyle.Regular ? "Regular" : style == (FontStyle.Bold | FontStyle.Italic) ? "Bold Italic" : style.ToString();
        (int Id, string Text)[] values = [(1, "Lokad Image Authored"), (2, face), (4, "Lokad Image Authored " + face), (6, "LokadImageAuthored-" + face.Replace(" ", ""))];
        var strings = values.Select(v => Encoding.BigEndianUnicode.GetBytes(v.Text)).ToArray(); var start = 6 + values.Length * 12;
        var result = new byte[start + strings.Sum(s => s.Length)]; W16(result, 2, values.Length); W16(result, 4, start); var offset = 0;
        for (var i = 0; i < values.Length; i++)
        {
            var p = 6 + i * 12; W16(result, p, 3); W16(result, p + 2, 1); W16(result, p + 4, 0x409);
            W16(result, p + 6, values[i].Id); W16(result, p + 8, strings[i].Length); W16(result, p + 10, offset);
            strings[i].CopyTo(result, start + offset); offset += strings[i].Length;
        }
        return result;
    }
    private static byte[] Wrap(SortedDictionary<string, byte[]> tables)
    {
        var start = 12 + tables.Count * 16; var size = start + tables.Values.Sum(t => (t.Length + 3) & ~3); var output = new byte[size];
        W32(output, 0, 0x00010000); W16(output, 4, tables.Count); var power = 1; var log = 0;
        while (power * 2 <= tables.Count) { power *= 2; log++; }
        W16(output, 6, power * 16); W16(output, 8, log); W16(output, 10, tables.Count * 16 - power * 16);
        var i = 0; var offset = start; var head = 0;
        foreach (var (tag, bytes) in tables)
        {
            var p = 12 + i++ * 16; Encoding.ASCII.GetBytes(tag).CopyTo(output, p); W32(output, p + 4, Checksum(bytes));
            W32(output, p + 8, (uint)offset); W32(output, p + 12, (uint)bytes.Length); bytes.CopyTo(output, offset);
            if (tag == "head") head = offset; offset += (bytes.Length + 3) & ~3;
        }
        W32(output, head + 8, unchecked(0xB1B0AFBAu - Checksum(output))); return output;
    }
    internal static int Offset(byte[] font, string tag)
    {
        var count = BinaryPrimitives.ReadUInt16BigEndian(font.AsSpan(4));
        for (var i = 0; i < count; i++) if (font.AsSpan(12 + i * 16, 4).SequenceEqual(Encoding.ASCII.GetBytes(tag)))
            return (int)BinaryPrimitives.ReadUInt32BigEndian(font.AsSpan(20 + i * 16));
        throw new ArgumentException("Missing authored table.", nameof(tag));
    }
    internal static uint Checksum(byte[] bytes)
    {
        uint sum = 0;
        for (var i = 0; i < bytes.Length; i += 4)
        {
            uint word = 0; for (var j = 0; j < 4; j++) word = word << 8 | (i + j < bytes.Length ? bytes[i + j] : 0u);
            sum = unchecked(sum + word);
        }
        return sum;
    }
    internal static void W16(byte[] bytes, int offset, int value) => BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(offset), unchecked((ushort)value));
    internal static void W32(byte[] bytes, int offset, uint value) => BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(offset), value);
    private static short I16(byte[] bytes, int offset) => BinaryPrimitives.ReadInt16BigEndian(bytes.AsSpan(offset));
}
