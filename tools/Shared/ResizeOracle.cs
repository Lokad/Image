namespace Lokad.Image.Tooling;

internal static class ResizeOracle
{
    // Opaque RGBA comparison profile. FFmpeg documents bicubic B/C parameters;
    // B=0,C=0.5 matches the public Catmull-Rom kernel. Downsizing widens support.
    // This does not establish transparent-alpha or high-frequency upscale parity.
    public static string Filter(int width, int height, ResizeFilter filter)
    {
        var algorithm = filter switch
        {
            ResizeFilter.Nearest => "neighbor", ResizeFilter.Bilinear => "bilinear", ResizeFilter.Bicubic => "bicubic",
            _ => throw new ArgumentOutOfRangeException(nameof(filter))
        };
        return $"format=rgba,scale={width}:{height}:flags={algorithm}+accurate_rnd+bitexact:param0=0:param1=0.5:gamma=0:sws_dither=none,format=rgba";
    }

    public static string[] Arguments(int sourceWidth, int sourceHeight, int width, int height, ResizeFilter filter)
        => ["-hide_banner", "-loglevel", "error", "-cpuflags", "0", "-f", "rawvideo", "-pixel_format", "rgba",
            "-video_size", $"{sourceWidth}x{sourceHeight}", "-i", "pipe:0", "-filter_threads", "1", "-vf", Filter(width, height, filter),
            "-frames:v", "1", "-threads:v", "1", "-pix_fmt", "rgba", "-f", "rawvideo", "pipe:1"];

    // Keep RGB and alpha separate: alpha-aware scaling can clear hidden RGB.
    // zscale point avoids swscale's large-axis coordinate quantization. Explicit
    // integer planar formats preserve all 8/16-bit samples without conversion.
    public static string NearestFilter(int width, int height, int depth)
    {
        if (width < 1 || height < 1 || depth is not (8 or 16)) throw new ArgumentException("Invalid nearest oracle geometry.");
        var rgb = depth == 16 ? "gbrp16le" : "gbrp";
        var planar = depth == 16 ? "gbrap16le" : "gbrap"; var packed = depth == 16 ? "rgba64le" : "rgba";
        var point = $"zscale=w={width}:h={height}:filter=point:rangein=full:range=full:matrixin=gbr:matrix=gbr:dither=none,format={rgb},setsar=1";
        // extractplanes outputs the selected RGB flags in r/g/b order.
        return $"extractplanes=r+g+b+a[r][g][b][a];[g][b][r]mergeplanes=0x001020:format={rgb},{point},extractplanes=r+g+b[rs][gs][bs];"
            + $"[a]mergeplanes=0x000000:format={rgb},{point},extractplanes=g[as];"
            + $"[gs][bs][rs][as]mergeplanes=0x00102030:format={planar},format={packed}";
    }

    public static string[] NearestArguments(int sourceWidth, int sourceHeight, int width, int height, int depth, int frames = 1)
        => ["-hide_banner", "-loglevel", "error", "-cpuflags", "0", "-threads:v", "1", "-f", "rawvideo",
            "-pixel_format", depth == 16 ? "rgba64le" : "rgba", "-video_size", $"{sourceWidth}x{sourceHeight}", "-i", "pipe:0",
            "-filter_complex_threads", "1", "-filter_complex", NearestFilter(width, height, depth),
            "-frames:v", frames.ToString(System.Globalization.CultureInfo.InvariantCulture), "-fps_mode", "passthrough",
            "-threads:v", "1", "-pix_fmt", depth == 16 ? "rgba64le" : "rgba", "-f", "rawvideo", "pipe:1"];
}
