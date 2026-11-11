using Lokad.Image.Tooling;

namespace Lokad.Image.Tests
{
    public sealed class CachedHorizontalQuartetTests
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

        [Theory]
        [InlineData(513, 256)] [InlineData(512, 256)] [InlineData(517, 255)]
        [InlineData(97, 48)] [InlineData(33, 16)] [InlineData(33, 15)]
        [InlineData(13, 5)] [InlineData(1, 5)]
        public void NarrowGeometryAndEveryOrderedDoubleStayExact(int sourceWidth, int width)
            => Internal.CachedBicubicResize.VerifyHorizontalQuartets(sourceWidth, width, -1, false);

        [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
        public void NarrowUnequalShiftedShortAndClampedWindowsCannotAcquireProof(int variant)
            => Internal.CachedBicubicResize.VerifyHorizontalQuartets(variant == 2 ? 13 : 48, 4, variant, false);

        [Theory] [InlineData(false)] [InlineData(true)]
        public void NarrowCanceledPreparationAndExecutionPreserveBorrowedStorage(bool execute)
            => Internal.CachedBicubicResize.VerifyHorizontalQuartetCancellation(execute, false);

        [Fact]
        public void NarrowPreparationAndExecutionAllocateNothingAndDoNotAdmitBorrowedStorage()
            => Internal.CachedBicubicResize.VerifyHorizontalQuartetAllocation(false);

        [Theory] [InlineData(8, false, 24, 9)] [InlineData(16, false, 24, 9)]
        [InlineData(8, true, 24, 9)] [InlineData(16, true, 24, 9)]
        [InlineData(8, false, 48, 18)] [InlineData(16, false, 48, 18)]
        [InlineData(8, true, 48, 18)] [InlineData(16, true, 48, 18)]
        public void PublicPlansKeepRationalPixelsAndIndependentOwners(int depth, bool tiny, int width, int height)
        {
            using var source = OriginalPixels.Buffer(97, 37, depth);
            if (tiny) for (var y = 0; y < 37; y++) for (var x = 0; x < 97; x++)
                source.SetSample(x, y, 3, x < 49 ? 1 : 0);
            var input = OriginalPixels.Bytes(source);
            var expected = ExactResizeReference.Resize(input, 97, 37, depth, width, height, ResizeFilter.Bicubic);
            using var root = new TestMemoryBudget(1 << 20);
            using var plan = BicubicResizePlan.Create(97, 37, width, height, depth, new() { MemoryBudget = root });
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
        internal static void VerifyHorizontalQuartets(int sourceWidth, int width, int variant, bool wide = true)
        {
            if (!Avx.IsSupported || !wide && !Avx512F.IsSupported) return;
            var capacity = wide ? 32 : 16; var taps = capacity / 2; var stride = wide ? 4 : 2; var units = wide ? 1 : 4;
            var weights = new double[width * capacity]; var offsets = new int[width * capacity]; var counts = new int[width];
            var scale = Math.Max(1, (double)sourceWidth / width);
            for (var x = 0; x < width; x++)
            {
                var center = (x + .5) * sourceWidth / width - .5;
                var first = (int)Math.Ceiling(center - 2 * scale);
                counts[x] = (int)Math.Floor(center + 2 * scale) - first + 1;
                if (variant >= 0)
                {
                    first = x * stride + (variant == 1 && x >= 2 ? 1 : variant == 3 ? -1 : 0);
                    counts[x] = variant == 0 && x == 2 ? taps + 1 : taps;
                }
                for (var t = 0; t < counts[x]; t++)
                {
                    offsets[x * capacity + t] = Math.Clamp(first + t, 0, sourceWidth - 1) * units;
                    weights[x * capacity + t] = variant < 0 ? ImageTransforms.Kernel((first + t - center) / scale, ResizeFilter.Bicubic)
                        : t % 2 == 0 ? (t + 1) * .1 : -(t + 1) * .0625;
                }
            }
            using var root = new TestMemoryBudget(0);
            using var context = new OperationContext(new() { MemoryBudget = root }, default);
            if (wide) PrepareWideHorizontalPairs(offsets, counts, width, sourceWidth, capacity, context);
            else PrepareOverlappingHorizontalPairs(offsets, counts, width, sourceWidth, context);
            // Poison only padding beyond either lane's maximum-count read.
            for (var x = 0; x < width; x++)
            {
                var partner = x ^ 1; var count = partner < width ? Math.Max(counts[x], counts[partner]) : counts[x];
                for (var t = count; t < capacity; t++) weights[x * capacity + t] = double.NaN;
            }
            var oldWeights = weights.ToArray(); var oldOffsets = offsets.ToArray(); var oldCounts = counts.ToArray();
            if (wide) PrepareHorizontalQuartets<WideHorizontalQuartet>(weights, offsets, counts, width, sourceWidth, context);
            else PrepareHorizontalQuartets<NarrowHorizontalQuartet>(weights, offsets, counts, width, sourceWidth, context);
            var allowed = new bool[weights.Length]; var heads = new HashSet<int>();
            for (var x = 0; x + 3 < width; x += 2)
            {
                if (offsets[x * capacity + capacity - 2] != PreparedHorizontalQuartet) continue;
                heads.Add(x);
                for (var c = 0; c < 4; c++) Assert.Equal(taps, counts[x + c]);
                for (var t = 0; t < taps; t++) for (var c = 0; c < 4; c++)
                {
                    var at = x * capacity + t / (taps / 4) * capacity + taps + t % (taps / 4) * 4 + c; allowed[at] = true;
                    Assert.Equal(BitConverter.DoubleToInt64Bits(oldWeights[(x + c) * capacity + t]), BitConverter.DoubleToInt64Bits(weights[at]));
                }
                x += 2;
            }
            if (variant >= 0) Assert.Empty(heads);
            if (wide && sourceWidth == 513 && width == 128) Assert.Equal(30, heads.Count);
            if (wide && sourceWidth == 512 && width == 128) Assert.Equal(31, heads.Count);
            if (wide && sourceWidth == 517 && width == 127) Assert.Equal(16, heads.Count);
            if (!wide && sourceWidth == 513 && width == 256) Assert.Equal(62, heads.Count);
            for (var i = 0; i < weights.Length; i++)
                if (!allowed[i]) Assert.Equal(BitConverter.DoubleToInt64Bits(oldWeights[i]), BitConverter.DoubleToInt64Bits(weights[i]));
            for (var i = 0; i < offsets.Length; i++)
                if (i % capacity != capacity - 2 || !heads.Contains(i / capacity)) Assert.Equal(oldOffsets[i], offsets[i]);
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
                if (wide) FilterWideHorizontal(decoded, output.AsSpan(8, width * 4), weights, offsets, counts, width, capacity, context);
                else FilterHorizontal(decoded, output.AsSpan(8, width * 4), weights, offsets, counts, width, capacity, false, context);
                for (var x = 0; x < width; x++) for (var c = 0; c < 4; c++)
                {
                    var partner = x ^ 1; var n = partner < width ? Math.Max(counts[x], counts[partner]) : counts[x]; double expected = 0;
                    for (var t = 0; t < n; t++) expected += QuartetOracleProduct(decoded[oldOffsets[x * capacity + t] / units * 4 + c], oldWeights[x * capacity + t]);
                    Assert.Equal(BitConverter.DoubleToInt64Bits(expected), BitConverter.DoubleToInt64Bits(output[8 + x * 4 + c]));
                }
                Assert.All(output.Take(8).Concat(output.Skip(width * 4 + 8)), v => Assert.True(double.IsNaN(v)));
                Assert.Equal(source, decoded); Assert.Equal(prepared, weights); Assert.Equal(indices, offsets);
            }
            Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Attempts); Assert.False(root.IsDisposed);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static double QuartetOracleProduct(double a, double b) => a * b;

        internal static void VerifyHorizontalQuartetCancellation(bool execute, bool wide = true)
        {
            if (!Avx.IsSupported || !wide && !Avx512F.IsSupported) return;
            var capacity = wide ? 32 : 16; var taps = capacity / 2; var stride = wide ? 4 : 2; var units = wide ? 1 : 4; var pixels = wide ? 28 : 14;
            var weights = new double[4 * capacity]; var offsets = new int[4 * capacity]; var counts = new[] { taps, taps, taps, taps };
            for (var x = 0; x < 4; x++) for (var t = 0; t < taps; t++) { weights[x * capacity + t] = 1; offsets[x * capacity + t] = (x * stride + t) * units; }
            using var root = new TestMemoryBudget(0); using var cancellation = new CancellationTokenSource();
            using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
            if (wide) PrepareWideHorizontalPairs(offsets, counts, 4, pixels, capacity, context);
            else PrepareOverlappingHorizontalPairs(offsets, counts, 4, pixels, context);
            if (execute)
            {
                if (wide) PrepareHorizontalQuartets<WideHorizontalQuartet>(weights, offsets, counts, 4, pixels, context);
                else PrepareHorizontalQuartets<NarrowHorizontalQuartet>(weights, offsets, counts, 4, pixels, context);
            }
            var beforeWeights = weights.ToArray(); var beforeOffsets = offsets.ToArray();
            var decoded = new double[pixels * 4]; var output = Enumerable.Repeat(double.NaN, 16).ToArray(); cancellation.Cancel();
            if (execute) Assert.Throws<OperationCanceledException>(() => FilterTestHorizontalQuartets(decoded, output, weights, offsets, counts, capacity, wide, context));
            else Assert.Throws<OperationCanceledException>(() => PrepareTestHorizontalQuartets(weights, offsets, counts, pixels, wide, context));
            Assert.Equal(beforeWeights, weights); Assert.Equal(beforeOffsets, offsets); Assert.All(output, v => Assert.True(double.IsNaN(v)));
            Assert.Equal(0, root.Attempts); Assert.Equal(0, root.Bytes);
        }

        internal static void VerifyHorizontalQuartetAllocation(bool wide = true)
        {
            if (!Avx.IsSupported || !wide && !Avx512F.IsSupported) return;
            var capacity = wide ? 32 : 16; var taps = capacity / 2; var stride = wide ? 4 : 2; var units = wide ? 1 : 4; var pixels = wide ? 28 : 14;
            var weights = new double[4 * capacity]; var offsets = new int[4 * capacity]; var counts = new[] { taps, taps, taps, taps };
            for (var x = 0; x < 4; x++) for (var t = 0; t < taps; t++) { weights[x * capacity + t] = .25; offsets[x * capacity + t] = (x * stride + t) * units; }
            using var root = new TestMemoryBudget(0); using var context = new OperationContext(new() { MemoryBudget = root }, default);
            var decoded = new double[pixels * 4]; var output = new double[16];
            if (wide) PrepareWideHorizontalPairs(offsets, counts, 4, pixels, capacity, context);
            else PrepareOverlappingHorizontalPairs(offsets, counts, 4, pixels, context);
            PrepareTestHorizontalQuartets(weights, offsets, counts, pixels, wide, context);
            FilterTestHorizontalQuartets(decoded, output, weights, offsets, counts, capacity, wide, context);
            var start = GC.GetAllocatedBytesForCurrentThread();
            for (var i = 0; i < 10; i++)
            {
                PrepareTestHorizontalQuartets(weights, offsets, counts, pixels, wide, context);
                FilterTestHorizontalQuartets(decoded, output, weights, offsets, counts, capacity, wide, context);
            }
            Assert.Equal(start, GC.GetAllocatedBytesForCurrentThread()); Assert.Equal(0, root.Attempts); Assert.Equal(0, root.Bytes);
        }
        private static void PrepareTestHorizontalQuartets(Span<double> weights, Span<int> offsets,
            ReadOnlySpan<int> counts, int sourceWidth, bool wide, OperationContext context)
        {
            if (wide) PrepareHorizontalQuartets<WideHorizontalQuartet>(weights, offsets, counts, 4, sourceWidth, context);
            else PrepareHorizontalQuartets<NarrowHorizontalQuartet>(weights, offsets, counts, 4, sourceWidth, context);
        }

        private static void FilterTestHorizontalQuartets(ReadOnlySpan<double> decoded, Span<double> output,
            ReadOnlySpan<double> weights, ReadOnlySpan<int> offsets, ReadOnlySpan<int> counts,
            int capacity, bool wide, OperationContext context)
        {
            if (wide) FilterWideHorizontal(decoded, output, weights, offsets, counts, 4, capacity, context);
            else FilterHorizontal(decoded, output, weights, offsets, counts, 4, capacity, false, context);
        }
    }
}
