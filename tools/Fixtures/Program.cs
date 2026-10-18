using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Lokad.Image.Tooling;

var output = Path.GetFullPath(args.Length == 0 ? "tests/Fixtures" : args[0]);
Directory.CreateDirectory(output);
if (args.Length == 3 && args[1] == "--compression-diagram")
{
    CompressionFixtures.Diagram(output, args[2]);
    return;
}
if (args.Length == 2 && args[1] is "--font-layout" or "--font-render")
{
    FontFixtures.Generate(output, args[1] == "--font-render");
    return;
}
var executable = Environment.GetEnvironmentVariable("LOKAD_IMAGE_FFMPEG") ?? "external/tools/ffmpeg.exe";
if (!File.Exists(executable)) throw new InvalidOperationException("Register the pinned oracle or set LOKAD_IMAGE_FFMPEG to its actual executable.");
var executableHash = Hash(File.ReadAllBytes(executable));
var version = Encoding.UTF8.GetString(OracleProcess.Run(executable, ["-version"]));
if (args.Length == 2 && args[1] == "--jpeg-quality")
{
    JpegQuality.Generate(output, executable, executableHash, version);
    return;
}
if (args.Length == 2 && args[1] == "--quality")
{
    Vp8Quality.Generate(output, executable, executableHash, version);
    return;
}
if (args.Length == 2 && args[1] == "--authored-vp8")
{
    AuthoredVp8Fixtures.Generate(output, executable, executableHash, version);
    return;
}
if (args.Length == 2 && args[1] == "--authored-png")
{
    AuthoredPngFixtures.Generate(output, executable, executableHash, version);
    return;
}
if (args.Length == 2 && args[1] == "--refresh-managed-vp8")
{
    Vp8Fixtures.RefreshManaged(output, executable, executableHash);
    return;
}
var fixtures = new List<object>();
foreach (var depth in new[] { 8, 16 })
foreach (var format in new[] { "rgba", "rgb", "gray", "grayalpha" })
foreach (var interlace in new[] { false, true })
{
    var width = 9; var height = 7;
    var grayscale = format is "gray" or "grayalpha";
    var opaque = format is "gray" or "rgb";
    var pixels = OriginalPixels.Generate(width, height, depth, grayscale, opaque);
    var inputFormat = depth == 8 ? "rgba" : "rgba64le";
    var outputFormat = (format, depth) switch
    {
        ("rgba", 8) => "rgba", ("rgba", 16) => "rgba64be", ("rgb", 8) => "rgb24", ("rgb", 16) => "rgb48be",
        ("gray", 8) => "gray", ("gray", 16) => "gray16be", ("grayalpha", 8) => "ya8", _ => "ya16be"
    };
    string[] encode = ["-hide_banner", "-loglevel", "error", "-cpuflags", "0", "-f", "rawvideo", "-pixel_format", inputFormat,
        "-video_size", $"{width}x{height}", "-i", "pipe:0", "-frames:v", "1", "-c:v", "png", "-threads:v", "1",
        "-pix_fmt", outputFormat, "-pred", "mixed", "-flags", interlace ? "+ildct" : "-ildct", "-f", "image2pipe", "pipe:1"];
    var encoded = OracleProcess.Run(executable, encode, pixels);
    if (encoded[28] != (interlace ? 1 : 0)) throw new InvalidOperationException("Oracle did not produce the requested PNG interlace mode.");
    string[] decode = ["-hide_banner", "-loglevel", "error", "-cpuflags", "0", "-noautorotate", "-f", "image2pipe", "-i", "pipe:0",
        "-frames:v", "1", "-threads:v", "1", "-pix_fmt", inputFormat, "-f", "rawvideo", "pipe:1"];
    var expected = OracleProcess.Run(executable, decode, encoded);
    var name = $"png-{format}{depth}-{(interlace ? "adam7" : "plain")}";
    File.WriteAllBytes(Path.Combine(output, name + ".png"), encoded);
    File.WriteAllBytes(Path.Combine(output, name + ".rgba"), expected);
    fixtures.Add(new
    {
        name, width, height, depth, recipe = "asymmetric-v1", inputHash = Hash(pixels), encode, decode,
        encodedHash = Hash(encoded), expectedHash = Hash(expected), comparison = "exact-rgba-samples"
    });
}
File.WriteAllText(Path.Combine(output, "manifest.json"), JsonSerializer.Serialize(new
{
    schemaVersion = 1, platform = RuntimeInformation.RuntimeIdentifier,
    ffmpegSha256 = executableHash, ffmpegVersion = version.Replace("\r\n", "\n"),
    fixtures
}, new JsonSerializerOptions { WriteIndented = true }) + "\n");
Console.WriteLine($"Generated {fixtures.Count} original PNG fixtures.");
JpegFixtures.Generate(output, executable, executableHash, version);
WebpFixtures.Generate(output, executable, executableHash, version);
Vp8Fixtures.Generate(output, executable, executableHash, version);
AuthoredVp8Fixtures.Generate(output, executable, executableHash, version);
AuthoredPngFixtures.Generate(output, executable, executableHash, version);

static string Hash(byte[] data) => Convert.ToHexStringLower(SHA256.HashData(data));
