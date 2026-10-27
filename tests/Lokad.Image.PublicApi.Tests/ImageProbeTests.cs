using System.Buffers.Binary;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class ImageProbeTests
{
    private const int Workspace = 512;

    [Theory]
    [InlineData(MetadataProfile.Png8)] [InlineData(MetadataProfile.Png16)]
    [InlineData(MetadataProfile.JpegBaseline420)] [InlineData(MetadataProfile.JpegProgressive420)]
    [InlineData(MetadataProfile.WebpLosslessAlpha)] [InlineData(MetadataProfile.WebpLossyAlpha)]
    public async Task SelectedFactsAgreeAcrossRoutesWithoutReadingPixelPayloads(MetadataProfile profile)
    {
        var fixture = OriginalExif.Create(profile, 17, 11);
        var data = OriginalExif.Attach(fixture, ImageOrientation.Rotate90, false);
        var expected = ImageCodec.Identify(data);
        foreach (var asynchronous in new[] { false, true })
        foreach (var seekable in new[] { false, true })
        {
            using var input = new ObservedInput(data, asynchronous, seekable, chunk: 3);
            var result = await Run(input, asynchronous);
            Assert.Equal((17, 11), (result.Width, result.Height));
            Assert.Null(result.Orientation); Assert.True(result.BytesTraversed < data.Length);
            Assert.Equal(input.BytesRead, result.BytesRead); Assert.Equal(input.Offset, result.BytesTraversed);
            if (fixture.Format == ImageFormat.Png) Assert.Null(result.IsAnimated);
            Assert.False(input.WasDisposed);

            using var all = new ObservedInput(data, asynchronous, seekable, chunk: 5);
            var complete = await Run(all, asynchronous, new() { Facts = ImageProbeFacts.All });
            Assert.Equal(ImageProbeFacts.All, complete.EstablishedFacts);
            Assert.Equal(expected.Format, complete.Format);
            Assert.Equal(expected.Orientation, complete.Orientation);
            Assert.Equal(expected.SourceColor, complete.SourceColor);
            Assert.Equal(expected.SourceAlpha, complete.SourceAlpha);
            Assert.Equal(expected.SourceComponentCount, complete.SourceComponentCount);
            Assert.Equal(expected.BitDepth, complete.BitDepth);
            Assert.Equal(expected.HasAlpha, complete.HasAlpha); Assert.False(complete.IsAnimated);
            Assert.Equal(all.BytesRead, complete.BytesRead); Assert.Equal(all.Offset, complete.BytesTraversed);
            Assert.False(all.WasDisposed);
        }
    }

    [Theory]
    [InlineData(MetadataProfile.Png8)] [InlineData(MetadataProfile.JpegBaseline420)]
    [InlineData(MetadataProfile.WebpLosslessAlpha)]
    public async Task PartialHeadersCanAlternateCompletedAndPendingReads(MetadataProfile profile)
    {
        var data = OriginalExif.Attach(OriginalExif.Create(profile, 17, 11), ImageOrientation.Rotate90, false);
        var expected = ImageCodec.Identify(data);
        using var root = new TestMemoryBudget(8448);
        var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = 8448 };
        foreach (var seekable in new[] { false, true })
        {
            using var input = new ObservedInput(data, true, seekable, chunk: 1, mixed: true);
            var result = await ImageCodec.ProbeAsync(input, new() { Facts = ImageProbeFacts.All, Limits = limits });
            Assert.Equal(expected, new ImageInfo(result.Format, result.Width!.Value, result.Height!.Value,
                result.BitDepth!.Value, result.HasAlpha!.Value, result.Orientation!.Value)
                { SourceColor = result.SourceColor!.Value, SourceAlpha = result.SourceAlpha!.Value,
                    SourceComponentCount = result.SourceComponentCount!.Value });
            Assert.True(input.ImmediateReads > 0); Assert.True(input.PendingReads > 0);
            Assert.Equal(input.BytesRead, result.BytesRead); Assert.False(input.WasDisposed); Assert.Equal(0, root.Bytes);
            using var complete = new ObservedInput(data, true, seekable, chunk: 1, mixed: true);
            Assert.Equal(expected, await ImageCodec.InspectHeadersAsync(complete, limits));
            Assert.True(complete.ImmediateReads > 0); Assert.True(complete.PendingReads > 0);
            Assert.False(complete.WasDisposed); Assert.Equal(0, root.Bytes);
        }
    }

    [Theory]
    [InlineData(ImageFormat.Png)] [InlineData(ImageFormat.Jpeg)] [InlineData(ImageFormat.WebP)]
    public async Task DimensionAcquisitionDoesNotGrowWithValidPixelPayload(ImageFormat format)
    {
        using var simple = ImageBuffer.Create(65, 33, 8);
        for (var y = 0; y < simple.Height; y++)
            for (var x = 0; x < simple.Width; x++) simple.GetRow8(y)[4 * x + 3] = 255;
        using var detailed = OriginalPixels.Buffer(65, 33, 8, opaque: true);
        var small = format == ImageFormat.Jpeg ? AuthoredJpegCompatibility.Create(65, 33, [128]) : Encode(simple, format);
        var large = format == ImageFormat.Jpeg ? AuthoredJpegCompatibility.Create(65, 33, [128], nonzeroAc: true, acTerms: 63) : Encode(detailed, format);
        Assert.True(large.Length > small.Length * 2);
        using var decodedSmall = ImageCodec.Decode(small); using var decodedLarge = ImageCodec.Decode(large);
        Assert.Equal((65, 33), (decodedLarge.Width, decodedSmall.Height));
        foreach (var asynchronous in new[] { false, true })
        foreach (var seekable in new[] { false, true })
        {
            using var a = new ObservedInput(small, asynchronous, seekable);
            using var b = new ObservedInput(large, asynchronous, seekable);
            var left = await Run(a, asynchronous); var right = await Run(b, asynchronous);
            Assert.Equal(left.BytesRead, right.BytesRead); Assert.Equal(left.BytesTraversed, right.BytesTraversed);
            if (format != ImageFormat.WebP)
                Assert.Equal(small[..(int)left.BytesRead], large[..(int)right.BytesRead]);
            Assert.Equal((65, 33), (right.Width, right.Height));
            Assert.True(right.BytesRead < small.Length); Assert.Equal(a.Seeks, b.Seeks);
            // RIFF/codec payload lengths vary; all dimension-bearing fields remain identical.
            Assert.Equal(left.Width, right.Width); Assert.Equal(left.Height, right.Height);
        }
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task LateExifRemainsUnknownUntilRequestedAndAcquisitionIsDistinctFromSeeking(bool seekable)
    {
        var fixture = OriginalExif.Create(MetadataProfile.WebpLosslessAlpha, 65, 33);
        var data = OriginalExif.Attach(fixture, ImageOrientation.Rotate270, true);
        using var dimensions = new ObservedInput(data, true, seekable);
        var cheap = await Run(dimensions, true);
        Assert.Null(cheap.Orientation); Assert.Null(cheap.HasAlpha);
        using var orientation = new ObservedInput(data, true, seekable);
        var result = await Run(orientation, true, new() { Facts = ImageProbeFacts.Dimensions | ImageProbeFacts.Orientation });
        Assert.Equal(ImageOrientation.Rotate270, result.Orientation);
        Assert.Equal(orientation.BytesRead, result.BytesRead); Assert.Equal(orientation.Offset, result.BytesTraversed);
        if (seekable) { Assert.True(result.BytesRead < result.BytesTraversed); Assert.True(orientation.Seeks > 0); }
        else Assert.Equal(result.BytesRead, result.BytesTraversed);
    }

    [Fact]
    public async Task JpegLateExifRequiresScanTraversalButDimensionProbeStopsAtFrame()
    {
        var fixture = OriginalExif.Create(MetadataProfile.JpegBaseline420, 65, 33);
        var early = OriginalExif.Attach(fixture, ImageOrientation.Transverse, false);
        var length = BinaryPrimitives.ReadUInt16BigEndian(early.AsSpan(4)) + 2;
        byte[] data = [.. early.AsSpan(0, 2), .. early.AsSpan(2 + length, early.Length - length - 4), .. early.AsSpan(2, length), 255, 217];
        Assert.Equal(ImageOrientation.Transverse, ImageCodec.Identify(data).Orientation);
        using var cheap = new ObservedInput(data, true, true);
        var dimensions = await Run(cheap, true); Assert.Null(dimensions.Orientation);
        using var all = new ObservedInput(data, true, true);
        var result = await Run(all, true, new() { Facts = ImageProbeFacts.Orientation });
        Assert.Equal(ImageOrientation.Transverse, result.Orientation);
        Assert.True(result.BytesRead > dimensions.BytesRead);
        Assert.Equal(all.Offset, result.BytesTraversed);
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task AnimationIsUnknownWhenUnexaminedAndCanBeDetectedWithoutDecoding(bool webp)
    {
        byte[] data;
        if (webp)
        {
            data = OriginalExif.Attach(OriginalExif.Create(MetadataProfile.WebpLosslessAlpha, 17, 11), ImageOrientation.Normal, false);
            data[20] |= 2;
        }
        else
        {
            var frame = new AuthoredPng.Frame("probe-animation-prefix", 6, 8, 17, 11, false);
            byte[] control = [0, 0, 0, 1, 0, 0, 0, 0];
            data = AuthoredPng.Wrap(frame, AuthoredPng.StoredZlib(AuthoredPng.FilteredRows(frame)), ("acTL", control));
        }
        using var input = new ObservedInput(data, true, false);
        var result = await Run(input, true, new() { Facts = ImageProbeFacts.Animation });
        Assert.True(result.IsAnimated); Assert.True(result.BytesRead < data.Length);
        Assert.Throws<UnsupportedImageException>(() => ImageCodec.Decode(data));
    }

    [Theory]
    [InlineData(MetadataProfile.Png8)] [InlineData(MetadataProfile.JpegBaseline420)] [InlineData(MetadataProfile.WebpLosslessAlpha)]
    public async Task ExaminedHeaderTruncationAndAcquisitionRefusalAreDistinct(MetadataProfile profile)
    {
        var data = OriginalExif.Create(profile, 17, 11).Encoded;
        using var good = new ObservedInput(data, true, false);
        var result = await Run(good, true);
        using var truncated = new ObservedInput(data[..((int)result.BytesRead - 1)], true, false);
        await Assert.ThrowsAsync<ImageDataException>(() => Run(truncated, true).AsTask());
        using var limited = new ObservedInput(data, true, false);
        var cap = result.BytesRead - 1;
        var failure = await Assert.ThrowsAsync<ImageLimitException>(() => Run(limited, true, new() { MaxAcquiredBytes = cap }).AsTask());
        Assert.Equal(ImageLimitKind.AcquiredBytes, failure.Limit); Assert.Equal(cap, limited.BytesRead);
        using var exact = new ObservedInput(data, true, false);
        Assert.Equal(result.BytesRead, (await Run(exact, true, new() { MaxAcquiredBytes = result.BytesRead })).BytesRead);
    }

    [Theory]
    [InlineData(ImageLimitKind.InputBytes)] [InlineData(ImageLimitKind.WorkingBytes)]
    [InlineData(ImageLimitKind.MetadataBytes)] [InlineData(ImageLimitKind.ContainerItems)]
    [InlineData(ImageLimitKind.Dimension)] [InlineData(ImageLimitKind.PixelCount)]
    public async Task PrefixLimitsRetainTheirStructuredFailures(ImageLimitKind kind)
    {
        var data = OriginalExif.Attach(OriginalExif.Create(MetadataProfile.JpegBaseline420, 17, 11), ImageOrientation.Normal, false);
        var limits = kind switch
        {
            ImageLimitKind.InputBytes => new() { MaxInputBytes = 1 },
            ImageLimitKind.WorkingBytes => new() { MaxWorkingBytes = Workspace - 1 },
            ImageLimitKind.MetadataBytes => new() { MaxMetadataBytes = 1 },
            ImageLimitKind.ContainerItems => new() { MaxContainerItems = 1 },
            ImageLimitKind.Dimension => new() { MaxDimension = 1 },
            _ => new ImageLimits { MaxPixels = 1 }
        };
        using var input = new ObservedInput(data, true, false);
        var failure = await Assert.ThrowsAsync<ImageLimitException>(() => Run(input, true, new() { Limits = limits }).AsTask());
        Assert.Equal(kind, failure.Limit); Assert.False(input.WasDisposed);
    }

    [Fact]
    public async Task ProbingColorDoesNotInventOrientationAfterSkippingExif()
    {
        var data = OriginalExif.Attach(OriginalExif.Create(MetadataProfile.JpegBaseline420, 17, 11), ImageOrientation.Rotate90, false);
        using var input = new ObservedInput(data, true, false);
        var result = await Run(input, true, new() { Facts = ImageProbeFacts.SourceRepresentation });
        Assert.Equal(ImageCodec.Identify(data).SourceColor, result.SourceColor); Assert.Null(result.Orientation);
        Assert.Equal(ImageProbeFacts.None, result.EstablishedFacts & ImageProbeFacts.Orientation);
    }

    [Fact]
    public async Task IndexedAlphaIsUnknownBeforeTrnsAndKnownWhenRequested()
    {
        var data = AuthoredPng.Build(new("probe-indexed-transparency", 3, 4, 17, 11, false, true));
        using var source = new ObservedInput(data, true, false);
        var early = await Run(source, true, new() { Facts = ImageProbeFacts.SourceRepresentation });
        Assert.Equal(SourceColorModel.Indexed, early.SourceColor); Assert.Equal(4, early.BitDepth);
        Assert.Null(early.SourceAlpha); Assert.Null(early.HasAlpha); Assert.Null(early.Orientation);
        using var alpha = new ObservedInput(data, true, false);
        var result = await Run(alpha, true, new() { Facts = ImageProbeFacts.Transparency });
        Assert.True(result.HasAlpha); Assert.Equal(SourceAlphaRepresentation.PaletteTransparency, result.SourceAlpha);
        Assert.True(result.BytesRead > early.BytesRead);
    }

    [Fact]
    public async Task HostileLengthsUnsupportedFormatsAndPreCancellationStayDistinct()
    {
        byte[] webp = [82, 73, 70, 70, 255, 255, 255, 255, 87, 69, 66, 80];
        using var declared = new ObservedInput(webp, true, false);
        var length = await Assert.ThrowsAsync<ImageLimitException>(() => Run(declared, true).AsTask());
        Assert.Equal(ImageLimitKind.InputBytes, length.Limit);
        using var jpeg = new ObservedInput([255, 216, 255, 225, 0, 1], true, false);
        await Assert.ThrowsAsync<ImageDataException>(() => Run(jpeg, true).AsTask());
        using var gif = new ObservedInput("GIF89a"u8.ToArray(), true, false);
        var unsupported = await Assert.ThrowsAsync<UnsupportedImageException>(() => Run(gif, true).AsTask());
        Assert.Equal(UnsupportedImageFeature.Format, unsupported.Feature);
        using var canceled = new ObservedInput([], true, false); using var token = new CancellationTokenSource(); token.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => ImageCodec.ProbeAsync(canceled, cancellationToken: token.Token).AsTask());
        Assert.Equal(0, canceled.BytesRead);
    }

    [Fact]
    public async Task AdmissionRefusalAndInFlightCancellationReleaseOnlyOwnedStorage()
    {
        var data = OriginalExif.Create(MetadataProfile.Png8, 17, 11).Encoded;
        using var root = new TestMemoryBudget(Workspace);
        var options = new ImageProbeOptions { Limits = new() { MaxWorkingBytes = Workspace, MemoryBudget = root } };
        Assert.True(root.TryReserve(1, out var borrowed));
        using var refused = new ObservedInput(data, true, false);
        var failure = await Assert.ThrowsAsync<ImageLimitException>(() => Run(refused, true, options).AsTask());
        Assert.Equal(ImageLimitScope.SharedBudget, failure.Scope); Assert.Equal(0, refused.BytesRead);
        Assert.Equal(1, root.Bytes); borrowed.Dispose();
        using var source = new CancellationTokenSource();
        using var canceled = new ObservedInput(data, true, false)
        {
            DuringAsyncRead = () => { Assert.Equal(Workspace, root.Bytes); source.Cancel(); }
        };
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => ImageCodec.ProbeAsync(canceled, options, source.Token).AsTask());
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.False(canceled.WasDisposed);
        using var success = new ObservedInput(data, true, false);
        Assert.Equal(17, (await Run(success, true, options)).Width); Assert.Equal(0, root.Bytes);
        // Asking for facts after the pixel payload retains the larger scan workspace.
        using var complete = new ObservedInput(data, true, false);
        var fullFailure = await Assert.ThrowsAsync<ImageLimitException>(() => Run(complete, true,
            options with { Facts = ImageProbeFacts.All }).AsTask());
        Assert.Equal(ImageLimitScope.Operation, fullFailure.Scope);
        Assert.Equal(0, complete.BytesRead); Assert.Equal(0, root.Bytes);
    }

    [Fact]
    public void InspectionDoesNotAdmitAnUnallocatedRasterAndStartsAtCurrentPosition()
    {
        var data = OriginalExif.Create(MetadataProfile.Png8, 17, 11).Encoded;
        BinaryPrimitives.WriteUInt32BigEndian(data.AsSpan(16), 50000);
        BinaryPrimitives.WriteUInt32BigEndian(data.AsSpan(20), 50000); // Probe does not promise CRC validation.
        using var input = new ObservedInput([1, 2, 3, .. data], false, true); input.Position = 3;
        var result = ImageCodec.Probe(input, new() { Limits = new() { MaxDimension = 65535, MaxPixels = 3_000_000_000, MaxWorkingBytes = Workspace } });
        Assert.Equal((50000, 50000), (result.Width, result.Height));
        Assert.Equal(29, result.BytesRead); Assert.Equal(32, input.Position); Assert.Equal(29, result.BytesTraversed);
    }

    private static ValueTask<ImageProbeResult> Run(Stream input, bool asynchronous, ImageProbeOptions? options = null)
        => asynchronous ? ImageCodec.ProbeAsync(input, options) : ValueTask.FromResult(ImageCodec.Probe(input, options));

    private static byte[] Encode(ImageBuffer image, ImageFormat format)
    {
        using var output = new MemoryStream();
        if (format == ImageFormat.Png) ImageCodec.EncodePng(image, output);
        else if (format == ImageFormat.Jpeg) ImageCodec.EncodeJpeg(image, output);
        else ImageCodec.EncodeWebp(image, output, new() { Lossless = true, Effort = 0 });
        return output.ToArray();
    }

    private sealed class ObservedInput(byte[] data, bool asynchronous, bool seekable, int chunk = int.MaxValue, bool mixed = false) : Stream
    {
        private int offset;
        public int Offset => offset;
        public long BytesRead;
        public int Seeks;
        public bool WasDisposed;
        public int ImmediateReads, PendingReads;
        public Action? DuringAsyncRead;
        private int ReadCore(Span<byte> target)
        {
            var count = Math.Min(Math.Min(target.Length, chunk), data.Length - offset);
            data.AsSpan(offset, count).CopyTo(target); offset += count; BytesRead += count; return count;
        }
        public override int Read(Span<byte> buffer)
        {
            Assert.False(asynchronous); return ReadCore(buffer);
        }
        public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            Assert.True(asynchronous);
            if (mixed && ImmediateReads == PendingReads) ImmediateReads++;
            else { PendingReads++; await Task.Yield(); }
            DuringAsyncRead?.Invoke();
            cancellationToken.ThrowIfCancellationRequested(); return ReadCore(buffer.Span);
        }
        public override long Seek(long value, SeekOrigin origin)
        {
            Assert.True(seekable); Seeks++;
            offset = checked((int)(origin switch { SeekOrigin.Begin => value, SeekOrigin.Current => offset + value, _ => data.Length + value }));
            return offset;
        }
        public override long Position { get => seekable ? offset : throw new NotSupportedException(); set => Seek(value, SeekOrigin.Begin); }
        public override long Length => seekable ? data.Length : throw new NotSupportedException();
        public override bool CanSeek => seekable;
        public override bool CanRead => !WasDisposed;
        public override bool CanWrite => false;
        protected override void Dispose(bool disposing) { WasDisposed = true; base.Dispose(disposing); }
        public override void Flush() => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
