using System.Buffers.Binary;
using Lokad.Image.Tooling;

public enum QualityEncoder { Jpeg444, Jpeg422, Jpeg420, Vp8 }
public enum QualityPattern { GrayRamp, ColorTiles, SoftGradient, Asymmetric, SeededNoise, ProceduralTexture }
public enum QualityAlpha { Opaque, Gradient, BinaryEdges }

public sealed record EncoderQualityProfile(int Width, int Depth, QualityEncoder Encoder, QualityPattern Pattern, QualityAlpha Alpha = QualityAlpha.Opaque, uint? BackgroundRgb = null)
{
    public int Height => Width / 2 + 3;
    public bool HasAlpha => Alpha != QualityAlpha.Opaque;
    public bool EncodedHasAlpha => Encoder == QualityEncoder.Vp8 && HasAlpha;
    public string SourceMetric => EncodedHasAlpha ? "premultiplied-RGB-before-rounding" : HasAlpha ? "whole-raster-RGB-after-once-flattening" : "whole-raster-RGB";
    public override string ToString() => $"{Encoder}-{Pattern}-{Width}x{Height}-d{Depth}" + (HasAlpha ? $"-{Alpha}Alpha" : "")
        + (BackgroundRgb.HasValue ? $"-bg{BackgroundRgb.Value:x6}" : "");
}

// Original deterministic benchmark recipes. No photographic or upstream inputs.
internal static class EncoderQualitySource
{
    public const string Recipe = "encoder-quality-v3: RGBA8/RGBA16LE; unchanged integer ramps, six 16-pixel color tiles, asymmetric-v1, seeded hash noise and interpolated 16-pixel texture knots; opaque alpha, horizontal alpha gradient with literal low-alpha first-row boundaries, or binary 16-pixel checker edges; RGB unchanged by alpha selection; transparent JPEG uses explicit RGB 0x204060 with single-round flattening/depth reduction, while VP8 preserves once-normalized alpha";

    public static byte[] Generate(EncoderQualityProfile profile)
    {
        if (!Enum.IsDefined(profile.Alpha) || profile.BackgroundRgb > 0xFFFFFF
            || (profile.HasAlpha && profile.Encoder != QualityEncoder.Vp8) != profile.BackgroundRgb.HasValue)
            throw new ArgumentException("Transparent JPEG quality profiles require an explicit RGB background; other profiles have none.", nameof(profile));
        var source = GenerateColors(profile);
        if (!profile.HasAlpha) return source;
        var maximum = profile.Depth == 8 ? 255 : 65535; var sampleBytes = profile.Depth / 8;
        int[] lowAlpha = profile.Depth == 8 ? [0, 1, 2, 127, 128, 129, 254, 255, 0, 1] : [0, 1, 127, 128, 129, 256, 257, 32768, 65534, 65535];
        for (var y = 0; y < profile.Height; y++)
        for (var x = 0; x < profile.Width; x++)
        {
            var alpha = profile.Alpha == QualityAlpha.BinaryEdges ? ((x / 16 + y / 16) % 2 == 0 ? 0 : maximum)
                : y == 0 && x < lowAlpha.Length ? lowAlpha[x] : (int)((long)x * maximum / (profile.Width - 1));
            var offset = ((y * profile.Width + x) * 4 + 3) * sampleBytes;
            if (profile.Depth == 8) source[offset] = (byte)alpha;
            else BinaryPrimitives.WriteUInt16LittleEndian(source.AsSpan(offset), (ushort)alpha);
        }
        return source;
    }

    public static byte[] Normalize(byte[] source, int depth)
    {
        if (depth is not (8 or 16) || source.Length % (4 * (depth / 8)) != 0) throw new ArgumentException("Invalid normalization source.");
        if (depth == 8) return source;
        var bytes = new byte[source.Length / 2];
        for (var sample = 0; sample < bytes.Length; sample++)
            bytes[sample] = (byte)((BinaryPrimitives.ReadUInt16LittleEndian(source.AsSpan(sample * 2)) + 128) / 257);
        return bytes;
    }

