using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests
{
    public sealed class CachedWidePublicationTests
    {
        [Fact]
        public void SharedCertificateKeepsRepairNeighborsTailsAndBorrowedOwners()
            => Internal.CachedBicubicResize.VerifySharedWidePublication();
    }
}

namespace Lokad.Image.Internal
{
    internal static partial class CachedBicubicResize
    {
        internal static void VerifySharedWidePublication()
        {
            if (!Avx512F.IsSupported || !Sse41.IsSupported) return;
            Span<byte> witness = stackalloc byte[8];
            foreach (var word in new[] { false, true })
            foreach (var tiny in new[] { false, true })
            {
                const int width = 5, capacity = 32, guard = 4;
                var alpha = tiny ? 1 : word ? 65535 : 255;
                int[] colors = word ? [65023, 65024, 513, 514, 18, 19, 39, 40, 65535, 65535]
                    : [127, 128, 83, 84, 18, 19, 39, 40, 211, 211];
                using var source = ImageBuffer.Create(10, 1, word ? 16 : 8);
                for (var x = 0; x < 10; x++)
                {
                    if (word)
                    {
                        source.GetRow16(0).Slice(x * 4, 3).Fill((ushort)colors[x]);
                        source.GetRow16(0)[x * 4 + 3] = (ushort)alpha;
                    }
                    else
                    {
                        source.GetRow8(0).Slice(x * 4, 3).Fill((byte)colors[x]);
                        source.GetRow8(0)[x * 4 + 3] = (byte)alpha;
                    }
                }
                var input = source.Pixels.ToArray();
                var weights = new double[width * capacity]; var offsets = new int[width * capacity];
                int[] counts = [2, 2, 2, 2, 2]; double[] normalizers = [1, 1, 1, 1, 1];
                var cached = Enumerable.Repeat(double.NaN, width * 4 + guard * 2).ToArray();
                for (var x = 0; x < width; x++)
                {
                    var w = x == 0 ? .5 - 1d / 4096 : x == 1 ? .5 + 1d / 4096 : x == 3 ? .75 : .5;
                    weights[x * capacity] = w; weights[x * capacity + 1] = 1 - w;
                    offsets[x * capacity] = x * 2; offsets[x * capacity + 1] = x * 2 + 1;
                    if (x == width - 1) continue; // The odd tail must use original evaluation.
                    var color = w * colors[x * 2] + (1 - w) * colors[x * 2 + 1];
                    cached.AsSpan(guard + x * 4, 3).Fill(color * alpha);
                    cached[guard + x * 4 + 3] = alpha;
                }
                int[] rows = [int.MaxValue, 0, int.MaxValue];
                double[] vertical = [double.NaN, 1, double.NaN];
                var saved = MemoryMarshal.AsBytes(cached.AsSpan()).ToArray();
                using var expected = ImageBuffer.Create(width, 1, word ? 16 : 8);
                using var actual = ImageBuffer.Create(width, 1, word ? 16 : 8);
                using var root = new TestMemoryBudget(0);
                using var context = new OperationContext(new() { MaxWorkingBytes = 1, MemoryBudget = root }, default);
                void Apply(ImageBuffer result, bool ordinary, OperationContext operation)
                {
                    if (word) WriteWideRow<WordSamples>(source, result, 0, cached.AsSpan(guard, width * 4),
                        rows.AsSpan(1, 1), vertical.AsSpan(1, 1), normalizers, 1, weights, offsets, counts,
                        0, capacity, WideErrorBounds.Create(true, capacity), ordinary, operation);
                    else WriteWideRow<ByteSamples>(source, result, 0, cached.AsSpan(guard, width * 4),
                        rows.AsSpan(1, 1), vertical.AsSpan(1, 1), normalizers, 1, weights, offsets, counts,
                        0, capacity, WideErrorBounds.Create(false, capacity), ordinary, operation);
                }
                if (!word && !tiny)
                {
                    var near = cached[guard] / alpha;
                    Assert.True(Math.Abs(near - Math.Floor(near) - .5) <= .001);
                    var pair = MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(cached.AsSpan(guard, 8)));
                    Assert.True(BicubicMath.TryWriteOrdinaryBytePair(pair, 1, 1, witness));
                }
                Apply(expected, false, context); Apply(actual, true, context);
                var allocated = GC.GetAllocatedBytesForCurrentThread(); Apply(actual, true, context);
                Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - allocated);
                Assert.Equal(expected.Pixels.ToArray(), actual.Pixels.ToArray());
                int[] golden = word ? [65024, 513, 19, 39, 65535] : [128, 83, 19, 39, 211];
                for (var x = 0; x < width; x++) for (var c = 0; c < 4; c++)
                    Assert.Equal(c == 3 ? alpha : golden[x], word ? actual.GetRow16(0)[x * 4 + c] : actual.GetRow8(0)[x * 4 + c]);
                Assert.Equal(saved, MemoryMarshal.AsBytes(cached.AsSpan()).ToArray());
                Assert.Equal(input, source.Pixels.ToArray());
                Assert.Equal(0, root.Bytes); Assert.Equal(0, root.GrowthAttempts); Assert.False(root.IsDisposed);
                actual.Pixels.Fill(0xa5);
                using var cancellation = new CancellationTokenSource();
                using var canceled = new OperationContext(new() { MaxWorkingBytes = 1, MemoryBudget = root }, cancellation.Token);
                cancellation.Cancel();
                Assert.Throws<OperationCanceledException>(() => Apply(actual, true, canceled));
                Assert.Equal(-1, actual.Pixels.IndexOfAnyExcept((byte)0xa5));
                Apply(actual, true, context);
                Assert.Equal(expected.Pixels.ToArray(), actual.Pixels.ToArray());
                Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
            }
        }
    }
}
