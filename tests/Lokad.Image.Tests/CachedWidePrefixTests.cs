using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class CachedWidePrefixTests
{
    [Fact]
    public void SecondPairRefusalPreservesPublishedFirstPairAndRestoresRows()
    {
        if (!Avx512F.IsSupported || !Avx512DQ.IsSupported || !Avx2.IsSupported || !Sse41.IsSupported) return;
        using var result = ImageBuffer.Create(8, 1, 8);
        result.GetRow8(0).Fill(0xcd);
        using var context = new OperationContext(ImageLimits.Default, default);
        // The third cache row has two opaque pixels, then transparent pixels.
        // Only the first pair passes the real ordinary-byte certificate.
        var cached = new double[3 * 8 * 4];
        for (var pixel = 0; pixel < 2; pixel++)
        {
            var at = 64 + pixel * 4;
            cached[at] = 20 * 255; cached[at + 1] = 30 * 255;
            cached[at + 2] = 40 * 255; cached[at + 3] = 255;
        }
        int[] rows = [64];
        var firstUnpublished = CachedBicubicResize.WriteWideByteQuartetPrefix(result, 0, cached,
            rows, [1d], Enumerable.Repeat(1d, 8).ToArray(), 1, context);
        Assert.Equal(2, firstUnpublished);
        Assert.Equal(64, rows[0]);
        Assert.Equal(new byte[] { 20, 30, 40, 255, 20, 30, 40, 255 }, result.GetRow8(0)[..8].ToArray());
        Assert.All(result.GetRow8(0)[8..].ToArray(), value => Assert.Equal((byte)0xcd, value));
    }
}
