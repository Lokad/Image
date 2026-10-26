using System.Buffers.Binary;

namespace Lokad.Image.Internal;

// Independent streaming header grammar: PNG3 chunks, T.81 marker framing,
// RFC9649 RIFF/VP8 headers and CIPA Exif IFD0. Pixel syntax and PNG CRCs are opaque.
internal static class StreamInspection
{
    internal static async ValueTask<ImageInfo> InspectAsync(InspectionReader r)
    {
        await r.ReadAsync(2).ConfigureAwait(false);
        if (r.Header[0] == 137 && r.Header[1] == 80)
        {
            await r.ReadAsync(6).ConfigureAwait(false);
            if (!r.Header.AsSpan(0, 6).SequenceEqual(PngCodec.Signature[2..])) throw new ImageDataException("Invalid PNG signature.");
            return await PngAsync(r).ConfigureAwait(false);
        }
        if (r.Header[0] == 255 && r.Header[1] == 216) return await JpegAsync(r).ConfigureAwait(false);
        if (r.Header[0] == 'R' && r.Header[1] == 'I')
        {
            await r.ReadAsync(10).ConfigureAwait(false);
            if (!r.Header.AsSpan(0, 2).SequenceEqual("FF"u8) || !r.Header.AsSpan(6, 4).SequenceEqual("WEBP"u8)) throw new ImageDataException("Invalid WebP RIFF header.");
            return await WebpAsync(r, BinaryPrimitives.ReadUInt32LittleEndian(r.Header.AsSpan(2)) + 8L).ConfigureAwait(false);
        }
        throw new UnsupportedImageException(UnsupportedImageFeature.Format, "Expected a still PNG, JPEG or WebP image.");
    }

    private static uint Four(ReadOnlySpan<byte> bytes) => BinaryPrimitives.ReadUInt32LittleEndian(bytes);
    private static ushort U16(ReadOnlySpan<byte> bytes, bool little) => little ? BinaryPrimitives.ReadUInt16LittleEndian(bytes) : BinaryPrimitives.ReadUInt16BigEndian(bytes);
    private static uint U32(ReadOnlySpan<byte> bytes, bool little) => little ? BinaryPrimitives.ReadUInt32LittleEndian(bytes) : BinaryPrimitives.ReadUInt32BigEndian(bytes);
    private static int U24(ReadOnlySpan<byte> bytes) => bytes[0] | bytes[1] << 8 | bytes[2] << 16;
    private static void Item(InspectionReader r, ref int items)
    {
        r.Context.Cancellation.ThrowIfCancellationRequested();
        if (++items > r.Context.Limits.MaxContainerItems) throw new ImageLimitException(ImageLimitKind.ContainerItems, "Image container item limit exceeded.");
    }
    private static void Metadata(InspectionReader r, long bytes, ref long total)
    {
        if (bytes > r.Context.Limits.MaxMetadataBytes - total) throw new ImageLimitException(ImageLimitKind.MetadataBytes, "Image metadata limit exceeded.");
        total += bytes;
    }
    private static void Dimensions(InspectionReader r, int width, int height)
    {
        if (width < 1 || height < 1) throw new ImageDataException("Invalid image dimensions.");
        if (width > r.Context.Limits.MaxDimension || height > r.Context.Limits.MaxDimension)
            throw new ImageLimitException(ImageLimitKind.Dimension, "Image dimensions exceed the configured limit.");
        if ((long)width * height > r.Context.Limits.MaxPixels)
            throw new ImageLimitException(ImageLimitKind.PixelCount, "Image pixel count exceeds the configured limit.");
        // No decoded array exists here, so no hypothetical RGBA array ceiling.
    }
    private static void PngName(ReadOnlySpan<byte> type)
    {
        foreach (var b in type) if (b is not (>= 65 and <= 90) and not (>= 97 and <= 122)) throw new ImageDataException("Invalid PNG chunk name.");
    }

