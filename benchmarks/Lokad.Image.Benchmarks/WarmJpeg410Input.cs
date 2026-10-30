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
        var expected = WarmJpegFixedReference.Quarter(blocks, columns, width, height, halfHeight: true);
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
            recipe = "Existing original513x259 RGBA8 flattened over0x204060, unchanged YCbCr projection; clamped8x8 fullY and32x16 quarterwidth-halfheightCbCr rounded means. OriginalT81 DC-only/unitquantization H4V2. Independent expanded authored component rasters, actual coded dimensions, native byte pre-expansion where required, normalized Q14 horizontal/Q12 vertical filters and rational BT.601 Q13 color; exact native pixels.",
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
        var outputs = WarmJpegFixedReference.ValidateQuarterNative(state, folder, packet, expected);
        File.WriteAllText(Path.Combine(folder, "native-validation.json"), JsonSerializer.Serialize(new
        {
            schemaVersion = 1, project = "Lokad.Image", inputSha256 = packetHash, ffmpegSha256 = JpegBenchmarkCase.HashFile(state.Ffmpeg),
            nativePlanesExact = true, nativeHorizontalPreExpansionVerified = true,
            planeArguments = planeArgs, nativePlaneSha256 = Hash(planes), actualCodedGridNormalization = true,
            normalization = "Original software MJPEG component planes converted directly at coded513x259 dimensions with the public full-range BT.601/bilinear/accurate/full-chroma/in_h_chr_pos0/in_v_chr_pos0 policy. Native state reset, component decode, normalization, fresh owned RGBA acquisition and release are timed. No padded-grid remapping, recovered chroma raster or in-place compaction.",
            passed = true, noTimingClaim = true, outputs
        }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine("Prepared original JPEG410 with exact independent/native pixels at coded dimensions.");
    }
}
