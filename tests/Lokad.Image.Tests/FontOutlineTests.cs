using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class FontOutlineTests
{
    private delegate void HeaderReader(ReadOnlySpan<byte> bytes);

    [Fact]
    public void SuppliedFontHeadersPreserveSignedBoundsAndTruncation()
    {
        // Exercise the actual eager validator, including signed coordinates,
        // empty/composite headers, unaligned reads and poisoned neighboring bytes.
        var read = typeof(TrueTypeFont).GetMethod("Header", System.Reflection.BindingFlags.Static
            | System.Reflection.BindingFlags.NonPublic)!.CreateDelegate<HeaderReader>();
        var ordinal = 0;
        void Check(short contours, short xmin, short ymin, short xmax, short ymax)
        {
            var bytes = new byte[24]; Array.Fill(bytes, (byte)0xa5); var offset = ordinal++ % 8;
            var header = bytes.AsSpan(offset, 10);
            System.Buffers.Binary.BinaryPrimitives.WriteInt16BigEndian(header, contours);
            System.Buffers.Binary.BinaryPrimitives.WriteInt16BigEndian(header[2..], xmin);
            System.Buffers.Binary.BinaryPrimitives.WriteInt16BigEndian(header[4..], ymin);
            System.Buffers.Binary.BinaryPrimitives.WriteInt16BigEndian(header[6..], xmax);
            System.Buffers.Binary.BinaryPrimitives.WriteInt16BigEndian(header[8..], ymax);
            var unchanged = bytes.ToArray();
            if (contours == 0 || xmin <= xmax && ymin <= ymax) read(bytes.AsSpan(offset, 10));
            else Assert.Throws<ImageDataException>(() => read(bytes.AsSpan(offset, 10)));
            Assert.Equal(unchanged, bytes);
        }
        short[] limits = [short.MinValue, short.MinValue + 1, -1, 0, 1, short.MaxValue - 1, short.MaxValue];
        foreach (var xmin in limits) foreach (var ymin in limits)
            foreach (var xmax in limits) foreach (var ymax in limits)
            {
                Check(1, xmin, ymin, xmax, ymax);
                Check(-1, xmin, ymin, xmax, ymax);
            }
        var random = new Random(0x1114);
        short Coordinate() => (short)random.Next(short.MinValue, short.MaxValue + 1);
        for (var i = 0; i < 4096; i++)
            Check(i % 3 == 0 ? (short)0 : short.MinValue, Coordinate(), Coordinate(), Coordinate(), Coordinate());
        var truncated = new byte[10];
        for (var length = 0; length < 10; length++)
        {
            var count = length;
            Assert.Throws<ImageDataException>(() => read(truncated.AsSpan(0, count)));
        }
    }

    [Fact]
    public void OnCurveAndOffCurveEndpointsProduceClosedQuadraticSegments()
    {
        var triangle = Segments([new(0, 0), new(100, 0), new(100, 100)]);
        Assert.Equal(3, triangle.Length); Assert.All(triangle, s => Assert.False(s.Quadratic));
        Assert.Equal(new FontPoint(0, 0), triangle[^1].End);
        var firstOff = Segments([new(0, 200, false), new(200, 0), new(0, 0)]);
        Assert.Equal(new FontSegment(new(0, 0), new(0, 200, false), new(200, 0), true), firstOff[0]);
        var lastOff = Segments([new(0, 0), new(200, 0), new(0, 200, false)]);
        Assert.Equal(new FontSegment(new(200, 0), new(0, 200, false), new(0, 0), true), lastOff[1]);
    }

    [Fact]
    public void ConsecutiveOffCurvePointsCreateImplicitMidpointsWithoutRenumbering()
    {
        var segments = Segments([new(0, 0, false), new(0, 200, false), new(200, 200, false), new(200, 0, false)]);
        Assert.Equal(5, segments.Length); Assert.Equal(4, segments.Count(s => s.Quadratic));
        Assert.Equal(new FontPoint(100, 0), segments[0].Start); Assert.Equal(new FontPoint(0, 100), segments[0].End);
        Assert.Equal(new FontPoint(100, 200), segments[1].End); Assert.Equal(new FontPoint(200, 100), segments[2].End);
        Assert.Equal(segments[0].Start, segments[3].End);
    }

    [Fact]
    public void SimpleOutlinePeakAndRetainedCapacityAreAdmittedAndReleasedExactly()
    {
        var font = TrueTypeFont.Load(AuthoredTrueType.Build());
        // 256-byte path, 512 descriptor/frame, 3*24+32 points,
        // 1*4+32 endpoint and 3+32 temporary flags.
        var limits = new ImageLimits { MaxWorkingBytes = font.StorageBytes + 943 };
        var context = new OperationContext(limits, default); context.Reserve(font.StorageBytes);
        var outline = font.OutlineForText(2, context);
        Assert.Equal(3, outline.PointCount); Assert.Equal(1, outline.ContourCount);
        Assert.Equal(652, outline.StorageBytes); Assert.Equal(291, context.RemainingWorkingBytes);
        Assert.Equal(new FontPoint(300, 700), outline.Points[1]); Assert.Equal(2, outline.Ends[0]);
        outline.Dispose(); outline.Dispose(); Assert.Equal(943, context.RemainingWorkingBytes);
        var denied = new OperationContext(limits with { MaxWorkingBytes = limits.MaxWorkingBytes - 1 }, default); denied.Reserve(font.StorageBytes);
        var failure = Assert.Throws<ImageLimitException>(() => font.OutlineForText(2, denied));
        Assert.Equal(ImageLimitKind.WorkingBytes, failure.Limit); Assert.Equal(942, denied.RemainingWorkingBytes);
    }

    private static FontSegment[] Segments(FontPoint[] points)
    {
        var enumerator = new FontSegmentEnumerator(points, [points.Length - 1]); var result = new List<FontSegment>();
        while (enumerator.Next(out var segment)) { result.Add(segment); Assert.True(result.Count <= 16); }
        return result.ToArray();
    }
}
