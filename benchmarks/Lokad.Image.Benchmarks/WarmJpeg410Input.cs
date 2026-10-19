using System.Security.Cryptography;
using System.Text.Json;
using Lokad.Image;
using Lokad.Image.Tooling;

internal static class WarmJpeg410Input
{
    internal static void Prepare()
    {
        using var state = new JpegBenchmarkCase();
        var root = state.Root;
        var folder = Path.Combine(root, "artifacts/warm-reference/jpeg-410"); Directory.CreateDirectory(folder);
        const int width = 513, height = 259;
        string Hash(byte[] b) => Convert.ToHexStringLower(SHA256.HashData(b));
        byte Quantize(double value) => (byte)Math.Clamp(Math.Round(value, MidpointRounding.AwayFromZero), 0, 255);
        var projection = File.ReadAllBytes(Path.Combine(root, "artifacts/warm-reference/jpeg-411/projection.ycbcr"));
        if (Hash(projection) != "89d2712a5f656b456ce5beb7a91de54e0c0b42179490b8c280e228da0c04122f") throw new Exception("Existing original YCbCr projection differs.");
        int[] columns = [68, 17, 17], rows = [34, 17, 17];
        var blocks = new byte[3][];
        for (var c = 0; c < 3; c++)
        {
            var hx = c == 0 ? 1 : 4; var vy = c == 0 ? 1 : 2;
            blocks[c] = new byte[columns[c] * rows[c]];
            for (var by = 0; by < rows[c]; by++)
            for (var bx = 0; bx < columns[c]; bx++)
            {
                var sum = 0;
                for (var dy = 0; dy < 8 * vy; dy++)
                for (var dx = 0; dx < 8 * hx; dx++)
                    sum += projection[(Math.Min(by * 8 * vy + dy, height - 1) * width + Math.Min(bx * 8 * hx + dx, width - 1)) * 3 + c];
                var area = 64 * hx * vy;
                blocks[c][by * columns[c] + bx] = (byte)((sum + area / 2) / area);
            }
        }
        var packet = AuthoredJpegCompatibility.Create(width, height, [0, 0, 0], 4, 2,
            blockSample: (c, x, y) => blocks[c][y * columns[c] + x]);
        double Sample(int c, int x, int y)
        {
            if (c == 0) return blocks[c][y / 8 * columns[c] + x / 8];
            var ex = 2 * x - 3; var qy = 2 * y - 1; var left = ex >> 3; var top = qy >> 2;
            var rightWeight = ex - left * 8; var bottomWeight = qy - top * 4;
            int At(int sx, int sy) => blocks[c][Math.Clamp(sy, 0, 129) / 8 * columns[c] + Math.Clamp(sx, 0, 128) / 8];
            return (At(left, top) * (8 - rightWeight) * (4 - bottomWeight) + At(left + 1, top) * rightWeight * (4 - bottomWeight)
                + At(left, top + 1) * (8 - rightWeight) * bottomWeight + At(left + 1, top + 1) * rightWeight * bottomWeight) / 32d;
        }
        var expected = new byte[width * height * 4];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var luma = Sample(0, x, y); var cb = Sample(1, x, y) - 128; var cr = Sample(2, x, y) - 128; var at = (y * width + x) * 4;
            expected[at] = Quantize(luma + 1.402 * cr); expected[at + 1] = Quantize(luma - .344136286 * cb - .714136286 * cr);
            expected[at + 2] = Quantize(luma + 1.772 * cb); expected[at + 3] = 255;
        }
        var packetHash = Hash(packet);
        using (var first = ImageCodec.Decode(packet))
        using (var survivor = ImageCodec.Decode(packet))
        {
            if ((first.Width, first.Height, first.BitDepth, first.Orientation) != (width, height, 8, ImageOrientation.Normal)
                || !OriginalPixels.Bytes(first).AsSpan().SequenceEqual(expected) || !OriginalPixels.Bytes(survivor).AsSpan().SequenceEqual(expected)) throw new Exception("Exact analytical410 RGBA pixels differ.");
            first.GetRow8(0)[0] ^= 255; first.Dispose();
            if (!OriginalPixels.Bytes(survivor).AsSpan().SequenceEqual(expected) || Hash(packet) != packetHash) throw new Exception("Owned outputs alias or encoded source differs.");
        }
        File.WriteAllBytes(Path.Combine(folder, "input.jpg"), packet); File.WriteAllBytes(Path.Combine(folder, "expected.rgba"), expected);
        var blockBytes = blocks.SelectMany(b => b).ToArray(); File.WriteAllBytes(Path.Combine(folder, "blocks.ycbcr"), blockBytes);
        File.WriteAllText(Path.Combine(folder, "preparation.json"), JsonSerializer.Serialize(new
        {
            schemaVersion = 1, project = "Lokad.Image", width, height, bitDepth = 8, horizontalSampling = 4, verticalSampling = 2,
            recipe = "Existing original513x259 RGBA8 flattened over0x204060, unchanged YCbCr projection; clamped8x8 fullY and32x16 quarterwidth-halfheightCbCr rounded means. OriginalT81 DC-only/unitquantization H4V2. Independent integer-eighth horizontal/quarter vertical centered chroma and frozenbinary64 BT601 matrix, final-only awayfromzero quantization.",
            rasterSha256 = "70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80", projectionSha256 = Hash(projection), blocksSha256 = Hash(blockBytes),
            encodedBytes = packet.Length, encodedSha256 = packetHash, rgbaSha256 = Hash(expected),
            exactManagedPixels = true, sourceUnchanged = true, independentOwnedOutputs = true, noTimingClaim = true
        }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine(File.ReadAllText(Path.Combine(folder, "preparation.json")));
        // Verify all native planes, including FFmpeg's exact even-position samples
        // and its odd-position averages, before selecting a normalization strategy.
        var planeArgs = new[] { "-hide_banner", "-loglevel", "error", "-threads:v", "1", "-thread_type:v", "0", "-noautorotate", "-f", "image2pipe", "-c:v", "mjpeg", "-i", "pipe:0", "-frames:v", "1", "-threads:v", "1", "-c:v", "rawvideo", "-pix_fmt", "yuvj420p", "-f", "rawvideo", "pipe:1" };
        var planes = OracleProcess.Run(state.Ffmpeg, planeArgs, packet);
        if (planes.Length != width * height + 2 * 257 * 130) throw new Exception("Native410 plane lengths differ.");
        var planeOffset = 0;
        for (var c = 0; c < 3; c++)
        {
            var cw = c == 0 ? width : 257; var ch = c == 0 ? height : 130;
            for (var y = 0; y < ch; y++)
            for (var x = 0; x < cw; x++)
            {
                int At(int sx) => blocks[c][y / 8 * columns[c] + Math.Min(sx, 128) / 8];
                var value = c == 0 ? blocks[c][y / 8 * columns[c] + x / 8] : (At(x / 2) + At((x + 1) / 2)) / 2;
                if (planes[planeOffset + y * cw + x] != value) throw new Exception("Native410 original planes or horizontal pre-expansion differ.");
            }
            planeOffset += cw * ch;
        }
        const int grid = 516, chromaWidth = 129, chromaHeight = 130;
        var normalized = new byte[(grid + 2 * chromaWidth) * height];
        for (var y = 0; y < height; y++)
        {
            planes.AsSpan(y * width, width).CopyTo(normalized.AsSpan(y * grid));
            normalized.AsSpan(y * grid + width, grid - width).Fill(planes[y * width + width - 1]);
        }
        for (var c = 0; c < 2; c++)
        for (var y = 0; y < height; y++)
        {
            var top = (y + 1) / 2 - 1; var t = Math.Clamp(top, 0, chromaHeight - 1); var bottom = Math.Clamp(top + 1, 0, chromaHeight - 1);
            var tw = (y & 1) == 0 ? 1 : 3; var start = width * height + c * 257 * chromaHeight;
            for (var x = 0; x < chromaWidth; x++)
                normalized[grid * height + c * chromaWidth * height + y * chromaWidth + x] =
                    (byte)((planes[start + t * 257 + x * 2] * tw + planes[start + bottom * 257 + x * 2] * (4 - tw) + 2) / 4);
        }
        var outputs = new List<object>();
        foreach (var scalar in new[] { false, true })
        {
            var arguments = new List<string> { "-hide_banner", "-loglevel", "error" };
            if (scalar) arguments.AddRange(["-cpuflags", "0"]);
            arguments.AddRange(["-threads:v", "1", "-f", "rawvideo", "-pixel_format", "yuvj411p", "-video_size", "516x259", "-i", "pipe:0", "-filter_threads", "1", "-vf", "scale=flags=bilinear+accurate_rnd+full_chroma_int+bitexact:in_h_chr_pos=384:in_v_chr_pos=0:in_color_matrix=bt601:in_range=pc:out_range=pc,format=rgba,crop=513:259:0:0", "-frames:v", "1", "-threads:v", "1", "-pix_fmt", "rgba", "-f", "rawvideo", "pipe:1"]);
            var pixels = OracleProcess.Run(state.Ffmpeg, arguments.ToArray(), normalized);
            var agreement = JpegQualityCorpus.Compare(expected, pixels);
            if (agreement.Maximum > 4 || agreement.Rmse > 1.5) throw new Exception("Native410 centered maximum4/RMSE1.5 exceeded.");
            var name = scalar ? "centered-scalar.rgba" : "centered-native.rgba";
            File.WriteAllBytes(Path.Combine(folder, name), pixels);
            outputs.Add(new { scalar, arguments, file = name, sha256 = Hash(pixels), maximum = agreement.Maximum, rmse = agreement.Rmse, alphaExact = true, passed = true });
        }
        File.WriteAllText(Path.Combine(folder, "native-validation.json"), JsonSerializer.Serialize(new
        {
            schemaVersion = 1, project = "Lokad.Image", inputSha256 = packetHash, ffmpegSha256 = JpegBenchmarkCase.HashFile(state.Ffmpeg),
            nativePlanesExact = true, nativeHorizontalPreExpansionVerified = true, recoveredQuarterSamplesExact = true,
            planeArguments = planeArgs, nativePlaneSha256 = Hash(planes), exactCenteredGridNormalization = true,
            normalization = "Original public softwareMJPEG YUVJ420P planes preserve quarter-width samples at even positions. Recover and expand centered half-height chroma in one pass, native ties-up8bit vertical rounding, extend only luma rightpadding, full-range BT601 centered516-column horizontal conversion and crop to513. Complete native worker times decode/recovery/interpolation/staging allocation-release/conversion/in-place compaction. No packet change or second RGBA raster; unchanged maximum4/RMSE1.5/exactalpha gate, exact managed pixels.",
            passed = true, noTimingClaim = true, outputs
        }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine("Prepared original JPEG410 with exact managed pixels and verified native component recovery/centered normalization.");
    }
}
