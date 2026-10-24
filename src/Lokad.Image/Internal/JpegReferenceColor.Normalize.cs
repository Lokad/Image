using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal static partial class JpegReferenceColor
{
    internal static void NormalizeRgba8(ReadOnlySpan<byte> rgba, Span<byte> rgb, uint? background)
    {
        if (rgba.Length % 4 != 0 || rgb.Length != rgba.Length / 4 * 3)
            throw new ArgumentException("Complete RGBA input and matching RGB output required.");
        var color = background ?? 0;
        var red = (int)(color >> 16 & 255); var green = (int)(color >> 8 & 255); var blue = (int)(color & 255);
        var x = 0; var count = rgb.Length / 3;
        if (Ssse3.IsSupported && BitConverter.IsLittleEndian)
        {
            var zero = Vector128<byte>.Zero;
            var backgroundBytes = Vector128.Create((uint)(red | green << 8 | blue << 16)).AsByte();
            var backgroundWords = Sse2.UnpackLow(backgroundBytes, zero).AsUInt16();
            var alphaMask = Vector128.Create(0xFF000000u).AsByte();
            var alphaSpread = Vector128.Create((byte)3, 3, 3, 3, 7, 7, 7, 7, 11, 11, 11, 11, 15, 15, 15, 15);
            var compact = Vector128.Create((byte)0, 1, 2, 4, 5, 6, 8, 9, 10, 12, 13, 14, 128, 128, 128, 128);
            for (; x + 4 <= count; x += 4)
            {
                var pixels = MemoryMarshal.Read<Vector128<byte>>(rgba[(x * 4)..]);
                if (background is null && Sse2.MoveMask(Sse2.CompareEqual(pixels & alphaMask, alphaMask)) != 65535)
                    throw new ArgumentException("JPEG requires opaque pixels or an explicit RGB background.", "image");
                var alpha = Ssse3.Shuffle(pixels, alphaSpread);
                var low = FlattenWords(Sse2.UnpackLow(pixels, zero).AsUInt16(), Sse2.UnpackLow(alpha, zero).AsUInt16(), backgroundWords);
                var high = FlattenWords(Sse2.UnpackHigh(pixels, zero).AsUInt16(), Sse2.UnpackHigh(alpha, zero).AsUInt16(), backgroundWords);
                var packed = Ssse3.Shuffle(Sse2.PackUnsignedSaturate(low.AsInt16(), high.AsInt16()), compact);
                var first = packed.AsUInt64().GetElement(0); var last = packed.AsUInt32().GetElement(2);
                MemoryMarshal.Write(rgb.Slice(x * 3, 8), in first);
                MemoryMarshal.Write(rgb.Slice(x * 3 + 8, 4), in last);
            }
        }
        for (; x < count; x++)
        {
            var alpha = rgba[x * 4 + 3];
            if (alpha != 255 && background is null)
                throw new ArgumentException("JPEG requires opaque pixels or an explicit RGB background.", "image");
            rgb[x * 3] = (byte)JpegEncoder.Flatten8(rgba[x * 4], alpha, red);
            rgb[x * 3 + 1] = (byte)JpegEncoder.Flatten8(rgba[x * 4 + 1], alpha, green);
            rgb[x * 3 + 2] = (byte)JpegEncoder.Flatten8(rgba[x * 4 + 2], alpha, blue);
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
