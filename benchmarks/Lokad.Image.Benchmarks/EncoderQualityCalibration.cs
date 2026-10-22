using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
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
internal sealed record QualityCalibrationProfile(EncoderQualityProfile Profile, string SourceSha256, string ReferenceSha256,
    string SourceAlphaSha256, int SourceBytes, string[] NativeDecodeArguments, string[][] NativeEncodeArguments, QualitySample[] OwnedSettings, QualityMatch[] Matches,
    string[] NativeEncodedFiles, string[] ClosestOwnedEncodedFiles, int[] OwnedEvaluationOrder);
internal sealed record QualityCalibrationCatalog(int SchemaVersion, string Project, string Scope, bool Complete, string GeneratedUtc,
    string Runtime, string Platform, string LibrarySha256, string ProbeSha256, string BenchmarkAssemblySha256,
    string FfmpegSha256, string FfmpegVersion, string Recipe, string Method, double MaximumPsnrGapDb,
    string OwnedQualityPolicy, int MaximumOwnedSettings,
    QualityCalibrationProfile[] Profiles);

// Development-only quality matching. No stopwatch, profiling or encoder tuning.
internal static class EncoderQualityCalibration
{
    public const double MaximumPsnrGapDb = 0.25;
    public const int MaximumOwnedSettings = 8;
    public const string OwnedQualityPolicy = "bounded-v1: seeds 1,25,50,75,100; at most three measured log-SSE bracket refinements; closest sampled setting only";
    private static readonly int[] SeedQualities = [1, 25, 50, 75, 100];
    public static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public static void Generate(string scope)
    {
        ValidateControls();
        using var identity = new JpegBenchmarkCase();
        if (!File.Exists(identity.Ffmpeg)) throw new InvalidOperationException("Quality calibration requires an explicit FFmpeg executable for provenance.");
        var directory = Path.Combine(identity.Root, "artifacts", "encoder-quality", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var manifest = Path.Combine(directory, "calibration.json");
        // JPEG is qualified by complete reference packets. Keep this remaining
        // calibration only for WebP until its stronger gates replace it too.
        var profiles = Profiles(scope).Where(profile => profile.Encoder == QualityEncoder.Vp8).ToArray();
        if (profiles.Length == 0) throw new ArgumentException("This scope has no remaining WebP calibration controls; JPEG uses exact packet checks.", nameof(scope));
        var catalog = new QualityCalibrationCatalog(4, "Lokad.Image", scope, false, DateTime.UtcNow.ToString("O", System.Globalization.CultureInfo.InvariantCulture),
            RuntimeInformation.FrameworkDescription, RuntimeInformation.RuntimeIdentifier,
            JpegBenchmarkCase.Hash(File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(identity.Probe)!, "Lokad.Image.dll"))),
            JpegBenchmarkCase.Hash(File.ReadAllBytes(identity.Probe)), JpegBenchmarkCase.Hash(File.ReadAllBytes(typeof(EncoderQualityCalibration).Assembly.Location)),
            JpegBenchmarkCase.Hash(File.ReadAllBytes(identity.Ffmpeg)), Encoding.UTF8.GetString(OracleProcess.Run(identity.Ffmpeg, ["-version"])).Replace("\r\n", "\n"),
            EncoderQualitySource.Recipe,
            "Untimed RGBA8/RGBA16LE calibration against MJPEG qscale 16/5 or libwebp quality 50/85; VP8 efforts fixed at 4. Native encoding uses actual single-file commands, normal CPU capabilities and one codec/filter thread. JPEG transparent input has an explicit RGB background and one rounding of alpha flattening plus depth reduction, independently reproduced by the native geq point equation; output alpha is 255. VP8 alpha equals once-normalized source alpha. Source/output alpha hashes are distinct. Raw public/native decoder RGB agreement stays unweighted maximum 4/RMSE 1.5, including hidden RGB. JPEG source quality uses whole-raster RGB after flattening; opaque quality uses unchanged RGB. Transparent VP8 uses integer SSE of alpha*(sourceRGB-nativeRGB), scale 255 for RMSE/PSNR, before composite rounding. Exact alpha makes this the unrounded component error over any background; zero-alpha contributions count as samples. At most eight owned settings are measured: five fixed seeds and at most three bounded refinements. Refinements are proposals from measured local error brackets, not a global monotonicity or optimality assumption. Exact SSE ratios select the closest sampled PSNR, with size/setting ties; both exact reconstructions match separately. Fixed maximum 0.25 dB gap, all unmatched anchors/closest sampled results retained. Numeric controls are encoder-specific; source error matching is not perceptual equivalence or equal effort. No timings, tuning or speed conclusions.",
            MaximumPsnrGapDb, OwnedQualityPolicy, MaximumOwnedSettings, []);
        Save(manifest, catalog);
        var completed = new List<QualityCalibrationProfile>();
        foreach (var profile in profiles)
        {
            var record = Calibrate(profile, directory);
            Save(Path.Combine(directory, profile + ".json"), record);
            completed.Add(record); catalog = catalog with { Profiles = completed.ToArray() };
            Save(manifest, catalog);
            Console.WriteLine($"Calibrated {profile}; {record.Matches.Count(match => match.Matched)}/2 anchors within the fixed band ({completed.Count}/{profiles.Length} profiles).");
        }
        Save(manifest, catalog with { Complete = true });
        Console.WriteLine($"Quality calibration complete: {manifest}; no timing measurements.");
    }

    private static QualityCalibrationProfile Calibrate(EncoderQualityProfile profile, string directory)
    {
        using var state = new JpegBenchmarkCase(profile.Encoder == QualityEncoder.Vp8 ? ".webp" : "");
        var source = EncoderQualitySource.Generate(profile); var reference = EncoderQualitySource.Reference(source, profile);
        File.WriteAllBytes(state.Input, source);
        var normalization = profile.BackgroundRgb.HasValue ? JpegFlattenOracle.Arguments(profile.Width, profile.Height, profile.Depth, profile.BackgroundRgb.Value)
            : profile.Depth == 16 ? RgbaOracle.Normalize16Arguments(profile.Width, profile.Height) : null;
        if (normalization is not null && !OracleProcess.Run(state.Ffmpeg, normalization, source).AsSpan().SequenceEqual(reference))
            throw new InvalidOperationException("Calibration native flattening/depth normalization differs.");
        var nativeSettings = profile.Encoder == QualityEncoder.Vp8 ? new[] { 50, 85 } : new[] { 16, 5 };
        var native = new List<QualitySample>(); var nativeArguments = new List<string[]>(); var nativeFiles = new List<string>();
        foreach (var setting in nativeSettings)
        {
            var arguments = NativeArguments(state, profile, setting); OracleProcess.Run(state.Ffmpeg, arguments);
            var bytes = File.ReadAllBytes(state.NativeOutput);
            native.Add(Validate(bytes, reference, profile, setting, state.Ffmpeg));
            var name = profile + "-native-" + setting + (profile.Encoder == QualityEncoder.Vp8 ? ".webp" : ".jpg");
            File.WriteAllBytes(Path.Combine(directory, name), bytes); nativeFiles.Add(name);
            nativeArguments.Add(arguments.Select(argument => argument.Replace(state.DirectoryPath, "{case}", StringComparison.Ordinal)).ToArray());
        }
        using var image = JpegBenchmarkCase.Buffer(source, profile.Width, profile.Height, profile.Depth);
        var evaluationOrder = new List<int>(); var validated = new Dictionary<string, QualitySample>(StringComparer.Ordinal);
        var owned = SampleOwned(native, quality =>
        {
            evaluationOrder.Add(quality);
            using var output = new MemoryStream(); Encode(image, output, profile, quality);
            var encoded = output.ToArray(); var hash = JpegBenchmarkCase.Hash(encoded);
            if (!validated.TryGetValue(hash, out var sample))
            {
                sample = Validate(encoded, reference, profile, quality, state.Ffmpeg); validated.Add(hash, sample);
            }
            return sample with { Setting = quality };
        });
        var matches = native.Select(anchor => Match(anchor, owned)).ToArray(); var closestFiles = new List<string>();
        foreach (var match in matches)
        {
            using var output = new MemoryStream(); Encode(image, output, profile, match.ClosestOwned.Setting);
            var bytes = output.ToArray();
            if (JpegBenchmarkCase.Hash(bytes) != match.ClosestOwned.EncodedSha256) throw new InvalidOperationException("Selected owned quality bitstream did not reproduce.");
            var name = profile + "-owned-" + match.ClosestOwned.Setting + (profile.Encoder == QualityEncoder.Vp8 ? ".webp" : ".jpg");
            File.WriteAllBytes(Path.Combine(directory, name), bytes); closestFiles.Add(name);
        }
        return new(profile, JpegBenchmarkCase.Hash(source), JpegBenchmarkCase.Hash(reference), EncoderQualitySource.AlphaHash(EncoderQualitySource.Normalize(source, profile.Depth)), source.Length,
            DecodeArguments(profile), nativeArguments.ToArray(), owned, matches, nativeFiles.ToArray(), closestFiles.ToArray(), evaluationOrder.ToArray());
    }

    internal static QualitySample[] SampleOwned(IReadOnlyList<QualitySample> native, Func<int, QualitySample> evaluate)
    {
        var owned = new List<QualitySample>();
        var attempts = native.ToDictionary(sample => sample.Setting, _ => 0);
        foreach (var quality in SeedQualities) Add(quality);
        while (owned.Count < MaximumOwnedSettings)
        {
            var proposal = native.Where(anchor => !Match(anchor, owned).Matched)
                .OrderBy(anchor => attempts[anchor.Setting])
                .ThenByDescending(anchor => Match(anchor, owned).PsnrGapDb ?? double.PositiveInfinity)
                .ThenBy(anchor => anchor.Setting)
                .Select(anchor => (Anchor: anchor, Quality: ProposeQuality(anchor, owned)))
                .FirstOrDefault(item => item.Quality.HasValue);
            if (proposal.Quality is not int quality) break;
            Add(quality); attempts[proposal.Anchor.Setting]++;
        }
        return owned.OrderBy(sample => sample.Setting).ToArray();

        void Add(int quality)
        {
            if (quality is < 1 or > 100 || owned.Any(sample => sample.Setting == quality))
                throw new InvalidOperationException("Bounded quality sampler proposed a duplicate or invalid setting.");
            var sample = evaluate(quality);
            if (sample.Setting != quality) throw new InvalidOperationException("Quality sample setting differs from its requested evaluation.");
            owned.Add(sample);
        }
    }

    private static int? ProposeQuality(QualitySample native, IReadOnlyList<QualitySample> owned)
    {
        var ordered = owned.OrderBy(sample => sample.Setting).ToArray();
        var intervals = ordered.Zip(ordered.Skip(1)).Where(pair => pair.Second.Setting - pair.First.Setting > 1).ToArray();
        if (intervals.Length == 0) return null;
        var target = native.SourceError.SumSquared;
        var brackets = intervals.Where(pair => Math.Min(pair.First.SourceError.SumSquared, pair.Second.SourceError.SumSquared) <= target
            && target <= Math.Max(pair.First.SourceError.SumSquared, pair.Second.SourceError.SumSquared)).ToArray();
        var nearest = Match(native, owned).ClosestOwned.Setting;
        var interval = brackets.Length > 0
            ? brackets.OrderBy(pair => pair.Second.Setting - pair.First.Setting).ThenBy(pair => pair.First.Setting).First()
            : intervals.OrderBy(pair => Math.Min(Math.Abs(pair.First.Setting - nearest), Math.Abs(pair.Second.Setting - nearest)))
                .ThenByDescending(pair => pair.Second.Setting - pair.First.Setting).ThenBy(pair => pair.First.Setting).First();
        var first = interval.First; var second = interval.Second;
        var estimate = (first.Setting + second.Setting) / 2.0;
        if (brackets.Length > 0 && target > 0 && first.SourceError.SumSquared > 0 && second.SourceError.SumSquared > 0
            && first.SourceError.SumSquared != second.SourceError.SumSquared)
            estimate = first.Setting + (Math.Log(target) - Math.Log(first.SourceError.SumSquared))
                / (Math.Log(second.SourceError.SumSquared) - Math.Log(first.SourceError.SumSquared)) * (second.Setting - first.Setting);
        return Math.Clamp((int)Math.Round(estimate, MidpointRounding.AwayFromZero), first.Setting + 1, second.Setting - 1);
    }

    internal static void ValidateSamplingPolicy(QualityCalibrationProfile profile)
    {
        var owned = profile.OwnedSettings;
        if (owned.Length is < 5 or > MaximumOwnedSettings || owned.Any(sample => sample.Setting is < 1 or > 100)
            || !owned.Select(sample => sample.Setting).SequenceEqual(owned.Select(sample => sample.Setting).Distinct().Order())
            || profile.OwnedEvaluationOrder is null || profile.OwnedEvaluationOrder.Length != owned.Length)
            throw new InvalidOperationException("Quality calibration must contain five to eight unique ordered sampled settings and its evaluation order.");
        var replay = new List<int>();
        var reproduced = SampleOwned(profile.Matches.Select(match => match.Native).ToArray(), quality =>
        {
            replay.Add(quality);
            return owned.Single(sample => sample.Setting == quality);
        });
        if (!replay.SequenceEqual(profile.OwnedEvaluationOrder) || !reproduced.SequenceEqual(owned))
            throw new InvalidOperationException("Recorded bounded quality sampling does not reproduce.");
    }

    public static void Encode(ImageBuffer source, Stream output, EncoderQualityProfile profile, int quality)
    {
        if (profile.Encoder == QualityEncoder.Vp8)
            ImageCodec.EncodeWebp(source, output, new WebpEncodeOptions { Lossless = false, Quality = quality, Effort = 4 });
        else ImageCodec.EncodeJpeg(source, output, new JpegEncodeOptions { Quality = quality, Sampling = Sampling(profile.Encoder), BackgroundRgb = profile.BackgroundRgb });
    }

    public static EncoderQualityProfile[] Profiles(string scope)
    {
        if (scope == "representative")
            return [new(129, 8, QualityEncoder.Jpeg444, QualityPattern.ColorTiles),
                new(129, 8, QualityEncoder.Jpeg422, QualityPattern.Asymmetric),
                new(129, 8, QualityEncoder.Jpeg420, QualityPattern.ProceduralTexture),
                new(129, 8, QualityEncoder.Vp8, QualityPattern.SoftGradient),
                new(513, 16, QualityEncoder.Vp8, QualityPattern.GrayRamp, QualityAlpha.Gradient),
                new(513, 16, QualityEncoder.Vp8, QualityPattern.SeededNoise, QualityAlpha.BinaryEdges),
                .. new[] { QualityEncoder.Jpeg444, QualityEncoder.Jpeg422, QualityEncoder.Jpeg420 }.SelectMany(encoder =>
                    new[] { new EncoderQualityProfile(513, 16, encoder, QualityPattern.GrayRamp, QualityAlpha.Gradient, 0x204060),
                        new EncoderQualityProfile(513, 16, encoder, QualityPattern.SeededNoise, QualityAlpha.BinaryEdges, 0x204060) })];
        var widths = scope == "larger" ? new[] { 1025, 2049 } : new[] { 129, 513, 1025, 2049 };
        var opaque = (from width in widths from depth in new[] { 8, 16 }
                      from encoder in Enum.GetValues<QualityEncoder>() from pattern in Enum.GetValues<QualityPattern>()
                      select new EncoderQualityProfile(width, depth, encoder, pattern)).ToArray();
        var vp8Transparent = (from width in widths from depth in new[] { 8, 16 }
                           from alpha in new[] { QualityAlpha.Gradient, QualityAlpha.BinaryEdges } from pattern in Enum.GetValues<QualityPattern>()
                           select new EncoderQualityProfile(width, depth, QualityEncoder.Vp8, pattern, alpha)).ToArray();
        var jpegTransparent = (from width in widths from depth in new[] { 8, 16 }
                               from encoder in new[] { QualityEncoder.Jpeg444, QualityEncoder.Jpeg422, QualityEncoder.Jpeg420 }
                               from alpha in new[] { QualityAlpha.Gradient, QualityAlpha.BinaryEdges } from pattern in Enum.GetValues<QualityPattern>()
                               select new EncoderQualityProfile(width, depth, encoder, pattern, alpha, 0x204060)).ToArray();
        return scope switch
        {
            "full" or "larger" => [.. opaque, .. vp8Transparent, .. jpegTransparent], "opaque" => opaque, "transparent" => [.. vp8Transparent, .. jpegTransparent],
            "vp8-transparent" => vp8Transparent, "jpeg-transparent" => jpegTransparent,
            "control" => ControlProfiles(129), "larger-control" => ControlProfiles(1025),
            "varied-control" => [.. Enum.GetValues<QualityPattern>().Select(pattern => new EncoderQualityProfile(129, 8, QualityEncoder.Jpeg444, pattern, QualityAlpha.Gradient, 0x204060)),
                .. Enum.GetValues<QualityPattern>().Select(pattern => new EncoderQualityProfile(129, 8, QualityEncoder.Vp8, pattern))],
            _ => throw new ArgumentException("Calibration scope must be representative, full, larger, opaque, transparent, vp8-transparent, jpeg-transparent, control, larger-control or varied-control.", nameof(scope))
        };
    }

    private static EncoderQualityProfile[] ControlProfiles(int width)
        => [.. Enum.GetValues<QualityEncoder>().Select(encoder => new EncoderQualityProfile(width, 8, encoder, QualityPattern.SoftGradient)),
                new(width, 16, QualityEncoder.Vp8, QualityPattern.SoftGradient, QualityAlpha.Gradient),
                new(width, 16, QualityEncoder.Vp8, QualityPattern.SoftGradient, QualityAlpha.BinaryEdges),
                .. new[] { QualityEncoder.Jpeg444, QualityEncoder.Jpeg422, QualityEncoder.Jpeg420 }.SelectMany(encoder =>
                    new[] { QualityAlpha.Gradient, QualityAlpha.BinaryEdges }.Select(alpha => new EncoderQualityProfile(width, 16, encoder, QualityPattern.SoftGradient, alpha, 0x204060)))];

    public static JpegSampling Sampling(QualityEncoder encoder) => encoder switch
    {
        QualityEncoder.Jpeg444 => JpegSampling.YCbCr444, QualityEncoder.Jpeg422 => JpegSampling.YCbCr422,
        QualityEncoder.Jpeg420 => JpegSampling.YCbCr420, _ => throw new ArgumentException("Encoder is not JPEG.", nameof(encoder))
    };

    public static string[] NativeArguments(JpegBenchmarkCase state, EncoderQualityProfile profile, int setting)
    {
        if (profile.Encoder != QualityEncoder.Vp8)
            return JpegBenchmarkCase.EncodeArguments(state.Input, state.NativeOutput, profile.Width, profile.Height, profile.Depth, Sampling(profile.Encoder), setting, profile.BackgroundRgb);
        var normalization = profile.Depth == 16 ? RgbaOracle.Normalize16Filter + "," : "";
        return ["-hide_banner", "-loglevel", "error", "-threads:v", "1", "-f", "rawvideo",
            "-pixel_format", profile.Depth == 16 ? "rgba64le" : "rgba", "-video_size", $"{profile.Width}x{profile.Height}",
            "-framerate", "1", "-i", state.Input, "-filter_threads", "1", "-vf", normalization + "format=bgra",
            "-frames:v", "1", "-threads:v", "1", "-c:v", "libwebp", "-lossless", "0", "-quality", JpegBenchmarkCase.Number(setting),
            "-compression_level", "4", "-map_metadata", "-1", "-f", "webp", "-y", state.NativeOutput];
    }

    public static string[] DecodeArguments(EncoderQualityProfile profile) => profile.Encoder == QualityEncoder.Vp8
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

    public static void ValidateControls()
    {
        byte[] alpha = [0, 1, 127, 128, 254, 255]; var reference = new byte[alpha.Length * 4]; var actual = new byte[reference.Length];
        for (var pixel = 0; pixel < alpha.Length; pixel++)
        {
            reference[pixel * 4 + 3] = actual[pixel * 4 + 3] = alpha[pixel];
            actual[pixel * 4] = 255; actual[pixel * 4 + 1] = 128; actual[pixel * 4 + 2] = 1;
        }
        var weighted = Error(reference, actual, weightAlpha: true);
        if (weighted.SumSquared != 162055L * 81410 || weighted.Maximum != 65025 || weighted.Samples != 18 || weighted.Scale != 255
            || Error(reference[..4], actual[..4], weightAlpha: true).SumSquared != 0 || Error(reference[..4], actual[..4]).Maximum != 255)
            throw new InvalidOperationException("Literal visible-error/hidden-RGB controls failed.");
        actual[3] = 1;
        try { _ = Error(reference, actual, weightAlpha: true); throw new Exception("Alpha mismatch accepted."); }
        catch (InvalidOperationException) { }
        QualitySample Sample(int setting, long squared, int bytes = 100) => new(setting, bytes, "", "", "", "control", "",
            new(squared, 1, 3), new(0, 0, 3));
        var native = Sample(5, 100);
        if (Match(native, [Sample(1, 101), Sample(2, 700), Sample(3, 100)]).ClosestOwned.Setting != 3
            || Match(native, [Sample(3, 50), Sample(2, 200), Sample(1, 200, 99)]).ClosestOwned.Setting != 1
            || Match(native, [Sample(3, 101), Sample(2, 100), Sample(1, 100)]).ClosestOwned.Setting != 1
            || Match(native, [Sample(1, 10000)]).Matched
            || Match(Sample(5, 0), [Sample(1, 1)]).Matched
            || !Match(Sample(5, 0), [Sample(1, 0)]).Matched
            || !Match(Sample(5, 1000), [Sample(1, 1059)]).Matched
            || Match(Sample(5, 1000), [Sample(1, 1060)]).Matched
            || Match(native, [Sample(1, 10000), Sample(2, 99)]).ClosestOwned.Setting != 2)
            throw new InvalidOperationException("Independent quality-selection controls failed.");
        var evaluations = new List<int>();
        var bounded = SampleOwned([Sample(16, 37), Sample(5, 74)], quality =>
        {
            evaluations.Add(quality); return Sample(quality, 10000 + quality % 2);
        });
        if (evaluations.Count != MaximumOwnedSettings || evaluations.Distinct().Count() != MaximumOwnedSettings
            || !evaluations.Take(5).SequenceEqual(SeedQualities) || bounded.Length != MaximumOwnedSettings
            || !bounded.Select(sample => sample.Setting).SequenceEqual(evaluations.Order()))
            throw new InvalidOperationException("Bounded calibration exceeded its budget or changed seed/order policy.");
        evaluations.Clear();
        var exact = SampleOwned([Sample(16, 100), Sample(5, 100)], quality =>
        {
            evaluations.Add(quality); return Sample(quality, 100);
        });
        if (exact.Length != 5 || evaluations.Count != 5)
            throw new InvalidOperationException("Already matched calibration must stop after the five seeds.");
        if (ProposeQuality(Sample(16, 400), [Sample(1, 10000), Sample(25, 1000), Sample(50, 100), Sample(75, 500), Sample(100, 10)]) != 35)
            throw new InvalidOperationException("Nonmonotonic measured-bracket proposal control failed.");
        var representative = Profiles("representative");
        if (representative.Length != 12 || representative.Distinct().Count() != 12
            || representative.Select(p => (p.Width, p.Depth, p.Pattern, p.Alpha)).Distinct().Count() != 6
            || representative.Select(p => p.Pattern).Distinct().Count() != 6
            || representative.Select(p => p.Encoder).Distinct().Count() != 4
            || representative.Select(p => p.Depth).Distinct().Count() != 2
            || representative.Select(p => p.Alpha).Distinct().Count() != 3)
            throw new InvalidOperationException("Representative calibration must cover twelve codec profiles from six original inputs.");
        foreach (var depth in new[] { 8, 16 })
        foreach (var (pattern, name) in new[] { (QualityPattern.GrayRamp, "gray-ramp"), (QualityPattern.ColorTiles, "color-tiles"),
            (QualityPattern.SoftGradient, "soft-gradient"), (QualityPattern.Asymmetric, "asymmetric-v1") })
        {
            var source = EncoderQualitySource.Generate(new(129, depth, QualityEncoder.Jpeg444, pattern));
            if (!source.AsSpan().SequenceEqual(JpegQualityCorpus.Source(name, depth, JpegSampling.YCbCr444)))
                throw new InvalidOperationException("Generalized benchmark recipe changed the original 129x67 samples.");
        }
    }

    private static void Save<T>(string path, T value) => File.WriteAllText(path, JsonSerializer.Serialize(value, Json) + "\n");
}
