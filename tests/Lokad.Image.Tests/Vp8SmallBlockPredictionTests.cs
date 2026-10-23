using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8SmallBlockPredictionTests
{
    public static IEnumerable<object[]> Modes()
    {
        yield return [0, "20a75a0391258027655df916b497e40329d8ba0a2f894c863eb90134a816e606"];
        yield return [1, "5c6751a83d5849b447f5e80dd329cca3648303a218a7744f171cbb8ba462a852"];
        yield return [2, "eb1cac2dc77159bb042a50444cbf04c2daf47d29739bffdf538a5361591789ba"];
        yield return [3, "d0eac2310554a4bcf2f96d2d48f5bfb6efe6daa86df874704507739721414b1a"];
        yield return [4, "25234075466aa7159aa2a206b33e9a1f68f5fd3cda2a2c46cfc49d3f4565ec14"];
        yield return [5, "c00acb8e7ca436eaf4bbf3a2536d018eb91af5237da8ece3e39e31262165cbcf"];
        yield return [6, "82c9828fef0df12e746ad6120678ba79742768c351c5d3e79a7f5fac296a054a"];
        yield return [7, "84872e5bd50fa9324a1cef98c3d503ea9db9eb009372ef9b94b8a704c2b4d114"];
        yield return [8, "948a982c63d3af751a76f0ee681f1a173f47d06ea2b63f189da3c37de2c1668e"];
        yield return [9, "f8633f34cff231b17a568594c22750ec25e4a79f1358b948a4636f01da19575d"];
    }

    private static byte Sample(ReadOnlySpan<byte> plane, int stride, int x, int y)
        => y < 0 ? (byte)127 : x < 0 ? (byte)129 : plane[y * stride + Math.Min(x, stride - 1)];

    [Theory]
    [MemberData(nameof(Modes))]
    public void AllNativeModePixelsMatchOnFiftyBoundedSourceAndRoundingContexts(int mode, string expected)
    {
        using var root = new TestMemoryBudget(16_000_000); using var image = OriginalVp8lControls.Buffer("opaque", 8, root);
        using var context = new OperationContext(new() { MemoryBudget = root }, default); using var input = Vp8PreparedInput.Create(image, context);
        var original = Vp8PreparedInputTests.Hash(input); var retained = root.Bytes;
        Span<byte> top = stackalloc byte[8]; Span<byte> left = stackalloc byte[4]; Span<byte> destination = stackalloc byte[18];
        destination[16] = 97; destination[17] = 98;
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256); var contexts = 0;
        foreach (var (mx, my) in new[] { (0, 0), (16, 16), (512, 256) })
        for (var by = 0; by < 16; by += 4) for (var bx = 0; bx < 16; bx += 4)
        {
            context.Work(128); var x = mx + bx; var y = my + by;
            for (var p = 0; p < 8; p++) top[p] = Sample(input.Y, input.Stride, x + p, bx == 12 && p >= 4 ? my - 1 : y - 1);
            for (var p = 0; p < 4; p++) left[p] = Sample(input.Y, input.Stride, x - 1, y + p);
            var corner = Sample(input.Y, input.Stride, x - 1, y - 1);
            Vp8Prediction.SmallBlock((Vp8SmallIntraMode)mode, top, left, corner, destination); hash.AppendData(destination[..16]); contexts++;
        }
        for (var corner = 0; corner <= 255; corner += 255)
        {
            for (var p = 0; p < 8; p++) top[p] = p % 2 == 0 ? (byte)0 : (byte)255;
            for (var p = 0; p < 4; p++) left[p] = p % 2 == 0 ? (byte)255 : (byte)0;
            Vp8Prediction.SmallBlock((Vp8SmallIntraMode)mode, top, left, (byte)corner, destination); hash.AppendData(destination[..16]); contexts++;
        }
        Assert.Equal(50, contexts); Assert.Equal(expected, Convert.ToHexStringLower(hash.GetHashAndReset()));
        Assert.Equal((97, 98), ((int)destination[16], (int)destination[17]));
        Assert.Equal(original, Vp8PreparedInputTests.Hash(input)); Assert.Equal(retained, root.Bytes);
    }

    [Fact]
    public void AllModesSnapshotAliasedBordersAndPreserveSeparateInputsAndTails()
    {
        byte[] top = [0,255,2,253,4,251,6,249], left = [255,0,253,2];
        var originalTop = top.ToArray(); var originalLeft = left.ToArray();
        for (var mode = 0; mode < 10; mode++)
        {
            var separate = new byte[18]; separate[16] = 97; separate[17] = 98;
            var aliased = new byte[18]; top.CopyTo(aliased, 0); left.CopyTo(aliased, 8); aliased[16] = 97; aliased[17] = 98;
            Vp8Prediction.SmallBlock((Vp8SmallIntraMode)mode, top, left, 173, separate);
            Vp8Prediction.SmallBlock((Vp8SmallIntraMode)mode, aliased.AsSpan(0, 8), aliased.AsSpan(8, 4), 173, aliased);
            Assert.Equal(separate, aliased); Assert.Equal(originalTop, top); Assert.Equal(originalLeft, left);
        }
    }

    [Fact]
    public void InvalidModesOrIncompleteBordersFailBeforeWritingOutput()
    {
        var top = new byte[8]; var left = new byte[4]; var output = Enumerable.Repeat((byte)73, 18).ToArray();
        Assert.Throws<ArgumentOutOfRangeException>(() => Vp8Prediction.SmallBlock((Vp8SmallIntraMode)255, top, left, 0, output));
        Assert.Throws<ArgumentException>(() => Vp8Prediction.SmallBlock(Vp8SmallIntraMode.Dc, top.AsSpan(0, 7), left, 0, output));
        Assert.Throws<ArgumentException>(() => Vp8Prediction.SmallBlock(Vp8SmallIntraMode.Dc, top, left.AsSpan(0, 3), 0, output));
        Assert.Throws<ArgumentException>(() => Vp8Prediction.SmallBlock(Vp8SmallIntraMode.Dc, top, left, 0, output.AsSpan(0, 15)));
        Assert.All(output, v => Assert.Equal((byte)73, v));
    }
}
