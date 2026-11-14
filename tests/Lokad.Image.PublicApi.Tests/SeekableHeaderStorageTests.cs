using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class SeekableHeaderStorageTests
{
    [Theory]
    [InlineData(MetadataProfile.Png8, false, 256)]
    [InlineData(MetadataProfile.Png16, true, 7)]
    [InlineData(MetadataProfile.WebpLosslessAlpha, false, 7)]
    [InlineData(MetadataProfile.WebpLossyAlpha, true, 256)]
    public async Task SeekableContainersKeepSmallStorageAndIndependentMetadata(MetadataProfile profile, bool asynchronous, int chunk)
    {
        var data = Pictured(profile); var saved = data.ToArray(); var expected = ImageCodec.Identify(data);
        using var root = new TestMemoryBudget(512);
        using var input = new ObservedInput(data, root, chunk);
        var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = 512, MaxInputBytes = data.Length };
        var actual = asynchronous ? await ImageCodec.InspectHeadersAsync(input, limits) : ImageCodec.InspectHeaders(input, limits);
        Assert.Equal(expected, actual); Assert.Equal(saved, data);
        Assert.Equal(data.Length, input.Position); Assert.Equal(1, input.EofReads);
        Assert.InRange(input.LargestRequest, 1, 256); Assert.Equal(512, root.Peak);
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.True(input.CanRead);
        input.Dispose(); Array.Clear(data);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task SmallCompleteJpegDoesNotAcquireBulkStorageForEof(bool asynchronous)
    {
        using var root = new TestMemoryBudget(512);
        using var input = new ObservedInput(BulkJpeg(0), root);
        var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = 512 };
        var actual = asynchronous ? await ImageCodec.InspectHeadersAsync(input, limits) : ImageCodec.InspectHeaders(input, limits);
        Assert.Equal(JpegInfo, actual); Assert.Equal(1, input.EofReads);
        Assert.Equal(512, root.Peak); Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Refusals);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task JpegBulkScanReplacesConsumedStorageWithinExistingCeiling(bool asynchronous)
    {
        var data = BulkJpeg(); using var root = new TestMemoryBudget(8448);
        var admissions = new List<long>(); root.Admitted = admissions.Add;
        using var input = new ObservedInput(data, root);
        var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = 8448, MaxInputBytes = data.Length };
        var actual = asynchronous ? await ImageCodec.InspectHeadersAsync(input, limits) : ImageCodec.InspectHeaders(input, limits);
        Assert.Equal(JpegInfo, actual); Assert.Equal(data.Length, input.Position);
        Assert.Equal(data.Length, input.BytesRead); Assert.Equal(1, input.EofReads);
        Assert.Equal(new long[] { 512, 8448 }, admissions);
        Assert.Equal(8448, root.Peak); Assert.Equal(8192, input.LargestRequest);
        Assert.Equal(0, root.Bytes); Assert.True(input.CanRead);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task BulkRefusalIsTerminalAndLeavesBorrowedRootReusable(bool localLimit)
    {
        using var root = new TestMemoryBudget(localLimit ? 8448 : 512);
        using var input = new ObservedInput(BulkJpeg(), root);
        var failure = await Assert.ThrowsAsync<ImageLimitException>(() => ImageCodec.InspectHeadersAsync(input,
            new() { MemoryBudget = root, MaxWorkingBytes = localLimit ? 8447 : 8448 }).AsTask());
        Assert.Equal(ImageLimitKind.WorkingBytes, failure.Limit);
        Assert.Equal(localLimit ? ImageLimitScope.Operation : ImageLimitScope.SharedBudget, failure.Scope);
        Assert.Equal(1, input.Reads); Assert.Equal(256, input.Position);
        Assert.Equal(localLimit ? 0 : 1, root.Refusals); Assert.Equal(0, root.Bytes); Assert.True(input.CanRead);
        using var subsequent = new ObservedInput(Pictured(MetadataProfile.Png8), root);
        Assert.Equal(ImageFormat.Png, ImageCodec.InspectHeaders(subsequent, new() { MemoryBudget = root, MaxWorkingBytes = 512 }).Format);
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void SmallStorageRefusalPrecedesReads()
    {
        using var root = new TestMemoryBudget(511);
        using var input = new ObservedInput(Pictured(MetadataProfile.Png8), root);
        var failure = Assert.Throws<ImageLimitException>(() => ImageCodec.InspectHeaders(input, new() { MemoryBudget = root }));
        Assert.Equal(ImageLimitKind.WorkingBytes, failure.Limit); Assert.Equal(ImageLimitScope.SharedBudget, failure.Scope);
        Assert.Equal(0, input.Reads); Assert.Equal(0, root.Bytes);
    }

    [Fact]
    public void CancellationDuringPromotionReleasesNewReservationBeforeReading()
    {
        using var root = new TestMemoryBudget(8448); using var cancellation = new CancellationTokenSource();
        root.Admitted = bytes => { if (bytes == 8448) cancellation.Cancel(); };
        using var input = new ObservedInput(BulkJpeg(), root);
        var error = Assert.ThrowsAny<OperationCanceledException>(() => ImageCodec.InspectHeaders(input,
            new() { MemoryBudget = root }, cancellation.Token));
        Assert.Equal(cancellation.Token, error.CancellationToken); Assert.Equal(1, input.Reads);
        Assert.Equal(0, root.Bytes); Assert.True(input.CanRead);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task CancelledPendingReadKeepsItsStorageUntilActualCompletion(bool bulk)
    {
        using var root = new TestMemoryBudget(8448); using var cancellation = new CancellationTokenSource();
        using var input = new HeldInput(bulk ? BulkJpeg() : Pictured(MetadataProfile.Png8), root, bulk);
        var pending = ImageCodec.InspectHeadersAsync(input, new() { MemoryBudget = root }, cancellation.Token).AsTask();
        await input.Entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
        cancellation.Cancel();
        Assert.False(pending.IsCompleted); Assert.Equal(bulk ? 8448 : 512, root.Bytes);
        input.Resume.SetResult();
        var error = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
        Assert.Equal(cancellation.Token, error.CancellationToken); Assert.True(input.SawLiveCharge);
        Assert.Equal(0, root.Bytes); Assert.True(input.CanRead);
        using var subsequent = new ObservedInput(Pictured(MetadataProfile.Png8), root);
        Assert.Equal(ImageFormat.Png, ImageCodec.InspectHeaders(subsequent, new() { MemoryBudget = root }).Format);
        Assert.Equal(0, root.Bytes);
    }

    [Fact]
    public async Task ConcurrentInspectionsShareAggregateAdmissionWithoutOwningRoot()
    {
        using var root = new TestMemoryBudget(1024); var data = Pictured(MetadataProfile.Png8);
        using var first = new HeldInput(data, root, false); using var second = new HeldInput(data, root, false);
        var limits = new ImageLimits { MemoryBudget = root };
        var a = ImageCodec.InspectHeadersAsync(first, limits).AsTask();
        var b = ImageCodec.InspectHeadersAsync(second, limits).AsTask();
        await Task.WhenAll(first.Entered.Task, second.Entered.Task).WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Equal(1024, root.Bytes);
        using var third = new ObservedInput(data, root);
        var refused = Assert.Throws<ImageLimitException>(() => ImageCodec.InspectHeaders(third, limits));
        Assert.Equal(ImageLimitScope.SharedBudget, refused.Scope); Assert.Equal(0, third.Reads);
        first.Resume.SetResult(); var one = await a; Assert.Equal(512, root.Bytes);
        second.Resume.SetResult(); var two = await b;
        Assert.Equal(one, two); Assert.NotSame(one, two); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        Assert.Equal(one, ImageCodec.InspectHeaders(third, limits)); Assert.Equal(0, root.Bytes);
    }

    [Fact]
    public async Task BulkReadFailurePreservesOriginalExceptionAndRefundsStorage()
    {
        using var root = new TestMemoryBudget(8448); var expected = new IOException("Caller read failure.");
        using var input = new ObservedInput(BulkJpeg(), root) { Failure = expected };
        var actual = await Assert.ThrowsAsync<IOException>(() => ImageCodec.InspectHeadersAsync(input,
            new() { MemoryBudget = root }).AsTask());
        Assert.Same(expected, actual); Assert.Equal(0, root.Bytes); Assert.True(input.CanRead);
    }

    private static byte[] Pictured(MetadataProfile profile)
        => OriginalExif.Attach(OriginalExif.Create(profile, 33, 17), ImageOrientation.Rotate90, bigEndian: false);

    private static readonly ImageInfo JpegInfo = new(ImageFormat.Jpeg, 17, 11, 8, false, ImageOrientation.Normal)
        { SourceColor = SourceColorModel.Grayscale, SourceAlpha = SourceAlphaRepresentation.None, SourceComponentCount = 1 };

    // Authored structural-only JPEG: no compressed-pixel validity is asserted by InspectHeaders.
    private static byte[] BulkJpeg(int entropy = 9000)
    {
        byte[] prefix = [255,216,255,192,0,11,8,0,11,0,17,1,1,0x11,0,255,218,0,8,1,1,0,0,63,0];
        var bytes = new byte[prefix.Length + entropy + 2]; prefix.CopyTo(bytes, 0);
        if (entropy >= 3002) { bytes[1000] = 255; bytes[1001] = 0; bytes[3000] = 255; bytes[3001] = 208; }
        bytes[^2] = 255; bytes[^1] = 217; return bytes;
    }

    private class ObservedInput(byte[] bytes, TestMemoryBudget root, int chunk = int.MaxValue) : MemoryStream(bytes, writable: false)
    {
        protected TestMemoryBudget Budget { get; } = root;
        public int Reads, EofReads, LargestRequest;
        public long BytesRead;
        public IOException? Failure;
        public override int Read(Span<byte> destination)
        {
            Assert.True(Budget.Bytes >= destination.Length);
            if (Failure is not null && Position >= 256) throw Failure;
            Reads++; LargestRequest = Math.Max(LargestRequest, destination.Length);
            var read = base.Read(destination[..Math.Min(chunk, destination.Length)]);
            BytesRead += read; if (read == 0) EofReads++; return read;
        }
        public override ValueTask<int> ReadAsync(Memory<byte> destination, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(Read(destination.Span));
    }

    private sealed class HeldInput(byte[] bytes, TestMemoryBudget root, bool bulk) : ObservedInput(bytes, root)
    {
        public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Resume { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public bool SawLiveCharge;
        private bool held;
        public override async ValueTask<int> ReadAsync(Memory<byte> destination, CancellationToken cancellationToken = default)
        {
            if (!held && (bulk ? destination.Length > 256 : Position == 0))
            {
                held = true; Entered.SetResult(); await Resume.Task.ConfigureAwait(false);
                SawLiveCharge = Budget.Bytes >= destination.Length;
            }
            return Read(destination.Span);
        }
    }
}
