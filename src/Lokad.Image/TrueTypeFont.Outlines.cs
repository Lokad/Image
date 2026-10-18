using Lokad.Image.Internal;

namespace Lokad.Image;

public sealed partial class TrueTypeFont
{
    internal int MapForText(int scalar, OperationContext context) => Map(scalar, context);

    internal FontOutline OutlineForText(int glyphIndex, OperationContext context)
    {
        const int maximumDepth = 64;
        context.Reserve(maximumDepth * 4);
        try
        {
            Span<int> path = stackalloc int[maximumDepth]; long visits = 0;
            return ReadOutline(glyphIndex, context, path, 0, ref visits);
        }
        finally { context.Release(maximumDepth * 4); }
    }

    private FontOutline ReadOutline(int glyphIndex, OperationContext context, Span<int> path, int depth, ref long visits)
    {
        context.Work(1);
        if ((uint)glyphIndex >= GlyphCount) throw new ImageDataException("Composite references an absent glyph.");
        if (++visits > context.Limits.MaxContainerItems) throw new ImageLimitException(ImageLimitKind.ContainerItems, "Glyph component limit exceeded.");
        for (var i = 0; i < depth; i++)
        { context.Work(1); if (path[i] == glyphIndex) throw new ImageDataException("Cyclic composite glyph."); }
        if (depth == path.Length) throw new UnsupportedImageException(UnsupportedImageFeature.FontOutline, "Composite nesting beyond 64 glyphs is unsupported.");
        path[depth] = glyphIndex;
        var result = new FontOutline(context);
        try
        {
            var metrics = Metrics(glyphIndex, context); var glyph = Glyph(glyphIndex);
            result.Left = new(metrics.XMin - metrics.LeftSideBearing, 0); result.Right = new(result.Left.X + metrics.AdvanceWidth, 0);
            VerticalPhantoms(glyphIndex, glyph, context, out result.Top, out result.Bottom);
            if (glyph.Length == 0) return result;
            var cursor = new OutlineCursor(glyph[10..], context); var count = I16(glyph);
            if (count >= 0) ReadSimple(result, count, ref cursor, context);
            else
            {
                var instructions = false; var components = 0; ushort flags;
                do
                {
                    flags = cursor.U16(); var childIndex = cursor.U16();
                    var words = (flags & 1) != 0; var xyArguments = (flags & 2) != 0;
                    int a, b;
                    if (words) { a = xyArguments ? cursor.I16() : cursor.U16(); b = xyArguments ? cursor.I16() : cursor.U16(); }
                    else { a = xyArguments ? (sbyte)cursor.Byte() : cursor.Byte(); b = xyArguments ? (sbyte)cursor.Byte() : cursor.Byte(); }
                    var transforms = flags & (8 | 64 | 128);
                    if (transforms != 0 && (transforms & (transforms - 1)) != 0 || (flags & 0x1800) == 0x1800)
                        throw new ImageDataException("Conflicting composite transforms or offset flags.");
                    double xx = 1, yx = 0, xy = 0, yy = 1;
                    if ((flags & 8) != 0) xx = yy = cursor.I16() / 16384.0;
                    else if ((flags & 64) != 0) { xx = cursor.I16() / 16384.0; yy = cursor.I16() / 16384.0; }
                    else if ((flags & 128) != 0)
                    { xx = cursor.I16() / 16384.0; yx = cursor.I16() / 16384.0; xy = cursor.I16() / 16384.0; yy = cursor.I16() / 16384.0; }
                    using var child = ReadOutline(childIndex, context, path, depth + 1, ref visits);
                    double dx, dy;
                    if (xyArguments)
                    {
                        dx = a; dy = b;
                        if ((flags & 0x800) != 0)
                        { var shifted = FontOutline.Transform(new(a, b), xx, yx, xy, yy); dx = shifted.X; dy = shifted.Y; }
                        // Unhinted design outlines: no device-grid fitting of offsets.
                    }
                    else
                    {
                        if (components == 0) throw new ImageDataException("The first component requires an XY offset.");
                        var parentPoint = result.Attachment(a);
                        var childPoint = FontOutline.Transform(child.Attachment(b), xx, yx, xy, yy);
                        dx = parentPoint.X - childPoint.X; dy = parentPoint.Y - childPoint.Y;
                    }
                    result.Append(child, xx, yx, xy, yy, dx, dy); components++;
                    if ((flags & 512) != 0)
                    {
                        // USE_MY_METRICS inherits the child's phantom metrics;
                        // the component transform/offset applies to its outline.
                        result.Left = child.Left; result.Right = child.Right;
                        result.Top = child.Top; result.Bottom = child.Bottom;
                    }
                    instructions |= (flags & 256) != 0;
                } while ((flags & 32) != 0);
                if (instructions) cursor.Skip(cursor.U16());
            }
            cursor.Skip(cursor.Remaining); return result;
        }
        catch { result.Dispose(); throw; }
    }

