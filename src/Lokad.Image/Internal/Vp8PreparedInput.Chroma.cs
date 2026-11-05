using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal sealed partial class Vp8PreparedInput
{
    private static bool Chroma8Supported => Avx2.IsSupported && Avx512F.IsSupported && Avx512BW.IsSupported && Avx512Vbmi.IsSupported;

    // Operation-local copies of the existing mathematical gamma tables. No
    // static cache or borrowed span escapes; the caller admits 4 KiB scratch
    // before constructing1088 bytes of tables/geometry,128 bytes of word tiles
    // and temporary lookup workspace. These fixed values never escape the pass.
    private readonly struct Chroma8Tables
    {
        private readonly Vector512<byte> lo0, lo1, lo2, lo3, hi0, hi1, hi2, hi3;
        private readonly Vector512<int> base0, base1, slope0, slope1;
        internal readonly Vector512<ushort> Even, Odd;
        internal readonly Vector512<int> Red, Green, Blue;

        internal Chroma8Tables(ReadOnlySpan<ushort> linear, ReadOnlySpan<int> knots)
        {
            Span<byte> lo = stackalloc byte[256]; Span<byte> hi = stackalloc byte[256];
            for (var i = 0; i < 256; i++) { lo[i] = (byte)linear[i]; hi[i] = (byte)(linear[i] >> 8); }
            lo0 = Vector512.Create(lo[..64]); lo1 = Vector512.Create(lo.Slice(64, 64));
            lo2 = Vector512.Create(lo.Slice(128, 64)); lo3 = Vector512.Create(lo.Slice(192, 64));
            hi0 = Vector512.Create(hi[..64]); hi1 = Vector512.Create(hi.Slice(64, 64));
            hi2 = Vector512.Create(hi.Slice(128, 64)); hi3 = Vector512.Create(hi.Slice(192, 64));
            base0 = Vector512.Create(knots[..16]); base1 = Vector512.Create(knots.Slice(16, 16));
            Span<int> slope = stackalloc int[32];
            for (var i = 0; i < 32; i++) slope[i] = knots[i + 1] - knots[i];
            slope0 = Vector512.Create(slope[..16]); slope1 = Vector512.Create(slope[16..]);
            // Select adjacent RGBA pixel pairs. Only the lower sixteen summed
            // components from each half are kept: four output samples per half.
            ReadOnlySpan<ushort> even = [0, 1, 2, 3, 8, 9, 10, 11, 16, 17, 18, 19, 24, 25, 26, 27,
                0, 1, 2, 3, 8, 9, 10, 11, 16, 17, 18, 19, 24, 25, 26, 27];
            ReadOnlySpan<ushort> odd = [4, 5, 6, 7, 12, 13, 14, 15, 20, 21, 22, 23, 28, 29, 30, 31,
                4, 5, 6, 7, 12, 13, 14, 15, 20, 21, 22, 23, 28, 29, 30, 31];
            Even = Vector512.Create(even); Odd = Vector512.Create(odd);
            ReadOnlySpan<int> red = [0, 4, 8, 12, 16, 20, 24, 28, 0, 4, 8, 12, 16, 20, 24, 28];
            ReadOnlySpan<int> green = [1, 5, 9, 13, 17, 21, 25, 29, 1, 5, 9, 13, 17, 21, 25, 29];
            ReadOnlySpan<int> blue = [2, 6, 10, 14, 18, 22, 26, 30, 2, 6, 10, 14, 18, 22, 26, 30];
            Red = Vector512.Create(red); Green = Vector512.Create(green); Blue = Vector512.Create(blue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector512<byte> Lookup(Vector512<byte> index, Vector512<byte> t0, Vector512<byte> t1,
            Vector512<byte> t2, Vector512<byte> t3)
        {
            var lower = Avx512BW.CompareEqual(index & Vector512.Create((byte)128), Vector512<byte>.Zero);
            var first = Avx512BW.CompareEqual(index & Vector512.Create((byte)64), Vector512<byte>.Zero);
            var a = Vector512.ConditionalSelect(first, Avx512Vbmi.PermuteVar64x8(t0, index), Avx512Vbmi.PermuteVar64x8(t1, index));
            var b = Vector512.ConditionalSelect(first, Avx512Vbmi.PermuteVar64x8(t2, index), Avx512Vbmi.PermuteVar64x8(t3, index));
            return Vector512.ConditionalSelect(lower, a, b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal (Vector512<ushort> Low, Vector512<ushort> High) Map(ReadOnlySpan<byte> row)
        {
            var index = Vector512.Create(row);
            var lo = Lookup(index, lo0, lo1, lo2, lo3); var hi = Lookup(index, hi0, hi1, hi2, hi3);
            var lower = Avx512BW.ConvertToVector512Int16(lo.GetLower()).AsUInt16()
                | (Avx512BW.ConvertToVector512Int16(hi.GetLower()).AsUInt16() << 8);
            var upper = Avx512BW.ConvertToVector512Int16(lo.GetUpper()).AsUInt16()
                | (Avx512BW.ConvertToVector512Int16(hi.GetUpper()).AsUInt16() << 8);
            return (lower, upper);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Vector512<int> Compress(Vector512<int> value)
        {
            var index = value >> 9; var fraction = value & Vector512.Create(511);
            return ((Avx512F.PermuteVar16x32x2(base0, index, base1) << 9)
                + fraction * Avx512F.PermuteVar16x32x2(slope0, index, slope1) + Vector512.Create(64)) >> 7;
        }
    }

    private static Vector64<byte> PackChroma8(Vector512<int> value)
    {
        value = Avx512F.Min(Avx512F.Max(value, Vector512<int>.Zero), Vector512.Create(255));
        var lower = value.GetLower();
        return Sse2.PackUnsignedSaturate(Sse2.PackSignedSaturate(lower.GetLower(), lower.GetUpper()), Vector128<short>.Zero).GetLower();
    }

    // Saturating the rounded sample at65535 is harmless: values above that
    // still normalize to255. For n<=65535, (n-(n>>8))>>8 equals floor(n/257).
    // This shared helper preserves every16-bit input, including alpha thresholds.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Normalize16(ReadOnlySpan<ushort> source, Span<byte> destination)
    {
        if (destination.Length < source.Length) throw new ArgumentException("Normalization destination is short.", nameof(destination));
        var at = 0;
        if (Avx512BW.IsSupported)
        {
            var bias = Vector512.Create((ushort)128);
            for (; at <= source.Length - 64; at += 64)
            {
                var a = Avx512BW.AddSaturate(Vector512.Create(source.Slice(at, 32)), bias);
                var b = Avx512BW.AddSaturate(Vector512.Create(source.Slice(at + 32, 32)), bias);
                Vector512.Narrow((a - (a >> 8)) >> 8, (b - (b >> 8)) >> 8).CopyTo(destination[at..]);
            }
        }
        if (Avx2.IsSupported)
        {
            var bias = Vector256.Create((ushort)128);
            for (; at <= source.Length - 32; at += 32)
            {
                var a = Avx2.AddSaturate(Vector256.Create(source.Slice(at, 16)), bias);
                var b = Avx2.AddSaturate(Vector256.Create(source.Slice(at + 16, 16)), bias);
                Vector256.Narrow((a - (a >> 8)) >> 8, (b - (b >> 8)) >> 8).CopyTo(destination[at..]);
            }
        }
        for (; at < source.Length; at++) destination[at] = (byte)((source[at] + 128) / 257);
    }

    // Byte rows are borrowed directly. Word rows use128 bytes of operation-local
    // tile storage, already covered by the caller's4KiB scratch admission.
    // Luma and chroma consume each tile before it is overwritten; nothing escapes.
    private static int PrepareOpaqueTiles(ReadOnlySpan<byte> bytes, ReadOnlySpan<ushort> words, bool eightBit,
        int width, int height, Span<byte> yPlane, Span<byte> uPlane, Span<byte> vPlane,
        int stride, ReadOnlySpan<ushort> linear, ReadOnlySpan<int> knots, OperationContext context)
    {
        var tables = new Chroma8Tables(linear, knots);
        Span<byte> normalized = stackalloc byte[128];
        var prepared = width / 16 * 16;
        for (var y = 0; y < height; y += 2)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var bottomY = Math.Min(y + 1, height - 1);
            for (var x = 0; x < prepared; x += 16)
            {
                var topAt = (y * width + x) * 4; var bottomAt = (bottomY * width + x) * 4;
                scoped ReadOnlySpan<byte> top, bottom;
                if (eightBit) { top = bytes.Slice(topAt, 64); bottom = bytes.Slice(bottomAt, 64); }
                else
                {
                    Normalize16(words.Slice(topAt, 64), normalized[..64]);
                    if (bottomY != y) Normalize16(words.Slice(bottomAt, 64), normalized[64..]);
                    top = normalized[..64]; bottom = bottomY == y ? top : normalized[64..];
                }
                PrepareLuma8(top, yPlane.Slice(y * stride + x, 16));
                if (bottomY != y) PrepareLuma8(bottom, yPlane.Slice(bottomY * stride + x, 16));
                PrepareChroma8Tile(top, bottom, uPlane.Slice(y / 2 * (stride / 2) + x / 2, 8),
                    vPlane.Slice(y / 2 * (stride / 2) + x / 2, 8), in tables);
            }
        }
        return prepared;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void PrepareChroma8Tile(ReadOnlySpan<byte> top, ReadOnlySpan<byte> bottom,
        Span<byte> uPlane, Span<byte> vPlane, in Chroma8Tables tables)
    {
        var t = tables.Map(top); var b = tables.Map(bottom);
        // Four twelve-bit values sum to at most16380, fitting signed16.
        var lower = Avx512BW.PermuteVar32x16(t.Low, tables.Even) + Avx512BW.PermuteVar32x16(t.Low, tables.Odd)
            + Avx512BW.PermuteVar32x16(b.Low, tables.Even) + Avx512BW.PermuteVar32x16(b.Low, tables.Odd);
        var upper = Avx512BW.PermuteVar32x16(t.High, tables.Even) + Avx512BW.PermuteVar32x16(t.High, tables.Odd)
            + Avx512BW.PermuteVar32x16(b.High, tables.Even) + Avx512BW.PermuteVar32x16(b.High, tables.Odd);
        var sum = Vector512.Create(lower.GetLower(), upper.GetLower());
        var n0 = tables.Compress(Avx512F.ConvertToVector512Int32(sum.GetLower().AsInt16()));
        var n1 = tables.Compress(Avx512F.ConvertToVector512Int32(sum.GetUpper().AsInt16()));
        var r = Avx512F.PermuteVar16x32x2(n0, tables.Red, n1);
        var g = Avx512F.PermuteVar16x32x2(n0, tables.Green, n1);
        var colorB = Avx512F.PermuteVar16x32x2(n0, tables.Blue, n1);
        var u = (r * Ur + g * Ug + colorB * (ChromaScale / 2) + Vector512.Create((128 << 18) + (1 << 17))) >> 18;
        var v = (r * Vr + g * Vg + colorB * Vb + Vector512.Create((128 << 18) + (1 << 17))) >> 18;
        PackChroma8(u).CopyTo(uPlane);
        PackChroma8(v).CopyTo(vPlane);
    }
}
