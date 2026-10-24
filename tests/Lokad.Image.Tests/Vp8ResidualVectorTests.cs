global using System;
using System.Buffers.Binary;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;

namespace Lokad.Image.Tests;
using Lokad.Image.Internal;
public sealed class Vp8ResidualVectorTests
{
    private uint state=0x91e10da5;
    private uint Next(){state^=state<<13;state^=state>>17;state^=state<<5;return state;}
    [Fact]
    public void FrozenCostsDomainRejectionAndZeroAllocationRemainExact()
    {
        if(!Avx2.IsSupported)return;
        var tables=new ushort[10032];for(var i=0;i<tables.Length;i++)tables[i]=(ushort)Next();
        using var hash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);Span<byte> bytes=stackalloc byte[4];
        Span<short> levels=stackalloc short[18];var comparisons=0;
        for(var test=0;test<32768;test++)
        {
            for(var p=0;p<16;p++)levels[p]=(short)((test%8) switch {
                0=>0,1=>(p&1)==0?2047:-2047,2=>p==test%16?(test%3==0?-1:1):0,
                3=>p<test%16?0:(int)(Next()%4095)-2047,4=>(int)(Next()%5)-2,
                5=>(int)(Next()%137)-68,6=>(Next()&7)==0?(int)(Next()%4095)-2047:0,
                _=>(int)(Next()%4095)-2047 });
            levels[16]=short.MinValue;levels[17]=short.MaxValue;
            for(var first=0;first<2;first++)
            {
                if(first==1)levels[0]=0;
                for(var type=0;type<4;type++)for(var ctx=0;ctx<3;ctx++)
                {
                    var before=Vp8RatePlan.ResidualVectors(tables,levels,type,first,ctx,out var nz0);
                    Assert.Equal(levels[first..16].ContainsAnyExcept((short)0),nz0);
                    BinaryPrimitives.WriteInt32LittleEndian(bytes,before);hash.AppendData(bytes);comparisons++;
                }
            }
        }
        var domainSamples=0;
        for(var offset=0;offset<65536;offset+=16)
        {
            for(var p=0;p<16;p++)levels[p]=unchecked((short)(offset+p));
            var absolute=Avx2.Abs(Vector256.Create(levels)).AsUInt16();var admitted=Avx2.CompareEqual(absolute,Avx2.Min(absolute,Vector256.Create((ushort)2047)));
            for(var p=0;p<16;p++)
            {
                var valid=levels[p]>=-2047&&levels[p]<=2047;
                if(valid!=(admitted.GetElement(p)==ushort.MaxValue)||absolute.GetElement(p)!=Math.Abs((int)levels[p]))throw new Exception("Signed domain mismatch");
                domainSamples++;
            }
        }
        levels.Clear();var invalidCases=0;
        for(var p=0;p<16;p++)foreach(var value in new short[]{short.MinValue,-2048,2048,short.MaxValue})
        {
            levels[p]=value;bool b=false;
            try{Vp8RatePlan.ResidualVectors(tables,levels,0,0,0,out _);}catch(ArgumentException){b=true;}
            if(!b)throw new Exception("Invalid sample accepted");levels[p]=0;invalidCases++;
        }
        levels.Fill(1);levels[0]=0;
        for(var i=0;i<1000;i++)Vp8RatePlan.ResidualVectors(tables,levels,3,1,2,out _);
        var allocationStart=GC.GetAllocatedBytesForCurrentThread();var checksum=0;
        for(var i=0;i<10000;i++)checksum^=Vp8RatePlan.ResidualVectors(tables,levels,3,1,2,out _);
        var allocated=GC.GetAllocatedBytesForCurrentThread()-allocationStart;
        if(allocated!=0||checksum!=0)throw new Exception("Unexpected warm allocation or unstable score");
        Assert.Equal(786432,comparisons);Assert.Equal(65536,domainSamples);Assert.Equal(64,invalidCases);Assert.Equal(0,allocated);
        Assert.Equal("2b61fd9aff020d7d4104cfedf4c2e4421fd766e29253c6be5e0ea349e508e26d",Convert.ToHexStringLower(hash.GetHashAndReset()));
    }
}
