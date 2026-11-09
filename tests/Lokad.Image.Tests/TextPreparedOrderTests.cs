namespace Lokad.Image.Tests
{
    public sealed class TextPreparedOrderTests
    {
        [Fact]
        public void PreparedPermutationMatchesIndependentEdgeSorting() => Internal.TextRasterizer.VerifyPreparedOrder(false);

        [Fact]
        public void CanceledPermutationPreservesInputsAndAllowsRecovery() => Internal.TextRasterizer.VerifyPreparedOrder(true);
    }
}

namespace Lokad.Image.Internal
{
    internal sealed partial class TextRasterizer
    {
        internal static void VerifyPreparedOrder(bool cancel)
        {
            var random = new Random(671);
            foreach (var count in new[] { 0, 1, 2, 3, 15, 16, 17, 31, 64, 127, 512 })
            for (var shape = 0; shape < 8; shape++)
            {
                var edges = new Edge[count + 2]; var order = new Crossing[count + 2];
                edges[0] = edges[^1] = new(new(12345, 12345), new(-12345, -12345), -7);
                order[0] = order[^1] = new(12345, -7, -7);
                for (var i = 0; i < count; i++)
                {
                    var y = shape switch
                    {
                        0 => i, 1 => count - i, 2 => 1, 3 => i % 3,
                        4 => i % 4 switch { 0 => double.NaN, 1 => double.NegativeInfinity, 2 => double.PositiveInfinity, _ => -0.0 },
                        _ => random.Next(-50, 51)
                    };
                    edges[i + 1] = new(new(i + .25, y), new(-i - .5, y), i);
                    order[i + 1] = new(y, i, i);
                }
                var expected = edges.AsSpan(1, count).ToArray();
                Array.Sort(expected, (a, b) => { var byY = a.MinY.CompareTo(b.MinY); return byY != 0 ? byY : a.Order.CompareTo(b.Order); });
                Array.Sort(order, 1, count, Comparer<Crossing>.Create((a, b) => { var byY = a.X.CompareTo(b.X); return byY != 0 ? byY : a.Order.CompareTo(b.Order); }));
                if (cancel)
                {
                    var originalEdges = edges.ToArray(); var originalOrder = order.ToArray();
                    using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
                    var failure = Assert.Throws<OperationCanceledException>(() => ReorderEdges(edges.AsSpan(1, count), order.AsSpan(1, count), cancellation.Token));
                    Assert.Equal(cancellation.Token, failure.CancellationToken);
                    Assert.Equal(originalEdges, edges); Assert.Equal(originalOrder, order);
                }
                ReorderEdges(edges.AsSpan(1, count), order.AsSpan(1, count), default);
                Assert.Equal(expected.Select(e => e.Order), edges.AsSpan(1, count).ToArray().Select(e => e.Order));
                for (var i = 0; i < count; i++)
                {
                    var a = expected[i]; var b = edges[i + 1];
                    Assert.Equal(BitConverter.DoubleToInt64Bits(a.A.X), BitConverter.DoubleToInt64Bits(b.A.X));
                    Assert.Equal(BitConverter.DoubleToInt64Bits(a.A.Y), BitConverter.DoubleToInt64Bits(b.A.Y));
                    Assert.Equal(BitConverter.DoubleToInt64Bits(a.B.X), BitConverter.DoubleToInt64Bits(b.B.X));
                    Assert.Equal(BitConverter.DoubleToInt64Bits(a.B.Y), BitConverter.DoubleToInt64Bits(b.B.Y));
                    Assert.Equal(-1, order[i + 1].Index);
                }
                Assert.Equal(-7, edges[0].Order); Assert.Equal(-7, edges[^1].Order);
                Assert.Equal(-7, order[0].Index); Assert.Equal(-7, order[^1].Index);
            }
        }
    }
}
