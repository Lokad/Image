using Lokad.Image.Tooling;

namespace Lokad.Image.DifferentialTests;

public sealed class AuthoredVp8FfmpegTests
{
    public static IEnumerable<object[]> Cases() => AuthoredVp8.Cases().Select(frame => new object[] { frame.Name });

    [Theory]
    [MemberData(nameof(Cases))]
    public void AuthoredSyntaxMatchesIndependentFfmpegDecode(string name)
    {
        var frame = AuthoredVp8.Cases().Single(frame => frame.Name == name);
        var encoded = AuthoredVp8.Build(frame); var expected = WebpOracle.Decode(encoded, frame.Width, frame.Height, false);
        using var actual = ImageCodec.Decode(encoded); var samples = OriginalPixels.Bytes(actual);
        Assert.Equal((frame.Width, frame.Height, 8), (actual.Width, actual.Height, actual.BitDepth));
        Assert.Equal(expected.Length, samples.Length);
        Assert.Equal(expected, samples);
    }
}
