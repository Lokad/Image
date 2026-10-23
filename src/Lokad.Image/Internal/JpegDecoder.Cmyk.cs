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
            if (!context.HasMemoryBudget) JpegCoefficientPool.Shared.EnableFourthSlot();
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
                    WriteFourComponentPixel(x, y, source, invert, row.Slice(x * 4, 4), 4);
                }
            }
            return result;
        }
        catch { result.Dispose(); throw; }
    }

    private void CanonicalCmykValues(int x, int y, JpegColorSpace source, bool invert,
        out double c, out double m, out double yellow, out double k)
    {
        c = Interpolate(components[0], x, y); m = Interpolate(components[1], x, y);
        yellow = Interpolate(components[2], x, y); k = Interpolate(components[3], x, y);
        if (source == JpegColorSpace.Ycck)
        {
            // The YCCK transform maps (255-C,255-M,255-Y) through the owned
            // YCbCr matrix; K is independent. Keep matrix/black arithmetic in
            // binary64 until canonical ink samples are quantized. RGB/gray
            // have a separate black-before-color precision boundary.
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
        if (format == JpegSampleFormat.Cmyk && source == JpegColorSpace.Cmyk && orientation == ImageOrientation.Normal
            && TryWriteFullResolutionCmyk(destination, stride, invert)) return;
        for (var y = 0; y < info.Height; y++)
        {
            context.Work(info.Width * 32L);
            for (var x = 0; x < info.Width; x++)
            {
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
                    CanonicalCmykValues(x, y, source, invert, out var c, out var m, out var yellow, out var k);
                    destination[offset] = Round(c); destination[offset + 1] = Round(m);
                    destination[offset + 2] = Round(yellow); destination[offset + 3] = Round(k);
                }
                else WriteFourComponentPixel(x, y, source, invert,
                    destination.Slice(offset, (int)format), (int)format);
            }
        }
    }

    private void WriteFourComponentPixel(int x, int y, JpegColorSpace source, bool inverted,
        Span<byte> destination, int channels)
    {
        var first = FourComponentSample(components[0], x, y);
        var second = FourComponentSample(components[1], x, y);
        var third = FourComponentSample(components[2], x, y);
        var black = FourComponentSample(components[3], x, y);
        if (source == JpegColorSpace.Ycck)
            JpegBlackColor.WriteYcck(first, second, third, black, inverted, destination, channels);
        else JpegBlackColor.WriteCmyk(first, second, third, black, inverted, destination, channels);
    }

    private byte FourComponentSample(Component component, int x, int y)
    {
        var halfH = component.H * 2 == maxH; var halfV = component.V * 2 == maxV;
        if ((component.H == maxH || halfH) && (component.V == maxV || halfV))
        {
            // Four-component half-resolution grids are expanded at integral
            // half-grid origins, rounding horizontally before vertically.
            var x0 = halfH ? x / 2 : x; var y0 = halfV ? y / 2 : y;
            var x1 = Math.Min(halfH ? (x + 1) / 2 : x, component.SampleWidth - 1);
            var y1 = Math.Min(halfV ? (y + 1) / 2 : y, component.SampleHeight - 1);
            var stride = component.BlockColumns * 8;
            var top = (int)component.Plane[y0 * stride + x0];
            if (halfH) top = (top + component.Plane[y0 * stride + x1]) >> 1;
            if (!halfV) return (byte)top;
            var bottom = (int)component.Plane[y1 * stride + x0];
            if (halfH) bottom = (bottom + component.Plane[y1 * stride + x1]) >> 1;
            return (byte)((top + bottom) >> 1);
        }
        // Other admitted sampling factors retain centered edge extension;
        // their exact native qualification is listed independently.
        return Round(Interpolate(component, x, y));
    }

}
