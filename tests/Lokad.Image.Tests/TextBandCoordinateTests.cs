namespace Lokad.Image.Tests
{
    public sealed class TextBandCoordinateTests
    {
        [Fact]
        public void PreparedBandCoordinatesPreserveEachOriginalBit()
            => Internal.TextRasterizer.VerifyBandCoordinates();
    }
}

namespace Lokad.Image.Internal
{
    internal sealed partial class TextRasterizer
    {
        internal static void VerifyBandCoordinates()
        {
            // The scalar coordinate also orders production crossings/events.
            // Trapezoid endpoints must preserve that coordinate at both lanes,
            // including signed zero, subnormal arithmetic and overflow.
            static void Equal(double expected, double actual)
            {
                if (double.IsNaN(expected)) Assert.True(double.IsNaN(actual));
                else Assert.Equal(BitConverter.DoubleToInt64Bits(expected), BitConverter.DoubleToInt64Bits(actual));
            }
            static void Check(Point a, Point b, double top, double bottom)
            {
                double Original(double y) => a.X + (b.X - a.X) * ((y - a.Y) / (b.Y - a.Y));
                var edge = new Edge(a, b, 0);
                Equal(Math.Min(a.Y, b.Y), edge.MinY); Equal(Math.Max(a.Y, b.Y), edge.MaxY);
                Assert.Equal(b.Y > a.Y ? 1 : -1, edge.Winding);
                Equal(Original(top), edge.X(top)); Equal(Original(bottom), edge.X(bottom));
                var pair = edge.Endpoints(top, bottom);
                Equal(Original(top), pair.Top); Equal(Original(bottom), pair.Bottom);
                var middle = top * .5 + bottom * .5;
                var triple = edge.Positions(top, middle, bottom);
                Equal(Original(top), System.Runtime.Intrinsics.Vector256.GetElement(triple, 0));
                Equal(Original(middle), System.Runtime.Intrinsics.Vector256.GetElement(triple, 1));
                Equal(Original(bottom), System.Runtime.Intrinsics.Vector256.GetElement(triple, 2));
                var reverse = edge.Endpoints(bottom, top);
                Equal(Original(bottom), reverse.Top); Equal(Original(top), reverse.Bottom);
            }
            var empty = default(Edge);
            Equal(0d, empty.MinY); Equal(0d, empty.MaxY); Assert.Equal(-1, empty.Winding);
            Assert.True(double.IsNaN(empty.X(0))); Assert.True(double.IsNaN(empty.Endpoints(0, 1).Top));
            double[] limits = [0d, -0d, double.Epsilon, -double.Epsilon,
                Math.ScaleB(1d, -1022), -Math.ScaleB(1d, -1022),
                Math.BitDecrement(1d), 1d, Math.BitIncrement(1d), -1d,
                -32768d, 32767d, Math.ScaleB(1d, 53), -Math.ScaleB(1d, 53),
                double.MaxValue, -double.MaxValue, double.PositiveInfinity, double.NegativeInfinity, double.NaN];
            for (var i = 0; i < limits.Length; i++)
                for (var j = 0; j < limits.Length; j++)
                    Check(new(limits[i], limits[(i + j) % limits.Length]),
                        new(limits[j], limits[(i + j + 1) % limits.Length]), limits[(i + 1) % limits.Length], limits[(j + 2) % limits.Length]);
            var random = new Random(0x1040);
            double Bits() => BitConverter.Int64BitsToDouble(random.NextInt64() | (random.Next(2) == 0 ? 0 : long.MinValue));
            double FontCoordinate() => Math.ScaleB(random.NextDouble() * 65536 - 32768, random.Next(-30, 31));
            for (var i = 0; i < 20000; i++)
            {
                Check(new(FontCoordinate(), FontCoordinate()), new(FontCoordinate(), FontCoordinate()), FontCoordinate(), FontCoordinate());
                Check(new(Bits(), Bits()), new(Bits(), Bits()), Bits(), Bits());
            }
        }
    }
}
