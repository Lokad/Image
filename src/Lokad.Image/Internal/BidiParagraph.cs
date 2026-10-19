namespace Lokad.Image.Internal;

// Independent Unicode 17 UAX #9, rev.51 implementation. One paragraph is
// resolved before shaping/wrapping; L1/L2 are applied separately to each line.
// This primitive does not implement glyph shaping, L3 cluster policy or L4
// glyph mirroring: those belong to the consuming layout pipeline.
internal sealed class BidiParagraph : IDisposable
{
    private struct Item
    {
        internal int Scalar, Match, Run, Pair;
        internal BidiClass Original, Type;
        internal byte Embedding, Level;
    }
    private readonly record struct Status(byte Level, BidiClass Override, bool Isolate);
    private readonly OperationContext context;
    private Item[] items = [];
    private bool disposed;
    private long storage;
    internal int Count { get { Check(); return items.Length; } }
    internal byte BaseLevel { get; }

    internal BidiParagraph(ReadOnlySpan<int> scalars, OperationContext context, int baseLevel = -1)
    {
        this.context = context; context.Work(0);
        if (baseLevel is < -1 or > 1) throw new ArgumentOutOfRangeException(nameof(baseLevel));
        if (scalars.Length > context.Limits.MaxContainerItems)
            throw new ImageLimitException(ImageLimitKind.ContainerItems, "Bidirectional item limit exceeded.");
        if (scalars.Length > Array.MaxLength / 32)
            throw new ImageLimitException(ImageLimitKind.BufferLength, "Bidirectional storage exceeds the managed limit.");
        var owned = scalars.Length * 32L + 128; context.Reserve(owned); storage = owned;
        var temporary = 0L;
        try
        {
            items = new Item[scalars.Length];
            context.Reserve(scalars.Length * 24L + 256); temporary = scalars.Length * 24L + 256;
            var active = new int[items.Length]; var sequence = new int[items.Length];
            var first = new int[items.Length]; var last = new int[items.Length];
            var next = new int[items.Length]; var previous = new int[items.Length];
            var isolates = 0;
            for (var i = 0; i < items.Length; i++)
            {
                context.Work(1); var type = UnicodeTextProperties.Bidi(scalars[i]);
                if (type == BidiClass.B && i != items.Length - 1)
                    throw new ArgumentException("Resolve one paragraph at a time.", nameof(scalars));
                items[i] = new() { Scalar = scalars[i], Original = type, Type = type, Match = -1, Run = -1, Pair = -1 };
                if (Isolate(type)) sequence[isolates++] = i;
                else if (type == BidiClass.PDI && isolates != 0)
                {
                    var opener = sequence[--isolates]; items[i].Match = opener; items[opener].Match = i;
                }
            }
            BaseLevel = baseLevel < 0 ? FirstStrong(0, items.Length) : (byte)baseLevel;
            ExplicitLevels();
            var count = 0; var runs = 0;
            for (var i = 0; i < items.Length; i++)
            {
                context.Work(1); if (Removed(items[i].Original)) continue;
                if (count == 0 || items[i].Embedding != items[active[count - 1]].Embedding)
                {
                    first[runs] = count; next[runs] = previous[runs] = -1; runs++;
                }
                items[i].Run = runs - 1; last[runs - 1] = count; active[count++] = i;
            }
            for (var run = 0; run < runs; run++)
            {
                context.Work(1); var end = items[active[last[run]]];
                if (!Isolate(end.Original) || end.Match < 0) continue;
                var target = items[end.Match].Run;
                if (target <= run || items[active[first[target]]].Embedding != end.Embedding) continue;
                next[run] = target; previous[target] = run;
            }
            for (var run = 0; run < runs; run++)
            {
                context.Work(1); if (previous[run] >= 0) continue;
                var used = 0; var final = run;
                for (var part = run; part >= 0; part = next[part])
                {
                    final = part;
                    for (var j = first[part]; j <= last[part]; j++) { context.Work(1); sequence[used++] = active[j]; }
                }
                var begin = first[run]; var finish = last[final];
                var level = items[sequence[0]].Embedding;
                var before = begin == 0 ? BaseLevel : items[active[begin - 1]].Embedding;
                var after = finish + 1 == count || Isolate(items[sequence[used - 1]].Original)
                    ? BaseLevel : items[active[finish + 1]].Embedding;
                Resolve(sequence.AsSpan(0, used), level, Direction(Math.Max(level, before)), Direction(Math.Max(level, after)));
            }
        }
        catch { Dispose(); throw; }
        finally { if (temporary != 0) context.Release(temporary); }
    }

