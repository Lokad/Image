using System.Buffers.Binary;
using System.Reflection;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class BicubicByteNormalizationTests
{
    private delegate bool PairWriter(Vector512<double> sum, double n0, double n1, Span<byte> output);
    private static readonly PairWriter Write = typeof(ColumnBicubicResize)
        .GetMethod("TryWritePair", BindingFlags.NonPublic | BindingFlags.Static)!.CreateDelegate<PairWriter>();

    [Theory]
    [InlineData(16, .5, 0)]
    [InlineData(16, .5, 1)]
    [InlineData(16, 4, 17)]
    [InlineData(17, 4, 127)]
    [InlineData(255, 4, 254)]
    [InlineData(255, .5, 255)]
    [InlineData(1020, 4, -255)]
    [InlineData(1020, 4, 510)]
    public void CertifiedPairsKeepClippingRoundingAndDestinationCanaries(double alpha, double normalizer, double color)
    {
        if (!Avx512F.IsSupported) return;
        Span<byte> destination = stackalloc byte[12];
        destination.Fill(0xa7);
        var sum = Vector512.Create(color * alpha, 13 * alpha, 241 * alpha, alpha,
            53 * alpha, color * alpha, 239 * alpha, alpha);
        Assert.True(Write(sum, normalizer, normalizer, destination.Slice(2, 8)));
        var expectedAlpha = (byte)Math.Clamp(Math.Round(alpha / normalizer, MidpointRounding.AwayFromZero), 0, 255);
        var expectedColor = (byte)Math.Clamp(color, 0, 255);
        Assert.Equal(new byte[] { expectedColor, 13, 241, expectedAlpha, 53, expectedColor, 239, expectedAlpha }, destination.Slice(2, 8).ToArray());
        Assert.Equal(0xa7a7u, BinaryPrimitives.ReadUInt16LittleEndian(destination));
        Assert.Equal(0xa7a7u, BinaryPrimitives.ReadUInt16LittleEndian(destination.Slice(10)));
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(127)] [InlineData(254)]
    public void HalfDecisionsRefuseWithoutWriting(int integer)
    {
        if (!Avx512F.IsSupported) return;
        var half = integer + .5;
        Span<byte> destination = stackalloc byte[8];
        foreach (var value in new[] { Math.BitDecrement(half), half, Math.BitIncrement(half) })
        {
            destination.Fill(0xa7);
            var sum = Vector512.Create(value * 16, 16d, 16d, 16d, 16d, 16d, 16d, 16d);
            Assert.False(Write(sum, 4, 4, destination));
            Assert.Equal(0xa7a7a7a7a7a7a7a7ul, BinaryPrimitives.ReadUInt64LittleEndian(destination));
        }
    }

    [Theory]
    [InlineData(15.999999, 4)]
    [InlineData(16, .499999999)]
    [InlineData(16, 16)]
    public void UncertainAlphaRefusesWithoutWriting(double alpha, double normalizer)
    {
        if (!Avx512F.IsSupported) return;
        Span<byte> destination = stackalloc byte[8];
        destination.Fill(0xa7);
        var sum = Vector512.Create(alpha, alpha, alpha, alpha, alpha, alpha, alpha, alpha);
        Assert.False(Write(sum, normalizer, normalizer, destination));
        Assert.Equal(0xa7a7a7a7a7a7a7a7ul, BinaryPrimitives.ReadUInt64LittleEndian(destination));
    }
}
