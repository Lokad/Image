using System.Runtime.ExceptionServices;

namespace Lokad.Image.Internal;

// Original scan conversion: split linear edges at vertices/intersections, then
// integrate each nonzero-winding trapezoid clipped to a pixel. Quadratic curves
// are subdivided until their control-to-chord midpoint deviation is <=1/1024 px.
// Only edges, event/crossing arrays and one coverage row are stored, never a raster.
internal sealed class TextRasterizer : IDisposable
{
    private readonly OperationContext context;
    private readonly Dictionary<TextLayout.Shape, RasterShape> shapes = new();
    private readonly Crossing[] crossings;
    private readonly double[] row;
    private readonly IComparer<Crossing> crossingComparer;
    private readonly IComparer<double> eventComparer;
    private long bytes;

    private readonly record struct Point(double X, double Y)
    {
        internal static Point Mid(Point a, Point b) => new(a.X * .5 + b.X * .5, a.Y * .5 + b.Y * .5);
    }
    private readonly record struct Edge(Point A, Point B)
    {
        internal double MinY => Math.Min(A.Y, B.Y);
        internal double MaxY => Math.Max(A.Y, B.Y);
        internal double X(double y) => A.X + (B.X - A.X) * ((y - A.Y) / (B.Y - A.Y));
        internal int Winding => B.Y > A.Y ? 1 : -1;
    }
    private readonly record struct Curve(Point A, Point C, Point B, int Depth);
    private readonly record struct Crossing(double X, int Index);

    internal TextRasterizer(TextLayout layout, OperationContext context, int width)
    {
        this.context = context;
        Admit(4096); // Owner/comparers plus bounded sort/integration frame allowance.
        crossingComparer = Comparer<Crossing>.Create((a, b) =>
        { context.Cancellation.ThrowIfCancellationRequested(); var order = a.X.CompareTo(b.X); return order != 0 ? order : a.Index.CompareTo(b.Index); });
        eventComparer = Comparer<double>.Create((a, b) => { context.Cancellation.ThrowIfCancellationRequested(); return a.CompareTo(b); });
        try
        {
            var maximum = 0;
            foreach (var p in layout.Placements)
            {
                context.Cancellation.ThrowIfCancellationRequested(); var shape = p.Shape!;
                if (shapes.ContainsKey(shape)) continue;
                Admit(512); var prepared = new RasterShape(this);
                try { prepared.Build(shape.Outline, layout.FontSize / layout.Font.UnitsPerEm); shapes.Add(shape, prepared); }
                catch { prepared.Dispose(); throw; }
                maximum = Math.Max(maximum, prepared.Count);
            }
            Admit(32 + maximum * 16L); Admit(32 + width * 8L);
            crossings = new Crossing[maximum]; row = new double[width];
        }
        catch { Dispose(); throw; }
    }
    private void Admit(long amount)
    {
        if (amount > Array.MaxLength) throw new ImageLimitException(ImageLimitKind.BufferLength, "Text raster storage exceeds the managed limit.");
        context.Reserve(amount); bytes += amount;
    }
    private void Release(long amount) { context.Release(amount); bytes -= amount; }

    internal void Draw(ImageBuffer canvas, TextLayout layout, double x, double y, RgbaColor color,
        int left, int top, int right, int bottom)
    {
        foreach (var glyph in layout.Placements)
        {
            context.Cancellation.ThrowIfCancellationRequested(); var shape = shapes[glyph.Shape!]; if (shape.Count == 0) continue;
            var dx = x + glyph.X; var dy = y + glyph.Y;
            var first = Math.Max(top, Math.Floor(shape.MinY + dy)); var last = Math.Min(bottom, Math.Ceiling(shape.MaxY + dy));
            if (first >= last) continue;
            var eventIndex = 0;
            for (var py = (int)first; py < last; py++)
            {
                for (var offset = 0; offset < row.Length;)
                {
                    var count = Math.Min(4096, row.Length - offset); context.Cancellation.ThrowIfCancellationRequested();
                    row.AsSpan(offset, count).Clear(); offset += count;
                }
                var band = py - dy; var rowEnd = py + 1 - dy;
                while (eventIndex < shape.EventCount && shape.Events[eventIndex] <= band) { context.Cancellation.ThrowIfCancellationRequested(); eventIndex++; }
                while (eventIndex < shape.EventCount && shape.Events[eventIndex] < rowEnd)
                {
                    var end = shape.Events[eventIndex++];
                    if (end > band) Band(shape, band, end, dx, dy - py, left, right);
                    band = end;
                }
                if (rowEnd > band) Band(shape, band, rowEnd, dx, dy - py, left, right);
                for (var i = 0; i < row.Length;)
                {
                    var count = Math.Min(128, row.Length - i); context.Cancellation.ThrowIfCancellationRequested();
                    for (var j = i; j < i + count; j++)
                    {
                        var coverage = (uint)Math.Clamp(Math.Floor(row[j] * 65535 + .5), 0, 65535);
                        if (coverage != 0) Blend(canvas, left + j, py, color, coverage);
                    }
                    i += count;
                }
            }
        }
    }

