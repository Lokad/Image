// Source-only complete-packet model for four frozen non-palette controls.
// Admitted production integration and other required paths remain unqualified.
#nullable enable
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Security.Cryptography;
public sealed record Vp8lPacketResult(byte[] Packet,byte[] GroupMap,object Diagnostics,object Framing,object Clustering);

public static class Vp8lPacketParity
{
    private static object Invoke(string name,params object[] args)=>typeof(Vp8lMainParity).GetMethod(name,BindingFlags.Static|BindingFlags.NonPublic)!.Invoke(null,args)!;
    private static MethodInfo Auxiliary(string type,string name)=>typeof(Vp8lAuxiliaryParity).Assembly.GetType(type)!.GetMethod(name,BindingFlags.Static|BindingFlags.NonPublic)!;
    private static readonly Func<int[],long> Population=Auxiliary("Vp8lLzParity","Population").CreateDelegate<Func<int[],long>>();
    private static readonly Func<int,int,int> Plane=Auxiliary("Vp8lLzParity","Plane").CreateDelegate<Func<int,int,int>>();
    private static readonly Func<int,(int Code,int Extra)> Prefix=Auxiliary("Vp8lLzParity","Prefix").CreateDelegate<Func<int,(int,int)>>();
    private static readonly Func<uint[],int,int,uint[]> Index=Auxiliary("Vp8lMatchParity","Build").CreateDelegate<Func<uint[],int,int,uint[]>>();
    public static Vp8lPacketResult Build(byte[] source,bool disableCache=false)
    {
        if(!BitConverter.IsLittleEndian)throw new Exception("Little endian stage required");
        var sourceHash=Convert.ToHexStringLower(SHA256.HashData(source));
        if(source.Length!=531468)throw new ArgumentException("Expected the frozen513x259 layout.");
        var choice=Vp8lInitialChoice.Select(source,513,259);
        var stage=Vp8lInputParity.Build(source,choice);var pixels=new uint[stage.CodedResidual.Length/4];Buffer.BlockCopy(stage.CodedResidual,0,pixels,0,stage.CodedResidual.Length);
        var matches=Index(pixels,513,75);var parsed=Vp8lMainParsing.Encode(pixels,matches,513,disableCache);var final=parsed.Tokens;var bits=parsed.Bits;
        var histograms=Enumerable.Range(0,2145).Select(_=>new[]{new int[280+(bits==0?0:1<<bits)],new int[256],new int[256],new int[256],new int[40]}).ToArray();
        var frameTokens=new List<Vp8lFrameToken>();
        using var tokenHash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);Span<byte> record=stackalloc byte[16];var count=0;
        foreach(var token in (IEnumerable)final)
        {
            var type=token.GetType();T Get<T>(string name)=>(T)type.GetProperty(name)!.GetValue(token)!;
            var position=Get<int>("Position");var pixel=Get<uint>("Pixel");var length=Get<int>("Length");var distance=Get<int>("Distance");var cached=type.GetProperty("CacheIndex") is { } cp?(int)cp.GetValue(token)!:-1;
            var tag=distance!=0?1:cached>=0?2:0;var value=tag==1?(uint)Plane(distance,513):tag==2?(uint)cached:pixel;
            System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(record,position);System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(record[4..],tag);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(record[8..],value);System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(record[12..],length);tokenHash.AppendData(record);
            var tile=(position/513>>3)*65+(position%513>>3);var h=histograms[tile];
            if(tag==1){h[0][256+Prefix(length).Code]++;h[4][Prefix((int)value).Code]++;}
            else if(tag==2)h[0][280+cached]++;
            else{h[0][pixel>>8&255]++;h[1][pixel>>16&255]++;h[2][pixel&255]++;h[3][pixel>>24]++;}
            frameTokens.Add(new(position,pixel,length,distance,cached));count++;
        }
        var semanticHash=Convert.ToHexStringLower(tokenHash.GetHashAndReset());
        using var hash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);var scratch=new byte[5216];
        foreach(var tile in histograms)foreach(var bank in tile){Buffer.BlockCopy(bank,0,scratch,0,bank.Length*4);hash.AppendData(scratch.AsSpan(0,bank.Length*4));}
        var clustered=Vp8lSpatialParity.Build(histograms,Population);
        var mapBytes=new byte[clustered.Map.Length*4];for(var i=0;i<clustered.Map.Length;i++)System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(mapBytes.AsSpan(i*4),(uint)clustered.Map[i]<<8);

        var frame=Vp8lFrameParity.Encode(stage,clustered.Map,clustered.Counts,frameTokens,bits);
        if(Convert.ToHexStringLower(SHA256.HashData(source))!=sourceHash)throw new Exception("Source changed");
        return new(frame.Packet,mapBytes,new{sourceOnly=true,sourceSha256=sourceHash,semanticTokens=count,semanticTokenSha256=semanticHash,countsSha256=Convert.ToHexStringLower(hash.GetHashAndReset()),choice,parser=parsed.Summary,cacheBits=bits,prefix=stage.Diagnostics,groups=clustered.Counts.Length,mapSha256=Convert.ToHexStringLower(SHA256.HashData(mapBytes)),oracleEncoderInputs=false,productionChanged=false},frame.Details,clustered.Details);
    }
}
