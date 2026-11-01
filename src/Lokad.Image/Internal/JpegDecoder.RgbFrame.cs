using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal ref partial struct JpegDecoder
{
    // Final IDCT bytes go straight to their channel in the privately owned
    // equal-grid RGB result. Channel two also writes opaque alpha. The complete
    // container and all three scans must succeed before this image can escape.
    internal static void StoreRgbBlock(ReadOnlySpan<byte> pixels, Span<byte> rgba,
        int stride, int width, int height, int channel)
    {
        if (width is < 1 or > 8 || height is < 1 or > 8 || (uint)channel > 2 || stride < width * 4)
            throw new ArgumentOutOfRangeException(nameof(width));
        if (pixels.Length < 64 || (long)(height - 1) * stride + width * 4 > rgba.Length)
            throw new ArgumentException("RGB block storage must cover the visible rows.");
        if (width == 8 && Avx2.IsSupported)
        {
            var shift = channel * 8;
            var counts = Vector256.Create((uint)shift);
            var mask = Vector256.Create(~(255u << shift));
            var alpha = Vector256.Create(channel == 2 ? 0xff000000u : 0u);
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
            if (channel == 2) rgba[y * stride + x * 4 + 3] = 255;
        }
    }
}
