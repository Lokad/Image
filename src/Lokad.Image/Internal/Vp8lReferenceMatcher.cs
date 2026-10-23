namespace Lokad.Image.Internal;

// Encoder workspace owned by its operation. Dispose before the operation context;
// callers borrow Values only while this owner is alive.
internal sealed class Vp8lMatchIndex : IDisposable
{
    private readonly OperationContext context;
    private uint[]? values;
    internal ReadOnlySpan<uint> Values => values ?? throw new ObjectDisposedException(nameof(Vp8lMatchIndex));
    internal Span<uint> Writable => values ?? throw new ObjectDisposedException(nameof(Vp8lMatchIndex));

    internal Vp8lMatchIndex(OperationContext context, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        this.context = context;
        context.Work(0);
        context.Reserve(length * 4L);
        try { context.Work(0); values = new uint[length]; }
        catch { context.Release(length * 4L); throw; }
    }

    public void Dispose()
    {
        var owned = Interlocked.Exchange(ref values, null);
        if (owned is not null) context.Release(owned.Length * 4L);
    }
}

// Independent match selection for the reference VP8L encoder being integrated.
// Inputs are borrowed. Major temporary capacity is admitted before allocation;
// the returned index keeps its separate operation charge until disposal.
internal static class Vp8lReferenceMatcher
{
    private const int MaximumLength = 4095;
    private const int HeadCount = 1 << 18;

    private static int Key(uint first, uint next)
        => (int)(unchecked(next * 0xc6a4a793u + first * 0x5bd1e996u) >> 14);

    private sealed class Heads
    {
        private readonly int[]? dense;
        private readonly Dictionary<int, int>? sparse;
        internal Heads(int count, OperationContext context)
        {
            // Sparse storage preserves the same eighteen-bit keys and chain order.
            // Its capacity is fixed from input length, never from available budget.
            if (count < 16384) sparse = new Dictionary<int, int>(count);
            else
            {
                dense = new int[HeadCount];
                for (var at = 0; at < dense.Length; at += 4096)
                {
                    var length = Math.Min(4096, dense.Length - at);
                    context.Work(length); Array.Fill(dense, -1, at, length);
                }
            }
        }
        internal int Get(int key) => dense is not null ? dense[key]
            : sparse!.TryGetValue(key, out var at) ? at : -1;
        internal void Set(int key, int at)
        { if (dense is not null) dense[key] = at; else sparse![key] = at; }
    }

    internal static Vp8lMatchIndex Build(ReadOnlySpan<uint> pixels, int width,
        int quality, bool lowEffort, OperationContext context)
    {
        Validate(pixels.Length, width);
        if (quality is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(quality));
        var result = new Vp8lMatchIndex(context, pixels.Length);
        long scratch = 0;
        try
        {
            if (pixels.Length <= 2) return result;
            // Dictionary backing capacity has a coarse allowance for its buckets,
            // entries and prime rounding. No dictionary grows after construction:
            // there can be at most pixels.Length inserted keys.
            var headBytes = pixels.Length < 16384 ? pixels.Length * 32L + 512 : HeadCount * 4L;
            var required = headBytes + pixels.Length * 4L;
            context.Reserve(required); scratch = required;
            context.Work(0);
            var heads = new Heads(pixels.Length, context);
            var previous = new int[pixels.Length];
            for (var at = 0; at < previous.Length; at += 4096)
            {
                var length = Math.Min(4096, previous.Length - at);
                context.Work(length); Array.Fill(previous, -1, at, length);
            }
            var position = 0; var repeated = pixels[0] == pixels[1];
            while (position < pixels.Length - 2)
            {
                context.Work(1);
                var nextRepeat = pixels[position + 1] == pixels[position + 2];
                if (repeated && nextRepeat)
                {
                    var run = 1;
                    while (position + run + 2 < pixels.Length)
                    {
                        context.Work(1);
                        if (pixels[position + run + 2] != pixels[position]) break;
                        run++;
                    }
                    if (run > MaximumLength) { position += run - MaximumLength; run = MaximumLength; }
                    while (run > 0)
                    {
                        context.Work(1);
                        var key = Key(pixels[position], (uint)run--);
                        previous[position] = heads.Get(key); heads.Set(key, position++);
                    }
                    repeated = false;
                }
                else
                {
                    var key = Key(pixels[position], pixels[position + 1]);
                    previous[position] = heads.Get(key); heads.Set(key, position++); repeated = nextRepeat;
                }
            }
            previous[position] = heads.Get(Key(pixels[position], pixels[position + 1]));
            var window = Math.Min((1 << 20) - 120,
                quality > 75 ? (1 << 20) - 120 : width * (quality > 50 ? 256 : quality > 25 ? 64 : 16));
            var checks = 8 + quality * quality / 128;
            var values = result.Writable;
            position = pixels.Length - 2;
            while (position > 0)
            {
                context.Work(1);
                var limit = Math.Min(MaximumLength, pixels.Length - 1 - position);
                var enough = Math.Min(256, limit); var minimum = Math.Max(0, position - window);
                var candidate = previous[position]; var remaining = checks; var length = 0; var distance = 0;
                if (!lowEffort)
                {
                    if (position >= width)
                    {
                        var found = Length(pixels, position - width, position, length, limit, context);
                        if (found > length) { length = found; distance = width; }
                        remaining--;
                    }
                    var left = Length(pixels, position - 1, position, length, limit, context);
                    if (left > length) { length = left; distance = 1; }
                    remaining--;
                    if (length == MaximumLength) candidate = minimum - 1;
                }
                while (candidate >= minimum && --remaining != 0)
                {
                    context.Work(1);
                    if (pixels[candidate + length] == pixels[position + length])
                    {
                        var found = Length(pixels, candidate, position, 0, limit, context);
                        if (found > length)
                        { length = found; distance = position - candidate; if (length >= enough) break; }
                    }
                    candidate = previous[candidate];
                }
                var extensionOrigin = position;
                while (true)
                {
                    context.Work(1);
                    values[position] = (uint)(distance << 12 | length); position--;
                    if (distance == 0 || position == 0 || position < distance || pixels[position - distance] != pixels[position]) break;
                    if (length == MaximumLength && distance != 1 && position + MaximumLength < extensionOrigin) break;
                    if (length < MaximumLength) { length++; extensionOrigin = position; }
                }
            }
            return result;
        }
        catch { result.Dispose(); throw; }
        finally { if (scratch != 0) context.Release(scratch); }
    }

