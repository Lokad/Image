using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8BlockPredictionTests
{
    public static IEnumerable<object[]> Modes()
    {
        yield return [16, 0, "234672ca1ab8b085ca584ef900a01f2d4a8a469b4f785557729a795f8dc33910"];
        yield return [8, 0, "2abd06b92411540c4cb14409d06886914c3685c62287934c866600ca06b3ff30"];
        yield return [16, 1, "909f9c0be9e98ccbd56974ab8fb8eb5ac3c41c776e73cce1e57ab1bb2cf22604"];
        yield return [8, 1, "8bbb743ad5ea96f7cf2a5e3c394ba83bf31aa0aefbc1725917734525c3944e6d"];
        yield return [16, 2, "d0f543b2356eb2efcbafe96fdea5e1898b3a8ad7a111ed1e75e396e3beb9b116"];
        yield return [8, 2, "5b3ff97e0763c8682025d81b48988245a5fe4b6a5377430fec88ce0064f3ae55"];
        yield return [16, 3, "9f477139957092ef48bed939276f178b6340364f4662434320c9af0470edd4bb"];
        yield return [8, 3, "8a8311240abceefdf3da3029219a9a2fdc610e5cc4f0360469115c3e7d0c2713"];
    }

    private static void Predict(ReadOnlySpan<byte> plane, int stride, int x, int y, int size, Vp8IntraMode mode, Span<byte> output)
    {
        ReadOnlySpan<byte> top = y == 0 ? default : plane.Slice((y - 1) * stride + x, size);
        Span<byte> leftValues = stackalloc byte[16]; var left = leftValues[..size];
        if (x == 0) left = default;
        else for (var p = 0; p < size; p++) left[p] = plane[(y + p) * stride + x - 1];
        var corner = x != 0 && y != 0 ? plane[(y - 1) * stride + x - 1] : (byte)0;
        Vp8BlockPrediction.Fill(size, mode, top, left, corner, output);
    }

    [Theory]
    [MemberData(nameof(Modes))]
    public void AllNativeModePixelsMatchWithOriginalSourceBordersAndReplicatedEdges(int size, int modeCode, string expected)
    {
        using var root = new TestMemoryBudget(16_000_000); using var source = OriginalVp8lControls.Buffer("opaque", 8, root);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var input = Vp8PreparedInput.Create(source, context); input.CleanupTransparentArea();
        var original = Vp8PreparedInputTests.Hash(input); var retained = root.Bytes;
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        Span<byte> yBlock = stackalloc byte[256]; Span<byte> uBlock = stackalloc byte[64]; Span<byte> vBlock = stackalloc byte[64];
        var mode = (Vp8IntraMode)modeCode; var macroblocks = 0;
        for (var my = 0; my < input.StorageHeight; my += 16)
        for (var mx = 0; mx < input.Stride; mx += 16)
        {
            if (size == 16)
            {
                Predict(input.Y, input.Stride, mx, my, 16, mode, yBlock); hash.AppendData(yBlock);
            }
            else
            {
                Predict(input.U, input.Stride / 2, mx / 2, my / 2, 8, mode, uBlock);
                Predict(input.V, input.Stride / 2, mx / 2, my / 2, 8, mode, vBlock);
                for (var y = 0; y < 8; y++) { hash.AppendData(uBlock.Slice(y * 8, 8)); hash.AppendData(vBlock.Slice(y * 8, 8)); }
            }
            macroblocks++;
        }
        Assert.Equal(561, macroblocks); Assert.Equal(expected, Convert.ToHexStringLower(hash.GetHashAndReset()));
        Assert.Equal(original, Vp8PreparedInputTests.Hash(input)); Assert.Equal(retained, root.Bytes);
    }

    [Theory]
    [MemberData(nameof(Modes))]
    public void BorrowedOutputMayOverlapBordersWithoutFeedbackAndKeepsTails(int size, int modeCode, string unused)
    {
        _ = unused;
        var top = new byte[size]; var left = new byte[size];
        for (var p = 0; p < size; p++) { top[p] = (byte)(p * 43 + 17); left[p] = (byte)(p * 71 + 5); }
        var topBefore = top.ToArray(); var leftBefore = left.ToArray();
        var expected = new byte[size * size + 2]; expected[^2] = 98; expected[^1] = 99;
        Vp8BlockPrediction.Fill(size, (Vp8IntraMode)modeCode, top, left, 147, expected);
        Assert.Equal(topBefore, top); Assert.Equal(leftBefore, left);
        var overlapping = new byte[expected.Length]; top.CopyTo(overlapping, 0); left.CopyTo(overlapping, size);
        overlapping[^2] = 98; overlapping[^1] = 99;
        Vp8BlockPrediction.Fill(size, (Vp8IntraMode)modeCode, overlapping.AsSpan(0, size), overlapping.AsSpan(size, size), 147, overlapping);
        Assert.Equal(expected, overlapping);
    }
}
