using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal ref partial struct JpegDecoder
{
    // Complete equal-grid MCU: all final IDCT groups are present. Pack
    // each visible row once, preserving the same byte component core and scalar tail.
    internal static void StoreComponentMcu(ReadOnlySpan<byte> blocks, Span<byte> rgba,
        int stride, int width, int height, int horizontal, int vertical, int channels)
    {
        if (channels is not (3 or 4) || horizontal is < 1 or > 4 || vertical is < 1 or > 4 || width < 1 || width > horizontal * 8
            || height < 1 || height > vertical * 8 || stride < width * 4)
            throw new ArgumentOutOfRangeException(nameof(width));
        var groupBytes = horizontal * vertical * 64;
        if (blocks.Length < groupBytes * channels || (long)(height - 1) * stride + width * 4 > rgba.Length)
            throw new ArgumentException("Component MCU storage must cover every component and visible row.");
        for (var by = 0; by < vertical; by++)
        for (var bx = 0; bx < horizontal; bx++)
        {
            var left = bx * 8; var top = by * 8;
            var visibleWidth = Math.Min(8, width - left); var visibleHeight = Math.Min(8, height - top);
            if (visibleWidth <= 0 || visibleHeight <= 0) continue;
            if (visibleWidth == 8 && Sse2.IsSupported)
            {
                StoreComponentRows(blocks, rgba.Slice(top * stride + left * 4,
                    (visibleHeight - 1) * stride + 32), groupBytes, stride, visibleHeight,
                    (by * horizontal + bx) * 64, channels);
                continue;
            }
            for (var line = 0; line < visibleHeight; line++)
            {
                var offset = (by * horizontal + bx) * 64 + line * 8;
                var red = blocks.Slice(offset, visibleWidth);
                var green = blocks.Slice(groupBytes + offset, visibleWidth);
                var blue = blocks.Slice(groupBytes * 2 + offset, visibleWidth);
                var output = rgba.Slice((top + line) * stride + left * 4, visibleWidth * 4);
                if (channels == 3) RenderRgbRow(red, green, blue, output);
                else PackCmykSamples(red, green, blue, blocks.Slice(groupBytes * 3 + offset, visibleWidth), false, output);
            }
        }
    }

    // Validate and prepare each component view once per tile, keeping its bytes
    // packed through publication. The caller has admitted the disjoint stack
    // staging; raw K remains data until complete container interpretation.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void StoreComponentRows(ReadOnlySpan<byte> blocks, Span<byte> rgba,
        int groupBytes, int stride, int height, int offset, int channels)
    {
        // IDCT initializes the entire tile, including rows outside the visible
        // bottom edge. Read two bounded rows together; publish only visible rows.
        var pairs = (height + 1) / 2;
        var red = MemoryMarshal.Cast<byte, Vector128<byte>>(blocks.Slice(offset, 64)).Slice(0, pairs);
        var green = MemoryMarshal.Cast<byte, Vector128<byte>>(blocks.Slice(groupBytes + offset, 64)).Slice(0, pairs);
        var blue = MemoryMarshal.Cast<byte, Vector128<byte>>(blocks.Slice(groupBytes * 2 + offset, 64)).Slice(0, pairs);
        var fourth = channels == 4
            ? MemoryMarshal.Cast<byte, Vector128<byte>>(blocks.Slice(groupBytes * 3 + offset, 64)).Slice(0, pairs) : red;
        for (var pair = 0; pair < red.Length; pair++)
        {
            var r = red[pair]; var g = green[pair]; var b = blue[pair];
            var alpha = channels == 4 ? fourth[pair] : Vector128.Create(byte.MaxValue);
            var rg = Sse2.UnpackLow(r, g).AsUInt16();
            var ba = Sse2.UnpackLow(b, alpha).AsUInt16();
            var low = Sse2.UnpackLow(rg, ba).AsByte();
            var high = Sse2.UnpackHigh(rg, ba).AsByte();
            var line = pair * 2;
            var output = rgba.Slice(line * stride, 32);
            MemoryMarshal.Write(output, in low);
            MemoryMarshal.Write(output.Slice(16), in high);
            if (line + 1 == height) break;
            rg = Sse2.UnpackHigh(r, g).AsUInt16();
            ba = Sse2.UnpackHigh(b, alpha).AsUInt16();
            low = Sse2.UnpackLow(rg, ba).AsByte();
            high = Sse2.UnpackHigh(rg, ba).AsByte();
            output = rgba.Slice((line + 1) * stride, 32);
            MemoryMarshal.Write(output, in low);
            MemoryMarshal.Write(output.Slice(16), in high);
        }
    }

    // Final IDCT bytes go straight to their channel in the privately owned
    // equal-grid result. RGB channel two also writes opaque alpha; four-component
    // data keeps K unchanged until final color conversion. The complete container
    // and all component scans must succeed before this image can escape.
    internal static void StoreComponentBlock(ReadOnlySpan<byte> pixels, Span<byte> rgba,
        int stride, int width, int height, int channel, int channels)
    {
        if (width is < 1 or > 8 || height is < 1 or > 8 || channels is not (3 or 4) || (uint)channel >= channels || stride < width * 4)
            throw new ArgumentOutOfRangeException(nameof(width));
        if (pixels.Length < 64 || (long)(height - 1) * stride + width * 4 > rgba.Length)
            throw new ArgumentException("Component block storage must cover the visible rows.");
        if (width == 8 && Avx2.IsSupported)
        {
            var shift = channel * 8;
            var counts = Vector256.Create((uint)shift);
            var mask = Vector256.Create(~(255u << shift));
            var alpha = Vector256.Create(channels == 3 && channel == 2 ? 0xff000000u : 0u);
            for (var y = 0; y < height; y++)
            {
                var samples = BinaryPrimitives.ReadUInt64LittleEndian(pixels.Slice(y * 8, 8));
                var values = Avx2.ConvertToVector256Int32(Vector128.CreateScalar(samples).AsByte()).AsUInt32();
                var row = rgba.Slice(y * stride, 32);
                var previous = MemoryMarshal.Read<Vector256<uint>>(row);
                var result = (previous & mask) | Avx2.ShiftLeftLogicalVariable(values, counts) | alpha;
                MemoryMarshal.Write(row, in result);
            }
            return;
        }
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            rgba[y * stride + x * 4 + channel] = pixels[y * 8 + x];
            if (channels == 3 && channel == 2) rgba[y * stride + x * 4 + 3] = 255;
        }
    }
}
