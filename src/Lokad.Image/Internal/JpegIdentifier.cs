using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace Lokad.Image.Internal;

// Complete metadata-only JPEG scan, preserving the scalar decoder preflight.
// Marker and scan-header grammar follows ITU-T T.81 annex B. Entropy symbols
// are opaque; only marker framing/stuffing and complete EOI/EOF are inspected.
internal static class JpegIdentifier
{
    public static ImageInfo Identify(ReadOnlySpan<byte> data, OperationContext context)
    {
        int position = 2, items = 0, metadata = 0, width = 0, height = 0;
        bool frame = false, scan = false, exif = false, scanSeen = false;
        var orientation = ImageOrientation.Normal;
        while (position < data.Length)
        {
            context.Work(1);
            if (scan && data[position] != 255)
            {
                position++; // The first byte was admitted by Work(1) above.
                // Admit the search extent from the remaining logical budget.
                // Each search is bounded to 4 KiB for cancellation responsiveness;
                // charge only the non-marker bytes actually consumed. A marker's
                // own Work(1), length and validation stay in their original order.
                var count = (int)Math.Min(context.RemainingWork, Math.Min(4096, data.Length - position));
                if (count != 0)
                {
                    if (!context.Cancellation.CanBeCanceled)
                    {
                        var consumed = EntropyRun(data.Slice(position, count), out var work);
                        context.CommitAdmittedWork(work); position += consumed;
                        continue;
                    }
                    var nextMarker = data.Slice(position, count).IndexOf((byte)255);
                    var plain = nextMarker < 0 ? count : nextMarker;
                    context.CommitAdmittedWork(plain); position += plain;
                }
                continue;
            }
            if (data[position++] != 255) throw new ImageDataException("Invalid JPEG marker.");
            while (position < data.Length && data[position] == 255) { position++; context.Work(1); }
            if (position == data.Length) break;
            var marker = data[position++];
            if (scan && marker == 0) continue;
            if (++items > context.Limits.MaxContainerItems) throw new ImageLimitException(ImageLimitKind.ContainerItems, "JPEG marker count limit exceeded.");
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
                if (marker is not (0xC0 or 0xC2)) throw new UnsupportedImageException(UnsupportedImageFeature.CodingProfile, "This JPEG coding profile is not supported.", ImageFormat.Jpeg);
                if (frame || payload.Length < 6) throw new ImageDataException("Invalid JPEG frame header.");
                if (payload[0] != 8) throw new UnsupportedImageException(UnsupportedImageFeature.BitDepth, "Only 8-bit JPEG is supported.", ImageFormat.Jpeg);
                height = BinaryPrimitives.ReadUInt16BigEndian(payload[1..]);
                width = BinaryPrimitives.ReadUInt16BigEndian(payload[3..]);
                var components = payload[5];
                if (components is not (1 or 3)) throw new UnsupportedImageException(UnsupportedImageFeature.ComponentLayout, "JPEG component layout is not supported.", ImageFormat.Jpeg);
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
                    throw new ImageLimitException(ImageLimitKind.MetadataBytes, "JPEG metadata limit exceeded.");
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

    private static int EntropyRun(ReadOnlySpan<byte> data, out int work)
    {
        // T.81 B.1.1.5: FF00 is one opaque entropy byte; only FF is charged.
        // The caller admits the entire bounded raw span without cancellation.
        // Leave every actual marker and any uncertain final FF to the parser.
        var position = 0; work = 0;
        if (Vector512.IsHardwareAccelerated)
        {
            var ff = Vector512.Create((byte)255);
            for (; position <= data.Length - 64;)
            {
                var value = Vector512.LoadUnsafe(ref MemoryMarshal.GetReference(data), (nuint)position);
                var markers = Vector512.ExtractMostSignificantBits(Vector512.Equals(value, ff));
                if (markers == 0) { position += 64; work += 64; continue; }
                var zeros = Vector512.ExtractMostSignificantBits(Vector512.Equals(value, Vector512<byte>.Zero));
                var actual = markers & ~(zeros >> 1);
                var cross = (markers >> 63) != 0 && position + 64 < data.Length && data[position + 64] == 0;
                if (cross) actual &= 0x7fffffffffffffff;
                if (actual != 0)
                {
                    var plain = BitOperations.TrailingZeroCount(actual);
                    work += plain - BitOperations.PopCount(markers & ((1UL << plain) - 1));
                    return position + plain;
                }
                var consumed = cross ? 65 : 64;
                work += consumed - BitOperations.PopCount(markers); position += consumed;
            }
        }
        while (position < data.Length)
        {
            var next = data[position..].IndexOf((byte)255);
            if (next < 0) { work += data.Length - position; return data.Length; }
            work += next; position += next;
            if (position == data.Length - 1 || data[position + 1] != 0) return position;
            position += 2; work++;
        }
        return position;
    }

}
