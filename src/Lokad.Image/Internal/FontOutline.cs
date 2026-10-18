namespace Lokad.Image.Internal;

internal readonly record struct FontPoint(double X, double Y, bool OnCurve = true);
internal readonly record struct FontSegment(FontPoint Start, FontPoint Control, FontPoint End, bool Quadratic);

// Operation-owned geometry. No global glyph cache or caller-visible backing arrays.
internal sealed class FontOutline : IDisposable
{
    internal const int DescriptorBytes = 512; // Object plus bounded parser-frame allowance.
    private readonly OperationContext context;
    private FontPoint[] points = [];
    private int[] ends = [];
    private bool disposed;
    internal int PointCount, ContourCount;
    internal FontPoint Left, Right, Top, Bottom;
    internal double Advance => Right.X - Left.X;
    internal long StorageBytes => DescriptorBytes + PointBytes(points.Length) + EndBytes(ends.Length);
    internal ReadOnlySpan<FontPoint> Points => points.AsSpan(0, PointCount);
    internal ReadOnlySpan<int> Ends => ends.AsSpan(0, ContourCount);

    internal FontOutline(OperationContext context) { this.context = context; context.Reserve(DescriptorBytes); }
    private static long PointBytes(int count) => count == 0 ? 0 : 32 + count * 24L;
    private static long EndBytes(int count) => count == 0 ? 0 : 32 + count * 4L;

    internal void SizeSimple(int pointCount, int contourCount)
    {
        Ensure(pointCount, contourCount, exact: true); PointCount = pointCount; ContourCount = contourCount;
    }
    internal void SetPoint(int index, FontPoint value) => points[index] = value;
    internal void SetEnd(int index, int value) => ends[index] = value;
    internal FontPoint Point(int index) => points[index];

    private void Ensure(int pointCount, int contourCount, bool exact = false)
    {
        if (pointCount > context.Limits.MaxContainerItems || contourCount > context.Limits.MaxContainerItems)
            throw new ImageLimitException(ImageLimitKind.ContainerItems, "Glyph point or contour limit exceeded.");
        if (pointCount > points.Length)
        {
            var capacity = exact ? pointCount : (int)Math.Min(context.Limits.MaxContainerItems,
                Math.Max(pointCount, Math.Max(4, points.Length * 2L)));
            var bytes = PointBytes(capacity);
            if (bytes > Array.MaxLength) throw new ImageLimitException(ImageLimitKind.BufferLength, "Glyph point storage exceeds the managed limit.");
            context.Reserve(bytes);
            FontPoint[] next;
            try { context.Work(PointCount); next = new FontPoint[capacity]; points.AsSpan(0, PointCount).CopyTo(next); }
            catch { context.Release(bytes); throw; }
            context.Release(PointBytes(points.Length)); points = next;
        }
        if (contourCount > ends.Length)
        {
            var capacity = exact ? contourCount : (int)Math.Min(context.Limits.MaxContainerItems,
                Math.Max(contourCount, Math.Max(2, ends.Length * 2L)));
            var bytes = EndBytes(capacity);
            if (bytes > Array.MaxLength) throw new ImageLimitException(ImageLimitKind.BufferLength, "Glyph contour storage exceeds the managed limit.");
            context.Reserve(bytes);
            int[] next;
            try { context.Work(ContourCount); next = new int[capacity]; ends.AsSpan(0, ContourCount).CopyTo(next); }
            catch { context.Release(bytes); throw; }
            context.Release(EndBytes(ends.Length)); ends = next;
        }
    }

