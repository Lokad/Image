using System.Reflection;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class Vp8ColorTests
{
    private delegate void Pixel(byte luma, int u, int v, Span<byte> output);
    private delegate void Pixels8(ReadOnlySpan<byte> luma, Vector256<int> u, Vector256<int> v, Vector256<int> alpha, Span<byte> output);
    private delegate int Expansion(byte[] plane, int stride, int width, int height, int x, int y);
    private static readonly Pixel Scalar = typeof(Vp8Color).GetMethod("WritePixel", BindingFlags.Static | BindingFlags.NonPublic)!.CreateDelegate<Pixel>();
    private static readonly Expansion Expand = typeof(Vp8Color).GetMethod("Chroma", BindingFlags.Static | BindingFlags.NonPublic)!.CreateDelegate<Expansion>();

    [Fact]
    public void DenseByteColorCubeKeepsEveryScalarRoundingAndAlphaLane()
    {
        if (!Avx2.IsSupported || !Sse41.IsSupported) return;
        var vector = typeof(Vp8Color).GetMethod("WritePixels8", BindingFlags.Static | BindingFlags.NonPublic)!.CreateDelegate<Pixels8>();
        Span<byte> luma = stackalloc byte[8]; Span<byte> actual = stackalloc byte[32]; Span<byte> expected = stackalloc byte[32];
        var alpha = Vector256.Create(0, 1, 127, 255, 254, 128, 7, 19);
        for (var y = 0; y < 256; y++)
        {
            luma.Fill((byte)y);
            for (var u = -128; u < 128; u++)
            for (var v = -128; v < 128; v += 8)
            {
                vector(luma, Vector256.Create(u), Vector256.Create(v, v + 1, v + 2, v + 3, v + 4, v + 5, v + 6, v + 7), alpha, actual);
                for (var lane = 0; lane < 8; lane++)
                {
                    Scalar((byte)y, u, v + lane, expected.Slice(lane * 4, 3)); expected[lane * 4 + 3] = (byte)alpha.GetElement(lane);
                }
                if (!actual.SequenceEqual(expected)) Assert.Fail($"Color mismatch at Y={y},U={u},V={v}.");
            }
        }
    }

    [Fact]
    public void RenderKeepsCenteredChromaEdgesTailsPaddingAndAlphaOwnership()
    {
        Span<byte> expected = stackalloc byte[3];
        foreach (var width in new[] { 1, 2, 8, 9, 10, 16, 17, 18, 31, 32, 33, 34, 63, 64, 65, 66 })
        foreach (var height in new[] { 1, 2, 3, 5 })
        foreach (var pattern in new[] { 0, 1 })
        foreach (var opaque in new[] { false, true })
        {
            var planes = Planes(width, height, pattern); var originalY = planes.Y.ToArray(); var originalU = planes.U.ToArray(); var originalV = planes.V.ToArray();
            var alpha = opaque ? null : Enumerable.Range(0, width * height).Select(i => (byte)(i * 43)).ToArray(); var originalAlpha = alpha?.ToArray();
            using var context = Context(planes, alpha); using var actual = Vp8Color.Render(planes, alpha, context);
            for (var y = 0; y < height; y++)
            {
                var row = actual.GetRow8(y);
                for (var x = 0; x < width; x++)
                {
                    var u = Expand(planes.U, planes.Stride / 2, (width + 1) / 2, (height + 1) / 2, x, y) - 128;
                    var v = Expand(planes.V, planes.Stride / 2, (width + 1) / 2, (height + 1) / 2, x, y) - 128;
                    Scalar(planes.Y[y * planes.Stride + x], u, v, expected);
                    Assert.True(expected.SequenceEqual(row.Slice(x * 4, 3))); Assert.Equal(alpha is null ? (byte)255 : alpha[y * width + x], row[x * 4 + 3]);
                }
            }
            Assert.Equal(originalY, planes.Y); Assert.Equal(originalU, planes.U); Assert.Equal(originalV, planes.V); Assert.Equal(originalAlpha, alpha);
        }
    }

    [Fact]
    public void ExactOutputMemoryAdmissionAndPlaneReleaseRemain()
    {
        var planes = Planes(33, 3, 1); var alpha = new byte[99];
        var input = planes.MemoryBytes + alpha.Length + 32L; var output = 33 * 3 * 4L;
        var below = Context(planes, alpha, ImageLimits.Default with { MaxWorkingBytes = input + output - 1 });
        Assert.Throws<ImageLimitException>(() => Vp8Color.Render(planes, alpha, below));
        Assert.Equal(input, LiveBytes(below));
        var exact = Context(planes, alpha, ImageLimits.Default with { MaxWorkingBytes = input + output });
        using var result = Vp8Color.Render(planes, alpha, exact);
        Assert.Equal(output, LiveBytes(exact));
    }

    [Fact]
    public void CancellationPrecedesFirstRowAccessWithoutCharging()
    {
        var planes = Planes(33, 3, 0) with { Y = [] };
        using var cancellation = new CancellationTokenSource();
        var context = Context(planes, null, cancellation: cancellation.Token);
        cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => Vp8Color.Render(planes, null, context));
        Assert.Equal(planes.MemoryBytes + 33 * 3 * 4L, LiveBytes(context));
    }

    private static OperationContext Context(Vp8Planes planes, byte[]? alpha, ImageLimits? limits = null,
        CancellationToken cancellation = default)
    {
        var context = new OperationContext(limits ?? ImageLimits.Default, cancellation);
        context.Reserve(planes.MemoryBytes + (alpha is null ? 0 : alpha.Length + 32L));
        return context;
    }

    private static long LiveBytes(OperationContext context)
        => (long)typeof(OperationContext).GetField("liveBytes", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(context)!;

    private static Vp8Planes Planes(int width, int height, int pattern)
    {
        var stride = (width + 19) & ~1;
        var y = Enumerable.Repeat((byte)0xd7, stride * height).ToArray();
        var u = Enumerable.Repeat((byte)0xd7, stride / 2 * ((height + 1) / 2)).ToArray();
        var v = u.ToArray();
        for (var row = 0; row < height; row++)
            for (var x = 0; x < width; x++) y[row * stride + x] = Value(x, row, pattern);
        for (var row = 0; row < (height + 1) / 2; row++)
            for (var x = 0; x < (width + 1) / 2; x++)
            {
                u[row * stride / 2 + x] = Value(x + 13, row + 7, pattern);
                v[row * stride / 2 + x] = Value(x + 29, row + 19, pattern);
            }
        return new Vp8Planes(width, height, stride, y, u, v);
    }

    private static byte Value(int x, int y, int pattern) => pattern == 0
        ? (byte)((x * 37 + y * 53 + x * y * 17 + (x ^ y) * 11 + 19) & 255)
        : (byte)((x + y) % 7 switch { 0 => 0, 1 => 255, 2 => 16, 3 => 235, 4 => 128, 5 => 127, _ => 129 });

}
