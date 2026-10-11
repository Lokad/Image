using System.Security.Cryptography;
using System.Runtime.Intrinsics;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegInverseTests
{
    // Captured from the accepted owned assembly, before two-pass evaluation.
    // These families include every frequency/sign, half ties, sparse/dense
    // cancellation, zero quantizers and full coefficient/quantizer extremes.
    [Theory]
    [InlineData(0, "e78ff88013c3fd671404fefcf0e73bd7a80bf08e6cec8fde0327ca6365718132")]
    [InlineData(1, "7250bb064871676160023abf0d0089e270d4ccedd6722ddf402a3a8b0005ce58")]
    [InlineData(2, "fe1cc707f870503b140b5321e5ed707c0fbe4d525efd6b69ea28e5936a897e9e")]
    [InlineData(3, "c5233a1a8192212f341a013a2033296ed9cb9fdc2c7718c27f9e52763dabb593")]
    [InlineData(4, "b512670ceee5c45de5b58758fb630636f81b519ceb73564362e50c638b120122")]
    [InlineData(5, "dfa839469288aa294670c5c717a118e93e2d0122f53179352d535c356539dbad")]
    [InlineData(6, "cd6036a68891d19d3b95c5fad49e0b26b51f879e03218b424c592b4c5204ca2e")]
    [InlineData(7, "1e22b138073c4922f423a81d61e5fe6e7a8c7ec0964727e7998ecbd5a107ad5c")]
    public void OriginalCoefficientFamiliesKeepEveryFrozenByte(int family, string expected)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        foreach (var (coefficients, quantizers) in JpegInverseBlocks.Generate(family))
        {
            var originalCoefficients = coefficients.ToArray(); var originalQuantizers = quantizers.ToArray();
            var guarded = Enumerable.Repeat((byte)19, 66).ToArray();
            JpegTransform.Inverse(coefficients, quantizers, guarded.AsSpan(1, 64),
                new OperationContext(ImageLimits.Default with { MaxWork = 4096 }, default));
            Assert.Equal(19, guarded[0]); Assert.Equal(19, guarded[^1]);
            Assert.Equal(originalCoefficients, coefficients); Assert.Equal(originalQuantizers, quantizers);
            hash.AppendData(guarded.AsSpan(1, 64));
        }
        Assert.Equal(expected, Convert.ToHexStringLower(hash.GetHashAndReset()));
    }

    [Fact]
    public void EightSampleRoundingPreservesEveryByteHalfTieNeighborhoodAndClipping()
    {
        var samples = new List<double> { -1e16, -65535, -double.Epsilon, -0.0, 0.0, double.Epsilon, 65535, 1e16 };
        for (var integer = -2; integer <= 257; integer++)
        {
            var half = integer + 0.5;
            samples.AddRange([Math.BitDecrement(half), half, Math.BitIncrement(half), integer]);
        }
        var values = samples.ToArray();
        for (var start = 0; start < values.Length; start += 8)
        {
            var guarded = Enumerable.Repeat((byte)19, 10).ToArray();
            JpegTransform.RoundSamples(Vector512.Create(values.AsSpan(start, 8)), guarded.AsSpan(1, 8));
            Assert.Equal(19, guarded[0]); Assert.Equal(19, guarded[^1]);
            for (var lane = 0; lane < 8; lane++)
                Assert.Equal((byte)Math.Clamp(Math.Round(values[start + lane], MidpointRounding.AwayFromZero), 0, 255), guarded[lane + 1]);
        }
    }

    [Fact]
    public void EightSampleRoundingKeepsDistinctLanesInOrder()
    {
        double[] samples = [254.75, 0.25, 127.5, 1.5, 255, -1, Math.BitDecrement(2.5), 192.75];
        byte[] expected = [255, 0, 128, 2, 255, 0, 2, 193];
        for (var phase = 0; phase < 8; phase++)
        {
            var rotated = Enumerable.Range(0, 8).Select(index => samples[(index + phase) & 7]).ToArray();
            var actual = new byte[8];
            JpegTransform.RoundSamples(Vector512.Create(rotated), actual);
            Assert.Equal(Enumerable.Range(0, 8).Select(index => expected[(index + phase) & 7]), actual);
        }
    }

    [Theory]
    [InlineData(0)] [InlineData(7)] [InlineData(9)]
    public void EightSampleRoundingRejectsIncompleteRowsBeforeWriting(int length)
    {
        var pixels = Enumerable.Repeat((byte)19, length).ToArray();
        Assert.Throws<ArgumentException>(() => JpegTransform.RoundSamples(Vector512<double>.Zero, pixels));
        Assert.All(pixels, value => Assert.Equal(19, value));
    }

    // Original analytic/dense/extreme coefficient blocks. Hashes were captured
    // from the owned scalar implementation before this optimization. Half ties,
    // clipping, ushort quantizers and the full int coefficient range are covered.
    [Theory]
    [InlineData(0, "1df1b7ce1fd8fcbe20cde61646875e54fe38d8945ea7911afd59e025cc520a68")]
    [InlineData(1, "dcbffc1a75cc58ab562156a4244112da7cc5274b1fa66e09a279a5018bfef432")]
    [InlineData(2, "c354e929475813d85401c35279e491162a070f17d4c6e1e1a3f24701f67fbc33")]
    [InlineData(3, "761421230b7d9df6f62ba984f246d5880b13a45ac28bbc93e38abc39f0c15add")]
    [InlineData(4, "1df1b7ce1fd8fcbe20cde61646875e54fe38d8945ea7911afd59e025cc520a68")]
    [InlineData(5, "8667e718294e9e0df1d30600ba3eeb201f764aad2dad72748643e4a285e1d1f7")]
    [InlineData(6, "7c8975e1e60a5c8337f28edf8c33c3b180360b7279644a9bc1af3c51e6220bf5")]
    [InlineData(7, "49ea434f002d18c4ebd735f86896b9348859effd2868b8ccb3fcc231f77e9f9a")]
    [InlineData(8, "d1808250fe4a165a52e7a62af64ad250e79e120ce0d222c5d16edb2e24755773")]
    [InlineData(9, "3e1188e2c0235db3a3cae7df0538ff8c989af0872cf439340373bc62225d6909")]
    [InlineData(10, "131c2bf93f5290d4a1cbdc847280d216568d7ae2d7e5da059ecc27e5d63f356a")]
    [InlineData(11, "4dbbf55a28dce428112ac79b5b535181da0a0f81c50008c01f3f85b58f1652a8")]
    public void InversePreservesOriginalPixelsInputAndBoundaries(int pattern, string expected)
    {
        var coefficients = new int[64]; var quantization = new ushort[64];
        for (var i = 0; i < 64; i++)
        {
            coefficients[i] = pattern switch
            {
                0 => 0, 1 => i == 0 ? 80 : 0, 2 => i == 0 ? -80 : 0,
                3 => i == 0 ? 4 : 0, 4 => i == 0 ? -4 : 0,
                5 => i == 0 ? 1020 : 0, 6 => i == 0 ? -1020 : 0, 7 => i == 27 ? 1023 : 0,
                8 => ((i & 1) == 0 ? 1 : -1) * (i * 17 + 3),
                9 => (i * 7919 + i * i * 12347 + 23459) % 4095 - 2047,
                10 => (i & 1) == 0 ? int.MinValue : int.MaxValue,
                _ => i % 3 == 0 ? 0 : i % 3 == 1 ? int.MaxValue : int.MinValue
            };
            quantization[i] = (ushort)(pattern < 7 ? 1 : pattern == 7 ? 3
                : pattern == 8 ? 1 + i * 2 % 31 : pattern == 9 ? 1 + i * 73 % 255 : 65535);
        }

        var originalCoefficients = coefficients.ToArray(); var originalQuantization = quantization.ToArray();
        var guarded = Enumerable.Repeat((byte)19, 66).ToArray();
        var context = new OperationContext(ImageLimits.Default with { MaxWork = 4096 }, default);
        JpegTransform.Inverse(coefficients, quantization, guarded.AsSpan(1, 64), context);
        Assert.Equal(19, guarded[0]); Assert.Equal(19, guarded[^1]);
        Assert.Equal(originalCoefficients, coefficients); Assert.Equal(originalQuantization, quantization);
        Assert.Equal(expected, Convert.ToHexStringLower(SHA256.HashData(guarded.AsSpan(1, 64))));
        Assert.Throws<ImageLimitException>(() => context.Work(1));
    }

    [Fact]
    public void WorkAndCancellationRejectBeforeAnyPixelIsWritten()
    {
        var pixels = Enumerable.Repeat((byte)19, 64).ToArray();
        Assert.Throws<ImageLimitException>(() => JpegTransform.Inverse(new int[64], new ushort[64], pixels,
            new OperationContext(ImageLimits.Default with { MaxWork = 4095 }, default)));
        Assert.All(pixels, value => Assert.Equal(19, value));
        using var cancellation = new CancellationTokenSource();
        var context = new OperationContext(ImageLimits.Default, cancellation.Token); cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => JpegTransform.Inverse(new int[64], new ushort[64], pixels, context));
        Assert.All(pixels, value => Assert.Equal(19, value));
    }

    [Theory]
    [InlineData(63,64,64)] [InlineData(65,64,64)] [InlineData(64,63,64)]
    [InlineData(64,65,64)] [InlineData(64,64,63)] [InlineData(64,64,65)]
    public void OnlyCompleteCoefficientQuantizerAndPixelBlocksAreAccepted(int input, int quantizers, int output)
    {
        var pixels = Enumerable.Repeat((byte)19, output).ToArray();
        Assert.Throws<ArgumentException>(() => JpegTransform.Inverse(new int[input], new ushort[quantizers], pixels,
            new OperationContext(ImageLimits.Default, default)));
        Assert.All(pixels, value => Assert.Equal(19, value));
    }
}