    internal void Append(FontOutline child, double xx, double yx, double xy, double yy, double dx, double dy)
    {
        var count = (long)PointCount + child.PointCount; var contours = (long)ContourCount + child.ContourCount;
        if (count > context.Limits.MaxContainerItems || contours > context.Limits.MaxContainerItems)
            throw new ImageLimitException(ImageLimitKind.ContainerItems, "Composite glyph point or contour limit exceeded.");
        Ensure((int)count, (int)contours);
        for (var i = 0; i < child.PointCount; i++)
        {
            context.Work(1); points[PointCount + i] = Transform(child.points[i], xx, yx, xy, yy, dx, dy);
        }
        for (var i = 0; i < child.ContourCount; i++)
        { context.Work(1); ends[ContourCount + i] = PointCount + child.ends[i]; }
        PointCount = (int)count; ContourCount = (int)contours;
    }
    internal static FontPoint Transform(FontPoint p, double xx, double yx, double xy, double yy, double dx = 0, double dy = 0)
    {
        var x = p.X * xx + p.Y * xy + dx; var y = p.X * yx + p.Y * yy + dy;
        if (!double.IsFinite(x) || !double.IsFinite(y)) throw new ImageDataException("Nonfinite glyph coordinates.");
        return new(x, y, p.OnCurve);
    }
    internal FontPoint Attachment(int index)
    {
        if ((uint)index < PointCount) return points[index];
        return (index - PointCount) switch
        { 0 => Left, 1 => Right, 2 => Top, 3 => Bottom, _ => throw new ImageDataException("Invalid glyph attachment point.") };
    }

    internal FontSegmentEnumerator Segments() => new(Points, Ends);
    internal (double XMin, double YMin, double XMax, double YMax) TightBounds(OperationContext context)
    {
        if (PointCount == 0) return default;
        var minX = double.PositiveInfinity; var minY = double.PositiveInfinity;
        var maxX = double.NegativeInfinity; var maxY = double.NegativeInfinity;
        void Include(double x, double y)
        { minX = Math.Min(minX, x); minY = Math.Min(minY, y); maxX = Math.Max(maxX, x); maxY = Math.Max(maxY, y); }
        var segments = Segments();
        while (segments.Next(out var segment))
        {
            context.Work(1); Include(segment.Start.X, segment.Start.Y); Include(segment.End.X, segment.End.Y);
            if (!segment.Quadratic) continue;
            for (var axis = 0; axis < 2; axis++)
            {
                var a = axis == 0 ? segment.Start.X : segment.Start.Y;
                var b = axis == 0 ? segment.Control.X : segment.Control.Y;
                var c = axis == 0 ? segment.End.X : segment.End.Y;
                var denominator = a - 2 * b + c;
                if (denominator == 0) continue;
                var t = (a - b) / denominator;
                if (t > 0 && t < 1)
                {
                    var u = 1 - t;
                    Include(u * (u * segment.Start.X + t * segment.Control.X) + t * (u * segment.Control.X + t * segment.End.X),
                        u * (u * segment.Start.Y + t * segment.Control.Y) + t * (u * segment.Control.Y + t * segment.End.Y));
                }
            }
        }
        return (minX, minY, maxX, maxY);
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true; context.Release(StorageBytes); points = []; ends = []; PointCount = ContourCount = 0;
    }
}

// TrueType implicit on-curve points are generated during traversal, preserving
// original point numbering for composite attachment. No expanded path copy.
internal ref struct FontSegmentEnumerator
{
    private readonly ReadOnlySpan<FontPoint> points;
    private readonly ReadOnlySpan<int> ends;
    private int contour, first, index, end;
    private FontPoint start, current;
    private bool active;
    internal FontSegmentEnumerator(ReadOnlySpan<FontPoint> points, ReadOnlySpan<int> ends)
    { this.points = points; this.ends = ends; contour = first = index = 0; end = -1; start = current = default; active = false; }
    internal bool Next(out FontSegment segment)
    {
        if (!active)
        {
            if (contour == ends.Length) { segment = default; return false; }
            end = ends[contour++]; var a = points[first]; var z = points[end];
            start = a.OnCurve ? a : z.OnCurve ? z : Mid(z, a); current = start;
            index = a.OnCurve ? first + 1 : first; active = true;
        }
        if (index <= end)
        {
            var p = points[index++];
            if (p.OnCurve) { segment = new(current, default, p, false); current = p; return true; }
            var next = index <= end ? points[index] : start;
            var target = next.OnCurve ? next : Mid(p, next);
            if (next.OnCurve) index++;
            segment = new(current, p, target, true); current = target; return true;
        }
        segment = new(current, default, start, false); first = end + 1; active = false; return true;
    }
    private static FontPoint Mid(FontPoint a, FontPoint b) => new(a.X * .5 + b.X * .5, a.Y * .5 + b.Y * .5);
}
