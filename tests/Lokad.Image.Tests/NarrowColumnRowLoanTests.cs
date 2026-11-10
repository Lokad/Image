using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests
{
    public sealed class NarrowColumnBicubicRowLoanTests
    {
        [Fact]
        public void RowLoanBoundsKeepTopWritesDisjointAndRejectUnsupportedLayouts()
        {
            foreach (var (width, height, depth, slots, eligible) in new[]
            {
                (256, 129, 8, 9, true), (256, 129, 16, 9, false),
                (256, 21, 8, 9, true), (256, 20, 8, 9, false),
                (151, 129, 8, 9, true), (64, 64, 8, 10, true),
                (32, 128, 8, 9, true), (31, 128, 8, 9, false),
                (75, 41, 8, 9, false), (256, 129, 8, 0, false), (256, 129, 8, 11, false)
            })
            {
                using var result = ImageBuffer.Create(width, height, depth);
                var allocated = GC.GetAllocatedBytesForCurrentThread();
                var actual = ColumnBicubicResize.TryLocateRowLoan(result, slots, out var first, out var start);
                Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - allocated); Assert.Equal(eligible, actual);
                if (!eligible) { Assert.Equal(0, first); Assert.Equal(0, start); continue; }
                var bytes = slots * 32 * 32 + 2560;
                Assert.True(first >= slots); Assert.Equal(first * result.Stride, start);
                Assert.True(start + bytes <= result.ByteLength);
                Assert.True(result.ByteLength - start - bytes < result.Stride);
                if (width == 256 && height == 129) { Assert.Equal(117, first); Assert.Equal(119808, start); }
            }
        }

        [Theory]
        [InlineData(513, 259, 256, 129)]
        [InlineData(512, 256, 256, 128)]
        [InlineData(144, 144, 64, 64)] // Ten row slots and six-column tail strips.
        [InlineData(307, 259, 151, 129)] // Unaligned row stride and a partial32-column strip.
        [InlineData(64, 256, 32, 128)] // No retained row-footer geometry.
        public void ActualRowLoanRetiresBeforePoisonedTailAndCancellation(int width, int height, int outputWidth, int outputHeight)
            => ColumnBicubicResize.VerifyRowLoanPhases(width, height, outputWidth, outputHeight);

        [Theory]
        [InlineData(false)] [InlineData(true)]
        public void LowAlphaAndMixedPixelsPreserveAdmissionFailureCleanupAndSurvivors(bool mixed)
        {
            const int width = 256, height = 129, bytes = width * height * 4;
            using var source = OriginalPixels.Buffer(513, 259, 8);
            for (var y = 0; y < source.Height; y++) for (var x = 0; x < source.Width; x++)
                source.GetRow8(y)[x * 4 + 3] = mixed ? (byte)(((x + y) % 4) switch { 0 => 0, 1 => 1, 2 => 2, _ => 255 }) : (byte)1;
            var input = OriginalPixels.Bytes(source);
            using var oracle = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic,
                new() { MaxWorkingBytes = source.ByteLength + bytes });
            var golden = OriginalPixels.Bytes(oracle);
            using var root = new TestMemoryBudget(2L * bytes + 4096);
            var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = source.ByteLength + bytes + 4096 };
            using var first = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic, limits);
            using var survivor = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic, limits);
            Assert.Equal(golden, OriginalPixels.Bytes(first)); Assert.Equal(golden, OriginalPixels.Bytes(survivor));
            Assert.Equal(2L * bytes, root.Bytes); Assert.True(root.Peak <= 2L * bytes + 4096);
            using (var caller = root.CreateScope())
            {
                Assert.True(caller.TryReserve(4096, out var held)); var refusals = root.Refusals;
                var error = Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic, limits));
                Assert.Equal(ImageLimitScope.SharedBudget, error.Scope); Assert.Equal(refusals + 1, root.Refusals);
                Assert.Equal(2L * bytes + held!.Bytes, root.Bytes);
            }
            first.Dispose(); Assert.Equal(bytes, root.Bytes);
            using var cancellation = new CancellationTokenSource(); root.Admitted = _ => cancellation.Cancel();
            Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic, limits, cancellation.Token));
            root.Admitted = null; Assert.Equal(bytes, root.Bytes);
            using (var recovered = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic, limits))
                Assert.Equal(golden, OriginalPixels.Bytes(recovered));
            Assert.Equal(input, OriginalPixels.Bytes(source)); source.Dispose();
            Assert.Equal(golden, OriginalPixels.Bytes(survivor)); survivor.Dispose();
            Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        }
    }
}

