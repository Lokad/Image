using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class WebpEntropyGroupsTests
{
    [Fact]
    public void SeparateSymbolDistributionsShareOnlyTheirMatchingCodebook()
    {
        using var context = new OperationContext(ImageLimits.Default, default);
        var histogram = new WebpLosslessEncoder.TileHistograms(18, context);
        int[][] frequencies = [new int[1304], new int[256], new int[256], new int[256], new int[40]];
        for (var tile = 0; tile < 18; tile++)
        for (var sample = 0; sample < 64; sample++)
        for (var channel = 0; channel < 4; channel++)
        {
            var symbol = channel == 3 ? 0 : tile % 9 + channel * 32;
            histogram.Add(tile, channel, symbol); frequencies[channel][symbol]++;
        }
        var tables = frequencies.Select(f => new WebpPrefixCode(f, context)).ToArray();
        var first = new byte[18]; var second = new byte[18]; var ranking = new long[18];
        WebpLosslessEncoder.LearnEntropyMap(histogram, tables, first, ranking, context);
        WebpLosslessEncoder.LearnEntropyMap(histogram, tables, second, ranking, context);
        Assert.Equal(first, second); Assert.InRange(first.Distinct().Count(), 2, 9);
        for (var tile = 0; tile < 9; tile++) Assert.Equal(first[tile], first[tile + 9]);
        Assert.All(histogram.Used, used => Assert.Equal(4, used));
        Assert.Equal(64, histogram.Frequencies[0]);
    }

    [Fact]
    public void HistogramStorageRefusalAndCancellationReleaseTheBorrowedRoot()
    {
        var bytes = WebpLosslessEncoder.TileHistograms.StorageBytes(18);
        using var root = new TestMemoryBudget(bytes); using var host = root.CreateScope(); Assert.True(host.TryReserve(1, out var held));
        using (var context = new OperationContext(new() { MemoryBudget = root }, default))
        {
            var attempts = root.Attempts;
            var error = Assert.Throws<ImageLimitException>(() => new WebpLosslessEncoder.TileHistograms(18, context));
            Assert.Equal(ImageLimitScope.SharedBudget, error.Scope); Assert.Equal(attempts + 1, root.Attempts); Assert.Equal(1, root.Bytes);
        }
        held!.Dispose(); using var token = new CancellationTokenSource(); root.Admitted = _ => token.Cancel();
        using (var context = new OperationContext(new() { MemoryBudget = root }, token.Token))
            Assert.Throws<OperationCanceledException>(() => new WebpLosslessEncoder.TileHistograms(18, context));
        Assert.Equal(0, root.Bytes); root.Admitted = null;
        using (var context = new OperationContext(new() { MemoryBudget = root }, default))
        {
            _ = new WebpLosslessEncoder.TileHistograms(18, context);
            Assert.Equal(bytes, root.Bytes); Assert.Equal(bytes, root.Peak); Assert.False(root.IsDisposed);
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
