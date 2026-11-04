using System.Runtime.InteropServices;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class Vp8PackedReconstructionTests
{
    [Fact]
    public void CertifiedRangeAndEveryOutOfRangeLaneKeepScalarPixelsAndGuards()
    {
        uint state = 0x6ac19f35;
        uint Next() { state ^= state << 13; state ^= state >> 17; state ^= state << 5; return state; }
        var coefficients = new short[32];
        for (var sample = 0; sample < 8192 + 16 * 6; sample++)
        {
            for (var i = 0; i < coefficients.Length; i++) coefficients[i] = (short)((int)(Next() % 4095) - 2047);
            if (sample < 16) Array.Fill(coefficients, (short)(sample % 2 == 0 ? 2047 : -2047));
            if (sample >= 8192)
            {
                var control = sample - 8192;
                coefficients[control / 6] = (control % 6) switch
                { 0 => -2048, 1 => 2048, 2 => short.MinValue, 3 => short.MaxValue, 4 => -2047, _ => 2047 };
            }
            var before = coefficients.ToArray(); var pair = (sample & 1) != 0;
            var stride = Math.Max(pair ? 8 : 4, (sample % 3) switch { 0 => 4, 1 => 5, _ => 16 });
            var actual = Enumerable.Repeat((byte)0xa7, 160).ToArray(); var offset = sample % 8;
            for (var y = 0; y < 4; y++) for (var x = 0; x < (pair ? 8 : 4); x++) actual[offset + y * stride + x] = (byte)Next();
            var expected = actual.ToArray(); Scalar(before, expected.AsSpan(offset), stride, pair);
            Vp8BlockReconstruction.Add(coefficients, actual.AsSpan(offset), stride, pair);
            Assert.Equal(expected, actual); Assert.Equal(before, coefficients);
        }
    }

    [Fact]
    public void CertifiedSingleAndPairOutputsMayOverlapCoefficientStorage()
    {
        foreach (var pair in new[] { false, true })
        foreach (var stride in new[] { 4, 8, 16, 31 })
        foreach (var value in new short[] { -2047, -1, 0, 1, 2047 })
        {
            if (pair && stride < 8) continue;
            var storage = Enumerable.Repeat((byte)0xa5, 256).ToArray();
            var coefficients = MemoryMarshal.Cast<byte, short>(storage.AsSpan(33, pair ? 64 : 32));
            for (var i = 0; i < coefficients.Length; i++) coefficients[i] = i % 2 == 0 ? value : (short)-value;
            var before = coefficients.ToArray(); var expected = storage.ToArray(); var offset = pair ? 65 : 41;
            Scalar(before, expected.AsSpan(offset), stride, pair);
            Vp8BlockReconstruction.Add(coefficients, storage.AsSpan(offset), stride, pair);
            Assert.Equal(expected, storage);
        }
    }

    private static void Scalar(short[] original, Span<byte> pixels, int stride, bool pair)
    {
        var coefficients = original.ToArray();
        for (var block = 0; block < (pair ? 2 : 1); block++)
        {
            var residual = coefficients.AsSpan(block * 16, 16); Vp8Transforms.InverseDct(residual);
            for (var y = 0; y < 4; y++) for (var x = 0; x < 4; x++)
            {
                var at = y * stride + block * 4 + x;
                pixels[at] = (byte)Math.Clamp(pixels[at] + residual[y * 4 + x], 0, 255);
            }
        }
    }
}
