using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

public enum WarmWebpDecodeCase { Lossless8, NativeLossless8, StreamNativeLossless8, NativeLosslessCancelable8 }

[MemoryDiagnoser]
public class WarmWebpDecode
{
    private const string Recipe = "HorizontalFlipInput.Create(8): existing original 513x259 asymmetric RGBA8, phase zero, exact alpha and zero-alpha hidden RGB. One owned VP8L input, effort 4; no transforms, color cache or spatial entropy groups. No new source raster or quality enumeration.";
    private const string NativeRecipe = "HorizontalFlipInput.Create(8): same original 513x259 asymmetric RGBA8. One frozen libwebp exact=1 (FFmpeg encoder backend) VP8L input, lossless=1/method=4/quality=75/thread_level=0. Exact RGBA including hidden RGB. No source, quality, effort or strategy enumeration.";
    public static IEnumerable<WarmWebpDecodeCase> Cases => SelectedCases();
    [ParamsSource(nameof(Cases))] public WarmWebpDecodeCase Case { get; set; }
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private byte[] encoded = [], pixels = [];
    private NativeWarmSample[] before = [];
    private WarmWebpDecodePreparation prepared = null!;
    private JsonElement sourceIdentity;
    private bool smoke, ready;
    private int cpu, sampleCount;
    private CancellationTokenSource? cancellationSource;
    private CancellationToken decodeCancellation;
    private bool Cancelable => Case == WarmWebpDecodeCase.NativeLosslessCancelable8;
    private bool StreamInput => Case == WarmWebpDecodeCase.StreamNativeLossless8;
    private bool NativeInput => Case != WarmWebpDecodeCase.Lossless8;

    public static WarmWebpDecodeCase[] SelectedCases()
    {
        var selection = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CASES");
        return selection switch
        {
            null or "Lossless8" => [WarmWebpDecodeCase.Lossless8],
            "NativeLossless8" => [WarmWebpDecodeCase.NativeLossless8],
            "NativeLosslessCancelable8" => [WarmWebpDecodeCase.NativeLosslessCancelable8],
            "StreamNativeLossless8" => [WarmWebpDecodeCase.StreamNativeLossless8],
            _ => throw new ArgumentException("Choose one warm WebP decode case: Lossless8, NativeLossless8, NativeLosslessCancelable8 or StreamNativeLossless8.")
        };
    }

