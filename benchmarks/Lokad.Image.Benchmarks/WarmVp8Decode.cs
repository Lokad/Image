using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

public enum WarmVp8DecodeCase { Native8, Opaque8, StreamNative8, OpaqueCancelable8 }

[MemoryDiagnoser]
public class WarmVp8Decode
{
    private const string InputHash = "8fcc1c935850d7813ebfce329a8b00c41fe9b02683d2be40b91dedfd74192a10";
    private const string RasterHash = "70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80";
    private const string NativeHash = "55cdb9d8ce5e4dbaf16967129b299f6f900aa1c2fa8dad554d26d91b02295dcd";
    private const string ManagedHash = "f8bc9742ab57cc3c399ff3506edeb91072fed7338e3c9997971157403000c75a";
    public static IEnumerable<WarmVp8DecodeCase> Cases => SelectedCases();
    [ParamsSource(nameof(Cases))] public WarmVp8DecodeCase Case { get; set; }
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private byte[] encoded = [], pixels = [], expectedNative = [];
    private NativeWarmSample[] before = [];
    private JsonElement sourceIdentity;
    private QualityError agreement = null!;
    private bool smoke, ready;
    private int cpu, sampleCount;
    private bool Opaque => Case is WarmVp8DecodeCase.Opaque8 or WarmVp8DecodeCase.OpaqueCancelable8;
    private bool Cancelable => Case == WarmVp8DecodeCase.OpaqueCancelable8;
    private CancellationTokenSource? cancellationSource;
    private CancellationToken decodeCancellation;
    private bool alreadyCanceledRejected;
    private bool StreamInput => Case == WarmVp8DecodeCase.StreamNative8;
    private string expectedManagedHash = ManagedHash, expectedNativeHash = NativeHash;

    public static WarmVp8DecodeCase[] SelectedCases()
    {
        var selection = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CASES");
        return selection switch
        {
            null or "Native8" => [WarmVp8DecodeCase.Native8],
            "Opaque8" => [WarmVp8DecodeCase.Opaque8],
            "OpaqueCancelable8" => [WarmVp8DecodeCase.OpaqueCancelable8],
            "StreamNative8" => [WarmVp8DecodeCase.StreamNative8],
            _ => throw new ArgumentException("Choose one warm VP8 decode case: Native8, Opaque8, OpaqueCancelable8 or StreamNative8.")
        };
    }

    // Keep the public oracle's specified color pipeline, with normal SIMD and one
    // input decoder thread. Quality matching is irrelevant to identical-input decode.
    private string[] DecodeArguments()
    {
        var arguments = WebpOracle.DecodeArguments(513, 259, !Opaque).ToList();
        var flags = arguments.IndexOf("-cpuflags"); arguments.RemoveRange(flags, 2);
        arguments.InsertRange(arguments.IndexOf("-f"), ["-threads:v", "1", "-thread_type:v", "0"]);
        return arguments.ToArray();
    }

