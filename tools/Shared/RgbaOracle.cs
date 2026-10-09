namespace Lokad.Image.Tooling;

internal static class RgbaOracle
{
    // First calculate the public nearest-integer byte value and place it at the
    // center of swscale's byte conversion cell (byte * 256), then disable dithering.
    // This keeps swscale's native 16-to-8 format conversion from choosing a
    // different rounding policy. The input remains full-precision RGBA16.
    public const string Normalize16Filter = "lutrgb=r='floor((val+128)/257)*256':g='floor((val+128)/257)*256':b='floor((val+128)/257)*256':a='floor((val+128)/257)*256',scale=flags=accurate_rnd+bitexact:sws_dither=none,format=rgba";

    public static string[] Normalize16Arguments(int width, int height)
        => ["-hide_banner", "-loglevel", "error", "-cpuflags", "0", "-f", "rawvideo", "-pixel_format", "rgba64le",
            "-video_size", $"{width}x{height}", "-i", "pipe:0", "-filter_threads", "1", "-vf", Normalize16Filter,
            "-frames:v", "1", "-threads:v", "1", "-pix_fmt", "rgba", "-f", "rawvideo", "pipe:1"];
}
