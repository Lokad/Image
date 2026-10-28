using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class BicubicWordPairTests
{
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
