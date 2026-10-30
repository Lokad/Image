using System.Numerics;

// Untimed, independent reference for the original DC-only JPEG controls. Expand
// authored block means into whole component rasters, then apply the declared
// sample-grid and BT.601 precision boundaries. Never read decoded library pixels.
internal static class WarmJpegFixedReference
{
    internal const string ColorConversion = "scale=in_range=pc:out_range=pc:in_color_matrix=bt601:out_color_matrix=bt601:flags=bilinear+accurate_rnd+full_chroma_int:in_h_chr_pos=0:in_v_chr_pos=0";
    private static readonly int Red = Coefficient(1402, 1000);
    private static readonly int Blue = Coefficient(1772, 1000);
    private static readonly int GreenU = Coefficient(2 * 114 * 886, 1000 * 587, negative: true);
    private static readonly int GreenV = Coefficient(2 * 299 * 701, 1000 * 587, negative: true);

    private static int Coefficient(long numerator, long denominator, bool negative = false)
    {
        // Exact rational BT.601: round studio-range Q16, remove the range
        // expansion by integer truncation, then round the signed Q13 value.
        BigInteger n = (BigInteger)numerator * 255 * 65536, d = (BigInteger)denominator * 224;
        var studio = BigInteger.DivRem(n, d, out var remainder);
        if (remainder * 2 >= d) studio++;
        var full = studio * 224 / 255;
        return (int)(((negative ? -full : full) + 4) >> 3);
    }

    internal static byte[] Ycck(byte[][] blocks, int[] columns, int width, int height)
    {
        var cw = (width + 1) / 2; var ch = (height + 1) / 2;
        var planes = blocks.Select((b, c) => Expand(b, columns[c], c is 1 or 2 ? cw : width, c is 1 or 2 ? ch : height)).ToArray();
        var rgba = new byte[width * height * 4];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var pixel = y * width + x; var black = planes[3][pixel];
            var left = x / 2; var right = Math.Min((x + 1) / 2, cw - 1);
            var top = y / 2; var bottom = Math.Min((y + 1) / 2, ch - 1);
            int Sample(int c) => ((planes[c][top * cw + left] + planes[c][top * cw + right]) / 2
                + (planes[c][bottom * cw + left] + planes[c][bottom * cw + right]) / 2) / 2;
            long Product(int value) => (long)value * black * ((1 << 16) / 255) >> 16;
            Color(rgba.AsSpan(pixel * 4, 4), Product(255 - planes[0][pixel]), Product(128 - Sample(1)), Product(128 - Sample(2)), 13);
        }
        return rgba;
    }

    internal static byte[] Quarter(byte[][] blocks, int[] columns, int width, int height, bool halfHeight)
    {
        var cw = (width + 3) / 4; var ch = halfHeight ? (height + 1) / 2 : height;
        var luma = Expand(blocks[0], columns[0], width, height);
        var chroma = new[] { Expand(blocks[1], columns[1], cw, ch), Expand(blocks[2], columns[2], cw, ch) };
        var nativeWidth = halfHeight ? (width + 1) / 2 : cw;
        if (halfHeight)
            for (var c = 0; c < chroma.Length; c++)
            {
                var expanded = new byte[nativeWidth * ch];
                for (var y = 0; y < ch; y++)
                for (var x = 0; x < nativeWidth; x++)
                    expanded[y * nativeWidth + x] = (byte)((chroma[c][y * cw + x / 2]
                        + chroma[c][y * cw + Math.Min((x + 1) / 2, cw - 1)]) / 2);
                chroma[c] = expanded;
            }
        var horizontal = Axis(nativeWidth, width, 14, halfHeight ? 16384 : 8192);
        var vertical = halfHeight ? Axis(ch, height, 12, 16384) : [];
        var rgba = new byte[width * height * 4];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var h = horizontal[x]; var v = halfHeight ? vertical[y] : new Tap(y, y, 4096, 0);
            long Sample(byte[] plane)
            {
                long Row(int row) => ((long)plane[row * nativeWidth + h.First] * h.FirstWeight
                    + (long)plane[row * nativeWidth + h.Second] * h.SecondWeight) >> 7;
                if (!halfHeight) return (Row(y) - (128 << 7)) * 4;
                return ((Row(v.First) * v.FirstWeight + Row(v.Second) * v.SecondWeight) >> 10) - (128 << 9);
            }
            var pixel = y * width + x;
            Color(rgba.AsSpan(pixel * 4, 4), luma[pixel], Sample(chroma[0]), Sample(chroma[1]), 22);
        }
        return rgba;
    }

    private readonly record struct Tap(int First, int Second, int FirstWeight, int SecondWeight);

    private static Tap[] Axis(int source, int target, int bits, int phase)
    {
        const int unit = 1 << 16;
        var step = ((long)source * unit + target / 2) / target;
        var result = new Tap[target];
        for (var i = 0; i < target; i++)
        {
            var position = i * step + step / 2 - phase;
            var first = (int)(position >> 16); var fraction = (int)(position & (unit - 1));
            if ((long)(unit - fraction) * 500 <= unit)
            {
                var only = Math.Clamp(first + 1, 0, source - 1);
                result[i] = new(only, only, 1 << bits, 0);
            }
            else
            {
                var firstWeight = (unit - fraction + (1 << (15 - bits))) >> (16 - bits);
                result[i] = new(Math.Clamp(first, 0, source - 1), Math.Clamp(first + 1, 0, source - 1), firstWeight, (1 << bits) - firstWeight);
            }
        }
        return result;
    }

    private static byte[] Expand(byte[] means, int columns, int width, int height)
    {
        var result = new byte[width * height];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++) result[y * width + x] = means[y / 8 * columns + x / 8];
        return result;
    }

    internal static object[] ValidateQuarterNative(JpegBenchmarkCase state, string folder, byte[] packet, byte[] expected)
    {
        var outputs = new List<object>();
        foreach (var scalar in new[] { false, true })
        {
            List<string> arguments = ["-hide_banner", "-loglevel", "error"];
            if (scalar) arguments.AddRange(["-cpuflags", "0"]);
            arguments.AddRange(["-threads:v", "1", "-thread_type:v", "0", "-noautorotate", "-f", "image2pipe", "-c:v", "mjpeg", "-i", "pipe:0", "-filter_threads", "1",
                "-vf", ColorConversion, "-frames:v", "1", "-threads:v", "1", "-pix_fmt", "rgba", "-f", "rawvideo", "pipe:1"]);
            var pixels = Lokad.Image.Tooling.OracleProcess.Run(state.Ffmpeg, arguments.ToArray(), packet);
            if (!pixels.AsSpan().SequenceEqual(expected)) throw new InvalidOperationException("Native actual-grid pixels differ from the independent fixed-point reference.");
            var name = scalar ? "scalar.rgba" : "native.rgba";
            File.WriteAllBytes(Path.Combine(folder, name), pixels);
            outputs.Add(new { scalar, arguments, file = name, sha256 = JpegBenchmarkCase.Hash(pixels), maximum = 0, rmse = 0d, alphaExact = true, passed = true });
        }
        return outputs.ToArray();
    }

    private static void Color(Span<byte> rgba, long y, long u, long v, int bits)
    {
        var center = (y << bits) + (1L << (bits - 1));
        rgba[0] = (byte)Math.Clamp((center + v * Red) >> bits, 0, 255);
        rgba[1] = (byte)Math.Clamp((center + u * GreenU + v * GreenV) >> bits, 0, 255);
        rgba[2] = (byte)Math.Clamp((center + u * Blue) >> bits, 0, 255); rgba[3] = 255;
    }
}