    public static void Prepare(int cpu)
    {
        if (!OperatingSystem.IsLinux() || cpu is < 0 or > 62) throw new ArgumentException("VP8L preparation requires Linux and one allowed CPU.");
        using var process = Process.GetCurrentProcess(); var previousAffinity = process.ProcessorAffinity;
        process.ProcessorAffinity = new IntPtr(1L << cpu);
        try
        {
            using var state = new JpegBenchmarkCase();
            var directory = Path.Combine(state.Root, "artifacts", "warm-reference", "webp-decode");
            var manifest = Path.Combine(directory, "preparation.json");
            if (File.Exists(manifest)) throw new InvalidOperationException("Reuse the frozen VP8L preparation; do not regenerate it for benchmark launches.");
            var pixels = HorizontalFlipInput.Create(8);
            using var source = JpegBenchmarkCase.Buffer(pixels, 513, 259, 8);
            using var output = new MemoryStream();
            ImageCodec.EncodeWebp(source, output, new WebpEncodeOptions { Lossless = true, Effort = 4 });
            var encoded = output.ToArray(); ValidateInput(encoded);
            using var decoded = ImageCodec.Decode(encoded); ValidatePixels(decoded, pixels);
            var oracle = OracleProcess.Run(state.Ffmpeg, DecodeArguments(), encoded);
            if (!oracle.AsSpan().SequenceEqual(pixels)) throw new InvalidOperationException("Frozen VP8L input differs from original pixels through the FFmpeg CLI.");
            Directory.CreateDirectory(directory);
            var input = Path.Combine(directory, "input.webp"); var raster = Path.Combine(directory, "source.rgba");
            File.WriteAllBytes(input, encoded); File.WriteAllBytes(raster, pixels);
            var record = new WarmWebpDecodePreparation(1, "Lokad.Image", Recipe,
                Path.GetRelativePath(state.Root, input), Path.GetRelativePath(state.Root, raster), encoded.Length,
                JpegBenchmarkCase.Hash(encoded), JpegBenchmarkCase.Hash(pixels),
                JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location), DateTime.UtcNow.ToString("O"));
            File.WriteAllText(manifest, JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true }) + "\n");
            Console.WriteLine($"Frozen one original VP8L decode input: {encoded.Length} bytes; exact RGBA through both decoders. No timing/quality enumeration.");
        }
        finally { process.ProcessorAffinity = previousAffinity; }
    }

    private static string[] DecodeArguments(bool scalar = false)
    {
        var arguments = new List<string> { "-hide_banner", "-loglevel", "error", "-threads:v", "1", "-thread_type:v", "0", "-filter_threads", "1", "-noautorotate" };
        if (scalar) arguments.AddRange(["-cpuflags", "0"]);
        arguments.AddRange(["-f", "image2pipe", "-i", "pipe:0", "-frames:v", "1", "-threads:v", "1", "-pix_fmt", "rgba", "-f", "rawvideo", "pipe:1"]);
        return arguments.ToArray();
    }

    private static void ValidateInput(byte[] value, bool nativeInput = false)
    {
        if (value.Length < 26 || !value.AsSpan(8, 8).SequenceEqual("WEBPVP8L"u8) || value[20] != 0x2f
            || (!nativeInput && (value[25] & 7) != 0))
            throw new InvalidOperationException("Expected the declared frozen VP8L input.");
        if (nativeInput && (value.Length != 183812 || JpegBenchmarkCase.Hash(value) != "08ddac115f1770d778e6dbc76373440224b32e6f93b616605e234c63206eebde"))
            throw new InvalidOperationException("Fixed exact native VP8L input differs.");
        var info = ImageCodec.Identify(value);
        if ((info.Format, info.Width, info.Height, info.BitDepth, info.Orientation) != (ImageFormat.WebP, 513, 259, 8, ImageOrientation.Normal))
            throw new InvalidOperationException("VP8L input geometry/depth/orientation differs.");
    }
    private static void ValidatePixels(ImageBuffer value, byte[] pixels)
    {
        if ((value.Width, value.Height, value.BitDepth, value.Orientation) != (513, 259, 8, ImageOrientation.Normal)
            || !OriginalPixels.Bytes(value).AsSpan().SequenceEqual(pixels))
            throw new InvalidOperationException("VP8L decoded RGBA differs, including alpha and zero-alpha hidden RGB.");
    }

    [GlobalSetup]
    public void Setup()
    {
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("Warm native VP8L decode requires Linux.");
        cpu = int.Parse(Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CPU") ?? throw new InvalidOperationException("Missing warm CPU."));
        smoke = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "smoke";
        sampleCount = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "confirm" ? 12 : 3;
        using var process = Process.GetCurrentProcess();
        if (cpu is < 0 or > 62 || process.ProcessorAffinity.ToInt64() != (1L << cpu) || Environment.ProcessorCount != 1
            || GCSettings.IsServerGC || Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") != "0"
            || !GC.GetConfigurationVariables().TryGetValue("ConcurrentGC", out var concurrent) || Convert.ToBoolean(concurrent))
            throw new InvalidOperationException("VP8L worker requires one CPU, optimized JIT and non-concurrent workstation GC.");
        state = new JpegBenchmarkCase();
        try
        {
            using (var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(state.Root, "tmp", "benchmark-source.json"))))
                sourceIdentity = JsonSerializer.SerializeToElement(new { sourceSha256 = manifest.RootElement.GetProperty("sourceSha256").GetString(), sourceRevision = manifest.RootElement.GetProperty("sourceRevision").GetString() });
            var nativeInput = NativeInput;
            prepared = JsonSerializer.Deserialize<WarmWebpDecodePreparation>(File.ReadAllText(Path.Combine(state.Root, "artifacts", "warm-reference", nativeInput ? "webp-native-decode" : "webp-decode", "preparation.json")))
                ?? throw new InvalidOperationException("Prepare one VP8L input explicitly before the warm run.");
            pixels = HorizontalFlipInput.Create(8); encoded = File.ReadAllBytes(Path.Combine(state.Root, prepared.InputFile));
            if (prepared.SchemaVersion != 1 || prepared.Project != "Lokad.Image" || prepared.Recipe != (nativeInput ? NativeRecipe : Recipe)
                || prepared.InputBytes != encoded.Length || prepared.InputSha256 != JpegBenchmarkCase.Hash(encoded)
                || prepared.RasterSha256 != JpegBenchmarkCase.Hash(pixels)
                || !File.ReadAllBytes(Path.Combine(state.Root, prepared.RasterFile)).AsSpan().SequenceEqual(pixels))
                throw new InvalidOperationException("Frozen VP8L preparation identity differs.");
            ValidateInput(encoded, nativeInput); File.WriteAllBytes(state.Input, encoded);
            if (Cancelable)
            {
                cancellationSource = new CancellationTokenSource(); decodeCancellation = cancellationSource.Token;
                using var canceled = new CancellationTokenSource(); canceled.Cancel();
                try
                {
                    using var rejected = ImageCodec.Decode(encoded, cancellationToken: canceled.Token);
                    throw new InvalidOperationException("VP8L accepted an already-canceled token.");
                }
                catch (OperationCanceledException error) when (error.CancellationToken == canceled.Token) { }
            }
            native = new NativeWarmReference(state, 8, cpu, StreamInput ? "webp-stream-decode" : "webp-decode"); ValidateNative();
            foreach (var scalar in new[] { false, true })
                if (!OracleProcess.Run(state.Ffmpeg, DecodeArguments(scalar), encoded).AsSpan().SequenceEqual(pixels))
                    throw new InvalidOperationException("Native VP8L output differs from the pinned normal/scalar public CLI.");
            ValidateManaged(); before = native.Measure(smoke, sampleCount: sampleCount); ready = true;
        }
        catch { native?.Dispose(); cancellationSource?.Dispose(); state.Dispose(); throw; }
    }

    private void ValidateNative()
    {
        if (!File.ReadAllBytes(state.NativeOutput).AsSpan().SequenceEqual(pixels)) throw new InvalidOperationException("Native VP8L RGBA differs from original pixels.");
    }
    private void ValidateManaged()
    {
        using var firstStream = new MemoryStream(encoded, writable: false);
        using var secondStream = new MemoryStream(encoded, writable: false);
        if (firstStream.TryGetBuffer(out _) || secondStream.TryGetBuffer(out _)) throw new InvalidOperationException("Stream backing buffer is exposed.");
        using var first = StreamInput ? ImageCodec.Decode(firstStream) : ImageCodec.Decode(encoded, cancellationToken: decodeCancellation);
        using var second = StreamInput ? ImageCodec.Decode(secondStream) : ImageCodec.Decode(encoded, cancellationToken: decodeCancellation);
        ValidatePixels(first, pixels); ValidatePixels(second, pixels); first.GetRow8(0)[0] ^= 255; ValidatePixels(second, pixels);
        if (StreamInput && (!firstStream.CanRead || !secondStream.CanRead || firstStream.Position != encoded.Length || secondStream.Position != encoded.Length))
            throw new InvalidOperationException("Stream lifetime or complete consumption differs.");
        if (JpegBenchmarkCase.Hash(encoded) != prepared.InputSha256 || !File.ReadAllBytes(state.Input).AsSpan().SequenceEqual(encoded)) throw new InvalidOperationException("Resident VP8L source changed.");
    }

    [Benchmark]
    public int Managed()
    {
        if (StreamInput)
        {
            using var stream = new MemoryStream(encoded, writable: false);
            using var decoded = ImageCodec.Decode(stream);
            return decoded.Stride * decoded.Height ^ decoded.GetRow8(0)[513 * 2];
        }
        using var result = ImageCodec.Decode(encoded, cancellationToken: decodeCancellation);
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
            var path = Path.Combine(state.Root, "artifacts", "benchmark-validation", nameof(WarmWebpDecode) + "-" + Guid.NewGuid().ToString("N") + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = nameof(WarmWebpDecode), caseId = Case.ToString(), smoke,
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"), width = 513, height = 259, depth = 8,
                cpu, managedAffinityCount = 1, processorCount = Environment.ProcessorCount, serverGc = GCSettings.IsServerGC,
                gcConfiguration = GC.GetConfigurationVariables(), runtime = RuntimeInformation.FrameworkDescription, platform = RuntimeInformation.RuntimeIdentifier,
                cancellationCanBeCanceled = decodeCancellation.CanBeCanceled, cancellationRequested = decodeCancellation.IsCancellationRequested,
                alreadyCanceledRejected = Cancelable ? true : (bool?)null, inputAnchorCase = Cancelable ? "NativeLossless8" : null,
                sourceIdentity, preparation = prepared, inputOrigin = NativeInput ? "prepared-native-exact-lossless" : "prepared-owned-lossless", codec = "VP8L",
                inputBytes = encoded.Length, inputSha256 = JpegBenchmarkCase.Hash(encoded), rasterSha256 = JpegBenchmarkCase.Hash(pixels),
                managedOutputBytes = pixels.Length, nativeOutputBytes = pixels.Length, managedOutputSha256 = JpegBenchmarkCase.Hash(pixels), nativeOutputSha256 = JpegBenchmarkCase.HashFile(state.NativeOutput),
                allRgbaSamplesExact = true, alphaExact = true, hiddenRgbExact = true, nativeCliPixelsExact = true, nativeCliNormalScalarPixelsExact = true,
                sourceUnchanged = true, independentOwnedOutput = true, nativeExitCode = 0,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location), benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmWebpDecode).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after,
                freshSeekableStream = StreamInput, inputCopyTimed = StreamInput, publicBufferExposed = false,
                inputConsumedToEnd = StreamInput, callerStreamLeftOpen = StreamInput,
                boundary = (StreamInput ? "Fresh read-only seekable MemoryStream with non-exposed buffer; complete public Decode(Stream) input reads, buffering, copies, stream creation/disposal and output release timed. Native fresh AVIO context/8192-byte scratch, length query, complete acquisition into a fresh padded owned packet, EOF check and input/scratch release timed; no prepared input view. " : "Identical resident frozen VP8L input. ")
                    + "Independent owned RGBA8. Complete public managed Decode, output acquisition/disposal; native software webp decode, state reset, ARGB-to-RGBA conversion, owned output allocation/release. Native codec/conversion contexts and packet/frame wrappers reused; every call decodes and converts every pixel. Padded native rows exposed without an extra packing copy. Startup, I/O, IPC, generation/encoding, hashing and validation excluded. Exact original RGBA, alpha and hidden RGB. One existing raster; no quality enumeration. "
                    + (NativeInput ? "This one frozen exact native encoder input exercises adaptive coding; no profile/strategy sweep." : "Only this literal/run VP8L profile is measured.")
                    + (Cancelable ? " Live cancelable token, never signaled; token/source creation and disposal outside timing. Native has no equivalent cancellation setting; complete decode/conversion/owned output release remain timed." : "")
            }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        }
        finally { native?.Dispose(); cancellationSource?.Dispose(); state?.Dispose(); ready = false; }
    }
}

internal sealed record WarmWebpDecodePreparation(int SchemaVersion, string Project, string Recipe, string InputFile, string RasterFile,
    int InputBytes, string InputSha256, string RasterSha256, string LibrarySha256, string PreparedUtc, JsonElement? NativeEncoderBuild = null);
