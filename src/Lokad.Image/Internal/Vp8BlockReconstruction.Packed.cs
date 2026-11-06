using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal static partial class Vp8BlockReconstruction
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void PackedLine16(Vector128<short> zero, Vector128<short> first,
        Vector128<short> second, Vector128<short> third, out Vector128<short> a,
        out Vector128<short> b, out Vector128<short> c, out Vector128<short> d)
    {
        // For every signed16 x, floor(x*35468/65536) is exactly
        // x + floor(x*(-30068)/65536). No imported implementation table.
        var sine = Vector128.Create((short)-30068); var cosine = Vector128.Create((short)20091);
        var sum = Sse2.Add(zero, second); var difference = Sse2.Subtract(zero, second);
        var firstSine = Sse2.Add(first, Sse2.MultiplyHigh(first, sine));
        var thirdSine = Sse2.Add(third, Sse2.MultiplyHigh(third, sine));
        var oddDifference = Sse2.Subtract(Sse2.Subtract(firstSine, third), Sse2.MultiplyHigh(third, cosine));
        var oddSum = Sse2.Add(Sse2.Add(first, Sse2.MultiplyHigh(first, cosine)), thirdSine);
        a = Sse2.Add(sum, oddSum); b = Sse2.Add(difference, oddDifference);
        c = Sse2.Subtract(difference, oddDifference); d = Sse2.Subtract(sum, oddSum);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void PackedTranspose16(Vector128<short> a, Vector128<short> b,
        Vector128<short> c, Vector128<short> d, out Vector128<short> first, out Vector128<short> second)
    {
        var ab = Sse2.UnpackLow(a, b).AsInt32(); var cd = Sse2.UnpackLow(c, d).AsInt32();
        first = Sse2.UnpackLow(ab, cd).AsInt16(); second = Sse2.UnpackHigh(ab, cd).AsInt16();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<short> UpperPackedRow(Vector128<short> rows)
        => Sse2.ShiftRightLogical128BitLane(rows.AsByte(), 8).AsInt16();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WritePackedRow(Vector128<short> residual, Span<byte> row)
    {
        var prediction = Vector128.CreateScalar(BinaryPrimitives.ReadUInt32LittleEndian(row)).AsByte();
        var value = Sse2.Add(residual, Sse2.UnpackLow(prediction, Vector128<byte>.Zero).AsInt16());
        BinaryPrimitives.WriteUInt32LittleEndian(row, Sse2.PackUnsignedSaturate(value, Vector128<short>.Zero).AsUInt32().GetElement(0));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryInversePacked16(Vector128<short> low, Vector128<short> high,
        out Vector128<short> first, out Vector128<short> second)
    {
        // Arithmetic selection only. With |input|<=2047, first-pass values are
        // <=7877 and second-pass values <=30310, including final +4. Mandatory
        // first-pass signed16 wrapping is an identity in this bounded domain.
        var outside = Sse2.Or(Sse2.CompareGreaterThan(Sse2.Max(low, high), Vector128.Create((short)2047)),
            Sse2.CompareGreaterThan(Vector128.Create((short)-2047), Sse2.Min(low, high)));
        if (Sse2.MoveMask(outside.AsByte()) != 0) { first = second = default; return false; }
        PackedLine16(low, UpperPackedRow(low), high, UpperPackedRow(high), out var a, out var b, out var c, out var d);
        PackedTranspose16(a, b, c, d, out first, out second);
        PackedLine16(first, UpperPackedRow(first), second, UpperPackedRow(second), out a, out b, out c, out d);
        PackedTranspose16(a, b, c, d, out first, out second);
        var rounding = Vector128.Create((short)4);
        first = Sse2.ShiftRightArithmetic(Sse2.Add(first, rounding), 3);
        second = Sse2.ShiftRightArithmetic(Sse2.Add(second, rounding), 3);
        return true;
    }
}
