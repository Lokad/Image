using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal static partial class Vp8BlockReconstruction
{
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

    // Both compact candidates and genuine strided callers share exactly the
    // same packed fast path and full-range first-pass wrap/rounding fallback.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<short> InverseValues(Vector256<short> coefficients)
    {
        var low = coefficients.GetLower(); var high = coefficients.GetUpper();
        if (TryInversePacked16(low, high, out var first, out var second)) return Vector256.Create(first, second);
        return InverseWideValues(coefficients);
    }

    // Keep the full-range path out of each hot ordered candidate search. A
    // value return avoids exposing hot packed-result locals across this call.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Vector256<short> InverseWideValues(Vector256<short> coefficients)
    {
        var low = coefficients.GetLower(); var high = coefficients.GetUpper();
        Line(Sse41.ConvertToVector128Int32(low), Sse41.ConvertToVector128Int32(UpperPackedRow(low)),
            Sse41.ConvertToVector128Int32(high), Sse41.ConvertToVector128Int32(UpperPackedRow(high)),
            out var a, out var b, out var c, out var d);
        a = Wrap16(a); b = Wrap16(b); c = Wrap16(c); d = Wrap16(d);
        Transpose(ref a, ref b, ref c, ref d);
        Line(a, b, c, d, out a, out b, out c, out d);
        Transpose(ref a, ref b, ref c, ref d);
        var rounding = Vector128.Create(4);
        a = Sse2.ShiftRightArithmetic(Sse2.Add(a, rounding), 3);
        b = Sse2.ShiftRightArithmetic(Sse2.Add(b, rounding), 3);
        c = Sse2.ShiftRightArithmetic(Sse2.Add(c, rounding), 3);
        d = Sse2.ShiftRightArithmetic(Sse2.Add(d, rounding), 3);
        // After mandatory first-pass wrap, second-pass L1<4*32768. Rounded
        // residuals and their byte predictions fit signed16; only pixels clamp.
        return Vector256.Create(Sse2.PackSignedSaturate(a, b), Sse2.PackSignedSaturate(c, d));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> AddPrediction(Vector128<short> first, Vector128<short> second, Vector128<byte> prediction)
    {
        first = Sse2.Add(first, Sse2.UnpackLow(prediction, Vector128<byte>.Zero).AsInt16());
        second = Sse2.Add(second, Sse2.UnpackHigh(prediction, Vector128<byte>.Zero).AsInt16());
        return Sse2.PackUnsignedSaturate(first, second);
    }

    // Caller guards SSE4.1; fixed value inputs cannot alias later publication.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<byte> AddValue(Vector256<short> coefficients, Vector128<byte> prediction)
    {
        var residual = InverseValues(coefficients);
        return AddPrediction(residual.GetLower(), residual.GetUpper(), prediction);
    }

    // Load every coefficient before publication, including aliased span callers.
    private static void AddVector(ReadOnlySpan<short> coefficients, Span<byte> pixels, int stride)
    {
        var residual = InverseValues(Vector256.Create(coefficients));
        var first = residual.GetLower(); var second = residual.GetUpper();
        if (stride == 4)
        {
            AddPrediction(first, second, Vector128.Create(pixels[..16])).CopyTo(pixels);
            return;
        }
        WritePackedRow(first, pixels[..4]); WritePackedRow(UpperPackedRow(first), pixels.Slice(stride, 4));
        WritePackedRow(second, pixels.Slice(2 * stride, 4)); WritePackedRow(UpperPackedRow(second), pixels.Slice(3 * stride, 4));
    }
}
