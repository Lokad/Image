using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

public enum WarmWebpLosslessCase { Encode8, Encode16, EncodeOpaque8 }

/// <summary>One complete VP8L encode on the existing original RGBA8/16 rasters.</summary>
[MemoryDiagnoser]
public class WarmWebpLossless
{
    public static IEnumerable<WarmWebpLosslessCase> Cases => SelectedCases();
    [ParamsSource(nameof(Cases))] public WarmWebpLosslessCase Case { get; set; }
    private int Depth => Case == WarmWebpLosslessCase.Encode16 ? 16 : 8;
    private bool Opaque => Case == WarmWebpLosslessCase.EncodeOpaque8;
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private ImageBuffer source = null!;
    private byte[] pixels = [], reference = [], managedEncoded = [], nativeEncoded = [];
    private NativeWarmSample[] before = [];
    private JsonElement sourceIdentity;
    private string originalRasterSha256 = "";
    private bool smoke, ready;
    private int cpu, sampleCount;
    private readonly WebpEncodeOptions options = new() { Lossless = true, Effort = 4 };

    public static WarmWebpLosslessCase[] SelectedCases()
        => Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CASES") switch
        {
            null or "Encode8" => [WarmWebpLosslessCase.Encode8],
            "Encode16" => [WarmWebpLosslessCase.Encode16],
            "EncodeOpaque8" => [WarmWebpLosslessCase.EncodeOpaque8],
            _ => throw new ArgumentException("Choose one lossless WebP encode case: Encode8, Encode16 or EncodeOpaque8.")
        };

    private static byte[] Normalize16(byte[] pixels)
    {
        var result = new byte[pixels.Length / 2];
        for (var i = 0; i < result.Length; i++)
            result[i] = (byte)((System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(pixels.AsSpan(i * 2)) + 128) / 257);
        return result;
    }

    private string NormalizedInput(string root)
    {
        var directory = Path.Combine(root, "artifacts", "warm-reference", "webp16-input");
        Directory.CreateDirectory(directory);
        Save("source.rgba", pixels); Save("normalized.rgba", reference);
        return Path.Combine(directory, "normalized.rgba");
        void Save(string name, byte[] bytes)
        {
            var path = Path.Combine(directory, name);
            if (File.Exists(path))
            {
                if (!File.ReadAllBytes(path).AsSpan().SequenceEqual(bytes))
                    throw new InvalidOperationException("Frozen source/normalized reference differs.");
            }
            else File.WriteAllBytes(path, bytes);
        }
    }

