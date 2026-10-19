using System.Buffers.Binary;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Lokad.Image;
using Lokad.Image.Tooling;

// Correctness-only access to Lokad.Image's staged internal primitive. Reference
// observations use public APIs; production contains no reflection-based shaper.
internal static class SubstitutionProbe
{
    private const BindingFlags Instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    internal static void Run(string root)
    {
        var path = Path.Combine(root, "external/text-reference/fonts/DejaVuSans.ttf");
        var expected = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "tools/TextReference/font-provenance.json")))
            .RootElement.GetProperty("files").EnumerateArray().Single(f => f.GetProperty("file").GetString() == "DejaVuSans.ttf");
        using var budget = new TestMemoryBudget(8 * 1024 * 1024);
        if (!budget.TryReserve(expected.GetProperty("bytes").GetInt64(), out var inputOwner)) throw new Exception("Probe input admission refused.");
        using (inputOwner)
        {
            var input = File.ReadAllBytes(path);
            if (Convert.ToHexStringLower(SHA256.HashData(input)) != expected.GetProperty("sha256").GetString()) throw new Exception("Probe font checksum mismatch.");
            var limits = new ImageLimits { MemoryBudget = budget };
            var observations = new List<object>();
            using (var font = TrueTypeFont.Load(input, limits))
            {
                var fontData = (byte[])typeof(TrueTypeFont).GetField("data", Instance)!.GetValue(font)!;
                var baseline = budget.Bytes;
                foreach (var (name, text, script) in new[]
                {
                    ("CombiningMarks", "A\u0301 g\u0301", "latn"),
                    ("ArabicCompositionOnly", "\u0627\u0644\u0633\u064E\u0651\u0644\u064E\u0627\u0645\u064F 123 \u0639\u0644\u064A\u0643\u0645", "arab")
                })
                {
                    var runes = text.EnumerateRunes().ToArray();
                    var context = New("OperationContext", limits, CancellationToken.None);
                    using ((IDisposable)context)
                    {
                        var buffer = New("ShapingBuffer", runes.Length, context);
                        using ((IDisposable)buffer)
                        {
                            var glyphType = Type("ShapingGlyph"); var offset = 0;
                            foreach (var rune in runes)
                            {
                                var glyph = Activator.CreateInstance(glyphType)!;
                                Set(glyph, "Glyph", font.GetGlyphIndex(rune, limits)); Set(glyph, "Scalar", rune.Value);
                                Set(glyph, "SourceOffset", offset); Set(glyph, "SourceLength", rune.Utf16SequenceLength);
                                Set(glyph, "Cluster", offset); Set(glyph, "Features", uint.MaxValue);
                                Call(buffer, "Add", glyph); offset += rune.Utf16SequenceLength;
                            }
                            var before = Values(buffer);
                            using (var engine = (IDisposable)New("OpenTypeSubstitution", Table(fontData, "GSUB"), Table(fontData, "GDEF"), font.GlyphCount, context))
                            {
                                Call(engine, "ApplyFeature", Tag(script), 0u, Tag("ccmp"), buffer, 0u, 0, -1, false);
                                observations.Add(new { Name = name, Before = before, After = Values(buffer),
                                    CompleteShaping = false, Remaining = "GPOS attachment, joining forms, ligatures and bidirectional ordering" });
                            }
                        }
                    }
                    if (budget.Bytes != baseline) throw new Exception("Substitution temporary storage leaked admission.");
                }
            }
            if (budget.Bytes != inputOwner.Bytes) throw new Exception("Font admission was not released.");
            Console.WriteLine(JsonSerializer.Serialize(new
            {
                ProductionSha256 = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(typeof(ImageBuffer).Assembly.Location))),
                FontSha256 = expected.GetProperty("sha256").GetString(), NoTiming = true, NoPublicLayoutIntegration = true,
                TemporaryOwnersReleased = true, RetainedFontReleased = true, PeakAdmittedBytes = budget.Peak, Cases = observations
            }));
        }
        if (budget.Bytes != 0 || budget.IsDisposed) throw new Exception("Caller-owned probe allowance leaked or was disposed by the library.");
    }
    private static uint Tag(string tag) => BinaryPrimitives.ReadUInt32BigEndian(Encoding.ASCII.GetBytes(tag));
    private static Type Type(string name) => typeof(ImageBuffer).Assembly.GetType("Lokad.Image.Internal." + name, true)!;
    private static object New(string name, params object[] args) => Activator.CreateInstance(Type(name), Instance, null, args, CultureInfo.InvariantCulture)!;
    private static object? Call(object target, string name, params object[] args) => target.GetType().GetMethod(name, Instance)!.Invoke(target, args);
    private static void Set(object glyph, string name, object value) => glyph.GetType().GetProperty(name, Instance)!.SetValue(glyph, value);
    private static int[] Values(object buffer)
    {
        var values = (Array)buffer.GetType().GetField("glyphs", Instance)!.GetValue(buffer)!;
        var count = (int)buffer.GetType().GetProperty("Count", Instance)!.GetValue(buffer)!;
        return Enumerable.Range(0, count).Select(i => (int)values.GetValue(i)!.GetType().GetProperty("Glyph", Instance)!.GetValue(values.GetValue(i))!).ToArray();
    }
    private static ReadOnlyMemory<byte> Table(byte[] data, string tag)
    {
        var count = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(4)); var wanted = Encoding.ASCII.GetBytes(tag);
        for (var i = 0; i < count; i++)
        {
            var p = 12 + i * 16;
            if (!data.AsSpan(p, 4).SequenceEqual(wanted)) continue;
            return data.AsMemory((int)BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(p + 8)), (int)BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(p + 12)));
        }
        return ReadOnlyMemory<byte>.Empty;
    }
}
