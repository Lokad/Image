using System.Globalization;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image.Tooling;

public sealed record MatchedQualityContentWorkload(EncoderQualityProfile[] Profiles, int NativeSetting, int[] OwnedSettings)
{
    public override string ToString() => $"{Profiles[0]}-patterns{Profiles.Length}-native{NativeSetting}-owned{string.Join('_', OwnedSettings)}";
}

/// <summary>One process encodes an ordered batch of calibrated content patterns.</summary>
[MemoryDiagnoser]
public class MatchedQualityContentBatch
{
    [ParamsSource(nameof(Cases))] public MatchedQualityContentWorkload Workload { get; set; } = null!;
    public const int FrameCount = 32;
    private JpegBenchmarkCase? state;
    private string[] managedArguments = [], nativeArguments = [];
    private string managedDirectory = "", nativeDirectory = "", extension = "";
    private string managedIndex = "", nativeIndex = "";
    private bool setupComplete;
    private TimeSpan ExecutionTimeout => Workload.Profiles[0].Width >= 1025 ? TimeSpan.FromMinutes(10) : TimeSpan.FromMinutes(1);

    internal sealed record GroupReceipt(EncoderQualityProfile[] Profiles, int NativeSetting, bool Accepted, QualityMatch[] Matches);
    private sealed record FrameReceipt(int Frame, string Name, QualityPattern Pattern, QualitySample Sample);
    private sealed record BatchReceipt(long OutputBytes, string OutputIndexSha256, int DistinctBitstreamsDecoded, FrameReceipt[] FrameValidation);

    internal static GroupReceipt[] Groups(QualityCalibrationCatalog catalog)
        => catalog.Profiles.GroupBy(row => (row.Profile.Width, row.Profile.Depth, row.Profile.Encoder, row.Profile.Alpha, row.Profile.BackgroundRgb))
            .SelectMany(group => (group.Key.Encoder == QualityEncoder.Vp8 ? new[] { 50, 85 } : new[] { 16, 5 }).Select(setting =>
            {
                var rows = group.OrderBy(row => row.Profile.Pattern).ToArray();
                var matches = rows.Select(row => row.Matches.Single(match => match.Native.Setting == setting)).ToArray();
                return new GroupReceipt(rows.Select(row => row.Profile).ToArray(), setting, matches.All(match => match.Matched), matches);
            })).ToArray();

    public IEnumerable<MatchedQualityContentWorkload> Cases
    {
        get
        {
            using var identity = new JpegBenchmarkCase();
            var (_, catalog) = MatchedQualityEncodeInvocation.ReadCatalog(identity);
            return Groups(catalog).Where(group => group.Accepted).Select(group => new MatchedQualityContentWorkload(group.Profiles,
                group.NativeSetting, group.Matches.Select(match => match.ClosestOwned.Setting).ToArray())).ToArray();
        }
    }

