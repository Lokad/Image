using System.Buffers.Binary;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class PixelValidationTests
{
    [Theory]
    [InlineData(MetadataProfile.Png8)] [InlineData(MetadataProfile.Png16)]
    [InlineData(MetadataProfile.JpegBaseline420)] [InlineData(MetadataProfile.JpegProgressive420)]
    [InlineData(MetadataProfile.WebpLosslessAlpha)] [InlineData(MetadataProfile.WebpLossyAlpha)]
    public async Task CompleteValidationMatchesMetadataAcrossInputRoutes(MetadataProfile profile)
    {
        var data = OriginalExif.Attach(OriginalExif.Create(profile, 17, 11), ImageOrientation.Rotate90, false);
        var original = data.ToArray(); var expected = ImageCodec.Identify(data);
        using var root = new TestMemoryBudget(4 * 1024 * 1024);
        var limits = new ImageLimits { MemoryBudget = root, MaxInputBytes = data.Length };
        Assert.Equal(expected, ImageCodec.ValidatePixels(data, limits)); Assert.Equal(0, root.Bytes);
        foreach (var seekable in new[] { false, true })
        {
            using var input = new Input(seekable ? [1, 2, 3, .. data] : data, seekable, asynchronous: false, chunk: 7);
            if (seekable) input.Position = 3;
            Assert.Equal(expected, ImageCodec.ValidatePixels(input, limits));
            Assert.Equal(data.Length, input.BytesRead); Assert.False(input.WasDisposed);
            using var asyncInput = new Input(seekable ? [1, 2, 3, .. data] : data, seekable, asynchronous: true, chunk: 5);
            if (seekable) asyncInput.Position = 3;
            Assert.Equal(expected, await ImageCodec.ValidatePixelsAsync(asyncInput, limits));
            Assert.Equal(data.Length, asyncInput.BytesRead); Assert.False(asyncInput.WasDisposed);
            Assert.Equal(0, root.Bytes);
            if (seekable) Assert.Equal(data.Length + 3, asyncInput.Position);
        }
        Assert.Equal(original, data); Assert.False(root.IsDisposed);
        Assert.Equal((17, 11, ImageOrientation.Rotate90), (expected.Width, expected.Height, expected.Orientation));
    }

    [Fact]
    public void ExistingAuthoredPngCorpusRetainsAllLegalDepthColorFilterAndAdam7Paths()
    {
        foreach (var frame in AuthoredPng.Cases())
        {
            var data = AuthoredPng.Build(frame);
            Assert.Equal(ImageCodec.Identify(data), ImageCodec.ValidatePixels(data));
            using var decoded = ImageCodec.Decode(data);
            Assert.Equal(AuthoredPng.Expected(frame), OriginalPixels.Bytes(decoded));
        }
    }

    [Fact]
    public void RequiredJpegProfilesUseActualEntropyTableAndProgressionChecks()
    {
        byte[][] cases =
        [
            AuthoredJpegCompatibility.Create(17, 11, [128]),
            AuthoredJpegCompatibility.Create(17, 11, [120, 130, 140], h: 2, v: 2, adobe: 1, restart: 2, vary: true),
            AuthoredJpegCompatibility.Create(17, 11, [120, 130, 140], h: 2, v: 2, progressive: true, restart: 2),
            AuthoredJpegCompatibility.Create(17, 11, [120, 130, 140, 200], adobe: 0),
            AuthoredJpegCompatibility.Create(17, 11, [120, 130, 140, 200], h: 2, v: 2, adobe: 2, progressive: true),
            AuthoredJpegCompatibility.Create(67, 19, [120, 130, 140], h: 4, v: 1, adobe: 1, vary: true),
            AuthoredJpegCompatibility.Create(67, 19, [120, 130, 140], h: 4, v: 2, adobe: 1, vary: true)
        ];
        foreach (var data in cases)
        {
            Assert.Equal(ImageCodec.Identify(data), ImageCodec.ValidatePixels(data));
            Assert.Equal("ok", DecodeOutcome(data));
            var quantization = data.AsSpan().IndexOf(new byte[] { 255, 219 }); Assert.True(quantization >= 0);
            var missingQuantization = data.ToArray(); missingQuantization[quantization + 5] = 0;
            Assert.Equal(nameof(ImageDataException), DecodeOutcome(missingQuantization));
            Assert.Equal(DecodeOutcome(missingQuantization), ValidationOutcome(missingQuantization));
        }
        var unsupported = AuthoredJpegCompatibility.Create(17, 11, [120, 130, 140, 200], adobe: 1);
        Assert.Equal("unsupported:ColorTransform", ValidationOutcome(unsupported));
        Assert.Equal(DecodeOutcome(unsupported), ValidationOutcome(unsupported));
    }

    [Fact]
    public void AuthoredVp8lTransformsHistoryAndHuffmanErrorsAgreeWithDecoding()
    {
        byte[][] valid = [AuthoredWebp.Palette(3, thenPredictor: true), AuthoredWebp.ColorTransform(),
            AuthoredWebp.SubtractGreen(), AuthoredWebp.CachedColor(), AuthoredWebp.BackwardReference(), AuthoredWebp.EntropyGroups(), AuthoredWebp.DuplicateSimpleSymbols()];
        foreach (var data in valid)
        {
            Assert.Equal("ok", DecodeOutcome(data)); Assert.Equal(ImageCodec.Identify(data), ImageCodec.ValidatePixels(data));
        }
        foreach (var data in new[] { AuthoredWebp.InvalidCache(0), AuthoredWebp.InvalidCache(12), AuthoredWebp.BackwardReference(missingLiteral: true) })
        {
            Assert.NotEqual("ok", DecodeOutcome(data)); Assert.Equal(DecodeOutcome(data), ValidationOutcome(data));
        }
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public void RawAndCompressedAlphaAreValidatedWithoutAnRgbaIntermediate(bool compressed)
    {
        using var source = OriginalPixels.Buffer(17, 11, 8, opaque: true); using var output = new MemoryStream();
        ImageCodec.EncodeWebp(source, output, new() { Lossless = false });
        var data = AuthoredAlpha.Wrap(output.ToArray(), 17, 11, 3, compressed: compressed);
        Assert.Equal("ok", DecodeOutcome(data)); Assert.Equal(ImageCodec.Identify(data), ImageCodec.ValidatePixels(data));
        var alpha = data.AsSpan().IndexOf("ALPH"u8); Assert.True(alpha > 0);
        var invalid = data.ToArray(); invalid[alpha + 8] |= 3; // Unsupported compression method.
        Assert.Equal("unsupported:AlphaCoding", ValidationOutcome(invalid)); Assert.Equal(DecodeOutcome(invalid), ValidationOutcome(invalid));
    }

    [Fact]
    public void PngHeaderValidityDoesNotReplacePixelFilterPaletteDeflateOrChecksumValidation()
    {
        var frame = new AuthoredPng.Frame("pixel-validation-errors", 6, 8, 19, 17, false);
        var rows = AuthoredPng.FilteredRows(frame); rows[0] = 5;
        var badFilter = AuthoredPng.Wrap(frame, AuthoredPng.StoredZlib(rows));
        var checksum = AuthoredPng.StoredZlib(AuthoredPng.FilteredRows(frame)); checksum[^1] ^= 1;
        var invalid = new List<byte[]> { badFilter, AuthoredPng.Wrap(frame, checksum), AuthoredPng.Wrap(frame, [0xA7]) };
        invalid.AddRange(AuthoredDeflate.InvalidDynamic().Select(item => AuthoredPng.Wrap(frame, item.Zlib)));
        var indexed = new AuthoredPng.Frame("invalid-palette-index", 3, 8, 1, 1, false);
        var palette = AuthoredPng.Build(indexed); var plte = palette.AsSpan().IndexOf("PLTE"u8); Assert.True(plte > 0);
        // Rebuild the palette with a single entry, but retain index255 and valid CRCs.
        using var packet = new MemoryStream(); packet.Write(palette.AsSpan(0, 33));
        PngChunk(packet, "PLTE"u8, [1, 2, 3]); PngChunk(packet, "IDAT"u8, AuthoredPng.StoredZlib([0, 255])); PngChunk(packet, "IEND"u8, []);
        invalid.Add(packet.ToArray());
        foreach (var data in invalid)
        {
            _ = ImageCodec.Identify(data); // The container/CRCs are valid.
            Assert.Equal(nameof(ImageDataException), DecodeOutcome(data)); Assert.Equal(DecodeOutcome(data), ValidationOutcome(data));
        }
    }

    [Fact]
    public void PngValidationCrossesHistorySlidesAndRejectsLateRowAndChecksumErrors()
    {
        var frame = new AuthoredPng.Frame("validation-sliding-rows", 6, 16, 97, 131, false);
        var filtered = AuthoredPng.FilteredRows(frame);
        Assert.True(filtered.Length > 65536);
        var valid = AuthoredPng.Wrap(frame, AuthoredPng.StoredZlib(filtered));
        var original = valid.ToArray();
        using var root = new TestMemoryBudget(512 * 1024);
        var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = 512 * 1024 };
        Assert.Equal(ImageCodec.Identify(valid), ImageCodec.ValidatePixels(valid, limits));
        Assert.Equal(0, root.Bytes); Assert.Equal(original, valid);
        var late = filtered.ToArray(); late[100 * (frame.Width * 8 + 1)] = 5;
        var badChecksum = AuthoredPng.StoredZlib(filtered); badChecksum[^1] ^= 1;
        foreach (var data in new[] { AuthoredPng.Wrap(frame, AuthoredPng.StoredZlib(late)), AuthoredPng.Wrap(frame, badChecksum) })
        {
            _ = ImageCodec.Identify(data);
            Assert.Throws<ImageDataException>(() => ImageCodec.ValidatePixels(data, limits));
            Assert.Equal(nameof(ImageDataException), DecodeOutcome(data));
            Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        }
        Assert.Equal(ImageCodec.Identify(valid), ImageCodec.ValidatePixels(valid, limits));
        Assert.Equal(0, root.Bytes);
    }

    [Theory]
    [InlineData(MetadataProfile.Png8)] [InlineData(MetadataProfile.JpegBaseline420)] [InlineData(MetadataProfile.WebpLosslessAlpha)]
    public void BoundedTruncationAndPayloadMutationsAgreeWithDecode(MetadataProfile profile)
    {
        var data = OriginalExif.Create(profile, 17, 11).Encoded;
        foreach (var size in new[] { 0, 1, 2, data.Length / 2, data.Length - 1 })
            Assert.Equal(DecodeOutcome(data[..size]), ValidationOutcome(data[..size]));
        for (var i = 1; i <= 16; i++)
        {
            var mutated = data.ToArray(); mutated[(int)((long)(data.Length - 1) * i / 17)] ^= 1;
            Assert.Equal(DecodeOutcome(mutated), ValidationOutcome(mutated));
        }
        Assert.Equal(DecodeOutcome([.. data, 0xA7]), ValidationOutcome([.. data, 0xA7]));
    }

    [Theory]
    [InlineData(MetadataProfile.Png8)] [InlineData(MetadataProfile.JpegBaseline420)] [InlineData(MetadataProfile.WebpLosslessAlpha)]
    public async Task MetadataPolicyIsSharedAndNeverWeakensResourceOrOuterContainerChecks(MetadataProfile profile)
    {
        var fixture = OriginalExif.Create(profile, 17, 11);
        var data = OriginalExif.Attach(fixture, ImageOrientation.Rotate90, false);
        var exif = fixture.Format == ImageFormat.Png ? 41 : fixture.Jpeg ? 12 : data.AsSpan().LastIndexOf("EXIF"u8) + 8;
        data[exif + 18] = 9;
        if (fixture.Format == ImageFormat.Png) UpdatePngCrc(data, 33);
        Assert.Equal(nameof(ImageDataException), ValidationOutcome(data));
        var policy = ImageMetadataPolicy.IgnoreInvalid;
        Assert.Equal(ImageCodec.Identify(data, null, default, policy), ImageCodec.ValidatePixels(data, metadataPolicy: policy));
        using var input = new Input(data, false, true, 3);
        Assert.Equal(ImageOrientation.Normal, (await ImageCodec.ValidatePixelsAsync(input, metadataPolicy: policy)).Orientation);
        var failure = Assert.Throws<ImageLimitException>(() => ImageCodec.ValidatePixels(data,
            new() { MaxMetadataBytes = 1 }, metadataPolicy: policy));
        Assert.Equal(ImageLimitKind.MetadataBytes, failure.Limit);
        Assert.Equal(DecodeOutcome(data[..^1], policy), ValidationOutcome(data[..^1], policy));
    }

    [Theory]
    [InlineData(MetadataProfile.Png8)] [InlineData(MetadataProfile.JpegBaseline420)] [InlineData(MetadataProfile.WebpLosslessAlpha)]
    public void RefusalAndCpuCancellationAreTerminalAndReleaseAllOwnedStorage(MetadataProfile profile)
    {
        var data = OriginalExif.Create(profile, 17, 11).Encoded;
        using var root = new TestMemoryBudget(8 * 1024 * 1024) { RefuseAtGrowth = 2 };
        var limits = new ImageLimits { MemoryBudget = root };
        var refusal = Assert.Throws<ImageLimitException>(() => ImageCodec.ValidatePixels(data, limits));
        Assert.Equal(ImageLimitScope.SharedBudget, refusal.Scope); Assert.Equal(1, root.Refusals); Assert.Equal(0, root.Bytes);
        root.RefuseAtGrowth = 0;
        using var canceled = new CancellationTokenSource();
        var nextGrowth = root.GrowthAttempts + 2;
        root.Admitted = _ => { if (root.GrowthAttempts == nextGrowth) canceled.Cancel(); };
        var failure = Assert.ThrowsAny<OperationCanceledException>(() => ImageCodec.ValidatePixels(data, limits, canceled.Token));
        Assert.Equal(canceled.Token, failure.CancellationToken); Assert.Equal(0, root.Bytes);
        root.Admitted = null;
        Assert.Equal(ImageCodec.Identify(data), ImageCodec.ValidatePixels(data, limits)); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public async Task AsyncUsersKeepStorageAdmittedThroughCancellationAndBorrowTheInput()
    {
        var data = OriginalExif.Create(MetadataProfile.Png16, 17, 11).Encoded;
        using var root = new TestMemoryBudget(1024 * 1024); using var cancellation = new CancellationTokenSource();
        using var input = new Input(data, false, true, 5)
        { DuringRead = () => { Assert.True(root.Bytes > 0); cancellation.Cancel(); } };
        var limits = new ImageLimits { MemoryBudget = root };
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => ImageCodec.ValidatePixelsAsync(input, limits, cancellation.Token).AsTask());
        Assert.Equal(0, root.Bytes); Assert.False(input.WasDisposed); Assert.False(root.IsDisposed);
        using var success = new Input(data, false, true, 5);
        await ImageCodec.ValidatePixelsAsync(success, limits); Assert.Equal(0, root.Bytes);
    }

    [Fact]
    public async Task ConcurrentReadsShareOneRootAndCancellationAwaitsTheActualUser()
    {
        const int capacity = 1024 * 1024;
        var data = OriginalExif.Create(MetadataProfile.Png8, 17, 11).Encoded;
        using var root = new TestMemoryBudget(capacity); Assert.True(root.TryReserve(capacity - 8192, out var caller));
        var limits = new ImageLimits { MemoryBudget = root };
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var cancellation = new CancellationTokenSource();
        using var input = new Input(data, false, true, 5) { DuringRead = () => entered.TrySetResult(), ReadGate = release.Task };
        var pending = ImageCodec.ValidatePixelsAsync(input, limits, cancellation.Token).AsTask();
        try
        {
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
            cancellation.Cancel(); Assert.False(pending.IsCompleted); Assert.Equal(capacity, root.Bytes);
            using var concurrent = new Input(data, false, true, 5);
            var refused = await Assert.ThrowsAsync<ImageLimitException>(() => ImageCodec.ValidatePixelsAsync(concurrent, limits).AsTask());
            Assert.Equal(ImageLimitScope.SharedBudget, refused.Scope); Assert.Equal(0, concurrent.BytesRead);
            Assert.Equal(capacity, root.Bytes); Assert.False(pending.IsCompleted);
        }
        finally { release.TrySetResult(); }
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
        Assert.Equal(capacity - 8192, root.Bytes); caller.Dispose(); Assert.Equal(0, root.Bytes);
        Assert.Equal(ImageCodec.Identify(data), ImageCodec.ValidatePixels(data, limits)); Assert.Equal(0, root.Bytes);
        Assert.False(input.WasDisposed); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void AnimationAndInputDimensionMemoryLimitsRemainStructured()
    {
        var frame = new AuthoredPng.Frame("pixel-validation-animation", 6, 8, 1, 1, false);
        var animated = AuthoredPng.Wrap(frame, AuthoredPng.StoredZlib(AuthoredPng.FilteredRows(frame)), ("acTL", new byte[8]));
        Assert.Equal("unsupported:Animation", ValidationOutcome(animated));
        var data = OriginalExif.Create(MetadataProfile.Png8, 17, 11).Encoded;
        var cases = new[] { (new ImageLimits { MaxInputBytes = data.Length - 1 }, ImageLimitKind.InputBytes),
            (new ImageLimits { MaxDimension = 1 }, ImageLimitKind.Dimension), (new ImageLimits { MaxPixels = 1 }, ImageLimitKind.PixelCount),
            (new ImageLimits { MaxWorkingBytes = 1 }, ImageLimitKind.WorkingBytes), (new ImageLimits { MaxContainerItems = 1 }, ImageLimitKind.ContainerItems) };
        foreach (var (limits, kind) in cases) Assert.Equal(kind, Assert.Throws<ImageLimitException>(() => ImageCodec.ValidatePixels(data, limits)).Limit);
    }

    private static string DecodeOutcome(byte[] data, ImageMetadataPolicy policy = ImageMetadataPolicy.Strict)
        => Observe(() => { using var result = ImageCodec.Decode(data, new() { MetadataPolicy = policy }); });
    private static string ValidationOutcome(byte[] data, ImageMetadataPolicy policy = ImageMetadataPolicy.Strict)
        => Observe(() => ImageCodec.ValidatePixels(data, metadataPolicy: policy));
    private static string Observe(Action action)
    {
        try { action(); return "ok"; }
        catch (ImageDataException) { return nameof(ImageDataException); }
        catch (UnsupportedImageException exception) { return "unsupported:" + exception.Feature; }
        catch (ImageLimitException exception) { return "limit:" + exception.Limit + ":" + exception.Scope; }
    }

    private static void PngChunk(Stream output, ReadOnlySpan<byte> type, byte[] payload)
    {
        var chunk = new byte[payload.Length + 12]; BinaryPrimitives.WriteInt32BigEndian(chunk, payload.Length);
        type.CopyTo(chunk.AsSpan(4)); payload.CopyTo(chunk, 8); UpdatePngCrc(chunk, 0); output.Write(chunk);
    }
    private static void UpdatePngCrc(byte[] packet, int offset)
    {
        var count = BinaryPrimitives.ReadInt32BigEndian(packet.AsSpan(offset)); uint crc = uint.MaxValue;
        foreach (var value in packet.AsSpan(offset + 4, count + 4))
        { crc ^= value; for (var bit = 0; bit < 8; bit++) crc = crc >> 1 ^ ((crc & 1) == 0 ? 0u : 0xEDB88320u); }
        BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(offset + 8 + count), ~crc);
    }

    private sealed class Input(byte[] data, bool seekable, bool asynchronous, int chunk) : Stream
    {
        private readonly MemoryStream inner = new(data, writable: false);
        public long BytesRead;
        public bool WasDisposed;
        public Action? DuringRead;
        public Task? ReadGate;
        public override bool CanSeek => seekable;
        public override bool CanRead => !WasDisposed;
        public override bool CanWrite => false;
        public override long Position { get => seekable ? inner.Position : throw new NotSupportedException(); set => inner.Position = value; }
        public override long Length => seekable ? inner.Length : throw new NotSupportedException();
        public override int Read(Span<byte> target)
        { Assert.False(asynchronous); var count = inner.Read(target[..Math.Min(target.Length, chunk)]); BytesRead += count; return count; }
        public override int Read(byte[] target, int offset, int count) => Read(target.AsSpan(offset, count));
        public override async ValueTask<int> ReadAsync(Memory<byte> target, CancellationToken cancellationToken = default)
        {
            Assert.True(asynchronous); await Task.Yield(); DuringRead?.Invoke();
            if (ReadGate is { } gate) await gate.ConfigureAwait(false);
            var count = await inner.ReadAsync(target[..Math.Min(target.Length, chunk)], cancellationToken); BytesRead += count; return count;
        }
        public override long Seek(long value, SeekOrigin origin) => seekable ? inner.Seek(value, origin) : throw new NotSupportedException();
        protected override void Dispose(bool disposing) { WasDisposed = true; inner.Dispose(); base.Dispose(disposing); }
        public override void Flush() => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] source, int offset, int count) => throw new NotSupportedException();
    }
}
