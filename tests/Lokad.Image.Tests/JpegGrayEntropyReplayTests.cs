using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class JpegGrayEntropyReplayTests
{
    private static readonly JpegEncodeOptions Options = new()
    { Sampling = JpegSampling.Grayscale, Quality = 69, BackgroundRgb = 0x204060 };

    [Fact]
    public void ReplayAndDeclaredTightWorkspaceKeepPacketsLogicalAndBorrowedSource()
    {
        using var image = OriginalPixels.Buffer(17, 11, 16);
        var before = OriginalPixels.Bytes(image);
        const long records = 3 * 2 * 32 * 4;
        const long tablePeak = 16384 + 65536 + 257 * 2048;
        var tightBytes = before.Length + 8192 + tablePeak + records + 128 - 1;
        using var cachedRoot = new TestMemoryBudget(2_000_000);
        using var tightRoot = new TestMemoryBudget(2_000_000);
        using var cached = new OperationContext(new() { MemoryBudget = cachedRoot }, default);
        using var tight = new OperationContext(new() { MemoryBudget = tightRoot, MaxWorkingBytes = tightBytes }, default);
        using var first = new MemoryStream(); using var second = new MemoryStream();
        JpegEncoder.Encode(image, first, Options, cached);
        JpegEncoder.Encode(image, second, Options, tight);
        Assert.Equal(first.ToArray(), second.ToArray());
        Assert.InRange(cachedRoot.Peak - tightRoot.Peak, records + 128, 2 * records + 128);
        Assert.Equal(0, cachedRoot.Bytes); Assert.Equal(0, tightRoot.Bytes);
        Assert.Equal(before, OriginalPixels.Bytes(image));
        Assert.False(cachedRoot.IsDisposed); Assert.False(tightRoot.IsDisposed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CacheAdmissionRefusalOrCancellationRefundsAndAllowsIndependentRecovery(bool cancel)
    {
        using var image = OriginalPixels.Buffer(17, 11, 16);
        var before = OriginalPixels.Bytes(image);
        using var root = new TestMemoryBudget(2_000_000);
        using var cancellation = new CancellationTokenSource();
        // The fixed workspace is first; the entropy owner is the second charge.
        if (cancel) root.Admitted = _ => { if (root.GrowthAttempts == 2) cancellation.Cancel(); };
        else root.RefuseAtGrowth = 2;
        using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
        using var output = new MemoryStream();
        if (cancel) Assert.Throws<OperationCanceledException>(() => JpegEncoder.Encode(image, output, Options, context));
        else
        {
            var error = Assert.Throws<ImageLimitException>(() => JpegEncoder.Encode(image, output, Options, context));
            Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        }
        Assert.Equal(2, root.GrowthAttempts); Assert.Equal(cancel ? 0 : 1, root.Refusals);
        Assert.Equal(0, output.Length); Assert.Equal(0, root.Bytes);
        root.Admitted = null; root.RefuseAtGrowth = 0;
        using var next = new OperationContext(new() { MemoryBudget = root }, default);
        JpegEncoder.Encode(image, output, Options, next);
        Assert.True(output.Length > 0); Assert.Equal(0, root.Bytes);
        Assert.Equal(before, OriginalPixels.Bytes(image)); Assert.False(root.IsDisposed);
    }
}
