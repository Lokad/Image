using System.Buffers.Binary;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8DistortionTests
{
    [Fact]
    public void OriginalByteDomainRetainsEverySpectralMagnitudeAndFrozenWeightedEnergy()
    {
        var pixels = new byte[128]; var zero = new byte[16];
        Span<ushort> weights = stackalloc ushort[16]; Vp8Distortion.TextureWeights(weights);
        Span<ushort> isolatedWeights = stackalloc ushort[16];
        Span<byte> packed = stackalloc byte[4]; uint state = 0x9e3779b9;
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        for (var sample = 0; sample < 102656; sample++)
        {
            var stride = (sample % 4) switch { 0 => 4, 1 => 7, 2 => 16, _ => 31 }; var offset = sample % 4;
            pixels.AsSpan().Fill(0x4d);
            for (var y = 0; y < 4; y++) for (var x = 0; x < 4; x++)
            {
                byte value;
                if (sample < 256) value = (byte)sample;
                else if (sample < 4352)
                {
                    var impulse = sample - 256; value = y * 4 + x == impulse / 256 ? (byte)(impulse % 256) : (byte)0;
                }
                else if (sample < 69888) value = (byte)((((sample - 4352) >> (y * 4 + x)) & 1) * 255);
                else { state ^= state << 13; state ^= state >> 17; state ^= state << 5; value = (byte)state; }
                pixels[offset + y * stride + x] = value;
            }
            var before = PixelChecksum(pixels); var input = pixels.AsSpan(offset); var energy = 0;
            for (var v = 0; v < 4; v++) for (var u = 0; u < 4; u++)
            {
                var expected = 0;
                for (var y = 0; y < 4; y++) for (var x = 0; x < 4; x++)
                    expected += SequencySign(v, y) * SequencySign(u, x) * input[y * stride + x];
                isolatedWeights.Clear(); isolatedWeights[v * 4 + u] = 256;
                // A zero second block and one weight of256 expose the complete
                // coefficient magnitude through the existing metric, without
                // losing any bits to its final division by32.
                var magnitude = Vp8Distortion.Texture4(input, stride, zero, 4, isolatedWeights);
                Assert.Equal(Math.Abs(expected) * 8, magnitude);
                energy += weights[v * 4 + u] * (magnitude / 8);
            }
            Assert.Equal(energy >> 5, Vp8Distortion.Texture4(input, stride, zero, 4, weights));
            Assert.Equal(before, PixelChecksum(pixels));
            BinaryPrimitives.WriteInt32LittleEndian(packed, energy); hash.AppendData(packed);
        }
        // Frozen from the independently qualified matrix before factorization:
        //256constants,4096impulses,all65536endpoint masks and32768seeded blocks.
        Assert.Equal("68204ba3a00ef287e008c94b17e01fd3a0e3a65bb1e8231cdbce139e55abec8d",
            Convert.ToHexStringLower(hash.GetHashAndReset()));
    }

    [Fact]
    public void TextureMetricsAllocateNoManagedStorage()
    {
        var a = new byte[256]; var b = new byte[256]; var weights = new ushort[16];
        for (var i = 0; i < a.Length; i++) { a[i] = (byte)(i * 13); b[i] = (byte)(i * 17 + 3); }
        Vp8Distortion.TextureWeights(weights); var result = 0;
        for (var i = 0; i < 1024; i++) result ^= Vp8Distortion.Texture16(a, 16, b, 16, weights);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 1024; i++) result ^= Vp8Distortion.Texture16(a, 16, b, 16, weights);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.Equal(0, result); Assert.Equal(0, allocated);
    }

    private static int SequencySign(int frequency, int position)
    {
        var gray = frequency ^ (frequency >> 1); var mask = (gray >> 1) | ((gray & 1) << 1);
        var bits = mask & position; return ((bits ^ (bits >> 1)) & 1) == 0 ? 1 : -1;
    }

    private static uint PixelChecksum(ReadOnlySpan<byte> pixels)
    {
        uint result = 0; foreach (var value in pixels) result = unchecked(result * 31 + value); return result;
    }

    private static void Predict(ReadOnlySpan<byte> plane, int stride, int x, int y, int size, Span<byte> output)
    {
        ReadOnlySpan<byte> top = y == 0 ? default : plane.Slice((y - 1) * stride + x, size);
        Span<byte> left = stackalloc byte[16]; left = left[..size];
        if (x == 0) left = default;
        else for (var p = 0; p < size; p++) left[p] = plane[(y + p) * stride + x - 1];
        var corner = x != 0 && y != 0 ? plane[(y - 1) * stride + x - 1] : (byte)0;
        Vp8BlockPrediction.Fill(size, Vp8IntraMode.TrueMotion, top, left, corner, output);
    }

    private static void Append(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b, ReadOnlySpan<ushort> weights, IncrementalHash hash)
    {
        Span<int> results = stackalloc int[35];
        results[0] = Vp8Distortion.Squared(a, 32, b, 32, 16, 16);
        results[1] = Vp8Distortion.Squared(a[16..], 32, b[16..], 32, 16, 8);
        results[2] = Vp8Distortion.Texture16(a, 32, b, 32, weights);
        var at = 3;
        for (var y = 0; y < 16; y += 4) for (var x = 0; x < 16; x += 4)
        {
            var aa = a[(y * 32 + x)..]; var bb = b[(y * 32 + x)..];
            results[at++] = Vp8Distortion.Squared(aa, 32, bb, 32, 4, 4);
            results[at++] = Vp8Distortion.Texture4(aa, 32, bb, 32, weights);
        }
        Span<byte> packed = stackalloc byte[140];
        for (var p = 0; p < 35; p++) BinaryPrimitives.WriteInt32LittleEndian(packed[(p * 4)..], results[p]);
        hash.AppendData(packed);
    }

    [Fact]
    public void AnalyticImportanceGenerationMatchesNativeProfileWithoutAnImportedTable()
    {
        Span<ushort> weights = stackalloc ushort[18]; weights[16] = 1234; weights[17] = 5678;
        Vp8Distortion.TextureWeights(weights); Span<byte> packed = stackalloc byte[32]; var sum = 0;
        for (var p = 0; p < 16; p++)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(packed[(p * 2)..], weights[p]); sum += weights[p];
            Assert.Equal(weights[p], weights[(p % 4) * 4 + p / 4]);
        }
        Assert.Equal(256, sum); Assert.Equal((1234, 5678), ((int)weights[16], (int)weights[17]));
        Assert.Equal("f7621921249116ae49950a4cf74d1c3df4bbb8167542a7a0c01f5d07fb461fac", Convert.ToHexStringLower(SHA256.HashData(packed)));
    }

    [Fact]
    public void EveryNativeMetricMatchesOnAllOpaqueMacroblocksAndTheirFrozenTrueMotionPredictions()
    {
        using var root = new TestMemoryBudget(16_000_000); using var image = OriginalVp8lControls.Buffer("opaque", 8, root);
        using var context = new OperationContext(new() { MemoryBudget = root }, default); using var input = Vp8PreparedInput.Create(image, context);
        var retained = root.Bytes; var original = Vp8PreparedInputTests.Hash(input);
        Span<byte> a = stackalloc byte[512]; Span<byte> b = stackalloc byte[512];
        Span<byte> yPred = stackalloc byte[256]; Span<byte> uPred = stackalloc byte[64]; Span<byte> vPred = stackalloc byte[64];
        Span<ushort> weights = stackalloc ushort[16]; Vp8Distortion.TextureWeights(weights);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256); var pairs = 0;
        for (var my = 0; my < input.StorageHeight; my += 16) for (var mx = 0; mx < input.Stride; mx += 16)
        {
a.Fill(0x4d); b.Fill(0x4d);
            Predict(input.Y, input.Stride, mx, my, 16, yPred);
            Predict(input.U, input.Stride / 2, mx / 2, my / 2, 8, uPred);
            Predict(input.V, input.Stride / 2, mx / 2, my / 2, 8, vPred);
            for (var y = 0; y < 16; y++)
            {
                input.Y.Slice((my + y) * input.Stride + mx, 16).CopyTo(a.Slice(y * 32, 16));
                yPred.Slice(y * 16, 16).CopyTo(b.Slice(y * 32, 16));
            }
            for (var y = 0; y < 8; y++)
            {
                input.U.Slice((my / 2 + y) * (input.Stride / 2) + mx / 2, 8).CopyTo(a.Slice(y * 32 + 16, 8));
                input.V.Slice((my / 2 + y) * (input.Stride / 2) + mx / 2, 8).CopyTo(a.Slice(y * 32 + 24, 8));
                uPred.Slice(y * 8, 8).CopyTo(b.Slice(y * 32 + 16, 8)); vPred.Slice(y * 8, 8).CopyTo(b.Slice(y * 32 + 24, 8));
            }
            Append(a, b, weights, hash); pairs++;
        }
        Assert.Equal(561, pairs); Assert.Equal("7c3f8a52a08902cbe0d01a00b4e678229202d071f119af499977b2d15c1b8e1d", Convert.ToHexStringLower(hash.GetHashAndReset()));
        Assert.Equal(original, Vp8PreparedInputTests.Hash(input)); Assert.Equal(retained, root.Bytes);
    }

    private static void Fragment(int fragment, Span<byte> a, Span<byte> b)
    {
        for (var y = 0; y < 16; y++) for (var x = 0; x < 32; x++)
        {
            var value = fragment switch
            {
                2 => ((x + y) & 1) * 255,
                3 => ((x / 4 + y / 4) & 1) * 255,
                4 => (13 * x + 29 * y + 3 * x * y) & 255,
                _ => 0
            };
            a[y * 32 + x] = (byte)value;
            b[y * 32 + x] = fragment switch { 0 => 0, 4 => (byte)((value + 1) & 255), _ => (byte)(255 - value) };
        }
    }

    [Theory]
    [InlineData(0, "24045c10c12a89f4c11e3b88ea34558fcdf926a8c1008cd08cc33bc71407c774")]
    [InlineData(1, "34bde35a19fc05f9e218eb0fa78acc2f98d32b89a19437976ce07ec95faba107")]
    [InlineData(2, "6bdb26145b46a9433c955fef6ea74103a5c4d182081c6599b55d6902052cb0f1")]
    [InlineData(3, "34bde35a19fc05f9e218eb0fa78acc2f98d32b89a19437976ce07ec95faba107")]
    [InlineData(4, "073370e75abc22175eca88dbba91cf573903754297aa8f17ba40a68d80076609")]
    public void ZeroMaxRangeSpectralSymmetryAndPerTileSignedAccumulationMatchNative(int fragment, string expected)
    {
        Span<byte> a = stackalloc byte[512]; Span<byte> b = stackalloc byte[512]; Fragment(fragment, a, b);
        var beforeA = a.ToArray(); var beforeB = b.ToArray(); Span<ushort> weights = stackalloc ushort[16]; Vp8Distortion.TextureWeights(weights);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256); Append(a, b, weights, hash);
        Assert.Equal(expected, Convert.ToHexStringLower(hash.GetHashAndReset()));
        Assert.True(a.SequenceEqual(beforeA)); Assert.True(b.SequenceEqual(beforeB));
        Assert.Equal(Vp8Distortion.Texture16(a, 32, b, 32, weights), Vp8Distortion.Texture16(b, 32, a, 32, weights));
        Assert.Equal(Vp8Distortion.Squared(a, 32, b, 32, 16, 16), Vp8Distortion.Squared(b, 32, a, 32, 16, 16));
        if (fragment is 1 or 3) Assert.Equal(77_520, Vp8Distortion.Texture16(a, 32, b, 32, weights));
        if (fragment == 2) { Assert.Equal(16_646_400, Vp8Distortion.Squared(a, 32, b, 32, 16, 16)); Assert.Equal(0, Vp8Distortion.Texture16(a, 32, b, 32, weights)); }
    }

    [Fact]
    public void IndependentCompactStridesUseTheSameNativeFourByFourBoundary()
    {
        Span<byte> a = stackalloc byte[512]; Span<byte> b = stackalloc byte[512]; Fragment(4, a, b);
        Span<byte> compactA = stackalloc byte[28]; Span<byte> compactB = stackalloc byte[36]; compactA.Fill(73); compactB.Fill(74);
        for (var y = 0; y < 4; y++) { a.Slice(y * 32, 4).CopyTo(compactA.Slice(y * 7, 4)); b.Slice(y * 32, 4).CopyTo(compactB.Slice(y * 9, 4)); }
        Span<ushort> weights = stackalloc ushort[16]; Vp8Distortion.TextureWeights(weights);
        Assert.Equal(16, Vp8Distortion.Squared(compactA, 7, compactB, 9, 4, 4));
        Assert.Equal(19, Vp8Distortion.Texture4(compactA, 7, compactB, 9, weights));
    }

    [Fact]
    public void InvalidLayoutsAndImportanceAreRejectedWithoutChangingCallerStorage()
    {
        var pixels = new byte[256]; var weights = new ushort[18]; Array.Fill(weights, (ushort)73);
        Assert.Throws<ArgumentException>(() => Vp8Distortion.TextureWeights(weights.AsSpan(0, 15))); Assert.All(weights, v => Assert.Equal((ushort)73, v));
        Vp8Distortion.TextureWeights(weights);
        Assert.Throws<ArgumentException>(() => Vp8Distortion.Squared(pixels, -1, pixels, 16, 16, 16));
        Assert.Throws<ArgumentException>(() => Vp8Distortion.Squared(pixels, int.MaxValue, pixels, 16, 16, 16));
        Assert.Throws<ArgumentException>(() => Vp8Distortion.Texture4(pixels, 16, pixels, 16, weights.AsSpan(0, 15)));
        weights[0]++; Assert.Throws<ArgumentException>(() => Vp8Distortion.Texture16(pixels, 16, pixels, 16, weights));
        Assert.All(pixels, v => Assert.Equal((byte)0, v));
    }
}
