using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;

public enum WarmIdentifyCase { Png8, Jpeg8 }

/// <summary>Complete resident PNG/JPEG inspection without pixel decoding.</summary>
[MemoryDiagnoser]
public class WarmIdentify
{
    public static IEnumerable<WarmIdentifyCase> Cases => SelectedCases();
    [ParamsSource(nameof(Cases))] public WarmIdentifyCase Case { get; set; }
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private byte[] encoded = [], descriptor = [];
    private NativeWarmSample[] before = [];
    private JsonElement sourceIdentity, proof;
    private string proofHash = "";
    private int cpu, sampleCount;
    private bool smoke, ready;
    private static readonly ImageInfo PngExpected = new(ImageFormat.Png, 513, 259, 8, true, ImageOrientation.Normal);
    private static readonly ImageInfo JpegExpected = new(ImageFormat.Jpeg, 513, 259, 8, false, ImageOrientation.Normal);
    private bool Jpeg => Case == WarmIdentifyCase.Jpeg8;
    private ImageInfo Expected => Jpeg ? JpegExpected : PngExpected;
    private string InputHash => Jpeg ? "ccca91bcdaddc90c7aecf62d07cdc629c09c27e607c955cfcc97a7bf91caaf08"
        : "4c0c823a1b84d7619f784ace6c2749ed10c0d54b7dc353beed3ad4888e0af57f";

    public static WarmIdentifyCase[] SelectedCases()
        => Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CASES") switch
        {
            null or "Png8" => [WarmIdentifyCase.Png8],
            "Jpeg8" => [WarmIdentifyCase.Jpeg8],
            _ => throw new ArgumentException("Choose one existing resident Identify boundary: Png8 or Jpeg8.")
        };

