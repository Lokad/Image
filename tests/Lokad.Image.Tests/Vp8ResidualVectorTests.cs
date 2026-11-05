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
    // Independent scalar pricing with the original variable/fixed semantics.
    // Literal format bands keep this oracle separate from SIMD address setup.
    private static int ReferenceCost(ReadOnlySpan<ushort> tables,ReadOnlySpan<short> levels,int type,int first,int context)
    {
        ReadOnlySpan<byte> bands=[0,1,2,3,6,4,5,6,6,6,6,6,6,6,6,7];
        var last=-1;for(var p=first;p<16;p++)if(levels[p]!=0)last=p;
        var index=(type*8+bands[first])*3+context;
        if(last<0)return tables[3312+index*2];
        var cost=context==0?tables[3312+index*2+1]:0;
        for(var p=first;p<=last;p++)
        {
            var level=Math.Abs((int)levels[p]);
            cost+=tables[256+level]+tables[3504+index*68+Math.Min(level,67)];
            if(p<15)index=(type*8+bands[p+1])*3+Math.Min(level,2);
        }
        if(last<15)cost+=tables[3312+index*2];
        return cost;
    }
    [Fact]
    public void FrozenCostsDomainRejectionAndZeroAllocationRemainExact()
    {
        if(!Avx2.IsSupported)return;
        // Synthetic prices must fit after fixed+variable preparation. Fourteen
        // bits per component cover well beyond actual generated variable costs.
        var original=new ushort[10032];for(var i=0;i<original.Length;i++)original[i]=(ushort)(Next()&16383);
        var tables=original.ToArray();
        for(var index=0;index<96;index++)for(var level=0;level<68;level++)
            tables[3504+index*68+level]=checked((ushort)(original[3504+index*68+level]+original[256+level]));
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
                    Assert.Equal(ReferenceCost(original,levels,type,first,ctx),before);
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
        // Frozen independently with the scalar Python oracle on these inputs.
        Assert.Equal("d26addb814c0185fd191e2626866501aaeb87d2d089926a2cd89287763363c50",Convert.ToHexStringLower(hash.GetHashAndReset()));
    }
}
