using System.Runtime;
using System.Runtime.CompilerServices;

namespace Lokad.Image.Tests;

public sealed class AllocationCounterTests
{
    private static byte[]? retained;
    [Fact]
    public void ExactAllocationHostDisablesBackgroundCollection()
    {
        Assert.True(GC.GetConfigurationVariables().TryGetValue("ConcurrentGC", out var concurrent));
        Assert.Equal(false, concurrent);
        Assert.Equal(GCLatencyMode.Batch, GCSettings.LatencyMode);
    }

    [Fact]
    public void CounterStillDetectsAnEscapedAllocationExactly()
    {
        for (var i = 0; i < 256; i++) _ = Allocate();
        var bytes = Allocate();
        Assert.Equal(64, bytes);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static long Allocate()
    {
        var before = GC.GetAllocatedBytesForCurrentThread();
        retained = new byte[40];
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        GC.KeepAlive(retained);
        return allocated;
    }
}
