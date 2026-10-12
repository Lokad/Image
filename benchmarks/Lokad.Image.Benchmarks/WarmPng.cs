using System.Buffers.Binary;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

public enum WarmPngCase { Encode8, Decode8, Decode16, Encode16, DecodeAdam7_16, DecodePaeth16 }

[MemoryDiagnoser]
public class WarmPng
{
    public static IEnumerable<WarmPngCase> Cases => SelectedCases();
    [ParamsSource(nameof(Cases))] public WarmPngCase Case { get; set; }
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private ImageBuffer source = null!;
    private byte[] pixels = [], encoded = [];
    private NativeWarmSample[] before = [];
    private JsonElement sourceIdentity;
    private bool smoke, ready;
    private int cpu, sampleCount, managedOutputBytes, nativeOutputBytes;
    private readonly PngEncodeOptions encodeOptions = new() { ColorType = PngColorType.Rgba, Compression = CompressionLevel.Optimal };
    public static WarmPngCase[] SelectedCases()
    {
        var selection = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CASES");
        var result = selection is null ? new[] { WarmPngCase.Decode8 } : selection.Split(',')
            .Select(value => Enum.TryParse<WarmPngCase>(value, out var parsed) && Enum.IsDefined(parsed)
                ? parsed : throw new ArgumentException("Choose Encode8, Encode16, Decode8, Decode16, DecodeAdam7_16 or DecodePaeth16.")).ToArray();
        if (result.Length is < 1 or > 3 || result.Distinct().Count() != result.Length)
            throw new ArgumentException("Choose one to three distinct PNG cases.");
        return result;
    }
    private int Depth => Case is WarmPngCase.Decode16 or WarmPngCase.Encode16 or WarmPngCase.DecodeAdam7_16 or WarmPngCase.DecodePaeth16 ? 16 : 8;
    private bool Encode => Case is WarmPngCase.Encode8 or WarmPngCase.Encode16;
    private bool Interlaced => Case == WarmPngCase.DecodeAdam7_16;
    private bool Paeth => Case == WarmPngCase.DecodePaeth16;
    private bool Prepared => Interlaced || Paeth;

