using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class BicubicWordQuartetTests
{
    [Theory]
    [InlineData(false, false)] [InlineData(false, true)]
    [InlineData(true, false)] [InlineData(true, true)]
    public void BothPositionsKeepFrozenPairDecisionsIndependentOfCompanion(bool secondPosition, bool refusedCompanion)
    {
        if (!Avx512F.IsSupported) return;
        var companion = refusedCompanion ? Vector512<double>.Zero
            : Vector512.Create(65535d * 10.25, 65535d * 20.25, 65535d * 30.25, 65535d,
                65535d * 40.25, 65535d * 50.25, 65535d * 60.25, 65535d);
        var result = OriginalBicubicPairs.Verify((sum, n0, n1, output) =>
        {
            Span<byte> actual = stackalloc byte[48]; actual.Fill(0xa5);
            Span<byte> expectedCompanion = stackalloc byte[16]; expectedCompanion.Fill(0xa5);
            var companionAccepted = BicubicMath.TryWriteWordPair(companion, 1, 1, expectedCompanion);
            Assert.Equal(!refusedCompanion, companionAccepted);
            var mask = secondPosition
                ? BicubicMath.WriteWordQuartet(companion, sum, 1, 1, n0, n1, actual.Slice(8, 32))
                : BicubicMath.WriteWordQuartet(sum, companion, n0, n1, 1, 1, actual.Slice(8, 32));
            Assert.Equal(companionAccepted, (mask & (secondPosition ? 1 : 2)) != 0);
            Assert.True(expectedCompanion.SequenceEqual(actual.Slice(secondPosition ? 8 : 24, 16)));
            Assert.Equal(-1, actual[..8].IndexOfAnyExcept((byte)0xa5));
            Assert.Equal(-1, actual[40..].IndexOfAnyExcept((byte)0xa5));
            var accepted = (mask & (secondPosition ? 2 : 1)) != 0;
            if (accepted) actual.Slice(secondPosition ? 24 : 8, 16).CopyTo(output);
            else Assert.Equal(-1, actual.Slice(secondPosition ? 24 : 8, 16).IndexOfAnyExcept((byte)0xa5));
            return accepted;
        });
        Assert.Equal(1008, result.Cases); Assert.Equal(68, result.Accepted);
        Assert.Equal("5cf2c6d6d478c7a5150a4763584ddd05915ef6c2e30abb8becbfb5f041f11bb6", result.Digest);
    }
}
