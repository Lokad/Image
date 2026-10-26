using System.Globalization;
using System.Text;

namespace Lokad.Image.Internal;

internal static class TextShaper
{
    internal const uint ParagraphRight = 1U << 31;
    private const uint MirrorFeature = 1U << 16;
    private const int ScriptShift = 8;
    private static uint Tag(string tag) => OpenTypeTable.Tag(tag[0], tag[1], tag[2], tag[3]);
    private static bool Control(int scalar) => scalar is 0x061C or 0x200E or 0x200F or >= 0x202A and <= 0x202E or >= 0x2066 and <= 0x2069;

    internal static ShapingBuffer Read(TrueTypeFont font, string text, int count, TextOptions options,
        OperationContext context, Func<int, (double Advance, double Origin)> metrics)
    {
        var glyphs = new ShapingBuffer(count, context);
        var bytes = count * 4L + 32; var reserved = false;
        try
        {
            context.Reserve(bytes); reserved = true; var scalars = new int[count];
            var cluster = 0; var clusterEnd = 0;
            for (var offset = 0; offset < text.Length;)
            {
                context.Cancellation.ThrowIfCancellationRequested(); Rune.DecodeFromUtf16(text.AsSpan(offset), out var rune, out var length);
                if (offset == clusterEnd)
                {
                    cluster = offset; var n = StringInfo.GetNextTextElementLength(text.AsSpan(offset));
                    context.Cancellation.ThrowIfCancellationRequested(); clusterEnd += n;
                }
                if (rune.Value == '\r' && offset + length < text.Length && text[offset + length] == '\n') length++;
                var scalar = rune.Value; var bidi = UnicodeTextProperties.Bidi(scalar);
                byte kind = bidi == BidiClass.B || scalar == 0x2028 ? (byte)3 : scalar == '\t' ? (byte)2
                    : BidiParagraph.Removed(bidi) ? (byte)5 : Control(scalar) ? (byte)4
                    : Rune.IsWhiteSpace(rune) && scalar is not (0xA0 or 0x202F) ? (byte)1 : (byte)0;
                var glyph = kind is 2 or 3 or 4 || kind == 5 && scalar != 0x200D ? -1 : font.MapForText(scalar, context);
                scalars[glyphs.Count] = scalar;
                glyphs.Add(new() { Glyph = glyph, Scalar = scalar, SourceOffset = offset, SourceLength = length, Cluster = cluster, Kind = kind });
                offset += length;
            }
            for (var start = 0; start < glyphs.Count;)
            {
                var end = start;
                do { end++; } while (end < glyphs.Count && UnicodeTextProperties.Bidi(scalars[end - 1]) != BidiClass.B);
                var direction = options.Direction == TextDirection.Auto ? -1 : options.Direction == TextDirection.RightToLeft ? 1 : 0;
                using var paragraph = new BidiParagraph(scalars.AsSpan(start, end - start), context, direction);
                byte nextLevel = paragraph.BaseLevel;
                for (var i = end - 1; i >= start; i--)
                {
                    context.Cancellation.ThrowIfCancellationRequested(); if (!paragraph.IsRemoved(i - start)) nextLevel = paragraph.Level(i - start);
                    glyphs[i].Level = nextLevel;
                    if (paragraph.BaseLevel == 1) glyphs[i].Features |= ParagraphRight;
                }
                var previousLevel = -1;
                for (var i = start; i < end; i++)
                {
                    context.Cancellation.ThrowIfCancellationRequested();
                    if (paragraph.IsRemoved(i - start)) { if (previousLevel >= 0) glyphs[i].Level = (byte)previousLevel; }
                    else previousLevel = glyphs[i].Level;
                    if (glyphs[i].Glyph >= 0 && (glyphs[i].Level & 1) != 0 && UnicodeTextProperties.Mirrored(glyphs[i].Scalar))
                    {
                        var mirrored = UnicodeTextProperties.Mirror(glyphs[i].Scalar);
                        var mapped = font.MapForText(mirrored, context);
                        if (mirrored != glyphs[i].Scalar && mapped != 0) glyphs[i].Glyph = mapped;
                        else if (glyphs[i].Glyph != 0) glyphs[i].Features |= MirrorFeature;
                    }
                }
                AssignScripts(glyphs, start, end); start = end;
            }
            ArabicJoining.Apply(glyphs);
            // ZWJ has already selected neighbouring joining forms. It must not
            // prevent ligature matching. ZWNJ remains a boundary until GSUB ends.
            for (var i = 0; i < glyphs.Count; i++)
                if (glyphs[i].Kind == 5 && glyphs[i].Scalar == 0x200C) glyphs[i].Kind = 6;
            glyphs.RemoveKind(5);
            using var gsub = new OpenTypeSubstitution(font.TableForText("GSUB"u8, context), font.TableForText("GDEF"u8, context), font.GlyphCount, context);
            using (var positioner = new Positioner(font, options.Kerning, context, metrics))
            {
                ReadOnlySpan<uint> features = [Tag("ccmp"), Tag("locl"), Tag("isol"), Tag("fina"), Tag("medi"), Tag("init"),
                    Tag("rlig"), Tag("rclt"), Tag("calt"), Tag("liga"), Tag("mset")];
                for (var start = 0; start < glyphs.Count;)
                {
                    context.Cancellation.ThrowIfCancellationRequested();
                    if (glyphs[start].Glyph < 0) { start++; continue; }
                    var script = (TextScript)((glyphs[start].Features >> ScriptShift) & 7); var level = glyphs[start].Level;
                    var end = start + 1;
                    while (end < glyphs.Count && glyphs[end].Glyph >= 0 && glyphs[end].Level == level
                        && ((glyphs[end].Features >> ScriptShift) & 7) == (uint)script) { context.Cancellation.ThrowIfCancellationRequested(); end++; }
                    var tag = script switch { TextScript.Arabic => Tag("arab"), TextScript.Hebrew => Tag("hebr"), _ => Tag("latn") };
                    var required = gsub.RequiredTag(tag, 0);
                    if (required != 0 && required != Tag("rtlm") && !features.Contains(required))
                        throw OpenTypeTable.Unsupported("The required OpenType substitution feature needs an unsupported shaping stage.");
                    var oldCount = glyphs.Count;
                    gsub.ApplyFeature(tag, 0, Tag("rtlm"), glyphs, MirrorFeature, start, end - start, required == Tag("rtlm"));
                    end += glyphs.Count - oldCount;
                    for (var i = start; i < end; i++)
                    {
                        context.Cancellation.ThrowIfCancellationRequested();
                        if ((glyphs[i].Features & MirrorFeature) == 0) continue;
                        if (glyphs[i].Glyph == font.MapForText(glyphs[i].Scalar, context))
                            throw OpenTypeTable.Unsupported("This mirrored glyph needs a mapped partner or a supported rtlm substitution.");
                        glyphs[i].Features &= ~MirrorFeature;
                    }
                    foreach (var feature in features)
                    {
                        var mask = feature == Tag("isol") ? ArabicJoining.Isolated : feature == Tag("fina") ? ArabicJoining.Final
                            : feature == Tag("medi") ? ArabicJoining.Medial : feature == Tag("init") ? ArabicJoining.Initial : 0U;
                        if (mask != 0 && script != TextScript.Arabic) continue;
                        var before = glyphs.Count;
                        var found = gsub.ApplyFeature(tag, 0, feature, glyphs, mask, start, end - start, required == feature);
                        if (!found && mask != 0 && mask != ArabicJoining.Isolated)
                            for (var i = start; i < end; i++)
                            {
                                context.Cancellation.ThrowIfCancellationRequested();
                                if ((glyphs[i].Features & mask) != 0 && glyphs[i].Glyph != 0)
                                    throw OpenTypeTable.Unsupported("The supplied font lacks a required Arabic joining-form feature.");
                            }
                        end += glyphs.Count - before;
                    }
                    start = end;
                }
                positioner.Apply(glyphs, 0, glyphs.Count);
            }
            return glyphs;
        }
        catch { glyphs.Dispose(); throw; }
        finally { if (reserved) context.Release(bytes); }
    }

