using System.Globalization;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image.Tooling;

/// <summary>One process streams 32 repeated calibrated rasters to independent still files.</summary>
[MemoryDiagnoser]
public class MatchedQualityEncodeBatch
{
    [ParamsSource(nameof(Cases))] public MatchedQualityWorkload Workload { get; set; } = null!;
    public IEnumerable<MatchedQualityWorkload> Cases => new MatchedQualityEncodeInvocation().Cases;
    public const int FrameCount = 32;
    private JpegBenchmarkCase? state;
    private string[] managedArguments = [], nativeArguments = [];
    private string managedDirectory = "", nativeDirectory = "", extension = "";
    private string managedIndex = "", nativeIndex = "";
    private bool setupComplete;
    private TimeSpan ExecutionTimeout => Workload.Profile.Width >= 1025 ? TimeSpan.FromMinutes(10) : TimeSpan.FromMinutes(1);

    private sealed record FrameReceipt(int Frame, string Name, QualitySample Sample);
    private sealed record BatchReceipt(long OutputBytes, string OutputIndexSha256, int DistinctBitstreamsDecoded, FrameReceipt[] FrameValidation);

    [GlobalSetup]
    public void Setup()
    {
        setupComplete = false;
        state = new JpegBenchmarkCase();
        try
        {
            var profile = Workload.Profile;
            var (path, catalog) = MatchedQualityEncodeInvocation.ReadCatalog(state);
            var calibrated = catalog.Profiles.Single(row => row.Profile == profile);
            var selected = calibrated.Matches.Single(match => match.Native.Setting == Workload.NativeSetting);
            if (!selected.Matched || selected.ClosestOwned.Setting != Workload.OwnedSetting)
                throw new InvalidOperationException("Requested batch row is not a matched calibration pair.");
            var source = EncoderQualitySource.Generate(profile);
            var reference = EncoderQualitySource.Reference(source, profile);
            if (JpegBenchmarkCase.Hash(source) != calibrated.SourceSha256 || JpegBenchmarkCase.Hash(reference) != calibrated.ReferenceSha256
                || EncoderQualitySource.AlphaHash(EncoderQualitySource.Normalize(source, profile.Depth)) != calibrated.SourceAlphaSha256)
                throw new InvalidOperationException("Matched-quality batch source changed since calibration.");
            using (var input = File.Create(state.Input))
                for (var frame = 0; frame < FrameCount; frame++) input.Write(source);
            extension = profile.Encoder == QualityEncoder.Vp8 ? ".webp" : ".jpg";
            managedDirectory = Path.Combine(state.DirectoryPath, "managed-frames");
            nativeDirectory = Path.Combine(state.DirectoryPath, "native-frames");
            Directory.CreateDirectory(managedDirectory); Directory.CreateDirectory(nativeDirectory);
            var format = profile.Encoder switch { QualityEncoder.Jpeg444 => "jpeg444", QualityEncoder.Jpeg422 => "jpeg422",
                QualityEncoder.Jpeg420 => "jpeg420", _ => "webp-lossy" };
            managedArguments = [state.Probe, "encode-sequence", state.Input, managedDirectory,
                JpegBenchmarkCase.Number(profile.Width), JpegBenchmarkCase.Number(profile.Height), JpegBenchmarkCase.Number(profile.Depth),
                format, JpegBenchmarkCase.Number(FrameCount), JpegBenchmarkCase.Number(Workload.OwnedSetting), "4"];
            if (profile.BackgroundRgb.HasValue)
                managedArguments = [.. managedArguments, profile.BackgroundRgb.Value.ToString("x6", CultureInfo.InvariantCulture)];
            nativeArguments = NativeArguments(state, profile, Workload.NativeSetting, nativeDirectory);
            Ffmpeg(); Managed();
            using (var pixels = JpegBenchmarkCase.Buffer(source, profile.Width, profile.Height, profile.Depth))
            using (var output = new MemoryStream())
            {
                EncoderQualityCalibration.Encode(pixels, output, profile, Workload.OwnedSetting);
                if (!output.ToArray().AsSpan().SequenceEqual(File.ReadAllBytes(FramePath(managedDirectory, 0))))
                    throw new InvalidOperationException("Matched-quality batch output differs from the exact public encode workflow.");
            }
            var managed = Validate(managedDirectory, reference, profile, selected.ClosestOwned);
            var native = Validate(nativeDirectory, reference, profile, selected.Native);
            managedIndex = managed.OutputIndexSha256; nativeIndex = native.OutputIndexSha256;
            var nativeMemory = ChildProcessMemory.Measure(state.Ffmpeg, nativeArguments, state.DirectoryPath, timeout: ExecutionTimeout);
            var managedMemory = ChildProcessMemory.Measure("dotnet", managedArguments, state.DirectoryPath, timeout: ExecutionTimeout);
            RequireOutputIndexes();
            state.Report(nameof(MatchedQualityEncodeBatch), new { profile.Width, profile.Height, profile.Depth, profile.Encoder,
                profile.Pattern, profile.Alpha, profile.EncodedHasAlpha, profile.BackgroundRgb, profile.SourceMetric,
                nativeSetting = Workload.NativeSetting, ownedQuality = Workload.OwnedSetting, frames = FrameCount,
                inputPattern = "repeat-calibrated-raster", codecThreads = 1, filterThreads = 1,
                operationTimeoutSeconds = (int)ExecutionTimeout.TotalSeconds,
                webpEffort = profile.Encoder == QualityEncoder.Vp8 ? (int?)4 : null, maximumPsnrGapDb = EncoderQualityCalibration.MaximumPsnrGapDb },
                managedArguments, nativeArguments, managed, native,
                stages: new { calibrationSha256 = JpegBenchmarkCase.Hash(File.ReadAllBytes(path)), calibrationScope = catalog.Scope,
                    sourceSha256 = calibrated.SourceSha256, referenceSha256 = calibrated.ReferenceSha256, sourceAlphaSha256 = calibrated.SourceAlphaSha256,
                    selected.PsnrGapDb, accepted = true, alphaExact = true, ownedEnumeratedSettings = calibrated.OwnedSettings.Length,
                    excludedAnchorsInCatalog = catalog.Profiles.Sum(row => row.Matches.Count(match => !match.Matched)) },
                inputs: new { rawFrames = FrameCount, frameBytes = source.Length, rawFileBytes = new FileInfo(state.Input).Length,
                    frames = Enumerable.Range(0, FrameCount).Select(frame => new { frame, rawOffset = (long)frame * source.Length,
                        rawBytes = source.Length, rawSha256 = calibrated.SourceSha256, referenceSha256 = calibrated.ReferenceSha256 }) },
                childMemory: new { native = nativeMemory, managed = managedMemory },
                method: "Untimed matched-quality validation of 32 repeated calibrated RGBA8/RGBA16LE rasters streamed from one raw file. Each subject launches one process, reads every frame and writes 32 independent numbered JPEG/VP8 still files. Every frame must reproduce the single-frame calibration bitstream, decoded samples, source/decoder metrics, exact target alpha and fixed maximum 0.25 dB PSNR gap. Each distinct bitstream is independently decoded once per subject; repeated frames are compared byte-for-byte with that validated bitstream. Managed output equals the exact public encode workflow. JPEG includes declared background flattening and depth reduction in one rounding; VP8 preserves normalized alpha. Input offsets, all ordered output files and hashes are recorded. Codec context reuse, startup/JIT, transport and file costs remain included. This repeated-input batch does not establish varied-frame or larger-image coverage, perceptual equivalence or isolated codec throughput. Child-memory runs are separate, must preserve every output, and remain unavailable on Windows. Timed methods contain no search, validation or counter wrapper; cleanup rechecks the final outputs. No local performance claims.");
            setupComplete = true;
        }
        catch { state.Dispose(); throw; }
    }

