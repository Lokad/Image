using System.Buffers.Binary;

namespace Lokad.Image.Tooling;

// Development-only original inputs; this file is never a production build input.
public enum MetadataProfile { Png8, Png16, JpegBaseline420, JpegProgressive420, WebpLosslessAlpha, WebpLossyAlpha }

internal sealed record MetadataFixture(byte[] Encoded, byte[]? Source, int Width, int Height, MetadataProfile Profile, bool? AlphaPresent = null)
{
    public int Depth => Profile == MetadataProfile.Png16 ? 16 : 8;
    public bool Jpeg => Profile is MetadataProfile.JpegBaseline420 or MetadataProfile.JpegProgressive420;
    public bool Lossy => Jpeg || Profile == MetadataProfile.WebpLossyAlpha;
    public bool Alpha => AlphaPresent ?? !Jpeg;
    public ImageFormat Format => Jpeg ? ImageFormat.Jpeg : Profile is MetadataProfile.Png8 or MetadataProfile.Png16 ? ImageFormat.Png : ImageFormat.WebP;
    public string Extension => Jpeg ? ".jpg" : Format == ImageFormat.Png ? ".png" : ".webp";
}

internal static class OriginalExif
{
    public static MetadataFixture Create(MetadataProfile profile, int width, int height, int phase = 0)
    {
        if (!Enum.IsDefined(profile) || width < 1 || height < 1) throw new ArgumentException("Invalid original metadata profile or geometry.");
        if (profile == MetadataProfile.JpegProgressive420)
            return new(AuthoredJpeg.ProgressiveFrame(width, height, JpegSampling.YCbCr420, 4), null, width, height, profile);
        var depth = profile == MetadataProfile.Png16 ? 16 : 8;
        var jpeg = profile == MetadataProfile.JpegBaseline420;
        var source = OriginalPixels.Generate(width, height, depth, opaque: jpeg, phase: phase);
        // Retain distinctive RGB at exactly zero alpha, at either source precision.
        if (!jpeg) source.AsSpan(3 * (depth / 8), depth / 8).Clear();
        using var image = ImageBuffer.Create(width, height, depth);
        for (var y = 0; y < height; y++)
        {
            if (depth == 8) source.AsSpan(y * width * 4, width * 4).CopyTo(image.GetRow8(y));
            else for (var sample = 0; sample < width * 4; sample++)
                image.GetRow16(y)[sample] = BinaryPrimitives.ReadUInt16LittleEndian(source.AsSpan((y * width * 4 + sample) * 2));
        }
        using var output = new MemoryStream();
        if (jpeg) ImageCodec.EncodeJpeg(image, output, new JpegEncodeOptions { Quality = 85, Sampling = JpegSampling.YCbCr420 });
        else if (profile is MetadataProfile.Png8 or MetadataProfile.Png16) ImageCodec.EncodePng(image, output);
        else ImageCodec.EncodeWebp(image, output, new WebpEncodeOptions { Lossless = profile == MetadataProfile.WebpLosslessAlpha, Quality = 85, Effort = 4 });
        return new(output.ToArray(), source, width, height, profile);
    }