    private static async ValueTask<ImageInfo> PngAsync(InspectionReader r)
    {
        int items = 0, width = 0, height = 0, depth = 0, color = 0, palette = 0;
        long metadata = 0, compressed = 0;
        bool header = false, data = false, closed = false, transparency = false, exif = false;
        var orientation = ImageOrientation.Normal;
        while (true)
        {
            Item(r, ref items); await r.ReadAsync(8).ConfigureAwait(false);
            var size = BinaryPrimitives.ReadUInt32BigEndian(r.Header); var type = Four(r.Header.AsSpan(4)); PngName(r.Header.AsSpan(4, 4));
            if (size > int.MaxValue) throw new ImageDataException("Invalid PNG chunk length.");
            var end = r.Position + size; r.DeclaredEnd(end + 4);
            if (!header && type != Four("IHDR"u8)) throw new ImageDataException("PNG must start with IHDR.");
            if (data && type != Four("IDAT"u8)) closed = true;
            if (type == Four("IHDR"u8))
            {
                if (header || size != 13) throw new ImageDataException("Invalid PNG IHDR.");
                await r.ReadAsync(13).ConfigureAwait(false);
                var w = BinaryPrimitives.ReadUInt32BigEndian(r.Header); var h = BinaryPrimitives.ReadUInt32BigEndian(r.Header.AsSpan(4));
                if (w is 0 or > int.MaxValue || h is 0 or > int.MaxValue) throw new ImageDataException("Invalid PNG dimensions.");
                width = (int)w; height = (int)h; depth = r.Header[8]; color = r.Header[9];
                var valid = color switch { 0 => depth is 1 or 2 or 4 or 8 or 16, 2 or 4 or 6 => depth is 8 or 16, 3 => depth is 1 or 2 or 4 or 8, _ => false };
                if (!valid || r.Header[10] != 0 || r.Header[11] != 0 || r.Header[12] > 1) throw new ImageDataException("Invalid PNG coding profile.");
                Dimensions(r, width, height); header = true;
            }
            else if (type == Four("PLTE"u8))
            {
                if (palette != 0 || data || transparency || color is 0 or 4 || size is < 3 or > 768 || size % 3 != 0 || color == 3 && size / 3 > 1 << depth)
                    throw new ImageDataException("Invalid PNG palette.");
                palette = (int)size;
            }
            else if (type == Four("tRNS"u8))
            {
                if (transparency || data || color == 3 && (palette == 0 || size == 0 || size > palette / 3)
                    || color == 0 && size != 2 || color == 2 && size != 6 || color is 4 or 6) throw new ImageDataException("Invalid PNG transparency chunk.");
                transparency = true;
                if (color is 0 or 2)
                {
                    await r.ReadAsync((int)size).ConfigureAwait(false);
                    for (var i = 0; i < size; i += 2) if (BinaryPrimitives.ReadUInt16BigEndian(r.Header.AsSpan(i)) > (1 << depth) - 1) throw new ImageDataException("PNG transparency sample exceeds bit depth.");
                }
            }
            else if (type == Four("IDAT"u8))
            {
                if (closed || color == 3 && palette == 0) throw new ImageDataException("Invalid PNG IDAT ordering.");
                data = true; compressed += size;
            }
            else if (type == Four("IEND"u8))
            {
                if (size != 0 || !data || compressed == 0) throw new ImageDataException("Invalid PNG end.");
                await r.SkipAsync(4).ConfigureAwait(false); await r.EndAsync().ConfigureAwait(false);
                return ImageSourceMetadata.Png(width, height, depth, color, transparency, orientation);
            }
            else if (type == Four("acTL"u8) || type == Four("fcTL"u8) || type == Four("fdAT"u8))
                throw new UnsupportedImageException(UnsupportedImageFeature.Animation, "Animated PNG is not supported.", ImageFormat.Png);
            else
            {
                if ((type & 32) == 0) throw new UnsupportedImageException(UnsupportedImageFeature.CriticalChunk, "Unknown critical PNG chunk.", ImageFormat.Png);
                Metadata(r, size, ref metadata);
                if (type == Four("eXIf"u8))
                {
                    if (exif || data) throw new ImageDataException("Duplicate or misplaced PNG EXIF chunk.");
                    orientation = await ExifAsync(r, size, true).ConfigureAwait(false); exif = true;
                }
            }
            await r.SkipAsync(end - r.Position).ConfigureAwait(false);
            await r.SkipAsync(4).ConfigureAwait(false); // CRC bytes are intentionally opaque.
        }
    }

