namespace Lokad.Image.Tooling;

internal static class JpegOracle
{
    private const string ColorConversion = "scale=in_range=pc:out_range=pc:in_color_matrix=bt601:out_color_matrix=bt601:flags=bilinear+accurate_rnd+full_chroma_int:in_h_chr_pos=0:in_v_chr_pos=0";

    public static byte[] Decode(byte[] encoded, int width, int height, JpegSampling sampling, string? executable = null)
        => OracleProcess.Run(executable ?? OracleProcess.Ffmpeg, DecodeArguments(width, height, sampling), encoded);

    public static string[] EncodeArguments(int width, int height, JpegSampling sampling, int qscale)
    {
        // Native MJPEG supports three-component YUV only. Grayscale sources are
        // encoded with neutral-chroma 4:4:4 and this difference is recorded.
        var format = sampling switch { JpegSampling.YCbCr420 => "yuvj420p", JpegSampling.YCbCr422 => "yuvj422p", _ => "yuvj444p" };
        var horizontal = sampling is JpegSampling.YCbCr420 or JpegSampling.YCbCr422 ? 128 : 0;
        var vertical = sampling == JpegSampling.YCbCr420 ? 128 : 0;
        return ["-hide_banner", "-loglevel", "error", "-cpuflags", "0", "-f", "rawvideo", "-pixel_format", "rgba", "-video_size", $"{width}x{height}", "-i", "pipe:0",
            "-filter_threads", "1", "-vf", $"scale=in_range=pc:out_range=pc:out_color_matrix=bt601:flags=area+accurate_rnd+bitexact:out_h_chr_pos={horizontal}:out_v_chr_pos={vertical}",
            "-frames:v", "1", "-threads:v", "1", "-c:v", "mjpeg", "-huffman", "optimal", "-pix_fmt", format, "-q:v", qscale.ToString(System.Globalization.CultureInfo.InvariantCulture), "-f", "image2pipe", "pipe:1"];
    }

    public static string[] DecodeArguments(int width, int height, JpegSampling sampling)
        => ["-hide_banner", "-loglevel", "error", "-cpuflags", "0", "-noautorotate", "-f", "image2pipe", "-i", "pipe:0",
            "-filter_complex", Filter(width, height, sampling), "-frames:v", "1", "-threads:v", "1", "-pix_fmt", "rgba", "-f", "rawvideo", "pipe:1"];

    public static string Filter(int width, int height, JpegSampling sampling)
    {
        if (sampling is JpegSampling.Grayscale or JpegSampling.YCbCr444) return ColorConversion;
        var h = 2; var v = sampling == JpegSampling.YCbCr420 ? 2 : 1;
        // Chroma dimensions are ceil(luma/sampling). Expanding the planes by the
        // sampling factors and then cropping keeps their original grid at odd
        // dimensions. Direct YUV-to-RGB scale would stretch ceil(width/2) to width.
        // See FFmpeg's documented extractplanes, scale, crop and mergeplanes filters.
        var expand = $"scale=iw*{h}:ih*{v}:flags=bilinear+accurate_rnd,crop={width}:{height},setsar=1";
        return $"extractplanes=y+u+v[y][u][v];[u]{expand}[ue];[v]{expand}[ve];"
            + $"[y]setsar=1[ye];[ye][ue][ve]mergeplanes=0x001020:format=yuv444p,{ColorConversion}";
    }
}
