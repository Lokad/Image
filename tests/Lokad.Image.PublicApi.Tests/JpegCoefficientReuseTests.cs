using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class JpegCoefficientReuseTests
{
    [Fact]
    public void EntropyAndTransformFailuresDoNotPoisonLaterDenseOrSparseDecodes()
    {
        var encoded=AuthoredJpeg.DirectRgb(adobe:false);
        var original=(byte[])encoded.Clone();
        for(var iteration=0;iteration<4;iteration++)
        {
            Assert.Throws<ImageDataException>(()=>ImageCodec.Decode(AuthoredJpeg.EobRun(1,crossRestart:true)));
            using var first=ImageCodec.Decode(encoded);
            using var second=ImageCodec.Decode(encoded);
            first.GetRow8(0)[0]=0;
            for(var y=0;y<8;y++)for(var x=0;x<8;x++)
                Assert.Equal(new byte[]{45,105,205,255},second.GetRow8(y).Slice(x*4,4).ToArray());
            using var sparse=ImageCodec.Decode(AuthoredJpeg.EmptyEobRun());
            for(var y=0;y<8;y++)for(var x=0;x<24;x++)
                Assert.Equal(new byte[]{128,128,128,255},sparse.GetRow8(y).Slice(x*4,4).ToArray());
            Assert.Equal(original,encoded);
        }
    }
}
