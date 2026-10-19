using System.Text.Json;
using Lokad.Image.Tooling;

internal sealed record YcckRgbPreparation(int SchemaVersion, string Project, string RasterSha256,
    int EncodedBytes, string EncodedSha256, string RgbSha256, string FfmpegSha256);

internal static class WarmJpegYcckRgbInput
{
    internal static string DirectoryPath(string root) => Path.Combine(root, "artifacts/warm-reference/jpeg-ycck420-caller-rgb");

    internal static void Prepare()
    {
        using var state = new JpegBenchmarkCase(); var original = WarmJpegYcckInput.DirectoryPath(state.Root);
        var prior = JsonSerializer.Deserialize<YcckJpegPreparation>(File.ReadAllText(Path.Combine(original, "preparation.json")), EncoderQualityCalibration.Json) ?? throw new InvalidOperationException("Prepare YCCK420 first.");
        var packet = File.ReadAllBytes(Path.Combine(original, "input.jpg")); var rgba = File.ReadAllBytes(Path.Combine(original, "expected.rgba"));
        if (prior.Project != "Lokad.Image" || prior.RasterSha256 != JpegBenchmarkCase.Hash(HorizontalFlipInput.Create(8))
            || JpegBenchmarkCase.Hash(packet) != "211fcc243d40db6e6e3e8944a2e479825289201bff140e5cf2901e85ee97053d"
            || JpegBenchmarkCase.Hash(rgba) != "47e27c6d2fdbac87f68220467673c2ef54079e4b2ccd1c486457635e6a0df74c")
            throw new InvalidOperationException("Existing original YCCK packet/analytical reference differs.");
        var expected = Project(rgba); var directory = DirectoryPath(state.Root); Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "expected.rgb");
        if (File.Exists(path) && !File.ReadAllBytes(path).AsSpan().SequenceEqual(expected)) throw new InvalidOperationException("Frozen caller YCCK RGB differs.");
        File.WriteAllBytes(path, expected);
        var record = new YcckRgbPreparation(1, "Lokad.Image", prior.RasterSha256, packet.Length, JpegBenchmarkCase.Hash(packet), JpegBenchmarkCase.Hash(expected), JpegBenchmarkCase.HashFile(state.Ffmpeg));
        var nativeRgbaRgb = Project(File.ReadAllBytes(Path.Combine(original, "native.rgba"))); var outputs = new List<object>();
        foreach (var scalar in new[] { false, true })
        {
            List<string> arguments = ["-hide_banner", "-loglevel", "error"];
            if (scalar) arguments.AddRange(["-cpuflags", "0"]);
            arguments.AddRange(["-threads:v", "1", "-thread_type:v", "0", "-noautorotate", "-f", "image2pipe", "-c:v", "mjpeg", "-i", "pipe:0", "-filter_threads", "1", "-vf", "scale=flags=neighbor+bitexact:in_color_matrix=bt601:in_range=pc:out_range=pc,format=rgb24", "-frames:v", "1", "-threads:v", "1", "-pix_fmt", "rgb24", "-f", "rawvideo", "pipe:1"]);
            var name = scalar ? "scalar.rgb" : "native.rgb"; path = Path.Combine(directory, name);
            var frozen = record.FfmpegSha256 == "8146c37e0388e8e82dc537f8c336cd75f5c7c7be1c26959f409be0c892fe2df2"
                && File.Exists(path) && JpegBenchmarkCase.HashFile(path) == "4faf220748d6a975919c876f0d2f0f554196e21a81d909c1f6c9bdae8ad94634";
            var pixels = frozen ? File.ReadAllBytes(path) : OracleProcess.Run(state.Ffmpeg, arguments.ToArray(), packet);
            var (maximum, rmse) = WarmJpegYcckRgbDecode.Agreement(pixels, expected);
            if (!pixels.AsSpan().SequenceEqual(nativeRgbaRgb)) throw new InvalidOperationException("Direct native RGB24 differs from approved RGBA color normalization.");
            File.WriteAllBytes(path, pixels);
            outputs.Add(new { scalar, file = name, sha256 = JpegBenchmarkCase.Hash(pixels), arguments, maximum, rmse, nativeRgbaRgbExact = true, reusedFrozenCli = frozen });
        }
        File.WriteAllText(Path.Combine(directory, "benchmark-preparation.json"), JsonSerializer.Serialize(record, EncoderQualityCalibration.Json) + "\n");
        File.WriteAllText(Path.Combine(directory, "native-validation.json"), JsonSerializer.Serialize(new { schemaVersion = 1, project = "Lokad.Image", passed = true, inputSha256 = record.EncodedSha256, record.FfmpegSha256, outputs }, EncoderQualityCalibration.Json) + "\n");
        Console.WriteLine(JsonSerializer.Serialize(record, EncoderQualityCalibration.Json));
    }

    internal static byte[] Project(byte[] rgba)
    {
        if (rgba.Length != 513 * 259 * 4) throw new InvalidOperationException("Existing YCCK reference geometry differs.");
        var rgb = new byte[513 * 259 * 3];
        for (var i = 0; i < 513 * 259; i++)
        {
            if (rgba[i * 4 + 3] != 255) throw new InvalidOperationException("Existing YCCK reference alpha differs.");
            rgba.AsSpan(i * 4, 3).CopyTo(rgb.AsSpan(i * 3, 3));
        }
        return rgb;
    }
}
