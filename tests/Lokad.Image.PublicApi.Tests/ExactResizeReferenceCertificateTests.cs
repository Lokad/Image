using System.Buffers.Binary;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class ExactResizeReferenceCertificateTests
{
    [Theory]
    [InlineData(8,false)]
    [InlineData(16,false)]
    [InlineData(8,true)]
    [InlineData(16,true)]
    public void HalfCertificateUsesPremultipliedColorAndExactAlpha(int depth,bool transparent)
    {
        var bytes=new byte[8*(depth/8)];
        var maximum=depth==8?255:65535;
        for(var x=0;x<2;x++)
        for(var c=0;c<4;c++)
        {
            var value=c==3?(transparent && x==0?0:maximum):53+x;
            if(depth==8)bytes[x*4+c]=(byte)value;
            else BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan((x*4+c)*2,2),(ushort)value);
        }
        Assert.Equal(!transparent,ExactResizeReference.SampleIsHalf(bytes,2,1,depth,1,1,0,0,0,ResizeFilter.Bilinear));
        Assert.Equal(transparent,ExactResizeReference.SampleIsHalf(bytes,2,1,depth,1,1,0,0,3,ResizeFilter.Bilinear));
        Assert.Equal(ExactResizeReference.AlphaIsHalf(bytes,2,1,depth,1,1,0,0,ResizeFilter.Bilinear),
            ExactResizeReference.SampleIsHalf(bytes,2,1,depth,1,1,0,0,3,ResizeFilter.Bilinear));
        bytes.AsSpan().Clear();
        Assert.False(ExactResizeReference.SampleIsHalf(bytes,2,1,depth,1,1,0,0,0,ResizeFilter.Bilinear));
        Assert.False(ExactResizeReference.SampleIsHalf(bytes,2,1,depth,1,1,0,0,3,ResizeFilter.Bilinear));
    }
}
