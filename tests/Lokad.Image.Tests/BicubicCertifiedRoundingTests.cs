using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class BicubicCertifiedRoundingTests
{
    [Fact]
    public void SharedPublicationKeepsDyadicSamplesAndUntouchedFailureBuffers()
    {
        if (!Avx512F.IsSupported) return;
        var values = new[] { -.25, .25, .5 - 1d / (1 << 20), .5 + 1d / (1 << 20),
            127.5 - 1d / 4096, 127.5 + 1d / 4096, 254.75, 255.25 };
        var accepted = 0; var rejected = 0;
        foreach (var alpha in new[] { 15, 16, 17, 32, 255 })
        foreach (var normalizer in new[] { .499, .5, 1d })
        for (var i = 0; i < values.Length; i++)
        {
            var r = values[i]; var g = values[(i + 3) % values.Length]; var b = values[(i + 6) % values.Length];
            var sum = Vector512.Create(r * alpha, g * alpha, b * alpha, alpha,
                b * alpha, g * alpha, r * alpha, alpha);
            const int guard = 32;
            var output = Enumerable.Repeat((byte)0xa5, guard * 2 + 8).ToArray();
            bool Apply() => BicubicMath.TryWriteOrdinaryBytePair(sum, normalizer, 1, output.AsSpan(guard, 8));
            Apply(); output.AsSpan().Fill(0xa5);
            var allocated = GC.GetAllocatedBytesForCurrentThread(); var actual = Apply();
            Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - allocated);
            if (alpha < 16 || normalizer < .5) Assert.False(actual);
            if (actual)
            {
                accepted++;
                static byte Round(double v) => (byte)Math.Round(Math.Clamp(v, 0, 255), MidpointRounding.ToEven);
                Assert.Equal(new[] { Round(r), Round(g), Round(b), Round(alpha / normalizer),
                    Round(b), Round(g), Round(r), Round(alpha) }, output.AsSpan(guard, 8).ToArray());
            }
            else
            {
                rejected++;
                Assert.Equal(-1, output.AsSpan().IndexOfAnyExcept((byte)0xa5));
            }
            Assert.Equal(-1, output.AsSpan(0, guard).IndexOfAnyExcept((byte)0xa5));
            Assert.Equal(-1, output.AsSpan(guard + 8).IndexOfAnyExcept((byte)0xa5));
        }
        Assert.True(accepted > 0); Assert.True(rejected > 0);
    }

    [Fact]
    public void SharedCompletionPreservesHalfIntegerDecisionsAndClippedSamples()
    {
        if (!Avx512F.IsSupported) return;
        static void Check(Vector512<double> values, Vector512<double> margins)
        {
            var expected = true;
            for (var c = 0; c < 8; c++)
            {
                var v = values.GetElement(c);
                if (v >= 0 && v <= 255 && Math.Abs(v - Math.Floor(v) - .5) <= margins.GetElement(c))
                    expected = false;
            }
            BicubicMath.TryRoundBytePair(values, margins, out _);
            var allocated = GC.GetAllocatedBytesForCurrentThread();
            var actual = BicubicMath.TryRoundBytePair(values, margins, out var samples);
            Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - allocated);
            Assert.Equal(expected, actual);
            if (actual) for (var c = 0; c < 8; c++)
                Assert.Equal((int)Math.Round(Math.Clamp(values.GetElement(c), 0, 255), MidpointRounding.ToEven), samples.GetElement(c));
        }
        foreach (var margin in new[] { 0d, 1d / 65536, 1d / (1 << 25), .01, Math.BitDecrement(.25) })
        foreach (var integer in new[] { -1, 0, 1, 2, 126, 127, 128, 254, 255, 256 })
        foreach (var delta in new[] { -2 * margin, -margin, -margin / 2, 0, margin / 2, margin, 2 * margin })
        {
            var v = integer + .5 + delta;
            var values = Vector512.Create(v, Math.BitDecrement(v), Math.BitIncrement(v), -v, 255 - v, -1e100, 1e100, integer);
            Check(values, Vector512.Create(margin));
            Check(values, Vector512.Create(margin, margin / 2, margin, margin / 2, margin, 0, .01, margin));
        }
        var random = new Random(755);
        for (var i = 0; i < 512; i++)
        {
            double Sample() => random.NextDouble() * 295 - 20;
            Check(Vector512.Create(Sample(), Sample(), Sample(), Sample(), Sample(), Sample(), Sample(), Sample()),
                Vector512.Create(0d, .01, 1d / 65536, .02, 1d / (1 << 25), .03, 0d, .01));
        }
        Check(Vector512.Create(-0d, Math.BitDecrement(0d), Math.BitIncrement(0d), 0d, 255d,
            Math.BitDecrement(255d), Math.BitIncrement(255d), 256d), Vector512.Create(1d / 65536));
    }
}