    public static byte[] Reference(byte[] source, EncoderQualityProfile profile)
        => profile.BackgroundRgb.HasValue ? JpegQualityCorpus.Reference(source, profile.Depth, profile.BackgroundRgb) : Normalize(source, profile.Depth);

    public static string AlphaHash(byte[] rgba8)
        => JpegBenchmarkCase.Hash(Enumerable.Range(0, rgba8.Length / 4).Select(pixel => rgba8[pixel * 4 + 3]).ToArray());

    private static byte[] GenerateColors(EncoderQualityProfile profile)
    {
        var width = profile.Width; var height = profile.Height; var depth = profile.Depth;
        if (width < 2 || height < 2 || depth is not (8 or 16) || !Enum.IsDefined(profile.Encoder) || !Enum.IsDefined(profile.Pattern))
            throw new ArgumentException("Invalid encoder quality profile.", nameof(profile));
        if (profile.Pattern == QualityPattern.Asymmetric) return OriginalPixels.Generate(width, height, depth, opaque: true);
        var maximum = depth == 8 ? 255 : 65535; var sampleBytes = depth / 8;
        byte[][] colors = [[16,32,200], [220,32,16], [32,200,48], [220,180,32], [32,180,200], [200,32,180]];
        var source = new byte[checked(width * height * 4 * sampleBytes)];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        for (var channel = 0; channel < 4; channel++)
        {
            int sample;
            if (channel == 3) sample = maximum;
            else sample = profile.Pattern switch
            {
                QualityPattern.GrayRamp => (int)((long)x * maximum / (width - 1)),
                QualityPattern.ColorTiles => colors[(x / 16 + y / 16 * 3) % colors.Length][channel] * (maximum / 255),
                QualityPattern.SoftGradient => channel switch
                {
                    0 => (int)((long)x * maximum / (width - 1)),
                    1 => (int)((long)y * maximum / (height - 1)),
                    _ => (int)(((long)x + y) * maximum / (width + (long)height - 2))
                },
                QualityPattern.SeededNoise => (int)((Noise(x, y, channel) & 65535) * (long)maximum / 65535),
                QualityPattern.ProceduralTexture => (int)((long)Texture(x, y, channel) * maximum / 65535),
                _ => throw new ArgumentException("Unknown quality recipe.")
            };
            var offset = ((y * width + x) * 4 + channel) * sampleBytes;
            if (depth == 8) source[offset] = (byte)sample;
            else BinaryPrimitives.WriteUInt16LittleEndian(source.AsSpan(offset), (ushort)sample);
        }
        return source;
    }

    private static uint Noise(int x, int y, int channel)
    {
        // Explicit modulo-2^32 integer mixing; seed and constants belong to this recipe.
        var value = unchecked(0x51A7E23Du + (uint)x * 0x9E3779B9u + (uint)y * 0x85EBCA6Bu + (uint)channel * 0xC2B2AE35u);
        value ^= value >> 16; value = unchecked(value * 0x7FEB352Du);
        value ^= value >> 15; value = unchecked(value * 0x846CA68Bu);
        return value ^ (value >> 16);
    }

    private static int Texture(int x, int y, int channel)
    {
        var gridX = x / 16; var gridY = y / 16; var dx = x % 16; var dy = y % 16;
        var top = (Noise(gridX, gridY, channel) & 65535) * (16 - dx) + (Noise(gridX + 1, gridY, channel) & 65535) * dx;
        var bottom = (Noise(gridX, gridY + 1, channel) & 65535) * (16 - dx) + (Noise(gridX + 1, gridY + 1, channel) & 65535) * dx;
        return (int)((top * (16 - dy) + bottom * dy + 128) / 256);
    }
}
