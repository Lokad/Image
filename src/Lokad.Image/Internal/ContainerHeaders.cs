using System.Buffers.Binary;

namespace Lokad.Image.Internal;

internal static class ContainerHeaders
{
    public static ImageInfo Jpeg(ReadOnlySpan<byte> data, OperationContext context)
    {
        int position = 2, items = 0, metadata = 0, width = 0, height = 0;
        bool frame = false, scan = false, exif = false, scanSeen = false;
        var orientation = ImageOrientation.Normal;
        while (position < data.Length)
        {
            context.Work(1);
            if (scan && data[position] != 255) { position++; continue; }
            if (data[position++] != 255) throw new ImageDataException("Invalid JPEG marker.");
            while (position < data.Length && data[position] == 255) { position++; context.Work(1); }
            if (position == data.Length) break;
            var marker = data[position++];
            if (scan && marker == 0) continue;
            if (++items > context.Limits.MaxContainerItems) throw new ImageLimitException("JPEG marker count limit exceeded.");
            if (marker is >= 0xD0 and <= 0xD7)
            {
                if (!scan) throw new ImageDataException("JPEG restart marker outside a scan.");
                continue;
            }
            scan = false;
            if (marker == 0xD9)
            {
                if (!frame || !scanSeen || position != data.Length) throw new ImageDataException("Invalid JPEG end or trailing bytes.");
                return new ImageInfo(ImageFormat.Jpeg, width, height, 8, false, orientation);
            }
            if (marker is 0 or 0xD8 or 0x01 || position > data.Length - 2)
                throw new ImageDataException("Invalid JPEG marker sequence.");
            var length = BinaryPrimitives.ReadUInt16BigEndian(data[position..]);
            if (length < 2 || length > data.Length - position) throw new ImageDataException("Truncated JPEG segment.");
            var payload = data.Slice(position + 2, length - 2); position += length;
            context.Work(length);
            if (marker is >= 0xC0 and <= 0xCF && marker is not (0xC4 or 0xC8 or 0xCC))
            {
                if (marker is not (0xC0 or 0xC2)) throw new UnsupportedImageException("This JPEG coding profile is not supported.");
                if (frame || payload.Length < 6) throw new ImageDataException("Invalid JPEG frame header.");
                if (payload[0] != 8) throw new UnsupportedImageException("Only 8-bit JPEG is supported.");
                height = BinaryPrimitives.ReadUInt16BigEndian(payload[1..]);
                width = BinaryPrimitives.ReadUInt16BigEndian(payload[3..]);
                var components = payload[5];
                if (components is not (1 or 3)) throw new UnsupportedImageException("JPEG component layout is not supported.");
                if (payload.Length != 6 + components * 3 || width == 0 || height == 0)
                    throw new ImageDataException("Invalid JPEG frame dimensions or components.");
                context.PixelBytes(width, height, 8); frame = true;
            }
            else if (marker == 0xDA)
            {
                if (!frame || payload.Length < 4 || payload[0] == 0 || payload.Length != 4 + 2 * payload[0])
                    throw new ImageDataException("Invalid JPEG scan header.");
                scan = true; scanSeen = true;
            }
            else if (marker is >= 0xE0 and <= 0xEF || marker == 0xFE)
            {
                if (payload.Length > context.Limits.MaxMetadataBytes - metadata)
                    throw new ImageLimitException("JPEG metadata limit exceeded.");
                metadata += payload.Length;
                if (marker == 0xE1 && payload.StartsWith("Exif\0\0"u8))
                {
                    if (exif) throw new ImageDataException("Duplicate JPEG EXIF.");
                    orientation = ExifOrientation.Read(payload, context); exif = true;
                }
            }
        }
        throw new ImageDataException("JPEG is missing EOI.");
    }

