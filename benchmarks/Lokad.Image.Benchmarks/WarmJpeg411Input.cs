using System.Security.Cryptography;
using System.Text.Json;
using Lokad.Image;
using Lokad.Image.Tooling;


internal static class WarmJpeg411Input
{
    internal static void Prepare()
    {
        using var state = new JpegBenchmarkCase();
        var root = state.Root;
        var folder = Path.Combine(root, "artifacts/warm-reference/jpeg-411");
        Directory.CreateDirectory(folder);
        const int width = 513, height = 259;
        string Hash(byte[] b) => Convert.ToHexStringLower(SHA256.HashData(b));
        byte Quantize(double value) => (byte)Math.Clamp(Math.Round(value, MidpointRounding.AwayFromZero), 0, 255);
        var original = OriginalPixels.Generate(width, height, 8);
        original.AsSpan(3, 1).Clear(); original.AsSpan(width * 4 - 1, 1).Clear();
        if (Hash(original) != "70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80") throw new Exception("Original raster differs.");
        var opaque = JpegQualityCorpus.Reference(original, 8, 0x204060);
        var projection = new byte[width * height * 3];
        for (var i = 0; i < width * height; i++)
        {
            double r = opaque[i * 4], g = opaque[i * 4 + 1], b = opaque[i * 4 + 2];
            projection[i * 3] = Quantize(.299 * r + .587 * g + .114 * b);
            projection[i * 3 + 1] = Quantize(128 - .168735892 * r - .331264108 * g + .5 * b);
            projection[i * 3 + 2] = Quantize(128 + .5 * r - .418687589 * g - .081312411 * b);
        }
        // Full MCU padding is authored using the same clamped spatial means.
        int[] columns = [68, 17, 17]; const int rows = 33;
        var blocks = new byte[3][];
        for (var c = 0; c < 3; c++)
        {
            var step = c == 0 ? 1 : 4;
            blocks[c] = new byte[columns[c] * rows];
            for (var by = 0; by < rows; by++)
            for (var bx = 0; bx < columns[c]; bx++)
            {
                var sum = 0;
                for (var dy = 0; dy < 8; dy++)
                for (var dx = 0; dx < 8 * step; dx++)
                    sum += projection[(Math.Min(by * 8 + dy, height - 1) * width + Math.Min(bx * 8 * step + dx, width - 1)) * 3 + c];
                var area = 64 * step;
                blocks[c][by * columns[c] + bx] = (byte)((sum + area / 2) / area);
            }
        }
        var packet = AuthoredJpegCompatibility.Create(width, height, [0, 0, 0], 4, 1,
            blockSample: (c, x, y) => blocks[c][y * columns[c] + x]);
        var expected = WarmJpegFixedReference.Quarter(blocks, columns, width, height, halfHeight: false);
        var packetHash = Hash(packet);
        using (var first = ImageCodec.Decode(packet))
        using (var survivor = ImageCodec.Decode(packet))
        {
            if ((first.Width, first.Height, first.BitDepth, first.Orientation) != (width, height, 8, ImageOrientation.Normal)
                || !OriginalPixels.Bytes(first).AsSpan().SequenceEqual(expected)
                || !OriginalPixels.Bytes(survivor).AsSpan().SequenceEqual(expected)) throw new Exception("Exact analytical411 RGBA pixels differ.");
            first.GetRow8(0)[0] ^= 255; first.Dispose();
            if (!OriginalPixels.Bytes(survivor).AsSpan().SequenceEqual(expected) || Hash(packet) != packetHash) throw new Exception("Owned outputs alias or encoded source differs.");
        }
        File.WriteAllBytes(Path.Combine(folder, "input.jpg"), packet);
        File.WriteAllBytes(Path.Combine(folder, "expected.rgba"), expected);
        File.WriteAllBytes(Path.Combine(folder, "projection.ycbcr"), projection);
        var blockBytes = blocks.SelectMany(b => b).ToArray(); File.WriteAllBytes(Path.Combine(folder, "blocks.ycbcr"), blockBytes);
        File.WriteAllText(Path.Combine(folder, "preparation.json"), JsonSerializer.Serialize(new
        {
            schemaVersion = 1, project = "Lokad.Image", width, height, bitDepth = 8, horizontalSampling = 4, verticalSampling = 1,
            recipe = "Existing original513x259 RGBA8 flattened over0x204060; RGB->YCbCr projection, clamped8x8 fullY and32x8 quarterwidthCbCr rounded means; authoredT81DC-only/unitquantization H4V1. Independent expanded authored component rasters, actual coded dimensions, native byte pre-expansion where required, normalized Q14 horizontal/Q12 vertical filters and rational BT.601 Q13 color; exact native pixels.",
            rasterSha256 = Hash(original), opaqueRgbaSha256 = Hash(opaque), projectionSha256 = Hash(projection), blocksSha256 = Hash(blockBytes),
            encodedBytes = packet.Length, encodedSha256 = packetHash, rgbaSha256 = Hash(expected),
            exactManagedPixels = true, sourceUnchanged = true, independentOwnedOutputs = true, noTimingClaim = true
        }, new JsonSerializerOptions { WriteIndented = true }));


        // Validate original decoded component samples before actual-grid color conversion.
        var planeArgs = new[] { "-hide_banner", "-loglevel", "error", "-threads:v", "1", "-thread_type:v", "0", "-noautorotate", "-f", "image2pipe", "-c:v", "mjpeg", "-i", "pipe:0", "-frames:v", "1", "-threads:v", "1", "-c:v", "rawvideo", "-pix_fmt", "yuvj411p", "-f", "rawvideo", "pipe:1" };
        var planes = OracleProcess.Run(state.Ffmpeg, planeArgs, packet);
        if (planes.Length != (width + 258) * height) throw new Exception("Native411 plane lengths differ.");
        var planeOffset = 0;
        for (var c = 0; c < 3; c++)
        {
            var cw = c == 0 ? width : 129;
            for (var y = 0; y < height; y++)
            for (var x = 0; x < cw; x++)
                if (planes[planeOffset + y * cw + x] != blocks[c][y / 8 * columns[c] + x / 8]) throw new Exception("Native411 planes differ.");
            planeOffset += cw * height;
        }
        var outputs = WarmJpegFixedReference.ValidateQuarterNative(state, folder, packet, expected);
        File.WriteAllText(Path.Combine(folder, "native-validation.json"), JsonSerializer.Serialize(new
        {
            schemaVersion = 1, project = "Lokad.Image", inputSha256 = packetHash, ffmpegSha256 = JpegBenchmarkCase.HashFile(state.Ffmpeg),
            nativePlanesExact = true, planeArguments = planeArgs, nativePlaneSha256 = Hash(planes), actualCodedGridNormalization = true,
            normalization = "Original software MJPEG component planes converted directly at coded513x259 dimensions with the public full-range BT.601/bilinear/accurate/full-chroma/in_h_chr_pos0/in_v_chr_pos0 policy. Native state reset, component decode, normalization, fresh owned RGBA acquisition and release are timed. No padded-grid remapping, recovered chroma raster or in-place compaction.",
            passed = true, noTimingClaim = true, outputs
        }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine("Prepared original JPEG411 with exact independent/native pixels at coded dimensions.");
    }
}