    [GlobalSetup]
    public void Setup()
    {
        setupComplete = false;
        state = new JpegBenchmarkCase();
        try
        {
            var (path, catalog) = MatchedQualityEncodeInvocation.ReadCatalog(state);
            var groups = Groups(catalog);
            var group = groups.Single(row => row.NativeSetting == Workload.NativeSetting && row.Profiles.SequenceEqual(Workload.Profiles));
            if (!group.Accepted || !group.Matches.Select(match => match.ClosestOwned.Setting).SequenceEqual(Workload.OwnedSettings))
                throw new InvalidOperationException("Content batch requires a matched pair for every declared pattern; exclusions must remain excluded.");
            var profile = group.Profiles[0];
            var calibrated = group.Profiles.Select(item => catalog.Profiles.Single(row => row.Profile == item)).ToArray();
            var sources = group.Profiles.Select(EncoderQualitySource.Generate).ToArray();
            var references = sources.Select((source, index) => EncoderQualitySource.Reference(source, group.Profiles[index])).ToArray();
            var publicOutputs = new byte[sources.Length][];
            for (var pattern = 0; pattern < sources.Length; pattern++)
            {
                if (JpegBenchmarkCase.Hash(sources[pattern]) != calibrated[pattern].SourceSha256
                    || JpegBenchmarkCase.Hash(references[pattern]) != calibrated[pattern].ReferenceSha256
                    || EncoderQualitySource.AlphaHash(EncoderQualitySource.Normalize(sources[pattern], profile.Depth)) != calibrated[pattern].SourceAlphaSha256)
                    throw new InvalidOperationException("Content batch source/reference changed since calibration.");
                using var pixels = JpegBenchmarkCase.Buffer(sources[pattern], profile.Width, profile.Height, profile.Depth);
                using var output = new MemoryStream();
                EncoderQualityCalibration.Encode(pixels, output, group.Profiles[pattern], Workload.OwnedSettings[pattern]);
                publicOutputs[pattern] = output.ToArray();
                if (JpegBenchmarkCase.Hash(publicOutputs[pattern]) != group.Matches[pattern].ClosestOwned.EncodedSha256)
                    throw new InvalidOperationException("Content batch public encoding changed since calibration.");
            }
            if (calibrated.Select(row => row.SourceSha256).Distinct().Count() != sources.Length)
                throw new InvalidOperationException("Different content patterns must have distinct actual input samples.");
            using (var input = File.Create(state.Input))
                for (var frame = 0; frame < FrameCount; frame++) input.Write(sources[frame % sources.Length]);
            var inputReceipts = Enumerable.Range(0, FrameCount).Select(frame =>
            {
                var pattern = frame % sources.Length; var row = calibrated[pattern];
                return new { frame, pattern = row.Profile.Pattern, rawOffset = (long)frame * row.SourceBytes,
                    rawBytes = row.SourceBytes, rawSha256 = row.SourceSha256, referenceSha256 = row.ReferenceSha256,
                    sourceAlphaSha256 = row.SourceAlphaSha256, ownedQuality = Workload.OwnedSettings[pattern] };
            }).ToArray();
            Array.Clear(sources);
            extension = profile.Encoder == QualityEncoder.Vp8 ? ".webp" : ".jpg";
            managedDirectory = Path.Combine(state.DirectoryPath, "managed-frames");
            nativeDirectory = Path.Combine(state.DirectoryPath, "native-frames");
            Directory.CreateDirectory(managedDirectory); Directory.CreateDirectory(nativeDirectory);
            var format = profile.Encoder switch { QualityEncoder.Jpeg444 => "jpeg444", QualityEncoder.Jpeg422 => "jpeg422", QualityEncoder.Jpeg420 => "jpeg420", _ => "webp-lossy" };
            var qualities = string.Join(',', Enumerable.Range(0, FrameCount).Select(frame => JpegBenchmarkCase.Number(Workload.OwnedSettings[frame % Workload.OwnedSettings.Length])));
            managedArguments = [state.Probe, "encode-quality-sequence", state.Input, managedDirectory,
                JpegBenchmarkCase.Number(profile.Width), JpegBenchmarkCase.Number(profile.Height), JpegBenchmarkCase.Number(profile.Depth),
                format, JpegBenchmarkCase.Number(FrameCount), qualities, "4"];
            if (profile.BackgroundRgb.HasValue)
                managedArguments = [.. managedArguments, profile.BackgroundRgb.Value.ToString("x6", CultureInfo.InvariantCulture)];
            nativeArguments = MatchedQualityEncodeBatch.NativeArguments(state, profile, Workload.NativeSetting, nativeDirectory);
            Ffmpeg(); Managed();
            var managed = Validate(managedDirectory, references, group, true, publicOutputs);
            var native = Validate(nativeDirectory, references, group, false, publicOutputs);
            managedIndex = managed.OutputIndexSha256; nativeIndex = native.OutputIndexSha256;
            var nativeMemory = ChildProcessMemory.Measure(state.Ffmpeg, nativeArguments, state.DirectoryPath, timeout: ExecutionTimeout);
            var managedMemory = ChildProcessMemory.Measure("dotnet", managedArguments, state.DirectoryPath, timeout: ExecutionTimeout);
            RequireOutputIndexes();
            state.Report(nameof(MatchedQualityContentBatch), new { profile.Width, profile.Height, profile.Depth, profile.Encoder,
                profile.Alpha, profile.EncodedHasAlpha, profile.BackgroundRgb, profile.SourceMetric, patterns = group.Profiles.Select(item => item.Pattern),
                distinctSourcePatterns = group.Profiles.Length, nativeSetting = Workload.NativeSetting, ownedQualities = Workload.OwnedSettings,
                frames = FrameCount, inputPattern = "round-robin-calibrated-patterns", codecThreads = 1, filterThreads = 1,
                operationTimeoutSeconds = (int)ExecutionTimeout.TotalSeconds, cleanupOutputHashCheck = true,
                webpEffort = profile.Encoder == QualityEncoder.Vp8 ? (int?)4 : null, maximumPsnrGapDb = EncoderQualityCalibration.MaximumPsnrGapDb },
                managedArguments, nativeArguments, managed, native,
                stages: new { calibrationSha256 = JpegBenchmarkCase.HashFile(path), calibrationScope = catalog.Scope,
                    accepted = true, alphaExact = true, matchedPatterns = group.Matches,
                    groupCoverage = groups.Select(item => new { item.Profiles, item.NativeSetting, item.Accepted,
                        excludedPatterns = item.Matches.Select((match, index) => new { pattern = item.Profiles[index].Pattern,
                            match.Matched, match.PsnrGapDb, match.ExclusionReason }).Where(match => !match.Matched) }),
                    excludedAnchorsInCatalog = catalog.Profiles.Sum(row => row.Matches.Count(match => !match.Matched)) },
                inputs: new { rawFrames = FrameCount, rawFileBytes = new FileInfo(state.Input).Length,
                    rawFileSha256 = JpegBenchmarkCase.HashFile(state.Input), frames = inputReceipts },
                childMemory: new { native = nativeMemory, managed = managedMemory },
                method: "Untimed validation of 32 RGBA8/RGBA16LE frames in round-robin order over all content patterns in this calibration group. One native anchor setting remains fixed; owned quality is individually calibrated for each pattern and supplied as a fixed per-frame list, with no timed search. Every pattern must satisfy the unchanged 0.25 dB bound; any excluded pattern excludes the entire group. Every ordered frame must reproduce its single-frame native/owned bitstream, decoder/source metrics and exact alpha; each distinct pattern/bitstream is decoded once per subject, with repeats checked byte-for-byte. Managed output equals the direct public workflow. Full/varied scopes contain six distinct original patterns; a one-pattern control is execution evidence only, not varied-content coverage. JPEG flattening/depth reduction rounds once; VP8 alpha is once-normalized. All accepted/excluded groups remain recorded. Raw input/output transport, per-frame option selection, process startup/JIT, codec-context reuse and files remain included. Child counters are separate untimed runs; cleanup rechecks actual final outputs. Execution timeouts are not performance budgets. No local performance conclusions.");
            Array.Clear(references); Array.Clear(publicOutputs);
            setupComplete = true;
        }
        catch { state.Dispose(); throw; }
    }

