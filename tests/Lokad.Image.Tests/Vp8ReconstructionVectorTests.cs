using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class Vp8ReconstructionVectorTests
{
    [Fact]
    public void CompactPixelsMayOverlapAnyPartOfTheSingleCoefficientBlock()
    {
        var storage = new byte[96];
        for (var sample = 0; sample < 64; sample++)
        for (var pixelOffset = 0; pixelOffset <= 33; pixelOffset++)
        {
            for (var i = 0; i < storage.Length; i++) storage[i] = (byte)(sample * 73 + i * 29);
            var coefficients = MemoryMarshal.Cast<byte, short>(storage.AsSpan(17, 32)).ToArray();
            var expected = storage.ToArray();
            Vp8Transforms.InverseDct(coefficients);
            for (var i = 0; i < 16; i++)
                expected[pixelOffset + i] = (byte)Math.Clamp(expected[pixelOffset + i] + coefficients[i], 0, 255);
            Vp8BlockReconstruction.Add(MemoryMarshal.Cast<byte, short>(storage.AsSpan(17, 32)), storage.AsSpan(pixelOffset), 4);
            Assert.Equal(expected, storage);
        }
    }

    [Fact]
    public void OriginalSigned16DomainRetainsColumnWrapAndPixelClamp()
    {
        var initialPixels = new byte[144]; var pixels = new byte[144];
        var coefficients = new short[32]; Span<short> before = stackalloc short[32];
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        uint state = 0x9e3779b9; var controls = 0;
        for (var sample = 0; sample < 229504; sample++)
        {
            for (var i = 0; i < 32; i++)
            {
                short value;
                if (sample < 65536) value = unchecked((short)(sample - 32768));
                else if (sample < 131072) value = i % 16 == 0 ? unchecked((short)(sample - 98304)) : (short)0;
                else if (sample < 196608) value = unchecked((short)((i % 3 == 0 ? -1 : 1) * (sample - 163840)));
                else { state ^= state << 13; state ^= state >> 17; state ^= state << 5; value = unchecked((short)state); }
                coefficients[i] = value;
            }
            coefficients.CopyTo(before); var pair = (sample & 1) != 0; var width = pair ? 8 : 4;
            var stride = Math.Max(width, (sample % 4) switch { 0 => 4, 1 => 7, 2 => 16, _ => 31 }); var offset = sample % 8;
            initialPixels.AsSpan().Fill(0x4d);
            for (var y = 0; y < 4; y++) for (var x = 0; x < width; x++)
                initialPixels[offset + y * stride + x] = (byte)(sample * 13 + y * 29 + x * 7);
            initialPixels.CopyTo(pixels, 0);
            Vp8BlockReconstruction.Add(coefficients, pixels.AsSpan(offset), stride, pair);
            Assert.True(before.SequenceEqual(coefficients));
            hash.AppendData(pixels); controls++;
        }
        var alias = 0;
        foreach (var pair in new[] { false, true })
        foreach (var coefficientOffset in new[] { 0, 1, 7 })
        foreach (var pixelOffset in new[] { 0, 1, 3, 16, 31 })
        foreach (var stride in new[] { 8, 16, 31 })
        {
            var old = new byte[256]; var current = new byte[256];
            for (var i = 0; i < old.Length; i++) old[i] = unchecked((byte)(i * 17 + 13));
            old.CopyTo(current, 0); var count = pair ? 64 : 32;
            Vp8BlockReconstruction.Add(MemoryMarshal.Cast<byte, short>(current.AsSpan(coefficientOffset,count)),current.AsSpan(pixelOffset),stride,pair);
            hash.AppendData(current); alias++;
        }
        for (var i = 0; i < 4096; i++) Vp8BlockReconstruction.Add(coefficients,pixels,31,true);
        var allocationStart = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 4096; i++) Vp8BlockReconstruction.Add(coefficients,pixels,31,true);
        var allocation = GC.GetAllocatedBytesForCurrentThread() - allocationStart;
        Assert.Equal(229504, controls); Assert.Equal(90, alias); Assert.Equal(0, allocation);
        // Frozen from the original scalar implementation before SIMD integration.
        Assert.Equal("ac9e479291f0caff40d9ad5b96ac38f1db3075b8584e9c0ed5cfec048d26249e", Convert.ToHexStringLower(hash.GetHashAndReset()));
    }
}
