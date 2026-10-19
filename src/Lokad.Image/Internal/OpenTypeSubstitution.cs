namespace Lokad.Image.Internal;

// Independent GSUB interpreter from the OpenType GSUB/chapter2 specifications.
// The selected script/language/feature defines the program; font bytes stay
// borrowed. This primitive is integrated only by the full shaping pipeline.
internal sealed class OpenTypeSubstitution : IDisposable
{
    private const int MaximumNesting = 16;
    private readonly OpenTypeTable table;
    private readonly OpenTypeGlyphClasses classes;
    private readonly OperationContext context;
    private readonly int lookupList;
    private byte[] selected;
    private bool disposed;
    private int runStart, runEnd;

    internal OpenTypeSubstitution(ReadOnlyMemory<byte> substitution, ReadOnlyMemory<byte> definitions,
        int glyphCount, OperationContext context)
    {
        this.context = context; table = new(substitution, glyphCount, context);
        classes = new(substitution.IsEmpty ? ReadOnlyMemory<byte>.Empty : definitions, glyphCount, context);
        if (table.Empty) { selected = []; return; }
        lookupList = table.LookupList(); var count = table.Count(lookupList, 2);
        context.Reserve(count + 32L);
        try { selected = new byte[count]; }
        catch { context.Release(count + 32L); throw; }
    }

