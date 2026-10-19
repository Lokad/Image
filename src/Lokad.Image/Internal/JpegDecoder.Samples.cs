using System.Buffers.Binary;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal ref partial struct JpegDecoder
{
    internal ImageInfo DecodeSamples(Span<byte> destination, JpegSampleFormat format, int stride,
        JpegDecodeOptions options)
    {
        var orientation = options.ApplyOrientation ? info.Orientation : ImageOrientation.Normal;
        var width = (int)orientation >= 5 ? info.Height : info.Width;
        var height = (int)orientation >= 5 ? info.Width : info.Height;
        var channels = (int)format;
        var adobeTransform = -1;
        var rowBytes = (long)width * channels;
        if (rowBytes > int.MaxValue)
            throw new ImageLimitException(ImageLimitKind.BufferLength, "JPEG sample row is too large.");
        if (stride == 0) stride = (int)rowBytes;
        if (stride < rowBytes) throw new ArgumentOutOfRangeException(nameof(stride), "Stride must cover a complete sample row.");
        if ((long)(height - 1) * stride + rowBytes > destination.Length)
            throw new ArgumentException("Destination does not cover all JPEG sample rows.", nameof(destination));

        // This entry point shares the original marker/entropy/IDCT primitives.
        // It keeps its own top-level walk so ordinary RGBA decoding retains its
        // existing dispatch, object state and measured operation boundaries.
        try
        {
            while (position < data.Length)
            {
                context.Work(1);
                var marker = ReadMarker();
                if (marker == 0xD9)
                {
                    if (!scanSeen) throw new ImageDataException("JPEG has no pixel scans.");
                    foreach (var component in components)
                        if (component.Approximation[0] < 0) throw new ImageDataException("JPEG component is missing a DC scan.");
                    var source = ResolveSampleColorSpace(options.SourceColorSpace, adobeTransform);
                    var sourceComponents = source == JpegColorSpace.Grayscale ? 1 : source is JpegColorSpace.Cmyk or JpegColorSpace.Ycck ? 4 : 3;
                    if (components.Length != sourceComponents || (format == JpegSampleFormat.Cmyk && sourceComponents != 4))
                        throw new UnsupportedImageException(UnsupportedImageFeature.ComponentLayout,
                            "JPEG components do not match the requested source color space.", ImageFormat.Jpeg);
                    MaterializeSamplePlanes();
                    if (sourceComponents == 4)
                        WriteCmykSamples(destination, format, stride, source, ResolveCmykInversion(options.CmykInversion, adobeTransform), orientation);
                    else WriteSamples(destination, format, stride, source, orientation);
                    return options.ApplyOrientation
                        ? info with { Width = width, Height = height, Orientation = ImageOrientation.Normal } : info;
                }
                var payload = ReadSegment();
                switch (marker)
                {
                    case 0xC0 or 0xC2: ReadFrame(payload, marker == 0xC2); break;
                    case 0xDB: ReadQuantization(payload); break;
                    case 0xC4: ReadHuffman(payload); break;
                    case 0xDD:
                        if (payload.Length != 2) throw new ImageDataException("Invalid JPEG restart interval.");
                        restartInterval = BinaryPrimitives.ReadUInt16BigEndian(payload); break;
                    case 0xDA: ReadScan(payload); scanSeen = true; break;
                    case 0xEE when payload.StartsWith("Adobe"u8):
                        if (payload.Length < 12) throw new ImageDataException("Truncated Adobe JPEG marker.");
                        if (payload[11] > 2) throw UnsupportedColorTransform();
                        adobeTransform = payload[11];
                        rgb = payload[11] == 0; break;
                    case >= 0xE0 and <= 0xEF or 0xFE: break;
                    default: throw new UnsupportedImageException(UnsupportedImageFeature.ControlMarker,
                        "Unsupported JPEG control marker.", ImageFormat.Jpeg);
                }
            }
            throw new ImageDataException("JPEG is missing its end marker.");
        }
        finally
        {
            foreach (var component in components)
            {
                if (component is null || component.Coefficients.Length == 0) continue;
                var storage = component.Coefficients; component.Coefficients = [];
                context.ReturnCoefficients(storage);
            }
        }
    }

    private void MaterializeSamplePlanes()
    {
        // Four-component decoding shares the exact DC/sparse/general walk.
        // Allocation, block charges and coefficient release retain their order.
        if (components.Length == 4) { MaterializeCmykPlanes(); return; }
        Span<byte> block = stackalloc byte[64];
        foreach (var component in components)
        {
            var length = component.Coefficients.Length;
            context.Reserve(length); component.Plane = new byte[length];
            for (var y = 0; y < component.BlockRows; y++)
            for (var x = 0; x < component.BlockColumns; x++)
            {
                ReadOnlySpan<int> coefficients = component.Coefficients.AsSpan((y * component.BlockColumns + x) * 64, 64);
                if (progressive)
                {
                    var natural = component.Approximation.AsSpan();
                    for (var k = 0; k < 64; k++) natural[JpegTransform.Zigzag[k]] = coefficients[k];
                    coefficients = natural;
                }
                JpegTransform.Inverse(coefficients, component.FrozenQuantization!, block, context);
                for (var by = 0; by < 8; by++)
                    block.Slice(by * 8, 8).CopyTo(component.Plane.AsSpan((y * 8 + by) * component.BlockColumns * 8 + x * 8, 8));
            }
            var storage = component.Coefficients; component.Coefficients = [];
            context.Release(length * 4L); context.ReturnCoefficients(storage);
        }
    }

    private void WriteSamples(Span<byte> destination, JpegSampleFormat format, int stride,
        JpegColorSpace source, ImageOrientation orientation)
    {
        if (format == JpegSampleFormat.Rgb && source == JpegColorSpace.YCbCr
            && orientation == ImageOrientation.Normal && info.Width >= 32
            && maxH == 2 && maxV == 2 && components.Length == 3
            && components[0].H == 2 && components[0].V == 2
            && components[1].H == 1 && components[1].V == 1
            && components[2].H == 1 && components[2].V == 1
            && Avx512F.IsSupported && Avx512F.VL.IsSupported && Avx2.IsSupported && Ssse3.IsSupported)
        {
            WriteRgb420Samples(destination, stride); return;
        }
        if (format == JpegSampleFormat.Grayscale && source != JpegColorSpace.Rgb
            && orientation == ImageOrientation.Normal)
        {
            for (var y = 0; y < info.Height; y++)
            {
                context.Work(info.Width * 32L);
                components[0].Plane.AsSpan(y * components[0].BlockColumns * 8, info.Width)
                    .CopyTo(destination.Slice(y * stride, info.Width));
            }
            return;
        }
        for (var y = 0; y < info.Height; y++)
        {
            context.Work(info.Width * 32L);
            var firstRow = components[0].Plane.AsSpan(y * components[0].BlockColumns * 8, info.Width);
            for (var x = 0; x < info.Width; x++)
            {
                double first = firstRow[x];
                var red = first; var green = first; var blue = first;
                if (source != JpegColorSpace.Grayscale && (format != JpegSampleFormat.Grayscale || source == JpegColorSpace.Rgb))
                {
                    var second = Interpolate(components[1], x, y); var third = Interpolate(components[2], x, y);
                    if (source == JpegColorSpace.Rgb) { green = second; blue = third; }
                    else
                    {
                        var cb = second - 128; var cr = third - 128;
                        red = first + 1.402 * cr;
                        green = first - 0.344136286 * cb - 0.714136286 * cr;
                        blue = first + 1.772 * cb;
                    }
                }
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
                if (format == JpegSampleFormat.Grayscale)
                    destination[offset] = Round(source == JpegColorSpace.Rgb ? 0.299 * red + 0.587 * green + 0.114 * blue : first);
                else
                {
                    destination[offset] = Round(red); destination[offset + 1] = Round(green); destination[offset + 2] = Round(blue);
                }
            }
        }
    }
}
