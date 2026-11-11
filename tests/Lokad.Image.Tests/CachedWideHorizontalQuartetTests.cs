using Lokad.Image.Tooling;

namespace Lokad.Image.Tests
{
    public sealed class CachedWideHorizontalQuartetTests
    {
        [Theory]
        [InlineData(513, 128)] [InlineData(512, 128)] [InlineData(517, 127)]
        [InlineData(96, 24)] [InlineData(33, 8)] [InlineData(33, 7)]
        [InlineData(7, 5)] [InlineData(1, 5)]
        public void LiveGeometryAndEveryOrderedDoubleStayExact(int sourceWidth, int width)
            => Internal.CachedBicubicResize.VerifyHorizontalQuartets(sourceWidth, width, -1);

        [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
        public void UnequalShiftedShortAndClampedWindowsCannotAcquireProof(int variant)
            => Internal.CachedBicubicResize.VerifyHorizontalQuartets(variant == 2 ? 27 : 96, 4, variant);

        [Fact]
        public void CanceledPreparationDoesNotMutateBorrowedGeometry()
            => Internal.CachedBicubicResize.VerifyHorizontalQuartetCancellation(false);

        [Fact]
        public void CanceledExecutionDoesNotMutateBorrowedRows()
            => Internal.CachedBicubicResize.VerifyHorizontalQuartetCancellation(true);

        [Fact]
        public void PreparationAndExecutionAllocateNothingAndDoNotAdmitBorrowedStorage()
            => Internal.CachedBicubicResize.VerifyHorizontalQuartetAllocation();

        [Theory] [InlineData(8, false)] [InlineData(16, false)]
        [InlineData(8, true)] [InlineData(16, true)]
        public void PublicPlansKeepRationalPixelsAndIndependentOwners(int depth, bool tiny)
        {
            using var source = OriginalPixels.Buffer(97, 37, depth);
            if (tiny) for (var y = 0; y < 37; y++) for (var x = 0; x < 97; x++)
                source.SetSample(x, y, 3, x < 49 ? 1 : 0);
            var input = OriginalPixels.Bytes(source);
            var expected = ExactResizeReference.Resize(input, 97, 37, depth, 24, 9, ResizeFilter.Bicubic);
            using var root = new TestMemoryBudget(1 << 20);
            using var plan = BicubicResizePlan.Create(97, 37, 24, 9, depth, new() { MemoryBudget = root });
            Assert.Throws<OperationCanceledException>(() => plan.Resize(source, new CancellationToken(true)));
            Assert.Equal(plan.StorageBytes, root.Bytes);
            using var first = plan.Resize(source); using var survivor = plan.Resize(source);
            Assert.Equal(expected, OriginalPixels.Bytes(first)); Assert.Equal(expected, OriginalPixels.Bytes(survivor));
            Assert.Equal(plan.StorageBytes + expected.Length * 2L, root.Bytes);
            if (depth == 8) first.GetRow8(0)[0] ^= 127; else first.GetRow16(0)[0] ^= 32767;
            plan.Dispose(); first.Dispose(); Assert.Equal(expected.Length, root.Bytes);
            Assert.Equal(input, OriginalPixels.Bytes(source)); source.Dispose();
            Assert.Equal(expected, OriginalPixels.Bytes(survivor)); survivor.Dispose(); Assert.Equal(0, root.Bytes);
            Assert.False(root.IsDisposed);
        }
    }
}

namespace Lokad.Image.Internal
{
    using System.Runtime.CompilerServices;
    using System.Runtime.Intrinsics.X86;
    using Lokad.Image.Tooling;

