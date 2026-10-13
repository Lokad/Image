using System.Buffers.Binary;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

public enum WarmPngCase { Encode8, Decode8, Decode16, Encode16, DecodeAdam7_16, DecodePaeth16, DecodePaeth8, DecodeRgb16, DecodeGray8, DecodeGrayAlpha8, DecodeRgb8, DecodeGray16, DecodeGrayAlpha16, DecodePalette8, DecodeGray1, DecodeGray4, DecodePalette4, DecodePalette2, DecodePalette1, DecodeGray2, DecodeUp8, DecodeSub8, DecodeAverage8, EncodeRgb8, EncodeRgb16, EncodeGray8, EncodeGray16 }

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
    private PngEncodeOptions encodeOptions => new() { ColorType = GrayAlpha ? PngColorType.GrayscaleAlpha : Gray ? PngColorType.Grayscale : Rgb ? PngColorType.Rgb : PngColorType.Rgba, Compression = CompressionLevel.Optimal };
    public static WarmPngCase[] SelectedCases()
    {
        var selection = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CASES");
        var result = selection is null ? new[] { WarmPngCase.Decode8 } : selection.Split(',')
            .Select(value => Enum.TryParse<WarmPngCase>(value, out var parsed) && Enum.IsDefined(parsed)
                ? parsed : throw new ArgumentException("Choose Encode8, Encode16, EncodeRgb8, EncodeRgb16, EncodeGray8, EncodeGray16, Decode8, Decode16, DecodeAdam7_16, DecodePaeth16, DecodePaeth8, DecodeUp8, DecodeSub8, DecodeAverage8, DecodeRgb16, DecodeGray8, DecodeGrayAlpha8, DecodeRgb8, DecodeGray16, DecodeGrayAlpha16, DecodePalette8, DecodeGray1, DecodeGray2, DecodeGray4, DecodePalette1, DecodePalette2 or DecodePalette4.")).ToArray();
        if (result.Length is < 1 or > 3 || result.Distinct().Count() != result.Length)
            throw new ArgumentException("Choose one to three distinct PNG cases.");
        return result;
    }
    private int Depth => Case is WarmPngCase.EncodeGray16 or WarmPngCase.Decode16 or WarmPngCase.Encode16 or WarmPngCase.EncodeRgb16 or WarmPngCase.DecodeAdam7_16 or WarmPngCase.DecodePaeth16 or WarmPngCase.DecodeRgb16 or WarmPngCase.DecodeGray16 or WarmPngCase.DecodeGrayAlpha16 ? 16 : 8;
    private bool Rgb => Case is WarmPngCase.DecodeRgb16 or WarmPngCase.DecodeRgb8 or WarmPngCase.EncodeRgb8 or WarmPngCase.EncodeRgb16;
    private bool Mono => Case is WarmPngCase.DecodeGray1 or WarmPngCase.DecodeGray2 or WarmPngCase.DecodeGray4;
    private int CodedDepth => Case is WarmPngCase.DecodeGray4 or WarmPngCase.DecodePalette4 ? 4 : Case is WarmPngCase.DecodePalette2 or WarmPngCase.DecodeGray2 ? 2 : Mono || Case == WarmPngCase.DecodePalette1 ? 1 : Depth;
    private bool Gray => Case is WarmPngCase.DecodeGray8 or WarmPngCase.DecodeGray16 or WarmPngCase.DecodeGray1 or WarmPngCase.DecodeGray2 or WarmPngCase.DecodeGray4 or WarmPngCase.EncodeGray8 or WarmPngCase.EncodeGray16;
    private bool GrayAlpha => Case is WarmPngCase.DecodeGrayAlpha8 or WarmPngCase.DecodeGrayAlpha16;
    private bool Palette => Case is WarmPngCase.DecodePalette8 or WarmPngCase.DecodePalette4 or WarmPngCase.DecodePalette2 or WarmPngCase.DecodePalette1;
    private int ColorType => Palette ? 3 : GrayAlpha ? 4 : Gray ? 0 : Rgb ? 2 : 6;
    private bool Encode => Case is WarmPngCase.Encode8 or WarmPngCase.Encode16 or WarmPngCase.EncodeRgb8 or WarmPngCase.EncodeRgb16 or WarmPngCase.EncodeGray8 or WarmPngCase.EncodeGray16;
    private bool Interlaced => Case == WarmPngCase.DecodeAdam7_16;
    private bool Paeth => Case is WarmPngCase.DecodePaeth16 or WarmPngCase.DecodePaeth8;
    private bool Up => Case == WarmPngCase.DecodeUp8;
    private bool Sub => Case == WarmPngCase.DecodeSub8;
    private bool Average => Case == WarmPngCase.DecodeAverage8;
    private int Filter => Paeth ? 4 : Up ? 2 : Sub ? 1 : Average ? 3 : 0;
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
            if (Mono) WarmPngMonoInput.Project(pixels, CodedDepth);
            if (Palette) WarmPngPaletteInput.Project(pixels, CodedDepth);
            if (Rgb)
                for (var i = 3 * Depth / 8; i < pixels.Length; i += 4 * Depth / 8)
                    pixels.AsSpan(i, Depth / 8).Fill(255);
            if (Gray || GrayAlpha)
                for (var i = 0; i < pixels.Length; i += 4 * Depth / 8)
                {
                    var gray = pixels.AsSpan(i, Depth / 8);
                    gray.CopyTo(pixels.AsSpan(i + Depth / 8, Depth / 8));
                    gray.CopyTo(pixels.AsSpan(i + 2 * Depth / 8, Depth / 8));
                    if (Gray) pixels.AsSpan(i + 3 * Depth / 8, Depth / 8).Fill(255);
                }
            source = JpegBenchmarkCase.Buffer(pixels, HorizontalFlipInput.Width, HorizontalFlipInput.Height, Depth);
            if (Prepared)
            {
                var directory = Path.Combine(state.Root, "artifacts", "warm-reference", Paeth ? "png-paeth-" + Depth : "png-adam7-16");
                using var prepared = JsonDocument.Parse(File.ReadAllBytes(directory + ".json"));
                encoded = File.ReadAllBytes(Path.Combine(directory, "input.png"));
                if (prepared.RootElement.GetProperty("inputSha256").GetString() != JpegBenchmarkCase.Hash(encoded)
                    || prepared.RootElement.GetProperty("rasterSha256").GetString() != JpegBenchmarkCase.Hash(pixels)
                    || !File.ReadAllBytes(Path.Combine(directory, "expected.raw")).AsSpan().SequenceEqual(pixels))
                    throw new InvalidOperationException("Prepared PNG input/raster differs.");
            }
            else if (Up) encoded = WarmPngUpInput.Build(pixels);
            else if (Sub) encoded = WarmPngSubInput.Build(pixels);
            else if (Average) encoded = WarmPngAverageInput.Build(pixels);
            else if (Mono) encoded = WarmPngMonoInput.Build(pixels, CodedDepth);
            else if (Palette) encoded = WarmPngPaletteInput.Build(pixels, CodedDepth);
            else using (var output = new MemoryStream())
            {
                ImageCodec.EncodePng(source, output, encodeOptions); encoded = output.ToArray();
            }
            ValidatePng(encoded); ValidateManaged();
            if (Rgb || Gray || GrayAlpha || Palette || Up || Sub || Average)
            {
                var directory = Path.Combine(state.Root, "artifacts", "warm-reference", Up ? "png-up-8" : Sub ? "png-sub-8" : Average ? "png-average-8" : Mono ? "png-gray" + CodedDepth : Palette ? "png-palette" + CodedDepth : GrayAlpha ? "png-gray-alpha" + Depth : Gray ? "png-gray" + Depth : "png-rgb" + Depth);
                Directory.CreateDirectory(directory);
                foreach (var (name, bytes) in new[] { ("input.png", encoded), ("expected.raw", pixels) })
                {
                    var path = Path.Combine(directory, name);
                    if (File.Exists(path))
                    {
                        if (!File.ReadAllBytes(path).AsSpan().SequenceEqual(bytes))
                            throw new InvalidOperationException("Frozen projected PNG input differs; preserve it before changing preparation.");
                    }
                    else File.WriteAllBytes(path, bytes);
                }
            }
            var rawPath = Path.Combine(state.DirectoryPath, "expected.raw");
            File.WriteAllBytes(rawPath, pixels); File.WriteAllBytes(state.Input, Encode ? pixels : encoded);
            native = new NativeWarmReference(state, Depth, cpu, Encode ? Rgb ? "png-rgb-encode" : Gray ? "png-gray-encode" : "png-encode" : Interlaced ? "png-adam7-decode" : "png-decode", rawPath);
            if (native.Ready.GetProperty("colorType").GetInt32() != ColorType
                || native.Ready.GetProperty("codedDepth").GetInt32() != CodedDepth
                || native.Ready.GetProperty("conversionThreads").GetInt32() != (Rgb || Gray || GrayAlpha || Palette ? 1 : 0)
                || native.Ready.GetProperty("rgbExpansionTimed").GetBoolean() != (Rgb && !Encode)
                || native.Ready.GetProperty("rgbPackingTimed").GetBoolean() != (Rgb && Encode)
                || native.Ready.GetProperty("grayExpansionTimed").GetBoolean() != (Gray && !Encode)
                || native.Ready.GetProperty("grayPackingTimed").GetBoolean() != (Gray && Encode)
                || native.Ready.GetProperty("grayPlaneExtractionTimed").GetBoolean() != (Gray && Encode && Depth == 16)
                || native.Ready.GetProperty("grayFilterOutputAllocationTimed").GetBoolean() != (Gray && Encode && Depth == 16)
                || native.Ready.GetProperty("grayAlphaExpansionTimed").GetBoolean() != GrayAlpha
                || native.Ready.GetProperty("paletteExpansionTimed").GetBoolean() != Palette)
                throw new InvalidOperationException("Native PNG color/expansion boundary differs.");
            ValidateNative();
            if (!Encode)
            {
                var arguments = new List<string> { "-v", "error", "-threads", "1", "-filter_threads", "1", "-cpuflags", "0",
                    "-noautorotate", "-f", "image2pipe", "-i", "pipe:0", "-frames:v", "1", "-threads:v", "1",
                    "-pix_fmt", Depth == 16 ? "rgba64le" : "rgba", "-f", "rawvideo", "pipe:1" };
                if (!OracleProcess.Run(state.Ffmpeg, arguments.ToArray(), encoded).AsSpan().SequenceEqual(pixels))
                    throw new InvalidOperationException("Scalar native CLI PNG samples differ.");
                arguments.RemoveRange(arguments.IndexOf("-cpuflags"), 2);
                if (!OracleProcess.Run(state.Ffmpeg, arguments.ToArray(), encoded).AsSpan().SequenceEqual(pixels))
                    throw new InvalidOperationException("Normal native CLI PNG samples differ.");
            }
            if (Encode)
            {
                var arguments = new List<string> { "-v", "error", "-threads", "1", "-filter_threads", "1", "-cpuflags", "0",
                    "-f", "rawvideo", "-pix_fmt", Depth == 16 ? "rgba64le" : "rgba", "-s", "513x259", "-i", "pipe:0",
                    "-frames:v", "1", "-c:v", "png", "-threads", "1", "-compression_level", "6", "-pred", "none",
                    "-pix_fmt", Gray ? Depth == 16 ? "gray16be" : "gray" : Rgb ? Depth == 16 ? "rgb48be" : "rgb24" : Depth == 16 ? "rgba64be" : "rgba", "-f", "image2pipe", "pipe:1" };
                if (Gray && Depth == 16) arguments.InsertRange(arguments.IndexOf("-frames:v"), ["-vf", "extractplanes=r,scale=iw:ih:flags=neighbor+bitexact:threads=1,format=gray16be"]);
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
        if (info.Format != ImageFormat.Png || info.Width != source.Width || info.Height != source.Height || info.BitDepth != CodedDepth
            || bytes[25] != ColorType || bytes[28] != (Interlaced ? 1 : 0)) throw new InvalidOperationException("PNG profile differs.");
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
            var bytesPerPixel = Depth / 8 * (Palette || Gray ? 1 : GrayAlpha ? 2 : Rgb ? 3 : 4);
            var passWidth = (source.Width - startX + stepX - 1) / stepX;
            var packed = CodedDepth < 8;
            var row = new byte[packed ? (passWidth * CodedDepth + 7) / 8 : passWidth * bytesPerPixel];
            for (var y = startY; y < source.Height; y += stepY)
            {
                if (inflater.ReadByte() != Filter) throw new InvalidOperationException("PNG row filter differs from the fixed profile.");
                inflater.ReadExactly(row);
                if (packed)
                {
                    var maximum = (1 << CodedDepth) - 1;
                    for (var x = 0; x < passWidth; x++)
                        if (((row[x * CodedDepth / 8] >> (8 - CodedDepth - x * CodedDepth % 8)) & maximum)
                            * (255 / maximum) != pixels[y * source.Stride + x * 4])
                            throw new InvalidOperationException("Packed PNG samples differ.");
                    if ((passWidth * CodedDepth & 7) != 0 && (row[^1] & ((1 << (8 - (passWidth * CodedDepth & 7))) - 1)) != 0)
                        throw new InvalidOperationException("Packed PNG padding differs.");
                    continue;
                }
                for (var x = startX; x < source.Width; x += stepX)
                for (var i = 0; i < bytesPerPixel; i++)
                {
                    var sampleByte = Depth == 16 ? i ^ 1 : i;
                    var sourceByte = GrayAlpha && sampleByte >= Depth / 8 ? sampleByte + 2 * Depth / 8 : sampleByte;
                    var original = pixels[y * source.Stride + x * Depth / 8 * 4 + sourceByte];
                    var prediction = 0;
                    if (Paeth)
                    {
                        var offset = x * bytesPerPixel + i;
                        int ByteAt(int rowY, int byteX) => rowY < 0 || byteX < 0 ? 0 : pixels[rowY * source.Stride + (Depth == 16 ? byteX ^ 1 : byteX)];
                        prediction = WarmPngPaethInput.Predict(ByteAt(y, offset - bytesPerPixel), ByteAt(y - 1, offset), ByteAt(y - 1, offset - bytesPerPixel));
                    }
                    else if (Up && y > 0) prediction = pixels[(y - 1) * source.Stride + x * bytesPerPixel + i];
                    else if (Sub && x > 0) prediction = pixels[y * source.Stride + (x - 1) * bytesPerPixel + i];
                    else if (Average) prediction = ((x == 0 ? 0 : pixels[y * source.Stride + (x - 1) * bytesPerPixel + i])
                        + (y == 0 ? 0 : pixels[(y - 1) * source.Stride + x * bytesPerPixel + i])) / 2;
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
                colorType = ColorType, codedDepth = CodedDepth,
                rasterRecipe = Mono ? $"Existing original RGBA8 raster; gray=(red>>{8 - CodedDepth})*{255 / ((1 << CodedDepth) - 1)}; RGB equal gray, alpha 255. Packed MSB-first, unused low bits zero. No additional original image."
                    : Palette ? $"Existing original RGBA8 raster; index j=red>>{8 - CodedDepth}, i=j*{255 / ((1 << CodedDepth) - 1)}, coded MSB-first with unused low bits zero; original {1 << CodedDepth}-entry palette R=i,G=255-i,B=(73*i+19)%256,A=i. Includes nonzero hidden RGB at i=0. No additional original image."
                    : GrayAlpha ? $"Existing original RGBA{Depth} raster; red retained as gray, green/blue set to red, alpha unchanged including hidden RGB. No additional original image."
                    : Gray ? $"Existing original RGBA{Depth} raster; red retained as gray, green/blue set to red, alpha set to {(Depth == 16 ? 65535 : 255)}. No additional original image."
                    : Rgb ? $"Existing original RGBA{Depth} raster; RGB unchanged, alpha set to {(Depth == 16 ? 65535 : 255)} for RGB PNG. No additional original image." : "Existing original RGBA raster, all samples unchanged.",
                originalRasterSha256 = JpegBenchmarkCase.Hash(HorizontalFlipInput.Create(Depth)),
                filter = Filter,
                compression = Paeth ? $"Authored noninterlaced PNG RGBA{Depth}; every row Paeth; BCL Optimal level 6; identical resident encoded input"
                    : Up ? "Authored noninterlaced PNG RGBA8; every row Up; BCL Optimal level 6; identical resident encoded input"
                    : Sub ? "Authored noninterlaced PNG RGBA8; every row Sub; BCL Optimal level 6; identical resident encoded input"
                    : Average ? "Authored noninterlaced PNG RGBA8; every row Average; BCL Optimal level 6; identical resident encoded input"
                    : Interlaced ? "Authored PNG Adam7 RGBA16; BCL Optimal level 6; filter None; identical resident encoded input"
                    : $"BCL Optimal (zlib default level 6) / FFmpeg level 6; filter None; noninterlaced {(Mono ? "packed gray" + CodedDepth + ", opaque" : Palette ? $"palette{CodedDepth}, {1 << CodedDepth} entries and full tRNS" : GrayAlpha ? "gray-alpha" : Gray ? Depth == 16 ? "gray16be" : "gray" : Rgb ? "RGB" : "RGBA")}",
                managedIdatBytes = IdatBytes(encoded), nativeIdatBytes = Encode ? IdatBytes(File.ReadAllBytes(state.NativeOutput)) : IdatBytes(encoded),
                allRgbaSamplesExact = true, alphaExact = true, hiddenRgbExact = true,
                nativeCliNormalScalarPixelsExact = true,
                nativeEndianRepackingTimed = Encode && Depth == 16, nativeEndianWorkspaceReused = Encode && Depth == 16 && !Gray,
                nativeRgbPackingTimed = Encode && Rgb, nativeRgbWorkspaceReused = Encode && Rgb,
                nativeGrayPackingTimed = Encode && Gray, nativeGrayWorkspaceReused = Encode && Gray && Depth == 8,
                nativeGrayPlaneExtractionTimed = Encode && Gray && Depth == 16, nativeGrayFilterOutputAllocationTimed = Encode && Gray && Depth == 16,
                nativeSampleTargetMilliseconds = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_NATIVE_SAMPLE_MS") ?? "200",
                sourceUnchanged = true, independentOwnedOutput = true, nativeExitCode = 0,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location),
                benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmPng).Assembly.Location), nativeBuild = native.BuildIdentity,
                nativeReady = native.Ready, nativeBefore = before, nativeAfter = after,
                boundary = Encode && Gray
                    ? $"Warm opaque grayscale RGBA{Depth} input to complete owned gray{Depth} PNG: fresh managed MemoryStream creation/growth/public EncodePng/disposal, no redundant ToArray. Native one-thread {(Depth == 16 ? "FFmpeg extractplanes red and GRAY16LE-to-GRAY16BE conversion with owned intermediate acquisition/release" : "full-range RGBA-to-GRAY8 conversion into a reused prepared frame")} runs inside every encode operation; source/context/frame wrappers reused; codec/packet wrappers reused, real independent owned packet acquisition/encode/release included. Both level6/filter None/color0. Equal RGB and opacity fixed and independently validated. IPC/file IO/hashes/validation excluded."
                    : Encode && Rgb
                    ? $"Warm opaque RGBA{Depth} input to complete owned RGB{Depth} PNG: fresh managed MemoryStream creation/growth/public EncodePng/disposal, no redundant ToArray. Native one-thread {(Depth == 16 ? "RGBA64LE-to-RGB48BE" : "RGBA-to-RGB24")} conversion runs inside every encode operation into a reused prepared frame; codec/packet wrappers reused, real independent owned packet acquisition/encode/release included. Both level6/filter None/color2. Opacity fixed and independently validated. IPC/file IO/hashes/validation excluded."
                    : Encode
                    ? "Warm RGBA input to complete owned PNG: fresh managed MemoryStream (creation, growth, encode and disposal included; no redundant ToArray); native codec context/packet wrapper reused, real independently owned packet acquisition, encode and release included. Both level 6/filter None. For RGBA16, native LE-to-BE full-frame sample conversion runs inside every operation, using a reused prepared frame; managed conversion/row storage is included in public EncodePng. IPC, file IO, hashes and validation excluded."
                    : "Warm identical PNG bytes to independent owned RGBA pixels: public managed Decode/allocation/disposal; native codec/input packet/frame wrappers reused, complete decode/owned output/release. RGBA16 BE-to-LE, RGB48BE-to-RGBA64LE, GRAY16BE-to-RGBA64LE, YA16BE-to-RGBA64LE, RGB24-to-RGBA, GRAY8-to-RGBA, MONOBLACK-to-RGBA, YA8-to-RGBA or PAL8-to-RGBA FFmpeg conversion and second output allocation included natively; direct RGBA8 requires no conversion. Native padded rows exposed directly; packing/file IO/IPC/hashes excluded."
            }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
            Console.WriteLine("Validated warm PNG pair: " + path);
        }
        finally { native?.Dispose(); source?.Dispose(); state?.Dispose(); ready = false; }
    }
}