    private void Check() => ObjectDisposedException.ThrowIf(disposed, this);
    internal byte Level(int index) { Check(); return items[index].Level; }
    internal bool IsRemoved(int index) { Check(); return Removed(items[index].Original); }
    internal static bool Removed(BidiClass type) => type is BidiClass.LRE or BidiClass.RLE or BidiClass.LRO or BidiClass.RLO or BidiClass.PDF or BidiClass.BN;
    private static bool Isolate(BidiClass type) => type is BidiClass.LRI or BidiClass.RLI or BidiClass.FSI;
    private static bool Neutral(BidiClass type) => type is BidiClass.B or BidiClass.S or BidiClass.WS or BidiClass.ON or BidiClass.LRI or BidiClass.RLI or BidiClass.FSI or BidiClass.PDI;
    private static BidiClass Direction(int level) => (level & 1) == 0 ? BidiClass.L : BidiClass.R;
    private static BidiClass Strong(BidiClass type) => type is BidiClass.EN or BidiClass.AN ? BidiClass.R : type;
    private byte FirstStrong(int start, int end)
    {
        for (var i = start; i < end; i++)
        {
            context.Work(1); var item = items[i];
            if (Isolate(item.Original)) { i = item.Match < 0 ? end : item.Match; continue; }
            if (item.Original == BidiClass.L) return 0;
            if (item.Original is BidiClass.R or BidiClass.AL) return 1;
        }
        return 0;
    }
    private void ExplicitLevels()
    {
        Span<Status> stack = stackalloc Status[127]; var top = 0;
        stack[0] = new(BaseLevel, BidiClass.ON, false);
        var overflowIsolate = 0; var overflowEmbedding = 0; var validIsolate = 0;
        for (var i = 0; i < items.Length; i++)
        {
            context.Work(1); ref var item = ref items[i]; var status = stack[top];
            var type = item.Original; item.Level = item.Embedding = status.Level;
            if (type is BidiClass.RLE or BidiClass.LRE or BidiClass.RLO or BidiClass.LRO || Isolate(type))
            {
                var isolate = Isolate(type);
                var right = type is BidiClass.RLE or BidiClass.RLO or BidiClass.RLI;
                if (type == BidiClass.FSI) right = FirstStrong(i + 1, item.Match < 0 ? items.Length : item.Match) == 1;
                if (isolate && status.Override != BidiClass.ON) item.Type = status.Override;
                var level = status.Level + 1;
                if (((level & 1) == 1) != right) level++;
                if (level <= 125 && overflowIsolate == 0 && overflowEmbedding == 0)
                {
                    var over = type == BidiClass.RLO ? BidiClass.R : type == BidiClass.LRO ? BidiClass.L : BidiClass.ON;
                    stack[++top] = new((byte)level, over, isolate); if (isolate) validIsolate++;
                }
                else if (isolate) overflowIsolate++;
                else if (overflowIsolate == 0) overflowEmbedding++;
            }
            else if (type == BidiClass.PDI)
            {
                if (overflowIsolate > 0) overflowIsolate--;
                else if (validIsolate > 0)
                {
                    overflowEmbedding = 0;
                    while (!stack[top].Isolate) { context.Work(1); top--; }
                    top--; validIsolate--;
                }
                status = stack[top]; item.Level = item.Embedding = status.Level;
                if (status.Override != BidiClass.ON) item.Type = status.Override;
            }
            else if (type == BidiClass.PDF)
            {
                if (overflowIsolate > 0) continue;
                if (overflowEmbedding > 0) overflowEmbedding--;
                else if (top > 0 && !status.Isolate) top--;
            }
            else if (type == BidiClass.B) item.Level = item.Embedding = BaseLevel;
            else if (type != BidiClass.BN && status.Override != BidiClass.ON) item.Type = status.Override;
        }
    }

