using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegCompiledColorTests
{
    [Fact]
    public void EveryByteRgbAndPairedBoundaryPreservesFrozenMatrixBytesWithoutWarmStorage()
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var encoded = new int[8192]; var count = 0; long triples = 0, paired = 0;
        for (var r = 0; r < 256; r++)
        for (var g = 0; g < 256; g++)
        for (var b = 0; b < 256; b++)
        {
            JpegReferenceColor.Convert(r, g, b, out var y, out var u, out var v);
            if (JpegReferenceColor.Luma(r, g, b) != y) Assert.Fail("Luma and full conversion differ.");
            JpegReferenceColor.LimitedChroma(r, g, b, false, out var cb, out var cr);
            encoded[count++] = y | u << 8 | v << 16; encoded[count++] = cb | cr << 16; triples++;
            FlushFull();
        }
        int[] edges = [0, 1, 2, 3, 254, 255, 256, 257, 508, 509, 510];
        foreach (var r in edges) foreach (var g in edges) foreach (var b in edges) Pair(r, g, b);
        uint state = 0x53e17ca9;
        for (var i = 0; i < 65536; i++) Pair(Next(), Next(), Next());
        int Next() { state ^= state << 13; state ^= state >> 17; state ^= state << 5; return (int)(state % 511); }
        void Pair(int r, int g, int b)
        {
            JpegReferenceColor.LimitedChroma(r, g, b, true, out var cb, out var cr);
            encoded[count++] = cb | cr << 16; paired++; FlushFull();
        }
        void FlushFull()
        {
            if (count != encoded.Length) return;
            hash.AppendData(MemoryMarshal.AsBytes(encoded.AsSpan())); count = 0;
        }
        hash.AppendData(MemoryMarshal.AsBytes(encoded.AsSpan(0, count)));
        var checksum = 0;
        for (var i = 0; i < 4096; i++) checksum ^= JpegReferenceColor.Luma(i & 255, i * 37 & 255, i * 73 & 255);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 4096; i++)
        {
            checksum ^= JpegReferenceColor.Luma(i & 255, i * 37 & 255, i * 73 & 255);
            JpegReferenceColor.LimitedChroma(i % 511, i * 37 % 511, i * 73 % 511, true, out var cb, out var cr);
            checksum ^= cb ^ cr;
        }
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        GC.KeepAlive(checksum); Assert.Equal(0, allocated);
        Assert.Equal(16777216, triples); Assert.Equal(66867, paired);
        // Frozen from the qualified scalar matrix before compile-time preparation.
        Assert.Equal("a9e4219bc4dc0f8b2f8f813c9fb2ea003839ab64bffa34f1cc0671c1c62c7ded", Convert.ToHexStringLower(hash.GetHashAndReset()));
    }
}