    [GlobalSetup]
    public void Setup()
    {
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("Use the dedicated Linux VM.");
        if (Case is not (WarmIdentifyCase.Png8 or WarmIdentifyCase.Jpeg8)) throw new ArgumentOutOfRangeException(nameof(Case));
        cpu = int.Parse(Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CPU") ?? throw new InvalidOperationException("Missing warm CPU."));
        smoke = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "smoke";
        sampleCount = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "confirm" ? 12 : 3;
        using var process = Process.GetCurrentProcess();
        if (cpu is < 0 or > 62 || process.ProcessorAffinity.ToInt64() != (1L << cpu) || Environment.ProcessorCount != 1
            || GCSettings.IsServerGC || Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") != "0"
            || !GC.GetConfigurationVariables().TryGetValue("ConcurrentGC", out var concurrent) || Convert.ToBoolean(concurrent))
            throw new InvalidOperationException("Identify requires one CPU, optimized JIT and non-concurrent workstation GC.");
        state = new JpegBenchmarkCase();
        try
        {
            using (var manifest = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "tmp", "benchmark-source.json"))))
                sourceIdentity = JsonSerializer.SerializeToElement(new { sourceSha256 = manifest.RootElement.GetProperty("sourceSha256").GetString(), sourceRevision = manifest.RootElement.GetProperty("sourceRevision").GetString() });
            encoded = File.ReadAllBytes(Path.Combine(state.Root, "artifacts", "warm-reference", Jpeg ? "jpeg-decode420" : "png-cancel8", Jpeg ? "native.jpg" : "input.png"));
            if (JpegBenchmarkCase.Hash(encoded) != InputHash) throw new InvalidOperationException("Existing original encoded packet changed.");
            descriptor = Bytes(Expected);
            ValidateManaged();
            RequirePreparationProof();
            File.WriteAllBytes(state.Input, encoded);
            native = new NativeWarmReference(state, cpu, Expected);
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
        var bytes = File.ReadAllBytes(Path.Combine(state.Root, "tmp", Jpeg ? "c02-jpeg-identify-controls" : "c01-png-identify-controls", "proof.json"));
        proofHash = JpegBenchmarkCase.Hash(bytes);
        using var document = JsonDocument.Parse(bytes); proof = document.RootElement.Clone();
        if (proof.GetProperty("timing").GetBoolean() || proof.GetProperty("inputSha256").GetString() != InputHash
            || proof.GetProperty("librarySha256").GetString() != JpegBenchmarkCase.HashFile(typeof(ImageCodec).Assembly.Location)
            || !proof.GetProperty("equivalentMetadataAndValidation").GetBoolean()
            || !proof.GetProperty(Jpeg ? "opaqueEntropyAcceptedWithoutPixelDecode" : "invalidDeflateAcceptedWithoutPixelDecode").GetBoolean()
            || !proof.GetProperty("changedMetadataObserved").GetBoolean() || !proof.GetProperty("nativeLiveDescriptorsIndependent").GetBoolean())
            throw new InvalidOperationException("Missing current equivalent-boundary prerequisite.");
        var controls = proof.GetProperty("controls").EnumerateArray().ToArray();
        if (controls.Length != 21 || controls.Select(c => c.GetProperty("name").GetString()).Distinct().Count() != 21
            || controls.Any(c => !c.GetProperty("managedAndNativeAgree").GetBoolean())
            || !controls.Single(c => c.GetProperty("name").GetString() == (Jpeg ? "opaque-entropy-not-decoded" : "invalid-deflate-valid-crc")).GetProperty("accepted").GetBoolean()
            || !controls.Single(c => c.GetProperty("name").GetString() == "changed-metadata-width").GetProperty("accepted").GetBoolean()
            || controls.Single(c => c.GetProperty("name").GetString() == (Jpeg ? "missing-eoi" : "idat-crc")).GetProperty("accepted").GetBoolean())
            throw new InvalidOperationException("Prepared boundary controls are incomplete.");
    }

    private void ValidateManaged()
    {
        if (Jpeg) { ValidateManagedJpeg(); return; }
        var first = ImageCodec.Identify(encoded); var second = ImageCodec.Identify(encoded);
        if (first != Expected || second != Expected || ReferenceEquals(first, second))
            throw new InvalidOperationException("Managed descriptors differ or share ownership.");
        var changed = encoded.ToArray(); BinaryPrimitives.WriteInt32BigEndian(changed.AsSpan(16), 514); UpdateCrc(changed, 8);
        if (ImageCodec.Identify(changed) != (Expected with { Width = 514 }) || first != Expected || second != Expected)
            throw new InvalidOperationException("Descriptor metadata is cached or aliases source.");
        var badDeflate = encoded.ToArray(); badDeflate[41] ^= 255; UpdateCrc(badDeflate, 33);
        if (ImageCodec.Identify(badDeflate) != Expected) throw new InvalidOperationException("Identify inflated IDAT.");
        var badCrc = encoded.ToArray(); badCrc[41] ^= 1;
        Reject(badCrc); Reject(encoded.AsSpan(0, encoded.Length - 1));
        var trailing = new byte[encoded.Length + 1]; encoded.CopyTo(trailing, 0); Reject(trailing);
        var exact = ImageLimits.Default with { MaxInputBytes = encoded.Length, MaxWorkingBytes = encoded.Length, MaxWork = encoded.Length - 8 };
        if (ImageCodec.Identify(encoded, exact) != Expected) throw new InvalidOperationException("Exact Identify admission differs.");
        foreach (var limits in new[] { exact with { MaxInputBytes = encoded.Length - 1 }, exact with { MaxWorkingBytes = encoded.Length - 1 }, exact with { MaxWork = encoded.Length - 9 } })
        {
            try { ImageCodec.Identify(encoded, limits); throw new InvalidOperationException("Identify accepted an insufficient limit."); }
            catch (ImageLimitException) { }
        }
        using var canceled = new CancellationTokenSource(); canceled.Cancel();
        try { ImageCodec.Identify(encoded, cancellationToken: canceled.Token); throw new InvalidOperationException("Identify accepted cancellation."); }
        catch (OperationCanceledException error) when (error.CancellationToken == canceled.Token) { }
        if (JpegBenchmarkCase.Hash(encoded) != InputHash) throw new InvalidOperationException("PNG source changed.");
    }
    private void ValidateManagedJpeg()
    {
        var first = ImageCodec.Identify(encoded); var second = ImageCodec.Identify(encoded);
        if (first != Expected || second != Expected || ReferenceEquals(first, second))
            throw new InvalidOperationException("JPEG descriptors differ or share ownership.");
        var framePayload = 0; var scanEnd = 0; var position = 2;
        while (position < encoded.Length)
        {
            if (encoded[position++] != 255) throw new InvalidOperationException("Prepared marker differs.");
            while (encoded[position] == 255) position++;
            var marker = encoded[position++]; var length = BinaryPrimitives.ReadUInt16BigEndian(encoded.AsSpan(position));
            if (marker is 0xc0 or 0xc2) framePayload = position + 2;
            position += length;
            if (marker == 0xda) { scanEnd = position; break; }
        }
        if (framePayload == 0 || scanEnd == 0) throw new InvalidOperationException("Prepared frame/scan missing.");
        var changed = encoded.ToArray(); BinaryPrimitives.WriteInt16BigEndian(changed.AsSpan(framePayload + 3), 514);
        if (ImageCodec.Identify(changed) != (Expected with { Width = 514 }) || first != Expected || second != Expected)
            throw new InvalidOperationException("JPEG metadata is cached or aliases source.");
        var opaque = new byte[scanEnd + 3]; encoded.AsSpan(0, scanEnd).CopyTo(opaque);
        opaque[scanEnd] = 127; opaque[scanEnd + 1] = 255; opaque[scanEnd + 2] = 0xd9;
        if (ImageCodec.Identify(opaque) != Expected) throw new InvalidOperationException("Identify decoded JPEG entropy.");
        Reject(encoded.AsSpan(0, encoded.Length - 1));
        var trailing = new byte[encoded.Length + 1]; encoded.CopyTo(trailing, 0); Reject(trailing);
        var exact = ImageLimits.Default with { MaxInputBytes = encoded.Length, MaxWorkingBytes = encoded.Length };
        if (ImageCodec.Identify(encoded, exact) != Expected) throw new InvalidOperationException("JPEG input/working admission differs.");
        foreach (var limits in new[] { exact with { MaxInputBytes = encoded.Length - 1 }, exact with { MaxWorkingBytes = encoded.Length - 1 }, exact with { MaxWork = 1 } })
        {
            try { ImageCodec.Identify(encoded, limits); throw new InvalidOperationException("JPEG Identify accepted insufficient limits."); }
            catch (ImageLimitException) { }
        }
        using var canceled = new CancellationTokenSource(); canceled.Cancel();
        try { ImageCodec.Identify(encoded, cancellationToken: canceled.Token); throw new InvalidOperationException("JPEG Identify accepted cancellation."); }
        catch (OperationCanceledException error) when (error.CancellationToken == canceled.Token) { }
        if (JpegBenchmarkCase.Hash(encoded) != InputHash) throw new InvalidOperationException("JPEG source changed.");
    }
    private static void Reject(ReadOnlySpan<byte> input)
    {
        try { ImageCodec.Identify(input); throw new InvalidOperationException("Identify accepted an invalid PNG."); }
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
    public ImageInfo Managed() => ImageCodec.Identify(encoded);

    [GlobalCleanup]
    public void Cleanup()
    {
        try
        {
            if (!ready) return;
            ValidateManaged();
            var after = native.Measure(smoke, before[0].Operations, sampleCount);
            native.Check(); RequireNativeOutput(); native.Stop();
            var path = Path.Combine(state.Root, "artifacts", "benchmark-validation", nameof(WarmIdentify) + "-" + Guid.NewGuid().ToString("N") + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = nameof(WarmIdentify), caseId = Case.ToString(), smoke,
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"), width = 513, height = 259, depth = 8,
                cpu, managedAffinityCount = 1, processorCount = Environment.ProcessorCount, serverGc = GCSettings.IsServerGC,
                gcConfiguration = GC.GetConfigurationVariables(), runtime = RuntimeInformation.FrameworkDescription, platform = RuntimeInformation.RuntimeIdentifier,
                sourceIdentity, nativeSampleTargetMilliseconds = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_NATIVE_SAMPLE_MS") ?? "200",
                inputBytes = encoded.Length, inputSha256 = InputHash, outputSha256 = JpegBenchmarkCase.Hash(descriptor), descriptor = Expected,
                descriptorExact = true, directSpanControlsExact = true, preparedEquivalentBoundaryControls = 21, preparedProofSha256 = proofHash,
                invalidDeflateAcceptedWithoutPixelDecode = !Jpeg, opaqueEntropyAcceptedWithoutPixelDecode = Jpeg, changedMetadataObserved = true, sourceUnchanged = true, independentOwnedOutput = true, nativeExitCode = 0,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageCodec).Assembly.Location), benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmIdentify).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after,
                boundary = Jpeg ? "One resident frozen 52093-byte original JPEG420 (513x259) to a fresh independently owned ImageInfo. Complete public span Identify, default/input/work/working admission, full original marker/scan-header/entropy-stuffing/EXIF/EOI+EOF structural validation; no coefficient/pixel decoding. FFmpeg MJPEG discard-all metadata extraction with original complete structural/EXIF scan and TIMED canonical17-byte SOF2 grayscale metadata-header projection. Projection removes stronger DQT/sampling/encoded-size checks intentionally outside Identify; all six original metadata fields remain exact. Fresh projection payload, native decoder reset/header submission, owned AVCodecParameters extraction/materialization/release timed; codec/frame/packet wrappers and original input reused, descriptor never cached. Pixel buffer callback rejects every request (zero allowed). Twenty-one same-packet controls plus direct span owner/changedwidth/opaque-entropy/limits/cancellation gates. Only this existing resident packet/default scope is timed; startup, IPC, I/O, hashing/preparation/validation excluded, one CPU, normal SIMD, no concurrency."
                    : "One resident frozen 218775-byte original RGBA8 PNG (513x259) to a fresh independently owned ImageInfo. Complete public span Identify, input/default-limit admission, full chunk scan/CRC, required metadata validation and descriptor construction. FFmpeg public PNG parser/CRC with original timed structural/metadata compatibility adapter and fresh AVCodecParameters acquisition/materialization/release; no pixel decoder or inflation on either side. Parser/context/input reused, descriptor never cached. All six metadata fields exact; native live descriptor independence and 21 same-packet controls including invalid Deflate/valid CRC, changed width, orientation6, CRC, truncation, ordering, IEND/EOF and limits. Direct managed span controls also cover exact input/working/work limits and cancellation. Only this resident PNG boundary is measured. Startup, IPC, I/O, generation/hashing/validation excluded; one CPU, normal SIMD, no concurrency."
            }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        }
        finally { native?.Dispose(); state?.Dispose(); ready = false; }
    }
}