    // Reused during line layout. Positioning is scoped to each final line, so a
    // pair outside a line cannot alter its first/last glyph or reported width.
    internal sealed class Positioner : IDisposable
    {
        private readonly TrueTypeFont font;
        private readonly bool kerning;
        private readonly OperationContext context;
        private readonly Func<int, (double Advance, double Origin)> metrics;
        private readonly OpenTypePositioning gpos;
        private readonly OpenTypeGlyphClasses classes;
        private TrueTypeFont.TextKerning? fallback;
        internal Positioner(TrueTypeFont font, bool kerning, OperationContext context, Func<int, (double Advance, double Origin)> metrics)
        {
            this.font = font; this.kerning = kerning; this.context = context; this.metrics = metrics;
            classes = new(font.TableForText("GDEF"u8, context), font.GlyphCount, context);
            gpos = new(font.TableForText("GPOS"u8, context), font.TableForText("GDEF"u8, context), font.GlyphCount, context);
        }
        internal void Apply(ShapingBuffer glyphs, int start, int end)
        {
            for (var at = start; at < end;)
            {
                context.Cancellation.ThrowIfCancellationRequested(); if (glyphs[at].Glyph < 0) { at++; continue; }
                var script = (TextScript)((glyphs[at].Features >> ScriptShift) & 7); var level = glyphs[at].Level;
                var finish = at + 1;
                while (finish < end && glyphs[finish].Glyph >= 0 && glyphs[finish].Level == level
                    && ((glyphs[finish].Features >> ScriptShift) & 7) == (uint)script) { context.Cancellation.ThrowIfCancellationRequested(); finish++; }
                var tag = script switch { TextScript.Arabic => Tag("arab"), TextScript.Hebrew => Tag("hebr"), _ => Tag("latn") };
                var hasKern = kerning && gpos.HasFeature(tag, 0, Tag("kern"));
                for (var i = at; i < finish; i++)
                {
                    context.Cancellation.ThrowIfCancellationRequested(); var m = metrics(glyphs[i].Glyph);
                    glyphs[i].OriginX = m.Origin; glyphs[i].Advance = classes.Class(glyphs[i]) == 3 ? 0 : m.Advance;
                    glyphs[i].OffsetX = glyphs[i].OffsetY = glyphs[i].AttachX = glyphs[i].AttachY = 0; glyphs[i].AttachTo = 0;
                }
                if (kerning && !hasKern)
                {
                    fallback ??= new(font, context);
                    for (var i = at; i + 1 < finish; i++)
                        if (classes.Class(glyphs[i]) != 3 && classes.Class(glyphs[i + 1]) != 3)
                            glyphs[i].Advance += fallback.Get(glyphs[i].Glyph, glyphs[i + 1].Glyph);
                }
                gpos.ApplyStandardFeatures(tag, 0, glyphs, kerning, at, finish - at);
                gpos.ResolveAttachments(glyphs, (level & 1) != 0, at, finish - at); at = finish;
            }
        }
        public void Dispose() { fallback?.Dispose(); gpos.Dispose(); }
    }