    internal static string[] NativeArguments(JpegBenchmarkCase state, EncoderQualityProfile profile, int setting, string directory)
    {
        var arguments = EncoderQualityCalibration.NativeArguments(state, profile, setting).ToList();
        arguments[arguments.IndexOf("-frames:v") + 1] = JpegBenchmarkCase.Number(FrameCount);
        arguments[arguments.LastIndexOf("-f") + 1] = "image2";
        arguments[^1] = Path.Combine(directory, "frame-%04d" + (profile.Encoder == QualityEncoder.Vp8 ? ".webp" : ".jpg"));
        arguments.InsertRange(arguments.Count - 1, ["-fps_mode", "passthrough", "-start_number", "0"]);
        return arguments.ToArray();
    }

    private BatchReceipt Validate(string directory, byte[] reference, EncoderQualityProfile profile, QualitySample expected)
    {
        RequireFileCount(directory);
        var distinct = new Dictionary<string, (byte[] Bytes, QualitySample Sample)>();
        var frames = new FrameReceipt[FrameCount]; long total = 0;
        for (var frame = 0; frame < FrameCount; frame++)
        {
            var path = FramePath(directory, frame); var bytes = File.ReadAllBytes(path);
            var hash = JpegBenchmarkCase.Hash(bytes);
            if (!distinct.TryGetValue(hash, out var validated))
            {
                validated = (bytes, EncoderQualityCalibration.Validate(bytes, reference, profile, expected.Setting, state!.Ffmpeg));
                distinct.Add(hash, validated);
            }
            else if (!bytes.AsSpan().SequenceEqual(validated.Bytes)) throw new InvalidOperationException("Batch bitstream hash collision.");
            if (validated.Sample != expected) throw new InvalidOperationException("Batch frame differs from the single-frame calibration; separate batch calibration is required.");
            total = checked(total + bytes.Length);
            frames[frame] = new(frame, Path.GetFileName(path), validated.Sample);
        }
        return new(total, OutputIndex(directory), distinct.Count, frames);
    }

