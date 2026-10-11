using Lokad.Image.Internal;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Tests;

public sealed class JpegForwardQuantizedTests
{
    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)]
    [InlineData(5)] [InlineData(6)] [InlineData(7)] [InlineData(8)] [InlineData(9)]
    public void EveryEncoderDivisorAndMixedLanesKeepOriginalQuantizedCoefficients(int pattern)
    {
        var source = Enumerable.Repeat((byte)123, 66).ToArray();
        for (var i = 0; i < 64; i++) source[i + 1] = pattern switch
        {
            0 => 0, 1 => 128, 2 => 255, 3 => (byte)(i * 4),
            4 => (byte)(((i / 8 + i % 8) & 1) * 255), 5 => (byte)(i == 27 ? 255 : 128),
            6 => (byte)((i * 73 + i * i * 19 + 17) & 255),
            7 => (byte)(255 - ((i * 73 + i * i * 19 + 17) & 255)),
            8 => 2, _ => 3
        };
        var original = source.ToArray(); var direct = new double[64];
        JpegTransform.Forward(source.AsSpan(1, 64), direct, new OperationContext(ImageLimits.Default, default));
        // Original constant blocks cross a quotient half boundary if row/column
        // regrouping escapes without certification. A serial direct evaluation
        // lies one ULP below the absolute half boundary for these divisors.
        if (pattern == 8) Assert.Equal(Math.BitIncrement(-1008d), direct[0]);
        if (pattern == 9) Assert.Equal(Math.BitIncrement(-1000d), direct[0]);
        var quantizers = new ushort[64]; var guarded = new int[66];
        for (var divisor = 1; divisor <= 255; divisor++)
        {
            Array.Fill(quantizers, (ushort)divisor); Check();
            if (pattern == 8 && divisor == 32) Assert.Equal(-31, guarded[1]);
            if (pattern == 9 && divisor == 16) Assert.Equal(-62, guarded[1]);
        }
        ushort[] choices = [1, 2, 3, 7, 17, 31, 127, 255];
        for (var phase = 0; phase < 8; phase++)
        {
            for (var i = 0; i < 64; i++) quantizers[i] = choices[(i + phase) & 7];
            Check();
        }
        void Check()
        {
            Array.Fill(guarded, 12345); var originalQuantizers = quantizers.ToArray();
            const long prior = 11;
            var context = new OperationContext(ImageLimits.Default with { MaxWork = prior + 4096 }, default);
            context.Work(prior); Transform(source.AsSpan(1, 64), quantizers, guarded.AsSpan(1, 64), context);
            for (var i = 0; i < 64; i++)
                Assert.Equal((int)Math.Round(direct[i] / quantizers[i], MidpointRounding.AwayFromZero), guarded[i + 1]);
            Assert.Throws<ImageLimitException>(() => context.Work(1));
            Assert.Equal(12345, guarded[0]); Assert.Equal(12345, guarded[^1]);
            Assert.Equal(original, source); Assert.Equal(originalQuantizers, quantizers);
        }
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void WholeTransformAdmissionAndCancellationPrecedeDestinationWrites(bool cancellable)
    {
        var source = Enumerable.Repeat((byte)2, 64).ToArray(); var original = source.ToArray();
        var quantizers = Enumerable.Repeat((ushort)32, 64).ToArray(); var result = Enumerable.Repeat(12345, 64).ToArray();
        using var cancellation = new CancellationTokenSource(); var token = cancellable ? cancellation.Token : default;
        const long prior = 11;
        var context = new OperationContext(ImageLimits.Default with { MaxWork = prior + 4095 }, token); context.Work(prior);
        var error = Assert.Throws<ImageLimitException>(() => Transform(source, quantizers, result, context));
        Assert.Equal("Computational work limit exceeded.", error.Message);
        context.Work(4095); Assert.Throws<ImageLimitException>(() => context.Work(1));
        Assert.All(result, value => Assert.Equal(12345, value)); Assert.Equal(original, source);
        var canceled = new OperationContext(ImageLimits.Default, cancellation.Token); canceled.Work(prior); cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => Transform(source, quantizers, result, canceled));
        Assert.All(result, value => Assert.Equal(12345, value)); Assert.Equal(original, source);
    }

    [Fact]
    public void ZeroDivisorDeclinesWithoutWorkOrDestinationWrites()
    {
        var source = Enumerable.Repeat((byte)2, 64).ToArray();
        var quantizers = Enumerable.Repeat((ushort)32, 64).ToArray(); quantizers[^1] = 0;
        var result = Enumerable.Repeat(12345, 64).ToArray();
        var originalSource = source.ToArray(); var originalQuantizers = quantizers.ToArray();
        var context = new OperationContext(ImageLimits.Default with { MaxWork = 11 }, default); context.Work(11);
        Assert.False(JpegTransform.TryForwardQuantized(source, quantizers, result, context));
        Assert.All(result, value => Assert.Equal(12345, value));
        Assert.Equal(originalSource, source); Assert.Equal(originalQuantizers, quantizers);
        Assert.Throws<ImageLimitException>(() => context.Work(1));
    }

    private static void Transform(ReadOnlySpan<byte> pixels, ReadOnlySpan<ushort> quantizers, Span<int> result, OperationContext context)
    {
        var selected = JpegTransform.TryForwardQuantized(pixels, quantizers, result, context);
        Assert.Equal(Avx512F.IsSupported && Avx2.IsSupported, selected);
        if (selected) return;
        Span<double> coefficients = stackalloc double[64];
        JpegTransform.Forward(pixels, coefficients, context); JpegEncoder.Quantize(coefficients, quantizers, result);
    }
}
