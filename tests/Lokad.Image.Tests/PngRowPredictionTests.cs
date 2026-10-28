using System.Buffers.Binary;
using System.Security.Cryptography;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class PngRowPredictionTests
{
    [Fact]
    public void OriginalRowsRetainScalarPolicyAndIndependentlyReconstructEveryByte()
    {
        // Original LCG rows, six legal byte strides, vector/tail and 4KiB edges.
        // The digest comes from an independent signed-byte/W3C predictor oracle
        // and is established against the scalar producer before SIMD adoption.
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var filters = new HashSet<byte>(); Span<byte> tag = stackalloc byte[13];
        foreach (var stride in new[] { 1, 2, 3, 4, 6, 8 })
        foreach (var width in new[] { 1, 2, 5, 7, 11, 17, 31, 33, 683 })
        for (var pattern = 0; pattern < 6; pattern++)
        {
            var length = stride * width; uint state = 0x947ac531 ^ ((uint)stride << 24) ^ ((uint)width << 8) ^ (uint)pattern;
            byte Next() { state = unchecked(1664525 * state + 1013904223); return (byte)(state >> 24); }
            var previous = Enumerable.Repeat((byte)0xcd, length + 32).ToArray();
            var row = previous.ToArray(); var filtered = previous.ToArray();
            var prior = previous.AsSpan(7, length); var raw = row.AsSpan(3, length); var output = filtered.AsSpan(11, length);
            for (var i = 0; i < length; i++) prior[i] = Next();
            for (var i = 0; i < length; i++) raw[i] = Next();
            for (var i = 0; i < length; i++)
            {
                var a = i < stride ? 0 : raw[i - stride]; var b = prior[i]; var c = i < stride ? 0 : prior[i - stride];
                raw[i] = pattern switch
                {
                    0 => 0, 1 when i >= stride => (byte)a, 2 => b,
                    3 => (byte)((a + b) / 2), 4 => (byte)Paeth(a, b, c), _ => raw[i]
                };
            }
            var beforeRow = row.ToArray(); var beforePrevious = previous.ToArray();
            var filter = PngRowPrediction.Apply(raw, prior, output, stride, default); filters.Add(filter);
            var bytes = filter == 0 ? raw : output;
            BinaryPrimitives.WriteInt32LittleEndian(tag, stride); BinaryPrimitives.WriteInt32LittleEndian(tag[4..], width);
            BinaryPrimitives.WriteInt32LittleEndian(tag[8..], pattern); tag[12] = filter; hash.AppendData(tag); hash.AppendData(bytes);
            var reconstructed = new byte[length];
            for (var i = 0; i < length; i++)
            {
                var a = i < stride ? 0 : reconstructed[i - stride]; var b = prior[i]; var c = i < stride ? 0 : prior[i - stride];
                var predictor = filter switch { 0 => 0, 1 => a, 2 => b, 3 => (a + b) / 2, _ => Paeth(a, b, c) };
                reconstructed[i] = unchecked((byte)(bytes[i] + predictor));
            }
            Assert.Equal(raw.ToArray(), reconstructed); Assert.Equal(beforeRow, row); Assert.Equal(beforePrevious, previous);
            Assert.All(filtered.Take(11), b => Assert.Equal(0xcd, b));
            Assert.All(filtered.Skip(11 + length), b => Assert.Equal(0xcd, b));
        }
        Assert.Equal(new byte[] { 0, 1, 2, 3, 4 }, filters.Order().ToArray());
        Assert.Equal("b005b3dc378748340fef2832955096685609db246043e075c15005f9501d611f", Convert.ToHexStringLower(hash.GetHashAndReset()));
    }

    [Fact]
    public void PreCanceledPredictionLeavesItsOutputUntouched()
    {
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        var output = Enumerable.Repeat((byte)0xcd, 8192).ToArray();
        Assert.Throws<OperationCanceledException>(() => PngRowPrediction.Apply(new byte[8192], new byte[8192], output, 8, cancellation.Token));
        Assert.All(output, b => Assert.Equal(0xcd, b));
    }

    private static int Paeth(int a, int b, int c)
    {
        var p = a + b - c; var da = Math.Abs(p - a); var db = Math.Abs(p - b); var dc = Math.Abs(p - c);
        return da <= db && da <= dc ? a : db <= dc ? b : c;
    }
}
