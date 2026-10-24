using Lokad.Image.Tooling;

namespace Lokad.Image.DifferentialTests;

public sealed class Vp8FfmpegTests
{
    [Theory]
    [InlineData(0, false)] [InlineData(1, false)] [InlineData(2, false)] [InlineData(3, false)]
    [InlineData(0, true)] [InlineData(1, true)] [InlineData(2, true)] [InlineData(3, true)]
    public void AuthoredRawAndCompressedAlphaFiltersInteroperateExactly(int filter, bool compressed)
    {
        var opaque = WebpOracle.Encode(OriginalPixels.Generate(31,23,8,opaque:true),31,23,6,75);
        var encoded = AuthoredAlpha.Wrap(opaque,31,23,filter,compressed:compressed); var expected = WebpOracle.Decode(encoded,31,23,true);
        using var actual = ImageCodec.Decode(encoded); var alpha = AuthoredAlpha.Samples(31,23);
        for (var y=0;y<23;y++)
        for (var x=0;x<31;x++)
        {
            Assert.Equal(alpha[y*31+x],expected[(y*31+x)*4+3]); Assert.Equal(alpha[y*31+x],actual.GetRow8(y)[x*4+3]);
        }
    }

    [Theory]
    [InlineData(0,0)] [InlineData(0,3)] [InlineData(1,1)] [InlineData(2,2)] [InlineData(3,3)]
    public void VersionsAndVideoScaleHintsInteroperateAsStillPixels(int version,int scale)
    {
        var encoded = WebpOracle.Encode(OriginalPixels.Generate(31,23,8,opaque:true),31,23,6,75);
        encoded[20] = (byte)((encoded[20]&~14)|(version<<1));
        encoded[27] = (byte)((encoded[27]&63)|(scale<<6)); encoded[29] = (byte)((encoded[29]&63)|(scale<<6));
        using var actual = ImageCodec.Decode(encoded);
        var expected = WebpOracle.Decode(encoded,31,23,false); var samples = OriginalPixels.Bytes(actual);
        Assert.Equal(expected, samples);
    }

    public static IEnumerable<object[]> Profiles()
    {
        foreach (var size in new[] { (1, 1), (1, 17), (17, 1), (31, 23), (129, 67) })
            foreach (var effort in new[] { 0, 4, 6 })
                foreach (var quality in new[] { 1, 30, 75, 100 })
                    foreach (var opaque in new[] { false, true }) yield return [size.Item1, size.Item2, effort, quality, opaque];
    }

    [Theory]
    [MemberData(nameof(Profiles))]
    public void DecodeMatchesExplicitFfmpegColorConversion(int width, int height, int effort, int quality, bool opaque)
    {
        var source = OriginalPixels.Generate(width, height, 8, opaque: opaque);
        var encoded = WebpOracle.Encode(source, width, height, effort, quality);
        var info = ImageCodec.Identify(encoded);
        using var decoded = ImageCodec.Decode(encoded);
        Assert.Equal((width, height, 8), (decoded.Width, decoded.Height, decoded.BitDepth));
        var expected = WebpOracle.Decode(encoded, width, height, info.HasAlpha);
        Assert.Equal(expected, OriginalPixels.Bytes(decoded));
    }
}
