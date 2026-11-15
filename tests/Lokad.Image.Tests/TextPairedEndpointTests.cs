namespace Lokad.Image.Tests
{
    public sealed class TextPairedEndpointTests
    {
        [Fact]
        public void PairedEndpointsPreserveScalarCrossingCoordinates()
            => Internal.TextRasterizer.VerifyPairedEndpoints();
    }
}

namespace Lokad.Image.Internal
{
    internal sealed partial class TextRasterizer
    {
        internal static void VerifyPairedEndpoints()
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
                var edge = new Edge(a, b, 0);
                var pair = edge.Endpoints(top, bottom);
                Equal(edge.X(top), pair.Top); Equal(edge.X(bottom), pair.Bottom);
                var reverse = edge.Endpoints(bottom, top);
                Equal(edge.X(bottom), reverse.Top); Equal(edge.X(top), reverse.Bottom);
            }
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