    [GlobalSetup]
    public void Setup()
    {
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("Warm native VP8 decode requires Linux.");
        cpu = int.Parse(Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CPU") ?? throw new InvalidOperationException("Missing warm CPU."));
        smoke = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "smoke";
        sampleCount = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "confirm" ? 12 : 3;
        using var process = Process.GetCurrentProcess();
        if (cpu is < 0 or > 62 || process.ProcessorAffinity.ToInt64() != (1L << cpu) || Environment.ProcessorCount != 1
            || GCSettings.IsServerGC || Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") != "0"
            || !GC.GetConfigurationVariables().TryGetValue("ConcurrentGC", out var concurrent) || Convert.ToBoolean(concurrent))
            throw new InvalidOperationException("VP8 worker requires one CPU, optimized JIT and non-concurrent workstation GC.");
        state = new JpegBenchmarkCase();
        try
        {
            using (var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(state.Root, "tmp", "benchmark-source.json"))))
                sourceIdentity = JsonSerializer.SerializeToElement(new { sourceSha256 = manifest.RootElement.GetProperty("sourceSha256").GetString(), sourceRevision = manifest.RootElement.GetProperty("sourceRevision").GetString() });
            if (Opaque)
            {
                using var prepared = JsonDocument.Parse(File.ReadAllText(Path.Combine(state.Root, "artifacts", "warm-reference", "vp8-opaque-match.json")));
                var receipt = prepared.RootElement;
                pixels = HorizontalFlipInput.Create(8);
                if (JpegBenchmarkCase.Hash(pixels) != RasterHash) throw new InvalidOperationException("Original opaque projection source differs.");
                for (var i = 3; i < pixels.Length; i += 4) pixels[i] = 255;
                encoded = File.ReadAllBytes(Path.Combine(state.Root, receipt.GetProperty("inputFile").GetString()!));
                expectedManagedHash = receipt.GetProperty("managedOutputSha256").GetString()!;
                expectedNativeHash = receipt.GetProperty("nativeOutputSha256").GetString()!;
                if (receipt.GetProperty("schemaVersion").GetInt32() != 1 || receipt.GetProperty("project").GetString() != "Lokad.Image"
                    || receipt.GetProperty("originalRasterSha256").GetString() != RasterHash
                    || receipt.GetProperty("nativeQuality").GetInt32() != 75 || receipt.GetProperty("nativeEffort").GetInt32() != 4
                    || receipt.GetProperty("qualityEnumeration").GetBoolean()
                    || JpegBenchmarkCase.Hash(pixels) != receipt.GetProperty("rasterSha256").GetString()
                    || JpegBenchmarkCase.Hash(encoded) != receipt.GetProperty("inputSha256").GetString()
                    || encoded.Length != receipt.GetProperty("inputBytes").GetInt32() || !encoded.AsSpan(12, 4).SequenceEqual("VP8 "u8))
                    throw new InvalidOperationException("Frozen opaque native VP8 input identity differs.");
            }
            else
            {
                var prepared = JsonSerializer.Deserialize<WarmWebpPreparation>(File.ReadAllText(Path.Combine(state.Root, "artifacts", "warm-reference", "webp-match.json")), EncoderQualityCalibration.Json)
                    ?? throw new InvalidOperationException("Reuse the frozen native VP8 input; do not enumerate encode qualities.");
                pixels = HorizontalFlipInput.Create(8); encoded = File.ReadAllBytes(Path.Combine(state.Root, prepared.NativeFile));
                var anchor = prepared.Match.Native;
                if (prepared.SchemaVersion != 1 || prepared.Project != "Lokad.Image" || prepared.NativeQuality != 75
                    || prepared.RasterSha256 != RasterHash || JpegBenchmarkCase.Hash(pixels) != RasterHash
                    || encoded.Length != 98590 || JpegBenchmarkCase.Hash(encoded) != InputHash
                    || anchor.Setting != 75 || anchor.EncodedBytes != encoded.Length || anchor.EncodedSha256 != InputHash
                    || anchor.Codec != "VP8" || anchor.Sampling != "4:2:0" || !anchor.AlphaExact
                    || anchor.NativeDecodedSha256 != NativeHash || anchor.PublicDecodedSha256 != ManagedHash)
                    throw new InvalidOperationException("Frozen native VP8 input identity differs.");
            }
            var info = ImageCodec.Identify(encoded);
            if ((info.Format, info.Width, info.Height, info.BitDepth, info.Orientation) != (ImageFormat.WebP, 513, 259, 8, ImageOrientation.Normal))
                throw new InvalidOperationException("VP8 input geometry/depth/orientation differs.");
            if (Opaque && info.HasAlpha) throw new InvalidOperationException("Opaque VP8 must not carry an alpha chunk.");
            expectedNative = OracleProcess.Run(state.Ffmpeg, DecodeArguments(), encoded);
            if (JpegBenchmarkCase.Hash(expectedNative) != expectedNativeHash) throw new InvalidOperationException("Pinned CLI VP8 pixels differ.");
            if ((StreamInput || Cancelable) && !OracleProcess.Run(state.Ffmpeg, ["-cpuflags", "0", .. DecodeArguments()], encoded).AsSpan().SequenceEqual(expectedNative))
                throw new InvalidOperationException("Scalar CLI VP8 Stream anchor differs.");
            ValidateAlpha(expectedNative); File.WriteAllBytes(state.Input, encoded);
            if (Cancelable)
            {
                cancellationSource = new CancellationTokenSource(); decodeCancellation = cancellationSource.Token;
                using var canceled = new CancellationTokenSource(); canceled.Cancel();
                try
                {
                    using var rejected = ImageCodec.Decode(encoded, cancellationToken: canceled.Token);
                    throw new InvalidOperationException("VP8 accepted an already-canceled token.");
                }
                catch (OperationCanceledException error) when (error.CancellationToken == canceled.Token)
                {
                    alreadyCanceledRejected = true;
                }
            }
            native = new NativeWarmReference(state, 8, cpu, StreamInput ? "vp8-stream-decode" : "vp8-decode", vp8Opaque: Opaque); ValidateNative();
            ValidateManaged(); before = native.Measure(smoke, sampleCount: sampleCount); ready = true;
        }
        catch { native?.Dispose(); cancellationSource?.Dispose(); state.Dispose(); throw; }
    }

    private void ValidateAlpha(byte[] value)
    {
        if (value.Length != pixels.Length) throw new InvalidOperationException("VP8 output size differs.");
        for (var index = 3; index < value.Length; index += 4)
            if (value[index] != pixels[index]) throw new InvalidOperationException("VP8 decoded alpha differs.");
    }
    private void ValidateNative()
    {
        if (!File.ReadAllBytes(state.NativeOutput).AsSpan().SequenceEqual(expectedNative))
            throw new InvalidOperationException("Native VP8 pixels differ from the pinned public CLI.");
    }
    private void ValidatePixels(ImageBuffer value)
    {
        if ((value.Width, value.Height, value.BitDepth, value.Orientation) != (513, 259, 8, ImageOrientation.Normal))
            throw new InvalidOperationException("VP8 decoded geometry/depth/orientation differs.");
        var actual = OriginalPixels.Bytes(value); ValidateAlpha(actual);
        if (JpegBenchmarkCase.Hash(actual) != expectedManagedHash) throw new InvalidOperationException("Frozen public VP8 pixels changed.");
        long sum = 0; var maximum = 0;
        for (var index = 0; index < actual.Length; index++)
        {
            if (index % 4 == 3) continue;
            var error = Math.Abs(actual[index] - expectedNative[index]);
            maximum = Math.Max(maximum, error); sum += (long)error * error;
        }
        agreement = new(sum, maximum, actual.Length / 4 * 3);
        if (agreement.RgbMaximum > 4 || agreement.RgbRmse > 1.5) throw new InvalidOperationException("VP8 RGB agreement exceeds the specified bounds, including hidden RGB.");
    }
    private void ValidateManaged()
    {
        if (Cancelable && (!decodeCancellation.CanBeCanceled || decodeCancellation.IsCancellationRequested || !alreadyCanceledRejected))
            throw new InvalidOperationException("VP8 requires a live, never-signaled token and pre-cancellation rejection.");
        using var first = DecodeManaged(); using var second = DecodeManaged();
        ValidatePixels(first); ValidatePixels(second); first.GetRow8(0)[0] ^= 255; ValidatePixels(second);
        if (!File.ReadAllBytes(state.Input).AsSpan().SequenceEqual(encoded)) throw new InvalidOperationException("Resident VP8 source changed.");
        if (StreamInput)
        {
            using var caller = new MemoryStream(encoded, writable: false);
            if (caller.TryGetBuffer(out _)) throw new InvalidOperationException("VP8 input must not expose its buffer.");
            using var output = ImageCodec.Decode(caller); ValidatePixels(output);
            if (!caller.CanRead || caller.Position != caller.Length || caller.ReadByte() != -1)
                throw new InvalidOperationException("VP8 caller lifetime or complete input reading differs.");
        }
    }

    private ImageBuffer DecodeManaged()
    {
        if (!StreamInput) return ImageCodec.Decode(encoded, cancellationToken: decodeCancellation);
        using var input = new MemoryStream(encoded, writable: false);
        return ImageCodec.Decode(input);
    }

    [Benchmark]
    public int Managed()
    {
        using var result = DecodeManaged();
        return result.Stride * result.Height ^ result.GetRow8(0)[513 * 2];
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        try
        {
            if (!ready) return;
            ValidateManaged(); var after = native.Measure(smoke, before[0].Operations, sampleCount);
            native.Check(); ValidateNative(); native.Stop();
            if (!OracleProcess.Run(state.Ffmpeg, DecodeArguments(), encoded).AsSpan().SequenceEqual(expectedNative))
                throw new InvalidOperationException("Final pinned CLI VP8 pixels differ.");
            var path = Path.Combine(state.Root, "artifacts", "benchmark-validation", nameof(WarmVp8Decode) + "-" + Guid.NewGuid().ToString("N") + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = nameof(WarmVp8Decode), caseId = Case.ToString(), smoke,
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"), width = 513, height = 259, depth = 8,
                cpu, managedAffinityCount = 1, processorCount = Environment.ProcessorCount, serverGc = GCSettings.IsServerGC,
                gcConfiguration = GC.GetConfigurationVariables(), runtime = RuntimeInformation.FrameworkDescription, platform = RuntimeInformation.RuntimeIdentifier,
                sourceIdentity, inputOrigin = "prepared-native-75", codec = "VP8", sampling = "4:2:0",
                inputBytes = encoded.Length, inputSha256 = JpegBenchmarkCase.Hash(encoded), rasterSha256 = JpegBenchmarkCase.Hash(pixels),
                managedOutputBytes = pixels.Length, nativeOutputBytes = pixels.Length, managedOutputSha256 = expectedManagedHash, nativeOutputSha256 = expectedNativeHash,
                opaque = Opaque, originalRasterSha256 = RasterHash, rasterRecipe = Opaque ? "Existing original RGBA8 RGB bytes unchanged; alpha set to255. No additional original image." : "Original RGBA8 including exact alpha and hidden RGB unchanged.",
                alphaExact = true, nativeCliPixelsExact = true, decoderAgreementWithinBounds = true,
                freshSeekableStream = StreamInput, inputView = false, inputCopiesTimed = StreamInput,
                callerInputLifetimeChecked = StreamInput, scalarCliPixelsExact = StreamInput || Cancelable,
                cancellationCanBeCanceled = decodeCancellation.CanBeCanceled,
                cancellationRequested = decodeCancellation.IsCancellationRequested,
                alreadyCanceledRejected = Cancelable ? alreadyCanceledRejected : (bool?)null,
                cancellationBoundary = Cancelable ? "One live never-signaled token supplied to every timed public Decode; token creation/disposal excluded. Pre-cancellation/token identity checked outside timing." : null,
                decoderMaximum = agreement.Maximum, decoderRmse = agreement.RgbRmse, decoderSumSquared = agreement.SumSquared,
                sourceUnchanged = true, independentOwnedOutput = true, nativeExitCode = 0,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location), benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmVp8Decode).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after,
                boundary = StreamInput
                    ? "Fresh non-exposed seekable VP8/ALPH Stream to independent owned RGBA8. Complete default managed input buffering/copies and EOF reading, full public Decode/disposal. Native fresh AVIO/owned packet acquisition, complete reading/EOF, software webp state reset/decode, centered chroma expansion, limited BT601 through RGBA16/no-dither reduction, exact alpha merge and all input/scratch/output acquisition/release timed. Decoder/conversion/frame/packet wrappers reused; every call reacquires input and decodes/converts all pixels. No demux, exposed input-buffer shortcut, output cache or redundant packing. One CPU/codec/conversion thread, normal SIMD. Startup/I/O/IPC/generation/hashing/validation excluded. Existing frozen input and exact per-decoder pixel anchors reused; no encoding or quality search."
                    : Opaque
                    ? "Identical resident frozen opaque VP8 keyframe input to independent owned RGBA8. Complete public managed Decode/disposal; native software webp decode/state reset, centered bilinear chroma expansion, limited BT601 conversion through RGBA16, no-dither RGBA8 reduction and real scratch/output allocation/release. One codec/conversion thread, normal SIMD. Opaque alpha255 comes from RGB conversion, with no alpha-plane merge. Contexts and packet/frame wrappers reused; every call decodes/converts all pixels. Padded rows exposed directly. Startup/I/O/IPC/hashing/checks/encoding excluded. One opacity projection of the existing original raster; no quality enumeration. Identical-input decode requires no matched encode quality."
                    : "Identical resident frozen VP8/ALPH input to independent owned RGBA8. Complete public managed Decode and disposal; native software webp decode, state reset, centered bilinear chroma expansion, limited BT.601 conversion through RGBA16, RGBA8 reduction, exact alpha merge and scratch/output allocation/release. One codec/conversion thread, normal SIMD. Native contexts and packet/frame wrappers reused; every call decodes and converts every pixel. Padded output rows exposed without a packing copy. Startup, I/O, IPC, hashing, validation and encoding excluded. One existing raster/input; no regeneration or quality enumeration. Decode does not require a matched encoder quality. Only this native keyframe/alpha profile is measured."
            }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        }
        finally { native?.Dispose(); cancellationSource?.Dispose(); state?.Dispose(); ready = false; }
    }
}
