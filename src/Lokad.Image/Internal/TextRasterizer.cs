namespace Lokad.Image.Internal;

// Original scan conversion: split linear edges at vertices/intersections, then
// integrate each nonzero-winding trapezoid clipped to a pixel. Quadratic curves
// are subdivided until their control-to-chord midpoint deviation is <=1/1024 px.
// Only edges, event/crossing arrays and one coverage row are stored, never a raster.
internal sealed partial class TextRasterizer : IDisposable
{
    private readonly OperationContext context;
    private readonly Dictionary<TextLayout.Shape, RasterShape> shapes = new();
    private readonly Crossing[] crossings;
    private readonly double[] row;
    private long bytes;

    private readonly record struct Point(double X, double Y)
    {
        internal static Point Mid(Point a, Point b) => new(a.X * .5 + b.X * .5, a.Y * .5 + b.Y * .5);
    }
    private readonly record struct Edge(Point A, Point B, int Order)
    {
        internal double MinY => Math.Min(A.Y, B.Y);
        internal double MaxY => Math.Max(A.Y, B.Y);
        internal double X(double y) => A.X + (B.X - A.X) * ((y - A.Y) / (B.Y - A.Y));
        internal int Winding => B.Y > A.Y ? 1 : -1;
    }
    private readonly record struct Curve(Point A, Point C, Point B, int Depth);
    private readonly record struct Crossing(double X, int Index, int Order);

