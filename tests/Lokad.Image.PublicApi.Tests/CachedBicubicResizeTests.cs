using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class CachedBicubicResizeTests
{
    [Fact]
    public void FullWordProductsAndPackingPreserveUpperUnsignedRange()
    {
        using var source=ImageBuffer.Create(3,3,16);
        for(var y=0;y<3;y++)
        {
            var row=source.GetRow16(y);
            for(var x=0;x<3;x++)
            {
                row[x*4]=65535;row[x*4+1]=(ushort)(65534-x*997-y*311);row[x*4+2]=32769;
                row[x*4+3]=y==1 && x==0?(ushort)1:y==1 && x==2?(ushort)0:(ushort)65535;
            }
        }
        using var direct=ImageTransforms.Resize(source,2,2,ResizeFilter.Bicubic,
            new ImageLimits {MaxWorkingBytes=3*3*8+2*2*8+4096});
        using var actual=ImageTransforms.Resize(source,2,2,ResizeFilter.Bicubic);
        Assert.Equal(OriginalPixels.Bytes(direct),OriginalPixels.Bytes(actual));
        for(var y=0;y<2;y++)for(var x=0;x<2;x++)Assert.Equal((ushort)65535,actual.GetRow16(y)[x*4]);
    }

    [Theory]
    [InlineData(8,33,17,16,8)]
    [InlineData(16,33,17,16,8)]
    [InlineData(8,37,19,15,7)]
    [InlineData(16,37,19,15,7)]
    [InlineData(8,61,39,18,11)]
    [InlineData(16,61,39,18,11)]
    [InlineData(8,513,31,256,15)]
    [InlineData(16,513,31,256,15)]
    [InlineData(8,17,9,35,19)]
    [InlineData(16,17,9,35,19)]
    [InlineData(8,67,13,135,27)]
    [InlineData(16,67,13,135,27)]
    [InlineData(8,513,9,1027,19)]
    [InlineData(16,513,9,1027,19)]
    [InlineData(8,17,19,35,9)]
    [InlineData(16,17,19,35,9)]
    [InlineData(8,67,27,135,13)]
    [InlineData(16,67,27,135,13)]
    [InlineData(8,513,19,1027,9)]
    [InlineData(16,513,19,1027,9)]
    public void SeparableRowsMatchOriginalOrderAtCacheAdmissionBoundary(int depth,int sourceWidth,int sourceHeight,int width,int height)
    {
        using var source=OriginalPixels.Buffer(sourceWidth,sourceHeight,depth);
        var input=OriginalPixels.Bytes(source);
        var baselinePeak=input.Length+(long)width*height*4*(depth/8)+4096;
        var workspace=716L*width+32L*sourceWidth+128;
        if(width>sourceWidth && height<sourceHeight)workspace+=(width&1)*512;
        using var direct=ImageTransforms.Resize(source,width,height,ResizeFilter.Bicubic,
            new ImageLimits {MaxWorkingBytes=baselinePeak});
        var expected=OriginalPixels.Bytes(direct);
        foreach(var peak in new[]{baselinePeak+workspace-1,baselinePeak+workspace})
        {
            using var actual=ImageTransforms.Resize(source,width,height,ResizeFilter.Bicubic,
                new ImageLimits {MaxWorkingBytes=peak});
            Assert.Equal(expected,OriginalPixels.Bytes(actual));
            if(depth==8)actual.GetRow8(0)[0]^=255;
            else actual.GetRow16(0)[0]^=65535;
            Assert.Equal(expected,OriginalPixels.Bytes(direct));
            Assert.Equal(input,OriginalPixels.Bytes(source));
        }
        if(depth==8)source.GetRow8(sourceHeight-1)[(sourceWidth-1)*4+3]^=255;
        else source.GetRow16(sourceHeight-1)[(sourceWidth-1)*4+3]^=65535;
        using var changed=ImageTransforms.Resize(source,width,height,ResizeFilter.Bicubic);
        using var changedDirect=ImageTransforms.Resize(source,width,height,ResizeFilter.Bicubic,
            new ImageLimits {MaxWorkingBytes=baselinePeak});
        Assert.Equal(OriginalPixels.Bytes(changedDirect),OriginalPixels.Bytes(changed));
        Assert.NotEqual(expected,OriginalPixels.Bytes(changed));
        Assert.Equal(expected,OriginalPixels.Bytes(direct));
    }

    [Theory]
    [InlineData(8,false,false)]
    [InlineData(16,false,false)]
    [InlineData(8,true,false)]
    [InlineData(16,true,false)]
    [InlineData(8,false,true)]
    [InlineData(8,true,true)]
    [InlineData(16,false,true)]
    [InlineData(16,true,true)]
    public void NegativeLobesAndTinyAlphaRetainOriginalQuantization(int depth,bool transparent,bool mixed)
    {
        using var source=ImageBuffer.Create(17,11,depth);
        for(var y=0;y<source.Height;y++)
        {
            if(depth==8)
            {
                var row=source.GetRow8(y);
                for(var x=0;x<source.Width;x++)
                {
                    row[x*4]=(byte)(x*53+y*31);row[x*4+1]=(byte)(255-x*19-y*47);row[x*4+2]=(byte)(x*97+y*11);
                    row[x*4+3]=transparent?(byte)0:(x+y)%7==0?(byte)255:(x+y)%3==0?(byte)1:(byte)0;
                }
            }
            else
            {
                var row=source.GetRow16(y);
                for(var x=0;x<source.Width;x++)
                {
                    row[x*4]=(ushort)(x*5531+y*3101);row[x*4+1]=(ushort)(65535-x*1913-y*4703);row[x*4+2]=(ushort)(x*9701+y*1103);
                    row[x*4+3]=transparent?(ushort)0:(x+y)%7==0?(ushort)65535:(x+y)%3==0?(ushort)1:(ushort)0;
                }
            }
        }
        var width=mixed?35:7;
        using var expected=ImageTransforms.Resize(source,width,5,ResizeFilter.Bicubic,
            new ImageLimits {MaxWorkingBytes=(long)source.Stride*source.Height+width*5*4*(depth/8)+4096});
        using var actual=ImageTransforms.Resize(source,width,5,ResizeFilter.Bicubic);
        Assert.Equal(OriginalPixels.Bytes(expected),OriginalPixels.Bytes(actual));
    }
}
