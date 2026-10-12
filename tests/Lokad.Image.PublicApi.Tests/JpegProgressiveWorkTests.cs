using System.Security.Cryptography;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class JpegProgressiveWorkTests
{
    [Theory]
    [InlineData(0,false)] [InlineData(0,true)] [InlineData(1,false)] [InlineData(1,true)]
    [InlineData(2,false)] [InlineData(2,true)] [InlineData(3,false)] [InlineData(3,true)]
    [InlineData(4,false)] [InlineData(4,true)]
    public void ExactPreOptimizationWorkBoundaryAndPixelsRemain(int shape, bool cancellable)
    {
        // Independent authored scan layouts; minima were frozen before the
        // entropy trials, through the complete public Decode operation.
        var (encoded,work,hash) = shape switch
        {
            0 => (AuthoredJpeg.RefinedBlock(),7786L,"04a8b219d26d42bd443bdc010ad3dacca80a66c27b9adc9f81a514d3cbfd0f90"),
            1 => (AuthoredJpeg.RefinedBlock(true),7786L,"ff4b81719373d98d0709db8920e613950e439722e1a708c17d5654c61f831092"),
            2 => (AuthoredJpeg.EobRun(),20322L,"ec2a88f225e708e45e36cf3089dcbe4c17bf17910846bdbfcf9a23c68378830f"),
            3 => (AuthoredJpeg.EobRun(1),20363L,"ec2a88f225e708e45e36cf3089dcbe4c17bf17910846bdbfcf9a23c68378830f"),
            _ => (AuthoredJpeg.SparseBands(),7859L,"b52ee8d8545058b89a7b2feb4cee1d8cc1d1a63eb210ccadb8e875dd16a2baf2")
        };
        var original = encoded.ToArray(); using var cancellation = new CancellationTokenSource();
        var token = cancellable ? cancellation.Token : default;
        Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(encoded,new DecodeOptions { Limits=ImageLimits.Default with { MaxWork=work-1 } },token));
        using var image = ImageCodec.Decode(encoded,new DecodeOptions { Limits=ImageLimits.Default with { MaxWork=work } },token);
        var raw = new byte[image.Width*image.Height*4];
        for (var y=0;y<image.Height;y++) image.GetRow8(y).CopyTo(raw.AsSpan(y*image.Width*4));
        Assert.Equal(hash,Convert.ToHexStringLower(SHA256.HashData(raw))); Assert.Equal(original,encoded);
        if (cancellable)
        {
            cancellation.Cancel();
            Assert.Throws<OperationCanceledException>(() => ImageCodec.Decode(encoded,new DecodeOptions { Limits=ImageLimits.Default with { MaxWork=work } },token));
        }
    }
}