    internal static partial class CachedBicubicResize
    {
        internal static void VerifyHorizontalQuartets(int sourceWidth, int width, int variant)
        {
            if (!Avx.IsSupported) return;
            var weights = new double[width * 32]; var offsets = new int[width * 32]; var counts = new int[width];
            var scale = Math.Max(1, (double)sourceWidth / width);
            for (var x = 0; x < width; x++)
            {
                var center = (x + .5) * sourceWidth / width - .5;
                var first = (int)Math.Ceiling(center - 2 * scale);
                counts[x] = (int)Math.Floor(center + 2 * scale) - first + 1;
                if (variant >= 0)
                {
                    first = x * 4 + (variant == 1 && x >= 2 ? 1 : variant == 3 ? -1 : 0);
                    counts[x] = variant == 0 && x == 2 ? 17 : 16;
                }
                for (var t = 0; t < counts[x]; t++)
                {
                    offsets[x * 32 + t] = Math.Clamp(first + t, 0, sourceWidth - 1);
                    weights[x * 32 + t] = variant < 0 ? ImageTransforms.Kernel((first + t - center) / scale, ResizeFilter.Bicubic)
                        : t % 2 == 0 ? (t + 1) * .1 : -(t + 1) * .0625;
                }
            }
            using var root = new TestMemoryBudget(0);
            using var context = new OperationContext(new() { MemoryBudget = root }, default);
            PrepareWideHorizontalPairs(offsets, counts, width, sourceWidth, 32, context);
            // Poison only padding beyond either lane's maximum-count read.
            for (var x = 0; x < width; x++)
            {
                var partner = x ^ 1; var count = partner < width ? Math.Max(counts[x], counts[partner]) : counts[x];
                for (var t = count; t < 32; t++) weights[x * 32 + t] = double.NaN;
            }
            var oldWeights = weights.ToArray(); var oldOffsets = offsets.ToArray(); var oldCounts = counts.ToArray();
            PrepareWideHorizontalQuartets(weights, offsets, counts, width, sourceWidth, context);
            var allowed = new bool[weights.Length]; var heads = new HashSet<int>();
            for (var x = 0; x + 3 < width; x += 2)
            {
                if (offsets[x * 32 + 30] != PreparedWideQuartet) continue;
                heads.Add(x);
                for (var c = 0; c < 4; c++) Assert.Equal(16, counts[x + c]);
                for (var t = 0; t < 16; t++) for (var c = 0; c < 4; c++)
                {
                    var at = x * 32 + t / 4 * 32 + 16 + t % 4 * 4 + c; allowed[at] = true;
                    Assert.Equal(BitConverter.DoubleToInt64Bits(oldWeights[(x + c) * 32 + t]), BitConverter.DoubleToInt64Bits(weights[at]));
                }
                x += 2;
            }
            if (variant >= 0) Assert.Empty(heads);
            if (sourceWidth == 513 && width == 128) Assert.Equal(30, heads.Count);
            if (sourceWidth == 512 && width == 128) Assert.Equal(31, heads.Count);
            if (sourceWidth == 517 && width == 127) Assert.Equal(16, heads.Count);
            for (var i = 0; i < weights.Length; i++)
                if (!allowed[i]) Assert.Equal(BitConverter.DoubleToInt64Bits(oldWeights[i]), BitConverter.DoubleToInt64Bits(weights[i]));
            for (var i = 0; i < offsets.Length; i++)
                if (i % 32 != 30 || !heads.Contains(i / 32)) Assert.Equal(oldOffsets[i], offsets[i]);
            Assert.Equal(oldCounts, counts);
            foreach (var maximum in new[] { 255, 65535 }) foreach (var tiny in new[] { false, true })
            {
                var random = new Random(826); var decoded = new double[sourceWidth * 4];
                for (var x = 0; x < sourceWidth; x++)
                {
                    var alpha = tiny ? random.Next(2) : random.Next(maximum + 1);
                    for (var c = 0; c < 4; c++) decoded[x * 4 + c] = c == 3 ? alpha : (double)alpha * random.Next(maximum + 1);
                }
                var source = decoded.ToArray(); var output = Enumerable.Repeat(double.NaN, width * 4 + 16).ToArray();
                var prepared = weights.ToArray(); var indices = offsets.ToArray();
                FilterWideHorizontal(decoded, output.AsSpan(8, width * 4), weights, offsets, counts, width, 32, context);
                for (var x = 0; x < width; x++) for (var c = 0; c < 4; c++)
                {
                    var partner = x ^ 1; var n = partner < width ? Math.Max(counts[x], counts[partner]) : counts[x]; double expected = 0;
                    for (var t = 0; t < n; t++) expected += QuartetOracleProduct(decoded[oldOffsets[x * 32 + t] * 4 + c], oldWeights[x * 32 + t]);
                    Assert.Equal(BitConverter.DoubleToInt64Bits(expected), BitConverter.DoubleToInt64Bits(output[8 + x * 4 + c]));
                }
                Assert.All(output.Take(8).Concat(output.Skip(width * 4 + 8)), v => Assert.True(double.IsNaN(v)));
                Assert.Equal(source, decoded); Assert.Equal(prepared, weights); Assert.Equal(indices, offsets);
            }
            Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Attempts); Assert.False(root.IsDisposed);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static double QuartetOracleProduct(double a, double b) => a * b;

