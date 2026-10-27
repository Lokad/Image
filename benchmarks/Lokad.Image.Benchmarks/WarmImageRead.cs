using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

public enum ImageReadCase
{
    ProbeDimensionsPng, ProbeDimensionsJpeg, ProbeDimensionsWebp, ProbeDimensionsJpegAsync,
    ValidatePng8, ValidatePng16, ValidateIndexedAdam7, ValidateJpegBaseline420,
    ValidateJpegProgressive420, ValidateVp8lAlpha, ValidateVp8Alpha, ValidatePng16Async
}

/// <summary>Named prefix-probing and complete-coding boundaries; no pixel normalization or output raster.</summary>
[MemoryDiagnoser]
public class WarmImageRead
{
    public static IEnumerable<ImageReadCase> Cases => SelectedCases();
    [ParamsSource(nameof(Cases))] public ImageReadCase Case { get; set; }
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private byte[] encoded = [], descriptor = [];
    private NativeWarmSample[] before = [];
    private ImageInfo expected = null!;
    private JsonElement proof, sourceIdentity;
    private int cpu, count;
    private bool smoke, ready;
    private bool Probe => Case <= ImageReadCase.ProbeDimensionsJpegAsync;
    private bool Async => Case is ImageReadCase.ProbeDimensionsJpegAsync or ImageReadCase.ValidatePng16Async;
    private ReadReferenceMode Mode => Probe ? Async ? ReadReferenceMode.ProbeNonseekable : ReadReferenceMode.ProbeSeekable
        : Async ? ReadReferenceMode.ValidateNonseekable : ReadReferenceMode.ValidateSpan;
    private static string Key(ImageReadCase value) => value switch
    {
        ImageReadCase.ProbeDimensionsPng or ImageReadCase.ValidatePng8 => "png8",
        ImageReadCase.ProbeDimensionsJpeg or ImageReadCase.ProbeDimensionsJpegAsync or ImageReadCase.ValidateJpegBaseline420 => "jpeg420",
        ImageReadCase.ProbeDimensionsWebp or ImageReadCase.ValidateVp8lAlpha => "vp8l",
        ImageReadCase.ValidatePng16 or ImageReadCase.ValidatePng16Async => "png16",
        ImageReadCase.ValidateIndexedAdam7 => "indexed", ImageReadCase.ValidateJpegProgressive420 => "progressive420", _ => "vp8"
    };
    internal static ImageReadCase[] SelectedCases()
    {
        var values = (Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CASES") ?? "ProbeDimensionsPng").Split(',')
            .Select(s => Enum.TryParse<ImageReadCase>(s, out var value) && Enum.IsDefined(value) ? value : throw new ArgumentException("Choose named read controls.")).ToArray();
        if (values.Length is < 1 or > 3 || values.Distinct().Count() != values.Length) throw new ArgumentException("Choose one to three distinct read controls.");
        return values;
    }

    internal static byte[] Serialize(ImageInfo info, bool probe)
    {
        int[] fields = [(int)info.Format, info.Width, info.Height, probe ? 0 : info.BitDepth,
            probe ? -1 : info.HasAlpha ? 1 : 0, probe ? 0 : (int)info.Orientation,
            probe ? 0 : (int)info.SourceColor, probe ? 0 : (int)info.SourceAlpha, probe ? 0 : info.SourceComponentCount];
        var bytes = new byte[36];
        for (var i = 0; i < fields.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), fields[i]);
        return bytes;
    }
    private static byte[] Serialize(object result)
    {
        if (result is ImageInfo info) return Serialize(info, false);
        var probe = (ImageProbeResult)result;
        if ((probe.EstablishedFacts & ImageProbeFacts.Dimensions) == 0 || probe.Width is null || probe.Height is null)
            throw new InvalidOperationException("Requested dimensions are unestablished.");
        return Serialize(new(probe.Format, probe.Width.Value, probe.Height.Value, 0, false, ImageOrientation.Normal), true);
    }

