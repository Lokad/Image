using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class WideBicubicResizeTests
{
    [Theory]
    [InlineData(8,53,37,12,8)]
    [InlineData(16,53,37,12,8)]
    [InlineData(8,99,83,17,13)]
    [InlineData(16,99,83,17,13)]
    [InlineData(8,513,31,128,7)]
    [InlineData(16,513,31,128,7)]
    [InlineData(8,59,41,9,7)]
    [InlineData(16,59,41,9,7)]
    [InlineData(8,67,51,8,7)]
    [InlineData(16,67,51,8,7)]
    [InlineData(8,99,83,9,7)]
    [InlineData(16,99,83,9,7)]
    [InlineData(8,513,65,64,7)]
    [InlineData(16,513,65,64,7)]
    [InlineData(8,53,41,5,3)]
    [InlineData(16,53,41,5,3)]
    [InlineData(8,99,81,6,5)]
    [InlineData(16,99,81,6,5)]
    [InlineData(8,101,83,7,6)]
    [InlineData(16,101,83,7,6)]
    public void WideRowsMatchDirectEvaluationAtAdmissionBoundary(int depth,int sourceWidth,int sourceHeight,int width,int height)
    {
        using var source=OriginalPixels.Buffer(sourceWidth,sourceHeight,depth);
        var input=OriginalPixels.Bytes(source);
        var baselinePeak=input.Length+(long)width*height*4*(depth/8)+4096;
        var beyond32=Math.Ceiling(4d*sourceWidth/width)+1>32 || Math.Ceiling(4d*sourceHeight/height)+1>32;
        var workspace=beyond32?2828L*width+32L*sourceWidth+512:1420L*width+32L*sourceWidth+256;
        using var direct=ImageTransforms.Resize(source,width,height,ResizeFilter.Bicubic,
            new ImageLimits {MaxWorkingBytes=baselinePeak});
        var expected=OriginalPixels.Bytes(direct);
        foreach(var peak in new[]{baselinePeak+workspace-1,baselinePeak+workspace})
        {
            using var actual=ImageTransforms.Resize(source,width,height,ResizeFilter.Bicubic,
                new ImageLimits {MaxWorkingBytes=peak});
            Assert.Equal(expected,OriginalPixels.Bytes(actual));
            if(depth==8)actual.GetRow8(0)[0]^=255;else actual.GetRow16(0)[0]^=65535;
            Assert.Equal(expected,OriginalPixels.Bytes(direct));
            Assert.Equal(input,OriginalPixels.Bytes(source));
        }
        if(depth==8)source.GetRow8(sourceHeight-1)[(sourceWidth-1)*4+3]^=255;
        else source.GetRow16(sourceHeight-1)[(sourceWidth-1)*4+3]^=65535;
        using var changed=ImageTransforms.Resize(source,width,height,ResizeFilter.Bicubic);
        using var changedDirect=ImageTransforms.Resize(source,width,height,ResizeFilter.Bicubic,
            new ImageLimits {MaxWorkingBytes=baselinePeak});
        Assert.Equal(OriginalPixels.Bytes(changedDirect),OriginalPixels.Bytes(changed));
        Assert.Equal(expected,OriginalPixels.Bytes(direct));
    }

    [Theory]
    [InlineData(8,false,12,8)]
    [InlineData(8,true,12,8)]
    [InlineData(16,false,12,8)]
    [InlineData(16,true,12,8)]
    [InlineData(8,false,4,3)]
    [InlineData(8,true,4,3)]
    [InlineData(16,false,4,3)]
    [InlineData(16,true,4,3)]
    public void WiderNegativeLobesAndTinyAlphaKeepExactPixels(int depth,bool transparent,int width,int height)
    {
        using var source=ImageBuffer.Create(53,37,depth);
        for(var y=0;y<source.Height;y++)for(var x=0;x<source.Width;x++)
        {
            if(depth==8)
            {
                var row=source.GetRow8(y);
                row[x*4]=(byte)(x*53+y*31);row[x*4+1]=(byte)(255-x*19-y*47);row[x*4+2]=(byte)(x*97+y*11);
                row[x*4+3]=transparent?(byte)0:(x+y)%11==0?(byte)255:(x+y)%3==0?(byte)1:(byte)0;
            }
            else
            {
                var row=source.GetRow16(y);
                row[x*4]=65535;row[x*4+1]=(ushort)(65535-x*1913-y*4703);row[x*4+2]=(ushort)(x*9701+y*1103);
                row[x*4+3]=transparent?(ushort)0:(x+y)%11==0?(ushort)65535:(x+y)%3==0?(ushort)1:(ushort)0;
            }
        }
        using var direct=ImageTransforms.Resize(source,width,height,ResizeFilter.Bicubic,
            new ImageLimits {MaxWorkingBytes=(long)source.Stride*source.Height+width*height*4*(depth/8)+4096});
        using var actual=ImageTransforms.Resize(source,width,height,ResizeFilter.Bicubic);
        Assert.Equal(OriginalPixels.Bytes(direct),OriginalPixels.Bytes(actual));
    }
}
