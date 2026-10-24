using System.Reflection;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class Vp8ColorTests
{
    [Fact]
    public void WorkAdmissionIsAtomicPerRowAndPrecedesNextRowAccess()
    {
        var original = Planes(33, 2, 0);
        var planes = original with { Y = original.Y.AsSpan(0, original.Stride).ToArray() };
        var rowWork = planes.Width * 64L;
        foreach (var budget in new[] { rowWork - 1, rowWork, rowWork * 2 - 1 })
        {
            var context = Context(planes, null, ImageLimits.Default with { MaxWork = budget });
            var error = Assert.Throws<ImageLimitException>(() => Vp8Color.Render(planes, null, context));
            Assert.Equal("Computational work limit exceeded.", error.Message);
            Assert.Equal(budget < rowWork ? budget : budget - rowWork, context.RemainingWork);
            Assert.Equal(planes.MemoryBytes + planes.Width * planes.Height * 4L, LiveBytes(context));
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
        Assert.Equal(ImageLimits.Default.MaxWork, below.RemainingWork);
        var exact = Context(planes, alpha, ImageLimits.Default with { MaxWorkingBytes = input + output });
        using var result = Vp8Color.Render(planes, alpha, exact);
        Assert.Equal(output, LiveBytes(exact));
        Assert.Equal(ImageLimits.Default.MaxWork - 33 * 3 * 64L, exact.RemainingWork);
    }

    [Fact]
    public void CancellationPrecedesFirstRowAccessWithoutChargingWork()
    {
        var planes = Planes(33, 3, 0) with { Y = [] };
        using var cancellation = new CancellationTokenSource();
        var context = Context(planes, null, cancellation: cancellation.Token);
        cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => Vp8Color.Render(planes, null, context));
        Assert.Equal(ImageLimits.Default.MaxWork, context.RemainingWork);
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