    private BatchReceipt Validate(string directory, byte[][] references, GroupReceipt group, bool managed, byte[][] publicOutputs)
    {
        RequireFileCount(directory);
        var distinct = new Dictionary<(int Pattern, string Hash), (byte[] Bytes, QualitySample Sample)>();
        var frames = new FrameReceipt[FrameCount]; long total = 0;
        for (var frame = 0; frame < FrameCount; frame++)
        {
            var pattern = frame % group.Profiles.Length; var profile = group.Profiles[pattern];
            var expected = managed ? group.Matches[pattern].ClosestOwned : group.Matches[pattern].Native;
            var path = FramePath(directory, frame); var bytes = File.ReadAllBytes(path);
            var key = (pattern, JpegBenchmarkCase.Hash(bytes));
            if (!distinct.TryGetValue(key, out var validated))
            {
                validated = (bytes, EncoderQualityCalibration.Validate(bytes, references[pattern], profile, expected.Setting, state!.Ffmpeg));
                distinct.Add(key, validated);
            }
            else if (!bytes.AsSpan().SequenceEqual(validated.Bytes)) throw new InvalidOperationException("Content batch bitstream hash collision.");
            if (validated.Sample != expected) throw new InvalidOperationException("Content frame differs from its single-frame calibration; separate batch calibration is required.");
            if (managed && !bytes.AsSpan().SequenceEqual(publicOutputs[pattern])) throw new InvalidOperationException("Content batch differs from the public encode workflow.");
            total = checked(total + bytes.Length); frames[frame] = new(frame, Path.GetFileName(path), profile.Pattern, validated.Sample);
        }
        return new(total, OutputIndex(directory), distinct.Count, frames);
    }