    private static async ValueTask<ImageInfo> JpegAsync(InspectionReader r)
    {
        int items = 0, width = 0, height = 0; long metadata = 0;
        bool frame = false, scan = false, scanSeen = false, exif = false;
        var orientation = ImageOrientation.Normal;
        var source = new JpegSourceMetadata();
        while (true)
        {
            var marker = await r.JpegMarkerAsync(scan).ConfigureAwait(false);
            if (scan && marker == 0) continue;
            Item(r, ref items);
            if (marker is >= 0xD0 and <= 0xD7) { if (!scan) throw new ImageDataException("JPEG restart marker outside a scan."); continue; }
            scan = false;
            if (marker == 0xD9)
            {
                if (!frame || !scanSeen) throw new ImageDataException("Invalid JPEG end.");
                await r.EndAsync().ConfigureAwait(false);
                return source.Info(width, height, orientation);
            }
            if (marker is 0 or 0xD8 or 1) throw new ImageDataException("Invalid JPEG marker sequence.");
            await r.ReadAsync(2).ConfigureAwait(false); var length = BinaryPrimitives.ReadUInt16BigEndian(r.Header);
            if (length < 2) throw new ImageDataException("Invalid JPEG segment length.");
            var size = length - 2; var end = r.Position + size; r.DeclaredEnd(end);
            if (marker is >= 0xC0 and <= 0xCF && marker is not (0xC4 or 0xC8 or 0xCC))
            {
                if (marker is not (0xC0 or 0xC2)) throw new UnsupportedImageException(UnsupportedImageFeature.CodingProfile, "This JPEG coding profile is not supported.", ImageFormat.Jpeg);
                if (frame || size < 6) throw new ImageDataException("Invalid JPEG frame header.");
                await r.ReadAsync(6).ConfigureAwait(false);
                if (r.Header[0] != 8) throw new UnsupportedImageException(UnsupportedImageFeature.BitDepth, "Only 8-bit JPEG is supported.", ImageFormat.Jpeg);
                height = BinaryPrimitives.ReadUInt16BigEndian(r.Header.AsSpan(1)); width = BinaryPrimitives.ReadUInt16BigEndian(r.Header.AsSpan(3)); var count = r.Header[5];
                if (count is not (1 or 3 or 4)) throw new UnsupportedImageException(UnsupportedImageFeature.ComponentLayout, "JPEG component layout is not supported.", ImageFormat.Jpeg);
                if (size != 6 + count * 3) throw new ImageDataException("Invalid JPEG frame component count.");
                Dimensions(r, width, height); frame = true;
                await r.ReadAsync(count * 3).ConfigureAwait(false);
                source.Frame(count, r.Header);
            }
            else if (marker == 0xDA)
            {
                if (!frame || size < 4) throw new ImageDataException("Invalid JPEG scan header.");
                await r.ReadAsync(1).ConfigureAwait(false);
                if (r.Header[0] is 0 or > 4 || size != 4 + 2 * r.Header[0]) throw new ImageDataException("Invalid JPEG scan component count.");
                scan = scanSeen = true;
            }
            else if (marker is >= 0xE0 and <= 0xEF || marker == 0xFE)
            {
                Metadata(r, size, ref metadata);
                if (marker == 0xE0 && size >= 5 || marker == 0xEE && size >= 12)
                {
                    var prefixLength = marker == 0xE0 ? 5 : 12;
                    await r.ReadAsync(prefixLength).ConfigureAwait(false);
                    source.Application(marker, r.Header.AsSpan(0, prefixLength));
                }
                if (marker == 0xE1 && size >= 6)
                {
                    await r.ReadAsync(6).ConfigureAwait(false);
                    if (r.Header.AsSpan(0, 6).SequenceEqual("Exif\0\0"u8))
                    {
                        if (exif) throw new ImageDataException("Duplicate JPEG EXIF.");
                        orientation = await ExifAsync(r, size - 6, false).ConfigureAwait(false); exif = true;
                    }
                }
            }
            await r.SkipAsync(end - r.Position).ConfigureAwait(false);
        }
    }

