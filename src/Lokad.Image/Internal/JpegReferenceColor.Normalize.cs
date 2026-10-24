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

    internal static void NormalizeRgba16(ReadOnlySpan<ushort> rgba, Span<int> rgb, uint? background)
    {
        if (rgba.Length % 4 != 0 || rgb.Length != rgba.Length / 4)
            throw new ArgumentException("Complete RGBA input and matching RGB output required.");
        const long denominator = 65535L * 65535;
        var color = background ?? 0;
        var red = (int)(color >> 16 & 255); var green = (int)(color >> 8 & 255); var blue = (int)(color & 255);
        var x = 0;
        if (Avx2.IsSupported && BitConverter.IsLittleEndian
            && !MemoryMarshal.AsBytes(rgba).Overlaps(MemoryMarshal.AsBytes(rgb)))
        {
            var backdrop = Vector256.Create((double)red, green, blue, 0);
            var maximum = Vector256.Create(65535.0);
            var scale = Vector256.Create(255.0);
            var half = Vector256.Create((double)(denominator / 2));
            var reciprocal = Vector256.Create(1.0 / denominator);
            for (; x + 2 <= rgb.Length; x += 2)
            {
                // Let the scalar path publish its original prefix before a
                // missing-background failure, even at the second pixel.
                if (background is null && (rgba[x * 4 + 3] != 65535 || rgba[x * 4 + 7] != 65535)) break;
                var words = MemoryMarshal.Read<Vector128<ushort>>(MemoryMarshal.AsBytes(rgba[(x * 4)..]));
                var wide = Avx2.ConvertToVector256Int32(words);
                var first = FlattenDoubleWords(Avx.ConvertToVector256Double(wide.GetLower()), backdrop, maximum, scale, half, reciprocal);
                var second = FlattenDoubleWords(Avx.ConvertToVector256Double(wide.GetUpper()), backdrop, maximum, scale, half, reciprocal);
                var packedWords = Sse2.PackSignedSaturate(first, second);
                var packed = Sse2.PackUnsignedSaturate(packedWords, Vector128<short>.Zero).AsInt32() & Vector128.Create(0xFFFFFF);
                rgb[x] = packed.GetElement(0); rgb[x + 1] = packed.GetElement(1);
            }
        }
        for (; x < rgb.Length; x++)
        {
            var alpha = rgba[x * 4 + 3];
            if (alpha != 65535 && background is null)
                throw new ArgumentException("JPEG requires opaque pixels or an explicit RGB background.", "image");
            var scaledAlpha = alpha * 255L;
            var inverseAlpha = (65535L - alpha) * 65535;
            var r = (int)((rgba[x * 4] * scaledAlpha + red * inverseAlpha + denominator / 2) / denominator);
            var g = (int)((rgba[x * 4 + 1] * scaledAlpha + green * inverseAlpha + denominator / 2) / denominator);
            var b = (int)((rgba[x * 4 + 2] * scaledAlpha + blue * inverseAlpha + denominator / 2) / denominator);
            rgb[x] = r | g << 8 | b << 16;
        }
    }

    // D=65535^2 is divisible by255; every numerator has residue127 modulo255.
    // All pre-reciprocal products/sums are exact binary64 integers below2^40.
    // Distance from an integer after division is >=127/D (>2.9e-8), while
    // reciprocal/product rounding error is <3*2^-46. Truncation is therefore
    // identical to integer division for every valid source/alpha/background.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<int> FlattenDoubleWords(Vector256<double> samples, Vector256<double> backdrop,
        Vector256<double> maximum, Vector256<double> scale, Vector256<double> half, Vector256<double> reciprocal)
    {
        var alpha = Avx2.Permute4x64(samples.AsInt64(), 255).AsDouble();
        var scaledAlpha = Avx.Multiply(alpha, scale);
        var inverseAlpha = Avx.Multiply(Avx.Subtract(maximum, alpha), maximum);
        var numerator = Avx.Add(Avx.Add(Avx.Multiply(samples, scaledAlpha), Avx.Multiply(backdrop, inverseAlpha)), half);
        return Avx.ConvertToVector128Int32WithTruncation(Avx.Multiply(numerator, reciprocal));
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
