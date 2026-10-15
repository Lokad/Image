using System.Buffers.Binary;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal static class PngCodec
{
    // Separate from released pixels: a compressed input remains leased until
    // the inflater and its streams are disposed, even when output stays live.
    private static readonly PixelStoragePool compressedStorage = new(1024 * 1024);
    private static readonly PixelStoragePool inflatedStorage = new();
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
        var compressed = compressedStorage.Rent(header.CompressedLength);
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
            var filteredLength = FilteredLength(header);
            var rowBytes = ((long)header.Info.Width * Channels(header.ColorType) * header.Depth + 7) / 8;
            var pixelBytes = (long)header.Info.Width * header.Info.Height * (header.Depth == 16 ? 8 : 4);
            // Select before framing work. Keep the original workspace allowance
            // and every subsequent row/result admission available. Tight budgets
            // and large/Adam7 inputs retain the original path. Cancelable framing
            // keeps its original per-symbol cancellation and work checks.
            var materialize = header.Interlace == 0
                && filteredLength <= PixelStoragePool.MaxRetainedBytes
                && context.RemainingWorkingBytes >= filteredLength + 4096 + 256 * 1024L
                    + pixelBytes + rowBytes * 2 + PngInflatedReader.BufferBytes;
            byte[]? filtered = null;
            var filteredBytes = materialize ? (int)filteredLength : 0;
            if (materialize) context.Reserve(filteredBytes);
            try
            {
                if (materialize) filtered = inflatedStorage.Rent(filteredBytes);
                int streamLength;
                var useFiltered = false;
                if (filtered is not null)
                {
                    streamLength = ZlibFraming.ValidateAndInflate(compressed, filtered, context, out var completeTrees);
                    if (completeTrees)
                    {
                        uint a = 1, b = 0;
                        Adler32.Append(filtered, ref a, ref b, context.Cancellation);
                        useFiltered = (b << 16 | a) == BinaryPrimitives.ReadUInt32BigEndian(compressed.AsSpan(streamLength - 4));
                    }
                    // An uncertain tree or invalid checksum must retain the BCL's
                    // original errors and row/filter precedence, without recharging
                    // the already completed framing walk.
                    if (!useFiltered)
                    {
                        inflatedStorage.Return(filtered); filtered = null;
                        context.Release(filteredBytes); filteredBytes = 0;
                    }
                }
                else streamLength = ZlibFraming.Validate(compressed, filteredLength, context);
                // BCL inflater workspace is admitted until its streams are disposed.
                context.Reserve(256 * 1024);
                try { return DecodeRaster(header, compressed, streamLength, context, useFiltered ? filtered : null); }
                finally { context.Release(256 * 1024); }
            }
            finally
            {
                if (filtered is not null) inflatedStorage.Return(filtered);
                context.Release(filteredBytes);
            }
        }
        finally
        {
            compressedStorage.Return(compressed);
            context.Release(header.CompressedLength + (long)header.Palette.Length + header.Transparency.Length);
        }
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

    private static ImageBuffer DecodeRaster(Header header, byte[] compressed, int streamLength, OperationContext context, byte[]? filtered = null)
    {
        using var zlib = new PngInflatedReader(compressed, streamLength, filtered);
        var result = ImageBuffer.AllocateForOverwrite(header.Info.Width, header.Info.Height, header.Depth == 16 ? 16 : 8, context);
        result.Orientation = header.Info.Orientation;
        uint adlerA = 1, adlerB = 0;
        var bufferedBytes = 0;
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
        finally { zlib.Dispose(); context.Release(bufferedBytes); }
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
                // Only the single-pass, noncancelable case can preflight all
                // remaining row work and memory without moving a failure ahead.
                if (header.Interlace == 0 && !context.Cancellation.CanBeCanceled
                    && context.RemainingWork >= height * (rowBytes * 6L + width * 4L)
                    && context.RemainingWorkingBytes >= PngInflatedReader.BufferBytes)
                {
                    bufferedBytes = PngInflatedReader.BufferBytes; context.Reserve(bufferedBytes); zlib.EnableBuffer();
                }
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
                    else if (header.ColorType == 6)
                    {
                        // Sparse RGBA pass pixels already have the output channels
                        // and precision; only their placement and byte order differ.
                        var offset = (startY + y * stepY) * result.Stride + startX * bytesPerPixel;
                        var length = ((width - 1) * stepX + 1) * bytesPerPixel;
                        CopySparseRgbaRow(row, result.Pixels.Slice(offset, length), bytesPerPixel, stepX,
                            header.Depth == 16 && BitConverter.IsLittleEndian, context.Cancellation);
                    }
                    else if (header.ColorType == 2 && header.Depth == 16 && stepX == 1 && header.Transparency.Length == 0)
                    {
                        var offset = (startY + y * stepY) * result.Stride + startX * 8;
                        CopyRgb16Row(row, result.Pixels.Slice(offset, width * 8), context.Cancellation);
                    }
                    else if (header.ColorType == 2 && header.Depth == 16 && stepX == 1 && header.Transparency.Length == 6)
                    {
                        var offset = (startY + y * stepY) * result.Stride + startX * 8;
                        CopyRgb16TransparentRow(row, result.Pixels.Slice(offset, width * 8),
                            BinaryPrimitives.ReadUInt16BigEndian(header.Transparency),
                            BinaryPrimitives.ReadUInt16BigEndian(header.Transparency.AsSpan(2)),
                            BinaryPrimitives.ReadUInt16BigEndian(header.Transparency.AsSpan(4)), context.Cancellation);
                    }
                    else if (header.ColorType == 2 && header.Depth == 8 && stepX == 1 && header.Transparency.Length == 0)
                    {
                        var offset = (startY + y * stepY) * result.Stride + startX * 4;
                        CopyRgb8Row(row, result.Pixels.Slice(offset, width * 4), context.Cancellation);
                    }
                    else if (header.ColorType == 2 && header.Depth == 8 && stepX == 1 && header.Transparency.Length == 6)
                    {
                        var offset = (startY + y * stepY) * result.Stride + startX * 4;
                        CopyRgb8TransparentRow(row, result.Pixels.Slice(offset, width * 4),
                            BinaryPrimitives.ReadUInt16BigEndian(header.Transparency),
                            BinaryPrimitives.ReadUInt16BigEndian(header.Transparency.AsSpan(2)),
                            BinaryPrimitives.ReadUInt16BigEndian(header.Transparency.AsSpan(4)), context.Cancellation);
                    }
                    else if (header.ColorType == 0 && header.Depth < 8 && stepX == 1 && header.Transparency.Length == 0)
                    {
                        var offset = (startY + y * stepY) * result.Stride + startX * 4;
                        CopyPackedGrayRow(row, result.Pixels.Slice(offset, width * 4), width, header.Depth, context.Cancellation);
                    }
                    else if (header.ColorType == 0 && header.Depth < 8 && stepX == 1 && header.Transparency.Length == 2)
                    {
                        var offset = (startY + y * stepY) * result.Stride + startX * 4;
                        CopyPackedGrayTransparentRow(row, result.Pixels.Slice(offset, width * 4), width, header.Depth,
                            BinaryPrimitives.ReadUInt16BigEndian(header.Transparency), context.Cancellation);
                    }
                    else if (header.ColorType == 0 && header.Depth == 8 && stepX == 1 && header.Transparency.Length == 0)
                    {
                        var offset = (startY + y * stepY) * result.Stride + startX * 4;
                        CopyGray8Row(row, result.Pixels.Slice(offset, width * 4), context.Cancellation);
                    }
                    else if (header.ColorType == 0 && header.Depth == 8 && stepX == 1 && header.Transparency.Length == 2)
                    {
                        var offset = (startY + y * stepY) * result.Stride + startX * 4;
                        CopyGray8TransparentRow(row, result.Pixels.Slice(offset, width * 4),
                            BinaryPrimitives.ReadUInt16BigEndian(header.Transparency), context.Cancellation);
                    }
                    else if (header.ColorType == 0 && header.Depth == 16 && stepX == 1 && header.Transparency.Length == 0)
                    {
                        var offset = (startY + y * stepY) * result.Stride + startX * 8;
                        CopyGray16Row(row, result.Pixels.Slice(offset, width * 8), context.Cancellation);
                    }
                    else if (header.ColorType == 4 && header.Depth == 8 && stepX == 1)
                    {
                        var offset = (startY + y * stepY) * result.Stride + startX * 4;
                        CopyGrayAlpha8Row(row, result.Pixels.Slice(offset, width * 4), context.Cancellation);
                    }
                    else if (header.ColorType == 4 && header.Depth == 16 && stepX == 1)
                    {
                        var offset = (startY + y * stepY) * result.Stride + startX * 8;
                        CopyGrayAlpha16Row(row, result.Pixels.Slice(offset, width * 8), context.Cancellation);
                    }
                    else if (header.ColorType == 3 && header.Depth == 8 && stepX == 1)
                    {
                        var offset = (startY + y * stepY) * result.Stride + startX * 4;
                        CopyPalette8Row(row, result.Pixels.Slice(offset, width * 4),
                            header.Palette, header.Transparency, context.Cancellation);
                    }
                    else if (header.ColorType == 3 && header.Depth < 8 && stepX == 1)
                    {
                        var offset = (startY + y * stepY) * result.Stride + startX * 4;
                        CopyPackedPaletteRow(row, result.Pixels.Slice(offset, width * 4), width, header.Depth,
                            header.Palette, header.Transparency, context.Cancellation);
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

    internal static void CopySparseRgbaRow(ReadOnlySpan<byte> row, Span<byte> destination,
        int bytesPerPixel, int stepX, bool reverseSamples, CancellationToken cancellation)
    {
        var pixels = row.Length / bytesPerPixel;
        if (bytesPerPixel == 8)
        {
            for (var x = 0; x < pixels; x++)
            {
                if ((x & 4095) == 0) cancellation.ThrowIfCancellationRequested();
                var value = MemoryMarshal.Read<ulong>(row[(x * 8)..]);
                // Swap each 16-bit channel, retaining RGBA channel order.
                if (reverseSamples)
                    value = (value & 0x00ff00ff00ff00ffUL) << 8 | (value >> 8 & 0x00ff00ff00ff00ffUL);
                MemoryMarshal.Write(destination[(x * stepX * 8)..], in value);
            }
        }
        else
        {
            for (var x = 0; x < pixels; x++)
            {
                if ((x & 4095) == 0) cancellation.ThrowIfCancellationRequested();
                var value = MemoryMarshal.Read<uint>(row[(x * 4)..]);
                MemoryMarshal.Write(destination[(x * stepX * 4)..], in value);
            }
        }
    }

    internal static void CopyPackedGrayRow(ReadOnlySpan<byte> row, Span<byte> destination,
        int pixels, int depth, CancellationToken cancellation)
    {
        // PNG section 7.3: packed gray1/2/4 samples are MSB-first. Their exact
        // RGBA8 scales are 255/85/17 respectively; unused row padding is ignored.
        var maximum = (1u << depth) - 1; var x = 0;
        if (Avx2.IsSupported && BitConverter.IsLittleEndian)
        {
            var shifts = Avx2.MultiplyLow(Vector256.Create(7u, 6, 5, 4, 3, 2, 1, 0), Vector256.Create((uint)depth));
            var mask = Vector256.Create(maximum);
            var replicate = Vector256.Create(255 / maximum * 0x00010101u);
            var alpha = Vector256.Create(0xff000000u);
            for (; x <= pixels - 8; x += 8)
            {
                if ((x & 4095) == 0) cancellation.ThrowIfCancellationRequested();
                var input = row[(x * depth / 8)..];
                var packed = depth == 1 ? input[0] : depth == 2 ? BinaryPrimitives.ReadUInt16BigEndian(input)
                    : BinaryPrimitives.ReadUInt32BigEndian(input);
                var samples = Avx2.ShiftRightLogicalVariable(Vector256.Create(packed), shifts) & mask;
                (Avx2.MultiplyLow(samples, replicate) | alpha).AsByte().CopyTo(destination[(x * 4)..]);
            }
        }
        for (; x < pixels; x++)
        {
            if ((x & 4095) == 0) cancellation.ThrowIfCancellationRequested();
            var sample = Sample(row, x, depth); var gray = (uint)sample * (255 / maximum);
            BinaryPrimitives.WriteUInt32LittleEndian(destination[(x * 4)..], gray * 0x00010101u | 0xff000000u);
        }
    }

    internal static void CopyPackedGrayTransparentRow(ReadOnlySpan<byte> row, Span<byte> destination,
        int pixels, int depth, ushort transparentKey, CancellationToken cancellation)
    {
        // PNG sections 7.3/11.3.2.1: compare the unscaled packed sample to the
        // complete two-byte key. Out-of-range keys never match; retain hidden RGB.
        var maximum = (1u << depth) - 1; var x = 0;
        if (Avx2.IsSupported && BitConverter.IsLittleEndian)
        {
            var shifts = Avx2.MultiplyLow(Vector256.Create(7u, 6, 5, 4, 3, 2, 1, 0), Vector256.Create((uint)depth));
            var mask = Vector256.Create(maximum);
            var replicate = Vector256.Create(255 / maximum * 0x00010101u);
            var opaque = Vector256.Create(0xff000000u);
            var key = Vector256.Create((uint)transparentKey);
            for (; x <= pixels - 8; x += 8)
            {
                if ((x & 4095) == 0) cancellation.ThrowIfCancellationRequested();
                var input = row[(x * depth / 8)..];
                var packed = depth == 1 ? input[0] : depth == 2 ? BinaryPrimitives.ReadUInt16BigEndian(input)
                    : BinaryPrimitives.ReadUInt32BigEndian(input);
                var samples = Avx2.ShiftRightLogicalVariable(Vector256.Create(packed), shifts) & mask;
                var alpha = Avx2.AndNot(Avx2.CompareEqual(samples, key), opaque);
                (Avx2.MultiplyLow(samples, replicate) | alpha).AsByte().CopyTo(destination[(x * 4)..]);
            }
        }
        for (; x < pixels; x++)
        {
            if ((x & 4095) == 0) cancellation.ThrowIfCancellationRequested();
            var sample = Sample(row, x, depth); var gray = (uint)sample * (255 / maximum);
            var alpha = sample == transparentKey ? 0u : 0xff000000u;
            BinaryPrimitives.WriteUInt32LittleEndian(destination[(x * 4)..], gray * 0x00010101u | alpha);
        }
    }

    internal static void CopyGray8Row(ReadOnlySpan<byte> row, Span<byte> destination, CancellationToken cancellation)
    {
        // PNG section 7.3: gray8 samples expand identically into RGB; no scaling.
        var x = 0;
        if (Avx2.IsSupported && BitConverter.IsLittleEndian)
        {
            var replicate = Vector256.Create(0x00010101);
            var alpha = Vector256.Create(unchecked((int)0xff000000));
            for (; x <= row.Length - 8; x += 8)
            {
                if ((x & 4095) == 0) cancellation.ThrowIfCancellationRequested();
                var packed = Vector128.CreateScalar(MemoryMarshal.Read<ulong>(row[x..])).AsByte();
                var samples = Avx2.ConvertToVector256Int32(packed);
                (Avx2.MultiplyLow(samples, replicate) | alpha).AsByte().CopyTo(destination[(x * 4)..]);
            }
        }
        for (; x < row.Length; x++)
        {
            if ((x & 4095) == 0) cancellation.ThrowIfCancellationRequested();
            var output = destination.Slice(x * 4, 4); var value = row[x];
            output[0] = value; output[1] = value; output[2] = value; output[3] = 255;
        }
    }

    internal static void CopyGray8TransparentRow(ReadOnlySpan<byte> row, Span<byte> destination,
        ushort transparentKey, CancellationToken cancellation)
    {
        // PNG 11.3.2.1: compare the complete two-byte key; keys beyond gray8's
        // sample range never match. Transparent pixels retain their gray RGB.
        var x = 0;
        if (Avx2.IsSupported && BitConverter.IsLittleEndian)
        {
            var replicate = Vector256.Create(0x00010101);
            var opaque = Vector256.Create(unchecked((int)0xff000000));
            var key = Vector256.Create((int)transparentKey);
            for (; x <= row.Length - 8; x += 8)
            {
                if ((x & 4095) == 0) cancellation.ThrowIfCancellationRequested();
                var packed = Vector128.CreateScalar(MemoryMarshal.Read<ulong>(row[x..])).AsByte();
                var samples = Avx2.ConvertToVector256Int32(packed);
                var alpha = Avx2.AndNot(Avx2.CompareEqual(samples, key), opaque);
                (Avx2.MultiplyLow(samples, replicate) | alpha).AsByte().CopyTo(destination[(x * 4)..]);
            }
        }
        for (; x < row.Length; x++)
        {
            if ((x & 4095) == 0) cancellation.ThrowIfCancellationRequested();
            var output = destination.Slice(x * 4, 4); var value = row[x];
            output[0] = value; output[1] = value; output[2] = value;
            output[3] = value == transparentKey ? (byte)0 : (byte)255;
        }
    }

    internal static void CopyGray16Row(ReadOnlySpan<byte> row, Span<byte> destination, CancellationToken cancellation)
    {
        // PNG section 7.3: retain each big-endian gray16 sample in all three
        // native-endian color channels and insert full-precision opaque alpha.
        var pixels = row.Length / 2; var x = 0;
        if (Avx2.IsSupported && BitConverter.IsLittleEndian)
        {
            // Broadcast four complete gray samples. Each lane selects two and
            // reverses their byte order while replicating the color channels.
            var shuffle = Vector256.Create(
                (byte)1, 0, 1, 0, 1, 0, 128, 128, 3, 2, 3, 2, 3, 2, 128, 128,
                5, 4, 5, 4, 5, 4, 128, 128, 7, 6, 7, 6, 7, 6, 128, 128);
            var alpha = Vector256.Create(0xffff000000000000UL).AsByte();
            for (; x <= pixels - 4; x += 4)
            {
                if ((x & 4095) == 0) cancellation.ThrowIfCancellationRequested();
                var packed = Vector256.Create(MemoryMarshal.Read<ulong>(row[(x * 2)..])).AsByte();
                (Avx2.Shuffle(packed, shuffle) | alpha).CopyTo(destination[(x * 8)..]);
            }
        }
        var samples = MemoryMarshal.Cast<byte, ushort>(destination);
        for (; x < pixels; x++)
        {
            if ((x & 4095) == 0) cancellation.ThrowIfCancellationRequested();
            var value = BinaryPrimitives.ReadUInt16BigEndian(row[(x * 2)..]);
            samples[x * 4] = value; samples[x * 4 + 1] = value;
            samples[x * 4 + 2] = value; samples[x * 4 + 3] = ushort.MaxValue;
        }
    }

    internal static void CopyGrayAlpha8Row(ReadOnlySpan<byte> row, Span<byte> destination, CancellationToken cancellation)
    {
        // PNG section 7.3: replicate gray into RGB and retain its associated alpha,
        // including hidden RGB at zero alpha. Byte shuffles need no sample scaling.
        var pixels = row.Length / 2; var x = 0;
        if (Avx2.IsSupported)
        {
            // Each lane selects four pixels from its copy of the bounded input.
            var shuffle = Vector256.Create(
                (byte)0, 0, 0, 1, 2, 2, 2, 3, 4, 4, 4, 5, 6, 6, 6, 7,
                8, 8, 8, 9, 10, 10, 10, 11, 12, 12, 12, 13, 14, 14, 14, 15);
            for (; x <= pixels - 8; x += 8)
            {
                if ((x & 4095) == 0) cancellation.ThrowIfCancellationRequested();
                var packed = MemoryMarshal.Read<Vector128<byte>>(row[(x * 2)..]);
                Avx2.Shuffle(Vector256.Create(packed, packed), shuffle).CopyTo(destination[(x * 4)..]);
            }
        }
        for (; x < pixels; x++)
        {
            if ((x & 4095) == 0) cancellation.ThrowIfCancellationRequested();
            var output = destination.Slice(x * 4, 4); var gray = row[x * 2];
            output[0] = gray; output[1] = gray; output[2] = gray; output[3] = row[x * 2 + 1];
        }
    }

    internal static void CopyGrayAlpha16Row(ReadOnlySpan<byte> row, Span<byte> destination, CancellationToken cancellation)
    {
        // PNG section 7.3: retain full big-endian gray/alpha samples. Replicate
        // gray into native-endian RGB, including hidden RGB under zero alpha.
        var pixels = row.Length / 4; var x = 0;
        if (Avx2.IsSupported && BitConverter.IsLittleEndian)
        {
            // Each lane selects two pixels from a copy of four complete inputs,
            // reversing sample bytes while repeating gray and retaining alpha.
            var shuffle = Vector256.Create(
                (byte)1, 0, 1, 0, 1, 0, 3, 2, 5, 4, 5, 4, 5, 4, 7, 6,
                9, 8, 9, 8, 9, 8, 11, 10, 13, 12, 13, 12, 13, 12, 15, 14);
            for (; x <= pixels - 4; x += 4)
            {
                if ((x & 4095) == 0) cancellation.ThrowIfCancellationRequested();
                var packed = MemoryMarshal.Read<Vector128<byte>>(row[(x * 4)..]);
                Avx2.Shuffle(Vector256.Create(packed, packed), shuffle).CopyTo(destination[(x * 8)..]);
            }
        }
        var samples = MemoryMarshal.Cast<byte, ushort>(destination);
        for (; x < pixels; x++)
        {
            if ((x & 4095) == 0) cancellation.ThrowIfCancellationRequested();
            var input = row.Slice(x * 4, 4); var gray = BinaryPrimitives.ReadUInt16BigEndian(input);
            samples[x * 4] = gray; samples[x * 4 + 1] = gray; samples[x * 4 + 2] = gray;
            samples[x * 4 + 3] = BinaryPrimitives.ReadUInt16BigEndian(input[2..]);
        }
    }

    internal static void CopyPalette8Row(ReadOnlySpan<byte> row, Span<byte> destination,
        ReadOnlySpan<byte> palette, ReadOnlySpan<byte> transparency, CancellationToken cancellation)
    {
        // PNG sections 7.3/11.2.2/11.3.2.1: indices select RGB triples. Missing
        // tRNS entries are opaque; zero alpha preserves the palette's hidden RGB.
        var colors = palette.Length / 3;
        if (colors == 256 && row.Length >= 256 && transparency.Length is 0 or 256
            && Avx512Vbmi.IsSupported && Avx512BW.IsSupported)
        {
            CopyFullPalette8Row(row, destination, palette, transparency, cancellation);
            return;
        }
        var x = colors is >= 1 and <= 16 && row.Length >= 16 && Ssse3.IsSupported
            ? CopySmallPalette8Row(row, destination, palette, transparency, cancellation) : 0;
        for (; x < row.Length; x++)
        {
            if ((x & 4095) == 0) cancellation.ThrowIfCancellationRequested();
            var index = row[x];
            if (index >= colors) throw new ImageDataException("PNG palette index out of range.");
            var rgb = palette.Slice(index * 3, 3);
            var alpha = index < transparency.Length ? transparency[index] : (byte)255;
            var rgba = (uint)BinaryPrimitives.ReadUInt16LittleEndian(rgb) | (uint)rgb[2] << 16 | (uint)alpha << 24;
            BinaryPrimitives.WriteUInt32LittleEndian(destination[(x * 4)..], rgba);
        }
    }

    private static int CopySmallPalette8Row(ReadOnlySpan<byte> row, Span<byte> destination,
        ReadOnlySpan<byte> palette, ReadOnlySpan<byte> transparency, CancellationToken cancellation)
    {
        // At most sixteen RGB triples fit per-channel byte shuffle vectors.
        // Variable insertion handles partial palettes/alpha without padded storage.
        var colors = palette.Length / 3;
        var red = Vector128<byte>.Zero; var green = red; var blue = red;
        var alpha = Vector128.Create((byte)255);
        for (var i = 0; i < colors; i++)
        {
            red = red.WithElement(i, palette[i * 3]);
            green = green.WithElement(i, palette[i * 3 + 1]);
            blue = blue.WithElement(i, palette[i * 3 + 2]);
            if (i < transparency.Length) alpha = alpha.WithElement(i, transparency[i]);
        }
        var maximum = Vector128.Create((byte)(colors - 1)); var x = 0;
        for (; x <= row.Length - 16; x += 16)
        {
            var indices = Vector128.Create(row.Slice(x, 16));
            // Do not store an invalid group. The original scalar loop visits its
            // prefix and cancellation checkpoint once, before the first error.
            if (!Vector128.EqualsAll(Sse2.Min(indices, maximum), indices)) break;
            if ((x & 4095) == 0) cancellation.ThrowIfCancellationRequested();
            var r = Ssse3.Shuffle(red, indices); var g = Ssse3.Shuffle(green, indices);
            var b = Ssse3.Shuffle(blue, indices); var a = Ssse3.Shuffle(alpha, indices);
            var rgLow = Sse2.UnpackLow(r, g).AsUInt16(); var baLow = Sse2.UnpackLow(b, a).AsUInt16();
            var rgHigh = Sse2.UnpackHigh(r, g).AsUInt16(); var baHigh = Sse2.UnpackHigh(b, a).AsUInt16();
            Sse2.UnpackLow(rgLow, baLow).AsByte().CopyTo(destination.Slice(x * 4, 16));
            Sse2.UnpackHigh(rgLow, baLow).AsByte().CopyTo(destination.Slice(x * 4 + 16, 16));
            Sse2.UnpackLow(rgHigh, baHigh).AsByte().CopyTo(destination.Slice(x * 4 + 32, 16));
            Sse2.UnpackHigh(rgHigh, baHigh).AsByte().CopyTo(destination.Slice(x * 4 + 48, 16));
        }
        return x;
    }

    private static void CopyFullPalette8Row(ReadOnlySpan<byte> row, Span<byte> destination,
        ReadOnlySpan<byte> palette, ReadOnlySpan<byte> transparency, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        // For each group of 64 RGB triples, component offsets are 3*i+c.
        // Select directly from the existing palette into registers: no expanded
        // table or additional image workspace is needed. All byte indices are
        // valid only for the complete 256-entry palette admitted by the caller.
        var indices = Vector512.Create<byte>([
            0, 3, 6, 9, 12, 15, 18, 21, 24, 27, 30, 33, 36, 39, 42, 45,
            48, 51, 54, 57, 60, 63, 66, 69, 72, 75, 78, 81, 84, 87, 90, 93,
            96, 99, 102, 105, 108, 111, 114, 117, 120, 123, 126, 129, 132, 135, 138, 141,
            144, 147, 150, 153, 156, 159, 162, 165, 168, 171, 174, 177, 180, 183, 186, 189]);
        ReadPaletteGroup(palette[..192], indices, out var r0, out var g0, out var b0);
        ReadPaletteGroup(palette.Slice(192, 192), indices, out var r1, out var g1, out var b1);
        ReadPaletteGroup(palette.Slice(384, 192), indices, out var r2, out var g2, out var b2);
        ReadPaletteGroup(palette.Slice(576, 192), indices, out var r3, out var g3, out var b3);
        var opaque = Vector512.Create((byte)255);
        var a0 = transparency.Length == 0 ? opaque : Vector512.Create(transparency[..64]);
        var a1 = transparency.Length == 0 ? opaque : Vector512.Create(transparency.Slice(64, 64));
        var a2 = transparency.Length == 0 ? opaque : Vector512.Create(transparency.Slice(128, 64));
        var a3 = transparency.Length == 0 ? opaque : Vector512.Create(transparency.Slice(192, 64));
        var x = 0;
        for (; x <= row.Length - 64; x += 64)
        {
            if ((x & 4095) == 0) cancellation.ThrowIfCancellationRequested();
            var packed = Vector512.Create(row.Slice(x, 64));
            var high = Vector512.LessThan(packed.AsSByte(), Vector512<sbyte>.Zero).AsByte();
            var r = LookupPaletteChannel(packed, high, r0, r1, r2, r3);
            var g = LookupPaletteChannel(packed, high, g0, g1, g2, g3);
            var b = LookupPaletteChannel(packed, high, b0, b1, b2, b3);
            var a = LookupPaletteChannel(packed, high, a0, a1, a2, a3);
            var rgLow = Avx512BW.UnpackLow(r, g).AsUInt16();
            var rgHigh = Avx512BW.UnpackHigh(r, g).AsUInt16();
            var baLow = Avx512BW.UnpackLow(b, a).AsUInt16();
            var baHigh = Avx512BW.UnpackHigh(b, a).AsUInt16();
            var first = Avx512BW.UnpackLow(rgLow, baLow).AsUInt32();
            var second = Avx512BW.UnpackHigh(rgLow, baLow).AsUInt32();
            var third = Avx512BW.UnpackLow(rgHigh, baHigh).AsUInt32();
            var fourth = Avx512BW.UnpackHigh(rgHigh, baHigh).AsUInt32();
            // Unpack operates within 128-bit lanes. Transpose the four lane
            // groups back into consecutive runs of 16 complete RGBA pixels.
            var even12 = Avx512F.Shuffle4x128(first, second, 0x88);
            var even34 = Avx512F.Shuffle4x128(third, fourth, 0x88);
            var odd12 = Avx512F.Shuffle4x128(first, second, 0xdd);
            var odd34 = Avx512F.Shuffle4x128(third, fourth, 0xdd);
            Avx512F.Shuffle4x128(even12, even34, 0x88).AsByte().CopyTo(destination.Slice(x * 4, 64));
            Avx512F.Shuffle4x128(odd12, odd34, 0x88).AsByte().CopyTo(destination.Slice(x * 4 + 64, 64));
            Avx512F.Shuffle4x128(even12, even34, 0xdd).AsByte().CopyTo(destination.Slice(x * 4 + 128, 64));
            Avx512F.Shuffle4x128(odd12, odd34, 0xdd).AsByte().CopyTo(destination.Slice(x * 4 + 192, 64));
        }
        for (; x < row.Length; x++)
        {
            if ((x & 4095) == 0) cancellation.ThrowIfCancellationRequested();
            var index = row[x]; var rgb = palette.Slice(index * 3, 3);
            var alpha = transparency.Length == 0 ? (byte)255 : transparency[index];
            var rgba = (uint)BinaryPrimitives.ReadUInt16LittleEndian(rgb) | (uint)rgb[2] << 16 | (uint)alpha << 24;
            BinaryPrimitives.WriteUInt32LittleEndian(destination[(x * 4)..], rgba);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ReadPaletteGroup(ReadOnlySpan<byte> palette, Vector512<byte> indices,
        out Vector512<byte> red, out Vector512<byte> green, out Vector512<byte> blue)
    {
        var first = Vector512.Create(palette[..64]);
        var middle = Vector512.Create(palette.Slice(64, 64));
        var last = Vector512.Create(palette.Slice(128, 64));
        red = SelectPaletteComponent(first, middle, last, indices);
        green = SelectPaletteComponent(first, middle, last, indices + Vector512.Create((byte)1));
        blue = SelectPaletteComponent(first, middle, last, indices + Vector512.Create((byte)2));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector512<byte> SelectPaletteComponent(Vector512<byte> first, Vector512<byte> middle,
        Vector512<byte> last, Vector512<byte> indices)
        => Vector512.ConditionalSelect(Vector512.LessThan(indices.AsSByte(), Vector512<sbyte>.Zero).AsByte(),
            Avx512Vbmi.PermuteVar64x8(last, indices), Avx512Vbmi.PermuteVar64x8x2(first, indices, middle));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector512<byte> LookupPaletteChannel(Vector512<byte> indices, Vector512<byte> high,
        Vector512<byte> first, Vector512<byte> second, Vector512<byte> third, Vector512<byte> fourth)
        => Vector512.ConditionalSelect(high, Avx512Vbmi.PermuteVar64x8x2(third, indices, fourth),
            Avx512Vbmi.PermuteVar64x8x2(first, indices, second));

    internal static void CopyPackedPaletteRow(ReadOnlySpan<byte> row, Span<byte> destination, int pixels, int depth,
        ReadOnlySpan<byte> palette, ReadOnlySpan<byte> transparency, CancellationToken cancellation)
    {
        // PNG sections 7.3/11.2.2/11.3.2.1: packed indices are MSB-first,
        // select RGB triples, and default to opaque beyond the tRNS entries.
        var x = 0; var mask = (1 << depth) - 1; var colors = palette.Length / 3;
        if (depth == 1 && pixels >= 16 && colors == 2 && transparency.Length is 0 or 2 && Ssse3.IsSupported)
        {
            var red = Vector128.CreateScalar((ushort)(palette[0] | palette[3] << 8)).AsByte();
            var green = Vector128.CreateScalar((ushort)(palette[1] | palette[4] << 8)).AsByte();
            var blue = Vector128.CreateScalar((ushort)(palette[2] | palette[5] << 8)).AsByte();
            var alpha = transparency.Length == 0 ? Vector128.Create((byte)255)
                : Vector128.CreateScalar(BinaryPrimitives.ReadUInt16LittleEndian(transparency)).AsByte();
            // Repeat each of two packed bytes over eight lanes, then test its
            // MSB-first bits. Complete two-entry palettes make every index valid.
            var repeat = Vector128.Create((byte)0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1);
            var bits = Vector128.Create((byte)128, 64, 32, 16, 8, 4, 2, 1, 128, 64, 32, 16, 8, 4, 2, 1);
            var one = Vector128.Create((byte)1);
            for (; x <= pixels - 16; x += 16)
            {
                if ((x & 4095) == 0) cancellation.ThrowIfCancellationRequested();
                var packed = Vector128.CreateScalar(BinaryPrimitives.ReadUInt16LittleEndian(row[(x / 8)..])).AsByte();
                var selected = Ssse3.Shuffle(packed, repeat) & bits;
                var indices = Sse2.AndNot(Sse2.CompareEqual(selected, Vector128<byte>.Zero), one);
                var r = Ssse3.Shuffle(red, indices); var g = Ssse3.Shuffle(green, indices);
                var b = Ssse3.Shuffle(blue, indices); var a = Ssse3.Shuffle(alpha, indices);
                var rgLow = Sse2.UnpackLow(r, g).AsUInt16(); var baLow = Sse2.UnpackLow(b, a).AsUInt16();
                var rgHigh = Sse2.UnpackHigh(r, g).AsUInt16(); var baHigh = Sse2.UnpackHigh(b, a).AsUInt16();
                Sse2.UnpackLow(rgLow, baLow).AsByte().CopyTo(destination.Slice(x * 4, 16));
                Sse2.UnpackHigh(rgLow, baLow).AsByte().CopyTo(destination.Slice(x * 4 + 16, 16));
                Sse2.UnpackLow(rgHigh, baHigh).AsByte().CopyTo(destination.Slice(x * 4 + 32, 16));
                Sse2.UnpackHigh(rgHigh, baHigh).AsByte().CopyTo(destination.Slice(x * 4 + 48, 16));
            }
        }
        else if (depth == 2 && pixels >= 16 && colors == 4 && transparency.Length is 0 or 4 && Ssse3.IsSupported)
        {
            // Four complete RGB triples become four-entry register lookups.
            // Every two-bit index is valid; partial palettes/alpha keep scalar
            // validation below, including the first invalid index's prefix.
            var red = Vector128.CreateScalar((uint)palette[0] | (uint)palette[3] << 8 | (uint)palette[6] << 16 | (uint)palette[9] << 24).AsByte();
            var green = Vector128.CreateScalar((uint)palette[1] | (uint)palette[4] << 8 | (uint)palette[7] << 16 | (uint)palette[10] << 24).AsByte();
            var blue = Vector128.CreateScalar((uint)palette[2] | (uint)palette[5] << 8 | (uint)palette[8] << 16 | (uint)palette[11] << 24).AsByte();
            var alpha = transparency.Length == 0 ? Vector128.Create((byte)255)
                : Vector128.CreateScalar(BinaryPrimitives.ReadUInt32LittleEndian(transparency)).AsByte();
            var pair = Vector128.Create((byte)3);
            for (; x <= pixels - 16; x += 16)
            {
                if ((x & 4095) == 0) cancellation.ThrowIfCancellationRequested();
                var packed = Vector128.CreateScalar(BinaryPrimitives.ReadUInt32LittleEndian(row[(x / 4)..])).AsByte();
                var first = Sse2.ShiftRightLogical(packed.AsUInt16(), 6).AsByte() & pair;
                var second = Sse2.ShiftRightLogical(packed.AsUInt16(), 4).AsByte() & pair;
                var third = Sse2.ShiftRightLogical(packed.AsUInt16(), 2).AsByte() & pair;
                var fourth = packed & pair;
                var indices = Sse2.UnpackLow(Sse2.UnpackLow(first, second).AsUInt16(),
                    Sse2.UnpackLow(third, fourth).AsUInt16()).AsByte();
                var r = Ssse3.Shuffle(red, indices); var g = Ssse3.Shuffle(green, indices);
                var b = Ssse3.Shuffle(blue, indices); var a = Ssse3.Shuffle(alpha, indices);
                var rgLow = Sse2.UnpackLow(r, g).AsUInt16(); var baLow = Sse2.UnpackLow(b, a).AsUInt16();
                var rgHigh = Sse2.UnpackHigh(r, g).AsUInt16(); var baHigh = Sse2.UnpackHigh(b, a).AsUInt16();
                Sse2.UnpackLow(rgLow, baLow).AsByte().CopyTo(destination.Slice(x * 4, 16));
                Sse2.UnpackHigh(rgLow, baLow).AsByte().CopyTo(destination.Slice(x * 4 + 16, 16));
                Sse2.UnpackLow(rgHigh, baHigh).AsByte().CopyTo(destination.Slice(x * 4 + 32, 16));
                Sse2.UnpackHigh(rgHigh, baHigh).AsByte().CopyTo(destination.Slice(x * 4 + 48, 16));
            }
        }
        else if (depth == 4 && pixels >= 16 && colors == 16 && transparency.Length is 0 or 16 && Ssse3.IsSupported)
        {
            // Split the 48 RGB bytes into three registers, then select each
            // component into a 16-entry byte lookup. These shuffle positions
            // are derived directly from RGB triples, not codec tables.
            var first = Vector128.Create(palette[..16]);
            var middle = Vector128.Create(palette.Slice(16, 16));
            var last = Vector128.Create(palette.Slice(32, 16));
            var red = Ssse3.Shuffle(first, Vector128.Create((byte)0, 3, 6, 9, 12, 15, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128))
                | Ssse3.Shuffle(middle, Vector128.Create((byte)128, 128, 128, 128, 128, 128, 2, 5, 8, 11, 14, 128, 128, 128, 128, 128))
                | Ssse3.Shuffle(last, Vector128.Create((byte)128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 1, 4, 7, 10, 13));
            var green = Ssse3.Shuffle(first, Vector128.Create((byte)1, 4, 7, 10, 13, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128))
                | Ssse3.Shuffle(middle, Vector128.Create((byte)128, 128, 128, 128, 128, 0, 3, 6, 9, 12, 15, 128, 128, 128, 128, 128))
                | Ssse3.Shuffle(last, Vector128.Create((byte)128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 2, 5, 8, 11, 14));
            var blue = Ssse3.Shuffle(first, Vector128.Create((byte)2, 5, 8, 11, 14, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128))
                | Ssse3.Shuffle(middle, Vector128.Create((byte)128, 128, 128, 128, 128, 1, 4, 7, 10, 13, 128, 128, 128, 128, 128, 128))
                | Ssse3.Shuffle(last, Vector128.Create((byte)128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 0, 3, 6, 9, 12, 15));
            var alpha = transparency.Length == 0 ? Vector128.Create((byte)255) : Vector128.Create(transparency);
            var nibble = Vector128.Create((byte)15);
            for (; x <= pixels - 16; x += 16)
            {
                if ((x & 4095) == 0) cancellation.ThrowIfCancellationRequested();
                var packed = Vector128.CreateScalar(BinaryPrimitives.ReadUInt64LittleEndian(row[(x / 2)..])).AsByte();
                var indices = Sse2.UnpackLow(Sse2.ShiftRightLogical(packed.AsUInt16(), 4).AsByte() & nibble, packed & nibble);
                var r = Ssse3.Shuffle(red, indices); var g = Ssse3.Shuffle(green, indices);
                var b = Ssse3.Shuffle(blue, indices); var a = Ssse3.Shuffle(alpha, indices);
                var rgLow = Sse2.UnpackLow(r, g).AsUInt16(); var baLow = Sse2.UnpackLow(b, a).AsUInt16();
                var rgHigh = Sse2.UnpackHigh(r, g).AsUInt16(); var baHigh = Sse2.UnpackHigh(b, a).AsUInt16();
                Sse2.UnpackLow(rgLow, baLow).AsByte().CopyTo(destination.Slice(x * 4, 16));
                Sse2.UnpackHigh(rgLow, baLow).AsByte().CopyTo(destination.Slice(x * 4 + 16, 16));
                Sse2.UnpackLow(rgHigh, baHigh).AsByte().CopyTo(destination.Slice(x * 4 + 32, 16));
                Sse2.UnpackHigh(rgHigh, baHigh).AsByte().CopyTo(destination.Slice(x * 4 + 48, 16));
            }
        }
        for (; x < pixels; x++)
        {
            if ((x & 4095) == 0) cancellation.ThrowIfCancellationRequested();
            var bit = x * depth; var index = row[bit / 8] >> (8 - depth - bit % 8) & mask;
            if (index >= colors) throw new ImageDataException("PNG palette index out of range.");
            var rgb = palette.Slice(index * 3, 3);
            var alpha = index < transparency.Length ? transparency[index] : (byte)255;
            var rgba = (uint)BinaryPrimitives.ReadUInt16LittleEndian(rgb) | (uint)rgb[2] << 16 | (uint)alpha << 24;
            BinaryPrimitives.WriteUInt32LittleEndian(destination[(x * 4)..], rgba);
        }
    }

    internal static void CopyRgb8Row(ReadOnlySpan<byte> row, Span<byte> destination, CancellationToken cancellation)
    {
        // PNG section 7.3: retain the three byte samples and insert opaque alpha.
        var pixels = row.Length / 3; var x = 0;
        if (Avx2.IsSupported && BitConverter.IsLittleEndian)
        {
            // Duplicate the middle word: each 128-bit lane holds four complete
            // RGB pixels, with its byte indices relative to that lane.
            var shuffle = Vector256.Create(
                (byte)0, 1, 2, 128, 3, 4, 5, 128, 6, 7, 8, 128, 9, 10, 11, 128,
                4, 5, 6, 128, 7, 8, 9, 128, 10, 11, 12, 128, 13, 14, 15, 128);
            var alpha = Vector256.Create(0xff000000u).AsByte();
            for (; x <= pixels - 8; x += 8)
            {
                if ((x & 4095) == 0) cancellation.ThrowIfCancellationRequested();
                var input = row[(x * 3)..];
                var first = MemoryMarshal.Read<ulong>(input);
                var middle = MemoryMarshal.Read<ulong>(input[8..]);
                var last = MemoryMarshal.Read<ulong>(input[16..]);
                var packed = Vector256.Create(first, middle, middle, last).AsByte();
                (Avx2.Shuffle(packed, shuffle) | alpha).CopyTo(destination[(x * 4)..]);
            }
        }
        for (; x < pixels; x++)
        {
            if ((x & 4095) == 0) cancellation.ThrowIfCancellationRequested();
            var input = row.Slice(x * 3, 3); var output = destination.Slice(x * 4, 4);
            output[0] = input[0]; output[1] = input[1]; output[2] = input[2]; output[3] = 255;
        }
    }

    internal static void CopyRgb8TransparentRow(ReadOnlySpan<byte> row, Span<byte> destination,
        ushort redKey, ushort greenKey, ushort blueKey, CancellationToken cancellation)
    {
        // PNG 11.3.2.1: compare all three complete 16-bit keys. Keys above 255
        // never match RGB8 samples; zero alpha retains the original RGB bytes.
        var pixels = row.Length / 3; var x = 0;
        if (Avx2.IsSupported && BitConverter.IsLittleEndian)
        {
            var shuffle = Vector256.Create(
                (byte)0, 1, 2, 128, 3, 4, 5, 128, 6, 7, 8, 128, 9, 10, 11, 128,
                4, 5, 6, 128, 7, 8, 9, 128, 10, 11, 12, 128, 13, 14, 15, 128);
            var opaque = Vector256.Create(0xff000000u);
            // The shuffle leaves alpha zero. An out-of-range key sets that
            // byte, making equality impossible without truncating any key.
            var key = Vector256.Create(redKey > 255 || greenKey > 255 || blueKey > 255
                ? 0xff000000u : (uint)redKey | (uint)greenKey << 8 | (uint)blueKey << 16);
            for (; x <= pixels - 8; x += 8)
            {
                if ((x & 4095) == 0) cancellation.ThrowIfCancellationRequested();
                var input = row[(x * 3)..];
                var first = MemoryMarshal.Read<ulong>(input);
                var middle = MemoryMarshal.Read<ulong>(input[8..]);
                var last = MemoryMarshal.Read<ulong>(input[16..]);
                var packed = Vector256.Create(first, middle, middle, last).AsByte();
                var rgb = Avx2.Shuffle(packed, shuffle).AsUInt32();
                var alpha = Avx2.AndNot(Avx2.CompareEqual(rgb, key), opaque);
                (rgb | alpha).AsByte().CopyTo(destination[(x * 4)..]);
            }
        }
        for (; x < pixels; x++)
        {
            if ((x & 4095) == 0) cancellation.ThrowIfCancellationRequested();
            var input = row.Slice(x * 3, 3); var output = destination.Slice(x * 4, 4);
            output[0] = input[0]; output[1] = input[1]; output[2] = input[2];
            output[3] = input[0] == redKey && input[1] == greenKey && input[2] == blueKey ? (byte)0 : (byte)255;
        }
    }

    internal static void CopyRgb16Row(ReadOnlySpan<byte> row, Span<byte> destination, CancellationToken cancellation)
    {
        // PNG section 7.3: RGB16 has three big-endian samples per pixel.
        // Expand to four native-endian samples with opaque alpha, without scaling.
        var pixels = row.Length / 6;
        var x = 0;
        if (Avx2.IsSupported && BitConverter.IsLittleEndian)
        {
            // Each 128-bit lane addresses its own 16 input bytes. Duplicate the
            // middle eight-byte word so both lanes contain two complete pixels.
            // Swap sample bytes while inserting the two opaque alpha bytes.
            var shuffle = Vector256.Create(
                (byte)1, 0, 3, 2, 5, 4, 128, 128, 7, 6, 9, 8, 11, 10, 128, 128,
                5, 4, 7, 6, 9, 8, 128, 128, 11, 10, 13, 12, 15, 14, 128, 128);
            var alpha = Vector256.Create(0xffff000000000000UL).AsByte();
            for (; x <= pixels - 4; x += 4)
            {
                if ((x & 4095) == 0) cancellation.ThrowIfCancellationRequested();
                var input = row[(x * 6)..];
                var first = MemoryMarshal.Read<ulong>(input);
                var middle = MemoryMarshal.Read<ulong>(input[8..]);
                var last = MemoryMarshal.Read<ulong>(input[16..]);
                var packed = Vector256.Create(first, middle, middle, last).AsByte();
                (Avx2.Shuffle(packed, shuffle) | alpha).CopyTo(destination[(x * 8)..]);
            }
        }
        var samples = MemoryMarshal.Cast<byte, ushort>(destination);
        for (; x < pixels; x++)
        {
            if ((x & 4095) == 0) cancellation.ThrowIfCancellationRequested();
            var input = row[(x * 6)..];
            samples[x * 4] = BinaryPrimitives.ReadUInt16BigEndian(input);
            samples[x * 4 + 1] = BinaryPrimitives.ReadUInt16BigEndian(input[2..]);
            samples[x * 4 + 2] = BinaryPrimitives.ReadUInt16BigEndian(input[4..]);
            samples[x * 4 + 3] = ushort.MaxValue;
        }
    }

    internal static void CopyRgb16TransparentRow(ReadOnlySpan<byte> row, Span<byte> destination,
        ushort redKey, ushort greenKey, ushort blueKey, CancellationToken cancellation)
    {
        // PNG 7.3/11.3.2.1: retain every RGB16 sample, comparing the full triplet.
        // Byte shuffling converts BE RGB to native LE RGB with initially zero alpha.
        var pixels = row.Length / 6; var x = 0;
        if (Avx2.IsSupported && BitConverter.IsLittleEndian)
        {
            var shuffle = Vector256.Create(
                (byte)1, 0, 3, 2, 5, 4, 128, 128, 7, 6, 9, 8, 11, 10, 128, 128,
                5, 4, 7, 6, 9, 8, 128, 128, 11, 10, 13, 12, 15, 14, 128, 128);
            var opaque = Vector256.Create(0xffff000000000000UL);
            var key = Vector256.Create((ulong)redKey | (ulong)greenKey << 16 | (ulong)blueKey << 32);
            for (; x <= pixels - 4; x += 4)
            {
                if ((x & 4095) == 0) cancellation.ThrowIfCancellationRequested();
                var input = row[(x * 6)..];
                var first = MemoryMarshal.Read<ulong>(input);
                var middle = MemoryMarshal.Read<ulong>(input[8..]);
                var last = MemoryMarshal.Read<ulong>(input[16..]);
                var packed = Vector256.Create(first, middle, middle, last).AsByte();
                var rgb = Avx2.Shuffle(packed, shuffle).AsUInt64();
                var alpha = Avx2.AndNot(Avx2.CompareEqual(rgb, key), opaque);
                (rgb | alpha).AsByte().CopyTo(destination[(x * 8)..]);
            }
        }
        var samples = MemoryMarshal.Cast<byte, ushort>(destination);
        for (; x < pixels; x++)
        {
            if ((x & 4095) == 0) cancellation.ThrowIfCancellationRequested();
            var input = row[(x * 6)..];
            var red = BinaryPrimitives.ReadUInt16BigEndian(input);
            var green = BinaryPrimitives.ReadUInt16BigEndian(input[2..]);
            var blue = BinaryPrimitives.ReadUInt16BigEndian(input[4..]);
            samples[x * 4] = red; samples[x * 4 + 1] = green; samples[x * 4 + 2] = blue;
            samples[x * 4 + 3] = red == redKey && green == greenKey && blue == blueKey ? (ushort)0 : ushort.MaxValue;
        }
    }

    private static void ReadExactly(PngInflatedReader stream, Span<byte> buffer, CancellationToken cancellation)
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
        if (filter == 1 && stride == 8 && Vector128.IsHardwareAccelerated && previous.Length >= row.Length)
        {
            UnfilterSub8(row, cancellation); return;
        }
        if (filter == 4 && (stride == 8 || stride == 4) && Vector128.IsHardwareAccelerated
            && previous.Length >= row.Length && !((ReadOnlySpan<byte>)row).Overlaps(previous))
        {
            if (stride == 8) UnfilterPaeth8(row, previous, cancellation);
            else UnfilterPaeth4(row, previous, cancellation);
            return;
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

    private static void UnfilterSub8(Span<byte> row, CancellationToken cancellation)
    {
        // PNG 9.2: eight independent byte sums, modulo256. Each vector contains
        // two pixels; the upper pixel first adds the lower, then both add carry.
        var shift = Vector128.Create((byte)255, 255, 255, 255, 255, 255, 255, 255, 0, 1, 2, 3, 4, 5, 6, 7);
        var repeat = Vector128.Create((byte)8, 9, 10, 11, 12, 13, 14, 15, 8, 9, 10, 11, 12, 13, 14, 15);
        var carry = Vector128<byte>.Zero; var i = 0;
        for (; i <= row.Length - 16; i += 16)
        {
            if ((i & 65535) == 0) cancellation.ThrowIfCancellationRequested();
            var value = Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(row), (nuint)i);
            value = value + Vector128.Shuffle(value, shift) + carry;
            value.StoreUnsafe(ref MemoryMarshal.GetReference(row), (nuint)i);
            carry = Vector128.Shuffle(value, repeat);
        }
        for (; i < row.Length; i++)
        {
            if ((i & 65535) == 0) cancellation.ThrowIfCancellationRequested();
            row[i] = unchecked((byte)(row[i] + (i >= 8 ? row[i - 8] : 0)));
        }
    }

    private static void UnfilterPaeth8(Span<byte> row, ReadOnlySpan<byte> previous, CancellationToken cancellation)
    {
        // PNG section 9.4: each of eight channel bytes depends only on its own
        // decoded left byte and the previous row. Distances fit signed 16 bits.
        var left = Vector128<short>.Zero; var upperLeft = Vector128<short>.Zero;
        var byteMask = Vector128.Create((ushort)255);
        var offset = 0; var end = row.Length & ~7;
        for (; offset < end; offset += 8)
        {
            if ((offset & 65535) == 0) cancellation.ThrowIfCancellationRequested();
            var up = Vector128.WidenLower(Vector128.Create(MemoryMarshal.Read<ulong>(previous[offset..]), 0UL).AsByte()).AsInt16();
            var delta = Vector128.WidenLower(Vector128.Create(MemoryMarshal.Read<ulong>(row[offset..]), 0UL).AsByte()).AsInt16();
            var a = Vector128.Abs(up - upperLeft);
            var b = Vector128.Abs(left - upperLeft);
            var c = Vector128.Abs(left + up - upperLeft - upperLeft);
            // Ties select left first, then up, then upper-left.
            var chooseLeft = Vector128.LessThanOrEqual(a, b) & Vector128.LessThanOrEqual(a, c);
            var prediction = Vector128.ConditionalSelect(chooseLeft, left,
                Vector128.ConditionalSelect(Vector128.LessThanOrEqual(b, c), up, upperLeft));
            left = ((prediction + delta).AsUInt16() & byteMask).AsInt16();
            var value = Vector128.Narrow(left.AsUInt16(), Vector128<ushort>.Zero).AsUInt64().GetElement(0);
            MemoryMarshal.Write(row[offset..], in value); upperLeft = up;
        }
        for (; offset < row.Length; offset++)
        {
            if ((offset & 65535) == 0) cancellation.ThrowIfCancellationRequested();
            row[offset] = unchecked((byte)(row[offset] + Paeth(offset >= 8 ? row[offset - 8] : 0,
                previous[offset], offset >= 8 ? previous[offset - 8] : 0)));
        }
    }

    private static void UnfilterPaeth4(Span<byte> row, ReadOnlySpan<byte> previous, CancellationToken cancellation)
    {
        // PNG section 9.4: each of four channel bytes depends only on its own
        // decoded left byte and the previous row. Distances fit signed 16 bits.
        var left = Vector128<short>.Zero; var upperLeft = Vector128<short>.Zero;
        var byteMask = Vector128.Create((ushort)255);
        var offset = 0; var end = row.Length & ~3;
        for (; offset < end; offset += 4)
        {
            if ((offset & 65535) == 0) cancellation.ThrowIfCancellationRequested();
            var up = Vector128.WidenLower(Vector128.Create(MemoryMarshal.Read<uint>(previous[offset..]), 0U, 0U, 0U).AsByte()).AsInt16();
            var delta = Vector128.WidenLower(Vector128.Create(MemoryMarshal.Read<uint>(row[offset..]), 0U, 0U, 0U).AsByte()).AsInt16();
            var a = Vector128.Abs(up - upperLeft);
            var b = Vector128.Abs(left - upperLeft);
            var c = Vector128.Abs(left + up - upperLeft - upperLeft);
            // Ties select left first, then up, then upper-left.
            var chooseLeft = Vector128.LessThanOrEqual(a, b) & Vector128.LessThanOrEqual(a, c);
            var prediction = Vector128.ConditionalSelect(chooseLeft, left,
                Vector128.ConditionalSelect(Vector128.LessThanOrEqual(b, c), up, upperLeft));
            left = ((prediction + delta).AsUInt16() & byteMask).AsInt16();
            var value = Vector128.Narrow(left.AsUInt16(), Vector128<ushort>.Zero).AsUInt32().GetElement(0);
            MemoryMarshal.Write(row[offset..], in value); upperLeft = up;
        }
        for (; offset < row.Length; offset++)
        {
            if ((offset & 65535) == 0) cancellation.ThrowIfCancellationRequested();
            row[offset] = unchecked((byte)(row[offset] + Paeth(offset >= 4 ? row[offset - 4] : 0,
                previous[offset], offset >= 4 ? previous[offset - 4] : 0)));
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
        // Optimal is a library policy, rather than a fixed zlib level. Byte
        // gray/RGB/gray-alpha need the BCL's deeper match search to retain
        // comparable compression. RGBA Fastest selects its streaming fast
        // backend below; other formats retain their existing selection.
        var compression = options.Compression == CompressionLevel.Optimal
            && source.BitDepth == 8 && color is 0 or 2 or 4
            ? CompressionLevel.SmallestSize : options.Compression;
        using (var chunks = new IdatWriter(output))
        {
            // The runtime's numeric level1 selects its fixed-tree quick path.
            // Numeric level2 selects fast adaptive coding, streaming through the
            // same bounded writer for ordinary, caller and cancelable outputs.
            using (var zlib = compression == CompressionLevel.Fastest && color == 6
                ? new ZLibStream(chunks, new ZLibCompressionOptions { CompressionLevel = 2 }, true)
                : new ZLibStream(chunks, compression, true))
            {
                for (var y = 0; y < source.Height; y++)
                {
                    context.Work(rowBytes * 8L);
                    if (color == 6 && source.BitDepth == 8)
                    {
                        for (var x = 0; x < source.Width; x += 4096)
                        {
                            context.Cancellation.ThrowIfCancellationRequested();
                            var bytes = Math.Min(4096, source.Width - x) * 4;
                            source.GetRow8(y).Slice(x * 4, bytes).CopyTo(row.AsSpan(x * 4, bytes));
                        }
                    }
                    else if (color == 6 && source.BitDepth == 16)
                    {
                        for (var x = 0; x < source.Width; x += 4096)
                        {
                            context.Cancellation.ThrowIfCancellationRequested();
                            var samples = Math.Min(4096, source.Width - x) * 4;
                            var input = source.GetRow16(y).Slice(x * 4, samples);
                            var destination = MemoryMarshal.Cast<byte, ushort>(row.AsSpan(x * 8, samples * 2));
                            // PNG stores full-word samples most significant byte first.
                            // The span overload provides portable vectorized conversion.
                            if (BitConverter.IsLittleEndian) BinaryPrimitives.ReverseEndianness(input, destination);
                            else input.CopyTo(destination);
                        }
                    }
                    else
                    {
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
