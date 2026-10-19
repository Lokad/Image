using System.Text.Json;
using Lokad.Image;
using Lokad.Image.Tooling;

internal sealed record CmykJpegPreparation(int SchemaVersion, string Project, string Recipe, string RasterSha256,
    string OpaqueRgbaSha256, string ProjectionSha256, string BlockSamplesSha256, int EncodedBytes, string EncodedSha256,
    string CanonicalCmykSha256, string RgbaSha256, string QuantizationSha256, string FfmpegSha256);

internal static class WarmJpegCmykInput
{
    public static string DirectoryPath(string root) => Path.Combine(root, "artifacts/warm-reference/jpeg-cmyk-adobe");

    public static void Prepare()
    {
        using var state = new JpegBenchmarkCase();
        var original = HorizontalFlipInput.Create(8); var opaque = JpegQualityCorpus.Reference(original, 8, 0x204060);
        var projection = new byte[opaque.Length];
        for (var i = 0; i < 513 * 259; i++)
        {
            var at = i * 4; var maximum = Math.Max(opaque[at], Math.Max(opaque[at + 1], opaque[at + 2]));
            // Adobe stores inverted ink. K' is max(R,G,B); C'/M'/Y' are
            // normalized device channels. Round the projection only here.
            for (var c = 0; c < 3; c++) projection[at + c] = maximum == 0 ? (byte)255 : (byte)((opaque[at + c] * 255 + maximum / 2) / maximum);
            projection[at + 3] = (byte)maximum;
        }
        const int columns = (513 + 7) / 8, rows = (259 + 7) / 8;
        var blocks = new byte[columns * rows * 4];
        for (var by = 0; by < rows; by++)
        for (var bx = 0; bx < columns; bx++)
        for (var c = 0; c < 4; c++)
        {
            var sum = 0;
            for (var dy = 0; dy < 8; dy++)
            for (var dx = 0; dx < 8; dx++) sum += projection[(Math.Min(by * 8 + dy, 258) * 513 + Math.Min(bx * 8 + dx, 512)) * 4 + c];
            blocks[(by * columns + bx) * 4 + c] = (byte)((sum + 32) / 64);
        }
        var packet = AuthoredJpegCompatibility.Create(513, 259, [0, 0, 0, 0], adobe: 0,
            blockSample: (c, x, y) => blocks[(y * columns + x) * 4 + c]);
        var expected = new byte[513 * 259 * 4]; var canonical = new byte[expected.Length];
        for (var y = 0; y < 259; y++)
        for (var x = 0; x < 513; x++)
        {
            var at = (y * 513 + x) * 4; var block = ((y / 8) * columns + x / 8) * 4;
            for (var c = 0; c < 4; c++) canonical[at + c] = (byte)(255 - blocks[block + c]);
            for (var c = 0; c < 3; c++) expected[at + c] = (byte)((blocks[block + c] * blocks[block + 3] + 127) / 255);
            expected[at + 3] = 255;
        }
        var actualCmyk = new byte[canonical.Length]; ImageCodec.DecodeJpeg(packet, actualCmyk, JpegSampleFormat.Cmyk);
        if (!actualCmyk.AsSpan().SequenceEqual(canonical)) throw new InvalidOperationException("Analytical CMYK fixture component/inversion check failed.");
        ImageCodec.DecodeJpeg(packet, actualCmyk, JpegSampleFormat.Cmyk, options: new() { CmykInversion = JpegCmykInversion.None });
        for (var i = 0; i < actualCmyk.Length; i++) if (actualCmyk[i] != 255 - canonical[i]) throw new InvalidOperationException("Explicit CMYK inversion control failed.");
        using (var decoded = ImageCodec.Decode(packet))
            if (!OriginalPixels.Bytes(decoded).AsSpan().SequenceEqual(expected)) throw new InvalidOperationException("Analytical CMYK device-RGB check failed.");
        var record = new CmykJpegPreparation(1, "Lokad.Image",
            "Original513x259 RGBA8 raster flattened over0x204060; canonical device-CMYK projection stored Adobe-inverted, clamped8x8 component means rounded ties-up, authored T.81 baseline DC-only unit-quantization four-plane packet with Adobe transform0. K is black, never alpha.",
            JpegBenchmarkCase.Hash(original), JpegBenchmarkCase.Hash(opaque), JpegBenchmarkCase.Hash(projection), JpegBenchmarkCase.Hash(blocks), packet.Length,
            JpegBenchmarkCase.Hash(packet), JpegBenchmarkCase.Hash(canonical), JpegBenchmarkCase.Hash(expected), JpegQualitySyntax.Read(packet).QuantizationHash, JpegBenchmarkCase.HashFile(state.Ffmpeg));
        var directory = DirectoryPath(state.Root); Directory.CreateDirectory(directory);
        foreach (var (name, data) in new[] { ("input.jpg", packet), ("projection.cmyk-inverted", projection), ("blocks.cmyk-inverted", blocks), ("expected.cmyk", canonical), ("expected.rgba", expected) })
        {
            var path = Path.Combine(directory, name);
            if (File.Exists(path) && !File.ReadAllBytes(path).AsSpan().SequenceEqual(data)) throw new InvalidOperationException("Frozen CMYK fixture differs: " + name);
            File.WriteAllBytes(path, data);
        }
        var json = JsonSerializer.Serialize(record, EncoderQualityCalibration.Json) + "\n"; var manifest = Path.Combine(directory, "preparation.json");
        if (File.Exists(manifest) && File.ReadAllText(manifest) != json) throw new InvalidOperationException("Frozen CMYK fixture manifest differs.");
        File.WriteAllText(manifest, json); Console.WriteLine(json);
    }
}
