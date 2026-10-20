using System.Buffers.Binary;
using System.Text;

namespace Lokad.Image.Tooling;

// Original tiny programs for the public OpenType GSUB/chapter2/GDEF formats.
// Glyph numbers and rules are authored here; no upstream font tables are copied.
internal static class AuthoredOpenType
{
    internal sealed record Lookup(int Type, int Flags, byte[][] Subtables, int MarkSet = 0);
    internal sealed record Feature(string Tag, int[] Lookups);
    internal static uint Tag(string tag) => BinaryPrimitives.ReadUInt32BigEndian(Encoding.ASCII.GetBytes(tag));
    internal static byte[] Words(params int[] words)
    {
        var bytes = new byte[words.Length * 2];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(i * 2), unchecked((ushort)words[i]));
        return bytes;
    }
    internal static byte[] Attach(byte[] header, params (int Field, byte[] Bytes)[] children)
    {
        var bytes = new byte[header.Length + children.Sum(c => c.Bytes.Length)]; header.CopyTo(bytes, 0);
        var offset = header.Length;
        foreach (var child in children)
        {
            if (offset > ushort.MaxValue) throw new InvalidOperationException("Authored fixture exceeds Offset16.");
            BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(child.Field), (ushort)offset);
            child.Bytes.CopyTo(bytes, offset); offset += child.Bytes.Length;
        }
        return bytes;
    }
    internal static byte[] Tagged(params (string Tag, byte[] Bytes)[] records)
    {
        var header = new byte[2 + records.Length * 6]; Write(header, 0, records.Length);
        for (var i = 0; i < records.Length; i++) Encoding.ASCII.GetBytes(records[i].Tag).CopyTo(header, 2 + i * 6);
        return Attach(header, records.Select((r, i) => (6 + i * 6, r.Bytes)).ToArray());
    }
    internal static byte[] Language(int[] features, int required = 0xFFFF)
        => Words([0, required, features.Length, .. features]);
    internal static byte[] Script(int[] features, int required = 0xFFFF)
        => Attach(Words(0, 0), (0, Language(features, required)));
    internal static byte[] NamedLanguage(string language, int[] features)
    {
        var header = Words(0, 1, 0, 0, 0); Encoding.ASCII.GetBytes(language).CopyTo(header, 4);
        return Attach(header, (8, Language(features)));
    }
    internal static byte[] Substitution(Lookup[] lookups, Feature[] features, (string Tag, byte[] Bytes)[]? scripts = null)
    {
        var lookupData = lookups.Select(lookup =>
        {
            var header = new byte[6 + lookup.Subtables.Length * 2 + ((lookup.Flags & 16) != 0 ? 2 : 0)];
            Write(header, 0, lookup.Type); Write(header, 2, lookup.Flags); Write(header, 4, lookup.Subtables.Length);
            if ((lookup.Flags & 16) != 0) Write(header, 6 + lookup.Subtables.Length * 2, lookup.MarkSet);
            return Attach(header, lookup.Subtables.Select((sub, i) => (6 + i * 2, sub)).ToArray());
        }).ToArray();
        var lookupList = Attach(Words([lookups.Length, .. new int[lookups.Length]]), lookupData.Select((sub, i) => (2 + i * 2, sub)).ToArray());
        var featureList = Tagged(features.Select(f => (f.Tag, Words([0, f.Lookups.Length, .. f.Lookups]))).ToArray());
        var scriptList = Tagged(scripts ?? [("latn", Script(Enumerable.Range(0, features.Length).ToArray()))]);
        return Attach(Words(1, 0, 0, 0, 0), (4, scriptList), (6, featureList), (8, lookupList));
    }
    internal static byte[] Coverage(params int[] glyphs) => Words([1, glyphs.Length, .. glyphs]);
    internal static byte[] CoverageRanges(params (int First, int Last, int Index)[] ranges)
        => Words([2, ranges.Length, .. ranges.SelectMany(r => new[] { r.First, r.Last, r.Index })]);
    internal static byte[] ClassValues(int first, params int[] classes) => Words([1, first, classes.Length, .. classes]);
    internal static byte[] ClassRanges(params (int First, int Last, int Class)[] ranges)
        => Words([2, ranges.Length, .. ranges.SelectMany(r => new[] { r.First, r.Last, r.Class })]);
    internal static byte[] SingleDelta(byte[] coverage, int delta) => Attach(Words(1, 0, delta), (2, coverage));
    internal static byte[] Single(int[] inputs, int[] outputs) => Attach(Words([2, 0, outputs.Length, .. outputs]), (2, Coverage(inputs)));
    internal static byte[] Ligature(int first, params (int Output, int[] Tail)[] candidates)
    {
        var children = candidates.Select(c => Words([c.Output, c.Tail.Length + 1, .. c.Tail])).ToArray();
        var set = Attach(Words([children.Length, .. new int[children.Length]]), children.Select((c, i) => (2 + i * 2, c)).ToArray());
        return Attach(Words(1, 0, 1, 0), (2, Coverage(first)), (6, set));
    }
    private static byte[] Rule(int[] back, int[] inputTail, int[] look, (int Sequence, int Lookup)[] actions)
        => Words([back.Length, .. back, inputTail.Length + 1, .. inputTail, look.Length, .. look,
            actions.Length, .. actions.SelectMany(a => new[] { a.Sequence, a.Lookup })]);
    internal static byte[] ChainedGlyphs(int first, int[] back, int[] inputTail, int[] look, params (int Sequence, int Lookup)[] actions)
    {
        var set = Attach(Words(1, 0), (2, Rule(back, inputTail, look, actions)));
        return Attach(Words(1, 0, 1, 0), (2, Coverage(first)), (6, set));
    }
    internal static byte[] ChainedClasses(int first, int firstClass, byte[] backClasses, byte[] inputClasses,
        byte[] lookClasses, int[] back, int[] inputTail, int[] look, params (int Sequence, int Lookup)[] actions)
    {
        var set = Attach(Words(1, 0), (2, Rule(back, inputTail, look, actions)));
        var header = Words([2, 0, 0, 0, 0, firstClass + 1, .. new int[firstClass + 1]]);
        return Attach(header, (2, Coverage(first)), (4, backClasses), (6, inputClasses), (8, lookClasses), (12 + firstClass * 2, set));
    }
    internal static byte[] ChainedCoverages(int[] back, int[] input, int[] look, params (int Sequence, int Lookup)[] actions)
    {
        var header = Words([3, back.Length, .. new int[back.Length], input.Length, .. new int[input.Length],
            look.Length, .. new int[look.Length], actions.Length, .. actions.SelectMany(a => new[] { a.Sequence, a.Lookup })]);
        var children = new List<(int, byte[])>(); var p = 4;
        foreach (var glyph in back) { children.Add((p, Coverage(glyph))); p += 2; }
        p += 2;
        foreach (var glyph in input) { children.Add((p, Coverage(glyph))); p += 2; }
        p += 2;
        foreach (var glyph in look) { children.Add((p, Coverage(glyph))); p += 2; }
        return Attach(header, children.ToArray());
    }
    internal static byte[] SequenceCoverages(int[][] input, params (int Sequence, int Lookup)[] actions)
    {
        var header = Words([3, input.Length, actions.Length, .. new int[input.Length],
            .. actions.SelectMany(a => new[] { a.Sequence, a.Lookup })]);
        return Attach(header, input.Select((glyphs, i) => (6 + i * 2, Coverage(glyphs))).ToArray());
    }
    internal static byte[] Extension(int type, byte[] child)
    {
        var bytes = new byte[8 + child.Length]; Write(bytes, 0, 1); Write(bytes, 2, type);
        BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(4), 8); child.CopyTo(bytes, 8); return bytes;
    }
    internal static byte[] Definitions(byte[]? classes = null, byte[]? markClasses = null, int[][]? markSets = null)
    {
        var children = new List<(int, byte[])>();
        if (classes is not null) children.Add((4, classes));
        if (markClasses is not null) children.Add((10, markClasses));
        if (markSets is not null)
        {
            var header = new byte[4 + markSets.Length * 4]; Write(header, 0, 1); Write(header, 2, markSets.Length);
            var coverings = markSets.Select(Coverage).ToArray(); var sets = new byte[header.Length + coverings.Sum(c => c.Length)]; header.CopyTo(sets, 0);
            var cursor = header.Length;
            for (var i = 0; i < coverings.Length; i++)
            {
                BinaryPrimitives.WriteUInt32BigEndian(sets.AsSpan(4 + i * 4), (uint)cursor);
                coverings[i].CopyTo(sets, cursor); cursor += coverings[i].Length;
            }
            children.Add((12, sets));
        }
        return Attach(Words(1, 2, 0, 0, 0, 0, 0), children.ToArray());
    }
    internal static byte[] Positioning(Lookup[] lookups, Feature[] features)
        => Substitution(lookups, features);
    internal static byte[] ItemVariationStore(int[][][] regions, int wordCount, params int[][] rows)
    {
        var axes = regions.Length == 0 ? 1 : regions[0].Length;
        var regionList = Words([axes, regions.Length, .. regions.SelectMany(r => r.SelectMany(a => a))]);
        var dataHeader = Words([rows.Length, wordCount, regions.Length, .. Enumerable.Range(0, regions.Length)]);
        var data = new byte[dataHeader.Length + rows.Length * (regions.Length + wordCount)];
        dataHeader.CopyTo(data, 0); var cursor = dataHeader.Length;
        foreach (var row in rows)
        for (var i = 0; i < row.Length; i++)
        {
            if (i < wordCount) { Write(data, cursor, row[i]); cursor += 2; }
            else data[cursor++] = unchecked((byte)row[i]);
        }
        var store = new byte[12 + regionList.Length + data.Length]; Write(store, 0, 1); Write(store, 6, 1);
        BinaryPrimitives.WriteUInt32BigEndian(store.AsSpan(2), 12);
        BinaryPrimitives.WriteUInt32BigEndian(store.AsSpan(8), (uint)(12 + regionList.Length));
        regionList.CopyTo(store, 12); data.CopyTo(store, 12 + regionList.Length); return store;
    }
    internal static byte[] DefinitionsWithVariation(byte[] store, byte[]? classes = null)
    {
        classes ??= [];
        var bytes = new byte[18 + classes.Length + store.Length]; Write(bytes, 0, 1); Write(bytes, 2, 3);
        if (classes.Length != 0) Write(bytes, 4, 18);
        BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(14), (uint)(18 + classes.Length));
        classes.CopyTo(bytes, 18); store.CopyTo(bytes, 18 + classes.Length); return bytes;
    }
    internal static byte[] SinglePosition(int glyph, int format, params int[] values)
        => Attach(Words([1, 0, format, .. values]), (2, Coverage(glyph)));
    internal static byte[] SinglePositionArray(int[] glyphs, int valueFormat, params int[][] values)
        => Attach(Words([2, 0, valueFormat, values.Length, .. values.SelectMany(v => v)]), (2, Coverage(glyphs)));
    internal static byte[] PairPosition(int first, int firstFormat, int secondFormat, params (int Second, int[] FirstValue, int[] SecondValue)[] pairs)
    {
        var set = Words([pairs.Length, .. pairs.SelectMany(p => new[] { p.Second }.Concat(p.FirstValue).Concat(p.SecondValue))]);
        return Attach(Words(1, 0, firstFormat, secondFormat, 1, 0), (2, Coverage(first)), (10, set));
    }
    internal static byte[] ClassPairPosition(int first, byte[] firstClasses, byte[] secondClasses,
        int firstCount, int secondCount, int firstFormat, int secondFormat, params int[][] values)
        => Attach(Words([2, 0, firstFormat, secondFormat, 0, 0, firstCount, secondCount, .. values.SelectMany(v => v)]),
            (2, Coverage(first)), (8, firstClasses), (10, secondClasses));
    internal static byte[] Anchor(int x, int y, int format = 1)
        => format switch { 2 => Words(2, x, y, 3), 3 => Words(3, x, y, 0, 0), _ => Words(format, x, y) };
    internal static byte[] MarkPosition(int parent, int mark, byte[]? parentAnchor, byte[] markAnchor, int markClass = 0, int classCount = 1)
    {
        var marks = Attach(Words(1, markClass, 0), (4, markAnchor));
        var header = Words([1, .. new int[classCount]]);
        var parents = parentAnchor is null ? header : Attach(header, (2 + markClass * 2, parentAnchor));
        return Attach(Words(1, 0, 0, classCount, 0, 0), (2, Coverage(mark)), (4, Coverage(parent)), (8, marks), (10, parents));
    }
    internal static byte[] LigatureMarkPosition(int ligature, int mark, byte[] markAnchor, params byte[][] componentAnchors)
    {
        var marks = Attach(Words(1, 0, 0), (4, markAnchor));
        var attach = Attach(Words([componentAnchors.Length, .. new int[componentAnchors.Length]]), componentAnchors.Select((a, i) => (2 + i * 2, a)).ToArray());
        var ligatures = Attach(Words(1, 0), (2, attach));
        return Attach(Words(1, 0, 0, 1, 0, 0), (2, Coverage(mark)), (4, Coverage(ligature)), (8, marks), (10, ligatures));
    }
    internal static void Write(byte[] bytes, int offset, int value) => BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(offset), unchecked((ushort)value));
}
