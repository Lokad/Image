using System.Buffers.Binary;
using System.Security.Cryptography;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class WebpEncoderPredictionTests
{
    [Theory] [InlineData(8,false)] [InlineData(16,false)] [InlineData(8,true)] [InlineData(16,true)]
    public void SelectedPredictionPreservesNormalizedPixelsSourceAlphaAndSurvivingOwners(int depth,bool alpha)
    {
        using var source=Gradient(129,129,depth,alpha);var expected=Normalized(source);
        var packet=Encode(source);Assert.Equal(13,packet[25]&63); // SG then predictor.
        Assert.Equal(alpha,(BinaryPrimitives.ReadUInt32LittleEndian(packet.AsSpan(21))&(1u<<28))!=0);
        Assert.Equal(alpha,ImageCodec.Identify(packet).HasAlpha);
        using var decoded=ImageCodec.Decode(packet);var independent=Encode(source);
        Assert.Equal(packet,independent);source.Dispose();packet.AsSpan().Clear();
        Assert.Equal(expected,OriginalPixels.Bytes(decoded));
        using var second=ImageCodec.Decode(independent);decoded.Dispose();
        Assert.Equal(expected,OriginalPixels.Bytes(second));
    }

    [Theory] [InlineData(16384,1)] [InlineData(1,16384)]
    public void SingleAxisLargeImagesKeepTheMandatoryPredictorBorders(int width,int height)
    {
        using var source=ImageBuffer.Create(width,height,8);
        for(var i=0;i<width*height;i++)
        {
            var row=source.GetRow8(i/width);var at=i%width*4;
            row[at]=(byte)i;row[at+1]=(byte)(i>>8);row[at+2]=(byte)(i*13+(i>>2));row[at+3]=255;
        }
        var expected=OriginalPixels.Bytes(source);var packet=Encode(source);
        Assert.Equal(13,packet[25]&63);Assert.False(ImageCodec.Identify(packet).HasAlpha);
        using var decoded=ImageCodec.Decode(packet);Assert.Equal(expected,OriginalPixels.Bytes(decoded));
    }

    [Fact]
    public void NonbeneficialNoiseRetainsTheExactPredecessorPacket()
    {
        using var source=ImageBuffer.Create(129,129,8);uint state=0x9e3779b9u;
        for(var y=0;y<source.Height;y++)
        for(var x=0;x<source.Width;x++)
        {
            state^=state<<13;state^=state>>17;state^=state<<5;
            var row=source.GetRow8(y);row[x*4]=(byte)state;row[x*4+1]=(byte)(state>>8);
            row[x*4+2]=(byte)(state>>16);row[x*4+3]=255;
        }
        var packet=Encode(source);
        // Authored xorshift32 input, recorded with the frozen accepted C34 core.
        Assert.Equal(50034,packet.Length);Assert.Equal(21,packet[25]&63);
        Assert.Equal("4f0df81a3dfe76060581919029b67ccbbfe822075028fe26f329321591ff81ba",Convert.ToHexStringLower(SHA256.HashData(packet)));
        using var decoded=ImageCodec.Decode(packet);Assert.Equal(OriginalPixels.Bytes(source),OriginalPixels.Bytes(decoded));
    }

    [Fact]
    public void OverlappingPlansRefuseTerminallyAndSucceedAfterCallerReleasesCapacity()
    {
        const int sourceBytes=129*129*4;
        using var root=new TestMemoryBudget(sourceBytes+65536);var limits=new ImageLimits { MemoryBudget=root };
        using var source=Gradient(129,129,8,false,limits);
        using var caller=root.CreateScope();Assert.True(caller.TryReserve(65536-55000,out var held));
        using var rejected=new MemoryStream();
        var error=Assert.Throws<ImageLimitException>(()=>ImageCodec.EncodeWebp(source,rejected,new() { Limits=limits }));
        Assert.Equal((ImageLimitKind.WorkingBytes,ImageLimitScope.SharedBudget),(error.Limit,error.Scope));
        Assert.Equal(1,root.Refusals);Assert.Equal(sourceBytes+held.Bytes,root.Bytes);Assert.Equal(0,rejected.Length);
        held.Dispose();using var output=new MemoryStream();ImageCodec.EncodeWebp(source,output,new() { Limits=limits });
        Assert.Equal(sourceBytes,root.Bytes);Assert.InRange(root.Peak,sourceBytes+60000,sourceBytes+65536);
        Assert.Equal(13,output.GetBuffer()[25]&63);Assert.False(root.IsDisposed);
    }

    [Fact]
    public void AnalysisCancellationAndOutputRefusalCleanUpBeforeCallerWrites()
    {
        using var root=new TestMemoryBudget(1_000_000);var limits=new ImageLimits { MemoryBudget=root };
        using var source=Gradient(129,129,8,false,limits);var baseline=root.Bytes;
        using(var cancellation=new CancellationTokenSource())
        {
            var largeAdmissions=0;
            root.Admitted=bytes=> { if(bytes>baseline+50000 && ++largeAdmissions==2)cancellation.Cancel(); };
            using var rejected=new MemoryStream();
            Assert.Throws<OperationCanceledException>(()=>ImageCodec.EncodeWebp(source,rejected,new() { Limits=limits },cancellation.Token));
            Assert.Equal(baseline,root.Bytes);Assert.Equal(0,rejected.Length);root.Admitted=null;
        }
        var packet=Encode(source);
        using var exact=new MemoryStream();ImageCodec.EncodeWebp(source,exact,new() { Limits=limits with { MaxOutputBytes=packet.Length } });
        Assert.Equal(packet,exact.ToArray());Assert.Equal(baseline,root.Bytes);
        using var shortOutput=new MemoryStream();
        Assert.Equal(ImageLimitKind.OutputBytes,Assert.Throws<ImageLimitException>(()=>ImageCodec.EncodeWebp(source,shortOutput,new() { Limits=limits with { MaxOutputBytes=packet.Length-1 } })).Limit);
        Assert.Equal(0,shortOutput.Length);Assert.Equal(baseline,root.Bytes);Assert.True(shortOutput.CanWrite);
    }

    private static ImageBuffer Gradient(int width,int height,int depth,bool alpha,ImageLimits? limits=null)
    {
        var source=ImageBuffer.Create(width,height,depth,limits);
        for(var y=0;y<height;y++)
        for(var x=0;x<width;x++)
        for(var c=0;c<4;c++)
        {
            var value=c switch { 0=>(x*3+y*7)&255,1=>(x*5+y*11)&255,2=>(x*13+y*17)&255,_=>alpha?(x*23+y*19+42)&255:255 };
            if(depth==8)source.GetRow8(y)[x*4+c]=(byte)value;
            else source.GetRow16(y)[x*4+c]=(ushort)Math.Clamp(value*257+((x+y)%3-1)*64,0,65535);
        }
        return source;
    }

    private static byte[] Normalized(ImageBuffer image)
    {
        var result=new byte[image.Width*image.Height*4];
        for(var y=0;y<image.Height;y++)
        for(var x=0;x<image.Width*4;x++)result[y*image.Width*4+x]=image.BitDepth==8?image.GetRow8(y)[x]:(byte)((image.GetRow16(y)[x]+128)/257);
        return result;
    }

    private static byte[] Encode(ImageBuffer image)
    { using var output=new MemoryStream();ImageCodec.EncodeWebp(image,output);return output.ToArray(); }
}