    private void Resolve(ReadOnlySpan<int> sequence, int level, BidiClass sos, BidiClass eos)
    {
        // W1, W2, W3. Preserve AL until the numeric scan has consumed it.
        var previous = sos;
        foreach (var at in sequence)
        {
            context.Work(1); ref var item = ref items[at];
            if (item.Type == BidiClass.NSM) item.Type = Isolate(previous) || previous == BidiClass.PDI ? BidiClass.ON : previous;
            previous = item.Type;
        }
        var strong = sos;
        foreach (var at in sequence)
        {
            context.Work(1); ref var item = ref items[at];
            if (item.Type is BidiClass.L or BidiClass.R or BidiClass.AL) strong = item.Type;
            else if (item.Type == BidiClass.EN && strong == BidiClass.AL) item.Type = BidiClass.AN;
            if (item.Type == BidiClass.AL) item.Type = BidiClass.R;
        }
        // W4 through W7. ET runs are consumed once, without quadratic searches.
        for (var i = 1; i + 1 < sequence.Length; i++)
        {
            context.Work(1); ref var item = ref items[sequence[i]];
            var left = items[sequence[i - 1]].Type; var right = items[sequence[i + 1]].Type;
            if (left == right && (left == BidiClass.EN && item.Type is BidiClass.ES or BidiClass.CS
                || left == BidiClass.AN && item.Type == BidiClass.CS)) item.Type = left;
        }
        for (var i = 0; i < sequence.Length; i++)
        {
            context.Work(1); if (items[sequence[i]].Type != BidiClass.ET) continue;
            var start = i; while (i + 1 < sequence.Length && items[sequence[i + 1]].Type == BidiClass.ET) { context.Work(1); i++; }
            if ((start > 0 ? items[sequence[start - 1]].Type : sos) != BidiClass.EN
                && (i + 1 < sequence.Length ? items[sequence[i + 1]].Type : eos) != BidiClass.EN) continue;
            for (var j = start; j <= i; j++) { context.Work(1); items[sequence[j]].Type = BidiClass.EN; }
        }
        strong = sos;
        foreach (var at in sequence)
        {
            context.Work(1); ref var item = ref items[at];
            if (item.Type is BidiClass.ES or BidiClass.ET or BidiClass.CS) item.Type = BidiClass.ON;
            if (item.Type is BidiClass.L or BidiClass.R) strong = item.Type;
            else if (item.Type == BidiClass.EN && strong == BidiClass.L) item.Type = BidiClass.L;
        }
        Brackets(sequence, Direction(level), sos);
        // N1/N2, then I1/I2. Original embedding levels stay available for X10.
        previous = sos;
        for (var i = 0; i < sequence.Length; i++)
        {
            context.Work(1); var type = items[sequence[i]].Type;
            if (!Neutral(type)) { previous = Strong(type); continue; }
            var start = i; while (i + 1 < sequence.Length && Neutral(items[sequence[i + 1]].Type)) { context.Work(1); i++; }
            var after = i + 1 == sequence.Length ? eos : Strong(items[sequence[i + 1]].Type);
            var resolved = previous == after ? previous : Direction(level);
            for (var j = start; j <= i; j++) { context.Work(1); items[sequence[j]].Type = resolved; }
            previous = resolved;
        }
        foreach (var at in sequence)
        {
            context.Work(1); ref var item = ref items[at];
            item.Level += (byte)((level & 1) != 0 ? item.Type is BidiClass.L or BidiClass.EN or BidiClass.AN ? 1 : 0
                : item.Type == BidiClass.R ? 1 : item.Type is BidiClass.EN or BidiClass.AN ? 2 : 0);
        }
    }

