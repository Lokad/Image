using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests
{
    public sealed class TextNaturalRunTests
    {
        private readonly record struct Item(double Key, int Ordinal, int Tag);
        private readonly struct Order : TextRasterSort.IOrder<Item>
        {
            public static int Compare(in Item a, in Item b)
            { var order = a.Key.CompareTo(b.Key); return order != 0 ? order : a.Ordinal.CompareTo(b.Ordinal); }
        }
        [Theory]
        [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
        public void BufferedRunsAndFallbackPreserveKeysOrdinalsAndPayload(int kind)
        {
            int[] lengths = kind switch { 0 => [64, 48, 112, 32], 1 => [129, 130],
                2 => Enumerable.Repeat(17, 17).ToArray(), _ => [17, 65, 41] };
            var items = new List<Item>();
            for (var run = 0; run < lengths.Length; run++)
                for (var i = 0; i < lengths[run]; i++)
                { var ordinal = items.Count; items.Add(new((lengths.Length - run) * 3 + i / 4, ordinal, ~ordinal)); }
            var expected = items.OrderBy(x => x.Key).ThenBy(x => x.Ordinal).ToArray(); var actual = items.ToArray();
            TextRasterSort.SortGeometry<Item, Order>(actual, default); Assert.Equal(expected, actual);
        }
        [Theory] [InlineData(8)] [InlineData(16)]
        public void CanceledBufferedCrossingMergeDoesNotAffectTheNextDraw(int depth)
            => TextRasterizer.VerifyBufferedCrossingRecovery(depth);
    }
}

namespace Lokad.Image.Internal
{
    internal sealed partial class TextRasterizer
    {
        [ThreadStatic] private static CancellationTokenSource? runCancellation;
        [ThreadStatic] private static int runComparisons;
        private readonly struct CancelRunOrder : TextRasterSort.IOrder<Crossing>
        {
            public static int Compare(in Crossing a, in Crossing b)
            {
                if (--runComparisons == 0) runCancellation!.Cancel();
                return CrossingOrder.Compare(in a, in b);
            }
        }
        internal static void VerifyBufferedCrossingRecovery(int depth)
        {
            var contours = Enumerable.Range(0, 24).Select(i => new (int, int, bool)[]
                { (i * 40, 0, true), (i * 40, 600, true), (i * 40 + 20, 600, true), (i * 40 + 20, 0, true) }).ToArray();
            using var font = TrueTypeFont.Load(AuthoredTrueType.Build(glyphOverrides: new Dictionary<int, byte[]>
                { [2] = AuthoredTrueType.Outline(contours) }));
            using var layout = TextLayout.Create(font, "A", new() { FontSize = 18 });
            using var budget = new TestMemoryBudget(4_000_000);
            using var context = new OperationContext(new() { MemoryBudget = budget }, default);
            using var raster = new TextRasterizer(layout, context, 64); var held = budget.Bytes;
            Assert.Equal(48, raster.crossings.Length);
            for (var i = 0; i < raster.crossings.Length; i++) raster.crossings[i] = new((4 - i / 12) * 100 + i % 12, i, i);
            using var cancellation = new CancellationTokenSource(); runCancellation = cancellation; runComparisons = 56;
            try
            {
                var failure = Assert.Throws<OperationCanceledException>(() => TextRasterSort.SortGeometry<Crossing, CancelRunOrder>(raster.crossings, cancellation.Token));
                Assert.Equal(cancellation.Token, failure.CancellationToken); Assert.Equal(held, budget.Bytes);
            }
            finally { runCancellation = null; }
            using var actual = OriginalPixels.Buffer(64, 48, depth); using var expected = actual.Clone();
            var ink = new RgbaColor(12345, 54321, 32769, 45678);
            raster.Draw(actual, 4.375, 22.625, ink, 0, 0, 64, 48, default);
            ImageDrawing.DrawText(expected, layout, 4.375, 22.625, ink);
            Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(actual));
            raster.Dispose(); Assert.Equal(0, budget.Bytes); Assert.False(budget.IsDisposed);
        }
    }
}