    private sealed class Input(byte[] bytes, bool seekable, bool asynchronous) : Stream
    {
        private int position;
        public long BytesRead;
        public int EofReads;
        public bool Closed;
        public override bool CanRead => !Closed;
        public override bool CanSeek => seekable;
        public override bool CanWrite => false;
        public override long Length => seekable ? bytes.Length : throw new NotSupportedException();
        public override long Position { get => seekable ? position : throw new NotSupportedException(); set => Seek(value, SeekOrigin.Begin); }
        private int Copy(Span<byte> target)
        {
            ObjectDisposedException.ThrowIf(Closed, this);
            var n = Math.Min(target.Length, bytes.Length - position);
            bytes.AsSpan(position, n).CopyTo(target); position += n; BytesRead += n; if (n == 0) EofReads++; return n;
        }
        public override int Read(Span<byte> target)
        { if (asynchronous) throw new InvalidOperationException("Synchronous acquisition on async boundary."); return Copy(target); }
        public override int Read(byte[] buffer, int offset, int size) => Read(buffer.AsSpan(offset, size));
        public override ValueTask<int> ReadAsync(Memory<byte> target, CancellationToken cancellationToken = default)
        {
            if (!asynchronous) throw new InvalidOperationException("Async acquisition on sync boundary.");
            cancellationToken.ThrowIfCancellationRequested(); return ValueTask.FromResult(Copy(target.Span));
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!seekable) throw new NotSupportedException();
            var next = (origin == SeekOrigin.Begin ? 0 : origin == SeekOrigin.Current ? position : bytes.Length) + offset;
            if (next < 0 || next > bytes.Length) throw new IOException("Seek outside source."); position = (int)next; return next;
        }
        protected override void Dispose(bool disposing) { Closed = true; base.Dispose(disposing); }
        public override void Flush() => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int size) => throw new NotSupportedException();
    }
    private object Invoke(Input? input = null)
    {
        if (!Probe && !Async) return ImageCodec.ValidatePixels(encoded);
        if (input is null) throw new ArgumentNullException(nameof(input));
        return Probe ? Async ? ImageCodec.ProbeAsync(input).GetAwaiter().GetResult() : ImageCodec.Probe(input)
            : ImageCodec.ValidatePixelsAsync(input).GetAwaiter().GetResult();
    }
    [Benchmark]
    public object Managed()
    {
        if (!ready) throw new InvalidOperationException("Read boundary is not qualified.");
        if (!Probe && !Async) return ImageCodec.ValidatePixels(encoded);
        using var input = new Input(encoded, !Async, Async);
        return Invoke(input);
    }
    private void ValidateManaged()
    {
        var hash = JpegBenchmarkCase.Hash(encoded);
        using var input = new Input(encoded, !Async, Async);
        var first = Invoke(Probe || Async ? input : null);
        if (!Serialize(first).AsSpan().SequenceEqual(descriptor) || input.Closed) throw new InvalidOperationException("Managed read descriptor/ownership differs.");
        if (Probe)
        {
            var value = (ImageProbeResult)first;
            if (value.BytesRead != input.BytesRead || value.BytesRead >= encoded.Length || value.BytesRead > 1024 * 1024 || input.EofReads != 0)
                throw new InvalidOperationException("Prefix read volume differs.");
        }
        else if (Async && (input.BytesRead != encoded.Length || input.EofReads == 0)) throw new InvalidOperationException("Complete async input delivery differs.");
        using var secondInput = new Input(encoded, !Async, Async);
        var second = Invoke(Probe || Async ? secondInput : null);
        if (ReferenceEquals(first, second) || !Serialize(second).AsSpan().SequenceEqual(descriptor)) throw new InvalidOperationException("Fresh read results alias/differ.");
        if (JpegBenchmarkCase.Hash(encoded) != hash) throw new InvalidOperationException("Read operation changed input.");
        var saved = encoded; encoded = saved.ToArray();
        using var heldInput = new Input(encoded, !Async, Async);
        var held = Invoke(Probe || Async ? heldInput : null); Array.Clear(encoded); encoded = saved;
        if (!Serialize(held).AsSpan().SequenceEqual(descriptor)) throw new InvalidOperationException("Surviving result aliases input.");
    }

    [GlobalSetup]
    public void Setup()
    {
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("Use the dedicated Linux VM.");
        state = new JpegBenchmarkCase();
        try
        {
            cpu = int.Parse(Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CPU") ?? throw new InvalidOperationException("Missing CPU."));
            smoke = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "smoke";
            count = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "confirm" ? 12 : 3;
            if (Process.GetCurrentProcess().ProcessorAffinity.ToInt64() != 1L << cpu || Environment.ProcessorCount != 1 || GCSettings.IsServerGC
                || Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") != "0"
                || !GC.GetConfigurationVariables().TryGetValue("ConcurrentGC", out var gc) || Convert.ToBoolean(gc))
                throw new InvalidOperationException("Read worker requires pinned single-CPU workstation GC without tiering/background GC.");
            var directory = Path.Combine(state.Root, "artifacts/warm-reference/read-boundaries");
            using (var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(directory, "proof.json")))) proof = document.RootElement.Clone();
            if (proof.GetProperty("timing").GetBoolean() || !proof.GetProperty("allDescriptorsExact").GetBoolean()
                || !proof.GetProperty("prefixAndCodingScopesVerified").GetBoolean()
                || proof.GetProperty("librarySha256").GetString() != JpegBenchmarkCase.HashFile(typeof(ImageCodec).Assembly.Location))
                throw new InvalidOperationException("Read preparation differs.");
            var control = proof.GetProperty("controls").GetProperty(Case.ToString());
            encoded = File.ReadAllBytes(Path.Combine(directory, Key(Case) + ".bin"));
            if (JpegBenchmarkCase.Hash(encoded) != control.GetProperty("inputSha256").GetString()) throw new InvalidOperationException("Frozen read input differs.");
            expected = control.GetProperty("expected").Deserialize<ImageInfo>()!; descriptor = Serialize(expected, Probe);
            using (var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "tmp/benchmark-source.json"))))
                sourceIdentity = JsonSerializer.SerializeToElement(new { sourceSha256 = document.RootElement.GetProperty("sourceSha256").GetString(), sourceRevision = document.RootElement.GetProperty("sourceRevision").GetString() });
            ValidateManaged(); File.WriteAllBytes(state.Input, encoded);
            native = new NativeWarmReference(state, cpu, expected, Mode);
            if (proof.GetProperty("nativeWorkerSha256").GetString() != native.BuildIdentity.GetProperty("workerSha256").GetString()) throw new InvalidOperationException("Native read worker changed.");
            RequireNative(); before = native.Measure(smoke, sampleCount: count); ready = true;
        }
        catch { native?.Dispose(); state?.Dispose(); throw; }
    }
    private void RequireNative()
    {
        if (!File.ReadAllBytes(state.NativeOutput).AsSpan().SequenceEqual(descriptor)) throw new InvalidOperationException("Native read descriptor differs.");
    }
    [GlobalCleanup]
    public void Cleanup()
    {
        if (!ready) { native?.Dispose(); state?.Dispose(); return; }
        try
        {
            ValidateManaged(); var after = native.Measure(smoke, before[0].Operations, count); native.Check(); RequireNative(); native.Stop();
            var runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID") ?? throw new InvalidOperationException("Missing run ID.");
            var path = Path.Combine(state.Root, "artifacts/benchmark-validation", $"WarmImageRead-{runId}-{Case}.json"); Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, runId, caseId = Case.ToString(), smoke, depth = expected.BitDepth, cpu,
                sourceIdentity, descriptorExact = true, sourceUnchanged = true, independentOwnedOutput = true,
                prefixAndCodingScopesVerified = true, inputSha256 = JpegBenchmarkCase.Hash(encoded), inputBytes = encoded.Length,
                descriptorSha256 = JpegBenchmarkCase.Hash(descriptor), managedOutputBytes = 36, nativeOutputBytes = 36,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageCodec).Assembly.Location),
                benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmImageRead).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after, nativeExitCode = 0,
                boundary = Probe ? "Fresh read-only source to independently owned requested dimensions; bounded prefix/header acquisition, source/scratch/descriptor lifetime included. FFmpeg AVIO and owned descriptor APIs with original labeled header adapter; no full-container, CRC or pixel validation. No whole-input buffering, cached result, IPC or real I/O. Async uses immediate resident callbacks. One CPU."
                    : "Same resident encoded bytes to independent coded source metadata and complete pixel-coding validation. Managed required decoder storage, native libavcodec natural frame and original source-metadata adapter, all cleanup included. No RGBA normalization, caller pixel output/copy, reorientation, cached descriptor, IPC or real I/O. Async includes fresh nonseekable source, actual acquisition/growth/EOF and release. Single-CPU codec state reused only as private scratch; every packet is parsed/decoded."
            }, EncoderQualityCalibration.Json) + "\n");
        }
        finally { native.Dispose(); state.Dispose(); ready = false; }
    }

    internal static void Prepare()
    {
        using var state = new JpegBenchmarkCase();
        var directory = Path.Combine(state.Root, "artifacts/warm-reference/read-boundaries"); Directory.CreateDirectory(directory);
        var profiles = new Dictionary<string, MetadataProfile> { ["png8"] = MetadataProfile.Png8, ["png16"] = MetadataProfile.Png16,
            ["jpeg420"] = MetadataProfile.JpegBaseline420, ["progressive420"] = MetadataProfile.JpegProgressive420,
            ["vp8l"] = MetadataProfile.WebpLosslessAlpha, ["vp8"] = MetadataProfile.WebpLossyAlpha };
        foreach (var (key, profile) in profiles) Store(key, OriginalExif.Create(profile, 513, 259).Encoded);
        Store("indexed", AuthoredPng.Build(new("c3-d4-adam7-all-filters", 3, 4, 19, 17, true)));
        var controls = new Dictionary<string, object>(); string? nativeHash = null; var prefixControls = 0;
        foreach (var value in Enum.GetValues<ImageReadCase>())
        {
            var test = new WarmImageRead { Case = value, encoded = File.ReadAllBytes(Path.Combine(directory, Key(value) + ".bin")) };
            test.expected = ImageCodec.Identify(test.encoded); test.descriptor = Serialize(test.expected, test.Probe); test.ValidateManaged();
            using var nativeCase = new JpegBenchmarkCase(); File.WriteAllBytes(nativeCase.Input, test.encoded);
            using var reference = new NativeWarmReference(nativeCase, 0, test.expected, test.Mode);
            if (!File.ReadAllBytes(nativeCase.NativeOutput).AsSpan().SequenceEqual(test.descriptor)) throw new InvalidOperationException("Prepared native read output differs: " + value);
            reference.Check(); reference.Stop(); nativeHash = reference.BuildIdentity.GetProperty("workerSha256").GetString();
            if (test.Probe)
            {
                using var prefixInput = new Input(test.encoded, !test.Async, test.Async);
                var measuredPrefix = (ImageProbeResult)test.Invoke(prefixInput);
                using var prefixCase = new JpegBenchmarkCase();
                File.WriteAllBytes(prefixCase.Input, test.encoded.AsSpan(0, checked((int)measuredPrefix.BytesTraversed)).ToArray());
                using var prefixReference = new NativeWarmReference(prefixCase, 0, test.expected, test.Mode);
                if (!File.ReadAllBytes(prefixCase.NativeOutput).AsSpan().SequenceEqual(test.descriptor)
                    || prefixReference.Ready.GetProperty("readBytes").GetInt64() != reference.Ready.GetProperty("readBytes").GetInt64())
                    throw new InvalidOperationException("Native prefix probe depends on pixel tail: " + value);
                prefixReference.Stop(); prefixControls++;
            }
            controls[value.ToString()] = new { inputKey = Key(value), inputSha256 = JpegBenchmarkCase.Hash(test.encoded), inputBytes = test.encoded.Length, expected = test.expected, nativeReady = reference.Ready };
        }
        // A complete valid container with invalid Deflate is valid for dimensions,
        // and must fail both pixel validators. This is a scope gate, not timed work.
        var invalid = AuthoredPng.Wrap(new("invalid-coding", 6, 8, 19, 17, false), [0xA7]);
        try { ImageCodec.ValidatePixels(invalid); throw new InvalidOperationException("Managed accepted invalid coding."); } catch (ImageDataException) { }
        using (var nativeCase = new JpegBenchmarkCase())
        {
            File.WriteAllBytes(nativeCase.Input, invalid);
            var rejected = false;
            try { using var unexpected = new NativeWarmReference(nativeCase, 0, ImageCodec.Identify(invalid), ReadReferenceMode.ValidateSpan); }
            catch (InvalidOperationException exception) when (exception.Message.Contains("Submit complete image", StringComparison.Ordinal)
                || exception.Message.Contains("Validate coded image", StringComparison.Ordinal)) { rejected = true; }
            if (!rejected) throw new InvalidOperationException("Native accepted invalid coding.");
        }
        if (prefixControls != 4) throw new InvalidOperationException("Missing prefix controls.");
        var proof = new { schemaVersion = 1, timing = false, allDescriptorsExact = true, prefixAndCodingScopesVerified = true, prefixControls,
            librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageCodec).Assembly.Location), nativeWorkerSha256 = nativeHash, controls };
        File.WriteAllText(Path.Combine(directory, "proof.json"), JsonSerializer.Serialize(proof, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        Console.WriteLine(JsonSerializer.Serialize(new { prepared = true, cases = controls.Count, nativeWorkerSha256 = nativeHash, librarySha256 = proof.librarySha256 }));
        void Store(string key, byte[] bytes)
        {
            var path = Path.Combine(directory, key + ".bin");
            if (File.Exists(path) && !File.ReadAllBytes(path).AsSpan().SequenceEqual(bytes)) throw new InvalidOperationException("Frozen read packet changed: " + key);
            File.WriteAllBytes(path, bytes);
        }
    }
}
