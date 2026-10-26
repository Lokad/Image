using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

// Shared exact rounding and guarded SIMD publication. Strip/cache kernels keep
// their own geometry, tap order and portable fallback selection.
internal static class BicubicMath
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool TryWriteBytePair(Vector512<double> sum, double n0, double n1, Span<byte> output)
    {
        const double unitError = 2.3283064365386963e-10;
        const double maximum = 255d;
        const double alphaError = unitError * maximum;
        var a0 = sum.GetElement(3); var a1 = sum.GetElement(7);
        if (!(n0 >= .5 && n1 >= .5 && a0 > alphaError && a1 > alphaError)) return false;
        var divisors = Vector512.Create(a0, a0, a0, n0, a1, a1, a1, n1);
        var values = Avx512F.Divide(sum, divisors);
        // With 2^-40*16^2 and maximum 255, quotient bounds at every
        // in-range threshold are below 1e-6 for alpha and 1e-4/alpha
        // for color. The quarter-margin guard protects clipping thresholds.
        // Alpha's lower power of two gives a conservative reciprocal bound.
        var exponent0 = (int)((BitConverter.DoubleToUInt64Bits(a0) >> 52) & 2047);
        var exponent1 = (int)((BitConverter.DoubleToUInt64Bits(a1) >> 52) & 2047);
        var margin0 = 1e-4 * BitConverter.UInt64BitsToDouble((ulong)(2046 - exponent0) << 52);
        var margin1 = 1e-4 * BitConverter.UInt64BitsToDouble((ulong)(2046 - exponent1) << 52);
        if (margin0 >= .25 || margin1 >= .25
            || !(values.GetElement(3) >= 1 + 1e-6 && values.GetElement(7) >= 1 + 1e-6)) return false;
        var margins = Vector512.Create(margin0, margin0, margin0, 1e-6, margin1, margin1, margin1, 1e-6);
        var fractional = Avx512F.Subtract(values, Avx512F.RoundScale(values, 1));
        var distance = Avx512F.AndNot(Vector512.Create(long.MinValue),
            Avx512F.Subtract(fractional, Vector512.Create(.5)).AsInt64()).AsDouble();
        var inRange = Avx512F.And(Avx512F.CompareGreaterThanOrEqual(values, Vector512<double>.Zero).AsInt64(),
            Avx512F.CompareLessThanOrEqual(values, Vector512.Create(maximum)).AsInt64());
        var ambiguous = Avx512F.And(inRange, Avx512F.CompareLessThanOrEqual(distance, margins).AsInt64());
        if (Vector512.ExtractMostSignificantBits(ambiguous) != 0) return false;
        var clipped = Avx512F.Min(Vector512.Create(maximum), Avx512F.Max(Vector512<double>.Zero, values));
        var ints = Avx512F.ConvertToVector256Int32WithTruncation(Avx512F.RoundScale(clipped, 0));
        var shorts = Sse2.PackSignedSaturate(ints.GetLower(), ints.GetUpper());
        var packed = Sse2.PackUnsignedSaturate(shorts, Vector128<short>.Zero).AsUInt64().GetElement(0);
        BinaryPrimitives.WriteUInt64LittleEndian(output, packed);
        return true;
    }

    internal static bool TryWriteByte(Vector256<double> sum, double normalizer, Span<byte> output)
    {
        const double unitError = 2.3283064365386963e-10;
        const double maximum = 255d;
        const double alphaError = unitError * maximum;
        var alpha = sum.GetElement(3);
        if (!(normalizer >= .5 && alpha > alphaError)) return false;
        var values = Avx.Divide(sum, Vector256.Create(alpha, alpha, alpha, normalizer));
        var magnitude = Math.Max(Math.Abs(values.GetElement(0)),
            Math.Max(Math.Abs(values.GetElement(1)), Math.Abs(values.GetElement(2))));
        var colorMargin = 2 * (alphaError * maximum + magnitude * alphaError) / (alpha - alphaError);
        var alphaMargin = 2 * (alphaError + Math.Abs(values.GetElement(3)) * unitError) / (normalizer - unitError);
        if (colorMargin >= .25 || !(values.GetElement(3) >= 1 + alphaMargin)) return false;
        for (var c = 0; c < 4; c++)
        {
            var value = values.GetElement(c);
            if (value >= 0 && value <= maximum
                && Math.Abs(value - Math.Floor(value) - .5) <= (c == 3 ? alphaMargin : colorMargin)) return false;
        }
        var clipped = Avx.Min(Vector256.Create(maximum), Avx.Max(Vector256<double>.Zero, values));
        var ints = Avx.ConvertToVector128Int32WithTruncation(Avx.RoundToNearestInteger(clipped));
        var shorts = Sse2.PackSignedSaturate(ints, Vector128<int>.Zero);
        var packed = Sse2.PackUnsignedSaturate(shorts, Vector128<short>.Zero).AsUInt32().GetElement(0);
        BinaryPrimitives.WriteUInt32LittleEndian(output, packed);
        return true;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool TryWriteWordPair(Vector512<double> sum, double n0, double n1, Span<byte> output)
    {
        const double unitError = 2.3283064365386963e-10;
        const double maximum = 65535d;
        const double alphaError = unitError * maximum;
        var a0 = sum.GetElement(3); var a1 = sum.GetElement(7);
        if (!(n0 >= .5 && n1 >= .5 && a0 > alphaError && a1 > alphaError)) return false;
        var divisors = Vector512.Create(a0, a0, a0, n0, a1, a1, a1, n1);
        var values = Avx512F.Divide(sum, divisors);
        // Established 2^-40*16^2 word bounds cover every in-range
        // rounding threshold: alpha margin 1e-4, color below 2.0001/alpha.
        // Alpha's lower power of two gives a conservative reciprocal bound.
        var exponent0 = (int)((BitConverter.DoubleToUInt64Bits(a0) >> 52) & 2047);
        var exponent1 = (int)((BitConverter.DoubleToUInt64Bits(a1) >> 52) & 2047);
        var margin0 = 2.0001 * BitConverter.UInt64BitsToDouble((ulong)(2046 - exponent0) << 52);
        var margin1 = 2.0001 * BitConverter.UInt64BitsToDouble((ulong)(2046 - exponent1) << 52);
        if (margin0 >= .25 || margin1 >= .25
            || !(values.GetElement(3) >= 1 + 1e-4 && values.GetElement(7) >= 1 + 1e-4)) return false;
        var margins = Vector512.Create(margin0, margin0, margin0, 1e-4, margin1, margin1, margin1, 1e-4);
        var fractional = Avx512F.Subtract(values, Avx512F.RoundScale(values, 1));
        var distance = Avx512F.AndNot(Vector512.Create(long.MinValue),
            Avx512F.Subtract(fractional, Vector512.Create(.5)).AsInt64()).AsDouble();
        var inRange = Avx512F.And(Avx512F.CompareGreaterThanOrEqual(values, Vector512<double>.Zero).AsInt64(),
            Avx512F.CompareLessThanOrEqual(values, Vector512.Create(maximum)).AsInt64());
        var ambiguous = Avx512F.And(inRange, Avx512F.CompareLessThanOrEqual(distance, margins).AsInt64());
        if (Vector512.ExtractMostSignificantBits(ambiguous) != 0) return false;
        var clipped = Avx512F.Min(Vector512.Create(maximum), Avx512F.Max(Vector512<double>.Zero, values));
        var ints = Avx512F.ConvertToVector256Int32WithTruncation(Avx512F.RoundScale(clipped, 0));
        var words = Sse41.PackUnsignedSaturate(ints.GetLower(), ints.GetUpper());
        MemoryMarshal.Write(output, in words);
        return true;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool TryWriteWordPairWide(Vector512<double> sum, double n0, double n1, Span<byte> output)
    {
        const double unitError = 2.3283064365386963e-10;
        const double maximum = 65535d;
        const double alphaError = unitError * maximum;
        var a0 = sum.GetElement(3); var a1 = sum.GetElement(7);
        if (!(n0 >= .5 && n1 >= .5 && a0 > alphaError && a1 > alphaError)) return false;
        var divisors = Vector512.Create(a0, a0, a0, n0, a1, a1, a1, n1);
        var values = Avx512F.Divide(sum, divisors);
        var magnitudes = Avx512F.AndNot(Vector512.Create(long.MinValue), values.AsInt64()).AsDouble();
        // Per-channel quotient bounds avoid scalar maximum/division chains.
        // Doubling covers rounding of the positive error-bound calculation.
        var errors = Vector512.Create(alphaError, alphaError, alphaError, unitError,
            alphaError, alphaError, alphaError, unitError);
        var numerators = Avx512F.Add(Vector512.Create(alphaError * maximum, alphaError * maximum, alphaError * maximum, alphaError,
            alphaError * maximum, alphaError * maximum, alphaError * maximum, alphaError), Avx512F.Multiply(magnitudes, errors));
        var margins = Avx512F.Divide(Avx512F.Multiply(numerators, Vector512.Create(2d)), Avx512F.Subtract(divisors, errors));
        if (Vector512.ExtractMostSignificantBits(Avx512F.CompareGreaterThanOrEqual(margins, Vector512.Create(.25))) != 0
            || !(values.GetElement(3) >= 1 + margins.GetElement(3) && values.GetElement(7) >= 1 + margins.GetElement(7))) return false;
        var fractional = Avx512F.Subtract(values, Avx512F.RoundScale(values, 1));
        var distance = Avx512F.AndNot(Vector512.Create(long.MinValue),
            Avx512F.Subtract(fractional, Vector512.Create(.5)).AsInt64()).AsDouble();
        var inRange = Avx512F.And(Avx512F.CompareGreaterThanOrEqual(values, Vector512<double>.Zero).AsInt64(),
            Avx512F.CompareLessThanOrEqual(values, Vector512.Create(maximum)).AsInt64());
        var ambiguous = Avx512F.And(inRange, Avx512F.CompareLessThanOrEqual(distance, margins).AsInt64());
        if (Vector512.ExtractMostSignificantBits(ambiguous) != 0) return false;
        var clipped = Avx512F.Min(Vector512.Create(maximum), Avx512F.Max(Vector512<double>.Zero, values));
        var ints = Avx512F.ConvertToVector256Int32WithTruncation(Avx512F.RoundScale(clipped, 0));
        var words = Sse41.PackUnsignedSaturate(ints.GetLower(), ints.GetUpper());
        MemoryMarshal.Write(output, in words);
        return true;
    }

    internal static bool TryWriteWord(Vector256<double> sum, double normalizer, Span<byte> output)
    {
        const double unitError = 2.3283064365386963e-10;
        const double maximum = 65535d;
        const double alphaError = unitError * maximum;
        var alpha = sum.GetElement(3);
        if (!(normalizer >= .5 && alpha > alphaError)) return false;
        var values = Avx.Divide(sum, Vector256.Create(alpha, alpha, alpha, normalizer));
        var magnitude = Math.Max(Math.Abs(values.GetElement(0)),
            Math.Max(Math.Abs(values.GetElement(1)), Math.Abs(values.GetElement(2))));
        var colorMargin = 2 * (alphaError * maximum + magnitude * alphaError) / (alpha - alphaError);
        var alphaMargin = 2 * (alphaError + Math.Abs(values.GetElement(3)) * unitError) / (normalizer - unitError);
        if (colorMargin >= .25 || !(values.GetElement(3) >= 1 + alphaMargin)) return false;
        for (var c = 0; c < 4; c++)
        {
            var value = values.GetElement(c);
            if (value >= 0 && value <= maximum
                && Math.Abs(value - Math.Floor(value) - .5) <= (c == 3 ? alphaMargin : colorMargin)) return false;
        }
        var clipped = Avx.Min(Vector256.Create(maximum), Avx.Max(Vector256<double>.Zero, values));
        var ints = Avx.ConvertToVector128Int32WithTruncation(Avx.RoundToNearestInteger(clipped));
        var words = Sse41.PackUnsignedSaturate(ints, Vector128<int>.Zero).AsUInt64().GetElement(0);
        BinaryPrimitives.WriteUInt64LittleEndian(output, words);
        return true;
    }

    internal static byte RoundByte(double value)
        => (byte)Math.Clamp(Math.Round(value, MidpointRounding.AwayFromZero), 0, 255);

    internal static ushort RoundWord(double value)
        => (ushort)Math.Clamp(Math.Round(value, MidpointRounding.AwayFromZero), 0, 65535);
    internal static int Round(double value,int maximum)=>(int)Math.Clamp(Math.Round(value,MidpointRounding.AwayFromZero),0,maximum);

    internal static Vector512<double> Coefficients(int first, double center, double scale)
    {
        var positions = Vector512.Create((double)first, first + 1d, first + 2d, first + 3d,
            first + 4d, first + 5d, first + 6d, first + 7d);
        var distance = Avx512F.Divide(Avx512F.Subtract(positions, Vector512.Create(center)), Vector512.Create(scale));
        var x = Avx512F.AndNot(Vector512.Create(long.MinValue), distance.AsInt64()).AsDouble();
        var squareTerm = Avx512F.Multiply(Avx512F.Multiply(Vector512.Create(2.5), x), x);
        var nearCube = Avx512F.Multiply(Avx512F.Multiply(Avx512F.Multiply(Vector512.Create(1.5), x), x), x);
        var near = Avx512F.Add(Avx512F.Subtract(nearCube, squareTerm), Vector512.Create(1d));
        var farCube = Avx512F.Multiply(Avx512F.Multiply(Avx512F.Multiply(Vector512.Create(-.5), x), x), x);
        var far = Avx512F.Add(Avx512F.Subtract(Avx512F.Add(farCube, squareTerm),
            Avx512F.Multiply(Vector512.Create(4d), x)), Vector512.Create(2d));
        var outside = Vector512.ConditionalSelect(Avx512F.CompareLessThan(x, Vector512.Create(2d)), far, Vector512<double>.Zero);
        return Vector512.ConditionalSelect(Avx512F.CompareLessThan(x, Vector512.Create(1d)), near, outside);
    }

    internal static double Coefficient(uint low, ushort high)
        => BitConverter.UInt64BitsToDouble(((ulong)high << 48) | ((ulong)low << 16));
}
