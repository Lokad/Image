using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegRefinementWindowTests
{
    private static bool Supported => Avx2.IsSupported && Bmi2.X64.IsSupported;

    [Fact]
    public void WindowPreservesConsecutiveBlocksTruncationAndErrorTails()
    {
        if (!Supported) return;
        using var tables = new OperationContext(new(), default);
        byte[] counts = new byte[16]; counts[2] = 5;
        var table = new JpegHuffman(counts, [0, 1, 0xf0, 0x10, 2], tables);
        for (var seed = 0; seed < 32; seed++)
        for (var phase = 0; phase < 8; phase++)
        foreach (var length in new[] { 0, 1, 3, 4, 5, 8, 9, 16, 23, 31, 48 })
        {
            var bytes = new List<byte>();
            for (var i = 0; i < 48; i++)
            {
                var value = (byte)(seed * 73 + i * 41); bytes.Add(value);
                if (value == 255) bytes.Add(0);
            }
            bytes.AddRange([255, 0xda]); var data = bytes.Take(length).ToArray();
            Assert.Equal(Run(false), Run(true));

            string Run(bool window)
            {
                using var context = new OperationContext(new(), default);
                var reader = new JpegEntropyReader(data, 0, context); var eob = seed % 3;
                var storage = Enumerable.Repeat(12345, 66).ToArray(); var block = storage.AsSpan(1, 64);
                var states = new List<string>(); Exception? failure = null;
                try
                {
                    if (phase != 0) reader.ReadBits(phase);
                    for (var round = 0; round < 4; round++)
                    {
                        var low = (seed & 1) == 0 ? 0 : 13; var increment = 1 << low;
                        for (var i = 0; i < 64; i++) block[i] = (i + seed + round) % 5 == 0 ? 0
                            : (2 + i % 2) * increment * (i % 3 == 0 ? -1 : 1);
                        if (window) Assert.True(table.TryReadRefinementBlock(ref reader, block, 1, 63, low, ref eob));
                        else JpegRefinementBlockTests.Reference(ref reader, block, table, 1, 63, low, ref eob);
                        states.Add(reader.ReadBits(round + 1).ToString());
                    }
                    states.Add(reader.ReadBits(16).ToString());
                }
                catch (ImageDataException error) { failure = error; }
                var position = reader.Position; var next = -1; Type? nextError = null;
                try { next = reader.ReadBits(8); } catch (ImageDataException error) { nextError = error.GetType(); }
                Assert.Equal(12345, storage[0]); Assert.Equal(12345, storage[^1]);
                return $"{failure?.GetType()}:{failure?.Message}:{position}:{eob}:{string.Join(',', storage)}:{string.Join(';', states)}:{next}:{nextError}:{reader.Position}";
            }
        }
    }

    [Fact]
    public void FailedEobFieldKeepsItsAlreadyReadWholeByte()
    {
        if (!Supported) return;
        using var tables = new OperationContext(new(), default);
        byte[] counts = new byte[16]; counts[7] = 1;
        var table = new JpegHuffman(counts, [0xe0], tables);
        using var context = new OperationContext(new(), default);
        var reader = new JpegEntropyReader([0, 0x72, 255, 0xd9, 0x31], 0, context);
        var values = new int[64]; var eob = 0;
        try { table.TryReadRefinementBlock(ref reader, values, 1, 63, 0, ref eob); Assert.Fail("The EOB field is truncated by a marker."); }
        catch (ImageDataException error) { Assert.Equal("Unexpected marker in JPEG entropy data.", error.Message); }
        Assert.Equal(4, reader.Position); Assert.Equal(0, eob);
        Assert.Equal(0x72, reader.ReadBits(8)); Assert.Equal(4, reader.Position);
        Assert.All(values, v => Assert.Equal(0, v));
    }

    [Fact]
    public void GroupedOverflowKeepsTheAlreadyConsumedFieldAndReturnsLookahead()
    {
        if (!Supported) return;
        using var tables = new OperationContext(new(), default);
        var table = new JpegHuffman([1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0], [0], tables);
        using var context = new OperationContext(new(), default);
        var reader = new JpegEntropyReader([0xa5, 0xc3, 0x15, 0x4e, 0x33, 0x6a], 0, context);
        var values = Enumerable.Repeat(2, 64).ToArray(); values[1] = int.MinValue; var eob = 1;
        try { table.TryReadRefinementBlock(ref reader, values, 1, 63, 0, ref eob); Assert.Fail("The first correction overflows."); }
        catch (OverflowException) { }
        // Existing grouped semantics admit sixteen correction bits before any
        // updates. The two unused prefetched bytes must return to the reader.
        Assert.Equal(2, reader.Position); Assert.Equal(1, eob);
        Assert.Equal(0x15, reader.ReadBits(8)); Assert.Equal(3, reader.Position);
        Assert.Equal(int.MinValue, values[1]); Assert.All(values.Skip(2), v => Assert.Equal(2, v));
    }

    [Fact]
    public void DenseWindowUsesNoOwnedStorage()
    {
        if (!Supported) return;
        using var tables = new OperationContext(new(), default);
        var table = new JpegHuffman([1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0], [0], tables);
        using var context = new OperationContext(new(), default);
        var data = Enumerable.Repeat((byte)0x55, 16).ToArray(); var values = Enumerable.Repeat(2, 64).ToArray();
        var warm = new JpegEntropyReader(data, 0, context); var eob = 1;
        Assert.True(table.TryReadRefinementBlock(ref warm, values, 1, 63, 0, ref eob));
        Array.Fill(values, 2); var reader = new JpegEntropyReader(data, 0, context); eob = 1;
        var before = GC.GetAllocatedBytesForCurrentThread();
        var applied = table.TryReadRefinementBlock(ref reader, values, 1, 63, 0, ref eob);
        Assert.Equal(before, GC.GetAllocatedBytesForCurrentThread()); Assert.True(applied);
        Assert.Equal(8, reader.Position); Assert.Equal(0, eob); Assert.Equal(1, reader.ReadBits(1));
        for (var i = 1; i < 64; i++) Assert.Equal((i & 1) == 0 ? 3 : 2, values[i]);
    }
}