    [GlobalSetup]
    public void Setup()
    {
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("Warm native PNG runs require Linux.");
        cpu = int.Parse(Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CPU") ?? throw new InvalidOperationException("Missing warm CPU."));
        smoke = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "smoke";
        sampleCount = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "confirm" ? 12 : 3;
        using var process = Process.GetCurrentProcess();
        if (cpu is < 0 or > 62 || process.ProcessorAffinity.ToInt64() != (1L << cpu) || Environment.ProcessorCount != 1
            || GCSettings.IsServerGC || Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") != "0"
            || !GC.GetConfigurationVariables().TryGetValue("ConcurrentGC", out var concurrent) || Convert.ToBoolean(concurrent))
            throw new InvalidOperationException("PNG worker requires one CPU, optimized JIT and non-concurrent workstation GC.");
        state = new JpegBenchmarkCase(Encode ? ".png" : "");
        try
        {
            using (var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(state.Root, "tmp", "benchmark-source.json"))))
                sourceIdentity = JsonSerializer.SerializeToElement(new
                {
                    sourceSha256 = manifest.RootElement.GetProperty("sourceSha256").GetString(),
                    sourceRevision = manifest.RootElement.GetProperty("sourceRevision").GetString()
                });
            pixels = HorizontalFlipInput.Create(Depth);
            source = JpegBenchmarkCase.Buffer(pixels, HorizontalFlipInput.Width, HorizontalFlipInput.Height, Depth);
            if (Prepared)
            {
                var directory = Path.Combine(state.Root, "artifacts", "warm-reference", Paeth ? "png-paeth-16" : "png-adam7-16");
                using var prepared = JsonDocument.Parse(File.ReadAllBytes(directory + ".json"));
                encoded = File.ReadAllBytes(Path.Combine(directory, "input.png"));
                if (prepared.RootElement.GetProperty("inputSha256").GetString() != JpegBenchmarkCase.Hash(encoded)
                    || prepared.RootElement.GetProperty("rasterSha256").GetString() != JpegBenchmarkCase.Hash(pixels)
                    || !File.ReadAllBytes(Path.Combine(directory, "expected.raw")).AsSpan().SequenceEqual(pixels))
                    throw new InvalidOperationException("Prepared PNG input/raster differs.");
            }
            else using (var output = new MemoryStream())
            {
                ImageCodec.EncodePng(source, output, encodeOptions); encoded = output.ToArray();
            }
            ValidatePng(encoded); ValidateManaged();
            var rawPath = Path.Combine(state.DirectoryPath, "expected.raw");
            File.WriteAllBytes(rawPath, pixels); File.WriteAllBytes(state.Input, Encode ? pixels : encoded);
            native = new NativeWarmReference(state, Depth, cpu, Encode ? "png-encode" : Interlaced ? "png-adam7-decode" : "png-decode", rawPath);
            ValidateNative();
            if (Prepared)
            {
                var arguments = new List<string> { "-v", "error", "-threads", "1", "-filter_threads", "1", "-cpuflags", "0",
                    "-noautorotate", "-f", "image2pipe", "-i", "pipe:0", "-frames:v", "1", "-threads:v", "1",
                    "-pix_fmt", "rgba64le", "-f", "rawvideo", "pipe:1" };
                if (!OracleProcess.Run(state.Ffmpeg, arguments.ToArray(), encoded).AsSpan().SequenceEqual(pixels))
                    throw new InvalidOperationException("Scalar native CLI prepared PNG samples differ.");
                arguments.RemoveRange(arguments.IndexOf("-cpuflags"), 2);
                if (!OracleProcess.Run(state.Ffmpeg, arguments.ToArray(), encoded).AsSpan().SequenceEqual(pixels))
                    throw new InvalidOperationException("Normal native CLI prepared PNG samples differ.");
            }
            if (Encode && Depth == 16)
            {
                var arguments = new List<string> { "-v", "error", "-threads", "1", "-filter_threads", "1", "-cpuflags", "0",
                    "-f", "rawvideo", "-pix_fmt", "rgba64le", "-s", "513x259", "-i", "pipe:0",
                    "-frames:v", "1", "-c:v", "png", "-threads", "1", "-compression_level", "6", "-pred", "none",
                    "-pix_fmt", "rgba64be", "-f", "image2pipe", "pipe:1" };
                var scalar = OracleProcess.Run(state.Ffmpeg, arguments.ToArray(), pixels);
                ValidatePng(scalar);
                using (var decoded = ImageCodec.Decode(scalar)) ValidatePixels(decoded);
                arguments.RemoveRange(arguments.IndexOf("-cpuflags"), 2);
                var normal = OracleProcess.Run(state.Ffmpeg, arguments.ToArray(), pixels);
                ValidatePng(normal);
                using (var decoded = ImageCodec.Decode(normal)) ValidatePixels(decoded);
            }
            before = native.Measure(smoke, sampleCount: sampleCount);
            ready = true;
        }
        catch { native?.Dispose(); source?.Dispose(); state.Dispose(); throw; }
    }

    private void ValidatePixels(ImageBuffer value)
    {
        if ((value.Width, value.Height, value.BitDepth, value.Orientation) != (source.Width, source.Height, Depth, ImageOrientation.Normal)
            || !OriginalPixels.Bytes(value).AsSpan().SequenceEqual(pixels))
            throw new InvalidOperationException("PNG samples/precision/orientation differ, including zero-alpha hidden RGB.");
    }
    private void ValidateManaged()
    {
        if (Encode)
        {
            using var first = new MemoryStream(); using var second = new MemoryStream();
            ImageCodec.EncodePng(source, first, encodeOptions); ImageCodec.EncodePng(source, second, encodeOptions);
            if (ReferenceEquals(first.GetBuffer(), second.GetBuffer()) || !first.ToArray().AsSpan().SequenceEqual(encoded)
                || !second.ToArray().AsSpan().SequenceEqual(encoded)) throw new InvalidOperationException("Managed PNG results are not independent/deterministic.");
            managedOutputBytes = checked((int)first.Length);
        }
        else
        {
            using var first = ImageCodec.Decode(encoded); using var second = ImageCodec.Decode(encoded);
            ValidatePixels(first); ValidatePixels(second);
            if (Depth == 8) first.GetRow8(0)[0] ^= 255; else first.GetRow16(0)[0] ^= 65535;
            ValidatePixels(second); managedOutputBytes = pixels.Length;
        }
        if (!OriginalPixels.Bytes(source).AsSpan().SequenceEqual(pixels)) throw new InvalidOperationException("Managed source changed.");
    }
    private void ValidateNative()
    {
        var output = File.ReadAllBytes(state.NativeOutput); nativeOutputBytes = output.Length;
        if (Encode)
        {
            ValidatePng(output);
            using var decoded = ImageCodec.Decode(output); ValidatePixels(decoded);
            // Pin the same level/filter and retain size evidence. A large payload
            // discrepancy is a preparation mismatch rather than a scored speed win.
            var nativeIdat = IdatBytes(output); var managedIdat = IdatBytes(encoded);
            if (Math.Abs(nativeIdat - (double)managedIdat) / Math.Max(nativeIdat, managedIdat) > 0.05)
                throw new InvalidOperationException("PNG compressed payload sizes differ by more than 5%; resolve effort matching before timing.");
        }
        else if (!output.AsSpan().SequenceEqual(pixels)) throw new InvalidOperationException("Native owned RGBA pixels differ.");
    }
    private static int IdatBytes(byte[] bytes)
    {
        var total = 0;
        for (var position = 8; position < bytes.Length;)
        {
            var length = checked((int)BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(position)));
            if (bytes.AsSpan(position + 4, 4).SequenceEqual("IDAT"u8)) total = checked(total + length);
            position = checked(position + length + 12);
        }
        return total;
    }
    private void ValidatePng(byte[] bytes)
    {
        var info = ImageCodec.Identify(bytes);
        if (info.Format != ImageFormat.Png || info.Width != source.Width || info.Height != source.Height || info.BitDepth != Depth
            || bytes[25] != 6 || bytes[28] != (Interlaced ? 1 : 0)) throw new InvalidOperationException("PNG profile differs.");
        using var compressed = new MemoryStream();
        for (var position = 8; position < bytes.Length;)
        {
            var length = checked((int)BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(position)));
            if (bytes.AsSpan(position + 4, 4).SequenceEqual("IDAT"u8)) compressed.Write(bytes.AsSpan(position + 8, length));
            position = checked(position + length + 12);
        }
        compressed.Position = 0;
        using var inflater = new ZLibStream(compressed, CompressionMode.Decompress);
        foreach (var (startX, startY, stepX, stepY) in Interlaced ? WarmPngAdam7Input.Passes : [(0, 0, 1, 1)])
        {
            var bytesPerPixel = Depth / 8 * 4;
            var row = new byte[(source.Width - startX + stepX - 1) / stepX * bytesPerPixel];
            for (var y = startY; y < source.Height; y += stepY)
            {
                if (inflater.ReadByte() != (Paeth ? 4 : 0)) throw new InvalidOperationException("PNG row filter differs from the fixed profile.");
                inflater.ReadExactly(row);
                for (var x = startX; x < source.Width; x += stepX)
                for (var i = 0; i < bytesPerPixel; i++)
                {
                    var original = pixels[y * source.Stride + x * bytesPerPixel + (Depth == 16 ? i ^ 1 : i)];
                    var prediction = 0;
                    if (Paeth)
                    {
                        var offset = x * bytesPerPixel + i;
                        int ByteAt(int rowY, int byteX) => rowY < 0 || byteX < 0 ? 0 : pixels[rowY * source.Stride + (byteX ^ 1)];
                        prediction = WarmPngPaethInput.Predict(ByteAt(y, offset - bytesPerPixel), ByteAt(y - 1, offset), ByteAt(y - 1, offset - bytesPerPixel));
                    }
                    if (row[(x - startX) / stepX * bytesPerPixel + i] != unchecked((byte)(original - prediction)))
                        throw new InvalidOperationException("PNG source/pass samples differ.");
                }
            }
        }
        if (inflater.ReadByte() != -1) throw new InvalidOperationException("PNG filtered length differs.");
    }

    [Benchmark]
    public int Managed()
    {
        if (Encode)
        {
            using var output = new MemoryStream(); ImageCodec.EncodePng(source, output, encodeOptions);
            return checked((int)output.Length) ^ output.GetBuffer()[checked((int)output.Length) / 2];
        }
        using var result = ImageCodec.Decode(encoded);
        return result.Stride * result.Height ^ (Depth == 8 ? result.GetRow8(0)[source.Stride / 2] : result.GetRow16(0)[source.Stride / 4]);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        try
        {
            if (!ready) return;
            ValidateManaged();
            var after = native.Measure(smoke, before[0].Operations, sampleCount);
            native.Check(); ValidateNative(); native.Stop();
            var path = Path.Combine(state.Root, "artifacts", "benchmark-validation", nameof(WarmPng) + "-" + Guid.NewGuid().ToString("N") + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = nameof(WarmPng), caseId = Case.ToString(), smoke, nativeSampleCount = smoke ? 1 : sampleCount,
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"),
                width = source.Width, height = source.Height, depth = Depth, cpu, managedAffinityCount = 1,
                processorCount = Environment.ProcessorCount, serverGc = GCSettings.IsServerGC, gcConfiguration = GC.GetConfigurationVariables(),
                runtime = RuntimeInformation.FrameworkDescription, platform = RuntimeInformation.RuntimeIdentifier,
                jitPolicy = "Tiered compilation disabled; fully optimized JIT before measurement", sourceIdentity,
                inputSha256 = JpegBenchmarkCase.Hash(Encode ? pixels : encoded), rasterSha256 = JpegBenchmarkCase.Hash(pixels),
                managedOutputBytes, nativeOutputBytes, managedEncodedSha256 = JpegBenchmarkCase.Hash(encoded),
                nativeOutputSha256 = JpegBenchmarkCase.HashFile(state.NativeOutput),
                interlace = Interlaced ? 1 : 0,
                filter = Paeth ? 4 : 0,
                compression = Paeth ? "Authored noninterlaced PNG RGBA16; every row Paeth; BCL Optimal level 6; identical resident encoded input"
                    : Interlaced ? "Authored PNG Adam7 RGBA16; BCL Optimal level 6; filter None; identical resident encoded input"
                    : "BCL Optimal (zlib default level 6) / FFmpeg level 6; filter None; noninterlaced RGBA",
                managedIdatBytes = IdatBytes(encoded), nativeIdatBytes = Encode ? IdatBytes(File.ReadAllBytes(state.NativeOutput)) : IdatBytes(encoded),
                allRgbaSamplesExact = true, alphaExact = true, hiddenRgbExact = true,
                nativeCliNormalScalarPixelsExact = Encode && Depth == 16 || Prepared,
                nativeEndianRepackingTimed = Encode && Depth == 16, nativeEndianWorkspaceReused = Encode && Depth == 16,
                nativeSampleTargetMilliseconds = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_NATIVE_SAMPLE_MS") ?? "200",
                sourceUnchanged = true, independentOwnedOutput = true, nativeExitCode = 0,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location),
                benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmPng).Assembly.Location), nativeBuild = native.BuildIdentity,
                nativeReady = native.Ready, nativeBefore = before, nativeAfter = after,
                boundary = Encode
                    ? "Warm RGBA input to complete owned PNG: fresh managed MemoryStream (creation, growth, encode and disposal included; no redundant ToArray); native codec context/packet wrapper reused, real independently owned packet acquisition, encode and release included. Both level 6/filter None. For RGBA16, native LE-to-BE full-frame sample conversion runs inside every operation, using a reused prepared frame; managed conversion/row storage is included in public EncodePng. IPC, file IO, hashes and validation excluded."
                    : "Warm identical PNG bytes to independent owned RGBA pixels: public managed Decode/allocation/disposal; native codec/input packet/frame wrappers reused, complete decode/owned output/release. RGBA16 BE-to-LE conversion and second output allocation included natively; no conversion for RGBA8. Native padded rows exposed directly; packing/file IO/IPC/hashes excluded."
            }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
            Console.WriteLine("Validated warm PNG pair: " + path);
        }
        finally { native?.Dispose(); source?.Dispose(); state?.Dispose(); ready = false; }
    }
}
