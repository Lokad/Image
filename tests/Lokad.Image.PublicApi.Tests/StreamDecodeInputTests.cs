using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class StreamDecodeInputTests
{
    private static readonly ImageLimits Limits = new() { MaxWorkingBytes = 4 * 1024 * 1024, MaxWork = 2_000_000 };

    [Theory]
    [InlineData(MetadataProfile.Png8)] [InlineData(MetadataProfile.Png16)]
    [InlineData(MetadataProfile.JpegBaseline420)] [InlineData(MetadataProfile.JpegProgressive420)]
    [InlineData(MetadataProfile.WebpLosslessAlpha)] [InlineData(MetadataProfile.WebpLossyAlpha)]
    public void ShortNonseekableInputPreservesPixelsWorkAndIndependentLifetimes(MetadataProfile profile)
    {
        var fixture = OriginalExif.Create(profile, 17, 11);
        var data = OriginalExif.Attach(fixture, ImageOrientation.Rotate90, true); var original = data.ToArray();
        var options = new DecodeOptions { Limits = Limits, ApplyOrientation = true };
        var spanBudget = new ImageBudget(Limits.MaxWorkingBytes, Limits.MaxWork);
        using var expected = spanBudget.Decode(data, options); var pixels = OriginalPixels.Bytes(expected.Image);
        var budget = new ImageBudget(Limits.MaxWorkingBytes, 3 * Limits.MaxWork);
        using var input = new SyncInput(data); using var actual = budget.Decode(input, options);
        Assert.Equal(spanBudget.ConsumedWork + data.Length, budget.ConsumedWork);
        Assert.Equal(Limits.MaxWorkingBytes - pixels.Length, budget.RemainingWorkingBytes);
        Assert.Equal((11, 17, expected.Image.BitDepth, ImageOrientation.Normal),
            (actual.Image.Width, actual.Image.Height, actual.Image.BitDepth, actual.Image.Orientation));
        Assert.Equal(pixels, OriginalPixels.Bytes(actual.Image)); Assert.Equal(original, data);
        Assert.Equal(1, input.EofReads); Assert.False(input.WasDisposed); Assert.InRange(input.LargestRequest, 1, 8192);
        input.Dispose();
        using var againInput = new SyncInput(data, 31); using var again = ImageCodec.Decode(againInput, options);
        Assert.Equal(pixels, OriginalPixels.Bytes(again));
        if (again.BitDepth == 8) again.GetRow8(0).Fill(0);
        else again.GetRow16(0).Fill(0);
        again.Dispose(); data.AsSpan().Fill(0xA7);
        Assert.Equal(pixels, OriginalPixels.Bytes(actual.Image));
        actual.Dispose(); Assert.Equal(budget.MaxWorkingBytes, budget.RemainingWorkingBytes);
    }

    [Fact]
    public void LargeOptionalMetadataNeedsNoFinalFullInputCopy()
    {
        var frame = new AuthoredPng.Frame("large-stream-input", 6, 8, 1, 1, false);
        var data = AuthoredPng.Wrap(frame, AuthoredPng.StoredZlib(AuthoredPng.FilteredRows(frame)), ("tEXt", new byte[1024 * 1024]));
        var previousCapacity = 8192L; while (previousCapacity * 2 < data.Length) previousCapacity *= 2;
        var limits = Limits with { MaxInputBytes = data.Length, MaxWorkingBytes = 8192 + previousCapacity + data.Length, MaxWork = 8_000_000 };
        using var input = new SyncInput(data, 8192); using var result = ImageCodec.Decode(input, new() { Limits = limits });
        using var expected = ImageCodec.Decode(data); Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(result));
        using var rejected = new SyncInput(data, 8192);
        var failure = Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(rejected,
            new() { Limits = limits with { MaxWorkingBytes = limits.MaxWorkingBytes - 1 } }));
        Assert.Equal(ImageLimitKind.WorkingBytes, failure.Limit); Assert.False(rejected.WasDisposed);
    }

    [Fact]
    public void RetainedCapacityRemainsAdmittedDuringPixelDecoding()
    {
        var frame = new AuthoredPng.Frame("retained-input-capacity", 6, 8, 1, 1, false);
        var compressed = AuthoredPng.StoredZlib(AuthoredPng.FilteredRows(frame)); var data = AuthoredPng.Wrap(frame, compressed);
        Assert.True(data.Length < 256);
        // PNG result + two rows + inflater + compressed bytes, with the full
        // retained 256-byte input lease rather than just its acquired prefix.
        var peak = 256L + compressed.Length + 4 + 8 + 256 * 1024;
        using var exact = new SyncInput(data); using var result = ImageCodec.Decode(exact, new() { Limits = Limits with { MaxWorkingBytes = peak } });
        using var shortInput = new SyncInput(data);
        var failure = Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(shortInput,
            new() { Limits = Limits with { MaxWorkingBytes = peak - 1 } }));
        Assert.Equal(ImageLimitKind.WorkingBytes, failure.Limit);
        using var span = ImageCodec.Decode(data, new() { Limits = Limits with { MaxWorkingBytes = peak - 1 } });
        Assert.Equal(OriginalPixels.Bytes(span), OriginalPixels.Bytes(result));
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void CompleteEofValidationAndFailedInputDoNotCorruptSurvivingOutputs(bool truncated)
    {
        var data = OriginalExif.Create(MetadataProfile.Png16, 17, 11).Encoded;
        using var good = new SyncInput(data); using var survivor = ImageCodec.Decode(good); var saved = OriginalPixels.Bytes(survivor);
        byte[] invalid = truncated ? data[..^1] : [.. data, 0xA7];
        using var broken = new SyncInput(invalid);
        var budget = new ImageBudget(Limits.MaxWorkingBytes, Limits.MaxWork);
        Assert.Throws<ImageDataException>(() => budget.Decode(broken, new() { Limits = Limits }));
        Assert.Equal(1, broken.EofReads); Assert.False(broken.WasDisposed);
        Assert.Equal(budget.MaxWorkingBytes, budget.RemainingWorkingBytes); Assert.Equal(saved, OriginalPixels.Bytes(survivor));
    }

    [Fact]
    public void ScratchGrowthInputAndWorkFailuresRefundSharedReservations()
    {
        var data = OriginalExif.Create(MetadataProfile.Png8, 33, 17).Encoded;
        var budget = new ImageBudget(Limits.MaxWorkingBytes, 5 * Limits.MaxWork);
        using var scratch = new SyncInput(data);
        var failure = Assert.Throws<ImageLimitException>(() => budget.Decode(scratch,
            new() { Limits = Limits with { MaxWorkingBytes = 8191 } }));
        Assert.Equal(ImageLimitKind.WorkingBytes, failure.Limit); Assert.Equal(0, scratch.ReadCalls);
        using var growth = new SyncInput(data, 256);
        failure = Assert.Throws<ImageLimitException>(() => budget.Decode(growth,
            new() { Limits = Limits with { MaxWorkingBytes = 8192 + 256 + 512 - 1 } }));
        Assert.Equal(ImageLimitKind.WorkingBytes, failure.Limit); Assert.Equal(512, growth.Consumed);
        var used = budget.ConsumedWork; using var input = new SyncInput(data);
        failure = Assert.Throws<ImageLimitException>(() => budget.Decode(input,
            new() { Limits = Limits with { MaxInputBytes = 23 } }));
        Assert.Equal(ImageLimitKind.InputBytes, failure.Limit); Assert.Equal(17, budget.ConsumedWork - used);
        used = budget.ConsumedWork; using var work = new SyncInput(data);
        failure = Assert.Throws<ImageLimitException>(() => budget.Decode(work,
            new() { Limits = Limits with { MaxWork = 64 } }));
        Assert.Equal(ImageLimitKind.Work, failure.Limit); Assert.Equal(51, budget.ConsumedWork - used); Assert.Equal(68, work.Consumed);
        Assert.Equal(budget.MaxWorkingBytes, budget.RemainingWorkingBytes);
        using var recovery = new SyncInput(data); using var result = budget.Decode(recovery, new() { Limits = Limits });
        using var expected = ImageCodec.Decode(data);
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(result.Image));
    }

    [Fact]
    public void OriginalIoExceptionAndCancellationPreserveOwnershipAndCharges()
    {
        var data = OriginalExif.Create(MetadataProfile.Png8, 17, 11).Encoded;
        var budget = new ImageBudget(Limits.MaxWorkingBytes, 3 * Limits.MaxWork);
        var original = new IOException("Original caller read failure."); using var broken = new SyncInput(data) { FailAfter = 17, Failure = original };
        Assert.Same(original, Assert.Throws<IOException>(() => budget.Decode(broken, new() { Limits = Limits })));
        Assert.Equal(17, budget.ConsumedWork); Assert.False(broken.WasDisposed);
        using var token = new CancellationTokenSource(); token.Cancel(); using var canceled = new SyncInput(data);
        var failure = Assert.ThrowsAny<OperationCanceledException>(() => budget.Decode(canceled, new() { Limits = Limits }, token.Token));
        Assert.Equal(token.Token, failure.CancellationToken); Assert.Equal(0, canceled.ReadCalls);
        using var atEof = new CancellationTokenSource(); using var eof = new SyncInput(data) { CancelAtEof = atEof };
        failure = Assert.ThrowsAny<OperationCanceledException>(() => budget.Decode(eof, new() { Limits = Limits }, atEof.Token));
        Assert.Equal(atEof.Token, failure.CancellationToken); Assert.Equal(1, eof.EofReads);
        Assert.Equal(budget.MaxWorkingBytes, budget.RemainingWorkingBytes); Assert.False(eof.WasDisposed);
    }

    [Theory]
    [InlineData(-1)] [InlineData(8193)]
    public void InvalidCallerReadCountsFailBeforePublication(int count)
    {
        using var input = new SyncInput([]) { InvalidCount = count };
        Assert.Throws<ImageDataException>(() => ImageCodec.Decode(input)); Assert.False(input.WasDisposed);
    }

    private sealed class SyncInput(byte[] data, int chunk = 17) : Stream
    {
        public int ReadCalls, EofReads, Consumed, LargestRequest;
        public bool WasDisposed;
        public int FailAfter = int.MaxValue;
        public int? InvalidCount;
        public IOException? Failure;
        public CancellationTokenSource? CancelAtEof;
        public override int Read(Span<byte> buffer)
        {
            ObjectDisposedException.ThrowIf(WasDisposed, this); ReadCalls++; LargestRequest = Math.Max(LargestRequest, buffer.Length);
            if (InvalidCount is { } invalid) return invalid;
            if (Consumed >= FailAfter && Failure is { } failure) throw failure;
            if (Consumed == data.Length) { EofReads++; CancelAtEof?.Cancel(); return 0; }
            var count = Math.Min(Math.Min(chunk, buffer.Length), data.Length - Consumed);
            data.AsSpan(Consumed, count).CopyTo(buffer); Consumed += count; return count;
        }
        public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => throw new InvalidOperationException("Async read forbidden.");
        protected override void Dispose(bool disposing) { WasDisposed = true; base.Dispose(disposing); }
        public override bool CanRead => !WasDisposed;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
