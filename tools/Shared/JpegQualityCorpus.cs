using System.Buffers.Binary;

namespace Lokad.Image.Tooling;

// Original synthetic inputs. Grayscale profiles start with grayscale samples;
// transparent profiles use the specified encoded-space flattening contract.
internal static class JpegQualityCorpus
{
    public const int Width = 129;
    public const int Height = 67;
    public static readonly string[] Patterns = ["gray-ramp", "color-tiles", "soft-gradient", "asymmetric-v1", "transparent-soft-gradient"];
    public static readonly int[] ManagedQualities = [1, 50, 85, 100];
    public static readonly int[] NativeQScales = [31, 16, 5, 2];
    public static uint? Background(string pattern) => pattern == "transparent-soft-gradient" ? 0x202020u : null;

    public static byte[] Source(string pattern, int depth, JpegSampling sampling)
    {
        var gray = sampling == JpegSampling.Grayscale;
        if (pattern == "asymmetric-v1") return OriginalPixels.Generate(Width, Height, depth, gray, opaque: true);
        if (!Patterns.Contains(pattern)) throw new ArgumentException("Unknown original pattern.", nameof(pattern));
        var maximum = depth == 16 ? 65535 : 255; var bytes = depth / 8;
        byte[][] colors = [[16,32,200], [220,32,16], [32,200,48], [220,180,32], [32,180,200], [200,32,180]];
        var result = new byte[Width * Height * 4 * bytes];
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
        {
            var r = x * maximum / (Width - 1); var g = y * maximum / (Height - 1); var b = (x + y) * maximum / (Width + Height - 2);
            if (pattern == "gray-ramp") g = b = r;
            else if (pattern == "color-tiles")
            {
                var color = colors[(x / 16 + y / 16 * 3) % colors.Length];
                r = color[0] * (maximum / 255); g = color[1] * (maximum / 255); b = color[2] * (maximum / 255);
            }
            if (gray) r = g = b = (299 * r + 587 * g + 114 * b + 500) / 1000;
            var alpha = Background(pattern).HasValue ? (x * 13 + y * 7) % 17 * maximum / 16 : maximum;
            var samples = new[] { r, g, b, alpha }; var offset = (y * Width + x) * 4 * bytes;
            for (var channel = 0; channel < 4; channel++)
                if (bytes == 1) result[offset + channel] = (byte)samples[channel];
                else BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(offset + channel * 2), (ushort)samples[channel]);
        }
        return result;
    }

    public static byte[] Reference(byte[] source, int depth, uint? background)
    {
        var bytes = depth / 8; var maximum = depth == 16 ? 65535 : 255;
        var result = new byte[source.Length / bytes]; var denominator = (long)maximum * maximum;
        for (var offset = 0; offset < result.Length; offset += 4)
        {
            var alpha = Sample(offset + 3);
            if (alpha != maximum && !background.HasValue) throw new InvalidOperationException("Transparent reference has no background.");
            for (var channel = 0; channel < 3; channel++)
            {
                var bg = (int)(((background ?? 0) >> (16 - channel * 8)) & 255);
                var numerator = Sample(offset + channel) * (long)alpha * 255 + bg * (long)(maximum - alpha) * maximum;
                result[offset + channel] = (byte)((numerator + denominator / 2) / denominator);
            }
            result[offset + 3] = 255;
        }
        return result;
        int Sample(int index) => bytes == 1 ? source[index] : BinaryPrimitives.ReadUInt16LittleEndian(source.AsSpan(index * 2));
    }

    public static ImageBuffer Buffer(byte[] source, int depth)
    {
        var result = ImageBuffer.Create(Width, Height, depth);
        for (var y = 0; y < Height; y++)
            if (depth == 8) source.AsSpan(y * Width * 4, Width * 4).CopyTo(result.GetRow8(y));
            else for (var i = 0; i < Width * 4; i++) result.GetRow16(y)[i] = BinaryPrimitives.ReadUInt16LittleEndian(source.AsSpan((y * Width * 4 + i) * 2));
        return result;
    }

    public static (int Maximum, double Rmse, double? Psnr) Compare(byte[] reference, byte[] actual)
    {
        if (reference.Length != actual.Length || reference.Length % 4 != 0) throw new InvalidOperationException("Quality raster sizes differ.");
        double squared = 0; var maximum = 0;
        for (var i = 0; i < actual.Length; i++)
        {
            if (i % 4 == 3) { if (actual[i] != 255 || reference[i] != 255) throw new InvalidOperationException("JPEG quality alpha differs."); continue; }
            var error = Math.Abs(reference[i] - actual[i]); squared += error * error; maximum = Math.Max(maximum, error);
        }
        var rmse = Math.Sqrt(squared / (actual.Length / 4 * 3));
        return (maximum, rmse, rmse == 0 ? null : 20 * Math.Log10(255 / rmse));
    }
}
