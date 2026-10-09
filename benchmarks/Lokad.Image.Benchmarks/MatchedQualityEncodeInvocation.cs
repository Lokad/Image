using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

public sealed record MatchedQualityWorkload(EncoderQualityProfile Profile, int NativeSetting, int OwnedSetting)
{
    public override string ToString() => $"{Profile}-native{NativeSetting}-owned{OwnedSetting}";
}

/// <summary>Matched measured RGB quality; one raw input and one still output per process.</summary>
[MemoryDiagnoser]
public class MatchedQualityEncodeInvocation
{
    [ParamsSource(nameof(Cases))] public MatchedQualityWorkload Workload { get; set; } = null!;
    public IEnumerable<MatchedQualityWorkload> Cases
    {
        get
        {
            using var identity = new JpegBenchmarkCase(); var (_, catalog) = ReadCatalog(identity);
            return catalog.Profiles.SelectMany(profile => profile.Matches.Where(match => match.Matched)
                .Select(match => new MatchedQualityWorkload(profile.Profile, match.Native.Setting, match.ClosestOwned.Setting))).ToArray();
        }
    }

    private JpegBenchmarkCase? state;
    private string[] managedArguments = [], nativeArguments = [];

    [GlobalSetup]
    public void Setup()
    {
        var profile = Workload.Profile;
        state = new JpegBenchmarkCase(profile.Encoder == QualityEncoder.Vp8 ? ".webp" : "");
        try
        {
            var (path, catalog) = ReadCatalog(state);
            var calibrated = catalog.Profiles.Single(row => row.Profile == profile);
            var selected = calibrated.Matches.Single(match => match.Native.Setting == Workload.NativeSetting);
            if (!selected.Matched || selected.ClosestOwned.Setting != Workload.OwnedSetting)
                throw new InvalidOperationException("Requested benchmark row is not a matched calibration pair.");
            var source = EncoderQualitySource.Generate(profile); var reference = EncoderQualitySource.Reference(source, profile);
            if (JpegBenchmarkCase.Hash(source) != calibrated.SourceSha256 || JpegBenchmarkCase.Hash(reference) != calibrated.ReferenceSha256
                || EncoderQualitySource.AlphaHash(EncoderQualitySource.Normalize(source, profile.Depth)) != calibrated.SourceAlphaSha256)
                throw new InvalidOperationException("Quality benchmark source changed since calibration.");
            File.WriteAllBytes(state.Input, source);
            nativeArguments = EncoderQualityCalibration.NativeArguments(state, profile, Workload.NativeSetting);
            var command = profile.Encoder == QualityEncoder.Vp8 ? "encode-webp" : "encode-jpeg";
            var last = profile.Encoder == QualityEncoder.Vp8 ? "4" : EncoderQualityCalibration.Sampling(profile.Encoder).ToString();
            managedArguments = state.ManagedArguments(command, state.ManagedOutput, JpegBenchmarkCase.Number(profile.Width),
                JpegBenchmarkCase.Number(profile.Height), JpegBenchmarkCase.Number(profile.Depth), JpegBenchmarkCase.Number(Workload.OwnedSetting), last);
            if (profile.BackgroundRgb.HasValue) managedArguments = [.. managedArguments, profile.BackgroundRgb.Value.ToString("x6", System.Globalization.CultureInfo.InvariantCulture)];
            Ffmpeg(); Managed();
            using var image = JpegBenchmarkCase.Buffer(source, profile.Width, profile.Height, profile.Depth);
            using var output = new MemoryStream(); EncoderQualityCalibration.Encode(image, output, profile, Workload.OwnedSetting);
            if (!output.ToArray().AsSpan().SequenceEqual(File.ReadAllBytes(state.ManagedOutput)))
                throw new InvalidOperationException("Matched-quality child output differs from the exact public encode workflow.");
            var native = EncoderQualityCalibration.Validate(File.ReadAllBytes(state.NativeOutput), reference, profile, Workload.NativeSetting, state.Ffmpeg);
            var managed = EncoderQualityCalibration.Validate(File.ReadAllBytes(state.ManagedOutput), reference, profile, Workload.OwnedSetting, state.Ffmpeg);
            if (native != selected.Native || managed != selected.ClosestOwned)
                throw new InvalidOperationException("Matched-quality bitstreams, pixels or metrics changed since calibration.");
            var actualMatch = EncoderQualityCalibration.Match(native, [managed]);
            if (!actualMatch.Matched) throw new InvalidOperationException("Executed outputs do not meet the fixed quality band.");
            state.Report(nameof(MatchedQualityEncodeInvocation), new { profile.Width, profile.Height, profile.Depth, profile.Encoder, profile.Pattern, profile.Alpha, profile.EncodedHasAlpha, profile.BackgroundRgb, profile.SourceMetric,
                nativeSetting = Workload.NativeSetting, ownedQuality = Workload.OwnedSetting, frames = 1, codecThreads = 1, filterThreads = 1,
                webpEffort = profile.Encoder == QualityEncoder.Vp8 ? (int?)4 : null, maximumPsnrGapDb = EncoderQualityCalibration.MaximumPsnrGapDb },
                managedArguments, nativeArguments, managed, native,
                stages: new { calibrationSha256 = JpegBenchmarkCase.Hash(File.ReadAllBytes(path)), calibrationScope = catalog.Scope,
                    sourceSha256 = calibrated.SourceSha256, referenceSha256 = calibrated.ReferenceSha256, sourceAlphaSha256 = calibrated.SourceAlphaSha256, actualMatch.PsnrGapDb,
                    accepted = true, alphaExact = true, ownedEnumeratedSettings = calibrated.OwnedSettings.Length,
                    excludedAnchorsInCatalog = catalog.Profiles.Sum(row => row.Matches.Count(match => !match.Matched)) },
                method: "Untimed original RGBA8/RGBA16LE raw file to JPEG/VP8 still file, one process and equivalent input/output boundaries. Five fixed owned qualities plus at most three refinements against two fixed native anchors supply settings; selection is closest among sampled settings only. Outputs must reproduce calibration bitstreams, decoded samples, source/target alpha hashes, source/decoder error and maximum 0.25 dB PSNR gap. JPEG transparent input uses the declared RGB background and one rounding of flattening plus depth reduction; source error is measured against that opaque RGB target. VP8 preserves normalized alpha and uses premultiplied source RGB error before composite rounding. Raw decoder agreement remains unweighted maximum 4/RMSE 1.5, including hidden RGB. Managed output equals exact public encoding. Matching error is not perceptual quality or compression effort. Unmatched anchors/closest results remain catalogued without matched-quality timing claims. No search or validation in timed methods. Linux child-memory runs are separate and must preserve output hashes. No local performance measurement.");
        }
        catch { state.Dispose(); throw; }
    }

