using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

// PNG byte predictors (W3C PNG section9). A bounded sample admits additional
// row storage only for a strong residual reduction; unpredictable inputs keep
// their existing None-filter packet policy and workspace.
internal static class PngRowPrediction
{
    private static int Cost(int value) { value &= 255; return Math.Min(value, 256 - value); }

    internal static bool Useful(ImageBuffer source, int color, CancellationToken cancellation)
    {
        var channels = color == 0 ? 1 : color == 2 ? 3 : color == 4 ? 2 : 4;
        var samples = Math.Min(32, source.Width);
        long none = 0, sub = 0, up = 0, average = 0, paeth = 0;
        for (var probe = 0; probe < 4; probe++)
        {
            cancellation.ThrowIfCancellationRequested();
            var y = probe switch { 0 => 0, 1 => Math.Min(1, source.Height - 1), 2 => source.Height / 2, _ => source.Height - 1 };
            for (var sample = 0; sample < samples; sample++)
            {
                var x = samples == 1 ? 0 : (int)((long)sample * (source.Width - 1) / (samples - 1));
                for (var channel = 0; channel < channels; channel++)
                for (var shift = source.BitDepth - 8; shift >= 0; shift -= 8)
                {
                    var component = color == 4 && channel == 1 ? 3 : channel;
                    var value = Byte(source, x, y, component, shift);
                    var a = x == 0 ? 0 : Byte(source, x - 1, y, component, shift);
                    var b = y == 0 ? 0 : Byte(source, x, y - 1, component, shift);
                    var c = x == 0 || y == 0 ? 0 : Byte(source, x - 1, y - 1, component, shift);
                    none += Cost(value); sub += Cost(value - a); up += Cost(value - b);
                    average += Cost(value - ((a + b) >> 1)); paeth += Cost(value - PngCodec.Paeth(a, b, c));
                }
            }
        }
        return Math.Min(Math.Min(sub, up), Math.Min(average, paeth)) * 4 < none;
    }

    private static int Byte(ImageBuffer source, int x, int y, int channel, int shift)
        => ((source.BitDepth == 8 ? source.GetRow8(y)[x * 4 + channel] : source.GetRow16(y)[x * 4 + channel]) >> shift) & 255;

    internal static byte Apply(ReadOnlySpan<byte> row, ReadOnlySpan<byte> previous, Span<byte> filtered, int stride,
        CancellationToken cancellation)
        => Avx2.IsSupported && row.Length >= stride + 32
            ? ApplyAvx2(row, previous, filtered, stride, cancellation)
            : ApplyScalar(row, previous, filtered, stride, cancellation);

    private static byte ApplyScalar(ReadOnlySpan<byte> row, ReadOnlySpan<byte> previous, Span<byte> filtered, int stride,
        CancellationToken cancellation)
    {
        Span<long> scores = stackalloc long[5]; scores.Clear();
        for (var i = 0; i < row.Length; i++)
        {
            if ((i & 4095) == 0) cancellation.ThrowIfCancellationRequested();
            var value = row[i]; var a = i < stride ? 0 : row[i - stride]; var b = previous[i]; var c = i < stride ? 0 : previous[i - stride];
            scores[0] += Cost(value); scores[1] += Cost(value - a); scores[2] += Cost(value - b);
            scores[3] += Cost(value - ((a + b) >> 1)); scores[4] += Cost(value - PngCodec.Paeth(a, b, c));
        }
        var filter = 0; for (var i = 1; i < 5; i++) if (scores[i] < scores[filter]) filter = i;
        if (filter == 0) return 0;
        for (var i = 0; i < row.Length; i++)
        {
            if ((i & 4095) == 0) cancellation.ThrowIfCancellationRequested();
            var a = i < stride ? 0 : row[i - stride]; var b = previous[i]; var c = i < stride ? 0 : previous[i - stride];
            var prediction = filter switch { 1 => a, 2 => b, 3 => (a + b) >> 1, _ => PngCodec.Paeth(a, b, c) };
            filtered[i] = unchecked((byte)(row[i] - prediction));
        }
        return (byte)filter;
    }

