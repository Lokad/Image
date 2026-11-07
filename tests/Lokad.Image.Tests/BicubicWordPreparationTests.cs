using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class BicubicWordPreparationTests
{
    [Fact]
    public void EveryAlphaExponentKeepsQualifiedGuardsAndPublishedWords()
    {
        if (!Avx512F.IsSupported) return;
        // Frozen through the actual qualified DLL before changing preparation.
        // Every accepted finite sample also passes independent scalar rounding.
        var result = OriginalBicubicPreparation.Verify(BicubicMath.TryWriteWordPair);
        Assert.Equal(24576, result.Cases); Assert.Equal(2741, result.Accepted);
        Assert.Equal("692a6879f82f609501eb948e6f7a99888837b03ed2048914b993a97855ed601c", result.Digest);
    }
}
