using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;

public enum WarmStreamIdentifyCase { Png8 }

/// <summary>Complete fresh Stream PNG inspection without pixel inflation.</summary>
[MemoryDiagnoser]
public class WarmStreamIdentify
{
    public static IEnumerable<WarmStreamIdentifyCase> Cases => SelectedCases();
    [ParamsSource(nameof(Cases))] public WarmStreamIdentifyCase Case { get; set; }
    private WarmAsyncPngIdentify? asyncInput;
    private WarmPngHeaders? headers;
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private byte[] encoded = [], descriptor = [];
    private NativeWarmSample[] before = [];
    private JsonElement sourceIdentity, proof;
    private string proofHash = "";
    private int cpu, sampleCount;
    private bool smoke, ready;
    private static readonly ImageInfo Expected = new(ImageFormat.Png, 513, 259, 8, true, ImageOrientation.Normal);
    private const string InputHash = "4c0c823a1b84d7619f784ace6c2749ed10c0d54b7dc353beed3ad4888e0af57f";

    public static WarmStreamIdentifyCase[] SelectedCases()
        => Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CASES") switch
        {
            null or "Png8" => [WarmStreamIdentifyCase.Png8],
            _ => throw new ArgumentException("Choose the existing C04 fresh Stream PNG Identify case: Png8.")
        };