    private static int Length(ReadOnlySpan<uint> pixels, int first, int second,
        int already, int limit, OperationContext context)
    {
        context.Work(1);
        if (pixels[first + already] != pixels[second + already]) return 0;
        var n = 0;
        while (n < limit)
        { context.Work(1); if (pixels[first + n] != pixels[second + n]) break; n++; }
        return n;
    }

    internal static Vp8lMatchIndex BuildBox(ReadOnlySpan<uint> pixels, int width,
        ReadOnlySpan<uint> ordinary, OperationContext context)
    {
        Validate(pixels.Length, width);
        if (ordinary.Length != pixels.Length) throw new ArgumentException("The ordinary index length differs.", nameof(ordinary));
        var result = new Vp8lMatchIndex(context, pixels.Length); long scratch = 0;
        try
        {
            context.Reserve(pixels.Length * 2L); scratch = pixels.Length * 2L;
            context.Work(0); var counts = new ushort[pixels.Length]; counts[^1] = 1;
            for (var at = pixels.Length - 2; at >= 0; at--)
            {
                context.Work(1);
                counts[at] = pixels[at] == pixels[at + 1] ? (ushort)Math.Min(MaximumLength, counts[at + 1] + 1) : (ushort)1;
            }
            Span<int> offsets = stackalloc int[32]; offsets.Clear();
            Span<int> entering = stackalloc int[32]; var count = 0; var newCount = 0;
            for (var y = 0; y <= 6; y++) for (var x = -6; x <= 6; x++)
            {
                var distance = y * width + x; if (distance <= 0) continue;
                context.Work(120); var code = Plane(distance, width) - 1;
                if (code < 32) offsets[code] = distance;
            }
            for (var i = 0; i < offsets.Length; i++) if (offsets[i] != 0) offsets[count++] = offsets[i];
            offsets = offsets[..count];
            for (var i = 0; i < count; i++)
            {
                context.Work(count); var reachable = false;
                for (var j = 0; j < count; j++) if (offsets[i] == offsets[j] + 1) { reachable = true; break; }
                if (!reachable) entering[newCount++] = offsets[i];
            }
            var previousLength = 0; var previousDistance = 0; var values = result.Writable;
            for (var at = 1; at < pixels.Length; at++)
            {
                context.Work(1); var length = (int)(ordinary[at] & 4095); var distance = (int)(ordinary[at] >> 12);
                if (length != MaximumLength || !offsets.Contains(distance))
                {
                    var carry = previousLength > 1 && previousLength < MaximumLength;
                    length = carry ? previousLength - 1 : 0; distance = carry ? previousDistance : 0;
                    foreach (var candidate in carry ? entering[..newCount] : offsets)
                    {
                        context.Work(1); var other = at - candidate;
                        if (other < 0 || pixels[other] != pixels[at]) continue;
                        var found = 0; var cursor = at;
                        do
                        {
                            context.Work(1); var a = counts[other]; var b = counts[cursor];
                            if (a != b) { found += Math.Min(a, b); break; }
                            found += a; other += a; cursor += a;
                        } while (found <= MaximumLength && cursor < pixels.Length && pixels[other] == pixels[cursor]);
                        if (found <= length) continue;
                        length = Math.Min(MaximumLength, found); distance = candidate;
                        if (length == MaximumLength) break;
                    }
                }
                if (length > 4)
                { values[at] = (uint)(distance << 12 | length); previousLength = length; previousDistance = distance; }
                else { previousLength = 0; previousDistance = 0; }
            }
            return result;
        }
        catch { result.Dispose(); throw; }
        finally { if (scratch != 0) context.Release(scratch); }
    }

    private static void Validate(int count, int width)
    {
        if (count < 1 || width is < 1 or > 16384 || count % width != 0 || count / width > 16384)
            throw new ArgumentException("Expected complete VP8L pixel rows.");
    }

    // Rank the RFC 9649 plane geometry by squared radius, then descending y,
    // then positive x before negative x. No implementation lookup table is used.
    private static int Plane(int distance, int width)
    {
        var y = distance / width; var x = distance % width;
        if (x <= 8 && y < 8) return Rank(x, y);
        if (x > width - 8 && y < 7) return Rank(x - width, y + 1);
        return distance + 120;
    }
    private static int Rank(int x, int y)
    {
        var radius = x * x + y * y; var rank = 1;
        for (var yy = 0; yy < 8; yy++) for (var xx = -7; xx <= 8; xx++)
        {
            if (yy == 0 && xx <= 0) continue;
            var r = xx * xx + yy * yy;
            if (r < radius || r == radius && (yy > y || yy == y && xx >= 0 && x < 0)) rank++;
        }
        return rank;
    }
}
