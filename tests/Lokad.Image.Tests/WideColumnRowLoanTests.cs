using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests
{
    public sealed class WideColumnBicubicRowLoanTests
    {
        [Fact]
        public void LayoutKeepsBorrowedStorageInsideUnpublishedRows()
        {
            foreach (var (width, height, depth, eligible) in new[]
            {
                (128, 64, 8, true), (128, 64, 16, false), (128, 39, 8, true),
                (128, 38, 8, false), (75, 47, 8, false), (17, 17, 8, false), (11, 9, 8, false)
            })
            {
                using var result = ImageBuffer.Create(width, height, depth);
                var allocated = GC.GetAllocatedBytesForCurrentThread();
                var actual = WideColumnBicubicResize.TryLocateRowLoan(result, out var first, out var start);
                Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - allocated);
                Assert.Equal(eligible, actual);
                if (!eligible) { Assert.Equal(0, first); Assert.Equal(0, start); continue; }
                Assert.True(first >= 17); Assert.Equal(first * result.Stride, start);
                Assert.True(start + 10880 <= result.ByteLength);
                Assert.True(result.ByteLength - start - 10880 < result.Stride);
                if (height == 64) { Assert.Equal(42, first); Assert.Equal(21504, start); }
            }
        }

        [Fact]
        public void TailMetadataMatchesWholePreparationWithoutTouchingCompletedRows()
        {
            foreach (var depth in new[] { 8, 16 })
            {
                using var expected = ImageBuffer.Create(128, 64, depth);
                using var actual = ImageBuffer.Create(128, 64, depth);
                expected.Pixels.Fill(0xa5); actual.Pixels.Fill(0xa5);
                _ = ColumnBicubicGeometry.Prepare(expected, 259, 259d / 64, 17, 4, default);
                _ = ColumnBicubicGeometry.PrepareTail(actual, 259, 259d / 64, 17, 4, 42, default);
                const int bytes = 16 + 17 * sizeof(double);
                var offset = actual.Stride - bytes;
                for (var y = 0; y < 64; y++)
                {
                    var row = actual.Pixels.Slice(y * actual.Stride, actual.Stride);
                    if (y < 42) Assert.Equal(-1, row.IndexOfAnyExcept((byte)0xa5));
                    else
                    {
                        Assert.Equal(-1, row[..offset].IndexOfAnyExcept((byte)0xa5));
                        Assert.Equal(expected.Pixels.Slice(y * expected.Stride + offset, bytes).ToArray(), row[offset..].ToArray());
                    }
                }
                var allocated = GC.GetAllocatedBytesForCurrentThread();
                _ = ColumnBicubicGeometry.PrepareTail(actual, 259, 259d / 64, 17, 4, 42, default);
                Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - allocated);
                actual.Pixels.Fill(0xe3);
                Assert.Throws<OperationCanceledException>(() => ColumnBicubicGeometry.PrepareTail(actual, 259, 259d / 64,
                    17, 4, 42, new CancellationToken(true)));
                Assert.Equal(-1, actual.Pixels.IndexOfAnyExcept((byte)0xe3));
                Assert.Throws<ArgumentOutOfRangeException>(() => ColumnBicubicGeometry.PrepareTail(actual, 259, 259d / 64,
                    17, 4, -1, default));
            }
            using var narrow = ImageBuffer.Create(17, 64, 8); narrow.Pixels.Fill(0xa5);
            var view = ColumnBicubicGeometry.PrepareTail(narrow, 259, 259d / 64, 17, 4, 42, default);
            Assert.Equal(0, view.PrefixColumns); Assert.Equal(-1, narrow.Pixels.IndexOfAnyExcept((byte)0xa5));
        }

        [Theory]
        [InlineData(513, 259, 128, 64)]
        [InlineData(528, 264, 128, 64)]
        [InlineData(307, 400, 75, 100)] // Odd stride and a partial sixteen-column strip.
        [InlineData(68, 800, 17, 200)] // No retained row-footer geometry.
        public void ActualPhasesRetireLoanBeforePoisonedTailAndCancellation(int width, int height, int outputWidth, int outputHeight)
            => WideColumnBicubicResize.VerifyRowLoanPhases(width, height, outputWidth, outputHeight);
    }
}

