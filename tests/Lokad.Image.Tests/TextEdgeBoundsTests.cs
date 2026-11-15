namespace Lokad.Image.Tests
{
    public sealed class TextEdgeBoundsTests
    {
        [Fact]
        public void ComputedXRangesContainActualCoordinatesAndExcludeNoCrossings()
            => Internal.TextRasterizer.VerifyComputedXRanges();
    }
}

namespace Lokad.Image.Internal
{
    internal sealed partial class TextRasterizer
    {
        internal static void VerifyComputedXRanges()
        {
            // For finite endpoint Y values and finite nonzero rounded deltaY,
            // rounded (y-originY) is between zero and that same deltaY whenever
            // y is between the original endpoints. Division therefore gives
            // a fraction in [0,1]. Rounded multiplication and addition are
            // monotone, so X(y) lies between originX and (originX+deltaX).
            // The latter is a computed endpoint, not the original endpoint X.
            // Degenerate/nonfinite inputs cannot use this certificate.
            static (double Low, double High) Envelope(Edge edge)
            {
                if (!(double.IsFinite(edge.MinY) && double.IsFinite(edge.MaxY)
                    && double.IsFinite(edge.deltaY) && edge.deltaY != 0
                    && double.IsFinite(edge.originX) && double.IsFinite(edge.deltaX)))
                    return (double.NegativeInfinity, double.PositiveInfinity);
                var end = edge.originX + edge.deltaX;
                return (Math.Min(edge.originX, end), Math.Max(edge.originX, end));
            }

            var checkedEdges = 0; var excludedPairs = 0;
            void CheckEdge(Edge edge)
            {
                var (low, high) = Envelope(edge);
                if (double.IsNegativeInfinity(low) && double.IsPositiveInfinity(high)) return;
                checkedEdges++;
                double[] positions = [edge.MinY, edge.MaxY, Math.BitIncrement(edge.MinY),
                    Math.BitDecrement(edge.MaxY), edge.MinY * .5 + edge.MaxY * .5,
                    edge.MinY * .25 + edge.MaxY * .75, edge.MinY * .75 + edge.MaxY * .25];
                foreach (var position in positions)
                {
                    var y = Math.Clamp(position, edge.MinY, edge.MaxY);
                    var actual = edge.X(y);
                    Assert.True(low <= actual && actual <= high);
                    var pair = edge.Endpoints(y, y);
                    Assert.True(low <= pair.Top && pair.Top <= high);
                    Assert.True(low <= pair.Bottom && pair.Bottom <= high);
                }
            }
            void CheckPair(Edge a, Edge b)
            {
                CheckEdge(a); CheckEdge(b);
                var first = Envelope(a); var second = Envelope(b);
                if (!(first.High < second.Low || second.High < first.Low)) return;
                var low = Math.Max(a.MinY, b.MinY); var high = Math.Min(a.MaxY, b.MaxY);
                if (!(low < high)) return;
                excludedPairs++;
                // This is the actual original crossing decision, including
                // subtraction overflow and the strict inequalities at vertices.
                var d0 = a.X(low) - b.X(low); var d1 = a.X(high) - b.X(high);
                Assert.False(d0 < 0 && d1 > 0 || d0 > 0 && d1 < 0);
            }

            double[] limits = [0d, -0d, double.Epsilon, -double.Epsilon,
                Math.ScaleB(1d, -1022), -Math.ScaleB(1d, -1022),
                Math.BitDecrement(1d), 1d, Math.BitIncrement(1d), -1d,
                -32768d, 32767d, Math.ScaleB(1d, 53), -Math.ScaleB(1d, 53),
                double.MaxValue, -double.MaxValue, double.PositiveInfinity, double.NegativeInfinity, double.NaN];
            for (var i = 0; i < limits.Length; i++)
            for (var j = 0; j < limits.Length; j++)
                CheckPair(new Edge(new(limits[i], limits[j]), new(limits[j], limits[(i + 1) % limits.Length]), 0),
                    new Edge(new(limits[(j + 2) % limits.Length], limits[(i + 3) % limits.Length]),
                        new(limits[(i + 4) % limits.Length], limits[(j + 5) % limits.Length]), 1));
            var random = new Random(0x1100);
            double Bits() => BitConverter.Int64BitsToDouble(random.NextInt64() | (random.Next(2) == 0 ? 0 : long.MinValue));
            double FontCoordinate() => Math.ScaleB(random.NextDouble() * 65536 - 32768, random.Next(-30, 31));
            for (var i = 0; i < 20000; i++)
            {
                CheckPair(new Edge(new(FontCoordinate(), FontCoordinate()), new(FontCoordinate(), FontCoordinate()), 0),
                    new Edge(new(FontCoordinate(), FontCoordinate()), new(FontCoordinate(), FontCoordinate()), 1));
                CheckPair(new Edge(new(Bits(), Bits()), new(Bits(), Bits()), 0),
                    new Edge(new(Bits(), Bits()), new(Bits(), Bits()), 1));
            }
            Assert.True(checkedEdges > 60000); Assert.True(excludedPairs > 1000);
        }
    }
}
