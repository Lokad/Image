using System.Buffers.Binary;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class OptionalMetadataPolicyTests
{
    private const ImageMetadataPolicy Permissive = ImageMetadataPolicy.IgnoreInvalid;

    [Theory]
    [InlineData(MetadataProfile.Png8)] [InlineData(MetadataProfile.JpegBaseline420)] [InlineData(MetadataProfile.WebpLosslessAlpha)]
    [InlineData(MetadataProfile.Png16)] [InlineData(MetadataProfile.JpegProgressive420)] [InlineData(MetadataProfile.WebpLossyAlpha)]
    public async Task UnusableExifPreservesPixelsAcrossEveryPublicReadRoute(MetadataProfile profile)
    {
        var fixture = OriginalExif.Create(profile, 17, 11);
        foreach (var corruption in new[] { "offset", "orientation", "byte-order", "short", "directory" })
        {
            var tiff = Tiff(6);
            if (corruption == "offset") BinaryPrimitives.WriteUInt32LittleEndian(tiff.AsSpan(4), uint.MaxValue);
            else if (corruption == "orientation") tiff[18] = 9;
            else if (corruption == "byte-order") tiff[0] = 0;
            else if (corruption == "short") tiff = tiff[..3];
            else tiff = tiff[..^5];
            var data = Attach(fixture, [tiff]);
            Assert.Throws<ImageDataException>(() => ImageCodec.Identify(data));
            Assert.Throws<ImageDataException>(() => ImageCodec.Decode(data));
            using (var strict = new MemoryStream(data))
                await Assert.ThrowsAsync<ImageDataException>(() => ImageCodec.InspectHeadersAsync(strict).AsTask());
            await CheckRoutes(data, fixture, ImageOrientation.Normal);
        }
    }

    [Theory]
    [InlineData(MetadataProfile.Png8)] [InlineData(MetadataProfile.JpegBaseline420)] [InlineData(MetadataProfile.WebpLosslessAlpha)]
    public async Task EveryValidOrientationAndAbsentTagsRemainUsable(MetadataProfile profile)
    {
        var fixture = OriginalExif.Create(profile, 17, 11);
        for (var value = 1; value <= 8; value++)
        {
            var data = OriginalExif.Attach(fixture, (ImageOrientation)value, bigEndian: value % 2 == 0);
            await CheckRoutes(data, fixture, (ImageOrientation)value);
            using var decoded = ImageCodec.Decode(data, new() { ApplyOrientation = true, MetadataPolicy = Permissive });
            using var strict = ImageCodec.Decode(data, new() { ApplyOrientation = true });
            Assert.Equal(OriginalPixels.Bytes(strict), OriginalPixels.Bytes(decoded));
            Assert.Equal(ImageOrientation.Normal, decoded.Orientation);
        }
        var missing = Tiff(6); BinaryPrimitives.WriteUInt16LittleEndian(missing.AsSpan(10), 0xC123);
        await CheckRoutes(Attach(fixture, [missing]), fixture, ImageOrientation.Normal);
        await CheckRoutes(fixture.Encoded, fixture, ImageOrientation.Normal);
    }

    [Theory]
    [InlineData(MetadataProfile.Png8)] [InlineData(MetadataProfile.JpegBaseline420)] [InlineData(MetadataProfile.WebpLosslessAlpha)]
    public async Task LastUsableOrientationWinsAndInvalidOrMissingTagsDoNotEraseIt(MetadataProfile profile)
    {
        var fixture = OriginalExif.Create(profile, 17, 11);
        var invalid = Tiff(9); var missing = Tiff(2); missing[10] = 0x34;
        var data = Attach(fixture, [Tiff(3), Tiff(6), invalid, missing]);
        Assert.Throws<ImageDataException>(() => ImageCodec.Identify(data));
        await CheckRoutes(data, fixture, ImageOrientation.Rotate90);
        var normalLast = Attach(fixture, [Tiff(6), Tiff(1)]);
        await CheckRoutes(normalLast, fixture, ImageOrientation.Normal);

        var duplicateTags = Directory(Tiff(3).AsSpan(10, 12).ToArray(), Tiff(8).AsSpan(10, 12).ToArray());
        var sameChunk = Attach(fixture, [duplicateTags]);
        Assert.Throws<ImageDataException>(() => ImageCodec.Decode(sameChunk));
        await CheckRoutes(sameChunk, fixture, ImageOrientation.Rotate270);
        duplicateTags[30] = 9; // Invalid later entry does not erase the earlier valid Rotate180.
        await CheckRoutes(Attach(fixture, [duplicateTags]), fixture, ImageOrientation.Rotate180);
    }

    [Fact]
    public async Task MisplacedOptionalPngExifIsToleratedWithoutAcceptingBrokenPixelOrdering()
    {
        var fixture = OriginalExif.Create(MetadataProfile.Png8, 17, 11);
        var data = Attach(fixture, [Tiff(6)], late: true);
        Assert.Throws<ImageDataException>(() => ImageCodec.Identify(data));
        await CheckRoutes(data, fixture, ImageOrientation.Rotate90);
        var position = 8;
        while (!fixture.Encoded.AsSpan(position + 4, 4).SequenceEqual("IDAT"u8))
            position += 12 + BinaryPrimitives.ReadInt32BigEndian(fixture.Encoded.AsSpan(position));
        var size = 12 + BinaryPrimitives.ReadInt32BigEndian(fixture.Encoded.AsSpan(position));
        byte[] interrupted = [.. data.AsSpan(0, data.Length - 12), .. fixture.Encoded.AsSpan(position, size), .. data.AsSpan(data.Length - 12)];
        Assert.Throws<ImageDataException>(() => ImageCodec.Identify(interrupted, null, default, Permissive));
        Assert.Throws<ImageDataException>(() => ImageCodec.Decode(interrupted, new() { MetadataPolicy = Permissive }));
        using var input = new MemoryStream(interrupted);
        Assert.Throws<ImageDataException>(() => ImageCodec.InspectHeaders(input, null, default, Permissive));
    }

    [Theory]
    [InlineData(MetadataProfile.Png8)] [InlineData(MetadataProfile.JpegBaseline420)] [InlineData(MetadataProfile.WebpLosslessAlpha)]
    public async Task UnrelatedAliasedTagsAreNotFollowedOrExpanded(MetadataProfile profile)
    {
        var fixture = OriginalExif.Create(profile, 17, 11);
        byte[] alias = new byte[12]; BinaryPrimitives.WriteUInt16LittleEndian(alias, 0x8769);
        BinaryPrimitives.WriteUInt16LittleEndian(alias.AsSpan(2), 4);
        BinaryPrimitives.WriteUInt32LittleEndian(alias.AsSpan(4), uint.MaxValue);
        BinaryPrimitives.WriteUInt32LittleEndian(alias.AsSpan(8), 8); // Self reference, deliberately never dereferenced.
        var data = Attach(fixture, [Directory(alias, alias, Tiff(7).AsSpan(10, 12).ToArray())]);
        Assert.Equal(ImageOrientation.Transverse, ImageCodec.Identify(data).Orientation);
        await CheckRoutes(data, fixture, ImageOrientation.Transverse);
    }

    [Theory]
    [InlineData(MetadataProfile.Png8)] [InlineData(MetadataProfile.JpegBaseline420)] [InlineData(MetadataProfile.WebpLosslessAlpha)]
    public async Task OptionalErrorsNeverHideOuterTruncationOrLimits(MetadataProfile profile)
    {
        var fixture = OriginalExif.Create(profile, 17, 11); var bad = Tiff(9);
        var data = Attach(fixture, [bad]);
        Assert.Throws<ImageDataException>(() => ImageCodec.Identify(data[..^1], null, default, Permissive));
        Assert.Throws<ImageDataException>(() => ImageCodec.Decode(data[..^1], new() { MetadataPolicy = Permissive }));
        foreach (var seekable in new[] { false, true })
        {
            using var truncated = new BorrowedInput(data[..^1], seekable);
            await Assert.ThrowsAsync<ImageDataException>(() => ImageCodec.InspectHeadersAsync(truncated, null, default, Permissive).AsTask());
            Assert.False(truncated.WasDisposed);
            var payload = fixture.Format == ImageFormat.Png ? 41 : fixture.Jpeg ? 12 : data.AsSpan().LastIndexOf("EXIF"u8) + 8;
            var cut = data[..(payload + 3)]; cut[payload] = 0; // Semantically invalid AND truncated outer payload.
            using var outer = new BorrowedInput(cut, seekable);
            await Assert.ThrowsAsync<ImageDataException>(() => ImageCodec.InspectHeadersAsync(outer, null, default, Permissive).AsTask());
        }
        var limits = new ImageLimits { MaxMetadataBytes = 1 };
        var metadata = Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(data, new() { Limits = limits, MetadataPolicy = Permissive }));
        Assert.Equal(ImageLimitKind.MetadataBytes, metadata.Limit);
        using var headers = new BorrowedInput(data, false);
        var streamed = await Assert.ThrowsAsync<ImageLimitException>(() => ImageCodec.InspectHeadersAsync(headers, limits, default, Permissive).AsTask());
        Assert.Equal(ImageLimitKind.MetadataBytes, streamed.Limit);

        var count = Tiff(6); BinaryPrimitives.WriteUInt16LittleEndian(count.AsSpan(8), 65535);
        var hostile = Attach(fixture, [count]);
        var items = Assert.Throws<ImageLimitException>(() => ImageCodec.Identify(hostile, new() { MaxContainerItems = 32 }, default, Permissive));
        Assert.Equal(ImageLimitKind.ContainerItems, items.Limit);
        using var hostileHeaders = new BorrowedInput(hostile, true);
        var directory = await Assert.ThrowsAsync<ImageLimitException>(() => ImageCodec.InspectHeadersAsync(hostileHeaders, new() { MaxContainerItems = 32 }, default, Permissive).AsTask());
        Assert.Equal(ImageLimitKind.ContainerItems, directory.Limit);
    }

    [Fact]
    public async Task PngChecksumPixelCodingAndUnusedCompressedMetadataKeepTheirScopes()
    {
        var fixture = OriginalExif.Create(MetadataProfile.Png8, 17, 11);
        var data = Attach(fixture, [Tiff(9)]); data[33 + 8 + 26] ^= 1;
        Assert.Throws<ImageDataException>(() => ImageCodec.Identify(data, null, default, Permissive));
        Assert.Throws<ImageDataException>(() => ImageCodec.Decode(data, new() { MetadataPolicy = Permissive }));
        using var headers = new MemoryStream(data);
        Assert.Equal(ImageOrientation.Normal, ImageCodec.InspectHeaders(headers, null, default, Permissive).Orientation); // CRC is outside header scope.
        var frame = new AuthoredPng.Frame("optional-unused-compression", 6, 8, 17, 11, false);
        var valid = AuthoredPng.Wrap(frame, AuthoredPng.StoredZlib(AuthoredPng.FilteredRows(frame)),
            ("iCCP", new byte[] { 88, 0, 0, 255 }), ("zTXt", new byte[] { 88, 0, 0, 255 }));
        using var decoded = ImageCodec.Decode(valid, new() { MetadataPolicy = Permissive });
        var pixelsInvalid = AuthoredPng.Wrap(frame, [0xA7]);
        Assert.Throws<ImageDataException>(() => ImageCodec.Decode(pixelsInvalid, new() { MetadataPolicy = Permissive }));
        using var root = new TestMemoryBudget(8448);
        using var input = new BorrowedInput(valid, false);
        await ImageCodec.InspectHeadersAsync(input, new() { MaxWorkingBytes = 8448, MemoryBudget = root }, default, Permissive);
        Assert.Equal(8448, root.Peak); Assert.Equal(0, root.Bytes);
    }

    [Fact]
    public async Task AdmissionAndCancellationAreTerminalAndCleanupAllowsSubsequentReads()
    {
        var fixture = OriginalExif.Create(MetadataProfile.Png8, 17, 11); var data = Attach(fixture, [Tiff(9)]);
        using var root = new TestMemoryBudget(8448); Assert.True(root.TryReserve(1, out var borrowed));
        using var input = new BorrowedInput(data, false);
        var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = 8448 };
        var refusal = await Assert.ThrowsAsync<ImageLimitException>(() => ImageCodec.InspectHeadersAsync(input, limits, default, Permissive).AsTask());
        Assert.Equal(ImageLimitScope.SharedBudget, refusal.Scope); Assert.Equal(1, root.Refusals); Assert.Equal(1, root.Bytes);
        borrowed.Dispose();
        using var cancellation = new CancellationTokenSource();
        using var canceled = new BorrowedInput(data, false)
        { DuringRead = () => { Assert.Equal(8448, root.Bytes); cancellation.Cancel(); } };
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => ImageCodec.InspectHeadersAsync(canceled, limits, cancellation.Token, Permissive).AsTask());
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.False(canceled.WasDisposed);
        using var success = new BorrowedInput(data, false);
        Assert.Equal(ImageOrientation.Normal, (await ImageCodec.InspectHeadersAsync(success, limits, default, Permissive)).Orientation);
        Assert.Equal(0, root.Bytes);
    }

    private static async Task CheckRoutes(byte[] data, MetadataFixture fixture, ImageOrientation expected)
    {
        Assert.Equal(expected, ImageCodec.Identify(data, null, default, Permissive).Orientation);
        using var original = ImageCodec.Decode(fixture.Encoded);
        var pixels = OriginalPixels.Bytes(original);
        using (var decoded = ImageCodec.Decode(data, new() { MetadataPolicy = Permissive }))
        { Assert.Equal(pixels, OriginalPixels.Bytes(decoded)); Assert.Equal(expected, decoded.Orientation); }
        foreach (var seekable in new[] { false, true })
        {
            using var input = new BorrowedInput(data, seekable);
            Assert.Equal(expected, ImageCodec.Identify(input, null, default, Permissive).Orientation); Assert.False(input.WasDisposed);
            using var identifyAsync = new BorrowedInput(data, seekable);
            Assert.Equal(expected, (await ImageCodec.IdentifyAsync(identifyAsync, null, default, Permissive)).Orientation);
            using var headers = new BorrowedInput(data, seekable);
            Assert.Equal(expected, ImageCodec.InspectHeaders(headers, null, default, Permissive).Orientation);
            using var headersAsync = new BorrowedInput(data, seekable);
            Assert.Equal(expected, (await ImageCodec.InspectHeadersAsync(headersAsync, null, default, Permissive)).Orientation);
            using var decode = new BorrowedInput(data, seekable);
            using var decoded = ImageCodec.Decode(decode, new() { MetadataPolicy = Permissive });
            Assert.Equal(pixels, OriginalPixels.Bytes(decoded)); Assert.Equal(expected, decoded.Orientation);
            using var decodeAsync = new BorrowedInput(data, seekable);
            using var decodedAsync = await ImageCodec.DecodeAsync(decodeAsync, new() { MetadataPolicy = Permissive });
            Assert.Equal(pixels, OriginalPixels.Bytes(decodedAsync)); Assert.Equal(expected, decodedAsync.Orientation);
            using var probe = new BorrowedInput(data, seekable);
            Assert.Equal(expected, ImageCodec.Probe(probe, new() { MetadataPolicy = Permissive, Facts = ImageProbeFacts.All }).Orientation);
            using var probeAsync = new BorrowedInput(data, seekable);
            Assert.Equal(expected, (await ImageCodec.ProbeAsync(probeAsync, new() { MetadataPolicy = Permissive, Facts = ImageProbeFacts.All })).Orientation);
        }
        if (fixture.Jpeg)
        {
            var samples = new byte[fixture.Width * fixture.Height * 3];
            Assert.Equal(expected, ImageCodec.DecodeJpeg(data, samples, JpegSampleFormat.Rgb,
                options: new() { MetadataPolicy = Permissive }).Orientation);
        }
    }

    private static byte[] Tiff(int orientation)
    {
        byte[] entry = new byte[12]; BinaryPrimitives.WriteUInt16LittleEndian(entry, 0x0112);
        BinaryPrimitives.WriteUInt16LittleEndian(entry.AsSpan(2), 3); BinaryPrimitives.WriteUInt32LittleEndian(entry.AsSpan(4), 1);
        BinaryPrimitives.WriteUInt16LittleEndian(entry.AsSpan(8), (ushort)orientation); return Directory(entry);
    }

    private static byte[] Directory(params byte[][] entries)
    {
        var data = new byte[14 + entries.Length * 12]; data[0] = data[1] = (byte)'I';
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(2), 42); BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(4), 8);
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(8), (ushort)entries.Length);
        for (var i = 0; i < entries.Length; i++) entries[i].CopyTo(data, 10 + 12 * i);
        return data;
    }

    private static byte[] Attach(MetadataFixture fixture, byte[][] payloads, bool late = false)
    {
        using var output = new MemoryStream();
        if (fixture.Format == ImageFormat.Png)
        {
            var split = late ? fixture.Encoded.Length - 12 : 33; output.Write(fixture.Encoded.AsSpan(0, split));
            foreach (var payload in payloads)
            {
                byte[] header = new byte[8]; BinaryPrimitives.WriteInt32BigEndian(header, payload.Length); "eXIf"u8.CopyTo(header.AsSpan(4));
                output.Write(header); output.Write(payload); uint crc = uint.MaxValue;
                foreach (var value in header.AsSpan(4).ToArray().Concat(payload))
                { crc ^= value; for (var bit = 0; bit < 8; bit++) crc = crc >> 1 ^ ((crc & 1) == 0 ? 0u : 0xEDB88320u); }
                byte[] checksum = new byte[4]; BinaryPrimitives.WriteUInt32BigEndian(checksum, ~crc); output.Write(checksum);
            }
            output.Write(fixture.Encoded.AsSpan(split));
        }
        else if (fixture.Jpeg)
        {
            output.Write(fixture.Encoded.AsSpan(0, 2));
            foreach (var payload in payloads)
            {
                var size = payload.Length + 8; output.Write(new byte[] { 255, 225, (byte)(size >> 8), (byte)size });
                output.Write("Exif\0\0"u8); output.Write(payload);
            }
            output.Write(fixture.Encoded.AsSpan(2));
        }
        else
        {
            var extended = OriginalExif.Attach(fixture, ImageOrientation.Normal, false);
            output.Write(extended.AsSpan(0, extended.Length - 34));
            foreach (var payload in payloads)
            {
                byte[] header = new byte[8]; "EXIF"u8.CopyTo(header); BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(4), payload.Length);
                output.Write(header); output.Write(payload); if ((payload.Length & 1) != 0) output.WriteByte(0);
            }
        }
        var result = output.ToArray();
        if (fixture.Format == ImageFormat.WebP) BinaryPrimitives.WriteInt32LittleEndian(result.AsSpan(4), result.Length - 8);
        return result;
    }

    private sealed class BorrowedInput(byte[] data, bool seekable) : Stream
    {
        private readonly MemoryStream inner = new(data, writable: false);
        public bool WasDisposed;
        public Action? DuringRead;
        public override bool CanSeek => seekable;
        public override bool CanRead => !WasDisposed;
        public override bool CanWrite => false;
        public override long Position { get => seekable ? inner.Position : throw new NotSupportedException(); set => inner.Position = value; }
        public override long Length => seekable ? inner.Length : throw new NotSupportedException();
        public override int Read(Span<byte> target) => inner.Read(target);
        public override int Read(byte[] target, int offset, int count) => inner.Read(target, offset, count);
        public override async ValueTask<int> ReadAsync(Memory<byte> target, CancellationToken cancellationToken = default)
        {
            await Task.Yield(); DuringRead?.Invoke(); return await inner.ReadAsync(target, cancellationToken);
        }
        public override long Seek(long value, SeekOrigin origin) => seekable ? inner.Seek(value, origin) : throw new NotSupportedException();
        protected override void Dispose(bool disposing) { WasDisposed = true; inner.Dispose(); base.Dispose(disposing); }
        public override void Flush() => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] source, int offset, int count) => throw new NotSupportedException();
    }
}