    private static async ValueTask<ImageInfo> WebpAsync(InspectionReader r, long end)
    {
        if (end < 12) throw new ImageDataException("Invalid WebP RIFF length."); r.DeclaredEnd(end);
        int items = 0, width = 0, height = 0, canvasW = 0, canvasH = 0; long metadata = 0;
        bool extended = false, pixels = false, alpha = false, alphaChunk = false, lossless = false, exif = false, icc = false, xmp = false;
        byte flags = 0; var orientation = ImageOrientation.Normal;
        while (r.Position < end)
        {
            Item(r, ref items);
            if (end - r.Position < 8) throw new ImageDataException("Truncated WebP chunk.");
            await r.ReadAsync(8).ConfigureAwait(false); var type = Four(r.Header); var size = BinaryPrimitives.ReadUInt32LittleEndian(r.Header.AsSpan(4));
            var payloadEnd = r.Position + size;
            if (size + (long)(size & 1) > end - r.Position) throw new ImageDataException("Invalid WebP chunk length.");
            if (type == Four("VP8X"u8))
            {
                if (items != 1 || size < 10) throw new ImageDataException("Invalid WebP VP8X.");
                await r.ReadAsync(10).ConfigureAwait(false); flags = r.Header[0];
                if ((flags & 2) != 0) throw new UnsupportedImageException(UnsupportedImageFeature.Animation, "Animated WebP is not supported.", ImageFormat.WebP);
                canvasW = U24(r.Header.AsSpan(4)) + 1; canvasH = U24(r.Header.AsSpan(7)) + 1;
                if ((long)canvasW * canvasH > uint.MaxValue) throw new ImageDataException("Invalid WebP canvas pixel count.");
                Dimensions(r, canvasW, canvasH); extended = true; alpha = (flags & 16) != 0;
            }
            else if (type == Four("ANIM"u8) || type == Four("ANMF"u8))
                throw new UnsupportedImageException(UnsupportedImageFeature.Animation, "Animated WebP is not supported.", ImageFormat.WebP);
            else if (type == Four("VP8L"u8))
            {
                if (pixels || alphaChunk || size < 5) throw new ImageDataException("Invalid WebP VP8L header.");
                await r.ReadAsync(5).ConfigureAwait(false);
                if (r.Header[0] != 0x2F) throw new ImageDataException("Invalid WebP VP8L signature.");
                var bits = BinaryPrimitives.ReadUInt32LittleEndian(r.Header.AsSpan(1));
                if (bits >> 29 != 0) throw new UnsupportedImageException(UnsupportedImageFeature.Version, "Unsupported VP8L version.", ImageFormat.WebP);
                width = (int)(bits & 0x3FFF) + 1; height = (int)((bits >> 14) & 0x3FFF) + 1;
                alpha |= (bits & (1 << 28)) != 0; pixels = lossless = true;
            }
            else if (type == Four("VP8 "u8))
            {
                if (pixels || size < 10) throw new ImageDataException("Invalid WebP VP8 header.");
                await r.ReadAsync(10).ConfigureAwait(false);
                if ((r.Header[0] & 1) != 0 || !r.Header.AsSpan(3, 3).SequenceEqual(new byte[] { 0x9D, 1, 0x2A })) throw new ImageDataException("Invalid WebP VP8 keyframe.");
                width = BinaryPrimitives.ReadUInt16LittleEndian(r.Header.AsSpan(6)) & 0x3FFF; height = BinaryPrimitives.ReadUInt16LittleEndian(r.Header.AsSpan(8)) & 0x3FFF;
                if (width == 0 || height == 0) throw new ImageDataException("Invalid VP8 dimensions."); pixels = true;
            }
            else if (type == Four("ALPH"u8))
            {
                if (!extended || pixels || alphaChunk || size == 0 || !alpha) throw new ImageDataException("Invalid WebP alpha chunk."); alphaChunk = true;
            }
            else
            {
                Metadata(r, size, ref metadata);
                if (type == Four("EXIF"u8))
                {
                    if (!extended || exif || (flags & 8) == 0) throw new ImageDataException("Invalid WebP EXIF chunk.");
                    orientation = await ExifAsync(r, size, true).ConfigureAwait(false); exif = true;
                }
                else if (type == Four("ICCP"u8))
                {
                    if (!extended || icc || pixels || (flags & 32) == 0) throw new ImageDataException("Invalid WebP ICC chunk."); icc = true;
                }
                else if (type == Four("XMP "u8))
                {
                    if (!extended || xmp || (flags & 4) == 0) throw new ImageDataException("Invalid WebP XMP chunk."); xmp = true;
                }
            }
            await r.SkipAsync(payloadEnd - r.Position).ConfigureAwait(false);
            if ((size & 1) != 0) { if (await r.ByteAsync().ConfigureAwait(false) != 0) throw new ImageDataException("Invalid WebP padding."); }
        }
        if (!pixels || extended && (width != canvasW || height != canvasH)) throw new ImageDataException("Invalid WebP canvas geometry.");
        if (extended && (((flags & 8) != 0) != exif || ((flags & 32) != 0) != icc || ((flags & 4) != 0) != xmp || !lossless && alpha != alphaChunk)) throw new ImageDataException("WebP feature flags disagree with chunks.");
        Dimensions(r, width, height); await r.EndAsync().ConfigureAwait(false);
        return ImageSourceMetadata.Webp(width, height, alpha, lossless, alphaChunk, orientation);
    }

