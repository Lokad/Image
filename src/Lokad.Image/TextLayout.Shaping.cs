using Lokad.Image.Internal;

namespace Lokad.Image;

public sealed partial class TextLayout
{
    private sealed partial class Builder
    {
        private ShapingBuffer? shaped;
        internal void ReadShaped(string text) => shaped = TextShaper.Read(font, text, itemCount, options, context,
            glyph => { var outline = Get(glyph).Outline; return (outline.Advance, outline.Left.X); });

        internal void ArrangeShaped()
        {
            var buffer = shaped!; if (buffer.Count == 0) return;
            // One reused line workspace; capacity stays charged through wrapping.
            var bytes = buffer.Count * 5L + 64; context.Reserve(bytes);
            try
            {
                var order = new int[buffer.Count]; var levels = new byte[buffer.Count];
                using var positioner = new TextShaper.Positioner(font, options.Kerning, context,
                    glyph => { var outline = Get(glyph).Outline; return (outline.Advance, outline.Left.X); });
                var start = 0; var at = 0; var pending = -1; double width = 0; var hasWord = false;
                while (at < buffer.Count)
                {
                    context.Work(1); var glyph = buffer[at];
                    if (glyph.Kind == 3)
                    {
                        do { start = EmitShapedLine(start, at, order, levels, positioner); } while (start < at);
                        start = ++at; width = 0; pending = -1; hasWord = false; continue;
                    }
                    if (glyph.Kind is 1 or 2)
                    {
                        if (pending < 0) pending = at;
                        width += Width(glyph, width); at++; continue;
                    }
                    var end = at + 1;
                    while (end < buffer.Count && buffer[end].Kind is not (1 or 2 or 3)) { context.Work(1); end++; }
                    double word = 0;
                    for (var j = at; j < end; j++) { context.Work(1); word += buffer[j].Advance * scale; }
                    if (hasWord && width + word > options.WrapWidth)
                    {
                        var cut = pending >= 0 ? pending : at;
                        if (cut > start)
                        {
                            var actual = EmitShapedLine(start, cut, order, levels, positioner);
                            start = actual < cut ? actual : at; width = RangeWidth(start, at); hasWord = HasWord(start, at);
                        }
                    }
                    pending = -1;
                    while (at < end)
                    {
                        var clusterEnd = at + 1;
                        while (clusterEnd < end && buffer[clusterEnd].Cluster == buffer[at].Cluster) { context.Work(1); clusterEnd++; }
                        double advance = 0;
                        for (var j = at; j < clusterEnd; j++) { context.Work(1); advance += buffer[j].Advance * scale; }
                        while (hasWord && width + advance > options.WrapWidth)
                        {
                            start = EmitShapedLine(start, at, order, levels, positioner);
                            width = RangeWidth(start, at); hasWord = HasWord(start, at);
                        }
                        hasWord |= HasWord(at, clusterEnd);
                        width += advance; at = clusterEnd;
                    }
                }
                do { start = EmitShapedLine(start, buffer.Count, order, levels, positioner); } while (start < buffer.Count);
            }
            finally { context.Release(bytes); }
        }
        private bool HasWord(int start, int end)
        {
            for (var i = start; i < end; i++) { context.Work(1); if (shaped![i].Glyph >= 0 && shaped[i].Kind == 0) return true; }
            return false;
        }
        private double RangeWidth(int start, int end)
        {
            double width = 0;
            for (var i = start; i < end; i++) { context.Work(1); width += Width(shaped![i], width); }
            return width;
        }
        private double Width(ShapingGlyph glyph, double current)
        {
            if (glyph.Kind != 2) return glyph.Advance * scale;
            var space = Get(font.MapForText(' ', context)).Outline.Advance * scale;
            var stop = (space > 0 ? space : options.FontSize / 3) * options.TabSpaces;
            return (Math.Floor(current / stop) + 1) * stop - current;
        }
        private int EmitShapedLine(int start, int end, Span<int> order, Span<byte> levels, TextShaper.Positioner positioner)
        {
            var buffer = shaped!;
            var minimum = start;
            while (minimum < end && !(buffer[minimum].Glyph >= 0 && buffer[minimum].Kind == 0)) { context.Work(1); minimum++; }
            if (minimum < end)
            {
                var cluster = buffer[minimum++].Cluster;
                while (minimum < end && buffer[minimum].Cluster == cluster) { context.Work(1); minimum++; }
            }
            while (true)
            {
                positioner.Apply(buffer, start, end);
                if (RangeWidth(start, end) <= options.WrapWidth || end <= minimum) break;
                var cluster = buffer[--end].Cluster;
                while (end > minimum && buffer[end - 1].Cluster == cluster) { context.Work(1); end--; }
            }
            var count = end - start;
            var baseLevel = start < buffer.Count && (buffer[start].Features & TextShaper.ParagraphRight) != 0 ? (byte)1 : (byte)0;
            var trailing = 0; double logicalPen = 0;
            for (var i = 0; i < count; i++)
            {
                context.Work(1); var glyph = buffer[start + i]; order[i] = start + i; levels[i] = glyph.Level;
                if (glyph.Kind == 2) buffer[start + i].Advance = Width(glyph, logicalPen) / scale;
                logicalPen += buffer[start + i].Advance * scale;
                var type = UnicodeTextProperties.Bidi(glyph.Scalar);
                if (type is BidiClass.B or BidiClass.S)
                { for (var j = trailing; j <= i; j++) { context.Work(1); levels[j] = baseLevel; } trailing = i + 1; }
                else if (type is not (BidiClass.WS or BidiClass.LRI or BidiClass.RLI or BidiClass.FSI or BidiClass.PDI or BidiClass.BN)) trailing = i + 1;
            }
            for (var i = trailing; i < count; i++) { context.Work(1); levels[i] = baseLevel; }
            var highest = 0; var lowestOdd = 127;
            for (var i = 0; i < count; i++) { highest = Math.Max(highest, levels[i]); if ((levels[i] & 1) != 0) lowestOdd = Math.Min(lowestOdd, levels[i]); }
            for (var level = highest; level >= lowestOdd; level--)
                for (var i = 0; i < count; i++)
                {
                    context.Work(1); if (levels[i] < level) continue;
                    var first = i; while (i + 1 < count && levels[i + 1] >= level) { context.Work(1); i++; }
                    order.Slice(first, i - first + 1).Reverse(); levels.Slice(first, i - first + 1).Reverse();
                }
            pen = 0;
            for (var i = 0; i < count; i++)
            {
                context.Work(1); var glyph = buffer[order[i]];
                if (glyph.Glyph >= 0)
                    Glyphs[Emitted++] = new() { Shape = Get(glyph.Glyph), GlyphIndex = glyph.Glyph, SourceOffset = glyph.SourceOffset,
                        SourceLength = glyph.SourceLength, LineIndex = LineCount, X = pen + glyph.OffsetX * scale,
                        Y = Baseline - glyph.OffsetY * scale, Advance = glyph.Advance * scale, Kind = glyph.Kind };
                pen += glyph.Advance * scale;
            }
            lineMax = pen; FinishLine();
            Lines[LineCount - 1] = Lines[LineCount - 1] with { Direction = baseLevel == 1 ? TextDirection.RightToLeft : TextDirection.LeftToRight };
            return end;
        }
        internal void AlignLines()
        {
            if (options.Shaping == TextShaping.Legacy && options.Alignment == TextAlignment.Start) return;
            var target = double.IsFinite(options.WrapWidth) ? options.WrapWidth : allMax;
            allMin = allMax = 0;
            for (var i = 0; i < LineCount; i++)
            {
                context.Work(1); var line = Lines[i]; var right = line.Direction == TextDirection.RightToLeft;
                var shift = options.Alignment switch { TextAlignment.Center => (target - line.AdvanceWidth) / 2,
                    TextAlignment.End => right ? 0 : target - line.AdvanceWidth, _ => right ? target - line.AdvanceWidth : 0 };
                for (var j = line.FirstGlyph; j < line.FirstGlyph + line.GlyphCount; j++) { context.Work(1); Glyphs[j].X += shift; }
                allMin = Math.Min(allMin, shift); allMax = Math.Max(allMax, shift + line.AdvanceWidth);
            }
        }
    }
}
