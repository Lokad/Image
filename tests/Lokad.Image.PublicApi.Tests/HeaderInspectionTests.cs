using System.Buffers.Binary;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class HeaderInspectionTests
{
    private const int Workspace = 8448;

    [Theory]
    [InlineData(MetadataProfile.Png8)] [InlineData(MetadataProfile.Png16)]
    [InlineData(MetadataProfile.JpegBaseline420)] [InlineData(MetadataProfile.JpegProgressive420)]
    [InlineData(MetadataProfile.WebpLosslessAlpha)] [InlineData(MetadataProfile.WebpLossyAlpha)]
    public async Task SyncAndAsyncShortReadsMatchFullMetadataWithoutOwningInput(MetadataProfile profile)
    {
        var fixture = OriginalExif.Create(profile, 17, 11);
        var data = OriginalExif.Attach(fixture, ImageOrientation.Rotate90, bigEndian: fixture.Jpeg);
        var original = data.ToArray(); var expected = ImageCodec.Identify(data);
        var limits = new ImageLimits { MaxWorkingBytes = Workspace };
        using var sync = SparseInput.Bytes(data, asynchronous: false, chunk: 7);
        Assert.Equal(expected, ImageCodec.InspectHeaders(sync, limits)); Assert.Equal(data.Length, sync.BytesRead);
        using var token = new CancellationTokenSource(); using var input = SparseInput.Bytes(data, asynchronous: true, chunk: 5);
        var actual = await ImageCodec.InspectHeadersAsync(input, limits, token.Token);
        Assert.Equal(expected, actual); Assert.Equal((11, 17), (actual.OrientedWidth, actual.OrientedHeight));
        Assert.Equal(token.Token, input.LastToken); Assert.False(input.WasDisposed); Assert.False(sync.WasDisposed);
        Assert.Equal(original, data); Assert.Equal(data.Length, input.BytesRead);
    }

    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)]
    [InlineData(5)] [InlineData(6)] [InlineData(7)] [InlineData(8)]
    public async Task EveryExifOrientationAndBothByteOrdersArePreserved(int value)
    {
        var fixture = OriginalExif.Create(MetadataProfile.JpegBaseline420, 17, 11);
        var data = OriginalExif.Attach(fixture, (ImageOrientation)value, bigEndian: value % 2 == 0, directoryEntries: 3);
        using var input = SparseInput.Bytes(data, asynchronous: true, chunk: 3);
        var result = await ImageCodec.InspectHeadersAsync(input, new() { MaxWorkingBytes = Workspace });
        Assert.Equal((ImageOrientation)value, result.Orientation);
        Assert.Equal(value >= 5 ? (11, 17) : (17, 11), (result.OrientedWidth, result.OrientedHeight));
    }

    [Theory]
    [InlineData(0, 1)] [InlineData(0, 16)] [InlineData(2, 8)] [InlineData(3, 1)] [InlineData(3, 8)]
    public async Task PackedPaletteAndColorKeyHeadersPreserveDeclaredPrecisionAndAlpha(int color, int depth)
    {
        var data = AuthoredPng.Build(new("inspection", color, depth, 19, 17, true));
        using var input = SparseInput.Bytes(data, asynchronous: true, chunk: 13);
        Assert.Equal(ImageCodec.Identify(data), await ImageCodec.InspectHeadersAsync(input, new() { MaxWorkingBytes = Workspace }));
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task FourComponentProgressiveRestartAndQuarterWidthHeadersRemainInspectable(bool progressive)
    {
        foreach (var values in new[] { new byte[] { 180, 90, 140 }, new byte[] { 180, 90, 140, 200 } })
        {
            var data = AuthoredJpegCompatibility.Create(67, 19, values, h: 4, v: 2, adobe: values.Length == 4 ? 2 : 1, progressive: progressive, restart: 2, vary: true);
            using var input = SparseInput.Bytes(data, asynchronous: true, chunk: 7);
            Assert.Equal(ImageCodec.Identify(data), await ImageCodec.InspectHeadersAsync(input, new() { MaxWorkingBytes = Workspace }));
        }
    }

    [Theory]
    [InlineData("png", false)] [InlineData("png", true)]
    [InlineData("webp", false)] [InlineData("webp", true)]
    [InlineData("jpeg", false)] [InlineData("jpeg", true)]
    public async Task LargeOpaquePayloadsUseFixedWorkspaceAndSeekOnlyWhenFramingAllows(string format, bool asynchronous)
    {
        using var input = Large(format, asynchronous);
        var limits = new ImageLimits { MaxWorkingBytes = Workspace, MaxDimension = 65535, MaxPixels = 3_500_000_000 };
        var result = asynchronous ? await ImageCodec.InspectHeadersAsync(input, limits) : ImageCodec.InspectHeaders(input, limits);
        var expected = format switch
        {
            "png" => new ImageInfo(ImageFormat.Png, 50000, 50000, 8, true, ImageOrientation.Normal),
            "webp" => new ImageInfo(ImageFormat.WebP, 16384, 4096, 8, false, ImageOrientation.Rotate270),
            _ => new ImageInfo(ImageFormat.Jpeg, 60000, 50000, 8, false, ImageOrientation.Rotate90)
        };
        Assert.Equal(expected, result); Assert.Equal(input.Length, input.Position); Assert.False(input.WasDisposed);
        Assert.InRange(input.LargestRequest, 1, 8192);
        if (format == "jpeg") { Assert.True(input.BytesRead >= 4 * 1024 * 1024); Assert.Equal(0, input.SeekCalls); }
        else { Assert.True(input.BytesRead < 20000); Assert.True(input.SeekCalls > 0); }
    }

    [Fact]
    public async Task LargeExifDirectoryGapIsSkippedWithoutMetadataAllocation()
    {
        const int payload = 16 * 1024 * 1024, offset = payload - 18;
        var prefix = PngHeader(17, 11); var chunk = ChunkHeader("eXIf", payload);
        var tiff = new byte[8]; "II"u8.CopyTo(tiff); BinaryPrimitives.WriteUInt16LittleEndian(tiff.AsSpan(2), 42); BinaryPrimitives.WriteInt32LittleEndian(tiff.AsSpan(4), offset);
        var directory = new byte[18]; BinaryPrimitives.WriteUInt16LittleEndian(directory, 1);
        BinaryPrimitives.WriteUInt16LittleEndian(directory.AsSpan(2), 0x0112); BinaryPrimitives.WriteUInt16LittleEndian(directory.AsSpan(4), 3);
        BinaryPrimitives.WriteInt32LittleEndian(directory.AsSpan(6), 1); BinaryPrimitives.WriteUInt16LittleEndian(directory.AsSpan(10), 8);
        var tail = new byte[25]; ChunkHeader("IDAT", 1).CopyTo(tail, 0); ChunkHeader("IEND", 0).CopyTo(tail, 13);
        using var input = new SparseInput(prefix.Length + 8L + payload + 4 + tail.Length, true, true,
            [(0, prefix), (prefix.Length, chunk), (prefix.Length + 8, tiff), (prefix.Length + 8L + offset, directory), (prefix.Length + 8L + payload + 4, tail)]);
        var result = await ImageCodec.InspectHeadersAsync(input, new() { MaxWorkingBytes = Workspace, MaxMetadataBytes = payload });
        Assert.Equal(ImageOrientation.Rotate270, result.Orientation); Assert.True(input.BytesRead < 20000); Assert.True(input.SeekCalls > 0);
    }

    [Theory]
    [InlineData(MetadataProfile.Png8)] [InlineData(MetadataProfile.JpegBaseline420)] [InlineData(MetadataProfile.WebpLosslessAlpha)]
    public async Task TruncationAndTrailingBytesAreRejectedEvenForSeekableSources(MetadataProfile profile)
    {
        var data = OriginalExif.Attach(OriginalExif.Create(profile, 17, 11), ImageOrientation.Rotate90, false);
        using var truncated = SparseInput.Bytes(data[..^1], true, seekable: true);
        await Assert.ThrowsAsync<ImageDataException>(() => ImageCodec.InspectHeadersAsync(truncated).AsTask());
        using var trailing = SparseInput.Bytes([.. data, 0xA7], true, seekable: true);
        await Assert.ThrowsAsync<ImageDataException>(() => ImageCodec.InspectHeadersAsync(trailing).AsTask());
        Assert.False(truncated.WasDisposed); Assert.False(trailing.WasDisposed);
    }

    [Fact]
    public void HeaderScopeDoesNotPretendToValidatePngCrc()
    {
        var data = OriginalExif.Create(MetadataProfile.Png8, 17, 11).Encoded;
        var expected = ImageCodec.Identify(data); data[29] ^= 1;
        Assert.Throws<ImageDataException>(() => ImageCodec.Identify(data));
        using var input = SparseInput.Bytes(data, false);
        Assert.Equal(expected, ImageCodec.InspectHeaders(input));
    }

    [Theory]
    [InlineData("gif", UnsupportedImageFeature.Format)]
    [InlineData("apng", UnsupportedImageFeature.Animation)]
    [InlineData("webp", UnsupportedImageFeature.Animation)]
    [InlineData("jpeg12", UnsupportedImageFeature.BitDepth)]
    public async Task UnsupportedFeaturesRemainStructured(string kind, UnsupportedImageFeature feature)
    {
        byte[] data;
        if (kind == "gif") data = "GIF89a123456789"u8.ToArray();
        else if (kind == "apng")
        {
            var frame = new AuthoredPng.Frame("animation", 6, 8, 1, 1, false);
            data = AuthoredPng.Wrap(frame, AuthoredPng.StoredZlib(AuthoredPng.FilteredRows(frame)), ("acTL", new byte[8]));
        }
        else if (kind == "webp")
        {
            data = OriginalExif.Attach(OriginalExif.Create(MetadataProfile.WebpLosslessAlpha, 17, 11), ImageOrientation.Normal, false);
            data[20] |= 2;
        }
        else
        {
            data = AuthoredJpegCompatibility.Create(2, 1, [128]);
            var frame = data.AsSpan().IndexOf(new byte[] { 255, 192 }); Assert.True(frame >= 0); data[frame + 4] = 12;
        }
        using var input = SparseInput.Bytes(data, true, chunk: 11);
        var failure = await Assert.ThrowsAsync<UnsupportedImageException>(() => ImageCodec.InspectHeadersAsync(input).AsTask());
        Assert.Equal(feature, failure.Feature); Assert.False(input.WasDisposed);
    }

    [Theory]
    [InlineData(ImageLimitKind.InputBytes)] [InlineData(ImageLimitKind.WorkingBytes)]
    [InlineData(ImageLimitKind.MetadataBytes)] [InlineData(ImageLimitKind.ContainerItems)]
    [InlineData(ImageLimitKind.Work)] [InlineData(ImageLimitKind.Dimension)] [InlineData(ImageLimitKind.PixelCount)]
    public async Task LimitsRemainTypedAndSharedReservationsRefundOnFailure(ImageLimitKind kind)
    {
        var data = OriginalExif.Attach(OriginalExif.Create(MetadataProfile.Png8, 17, 11), ImageOrientation.Rotate90, false);
        var limits = new ImageLimits { MaxWorkingBytes = Workspace, MaxWork = 1_000_000 };
        limits = kind switch
        {
            ImageLimitKind.InputBytes => limits with { MaxInputBytes = 1 }, ImageLimitKind.WorkingBytes => limits with { MaxWorkingBytes = Workspace - 1 },
            ImageLimitKind.MetadataBytes => limits with { MaxMetadataBytes = 1 }, ImageLimitKind.ContainerItems => limits with { MaxContainerItems = 1 },
            ImageLimitKind.Work => limits with { MaxWork = 1 }, ImageLimitKind.Dimension => limits with { MaxDimension = 1 },
            _ => limits with { MaxPixels = 1 }
        };
        var budget = new ImageBudget(Workspace, 2_000_000); using var input = SparseInput.Bytes(data, true);
        var failure = await Assert.ThrowsAsync<ImageLimitException>(() => budget.InspectHeadersAsync(input, limits).AsTask());
        Assert.Equal(kind, failure.Limit); Assert.Equal(ImageLimitScope.Operation, failure.Scope);
        Assert.Equal(budget.MaxWorkingBytes, budget.RemainingWorkingBytes); Assert.False(input.WasDisposed);
        if (kind == ImageLimitKind.WorkingBytes) Assert.Equal(0, input.ReadCalls);
    }

    [Fact]
    public async Task SharedBudgetSurvivesAwaitCancellationAndCallerIoFailure()
    {
        var data = OriginalExif.Create(MetadataProfile.Png8, 17, 11).Encoded;
        var limits = new ImageLimits { MaxWorkingBytes = Workspace, MaxWork = 1_000_000 };
        var budget = new ImageBudget(Workspace, 3_000_000);
        using var token = new CancellationTokenSource(); using var input = SparseInput.Bytes(data, true); input.Pause = true;
        var pending = budget.InspectHeadersAsync(input, limits, token.Token).AsTask();
        await input.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5)); Assert.False(pending.IsCompleted); Assert.Equal(0, budget.RemainingWorkingBytes);
        var refused = Assert.Throws<ImageLimitException>(() => budget.Reserve(1)); Assert.Equal(ImageLimitScope.SharedBudget, refused.Scope);
        token.Cancel(); var canceled = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
        Assert.Equal(token.Token, canceled.CancellationToken); Assert.Equal(Workspace, budget.RemainingWorkingBytes); Assert.False(input.WasDisposed);
        var original = new IOException("Original caller read failure."); using var broken = SparseInput.Bytes(data, true, chunk: 7); broken.FailAfter = 7; broken.Failure = original;
        var failure = await Assert.ThrowsAsync<IOException>(() => budget.InspectHeadersAsync(broken, limits).AsTask());
        Assert.Same(original, failure); Assert.Equal(7, budget.ConsumedWork); Assert.Equal(Workspace, budget.RemainingWorkingBytes);
        using var recovery = SparseInput.Bytes(data, false); Assert.Equal(ImageCodec.Identify(data), budget.InspectHeaders(recovery, limits));
        Assert.Equal(Workspace, budget.RemainingWorkingBytes);
    }

    [Fact]
    public async Task FalseSeekResultIsRejectedAndPreCancellationDoesNotRead()
    {
        using var input = Large("png", true); input.FalseSeek = true;
        await Assert.ThrowsAsync<ImageDataException>(() => ImageCodec.InspectHeadersAsync(input,
            new() { MaxDimension = 65535, MaxPixels = 3_500_000_000 }).AsTask());
        using var token = new CancellationTokenSource(); token.Cancel(); using var canceled = SparseInput.Bytes([], true);
        var failure = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => ImageCodec.InspectHeadersAsync(canceled, cancellationToken: token.Token).AsTask());
        Assert.Equal(token.Token, failure.CancellationToken); Assert.Equal(0, canceled.ReadCalls);
    }

    [Theory]
    [InlineData(MetadataProfile.Png8)] [InlineData(MetadataProfile.JpegBaseline420)] [InlineData(MetadataProfile.WebpLosslessAlpha)]
    public async Task SeekableInspectionStartsAtCurrentPositionWithRelativeInputAdmission(MetadataProfile profile)
    {
        var data = OriginalExif.Attach(OriginalExif.Create(profile, 17, 11), ImageOrientation.Rotate90, false);
        using var input = SparseInput.Bytes([1, 2, 3, .. data], true, seekable: true);
        input.Position = 3;
        Assert.Equal(ImageCodec.Identify(data), await ImageCodec.InspectHeadersAsync(input,
            new() { MaxWorkingBytes = Workspace, MaxInputBytes = data.Length }));
        Assert.Equal(input.Length, input.Position); Assert.Equal(data.Length, input.BytesRead);
    }

    [Theory]
    [InlineData("byte-order")] [InlineData("offset")] [InlineData("directory")]
    [InlineData("type")] [InlineData("count")] [InlineData("orientation")]
    public async Task InvalidStreamedExifIsRejectedWithoutLeakingAdmission(string field)
    {
        var tiff = Tiff(6);
        switch (field)
        {
            case "byte-order": tiff[0] = 0; break;
            case "offset": BinaryPrimitives.WriteUInt32LittleEndian(tiff.AsSpan(4), uint.MaxValue); break;
            case "directory": BinaryPrimitives.WriteUInt16LittleEndian(tiff.AsSpan(8), ushort.MaxValue); break;
            case "type": tiff[12] = 4; break;
            case "count": tiff[14] = 2; break;
            default: tiff[18] = 9; break;
        }
        var frame = new AuthoredPng.Frame("invalid-exif", 6, 8, 1, 1, false);
        var data = AuthoredPng.Wrap(frame, AuthoredPng.StoredZlib(AuthoredPng.FilteredRows(frame)), ("eXIf", tiff));
        var limits = new ImageLimits { MaxWorkingBytes = Workspace, MaxWork = 1_000_000 };
        var budget = new ImageBudget(Workspace, limits.MaxWork); using var input = SparseInput.Bytes(data, true, chunk: 3);
        await Assert.ThrowsAsync<ImageDataException>(() => budget.InspectHeadersAsync(input, limits).AsTask());
        Assert.Equal(Workspace, budget.RemainingWorkingBytes); Assert.False(input.WasDisposed);
    }

    [Fact]
    public async Task SeekedPayloadStillConsumesInputAndWorkQuotas()
    {
        using var input = Large("png", true);
        var limits = new ImageLimits { MaxWorkingBytes = Workspace, MaxDimension = 65535, MaxPixels = 3_500_000_000,
            MaxInputBytes = (int)input.Length, MaxWork = input.Length + 3 };
        var budget = new ImageBudget(Workspace, 3 * limits.MaxWork);
        await budget.InspectHeadersAsync(input, limits);
        Assert.Equal(input.Length + 3, budget.ConsumedWork); Assert.True(input.BytesRead < 20000);
        using var work = Large("png", true);
        var failure = await Assert.ThrowsAsync<ImageLimitException>(() => budget.InspectHeadersAsync(work,
            limits with { MaxWork = limits.MaxWork - 1 }).AsTask());
        Assert.Equal(ImageLimitKind.Work, failure.Limit);
        using var bytes = Large("png", true);
        failure = await Assert.ThrowsAsync<ImageLimitException>(() => budget.InspectHeadersAsync(bytes,
            limits with { MaxInputBytes = limits.MaxInputBytes - 1 }).AsTask());
        Assert.Equal(ImageLimitKind.InputBytes, failure.Limit); Assert.Equal(Workspace, budget.RemainingWorkingBytes);
    }

    private static byte[] ChunkHeader(string type, int size)
    {
        var bytes = new byte[8]; BinaryPrimitives.WriteInt32BigEndian(bytes, size); System.Text.Encoding.ASCII.GetBytes(type).CopyTo(bytes, 4); return bytes;
    }
    private static byte[] PngHeader(int width, int height)
    {
        var bytes = new byte[33]; new byte[] {137,80,78,71,13,10,26,10}.CopyTo(bytes, 0); ChunkHeader("IHDR", 13).CopyTo(bytes, 8);
        BinaryPrimitives.WriteInt32BigEndian(bytes.AsSpan(16), width); BinaryPrimitives.WriteInt32BigEndian(bytes.AsSpan(20), height); bytes[24] = 8; bytes[25] = 6; return bytes;
    }
    private static byte[] Tiff(int orientation)
    {
        var bytes = new byte[26]; "II"u8.CopyTo(bytes); BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(2), 42); BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(4), 8);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(8), 1); BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(10), 0x0112); BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(12), 3);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(14), 1); BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(18), (ushort)orientation); return bytes;
    }
    private static SparseInput Large(string format, bool asynchronous)
    {
        const int payload = 32 * 1024 * 1024;
        if (format == "png")
        {
            var prefix = PngHeader(50000, 50000); var idat = ChunkHeader("IDAT", payload); var iend = new byte[12]; ChunkHeader("IEND", 0).CopyTo(iend, 0);
            return new SparseInput(33L + 8 + payload + 4 + 12, true, asynchronous, [(0, prefix), (33, idat), (33L + 8 + payload + 4, iend)]);
        }
        if (format == "webp")
        {
            var prefix = new byte[43]; "RIFF"u8.CopyTo(prefix); "WEBPVP8X"u8.CopyTo(prefix.AsSpan(8)); BinaryPrimitives.WriteInt32LittleEndian(prefix.AsSpan(16), 10); prefix[20] = 8;
            prefix[24] = 255; prefix[25] = 63; prefix[27] = 255; prefix[28] = 15;
            "VP8L"u8.CopyTo(prefix.AsSpan(30)); BinaryPrimitives.WriteInt32LittleEndian(prefix.AsSpan(34), payload); prefix[38] = 0x2F;
            BinaryPrimitives.WriteUInt32LittleEndian(prefix.AsSpan(39), 16383u | 4095u << 14);
            var exif = new byte[34]; "EXIF"u8.CopyTo(exif); BinaryPrimitives.WriteInt32LittleEndian(exif.AsSpan(4), 26); Tiff(8).CopyTo(exif, 8);
            var length = 38L + payload + exif.Length; BinaryPrimitives.WriteInt32LittleEndian(prefix.AsSpan(4), (int)length - 8);
            return new SparseInput(length, true, asynchronous, [(0, prefix), (38L + payload, exif)]);
        }
        const int entropy = 4 * 1024 * 1024;
        byte[] jpeg = [255,216,255,192,0,11,8,0xC3,0x50,0xEA,0x60,1,1,0x11,0,255,218,0,8,1,1,0,0,63,0];
        var tail = new byte[38]; tail[0] = 255; tail[1] = 225; tail[3] = 34; "Exif\0\0"u8.CopyTo(tail.AsSpan(4)); Tiff(6).CopyTo(tail, 10); tail[^2] = 255; tail[^1] = 217;
        return new SparseInput(jpeg.Length + (long)entropy + tail.Length, true, asynchronous, [(0, jpeg), (jpeg.Length + (long)entropy, tail)]);
    }

    private sealed class SparseInput(long length, bool seekable, bool asynchronous, (long Offset, byte[] Bytes)[] pieces, int chunk = int.MaxValue) : Stream
    {
        private long position;
        public long BytesRead;
        public int ReadCalls, SeekCalls, LargestRequest;
        public bool WasDisposed, Pause, FalseSeek;
        public long FailAfter = long.MaxValue;
        public IOException? Failure;
        public CancellationToken LastToken;
        public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Resume { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal static SparseInput Bytes(byte[] data, bool asynchronous, int chunk = int.MaxValue, bool seekable = false)
            => new(data.Length, seekable, asynchronous, [(0, data)], chunk);
        private int ReadCore(Span<byte> buffer)
        {
            ObjectDisposedException.ThrowIf(WasDisposed, this);
            if (BytesRead >= FailAfter && Failure is { } failure) throw failure;
            if (position >= length) return 0;
            var count = (int)Math.Min(Math.Min(buffer.Length, chunk), length - position); buffer[..count].Clear();
            foreach (var piece in pieces)
            {
                var start = Math.Max(position, piece.Offset); var end = Math.Min(position + count, piece.Offset + piece.Bytes.Length);
                if (end > start) piece.Bytes.AsSpan((int)(start - piece.Offset), (int)(end - start)).CopyTo(buffer[(int)(start - position)..]);
            }
            position += count; BytesRead += count; return count;
        }
        public override int Read(Span<byte> buffer)
        {
            if (asynchronous) throw new InvalidOperationException("Sync read forbidden."); ReadCalls++; LargestRequest = Math.Max(LargestRequest, buffer.Length); return ReadCore(buffer);
        }
        public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (!asynchronous) throw new InvalidOperationException("Async read forbidden."); ReadCalls++; LargestRequest = Math.Max(LargestRequest, buffer.Length); LastToken = cancellationToken;
            await Task.Yield(); cancellationToken.ThrowIfCancellationRequested();
            if (Pause) { Entered.TrySetResult(); await Resume.Task.WaitAsync(cancellationToken); }
            return ReadCore(buffer.Span);
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!seekable) throw new NotSupportedException(); SeekCalls++;
            var target = origin switch { SeekOrigin.Current => position + offset, SeekOrigin.Begin => offset, _ => length + offset };
            if (target < 0) throw new IOException(); position = target; return FalseSeek ? target + 1 : target;
        }
        protected override void Dispose(bool disposing) { WasDisposed = true; base.Dispose(disposing); }
        public override bool CanRead => !WasDisposed;
        public override bool CanSeek => seekable;
        public override bool CanWrite => false;
        public override long Length => seekable ? length : throw new NotSupportedException();
        public override long Position { get => seekable ? position : throw new NotSupportedException(); set => Seek(value, SeekOrigin.Begin); }
        public override void Flush() => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
