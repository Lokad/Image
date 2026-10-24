using System.Buffers.Binary;
using System.Security.Cryptography;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegForwardTransformTests
{
    [Fact]
    public void PreparedQuantizationPreservesEverySignedCoefficientAtEveryEightBitStep()
    {
        Span<ushort> quantization = stackalloc ushort[64];
        Span<int> plan = stackalloc int[128];
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