    private static void AssignScripts(ShapingBuffer glyphs, int start, int end)
    {
        var context = glyphs.Context; var next = TextScript.Latin; var previous = TextScript.Common; var previousLevel = -1;
        for (var i = end - 1; i >= start; i--)
        {
            context.Cancellation.ThrowIfCancellationRequested(); var script = UnicodeTextProperties.Script(glyphs[i].Scalar);
            if (script == TextScript.Other && Rune.GetUnicodeCategory(new Rune(glyphs[i].Scalar)) != UnicodeCategory.OtherNotAssigned)
                throw OpenTypeTable.Unsupported("OpenType text currently supports Latin, Hebrew and Arabic scripts.");
            if (script is TextScript.Latin or TextScript.Hebrew or TextScript.Arabic) next = script;
            glyphs[i].Features |= (uint)next << ScriptShift;
        }
        for (var i = start; i < end; i++)
        {
            context.Cancellation.ThrowIfCancellationRequested(); var script = UnicodeTextProperties.Script(glyphs[i].Scalar);
            if (script is TextScript.Common or TextScript.Inherited && previousLevel == glyphs[i].Level)
                glyphs[i].Features = (glyphs[i].Features & ~(7U << ScriptShift)) | (uint)previous << ScriptShift;
            previous = (TextScript)((glyphs[i].Features >> ScriptShift) & 7); previousLevel = glyphs[i].Level;
        }
    }
}
