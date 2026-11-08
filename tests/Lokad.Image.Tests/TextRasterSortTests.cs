using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class TextRasterSortTests
{
    private readonly struct Order : TextRasterSort.IOrder<double>
    {
        public static int Compare(in double a, in double b) => a.CompareTo(b);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(16)]
    [InlineData(17)]
    [InlineData(257)]
    [InlineData(4097)]
    public void SortedReversedDuplicateAndMixedEventsPreserveEveryValue(int count)
    {
        var random = new Random(573);
        var values = new double[count];
        for (var shape = 0; shape < 5; shape++)
        {
            for (var i = 0; i < count; i++)
                values[i] = shape switch { 0 => i, 1 => count - i, 2 => 1, 3 => Math.Min(i, count - i), _ => random.Next(-50, 51) };
            if (shape == 4 && count > 5)
            { values[0] = double.NaN; values[1] = double.PositiveInfinity; values[2] = double.NegativeInfinity; values[3] = -0.0; values[4] = 0; }
            var expected = values.ToArray(); Array.Sort(expected);
            TextRasterSort.Sort<double, Order>(values, default);
            Assert.Equal(expected, values);
            Assert.Equal(expected.Select(BitConverter.DoubleToInt64Bits).Order(), values.Select(BitConverter.DoubleToInt64Bits).Order());
        }
    }

    [Fact]
    public void CancellationIsDirectAndDoesNotRetainStateForTheNextSort()
    {
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        var values = new double[] { 3, 1, 2 };
        var failure = Assert.Throws<OperationCanceledException>(() => TextRasterSort.Sort<double, Order>(values, cancellation.Token));
        Assert.Equal(cancellation.Token, failure.CancellationToken); Assert.Equal([3d, 1d, 2d], values);
        TextRasterSort.Sort<double, Order>(values, default); Assert.Equal([1d, 2d, 3d], values);
    }
}
