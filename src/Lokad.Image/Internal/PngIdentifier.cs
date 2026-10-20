using System.Buffers.Binary;

namespace Lokad.Image.Internal;

// Metadata-only scan: the pixel decoder's scan/CRC path stays unchanged.
// PNG chunk grammar and CRC: https://www.w3.org/TR/png-3/.
// Keep the same work, metadata and working-byte admission and failure order;
// palette/transparency bytes are charged as before, but need not be retained
// when returning metadata. No intermediate decoder Header is materialized.
internal static class PngIdentifier
{
    public static ImageInfo Identify(ReadOnlySpan<byte> data, OperationContext context)
    {
        if (!data.StartsWith(PngCodec.Signature)) throw new ImageDataException("Invalid PNG signature.");
        int position = 8, items = 0, width = 0, height = 0, color = 0, depth = 0, interlace = 0;
        int compressedLength = 0, metadataBytes = 0;
        var paletteLength = 0;
        bool headerSeen = false, paletteSeen = false, transparencySeen = false, exifSeen = false;
        bool dataSeen = false, dataClosed = false;
        var orientation = ImageOrientation.Normal;
        while (position < data.Length)
        {
            if (++items > context.Limits.MaxContainerItems) throw new ImageLimitException(ImageLimitKind.ContainerItems, "PNG chunk count limit exceeded.");
            if (data.Length - position < 12) throw new ImageDataException("Truncated PNG chunk.");
            var lengthValue = BinaryPrimitives.ReadUInt32BigEndian(data[position..]);
            if (lengthValue > int.MaxValue || lengthValue > data.Length - position - 12)
                throw new ImageDataException("Invalid PNG chunk length.");
            var length = (int)lengthValue;
            var type = data.Slice(position + 4, 4);
            foreach (var value in type)
                if (value is not (>= 65 and <= 90) and not (>= 97 and <= 122))
                    throw new ImageDataException("Invalid PNG chunk name.");
            var payload = data.Slice(position + 8, length);
            context.Work(length + 12L);
            if (PngIdentificationCrc32.Compute(data.Slice(position + 4, length + 4), context.Cancellation)
                != BinaryPrimitives.ReadUInt32BigEndian(data[(position + 8 + length)..]))
                throw new ImageDataException("Invalid PNG chunk CRC.");
            position += length + 12;
            if (!headerSeen && !type.SequenceEqual("IHDR"u8)) throw new ImageDataException("PNG must start with IHDR.");
            if (dataSeen && !type.SequenceEqual("IDAT"u8)) dataClosed = true;
            if (type.SequenceEqual("IHDR"u8))
            {
                if (headerSeen || length != 13) throw new ImageDataException("Invalid PNG IHDR.");
                var w = BinaryPrimitives.ReadUInt32BigEndian(payload);
                var h = BinaryPrimitives.ReadUInt32BigEndian(payload[4..]);
                if (w is 0 or > int.MaxValue || h is 0 or > int.MaxValue)
                    throw new ImageDataException("Invalid PNG dimensions.");
                width = (int)w; height = (int)h; depth = payload[8]; color = payload[9]; interlace = payload[12];
                var validDepth = color switch
                {
                    0 => depth is 1 or 2 or 4 or 8 or 16,
                    2 or 4 or 6 => depth is 8 or 16,
                    3 => depth is 1 or 2 or 4 or 8,
                    _ => false
                };
                if (!validDepth || payload[10] != 0 || payload[11] != 0 || interlace > 1)
                    throw new ImageDataException("Invalid PNG color type, depth or coding method.");
                context.PixelBytes(width, height, depth == 16 ? 16 : 8);
                headerSeen = true;
            }
            else if (type.SequenceEqual("PLTE"u8))
            {
                if (paletteSeen || dataSeen || transparencySeen || color is 0 or 4 || length is < 3 or > 768
                    || length % 3 != 0 || (color == 3 && length / 3 > 1 << depth))
                    throw new ImageDataException("Invalid PNG palette.");
                context.Reserve(length);
                paletteLength = length; paletteSeen = true;
            }
            else if (type.SequenceEqual("tRNS"u8))
            {
                if (transparencySeen || dataSeen || (color == 3 && (!paletteSeen || length == 0 || length > paletteLength / 3))
                    || (color == 0 && length != 2) || (color == 2 && length != 6) || color is 4 or 6)
                    throw new ImageDataException("Invalid PNG transparency chunk.");
                context.Reserve(length);
                transparencySeen = true;
                if (color is 0 or 2)
                    for (var i = 0; i < length; i += 2)
                        if (BinaryPrimitives.ReadUInt16BigEndian(payload[i..]) > ((1 << depth) - 1))
                            throw new ImageDataException("PNG transparency sample exceeds bit depth.");
            }
            else if (type.SequenceEqual("IDAT"u8))
            {
                if (dataClosed || (color == 3 && !paletteSeen)) throw new ImageDataException("Invalid PNG IDAT ordering.");
                dataSeen = true;
                compressedLength = checked(compressedLength + length);
            }
            else if (type.SequenceEqual("IEND"u8))
            {
                if (length != 0 || !dataSeen || compressedLength == 0 || position != data.Length)
                    throw new ImageDataException("Invalid PNG end or trailing data.");
                return ImageSourceMetadata.Png(width, height, depth, color, transparencySeen, orientation);
            }
            else if (type.SequenceEqual("acTL"u8) || type.SequenceEqual("fcTL"u8) || type.SequenceEqual("fdAT"u8))
                throw new UnsupportedImageException(UnsupportedImageFeature.Animation, "Animated PNG is not supported.", ImageFormat.Png);
            else
            {
                if ((type[0] & 32) == 0) throw new UnsupportedImageException(UnsupportedImageFeature.CriticalChunk, "Unknown critical PNG chunk.", ImageFormat.Png);
                if (length > context.Limits.MaxMetadataBytes - metadataBytes)
                    throw new ImageLimitException(ImageLimitKind.MetadataBytes, "PNG metadata limit exceeded.");
                metadataBytes += length;
                if (type.SequenceEqual("eXIf"u8))
                {
                    if (exifSeen || dataSeen) throw new ImageDataException("Duplicate or misplaced PNG EXIF chunk.");
                    orientation = ExifOrientation.Read(payload, context); exifSeen = true;
                }
                // Ancillary payloads are validated structurally and skipped, never inflated.
            }
        }
        throw new ImageDataException("PNG is missing IEND.");
    }

}