    [GlobalSetup]
    public void Setup()
    {
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("Use the dedicated Linux VM.");
        if (!Enum.IsDefined(Case)) throw new ArgumentOutOfRangeException(nameof(Case));
        cpu = int.Parse(Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CPU") ?? throw new InvalidOperationException("Missing warm CPU."));
        smoke = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "smoke";
        sampleCount = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "confirm" ? 12 : 3;
        using var process = Process.GetCurrentProcess();
        if (cpu is < 0 or > 62 || process.ProcessorAffinity.ToInt64() != (1L << cpu)
            || Environment.ProcessorCount != 1 || GCSettings.IsServerGC
            || Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") != "0"
            || !GC.GetConfigurationVariables().TryGetValue("ConcurrentGC", out var concurrent) || Convert.ToBoolean(concurrent))
            throw new InvalidOperationException("Lossless WebP requires one CPU, optimized JIT and non-concurrent workstation GC.");
        state = new JpegBenchmarkCase();
        try
        {
            using (var manifest = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "tmp/benchmark-source.json"))))
                sourceIdentity = JsonSerializer.SerializeToElement(new { sourceSha256 = manifest.RootElement.GetProperty("sourceSha256").GetString(), sourceRevision = manifest.RootElement.GetProperty("sourceRevision").GetString() });
            pixels = HorizontalFlipInput.Create(Depth);
            originalRasterSha256 = JpegBenchmarkCase.Hash(pixels);
            if (originalRasterSha256 != (Depth == 8 ? "70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80" : "3f861b61360b442c84063e7a6e1cb4f35957dcaa16f898048beaa36bbf4272f9"))
                throw new InvalidOperationException("Original raster differs.");
            if (Opaque)
                for (var i = 3; i < pixels.Length; i += 4) pixels[i] = 255;
            reference = Depth == 8 ? pixels : Normalize16(pixels);
            source = JpegBenchmarkCase.Buffer(pixels, 513, 259, Depth); File.WriteAllBytes(state.Input, pixels);
            ValidateManaged();
            native = new NativeWarmReference(state, Depth, cpu, "webp-lossless-encode", expectedRaw: Depth == 16 ? NormalizedInput(state.Root) : null, nativeQuality: 75);
            ValidateNative(); before = native.Measure(smoke, sampleCount: sampleCount); ready = true;
        }
        catch { native?.Dispose(); source?.Dispose(); state.Dispose(); throw; }
    }

    private byte[] EncodeManaged()
    {
        using var output = new MemoryStream(); ImageCodec.EncodeWebp(source, output, options);
        return output.ToArray(); // Validation only; timed operations keep the owned stream.
    }
    private void ValidatePixels(ImageBuffer image)
    {
        if ((image.Width, image.Height, image.BitDepth, image.Orientation) != (513, 259, 8, ImageOrientation.Normal)
            || !OriginalPixels.Bytes(image).AsSpan().SequenceEqual(reference))
            throw new InvalidOperationException("Lossless RGBA, alpha or hidden RGB differs.");
    }
    private void ValidateBitstream(byte[] encoded)
    {
        // RFC 9649 RIFF/VP8L framing; Identify independently checks the container.
        var info = ImageCodec.Identify(encoded);
        if ((info.Format, info.Width, info.Height, info.BitDepth) != (ImageFormat.WebP, 513, 259, 8))
            throw new InvalidOperationException("Lossless WebP container geometry differs.");
        var position = 12; var payloads = 0;
        while (position < encoded.Length)
        {
            var length = checked((int)System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(encoded.AsSpan(position + 4)));
            if (encoded.AsSpan(position, 4).SequenceEqual("VP8L"u8)) payloads++;
            else if (!encoded.AsSpan(position, 4).SequenceEqual("VP8X"u8))
                throw new InvalidOperationException("Expected only still VP8L/container chunks.");
            position = checked(position + 8 + length + (length & 1));
        }
        if (payloads != 1 || position != encoded.Length) throw new InvalidOperationException("Missing VP8L payload.");
        using var decoded = ImageCodec.Decode(encoded); ValidatePixels(decoded);
        foreach (var scalar in new[] { false, true })
        {
            var arguments = new List<string> { "-v", "error", "-threads", "1", "-filter_threads", "1", "-noautorotate" };
            if (scalar) arguments.AddRange(["-cpuflags", "0"]);
            arguments.AddRange(["-f", "image2pipe", "-i", "pipe:0", "-frames:v", "1", "-threads:v", "1", "-thread_type:v", "0", "-pix_fmt", "rgba", "-f", "rawvideo", "pipe:1"]);
            if (!OracleProcess.Run(state.Ffmpeg, arguments.ToArray(), encoded).AsSpan().SequenceEqual(reference))
                throw new InvalidOperationException("Normal/scalar native VP8L pixels differ.");
        }
    }
    private void ValidateManaged()
    {
        var first = EncodeManaged(); var second = EncodeManaged();
        if (!first.AsSpan().SequenceEqual(second) || (managedEncoded.Length != 0 && !first.AsSpan().SequenceEqual(managedEncoded)))
            throw new InvalidOperationException("Managed bitstream changed.");
        managedEncoded = second; ValidateBitstream(second); first[0] ^= 255;
        if (!second.AsSpan().SequenceEqual(EncodeManaged()) || !OriginalPixels.Bytes(source).AsSpan().SequenceEqual(pixels))
            throw new InvalidOperationException("Encoded outputs alias or source changed.");
    }
    private void ValidateNative()
    {
        var encoded = File.ReadAllBytes(state.NativeOutput);
        if (nativeEncoded.Length != 0 && !encoded.AsSpan().SequenceEqual(nativeEncoded))
            throw new InvalidOperationException("Native bitstream changed.");
        nativeEncoded = encoded; ValidateBitstream(encoded);
        if (!File.ReadAllBytes(state.Input).AsSpan().SequenceEqual(pixels)) throw new InvalidOperationException("Native source changed.");
    }
    [Benchmark]
    public int Managed()
    {
        using var output = new MemoryStream(); ImageCodec.EncodeWebp(source, output, options);
        return checked((int)output.Length) ^ output.GetBuffer()[checked((int)output.Length) / 2];
    }
    [GlobalCleanup]
    public void Cleanup()
    {
        try
        {
            if (!ready) return;
            ValidateManaged(); var after = native.Measure(smoke, before[0].Operations, sampleCount);
            native.Check(); ValidateNative(); native.Stop();
            var path = Path.Combine(state.Root, "artifacts/benchmark-validation", nameof(WarmWebpLossless) + "-" + Guid.NewGuid().ToString("N") + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = nameof(WarmWebpLossless), caseId = Case.ToString(), smoke,
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"), width = 513, height = 259, depth = Depth, encodedDepth = 8,
                cpu, managedAffinityCount = 1, processorCount = Environment.ProcessorCount, serverGc = GCSettings.IsServerGC,
                gcConfiguration = GC.GetConfigurationVariables(), runtime = RuntimeInformation.FrameworkDescription, platform = RuntimeInformation.RuntimeIdentifier,
                sourceIdentity, codec = "VP8L", lossless = true, effort = 4, nativeCompressionQuality = 75, nativeExactRgb = true, baselineBackend = "libwebp exact=1 (FFmpeg encoder backend)",
                originalRasterSha256, sourceOpaque = Opaque,
                rasterRecipe = Opaque ? "Existing original RGBA8 RGB unchanged; alpha set to255. One explicit opacity projection, no new original raster or setting grid." : "Existing original RGBA raster unchanged.",
                inputSha256 = JpegBenchmarkCase.Hash(pixels), rasterSha256 = JpegBenchmarkCase.Hash(pixels), referenceSha256 = JpegBenchmarkCase.Hash(reference),
                fullSourceNormalizationTimed = Depth == 16, normalizedReferenceExact = true,
                managedOutputBytes = managedEncoded.Length, nativeOutputBytes = nativeEncoded.Length,
                managedOutputSha256 = JpegBenchmarkCase.Hash(managedEncoded), nativeOutputSha256 = JpegBenchmarkCase.Hash(nativeEncoded),
                allRgbaSamplesExact = true, alphaExact = true, hiddenRgbExact = true, nativeCliPixelsExact = true,
                nativeCliNormalScalarPixelsExact = true, bitstreamSyntaxVerified = true,
                sourceUnchanged = true, independentOwnedOutput = true, nativeExitCode = 0,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location), benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmWebpLossless).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after,
                boundary = $"One resident original 513x259 RGBA{Depth} raster{(Opaque ? " with RGB unchanged and alpha255" : "")} to complete independently owned VP8L. Public managed EncodeWebp, admission, fresh MemoryStream creation/growth and disposal; no redundant ToArray. Native libwebp exact=1 (FFmpeg encoder backend), lossless1, effort4, quality75 fixed compression control; Full channel normalization and RGBA-to-BGRA full-frame repacking inside every call, fresh pixel import/allocation, encode and independently owned packet acquisition/release. Codec/input/frame/packet wrappers and BGRA workspace reused; every pixel reprocessed. Exact RGBA/alpha/hidden RGB required. Equal pixels do not imply equal compression ratios; output sizes recorded. Startup, I/O, IPC, preparation, hashing and validation excluded. One CPU/codec/conversion thread, normal SIMD. No source/quality/effort/profile enumeration."
            }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
            if (Opaque)
            {
                var directory = Path.Combine(state.Root, "artifacts", "warm-reference", "webp-lossless-opaque8");
                Directory.CreateDirectory(directory);
                var input = Path.Combine(directory, "source.rgba");
                if (File.Exists(input) && !File.ReadAllBytes(input).AsSpan().SequenceEqual(pixels))
                    throw new InvalidOperationException("Frozen opaque source differs.");
                File.WriteAllBytes(input, pixels);
                File.WriteAllBytes(Path.Combine(directory, "managed.webp"), managedEncoded);
                File.WriteAllBytes(Path.Combine(directory, "native.webp"), nativeEncoded);
            }
        }
        finally { native?.Dispose(); source?.Dispose(); state?.Dispose(); ready = false; }
    }
}
