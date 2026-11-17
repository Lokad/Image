using System.Runtime.InteropServices;
using Lokad.Image.Tooling;
namespace Lokad.Image.Tests
{
    public sealed class TextInitialEdgeStorageTests
    {
        [Theory]
        [InlineData(0)] [InlineData(2)] [InlineData(6)]
        [InlineData(63)] [InlineData(64)] [InlineData(65)]
        [InlineData(128)] [InlineData(129)] [InlineData(257)]
        public void InitialPrefixAndEverySpillPreserveEdgesAndOwnership(int count)
            => Internal.TextRasterizer.VerifyInitialEdgeStorage(count);
    }
}
namespace Lokad.Image.Internal
{
    internal sealed partial class TextRasterizer
    {
        internal static void VerifyInitialEdgeStorage(int count)
        {
            Span<Edge> sizeProbe = stackalloc Edge[64];
            Assert.Equal(3584, MemoryMarshal.AsBytes(sizeProbe).Length);
            using var fixture = new OperationContext(ImageLimits.Default, default);
            using var outline = new FontOutline(fixture);
            outline.SizeSimple(count, count == 0 ? 0 : 1);
            outline.Left = new(2.5, 0);
            for (var i = 0; i < count; i++) outline.SetPoint(i, new(i % 7 - 3, i));
            if (count != 0) outline.SetEnd(0, count - 1);
            var before = outline.Points.ToArray();
            var expected = new List<Edge>(); var segments = outline.Segments();
            while (segments.Next(out var segment))
            {
                Assert.False(segment.Quadratic);
                var a = new Point((segment.Start.X - outline.Left.X) * .375, -segment.Start.Y * .375);
                var b = new Point((segment.End.X - outline.Left.X) * .375, -segment.End.Y * .375);
                if (a.Y != b.Y) expected.Add(new(a, b, expected.Count));
            }
            using var font = TrueTypeFont.Load(AuthoredTrueType.Build());
            using var layout = TextLayout.Create(font, "A", new() { FontSize = 10 });
            using var root = new TestMemoryBudget(16 * 1024 * 1024);
            using (var context = new OperationContext(new() { MemoryBudget = root }, default))
            using (var owner = new TextRasterizer(layout, context, 80))
            {
                var held = root.Bytes;
                using (var shape = new RasterShape(owner))
                {
                    shape.BuildEdges(outline, .375);
                    Assert.Equal(expected.Count, shape.Count);
                    Assert.Equal(expected, shape.Edges.AsSpan(0, shape.Count).ToArray());
                    var capacity = count <= 64 ? count : 128;
                    while (capacity < count) capacity *= 2;
                    Assert.Equal(capacity, shape.Edges.Length);
                    Assert.Equal(held + (count == 0 ? 0 : 32 + capacity * 56L), root.Bytes);
                    Assert.Equal(before, outline.Points.ToArray());
                }
                Assert.Equal(held, root.Bytes);
            }
            Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        }
    }
}
