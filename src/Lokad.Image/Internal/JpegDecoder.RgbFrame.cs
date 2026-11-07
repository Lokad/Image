using System.Buffers.Binary;
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
            for (var line = 0; line < visibleHeight; line++)
            {
                var offset = (by * horizontal + bx) * 64 + line * 8;
                var red = blocks.Slice(offset, visibleWidth);
                var green = blocks.Slice(groupBytes + offset, visibleWidth);
                var blue = blocks.Slice(groupBytes * 2 + offset, visibleWidth);
                var output = rgba.Slice((top + line) * stride + left * 4, visibleWidth * 4);
                if (visibleWidth == 8 && Avx2.IsSupported)
                {
                    var fourth = channels == 3 ? Vector256.Create(0xff000000u)
                        : Avx2.ShiftLeftLogical(WidenSamples8(blocks.Slice(groupBytes * 3 + offset, 8)).AsUInt32(), 24);
                    WriteComponents8(red, green, blue, fourth, output);
                }
                else if (channels == 3) RenderRgbRow(red, green, blue, output);
                else PackCmykSamples(red, green, blue, blocks.Slice(groupBytes * 3 + offset, visibleWidth), false, output);
            }
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
