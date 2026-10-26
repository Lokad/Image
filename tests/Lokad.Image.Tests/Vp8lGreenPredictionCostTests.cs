using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8lGreenPredictionCostTests
{
    private uint state = 0x6d2b79f5;
    private uint Next() { state ^= state << 13; state ^= state >> 17; state ^= state << 5; return state; }
    private void Fill(Span<int> bins, int total, int active, int phase)
    {
        bins.Clear(); var remaining = total;
        for (var i = 0; i < active; i++)
        {
            var count = i == active - 1 ? remaining : (int)(Next() % (uint)(Math.Min(remaining, 2L * remaining / (active - i)) + 1));
            bins[(phase + 73 * i) & 255] = count; remaining -= count;
        }
    }
    private void Input(Span<int> histogram, int total, int active, int phase)
    {
        histogram.Clear(); var zero = (int)(Next() % (uint)(total + 1));
        histogram[0] = zero; histogram[1] = total - zero;
        histogram[256] = histogram[768] = total;
        Fill(histogram.Slice(512, 256), total, active, phase);
    }
    [Fact]
    public void FrozenFourPlaneScoresAndRemainExact()
    {
        var current = new int[1024]; var previous = new int[1024];
        int[] populations = [0, 1, 4, 16, 1024, 262144];
        int[] histories = [0, 1, 255, 256, 4095, 4096, 65535, 65536, 16777216, 268435456];
        int[] active = [1, 4, 16, 64, 256];
        using var generic = new OperationContext(ImageLimits.Default, default); using var specialized = new OperationContext(ImageLimits.Default, default);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        Span<long> result = stackalloc long[1];
        for (var test = 0; test < 16384; test++)
        {
            Input(current, populations[test % populations.Length], active[(test / 3) % active.Length], test & 255);
            Input(previous, histories[test % histories.Length], active[(test / 7) % active.Length], (test >> 3) & 255);
            var wanted = Vp8lReferenceCost.Prediction(current, previous, generic);
            var actual = Vp8lReferenceCost.PredictionGreen(current, previous, specialized);
            Assert.Equal(wanted, actual);
            hash.AppendData(MemoryMarshal.AsBytes(current.AsSpan()));
            hash.AppendData(MemoryMarshal.AsBytes(previous.AsSpan()));
            result[0] = actual; hash.AppendData(MemoryMarshal.AsBytes(result));
        }
        var digest = Convert.ToHexStringLower(hash.GetHashAndReset());
        Assert.Equal("844c370b8ffaf7560f80fb26a5debb23d948d1d806bc44cc4f6e96595aeed7c7", digest);
    }

    [Theory]

    [InlineData(true)]
    public void FailureKeepsBorrowedHistogramsAndAllowsIndependentRecovery(bool cancel)
    {
        using var root = new TestMemoryBudget(10000); using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(new() { MemoryBudget = root}, cancellation.Token);
        if (cancel) cancellation.Cancel();
        var current = new int[1024]; var previous = new int[1024];
        Input(current, 1024, 16, 17); Input(previous, 65536, 64, 37);
        var first = current.ToArray(); var second = previous.ToArray();
        if (cancel) Assert.Throws<OperationCanceledException>(() => Vp8lReferenceCost.PredictionGreen(current, previous, context));

Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Refusals);
        Assert.Equal(first, current); Assert.Equal(second, previous); Assert.False(root.IsDisposed);
        using var next = new OperationContext(new() { MemoryBudget = root }, default);
        Assert.Equal(Vp8lReferenceCost.Prediction(current, previous, next), Vp8lReferenceCost.PredictionGreen(current, previous, next));
Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
