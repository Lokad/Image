using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegCertifiedColorRowTests
{
    [Fact]
    public void CertifiedBatchesAndFallbackKeepIndependentBinary64CoordinatesAndGuards()
    {
        Span<byte> scratch = stackalloc byte[24];
        for (var phase = 0; phase < 8; phase++)
        for (var seed = 0; seed < 256; seed++)
        foreach (var y in new[] { 2, 3 })
        foreach (var reused in new[] { false, true })
        {
            var inputs = Enumerable.Range(0, 5).Select(c => Enumerable.Range(0, 18).Select(i => (byte)(phase switch
            {
                0 => 0, 1 => 255, 2 => 128, 3 => (i + c) % 2 == 0 ? 0 : 255,
                _ => i * (53 + phase * 2) + c * 101 + seed * (phase * 2 + 1)
            })).ToArray()).ToArray();
            if (reused) { inputs[2] = inputs[1]; inputs[4] = inputs[3]; }
            var original = inputs.Select(x => x.ToArray()).ToArray();
            var rgba = Enumerable.Repeat((byte)123, 66).ToArray();
            var topWeight = y == 2 ? 1 : 3;
            var accepted = JpegDecoder.TryRenderYcbcr420Row16(inputs[0].AsSpan(1, 16), inputs[1].AsSpan(1, 16),
                inputs[2].AsSpan(1, 16), inputs[3].AsSpan(1, 16), inputs[4].AsSpan(1, 16), topWeight, rgba.AsSpan(1, 64));
            if (!accepted)
            {
                Assert.All(rgba, x => Assert.Equal((byte)123, x));
                for (var block = 0; block < 2; block++)
                    JpegDecoder.RenderYcbcr420Row8(inputs[0].AsSpan(1 + block * 8, 8), inputs[1].AsSpan(1 + block * 4, 8),
                        inputs[2].AsSpan(1 + block * 4, 8), inputs[3].AsSpan(1 + block * 4, 8), inputs[4].AsSpan(1 + block * 4, 8),
                        topWeight, rgba.AsSpan(1 + block * 32, 32), scratch);
            }
            for (var lane = 0; lane < 16; lane++)
            {
                var cx = (lane + 2 + .5) / 2 - .5; var left = (int)Math.Floor(cx); var fx = cx - left;
                var cy = (y + .5) / 2 - .5; var fy = cy - Math.Floor(cy);
                double Interpolate(int upper, int lower)
                {
                    var a = inputs[upper][left + 1] * (1 - fx) + inputs[upper][left + 2] * fx;
                    var b = inputs[lower][left + 1] * (1 - fx) + inputs[lower][left + 2] * fx;
                    return a * (1 - fy) + b * fy - 128;
                }
                var luma = inputs[0][lane + 1]; var cb = Interpolate(1, 2); var cr = Interpolate(3, 4);
                Assert.Equal(Round(luma + 1.402 * cr), rgba[1 + lane * 4]);
                Assert.Equal(Round(luma - .344136286 * cb - .714136286 * cr), rgba[2 + lane * 4]);
                Assert.Equal(Round(luma + 1.772 * cb), rgba[3 + lane * 4]); Assert.Equal(255, rgba[4 + lane * 4]);
            }
            Assert.Equal(123, rgba[0]); Assert.Equal(123, rgba[^1]);
            for (var i = 0; i < 5; i++) Assert.Equal(original[i], inputs[i]);
        }
    }

    [Fact]
    public void ExactHalfRefusesWithoutWritingAndIntegerColorsCanBeCertified()
    {
        var luma = new byte[16]; var cb = Enumerable.Repeat((byte)253, 16).ToArray();
        var neutral = Enumerable.Repeat((byte)128, 16).ToArray(); var output = Enumerable.Repeat((byte)123, 64).ToArray();
        Assert.Equal(221.5, 1.772 * 125);
        Assert.False(JpegDecoder.TryRenderYcbcr420Row16(luma, cb, cb, neutral, neutral, 1, output));
        Assert.All(output, x => Assert.Equal((byte)123, x));
        var supported = Avx512F.IsSupported && Avx512F.VL.IsSupported && Avx2.IsSupported;
        Assert.Equal(supported, JpegDecoder.TryRenderYcbcr420Row16(luma, neutral, neutral, neutral, neutral, 3, output));
        if (supported)
            for (var i = 0; i < 16; i++) Assert.Equal(new byte[] { 0, 0, 0, 255 }, output.AsSpan(i * 4, 4).ToArray());
    }

    private static byte Round(double value) => (byte)Math.Clamp(Math.Round(value, MidpointRounding.AwayFromZero), 0, 255);
}
