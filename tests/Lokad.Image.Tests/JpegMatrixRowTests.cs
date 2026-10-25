using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegMatrixRowTests
{
    private uint state=0x68abc17f;
    private int Next(){state^=state<<13;state^=state>>17;state^=state<<5;return (int)state;}
    // Original E226 scalar/row controls, frozen before production integration.
    [Fact]
    public void VectorAndScalarRowsPreserveClampedPixelsBorrowedInputAndGuards()
    {
        using var hash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);long chroma=0,luma=0;
        for(var pair=0;pair<=1;pair++)for(var columns=1;columns<=18;columns++)
        {
            if(pair!=0&&(columns&1)!=0)continue;
            for(var alignment=0;alignment<4;alignment++)for(var test=0;test<256;test++)
            {
                var expected=new int[columns+8];for(var i=0;i<expected.Length;i++)expected[i]=Next();
                var actual=(int[])expected.Clone();var row=expected.AsSpan(alignment,columns);
                for(var x=0;x<columns;x+=pair!=0?2:1)
                {
                    var p=row[x];var r=p&255;var g=p>>8&255;var b=p>>16&255;
                    if(pair!=0){var q=row[x+1];r+=q&255;g+=q>>8&255;b+=q>>16&255;}
                    JpegReferenceColor.LimitedChroma(r,g,b,pair!=0,out var u,out var v);row[x]=u|v<<16;
                }
                JpegReferenceColor.MatrixChromaRow(actual.AsSpan(alignment,columns),columns,pair!=0);
                if(!actual.AsSpan().SequenceEqual(expected))throw new Exception("Chroma/guard/odd-lane mismatch");
                hash.AppendData(MemoryMarshal.AsBytes(row));chroma++;
            }
        }
        for(var width=1;width<=65;width++)for(var left=0;left<width;left+=16)for(var halo=0;halo<=1;halo++)
        for(var alignment=0;alignment<4;alignment++)for(var test=0;test<64;test++)
        {
            var firstX=Math.Max(0,left-halo);var lastX=Math.Min(width-1,left+16);var length=lastX-firstX+1;
            var source=new int[length+8];for(var i=0;i<source.Length;i++)source[i]=Next();var unchanged=(int[])source.Clone();
            var actual=new byte[32];actual.AsSpan().Fill(173);var expected=(byte[])actual.Clone();
            for(var x=0;x<16;x++)
            {
                var p=source[alignment+Math.Min(left+x,width-1)-firstX];
                expected[(x<8?3+x:17+x-8)]=JpegReferenceColor.Luma(p&255,p>>8&255,p>>16&255);
            }
            JpegReferenceColor.MatrixLumaRow(source.AsSpan(alignment,length),firstX,left,width,actual.AsSpan(3,8),actual.AsSpan(17,8));
            if(!actual.AsSpan().SequenceEqual(expected)||!source.AsSpan().SequenceEqual(unchanged))throw new Exception("Luma/guard/borrowed source mismatch");
            hash.AppendData(actual);luma++;
        }
        int[] scratch=new int[18];byte[] output=new byte[16];var checksum=0;
        for(var i=0;i<4096;i++){JpegReferenceColor.MatrixLumaRow(scratch,0,0,16,output.AsSpan(0,8),output.AsSpan(8,8));JpegReferenceColor.MatrixChromaRow(scratch,18,false);}
        var before=GC.GetAllocatedBytesForCurrentThread();
        for(var i=0;i<4096;i++){JpegReferenceColor.MatrixLumaRow(scratch,0,0,16,output.AsSpan(0,8),output.AsSpan(8,8));JpegReferenceColor.MatrixChromaRow(scratch,18,false);checksum^=scratch[i%18];}
        var allocated=GC.GetAllocatedBytesForCurrentThread()-before;if(allocated!=0)throw new Exception("Row allocated");GC.KeepAlive(checksum);
        Assert.Equal(27648, chroma); Assert.Equal(84480, luma);
        Assert.Equal("f5014fbc852821a7b227e564fe25e1b71f038bd6a0bb6f56e6719d05beb57087", Convert.ToHexStringLower(hash.GetHashAndReset()));
    }
}
