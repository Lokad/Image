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
        for (var offset = 0; offset < count; offset += cb.Length)
        {
            var length = Math.Min(cb.Length, count - offset);
            for (var i = 0; i < length; i++)
            {
                cb[i] = Round(Interpolate(components[1], start + offset + i, y));
                cr[i] = Round(Interpolate(components[2], start + offset + i, y));
            }
            JpegInverseColor.WriteRow(luma.Slice(offset, length), cb[..length], cr[..length],
                destination.Slice(offset * channels, length * channels), channels);
        }
    }
}
