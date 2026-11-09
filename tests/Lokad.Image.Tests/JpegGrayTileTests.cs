using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class JpegGrayTileTests
{
    [Theory]
    [InlineData(9,1)] [InlineData(63,7)] [InlineData(64,8)]
    [InlineData(65,9)] [InlineData(133,19)]
    public void TilesKeepQualifiedBlockSamplesAndBorrowedGuards(int width,int height)
    {
        foreach(var depth in new[]{8,16})foreach(var opaque in new[]{false,true})
        {
            using var image=OriginalPixels.Buffer(width,height,depth,opaque:opaque);
            var before=OriginalPixels.Bytes(image);
            var tile=Enumerable.Repeat((byte)0xD7,514).ToArray();
            var packed=Enumerable.Repeat(0x5A5A5A5A,66).ToArray();var block=new byte[64];
            using var context=new OperationContext(ImageLimits.Default,default);
            for(var top=0;top<height;top+=8)for(var left=0;left<width;left+=64)
            {
                var scratchBefore=packed.ToArray();
                var count=JpegGrayPreparation.PrepareTile(image,left,top,opaque?null:0x204060u,
                    tile.AsSpan(1,512),packed.AsSpan(1,64),context);
                Assert.Equal(Math.Min(64,width-left),count);
                for(var column=0;column<count;column+=8)
                {
                    JpegGrayPreparation.CopyBlock(image,default,width,height,left+column,top,opaque?null:0x204060u,block);
                    for(var y=0;y<8;y++)Assert.Equal(block.AsSpan(y*8,8).ToArray(),tile.AsSpan(1+y*64+column,8).ToArray());
                }
                Assert.Equal(0xD7,tile[0]);Assert.Equal(0xD7,tile[^1]);
                Assert.Equal(0x5A5A5A5A,packed[0]);Assert.Equal(0x5A5A5A5A,packed[^1]);
                Assert.Equal(scratchBefore.AsSpan(1+count,64-count).ToArray(),packed.AsSpan(1+count,64-count).ToArray());
            }
            Assert.Equal(before,OriginalPixels.Bytes(image));
        }
    }

    [Fact]
    public void TilePreparationUsesExistingAdmissionAndNoManagedAllocation()
    {
        using var image8=OriginalPixels.Buffer(65,9,8);using var image16=OriginalPixels.Buffer(65,9,16);
        var tile=new byte[512];var packed=new int[64];
        using var root=new TestMemoryBudget(8192);
        using var context=new OperationContext(new(){MemoryBudget=root},default);context.Reserve(8192);
        var attempts=root.Attempts;
        for(var i=0;i<256;i++)
        {
            JpegGrayPreparation.PrepareTile(image8,0,0,0x204060,tile,packed,context);
            JpegGrayPreparation.PrepareTile(image16,64,8,0x204060,tile,packed,context);
        }
        var before=GC.GetAllocatedBytesForCurrentThread();
        for(var i=0;i<1024;i++)
        {
            JpegGrayPreparation.PrepareTile(image8,0,0,0x204060,tile,packed,context);
            JpegGrayPreparation.PrepareTile(image16,64,8,0x204060,tile,packed,context);
        }
        Assert.Equal(0,GC.GetAllocatedBytesForCurrentThread()-before);
        Assert.Equal(8192,root.Bytes);Assert.Equal(attempts,root.Attempts);
        context.Release(8192);Assert.Equal(0,root.Bytes);Assert.False(root.IsDisposed);
    }

    [Fact]
    public void CancellationBeforePreparationPreservesBorrowedInputs()
    {
        using var image=OriginalPixels.Buffer(65,9,8);var before=OriginalPixels.Bytes(image);
        var tile=Enumerable.Repeat((byte)77,512).ToArray();var packed=Enumerable.Repeat(12345,64).ToArray();
        using var root=new TestMemoryBudget(8192);using var cancellation=new CancellationTokenSource();
        using var context=new OperationContext(new(){MemoryBudget=root},cancellation.Token);
        context.Reserve(8192);cancellation.Cancel();
        var error=Assert.Throws<OperationCanceledException>(()=>JpegGrayPreparation.PrepareTile(image,0,0,0x204060,tile,packed,context));
        Assert.Equal(cancellation.Token,error.CancellationToken);Assert.Equal(before,OriginalPixels.Bytes(image));
        Assert.All(tile,v=>Assert.Equal((byte)77,v));Assert.All(packed,v=>Assert.Equal(12345,v));
        Assert.Equal(8192,root.Bytes);context.Release(8192);Assert.Equal(0,root.Bytes);Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void RecordedFailureKeepsOutputPrivateAndReleasesAdmission(int depth)
    {
        using var root=new TestMemoryBudget(4*1024*1024);var limits=new ImageLimits{MemoryBudget=root};
        using var image=ImageBuffer.Create(133,19,depth,limits);
        for(var y=0;y<image.Height;y++)for(var x=0;x<image.Width;x++)
        {
            if(depth==8){var row=image.GetRow8(y);row[x*4]=(byte)(x*37);row[x*4+1]=(byte)(y*59);row[x*4+2]=177;row[x*4+3]=255;}
            else{var row=image.GetRow16(y);row[x*4]=(ushort)(x*397);row[x*4+1]=(ushort)(y*359);row[x*4+2]=31337;row[x*4+3]=65535;}
        }
        if(depth==8)image.GetRow8(0)[72*4+3]=0;else image.GetRow16(0)[72*4+3]=0;
        var before=OriginalPixels.Bytes(image);var held=root.Bytes;
        using var output=new MemoryStream();output.Write(new byte[]{1,2,3,4,5});
        var options=new JpegEncodeOptions{Sampling=JpegSampling.Grayscale,Quality=69,Limits=limits};
        var error=Assert.Throws<ArgumentException>(()=>ImageCodec.EncodeJpeg(image,output,options));
        Assert.Equal("image",error.ParamName);Assert.Equal(new byte[]{1,2,3,4,5},output.ToArray());
        Assert.Equal(before,OriginalPixels.Bytes(image));Assert.Equal(held,root.Bytes);Assert.True(output.CanWrite);Assert.False(root.IsDisposed);
        if(depth==8)image.GetRow8(0)[72*4+3]=255;else image.GetRow16(0)[72*4+3]=65535;
        using var valid=new MemoryStream();ImageCodec.EncodeJpeg(image,valid,options);
        Assert.True(valid.Length>0);Assert.Equal(held,root.Bytes);
        image.Dispose();Assert.Equal(0,root.Bytes);Assert.True(valid.CanWrite);Assert.False(root.IsDisposed);
    }
}
