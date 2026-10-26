using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class FontOutlineTests
{
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
