using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegGrayForwardTests
{
    [Fact]
    public void OriginalByteDomainControlsKeepEveryFrozenCoefficient()
    {
        var input = new byte[64]; var output = new short[64]; var packed = new byte[128]; var blocks = 0;
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        void Check()
        {
            JpegGrayReferenceTransform.Forward(input, output);
            for (var p = 0; p < 64; p++) BinaryPrimitives.WriteInt16LittleEndian(packed.AsSpan(p * 2), output[p]);
            hash.AppendData(packed); blocks++;
        }
        for (var value = 0; value < 256; value++) { Array.Fill(input, (byte)value); Check(); }
        for (var position = 0; position < 64; position++)
        for (var value = 0; value < 256; value++) { Array.Clear(input); input[position] = (byte)value; Check(); }
        for (var mask = 0; mask < 65536; mask++)
        {
            for (var p = 0; p < 64; p++) input[p] = (byte)((mask >> ((p / 8 % 4) * 4 + p % 4) & 1) * 255);
            Check();
        }
        uint state = 0x9e3779b9;
        for (var sample = 0; sample < 32768; sample++)
        {
            for (var p = 0; p < 64; p++) { state ^= state << 13; state ^= state >> 17; state ^= state << 5; input[p] = (byte)state; }
            Check();
        }
        Assert.Equal(114944, blocks);
        Assert.Equal("b34ab9e7af2b7a6f0c9a29deef7c88054ee0d848f70d5c0020d3cece815cb761",
            Convert.ToHexStringLower(hash.GetHashAndReset()));
    }

    [Fact]
    public void RoundedKernelAndFactoredIntermediatesFitForEveryByteInput()
    {
        static int Fixed(double value) => (int)Math.Round(value * 8192, MidpointRounding.AwayFromZero);
        static long Abs(int value) => Math.Abs((long)value);
        var root = Math.Sqrt(2); var c1 = Math.Cos(Math.PI / 16); var c3 = Math.Cos(3 * Math.PI / 16);
        var c5 = Math.Cos(5 * Math.PI / 16); var c7 = Math.Cos(7 * Math.PI / 16);
        var rotation = Fixed(root * Math.Cos(3 * Math.PI / 8));
        var near = Fixed(2 * Math.Cos(3 * Math.PI / 8)); var far = Fixed(2 * Math.Cos(Math.PI / 8));
        var common = Fixed(root * c3); var a = Fixed(root * (c7 - c3)); var b = Fixed(root * (-c1 - c3));
        var c = Fixed(root * (-c3 - c5)); var d = Fixed(root * (c5 - c3));
        var e0 = Fixed(root * (c1 + c3 - c5 - c7)); var e1 = Fixed(root * (c1 + c3 + c5 - c7));
        var e2 = Fixed(root * (c1 + c3 - c5 + c7)); var e3 = Fixed(root * (-c1 + c3 + c5 - c7));
        // Independently expanded Q13 rows, before either rounding stage.
        var l1 = new[] { 8 * 8192L, 4 * (Abs(rotation + near) + Abs(rotation)), 4 * (Abs(rotation) + Abs(rotation - far)),
            2 * (Abs(common + a) + Abs(common + d) + Abs(common) + Abs(common + a + d + e0)),
            2 * (Abs(common + c) + Abs(common + b) + Abs(common + b + c + e1) + Abs(common)),
            2 * (Abs(common) + Abs(common + b + d + e2) + Abs(common + b) + Abs(common + d)),
            2 * (Abs(common + a + c + e3) + Abs(common) + Abs(common + c) + Abs(common + a)) }.Max();
        Assert.Equal(65536, l1);
        var m = (128 * l1 + 1024 + 2047) >> 11; Assert.Equal(4097, m);
        Assert.Equal(268517376, m * l1 + 16384);
        // Sum absolute contributions before cancellation. These also cover all
        // partial additions/products; the first byte-domain pass has smaller m.
        var bound = new[] { 8 * m * 8192, 8 * m * Abs(rotation) + 4 * m * Abs(near),
            8 * m * Abs(rotation) + 4 * m * Abs(far),
            8 * m * Abs(common) + 4 * m * (Abs(a) + Abs(d)) + 2 * m * Abs(e0),
            8 * m * Abs(common) + 4 * m * (Abs(b) + Abs(c)) + 2 * m * Abs(e1),
            8 * m * Abs(common) + 4 * m * (Abs(b) + Abs(d)) + 2 * m * Abs(e2),
            8 * m * Abs(common) + 4 * m * (Abs(a) + Abs(c)) + 2 * m * Abs(e3) }.Max() + 16384;
        Assert.Equal(1129411792, bound); Assert.True(bound < int.MaxValue);
    }

    [Fact]
    public void UnalignedOverlappingLayoutsSnapshotInputAndKeepGuards()
    {
        var source = Enumerable.Range(0, 64).Select(p => (byte)(p * 73 + p / 8 * 19)).ToArray();
        var expected = new short[64]; JpegGrayReferenceTransform.Forward(source, expected);
        foreach (var offsets in new[] { (1, 32), (64, 1), (1, 1) })
        {
            var data = Enumerable.Repeat((byte)0xA7, 256).ToArray(); source.CopyTo(data, offsets.Item1);
            var wanted = data.ToArray(); MemoryMarshal.AsBytes(expected.AsSpan()).CopyTo(wanted.AsSpan(offsets.Item2, 128));
            JpegGrayReferenceTransform.Forward(data.AsSpan(offsets.Item1, 64), MemoryMarshal.Cast<byte, short>(data.AsSpan(offsets.Item2, 128)));
            Assert.Equal(wanted, data);
        }
    }

    [Fact]
    public void InvalidLengthsFailBeforeAnyOutputMutation()
    {
        foreach (var lengths in new[] { (0, 64), (63, 64), (65, 64), (64, 0), (64, 63), (64, 65) })
        {
            var input = Enumerable.Repeat((byte)23, lengths.Item1).ToArray(); var output = Enumerable.Repeat((short)19000, lengths.Item2).ToArray();
            Assert.Throws<ArgumentException>(() => JpegGrayReferenceTransform.Forward(input, output));
            Assert.All(input, value => Assert.Equal((byte)23, value)); Assert.All(output, value => Assert.Equal((short)19000, value));
        }
    }

    [Fact]
    public void WarmForwardHasNoManagedAllocation()
    {
        var input = Enumerable.Range(0, 64).Select(p => (byte)(p * 73)).ToArray(); var output = new short[64];
        for (var i = 0; i < 256; i++) JpegGrayReferenceTransform.Forward(input, output);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 10000; i++) JpegGrayReferenceTransform.Forward(input, output);
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
    }
}
