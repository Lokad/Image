using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal ref partial struct JpegDecoder
{
    private bool TryRenderHalfResolutionYcck(ImageBuffer output, bool inverted)
    {
        if (components.Length != 4) return false;
        var luma = components[0]; var cb = components[1]; var cr = components[2]; var black = components[3];
        if (luma.H != maxH || luma.V != maxV || black.H != maxH || black.V != maxV
            || cb.H * 2 != maxH || cb.V * 2 != maxV || cr.H != cb.H || cr.V != cb.V) return false;
        for (var y = 0; y < info.Height; y++)
        {
            var top = y / 2; var bottom = Math.Min((y + 1) / 2, cb.SampleHeight - 1);
            RenderHalfYcckRow(luma.Plane.AsSpan(y * luma.BlockColumns * 8, info.Width),
                cb.Plane.AsSpan(top * cb.BlockColumns * 8, cb.SampleWidth), cb.Plane.AsSpan(bottom * cb.BlockColumns * 8, cb.SampleWidth),
                cr.Plane.AsSpan(top * cr.BlockColumns * 8, cr.SampleWidth), cr.Plane.AsSpan(bottom * cr.BlockColumns * 8, cr.SampleWidth),
                black.Plane.AsSpan(y * black.BlockColumns * 8, info.Width), top != bottom, inverted, output.GetRow8(y), context);
        }
        return true;
    }

    internal static void RenderHalfYcckRow(ReadOnlySpan<byte> luma, ReadOnlySpan<byte> cbTop, ReadOnlySpan<byte> cbBottom,
        ReadOnlySpan<byte> crTop, ReadOnlySpan<byte> crBottom, ReadOnlySpan<byte> black,
        bool interpolateRows, bool inverted, Span<byte> output, OperationContext context)
    {
        var half = (luma.Length + 1L) / 2;
        if (black.Length != luma.Length || cbTop.Length != half || cbBottom.Length != half
            || crTop.Length != half || crBottom.Length != half || output.Length != luma.Length * 4L)
            throw new ArgumentException("YCCK rows require full luma/black, two half-width chroma rows and four output bytes per pixel.");
        // Small bounded stack storage; source planes and output keep their existing
        // owners. Each tile reaches the existing color kernel as a complete row.
        Span<byte> scratch = stackalloc byte[64 * 3];
        var yValues = scratch[..64]; var uValues = scratch.Slice(64, 64); var vValues = scratch.Slice(128, 64);
        for (var start = 0; start < luma.Length; start += 64)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var count = Math.Min(64, luma.Length - start); var i = 0;
            if (Avx2.IsSupported && Ssse3.IsSupported)
                for (; i <= count - 8; i += 8)
                {
                    var x = start + i;
                    var yy = Avx2.ConvertToVector256Int32(Vector128.CreateScalar(BinaryPrimitives.ReadUInt64LittleEndian(luma.Slice(x, 8))).AsByte());
                    var kk = Avx2.ConvertToVector256Int32(Vector128.CreateScalar(BinaryPrimitives.ReadUInt64LittleEndian(black.Slice(x, 8))).AsByte());
                    var u = ExpandHalfYcck(cbTop, cbBottom, x / 2, interpolateRows);
                    var v = ExpandHalfYcck(crTop, crBottom, x / 2, interpolateRows);
                    if (inverted)
                    {
                        yy = Avx2.Subtract(Vector256.Create(255), yy);
                        u = Avx2.Subtract(Vector256.Create(128), u); v = Avx2.Subtract(Vector256.Create(128), v);
                    }
                    else
                    {
                        kk = Avx2.Subtract(Vector256.Create(255), kk);
                        u = Avx2.Subtract(u, Vector256.Create(128)); v = Avx2.Subtract(v, Vector256.Create(128));
                    }
                    StoreYcckEight(BlackProduct(yy, kk), yValues.Slice(i, 8));
                    StoreYcckEight(Avx2.Add(BlackProduct(u, kk), Vector256.Create(128)), uValues.Slice(i, 8));
                    StoreYcckEight(Avx2.Add(BlackProduct(v, kk), Vector256.Create(128)), vValues.Slice(i, 8));
                }
            for (; i < count; i++)
            {
                var x = start + i; var left = x / 2; var right = Math.Min((x + 1) / 2, cbTop.Length - 1);
                var u = (cbTop[left] + cbTop[right]) >> 1; var v = (crTop[left] + crTop[right]) >> 1;
                if (interpolateRows)
                {
                    u = (u + ((cbBottom[left] + cbBottom[right]) >> 1)) >> 1;
                    v = (v + ((crBottom[left] + crBottom[right]) >> 1)) >> 1;
                }
                var k = inverted ? black[x] : 255 - black[x];
                yValues[i] = (byte)JpegBlackColor.Product(inverted ? 255 - luma[x] : luma[x], k);
                uValues[i] = (byte)(JpegBlackColor.Product(inverted ? 128 - u : u - 128, k) + 128);
                vValues[i] = (byte)(JpegBlackColor.Product(inverted ? 128 - v : v - 128, k) + 128);
            }
            JpegInverseColor.WriteRow(yValues[..count], uValues[..count], vValues[..count], output.Slice(start * 4, count * 4), 4);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<int> ExpandHalfYcck(ReadOnlySpan<byte> top, ReadOnlySpan<byte> bottom, int first, bool interpolateRows)
    {
        var value = Horizontal(top, first);
        if (interpolateRows) value = FloorAverage(value, Horizontal(bottom, first));
        return Avx2.ConvertToVector256Int32(value);

        static Vector128<byte> Horizontal(ReadOnlySpan<byte> row, int at)
        {
            var samples = Vector128.CreateScalar(BinaryPrimitives.ReadUInt32LittleEndian(row.Slice(at, 4))).AsByte()
                .WithElement(4, row[Math.Min(at + 4, row.Length - 1)]);
            var left = Ssse3.Shuffle(samples, Vector128.Create((byte)0, 0, 1, 1, 2, 2, 3, 3, 128, 128, 128, 128, 128, 128, 128, 128));
            var right = Ssse3.Shuffle(samples, Vector128.Create((byte)0, 1, 1, 2, 2, 3, 3, 4, 128, 128, 128, 128, 128, 128, 128, 128));
            return FloorAverage(left, right);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> FloorAverage(Vector128<byte> left, Vector128<byte> right)
        => Sse2.Subtract(Sse2.Average(left, right), Sse2.And(Sse2.Xor(left, right), Vector128.Create((byte)1)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<int> BlackProduct(Vector256<int> value, Vector256<int> black)
        => Avx2.ShiftRightArithmetic(Avx2.MultiplyLow(Avx2.MultiplyLow(value, black), Vector256.Create((int)JpegBlackColor.ProductScale)), 16);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void StoreYcckEight(Vector256<int> value, Span<byte> output)
    {
        var words = Sse2.PackSignedSaturate(value.GetLower(), value.GetUpper());
        var bytes = Sse2.PackUnsignedSaturate(words, Vector128<short>.Zero);
        BinaryPrimitives.WriteUInt64LittleEndian(output, bytes.AsUInt64().ToScalar());
    }
}
