using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class ColumnBicubicRepairTests
{
    [Fact]
    public void FrozenNearHalfPixelsKeepTheColumnPathAndItsAdmittedWorkspace()
    {
        if (!Avx512F.IsSupported) return;
        using var source = OriginalPixels.Buffer(513, 259, 8);
        source.GetRow8(0)[3] = source.GetRow8(0)[513 * 4 - 1] = 0;
        var input = OriginalPixels.Bytes(source);
        using var result = ImageBuffer.Create(256, 129, 8);
        using var root = new TestMemoryBudget(4096);
        using var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default);
        // Independent rational analysis identifies alpha(6,4) and blue(142,94)
        // inside the certificate's half margin. Replaying the entire frame would
        // return false here even if the public fallback later produced exact pixels.
        Assert.True(ColumnBicubicResize.TryApply(source, result, 513d / 256, 259d / 129, 10, 10, context));
        Assert.Equal("656bb2248362c88dff8fe510dcc21055155976a3dd2b33aa20c48d2b036f59a8",
            Convert.ToHexStringLower(SHA256.HashData(OriginalPixels.Bytes(result))));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Equal(0, root.Bytes);
        Assert.Equal(4096, root.Peak);
        Assert.Equal(4096, context.RemainingWorkingBytes);
        Assert.False(root.IsDisposed);
    }

    [Fact]
    public void TinyAlphaUnwindsBeforeExactScratchAndTheBorrowedRootIsReusable()
    {
        if (!Avx512F.IsSupported) return;
        using var source = ImageBuffer.Create(9, 7, 8);
        using var result = ImageBuffer.Create(4, 3, 8);
        source.GetRow8(0).Fill(1);
        using var root = new TestMemoryBudget(4096);
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default))
            Assert.False(ColumnBicubicResize.TryApply(source, result, 9d / 4, 7d / 3, 10, 10, context));
        Assert.Equal(0, root.Bytes);
        for (var y = 0; y < source.Height; y++) source.GetRow8(y).Fill(255);
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default))
            Assert.True(ColumnBicubicResize.TryApply(source, result, 9d / 4, 7d / 3, 10, 10, context));
        Assert.All(OriginalPixels.Bytes(result), v => Assert.Equal(255, v));
        Assert.Equal(0, root.Bytes);
        Assert.Equal(4096, root.Peak);
        Assert.False(root.IsDisposed);
    }

    [Fact]
    public void AdmissionRefusalIsTerminalAndCancellationReleasesBeforeReuse()
    {
        if (!Avx512F.IsSupported) return;
        using var source = ImageBuffer.Create(9, 7, 8);
        using var result = ImageBuffer.Create(4, 3, 8);
        for (var y = 0; y < source.Height; y++) source.GetRow8(y).Fill(255);
        using var refused = new TestMemoryBudget(4095);
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = refused }, default))
            Assert.Throws<ImageLimitException>(() => ColumnBicubicResize.TryApply(source, result, 9d / 4, 7d / 3, 10, 10, context));
        Assert.Equal(1, refused.Attempts);
        Assert.Equal(1, refused.Refusals);
        Assert.Equal(0, refused.Bytes);
        Assert.False(refused.IsDisposed);
        using var root = new TestMemoryBudget(4096);
        using var cancellation = new CancellationTokenSource();
        root.Admitted = _ => cancellation.Cancel();
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, cancellation.Token))
            Assert.Throws<OperationCanceledException>(() => ColumnBicubicResize.TryApply(source, result, 9d / 4, 7d / 3, 10, 10, context));
        Assert.Equal(0, root.Bytes);
        root.Admitted = null;
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default))
            Assert.True(ColumnBicubicResize.TryApply(source, result, 9d / 4, 7d / 3, 10, 10, context));
        Assert.Equal(0, root.Bytes);
        Assert.Equal(4096, root.Peak);
        Assert.False(root.IsDisposed);
    }
}