    private static int CanonicalBracket(int scalar) => scalar == 0x232A ? 0x3009 : scalar;
    private void Brackets(ReadOnlySpan<int> sequence, BidiClass direction, BidiClass sos)
    {
        Span<int> stack = stackalloc int[63]; var used = 0;
        for (var i = 0; i < sequence.Length; i++)
        {
            context.Work(1); var item = items[sequence[i]];
            if (item.Type != BidiClass.ON) continue;
            var partner = UnicodeTextProperties.Bracket(item.Scalar, out var opening);
            if (partner < 0) continue;
            if (opening)
            {
                if (used == stack.Length)
                {
                    foreach (var at in sequence) { context.Work(1); items[at].Pair = -1; }
                    return; // BD16 discards ALL pairs on bracket stack overflow.
                }
                stack[used++] = i;
            }
            else for (var j = used - 1; j >= 0; j--)
            {
                context.Work(1); var open = stack[j];
                var close = UnicodeTextProperties.Bracket(items[sequence[open]].Scalar, out _);
                if (CanonicalBracket(close) != CanonicalBracket(item.Scalar)) continue;
                items[sequence[open]].Pair = i; used = j; break;
            }
        }
        // Logical opening order, rather than the closing order of a stack pop.
        var preceding = sos;
        for (var i = 0; i < sequence.Length; i++)
        {
            context.Work(1); ref var item = ref items[sequence[i]]; var close = item.Pair;
            if (close >= 0)
            {
                var inside = BidiClass.ON;
                for (var j = i + 1; j < close; j++)
                {
                    context.Work(1); var type = Strong(items[sequence[j]].Type);
                    if (type == direction) { inside = type; break; }
                    if (type is BidiClass.L or BidiClass.R) inside = type;
                }
                if (inside != BidiClass.ON)
                {
                    var resolved = inside == direction || preceding == direction ? direction : preceding;
                    item.Type = items[sequence[close]].Type = resolved;
                    PropagateMarks(sequence, i + 1, resolved); PropagateMarks(sequence, close + 1, resolved);
                }
            }
            var current = Strong(item.Type); if (current is BidiClass.L or BidiClass.R) preceding = current;
        }
    }
    private void PropagateMarks(ReadOnlySpan<int> sequence, int start, BidiClass type)
    {
        for (var i = start; i < sequence.Length && items[sequence[i]].Original == BidiClass.NSM; i++)
        { context.Work(1); items[sequence[i]].Type = type; }
    }

    // Borrowed output storage. No mutation of paragraph levels: separate line
    // requests remain independent, and a failed/cancelled request is disposable.
    internal int ReorderLine(int start, int length, Span<int> order, Span<byte> levels)
    {
        Check(); context.Work(0);
        if (start < 0 || length < 0 || start > items.Length - length) throw new ArgumentOutOfRangeException(nameof(length));
        if (order.Length < length || levels.Length < length) throw new ArgumentException("Line storage is too short.");
        var count = 0; var trailing = 0;
        for (var i = start; i < start + length; i++)
        {
            context.Work(1); var item = items[i]; if (Removed(item.Original)) continue;
            order[count] = i; levels[count] = item.Level;
            if (item.Original is BidiClass.B or BidiClass.S)
            {
                for (var j = trailing; j <= count; j++) { context.Work(1); levels[j] = BaseLevel; }
                trailing = count + 1;
            }
            else if (item.Original != BidiClass.WS && !Isolate(item.Original) && item.Original != BidiClass.PDI) trailing = count + 1;
            count++;
        }
        for (var i = trailing; i < count; i++) { context.Work(1); levels[i] = BaseLevel; }
        var highest = 0; var lowestOdd = 127;
        for (var i = 0; i < count; i++)
        {
            context.Work(1); highest = Math.Max(highest, levels[i]); if ((levels[i] & 1) != 0) lowestOdd = Math.Min(lowestOdd, levels[i]);
        }
        for (var level = highest; level >= lowestOdd; level--)
            for (var i = 0; i < count; i++)
            {
                context.Work(1); if (levels[i] < level) continue;
                var first = i; while (i + 1 < count && levels[i + 1] >= level) { context.Work(1); i++; }
                order.Slice(first, i - first + 1).Reverse(); levels.Slice(first, i - first + 1).Reverse();
            }
        return count;
    }
    public void Dispose()
    {
        if (disposed) return; disposed = true; items = [];
        if (storage != 0) context.Release(storage); storage = 0;
    }
}
