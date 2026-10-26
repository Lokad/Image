using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8BlockReconstructionTests
{
    private static void Pixels(Span<byte> pixels, int stride, int width, int block)
    {
        pixels.Fill(0xa7);
        for (var y = 0; y < 4; y++) for (var x = 0; x < width; x++) pixels[y * stride + x] = (byte)(29 * block + 73 * x + 41 * y);
    }

    private static void AppendPixels(ReadOnlySpan<short> coefficients, int block, IncrementalHash hash, bool pair = false, int stride = 32)
    {
        Span<byte> pixels = stackalloc byte[4 * 32 + 3]; pixels = pixels[..(4 * stride + 3)];
        Span<short> original = stackalloc short[32]; coefficients.CopyTo(original);
        var width = pair ? 8 : 4; Pixels(pixels, stride, width, block);
        Vp8BlockReconstruction.Add(coefficients, pixels, stride, pair);
        Assert.True(coefficients.SequenceEqual(original[..coefficients.Length]));
        var padding = true;
        for (var y = 0; y < 4; y++)
        {
            hash.AppendData(pixels.Slice(y * stride, width));
            for (var x = width; x < stride; x++) padding &= pixels[y * stride + x] == 0xa7;
        }
        Assert.True(padding); Assert.True(pixels[(4 * stride)..].SequenceEqual(new byte[] { 0xa7, 0xa7, 0xa7 }));
    }

    private static void AppendDc(ReadOnlySpan<short> coefficients, IncrementalHash hash)
    {
        Span<short> blocks = stackalloc short[257]; blocks.Fill(0x5a5a);
        Span<short> original = stackalloc short[16]; coefficients[..16].CopyTo(original);
        Vp8BlockReconstruction.WriteWalshDc(coefficients, blocks);
        Assert.True(coefficients[..16].SequenceEqual(original));
        Span<byte> bytes = stackalloc byte[32];
        for (var p = 0; p < 16; p++) BinaryPrimitives.WriteInt16LittleEndian(bytes[(2 * p)..], blocks[p * 16]);
        hash.AppendData(bytes); var preserved = true;
        for (var p = 0; p < blocks.Length; p++) if (p % 16 != 0 || p == 256) preserved &= blocks[p] == 0x5a5a;
        Assert.True(preserved);
    }

    private static int Plane(ReadOnlySpan<byte> source, int stride, Vp8QuantizationMatrix matrix,
        IncrementalHash hash, int block, OperationContext context)
    {
        Span<short> residual = stackalloc short[16]; Span<short> coefficients = stackalloc short[16]; Span<short> levels = stackalloc short[16];
        for (var y = 0; y < source.Length / stride; y += 4)
        for (var x = 0; x < stride; x += 4)
        {
            for (var dy = 0; dy < 4; dy++) for (var dx = 0; dx < 4; dx++) residual[dy * 4 + dx] = (short)(source[(y + dy) * stride + x + dx] - 128);
            Vp8ForwardTransform.Dct(residual, coefficients); Vp8BlockQuantization.Apply(coefficients, levels, matrix);
            AppendPixels(coefficients, block++, hash);
        }
        return block;
    }

    [Theory]
    [InlineData(0, 8976, "eca643ce229746671e540d3b173073ac959118b1d8393d1048a16b00ce398cde")]
    [InlineData(2, 4488, "58c20cb0748d282b53eee3fe49f0183ce2284cf23cad51ccee0e9cb6dc1d3d9f")]
    [InlineData(1, 561, "2adc8ca12e0938b1c2a1a5bf75e73e96af7d5c6482fd1844c554f89ff2641649")]
    public void AllNativeRasterPixelsAndSparseWalshDcMatchUsingExistingInverseTransforms(int plane, int expectedBlocks, string expected)
    {
        using var root = new TestMemoryBudget(16_000_000); using var image = OriginalVp8lControls.Buffer("opaque", 8, root);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var input = Vp8PreparedInput.Create(image, context); var original = Vp8PreparedInputTests.Hash(input);
        using var analysis = Vp8SourceAnalysis.Create(input, context); using var segments = Vp8SourceSegments.Create(analysis, 75, false, context);
        using var plan = Vp8QuantizationPlan.Create(segments, analysis.MeanChromaScore, context); var retained = root.Bytes;
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256); var matrix = plan.Matrix(0, (Vp8CoefficientPlane)plane); var blocks = 0;
        if (plane == 0) blocks = Plane(input.Y, input.Stride, matrix, hash, 0, context);
        else if (plane == 2)
        {
            blocks = Plane(input.U, input.Stride / 2, matrix, hash, 0, context);
            blocks = Plane(input.V, input.Stride / 2, matrix, hash, blocks, context);
        }
        else
        {
            Span<short> residual = stackalloc short[16]; Span<short> dc = stackalloc short[16]; Span<short> dct = stackalloc short[16];
            Span<short> coefficients = stackalloc short[16]; Span<short> levels = stackalloc short[16];
            for (var my = 0; my < input.StorageHeight; my += 16) for (var mx = 0; mx < input.Stride; mx += 16)
            {
                for (var by = 0; by < 16; by += 4) for (var bx = 0; bx < 16; bx += 4)
                {
                    for (var dy = 0; dy < 4; dy++) for (var dx = 0; dx < 4; dx++) residual[dy * 4 + dx] = (short)(input.Y[(my + by + dy) * input.Stride + mx + bx + dx] - 128);
                    Vp8ForwardTransform.Dct(residual, dct); dc[by / 4 * 4 + bx / 4] = dct[0];
                }
                Vp8ForwardTransform.WalshDc(dc, coefficients); Vp8BlockQuantization.Apply(coefficients, levels, matrix); AppendDc(coefficients, hash); blocks++;
            }
        }
        Assert.Equal(expectedBlocks, blocks); Assert.Equal(expected, Convert.ToHexStringLower(hash.GetHashAndReset()));
        Assert.Equal(original, Vp8PreparedInputTests.Hash(input)); Assert.Equal(retained, root.Bytes);
    }

    [Theory]
    [InlineData(false, "f5c967757fa26fc45d83bd675b4bbb4a9cda5ef67ffef012fc6591a4fab58ee9")]
    [InlineData(true, "2b6deec89cf9f7f4e8dbda1f8cde88137a0229e361791541f037ce9753668c4e")]
    public void ExistingValidSignedForwardFragmentsReconstructExactly(bool walsh, string expected)
    {
        Span<short> source = stackalloc short[16]; Span<short> coefficients = stackalloc short[16];
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        for (var fragment = 0; fragment < 36; fragment++)
        {
            for (var p = 0; p < 16; p++) source[p] = (short)(fragment switch
            {
                1 => walsh ? 2040 : 255, 2 => walsh ? -2040 : -255,
                3 => (p / 4 + p % 4) % 2 == 0 ? walsh ? 2040 : 255 : walsh ? -2040 : -255,
                >= 4 when p == (fragment - 4) / 2 => walsh ? (fragment % 2 == 0 ? 1 : -1) : (fragment % 2 == 0 ? 255 : -255),
                _ => 0
            });
            if (walsh) { Vp8ForwardTransform.WalshDc(source, coefficients); AppendDc(coefficients, hash); }
            else { Vp8ForwardTransform.Dct(source, coefficients); AppendPixels(coefficients, fragment, hash); }
        }
        Assert.Equal(expected, Convert.ToHexStringLower(hash.GetHashAndReset()));
    }

    [Theory]
    [InlineData(32)] [InlineData(9)]
    public void PairedNativeBoundaryMatchesWithNativeOrCompactManagedStride(int stride)
    {
        using var image = OriginalVp8lControls.Buffer("opaque", 8); using var context = new OperationContext(ImageLimits.Default, default);
        using var input = Vp8PreparedInput.Create(image, context); using var analysis = Vp8SourceAnalysis.Create(input, context);
        using var segments = Vp8SourceSegments.Create(analysis, 75, false, context); using var plan = Vp8QuantizationPlan.Create(segments, analysis.MeanChromaScore, context);
        Span<short> residual = stackalloc short[16]; Span<short> coefficients = stackalloc short[32]; Span<short> levels = stackalloc short[16];
        for (var block = 0; block < 2; block++)
        {
            for (var y = 0; y < 4; y++) for (var x = 0; x < 4; x++) residual[y * 4 + x] = (short)(input.Y[y * input.Stride + block * 4 + x] - 128);
            var c = coefficients.Slice(block * 16, 16); Vp8ForwardTransform.Dct(residual, c); Vp8BlockQuantization.Apply(c, levels, plan.Matrix(0, Vp8CoefficientPlane.Luma));
        }
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256); AppendPixels(coefficients, 0, hash, true, stride);
        Assert.Equal("a3d6cec674d728c086b82d9902f5f6a50bfed7c10f29de875fb101058e3231b3", Convert.ToHexStringLower(hash.GetHashAndReset()));
    }

    [Fact]
    public void PairedPixelOutputMayAliasBothCoefficientBlocksWithoutFeedback()
    {
        var storage = new short[80]; storage[0] = 64; storage[16] = -64;
        var coefficients = storage.AsSpan(0, 32).ToArray(); var expected = MemoryMarshal.AsBytes(storage.AsSpan()).ToArray();
        Vp8BlockReconstruction.Add(coefficients, expected.AsSpan(0, 131), 32, true);
        Vp8BlockReconstruction.Add(storage.AsSpan(0, 32), MemoryMarshal.AsBytes(storage.AsSpan())[..131], 32, true);
        Assert.Equal(expected, MemoryMarshal.AsBytes(storage.AsSpan()).ToArray());
    }

    [Fact]
    public void SparseWalshOutputMayAliasItsSourceAndPreservesAllOtherSlots()
    {
        var storage = Enumerable.Range(0, 257).Select(p => (short)(p * 7 - 55)).ToArray(); var expected = storage.ToArray();
        var dc = storage.AsSpan(3, 16).ToArray(); Vp8Transforms.InverseWalsh(dc);
        for (var p = 0; p < 16; p++) expected[p * 16] = dc[p];
        Vp8BlockReconstruction.WriteWalshDc(storage.AsSpan(3, 16), storage); Assert.Equal(expected, storage);
    }

    [Fact]
    public void IncompleteOrOverflowingLayoutsFailBeforeChangingCallerStorage()
    {
        var coefficients = new short[32]; var pixels = Enumerable.Repeat((byte)91, 131).ToArray();
        Assert.Throws<ArgumentException>(() => Vp8BlockReconstruction.Add(coefficients.AsSpan(0, 15), pixels, 32));
        Assert.Throws<ArgumentException>(() => Vp8BlockReconstruction.Add(coefficients, pixels, 3));
        Assert.Throws<ArgumentException>(() => Vp8BlockReconstruction.Add(coefficients, pixels, int.MaxValue, true));
        Assert.Throws<ArgumentException>(() => Vp8BlockReconstruction.Add(coefficients, pixels.AsSpan(0, 103), 32, true));
        Assert.All(pixels, value => Assert.Equal((byte)91, value));
        var blocks = Enumerable.Repeat((short)73, 241).ToArray();
        Assert.Throws<ArgumentException>(() => Vp8BlockReconstruction.WriteWalshDc(coefficients.AsSpan(0, 15), blocks));
        Assert.Throws<ArgumentException>(() => Vp8BlockReconstruction.WriteWalshDc(coefficients, blocks.AsSpan(0, 240)));
        Assert.All(blocks, value => Assert.Equal((short)73, value));
    }
}
