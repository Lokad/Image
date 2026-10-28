using System.Buffers.Binary;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal ref partial struct JpegDecoder
{
    private bool HasHalfResolutionChroma()
        => components.Length == 3
            && components[0].H == maxH && components[0].V == maxV
            && components[1].H * 2 == maxH
            && (components[1].V == maxV || components[1].V * 2 == maxV)
            && components[2].H == components[1].H && components[2].V == components[1].V;

    private void WriteYcbcrPixels(int y, int start, int count, Span<byte> destination, int channels)
    {
        // Preserve centered interpolation and edge clamping. The native color
        // boundary consumes rounded eight-bit chroma, not fractional samples.
        // Fixed chunks avoid a row/frame copy; the parent checks row cancellation.
        Span<byte> cb = stackalloc byte[32];
        Span<byte> cr = stackalloc byte[32];
        var luma = components[0].Plane.AsSpan(y * components[0].BlockColumns * 8 + start, count);
        var u = components[1]; var v = components[2];
        var fullHeight = u.V == maxV;
        var top = fullHeight ? y : (y - 1) >> 1;
        var bottom = fullHeight ? y : Math.Min(top + 1, u.SampleHeight - 1);
        top = Math.Max(top, 0);
        var topWeight = fullHeight ? 4 : (y & 1) == 0 ? 1 : 3;
        var bottomWeight = 4 - topWeight;
        var ut = u.Plane.AsSpan(top * u.BlockColumns * 8, u.SampleWidth);
        var ub = u.Plane.AsSpan(bottom * u.BlockColumns * 8, u.SampleWidth);
        var vt = v.Plane.AsSpan(top * v.BlockColumns * 8, v.SampleWidth);
        var vb = v.Plane.AsSpan(bottom * v.BlockColumns * 8, v.SampleWidth);
        var last = u.SampleWidth - 1;
        for (var offset = 0; offset < count; offset += cb.Length)
        {
            var length = Math.Min(cb.Length, count - offset);
            for (var i = 0; i < length;)
            {
                var x = start + offset + i;
                if (Sse2.IsSupported && i + 16 <= length && x >= 2 && (x & 1) == 0 && (x >> 1) + 8 <= last)
                {
                    // Three complete eight-sample windows cover sixteen
                    // interior pixels. Edge/tail/single-pixel calls stay below.
                    HalfChroma16(ut, ub, x >> 1, topWeight).CopyTo(cb.Slice(i, 16));
                    HalfChroma16(vt, vb, x >> 1, topWeight).CopyTo(cr.Slice(i, 16));
                    i += 16;
                    continue;
                }
                // Centered half-resolution coordinates have exact quarter
                // phases: even pixels use weights 1/3, odd pixels 3/1.
                // Share clamped indices across U/V, then round only once.
                var left = (x - 1) >> 1; var right = Math.Min(left + 1, last);
                left = Math.Max(left, 0);
                var leftWeight = (x & 1) == 0 ? 1 : 3; var rightWeight = 4 - leftWeight;
                var upperU = ut[left] * leftWeight + ut[right] * rightWeight;
                var lowerU = ub[left] * leftWeight + ub[right] * rightWeight;
                var upperV = vt[left] * leftWeight + vt[right] * rightWeight;
                var lowerV = vb[left] * leftWeight + vb[right] * rightWeight;
                cb[i] = (byte)((upperU * topWeight + lowerU * bottomWeight + 8) >> 4);
                cr[i] = (byte)((upperV * topWeight + lowerV * bottomWeight + 8) >> 4);
                i++;
            }
            JpegInverseColor.WriteRow(luma.Slice(offset, length), cb[..length], cr[..length],
                destination.Slice(offset * channels, length * channels), channels);
        }
    }

    private static Vector128<byte> HalfChroma16(ReadOnlySpan<byte> top, ReadOnlySpan<byte> bottom,
        int center, int topWeight)
    {
        var left = VerticalChroma8(top, bottom, center - 1, topWeight);
        var middle = VerticalChroma8(top, bottom, center, topWeight);
        var right = VerticalChroma8(top, bottom, center + 1, topWeight);
        var triple = Sse2.Add(Sse2.ShiftLeftLogical(middle, 1), middle);
        var bias = Vector128.Create((ushort)8);
        var even = Sse2.ShiftRightLogical(Sse2.Add(Sse2.Add(left, triple), bias), 4);
        var odd = Sse2.ShiftRightLogical(Sse2.Add(Sse2.Add(triple, right), bias), 4);
        // All Q4 sums are at most 4080 before bias, so unsigned words preserve
        // the exact integer round and the final values already lie in 0..255.
        return Sse2.PackUnsignedSaturate(Sse2.UnpackLow(even, odd).AsInt16(), Sse2.UnpackHigh(even, odd).AsInt16());
    }

    private static Vector128<ushort> VerticalChroma8(ReadOnlySpan<byte> top, ReadOnlySpan<byte> bottom,
        int start, int topWeight)
    {
        var upper = Vector128.CreateScalar(BinaryPrimitives.ReadUInt64LittleEndian(top.Slice(start, 8))).AsByte();
        var lower = Vector128.CreateScalar(BinaryPrimitives.ReadUInt64LittleEndian(bottom.Slice(start, 8))).AsByte();
        var u = Sse2.UnpackLow(upper, Vector128<byte>.Zero).AsUInt16();
        var l = Sse2.UnpackLow(lower, Vector128<byte>.Zero).AsUInt16();
        return Sse2.Add(Sse2.MultiplyLow(u, Vector128.Create((ushort)topWeight)),
            Sse2.MultiplyLow(l, Vector128.Create((ushort)(4 - topWeight))));
    }
}