namespace Lokad.Image.Internal
{
    internal static partial class WideColumnBicubicResize
    {
        internal static void VerifyRowLoanPhases(int width, int height, int outputWidth, int outputHeight)
        {
            if (!Avx512F.IsSupported || !Avx2.IsSupported || !Sse41.IsSupported) return;
            using var source = OriginalPixels.Buffer(width, height, 8);
            for (var y = 0; y < height; y++) for (var x = 0; x < width; x++) source.GetRow8(y)[x * 4 + 3] = 255;
            var input = OriginalPixels.Bytes(source);
            using var expected = ImageTransforms.Resize(source, outputWidth, outputHeight, ResizeFilter.Bicubic,
                new() { MaxWorkingBytes = source.ByteLength + outputWidth * outputHeight * 4 });
            var golden = OriginalPixels.Bytes(expected); using var actual = ImageBuffer.Create(outputWidth, outputHeight, 8);
            Assert.True(TryLocateRowLoan(actual, out var first, out var start));
            using var root = new TestMemoryBudget(4096);
            foreach (var cancel in new[] { false, true })
            {
                using var cancellation = new CancellationTokenSource();
                using var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, cancellation.Token);
                context.Reserve(4096);
                try
                {
                    var sx = (double)width / outputWidth; var sy = (double)height / outputHeight;
                    Assert.True(ApplyRange<RowLoanColumns, ByteSamples>(source, actual, sx, sy, context));
                    Assert.True(ApplyRange<TailColumns, ByteSamples>(source, actual, sx, sy, context));
                    actual.Pixels.Fill(0xa5);
                    var allocated = GC.GetAllocatedBytesForCurrentThread();
                    var top = ApplyRange<RowLoanColumns, ByteSamples>(source, actual, sx, sy, context);
                    Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - allocated); Assert.True(top);
                    Assert.Equal(golden.AsSpan(0, start).ToArray(), actual.Pixels[..start].ToArray());
                    var completed = actual.Pixels[..start].ToArray();
                    // All cache/weight readers have returned. Destroy the entire
                    // loan, including its old metadata, before using the tail.
                    actual.Pixels[start..].Fill(0xe3);
                    if (cancel)
                    {
                        cancellation.Cancel();
                        Assert.Throws<OperationCanceledException>(() => ApplyRange<TailColumns, ByteSamples>(
                            source, actual, sx, sy, context));
                        Assert.Equal(-1, actual.Pixels[start..].IndexOfAnyExcept((byte)0xe3));
                    }
                    else
                    {
                        allocated = GC.GetAllocatedBytesForCurrentThread();
                        var tail = ApplyRange<TailColumns, ByteSamples>(source, actual, sx, sy, context);
                        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - allocated); Assert.True(tail);
                        Assert.Equal(golden, OriginalPixels.Bytes(actual));
                    }
                    Assert.Equal(completed, actual.Pixels[..start].ToArray()); Assert.Equal(4096, root.Bytes);
                }
                finally { context.Release(4096); }
                Assert.Equal(0, root.Bytes); Assert.Equal(4096, root.Peak); Assert.False(root.IsDisposed);
                using var recovered = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default);
                Assert.True(TryApply(source, actual, (double)width / outputWidth, (double)height / outputHeight,
                    (long)Math.Ceiling(4d * width / outputWidth) + 1, (long)Math.Ceiling(4d * height / outputHeight) + 1, recovered));
                Assert.Equal(golden, OriginalPixels.Bytes(actual)); Assert.Equal(0, root.Bytes);
                Assert.Equal(input, OriginalPixels.Bytes(source)); Assert.Equal(4096, recovered.RemainingWorkingBytes);
            }
            source.Dispose(); Assert.Equal(golden, OriginalPixels.Bytes(actual)); Assert.False(root.IsDisposed);
        }
    }
}
