// Source-only stage model for ONE frozen control, not production codec code.
// RFC 9649 defines the container payload and transform/auxiliary-image syntax.
#nullable enable
using System;
using System.Buffers.Binary;
using System.Linq;
using System.Security.Cryptography;

public sealed record Vp8lAuxiliaryResult(byte[] Bytes, int BitCount, int TokenStart,
    int TokenCount, byte[][] TreeLengths, string MatchIndexSha256, object Selection);

public sealed record Vp8lPrefixResult(byte[] Bytes, int BitCount, byte[] Modes,
    byte[] Residual, byte[] ColorMap, byte[] CodedResidual,
    Vp8lAuxiliaryResult Predictor, Vp8lAuxiliaryResult Color);

public static class Vp8lAuxiliaryParity
{
    public static Vp8lAuxiliaryResult Encode(uint[] pixels, int width, int quality, bool lowEffort=false)
    {
        if (width <= 0 || pixels.Length < 1 || pixels.Length % width != 0 || pixels.Length > 65536)
            throw new ArgumentException("Expected a bounded auxiliary raster.");
        if (quality != 20 && quality != 75) throw new ArgumentOutOfRangeException(nameof(quality), "Only pinned auxiliary qualities20/75 are qualified.");
        var matches = lowEffort?Vp8lMatchParity.BuildLowEffort(pixels,width,quality):Vp8lMatchParity.Build(pixels,width,quality);
        var parsed = Vp8lLzParity.Encode(pixels, matches, width, quality>=25);
        var trees = Vp8lLzParity.Histograms(parsed.Result, width, true)
            .Select(x => Vp8lPrefixParity.Lengths(x)).ToArray();
        var encoded = Vp8lBitParity.Encode(parsed.Result, width, trees);
        var bytes = new byte[matches.Length * 4];
        for (var i = 0; i < matches.Length; i++) BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(i * 4), matches[i]);
        return new(encoded.Bytes, encoded.BitCount, encoded.TokenStart, parsed.Result.Count,
            trees, Convert.ToHexStringLower(SHA256.HashData(bytes)), parsed.Summary);
    }

    public static Vp8lPrefixResult Build(byte[] rgba)
    {
        const int width = 513, height = 259, tileBits = 3, quality = 75;
        if (rgba.Length != width * height * 4) throw new ArgumentException("Expected the frozen RGBA8 raster.");
        var modes = Vp8lPredictorParity.EncodeModes(rgba, width, height, tileBits);
        var residual = Vp8lPredictorParity.Residual(rgba, width, height, tileBits, modes);
        var color = Vp8lCrossColorParity.Select(residual, width, height, tileBits, quality);
        var predictor = Encode(modes.Select(m => 0xff000000u | (uint)m << 8).ToArray(), 65, quality);
        var colorPixels = new uint[color.Map.Length / 4];
        for (var i = 0; i < colorPixels.Length; i++) colorPixels[i] = BinaryPrimitives.ReadUInt32LittleEndian(color.Map.AsSpan(i * 4));
        var colorImage = Encode(colorPixels, 65, quality);
        var bits = new Vp8lParityBits();
        bits.Put(0x2f, 8); bits.Put(width - 1, 14); bits.Put(height - 1, 14);
        var alpha = false; for (var i = 3; i < rgba.Length; i += 4) alpha |= rgba[i] != 255;
        bits.Put(alpha ? 1 : 0, 1); bits.Put(0, 3);
        bits.Put(1, 1); bits.Put(2, 2); // Subtract green.
        bits.Put(1, 1); bits.Put(0, 2); bits.Put(tileBits - 2, 3);
        Append(predictor);
        bits.Put(1, 1); bits.Put(1, 2); bits.Put(tileBits - 2, 3);
        Append(colorImage);
        return new(bits.Finish(), bits.Count, modes, residual, color.Map, color.Coded, predictor, colorImage);

        void Append(Vp8lAuxiliaryResult image)
        {
            for (var bit = 0; bit < image.BitCount; bit++) bits.Put((image.Bytes[bit >> 3] >> (bit & 7)) & 1, 1);
        }
    }
}
