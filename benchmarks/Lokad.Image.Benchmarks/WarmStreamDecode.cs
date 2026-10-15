using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

/// <summary>Warm single-CPU operation; native elapsed samples are measured inside its worker.</summary>
public enum StreamDecodeCase { Png8, PngCropFit8, Png16 }

[MemoryDiagnoser]
public class WarmStreamDecode
{
    public static IEnumerable<StreamDecodeCase> Cases => SelectedCases();
    [ParamsSource(nameof(Cases))] public StreamDecodeCase Case { get; set; }
    private int Depth => Case == StreamDecodeCase.Png16 ? 16 : 8;
    private string EncodedHash => Depth == 16 ? "3e4b64d778c395392451d5331d2a11b541d262e2f39826e1322d22d6f03bb4a8" : "4c0c823a1b84d7619f784ace6c2749ed10c0d54b7dc353beed3ad4888e0af57f";
    private bool Compose => Case == StreamDecodeCase.PngCropFit8;
    private int OutputWidth => Compose ? 127 : HorizontalFlipInput.Width;
    private int OutputHeight => Compose ? 64 : HorizontalFlipInput.Height;
    public static StreamDecodeCase[] SelectedCases() => Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CASES") switch
    {
        null or "Png8" => [StreamDecodeCase.Png8],
        "PngCropFit8" => [StreamDecodeCase.PngCropFit8],
        "Png16" => [StreamDecodeCase.Png16],
        _ => throw new ArgumentException("Choose one fresh Stream case: Png8, Png16 or PngCropFit8.")
    };
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private byte[] encoded = [];
    private byte[] input = [], expected = [];
    private NativeWarmSample[] before = [];
    private bool smoke, ready;
    private int cpu;
    private int sampleCount;
    private JsonElement sourceIdentity;

