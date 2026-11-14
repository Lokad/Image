using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class StreamingColumnPairTests
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double Product(double sample, double weight) => sample * weight;

    [Fact]
    public void SharedDecodingPreservesBothOrderedSumsAndBorrowedStorage()
    {
        if (!Avx512F.IsSupported || !Avx2.IsSupported || !Sse41.IsSupported) return;
        foreach (var (width, first, count0, delta, count1) in new[]
        {
            (19, -9, 25, 7, 27), (257, 0, 128, 31, 129),
            (257, -33, 129, 32, 128), (257, 141, 129, 33, 128),
            (17, 0, 0, 0, 1), (17, 0, 1, 2, 0), (19, 5, 12, 3, 4)
        })
        {
            const int guard = 32;
            var pixels = Enumerable.Repeat((byte)0xa5, guard * 2 + width * 8).ToArray();
            for (var x = 0; x < width; x++) for (var c = 0; c < 4; c++)
            {
                var value = c == 3 ? (x % 4) switch { 0 => 0, 1 => 1, 2 => 65535, _ => 32768 }
                    : c == 0 ? 65535 : (x * 7919 + c * 2309) & 65535;
                BinaryPrimitives.WriteUInt16LittleEndian(pixels.AsSpan(guard + x * 8 + c * 2), (ushort)value);
            }
            var original = pixels.ToArray();
            var weights0 = Enumerable.Repeat(double.NaN, count0 + 2).ToArray();
            var weights1 = Enumerable.Repeat(double.NaN, count1 + 2).ToArray();
            var offsets0 = Enumerable.Repeat(ushort.MaxValue, count0 + 2).ToArray();
            var offsets1 = Enumerable.Repeat(ushort.MaxValue, count1 + 2).ToArray();
            var sourceFirst = Math.Clamp(first, 0, width - 1); var sourceCount = 0;
            Prepare(weights0, offsets0, first); Prepare(weights1, offsets1, first + delta);
            void Prepare(double[] weights, ushort[] offsets, int start)
            {
                for (var t = 0; t < weights.Length - 2; t++)
                {
                    weights[t + 1] = (t % 6) switch
                    {
                        0 => .1, 1 => -1d / 3, 2 => .75, 3 => -1d / 7,
                        4 => 0d, _ => BitConverter.UInt64BitsToDouble(1UL << 63)
                    };
                    var offset = Math.Clamp(start + t, 0, width - 1) - sourceFirst;
                    offsets[t + 1] = (ushort)offset; sourceCount = Math.Max(sourceCount, offset + 1);
                }
            }
            (Vector256<double> First, Vector256<double> Second) Apply()
            {
                StreamingWordBicubicResize.FilterColumnPair(
                    pixels.AsSpan(guard + sourceFirst * 8, sourceCount * 8),
                    weights0.AsSpan(1, count0), offsets0.AsSpan(1, count0), delta,
                    weights1.AsSpan(1, count1), offsets1.AsSpan(1, count1), out var a, out var b);
                return (a, b);
            }
            Apply(); var allocated = GC.GetAllocatedBytesForCurrentThread(); var actual = Apply();
            Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - allocated);
            Check(weights0, first, actual.First); Check(weights1, first + delta, actual.Second);
            void Check(double[] weights, int start, Vector256<double> value)
            {
                for (var c = 0; c < 4; c++)
                {
                    var expected = 0d;
                    for (var t = 0; t < weights.Length - 2; t++)
                    {
                        var at = guard + Math.Clamp(start + t, 0, width - 1) * 8;
                        var alpha = BinaryPrimitives.ReadUInt16LittleEndian(pixels.AsSpan(at + 6));
                        var sample = c == 3 ? alpha
                            : (double)((uint)BinaryPrimitives.ReadUInt16LittleEndian(pixels.AsSpan(at + c * 2)) * alpha);
                        expected += Product(sample, weights[t + 1]);
                    }
                    Assert.Equal(BitConverter.DoubleToInt64Bits(expected), BitConverter.DoubleToInt64Bits(value.GetElement(c)));
                }
            }
            Assert.Equal(original, pixels);
            foreach (var offsets in new[] { offsets0, offsets1 })
            {
                Assert.Equal(ushort.MaxValue, offsets[0]); Assert.Equal(ushort.MaxValue, offsets[^1]);
            }
            foreach (var weights in new[] { weights0, weights1 })
            {
                Assert.True(double.IsNaN(weights[0])); Assert.True(double.IsNaN(weights[^1]));
            }
        }
        using var source = ImageBuffer.Create(513, 259, 16);
        using var result = ImageBuffer.Create(16, 8, 16);
        using var root = new TestMemoryBudget(4095);
        using var context = new OperationContext(new()
        {
            MemoryBudget = root, MaxWorkingBytes = source.ByteLength + result.ByteLength + 4096
        }, CancellationToken.None);
        context.ReserveBorrowed(source.ByteLength + result.ByteLength);
        var scaleX = (double)source.Width / result.Width; var scaleY = (double)source.Height / result.Height;
        var failure = Assert.Throws<ImageLimitException>(() => StreamingBicubicResize.TryApply(
            source, result, scaleX, scaleY, (long)Math.Ceiling(4 * scaleX) + 1, (long)Math.Ceiling(4 * scaleY) + 1, context));
        Assert.Equal(ImageLimitKind.WorkingBytes, failure.Limit); Assert.Equal(ImageLimitScope.SharedBudget, failure.Scope);
        Assert.Equal(1, root.Refusals); Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Peak);
        Assert.False(root.IsDisposed); Assert.True(result.Pixels.IndexOfAnyExcept((byte)0) < 0);
    }
}
