using Lokad.Image.Tooling;

namespace Lokad.Image.DifferentialTests;

public sealed class FfmpegTests
{
    [Theory]
    [InlineData(8)]
    [InlineData(16)]
    public void ManagedEncodeAndExactTransformsInteroperate(int depth)
    {
        using var source = OriginalPixels.Buffer(9, 7, depth);
        using var encoded = new MemoryStream();
        ImageCodec.EncodePng(source, encoded);
        var format = depth == 8 ? "rgba" : "rgba64le";
        using var flipped = ImageTransforms.FlipHorizontal(source);
        using var rotated = ImageTransforms.Rotate(flipped, QuarterTurn.Clockwise90);
        var output = OracleProcess.Run(OracleProcess.Ffmpeg,
            ["-hide_banner", "-loglevel", "error", "-cpuflags", "0", "-noautorotate", "-f", "image2pipe", "-i", "pipe:0",
             "-vf", "hflip,transpose=clock", "-frames:v", "1", "-threads:v", "1", "-pix_fmt", format, "-f", "rawvideo", "pipe:1"], encoded.ToArray());
        Assert.Equal(OriginalPixels.Bytes(rotated), output);
    }

    [Theory]
    [InlineData("mjpeg", "Jpeg")]
    [InlineData("libwebp", "WebP")]
    public void IdentifyIndependentCodecOutput(string codec, string format)
    {
        var data = OracleProcess.Run(OracleProcess.Ffmpeg,
            ["-hide_banner", "-loglevel", "error", "-f", "rawvideo", "-pixel_format", "rgba", "-video_size", "9x7",
             "-i", "pipe:0", "-frames:v", "1", "-threads:v", "1", "-c:v", codec, "-f", "image2pipe", "pipe:1"],
            OriginalPixels.Generate(9, 7, 8, opaque: true));
        var info = ImageCodec.Identify(data);
        Assert.Equal(format, info.Format.ToString()); Assert.Equal((9, 7), (info.Width, info.Height));
        using (var decoded = ImageCodec.Decode(data))
        {
            Assert.Equal((9, 7), (decoded.Width, decoded.Height));
        }
    }
}
