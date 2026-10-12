using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class JpegGrayscaleRenderTests
{
    [Theory]
    [InlineData(1,false)] [InlineData(1,true)] [InlineData(7,false)] [InlineData(7,true)]
    [InlineData(8,false)] [InlineData(8,true)] [InlineData(9,false)] [InlineData(9,true)]
    [InlineData(15,false)] [InlineData(15,true)] [InlineData(16,false)] [InlineData(16,true)]
    [InlineData(17,false)] [InlineData(17,true)] [InlineData(2049,false)] [InlineData(2049,true)]
    public void AnalyticalDcGridKeepsEveryLumaByteOpaqueAlphaAndIndependentOwners(int width, bool progressive)
    {
        var encoded = AuthoredJpeg.GrayscaleDcGrid(width,9,progressive); var original = encoded.ToArray();
        using var first = ImageCodec.Decode(encoded); using var second = ImageCodec.Decode(encoded);
        Assert.Equal((width,9,8,ImageOrientation.Normal),(first.Width,first.Height,first.BitDepth,first.Orientation));
        var columns = (width+7)/8;
        for (var y = 0; y < 9; y++)
        for (var x = 0; x < width; x++)
        {
            var expected = (byte)((x/8+y/8*columns)&255);
            Assert.Equal(new byte[] { expected,expected,expected,255 },first.GetRow8(y).Slice(x*4,4).ToArray());
            Assert.Equal(new byte[] { expected,expected,expected,255 },second.GetRow8(y).Slice(x*4,4).ToArray());
        }
        first.GetRow8(0)[0]^=255;
        Assert.Equal(new byte[] { 0,0,0,255 },second.GetRow8(0)[..4].ToArray());
        Assert.Equal(original,encoded);
    }
}
