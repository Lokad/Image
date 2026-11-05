using System.Reflection;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8lMatchLengthTests
{
    private delegate int Prefix(ReadOnlySpan<uint> pixels, int first, int second, int already, int limit, OperationContext context);
    // Bind the actual production primitive without widening its visibility.
    private static Prefix Comparison(bool cancelable) => typeof(Vp8lReferenceMatcher)
        .GetMethod("Length", BindingFlags.NonPublic | BindingFlags.Static)!
        .MakeGenericMethod(typeof(Vp8lReferenceMatcher).GetNestedType(cancelable ? "Cancelable" : "Uncancelable", BindingFlags.NonPublic)!)
        .CreateDelegate<Prefix>();

    private static int Expected(uint[] pixels, int first, int second, int already, int limit)
        => pixels[first + already] != pixels[second + already] ? 0
            : Enumerable.Range(0, limit).TakeWhile(i => pixels[first + i] == pixels[second + i]).Count();

    [Theory] [InlineData(false)] [InlineData(true)]
    public void FullWidthPrefixesAndAlreadyPositionPruningKeepExactBoundaries(bool cancelable)
    {
        var Compare = Comparison(cancelable); using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(ImageLimits.Default, cancelable ? cancellation.Token : default);
        foreach (var limit in new[] { 0, 1, 2, 3, 4, 5, 7, 8, 9, 15, 16, 17, 31, 32, 33, 63, 64, 65, 4095 })
        foreach (var mismatch in Enumerable.Range(0, Math.Min(limit, 35) + 1)
            .Concat([limit - 1, limit, limit + 1]).Where(i => i >= 0).Distinct())
        {
            var data = new uint[2 * limit + 12]; uint state = 0x6d2b79f5;
            for (var i = 0; i < data.Length; i++)
            {
                state ^= state << 13; state ^= state >> 17; state ^= state << 5; data[i] = state;
            }
            var first = 1; var second = limit + 5;
            data.AsSpan(first, limit + 1).CopyTo(data.AsSpan(second, limit + 1));
            if (mismatch <= limit) data[second + mismatch] ^= 1u << ((mismatch + limit) & 31);
            var original = data.ToArray();
            foreach (var already in new[] { 0, Math.Min(1, limit), limit }.Distinct())
                Assert.Equal(Expected(data, first, second, already, limit),
                    Compare(data, first, second, already, limit, context));
            Assert.Equal(original, data);
        }
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public void OverlappingAndIdenticalSlicesKeepTheirFirstMismatch(bool cancelable)
    {
        var Compare = Comparison(cancelable); using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(ImageLimits.Default, cancelable ? cancellation.Token : default);
        var data = Enumerable.Range(0, 160).Select(i => i < 63 ? 0x80000000u : uint.MaxValue - (uint)(i & 3)).ToArray();
        var original = data.ToArray();
        foreach (var (first, second) in new[] { (1, 2), (2, 1), (1, 1), (3, 7) })
        foreach (var limit in new[] { 0, 1, 3, 4, 5, 7, 8, 9, 63, 64, 65, 127 })
        foreach (var already in new[] { 0, Math.Min(4, limit), limit }.Distinct())
            Assert.Equal(Expected(data, first, second, already, limit),
                Compare(data, first, second, already, limit, context));
        Assert.Equal(original, data);
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public void TightAndUnalignedSpanTailsKeepTheExactSampleLimit(bool cancelable)
    {
        var compare = Comparison(cancelable); using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(ImageLimits.Default, cancelable ? cancellation.Token : default);
        foreach (var length in new[] { 0, 1, 2, 3, 4, 7 })
        foreach (var skip in new[] { 0, 1 })
        {
            var data = Enumerable.Repeat(0x89abcdefu, skip + 2 + Math.Max(1, length)).ToArray();
            var samples = data.AsSpan(skip); // second slice ends exactly at the array end.
            Assert.Equal(length, compare(samples, 0, 2, 0, length, context));
            if (length != 0)
            {
                data[^1] ^= 0x80000000;
                Assert.Equal(length - 1, compare(samples, 0, 2, 0, length, context));
            }
        }
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public void ComparisonAllocatesNothingAndCancellationPrecedesInputReads(bool cancelable)
    {
        var Compare = Comparison(cancelable);
        var data = Enumerable.Repeat(uint.MaxValue, 4100).ToArray();
        using var root = new TestMemoryBudget(0);
        using var active = new CancellationTokenSource();
        using var context = new OperationContext(new() { MemoryBudget = root }, cancelable ? active.Token : default);
        var checksum = 0;
        for (var i = 0; i < 512; i++) checksum ^= Compare(data, 1, 2, 0, 4095, context);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 512; i++) checksum ^= Compare(data, 1, 2, 0, 4095, context);
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before); Assert.Equal(0, checksum);
        Assert.Equal(0, root.Bytes); Assert.Equal(0, root.GrowthAttempts); Assert.False(root.IsDisposed);
        using var cancellation = new CancellationTokenSource();
        using var canceled = new OperationContext(new() { MemoryBudget = root }, cancellation.Token); cancellation.Cancel();
        var liveComparison = Comparison(true);
        var error = Assert.Throws<OperationCanceledException>(() => liveComparison([], 0, 0, 0, 4095, canceled));
        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.Equal(4095, Compare(data, 1, 2, 0, 4095, context));
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