        internal static void VerifyHorizontalQuartetCancellation(bool execute)
        {
            if (!Avx.IsSupported) return;
            var weights = new double[128]; var offsets = new int[128]; var counts = new[] { 16, 16, 16, 16 };
            for (var x = 0; x < 4; x++) for (var t = 0; t < 16; t++) { weights[x * 32 + t] = 1; offsets[x * 32 + t] = x * 4 + t; }
            using var root = new TestMemoryBudget(0); using var cancellation = new CancellationTokenSource();
            using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
            PrepareWideHorizontalPairs(offsets, counts, 4, 28, 32, context);
            if (execute) PrepareWideHorizontalQuartets(weights, offsets, counts, 4, 28, context);
            var beforeWeights = weights.ToArray(); var beforeOffsets = offsets.ToArray();
            var decoded = new double[112]; var output = Enumerable.Repeat(double.NaN, 16).ToArray(); cancellation.Cancel();
            if (execute) Assert.Throws<OperationCanceledException>(() => FilterWideHorizontal(decoded, output, weights, offsets, counts, 4, 32, context));
            else Assert.Throws<OperationCanceledException>(() => PrepareWideHorizontalQuartets(weights, offsets, counts, 4, 28, context));
            Assert.Equal(beforeWeights, weights); Assert.Equal(beforeOffsets, offsets); Assert.All(output, v => Assert.True(double.IsNaN(v)));
            Assert.Equal(0, root.Attempts); Assert.Equal(0, root.Bytes);
        }

        internal static void VerifyHorizontalQuartetAllocation()
        {
            if (!Avx.IsSupported) return;
            var weights = new double[128]; var offsets = new int[128]; var counts = new[] { 16, 16, 16, 16 };
            for (var x = 0; x < 4; x++) for (var t = 0; t < 16; t++) { weights[x * 32 + t] = .25; offsets[x * 32 + t] = x * 4 + t; }
            using var root = new TestMemoryBudget(0); using var context = new OperationContext(new() { MemoryBudget = root }, default);
            var decoded = new double[112]; var output = new double[16];
            PrepareWideHorizontalPairs(offsets, counts, 4, 28, 32, context);
            PrepareWideHorizontalQuartets(weights, offsets, counts, 4, 28, context);
            FilterWideHorizontal(decoded, output, weights, offsets, counts, 4, 32, context);
            var start = GC.GetAllocatedBytesForCurrentThread();
            for (var i = 0; i < 10; i++)
            {
                PrepareWideHorizontalQuartets(weights, offsets, counts, 4, 28, context);
                FilterWideHorizontal(decoded, output, weights, offsets, counts, 4, 32, context);
            }
            Assert.Equal(start, GC.GetAllocatedBytesForCurrentThread()); Assert.Equal(0, root.Attempts); Assert.Equal(0, root.Bytes);
        }
    }
}
