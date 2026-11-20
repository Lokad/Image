using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Lokad.Image;
using Lokad.Image.Tooling;

internal sealed record QualityError(long SumSquared, int Maximum, int Samples, int Scale = 1)
{
    public double RgbMaximum => (double)Maximum / Scale;
    public double RgbRmse => Math.Sqrt((double)SumSquared / Samples) / Scale;
    public double? RgbPsnr => SumSquared == 0 ? null : 20 * Math.Log10(255 / RgbRmse);
}

internal sealed record QualitySample(int Setting, int EncodedBytes, string EncodedSha256,
    string NativeDecodedSha256, string PublicDecodedSha256, string Codec, string Sampling,
    QualityError SourceError, QualityError DecoderDifference, bool AlphaExact = true, string? AlphaSha256 = null);
internal sealed record QualityMatch(QualitySample Native, QualitySample ClosestOwned, double? PsnrGapDb, bool Matched, string? ExclusionReason);

// Quality checks shared by the warm reference preparation and validation.
internal static class EncoderQualityCalibration
{
    public const double MaximumPsnrGapDb = 0.25;
    public static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    private static JpegSampling Sampling(QualityEncoder encoder) => encoder switch
    {
        QualityEncoder.Jpeg444 => JpegSampling.YCbCr444, QualityEncoder.Jpeg422 => JpegSampling.YCbCr422,
        QualityEncoder.Jpeg420 => JpegSampling.YCbCr420, _ => throw new ArgumentException("Encoder is not JPEG.", nameof(encoder))
    };

    private static string[] DecodeArguments(EncoderQualityProfile profile) => profile.Encoder == QualityEncoder.Vp8
        ? WebpOracle.DecodeArguments(profile.Width, profile.Height, profile.HasAlpha)
        : JpegOracle.DecodeArguments(profile.Width, profile.Height, Sampling(profile.Encoder));

    public static QualitySample Validate(byte[] encoded, byte[] reference, EncoderQualityProfile profile, int setting, string ffmpeg, JpegSampling? jpegSampling = null)
    {
        var info = ImageCodec.Identify(encoded);
        var format = profile.Encoder == QualityEncoder.Vp8 ? ImageFormat.WebP : ImageFormat.Jpeg;
        if ((info.Format, info.Width, info.Height, info.BitDepth, info.HasAlpha) != (format, profile.Width, profile.Height, 8, profile.EncodedHasAlpha))
            throw new InvalidOperationException("Quality bitstream geometry, precision or opacity differs.");
        var sampling = "4:2:0";
        if (format == ImageFormat.Jpeg)
        {
            var header = JpegQualitySyntax.Read(encoded);
            if (!JpegBenchmarkCase.MatchesSampling(header, jpegSampling ?? Sampling(profile.Encoder))) throw new InvalidOperationException("Quality JPEG sampling ratios differ.");
            sampling = header.Sampling;
        }
        else RequireVp8(encoded);
        var native = OracleProcess.Run(ffmpeg, jpegSampling.HasValue
            ? JpegOracle.DecodeArguments(profile.Width, profile.Height, jpegSampling.Value) : DecodeArguments(profile), encoded);
        using var image = ImageCodec.Decode(encoded); var decoded = OriginalPixels.Bytes(image);
        if ((image.Width, image.Height, image.BitDepth) != (profile.Width, profile.Height, 8)) throw new InvalidOperationException("Quality public decoded geometry differs.");
        var agreement = Error(native, decoded);
        if (agreement.Maximum > 4 || agreement.RgbRmse > 1.5) throw new InvalidOperationException("Quality decoder agreement exceeded the frozen bounds.");
        return new(setting, encoded.Length, JpegBenchmarkCase.Hash(encoded), JpegBenchmarkCase.Hash(native),
            JpegBenchmarkCase.Hash(decoded), format == ImageFormat.WebP ? "VP8" : "JPEG", sampling, Error(reference, native, profile.EncodedHasAlpha), agreement,
            AlphaSha256: EncoderQualitySource.AlphaHash(reference));
    }

