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
            for (var i = 0; i < length; i++)
            {
                // Centered half-resolution coordinates have exact quarter
                // phases: even pixels use weights 1/3, odd pixels 3/1.
                // Share clamped indices across U/V, then round only once.
                var x = start + offset + i;
                var left = (x - 1) >> 1; var right = Math.Min(left + 1, last);
                left = Math.Max(left, 0);
                var leftWeight = (x & 1) == 0 ? 1 : 3; var rightWeight = 4 - leftWeight;
                var upperU = ut[left] * leftWeight + ut[right] * rightWeight;
                var lowerU = ub[left] * leftWeight + ub[right] * rightWeight;
                var upperV = vt[left] * leftWeight + vt[right] * rightWeight;
                var lowerV = vb[left] * leftWeight + vb[right] * rightWeight;
                cb[i] = (byte)((upperU * topWeight + lowerU * bottomWeight + 8) >> 4);
                cr[i] = (byte)((upperV * topWeight + lowerV * bottomWeight + 8) >> 4);
            }
            JpegInverseColor.WriteRow(luma.Slice(offset, length), cb[..length], cr[..length],
                destination.Slice(offset * channels, length * channels), channels);
        }
    }
}
