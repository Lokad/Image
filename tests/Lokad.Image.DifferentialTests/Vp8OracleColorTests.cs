using Lokad.Image.Tooling;

namespace Lokad.Image.DifferentialTests;

public sealed class Vp8OracleColorTests
{
    [Theory]
    [InlineData(16, 128, 128, 0, 0, 0)]
    [InlineData(235, 128, 128, 255, 255, 255)]
    [InlineData(255, 255, 128, 255, 229, 255)]
    [InlineData(255, 255, 255, 255, 125, 255)]
    [InlineData(0, 0, 0, 0, 136, 0)]
    [InlineData(255, 0, 0, 74, 255, 20)]
    public void ReferenceMatrixClipsExtremeSamplesWithoutWrapping(int y, int u, int v, int red, int green, int blue)
    {
        var actual = OracleProcess.Run(OracleProcess.Ffmpeg,
            ["-hide_banner","-loglevel","error","-cpuflags","0","-f","rawvideo","-pix_fmt","yuv444p","-s","1x1","-i","pipe:0",
             "-vf",WebpOracle.ColorFilter,"-frames:v","1","-threads:v","1","-pix_fmt","rgba","-f","rawvideo","pipe:1"],
            [(byte)y, (byte)u, (byte)v]);
        Assert.Equal(4, actual.Length); Assert.Equal(255, actual[3]);
        Assert.InRange(Math.Abs(actual[0] - red), 0, 2); Assert.InRange(Math.Abs(actual[1] - green), 0, 2); Assert.InRange(Math.Abs(actual[2] - blue), 0, 2);
    }

    [Fact]
    public void EmptyTokenPartitionsAreRejectedByBothImplementations()
    {
        foreach (var frame in AuthoredVp8.InvalidEmptyPartitions())
        {
            var encoded = AuthoredVp8.Build(frame);
            Assert.Throws<ImageDataException>(() => ImageCodec.Decode(encoded));
            var error = Assert.Throws<InvalidOperationException>(() => WebpOracle.Decode(encoded, frame.Width, frame.Height, false));
            Assert.Contains("Invalid partitions", error.Message);
        }
    }
}
