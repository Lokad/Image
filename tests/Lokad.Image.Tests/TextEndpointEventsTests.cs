using Lokad.Image.Tooling;

namespace Lokad.Image.Tests
{
    public sealed class TextEndpointEventsTests
    {
        [Theory]
        [InlineData(2)] [InlineData(8)] [InlineData(17)] [InlineData(65)] [InlineData(257)]
        public void SharedTouchingCrossingAndSignedZeroEventsMatchOriginalSet(int count)
            => Internal.TextRasterizer.VerifyEndpointEvents(count, false);

        [Fact]
        public void CanceledEventPreparationLeavesEdgesAndAdmissionUntouched()
            => Internal.TextRasterizer.VerifyEndpointEvents(65, true);
    }
}

namespace Lokad.Image.Internal
{
    internal sealed partial class TextRasterizer
    {
        internal static void VerifyEndpointEvents(int count, bool cancel)
        {
            using var font = TrueTypeFont.Load(AuthoredTrueType.Build());
            using var layout = TextLayout.Create(font, "AVg", new() { FontSize = 18 });
            for (var kind = 0; kind < 5; kind++)
            {
                using var root = new TestMemoryBudget(16 * 1024 * 1024);
                using var cancellation = new CancellationTokenSource();
                using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
                using (var owner = new TextRasterizer(layout, context, 80))
                using (var shape = new RasterShape(owner))
                {
                    // These edge arrays and ordering scratch are test-owned inputs.
                    // Event storage itself is allocated/admitted by the real builder.
                    var edges = new Edge[count];
                    for (var i = 0; i < count; i++)
                    {
                        var a = kind switch { 0 => i + 1d, 1 => -i - 1d, 2 => i % 2 == 0 ? -0d : 1d, 3 => -1d, _ => i % 3 == 0 ? 0d : -0d };
                        var b = kind switch { 0 => i + 2d, 1 => -i - 2d, 2 => i % 2 == 0 ? 1d : 0d, 3 => 1d, _ => i % 2 == 0 ? -1d : 1d };
                        edges[i] = new(new(i * .5, a), new(kind == 3 ? -i * .5 : i * .5 + .25, b), i);
                    }
                    var unchanged = edges.ToArray();
                    var expected = new List<double>();
                    foreach (var edge in edges) { expected.Add(edge.MinY); expected.Add(edge.MaxY); }
                    // Independent original all-pair event construction; sorting the
                    // interval traversal must not omit any strict interior crossing.
                    for (var i = 0; i < count; i++)
                    for (var j = i + 1; j < count; j++)
                    {
                        var a = edges[i]; var b = edges[j];
                        var low = Math.Max(a.MinY, b.MinY); var high = Math.Min(a.MaxY, b.MaxY);
                        if (!(low < high)) continue;
                        var d0 = a.X(low) - b.X(low); var d1 = a.X(high) - b.X(high);
                        if (d0 < 0 && d1 > 0 || d0 > 0 && d1 < 0)
                        {
                            var y = low + (high - low) * (d0 / (d0 - d1));
                            if (low < y && y < high) expected.Add(y);
                        }
                    }
                    expected.Sort();
                    var unique = new List<double>();
                    foreach (var y in expected) if (unique.Count == 0 || y != unique[^1]) unique.Add(y);
                    shape.Edges = edges; shape.Count = count;
                    if (cancel)
                    {
                        var held = root.Bytes; cancellation.Cancel();
                        var failure = Assert.Throws<OperationCanceledException>(() => shape.BuildEvents(new Crossing[count]));
                        Assert.Equal(cancellation.Token, failure.CancellationToken);
                        Assert.Equal(0, shape.EventCount); Assert.Equal(unchanged, edges); Assert.Equal(held, root.Bytes);
                    }
                    else
                    {
                        shape.BuildEvents(new Crossing[count]); Assert.Equal(unique.Count, shape.EventCount);
                        for (var i = 0; i < unique.Count; i++)
                        {
                            // Zero signs describe the same event coordinate; all
                            // nonzero event/intersection values remain bit exact.
                            if (unique[i] == 0) Assert.Equal(0d, shape.Events[i]);
                            else Assert.Equal(BitConverter.DoubleToInt64Bits(unique[i]), BitConverter.DoubleToInt64Bits(shape.Events[i]));
                        }
                        Assert.Equal(Enumerable.Range(0, count).OrderBy(i => unchanged[i].MinY).ThenBy(i => i),
                            shape.Edges.AsSpan(0, count).ToArray().Select(e => e.Order));
                    }
                }
                Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
            }
        }
    }
}
