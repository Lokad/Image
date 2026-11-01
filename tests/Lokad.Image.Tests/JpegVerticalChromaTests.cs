using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegVerticalChromaTests
{
    [Fact]
    public void RowsMatchWideIntegerReferenceWithSignedSamplesPhasesAndGuards()
    {
        short[] values = [short.MinValue, -129, -128, -65, -64, -1, 0, 1, 63, 64, 127, 128, 32639, 32640, 32766, short.MaxValue];
        var random = new Random(0x64c731);
        var source = new short[3 + 3 * 36 + 5];
        var output = new byte[27];
        var counts = new HashSet<int>();
        foreach (var length in new[] { 1, 2, 3, 4, 5, 16, 17, 31, 32, 33, 258, 259, 513, 1025 })
        foreach (var reduce in new[] { false, true })
        {
            var target = reduce ? (length + 1) / 2 : length;
            for (var index = 0; index < target; index++)
            {
                var plan = JpegReferenceArea.Create(length, target, index, 12);
                counts.Add(plan.Count);
                var weightSum = 0;
                for (var tap = 0; tap < plan.Count; tap++)
                {
                    Assert.InRange(plan.Weight(tap), 0, 4096);
                    weightSum += plan.Weight(tap);
                }
                Assert.Equal(4096, weightSum);
                var firstY = plan.Source(0);
                foreach (var stride in new[] { 16, 36 })
                for (var sample = 0; sample <= values.Length; sample++)
                {
                    for (var i = 0; i < source.Length; i++)
                        source[i] = sample == values.Length ? (short)random.Next(short.MinValue, short.MaxValue + 1)
                            : values[(i + sample) % values.Length];
                    var saved = (short[])source.Clone();
                    output.AsSpan().Fill(173);
                    var expected = (byte[])output.Clone();
                    for (var x = 0; x < 8; x++)
                    for (var channel = 0; channel < 2; channel++)
                    {
                        long sum = 1 << 18;
                        for (var tap = 0; tap < plan.Count; tap++)
                            sum += (long)source[3 + (plan.Source(tap) - firstY) * stride + x * 2 + channel] * plan.Weight(tap);
                        expected[(channel == 0 ? 2 : 16) + x] = (byte)Math.Clamp(sum >> 19, 0, 255);
                    }
                    JpegReferenceColor.VerticalChromaRow(source.AsSpan(3, (plan.Source(plan.Count - 1) - firstY) * stride + 16),
                        stride, firstY, plan, output.AsSpan(2, 8), output.AsSpan(16, 8));
                    Assert.Equal(expected, output);
                    Assert.Equal(saved, source);
                }
            }
        }
        Assert.Equal(new[] { 1, 2, 3 }, counts.Order().ToArray());
    }

    [Fact]
    public void RowUsesOnlyBorrowedStorage()
    {
        var source = new short[3 * 36]; var output = new byte[16];
        var plan = JpegReferenceArea.Create(259, 130, 63, 12);
        var firstY = plan.Source(0);
        for (var i = 0; i < 4096; i++)
            JpegReferenceColor.VerticalChromaRow(source, 36, firstY, plan, output.AsSpan(0, 8), output.AsSpan(8, 8));
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 4096; i++)
            JpegReferenceColor.VerticalChromaRow(source, 36, firstY, plan, output.AsSpan(0, 8), output.AsSpan(8, 8));
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        GC.KeepAlive(output);
        Assert.Equal(0, allocated);
    }
}
