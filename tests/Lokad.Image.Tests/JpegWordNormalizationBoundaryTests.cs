using System.Runtime.InteropServices;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegWordNormalizationBoundaryTests
{
    private static void Expected(ReadOnlySpan<ushort> source, Span<int> output, uint? background)
    {
        const long denominator = 65535L * 65535;
        for (var i = 0; i < output.Length; i++)
        {
            var alpha = source[i * 4 + 3];
            if (alpha != 65535 && background is null) throw new ArgumentException("background required");
            var packed = 0;
            for (var c = 0; c < 3; c++)
            {
                var backdrop = (background ?? 0) >> ((2 - c) * 8) & 255;
                var numerator = source[i * 4 + c] * (long)alpha * 255 + backdrop * (65535 - alpha) * 65535L + denominator / 2;
                packed |= (int)(numerator / denominator) << (c * 8);
            }
            output[i] = packed;
        }
    }

    private static bool Invoke(bool expected, byte[] storage, int inputOffset, int count, int outputOffset, uint? color)
    {
        var source = MemoryMarshal.Cast<byte, ushort>(storage.AsSpan(inputOffset, count * 8));
        var output = MemoryMarshal.Cast<byte, int>(storage.AsSpan(outputOffset, count * 4));
        try
        {
            if (expected) Expected(source, output, color);
            else JpegReferenceColor.NormalizeRgba16(source, output, color);
            return true;
        }
        catch (ArgumentException) { return false; }
    }

    [Fact]
    public void AliasesUnalignedRowsAndMissingBackgroundPreserveExactWritePrefixes()
    {
        uint state = 0x497ca35d;
        foreach (var count in new[] { 0, 1, 2, 3, 4, 5, 17, 18, 33 })
        foreach (var inputOffset in new[] { 2, 3, 8 })
        foreach (var outputOffset in new[] { 0, 4, 9, 17, 32, 304 })
        foreach (var failAt in new[] { -2, -1, 0, count / 2, count - 1 })
        {
            var storage = new byte[1024];
            for (var i = 0; i < storage.Length; i++)
            { state ^= state << 13; state ^= state >> 17; state ^= state << 5; storage[i] = (byte)state; }
            var input = MemoryMarshal.Cast<byte, ushort>(storage.AsSpan(inputOffset, count * 8));
            for (var i = 0; i < count; i++) input[i * 4 + 3] = 65535;
            if (failAt >= 0 && failAt < count) input[failAt * 4 + 3] = 65534;
            var expected = storage.ToArray(); var actual = storage.ToArray(); uint? color = failAt == -2 ? 0x204060U : null;
            Assert.Equal(Invoke(true, expected, inputOffset, count, outputOffset, color), Invoke(false, actual, inputOffset, count, outputOffset, color));
            Assert.Equal(expected, actual);
        }
    }

    [Fact]
    public void WordNormalizationHasNoWarmOwnedAllocation()
    {
        var source = new ushort[80]; var output = new int[20];
        for (var i = 0; i < source.Length; i++) source[i] = (ushort)(i * 9973);
        for (var i = 0; i < 4096; i++) JpegReferenceColor.NormalizeRgba16(source, output, 0x204060);
        var start = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 4096; i++) JpegReferenceColor.NormalizeRgba16(source, output, 0x204060);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - start;
        Assert.Equal(0, allocated);
    }
}
