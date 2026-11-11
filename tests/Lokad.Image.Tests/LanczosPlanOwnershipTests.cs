using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class LanczosPlanOwnershipTests
{
    [Theory] [InlineData(false)] [InlineData(true)]
    public void PublicPlansKeepExactPixelsAndSurvivorsIndependent(bool tinyAlpha)
    {
        using var source = OriginalPixels.Buffer(37, 23, 8);
        if (tinyAlpha) for (var y = 0; y < 23; y++) for (var x = 0; x < 37; x++) source.SetSample(x, y, 3, x < 19 ? 1 : 0);
        var original = source.Pixels.ToArray();
        using var exact = new OperationContext(ImageLimits.Default, default);
        using var h = LanczosPlan.Create(37, 19, exact); using var v = LanczosPlan.Create(23, 11, exact);
        using var workspace = new LanczosWorkspace(h, v, 8, false, exact);
        using var expected = LanczosResize.Execute(source, 19, 11, h, v, workspace, exact);
        using var root = new TestMemoryBudget(1 << 20);
        using var plan = Lanczos3ResizePlan.Create(37, 23, 19, 11, 8, new() { MemoryBudget = root });
        Assert.Throws<OperationCanceledException>(() => plan.Resize(source, new CancellationToken(true)));
        Assert.Equal(plan.StorageBytes, root.Bytes);
        using var first = plan.Resize(source); using var survivor = plan.Resize(source);
        Assert.Equal(expected.Pixels.ToArray(), first.Pixels.ToArray()); Assert.Equal(expected.Pixels.ToArray(), survivor.Pixels.ToArray());
        Assert.Equal(plan.StorageBytes + survivor.ByteLength * 2L, root.Bytes);
        first.GetRow8(0)[0] ^= 127; plan.Dispose(); first.Dispose();
        Assert.Equal(survivor.ByteLength, root.Bytes); Assert.Equal(original, source.Pixels.ToArray()); source.Dispose();
        Assert.Equal(expected.Pixels.ToArray(), survivor.Pixels.ToArray()); survivor.Dispose(); Assert.Equal(0, root.Bytes);
        Assert.False(root.IsDisposed);
    }
}
