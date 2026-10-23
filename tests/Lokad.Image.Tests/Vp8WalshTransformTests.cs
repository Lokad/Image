using System.Buffers.Binary;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8WalshTransformTests
{
    // Native outputs for our original zero/extreme/checker/signed-unit DCs.
    // Unit values deliberately exercise signed arithmetic half-rounding.
    private static readonly short[][] NativeFragments =
    [
        [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
        [16320, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
        [-16320, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
        [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 16320],
        [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
        [-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1],
        [0, 0, -1, -1, 0, 0, -1, -1, 0, 0, -1, -1, 0, 0, -1, -1],
        [-1, -1, 0, 0, -1, -1, 0, 0, -1, -1, 0, 0, -1, -1, 0, 0],
        [0, -1, -1, 0, 0, -1, -1, 0, 0, -1, -1, 0, 0, -1, -1, 0],
        [-1, 0, 0, -1, -1, 0, 0, -1, -1, 0, 0, -1, -1, 0, 0, -1],
        [0, -1, 0, -1, 0, -1, 0, -1, 0, -1, 0, -1, 0, -1, 0, -1],
        [-1, 0, -1, 0, -1, 0, -1, 0, -1, 0, -1, 0, -1, 0, -1, 0],
        [0, 0, 0, 0, 0, 0, 0, 0, -1, -1, -1, -1, -1, -1, -1, -1],
        [-1, -1, -1, -1, -1, -1, -1, -1, 0, 0, 0, 0, 0, 0, 0, 0],
        [0, 0, -1, -1, 0, 0, -1, -1, -1, -1, 0, 0, -1, -1, 0, 0],
        [-1, -1, 0, 0, -1, -1, 0, 0, 0, 0, -1, -1, 0, 0, -1, -1],
        [0, -1, -1, 0, 0, -1, -1, 0, -1, 0, 0, -1, -1, 0, 0, -1],
        [-1, 0, 0, -1, -1, 0, 0, -1, 0, -1, -1, 0, 0, -1, -1, 0],
        [0, -1, 0, -1, 0, -1, 0, -1, -1, 0, -1, 0, -1, 0, -1, 0],
        [-1, 0, -1, 0, -1, 0, -1, 0, 0, -1, 0, -1, 0, -1, 0, -1],
        [0, 0, 0, 0, -1, -1, -1, -1, -1, -1, -1, -1, 0, 0, 0, 0],
        [-1, -1, -1, -1, 0, 0, 0, 0, 0, 0, 0, 0, -1, -1, -1, -1],
        [0, 0, -1, -1, -1, -1, 0, 0, -1, -1, 0, 0, 0, 0, -1, -1],
        [-1, -1, 0, 0, 0, 0, -1, -1, 0, 0, -1, -1, -1, -1, 0, 0],
        [0, -1, -1, 0, -1, 0, 0, -1, -1, 0, 0, -1, 0, -1, -1, 0],
        [-1, 0, 0, -1, 0, -1, -1, 0, 0, -1, -1, 0, -1, 0, 0, -1],
        [0, -1, 0, -1, -1, 0, -1, 0, -1, 0, -1, 0, 0, -1, 0, -1],
        [-1, 0, -1, 0, 0, -1, 0, -1, 0, -1, 0, -1, -1, 0, -1, 0],
        [0, 0, 0, 0, -1, -1, -1, -1, 0, 0, 0, 0, -1, -1, -1, -1],
        [-1, -1, -1, -1, 0, 0, 0, 0, -1, -1, -1, -1, 0, 0, 0, 0],
        [0, 0, -1, -1, -1, -1, 0, 0, 0, 0, -1, -1, -1, -1, 0, 0],
        [-1, -1, 0, 0, 0, 0, -1, -1, -1, -1, 0, 0, 0, 0, -1, -1],
        [0, -1, -1, 0, -1, 0, 0, -1, 0, -1, -1, 0, -1, 0, 0, -1],
        [-1, 0, 0, -1, 0, -1, -1, 0, -1, 0, 0, -1, 0, -1, -1, 0],
        [0, -1, 0, -1, -1, 0, -1, 0, 0, -1, 0, -1, -1, 0, -1, 0],
        [-1, 0, -1, 0, 0, -1, 0, -1, -1, 0, -1, 0, 0, -1, 0, -1],
    ];
    public static IEnumerable<object[]> Fragments()
    {
        for (var i = 0; i < NativeFragments.Length; i++) yield return [i, NativeFragments[i]];
    }

    [Theory]
    [MemberData(nameof(Fragments))]
    public void SignedDcFragmentsMatchNativeAndSupportCallerOwnedOverlappingBuffers(int fragment, short[] expected)
    {
        var dc = new short[18]; dc[16] = 30000; dc[17] = -30000;
        for (var p = 0; p < 16; p++)
            dc[p] = (short)(fragment switch
            {
                1 => 2040, 2 => -2040, 3 => (p / 4 + p % 4) % 2 == 0 ? 2040 : -2040,
                >= 4 when p == (fragment - 4) / 2 => (fragment - 4) % 2 == 0 ? 1 : -1,
                _ => 0
            });
        var original = dc.ToArray(); var output = Enumerable.Repeat((short)51, 18).ToArray();
        Vp8ForwardTransform.WalshDc(dc, output);
        Assert.Equal(expected, output[..16]); Assert.Equal(original, dc);
        Assert.Equal((short)51, output[16]); Assert.Equal((short)51, output[17]);
        Vp8ForwardTransform.WalshDc(dc, dc);
        Assert.Equal(expected, dc[..16]); Assert.Equal(original[16..], dc[16..]);
    }

    [Fact]
    public void AllOpaqueControlLumaMacroblocksMatchNativeY2Coefficients()
    {
        using var root = new TestMemoryBudget(16_000_000);
        using var source = OriginalVp8lControls.Buffer("opaque", 8, root);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var input = Vp8PreparedInput.Create(source, context); input.CleanupTransparentArea();
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        Span<short> residual = stackalloc short[16]; Span<short> dct = stackalloc short[16];
        Span<short> dc = stackalloc short[16]; Span<short> walsh = stackalloc short[16]; Span<byte> bytes = stackalloc byte[32];
        var macroblocks = 0; var original = Vp8PreparedInputTests.Hash(input); var retained = root.Bytes;
        for (var my = 0; my < input.StorageHeight; my += 16)
        for (var mx = 0; mx < input.Stride; mx += 16)
        {
            for (var by = 0; by < 4; by++)
            for (var bx = 0; bx < 4; bx++)
            {
                for (var y = 0; y < 4; y++)
                for (var x = 0; x < 4; x++) residual[y * 4 + x] = (short)(input.Y[(my + by * 4 + y) * input.Stride + mx + bx * 4 + x] - 128);
                Vp8ForwardTransform.Dct(residual, dct); dc[by * 4 + bx] = dct[0];
            }
            Vp8ForwardTransform.WalshDc(dc, walsh);
            for (var i = 0; i < 16; i++) BinaryPrimitives.WriteInt16LittleEndian(bytes.Slice(i * 2, 2), walsh[i]);
            hash.AppendData(bytes); macroblocks++;
        }
        Assert.Equal(561, macroblocks);
        Assert.Equal("429a1d6abd085057d28828ae65d8f8e80d9975f44530bbe658d535b00d0c4f81", Convert.ToHexStringLower(hash.GetHashAndReset()));
        Assert.Equal(original, Vp8PreparedInputTests.Hash(input)); Assert.Equal(retained, root.Bytes);
    }

    [Theory]
    [InlineData(-2041)]
    [InlineData(2041)]
    public void UnsupportedDcRangeFailsBeforeChangingCallerOutput(short invalid)
    {
        var dc = new short[16]; dc[15] = invalid; var output = Enumerable.Repeat((short)71, 16).ToArray();
        Assert.Throws<ArgumentOutOfRangeException>(() => Vp8ForwardTransform.WalshDc(dc, output));
        Assert.All(output, value => Assert.Equal((short)71, value));
    }
}