    private static byte ApplyAvx2(ReadOnlySpan<byte> row, ReadOnlySpan<byte> previous, Span<byte> filtered, int stride,
        CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        Span<long> scores = stackalloc long[5]; scores.Clear();
        // The first pixel has no left/corner byte. Subsequent vectors borrow
        // the original packed rows, so unaligned loads and every byte stride
        // have identical PNG semantics; no predictor storage is introduced.
        var i = 0;
        for (; i < stride; i++) ScoreByte(row, previous, stride, i, scores);
        var none = Vector256<ulong>.Zero; var sub = none; var up = none; var average = none; var paeth = none;
        for (; i <= row.Length - 32; i += 32)
        {
            if (((i - stride) & 4095) == 0) cancellation.ThrowIfCancellationRequested();
            var value = Vector256.Create(row.Slice(i, 32)); var a = Vector256.Create(row.Slice(i - stride, 32));
            var b = Vector256.Create(previous.Slice(i, 32)); var c = Vector256.Create(previous.Slice(i - stride, 32));
            none = Avx2.Add(none, Magnitudes(value));
            sub = Avx2.Add(sub, Magnitudes(Avx2.Subtract(value, a)));
            up = Avx2.Add(up, Magnitudes(Avx2.Subtract(value, b)));
            average = Avx2.Add(average, Magnitudes(Avx2.Subtract(value, Average(a, b))));
            paeth = Avx2.Add(paeth, Magnitudes(Avx2.Subtract(value, Paeth(a, b, c))));
        }
        scores[0] += (long)Vector256.Sum(none); scores[1] += (long)Vector256.Sum(sub); scores[2] += (long)Vector256.Sum(up);
        scores[3] += (long)Vector256.Sum(average); scores[4] += (long)Vector256.Sum(paeth);
        for (; i < row.Length; i++) ScoreByte(row, previous, stride, i, scores);
        var filter = 0; for (i = 1; i < 5; i++) if (scores[i] < scores[filter]) filter = i;
        if (filter == 0) return 0;
        for (i = 0; i < stride; i++) filtered[i] = Residual(row, previous, stride, i, filter);
        for (; i <= row.Length - 32; i += 32)
        {
            if (((i - stride) & 4095) == 0) cancellation.ThrowIfCancellationRequested();
            var value = Vector256.Create(row.Slice(i, 32)); var a = Vector256.Create(row.Slice(i - stride, 32));
            var b = Vector256.Create(previous.Slice(i, 32));
            var predicted = filter switch
            {
                1 => a, 2 => b, 3 => Average(a, b),
                _ => Paeth(a, b, Vector256.Create(previous.Slice(i - stride, 32)))
            };
            Avx2.Subtract(value, predicted).CopyTo(filtered.Slice(i, 32));
        }
        for (; i < row.Length; i++) filtered[i] = Residual(row, previous, stride, i, filter);
        return (byte)filter;
    }

    private static void ScoreByte(ReadOnlySpan<byte> row, ReadOnlySpan<byte> previous, int stride, int i, Span<long> scores)
    {
        var value = row[i]; var a = i < stride ? 0 : row[i - stride]; var b = previous[i]; var c = i < stride ? 0 : previous[i - stride];
        scores[0] += Cost(value); scores[1] += Cost(value - a); scores[2] += Cost(value - b);
        scores[3] += Cost(value - ((a + b) >> 1)); scores[4] += Cost(value - PngCodec.Paeth(a, b, c));
    }

    private static byte Residual(ReadOnlySpan<byte> row, ReadOnlySpan<byte> previous, int stride, int i, int filter)
    {
        var a = i < stride ? 0 : row[i - stride]; var b = previous[i]; var c = i < stride ? 0 : previous[i - stride];
        var predicted = filter switch { 1 => a, 2 => b, 3 => (a + b) >> 1, _ => PngCodec.Paeth(a, b, c) };
        return unchecked((byte)(row[i] - predicted));
    }

    // abs(signed wrapped byte), including -128 ->128, is exactly Cost().
    // SAD's four nonnegative UInt64 sums cannot overflow for admitted PNG rows.
    private static Vector256<ulong> Magnitudes(Vector256<byte> value)
        => Avx2.SumAbsoluteDifferences(Avx2.Abs(value.AsSByte()), Vector256<byte>.Zero).AsUInt64();

    private static Vector256<byte> Average(Vector256<byte> a, Vector256<byte> b)
        => Avx2.Subtract(Avx2.Average(a, b), Avx2.And(Avx2.Xor(a, b), Vector256.Create((byte)1)));

    private static Vector256<byte> Paeth(Vector256<byte> a, Vector256<byte> b, Vector256<byte> c)
        => Vector256.Create(Paeth16(a.GetLower(), b.GetLower(), c.GetLower()), Paeth16(a.GetUpper(), b.GetUpper(), c.GetUpper()));

    private static Vector128<byte> Paeth16(Vector128<byte> left, Vector128<byte> above, Vector128<byte> corner)
    {
        var a = Avx2.ConvertToVector256Int16(left); var b = Avx2.ConvertToVector256Int16(above); var c = Avx2.ConvertToVector256Int16(corner);
        // |p-a|=|b-c|, |p-b|=|a-c|, |p-c|=|a+b-2c|, all <=510.
        // Strict losing comparisons preserve PNG's left/above/corner tie order.
        var da = Avx2.Abs(Avx2.Subtract(b, c)).AsInt16(); var db = Avx2.Abs(Avx2.Subtract(a, c)).AsInt16();
        var dc = Avx2.Abs(Avx2.Subtract(Avx2.Add(a, b), Avx2.Add(c, c))).AsInt16();
        var bc = Avx2.BlendVariable(b.AsByte(), c.AsByte(), Avx2.CompareGreaterThan(db, dc).AsByte());
        var chosen = Avx2.BlendVariable(a.AsByte(), bc, Avx2.Or(Avx2.CompareGreaterThan(da, db), Avx2.CompareGreaterThan(da, dc)).AsByte()).AsInt16();
        return Sse2.PackUnsignedSaturate(chosen.GetLower(), chosen.GetUpper());
    }
}
