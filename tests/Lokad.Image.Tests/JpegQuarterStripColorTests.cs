using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegQuarterStripColorTests
{
    [Fact]
    public void Q7RowsKeepActualScalarPrecisionAcrossExtremesWeightsTailsAndGuards()
    {
        foreach (var width in new[] { 1, 2, 7, 8, 9, 15, 16, 17, 31, 32, 33, 63, 64, 65, 129, 513 })
        foreach (var weight in new[] { 0, 1, 2047, 2048, 4095, 4096 })
        for (var seed = 0; seed < 4; seed++)
        {
            var random = new Random(1739 + seed * 137 + width); var luma = new byte[width + 2]; random.NextBytes(luma);
            var planes = Enumerable.Range(0, 4).Select(_ => new ushort[width + 2]).ToArray();
            foreach (var plane in planes)
            {
                for (var i = 0; i < plane.Length; i++) plane[i] = (ushort)random.Next(32641);
                plane[0] = 123; plane[^1] = 456;
            }
            for (var i = 0; i < width; i += 4)
            {
                luma[i + 1] = (byte)((seed & 1) * 255);
                for (var p = 0; p < planes.Length; p++) planes[p][i + 1] = (ushort)(((p + seed) & 1) * 32640);
            }
            var originalLuma = luma.ToArray(); var originalPlanes = planes.Select(p => p.ToArray()).ToArray();
            var expected = new byte[width * 4];
            for (var i = 0; i < width; i++)
            {
                var u = (planes[0][i + 1] * (4096 - weight) + planes[1][i + 1] * weight - (128 << 19)) >> 10;
                var v = (planes[2][i + 1] * (4096 - weight) + planes[3][i + 1] * weight - (128 << 19)) >> 10;
                JpegInverseColor.WriteFilteredPixel(luma[i + 1], u, v, expected.AsSpan(i * 4, 4), 4);
            }
            var actual = Enumerable.Repeat((byte)165, expected.Length + 14).ToArray();
            JpegInverseColor.WriteFilteredQuarterRow(luma.AsSpan(1, width), planes[0].AsSpan(1, width), planes[1].AsSpan(1, width),
                planes[2].AsSpan(1, width), planes[3].AsSpan(1, width), weight, actual.AsSpan(7, expected.Length));
            Assert.Equal(expected, actual.AsSpan(7, expected.Length).ToArray());
            Assert.All(actual.Take(7).Concat(actual.Skip(expected.Length + 7)), b => Assert.Equal(165, b));
            Assert.Equal(originalLuma, luma);
            for (var p = 0; p < planes.Length; p++) Assert.Equal(originalPlanes[p], planes[p]);
        }
    }

    [Fact]
    public void InvalidWeightsAndShapesLeaveOutputUntouchedAndValidRowsAllocateNoHeap()
    {
        var luma = new byte[65]; var plane = Enumerable.Range(0, 65).Select(i => (ushort)(i * 127)).ToArray();
        var output = Enumerable.Repeat((byte)165, 65 * 4).ToArray();
        Assert.Throws<ArgumentOutOfRangeException>(() => JpegInverseColor.WriteFilteredQuarterRow(luma, plane, plane, plane, plane, -1, output));
        Assert.Throws<ArgumentOutOfRangeException>(() => JpegInverseColor.WriteFilteredQuarterRow(luma, plane, plane, plane, plane, 4097, output));
        Assert.Throws<ArgumentException>(() => JpegInverseColor.WriteFilteredQuarterRow(luma, plane.AsSpan(1), plane, plane, plane, 2048, output));
        Assert.Throws<ArgumentException>(() => JpegInverseColor.WriteFilteredQuarterRow(luma, plane, plane, plane, plane, 2048, output.AsSpan(1)));
        Assert.All(output, b => Assert.Equal(165, b));
        for (var i = 0; i < 16; i++) JpegInverseColor.WriteFilteredQuarterRow(luma, plane, plane, plane, plane, 2048, output);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 256; i++) JpegInverseColor.WriteFilteredQuarterRow(luma, plane, plane, plane, plane, 2048, output);
        Assert.Equal(before, GC.GetAllocatedBytesForCurrentThread());
    }
}
