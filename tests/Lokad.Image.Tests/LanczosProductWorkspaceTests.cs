using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class LanczosProductWorkspaceTests
{
    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void SourceProductsStayChargedUntilTheWorkspaceIsDisposed(int depth)
    {
        using var geometry = new OperationContext(ImageLimits.Default, default);
        using var horizontal = LanczosPlan.Create(37, 19, geometry);
        using var vertical = LanczosPlan.Create(23, 11, geometry);
        using var root = new TestMemoryBudget(1 << 20);
        using var owner = new OperationContext(new() { MemoryBudget = root }, default);
        using var workspace = new LanczosWorkspace(horizontal, vertical, depth, false, owner);
        var slots = Math.Min(vertical.MaximumCount, 64);
        var original = 128 + 19 * 4L * slots * 8 + 19 * 4L * 16 + slots * 4L;
        var prepared = LanczosResize.PreparedHorizontalSupported(depth);
        Assert.Equal(original + (prepared ? 32 + 37 * 16L : 0), workspace.Bytes);
        Assert.Equal(workspace.Bytes, root.Bytes);
        Assert.Equal(prepared ? 37 * 4 : 0, workspace.Products.Length);
        workspace.Products.AsSpan().Fill(uint.MaxValue);
        workspace.Dispose();
        Assert.Empty(workspace.Products); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void RefusalAndCancellationReleaseCapacityBeforeSubsequentUse(int depth)
    {
        using var geometry = new OperationContext(ImageLimits.Default, default);
        using var horizontal = LanczosPlan.Create(37, 19, geometry);
        using var vertical = LanczosPlan.Create(23, 11, geometry);
        long required;
        using (var probe = new LanczosWorkspace(horizontal, vertical, depth, false, geometry)) required = probe.Bytes;
        using (var root = new TestMemoryBudget(required - 1))
        {
            using var owner = new OperationContext(new() { MemoryBudget = root }, default);
            var refusal = Assert.Throws<ImageLimitException>(() => new LanczosWorkspace(horizontal, vertical, depth, false, owner));
            Assert.Equal(ImageLimitScope.SharedBudget, refusal.Scope); Assert.Equal(0, root.Bytes);
            using (var recovery = ImageBuffer.Create(1, 1, depth, new() { MemoryBudget = root }))
                Assert.Equal(depth / 8 * 4, root.Bytes);
            Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        }
        using var admitted = new TestMemoryBudget(required);
        using var cancellation = new CancellationTokenSource();
        using var canceled = new OperationContext(new() { MemoryBudget = admitted }, cancellation.Token);
        admitted.Admitted = _ => cancellation.Cancel();
        try
        {
            var error = Assert.Throws<OperationCanceledException>(() => new LanczosWorkspace(horizontal, vertical, depth, false, canceled));
            Assert.Equal(cancellation.Token, error.CancellationToken);
        }
        finally { admitted.Admitted = null; }
        Assert.Equal(0, admitted.Bytes);
        using var next = new OperationContext(new() { MemoryBudget = admitted }, default);
        using (var recovery = new LanczosWorkspace(horizontal, vertical, depth, false, next))
            Assert.Equal(required, admitted.Bytes);
        Assert.Equal(0, admitted.Bytes); Assert.False(admitted.IsDisposed);
    }
}
