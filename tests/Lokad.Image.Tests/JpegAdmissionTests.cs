using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegAdmissionTests
{
    [Fact]
    public void SingleSymbolHuffmanTableAdmitsItsArraysAndAllLengthVisits()
    {
        var counts = new byte[16]; counts[0] = 1;
        byte[] symbols = [7];
        var limits = new ImageLimits { MaxWorkingBytes = 717, MaxWork = 272 };
        _ = new JpegHuffman(counts, symbols, new OperationContext(limits, default));
        Assert.Throws<ImageLimitException>(() => new JpegHuffman(counts, symbols,
            new OperationContext(limits with { MaxWorkingBytes = 716 }, default)));
        Assert.Throws<ImageLimitException>(() => new JpegHuffman(counts, symbols,
            new OperationContext(limits with { MaxWork = 271 }, default)));
    }

    [Fact]
    public void InverseBlockTransformAdmitsWorkBeforeChangingItsDestination()
    {
        var pixels = Enumerable.Repeat((byte)127, 64).ToArray();
        var integerCoefficients = new int[64];
        var quantization = Enumerable.Repeat((ushort)1, 64).ToArray();
        var context = new OperationContext(new ImageLimits { MaxWork = 4096 }, default);
        Transform();
        var unchangedPixels = pixels.ToArray();
        Assert.Throws<ImageLimitException>(Transform);
        Assert.Equal(unchangedPixels, pixels);

        using var cancellation = new CancellationTokenSource();
        context = new OperationContext(ImageLimits.Default, cancellation.Token);
        cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(Transform);
        Assert.Equal(unchangedPixels, pixels);

        void Transform()
        {
            JpegTransform.Inverse(integerCoefficients, quantization, pixels, context);
        }
    }
}