    private static void ReadSimple(FontOutline result, int contours, ref OutlineCursor cursor, OperationContext context)
    {
        if (contours == 0) { if (cursor.Remaining >= 2) cursor.Skip(cursor.U16()); return; }
        Items(contours, context);
        // Peek the final endpoint only after admitting/validating the whole array.
        var endpointBytes = cursor.Read(contours * 2); var previous = -1;
        for (var i = 0; i < contours; i++)
        { var end = U16(endpointBytes[(i * 2)..]); if (end <= previous) throw new ImageDataException("Invalid glyph contour endpoints."); previous = end; }
        var count = previous + 1; result.SizeSimple(count, contours);
        for (var i = 0; i < contours; i++) result.SetEnd(i, U16(endpointBytes[(i * 2)..]));
        cursor.Skip(cursor.U16());
        var flagBytes = count + 32L; context.Reserve(flagBytes);
        try
        {
            var flags = new byte[count]; var index = 0;
            while (index < count)
            {
                var flag = cursor.Byte(); var repeat = (flag & 8) != 0 ? cursor.Byte() : 0;
                if (repeat >= count - index) throw new ImageDataException("Glyph flag repeat exceeds its point count.");
                context.Work(repeat + 1); flags.AsSpan(index, repeat + 1).Fill(flag); index += repeat + 1;
            }
            var x = 0; var y = 0;
            for (var i = 0; i < count; i++)
            {
                context.Work(1); x += Coordinate(flags[i], 2, 16, ref cursor);
                if (x is < short.MinValue or > short.MaxValue) throw new ImageDataException("Glyph X coordinate exceeds the design grid.");
                result.SetPoint(i, new(x, 0, (flags[i] & 1) != 0));
            }
            for (var i = 0; i < count; i++)
            {
                context.Work(1); y += Coordinate(flags[i], 4, 32, ref cursor);
                if (y is < short.MinValue or > short.MaxValue) throw new ImageDataException("Glyph Y coordinate exceeds the design grid.");
                result.SetPoint(i, result.Point(i) with { Y = y });
            }
        }
        finally { context.Release(flagBytes); }
    }
    private static int Coordinate(int flag, int shortBit, int sameBit, ref OutlineCursor cursor)
        => (flag & shortBit) != 0 ? cursor.Byte() * ((flag & sameBit) != 0 ? 1 : -1)
            : (flag & sameBit) != 0 ? 0 : cursor.I16();

    private void VerticalPhantoms(int glyphIndex, ReadOnlySpan<byte> glyph, OperationContext context, out FontPoint top, out FontPoint bottom)
    {
        top = new(0, Ascender); bottom = new(0, Descender);
        var header = Find("vhea"u8, context); var metrics = Find("vmtx"u8, context);
        if (header is null && metrics is null) return;
        if (header is null || metrics is null) throw new ImageDataException("Incomplete vertical font metrics.");
        var h = Span(header.Value); var m = Span(metrics.Value);
        if (h.Length < 36) throw new ImageDataException("Truncated vertical font metrics.");
        if (U16(h) != 1 || I16(h[32..]) != 0) throw new UnsupportedImageException(UnsupportedImageFeature.FontTable, "Unsupported vertical metrics version.");
        var count = U16(h[34..]);
        if (count == 0 || count > GlyphCount || count * 4L + (GlyphCount - count) * 2L > m.Length)
            throw new ImageDataException("Invalid vertical font metrics.");
        var advance = U16(m[(Math.Min(glyphIndex, count - 1) * 4)..]);
        var bearing = I16(m[(glyphIndex < count ? glyphIndex * 4 + 2 : count * 4 + (glyphIndex - count) * 2)..]);
        top = new(0, (glyph.Length == 0 ? 0 : I16(glyph[8..])) + bearing); bottom = new(0, top.Y - advance);
    }

    private ref struct OutlineCursor(ReadOnlySpan<byte> bytes, OperationContext context)
    {
        private ReadOnlySpan<byte> rest = bytes;
        internal int Remaining => rest.Length;
        internal ReadOnlySpan<byte> Read(int length)
        {
            if (length < 0 || length > rest.Length) throw new ImageDataException("Truncated TrueType outline.");
            context.Work(length); var value = rest[..length]; rest = rest[length..]; return value;
        }
        internal byte Byte() => Read(1)[0];
        internal ushort U16() => TrueTypeFont.U16(Read(2));
        internal short I16() => TrueTypeFont.I16(Read(2));
        internal void Skip(int count) => Read(count);
    }
}
