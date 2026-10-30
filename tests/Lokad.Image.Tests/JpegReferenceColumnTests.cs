using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class JpegReferenceColumnTests
{
    [Fact]
    public void StorageEstimateCoversTheActualRetainedArrayLayout()
    {
        var columns = new JpegReferenceColumn[33];
        Assert.Equal(MemoryMarshal.AsBytes(columns.AsSpan()).Length, JpegReferenceColumn.ByteSize * columns.Length);
        Assert.InRange(JpegReferenceColumn.ByteSize, 392, 512);
    }

    [Theory]
    [InlineData(8, JpegSampling.YCbCr420)]
    [InlineData(16, JpegSampling.YCbCr422)]
    public void CachedColumnsAndDeclaredTightStackPathKeepExactPacketsAndReleaseCapacity(int depth, JpegSampling sampling)
    {
        using var image = OriginalPixels.Buffer(33, 33, depth);
        var before = OriginalPixels.Bytes(image);
        using var cachedRoot = new TestMemoryBudget(2_000_000);
        using var tightRoot = new TestMemoryBudget(2_000_000);
        using var cached = new OperationContext(new() { MemoryBudget = cachedRoot }, default);
        // Fixed tables avoid a second transform pass. The remaining 640 bytes
        // cover the actual entropy buffer but cannot hold three column values.
        using var tight = new OperationContext(new()
        {
            MemoryBudget = tightRoot,
            MaxWorkingBytes = before.Length + 8192 + 16384 + JpegEntropyWriter.StorageBytes
        }, default);
        var options = new JpegEncodeOptions { Sampling = sampling, Quality = 85, BackgroundRgb = 0x204060, RestartInterval = 3 };
        using var first = new MemoryStream(); using var second = new MemoryStream();
        JpegEncoder.Encode(image, first, options, cached);
        JpegEncoder.Encode(image, second, options, tight);
        Assert.Equal(first.ToArray(), second.ToArray());
        var capacity = Avx512BW.IsSupported && BitConverter.IsLittleEndian ? 3L * JpegReferenceColumn.ByteSize + 32 : 0;
        Assert.Equal(capacity, cachedRoot.Peak - tightRoot.Peak);
        Assert.Equal(0, cachedRoot.Bytes); Assert.Equal(0, tightRoot.Bytes);
        Assert.Equal(before, OriginalPixels.Bytes(image));
        Assert.True(first.CanWrite && second.CanWrite);
        Assert.False(cachedRoot.IsDisposed); Assert.False(tightRoot.IsDisposed);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void ColumnAdmissionRefusalAndCancellationReleaseEarlierOwnersAndAllowRecovery(bool cancel)
    {
        if (!Avx512BW.IsSupported || !BitConverter.IsLittleEndian) return;
        using var image = OriginalPixels.Buffer(33, 33, 8);
        var before = OriginalPixels.Bytes(image);
        using var root = new TestMemoryBudget(2_000_000);
        using var cancellation = new CancellationTokenSource();
        // Fixed MCU storage and adaptive entropy records precede column storage.
        if (cancel) root.Admitted = _ => { if (root.GrowthAttempts == 3) cancellation.Cancel(); };
        else root.RefuseAtGrowth = 3;
        using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
        using var output = new MemoryStream();
        var options = new JpegEncodeOptions { Sampling = JpegSampling.YCbCr420, Quality = 85, BackgroundRgb = 0x204060 };
        if (cancel)
            Assert.Equal(cancellation.Token, Assert.Throws<OperationCanceledException>(
                () => JpegEncoder.Encode(image, output, options, context)).CancellationToken);
        else
        {
            var error = Assert.Throws<ImageLimitException>(() => JpegEncoder.Encode(image, output, options, context));
            Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        }
        Assert.Equal(3, root.GrowthAttempts); Assert.Equal(cancel ? 0 : 1, root.Refusals);
        Assert.Equal(0, output.Length); Assert.Equal(0, root.Bytes);
        root.Admitted = null; root.RefuseAtGrowth = 0;
        using var next = new OperationContext(new() { MemoryBudget = root }, default);
        JpegEncoder.Encode(image, output, options, next);
        Assert.True(output.Length > 0); Assert.Equal(0, root.Bytes);
        Assert.Equal(before, OriginalPixels.Bytes(image)); Assert.False(root.IsDisposed);
    }
}
