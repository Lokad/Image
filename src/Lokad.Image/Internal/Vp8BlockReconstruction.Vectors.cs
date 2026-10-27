using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal static partial class Vp8BlockReconstruction
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<int> Row(ReadOnlySpan<short> values)
        => Sse41.ConvertToVector128Int32(Vector128.Create(
            MemoryMarshal.Read<Vector64<short>>(MemoryMarshal.AsBytes(values)), Vector64<short>.Zero));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Line(Vector128<int> zero, Vector128<int> first, Vector128<int> second, Vector128<int> third,
        out Vector128<int> a, out Vector128<int> b, out Vector128<int> c, out Vector128<int> d)
    {
        // RFC6386 section14.4: the same integer approximations as DctLine.
        var sine = Vector128.Create(35468); var cosine = Vector128.Create(20091);
        var sum = Sse2.Add(zero, second); var difference = Sse2.Subtract(zero, second);
        var oddDifference = Sse2.Subtract(Sse2.Subtract(
            Sse2.ShiftRightArithmetic(Sse41.MultiplyLow(first, sine), 16), third),
            Sse2.ShiftRightArithmetic(Sse41.MultiplyLow(third, cosine), 16));
        var oddSum = Sse2.Add(Sse2.Add(first,
            Sse2.ShiftRightArithmetic(Sse41.MultiplyLow(first, cosine), 16)),
            Sse2.ShiftRightArithmetic(Sse41.MultiplyLow(third, sine), 16));
        a = Sse2.Add(sum, oddSum); b = Sse2.Add(difference, oddDifference);
        c = Sse2.Subtract(difference, oddDifference); d = Sse2.Subtract(sum, oddSum);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<int> Wrap16(Vector128<int> value)
        => Sse2.ShiftRightArithmetic(Sse2.ShiftLeftLogical(value, 16), 16);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Transpose(ref Vector128<int> a, ref Vector128<int> b, ref Vector128<int> c, ref Vector128<int> d)
    {
        var ab0 = Sse2.UnpackLow(a, b); var ab1 = Sse2.UnpackHigh(a, b);
        var cd0 = Sse2.UnpackLow(c, d); var cd1 = Sse2.UnpackHigh(c, d);
        a = Sse2.UnpackLow(ab0.AsInt64(), cd0.AsInt64()).AsInt32();
        b = Sse2.UnpackHigh(ab0.AsInt64(), cd0.AsInt64()).AsInt32();
        c = Sse2.UnpackLow(ab1.AsInt64(), cd1.AsInt64()).AsInt32();
        d = Sse2.UnpackHigh(ab1.AsInt64(), cd1.AsInt64()).AsInt32();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Write(Vector128<int> residual, Span<byte> row)
    {
        // After the mandatory first-pass signed16 wrap, the second-pass
        // coefficient L1 bound is <4*32768. Division by8 therefore fits
        // signed16, also after adding a byte prediction. Saturation here is
        // solely the original final pixel clamp, never a coefficient change.
        residual = Sse2.ShiftRightArithmetic(Sse2.Add(residual, Vector128.Create(4)), 3);
        var prediction = Vector128.CreateScalar(BinaryPrimitives.ReadUInt32LittleEndian(row)).AsByte();
        var values = Sse2.Add(Sse2.PackSignedSaturate(residual, Vector128<int>.Zero),
            Sse2.UnpackLow(prediction, Vector128<byte>.Zero).AsInt16());
        var pixels = Sse2.PackUnsignedSaturate(values, Vector128<short>.Zero);
        BinaryPrimitives.WriteUInt32LittleEndian(row, pixels.AsUInt32().GetElement(0));
    }

    // All sixteen coefficients are loaded before any pixel publication. Pair
    // callers additionally snapshot both blocks before reconstructing either.
    private static void AddVector(ReadOnlySpan<short> coefficients, Span<byte> pixels, int stride)
    {
        Line(Row(coefficients[..4]), Row(coefficients.Slice(4, 4)), Row(coefficients.Slice(8, 4)), Row(coefficients.Slice(12, 4)),
            out var a, out var b, out var c, out var d);
        a = Wrap16(a); b = Wrap16(b); c = Wrap16(c); d = Wrap16(d);
        Transpose(ref a, ref b, ref c, ref d);
        Line(a, b, c, d, out a, out b, out c, out d);
        Transpose(ref a, ref b, ref c, ref d);
        if (stride == 4)
        {
            // Compact RDO predictions occupy one complete vector. Round and
            // clamp in the same order as Write, using one pixel load/store.
            var rounding = Vector128.Create(4);
            a = Sse2.ShiftRightArithmetic(Sse2.Add(a, rounding), 3);
            b = Sse2.ShiftRightArithmetic(Sse2.Add(b, rounding), 3);
            c = Sse2.ShiftRightArithmetic(Sse2.Add(c, rounding), 3);
            d = Sse2.ShiftRightArithmetic(Sse2.Add(d, rounding), 3);
            var prediction = Vector128.Create(pixels[..16]);
            var first = Sse2.Add(Sse2.PackSignedSaturate(a, b),
                Sse2.UnpackLow(prediction, Vector128<byte>.Zero).AsInt16());
            var second = Sse2.Add(Sse2.PackSignedSaturate(c, d),
                Sse2.UnpackHigh(prediction, Vector128<byte>.Zero).AsInt16());
            Sse2.PackUnsignedSaturate(first, second).CopyTo(pixels);
            return;
        }
        Write(a, pixels[..4]); Write(b, pixels.Slice(stride, 4));
        Write(c, pixels.Slice(2 * stride, 4)); Write(d, pixels.Slice(3 * stride, 4));
    }
}
