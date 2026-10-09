namespace Lokad.Image.Tooling;

// Development-only native evaluation of SPEC.md's single-round JPEG background
// contract. Point lookups use documented geq functions, with no interpolation:
// https://ffmpeg.org/ffmpeg-filters.html#geq
internal static class JpegFlattenOracle
{
    public static string Filter(int depth, uint background)
    {
        if (depth is not (8 or 16) || background > 0xFFFFFF) throw new ArgumentException("Invalid JPEG flattening declaration.");
        var maximum = depth == 16 ? 65535 : 255; var denominator = (long)maximum * maximum;
        var expressions = new string[3];
        for (var channel = 0; channel < 3; channel++)
        {
            var name = "rgb"[channel]; var bg = (background >> (16 - channel * 8)) & 255;
            expressions[channel] = FormattableString.Invariant($"{name}='floor(({name}(X,Y)*alpha(X,Y)*255+{bg}*({maximum}-alpha(X,Y))*{maximum}+{denominator / 2})/{denominator})*{(depth == 16 ? 256 : 1)}'");
        }
        // All integer products fit exactly in binary64. Compute the target byte
        // before conversion; byte*256 centers the 16-bit swscale conversion cell.
        return $"format={(depth == 16 ? "gbrap16le" : "gbrap")},geq=" + string.Join(':', expressions)
            + FormattableString.Invariant($":a={maximum}:i=nearest")
            + (depth == 16 ? ",scale=flags=accurate_rnd+bitexact:sws_dither=none,format=rgba" : ",format=rgba");
    }

    public static string[] Arguments(int width, int height, int depth, uint background)
        => ["-hide_banner", "-loglevel", "error", "-threads:v", "1", "-f", "rawvideo",
            "-pixel_format", depth == 16 ? "rgba64le" : "rgba", "-video_size", FormattableString.Invariant($"{width}x{height}"),
            "-i", "pipe:0", "-filter_threads", "1", "-vf", Filter(depth, background),
            "-frames:v", "1", "-threads:v", "1", "-pix_fmt", "rgba", "-f", "rawvideo", "pipe:1"];
}
