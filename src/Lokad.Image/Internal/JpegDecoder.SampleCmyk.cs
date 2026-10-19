using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal ref partial struct JpegDecoder
{
    private bool TryWriteFullResolutionCmyk(Span<byte> destination, int stride, bool invert)
    {
        if (components.Length != 4) return false;
        foreach (var component in components)
            if (component.H != maxH || component.V != maxV) return false;
        for (var y = 0; y < info.Height; y++)
        {
            context.Work(info.Width * 32L);
            PackCmykSamples(components[0].Plane.AsSpan(y * components[0].BlockColumns * 8, info.Width),
                components[1].Plane.AsSpan(y * components[1].BlockColumns * 8, info.Width),
                components[2].Plane.AsSpan(y * components[2].BlockColumns * 8, info.Width),
                components[3].Plane.AsSpan(y * components[3].BlockColumns * 8, info.Width), invert,
                destination.Slice(y * stride, info.Width * 4));
        }
        return true;
    }

    internal static void PackCmykSamples(ReadOnlySpan<byte> cyan, ReadOnlySpan<byte> magenta,
        ReadOnlySpan<byte> yellow, ReadOnlySpan<byte> black, bool invert, Span<byte> destination)
    {
        if (magenta.Length != cyan.Length || yellow.Length != cyan.Length || black.Length != cyan.Length || destination.Length != cyan.Length * 4L)
            throw new ArgumentException("CMYK planes must cover the same complete packed caller row.");
        // Equal sampling factors select exact integer byte samples. The old
        // interpolation/optional complement/away-from-zero rounding therefore
        // reduces exactly to byte packing and optional XOR on all four channels.
        var x = 0;
        if (Avx2.IsSupported)
        {
            var mask = Vector256.Create(invert ? (byte)255 : (byte)0);
            for (; x <= cyan.Length - 32; x += 32)
            {
                var c = Vector256.Create(cyan.Slice(x, 32)); var m = Vector256.Create(magenta.Slice(x, 32));
                var y = Vector256.Create(yellow.Slice(x, 32)); var k = Vector256.Create(black.Slice(x, 32));
                var cmLow = Avx2.UnpackLow(c, m); var cmHigh = Avx2.UnpackHigh(c, m);
                var ykLow = Avx2.UnpackLow(y, k); var ykHigh = Avx2.UnpackHigh(y, k);
                var a = Avx2.UnpackLow(cmLow.AsUInt16(), ykLow.AsUInt16()).AsByte();
                var b = Avx2.UnpackHigh(cmLow.AsUInt16(), ykLow.AsUInt16()).AsByte();
                var d = Avx2.UnpackLow(cmHigh.AsUInt16(), ykHigh.AsUInt16()).AsByte();
                var e = Avx2.UnpackHigh(cmHigh.AsUInt16(), ykHigh.AsUInt16()).AsByte();
                Avx2.Xor(Avx2.Permute2x128(a, b, 0x20), mask).CopyTo(destination.Slice(x * 4, 32));
                Avx2.Xor(Avx2.Permute2x128(d, e, 0x20), mask).CopyTo(destination.Slice(x * 4 + 32, 32));
                Avx2.Xor(Avx2.Permute2x128(a, b, 0x31), mask).CopyTo(destination.Slice(x * 4 + 64, 32));
                Avx2.Xor(Avx2.Permute2x128(d, e, 0x31), mask).CopyTo(destination.Slice(x * 4 + 96, 32));
            }
        }
        var flip = invert ? 255 : 0;
        for (; x < cyan.Length; x++)
        {
            destination[x * 4] = (byte)(cyan[x] ^ flip); destination[x * 4 + 1] = (byte)(magenta[x] ^ flip);
            destination[x * 4 + 2] = (byte)(yellow[x] ^ flip); destination[x * 4 + 3] = (byte)(black[x] ^ flip);
        }
    }
}
