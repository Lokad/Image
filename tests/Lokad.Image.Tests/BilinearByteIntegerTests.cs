using System.Reflection;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class BilinearByteIntegerTests
{
    private delegate Vector512<int> Horizontal(Vector128<uint> left, Vector128<uint> right,
        Vector512<int> leftWeights, Vector512<int> rightWeights);
    private delegate void Tap(ref Vector512<long> first, ref Vector512<long> second,
        Vector128<uint> pixels, int w0, int w1, int w2, int w3);

    [Fact]
    public void FactoredIntegerLanesMatchScalarAndOriginalFourTapSums()
    {
        if (!Avx512F.IsSupported || !Avx512DQ.IsSupported) return;
        var type = typeof(BilinearUpsample);
        var horizontal = type.GetMethod("Horizontal", BindingFlags.NonPublic | BindingFlags.Static)!.CreateDelegate<Horizontal>();
        var tap = type.GetMethod("Tap", BindingFlags.NonPublic | BindingFlags.Static)!.CreateDelegate<Tap>();
        uint[][] samples = [[0xffffffff, 0x00ffffff, 0x0100ffff, 0xffabcdef],
            [0, 0xff000000, 0x80808080, 0xffffffff], [0x01010101, 0xfeff00ff, 0x7f00ff00, 0xffffffff]];
        foreach (var (dx, dy) in new[] { (2, 2), (2054, 1038), (33024, 65024) })
        foreach (var topLeft in samples)
        foreach (var bottomRight in samples)
        foreach (var fy in new[] { 0, dy / 2, dy - 1, dy })
        {
            int[] f = [0, dx / 2, dx - 1, dx]; var w = f.Select(x => dx - x).ToArray();
            var a = Vector128.Create(topLeft[0], topLeft[1], topLeft[2], topLeft[3]);
            var b = Vector128.Create(bottomRight[0], bottomRight[1], bottomRight[2], bottomRight[3]);
            var weights = Vector512.Create(w[0], w[0], w[0], w[0], w[1], w[1], w[1], w[1],
                w[2], w[2], w[2], w[2], w[3], w[3], w[3], w[3]);
            var fractions = Vector512.Create(f[0], f[0], f[0], f[0], f[1], f[1], f[1], f[1],
                f[2], f[2], f[2], f[2], f[3], f[3], f[3], f[3]);
            var top = horizontal(a, b, weights, fractions); var bottom = horizontal(b, a, weights, fractions);
            var oldFirst = Vector512<long>.Zero; var oldSecond = Vector512<long>.Zero; var wy = dy - fy;
            tap(ref oldFirst, ref oldSecond, a, w[0] * wy, w[1] * wy, w[2] * wy, w[3] * wy);
            tap(ref oldFirst, ref oldSecond, b, f[0] * wy, f[1] * wy, f[2] * wy, f[3] * wy);
            tap(ref oldFirst, ref oldSecond, b, w[0] * fy, w[1] * fy, w[2] * fy, w[3] * fy);
            tap(ref oldFirst, ref oldSecond, a, f[0] * fy, f[1] * fy, f[2] * fy, f[3] * fy);
            for (var pixel = 0; pixel < 4; pixel++)
            for (var channel = 0; channel < 4; channel++)
            {
                var lane = pixel * 4 + channel;
                var first = Product(topLeft[pixel], channel); var second = Product(bottomRight[pixel], channel);
                var topExpected = first * w[pixel] + second * f[pixel];
                var bottomExpected = second * w[pixel] + first * f[pixel];
                Assert.InRange(topExpected, 0, int.MaxValue); Assert.InRange(bottomExpected, 0, int.MaxValue);
                Assert.Equal(topExpected, top.GetElement(lane)); Assert.Equal(bottomExpected, bottom.GetElement(lane));
                var exact = topExpected * wy + bottomExpected * fy;
                Assert.Equal(exact, lane < 8 ? oldFirst.GetElement(lane) : oldSecond.GetElement(lane - 8));
                Assert.Equal(exact, (long)top.GetElement(lane) * wy + (long)bottom.GetElement(lane) * fy);
            }
        }
    }

    private static long Product(uint pixel, int channel)
        => channel == 3 ? pixel >> 24 : (pixel >> (channel * 8) & 255) * (pixel >> 24);
}
