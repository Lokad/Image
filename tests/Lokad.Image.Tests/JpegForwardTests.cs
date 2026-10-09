using System.Buffers.Binary;
using System.Security.Cryptography;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegForwardTests
{
    // Original analytic blocks. SHA256 pins all 64 binary64 coefficient bits
    // from the owned scalar implementation before SIMD, serialized little-endian.
    [Theory]
    [InlineData(0, "6a3f69f9e31dfc7cfa0f6d015a897a297587be916cbf9c075e172abe17a04817")]
    [InlineData(1, "076a27c79e5ace2a3d47f9dd2e83e4ff6ea8872b3c2218f66c92b89b55f36560")]
    [InlineData(2, "0ef4ccb8f895deb5001307ccc7596c05c383bbba2cc756643e4446f1fe0a9aed")]
    [InlineData(3, "a45eae35013670625f76fc6142557808f8c83154697490514b295cae3401c671")]
    [InlineData(4, "76c1d0d8eb1d5498ba3cb2b7e175567e287b94e75ecb3f683478084717916e6e")]
    [InlineData(5, "e978934998919ee9cf2abd141ba6ea46ef4cf3c28e9b370ae39267686442c5e8")]
    [InlineData(6, "5a0c3719b228a1b33c5c2115ff2a1e3538db25a52bc89419c2a26aa7f643e832")]
    [InlineData(7, "ffeb5de9b637ea158110dd2106797ee75bcaf82795d67d47d31c1977e46acb7a")]
    public void ForwardPreservesEveryOriginalCoefficientBitAndBounds(int pattern, string expected)
    {
        var pixels = new byte[64];
        for (var i = 0; i < 64; i++) pixels[i] = pattern switch
        {
            0 => 0, 1 => 128, 2 => 255, 3 => (byte)(i * 4),
            4 => (byte)(((i / 8 + i % 8) & 1) * 255), 5 => (byte)(i == 27 ? 255 : 128),
            6 => (byte)((i * 73 + i * i * 19 + 17) & 255),
            _ => (byte)(255 - ((i * 73 + i * i * 19 + 17) & 255))
        };
        var original = pixels.ToArray();
        var guarded = Enumerable.Repeat(12345d, 66).ToArray();
        var context = new OperationContext(ImageLimits.Default with { MaxWork = 4096 }, default);
        JpegTransform.Forward(pixels, guarded.AsSpan(1, 64), context);
        Assert.Equal(12345d, guarded[0]); Assert.Equal(12345d, guarded[^1]); Assert.Equal(original, pixels);
        var bytes = new byte[512];
        for (var i = 0; i < 64; i++)
            BinaryPrimitives.WriteInt64LittleEndian(bytes.AsSpan(i * 8), BitConverter.DoubleToInt64Bits(guarded[i + 1]));
        Assert.Equal(expected, Convert.ToHexStringLower(SHA256.HashData(bytes)));
        Assert.Throws<ImageLimitException>(() => context.Work(1));
    }

    [Fact]
    public void WorkAndCancellationRejectBeforeAnyCoefficientIsWritten()
    {
        var pixels = new byte[64]; var coefficients = Enumerable.Repeat(17d, 64).ToArray();
        Assert.Throws<ImageLimitException>(() => JpegTransform.Forward(pixels, coefficients,
            new OperationContext(ImageLimits.Default with { MaxWork = 4095 }, default)));
        Assert.All(coefficients, value => Assert.Equal(17d, value));
        using var cancellation = new CancellationTokenSource();
        var context = new OperationContext(ImageLimits.Default, cancellation.Token); cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => JpegTransform.Forward(pixels, coefficients, context));
        Assert.All(coefficients, value => Assert.Equal(17d, value));
    }

    [Theory]
    [InlineData(63, 64)] [InlineData(65, 64)] [InlineData(64, 63)] [InlineData(64, 65)]
    public void OnlyCompleteBlocksAreAccepted(int input, int output)
    {
        var coefficients = Enumerable.Repeat(17d, output).ToArray();
        Assert.Throws<ArgumentException>(() => JpegTransform.Forward(new byte[input], coefficients,
            new OperationContext(ImageLimits.Default, default)));
        Assert.All(coefficients, value => Assert.Equal(17d, value));
    }
}
