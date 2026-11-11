using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests
{
    public sealed class DeepRowDrivenBicubicTests
    {
        [Theory]
        [InlineData(37, false)] [InlineData(64, false)] [InlineData(65, false)] [InlineData(72, false)]
        [InlineData(37, true)] [InlineData(64, true)] [InlineData(65, true)] [InlineData(72, true)]
        public void PreparedColumnsPreserveEveryOrderedDoubleAndIgnorePadding(int taps, bool clamped)
        {
            if (!Avx512F.IsSupported || !Avx2.IsSupported || !Sse41.IsSupported) return;
            var products = Enumerable.Repeat(ushort.MaxValue, 94 * 4).ToArray();
            for (var p = 0; p < 92; p++) for (var c = 0; c < 4; c++) products[4 + p * 4 + c] = (ushort)((p * 1709 + c * 719) % 65026);
            var offsets = Enumerable.Repeat(ushort.MaxValue, taps + 2).ToArray();
            var weights = Enumerable.Repeat(double.NaN, taps + 2).ToArray();
            for (var t = 0; t < taps; t++)
            {
                offsets[t + 1] = (ushort)(clamped ? Math.Clamp(t - 7, 0, 51) : t);
                weights[t + 1] = (t % 5) switch { 0 => .1, 1 => -1d / 3, 2 => .75, 3 => -0d, _ => 1d / 7 };
            }
            var original = products.ToArray();
            var actual = DeepColumnBicubicResize.FilterPreparedColumn(products.AsSpan(4, 92 * 4), offsets.AsSpan(1, taps), weights.AsSpan(1, taps));
            for (var c = 0; c < 4; c++)
            {
                double even = 0, odd = 0; var t = 0;
                for (; t + 1 < taps; t += 2)
                {
                    even += Product(products[4 + offsets[t + 1] * 4 + c], weights[t + 1]);
                    odd += Product(products[4 + offsets[t + 2] * 4 + c], weights[t + 2]);
                }
                var expected = even + odd;
                if (t < taps) expected += Product(products[4 + offsets[t + 1] * 4 + c], weights[t + 1]);
                Assert.Equal(BitConverter.DoubleToInt64Bits(expected), BitConverter.DoubleToInt64Bits(actual.GetElement(c)));
            }
            Assert.Equal(original, products);
            var allocation = GC.GetAllocatedBytesForCurrentThread();
            for (var repeat = 0; repeat < 4; repeat++)
                DeepColumnBicubicResize.FilterPreparedColumn(products.AsSpan(4, 92 * 4), offsets.AsSpan(1, taps), weights.AsSpan(1, taps));
            Assert.Equal(allocation, GC.GetAllocatedBytesForCurrentThread());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static double Product(double a, double b) => a * b;

        [Theory]
        [InlineData(513, 259, 32, 16)]
        [InlineData(401, 245, 23, 14)]
        public void ActualRowFramePreservesPoisonedPixelsAdmissionAndCancellation(int sw, int sh, int width, int height)
            => DeepColumnBicubicResize.VerifyRows(sw, sh, width, height);

        [Fact]
        public void TinyAlphaDeclineRetiresAdmissionBeforePublicReplay()
        {
            if (!Avx512F.IsSupported || !Avx2.IsSupported || !Sse41.IsSupported) return;
            using var source = ImageBuffer.Create(121, 61, 8);
            for (var y = 0; y < source.Height; y++) for (var x = 0; x < source.Width; x++)
            {
                var p = source.GetRow8(y).Slice(x * 4, 4);
                p[0] = 19; p[1] = 103; p[2] = 241; p[3] = (byte)((x + y) % 3 == 0 ? 1 : 0);
            }
            var input = OriginalPixels.Bytes(source);
            using var expected = ImageTransforms.Resize(source, 7, 4, ResizeFilter.Bicubic);
            using var abandoned = ImageBuffer.Create(7, 4, 8); abandoned.Pixels.Fill(0xa5);
            using var root = new TestMemoryBudget(abandoned.ByteLength + 4096);
            using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default))
                Assert.False(DeepColumnBicubicResize.TryApply(source, abandoned, 121d / 7, 61d / 4, 72, 72, context));
            Assert.Equal(0, root.Bytes);
            using (var result = ImageTransforms.Resize(source, 7, 4, ResizeFilter.Bicubic,
                new() { MaxWorkingBytes = input.Length + abandoned.ByteLength + 4096, MemoryBudget = root }))
            {
                Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(result));
                Assert.Equal(abandoned.ByteLength, root.Bytes);
            }
            Assert.Equal(input, OriginalPixels.Bytes(source)); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        }
    }
}

namespace Lokad.Image.Internal
{
    internal static partial class DeepColumnBicubicResize
    {
        internal static void VerifyRows(int sw, int sh, int width, int height)
        {
            if (!Avx512F.IsSupported || !Avx2.IsSupported || !Sse41.IsSupported) return;
            using var source = OriginalPixels.Buffer(sw, sh, 8);
            using var expected = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic);
            var golden = OriginalPixels.Bytes(expected); var input = OriginalPixels.Bytes(source);
            using var actual = ImageBuffer.Create(width, height, 8);
            using var root = new TestMemoryBudget(4096);
            using (var cancellation = new CancellationTokenSource())
            using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, cancellation.Token))
            {
                context.Reserve(4096);
                Assert.True(ApplyRows(source, actual, (double)sw / width, (double)sh / height, context));
                actual.Pixels.Fill(0xa5);
                var allocation = GC.GetAllocatedBytesForCurrentThread();
                Assert.True(ApplyRows(source, actual, (double)sw / width, (double)sh / height, context));
                Assert.Equal(allocation, GC.GetAllocatedBytesForCurrentThread());
                Assert.Equal(golden, OriginalPixels.Bytes(actual)); Assert.Equal(4096, root.Bytes);
                cancellation.Cancel(); actual.Pixels.Fill(0xe3);
                var error = Assert.Throws<OperationCanceledException>(() => ApplyRows(source, actual, (double)sw / width, (double)sh / height, context));
                Assert.Equal(cancellation.Token, error.CancellationToken);
                Assert.Equal(-1, actual.Pixels.IndexOfAnyExcept((byte)0xe3));
            }
            Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.Equal(input, OriginalPixels.Bytes(source));
        }
    }
}
