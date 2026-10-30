using System.Text.Json;
using Lokad.Image;
using Lokad.Image.Tooling;

internal sealed record YcckJpegPreparation(int SchemaVersion, string Project, string Recipe, string RasterSha256,
    string ProjectionSha256, string BlocksSha256, int EncodedBytes, string EncodedSha256,
    string RgbaSha256, string QuantizationSha256, string FfmpegSha256);

internal static class WarmJpegYcckInput
{
    internal static string DirectoryPath(string root) => Path.Combine(root, "artifacts/warm-reference/jpeg-ycck420");

    internal static void Prepare()
    {
        using var state = new JpegBenchmarkCase();
        const int width = 513, height = 259;
        var original = HorizontalFlipInput.Create(8); var opaque = JpegQualityCorpus.Reference(original, 8, 0x204060);
        var projection = new byte[width * height * 4];
        for (var i = 0; i < projection.Length; i += 4)
        {
            var k = Math.Max(opaque[i], Math.Max(opaque[i + 1], opaque[i + 2]));
            var c = k == 0 ? 0 : 255 - (opaque[i] * 255 + k / 2) / k;
            var m = k == 0 ? 0 : 255 - (opaque[i + 1] * 255 + k / 2) / k;
            var y = k == 0 ? 0 : 255 - (opaque[i + 2] * 255 + k / 2) / k;
            projection[i] = Quantize(.299 * c + .587 * m + .114 * y);
            projection[i + 1] = Quantize(128 - .168735892 * c - .331264108 * m + .5 * y);
            projection[i + 2] = Quantize(128 + .5 * c - .418687589 * m - .081312411 * y);
            projection[i + 3] = (byte)k;
        }
        var columns = new[] { 66, 33, 33, 66 }; var rows = new[] { 34, 17, 17, 34 };
        var blocks = new byte[4][];
        for (var component = 0; component < 4; component++)
        {
            var step = component is 1 or 2 ? 2 : 1;
            blocks[component] = new byte[columns[component] * rows[component]];
            for (var by = 0; by < rows[component]; by++)
            for (var bx = 0; bx < columns[component]; bx++)
            {
                var sum = 0;
                for (var dy = 0; dy < 8 * step; dy++)
                for (var dx = 0; dx < 8 * step; dx++)
                    sum += projection[(Math.Min(by * 8 * step + dy, height - 1) * width + Math.Min(bx * 8 * step + dx, width - 1)) * 4 + component];
                var area = 64 * step * step;
                blocks[component][by * columns[component] + bx] = (byte)((sum + area / 2) / area);
            }
        }
        var packet = AuthoredJpegCompatibility.Create(width, height, [0, 0, 0, 0], 2, 2, true, 2,
            blockSample: (c, x, y) => blocks[c][y * columns[c] + x]);
        var expected = WarmJpegFixedReference.Ycck(blocks, columns, width, height);
        using (var decoded = ImageCodec.Decode(packet))
            if (!OriginalPixels.Bytes(decoded).AsSpan().SequenceEqual(expected)) throw new InvalidOperationException("YCCK420 analytical pixels differ.");
        var blockBytes = blocks.SelectMany(b => b).ToArray();
        var record = new YcckJpegPreparation(1, "Lokad.Image",
            "Original513x259 RGBA8 flattened over0x204060; canonical device-CMYK projection, YCbCr of CMY ink with stored invertedK. Varying all four channels. Clamped8x8 full Y/K and16x16 Cb/Cr means ties-up; authored T.81 DC-only unit-quantization Adobe2 H2V2 fullK. Independent half-grid byte interpolation, Q16 black products and rational BT.601 Q13 color reference; exact pinned native pixels.",
            JpegBenchmarkCase.Hash(original), JpegBenchmarkCase.Hash(projection), JpegBenchmarkCase.Hash(blockBytes), packet.Length,
            JpegBenchmarkCase.Hash(packet), JpegBenchmarkCase.Hash(expected), JpegQualitySyntax.Read(packet).QuantizationHash, JpegBenchmarkCase.HashFile(state.Ffmpeg));
        var directory = DirectoryPath(state.Root); Directory.CreateDirectory(directory);
        foreach (var (name, data) in new[] { ("input.jpg", packet), ("projection.ycck", projection), ("blocks.ycck", blockBytes), ("expected.rgba", expected) })
        {
            var path = Path.Combine(directory, name);
            if (File.Exists(path) && !File.ReadAllBytes(path).AsSpan().SequenceEqual(data)) throw new InvalidOperationException("Frozen YCCK420 fixture differs: " + name);
            File.WriteAllBytes(path, data);
        }
        var json = JsonSerializer.Serialize(record, EncoderQualityCalibration.Json) + "\n";
        var manifest = Path.Combine(directory, "preparation.json");
        if (File.Exists(manifest) && File.ReadAllText(manifest) != json) throw new InvalidOperationException("Frozen YCCK420 manifest differs.");
        File.WriteAllText(manifest, json); Console.WriteLine(json);
        var outputs = new List<object>(); var passed = true;
        var cachedValidation = Path.Combine(directory, "native-validation.json");
        using var cached = File.Exists(cachedValidation) ? JsonDocument.Parse(File.ReadAllText(cachedValidation)) : null;
        foreach (var scalar in new[] { false, true })
        {
            List<string> arguments = ["-hide_banner", "-loglevel", "error"];
            if (scalar) arguments.AddRange(["-cpuflags", "0"]);
            arguments.AddRange(["-threads:v", "1", "-thread_type:v", "0", "-noautorotate", "-f", "image2pipe", "-c:v", "mjpeg", "-i", "pipe:0", "-filter_threads", "1",
                "-vf", "scale=flags=neighbor+bitexact:in_color_matrix=bt601:in_range=pc:out_range=pc,format=rgba", "-frames:v", "1", "-threads:v", "1", "-pix_fmt", "rgba", "-f", "rawvideo", "pipe:1"]);
            var name = scalar ? "scalar.rgba" : "native.rgba";
            byte[] pixels;
            if (cached is not null && cached.RootElement.GetProperty("inputSha256").GetString() == record.EncodedSha256
                && cached.RootElement.GetProperty("ffmpegSha256").GetString() == record.FfmpegSha256)
            {
                var entry = cached.RootElement.GetProperty("outputs").EnumerateArray().Single(e => e.GetProperty("scalar").GetBoolean() == scalar);
                pixels = File.ReadAllBytes(Path.Combine(directory, name));
                if (JpegBenchmarkCase.Hash(pixels) != entry.GetProperty("sha256").GetString()
                    || !entry.GetProperty("arguments").EnumerateArray().Select(a => a.GetString()).SequenceEqual(arguments))
                    throw new InvalidOperationException("Cached native YCCK CLI input/output/protocol differs.");
            }
            else pixels = OracleProcess.Run(state.Ffmpeg, arguments.ToArray(), packet);
            if (pixels.Length != expected.Length) throw new InvalidOperationException("YCCK native CLI dimensions differ.");
            var maximum = 0; long squared = 0; var opaqueAlpha = true; var withinMaximum = true; var interiorMaximum = 0;
            for (var i = 0; i < pixels.Length; i++)
            {
                if (i % 4 == 3) { opaqueAlpha &= pixels[i] == 255; continue; }
                var error = Math.Abs(pixels[i] - expected[i]); maximum = Math.Max(maximum, error); squared += error * error;
                var pixel = i / 4; var x = pixel % width; var y = pixel / width;
                var transition = x % 16 is 0 or 15 || y % 16 is 0 or 15;
                withinMaximum &= error <= (transition ? 16 : 4);
                if (!transition) interiorMaximum = Math.Max(interiorMaximum, error);
            }
            var rmse = Math.Sqrt((double)squared / (width * height * 3));
            var withinBounds = opaqueAlpha && pixels.AsSpan().SequenceEqual(expected); passed &= withinBounds;
            File.WriteAllBytes(Path.Combine(directory, name), pixels);
            outputs.Add(new { scalar, maximum, interiorMaximum, rmse, opaqueAlpha, withinBounds, file = name, sha256 = JpegBenchmarkCase.Hash(pixels), arguments });
        }
        File.WriteAllText(Path.Combine(directory, "native-validation.json"), JsonSerializer.Serialize(new
        {
            schemaVersion = 1, project = "Lokad.Image", inputSha256 = record.EncodedSha256, record.FfmpegSha256,
            exactManagedPixels = true, exactNativePixels = true, nativeExceptionUsed = false, passed, outputs,
            approvedNativeOnlyGate = "Owner-approved2026-10-19: RGB<=16 only at x/y mod16=0or15 authored chroma transitions, <=4 elsewhere, RGB RMSE<=1.5; alpha255 and exact managed pixels unchanged.",
            reusedFrozenCli = cached is not null
        }, EncoderQualityCalibration.Json) + "\n");
        if (!passed) throw new InvalidOperationException("YCCK420 exact native pixels differ.");
    }

    private static byte Quantize(double value) => (byte)Math.Clamp(Math.Round(value, MidpointRounding.AwayFromZero), 0, 255);
}