    // CIPA Exif/TIFF: byte order, magic 42, first IFD at 8, one SHORT tag 0x0112,
    // count 1 with its inline value, and a zero next-IFD offset. Optional unrelated
    // inline LONG entries precede orientation to exercise bounded IFD traversal.
    // https://www.cipa.jp/std/documents/e/DC-X008-Translation-2019-E.pdf
    public static byte[] Attach(MetadataFixture fixture, ImageOrientation orientation, bool bigEndian, int directoryEntries = 1)
    {
        if ((int)orientation is < 1 or > 8) throw new ArgumentOutOfRangeException(nameof(orientation));
        if (directoryEntries is < 1 or > ushort.MaxValue) throw new ArgumentOutOfRangeException(nameof(directoryEntries));
        var tiff = new byte[14 + directoryEntries * 12]; tiff[0] = tiff[1] = bigEndian ? (byte)'M' : (byte)'I';
        U16(tiff.AsSpan(2), 42, bigEndian); U32(tiff.AsSpan(4), 8, bigEndian);
        U16(tiff.AsSpan(8), (ushort)directoryEntries, bigEndian);
        for (var i = 0; i < directoryEntries - 1; i++)
        {
            var entry = tiff.AsSpan(10 + i * 12, 12);
            U16(entry, (ushort)(0xC000 + i % 0x4000), bigEndian); U16(entry[2..], 4, bigEndian);
            U32(entry[4..], 1, bigEndian); U32(entry[8..], uint.MaxValue, bigEndian);
        }
        var orientationEntry = tiff.AsSpan(10 + (directoryEntries - 1) * 12, 12);
        U16(orientationEntry, 0x0112, bigEndian); U16(orientationEntry[2..], 3, bigEndian);
        U32(orientationEntry[4..], 1, bigEndian); U16(orientationEntry[8..], (ushort)orientation, bigEndian);
        using var output = new MemoryStream();
        if (fixture.Format == ImageFormat.Jpeg)
        {
            if (tiff.Length + 8 > ushort.MaxValue) throw new ArgumentOutOfRangeException(nameof(directoryEntries));
            var segmentLength = tiff.Length + 8;
            output.Write(fixture.Encoded.AsSpan(0, 2));
            output.Write(new byte[] { 255, 225, (byte)(segmentLength >> 8), (byte)segmentLength });
            output.Write("Exif\0\0"u8); output.Write(tiff); output.Write(fixture.Encoded.AsSpan(2));
        }
        else if (fixture.Format == ImageFormat.Png)
        {
            // W3C PNG third edition, section 11.3.4.5: eXIf contains raw TIFF,
            // without the JPEG APP1 identifier, and precedes IDAT.
            // https://www.w3.org/TR/png-3/#eXIf
            output.Write(fixture.Encoded.AsSpan(0, 33));
            var chunk = new byte[12 + tiff.Length];
            BinaryPrimitives.WriteInt32BigEndian(chunk, tiff.Length); "eXIf"u8.CopyTo(chunk.AsSpan(4)); tiff.CopyTo(chunk, 8);
            uint crc = uint.MaxValue;
            foreach (var value in chunk.AsSpan(4, 4 + tiff.Length))
            {
                crc ^= value;
                for (var bit = 0; bit < 8; bit++) crc = (crc >> 1) ^ ((crc & 1) == 0 ? 0u : 0xEDB88320u);
            }
            BinaryPrimitives.WriteUInt32BigEndian(chunk.AsSpan(8 + tiff.Length), ~crc);
            output.Write(chunk); output.Write(fixture.Encoded.AsSpan(33));
        }
        else
        {
            // RFC 9649 / WebP container: extended canvas and EXIF feature bit.
            // https://developers.google.com/speed/webp/docs/riff_container
            output.Write(fixture.Encoded.AsSpan(0, 12));
            if (fixture.Encoded.AsSpan(12, 4).SequenceEqual("VP8X"u8))
            {
                var chunks = fixture.Encoded.AsSpan(12).ToArray(); chunks[8] |= 8; output.Write(chunks);
            }
            else
            {
                var extended = new byte[10]; extended[0] = (byte)(8 | (fixture.Alpha ? 16 : 0));
                Write24(extended.AsSpan(4), fixture.Width - 1); Write24(extended.AsSpan(7), fixture.Height - 1);
                RiffChunk(output, "VP8X"u8, extended); output.Write(fixture.Encoded.AsSpan(12));
            }
            RiffChunk(output, "EXIF"u8, tiff);
        }
        var encoded = output.ToArray();
        if (fixture.Format == ImageFormat.WebP) BinaryPrimitives.WriteInt32LittleEndian(encoded.AsSpan(4), encoded.Length - 8);
        return encoded;
    }

    // Independent forward placement of each coded pixel, from the specification's
    // row/column directions. No production transforms or destination-to-source code.
    public static byte[] Permute(byte[] source, int width, int height, int depth, ImageOrientation orientation)
    {
        if ((int)orientation is < 1 or > 8 || depth is not (8 or 16)
            || source.Length != checked(width * height * 4 * (depth / 8))) throw new ArgumentException("Invalid orientation raster.");
        var stride = 4 * (depth / 8); var outputWidth = (int)orientation >= 5 ? height : width;
        var result = new byte[source.Length];
        for (var sy = 0; sy < height; sy++)
        for (var sx = 0; sx < width; sx++)
        {
            var (dx, dy) = orientation switch
            {
                ImageOrientation.Normal => (sx, sy), ImageOrientation.MirrorHorizontal => (width - 1 - sx, sy),
                ImageOrientation.Rotate180 => (width - 1 - sx, height - 1 - sy), ImageOrientation.MirrorVertical => (sx, height - 1 - sy),
                ImageOrientation.Transpose => (sy, sx), ImageOrientation.Rotate90 => (height - 1 - sy, sx),
                ImageOrientation.Transverse => (height - 1 - sy, width - 1 - sx), ImageOrientation.Rotate270 => (sy, width - 1 - sx),
                _ => throw new ArgumentOutOfRangeException(nameof(orientation))
            };
            source.AsSpan((sy * width + sx) * stride, stride).CopyTo(result.AsSpan((dy * outputWidth + dx) * stride));
        }
        return result;
    }

    private static void U16(Span<byte> bytes, ushort value, bool bigEndian)
    { if (bigEndian) BinaryPrimitives.WriteUInt16BigEndian(bytes, value); else BinaryPrimitives.WriteUInt16LittleEndian(bytes, value); }
    private static void U32(Span<byte> bytes, uint value, bool bigEndian)
    { if (bigEndian) BinaryPrimitives.WriteUInt32BigEndian(bytes, value); else BinaryPrimitives.WriteUInt32LittleEndian(bytes, value); }
    private static void Write24(Span<byte> bytes, int value)
    { bytes[0] = (byte)value; bytes[1] = (byte)(value >> 8); bytes[2] = (byte)(value >> 16); }
    private static void RiffChunk(Stream output, ReadOnlySpan<byte> type, byte[] payload)
    {
        output.Write(type); Span<byte> size = stackalloc byte[4]; BinaryPrimitives.WriteInt32LittleEndian(size, payload.Length);
        output.Write(size); output.Write(payload); if (payload.Length % 2 != 0) output.WriteByte(0);
    }
}