    private string OutputIndex(string directory)
    {
        RequireFileCount(directory);
        var frames = Enumerable.Range(0, FrameCount).Select(frame =>
        {
            var path = FramePath(directory, frame); var bytes = File.ReadAllBytes(path);
            if (bytes.Length == 0) throw new InvalidOperationException("Matched-quality batch contains an empty still file.");
            return new { frame, name = Path.GetFileName(path), encodedBytes = bytes.Length, encodedSha256 = JpegBenchmarkCase.Hash(bytes) };
        });
        return JpegBenchmarkCase.Hash(JsonSerializer.SerializeToUtf8Bytes(frames));
    }

    private static void RequireFileCount(string directory)
    {
        if (Directory.GetFiles(directory).Length != FrameCount) throw new InvalidOperationException("Matched-quality batch frame count differs.");
    }

    private string FramePath(string directory, int frame) => Path.Combine(directory, "frame-" + frame.ToString("D4", CultureInfo.InvariantCulture) + extension);
    private void RequireOutputIndexes()
    {
        if (OutputIndex(managedDirectory) != managedIndex || OutputIndex(nativeDirectory) != nativeIndex)
            throw new InvalidOperationException("Matched-quality batch execution changed a validated still file.");
    }

    private long Run(string executable, string[] arguments, string directory)
    {
        OracleProcess.Run(executable, arguments, timeout: ExecutionTimeout); RequireFileCount(directory);
        long total = 0;
        for (var frame = 0; frame < FrameCount; frame++) total = checked(total + new FileInfo(FramePath(directory, frame)).Length);
        return total;
    }

    [Benchmark(Baseline = true)] public long Ffmpeg() => Run(state!.Ffmpeg, nativeArguments, nativeDirectory);
    [Benchmark] public long Managed() => Run("dotnet", managedArguments, managedDirectory);
    [GlobalCleanup] public void Cleanup()
    {
        try { if (setupComplete) RequireOutputIndexes(); }
        finally { state?.Dispose(); setupComplete = false; }
    }
}