    private readonly struct CrossingOrder : TextRasterSort.IOrder<Crossing>
    {
        public static int Compare(in Crossing a, in Crossing b)
        { var order = a.X.CompareTo(b.X); return order != 0 ? order : a.Order.CompareTo(b.Order); }
    }
    private readonly struct EventOrder : TextRasterSort.IOrder<double>
    {
        public static int Compare(in double a, in double b) => a.CompareTo(b);
    }
    internal TextRasterizer(TextLayout layout, OperationContext context, int width)
    {
        this.context = context;
        Admit(4096); // Owner plus bounded sort/integration frame allowance.
        try
        {
            var maximum = 0;
            foreach (var p in layout.Placements)
            {
                context.Cancellation.ThrowIfCancellationRequested(); var shape = p.Shape!;
                if (shapes.ContainsKey(shape)) continue;
                Admit(512); var prepared = new RasterShape(this);
                try { prepared.BuildEdges(shape.Outline, layout.FontSize / layout.Font.UnitsPerEm); shapes.Add(shape, prepared); }
                catch { prepared.Dispose(); throw; }
                maximum = Math.Max(maximum, prepared.Count);
            }
            Admit(32 + maximum * 16L); Admit(32 + width * 8L);
            crossings = new Crossing[maximum]; row = new double[width];
            // Preparation and scan conversion do not overlap. Reuse the crossing
            // storage to order edge intervals before it holds scan-band crossings.
            foreach (var prepared in shapes.Values) prepared.BuildEvents(crossings);
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
            var eventIndex = 0; var activeCount = 0; var nextEdge = 0;
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
                    if (end > band) Band(shape, band, end, dx, dy - py, left, right, ref activeCount, ref nextEdge);
                    band = end;
                }
                if (rowEnd > band) Band(shape, band, rowEnd, dx, dy - py, left, right, ref activeCount, ref nextEdge);
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

    private void Band(RasterShape shape, double top, double bottom, double dx, double dy, int left, int right,
        ref int activeCount, ref int nextEdge)
    {
        var middle = top * .5 + bottom * .5; var count = 0;
        // Bands advance monotonically within one glyph. Keep the same active
        // set and exact X values as a full scan, without revisiting dead edges.
        for (var i = 0; i < activeCount; i++)
        {
            context.Cancellation.ThrowIfCancellationRequested(); var index = crossings[i].Index; var edge = shape.Edges[index];
            if (middle < edge.MaxY) crossings[count++] = new(edge.X(middle), index, edge.Order);
        }
        while (nextEdge < shape.Count && shape.Edges[nextEdge].MinY < middle)
        {
            context.Cancellation.ThrowIfCancellationRequested(); var index = nextEdge++; var edge = shape.Edges[index];
            if (middle < edge.MaxY) crossings[count++] = new(edge.X(middle), index, edge.Order);
        }
        activeCount = count;
        TextRasterSort.Sort<Crossing, CrossingOrder>(crossings.AsSpan(0, count), context.Cancellation);
        Span<Point> firstPolygon = stackalloc Point[8]; Span<Point> secondPolygon = stackalloc Point[8];
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
                var y0 = top + dy; var y1 = bottom + dy;
                InteriorRun(a, b, c, d, y0, y1, first, last, out var insideFirst, out var insideLast);
                var px = (int)first;
                for (; px < insideFirst; px++)
                {
                    context.Cancellation.ThrowIfCancellationRequested();
                    row[px - left] += Area(a - px, b - px, c - px, d - px, y0, y1, firstPolygon, secondPolygon);
                }
                if (px < insideLast)
                {
                    var area = InteriorArea(y0, y1);
                    for (; px < insideLast; px++)
                    {
                        context.Cancellation.ThrowIfCancellationRequested();
                        row[px - left] += area;
                    }
                }
                for (; px < last; px++)
                {
                    context.Cancellation.ThrowIfCancellationRequested();
                    row[px - left] += Area(a - px, b - px, c - px, d - px, y0, y1, firstPolygon, secondPolygon);
                }
            }
        }
        if (winding != 0) throw new ImageDataException("Unclosed text contour.");
    }

    private static void InteriorRun(double a, double b, double c, double d, double top, double bottom,
        double first, double last, out int start, out int end)
    {
        start = end = (int)first;
        if (!(double.IsFinite(a) && double.IsFinite(b) && double.IsFinite(c) && double.IsFinite(d)
            && double.IsFinite(top) && double.IsFinite(bottom))) return;
        // Strict inequalities keep equality/duplicate-vertex rounding on the
        // original clipping path. Clamp in double before converting to canvas x.
        var low = Math.Max(first, Math.Floor(Math.Max(a, d)) + 1);
        var high = Math.Min(last, Math.Ceiling(Math.Min(b, c)) - 1);
        if (low < high) { start = (int)low; end = (int)high; }
    }

    private static double InteriorArea(double top, double bottom)
    {
        // Exactly the clipped rectangle's ordered shoelace terms, including
        // signed-zero canonicalization at the horizontal intersections.
        top += 0d; bottom += 0d;
        double result = 0;
        result += 0d * top - 0d * bottom;
        result += 0d * top - top;
        result += bottom - top;
        result += bottom - 0d * bottom;
        return Math.Abs(result) * .5;
    }

    // Polygon clipping happens in pixel-local coordinates before the area sum.
    private static double Area(double a, double b, double c, double d, double top, double bottom,
        Span<Point> first, Span<Point> second)
    {
        first[0] = new(a, top); first[1] = new(b, top); first[2] = new(c, bottom); first[3] = new(d, bottom);
        var leftInside = a >= 0 && b >= 0 && c >= 0 && d >= 0;
        ReadOnlySpan<Point> polygon = first[..4];
        if (!leftInside) polygon = second[..Clip(polygon, second, 0, greater: true)];
        // A no-op clip only copies vertices in their original order. Clipping
        // left introduces x=0 vertices, which also satisfy the right boundary.
        if (!(a <= 1 && b <= 1 && c <= 1 && d <= 1))
        {
            var output = leftInside ? second : first;
            polygon = output[..Clip(polygon, output, 1, greater: false)];
        }
        if (polygon.Length < 3) return 0;
        double result = 0; var previous = polygon[^1];
        foreach (var p in polygon) { result += previous.X * p.Y - p.X * previous.Y; previous = p; }
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
        var sa = (ulong)color.A * coverage; if (sa == 0) return;
        if (canvas.BitDepth == 8) BlendByte(canvas.GetRow8(y).Slice(x * 4, 4), color, sa);
        else BlendWord(canvas.GetRow16(y).Slice(x * 4, 4), color, sa);
    }

    private static void BlendByte(Span<byte> pixel, RgbaColor color, ulong sa)
    {
        const ulong maximum = 65535, denominator = maximum * maximum;
        var remainder = denominator - sa; var da = (ulong)pixel[3];
        var alpha = sa * 255 + da * remainder;
        var divisor = maximum * alpha; var half = divisor / 2;
        var sourceWeight = sa * (255UL * 255); var backgroundWeight = da * remainder * maximum;
        // n <= 255*d; even n+floor(d/2) is at most18,337,958,949,380,194,687,
        // below UInt64.MaxValue. Keep full16-bit source color and coverage.
        pixel[0] = (byte)((color.R * sourceWeight + pixel[0] * backgroundWeight + half) / divisor);
        pixel[1] = (byte)((color.G * sourceWeight + pixel[1] * backgroundWeight + half) / divisor);
        pixel[2] = (byte)((color.B * sourceWeight + pixel[2] * backgroundWeight + half) / divisor);
        pixel[3] = (byte)((alpha + denominator / 2) / denominator);
    }

    private static void BlendWord(Span<ushort> pixel, RgbaColor color, ulong sa)
    {
        const ulong maximum = 65535, denominator = maximum * maximum;
        var remainder = denominator - sa; var da = (ulong)pixel[3];
        var alpha = sa * maximum + da * remainder;
        // Each weight and divisor is bounded by65535^4 < 2^64. Word channel
        // numerators still need UInt128; no source precision is discarded.
        var divisor = maximum * alpha; var half = divisor / 2;
        var sourceWeight = sa * denominator; var backgroundWeight = da * remainder * maximum;
        pixel[0] = (ushort)(((UInt128)color.R * sourceWeight + (UInt128)pixel[0] * backgroundWeight + half) / divisor);
        pixel[1] = (ushort)(((UInt128)color.G * sourceWeight + (UInt128)pixel[1] * backgroundWeight + half) / divisor);
        pixel[2] = (ushort)(((UInt128)color.B * sourceWeight + (UInt128)pixel[2] * backgroundWeight + half) / divisor);
        pixel[3] = (ushort)((alpha + denominator / 2) / denominator);
    }

    private static void ReorderEdges(Span<Edge> edges, Span<Crossing> order, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        for (var first = 0; first < edges.Length; first++)
        {
            cancellation.ThrowIfCancellationRequested();
            if (order[first].Index < 0) continue;
            var saved = edges[first]; var at = first;
            while (order[at].Index != first)
            {
                cancellation.ThrowIfCancellationRequested();
                var origin = order[at].Index;
                edges[at] = edges[origin];
                order[at] = order[at] with { Index = -1 };
                at = origin;
            }
            cancellation.ThrowIfCancellationRequested();
            edges[at] = saved;
            order[at] = order[at] with { Index = -1 };
        }
    }

    private sealed class RasterShape(TextRasterizer owner) : IDisposable
    {
        internal Edge[] Edges = [];
        internal double[] Events = [];
        internal int Count, EventCount;
        internal double MinY, MaxY;
        private long edgeBytes, eventBytes;

        internal void BuildEdges(FontOutline outline, double scale)
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
        }
        internal void BuildEvents(Crossing[] order)
        {
            for (var i = 0; i < Count; i++)
            {
                owner.context.Cancellation.ThrowIfCancellationRequested();
                Event(Edges[i].MinY); Event(Edges[i].MaxY);
                order[i] = new(Edges[i].MinY, i, i);
            }
            TextRasterSort.Sort<Crossing, CrossingOrder>(order.AsSpan(0, Count), owner.context.Cancellation);
            for (var i = 0; i < Count; i++)
            {
                owner.context.Cancellation.ThrowIfCancellationRequested();
                var index = order[i].Index; var maximum = Edges[index].MaxY;
                for (var j = i + 1; j < Count && order[j].X < maximum; j++)
                {
                    owner.context.Cancellation.ThrowIfCancellationRequested();
                    // Preserve original edge-index orientation and arithmetic:
                    // sorting changes which pairs are visited, not their results.
                    var other = order[j].Index;
                    var a = Edges[Math.Min(index, other)]; var b = Edges[Math.Max(index, other)];
                    var low = Math.Max(a.MinY, b.MinY); var high = Math.Min(a.MaxY, b.MaxY);
                    var d0 = a.X(low) - b.X(low); var d1 = a.X(high) - b.X(high);
                    if (d0 < 0 && d1 > 0 || d0 > 0 && d1 < 0)
                    {
                        var y = low + (high - low) * (d0 / (d0 - d1));
                        if (low < y && y < high) Event(y);
                    }
                }
            }
            TextRasterSort.Sort<double, EventOrder>(Events.AsSpan(0, EventCount), owner.context.Cancellation);
            var unique = 0;
            for (var i = 0; i < EventCount; i++)
            { owner.context.Cancellation.ThrowIfCancellationRequested(); if (unique == 0 || Events[i] != Events[unique - 1]) Events[unique++] = Events[i]; }
            EventCount = unique;
            if (unique != 0) { MinY = Events[0]; MaxY = Events[unique - 1]; }
            // The crossing descriptors already give this same MinY/original-
            // ordinal order. Their indexes retire after event preparation.
            ReorderEdges(Edges.AsSpan(0, Count), order.AsSpan(0, Count), owner.context.Cancellation);
        }
        private void Add(Point a, Point b)
        {
            if (a.Y == b.Y) return;
            Grow(ref Edges, Count, 40, ref edgeBytes); Edges[Count] = new(a, b, Count); Count++;
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
