namespace Lokad.Image.Internal;

internal ref partial struct JpegDecoder
{
    private static UnsupportedImageException UnsupportedColorTransform()
        => new(UnsupportedImageFeature.ColorTransform, "Unsupported JPEG Adobe color transform.", ImageFormat.Jpeg);

    private JpegColorSpace ResolveSampleColorSpace(JpegColorSpace requested, int adobeTransform)
    {
        if (requested != JpegColorSpace.Auto) return requested;
        if (components.Length == 4)
            return adobeTransform switch { -1 or 0 => JpegColorSpace.Cmyk, 2 => JpegColorSpace.Ycck, _ => throw UnsupportedColorTransform() };
        if (adobeTransform == 2) throw UnsupportedColorTransform();
        return components.Length == 1 ? JpegColorSpace.Grayscale : rgb ? JpegColorSpace.Rgb : JpegColorSpace.YCbCr;
    }

    private static bool ResolveCmykInversion(JpegCmykInversion inversion, int adobeTransform)
        => inversion == JpegCmykInversion.Invert || (inversion == JpegCmykInversion.Auto && adobeTransform is 0 or 2);

    private ImageBuffer RenderCmyk(int adobeTransform)
    {
        var source = ResolveSampleColorSpace(JpegColorSpace.Auto, adobeTransform);
        var invert = ResolveCmykInversion(JpegCmykInversion.Auto, adobeTransform);
        var fullResolution = source == JpegColorSpace.Cmyk && components.Length == 4;
        for (var c = 0; fullResolution && c < components.Length; c++)
            fullResolution = components[c].H == maxH && components[c].V == maxV;
        if (fullResolution)
        {
            JpegCoefficientPool.Shared.EnableFourthSlot();
            MaterializeCmykPlanes();
        }
        else MaterializeSamplePlanes();
        var result = ImageBuffer.Allocate(info.Width, info.Height, 8, context); result.Orientation = info.Orientation;
        try
        {
            if (fullResolution)
            {
                for (var y = 0; y < info.Height; y++)
                {
                    context.Work(info.Width * 32L);
                    RenderCmykRow(components[0].Plane.AsSpan(y * components[0].BlockColumns * 8, info.Width),
                        components[1].Plane.AsSpan(y * components[1].BlockColumns * 8, info.Width),
                        components[2].Plane.AsSpan(y * components[2].BlockColumns * 8, info.Width),
                        components[3].Plane.AsSpan(y * components[3].BlockColumns * 8, info.Width), invert, result.GetRow8(y));
                }
                return result;
            }
            for (var y = 0; y < info.Height; y++)
            {
                context.Work(info.Width * 32L);
                var row = result.GetRow8(y);
                for (var x = 0; x < info.Width; x++)
                {
                    CmykValues(x, y, source, invert, out var c, out var m, out var yellow, out var k);
                    row[x * 4] = Round((255 - c) * (255 - k) / 255);
                    row[x * 4 + 1] = Round((255 - m) * (255 - k) / 255);
                    row[x * 4 + 2] = Round((255 - yellow) * (255 - k) / 255); row[x * 4 + 3] = 255;
                }
            }
            return result;
        }
        catch { result.Dispose(); throw; }
    }

    private void CmykValues(int x, int y, JpegColorSpace source, bool invert,
        out double c, out double m, out double yellow, out double k)
    {
        c = Interpolate(components[0], x, y); m = Interpolate(components[1], x, y);
        yellow = Interpolate(components[2], x, y); k = Interpolate(components[3], x, y);
        if (source == JpegColorSpace.Ycck)
        {
            // The YCCK transform maps (255-C,255-M,255-Y) through the owned
            // YCbCr matrix; K is independent. Keep matrix/black arithmetic in
            // binary64 until the requested output channels are quantized.
            var luma = c; var cb = m - 128; var cr = yellow - 128;
            c = 255 - (luma + 1.402 * cr);
            m = 255 - (luma - 0.344136286 * cb - 0.714136286 * cr);
            yellow = 255 - (luma + 1.772 * cb);
        }
        if (invert) { c = 255 - c; m = 255 - m; yellow = 255 - yellow; k = 255 - k; }
    }

    private void WriteCmykSamples(Span<byte> destination, JpegSampleFormat format, int stride,
        JpegColorSpace source, bool invert, ImageOrientation orientation)
    {
        for (var y = 0; y < info.Height; y++)
        {
            context.Work(info.Width * 32L);
            for (var x = 0; x < info.Width; x++)
            {
                CmykValues(x, y, source, invert, out var c, out var m, out var yellow, out var k);
                var (dx, dy) = orientation switch
                {
                    ImageOrientation.MirrorHorizontal => (info.Width - 1 - x, y),
                    ImageOrientation.Rotate180 => (info.Width - 1 - x, info.Height - 1 - y),
                    ImageOrientation.MirrorVertical => (x, info.Height - 1 - y),
                    ImageOrientation.Transpose => (y, x),
                    ImageOrientation.Rotate90 => (info.Height - 1 - y, x),
                    ImageOrientation.Transverse => (info.Height - 1 - y, info.Width - 1 - x),
                    ImageOrientation.Rotate270 => (y, info.Width - 1 - x),
                    _ => (x, y)
                };
                var offset = dy * stride + dx * (int)format;
                if (format == JpegSampleFormat.Cmyk)
                {
                    destination[offset] = Round(c); destination[offset + 1] = Round(m);
                    destination[offset + 2] = Round(yellow); destination[offset + 3] = Round(k);
                }
                else
                {
                    var r = (255 - c) * (255 - k) / 255;
                    var g = (255 - m) * (255 - k) / 255;
                    var b = (255 - yellow) * (255 - k) / 255;
                    if (format == JpegSampleFormat.Grayscale) destination[offset] = Round(0.299 * r + 0.587 * g + 0.114 * b);
                    else { destination[offset] = Round(r); destination[offset + 1] = Round(g); destination[offset + 2] = Round(b); }
                }
            }
        }
    }
}