    internal bool ApplyFeature(uint script, uint language, uint feature, ShapingBuffer glyphs,
        uint mask = 0, int start = 0, int length = -1, bool required = false)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (!ReferenceEquals(glyphs.Context, context)) throw new ArgumentException("Shaping storage must share the operation context.", nameof(glyphs));
        if (start < 0 || start > glyphs.Count || length < -1 || length > glyphs.Count - start)
            throw new ArgumentOutOfRangeException(nameof(length));
        if (table.Empty) return false;
        if (!table.Select(script, language, feature, selected, required)) return false;
        runStart = start; runEnd = length < 0 ? glyphs.Count : start + length;
        for (var lookup = 0; lookup < selected.Length; lookup++)
        {
            if (selected[lookup] == 0) continue;
            var at = start;
            while (at < runEnd)
            {
                context.Work(1);
                if (mask != 0 && (glyphs[at].Features & mask) == 0) { at++; continue; }
                ApplyAt(lookup, glyphs, at, runEnd, 0, out var next);
                at = Math.Max(at + 1, next);
            }
        }
        return true;
    }
    internal uint RequiredTag(uint script, uint language)
    {
        ObjectDisposedException.ThrowIf(disposed, this); return table.RequiredTag(script, language);
    }

    private bool ApplyAt(int lookup, ShapingBuffer glyphs, int at, int inputEnd, int depth, out int next)
    {
        next = at + 1;
        if (depth >= MaximumNesting) throw OpenTypeTable.Unsupported("OpenType substitution nesting beyond 16 lookups is unsupported.");
        if ((uint)lookup >= selected.Length) throw OpenTypeTable.Bad("Invalid nested OpenType lookup index.");
        if (at < runStart || at >= inputEnd || Boundary(glyphs[at])) return false;
        var p = table.Offset16(lookupList, lookupList + 2 + lookup * 2);
        var type = table.U16(p); var flags = table.U16(p + 2); var count = table.Count(p + 4, 2);
        if ((flags & 0xE0) != 0) throw OpenTypeTable.Unsupported("Reserved OpenType lookup flags are unsupported.");
        var filteringSet = (flags & 0x10) != 0 ? table.U16(p + 6 + count * 2) : 0;
        if (classes.Ignored(glyphs[at], flags, filteringSet)) return false;
        for (var i = 0; i < count; i++)
        {
            var sub = table.Offset16(p, p + 6 + i * 2);
            if (ApplySubtable(type, sub, glyphs, at, inputEnd, flags, filteringSet, depth, out next)) return true;
        }
        next = at + 1; return false;
    }

    private bool ApplySubtable(int type, int sub, ShapingBuffer glyphs, int at, int inputEnd,
        int flags, int filteringSet, int depth, out int next)
    {
        next = at + 1;
        switch (type)
        {
            case 1: return Single(sub, glyphs, at);
            case 4: return Ligature(sub, glyphs, at, inputEnd, flags, filteringSet);
            case 6: return Context(sub, glyphs, at, inputEnd, flags, filteringSet, depth, out next);
            case 7:
                if (table.U16(sub) != 1) throw OpenTypeTable.Unsupported("Unsupported OpenType substitution extension format.");
                var extensionType = table.U16(sub + 2);
                if (extensionType == 7) throw OpenTypeTable.Bad("Recursive OpenType substitution extension.");
                return ApplySubtable(extensionType, table.Offset32(sub, sub + 4), glyphs, at,
                    inputEnd, flags, filteringSet, depth, out next);
            default: throw OpenTypeTable.Unsupported("Unsupported OpenType substitution lookup type.");
        }
    }

    private bool Single(int sub, ShapingBuffer glyphs, int at)
    {
        var format = table.U16(sub);
        if (format is not (1 or 2)) throw OpenTypeTable.Unsupported("Unsupported OpenType single substitution format.");
        var covered = table.Coverage(table.Offset16(sub, sub + 2), glyphs[at].Glyph);
        if (covered < 0) return false;
        int value;
        if (format == 1) value = (glyphs[at].Glyph + table.I16(sub + 4)) & 0xFFFF;
        else
        {
            var count = table.Count(sub + 4, 2);
            if (covered >= count) throw OpenTypeTable.Bad("OpenType single substitution coverage exceeds its output array.");
            value = table.U16(sub + 6 + covered * 2);
        }
        glyphs[at].Glyph = table.Glyph(value); return true;
    }

    private bool Ligature(int sub, ShapingBuffer glyphs, int at, int inputEnd, int flags, int filteringSet)
    {
        if (table.U16(sub) != 1) throw OpenTypeTable.Unsupported("Unsupported OpenType ligature substitution format.");
        var covered = table.Coverage(table.Offset16(sub, sub + 2), glyphs[at].Glyph);
        if (covered < 0) return false;
        var sets = table.Count(sub + 4, 2);
        if (covered >= sets) throw OpenTypeTable.Bad("OpenType ligature coverage exceeds its set array.");
        var set = table.Offset16(sub, sub + 6 + covered * 2); var count = table.Count(set, 2);
        Span<int> scratch = stackalloc int[64];
        for (var candidate = 0; candidate < count; candidate++)
        {
            var lig = table.Offset16(set, set + 2 + candidate * 2); var replacement = table.Glyph(table.U16(lig));
            var components = table.U16(lig + 2);
            if (components == 0) throw OpenTypeTable.Bad("An OpenType ligature has no components.");
            table.Items(components); table.Range(lig + 4, (components - 1L) * 2);
            if (components > inputEnd - at) continue;
            int[]? heap = null;
            var bytes = components > scratch.Length ? components * 4L + 32 : 0;
            if (bytes != 0)
            {
                context.Reserve(bytes);
                try { heap = new int[components]; }
                catch { context.Release(bytes); throw; }
            }
            try
            {
                var matched = heap is null ? scratch[..components] : heap.AsSpan(); matched[0] = at;
                var cursor = at; var success = true;
                for (var j = 1; j < components; j++)
                {
                    cursor = Next(glyphs, cursor + 1, 1, inputEnd, flags, filteringSet);
                    if (cursor < 0 || glyphs[cursor].Glyph != table.Glyph(table.U16(lig + 2 + j * 2))) { success = false; break; }
                    matched[j] = cursor;
                }
                if (!success) continue;
                Merge(glyphs, matched, replacement); return true;
            }
            finally { if (bytes != 0) context.Release(bytes); }
        }
        return false;
    }

    private void Merge(ShapingBuffer glyphs, ReadOnlySpan<int> matched, int replacement)
    {
        var first = glyphs[matched[0]]; var sourceStart = first.SourceOffset;
        var sourceEnd = checked(first.SourceOffset + first.SourceLength); var cluster = first.Cluster;
        var components = 0; var id = checked(++glyphs.NextLigature);
        for (var i = 0; i < matched.Length; i++)
        {
            context.Work(1); var component = glyphs[matched[i]];
            sourceStart = Math.Min(sourceStart, component.SourceOffset);
            sourceEnd = Math.Max(sourceEnd, checked(component.SourceOffset + component.SourceLength));
            cluster = Math.Min(cluster, component.Cluster);
            var componentCount = Math.Max(1, component.ComponentCount);
            if (componentCount > context.Limits.MaxContainerItems - components)
                throw new ImageLimitException(ImageLimitKind.ContainerItems, "OpenType ligature component limit exceeded.");
            if (i + 1 < matched.Length)
            {
                var attached = true;
                for (var mark = matched[i] + 1; mark < matched[i + 1]; mark++)
                {
                    context.Work(1);
                    if (classes.Class(glyphs[mark]) != 3) { attached = false; continue; }
                    if (!attached) continue;
                    var part = component.ComponentCount > 1 && glyphs[mark].Ligature == component.Ligature
                        ? glyphs[mark].Component : componentCount - 1;
                    glyphs[mark].Ligature = id; glyphs[mark].Component = components + part;
                }
            }
            components += componentCount;
        }
        // Ignored marks survive; their source cluster belongs to the new ligature.
        for (var i = matched[0] + 1; i < matched[^1]; i++)
            if (glyphs[i].Ligature == id) glyphs[i].Cluster = cluster;
        var oldCount = glyphs.Count; glyphs.RemoveComponents(matched); runEnd += glyphs.Count - oldCount;
        first.Glyph = replacement; first.SourceOffset = sourceStart; first.SourceLength = sourceEnd - sourceStart;
        first.Cluster = cluster; first.Ligature = id; first.Component = 0; first.ComponentCount = components;
        glyphs[matched[0]] = first;
    }

    private bool Context(int sub, ShapingBuffer glyphs, int at, int inputEnd, int flags, int filteringSet, int depth, out int next)
    {
        var format = table.U16(sub); next = at + 1;
        if (format == 3) return CoverageContext(sub, glyphs, at, inputEnd, flags, filteringSet, depth, out next);
        if (format is not (1 or 2)) throw OpenTypeTable.Unsupported("Unsupported OpenType chained-context substitution format.");
        var coverage = table.Coverage(table.Offset16(sub, sub + 2), glyphs[at].Glyph);
        if (coverage < 0) return false;
        var backClass = format == 2 ? table.Offset16(sub, sub + 4) : 0;
        var inputClass = format == 2 ? table.Offset16(sub, sub + 6) : 0;
        var lookClass = format == 2 ? table.Offset16(sub, sub + 8) : 0;
        var setsField = sub + (format == 2 ? 10 : 4); var count = table.Count(setsField, 2);
        var index = format == 2 ? table.Class(inputClass, glyphs[at].Glyph) : coverage;
        if (index >= count) return false;
        var set = table.Offset16(sub, setsField + 2 + index * 2, optional: true);
        if (set == 0) return false;
        var rules = table.Count(set, 2);
        for (var i = 0; i < rules; i++)
        {
            var p = table.Offset16(set, set + 2 + i * 2);
            var backCount = table.Count(p, 2); var back = p + 2; p = back + backCount * 2;
            var inputCount = table.U16(p);
            if (inputCount == 0) throw OpenTypeTable.Bad("An OpenType context has no input.");
            table.Items(inputCount); var input = p + 2; table.Range(input, (inputCount - 1L) * 2); p = input + (inputCount - 1) * 2;
            var lookCount = table.Count(p, 2); var look = p + 2; p = look + lookCount * 2;
            var actions = table.Count(p, 4); p += 2;
            if (!Match(glyphs, at - 1, -1, runStart, back, backCount, backClass, format == 2, flags, filteringSet, out _)) continue;
            var last = at;
            if (!Match(glyphs, at + 1, 1, inputEnd, input, inputCount - 1, inputClass, format == 2, flags, filteringSet, out var inputLast)) continue;
            if (inputCount > 1) last = inputLast;
            if (!Match(glyphs, last + 1, 1, runEnd, look, lookCount, lookClass, format == 2, flags, filteringSet, out _)) continue;
            var end = last + 1;
            ApplyActions(glyphs, at, ref end, p, actions, flags, filteringSet, depth);
            next = end; return true;
        }
        return false;
    }

    private bool CoverageContext(int sub, ShapingBuffer glyphs, int at, int inputEnd, int flags, int filteringSet, int depth, out int next)
    {
        next = at + 1; var p = sub + 2;
        var backCount = table.Count(p, 2); var back = p + 2; p = back + backCount * 2;
        var inputCount = table.Count(p, 2); var input = p + 2; p = input + inputCount * 2;
        if (inputCount == 0) throw OpenTypeTable.Bad("An OpenType context has no input.");
        var lookCount = table.Count(p, 2); var look = p + 2; p = look + lookCount * 2;
        var actions = table.Count(p, 4); p += 2;
        if (!MatchCoverage(glyphs, sub, at - 1, -1, runStart, back, backCount, flags, filteringSet, out _)) return false;
        if (!MatchCoverage(glyphs, sub, at, 1, inputEnd, input, inputCount, flags, filteringSet, out var last)) return false;
        if (!MatchCoverage(glyphs, sub, last + 1, 1, runEnd, look, lookCount, flags, filteringSet, out _)) return false;
        var end = last + 1;
        ApplyActions(glyphs, at, ref end, p, actions, flags, filteringSet, depth);
        next = end; return true;
    }

    private void ApplyActions(ShapingBuffer glyphs, int at, ref int end, int actions, int count, int flags, int filteringSet, int depth)
    {
        // Sequence indexes address the result of earlier actions, not the
        // original glyph positions (GSUB contextual substitution example).
        for (var i = 0; i < count; i++)
        {
            var index = table.U16(actions + i * 4); var target = at;
            for (var j = 0; j < index && target >= 0; j++) target = Next(glyphs, target + 1, 1, end, flags, filteringSet);
            if (target < 0 || target >= end) throw OpenTypeTable.Bad("Invalid OpenType contextual sequence index.");
            var before = glyphs.Count;
            ApplyAt(table.U16(actions + i * 4 + 2), glyphs, target, end, depth + 1, out _);
            var delta = glyphs.Count - before; end += delta;
        }
    }

    private bool Match(ShapingBuffer glyphs, int from, int step, int end, int pattern, int count,
        int classDefinition, bool useClasses, int flags, int filteringSet, out int last)
    {
        last = from - step;
        for (var i = 0; i < count; i++)
        {
            var cursor = Next(glyphs, from, step, end, flags, filteringSet);
            if (cursor < 0) return false;
            var value = useClasses ? table.Class(classDefinition, glyphs[cursor].Glyph) : glyphs[cursor].Glyph;
            if (value != table.U16(pattern + i * 2)) return false;
            last = cursor; from = cursor + step;
        }
        return true;
    }
    private bool MatchCoverage(ShapingBuffer glyphs, int origin, int from, int step, int end, int pattern,
        int count, int flags, int filteringSet, out int last)
    {
        last = from - step;
        for (var i = 0; i < count; i++)
        {
            var cursor = Next(glyphs, from, step, end, flags, filteringSet);
            if (cursor < 0 || table.Coverage(table.Offset16(origin, pattern + i * 2), glyphs[cursor].Glyph) < 0) return false;
            last = cursor; from = cursor + step;
        }
        return true;
    }
    private static bool Boundary(ShapingGlyph glyph) => glyph.Glyph < 0 || glyph.Kind is 2 or 3;
    private int Next(ShapingBuffer glyphs, int at, int step, int end, int flags, int filteringSet)
    {
        while (step > 0 ? at < end : at >= end)
        {
            context.Work(1);
            if (at < runStart || at >= runEnd || Boundary(glyphs[at])) return -1;
            if (!classes.Ignored(glyphs[at], flags, filteringSet)) return at;
            at += step;
        }
        return -1;
    }
    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        if (!table.Empty) context.Release(selected.Length + 32L);
        selected = [];
    }
}
