using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegNaturalSupportTests
{
    [Fact]
    public void EveryNaturalWordIncludingDcHasExactlyItsOwnBit()
    {
        var words = new short[64];
        if (!Avx2.IsSupported && !Avx512BW.IsSupported)
        {
            Assert.Throws<PlatformNotSupportedException>(() => JpegNonzeroMask.Natural(words));
            return;
        }
        Assert.Equal(0UL, JpegNonzeroMask.Natural(words));
        foreach (var value in new short[] { 1, -1, short.MinValue, short.MaxValue, 256, -256 })
        for (var i = 0; i < 64; i++)
        {
            words[i] = value;
            Assert.Equal(1UL << i, JpegNonzeroMask.Natural(words));
            words[i] = 0;
        }
        words.AsSpan().Fill(-1); Assert.Equal(ulong.MaxValue, JpegNonzeroMask.Natural(words));
    }

    [Fact]
    public void NaturalSupportMatchesScalarWordsAndPreservesUnalignedInput()
    {
        if (!Avx2.IsSupported && !Avx512BW.IsSupported) return;
        var storage = new short[81]; var words = storage.AsSpan(1, 64);
        storage[0] = 123; storage.AsSpan(65).Fill(-123);
        var random = new Random(0x787);
        for (var trial = 0; trial < 4000; trial++)
        {
            ulong expected = 0;
            for (var i = 0; i < 64; i++)
            {
                words[i] = trial % 8 == 0 || random.Next(4) == 0
                    ? (short)0 : (short)random.Next(short.MinValue, short.MaxValue + 1);
                if (words[i] != 0) expected |= 1UL << i;
            }
            var before = storage.ToArray();
            Assert.Equal(expected, JpegNonzeroMask.Natural(storage.AsSpan(1)));
            Assert.Equal(before, storage);
        }
        words.Clear(); words[0] = 7; words[8] = -7;
        Assert.Equal(0x101UL, JpegNonzeroMask.Natural(words));
        if (Avx512BW.IsSupported) Assert.Equal(4UL, JpegNonzeroMask.Ac(words));
        var allocated = GC.GetAllocatedBytesForCurrentThread(); ulong observed = 0;
        for (var i = 0; i < 10001; i++) observed ^= JpegNonzeroMask.Natural(words);
        var bytes = GC.GetAllocatedBytesForCurrentThread() - allocated;
        Assert.Equal(0L, bytes); Assert.Equal(0x101UL, observed);
    }
}
