using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegReferenceHorizontalFilterTests
{
    [Fact]
    public void AreaPhasesPairedRowsSignedGuardsAndUnalignedTailsMatchScalarFiltering()
    {
        if (!Avx512BW.IsSupported || !BitConverter.IsLittleEndian) return;
        Span<JpegReferenceArea> plans = stackalloc JpegReferenceArea[8];
        var source = new int[20]; var output = new short[20];
        var random = new Random(721934);
        for (var width = 16; width <= 1025; width++)
        {
            var pair = (width & 1) == 0; var target = (width + 1) / 2;
            for (var left = 0; left < width; left += 16)
            {
                var first = left; var last = Math.Min(left + 15, width - 1);
                for (var x = 0; x < 8; x++)
                {
                    var plan = plans[x] = JpegReferenceArea.Create(pair ? width / 2 : width, target,
                        Math.Min(left / 2 + x, target - 1), 14);
                    first = Math.Min(first, plan.Source(0) * (pair ? 2 : 1));
                    last = Math.Max(last, plan.Source(plan.Count - 1) * (pair ? 2 : 1) + (pair ? 1 : 0));
                }
                var columns = last - first + 1;
                if (columns < 16) continue;
                var filter = new JpegReferenceHorizontalFilter(plans, first, pair);
                for (var control = 0; control < 5; control++)
                {
                    Array.Fill(source, unchecked((int)0xa5a5a5a5)); Array.Fill(output, (short)165);
                    var row = source.AsSpan(1, columns);
                    for (var x = 0; x < columns; x++)
                    {
                        var cb = control == 0 ? 1024 : control == 1 ? 15360 : control == 2 ? 0 : random.Next(1024, 15361);
                        var cr = control == 0 ? 15360 : control == 1 ? 1024 : control == 2 ? 16384 : random.Next(1024, 15361);
                        row[x] = cb | cr << 16;
                    }
                    var before = source.ToArray();
                    filter.Apply(row, output.AsSpan(1));
                    for (var x = 0; x < 8; x++)
                    {
                        long cb = 0, cr = 0;
                        for (var tap = 0; tap < plans[x].Count; tap++)
                        {
                            var packed = row[plans[x].Source(tap) * (pair ? 2 : 1) - first];
                            cb += (packed & 65535) * (long)plans[x].Weight(tap);
                            cr += (packed >> 16) * (long)plans[x].Weight(tap);
                        }
                        Assert.Equal(Expected(cb), output[1 + x * 2]);
                        Assert.Equal(Expected(cr), output[2 + x * 2]);
                    }
                    Assert.Equal(before, source);
                    Assert.Equal((short)165, output[0]);
                    Assert.All(output.Skip(17), value => Assert.Equal((short)165, value));
                }
            }
        }
    }

    private static short Expected(long sum)
    {
        var limited = Math.Min(32767, sum >> 13);
        const long scale = (255 * 16384 + 223) / 224;
        const long offset = (255L << 21) - ((16 + 224) << 7) * scale + 8192;
        return checked((short)Math.Min(32767, (limited * scale + offset) >> 14));
    }

    [Fact]
    public void ReusedPlanDoesNotAllocateOrChangeTheBorrowedRow()
    {
        if (!Avx512BW.IsSupported || !BitConverter.IsLittleEndian) return;
        Span<JpegReferenceArea> plans = stackalloc JpegReferenceArea[8];
        for (var x = 0; x < 8; x++) plans[x] = JpegReferenceArea.Create(513, 257, 8 + x, 14);
        var filter = new JpegReferenceHorizontalFilter(plans, 15, false);
        Span<int> row = stackalloc int[18]; row.Fill(8192 | 8192 << 16);
        Span<short> output = stackalloc short[16];
        filter.Apply(row, output);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 128; i++) filter.Apply(row, output);
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
        foreach (var value in row) Assert.Equal(8192 | 8192 << 16, value);
    }
}