    private void Band(RasterShape shape, double top, double bottom, double dx, double dy, int left, int right)
    {
        var middle = top * .5 + bottom * .5; var count = 0;
        for (var i = 0; i < shape.Count; i++)
        {
            context.Cancellation.ThrowIfCancellationRequested(); var edge = shape.Edges[i];
            if (edge.MinY < middle && middle < edge.MaxY) crossings[count++] = new(edge.X(middle), i);
        }
        Sort(crossings, count, crossingComparer);
        var winding = 0; Edge start = default;
        for (var i = 0; i < count; i++)
        {
            context.Cancellation.ThrowIfCancellationRequested(); var edge = shape.Edges[crossings[i].Index]; var previous = winding; winding += edge.Winding;
            if (previous == 0 && winding != 0) start = edge;
            else if (previous != 0 && winding == 0)
            {
                var a = start.X(top) + dx; var b = edge.X(top) + dx;
                var c = edge.X(bottom) + dx; var d = start.X(bottom) + dx;
                var first = Math.Max(left, Math.Floor(Math.Min(a, d))); var last = Math.Min(right, Math.Ceiling(Math.Max(b, c)));
                if (first >= last) continue;
                for (var px = (int)first; px < last; px++)
                {
                    context.Cancellation.ThrowIfCancellationRequested();
                    row[px - left] += Area(a - px, b - px, c - px, d - px, top + dy, bottom + dy);
                }
            }
        }
        if (winding != 0) throw new ImageDataException("Unclosed text contour.");
    }

    // Polygon clipping happens in pixel-local coordinates before the area sum.
    private static double Area(double a, double b, double c, double d, double top, double bottom)
    {
        Span<Point> first = stackalloc Point[8]; Span<Point> second = stackalloc Point[8];
        first[0] = new(a, top); first[1] = new(b, top); first[2] = new(c, bottom); first[3] = new(d, bottom);
        var count = Clip(first[..4], second, 0, greater: true);
        count = Clip(second[..count], first, 1, greater: false);
        if (count < 3) return 0;
        double result = 0; var previous = first[count - 1];
        for (var i = 0; i < count; i++) { var p = first[i]; result += previous.X * p.Y - p.X * previous.Y; previous = p; }
        return Math.Abs(result) * .5;
    }
    private static int Clip(ReadOnlySpan<Point> input, Span<Point> output, double boundary, bool greater)
    {
        if (input.IsEmpty) return 0;
        var count = 0; var previous = input[^1]; var wasInside = greater ? previous.X >= boundary : previous.X <= boundary;
        foreach (var point in input)
        {
            var inside = greater ? point.X >= boundary : point.X <= boundary;
            if (inside != wasInside)
            {
                var t = (boundary - previous.X) / (point.X - previous.X);
                output[count++] = new(boundary, previous.Y + (point.Y - previous.Y) * t);
            }
            if (inside) output[count++] = point;
            previous = point; wasInside = inside;
        }
        return count;
    }

    private static void Blend(ImageBuffer canvas, int x, int y, RgbaColor color, uint coverage)
    {
        const ulong maximum = 65535, denominator = maximum * maximum;
        var sa = (ulong)color.A * coverage; if (sa == 0) return;
        var dm = canvas.BitDepth == 8 ? 255UL : maximum; var da = (ulong)canvas.Sample(x, y, 3);
        var alpha = sa * dm + da * (denominator - sa);
        var divisor = (UInt128)maximum * alpha;
        for (var channel = 0; channel < 3; channel++)
        {
            var sample = channel == 0 ? color.R : channel == 1 ? color.G : color.B;
            var numerator = (UInt128)sample * sa * dm * dm
                + (UInt128)(uint)canvas.Sample(x, y, channel) * da * (denominator - sa) * maximum;
            canvas.SetSample(x, y, channel, (int)((numerator + divisor / 2) / divisor));
        }
        canvas.SetSample(x, y, 3, (int)((alpha + denominator / 2) / denominator));
    }

    private static void Sort<T>(T[] values, int count, IComparer<T> comparer)
    {
        try { Array.Sort(values, 0, count, comparer); }
        catch (InvalidOperationException e) when (e.InnerException is OperationCanceledException or ImageLimitException)
        { ExceptionDispatchInfo.Capture(e.InnerException).Throw(); }
    }