namespace Lokad.Image.Internal
{
    internal static partial class ColumnBicubicResize
    {
        private static bool VerifyTailAfterRestore(ImageBuffer source, ImageBuffer result, double sx, double sy,
            int slots, int first, OperationContext context)
        {
            var geometry = ColumnBicubicGeometry.PrepareTail(result, source.Height, sy, 10,
                slots == 9 ? 8 : 6, first, context.Cancellation);
            return ApplyRange<RowTailStrip, ByteSamples>(source, result, sx, sy, slots,
                geometry, default, 0, 0, result.Width, context);
        }

        internal static void VerifyRowLoanPhases(int width, int height, int outputWidth, int outputHeight)
        {
            if (!Avx512F.IsSupported || !Avx2.IsSupported || !Sse41.IsSupported) return;
            using var source = OriginalPixels.Buffer(width, height, 8);
            for (var y = 0; y < height; y++) for (var x = 0; x < width; x++) source.GetRow8(y)[x * 4 + 3] = 255;
            var input = OriginalPixels.Bytes(source); var bytes = outputWidth * outputHeight * 4;
            using var oracle = ImageTransforms.Resize(source, outputWidth, outputHeight, ResizeFilter.Bicubic,
                new() { MaxWorkingBytes = source.ByteLength + bytes });
            var golden = OriginalPixels.Bytes(oracle); using var actual = ImageBuffer.Create(outputWidth, outputHeight, 8);
            var sx = (double)width / outputWidth; var sy = (double)height / outputHeight; var slots = sy < 2.25 ? 9 : 10;
            Assert.True(TryLocateRowLoan(actual, slots, out var first, out var start));
            using var root = new TestMemoryBudget(4096);
            foreach (var cancel in new[] { false, true })
            {
                using var cancellation = new CancellationTokenSource();
                using var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, cancellation.Token);
                context.Reserve(4096);
                try
                {
                    var geometry = ColumnBicubicGeometry.Prepare(actual, height, sy, 10, slots == 9 ? 8 : 6, default);
                    Assert.True(ApplyRange<RowLoanStrip, ByteSamples>(source, actual, sx, sy, slots, geometry, default, 32, 0, outputWidth, context));
                    Assert.True(VerifyTailAfterRestore(source, actual, sx, sy, slots, first, context));
                    actual.Pixels.Fill(0xa5);
                    var allocated = GC.GetAllocatedBytesForCurrentThread();
                    geometry = ColumnBicubicGeometry.Prepare(actual, height, sy, 10, slots == 9 ? 8 : 6, context.Cancellation);
                    var top = ApplyRange<RowLoanStrip, ByteSamples>(source, actual, sx, sy, slots, geometry, default, 32, 0, outputWidth, context);
                    Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - allocated); Assert.True(top);
                    Assert.Equal(golden.AsSpan(0, start).ToArray(), actual.Pixels[..start].ToArray());
                    var completed = actual.Pixels[..start].ToArray(); actual.Pixels[start..].Fill(0xe3);
                    if (cancel)
                    {
                        cancellation.Cancel();
                        Assert.Throws<OperationCanceledException>(() => VerifyTailAfterRestore(source, actual, sx, sy, slots, first, context));
                        Assert.Equal(-1, actual.Pixels[start..].IndexOfAnyExcept((byte)0xe3));
                    }
                    else
                    {
                        allocated = GC.GetAllocatedBytesForCurrentThread(); var tail = VerifyTailAfterRestore(source, actual, sx, sy, slots, first, context);
                        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - allocated); Assert.True(tail);
                        Assert.Equal(golden, OriginalPixels.Bytes(actual));
                    }
                    Assert.Equal(completed, actual.Pixels[..start].ToArray()); Assert.Equal(4096, root.Bytes);
                }
                finally { context.Release(4096); }
                Assert.Equal(0, root.Bytes); Assert.Equal(4096, root.Peak); Assert.False(root.IsDisposed);
                using var recovered = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default);
                Assert.True(TryApply(source, actual, sx, sy, (long)Math.Ceiling(4 * sx) + 1, (long)Math.Ceiling(4 * sy) + 1, recovered));
                Assert.Equal(golden, OriginalPixels.Bytes(actual)); Assert.Equal(0, root.Bytes);
                Assert.Equal(input, OriginalPixels.Bytes(source)); Assert.Equal(4096, recovered.RemainingWorkingBytes);
            }
            source.Dispose(); Assert.Equal(golden, OriginalPixels.Bytes(actual)); Assert.False(root.IsDisposed);
        }
    }
}