    private string OutputIndex(string directory)
    {
        RequireFileCount(directory);
        var frames = Enumerable.Range(0, FrameCount).Select(frame =>
        {
            var path = FramePath(directory, frame); var length = new FileInfo(path).Length;
            if (length == 0) throw new InvalidOperationException("Content batch contains an empty still file.");
            return new { frame, name = Path.GetFileName(path), encodedBytes = length, encodedSha256 = JpegBenchmarkCase.HashFile(path) };
        });
        return JpegBenchmarkCase.Hash(JsonSerializer.SerializeToUtf8Bytes(frames));
    }
    private static void RequireFileCount(string directory)
    {
        if (Directory.GetFiles(directory).Length != FrameCount) throw new InvalidOperationException("Content batch frame count differs.");
    }
    private string FramePath(string directory, int frame) => Path.Combine(directory, "frame-" + frame.ToString("D4", CultureInfo.InvariantCulture) + extension);
    private void RequireOutputIndexes()
    {
        if (OutputIndex(managedDirectory) != managedIndex || OutputIndex(nativeDirectory) != nativeIndex)
            throw new InvalidOperationException("Content batch execution changed a validated still file.");
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
    internal static void ValidateCleanupControls()
    {
        var selected = new MatchedQualityContentBatch().Cases.First();
        var controls = 0;
        foreach (var managed in new[] { false, true })
        foreach (var extra in new[] { false, true })
        {
            var workload = new MatchedQualityContentBatch { Workload = selected };
            string? directory = null;
            try
            {
                workload.Setup(); directory = workload.state!.DirectoryPath;
                var output = managed ? workload.managedDirectory : workload.nativeDirectory;
                if (extra) File.WriteAllText(Path.Combine(output, "unexpected"), "owned cleanup control");
                else
                {
                    using var file = File.Open(workload.FramePath(output, 0), FileMode.Open, FileAccess.ReadWrite);
                    var first = file.ReadByte(); file.Position = 0; file.WriteByte((byte)(first ^ 1));
                }
                var rejected = false;
                try { workload.Cleanup(); }
                catch (InvalidOperationException) { rejected = true; }
                if (!rejected || Directory.Exists(directory)) throw new InvalidOperationException("Content batch cleanup failed to reject changed output and remove its owned directory.");
                controls++;
            }
            finally { workload.Cleanup(); }
        }
        Console.WriteLine($"All {controls} content batch cleanup controls rejected changed outputs and removed owned temporary directories.");
    }
    [GlobalCleanup] public void Cleanup()
    {
        try { if (setupComplete) RequireOutputIndexes(); }
        finally { state?.Dispose(); setupComplete = false; }
    }
}
