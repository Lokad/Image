using System.Buffers.Binary;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace Lokad.Image.Internal;

internal static class PngCodec
{
    internal static ReadOnlySpan<byte> Signature => [137, 80, 78, 71, 13, 10, 26, 10];
    private sealed record Header(ImageInfo Info, int ColorType, int Depth, int Interlace,
        byte[] Palette, byte[] Transparency, int CompressedLength);

    public static ImageInfo Identify(ReadOnlySpan<byte> data, OperationContext context) => Scan(data, context).Info;

    private static Header Scan(ReadOnlySpan<byte> data, OperationContext context)
    {
        if (!data.StartsWith(Signature)) throw new ImageDataException("Invalid PNG signature.");
        int position = 8, items = 0, width = 0, height = 0, color = 0, depth = 0, interlace = 0;
        int compressedLength = 0, metadataBytes = 0;
        var palette = Array.Empty<byte>();
        var transparency = Array.Empty<byte>();
        bool headerSeen = false, paletteSeen = false, transparencySeen = false, exifSeen = false;
        bool dataSeen = false, dataClosed = false;
        var orientation = ImageOrientation.Normal;
        while (position < data.Length)
        {
            if (++items > context.Limits.MaxContainerItems) throw new ImageLimitException("PNG chunk count limit exceeded.");
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
            if (Crc32.Compute(data.Slice(position + 4, length + 4), context.Cancellation)
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
                palette = payload.ToArray(); paletteSeen = true;
            }
            else if (type.SequenceEqual("tRNS"u8))
            {
                if (transparencySeen || dataSeen || (color == 3 && (!paletteSeen || length == 0 || length > palette.Length / 3))
                    || (color == 0 && length != 2) || (color == 2 && length != 6) || color is 4 or 6)
                    throw new ImageDataException("Invalid PNG transparency chunk.");
                context.Reserve(length);
                transparency = payload.ToArray(); transparencySeen = true;
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
                return new Header(new ImageInfo(ImageFormat.Png, width, height, depth,
                    color is 4 or 6 || transparencySeen, orientation), color, depth, interlace,
                    palette, transparency, compressedLength);
            }
            else if (type.SequenceEqual("acTL"u8) || type.SequenceEqual("fcTL"u8) || type.SequenceEqual("fdAT"u8))
                throw new UnsupportedImageException("Animated PNG is not supported.");
            else
            {
                if ((type[0] & 32) == 0) throw new UnsupportedImageException("Unknown critical PNG chunk.");
                if (length > context.Limits.MaxMetadataBytes - metadataBytes)
                    throw new ImageLimitException("PNG metadata limit exceeded.");
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

    public static ImageBuffer Decode(ReadOnlySpan<byte> data, OperationContext context)
    {
        var header = Scan(data, context);
        context.Reserve(header.CompressedLength);
        var compressed = new byte[header.CompressedLength];
        try
        {
            int source = 8, target = 0;
            while (source < data.Length)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                var length = (int)BinaryPrimitives.ReadUInt32BigEndian(data[source..]);
                if (data.Slice(source + 4, 4).SequenceEqual("IDAT"u8))
                    for (var offset = 0; offset < length;)
                    {
                        var count = Math.Min(65536, length - offset); context.Work(count);
                        data.Slice(source + 8 + offset, count).CopyTo(compressed.AsSpan(target));
                        target += count; offset += count;
                    }
                source += length + 12;
            }
            var streamLength = ZlibFraming.Validate(compressed, FilteredLength(header), context);
            // BCL inflater workspace is admitted until its streams are disposed.
            context.Reserve(256 * 1024);
            try { return DecodeRaster(header, compressed, streamLength, context); }
            finally { context.Release(256 * 1024); }
        }
        finally { context.Release(header.CompressedLength + (long)header.Palette.Length + header.Transparency.Length); }
    }

    private static long FilteredLength(Header header)
    {
        if (header.Interlace == 0) return Pass(0, 0, 1, 1);
        return Pass(0, 0, 8, 8) + Pass(4, 0, 8, 8) + Pass(0, 4, 4, 8) + Pass(2, 0, 4, 4)
            + Pass(0, 2, 2, 4) + Pass(1, 0, 2, 2) + Pass(0, 1, 1, 2);

        long Pass(int x, int y, int dx, int dy)
        {
            if (x >= header.Info.Width || y >= header.Info.Height) return 0;
            var width = (header.Info.Width - x + (long)dx - 1) / dx;
            var height = (header.Info.Height - y + (long)dy - 1) / dy;
            return height * (1 + (width * Channels(header.ColorType) * header.Depth + 7) / 8);
        }
    }

    private static ImageBuffer DecodeRaster(Header header, byte[] compressed, int streamLength, OperationContext context)
    {
        using var input = new MemoryStream(compressed, 0, streamLength, false);
        using var zlib = new ZLibStream(input, CompressionMode.Decompress);
        var result = ImageBuffer.Allocate(header.Info.Width, header.Info.Height, header.Depth == 16 ? 16 : 8, context);
        result.Orientation = header.Info.Orientation;
        uint adlerA = 1, adlerB = 0;
        try
        {
            if (header.Interlace == 0) ReadPass(0, 0, 1, 1);
            else
            {
                // Adam7 pass geometry specified by PNG section 8.
                ReadPass(0, 0, 8, 8); ReadPass(4, 0, 8, 8); ReadPass(0, 4, 4, 8);
                ReadPass(2, 0, 4, 4); ReadPass(0, 2, 2, 4); ReadPass(1, 0, 2, 2); ReadPass(0, 1, 1, 2);
            }
            if (zlib.ReadByte() != -1) throw new ImageDataException("PNG contains excess decompressed pixels.");
            if ((adlerB << 16 | adlerA) != BinaryPrimitives.ReadUInt32BigEndian(compressed.AsSpan(streamLength - 4)))
                throw new ImageDataException("Invalid PNG zlib checksum.");
        }
        catch (InvalidDataException exception)
        {
            result.Dispose();
            throw new ImageDataException("Invalid PNG compressed data.", exception);
        }
        catch { result.Dispose(); throw; }
        return result;

        void ReadPass(int startX, int startY, int stepX, int stepY)
        {
            var width = startX >= result.Width ? 0 : (result.Width - startX + stepX - 1) / stepX;
            var height = startY >= result.Height ? 0 : (result.Height - startY + stepY - 1) / stepY;
            if (width == 0 || height == 0) return;
            var channels = Channels(header.ColorType);
            var rowBytes = (int)(((long)width * channels * header.Depth + 7) / 8);
            var bytesPerPixel = Math.Max(1, (channels * header.Depth + 7) / 8);
            context.Reserve(rowBytes * 2L);
            var previous = new byte[rowBytes];
            var row = new byte[rowBytes];
            try
            {
                for (var y = 0; y < height; y++)
                {
                    context.Work(rowBytes * 6L + width * 4L);
                    var filter = zlib.ReadByte();
                    if (filter is < 0 or > 4) throw new ImageDataException("Invalid PNG row filter.");
                    AddAdler((byte)filter);
                    ReadExactly(zlib, row, context.Cancellation);
                    Adler32.Append(row, ref adlerA, ref adlerB, context.Cancellation);
                    Unfilter(row, previous, bytesPerPixel, filter, context.Cancellation);
                    if (header.ColorType == 6 && stepX == 1)
                    {
                        // RGBA already has the destination channels and precision.
                        // Dense Adam7 rows also qualify; sparse passes keep mapping.
                        var offset = (startY + y * stepY) * result.Stride + startX * bytesPerPixel;
                        CopyRgbaRow(row, result.Pixels.Slice(offset, row.Length),
                            header.Depth == 16 && BitConverter.IsLittleEndian, context.Cancellation);
                    }
                    else
                    {
                        for (var x = 0; x < width; x++)
                        {
                            if ((x & 4095) == 0) context.Cancellation.ThrowIfCancellationRequested();
                            WritePixel(row, x, startX + x * stepX, startY + y * stepY, header, result);
                        }
                    }
                    (previous, row) = (row, previous);
                }
            }
            finally { context.Release(rowBytes * 2L); }
        }

        void AddAdler(byte value)
        {
            adlerA = (adlerA + value) % 65521;
            adlerB = (adlerB + adlerA) % 65521;
        }
    }

    internal static void CopyRgbaRow(ReadOnlySpan<byte> row, Span<byte> destination, bool reverseSamples, CancellationToken cancellation)
    {
        // Legal RGBA rows are multiples of four/eight bytes. Chunking keeps
        // cancellation at least as frequent as the old per-4096-pixel loop.
        for (var offset = 0; offset < row.Length;)
        {
            cancellation.ThrowIfCancellationRequested();
            var count = Math.Min(16384, row.Length - offset);
            var input = row.Slice(offset, count); var output = destination.Slice(offset, count);
            if (reverseSamples)
                BinaryPrimitives.ReverseEndianness(MemoryMarshal.Cast<byte, ushort>(input), MemoryMarshal.Cast<byte, ushort>(output));
            else input.CopyTo(output);
            offset += count;
        }
    }

    private static void ReadExactly(Stream stream, Span<byte> buffer, CancellationToken cancellation)
    {
        while (!buffer.IsEmpty)
        {
            cancellation.ThrowIfCancellationRequested();
            var read = stream.Read(buffer[..Math.Min(buffer.Length, 65536)]);
            if (read == 0) throw new ImageDataException("Truncated PNG pixel data.");
            buffer = buffer[read..];
        }
    }

    internal static void Unfilter(Span<byte> row, ReadOnlySpan<byte> previous, int stride, int filter, CancellationToken cancellation = default)
    {
        if (filter == 0)
        {
            // PNG section 9.2: None stores the original bytes without prediction.
            cancellation.ThrowIfCancellationRequested(); return;
        }
        for (var i = 0; i < row.Length; i++)
        {
            if ((i & 65535) == 0) cancellation.ThrowIfCancellationRequested();
            var left = i >= stride ? row[i - stride] : 0;
            var up = previous[i];
            var upperLeft = i >= stride ? previous[i - stride] : 0;
            var prediction = filter switch
            {
                0 => 0, 1 => left, 2 => up, 3 => (left + up) / 2, 4 => Paeth(left, up, upperLeft),
                _ => throw new ImageDataException("Invalid PNG filter.")
            };
            row[i] = unchecked((byte)(row[i] + prediction));
        }
    }

    private static int Paeth(int left, int up, int upperLeft)
    {
        var predicted = left + up - upperLeft;
        var a = Math.Abs(predicted - left); var b = Math.Abs(predicted - up); var c = Math.Abs(predicted - upperLeft);
        return a <= b && a <= c ? left : b <= c ? up : upperLeft;
    }

    private static int Channels(int color) => color switch { 0 or 3 => 1, 2 => 3, 4 => 2, 6 => 4, _ => 0 };

    private static void WritePixel(ReadOnlySpan<byte> row, int index, int x, int y, Header header, ImageBuffer target)
    {
        var channels = Channels(header.ColorType);
        var maximum = (1 << header.Depth) - 1;
        var outputMaximum = target.BitDepth == 16 ? 65535 : 255;
        int r, g, b, a = outputMaximum;
        if (header.ColorType == 3)
        {
            var paletteIndex = Sample(row, index, header.Depth);
            if (paletteIndex >= header.Palette.Length / 3) throw new ImageDataException("PNG palette index out of range.");
            r = header.Palette[paletteIndex * 3]; g = header.Palette[paletteIndex * 3 + 1]; b = header.Palette[paletteIndex * 3 + 2];
            if (paletteIndex < header.Transparency.Length) a = header.Transparency[paletteIndex];
        }
        else
        {
            var first = Sample(row, index * channels, header.Depth);
            var second = channels >= 2 ? Sample(row, index * channels + 1, header.Depth) : first;
            var third = channels >= 3 ? Sample(row, index * channels + 2, header.Depth) : first;
            r = (int)((first * (long)outputMaximum + maximum / 2) / maximum);
            g = header.ColorType is 0 or 4 ? r : (int)((second * (long)outputMaximum + maximum / 2) / maximum);
            b = header.ColorType is 0 or 4 ? r : (int)((third * (long)outputMaximum + maximum / 2) / maximum);
            if (header.ColorType is 4 or 6)
                a = Sample(row, index * channels + channels - 1, header.Depth);
            else if (header.Transparency.Length != 0)
            {
                var transparent = first == BinaryPrimitives.ReadUInt16BigEndian(header.Transparency);
                if (header.ColorType == 2)
                    transparent &= second == BinaryPrimitives.ReadUInt16BigEndian(header.Transparency.AsSpan(2))
                        && third == BinaryPrimitives.ReadUInt16BigEndian(header.Transparency.AsSpan(4));
                if (transparent) a = 0;
            }
        }
        target.SetSample(x, y, 0, r); target.SetSample(x, y, 1, g); target.SetSample(x, y, 2, b); target.SetSample(x, y, 3, a);
    }

    private static int Sample(ReadOnlySpan<byte> row, int index, int depth)
        => depth == 16 ? BinaryPrimitives.ReadUInt16BigEndian(row[(index * 2)..])
            : depth == 8 ? row[index] : (row[index * depth / 8] >> (8 - depth - index * depth % 8)) & ((1 << depth) - 1);

    public static void Encode(ImageBuffer source, Stream output, PngEncodeOptions options, OperationContext context)
    {
        if (!Enum.IsDefined(options.ColorType) || !Enum.IsDefined(options.Compression))
            throw new ArgumentOutOfRangeException(nameof(options));
        context.PixelBytes(source.Width, source.Height, source.BitDepth);
        context.Reserve(source.ByteLength);
        var color = (int)options.ColorType;
        var rowBytes = source.Width * Channels(color) * (source.BitDepth / 8);
        // Fixed admission allowance for the BCL compressor and its managed IO
        // buffer. Default zlib deflate state alone exceeds 256 KiB; leave margin
        // for the runtime's implementation. This is not a total-process RSS cap.
        // https://zlib.net/zlib_tech.html#memory
        context.Reserve(rowBytes + 1024 * 1024L + 65536);
        var row = new byte[rowBytes];
        output.Write(Signature);
        Span<byte> header = stackalloc byte[13];
        BinaryPrimitives.WriteInt32BigEndian(header, source.Width);
        BinaryPrimitives.WriteInt32BigEndian(header[4..], source.Height);
        header[8] = (byte)source.BitDepth; header[9] = (byte)color; header[10] = 0; header[11] = 0; header[12] = 0;
        WriteChunk(output, "IHDR"u8, header);
        using (var chunks = new IdatWriter(output))
        {
            using (var zlib = new ZLibStream(chunks, options.Compression, true))
            {
                for (var y = 0; y < source.Height; y++)
                {
                    context.Work(rowBytes * 8L);
                    var position = 0;
                    for (var x = 0; x < source.Width; x++)
                    {
                        if ((x & 4095) == 0) context.Cancellation.ThrowIfCancellationRequested();
                        var r = source.Sample(x, y, 0); var g = source.Sample(x, y, 1); var b = source.Sample(x, y, 2);
                        var a = source.Sample(x, y, 3);
                        var maximum = source.BitDepth == 8 ? 255 : 65535;
                        if ((color is 0 or 4) && (r != g || r != b))
                            throw new ArgumentException("Grayscale encoding requires equal RGB channels.", nameof(source));
                        if ((color is 0 or 2) && a != maximum)
                            throw new ArgumentException("Encoding without alpha requires opaque pixels.", nameof(source));
                        WriteSample(row, ref position, r, source.BitDepth);
                        if (color is 2 or 6)
                        {
                            WriteSample(row, ref position, g, source.BitDepth);
                            WriteSample(row, ref position, b, source.BitDepth);
                        }
                        if (color is 4 or 6) WriteSample(row, ref position, a, source.BitDepth);
                    }
                    zlib.WriteByte(0); // Correctness-first encoder: PNG filter None.
                    zlib.Write(row);
                }
            }
        }
        context.Cancellation.ThrowIfCancellationRequested();
        WriteChunk(output, "IEND"u8, []);
    }

    private static void WriteSample(Span<byte> row, ref int position, int sample, int depth)
    {
        if (depth == 16) { BinaryPrimitives.WriteUInt16BigEndian(row[position..], (ushort)sample); position += 2; }
        else row[position++] = (byte)sample;
    }

    internal static void WriteChunk(Stream output, ReadOnlySpan<byte> type, ReadOnlySpan<byte> payload)
    {
        Span<byte> head = stackalloc byte[8];
        BinaryPrimitives.WriteInt32BigEndian(head, payload.Length); type.CopyTo(head[4..]);
        output.Write(head); output.Write(payload);
        // Incremental CRC avoids duplicating large payload buffers.
        var crc = Crc32.ComputeChunk(type, payload);
        Span<byte> tail = stackalloc byte[4]; BinaryPrimitives.WriteUInt32BigEndian(tail, crc); output.Write(tail);
    }

    private static uint UpdateCrc(uint crc, byte value)
    {
        crc ^= value;
        for (var i = 0; i < 8; i++) crc = (crc >> 1) ^ ((crc & 1) == 0 ? 0 : 0xEDB88320u);
        return crc;
    }

    private sealed class IdatWriter(Stream output) : Stream
    {
        private readonly byte[] buffer = new byte[65536];
        private int count;
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Write(ReadOnlySpan<byte> data)
        {
            while (!data.IsEmpty)
            {
                var copy = Math.Min(buffer.Length - count, data.Length);
                data[..copy].CopyTo(buffer.AsSpan(count)); count += copy; data = data[copy..];
                if (count == buffer.Length) Flush();
            }
        }
        public override void Write(byte[] data, int offset, int count) => Write(data.AsSpan(offset, count));
        public override void Flush() { if (count == 0) return; WriteChunk(output, "IDAT"u8, buffer.AsSpan(0, count)); count = 0; }
        protected override void Dispose(bool disposing) { if (disposing) Flush(); base.Dispose(disposing); }
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }
}
