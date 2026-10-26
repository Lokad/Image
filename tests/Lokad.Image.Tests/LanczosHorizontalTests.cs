using System.Numerics;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class LanczosHorizontalTests
{
    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void FullPrecisionAndNegativeWeightsMatchIndependentBigIntegerRows(int depth)
    {
        var maximum = depth == 8 ? 255 : 65535;
        // The long support crosses the 1024-tap poll and has a three-tap tail.
        foreach (var (width, target) in new[] { (7, 13), (31, 9), (1, 5), (1031, 2) })
        {
            using var source = ImageBuffer.Create(width, 2, depth);
            for (var y = 0; y < 2; y++)
            for (var x = 0; x < width; x++)
            {
                source.SetSample(x, y, 0, x % 3 == 0 ? maximum : maximum - 1);
                source.SetSample(x, y, 1, x % 3 == 1 ? maximum : maximum / 2);
                source.SetSample(x, y, 2, (x + y) % 3 == 2 ? maximum : 1);
                source.SetSample(x, y, 3, (x + y) % 5 switch { 0 => 0, 1 => 1, _ => maximum });
            }
            var before = source.Pixels.ToArray(); var expected = LanczosReference.Axis(width, target);
            if (width > 1) Assert.Contains(expected.SelectMany(w => w), w => w < 0);
            using var context = new OperationContext(ImageLimits.Default, default);
            using var plan = LanczosPlan.Create(width, target, context);
            var output = new long[target * 4 + 2]; Array.Fill(output, long.MinValue + 37);
var storage = context.RemainingWorkingBytes;
            LanczosResize.Horizontal(source, 1, plan, output.AsSpan(1, target * 4), context);
            Assert.Equal(storage, context.RemainingWorkingBytes);
            Assert.Equal(long.MinValue + 37, output[0]); Assert.Equal(long.MinValue + 37, output[^1]);
            for (var x = 0; x < target; x++)
            for (var c = 0; c < 4; c++)
            {
                BigInteger sum = 0;
                for (var i = 0; i < width; i++)
                    sum += (BigInteger)source.Sample(i, 1, 3) * (c == 3 ? 1 : source.Sample(i, 1, c)) * expected[x][i];
                Assert.Equal(sum, (BigInteger)output[1 + x * 4 + c]);
            }
            Assert.Equal(before, source.Pixels.ToArray());
        }
    }

    [Fact]
    public void CanceledHorizontalDoesNotPublishPartialRows()
    {
        using var source = ImageBuffer.Create(7, 1, 16);
        using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(ImageLimits.Default, cancellation.Token);
        using var plan = LanczosPlan.Create(7, 13, context);
        var output = new long[52]; Array.Fill(output, 123456789);
        cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => LanczosResize.Horizontal(source, 0, plan, output, context));
        Assert.All(output, value => Assert.Equal(123456789, value));
    }
}
