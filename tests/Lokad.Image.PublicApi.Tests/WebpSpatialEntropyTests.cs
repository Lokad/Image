using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class WebpSpatialEntropyTests
{
    [Theory] [InlineData(8)] [InlineData(16)]
    public void HeterogeneousResidualsPreserveExactNormalizedPixelsAndIndependentOwnership(int depth)
    {
        using var root = new TestMemoryBudget(32L * 1024 * 1024);
        using var source = ImageBuffer.Create(129,129,depth); uint state = 0x18347a9;
        var expected = new byte[129*129*4];
        for (var y=0;y<129;y++)
        {
            var green=0;
            for (var x=0;x<129;x++)
            {
                state=unchecked(state*1664525+1013904223);var bits=1+(x/32+y/32)%3;
                green=(green+(int)(state>>16&((1u<<bits)-1)))&255;
                byte[] pixel=[(byte)(green+(state>>23&7)),(byte)green,(byte)(green+(state>>27&15)),(byte)((x+y)%13==0?0:255)];
                for (var c=0;c<4;c++)
                {
                    if(depth==8)source.GetRow8(y)[x*4+c]=pixel[c];
                    else source.GetRow16(y)[x*4+c]=(ushort)Math.Clamp(pixel[c]*257+((x+y)%3-1)*64,0,65535);
                    expected[(y*129+x)*4+c]=pixel[c];
                }
            }
        }
        var before=OriginalPixels.Bytes(source);var options=new WebpEncodeOptions { Limits=new() { MemoryBudget=root } };
        using var output=new MemoryStream();ImageCodec.EncodeWebp(source,output,options);
        Assert.Equal(0,root.Bytes);Assert.True(root.Peak>800_000);Assert.False(root.IsDisposed);Assert.Equal(before,OriginalPixels.Bytes(source));
        if(depth==8)
        {
            root.Admitted=bytes=> { if(bytes>800_000) { root.RefuseAtGrowth=root.GrowthAttempts+1;root.Admitted=null; } };
            using var refused=new MemoryStream();
            var error=Assert.Throws<ImageLimitException>(()=>ImageCodec.EncodeWebp(source,refused,options));
            Assert.Equal(ImageLimitScope.SharedBudget,error.Scope);Assert.Equal(1,root.Refusals);Assert.Equal(0,root.Bytes);Assert.Equal(0,refused.Length);
            root.RefuseAtGrowth=0;root.Admitted=null;
            using var cancellation=new CancellationTokenSource();
            root.Admitted=bytes=> { if(bytes>800_000)cancellation.Cancel(); };
            using var cancelled=new MemoryStream();
            Assert.Throws<OperationCanceledException>(()=>ImageCodec.EncodeWebp(source,cancelled,options,cancellation.Token));
            Assert.Equal(0,root.Bytes);Assert.Equal(0,cancelled.Length);root.Admitted=null;
            using var recovery=new MemoryStream();ImageCodec.EncodeWebp(source,recovery,options);
            Assert.Equal(output.ToArray(),recovery.ToArray());Assert.Equal(0,root.Bytes);
        }
        var packet=output.ToArray();using var decoded=ImageCodec.Decode(packet);source.Dispose();packet.AsSpan().Clear();
        Assert.Equal(expected,OriginalPixels.Bytes(decoded));
        using var independent=ImageCodec.Decode(output.ToArray());decoded.Dispose();
        Assert.Equal(expected,OriginalPixels.Bytes(independent));
    }
}
