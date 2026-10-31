using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class Vp8lColorCostTests
{
    private uint state = 0x6d2b79f5;
    private uint Next() { state ^= state << 13; state ^= state >> 17; state ^= state << 5; return state; }
    private void Partition(Span<int> bins, int total, int active, int phase)
    {
        bins.Clear(); var remaining = total;
        for (var i = 0; i < active; i++)
        {
            var value = i == active - 1 ? remaining : (int)(Next() % (uint)(Math.Min(remaining, 2L * remaining / (active - i)) + 1));
            bins[(phase + 73 * i) & 255] = value; remaining -= value;
        }
        if (remaining != 0) throw new Exception("Partition incomplete");
    }

    [Fact]
    public void InvariantHistoryTermsPreserveFrozenScalarCostsAtPopulationExtrema()
    {
        var histogram = new int[256]; var history = new int[256]; var products = new long[256]; var activeBins = new int[256];
        int[] populations = [0, 1, 4, 16, 1024, 262144];
        int[] histories = [0, 1, 255, 256, 4095, 4096, 65535, 65536, 16777216, 268435456];
        int[] active = [1, 4, 16, 64, 256];
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        Span<long> result = stackalloc long[1]; var checkedCases = 0;
        for (var test = 0; test < 65536; test++)
        {
            var count = populations[test % populations.Length];
            var priorCount = histories[test % histories.Length];
            Partition(histogram, count, active[(test / 3) % active.Length], test & 255);
            Partition(history, priorCount, active[(test / 7) % active.Length], (test >> 3) & 255);
            var original = Original(histogram, history);
            var baseline = Vp8lColorCost.Prepare(history, count, products);
            var activeCount = 0;
            // A full permutation exercises the construction order independently
            // of the ascending-bin scalar oracle, including dense populations.
            for (var i = 0; i < 256; i++)
            {
                var bin = (i * 73 + test) & 255;
                if (histogram[bin] != 0) activeBins[activeCount++] = bin;
            }
            var actual = Vp8lColorCost.Evaluate(histogram, history, products, baseline, activeBins.AsSpan(0, activeCount));
            Assert.Equal(original, actual);
            hash.AppendData(MemoryMarshal.AsBytes(histogram.AsSpan()));
            hash.AppendData(MemoryMarshal.AsBytes(history.AsSpan()));
            result[0] = actual; hash.AppendData(MemoryMarshal.AsBytes(result)); checkedCases++;
        }
        var digest = Convert.ToHexStringLower(hash.GetHashAndReset());
        Assert.Equal(65536, checkedCases);
        Assert.Equal("d30ff3e3b6e6cff4e4f2fb34575ec869e15abbbfc104b18610729cec0578e54f", digest);
    }
    private static long Original(ReadOnlySpan<int> histogram, ReadOnlySpan<int> history)
        {
            var total = 0; var combinedTotal = 0; long cost = 0;
            for (var i = 0; i < 256; i++)
            {
                var count = histogram[i]; var combined = count + history[i];
                total += count; combinedTotal += combined;
                cost -= Vp8lReferenceCost.SLog(count) + Vp8lReferenceCost.SLog(combined);
            }
            cost += Vp8lReferenceCost.SLog(total) + Vp8lReferenceCost.SLog(combinedTotal);
            var weighted = histogram[0] * 3 * Vp8lReferenceCost.Scale;
            var weight = 240 * Vp8lReferenceCost.Scale;
            for (var magnitude = 1; magnitude < 16; magnitude++)
            {
                weighted += (weight * (histogram[magnitude] + histogram[256 - magnitude]) + 50) / 100;
                weight = (weight * 6 + 5) / 10;
            }
            return cost - (weighted + 5) / 10;
        }

}
