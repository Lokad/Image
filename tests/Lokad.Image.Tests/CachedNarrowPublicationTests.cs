using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Tests
{
    public sealed class CachedNarrowPublicationTests
    {
        [Fact]
        public void BorrowedRowsKeepCertifiedSamplesRepairAndCancellationCleanup()
            => Internal.CachedBicubicResize.VerifySharedNarrowPublication();
    }
}

namespace Lokad.Image.Internal
{
    internal static partial class CachedBicubicResize
    {
        internal static void VerifySharedNarrowPublication()
        {
            if (!Avx512F.IsSupported || !Sse41.IsSupported) return;
            const int width = 5, capacity = 16, guard = 4;
            foreach (var alpha in new[] { 0, 1, 15, 16, 17, 255 })
            {
                int[] colors = [127, 128, 83, 84, 18, 19, 39, 40, 211, 211];
                using var source = ImageBuffer.Create(10, 1, 8);
                for (var x = 0; x < 10; x++)
                {
                    source.GetRow8(0).Slice(x * 4, 3).Fill((byte)colors[x]);
                    source.GetRow8(0)[x * 4 + 3] = (byte)alpha;
                }
                var input = source.Pixels.ToArray();
                var weights = new double[width * capacity]; var offsets = new int[width * capacity];
                int[] counts = [2, 2, 2, 2, 2]; double[] normalizers = [1, 1, 1, 1, 1];
                var cached = Enumerable.Repeat(double.NaN, width * 4 + guard * 2).ToArray();
                for (var x = 0; x < width; x++)
                {
                    var weight = x == 0 ? .5 - 1d / 4096 : x == 1 ? .5 + 1d / 4096 : x == 3 ? .75 : .5;
                    weights[x * capacity] = weight; weights[x * capacity + 1] = 1 - weight;
                    offsets[x * capacity] = x * 8; offsets[x * capacity + 1] = x * 8 + 4;
                    if (x == width - 1) continue; // Odd tail must read the original source.
                    var value = weight * colors[x * 2] + (1 - weight) * colors[x * 2 + 1];
                    cached.AsSpan(guard + x * 4, 3).Fill(value * alpha);
                    cached[guard + x * 4 + 3] = alpha;
                }
                var saved = MemoryMarshal.AsBytes(cached.AsSpan()).ToArray();
                // Zero endpoints must never address the poisoned outer rows.
                int[] rows = [int.MaxValue, 0, int.MaxValue]; double[] vertical = [0, 1, 0];
                using var expected = ImageBuffer.Create(width, 1, 8);
                using var actual = ImageBuffer.Create(width, 1, 8);
                using var root = new Lokad.Image.Tooling.TestMemoryBudget(0);
                using var context = new OperationContext(new() { MaxWorkingBytes = 1, MemoryBudget = root }, default);
                for (var x = 0; x < width; x++)
                    Original(source, expected, x, 0, weights, offsets, counts, vertical, -1, capacity, context);
                void Apply(OperationContext operation)
                {
                    WriteByteRow(source, actual, 0, cached.AsSpan(guard, width * 4), rows, vertical,
                        normalizers, 1, weights, offsets, counts, -1, operation);
                    Original(source, actual, width - 1, 0, weights, offsets, counts, vertical, -1, capacity, operation);
                }
                Apply(context);
                var allocated = GC.GetAllocatedBytesForCurrentThread(); Apply(context);
                Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - allocated);
                Assert.Equal(expected.Pixels.ToArray(), actual.Pixels.ToArray());
                int[] golden = [128, 83, 19, 39, 211];
                for (var x = 0; x < width; x++)
                for (var channel = 0; channel < 4; channel++)
                    Assert.Equal(channel == 3 ? alpha : alpha == 0 ? 0 : golden[x], actual.GetRow8(0)[x * 4 + channel]);
                actual.Pixels.Fill(0xA5);
                using var cancellation = new CancellationTokenSource();
                using var canceled = new OperationContext(new() { MaxWorkingBytes = 1, MemoryBudget = root }, cancellation.Token);
                cancellation.Cancel();
                Assert.Throws<OperationCanceledException>(() => Apply(canceled));
                Assert.Equal(-1, actual.Pixels.IndexOfAnyExcept((byte)0xA5));
                Apply(context);
                Assert.Equal(expected.Pixels.ToArray(), actual.Pixels.ToArray());
                Assert.Equal(saved, MemoryMarshal.AsBytes(cached.AsSpan()).ToArray());
                Assert.Equal(input, source.Pixels.ToArray());
                Assert.Equal(0, root.Bytes); Assert.Equal(0, root.GrowthAttempts); Assert.False(root.IsDisposed);
            }
        }
    }
}
