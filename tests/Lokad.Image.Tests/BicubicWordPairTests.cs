using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class BicubicWordPairTests
{
    [Fact]
    public void VerticalWordCoefficientsKeepScalarBits()
    {
        if (!Avx512F.IsSupported) return;
        foreach (var (height, outputHeight) in new[] { (259, 519), (259, 259), (1, 3) })
        for (var y = 0; y < outputHeight; y++)
        {
            var center = (y + .5) * height / outputHeight - .5;
            var first = (int)Math.Ceiling(center - 2);
            var count = (int)Math.Floor(center + 2) - first + 1;
            Assert.InRange(count, 4, 5);
            var generated = BicubicMath.Coefficients(first, center, 1);
            for (var tap = 0; tap < count; tap++)
                Assert.Equal(BitConverter.DoubleToInt64Bits(ImageTransforms.Kernel(first + tap - center, ResizeFilter.Bicubic)),
                    BitConverter.DoubleToInt64Bits(generated.GetElement(tap)));
        }
    }

    [Fact]
    public void FrozenGuardDecisionsAndWordsMatchIndependentRounding()
    {
        if (!Avx512F.IsSupported) return;
        // Frozen through the actual qualified DLL before vector construction
        // changes. Accepted words also pass an independent scalar oracle.
        var result = OriginalBicubicPairs.Verify(BicubicMath.TryWriteWordPair);
        Assert.Equal(1008, result.Cases); Assert.Equal(68, result.Accepted);
        Assert.Equal("5cf2c6d6d478c7a5150a4763584ddd05915ef6c2e30abb8becbfb5f041f11bb6", result.Digest);
    }
}
