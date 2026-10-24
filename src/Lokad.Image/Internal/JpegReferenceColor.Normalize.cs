using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal static partial class JpegReferenceColor
{
    internal static void NormalizeRgba8(ReadOnlySpan<byte> rgba, Span<int> rgb, uint? background)
    {
        if (rgba.Length % 4 != 0 || rgb.Length != rgba.Length / 4)
            throw new ArgumentException("Complete RGBA input and matching RGB output required.");
        var color = background ?? 0;
        var red = (int)(color >> 16 & 255); var green = (int)(color >> 8 & 255); var blue = (int)(color & 255);
        var x = 0; var count = rgb.Length;
        if (Sse2.IsSupported && BitConverter.IsLittleEndian)
        {
            var zero = Vector128<byte>.Zero;
            var backgroundBytes = Vector128.Create((uint)(red | green << 8 | blue << 16)).AsByte();
            var backgroundWords = Sse2.UnpackLow(backgroundBytes, zero).AsUInt16();
            var alphaMask = Vector128.Create(0xFF000000u).AsByte();
            for (; x + 4 <= count; x += 4)
            {
                var pixels = MemoryMarshal.Read<Vector128<byte>>(rgba[(x * 4)..]);
                if (background is null && Sse2.MoveMask(Sse2.CompareEqual(pixels & alphaMask, alphaMask)) != 65535)
                    throw new ArgumentException("JPEG requires opaque pixels or an explicit RGB background.", "image");
                var lowPixels = Sse2.UnpackLow(pixels, zero).AsUInt16();
                var highPixels = Sse2.UnpackHigh(pixels, zero).AsUInt16();
                var lowAlpha = Sse2.ShuffleHigh(Sse2.ShuffleLow(lowPixels, 255), 255);
                var highAlpha = Sse2.ShuffleHigh(Sse2.ShuffleLow(highPixels, 255), 255);
                var low = FlattenWords(lowPixels, lowAlpha, backgroundWords);
                var high = FlattenWords(highPixels, highAlpha, backgroundWords);
                var packed = Sse2.PackUnsignedSaturate(low.AsInt16(), high.AsInt16()).AsInt32()
                    & Vector128.Create(0xFFFFFF);
                packed.CopyTo(rgb[x..]);
            }
        }
        for (; x < count; x++)
        {
            var alpha = rgba[x * 4 + 3];
            if (alpha != 255 && background is null)
                throw new ArgumentException("JPEG requires opaque pixels or an explicit RGB background.", "image");
            rgb[x] = JpegEncoder.Flatten8(rgba[x * 4], alpha, red)
                | JpegEncoder.Flatten8(rgba[x * 4 + 1], alpha, green) << 8
                | JpegEncoder.Flatten8(rgba[x * 4 + 2], alpha, blue) << 16;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<ushort> FlattenWords(Vector128<ushort> pixels, Vector128<ushort> alpha, Vector128<ushort> background)
    {
        // The nonnegative numerator is at most255*255+127=65152. For this
        // domain floor(n/255) is exactly (n+1+(n>>8))>>8, without overflow.
        var numerator = Sse2.MultiplyLow(pixels, alpha)
            + Sse2.MultiplyLow(background, Vector128.Create((ushort)255) - alpha) + Vector128.Create((ushort)127);
        return Sse2.ShiftRightLogical(numerator + Vector128.Create((ushort)1) + Sse2.ShiftRightLogical(numerator, 8), 8);
    }
}
