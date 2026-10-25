using Lokad.Image.Internal;
using Lokad.Image.Tooling;
using System.Security.Cryptography;

namespace Lokad.Image.Tests;

public sealed class JpegEntropyBufferTests
{
    [Fact]
    public void FrozenGrayPacketKeepsBoundedCallerStreamCapacityAfterItsSmallHeader()
    {
        using var source = OriginalPixels.Buffer(513, 259, 8);
        source.GetRow8(0)[3] = source.GetRow8(0)[513 * 4 - 1] = 0;
        using var output = new MemoryStream();
        ImageCodec.EncodeJpeg(source, output, new() { Quality = 69, Sampling = JpegSampling.Grayscale, BackgroundRgb = 0x204060 });
        Assert.Equal(58910, output.Length);
        Assert.Equal("444e39e6126dd3077d22c0b4f60e38d8a22611fa5721b226c5168a63b7e94423",
            Convert.ToHexStringLower(SHA256.HashData(output.GetBuffer().AsSpan(0, (int)output.Length))));
        // A first512-byte write grew256->740 and ultimately94,720 bytes.
        // This existing control must retain at most64KiB of caller capacity.
        Assert.InRange(output.Capacity, 58910, 65536);
    }

    [Fact]
    public void LazyOwnersShareOneRootAndReleaseCapacityForAnIndependentLaterOperation()
    {
        using var root = new TestMemoryBudget(JpegEntropyWriter.StorageBytes);
        using var firstContext = new OperationContext(new() { MemoryBudget = root }, default);
        using var secondContext = new OperationContext(new() { MemoryBudget = root }, default);
        using var firstOutput = new MemoryStream(); using var secondOutput = new MemoryStream();
        using var first = new JpegEntropyWriter(firstOutput, firstContext);
        using var second = new JpegEntropyWriter(secondOutput, secondContext);
        first.Write(0, 0); Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Attempts);
        Assert.Throws<ArgumentOutOfRangeException>(() => first.Write(1, 0)); Assert.Equal(0, root.Attempts);
        first.Write(83, 8); Assert.Equal(JpegEntropyWriter.StorageBytes, root.Bytes);
        var growth = root.GrowthAttempts;
        var error = Assert.Throws<ImageLimitException>(() => second.Write(37, 8));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        Assert.Equal(growth + 1, root.GrowthAttempts); Assert.Equal(1, root.Refusals);
        second.Dispose(); Assert.Equal(JpegEntropyWriter.StorageBytes, root.Bytes);
        Assert.Equal(0, firstOutput.Length); Assert.Equal(0, secondOutput.Length);
        first.Align(); first.Dispose(); Assert.Equal(0, root.Bytes); Assert.Equal(new byte[] { 83 }, firstOutput.ToArray());
        using var nextContext = new OperationContext(new() { MemoryBudget = root }, default);
        using var next = new JpegEntropyWriter(secondOutput, nextContext);
        next.Write(37, 8); next.Align(); next.Dispose(); Assert.Equal(0, root.Bytes);
        Assert.Equal(new byte[] { 37 }, secondOutput.ToArray()); Assert.Equal(new byte[] { 83 }, firstOutput.ToArray());
        Assert.Equal(JpegEntropyWriter.StorageBytes, root.Peak); Assert.False(root.IsDisposed);
        Assert.True(firstOutput.CanWrite); Assert.True(secondOutput.CanWrite);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RefusalOrCancellationAfterAdmissionCleansUpWithoutRetryAndAllowsRecovery(bool cancel)
    {
        using var root = new TestMemoryBudget(JpegEntropyWriter.StorageBytes);
        using var cancellation = new CancellationTokenSource();
        if (cancel) root.Admitted = _ => cancellation.Cancel(); else root.RefuseAtGrowth = 1;
        using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
        using var output = new MemoryStream(); using var writer = new JpegEntropyWriter(output, context);
        if (cancel) Assert.Throws<OperationCanceledException>(() => writer.Write(255, 8));
        else
        {
            var error = Assert.Throws<ImageLimitException>(() => writer.Write(255, 8));
            Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        }
        Assert.Equal(1, context.ConsumedWork); Assert.Equal(1, root.GrowthAttempts);
        Assert.Equal(cancel ? 0 : 1, root.Refusals); writer.Dispose();
        Assert.Equal(0, root.Bytes); Assert.Equal(0, output.Length); Assert.False(root.IsDisposed);
        root.Admitted = null; root.RefuseAtGrowth = 0;
        using var nextContext = new OperationContext(new() { MemoryBudget = root }, default);
        using var next = new JpegEntropyWriter(output, nextContext);
        next.Write(7, 8); next.Align(); next.Dispose(); Assert.Equal(0, root.Bytes);
        Assert.Equal(new byte[] { 7 }, output.ToArray()); Assert.True(output.CanWrite);
    }

    private sealed class ObservedOutput(Action onWrite) : MemoryStream
    {
        internal int Writes;
        public override void Write(ReadOnlySpan<byte> bytes)
        {
            Writes++; onWrite(); base.Write(bytes);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OutputUserKeepsItsChargeAndFailureDisposalDoesNotPublishPendingBytes(bool cancel)
    {
        using var root = new TestMemoryBudget(JpegEntropyWriter.StorageBytes);
        using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
        using var output = new ObservedOutput(() =>
        {
            Assert.Equal(JpegEntropyWriter.StorageBytes, root.Bytes);
            if (cancel) cancellation.Cancel(); else throw new IOException("Authored output failure");
        });
        using var writer = new JpegEntropyWriter(output, context);
        void Encode() { for (var i = 0; i < 1024; i++) writer.Write(91, 8); }
        if (cancel) Assert.Throws<OperationCanceledException>(Encode); else Assert.Throws<IOException>(Encode);
        var writes = output.Writes; var length = output.Length;
        writer.Dispose(); writer.Dispose(); Assert.Equal(0, root.Bytes);
        Assert.Equal(writes, output.Writes); Assert.Equal(length, output.Length);
        Assert.True(output.CanWrite); Assert.False(root.IsDisposed); Assert.Equal(0, root.Refusals);
    }

    [Fact]
    public void WorkFailureRetainsItsOriginalChargeAndDropsPendingBytesOnDisposal()
    {
        using var root = new TestMemoryBudget(JpegEntropyWriter.StorageBytes);
        using var context = new OperationContext(new() { MemoryBudget = root, MaxWork = 2 }, default);
        using var output = new MemoryStream(); using var writer = new JpegEntropyWriter(output, context);
        writer.Write(91, 8); writer.Write(92, 8);
        var error = Assert.Throws<ImageLimitException>(() => writer.Write(93, 8));
        Assert.Equal((ImageLimitKind.Work, ImageLimitScope.Operation), (error.Limit, error.Scope));
        Assert.Equal(2, context.ConsumedWork); Assert.Equal(JpegEntropyWriter.StorageBytes, root.Bytes);
        writer.Dispose(); Assert.Equal(0, root.Bytes); Assert.Equal(0, output.Length); Assert.True(output.CanWrite);
        Assert.Throws<ObjectDisposedException>(() => writer.Write(0, 0));
    }
}
