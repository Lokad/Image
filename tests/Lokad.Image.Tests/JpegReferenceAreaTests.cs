using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegReferenceAreaTests
{
    [Fact]
    public void LegalAxisPhasesAndMcuExtentsFitTheBoundedPackedTile()
    {
        Span<int> starts = stackalloc int[3];
        for (var length = 1; length <= 65535; length++)
        {
            var target = (length + 1) / 2;
            if (length >= 5 && (length & 1) != 0)
            {
                var step = ((long)length * 65536 + target / 2) / target;
                var deficit = 131072 - step;
                // Odd-axis phase is monotone in the output index. Its last
                // phase stays below1.5 cells, bounding the three-tap start
                // below by2*i-2. Even axes have an exact two-cell step.
                Assert.True(deficit > 0 && (2L * target - 1) * deficit < 3L * 65536);
            }
            var lastMcu = (length - 1) / 16 * 16;
            starts[0] = 0; starts[1] = lastMcu / 32 * 16; starts[2] = lastMcu;
            foreach (var start in starts)
            {
                var first = start; var last = Math.Min(start + 15, length - 1);
                for (var x = 0; x < 8; x++)
                {
                    var plan = JpegReferenceArea.Create(length, target, Math.Min(start / 2 + x, target - 1), 14);
                    first = Math.Min(first, plan.Source(0)); last = Math.Max(last, plan.Source(plan.Count - 1));
                }
                if (first < 0 || last >= length || last - first + 1 > 18)
                    Assert.Fail($"Length {length}, MCU {start}: [{first}, {last}].");
            }
        }
    }
}
