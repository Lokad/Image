using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class WideBicubicResizeTests
{
    [Theory]
    [InlineData(53,37,12,8)]
    [InlineData(99,83,17,13)]
    [InlineData(513,31,128,7)]
    [InlineData(59,41,9,7)]
    [InlineData(67,51,8,7)]
    public void WideRowsMatchDirectEvaluationAtAdmissionBoundary(int sourceWidth,int sourceHeight,int width,int height)
    {
        using var source=OriginalPixels.Buffer(sourceWidth,sourceHeight,8);
        var input=OriginalPixels.Bytes(source);
        var baselinePeak=input.Length+(long)width*height*4+4096;
        var workspace=1420L*width+32L*sourceWidth+256;
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
        Assert.Equal(expected,OriginalPixels.Bytes(direct));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void WiderNegativeLobesAndTinyAlphaKeepExactPixels(bool transparent)
    {
        using var source=ImageBuffer.Create(53,37,8);
        for(var y=0;y<source.Height;y++)
        {
            var row=source.GetRow8(y);
            for(var x=0;x<source.Width;x++)
            {
                row[x*4]=(byte)(x*53+y*31);row[x*4+1]=(byte)(255-x*19-y*47);row[x*4+2]=(byte)(x*97+y*11);
                row[x*4+3]=transparent?(byte)0:(x+y)%11==0?(byte)255:(x+y)%3==0?(byte)1:(byte)0;
            }
        }
        using var direct=ImageTransforms.Resize(source,12,8,ResizeFilter.Bicubic,
            new ImageLimits {MaxWorkingBytes=(long)source.Stride*source.Height+12*8*4+4096});
        using var actual=ImageTransforms.Resize(source,12,8,ResizeFilter.Bicubic);
        Assert.Equal(OriginalPixels.Bytes(direct),OriginalPixels.Bytes(actual));
    }
}
