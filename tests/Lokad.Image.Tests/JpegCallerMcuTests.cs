using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegCallerMcuTests
{
    [Fact]
    public void PreparedBlocksMatchScalarSamplesIncludingPaddedAndClampedEdges()
    {
        if (!(Avx512F.IsSupported && Avx512F.VL.IsSupported && Avx2.IsSupported && Ssse3.IsSupported)) return;
        const int width = 19, height = 17, stride = 65;
        var samples = Enumerable.Range(0, stride * height).Select(i => (byte)(i * 73 + i / 7)).ToArray();
        var unchanged = samples.ToArray(); var context = new OperationContext(ImageLimits.Default, default);
        var input = new JpegSampleInput(samples, width, height, JpegSampleFormat.Rgb, stride, context);
        var blocks = new byte[384];
        foreach (var top in new[] { 0, 16 })
        foreach (var left in new[] { 0, 16 })
        {
            JpegEncoder.SampleCaller420Mcu(input, left, top, blocks);
            for (var component = 0; component < 3; component++)
            for (var by = 0; by < (component == 0 ? 2 : 1); by++)
            for (var bx = 0; bx < (component == 0 ? 2 : 1); bx++)
            for (var y = 0; y < 8; y++)
            for (var x = 0; x < 8; x++)
            {
                var factor = component == 0 ? 1 : 2; double sum = 0;
                for (var dy = 0; dy < factor; dy++)
                for (var dx = 0; dx < factor; dx++)
                {
                    var sx = Math.Min(left + bx * 8 + x * factor + dx, width - 1);
                    var sy = Math.Min(top + by * 8 + y * factor + dy, height - 1);
                    var at = sy * stride + sx * 3; var r = samples[at]; var g = samples[at + 1]; var b = samples[at + 2];
                    sum += component == 0 ? 0.299 * r + 0.587 * g + 0.114 * b
                        : component == 1 ? 128 - 0.168735892 * r - 0.331264108 * g + 0.5 * b
                        : 128 + 0.5 * r - 0.418687589 * g - 0.081312411 * b;
                }
                var block = component == 0 ? by * 2 + bx : component + 3;
                Assert.Equal((byte)Math.Clamp(Math.Round(sum / (factor * factor), MidpointRounding.AwayFromZero), 0, 255), blocks[block * 64 + y * 8 + x]);
            }
        }
        Assert.Equal(unchanged, samples);
    }
}
