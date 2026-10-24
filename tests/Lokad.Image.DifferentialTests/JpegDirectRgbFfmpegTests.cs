using Lokad.Image.Tooling;

namespace Lokad.Image.DifferentialTests;

public sealed class JpegDirectRgbFfmpegTests
{
    [Theory] [InlineData(422)] [InlineData(420)]
    public void FrozenDirectRgbAndCallerGrayMatchTheirApprovedNormalizedReference(int sampling)
    {
        const int width = 513, height = 259, pixels = width * height;
        var control = OriginalDirectRgbControls.Load(sampling);
        var arguments = JpegOracle.DecodeArguments(width, height, JpegSampling.YCbCr444);
        arguments[Array.IndexOf(arguments, "-filter_complex") + 1] = sampling == 422 ? "format=rgba"
            : "extractplanes=y+u+v[r][g][b];[g]scale=iw*2:ih*2:flags=bilinear+accurate_rnd,crop=513:259,setsar=1[ge];"
            + "[b]scale=iw*2:ih*2:flags=bilinear+accurate_rnd,crop=513:259,setsar=1[be];"
            + "[r]setsar=1[re];[ge][be][re]mergeplanes=0x001020:format=gbrp,format=rgba";
        var native = OracleProcess.Run(OracleProcess.Ffmpeg, arguments, control.Encoded);
        if (sampling == 422) native = OriginalDirectRgbControls.Center422(native);
        Assert.Equal(control.RgbaSha256, OriginalDirectRgbControls.Hash(native));
        using var image = ImageCodec.Decode(control.Encoded); Assert.Equal(native, OriginalPixels.Bytes(image));
        var rgb = new byte[pixels * 3]; ImageCodec.DecodeJpeg(control.Encoded, rgb, JpegSampleFormat.Rgb);
        Assert.Equal(control.RgbSha256, OriginalDirectRgbControls.Hash(rgb));
        var gbr = new byte[pixels * 3];
        for (var i = 0; i < pixels; i++) { gbr[i] = native[i * 4 + 1]; gbr[pixels + i] = native[i * 4 + 2]; gbr[pixels * 2 + i] = native[i * 4]; }
        var grayNative = OracleProcess.Run(OracleProcess.Ffmpeg,
            ["-hide_banner", "-loglevel", "error", "-cpuflags", "0", "-f", "rawvideo", "-pixel_format", "gbrp", "-video_size", "513x259", "-i", "pipe:0",
             "-filter_threads", "1", "-vf", "scale=in_range=pc:out_range=pc:in_color_matrix=bt601:out_color_matrix=bt601:flags=bilinear+accurate_rnd+full_chroma_int:in_h_chr_pos=0:in_v_chr_pos=0",
             "-frames:v", "1", "-threads:v", "1", "-pix_fmt", "gray", "-f", "rawvideo", "pipe:1"], gbr);
        Assert.Equal(control.GraySha256, OriginalDirectRgbControls.Hash(grayNative));
        var gray = new byte[pixels]; ImageCodec.DecodeJpeg(control.Encoded, gray, JpegSampleFormat.Grayscale); Assert.Equal(grayNative, gray);
    }
}
