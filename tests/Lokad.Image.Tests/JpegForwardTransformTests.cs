using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegForwardTransformTests
{
    [Fact]
    public void ByteDomainColumnIntervalsProveEveryPartialRowSumFitsInt32()
    {
        var t1 = Fixed(Math.Tan(Math.PI / 16), 16); var t2 = Fixed(Math.Tan(Math.PI / 8), 16);
        var t3 = Fixed(Math.Tan(3 * Math.PI / 16), 16) - (1 << 16); var cosine = Fixed(Math.Cos(Math.PI / 4), 15);
        // Independent interval arithmetic admits every byte combination, with
        // deliberately uncorrelated intermediates. Each column operation fits
        // signed16; the absolute full-row bound covers every partial dot sum.
        var pair = Shift(new(0, 510), 3); var difference = Shift(new(-255, 255), 3); var middle = Shift(new(-255, 255), 4);
        var outside = Add(pair, pair); var inside = Add(pair, pair); var delta = Sub(pair, pair);
        var sumRotation = Odd(High(Add(middle, middle), cosine)); var differenceRotation = High(Sub(middle, middle), cosine);
        var low = Add(difference, sumRotation); var high = Sub(difference, sumRotation);
        var lowRotation = Add(difference, differenceRotation); var highRotation = Sub(difference, differenceRotation);
        var bounds = new[] { Add(outside, inside), Odd(Add(low, High(lowRotation, t1))), Odd(Add(delta, High(delta, t2))),
            Sub(Sub(high, highRotation), High(highRotation, t3)), Sub(outside, inside), Add(Add(highRotation, high), High(high, t3)),
            Odd(Sub(High(delta, t2), delta)), Sub(High(low, t1), lowRotation) };
        long largest = 0;
        for (var v = 0; v < 8; v++) for (var u = 0; u < 8; u++)
        {
            var angle = v is 0 or 4 ? 4 : Math.Min(v, 8 - v); var scale = Math.Cos(angle * Math.PI / 16);
            var magnitude = Math.Max(Math.Abs(bounds[v].Low), Math.Abs(bounds[v].High)); long absolute = 1 << 16;
            for (var x = 0; x < 8; x++)
            {
                var weight = Fixed((u == 0 ? Math.Cos(Math.PI / 4) : Math.Cos((2 * x + 1) * u * Math.PI / 16)) * scale, 15);
                absolute += (long)magnitude * Math.Abs(weight);
            }
            Assert.InRange(absolute, 0, int.MaxValue); largest = Math.Max(largest, absolute);
        }
        Assert.Equal(2_139_160_576L, largest);
    }

    [Fact]
    public void OriginalByteBlockFamiliesKeepFrozenCoefficientsAndBorrowedSpanBoundaries()
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var input = new byte[68]; Array.Fill(input, (byte)215); var output = new short[68]; Array.Fill(output, (short)4660);
        var packed = new byte[128]; uint state = 0x9e3779b9;
        for (var sample = 0; sample < 6400; sample++)
        {
            for (var y = 0; y < 8; y++) for (var x = 0; x < 8; x++)
            {
                byte value;
                if (sample < 256) value = (byte)sample;
                else if (sample < 2304)
                {
                    var mask = (sample - 256) / 8; var phase = (sample - 256) % 8;
                    value = (byte)(((mask >> ((y + phase * x) & 7)) & 1) * 255);
                }
                else { state ^= state << 13; state ^= state >> 17; state ^= state << 5; value = (byte)state; }
                input[2 + y * 8 + x] = value;
            }
            var before = Checksum(input);
            JpegReferenceTransform.Forward(input.AsSpan(2, 64), output.AsSpan(2, 64));
            Assert.Equal(before, Checksum(input));
            Assert.Equal((4660, 4660, 4660, 4660), ((int)output[0], output[1], output[66], output[67]));
            for (var i = 0; i < 64; i++) BinaryPrimitives.WriteInt16LittleEndian(packed.AsSpan(i * 2), output[i + 2]);
            hash.AppendData(packed);
        }
        // Frozen against the preceding qualified production transform:256
        // constants,2048 endpoint-column families and4096 seeded byte blocks.
        Assert.Equal("d3a5bebf3eb400a4640e8a18fbb3894055eda07efcf51fb9100699f3ac1de678",
            Convert.ToHexStringLower(hash.GetHashAndReset()));
    }

    [Fact]
    public void UnalignedPartiallyOverlappingBuffersPreserveOutputAndExternalStorage()
    {
        var block = new byte[64]; for (var i = 0; i < block.Length; i++) block[i] = (byte)(i * 13 + 27);
        var expected = new short[64]; JpegReferenceTransform.Forward(block, expected);
        foreach (var (inputStart, outputStart) in new[] { (1, 3), (9, 1) })
        {
            var storage = new byte[160]; Array.Fill(storage, (byte)0x6d); block.CopyTo(storage, inputStart); var before = storage.ToArray();
            var target = MemoryMarshal.Cast<byte, short>(storage.AsSpan(outputStart, 128));
            JpegReferenceTransform.Forward(storage.AsSpan(inputStart, 64), target);
            Assert.Equal(expected, target.ToArray());
            for (var i = 0; i < storage.Length; i++) if (i < outputStart || i >= outputStart + 128) Assert.Equal(before[i], storage[i]);
        }
        Assert.Throws<ArgumentException>(() => JpegReferenceTransform.Forward(block.AsSpan(0, 63), expected));
        Assert.Throws<ArgumentException>(() => JpegReferenceTransform.Forward(block, expected.AsSpan(0, 63)));
    }

    [Fact]
    public void ForwardTransformHasNoWarmManagedAllocation()
    {
        var input = new byte[64]; for (var i = 0; i < input.Length; i++) input[i] = (byte)(i * 13 + 27);
        var output = new short[64];
        for (var i = 0; i < 1024; i++) JpegReferenceTransform.Forward(input, output);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 1024; i++) JpegReferenceTransform.Forward(input, output);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        GC.KeepAlive(output); Assert.Equal(0, allocated);
    }

    private readonly record struct Interval(int Low, int High);
    private static int Fixed(double value, int bits) => (int)Math.Round(value * (1 << bits), MidpointRounding.AwayFromZero);
    private static Interval Checked(Interval range)
    {
        Assert.InRange(range.Low, short.MinValue, short.MaxValue); Assert.InRange(range.High, short.MinValue, short.MaxValue); return range;
    }
    private static Interval Add(Interval a, Interval b) => Checked(new(a.Low + b.Low, a.High + b.High));
    private static Interval Sub(Interval a, Interval b) => Checked(new(a.Low - b.High, a.High - b.Low));
    private static Interval Shift(Interval a, int bits) => Checked(new(a.Low << bits, a.High << bits));
    private static Interval Odd(Interval a) => Checked(new(a.Low | 1, a.High | 1));
    private static Interval High(Interval a, int multiplier)
    {
        var low = (int)((long)a.Low * multiplier >> 16); var high = (int)((long)a.High * multiplier >> 16);
        return Checked(new(Math.Min(low, high), Math.Max(low, high)));
    }
    private static uint Checksum(ReadOnlySpan<byte> values)
    {
        uint result = 0; foreach (var value in values) result = unchecked(result * 31 + value); return result;
    }

    [Fact]
    public void PreparedQuantizationPreservesEverySignedCoefficientAtEveryEightBitStep()
    {
        Span<ushort> quantization = stackalloc ushort[64];
        Span<ushort> plan = stackalloc ushort[128];
        Span<short> transformed = stackalloc short[64], output = stackalloc short[64];
        // Exhaust the full signed16-bit coefficient domain, including negative
        // magnitudes and every rounding threshold, for all legal8-bit steps.
        for (var q = 1; q <= 255; q++)
        {
            quantization.Fill((ushort)q); quantization[0] = 8;
            JpegReferenceTransform.PrepareQuantization(quantization, plan);
            for (var first = -32768; first <= 32767; first += 63)
            {
                transformed[0] = (short)first;
                for (var k = 1; k < 64; k++) transformed[k] = (short)Math.Min(32767, first + k - 1);
                JpegReferenceTransform.Quantize(transformed, plan, output);
                Assert.Equal((short)(((first + 32) >> 6) - 128), output[0]);
                for (var k = 1; k < 64; k++)
                {
                    var reciprocal = 8192 / q;
                    var rounding = (24576 + reciprocal / 2) / reciprocal;
                    var value = (int)transformed[k];
                    var magnitude = (Math.Abs(value) + rounding) * reciprocal >> 16;
                    var expected = (short)(value < 0 ? -magnitude : magnitude);
                    if (output[k] != expected)
                        Assert.Fail($"q={q}, coefficient={value}: expected {expected}, got {output[k]}.");
                }
            }
        }
    }

    [Fact]
    public void OriginalBlockVectorsPreserveQualifiedIntegerCoefficientsAndSpanBoundaries()
    {
        // Original flat/stripe/checker/impulse/complement/xorshift block recipes.
        // Frozen from the pre-SIMD integer forward transform, whose complete
        // encoder packets match pinned FFmpeg. No upstream block corpus/table.
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var count = 0; var bytes = new byte[128];
        foreach (var block in Blocks())
        {
            var input = Enumerable.Repeat((byte)215, 68).ToArray(); block.CopyTo(input, 2);
            var before = input.ToArray(); var output = Enumerable.Repeat((short)4660, 68).ToArray();
            JpegReferenceTransform.Forward(input.AsSpan(2, 64), output.AsSpan(2, 64));
            Assert.Equal(before, input);
            Assert.Equal(new short[] { 4660, 4660 }, output[..2]);
            Assert.Equal(new short[] { 4660, 4660 }, output[66..]);
            for (var i = 0; i < 64; i++) BinaryPrimitives.WriteInt16LittleEndian(bytes.AsSpan(2 * i), output[i + 2]);
            hash.AppendData(bytes); count++;
        }
        Assert.Equal(166, count);
        Assert.Equal("1796a7c271df39bfe48c938e5deefa2d5b2b79e9d0f8231e72b0aa3d235bda71", Convert.ToHexStringLower(hash.GetHashAndReset()));
    }

    static System.Collections.Generic.IEnumerable<byte[]> Blocks()
    {
        foreach (var value in new byte[] { 0, 128, 255 }) { var b = new byte[64]; Array.Fill(b, value); yield return b; }
        for (var kind = 0; kind < 3; kind++)
        {
            var b = new byte[64];
            for (var y = 0; y < 8; y++) for (var x = 0; x < 8; x++) b[y * 8 + x] = (byte)(((kind == 0 ? x : kind == 1 ? y : x + y) & 1) * 255);
            yield return b;
        }
        for (var inverse = 0; inverse < 2; inverse++)
        for (var at = 0; at < 64; at++) { var b = new byte[64]; Array.Fill(b, (byte)(inverse * 255)); b[at] = (byte)((1 - inverse) * 255); yield return b; }
        uint state = 0x13e98b7u;
        for (var block = 0; block < 32; block++)
        {
            var b = new byte[64];
            for (var i = 0; i < 64; i++) { state ^= state << 13; state ^= state >> 17; state ^= state << 5; b[i] = (byte)state; }
            yield return b;
        }
    }
}