    [GlobalSetup]
    public void Setup()
    {
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("Use the dedicated Linux VM.");
        if (Case != WarmStreamIdentifyCase.Png8) throw new ArgumentOutOfRangeException(nameof(Case));
        cpu = int.Parse(Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CPU") ?? throw new InvalidOperationException("Missing warm CPU."));
        smoke = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "smoke";
        sampleCount = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "confirm" ? 12 : 3;
        using var process = Process.GetCurrentProcess();
        if (cpu is < 0 or > 62 || process.ProcessorAffinity.ToInt64() != (1L << cpu) || Environment.ProcessorCount != 1
            || GCSettings.IsServerGC || Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") != "0"
            || !GC.GetConfigurationVariables().TryGetValue("ConcurrentGC", out var concurrent) || Convert.ToBoolean(concurrent))
            throw new InvalidOperationException("Identify requires one CPU, optimized JIT and non-concurrent workstation GC.");
        if (Environment.GetEnvironmentVariable("LOKAD_IMAGE_PNG_HEADERS") == "1")
        {
            headers = new WarmPngHeaders(); headers.Setup(cpu, smoke, sampleCount); return;
        }
        if (Environment.GetEnvironmentVariable("LOKAD_IMAGE_IDENTIFY_ASYNC_NONSEEKABLE") == "1")
        {
            asyncInput = new WarmAsyncPngIdentify(); asyncInput.Setup(cpu, smoke, sampleCount); return;
        }
        state = new JpegBenchmarkCase();
        try
        {
            using (var manifest = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "tmp", "benchmark-source.json"))))
                sourceIdentity = JsonSerializer.SerializeToElement(new { sourceSha256 = manifest.RootElement.GetProperty("sourceSha256").GetString(), sourceRevision = manifest.RootElement.GetProperty("sourceRevision").GetString() });
            encoded = File.ReadAllBytes(Path.Combine(state.Root, "artifacts", "warm-reference", "png-cancel8", "input.png"));
            if (JpegBenchmarkCase.Hash(encoded) != InputHash) throw new InvalidOperationException("Existing original PNG packet changed.");
            descriptor = Bytes(Expected);
            ValidateManaged();
            RequirePreparationProof();
            File.WriteAllBytes(state.Input, encoded);
            native = new NativeWarmReference(state, cpu, Expected, stream: true);
            if (proof.GetProperty("nativeBuild").GetProperty("workerSha256").GetString() != native.BuildIdentity.GetProperty("workerSha256").GetString()
                || proof.GetProperty("nativeBuild").GetProperty("sourceSha256").GetString() != native.BuildIdentity.GetProperty("sourceSha256").GetString())
                throw new InvalidOperationException("Prepared equivalent-boundary worker differs from the measured worker.");
            RequireNativeOutput();
            before = native.Measure(smoke, sampleCount: sampleCount);
            ready = true;
        }
        catch { native?.Dispose(); state.Dispose(); throw; }
    }

    private void RequirePreparationProof()
    {
        var bytes = File.ReadAllBytes(Path.Combine(state.Root, "tmp", "c04-png-stream-identify-controls", "proof.json"));
        proofHash = JpegBenchmarkCase.Hash(bytes);
        using var document = JsonDocument.Parse(bytes); proof = document.RootElement.Clone();
        if (proof.GetProperty("timing").GetBoolean() || proof.GetProperty("inputSha256").GetString() != InputHash
            || proof.GetProperty("librarySha256").GetString() != JpegBenchmarkCase.HashFile(typeof(ImageCodec).Assembly.Location)
            || !proof.GetProperty("equivalentMetadataAndValidation").GetBoolean()
            || !proof.GetProperty("invalidDeflateAcceptedWithoutPixelDecode").GetBoolean()
            || !proof.GetProperty("changedMetadataObserved").GetBoolean() || !proof.GetProperty("nativeLiveDescriptorsIndependent").GetBoolean())
            throw new InvalidOperationException("Missing current equivalent-boundary prerequisite.");
        var controls = proof.GetProperty("controls").EnumerateArray().ToArray();
        if (controls.Length != 21 || controls.Select(c => c.GetProperty("name").GetString()).Distinct().Count() != 21
            || controls.Any(c => !c.GetProperty("managedAndNativeAgree").GetBoolean())
            || !controls.Single(c => c.GetProperty("name").GetString() == "invalid-deflate-valid-crc").GetProperty("accepted").GetBoolean()
            || !controls.Single(c => c.GetProperty("name").GetString() == "changed-metadata-width").GetProperty("accepted").GetBoolean()
            || controls.Single(c => c.GetProperty("name").GetString() == "idat-crc").GetProperty("accepted").GetBoolean())
            throw new InvalidOperationException("Prepared PNG boundary controls are incomplete.");
    }

    private void ValidateManaged()
    {
        var first = Inspect(encoded); var second = Inspect(encoded);
        if (first != Expected || second != Expected || ReferenceEquals(first, second))
            throw new InvalidOperationException("Managed descriptors differ or share ownership.");
        var changed = encoded.ToArray(); BinaryPrimitives.WriteInt32BigEndian(changed.AsSpan(16), 514); UpdateCrc(changed, 8);
        if (Inspect(changed) != (Expected with { Width = 514 }) || first != Expected || second != Expected)
            throw new InvalidOperationException("Descriptor metadata is cached or aliases source.");
        var badDeflate = encoded.ToArray(); badDeflate[41] ^= 255; UpdateCrc(badDeflate, 33);
        if (Inspect(badDeflate) != Expected) throw new InvalidOperationException("Identify inflated IDAT.");
        var badCrc = encoded.ToArray(); badCrc[41] ^= 1;
        Reject(badCrc); Reject(encoded.AsSpan(0, encoded.Length - 1));
        var trailing = new byte[encoded.Length + 1]; encoded.CopyTo(trailing, 0); Reject(trailing);
        var exact = ImageLimits.Default with { MaxInputBytes = encoded.Length, MaxWorkingBytes = 262144 + encoded.Length + 8192, MaxWork = 2L * encoded.Length - 8 };
        if (Inspect(encoded, exact) != Expected) throw new InvalidOperationException("Exact Identify admission differs.");
        foreach (var limits in new[] { exact with { MaxInputBytes = encoded.Length - 1 }, exact with { MaxWorkingBytes = exact.MaxWorkingBytes - 1 }, exact with { MaxWork = exact.MaxWork - 1 } })
        {
            try { Inspect(encoded, limits); throw new InvalidOperationException("Identify accepted an insufficient limit."); }
            catch (ImageLimitException) { }
        }
        using var canceled = new CancellationTokenSource(); canceled.Cancel();
        try { Inspect(encoded, cancellationToken: canceled.Token); throw new InvalidOperationException("Identify accepted cancellation."); }
        catch (OperationCanceledException error) when (error.CancellationToken == canceled.Token) { }
        using var observed = new ObservedInput(encoded);
        if (observed.TryGetBuffer(out _) || ImageCodec.Identify(observed) != Expected || !observed.CanRead
            || observed.Position != encoded.Length || !observed.EofRead || observed.ReadByte() != -1)
            throw new InvalidOperationException("Fresh non-exposed Stream/EOF/caller lifetime differs.");
        if (JpegBenchmarkCase.Hash(encoded) != InputHash) throw new InvalidOperationException("PNG source changed.");
    }
    private sealed class ObservedInput(byte[] bytes) : MemoryStream(bytes, writable: false)
    {
        public bool EofRead { get; private set; }
        public override int Read(Span<byte> buffer)
        {
            var count = base.Read(buffer); if (count == 0) EofRead = true; return count;
        }
    }
    private static ImageInfo Inspect(byte[] bytes, ImageLimits? limits = null, CancellationToken cancellationToken = default)
    {
        using var input = new MemoryStream(bytes, writable: false);
        return ImageCodec.Identify(input, limits, cancellationToken);
    }
    private static void Reject(ReadOnlySpan<byte> input)
    {
        try { Inspect(input.ToArray()); throw new InvalidOperationException("Identify accepted an invalid PNG."); }
        catch (ImageDataException) { }
    }
    // Original untimed mutation helper; CRC polynomial is defined by PNG.
    private static void UpdateCrc(byte[] packet, int offset)
    {
        var length = BinaryPrimitives.ReadInt32BigEndian(packet.AsSpan(offset)); uint crc = uint.MaxValue;
        foreach (var value in packet.AsSpan(offset + 4, length + 4))
        {
            crc ^= value;
            for (var i = 0; i < 8; i++) crc = (crc >> 1) ^ (0xedb88320u & (uint)-(int)(crc & 1));
        }
        BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(offset + 8 + length), ~crc);
    }
    private static byte[] Bytes(ImageInfo info)
    {
        var result = new byte[24]; int[] values = [(int)info.Format, info.Width, info.Height, info.BitDepth, info.HasAlpha ? 1 : 0, (int)info.Orientation];
        for (var i = 0; i < values.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(result.AsSpan(4 * i), values[i]);
        return result;
    }
    private void RequireNativeOutput()
    {
        if (!File.ReadAllBytes(state.NativeOutput).AsSpan().SequenceEqual(descriptor))
            throw new InvalidOperationException("Native owned descriptor differs from all six public metadata fields.");
    }
    [Benchmark]
    public ImageInfo Managed()
    {
        if (headers is not null) return headers.Apply();
        if (asyncInput is not null) return asyncInput.Apply();
        using var input = new MemoryStream(encoded, writable: false);
        return ImageCodec.Identify(input);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (headers is not null) { headers.Cleanup(); headers = null; return; }
        if (asyncInput is not null) { asyncInput.Cleanup(); return; }
        try
        {
            if (!ready) return;
            ValidateManaged();
            var after = native.Measure(smoke, before[0].Operations, sampleCount);
            native.Check(); RequireNativeOutput(); native.Stop();
            var path = Path.Combine(state.Root, "artifacts", "benchmark-validation", nameof(WarmStreamIdentify) + "-" + Guid.NewGuid().ToString("N") + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = nameof(WarmStreamIdentify), caseId = Case.ToString(), smoke,
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"), width = 513, height = 259, depth = 8,
                cpu, managedAffinityCount = 1, processorCount = Environment.ProcessorCount, serverGc = GCSettings.IsServerGC,
                gcConfiguration = GC.GetConfigurationVariables(), runtime = RuntimeInformation.FrameworkDescription, platform = RuntimeInformation.RuntimeIdentifier,
                sourceIdentity, nativeSampleTargetMilliseconds = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_NATIVE_SAMPLE_MS") ?? "200",
                inputBytes = encoded.Length, inputSha256 = InputHash, outputSha256 = JpegBenchmarkCase.Hash(descriptor), descriptor = Expected,
                descriptorExact = true, freshStreamControlsExact = true, freshSeekableStream = true, inputView = false, inputCopyTimed = true, eofChecked = true, callerSourceLeftOpen = true, streamAcquisitionAndReleaseTimed = true, preparedEquivalentBoundaryControls = 21, preparedProofSha256 = proofHash,
                invalidDeflateAcceptedWithoutPixelDecode = true, changedMetadataObserved = true, sourceUnchanged = true, independentOwnedOutput = true, nativeExitCode = 0,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageCodec).Assembly.Location), benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmStreamIdentify).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after,
                boundary = "One existing218775-byte original PNG (513x259) through a fresh seekable non-exposed MemoryStream to a fresh independently owned ImageInfo. Stream wrapper/input buffering/copies/EOF and wrapper disposal timed; caller remains open during Identify. Default limits; complete original public ReadInput+PNG structural/metadata/CRC scan, no pixel inflation. FFmpeg fresh custom AVIO context/scratch, length query, exact owned input acquisition/full reads/EOF, original public PNG parser+CRC/full metadata adapter, descriptor materialization/independent ownership and all input/AVIO/descriptor release timed. Original source/parser/codec context reused, input/output owners never cached. Exact six fields,21 same-packet controls and direct fresh non-exposed Stream/EOF/caller lifetime/owner/changedwidth/invalid Deflate/exact input/working/work limits/cancellation gates. One CPU, normal SIMD, nonconcurrent workstation GC; startup, IPC, I/O, generation/hashing/preparation/validation excluded. Same one packet as resident C01; no Stream format/depth grid."
            }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        }
        finally { native?.Dispose(); state?.Dispose(); ready = false; }
    }
}
