using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class CachedBicubicResizeTests
{
    [Theory]
    [InlineData(33,17,16,8)]
    [InlineData(37,19,15,7)]
    [InlineData(61,39,18,11)]
    [InlineData(513,31,256,15)]
    public void SeparableRowsMatchOriginalOrderAtCacheAdmissionBoundary(int sourceWidth,int sourceHeight,int width,int height)
    {
        using var source=OriginalPixels.Buffer(sourceWidth,sourceHeight,8);
        var input=OriginalPixels.Bytes(source);
        var baselinePeak=input.Length+(long)width*height*4+4096;
        var workspace=716L*width+32L*sourceWidth+128;
        using var direct=ImageTransforms.Resize(source,width,height,ResizeFilter.Bicubic,
            new ImageLimits {MaxWorkingBytes=baselinePeak});
        var expected=OriginalPixels.Bytes(direct);
        foreach(var peak in new[]{baselinePeak+workspace-1,baselinePeak+workspace})
        {
            using var actual=ImageTransforms.Resize(source,width,height,ResizeFilter.Bicubic,
                new ImageLimits {MaxWorkingBytes=peak});
            Assert.Equal(expected,OriginalPixels.Bytes(actual));
            actual.GetRow8(0)[0]^=255;
            Assert.Equal(expected,OriginalPixels.Bytes(direct));
            Assert.Equal(input,OriginalPixels.Bytes(source));
        }
        source.GetRow8(sourceHeight-1)[(sourceWidth-1)*4+3]^=255;
        using var changed=ImageTransforms.Resize(source,width,height,ResizeFilter.Bicubic);
        using var changedDirect=ImageTransforms.Resize(source,width,height,ResizeFilter.Bicubic,
            new ImageLimits {MaxWorkingBytes=baselinePeak});
        Assert.Equal(OriginalPixels.Bytes(changedDirect),OriginalPixels.Bytes(changed));
        Assert.NotEqual(expected,OriginalPixels.Bytes(changed));
        Assert.Equal(expected,OriginalPixels.Bytes(direct));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NegativeLobesAndTinyAlphaRetainOriginalQuantization(bool transparent)
    {
        using var source=ImageBuffer.Create(17,11,8);
        for(var y=0;y<source.Height;y++)
        {
            var row=source.GetRow8(y);
            for(var x=0;x<source.Width;x++)
            {
                row[x*4]=(byte)(x*53+y*31);row[x*4+1]=(byte)(255-x*19-y*47);row[x*4+2]=(byte)(x*97+y*11);
                row[x*4+3]=transparent?(byte)0:(x+y)%7==0?(byte)255:(x+y)%3==0?(byte)1:(byte)0;
            }
        }
        using var expected=ImageTransforms.Resize(source,7,5,ResizeFilter.Bicubic,
            new ImageLimits {MaxWorkingBytes=(long)source.Stride*source.Height+7*5*4+4096});
        using var actual=ImageTransforms.Resize(source,7,5,ResizeFilter.Bicubic);
        Assert.Equal(OriginalPixels.Bytes(expected),OriginalPixels.Bytes(actual));
    }
}