    public static ImageInfo WebP(ReadOnlySpan<byte> data, OperationContext context)
    {
        if (data.Length < 12 || !data.StartsWith("RIFF"u8) || !data.Slice(8, 4).SequenceEqual("WEBP"u8)
            || (long)BinaryPrimitives.ReadUInt32LittleEndian(data[4..]) + 8 != data.Length)
            throw new ImageDataException("Invalid WebP RIFF header.");
        int position = 12, items = 0, metadata = 0, width = 0, height = 0, canvasW = 0, canvasH = 0;
        bool extended = false, pixels = false, alpha = false, exif = false, alphaChunk = false;
        bool icc = false, xmp = false, lossless = false;
        byte featureFlags = 0;
        var orientation = ImageOrientation.Normal;
        while (position < data.Length)
        {
            if (++items > context.Limits.MaxContainerItems) throw new ImageLimitException("WebP chunk count limit exceeded.");
            if (data.Length - position < 8) throw new ImageDataException("Truncated WebP chunk.");
            var type = data.Slice(position, 4);
            var size = BinaryPrimitives.ReadUInt32LittleEndian(data[(position + 4)..]);
            var padded = (long)size + (size & 1);
            if (padded > data.Length - position - 8) throw new ImageDataException("Invalid WebP chunk length.");
            var payload = data.Slice(position + 8, (int)size);
            if ((size & 1) != 0 && data[position + 8 + (int)size] != 0) throw new ImageDataException("Invalid WebP padding.");
            context.Work(size + 8L);
            position += 8 + (int)padded;
            if (type.SequenceEqual("VP8X"u8))
            {
                if (items != 1 || size < 10) throw new ImageDataException("Invalid WebP VP8X.");
                // RFC 9649 section 2.7 requires readers to ignore reserved and future fields.
                if ((payload[0] & 2) != 0) throw new UnsupportedImageException("Animated WebP is not supported.");
                canvasW = Read24(payload[4..]) + 1; canvasH = Read24(payload[7..]) + 1;
                if ((long)canvasW * canvasH > uint.MaxValue) throw new ImageDataException("Invalid WebP canvas pixel count.");
                context.PixelBytes(canvasW, canvasH, 8);
                featureFlags = payload[0]; extended = true; alpha = (featureFlags & 16) != 0;
            }
            else if (type.SequenceEqual("ANIM"u8) || type.SequenceEqual("ANMF"u8))
                throw new UnsupportedImageException("Animated WebP is not supported.");
            else if (type.SequenceEqual("VP8L"u8))
            {
                if (pixels || alphaChunk || payload.Length < 5 || payload[0] != 0x2F) throw new ImageDataException("Invalid WebP VP8L header.");
                var bits = BinaryPrimitives.ReadUInt32LittleEndian(payload[1..]);
                if (bits >> 29 != 0) throw new UnsupportedImageException("Unsupported VP8L version.");
                width = (int)(bits & 0x3FFF) + 1; height = (int)((bits >> 14) & 0x3FFF) + 1;
                alpha |= (bits & (1 << 28)) != 0; pixels = true; lossless = true;
            }
            else if (type.SequenceEqual("VP8 "u8))
            {
                if (pixels || payload.Length < 10 || (payload[0] & 1) != 0
                    || !payload.Slice(3, 3).SequenceEqual(new byte[] { 0x9D, 0x01, 0x2A }))
                    throw new ImageDataException("Invalid WebP VP8 keyframe header.");
                width = BinaryPrimitives.ReadUInt16LittleEndian(payload[6..]) & 0x3FFF;
                height = BinaryPrimitives.ReadUInt16LittleEndian(payload[8..]) & 0x3FFF;
                if (width == 0 || height == 0) throw new ImageDataException("Invalid VP8 dimensions.");
                pixels = true;
            }
            else if (type.SequenceEqual("ALPH"u8))
            {
                if (!extended || pixels || alphaChunk || payload.Length == 0 || !alpha) throw new ImageDataException("Invalid WebP alpha chunk.");
                alphaChunk = true;
            }
            else
            {
                if (payload.Length > context.Limits.MaxMetadataBytes - metadata)
                    throw new ImageLimitException("WebP metadata limit exceeded.");
                metadata += payload.Length;
                if (type.SequenceEqual("EXIF"u8))
                {
                    if (!extended || exif || (featureFlags & 8) == 0) throw new ImageDataException("Invalid WebP EXIF chunk.");
                    orientation = ExifOrientation.Read(payload, context); exif = true;
                }
                else if (type.SequenceEqual("ICCP"u8))
                {
                    if (!extended || icc || pixels || (featureFlags & 32) == 0) throw new ImageDataException("Invalid WebP ICC chunk.");
                    icc = true;
                }
                else if (type.SequenceEqual("XMP "u8))
                {
                    if (!extended || xmp || (featureFlags & 4) == 0) throw new ImageDataException("Invalid WebP XMP chunk.");
                    xmp = true;
                }
            }
        }
        if (!pixels || (extended && (width != canvasW || height != canvasH))) throw new ImageDataException("Invalid WebP canvas geometry.");
        if (extended && (((featureFlags & 8) != 0) != exif || ((featureFlags & 32) != 0) != icc
            || ((featureFlags & 4) != 0) != xmp || (!lossless && alpha != alphaChunk)))
            throw new ImageDataException("WebP feature flags disagree with its chunks.");
        context.PixelBytes(width, height, 8);
        return new ImageInfo(ImageFormat.WebP, width, height, 8, alpha, orientation);
    }

    private static int Read24(ReadOnlySpan<byte> bytes) => bytes[0] | bytes[1] << 8 | bytes[2] << 16;
}
