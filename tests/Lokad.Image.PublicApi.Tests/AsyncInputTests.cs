using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class AsyncInputTests
{
    [Theory]
    [InlineData(MetadataProfile.Png8)] [InlineData(MetadataProfile.Png16)]
    [InlineData(MetadataProfile.JpegBaseline420)] [InlineData(MetadataProfile.JpegProgressive420)]
    [InlineData(MetadataProfile.WebpLosslessAlpha)] [InlineData(MetadataProfile.WebpLossyAlpha)]
    public async Task AsyncOnlyShortReadsPreservePrecisionOrientationAndSource(MetadataProfile profile)
    {
        var fixture = OriginalExif.Create(profile, 17, 11);
        var encoded = OriginalExif.Attach(fixture, ImageOrientation.Rotate90, bigEndian: true);
        var saved = encoded.ToArray(); using var token = new CancellationTokenSource();
        using var input = new AsyncOnlyInput(encoded);
        var info = await ImageCodec.IdentifyAsync(input, cancellationToken: token.Token);
        Assert.Equal(ImageCodec.Identify(encoded), info); Assert.Equal(ImageOrientation.Rotate90, info.Orientation);
        Assert.Equal(1, input.EofReads); Assert.False(input.WasDisposed); Assert.Equal(token.Token, input.LastToken);
        using var pixelsInput = new AsyncOnlyInput(encoded, 31);
        var options = new DecodeOptions { ApplyOrientation = true };
        using var expected = ImageCodec.Decode(encoded, options);
        using var result = await ImageCodec.DecodeAsync(pixelsInput, options, token.Token);
        Assert.Equal((11, 17, expected.BitDepth, ImageOrientation.Normal), (result.Width, result.Height, result.BitDepth, result.Orientation));
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(result));
        Assert.Equal(saved, encoded); Assert.Equal(1, pixelsInput.EofReads); Assert.False(pixelsInput.WasDisposed);
        Assert.Equal(token.Token, pixelsInput.LastToken); Assert.InRange(pixelsInput.LargestRequest, 1, 8192);
    }

    [Theory]
    [InlineData(MetadataProfile.Png8)] [InlineData(MetadataProfile.Png16)]
    [InlineData(MetadataProfile.JpegBaseline420)] [InlineData(MetadataProfile.JpegProgressive420)]
    [InlineData(MetadataProfile.WebpLosslessAlpha)] [InlineData(MetadataProfile.WebpLossyAlpha)]
    public async Task IdentifyRetainsCompleteEofValidation(MetadataProfile profile)
    {
        var encoded = OriginalExif.Create(profile, 17, 11).Encoded;
        var trailing = new byte[encoded.Length + 1]; encoded.CopyTo(trailing, 0); trailing[^1] = 0xA7;
        Assert.Throws<ImageDataException>(() => ImageCodec.Identify(trailing));
        using var input = new AsyncOnlyInput(trailing, 53);
        await Assert.ThrowsAsync<ImageDataException>(() => ImageCodec.IdentifyAsync(input).AsTask());
        Assert.Equal(1, input.EofReads); Assert.False(input.WasDisposed);
        using var truncated = new AsyncOnlyInput(encoded[..^1], 53);
        await Assert.ThrowsAsync<ImageDataException>(() => ImageCodec.IdentifyAsync(truncated).AsTask());
        Assert.Equal(1, truncated.EofReads); Assert.False(truncated.WasDisposed);
    }

    [Fact]
    public async Task ExactAcquisitionAllowanceNeedsNoFinalFullInputCopy()
    {
        var encoded = OriginalExif.Create(MetadataProfile.Png8, 1, 1).Encoded;
        var limits = new ImageLimits { MaxInputBytes = encoded.Length, MaxWorkingBytes = 8192L + encoded.Length };
        using var input = new AsyncOnlyInput(encoded, encoded.Length);
        Assert.Equal(ImageCodec.Identify(encoded), await ImageCodec.IdentifyAsync(input, limits));
        using var rejected = new AsyncOnlyInput(encoded, encoded.Length);
        var failure = await Assert.ThrowsAsync<ImageLimitException>(() => ImageCodec.IdentifyAsync(rejected,
            limits with { MaxWorkingBytes = limits.MaxWorkingBytes - 1 }).AsTask());
        Assert.Equal(ImageLimitKind.WorkingBytes, failure.Limit); Assert.False(rejected.WasDisposed);
    }

    [Fact]
    public async Task ScratchAndSimultaneousGrowthAreAdmittedBeforeAllocation()
    {
        var encoded = OriginalExif.Create(MetadataProfile.Png8, 33, 17).Encoded;
        Assert.True(encoded.Length > 512);
        using var noScratch = new AsyncOnlyInput(encoded);
        var scratch = await Assert.ThrowsAsync<ImageLimitException>(() => ImageCodec.IdentifyAsync(noScratch,
            new() { MaxWorkingBytes = 8191 }).AsTask());
        Assert.Equal(ImageLimitKind.WorkingBytes, scratch.Limit); Assert.Equal(0, noScratch.ReadCalls);
        using var growth = new AsyncOnlyInput(encoded, 256);
        var failure = await Assert.ThrowsAsync<ImageLimitException>(() => ImageCodec.DecodeAsync(growth,
            new() { Limits = new() { MaxWorkingBytes = 8192 + 256 + 512 - 1 } }).AsTask());
        Assert.Equal(ImageLimitKind.WorkingBytes, failure.Limit); Assert.Equal(512, growth.Consumed); Assert.False(growth.WasDisposed);
    }

    [Fact]
    public async Task InputAndWorkFailuresCommitOnlyConsumedWorkAndRecover()
    {
        var encoded = OriginalExif.Create(MetadataProfile.Png8, 17, 11).Encoded;
        var limits = new ImageLimits { MaxWorkingBytes = 1_000_000, MaxWork = 1_000_000, MaxInputBytes = 23 };
        var budget = new ImageBudget(limits.MaxWorkingBytes, 3_000_000);
        using var input = new AsyncOnlyInput(encoded, 17);
        var failure = await Assert.ThrowsAsync<ImageLimitException>(() => budget.DecodeAsync(input, new() { Limits = limits }).AsTask());
        Assert.Equal(ImageLimitKind.InputBytes, failure.Limit); Assert.Equal(17, budget.ConsumedWork);
        Assert.Equal(budget.MaxWorkingBytes, budget.RemainingWorkingBytes); Assert.False(input.WasDisposed);
        var used = budget.ConsumedWork;
        using var workInput = new AsyncOnlyInput(encoded, 17);
        var work = await Assert.ThrowsAsync<ImageLimitException>(() => budget.IdentifyAsync(workInput,
            limits with { MaxInputBytes = encoded.Length, MaxWork = 64 }).AsTask());
        Assert.Equal(ImageLimitKind.Work, work.Limit); Assert.Equal(51, budget.ConsumedWork - used);
        Assert.Equal(68, workInput.Consumed); Assert.Equal(budget.MaxWorkingBytes, budget.RemainingWorkingBytes);
        using var recovery = new AsyncOnlyInput(encoded, 113);
        using var result = await budget.DecodeAsync(recovery, new() { Limits = limits with { MaxInputBytes = encoded.Length } });
        using var expected = ImageCodec.Decode(encoded);
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(result.Image));
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task CallerIoFailuresPropagateAndRefundReservations(bool decode)
    {
        var encoded = OriginalExif.Create(MetadataProfile.Png8, 17, 11).Encoded;
        var limits = new ImageLimits { MaxWorkingBytes = 1_000_000, MaxWork = 1_000_000 };
        var budget = new ImageBudget(limits.MaxWorkingBytes, 2_000_000);
        var expected = new IOException("Original caller failure.");
        using var input = new AsyncOnlyInput(encoded, 17) { Failure = expected, FailAfter = 17 };
        var actual = await Assert.ThrowsAsync<IOException>(async () =>
        {
            if (decode) { using var result = await budget.DecodeAsync(input, new() { Limits = limits }); }
            else await budget.IdentifyAsync(input, limits);
        });
        Assert.Same(expected, actual); Assert.Equal(17, budget.ConsumedWork);
        Assert.Equal(budget.MaxWorkingBytes, budget.RemainingWorkingBytes); Assert.False(input.WasDisposed);
    }

    [Fact]
    public async Task SharedReservationSurvivesAwaitAndRetainedImageOwnsOnlyItsPixels()
    {
        var encoded = OriginalExif.Create(MetadataProfile.Png16, 17, 11).Encoded;
        var limits = new ImageLimits { MaxWorkingBytes = 1_000_000, MaxWork = 1_000_000 };
        var budget = new ImageBudget(limits.MaxWorkingBytes + 77, 3_000_000);
        using var caller = budget.Reserve(77);
        using var input = new AsyncOnlyInput(encoded) { Pause = true };
        var pending = budget.DecodeAsync(input, new() { Limits = limits }).AsTask();
        try
        {
            await input.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.False(pending.IsCompleted); Assert.Equal(0, budget.RemainingWorkingBytes);
            using var competing = new AsyncOnlyInput(encoded);
            var failure = await Assert.ThrowsAsync<ImageLimitException>(() => budget.IdentifyAsync(competing, limits).AsTask());
            Assert.Equal(ImageLimitScope.SharedBudget, failure.Scope); Assert.Equal(ImageLimitKind.WorkingBytes, failure.Limit);
            Assert.Equal(0, competing.ReadCalls);
            input.Resume.TrySetResult();
            using var result = await pending;
            var expected = OriginalPixels.Bytes(result.Image);
            Assert.Equal(limits.MaxWorkingBytes - expected.Length, budget.RemainingWorkingBytes);
            Assert.True(budget.ConsumedWork > encoded.Length); Assert.False(input.WasDisposed);
            using var anotherInput = new AsyncOnlyInput(encoded);
            using var another = await ImageCodec.DecodeAsync(anotherInput);
            Assert.Equal(expected, OriginalPixels.Bytes(another));
            another.Dispose(); Assert.Equal(expected, OriginalPixels.Bytes(result.Image));
            result.Dispose(); Assert.Equal(limits.MaxWorkingBytes, budget.RemainingWorkingBytes);
        }
        finally
        {
            input.Resume.TrySetResult();
            (await pending).Dispose();
        }
    }

    [Fact]
    public async Task CancellationDuringSuspendedReadRefundsAndKeepsCallerOwnership()
    {
        var encoded = OriginalExif.Create(MetadataProfile.Png8, 17, 11).Encoded;
        var limits = new ImageLimits { MaxWorkingBytes = 1_000_000, MaxWork = 1_000_000 };
        var budget = new ImageBudget(limits.MaxWorkingBytes, limits.MaxWork);
        using var cancellation = new CancellationTokenSource();
        using var input = new AsyncOnlyInput(encoded) { Pause = true };
        var pending = budget.DecodeAsync(input, new() { Limits = limits }, cancellation.Token).AsTask();
        try
        {
            await input.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5)); cancellation.Cancel();
            var failure = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
            Assert.Equal(cancellation.Token, failure.CancellationToken); Assert.Equal(cancellation.Token, input.LastToken);
            Assert.Equal(0, budget.ConsumedWork); Assert.Equal(budget.MaxWorkingBytes, budget.RemainingWorkingBytes);
            Assert.Equal(budget.MaxWork, budget.RemainingWork); Assert.False(input.WasDisposed);
        }
        finally
        {
            cancellation.Cancel(); input.Resume.TrySetResult();
            try { (await pending).Dispose(); }
            catch (OperationCanceledException) { }
        }
    }

    [Fact]
    public async Task PreCancellationAndEofCancellationReachTheCallerUnchanged()
    {
        using var canceled = new CancellationTokenSource(); canceled.Cancel();
        using var input = new AsyncOnlyInput([]);
        var pre = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => ImageCodec.DecodeAsync(input,
            new() { Limits = new() { MaxWorkingBytes = 1 } }, canceled.Token).AsTask());
        Assert.Equal(canceled.Token, pre.CancellationToken); Assert.Equal(0, input.ReadCalls);
        using var atEof = new CancellationTokenSource();
        var encoded = OriginalExif.Create(MetadataProfile.Png8, 1, 1).Encoded;
        using var eof = new AsyncOnlyInput(encoded) { CancelAtEof = atEof };
        var late = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => ImageCodec.IdentifyAsync(eof, cancellationToken: atEof.Token).AsTask());
        Assert.Equal(atEof.Token, late.CancellationToken); Assert.Equal(1, eof.EofReads); Assert.False(eof.WasDisposed);
    }

    [Theory]
    [InlineData(-1)] [InlineData(8193)]
    public async Task InvalidCallerReadCountsAreRejectedWithoutBufferPublication(int count)
    {
        using var input = new AsyncOnlyInput([]) { InvalidCount = count };
        await Assert.ThrowsAsync<ImageDataException>(() => ImageCodec.DecodeAsync(input).AsTask());
        Assert.False(input.WasDisposed);
    }

    private sealed class AsyncOnlyInput(byte[] data, int chunk = 17) : Stream
    {
        public int ReadCalls, EofReads, Consumed, LargestRequest;
        public CancellationToken LastToken;
        public bool WasDisposed, Pause;
        public int FailAfter = int.MaxValue;
        public int? InvalidCount;
        public IOException? Failure;
        public CancellationTokenSource? CancelAtEof;
        public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Resume { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(WasDisposed, this);
            LastToken = cancellationToken; ReadCalls++; LargestRequest = Math.Max(LargestRequest, buffer.Length);
            // Announce the actual suspension before queuing artificial yield
            // work, so phase entry does not depend on other test continuations.
            if (Pause) { Entered.TrySetResult(); await Resume.Task.WaitAsync(cancellationToken); }
            await Task.Yield(); cancellationToken.ThrowIfCancellationRequested();
            if (InvalidCount is { } invalid) return invalid;
            if (Consumed >= FailAfter && Failure is { } failure) throw failure;
            if (Consumed == data.Length) { EofReads++; CancelAtEof?.Cancel(); return 0; }
            var count = Math.Min(Math.Min(chunk, buffer.Length), data.Length - Consumed);
            data.AsMemory(Consumed, count).CopyTo(buffer); Consumed += count; return count;
        }
        protected override void Dispose(bool disposing) { WasDisposed = true; base.Dispose(disposing); }
        public override bool CanRead => !WasDisposed;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override int Read(byte[] buffer, int offset, int count) => throw new InvalidOperationException("Synchronous read is forbidden.");
        public override int Read(Span<byte> buffer) => throw new InvalidOperationException("Synchronous read is forbidden.");
        public override void Flush() => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
