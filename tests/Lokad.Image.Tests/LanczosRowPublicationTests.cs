using System.Numerics;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class LanczosRowPublicationTests
{
    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void ExactRowsAndSupportedFloatingProducerKeepPrecisionAndOwnership(int depth)
    {
        var roundedWords = 0;
        foreach (var (width, target) in new[] { (31, 9), (1031, 2), (7, 13), (1, 5) })
        {
            using var source = OriginalPixels.Buffer(width, 2, depth);
            var maximum = depth == 8 ? 255 : 65535;
            for (var y = 0; y < 2; y++)
            for (var x = 0; x < width; x++)
            {
                source.SetSample(x, y, 0, x % 3 == 0 ? maximum : maximum - 1);
                source.SetSample(x, y, 1, x % 3 == 1 ? maximum : maximum / 2);
                source.SetSample(x, y, 2, (x + y) % 3 == 2 ? maximum : 1);
                source.SetSample(x, y, 3, ((x + y) % 3) switch { 0 => 0, 1 => 1, _ => maximum });
            }
            var unchanged = OriginalPixels.Bytes(source); var expected = LanczosReference.Axis(width, target);
            using var root = new TestMemoryBudget(1 << 20);
            using var context = new OperationContext(new() { MemoryBudget = root }, default);
            using var horizontal = LanczosPlan.Create(width, target, context);
            using var vertical = LanczosPlan.Create(2, 1, context);
            using var workspace = new LanczosWorkspace(horizontal, vertical, depth, true, context);
            var retained = root.Bytes;
            var floatingSupported = LanczosFmaResize.CanUse(depth, target, 1, horizontal, vertical, context);
            var integers = new long[target * 4 + 2]; Array.Fill(integers, long.MinValue + 73);
            const ulong canary = 0x7ff8123456789abc;
            var floating = new double[target * 4 + 2]; Array.Fill(floating, BitConverter.UInt64BitsToDouble(canary));
            for (var y = 0; y < 2; y++)
            {
                LanczosResize.Horizontal(source, y, horizontal, integers.AsSpan(1, target * 4), workspace.Products, context);
                if (floatingSupported) LanczosFmaResize.Horizontal(source, y, horizontal, floating.AsSpan(1, target * 4), workspace.Products, context);
                for (var x = 0; x < target; x++)
                for (var c = 0; c < 4; c++)
                {
                    BigInteger sum = 0;
                    for (var i = 0; i < width; i++) sum += (BigInteger)source.Sample(i, y, 3) * (c == 3 ? 1 : source.Sample(i, y, c)) * expected[x][i];
                    var exact = checked((long)sum); var at = 1 + x * 4 + c;
                    Assert.Equal(exact, integers[at]);
                    if (floatingSupported)
                    {
                        if (depth == 8 || c == 3)
                            Assert.Equal(BitConverter.DoubleToUInt64Bits((double)exact), BitConverter.DoubleToUInt64Bits(floating[at]));
                        else Assert.True(BigInteger.Abs((BigInteger)floating[at] - sum) <= (horizontal.MaximumCount + 3L) * 512);
                    }
                    if (depth == 16 && (BigInteger)(double)exact != sum) roundedWords++;
                }
            }
            Assert.Equal(long.MinValue + 73, integers[0]); Assert.Equal(long.MinValue + 73, integers[^1]);
            Assert.Equal(canary, BitConverter.DoubleToUInt64Bits(floating[0])); Assert.Equal(canary, BitConverter.DoubleToUInt64Bits(floating[^1]));
            Assert.Equal(unchanged, OriginalPixels.Bytes(source)); Assert.Equal(retained, root.Bytes);
            for (var i = 0; i < 100; i++)
                if (floatingSupported) LanczosFmaResize.Horizontal(source, 0, horizontal, floating.AsSpan(1, target * 4), workspace.Products, context);
                else LanczosResize.Horizontal(source, 0, horizontal, integers.AsSpan(1, target * 4), workspace.Products, context);
            var start = GC.GetAllocatedBytesForCurrentThread();
            for (var i = 0; i < 100; i++)
                if (floatingSupported) LanczosFmaResize.Horizontal(source, 0, horizontal, floating.AsSpan(1, target * 4), workspace.Products, context);
                else LanczosResize.Horizontal(source, 0, horizontal, integers.AsSpan(1, target * 4), workspace.Products, context);
            Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - start); Assert.Equal(retained, root.Bytes);
            using var cancellation = new CancellationTokenSource();
            using var canceled = new OperationContext(new() { MemoryBudget = root }, cancellation.Token); cancellation.Cancel();
            var beforeCancel = floating.ToArray();
            if (floatingSupported) Assert.Throws<OperationCanceledException>(() => LanczosFmaResize.Horizontal(source, 0, horizontal, floating.AsSpan(1, target * 4), workspace.Products, canceled));
            else Assert.Throws<OperationCanceledException>(() => LanczosResize.Horizontal(source, 0, horizontal, integers.AsSpan(1, target * 4), workspace.Products, canceled));
            Assert.Equal(beforeCancel, floating); Assert.Equal(retained, root.Bytes);
            workspace.Dispose(); vertical.Dispose(); horizontal.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        }
        if (depth == 16) Assert.True(roundedWords > 0, "Exercise non-exact Int64-to-double rounding.");
    }
}