    [GlobalSetup]
    public void Setup()
    {
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("Warm FFmpeg reference measurements require Linux.");
        ready = false;
        if (Case is not (StreamDecodeCase.Png8 or StreamDecodeCase.PngCropFit8 or StreamDecodeCase.Png16)) throw new ArgumentOutOfRangeException(nameof(Case));
        cpu = int.Parse(Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CPU") ?? throw new InvalidOperationException("Missing warm CPU."));
        smoke = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "smoke";
        sampleCount = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "confirm" ? 12 : 3;
        if (cpu is < 0 or > 62 || Process.GetCurrentProcess().ProcessorAffinity.ToInt64() != (1L << cpu)
            || Environment.ProcessorCount != 1 || GCSettings.IsServerGC
            || Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") != "0")
            throw new InvalidOperationException("Managed worker requires one pinned CPU, processor count one and workstation GC.");
        var gc = GC.GetConfigurationVariables();
        if (!gc.TryGetValue("ConcurrentGC", out var concurrent) || Convert.ToBoolean(concurrent))
            throw new InvalidOperationException("Managed warm worker requires background GC disabled.");
        state = new JpegBenchmarkCase();
        try
        {
            var manifestPath = Path.Combine(state.Root, "tmp", "benchmark-source.json");
            if (!File.Exists(manifestPath)) throw new InvalidOperationException("Capture the owned source inventory before warm execution.");
            using (var manifest = JsonDocument.Parse(File.ReadAllText(manifestPath)))
                sourceIdentity = JsonSerializer.SerializeToElement(new
                {
                    sourceSha256 = manifest.RootElement.GetProperty("sourceSha256").GetString(),
                    sourceRevision = manifest.RootElement.GetProperty("sourceRevision").GetString()
                });
            input = HorizontalFlipInput.Create(Depth);
            expected = Compose ? ExpectedComposition(input) : input;
            using (var source = JpegBenchmarkCase.Buffer(input, HorizontalFlipInput.Width, HorizontalFlipInput.Height, Depth))
            using (var output = new MemoryStream())
            {
                ImageCodec.EncodePng(source, output); encoded = output.ToArray();
            }
            if (JpegBenchmarkCase.Hash(encoded) != EncodedHash)
                throw new InvalidOperationException("Frozen existing PNG input changed.");
            File.WriteAllBytes(state.Input, encoded);
            File.WriteAllBytes(Path.Combine(state.DirectoryPath, "expected.raw"), expected);
            ValidateManaged();
            native = new NativeWarmReference(state, Depth, cpu, Compose ? "png-stream-crop-fit" : Depth == 16 ? "png-stream16-decode" : "png-stream-decode", Path.Combine(state.DirectoryPath, "expected.raw"));
            RequireNativeOutput();
            before = native.Measure(smoke, sampleCount: sampleCount);
            ready = true;
        }
        catch { native?.Dispose(); state.Dispose(); throw; }
    }

    private void ValidateManaged()
    {
        using var firstStream = new MemoryStream(encoded, writable: false);
        using var secondStream = new MemoryStream(encoded, writable: false);
        if (firstStream.TryGetBuffer(out _) || secondStream.TryGetBuffer(out _))
            throw new InvalidOperationException("The input buffer must not be exposed by the stream.");
        using var first = DecodeAndTransform(firstStream); using var second = DecodeAndTransform(secondStream);
        if (first.Width != OutputWidth || first.Height != OutputHeight || first.BitDepth != Depth || second.BitDepth != Depth || first.Orientation != ImageOrientation.Normal)
            throw new InvalidOperationException("Output geometry/orientation differs.");
        if (!OriginalPixels.Bytes(first).AsSpan().SequenceEqual(expected) || !OriginalPixels.Bytes(second).AsSpan().SequenceEqual(expected))
            throw new InvalidOperationException("Stream decode pixels differ.");
        if (Depth == 8) first.GetRow8(0)[0] ^= 255; else first.GetRow16(0)[0] ^= 65535;
        if (!OriginalPixels.Bytes(second).AsSpan().SequenceEqual(expected)) throw new InvalidOperationException("Live decoded outputs alias.");
        if (!firstStream.CanRead || !secondStream.CanRead || firstStream.Position != encoded.Length || secondStream.Position != encoded.Length)
            throw new InvalidOperationException("Caller stream lifetime or consumed position differs.");
        if (JpegBenchmarkCase.Hash(encoded) != EncodedHash)
            throw new InvalidOperationException("Stream input changed.");
    }

    private ImageBuffer DecodeAndTransform(Stream stream)
    {
        if (!Compose) return ImageCodec.Decode(stream);
        using var decoded = ImageCodec.Decode(stream);
        using var oriented = ImageTransforms.AutoOrient(decoded);
        using var cropped = ImageTransforms.CropCentered(oriented, 256, 129);
        return ImageTransforms.ResizeToFit(cropped, 128, 64, allowUpscaling: false, ResizeFilter.Nearest);
    }

    private static byte[] ExpectedComposition(byte[] original)
    {
        // Centered crop starts at (128,65); fit ratio is 64/129, width rounds to127.
        var result = new byte[127 * 64 * 4];
        for (var y = 0; y < 64; y++)
        for (var x = 0; x < 127; x++)
        {
            var sx = 128 + (int)(((long)2 * x + 1) * 256 / (2 * 127));
            var sy = 65 + (int)(((long)2 * y + 1) * 129 / (2 * 64));
            original.AsSpan((sy * 513 + sx) * 4, 4).CopyTo(result.AsSpan((y * 127 + x) * 4, 4));
        }
        return result;
    }

    private void RequireNativeOutput()
    {
        var bytes = File.ReadAllBytes(state.NativeOutput);
        if (!bytes.AsSpan().SequenceEqual(expected))
            throw new InvalidOperationException("Native pixels differ from the independent coordinate reference.");
    }

    [Benchmark]
    public int Managed()
    {
        using var stream = new MemoryStream(encoded, writable: false);
        using var result = DecodeAndTransform(stream);
        return (Depth == 8 ? result.GetRow8(0)[0] : result.GetRow16(0)[0]) + result.Stride * result.Height;

    }

    [GlobalCleanup]
    public void Cleanup()
    {
        try
        {
            if (!ready) return;
            ValidateManaged();
            var after = native.Measure(smoke, before[0].Operations, sampleCount);
            native.Check(); RequireNativeOutput(); native.Stop();
            var directory = Path.Combine(state.Root, "artifacts", "benchmark-validation");
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, nameof(WarmStreamDecode) + "-" + Guid.NewGuid().ToString("N") + ".json");
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = nameof(WarmStreamDecode), caseId = Case.ToString(), smoke, nativeSampleCount = smoke ? 1 : sampleCount,
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"),
                width = HorizontalFlipInput.Width, height = HorizontalFlipInput.Height, depth = Depth, cpu, managedAffinityCount = 1,
                processorCount = Environment.ProcessorCount, serverGc = GCSettings.IsServerGC,
                gcConfiguration = GC.GetConfigurationVariables(), runtime = RuntimeInformation.FrameworkDescription,
                jitPolicy = "Tiered compilation disabled; fully optimized JIT before measurement",
                vector128 = System.Runtime.Intrinsics.Vector128.IsHardwareAccelerated,
                vector256 = System.Runtime.Intrinsics.Vector256.IsHardwareAccelerated,
                vector512 = System.Runtime.Intrinsics.Vector512.IsHardwareAccelerated,
                platform = RuntimeInformation.RuntimeIdentifier, sourceIdentity,
                inputSha256 = JpegBenchmarkCase.Hash(encoded), rasterSha256 = JpegBenchmarkCase.Hash(input), encodedInputBytes = encoded.Length, outputSha256 = JpegBenchmarkCase.Hash(expected),
                allRgbaSamplesExact = true, sourceUnchanged = true, independentOwnedOutput = true,
                nativeExitCode = 0, librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location),
                benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmStreamDecode).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready,
                nativeEndianConversionTimed = Depth == 16, composed = Compose, outputWidth = OutputWidth, outputHeight = OutputHeight,
                orientation = "Normal", cropWidth = Compose ? 256 : 0, cropHeight = Compose ? 129 : 0,
                fitMaxWidth = Compose ? 128 : 0, fitMaxHeight = Compose ? 64 : 0,
                freshSeekableStream = true, publicBufferExposed = false, inputCopyTimed = true,
                inputConsumedToEnd = true, callerStreamLeftOpen = true,
                nativeBefore = before, nativeAfter = after,
                boundary = (Compose ? "Composed Decode(Stream), AutoOrient(Normal), CropCentered(256,129), ResizeToFit(128,64,false,Nearest) gives 127x64. All managed intermediate owners are materialized and released. Native equivalent single-thread crop/point graph may retain intermediate views; final output is independently owned. Complete graph and pixel conversions are timed. " : "") + "Fresh read-only seekable in-memory Stream with non-exposed backing buffer to complete independently owned RGBA pixels. Native PNG16 includes fresh owned RGBA64LE acquisition and full BE-to-LE conversion. Managed MemoryStream creation, complete public Decode(Stream) reads/buffering/copies, full decode and input/output disposal included. Native fresh public AVIO context and 8192-byte scratch, length query, complete av_get_packet acquisition into fresh owned padded packet, EOF check, full PNG decode and release included; no prepared input view. Native codec/frame/packet wrapper reused, payload reacquired every operation. Original encoded source is resident; no file/network IO, IPC, startup, validation or hashing timed. Exact alpha/hidden RGB/source and two-live-output independence checked outside timing."

            }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
            Console.WriteLine("Validated warm memory pair; untimed evidence: " + path);
        }
        finally { native?.Dispose(); state?.Dispose(); ready = false; }
    }
}
