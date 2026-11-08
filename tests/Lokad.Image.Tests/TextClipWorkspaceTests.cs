namespace Lokad.Image.Tests
{
    public sealed class TextClipWorkspaceTests
    {
        [Fact]
        public void ReusedWorkspaceMatchesAlwaysClippedAreaExactly() => Internal.TextRasterizer.VerifyClipWorkspace(false);

        [Fact]
        public void DegenerateAndNonfiniteCoordinatesPreserveClippingSemantics() => Internal.TextRasterizer.VerifyClipWorkspace(true);
    }
}

namespace Lokad.Image.Internal
{
    internal sealed partial class TextRasterizer
    {
        internal static void VerifyClipWorkspace(bool special)
        {
            Span<Point> first = stackalloc Point[10]; Span<Point> second = stackalloc Point[10];
            var canary = new Point(12345, -98765); first.Fill(canary); second.Fill(canary);
            double[] values = special
                ? [double.NaN, double.NegativeInfinity, double.PositiveInfinity, -double.MaxValue, double.MaxValue, -0.0, 0, 1]
                : [-2, -.5, -double.Epsilon, -0.0, 0, double.Epsilon, .25, .75, 1 - 1e-12, 1, 1 + 1e-12, 2];
            foreach (var a in values)
            foreach (var b in values)
            foreach (var c in values)
            foreach (var d in values)
            {
                Check(a, b, c, d, special ? a : -.125, special ? d : .875, first[1..9], second[1..9]);
                Assert.Equal(canary, first[0]); Assert.Equal(canary, first[9]);
                Assert.Equal(canary, second[0]); Assert.Equal(canary, second[9]);
            }
            if (special) return;
            var random = new Random(587);
            for (var i = 0; i < 10000; i++)
                Check(random.NextDouble() * 8 - 4, random.NextDouble() * 8 - 4,
                    random.NextDouble() * 8 - 4, random.NextDouble() * 8 - 4,
                    random.NextDouble() - .5, random.NextDouble() + .5, first[1..9], second[1..9]);
            Assert.Equal(canary, first[0]); Assert.Equal(canary, first[9]);
            Assert.Equal(canary, second[0]); Assert.Equal(canary, second[9]);
        }

        private static void Check(double a, double b, double c, double d, double top, double bottom,
            Span<Point> first, Span<Point> second)
        {
            Span<Point> referenceFirst = stackalloc Point[8]; Span<Point> referenceSecond = stackalloc Point[8];
            referenceFirst[0] = new(a, top); referenceFirst[1] = new(b, top);
            referenceFirst[2] = new(c, bottom); referenceFirst[3] = new(d, bottom);
            var count = Clip(referenceFirst[..4], referenceSecond, 0, greater: true);
            count = Clip(referenceSecond[..count], referenceFirst, 1, greater: false);
            double expected = 0;
            if (count >= 3)
            {
                var previous = referenceFirst[count - 1];
                for (var i = 0; i < count; i++)
                { var p = referenceFirst[i]; expected += previous.X * p.Y - p.X * previous.Y; previous = p; }
                expected = Math.Abs(expected) * .5;
            }
            var actual = Area(a, b, c, d, top, bottom, first, second);
            if (double.IsNaN(expected)) Assert.True(double.IsNaN(actual));
            else Assert.Equal(BitConverter.DoubleToInt64Bits(expected), BitConverter.DoubleToInt64Bits(actual));
        }
    }
}
