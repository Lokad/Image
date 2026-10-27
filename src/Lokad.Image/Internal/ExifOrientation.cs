using System.Buffers.Binary;

namespace Lokad.Image.Internal;

internal static class ExifOrientation
{
    internal static void ValidatePolicy(ImageMetadataPolicy metadataPolicy)
    {
        if (metadataPolicy is not (ImageMetadataPolicy.Strict or ImageMetadataPolicy.IgnoreInvalid))
            throw new ArgumentOutOfRangeException(nameof(metadataPolicy));
    }

    public static ImageOrientation? Read(ReadOnlySpan<byte> data, OperationContext context,
        ImageMetadataPolicy metadataPolicy = ImageMetadataPolicy.Strict)
    {
        try { return ReadCore(data, context, metadataPolicy); }
        catch (ImageDataException) when (metadataPolicy == ImageMetadataPolicy.IgnoreInvalid)
        {
            // The caller has already bounded the complete optional payload.
            // Limit/cancellation/admission failures are never suppressed.
            return null;
        }
    }

    private static ImageOrientation? ReadCore(ReadOnlySpan<byte> data, OperationContext context, ImageMetadataPolicy metadataPolicy)
    {
        if (data.StartsWith("Exif\0\0"u8)) data = data[6..];
        if (data.Length < 8) throw new ImageDataException("Truncated EXIF header.");
        var little = data.StartsWith("II"u8);
        if (!little && !data.StartsWith("MM"u8)) throw new ImageDataException("Invalid EXIF byte order.");
        if (Read16(data[2..], little) != 42) throw new ImageDataException("Invalid TIFF identifier.");
        var offset = Read32(data[4..], little);
        if (offset < 8 || offset > data.Length - 2) throw new ImageDataException("Invalid EXIF directory offset.");
        var directory = data[(int)offset..];
        var count = Read16(directory, little);
        var truncated = 2L + count * 12L + 4 > directory.Length;
        // Preserve strict malformed-first classification. Permissive handling
        // must not turn an oversized declared directory into ignored metadata.
        if (truncated && metadataPolicy == ImageMetadataPolicy.Strict)
            throw new ImageDataException("Truncated EXIF directory.");
        if (count > context.Limits.MaxContainerItems)
            throw new ImageLimitException(ImageLimitKind.ContainerItems, "EXIF directory entry count limit exceeded.");
        if (truncated) throw new ImageDataException("Truncated EXIF directory.");
        ImageOrientation? orientation = null;
        var found = false;
        for (var i = 0; i < count; i++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var entry = directory.Slice(2 + i * 12, 12);
            if (Read16(entry, little) != 0x0112) continue;
            if (found && metadataPolicy == ImageMetadataPolicy.Strict)
                throw new ImageDataException("Duplicate EXIF orientation field.");
            if (Read16(entry[2..], little) != 3 || Read32(entry[4..], little) != 1)
            {
                if (metadataPolicy == ImageMetadataPolicy.Strict) throw new ImageDataException("Invalid EXIF orientation field.");
                continue;
            }
            var value = Read16(entry[8..], little);
            if (value is < 1 or > 8)
            {
                if (metadataPolicy == ImageMetadataPolicy.Strict) throw new ImageDataException("Invalid EXIF orientation value.");
                continue;
            }
            orientation = (ImageOrientation)value;
            found = true;
        }
        // Only IFD0 orientation is read. Ignore all other tags and directory links:
        // no recursion, thumbnail allocation or external offset dereference.
        return orientation;
    }

    private static ushort Read16(ReadOnlySpan<byte> data, bool little)
        => little ? BinaryPrimitives.ReadUInt16LittleEndian(data) : BinaryPrimitives.ReadUInt16BigEndian(data);
    private static uint Read32(ReadOnlySpan<byte> data, bool little)
        => little ? BinaryPrimitives.ReadUInt32LittleEndian(data) : BinaryPrimitives.ReadUInt32BigEndian(data);
}