    private static async ValueTask<ImageOrientation> ExifAsync(InspectionReader r, long length, bool optionalPrefix)
    {
        var end = r.Position + length;
        if (length < 8) throw new ImageDataException("Truncated EXIF header.");
        await r.ReadAsync(8).ConfigureAwait(false);
        bool little; uint offset;
        if (optionalPrefix && r.Header.AsSpan(0, 6).SequenceEqual("Exif\0\0"u8))
        {
            if (length < 14) throw new ImageDataException("Truncated EXIF header.");
            little = ByteOrder(r.Header.AsSpan(6, 2));
            await r.ReadAsync(6).ConfigureAwait(false);
            if (U16(r.Header, little) != 42) throw new ImageDataException("Invalid TIFF identifier.");
            offset = U32(r.Header.AsSpan(2), little);
        }
        else
        {
            little = ByteOrder(r.Header.AsSpan(0, 2));
            if (U16(r.Header.AsSpan(2), little) != 42) throw new ImageDataException("Invalid TIFF identifier.");
            offset = U32(r.Header.AsSpan(4), little);
        }
        if (offset < 8 || offset - 8L > end - r.Position - 2) throw new ImageDataException("Invalid EXIF directory offset.");
        await r.SkipAsync(offset - 8L).ConfigureAwait(false); await r.ReadAsync(2).ConfigureAwait(false);
        var count = U16(r.Header, little);
        if (count * 12L + 4 > end - r.Position) throw new ImageDataException("Truncated EXIF directory.");
        if (count > r.Context.Limits.MaxContainerItems) throw new ImageLimitException(ImageLimitKind.ContainerItems, "EXIF directory entry count limit exceeded.");
        var found = false; var orientation = ImageOrientation.Normal;
        for (var i = 0; i < count; i++)
        {
            r.Context.Cancellation.ThrowIfCancellationRequested(); await r.ReadAsync(12).ConfigureAwait(false);
            if (U16(r.Header, little) != 0x0112) continue;
            if (found || U16(r.Header.AsSpan(2), little) != 3 || U32(r.Header.AsSpan(4), little) != 1) throw new ImageDataException("Invalid EXIF orientation field.");
            var value = U16(r.Header.AsSpan(8), little); if (value is < 1 or > 8) throw new ImageDataException("Invalid EXIF orientation value.");
            orientation = (ImageOrientation)value; found = true;
        }
        await r.SkipAsync(end - r.Position).ConfigureAwait(false);
        return orientation;
    }

    private static bool ByteOrder(ReadOnlySpan<byte> bytes)
        => bytes.SequenceEqual("II"u8) ? true : bytes.SequenceEqual("MM"u8) ? false : throw new ImageDataException("Invalid EXIF byte order.");
}
