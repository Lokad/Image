using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Tests;

public sealed class ByteCompositeVectorTests
{
    [Fact]
    public void DivisionKeepsEveryDenominatorsNearestIntegerBoundaries()
    {
        if (!Avx512F.IsSupported) return;
        // The only places a rounded float could change the integer result
        // are immediately adjacent to integer quotients. Cover every valid
        // divisor, every byte quotient and both adjacent integer numerators.
        for (var d = 1; d <= 65025; d++)
        for (var first = 0; first < 256; first += 16)
        for (var delta = -1; delta <= 1; delta++)
        {
            var n = (Vector512.Create(first, first + 1, first + 2, first + 3,
                first + 4, first + 5, first + 6, first + 7,
                first + 8, first + 9, first + 10, first + 11,
                first + 12, first + 13, first + 14, first + 15) * Vector512.Create(d)) + Vector512.Create(delta);
            n = Vector512.Max(n, Vector512<int>.Zero);
            var actual = ImageDrawing.DivideByteColor(n, Vector512.Create(d));
            for (var lane = 0; lane < 16; lane++)
                if (actual.GetElement(lane) != n.GetElement(lane) / d)
                    Assert.Fail($"Numerator {n.GetElement(lane)}, denominator {d}.");
        }
    }

    [Fact]
    public void FourPixelsPreserveAllAlphaPairsAndChannelExtremes()
    {
        if (!Avx512F.IsSupported) return;
        for (var sa = 0; sa < 256; sa++)
        for (var da = 0; da < 256; da++)
        {
            var s = Vector128.Create((byte)0, (byte)255, (byte)127, (byte)sa,
                (byte)255, (byte)0, (byte)128, (byte)sa,
                (byte)sa, (byte)da, (byte)(sa ^ da), (byte)sa,
                (byte)1, (byte)254, (byte)255, (byte)sa);
            var d = Vector128.Create((byte)255, (byte)0, (byte)128, (byte)da,
                (byte)0, (byte)255, (byte)127, (byte)da,
                (byte)da, (byte)sa, (byte)(255 - (sa ^ da)), (byte)da,
                (byte)254, (byte)1, (byte)0, (byte)da);
            var actual = ImageDrawing.CompositeFour8(s, d);
            var sw = sa * 255; var dw = da * (255 - sa); var coverage = sw + dw;
            for (var lane = 0; lane < 16; lane++)
            {
                var expected = sa == 0 ? d.GetElement(lane) : lane % 4 == 3 ? (coverage + 127) / 255
                    : (s.GetElement(lane) * sw + d.GetElement(lane) * dw + coverage / 2) / coverage;
                if (actual.GetElement(lane) != expected) Assert.Fail($"Alpha {sa}/{da}, lane {lane}.");
            }
        }
    }
}
