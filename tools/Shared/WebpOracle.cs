namespace Lokad.Image.Tooling;

internal static class WebpOracle
{
    public static byte[] Encode(byte[] rgba, int width, int height, int effort, int quality, string? executable = null)
        => OracleProcess.Run(executable ?? OracleProcess.Ffmpeg, EncodeArguments(width, height, effort, quality), rgba);

    public static string[] EncodeArguments(int width, int height, int effort, int quality)
        => ["-hide_banner", "-loglevel", "error", "-cpuflags", "0", "-f", "rawvideo", "-pix_fmt", "rgba", "-s", $"{width}x{height}",
            "-i", "pipe:0", "-frames:v", "1", "-threads:v", "1", "-c:v", "libwebp", "-lossless", "0", "-quality", quality.ToString(),
            "-compression_level", effort.ToString(), "-f", "webp", "pipe:1"];

    public static byte[] Decode(byte[] encoded, int width, int height, bool alpha, string? executable = null)
        => OracleProcess.Run(executable ?? OracleProcess.Ffmpeg, DecodeArguments(width, height, alpha), encoded);

    public static string[] DecodeArguments(int width, int height, bool alpha)
        => ["-hide_banner", "-loglevel", "error", "-cpuflags", "0", "-filter_complex_threads", "1", "-noautorotate",
            "-f", "image2pipe", "-i", "pipe:0", "-filter_complex", Filter(width, height, alpha),
            "-frames:v", "1", "-threads:v", "1", "-pix_fmt", "rgba", "-f", "rawvideo", "pipe:1"];

    public static string[] YuvArguments(bool alpha)
        => ["-hide_banner", "-loglevel", "error", "-cpuflags", "0", "-noautorotate", "-f", "image2pipe", "-i", "pipe:0",
            "-frames:v", "1", "-threads:v", "1", "-pix_fmt", alpha ? "yuva420p" : "yuv420p", "-f", "rawvideo", "pipe:1"];

    public static string Filter(int width, int height, bool alpha)
    {
        var expand = $"scale=iw*2:ih*2:flags=bilinear+accurate_rnd,crop={width}:{height},setsar=1";
        var color = ColorFilter;
        return $"extractplanes=y+u+v{(alpha ? "+a" : "")}[y][u][v]{(alpha ? "[a]" : "")};"
            + $"[u]{expand}[ue];[v]{expand}[ve];[y]setsar=1[ye];"
            + $"[ye][ue][ve]mergeplanes=0x001020:format=yuv444p,{color}"
            + (alpha ? "[color];[color][a]alphamerge,format=rgba" : "");
    }

    // The registered scalar 8-bit RGB conversion wraps blue at extreme legal VP8
    // samples (Y=255,U=255). Saturate in 16-bit RGB, then explicitly reduce precision.
    public static string ColorFilter => "scale=in_range=tv:out_range=pc:in_color_matrix=bt601:out_color_matrix=bt601:flags=bilinear+accurate_rnd+full_chroma_int:in_h_chr_pos=0:in_v_chr_pos=0,format=rgba64le,scale=flags=accurate_rnd+bitexact:sws_dither=none,format=rgba";
}
