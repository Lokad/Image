using System.Buffers.Binary;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8SourceHistogramTests
{
    [Fact]
    public void OverlappingPopulationPreservesSequentialCoefficientFeedback()
    {
        var population = new int[33]; population[0] = 16; population[2] = 7; population[32] = 147;
        var coefficients = System.Runtime.InteropServices.MemoryMarshal.Cast<int, short>(population.AsSpan())[..16];
        Vp8SourceHistogram.AddBlock(coefficients, population);
        var expected = new int[33]; expected[0] = 30; expected[1] = 1; expected[2] = 8; expected[32] = 147;
        Assert.Equal(expected, population);
    }

    // Peak/last are directly observed native summaries. Raw scores freeze the
    // separately inspected scalar analysis policy, including uncapped values.
    public static IEnumerable<object[]> Populations()
    {
        yield return [0, 0, 1, 0];
        yield return [1, 256, 0, 0];
        yield return [2, 1, 31, 0];
        yield return [3, 2, 31, 7905];
        yield return [4, 2, 31, 7905];
        yield return [5, 9, 31, 1756];
        yield return [6, 1, 2, 0];
        yield return [7, 3, 29, 4930];
        yield return [8, 200, 31, 79];
        yield return [9, 1, 0, 0];
    }

    [Theory]
    [MemberData(nameof(Populations))]
    public void NativePopulationShapesAndUnclampedScoreMatch(int probe, int peak, int last, int score)
    {
        var counts = new int[34]; counts[32] = 123; counts[33] = 456;
        switch (probe)
        {
            case 1: counts[0] = 256; break;
            case 2: counts.AsSpan(0, 32).Fill(1); break;
            case 3: counts[31] = 2; break;
            case 4: counts[0] = 2; counts[31] = 1; break;
            case 5: counts[0] = counts[31] = 9; break;
            case 6: counts[2] = 1; break;
            case 7: counts[7] = counts[29] = 3; break;
            case 8: counts[1] = 200; counts[31] = 1; break;
            case 9: counts[0] = 1; break;
        }
        var original = counts.ToArray(); var shape = Vp8SourceHistogram.Describe(counts);
        Assert.Equal((peak, last, score), (shape.PeakPopulation, shape.LastBin, shape.RawScore));
        Assert.Equal(original, counts);
    }

    [Fact]
    public void SignedMagnitudeBucketsAccumulateAndPreserveBorrowedBuffersAndTails()
    {
        short[] coefficients = [0, 7, -7, 8, -8, 15, -15, 239, -239, 240, -240, 248, -248, short.MaxValue, short.MinValue, 0, 77];
        var original = coefficients.ToArray(); var counts = new int[33]; counts[32] = 147;
        Vp8SourceHistogram.AddBlock(coefficients, counts);
        var expected = new int[33]; expected[0] = expected[1] = expected[31] = 4; expected[29] = expected[30] = 2; expected[32] = 147;
        Assert.Equal(expected, counts); Assert.Equal(original, coefficients);
        Assert.Equal(new(4, 31), Vp8SourceHistogram.Describe(counts));
        Assert.Equal(3952, Vp8SourceHistogram.Describe(counts).RawScore);
        Vp8SourceHistogram.AddBlock(coefficients, counts);
        for (var i = 0; i < 32; i++) expected[i] *= 2;
        Assert.Equal(expected, counts); Assert.Equal(1976, Vp8SourceHistogram.Describe(counts).RawScore);
    }

    private static void AddPlane(ReadOnlySpan<byte> plane, int stride, int left, int top, int blocks, Span<int> counts)
    {
        Span<short> residual = stackalloc short[16]; Span<short> coefficients = stackalloc short[16];
        for (var by = 0; by < blocks; by++)
        for (var bx = 0; bx < blocks; bx++)
        {
            for (var y = 0; y < 4; y++)
            for (var x = 0; x < 4; x++) residual[y * 4 + x] = (short)(plane[(top + by * 4 + y) * stride + left + bx * 4 + x] - 128);
            Vp8ForwardTransform.Dct(residual, coefficients); Vp8SourceHistogram.AddBlock(coefficients, counts);
        }
    }

    private static void Append(Vp8SourceHistogram.Shape shape, IncrementalHash hash)
    {
        Span<byte> bytes = stackalloc byte[8]; BinaryPrimitives.WriteInt32LittleEndian(bytes, shape.PeakPopulation);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[4..], shape.LastBin); hash.AppendData(bytes);
    }

    [Fact]
    public void AllOpaqueControlLumaAndUvSummariesMatchNativeHistogramEntry()
    {
        using var root = new TestMemoryBudget(16_000_000); using var source = OriginalVp8lControls.Buffer("opaque", 8, root);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var input = Vp8PreparedInput.Create(source, context); input.CleanupTransparentArea();
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        Span<int> counts = stackalloc int[32]; var summaries = 0; var original = Vp8PreparedInputTests.Hash(input); var retained = root.Bytes;
        for (var my = 0; my < input.StorageHeight; my += 16)
        for (var mx = 0; mx < input.Stride; mx += 16)
        {
            counts.Clear(); AddPlane(input.Y, input.Stride, mx, my, 4, counts);
            Append(Vp8SourceHistogram.Describe(counts), hash); summaries++;
            counts.Clear(); AddPlane(input.U, input.Stride / 2, mx / 2, my / 2, 2, counts);
            AddPlane(input.V, input.Stride / 2, mx / 2, my / 2, 2, counts);
            Append(Vp8SourceHistogram.Describe(counts), hash); summaries++;
        }
        Assert.Equal(1122, summaries);
        Assert.Equal("0a5a98f718855df77c8698fd638252fa89506ae194f633569959f267f0655371", Convert.ToHexStringLower(hash.GetHashAndReset()));
        Assert.Equal(original, Vp8PreparedInputTests.Hash(input)); Assert.Equal(retained, root.Bytes);
    }
}