    internal static (string Path, QualityCalibrationCatalog Catalog) ReadCatalog(JpegBenchmarkCase identity)
    {
        var configured = Environment.GetEnvironmentVariable("LOKAD_IMAGE_QUALITY_CALIBRATION");
        if (string.IsNullOrEmpty(configured)) throw new InvalidOperationException("Set LOKAD_IMAGE_QUALITY_CALIBRATION to the current completed --calibrate-encoder-quality manifest before selecting matched-quality benchmarks.");
        var path = Path.GetFullPath(configured, identity.Root);
        var catalog = System.Text.Json.JsonSerializer.Deserialize<QualityCalibrationCatalog>(File.ReadAllText(path), EncoderQualityCalibration.Json)
            ?? throw new InvalidOperationException("Quality calibration manifest is empty.");
        var benchmark = Path.Combine(identity.Root, "benchmarks", "Lokad.Image.Benchmarks", "bin", "Release", "net10.0", "Lokad.Image.Benchmarks.dll");
        var child = Path.Combine(Path.GetDirectoryName(identity.Probe)!, "Lokad.Image.dll");
        if (catalog.SchemaVersion != 4 || catalog.Project != "Lokad.Image" || !catalog.Complete || catalog.Scope is not ("representative" or "full" or "larger" or "opaque" or "transparent" or "vp8-transparent" or "jpeg-transparent" or "control" or "larger-control" or "varied-control")
            || catalog.MaximumPsnrGapDb != EncoderQualityCalibration.MaximumPsnrGapDb || catalog.Recipe != EncoderQualitySource.Recipe
            || catalog.OwnedQualityPolicy != EncoderQualityCalibration.OwnedQualityPolicy || catalog.MaximumOwnedSettings != EncoderQualityCalibration.MaximumOwnedSettings
            || catalog.Platform != System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier
            || catalog.Runtime != System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription
            || catalog.LibrarySha256 != JpegBenchmarkCase.Hash(File.ReadAllBytes(child))
            || catalog.ProbeSha256 != JpegBenchmarkCase.Hash(File.ReadAllBytes(identity.Probe))
            || catalog.BenchmarkAssemblySha256 != JpegBenchmarkCase.Hash(File.ReadAllBytes(benchmark))
            || !File.Exists(identity.Ffmpeg) || catalog.FfmpegSha256 != JpegBenchmarkCase.Hash(File.ReadAllBytes(identity.Ffmpeg)))
            throw new InvalidOperationException("Quality calibration identity, policy or completion changed; regenerate it with current tools.");
        var expected = EncoderQualityCalibration.Profiles(catalog.Scope);
        if (catalog.Profiles.Length != expected.Length || catalog.Profiles.Select(row => row.Profile).Distinct().Count() != expected.Length
            || expected.Except(catalog.Profiles.Select(row => row.Profile)).Any()) throw new InvalidOperationException("Quality calibration profile coverage is incomplete.");
        foreach (var profile in catalog.Profiles)
        {
            if (profile.SourceAlphaSha256 is not { Length: 64 }) throw new InvalidOperationException("Quality source alpha evidence is missing.");
            EncoderQualityCalibration.ValidateSamplingPolicy(profile);
            var anchors = profile.Profile.Encoder == QualityEncoder.Vp8 ? new[] { 50, 85 } : new[] { 16, 5 };
            if (!profile.Matches.Select(match => match.Native.Setting).SequenceEqual(anchors)) throw new InvalidOperationException("Native quality anchors changed.");
            var scale = profile.Profile.EncodedHasAlpha ? 255 : 1;
            if (profile.OwnedSettings.Concat(profile.Matches.Select(match => match.Native)).Any(sample => !sample.AlphaExact
                || sample.AlphaSha256 is null || sample.SourceError.Scale != scale || sample.DecoderDifference.Scale != 1
                || sample.DecoderDifference.Maximum > 4 || sample.DecoderDifference.RgbRmse > 1.5))
                throw new InvalidOperationException("Quality metric scale, decoder bounds or alpha evidence changed.");
            foreach (var match in profile.Matches)
                if (EncoderQualityCalibration.Match(match.Native, profile.OwnedSettings) != match)
                    throw new InvalidOperationException("Recorded quality selection or exclusions do not reproduce.");
        }
        return (path, catalog);
    }

    [Benchmark(Baseline = true)] public long Ffmpeg() => state!.Run(state.Ffmpeg, nativeArguments, state.NativeOutput);
    [Benchmark] public long Managed() => state!.Run("dotnet", managedArguments, state.ManagedOutput);
    [GlobalCleanup] public void Cleanup() => state?.Dispose();
}