    private static void RequireVp8(ReadOnlySpan<byte> encoded)
    {
        var found = false;
        for (var offset = 12; offset < encoded.Length;)
        {
            if (encoded.Length - offset < 8) throw new InvalidOperationException("Truncated calibration WebP chunk.");
            var size = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(encoded[(offset + 4)..]);
            var length = 8L + size + (size & 1);
            if (length > encoded.Length - offset) throw new InvalidOperationException("Invalid calibration WebP chunk length.");
            if (encoded.Slice(offset, 4).SequenceEqual("VP8L"u8)) throw new InvalidOperationException("Lossy calibration emitted VP8L.");
            if (encoded.Slice(offset, 4).SequenceEqual("VP8 "u8)) found = true;
            offset += (int)length;
        }
        if (!found) throw new InvalidOperationException("Lossy calibration has no VP8 chunk.");
    }

    private static QualityError Error(byte[] reference, byte[] actual, bool weightAlpha = false)
    {
        if (reference.Length != actual.Length || actual.Length == 0 || actual.Length % 4 != 0) throw new InvalidOperationException("Quality raster sizes differ.");
        long sum = 0; var maximum = 0;
        for (var index = 0; index < actual.Length; index++)
        {
            if (index % 4 == 3)
            {
                if (reference[index] != actual[index]) throw new InvalidOperationException("Quality alpha differs.");
                continue;
            }
            var error = Math.Abs(reference[index] - actual[index]) * (weightAlpha ? reference[(index & ~3) + 3] : 1);
            maximum = Math.Max(maximum, error); sum += (long)error * error;
        }
        return new(sum, maximum, actual.Length / 4 * 3, weightAlpha ? 255 : 1);
    }

    public static QualityMatch Match(QualitySample native, IReadOnlyList<QualitySample> owned)
    {
        if (owned.Count == 0 || native.SourceError.SumSquared < 0 || native.SourceError.Samples < 1 || native.SourceError.Scale < 1)
            throw new ArgumentException("Quality matching requires nonempty valid samples.");
        QualitySample? closest = null;
        foreach (var candidate in owned)
        {
            if (candidate.SourceError.Samples != native.SourceError.Samples || candidate.SourceError.Scale != native.SourceError.Scale || candidate.SourceError.SumSquared < 0)
                throw new ArgumentException("Quality matching requires identical sample counts/scales and nonnegative squared errors.");
            if (closest is null) { closest = candidate; continue; }
            var distance = CompareDistance(candidate.SourceError.SumSquared, closest.SourceError.SumSquared, native.SourceError.SumSquared);
            if (distance < 0 || distance == 0 && (candidate.EncodedBytes < closest.EncodedBytes
                || candidate.EncodedBytes == closest.EncodedBytes && candidate.Setting < closest.Setting)) closest = candidate;
        }
        var target = native.SourceError.SumSquared; var value = closest!.SourceError.SumSquared;
        double? gap = target == 0 || value == 0 ? target == value ? 0 : null : 10 * Math.Log10((double)Math.Max(target, value) / Math.Min(target, value));
        var matched = gap is <= MaximumPsnrGapDb;
        var reason = matched ? null : target == 0 || value == 0
            ? "Exact and nonexact reconstruction cannot be matched by finite PSNR."
            : target < owned.Min(sample => sample.SourceError.SumSquared) || target > owned.Max(sample => sample.SourceError.SumSquared)
                ? "Native target lies outside the sampled owned error range."
                : "No sampled owned setting meets the fixed 0.25 dB band within the bounded search budget.";
        return new(native, closest, gap, matched, reason);
    }

    private static int CompareDistance(long first, long second, long target)
    {
        var a = Ratio(first, target); var b = Ratio(second, target);
        if (a.Denominator == 0 || b.Denominator == 0) return (a.Denominator == 0).CompareTo(b.Denominator == 0);
        return ((BigInteger)a.Numerator * b.Denominator).CompareTo((BigInteger)b.Numerator * a.Denominator);

        static (long Numerator, long Denominator) Ratio(long value, long target)
            => value == target ? (1, 1) : (Math.Max(value, target), Math.Min(value, target));
    }
}
