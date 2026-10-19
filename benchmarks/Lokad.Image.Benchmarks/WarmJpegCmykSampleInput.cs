using System.Text.Json;
using Lokad.Image;
using Lokad.Image.Tooling;

internal sealed record CmykSamplePreparation(int SchemaVersion, string Project, string Recipe, string RasterSha256,
    string OriginalProjectionSha256, string OriginalBlockSamplesSha256, int EncodedBytes, string EncodedSha256,
    string CmykSha256, string FfmpegSha256);

internal static class WarmJpegCmykSampleInput
{
    internal static string DirectoryPath(string root) => Path.Combine(root, "artifacts/warm-reference/jpeg-cmyk-caller");

    internal static void Prepare()
    {
        using var state = new JpegBenchmarkCase();
        var original = WarmJpegCmykInput.DirectoryPath(state.Root);
        var prior = JsonSerializer.Deserialize<CmykJpegPreparation>(File.ReadAllText(Path.Combine(original, "preparation.json")), EncoderQualityCalibration.Json)
            ?? throw new InvalidOperationException("Prepare the original CMYK projection first.");
        var blocks = File.ReadAllBytes(Path.Combine(original, "blocks.cmyk-inverted"));
        if (prior.Project != "Lokad.Image" || prior.RasterSha256 != JpegBenchmarkCase.Hash(HorizontalFlipInput.Create(8))
            || JpegBenchmarkCase.Hash(blocks) != prior.BlockSamplesSha256
            || JpegBenchmarkCase.HashFile(Path.Combine(original, "projection.cmyk-inverted")) != prior.ProjectionSha256)
            throw new InvalidOperationException("Original CMYK projection/block means differ.");
        var packet = AuthoredJpegCompatibility.Create(513, 259, [0, 0, 0, 0],
            blockSample: (c, x, y) => 255 - blocks[(y * 65 + x) * 4 + c]);
        var expected = new byte[513 * 259 * 4];
        for (var y = 0; y < 259; y++)
        for (var x = 0; x < 513; x++)
        for (var c = 0; c < 4; c++) expected[(y * 513 + x) * 4 + c] = (byte)(255 - blocks[((y / 8) * 65 + x / 8) * 4 + c]);
        var record = new CmykSamplePreparation(1, "Lokad.Image",
            "Same original CMYK projection/block means complemented to canonical ink; untagged authored T.81 baseline four full-resolution DC planes/unit quantization. C/M/Y/K all vary; K is ink, never alpha. No RGBA intermediate.",
            prior.RasterSha256, prior.ProjectionSha256, prior.BlockSamplesSha256, packet.Length, JpegBenchmarkCase.Hash(packet), JpegBenchmarkCase.Hash(expected), JpegBenchmarkCase.HashFile(state.Ffmpeg));
        var directory = DirectoryPath(state.Root); Directory.CreateDirectory(directory);
        foreach (var (name, data) in new[] { ("input.jpg", packet), ("expected.cmyk", expected) })
        {
            var path = Path.Combine(directory, name);
            if (File.Exists(path) && !File.ReadAllBytes(path).AsSpan().SequenceEqual(data)) throw new InvalidOperationException("Frozen caller CMYK fixture differs: " + name);
            File.WriteAllBytes(path, data);
        }
        var outputs = new List<object>();
        foreach (var scalar in new[] { false, true })
        {
            List<string> arguments = ["-hide_banner", "-loglevel", "error"];
            if (scalar) arguments.AddRange(["-cpuflags", "0"]);
            arguments.AddRange(["-threads:v", "1", "-thread_type:v", "0", "-noautorotate", "-f", "image2pipe", "-c:v", "mjpeg", "-i", "pipe:0", "-filter_threads", "1", "-frames:v", "1", "-threads:v", "1", "-pix_fmt", "yuva444p", "-f", "rawvideo", "pipe:1"]);
            var name = scalar ? "scalar.planar-cmyk" : "native.planar-cmyk"; var path = Path.Combine(directory, name);
            var frozen = record.EncodedSha256 == "80c4e830cbf95a71d3f677bd8bd1ae48fe936a8464f9b30817b85d72887ae32e"
                && record.FfmpegSha256 == "8146c37e0388e8e82dc537f8c336cd75f5c7c7be1c26959f409be0c892fe2df2" && File.Exists(path)
                && JpegBenchmarkCase.HashFile(path) == "60da5eb76317285ef579ce00fc974a85d8407861452aed125f04a16b1083962b";
            var planar = frozen ? File.ReadAllBytes(path) : OracleProcess.Run(state.Ffmpeg, arguments.ToArray(), packet);
            if (planar.Length != expected.Length) throw new InvalidOperationException("Raw native CMYK dimensions differ.");
            var packed = Pack(planar);
            if (!packed.AsSpan().SequenceEqual(expected)) throw new InvalidOperationException("Analytical native CMYK planes/black differ.");
            File.WriteAllBytes(path, planar);
            outputs.Add(new { scalar, file = name, sha256 = JpegBenchmarkCase.Hash(planar), packedSha256 = JpegBenchmarkCase.Hash(packed), arguments, exact = true, reusedFrozenCli = frozen });
        }
        File.WriteAllText(Path.Combine(directory, "benchmark-preparation.json"), JsonSerializer.Serialize(record, EncoderQualityCalibration.Json) + "\n");
        File.WriteAllText(Path.Combine(directory, "native-validation.json"), JsonSerializer.Serialize(new
        {
            schemaVersion = 1, project = "Lokad.Image", passed = true, inputSha256 = record.EncodedSha256, record.FfmpegSha256,
            outputFormat = "Raw full-resolution component planes C/M/Y/K in YUVA444P containers; no YUV/color/black/alpha transformation.", outputs
        }, EncoderQualityCalibration.Json) + "\n");
        Console.WriteLine(JsonSerializer.Serialize(record, EncoderQualityCalibration.Json));
    }

    internal static byte[] Pack(byte[] planar)
    {
        const int count = 513 * 259;
        if (planar.Length != count * 4) throw new InvalidOperationException("Four raw CMYK planes required.");
        var packed = new byte[planar.Length];
        for (var i = 0; i < count; i++)
        for (var c = 0; c < 4; c++) packed[4 * i + c] = planar[c * count + i];
        return packed;
    }
}
