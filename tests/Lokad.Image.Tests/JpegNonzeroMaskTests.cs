using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegNonzeroMaskTests
{
    [Fact]
    public void EveryQuadrantPatternAndMixedSignedBlocksMatchIndependentScanGeometry()
    {
        var coefficients = new short[64];
        if (!Avx512BW.IsSupported)
        {
            Assert.Throws<PlatformNotSupportedException>(() => JpegNonzeroMask.Ac(coefficients));
            return; // Public packet tests exercise the scalar traversal on this hardware.
        }
        void Check()
        {
            ulong expected = 0;
            for (var k = 1; k < 64; k++) if (coefficients[JpegTransform.Zigzag[k]] != 0) expected |= 1UL << k;
            Assert.Equal(expected, JpegNonzeroMask.Ac(coefficients));
        }
        Check();
        foreach (var value in new short[] { short.MinValue, -1, 1, short.MaxValue })
        for (var position = 0; position < 64; position++)
        {
            Array.Clear(coefficients); coefficients[position] = value; Check();
        }
        for (var group = 0; group < 4; group++)
        for (var mask = 0; mask < 65536; mask++)
        {
            Array.Clear(coefficients);
            for (var bit = 0; bit < 16; bit++)
                if ((mask & 1 << bit) != 0) coefficients[group * 16 + bit] = (short)(bit % 2 == 0 ? bit + 1 : -bit - 1);
            Check();
        }
        uint state = 0x9e3779b9;
        for (var sample = 0; sample < 32768; sample++)
        {
            for (var position = 0; position < 64; position++)
            {
                state ^= state << 13; state ^= state >> 17; state ^= state << 5;
                coefficients[position] = (state & 7) < sample % 9 ? (short)((int)(state >> 8) % 65535 - 32767) : (short)0;
            }
            Check();
        }
    }

    [Fact]
    public void UnalignedBorrowedBlockKeepsTailsAndHasZeroWarmAllocation()
    {
        var storage = new short[67]; storage[0] = 30000; storage[65] = -30000; storage[66] = 12345;
        for (var p = 0; p < 64; p++) storage[p + 1] = (short)(p % 3 == 0 ? p * 17 - 2000 : 0);
        var saved = storage.ToArray();
        if (!Avx512BW.IsSupported)
        {
            Assert.Throws<PlatformNotSupportedException>(() => JpegNonzeroMask.Ac(storage.AsSpan(1)));
            Assert.Equal(saved, storage); return;
        }
        var expected = JpegNonzeroMask.Ac(storage.AsSpan(1, 64));
        Assert.Equal(expected, JpegNonzeroMask.Ac(storage.AsSpan(1)));
        for (var i = 0; i < 256; i++) JpegNonzeroMask.Ac(storage.AsSpan(1, 64));
        var before = GC.GetAllocatedBytesForCurrentThread(); ulong observed = 0;
        for (var i = 0; i < 10001; i++) observed ^= JpegNonzeroMask.Ac(storage.AsSpan(1, 64));
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
        Assert.Equal(expected, observed); Assert.Equal(saved, storage);
        Assert.ThrowsAny<ArgumentException>(() => JpegNonzeroMask.Ac(storage.AsSpan(1, 63)));
    }
}