    private sealed class RasterShape(TextRasterizer owner) : IDisposable
    {
        internal Edge[] Edges = [];
        internal double[] Events = [];
        internal int Count, EventCount;
        internal double MinY, MaxY;
        private long edgeBytes, eventBytes;

        internal void Build(FontOutline outline, double scale)
        {
            const int depthLimit = 24;
            owner.Admit((depthLimit + 1) * 64);
            try
            {
                Span<Curve> stack = stackalloc Curve[depthLimit + 1];
                var segments = outline.Segments();
                Point Position(FontPoint p) => new((p.X - outline.Left.X) * scale, -p.Y * scale);
                while (segments.Next(out var segment))
                {
                    owner.context.Cancellation.ThrowIfCancellationRequested(); var a = Position(segment.Start); var b = Position(segment.End);
                    if (!segment.Quadratic) { Add(a, b); continue; }
                    var used = 1; stack[0] = new(a, Position(segment.Control), b, 0);
                    while (used != 0)
                    {
                        owner.context.Cancellation.ThrowIfCancellationRequested(); var curve = stack[--used];
                        var deviation = Math.Max(Math.Abs(curve.A.X - 2 * curve.C.X + curve.B.X), Math.Abs(curve.A.Y - 2 * curve.C.Y + curve.B.Y)) * .25;
                        if (deviation <= 1.0 / 1024) { Add(curve.A, curve.B); continue; }
                        if (curve.Depth == depthLimit) throw new UnsupportedImageException(UnsupportedImageFeature.FontOutline, "Text curve subdivision exceeds 24 levels.");
                        var ac = Point.Mid(curve.A, curve.C); var cb = Point.Mid(curve.C, curve.B); var mid = Point.Mid(ac, cb);
                        stack[used++] = new(mid, cb, curve.B, curve.Depth + 1);
                        stack[used++] = new(curve.A, ac, mid, curve.Depth + 1);
                    }
                }
            }
            finally { owner.Release((depthLimit + 1) * 64); }
            for (var i = 0; i < Count; i++) { Event(Edges[i].MinY); Event(Edges[i].MaxY); }
            for (var i = 0; i < Count; i++)
            for (var j = i + 1; j < Count; j++)
            {
                owner.context.Cancellation.ThrowIfCancellationRequested(); var a = Edges[i]; var b = Edges[j];
                var low = Math.Max(a.MinY, b.MinY); var high = Math.Min(a.MaxY, b.MaxY); if (low >= high) continue;
                var d0 = a.X(low) - b.X(low); var d1 = a.X(high) - b.X(high);
                if (d0 < 0 && d1 > 0 || d0 > 0 && d1 < 0)
                {
                    var y = low + (high - low) * (d0 / (d0 - d1));
                    if (low < y && y < high) Event(y);
                }
            }
            Sort(Events, EventCount, owner.eventComparer);
            var unique = 0;
            for (var i = 0; i < EventCount; i++)
            { owner.context.Cancellation.ThrowIfCancellationRequested(); if (unique == 0 || Events[i] != Events[unique - 1]) Events[unique++] = Events[i]; }
            EventCount = unique;
            if (unique != 0) { MinY = Events[0]; MaxY = Events[unique - 1]; }
        }
        private void Add(Point a, Point b)
        {
            if (a.Y == b.Y) return;
            Grow(ref Edges, Count, 40, ref edgeBytes); Edges[Count++] = new(a, b);
        }
        private void Event(double y) { Grow(ref Events, EventCount, 8, ref eventBytes); Events[EventCount++] = y; }
        private void Grow<T>(ref T[] array, int count, int itemBytes, ref long bytes)
        {
            if (count == owner.context.Limits.MaxContainerItems) throw new ImageLimitException(ImageLimitKind.ContainerItems, "Text raster edge/event limit exceeded.");
            if (count < array.Length) return;
            var capacity = (int)Math.Min(owner.context.Limits.MaxContainerItems, Math.Max(count + 1L, Math.Max(8, array.Length * 2L)));
            var nextBytes = 32 + capacity * (long)itemBytes; owner.Admit(nextBytes);
            T[] next;
            try { owner.context.Cancellation.ThrowIfCancellationRequested(); next = new T[capacity]; array.AsSpan(0, count).CopyTo(next); }
            catch { owner.Release(nextBytes); throw; }
            owner.Release(bytes); bytes = nextBytes; array = next;
        }
        public void Dispose() { owner.Release(edgeBytes + eventBytes); edgeBytes = eventBytes = 0; Edges = []; Events = []; }
    }

    public void Dispose()
    {
        foreach (var shape in shapes.Values) shape.Dispose();
        shapes.Clear(); context.Release(bytes); bytes = 0;
    }
}
