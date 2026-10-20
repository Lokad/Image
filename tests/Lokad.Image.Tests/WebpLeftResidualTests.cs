using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class WebpLeftResidualTests
{
    [Theory] [InlineData(8)] [InlineData(16)]
    public void NormativeFirstPixelLeftAndTopResidualsMatchHandComputedChannels(int depth)
    {
        byte[][] pixels = [[10,20,30,255],[21,19,55,0],[200,4,2,128],[9,17,28,254],[33,80,128,64],[0,0,0,0]];
        uint[] expected = [0x00f6140a,0x010cff1a,0x80c2f1da,0xff02fd01,0x42d93f25,0xc02fb0d0];
        using var image = ImageBuffer.Create(3,2,depth); Fill(image,pixels);
        using var context = new OperationContext(ImageLimits.Default,default);
        for (var i=0;i<pixels.Length;i++) Assert.Equal(expected[i],WebpLosslessEncoder.LeftResidual(image,i,context));
        // Reading neighbors never replaces normalized source samples.
        for (var i=0;i<pixels.Length;i++)
        for (var c=0;c<4;c++)
            Assert.Equal(pixels[i][c],depth==8 ? image.GetRow8(i/3)[i%3*4+c] : (image.GetRow16(i/3)[i%3*4+c]+128)/257);
    }

    [Theory] [InlineData(1,3)] [InlineData(3,1)]
    public void AColumnUsesTopAndARowUsesLeft(int width,int height)
    {
        byte[][] pixels = [[10,20,30,255],[21,19,55,0],[200,4,2,128]];
        using var image=ImageBuffer.Create(width,height,8);Fill(image,pixels);
        using var context=new OperationContext(ImageLimits.Default,default);
        Assert.Equal(0x00f6140au,WebpLosslessEncoder.LeftResidual(image,0,context));
        Assert.Equal(0x010cff1au,WebpLosslessEncoder.LeftResidual(image,1,context));
        Assert.Equal(0x80c2f1dau,WebpLosslessEncoder.LeftResidual(image,2,context));
    }

    private static void Fill(ImageBuffer image,byte[][] pixels)
    {
        for (var i=0;i<pixels.Length;i++)
        for (var c=0;c<4;c++)
        {
            if(image.BitDepth==8)image.GetRow8(i/image.Width)[i%image.Width*4+c]=pixels[i][c];
            else image.GetRow16(i/image.Width)[i%image.Width*4+c]=(ushort)Math.Clamp(pixels[i][c]*257+(i%3-1)*64,0,65535);
        }
    }
}
