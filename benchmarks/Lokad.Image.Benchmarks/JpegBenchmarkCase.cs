using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Lokad.Image;
using Lokad.Image.Tooling;

internal sealed class JpegBenchmarkCase : IDisposable
{
    public string Root { get; }
    public string DirectoryPath { get; }
    public string Probe { get; }
    public string Ffmpeg { get; }
    public string Ffprobe { get; }
    private readonly string outputExtension;
    public string Input => Path.Combine(DirectoryPath, "input");
    public string ManagedOutput => Path.Combine(DirectoryPath, "managed-output" + outputExtension);
    public string NativeOutput => Path.Combine(DirectoryPath, "ffmpeg-output" + outputExtension);

    public JpegBenchmarkCase(string outputExtension = "")
    {
        if (outputExtension is not ("" or ".png" or ".webp")) throw new ArgumentException("Unexpected benchmark output extension.", nameof(outputExtension));
        this.outputExtension = outputExtension;
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "Lokad.Image.slnx"))) root = root.Parent;
        Root = root?.FullName ?? throw new InvalidOperationException("Repository root not found.");
        Probe = Path.Combine(Root, "tools", "ImageProbe", "bin", "Release", "net10.0", "ImageProbe.dll");
        if (!File.Exists(Probe)) throw new InvalidOperationException("Build ImageProbe in Release first.");
        var executable = Environment.GetEnvironmentVariable("LOKAD_IMAGE_FFMPEG");
        var registered = Path.Combine(Root, "external", "tools", "ffmpeg.exe");
        Ffmpeg = executable is null ? File.Exists(registered) ? registered : "ffmpeg"
            : File.Exists(Path.Combine(Root, executable)) ? Path.GetFullPath(Path.Combine(Root, executable)) : executable;
        var probeExecutable = Environment.GetEnvironmentVariable("LOKAD_IMAGE_FFPROBE");
        var registeredProbe = Path.Combine(Root, "external", "tools", "ffprobe.exe");
        Ffprobe = probeExecutable is null ? File.Exists(registeredProbe) ? registeredProbe : "ffprobe"
            : File.Exists(Path.Combine(Root, probeExecutable)) ? Path.GetFullPath(Path.Combine(Root, probeExecutable)) : probeExecutable;
        DirectoryPath = Path.Combine(Path.GetTempPath(), "lokad-image-jpeg-case-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(DirectoryPath);
    }

    public void CreateInput(int width, int height, JpegSampling sampling, bool progressive)
    {
        byte[] encoded;
        if (progressive) encoded = AuthoredJpeg.ProgressiveFrame(width, height, sampling, 4);
        else if (sampling == JpegSampling.Grayscale)
        {
            using var pixels = OriginalPixels.Buffer(width, height, 8, grayscale: true, opaque: true);
            using var output = new MemoryStream(); ImageCodec.EncodeJpeg(pixels, output, new JpegEncodeOptions { QuantizationPolicy = JpegQuantizationPolicy.Original, Quality = 85, Sampling = sampling });
            encoded = output.ToArray();
        }
        else encoded = OracleProcess.Run(Ffmpeg, JpegOracle.EncodeArguments(width, height, sampling, 5), OriginalPixels.Generate(width, height, 8, opaque: true));
        File.WriteAllBytes(Input, encoded);
    }

    public string[] ManagedArguments(string command, string output, params string[] options)
        => [Probe, command, Input, output, .. options];

    public static string[] DecodeArguments(string input, string output, int width, int height, JpegSampling sampling, string? transform = null)
        => ["-hide_banner", "-loglevel", "error", "-threads:v", "1", "-filter_complex_threads", "1", "-noautorotate", "-i", input,
            "-filter_complex", JpegOracle.Filter(width, height, sampling) + (transform is null ? "" : "," + transform),
            "-frames:v", "1", "-threads:v", "1", "-pix_fmt", "rgba", "-f", "rawvideo", "-y", output];

    public static string[] EncodeArguments(string input, string output, int width, int height, int depth, JpegSampling sampling, int qscale = 5, uint? backgroundRgb = null)
    {
        if (sampling == JpegSampling.Grayscale) throw new InvalidOperationException("Native MJPEG has no equivalent one-component encoder.");
        var arguments = JpegOracle.EncodeArguments(width, height, sampling, qscale).ToList();
        arguments.RemoveRange(arguments.IndexOf("-cpuflags"), 2); // Normal CPU capabilities for the benchmark baseline.
        arguments.InsertRange(arguments.IndexOf("-i"), ["-threads:v", "1"]);
        arguments[arguments.IndexOf("-pixel_format") + 1] = depth == 16 ? "rgba64le" : "rgba";
        arguments[arguments.IndexOf("-i") + 1] = input;
        if (backgroundRgb.HasValue) arguments[arguments.IndexOf("-vf") + 1] = JpegFlattenOracle.Filter(depth, backgroundRgb.Value) + "," + arguments[arguments.IndexOf("-vf") + 1];
        else if (depth == 16) arguments[arguments.IndexOf("-vf") + 1] = RgbaOracle.Normalize16Filter + "," + arguments[arguments.IndexOf("-vf") + 1];
        arguments.Insert(arguments.Count - 1, "-y"); arguments[^1] = output;
        return arguments.ToArray();
    }

    public static string[] ResizeEncodeArguments(string input, string output, int width, int height, int targetWidth, int targetHeight, ResizeFilter filter)
    {
        var encoder = JpegOracle.EncodeArguments(targetWidth, targetHeight, JpegSampling.YCbCr420, 5);
        var conversion = encoder[Array.IndexOf(encoder, "-vf") + 1];
        return ["-hide_banner", "-loglevel", "error", "-threads:v", "1", "-filter_complex_threads", "1", "-noautorotate", "-i", input,
            "-filter_complex", JpegOracle.Filter(width, height, JpegSampling.YCbCr420) + "," + ResizeOracle.Filter(targetWidth, targetHeight, filter) + "," + conversion,
            "-map_metadata", "-1", "-frames:v", "1", "-threads:v", "1", "-c:v", "mjpeg", "-huffman", "optimal", "-pix_fmt", "yuvj420p", "-q:v", "5",
            "-f", "image2pipe", "-y", output];
    }

    public long Run(string executable, string[] arguments, string output)
    {
        OracleProcess.Run(executable, arguments);
        return new FileInfo(output).Length;
    }

    public object ValidateRaw(string output, byte[] reference)
    {
        var bytes = File.ReadAllBytes(output); var difference = JpegQualityCorpus.Compare(reference, bytes);
        RequireDecoderAgreement(difference);
        return new { outputBytes = bytes.Length, outputSha256 = Hash(bytes), referenceSha256 = Hash(reference), pixelDifference = Metrics(difference) };
    }

    public object ValidateEncoded(string output, byte[] source, int width, int height, JpegSampling sampling)
    {
        var bytes = File.ReadAllBytes(output); var header = JpegQualitySyntax.Read(bytes);
        if ((header.Width, header.Height, header.Depth) != (width, height, 8) || !MatchesSampling(header, sampling))
            throw new InvalidOperationException("JPEG benchmark output profile differs.");
        var native = JpegOracle.Decode(bytes, width, height, sampling, Ffmpeg);
        using var actual = ImageCodec.Decode(bytes); var managed = OriginalPixels.Bytes(actual);
        var difference = JpegQualityCorpus.Compare(native, managed); RequireDecoderAgreement(difference);
        return new
        {
            outputBytes = bytes.Length, outputSha256 = Hash(bytes), header.Components, codedSampling = header.Sampling,
            header.QuantizationHash, nativeDecodedSha256 = Hash(native), managedDecodedSha256 = Hash(managed),
            sourceSha256 = Hash(source), sourceError = Metrics(JpegQualityCorpus.Compare(source, native)), decoderDifference = Metrics(difference)
        };
    }

    public void ValidateManagedEncode(byte[] source, int width, int height, int depth, JpegSampling sampling)
    {
        using var image = Buffer(source, width, height, depth); using var expected = new MemoryStream();
        ImageCodec.EncodeJpeg(image, expected, new JpegEncodeOptions { QuantizationPolicy = JpegQuantizationPolicy.Original, Quality = 85, Sampling = sampling });
        if (!expected.ToArray().AsSpan().SequenceEqual(File.ReadAllBytes(ManagedOutput))) throw new InvalidOperationException("ImageProbe output differs from the requested public API workflow.");
    }

    public static ImageBuffer Buffer(byte[] pixels, int width, int height, int depth = 8)
    {
        var image = ImageBuffer.Create(width, height, depth);
        for (var y = 0; y < height; y++)
            if (depth == 8) pixels.AsSpan(y * width * 4, width * 4).CopyTo(image.GetRow8(y));
            else for (var i = 0; i < width * 4; i++) image.GetRow16(y)[i] = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(pixels.AsSpan((y * width * 4 + i) * 2));
        return image;
    }

    public void Report(string name, object parameters, string[] managedArguments, string[] nativeArguments, object managed, object native,
        object? stages = null, string? method = null, object? inputs = null, object? childMemory = null, long? totalInputBytes = null,
        bool inputIsIndex = false, string? nativeExecutable = null)
    {
        var baselineExecutable = nativeExecutable ?? Ffmpeg;
        if (childMemory is null)
        {
            var nativeHash = Hash(File.ReadAllBytes(NativeOutput)); var managedHash = Hash(File.ReadAllBytes(ManagedOutput));
            childMemory = new
            {
                native = ChildProcessMemory.Measure(baselineExecutable, nativeArguments, DirectoryPath),
                managed = ChildProcessMemory.Measure("dotnet", managedArguments, DirectoryPath)
            };
            if (nativeHash != Hash(File.ReadAllBytes(NativeOutput)) || managedHash != Hash(File.ReadAllBytes(ManagedOutput)))
                throw new InvalidOperationException("Child-memory counter execution changed the validated benchmark output.");
        }
        var directory = Path.Combine(Root, "artifacts", "benchmark-validation"); Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, name + "-" + Guid.NewGuid().ToString("N") + ".json");
        var version = Encoding.UTF8.GetString(OracleProcess.Run(Ffmpeg, ["-version"])).Replace("\r\n", "\n");
        var baselineVersion = baselineExecutable == Ffmpeg ? version
            : Encoding.UTF8.GetString(OracleProcess.Run(baselineExecutable, ["-version"])).Replace("\r\n", "\n");
        File.WriteAllText(path, JsonSerializer.Serialize(new
        {
            schemaVersion = 2, name, parameters, method = method ?? "Untimed correctness validation; prepared inputs, one process and file input/output per subject. No timing measurement. Child-memory counters are collected in separate untimed Linux executions and explicitly unavailable on other platforms. Cases producing JPEG use independent encoder quality 85 and native qscale 5 settings.",
            runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"),
            runtime = RuntimeInformation.FrameworkDescription, platform = RuntimeInformation.RuntimeIdentifier,
            librarySha256 = Hash(File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(Probe)!, "Lokad.Image.dll"))),
            libraryHashScope = "ImageProbe codec child library",
            validationLibrarySha256 = Hash(File.ReadAllBytes(typeof(ImageBuffer).Assembly.Location)), probeSha256 = Hash(File.ReadAllBytes(Probe)),
            benchmarkAssemblySha256 = Hash(File.ReadAllBytes(typeof(JpegDecodeInvocation).Assembly.Location)),
            inputSha256 = HashFile(Input), inputBytes = totalInputBytes ?? new FileInfo(Input).Length,
            inputHashScope = inputIsIndex ? "ordered numbered-file index containing each encoded file's hash" : "single encoded or raw input file",
            inputIndexBytes = inputIsIndex ? new FileInfo(Input).Length : (long?)null,
            ffmpegVersion = version, ffmpegExecutableSha256 = File.Exists(Ffmpeg) ? Hash(File.ReadAllBytes(Ffmpeg)) : null,
            ffmpegHashScope = File.Exists(Ffmpeg) ? "explicit executable" : "PATH executable hash not established",
            nativeExecutableName = Path.GetFileName(baselineExecutable), nativeVersion = baselineVersion,
            nativeExecutableSha256 = File.Exists(baselineExecutable) ? Hash(File.ReadAllBytes(baselineExecutable)) : null,
            nativeHashScope = File.Exists(baselineExecutable) ? "explicit baseline executable" : "PATH baseline executable hash not established",
            managedArguments = managedArguments.Select(NormalizeArgument), nativeArguments = nativeArguments.Select(NormalizeArgument), managed, native, stages, inputs, childMemory
        }, new JsonSerializerOptions { WriteIndented = true, Converters = { new JsonStringEnumConverter() } }) + "\n");
        Console.WriteLine($"Validated {name}; untimed evidence: {path}");
    }

    private string NormalizeArgument(string argument) => argument.Replace(Probe, "{ImageProbe}", StringComparison.Ordinal).Replace(DirectoryPath, "{case}", StringComparison.Ordinal);
    public static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);
    public static string Hash(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));
    public static string HashFile(string path)
    {
        using var input = File.OpenRead(path);
        return Convert.ToHexStringLower(SHA256.HashData(input));
    }
    public static object Metrics((int Maximum, double Rmse, double? Psnr) difference)
        => new { maximum = difference.Maximum, rgbRmse = difference.Rmse, rgbPsnr = difference.Psnr };

    private static void RequireDecoderAgreement((int Maximum, double Rmse, double? Psnr) difference)
    {
        if (difference.Maximum > 4 || difference.Rmse > 1.5) throw new InvalidOperationException($"JPEG decoder benchmark differs: maximum {difference.Maximum}, RMSE {difference.Rmse}.");
    }

    public static bool MatchesSampling(JpegQualitySyntax.Header header, JpegSampling sampling)
    {
        var components = header.Sampling.Split(',').Select(value => value.Split('x').Select(part => int.Parse(part, CultureInfo.InvariantCulture)).ToArray()).ToArray();
        if (sampling == JpegSampling.Grayscale) return header.Components == 1;
        var h = sampling is JpegSampling.YCbCr422 or JpegSampling.YCbCr420 ? 2 : 1; var v = sampling == JpegSampling.YCbCr420 ? 2 : 1;
        return header.Components == 3 && components.Length == 3 && components.Skip(1).All(factor => components[0][0] == factor[0] * h && components[0][1] == factor[1] * v);
    }

    public void Dispose()
    {
        if (!Directory.Exists(DirectoryPath)) return;
        var target = Path.GetFullPath(DirectoryPath); var parent = Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (!string.Equals(Path.GetDirectoryName(target), parent, StringComparison.OrdinalIgnoreCase)
            || !Path.GetFileName(target).StartsWith("lokad-image-jpeg-case-", StringComparison.Ordinal)) throw new InvalidOperationException("Unexpected JPEG benchmark cleanup path.");
        Directory.Delete(target, true);
    }
}
